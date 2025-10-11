
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

---

### Notes
- The catalog **URLs** reference the public NetSuite Schema Browser (2025.1) for human verification.
- The plan constrains the LLM to **only** choose fields/joins that appear in this catalog (enforced at validation time).
