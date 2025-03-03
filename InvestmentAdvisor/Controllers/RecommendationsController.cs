using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Domain.Interfaces;

namespace InvestmentAdvisor.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;
        private readonly IInvestmentInstrumentRepository _repository;

        public RecommendationsController(
            IRecommendationService recommendationService,
            IInvestmentInstrumentRepository repository)
        {
            _recommendationService = recommendationService;
            _repository = repository;
        }

        [HttpGet("{symbol}")]
        public async Task<IActionResult> GenerateRecommendation(string symbol, CancellationToken cancellationToken = default)
        {
            var instrument = await _repository.GetBySymbolAsync(symbol, cancellationToken);
            
            if (instrument == null)
                return NotFound($"Instrument with symbol '{symbol}' not found");
                
            var recommendation = await _recommendationService.GenerateRecommendationAsync(instrument, cancellationToken);
            
            return Ok(new { 
                Symbol = symbol,
                Action = recommendation.Action.ToString(),
                TargetPrice = recommendation.TargetPrice,
                //TimePeriod = recommendation.TimePeriod, TODO
                Rationale = recommendation.Rationale,
                RiskLevel = recommendation.RiskLevel,
                Score = recommendation.Score,
                GeneratedDate = DateTime.UtcNow
            });
        }
    }
}