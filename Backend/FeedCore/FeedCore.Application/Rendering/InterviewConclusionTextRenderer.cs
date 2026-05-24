using System.Text;
using FeedCore.Application.Models;

namespace FeedCore.Application.Rendering;

public sealed class InterviewConclusionTextRenderer
{
    public string Render(InterviewConclusionData conclusion)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Professional profile");
        AppendValue(builder, "Cluster", conclusion.Cluster);

        AppendSection(
            builder,
            "Specializations",
            conclusion.Specializations,
            specialization => FormatNameAliases(specialization.Name, specialization.AlternativeNames));

        AppendSection(
            builder,
            "Skills",
            conclusion.Skills,
            skill =>
                $"{Normalize(skill.DisplayName)}; importance: {skill.DominanceLevel}; description: {Normalize(skill.Description)}; aliases: {FormatAliases(skill.AlternativeNames)}");

        AppendSection(
            builder,
            "Tools",
            conclusion.Tools,
            tool =>
                $"{Normalize(tool.ToolStandardName)}; usage: {tool.UsageFrequency}; aliases: {FormatAliases(tool.ToolAltNames)}");

        AppendSection(
            builder,
            "Domains",
            conclusion.PreferredDomains,
            domain => FormatNameAliases(domain.Name, domain.AlternativeNames));

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
