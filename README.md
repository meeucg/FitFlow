# FitFlow

FitFlow is a job recommendation and interview preparation platform for freelance and tech specialists. It collects postings from external freelance sources, cleans and normalizes them with AI, compares them with a user's profile, and streams matching opportunities back to the user.

The core idea is simple: instead of manually watching many marketplaces and trying to decide which jobs are worth attention, a user keeps a structured FitFlow profile, completes an interview-style profile setup, and receives a live feed of relevant vacancies and projects.

## What Users Get

- A single web interface for profile, interview, and recommendation workflows.
- Authentication through Keycloak with an OIDC-based login flow.
- AI-assisted interview/profile collection.
- A recommendation feed based on normalized job postings and profile embeddings.
- Live recommendation updates through server-sent events.

## High Level Architecture

The diagram below recreates the high-level architecture sketch from the project notes.

```mermaid
flowchart TB
    classDef service fill:#ffffff,stroke:#222222,stroke-width:2px,color:#222222;
    classDef store fill:#ffffff,stroke:#222222,stroke-width:2px,color:#222222;
    classDef external fill:#ffffff,stroke:#222222,stroke-width:2px,color:#222222;
    classDef note fill:transparent,stroke:transparent,color:#222222;
    classDef hidden fill:transparent,stroke:transparent,color:transparent;

    subgraph topSupport[" "]
        direction LR
        topBlank1[" "]:::hidden
        topBlank2[" "]:::hidden
        rawTextAI@{ shape: tri, label: "TextAI" }
        rawPg[("pgsql")]
        feedNote["• Хранение + embedding<br/>нормализованных<br/>вакансий<br/>• Рекомендации"]:::note
        feedPg[("pgsql")]
    end

    subgraph ingestion[" "]
        direction LR
        externalServices@{ shape: tri, label: "Kwork, tg, fl,<br/>freelance.ru" }
        parsers["Parsers"]:::service
        rawFilter["RawPostingsFilter"]:::service
        feedCore["FeedCore"]:::service
        embeddingAI@{ shape: tri, label: "Embeding<br/>AI" }
    end

    subgraph topNotes[" "]
        direction LR
        externalNote["External services"]:::note
        noteBlank1[" "]:::hidden
        rawNote["• Удаление вакансий<br/>меньше 20 слов<br/>• Дедупликация<br/>• ИИ-нормализация"]:::note
        noteBlank2[" "]:::hidden
    end

    subgraph middleSupport[" "]
        direction LR
        interviewTextAI@{ shape: tri, label: "TextAI" }
        midBlank1[" "]:::hidden
        workerNote["Worker"]:::note
    end

    subgraph middle[" "]
        direction LR
        interviewNote["• Хранение и<br/>проведение<br/>интервью"]:::note
        interviewService["InterviewService"]:::service
        apiGateway["ApiGateway"]:::service
        keycloak["Keycloak"]:::service
        keycloakNote["• Authentication/Auth<br/>orization (OIDC +<br/>TOTP)"]:::note
    end

    subgraph lower[" "]
        direction LR
        interviewPg[("pgsql")]
        interviewRedis[("redis")]
        gatewayNote["• Общая точка входа"]:::note
        frontend["Frontend"]:::service
    end

    externalServices --> parsers
    parsers -->|RabbitMq| rawFilter
    rawFilter -->|RabbitMq| feedCore
    feedCore --> embeddingAI

    rawFilter --> rawTextAI
    rawFilter --> rawPg
    feedCore --> feedPg

    interviewService --> interviewTextAI
    interviewService --> interviewPg
    interviewService --> interviewRedis

    apiGateway -->|grpc| interviewService
    apiGateway -->|http| keycloak
    apiGateway -->|grpc| feedCore
    feedCore -->|RabbitMq| workerNote
    workerNote --> apiGateway

    apiGateway -->|SSE| frontend
    frontend -->|Http| apiGateway

    class rawTextAI,externalServices,embeddingAI,interviewTextAI external;
    class rawPg,feedPg,interviewPg,interviewRedis store;

    style topSupport fill:transparent,stroke:transparent
    style ingestion fill:transparent,stroke:transparent
    style topNotes fill:transparent,stroke:transparent
    style middleSupport fill:transparent,stroke:transparent
    style middle fill:transparent,stroke:transparent
    style lower fill:transparent,stroke:transparent
```

## Main Runtime Flow

1. Parsers collect raw postings from external freelance and job sources.
2. RabbitMQ delivers raw postings to RawPostingsFilter.
3. RawPostingsFilter removes low-value postings, deduplicates data, calls TextAI for normalization, stores processed input, and publishes normalized postings.
4. FeedCore consumes normalized postings, creates embeddings through the Embedding AI provider, stores searchable vectors in PostgreSQL/pgvector, and publishes recommendation events.
5. ApiGateway consumes recommendation events and exposes recommendations to the frontend through REST and SSE.
6. InterviewService handles interview/profile workflows and uses TextAI for AI-assisted validation and generation.
7. Keycloak owns authentication and authorization for browser users.

