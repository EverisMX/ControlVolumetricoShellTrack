using System;

namespace ControlVolumetricoShellWS.Implementation
{
    public class SupplyTransaction
    {
        /// <summary>
        /// Identificador
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador del surtidor donde se generó la transacción
        /// </summary>
        public int FuellingPointId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SuppyTransactionType Type { get; set; }

        /// <summary>
        /// Gets or sets the grade identifier.
        /// </summary>
        public int GradeId { get; set; }

        /// <summary>
        /// Price of grade
        /// </summary>
        public decimal GradeUnitPrice { get; set; }

        /// <summary>
        /// Ilion reference of grade
        /// </summary>
        public string GradeReference { get; set; }

        /// <summary>
        /// Monetary amount served
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// Volume supplied
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// Top value to reach in fuelling operation, if any
        /// </summary>
        public decimal? FuellingLimitValue { get; set; }

        /// <summary>
        /// Type of value to limit the fuelling operation, if any
        /// </summary>
        public FuellingLimitType? FuellingLimitType { get; set; }

        /// <summary>
        /// POS id that is actually locking this transaction.
        /// Null if none.
        /// </summary>
        public int? LockingPOSId { get; set; }

        /// <summary>
        /// Tipo de modo de servicio.
        /// Será "Otro" cuando el surtidor esté cerrado o sea uno distinto de los soportados
        /// </summary>
        public ServiceModeType ServiceModeType { get; set; }

        /// <summary>
        /// Staring Date and time of supply
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Ending Date and time of supply
        /// </summary>
        public DateTime FinishDateTime { get; set; }
    }
    public enum SuppyTransactionType
    {
        PostpaidLockedByOwnPOS,
        PostpaidLockedByOtherPOS,
        PostpaidNoLocked,

        PrepaidCompleteLockedByOwnPOS,
        PrepaidCompleteLockedByOtherPOS,
        PrepaidCompleteNotLocked,

        PrepaidPartialLockedByOwnPOS,
        PrepaidPartialLockedByOtherPOS,
        PrepaidPartialNotLocked,

        Other
    }
    public enum ServiceModeType
    {
        /// <summary>
        /// Payment will be made once the supply is made
        /// </summary>
        PostPaid = 0,

        /// <summary>
        /// Payment will be made before the supply is made
        /// </summary>
        PrePaid = 1,

        /// <summary>
        /// Other than post-paid and pre-paid fuelling mode
        /// (not necessarily unknown)
        /// </summary>
        Other = 2
    }
    public enum FuellingLimitType
    {
        /// <summary>
        /// Monetary limit type
        /// </summary>
        Monetary = 0,

        /// <summary>
        /// Volumne limit type
        /// </summary>
        Volume = 1
    }
}
