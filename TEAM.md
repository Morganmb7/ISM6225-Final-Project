# Team Plan: Affiliate Campaign Performance Assistant

## Project Overview

Build an ASP.NET Core MVC chatbot that helps marketing managers
decide which affiliate campaigns to repeat and which affiliates
need additional support.

The application uses synthetic data stored in SQLite and answers
only from verified results retrieved through eight HTTP MCP tools.
A question flows from the chat page (with an anti-forgery token)
to the ChatController, then to the chatbot service, where the
model chooses tools (or fallback rules apply if no model is
available). The MCP client calls the HTTP MCP server, which runs
parameterized repository queries against SQLite. The answer
returns the same way, with each tool call shown as evidence.

## Team Responsibilities

| Member | Role | Tasks | Deliverables |
|---|---|---|---|
| Ignatious | Data & Analytics Lead | Design the Affiliates, Campaigns, AffiliateCampaigns, and AffiliateSales tables. Define primary/foreign keys and seed at least 25 meaningful synthetic records. Build parameterized C# repository queries. Include affiliates with no sales, multiple campaigns, and different commission amounts. Verify revenue and commission calculations. | SQLite database, seed code, ERD, repository methods, and expected results for testing. |
| Morgan | MCP & Backend Lead | Build the eight MCP tools: campaign list, affiliate lookup, affiliate campaign history, campaign participants without sales, campaign summary, campaign ranking, affiliate ranking, and affiliate sales ranking. Add tool descriptions, parameter validation, and output limits. Implement the MCP client, runtime discovery, model integration, system prompt, fallback mode, and tool-use logs. | Working HTTP MCP endpoint, chatbot service, eight documented tools, and discovery/invocation evidence. |
| Joel | MVC & Experience Lead | Build the chat page and About & Architecture page. Implement controllers, view models, conversation history, suggested marketing questions, responsive styling, input validation (3–500 characters), anti-forgery protection, rate limiting, and friendly errors. Display tool evidence and model/fallback mode. | Polished MVC interface connected to the chatbot service, architecture page, and validation/usability evidence. |
| David | Cloud, Testing & Documentation Lead | Deploy the application to Azure. Configure HTTPS, AllowedHosts, secrets, and persistent SQLite storage. Coordinate acceptance tests, the ten-question evaluation matrix, screenshots/logs, README, report assembly, and presentation rehearsal. | Working Azure URL, deployment instructions, test matrix, evidence appendix, assembled report, and restart-persistence evidence. |

## Database Plan

| Table | Purpose | Fields |
|---|---|---|
| Affiliates | Stores synthetic affiliate profiles. | AffiliateId (PK), Name, ContentCategory, JoinDate |
| Campaigns | Stores campaign names and dates. | CampaignId (PK), CampaignName, StartDate, EndDate |
| AffiliateCampaigns | Records who took part in each campaign, including those with no sales, with commission rate. | AffiliateCampaignId (PK), AffiliateId (FK), CampaignId (FK), CommissionRate |
| AffiliateSales | Stores sales attributed to an affiliate's campaign participation. | SaleId (PK), AffiliateCampaignId (FK), SaleDate, GrossRevenue, DiscountAmount, RefundAmount, CommissionExpense |

Each affiliate/campaign pair appears only once in
AffiliateCampaigns. The database contains at least 25
meaningful records total across the four tables and persists
across app restarts.

## MCP Tool Plan

