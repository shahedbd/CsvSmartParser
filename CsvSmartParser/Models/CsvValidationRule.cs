namespace CsvSmartParser.Models;

/// <summary>
/// Represents a validation rule for CSV column values.
/// </summary>
public class CsvValidationRule
{
    /// <summary>
    /// Gets or sets whether the column is required (cannot be null or empty).
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the minimum length for string values.
    /// </summary>
    public int? MinLength { get; set; }

    /// <summary>
    /// Gets or sets the maximum length for string values.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets a regular expression pattern for validation.
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    /// Gets or sets a custom validation function.
    /// </summary>
    public Func<string, bool>? CustomValidator { get; set; }

    /// <summary>
    /// Gets or sets the error message for validation failures.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
