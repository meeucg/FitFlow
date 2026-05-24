namespace ApiGateway.Application.Exceptions;

public sealed class ExternalGatewayException(
    ExternalGatewayFailure failure,
    string message,
    Exception? innerException = null) : Exception(message, innerException)
{
    public ExternalGatewayFailure Failure { get; } = failure;
}

public enum ExternalGatewayFailure
{
    InvalidArgument,
    NotFound,
    Cancelled,
    Unavailable
}
