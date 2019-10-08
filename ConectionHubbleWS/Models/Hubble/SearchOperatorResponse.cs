using System;
using System.Collections.Generic;

namespace Conection.HubbleWS
{
    public class SearchOperatorResponse : BaseResponse
    {
        public IList<SearchOperator> OperatorList { get; set; } = new List<SearchOperator>();
    }
}
