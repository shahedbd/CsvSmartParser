# CsvSmartParser

A robust, easy-to-use CSV parsing library for .NET 8.0 with auto-detection features, template-based parsing, and comprehensive validation.

## Features

- 🚀 **Async/Await Support**: Fully asynchronous APIs using Task
- 🔍 **Auto-Detection**: Automatically detects file encoding and delimiters
- 📋 **Template-Based Parsing**: Type-safe parsing with customizable templates
- ✅ **Validation**: Built-in validation rules with custom validator support
- 🎯 **Flexible**: Parse from files, strings, or streams
- 📊 **Large File Support**: Streaming support for processing large CSV files
- 🛡️ **Error Handling**: Meaningful exceptions with detailed error messages

## Installation

```bash
dotnet add package CsvSmartParser
```

## Quick Start

### Basic File Parsing

```csharp
using CsvSmartParser;

var parser = new CsvParser();

// Parse from file with auto-detection
var path = Path.Combine(AppContext.BaseDirectory, "Data", "Year_End_Stock_Prices_2015_2024.csv");
var data = await parser.ParseFileAsync(path);

// Print basic information
Console.WriteLine($"Total rows: {data.Count}");
Console.WriteLine($"Columns: {string.Join(", ", data.FirstOrDefault()?.Keys ?? Array.Empty<string>())}");
Console.WriteLine();

// Print all data
foreach (var row in data)
{
    foreach (var kvp in row)
    {
        Console.Write($"{kvp.Key}: {kvp.Value} | ");
    }
    Console.WriteLine();
}
```

### String Parsing

```csharp
using CsvSmartParser;

var parser = new CsvParser();

// Parse from string
var csvString = "Name,Age,City\nJohn,30,New York\nJane,25,Los Angeles";
var result = await parser.ParseStringAsync(csvString);

foreach (var row in result)
{
    Console.WriteLine($"Name: {row["Name"]}, Age: {row["Age"]}, City: {row["City"]}");
}
```

### Template-Based Validation

```csharp
using CsvSmartParser;
using CsvSmartParser.Models;

// Create a validation template
var template = new CsvTemplate()
    .AddStringColumn("Name", new CsvValidationRule 
    { 
        IsRequired = true, 
        MinLength = 2, 
        MaxLength = 50 
    })
    .AddIntColumn("Age", new CsvValidationRule 
    { 
        IsRequired = true,
        CustomValidator = value => int.TryParse(value, out var age) && age >= 0 && age <= 150,
        ErrorMessage = "Age must be between 0 and 150"
    })
    .AddStringColumn("Email", new CsvValidationRule 
    { 
        IsRequired = true,
        Pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        ErrorMessage = "Please provide a valid email address"
    });

var parser = new CsvParser();

try
{
    var employees = await parser.ParseFileAsync("employees.csv", template);
    Console.WriteLine($"Successfully validated {employees.Count} employees");
}
catch (CsvValidationException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

### Streaming Large Files

```csharp
using CsvSmartParser;

var parser = new CsvParser();

// Process large files without loading everything into memory
using var fileStream = File.OpenRead("large-dataset.csv");

await foreach (var row in parser.ParseStreamAsync(fileStream))
{
    // Process each row individually
    Console.WriteLine($"Processing: {string.Join(", ", row.Values)}");
    
    // Your processing logic here
    await ProcessRowAsync(row);
}

async Task ProcessRowAsync(Dictionary<string, string> row)
{
    // Simulate some async processing
    await Task.Delay(10);
}
```

### Custom Configuration

```csharp
using CsvSmartParser;
using CsvSmartParser.Models;

var options = new CsvParsingOptions
{
    Delimiter = ';',           // Use semicolon as delimiter
    HasHeaders = true,         // First row contains headers
    TrimWhitespace = true,     // Trim spaces from values
    SkipEmptyLines = true,     // Skip empty lines
    Encoding = Encoding.UTF8   // Specify encoding
};

var parser = new CsvParser(options);
var result = await parser.ParseFileAsync("data.csv");
```

## Advanced Features

### Auto-Detection Capabilities

The library automatically detects:
- **Encodings**: UTF-8 (with/without BOM), UTF-16 LE/BE, UTF-32 LE/BE
- **Delimiters**: Comma (,), Semicolon (;), Tab (\t), Pipe (|), Colon (:)
- **Headers**: Automatic detection of header rows

### Validation Rules

```csharp
var template = new CsvTemplate()
    .AddStringColumn("ProductName", new CsvValidationRule 
    { 
        IsRequired = true,
        MinLength = 3,
        MaxLength = 100,
        Pattern = @"^[A-Za-z0-9\s\-]+$",
        ErrorMessage = "Product name can only contain letters, numbers, spaces, and hyphens"
    })
    .AddDecimalColumn("Price", new CsvValidationRule 
    { 
        IsRequired = true,
        CustomValidator = value => decimal.TryParse(value, out var price) && price > 0,
        ErrorMessage = "Price must be a positive number"
    })
    .AddDateTimeColumn("LaunchDate", new CsvValidationRule 
    { 
        IsRequired = false,
        CustomValidator = value => string.IsNullOrEmpty(value) || 
                                 DateTime.TryParse(value, out var date) && date <= DateTime.Now,
        ErrorMessage = "Launch date cannot be in the future"
    });
```

### Error Handling

```csharp
try
{
    var data = await parser.ParseFileAsync("data.csv", template);
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File not found: {ex.Message}");
}
catch (CsvParsingException ex)
{
    Console.WriteLine($"CSV parsing error at row {ex.RowNumber}: {ex.Message}");
}
catch (CsvValidationException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
    foreach (var error in ex.ValidationErrors)
    {
        Console.WriteLine($"  Row {error.RowNumber}, Column '{error.ColumnName}': {error.ErrorMessage}");
    }
}
```

## API Reference

### CsvParser Methods

- `ParseFileAsync(string filePath, CsvTemplate? template = null, Encoding? encoding = null)` - Parse CSV file with optional template validation
- `ParseStringAsync(string csvData, CsvTemplate? template = null)` - Parse CSV from string data
- `ParseStreamAsync(Stream stream, CsvTemplate? template = null, Encoding? encoding = null)` - Stream large CSV files

### CsvTemplate Methods

- `AddStringColumn(string name, CsvValidationRule? rule = null)` - Add string column with validation
- `AddIntColumn(string name, CsvValidationRule? rule = null)` - Add integer column with validation
- `AddDecimalColumn(string name, CsvValidationRule? rule = null)` - Add decimal column with validation
- `AddDateTimeColumn(string name, CsvValidationRule? rule = null)` - Add DateTime column with validation
- `AddBooleanColumn(string name, CsvValidationRule? rule = null)` - Add boolean column with validation

## Requirements

- .NET 8.0 or later
- No external dependencies

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.