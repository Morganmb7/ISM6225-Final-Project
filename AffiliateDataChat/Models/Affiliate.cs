namespace AffiliateDataChat.Models;

public sealed record Affiliate(
    string AffiliateId,
    string Name,
    string Email,
    string Status,
    int ActiveCampaigns,
    decimal LifetimeSales);
