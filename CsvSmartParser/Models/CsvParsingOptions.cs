namespace CsvSmartParser.Models;

/// <summary>
/// Configuration options for CSV parsing operations.
/// </summary>
public class CsvParsingOptions
{
    /// <summary>
    /// Gets or sets the delimiter character. If null, auto-detection will be used.
    /// </summary>
    public char? Delimiter { get; set; }

    /// <summary>
    /// Gets or sets whether the CSV has headers in the first row.
    /// </summary>
    public bool HasHeaders { get; set; } = true;

    /// <summary>
    /// Gets or sets the quote character used to escape values.
    /// </summary>
    public char QuoteChar { get; set; } = '"';

    /// <summary>
    /// Gets or sets whether to trim whitespace from values.
    /// </summary>
    public bool TrimWhitespace { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to skip empty lines.
    /// </summary>
    public bool SkipEmptyLines { get; set; } = true;

    /// <summary>
    /// Gets or sets the buffer size for streaming large files.
    /// </summary>
    public int BufferSize { get; set; } = 4096;
}
