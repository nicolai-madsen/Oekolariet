# ADR-0001: Generic content model (theme is data, mechanics are code)

**Status:** Proposed


## Context

Økolariet wants the system to be reusable for other exhibitions, and all text and images must be editable through a CMS.


## Decision

The domain models **mechanics** (pages, guide, sorting game), not **themes** (waste, Otto Odder). All theme-specific content is stored as data in the database. The presentation consists of generic pages with text, an image and an order.


## Alternatives considered

- **Theme-specific classes** such as `WasteItem` or `WelcomePage`. Rejected because every new exhibition would require code changes.


## Consequences

- A new exhibition only needs new content, not new code.
- The CMS has to handle more, and the model is somewhat more abstract.
