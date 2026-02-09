# Backend Unit Testing Agent

## Purpose

Specialized agent for implementing comprehensive unit tests for the .NET 8 backend following established patterns, conventions, and best practices demonstrated in the Warehouse feature tests.

## Scope

- **Domain Layer Testing**: Entity behavior, property validation, business rules
- **Application Layer Testing**: CQRS handlers, validators, service logic
- **Test Infrastructure**: xUnit, FluentAssertions, Moq setup and configuration
- **Test Patterns**: AAA (Arrange-Act-Assert), mocking, parameterized tests

## Technology Stack

- **Testing Framework**: xUnit
- **Assertion Library**: FluentAssertions
- **Mocking Framework**: Moq
- **Test Runner**: `dotnet test`
- **Code Coverage**: XPlat Code Coverage (optional)

## Reference Implementation

The **Warehouse feature tests** serve as the canonical reference for all unit testing in this project:

### Domain Tests
- **Location**: `backend/tests/Domain.Tests/Entities/WarehouseTests.cs`
- **Coverage**: Entity initialization, property setters, optional fields, default values

### Application Tests
- **Location**: `backend/tests/Application.Tests/Features/Warehouses/`
- **Coverage**: Commands (Create, Update, Delete), Queries (GetById, GetAll), Validators

## Test Project Structure

```
backend/
├── tests/
│   ├── Domain.Tests/
│   │   ├── Domain.Tests.csproj
│   │   └── Entities/
│   │       └── {EntityName}Tests.cs
│   ├── Application.Tests/
│   │   ├── Application.Tests.csproj
│   │   └── Features/
│   │       └── {FeatureName}/
│   │           ├── Commands/
│   │           │   ├── {CommandName}CommandHandlerTests.cs
│   │           │   └── {CommandName}CommandValidatorTests.cs
│   │           └── Queries/
│   │               └── {QueryName}QueryHandlerTests.cs
│   └── Infrastructure.Tests/
│       ├── Infrastructure.Tests.csproj
│       └── Services/
│           └── {ServiceName}Tests.cs
```

## Testing Guidelines

### 1. Domain Layer Tests

**Purpose**: Test entity behavior, business rules, and value objects.

**Naming Convention**: `{EntityName}Tests`

**Example Pattern**:
```csharp
public class WarehouseTests
{
    [Fact]
    public void Entity_ShouldInitialize_WithDefaultValues()
    {
        // Arrange & Act
        var entity = new Warehouse();

        // Assert
        entity.IsActive.Should().BeTrue();
        entity.Name.Should().BeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Property_CanBeSet_ToValidValue(bool value)
    {
        // Arrange & Act
        var entity = new Warehouse { IsActive = value };

        // Assert
        entity.IsActive.Should().Be(value);
    }
}
```

**Coverage Checklist**:
- ✅ Default value initialization
- ✅ Property setters for all required fields
- ✅ Optional field handling (null values)
- ✅ Business rule validation (if applicable)
- ✅ Value object behavior (equality, formatting)

### 2. Validator Tests

**Purpose**: Test FluentValidation rules for commands and queries.

**Naming Convention**: `{CommandName}CommandValidatorTests`

**Example Pattern**:
```csharp
public class CreateWarehouseCommandValidatorTests
{
    private readonly CreateWarehouseCommandValidator _validator;

    public CreateWarehouseCommandValidatorTests()
    {
        _validator = new CreateWarehouseCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateWarehouseCommand
        {
            Name = "Valid Name",
            Code = "VALID-CODE"
            // ... all required fields
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyRequiredField_ShouldFail(string value)
    {
        // Arrange
        var command = new CreateWarehouseCommand { Name = value };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Name" &&
            e.ErrorMessage.Contains("required"));
    }
}
```

**Coverage Checklist**:
- ✅ Valid command passes validation
- ✅ Required fields validation
- ✅ Maximum length validation
- ✅ Format validation (email, regex patterns)
- ✅ Business rule validation
- ✅ Optional field handling

### 3. Command Handler Tests

**Purpose**: Test CQRS command handlers, business logic, and side effects.

**Naming Convention**: `{CommandName}CommandHandlerTests`

**Mock Setup Pattern**:
```csharp
public class CreateWarehouseCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<CreateWarehouseCommandHandler>> _loggerMock;
    private readonly Mock<IWarehouseRepository> _repositoryMock;
    private readonly CreateWarehouseCommandHandler _handler;

    public CreateWarehouseCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<CreateWarehouseCommandHandler>>();
        _repositoryMock = new Mock<IWarehouseRepository>();

        _unitOfWorkMock.Setup(u => u.Warehouses).Returns(_repositoryMock.Object);

        _handler = new CreateWarehouseCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var command = new CreateWarehouseCommand { /* ... */ };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        _repositoryMock.Verify(r => r.AddAsync(
            It.IsAny<Warehouse>(),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u =>
            u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
```

