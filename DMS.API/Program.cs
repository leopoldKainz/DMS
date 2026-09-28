using DMS.Business.Interfaces;
using DMS.Business.Services;
using DMS.DAL.DTOs;
using Mapster;

namespace DMS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddMapster();
            TypeAdapterConfig.GlobalSettings.Scan(typeof(MappingConfig).Assembly);
            builder.Services.AddScoped<IDocumentService, DocumentService>();

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
