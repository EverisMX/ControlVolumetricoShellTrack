using System;
using System.Collections.Generic;

namespace Conection.HubbleWS
{
    public class GetProvisionalIdToDocumentNumberMappingRequest : BaseRequest
    {
        public IList<CreateDocumentDAO> CreateDAOList { get; set; } = new List<CreateDocumentDAO>();
    }
}
