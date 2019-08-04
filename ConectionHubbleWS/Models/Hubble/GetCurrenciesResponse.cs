using System.Collections.Generic;

namespace Conection.HubbleWS
{
    public class GetCurrenciesResponse : BaseResponse
    {
        public IList<Currency> CurrencyList { get; set; } = new List<Currency>();
    }
}
