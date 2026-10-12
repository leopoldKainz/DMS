using DMS.API.Filters;
using DMS.Business.Interfaces;
using DMS.Business.Models.Validation;
using DMS.Business.Services;
using DMS.DAL;
using DMS.DAL.DTOs;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DMS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<DmsDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });
            
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = "";
                    options.Audience = "";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true
                    };
                });
            
            builder.Services.AddMapster();
            TypeAdapterConfig.GlobalSettings.Scan(typeof(MappingConfig).Assembly);
            builder.Services.AddScoped<IDocumentService, DocumentService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ValidationFilter>();
            });
            builder.Services.AddOpenApi();

            var app = builder.Build();
            app.UseExceptionHandler();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "DMS API v1");
                    options.RoutePrefix = "swagger";
                });
            }

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
