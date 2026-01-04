using MessagePack;
using Reble.ArduinoRouter.Internal;
using Reble.ArduinoRouter.Internal.MessagePack;

namespace Reble.ArduinoRouter;

public sealed class RequestContext<TRequest> : IAsyncDisposable
{
	private readonly BridgeRequestMessage _request;
	private readonly Stream _stream;
	private readonly CancellationToken _ct;
	private bool AckSent { get; set; }

	public uint MessageId => _request.MessageId;
	public string MethodName => _request.MethodName;

	public TRequest DeserializePayloadSingle() => DeserializePayloadArray().Single();

	public TRequest[] DeserializePayloadArray()
		=> MessagePackSerializer.Deserialize<TRequest[]>(_request.SecondPayload);


	internal RequestContext(BridgeRequestMessage request, Stream stream, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		_request = request;
		_stream = stream;
		_ct = ct;
	}


	public ValueTask DisposeAsync()
	{
		if (AckSent)
			return ValueTask.CompletedTask;

		return new ValueTask(AcknowledgeAsync());
	}


	public async Task AcknowledgeAsync()
	{
		var responseMessage = ArduinoRouterResponseMessage<object>.Create(_request.MessageId);
		await MessagePackSerializer.SerializeAsync(_stream, responseMessage, cancellationToken: _ct);
		AckSent = true;
	}

	public async Task AcknowledgeAsync<TResponse>(TResponse parameter)
	{
		var responseMessage = ArduinoRouterResponseMessage<TResponse>.Create(_request.MessageId, parameter);
		await MessagePackSerializer.SerializeAsync(_stream, responseMessage, cancellationToken: _ct);
		AckSent = true;
	}

	public async Task FailAsync(string errorMessage)
	{
		var responseMessage = ArduinoRouterResponseMessage<object>.CreateError(_request.MessageId, errorMessage);
		await MessagePackSerializer.SerializeAsync(_stream, responseMessage, cancellationToken: _ct);
		AckSent = true;
	}
}
