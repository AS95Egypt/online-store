# online-store

This repository contains a single-vendor online store where customers can browse products, add them to a cart, and complete an order checkout flow.

## Repository layout

- `/frontend` — Angular application shell and future storefront UI.
- `/backend` — ASP.NET Core Web API with layered .NET architecture.
- `/docs` — shared project conventions and repo guidance.
- `/.squad` — squad-kit planning and project tracking metadata.

## Prerequisites

- Node.js LTS and npm
- .NET SDK 8.0.423 (see `backend/global.json`)
- SQL Server local instance or a Dockerized SQL Server instance for future persistence work
- No live database is required to build or run the scaffolded app

## Backend setup, build, run, and test

```bash
cd backend
dotnet restore
dotnet build
dotnet run --project src/OnlineStore.Api
dotnet test
```

The API exposes a health check at `GET /api/health` and returns a `200 OK` JSON payload with the application status.

Local database tooling note: if you plan to work with EF Core migrations later, install the CLI once with:

```bash
dotnet tool install --global dotnet-ef
```

## Frontend setup, build, run, and test

```bash
cd frontend
npm install
npm run build
npm start
npm run test
```

The Angular app starts on the default development port and opens the public product catalog at the home route.

## Authentication (local dev)

Use local user secrets for JWT configuration before running the API:

```bash
cd backend
dotnet user-secrets init --project src/OnlineStore.Api
dotnet user-secrets set "Jwt:Secret" "replace-with-a-local-secret" --project src/OnlineStore.Api
dotnet user-secrets set "Jwt:Issuer" "online-store-local" --project src/OnlineStore.Api
dotnet user-secrets set "Jwt:Audience" "online-store-client" --project src/OnlineStore.Api
dotnet user-secrets set "Jwt:ExpiryMinutes" "60" --project src/OnlineStore.Api
```

The auth flow uses ASP.NET Core password hashing with JWT. The initial EF Core migration creates two application tables: `Users` and `PasswordResetOtps`. EF Core also creates its internal `__EFMigrationsHistory` table to track applied migrations. `Users` stores the name, email, password hash, and role; roles are constrained to `ADMIN` or `CUSTOMER`, and public registration always assigns `CUSTOMER`. Password reset OTPs reference their user and are deleted when that user is deleted.

Apply the database migration from the `backend` directory with:

```bash
dotnet ef database update --project src/OnlineStore.Infrastructure --startup-project src/OnlineStore.Api
```

`POST /api/auth/register`, `/login`, `/forgot-password`, `/verify-otp`, and `/reset-password` are available from Swagger. OTPs are logged to the console in development through `LoggingEmailSender`, and logout is intentionally client-side token discard because JWT revocation is out of scope for this story.

Authenticated customers can use `GET /api/customers/me` to view their profile, `PUT /api/customers/me` to update their name, and `POST /api/customers/me/change-password` to change their password. These endpoints identify the customer from the JWT; email is read-only.

### Product catalog

The public `GET /api/products` endpoint returns active products in the paginated response format defined in [app-conventions.md](docs/app-conventions.md). After applying EF migrations, run the API in Development to seed sample brands, categories, and products (including inactive and out-of-stock examples). The seeder fills missing reference tables independently and skips adding sample products when products already exist.

Cloudinary upload credentials and upload UI are deferred to the admin product-management story. Product image upload is not part of this catalog listing story.

### Seeding a development admin account

The API seeds one configured admin account only when running in the Development environment. It creates the user only if the configured email does not already exist. If either setting is missing, seeding is skipped. Configure credentials with user-secrets from the `backend` directory:

```bash
dotnet user-secrets set "Seed:AdminEmail" "admin@example.com" --project src/OnlineStore.Api
dotnet user-secrets set "Seed:AdminPassword" "replace-with-a-strong-local-password" --project src/OnlineStore.Api
```

Never commit real admin credentials. The admin role is not accepted by public registration; new registrations are always customers. `GET /api/admin/ping` is a role-protected verification endpoint. Admin API authorization is enforced on the backend, while the frontend role guard only controls navigation and visibility.
