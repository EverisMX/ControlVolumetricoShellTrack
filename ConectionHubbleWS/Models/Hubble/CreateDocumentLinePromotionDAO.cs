
namespace Conection.HubbleWS.Models
{
    /// <summary>
    /// Representa una promoción aplicada a una línea de documento que se va a dar de alta
    /// </summary>
    public class CreateDocumentLinePromotionDAO
    {
        /// <summary>
        /// Identificador de la promoción
        /// </summary>
        public string PromotionId { get; set; }

        /// <summary>
        /// Descripción de la promoción.
        /// 
        /// Aunque puede obtenerse el texto de la promoción mediente su identificador,
        /// el objetivo de este campo es mantener el texto mostrado al cliente, por si 
        /// parte del contenido del mismo fuese de generación dinámica
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Valor monetario del descuento aplicado, con impuestos incluidos
        /// </summary>
        public decimal DiscountAmountWithTax { get; set; }

        /// <summary>
        /// Número de veces aplicado
        /// </summary>
        public int NumberOfTimesApplied { get; set; }
    }
}
