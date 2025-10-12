
# NetSuite NL→Saved Search Planning Plan + Catalog
**Version:** 2025.1-seed  
**Generated:** 2025-10-08 02:59:03 UTC+03:00+0300

---

## Part A — Plan: NL → Catalog-Valid Saved Search (Fields + Joins)

### 0) Inputs & Outputs

**Inputs**
- User English query (e.g., “Show vendor spend by category for Q4 2024 in the Kenya subsidiary”).
- Tenant context: NetSuite role, allowed subsidiaries, fiscal settings, user preferences.
- **Catalog** (authoritative): for each record type → allowed `searchJoins`, `filters`, `columns`, field types, enum/list resolvers.
- Custom Field Descriptors: discovered nightly (with aliases, types, options).
- Operator matrix & guardrails (posting, date window, row/column caps).

**Output (SavedSearchPlan JSON)**
```json
{
  "recordType": "transaction",
  "joins": ["vendorJoin","expenseCategoryJoin","subsidiaryJoin"],
  "filters": [
    {"field":"type","operator":"anyof","values":["vendorBill","vendorPayment"],"required":true},
    {"field":"posting","operator":"is","values":["T"],"required":true},
    {"field":"trandate","operator":"within","values":["2024-10-01","2024-12-31"],"required":true},
    {"field":"subsidiary","operator":"anyof","values":["<Kenya ID>"],"required":true}
  ],
  "columns": [
    "trandate","entity","amount","taxamount","memo",
    {"join":"expenseCategoryJoin","field":"name"},
    {"join":"vendorJoin","field":"entityid"}
  ],
  "summaries":[{"field":"amount","function":"sum"},{"field":"name","function":"group"}],
  "sort":{"field":"amount","order":"desc"},
  "disambiguationCandidates":{"subsidiary":[/* if ambiguous */]}
}
```

---

### 1) Pre-parse & Normalize
- Lightweight extractors for time phrases, numeric thresholds, entities (subsidiary/account/vendor/customer), metrics and aggregations.
- Glossary & synonyms (e.g., vendor=supplier, class=classification).  
**Output:** `IntentDraft` (base domain guess, entities, period, grain, measures, dimensions).

### 2) Retrieve Planning Context (RAG)
- Pull compact **catalog slices** for candidate record types: allowed joins/filters/columns + types.
- Include top-k matching **custom field descriptors** (aliases, types).
- Include operator matrix + guardrails. Use a machine-friendly layout.

### 3) Base Record Type Selection
- Heuristics: posted amounts/time → `transaction`; master lists → entity record; GL references → `transaction` (+ account join).  
- Tie-break by coverage in catalog. If required fields not reachable → flag for **composition**.

### 4) Candidate Field Mapping
- Map user terms → field labels/aliases using BM25 + embeddings over catalog + custom descriptors.
- Resolve ambiguity; mark list fields needing **ID resolution**.  
**Output:** `RequiredFields` with `(fieldId, type, needsJoin?, joinName?, confidence)`.

### 5) Join Feasibility & Cover (Join Planner)
- Let U = required fields not on base `*SearchBasic`.
- For each allowed join J, compute `Fields(J)` from `*SearchRowBasic` and pick minimal set of joins via **greedy set cover** with constraints:
  - Max joins (e.g., ≤10).
  - Penalize high-fanout joins unless required.
  - Prefer “main” joins when overlapping fields exist.  
**Output:** ordered `joins[]` strictly from the catalog.

### 6) Operator & Filter Resolution
- Enforce operator matrix (list/date/number/text/checkbox).
- Normalize values: dates (fiscal), amounts/currency, names→IDs, transaction subtypes.
- Auto-guardrails: `posting=T`, period window, subsidiary scope.

### 7) LLM Prompting & Structured Output
- **Call A (Plan Draft):** Constrained JSON-only output; include catalog slice + custom fields + operator matrix.
- **Call B (Self-Check):** Ask LLM to remove any non-catalog fields/joins; keep rationale for telemetry.
- Enforce output with JSON Schema (Layer 1).

### 8) Four-Layer Validation
1. JSON schema structure/types.  
2. **Catalog** compatibility (fields on base or via chosen joins; joins allowed on base).  
3. Operator compatibility (incl. value arity).  
4. Guardrails (`posting`, period, subsidiary; caps; summaries need grouping).

### 9) Disambiguation Loop (UI-assisted)
- Trigger 409 when names/periods/fields are ambiguous; return candidates + preferences.
- Apply user selection **without re-planning** (fill slots).

### 10) Composition Escalation
- If set cover fails: generate **CompositionPlan** with sub-templates (e.g., transactions + budgets),
  normalize grains, join in DuckDB, cache composition template.

### 11) Transparency & Telemetry
- Store canonical plan + explain string; track guardrail hits, join failures, disambiguations.

### 12) Example Walk-throughs
- Vendor spend by category (joins: `vendorJoin`, `expenseCategoryJoin`, `subsidiaryJoin`).
- Invoices + customer credit limit (join: `customerJoin`, custom credit-limit field if present).
- Revenue vs budget by month (composition).

---

## Part B — JSON Catalog (Starter, 2025.1)

