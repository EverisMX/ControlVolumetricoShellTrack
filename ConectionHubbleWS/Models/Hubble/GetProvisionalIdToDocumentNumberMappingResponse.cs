using System;
using System.Collections.Generic;


namespace Conection.HubbleWS
{
    public class GetProvisionalIdToDocumentNumberMappingResponse : BaseResponse
    {
        /// <summary>
        /// Diccionario con la relación entre los identificadores de documento provisionales indicados
        /// en la entrada del proceso y los identificadores permanentes/definitivos generados.
        /// [Key-> identificador provisional ;
        ///  Value -> Una tupla con el identificador (NCompany + DocumentNumber) definitivo de documento y el código de documento definitivo correspondiente]
        /// </summary>
        public IDictionary<int, Tuple<string, string>> ProvisionalToDefinitiveDocumentIdDictionary { get; set; } = new Dictionary<int, Tuple<string, string>>();
    }
}
