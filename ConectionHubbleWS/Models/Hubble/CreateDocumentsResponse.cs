using System;
using System.Collections.Generic;

namespace Conection.HubbleWS.Models.Hubble
{
    public class CreateDocumentsResponse : BaseResponse
    {
        /// <summary>
        /// Diccionario con la relación entre los identificadores de documento provisionales indicados
        /// en la entrada del proceso y los identificadores permanentes/definitivos generados.
        /// [Key-> identificador provisional ; Value -> identificador (NCompany + DocumentNumber) definitivo de documento]
        /// </summary>
        public IDictionary<int, string> ProvisionalToDefinitiveDocumentIdDictionary { get; set; } = new Dictionary<int, string>();
    }
}