```json
{
  "version": "2025.1-seed",
  "source": "NetSuite Schema Browser 2025.1",
  "generatedAt": "2025-10-08 02:59:03 UTC+03:00+0300",
  "records": [
    {
      "recordType": "transaction",
      "status": "authoritative",
      "searchObjectRef": {
        "title": "TransactionSearch (joins)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/transactionsearch.html"
      },
      "filtersRef": {
        "title": "TransactionSearchBasic (Filters)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/transactionsearchbasic.html"
      },
      "columnsRef": {
        "title": "TransactionSearchRowBasic (Columns)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/transactionsearchrowbasic.html"
      },
      "searchJoins": [
        "accountingPeriodJoin","advanceToApplyAccountJoin","advancedToApplyTransactionJoin","appliedToTransactionJoin","applyingTransactionJoin","assemblyItemBillOfMaterialsJoin","billableTransactionJoin","billingTransactionJoin","binNumberJoin","callJoin","caseJoin","classJoin","cogsPurchaseTransactionJoin","cogsSaleTransactionJoin","contactPrimaryJoin","createdFromTransactionJoin","creatingTransactionJoin","currencyJoin","customerJoin","customerMainJoin","departmentJoin","depositTransactionJoin","destinationTransactionJoin","employeeJoin","eventJoin","expenseCategoryJoin","fileJoin","fulfillmentTransactionJoin","hcmJobJoin","hcmJobRequisitionJoin","hcmPositionJoin","inventoryDetailJoin","inventoryNumberJoin","itemJoin","itemMainJoin","jobEnableTaskJoin","jobJoin","jobMainJoin","jobTypeJoin","leadSourceJoin","locationJoin","manufacturingRoutingJoin","messagesJoin","nextApproverJoin","notesJoin","opportunityJoin","originatingLeadJoin","originatingTransactionJoin","paidTransactionJoin","partnerJoin","paymentOptionJoin","paymentTransactionJoin","payrollItemJoin","periodJoin","plannedWorkOrderJoin","promotionalCouponCodeJoin","purchaseOrderJoin","requestorJoin","revComittingTransactionJoin","revisionJoin","revRecScheduleJoin","rvTransactionJoin","saleJoin","salesEffectiveDateJoin","salesOrderJoin","salesRepJoin","salesTeamMemberJoin","salesTeamRoleJoin","shippingAddressJoin","statusEnumJoin","storeJoin","subsidiaryJoin","taskJoin","taxDetailJoin","taxItemJoin","timeJoin","toLocationJoin","transactionPrfjJoin","transactionLinePrfjJoin","transactionLinePtdJoin","transactionPartnerJoin","transactionShippingAddressJoin","transactionStatusJoin","vendorJoin","vendorMainJoin","workOrderJoin"
      ],
      "enumsRef": [
        { "title": "TransactionType", "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/enum/transactiontype.html" }
      ],
      "customFields": {
        "supportsCustomFields": true,
        "prefixExamples": ["custbody", "custcol"],
        "note": "All deployed transaction body/line custom fields are available to Saved Searches as filters and columns."
      },
      "notes": [
        "Use TransactionSearchBasic for filters and TransactionSearchRowBasic for columns.",
        "Apply the join legs listed here to pull fields from related records.",
        "Subtype reports (e.g., Sales Orders) are Transaction searches filtered by type."
      ]
    },
    {
      "recordType": "account",
      "status": "authoritative",
      "searchObjectRef": {
        "title": "AccountSearch (joins)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/accountsearch.html"
      },
      "filtersRef": {
        "title": "AccountSearchBasic (Filters)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/accountsearchbasic.html"
      },
      "columnsRef": {
        "title": "AccountSearchRowBasic (Columns)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/accountsearchrowbasic.html"
      },
      "searchJoins": ["userJoin"],
      "customFields": {
        "supportsCustomFields": true,
        "prefixExamples": [],
        "note": "Account supports custom fields; presence indicated by customFieldList on record page."
      },
      "notes": ["Common filters include acctType, isInactive, and subsidiary scope (OneWorld)."]
    },
    {
      "recordType": "customer",
      "status": "seed",
      "searchObjectRef": {
        "title": "CustomerSearch (joins)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/customersearch.html"
      },
      "filtersRef": {
        "title": "CustomerSearchBasic (Filters)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/customersearchbasic.html"
      },
      "columnsRef": {
        "title": "CustomerSearchRowBasic (Columns)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/customersearchrowbasic.html"
      },
      "searchJoins": ["transactionJoin","contactJoin","contactPrimaryJoin","addressJoin","messagesJoin","campaignResponseJoin","taskJoin","phoneCallJoin","eventJoin","fileJoin","userNotesJoin","salesRepJoin","partnerJoin","jobJoin","opportunityJoin"],
      "customFields": {
        "supportsCustomFields": true,
        "prefixExamples": ["custentity"],
        "note": "Customer entity custom fields available as filters/columns."
      },
      "notes": ["For revenue metrics, prefer transaction searches filtered by entity and summarized by period."]
    },
    {
      "recordType": "vendor",
      "status": "seed",
      "searchObjectRef": {
        "title": "VendorSearch (joins)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/vendorsearch.html"
      },
      "filtersRef": {
        "title": "VendorSearchBasic (Filters)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/vendorsearchbasic.html"
      },
      "columnsRef": {
        "title": "VendorSearchRowBasic (Columns)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/vendorsearchrowbasic.html"
      },
      "searchJoins": ["transactionJoin","addressJoin","fileJoin","messagesJoin","userNotesJoin","contactJoin","taskJoin","phoneCallJoin","eventJoin","subsidiaryJoin"],
      "customFields": {
        "supportsCustomFields": true,
        "prefixExamples": ["custentity"],
        "note": "Vendor entity custom fields available as filters/columns."
      },
      "notes": ["AP analytics typically run as transaction searches filtered by vendor and transaction types."]
    },
    {
      "recordType": "item",
      "status": "seed",
      "searchObjectRef": {
        "title": "ItemSearch (joins)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/itemsearch.html"
      },
      "filtersRef": {
        "title": "ItemSearchBasic (Filters)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/itemsearchbasic.html"
      },
      "columnsRef": {
        "title": "ItemSearchRowBasic (Columns)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/itemsearchrowbasic.html"
      },
      "searchJoins": ["transactionJoin","vendorJoin","pricingJoin","binNumberJoin","memberItemJoin","assemblyItemJoin","accountingBookDetailJoin","locationJoin"],
      "customFields": {
        "supportsCustomFields": true,
        "prefixExamples": ["custitem"],
        "note": "Item custom fields (`custitem*`) are available in Item searches."
      },
      "notes": ["Inventory KPIs may require either item searches (with location) or transaction searches with posting=T."]
    },
    {
      "recordType": "subsidiary",
      "status": "seed",
      "searchObjectRef": {
        "title": "SubsidiarySearch (joins)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/subsidiarysearch.html"
      },
      "filtersRef": {
        "title": "SubsidiarySearchBasic (Filters)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/subsidiarysearchbasic.html"
      },
      "columnsRef": {
        "title": "SubsidiarySearchRowBasic (Columns)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/subsidiarysearchrowbasic.html"
      },
      "searchJoins": ["transactionJoin","employeeJoin","addressJoin","accountingBookDetailJoin","currencyJoin","taxRegistrationJoin"],
      "customFields": {
        "supportsCustomFields": true,
        "prefixExamples": [],
        "note": "Subsidiary custom fields appear in Basic/RowBasic where deployed."
      },
      "notes": ["Use subsidiary searches for master listings or to constrain other searches by subsidiary."]
    },
    {
      "recordType": "department",
      "status": "seed",
      "searchObjectRef": {
        "title": "DepartmentSearch (joins)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/departmentsearch.html"
      },
      "filtersRef": {
        "title": "DepartmentSearchBasic (Filters)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/departmentsearchbasic.html"
      },
      "columnsRef": {
        "title": "DepartmentSearchRowBasic (Columns)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/departmentsearchrowbasic.html"
      },
      "searchJoins": ["transactionJoin","employeeJoin","accountJoin"],
      "customFields": {
        "supportsCustomFields": true,
        "prefixExamples": [],
        "note": "Custom department fields appear in Basic/RowBasic when present."
      },
      "notes": ["For P&L by department, run transaction search grouped by department + period."]
    },
    {
      "recordType": "classification",
      "alias": "class",
      "status": "seed",
      "searchObjectRef": {
        "title": "ClassificationSearch (joins)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/classificationsearch.html"
      },
      "filtersRef": {
        "title": "ClassificationSearchBasic (Filters)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/classificationsearchbasic.html"
      },
      "columnsRef": {
        "title": "ClassificationSearchRowBasic (Columns)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/classificationsearchrowbasic.html"
      },
      "searchJoins": ["transactionJoin","employeeJoin"],
      "customFields": {
        "supportsCustomFields": true,
        "prefixExamples": [],
        "note": "Custom class fields appear in Basic/RowBasic when present."
      },
      "notes": ["Disambiguate 'class' vs 'department' via glossary and UI prompts."]
    },
    {
      "recordType": "location",
      "status": "seed",
      "searchObjectRef": {
        "title": "LocationSearch (joins)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/locationsearch.html"
      },
      "filtersRef": {
        "title": "LocationSearchBasic (Filters)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/locationsearchbasic.html"
      },
      "columnsRef": {
        "title": "LocationSearchRowBasic (Columns)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/locationsearchrowbasic.html"
      },
      "searchJoins": ["transactionJoin","inventoryNumberJoin","binJoin","employeeJoin"],
      "customFields": {
        "supportsCustomFields": true,
        "prefixExamples": [],
        "note": "Custom location fields appear in Basic/RowBasic when present."
      },
      "notes": ["Inventory balances by location may require item/location summaries or transaction filters."]
    },
    {
      "recordType": "employee",
      "status": "seed",
      "searchObjectRef": {
        "title": "EmployeeSearch (joins)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/employeesearch.html"
      },
      "filtersRef": {
        "title": "EmployeeSearchBasic (Filters)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/employeesearchbasic.html"
      },
      "columnsRef": {
        "title": "EmployeeSearchRowBasic (Columns)",
        "url": "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/employeesearchrowbasic.html"
      },
      "searchJoins": ["transactionJoin","timeJoin","addressJoin","fileJoin","messagesJoin","userNotesJoin","taskJoin","phoneCallJoin","eventJoin","campaignResponseJoin","roleJoin","departmentJoin","classJoin","locationJoin"],
      "customFields": {
        "supportsCustomFields": true,
        "prefixExamples": ["custentity"],
        "note": "Employee entity custom fields available as filters/columns; enforce PII controls."
      },
      "notes": ["PII: mask sensitive values in UI and require approvals for exports with restricted fields."]
    }
  ],
  "transactionSubtypes": {
    "salesOrder": {
      "recordType": "transaction",
      "filter": { "field": "type", "operator": "anyof", "values": ["salesOrder"] },
      "enumRef": "TransactionType"
    },
    "purchaseOrder": {
      "recordType": "transaction",
      "filter": { "field": "type", "operator": "anyof", "values": ["purchaseOrder"] },
      "enumRef": "TransactionType"
    },
    "vendorBill": {
      "recordType": "transaction",
      "filter": { "field": "type", "operator": "anyof", "values": ["vendorBill"] },
      "enumRef": "TransactionType"
    },
    "invoice": {
      "recordType": "transaction",
      "filter": { "field": "type", "operator": "anyof", "values": ["invoice"] },
      "enumRef": "TransactionType"
    },
    "creditMemo": {
      "recordType": "transaction",
      "filter": { "field": "type", "operator": "anyof", "values": ["creditMemo"] },
      "enumRef": "TransactionType"
    },
    "journalEntry": {
      "recordType": "transaction",
      "filter": { "field": "type", "operator": "anyof", "values": ["journalEntry"] },
      "enumRef": "TransactionType"
    },
    "customerPayment": {
      "recordType": "transaction",
      "filter": { "field": "type", "operator": "anyof", "values": ["customerPayment"] },
      "enumRef": "TransactionType"
    },
    "itemFulfillment": {
      "recordType": "transaction",
      "filter": { "field": "type", "operator": "anyof", "values": ["itemFulfillment"] },
      "enumRef": "TransactionType"
    },
    "cashSale": {
      "recordType": "transaction",
      "filter": { "field": "type", "operator": "anyof", "values": ["cashSale"] },
      "enumRef": "TransactionType"
    },
    "cashRefund": {
      "recordType": "transaction",
      "filter": { "field": "type", "operator": "anyof", "values": ["cashRefund"] },
      "enumRef": "TransactionType"
    }
  },
  "plannerGuardrails": {
    "operatorMatrix": {
      "list": ["anyof", "noneof"],
      "date": ["on", "before", "after", "within"],
      "number": ["equals", "greaterthan", "lessthan", "between"],
      "text": ["contains", "startswith", "is", "isnot"]
    },
    "notes": [
      "Choose base record carefully; for time-based metrics, prefer transaction.",
      "Resolve custom field types/operators via the Field Descriptor cache."
    ]
  }
}
```

