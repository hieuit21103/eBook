# API Documentation

This document provides comprehensive API documentation for all microservices in the eBook system.

## Table of Contents

- [Authentication](#authentication)
- [Identity Service API](#identity-service-api)
- [Document Service API](#document-service-api)
- [FileStorage Service API](#filestorage-service-api)
- [Error Handling](#error-handling)
- [Pagination](#pagination)
- [Filtering](#filtering)

## Authentication

All API endpoints (except registration and login) require JWT authentication.

### Authentication Header

Include the JWT access token in the Authorization header:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Token Expiration

- **Access Token**: Expires in 60 minutes (configurable)
- **Refresh Token**: Expires in 7 days (configurable)

### Refresh Token Flow

1. Access token expires
2. Client calls `/api/auth/refresh` with refresh token in cookie
3. Receive new access token
4. Continue making API requests

## Identity Service API

Base URL: `http://localhost:5000/identity` (via API Gateway)

### Authentication Endpoints

#### Register New User

```http
POST /api/auth/register
Content-Type: application/json
```

**Request Body**:
```json
{
  "email": "user@example.com",
  "username": "john_doe",
  "password": "SecurePassword123!"
}
```

**Response** (200 OK):
```json
{
  "accessToken": "eyJhbGc...",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "username": "john_doe",
  "email": "user@example.com",
  "role": "User"
}
```

**Cookies Set**:
- `refreshToken`: HTTP-only cookie with refresh token

**Validation Rules**:
- Email: Required, valid email format
- Username: Required, 3-50 characters
- Password: Required, minimum 6 characters

---

#### Login

```http
POST /api/auth/login
Content-Type: application/json
```

**Request Body**:
```json
{
  "username": "john_doe",
  "password": "SecurePassword123!"
}
```

**Response** (200 OK):
```json
{
  "accessToken": "eyJhbGc...",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "username": "john_doe",
  "email": "user@example.com",
  "role": "User"
}
```

**Cookies Set**:
- `refreshToken`: HTTP-only cookie with refresh token

**Error Responses**:
- 400: Invalid credentials
- 403: User account is inactive

---

#### Refresh Access Token

```http
POST /api/auth/refresh
Cookie: refreshToken=<refresh_token>
```

**Response** (200 OK):
```json
{
  "accessToken": "eyJhbGc...",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "username": "john_doe",
  "email": "user@example.com",
  "role": "User"
}
```

**Cookies Set**:
- `refreshToken`: New HTTP-only cookie with new refresh token (token rotation)

**Error Responses**:
- 400: Invalid or expired refresh token
- 401: Unauthorized

---

#### Logout

```http
POST /api/auth/logout
Authorization: Bearer <access_token>
Cookie: refreshToken=<refresh_token>
```

**Response** (200 OK):
```json
{
  "message": "Logged out successfully"
}
```

**Cookies Cleared**:
- `refreshToken`: Cookie removed

---

### User Management Endpoints

#### Get All Users (Admin Only)

```http
GET /api/users
Authorization: Bearer <access_token>
```

**Query Parameters**:
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10)

**Response** (200 OK):
```json
{
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "email": "user@example.com",
      "username": "john_doe",
      "role": "User",
      "isActive": true,
      "lastLoginAt": "2024-01-15T10:30:00Z",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "totalCount": 100,
  "page": 1,
  "pageSize": 10
}
```

**Authorization**: Requires Admin role

---

#### Get User By ID

```http
GET /api/users/{id}
Authorization: Bearer <access_token>
```

**Path Parameters**:
- `id`: User ID (UUID)

**Response** (200 OK):
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "username": "john_doe",
  "role": "User",
  "isActive": true,
  "lastLoginAt": "2024-01-15T10:30:00Z",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Authorization**: User can access their own data, Admin can access any user

**Error Responses**:
- 404: User not found
- 403: Forbidden (trying to access other user's data)

---

#### Update User Status (Admin Only)

```http
PATCH /api/users/{id}/status
Authorization: Bearer <access_token>
Content-Type: application/json
```

**Path Parameters**:
- `id`: User ID (UUID)

**Request Body**:
```json
{
  "isActive": false
}
```

**Response** (200 OK):
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "isActive": false
}
```

**Authorization**: Requires Admin role

**Side Effects**:
- Publishes `UserStatusChangedEvent` to RabbitMQ

---

## Document Service API

Base URL: `http://localhost:5000/document` (via API Gateway)

All endpoints require authentication.

### Document Endpoints

#### Get All Documents

```http
GET /api/document
Authorization: Bearer <access_token>
```

**Query Parameters**:
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10)
- `topic`: Filter by topic (optional)
- `userId`: Filter by user ID (optional)
- `categoryId`: Filter by category ID (optional)
- `searchTerm`: Search in title and description (optional)

**Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "userId": "user-id",
      "title": "Introduction to Microservices",
      "topic": "Software Architecture",
      "description": "A comprehensive guide to microservices",
      "totalPages": 25,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-15T10:30:00Z"
    }
  ],
  "totalCount": 50,
  "page": 1,
  "pageSize": 10
}
```

---

#### Get Document By ID

```http
GET /api/document/{id}
Authorization: Bearer <access_token>
```

**Path Parameters**:
- `id`: Document ID (UUID)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userId": "user-id",
    "title": "Introduction to Microservices",
    "topic": "Software Architecture",
    "description": "A comprehensive guide to microservices",
    "totalPages": 25,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  }
}
```

**Error Responses**:
- 404: Document not found

---

#### Get Document With Full Details

```http
GET /api/document/{id}/details
Authorization: Bearer <access_token>
```

**Path Parameters**:
- `id`: Document ID (UUID)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userId": "user-id",
    "title": "Introduction to Microservices",
    "topic": "Software Architecture",
    "description": "A comprehensive guide to microservices",
    "totalPages": 25,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-15T10:30:00Z",
    "pages": [
      {
        "id": "page-id",
        "pageNumber": 1,
        "fileId": "file-id",
        "createdAt": "2024-01-01T00:00:00Z"
      }
    ],
    "categories": [
      {
        "id": "category-id",
        "name": "Technology"
      }
    ]
  }
}
```

---

#### Create Document

```http
POST /api/document
Authorization: Bearer <access_token>
Content-Type: application/json
```

**Request Body**:
```json
{
  "title": "Introduction to Microservices",
  "topic": "Software Architecture",
  "description": "A comprehensive guide to microservices",
  "categoryIds": ["category-id-1", "category-id-2"]
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userId": "user-id",
    "title": "Introduction to Microservices",
    "topic": "Software Architecture",
    "description": "A comprehensive guide to microservices",
    "totalPages": 0,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

**Validation Rules**:
- Title: Required, max 200 characters
- Topic: Required, max 100 characters
- Description: Optional, max 1000 characters

---

#### Update Document

```http
PUT /api/document/{id}
Authorization: Bearer <access_token>
Content-Type: application/json
```

**Path Parameters**:
- `id`: Document ID (UUID)

**Request Body**:
```json
{
  "title": "Updated Title",
  "topic": "Updated Topic",
  "description": "Updated description",
  "categoryIds": ["category-id-1"]
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Updated Title",
    "topic": "Updated Topic",
    "description": "Updated description",
    "updatedAt": "2024-01-15T10:30:00Z"
  }
}
```

**Authorization**: Only document owner can update

**Error Responses**:
- 403: Forbidden (not document owner)
- 404: Document not found

---

#### Delete Document

```http
DELETE /api/document/{id}
Authorization: Bearer <access_token>
```

**Path Parameters**:
- `id`: Document ID (UUID)

**Response** (204 No Content)

**Authorization**: Only document owner or Admin can delete

**Note**: Soft delete - document is marked as deleted but not removed from database

---

### Page Endpoints

#### Get Pages By Document

```http
GET /api/page/document/{documentId}
Authorization: Bearer <access_token>
```

**Path Parameters**:
- `documentId`: Document ID (UUID)

**Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "id": "page-id",
      "documentId": "document-id",
      "pageNumber": 1,
      "fileId": "file-id",
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

---

#### Get Page By ID

```http
GET /api/page/{id}
Authorization: Bearer <access_token>
```

**Path Parameters**:
- `id`: Page ID (UUID)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "page-id",
    "documentId": "document-id",
    "pageNumber": 1,
    "fileId": "file-id",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

---

#### Create Page

```http
POST /api/page
Authorization: Bearer <access_token>
Content-Type: application/json
```

**Request Body**:
```json
{
  "documentId": "document-id",
  "pageNumber": 1,
  "fileId": "file-id"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "id": "new-page-id",
    "documentId": "document-id",
    "pageNumber": 1,
    "fileId": "file-id",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

**Validation Rules**:
- DocumentId: Required
- PageNumber: Required, positive integer
- FileId: Optional

---

#### Update Page

```http
PUT /api/page/{id}
Authorization: Bearer <access_token>
Content-Type: application/json
```

**Path Parameters**:
- `id`: Page ID (UUID)

**Request Body**:
```json
{
  "pageNumber": 2,
  "fileId": "new-file-id"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "page-id",
    "documentId": "document-id",
    "pageNumber": 2,
    "fileId": "new-file-id",
    "updatedAt": "2024-01-15T10:30:00Z"
  }
}
```

**Side Effects**:
- If fileId changes, publishes `PageUpdatedEvent` to delete old file

---

#### Delete Page

```http
DELETE /api/page/{id}
Authorization: Bearer <access_token>
```

**Path Parameters**:
- `id`: Page ID (UUID)

**Response** (204 No Content)

**Side Effects**:
- Publishes `PageDeletedEvent` to delete associated file

---

### Category Endpoints

#### Get All Categories

```http
GET /api/category
Authorization: Bearer <access_token>
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "id": "category-id",
      "name": "Technology",
      "description": "Technology related documents",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

