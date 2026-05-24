# ApiGateway Generated Documentation

This file is generated from the XML documentation summaries emitted by `ApiGateway.csproj`. Update the XML comments in source code, run `dotnet build .\ApiGateway.sln`, and regenerate this file when signatures or contracts change.

## Runtime Contract

- ApiGateway is a REST facade over InterviewService gRPC.
- Keycloak owns browser authentication, registration, TOTP, sessions, and tokens.
- ApiGateway validates Keycloak access tokens and stores a local user row keyed by the Keycloak `sub` claim.
- Each user has at most one hidden `CurrentInterviewId`; REST clients use `/my-interview` and never send or receive interview ids.

## REST Endpoints

| Method | Path | Auth | Return | Summary |
| --- | --- | --- | --- | --- |
| `GET` | `/` | No | `ServiceStatusDto` | Returns a public status payload confirming that ApiGateway is reachable. |
| `GET` | `/me` | Yes | `CurrentUserDto` | Loads or creates the local user represented by the authenticated Keycloak token. |
| `GET` | `/my-interview` | Yes | `MyInterviewDisplayDto` | Loads the authenticated user's interview, creating the one allowed interview when it does not exist yet. |
| `POST` | `/my-interview/answers` | Yes | `FormElementDto` | Submits an answer to the authenticated user's stored interview and returns the next form element. |

## Endpoints and Error Mapping

### Method: `ApiGateway.Endpoints.FitFlowEndpointRouteBuilderExtensions.GetCurrentUserAsync(ApiGateway.Services.CurrentUserService,Microsoft.AspNetCore.Http.HttpContext,System.Threading.CancellationToken)`

Loads or creates the local user represented by the authenticated Keycloak token.

Parameters:
- `currentUserService`: The service that synchronizes Keycloak claims into the local user table.
- `httpContext`: The current HTTP context containing the authenticated principal.
- `cancellationToken`: The cancellation token for the request.

Returns: A successful HTTP response containing the current local user profile.

### Method: `ApiGateway.Endpoints.FitFlowEndpointRouteBuilderExtensions.GetMyInterviewAsync(FitFlow.Interview.Grpc.Contracts.InterviewGateway.InterviewGatewayClient,ApiGateway.Services.CurrentUserService,ApiGateway.Data.ApiGatewayDbContext,AutoMapper.IMapper,Microsoft.AspNetCore.Http.HttpContext,System.Threading.CancellationToken)`

Loads the authenticated user's interview, creating the one allowed interview when it does not exist yet.

Parameters:
- `client`: The generated gRPC client used to call InterviewService.
- `currentUserService`: The service that resolves the authenticated local user.
- `dbContext`: The ApiGateway database context used to persist the user's interview ownership link.
- `mapper`: The AutoMapper instance that maps generated gRPC models to REST DTOs.
- `httpContext`: The current HTTP context containing the authenticated principal.
- `cancellationToken`: The cancellation token for the request.

Returns: A successful HTTP response containing an interview display without the hidden interview id.

### Method: `ApiGateway.Endpoints.FitFlowEndpointRouteBuilderExtensions.GetServiceStatus`

Returns a small public status payload confirming that ApiGateway is reachable.

Returns: A successful HTTP response containing the ApiGateway service name.

### Method: `ApiGateway.Endpoints.FitFlowEndpointRouteBuilderExtensions.HideInterviewId(ApiGateway.Models.InterviewDisplayDto)`

Removes the service-owned interview id from an interview display before returning it to REST clients.

Parameters:
- `interview`: The mapped interview display that still contains the gRPC interview id.

Returns: An interview display DTO whose shape intentionally omits the interview id.

### Method: `ApiGateway.Endpoints.FitFlowEndpointRouteBuilderExtensions.MapFitFlowApiGatewayEndpoints(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder)`

Adds public health metadata, current-user, and user-owned interview endpoints to the route table.

Parameters:
- `endpoints`: The endpoint route builder that receives the FitFlow routes.

Returns: The same route builder so startup code can continue chaining route registrations.

