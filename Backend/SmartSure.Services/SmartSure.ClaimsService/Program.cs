using SmartSure.Shared.Contracts.Extensions;
using SmartSure.Shared.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartSure.ClaimsService.Consumers;
using SmartSure.ClaimsService.Data;
using SmartSure.ClaimsService.Repositories;
using SmartSure.ClaimsService.Services;
using SmartSure.Shared.Messaging;
using System.Text;

// Load .env — walk up from the executable directory to find the .env in the project root.
// This works whether running via VS (bin/Debug/net10.0), dotnet run, or dotnet exec.
{
    var envPath = Path.Combine(AppContext.BaseDirectory, ".env");
    if (!File.Exists(envPath))
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, ".env")))
            dir = dir.Parent;
        if (dir != null)
            envPath = Path.Combine(dir.FullName, ".env");
    }
    if (File.Exists(envPath))
        DotNetEnv.Env.Load(envPath);
}
var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging("ClaimsService");
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
        var claimsAudience = builder.Configuration["Jwt:Aud3"] ?? throw new InvalidOperationException("Jwt:Aud3 is missing.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = claimsAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddDbContext<ClaimsDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("ClaimsDb");
    options.UseSqlServer(connectionString);
});
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<ClaimStatusChangedConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        var options = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>() ?? new RabbitMqOptions();
        var virtualHost = options.VirtualHost == "/" ? "/" : options.VirtualHost.TrimStart('/');
        cfg.Host(options.HostName, virtualHost, h =>
        {
            h.Username(options.UserName);
            h.Password(options.Password);
        });

        cfg.ReceiveEndpoint("claims-status-changed", endpoint =>
        {
            endpoint.ConfigureConsumer<ClaimStatusChangedConsumer>(context);
        });
    });
});
builder.Services.AddScoped<IClaimRepository, ClaimRepository>();
builder.Services.AddScoped<IClaimEventPublisher, ClaimEventPublisher>();
builder.Services.AddScoped<IMegaStorageService, MegaStorageService>();
builder.Services.AddHttpClient<IPolicyVerificationService, PolicyVerificationService>();
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IClaimAdminService, ClaimAdminService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ClaimsService API", Version = "v1" });

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

// Serve uploaded claim documents as static files from /uploads/*
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.MapControllers();
app.Run();
