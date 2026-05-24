using System.Text;
using FeedCore.Core.Models;

namespace FeedCore.Application.Rendering;

public sealed class JobPostingTextRenderer
{
    public string Render(JobPostingDisplayData posting)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Job posting");
        AppendValue(builder, "Cluster", posting.Cluster);

        AppendSection(
            builder,
            "Specializations",
            posting.Specializations,
            specialization => FormatNameAliases(specialization.Name, specialization.AlternativeNames));

        AppendSection(
            builder,
            "Required skills",
            posting.RequiredSkills,
            skill =>
                $"{Normalize(skill.DisplayName)}; description: {Normalize(skill.Description)}; aliases: {FormatAliases(skill.AlternativeNames)}");

        AppendSection(
            builder,
            "Bonus skills",
            posting.BonusSkills,
            skill =>
                $"{Normalize(skill.DisplayName)}; description: {Normalize(skill.Description)}; aliases: {FormatAliases(skill.AlternativeNames)}");

        AppendSection(
            builder,
            "Required tools",
            posting.RequiredTools,
            tool => $"{Normalize(tool.ToolStandardName)}; aliases: {FormatAliases(tool.ToolAltNames)}");

        AppendSection(
            builder,
            "Bonus tools",
            posting.BonusTools,
            tool => $"{Normalize(tool.ToolStandardName)}; aliases: {FormatAliases(tool.ToolAltNames)}");

        AppendSection(
            builder,
            "Domains",
            posting.Domains,
            domain => FormatNameAliases(domain.Name, domain.AlternativeNames));

        var title = Normalize(posting.Title);
        var description = Normalize(posting.Description);

        if (title.Length > 0 || description.Length > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Work summary:");
            AppendValue(builder, "Title", title);
            AppendValue(builder, "Description", description);
        }

        return builder.ToString().Trim();
    }

    private static void AppendValue(StringBuilder builder, string label, string? value)
    {
        var normalized = Normalize(value);
        if (normalized.Length == 0)
            return;

        builder.Append(label).Append(": ").AppendLine(normalized);
    }

    private static void AppendSection<T>(
        StringBuilder builder,
        string title,
        IReadOnlyList<T> values,
        Func<T, string> formatter)
    {
        var lines = values
            .Select(formatter)
            .Select(Normalize)
            .Where(static x => x.Length > 0)
            .ToList();

        if (lines.Count == 0)
            return;

        builder.AppendLine();
        builder.AppendLine(title + ":");

        foreach (var line in lines)
            builder.Append("- ").AppendLine(line);
    }

    private static string FormatNameAliases(string name, IReadOnlyList<string> aliases)
        => $"{Normalize(name)}; aliases: {FormatAliases(aliases)}";

    private static string FormatAliases(IReadOnlyList<string> aliases)
    {
        var joined = string.Join(", ", aliases.Select(Normalize).Where(static x => x.Length > 0));
        return joined.Length == 0 ? "none" : joined;
    }

    private static string Normalize(string? value)
        => string.Join(' ', (value ?? string.Empty).Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
