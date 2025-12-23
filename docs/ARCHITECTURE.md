# Architecture Documentation

## Overview

eBook is built using a microservices architecture pattern, where each service is independently deployable and scalable. This document provides detailed information about the architectural decisions, patterns, and design principles used in the system.

## Architecture Principles

### 1. Domain-Driven Design (DDD)
Each microservice is organized around a specific business domain:
- **Identity**: User management and authentication
- **Document**: Document and page management
- **FileStorage**: File storage and retrieval

### 2. Clean Architecture
Each microservice follows Clean Architecture with clear separation of concerns:

```
┌─────────────────────────────────────────────────────┐
│                  Presentation Layer                  │
│              (Controllers, Middleware)               │
└──────────────────────┬──────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────┐
│                 Application Layer                    │
│         (Services, DTOs, Interfaces)                 │
└──────────────────────┬──────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────┐
│                   Domain Layer                       │
│         (Entities, Enums, Interfaces)                │
└──────────────────────┬──────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────┐
│               Infrastructure Layer                   │
│    (Repositories, DbContext, External Services)      │
└──────────────────────────────────────────────────────┘
```

**Layer Responsibilities**:

- **Presentation Layer**: Handles HTTP requests/responses, authentication, authorization
- **Application Layer**: Contains business logic, orchestrates domain objects, handles DTOs
- **Domain Layer**: Core business entities and rules, technology-agnostic
- **Infrastructure Layer**: Database access, external services, messaging

### 3. Separation of Concerns
- Each layer has a single responsibility
- Dependencies point inward (Dependency Inversion)
- Domain layer has no external dependencies

## Microservices Design

### Service Boundaries

Services are designed around business capabilities:

1. **Identity Service**: Owns user data and authentication logic
2. **Document Service**: Owns document, page, category, and bookmark data
3. **FileStorage Service**: Owns file metadata and handles file storage operations

### Data Management

Each service has its own database following the **Database per Service** pattern:
- Ensures loose coupling
- Allows independent scaling
- Enables different database technologies if needed
- Maintains data consistency through eventual consistency

**Databases**:
- `ebook_identity`: User data
- `ebook_documents`: Documents, pages, categories, bookmarks
- `ebook_filestorage`: File metadata

### Communication Patterns

#### 1. Synchronous Communication

**REST APIs** (HTTP/JSON):
- Client → API Gateway → Services
- Used for request-response operations
- Provides CRUD operations

**gRPC** (Protocol Buffers):
- Document Service → FileStorage Service
- Used for high-performance file streaming
- Binary protocol for efficiency
- Bi-directional streaming for large files

```protobuf
service FileStorageService {
  rpc UploadFile (stream UploadFileRequest) returns (FileUploadResponse);
  rpc DownloadFile (FileRequest) returns (stream FileDownloadResponse);
  rpc GetFileMetadata (FileRequest) returns (FileMetadataResponse);
  rpc GetPresignedUrl (FileRequest) returns (UrlResponse);
  rpc DeleteFile (FileRequest) returns (DeleteResponse);
}
```

#### 2. Asynchronous Communication

**Event-Driven with RabbitMQ**:
- Used for eventual consistency
- Decouples services
- Allows retry and error handling
- Publishes domain events

**Events**:
1. `UserStatusChangedEvent`: Identity → Document
   - Published when user is activated/deactivated
   - Document service updates document ownership status

2. `PageUpdatedEvent`: Document → FileStorage
   - Published when page file is updated
   - FileStorage service deletes old file from S3

3. `PageDeletedEvent`: Document → FileStorage
   - Published when page is deleted
   - FileStorage service deletes associated file

**Message Flow Example**:
```
User Status Change Flow:
Identity Service → RabbitMQ → Document Service
1. Admin changes user status to inactive
2. Identity publishes UserStatusChangedEvent
3. Document service consumes event
4. Document service updates all documents owned by that user

Page Deletion Flow:
Document Service → RabbitMQ → FileStorage Service
1. User deletes a page
2. Document service publishes PageDeletedEvent
3. FileStorage service consumes event
4. FileStorage service deletes file from S3 and metadata from DB
```

#### 3. Real-time Communication

