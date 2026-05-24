# FitFlow ApiGateway API Usage Guide for LLM Agents

This document explains how another agent should use the current FitFlow
ApiGateway REST API. The gateway is a REST facade over `InterviewService`
gRPC, with Keycloak-owned authentication and one hidden interview per user.

The most important rule: clients never receive or send interview ids. A logged
in user has at most one current interview, and the gateway stores that
interview id server-side in its own Postgres database.

## Local Services

Run the stack from this directory:

```powershell
docker compose up --build -d
docker compose ps
```

Default local URLs:

| Service | URL | Notes |
| --- | --- | --- |
| ApiGateway REST API | `http://localhost:5266` | Use this API from clients. |
| Keycloak realm | `http://localhost:8080/realms/fitflow` | Token issuer. |
| Keycloak admin | `http://localhost:8080` | Local admin is `admin` / `admin`. |
| Keycloak OIDC discovery | `http://localhost:8080/realms/fitflow/.well-known/openid-configuration` | Metadata and endpoint discovery. |
| InterviewService gRPC | `http://localhost:7297` | Gateway dependency. Do not call from browser clients. |

Inside Docker, ApiGateway reaches InterviewService at
`http://interview-grpc-api:8080`.

## Authentication Model

Keycloak owns registration, password login, TOTP setup, sessions, access
tokens, and refresh tokens.

ApiGateway only validates Keycloak access tokens. It does not issue tokens and
does not accept passwords or TOTP codes.

Current Keycloak client configuration:

| Item | Value |
| --- | --- |
| Realm | `fitflow` |
| Browser client id | `fitflow-spa` |
| API audience | `fitflow-api` |
| Flow | Authorization Code with PKCE `S256` |
| Direct password grant | Disabled |
| Redirect origins | `http://localhost:3000/*`, `http://localhost:3001/*`, `http://localhost:3002/*` |
| Web origins / CORS | `http://localhost:3000`, `http://localhost:3001`, `http://localhost:3002` |
| Required action | TOTP setup after registration / first login |

The access token must contain the `fitflow-api` audience. The realm import adds
that audience to tokens issued for `fitflow-spa`.

### Browser Token Flow

For browser or SPA callers, use `keycloak-js`:

```ts
import Keycloak from 'keycloak-js'

const keycloak = new Keycloak({
  url: 'http://localhost:8080',
  realm: 'fitflow',
  clientId: 'fitflow-spa',
})

await keycloak.init({
  onLoad: 'check-sso',
  pkceMethod: 'S256',
  checkLoginIframe: false,
})

await keycloak.login({ redirectUri: `${window.location.origin}/profile` })

await keycloak.updateToken(30)
const token = keycloak.token
```

The authorization code appears in the redirect URL briefly, then
`keycloak-js` exchanges it with Keycloak's token endpoint using the PKCE code
verifier. The SPA should keep tokens in memory through the Keycloak adapter.
Do not put tokens in `localStorage`.

Use the token on API calls:

```http
Authorization: Bearer <access_token>
```

### Existing Landing Frontend Helper

The current landing frontend has an auth helper at:

`C:\Users\meeuc\OneDrive\Desktop\FitFlow\ComponentTests\LandingTest\landing-test\src\services\auth.ts`

Use its `getAccessToken()` helper before future API calls. It initializes
Keycloak if needed and runs `updateToken(30)`.

Example frontend API helper:

```ts
import { getAccessToken } from '@/services/auth'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5266'

export async function apiFetch(path: string, init: RequestInit = {}) {
  const token = await getAccessToken()
  if (!token) {
    throw new Error('User is not authenticated')
  }

  const headers = new Headers(init.headers)
  headers.set('Authorization', `Bearer ${token}`)
  headers.set('Content-Type', 'application/json')

  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers,
  })

  if (!response.ok) {
    throw new Error(`API request failed with ${response.status}`)
  }

  return response.json()
}
```

## Current User Creation

On the first authenticated API request, ApiGateway creates or updates a local
user row from token claims:

| Token claim | Local field |
| --- | --- |
| `sub` | `keycloak_subject` |
| `email` | `email` |
| `given_name` | `first_name` |
| `family_name` | `last_name` |

The local user also has `current_interview_id`, which is nullable and unique.
That value is the ownership link between a user and their one interview.

Nickname is not required by ApiGateway.

## Endpoint Summary

| Method | Path | Auth | Purpose |
| --- | --- | --- | --- |
| `GET` | `/` | No | Health-ish public root response. |
| `GET` | `/me` | Yes | Load/create current local user and return user info. |
| `GET` | `/my-interview` | Yes | Load the user's interview, creating it on first call. |
| `POST` | `/my-interview/answers` | Yes | Submit an answer to the user's current interview. |