### Method: `ApiGateway.Endpoints.FitFlowEndpointRouteBuilderExtensions.PostMyInterviewAnswerAsync(ApiGateway.Models.AnswerDto,FitFlow.Interview.Grpc.Contracts.InterviewGateway.InterviewGatewayClient,ApiGateway.Services.CurrentUserService,AutoMapper.IMapper,Microsoft.AspNetCore.Http.HttpContext,System.Threading.CancellationToken)`

Submits an answer to the authenticated user's stored interview and returns the next form element.

Parameters:
- `answer`: The answer payload for the current interview question.
- `client`: The generated gRPC client used to call InterviewService.
- `currentUserService`: The service that resolves the authenticated local user.
- `mapper`: The AutoMapper instance that maps REST DTOs to gRPC models and back.
- `httpContext`: The current HTTP context containing the authenticated principal.
- `cancellationToken`: The cancellation token for the request.

Returns: A successful HTTP response containing the next question or final user profile form element.

### Method: `ApiGateway.Endpoints.GrpcErrors.ToResult(Grpc.Core.RpcException)`

Maps a gRPC exception to the ApiGateway HTTP response contract.

Parameters:
- `exception`: The gRPC exception thrown by the InterviewService client.

Returns: An HTTP result with the configured status code and an error DTO where applicable.

### Type: `ApiGateway.Endpoints.FitFlowEndpointRouteBuilderExtensions`

Registers the FitFlow REST facade endpoints exposed by ApiGateway.

### Type: `ApiGateway.Endpoints.GrpcErrors`

Converts InterviewService gRPC failures into stable REST status codes and error bodies.

## Infrastructure

### Method: `AuthorityRewritingDocumentRetriever.constructor(System.String,System.String,System.Boolean)`

Creates a metadata retriever that can fetch OIDC discovery through an internal Docker hostname while preserving the public issuer.

Parameters:
- `publicAuthority`: The issuer URL that must remain visible in validated tokens.
- `backchannelAuthority`: Optional internal authority used for server-to-server metadata retrieval.
- `requireHttps`: Whether HTTP metadata retrieval should be rejected.

### Method: `AuthorityRewritingDocumentRetriever.GetDocumentAsync(System.String,System.Threading.CancellationToken)`

Gets an OIDC metadata document, rewriting the request address to the backchannel authority when configured.

Parameters:
- `address`: The metadata or signing-key document URL requested by the JWT middleware.
- `cancel`: Cancellation token for the document retrieval request.

Returns: The raw metadata document content.

### Type: `AuthorityRewritingDocumentRetriever`

Retrieves OIDC discovery documents through an optional backchannel authority while preserving the public issuer.

### Type: `Program`

Auto-generated public partial Program class for top-level statement apps.

## Mapping

### Method: `ApiGateway.Mapping.InterviewMappingProfile.constructor`

Creates all interview, answer, profile, and protobuf repeated-field mappings used by ApiGateway.

### Method: `ApiGateway.Mapping.RepeatedFieldToListConverter.Convert(Google.Protobuf.Collections.RepeatedField{},System.Collections.Generic.List{},AutoMapper.ResolutionContext)`

Converts a protobuf repeated field into a mutable list by mapping each item.

Parameters:
- `source`: The protobuf repeated field source collection.
- `destination`: The destination list supplied by AutoMapper.
- `context`: AutoMapper resolution context used to map individual items.

Returns: A list containing mapped destination items.

### Method: `ApiGateway.Mapping.RepeatedFieldToReadOnlyListConverter.Convert(Google.Protobuf.Collections.RepeatedField{},System.Collections.Generic.IReadOnlyList{},AutoMapper.ResolutionContext)`

Converts a protobuf repeated field into a read-only list by mapping each item.

Parameters:
- `source`: The protobuf repeated field source collection.
- `destination`: The destination read-only list supplied by AutoMapper.
- `context`: AutoMapper resolution context used to map individual items.

Returns: A read-only list containing mapped destination items.

### Type: `ApiGateway.Mapping.InterviewMappingProfile`

AutoMapper profile that translates between generated InterviewService gRPC contracts and ApiGateway REST DTOs.

## Persistence

