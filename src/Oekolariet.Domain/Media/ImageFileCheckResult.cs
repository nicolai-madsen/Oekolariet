namespace Oekolariet.Domain.Media;

// The result of checking an uploaded image file.
// The API turns each value into an HTTP status and a Danish message.
public enum ImageFileCheckResult
{
    Allowed,         // PNG, JPEG or WebP, max 5 MB
    Empty,           // the file has no content (0 bytes)
    TooLarge,        // bigger than 5 MB (413)
    UnsupportedType  // not PNG, JPEG or WebP (415)
}
