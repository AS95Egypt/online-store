# CLAUDE.md

This file gives Claude Code (claude.ai/code) guidance for working in this repository.

## Project status

**online-store** is a web app for a single-vendor business: customers browse products, add them to a cart, and check out.

The repo is **pre-scaffolding**. There is no application code yet, so there are no build, lint, or test commands. Right now it holds only `README.md`, `.gitignore`, and the `.squad/` planning workspace. Update this file once scaffolding (story A1) lands.

## Target architecture (from story A1, "Project Scaffolding")

This is the planned layout for the monorepo. It is defined in `.squad/stories/project-init/1/intake.md`, which comes from GitHub issue #1.

- `/frontend`: **Angular** app. Builds with `npm run build` and tests with `npm run test`. Folders are split into `core`, `shared`, `features`, and `layout`. It has dev and prod environment configs, and the root route serves a placeholder home page.
- `/backend`: **ASP.NET Core Web API**. Builds with `dotnet build` and tests with `dotnet test`. The solution is split into API, Application, Domain, and Infrastructure layers. It uses **EF Core with SQL Server** and has migrations set up. `GET /api/health` returns 200.
- `/docs`: shared docs, including `docs/app-conventions.md`. Linting and formatting rules should follow this file.
- CI runs on every PR, builds both apps, and blocks the merge on failure.
- The root `README.md` must document how to set up, build, and run both apps.

Constraints that story A1 sets:
- Keep the work to scaffolding only: no auth, catalog, cart, or orders.
- Don't add architecture beyond the baseline structure.
- Skip tests for now. A later story covers them.
- Docker and cloud deployment are out of scope.

`.squad/config.yaml` lists `primaryLanguage: typescript`, but the backend is C#/.NET.

## Planned feature roadmap

Good first tickets are listed in `.squad/knowledge/overview.md`:
1. **OS-1**: Browse and search the product catalogue. Its stage is `specs`. Open analyst/architect questions are in `.squad/tickets/OS-1/questions.md`: who can browse, where product data comes from, which filters and sorts to support, which fields each product shows, and how large the catalogue will be.
2. Add products to a cart and review it.
3. Place an order at checkout.

## squad-kit workflow (`.squad/`)

Planning is managed by [squad-kit](https://github.com/AzmSquad/squad-kit). The tracker is GitHub (`AS95Egypt/online-store`), and the board key is `OS`.

- `stories/<feature>/<id>/intake.md` holds the story input that the planner reads verbatim. Create one with `squad new-story <feature-slug>`.
- `plans/<feature>/` holds the generated plans. `00-overview.md` indexes the `NN-story-*.md` files, which run in `NN` order. Generate them with `/squad-plan <intake-path>` or `squad new-plan <intake-path>`.
- Implement each story in a fresh, scoped session that has **only** its `NN-story-*.md` attached.
- `tickets/OS-N/` holds board tickets (`ticket.md`) and their open questions (`questions.md`).
- `knowledge/` contains the project overview and summary docs.
- `rules/{analyst,architect,builder,reviewer,tester}.md` hold per-role instructions. They are currently empty templates.
- `.squad/secrets.yaml` is gitignored. Don't read it, print it, or commit it. Leave the squad-kit-managed block in `.gitignore` unchanged.
