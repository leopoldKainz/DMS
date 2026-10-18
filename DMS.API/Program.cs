using System.Text;
using DMS.API.Auth;
using DMS.API.Filters;
using DMS.Business.Interfaces;
using DMS.Business.Models;
using DMS.Business.Models.Validation;
using DMS.Business.Services;
using DMS.DAL;
using DMS.DAL.DTOs;
using FluentValidation;
using Mapster;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace DMS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<DmsDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });
            
            var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
            var jwtOptions = jwtSection.Get<JwtOptions>()
                ?? throw new InvalidOperationException("Missing 'Jwt' configuration section.");
            builder.Services.Configure<JwtOptions>(jwtSection);
            builder.Services.AddSingleton<ITokenService, TokenService>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                });
            builder.Services.AddAuthorization();

            builder.Services.AddMapster();
            TypeAdapterConfig.GlobalSettings.Scan(typeof(MappingConfig).Assembly);
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IDocumentService, DocumentService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserDocumentService, UserDocumentService>();
            builder.Services.AddScoped<IShareRequestService, ShareRequestService>();
            builder.Services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ValidationFilter>();
            });
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, _, _) =>
                {
                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                    document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Description = "Token from POST /User/login"
                    };
                    document.Security =
                    [
                        new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] }
                    ];
                    return Task.CompletedTask;
                });
            });
            
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<OcrWorkerConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var connectionString = builder.Configuration.GetConnectionString("RabbitMq") 
                                           ?? throw new InvalidOperationException("RabbitMQ connection string is missing.");

                    cfg.Host(connectionString);
                    cfg.ConfigureEndpoints(context);
                });
            });

            var app = builder.Build();
            app.UseExceptionHandler();

           
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "DMS API v1");
                    options.RoutePrefix = "swagger";
                });
            }

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
