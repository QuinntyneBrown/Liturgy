# CI/CD Pipeline

The pipeline lives in [`.github/workflows/ci-cd.yml`](../.github/workflows/ci-cd.yml).

## What it does

| Trigger | Build & Test | Deploy |
| --- | --- | --- |
| Pull request → `main` | ✅ | ❌ |
| Push → `main` | ✅ | ✅ (only if tests pass) |

**Build & Test** (`build-and-test` job)

1. Restores, builds, and tests the .NET 9 solution (`backend/Liturgy.sln`) — both
   `Liturgy.UnitTests` and `Liturgy.IntegrationTests`. A **SQL Server 2022 service
   container** backs the integration tests; the job sets `LITURGY_TEST_SQLSERVER`
   so `AcceptanceSqlServer` connects to it instead of local SQLEXPRESS.
2. Installs the frontend (`npm ci`) and runs the Jest unit tests.
3. Builds the Angular app for production (entry emitted as `app.html`) and copies
   `frontend/dist/liturgy-app/browser/` into `backend/src/Liturgy.Api/wwwroot/`,
   then copies the static marketing site (`marketing/`) in beside it —
   `marketing/index.html` owns the web root. The model is described in
   [marketing-deployment.md](marketing-deployment.md).
4. `dotnet publish`es the API — which now embeds the SPA — and uploads it as the
   `app` artifact.

**Deploy** (`deploy` job) — runs only on a push to `main` after the tests are green.
It logs into Azure with a passwordless **OIDC federated credential** and deploys the
published bundle to a single **Azure App Service**.

> Playwright E2E tests (`npm run e2e`) are **not** part of the gate — they need a
> running app and browser binaries. Run them locally or add a dedicated job later.

## One-time Azure setup

### 1. Create the App Service

A Linux App Service on the .NET 9 runtime, e.g.:

```bash
az group create -n liturgy-rg -l eastus
az appservice plan create -g liturgy-rg -n liturgy-plan --sku B1 --is-linux
az webapp create -g liturgy-rg -p liturgy-plan -n <your-webapp-name> --runtime "DOTNETCORE:9.0"
```

### 2. Configure the database connection string

The API runs EF Core migrations against `ConnectionStrings:DefaultConnection` on
startup, so the App Service must have a reachable SQL database. Set it as a
**connection string** app setting (type `SQLAzure`):

```bash
az webapp config connection-string set -g liturgy-rg -n <your-webapp-name> \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:<sql-server>.database.windows.net,1433;Initial Catalog=<db>;Authentication=Active Directory Default;Encrypt=True;"
```

Also set the JWT settings the API requires (`Jwt:Issuer`, `Jwt:Audience`,
`Jwt:SigningKey`) and, since the SPA is served same-origin, optionally
`Cors:AllowedOrigins`:

```bash
az webapp config appsettings set -g liturgy-rg -n <your-webapp-name> --settings \
  Jwt__Issuer="https://<your-webapp-name>.azurewebsites.net" \
  Jwt__Audience="https://<your-webapp-name>.azurewebsites.net" \
  Jwt__SigningKey="<a-long-random-secret>"
```

### 3. Set up OIDC federated credentials (passwordless login)

Create an Entra ID app registration, give it a federated credential scoped to this
repo, and grant it Contributor on the App Service:

```bash
# App registration + service principal
az ad app create --display-name "liturgy-github-oidc"
APP_ID=$(az ad app list --display-name "liturgy-github-oidc" --query "[0].appId" -o tsv)
az ad sp create --id "$APP_ID"

# Federated credential — trust GitHub Actions on the main branch
az ad app federated-credential create --id "$APP_ID" --parameters '{
  "name": "liturgy-main",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:<owner>/<repo>:ref:refs/heads/main",
  "audiences": ["api://AzureADTokenExchange"]
}'

# Role assignment — Contributor on the web app
SUB=$(az account show --query id -o tsv)
az role assignment create --assignee "$APP_ID" --role Contributor \
  --scope "/subscriptions/$SUB/resourceGroups/liturgy-rg/providers/Microsoft.Web/sites/<your-webapp-name>"
```

> The `deploy` job also uses `environment: production`. If you protect that GitHub
> environment (e.g. add the deployment branch policy `main`), add the same
> `subject` for it, or add a second federated credential using
> `subject: repo:<owner>/<repo>:environment:production`.

## GitHub configuration

Add these under **Settings → Secrets and variables → Actions**.

### Secrets

| Name | Value |
| --- | --- |
| `AZURE_CLIENT_ID` | App registration (client) ID — `$APP_ID` above |
| `AZURE_TENANT_ID` | Entra tenant ID (`az account show --query tenantId -o tsv`) |
| `AZURE_SUBSCRIPTION_ID` | Subscription ID (`az account show --query id -o tsv`) |

### Variables

| Name | Value |
| --- | --- |
| `AZURE_WEBAPP_NAME` | The App Service name you created |
| `AZURE_WEBAPP_SLOT` | *(optional)* deployment slot; defaults to `production` |

Once these are in place, every push to `main` runs the tests and — if they pass —
deploys the bundled API + SPA to Azure.
