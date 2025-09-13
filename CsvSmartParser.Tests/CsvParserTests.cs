using System.Text;
using CsvSmartParser;
using CsvSmartParser.Exceptions;
using CsvSmartParser.Models;
using FluentAssertions;
using Xunit;

namespace CsvSmartParser.Tests;

public class CsvParserTests
{
    [Fact]
    public async Task ParseStringAsync_WithSimpleCsv_ShouldReturnCorrectData()
    {
        // Arrange
        var csvData = "Name,Age,City\nJohn,30,New York\nJane,25,Los Angeles";
        var parser = new CsvParser();

        // Act
        var result = await parser.ParseStringAsync(csvData);

        // Assert
        result.Should().HaveCount(2);
        result[0]["Name"].Should().Be("John");
        result[0]["Age"].Should().Be("30");
        result[0]["City"].Should().Be("New York");
        result[1]["Name"].Should().Be("Jane");
        result[1]["Age"].Should().Be("25");
        result[1]["City"].Should().Be("Los Angeles");
    }

    [Fact]
    public async Task ParseStringAsync_WithQuotedValues_ShouldHandleCorrectly()
    {
        // Arrange
        var csvData = "Name,Description\n\"John Doe\",\"A person with, comma\"\n\"Jane Smith\",\"Another \"\"quoted\"\" value\"";
        var parser = new CsvParser();

        // Act
        var result = await parser.ParseStringAsync(csvData);

        // Assert
        result.Should().HaveCount(2);
        result[0]["Name"].Should().Be("John Doe");
        result[0]["Description"].Should().Be("A person with, comma");
        result[1]["Name"].Should().Be("Jane Smith");
        result[1]["Description"].Should().Be("Another \"quoted\" value");
    }

    [Fact]
    public async Task ParseStringAsync_WithSemicolonDelimiter_ShouldDetectCorrectly()
    {
        // Arrange
        var csvData = "Name;Age;City\nJohn;30;New York\nJane;25;Los Angeles";
        var parser = new CsvParser();

        // Act
        var result = await parser.ParseStringAsync(csvData);

        // Assert
        result.Should().HaveCount(2);
        result[0]["Name"].Should().Be("John");
        result[0]["Age"].Should().Be("30");
        result[0]["City"].Should().Be("New York");
    }

    [Fact]
    public async Task ParseStringAsync_WithTabDelimiter_ShouldDetectCorrectly()
    {
        // Arrange
        var csvData = "Name\tAge\tCity\nJohn\t30\tNew York\nJane\t25\tLos Angeles";
        var parser = new CsvParser();

        // Act
        var result = await parser.ParseStringAsync(csvData);

        // Assert
        result.Should().HaveCount(2);
        result[0]["Name"].Should().Be("John");
        result[0]["Age"].Should().Be("30");
        result[0]["City"].Should().Be("New York");
    }

    [Fact]
    public async Task ParseStringAsync_WithEmptyString_ShouldReturnEmptyList()
    {
        // Arrange
        var parser = new CsvParser();

        // Act
        var result = await parser.ParseStringAsync("");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ParseStringAsync_WithEmptyLines_ShouldSkipByDefault()
    {
        // Arrange
        var csvData = "Name,Age\nJohn,30\n\nJane,25\n";
        var parser = new CsvParser();

        // Act
        var result = await parser.ParseStringAsync(csvData);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ParseFileAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var parser = new CsvParser();

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            parser.ParseFileAsync("nonexistent.csv"));
    }

    [Fact]
    public async Task ParseFileAsync_WithValidFile_ShouldParseCorrectly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var csvContent = "Name,Age,City\nJohn,30,New York\nJane,25,Los Angeles";
        await File.WriteAllTextAsync(tempFile, csvContent, Encoding.UTF8);
        var parser = new CsvParser();

        try
        {
            // Act
            var result = await parser.ParseFileAsync(tempFile);

            // Assert
            result.Should().HaveCount(2);
            result[0]["Name"].Should().Be("John");
            result[1]["Name"].Should().Be("Jane");
        }
        finally
        {
            // Cleanup
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ParseFileAsync_WithUtf8Bom_ShouldDetectEncodingCorrectly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var csvContent = "Name,Age\nJöhn,30\nJäne,25";
        var utf8WithBom = new UTF8Encoding(true);
        await File.WriteAllTextAsync(tempFile, csvContent, utf8WithBom);
        var parser = new CsvParser();

        try
        {
            // Act
            var result = await parser.ParseFileAsync(tempFile);

            // Assert
            result.Should().HaveCount(2);
            result[0]["Name"].Should().Be("Jöhn");
            result[1]["Name"].Should().Be("Jäne");
        }
        finally
        {
            // Cleanup
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ParseStringAsync_WithCustomOptions_ShouldRespectOptions()
    {
        // Arrange
        var csvData = "Name|Age|City\n  John  |  30  |  New York  \n  Jane  |  25  |  Los Angeles  ";
        var options = new CsvParsingOptions
        {
            Delimiter = '|',
            TrimWhitespace = true
        };
        var parser = new CsvParser(options);

        // Act
        var result = await parser.ParseStringAsync(csvData);

        // Assert
        result.Should().HaveCount(2);
        result[0]["Name"].Should().Be("John");
        result[0]["Age"].Should().Be("30");
        result[0]["City"].Should().Be("New York");
    }

    [Fact]
    public async Task ParseStreamAsync_WithLargeData_ShouldStreamCorrectly()
    {
        // Arrange
        var csvContent = "Name,Age\n";
        for (int i = 0; i < 1000; i++)
        {
            csvContent += $"Person{i},{20 + i}\n";
        }

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var parser = new CsvParser();

        // Act
        var results = new List<Dictionary<string, string>>();
        await foreach (var row in parser.ParseStreamAsync(stream))
        {
            results.Add(row);
        }

        // Assert
        results.Should().HaveCount(1000);
        results[0]["Name"].Should().Be("Person0");
        results[999]["Name"].Should().Be("Person999");
    }
}
