using System.Text;
using CsvSmartParser.Exceptions;
using CsvSmartParser.Models;
using CsvSmartParser.Utilities;

namespace CsvSmartParser;

/// <summary>
/// A robust CSV parser with auto-detection capabilities and template-based validation.
/// </summary>
public class CsvParser
{
    private readonly CsvParsingOptions _options;

    /// <summary>
    /// Initializes a new instance of the CsvParser class with default options.
    /// </summary>
    public CsvParser() : this(new CsvParsingOptions()) { }

    /// <summary>
    /// Initializes a new instance of the CsvParser class with custom options.
    /// </summary>
    /// <param name="options">The parsing options to use.</param>
    public CsvParser(CsvParsingOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Loads and parses a CSV file from the specified path.
    /// </summary>
    /// <param name="filePath">The path to the CSV file.</param>
    /// <param name="template">Optional template for strict column mapping and validation.</param>
    /// <param name="encoding">Optional encoding. If not provided, encoding will be auto-detected.</param>
    /// <returns>A list of dictionaries representing the parsed CSV data.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file is not found.</exception>
    /// <exception cref="CsvParsingException">Thrown when parsing fails.</exception>
    /// <exception cref="CsvTemplateException">Thrown when template validation fails.</exception>
    public async Task<List<Dictionary<string, string>>> ParseFileAsync(
        string filePath,
        CsvTemplate? template = null,
        Encoding? encoding = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        try
        {
            // Detect encoding if not provided
            encoding ??= await EncodingDetector.DetectEncodingAsync(filePath);

            // Read file content
            var csvData = await File.ReadAllTextAsync(filePath, encoding);

            return await ParseStringAsync(csvData, template);
        }
        catch (Exception ex) when (!(ex is CsvParsingException || ex is CsvTemplateException || ex is CsvValidationException))
        {
            throw new CsvParsingException($"Failed to parse CSV file '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Parses CSV data from a raw string.
    /// </summary>
    /// <param name="csvData">The raw CSV data string.</param>
    /// <param name="template">Optional template for strict column mapping and validation.</param>
    /// <returns>A list of dictionaries representing the parsed CSV data.</returns>
    /// <exception cref="CsvParsingException">Thrown when parsing fails.</exception>
    /// <exception cref="CsvTemplateException">Thrown when template validation fails.</exception>
    public async Task<List<Dictionary<string, string>>> ParseStringAsync(
        string csvData,
        CsvTemplate? template = null)
    {
        if (string.IsNullOrEmpty(csvData))
            return new List<Dictionary<string, string>>();

        try
        {
            return await Task.Run(() => ParseCsvData(csvData, template));
        }
        catch (Exception ex) when (!(ex is CsvParsingException || ex is CsvTemplateException || ex is CsvValidationException))
        {
            throw new CsvParsingException($"Failed to parse CSV data: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Parses CSV data from a stream for large file support.
    /// </summary>
    /// <param name="stream">The stream containing CSV data.</param>
    /// <param name="template">Optional template for strict column mapping and validation.</param>
    /// <param name="encoding">The encoding to use for reading the stream.</param>
    /// <returns>An async enumerable of dictionaries representing the parsed CSV data.</returns>
    public async IAsyncEnumerable<Dictionary<string, string>> ParseStreamAsync(
        Stream stream,
        CsvTemplate? template = null,
        Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;

        using var reader = new StreamReader(stream, encoding, bufferSize: _options.BufferSize);

        string? headerLine = await reader.ReadLineAsync();
        if (string.IsNullOrEmpty(headerLine))
            yield break;

        var delimiter = _options.Delimiter ?? DelimiterDetector.DetectDelimiter(headerLine, 1);
        var headers = ParseLine(headerLine, delimiter).ToArray();

        // Validate headers against template if provided
        template?.ValidateHeaders(headers);

        var rowIndex = 1;
        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (_options.SkipEmptyLines && string.IsNullOrWhiteSpace(line))
                continue;

            var values = ParseLine(line, delimiter).ToArray();
            var row = CreateRowDictionary(headers, values);

            // Validate row against template if provided
            template?.ValidateRow(row, rowIndex);

            yield return row;
            rowIndex++;
        }
    }

    private List<Dictionary<string, string>> ParseCsvData(string csvData, CsvTemplate? template)
    {
        var lines = csvData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0)
            return new List<Dictionary<string, string>>();

        // Detect delimiter
        var delimiter = _options.Delimiter ?? DelimiterDetector.DetectDelimiter(csvData);

        // Parse headers
        var headers = ParseLine(lines[0], delimiter).ToArray();

        // Validate headers against template if provided
        template?.ValidateHeaders(headers);

        var result = new List<Dictionary<string, string>>();
        var startIndex = _options.HasHeaders ? 1 : 0;

        for (var i = startIndex; i < lines.Length; i++)
        {
            var line = lines[i];

            if (_options.SkipEmptyLines && string.IsNullOrWhiteSpace(line))
                continue;

            var values = ParseLine(line, delimiter).ToArray();
            var row = CreateRowDictionary(headers, values);

            // Validate row against template if provided
            template?.ValidateRow(row, i);

            result.Add(row);
        }

        return result;
    }

    private IEnumerable<string> ParseLine(string line, char delimiter)
    {
        var values = new List<string>();
        var currentValue = new StringBuilder();
        var inQuotes = false;
        var i = 0;

        while (i < line.Length)
        {
            var currentChar = line[i];

            if (currentChar == _options.QuoteChar)
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == _options.QuoteChar)
                {
                    // Escaped quote
                    currentValue.Append(_options.QuoteChar);
                    i += 2;
                }
                else
                {
                    inQuotes = !inQuotes;
                    i++;
                }
            }
            else if (currentChar == delimiter && !inQuotes)
            {
                values.Add(ProcessValue(currentValue.ToString()));
                currentValue.Clear();
                i++;
            }
            else
            {
                currentValue.Append(currentChar);
                i++;
            }
        }

        // Add the last value
        values.Add(ProcessValue(currentValue.ToString()));

        return values;
    }

    private string ProcessValue(string value)
    {
        return _options.TrimWhitespace ? value.Trim() : value;
    }

    private static Dictionary<string, string> CreateRowDictionary(string[] headers, string[] values)
    {
        var row = new Dictionary<string, string>();

        for (var i = 0; i < headers.Length; i++)
        {
            var value = i < values.Length ? values[i] : string.Empty;
            row[headers[i]] = value;
        }

        return row;
    }
}



