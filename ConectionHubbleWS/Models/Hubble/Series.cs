
namespace Conection.HubbleWS
{
    public class Series
    {
        /// <summary>
        /// Identificador 
        /// (NCompany + Code)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Código
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Contador.
        /// Su valor corresponde con el usado en el último documento generado
        /// </summary>
        public int Counter { get; set; }

        /// <summary>
        /// Nombre definido para la Serie
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Tipo
        /// </summary>
        public SeriesType Type { get; set; }
    }
}
