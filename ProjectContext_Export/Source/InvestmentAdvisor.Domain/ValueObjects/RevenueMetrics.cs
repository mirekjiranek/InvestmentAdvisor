namespace Domain.ValueObjects
{
    /// <summary>
    /// Value Object obsahující ukazatele tržeb společnosti.
    /// </summary>
    public record RevenueMetrics(
        decimal SalesPerShare // Tržby na akcii (Revenue per Share)
    );
}
