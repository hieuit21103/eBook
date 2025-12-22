using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using Infrastructure.Data;
using Domain.Interfaces;
using Infrastructure.Repositories;
using Application.Interfaces;
using Application.Services;
using Middleware;
using MassTransit;
using IntegrationEvents;

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

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
    {
        var host = builder.Configuration["PgHost"] ?? throw new InvalidOperationException("Postgres host not found.");
        var port = builder.Configuration["PgPort"] ?? throw new InvalidOperationException("Postgres port not found.");
        var username = builder.Configuration["PgUsername"] ?? throw new InvalidOperationException("Postgres username not found.");
        var password = builder.Configuration["PgPassword"] ?? throw new InvalidOperationException("Postgres password not found.");
        var connectionString = $"Host={host};Port={port};Database=ebook_documents;Username={username};Password={password}";
        options.UseNpgsql(connectionString);
    });

// Register repositories
builder.Services.AddScoped<IBookmarkRepository, BookmarkRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IPageRepository, PageRepository>();
builder.Services.AddScoped<IDocumentCategoryRepository, DocumentCategoryRepository>();

// Register services
builder.Services.AddScoped<IBookmarkService, BookmarkService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IPageService, PageService>();

// GRPC Channel
builder.Services.AddGrpcClient<FileStorage.Protos.FileStorageService.FileStorageServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["FileStorage:GrpcUrl"] ?? "http://localhost:5001");
});

// RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserStatusChangedConsumer>();
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
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<GlobalExceptionHandler>();

app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
