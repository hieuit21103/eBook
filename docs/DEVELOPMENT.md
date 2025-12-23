# Development Guide

This guide provides information for developers working on the eBook project.

## Table of Contents

- [Development Environment Setup](#development-environment-setup)
- [Project Structure](#project-structure)
- [Coding Standards](#coding-standards)
- [Development Workflow](#development-workflow)
- [Testing](#testing)
- [Debugging](#debugging)
- [Database Migrations](#database-migrations)
- [Adding New Features](#adding-new-features)
- [Common Tasks](#common-tasks)
- [Troubleshooting](#troubleshooting)

## Development Environment Setup

### Required Tools

1. **IDE**:
   - Visual Studio 2022 (recommended)
   - Visual Studio Code with C# extension
   - JetBrains Rider

2. **SDK and Runtime**:
   - [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

3. **Database Tools**:
   - PostgreSQL client (psql or pgAdmin)
   - Redis CLI or RedisInsight
   - RabbitMQ Management UI

4. **API Testing**:
   - Postman or Insomnia
   - cURL
   - HTTPie

5. **Version Control**:
   - Git

### Local Development Setup

#### 1. Clone Repository

```bash
git clone https://github.com/hieuit21103/eBook.git
cd eBook
```

#### 2. Install .NET SDK

```bash
# Verify installation
dotnet --version
# Should output: 9.0.x
```

#### 3. Start Infrastructure Services

**Option A: Using Docker Compose** (Recommended)
```bash
# Start only infrastructure services
docker-compose up -d rabbitmq

# Or manually start individual services
docker run -d --name postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 postgres:16

docker run -d --name redis \
  -p 6379:6379 redis:7

docker run -d --name rabbitmq \
  -p 5672:5672 -p 15672:15672 \
  rabbitmq:4-management
```

**Option B: Local Installation**
- Install PostgreSQL, Redis, and RabbitMQ locally
- Configure to start on system boot

#### 4. Configure Environment

Create `appsettings.Development.json` for each service:

**Identity Service** (`src/Identity/appsettings.Development.json`):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "PgHost": "localhost",
    "PgPort": "5432",
    "PgUsername": "postgres",
    "PgPassword": "postgres"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Jwt": {
    "Issuer": "eBook-Identity",
    "Audience": "eBook-API",
    "SecretKey": "YourDevelopmentSecretKeyMinimum32Characters",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  }
}
```

#### 5. Setup Databases

```bash
# Create databases
psql -U postgres -h localhost -c "CREATE DATABASE ebook_identity;"
psql -U postgres -h localhost -c "CREATE DATABASE ebook_documents;"
psql -U postgres -h localhost -c "CREATE DATABASE ebook_filestorage;"
```

#### 6. Run Migrations

```bash
# Identity Service
cd src/Identity
dotnet ef database update

# Document Service
cd ../Document
dotnet ef database update

# FileStorage Service
cd ../FileStorage
dotnet ef database update
```

#### 7. Restore Dependencies

```bash
# From repository root
dotnet restore eBook.sln
```

#### 8. Build Solution

```bash
dotnet build eBook.sln
```

#### 9. Run Services

Open 4 separate terminal windows:

```bash
# Terminal 1 - Identity Service
cd src/Identity
dotnet run

# Terminal 2 - Document Service
cd src/Document
dotnet run

# Terminal 3 - FileStorage Service
cd src/FileStorage
dotnet run

# Terminal 4 - API Gateway
cd src/ApiGateway
dotnet run
```

Or use `dotnet watch` for hot reload:
```bash
dotnet watch run
```

#### 10. Verify Setup

```bash
# Check health endpoints
curl http://localhost:5001/health  # Identity
curl http://localhost:5002/health  # Document
curl http://localhost:5003/health  # FileStorage
curl http://localhost:5000/health  # API Gateway
```

Access Swagger UI:
- Identity: http://localhost:5001/swagger
- Document: http://localhost:5002/swagger

## Project Structure

### Solution Structure

```
eBook.sln                      # Main solution file
├── src/
│   ├── ApiGateway/           # API Gateway project
│   ├── Identity/             # Identity microservice
│   ├── Document/             # Document microservice
│   ├── FileStorage/          # FileStorage microservice
│   └── Shared/               # Shared library
└── docs/                     # Documentation
```

### Service Structure (Clean Architecture)

Each microservice follows this structure:

```
Service/
├── Program.cs                # Entry point and service configuration
├── Service.csproj           # Project file
├── appsettings.json         # Configuration
├── Application/             # Application Layer
│   ├── Services/           # Business logic implementations
│   ├── Interfaces/         # Service interfaces
│   └── DTOs/              # Data Transfer Objects
├── Controllers/            # API Controllers (Presentation Layer)
├── Domain/                # Domain Layer
│   ├── Entities/         # Domain entities
│   ├── Enums/           # Enumerations
│   ├── Interfaces/      # Repository interfaces
│   └── Filters/        # Query filters (optional)
├── Infrastructure/        # Infrastructure Layer
│   ├── Data/           # DbContext, configurations, seeders
│   ├── Repositories/  # Repository implementations
│   └── Services/     # External service implementations
├── Middleware/          # Custom middleware
├── Migrations/         # EF Core migrations
└── Properties/        # Launch settings
```

## Coding Standards

### C# Conventions

Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions):

**Naming**:
```csharp
// PascalCase for classes, methods, properties, public fields
public class DocumentService { }
public async Task<Document> GetDocumentAsync(Guid id) { }

// camelCase for local variables, parameters, private fields
private readonly IRepository repository;
public void ProcessData(string userName) { }

// Interfaces start with 'I'
public interface IDocumentService { }

// Async methods end with 'Async'
public async Task<User> GetUserAsync(Guid id) { }
```

**Organization**:
```csharp
// 1. Using statements
using System;
using Microsoft.AspNetCore.Mvc;

// 2. Namespace
namespace Application.Services;

// 3. Class definition
public class DocumentService : IDocumentService
{
    // 4. Private fields
    private readonly IDocumentRepository _repository;
    private readonly ILogger<DocumentService> _logger;

    // 5. Constructor
    public DocumentService(
        IDocumentRepository repository,
        ILogger<DocumentService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // 6. Public methods
    public async Task<DocumentResponse> GetByIdAsync(Guid id)
    {
        // Implementation
    }

    // 7. Private methods
    private void ValidateDocument(Document document)
    {
        // Implementation
    }
}
```

### API Design

**Controller Structure**:
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _service;

    public DocumentController(IDocumentService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get document by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentResponse>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(new ApiResponse<DocumentResponse>
        {
            Success = true,
            Data = result
        });
    }
}
```

**DTOs**:
```csharp
// Request DTO
public class DocumentCreateRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Topic { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public List<Guid> CategoryIds { get; set; } = new();
}

// Response DTO
public class DocumentResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalPages { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Error Handling

**Use Custom Exceptions**:
```csharp
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
```

**Global Exception Handler**:
```csharp
public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status404NotFound);
        }
        catch (ValidationException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex, StatusCodes.Status500InternalServerError);
        }
    }

    private static Task HandleExceptionAsync(
        HttpContext context, 
        Exception exception, 
        int statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new ErrorResponse
        {
            Success = false,
            Message = exception.Message,
            StatusCode = statusCode
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
```

### Dependency Injection

**Register Services**:
```csharp
// Program.cs
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<IPageRepository, PageRepository>();
```

**Constructor Injection**:
```csharp
public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;
    private readonly IPageRepository _pageRepository;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IDocumentRepository repository,
        IPageRepository pageRepository,
        ILogger<DocumentService> logger)
    {
        _repository = repository;
        _pageRepository = pageRepository;
        _logger = logger;
    }
}
```

## Development Workflow

### Git Workflow

1. **Create Feature Branch**:
```bash
git checkout -b feature/add-document-tags
```

2. **Make Changes**:
```bash
# Edit files
# Build and test locally
dotnet build
dotnet test
```

3. **Commit Changes**:
```bash
git add .
git commit -m "Add document tagging feature"
```

4. **Push to Remote**:
```bash
git push origin feature/add-document-tags
```

5. **Create Pull Request**:
- Open PR on GitHub
- Request review
- Address feedback
- Merge to main

### Branch Naming

- `feature/` - New features
- `bugfix/` - Bug fixes
- `hotfix/` - Urgent production fixes
- `refactor/` - Code refactoring
- `docs/` - Documentation updates

### Commit Messages

Follow conventional commits:

```
type(scope): subject

body

footer
```

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `style`: Code style changes
- `refactor`: Code refactoring
- `test`: Adding tests
- `chore`: Maintenance tasks

**Examples**:
```
feat(document): add tagging functionality

Add ability to tag documents with custom tags.
Tags can be added, removed, and searched.

Closes #123
```

```
fix(auth): resolve token refresh issue

Fix bug where refresh token was not properly
rotated on refresh endpoint.

Fixes #456
```

## Testing

### Unit Testing

**Setup**:
```bash
# Add test project
dotnet new xunit -n Identity.Tests
cd Identity.Tests
dotnet add reference ../src/Identity/Identity.csproj
dotnet add package Moq
dotnet add package FluentAssertions
```

**Example Test**:
```csharp
public class DocumentServiceTests
{
    private readonly Mock<IDocumentRepository> _mockRepository;
    private readonly Mock<ILogger<DocumentService>> _mockLogger;
    private readonly DocumentService _service;

    public DocumentServiceTests()
    {
        _mockRepository = new Mock<IDocumentRepository>();
        _mockLogger = new Mock<ILogger<DocumentService>>();
        _service = new DocumentService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document
        {
            Id = documentId,
            Title = "Test Document"
        };
        _mockRepository
            .Setup(r => r.GetByIdAsync(documentId))
            .ReturnsAsync(document);

        // Act
        var result = await _service.GetByIdAsync(documentId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(documentId);
        result.Title.Should().Be("Test Document");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.GetByIdAsync(documentId))
            .ReturnsAsync((Document?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.GetByIdAsync(documentId));
    }
}
```

**Run Tests**:
```bash
dotnet test
dotnet test --logger "console;verbosity=detailed"
dotnet test --filter "Category=Unit"
```

### Integration Testing

**Example**:
```csharp
public class DocumentControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DocumentControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDocuments_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/document");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
    }
}
```

## Debugging

### Visual Studio

1. Set breakpoints in code
2. Press F5 or click "Start Debugging"
3. Step through code with F10 (over) and F11 (into)

### Visual Studio Code

1. Install C# extension
2. Open `.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (Identity)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/Identity/bin/Debug/net9.0/Identity.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/Identity",
      "stopAtEntry": false,
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ]
}
```

3. Set breakpoints and press F5

### Debugging Tips

**Enable detailed logging**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Debug"
    }
  }
}
```

**Debug SQL queries**:
```csharp
// Add to DbContext configuration
optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information)
              .EnableSensitiveDataLogging()
              .EnableDetailedErrors();
```

**Attach to running process**:
```bash
# Get process ID
dotnet list process

# Visual Studio: Debug > Attach to Process
# Select dotnet.exe with appropriate PID
```

## Database Migrations

### Create Migration

```bash
cd src/Identity
dotnet ef migrations add AddEmailVerificationField
```

### Update Database

```bash
dotnet ef database update
```

### Rollback Migration

```bash
# Rollback to previous migration
dotnet ef database update PreviousMigrationName

# Remove last migration (if not applied)
dotnet ef migrations remove
```

### Generate SQL Script

```bash
# Generate script for all migrations
dotnet ef migrations script

# Generate script for specific migration range
dotnet ef migrations script InitialCreate AddEmailVerificationField
```

### Migration Best Practices

1. **Always backup** database before applying migrations in production
2. **Test migrations** in development and staging first
3. **Use descriptive names** for migrations
4. **Don't modify** existing migrations that have been applied
5. **Create separate migrations** for different changes

## Adding New Features

### Adding a New Entity

1. **Create Entity** in `Domain/Entities/`:
```csharp
public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

2. **Configure Entity** in `Infrastructure/Data/Configurations/`:
```csharp
public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.Name).IsUnique();
    }
}
```

3. **Add DbSet** to DbContext:
```csharp
public DbSet<Tag> Tags { get; set; }
```

4. **Create Migration**:
```bash
dotnet ef migrations add AddTagEntity
dotnet ef database update
```

5. **Create Repository Interface** in `Domain/Interfaces/`:
```csharp
public interface ITagRepository : IRepository<Tag>
{
    Task<Tag?> GetByNameAsync(string name);
}
```

6. **Implement Repository** in `Infrastructure/Repositories/`:
```csharp
public class TagRepository : Repository<Tag>, ITagRepository
{
    public TagRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Tag?> GetByNameAsync(string name)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Name == name);
    }
}
```

7. **Create DTOs** in `Application/DTOs/`:
```csharp
public class TagCreateRequest
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}