**SignalR**:
- Document Service → Web Clients
- WebSocket-based real-time updates
- Notifies clients of document changes
- Supports collaborative editing scenarios

**Hub**: `/documentHub`
```csharp
public class DocumentHub : Hub
{
    // Clients can receive real-time notifications
    // When documents are created, updated, or deleted
}
```

## API Gateway Pattern

### YARP (Yet Another Reverse Proxy)

The API Gateway uses Microsoft YARP for reverse proxy capabilities:

**Benefits**:
- Single entry point for clients
- Request routing to appropriate services
- Load balancing capabilities
- SSL termination
- Cross-cutting concerns (auth, rate limiting)

**Configuration**:
```json
{
  "ReverseProxy": {
    "Routes": {
      "identity-route": {
        "ClusterId": "identity",
        "Match": {
          "Path": "/identity/{**catch-all}"
        }
      },
      "document-route": {
        "ClusterId": "document",
        "Match": {
          "Path": "/document/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "identity": {
        "Destinations": {
          "d1": {
            "Address": "http://ebook-identity:5001"
          }
        }
      },
      "document": {
        "Destinations": {
          "d1": {
            "Address": "http://ebook-document:5002"
          }
        }
      }
    }
  }
}
```

### Rate Limiting

Implements fixed window rate limiting:
- 5 requests per 10-second window
- Queue limit of 2
- Returns 429 Too Many Requests when exceeded

## Design Patterns

### 1. Repository Pattern

Abstracts data access logic:

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
```

**Benefits**:
- Testability (easy to mock)
- Separation of concerns
- Centralized data access logic

### 2. CQRS (Command Query Responsibility Segregation)

Separates read and write operations in services:
- Commands: Create, Update, Delete operations
- Queries: Get, List operations

**Example**:
```csharp
// Query
Task<DocumentResponse> GetByIdAsync(Guid id);

// Command
Task<DocumentResponse> CreateAsync(DocumentCreateRequest request);
```

### 3. Unit of Work Pattern

Managed by Entity Framework Core's DbContext:
- Tracks changes
- Coordinates writes to database
- Ensures consistency

### 4. Dependency Injection

Used throughout for loose coupling:
```csharp
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
```

### 5. Middleware Pattern

Custom middleware for cross-cutting concerns:

**GlobalExceptionHandler**:
```csharp
public class GlobalExceptionHandler
{
    // Catches all exceptions
    // Returns consistent error responses
    // Logs errors
}
```

## Data Models

### Identity Service

**User Entity**:
```csharp
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string Password { get; set; } // BCrypt hashed
    public Role Role { get; set; } // Admin, User
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Document Service

**Document Entity**:
```csharp
public class Document
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; }
    public string Topic { get; set; }
    public string? Description { get; set; }
    public int TotalPages { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Page> Pages { get; set; }
    public ICollection<DocumentCategory> Categories { get; set; }
}
```

**Page Entity**:
```csharp
public class Page
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int PageNumber { get; set; }
    public Guid? FileId { get; set; } // Reference to FileStorage
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public Document Document { get; set; }
    public ICollection<Bookmark> Bookmarks { get; set; }
}
```

**Category Entity**:
```csharp
public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public ICollection<DocumentCategory> Documents { get; set; }
}
```

**Bookmark Entity**:
```csharp
public class Bookmark
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PageId { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Page Page { get; set; }
}
```

### FileStorage Service

