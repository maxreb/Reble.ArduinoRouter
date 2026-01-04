namespace Reble.ArduinoRouter;

public class ArduinoRouterResponseException : ArduinoRouterException
{
	public ArduinoRouterResponseException(ArduinoRouterError error) : base(
		$"Error on response: {error.Message} ({error.Type})")
	{
	}
}
