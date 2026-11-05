using AffiliateDataChat.Models;

namespace AffiliateDataChat.Data;

public interface IAffiliateDataSource
{
    Task<IReadOnlyList<Affiliate>> LookupAsync(string query, int limit, CancellationToken cancellationToken);

    Task<IReadOnlyList<CampaignParticipant>> GetParticipantsWithoutSalesAsync(
        string campaignId,
        int limit,
        CancellationToken cancellationToken);

    Task<CampaignSummary?> GetCampaignSummaryAsync(string campaignId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AffiliateRanking>> GetAffiliateRankingAsync(
        string campaignId,
        string metric,
        int limit,
        CancellationToken cancellationToken);
}
