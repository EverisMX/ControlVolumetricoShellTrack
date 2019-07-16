using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class GetPaymentMethodsResponse : BaseResponse
    {
        public IList<PaymentMethod> PaymentMethodList { get; set; } = new List<PaymentMethod>();

        /// <summary>
        /// Identificador de la tarjeta bancaria por defecto.
        /// Nulo si no hay ninguna
        /// </summary>
        public string DefaultBankCardId { get; set; }
    }
}
