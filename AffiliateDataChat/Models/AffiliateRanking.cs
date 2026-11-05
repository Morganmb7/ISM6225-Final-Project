namespace AffiliateDataChat.Models;

public sealed record AffiliateRanking(
    int Rank,
    string AffiliateId,
    string AffiliateName,
    int SalesCount,
    decimal SalesAmount);