# NetSuite Operator Schema & Matrix (v1.0)
**Generated (UTC):** 2025-10-12 17:56 UTC  
**Purpose:** Constrain LLM output to *valid, legal* Saved Search filters by field type. Pairs with your `source_of_truth.json` catalog.

> **Note:** This matrix is deliberately conservative and aligned with common NetSuite Saved Search semantics. Tenant-specific differences can be applied by overriding per-tenant policy (see §7).

---

## 1) Operator Vocabulary (canonical IDs)

Each operator has: an **id**, **category**, **arity** (number of operands), **valueTypes** (allowed operand data types), and safety hints.

```json
{
  "schemaVersion": "1.0",
  "operators": [
    {
      "id": "anyof",
      "category": "set",
      "arity": ">=1",
      "valueTypes": ["listId","text"],
      "supportsMulti": true,
      "description": "Field value is any of the specified list/record IDs or names.",
      "examples": ["subsidiary anyof ['123','125']", "account anyof ['4000 Revenue']"]
    },
    {
      "id": "noneof",
      "category": "set",
      "arity": ">=1",
      "valueTypes": ["listId","text"],
      "supportsMulti": true,
      "description": "Field value is none of the specified IDs or names."
    },
    {
      "id": "equalto",
      "category": "comparison",
      "arity": 1,
      "valueTypes": ["number","currency","percent","integer","text","listId","boolean"],
      "supportsMulti": false,
      "description": "Field equals the operand (exact match for numbers; case-insensitive for text in NS UI semantics)."
    },
    {
      "id": "notequalto",
      "category": "comparison",
      "arity": 1,
      "valueTypes": ["number","currency","percent","integer","text","listId","boolean"]
    },
    {
      "id": "greaterthan",
      "category": "comparison",
      "arity": 1,
      "valueTypes": ["number","currency","percent","integer","date","datetime"]
    },
    {
      "id": "greaterthanorequalto",
      "category": "comparison",
      "arity": 1,
      "valueTypes": ["number","currency","percent","integer","date","datetime"]
    },
    {
      "id": "lessthan",
      "category": "comparison",
      "arity": 1,
      "valueTypes": ["number","currency","percent","integer","date","datetime"]
    },
    {
      "id": "lessthanorequalto",
      "category": "comparison",
      "arity": 1,
      "valueTypes": ["number","currency","percent","integer","date","datetime"]
    },
    {
      "id": "between",
      "category": "range",
      "arity": 2,
      "valueTypes": ["number","currency","percent","integer","date","datetime"],
      "description": "Inclusive range: low..high"
    },
    {
      "id": "on",
      "category": "date",
      "arity": 1,
      "valueTypes": ["date","datetime"]
    },
    {
      "id": "onorafter",
      "category": "date",
      "arity": 1,
      "valueTypes": ["date","datetime"]
    },
    {
      "id": "onorbefore",
      "category": "date",
      "arity": 1,
      "valueTypes": ["date","datetime"]
    },
    {
      "id": "after",
      "category": "date",
      "arity": 1,
      "valueTypes": ["date","datetime"]
    },
    {
      "id": "before",
      "category": "date",
      "arity": 1,
      "valueTypes": ["date","datetime"]
    },
    {
      "id": "within",
      "category": "date_range",
      "arity": 2,
      "valueTypes": ["date","datetime"],
      "description": "Between two dates (inclusive)."
    },
    {
      "id": "notwithin",
      "category": "date_range",
      "arity": 2,
      "valueTypes": ["date","datetime"]
    },
    {
      "id": "fiscalis",
      "category": "period",
      "arity": 1,
      "valueTypes": ["periodId","periodName"],
      "description": "Matches an accounting period by id/name (when period filter is available)."
    },
    {
      "id": "contains",
      "category": "text",
      "arity": 1,
      "valueTypes": ["text"]
    },
    {
      "id": "doesnotcontain",
      "category": "text",
      "arity": 1,
      "valueTypes": ["text"]
    },
    {
      "id": "startswith",
      "category": "text",
      "arity": 1,
      "valueTypes": ["text"]
    },
    {
      "id": "doesnotstartwith",
      "category": "text",
      "arity": 1,
      "valueTypes": ["text"]
    },
    {
      "id": "isempty",
      "category": "nullcheck",
      "arity": 0,
      "valueTypes": []
    },
    {
      "id": "isnotempty",
      "category": "nullcheck",
      "arity": 0,
      "valueTypes": []
    },
    {
      "id": "istrue",
      "category": "boolean",
      "arity": 0,
      "valueTypes": []
    },
    {
      "id": "isfalse",
      "category": "boolean",
      "arity": 0,
      "valueTypes": []
    }
  ]
}
```

