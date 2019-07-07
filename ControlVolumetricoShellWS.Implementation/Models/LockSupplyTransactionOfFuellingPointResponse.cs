
namespace ControlVolumetricoShellWS.Implementation
{
    public class LockSupplyTransactionOfFuellingPointResponse
    {
        public string ProductReference { get; set; }
        public string ProductName { get; set; }
        public decimal UnitaryPricePreDiscount { get; set; }
        public decimal CorrespondingVolume { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountedAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal TaxPercentage { get; set; }
    }
}
