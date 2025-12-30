using FileStorage.Infrastructure.Data;
using FileStorage.Infrastructure.Repositories;
using FileStorage.Infrastructure.Services;
using FileStorage.Application.Interfaces;
using FileStorage.Application.Services;
using FileStorage.Domain.Interfaces;
using FileStorage.Application.Options;
using FileStorage.gRPC;
using FileStorage.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Amazon;
using Amazon.S3;
using Amazon.Runtime;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

// Database
var host = builder.Configuration.GetConnectionString("PgHost") ?? throw new InvalidOperationException("Postgres host is not configured");
var port = builder.Configuration.GetConnectionString("PgPort") ?? throw new InvalidOperationException("Postgres port is not configured");
var username = builder.Configuration.GetConnectionString("PgUsername") ?? throw new InvalidOperationException("Postgres username is not configured");
var password = builder.Configuration.GetConnectionString("PgPassword") ?? throw new InvalidOperationException("Postgres password is not configured");
var connectionString = $"Host={host};Port={port};Database=ebook_filestorage;Username={username};Password={password}";
builder.Services.AddDbContext<FileStorageDbContext>(options =>
    options.UseNpgsql(connectionString));

// S3 Configuration
var endpoint = builder.Configuration["S3:Endpoint"]
    ?? throw new InvalidOperationException("S3 Endpoint is not configured");
var accessKey = builder.Configuration["S3:AccessKey"]
    ?? throw new InvalidOperationException("S3 Access Key is not configured");
var secretKey = builder.Configuration["S3:SecretKey"]
    ?? throw new InvalidOperationException("S3 Secret Key is not configured");

var s3Config = new AmazonS3Config
{
    ServiceURL = endpoint,
    ForcePathStyle = true,
    UseHttp = true,
    SignatureVersion = "4",
    AuthenticationRegion = "apac"
};
var credentials = new BasicAWSCredentials(accessKey, secretKey);
var s3Client = new AmazonS3Client(credentials, s3Config);
builder.Services.AddSingleton<IAmazonS3>(s3Client);
builder.Services.AddOptions<S3Options>()
                .Bind(builder.Configuration.GetSection("S3") ?? throw new InvalidOperationException("S3 configuration section is not found"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

// MassTransit with RabbitMQ
// RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PageUpdatedConsumer>();
    x.AddConsumer<PageDeletedConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

// Dependency Injection
builder.Services.AddScoped<IFileMetadataRepository, FileMetadataRepository>();
builder.Services.AddScoped<IS3Service, S3Service>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<FileGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.MapHealthChecks("/health");

app.Run();