---

#### Get Category By ID

```http
GET /api/category/{id}
Authorization: Bearer <access_token>
```

**Path Parameters**:
- `id`: Category ID (UUID)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "category-id",
    "name": "Technology",
    "description": "Technology related documents",
    "createdAt": "2024-01-01T00:00:00Z"
  }
}
```

---

#### Create Category

```http
POST /api/category
Authorization: Bearer <access_token>
Content-Type: application/json
```

**Request Body**:
```json
{
  "name": "Technology",
  "description": "Technology related documents"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "id": "new-category-id",
    "name": "Technology",
    "description": "Technology related documents",
    "createdAt": "2024-01-01T00:00:00Z"
  }
}
```

**Validation Rules**:
- Name: Required, max 100 characters
- Description: Optional, max 500 characters

---

#### Update Category

```http
PUT /api/category/{id}
Authorization: Bearer <access_token>
Content-Type: application/json
```

**Path Parameters**:
- `id`: Category ID (UUID)

**Request Body**:
```json
{
  "name": "Updated Technology",
  "description": "Updated description"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "category-id",
    "name": "Updated Technology",
    "description": "Updated description"
  }
}
```

---

#### Delete Category

```http
DELETE /api/category/{id}
Authorization: Bearer <access_token>
```

**Path Parameters**:
- `id`: Category ID (UUID)

**Response** (204 No Content)

---

### Bookmark Endpoints

#### Get User Bookmarks

```http
GET /api/bookmark
Authorization: Bearer <access_token>
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "id": "bookmark-id",
      "userId": "user-id",
      "pageId": "page-id",
      "note": "Important section",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

