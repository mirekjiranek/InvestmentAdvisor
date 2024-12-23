using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    // DTO (Data Transfer Object) pro návratové typy z dotazů nebo přenos dat mezi vrstvami.
    // Slouží k tomu, abychom nevystavovali přímo doménové entity (výhodné i pro mapování).
    public class InstrumentDto
    {
        public Guid Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        // Můžeme přidat i další data, např. poslední cenu, doporučení apod.
        public decimal? LastPrice { get; set; }
        public string? CurrentRecommendation { get; set; }
    }
}
