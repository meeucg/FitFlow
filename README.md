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

The diagram below follows the high-level architecture sketch from the project notes.

```mermaid
flowchart LR
    external["External services<br/>Kwork, Telegram, FL.ru,<br/>freelance.ru"]
    parsers["Parsers"]
    rabbit["RabbitMQ"]
    raw["RawPostingsFilter<br/><br/>Removes very short vacancies<br/>Deduplicates postings<br/>AI-normalizes content"]
    rawTextAi["TextAI"]
    rawDb[("PostgreSQL")]
    feed["FeedCore<br/><br/>Stores normalized vacancies<br/>Stores embeddings<br/>Builds recommendations"]
    feedDb[("PostgreSQL + pgvector")]
    embeddingAi["Embedding AI"]

    gateway["ApiGateway<br/><br/>Single backend entry point<br/>REST, SSE, gRPC clients<br/>Recommendation event worker"]
    frontend["Frontend"]
    keycloak["Keycloak<br/><br/>Authentication and authorization<br/>OIDC + TOTP"]
    interview["InterviewService<br/><br/>Stores and runs interviews"]
    interviewTextAi["TextAI"]
    interviewDb[("PostgreSQL")]
    redis[("Redis")]

    external --> parsers
    parsers -->|raw postings| rabbit
    rabbit -->|raw-postings.incoming| raw
    raw --> rawTextAi
    raw --> rawDb
    raw -->|normalized postings| rabbit
    rabbit -->|feed-core.normalized-postings| feed

    feed --> feedDb
    feed --> embeddingAi
    feed -->|recommendation.created| rabbit
    rabbit -->|recommendation events| gateway

    gateway -->|gRPC| feed
    gateway -->|gRPC| interview
    gateway -->|HTTP auth flow| keycloak
    interview --> interviewTextAi
    interview --> interviewDb
    interview --> redis

    frontend -->|HTTP API| gateway
    gateway -->|SSE live updates| frontend
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
