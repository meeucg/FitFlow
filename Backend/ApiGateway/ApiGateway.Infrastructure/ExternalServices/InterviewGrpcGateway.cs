using ApiGateway.Application.Abstractions;
using ApiGateway.Application.Models;
using AutoMapper;
using FitFlow.Interview.Grpc.Contracts;
using Grpc.Core;

namespace ApiGateway.Infrastructure.ExternalServices;

internal sealed class InterviewGrpcGateway(
    InterviewGateway.InterviewGatewayClient client,
    IMapper mapper) : IInterviewGateway
{
    public async Task<InterviewDisplayDto> CreateNewInterviewAsync(CancellationToken cancellationToken)
    {
        try
        {
            var reply = await client.CreateNewInterviewAsync(
                new CreateNewInterviewRequest(),
                cancellationToken: cancellationToken);

            return mapper.Map<InterviewDisplayDto>(reply.InterviewDisplay);
        }
        catch (RpcException exception)
        {
            throw GrpcExceptionMapper.Map(exception);
        }
    }

    public async Task<InterviewDisplayDto> GetInterviewDisplayAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var reply = await client.GetInterviewDisplayAsync(
                new GetInterviewDisplayRequest { Id = id.ToString("D") },
                cancellationToken: cancellationToken);

            return mapper.Map<InterviewDisplayDto>(reply.InterviewDisplay);
        }
        catch (RpcException exception)
        {
            throw GrpcExceptionMapper.Map(exception);
        }
    }

    public async Task<FormElementDto> AnswerAsync(
        Guid id,
        AnswerDto answer,
        CancellationToken cancellationToken)
    {
        try
        {
            var reply = await client.AnswerAsync(
                new AnswerRequest
                {
                    Id = id.ToString("D"),
                    Answer = mapper.Map<Answer>(answer),
                },
                cancellationToken: cancellationToken);

            return mapper.Map<FormElementDto>(reply.FormElement);
        }
        catch (RpcException exception)
        {
            throw GrpcExceptionMapper.Map(exception);
        }
    }

    public async Task<UserProfileDto?> GetInterviewConclusionAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var reply = await client.GetInterviewConclusionAsync(
                new GetInterviewConclusionRequest { Id = id.ToString("D") },
                cancellationToken: cancellationToken);

            return reply.UserProfile is null ? null : mapper.Map<UserProfileDto>(reply.UserProfile);
        }
        catch (RpcException exception)
        {
            throw GrpcExceptionMapper.Map(exception);
        }
    }
}
