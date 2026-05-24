using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using ApiGateway.Application.Abstractions;
using ApiGateway.Application.Exceptions;
using ApiGateway.Application.Interviews;
using ApiGateway.Application.Models;
using ApiGateway.Application.Recommendations;
using ApiGateway.Application.Users;
using ApiGateway.Authentication;
using ApiGateway.Options;
using ApiGateway.Services;
using Microsoft.Extensions.Options;

namespace ApiGateway.Endpoints;

/// <summary>
/// Registers the FitFlow REST facade endpoints exposed by ApiGateway.
/// </summary>
public static class FitFlowEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Adds public health metadata, current-user, and user-owned interview endpoints to the route table.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder that receives the FitFlow routes.</param>
    /// <returns>The same route builder so startup code can continue chaining route registrations.</returns>
    public static IEndpointRouteBuilder MapFitFlowApiGatewayEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", GetServiceStatus)
            .WithName("GetServiceStatus")
            .Produces<ServiceStatusDto>();

        endpoints.MapGet("/me", GetCurrentUserAsync)
            .WithName("GetCurrentUser")
            .RequireAuthorization()
            .Produces<CurrentUserDto>()
            .Produces(StatusCodes.Status401Unauthorized);

        endpoints.MapGet("/my-interview", GetMyInterviewAsync)
            .WithName("GetMyInterview")
            .RequireAuthorization()
            .Produces<MyInterviewDisplayDto>()
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status404NotFound)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(499);

        endpoints.MapPost("/my-interview/answers", PostMyInterviewAnswerAsync)
            .WithName("PostMyInterviewAnswer")
            .RequireAuthorization()
            .Produces<FormElementDto>()
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status404NotFound)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(499);

        endpoints.MapGet("/my-recommendations/events", StreamRecommendationEventsAsync)
            .WithName("StreamRecommendationEvents")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK, contentType: "text/event-stream")
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        endpoints.MapGet("/job-postings/{id}", GetJobPostingAsync)
            .WithName("GetJobPosting")
            .RequireAuthorization()
            .Produces<JobPostingDto>()
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status404NotFound)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway)
            .Produces(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    /// <summary>
    /// Returns a small public status payload confirming that ApiGateway is reachable.
    /// </summary>
    /// <returns>A successful HTTP response containing the ApiGateway service name.</returns>
    private static IResult GetServiceStatus()
    {
        return Results.Ok(new ServiceStatusDto("FitFlow ApiGateway"));
    }

    /// <summary>
    /// Loads or creates the local user represented by the authenticated Keycloak token.
    /// </summary>
    /// <param name="currentUserService">The service that synchronizes Keycloak claims into the local user table.</param>
    /// <param name="httpContext">The current HTTP context containing the authenticated principal.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A successful HTTP response containing the current local user profile.</returns>
    private static async Task<IResult> GetCurrentUserAsync(
        CurrentUserService currentUserService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var user = await currentUserService.GetOrCreateAsync(
            AuthenticatedUserClaims.From(httpContext.User),
            cancellationToken);
        return Results.Ok(
            new CurrentUserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                HasInterview = user.CurrentInterviewId is not null,
            });
    }

    /// <summary>
    /// Loads the authenticated user's interview, creating the one allowed interview when it does not exist yet.
    /// </summary>
    /// <param name="currentUserService">The service that resolves the authenticated local user.</param>
    /// <param name="interviewService">The service that owns interview orchestration.</param>
    /// <param name="httpContext">The current HTTP context containing the authenticated principal.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A successful HTTP response containing an interview display without the hidden interview id.</returns>
    private static async Task<IResult> GetMyInterviewAsync(
        CurrentUserService currentUserService,
        UserInterviewService interviewService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var user = await currentUserService.GetOrCreateAsync(
            AuthenticatedUserClaims.From(httpContext.User),
            cancellationToken);

        try
        {
            return Results.Ok(await interviewService.GetOrCreateAsync(user, cancellationToken));
        }
        catch (ExternalGatewayException exception)
        {
            return ExternalGatewayErrors.ToResult(exception);
        }
    }

    /// <summary>
    /// Submits an answer to the authenticated user's stored interview and returns the next form element.
    /// </summary>
    /// <param name="answer">The answer payload for the current interview question.</param>
    /// <param name="currentUserService">The service that resolves the authenticated local user.</param>
    /// <param name="interviewService">The service that owns interview orchestration.</param>
    /// <param name="httpContext">The current HTTP context containing the authenticated principal.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A successful HTTP response containing the next question or final user profile form element.</returns>
    private static async Task<IResult> PostMyInterviewAnswerAsync(
        AnswerDto answer,
        CurrentUserService currentUserService,
        UserInterviewService interviewService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var user = await currentUserService.GetOrCreateAsync(
            AuthenticatedUserClaims.From(httpContext.User),
            cancellationToken);

        try
        {
            var formElement = await interviewService.AnswerAsync(user, answer, cancellationToken);
            if (formElement is null)
                return Results.BadRequest(new ErrorDto("Interview has not been created yet. Load /my-interview first."));

            return Results.Ok(formElement);
        }
        catch (ExternalGatewayException exception)
        {
            return ExternalGatewayErrors.ToResult(exception);
        }
    }

    private static async Task StreamRecommendationEventsAsync(
        string? cursor,
        CurrentUserService currentUserService,
        RecommendationSnapshotService recommendationSnapshotService,
        RecommendationSseHub sseHub,
        IOptions<RecommendationsOptions> options,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var cursorKind = RecommendationSnapshotService.ParseCursor(cursor, out var parsedCursor);
        if (cursorKind == RecommendationCursorKind.Invalid)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(
                new ErrorDto("Invalid recommendation cursor."),
                cancellationToken);
            return;
        }

        var user = await currentUserService.GetOrCreateAsync(
            AuthenticatedUserClaims.From(httpContext.User),
            cancellationToken);

        httpContext.Response.Headers.ContentType = "text/event-stream";
        httpContext.Response.Headers.CacheControl = "no-cache";
        httpContext.Response.Headers.Append("X-Accel-Buffering", "no");

        var initialBatch = await recommendationSnapshotService.GetInitialBatchAsync(
            cursorKind,
            parsedCursor,
            user,
            cancellationToken);
        if (initialBatch is not null)
        {
            await WriteSseEventAsync(
                httpContext.Response,
                SseRecommendationEvent.Batch(initialBatch.JobPostingIds, initialBatch.LatestRecommendationAt),
                cancellationToken);
        }

        using var subscription = sseHub.Subscribe(user.Id);

        while (!cancellationToken.IsCancellationRequested)
        {
            using var waitCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            waitCancellation.CancelAfter(options.Value.SseHeartbeatInterval);

            try
            {
                if (!await subscription.Reader.WaitToReadAsync(waitCancellation.Token))
                    break;

                while (subscription.Reader.TryRead(out var recommendationEvent))
                    await WriteSseEventAsync(httpContext.Response, recommendationEvent, cancellationToken);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                await WriteSseHeartbeatAsync(httpContext.Response, cancellationToken);
            }
        }
    }

    private static async Task<IResult> GetJobPostingAsync(
        string id,
        CurrentUserService currentUserService,
        JobPostingLookupService jobPostingLookupService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var jobPostingId))
            return Results.BadRequest(new ErrorDto("Job posting id must be a valid GUID."));

        var user = await currentUserService.GetOrCreateAsync(
            AuthenticatedUserClaims.From(httpContext.User),
            cancellationToken);

        try
        {
            var dto = await jobPostingLookupService.GetRecommendedAsync(user.Id, jobPostingId, cancellationToken);
            if (dto is null)
                return Results.NotFound(new ErrorDto("Job posting was not found."));

            return Results.Ok(dto);
        }
        catch (ExternalGatewayException exception)
        {
            return ExternalGatewayErrors.ToResult(exception);
        }
    }

    private static async Task WriteSseEventAsync(
        HttpResponse response,
        SseRecommendationEvent recommendationEvent,
        CancellationToken cancellationToken)
    {
        var id = recommendationEvent.LatestRecommendationAt
            .UtcDateTime
            .ToString("O", CultureInfo.InvariantCulture);
        var data = JsonSerializer.Serialize(recommendationEvent.Command, SseJsonOptions);

        await response.WriteAsync($"id: {id}\n", cancellationToken);
        await response.WriteAsync($"data: {data}\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }

    private static async Task WriteSseHeartbeatAsync(HttpResponse response, CancellationToken cancellationToken)
    {
        await response.WriteAsync(": keep-alive\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }

    private static readonly JsonSerializerOptions SseJsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower
    };
}

/// <summary>
/// Converts InterviewService gRPC failures into stable REST status codes and error bodies.
/// </summary>
internal static class ExternalGatewayErrors
{
    /// <summary>
    /// Maps a gRPC exception to the ApiGateway HTTP response contract.
    /// </summary>
    /// <param name="exception">The external gateway exception thrown by an infrastructure adapter.</param>
    /// <returns>An HTTP result with the configured status code and an error DTO where applicable.</returns>
    public static IResult ToResult(ExternalGatewayException exception)
    {
        var error = new ErrorDto(exception.Message);

        return exception.Failure switch
        {
            ExternalGatewayFailure.InvalidArgument => Results.BadRequest(error),
            ExternalGatewayFailure.NotFound => Results.NotFound(error),
            ExternalGatewayFailure.Cancelled => Results.StatusCode(499),
            _ => Results.Json(error, statusCode: StatusCodes.Status502BadGateway),
        };
    }
}
