# System Architecture

**Last Updated:** 2025-10-10  
**Project:** NetSuite RAG Reporting Tool

---

## High-Level Overview

```
┌─────────────────────────────────────────────────────────────┐
│                         User (Browser)                       │
│                      Angular 20 SPA                          │
└───────────────────────────┬─────────────────────────────────┘
                            │ HTTPS
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    ASP.NET Core API                          │
│                   (.NET 9, C# 13)                            │
│                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │  Controllers │  │   Services   │  │  Validators  │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
│                                                              │
└─────┬────────┬────────┬────────┬────────┬───────────────────┘
      │        │        │        │        │
      │        │        │        │        │
      ▼        ▼        ▼        ▼        ▼
┌──────────┐ ┌─────┐ ┌─────────┐ ┌──────────┐ ┌──────────┐
│PostgreSQL│ │Redis│ │ Qdrant  │ │  Ollama  │ │ NetSuite │
│(Metadata)│ │Cache│ │(Vectors)│ │  (LLM)   │ │   API    │
└──────────┘ └─────┘ └─────────┘ └──────────┘ └──────────┘
```

---

## System Layers

### 1. Presentation Layer (Angular 20)
**Purpose:** User interaction and visualization

**Components:**
- Query Console - Natural language input
- Results Table - Virtualized display (AG Grid)
- Disambiguation Dialog - Resolve ambiguous queries
- Export Dialog - Download results
- Feedback Widget - User satisfaction tracking

**Technology:**
- Angular 20 (standalone components, signals)
- RxJS for reactive state
- AG Grid Community for tables
- Server-Sent Events (SSE) for streaming

---

### 2. API Layer (ASP.NET Core)
**Purpose:** Business logic and orchestration

**Controllers:**
- `QueriesController` - Query lifecycle management
- `TemplatesController` - Template CRUD operations
- `FieldsController` - Field metadata access

**Services:**
- `PlanningService` - Generate query plans using LLM
- `ValidationService` - 5-layer validation pipeline
- `NetSuiteService` - Execute searches, handle paging
- `TemplateService` - Template matching and reuse
- `VectorService` - Embedding generation and search
- `FieldDictionaryService` - Field metadata management

**Technology:**
- .NET 9, C# 13
- .NET Aspire for orchestration
- Microsoft.SemanticKernel for LLM
- Microsoft.Extensions.AI for abstractions

---

### 3. Data Layer
**Purpose:** Persistent storage

**PostgreSQL (Primary Store):**
- Queries - User query history
- Templates - Reusable query templates
- Audit Logs - Immutable activity log
- Custom Field Descriptors - NetSuite field metadata
- Query Checkpoints - Resume state for large queries

**Redis (Cache):**
- Field metadata cache (24h TTL)
- NetSuite schema cache
- Session state
- Rate limiting counters

**Qdrant (Vector Store):**
- Field embeddings
- Exemplar template embeddings
- Business glossary embeddings
- Custom field descriptor embeddings

---

### 4. External Services

**Ollama (Local LLM):**
- Model: llama3.1:8b
- Purpose: Query plan generation
- Endpoint: http://localhost:11434

**NetSuite:**
- Integration: RESTlet (preferred), REST/SOAP (fallback)
- Authentication: OAuth 2.0 / Token-Based Auth
- Operations: Dynamic SavedSearch creation, paging

---

## Core Data Flow

### Flow 1: Natural Language → Results

```
1. User enters NL query
   ↓
2. API: Retrieve RAG context from Qdrant
   - Field definitions
   - Custom field descriptors
   - Exemplar templates
   - Business glossary
   ↓
3. API: Call Ollama with context
   - Generate SavedSearchPlan JSON
   ↓
4. API: Validate plan (5 layers)
   L1: JSON schema validation
   L2: NetSuite schema validation (cached)
   L3: Operator compatibility check
   L4: Business guardrails
   L5: Semantic validation
   ↓
5. IF ambiguous → Return 409 with options
   ELSE proceed
   ↓
6. API: Execute search via NetSuite
   - Paging (1000 rows per page)
   - Stream via SSE
   ↓
7. UI: Display results as they arrive
   - Client acknowledges each page
   - Backpressure control
   ↓
8. User exports results
   - Tier engine determines format
   - Real-time or async generation
```

