using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using DMS.OCR.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DMS.OCR.Services;

public class GhostscriptTesseractOcrService : IOcrService
{
    private readonly OcrOptions _options;
    private readonly ILogger<GhostscriptTesseractOcrService> _logger;

    public GhostscriptTesseractOcrService(
        IOptions<OcrOptions> options,
        ILogger<GhostscriptTesseractOcrService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> ProcessPdfAsync(string pdfFilePath, CancellationToken ct = default)
    {
        if (!File.Exists(pdfFilePath))
        {
            throw new FileNotFoundException($"PDF file not found: {pdfFilePath}");
        }

        var workDir = Path.Combine(Path.GetTempPath(), "dms_ocr", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDir);

        try
        {
            return await ProcessPdfPageByPageAsync(pdfFilePath, workDir, ct);
        }
        finally
        {
            try
            {
                if (Directory.Exists(workDir))
                {
                    Directory.Delete(workDir, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clean up temporary directory {WorkDir}", workDir);
            }
        }
    }

    private async Task<string> ProcessPdfPageByPageAsync(string pdfFilePath, string workDir, CancellationToken ct)
    {
        var pageImagePattern = Path.Combine(workDir, "page_%04d.png");

        // Step 1: Rasterize PDF pages to individual 300 DPI PNG images using Ghostscript
        var gsArgs = $"-sDEVICE=png16m -r{_options.Dpi} -dNOPAUSE -dBATCH -dSAFER -sOutputFile=\"{pageImagePattern}\" \"{pdfFilePath}\"";
        _logger.LogInformation("Rasterizing PDF pages with Ghostscript: {GsBinary} {Args}", _options.GhostscriptPath, gsArgs);

        var (gsExitCode, _, gsStderr) = await RunProcessAsync(_options.GhostscriptPath, gsArgs, ct);
        if (gsExitCode != 0)
        {
            throw new InvalidOperationException($"Ghostscript rasterization failed with code {gsExitCode}: {gsStderr}");
        }

        // Step 2: Retrieve rasterized page images, sorted numerically by page number
        var pageFiles = Directory.GetFiles(workDir, "page_*.png")
            .OrderBy(f =>
            {
                var match = Regex.Match(Path.GetFileNameWithoutExtension(f), @"\d+");
                return match.Success && int.TryParse(match.Value, out var n) ? n : 0;
            })
            .ToList();

        if (pageFiles.Count == 0)
        {
            _logger.LogWarning("No page images were produced by Ghostscript for {PdfPath}", pdfFilePath);
            return string.Empty;
        }

        _logger.LogInformation("Document '{PdfPath}' split into {TotalPages} page(s). Starting page-by-page OCR...",
            pdfFilePath, pageFiles.Count);

        var sb = new StringBuilder();

        // Step 3: Run OCR on each page individually and accumulate results
        for (int i = 0; i < pageFiles.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            var pageNumber = i + 1;
            var pageFile = pageFiles[i];
            var pageOutputBase = Path.Combine(workDir, $"tess_page_{pageNumber:D4}");
            var pageTxtPath = $"{pageOutputBase}.txt";

            _logger.LogInformation("Running OCR on page {PageNumber}/{TotalPages} ({PageFile})...",
                pageNumber, pageFiles.Count, Path.GetFileName(pageFile));

            var pageText = await OcrPageAsync(pdfFilePath, pageFile, pageOutputBase, pageNumber, workDir, ct);

            if (!string.IsNullOrWhiteSpace(pageText))
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }
                sb.Append(pageText.Trim());

                _logger.LogInformation("Page {PageNumber}/{TotalPages} completed ({CharCount} characters extracted).",
                    pageNumber, pageFiles.Count, pageText.Trim().Length);
            }
            else
            {
                _logger.LogInformation("Page {PageNumber}/{TotalPages} completed (no text found or empty page).",
                    pageNumber, pageFiles.Count);
            }

            // Clean up individual page files as we go to save disk space
            try
            {
                if (File.Exists(pageFile)) File.Delete(pageFile);
                if (File.Exists(pageTxtPath)) File.Delete(pageTxtPath);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to clean up temporary files for page {PageNumber}", pageNumber);
            }
        }

        var fullResult = sb.ToString().Trim();
        _logger.LogInformation("Completed page-by-page OCR for '{PdfPath}' ({TotalPages} pages processed, {TotalChars} total characters extracted).",
            pdfFilePath, pageFiles.Count, fullResult.Length);

        return fullResult;
    }

    private async Task<string> OcrPageAsync(
        string pdfFilePath,
        string pageImageFile,
        string pageOutputBase,
        int pageNumber,
        string workDir,
        CancellationToken ct)
    {
        var pageTxtPath = $"{pageOutputBase}.txt";

        // Primary per-page strategy: Tesseract CLI on rasterized page image
        var tessArgs = $"\"{pageImageFile}\" \"{pageOutputBase}\" -l {_options.Language} --psm 3";
        var (tessExitCode, _, tessStderr) = await RunProcessAsync(_options.TesseractPath, tessArgs, ct);

        if (tessExitCode == 0 && File.Exists(pageTxtPath))
        {
            return await File.ReadAllTextAsync(pageTxtPath, Encoding.UTF8, ct);
        }

        _logger.LogWarning("Tesseract CLI failed on page {PageNumber} with code {ExitCode}: {Stderr}. Trying Ghostscript single-page OCR fallback...",
            pageNumber, tessExitCode, tessStderr);

        // Fallback per-page strategy: Ghostscript direct OCR constrained to only this page
        var gsPageTxtPath = Path.Combine(workDir, $"gs_page_{pageNumber:D4}.txt");
        var gsArgs = $"-sDEVICE=ocr -r{_options.Dpi} -sOCRLanguage=\"{_options.Language}\" -dFirstPage={pageNumber} -dLastPage={pageNumber} -dNOPAUSE -dBATCH -dSAFER -sOutputFile=\"{gsPageTxtPath}\" \"{pdfFilePath}\"";

        var (gsExitCode, _, gsStderr) = await RunProcessAsync(_options.GhostscriptPath, gsArgs, ct);
        if (gsExitCode == 0 && File.Exists(gsPageTxtPath))
        {
            var text = await File.ReadAllTextAsync(gsPageTxtPath, Encoding.UTF8, ct);
            try { File.Delete(gsPageTxtPath); } catch { /* ignore */ }
            return text;
        }

        _logger.LogWarning("Ghostscript single-page OCR also failed on page {PageNumber} with code {ExitCode}: {Stderr}",
            pageNumber, gsExitCode, gsStderr);

        return string.Empty;
    }

    private async Task<(int ExitCode, string Stdout, string Stderr)> RunProcessAsync(string binary, string arguments, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = binary,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (!string.IsNullOrWhiteSpace(_options.TessDataPrefix))
        {
            psi.EnvironmentVariables["TESSDATA_PREFIX"] = _options.TessDataPrefix;
        }
        else if (Environment.GetEnvironmentVariable("TESSDATA_PREFIX") is { Length: > 0 } envPrefix)
        {
            psi.EnvironmentVariables["TESSDATA_PREFIX"] = envPrefix;
        }

        try
        {
            using var process = new Process { StartInfo = psi };
            var stdoutBuilder = new StringBuilder();
            var stderrBuilder = new StringBuilder();

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null) stdoutBuilder.AppendLine(e.Data);
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null) stderrBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(ct);

            return (process.ExitCode, stdoutBuilder.ToString(), stderrBuilder.ToString());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to start process {Binary}", binary);
            return (-1, string.Empty, ex.Message);
        }
    }
}