Removed / intentionally unavailable routes:

| Old route | Replacement |
| --- | --- |
| `POST /interviews` | `GET /my-interview` creates on first access. |
| `GET /interviews/{id}` | `GET /my-interview` |
| `POST /interviews/{id}/answers` | `POST /my-interview/answers` |
| `GetInterviewConclusion` gRPC facade | Not exposed as REST. Read `conclusion` / `userProfile` from interview responses. |

## Common Headers

For authenticated requests:

```http
Authorization: Bearer <access_token>
Accept: application/json
```

For `POST` requests:

```http
Content-Type: application/json
```

## `GET /`

Public endpoint.

Response:

```json
{
  "service": "FitFlow ApiGateway"
}
```

## `GET /me`

Requires authorization.

This endpoint is safe to call immediately after login. It creates the local
ApiGateway user row if this is the user's first API request.

Example:

```bash
curl.exe -H "Authorization: Bearer <access_token>" http://localhost:5266/me
```

Response shape:

```json
{
  "id": "8f76b1a7-4f3d-4c55-8c95-7cf4f4d1e51f",
  "email": "user@example.com",
  "firstName": "Ada",
  "lastName": "Lovelace",
  "hasInterview": false
}
```

Field notes:

| Field | Type | Notes |
| --- | --- | --- |
| `id` | GUID string | ApiGateway local user id. Not the Keycloak `sub`. |
| `email` | string | From Keycloak token claims. |
| `firstName` | string or null | Optional. |
| `lastName` | string or null | Optional. |
| `hasInterview` | boolean | True after the user has a stored current interview id. |

## `GET /my-interview`

Requires authorization.

Behavior:

1. ApiGateway loads or creates the local user from the JWT.
2. If `current_interview_id` is null, ApiGateway calls InterviewService
   `CreateNewInterview`, saves the returned id server-side, and returns the
   display DTO without the interview id.
3. If `current_interview_id` is already set, ApiGateway calls InterviewService
   `GetInterviewDisplay` with the stored id.
4. The REST response never includes the interview id.

Example:

```bash
curl.exe -H "Authorization: Bearer <access_token>" http://localhost:5266/my-interview
```

Response shape:

```json
{
  "setup": {
    "hashGuid": "a1d19c7d-0000-0000-0000-000000000000",
    "requiredQuestions": [
      {
        "questionText": "Question text",
        "answerOptions": ["Option A", "Option B"],
        "answerLevels": [],
        "plainTextOptionPresent": false,
        "isSingleChoice": true,
        "isOptional": false
      }
    ]
  },
  "requiredAnswers": [],
  "completedSteps": [],
  "currentQuestion": {
    "questionText": "Question text",
    "answerOptions": ["Option A", "Option B"],
    "answerLevels": [],
    "plainTextOptionPresent": false,
    "isSingleChoice": true,
    "isOptional": false
  },
  "conclusion": null
}
```

Top-level fields:

| Field | Type | Notes |
| --- | --- | --- |
| `setup` | `InterviewSetup` or null | Static setup used for this interview. |
| `requiredAnswers` | `Answer[]` | Answers submitted for setup questions. |
| `completedSteps` | `InterviewStep[]` | Transcript-style history of questions and answers. |
| `currentQuestion` | `Question` or null | The question the user should answer next. |
| `conclusion` | `UserProfile` or null | Final profile when the interview has concluded. |

Important: `setup.hashGuid` is the setup identity, not the interview id. It is
safe for clients to see.

## `POST /my-interview/answers`

Requires authorization.

This endpoint submits an answer for the current user's stored interview. The
request does not contain an interview id.

If the user has not called `GET /my-interview` yet, this endpoint returns
`400`:

```json
{
  "message": "Interview has not been created yet. Load /my-interview first."
}
```

Request body shape:

```json
{
  "selectedOptions": [
    {
      "optionId": 0,
      "selectedLevel": null
    }
  ],
  "textAnswer": null,
  "isSkipped": false
}
```

Response shape:

```json
{
  "question": {
    "questionText": "Next question text",
    "answerOptions": [],
    "answerLevels": [],
    "plainTextOptionPresent": true,
    "isSingleChoice": false,
    "isOptional": false
  },
  "userProfile": null
}
```

When the interview is finished, `question` is null and `userProfile` is set:

```json
{
  "question": null,
  "userProfile": {
    "cluster": "design",
    "specializations": [],
    "skills": [],
    "tools": [],
    "preferredDomains": []
  }
}
```

