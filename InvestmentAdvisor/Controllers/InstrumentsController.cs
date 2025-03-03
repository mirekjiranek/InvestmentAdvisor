using Application.Commands;
using Application.DTOs;
using Application.Queries;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InvestmentAdvisor.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstrumentsController : ControllerBase
    {
        private readonly IQueryHandler<GetInstrumentQuery, InstrumentDto?> _getInstrumentQueryHandler;
        private readonly ICommandHandler<UpdateInstrumentsCommand> _updateInstrumentsCommandHandler;

        public InstrumentsController(
            IQueryHandler<GetInstrumentQuery, InstrumentDto?> getInstrumentQueryHandler,
            ICommandHandler<UpdateInstrumentsCommand> updateInstrumentsCommandHandler)
        {
            _getInstrumentQueryHandler = getInstrumentQueryHandler;
            _updateInstrumentsCommandHandler = updateInstrumentsCommandHandler;
        }

        [HttpGet("{symbol}")]
        public async Task<ActionResult<InstrumentDto>> Get(string symbol, CancellationToken cancellationToken = default)
        {
            var query = new GetInstrumentQuery(symbol);
            var result = await _getInstrumentQueryHandler.HandleAsync(query, cancellationToken);
            
            if (result == null)
                return NotFound();
                
            return Ok(result);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateInstruments([FromBody] List<string> symbols, CancellationToken cancellationToken = default)
        {
            var command = new UpdateInstrumentsCommand(symbols);
            await _updateInstrumentsCommandHandler.HandleAsync(command, cancellationToken);
            return Ok();
        }
    }
}