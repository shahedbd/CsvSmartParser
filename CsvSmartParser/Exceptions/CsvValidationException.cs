namespace CsvSmartParser.Exceptions;

/// <summary>
/// Exception thrown when CSV data validation fails.
/// </summary>
public class CsvValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the CsvValidationException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public CsvValidationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the CsvValidationException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public CsvValidationException(string message, Exception innerException) : base(message, innerException) { }
}
