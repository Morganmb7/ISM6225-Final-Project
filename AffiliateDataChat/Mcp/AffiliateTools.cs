using System.ComponentModel;
using System.Text.Json;
using AffiliateDataChat.Data;
using ModelContextProtocol.Server;

namespace AffiliateDataChat.Mcp;

[McpServerToolType]
public sealed class AffiliateTools(IAffiliateDataSource dataSource)
{
    private const int MaxLookupResults = 10;
    private const int MaxParticipantResults = 25;
    private const int MaxRankingResults = 10;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    [McpServerTool(Name = "affiliate_lookup"), Description(
        "Find affiliates by affiliate ID, name, or email. Use this for identity and status lookups. " +
        "The response contains at most 10 affiliates.")]
    public async Task<string> AffiliateLookupAsync(
        [Description("At least two characters from an affiliate ID, name, or email address.")] string query,
        [Description("Maximum number of affiliates to return, from 1 through 10.")] int limit = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
        {
            return "query must contain at least two non-whitespace characters.";
        }

        if (query.Trim().Length > 100)
        {
            return "query must be 100 characters or fewer.";
        }

        if (limit is < 1 or > MaxLookupResults)
        {
            return $"limit must be between 1 and {MaxLookupResults}.";
        }

        var results = await dataSource.LookupAsync(query, limit, cancellationToken);
        return JsonSerializer.Serialize(results, JsonOptions);
    }

    [McpServerTool(Name = "campaign_participants_without_sales"), Description(
        "List participants in a campaign who have zero sales. Use this to identify affiliates needing follow-up. " +
        "The response contains at most 25 participants.")]
    public async Task<string> CampaignParticipantsWithoutSalesAsync(
        [Description("Campaign identifier, such as SPRING-2026.")] string campaignId,
        [Description("Maximum number of participants to return, from 1 through 25.")] int limit = 25,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidCampaignId(campaignId))
        {
            return "campaignId must contain 3 to 50 letters, numbers, or hyphens.";
        }

        if (limit is < 1 or > MaxParticipantResults)
        {
            return $"limit must be between 1 and {MaxParticipantResults}.";
        }

        var results = await dataSource.GetParticipantsWithoutSalesAsync(campaignId, limit, cancellationToken);
        return JsonSerializer.Serialize(results, JsonOptions);
    }

    [McpServerTool(Name = "campaign_summary"), Description(
        "Return participant, sales, and revenue totals for one campaign, including the number of participants without sales.")]
    public async Task<string> CampaignSummaryAsync(
        [Description("Campaign identifier, such as SPRING-2026.")] string campaignId,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidCampaignId(campaignId))
        {
            return "campaignId must contain 3 to 50 letters, numbers, or hyphens.";
        }

        var result = await dataSource.GetCampaignSummaryAsync(campaignId, cancellationToken);
        return result is null
            ? $"No campaign was found for {campaignId.Trim()}."
            : JsonSerializer.Serialize(result, JsonOptions);
    }

    [McpServerTool(Name = "affiliate_ranking"), Description(
        "Rank campaign affiliates by sales count or sales amount, highest first. The response contains at most 10 affiliates.")]
    public async Task<string> AffiliateRankingAsync(
        [Description("Campaign identifier, such as SPRING-2026.")] string campaignId,
        [Description("Ranking metric: salesCount or salesAmount.")] string metric = "salesCount",
        [Description("Maximum number of ranked affiliates to return, from 1 through 10.")] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidCampaignId(campaignId))
        {
            return "campaignId must contain 3 to 50 letters, numbers, or hyphens.";
        }

        if (!string.Equals(metric, "salesCount", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(metric, "salesAmount", StringComparison.OrdinalIgnoreCase))
        {
            return "metric must be salesCount or salesAmount.";
        }

        if (limit is < 1 or > MaxRankingResults)
        {
            return $"limit must be between 1 and {MaxRankingResults}.";
        }

        var results = await dataSource.GetAffiliateRankingAsync(campaignId, metric, limit, cancellationToken);
        return JsonSerializer.Serialize(results, JsonOptions);
    }

    private static bool IsValidCampaignId(string? campaignId)
    {
        var value = campaignId?.Trim();
        return value is not null
            && value.Length is >= 3 and <= 50
            && value.All(character => char.IsLetterOrDigit(character) || character == '-');
    }
}
