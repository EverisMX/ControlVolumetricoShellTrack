
namespace Conection.HubbleWS
{
    public class SearchOperatorCriteria
    {
        /// <summary>
        /// Texto a buscar
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Campo en el que buscar
        /// </summary>
        public SearchOperatorCriteriaFieldType Field { get; set; }

        /// <summary>
        /// Forma en la que se acepta la ocurrencia: que empiece por, en cualquier parte...
        /// </summary>
        public SearchOperatorCriteriaMatchingType MatchingType { get; set; }
    }
}