### Method: `ApiGateway.Data.ApiGatewayDbContext.constructor(Microsoft.EntityFrameworkCore.DbContextOptions{ApiGateway.Data.ApiGatewayDbContext})`

Entity Framework database context for ApiGateway-owned persistence.

Parameters:
- `options`: Entity Framework options for the ApiGateway database connection.

### Method: `ApiGateway.Data.ApiGatewayDbContext.OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder)`

Configures the ApiGateway relational model and indexes.

Parameters:
- `modelBuilder`: Entity Framework model builder used to configure entity mappings.

### Method: `ApiGateway.Migrations.CreateUsers.Down(Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder)`

Removes the initial users schema.

Parameters:
- `migrationBuilder`: Builder used to apply rollback schema operations.

### Method: `ApiGateway.Migrations.CreateUsers.Up(Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder)`

Applies the initial users schema.

Parameters:
- `migrationBuilder`: Builder used to apply schema operations.

### Method: `ApiGateway.Migrations.RemoveNicknameAddOptionalNames.Down(Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder)`

Restores the legacy nickname schema.

Parameters:
- `migrationBuilder`: Builder used to apply rollback schema operations.

### Method: `ApiGateway.Migrations.RemoveNicknameAddOptionalNames.Up(Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder)`

Applies the user profile schema update.

Parameters:
- `migrationBuilder`: Builder used to apply schema operations.

### Method: `ApiGateway.Services.CurrentUserService.constructor(ApiGateway.Data.ApiGatewayDbContext)`

Resolves the current Keycloak principal into an ApiGateway local user row.

Parameters:
- `dbContext`: Database context used to read and update local users.

### Property: `ApiGateway.Data.ApiGatewayDbContext.Users`

Local users synchronized from Keycloak access-token claims.

### Property: `ApiGateway.Data.User.CreatedAt`

UTC timestamp for when this user row was created.

### Property: `ApiGateway.Data.User.CurrentInterviewId`

Hidden InterviewService interview id owned by this user.

### Property: `ApiGateway.Data.User.Email`

Email address synchronized from Keycloak claims.

### Property: `ApiGateway.Data.User.FirstName`

Optional first name synchronized from Keycloak claims.

### Property: `ApiGateway.Data.User.Id`

ApiGateway local user identifier.

### Property: `ApiGateway.Data.User.KeycloakSubject`

Stable Keycloak subject claim that owns this local user row.

### Property: `ApiGateway.Data.User.LastName`

Optional last name synchronized from Keycloak claims.

### Property: `ApiGateway.Data.User.UpdatedAt`

UTC timestamp for when this user row was last updated.

### Type: `ApiGateway.Data.ApiGatewayDbContext`

Entity Framework database context for ApiGateway-owned persistence.

Parameters:
- `options`: Entity Framework options for the ApiGateway database connection.

### Type: `ApiGateway.Data.User`

Local ApiGateway user record linked to a Keycloak subject and at most one interview.

### Type: `ApiGateway.Migrations.CreateUsers`

Creates the initial ApiGateway users table and ownership indexes.

### Type: `ApiGateway.Migrations.RemoveNicknameAddOptionalNames`

Removes the legacy nickname column and adds optional first and last name columns.

## REST Models

### Method: `ApiGateway.Models.ErrorDto.constructor(System.String)`

Describes an API error returned by ApiGateway.

Parameters:
- `Message`: Human-readable error details suitable for diagnostics and UI display.

### Method: `ApiGateway.Models.ServiceStatusDto.constructor(System.String)`

Public service status response returned by the root endpoint.

Parameters:
- `Service`: Name of the running service.

### Property: `ApiGateway.Models.AnswerDto.IsSkipped`

Indicates that an optional question was intentionally skipped.

### Property: `ApiGateway.Models.AnswerDto.SelectedOptions`

Selected option indexes and optional selected levels.

### Property: `ApiGateway.Models.AnswerDto.TextAnswer`

Optional free-text answer when the question supports text input.

### Property: `ApiGateway.Models.CurrentUserDto.Email`

Email address synchronized from the Keycloak access token.