### Answer Construction Rules

Always answer the current question returned by `GET /my-interview` or by the
previous `POST /my-interview/answers` response.

Use these rules:

| Question field | Meaning for the client |
| --- | --- |
| `answerOptions` | Select by zero-based index. The first option is `optionId: 0`. |
| `answerLevels` | Optional level labels. If present, `selectedLevel` is a zero-based index into this array. |
| `plainTextOptionPresent` | The user may provide free text in `textAnswer`. If there are no options, use text only. |
| `isSingleChoice` | Usually submit exactly one selected option. |
| `isOptional` | If true, the client may skip with `isSkipped: true`. |

For an option-only single-choice question:

```json
{
  "selectedOptions": [
    {
      "optionId": 0,
      "selectedLevel": null
    }
  ],
  "textAnswer": null,
  "isSkipped": false
}
```

For a text-only question:

```json
{
  "selectedOptions": [],
  "textAnswer": "Industrial designer with 3D modeling experience",
  "isSkipped": false
}
```

For a multi-choice question with levels:

```json
{
  "selectedOptions": [
    {
      "optionId": 0,
      "selectedLevel": 2
    },
    {
      "optionId": 3,
      "selectedLevel": 1
    }
  ],
  "textAnswer": null,
  "isSkipped": false
}
```

For an optional skipped question:

```json
{
  "selectedOptions": [],
  "textAnswer": null,
  "isSkipped": true
}
```

Do not set `isSkipped: true` for required questions.

### Typical Interview Loop

Pseudocode for an agent:

```text
1. Obtain Keycloak access token.
2. GET /me to ensure the local user exists.
3. GET /my-interview.
4. If response.conclusion is set, stop. The interview is complete.
5. Let question = response.currentQuestion.
6. Build an Answer from question.answerOptions, question.answerLevels,
   question.plainTextOptionPresent, question.isSingleChoice, and
   question.isOptional.
7. POST /my-interview/answers.
8. If response.userProfile is set, stop. The interview is complete.
9. If response.question is set, answer that question next.
10. Repeat from step 6.
```

Do not cache or invent an interview id. Do not call old `/interviews/{id}`
routes.

For deterministic local smoke tests, the first setup question currently asks
the user to choose a broad professional category. Option `0` is the IT path,
and option `1` is the design path. Always prefer reading `answerOptions` from
the API response over hardcoding those values in production code.

## DTO Reference

All JSON uses camelCase property names.

The snippets below are schema sketches. Quoted DTO names such as
`"QuestionDto or null"` mean a nested object of that type, not a literal string
returned by the API.

### ErrorDto

```json
{
  "message": "Error details"
}
```

### CurrentUserDto

```json
{
  "id": "guid",
  "email": "string",
  "firstName": "string or null",
  "lastName": "string or null",
  "hasInterview": true
}
```

### MyInterviewDisplayDto

```json
{
  "setup": "InterviewSetupDto or null",
  "requiredAnswers": ["AnswerDto"],
  "completedSteps": ["InterviewStepDto"],
  "currentQuestion": "QuestionDto or null",
  "conclusion": "UserProfileDto or null"
}
```

There is no `id` field here by design.

### InterviewSetupDto

```json
{
  "hashGuid": "guid",
  "requiredQuestions": ["QuestionDto"]
}
```

### InterviewStepDto

```json
{
  "question": "QuestionDto or null",
  "answer": "AnswerDto or null"
}
```

### FormElementDto

Returned from `POST /my-interview/answers`.

```json
{
  "question": "QuestionDto or null",
  "userProfile": "UserProfileDto or null"
}
```

Exactly one of `question` or `userProfile` is normally set.

### QuestionDto

```json
{
  "questionText": "string",
  "answerOptions": ["string"],
  "answerLevels": ["string"],
  "plainTextOptionPresent": false,
  "isSingleChoice": true,
  "isOptional": false
}
```

### AnswerDto

```json
{
  "selectedOptions": ["OptionAnswerDto"],
  "textAnswer": "string or null",
  "isSkipped": false
}
```

### OptionAnswerDto

```json
{
  "optionId": 0,
  "selectedLevel": null
}
```

`optionId` is zero-based. `selectedLevel` is zero-based when used, otherwise
null or omitted.

### UserProfileDto

```json
{
  "cluster": "string",
  "specializations": ["SpecializationDto"],
  "skills": ["SkillDto"],
  "tools": ["ToolDto"],
  "preferredDomains": ["DomainDto"]
}
```

### SpecializationDto

