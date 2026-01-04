using MessagePack;

namespace Reble.ArduinoRouter.Internal;

internal class BridgeRequestMessage : BridgeMessage
{
	public string MethodName { get; }

	internal BridgeRequestMessage(ArduinoRouterMessageType type, uint messageId,
		byte[] firstPayload, byte[] secondPayload) :
		base(type, messageId, firstPayload, secondPayload)
	{
		if (type != ArduinoRouterMessageType.Request)
			throw new ArgumentOutOfRangeException(nameof(type), type, "Should be Request");
		MethodName = MessagePackSerializer.Deserialize<string>(firstPayload);
	}
}
