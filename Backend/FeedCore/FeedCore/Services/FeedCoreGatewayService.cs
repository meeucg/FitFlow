using System.Globalization;
using FeedCore.Application.Exceptions;
using FeedCore.Application.Models;
using FeedCore.Application.UseCases;
using FeedCore.Core.Models;
using FitFlow.FeedCore.Grpc.Contracts;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using FeedCurrency = FeedCore.Core.Models.Currency;
using ProfileSkillDominanceLevel = FeedCore.Application.Models.SkillDominanceLevel;
using ProfileToolUsageFrequency = FeedCore.Application.Models.ToolUsageFrequency;
using ProtoCurrency = FitFlow.FeedCore.Grpc.Contracts.Currency;

namespace FeedCore.Services;

public sealed class FeedCoreGatewayService(
    AddNewUserUseCase addNewUserUseCase,
    GetJobPostingUseCase getJobPostingUseCase,
    ILogger<FeedCoreGatewayService> logger) : FeedCoreGateway.FeedCoreGatewayBase
{
    public override async Task<UserStarterRecommendations> AddNewUser(
        AddNewUserRequest request,
        ServerCallContext context)
    {
        try
        {
            if (request.InterviewConclusion is null)
                throw new FeedCoreValidationException("interview_conclusion is required.");

            var recommendationIds = await addNewUserUseCase.ExecuteAsync(
                request.UserId,
                MapConclusion(request.InterviewConclusion),
                context.CancellationToken);

            var response = new UserStarterRecommendations();
            response.RecommendationIds.AddRange(recommendationIds.Select(x => x.ToString()));
            return response;
        }
        catch (Exception exception)
        {
            throw ToRpcException(exception);
        }
    }

    public override async Task<JobPostingDisplay> GetJobPosting(
        GetJobPostingRequest request,
        ServerCallContext context)
    {
        try
        {
            var posting = await getJobPostingUseCase.ExecuteAsync(request.Id, context.CancellationToken);
            return MapPosting(posting);
        }
        catch (Exception exception)
        {
            throw ToRpcException(exception);
        }
    }

    private RpcException ToRpcException(Exception exception)
    {
        return exception switch
        {
            FeedCoreValidationException => new RpcException(new Status(StatusCode.InvalidArgument, exception.Message)),
            FeedCoreNotFoundException => new RpcException(new Status(StatusCode.NotFound, exception.Message)),
            EmbeddingProviderException => new RpcException(new Status(StatusCode.Unavailable, "Embedding provider is unavailable.")),
            FeedCorePersistenceException => new RpcException(new Status(StatusCode.Unavailable, "FeedCore persistence is unavailable.")),
            RpcException rpcException => rpcException,
            _ => CreateUnexpectedRpcException(exception)
        };
    }

    private RpcException CreateUnexpectedRpcException(Exception exception)
    {
        logger.LogError(exception, "Unhandled FeedCore gRPC error.");
        return new RpcException(new Status(StatusCode.Unavailable, "FeedCore request failed."));
    }

    private static InterviewConclusionData MapConclusion(InterviewConclusion conclusion)
        => new()
        {
            Cluster = conclusion.Cluster,
            Specializations = conclusion.Specializations
                .Select(x => new ProfileSpecializationData
                {
                    Name = x.Name,
                    AlternativeNames = x.AlternativeNames.ToList()
                })
                .ToList(),
            Skills = conclusion.Skills
                .Select(x => new ProfileSkillData
                {
                    DisplayName = x.DisplayName,
                    Description = x.Description,
                    DominanceLevel = MapSkillDominance(x.DominanceLevel),
                    AlternativeNames = x.AlternativeNames.ToList()
                })
                .ToList(),
            Tools = conclusion.Tools
                .Select(x => new ProfileToolData
                {
                    ToolStandardName = x.ToolStandardName,
                    UsageFrequency = MapToolFrequency(x.UsageFrequency),
                    ToolAltNames = x.ToolAltNames.ToList()
                })
                .ToList(),
            PreferredDomains = conclusion.PreferredDomains
                .Select(x => new ProfileDomainData
                {
                    Name = x.Name,
                    AlternativeNames = x.AlternativeNames.ToList()
                })
                .ToList()
        };

    private static ProfileSkillDominanceLevel MapSkillDominance(
        FitFlow.FeedCore.Grpc.Contracts.SkillDominanceLevel dominanceLevel)
        => dominanceLevel switch
        {
            FitFlow.FeedCore.Grpc.Contracts.SkillDominanceLevel.Core => ProfileSkillDominanceLevel.Core,
            FitFlow.FeedCore.Grpc.Contracts.SkillDominanceLevel.Important => ProfileSkillDominanceLevel.Important,
            FitFlow.FeedCore.Grpc.Contracts.SkillDominanceLevel.Secondary => ProfileSkillDominanceLevel.Secondary,
            FitFlow.FeedCore.Grpc.Contracts.SkillDominanceLevel.Limited => ProfileSkillDominanceLevel.Limited,
            _ => ProfileSkillDominanceLevel.Unspecified
        };

    private static ProfileToolUsageFrequency MapToolFrequency(
        FitFlow.FeedCore.Grpc.Contracts.ToolUsageFrequency usageFrequency)
        => usageFrequency switch
        {
            FitFlow.FeedCore.Grpc.Contracts.ToolUsageFrequency.Core => ProfileToolUsageFrequency.Core,
            FitFlow.FeedCore.Grpc.Contracts.ToolUsageFrequency.Regular => ProfileToolUsageFrequency.Regular,
            FitFlow.FeedCore.Grpc.Contracts.ToolUsageFrequency.Occasional => ProfileToolUsageFrequency.Occasional,
            FitFlow.FeedCore.Grpc.Contracts.ToolUsageFrequency.Rare => ProfileToolUsageFrequency.Rare,
            _ => ProfileToolUsageFrequency.Unspecified
        };

    private static JobPostingDisplay MapPosting(JobPostingDisplayData posting)
    {
        var response = new JobPostingDisplay
        {
            Id = posting.Id.ToString(),
            Source = posting.Source,
            Url = posting.Url,
            PostedAt = Timestamp.FromDateTimeOffset(posting.PostedAt),
            Currency = MapCurrency(posting.Currency)
        };

        SetIfPresent(posting.Author, value => response.Author = value);
        SetIfPresent(posting.Title, value => response.Title = value);
        SetIfPresent(posting.PriceMin?.ToString(CultureInfo.InvariantCulture), value => response.PriceMin = value);
        SetIfPresent(posting.PriceMax?.ToString(CultureInfo.InvariantCulture), value => response.PriceMax = value);
        SetIfPresent(posting.Description, value => response.Description = value);
        SetIfPresent(posting.Cluster, value => response.Cluster = value);

        response.AttachedFiles.AddRange(posting.AttachedFiles.Select(MapAttachment));
        response.Specializations.AddRange(posting.Specializations.Select(MapSpecialization));
        response.RequiredSkills.AddRange(posting.RequiredSkills.Select(MapSkill));
        response.BonusSkills.AddRange(posting.BonusSkills.Select(MapSkill));
        response.RequiredTools.AddRange(posting.RequiredTools.Select(MapTool));
        response.BonusTools.AddRange(posting.BonusTools.Select(MapTool));
        response.Domains.AddRange(posting.Domains.Select(MapDomain));

        return response;
    }

    private static ProtoCurrency MapCurrency(FeedCurrency currency)
        => currency switch
        {
            FeedCurrency.Rub => ProtoCurrency.Rub,
            FeedCurrency.Usd => ProtoCurrency.Usd,
            FeedCurrency.Eur => ProtoCurrency.Eur,
            _ => ProtoCurrency.Unspecified
        };

    private static PostingAttachment MapAttachment(PostingAttachmentData attachment)
    {
        var result = new PostingAttachment { Extension = attachment.Extension };
        SetIfPresent(attachment.Url, value => result.Url = value);
        SetIfPresent(attachment.Base64, value => result.Base64 = value);
        return result;
    }

    private static JobPostingSpecialization MapSpecialization(JobPostingSpecializationData specialization)
    {
        var result = new JobPostingSpecialization { Name = specialization.Name };
        result.AlternativeNames.AddRange(specialization.AlternativeNames);
        return result;
    }

    private static JobPostingSkill MapSkill(JobPostingSkillData skill)
    {
        var result = new JobPostingSkill
        {
            DisplayName = skill.DisplayName,
            Description = skill.Description
        };
        result.AlternativeNames.AddRange(skill.AlternativeNames);
        return result;
    }

    private static JobPostingTool MapTool(JobPostingToolData tool)
    {
        var result = new JobPostingTool { ToolStandardName = tool.ToolStandardName };
        result.ToolAltNames.AddRange(tool.ToolAltNames);
        return result;
    }

    private static JobPostingDomain MapDomain(JobPostingDomainData domain)
    {
        var result = new JobPostingDomain { Name = domain.Name };
        result.AlternativeNames.AddRange(domain.AlternativeNames);
        return result;
    }

    private static void SetIfPresent(string? value, Action<string> set)
    {
        if (!string.IsNullOrWhiteSpace(value))
            set(value);
    }
}
