# Simple Survival Budget Tracker

A minimal, self-hosted daily budget app focused on one question:

> How much can I safely spend today until salary arrives?

The MVP includes a mobile-first SvelteKit dashboard, an ASP.NET Core Web API, JWT-backed app sessions, generic OIDC login, EF Core with SQLite, editable transactions, adaptive daily allowance calculations, and Docker Compose deployment.

## Repository layout

```text
root/
├── frontend/          # SvelteKit + TypeScript + TailwindCSS
├── backend/           # ASP.NET Core 10 Web API + EF Core + SQLite
├── docker-compose.yml # Self-hosted deployment stack
├── docs/              # Implementation notes
└── README.md
```

## Budget formula

```text
Daily Budget = Remaining Money / Remaining Days
```

Expenses reduce the remaining money and lower future allowance. Income increases the remaining money and raises future allowance. Editing or deleting a transaction recomputes the dashboard on the next API response.

## Local development

### Frontend

```bash
cd frontend
npm install
npm run dev
```

### Backend

The backend targets `net10.0` and uses SQLite. With the .NET 10 SDK installed:

```bash
cd backend
dotnet restore
dotnet run
```

## Configuration

Copy `.env.example` to `.env` for Docker Compose. Important values:

| Variable | Purpose |
| --- | --- |
| `OIDC_AUTHORITY` | Base URL for Authentik, Keycloak, or another OIDC provider |
| `OIDC_CLIENT_ID` | Public OIDC client ID used by the frontend PKCE login |
| `OIDC_CLIENT_SECRET` | Optional secret for confidential clients |
| `JWT_SIGNING_KEY` | Local app session signing key; use at least 32 characters |
| `FRONTEND_ORIGIN` | CORS origin allowed by the backend |
| `API_BASE` | Runtime API URL used by the frontend container |

The OIDC redirect URI is:

```text
<frontend-origin>/auth/callback
```


## Dockerfiles

The repository includes separate production Dockerfiles for each service:

- `backend/Dockerfile` follows the ASP.NET multi-stage pattern: `aspnet:10.0` runtime base, `sdk:10.0` restore/build/publish stages, `BUILD_CONFIGURATION` build arg, port `5000`, and `dotnet DailyBudget.Api.dll` entrypoint.
- `frontend/Dockerfile` builds the SvelteKit static bundle with Node and serves it with nginx on port `80`; `frontend/docker-entrypoint.sh` writes `/config.json` from runtime environment variables so the same frontend image can be reused across environments.

## Docker Compose

```bash
docker compose up --build
```

SQLite data is persisted in the `daily-budget-data` Docker volume at `/data/daily-budget.db` inside the backend container.
