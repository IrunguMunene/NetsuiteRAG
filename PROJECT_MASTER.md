# NetSuite RAG Project - Master Control Document

**Last Updated:** 2025-10-13 01:05
**Project Path:** D:\Development\NetsuiteRAG

---

## Project Overview

Building a NetSuite RAG reporting tool using Claude as the senior developer.

### Goals
- Natural language querying of NetSuite data
- Intelligent template reuse
- Production-ready quality
- Full governance and auditability

### Timeline
- **Start Date:** 2025-10-10
- **Target Completion:** 2026-01-16 (14 weeks)
- **Current Week:** 1 of 14

---

## Current Work

### Active Task
**Task ID:** T2.03
**Task Name:** Semantic Validator (Layer 5)
**Status:** Not Started
**Session File:** sessions/session-011-T2.03-semantic-validator.md

### Last Completed Task
**Task ID:** T2.02
**Task Name:** Layer 1-4 validators
**Status:** Complete (Merged)
**Session File:** sessions/session-010-T2.02-layer-validators.md

### Quick Start Command
```powershell
.\scripts\start-session.ps1
claude
```
Then tell Claude: **"Read .clinerules and continue"**

---

## Reference Documents

Located in: `docs/`

- [x] netsuite_rag_recommendations.md
- [x] NetSuite_RAG_Reporting_Tool_PRD_v3.1.md
- [x] NetSuite_RAG_System_Architecture_v3.1.md
- [x] NetSuite_RAG_Tasks_and_User_Stories_v3.1.md
- [x] Tech_Stack_Best_Practices_No_DSPy_v2.md
- [x] NL_to_SavedSearch_Plan_and_Catalog_2025-1.md

---

## Development Principles

1. **Claude as Senior Developer** - Claude designs and instructs, I execute
2. **Auto-Detection** - Claude figures out what is next from markers
3. **Git Branching** - Each task on feature branch, PR before merge
4. **One Step at a Time** - Complete and verify each step
5. **Code Review** - Automated review before PR creation
6. **Test-Driven** - Tests before implementation
7. **Documented** - Every decision documented

---

## Environment Setup

- [x] PostgreSQL running (localhost:5432)
- [x] Redis running (localhost:6379)
- [x] Qdrant running (localhost:6333)
- [x] Ollama running (localhost:11434)
- [x] .NET 9 SDK installed
- [x] .NET Aspire workload installed
- [x] Node.js 20.x installed
- [x] Angular CLI 20 installed

---

## Phase Checklist

- [x] Phase 0: Foundations (Week 1-2) - Complete (3 of 3 complete)
- [x] Phase 1: RAG Corpus (Week 1-2) - Complete (5 of 5 complete)
- [ ] Phase 2: Planning & Validation (Week 3-4) (1 of 6 complete)
- [ ] Phase 4: Execution & Streaming (Week 5-6)
- [ ] Phase 6: Angular UI (Week 7-8)
- [ ] Phase 3: Template Reuse (Week 9-10)
- [ ] Phase 5: Composition (Week 11-12)
- [ ] Phase 7-9: Quality & Ops (Week 13-14)

---

## Task Progress

### Phase 0: Foundation
- [x] T0.01: Project scaffolding
- [x] T0.02: Observability baseline
- [x] T0.03: CI scaffolding

### Phase 1: RAG Corpus
- [x] T1.01: Field dictionaries (standard)
- [x] T1.02: Custom Field Crawler (daily)
- [x] T1.03: Descriptor Enrichment
- [x] T1.04: Business glossary + exemplars (30-50)
- [x] T1.05: Index artifacts to vector DB

### Phase 2: Planning & Validation
- [x] T2.01: SavedSearchPlan schema (strict)
- [x] T2.02: Layer 1-4 validators
- [ ] T2.03: Semantic Validator (Layer 5) <- **CURRENT**
- [ ] T2.04: Name to ID resolvers & ambiguity detection
- [ ] T2.05: Dry-run mode
- [ ] T2.06: Guardrail telemetry + weekly review

