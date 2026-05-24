using FeedCore.Application.Abstractions;
using FeedCore.Application.Exceptions;
using FeedCore.Core.Models;

namespace FeedCore.Application.UseCases;

public sealed class GetJobPostingUseCase(IFeedCoreStore store)
{
    public async Task<JobPostingDisplayData> ExecuteAsync(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var parsedId))
            throw new FeedCoreValidationException("Job posting id must be a valid GUID.");

        return await store.GetJobPostingDisplayAsync(parsedId, cancellationToken)
               ?? throw new FeedCoreNotFoundException("Job posting was not found.");
    }
}
