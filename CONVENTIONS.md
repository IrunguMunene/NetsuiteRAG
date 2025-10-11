# Coding Conventions

**Last Updated:** 2025-10-10

---

## C# Standards

### Language Features (C# 13)

**Primary Constructors:**
```csharp
// Use for simple classes
public class FieldService(
    IDistributedCache cache,
    ILogger<FieldService> logger) : IFieldService
{
    public async Task<Field?> GetAsync(string id)
    {
        logger.LogInformation("Fetching field {FieldId}", id);
        // Use cache and logger directly - no need for fields
    }
}
```

**Collection Expressions:**
```csharp
// Modern way
List<string> fields = ["trandate", "amount", "entity"];

// Old way (avoid)
List<string> fields = new List<string> { "trandate", "amount", "entity" };
```

**Required Properties:**
```csharp
public class SavedSearchPlan
{
    public required string RecordType { get; init; }
    public required List<Filter> Filters { get; init; }
    public List<Column>? Columns { get; init; }
}
```

**File-Scoped Namespaces:**
```csharp
namespace NetSuiteRAG.Api.Services;

public class QueryService { }
```

---

## Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| Classes | PascalCase | `FieldDictionaryService` |
| Interfaces | IPascalCase | `IFieldDictionaryService` |
| Methods | PascalCase | `GetFieldAsync` |
| Parameters | camelCase | `recordType`, `fieldId` |
| Private fields | _camelCase | `_cache`, `_logger` |
| Local variables | camelCase | `result`, `query` |
| Constants | PascalCase | `MaxRetries`, `DefaultTimeout` |
| Properties | PascalCase | `RecordType`, `FieldId` |

---

## Project Structure

```
src/NetSuiteRAG.Api/
├── Controllers/              # API endpoints
│   ├── QueriesController.cs
│   └── TemplatesController.cs
├── Services/
│   ├── Interfaces/           # Service contracts
│   │   ├── IFieldService.cs
│   │   └── IQueryService.cs
│   └── Implementations/      # Service implementations
│       ├── FieldService.cs
│       └── QueryService.cs
├── Models/                   # DTOs and request/response models
│   ├── Requests/
│   └── Responses/
├── Data/                     # Database context and repositories
│   ├── AppDbContext.cs
│   └── Migrations/
├── Validators/               # FluentValidation validators
└── Extensions/               # Extension methods

src/NetSuiteRAG.Shared/
├── Models/                   # Domain models
│   ├── Query.cs
│   ├── Template.cs
│   └── SavedSearchPlan.cs
└── Interfaces/               # Shared contracts

tests/NetSuiteRAG.Api.Tests/
├── Services/                 # Service tests
├── Controllers/              # Controller tests
└── Integration/              # End-to-end tests
```

---

## Design Patterns

### Result Pattern (Instead of Exceptions)

```csharp
// Result type
public record Result<T>(
    bool IsSuccess,
    T? Value,
    string? Error
)
{
    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}

// Usage in service
public async Task<Result<SavedSearchPlan>> ValidateAsync(SavedSearchPlan plan)
{
    if (!IsValid(plan))
        return Result<SavedSearchPlan>.Failure("Invalid plan structure");
    
    return Result<SavedSearchPlan>.Success(plan);
}

// Usage in controller
var result = await _service.ValidateAsync(plan);
if (!result.IsSuccess)
    return BadRequest(new { error = result.Error });

return Ok(result.Value);
```

### Service Pattern

```csharp
// Interface
public interface IFieldDictionaryService
{
    Task<FieldDefinition?> GetFieldAsync(string recordType, string fieldId);
    Task<List<FieldDefinition>> SearchFieldsAsync(string recordType, string query);
}

// Implementation with primary constructor
public class FieldDictionaryService(
    IDistributedCache cache,
    ILogger<FieldDictionaryService> logger) : IFieldDictionaryService
{
    public async Task<FieldDefinition?> GetFieldAsync(
        string recordType, 
        string fieldId)
    {
        logger.LogInformation(
            "Fetching field {FieldId} for {RecordType}", 
            fieldId, 
            recordType
        );
        
        // Implementation
        return null;
    }
}
```

