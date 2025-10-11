# Session 006 - T1.03: Descriptor Enrichment

**Date:** 2025-10-11
**Task:** T1.03 - Descriptor Enrichment
**Branch:** feature/T1.03-descriptor-enrichment
**Status:** ✅ Complete

---

## Objective

Enrich field descriptors with aliases, business context, ambiguity scoring, and generate embeddings for semantic search. This will enable the RAG system to better understand user queries and map natural language to NetSuite fields.

**Acceptance Criteria:**
- 95% of descriptors enriched
- All descriptors vectorized (embeddings generated)
- Aliases and business context added
- Ambiguity scores calculated

---

## Requirements

### Functional Requirements
1. **Aliases**: Add common business terms and synonyms for each field
2. **Business Context**: Add domain-specific descriptions and usage examples
3. **Ambiguity Scoring**: Calculate scores for fields with similar names/purposes
4. **Embeddings**: Generate vector embeddings using Ollama for semantic search
5. **Storage**: Persist enriched data in database with JSONB columns
6. **API Endpoints**: Expose enrichment data via REST API

### Technical Requirements
- C# 13 with primary constructors
- Ollama integration for embeddings (nomic-embed-text model)
- Result pattern (no exceptions for business logic)
- XML documentation on all public methods
- EF Core for persistence
- JSONB columns for flexible metadata
- Dependency injection

---

## Implementation Plan

### Step 1: Domain Models & Schema
- [ ] FieldEnrichment model (aliases, business_context, ambiguity_score, embedding)
- [ ] Add enrichment columns to FieldDefinition and CustomFieldDescriptor tables
- [ ] Database migration for enrichment schema

### Step 2: Embedding Service
- [ ] IOllamaEmbeddingService interface
- [ ] OllamaEmbeddingService implementation
- [ ] Integration with Ollama HTTP API
- [ ] Batch embedding generation
- [ ] Error handling and retries

### Step 3: Enrichment Service
- [ ] IFieldEnrichmentService interface
- [ ] FieldEnrichmentService implementation
- [ ] Alias generation logic (manual seed + AI suggestions)
- [ ] Business context generation
- [ ] Ambiguity score calculation
- [ ] Orchestration of enrichment pipeline

### Step 4: Background Processing
- [ ] FieldEnrichmentBackgroundService
- [ ] Batch processing for all fields
- [ ] Progress tracking and logging
- [ ] Configurable schedule

### Step 5: API Endpoints
- [ ] FieldEnrichmentController
- [ ] GET /api/enrichment/status - Get enrichment status
- [ ] POST /api/enrichment/trigger - Trigger enrichment
- [ ] GET /api/enrichment/field/{recordType}/{fieldId} - Get enrichment for specific field
- [ ] PUT /api/enrichment/field/{recordType}/{fieldId} - Update enrichment manually

### Step 6: Testing
- [ ] Unit tests for OllamaEmbeddingService
- [ ] Unit tests for FieldEnrichmentService
- [ ] Integration tests for API endpoints
- [ ] Embedding generation tests

### Step 7: Verification
- [ ] dotnet build succeeds
- [ ] dotnet test passes
- [ ] Code review checklist
- [ ] Verify 95% enrichment coverage

---

## Progress Log

### 2025-10-11 - Session Start
- Created feature branch: feature/T1.03-descriptor-enrichment
- Created session file
- Planning implementation approach

### 2025-10-11 - Implementation Complete
- ✅ Enhanced FieldDefinition and CustomFieldDescriptor models with enrichment properties
- ✅ Ollama embedding service with retry logic (nomic-embed-text model)
- ✅ Field enrichment service with alias generation and business context
- ✅ Database migration for enrichment columns (excluding embeddings - stored in Qdrant)
- ✅ Background enrichment service with configurable schedule
- ✅ FieldEnrichmentController with 4 REST endpoints
- ✅ Service registration in Program.cs
- ✅ Ollama configuration in appsettings.json
- ✅ All builds pass (0 errors, 3 warnings)
- ✅ All tests pass (4/4)

---

## Files to Create/Modify

### Domain Models (NetSuiteRAG.Shared/Models/)
- FieldEnrichment.cs (enrichment entity)
- OllamaEmbeddingRequest.cs (DTO)
- OllamaEmbeddingResponse.cs (DTO)

### Data Layer (NetSuiteRAG.Api/Data/)
- AppDbContext.cs (add FieldEnrichments DbSet)
- Migration: AddFieldEnrichment

### Service Layer (NetSuiteRAG.Api/Services/)
- Interfaces/IOllamaEmbeddingService.cs
- Interfaces/IFieldEnrichmentService.cs
- Implementations/OllamaEmbeddingService.cs
- Implementations/FieldEnrichmentService.cs
- Implementations/FieldEnrichmentBackgroundService.cs

### API Layer (NetSuiteRAG.Api/Controllers/)
- FieldEnrichmentController.cs

### Configuration
- Program.cs (service registration)
- appsettings.json (Ollama configuration)

---

## Implementation Notes

**Embedding Strategy:**
- Use Ollama with nomic-embed-text model (768 dimensions)
- Endpoint: http://localhost:11434/api/embeddings
- Input: field label + description + aliases + business context
- Store as vector type in PostgreSQL (pgvector extension)

**Alias Generation:**
- Seed with common NetSuite terminology mappings
- Example: "trandate" → ["transaction date", "date", "trans date"]
- Example: "entity" → ["customer", "vendor", "contact", "company"]
- Allow manual overrides via API

**Business Context:**
- Field usage scenarios
- Example values
- Common filters/operations
- Related fields

**Ambiguity Scoring:**
- Compare field labels using cosine similarity
- Flag fields with >0.8 similarity
- Store ambiguity groups (e.g., "amount" fields)

**Database Schema:**
```sql
-- Add to field_definitions and custom_field_descriptors
ALTER TABLE field_definitions
  ADD COLUMN aliases TEXT[],
  ADD COLUMN business_context JSONB,
  ADD COLUMN ambiguity_score DECIMAL(3,2),
  ADD COLUMN embedding vector(768),
  ADD COLUMN enriched_at TIMESTAMP;
```

---

## Next Steps

1. Create enrichment domain models
2. Implement Ollama embedding service
3. Create field enrichment service
4. Add database migration
5. Create API endpoints
6. Add tests
7. Verify and commit

---

## Notes

- Building on T1.01 (field dictionaries) and T1.02 (custom field crawler)
- Enrichment enables semantic search in RAG pipeline
- Embeddings will be indexed in Qdrant (T1.05)
- Ambiguity detection helps with disambiguation (T2.04)
- Business context improves LLM understanding of fields

---