---

## 2) Field Type → Allowed Operators (Matrix)

> Use this as the **authoritative** map when validating LLM plans (Layer 3). If an operator is not in the list for the field’s type, reject the plan with a structured error.

| **Field Type**       | **Valid Operators** |
|----------------------|---------------------|
| `list` (record refs) | `anyof`, `noneof`, `equalto`, `notequalto`, `isempty`, `isnotempty` |
| `multiselect`        | `anyof`, `noneof`, `isempty`, `isnotempty` |
| `text`               | `contains`, `doesnotcontain`, `startswith`, `doesnotstartwith`, `equalto`, `notequalto`, `isempty`, `isnotempty` |
| `email`              | `contains`, `startswith`, `equalto`, `notequalto`, `isempty`, `isnotempty` |
| `phone`              | `contains`, `startswith`, `equalto`, `notequalto`, `isempty`, `isnotempty` |
| `url`                | `contains`, `startswith`, `equalto`, `notequalto`, `isempty`, `isnotempty` |
| `checkbox/boolean`   | `istrue`, `isfalse`, `isempty`, `isnotempty` |
| `number`             | `equalto`, `notequalto`, `greaterthan`, `greaterthanorequalto`, `lessthan`, `lessthanorequalto`, `between`, `isempty`, `isnotempty` |
| `integer`            | `equalto`, `notequalto`, `greaterthan`, `greaterthanorequalto`, `lessthan`, `lessthanorequalto`, `between`, `isempty`, `isnotempty` |
| `currency`           | `equalto`, `notequalto`, `greaterthan`, `greaterthanorequalto`, `lessthan`, `lessthanorequalto`, `between`, `isempty`, `isnotempty` |
| `percent`            | `equalto`, `notequalto`, `greaterthan`, `greaterthanorequalto`, `lessthan`, `lessthanorequalto`, `between`, `isempty`, `isnotempty` |
| `date`               | `on`, `before`, `after`, `onorbefore`, `onorafter`, `within`, `notwithin`, `between`, `isempty`, `isnotempty` |
| `datetime`           | `on`, `before`, `after`, `onorbefore`, `onorafter`, `within`, `notwithin`, `between`, `isempty`, `isnotempty` |
| `period` (acct)      | `fiscalis`, `anyof`, `noneof` |
| `document` (file id) | `anyof`, `noneof`, `isempty`, `isnotempty` |

