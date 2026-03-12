# AGENTS.md

## Project purpose

This repository contains a .NET application.
Prefer minimal, safe, production-minded changes that fit the existing architecture and coding style.

## Working style

- Before changing code, inspect the relevant project structure, dependencies, and call flow.
- Prefer small, high-confidence edits over broad rewrites.
- Preserve existing public behavior unless the task explicitly asks for a breaking change.
- Do not rename files, classes, methods, or APIs without a clear reason.
- Avoid speculative refactors unrelated to the requested task.
- When multiple approaches are possible, choose the one with the lowest blast radius.

## Build and test

- Restore with: `dotnet restore`
- Build with: `dotnet build`
- Run tests with: `dotnet test`
- If the repo contains multiple solutions or test projects, identify the most relevant one before running commands.
- After code changes, build the affected projects and run the nearest relevant tests.
- If tests cannot be run, explain why clearly.

## C# coding rules

- Target the existing .NET and C# version already used by the repository.
- Follow SOLID principles.
- Prefer composition over inheritance unless inheritance is already the established pattern.
- Prefer dependency injection over service location.
- Use constructor injection for required dependencies.
- Keep methods focused and cohesive.
- Favor clear names over short names.
- Avoid static mutable state.
- Avoid unnecessary allocations in hot paths.
- Use `async`/`await` correctly; do not block on async code with `.Result` or `.Wait()`.
- Pass `CancellationToken` through async flows where appropriate.
- Respect nullable reference types if enabled.
- Do not suppress warnings without a strong reason.
- Prefer immutable DTOs/value objects when practical.
- Use `var` when the type is obvious from the right-hand side; otherwise prefer explicit types for readability.

## ASP.NET Core rules

- Keep endpoint handlers thin.
- Put business logic in services, not controllers/endpoints.
- Keep DI registrations consistent with existing patterns.
- Validate inputs at the boundary.
- Return appropriate HTTP status codes.
- Do not leak internal exception details to clients.
- Preserve middleware ordering unless the task requires changing it.
- For background work, prefer hosted services or existing infrastructure over ad hoc thread usage.

## Data access rules

- Keep queries scoped to the task.
- Avoid N+1 query patterns.
- Preserve transaction boundaries unless explicitly changing them.
- If using EF Core, prefer projections for read models where appropriate.
- Do not introduce breaking schema changes unless explicitly requested.
- For SQL changes, keep scripts idempotent where possible.

## Concurrency and performance rules

- Treat shared mutable state as dangerous.
- Check thread-safety when modifying caches, collections, timers, channels, or singleton services.
- For async streams, channels, SSE, WebSockets, and background publishers, review backpressure, cancellation, and
  disposal behavior.
- Avoid sync-over-async and unnecessary locking.
- If a change may affect performance, mention the likely tradeoff.

## Logging and diagnostics

- Preserve structured logging.
- Do not log secrets, tokens, passwords, or connection strings.
- Prefer actionable log messages with relevant identifiers.
- Add diagnostics only when useful and keep noise low.

## Security rules

- Never hardcode secrets.
- Do not weaken authentication or authorization.
- Preserve validation and permission checks.
- Treat external input as untrusted.
- Flag any obvious security risks discovered while working.

## Testing guidance

- Prefer targeted automated tests for the changed behavior.
- Add or update tests when fixing bugs or changing logic.
- Keep tests deterministic.
- Do not rewrite unrelated tests.
- For concurrency-sensitive code, prefer focused tests around cancellation, timing boundaries, and repeated execution
  where feasible.

## Output expectations

- Summarize what changed and why.
- Mention any assumptions.
- Mention validation steps performed.
- Mention risks, limitations, or follow-up work if relevant.