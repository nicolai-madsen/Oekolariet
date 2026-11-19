# ADR-0003: The server validates answers and calculates the score

**Status:** Proposed


## Context

The client runs in the browser, where it can be inspected and manipulated. The API may be reachable from machines other than the touchscreen. 
The statistics across all guests must be trustworthy. 


## Desicion

Correct answers are never sent to the client. When a guest starts a game, the server creates a game session bound to the active version. 
The client only sends the session id, thhe item and the chosen category. The server decides wether the answer is correct and calculates the score.


## Alternatives considered

- **Correct answers in the client** for faster feedbck. 
Rejected because the requirements for smooth interaction only applies to the drag and drop functionally, which happens locally. 
Validation is a single short request.


## Consequences

- Protects against cheating and manipulation of the highscore (OWASP: Insecure design).
- One extra network request per answer.