### Phase 4: Execute, Stream, Export
- [ ] T4.01: SuiteScript/REST adapters
- [ ] T4.02: SSE streaming + backpressure + resume
- [ ] T4.03: Export tier engine + real-time XLSX
- [ ] T4.04: Async export jobs + email
- [ ] T4.05: Retry + Circuit breaker
- [ ] T4.06: Redis Metadata Cache

### Phase 6: UX & UAT
- [ ] T6.01: Query console + history
- [ ] T6.02: Disambiguation dialogs
- [ ] T6.03: Virtualized results table
- [ ] T6.04: Export dialog (tier-aware)
- [ ] T6.05: Feedback widget + reasons
- [ ] T6.06: UAT run (3-4 weeks)

### Phase 3: Template Reuse
- [ ] T3.01: Canonical template format
- [ ] T3.02: Dual-key retrieval
- [ ] T3.03: Shadow path (10%)
- [ ] T3.04: Rejection feedback weightings
- [ ] T3.05: Template Registry (state + audit)
- [ ] T3.06: Approval workflow & notifications
- [ ] T3.07: Template Confidence & Quarantine UX
- [ ] T3.08: Governance dashboards & weekly digest

### Phase 5: Composition
- [ ] T5.01: Composition schema
- [ ] T5.02: 5a: Parallel execution
- [ ] T5.03: 5b: Simple joins in DuckDB
- [ ] T5.04: Composition template caching

### Phase 7-9: Quality, Ops, Cost
- [ ] T7.01: CI Performance Gate (k6)
- [ ] T7.02: Decision Attribution pipeline
- [ ] T7.03: Cost Metering service
- [ ] T7.04: Cost dashboards & exports
- [ ] T7.05: Quotas & throttling
- [ ] T7.06: Template health monitor
- [ ] T7.07: Security/PII workflows
- [ ] T7.08: Runbooks & on-call

---

## Session History






2025-10-13 01:05 : T2.02 - Complete
2025-10-12 18:47 : T2.01 - Complete - Session ended
2025-10-12 18:44 : T2.01 - Complete
2025-10-12 15:18 : T1.05 - Complete - Session ended
2025-10-12 15:16 : T1.05 - Complete
### Completed Sessions
- 2025-10-10 09:00 - T0.01: Project scaffolding - Complete
- 2025-10-10 14:00 - T0.02: Observability baseline - Complete
- 2025-10-11 13:30 - T0.03: CI scaffolding - Complete
- 2025-10-11 15:00 - T1.01: Field dictionaries (standard) - Complete
- 2025-10-11 19:00 - T1.02: Custom Field Crawler (daily) - Complete
- 2025-10-11 20:15 - T1.03: Descriptor Enrichment - Complete
- 2025-10-12 - T1.04: Business glossary + exemplars - Complete (Ready for PR)

---

## Current Blockers

None

---

## Notes

- **Phase 0 Complete!** All foundation tasks finished
- T0.01: Project scaffolding in place with .NET solution structure
- T0.02: Observability baseline configured with OpenTelemetry
- T0.03: CI pipeline with GitHub Actions, parallel jobs, automated testing
- **Phase 1 Nearly Complete!** RAG Corpus implementation (4 of 5 complete)
- T1.01: Field dictionary service with Redis caching, 6 REST endpoints, 14 standard fields
- T1.02: Custom field crawler with daily schedule, stale detection (90+ days), 5 REST endpoints
- T1.03: Field enrichment with Ollama embeddings, alias generation, business context, 4 REST endpoints
- T1.04: Business glossary with 50 terms + 50 query exemplars, 13 REST endpoints
  - **Architectural Note:** Embeddings will be stored only in Qdrant (T1.05), not PostgreSQL
  - PostgreSQL stores business data only (terms, exemplars, metadata)
- Ready for T1.05: Index artifacts to vector DB (Qdrant)

---

## Quick Commands

```powershell
# Start session (Claude auto-detects task)
.\scripts\start-session.ps1
claude
# Then: "Read .clinerules and continue"

# Check environment
.\scripts\check-environment.ps1

# Manual task update
.\scripts\update-task.ps1 -TaskId "T0.03" -Status "Complete"

# Code review before PR
.\scripts\review-code.ps1 -TaskId "T0.03"

# Create PR
.\scripts\create-pr.ps1 -TaskId "T0.03" -Description "Summary"
```