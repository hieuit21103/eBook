# Deployment Guide

This guide covers deployment strategies for the eBook microservices application.

## Table of Contents

- [Docker Deployment](#docker-deployment)
- [Kubernetes Deployment](#kubernetes-deployment)
- [Production Considerations](#production-considerations)
- [Environment Configuration](#environment-configuration)
- [Database Setup](#database-setup)
- [Monitoring and Logging](#monitoring-and-logging)
- [Backup and Recovery](#backup-and-recovery)
- [Troubleshooting](#troubleshooting)

## Docker Deployment

### Prerequisites

- Docker Engine 20.10+
- Docker Compose 2.0+
- Minimum 4GB RAM
- 10GB free disk space

### Quick Start

1. **Clone the repository**:
```bash
git clone https://github.com/hieuit21103/eBook.git
cd eBook
```

2. **Configure environment**:
```bash
cp .env.example .env
# Edit .env with your configuration
nano .env
```

3. **Start services**:
```bash
docker-compose up -d
```

4. **Verify deployment**:
```bash
docker-compose ps
docker-compose logs -f
```

5. **Access services**:
- API Gateway: http://localhost:5000
- RabbitMQ Management: http://localhost:15672

### Docker Compose Configuration

The `docker-compose.yml` defines all services:

```yaml
services:
  apigateway:     # Port 5000
  identity:       # Port 5001 (internal)
  document:       # Port 5002 (internal)
  filestorage:    # Port 5003 (internal)
  rabbitmq:       # Ports 5672, 15672
```

### Service Management

**Start all services**:
```bash
docker-compose up -d
```

**Stop all services**:
```bash
docker-compose down
```

**Restart a specific service**:
```bash
docker-compose restart identity
```

**View logs**:
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f document

# Last 100 lines
docker-compose logs --tail=100 identity
```

**Scale services**:
```bash
docker-compose up -d --scale document=3
```

### Building Images

**Build all images**:
```bash
docker-compose build
```

**Build specific service**:
```bash
docker-compose build identity
```

**Build without cache**:
```bash
docker-compose build --no-cache
```

### Data Persistence

Data is persisted in Docker volumes:

```bash
# List volumes
docker volume ls

# Inspect volume
docker volume inspect ebook_postgres_data

# Backup volume
docker run --rm -v ebook_postgres_data:/data -v $(pwd):/backup \
  alpine tar czf /backup/postgres-backup.tar.gz -C /data .

# Restore volume
docker run --rm -v ebook_postgres_data:/data -v $(pwd):/backup \
  alpine tar xzf /backup/postgres-backup.tar.gz -C /data
```

## Kubernetes Deployment

### Prerequisites

- Kubernetes cluster (1.24+)
- kubectl configured
- Helm 3.0+ (optional)

### Namespace Setup

```bash
kubectl create namespace ebook

# Set as default
kubectl config set-context --current --namespace=ebook
```

### ConfigMaps and Secrets

**Create ConfigMap for non-sensitive data**:
```yaml
# configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: ebook-config
  namespace: ebook
data:
  PG_HOST: "postgres-service"
  PG_PORT: "5432"
  REDIS_HOST: "redis-service"
  RABBITMQ_HOST: "rabbitmq-service"
```

```bash
kubectl apply -f configmap.yaml
```

**Create Secret for sensitive data**:
```yaml
# secret.yaml
apiVersion: v1
kind: Secret
metadata:
  name: ebook-secrets
  namespace: ebook
type: Opaque
stringData:
  PG_USERNAME: "postgres"
  PG_PASSWORD: "your_secure_password"
  JWT_SECRET_KEY: "your_jwt_secret_minimum_32_characters"
  S3_ACCESS_KEY: "your_s3_access_key"
  S3_SECRET_KEY: "your_s3_secret_key"
  RABBITMQ_USERNAME: "admin"
  RABBITMQ_PASSWORD: "secure_password"
```

```bash
kubectl apply -f secret.yaml
```

### PostgreSQL Deployment

```yaml
# postgres-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: postgres
  namespace: ebook
spec:
  replicas: 1
  selector:
    matchLabels:
      app: postgres
  template:
    metadata:
      labels:
        app: postgres
    spec:
      containers:
      - name: postgres
        image: postgres:16
        ports:
        - containerPort: 5432
        env:
        - name: POSTGRES_USER
          valueFrom:
            secretKeyRef:
              name: ebook-secrets
              key: PG_USERNAME
        - name: POSTGRES_PASSWORD
          valueFrom:
            secretKeyRef:
              name: ebook-secrets
              key: PG_PASSWORD
        volumeMounts:
        - name: postgres-storage
          mountPath: /var/lib/postgresql/data
      volumes:
      - name: postgres-storage
        persistentVolumeClaim:
          claimName: postgres-pvc
---
apiVersion: v1
kind: Service
metadata:
  name: postgres-service
  namespace: ebook
spec:
  selector:
    app: postgres
  ports:
  - port: 5432
    targetPort: 5432
```

### Identity Service Deployment

```yaml
# identity-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: identity
  namespace: ebook
spec:
  replicas: 2
  selector:
    matchLabels:
      app: identity
  template:
    metadata:
      labels:
        app: identity
    spec:
      containers:
      - name: identity
        image: ebook/identity:latest
        ports:
        - containerPort: 5001
        env:
        - name: ASPNETCORE_URLS
          value: "http://+:5001"
        - name: ConnectionStrings__PgHost
          valueFrom:
            configMapKeyRef:
              name: ebook-config
              key: PG_HOST
        - name: ConnectionStrings__PgPort
          valueFrom:
            configMapKeyRef:
              name: ebook-config
              key: PG_PORT
        - name: ConnectionStrings__PgUsername
          valueFrom:
            secretKeyRef:
              name: ebook-secrets
              key: PG_USERNAME
        - name: ConnectionStrings__PgPassword
          valueFrom:
            secretKeyRef:
              name: ebook-secrets
              key: PG_PASSWORD
        - name: Jwt__SecretKey
          valueFrom:
            secretKeyRef:
              name: ebook-secrets
              key: JWT_SECRET_KEY
        livenessProbe:
          httpGet:
            path: /health
            port: 5001
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 5001
          initialDelaySeconds: 10
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: identity-service
  namespace: ebook
spec:
  selector:
    app: identity
  ports:
  - port: 5001
    targetPort: 5001
```

### Ingress Configuration

```yaml
# ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: ebook-ingress
  namespace: ebook
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
spec:
  ingressClassName: nginx
  tls:
  - hosts:
    - api.ebook.example.com
    secretName: ebook-tls
  rules:
  - host: api.ebook.example.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: apigateway-service
            port:
              number: 5000
```

### Horizontal Pod Autoscaling

```yaml
# hpa.yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: identity-hpa
  namespace: ebook
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: identity
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

### Deploy to Kubernetes

```bash
# Apply all configurations
kubectl apply -f k8s/

# Check deployment status
kubectl get deployments
kubectl get pods
kubectl get services

# Check logs
kubectl logs -f deployment/identity

# Describe pod for troubleshooting
kubectl describe pod <pod-name>
```

## Production Considerations

### Security

1. **Use HTTPS/TLS**:
   - Configure SSL certificates (Let's Encrypt)
   - Redirect HTTP to HTTPS
   - Use secure cookies

2. **Secret Management**:
   - Use Kubernetes Secrets or external secret managers (AWS Secrets Manager, Azure Key Vault)
   - Rotate secrets regularly
   - Never commit secrets to git

3. **Network Security**:
   - Use network policies to restrict pod-to-pod communication
   - Configure firewall rules
   - Use private networks for databases

4. **Authentication & Authorization**:
   - Enforce strong password policies
   - Implement rate limiting
   - Use role-based access control

5. **API Security**:
   - Validate all inputs
   - Implement CORS properly
   - Use API versioning

### Performance

1. **Database Optimization**:
   - Create appropriate indexes
   - Use connection pooling
   - Configure read replicas for read-heavy workloads
   - Regular maintenance (VACUUM, ANALYZE)

2. **Caching**:
   - Implement Redis caching for frequently accessed data
   - Use distributed caching for multi-instance scenarios
   - Cache API responses where appropriate

3. **Load Balancing**:
   - Use load balancer (NGINX, HAProxy, cloud LB)
   - Configure health checks
   - Implement session affinity if needed

4. **Resource Limits**:
   ```yaml
   resources:
     requests:
       memory: "256Mi"
       cpu: "250m"
     limits:
       memory: "512Mi"
       cpu: "500m"
   ```

### Reliability

1. **Health Checks**:
   - Configure liveness probes
   - Configure readiness probes
   - Monitor health endpoints

2. **Graceful Shutdown**:
   ```yaml
   lifecycle:
     preStop:
       exec:
         command: ["/bin/sh", "-c", "sleep 15"]
   ```

3. **Circuit Breakers**:
   - Implement using Polly
   - Configure timeouts and retries
   - Handle failures gracefully

4. **Backup Strategy**:
   - Automated database backups
   - Backup retention policy
   - Test restore procedures

### Scalability

1. **Horizontal Scaling**:
   - Use HPA for automatic scaling
   - Ensure services are stateless
   - Use external session storage (Redis)

2. **Database Scaling**:
   - Read replicas for read operations
   - Connection pooling
   - Consider database sharding for large datasets

3. **Message Queue**:
   - RabbitMQ clustering for high availability
   - Configure queue limits
   - Monitor queue depth

4. **File Storage**:
   - Use CDN for file delivery
   - Implement presigned URLs
   - Configure S3 lifecycle policies

## Environment Configuration

### Development

```env
# .env.development
PG_HOST=localhost
PG_PORT=5432
REDIS_CONNECTION_STRING=localhost:6379
S3_ENDPOINT=http://localhost:9000
ASPNETCORE_ENVIRONMENT=Development
```

### Staging

```env
# .env.staging
PG_HOST=staging-db.example.com
PG_PORT=5432
REDIS_CONNECTION_STRING=staging-redis.example.com:6379
S3_ENDPOINT=https://s3-staging.example.com
ASPNETCORE_ENVIRONMENT=Staging
```

### Production

```env
# .env.production
PG_HOST=prod-db.example.com
PG_PORT=5432
REDIS_CONNECTION_STRING=prod-redis.example.com:6379
S3_ENDPOINT=https://s3.amazonaws.com
ASPNETCORE_ENVIRONMENT=Production
```

## Database Setup

### Manual Database Creation

```sql
-- Connect to PostgreSQL
psql -h localhost -U postgres

-- Create databases
CREATE DATABASE ebook_identity;
CREATE DATABASE ebook_documents;
CREATE DATABASE ebook_filestorage;

-- Create user
CREATE USER ebook_user WITH PASSWORD 'secure_password';

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE ebook_identity TO ebook_user;
GRANT ALL PRIVILEGES ON DATABASE ebook_documents TO ebook_user;
GRANT ALL PRIVILEGES ON DATABASE ebook_filestorage TO ebook_user;
```

### Run Migrations

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

### Production Migration Strategy

1. **Backup database** before migration
2. **Test migrations** in staging environment
3. **Schedule maintenance window** for production
4. **Apply migrations** during low traffic period
5. **Verify** application functionality after migration
6. **Rollback plan** ready if issues occur

## Monitoring and Logging

### Application Logging

Configure logging in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

### Structured Logging with Serilog

```csharp
// Install Serilog packages
// Serilog.AspNetCore
// Serilog.Sinks.Console
// Serilog.Sinks.File
// Serilog.Sinks.Elasticsearch

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "ebook-logs-{0:yyyy.MM.dd}"
    })
    .CreateLogger();
```

### Monitoring Tools

**Prometheus + Grafana**:
```bash
# Add Prometheus endpoint
dotnet add package prometheus-net.AspNetCore

// Program.cs
app.UseMetricServer();
app.UseHttpMetrics();
```

**Application Insights** (Azure):
```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore

// Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

### Health Checks

Configure comprehensive health checks:

```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddRedis(redisConnectionString)
    .AddRabbitMQ(rabbitMqConnectionString);

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

## Backup and Recovery

### Database Backup

**Automated backup script**:
```bash
#!/bin/bash
# backup-db.sh

TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_DIR="/backups"

# Backup Identity database
pg_dump -h $PG_HOST -U $PG_USER ebook_identity > \
  $BACKUP_DIR/identity_$TIMESTAMP.sql

# Backup Document database
pg_dump -h $PG_HOST -U $PG_USER ebook_documents > \
  $BACKUP_DIR/documents_$TIMESTAMP.sql

# Backup FileStorage database
pg_dump -h $PG_HOST -U $PG_USER ebook_filestorage > \
  $BACKUP_DIR/filestorage_$TIMESTAMP.sql

# Compress backups
gzip $BACKUP_DIR/*_$TIMESTAMP.sql

# Upload to S3
aws s3 cp $BACKUP_DIR/ s3://ebook-backups/ --recursive

# Delete local backups older than 7 days
find $BACKUP_DIR -type f -mtime +7 -delete
```

**Schedule with cron**:
```cron
# Run backup daily at 2 AM
0 2 * * * /scripts/backup-db.sh
```

### Database Restore

```bash
# Restore from backup
gunzip identity_20240101_020000.sql.gz
psql -h $PG_HOST -U $PG_USER ebook_identity < identity_20240101_020000.sql
```

### S3 Backup

Configure S3 lifecycle policies for automatic backup:

```json
{
  "Rules": [
    {
      "Id": "ArchiveOldFiles",
      "Status": "Enabled",
      "Transitions": [
        {
          "Days": 90,
          "StorageClass": "GLACIER"
        }
      ],
      "Expiration": {
        "Days": 365
      }
    }
  ]
}
```

## Troubleshooting

### Common Issues

#### Services Not Starting

**Check logs**:
```bash
docker-compose logs identity
kubectl logs -f deployment/identity
```

**Common causes**:
- Database connection failure
- Missing environment variables
- Port conflicts
- Insufficient resources

#### Database Connection Issues

**Test connection**:
```bash
psql -h $PG_HOST -U $PG_USER -d ebook_identity
```

**Check network**:
```bash
docker network inspect ebook-network
kubectl get services
```

#### RabbitMQ Connection Issues

**Access RabbitMQ management**:
```
http://localhost:15672
```

**Check queues**:
```bash
docker exec ebook-rabbitmq rabbitmqctl list_queues
```

#### File Upload Failures

**Check S3 connectivity**:
```bash
aws s3 ls s3://your-bucket
```

**Verify permissions**:
- S3 bucket policy
- IAM role permissions
- CORS configuration

### Performance Issues

**Check resource usage**:
```bash
docker stats
kubectl top pods
```

**Database performance**:
```sql
-- Check slow queries
SELECT * FROM pg_stat_statements 
ORDER BY mean_exec_time DESC 
LIMIT 10;

-- Check index usage
SELECT * FROM pg_stat_user_indexes;
```

**Application metrics**:
- Monitor response times
- Check error rates
- Monitor memory usage
- Check connection pool stats

### Debug Mode

Enable debug logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

Access container:
```bash
docker exec -it ebook-identity /bin/bash
kubectl exec -it <pod-name> -- /bin/bash
```

## Rollback Procedures

### Docker Rollback

```bash
# Stop current version
docker-compose down

# Checkout previous version
git checkout <previous-tag>

# Start previous version
docker-compose up -d
```

### Kubernetes Rollback

```bash
# View deployment history
kubectl rollout history deployment/identity

# Rollback to previous version
kubectl rollout undo deployment/identity

# Rollback to specific revision
kubectl rollout undo deployment/identity --to-revision=2

# Check rollback status
kubectl rollout status deployment/identity
```

### Database Rollback

```bash
# Restore from backup
psql -h $PG_HOST -U $PG_USER ebook_identity < backup.sql

# Or use EF Core migrations
cd src/Identity
dotnet ef database update <previous-migration>
```

## Production Checklist

Before deploying to production:

- [ ] All environment variables configured
- [ ] Secrets properly secured
- [ ] HTTPS/TLS certificates installed
- [ ] Database migrations tested
- [ ] Backup strategy in place
- [ ] Monitoring and alerting configured
- [ ] Load testing completed
- [ ] Security scan completed
- [ ] Documentation updated
- [ ] Rollback plan prepared
- [ ] On-call team notified
- [ ] Maintenance window scheduled

## Support

For deployment issues:
1. Check logs for error messages
2. Verify configuration
3. Review this documentation
4. Check GitHub issues
5. Contact development team

---

For additional help, refer to the [Architecture Documentation](ARCHITECTURE.md) and [API Documentation](API.md).
