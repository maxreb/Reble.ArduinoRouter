using MessagePack;

namespace Reble.ArduinoRouter.Internal.MessagePack;

[MessagePackObject(AllowPrivate = true)]
internal partial class ArduinoRouterResponseMessage<TResponse>
{
	[Key(0)] internal required int Type { get; init; }
	[Key(1)] internal required uint MsgId { get; init; }
	[Key(2)] internal required object[]? Error { get; init; }
	[Key(3)] internal required TResponse? Parameter { get; init; }

	internal ArduinoRouterError? GetErrorMessage()
	{
		if (Error is null)
			return null;
		return new ArduinoRouterError((ArduinoRouterErrorType)Convert.ToInt32((byte)Error[0]), (string)Error[1]);
	}

	internal static ArduinoRouterResponseMessage<TResponse> Create(uint msgId, TResponse? parameter = default)
		=> new()
		{
			Type = (int)ArduinoRouterMessageType.Response,
			MsgId = msgId,
			Error = null,
			Parameter = parameter
		};

	internal static ArduinoRouterResponseMessage<TResponse> CreateError(uint msgId, string errorMessage)
		=> new()
		{
			Type = (int)ArduinoRouterMessageType.Response,
			MsgId = msgId,
			Error = [(byte)4, errorMessage],
			Parameter = default
		};
}
