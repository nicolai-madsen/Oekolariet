namespace Oekolariet.Domain.Media;

// Decides whether an uploaded file is an allowed image.
// Only PNG, JPEG and WebP up to 5 MB are allowed.
// The type is decided by the file's first bytes (magic numbers), never by the file name or extension.
public static class ImageFileValidator
{
    // 5 MB fits a Full HD (and even a 4K) photo saved as JPEG. Screen resolution is an assumption.
    public const long MaxSizeBytes = 5 * 1024 * 1024;

    // The first bytes that each allowed file type starts with
    private static readonly byte[] PngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    private static readonly byte[] JpegSignature = new byte[] { 0xFF, 0xD8, 0xFF };
    private static readonly byte[] WebpRiff = new byte[] { 0x52, 0x49, 0x46, 0x46 }; // "RIFF"
    private static readonly byte[] WebpWebp = new byte[] { 0x57, 0x45, 0x42, 0x50 }; // "WEBP"

    // Checks the size first, then the file type, and returns the reason if the file is not allowed.
    // header: the first bytes of the file (at least 12 bytes are needed to recognise WebP).
    // sizeBytes: the total size of the file in bytes.
    public static ImageFileCheckResult Check(byte[] header, long sizeBytes)
    {
        if (sizeBytes <= 0)
        {
            return ImageFileCheckResult.Empty;
        }

        if (sizeBytes > MaxSizeBytes)
        {
            return ImageFileCheckResult.TooLarge;
        }

        bool isPng = HasBytesAt(header, PngSignature, 0);
        bool isJpeg = HasBytesAt(header, JpegSignature, 0);

        // WebP: "RIFF" at byte 0, then 4 bytes of size (any value), then "WEBP" at byte 8.
        bool isWebp = HasBytesAt(header, WebpRiff, 0) && HasBytesAt(header, WebpWebp, 8);

        if (isPng || isJpeg || isWebp)
        {
            return ImageFileCheckResult.Allowed;
        }

        return ImageFileCheckResult.UnsupportedType;
    }

    // Returns true if 'data' contains exactly the bytes in 'expected', starting at position 'start'.
    // Returns false if 'data' is too short.
    private static bool HasBytesAt(byte[] data, byte[] expected, int start)
    {
        if (data.Length < start + expected.Length)
        {
            return false; // The file is too short to contain these bytes.
        }

        for (int i = 0; i < expected.Length; i++)
        {
            if (data[start + i] != expected[i])
            {
                return false;
            }
        }

        return true;
    }
}
