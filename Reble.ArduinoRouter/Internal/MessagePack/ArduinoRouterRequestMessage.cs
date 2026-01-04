using MessagePack;

namespace Reble.ArduinoRouter.Internal.MessagePack;

[MessagePackObject(AllowPrivate = true)]
internal partial class ArduinoRouterRequestMessage
{
	[Key(0)] internal required ArduinoRouterMessageType Type { get; init; }
	[Key(1)] internal required uint MsgId { get; init; }
	[Key(2)] internal required string MethodName { get; init; }
	[Key(3)] internal required object[] Parameters { get; init; }

	internal static ArduinoRouterRequestMessage Create(ref uint messageId, string method, object[] parameters)
	{
		var id = Interlocked.Increment(ref messageId);
		return new ArduinoRouterRequestMessage
		{
			Type = ArduinoRouterMessageType.Request,
			MsgId = id,
			MethodName = method,
			Parameters = parameters
		};
	}

	internal static ArduinoRouterRequestMessage CreateRegister(ref uint messageId, string methodName)
	{
		var id = Interlocked.Increment(ref messageId);
		return new ArduinoRouterRequestMessage
		{
			Type = ArduinoRouterMessageType.Request,
			MsgId = id,
			MethodName = "$/register",
			Parameters = [methodName]
		};
	}
}
