# Project: PromptForge

## Problem statement

Developers and startups want to move from an idea to a running application
quickly. PromptForge bridges that gap — a user describes what they want to build
in plain English, and the platform handles the rest: code generation, GitHub
commit, deployment, and a live URL back to the user.

## Core flow

```
prompt input
  → parse requirements
  → select template
  → generate full-stack artefacts
  → create/update GitHub repository
  → commit generated code
  → trigger deployment pipeline
  → return live URL + status
```

## Primary users

| User                  | Side   | Description                                      |
|-----------------------|--------|--------------------------------------------------|
| Product Builder/Founder | Tenant | Submits prompts, triggers builds, views live URL |
| Developer             | Tenant | Views history, compares versions, reruns prompts |
| Platform Administrator| Host   | Manages platform — templates, infra, queue       |

See `roles-and-tenancy.md` for the full role model.

## MVP scope (build only this unless explicitly told otherwise)

- GitHub OAuth sign-in (or mocked GitHub integration)
- Prompt input UI with template selection
- Template-based full-stack code generation (frontend + backend + database)
- GitHub repository creation and code push
- Deployment pipeline (simulated or real)
- Live URL and status page
- Traceable job history and status

## Functional requirements

### User input
- Project description prompt
- Project category selection
- Stack / template options
- Deployment target selection
- Repo visibility selection (public / private)

### Generation engine
- Generate frontend, backend, and database scaffold from template
- Generate README
- Generate initial entities and pages
- Generate environment / config placeholders
- Validate generated file structure before committing

### GitHub integration
- Authenticate user via GitHub OAuth
- Create new repo or push to existing repo
- Push generated code with meaningful commit messages
- Optionally create branches per generation version

### Deployment pipeline
- Trigger deployment workflow after successful commit
- Track deployment status in real time
- Surface success / failure logs to user
- Return final application URL on success

### Output review
- Generated architecture summary
- List of generated modules
- Live preview link
- Deployment log and status

## Non-functional requirements

- Secure handling of tokens and secrets (never plain strings — see `decisions.md`)
- Queue-based job tracking with retry handling
- Clear user feedback at every stage of the workflow
- Structured logging throughout

## Stretch features (do NOT build unless explicitly instructed)

- Iterative prompt refinement
- Branch-per-version strategy
- Preview environments
- App quality scoring
- Template marketplace
