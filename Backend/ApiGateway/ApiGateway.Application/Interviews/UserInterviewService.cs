using ApiGateway.Application.Abstractions;
using ApiGateway.Application.Models;
using ApiGateway.Core;
using ApiGateway.Core.Entities;

namespace ApiGateway.Application.Interviews;

public sealed class UserInterviewService(IInterviewGateway interviewGateway, IUnitOfWork unitOfWork)
{
    public async Task<MyInterviewDisplayDto> GetOrCreateAsync(User user, CancellationToken cancellationToken)
    {
        InterviewDisplayDto display;
        if (user.CurrentInterviewId is null)
        {
            display = await interviewGateway.CreateNewInterviewAsync(cancellationToken);
            user.CurrentInterviewId = Guid.Parse(display.Id);
            user.UpdatedAt = DateTimeOffset.UtcNow;
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            display = await interviewGateway.GetInterviewDisplayAsync(user.CurrentInterviewId.Value, cancellationToken);
        }

        return HideInterviewId(display);
    }

    public async Task<FormElementDto?> AnswerAsync(
        User user,
        AnswerDto answer,
        CancellationToken cancellationToken)
    {
        if (user.CurrentInterviewId is null)
            return null;

        var formElement = await interviewGateway.AnswerAsync(user.CurrentInterviewId.Value, answer, cancellationToken);

        if (formElement.UserProfile is not null &&
            user.RecommendationState is RecommendationInitializationState.NotStarted or RecommendationInitializationState.Failed)
        {
            var now = DateTimeOffset.UtcNow;
            user.RecommendationState = RecommendationInitializationState.Pending;
            user.RecommendationRequestedAt = now;
            user.RecommendationLastError = null;
            user.UpdatedAt = now;
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return formElement;
    }

    private static MyInterviewDisplayDto HideInterviewId(InterviewDisplayDto interview)
        => new()
        {
            Setup = interview.Setup,
            RequiredAnswers = interview.RequiredAnswers,
            CompletedSteps = interview.CompletedSteps,
            CurrentQuestion = interview.CurrentQuestion,
            Conclusion = interview.Conclusion,
        };
}
