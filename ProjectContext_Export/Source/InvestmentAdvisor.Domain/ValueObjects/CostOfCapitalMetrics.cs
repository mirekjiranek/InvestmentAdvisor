namespace Domain.ValueObjects
{
    /// <summary>
    /// Value Object obsahující náklady kapitálu (WACC) a požadovanou návratnost vlastního kapitálu.
    /// </summary>
    public record CostOfCapitalMetrics(
        decimal WACC,                    // Vážený průměr nákladů kapitálu
        decimal RequiredReturnOnEquity   // Požadovaná návratnost vlastního kapitálu
    );
}
