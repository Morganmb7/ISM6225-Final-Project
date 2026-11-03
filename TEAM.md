# Team Plan: Affiliate Campaign Performance Assistant

## Project Overview

Build an ASP.NET Core MVC chatbot that helps marketing managers
decide which affiliate campaigns to repeat and which affiliates
need additional support.

The application uses synthetic data stored in SQLite and retrieves
verified results through four HTTP MCP tools.

Example questions the assistant must answer:

- Which affiliate generated the most net revenue?
- Which affiliates participated in a campaign but made no sales?
- How much revenue and commission expense did a campaign generate?
- Show me affiliate 104.

## Starting Point: The CampusDataChat Framework

The `CampusDataChat` folder on the `add-campus-data-chat` branch is a
working app from a different domain (university courses). Its
architecture is exactly what we need, so we **keep the plumbing and
replace the domain**.

**Keep as-is (already working):**

- Single MVC app that also hosts a stateless HTTP MCP server at `/mcp` (`Program.cs`)
- MCP client with runtime tool discovery, OpenAI mode, and keyword-based demo/fallback mode (`Services/ChatOrchestrator.cs`)
- Chat controller with session history, anti-forgery, validation, and friendly errors (`Controllers/ChatController.cs`)
- Azure-aware SQLite path resolution (`Data/DatabasePath.cs`)
- Layout, styling, and chat page structure

**Replace (course domain → affiliate domain):**

| Current file | Becomes | Owner |
|---|---|---|
| `Models/Course.cs` | `Models/Affiliate.cs`, `Campaign.cs`, `CampaignSummary.cs`, `AffiliateRanking.cs`, `ParticipantWithoutSales.cs` | Ignatious |
| `Data/DatabaseInitializer.cs` (1 table, 8 rows) | 4 tables with PK/FK + synthetic seed (25+ rows) | Ignatious |
| `Data/SqliteCourseRepository.cs` | `Data/SqliteAffiliateRepository.cs` | Ignatious |
| `App_Data/campus.db` | `App_Data/affiliates.db` (delete the old file; it is regenerated at startup) | Ignatious |
| `Mcp/CourseTools.cs` (4 course tools) | `Mcp/AffiliateTools.cs` (4 affiliate tools) | Morgan |
| `ChatOrchestrator.cs` system prompt, demo routing, `CourseCodeRegex` | Affiliate system prompt, affiliate keyword routing, ID/campaign extraction | Morgan |
| `Program.cs` repository registration | Register `SqliteAffiliateRepository` | Morgan |
| `Views/Chat/Index.cshtml`, `Models/ChatViewModel.cs` | Affiliate headings, suggested questions, tool list, input label | Joel |
| `Views/Home/Index.cshtml`, `_Layout.cshtml` | About/Architecture page and branding for the affiliate assistant | Joel |
| `README.md`, `appsettings.json`, `.gitignore` | Setup/run/deploy instructions, Azure hosts and secrets, ignore fixes | David |

> **Do first (Stage 0):** Morgan renames the project/namespace from
> `CampusDataChat` to `AffiliateDataChat` (folder, `.csproj`, `.slnx`,
> namespaces) in one commit and merges it **before** anyone else starts
> editing, so we avoid merge conflicts across every file.

## Team Responsibilities

| Member | Role | Tasks | Deliverables |
|---|---|---|---|
| Ignatious | Data & Analytics Lead | Rewrite `DatabaseInitializer` to create Affiliates, Campaigns, AffiliateCampaigns, and AffiliateSales with PK/FK constraints (`PRAGMA foreign_keys = ON`). Seed at least 25 meaningful synthetic records, including affiliates with no sales, affiliates in multiple campaigns, a fully refunded sale, and different commission rates. Replace `SqliteCourseRepository` with `SqliteAffiliateRepository` using parameterized queries (follow the existing `$param` pattern). Create the result models. Hand-calculate expected results for testing. | SQLite schema and seed code, ERD, repository methods matching the interface below, expected-results sheet. |
| Morgan | MCP & Backend Lead | Stage 0 rename. Replace `CourseTools` with `AffiliateTools` exposing the four tools below, keeping the existing pattern: `[Description]` on every tool/parameter, input validation, clamped result limits, JSON output, and clear "not found" messages. Update the OpenAI system prompt for the affiliate domain and the calculation rules. Rewrite demo-mode routing so each example question reaches the right tool without an API key. Add tool-use logging. | Working `/mcp` endpoint with four documented tools, updated chatbot service, discovery and invocation evidence (logs/screenshots). |
| Joel | MVC & Experience Lead | Rebrand the layout and chat page to "Affiliate Campaign Assistant." Replace the course suggestions with clickable marketing questions, update the tool list in the sidebar, and change the input label/placeholder. Rewrite the Home page as an About/Architecture page (MVC → ChatOrchestrator → MCP → repository → SQLite). Show which MCP tool was used and whether the app is in OpenAI or demo mode. Check responsive layout and error messages. | Polished affiliate-themed MVC interface, architecture page, validation/usability evidence. |
| David | Cloud, Testing & Documentation Lead | Deploy to Azure App Service. Set `AllowedHosts` for the Azure domain, store `Ai:ApiKey` in App Service settings (never in Git), set `Mcp:Endpoint` to the deployed `/mcp` URL, and confirm the database persists under `HOME/data` after restart. Fix `.gitignore` (it lists `.appmod/appcat/` but the committed folder is `.appmod/.appcat/`). Write the README. Coordinate acceptance tests, the ten-question evaluation matrix, screenshots/logs, report assembly, and rehearsal. | Working Azure URL, deployment instructions, test matrix, evidence appendix, assembled report, restart-persistence evidence. |

