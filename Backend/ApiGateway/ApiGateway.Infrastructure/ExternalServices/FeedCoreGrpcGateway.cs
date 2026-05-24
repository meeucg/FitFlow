using ApiGateway.Application.Abstractions;
using ApiGateway.Application.Models;
using ApiGateway.Infrastructure.Mapping;
using ApiGateway.Infrastructure.Options;
using FeedCoreContracts = FitFlow.FeedCore.Grpc.Contracts;
using Grpc.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ApiGateway.Infrastructure.ExternalServices;

internal sealed class FeedCoreGrpcGateway(
    FeedCoreContracts.FeedCoreGateway.FeedCoreGatewayClient client,
    IMemoryCache cache,
    IOptions<JobPostingCacheOptions> options) : IFeedCoreGateway
{
    public async Task<IReadOnlyList<Guid>> AddNewUserAsync(
        Guid userId,
        UserProfileDto interviewConclusion,
        CancellationToken cancellationToken)
    {
        try
        {
            var reply = await client.AddNewUserAsync(
                new FeedCoreContracts.AddNewUserRequest
                {
                    UserId = userId.ToString("D"),
                    InterviewConclusion = FeedCoreContractMapper.MapConclusion(interviewConclusion)
                },
                cancellationToken: cancellationToken);

            return reply.RecommendationIds
                .Select(id => Guid.TryParse(id, out var parsed) ? parsed : (Guid?)null)
                .Where(id => id is not null)
                .Select(id => id!.Value)
                .ToList();
        }
        catch (RpcException exception)
        {
            throw GrpcExceptionMapper.Map(exception);
        }
    }

    public async Task<JobPostingDto?> GetJobPostingAsync(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = $"job-posting:{id:D}";
        if (cache.TryGetValue(cacheKey, out JobPostingDto? cached) && cached is not null)
            return cached;

        try
        {
            var reply = await client.GetJobPostingAsync(
                new FeedCoreContracts.GetJobPostingRequest { Id = id.ToString("D") },
                cancellationToken: cancellationToken);
            var dto = FeedCoreContractMapper.MapJobPosting(reply);
            cache.Set(cacheKey, dto, options.Value.JobPostingCacheTtl);
            return dto;
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
        catch (RpcException exception)
        {
            throw GrpcExceptionMapper.Map(exception);
        }
    }
}
