using ApiGateway.Application.Models;

namespace ApiGateway.Application.Abstractions;

public interface IFeedCoreGateway
{
    Task<IReadOnlyList<Guid>> AddNewUserAsync(
        Guid userId,
        UserProfileDto interviewConclusion,
        CancellationToken cancellationToken);

    Task<JobPostingDto?> GetJobPostingAsync(Guid id, CancellationToken cancellationToken);
}
