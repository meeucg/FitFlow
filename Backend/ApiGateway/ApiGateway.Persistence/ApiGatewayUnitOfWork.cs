using ApiGateway.Application.Abstractions;

namespace ApiGateway.Persistence;

internal sealed class ApiGatewayUnitOfWork(ApiGatewayDbContext dbContext) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
