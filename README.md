# Reble.ArduinoRouter

A .NET client library for [Arduino Router](https://github.com/arduino/arduino-router) which is a MessagePack RPC router for Arduino devices. The first device using this is the [Arduino UNO Q](https://www.arduino.cc/product-uno-q)

## Installation

```bash
dotnet add package Reble.ArduinoRouter
```

## Usage

### Calling Methods

```csharp
using Reble.ArduinoRouter;

var client = new ArduinoRouterClient();

// Call a method and get a typed response
var response = await client.CallAsync<int>("add", [5, 3]);
response.EnsureSuccess();
var result = response.DeserializePayload(); // 8

// Call without expecting a response payload
await client.CallAsync("set_led", [0, true]);
```

### Providing Methods

Register your .NET application as a method provider:

```csharp
using Reble.ArduinoRouter;

var client = new ArduinoRouterClient();

await foreach (var request in client.ProvideAsync<double>("multiply"))
{
    var factors = request.DeserializePayloadArray();
    var product = factors[0] * factors[1];

    //No need for Acknowledgement, the ProvideAsync method will handle it automatically
    // but if you want to add a response value, you can use the AcknowledgeAsync method
    await request.AcknowledgeAsync(product);
}
```

### Error Handling

```csharp
var response = await client.CallAsync<string>("some_method");

// Option 1: Check error explicitly
if (response.Error is { } error)
{
    Console.WriteLine($"Error: {error.Message} ({error.Type})");
}

// Option 2: Throw on error
response.EnsureSuccess(); // Throws ArduinoRouterResponseException if error
```

### ASP.NET Core Integration

```csharp
// Program.cs
builder.Services.AddScoped<IArduinoRouterClient, ArduinoRouterClient>();

// In endpoints
app.MapGet("/greet", async (IArduinoRouterClient client) =>
{
    var response = await client.CallAsync<string>("greet");
    //Throw on error will result in 500
    response.EnsureSuccess();
    return response.DeserializePayload();
});
```

## Example
See [Reble.ArduinoRouter.ApiExample](Reble.ArduinoRouter.ApiExample/README.md) for a running example on the Arduino UNO Q

## Requirements

- .NET 10.0 or later
- Arduino Router running on the target device (communicates via Unix domain socket at `/var/run/arduino-router.sock`)

## License

[MIT](LICENSE.md)
