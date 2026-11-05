namespace AffiliateDataChat.Models;

public sealed record CampaignParticipant(
    string CampaignId,
    string AffiliateId,
    string AffiliateName,
    int SalesCount,
    decimal SalesAmount);
