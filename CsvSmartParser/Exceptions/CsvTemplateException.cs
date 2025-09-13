namespace CsvSmartParser.Exceptions;

/// <summary>
/// Exception thrown when CSV template validation fails.
/// </summary>
public class CsvTemplateException : Exception
{
    /// <summary>
    /// Initializes a new instance of the CsvTemplateException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public CsvTemplateException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the CsvTemplateException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public CsvTemplateException(string message, Exception innerException) : base(message, innerException) { }
}
