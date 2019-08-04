
namespace Conection.HubbleWS
{
    public class Operator
    {
        /// <summary>
        /// Identificador
        /// (NComapny + TIN)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Nombre
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Número de identificación fiscal (NIF, CIF, RUC...)
        /// </summary>
        public string TIN { get; set; }
    }
}
