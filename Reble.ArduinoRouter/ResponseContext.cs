using MessagePack;
using Reble.ArduinoRouter.Internal;

namespace Reble.ArduinoRouter;

public sealed class ResponseContext<TResult>
{
	public uint MessageId => _response.MessageId;
	public ArduinoRouterError? Error => _response.Error;

	internal ResponseContext(BridgeResponseMessage response)
	{
		_response = response;
	}

	private readonly BridgeResponseMessage _response;

	/// <summary>
	/// Throws an ArduinoRouterResponseException if Error is not null
	/// </summary>
	/// <exception cref="ArduinoRouterResponseException">Will be thrown when Error is not null</exception>
	public void EnsureSuccess()
	{
		if (Error is { } err)
			throw new ArduinoRouterResponseException(err);
	}

	public TResult DeserializePayload()
		=> MessagePackSerializer.Deserialize<TResult>(_response.SecondPayload);
}