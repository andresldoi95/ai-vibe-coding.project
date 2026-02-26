---
applyTo: "backend/tests/**"
---

# Backend Unit Testing Instructions

You are an expert in writing comprehensive unit tests for the .NET 8 backend.
Consolidates: **Backend Unit Testing Agent**.

---

## Tech Stack

- **Framework**: xUnit
- **Assertions**: FluentAssertions — always prefer over `Assert.*`
- **Mocking**: Moq
- **Runner**: `dotnet test`

---

## Reference Implementation

The **Warehouse feature tests** are the canonical pattern for all test work:
- Domain: `backend/tests/Domain.Tests/Entities/WarehouseTests.cs`
- Application: `backend/tests/Application.Tests/Features/Warehouses/`

Follow this structure exactly for any new module.

---

## Test Project Structure

```
backend/tests/
├── Domain.Tests/
│   ├── Domain.Tests.csproj
│   └── Entities/{EntityName}Tests.cs
├── Application.Tests/
│   ├── Application.Tests.csproj
│   └── Features/{FeatureName}/
│       ├── Commands/Create{Entity}CommandHandlerTests.cs
│       ├── Commands/Create{Entity}CommandValidatorTests.cs
│       ├── Commands/Update{Entity}CommandHandlerTests.cs
│       ├── Commands/Delete{Entity}CommandHandlerTests.cs
│       └── Queries/Get{Entity}ByIdQueryHandlerTests.cs
└── Infrastructure.Tests/
    └── Services/{ServiceName}Tests.cs
```

---

## Test Naming Convention

`MethodOrScenario_Condition_ExpectedOutcome`

Examples:
- `Handle_ValidCommand_ShouldCreateWarehouse`
- `Validate_EmptyName_ShouldFail`
- `Handle_NoTenantContext_ShouldReturnFailure`
- `Handle_SoftDeletedEntity_ShouldReturnNotFound`
- `Handle_DifferentTenant_ShouldReturnAccessDenied`

---

## AAA Structure (Always Use)

```csharp
[Fact]
public async Task Handle_ValidCommand_ShouldCreateEntity()
{
    // Arrange
    var tenantId = Guid.NewGuid();
    _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
    var command = new CreateWarehouseCommand { Name = "Main Warehouse", Code = "WH-001" };

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Warehouse>(), It.IsAny<CancellationToken>()), Times.Once);
    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
}
```

---

## Domain Entity Tests

```csharp
public class WarehouseTests
{
    [Fact]
    public void Entity_ShouldInitialize_WithDefaultValues()
    {
        var entity = new Warehouse();
        entity.IsActive.Should().BeTrue();
        entity.Name.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Warehouse A")]
    [InlineData("Distribution Center")]
    public void Name_CanBeSet_ToValidValue(string name)
    {
        var entity = new Warehouse { Name = name };
        entity.Name.Should().Be(name);
    }
}
```

**Coverage targets:**
- ✅ Default value initialization
- ✅ All property setters
- ✅ Optional field handling (null values)
- ✅ Business rule validation invariants

---

## Validator Tests

```csharp
public class CreateWarehouseCommandValidatorTests
{
    private readonly CreateWarehouseCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new CreateWarehouseCommand { Name = "Valid", Code = "WH-001" };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_EmptyName_ShouldFail(string value)
    {
        var command = new CreateWarehouseCommand { Name = value };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }
}
```

**Coverage targets — all validators must have 100% coverage:**
- ✅ Valid command passes
- ✅ Each required field failing independently
- ✅ MaxLength constraints
- ✅ Format rules (email, phone, regex)
- ✅ Optional field handling

---

## Command Handler Mock Setup

```csharp
public class CreateWarehouseCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITenantContext> _tenantContextMock = new();
    private readonly Mock<ILogger<CreateWarehouseCommandHandler>> _loggerMock = new();
    private readonly Mock<IWarehouseRepository> _repositoryMock = new();
    private readonly CreateWarehouseCommandHandler _handler;

    public CreateWarehouseCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Warehouses).Returns(_repositoryMock.Object);
        _handler = new CreateWarehouseCommandHandler(
            _unitOfWorkMock.Object, _tenantContextMock.Object, _loggerMock.Object);
    }
}
```

**Coverage targets for every command handler:**
- ✅ Happy path — entity created/updated/deleted
- ✅ Missing tenant context → failure result
- ✅ Entity not found → failure result
- ✅ Duplicate entity (unique constraint) → failure result
- ✅ Cross-tenant access → failure result (access denied)
- ✅ Soft-deleted entity → treated as not found
- ✅ Repository `AddAsync` / `Update` / `Remove` called with correct args
- ✅ `SaveChangesAsync` called once
- ✅ Critical operations logged

---

## Multi-Tenant Access Test (Required for Every Handler)

```csharp
[Fact]
public async Task Handle_EntityBelongsToDifferentTenant_ShouldReturnAccessDenied()
{
    // Arrange
    var currentTenantId = Guid.NewGuid();
    var entityTenantId = Guid.NewGuid(); // Different!
    _tenantContextMock.Setup(t => t.TenantId).Returns(currentTenantId);

    var entity = new Warehouse { Id = Guid.NewGuid(), TenantId = entityTenantId, IsDeleted = false };
    _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(entity);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Error.Should().Contain("Access denied");
}
```

---

## Soft Delete Test (Required for Get/Update/Delete Handlers)

```csharp
[Fact]
public async Task Handle_SoftDeletedEntity_ShouldReturnNotFound()
{
    var entity = new Warehouse { Id = Guid.NewGuid(), TenantId = _tenantId, IsDeleted = true };
    _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(entity);

    var result = await _handler.Handle(query, CancellationToken.None);

    result.IsSuccess.Should().BeFalse();
    result.Error.Should().Contain("not found");
}
```

---

## Coverage Goals

| Layer | Target |
|---|---|
| Domain Entities | 90%+ |
| Application Handlers | 80%+ |
| Validators | 100% |
| Infrastructure Services | 70%+ |

---

## Running Tests

```bash
dotnet test                                                                       # All tests
dotnet test backend/tests/Domain.Tests/Domain.Tests.csproj                       # Domain only
dotnet test backend/tests/Application.Tests/Application.Tests.csproj             # Application only
dotnet test --collect:"XPlat Code Coverage"                                       # With coverage
dotnet test --filter "FullyQualifiedName~WarehouseTests"                         # Specific class
dotnet test --verbosity detailed                                                  # Verbose output
```