---

## Key Components Detail

### Planning Service

**Purpose:** Convert natural language to SavedSearchPlan

**Process:**
1. Embed user query using Ollama
2. Search Qdrant for similar exemplars (top 5)
3. Retrieve field definitions for record type
4. Assemble prompt with context
5. Call Ollama with Semantic Kernel
6. Parse JSON response
7. Return SavedSearchPlan

**Caching:**
- Query hash → Plan (1 hour TTL)
- Reduces LLM calls by ~70%

---

### Validation Service

**Purpose:** Ensure query plans are valid and safe

**5 Layers:**

**Layer 1 - JSON Validation:**
- Schema validation
- Required fields present
- Correct types

**Layer 2 - NetSuite Schema:**
- Record type exists
- Fields exist for record type
- Custom fields verified
- Cached for 24 hours

**Layer 3 - Operator Compatibility:**
- Field type supports operator
- Example: date fields don't use "contains"
- Operator matrix lookup

**Layer 4 - Business Guardrails:**
- Posting=true for transactions
- Period/subsidiary required
- Row/column limits enforced
- No SELECT *

**Layer 5 - Semantic Validation:**
- Date logic (end > start)
- Fiscal period boundaries
- Empty result predictors
- Errors block, warnings allow override

---

### NetSuite Service

**Purpose:** Execute searches and handle paging

**Features:**
- Dynamic SavedSearch creation via RESTlet
- Paging at 1000 rows per request
- Retry with exponential backoff
- Circuit breaker pattern
- Resume from checkpoint on disconnect

**Flow:**
1. Create SavedSearch from plan
2. Execute search
3. Fetch first page
4. Stream via SSE
5. Wait for client ACK
6. Fetch next page
7. Repeat until complete

---

### Template Service

**Purpose:** Reuse previous successful queries

**Dual-Key Retrieval:**
1. Semantic similarity (embedding search)
2. Structural similarity (JSON diff)
3. Composite score = 0.7 * semantic + 0.3 * structural
4. Threshold: 0.75

**Template Lifecycle:**
```
Draft → Pending Review → Approved → Active → Deprecated → Archived
                                        ↓
                                   Quarantine (if score drops)
```

**Governance:**
- Confidence score tracking
- Success rate monitoring
- Rejection feedback loop
- Auto-quarantine on poor performance

---

### Vector Service

**Purpose:** Generate and search embeddings

**Operations:**
- Generate embedding (via Ollama nomic-embed-text)
- Store in Qdrant
- Search by similarity
- Batch operations

**Collections:**
- `fields` - Field definitions
- `exemplars` - Example queries
- `glossary` - Business terms
- `custom_fields` - Custom field descriptors

---

## Data Models

### Core Entities