**Coverage Checklist**:
- ✅ Successful operation (happy path)
- ✅ Missing tenant context
- ✅ Entity not found scenarios
- ✅ Duplicate entity validation
- ✅ Multi-tenant access control
- ✅ Repository interaction verification
- ✅ UnitOfWork SaveChanges verification
- ✅ Logging verification for critical operations
- ✅ Error handling and failure results

### 4. Query Handler Tests

**Purpose**: Test CQRS query handlers and data retrieval logic.

**Naming Convention**: `{QueryName}QueryHandlerTests`

**Example Pattern**:
```csharp
[Fact]
public async Task Handle_ValidQuery_ShouldReturnEntity()
{
    // Arrange
    var tenantId = Guid.NewGuid();
    var entityId = Guid.NewGuid();
    _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

    var entity = new Warehouse
    {
        Id = entityId,
        TenantId = tenantId,
        Name = "Test",
        IsDeleted = false
    };

    _repositoryMock
        .Setup(r => r.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(entity);

    var query = new GetWarehouseByIdQuery { Id = entityId };

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value!.Id.Should().Be(entityId);
    result.Value.Name.Should().Be("Test");
}
```

**Coverage Checklist**:
- ✅ Successful entity retrieval
- ✅ Entity not found scenarios
- ✅ Soft-deleted entity handling
- ✅ Multi-tenant access control
- ✅ DTO mapping verification
- ✅ Optional field handling
- ✅ Logging verification

## Testing Conventions

### Test Naming

Use the pattern: `MethodName_Scenario_ExpectedBehavior`

**Examples**:
- `Handle_ValidCommand_ShouldCreateWarehouse`
- `Validate_EmptyName_ShouldFail`
- `Handle_NoTenantContext_ShouldReturnFailure`
- `Handle_DifferentTenant_ShouldReturnAccessDenied`

### AAA Pattern

Always structure tests with clear sections:

```csharp
[Fact]
public async Task TestMethod()
{
    // Arrange
    // Setup mocks, create test data

    // Act
    // Execute the method under test

    // Assert
    // Verify expectations
}
```

### Mock Verification

**Repository Operations**:
```csharp
_repositoryMock.Verify(r => r.AddAsync(
    It.Is<Warehouse>(w => w.Name == "Expected Name"),
    It.IsAny<CancellationToken>()),
    Times.Once);

_unitOfWorkMock.Verify(u =>
    u.SaveChangesAsync(It.IsAny<CancellationToken>()),
    Times.Once);
```

**Logger Verification**:
```csharp
_loggerMock.Verify(
    x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("expected text")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
    Times.Once);
```

### FluentAssertions Best Practices

**Use readable, expressive assertions**:
```csharp
// ✅ Good
result.IsSuccess.Should().BeTrue();
result.Value.Should().NotBeNull();
result.Value!.Name.Should().Be("Expected");
result.Errors.Should().Contain(e => e.PropertyName == "Name");

// ❌ Avoid
Assert.True(result.IsSuccess);
Assert.NotNull(result.Value);
Assert.Equal("Expected", result.Value.Name);
```

## Multi-Tenant Testing Patterns

**Always test tenant isolation**:

```csharp
[Fact]
public async Task Handle_DifferentTenant_ShouldReturnAccessDenied()
{
    // Arrange
    var currentTenantId = Guid.NewGuid();
    var entityTenantId = Guid.NewGuid(); // Different tenant

    _tenantContextMock.Setup(t => t.TenantId).Returns(currentTenantId);

    var entity = new Warehouse
    {
        Id = Guid.NewGuid(),
        TenantId = entityTenantId, // Belongs to different tenant
        IsDeleted = false
    };

    _repositoryMock
        .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(entity);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Error.Should().Contain("Access denied");
}
```

## Soft Delete Testing

**Always test soft delete behavior**:

```csharp
[Fact]
public async Task Handle_DeletedEntity_ShouldReturnNotFound()
{
    // Arrange
    var entity = new Warehouse
    {
        Id = Guid.NewGuid(),
        TenantId = _tenantId,
        IsDeleted = true // Soft deleted
    };

    _repositoryMock
        .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(entity);

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Error.Should().Contain("not found");
}
```

## Coverage Goals

