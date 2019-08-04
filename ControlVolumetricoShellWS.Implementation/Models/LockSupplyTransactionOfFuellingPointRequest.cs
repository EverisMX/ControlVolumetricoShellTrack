
namespace ControlVolumetricoShellWS.Implementation
{
    public class LockSupplyTransactionOfFuellingPointRequest
    {
        public string CustomerId { get; set; }
        public string OperatorId { get; set; }
        public int SupplyTransactionId { get; set; }
        public int FuellingPointId { get; set; }
    }
}