> **Tip:** Treat `between` on date/datetime as equivalent to `within` if your adapter normalizes them.

---

## 3) JSON Policy (to embed into `source_of_truth.json`)

Embed a compact policy object so the validator has a single source of truth.

```json
{
  "operatorMatrix": {
    "filtersByType": {
      "list": ["anyof","noneof","equalto","notequalto","isempty","isnotempty"],
      "multiselect": ["anyof","noneof","isempty","isnotempty"],
      "text": ["contains","doesnotcontain","startswith","doesnotstartwith","equalto","notequalto","isempty","isnotempty"],
      "email": ["contains","startswith","equalto","notequalto","isempty","isnotempty"],
      "phone": ["contains","startswith","equalto","notequalto","isempty","isnotempty"],
      "url": ["contains","startswith","equalto","notequalto","isempty","isnotempty"],
      "boolean": ["istrue","isfalse","isempty","isnotempty"],
      "number": ["equalto","notequalto","greaterthan","greaterthanorequalto","lessthan","lessthanorequalto","between","isempty","isnotempty"],
      "integer": ["equalto","notequalto","greaterthan","greaterthanorequalto","lessthan","lessthanorequalto","between","isempty","isnotempty"],
      "currency": ["equalto","notequalto","greaterthan","greaterthanorequalto","lessthan","lessthanorequalto","between","isempty","isnotempty"],
      "percent": ["equalto","notequalto","greaterthan","greaterthanorequalto","lessthan","lessthanorequalto","between","isempty","isnotempty"],
      "date": ["on","before","after","onorbefore","onorafter","within","notwithin","between","isempty","isnotempty"],
      "datetime": ["on","before","after","onorbefore","onorafter","within","notwithin","between","isempty","isnotempty"],
      "period": ["fiscalis","anyof","noneof"],
      "document": ["anyof","noneof","isempty","isnotempty"]
    },
    "summariesByType": {
      "number": ["sum","avg","min","max","count","group"],
      "integer": ["sum","avg","min","max","count","group"],
      "currency": ["sum","avg","min","max","count","group"],
      "percent": ["avg","min","max","count","group"],
      "text": ["group","count"],
      "list": ["group","count"],
      "boolean": ["count"],
      "date": ["group","count","min","max"],
      "datetime": ["group","count","min","max"],
      "period": ["group","count"]
    }
  }
}
```

---

## 4) Planner/Validator Contract (LLM Guardrails)

- **Rule 1:** For each filter, resolve the **field’s type** from the catalog, then verify the chosen operator is in `filtersByType[fieldType]`.
- **Rule 2:** If operator requires operands (arity 1 or 2), verify **operand types** match (`listId`, `date`, `number`, etc.).
- **Rule 3:** If operator is multi-valued (e.g., `anyof`), ensure array values are provided.
- **Rule 4:** For date-like operators (`within`, `between`), enforce two operands and canonicalize to ISO dates (`YYYY-MM-DD`).
- **Rule 5:** For period filters (`fiscalis` / `anyof` on period), map friendly labels (e.g., “FY2025-Q1”) to period IDs via resolver.
- **Rule 6:** Reject unknown operators; return structured error with allowed set for that field type.
- **Rule 7:** Summaries in columns must respect `summariesByType` (e.g., only `sum` on numeric).

