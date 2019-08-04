
namespace Conection.HubbleWS
{
    public class POSInformation
    {
        /// <summary>
        /// Codigo
        /// (Sin NCompany)
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Codigo de la caja (Sin NCompany )
        /// Null si no tiene una asignada
        /// </summary>
        public string CashboxCode { get; set; }

        /// <summary>
        /// Codigo de la tienda (Sin NCompany )
        /// Null si no tiene una asignada
        /// </summary>
        public string ShopCode { get; set; }

        /// <summary>
        /// Codigo del almacen (Sin NCompany )
        /// Null si no tiene una asignada
        /// </summary>
        public string StoreCode { get; set; }

        /// <summary>
        /// Codigo de la empresa
        /// (Ncomapany)
        /// </summary>
        public string CompanyCode { get; set; }
    }
}
