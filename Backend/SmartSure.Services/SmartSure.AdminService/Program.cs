using SmartSure.Shared.Contracts.Extensions;
using SmartSure.Shared.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartSure.AdminService.Consumers;
using SmartSure.AdminService.Data;
using SmartSure.AdminService.Repositories;
using SmartSure.AdminService.Services;
using SmartSure.Shared.Messaging;
using System.Text;

// Load .env file for environment variables
DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging("AdminService");
builder.Services.AddCors(options =>
{
    options.AddPolicy("ServiceCors", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200", "https://localhost:4200", "http://localhost:3000", "https://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddMemoryCache();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
        var issuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
        var adminAudience = builder.Configuration["Jwt:Aud2"] ?? throw new InvalidOperationException("Jwt:Aud2 is missing.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = adminAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddDbContext<AdminDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("AdminDb");
    options.UseSqlServer(connectionString);
});
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<AuditEventConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        var options = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>() ?? new RabbitMqOptions();
        var virtualHost = options.VirtualHost == "/" ? "/" : options.VirtualHost.TrimStart('/');
        cfg.Host(options.HostName, virtualHost, h =>
        {
            h.Username(options.UserName);
            h.Password(options.Password);
        });

        cfg.ReceiveEndpoint("admin-audit-logs", endpoint =>
        {
            endpoint.ConfigureConsumer<AuditEventConsumer>(context);
        });
    });
});
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "AdminService API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter token as: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("ServiceCors");
app.UseAuthentication();
app.UseAuthorization();
app.UseGlobalExceptionHandler();
app.MapControllers();

app.Run();
