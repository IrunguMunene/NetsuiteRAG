# Tasks & User Stories — v3.1 (Aligned to PRD v3.1)
**Date:** October 05, 2025

## Legend
- **ID format:** T<Phase>.<Sequence> (single-purpose, strictly ordered)
- Each task lists **Inputs → Work → Outputs → Acceptance**.
- User stories map to tasks where relevant.

---

## Phase 0 — Foundations
**US-OPS-01:** As an engineer, I need feature flags to safely roll out new capabilities.

- **T0.01 Feature Flag Service bootstrap**  
  Inputs: repo, envs.  
  Work: add flag SDK, config by tenant/role/user; kill switches; audit.  
  Outputs: /flags endpoint; admin config.  
  Acceptance: flags toggle in runtime; audit entries produced.

- **T0.02 Observability baseline**  
  Work: OpenTelemetry traces; logs with queryId; dashboards skeleton.  
  Acceptance: traces for NL→plan→exec; error rate and latency charts exist.

- **T0.03 CI scaffolding**  
  Work: GitHub Actions; unit tests min 70%; artifact uploads.  
  Acceptance: PRs run tests; artifacts retained 7 days.

---

## Phase 1 — RAG Corpus & Schema
**US-ADM-01:** As admin, I want custom fields auto-discovered and enriched.

- **T1.01 Field dictionaries (standard)**  
  Outputs: JSON dictionaries per record type; operator matrix.

- **T1.02 Custom Field Crawler (daily)**  
  Outputs: descriptors table; change log.  
  Acceptance: discovers, updates, flags stale 90+ days.

- **T1.03 Descriptor Enrichment**  
  Work: aliases, business context, ambiguity score; embeddings.  
  Acceptance: 95% descriptors enriched; vectorized.

- **T1.04 Business glossary + exemplars (30-50)**  
  Outputs: curated text; embeddings.  
  Acceptance: retrieved in planning context by keywords.

- **T1.05 Index artifacts to vector DB**  
  Acceptance: search returns top-5 relevant in <200ms P95.

---

## Phase 2 — Planning & Validation
**US-FA-08:** As analyst, I want early detection of illogical queries.

- **T2.01 SavedSearchPlan schema (strict)**  
  Acceptance: JSON-schema validation with clear errors.

- **T2.02 Layer 1–4 validators**  
  Acceptance: unit tests for error cases; detailed messages.

- **T2.03 **Semantic Validator** (Layer 5)**  
  Inputs: YAML rules (error/warn), fiscal calendar.  
  Outputs: 400 errors or warn-with-override.  
  Acceptance: intercept ≥80% empties; false-positive <5%.

- **T2.04 Name→ID resolvers & ambiguity detection**  
  Acceptance: 409 payload shapes match PRD; prefs persisted.

- **T2.05 Dry-run mode**  
  Acceptance: `?dryRun=true` returns validated plan only.

- **T2.06 Guardrail telemetry + weekly review job**  
  Outputs: report of top violations; ticket suggestions.

---

## Phase 4 — Execute, Stream, Export (moved earlier)
**US-FA-01:** As analyst, I need fast first page and stable streaming.

- **T4.01 SuiteScript/REST adapters**  
  Acceptance: run paged searches @1000 rows.

- **T4.02 SSE streaming + backpressure + resume**  
  Acceptance: buffer=5; resume from checkpoint on reconnect.

- **T4.03 Export tier engine + real-time XLSX**  
  Acceptance: tier selection; <50K returns in <30s.

- **T4.04 Async export jobs + email**  
  Acceptance: status endpoint; link expiry 7 days.

- **T4.05 Retry + Circuit breaker**  
  Acceptance: backoff, jitter; open/half-open/closed flows logged.

- **T4.06 Redis **Metadata Cache** (read-through + warmers)**  
  Acceptance: lookups <10ms P95; NS metadata calls reduced ≥60%.

---

## Phase 6 — UX & UAT
**US-FA-02:** As analyst, I want helpful disambiguation with sensible defaults.

- **T6.01 Query console + history**  
  Acceptance: 50 saved entries per user.

- **T6.02 Disambiguation dialogs (decision matrix)**  
  Acceptance: auto-fill strong pref; confirm boundary cases.

- **T6.03 Virtualized results table**  
  Acceptance: smooth @100K rows; sorting/filters client-side.

- **T6.04 Export dialog (tier-aware)**  
  Acceptance: text changes per tier; disabled while streaming.

- **T6.05 Feedback widget + reasons**  
  Acceptance: thumbs + reason codes stored.

- **T6.06 UAT run (3–4 weeks)**  
  Acceptance: ≥90% first-try, ≥95% post-disambig; ≥4/5 CSAT.

---

## Phase 3 — Template Reuse (post-MVP)
**US-FM-03:** As manager, I need reliable, approved report templates.

- **T3.01 Canonical template format (structure vs slots)**  
  Acceptance: JSON schema published; examples.

- **T3.02 Dual-key retrieval (semantic + structural)**  
  Acceptance: composite score calc; threshold 0.75.

- **T3.03 Shadow path (10%)**  
  Acceptance: logged comparisons; user sees cached result.

- **T3.04 Rejection feedback weightings**  
  Acceptance: penalties & decay modeled; flags after 5 rejections.

- **T3.05 Template Registry (state + audit)**  
  Acceptance: lifecycle states; immutable audit log.

- **T3.06 Approval workflow & notifications**  
  Acceptance: reviewer queue; SLA breaches alerted.

- **T3.07 Template Confidence & **Quarantine** UX**  
  Acceptance: badges; force “Plan Fresh”; auto-quarantine logic.

- **T3.08 Governance dashboards & weekly digest**  
  Acceptance: trend charts; top actions.

---

## Phase 5 — Composition (staged)
**US-FA-12:** As analyst, I want combined results across subreports.

- **T5.01 Composition schema**  
  Acceptance: subtemplates, join keys, grain, post-calcs.

- **T5.02 5a: Parallel execution**  
  Acceptance: side-by-side results; independent progress.

- **T5.03 5b: Simple joins in DuckDB**  
  Acceptance: join on exact keys; normalized grain.

- **T5.04 5c (Deferred): Advanced composition**  
  Acceptance (later): grain mismatch, dimension history, derived metrics.

- **T5.05 Composition template caching**  
  Acceptance: references to subtemplate IDs; reuse when filters differ.

---

## Phase 7–9 — Quality, Ops, Cost, Attribution
- **T7.01 CI **Performance Gate** (k6)** → block on regressions.  
- **T7.02 Decision Attribution pipeline** → artifact influence metrics & alerts.  
- **T7.03 Cost Metering service** → tag & store usage per tenant/user.  
- **T7.04 Cost dashboards & exports** → chargeback CSV monthly.  
- **T7.05 Quotas & throttling** → per-user/tenant; 429s with reset ETA.  
- **T7.06 Template health monitor** → TTL, schema validation, staleness.  
- **T7.07 Security/PII workflows** → approvals, watermarking, notifications.  
- **T7.08 Runbooks & on-call** → circuit breaker, degraded-mode, cache flush.  

## Story→Task Traceability
- US-FA-08 → T2.03, T2.06  
- US-ADM-01 → T1.02, T1.03  
- US-FM-03 → T3.05–T3.08  
- US-OPS-01 → T0.01–T0.03, T7.01–T7.05
