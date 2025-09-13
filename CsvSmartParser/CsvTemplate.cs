using System.Text.RegularExpressions;
using CsvSmartParser.Models;
using CsvSmartParser.Exceptions;

namespace CsvSmartParser;

/// <summary>
/// Defines a template for strict CSV parsing with column definitions and validation rules.
/// </summary>
public class CsvTemplate
{
    private readonly Dictionary<string, (Type DataType, CsvValidationRule? ValidationRule)> _columns = new();

    /// <summary>
    /// Gets the defined column names.
    /// </summary>
    public IEnumerable<string> ColumnNames => _columns.Keys;

    /// <summary>
    /// Adds a column definition to the template.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="dataType">The expected data type for the column.</param>
    /// <param name="validationRule">Optional validation rule for the column.</param>
    /// <returns>The current template instance for method chaining.</returns>
    public CsvTemplate AddColumn(string columnName, Type dataType, CsvValidationRule? validationRule = null)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException("Column name cannot be null or empty.", nameof(columnName));

        _columns[columnName] = (dataType, validationRule);
        return this;
    }

    /// <summary>
    /// Adds a string column to the template.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="validationRule">Optional validation rule for the column.</param>
    /// <returns>The current template instance for method chaining.</returns>
    public CsvTemplate AddStringColumn(string columnName, CsvValidationRule? validationRule = null)
        => AddColumn(columnName, typeof(string), validationRule);

    /// <summary>
    /// Adds an integer column to the template.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="validationRule">Optional validation rule for the column.</param>
    /// <returns>The current template instance for method chaining.</returns>
    public CsvTemplate AddIntColumn(string columnName, CsvValidationRule? validationRule = null)
        => AddColumn(columnName, typeof(int), validationRule);

    /// <summary>
    /// Adds a decimal column to the template.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="validationRule">Optional validation rule for the column.</param>
    /// <returns>The current template instance for method chaining.</returns>
    public CsvTemplate AddDecimalColumn(string columnName, CsvValidationRule? validationRule = null)
        => AddColumn(columnName, typeof(decimal), validationRule);

    /// <summary>
    /// Adds a DateTime column to the template.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="validationRule">Optional validation rule for the column.</param>
    /// <returns>The current template instance for method chaining.</returns>
    public CsvTemplate AddDateTimeColumn(string columnName, CsvValidationRule? validationRule = null)
        => AddColumn(columnName, typeof(DateTime), validationRule);

    /// <summary>
    /// Adds a boolean column to the template.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="validationRule">Optional validation rule for the column.</param>
    /// <returns>The current template instance for method chaining.</returns>
    public CsvTemplate AddBooleanColumn(string columnName, CsvValidationRule? validationRule = null)
        => AddColumn(columnName, typeof(bool), validationRule);

    /// <summary>
    /// Validates a row against the template.
    /// </summary>
    /// <param name="row">The row data to validate.</param>
    /// <param name="rowIndex">The index of the row being validated.</param>
    /// <exception cref="CsvValidationException">Thrown when validation fails.</exception>
    internal void ValidateRow(Dictionary<string, string> row, int rowIndex)
    {
        foreach (var (columnName, (dataType, validationRule)) in _columns)
        {
            if (!row.TryGetValue(columnName, out var value))
            {
                if (validationRule?.IsRequired == true)
                    throw new CsvValidationException($"Required column '{columnName}' is missing in row {rowIndex}.");
                continue;
            }

            // Validate data type
            if (!TryConvertValue(value, dataType, out _))
                throw new CsvValidationException($"Invalid data type for column '{columnName}' in row {rowIndex}. Expected {dataType.Name}.");

            // Apply validation rules
            if (validationRule != null)
                ValidateValue(value, columnName, validationRule, rowIndex);
        }
    }

    /// <summary>
    /// Validates a column header against the template.
    /// </summary>
    /// <param name="headers">The headers from the CSV file.</param>
    /// <exception cref="CsvTemplateException">Thrown when headers don't match the template.</exception>
    internal void ValidateHeaders(IEnumerable<string> headers)
    {
        var headerSet = new HashSet<string>(headers, StringComparer.OrdinalIgnoreCase);
        var missingColumns = _columns.Keys.Where(col => !headerSet.Contains(col)).ToList();

        if (missingColumns.Any())
            throw new CsvTemplateException($"Missing required columns: {string.Join(", ", missingColumns)}");
    }

    private static void ValidateValue(string value, string columnName, CsvValidationRule rule, int rowIndex)
    {
        if (rule.IsRequired && string.IsNullOrWhiteSpace(value))
            throw new CsvValidationException(rule.ErrorMessage ?? $"Column '{columnName}' is required in row {rowIndex}.");

        if (!string.IsNullOrEmpty(value))
        {
            if (rule.MinLength.HasValue && value.Length < rule.MinLength.Value)
                throw new CsvValidationException(rule.ErrorMessage ?? $"Column '{columnName}' in row {rowIndex} is too short. Minimum length: {rule.MinLength.Value}");

            if (rule.MaxLength.HasValue && value.Length > rule.MaxLength.Value)
                throw new CsvValidationException(rule.ErrorMessage ?? $"Column '{columnName}' in row {rowIndex} is too long. Maximum length: {rule.MaxLength.Value}");

            if (!string.IsNullOrEmpty(rule.Pattern) && !Regex.IsMatch(value, rule.Pattern))
                throw new CsvValidationException(rule.ErrorMessage ?? $"Column '{columnName}' in row {rowIndex} doesn't match the required pattern.");

            if (rule.CustomValidator != null && !rule.CustomValidator(value))
                throw new CsvValidationException(rule.ErrorMessage ?? $"Column '{columnName}' in row {rowIndex} failed custom validation.");
        }
    }

    private static bool TryConvertValue(string value, Type targetType, out object? convertedValue)
    {
        convertedValue = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            convertedValue = targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            return true;
        }

        try
        {
            convertedValue = targetType.Name switch
            {
                nameof(String) => value,
                nameof(Int32) => int.Parse(value),
                nameof(Int64) => long.Parse(value),
                nameof(Decimal) => decimal.Parse(value),
                nameof(Double) => double.Parse(value),
                nameof(Single) => float.Parse(value),
                nameof(Boolean) => bool.Parse(value),
                nameof(DateTime) => DateTime.Parse(value),
                nameof(Guid) => Guid.Parse(value),
                _ => Convert.ChangeType(value, targetType)
            };
            return true;
        }
        catch
        {
            return false;
        }
    }
}
