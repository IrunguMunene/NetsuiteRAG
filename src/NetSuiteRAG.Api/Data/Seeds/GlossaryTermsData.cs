using NetSuiteRAG.Shared.Models;

namespace NetSuiteRAG.Api.Data.Seeds;

/// <summary>
/// Seed data for NetSuite business glossary terms.
/// Contains 50 curated terms covering accounting, fields, operators, record types, joins, and common patterns.
/// </summary>
public static class GlossaryTermsData
{
    /// <summary>
    /// Gets all predefined glossary terms for seeding.
    /// </summary>
    public static List<GlossaryTerm> GetGlossaryTerms()
    {
        var now = DateTime.UtcNow;
        var terms = new List<GlossaryTerm>();

        // ========== Accounting Terms ==========

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "posting",
            Definition = "Indicates whether a transaction affects the general ledger and impacts financial reports",
            Synonyms = ["posted", "GL impact", "accounting impact", "posting status"],
            Category = "accounting",
            Examples = ["posting=T", "only posted transactions", "show posted invoices"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "fiscal period",
            Definition = "A financial period used for accounting purposes, typically a month or quarter in a fiscal year",
            Synonyms = ["accounting period", "period", "fiscal month"],
            Category = "accounting",
            Examples = ["fiscal period is January 2024", "in Q1 fiscal periods"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "subsidiary",
            Definition = "A legal entity or company within a multi-subsidiary NetSuite account, used for consolidation",
            Synonyms = ["legal entity", "company", "sub"],
            Category = "accounting",
            Examples = ["subsidiary is ACME US", "filter by subsidiary"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "multi-book accounting",
            Definition = "NetSuite feature allowing multiple accounting books with different GAAPs or reporting standards",
            Synonyms = ["accounting book", "primary book", "secondary book"],
            Category = "accounting",
            Examples = ["primary accounting book", "IFRS book vs GAAP book"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        // ========== Field Names ==========

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "trandate",
            Definition = "Transaction date field - the primary date field on transaction records",
            Synonyms = ["transaction date", "date", "tran date"],
            Category = "fields",
            Examples = ["trandate is this month", "order by trandate", "trandate within last quarter"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "entity",
            Definition = "The customer, vendor, or employee associated with a transaction",
            Synonyms = ["customer", "vendor", "name", "party"],
            Category = "fields",
            Examples = ["entity is ACME Corp", "filter by entity", "entity.name contains"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "amount",
            Definition = "The monetary value of a transaction or line item, typically in the transaction currency",
            Synonyms = ["total", "value", "sum", "transaction amount"],
            Category = "fields",
            Examples = ["amount greater than 1000", "sum of amount", "amount in USD"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "status",
            Definition = "The current state of a record, such as Open, Closed, Pending Approval, Billed, Fulfilled",
            Synonyms = ["state", "record status", "transaction status"],
            Category = "fields",
            Examples = ["status is Open", "status not Closed", "pending approval status"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "type",
            Definition = "The specific kind of transaction or record, such as Invoice, Sales Order, Purchase Order",
            Synonyms = ["record type", "transaction type", "document type"],
            Category = "fields",
            Examples = ["type is Invoice", "type anyof Invoice,Credit Memo", "transaction type"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "memo",
            Definition = "A text field containing notes, comments, or descriptions on transactions and records",
            Synonyms = ["note", "description", "comment", "remarks"],
            Category = "fields",
            Examples = ["memo contains urgent", "memo is not empty", "memo field"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "item",
            Definition = "A product, service, or inventory item that can be bought, sold, or tracked in NetSuite",
            Synonyms = ["product", "inventory item", "sku", "service item"],
            Category = "fields",
            Examples = ["item is Widget-100", "item.name contains laptop", "inventory item"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        // ========== Operators ==========

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "anyof",
            Definition = "Operator that matches if the field equals any value in a comma-separated list",
            Synonyms = ["any of", "in list", "one of"],
            Category = "operators",
            Examples = ["status anyof Open,Pending", "type anyof Invoice,Credit Memo"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "noneof",
            Definition = "Operator that matches if the field does not equal any value in a comma-separated list",
            Synonyms = ["none of", "not in list", "excluding"],
            Category = "operators",
            Examples = ["status noneof Closed,Voided", "type noneof Credit Memo"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "within",
            Definition = "Date operator for relative date ranges like 'this month', 'last quarter', 'this fiscal year'",
            Synonyms = ["date range", "time period", "during"],
            Category = "operators",
            Examples = ["trandate within this month", "date within last quarter"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "contains",
            Definition = "Text operator that matches if the field contains the specified substring (case-insensitive)",
            Synonyms = ["includes", "has", "text contains"],
            Category = "operators",
            Examples = ["name contains ACME", "memo contains urgent"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "is",
            Definition = "Equality operator that matches exact values, used for IDs, names, and exact matches",
            Synonyms = ["equals", "is equal to", "matches"],
            Category = "operators",
            Examples = ["status is Open", "entity is ACME Corp", "posting is T"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "greaterthan",
            Definition = "Numeric operator that matches values strictly greater than the specified number",
            Synonyms = ["greater than", "more than", "exceeds", ">"],
            Category = "operators",
            Examples = ["amount greaterthan 1000", "quantity greaterthan 0"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "lessthan",
            Definition = "Numeric operator that matches values strictly less than the specified number",
            Synonyms = ["less than", "below", "under", "<"],
            Category = "operators",
            Examples = ["amount lessthan 100", "quantity lessthan 10"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        // ========== Record Types ==========

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "transaction",
            Definition = "Base record type for all financial transactions including invoices, sales orders, bills, purchase orders",
            Synonyms = ["financial transaction", "accounting transaction", "tran"],
            Category = "recordtype",
            Examples = ["transaction record type", "search transactions", "all transaction types"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "invoice",
            Definition = "A transaction record representing a bill sent to a customer for goods or services",
            Synonyms = ["customer invoice", "sales invoice", "bill to customer"],
            Category = "recordtype",
            Examples = ["type is Invoice", "all invoices", "invoice transactions"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "sales order",
            Definition = "A transaction record representing a customer's order for goods or services, before invoicing",
            Synonyms = ["SO", "customer order", "order"],
            Category = "recordtype",
            Examples = ["type is Sales Order", "open sales orders", "SO status"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "customer",
            Definition = "A record representing a person or company that purchases goods or services",
            Synonyms = ["client", "account", "buyer"],
            Category = "recordtype",
            Examples = ["customer record", "customer.name", "search customers"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "vendor",
            Definition = "A record representing a person or company from whom goods or services are purchased",
            Synonyms = ["supplier", "merchant", "seller"],
            Category = "recordtype",
            Examples = ["vendor record", "vendor.name", "search vendors"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "item",
            Definition = "A record type for products, services, inventory items, non-inventory items, and other item types",
            Synonyms = ["inventory", "product", "SKU", "inventory item"],
            Category = "recordtype",
            Examples = ["item record", "item.displayname", "search items"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        // ========== Joins ==========

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "customerJoin",
            Definition = "A join to access customer record fields from a transaction or other related record",
            Synonyms = ["customer join", "join to customer", "entity join"],
            Category = "joins",
            Examples = ["customerJoin.email", "customerJoin.subsidiary", "customer.name"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "vendorJoin",
            Definition = "A join to access vendor record fields from a transaction or other related record",
            Synonyms = ["vendor join", "join to vendor", "supplier join"],
            Category = "joins",
            Examples = ["vendorJoin.email", "vendorJoin.category", "vendor.name"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "itemJoin",
            Definition = "A join to access item record fields from a transaction line or other related record",
            Synonyms = ["item join", "join to item", "product join"],
            Category = "joins",
            Examples = ["itemJoin.type", "itemJoin.class", "item.displayname"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "subsidiaryJoin",
            Definition = "A join to access subsidiary record fields from a transaction or other related record",
            Synonyms = ["subsidiary join", "join to subsidiary", "legal entity join"],
            Category = "joins",
            Examples = ["subsidiaryJoin.country", "subsidiaryJoin.currency"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        // ========== Common Patterns ==========

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "summary",
            Definition = "Aggregation of data with grouping, such as sum, count, average, min, max",
            Synonyms = ["aggregation", "group by", "totals", "rollup"],
            Category = "patterns",
            Examples = ["sum of amount by customer", "count transactions by month"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "date range",
            Definition = "Filtering records within a specific time period using start and end dates or relative periods",
            Synonyms = ["time range", "period", "date filter"],
            Category = "patterns",
            Examples = ["this month", "last quarter", "between 1/1/2024 and 3/31/2024"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "open transactions",
            Definition = "Transactions that are not yet closed, billed, fulfilled, or completed",
            Synonyms = ["outstanding", "pending", "unfulfilled", "not closed"],
            Category = "patterns",
            Examples = ["status is Open", "open invoices", "outstanding sales orders"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "voided",
            Definition = "Transactions that have been voided or cancelled and should not affect financial reports",
            Synonyms = ["cancelled", "void", "canceled"],
            Category = "patterns",
            Examples = ["status is Voided", "exclude voided", "mainline is F and voided is F"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "mainline",
            Definition = "A flag indicating the main transaction line (T) vs detail lines (F) on multi-line transactions",
            Synonyms = ["main line", "header line", "transaction header"],
            Category = "patterns",
            Examples = ["mainline is T", "main transaction lines only", "header vs detail"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        // ========== Advanced Concepts ==========

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "saved search",
            Definition = "A stored query in NetSuite that defines filters, columns, and results for reporting and analysis",
            Synonyms = ["search", "report", "query"],
            Category = "concepts",
            Examples = ["run saved search", "create search", "search definition"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "filter",
            Definition = "A condition that restricts which records are included in search results",
            Synonyms = ["criteria", "condition", "where clause", "restriction"],
            Category = "concepts",
            Examples = ["add filter", "filter by status", "filter criteria"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "column",
            Definition = "A field or calculated value displayed in search results",
            Synonyms = ["result column", "output field", "display field"],
            Category = "concepts",
            Examples = ["show column", "add amount column", "display customer name"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "internal ID",
            Definition = "The unique numeric identifier for a record in NetSuite, immutable and used for API references",
            Synonyms = ["ID", "record ID", "internalid", "primary key"],
            Category = "concepts",
            Examples = ["internal ID is 12345", "customer ID", "entity ID"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        // ========== NetSuite-Specific Terms ==========

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "OneWorld",
            Definition = "NetSuite's multi-subsidiary and multi-currency feature for global companies",
            Synonyms = ["multi-subsidiary", "global edition", "international"],
            Category = "concepts",
            Examples = ["OneWorld account", "multiple subsidiaries", "multi-currency"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "SuiteScript",
            Definition = "NetSuite's JavaScript-based scripting language for customization and automation",
            Synonyms = ["script", "customization", "automation"],
            Category = "concepts",
            Examples = ["SuiteScript function", "run script", "custom script"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "restlet",
            Definition = "A SuiteScript-based custom RESTful API endpoint in NetSuite",
            Synonyms = ["REST endpoint", "custom API", "web service"],
            Category = "concepts",
            Examples = ["call restlet", "restlet endpoint", "custom REST API"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "custom field",
            Definition = "A user-defined field added to standard NetSuite records for additional data capture",
            Synonyms = ["custom column", "user-defined field", "extension field"],
            Category = "concepts",
            Examples = ["custbody_priority", "custrecord_rating", "custom transaction field"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "record type ID",
            Definition = "The internal identifier for a record type in NetSuite, such as 'customer', 'invoice', 'salesorder'",
            Synonyms = ["record type", "type ID", "entity type"],
            Category = "concepts",
            Examples = ["recordtype is transaction", "customer record type", "type identifier"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        // ========== Financial Terms ==========

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "accounts receivable",
            Definition = "Money owed to the company by customers for goods or services delivered on credit",
            Synonyms = ["AR", "receivables", "customer debt", "amounts due"],
            Category = "accounting",
            Examples = ["AR aging", "accounts receivable balance", "customer receivables"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "accounts payable",
            Definition = "Money owed by the company to vendors for goods or services received on credit",
            Synonyms = ["AP", "payables", "vendor debt", "amounts owed"],
            Category = "accounting",
            Examples = ["AP aging", "accounts payable balance", "vendor payables"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "general ledger",
            Definition = "The master accounting record containing all financial transactions organized by account",
            Synonyms = ["GL", "ledger", "chart of accounts", "financial ledger"],
            Category = "accounting",
            Examples = ["GL account", "post to general ledger", "GL impact"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "journal entry",
            Definition = "A manual accounting transaction that directly debits and credits GL accounts",
            Synonyms = ["JE", "manual entry", "accounting entry", "GL entry"],
            Category = "recordtype",
            Examples = ["type is Journal Entry", "create journal entry", "JE for adjustment"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        // ========== Additional Useful Terms ==========

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "class",
            Definition = "A classification dimension for reporting, such as department, division, or business unit",
            Synonyms = ["department", "classification", "business unit", "segment"],
            Category = "fields",
            Examples = ["class is Marketing", "filter by class", "class dimension"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "location",
            Definition = "A physical location such as warehouse, store, or office used for inventory and reporting",
            Synonyms = ["warehouse", "site", "facility", "branch"],
            Category = "fields",
            Examples = ["location is Main Warehouse", "filter by location", "inventory location"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "currency",
            Definition = "The monetary unit used for a transaction, customer, or subsidiary",
            Synonyms = ["money", "denomination", "forex", "exchange"],
            Category = "fields",
            Examples = ["currency is USD", "multi-currency", "foreign currency"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        terms.Add(new GlossaryTerm
        {
            Id = Guid.NewGuid(),
            Term = "tax",
            Definition = "Sales tax, VAT, or other tax amounts applied to transactions",
            Synonyms = ["VAT", "sales tax", "tax amount", "taxation"],
            Category = "fields",
            Examples = ["tax amount", "tax code", "VAT rate", "calculate tax"],
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        return terms;
    }
}