**Error example:**  
```json
{
  "error": "OPERATOR_NOT_ALLOWED",
  "field": "memo",
  "fieldType": "text",
  "operator": "greaterthan",
  "allowed": ["contains","doesnotcontain","startswith","doesnotstartwith","equalto","notequalto","isempty","isnotempty"],
  "remediation": "Use a text operator such as 'contains' or 'equalto'."
}
```

---

## 5) Testing Matrix (Golden Cases)

- `subsidiary anyof ["ACME Corp US East","ACME Corp EU"]` → **OK** (`list` + `anyof`)  
- `amount between [1000,5000]` → **OK** (`currency` + `between`)  
- `trandate within ["2025-01-01","2025-03-31"]` → **OK** (`date` + `within`)  
- `memo greaterthan 10` → **REJECT** (`text` cannot use numeric comparison)  
- `approvalstatus contains "Approved"` → **REJECT** (if `approvalstatus` is a `list`, use `anyof`)  
- `period fiscalis "FY2025-Q1"` → **OK** (resolver maps to period id)  

---

# NetSuite RAG – Unified LLM Constraint Kit (Planner + Catalog + Operator Matrix)

**Version:** 2025.1-unified  
**Generated:** 2025-10-12  
**Purpose:** Provide a single, authoritative specification that constrains NL→Saved Search planning to valid, cataloged fields/joins/operators and passes a four-layer validator. This merges the planning framework & starter catalog with the operator schema/matrix.

---

## 0) What’s in here

- Planning workflow (inputs → SavedSearchPlan JSON) with guardrails and disambiguation flow.  
- Starter Join/Filter/Column Catalog (seed set: transaction, account, customer, vendor, item, subsidiary, department, class, location, employee + transaction subtypes).  
- Operator Schema & Matrix (canonical operator IDs, arity, value types, and type→allowed operator map).  
- Single JSON “policy” block to embed in `source_of_truth.json` for the validator (Layer 3).

---

## 1) Inputs → Outputs (Planner Contract)

**Inputs**
- NL query, tenant context (role/subsidiaries/fiscal), user prefs.
- **Catalog slice** for candidate record types (allowed joins, filters, columns, field types, enum resolvers).
- Custom Field Descriptors (nightly discovery).  
- Operator matrix & guardrails.

**Output (SavedSearchPlan JSON — normalized, executable)**  
(Example abbreviated)
```json
{
  "recordType": "transaction",
  "joins": ["vendorJoin","expenseCategoryJoin","subsidiaryJoin"],
  "filters": [
    {"field":"type","operator":"anyof","values":["vendorBill","vendorPayment"],"required":true},
    {"field":"posting","operator":"is","values":["T"],"required":true},
    {"field":"trandate","operator":"within","values":["2024-10-01","2024-12-31"],"required":true},
    {"field":"subsidiary","operator":"anyof","values":["<Kenya ID>"],"required":true}
  ],
  "columns": [
    "trandate","entity","amount",
    {"join":"expenseCategoryJoin","field":"name"},
    {"join":"vendorJoin","field":"entityid"}
  ],
  "summaries":[{"field":"amount","function":"sum"},{"field":"name","function":"group"}],
  "sort":{"field":"amount","order":"desc"}
}
```

---

## 2) Planning Workflow (12 Steps)

1) **Pre-parse & normalize**: extract period, entities, amounts, synonyms.  
2) **RAG retrieval**: pull compact catalog slice(s), custom fields, operator matrix.  
3) **Base recordType selection**: prefer `transaction` for time/amount metrics; master lists → entity records; escalate to composition if coverage fails.  
4) **Field mapping**: map user terms → field IDs/types; mark list-ID resolvers.  
5) **Join planner (set cover)**: choose minimal allowed joins to expose all required fields; penalize high-fanout; cap join count.  
6) **Operator/Filter resolution**: enforce matrix; normalize values; auto-guardrails (`posting=T`, period window, subsidiary).  
7) **LLM calls**: JSON-only Plan Draft → Self-Check pruning non-catalog items.  
8) **Four-Layer Validation**: (1) JSON Schema, (2) Catalog compatibility, (3) Operator compatibility, (4) Guardrails.  
9) **Disambiguation loop**: 409 with candidates; apply selection without re-planning.  
10) **Composition escalation** (when needed): subtemplates + DuckDB join; cache composition template.  
11) **Transparency & telemetry**: store plan + rationale; guardrail hits; join failures.  
12) **Examples**: vendor spend by category; invoices + customer credit limit; revenue vs budget by month.

---

## 3) Operator Schema & Matrix (Authoritative)