## Database Plan

| Table | Purpose | Fields |
|---|---|---|
| Affiliates | Synthetic affiliate profiles. | AffiliateId (PK), Name, ContentCategory, JoinDate |
| Campaigns | Campaign details. | CampaignId (PK), CampaignName, StartDate, EndDate |
| AffiliateCampaigns | Which affiliates participated in each campaign, including those with no sales. | AffiliateCampaignId (PK), AffiliateId (FK), CampaignId (FK), CommissionRate; UNIQUE(AffiliateId, CampaignId) |
| AffiliateSales | Sales attributed to an affiliate's campaign participation. | SaleId (PK), AffiliateCampaignId (FK), SaleDate, GrossRevenue, DiscountAmount, RefundAmount, CommissionExpense |

Use affiliate IDs starting at 101 so "Show me affiliate 104" works.
The database must contain at least 25 meaningful records across the
four tables.

## Repository ↔ Tool Contract

Ignatious and Morgan agree on these signatures first so both can work
in parallel. Morgan can stub them with fake data until the real
queries land.

| Repository method (Ignatious) | MCP tool (Morgan) | Example question |
|---|---|---|
| `GetAffiliateByIdAsync(int affiliateId)` → affiliate profile + campaigns joined + totals | `lookup_affiliate` | Show me affiliate 104. |
| `FindParticipantsWithoutSalesAsync(int? campaignId, string? campaignName)` | `find_affiliates_without_sales` | Which affiliates joined the Summer campaign but made no sales? |
| `SummarizeCampaignAsync(int? campaignId, string? campaignName)` → sale count, gross, discounts, refunds, net revenue, commission, revenue after commission | `summarize_campaign` | How much revenue and commission expense did campaign 3 generate? |
| `RankAffiliatesAsync(int? campaignId, DateOnly? from, DateOnly? to, int limit)` (limit clamped 1–10) | `rank_affiliates` | Which affiliate generated the most net revenue? |

## Calculation Rules

| Measure | Definition |
|---|---|
| Net Revenue | Gross Revenue − Discount Amount − Refund Amount |
| Commission Expense | Recorded `CommissionExpense` on each sale (seed it as net revenue × the participation's CommissionRate). |
| Revenue After Commission | Net Revenue − Commission Expense. This is not profit; other costs are excluded. |
| Participant Without Sales | An AffiliateCampaigns row with no related AffiliateSales rows. |

A fully refunded sale still counts as a recorded sale.
Zero sales and zero net revenue must be treated separately.

## Testing Responsibilities

| Member | Tests and Evidence |
|---|---|
| Ignatious | Seed counts, FK relationships, campaign totals, affiliate rankings, and participants without sales match the hand-calculated expected results. |
| Morgan | Runtime discovery of all four tools, validation and output limits, grounded answers, missing-data responses, prompt injection, demo-mode routing for each example question, and model/tool failure handling. |
| Joel | Invalid/long input, anti-forgery protection, conversation history, mode indicator, tool evidence, responsive layout, and friendly errors. |
| David | Release build, Azure deployment, local/cloud answer consistency, database persistence after restart, and all A1–A10 acceptance evidence. |
| Everyone | Contribute to the ten-question evaluation and confirm SQL-like input does not alter the database. |

## Work Sequence

| Stage | Work | Owner |
|---|---|---|
| 0. Rename | Rename project/namespace to `AffiliateDataChat`, merge before other work. | Morgan |
| 1. Agree on Design | Finalize schema, calculation rules, the repository ↔ tool contract, and ten test questions. | Everyone |
| 2. Convert Components | Data layer (Ignatious), tools and orchestrator (Morgan), views (Joel), early Azure deploy of the renamed app (David). Each on its own branch. | Each role owner |
| 3. Connect One Complete Flow | Ask "Show me affiliate 104" in the UI → `lookup_affiliate` → SQLite → grounded answer, locally and on Azure. | Everyone |
| 4. Finish Features | Remaining tools, demo/OpenAI behavior, UI polish, security controls, draft evidence. | Each role owner |
| 5. Test and Submit | Local/Azure tests, report, AI-use record, contribution record, rehearsal. | Everyone; David coordinates |

## Code Reviews

| Author | Reviewer | Review Focus |
|---|---|---|
| Ignatious | Morgan | Repository methods, query parameters, and contract compatibility. |
| Morgan | Ignatious | Tool results, calculations, and data grounding. |
| Joel | David | Validation, errors, and deployment compatibility. |
| David | Joel | Deployment configuration and reproducible setup. |

## Shared Responsibilities

- Everyone makes visible GitHub commits and authors or meaningfully reviews C# code.
- No leftover course references: before submitting, search the repo for `Course`, `campus`, and `enrollment`.
- Everyone writes their own report section and supplies test evidence.
- Each member contributes at least two consequential AI-use records.
- The team documents one AI suggestion that compiled but required correction.
- Document at least two limitations and one responsible-AI consideration.
- Campaign rankings describe the synthetic data; they do not prove why an affiliate performed well or poorly.
- Everyone speaks during the presentation and can explain the full architecture.
- Complete a one-page contribution record with commit references and peer confirmation.
- Rehearse and prepare a local backup demo at least 48 hours before presenting.
