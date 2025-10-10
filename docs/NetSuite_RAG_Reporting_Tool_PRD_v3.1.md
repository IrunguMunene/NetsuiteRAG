# NetSuite RAG Reporting Tool — PRD v3.1
**Date:** October 05, 2025  
**Status:** Final for Implementation (Incorporates external recommendations)  
**Classification:** Internal Use Only

---

## 0. Change Log (v3 → v3.1)
- **Phase resequencing:** Execute & Paging (Phase 4) moved before Template Reuse (Phase 3); Composition 5c deferred; 5a/5b staged with 6–8 week budget.
- **Validation expanded to 4+1 layers:** JSON → NetSuite Schema → Operator Compatibility → Guardrails → **Semantic Query Validation (new)** with error vs. override flows.
- **Custom fields (FR-11/12) consolidated:** Daily discovery + Admin dashboard; descriptors enriched and included in RAG.
- **Disambiguation decision matrix:** Explicit heuristics & 409 flow; preferences are versioned and time-scoped.
- **Template governance:** State machine (Draft→Pending Review→Approved→Active→Deprecated→Archived), ownership, versioning, quality scoring & quarantine.
- **Metadata cache:** Redis read-through + warmers; stale-while-revalidate; degraded-mode policy.
- **Performance as a gate:** CI load tests (k6) with budgets; PRs fail on P95 regressions >20%.
- **Cost metering & quotas:** Multi-tenant consumption tracking; dashboards; soft/hard limits & alerts.
- **LLM decision attribution:** Measure which RAG artifacts influenced planning; dashboards & alerts.
- **Template trust UX:** Confidence badges, success rate, last schema validation; force-replan button.
- **Feature flags:** Progressive rollout, canaries, kill switches.
- **API failure handling:** Exponential backoff, circuit breaker, resume checkpoints >100K rows.
- **PII program:** Field classification, approval workflows for Restricted, watermarking & notifications.
- **Composition plan storage:** Composition templates reference subtemplates, join keys, grain.

---

## 1. Executive Summary
Governed, natural-language reporting on NetSuite via dynamic Saved Searches, with strong RBAC, auditable planning, and accuracy-first reuse. v3.1 adds semantic validation, governance for templates, performance gates, and cost/observability controls to guarantee trustworthy outputs at scale.

## 2. Personas
(unchanged; expanded admin goals for template lifecycle, quarantine review, and cost oversight).

---

## 3. Functional Requirements (additions)
### FR-13: **Semantic Query Validation (Pre-Execution)**
**Priority:** P0  
**Goal:** Catch business-logic contradictions before NetSuite calls.

**Examples (Error vs. Warning):**
- Error: End date < start date; period outside fiscal years loaded; invalid subsidiary+entity combo.
- Warning (override allowed): “Last quarter” in 7-day boundary window; wide open date with >10 columns; empty-result predictors.

**Behavior:**
- Errors → HTTP 400 with remediation and “Edit query” CTA.
- Warnings → Modal: “Proceed anyway?” with explanation; log override.

**Acceptance:**
- ≥80% of previously empty/illogical queries intercepted; false-positive rate <5%.

### FR-14: **Template Governance & Lifecycle**
**Priority:** P1  
States: **Draft → Pending Review → Approved → Active → Deprecated → Archived**.  
Transitions: creator→reviewer→publish; auto-deprecate on schema break or low score; quarantine if score < threshold or repeated rejections.

**Acceptance:**
- 100% template changes audited; rollback < 1 minute; quarantine auto-applies within 1 hour of trigger.

### FR-15: **Metadata Cache (Redis)**
**Priority:** P0  
Caches: field lists, options, fiscal calendar, subsidiary/account dictionaries.  
Policies: read-through; TTL (24h default), SWR (5m), degraded-mode if NetSuite unavailable.

**Acceptance:**
- Validation lookups <10ms P95; NetSuite metadata calls reduced ≥60%.

### FR-16: **Performance CI Gate**
**Priority:** P0  
k6 suite runs on PR with synthetic data; block merge if P95 latencies regress >20% on: planning, template lookup, first-page, per-page, export tier selection.

**Acceptance:**
- CI posts perf report artifact; red builds block merge.