```json
{
  "name": "string",
  "alternativeNames": ["string"]
}
```

### SkillDto

```json
{
  "displayName": "string",
  "description": "string",
  "dominanceLevel": "core",
  "alternativeNames": ["string"]
}
```

`dominanceLevel` values currently returned by the gateway:

```text
core
important
secondary
limited
unspecified
```

### ToolDto

```json
{
  "toolStandardName": "string",
  "usageFrequency": "regular",
  "toolAltNames": ["string"]
}
```

`usageFrequency` values currently returned by the gateway:

```text
core
regular
occasional
rare
unspecified
```

### DomainDto

```json
{
  "name": "string",
  "alternativeNames": ["string"]
}
```

## HTTP Status Codes

| Status | When it happens | Body |
| --- | --- | --- |
| `200` | Successful request. | Endpoint DTO. |
| `400` | Bad request, invalid gRPC argument, or answer before interview creation. | Usually `ErrorDto`. |
| `401` | Missing, expired, invalid, or incomplete auth token. | Usually no stable JSON contract. |
| `404` | InterviewService reports a missing resource. | `ErrorDto`. |
| `499` | gRPC call was cancelled. | No JSON body from the gateway. |
| `502` | Other gRPC failure from InterviewService. | `ErrorDto`. |

Common cases:

| Symptom | Likely cause |
| --- | --- |
| `401` after registration | TOTP setup or OIDC callback did not finish, so the SPA has no access token yet. |
| `401` with a token | Token issuer/audience mismatch, expired token, or wrong realm/client. |
| `400` on `POST /my-interview/answers` | The user has not created their interview with `GET /my-interview` first, or the answer is invalid. |
| `502` on `GET /my-interview` | InterviewService, Redis, or its Postgres dependency is unhealthy/unreachable. |
| `502` after answering required setup questions | The AI provider path may have failed. Check InterviewService logs and `TEXTAI_API_KEY` availability. |

Useful diagnostics:

```powershell
docker compose ps
docker compose logs apigateway --tail=200
docker compose logs interview-grpc-api --tail=200
docker compose logs keycloak --tail=200
```

## PowerShell Examples

Assume `$env:TOKEN` contains a valid Keycloak access token.

Load the current user:

```powershell
$headers = @{ Authorization = "Bearer $env:TOKEN" }
Invoke-RestMethod -Uri "http://localhost:5266/me" -Headers $headers
```

Create or load the current user's interview:

```powershell
$headers = @{ Authorization = "Bearer $env:TOKEN" }
Invoke-RestMethod -Uri "http://localhost:5266/my-interview" -Headers $headers
```

Submit the first option for the current question:

```powershell
$headers = @{
  Authorization = "Bearer $env:TOKEN"
  "Content-Type" = "application/json"
}

$body = @{
  selectedOptions = @(
    @{
      optionId = 0
      selectedLevel = $null
    }
  )
  textAnswer = $null
  isSkipped = $false
} | ConvertTo-Json -Depth 10

Invoke-RestMethod `
  -Method Post `
  -Uri "http://localhost:5266/my-interview/answers" `
  -Headers $headers `
  -Body $body
```

Submit a text answer:

```powershell
$headers = @{
  Authorization = "Bearer $env:TOKEN"
  "Content-Type" = "application/json"
}

$body = @{
  selectedOptions = @()
  textAnswer = "Product designer focused on mobile apps and design systems"
  isSkipped = $false
} | ConvertTo-Json -Depth 10

Invoke-RestMethod `
  -Method Post `
  -Uri "http://localhost:5266/my-interview/answers" `
  -Headers $headers `
  -Body $body
```

## Security and Ownership Rules for Agents

Follow these constraints when building clients or tests:

1. Use Keycloak Authorization Code + PKCE for browser login.
2. Do not implement a custom password form that sends credentials to
   ApiGateway.
3. Do not use Direct Access Grant for normal browser login. It is disabled for
   `fitflow-spa`.
4. Do not store access or refresh tokens in `localStorage`.
5. Do not expose, log, ask for, or invent interview ids.
6. Use `/my-interview` and `/my-interview/answers` only.
7. Treat `/me.id` as a local user id, not as an interview id and not as the
   Keycloak subject.
8. Refresh the token before calls if it may expire soon.

## Resetting Local Auth Data

If Keycloak realm changes do not appear locally, the persistent Keycloak
Postgres volume may already contain an older imported realm. For a full local
reset:

```powershell
docker compose down -v
docker compose up --build -d
```

This deletes local Docker volumes for the stack, including local Keycloak,
ApiGateway, InterviewService Postgres, and Redis data.
