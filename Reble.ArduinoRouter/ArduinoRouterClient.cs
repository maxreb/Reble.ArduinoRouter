using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using MessagePack;
using Reble.ArduinoRouter.Internal;
using Reble.ArduinoRouter.Internal.MessagePack;

namespace Reble.ArduinoRouter;

[SupportedOSPlatform("linux")]
public interface IArduinoRouterClient
{
	Task<ResponseContext<TResponse>> CallAsync<TResponse>(string methodName, object[]? parameters = null,
		CancellationToken ct = default);

	Task CallAsync(string methodName, object[]? parameters = null, CancellationToken ct = default);

	IAsyncEnumerable<RequestContext<TRequest>>
		ProvideAsync<TRequest>(string methodName, CancellationToken ct = default);
}

[SupportedOSPlatform("linux")]
public sealed class ArduinoRouterClient : IArduinoRouterClient
{
	private uint _messageId;
	private static readonly UnixDomainSocketEndPoint ArduinoRouterRemoteEndpoint = new("/var/run/arduino-router.sock");

	public async Task<ResponseContext<TResponse>> CallAsync<TResponse>(string methodName, object[]? parameters = null,
		CancellationToken ct = default)
	{
		var response = await CallInternal(methodName, parameters, ct);
		return new ResponseContext<TResponse>(response);
	}

	public Task CallAsync(string methodName, object[]? parameters = null, CancellationToken ct = default)
		=> CallInternal(methodName, parameters, ct);

	public async IAsyncEnumerable<RequestContext<TRequest>> ProvideAsync<TRequest>(string methodName,
		[EnumeratorCancellation] CancellationToken ct = default)
	{
		await using var stream = await CreateNewConnectionAsync(ct);

		var msg = ArduinoRouterRequestMessage.CreateRegister(ref _messageId, methodName);
		await MessagePackSerializer.SerializeAsync(stream, msg, cancellationToken: ct);

		using var reader = new MessagePackStreamReader(stream);

		await ReadResponse(reader, ct);


		while (true)
		{
			ct.ThrowIfCancellationRequested();
			var builder = new BridgeMessageBuilder();
			var receivedData = false;

			await foreach (var sequence in reader.ReadArrayAsync(ct))
			{
				receivedData = true;
				builder.Next(sequence);
			}

			if (!receivedData)
				yield break; // Connection closed

			var rawMessage = builder.Build();
			if (rawMessage is not BridgeRequestMessage request)
				throw new InvalidOperationException(
					$"Expected {nameof(BridgeRequestMessage)}, but got {rawMessage.GetType().Name}");

			await using var context = new RequestContext<TRequest>(request, stream, ct);
			yield return context;
		}
	}

	private async Task<BridgeResponseMessage> CallInternal(string methodName, object[]? parameters,
		CancellationToken ct)
	{
		await using var stream = await CreateNewConnectionAsync(ct);

		var requestMessage = ArduinoRouterRequestMessage.Create(ref _messageId, methodName, parameters ?? []);
		await MessagePackSerializer.SerializeAsync(stream, requestMessage, cancellationToken: ct);

		using var reader = new MessagePackStreamReader(stream);
		var response = await ReadResponse(reader, ct);

		return response;
	}

	private static async Task<Stream> CreateNewConnectionAsync(CancellationToken ct)
	{
		var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
		await socket.ConnectAsync(ArduinoRouterRemoteEndpoint, ct);
		return new NetworkStream(socket, ownsSocket: true);
	}

	private static async Task<BridgeResponseMessage> ReadResponse(
		MessagePackStreamReader reader,
		CancellationToken ct)
	{
		var builder = new BridgeMessageBuilder();

		await foreach (var readOnlySequence in reader.ReadArrayAsync(ct))
			builder.Next(readOnlySequence);

		var message = builder.Build();
		if (message is not BridgeResponseMessage response)
			throw new InvalidOperationException(
				$"Expected {nameof(BridgeResponseMessage)}, but got {message.GetType().Name}");

		return response;
	}
}