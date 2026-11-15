namespace DMS.OCR.Services;

public interface IOcrService
{
    Task<string> ProcessPdfAsync(string pdfFilePath, CancellationToken ct = default);
}
