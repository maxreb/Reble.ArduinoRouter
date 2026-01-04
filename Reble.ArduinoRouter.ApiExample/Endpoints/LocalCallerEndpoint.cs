using System.ComponentModel;
using System.Text.Json;
using MessagePack;

namespace Reble.ArduinoRouter.ApiExample.Endpoints;

public static class LocalCallerEndpoint
{
	private const string FunctionName = "my-local-test";

	extension(WebApplication app)
	{
		public void MapLocalCallerTest()
		{
			app.MapGet("/register-test", RegisterHandler)
				.WithName("RegisterTest");

			app.MapGet("/call-test", CallHandler)
				.WithName("SendTest");
		}

		private static async Task<IResult> CallHandler(IArduinoRouterClient router,
			[Description("Try to use a string that has not 5 characters"), DefaultValue("hello")]
			string myText,
			CancellationToken ct)
		{
			try
			{
				var request = new MyRequestClass { Text = myText, Date = DateTimeOffset.Now };
				var responseContext = await router.CallAsync<MyResponseClass>(FunctionName, [request], ct);
				responseContext.EnsureSuccess();
				var response = responseContext.DeserializePayload();
				return Results.Ok($"The favourite number is {response.MyFavouriteNumber} at {response.Date}");
			}
			catch (Exception e)
			{
				return Results.BadRequest(e.Message);
			}
		}


		private static IResult RegisterHandler(IArduinoRouterClient router, ILogger<Program> logger,
			CancellationToken ct)
		{
			// Microsoft.AspNetCore.Server.Kestrel[13]
			// Connection id "0HNIATR31RLNV", Request id "0HNIATR31RLNV:00000002": An unhandled exception was thrown by the application.
			// 	System.InvalidOperationException: route already exists: my-local-test
			// at Reble.ArduinoBridge.RouterSocket.ArduinoRouter.ReadResponseAndThrowOnError[TResponse](MessagePackStreamReader reader, CancellationToken ct) in M:\repos\reble\Reble.ArduinoBridge\Reble.ArduinoBridge.RouterSocket\ArduinoRouter.cs:line 112
			//
			return Results.ServerSentEvents(GetItems(), eventType: "responses");

			async IAsyncEnumerable<string> GetItems()
			{
				await foreach (var context in router.ProvideAsync<MyRequestClass>(FunctionName, ct))
				{
					var requestMessage = context.DeserializePayloadSingle();
					logger.LogInformation("Method: {MethodName} called with {Properties}", context.MethodName,
						JsonSerializer.Serialize(requestMessage));
					if (requestMessage.Text.Length != 5)
					{
						await context.FailAsync("Error: we only like strings that are 5 chars long");
						yield return "failed: we only like strings that are 5 chars long";
						yield break;
					}

					await context.AcknowledgeAsync(new MyResponseClass
					{
						MyFavouriteNumber = Random.Shared.Next(0, 100),
						Date = DateTimeOffset.Now
					});

					yield return
						$"method: {context.MethodName}(text = {requestMessage.Text},  date = {requestMessage.Date})";
				}
			}
		}
	}
}

[MessagePackObject]
public class MyRequestClass
{
	[Key(0)] public required string Text { get; set; }
	[Key(1)] public required DateTimeOffset Date { get; set; }
}

[MessagePackObject]
public class MyResponseClass
{
	[Key(0)] public required int MyFavouriteNumber { get; set; }
	[Key(1)] public required DateTimeOffset Date { get; set; }
}