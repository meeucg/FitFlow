using FeedCore.Application.Abstractions;
using FeedCore.Application.Exceptions;
using FeedCore.Application.Models;

namespace FeedCore.Application.UseCases;

public sealed class AcceptNormalizedJobPostingUseCase(
    IFeedCoreStore store,
    TimeProvider timeProvider)
{
    public Task<AcceptNormalizedJobPostingResult> ExecuteAsync(
        NormalizedJobPostingInput posting,
        CancellationToken cancellationToken)
    {
        Validate(posting);

        return store.SaveNormalizedPostingAsync(
            posting,
            timeProvider.GetUtcNow(),
            cancellationToken);
    }

    private static void Validate(NormalizedJobPostingInput posting)
    {
        if (string.IsNullOrWhiteSpace(posting.Source))
            throw new FeedCoreValidationException("Normalized job posting source is required.");

        if (string.IsNullOrWhiteSpace(posting.Url))
            throw new FeedCoreValidationException("Normalized job posting url is required.");

        if (posting.PostedAt == default)
            throw new FeedCoreValidationException("Normalized job posting posted_at is required.");

        if (posting.DisplayData is null)
            throw new FeedCoreValidationException("Normalized job posting payload is required.");
    }
}
