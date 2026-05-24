using ApiGateway.Application.Abstractions;
using ApiGateway.Application.Exceptions;
using Grpc.Core;

namespace ApiGateway.Infrastructure.ExternalServices;

internal static class GrpcExceptionMapper
{
    public static ExternalGatewayException Map(RpcException exception)
        => new(ToFailure(exception.StatusCode), exception.Status.Detail, exception);

    private static ExternalGatewayFailure ToFailure(StatusCode statusCode)
        => statusCode switch
        {
            StatusCode.InvalidArgument => ExternalGatewayFailure.InvalidArgument,
            StatusCode.NotFound => ExternalGatewayFailure.NotFound,
            StatusCode.Cancelled => ExternalGatewayFailure.Cancelled,
            _ => ExternalGatewayFailure.Unavailable
        };
}
