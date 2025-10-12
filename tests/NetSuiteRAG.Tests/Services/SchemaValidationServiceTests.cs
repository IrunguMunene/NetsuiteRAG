using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NetSuiteRAG.Api.Services.Implementations;
using NetSuiteRAG.Api.Services.Interfaces;
using NetSuiteRAG.Shared.Models;

namespace NetSuiteRAG.Tests.Services;

/// <summary>
/// Tests for SchemaValidationService focusing on Layer 4 business guardrails validation.
/// </summary>
public class SchemaValidationServiceTests
{
    private readonly Mock<IFieldDictionaryService> _mockFieldDictionary;
    private readonly Mock<ILogger<SchemaValidationService>> _mockLogger;
    private readonly SchemaValidationService _service;

    public SchemaValidationServiceTests()
    {
        _mockFieldDictionary = new Mock<IFieldDictionaryService>();
        _mockLogger = new Mock<ILogger<SchemaValidationService>>();
        _service = new SchemaValidationService(_mockFieldDictionary.Object, _mockLogger.Object);

        // Setup default field dictionary responses
        SetupDefaultFieldDictionary();
    }

    private void SetupDefaultFieldDictionary()
    {
        // Comprehensive field setup for transaction types
        var transactionFields = CreateTransactionFields();

        // Setup for customer record type
        var customerFields = CreateCustomerFields();

        // Setup mocks for all record types used in tests
        _mockFieldDictionary
            .Setup(x => x.GetFieldsForRecordTypeAsync("invoice", It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionFields);

        _mockFieldDictionary
            .Setup(x => x.GetFieldsForRecordTypeAsync("salesorder", It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionFields);

        _mockFieldDictionary
            .Setup(x => x.GetFieldsForRecordTypeAsync("cashsale", It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionFields);

        _mockFieldDictionary
            .Setup(x => x.GetFieldsForRecordTypeAsync("bill", It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionFields);

        _mockFieldDictionary
            .Setup(x => x.GetFieldsForRecordTypeAsync("journalentry", It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionFields);

        _mockFieldDictionary
            .Setup(x => x.GetFieldsForRecordTypeAsync("customer", It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerFields);
    }

    private static List<FieldDefinition> CreateTransactionFields()
    {
        var transactionFields = new List<FieldDefinition>
        {
            new()
            {
                RecordType = "transaction",
                FieldId = "trandate",
                Label = "Transaction Date",
                FieldType = FieldType.Date,
                ValidOperators = [OperatorType.Is, OperatorType.Between, OperatorType.Within, OperatorType.On, OperatorType.Before, OperatorType.After],
                Source = "test"
            },
            new()
            {
                RecordType = "transaction",
                FieldId = "startdate",
                Label = "Start Date",
                FieldType = FieldType.Date,
                ValidOperators = [OperatorType.Is, OperatorType.Between, OperatorType.Within, OperatorType.On, OperatorType.Before, OperatorType.After],
                Source = "test"
            },
            new()
            {
                RecordType = "transaction",
                FieldId = "enddate",
                Label = "End Date",
                FieldType = FieldType.Date,
                ValidOperators = [OperatorType.Is, OperatorType.Between, OperatorType.Within, OperatorType.On, OperatorType.Before, OperatorType.After],
                Source = "test"
            },
            new()
            {
                RecordType = "transaction",
                FieldId = "createddate",
                Label = "Date Created",
                FieldType = FieldType.Date,
                ValidOperators = [OperatorType.Is, OperatorType.Between, OperatorType.Within, OperatorType.On, OperatorType.Before, OperatorType.After],
                Source = "test"
            },
            new()
            {
                RecordType = "transaction",
                FieldId = "lastmodifieddate",
                Label = "Last Modified Date",
                FieldType = FieldType.Date,
                ValidOperators = [OperatorType.Is, OperatorType.Between, OperatorType.Within, OperatorType.On, OperatorType.Before, OperatorType.After],
                Source = "test"
            },
            new()
            {
                RecordType = "transaction",
                FieldId = "duedate",
                Label = "Due Date",
                FieldType = FieldType.Date,
                ValidOperators = [OperatorType.Is, OperatorType.Between, OperatorType.Within, OperatorType.On, OperatorType.Before, OperatorType.After],
                Source = "test"
            },
            new()
            {
                RecordType = "transaction",
                FieldId = "posting",
                Label = "Posting",
                FieldType = FieldType.Checkbox,
                ValidOperators = [OperatorType.Is],
                Source = "test"
            },
            new()
            {
                RecordType = "transaction",
                FieldId = "subsidiary",
                Label = "Subsidiary",
                FieldType = FieldType.List,
                ValidOperators = [OperatorType.Is, OperatorType.AnyOf, OperatorType.NoneOf],
                Source = "test"
            },
            new()
            {
                RecordType = "transaction",
                FieldId = "entity",
                Label = "Name",
                FieldType = FieldType.List,
                ValidOperators = [OperatorType.Is, OperatorType.AnyOf, OperatorType.NoneOf],
                Source = "test"
            },
            new()
            {
                RecordType = "transaction",
                FieldId = "amount",
                Label = "Amount",
                FieldType = FieldType.Currency,
                ValidOperators = [OperatorType.Is, OperatorType.GreaterThan, OperatorType.LessThan, OperatorType.Between],
                Source = "test"
            },
            new()
            {
                RecordType = "transaction",
                FieldId = "memo",
                Label = "Memo",
                FieldType = FieldType.Text,
                ValidOperators = [OperatorType.Is, OperatorType.Contains, OperatorType.StartsWith],
                Source = "test"
            }
        };

        // Add 60 mock fields for column cap testing
        for (int i = 1; i <= 60; i++)
        {
            transactionFields.Add(new()
            {
                RecordType = "transaction",
                FieldId = $"field{i}",
                Label = $"Field {i}",
                FieldType = FieldType.Text,
                ValidOperators = [OperatorType.Is],
                Source = "test"
            });
        }

        return transactionFields;
    }

    private static List<FieldDefinition> CreateCustomerFields()
    {
        var fields = new List<FieldDefinition>
        {
            new()
            {
                RecordType = "customer",
                FieldId = "entityid",
                Label = "Name",
                FieldType = FieldType.Text,
                ValidOperators = [OperatorType.Is, OperatorType.Contains, OperatorType.StartsWith],
                Source = "test"
            },
            new()
            {
                RecordType = "customer",
                FieldId = "email",
                Label = "Email",
                FieldType = FieldType.Email,
                ValidOperators = [OperatorType.Is, OperatorType.Contains],
                Source = "test"
            },
            new()
            {
                RecordType = "customer",
                FieldId = "phone",
                Label = "Phone",
                FieldType = FieldType.Phone,
                ValidOperators = [OperatorType.Is, OperatorType.Contains],
                Source = "test"
            }
        };

        // Add 60 mock fields for column cap testing
        for (int i = 1; i <= 60; i++)
        {
            fields.Add(new()
            {
                RecordType = "customer",
                FieldId = $"field{i}",
                Label = $"Field {i}",
                FieldType = FieldType.Text,
                ValidOperators = [OperatorType.Is],
                Source = "test"
            });
        }

        return fields;
    }

    #region Layer 4: Transaction Posting Validation

    [Fact]
    public void ValidateSchema_TransactionWithoutPosting_ReturnsError()
    {
        // Arrange
        var plan = new SavedSearchPlan
        {
            RecordType = "invoice",
            Filters =
            [
                new Filter { Field = "trandate", Operator = OperatorType.Within, Value = new[] { "2024-01-01", "2024-12-31" } }
            ],
            Columns =
            [
                new Column { Field = "trandate", Label = "Date" }
            ]
        };

        // Act
        var result = _service.ValidateSchema(plan);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorCode == "MISSING_POSTING_FILTER");
        result.Errors.First(e => e.ErrorCode == "MISSING_POSTING_FILTER")
            .Message.Should().Contain("posting=true filter");
    }

    [Fact]
    public void ValidateSchema_TransactionWithPosting_Passes()
    {
        // Arrange
        var plan = new SavedSearchPlan
        {
            RecordType = "invoice",
            Filters =
            [
                new Filter { Field = "trandate", Operator = OperatorType.Within, Value = new[] { "2024-01-01", "2024-12-31" } },
                new Filter { Field = "posting", Operator = OperatorType.Is, Value = true }
            ],
            Columns =
            [
                new Column { Field = "trandate", Label = "Date" }
            ]
        };

        // Act
        var result = _service.ValidateSchema(plan);

        // Debug: Print all errors if validation fails
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Error: {error.ErrorCode} - {error.Message}");
            }
        }

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => e.ErrorCode == "MISSING_POSTING_FILTER");
    }

    [Theory]
    [InlineData("salesorder")]
    [InlineData("invoice")]
    [InlineData("cashsale")]
    [InlineData("bill")]
    [InlineData("journalentry")]
    public void ValidateSchema_VariousTransactionTypes_RequirePosting(string recordType)
    {
        // Arrange
        var plan = new SavedSearchPlan
        {
            RecordType = recordType,
            Filters = [],
            Columns = [new Column { Field = "trandate", Label = "Date" }]
        };

        // Act
        var result = _service.ValidateSchema(plan);

        // Assert
        result.Errors.Should().Contain(e => e.ErrorCode == "MISSING_POSTING_FILTER");
    }

    [Fact]
    public void ValidateSchema_NonTransactionType_DoesNotRequirePosting()
    {
        // Arrange
        var plan = new SavedSearchPlan
        {
            RecordType = "customer",
            Filters = [],
            Columns = [new Column { Field = "entityid", Label = "Name" }]
        };

        // Act
        var result = _service.ValidateSchema(plan);

        // Assert
        result.Errors.Should().NotContain(e => e.ErrorCode == "MISSING_POSTING_FILTER");
    }

    #endregion

    #region Layer 4: Bounded Period Validation

    [Fact]
    public void ValidateSchema_DateRangeExceeds5Years_ReturnsError()
    {
        // Arrange
        var plan = new SavedSearchPlan
        {
            RecordType = "invoice",
            Filters =
            [
                new Filter { Field = "posting", Operator = OperatorType.Is, Value = true },
                new Filter { Field = "trandate", Operator = OperatorType.Between, Value = new[] { "2018-01-01", "2024-12-31" } } // 7 years
            ],
            Columns = [new Column { Field = "trandate", Label = "Date" }]
        };

        // Act
        var result = _service.ValidateSchema(plan);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorCode == "UNBOUNDED_DATE_RANGE");
        var error = result.Errors.First(e => e.ErrorCode == "UNBOUNDED_DATE_RANGE");
        error.Message.Should().Contain("exceeds 5 years");
        error.Details.Should().ContainKey("yearsDifference");
    }

    [Fact]
    public void ValidateSchema_DateRangeWithin5Years_Passes()
    {
        // Arrange
        var plan = new SavedSearchPlan
        {
            RecordType = "invoice",
            Filters =
            [
                new Filter { Field = "posting", Operator = OperatorType.Is, Value = true },
                new Filter { Field = "trandate", Operator = OperatorType.Between, Value = new[] { "2022-01-01", "2024-12-31" } } // 3 years
            ],
            Columns = [new Column { Field = "trandate", Label = "Date" }]
        };

        // Act
        var result = _service.ValidateSchema(plan);

        // Assert
        result.Errors.Should().NotContain(e => e.ErrorCode == "UNBOUNDED_DATE_RANGE");
    }

    [Fact]
    public void ValidateSchema_DateRangeExactly5Years_Passes()
    {
        // Arrange
        var plan = new SavedSearchPlan
        {
            RecordType = "invoice",
            Filters =
            [
                new Filter { Field = "posting", Operator = OperatorType.Is, Value = true },
                new Filter { Field = "trandate", Operator = OperatorType.Between, Value = new[] { "2020-01-01", "2024-12-31" } } // Just under 5 years
            ],
            Columns = [new Column { Field = "trandate", Label = "Date" }]
        };

        // Act
        var result = _service.ValidateSchema(plan);

        // Assert
        result.Errors.Should().NotContain(e => e.ErrorCode == "UNBOUNDED_DATE_RANGE");
    }

    [Theory]
    [InlineData("trandate")]
    [InlineData("startdate")]
    [InlineData("enddate")]
    [InlineData("createddate")]
    [InlineData("duedate")]
    public void ValidateSchema_VariousDateFields_ValidateBounds(string fieldName)
    {
        // Arrange
        var plan = new SavedSearchPlan
        {
            RecordType = "invoice",
            Filters =
            [
                new Filter { Field = "posting", Operator = OperatorType.Is, Value = true },
                new Filter { Field = fieldName, Operator = OperatorType.Between, Value = new[] { "2015-01-01", "2024-12-31" } } // 10 years
            ],
            Columns = [new Column { Field = "trandate", Label = "Date" }]
        };

        // Act
        var result = _service.ValidateSchema(plan);

        // Assert
        result.Errors.Should().Contain(e => e.ErrorCode == "UNBOUNDED_DATE_RANGE");
    }

    #endregion

    #region Layer 4: Subsidiary Scope Validation

    [Fact]
    public void ValidateSchema_TransactionWithoutSubsidiary_ReturnsWarning()
    {
        // Arrange
        var plan = new SavedSearchPlan
        {
            RecordType = "invoice",
            Filters =
            [
                new Filter { Field = "posting", Operator = OperatorType.Is, Value = true }
            ],
            Columns = [new Column { Field = "trandate", Label = "Date" }]
        };

        // Act
        var result = _service.ValidateSchema(plan);

        // Assert
        result.IsValid.Should().BeTrue(); // Warnings don't fail validation
        result.Warnings.Should().NotBeNull();
        result.Warnings.Should().Contain(w => w.WarningCode == "MISSING_SUBSIDIARY_FILTER");
    }

    [Fact]
    public void ValidateSchema_TransactionWithSubsidiary_Passes()
    {
        // Arrange
        var plan = new SavedSearchPlan
        {
            RecordType = "invoice",
            Filters =
            [
                new Filter { Field = "posting", Operator = OperatorType.Is, Value = true },
                new Filter { Field = "subsidiary", Operator = OperatorType.AnyOf, Value = new[] { "1", "2" } }
            ],
            Columns = [new Column { Field = "trandate", Label = "Date" }]
        };

        // Act
        var result = _service.ValidateSchema(plan);

        // Assert
        result.Errors.Should().NotContain(e => e.ErrorCode == "MISSING_SUBSIDIARY_FILTER");
    }

    #endregion

    #region Layer 4: Column Cap Validation

    [Fact]
    public void ValidateSchema_MoreThan50Columns_ReturnsError()
    {
        // Arrange
        var columns = new List<Column>();
        for (int i = 1; i <= 51; i++)
        {
            columns.Add(new Column { Field = $"field{i}", Label = $"Field {i}" });
        }

        var plan = new SavedSearchPlan
        {
            RecordType = "customer",
            Filters = [],
            Columns = columns
        };

        // Act
        var result = _service.ValidateSchema(plan);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorCode == "COLUMN_CAP_EXCEEDED");
        var error = result.Errors.First(e => e.ErrorCode == "COLUMN_CAP_EXCEEDED");
        error.Message.Should().Contain("51");
        error.Message.Should().Contain("50");
        error.Details.Should().ContainKey("excess");
    }

