namespace AffiliateDataChat.Models;

public sealed record CampaignSummary(
    string CampaignId,
    string CampaignName,
    int ParticipantCount,
    int ParticipantsWithSales,
    int ParticipantsWithoutSales,
    int TotalSales,
    decimal TotalSalesAmount,
    decimal AverageSalesPerParticipant);
