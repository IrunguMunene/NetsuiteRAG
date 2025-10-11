# NetSuite RAG Reporting Tool

[![CI](https://github.com/IrunguMunene/NetsuiteRAG/actions/workflows/ci.yml/badge.svg)](https://github.com/IrunguMunene/NetsuiteRAG/actions/workflows/ci.yml)

A production-ready NetSuite reporting tool that uses RAG (Retrieval-Augmented Generation) to convert natural language queries into NetSuite SavedSearch plans.

## Features

- **Natural Language Querying**: Ask questions in plain English, get NetSuite results
- **Intelligent Template Reuse**: Learn from previous successful queries
- **5-Layer Validation**: Comprehensive validation pipeline ensures query safety and correctness
- **Real-time Streaming**: Server-Sent Events (SSE) for responsive large dataset handling
- **Governance & Audit**: Complete audit trail and template approval workflow

## Tech Stack

- **Backend**: .NET 9, C# 13, ASP.NET Core
- **Frontend**: Angular 20 (standalone components)
- **Database**: PostgreSQL
- **Cache**: Redis
- **Vector Store**: Qdrant
- **LLM**: Ollama (llama3.1:8b, nomic-embed-text)
- **Orchestration**: .NET Aspire
- **CI/CD**: GitHub Actions

## Project Structure

```
src/
├── NetSuiteRAG.Api/         # Main API service
├── NetSuiteRAG.Shared/      # Shared models and contracts
└── NetSuiteRAG.AppHost/     # Aspire orchestration host

tests/
└── NetSuiteRAG.Tests/       # Unit and integration tests
```

## Getting Started

### Prerequisites

- .NET 9 SDK
- Docker (for PostgreSQL, Redis, Qdrant)
- Ollama with llama3.1:8b and nomic-embed-text models

### Running Locally

1. Start infrastructure services:
   ```powershell
   docker-compose up -d
   ```

2. Run the application with Aspire:
   ```bash
   dotnet run --project src/NetSuiteRAG.AppHost
   ```

3. Access the Aspire Dashboard at `http://localhost:15888`

### Running Tests

```bash
dotnet test
```

## Development

See [CONVENTIONS.md](CONVENTIONS.md) for coding standards and [ARCHITECTURE.md](ARCHITECTURE.md) for system design details.

### Key Scripts

- `.\scripts\check-environment.ps1` - Verify all services running
- `.\scripts\start-session.ps1` - Start development session
- `.\scripts\review-code.ps1` - Run code review checks

## CI/CD

The project uses GitHub Actions for continuous integration:
- Build verification
- Automated testing
- Code formatting checks
- Build warnings as errors

## License

[License TBD]
