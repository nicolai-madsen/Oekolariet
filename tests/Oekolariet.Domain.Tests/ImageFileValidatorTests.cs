using Oekolariet.Domain.Media;

namespace Oekolariet.Domain.Tests;

// Unit tests for ImageFileValidator (US14 AC-03)
public class ImageFileValidatorTests
{
    private static readonly byte[] Png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    private static readonly byte[] Exe = new byte[] { 0x4D, 0x5A, 0x90, 0x00 };

    // The file content decides the type. Size is the same (1 KB) in every row.
    [Theory]
    [InlineData(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, ImageFileCheckResult.Allowed)] // PNG
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, ImageFileCheckResult.Allowed)] // JPEG
    [InlineData(new byte[] { 0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50 }, ImageFileCheckResult.Allowed)] // WebP
    [InlineData(new byte[] { 0x4D, 0x5A, 0x90, 0x00 }, ImageFileCheckResult.UnsupportedType)] // .exe renamed to .png
    [InlineData(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, ImageFileCheckResult.UnsupportedType)] // too short
    [InlineData(new byte[] { 0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00, 0x57, 0x41, 0x56, 0x45 }, ImageFileCheckResult.UnsupportedType)] // RIFF but WAVE (sound)
    public void Check_FileContent_ReturnsExpectedResult(byte[] header, ImageFileCheckResult expected)
    {
        // Arrange
        long size = 1024;

        // Act
        var result = ImageFileValidator.Check(header, size);

        // Assert
        Assert.Equal(expected, result);
    }

    // The size decides the result. The content is always a valid PNG.
    [Theory]
    [InlineData(0L, ImageFileCheckResult.Empty)]
    [InlineData(ImageFileValidator.MaxSizeBytes, ImageFileCheckResult.Allowed)] // exactly 5 MB
    [InlineData(ImageFileValidator.MaxSizeBytes + 1, ImageFileCheckResult.TooLarge)] // 5 MB + 1 byte
    public void Check_FileSize_ReturnsExpectedResult(long size, ImageFileCheckResult expected)
    {
        // Act
        var result = ImageFileValidator.Check(Png, size);

        // Assert
        Assert.Equal(expected, result);
    }

    // Size is checked before type, so a huge non-image is reported as too large.
    [Fact]
    public void Check_TooLargeExe_ReturnsTooLarge()
    {
        // Arrange
        long size = ImageFileValidator.MaxSizeBytes + 1;

        // Act
        var result = ImageFileValidator.Check(Exe, size);

        // Assert
        Assert.Equal(ImageFileCheckResult.TooLarge, result);
    }
}
