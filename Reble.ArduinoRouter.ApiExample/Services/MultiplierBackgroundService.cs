namespace Reble.ArduinoRouter.ApiExample.Services;

public class MultiplierBackgroundService : BackgroundService
{
	private readonly IServiceScopeFactory _scopeFactory;

	public MultiplierBackgroundService(IServiceScopeFactory scopeFactory)
	{
		_scopeFactory = scopeFactory;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var scope = _scopeFactory.CreateScope();
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<MultiplierBackgroundService>>();
		var router = scope.ServiceProvider.GetRequiredService<IArduinoRouterClient>();
		try
		{
			await foreach (var requestContext in router.ProvideAsync<double>("multiply", stoppingToken))
			{
				var factors = requestContext.DeserializePayloadArray();
				var product = factors[0] * factors[1];
				logger.LogInformation("{Fac1} * {Fac2} = {Product}", factors[0], factors[1], product);
				await requestContext.AcknowledgeAsync(product);
			}
		}
		catch (OperationCanceledException)
		{
			//Nothing to log, app is shutting down
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An exception occurred while executing the arduino router service");
		}
	}
}