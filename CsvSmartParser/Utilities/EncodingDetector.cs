using System.Text;

namespace CsvSmartParser.Utilities;

/// <summary>
/// Utility class for detecting file encoding.
/// </summary>
internal static class EncodingDetector
{
    /// <summary>
    /// Detects the encoding of a file by examining its byte order mark (BOM) and content.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <returns>The detected encoding.</returns>
    public static async Task<Encoding> DetectEncodingAsync(string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var buffer = new byte[4];
        var bytesRead = await fileStream.ReadAsync(buffer, 0, 4);

        // Check for BOM
        if (bytesRead >= 3)
        {
            if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
                return Encoding.UTF8;

            if (buffer[0] == 0xFF && buffer[1] == 0xFE)
            {
                if (bytesRead >= 4 && buffer[2] == 0x00 && buffer[3] == 0x00)
                    return Encoding.UTF32; // UTF-32 LE
                return Encoding.Unicode; // UTF-16 LE
            }

            if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                return Encoding.BigEndianUnicode; // UTF-16 BE

            if (bytesRead >= 4 && buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0xFE && buffer[3] == 0xFF)
                return new UTF32Encoding(true, true); // UTF-32 BE
        }

        // If no BOM, try to detect by content analysis
        fileStream.Seek(0, SeekOrigin.Begin);
        var sampleBuffer = new byte[Math.Min(1024, fileStream.Length)];
        await fileStream.ReadAsync(sampleBuffer, 0, sampleBuffer.Length);

        // Check for UTF-8 validity
        if (IsValidUtf8(sampleBuffer))
            return Encoding.UTF8;

        // Default to system default encoding
        return Encoding.Default;
    }

    /// <summary>
    /// Detects the encoding of a byte array.
    /// </summary>
    /// <param name="data">The byte array to analyze.</param>
    /// <returns>The detected encoding.</returns>
    public static Encoding DetectEncoding(byte[] data)
    {
        if (data.Length >= 3)
        {
            if (data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
                return Encoding.UTF8;

            if (data[0] == 0xFF && data[1] == 0xFE)
            {
                if (data.Length >= 4 && data[2] == 0x00 && data[3] == 0x00)
                    return Encoding.UTF32;
                return Encoding.Unicode;
            }

            if (data[0] == 0xFE && data[1] == 0xFF)
                return Encoding.BigEndianUnicode;

            if (data.Length >= 4 && data[0] == 0x00 && data[1] == 0x00 && data[2] == 0xFE && data[3] == 0xFF)
                return new UTF32Encoding(true, true);
        }

        if (IsValidUtf8(data))
            return Encoding.UTF8;

        return Encoding.Default;
    }

    private static bool IsValidUtf8(byte[] data)
    {
        try
        {
            var decoder = Encoding.UTF8.GetDecoder();
            decoder.Fallback = DecoderFallback.ExceptionFallback;

            var charCount = decoder.GetCharCount(data, 0, data.Length, true);
            return charCount > 0;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }
}
