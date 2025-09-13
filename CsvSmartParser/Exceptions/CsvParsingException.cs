namespace CsvSmartParser.Exceptions;

/// <summary>
/// Exception thrown when CSV parsing fails.
/// </summary>
public class CsvParsingException : Exception
{
    /// <summary>
    /// Initializes a new instance of the CsvParsingException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public CsvParsingException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the CsvParsingException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public CsvParsingException(string message, Exception innerException) : base(message, innerException) { }
}
