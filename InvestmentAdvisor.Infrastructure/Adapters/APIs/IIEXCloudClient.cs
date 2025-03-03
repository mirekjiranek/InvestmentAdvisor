namespace Infrastructure.Adapters.APIs
{
    public interface IIEXCloudClient
    {
        Task<string> GetQuoteAsync(string symbol);
    }
}
