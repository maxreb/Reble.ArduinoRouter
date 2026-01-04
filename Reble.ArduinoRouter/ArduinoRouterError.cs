using MessagePack;

namespace Reble.ArduinoRouter;

[MessagePackObject]
public record ArduinoRouterError([property: Key(0)] ArduinoRouterErrorType Type, [property: Key(1)] string Message);