### Repository Pattern (If Needed)

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}
```

---

## API Conventions

### Endpoint Structure

```
/api/{resource}                    # Collection
/api/{resource}/{id}               # Single item
/api/{resource}/{id}/{action}      # Action on item
/api/{resource}/{id}/{subresource} # Nested resource
```

**Examples:**
```
GET    /api/queries                # Get all queries
POST   /api/queries                # Create query
GET    /api/queries/{id}           # Get specific query
GET    /api/queries/{id}/stream    # Stream query results
POST   /api/queries/{id}/resolve   # Resolve disambiguation
DELETE /api/queries/{id}           # Delete query

GET    /api/templates              # Get all templates
POST   /api/templates              # Create template
PUT    /api/templates/{id}         # Update template
```

### HTTP Status Codes

| Code | Meaning | When to Use |
|------|---------|-------------|
| 200 | OK | Successful GET, PUT, DELETE |
| 201 | Created | Successful POST |
| 204 | No Content | Successful DELETE with no response body |
| 400 | Bad Request | Validation error, malformed request |
| 401 | Unauthorized | Authentication required |
| 403 | Forbidden | Authenticated but no permission |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Disambiguation needed, business rule conflict |
| 422 | Unprocessable Entity | Semantic validation failed |
| 500 | Internal Server Error | Unexpected server error |

### Response Format

**Success Response:**
```json
{
  "data": { 
    "id": "123",
    "name": "Query 1"
  },
  "meta": {
    "timestamp": "2025-10-10T10:30:00Z"
  }
}
```

**Error Response:**
```json
{
  "error": {
    "code": "VALIDATION_FAILED",
    "message": "Invalid query plan",
    "details": [
      {
        "field": "recordType",
        "message": "RecordType is required"
      }
    ]
  }
}
```

---

## Testing Conventions

### Test Naming

```csharp
// Format: MethodName_Scenario_ExpectedResult
[Fact]
public async Task GetFieldAsync_ExistingField_ReturnsField()
{
    // Arrange
    var service = CreateService();
    
    // Act
    var result = await service.GetFieldAsync("transaction", "trandate");
    
    // Assert
    result.Should().NotBeNull();
    result.Id.Should().Be("trandate");
}

[Fact]
public async Task GetFieldAsync_NonExistentField_ReturnsNull()
{
    // Arrange
    var service = CreateService();
    
    // Act
    var result = await service.GetFieldAsync("transaction", "invalid");
    
    // Assert
    result.Should().BeNull();
}
```

### AAA Pattern

Always use Arrange-Act-Assert:

```csharp
[Fact]
public async Task ValidationService_InvalidPlan_ReturnsFailure()
{
    // Arrange - Set up test data and mocks
    var validator = new ValidationService();
    var invalidPlan = new SavedSearchPlan { RecordType = "" };
    
    // Act - Execute the method being tested
    var result = await validator.ValidateAsync(invalidPlan);
    
    // Assert - Verify the results
    result.IsSuccess.Should().BeFalse();
    result.Error.Should().Contain("RecordType");
}
```

### FluentAssertions

```csharp
// Use FluentAssertions for readable tests
result.Should().NotBeNull();
result.Should().BeOfType<SavedSearchPlan>();
result.RecordType.Should().Be("transaction");

// Collections
fields.Should().HaveCount(5);
fields.Should().Contain(f => f.Id == "trandate");
fields.Should().BeInAscendingOrder(f => f.Label);

// Exceptions
var act = () => service.GetField(null);
act.Should().Throw<ArgumentNullException>()
   .WithMessage("*fieldId*");
