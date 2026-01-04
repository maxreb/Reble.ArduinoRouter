using System.Buffers;
using MessagePack;

namespace Reble.ArduinoRouter.Internal;

internal class BridgeMessageBuilder
{
	private int _key;
	private ArduinoRouterMessageType? _messageType;
	private uint? _messageId;
	private byte[]? _firstPayload;
	private byte[]? _secondPayload;


	internal void Next(ReadOnlySequence<byte> sequence)
	{
		switch (_key++)
		{
			case 0:
				_messageType = MessagePackSerializer.Deserialize<ArduinoRouterMessageType>(sequence);
				break;
			case 1:
				_messageId = MessagePackSerializer.Deserialize<uint>(sequence);
				break;
			case 2:
				_firstPayload = sequence.ToArray();
				break;
			case 3:
				_secondPayload = sequence.ToArray();
				break;
			default:
				throw new InvalidOperationException("More arrays received than expected (4)");
		}
	}

	public BridgeMessage Build()
	{
		if (!_messageType.HasValue)
			throw new ArgumentNullException(nameof(_messageType));
		if (!_messageId.HasValue)
			throw new ArgumentNullException(nameof(_messageId));
		if (_firstPayload is not { Length: > 0 } firstPayload)
			throw new ArgumentNullException(nameof(_firstPayload), "First payload was not set");
		if (_secondPayload is not { Length: > 0 } secondPayload)
			throw new ArgumentNullException(nameof(_secondPayload), "Second payload was not set");

		return _messageType.Value switch
		{
			ArduinoRouterMessageType.Request =>
				new BridgeRequestMessage(_messageType.Value, _messageId.Value, firstPayload, secondPayload),
			ArduinoRouterMessageType.Response =>
				new BridgeResponseMessage(_messageType.Value, _messageId.Value, firstPayload, secondPayload),
			_ => throw new ArgumentOutOfRangeException(nameof(_messageType), _messageType, null)
		};
	}
}
