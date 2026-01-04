using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Reble.ArduinoRouter.ApiExample.Endpoints;
using Reble.ArduinoRouter.ApiExample.Services;
using Reble.ArduinoRouter;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.Configure<JsonOptions>(o =>
{
	o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
	o.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

builder.Services.AddScoped<IArduinoRouterClient, ArduinoRouterClient>();
builder.Services.AddHostedService<MultiplierBackgroundService>();
var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "v1"); });

app.MapGet("/", () => Results.Redirect("/swagger"))
	.ExcludeFromDescription();


app.MapLocalCallerTest();

app.MapGet("/set-led",
		async (IArduinoRouterClient router, CancellationToken ct, int numLed = 0, bool ledState = true) =>
		{
			var responseContext = await router.CallAsync<bool>("set_led", [numLed, ledState], ct);

			if (responseContext.Error is { } err)
				return Results.InternalServerError(err);

			return responseContext.DeserializePayload()
				? Results.Ok()
				: Results.BadRequest("numLed is only valid between 0 and 1");
		})
	.WithName("SetLed");

app.MapGet("/add", async (IArduinoRouterClient router, int num1, int num2, CancellationToken ct) =>
{
	var responseContext = await router.CallAsync<int>("add", [num1, num2], ct);
	try
	{
		responseContext.EnsureSuccess();
	}
	catch (ArduinoRouterException e)
	{
		return Results.InternalServerError(e.Message);
	}

	return Results.Ok(responseContext.DeserializePayload());
});

//Bare minimum
app.MapGet("/greet", async (IArduinoRouterClient router) =>
{
	var responseContext = await router.CallAsync<string>("greet");
	responseContext.EnsureSuccess();
	return Results.Ok(responseContext.DeserializePayload());
});


app.Run();