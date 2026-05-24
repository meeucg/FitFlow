using ApiGateway.Application.Models;
using AutoMapper;
using FitFlow.Interview.Grpc.Contracts;
using Google.Protobuf.Collections;

namespace ApiGateway.Infrastructure.Mapping;

/// <summary>
/// AutoMapper profile that translates between generated InterviewService gRPC contracts and ApiGateway REST DTOs.
/// </summary>
public sealed class InterviewMappingProfile : Profile
{
    /// <summary>
    /// Creates all interview, answer, profile, and protobuf repeated-field mappings used by ApiGateway.
    /// </summary>
    public InterviewMappingProfile()
    {
        CreateMap<InterviewDisplay, InterviewDisplayDto>();
        CreateMap<InterviewSetup, InterviewSetupDto>();
        CreateMap<InterviewStep, InterviewStepDto>();
        CreateMap<FormElement, FormElementDto>();
        CreateMap<Question, QuestionDto>();
        CreateMap<UserProfile, UserProfileDto>();
        CreateMap<Specialization, SpecializationDto>();
        CreateMap<Domain, DomainDto>();

        CreateMap<Answer, AnswerDto>()
            .ForMember(
                destination => destination.TextAnswer,
                options => options.MapFrom(source => string.IsNullOrWhiteSpace(source.TextAnswer) ? null : source.TextAnswer));

        CreateMap<OptionAnswer, OptionAnswerDto>()
            .ForMember(
                destination => destination.SelectedLevel,
                options => options.MapFrom(source => source.SelectedLevel >= 0 ? source.SelectedLevel : (int?)null));

        CreateMap<Skill, SkillDto>()
            .ForMember(
                destination => destination.DominanceLevel,
                options => options.MapFrom(source => ToDto(source.DominanceLevel)));

        CreateMap<Tool, ToolDto>()
            .ForMember(
                destination => destination.UsageFrequency,
                options => options.MapFrom(source => ToDto(source.UsageFrequency)));

        CreateMap<AnswerDto, Answer>()
            .ForMember(
                destination => destination.TextAnswer,
                options => options.MapFrom(source => source.TextAnswer ?? string.Empty))
            .ForMember(destination => destination.SelectedOptions, options => options.Ignore())
            .AfterMap((source, destination, context) =>
            {
                destination.SelectedOptions.AddRange(source.SelectedOptions.Select(context.Mapper.Map<OptionAnswer>));
            });

        CreateMap<OptionAnswerDto, OptionAnswer>()
            .ForMember(
                destination => destination.SelectedLevel,
                options => options.MapFrom(source => source.SelectedLevel ?? -1));

        CreateMap(typeof(RepeatedField<>), typeof(List<>)).ConvertUsing(typeof(RepeatedFieldToListConverter<,>));
        CreateMap(typeof(RepeatedField<>), typeof(IReadOnlyList<>)).ConvertUsing(typeof(RepeatedFieldToReadOnlyListConverter<,>));
    }

    private static string ToDto(SkillDominanceLevel source)
    {
        return source switch
        {
            SkillDominanceLevel.Core => "core",
            SkillDominanceLevel.Important => "important",
            SkillDominanceLevel.Secondary => "secondary",
            SkillDominanceLevel.Limited => "limited",
            _ => "unspecified",
        };
    }

    private static string ToDto(ToolUsageFrequency source)
    {
        return source switch
        {
            ToolUsageFrequency.Core => "core",
            ToolUsageFrequency.Regular => "regular",
            ToolUsageFrequency.Occasional => "occasional",
            ToolUsageFrequency.Rare => "rare",
            _ => "unspecified",
        };
    }
}

internal sealed class RepeatedFieldToListConverter<TSource, TDestination>
    : ITypeConverter<RepeatedField<TSource>, List<TDestination>>
{
    /// <summary>
    /// Converts a protobuf repeated field into a mutable list by mapping each item.
    /// </summary>
    /// <param name="source">The protobuf repeated field source collection.</param>
    /// <param name="destination">The destination list supplied by AutoMapper.</param>
    /// <param name="context">AutoMapper resolution context used to map individual items.</param>
    /// <returns>A list containing mapped destination items.</returns>
    public List<TDestination> Convert(
        RepeatedField<TSource> source,
        List<TDestination> destination,
        ResolutionContext context)
    {
        return source.Select(item => context.Mapper.Map<TDestination>(item)).ToList();
    }
}

internal sealed class RepeatedFieldToReadOnlyListConverter<TSource, TDestination>
    : ITypeConverter<RepeatedField<TSource>, IReadOnlyList<TDestination>>
{
    /// <summary>
    /// Converts a protobuf repeated field into a read-only list by mapping each item.
    /// </summary>
    /// <param name="source">The protobuf repeated field source collection.</param>
    /// <param name="destination">The destination read-only list supplied by AutoMapper.</param>
    /// <param name="context">AutoMapper resolution context used to map individual items.</param>
    /// <returns>A read-only list containing mapped destination items.</returns>
    public IReadOnlyList<TDestination> Convert(
        RepeatedField<TSource> source,
        IReadOnlyList<TDestination> destination,
        ResolutionContext context)
    {
        return source.Select(item => context.Mapper.Map<TDestination>(item)).ToList();
    }
}
