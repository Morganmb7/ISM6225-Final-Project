using AffiliateDataChat.Models;

namespace AffiliateDataChat.Data;

public sealed class DemoAffiliateDataSource : IAffiliateDataSource
{
    private static readonly IReadOnlyList<Affiliate> Affiliates =
    [
        new("AFF-1001", "Avery Johnson", "avery@example.com", "Active", 2, 18450.00m),
        new("AFF-1002", "Jordan Lee", "jordan@example.com", "Active", 1, 12600.00m),
        new("AFF-1003", "Morgan Patel", "morgan@example.com", "Active", 2, 9875.00m),
        new("AFF-1004", "Taylor Smith", "taylor@example.com", "Pending", 1, 2150.00m),
        new("AFF-1005", "Riley Chen", "riley@example.com", "Active", 1, 760.00m)
    ];

    private static readonly IReadOnlyList<CampaignParticipant> Participants =
    [
        new("SPRING-2026", "AFF-1001", "Avery Johnson", 12, 8400.00m),
        new("SPRING-2026", "AFF-1002", "Jordan Lee", 8, 5600.00m),
        new("SPRING-2026", "AFF-1003", "Morgan Patel", 0, 0.00m),
        new("SPRING-2026", "AFF-1004", "Taylor Smith", 0, 0.00m),
        new("SPRING-2026", "AFF-1005", "Riley Chen", 3, 2100.00m),
        new("SUMMER-2026", "AFF-1001", "Avery Johnson", 7, 4900.00m),
        new("SUMMER-2026", "AFF-1003", "Morgan Patel", 5, 3475.00m),
        new("SUMMER-2026", "AFF-1004", "Taylor Smith", 0, 0.00m)
    ];

    private static readonly IReadOnlyDictionary<string, string> CampaignNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["SPRING-2026"] = "Spring 2026 Launch",
            ["SUMMER-2026"] = "Summer 2026 Growth"
        };

    public Task<IReadOnlyList<Affiliate>> LookupAsync(string query, int limit, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalized = query.Trim();
        var results = Affiliates
            .Where(affiliate => affiliate.AffiliateId.Contains(normalized, StringComparison.OrdinalIgnoreCase)
                || affiliate.Name.Contains(normalized, StringComparison.OrdinalIgnoreCase)
                || affiliate.Email.Contains(normalized, StringComparison.OrdinalIgnoreCase))
            .OrderBy(affiliate => affiliate.Name)
            .Take(limit)
            .ToArray();
        return Task.FromResult<IReadOnlyList<Affiliate>>(results);
    }

    public Task<IReadOnlyList<CampaignParticipant>> GetParticipantsWithoutSalesAsync(
        string campaignId,
        int limit,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var results = Participants
            .Where(participant => string.Equals(participant.CampaignId, campaignId.Trim(), StringComparison.OrdinalIgnoreCase)
                && participant.SalesCount == 0)
            .OrderBy(participant => participant.AffiliateName)
            .Take(limit)
            .ToArray();
        return Task.FromResult<IReadOnlyList<CampaignParticipant>>(results);
    }

    public Task<CampaignSummary?> GetCampaignSummaryAsync(string campaignId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var rows = Participants
            .Where(participant => string.Equals(participant.CampaignId, campaignId.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (rows.Length == 0)
        {
            return Task.FromResult<CampaignSummary?>(null);
        }

        var salesCount = rows.Sum(row => row.SalesCount);
        return Task.FromResult<CampaignSummary?>(new CampaignSummary(
            rows[0].CampaignId,
            CampaignNames[rows[0].CampaignId],
            rows.Length,
            rows.Count(row => row.SalesCount > 0),
            rows.Count(row => row.SalesCount == 0),
            salesCount,
            rows.Sum(row => row.SalesAmount),
            Math.Round((decimal)salesCount / rows.Length, 2)));
    }

    public Task<IReadOnlyList<AffiliateRanking>> GetAffiliateRankingAsync(
        string campaignId,
        string metric,
        int limit,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var descending = string.Equals(metric, "salesAmount", StringComparison.OrdinalIgnoreCase);
        var rows = Participants
            .Where(participant => string.Equals(participant.CampaignId, campaignId.Trim(), StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(participant => descending ? participant.SalesAmount : participant.SalesCount)
            .ThenBy(participant => participant.AffiliateName)
            .Take(limit)
            .Select((participant, index) => new AffiliateRanking(
                index + 1,
                participant.AffiliateId,
                participant.AffiliateName,
                participant.SalesCount,
                participant.SalesAmount))
            .ToArray();
        return Task.FromResult<IReadOnlyList<AffiliateRanking>>(rows);
    }

    public static bool CampaignExists(string campaignId) =>
        CampaignNames.ContainsKey(campaignId.Trim());
}
