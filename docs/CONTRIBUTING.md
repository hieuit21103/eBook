# Contributing to eBook

Thank you for your interest in contributing to the eBook project! This document provides guidelines and instructions for contributing.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [How to Contribute](#how-to-contribute)
- [Development Process](#development-process)
- [Pull Request Process](#pull-request-process)
- [Coding Standards](#coding-standards)
- [Reporting Bugs](#reporting-bugs)
- [Suggesting Enhancements](#suggesting-enhancements)
- [Community](#community)

## Code of Conduct

### Our Pledge

We pledge to make participation in our project a harassment-free experience for everyone, regardless of age, body size, disability, ethnicity, gender identity and expression, level of experience, nationality, personal appearance, race, religion, or sexual identity and orientation.

### Our Standards

**Positive behavior includes**:
- Using welcoming and inclusive language
- Being respectful of differing viewpoints and experiences
- Gracefully accepting constructive criticism
- Focusing on what is best for the community
- Showing empathy towards other community members

**Unacceptable behavior includes**:
- Trolling, insulting/derogatory comments, and personal attacks
- Public or private harassment
- Publishing others' private information without permission
- Other conduct which could reasonably be considered inappropriate

## Getting Started

### Prerequisites

Before contributing, ensure you have:

1. **Development Environment**: See [Development Guide](DEVELOPMENT.md) for setup instructions
2. **GitHub Account**: Create one at [github.com](https://github.com)
3. **Git Knowledge**: Basic understanding of Git and GitHub workflows
4. **.NET Experience**: Familiarity with C# and .NET development

### Fork and Clone

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
```bash
git clone https://github.com/YOUR_USERNAME/eBook.git
cd eBook
```

3. **Add upstream remote**:
```bash
git remote add upstream https://github.com/hieuit21103/eBook.git
```

4. **Verify remotes**:
```bash
git remote -v
# origin    https://github.com/YOUR_USERNAME/eBook.git (fetch)
# origin    https://github.com/YOUR_USERNAME/eBook.git (push)
# upstream  https://github.com/hieuit21103/eBook.git (fetch)
# upstream  https://github.com/hieuit21103/eBook.git (push)
```

### Set Up Development Environment

Follow the [Development Guide](DEVELOPMENT.md) to:
1. Install required tools
2. Configure local environment
3. Run the application locally
4. Verify everything works

## How to Contribute

### Types of Contributions

We welcome various types of contributions:

1. **Bug Fixes**: Fix issues reported in GitHub Issues
2. **New Features**: Implement new functionality
3. **Documentation**: Improve or add documentation
4. **Tests**: Add or improve test coverage
5. **Performance**: Optimize code performance
6. **Refactoring**: Improve code structure and quality
7. **Security**: Fix security vulnerabilities

### Finding Something to Work On

1. **Browse Issues**: Check [GitHub Issues](https://github.com/hieuit21103/eBook/issues)
2. **Look for Labels**:
   - `good first issue`: Good for newcomers
   - `help wanted`: Extra attention is needed
   - `bug`: Something isn't working
   - `enhancement`: New feature or request
   - `documentation`: Improvements or additions to documentation

3. **Ask for Assignment**: Comment on the issue asking to be assigned

## Development Process

### 1. Create a Branch

Always create a new branch for your work:

```bash
# Update your local main branch
git checkout main
git pull upstream main

# Create and switch to a new branch
git checkout -b feature/your-feature-name
```

**Branch naming conventions**:
- `feature/description` - New features
- `bugfix/description` - Bug fixes
- `hotfix/description` - Urgent fixes
- `docs/description` - Documentation changes
- `refactor/description` - Code refactoring
- `test/description` - Test additions/changes

### 2. Make Changes

1. **Write Code**: Implement your changes following [Coding Standards](#coding-standards)
2. **Write Tests**: Add tests for new functionality
3. **Update Documentation**: Update relevant documentation
4. **Test Locally**: Ensure all tests pass

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run locally
dotnet run
```

### 3. Commit Changes

**Follow Conventional Commits**:

```
type(scope): subject

body (optional)

footer (optional)
```

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks
- `perf`: Performance improvements

**Examples**:

```bash
git commit -m "feat(document): add tagging functionality

Add ability to tag documents with custom tags.
Users can add, remove, and filter by tags.

Closes #123"
```

```bash
git commit -m "fix(auth): resolve token refresh issue"
```

```bash
git commit -m "docs: update API documentation"
```

### 4. Keep Your Branch Updated

Regularly sync with upstream:

```bash
git fetch upstream
git rebase upstream/main
```

If there are conflicts:
```bash
# Resolve conflicts in your editor
git add .
git rebase --continue
```

### 5. Push Changes

```bash
git push origin feature/your-feature-name
```

## Pull Request Process

### Before Creating a PR

Ensure:
- [ ] Code builds successfully
- [ ] All tests pass
- [ ] New tests added for new features
- [ ] Code follows project conventions
- [ ] Documentation updated
- [ ] Commits follow conventional commit format
- [ ] Branch is up-to-date with main

### Creating a Pull Request

1. **Go to GitHub**: Navigate to your fork
2. **Click "New Pull Request"**
3. **Select Branches**:
   - Base: `hieuit21103/eBook` - `main`
   - Compare: `your-fork` - `your-branch`

4. **Fill PR Template**:

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Related Issues
Closes #123

## Changes Made
- Change 1
- Change 2
- Change 3

## Testing
How was this tested?

## Checklist
- [ ] Code builds successfully
- [ ] Tests pass
- [ ] Documentation updated
- [ ] Follows coding standards
```

5. **Submit PR**

### PR Review Process

1. **Automated Checks**: CI/CD runs automated tests
2. **Code Review**: Maintainers review your code
3. **Feedback**: Address review comments
4. **Approval**: At least one maintainer approves
5. **Merge**: Maintainer merges your PR

### Addressing Review Comments

1. **Make Changes**:
```bash
# Make changes in your local branch
git add .
git commit -m "fix: address review comments"
git push origin feature/your-feature-name
```

2. **Respond to Comments**: Reply to comments on GitHub

3. **Request Re-review**: Request another review after changes

### After Your PR is Merged

1. **Delete Your Branch**:
```bash
git branch -d feature/your-feature-name
git push origin --delete feature/your-feature-name
```

2. **Update Your Fork**:
```bash
git checkout main
git pull upstream main
git push origin main
```

## Coding Standards

### C# Style Guide

Follow the guidelines in [Development Guide - Coding Standards](DEVELOPMENT.md#coding-standards)

**Key Points**:
- Use PascalCase for public members
- Use camelCase for private fields and local variables
- Prefix interfaces with 'I'
- Suffix async methods with 'Async'
- Use meaningful variable names
- Add XML comments for public APIs
- Keep methods small and focused
- Use dependency injection
- Follow SOLID principles

### Code Quality

**Use Code Analysis**:
```bash
# Run code analysis
dotnet build /p:TreatWarningsAsErrors=true

# Run security scan
dotnet list package --vulnerable
```

**Use EditorConfig**:
The project includes `.editorconfig` for consistent formatting.

### Testing Requirements

**All new features must include**:
- Unit tests for business logic
- Integration tests for API endpoints
- Minimum 80% code coverage

**Test Structure**:
```csharp
public class MyServiceTests
{
    [Fact]
    public async Task MethodName_Scenario_ExpectedBehavior()
    {
        // Arrange
        // ... setup

        // Act
        // ... execute

        // Assert
        // ... verify
    }
}
```

## Reporting Bugs

### Before Submitting a Bug Report

1. **Check Documentation**: Ensure it's not expected behavior
2. **Search Existing Issues**: Check if already reported
3. **Try Latest Version**: Bug might be already fixed

### Submitting a Bug Report

Create an issue with:

**Title**: Clear, descriptive title

**Description**:
```markdown
## Description
Brief description of the bug

## Steps to Reproduce
1. Go to '...'
2. Click on '...'
3. Scroll down to '...'
4. See error

## Expected Behavior
What you expected to happen

## Actual Behavior
What actually happened

## Environment
- OS: [e.g., Windows 11, macOS 13]
- .NET Version: [e.g., 9.0.0]
- Browser: [if applicable]

## Additional Context
- Screenshots
- Error logs
- Stack traces
```

**Labels**: Add appropriate labels (bug, priority, etc.)

## Suggesting Enhancements

### Before Submitting an Enhancement

1. **Check if it aligns** with project goals
2. **Search existing issues** for similar suggestions
3. **Consider alternatives** and trade-offs

### Submitting an Enhancement Suggestion

Create an issue with:

```markdown
## Feature Description
Clear description of the enhancement

## Problem it Solves
What problem does this solve?

## Proposed Solution
How should this work?

## Alternatives Considered
What other solutions did you consider?

## Additional Context
- Mockups
- Examples from other projects
- Use cases
```

## Community

### Communication Channels

- **GitHub Issues**: Bug reports and feature requests
- **GitHub Discussions**: General questions and discussions
- **Pull Requests**: Code review and feedback

### Getting Help

1. **Check Documentation**: See `docs/` folder
2. **Search Issues**: Look for similar questions
3. **Ask Question**: Create a GitHub Discussion

### Recognition

We appreciate all contributions! Contributors will be:
- Listed in project contributors
- Credited in release notes
- Thanked in community channels

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.

## Questions?

If you have questions about contributing, feel free to:
1. Open a GitHub Discussion
2. Comment on an existing issue
3. Reach out to maintainers

---

Thank you for contributing to eBook! 🎉
