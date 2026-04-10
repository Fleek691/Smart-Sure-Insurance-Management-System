using SmartSure.Shared.Contracts.Extensions;
using SmartSure.Shared.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartSure.PolicyService.Consumers;
using SmartSure.PolicyService.Data;
using SmartSure.PolicyService.Repositories;
using SmartSure.PolicyService.Services;
using SmartSure.Shared.Messaging;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging("PolicyService");
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
        var policyAudience = builder.Configuration["Jwt:Aud4"] ?? throw new InvalidOperationException("Jwt:Aud4 is missing.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = policyAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddDbContext<PolicyDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("PolicyDb");
    options.UseSqlServer(connectionString);
});
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<ClaimSubmittedConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        var options = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>() ?? new RabbitMqOptions();
        var virtualHost = options.VirtualHost == "/" ? "/" : options.VirtualHost.TrimStart('/');
        cfg.Host(options.HostName, virtualHost, h =>
        {
            h.Username(options.UserName);
            h.Password(options.Password);
        });

        cfg.ReceiveEndpoint("policy-claim-submitted", endpoint =>
        {
            endpoint.ConfigureConsumer<ClaimSubmittedConsumer>(context);
        });
    });
});
builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
builder.Services.AddScoped<IPolicyEventPublisher, PolicyEventPublisher>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "PolicyService API", Version = "v1" });

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
