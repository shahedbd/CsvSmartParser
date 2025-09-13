using CsvSmartParser;
using CsvSmartParser.Exceptions;
using CsvSmartParser.Models;
using FluentAssertions;
using Xunit;

namespace CsvSmartParser.Tests;

public class CsvTemplateTests
{
    [Fact]
    public async Task ParseStringAsync_WithValidTemplate_ShouldValidateSuccessfully()
    {
        // Arrange
        var csvData = "Name,Age,Email\nJohn,30,john@example.com\nJane,25,jane@example.com";
        var template = new CsvTemplate()
            .AddStringColumn("Name", new CsvValidationRule { IsRequired = true })
            .AddIntColumn("Age", new CsvValidationRule { IsRequired = true })
            .AddStringColumn("Email", new CsvValidationRule { IsRequired = true });

        var parser = new CsvParser();

        // Act
        var result = await parser.ParseStringAsync(csvData, template);

        // Assert
        result.Should().HaveCount(2);
        result[0]["Name"].Should().Be("John");
        result[0]["Age"].Should().Be("30");
        result[0]["Email"].Should().Be("john@example.com");
    }

    [Fact]
    public async Task ParseStringAsync_WithMissingRequiredColumn_ShouldThrowCsvTemplateException()
    {
        // Arrange
        var csvData = "Name,Age\nJohn,30\nJane,25";
        var template = new CsvTemplate()
            .AddStringColumn("Name", new CsvValidationRule { IsRequired = true })
            .AddIntColumn("Age", new CsvValidationRule { IsRequired = true })
            .AddStringColumn("Email", new CsvValidationRule { IsRequired = true });

        var parser = new CsvParser();

        // Act & Assert
        await Assert.ThrowsAsync<CsvTemplateException>(() =>
            parser.ParseStringAsync(csvData, template));
    }

    [Fact]
    public async Task ParseStringAsync_WithInvalidDataType_ShouldThrowCsvValidationException()
    {
        // Arrange
        var csvData = "Name,Age,Email\nJohn,NotANumber,john@example.com";
        var template = new CsvTemplate()
            .AddStringColumn("Name")
            .AddIntColumn("Age")
            .AddStringColumn("Email");

        var parser = new CsvParser();

        // Act & Assert
        await Assert.ThrowsAsync<CsvValidationException>(() =>
            parser.ParseStringAsync(csvData, template));
    }

    [Fact]
    public async Task ParseStringAsync_WithStringLengthValidation_ShouldValidateCorrectly()
    {
        // Arrange
        var csvData = "Name,Description\nJohn,Short\nJane,This is a very long description that exceeds the maximum length";
        var template = new CsvTemplate()
            .AddStringColumn("Name", new CsvValidationRule { MinLength = 2, MaxLength = 10 })
            .AddStringColumn("Description", new CsvValidationRule { MaxLength = 20 });

        var parser = new CsvParser();

        // Act & Assert
        await Assert.ThrowsAsync<CsvValidationException>(() =>
            parser.ParseStringAsync(csvData, template));
    }

    [Fact]
    public async Task ParseStringAsync_WithPatternValidation_ShouldValidateCorrectly()
    {
        // Arrange
        var csvData = "Name,Email\nJohn,john@example.com\nJane,invalid-email";
        var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        var template = new CsvTemplate()
            .AddStringColumn("Name")
            .AddStringColumn("Email", new CsvValidationRule { Pattern = emailPattern });

        var parser = new CsvParser();

        // Act & Assert
        await Assert.ThrowsAsync<CsvValidationException>(() =>
            parser.ParseStringAsync(csvData, template));
    }

    [Fact]
    public async Task ParseStringAsync_WithCustomValidator_ShouldValidateCorrectly()
    {
        // Arrange
        var csvData = "Name,Age\nJohn,30\nJane,15";
        var template = new CsvTemplate()
            .AddStringColumn("Name")
            .AddIntColumn("Age", new CsvValidationRule
            {
                CustomValidator = value => int.TryParse(value, out var age) && age >= 18,
                ErrorMessage = "Age must be 18 or older"
            });

        var parser = new CsvParser();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CsvValidationException>(() =>
            parser.ParseStringAsync(csvData, template));

        exception.Message.Should().Contain("Age must be 18 or older");
    }

    [Fact]
    public async Task ParseStringAsync_WithDateTimeColumn_ShouldParseCorrectly()
    {
        // Arrange
        var csvData = "Name,BirthDate\nJohn,1990-01-15\nJane,1995-06-20";
        var template = new CsvTemplate()
            .AddStringColumn("Name")
            .AddDateTimeColumn("BirthDate");

        var parser = new CsvParser();

        // Act
        var result = await parser.ParseStringAsync(csvData, template);

        // Assert
        result.Should().HaveCount(2);
        result[0]["BirthDate"].Should().Be("1990-01-15");
        result[1]["BirthDate"].Should().Be("1995-06-20");
    }

    [Fact]
    public async Task ParseStringAsync_WithBooleanColumn_ShouldParseCorrectly()
    {
        // Arrange
        var csvData = "Name,IsActive\nJohn,true\nJane,false";
        var template = new CsvTemplate()
            .AddStringColumn("Name")
            .AddBooleanColumn("IsActive");

        var parser = new CsvParser();

        // Act
        var result = await parser.ParseStringAsync(csvData, template);

        // Assert
        result.Should().HaveCount(2);
        result[0]["IsActive"].Should().Be("true");
        result[1]["IsActive"].Should().Be("false");
    }

    [Fact]
    public async Task ParseStringAsync_WithDecimalColumn_ShouldParseCorrectly()
    {
        // Arrange
        var csvData = "Name,Salary\nJohn,50000.50\nJane,75000.75";
        var template = new CsvTemplate()
            .AddStringColumn("Name")
            .AddDecimalColumn("Salary");

        var parser = new CsvParser();

        // Act
        var result = await parser.ParseStringAsync(csvData, template);

        // Assert
        result.Should().HaveCount(2);
        result[0]["Salary"].Should().Be("50000.50");
        result[1]["Salary"].Should().Be("75000.75");
    }

    [Fact]
    public void AddColumn_WithNullColumnName_ShouldThrowArgumentException()
    {
        // Arrange
        var template = new CsvTemplate();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            template.AddColumn(null!, typeof(string)));
    }

    [Fact]
    public void AddColumn_WithEmptyColumnName_ShouldThrowArgumentException()
    {
        // Arrange
        var template = new CsvTemplate();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            template.AddColumn("", typeof(string)));
    }

    [Fact]
    public void ColumnNames_ShouldReturnAllDefinedColumns()
    {
        // Arrange
        var template = new CsvTemplate()
            .AddStringColumn("Name")
            .AddIntColumn("Age")
            .AddStringColumn("Email");

        // Act
        var columnNames = template.ColumnNames.ToList();

        // Assert
        columnNames.Should().HaveCount(3);
        columnNames.Should().Contain("Name");
        columnNames.Should().Contain("Age");
        columnNames.Should().Contain("Email");
    }
}
