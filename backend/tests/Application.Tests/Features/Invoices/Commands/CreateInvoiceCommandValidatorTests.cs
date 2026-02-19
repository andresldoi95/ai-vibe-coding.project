using Xunit;
using FluentAssertions;
using SaaS.Application.Features.Invoices.Commands.CreateInvoice;
using SaaS.Application.DTOs;

namespace Application.Tests.Features.Invoices.Commands;

public class CreateInvoiceCommandValidatorTests
{
    private readonly CreateInvoiceCommandValidator _validator;

    public CreateInvoiceCommandValidatorTests()
    {
        _validator = new CreateInvoiceCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            CustomerId = Guid.NewGuid(),
            EmissionPointId = Guid.NewGuid(),
            Items = new List<CreateInvoiceItemDto>
            {
                new CreateInvoiceItemDto
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 2,
                    TaxRateId = Guid.NewGuid(),
                    Description = "Test Product"
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyCustomerId_ShouldFail()
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            CustomerId = Guid.Empty,
            EmissionPointId = Guid.NewGuid(),
            Items = new List<CreateInvoiceItemDto>
            {
                new CreateInvoiceItemDto
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    TaxRateId = Guid.NewGuid(),
                    Description = "Test"
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerId");
    }

    [Fact]
    public void Validate_EmptyEmissionPointId_ShouldFail()
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            CustomerId = Guid.NewGuid(),
            EmissionPointId = Guid.Empty,
            Items = new List<CreateInvoiceItemDto>
            {
                new CreateInvoiceItemDto
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    TaxRateId = Guid.NewGuid(),
                    Description = "Test"
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EmissionPointId");
    }

    [Fact]
    public void Validate_EmptyItemsCollection_ShouldFail()
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            CustomerId = Guid.NewGuid(),
            EmissionPointId = Guid.NewGuid(),
            Items = new List<CreateInvoiceItemDto>()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Items");
    }

    [Fact]
    public void Validate_NullItemsCollection_ShouldFail()
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            CustomerId = Guid.NewGuid(),
            EmissionPointId = Guid.NewGuid(),
            Items = null!
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Items");
    }

    [Fact]
    public void Validate_ItemWithEmptyProductId_ShouldFail()
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            CustomerId = Guid.NewGuid(),
            EmissionPointId = Guid.NewGuid(),
            Items = new List<CreateInvoiceItemDto>
            {
                new CreateInvoiceItemDto
                {
                    ProductId = Guid.Empty,
                    Quantity = 1,
                    TaxRateId = Guid.NewGuid(),
                    Description = "Test"
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("ProductId"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_ItemWithInvalidQuantity_ShouldFail(int quantity)
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            CustomerId = Guid.NewGuid(),
            EmissionPointId = Guid.NewGuid(),
            Items = new List<CreateInvoiceItemDto>
            {
                new CreateInvoiceItemDto
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = quantity,
                    TaxRateId = Guid.NewGuid(),
                    Description = "Test"
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Quantity"));
    }

    [Fact]
    public void Validate_ItemWithEmptyTaxRateId_ShouldFail()
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            CustomerId = Guid.NewGuid(),
            EmissionPointId = Guid.NewGuid(),
            Items = new List<CreateInvoiceItemDto>
            {
                new CreateInvoiceItemDto
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    TaxRateId = Guid.Empty,
                    Description = "Test"
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("TaxRateId"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ItemWithEmptyDescription_ShouldFail(string? description)
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            CustomerId = Guid.NewGuid(),
            EmissionPointId = Guid.NewGuid(),
            Items = new List<CreateInvoiceItemDto>
            {
                new CreateInvoiceItemDto
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    TaxRateId = Guid.NewGuid(),
                    Description = description!
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Description"));
    }

    [Fact]
    public void Validate_ItemWithTooLongDescription_ShouldFail()
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            CustomerId = Guid.NewGuid(),
            EmissionPointId = Guid.NewGuid(),
            Items = new List<CreateInvoiceItemDto>
            {
                new CreateInvoiceItemDto
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    TaxRateId = Guid.NewGuid(),
                    Description = new string('A', 1001)
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Description") && e.ErrorMessage.Contains("1000"));
    }

    [Fact]
    public void Validate_SriAuthorizationTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            CustomerId = Guid.NewGuid(),
            EmissionPointId = Guid.NewGuid(),
            SriAuthorization = new string('A', 50),
            Items = new List<CreateInvoiceItemDto>
            {
                new CreateInvoiceItemDto
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    TaxRateId = Guid.NewGuid(),
                    Description = "Test"
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SriAuthorization" && e.ErrorMessage.Contains("49"));
    }

    [Fact]
    public void Validate_NotesTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            CustomerId = Guid.NewGuid(),
            EmissionPointId = Guid.NewGuid(),
            Notes = new string('A', 2001),
            Items = new List<CreateInvoiceItemDto>
            {
                new CreateInvoiceItemDto
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    TaxRateId = Guid.NewGuid(),
                    Description = "Test"
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Notes" && e.ErrorMessage.Contains("2000"));
    }
}
