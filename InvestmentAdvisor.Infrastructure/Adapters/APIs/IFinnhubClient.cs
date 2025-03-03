namespace Infrastructure.Adapters.APIs
{
    public interface IFinnhubClient
    {
        Task<string> GetCompanyProfileAsync(string symbol);
    }
}