### 3.1 Operator Vocabulary (canonical IDs)
Each operator: `id`, `category`, `arity`, `valueTypes`, `supportsMulti`, description/examples.
```json
{
  "schemaVersion": "1.0",
  "operators": [
    {"id":"anyof","category":"set","arity":">=1","valueTypes":["listId","text"],"supportsMulti":true},
    {"id":"noneof","category":"set","arity":">=1","valueTypes":["listId","text"],"supportsMulti":true},
    {"id":"equalto","category":"comparison","arity":1,"valueTypes":["number","currency","percent","integer","text","listId","boolean"]},
    {"id":"notequalto","category":"comparison","arity":1,"valueTypes":["number","currency","percent","integer","text","listId","boolean"]},
    {"id":"greaterthan","category":"comparison","arity":1,"valueTypes":["number","currency","percent","integer","date","datetime"]},
    {"id":"greaterthanorequalto","category":"comparison","arity":1,"valueTypes":["number","currency","percent","integer","date","datetime"]},
    {"id":"lessthan","category":"comparison","arity":1,"valueTypes":["number","currency","percent","integer","date","datetime"]},
    {"id":"lessthanorequalto","category":"comparison","arity":1,"valueTypes":["number","currency","percent","integer","date","datetime"]},
    {"id":"between","category":"range","arity":2,"valueTypes":["number","currency","percent","integer","date","datetime"]},
    {"id":"on","category":"date","arity":1,"valueTypes":["date","datetime"]},
    {"id":"onorafter","category":"date","arity":1,"valueTypes":["date","datetime"]},
    {"id":"onorbefore","category":"date","arity":1,"valueTypes":["date","datetime"]},
    {"id":"after","category":"date","arity":1,"valueTypes":["date","datetime"]},
    {"id":"before","category":"date","arity":1,"valueTypes":["date","datetime"]},
    {"id":"within","category":"date_range","arity":2,"valueTypes":["date","datetime"]},
    {"id":"notwithin","category":"date_range","arity":2,"valueTypes":["date","datetime"]},
    {"id":"fiscalis","category":"period","arity":1,"valueTypes":["periodId","periodName"]},
    {"id":"contains","category":"text","arity":1,"valueTypes":["text"]},
    {"id":"doesnotcontain","category":"text","arity":1,"valueTypes":["text"]},
    {"id":"startswith","category":"text","arity":1,"valueTypes":["text"]},
    {"id":"doesnotstartwith","category":"text","arity":1,"valueTypes":["text"]},
    {"id":"isempty","category":"nullcheck","arity":0,"valueTypes":[]},
    {"id":"isnotempty","category":"nullcheck","arity":0,"valueTypes":[]},
    {"id":"istrue","category":"boolean","arity":0,"valueTypes":[]},
    {"id":"isfalse","category":"boolean","arity":0,"valueTypes":[]}
  ]
}
```

### 3.2 FieldType → Allowed Operators
Use this **exact** matrix when validating LLM plans (Layer 3).

| Field Type       | Valid Operators |
|------------------|-----------------|
| list             | anyof, noneof, equalto, notequalto, isempty, isnotempty |
| multiselect      | anyof, noneof, isempty, isnotempty |
| text             | contains, doesnotcontain, startswith, doesnotstartwith, equalto, notequalto, isempty, isnotempty |
| email/phone/url  | contains, startswith, equalto, notequalto, isempty, isnotempty |
| boolean          | istrue, isfalse, isempty, isnotempty |
| number/integer   | equalto, notequalto, greaterthan, greaterthanorequalto, lessthan, lessthanorequalto, between, isempty, isnotempty |
| currency/percent | equalto, notequalto, greaterthan, greaterthanorequalto, lessthan, lessthanorequalto, between, isempty, isnotempty |
| date/datetime    | on, before, after, onorbefore, onorafter, within, notwithin, between, isempty, isnotempty |
| period           | fiscalis, anyof, noneof |
| document (file)  | anyof, noneof, isempty, isnotempty |

### 3.3 JSON Policy Block (drop-in)
Embed under `operatorMatrix` in `source_of_truth.json`. 
```json
{
  "operatorMatrix": {
    "filtersByType": {
      "list": ["anyof","noneof","equalto","notequalto","isempty","isnotempty"],
      "multiselect": ["anyof","noneof","isempty","isnotempty"],
      "text": ["contains","doesnotcontain","startswith","doesnotstartwith","equalto","notequalto","isempty","isnotempty"],
      "email": ["contains","startswith","equalto","notequalto","isempty","isnotempty"],
      "phone": ["contains","startswith","equalto","notequalto","isempty","isnotempty"],
      "url": ["contains","startswith","equalto","notequalto","isempty","isnotempty"],
      "boolean": ["istrue","isfalse","isempty","isnotempty"],
      "number": ["equalto","notequalto","greaterthan","greaterthanorequalto","lessthan","lessthanorequalto","between","isempty","isnotempty"],
      "integer": ["equalto","notequalto","greaterthan","greaterthanorequalto","lessthan","lessthanorequalto","between","isempty","isnotempty"],
      "currency": ["equalto","notequalto","greaterthan","greaterthanorequalto","lessthan","lessthanorequalto","between","isempty","isnotempty"],
      "percent": ["equalto","notequalto","greaterthan","greaterthanorequalto","lessthan","lessthanorequalto","between","isempty","isnotempty"],
      "date": ["on","before","after","onorbefore","onorafter","within","notwithin","between","isempty","isnotempty"],
      "datetime": ["on","before","after","onorbefore","onorafter","within","notwithin","between","isempty","isnotempty"],
      "period": ["fiscalis","anyof","noneof"],
      "document": ["anyof","noneof","isempty","isnotempty"]
    },
    "summariesByType": {
      "number": ["sum","avg","min","max","count","group"],
      "integer": ["sum","avg","min","max","count","group"],
      "currency": ["sum","avg","min","max","count","group"],
      "percent": ["avg","min","max","count","group"],
      "text": ["group","count"],
      "list": ["group","count"],
      "boolean": ["count"],
      "date": ["group","count","min","max"],
      "datetime": ["group","count","min","max"],
      "period": ["group","count"]
    }
  }
}
```

---

## 4) Starter Join/Filter/Column Catalog (seed)

The catalog constrains joins/filters/columns per record type (subset shown; use as authoritative).

