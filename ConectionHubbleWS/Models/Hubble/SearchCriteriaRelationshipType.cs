
namespace Conection.HubbleWS
{
    /// <summary>
    /// Especifica cómo se deben utilizar los criterios de búsqueda cuando hay más de uno,
    /// atendiendo a la relación que debe haber entre ellos: que se cumpla cualquiera, todos,...
    /// </summary>
    public enum SearchCriteriaRelationshipType
    {
        /// <summary>
        /// Todos los criterios deben cumplirse
        /// </summary>
        And = 0,

        /// <summary>
        /// Debe cumplirse cualquiera de los criterios
        /// </summary>
        Or = 1
    }
}
