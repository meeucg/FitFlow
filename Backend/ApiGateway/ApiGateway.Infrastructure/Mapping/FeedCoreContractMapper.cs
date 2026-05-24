using ApiGateway.Application.Models;
using FeedCoreContracts = FitFlow.FeedCore.Grpc.Contracts;

namespace ApiGateway.Infrastructure.Mapping;

/// <summary>
/// Maps between ApiGateway/InterviewService models and FeedCore public contracts.
/// </summary>
public static class FeedCoreContractMapper
{
    /// <summary>
    /// Copies an InterviewService user profile into FeedCore's independent interview conclusion contract.
    /// </summary>
    public static FeedCoreContracts.InterviewConclusion MapConclusion(UserProfileDto profile)
    {
        var conclusion = new FeedCoreContracts.InterviewConclusion
        {
            Cluster = profile.Cluster
        };

        conclusion.Specializations.AddRange(profile.Specializations.Select(MapSpecialization));
        conclusion.Skills.AddRange(profile.Skills.Select(MapSkill));
        conclusion.Tools.AddRange(profile.Tools.Select(MapTool));
        conclusion.PreferredDomains.AddRange(profile.PreferredDomains.Select(MapDomain));
        return conclusion;
    }

    /// <summary>
    /// Maps a FeedCore job posting display to the ApiGateway REST DTO.
    /// </summary>
    public static JobPostingDto MapJobPosting(FeedCoreContracts.JobPostingDisplay posting)
        => new()
        {
            Id = posting.Id,
            Source = posting.Source,
            PostedAt = posting.PostedAt.ToDateTimeOffset(),
            Url = posting.Url,
            Author = posting.HasAuthor ? posting.Author : null,
            Title = posting.HasTitle ? posting.Title : null,
            PriceMin = posting.HasPriceMin ? posting.PriceMin : null,
            PriceMax = posting.HasPriceMax ? posting.PriceMax : null,
            Currency = ToDto(posting.Currency),
            Description = posting.HasDescription ? posting.Description : null,
            Cluster = posting.HasCluster ? posting.Cluster : null,
            AttachedFiles = posting.AttachedFiles
                .Select(x => new PostingAttachmentDto(
                    x.HasUrl ? x.Url : null,
                    x.HasBase64 ? x.Base64 : null,
                    x.Extension))
                .ToList(),
            Specializations = posting.Specializations
                .Select(x => new NamedAliasesDto(x.Name, x.AlternativeNames.ToList()))
                .ToList(),
            RequiredSkills = posting.RequiredSkills
                .Select(x => new JobPostingSkillDto(x.DisplayName, x.Description, x.AlternativeNames.ToList()))
                .ToList(),
            BonusSkills = posting.BonusSkills
                .Select(x => new JobPostingSkillDto(x.DisplayName, x.Description, x.AlternativeNames.ToList()))
                .ToList(),
            RequiredTools = posting.RequiredTools
                .Select(x => new ToolAliasesDto(x.ToolStandardName, x.ToolAltNames.ToList()))
                .ToList(),
            BonusTools = posting.BonusTools
                .Select(x => new ToolAliasesDto(x.ToolStandardName, x.ToolAltNames.ToList()))
                .ToList(),
            Domains = posting.Domains
                .Select(x => new NamedAliasesDto(x.Name, x.AlternativeNames.ToList()))
                .ToList()
        };

    private static FeedCoreContracts.Specialization MapSpecialization(SpecializationDto specialization)
    {
        var result = new FeedCoreContracts.Specialization { Name = specialization.Name };
        result.AlternativeNames.AddRange(specialization.AlternativeNames);
        return result;
    }

    private static FeedCoreContracts.Skill MapSkill(SkillDto skill)
    {
        var result = new FeedCoreContracts.Skill
        {
            DisplayName = skill.DisplayName,
            Description = skill.Description,
            DominanceLevel = MapSkillDominance(skill.DominanceLevel)
        };
        result.AlternativeNames.AddRange(skill.AlternativeNames);
        return result;
    }

    private static FeedCoreContracts.Tool MapTool(ToolDto tool)
    {
        var result = new FeedCoreContracts.Tool
        {
            ToolStandardName = tool.ToolStandardName,
            UsageFrequency = MapToolFrequency(tool.UsageFrequency)
        };
        result.ToolAltNames.AddRange(tool.ToolAltNames);
        return result;
    }

    private static FeedCoreContracts.Domain MapDomain(DomainDto domain)
    {
        var result = new FeedCoreContracts.Domain { Name = domain.Name };
        result.AlternativeNames.AddRange(domain.AlternativeNames);
        return result;
    }

    private static FeedCoreContracts.SkillDominanceLevel MapSkillDominance(string dominanceLevel)
        => dominanceLevel switch
        {
            "core" => FeedCoreContracts.SkillDominanceLevel.Core,
            "important" => FeedCoreContracts.SkillDominanceLevel.Important,
            "secondary" => FeedCoreContracts.SkillDominanceLevel.Secondary,
            "limited" => FeedCoreContracts.SkillDominanceLevel.Limited,
            _ => FeedCoreContracts.SkillDominanceLevel.Unspecified
        };

    private static FeedCoreContracts.ToolUsageFrequency MapToolFrequency(string usageFrequency)
        => usageFrequency switch
        {
            "core" => FeedCoreContracts.ToolUsageFrequency.Core,
            "regular" => FeedCoreContracts.ToolUsageFrequency.Regular,
            "occasional" => FeedCoreContracts.ToolUsageFrequency.Occasional,
            "rare" => FeedCoreContracts.ToolUsageFrequency.Rare,
            _ => FeedCoreContracts.ToolUsageFrequency.Unspecified
        };

    private static string ToDto(FeedCoreContracts.Currency currency)
        => currency switch
        {
            FeedCoreContracts.Currency.Rub => "rub",
            FeedCoreContracts.Currency.Usd => "usd",
            FeedCoreContracts.Currency.Eur => "eur",
            _ => "unspecified"
        };
}
