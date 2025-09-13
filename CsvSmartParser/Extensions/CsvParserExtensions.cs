using CsvSmartParser.Models;

namespace CsvSmartParser.Extensions;

/// <summary>
/// Extension methods for fluent configuration of CSV parsing.
/// </summary>
public static class CsvParserExtensions
{
    /// <summary>
    /// Configures the CSV parser with a fluent API.
    /// </summary>
    /// <param name="parser">The CSV parser instance.</param>
    /// <param name="configure">The configuration action.</param>
    /// <returns>A configured CSV parser.</returns>
    public static CsvParser Configure(this CsvParser parser, Action<CsvParsingOptions> configure)
    {
        var options = new CsvParsingOptions();
        configure(options);
        return new CsvParser(options);
    }

    /// <summary>
    /// Sets a custom delimiter for CSV parsing.
    /// </summary>
    /// <param name="options">The parsing options.</param>
    /// <param name="delimiter">The delimiter character.</param>
    /// <returns>The parsing options for method chaining.</returns>
    public static CsvParsingOptions WithDelimiter(this CsvParsingOptions options, char delimiter)
    {
        options.Delimiter = delimiter;
        return options;
    }

    /// <summary>
    /// Configures whether the CSV has headers.
    /// </summary>
    /// <param name="options">The parsing options.</param>
    /// <param name="hasHeaders">True if the CSV has headers.</param>
    /// <returns>The parsing options for method chaining.</returns>
    public static CsvParsingOptions WithHeaders(this CsvParsingOptions options, bool hasHeaders = true)
    {
        options.HasHeaders = hasHeaders;
        return options;
    }

    /// <summary>
    /// Configures whitespace trimming.
    /// </summary>
    /// <param name="options">The parsing options.</param>
    /// <param name="trimWhitespace">True to trim whitespace from values.</param>
    /// <returns>The parsing options for method chaining.</returns>
    public static CsvParsingOptions WithTrimming(this CsvParsingOptions options, bool trimWhitespace = true)
    {
        options.TrimWhitespace = trimWhitespace;
        return options;
    }

    /// <summary>
    /// Configures empty line handling.
    /// </summary>
    /// <param name="options">The parsing options.</param>
    /// <param name="skipEmptyLines">True to skip empty lines.</param>
    /// <returns>The parsing options for method chaining.</returns>
    public static CsvParsingOptions SkipEmptyLines(this CsvParsingOptions options, bool skipEmptyLines = true)
    {
        options.SkipEmptyLines = skipEmptyLines;
        return options;
    }
}

