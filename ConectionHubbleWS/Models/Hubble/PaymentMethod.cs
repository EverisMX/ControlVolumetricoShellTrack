
namespace Conection.HubbleWS
{
    public class PaymentMethod
    {
        /// <summary>
        /// Identificador
        /// (NCompany + Code)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Descripción
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Número de copias a imprimir (como mínimo) en la emisión del documento donde se use este medio de pago
        /// </summary>
        public int NumberOfCopiesToPrint { get; set; }

        /// <summary>
        /// Tipo
        /// </summary>
        public PaymentMethodType Type { get; set; }
    }
}
