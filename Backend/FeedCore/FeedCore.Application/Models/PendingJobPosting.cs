using FeedCore.Core.Models;

namespace FeedCore.Application.Models;

public sealed record PendingJobPosting(Guid Id, JobPostingDisplayData DisplayData);