**FileMetadata Entity**:
```csharp
public class FileMetadata
{
    public Guid Id { get; set; }
    public FileType FileType { get; set; } // PDF, TEXT, EXCEL
    public string FileName { get; set; }
    public string FilePath { get; set; } // S3 path
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

## Security Architecture

### Authentication Flow

```
1. User sends credentials to Identity Service via API Gateway
2. Identity Service validates credentials
3. Identity Service generates JWT access token (short-lived, 60 min)
4. Identity Service generates refresh token (long-lived, 7 days)
5. Access token returned in response body
6. Refresh token stored in Redis and set as HTTP-only cookie
7. Client includes access token in Authorization header for subsequent requests
8. API Gateway validates JWT on each request
9. Request forwarded to target service if valid
10. When access token expires, client uses refresh token to get new access token
```

### JWT Token Structure

**Access Token Claims**:
```json
{
  "sub": "user-id",
  "username": "john_doe",
  "email": "john@example.com",
  "role": "User",
  "exp": 1234567890,
  "iss": "eBook-Identity",
  "aud": "eBook-API"
}
```

### Authorization

**Role-Based Access Control (RBAC)**:
- `User`: Standard user permissions (create/edit own documents)
- `Admin`: Full system access (manage all users and documents)

**Implementation**:
```csharp
[Authorize] // Requires authentication
[Authorize(Roles = "Admin")] // Requires Admin role
```

### Password Security

- Passwords hashed using BCrypt with salt
- Password requirements enforced at application level
- Never stored in plain text
- Never returned in API responses

## Scalability Considerations

### Horizontal Scaling

Services are stateless and can be scaled horizontally:
```bash
docker-compose up -d --scale document=3
```

### Database Scaling

- Read replicas for read-heavy operations
- Connection pooling configured in EF Core
- Indexes on frequently queried fields

### Caching

- Redis used for refresh token storage
- Can be extended for caching frequent queries
- Distributed cache for multi-instance scenarios

### Message Queue

- RabbitMQ handles asynchronous processing
- Retry policies configured (3 retries, 10-second intervals)
- Dead letter queues for failed messages
- Queue persistence for reliability

### File Storage

- S3-compatible storage naturally scales
- Presigned URLs reduce server load
- Files served directly from storage

## Resilience Patterns

### Retry Pattern

Configured in RabbitMQ consumers:
```csharp
cfg.UseMessageRetry(r => 
    r.Interval(3, TimeSpan.FromSeconds(10)));
```

### Circuit Breaker

Can be implemented with Polly (future enhancement)

### Health Checks

Each service exposes `/health` endpoint:
```csharp
app.MapHealthChecks("/health");
```

### Graceful Degradation

Services can operate independently:
- Document service continues if FileStorage is down (file ops fail gracefully)
- Identity remains operational if RabbitMQ is down (events queued)

## Monitoring and Observability

### Logging

- Structured logging with built-in .NET logging
- Log levels: Debug, Information, Warning, Error, Critical
- Can integrate with ELK stack (Elasticsearch, Logstash, Kibana)

### Metrics

- Health check endpoints for liveness/readiness probes
- Can integrate with Prometheus and Grafana

### Distributed Tracing

- Can implement with OpenTelemetry
- Trace requests across service boundaries

## Future Enhancements

### Potential Improvements

1. **API Versioning**: Version APIs for backward compatibility
2. **Circuit Breaker**: Add Polly for resilience
3. **Distributed Tracing**: Implement OpenTelemetry
4. **API Documentation**: Centralized API docs with Swagger aggregation
5. **Service Mesh**: Consider Istio or Linkerd for advanced networking
6. **CQRS with Event Sourcing**: Full event sourcing implementation
7. **GraphQL Gateway**: Alternative to REST for flexible queries
8. **Caching Strategy**: Implement distributed caching with Redis
9. **Rate Limiting per User**: More granular rate limiting
10. **File Virus Scanning**: Scan uploaded files for malware

## Technology Decisions

### Why .NET 9?
- Modern, high-performance framework
- Cross-platform support
- Strong ecosystem for microservices
- Built-in dependency injection
- Excellent tooling

### Why PostgreSQL?
- ACID compliance
- Rich data types
- JSON support
- Proven reliability
- Open source

### Why RabbitMQ?
- Reliable message delivery
- Rich routing capabilities
- Easy to set up and manage
- Strong .NET support via MassTransit

### Why gRPC?
- High performance binary protocol
- Strong typing with Protocol Buffers
- Bi-directional streaming for large files
- Better than REST for service-to-service

### Why SignalR?
- Real-time communication
- Automatic fallback (WebSocket → SSE → Long polling)
- .NET native solution
- Easy to integrate

## Conclusion

The eBook system architecture is designed for:
- **Scalability**: Services can scale independently
- **Maintainability**: Clean separation of concerns
- **Reliability**: Event-driven architecture with retry logic
- **Security**: JWT authentication, role-based authorization
- **Performance**: gRPC for file operations, efficient data access

The architecture supports future growth and can adapt to changing requirements while maintaining system stability.
