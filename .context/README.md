# PromptForge — Agent Context

You are working on **PromptForge**, an AI-powered SaaS platform that accepts a
natural-language prompt and produces a fully deployed full-stack application with
a live URL.

## Read these files in order before doing anything

1. `project.md`          — what we are building, MVP scope, stretch features
2. `domain-model.md`     — bounded contexts, aggregates, ubiquitous language
3. `architecture.md`     — Clean Architecture layers, ABP setup, project structure
4. `roles-and-tenancy.md`— Host vs Tenant, the three roles, tenancy decisions
5. `decisions.md`        — ADRs — every settled decision (do not re-open)
6. `code-style.md`       — naming conventions, patterns to follow and avoid

## Ground rules

- Never contradict `decisions.md` without explicitly flagging it as a proposed
  change and asking for confirmation.
- When working in `aspnet-core/`, check `aspnet-core/.agent/skills/` for a
  relevant skill before writing any code.
- When working in `frontend/`, check `frontend/.agent/skills/` for a relevant
  skill before writing any code.
- The ubiquitous language in `domain-model.md` is law — use those exact terms
  everywhere: in class names, variable names, API routes, UI labels, and comments.
- The project namespace is `PromptForge` — never use `ABPGroup`.
