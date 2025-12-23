# eBook Documentation

Welcome to the eBook project documentation! This directory contains comprehensive documentation for understanding, developing, deploying, and contributing to the eBook digital document management system.

## 📚 Documentation Index

### [Main README](../README.md)
**Start here!** Provides an overview of the project, features, quick start guide, and key concepts.

**Topics covered**:
- Project overview and features
- Technology stack
- Quick start with Docker Compose
- Project structure
- Microservices overview
- Configuration basics

---

### [Architecture Documentation](ARCHITECTURE.md)
Deep dive into the system architecture, design patterns, and technical decisions.

**Topics covered**:
- Microservices architecture design
- Clean Architecture implementation
- Communication patterns (REST, gRPC, RabbitMQ, SignalR)
- Domain models and entities
- Security architecture
- Scalability considerations
- Design patterns used
- Technology choices and rationale

**Read this if you want to**:
- Understand the system design
- Learn about architectural decisions
- Understand service interactions
- Explore design patterns

---

### [API Documentation](API.md)
Complete API reference for all microservices with request/response examples.

**Topics covered**:
- Authentication and authorization flow
- Identity Service API (auth, user management)
- Document Service API (documents, pages, categories, bookmarks)
- FileStorage Service API (gRPC methods)
- Error handling and status codes
- Pagination and filtering
- SignalR real-time events
- API client examples (cURL, JavaScript, C#)

**Read this if you want to**:
- Integrate with the API
- Understand available endpoints
- Learn request/response formats
- Build a client application

---

### [Deployment Guide](DEPLOYMENT.md)
Instructions for deploying the application in various environments.

**Topics covered**:
- Docker deployment
- Kubernetes deployment
- Production considerations (security, performance, reliability)
- Environment configuration
- Database setup and migrations
- Monitoring and logging
- Backup and recovery
- Troubleshooting deployment issues
- Rollback procedures

**Read this if you want to**:
- Deploy the application
- Set up production environment
- Configure monitoring
- Plan disaster recovery
- Scale the system

---

### [Development Guide](DEVELOPMENT.md)
Guide for developers working on the codebase.

**Topics covered**:
- Development environment setup
- Project structure details
- Coding standards and conventions
- Development workflow
- Testing (unit and integration)
- Debugging techniques
- Database migrations
- Adding new features
- Common development tasks

**Read this if you want to**:
- Start developing
- Understand the codebase
- Follow best practices
- Add new features
- Write tests

---

### [Contributing Guide](CONTRIBUTING.md)
Guidelines for contributing to the project.

**Topics covered**:
- Code of conduct
- How to contribute
- Development process
- Pull request process
- Coding standards
- Reporting bugs
- Suggesting enhancements
- Community guidelines

**Read this if you want to**:
- Contribute code
- Report bugs
- Suggest features
- Understand the contribution process

---

## 🚀 Quick Links by Role

### For New Users
1. Start with [Main README](../README.md)
2. Try the Quick Start guide
3. Explore [API Documentation](API.md)

### For Developers
1. Read [Main README](../README.md)
2. Follow [Development Guide](DEVELOPMENT.md)
3. Review [Architecture Documentation](ARCHITECTURE.md)
4. Check [Contributing Guide](CONTRIBUTING.md)

### For DevOps/Infrastructure
1. Review [Deployment Guide](DEPLOYMENT.md)
2. Understand [Architecture Documentation](ARCHITECTURE.md)
3. Check [Main README](../README.md) for configuration

### For API Consumers
1. Start with [API Documentation](API.md)
2. Review authentication in [Architecture Documentation](ARCHITECTURE.md#security-architecture)
3. Check examples in [API Documentation](API.md#api-client-examples)

---

## 📖 Documentation Structure

```
docs/
├── README.md              # This file - Documentation index
├── ARCHITECTURE.md        # System architecture and design
├── API.md                # Complete API reference
├── DEPLOYMENT.md         # Deployment and operations guide
├── DEVELOPMENT.md        # Developer guide
└── CONTRIBUTING.md       # Contribution guidelines
```

---

## 🔍 Finding Information

### Search Tips

1. **Installation/Setup**: See [Main README](../README.md) and [Development Guide](DEVELOPMENT.md)
2. **API Endpoints**: Check [API Documentation](API.md)
3. **Architecture Questions**: Review [Architecture Documentation](ARCHITECTURE.md)
4. **Deployment Issues**: Consult [Deployment Guide](DEPLOYMENT.md)
5. **Contributing**: Read [Contributing Guide](CONTRIBUTING.md)
6. **Coding Standards**: See [Development Guide](DEVELOPMENT.md#coding-standards)

### Common Questions

**Q: How do I run the application locally?**  
A: See [Quick Start](../README.md#quick-start-with-docker-compose) or [Development Setup](DEVELOPMENT.md#development-environment-setup)

**Q: How do I authenticate with the API?**  
A: See [Authentication Flow](API.md#authentication)

**Q: What technologies are used?**  
A: See [Technology Stack](../README.md#technology-stack)

**Q: How do the services communicate?**  
A: See [Communication Patterns](ARCHITECTURE.md#communication-patterns)

**Q: How do I deploy to production?**  
A: See [Deployment Guide](DEPLOYMENT.md)

**Q: How do I contribute?**  
A: See [Contributing Guide](CONTRIBUTING.md)

---

## 📝 Documentation Statistics

- **Total Documentation Files**: 6 (including main README)
- **Total Lines**: ~5,000 lines
- **Topics Covered**: Architecture, API, Deployment, Development, Contributing
- **Last Updated**: December 2024

---

## 🤝 Improving Documentation

Found an error or want to improve documentation?

1. **Small fixes**: Create a pull request with the fix
2. **New content**: Open an issue to discuss first
3. **Questions**: Open a GitHub Discussion

See [Contributing Guide](CONTRIBUTING.md) for details.

---

## 📞 Support

Need help?

1. **Documentation**: Start here in the docs folder
2. **GitHub Issues**: Report bugs or request features
3. **GitHub Discussions**: Ask questions and discuss

---

## 📄 License

This documentation is part of the eBook project and follows the same license.

---

**Happy Building! 🎉**
