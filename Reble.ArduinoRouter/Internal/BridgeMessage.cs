namespace Reble.ArduinoRouter.Internal;

internal class BridgeMessage
{
	public ArduinoRouterMessageType Type { get; }
	public uint MessageId { get; }
	public byte[] FirstPayload { get; }
	public byte[] SecondPayload { get; }

	internal BridgeMessage(ArduinoRouterMessageType type, uint messageId,
		byte[] firstPayload, byte[] secondPayload)
	{
		Type = type;
		MessageId = messageId;
		FirstPayload = firstPayload;
		SecondPayload = secondPayload;
	}
}
