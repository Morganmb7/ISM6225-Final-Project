using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenAI;
namespace AffiliateDataChat.Services;

public sealed partial class ChatOrchestrator(
    IConfiguration configuration,
    McpToolClient mcpToolClient,
    ILogger<ChatOrchestrator> logger)
{
    public string ProviderLabel => IsOpenAiConfigured ? $"OpenAI · {configuration["Ai:Model"]}" : "Guided demo · Affiliate MCP tools";
    private bool IsOpenAiConfigured =>
    string.Equals(configuration["Ai:Provider"], "OpenAI", StringComparison.OrdinalIgnoreCase)
    && !string.IsNullOrWhiteSpace(configuration["Ai:ApiKey"]);
    public async Task<string> AskAsync(string question, CancellationToken cancellationToken)
    {
        return await mcpToolClient.WithDiscoveredToolsAsync(async tools =>
        {
            if (IsOpenAiConfigured)
            {
                return await AskOpenAiAsync(question, tools, cancellationToken);
            }
            return await AskDemoAsync(question, tools, cancellationToken);
        }, cancellationToken);
    }
    private async Task<string> AskOpenAiAsync(
    string question,
    IList<McpClientTool> tools,
    CancellationToken cancellationToken)
    {
        var apiKey = configuration["Ai:ApiKey"]!;
        var model = configuration["Ai:Model"] ?? "gpt-5-mini";
        IChatClient client = new OpenAIClient(apiKey)
        .GetChatClient(model)
        .AsIChatClient()
        .AsBuilder()
        .UseFunctionInvocation()
        .Build();
        var systemPrompt = configuration["Ai:SystemPrompt"]
            ?? "You are a concise affiliate and campaign-data assistant. Use the MCP tools for every factual data answer. " +
            "Use affiliate_lookup for affiliate identity or status, campaign_participants_without_sales for zero-sale participants, " +
            "campaign_summary for campaign totals, and affiliate_ranking for ordered campaign performance. " +
            "Never invent rows, infer missing data, or run arbitrary SQL. State when no result is found. " +
            "The current environment may use demo data until the affiliate dataset is connected.";
        var messages = new List<ChatMessage>
{
 new(ChatRole.System, systemPrompt),
new(ChatRole.User, question)
};
        var response = await client.GetResponseAsync(messages, new ChatOptions
        {
            Tools = [.. tools],
            MaxOutputTokens = 700
        }, cancellationToken);
        logger.LogInformation("OpenAI response completed using {ToolCount} discovered MCP tools and model {Model}.", tools.Count, model);
        return response.Text;
    }
    private async Task<string> AskDemoAsync(
    string question,
    IList<McpClientTool> tools,
    CancellationToken cancellationToken)
    {
        var lower = question.ToLowerInvariant();
        McpClientTool tool;
        IReadOnlyDictionary<string, object?>? arguments;
        var campaignId = CampaignIdRegex().Match(question);
        if (lower.Contains("without sales") || lower.Contains("no sales") || lower.Contains("zero sales"))
        {
            tool = FindTool(tools, "campaign_participants_without_sales");
            arguments = new Dictionary<string, object?> { ["campaignId"] = campaignId.Success ? campaignId.Value.ToUpperInvariant() : "SPRING-2026" };
        }
        else if ((lower.Contains("campaign") || lower.Contains("affiliate")) && (lower.Contains("summary") || lower.Contains("overview") || lower.Contains("total")))
        {
            tool = FindTool(tools, "campaign_summary");
            arguments = new Dictionary<string, object?> { ["campaignId"] = campaignId.Success ? campaignId.Value.ToUpperInvariant() : "SPRING-2026" };
        }
        else if ((lower.Contains("rank") || lower.Contains("top affiliates") || lower.Contains("best affiliates")) && lower.Contains("affiliate"))
        {
            tool = FindTool(tools, "affiliate_ranking");
            arguments = new Dictionary<string, object?>
            {
                ["campaignId"] = campaignId.Success ? campaignId.Value.ToUpperInvariant() : "SPRING-2026",
                ["limit"] = ExtractCount(question)
            };
        }
        else if (lower.Contains("affiliate") || lower.Contains("partner"))
        {
            tool = FindTool(tools, "affiliate_lookup");
            var affiliateId = AffiliateIdRegex().Match(question);
            arguments = new Dictionary<string, object?>
            {
                ["query"] = affiliateId.Success ? affiliateId.Value.ToUpperInvariant() : ExtractAffiliateQuery(question)
            };
        }
        else
        {
            tool = FindTool(tools, "affiliate_lookup");
            var affiliateId = AffiliateIdRegex().Match(question);
            arguments = new Dictionary<string, object?>
            {
                ["query"] = affiliateId.Success ? affiliateId.Value.ToUpperInvariant() : ExtractAffiliateQuery(question)
            };
        }
        logger.LogInformation("Demo mode selected MCP tool {ToolName}.", tool.Name);
        var started = Stopwatch.GetTimestamp();
        var result = await tool.CallAsync(arguments, cancellationToken: cancellationToken);
        var text = string.Join("\n", result.Content.OfType<TextContentBlock>().Select(block => block.Text));
        logger.LogInformation("MCP tool {ToolName} completed in {ElapsedMs} ms with {ContentBlocks} content blocks.",
            tool.Name,
            Stopwatch.GetElapsedTime(started).TotalMilliseconds,
            result.Content.Count);
        return $"Demo mode used the MCP tool `{tool.Name}`.\n\n{text}\n\nConfigure an OpenAI key to receive a natural-language interpretation.";
    }
    private static McpClientTool FindTool(IEnumerable<McpClientTool> tools, string name) =>
    tools.Single(tool => string.Equals(tool.Name, name, StringComparison.Ordinal));
    private static int ExtractCount(string question)
    {
        var match = Regex.Match(question, @"\b([1-9]|10)\b");
        return match.Success ? int.Parse(match.Value) : 5;
    }
    private static string ExtractAffiliateQuery(string question)
    {
        var cleaned = Regex.Replace(question, "\\b(affiliate|partner|lookup|find|search|for|who|is|the)\\b", " ", RegexOptions.IgnoreCase);
        return string.IsNullOrWhiteSpace(cleaned) ? "AFF-1001" : cleaned.Trim();
    }
    private static Regex CampaignIdRegex() => new(@"\b[A-Z]{2,20}-\d{4}\b", RegexOptions.IgnoreCase);
    private static Regex AffiliateIdRegex() => new(@"\bAFF-\d{4}\b", RegexOptions.IgnoreCase);
}