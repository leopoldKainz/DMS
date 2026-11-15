using DMS.ELK;
using DMS.OCR.Consumers;
using DMS.Storage;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DMS.OCR;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        // Configure Logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        // Configure Storage (RustFS S3)
        builder.Services.AddFileStorage(builder.Configuration);

        // Configure Search Index (Elasticsearch)
        builder.Services.AddSearchIndex(builder.Configuration);

        // Configure OCR
        builder.Services.AddOcr(builder.Configuration);

        // Configure MassTransit with RabbitMQ
        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<OcrWorkerConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var connectionString = builder.Configuration.GetConnectionString("RabbitMq")
                                       ?? throw new InvalidOperationException("RabbitMQ connection string is missing.");
                cfg.Host(connectionString);

                cfg.ReceiveEndpoint("ocr-process-queue", e =>
                {
                    e.ConfigureConsumer<OcrWorkerConsumer>(context);
                });
            });
        });

        var host = builder.Build();
        host.Run();
    }
}
