using System;
using System.Collections.Generic;

namespace Conection.HubbleWS
{
    public class GetSeriesResponse : BaseResponse
    {
        public IList<Series> SeriesList { get; set; } = new List<Series>();
    }
}
