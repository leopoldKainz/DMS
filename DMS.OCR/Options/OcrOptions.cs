namespace DMS.OCR.Options;

public class OcrOptions
{
    public const string SectionName = "Ocr";

    public string GhostscriptPath { get; set; } = "gs";
    public string TesseractPath { get; set; } = "tesseract";
    public string Language { get; set; } = "deu+eng";
    public int Dpi { get; set; } = 300;
    public string? TessDataPrefix { get; set; }
}
