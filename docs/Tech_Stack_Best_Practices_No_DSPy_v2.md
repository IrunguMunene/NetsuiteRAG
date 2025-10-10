
# Tech-Stack Best Practices (No DSPy) — **v2 with Code Standards**
**Version:** 2.0  
**Generated:** 2025-10-08 00:39 UTC

This document extends the stack practices with **code best practices for Angular 20+ and C#/.NET**, including SOLID, DRY, KISS, YAGNI, and concrete engineering guardrails.

It builds on the earlier best-practices (v1); the original sections (1–13) are preserved and a new **Section 14** is added.

---

## 1) LLM Planning & Orchestration (.NET, SK, Microsoft.Extensions.AI)
- **Single-responsibility planners:** Split NL→Plan into small steps: (a) intent/record-type, (b) field mapping, (c) join selection, (d) operator/filters, (e) disambiguation prompts, (f) composition trigger (optional).
- **Top-k candidate planning:** Generate 3–5 candidate plans at **low temperature (0.2–0.3)**; pick with a **deterministic re-ranker** (penalize illegal fields/joins, guardrail misses, excessive joins).
- **JSON-only outputs:** Use **grammar/JSON schema constraints** (Semantic Kernel function calling or strict JSON templates). Reject any plan that doesn’t parse.
- **Deterministic repair pass:** Before execution, run a **fast “plan-repair”** step in C#: fix operator–type mismatches, inject required guardrails (posting/period/subsidiary), and drop non-catalog fields.
- **Short, bounded context:** Keep prompts tight. Provide only the **catalog slice** (allowed filters/cols/joins, operator matrix, fiscal settings) relevant to the inferred record type and tenant.
- **Prompt packs as code:** Version prompts and exemplars alongside code (Git), with feature flags to swap variants at runtime.
- **Time-box models:** Enforce per-call timeouts (e.g., 5s) and a max token budget; surface graceful errors to the UI.

## 2) Validation & Guardrails (4 Layers)
- **Layer 1 – JSON schema:** Validate shape and required keys; return structured 400 errors (no hidden 500s).
- **Layer 2 – NetSuite schema check:** Verify fields exist and joins are legal **per your catalog** (including custom field descriptors).
- **Layer 3 – Operator matrix:** Enforce operator–type compatibility (date: within/on/before/after; list: anyof/noneof; number: equals/>, <; text: contains/startsWith).
- **Layer 4 – Governance guardrails:** Inject/require `posting=true`, **bounded period** (≤5 yrs), **subsidiary scope**, column cap (≤50), hard row cap (2M), **canary LIMIT 10** sanity run when risky.

## 3) Catalog Adherence (Joins/Fields/Filters)
- **Join catalog is law:** Treat the catalog (joins, filters, columns) as the **authoritative contract**; planners only propose within it.
- **Base-first resolution:** Prefer a base record that naturally contains most requested fields (often `transaction`), then **minimal join set** to cover the rest.
- **Set-cover join selection (deterministic):** Given fields needed, compute smallest legal join set from the catalog; if multiple, pick lowest fan-out / most stable paths (e.g., `entity→customer` before exotic joins).
- **Subtype scoping:** Map phrases like “sales orders”, “vendor bills”, “credit memos” to `type anyof (…)` early to avoid wide scans.

## 4) Custom Field Handling (No-Drama Mode)
- **Daily discovery:** Crawl & cache custom field descriptors (IDs, labels, types, options, aliases, sensitivity).
- **Preference by success:** When synonyms map to multiple custom fields, prefer the one with **higher historical success rate** and recent usage.
- **Strict operator rules:** Use list operators for list-type custom fields; never “contains” on picklists.
- **Real-time fallback:** If a referenced custom field is missing in cache, **validate once** against NetSuite metadata; on success, backfill cache asynchronously.

## 5) Name→ID Resolvers (High-Precision)
- **Hybrid matching:** exact → normalized (trim/case/fuzzy) → synonyms/aliases; require **confidence ≥0.9** else raise disambiguation with top-5 candidates.
- **Tenant-scoped caches:** Cache entity/account/subsidiary lookups with **TTL + ETag** based invalidation (detect renames).
- **Locale-aware periods:** Resolve “last quarter”, “FYTD” using **NetSuite fiscal calendar**, not Gregorian by default.

