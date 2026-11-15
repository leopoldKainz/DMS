using Amazon.Runtime;
using Amazon.S3;
using DMS.Storage.Interfaces;
using DMS.Storage.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DMS.Storage;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(RustFsOptions.SectionName);
        var options = section.Get<RustFsOptions>()
            ?? throw new InvalidOperationException($"Missing '{RustFsOptions.SectionName}' configuration section.");

        if (string.IsNullOrWhiteSpace(options.Endpoint))
        {
            throw new InvalidOperationException($"'{RustFsOptions.SectionName}:Endpoint' is not configured.");
        }

        services.Configure<RustFsOptions>(section);
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var rustFsOptions = sp.GetRequiredService<IOptions<RustFsOptions>>().Value;
            var scheme = rustFsOptions.UseSsl ? "https" : "http";
            var config = new AmazonS3Config
            {
                ServiceURL = $"{scheme}://{rustFsOptions.Endpoint}",
                ForcePathStyle = true,
                AuthenticationRegion = rustFsOptions.Region,
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED
            };
            return new AmazonS3Client(new BasicAWSCredentials(rustFsOptions.AccessKey, rustFsOptions.SecretKey), config);
        });
        services.AddSingleton<IFileStorage, RustFsFileStorage>();

        return services;
    }
}