```

---

## XML Documentation

**All public methods and classes must have XML docs:**

```csharp
/// <summary>
/// Retrieves a field definition from cache or storage.
/// </summary>
/// <param name="recordType">NetSuite record type (e.g., "transaction").</param>
/// <param name="fieldId">Field identifier (e.g., "trandate").</param>
/// <returns>Field definition if found; otherwise, null.</returns>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="recordType"/> or <paramref name="fieldId"/> is null.
/// </exception>
public async Task<FieldDefinition?> GetFieldAsync(
    string recordType, 
    string fieldId)
{
    // Implementation
}
```

---

## Error Handling

### Controllers

```csharp
[HttpGet("{id}")]
public async Task<ActionResult<Query>> GetQuery(Guid id)
{
    try
    {
        var query = await _queryService.GetAsync(id);
        if (query == null)
            return NotFound();
        
        return Ok(query);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving query {QueryId}", id);
        return StatusCode(500, new { error = "Internal server error" });
    }
}
```

### Services (Use Result Pattern)

```csharp
public async Task<Result<Query>> CreateAsync(CreateQueryRequest request)
{
    try
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.NaturalLanguage))
            return Result<Query>.Failure("Natural language query is required");
        
        // Business logic
        var query = new Query { /* ... */ };
        await _dbContext.Queries.AddAsync(query);
        await _dbContext.SaveChangesAsync();
        
        return Result<Query>.Success(query);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating query");
        return Result<Query>.Failure("Failed to create query");
    }
}
```

---

## Logging

### Structured Logging

```csharp
// Good - structured
_logger.LogInformation(
    "User {UserId} created query {QueryId} for record type {RecordType}",
    userId,
    queryId,
    recordType
);

// Bad - string interpolation
_logger.LogInformation($"User {userId} created query {queryId}");
```

### Log Levels

| Level | When to Use |
|-------|-------------|
| Debug | Detailed diagnostic information |
| Information | General informational messages |
| Warning | Unexpected but recoverable situations |
| Error | Errors and exceptions |
| Critical | Critical failures requiring immediate attention |

---

## Async/Await

```csharp
// Always use async for I/O operations
public async Task<FieldDefinition?> GetFieldAsync(string id)
{
    // Good
    return await _cache.GetAsync(id);
    
    // Bad - blocking
    return _cache.GetAsync(id).Result;
}

// Use ConfigureAwait(false) in library code (not needed in ASP.NET Core)
public async Task<Data> GetDataAsync()
{
    return await _httpClient.GetAsync(url).ConfigureAwait(false);
}
```

---

## Dependency Injection

### Registration

```csharp
// Program.cs
builder.Services.AddScoped<IQueryService, QueryService>();
builder.Services.AddSingleton<IFieldDictionaryService, FieldDictionaryService>();
builder.Services.AddTransient<IValidator<SavedSearchPlan>, SavedSearchPlanValidator>();
```

### Lifetimes

- **Transient:** Created each time requested (validators, lightweight services)
- **Scoped:** Created once per request (services that use DbContext)
- **Singleton:** Created once for application lifetime (caches, configuration)

---

## Configuration

```csharp
// Use strongly-typed configuration
public class OllamaSettings
{
    public required string Url { get; init; }
    public required string Model { get; init; }
    public int Timeout { get; init; } = 30000;
}

// Register in Program.cs
builder.Services.Configure<OllamaSettings>(
    builder.Configuration.GetSection("Ollama")
);

// Use in services
public class LlmService(IOptions<OllamaSettings> options)
{
    private readonly OllamaSettings _settings = options.Value;
}
```

---

## Common Anti-Patterns to Avoid

❌ **Using var everywhere**
```csharp
// Bad - unclear type
var x = GetData();

// Good - clear intent
SavedSearchPlan plan = GetData();
```

❌ **Catching generic exceptions without logging**
```csharp
// Bad
try { }
catch { }

// Good
try { }
catch (Exception ex)
{
    _logger.LogError(ex, "Error processing request");
    throw;
}
```

❌ **Not disposing IDisposable**
```csharp
// Bad
var client = new HttpClient();

// Good
using var client = new HttpClient();
```

❌ **Hardcoded values**
```csharp
// Bad
var url = "http://localhost:11434";

// Good
var url = _configuration["Ollama:Url"];
```

---

## Summary Checklist

Before committing code, verify:

- [ ] Follows naming conventions
- [ ] Uses C# 13 features where appropriate
- [ ] Has XML documentation on public members
- [ ] Uses structured logging
- [ ] Handles errors properly
- [ ] Has corresponding unit tests
- [ ] Uses dependency injection
- [ ] No hardcoded values
- [ ] No commented-out code
- [ ] Follows result pattern for business logic