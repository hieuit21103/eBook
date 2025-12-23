# eBook - Digital Document Management System

A modern, microservices-based digital document management system built with .NET 9, featuring document storage, user management, file handling, and real-time collaboration capabilities.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Microservices](#microservices)
- [API Documentation](#api-documentation)
- [Configuration](#configuration)
- [Development](#development)
- [Deployment](#deployment)
- [Communication Patterns](#communication-patterns)
- [Security](#security)
- [Contributing](#contributing)

## Overview

eBook is a distributed digital document management platform that allows users to create, manage, and organize documents with multiple pages. The system is designed using microservices architecture, providing scalability, maintainability, and independent service deployment.

### Key Capabilities

- **User Management**: Registration, authentication, and authorization with JWT tokens
- **Document Management**: Create, update, delete, and organize documents with categories
- **Page Management**: Handle multiple pages within documents with file storage
- **File Storage**: Secure file storage using S3-compatible object storage with gRPC streaming
- **Real-time Updates**: SignalR integration for live document updates
- **API Gateway**: Centralized entry point with rate limiting and reverse proxy
- **Event-Driven Architecture**: Asynchronous communication using RabbitMQ

## Architecture

The system follows a **microservices architecture** with the following services:

```
┌─────────────────────────────────────────────────────────────────┐
│                         API Gateway (Port 5000)                  │
│                    (YARP Reverse Proxy + Rate Limiting)          │
└─────────────────────────────────────────────────────────────────┘
                    │                           │
        ┌───────────┴───────────┐      ┌───────┴────────────┐
        │                       │      │                     │
┌───────▼─────────┐    ┌───────▼─────────┐    ┌───────────▼────────┐
│   Identity      │    │    Document      │    │   FileStorage      │
│  Service        │    │    Service       │    │   Service          │
│  (Port 5001)    │    │   (Port 5002)    │    │   (Port 5003)      │
│                 │    │                  │    │                    │
│ - Auth          │    │ - Documents      │    │ - File Upload/     │
│ - User Mgmt     │    │ - Pages          │    │   Download         │
│ - JWT Tokens    │    │ - Categories     │    │ - S3 Storage       │
│                 │    │ - Bookmarks      │    │ - Presigned URLs   │
│                 │    │ - SignalR Hub    │    │ - gRPC Service     │
└─────────┬───────┘    └──────────┬───────┘    └─────────┬──────────┘
          │                       │                       │
          │            ┌──────────┴───────────────────────┘
          │            │
┌─────────▼────────────▼─────────────────────────────────────────┐
│                      RabbitMQ Message Bus                       │
│                 (Event-Driven Communication)                    │
└─────────────────────────────────────────────────────────────────┘
          │                       │                       │
┌─────────▼───────────┐  ┌───────▼────────┐  ┌──────────▼────────┐
│  PostgreSQL         │  │  Redis Cache   │  │  S3-Compatible    │
│  (Identity DB)      │  │                │  │  Object Storage   │
│  (Document DB)      │  │                │  │                   │
│  (FileStorage DB)   │  │                │  │                   │
└─────────────────────┘  └────────────────┘  └───────────────────┘
```

### Architectural Patterns

- **Clean Architecture**: Each service follows Clean Architecture principles with Domain, Application, Infrastructure layers
- **CQRS Pattern**: Separation of read and write operations in service layer
- **Repository Pattern**: Data access abstraction through repositories
- **Domain-Driven Design**: Rich domain models with business logic
- **Event-Driven Architecture**: Asynchronous communication via RabbitMQ events
- **API Gateway Pattern**: Centralized entry point using YARP reverse proxy

## Features

### Identity Service
- User registration and authentication
- JWT access token and refresh token generation
- Password hashing with BCrypt
- Role-based authorization (Admin/User)
- User status management (Active/Inactive)
- Redis-based refresh token storage
- User status change event publishing

### Document Service
- Document CRUD operations with soft delete
- Multi-page document support
- Document categorization with many-to-many relationships
- Bookmark management for pages
- Filtering and pagination
- Real-time updates via SignalR
- Document ownership and access control
- Integration with FileStorage via gRPC

### FileStorage Service
- File upload/download via gRPC streaming
- S3-compatible object storage integration
- Presigned URL generation for secure file access
- File metadata management (PDF, TEXT, EXCEL types)
- Event-driven file cleanup (on page update/delete)
- Chunked file transfer for large files

### API Gateway
- Reverse proxy to microservices using YARP
- JWT authentication middleware
- Rate limiting (5 requests per 10 seconds)
- Health check aggregation
- Swagger UI integration for all services

## Technology Stack

### Backend
- **.NET 9.0**: Core framework
- **ASP.NET Core**: Web API framework
- **Entity Framework Core 9**: ORM with PostgreSQL provider
- **MassTransit**: Message bus abstraction for RabbitMQ
- **Grpc.AspNetCore**: gRPC framework
- **SignalR**: Real-time web functionality
- **YARP (Yet Another Reverse Proxy)**: API Gateway

### Database & Storage
- **PostgreSQL**: Relational database for all services
- **Redis**: Distributed caching and refresh token storage
- **S3-Compatible Storage**: Object storage for files (MinIO/AWS S3)

### Messaging & Communication
- **RabbitMQ**: Message broker for event-driven communication
- **gRPC**: High-performance RPC for Document-FileStorage communication
- **REST APIs**: HTTP-based APIs for client communication
- **SignalR**: WebSocket-based real-time updates

### Security
- **JWT (JSON Web Tokens)**: Authentication and authorization
- **BCrypt**: Password hashing
- **HTTPS**: Secure communication

### DevOps
- **Docker**: Containerization
- **Docker Compose**: Multi-container orchestration

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started) and [Docker Compose](https://docs.docker.com/compose/)
- [PostgreSQL](https://www.postgresql.org/download/) (if running locally without Docker)
- [Redis](https://redis.io/download) (if running locally without Docker)
- [RabbitMQ](https://www.rabbitmq.com/download.html) (if running locally without Docker)
- S3-compatible storage (AWS S3 or [MinIO](https://min.io/))

## Getting Started

### Quick Start with Docker Compose

1. **Clone the repository**
```bash
git clone https://github.com/hieuit21103/eBook.git
cd eBook
```

2. **Configure environment variables**
```bash
cp .env.example .env
# Edit .env with your configuration
```

3. **Start all services**
```bash
docker-compose up -d
```

4. **Access the services**
- API Gateway: http://localhost:5000
- Identity Service Swagger: http://localhost:5000/identity/swagger
- Document Service: http://localhost:5002 (via gateway)
- FileStorage Service: http://localhost:5003 (via gateway)
- RabbitMQ Management: http://localhost:15672 (guest/guest)

### Manual Setup (Development)

1. **Install dependencies**
```bash
dotnet restore eBook.sln
```

2. **Start infrastructure services**
```bash
# Start PostgreSQL, Redis, and RabbitMQ
docker-compose up -d rabbitmq
# Or use local installations
```

3. **Configure appsettings**
Update `appsettings.Development.json` in each service with your database and service URLs.

4. **Run database migrations**
```bash
cd src/Identity
dotnet ef database update

cd ../Document
dotnet ef database update

cd ../FileStorage
dotnet ef database update
```

5. **Run services**
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

## Project Structure

```
eBook/
├── src/
│   ├── ApiGateway/              # API Gateway service
│   │   ├── Program.cs           # Gateway configuration
│   │   └── appsettings.json     # Gateway settings (YARP config)
│   │
│   ├── Identity/                # Identity microservice
│   │   ├── Application/         # Application layer
│   │   │   ├── DTOs/           # Data transfer objects
│   │   │   ├── Interfaces/     # Service interfaces
│   │   │   └── Services/       # Business logic
│   │   ├── Controllers/         # API controllers
│   │   ├── Domain/             # Domain layer
│   │   │   ├── Entities/       # Domain entities (User)
│   │   │   ├── Enums/          # Enumerations (Role)
│   │   │   └── Interfaces/     # Repository interfaces
│   │   ├── Infrastructure/      # Infrastructure layer
│   │   │   ├── Data/           # DbContext, configurations, seeders
│   │   │   ├── Repositories/   # Repository implementations
│   │   │   └── Services/       # External services (Token, Password)
│   │   └── Middleware/         # Custom middleware (Exception handling)
│   │
│   ├── Document/               # Document microservice
│   │   ├── Application/        # Application layer
│   │   │   ├── DTOs/          # DTOs for Document, Page, Category, Bookmark
│   │   │   ├── Interfaces/    # Service interfaces
│   │   │   └── Services/      # Business logic
│   │   ├── Controllers/        # API controllers
│   │   ├── Domain/            # Domain layer
│   │   │   ├── Entities/      # Entities (Document, Page, Category, etc.)
│   │   │   ├── Filters/       # Query filters
│   │   │   └── Interfaces/    # Repository interfaces
│   │   ├── Infrastructure/     # Infrastructure layer
│   │   │   ├── Data/          # DbContext and configurations
│   │   │   ├── Hubs/          # SignalR hubs
│   │   │   ├── Repositories/  # Repository implementations
│   │   │   └── Services/      # SignalR service
│   │   ├── IntegrationEvents/ # Event consumers
│   │   ├── Middleware/        # Exception handling
│   │   └── Protos/            # gRPC proto files
│   │
│   ├── FileStorage/           # FileStorage microservice
│   │   ├── Application/       # Application layer
│   │   │   ├── DTOs/         # DTOs for file operations
│   │   │   ├── Interfaces/   # Service interfaces
│   │   │   ├── Options/      # Configuration options (S3)
│   │   │   └── Services/     # File storage business logic
│   │   ├── Domain/           # Domain layer
│   │   │   ├── Entities/     # FileMetadata entity
│   │   │   ├── Enums/        # FileType enum
│   │   │   └── Interfaces/   # Repository interfaces
│   │   ├── gRPC/             # gRPC service implementation
│   │   ├── Infrastructure/    # Infrastructure layer
│   │   │   ├── Data/         # DbContext
│   │   │   ├── Repositories/ # Repository implementations
│   │   │   └── Services/     # S3 service
│   │   ├── IntegrationEvents/ # Event consumers (Page events)
│   │   └── Protos/           # gRPC proto definitions
│   │
│   └── Shared/               # Shared library
│       ├── DTOs/             # Common DTOs
│       └── *Event.cs         # Integration event definitions
│
├── docker-compose.yml        # Docker Compose configuration
├── .env.example             # Environment variables template
└── README.md               # This file
```

## Microservices

### Identity Service (Port 5001)

**Purpose**: User authentication and authorization

**Endpoints**:
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT tokens
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Logout user
- `GET /api/users` - Get all users (Admin only)
- `GET /api/users/{id}` - Get user by ID
- `PATCH /api/users/{id}/status` - Update user status (Admin only)

**Database**: `ebook_identity`
- Users table

**Events Published**:
- `UserStatusChangedEvent` - When user status changes (active/inactive)

### Document Service (Port 5002)

**Purpose**: Document and page management

**Endpoints**:

*Documents*:
- `GET /api/document` - Get all documents (with filtering)
- `GET /api/document/{id}` - Get document by ID
- `GET /api/document/{id}/details` - Get document with full details
- `POST /api/document` - Create new document
- `PUT /api/document/{id}` - Update document
- `DELETE /api/document/{id}` - Delete document (soft delete)

*Pages*:
- `GET /api/page/document/{documentId}` - Get pages by document
- `GET /api/page/{id}` - Get page by ID
- `POST /api/page` - Create new page
- `PUT /api/page/{id}` - Update page
- `DELETE /api/page/{id}` - Delete page

*Categories*:
- `GET /api/category` - Get all categories
- `GET /api/category/{id}` - Get category by ID
- `POST /api/category` - Create category
- `PUT /api/category/{id}` - Update category
- `DELETE /api/category/{id}` - Delete category

*Bookmarks*:
- `GET /api/bookmark` - Get user bookmarks
- `GET /api/bookmark/{id}` - Get bookmark by ID
- `POST /api/bookmark` - Create bookmark
- `DELETE /api/bookmark/{id}` - Delete bookmark

**Database**: `ebook_documents`
- Documents, Pages, Categories, DocumentCategories, Bookmarks tables

**SignalR Hub**: `/documentHub`
- Real-time document updates

**Events Consumed**:
- `UserStatusChangedEvent` - Updates document ownership when user status changes

**Events Published**:
- `PageUpdatedEvent` - When page is updated (for file cleanup)
- `PageDeletedEvent` - When page is deleted (for file cleanup)

### FileStorage Service (Port 5003)

**Purpose**: File storage and retrieval

**gRPC Service**: `FileStorageService`
- `UploadFile(stream)` - Upload file with streaming
- `DownloadFile(id)` - Download file with streaming
- `GetFileMetadata(id)` - Get file metadata
- `GetAllFiles()` - Get all file metadata
- `DeleteFile(id)` - Delete file
- `GetPresignedUrl(id)` - Get presigned URL for direct S3 access

**Database**: `ebook_filestorage`
- FileMetadata table

**Events Consumed**:
- `PageUpdatedEvent` - Deletes old file when page is updated
- `PageDeletedEvent` - Deletes file when page is deleted

**Storage**: S3-compatible object storage

### API Gateway (Port 5000)

**Purpose**: Single entry point for all services

**Features**:
- Reverse proxy using YARP
- JWT authentication
- Rate limiting (5 requests/10 seconds)
- Health checks
- Swagger UI aggregation

**Routes**:
- `/identity/*` → Identity Service
- `/document/*` → Document Service  
- `/file/*` → FileStorage Service (if exposed)

## API Documentation

Each service provides Swagger/OpenAPI documentation:

- **Identity API**: http://localhost:5000/identity/swagger
- **Document API**: http://localhost:5002/swagger (or via gateway)
- **FileStorage API**: gRPC service (use gRPC client tools)

### Authentication Flow

1. **Register**: `POST /api/auth/register`
```json
{
  "email": "user@example.com",
  "username": "username",
  "password": "SecurePassword123!"
}
```

2. **Login**: `POST /api/auth/login`
```json
{
  "username": "username",
  "password": "SecurePassword123!"
}
```
Returns:
```json
{
  "accessToken": "eyJhbGc...",
  "userId": "guid",
  "username": "username",
  "email": "user@example.com",
  "role": "User"
}
```
Refresh token set as HTTP-only cookie.

3. **Use Access Token**: Include in Authorization header
```
Authorization: Bearer eyJhbGc...
```

4. **Refresh Token**: `POST /api/auth/refresh`
Uses refresh token from cookie to get new access token.

## Configuration

### Environment Variables

See `.env.example` for all configuration options.

**Key Settings**:

```env
# Database
PG_HOST=localhost
PG_PORT=5432
PG_USERNAME=postgres
PG_PASSWORD=postgres

# Redis
REDIS_CONNECTION_STRING=localhost:6379

# JWT
JWT_ISSUER=YourIssuer
JWT_AUDIENCE=YourAudience
JWT_SECRET_KEY=YourSecretKeyMinimum32Characters
JWT_EXPIRY_MINUTES=60
REFRESH_TOKEN_EXPIRY_DAYS=7

# S3 Storage
S3_ENDPOINT=http://localhost:9000
S3_ACCESS_KEY=minioadmin
S3_SECRET_KEY=minioadmin
S3_BUCKET_NAME=ebook-files
S3_PRESIGN_EXPIRATION_MINUTES=15

# RabbitMQ
RABBITMQ_HOST=localhost
RABBITMQ_USERNAME=guest
RABBITMQ_PASSWORD=guest
```

## Development

### Adding a New Migration

```bash
# Identity Service
cd src/Identity
dotnet ef migrations add MigrationName
dotnet ef database update

# Document Service
cd src/Document
dotnet ef migrations add MigrationName
dotnet ef database update

# FileStorage Service
cd src/FileStorage
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Running Tests

```bash
dotnet test
```

### Code Style

The project follows standard C# coding conventions:
- PascalCase for classes, methods, properties
- camelCase for local variables and parameters
- Interfaces prefixed with 'I'
- Async methods suffixed with 'Async'

## Deployment

### Docker Deployment

1. Build images:
```bash
docker-compose build
```

2. Deploy:
```bash
docker-compose up -d
```

3. Check logs:
```bash
docker-compose logs -f [service-name]
```

4. Scale services:
```bash
docker-compose up -d --scale document=3
```

### Production Considerations

- Use production-grade databases (managed PostgreSQL)
- Set up Redis cluster for high availability
- Use managed RabbitMQ service or cluster
- Configure proper secrets management
- Set up load balancing
- Enable HTTPS with proper certificates
- Configure monitoring and logging (ELK stack, Application Insights)
- Set up health checks and auto-recovery
- Implement proper backup strategies

## Communication Patterns

### REST API
- Client ↔ API Gateway
- API Gateway ↔ Microservices

### gRPC
- Document Service ↔ FileStorage Service
- Used for high-performance file streaming

### Message Queue (RabbitMQ)
- Identity Service → Document Service (`UserStatusChangedEvent`)
- Document Service → FileStorage Service (`PageUpdatedEvent`, `PageDeletedEvent`)
- Provides loose coupling and asynchronous processing

### SignalR
- Document Service → Clients
- Real-time document updates

## Security

### Authentication & Authorization
- JWT-based authentication
- Role-based authorization (Admin, User)
- Refresh token rotation
- HTTP-only cookies for refresh tokens
- Secure password hashing with BCrypt

### API Security
- Rate limiting on API Gateway
- JWT validation on each service
- CORS configuration
- Input validation
- SQL injection prevention (parameterized queries)

### Data Security
- Encrypted connections (HTTPS)
- Sensitive data not logged
- Environment-based secrets
- Presigned URLs for secure file access

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines

- Follow Clean Architecture principles
- Write unit tests for business logic
- Use meaningful commit messages
- Update documentation for API changes
- Ensure all services can run independently
- Test integration between services

## License

This project is for educational purposes.

## Contact

For questions or support, please open an issue in the GitHub repository.

---

**Built with ❤️ using .NET 9 and Microservices Architecture**

