using ApiGateway.Application.Models;

namespace ApiGateway.Application.Abstractions;

public interface IInterviewGateway
{
    Task<InterviewDisplayDto> CreateNewInterviewAsync(CancellationToken cancellationToken);

    Task<InterviewDisplayDto> GetInterviewDisplayAsync(Guid id, CancellationToken cancellationToken);

    Task<FormElementDto> AnswerAsync(Guid id, AnswerDto answer, CancellationToken cancellationToken);

    Task<UserProfileDto?> GetInterviewConclusionAsync(Guid id, CancellationToken cancellationToken);
}
