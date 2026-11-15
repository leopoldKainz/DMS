using DMS.OCR.Options;
using DMS.OCR.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.OCR;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOcr(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(OcrOptions.SectionName);
        services.Configure<OcrOptions>(section);
        services.AddSingleton<IOcrService, GhostscriptTesseractOcrService>();

        return services;
    }
}
