using MessagePack;

namespace Reble.ArduinoRouter.Internal;

internal class BridgeResponseMessage : BridgeMessage
{
	public ArduinoRouterError? Error { get; }

	internal BridgeResponseMessage(ArduinoRouterMessageType type, uint messageId,
		byte[] firstPayload, byte[] secondPayload) :
		base(type, messageId, firstPayload, secondPayload)
	{
		Error = MessagePackSerializer.Deserialize<ArduinoRouterError?>(firstPayload);
	}
}