**Query:**
```csharp
public class Query
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string NaturalLanguage { get; set; }
    public string? PlanJson { get; set; }
    public QueryStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

**Template:**
```csharp
public class Template
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string PlanJson { get; set; }
    public string EmbeddingId { get; set; }
    public decimal ConfidenceScore { get; set; }
    public TemplateState State { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**SavedSearchPlan:**
```csharp
public class SavedSearchPlan
{
    public required string RecordType { get; init; }
    public required List<Filter> Filters { get; init; }
    public required List<Column> Columns { get; init; }
    public List<Sort>? Sorts { get; init; }
    public AmbiguousFields? Ambiguous { get; init; }
}
```

---

## Integration Patterns

### NetSuite Integration

**Primary: SuiteScript RESTlet**
```javascript
// RESTlet endpoint
function executeSearch(request) {
    var search = createSearch(request.plan);
    var results = search.run();
    return paginateResults(results, request.pageIndex);
}
```

**Fallback: REST API**
```
POST /services/rest/query
{
  "q": "SELECT ... FROM ..."
}
```

**Fallback: SOAP API**
```xml
<search>
  <searchRecord>...</searchRecord>
</search>
```

---

### Caching Strategy

**Redis Cache Layers:**

**L1 - Hot Cache (5 min TTL):**
- Active queries
- Recent field lookups

**L2 - Warm Cache (1 hour TTL):**
- Field definitions
- Operator matrix
- Semantic validation results

**L3 - Cold Cache (24 hour TTL):**
- NetSuite schema
- Custom field metadata
- Fiscal calendar

**Cache Keys:**
```
field:{recordType}:{fieldId}
schema:{recordType}
plan:{hash}
template:{queryHash}
```

---

## Scalability Considerations

### Current (Local Development)
- Single machine
- All services local
- ~50-100 queries/day capacity

### Future (Production)
**Horizontal Scaling:**
- Stateless API (multiple instances)
- Sticky sessions for SSE
- Redis Sentinel for HA
- PostgreSQL read replicas

**Vertical Scaling:**
- GPU for Ollama
- More memory for Qdrant
- SSD for PostgreSQL

---

## Security

**Authentication:**
- JWT bearer tokens
- SSO integration ready

**Authorization:**
- Role-Based Access Control (RBAC)
- NetSuite subsidiary scoping
- Template ownership

**Data Protection:**
- TLS for all connections
- Secrets in environment variables
- No PII in logs
- Query audit trail

---

## Observability

**Logging:**
- Structured logging (Serilog)
- Correlation IDs
- Request tracing

**Metrics:**
- Request rate
- Error rate
- Response times (P50, P95, P99)
- Cache hit rate
- Template reuse rate

**Tracing:**
- OpenTelemetry
- Distributed tracing
- Performance bottleneck identification

**Dashboards:**
- Operational metrics
- Business metrics
- Cost tracking

---

## Design Principles

1. **Stateless API** - Horizontal scaling
2. **Cache-First** - Reduce external calls
3. **Fail-Safe** - Circuit breakers, retries
4. **Observable** - Comprehensive logging/tracing
5. **Testable** - Dependency injection, interfaces
6. **Maintainable** - Clear separation of concerns
7. **Performant** - Async/await, paging, streaming

---

## Technology Choices

| Component | Technology | Why |
|-----------|------------|-----|
| Backend | .NET 9 | Performance, modern C#, excellent tooling |
| Frontend | Angular 20 | Enterprise-ready, strong typing, signals |
| Database | PostgreSQL | ACID, JSON support, mature |
| Cache | Redis | Fast, versatile, proven |
| Vector DB | Qdrant | Performance, easy to use, local-first |
| LLM | Ollama | Privacy, local execution, cost-effective |
| Orchestration | .NET Aspire | Cloud-native, observability, DX |

---

## Future Enhancements

**Phase 2 Features:**
- Advanced composition (5c)
- Multi-language support
- Custom field daily crawler
- Template governance automation

**Performance:**
- Query result caching
- Predictive template loading
- Parallel NetSuite requests

**Features:**
- Natural language export descriptions
- Scheduled report generation
- Email delivery
- Mobile UI

---

## Quick Reference

**Key Files:**
- `Controllers/QueriesController.cs` - Main API endpoint
- `Services/PlanningService.cs` - LLM integration
- `Services/ValidationService.cs` - 5-layer validation
- `Services/NetSuiteService.cs` - NetSuite integration
- `Data/AppDbContext.cs` - EF Core context

**Key Endpoints:**
- `POST /api/queries` - Create query
- `GET /api/queries/{id}/stream` - Stream results
- `POST /api/queries/{id}/resolve` - Resolve disambiguation
- `GET /api/templates` - List templates

**Key Patterns:**
- Result pattern for business logic
- Repository pattern for data access
- Service pattern for business operations
- Strategy pattern for validation layers