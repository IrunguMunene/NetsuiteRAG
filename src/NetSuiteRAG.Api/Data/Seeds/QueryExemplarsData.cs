using NetSuiteRAG.Shared.Models;

namespace NetSuiteRAG.Api.Data.Seeds;

/// <summary>
/// Seed data for query exemplars with SavedSearchPlan JSON solutions.
/// Contains 50 curated exemplars covering simple, moderate, and complex NetSuite queries.
/// </summary>
public static class QueryExemplarsData
{
    /// <summary>
    /// Gets all predefined query exemplars for seeding.
    /// </summary>
    public static List<QueryExemplar> GetQueryExemplars()
    {
        var now = DateTime.UtcNow;
        var exemplars = new List<QueryExemplar>();

        // ========== Simple Queries (15 examples) ==========

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Show me all open invoices",
            SavedSearchPlanJson = """
            {
              "recordType": "invoice",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "status", "operator": "anyof", "values": ["Open"] }
              ],
              "columns": [
                { "field": "tranid" },
                { "field": "trandate" },
                { "field": "entity" },
                { "field": "amount" },
                { "field": "status" }
              ]
            }
            """,
            Explanation = "Searches invoice transactions with mainline filter and status filter for Open invoices. Returns transaction number, date, customer, amount, and status.",
            Tags = ["invoice", "status", "open", "simple"],
            Difficulty = DifficultyLevel.Simple,
            RecordType = "invoice",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "List all customers",
            SavedSearchPlanJson = """
            {
              "recordType": "customer",
              "filters": [
                { "field": "isinactive", "operator": "is", "values": ["F"] }
              ],
              "columns": [
                { "field": "entityid" },
                { "field": "companyname" },
                { "field": "email" },
                { "field": "phone" }
              ]
            }
            """,
            Explanation = "Searches customer records excluding inactive customers. Returns customer ID, company name, email, and phone.",
            Tags = ["customer", "list", "simple"],
            Difficulty = DifficultyLevel.Simple,
            RecordType = "customer",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Show invoices from this month",
            SavedSearchPlanJson = """
            {
              "recordType": "invoice",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "trandate", "operator": "within", "values": ["TM"] }
              ],
              "columns": [
                { "field": "tranid" },
                { "field": "trandate" },
                { "field": "entity" },
                { "field": "amount" }
              ]
            }
            """,
            Explanation = "Searches invoices with transaction date within this month. Uses relative date filter 'TM' for 'this month'.",
            Tags = ["invoice", "date-range", "this-month", "simple"],
            Difficulty = DifficultyLevel.Simple,
            RecordType = "invoice",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Find all sales orders with amount greater than 10000",
            SavedSearchPlanJson = """
            {
              "recordType": "salesorder",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "amount", "operator": "greaterthan", "values": ["10000"] }
              ],
              "columns": [
                { "field": "tranid" },
                { "field": "trandate" },
                { "field": "entity" },
                { "field": "amount" },
                { "field": "status" }
              ]
            }
            """,
            Explanation = "Searches sales orders with amount greater than 10000. Uses numeric comparison operator.",
            Tags = ["salesorder", "amount", "filter", "simple"],
            Difficulty = DifficultyLevel.Simple,
            RecordType = "salesorder",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "List all vendors",
            SavedSearchPlanJson = """
            {
              "recordType": "vendor",
              "filters": [
                { "field": "isinactive", "operator": "is", "values": ["F"] }
              ],
              "columns": [
                { "field": "entityid" },
                { "field": "companyname" },
                { "field": "email" },
                { "field": "phone" }
              ]
            }
            """,
            Explanation = "Searches vendor records excluding inactive vendors. Returns vendor ID, company name, email, and phone.",
            Tags = ["vendor", "list", "simple"],
            Difficulty = DifficultyLevel.Simple,
            RecordType = "vendor",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Show me posted transactions from last quarter",
            SavedSearchPlanJson = """
            {
              "recordType": "transaction",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "posting", "operator": "is", "values": ["T"] },
                { "field": "trandate", "operator": "within", "values": ["LQ"] }
              ],
              "columns": [
                { "field": "type" },
                { "field": "tranid" },
                { "field": "trandate" },
                { "field": "entity" },
                { "field": "amount" }
              ]
            }
            """,
            Explanation = "Searches all posted transactions from last quarter. Uses posting filter for GL impact and relative date 'LQ' for last quarter.",
            Tags = ["transaction", "posting", "date-range", "simple"],
            Difficulty = DifficultyLevel.Simple,
            RecordType = "transaction",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Find purchase orders with status pending approval",
            SavedSearchPlanJson = """
            {
              "recordType": "purchaseorder",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "status", "operator": "anyof", "values": ["PurchOrd:A"] }
              ],
              "columns": [
                { "field": "tranid" },
                { "field": "trandate" },
                { "field": "entity" },
                { "field": "amount" },
                { "field": "status" }
              ]
            }
            """,
            Explanation = "Searches purchase orders with pending approval status. Uses internal status value 'PurchOrd:A'.",
            Tags = ["purchaseorder", "status", "approval", "simple"],
            Difficulty = DifficultyLevel.Simple,
            RecordType = "purchaseorder",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Show all items with quantity on hand greater than zero",
            SavedSearchPlanJson = """
            {
              "recordType": "item",
              "filters": [
                { "field": "isinactive", "operator": "is", "values": ["F"] },
                { "field": "quantityonhand", "operator": "greaterthan", "values": ["0"] }
              ],
              "columns": [
                { "field": "itemid" },
                { "field": "displayname" },
                { "field": "quantityonhand" },
                { "field": "type" }
              ]
            }
            """,
            Explanation = "Searches items with positive quantity on hand. Useful for inventory availability reports.",
            Tags = ["item", "inventory", "quantity", "simple"],
            Difficulty = DifficultyLevel.Simple,
            RecordType = "item",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "List journal entries from this fiscal year",
            SavedSearchPlanJson = """
            {
              "recordType": "journalentry",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "trandate", "operator": "within", "values": ["TFY"] }
              ],
              "columns": [
                { "field": "tranid" },
                { "field": "trandate" },
                { "field": "memo" },
                { "field": "posting" }
              ]
            }
            """,
            Explanation = "Searches journal entries from this fiscal year. Uses relative date 'TFY' for this fiscal year.",
            Tags = ["journalentry", "date-range", "fiscal-year", "simple"],
            Difficulty = DifficultyLevel.Simple,
            RecordType = "journalentry",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Show credit memos issued this year",
            SavedSearchPlanJson = """
            {
              "recordType": "creditmemo",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "trandate", "operator": "within", "values": ["TY"] }
              ],
              "columns": [
                { "field": "tranid" },
                { "field": "trandate" },
                { "field": "entity" },
                { "field": "amount" },
                { "field": "memo" }
              ]
            }
            """,
            Explanation = "Searches credit memos from this year. Credit memos represent refunds or credits to customers.",
            Tags = ["creditmemo", "date-range", "this-year", "simple"],
            Difficulty = DifficultyLevel.Simple,
            RecordType = "creditmemo",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Find all bills that are not paid yet",
            SavedSearchPlanJson = """
            {
              "recordType": "vendorbill",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "status", "operator": "anyof", "values": ["VendBill:A", "VendBill:B"] }
              ],
              "columns": [
                { "field": "tranid" },
                { "field": "trandate" },
                { "field": "entity" },
                { "field": "amount" },
                { "field": "duedate" },
                { "field": "status" }
              ]
            }
            """,
            Explanation = "Searches vendor bills with open or pending payment status. Useful for accounts payable management.",
            Tags = ["vendorbill", "status", "payable", "simple"],
            Difficulty = DifficultyLevel.Simple,
            RecordType = "vendorbill",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Show sales orders from last month",
            SavedSearchPlanJson = """
            {
              "recordType": "salesorder",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "trandate", "operator": "within", "values": ["LM"] }
              ],
              "columns": [
                { "field": "tranid" },
                { "field": "trandate" },
                { "field": "entity" },
                { "field": "amount" },
                { "field": "status" }
              ]
            }
            """,
            Explanation = "Searches sales orders from last month. Uses relative date 'LM' for last month.",
            Tags = ["salesorder", "date-range", "last-month", "simple"],
            Difficulty = DifficultyLevel.Simple,
            RecordType = "salesorder",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "List all employees",
            SavedSearchPlanJson = """
            {
              "recordType": "employee",
              "filters": [
                { "field": "isinactive", "operator": "is", "values": ["F"] }
              ],
              "columns": [
                { "field": "entityid" },
                { "field": "firstname" },
                { "field": "lastname" },
                { "field": "email" },
                { "field": "phone" }
              ]
            }
            """,
            Explanation = "Searches employee records excluding inactive employees. Returns employee ID, name, email, and phone.",
            Tags = ["employee", "list", "simple"],
            Difficulty = DifficultyLevel.Simple,
            RecordType = "employee",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Show me cash sales from today",
            SavedSearchPlanJson = """
            {
              "recordType": "cashsale",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "trandate", "operator": "within", "values": ["TD"] }
              ],
              "columns": [
                { "field": "tranid" },
                { "field": "trandate" },
                { "field": "entity" },
                { "field": "amount" }
              ]
            }
            """,
            Explanation = "Searches cash sales from today. Uses relative date 'TD' for today.",
            Tags = ["cashsale", "date-range", "today", "simple"],
            Difficulty = DifficultyLevel.Simple,
            RecordType = "cashsale",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Find all opportunities with status closed won",
            SavedSearchPlanJson = """
            {
              "recordType": "opportunity",
              "filters": [
                { "field": "status", "operator": "anyof", "values": ["Opport:G"] }
              ],
              "columns": [
                { "field": "title" },
                { "field": "entity" },
                { "field": "expectedclosedate" },
                { "field": "projectedtotal" },
                { "field": "status" }
              ]
            }
            """,
            Explanation = "Searches opportunities with closed won status. Useful for sales reporting. Uses internal status value 'Opport:G'.",
            Tags = ["opportunity", "status", "closed-won", "simple"],
            Difficulty = DifficultyLevel.Simple,
            RecordType = "opportunity",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        // ========== Moderate Queries (20 examples) ==========

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Show me all invoices for ACME Corp in Q4 2024",
            SavedSearchPlanJson = """
            {
              "recordType": "invoice",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "entity", "operator": "anyof", "values": ["ACME Corp"] },
                { "field": "trandate", "operator": "within", "values": ["2024-10-01", "2024-12-31"] }
              ],
              "columns": [
                { "field": "tranid" },
                { "field": "trandate" },
                { "field": "entity" },
                { "field": "amount" },
                { "field": "status" }
              ]
            }
            """,
            Explanation = "Searches invoices for specific customer in Q4 2024. Combines entity filter with absolute date range.",
            Tags = ["invoice", "customer", "date-range", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "invoice",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Total sales by customer this year",
            SavedSearchPlanJson = """
            {
              "recordType": "invoice",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "trandate", "operator": "within", "values": ["TY"] }
              ],
              "columns": [
                { "field": "entity", "summary": "GROUP" },
                { "field": "amount", "summary": "SUM" }
              ]
            }
            """,
            Explanation = "Summarizes invoice amounts by customer for this year. Uses GROUP and SUM for aggregation.",
            Tags = ["invoice", "summary", "customer", "aggregate", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "invoice",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Show invoices with customer email addresses",
            SavedSearchPlanJson = """
            {
              "recordType": "invoice",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] }
              ],
              "columns": [
                { "field": "tranid" },
                { "field": "trandate" },
                { "field": "entity" },
                { "field": "amount" },
                { "field": "email", "join": "customer" }
              ]
            }
            """,
            Explanation = "Searches invoices and joins to customer record to include email. Demonstrates customer join usage.",
            Tags = ["invoice", "customer-join", "email", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "invoice",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Count transactions by type this month",
            SavedSearchPlanJson = """
            {
              "recordType": "transaction",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "trandate", "operator": "within", "values": ["TM"] }
              ],
              "columns": [
                { "field": "type", "summary": "GROUP" },
                { "field": "internalid", "summary": "COUNT" }
              ]
            }
            """,
            Explanation = "Counts transactions by type for this month. Groups by transaction type and counts records.",
            Tags = ["transaction", "summary", "count", "type", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "transaction",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Show sales orders with item details",
            SavedSearchPlanJson = """
            {
              "recordType": "salesorder",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["F"] },
                { "field": "taxline", "operator": "is", "values": ["F"] },
                { "field": "cogs", "operator": "is", "values": ["F"] }
              ],
              "columns": [
                { "field": "tranid" },
                { "field": "trandate" },
                { "field": "entity" },
                { "field": "item" },
                { "field": "quantity" },
                { "field": "rate" },
                { "field": "amount" }
              ]
            }
            """,
            Explanation = "Searches sales order line items (not mainline). Filters out tax and COGS lines to show actual items sold.",
            Tags = ["salesorder", "line-items", "items", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "salesorder",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Average invoice amount by subsidiary this quarter",
            SavedSearchPlanJson = """
            {
              "recordType": "invoice",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "trandate", "operator": "within", "values": ["TQ"] }
              ],
              "columns": [
                { "field": "subsidiary", "summary": "GROUP" },
                { "field": "amount", "summary": "AVG" }
              ]
            }
            """,
            Explanation = "Calculates average invoice amount by subsidiary for this quarter. Groups by subsidiary with AVG aggregation.",
            Tags = ["invoice", "summary", "subsidiary", "average", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "invoice",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Show purchases from vendors in the electronics category",
            SavedSearchPlanJson = """
            {
              "recordType": "vendorbill",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "category", "join": "vendor", "operator": "anyof", "values": ["Electronics"] }
              ],
              "columns": [
                { "field": "tranid" },
                { "field": "trandate" },
                { "field": "entity" },
                { "field": "amount" },
                { "field": "category", "join": "vendor" }
              ]
            }
            """,
            Explanation = "Searches vendor bills filtered by vendor category. Uses vendor join to access vendor record fields.",
            Tags = ["vendorbill", "vendor-join", "category", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "vendorbill",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "List customers with total sales over 50000 this year",
            SavedSearchPlanJson = """
            {
              "recordType": "invoice",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "trandate", "operator": "within", "values": ["TY"] }
              ],
              "columns": [
                { "field": "entity", "summary": "GROUP" },
                { "field": "amount", "summary": "SUM" }
              ],
              "having": [
                { "field": "amount", "summary": "SUM", "operator": "greaterthan", "values": ["50000"] }
              ]
            }
            """,
            Explanation = "Groups invoices by customer with sum of amounts, then filters to customers with total over 50000. Uses HAVING clause for post-aggregation filtering.",
            Tags = ["invoice", "summary", "customer", "having", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "invoice",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Show overdue invoices with days past due",
            SavedSearchPlanJson = """
            {
              "recordType": "invoice",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "status", "operator": "anyof", "values": ["CustInvc:A"] },
                { "field": "duedate", "operator": "onorbefore", "values": ["today"] }
              ],
              "columns": [
                { "field": "tranid" },
                { "field": "entity" },
                { "field": "amount" },
                { "field": "duedate" },
                { "field": "daysoverdue" }
              ]
            }
            """,
            Explanation = "Searches open invoices with due date on or before today. Includes daysoverdue field to show aging.",
            Tags = ["invoice", "overdue", "aging", "duedate", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "invoice",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Monthly sales trend for this year",
            SavedSearchPlanJson = """
            {
              "recordType": "invoice",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "trandate", "operator": "within", "values": ["TY"] }
              ],
              "columns": [
                { "field": "trandate", "summary": "GROUP", "function": "MONTH" },
                { "field": "amount", "summary": "SUM" }
              ]
            }
            """,
            Explanation = "Groups invoices by month and sums amounts. Uses MONTH function on trandate for monthly grouping.",
            Tags = ["invoice", "summary", "monthly", "trend", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "invoice",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Show items sold with customer names",
            SavedSearchPlanJson = """
            {
              "recordType": "transaction",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["F"] },
                { "field": "taxline", "operator": "is", "values": ["F"] },
                { "field": "type", "operator": "anyof", "values": ["CustInvc", "CashSale"] }
              ],
              "columns": [
                { "field": "trandate" },
                { "field": "tranid" },
                { "field": "entity" },
                { "field": "item" },
                { "field": "quantity" },
                { "field": "amount" }
              ]
            }
            """,
            Explanation = "Searches transaction line items for invoices and cash sales. Shows items sold with customer information.",
            Tags = ["transaction", "line-items", "items", "customer", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "transaction",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Total expenses by class this fiscal year",
            SavedSearchPlanJson = """
            {
              "recordType": "transaction",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "type", "operator": "anyof", "values": ["VendBill", "ExpRept", "Check"] },
                { "field": "trandate", "operator": "within", "values": ["TFY"] }
              ],
              "columns": [
                { "field": "class", "summary": "GROUP" },
                { "field": "amount", "summary": "SUM" }
              ]
            }
            """,
            Explanation = "Summarizes expense transactions by class for this fiscal year. Includes vendor bills, expense reports, and checks.",
            Tags = ["transaction", "expenses", "class", "summary", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "transaction",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Show sales orders with customer subsidiary information",
            SavedSearchPlanJson = """
            {
              "recordType": "salesorder",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] }
              ],
              "columns": [
                { "field": "tranid" },
                { "field": "trandate" },
                { "field": "entity" },
                { "field": "amount" },
                { "field": "subsidiary", "join": "customer" }
              ]
            }
            """,
            Explanation = "Searches sales orders with customer subsidiary. Uses customer join to access the customer's subsidiary field.",
            Tags = ["salesorder", "customer-join", "subsidiary", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "salesorder",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Find purchase orders where amount differs from vendor bill",
            SavedSearchPlanJson = """
            {
              "recordType": "purchaseorder",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "status", "operator": "anyof", "values": ["PurchOrd:H"] }
              ],
              "columns": [
                { "field": "tranid" },
                { "field": "entity" },
                { "field": "amount" },
                { "field": "amountbilled" },
                { "field": "formulanumeric", "formula": "ABS({amount}-{amountbilled})" }
              ]
            }
            """,
            Explanation = "Searches fully billed purchase orders with calculated variance. Uses formula to compute absolute difference.",
            Tags = ["purchaseorder", "formula", "variance", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "purchaseorder",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Show items with inventory value by location",
            SavedSearchPlanJson = """
            {
              "recordType": "item",
              "filters": [
                { "field": "isinactive", "operator": "is", "values": ["F"] },
                { "field": "type", "operator": "anyof", "values": ["InvtPart"] }
              ],
              "columns": [
                { "field": "itemid" },
                { "field": "displayname" },
                { "field": "locationquantityonhand", "summary": "GROUP" },
                { "field": "locationaveragecost", "summary": "AVG" },
                { "field": "formulanumeric", "summary": "SUM", "formula": "{locationquantityonhand}*{locationaveragecost}" }
              ]
            }
            """,
            Explanation = "Calculates inventory value by location. Groups by item and location, then multiplies quantity by average cost.",
            Tags = ["item", "inventory", "location", "formula", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "item",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "List customers with no sales this year",
            SavedSearchPlanJson = """
            {
              "recordType": "customer",
              "filters": [
                { "field": "isinactive", "operator": "is", "values": ["F"] },
                { "field": "trandate", "join": "transaction", "operator": "notwithin", "values": ["TY"] }
              ],
              "columns": [
                { "field": "entityid" },
                { "field": "companyname" },
                { "field": "email" },
                { "field": "lastorderdate" }
              ]
            }
            """,
            Explanation = "Finds customers with no transactions this year. Uses transaction join with notwithin operator to exclude recent customers.",
            Tags = ["customer", "transaction-join", "inactive", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "customer",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Top 10 selling items by quantity this quarter",
            SavedSearchPlanJson = """
            {
              "recordType": "transaction",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["F"] },
                { "field": "taxline", "operator": "is", "values": ["F"] },
                { "field": "type", "operator": "anyof", "values": ["CustInvc", "CashSale"] },
                { "field": "trandate", "operator": "within", "values": ["TQ"] }
              ],
              "columns": [
                { "field": "item", "summary": "GROUP" },
                { "field": "quantity", "summary": "SUM" }
              ],
              "sortColumns": [
                { "field": "quantity", "summary": "SUM", "order": "DESC" }
              ],
              "maxResults": 10
            }
            """,
            Explanation = "Finds top 10 items by total quantity sold this quarter. Groups by item, sums quantity, sorts descending, limits to 10.",
            Tags = ["transaction", "items", "summary", "top-n", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "transaction",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Show payment applications to invoices",
            SavedSearchPlanJson = """
            {
              "recordType": "transaction",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["F"] },
                { "field": "type", "operator": "anyof", "values": ["CustPymt"] }
              ],
              "columns": [
                { "field": "trandate" },
                { "field": "tranid" },
                { "field": "entity" },
                { "field": "appliedtotransaction" },
                { "field": "amount" }
              ]
            }
            """,
            Explanation = "Searches customer payment line items showing which invoices were paid. Uses appliedtotransaction field.",
            Tags = ["payment", "invoice", "application", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "transaction",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Show sales orders that are partially fulfilled",
            SavedSearchPlanJson = """
            {
              "recordType": "salesorder",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "status", "operator": "anyof", "values": ["SalesOrd:D"] }
              ],
              "columns": [
                { "field": "tranid" },
                { "field": "entity" },
                { "field": "amount" },
                { "field": "quantityshiprecv" },
                { "field": "quantity" }
              ]
            }
            """,
            Explanation = "Searches sales orders with partially fulfilled status. Shows shipped vs ordered quantities. Uses status 'SalesOrd:D'.",
            Tags = ["salesorder", "fulfillment", "status", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "salesorder",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Calculate commission by sales rep this quarter",
            SavedSearchPlanJson = """
            {
              "recordType": "invoice",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "trandate", "operator": "within", "values": ["TQ"] }
              ],
              "columns": [
                { "field": "salesrep", "summary": "GROUP" },
                { "field": "amount", "summary": "SUM" },
                { "field": "formulapercent", "summary": "MAX", "formula": "0.05", "label": "Commission Rate" },
                { "field": "formulacurrency", "summary": "SUM", "formula": "{amount}*0.05", "label": "Commission" }
              ]
            }
            """,
            Explanation = "Calculates 5% commission for each sales rep based on invoice amounts this quarter. Uses formula fields for calculation.",
            Tags = ["invoice", "salesrep", "commission", "formula", "moderate"],
            Difficulty = DifficultyLevel.Moderate,
            RecordType = "invoice",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        // ========== Complex Queries (15 examples) ==========

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Show sales and costs by item with gross profit margin",
            SavedSearchPlanJson = """
            {
              "recordType": "transaction",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["F"] },
                { "field": "taxline", "operator": "is", "values": ["F"] },
                { "field": "cogs", "operator": "is", "values": ["F"] },
                { "field": "type", "operator": "anyof", "values": ["CustInvc", "CashSale"] },
                { "field": "trandate", "operator": "within", "values": ["TY"] }
              ],
              "columns": [
                { "field": "item", "summary": "GROUP" },
                { "field": "displayname", "join": "item", "summary": "GROUP" },
                { "field": "amount", "summary": "SUM", "label": "Revenue" },
                { "field": "costestimaterate", "summary": "SUM", "label": "Cost" },
                { "field": "formulacurrency", "summary": "SUM", "formula": "{amount}-{costestimaterate}", "label": "Gross Profit" },
                { "field": "formulapercent", "summary": "MAX", "formula": "({amount}-{costestimaterate})/{amount}", "label": "Margin %" }
              ]
            }
            """,
            Explanation = "Analyzes profitability by item. Calculates revenue, cost, gross profit, and margin percentage using multiple formula fields.",
            Tags = ["transaction", "items", "profitability", "formula", "complex"],
            Difficulty = DifficultyLevel.Complex,
            RecordType = "transaction",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Compare this year vs last year sales by month",
            SavedSearchPlanJson = """
            {
              "recordType": "invoice",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "trandate", "operator": "within", "values": ["TY", "LY"] }
              ],
              "columns": [
                { "field": "trandate", "summary": "GROUP", "function": "MONTH" },
                { "field": "formulacurrency", "summary": "SUM", "formula": "CASE WHEN {trandate} >= TO_DATE('2024-01-01') THEN {amount} ELSE 0 END", "label": "This Year" },
                { "field": "formulacurrency", "summary": "SUM", "formula": "CASE WHEN {trandate} < TO_DATE('2024-01-01') THEN {amount} ELSE 0 END", "label": "Last Year" },
                { "field": "formulapercent", "summary": "MAX", "formula": "(SUM(CASE WHEN {trandate} >= TO_DATE('2024-01-01') THEN {amount} ELSE 0 END) - SUM(CASE WHEN {trandate} < TO_DATE('2024-01-01') THEN {amount} ELSE 0 END)) / SUM(CASE WHEN {trandate} < TO_DATE('2024-01-01') THEN {amount} ELSE 0 END)", "label": "Growth %" }
              ]
            }
            """,
            Explanation = "Year-over-year sales comparison by month. Uses CASE expressions to separate this year and last year, then calculates growth percentage.",
            Tags = ["invoice", "yoy", "comparison", "formula", "complex"],
            Difficulty = DifficultyLevel.Complex,
            RecordType = "invoice",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Customer lifetime value with first and last purchase dates",
            SavedSearchPlanJson = """
            {
              "recordType": "transaction",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "type", "operator": "anyof", "values": ["CustInvc", "CashSale"] }
              ],
              "columns": [
                { "field": "entity", "summary": "GROUP" },
                { "field": "companyname", "join": "customer", "summary": "GROUP" },
                { "field": "amount", "summary": "SUM", "label": "Total Revenue" },
                { "field": "internalid", "summary": "COUNT", "label": "Transaction Count" },
                { "field": "trandate", "summary": "MIN", "label": "First Purchase" },
                { "field": "trandate", "summary": "MAX", "label": "Last Purchase" },
                { "field": "formulanumeric", "summary": "MAX", "formula": "ROUND(({today}-MIN({trandate}))/365, 1)", "label": "Customer Age (Years)" }
              ]
            }
            """,
            Explanation = "Calculates customer lifetime value metrics. Groups by customer with total revenue, transaction count, date ranges, and customer age in years.",
            Tags = ["transaction", "customer", "ltv", "summary", "complex"],
            Difficulty = DifficultyLevel.Complex,
            RecordType = "transaction",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Inventory turns by item class with days on hand",
            SavedSearchPlanJson = """
            {
              "recordType": "item",
              "filters": [
                { "field": "isinactive", "operator": "is", "values": ["F"] },
                { "field": "type", "operator": "anyof", "values": ["InvtPart"] }
              ],
              "columns": [
                { "field": "class", "summary": "GROUP" },
                { "field": "quantityonhand", "summary": "SUM" },
                { "field": "averagecost", "summary": "AVG" },
                { "field": "formulacurrency", "summary": "SUM", "formula": "{quantityonhand}*{averagecost}", "label": "Inventory Value" },
                { "field": "formulanumeric", "summary": "MAX", "formula": "365/({quantitysold}/12)", "label": "Days On Hand" }
              ],
              "having": [
                { "field": "quantityonhand", "summary": "SUM", "operator": "greaterthan", "values": ["0"] }
              ]
            }
            """,
            Explanation = "Analyzes inventory efficiency by item class. Calculates total value and days on hand based on monthly sales velocity. Uses HAVING to exclude zero inventory.",
            Tags = ["item", "inventory", "turns", "formula", "complex"],
            Difficulty = DifficultyLevel.Complex,
            RecordType = "item",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Accounts receivable aging by customer with buckets",
            SavedSearchPlanJson = """
            {
              "recordType": "invoice",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "status", "operator": "anyof", "values": ["CustInvc:A"] }
              ],
              "columns": [
                { "field": "entity", "summary": "GROUP" },
                { "field": "formulacurrency", "summary": "SUM", "formula": "CASE WHEN {daysoverdue} <= 30 THEN {amountremaining} ELSE 0 END", "label": "0-30 Days" },
                { "field": "formulacurrency", "summary": "SUM", "formula": "CASE WHEN {daysoverdue} > 30 AND {daysoverdue} <= 60 THEN {amountremaining} ELSE 0 END", "label": "31-60 Days" },
                { "field": "formulacurrency", "summary": "SUM", "formula": "CASE WHEN {daysoverdue} > 60 AND {daysoverdue} <= 90 THEN {amountremaining} ELSE 0 END", "label": "61-90 Days" },
                { "field": "formulacurrency", "summary": "SUM", "formula": "CASE WHEN {daysoverdue} > 90 THEN {amountremaining} ELSE 0 END", "label": "Over 90 Days" },
                { "field": "amountremaining", "summary": "SUM", "label": "Total Outstanding" }
              ]
            }
            """,
            Explanation = "Creates AR aging report with standard buckets. Uses CASE expressions to categorize outstanding amounts by days overdue.",
            Tags = ["invoice", "ar-aging", "aging-buckets", "formula", "complex"],
            Difficulty = DifficultyLevel.Complex,
            RecordType = "invoice",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Sales pipeline analysis by stage with win rate",
            SavedSearchPlanJson = """
            {
              "recordType": "opportunity",
              "filters": [
                { "field": "expectedclosedate", "operator": "within", "values": ["TY"] }
              ],
              "columns": [
                { "field": "stage", "summary": "GROUP" },
                { "field": "probability", "summary": "AVG", "label": "Avg Probability" },
                { "field": "internalid", "summary": "COUNT", "label": "Count" },
                { "field": "projectedtotal", "summary": "SUM", "label": "Total Value" },
                { "field": "formulacurrency", "summary": "SUM", "formula": "{projectedtotal}*{probability}/100", "label": "Weighted Value" },
                { "field": "formulapercent", "summary": "MAX", "formula": "COUNT(CASE WHEN {status} = 'Opport:G' THEN 1 END) / COUNT(*)", "label": "Win Rate" }
              ]
            }
            """,
            Explanation = "Analyzes sales pipeline by opportunity stage. Calculates weighted value based on probability and win rate for closed opportunities.",
            Tags = ["opportunity", "pipeline", "win-rate", "formula", "complex"],
            Difficulty = DifficultyLevel.Complex,
            RecordType = "opportunity",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Multi-currency revenue recognition by month and subsidiary",
            SavedSearchPlanJson = """
            {
              "recordType": "invoice",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "posting", "operator": "is", "values": ["T"] },
                { "field": "trandate", "operator": "within", "values": ["TY"] }
              ],
              "columns": [
                { "field": "subsidiary", "summary": "GROUP" },
                { "field": "trandate", "summary": "GROUP", "function": "MONTH" },
                { "field": "currency", "summary": "GROUP" },
                { "field": "amount", "summary": "SUM", "label": "Transaction Currency" },
                { "field": "fxamount", "summary": "SUM", "label": "Base Currency" },
                { "field": "formulanumeric", "summary": "AVG", "formula": "{fxamount}/{amount}", "label": "Avg FX Rate" }
              ]
            }
            """,
            Explanation = "Multi-dimensional revenue analysis. Groups by subsidiary, month, and currency. Shows amounts in both transaction and base currency with average exchange rate.",
            Tags = ["invoice", "multi-currency", "subsidiary", "summary", "complex"],
            Difficulty = DifficultyLevel.Complex,
            RecordType = "invoice",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Order to cash cycle time by customer segment",
            SavedSearchPlanJson = """
            {
              "recordType": "salesorder",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "status", "operator": "anyof", "values": ["SalesOrd:H"] },
                { "field": "closedate", "operator": "within", "values": ["TY"] }
              ],
              "columns": [
                { "field": "custentity_segment", "join": "customer", "summary": "GROUP", "label": "Customer Segment" },
                { "field": "internalid", "summary": "COUNT", "label": "Orders" },
                { "field": "formulanumeric", "summary": "AVG", "formula": "({closedate}-{trandate})", "label": "Avg Days to Close" },
                { "field": "formulanumeric", "summary": "MIN", "formula": "({closedate}-{trandate})", "label": "Min Days" },
                { "field": "formulanumeric", "summary": "MAX", "formula": "({closedate}-{trandate})", "label": "Max Days" }
              ]
            }
            """,
            Explanation = "Measures order-to-cash efficiency by customer segment. Calculates average, min, and max days from order to close for completed orders.",
            Tags = ["salesorder", "cycle-time", "customer", "formula", "complex"],
            Difficulty = DifficultyLevel.Complex,
            RecordType = "salesorder",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Vendor performance scorecard with on-time delivery rate",
            SavedSearchPlanJson = """
            {
              "recordType": "purchaseorder",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "status", "operator": "anyof", "values": ["PurchOrd:H"] },
                { "field": "trandate", "operator": "within", "values": ["TY"] }
              ],
              "columns": [
                { "field": "entity", "summary": "GROUP" },
                { "field": "companyname", "join": "vendor", "summary": "GROUP" },
                { "field": "internalid", "summary": "COUNT", "label": "Total POs" },
                { "field": "amount", "summary": "SUM", "label": "Total Spend" },
                { "field": "formulanumeric", "summary": "AVG", "formula": "({expectedreceiptdate}-{trandate})", "label": "Avg Lead Time" },
                { "field": "formulapercent", "summary": "MAX", "formula": "COUNT(CASE WHEN {actualshipdate} <= {expectedreceiptdate} THEN 1 END) / COUNT(*)", "label": "On-Time %" }
              ]
            }
            """,
            Explanation = "Evaluates vendor performance. Tracks total POs, spend, average lead time, and on-time delivery percentage for completed orders.",
            Tags = ["purchaseorder", "vendor", "performance", "formula", "complex"],
            Difficulty = DifficultyLevel.Complex,
            RecordType = "purchaseorder",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Product mix analysis with percentage of total sales",
            SavedSearchPlanJson = """
            {
              "recordType": "transaction",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["F"] },
                { "field": "taxline", "operator": "is", "values": ["F"] },
                { "field": "type", "operator": "anyof", "values": ["CustInvc", "CashSale"] },
                { "field": "trandate", "operator": "within", "values": ["TY"] }
              ],
              "columns": [
                { "field": "class", "join": "item", "summary": "GROUP", "label": "Product Category" },
                { "field": "amount", "summary": "SUM", "label": "Revenue" },
                { "field": "quantity", "summary": "SUM", "label": "Units Sold" },
                { "field": "formulapercent", "summary": "MAX", "formula": "SUM({amount}) / (SELECT SUM({amount}) FROM transaction WHERE {mainline} = 'F' AND {taxline} = 'F')", "label": "% of Total" },
                { "field": "formulacurrency", "summary": "AVG", "formula": "{amount}/{quantity}", "label": "Avg Unit Price" }
              ],
              "sortColumns": [
                { "field": "amount", "summary": "SUM", "order": "DESC" }
              ]
            }
            """,
            Explanation = "Analyzes product mix by category. Calculates revenue, units, percentage of total sales, and average unit price. Uses subquery for percentage calculation.",
            Tags = ["transaction", "product-mix", "items", "formula", "complex"],
            Difficulty = DifficultyLevel.Complex,
            RecordType = "transaction",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Cash flow projection from open AR and AP",
            SavedSearchPlanJson = """
            {
              "recordType": "transaction",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "type", "operator": "anyof", "values": ["CustInvc", "VendBill"] },
                { "field": "status", "operator": "noneof", "values": ["CustInvc:B", "VendBill:D"] }
              ],
              "columns": [
                { "field": "duedate", "summary": "GROUP", "function": "MONTH" },
                { "field": "formulacurrency", "summary": "SUM", "formula": "CASE WHEN {type} = 'CustInvc' THEN {amountremaining} ELSE 0 END", "label": "Expected Inflow" },
                { "field": "formulacurrency", "summary": "SUM", "formula": "CASE WHEN {type} = 'VendBill' THEN {amountremaining} ELSE 0 END", "label": "Expected Outflow" },
                { "field": "formulacurrency", "summary": "SUM", "formula": "CASE WHEN {type} = 'CustInvc' THEN {amountremaining} ELSE -{amountremaining} END", "label": "Net Cash Flow" }
              ],
              "sortColumns": [
                { "field": "duedate", "summary": "GROUP", "function": "MONTH", "order": "ASC" }
              ]
            }
            """,
            Explanation = "Projects cash flow by month from open receivables and payables. Calculates expected inflow from invoices and outflow from bills, grouped by due date month.",
            Tags = ["transaction", "cash-flow", "projection", "formula", "complex"],
            Difficulty = DifficultyLevel.Complex,
            RecordType = "transaction",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Customer churn analysis with last purchase recency",
            SavedSearchPlanJson = """
            {
              "recordType": "customer",
              "filters": [
                { "field": "isinactive", "operator": "is", "values": ["F"] }
              ],
              "columns": [
                { "field": "entityid" },
                { "field": "companyname" },
                { "field": "salesrep" },
                { "field": "datecreated", "label": "Customer Since" },
                { "field": "lastorderdate", "join": "transaction", "label": "Last Purchase" },
                { "field": "formulanumeric", "formula": "ROUND(({today}-{transaction.lastorderdate}), 0)", "label": "Days Since Purchase" },
                { "field": "formulatext", "formula": "CASE WHEN ({today}-{transaction.lastorderdate}) > 365 THEN 'At Risk' WHEN ({today}-{transaction.lastorderdate}) > 180 THEN 'Warning' ELSE 'Active' END", "label": "Status" }
              ],
              "sortColumns": [
                { "field": "lastorderdate", "join": "transaction", "order": "ASC" }
              ]
            }
            """,
            Explanation = "Identifies at-risk customers based on purchase recency. Calculates days since last order and categorizes customers as Active, Warning, or At Risk.",
            Tags = ["customer", "churn", "recency", "formula", "complex"],
            Difficulty = DifficultyLevel.Complex,
            RecordType = "customer",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Sales rep quota attainment with month-by-month progress",
            SavedSearchPlanJson = """
            {
              "recordType": "invoice",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "trandate", "operator": "within", "values": ["TY"] }
              ],
              "columns": [
                { "field": "salesrep", "summary": "GROUP" },
                { "field": "trandate", "summary": "GROUP", "function": "MONTH" },
                { "field": "amount", "summary": "SUM", "label": "Actual Sales" },
                { "field": "custentity_monthly_quota", "join": "salesrep", "summary": "MAX", "label": "Monthly Quota" },
                { "field": "formulacurrency", "summary": "MAX", "formula": "SUM({amount}) - {salesrep.custentity_monthly_quota}", "label": "Variance" },
                { "field": "formulapercent", "summary": "MAX", "formula": "SUM({amount}) / {salesrep.custentity_monthly_quota}", "label": "Attainment %" }
              ]
            }
            """,
            Explanation = "Tracks sales rep performance against quota by month. Compares actual sales to quota with variance and attainment percentage. Requires custom field for quota.",
            Tags = ["invoice", "salesrep", "quota", "formula", "complex"],
            Difficulty = DifficultyLevel.Complex,
            RecordType = "invoice",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Intercompany reconciliation by subsidiary pair",
            SavedSearchPlanJson = """
            {
              "recordType": "transaction",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["T"] },
                { "field": "intercotransaction", "operator": "is", "values": ["T"] },
                { "field": "trandate", "operator": "within", "values": ["TM"] }
              ],
              "columns": [
                { "field": "subsidiary", "summary": "GROUP", "label": "From Subsidiary" },
                { "field": "tosubsidiary", "summary": "GROUP", "label": "To Subsidiary" },
                { "field": "type", "summary": "GROUP" },
                { "field": "internalid", "summary": "COUNT", "label": "Transaction Count" },
                { "field": "amount", "summary": "SUM", "label": "Total Amount" },
                { "field": "formulacurrency", "summary": "SUM", "formula": "CASE WHEN {eliminationtransaction} IS NULL THEN {amount} ELSE 0 END", "label": "Uneliminated" }
              ]
            }
            """,
            Explanation = "Analyzes intercompany transactions by subsidiary pair. Identifies uneliminated transactions that need reconciliation. Useful for OneWorld accounts.",
            Tags = ["transaction", "intercompany", "subsidiary", "formula", "complex"],
            Difficulty = DifficultyLevel.Complex,
            RecordType = "transaction",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        exemplars.Add(new QueryExemplar
        {
            Id = Guid.NewGuid(),
            NaturalQuery = "Item demand forecast based on historical sales trends",
            SavedSearchPlanJson = """
            {
              "recordType": "transaction",
              "filters": [
                { "field": "mainline", "operator": "is", "values": ["F"] },
                { "field": "taxline", "operator": "is", "values": ["F"] },
                { "field": "type", "operator": "anyof", "values": ["CustInvc", "CashSale"] },
                { "field": "trandate", "operator": "within", "values": ["LY"] }
              ],
              "columns": [
                { "field": "item", "summary": "GROUP" },
                { "field": "displayname", "join": "item", "summary": "GROUP" },
                { "field": "quantity", "summary": "SUM", "label": "Total Sold Last Year" },
                { "field": "formulanumeric", "summary": "MAX", "formula": "ROUND(SUM({quantity})/12, 0)", "label": "Avg Monthly Demand" },
                { "field": "formulanumeric", "summary": "MAX", "formula": "ROUND(SUM({quantity})/12*3, 0)", "label": "Suggested Reorder Qty (3mo)" },
                { "field": "quantityonhand", "join": "item", "summary": "MAX", "label": "Current On Hand" },
                { "field": "formulatext", "summary": "MAX", "formula": "CASE WHEN {item.quantityonhand} < (SUM({quantity})/12) THEN 'Order Now' ELSE 'Sufficient' END", "label": "Action" }
              ],
              "sortColumns": [
                { "field": "quantity", "summary": "SUM", "order": "DESC" }
              ]
            }
            """,
            Explanation = "Forecasts demand based on last year's sales. Calculates average monthly demand, suggests reorder quantity, and flags items needing replenishment.",
            Tags = ["transaction", "inventory", "forecast", "formula", "complex"],
            Difficulty = DifficultyLevel.Complex,
            RecordType = "transaction",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        return exemplars;
    }
}