## 6) Template Reuse (Vector Cache, No DSPy)
- **Canonical templates:** Separate **structure** (cols/summaries/sort/joins) from **slot filters** (period, subsidiary, entity).
- **Dual-key retrieval:** Semantic similarity (embedding) **and** structural compatibility (required slots match, column Jaccard, summary alignment).
- **Shadow path:** 10% of traffic builds fresh plans silently for quality measurement; user still gets the best known template.
- **Versioning + TTL:** New version if structure changes; auto-archive unused templates after 180 days (restoreable).

## 7) Angular Frontend (20+)
- **SSE streaming + backpressure:** Use EventSource for streaming pages; **acknowledge per page** and pause server when buffer is full.
- **Virtualized grid:** AG Grid/Angular CDK Virtual Scroll; fixed row height; client-side sort/filter on fetched subset; warn if partial sort.
- **Disambiguation UX:** Decision matrix: auto-fill high-confidence defaults; confirm on high-impact overrides; persist preferences with **context** (fiscal year, report family) and expiry.
- **Tiered export UX:** <50K synchronous XLSX; 50K–500K async XLSX (email); >500K CSV or partitioned Excel (ZIP). Watermark with user/timestamp/query hash.

## 8) Execution & Resiliency (NetSuite)
- **SuiteScript RESTlet preferred:** Faster paging & governance visibility; fall back to REST Saved Search; last resort SOAP.
- **Retries & circuit breaker:** Exponential backoff (2s/4s/8s), max 3 tries; open breaker on repeated failures; show user friendly status + ETA.
- **Checkpoint resume:** If client drops, resume from last acked page on reconnect (queryId-based).
- **Governance telemetry:** Track usage units, timeouts, complex-search errors; auto-suggest filter tightening on repeated hits.

## 9) Security & Compliance
- **RBAC parity:** Execute **only** under least-privileged integration roles; enforce subsidiary scopes server-side every time.
- **PII classification:** Maintain sensitivity map (Public/Internal/Confidential/Restricted); require manager approval & watermarking for Restricted fields/exports.
- **Immutable audit:** Log NL query, plan, disambiguations, row counts, exports; append-only with signatures; retain 7 years.

## 10) Observability & Quality
- **KPIs:** first-try accuracy, post-disambiguation accuracy, validation failure rate (by layer), template reuse rate, median join count.
- **Golden question regression:** Nightly replay of 50–100 queries; flag drifts; block release on accuracy regressions.
- **Structured errors:** Always return layer + remediation; never generic 500s for plan problems.
- **Prompt pack A/B:** Toggle via feature flags; keep a **known-good** and a **candidate** pack; roll back instantly.

## 11) Performance & Cost
- **Latency budgets:** Planning <1s P95; first page 2–4s P95; per-page 1–2s P95; template match <200ms P95.
- **Token hygiene:** Short prompts; catalog slices; stop words removed; minimize verbose exemplars.
- **Embeddings:** Use small, fast local embedding model (e.g., `bge-small`, `all-MiniLM`) for templates/lexicon; batch inserts; compress vectors if supported.

## 12) CI/CD & Config (.NET Aspire)
- **Aspire resources:** Define Qdrant/Postgres/Redis/RESTlet endpoints as **Aspire resources**; health checks; secrets via user-secrets or Vault.
- **Blue/green prompt deploys:** Ship prompt packs and catalogs as versioned artifacts; roll forward with flags.
- **Contract tests:** Validate RESTlet/SOAP adapters against a sandbox with known searches (pinned IDs).
- **Seed data:** Include a default golden-question set and a demo template set to validate new envs fast.

## 13) Anti-Patterns to Avoid
- Letting the LLM “discover” fields or joins outside the catalog.
- Using `contains` on picklists or dates; using `within` with a single date.
- Running huge exports synchronously (browser death spiral).
- Skipping canary LIMIT checks on risky plans.
- Storing prompts in the database without version control.
- Silent catch/retry loops that hide poor accuracy or planner drift.

---

## 14) **Code Best Practices: Angular 20+ & C#/.NET**