## Repository Layout

| Path | Purpose |
| --- | --- |
| `Frontend` | Vue frontend served through Nginx in Docker. |
| `Backend/ApiGateway` | Public backend entry point, REST/SSE API, Keycloak integration, and gRPC clients. |
| `Backend/InterviewService` | Interview/profile domain service with PostgreSQL and Redis persistence. |
| `Backend/FeedCore` | Recommendation domain service, normalized posting ingestion, embeddings, and recommendation events. |
| `Backend/RawPostingsFilter` | Raw posting consumer, deduplication, AI normalization, and normalized posting publisher. |
| `Backend/AIServices` | Shared AI provider abstractions and helpers. |
| `Backend/Parsers/ParserMock` | Mock parser used in this repository to publish sample postings into RabbitMQ. |
| `observability` | Prometheus and Grafana configuration. |
| `compose.yaml` | Top-level Docker Compose entry point for the project. |

This public/display repository intentionally includes only `ParserMock` under `Backend/Parsers`. The full private/local workspace can contain additional production parsers.

## Services

### Frontend

The frontend is the browser-facing FitFlow application. It handles login redirects, profile/interview screens, recommendation views, and live updates from the backend.

### ApiGateway

ApiGateway is the single backend entry point for the frontend. It validates Keycloak tokens, exposes HTTP APIs, opens SSE streams for live recommendations, calls FeedCore and InterviewService over gRPC, and consumes recommendation events from RabbitMQ.

### InterviewService

InterviewService stores interview setup data, active interview state, and archived interview results. PostgreSQL stores durable interview data, Redis supports active state, and TextAI supports AI-assisted profile/interview logic.

### RawPostingsFilter

RawPostingsFilter is the ingestion quality gate. It consumes raw parser output, removes low-quality postings, deduplicates similar postings, normalizes useful data through TextAI, stores the result, and publishes normalized postings.

### FeedCore

FeedCore is the recommendation engine. It consumes normalized postings, creates embeddings, stores searchable job/profile vectors in PostgreSQL with pgvector, serves recommendation/profile operations over gRPC, and publishes recommendation events.

### RabbitMQ

RabbitMQ is the event backbone between parsers, RawPostingsFilter, FeedCore, and ApiGateway. It carries raw postings, normalized postings, dead-letter queues, and recommendation events.

### Keycloak

Keycloak provides OIDC authentication and authorization. The project includes a FitFlow realm import and a custom theme.

### Observability

Prometheus scrapes application and RabbitMQ metrics. Grafana is provisioned with a Prometheus datasource and a FitFlow dashboard.

## Configuration

Real `appsettings.json` files are intentionally ignored by Git because they can contain API keys and local secrets. The repository includes sanitized `appsettings.Development.json` files that mirror the real configuration shape but leave API keys empty.

Before running AI-backed features locally, add local secret values in ignored `appsettings.json` files or in local development settings that are not committed.

## Docker Quick Start

From the repository root:

```powershell
docker compose up -d --build
```

Useful local endpoints:

| Service | URL |
| --- | --- |
| Frontend | http://localhost:5173 |
| ApiGateway | http://localhost:5266 |
| Keycloak | http://localhost:8080 |
| RabbitMQ Management | http://localhost:15672 |
| Prometheus | http://localhost:9090 |
| Grafana | http://localhost:3001 |
| FeedCore gRPC | http://localhost:7301 |
| FeedCore metrics/health | http://localhost:7302 |
| InterviewService gRPC | http://localhost:7297 |
| InterviewService metrics/health | http://localhost:7298 |
| RawPostingsFilter metrics/health | http://localhost:5089 |
| ParserMock metrics/health | http://localhost:5088 |

## Persistent Volumes

Docker Compose keeps service state in named volumes:

| Volume | Owner |
| --- | --- |
| `apigateway-postgres-data` | ApiGateway PostgreSQL |
| `feedcore-postgres-data` | FeedCore PostgreSQL + pgvector |
| `interview-postgres-auth-data-2` | InterviewService PostgreSQL |
| `keycloak-postgres-data` | Keycloak PostgreSQL |
| `raw-postings-filter-postgres-data` | RawPostingsFilter PostgreSQL |
| `prometheus-data` | Prometheus |
| `grafana-data` | Grafana |

## Development Notes

- Use the root `compose.yaml` as the main entry point.
- Keep API keys out of Git.
- Keep `appsettings.Development.json` safe to share.
- Use `ParserMock` for demo ingestion when the production parser set is not present.
- Grafana defaults to `admin/admin` unless the existing Grafana volume has already changed the password.
