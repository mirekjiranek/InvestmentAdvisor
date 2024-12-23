using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class InvestmentInstrumentRepository : BaseRepository<InvestmentInstrument>, IInvestmentInstrumentRepository
    {
        public InvestmentInstrumentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<InvestmentInstrument?> GetBySymbolAsync(string symbol)
        {
            return await _context.InvestmentInstruments
                                 .Include(i => i.FundamentalData)
                                 .Include(i => i.PriceHistory)
                                 .Include(i => i.CurrentRecommendation)
                                 .FirstOrDefaultAsync(i => i.Symbol == symbol);
        }
    }
}
