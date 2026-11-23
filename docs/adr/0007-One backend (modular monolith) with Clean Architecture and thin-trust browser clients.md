# ADR-0007: One backend (modular monolith) with Clean Architecture and thin-trust browser clients

**Status:** Accepted


## Context

Økolariet has two kinds of users with vastly different trust levels: anonymous guests at a touchscreen, and staff who edit content from a PC.
Both need the same data (exhibitions, versions, images, statistics).

Some use cases change several aggregates at once. Publishing a draft creates an `ExhibitionVersion`, moves `Exhibition.ActiveVersionId` and removes the `Draft`. This must happen in one transaction (ADR-0006).

The OpenAPI specification already groups the endpoints by prefix (`/auth`, `/exhibitions`, `/media`, `/games`, `/statistics`), so a later split into services is possible.


## Decision

1. **One backend, deployed as one container:** `Oekolariet.Api` (ASP.NET Core Minimal API). Inside it, the code is split by Clean Architecture:
   - `Domain`: aggregates and invariants. No dependencies.
   - `Application`: use cases organised by feature, plus the interfaces they need (repositories, Unit of Work). No HTTP, no EF Core.
   - `Infrastructure`: EF Core, repositories, Unit of Work. Implements the interfaces from `Application`.
   - `Api`: endpoints, authentication, CORS, mapping of exceptions to ProblemDetails. Composition root.
2. **One SQL Server database** with one `DbContext`.
3. **Two browser clients** (GuestClient and CMS) built with Blazor WebAssembly. They talk to the API only through REST/JSON on `/api/v1` and reference only `Oekolariet.Shared` (DTOs).
4. **The server is authoritative.** All business rules, validation, scoring, authorisation, concurrency control and server-set fields live in the backend. The clients only handle presentation and interaction.
5. **Feature boundaries are kept visible** (feature folders in `Application`, URL prefixes in the API), so a feature can later be moved into its own service.


## Alternatives considered

- **Microservices** (AuthService, ContentService, GameService behind a YARP gateway, possibly with Dapr/Aspire).
  Rejected for now. The game needs the correct answers of the active version, which belong to the content side. That means a network call or copied data between services. Each service needs its own deploy, database and monitoring. With one small team that owns everything, the boundaries would not match the organisation (Conway's law), and the cost would take time from the features the exam is about.
- **Classic N-layer** (UI → Business → Data access, with the business layer depending on the data layer).
  Rejected. The business logic would depend on EF Core, which makes it harder to unit test and breaks the Dependency Inversion Principle. In Clean Architecture the domain can be tested with no database at all.
- **One server-rendered app without a separate API** (Blazor Server / InteractiveServer or Razor Pages calling the database directly).
  Rejected. The exam requires a REST API. Two clients share the same data. A network drop or a deploy would freeze the touchscreen in the middle of a game (ADR-0005).
- **Business logic in the client** (e.g. checking answers in the browser for instant feedback).
  Rejected. Browser code can be inspected and changed, so the highscore could be faked (ADR-0003).
- **A separate database per feature.**
  Rejected together with microservices. It would remove the single transaction that publishing relies on.


## Consequences

**Positive**
- Use cases that touch several aggregates run in one database transaction. No distributed transactions.
- Simple to run: `docker compose up` starts `db`, `api` and `guestclient`.
- `Domain` and `Application` can be unit tested without a database or HTTP.
- The OpenAPI contract and `Shared` DTOs decouple the clients from the backend, so frontend and backend can be built in parallel.
- Sensitive logic and secrets stay on the server.

**Negative**
- Everything is deployed and scaled together. A bad deploy affects both clients. (The GuestClient keeps running in the browser during a deploy, ADR-0005.)
- Module boundaries inside the backend are only kept by discipline. Nothing stops a game handler from using a content repository directly. Architecture tests (e.g. NetArchTest) could enforce this later.
- Every client is a separate origin, so the API must be configured with CORS per client.
- An extra network request per answer in the game, because the server validates it.
- Moving to microservices later is possible, but will require splitting the database and replacing in-process calls with network calls.
