# ADR-0005: Blazor WebAssembly for the GuestClient

**Status:** Proposed


## Context

The touchscreen runs all day. The sorting game needs smooth drag-and-drop. Nobody reloads the screen manually.


## Decision

The GuestClient is built with Blazor WebAssembly. After a period without touch, the screen resets to the welcome page and fetches the latest published version.


## Alternatives considered

- **InteractiveServer.** Every movement would go over the network, and a deploy or a network outage would freeze the screen in the middle of the game.


## Consequences

- The load time is only paid once, in the morning.
- The client is thick, so nothing sensitive may be in it (See ADR-0003).
- The client keeps running during deploy.
