using Infrastructure.Data;
using Infrastructure.Data.Seeders;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.OpenApi;
using Middleware;
using MassTransit;
using FluentValidation;
using FluentValidation.AspNetCore;
using Application.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
}
);
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
    {
        var host = builder.Configuration.GetConnectionString("PgHost") ?? throw new InvalidOperationException("Postgres host not found.");
        var port = builder.Configuration.GetConnectionString("PgPort") ?? throw new InvalidOperationException("Postgres port not found.");
        var username = builder.Configuration.GetConnectionString("PgUsername") ?? throw new InvalidOperationException("Postgres username not found.");
        var password = builder.Configuration.GetConnectionString("PgPassword") ?? throw new InvalidOperationException("Postgres password not found.");
        var connectionString = $"Host={host};Port={port};Database=ebook_identity;Username={username};Password={password}";
        options.UseNpgsql(connectionString);
    });

builder.Services.AddSingleton<IConnectionMultiplexer>(
    sp =>
    {
        var RedisConnectionString = builder.Configuration["Redis:ConnectionString"] ?? throw new InvalidOperationException("Redis connection string not found.");
        return ConnectionMultiplexer.Connect(RedisConnectionString);
    });

// Register application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddAuthentication(Options =>
{
    Options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    Options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    Options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
    {
        var JwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not found.");
        var JwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not found.");
        var JwtKey = builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT Key not found.");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = JwtIssuer,
            ValidAudience = JwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
        cfg.UseMessageRetry(r =>
            r.Interval(
                builder.Configuration.GetValue<int?>("RabbitMQ:RetryCount") ?? 3,
                TimeSpan.FromSeconds(builder.Configuration.GetValue<int?>("RabbitMQ:RetryIntervalSeconds") ?? 10)));
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.UseMiddleware<GlobalExceptionHandler>();

// Seed default users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DefaultUserSeeder.SeedAsync(services);
}

app.Run();

