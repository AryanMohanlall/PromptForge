# PromptForge: AI Platform for Generating and Deploying Full-Stack Applications

**PromptForge** is an intelligent orchestration platform that bridges the gap between natural language ideas and live, running applications. By leveraging a high-performance backend (ASP.NET Core/ABP) and a modern frontend (Next.js), it automates the entire software creation lifecycle: from initial scaffolding to automated cloud deployment.

---

## 1. Overview & Goal
Developers and startups often face significant friction when moving from an idea to a functional MVP. PromptForge eliminates this manual overhead. 

**The Goal:** Build a platform that:
* Accepts **natural language** app requests.
* Generates a structured **full-stack application**.
* Automates **GitHub repository** creation and commits.
* Triggers **automatic deployment** pipelines.
* Returns a **live URL** to the user instantly.

---

## 2. Design & Documentation

| Resource | Link |
| :--- | :--- |
| **Figma Design** | [View UI/UX Designs](https://www.figma.com/design/ztpFt2jwUETAhxCnkDM4xs/PromptForge?node-id=0-1&t=HZx01yfot5KoMsKj-1) |
| **Domain Model** | [View Domain Model](https://lucid.app/lucidchart/a7f4c7cf-b4cb-4bea-9c13-91cf348e0c52/edit?invitationId=inv_6141d060-2270-4da9-b417-a3dd1cf3d611&page=0_0#) |view?usp=sharing) |
| **Live Demo** | https://abp-group.vercel.app/ |

---

## 3. Core Use Cases

### 👤 Product Builder / Founder
* **Sign in** with GitHub.
* **Define requirements** using a natural language prompt.
* **Choose templates** and trigger generation.
* **Access the live URL** once the automated build completes.

### 💻 Developer
* **View history** of generated versions.
* **Inspect commits** and architecture summaries.
* **Refine apps** by providing follow-up prompts to the AI engine.

### ⚙️ Administrator
* **Manage templates** and deployment settings.
* **Monitor logs** and handle failed build queues.
* **Track usage** and platform health.

---

## 4. Technical Specifications

### 4.1 Tech Stack
| Component | Technology | Purpose |
| :--- | :--- | :--- |
| **Backend** | .NET 9.0 / ABP Framework | Modular API, Multi-tenancy, & Auth |
| **Frontend** | Next.js 16 / React 19 | User dashboard & Prompt interface |
| **Database** | PostgreSQL / EF Core 9 | Relational data & Job tracking |
| **AI Engine** | LLM Integration | Prompt parsing & Code generation |
| **VCS** | GitHub API | Repository & Version management |

### 4.2 Suggested Database Entities
To support the workflow, the system manages the following:
* **Identity:** `Users`, `GitProfiles`
* **Orchestration:** `AppRequests`, `PromptSessions`, `GeneratedProjects`
* **Infrastructure:** `Repositories`, `BuildJobs`, `Deployments`, `DeploymentLogs`
* **Assets:** `Templates`, `GeneratedArtifacts`

---

## 5. Functional Requirements

### 🛠 Generation Engine
* Map natural language requirements to template capabilities.
* Create frontend, backend, and database structures.
* Generate project-specific `README` files and environment placeholders.
* Validate file structure integrity before pushing to Git.

### 🚀 GitHub & Deployment Pipeline
* **Authentication:** Secure OAuth flow for GitHub.
* **Automation:** Create repositories, manage branches, and push code.
* **Deployment:** Trigger CI/CD workflows and track real-time status/logs.
* **Feedback:** Present a "Success" page with the final application URL.

---

## 6. Project Structure
```text
.
├── aspnet-core/               # ABP Backend (C#)
│   ├── src/
│   │   ├── PromptForge.Application/   # Logic for Prompt Parsing
│   │   ├── PromptForge.Core/          # Domain Entities (Jobs, Projects)
│   │   ├── PromptForge.EntityFrameworkCore/ # Database Migrations
│   │   └── PromptForge.Web.Host/      # API Endpoints
├── frontend/                  # Next.js App (TypeScript)
│   ├── src/app/               # Prompt UI & Dashboard
│   └── public/                # Static Assets
├── _screenshots/              # UI Previews
└── README.md
```

## 7. Getting Started

### Prerequisites
* **.NET SDK 9.x**
* **Node.js** (v20+)
* **PostgreSQL**

### Installation

```bash
# 1. Clone the platform
git clone <repository-url>
cd PromptForge

# 2. Setup Backend
dotnet restore aspnet-core/PromptForge.sln

# 3. Setup Frontend
cd frontend
npm install
```

### Running Locally
To get the platform up and running in a development environment, follow these steps:

1. Apply Database Migrations
Before starting the API, ensure your PostgreSQL database is initialized and schema migrations are applied:
```bash
dotnet run --project aspnet-core/src/PromptForge.Migrator/PromptForge.Migrator.csproj
```
2. Start the Backend API
Run the host project to start the Swagger UI and REST endpoints:
```bash
dotnet run --project aspnet-core/src/PromptForge.Web.Host/PromptForge.Web.Host.csproj
```
3. Start the Frontend Dashboard
Open a new terminal window and start the Next.js development server:
```bash
cd frontend
npm run dev
```
