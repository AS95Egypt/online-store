# Application Conventions

> These conventions apply to the whole monorepo and must be followed by every agent and contributor. Detailed technical plans are produced by Squad Kit per story; these conventions are the shared rules those plans must respect.

---

## 1. Repository Structure

```javascript
/
  frontend/        # Angular application (customer storefront + admin UI)
  backend/         # ASP.NET Core Web API
  docs/            # Project documentation (this file, architecture decisions)
  squad-kit/       # Squad Kit configuration and generated plans
  README.md        # Root setup/run instructions
```

- Frontend and backend are separate, self-contained projects. Neither imports code from the other.
- Cross-app contracts (API shapes) are defined in stories and plans, and documented as they stabilize.

## 2. Tech Stack (fixed unless a story explicitly changes it)

| Layer | Technology |
| --- | --- |
| Frontend | Angular, TypeScript |
| Backend | ASP.NET Core Web API, C# |
| Database | SQL Server via EF Core |
| Auth | ASP.NET Core Identity + JWT (introduced in A2/A4) |
| Currency | Egyptian Pound (EGP), `decimal` only |

## 3. Business Rules Authority

- **The backend is the single authoritative source** for all business rules, validation, calculations, authorization, and data integrity.
- Frontend validation exists only to improve user experience and must never be the sole enforcement of a rule.
- Any conflict between a story and the project overview (`online store business rules`) must be flagged for clarification — never resolved by assumption.


## 4. Story Lifecycle

1. Story written in `/stories` using the story template (see `stories/A1-project-scaffolding-ci.md`).
2. Squad Kit generates the implementation plan.
3. Implementation follows the plan and these conventions.
4. Story is merged only when its acceptance criteria pass and CI is green.
5. Story status updated (`Ready → In Progress → Done`).

**Definition of Ready:** story has clear description, role responsibilities, acceptance criteria, and dependencies.
**Definition of Done:** all acceptance criteria verified, code reviewed/merged via PR, CI green, relevant docs updated.

## 5. Coding Conventions — Backend

- Enable nullable reference types; avoid null-forgiving operators without justification.
- Async all the way for I/O; async methods named with the `Async` suffix.
- Controllers stay thin: no business logic in controllers — delegate to services/application layer.
- All monetary values use `decimal` — never `double` or `float`.
- Money calculations, shipping fees, and totals are always computed on the backend.
- Data integrity enforced via EF Core configurations and DB constraints (unique indexes, required fields, check constraints) in addition to code validation.
- Use EF Core migrations for every schema change. Never edit the database by hand.

## 6. Coding Conventions — Frontend

- TypeScript in strict mode; no `any` without justification.
- Use Simple, Modern, Flat UI design and make it consistent through the entire app.
- Prefer standalone components and a feature-based folder structure (`features/<feature>/`).
- Presentational/container separation: components do not call APIs directly — use services.
- API communication centralized in `core/` services; no scattered `HttpClient` usage.
- Show loading, empty, and error states for every data-driven screen.
- Form validation mirrors backend rules for UX, but always relies on backend responses as the final word.
- Role-based UI visibility is a convenience only — the backend enforces authorization independently.
- 

## 7. API Conventions

- RESTful resources, plural nouns: `/api/products`, `/api/orders`.
- Consistent error response shape: `{ success: false, error: { code, message, details? } }`.
- Consistent success envelope where appropriate: `{ success: true, data, meta? }`.
- Pagination on all list endpoints: `page` (1-based, default `1`) and `pageSize` (default `20`, maximum `100`). Invalid or non-positive values use the defaults; values above the maximum are capped.
- Paginated success responses use `{ "success": true, "data": [...], "meta": { "total": 0, "page": 1, "pageSize": 20 } }`; `total` is the number of records matching the query before pagination.
- Authentication via `Authorization: Bearer` once introduced (A2).
- API changes that break the frontend must be made in the same story/PR that updates the frontend.

## 9. Security & Privacy

- Customers can only access their own data (profile, address, cart, orders) — enforced in the backend, tested per story.
- Admin operations require the Admin role; every admin endpoint is covered by an authorization test.
- No secrets in code or config files committed to the repo. Use user-secrets locally and environment variables.
- Validate and sanitize all user input server-side.

## 10. Documentation

- Root README kept current: how to build, run, and test both apps.
- Non-obvious decisions are recorded as short notes in `docs/` (date + decision + reason).
- Each story file records its final status when done.

## 11. Scope Discipline

- The MVP scope is defined in the project overview document. Out-of-scope features (online payments, guest checkout, variations, coupons, wishlist, reviews, refunds, multi-warehouse, POS/ERP, loyalty, AI recommendations, mobile apps) must not be built unless a story explicitly re-scopes them.
- Keep implementations simple. Avoid speculative abstractions; build for the story at hand.

---

*Maintained by the team; last updated: 2026-11-12.*