### Property: `ApiGateway.Models.CurrentUserDto.FirstName`

Optional first name synchronized from the Keycloak access token.

### Property: `ApiGateway.Models.CurrentUserDto.HasInterview`

Indicates whether this user already has a server-owned interview id.

### Property: `ApiGateway.Models.CurrentUserDto.Id`

ApiGateway local user identifier.

### Property: `ApiGateway.Models.CurrentUserDto.LastName`

Optional last name synchronized from the Keycloak access token.

### Property: `ApiGateway.Models.DomainDto.AlternativeNames`

Alternative names or aliases for the domain.

### Property: `ApiGateway.Models.DomainDto.Name`

Canonical domain name.

### Property: `ApiGateway.Models.ErrorDto.Message`

Human-readable error details suitable for diagnostics and UI display.

### Property: `ApiGateway.Models.FormElementDto.Question`

Next question to ask, or null when the interview has produced a final profile.

### Property: `ApiGateway.Models.FormElementDto.UserProfile`

Final generated user profile, or null while more questions remain.

### Property: `ApiGateway.Models.InterviewDisplayDto.CompletedSteps`

Transcript of completed question and answer pairs.

### Property: `ApiGateway.Models.InterviewDisplayDto.Conclusion`

Final generated user profile when the interview has concluded.

### Property: `ApiGateway.Models.InterviewDisplayDto.CurrentQuestion`

Current question the user should answer next, or null when the interview has concluded.

### Property: `ApiGateway.Models.InterviewDisplayDto.Id`

InterviewService interview identifier; never exposed by user-owned REST endpoints.

### Property: `ApiGateway.Models.InterviewDisplayDto.RequiredAnswers`

Answers already submitted for the setup questions.

### Property: `ApiGateway.Models.InterviewDisplayDto.Setup`

Static setup metadata and required setup questions for the interview.

### Property: `ApiGateway.Models.InterviewSetupDto.HashGuid`

Deterministic setup hash GUID produced from the setup group and payload.

### Property: `ApiGateway.Models.InterviewSetupDto.RequiredQuestions`

Required questions that must be answered before dynamic interview generation starts.

### Property: `ApiGateway.Models.InterviewStepDto.Answer`

Answer submitted by the user for this step.

### Property: `ApiGateway.Models.InterviewStepDto.Question`

Question shown to the user for this step.

### Property: `ApiGateway.Models.MyInterviewDisplayDto.CompletedSteps`

Transcript of completed question and answer pairs.

### Property: `ApiGateway.Models.MyInterviewDisplayDto.Conclusion`

Final generated user profile when the interview has concluded.

### Property: `ApiGateway.Models.MyInterviewDisplayDto.CurrentQuestion`

Current question the user should answer next, or null when the interview has concluded.

### Property: `ApiGateway.Models.MyInterviewDisplayDto.RequiredAnswers`

Answers already submitted for the setup questions.

### Property: `ApiGateway.Models.MyInterviewDisplayDto.Setup`

Static setup metadata and required setup questions for the interview.

### Property: `ApiGateway.Models.OptionAnswerDto.OptionId`

Zero-based index into the question's collection.

### Property: `ApiGateway.Models.OptionAnswerDto.SelectedLevel`

Optional zero-based index into the question's collection.

### Property: `ApiGateway.Models.QuestionDto.AnswerLevels`

Optional zero-based level labels that can be attached to selected options.

### Property: `ApiGateway.Models.QuestionDto.AnswerOptions`

Available zero-based answer options, excluding any free-text option.

### Property: `ApiGateway.Models.QuestionDto.IsOptional`

Indicates whether the question may be skipped.

### Property: `ApiGateway.Models.QuestionDto.IsSingleChoice`

Indicates whether the user should select exactly one answer option.

### Property: `ApiGateway.Models.QuestionDto.PlainTextOptionPresent`

Indicates whether the user may submit a custom free-text answer.

### Property: `ApiGateway.Models.QuestionDto.QuestionText`

Human-readable question text shown to the user.

### Property: `ApiGateway.Models.ServiceStatusDto.Service`

Name of the running service.

### Property: `ApiGateway.Models.SkillDto.AlternativeNames`