### 14.1 Universal Engineering Principles
- **SOLID:** Single Responsibility (services/components do one thing), Open/Closed (extend via interfaces/providers), Liskov (subtypes don’t narrow contracts), Interface Segregation (small, focused interfaces), Dependency Inversion (depend on abstractions, not concretions).
- **DRY:** Share logic via services/utilities; avoid duplicated RxJS chains or LINQ projections; extract reusable pipes/functions.
- **KISS:** Prefer simple, explicit flows; avoid meta-framework rabbit holes (e.g., over-abstracted state, generic factories everywhere).
- **YAGNI:** Don’t add global state, caches, or composition engines until usage data justifies them.
- **Clean Architecture:** API (controllers/minimal endpoints) → Application (use-cases) → Domain (entities/specifications) → Infrastructure (adapters). Boundaries are explicit; domain is free of framework code.
- **Functional Core, Imperative Shell:** Pure, testable functions for transforms (mapping rows, computing filters); side effects (HTTP, IO) at the edges.
- **12‑Factor Config:** Use environment-based config, secrets vaults, and typed options with validation.

---

### 14.2 Angular 20+ Code Standards

#### Project & Module Structure
- **Standalone components** (no NgModules); feature-based folders: `/features`, `/shared`, `/core` (singleton services only).
- **Routing:** Route-level **lazy loading**; meaningful URLs; guards for auth/role; **resolver** only when prefetch critical data.
- **Change detection:** Default to **OnPush**; prefer **Signals** for local state; avoid heavy two-way bindings.
- **State management:** Start simple with **component signals + services**; graduate to **ComponentStore** or **NgRx** only when (a) multiple features share state, (b) undo/redo or devtools are needed.
- **HTTP layer:** Central `ApiClient` service; **interceptors** for auth headers, correlation ID, retry (idempotent only), and error normalization.
- **SSE client:** Wrap `EventSource` in a service returning an Observable/Signal; handle reconnect/backoff; surface typed events.

#### RxJS/Signals Practices
- **Favor `switchMap`** for query refresh; **`exhaustMap`** for “click → export” (avoid duplicates); **`concatMap`** for ordered side-effects.
- **Never nest `subscribe`**; compose with operators; in components use `async` pipe or `toSignal()`.
- **Lifecycle cleanup:** `takeUntilDestroyed()` or `DestroyRef`; avoid manual `Subject` cleanup.
- **Backpressure:** Debounce fast inputs; throttle infinite scroll; batch UI updates with `effect()`.

#### Components & Templates
- **Inputs/Outputs:** Narrow, typed inputs; prefer immutable inputs; avoid optional chains in hot paths.
- **Lists:** Always set `trackBy`; virtualize large tables (AG Grid/Angular CDK).
- **Pipes:** Prefer **pure pipes**; avoid heavy computation in templates.
- **Accessibility:** ARIA labels; keyboard nav; color contrast; focus management after dialogs/errors.

#### Testing & Quality
- **Unit tests:** Jest or Angular test runner; **Testing Library** for DOM; target 70–80% coverage where it matters.
- **Component tests:** Focus on behavior (rendered rows/events), not internals; mock HTTP/SSE.
- **E2E:** Playwright; cover query→disambiguation→streaming→export flows.
- **Linting/format:** `angular-eslint` + Prettier; Husky pre-commit hooks (lint, test);
  commit messages via **commitlint** (Conventional Commits).

#### Performance & Security
- **Performance:** Route-level code-splitting; `optimizeImage`; preconnect to API; avoid global event listeners; use **`runOutsideAngular`** for heavy non-UI loops.
- **Security:** Strict TSC; **DomSanitizer** only when unavoidable; never `bypassSecurityTrust…` for untrusted content; CSP headers; audit 3rd‑party libs.

---

### 14.3 C#/.NET Code Standards (ASP.NET Core, SK, Aspire)

#### Project Layout
- **Sane layers:** `Api` (endpoints), `Application` (handlers/use-cases), `Domain` (entities/specs), `Infrastructure` (EF/Dapper, NetSuite adapters, Qdrant clients), `Contracts` (DTOs).
- **Dependency Injection:** Register abstractions in `Application`; concrete adapters in `Infrastructure`. Keep controllers thin—delegate to use-cases.
- **Options pattern:** Strongly typed options + **DataAnnotations** validation; fail fast on invalid config.

