using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries
{
    // Interfaces/IQueryHandler.cs
    // Handler zpracovávající dotaz a vracející výsledek.
    public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
        // HandleAsync pro dotazy vrací data typu TResult.
    }
}
