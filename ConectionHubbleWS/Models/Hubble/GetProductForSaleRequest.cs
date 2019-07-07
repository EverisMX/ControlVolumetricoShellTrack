using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class GetProductForSaleRequest : BaseRequest
    {
        /// <summary>
        /// [Optional]
        /// Get or sets the customer Id
        /// </summary>
        public string CustomerId { get; set; }
        /// <summary>
        /// Get or sets the product Id
        /// </summary>
        public string ProductId { get; set; }
        /// <summary>
        /// Get or sets the number of products to compute
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        ///  Gets or sets barcode
        /// </summary>
        public string Barcode { get; set; }
    }
}
