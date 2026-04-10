using SmartSure.Shared.Contracts.Extensions;
using SmartSure.Shared.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartSure.IdentityService.Helpers;
using SmartSure.IdentityService.Consumers;
using SmartSure.IdentityService.Data;
using SmartSure.IdentityService.Repositories;
using SmartSure.IdentityService.Services;
using SmartSure.Shared.Messaging;
using System.Text;

// Load .env file for environment variables
DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging("IdentityService");
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
        var audiences = GetJwtAudiences(builder.Configuration);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = audiences.Length > 0,
            ValidAudiences = audiences,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();
var authConnectionString = builder.Configuration.GetConnectionString("AuthDb");
builder.Services.AddDbContext<IdentityDbContext>(ConfigureIdentityDbContext);
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<PolicyActivatedConsumer>();
    config.AddConsumer<PolicyCancelledConsumer>();
    config.AddConsumer<ClaimStatusChangedConsumer>();
    config.AddConsumer<ClaimApprovedConsumer>();
    config.AddConsumer<ClaimRejectedConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        var options = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>() ?? new RabbitMqOptions();
        var virtualHost = options.VirtualHost == "/" ? "/" : options.VirtualHost.TrimStart('/');
        cfg.Host(options.HostName, virtualHost, h =>
        {
            h.Username(options.UserName);
            h.Password(options.Password);
        });

        cfg.ReceiveEndpoint("identity-policy-activated", endpoint =>
        {
            endpoint.ConfigureConsumer<PolicyActivatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("identity-claim-status-changed", endpoint =>
        {
            endpoint.ConfigureConsumer<ClaimStatusChangedConsumer>(context);
        });

        cfg.ReceiveEndpoint("identity-policy-cancelled", endpoint =>
        {
            endpoint.ConfigureConsumer<PolicyCancelledConsumer>(context);
        });

        cfg.ReceiveEndpoint("identity-claim-approved", endpoint =>
        {
            endpoint.ConfigureConsumer<ClaimApprovedConsumer>(context);
        });

        cfg.ReceiveEndpoint("identity-claim-rejected", endpoint =>
        {
            endpoint.ConfigureConsumer<ClaimRejectedConsumer>(context);
        });
    });
});
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserRegisteredEventPublisher, UserRegisteredEventPublisher>();
builder.Services.AddHttpClient<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IUserAdministrationService, UserAdministrationService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "IdentityService API", Version = "v1" });

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

void ConfigureIdentityDbContext(DbContextOptionsBuilder options)
{
    options.UseSqlServer(authConnectionString);
}

string[] GetJwtAudiences(IConfiguration configuration)
{
    var audiences = new List<string>();
    AddAudience("Jwt:Aud1");
    AddAudience("Jwt:Aud2");
    AddAudience("Jwt:Aud3");
    AddAudience("Jwt:Aud4");
    AddAudience("Jwt:Aud5");
    return audiences.ToArray();

    void AddAudience(string key)
    {
        var audience = configuration[key];
        if (!string.IsNullOrWhiteSpace(audience))
        {
            audiences.Add(audience);
        }
    }
}
