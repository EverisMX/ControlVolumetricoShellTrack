
namespace Conection.HubbleWS
{
    public class Currency
    {
        /// <summary>
        /// Identificador
        /// (NCompany + Code)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Descripción.
        /// Null en caso de no estar definido
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Tipo, atendiendo a criterio de prioridad 
        /// (moneda base: más prioritaria o por defecto; moneda secundaria, ...)
        /// </summary>
        public CurrencyPriorityType PriorityType { get; set; }

        /// <summary>
        /// Factor de cambio respecto a la moneda base, es decir, cuántas por cada unidad de la moneda base
        /// La moneda base tiene un factor de 1.
        /// Null en caso de no estar definido.
        /// (p.ej: si la base fuera el euro, la peseta tendría un factor de 166.386)
        /// </summary>
        public decimal ChangeFactorFromBase { get; set; }
    }
}
