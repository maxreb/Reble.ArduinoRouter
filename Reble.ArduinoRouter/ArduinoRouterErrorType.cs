namespace Reble.ArduinoRouter;

public enum ArduinoRouterErrorType
{
	// Error codes for the router
	InvalidParams = 1,
	MethodNotAvailable = 2,
	FailedToSendRequests = 3,
	GenericError = 4,
	RouteAlreadyExists = 5
}
