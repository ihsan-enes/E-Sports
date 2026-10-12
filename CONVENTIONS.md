# Project: E-Spor — esports tracker (Turkey + international)

## Goal
Web app tracking professional esports for ONLY 3 games: League of Legends, CS2, Valorant.
Features: live scores, tournament/match calendar, game filter, team & player stats.
All data comes automatically from an external esports API via background jobs. No manual data entry, ever.

## Repo layout (git root = E-Spor)
- /backend  -> ASP.NET Core Web API (latest .NET LTS), C#, Clean Architecture
- /frontend -> Vue 3 + Vite + TypeScript
- docker-compose.yml at root for local PostgreSQL

## Relevance rule (what we store)
Only store a match/tournament if ANY of these is true:
1. Tournament tier is S or A (international level)
2. At least one participant team is Turkish (country = TR)
3. Tournament belongs to a Turkish league (configurable slug list)
Everything else (e.g. small regional leagues from other countries) must be discarded BEFORE saving to DB.
Configurable via `RelevanceOptions` in appsettings (AllowedTiers, IncludeTurkishTeams, TurkishLeagueSlugs).

## Backend architecture
Solution `ESpor.sln` with projects:
- ESpor.Domain         (entities, enums, no dependencies)
- ESpor.Application    (use-case handlers, DTOs, interfaces, validators; depends on Domain only)
- ESpor.Infrastructure (EF Core, external API providers, background jobs; depends on Application)
- ESpor.Api            (controllers/minimal endpoints, DI, Program.cs; depends on Application + Infrastructure)

Rules:
- Dependencies point inward only. Domain has zero NuGet dependencies.
- Do NOT use MediatR or AutoMapper (commercial licenses). Use plain handler classes (e.g. `GetLiveMatchesHandler`) and manual mapping.
- FluentValidation allowed. Use Result pattern for expected failures, ProblemDetails for HTTP errors, global exception handler.
- async everywhere, pass CancellationToken.
- Database: PostgreSQL via EF Core + Npgsql.EntityFrameworkCore.PostgreSQL. Use migrations. Use snake_case naming (EFCore.NamingConventions) and `timestamptz` for all dates.
- Connection string from user-secrets / environment variable (ConnectionStrings__Default). appsettings.Development.json may contain only a local docker dev connection string with a throwaway password.
- docker-compose.yml: postgres:17 service with a named volume, port 5432.
- API docs: built-in Microsoft.AspNetCore.OpenApi + Scalar.AspNetCore (UI at /scalar/v1 in Development). Do NOT use Swashbuckle.
- All DateTimes are UTC (DateTimeOffset or DateTime with Kind=Utc).
- Secrets (API keys, admin key) via user-secrets / environment variables, never in committed files.
- CORS enabled for the frontend dev origin (http://localhost:5173).
- Apply migrations automatically on startup in Development only.

## Domain model
Game (LoL, CS2, Valorant seeded), Tournament (tier, region, country, start/end dates),
Team (country, logo), Player (team, role, nickname),
Match (game, tournament, status: Scheduled/Live/Finished/Canceled, scheduledAtUtc, bestOf,
Team1Id, Team2Id (nullable for TBD), Team1Score, Team2Score, WinnerTeamId).
Every external entity has (Provider, ExternalId) with a unique index; all sync is an idempotent upsert.

## External data
- `IEsportsDataProvider` interface in Application; `PandaScoreProvider` implementation in Infrastructure (typed HttpClient + resilience/retry via Microsoft.Extensions.Http.Resilience, respect rate limits).
- Provider design must allow adding other providers later without touching other layers.
- Only the backend talks to external APIs. The frontend never does.

## Background sync (BackgroundService + PeriodicTimer + SyncState table with last success time)
- LiveSyncJob: every 60s, only live matches + matches starting within 30 min
- FixturesSyncJob: every 24h, upcoming tournaments & matches
- StatsSyncJob: every 7 days, teams, players, historical results/stats
- On startup, if a job's last success is older than its interval, run it immediately.
- Intervals configurable in appsettings. Failures are logged and never crash the app.
- Protected manual trigger: POST /api/admin/sync/{job} with X-Admin-Key header.

## API endpoints (REST, /api prefix)
GET /api/games
GET /api/matches/live?game=
GET /api/matches/upcoming?game=&from=&to=
GET /api/matches/{id}
GET /api/tournaments?game=&status=
GET /api/tournaments/{id}
GET /api/teams?game=&search=
GET /api/teams/{id}   (recent matches, win rate, roster)
GET /api/players/{id}

## Frontend rules
- Vue 3 Composition API with `<script setup lang="ts">`, Pinia, Vue Router, Tailwind CSS.
- API base URL from .env (VITE_API_BASE_URL). Typed API client layer in src/api.
- Pages: Home (live + next matches), Calendar, Tournament detail, Team detail, Player detail.
- Game filter chips (LoL, CS2, Valorant) stored in the URL query string.
- Live page polls the backend every 30s (SignalR may come later).
- UI text in Turkish. Display times in Europe/Istanbul timezone.
- Responsive and dark-theme friendly (esports look).

## Working rules
- Work in small steps; the project must build after every step.
- Do not add NuGet/npm packages outside this document without asking.
- Do not create files outside the requested scope.
- Keep .gitignore correct (bin, obj, node_modules, .env, secrets, *.user).