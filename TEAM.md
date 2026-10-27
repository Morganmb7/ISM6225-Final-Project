# Team Roles and Responsibilities

| Team Member | Role | Responsibilities | Deliverables |
|---|---|---|---|
| Ignatious | Data & Analytics Lead | Design SQLite schema; create at least two related tables and 25 synthetic records; implement database initialization and parameterized C# repository queries; verify analytical results. | Seeded database, ERD, repository code, known-answer results, database initialization evidence. |
| Morgan | MCP & Backend Lead | Build four HTTP MCP tools: lookup, filtered search, aggregate, and comparison/ranking; implement validation and output limits; connect chatbot through runtime tool discovery; add system prompt, fallback mode, and tool-use logs. | Working MCP endpoint, chatbot integration, tool documentation, discovery/invocation logs, no-result and failure-handling evidence. |
| Joel | MVC & Experience Lead | Build controllers, view models, Razor views, chat page, and About/Architecture page; add conversation history, responsive styling, accessibility, server-side validation, anti-forgery protection, friendly errors, and mode indicators. | Working MVC interface, chat service connection, architecture page, validation screenshots, usability review. |
| David | Cloud, Testing & Documentation Lead | Deploy to Azure; configure HTTPS, AllowedHosts, environment variables, and persistent SQLite storage; coordinate testing, evidence collection, report assembly, README, and rehearsal. | Working Azure URL, deployment instructions, evaluation matrix, evidence appendix, assembled report, restart-persistence evidence. |

## Testing Ownership

| Owner | Assigned Tests |
|---|---|
| Ignatious | A2: Database initialization and seed counts. A4: Known correct results for all four tools. Support A7 with before/after database checks. |
| Morgan | A3: Runtime MCP tool discovery. A6: Honest no-result response. Backend portions of A7, prompt-injection testing, and model/tool failure handling. |
| Joel | A8: Invalid form input. Long-input handling, conversation history, mode indicators, responsive layout, and friendly errors. |
| David | A1: Release build. Coordinate A5: Ten scripted questions. A9: Azure results. A10: Restart persistence. Assemble test evidence. |

## Project Checkpoints

| Stage | Tasks | Owner |
|---|---|---|
| Planning | Agree on business domain, audience, schema, ten business questions, four tool contracts, and chat service interface. | Everyone |
| Initial Development | Build database/repository, MCP tools, MVC interface, and an early Azure deployment. | Each role owner |
| Local Integration | Complete the request path from chat page through MCP to SQLite and back. | Everyone |
| Feature Complete | Finish all four tools, model/fallback behavior, interface, tests, and draft evidence. | Each role owner |
| Final Verification | Run local and Azure tests, verify restart persistence, finish documentation, and rehearse at least 48 hours before presenting. | Everyone; David coordinates |

## Code Review Assignments

| Author | Reviewer | Review Focus |
|---|---|---|
| Ignatious | Morgan | Repository methods, query parameters, and tool compatibility. |
| Morgan | Ignatious | Tool results, data grounding, and analytical correctness. |
| Joel | David | MVC validation, error handling, and deployment compatibility. |
| David | Joel | Deployment configuration, reproducible setup, and test coverage. |

## Presentation Responsibilities

| Team Member | Segment | Target Time |
|---|---|---|
| Joel | Business problem, audience, and user experience. | 1 minute |
| Ignatious | Database, schema, and analytical correctness. | 2 minutes |
| Morgan | MCP/chatbot architecture and live demonstration. | 4 minutes |
| David | Testing, AI revision story, deployment, limitations, and next improvement. | 3 minutes |

## Shared Requirements

- Every member must author or meaningfully review C# code and make visible GitHub commits.
- Everyone must understand the full request path and explain one code file they did not author.
- Each member writes their technical report section and supplies supporting screenshots/logs.
- Each member contributes at least two consequential AI-use entries, including evaluation and revision evidence.
- The team documents at least one AI suggestion that compiled but required correction.
- Everyone contributes to the one-page contribution record and confirms teammates' contributions.
- Everyone participates in the final rehearsal and prepares a local backup demo.

