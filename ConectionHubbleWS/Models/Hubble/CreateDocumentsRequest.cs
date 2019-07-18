using System;
using System.Collections.Generic;

namespace Conection.HubbleWS
{
    public class CreateDocumentsRequest : BaseRequest
    {
        public IList<CreateDocumentDAO> CreateDAOList { get; set; } = new List<CreateDocumentDAO>();
    }
}
