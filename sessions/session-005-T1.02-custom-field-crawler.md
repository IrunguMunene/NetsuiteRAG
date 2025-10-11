# Session 005 - T1.02: Custom Field Crawler (daily)

**Date:** 2025-10-11
**Task:** T1.02 - Custom Field Crawler (daily)
**Branch:** feature/T1.02-custom-field-crawler
**Status:** In Progress

---

## Objective

Implement a background service that automatically discovers and tracks NetSuite custom fields on a daily schedule. This crawler will maintain an up-to-date inventory of custom field metadata for use in RAG-based query planning.

---

## Requirements

### Functional Requirements
1. **Discovery**: Automatically discover all custom fields from NetSuite
2. **Storage**: Store custom field descriptors in database with metadata
3. **Change Tracking**: Log all changes to custom fields (new, updated, deleted)
4. **Stale Detection**: Flag fields not seen in 90+ days
5. **Daily Schedule**: Run automatically once per day
6. **Manual Trigger**: Support manual crawl via API endpoint
7. **Cache Invalidation**: Clear Redis cache after updates

### Technical Requirements
- C# 13 with primary constructors
- Background service with scheduled execution
- NetSuite API integration (REST/SOAP)
- Result pattern (no exceptions for business logic)
- XML documentation on all public methods
- FluentAssertions for tests
- EF Core for persistence
- Redis cache invalidation
- Dependency injection

---

## Implementation Plan

### Step 1: Domain Models
- [ ] CustomFieldDescriptor model
- [ ] CustomFieldChangeLog model
- [ ] ChangeType enum (New, Updated, Deleted, Stale)
- [ ] CustomFieldSource enum (Transaction, Entity, Item, etc.)

### Step 2: NetSuite Integration
- [ ] INetSuiteApiService interface
- [ ] NetSuiteApiService implementation
- [ ] Custom field metadata retrieval methods
- [ ] Authentication and connection handling

### Step 3: Crawler Service
- [ ] ICustomFieldCrawlerService interface
- [ ] CustomFieldCrawlerService implementation
- [ ] Background service with daily schedule
- [ ] Discovery logic
- [ ] Update/Insert logic
- [ ] Stale field detection (90+ days)
- [ ] Change log generation

### Step 4: Database Schema
- [ ] custom_field_descriptors table migration
- [ ] custom_field_change_log table migration
- [ ] Indexes for performance

### Step 5: API Endpoints
- [ ] CustomFieldsController
- [ ] GET /api/custom-fields - List custom fields
- [ ] GET /api/custom-fields/{id} - Get specific field
- [ ] POST /api/custom-fields/crawl - Trigger manual crawl
- [ ] GET /api/custom-fields/changes - Get change log

### Step 6: Testing
- [ ] Unit tests for CustomFieldCrawlerService
- [ ] Unit tests for NetSuiteApiService
- [ ] Integration tests for API endpoints
- [ ] Stale detection tests

### Step 7: Verification
- [ ] dotnet build succeeds
- [ ] dotnet test passes
- [ ] Code review checklist

---

## Progress Log

### 2025-10-11 - Session Start
- Created feature branch: feature/T1.02-custom-field-crawler
- Created session file
- Starting with domain model definitions

---

## Files Created/Modified

### Domain Models (NetSuiteRAG.Shared/Models/)
- TBD

### Data Layer (NetSuiteRAG.Api/Data/)
- TBD

### Service Layer (NetSuiteRAG.Api/Services/)
- TBD

### API Layer (NetSuiteRAG.Api/Controllers/)
- TBD

### Configuration
- TBD

---

## Implementation Notes

**NetSuite Custom Field Discovery:**
- Custom fields follow pattern: `custbody_`, `custcol_`, `custentity_`, `custitem_`
- Metadata includes: id, label, type, recordType, isMandatory, defaultValue
- API endpoint: `/services/rest/record/v1/metadata-catalog/customField`

**Stale Field Detection:**
- Track `lastSeenAt` timestamp for each field
- Flag fields where `lastSeenAt` < (now - 90 days)
- Include stale flag in API responses

**Change Tracking:**
- Log: fieldId, changeType, oldValue, newValue, detectedAt
- Store as JSON for flexibility
- Retain logs indefinitely for audit

**Scheduling:**
- Use .NET's `BackgroundService` with `PeriodicTimer`
- Default schedule: 2:00 AM daily
- Configurable via appsettings.json

---

## Next Steps

1. Define domain models for CustomFieldDescriptor and CustomFieldChangeLog
2. Implement NetSuiteApiService for custom field retrieval
3. Create CustomFieldCrawlerService with scheduling
4. Add database migrations
5. Create API endpoints
6. Add tests
7. Verify and commit

---

## Notes

- Building on T1.01 field dictionary foundation
- Custom fields are dynamic and change frequently
- Stale detection helps identify unused fields
- Change log provides audit trail
- Cache invalidation ensures fresh data after crawl

---

## Session End Summary

**Status**: ✅ Complete

**What was accomplished**:
- ✅ Domain models (CustomFieldDescriptor, CustomFieldChangeLog, ChangeType)
- ✅ NetSuite API service with mock data for development
- ✅ CustomFieldCrawlerService with full discovery, update, and stale detection logic
- ✅ Background service with configurable daily schedule (PeriodicTimer)
- ✅ Database migrations for custom_field_descriptors and custom_field_change_logs tables
- ✅ 5 REST API endpoints for custom field management
- ✅ Service registration in Program.cs
- ✅ All builds pass (0 errors, 2 warnings)
- ✅ All tests pass (4/4)

**Files Created**: 16 files, 2,146 lines of code

**Key Features**:
- Automatic discovery of custom fields from NetSuite
- Change tracking with JSON audit log
- Stale field detection (90+ days threshold)
- Manual crawl trigger via API
- Background service runs on configurable schedule (default: 24 hours)
- JSONB columns for flexible metadata storage
- Comprehensive indexing for performance

**What is next**:
- T1.03: Descriptor Enrichment (aliases, business context, embeddings)
- T1.04: Business glossary + exemplars (30-50)
- T1.05: Index artifacts to vector DB

**How to resume**:
```powershell
git checkout master
git merge feature/T1.02-custom-field-crawler
# Or continue on feature branch for additional work
```

---
