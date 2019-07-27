
namespace ControlVolumetricoShellWS.Implementation
{
    public class LockSupplyTransactionOfFuellingPointResponse : BaseResponse
    {
        public string ProductReference { get; set; }
        public string ProductName { get; set; }
        public decimal UnitaryPricePreDiscount { get; set; }
        public decimal CorrespondingVolume { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountedAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal TaxPercentage { get; set; }
        /// <summary>
        /// Identificador de la trnnsaccion del surtidor.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Price of grade
        /// </summary>
        public decimal GradeUnitPrice { get; set; }
        /// <summary>
        /// Gets or sets the grade identifier.
        /// </summary>
        public int GradeId { get; set; }
        public int posID { get; set; }
    }
}