---

#### Get Bookmark By ID

```http
GET /api/bookmark/{id}
Authorization: Bearer <access_token>
```

**Path Parameters**:
- `id`: Bookmark ID (UUID)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "bookmark-id",
    "userId": "user-id",
    "pageId": "page-id",
    "note": "Important section",
    "createdAt": "2024-01-01T00:00:00Z"
  }
}
```

---

#### Create Bookmark

```http
POST /api/bookmark
Authorization: Bearer <access_token>
Content-Type: application/json
```

**Request Body**:
```json
{
  "pageId": "page-id",
  "note": "Important section"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "id": "new-bookmark-id",
    "userId": "user-id",
    "pageId": "page-id",
    "note": "Important section",
    "createdAt": "2024-01-01T00:00:00Z"
  }
}
```

**Validation Rules**:
- PageId: Required
- Note: Optional, max 500 characters

---

#### Delete Bookmark

```http
DELETE /api/bookmark/{id}
Authorization: Bearer <access_token>
```

**Path Parameters**:
- `id`: Bookmark ID (UUID)

**Response** (204 No Content)

**Authorization**: Only bookmark owner can delete

---

## FileStorage Service API

The FileStorage service uses **gRPC** for communication and is not directly exposed via REST API. It's accessed by the Document Service.

### gRPC Service Definition

```protobuf
service FileStorageService {
  rpc UploadFile (stream UploadFileRequest) returns (FileUploadResponse);
  rpc DownloadFile (FileRequest) returns (stream FileDownloadResponse);
  rpc GetFileMetadata (FileRequest) returns (FileMetadataResponse);
  rpc GetAllFiles (google.protobuf.Empty) returns (FileListResponse);
  rpc DeleteFile (FileRequest) returns (DeleteResponse);
  rpc GetPresignedUrl (FileRequest) returns (UrlResponse);
}
```

### File Types

```protobuf
enum FileType {
  PDF = 0;
  TEXT = 10;
  EXCEL = 20;
}
```

### gRPC Methods

#### UploadFile

Uploads a file using streaming.

**Request** (stream):
1. First message: Metadata
```json
{
  "metadata": {
    "file_name": "document.pdf",
    "content_type": "application/pdf",
    "file_path": "documents/2024/document.pdf"
  }
}
```

2. Subsequent messages: File chunks
```json
{
  "chunk_data": <bytes>
}
```

**Response**:
```json
{
  "id": "file-id",
  "file_type": "PDF",
  "file_name": "document.pdf",
  "file_path": "documents/2024/document.pdf",
  "created_at": "2024-01-01T00:00:00Z"
}
```

---

#### DownloadFile

Downloads a file using streaming.

**Request**:
```json
{
  "id": "file-id"
}
```

**Response** (stream):
1. First message: Metadata
```json
{
  "metadata": {
    "file_name": "document.pdf",
    "file_type": "PDF"
  }
}
```

2. Subsequent messages: File chunks
```json
{
  "chunk_data": <bytes>
}
```

---

#### GetFileMetadata

Gets file metadata.

**Request**:
```json
{
  "id": "file-id"
}
```

**Response**:
```json
{
  "id": "file-id",
  "file_type": "PDF",
  "file_name": "document.pdf",
  "file_path": "documents/2024/document.pdf",
  "created_at": "2024-01-01T00:00:00Z",
  "updated_at": "2024-01-01T00:00:00Z"
}
```

---

#### GetPresignedUrl

Gets a presigned URL for direct S3 access (expires in 15 minutes by default).

**Request**:
```json
{
  "id": "file-id"
}
```

**Response**:
```json
{
  "file_type": "PDF",
  "url": "https://s3.amazonaws.com/bucket/file?presigned-params"
}
```

---

#### DeleteFile

Deletes a file from S3 and database.

**Request**:
```json
{
  "id": "file-id"
}
```

**Response**:
```json
{
  "success": true
}
```

---

## Error Handling

### Standard Error Response

All errors follow this format:

```json
{
  "success": false,
  "message": "Error description",
  "errors": [
    {
      "field": "email",
      "message": "Email is required"
    }
  ]
}
```

### HTTP Status Codes

- **200 OK**: Successful GET, PUT, PATCH requests
- **201 Created**: Successful POST request
- **204 No Content**: Successful DELETE request
- **400 Bad Request**: Invalid request data, validation errors
- **401 Unauthorized**: Missing or invalid authentication token
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **409 Conflict**: Resource conflict (e.g., duplicate username)
- **429 Too Many Requests**: Rate limit exceeded
- **500 Internal Server Error**: Server-side error

### Common Error Scenarios

#### Authentication Errors

```json
{
  "success": false,
  "message": "Unauthorized",
  "statusCode": 401
}
```

#### Validation Errors

```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    {
      "field": "email",
      "message": "Invalid email format"
    },
    {
      "field": "password",
      "message": "Password must be at least 6 characters"
    }
  ]
}
```

#### Not Found Errors

```json
{
  "success": false,
  "message": "Document not found",
  "statusCode": 404
}
```

---

## Pagination

### Query Parameters

- `page`: Page number (starts at 1)
- `pageSize`: Number of items per page (default: 10, max: 100)

### Response Format

```json
{
  "data": [...],
  "totalCount": 150,
  "page": 1,
  "pageSize": 10,
  "totalPages": 15
}
```

### Example

```http
GET /api/document?page=2&pageSize=20
```

---

## Filtering

### Document Filters

Available filters on `/api/document`:

- `topic`: Filter by topic (exact match)
- `userId`: Filter by user ID
- `categoryId`: Filter by category ID
- `searchTerm`: Search in title and description (case-insensitive, partial match)

### Example

```http
GET /api/document?topic=Software%20Architecture&searchTerm=microservices&page=1&pageSize=20
```

---

## Rate Limiting

The API Gateway implements rate limiting:

- **Limit**: 5 requests per 10-second window
- **Response**: 429 Too Many Requests

### Rate Limit Headers

```http
X-RateLimit-Limit: 5
X-RateLimit-Remaining: 3
X-RateLimit-Reset: 1640000000
```

---

## SignalR Real-time Updates

### Connection

Connect to SignalR hub:

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5002/documentHub", {
        accessTokenFactory: () => accessToken
    })
    .build();

await connection.start();
```