### FR-17: **Cost Metering & Quotas**
**Priority:** P1  
Meter NetSuite usage units, LLM tokens, storage/egress per tenant/user; soft warnings at 80%; HTTP 429 at 100% with reset ETA.

**Acceptance:**
- Cost dashboard live; monthly chargeback CSV; false-positive <2%.

### FR-18: **LLM Decision Attribution**
**Priority:** P1  
Emit which artifacts contributed (field dicts, custom descriptors, exemplars, glossary). Alert when effectiveness drops >10% WoW.

### FR-19: **Template Confidence & Quarantine**
**Priority:** P1  
Confidence score from success rate, schema validation freshness, rejection ratio; badges in UI; force “Plan Fresh” button; auto-quarantine under threshold.

### FR-20: **Feature Flags & Progressive Rollout**
**Priority:** P0  
All new features behind flags; canary cohorts; instant kill switches.

(Existing FR-6..FR-12 retained; custom fields FR-11/12 folded from proposal.)

---

## 4. Non-Functional Requirements (selected updates)
- **Performance targets:** Planning <1s P95; First page 2–4s P95; Validation lookups <10ms; Template match <200ms P95.
- **Reliability:** Resume from last acked page on reconnect; circuit breaker policies documented.
- **Security:** PII classification & approval; export watermarking; immutable audit (signed).
- **Scalability:** Sticky sessions for SSE; Redis + Qdrant sharding guidance; autoscale at 70% CPU or connection threshold.
- **Observability:** Decision attribution, guardrail telemetry weekly review, cost dashboards, template health.

---

## 5. System Architecture (summary)
New components: Semantic Validator, Redis Metadata Cache, Template Registry & Approval Queue, Cost Metering, Feature Flag Service, Decision Attribution pipeline. (See Architecture v3.1 for detail & diagrams.)

---

## 6. Data Model (delta highlights)
- **Template registry:** lifecycle state, owner, approvals, confidence score, quarantine flags.
- **Custom field descriptors:** enrichment status, ambiguity score, embeddings.
- **Audit log:** semantic layer outcome, warning overrides, template state transitions.
- **Cost meter:** tenant/user counters per resource type.
- **Decision attribution:** artifact IDs with weights.

---

## 7. Validation (now 5 layers)
1. JSON schema.  
2. NetSuite schema (custom-field fallback).  
3. Operator compatibility.  
4. Guardrails.  
5. **Semantic validation** (business rules, temporal boundaries, emptiness predictors).

---

## 8. Disambiguation (decision matrix finalized)
- Auto-fill strong preference (>10 uses, <90 days).  
- Confirm on high-impact change or boundary conditions.  
- Entity-implies-subsidiary rule; preference versioning by fiscal-year.

---

## 9. Execution, Paging & Exports (clarifications)
- 1,000-row pages; buffer=5; checkpoint/resume.  
- Export tiers: <50K realtime XLSX; 50–500K async XLSX; >500K CSV/partitioned; 2M hard cap.

---

## 10. Composition (staged)
- **5a (parallel)** and **5b (simple joins)** in release; **5c** deferred; budget 6–8 weeks.  
- Composition plans **stored as templates** referencing subtemplates + join keys + grain.

---

## 11. Implementation Phases (resequenced)
**MVP:** Phase 0 → 1 → 2 → **4** → 6 → Go-Live → 3 → 5a/5b → 7–9 → (5c later).  
UAT = 3–4 weeks; weekly guardrail review; perf CI gate mandatory.

---

## 12. Acceptance Criteria (new/updated)
- Cache hit rate ≥80% for schema reads.  
- Template quarantine MTTR <1h.  
- Cost metering accuracy ≥98%.  
- Decision attribution coverage ≥90%.  
- Guardrail violation categories reduced ≥50% QoQ.

---

## 13. Risks & Mitigations
- Template quality drift → governance + quarantine.  
- Cache staleness → SWR, degraded-mode banner, background refresh.  
- Cost spikes → quotas + alerts.  
- Accuracy regressions → perf/attribution dashboards, A/B prompt testing.

---

## 14. Appendices
- Disambiguation matrix, operator matrix, example semantic rules (YAML), template lifecycle diagram, CI perf budget table.