Alternative names or aliases for the skill.

### Property: `ApiGateway.Models.SkillDto.Description`

Short description of the skill's meaning in the profile.

### Property: `ApiGateway.Models.SkillDto.DisplayName`

User-facing skill name.

### Property: `ApiGateway.Models.SkillDto.DominanceLevel`

Relative importance of the skill, returned as core, important, secondary, limited, or unspecified.

### Property: `ApiGateway.Models.SpecializationDto.AlternativeNames`

Alternative names or aliases for the specialization.

### Property: `ApiGateway.Models.SpecializationDto.Name`

Canonical specialization name.

### Property: `ApiGateway.Models.ToolDto.ToolAltNames`

Alternative names or aliases for the tool.

### Property: `ApiGateway.Models.ToolDto.ToolStandardName`

Canonical tool name.

### Property: `ApiGateway.Models.ToolDto.UsageFrequency`

Relative tool usage frequency, returned as core, regular, occasional, rare, or unspecified.

### Property: `ApiGateway.Models.UserProfileDto.Cluster`

Broad professional cluster inferred for the user.

### Property: `ApiGateway.Models.UserProfileDto.PreferredDomains`

Preferred work domains inferred from the interview answers.

### Property: `ApiGateway.Models.UserProfileDto.Skills`

Skills inferred from the interview answers.

### Property: `ApiGateway.Models.UserProfileDto.Specializations`

Specializations inferred from the interview answers.

### Property: `ApiGateway.Models.UserProfileDto.Tools`

Tools inferred from the interview answers.

### Type: `ApiGateway.Models.AnswerDto`

User answer submitted to the current interview question.

### Type: `ApiGateway.Models.CurrentUserDto`

Current authenticated user profile stored by ApiGateway.

### Type: `ApiGateway.Models.DomainDto`

Preferred professional domain inferred for the user profile.

### Type: `ApiGateway.Models.ErrorDto`

Describes an API error returned by ApiGateway.

Parameters:
- `Message`: Human-readable error details suitable for diagnostics and UI display.

### Type: `ApiGateway.Models.FormElementDto`

Next piece of interview UI returned after an answer is submitted.

### Type: `ApiGateway.Models.InterviewDisplayDto`

Full interview display used internally before ApiGateway removes the service-owned interview id.

### Type: `ApiGateway.Models.InterviewSetupDto`

Immutable setup definition used to initialize an interview.

### Type: `ApiGateway.Models.InterviewStepDto`

Completed interview step containing a question and the answer submitted for it.

### Type: `ApiGateway.Models.MyInterviewDisplayDto`

User-facing interview display returned by GET /my-interview without exposing the interview id.

### Type: `ApiGateway.Models.OptionAnswerDto`

Selected answer option and optional level for a question.

### Type: `ApiGateway.Models.QuestionDto`

Interview question with choice, level, text, and optional-skip metadata.

### Type: `ApiGateway.Models.ServiceStatusDto`

Public service status response returned by the root endpoint.

Parameters:
- `Service`: Name of the running service.

### Type: `ApiGateway.Models.SkillDto`

Skill inferred for the user profile.

### Type: `ApiGateway.Models.SpecializationDto`

Professional specialization with optional alternative names.

### Type: `ApiGateway.Models.ToolDto`

Tool inferred for the user profile.

### Type: `ApiGateway.Models.UserProfileDto`

Generated professional profile produced when the interview concludes.

## Services

### Method: `ApiGateway.Services.CurrentUserService.GetOrCreateAsync(System.Security.Claims.ClaimsPrincipal,System.Threading.CancellationToken)`

Gets the local user for the authenticated principal, creating or refreshing the row when needed.

Parameters:
- `principal`: Authenticated claims principal produced by JWT bearer validation.
- `cancellationToken`: Cancellation token for database work.

Returns: The local user synchronized from the Keycloak subject, email, and optional name claims.

### Type: `ApiGateway.Services.CurrentUserService`

Resolves the current Keycloak principal into an ApiGateway local user row.

Parameters:
- `dbContext`: Database context used to read and update local users.