    [Fact]
    public void ValidateSchema_Exactly50Columns_Passes()
    {
        // Arrange
        var columns = new List<Column>();
        for (int i = 1; i <= 50; i++)
        {
            columns.Add(new Column { Field = $"field{i}", Label = $"Field {i}" });
        }

        var plan = new SavedSearchPlan
        {
            RecordType = "customer",
            Filters = [],
            Columns = columns
        };

        // Act
        var result = _service.ValidateSchema(plan);

        // Assert
        result.Errors.Should().NotContain(e => e.ErrorCode == "COLUMN_CAP_EXCEEDED");
    }

    [Fact]
    public void ValidateSchema_LessThan50Columns_Passes()
    {
        // Arrange
        var plan = new SavedSearchPlan
        {
            RecordType = "customer",
            Filters = [],
            Columns =
            [
                new Column { Field = "field1", Label = "Field 1" },
                new Column { Field = "field2", Label = "Field 2" }
            ]
        };

        // Act
        var result = _service.ValidateSchema(plan);

        // Assert
        result.Errors.Should().NotContain(e => e.ErrorCode == "COLUMN_CAP_EXCEEDED");
    }

    #endregion

    #region Layer 4: Row Cap Validation

    [Fact]
    public void ValidateSchema_MaxResultsExceeds2Million_ReturnsError()
    {
        // Arrange
        var plan = new SavedSearchPlan
        {
            RecordType = "customer",
            Filters = [],
            Columns = [new Column { Field = "entityid", Label = "Name" }],
            MaxResults = 2_500_000
        };

        // Act
        var result = _service.ValidateSchema(plan);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorCode == "ROW_CAP_EXCEEDED");
        var error = result.Errors.First(e => e.ErrorCode == "ROW_CAP_EXCEEDED");
        error.Message.Should().Contain("2,500,000");
        error.Message.Should().Contain("2,000,000");
        error.Details.Should().ContainKey("excess");
    }

    [Fact]
    public void ValidateSchema_MaxResultsExactly2Million_Passes()
    {
        // Arrange
        var plan = new SavedSearchPlan
        {
            RecordType = "customer",
            Filters = [],
            Columns = [new Column { Field = "entityid", Label = "Name" }],
            MaxResults = 2_000_000
        };

        // Act
        var result = _service.ValidateSchema(plan);

        // Assert
        result.Errors.Should().NotContain(e => e.ErrorCode == "ROW_CAP_EXCEEDED");
    }

    [Fact]
    public void ValidateSchema_MaxResultsUnder2Million_Passes()
    {
        // Arrange
        var plan = new SavedSearchPlan
        {
            RecordType = "customer",
            Filters = [],
            Columns = [new Column { Field = "entityid", Label = "Name" }],
            MaxResults = 1000
        };

        // Act
        var result = _service.ValidateSchema(plan);

        // Assert
        result.Errors.Should().NotContain(e => e.ErrorCode == "ROW_CAP_EXCEEDED");
    }

    [Fact]
    public void ValidateSchema_NoMaxResults_Passes()
    {
        // Arrange
        var plan = new SavedSearchPlan
        {
            RecordType = "customer",
            Filters = [],
            Columns = [new Column { Field = "entityid", Label = "Name" }]
        };

        // Act
        var result = _service.ValidateSchema(plan);

        // Assert
        result.Errors.Should().NotContain(e => e.ErrorCode == "ROW_CAP_EXCEEDED");
    }

    #endregion

    #region Integration Tests: Multiple Layer 4 Violations

    [Fact]
    public void ValidateSchema_MultipleLayer4Violations_ReturnsAllErrors()
    {
        // Arrange
        var columns = new List<Column>();
        for (int i = 1; i <= 60; i++)
        {
            columns.Add(new Column { Field = $"field{i}", Label = $"Field {i}" });
        }

        var plan = new SavedSearchPlan
        {
            RecordType = "invoice",
            Filters =
            [
                // Missing posting filter
                new Filter { Field = "trandate", Operator = OperatorType.Between, Value = new[] { "2010-01-01", "2024-12-31" } } // > 5 years
                // Missing subsidiary filter
            ],
            Columns = columns, // > 50 columns
            MaxResults = 3_000_000 // > 2M
        };

        // Act
        var result = _service.ValidateSchema(plan);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "MISSING_POSTING_FILTER");
        result.Errors.Should().Contain(e => e.ErrorCode == "UNBOUNDED_DATE_RANGE");
        result.Warnings.Should().NotBeNull();
        result.Warnings.Should().Contain(w => w.WarningCode == "MISSING_SUBSIDIARY_FILTER");
        result.Errors.Should().Contain(e => e.ErrorCode == "COLUMN_CAP_EXCEEDED");
        result.Errors.Should().Contain(e => e.ErrorCode == "ROW_CAP_EXCEEDED");
    }

    [Fact]
    public void ValidateSchema_AllLayer4RulesMet_Passes()
    {
        // Arrange
        var plan = new SavedSearchPlan
        {
            RecordType = "invoice",
            Filters =
            [
                new Filter { Field = "posting", Operator = OperatorType.Is, Value = true },
                new Filter { Field = "trandate", Operator = OperatorType.Between, Value = new[] { "2023-01-01", "2024-12-31" } }, // < 5 years
                new Filter { Field = "subsidiary", Operator = OperatorType.AnyOf, Value = new[] { "1" } }
            ],
            Columns =
            [
                new Column { Field = "trandate", Label = "Date" },
                new Column { Field = "entity", Label = "Customer" },
                new Column { Field = "amount", Label = "Amount" }
            ], // < 50 columns
            MaxResults = 10000 // < 2M
        };

        // Act
        var result = _service.ValidateSchema(plan);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Message.Should().Contain("all 4 layers passed");
    }

    #endregion
}
