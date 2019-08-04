using System;
using System.Collections.Generic;

namespace Conection.HubbleWS
{
    public class SearchOperatorRequest : BaseRequest
    {
        /// <summary>
        /// Criterios de búsqueda
        /// </summary>
        public IList<SearchOperatorCriteria> CriteriaList { get; set; } = new List<SearchOperatorCriteria>();

        /// <summary>
        /// Especifica el tipo de relación entre los criterios de búsqueda especificados
        /// (deben cumplirse todos, cualquiera de ellos,...)
        /// </summary>
        public SearchCriteriaRelationshipType CriteriaRelationshipType { get; set; }

        /// <summary>
        /// ¿Se deben incluir operadores dados de baja actualmente?
        /// </summary>
        public bool MustIncludeDischarged { get; set; }
    }
}
