# ABPGroup

A full-stack starter based on ASP.NET Boilerplate (ABP) with an ASP.NET Core backend and a Next.js frontend.

## Overview

ABPGroup is structured as a backend-first template using ABP (ASP.NET Boilerplate) with multi-tenancy, authentication, and role-based administration baked in. The backend exposes application services and JWT authentication endpoints and ships with database migrations and a dedicated migrator project.

The frontend is a Next.js app under `frontend/` and currently contains the default starter page. This repo is a good base for building a multi-tenant admin system with a modern React UI backed by ABP.

## Design

| Resource     | Link                              |
| ------------ | --------------------------------- |
| Figma Design | [View Designs](#) (placeholder)   |
| Live Demo    | Not deployed yet (placeholder)    |

## Tech Stack

| Tool | Purpose |
| --- | --- |
| .NET 9.0 (TargetFramework `net9.0`) | Backend runtime |
| ASP.NET Boilerplate (ABP) 10.2.0 | Modular application framework |
| ASP.NET Core | Web host and API |
| Entity Framework Core 9.0.5 | ORM and migrations |
| Npgsql EF Core Provider 9.0.4 | PostgreSQL support |
| SQL Server EF Core Provider 9.0.5 | SQL Server support |
| Swashbuckle.AspNetCore 8.1.2 | Swagger/OpenAPI tooling |
| JWT Bearer Auth 9.0.5 | Token authentication |
| Next.js 16.1.6 | Frontend framework |
| React 19.2.3 | UI library |
| antd-style 4.1.0 | Styling utilities |
| TypeScript 5.x | Static typing |
| xUnit 2.9.3 | Backend tests |

## Roles

Seeded roles found in the backend:

| Feature | Host Admin | Tenant Admin |
| --- | :---: | :---: |
| Tenant management (`Pages.Tenants`) | Yes | No |
| User management (`Pages.Users`) | Yes | Yes |
| User activation (`Pages.Users.Activation`) | Yes | Yes |
| Role management (`Pages.Roles`) | Yes | Yes |

## Features

### Authentication
- JWT token authentication (`/api/TokenAuth/Authenticate`)
- Account registration service and tenant availability check

### Multi-Tenancy
- Host and tenant roles with seeded admin users
- Tenant availability endpoint

### Administration
- User and role management services
- Session info and configuration services

### Data & Migrations
- EF Core migrations in `aspnet-core/src/ABPGroup.EntityFrameworkCore/Migrations`
- Separate migrator project for database initialization

### Frontend
- Next.js App Router project in `frontend/`
- Starter page in `frontend/src/app/page.tsx`

## Project Structure

```
.
├── aspnet-core/
│   ├── ABPGroup.sln
│   ├── build/
│   ├── docker/
│   │   └── ng/
│   ├── src/
│   │   ├── ABPGroup.Application/
│   │   ├── ABPGroup.Core/
│   │   ├── ABPGroup.EntityFrameworkCore/
│   │   ├── ABPGroup.Migrator/
│   │   ├── ABPGroup.Web.Core/
│   │   └── ABPGroup.Web.Host/
│   └── test/
│       ├── ABPGroup.Tests/
│       └── ABPGroup.Web.Tests/
├── frontend/
│   ├── .agents/skills/
│   ├── public/
│   └── src/app/
├── _screenshots/
├── LICENSE
└── README.md
```

## API Integration

**Base URL**: `https://localhost:44311/`

| Module | Base Path |
| --- | --- |
| Token Auth | `/api/TokenAuth/Authenticate` (POST) |
| Account | `/api/services/app/Account/IsTenantAvailable` (POST) |

## State Management

No custom state management or providers were found. The frontend currently uses the default Next.js setup.

## Getting Started

### Prerequisites

- .NET SDK 9.x
- Node.js (for the Next.js frontend)
- PostgreSQL (default) or SQL Server (alternative provider is included)

### Installation

```bash
# 1. Clone
git clone <repository-url>
cd abp-group

# 2. Backend restore
dotnet restore aspnet-core/ABPGroup.sln

# 3. Frontend install
cd frontend
npm install
```

### Environment Variables

No `.env.example` files were found. Configure backend settings in `appsettings.json`:

| Variable | Description | Required |
| --- | --- | :---: |
| `ConnectionStrings:Default` | Database connection string (default is PostgreSQL) | Yes |
| `App:ServerRootAddress` | Backend base URL | Yes |
| `App:ClientRootAddress` | Frontend base URL | Yes |

Frontend environment variables: none detected (placeholder for future additions).

### Running Locally

```bash
# Backend
dotnet run --project aspnet-core/src/ABPGroup.Web.Host/ABPGroup.Web.Host.csproj

# Optional: run migrator to apply migrations
dotnet run --project aspnet-core/src/ABPGroup.Migrator/ABPGroup.Migrator.csproj

# Frontend (from frontend/)
npm run dev
```

### Test Credentials

```
Host Admin:
  Email: admin@aspnetboilerplate.com
  Password: 123qwe

Tenant Admin:
  Email: admin@defaulttenant.com
  Password: 123qwe
```

## Building for Production

```bash
# Backend
dotnet publish aspnet-core/src/ABPGroup.Web.Host/ABPGroup.Web.Host.csproj -c Release

# Frontend
cd frontend
npm run build
npm run start
```

## License

MIT
