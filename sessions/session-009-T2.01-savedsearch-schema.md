# Session 009 - T2.01: SavedSearchPlan Schema (Strict)

**Date:** 2025-10-12
**Task:** T2.01 - SavedSearchPlan schema (strict)
**Branch:** feature/T2.01-savedsearch-schema
**Status:** In Progress

---

## Objective

Define and implement strict JSON schema validation for SavedSearchPlan. This is Layer 1 of the 5-layer validation pipeline that ensures query plans are structurally valid before being processed.

**Acceptance Criteria:**
- JSON-schema validation with clear errors
- All required fields validated
- Type checking for all properties
- Format validation where applicable
- Clear, actionable error messages

---

## Requirements

### Functional Requirements
1. **SavedSearchPlan Schema Definition**: Strict typing for all fields
2. **Validation Service**: Validate plan structure before processing
3. **Error Messages**: Clear, specific validation errors
4. **REST API**: Endpoint to test schema validation
5. **Unit Tests**: Comprehensive test coverage

### Technical Requirements
- C# 13 with primary constructors
- Result pattern (no exceptions for business logic)
- XML documentation on all public methods
- FluentValidation or System.Text.Json schema validation
- Dependency injection
- RESTful API design

---

## SavedSearchPlan Structure

Based on ARCHITECTURE.md:

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

**Supporting Models:**
- Filter: Field, operator, value(s)
- Column: Field, label, summary type
- Sort: Field, direction
- AmbiguousFields: Entity, period, subsidiary disambiguation

---

## Implementation Plan

### Step 1: Define Schema Models
- [x] Create SavedSearchPlan.cs
- [ ] Create Filter.cs
- [ ] Create Column.cs
- [ ] Create Sort.cs
- [ ] Create AmbiguousFields.cs
- [ ] Define operator enums
- [ ] Define field type enums

### Step 2: Create Validation Service
- [ ] Create ISchemaValidationService interface
- [ ] Implement SchemaValidationService
- [ ] Validation result model
- [ ] Error detail model

### Step 3: Implement Validation Logic
- [ ] Required field validation
- [ ] Type validation
- [ ] Format validation
- [ ] Collection validation (at least one filter/column)
- [ ] Enum validation

### Step 4: API Endpoints
- [ ] ValidationController
- [ ] POST /api/validation/schema
- [ ] Add MetricsCollector integration

### Step 5: DI Registration
- [ ] Register validation service
- [ ] Add configuration if needed

### Step 6: Testing
- [ ] Unit tests for each validation rule
- [ ] Test error message clarity
- [ ] Test valid plans
- [ ] Test invalid plans
- [ ] Edge cases

### Step 7: Verification
- [ ] dotnet build succeeds
- [ ] dotnet test passes
- [ ] All validation rules working
- [ ] Error messages are clear

---

## Progress Log

### 2025-10-12 - Session Start
- ✅ Created feature branch: feature/T2.01-savedsearch-schema
- ✅ Created session file

---

## Architecture Notes

### Validation Pipeline (5 Layers)
**Layer 1 - JSON Schema Validation** (This Task - T2.01)
- Structure validation
- Required fields present
- Correct types
- Format checks

**Layer 2 - NetSuite Schema** (T2.02)
- Record type exists
- Fields exist for record type
- Custom fields verified

**Layer 3 - Operator Compatibility** (T2.02)
- Field type supports operator
- Operator matrix lookup

**Layer 4 - Business Guardrails** (T2.02)
- Posting=true for transactions
- Period/subsidiary required
- Row/column limits

**Layer 5 - Semantic Validation** (T2.03)
- Date logic (end > start)
- Fiscal period boundaries
- Empty result predictors

---

## Files to Create/Modify

### Domain Models (NetSuiteRAG.Shared/Models/)
- SavedSearchPlan.cs
- Filter.cs
- Column.cs
- Sort.cs
- AmbiguousFields.cs
- ValidationResult.cs
- ValidationError.cs

### Enums (NetSuiteRAG.Shared/Models/Enums/)
- FilterOperator.cs
- SortDirection.cs
- SummaryType.cs
- FieldType.cs

### Service Interfaces (NetSuiteRAG.Api/Services/Interfaces/)
- ISchemaValidationService.cs

### Service Implementations (NetSuiteRAG.Api/Services/Implementations/)
- SchemaValidationService.cs

### API Layer (NetSuiteRAG.Api/Controllers/)
- ValidationController.cs

### Configuration
- Program.cs (service registration)

### Tests (NetSuiteRAG.Api.Tests/)
- Services/SchemaValidationServiceTests.cs
- Controllers/ValidationControllerTests.cs

---

## Next Steps

1. Define SavedSearchPlan and supporting models
2. Create validation service interface
3. Implement validation logic
4. Create REST API endpoint
5. Write comprehensive tests

---

## Notes

- This is Layer 1 of 5-layer validation pipeline
- Focus on structure and type validation
- Business logic validation comes in later layers
- Clear error messages are crucial for developer experience
- Foundation for T2.02 (Layers 2-4) and T2.03 (Layer 5)

---