| Tool | Purpose | Example Question | Owner |
|---|---|---|---|
| list_campaigns | Available campaigns with dates, participation, and sales counts. | Which campaigns are available? | Morgan |
| affiliate_lookup | Exact lookup of an affiliate by ID. | Show me affiliate 104. | Morgan |
| get_affiliate_campaign_history | Sales and commission performance for an affiliate across campaigns. | Show affiliate 104's campaign history. | Morgan |
| campaign_participants_without_sales | Campaign participants with no recorded sales, optionally for one campaign. | Which affiliates participated in the summer campaign but made no sales? | Morgan |
| campaign_summary | Campaign revenue, commission expense, and sales counts (one aggregate record). | How much revenue and commission expense did campaign 3 generate? | Morgan |
| campaign_ranking | Campaigns ranked by recorded sales count. | Which campaign made the most sales? | Morgan |
| affiliate_ranking | Affiliates ranked by net revenue for a campaign or date range, with a result limit (one row per affiliate). | Which affiliate generated the most net revenue? | Morgan |
| affiliate_sales_ranking | Affiliates ranked by recorded sales count, optionally for one campaign. | Which affiliate made the most sales? | Morgan |

Ignatious implements the repository queries behind these tools.
Morgan exposes them through MCP and connects them to the chatbot.

## Calculation Rules

| Measure | Definition |
|---|---|
| Net Revenue | Gross Revenue - Discount Amount - Refund Amount |
| Commission Expense | Recorded commission expense for the attributed sales. |
| Revenue After Commission | Net Revenue - Commission Expense; this is not profit because other costs are excluded. |
| Participant Without Sales | An affiliate listed in AffiliateCampaigns with no related AffiliateSales records for that campaign. |

A fully refunded sale still counts as a recorded sale.
Zero sales and zero net revenue are treated separately.

## Testing Responsibilities

| Member | Tests and Evidence |
|---|---|
| Ignatious | Verify seed counts, table relationships, campaign totals, affiliate rankings, and participants without sales against known results. |
| Morgan | Verify runtime discovery of all eight tools, tool validation/output limits, grounded answers, missing-data responses, prompt injection, and model/tool failure handling. |
| Joel | Verify invalid/long input, anti-forgery protection, rate limiting, conversation history, mode indicators, tool evidence, responsive layout, and friendly errors. |
| David | Verify release build, Azure deployment, local/cloud answer consistency, database persistence after restart, and completion of all A1–A10 acceptance evidence. |
| Everyone | Contribute to the ten-question evaluation and verify SQL-like input does not alter the database. |

## Work Sequence

| Stage | Work | Owner |
|---|---|---|
| 1. Agree on Design | Finalize audience, schema, calculation rules, tool parameters/results, and ten test questions. | Everyone |
| 2. Build Components | Create database/repository, MCP/chat service, MVC interface, and early Azure deployment. | Each role owner |
| 3. Connect One Complete Flow | Submit a question through MVC, call an MCP tool, query SQLite, and display a grounded answer. | Everyone |
| 4. Finish Features | Complete all tools, model/fallback behavior, UI, security controls, and draft evidence. | Each role owner |
| 5. Test and Submit | Complete local/Azure tests, report, AI-use record, contribution record, and rehearsal. | Everyone; David coordinates |

## Code Reviews

| Author | Reviewer | Review Focus |
|---|---|---|
| Ignatious | Morgan | Repository methods, query parameters, and tool compatibility. |
| Morgan | Ignatious | Tool results, calculations, and data grounding. |
| Joel | David | Validation, errors, and deployment compatibility. |
| David | Joel | Deployment configuration and reproducible setup. |

## Shared Responsibilities

- Everyone makes visible GitHub commits and authors or meaningfully reviews C# code.
- Everyone writes their own report section and supplies test evidence.
- Each member contributes at least two consequential AI-use records.
- The team documents one AI suggestion that compiled but required correction.
- Document at least two limitations and one responsible-AI consideration:
  - All data is synthetic; campaign rankings describe the data and do not prove why an affiliate performed well or poorly.
  - The assistant only answers what the tools cover; other questions get a "can't answer" response rather than a guess.
  - Conversation history is lost when the app restarts (the database itself persists).
- Everyone speaks during the presentation and can explain the full architecture.
- Rehearse and prepare a local backup demo at least 48 hours before presenting.