```json
{
  "version": "2025.1-seed",
  "records": [
    {
      "recordType": "transaction",
      "searchObjectRef": {"title":"TransactionSearch (joins)","url":"<schema-browser>/transactionsearch.html"},
      "filtersRef": {"title":"TransactionSearchBasic (Filters)","url":"<schema-browser>/transactionsearchbasic.html"},
      "columnsRef": {"title":"TransactionSearchRowBasic (Columns)","url":"<schema-browser>/transactionsearchrowbasic.html"},
      "searchJoins": ["accountingPeriodJoin","customerJoin","vendorJoin","itemJoin","departmentJoin","classJoin","locationJoin","subsidiaryJoin","periodJoin"],
      "customFields": {"supportsCustomFields": true,"prefixExamples":["custbody","custcol"]}
    },
    {
      "recordType": "account",
      "searchObjectRef": {"title":"AccountSearch (joins)","url":"<schema-browser>/accountsearch.html"},
      "filtersRef": {"title":"AccountSearchBasic (Filters)","url":"<schema-browser>/accountsearchbasic.html"},
      "columnsRef": {"title":"AccountSearchRowBasic (Columns)","url":"<schema-browser>/accountsearchrowbasic.html"},
      "searchJoins": ["userJoin"],
      "customFields": {"supportsCustomFields": true}
    },
    { "recordType":"customer", "searchJoins":["transactionJoin","contactJoin","addressJoin"], "customFields":{"supportsCustomFields":true,"prefixExamples":["custentity"]} },
    { "recordType":"vendor",   "searchJoins":["transactionJoin","addressJoin","subsidiaryJoin"], "customFields":{"supportsCustomFields":true,"prefixExamples":["custentity"]} },
    { "recordType":"item",     "searchJoins":["transactionJoin","vendorJoin","pricingJoin","locationJoin"], "customFields":{"supportsCustomFields":true,"prefixExamples":["custitem"]} },
    { "recordType":"subsidiary","searchJoins":["transactionJoin","employeeJoin","currencyJoin"], "customFields":{"supportsCustomFields":true} },
    { "recordType":"department","searchJoins":["transactionJoin","employeeJoin","accountJoin"], "customFields":{"supportsCustomFields":true} },
    { "recordType":"classification","alias":"class","searchJoins":["transactionJoin","employeeJoin"], "customFields":{"supportsCustomFields":true} },
    { "recordType":"location","searchJoins":["transactionJoin","inventoryNumberJoin","employeeJoin"], "customFields":{"supportsCustomFields":true} },
    { "recordType":"employee","searchJoins":["transactionJoin","timeJoin","departmentJoin","classJoin","locationJoin"], "customFields":{"supportsCustomFields":true,"prefixExamples":["custentity"]} }
  ],
  "transactionSubtypes": {
    "salesOrder":    {"recordType":"transaction","filter":{"field":"type","operator":"anyof","values":["salesOrder"]}},
    "purchaseOrder": {"recordType":"transaction","filter":{"field":"type","operator":"anyof","values":["purchaseOrder"]}},
    "vendorBill":    {"recordType":"transaction","filter":{"field":"type","operator":"anyof","values":["vendorBill"]}},
    "invoice":       {"recordType":"transaction","filter":{"field":"type","operator":"anyof","values":["invoice"]}},
    "creditMemo":    {"recordType":"transaction","filter":{"field":"type","operator":"anyof","values":["creditMemo"]}},
    "journalEntry":  {"recordType":"transaction","filter":{"field":"type","operator":"anyof","values":["journalEntry"]}},
    "customerPayment":{"recordType":"transaction","filter":{"field":"type","operator":"anyof","values":["customerPayment"]}},
    "itemFulfillment":{"recordType":"transaction","filter":{"field":"type","operator":"anyof","values":["itemFulfillment"]}},
    "cashSale":      {"recordType":"transaction","filter":{"field":"type","operator":"anyof","values":["cashSale"]}},
    "cashRefund":    {"recordType":"transaction","filter":{"field":"type","operator":"anyof","values":["cashRefund"]}}
  }
}
```

> Notes  
> • Subtype forms are just `transaction` searches filtered by `type`.  
> • All deployed custom fields (body/line/entity) can appear as filters/columns once discovered by the nightly crawler; their types flow into the operator matrix.

---

## 5) Four-Layer Validation (enforced)

1) **JSON Schema**: structure & types OK.  
2) **Catalog Compatibility**: every field is on base or on a chosen **allowed** join; chosen joins are allowed on base.  
3) **Operator Compatibility**: operator ∈ matrix for the field’s type; arity/values match (`date` needs ISO dates; `anyof` needs IDs/names).  
4) **Guardrails**: `posting=T`, period scope (≤5y), subsidiary scope; caps; summaries require grouping.  
(Structured errors include layer, field, allowed operators, remediation.)

---

## 6) Planner/Validator Error Contract (examples)

```json
{
  "error": "OPERATOR_NOT_ALLOWED",
  "field": "memo",
  "fieldType": "text",
  "operator": "greaterthan",
  "allowed": ["contains","doesnotcontain","startswith","doesnotstartwith","equalto","notequalto","isempty","isnotempty"],
  "remediation": "Use a text operator such as 'contains' or 'equalto'."
}
```

---

## 7) Golden Tests (must pass)

- `subsidiary anyof ["ACME Corp US East","ACME Corp EU"]` → OK (`list` + `anyof`).  
- `amount between [1000,5000]` → OK (`currency` + `between`).  
- `trandate within ["2025-01-01","2025-03-31"]` → OK (`date` + `within`).  
- `memo greaterthan 10` → **REJECT** (text vs numeric).  
- `approvalstatus contains "Approved"` → **REJECT** if `approvalstatus` is `list` (use `anyof`).  
- `period fiscalis "FY2025-Q1"` → OK (map label → period id).

---

## 8) How to wire this into the system

- **Prompting**: Provide the LLM with only the **catalog slice** for the selected base record and an abridged operator table; require JSON output conforming to `SavedSearchPlan`. Self-check to prune non-catalog artifacts.  
- **Validator**: Load `source_of_truth.json` embedding the **operatorMatrix** (Section 3.3) and **records[]** catalog (Section 4). Enforce the four layers in order; return structured errors with remediation.  
- **Disambiguation**: On ambiguous names/periods, return 409 with candidates; fill selections and proceed without re-planning.

---

---

### Notes
- The catalog **URLs** reference the public NetSuite Schema Browser (2025.1) for human verification.
- The plan constrains the LLM to **only** choose fields/joins that appear in this catalog (enforced at validation time).