- **Domain Entities**: 90%+ (critical business logic)
- **Application Handlers**: 80%+ (core CQRS operations)
- **Validators**: 100% (all validation rules)
- **Infrastructure Services**: 70%+ (external dependencies)

## Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test backend/tests/Domain.Tests/Domain.Tests.csproj
dotnet test backend/tests/Application.Tests/Application.Tests.csproj

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests with verbose output
dotnet test --verbosity detailed

# Run specific test class
dotnet test --filter "FullyQualifiedName~WarehouseTests"
```

## Implementation Checklist

When creating tests for a new feature:

### Domain Tests
- [ ] Create `{EntityName}Tests.cs` in `Domain.Tests/Entities/`
- [ ] Test default value initialization
- [ ] Test all property setters
- [ ] Test optional field behavior
- [ ] Test business rule validation (if any)
- [ ] Verify all tests pass

### Validator Tests
- [ ] Create `{CommandName}CommandValidatorTests.cs`
- [ ] Test valid command passes
- [ ] Test all required field validation
- [ ] Test maximum length constraints
- [ ] Test format validation (email, regex)
- [ ] Test optional field handling
- [ ] Verify all tests pass

### Command Handler Tests
- [ ] Create `{CommandName}CommandHandlerTests.cs`
- [ ] Setup all required mocks (IUnitOfWork, ITenantContext, ILogger, Repositories)
- [ ] Test successful operation (happy path)
- [ ] Test missing tenant context
- [ ] Test entity not found
- [ ] Test duplicate entity prevention
- [ ] Test multi-tenant access control
- [ ] Test soft delete handling
- [ ] Verify repository calls with `Verify()`
- [ ] Verify SaveChanges called
- [ ] Verify logging for critical operations
- [ ] Verify all tests pass

### Query Handler Tests
- [ ] Create `{QueryName}QueryHandlerTests.cs`
- [ ] Setup all required mocks
- [ ] Test successful entity retrieval
- [ ] Test entity not found
- [ ] Test soft-deleted entity handling
- [ ] Test multi-tenant access control
- [ ] Test DTO mapping correctness
- [ ] Test optional field handling
- [ ] Verify all tests pass

## Best Practices

1. **Test Isolation**: Each test should be independent and not rely on other tests
2. **Clear Intent**: Test names should clearly describe what is being tested
3. **No Business Logic in Tests**: Tests should only verify behavior, not implement logic
4. **Mock Only Dependencies**: Mock external dependencies, not the system under test
5. **One Assertion Per Concept**: Group related assertions, but keep tests focused
6. **Parameterized Tests**: Use `[Theory]` and `[InlineData]` for similar test cases
7. **Async All The Way**: Use `async Task` for all async handlers, never `.Result` or `.Wait()`
8. **Verify Mock Interactions**: Always verify that mocked methods were called as expected
9. **Test Failure Paths**: Don't just test happy paths, test error scenarios
10. **Keep Tests Maintainable**: Refactor test setup into helper methods when appropriate

## Reference Files

**Canonical Test Implementation**:
- [`backend/tests/Domain.Tests/Entities/WarehouseTests.cs`](../backend/tests/Domain.Tests/Entities/WarehouseTests.cs)
- [`backend/tests/Application.Tests/Features/Warehouses/Commands/CreateWarehouseCommandValidatorTests.cs`](../backend/tests/Application.Tests/Features/Warehouses/Commands/CreateWarehouseCommandValidatorTests.cs)
- [`backend/tests/Application.Tests/Features/Warehouses/Commands/CreateWarehouseCommandHandlerTests.cs`](../backend/tests/Application.Tests/Features/Warehouses/Commands/CreateWarehouseCommandHandlerTests.cs)
- [`backend/tests/Application.Tests/Features/Warehouses/Commands/UpdateWarehouseCommandHandlerTests.cs`](../backend/tests/Application.Tests/Features/Warehouses/Commands/UpdateWarehouseCommandHandlerTests.cs)
- [`backend/tests/Application.Tests/Features/Warehouses/Commands/DeleteWarehouseCommandHandlerTests.cs`](../backend/tests/Application.Tests/Features/Warehouses/Commands/DeleteWarehouseCommandHandlerTests.cs)
- [`backend/tests/Application.Tests/Features/Warehouses/Queries/GetWarehouseByIdQueryHandlerTests.cs`](../backend/tests/Application.Tests/Features/Warehouses/Queries/GetWarehouseByIdQueryHandlerTests.cs)

## Notes

- This agent focuses exclusively on **unit testing** for backend code
- Integration tests and API tests are out of scope
- Always reference the Warehouse tests as the canonical example
- Follow the established patterns consistently across all features
- Update this document when new testing patterns emerge
