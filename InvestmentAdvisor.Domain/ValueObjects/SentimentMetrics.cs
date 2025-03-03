namespace Domain.ValueObjects
{
    /// <summary>
    /// Sentiment a analytické odhady
    /// </summary>
    public record SentimentMetrics(
        decimal ConsensusTargetPrice,
        string AnalystRecommendation,  
        decimal MediaSentimentScore,
        decimal AnalystConsensus = 0,  
        decimal InstitutionalOwnership = 0, // 0-1, kde 1 = 100% vlastnictví institucemi
        decimal InsiderBuying = 0);    // Míra nákupů insiderů (0-1, kde 1 = silné nákupy)
}