using DMS.OCR.Options;
using DMS.OCR.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace DMS.OCR.Test;

[TestFixture]
public class GhostscriptTesseractOcrServiceTests
{
    private GhostscriptTesseractOcrService _service = null!;

    [SetUp]
    public void SetUp()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new OcrOptions
        {
            GhostscriptPath = "gs",
            TesseractPath = "tesseract",
            Language = "deu+eng",
            Dpi = 300
        });

        _service = new GhostscriptTesseractOcrService(options, NullLogger<GhostscriptTesseractOcrService>.Instance);
    }

    [Test]
    public void ProcessPdfAsync_FileDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.pdf");

        // Act & Assert
        Assert.ThrowsAsync<FileNotFoundException>(async () => await _service.ProcessPdfAsync(nonExistentPath));
    }
}