#### SOLID Applied
- **SRP:** Each use‑case/handler performs one business action (e.g., `PlanSavedSearch`, `ExecutePagedSearch`).
- **Open/Closed:** Add new record-type planners via new classes implementing `IPlanStep` without touching existing ones.
- **Liskov:** Interfaces should not force unsupported operations (split read vs write repos).
- **ISP:** Split big “service” interfaces into smaller per‑capability interfaces.
- **DIP:** Depend on `INetSuiteSearchClient`, `IVectorStore`, not concrete SDKs.

#### API & Streaming
- **Minimal APIs or Controllers:** Keep endpoints small and composable; use **problem details** (RFC7807) for errors.
- **SSE endpoints:** Set `text/event-stream`, disable response buffering, **flush per event**, honor **CancellationToken**; use `Channel<T>` for backpressure.
- **IAsyncEnumerable<T>:** Stream pages with `await foreach`; never buffer entire result sets in memory.

#### Resilience & Networking
- **HttpClientFactory + Polly:** Timeouts, retries (idempotent only), circuit breakers, bulkheads; per-endpoint policies.
- **Rate limiting middleware:** Per-user and per-tenant quotas; 429 responses with reset hints.
- **Serilog + OpenTelemetry:** Structured logs with correlation IDs; traces/spans across NetSuite, vector DB, and exports.

#### Data & Performance
- **EF Core vs Dapper:** Use **EF Core** for metadata/config (migrations, relations); **Dapper** for hot paths or bulk inserts (telemetry logs).
- **Async all the way:** Avoid sync-over-async; propagate `CancellationToken` everywhere.
- **Pooling:** Reuse DB connections; beware large object heap (avoid `MemoryStream` growth loops).

#### Error Handling & Validation
- **FluentValidation** or custom validators for DTOs; return **400s with field-level errors**.
- **Guard clauses** for null/empty inputs; throw domain-specific exceptions; map to HTTP problem details.
- **No swallowed exceptions:** Always log with context; rethrow preserving stack trace when needed.

#### Testing
- **Unit tests:** xUnit; Moq/NSubstitute; cover planners, validators, resolvers.
- **Integration tests:** `WebApplicationFactory` for endpoints; **Testcontainers** for Postgres/Qdrant/Redis.
- **Contract tests:** Golden searches in a NetSuite sandbox; verify paging/joins/filters.
- **Load tests:** k6/Gatling against streaming endpoints.

#### Code Quality & Tooling
- **Analyzers:** .NET analyzers + **StyleCop.Analyzers**; warnings as errors in CI.
- **Formatting:** `dotnet format` gate; EditorConfig at repo root; nullable enabled.
- **Source generators (optional):** For DTO mappers or SDK clients where beneficial.
- **Docs:** XML summaries on public APIs; architecture decision records (ADRs) checked in.

#### Security
- **AuthN/Z:** SSO (OIDC/SAML) integration; enforce role/subsidiary scope server‑side; claims → policies mapping.
- **Secrets:** Never in code; use Aspire secret stores or Vault; rotate regularly.
- **PII:** Central sensitivity map; approval workflow for Restricted; mask in UI; watermark exports.

---

### 14.4 Review Checklists (Copy/Paste for PRs)

**Angular PR Checklist**
- [ ] Standalone component with OnPush/Signals
- [ ] No nested `subscribe`; proper cleanup with `takeUntilDestroyed()`
- [ ] `trackBy` on `*ngFor`; virtualized tables
- [ ] Interceptors used for cross-cutting concerns
- [ ] Accessibility (labels, focus, contrast)
- [ ] Unit tests + E2E updated; ESLint/Prettier pass

**.NET PR Checklist**
- [ ] Endpoint returns ProblemDetails on error
- [ ] Use-cases are SRP and async w/ CancellationToken
- [ ] HttpClientFactory + Polly policies applied
- [ ] Structured logs with correlation IDs; OTel spans present
- [ ] Validation + guardrails; no unchecked nulls
- [ ] Tests (unit/integration) pass; analyzers clean

---

## Outcome Targets (No DSPy)
- **First-try accuracy:** ~88–92% with top-k + re-rank/repair + resolvers + catalog adherence.
- **Post-disambiguation accuracy:** ≥95%.
- **Governance:** All searches remain Saved Search–legal, within join/operator rules, and enforce RBAC.

