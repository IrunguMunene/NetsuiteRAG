\# NetSuite RAG Project - Master Control Document



\*\*Last Updated:\*\* 2025-10-10

\*\*Project Path:\*\* D:\\Development\\NetsuiteRAG



\## Project Overview



Building a NetSuite RAG reporting tool using Claude as the senior developer.



\### Goals

\- Natural language querying of NetSuite data

\- Intelligent template reuse

\- Production-ready quality

\- Full governance and auditability



\### Timeline

\- \*\*Start Date:\*\* 2025-10-10

\- \*\*Target Completion:\*\* 2026-01-16 (14 weeks)

\- \*\*Current Week:\*\* 1 of 14



\## Reference Documents



Located in: `docs/`



\- \[x] netsuite\_rag\_recommendations.md

\- \[x] NetSuite\_RAG\_Reporting\_Tool\_PRD\_v3.1.md

\- \[x] NetSuite\_RAG\_System\_Architecture\_v3.1.md

\- \[x] NetSuite\_RAG\_Tasks\_and\_User\_Stories\_v3.1.md

\- \[x] Tech\_Stack\_Best\_Practices\_No\_DSPy\_v2.md



\## Development Principles



1\. \*\*Claude as Senior Developer\*\* - Claude designs and instructs, I execute

2\. \*\*One Step at a Time\*\* - Complete and verify each step before moving on

3\. \*\*Test-Driven\*\* - Tests before implementation

4\. \*\*Documented\*\* - Every decision documented

5\. \*\*Iterative\*\* - Quick feedback loops



\## Current Status



\### Current Phase

\*\*Phase:\*\* 0 - Foundation

\*\*Status:\*\* In Progress

\*\*Next Task:\*\* T0.03 - CI scaffolding



\### Environment Setup

\- \[ ] PostgreSQL running (localhost:5432)

\- \[ ] Redis running (localhost:6379)

\- \[ ] Qdrant running (localhost:6333)

\- \[ ] Ollama running (localhost:11434)

\- \[ ] .NET 9 SDK installed

\- \[ ] .NET Aspire workload installed

\- \[ ] Node.js 20.x installed

\- \[ ] Angular CLI 20 installed



\## Phase Checklist



\- \[ ] Phase 0: Foundations (Week 1-2)

\- \[ ] Phase 1: RAG Corpus (Week 1-2)

\- \[ ] Phase 2: Planning \& Validation (Week 3-4)

\- \[ ] Phase 4: Execution \& Streaming (Week 5-6)

\- \[ ] Phase 6: Angular UI (Week 7-8)

\- \[ ] Phase 3: Template Reuse (Week 9-10)

\- \[ ] Phase 5: Composition (Week 11-12)

\- \[ ] Phase 7-9: Quality \& Ops (Week 13-14)



\## Task Progress



\### Phase 0: Foundations

\- \[x] T0.01: Project scaffolding

\- \[x] T0.02: Observability baseline

\- \[ ] T0.03: CI scaffolding



\## Session History





2025-10-10 21:30 : Session Break - T0.02 complete with Aspire orchestration configured
2025-10-10 20:16 : T0.02 - Complete - Observability baseline with OpenTelemetry, Serilog, queryId correlation, and monitoring dashboard
2025-10-10 16:11 : T0.01 - Complete - Session ended
2025-10-10 16:10 : T0.01 - Complete - Complete solution structure created with .NET 9.0 and Aspire orchestration



\## Current Blockers



None



\## Notes



**Session 002 Achievements:**
- T0.02 Observability baseline completed
- OpenTelemetry, Serilog, queryId correlation implemented
- Custom monitoring dashboard created
- Aspire orchestration configured (PostgreSQL, Redis, Qdrant)
- Health checks and metrics endpoints operational
- 4/4 tests passing

**Infrastructure Status:**
- PostgreSQL: Docker container (managed by Aspire)
- Redis: Docker container (managed by Aspire)
- Qdrant: Local instance (localhost:6333)
- API: Aspire-orchestrated with service integrations

**How to Run:**
```powershell
dotnet run --project src/NetSuiteRAG.AppHost
```
Aspire Dashboard: https://localhost:17110
Custom Dashboard: Available via API root endpoint

