namespace Domain.ValueObjects
{
    /// <summary>
    /// Sentiment a analytické odhady
    /// </summary>
    public record SentimentMetrics(
        decimal ConsensusTargetPrice,
        string AnalystRecommendation,  
        decimal MediaSentimentScore,
        decimal AnalystConsensus = 0);  
}