public class TagResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

8. **Create Service Interface** in `Application/Interfaces/`:
```csharp
public interface ITagService
{
    Task<TagResponse> CreateAsync(TagCreateRequest request);
    Task<IEnumerable<TagResponse>> GetAllAsync();
}
```

9. **Implement Service** in `Application/Services/`:
```csharp
public class TagService : ITagService
{
    private readonly ITagRepository _repository;

    public TagService(ITagRepository repository)
    {
        _repository = repository;
    }

    public async Task<TagResponse> CreateAsync(TagCreateRequest request)
    {
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(tag);

        return new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            CreatedAt = tag.CreatedAt
        };
    }

    public async Task<IEnumerable<TagResponse>> GetAllAsync()
    {
        var tags = await _repository.GetAllAsync();
        return tags.Select(t => new TagResponse
        {
            Id = t.Id,
            Name = t.Name,
            CreatedAt = t.CreatedAt
        });
    }
}
```

10. **Create Controller** in `Controllers/`:
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TagController : ControllerBase
{
    private readonly ITagService _service;

    public TagController(ITagService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<TagResponse>> Create(TagCreateRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagResponse>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }
}
```

11. **Register Services** in `Program.cs`:
```csharp
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ITagService, TagService>();
```

12. **Test**:
```bash
dotnet build
dotnet run
# Test endpoints with Postman/cURL
```

## Common Tasks

### Adding a New NuGet Package

```bash
cd src/Identity
dotnet add package PackageName
```

### Updating Packages

```bash
# List outdated packages
dotnet list package --outdated

# Update specific package
dotnet add package PackageName

# Update all packages
dotnet restore
```

### Adding Configuration

1. Add to `appsettings.json`:
```json
{
  "MyFeature": {
    "Setting1": "value1",
    "Setting2": 42
  }
}
```

2. Create configuration class:
```csharp
public class MyFeatureOptions
{
    public string Setting1 { get; set; } = string.Empty;
    public int Setting2 { get; set; }
}
```

3. Register in `Program.cs`:
```csharp
builder.Services.Configure<MyFeatureOptions>(
    builder.Configuration.GetSection("MyFeature"));
```

4. Inject in service:
```csharp
public class MyService
{
    private readonly MyFeatureOptions _options;

    public MyService(IOptions<MyFeatureOptions> options)
    {
        _options = options.Value;
    }
}
```

### Adding Middleware

1. Create middleware:
```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
        await _next(context);
    }
}
```

2. Register in `Program.cs`:
```csharp
app.UseMiddleware<RequestLoggingMiddleware>();
```

## Troubleshooting

### Build Errors

**Clear build artifacts**:
```bash
dotnet clean
rm -rf bin obj
dotnet restore
dotnet build
```

### Port Already in Use

```bash
# Find process using port
lsof -i :5001  # macOS/Linux
netstat -ano | findstr :5001  # Windows

# Kill process
kill -9 <PID>  # macOS/Linux
taskkill /PID <PID> /F  # Windows
```

### Database Connection Issues

```bash
# Test connection
psql -h localhost -U postgres -d ebook_identity

# Reset database
dotnet ef database drop --force
dotnet ef database update
```

### Migration Issues

```bash
# Remove all migrations
rm -rf Migrations/
dotnet ef database drop --force

# Recreate from scratch
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Resources

- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [C# Programming Guide](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## Getting Help

1. Check documentation in `/docs` folder
2. Search existing GitHub issues
3. Ask in team Slack/Discord channel
4. Create new GitHub issue

---

Happy coding! 🚀
