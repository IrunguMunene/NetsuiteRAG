using Microsoft.EntityFrameworkCore;
using NetSuiteRAG.Shared.Models;

namespace NetSuiteRAG.Api.Data;

/// <summary>
/// Seeds the database with standard NetSuite field definitions from the catalog.
/// This is a simplified seeder for Phase 1 - T1.01.
/// Full catalog import will be implemented in subsequent tasks.
/// </summary>
public static class FieldDefinitionSeeder
{
    /// <summary>
    /// Seeds essential field definitions for transaction record type.
    /// Based on the catalog in docs/NL_to_SavedSearch_Plan_and_Catalog_2025-1.md
    /// </summary>
    /// <param name="context">Database context.</param>
    /// <param name="logger">Logger instance.</param>
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        // Check if already seeded
        var existingCount = await context.FieldDefinitions.CountAsync();
        if (existingCount > 0)
        {
            logger.LogInformation(
                "Field definitions already seeded ({Count} records found). Skipping seed.",
                existingCount);
            return;
        }

        logger.LogInformation("Seeding field definitions from catalog...");

        var fields = GetSeedFields();

        await context.FieldDefinitions.AddRangeAsync(fields);
        await context.SaveChangesAsync();

        logger.LogInformation(
            "Successfully seeded {Count} field definitions",
            fields.Count);
    }

    /// <summary>
    /// Gets the initial seed data for standard transaction fields.
    /// This is a subset of commonly-used fields from TransactionSearchBasic and TransactionSearchRowBasic.
    /// </summary>
    private static List<FieldDefinition> GetSeedFields()
    {
        var source = "catalog-2025.1-seed";
        var schemaBase = "https://www.netsuite.com/help/helpcenter/en_US/srbrowser/Browser2025_1/schema/search/";

        return
        [
            // Transaction - Basic Fields (No Join Required)
            new FieldDefinition
            {
                Id = Guid.NewGuid(),
                RecordType = "transaction",
                FieldId = "trandate",
                Label = "Transaction Date",
                FieldType = FieldType.Date,
                Description = "The date of the transaction",
                IsCustomField = false,
                ValidOperators = OperatorMatrix.GetValidOperators(FieldType.Date),
                RequiredJoin = null,
                Aliases = ["date", "tran date"],
                SupportsSummary = true,
                SchemaUrl = $"{schemaBase}transactionsearchbasic.html",
                Source = source
            },
            new FieldDefinition
            {
                Id = Guid.NewGuid(),
                RecordType = "transaction",
                FieldId = "type",
                Label = "Transaction Type",
                FieldType = FieldType.List,
                Description = "Type of transaction (invoice, bill, payment, etc.)",
                IsCustomField = false,
                ValidOperators = OperatorMatrix.GetValidOperators(FieldType.List),
                RequiredJoin = null,
                Aliases = ["transaction type", "trantype"],
                SupportsSummary = true,
                SchemaUrl = $"{schemaBase}transactionsearchbasic.html",
                Source = source
            },
            new FieldDefinition
            {
                Id = Guid.NewGuid(),
                RecordType = "transaction",
                FieldId = "posting",
                Label = "Posting",
                FieldType = FieldType.Checkbox,
                Description = "Whether the transaction affects GL (posting=T for financial reports)",
                IsCustomField = false,
                ValidOperators = OperatorMatrix.GetValidOperators(FieldType.Checkbox),
                RequiredJoin = null,
                Aliases = ["is posting", "gl posting"],
                SupportsSummary = false,
                SchemaUrl = $"{schemaBase}transactionsearchbasic.html",
                Source = source
            },
            new FieldDefinition
            {
                Id = Guid.NewGuid(),
                RecordType = "transaction",
                FieldId = "amount",
                Label = "Amount",
                FieldType = FieldType.Currency,
                Description = "Transaction amount",
                IsCustomField = false,
                ValidOperators = OperatorMatrix.GetValidOperators(FieldType.Currency),
                RequiredJoin = null,
                Aliases = ["total", "transaction amount"],
                SupportsSummary = true,
                SchemaUrl = $"{schemaBase}transactionsearchrowbasic.html",
                Source = source
            },
            new FieldDefinition
            {
                Id = Guid.NewGuid(),
                RecordType = "transaction",
                FieldId = "entity",
                Label = "Entity",
                FieldType = FieldType.List,
                Description = "Customer or vendor associated with transaction",
                IsCustomField = false,
                ValidOperators = OperatorMatrix.GetValidOperators(FieldType.List),
                RequiredJoin = null,
                Aliases = ["customer", "vendor", "name"],
                SupportsSummary = true,
                SchemaUrl = $"{schemaBase}transactionsearchbasic.html",
                Source = source
            },
            new FieldDefinition
            {
                Id = Guid.NewGuid(),
                RecordType = "transaction",
                FieldId = "subsidiary",
                Label = "Subsidiary",
                FieldType = FieldType.List,
                Description = "Subsidiary where transaction was recorded (OneWorld only)",
                IsCustomField = false,
                ValidOperators = OperatorMatrix.GetValidOperators(FieldType.List),
                RequiredJoin = null,
                Aliases = ["sub", "company"],
                SupportsSummary = true,
                SchemaUrl = $"{schemaBase}transactionsearchbasic.html",
                Source = source
            },
            new FieldDefinition
            {
                Id = Guid.NewGuid(),
                RecordType = "transaction",
                FieldId = "memo",
                Label = "Memo",
                FieldType = FieldType.Text,
                Description = "Transaction memo/description",
                IsCustomField = false,
                ValidOperators = OperatorMatrix.GetValidOperators(FieldType.Text),
                RequiredJoin = null,
                Aliases = ["description", "notes"],
                SupportsSummary = false,
                SchemaUrl = $"{schemaBase}transactionsearchrowbasic.html",
                Source = source
            },
            new FieldDefinition
            {
                Id = Guid.NewGuid(),
                RecordType = "transaction",
                FieldId = "tranid",
                Label = "Transaction Number",
                FieldType = FieldType.Text,
                Description = "Transaction document number",
                IsCustomField = false,
                ValidOperators = OperatorMatrix.GetValidOperators(FieldType.Text),
                RequiredJoin = null,
                Aliases = ["transaction number", "document number", "number"],
                SupportsSummary = false,
                SchemaUrl = $"{schemaBase}transactionsearchrowbasic.html",
                Source = source
            },
            new FieldDefinition
            {
                Id = Guid.NewGuid(),
                RecordType = "transaction",
                FieldId = "status",
                Label = "Status",
                FieldType = FieldType.List,
                Description = "Transaction status (Open, Paid In Full, etc.)",
                IsCustomField = false,
                ValidOperators = OperatorMatrix.GetValidOperators(FieldType.List),
                RequiredJoin = null,
                Aliases = ["transaction status"],
                SupportsSummary = true,
                SchemaUrl = $"{schemaBase}transactionsearchbasic.html",
                Source = source
            },
            new FieldDefinition
            {
                Id = Guid.NewGuid(),
                RecordType = "transaction",
                FieldId = "department",
                Label = "Department",
                FieldType = FieldType.List,
                Description = "Department classification",
                IsCustomField = false,
                ValidOperators = OperatorMatrix.GetValidOperators(FieldType.List),
                RequiredJoin = null,
                Aliases = ["dept"],
                SupportsSummary = true,
                SchemaUrl = $"{schemaBase}transactionsearchbasic.html",
                Source = source
            },
            new FieldDefinition
            {
                Id = Guid.NewGuid(),
                RecordType = "transaction",
                FieldId = "class",
                Label = "Class",
                FieldType = FieldType.List,
                Description = "Class classification",
                IsCustomField = false,
                ValidOperators = OperatorMatrix.GetValidOperators(FieldType.List),
                RequiredJoin = null,
                Aliases = ["classification"],
                SupportsSummary = true,
                SchemaUrl = $"{schemaBase}transactionsearchbasic.html",
                Source = source
            },
            new FieldDefinition
            {
                Id = Guid.NewGuid(),
                RecordType = "transaction",
                FieldId = "location",
                Label = "Location",
                FieldType = FieldType.List,
                Description = "Location where transaction occurred",
                IsCustomField = false,
                ValidOperators = OperatorMatrix.GetValidOperators(FieldType.List),
                RequiredJoin = null,
                Aliases = ["loc"],
                SupportsSummary = true,
                SchemaUrl = $"{schemaBase}transactionsearchbasic.html",
                Source = source
            },

            // Transaction - Fields requiring joins
            new FieldDefinition
            {
                Id = Guid.NewGuid(),
                RecordType = "transaction",
                FieldId = "entityid",
                Label = "Entity ID",
                FieldType = FieldType.Text,
                Description = "Customer or vendor ID/name",
                IsCustomField = false,
                ValidOperators = OperatorMatrix.GetValidOperators(FieldType.Text),
                RequiredJoin = "customerJoin",
                Aliases = ["customer name", "vendor name"],
                SupportsSummary = true,
                SchemaUrl = $"{schemaBase}customersearchrowbasic.html",
                Source = source
            },
            new FieldDefinition
            {
                Id = Guid.NewGuid(),
                RecordType = "transaction",
                FieldId = "accountnumber",
                Label = "Account Number",
                FieldType = FieldType.Text,
                Description = "GL account number",
                IsCustomField = false,
                ValidOperators = OperatorMatrix.GetValidOperators(FieldType.Text),
                RequiredJoin = "accountJoin",
                Aliases = ["account", "gl account"],
                SupportsSummary = true,
                SchemaUrl = $"{schemaBase}accountsearchrowbasic.html",
                Source = source
            }
        ];
    }
}
