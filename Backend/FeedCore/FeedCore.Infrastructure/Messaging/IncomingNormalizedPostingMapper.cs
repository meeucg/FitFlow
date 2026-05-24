using FeedCore.Application.Exceptions;
using FeedCore.Application.Models;
using FeedCore.Core.Models;

namespace FeedCore.Infrastructure.Messaging;

internal static class IncomingNormalizedPostingMapper
{
    public static NormalizedJobPostingInput Map(IncomingNormalizedJobPosting message)
    {
        if (message.Payload is null)
            throw new FeedCoreValidationException("Normalized posting payload is required.");

        var display = new JobPostingDisplayData
        {
            Id = Guid.Empty,
            Source = message.Source,
            PostedAt = message.PostedAt,
            Url = message.Url,
            Author = NullIfWhiteSpace(message.Payload.Author),
            Title = NullIfWhiteSpace(message.Payload.Title),
            PriceMin = message.Payload.PriceMin,
            PriceMax = message.Payload.PriceMax,
            Currency = message.Payload.Currency,
            Description = NullIfWhiteSpace(message.Payload.Description),
            Cluster = NullIfWhiteSpace(message.Payload.Cluster),
            AttachedFiles = message.Payload.AttachedFiles?
                .Select(x => new PostingAttachmentData
                {
                    Url = NullIfWhiteSpace(x.Url),
                    Base64 = NullIfWhiteSpace(x.Base64),
                    Extension = x.Extension
                })
                .ToList() ?? [],
            Specializations = message.Payload.Specializations
                .Select(x => new JobPostingSpecializationData { Name = x.Name, AlternativeNames = x.AlternativeNames })
                .ToList(),
            RequiredSkills = message.Payload.RequiredSkills
                .Select(x => new JobPostingSkillData { DisplayName = x.DisplayName, Description = x.Description, AlternativeNames = x.AlternativeNames })
                .ToList(),
            BonusSkills = message.Payload.BonusSkills
                .Select(x => new JobPostingSkillData { DisplayName = x.DisplayName, Description = x.Description, AlternativeNames = x.AlternativeNames })
                .ToList(),
            RequiredTools = message.Payload.RequiredTools
                .Select(x => new JobPostingToolData { ToolStandardName = x.ToolStandardName, ToolAltNames = x.ToolAltNames })
                .ToList(),
            BonusTools = message.Payload.BonusTools
                .Select(x => new JobPostingToolData { ToolStandardName = x.ToolStandardName, ToolAltNames = x.ToolAltNames })
                .ToList(),
            Domains = message.Payload.Domains
                .Select(x => new JobPostingDomainData { Name = x.Name, AlternativeNames = x.AlternativeNames })
                .ToList()
        };

        return new NormalizedJobPostingInput
        {
            Source = message.Source,
            PostedAt = message.PostedAt,
            Url = message.Url,
            DisplayData = display
        };
    }

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