### Events

The DocumentHub broadcasts these events:

- `DocumentCreated`: When a new document is created
- `DocumentUpdated`: When a document is updated
- `DocumentDeleted`: When a document is deleted
- `PageCreated`: When a new page is added
- `PageUpdated`: When a page is updated
- `PageDeleted`: When a page is deleted

### Subscribe to Events

```javascript
connection.on("DocumentCreated", (document) => {
    console.log("New document:", document);
});

connection.on("DocumentUpdated", (document) => {
    console.log("Document updated:", document);
});
```

---

## API Client Examples

### cURL Examples

**Login**:
```bash
curl -X POST http://localhost:5000/identity/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"john_doe","password":"password123"}'
```

**Create Document**:
```bash
curl -X POST http://localhost:5000/document/api/document \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"title":"My Document","topic":"General","description":"Test document"}'
```

### JavaScript/TypeScript Example

```typescript
// Login
const loginResponse = await fetch('http://localhost:5000/identity/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ username: 'john_doe', password: 'password123' }),
  credentials: 'include' // Include cookies
});

const { accessToken } = await loginResponse.json();

// Get documents
const documentsResponse = await fetch('http://localhost:5000/document/api/document', {
  headers: { 'Authorization': `Bearer ${accessToken}` }
});

const documents = await documentsResponse.json();
```

### C# Example

```csharp
using var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("http://localhost:5000");

// Login
var loginRequest = new { username = "john_doe", password = "password123" };
var loginResponse = await httpClient.PostAsJsonAsync("/identity/api/auth/login", loginRequest);
var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

// Get documents
httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", loginResult.AccessToken);
var documents = await httpClient.GetFromJsonAsync<DocumentListResponse>("/document/api/document");
```

---

## Best Practices

1. **Always use HTTPS in production**
2. **Store access tokens securely** (memory, not localStorage)
3. **Implement token refresh logic** before token expires
4. **Handle 401 errors** by refreshing token or redirecting to login
5. **Implement retry logic** for transient failures
6. **Use pagination** for large datasets
7. **Validate input** on client side before sending
8. **Handle rate limiting** with exponential backoff
9. **Use presigned URLs** for file downloads to reduce server load
10. **Subscribe to SignalR events** for real-time updates

---

For more information, refer to the Swagger documentation available at each service endpoint in development mode.
