namespace CsvSmartParser.Utilities;

/// <summary>
/// Utility class for detecting CSV delimiters.
/// </summary>
internal static class DelimiterDetector
{
    private static readonly char[] CommonDelimiters = { ',', ';', '\t', '|', ':' };

    /// <summary>
    /// Detects the most likely delimiter used in the CSV data.
    /// </summary>
    /// <param name="csvData">The CSV data to analyze.</param>
    /// <param name="sampleLines">Number of lines to sample for detection.</param>
    /// <returns>The detected delimiter character.</returns>
    public static char DetectDelimiter(string csvData, int sampleLines = 5)
    {
        var lines = csvData.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                          .Take(sampleLines)
                          .ToArray();

        if (lines.Length == 0)
            return ','; // Default to comma

        var delimiterCounts = new Dictionary<char, int>();

        foreach (var delimiter in CommonDelimiters)
        {
            var counts = lines.Select(line => CountDelimiterOccurrences(line, delimiter)).ToArray();

            // Check if the delimiter appears consistently across lines
            if (counts.Length > 1 && counts.All(c => c == counts[0]) && counts[0] > 0)
            {
                delimiterCounts[delimiter] = counts[0];
            }
            else if (counts.Length == 1 && counts[0] > 0)
            {
                delimiterCounts[delimiter] = counts[0];
            }
        }

        // Return the delimiter with the highest consistent count
        return delimiterCounts.Count > 0
            ? delimiterCounts.OrderByDescending(kvp => kvp.Value).First().Key
            : ',';
    }

    private static int CountDelimiterOccurrences(string line, char delimiter)
    {
        var count = 0;
        var inQuotes = false;
        var quoteChar = '"';

        for (var i = 0; i < line.Length; i++)
        {
            var currentChar = line[i];

            if (currentChar == quoteChar)
            {
                inQuotes = !inQuotes;
            }
            else if (currentChar == delimiter && !inQuotes)
            {
                count++;
            }
        }

        return count;
    }
}
