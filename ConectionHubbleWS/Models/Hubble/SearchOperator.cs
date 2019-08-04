
namespace Conection.HubbleWS
{
    public class SearchOperator
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

        /// <summary>
        /// Login
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Código
        /// </summary>
        public string Code { get; set; }

    }
}
