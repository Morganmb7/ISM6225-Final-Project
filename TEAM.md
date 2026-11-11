Affiliate Campaign Performance Assistant

An ASP.NET Core MVC chatbot that helps marketing managers decide which affiliate campaigns to repeat and which affiliates need more support. It answers only from verified database results, retrieved through MCP tools over synthetic data stored in SQLite.

Live app: https://chatbot-apps1-dyb2d6f2h5b6fagy.francecentral-01.azurewebsites.net/ (About & Architecture: /Home/About)

Architecture

How a question is answered:

Layer	Responsibility
Browser	Chat page sends the question with an anti-forgery token.
ASP.NET Core MVC	ChatController validates input, keeps conversation history, and shows tool evidence.
Chatbot service	System prompt + model chooses tools; fallback rules apply if the model is unavailable.
MCP client → HTTP MCP server	Tools are discovered at runtime; parameters validated; output limited.
Repository	Parameterized C# queries — user text is never pasted into SQL.
SQLite	Synthetic affiliates, campaigns, participation, and sales.

The answer travels back the same way, with each tool call listed under it as evidence. Campaign summaries return one aggregate record; affiliate rankings return one row per affiliate.

The app runs in model mode (an AI model selects tools) or fallback mode (rule-based tool selection when no model is configured). The current mode, model, and number of discovered tools are shown on the chat and About pages.

MCP Tools

Eight tools are discovered at runtime from the HTTP MCP endpoint.

Tool	Purpose	Example Question
list_campaigns	Available campaigns with dates, participation, and sales counts.	Which campaigns are available?
affiliate_lookup	Exact lookup of an affiliate by ID.	Show me affiliate 104.
get_affiliate_campaign_history	Sales and commission performance for an affiliate across campaigns.	Show affiliate 104's campaign history.
campaign_participants_without_sales	Campaign participants with no recorded sales, optionally for one campaign.	Which affiliates participated in the summer campaign but made no sales?
campaign_summary	Campaign revenue, commission expense, and sales counts.	How much revenue and commission expense did campaign 3 generate?
campaign_ranking	Campaigns ranked by recorded sales count.	Which campaign made the most sales?
affiliate_ranking	Affiliates ranked by net revenue for a campaign or date range, with a result limit.	Which affiliate generated the most net revenue?
affiliate_sales_ranking	Affiliates ranked by recorded sales count, optionally for one campaign.	Which affiliate made the most sales?

Ignatious implements the repository queries behind these tools. Morgan exposes them through MCP and connects them to the chatbot.

Database
Table	Purpose	Fields
Affiliates	Synthetic affiliate profiles.	AffiliateId (PK), Name, ContentCategory, JoinDate
Campaigns	Campaign names and dates.	CampaignId (PK), CampaignName, StartDate, EndDate
AffiliateCampaigns	Who took part in each campaign (one row per affiliate/campaign pair), with commission rate. Includes participants with no sales.	AffiliateCampaignId (PK), AffiliateId (FK), CampaignId (FK), CommissionRate
AffiliateSales	Sales attributed to that participation.	SaleId (PK), AffiliateCampaignId (FK), SaleDate, GrossRevenue, DiscountAmount, RefundAmount, CommissionExpense

The database contains at least 25 meaningful synthetic records across the four tables and persists across app restarts.

Calculation Rules
Measure	Definition
Net Revenue	Gross Revenue − Discount Amount − Refund Amount
Commission Expense	Recorded commission expense for the attributed sales.
Revenue After Commission	Net Revenue − Commission Expense. This is not profit; other costs are excluded.
Participant Without Sales	An affiliate in AffiliateCampaigns with no AffiliateSales records for that campaign.

A fully refunded sale still counts as a recorded sale. Zero sales and zero net revenue are treated as different things.

Safeguards
Questions must be 3–500 characters, checked in the browser and again on the server.
Every form post carries an anti-forgery token; posts without one are rejected.
Answers are rendered as plain text, so neither a user nor the model can inject HTML or scripts.
Errors and timeouts show a plain message; technical details go to the server log only.
Rate limit of 10 questions per minute per visitor, to protect the model budget.
Conversation history is kept per browser session and can be cleared at any time.
SQL-like input cannot alter the database (parameterized queries only).
Limitations and Responsible Use
All data is synthetic. Rankings describe this data; they do not prove why an affiliate performed well or poorly.
The assistant only answers what the available tools cover. Other questions get a "can't answer" response rather than a guess.
Conversation history is lost when the app restarts (the database itself persists).
Team Responsibilities
Member	Role	Tasks	Deliverables
Ignatious	Data & Analytics Lead	Design the Affiliates, Campaigns, AffiliateCampaigns, and AffiliateSales tables. Define primary/foreign keys and seed at least 25 meaningful synthetic records. Build parameterized C# repository queries. Include affiliates with no sales, multiple campaigns, and different commission amounts. Verify revenue and commission calculations.	SQLite database, seed code, ERD, repository methods, and expected results for testing.
Morgan	MCP & Backend Lead	Build the eight MCP tools (campaign list, affiliate lookup, affiliate campaign history, participants without sales, campaign summary, campaign ranking, affiliate net-revenue ranking, affiliate sales ranking). Add tool descriptions, parameter validation, and output limits. Implement the MCP client, runtime discovery, model integration, system prompt, fallback mode, and tool-use logs.	Working HTTP MCP endpoint, chatbot service, eight documented tools, and discovery/invocation evidence.
Joel	MVC & Experience Lead	Build the chat page and About & Architecture page. Implement controllers, view models, conversation history, suggested marketing questions, responsive styling, input validation, anti-forgery protection, rate limiting, and friendly errors. Display tool evidence and model/fallback mode.	Polished MVC interface connected to the chatbot service, architecture page, and validation/usability evidence.
David	Cloud, Testing & Documentation Lead	Deploy the application to Azure. Configure HTTPS, AllowedHosts, secrets, and persistent SQLite storage. Coordinate acceptance tests, the ten-question evaluation matrix, screenshots/logs, README, report assembly, and presentation rehearsal.	Working Azure URL, deployment instructions, test matrix, evidence appendix, assembled report, and restart-persistence evidence.
Testing Responsibilities
Member	Tests and Evidence
Ignatious	Verify seed counts, table relationships, campaign totals, affiliate rankings, and participants without sales against known results.
Morgan	Verify runtime discovery of all eight tools, tool validation/output limits, grounded answers, missing-data responses, prompt injection, and model/tool failure handling.
Joel	Verify invalid/long input, anti-forgery protection, rate limiting, conversation history, mode indicators, tool evidence, responsive layout, and friendly errors.
David	Verify release build, Azure deployment, local/cloud answer consistency, database persistence after restart, and completion of all A1–A10 acceptance evidence.
Everyone	Contribute to the ten-question evaluation and verify SQL-like input does not alter the database.
Code Reviews
Author	Reviewer	Review Focus
Ignatious	Morgan	Repository methods, query parameters, and tool compatibility.
Morgan	Ignatious	Tool results, calculations, and data grounding.
Joel	David	Validation, errors, and deployment compatibility.
David	Joel	Deployment configuration and reproducible setup.
Shared Responsibilities
Everyone makes visible GitHub commits and authors or meaningfully reviews C# code.
Everyone writes their own report section and supplies test evidence.
Each member contributes at least two consequential AI-use records.
The team documents one AI suggestion that compiled but required correction.
Document at least two limitations and one responsible-AI consideration.
Everyone speaks during the presentation and can explain the full architecture.
Rehearse and prepare a local backup demo at least 48 hours before presenting.
