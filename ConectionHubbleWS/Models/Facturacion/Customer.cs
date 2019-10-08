using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models.Facturacion
{
    public class Customer
    {

        /// (NComapny + Code)
        /// <summary>
        /// Identificador
        public string Id { get; set; }
        /// </summary>

        /// <summary>
        /// Número de identificación fiscal.
        /// (Ej: 22123456Z en el caso de un NIF).
        /// </summary>
        public string TIN { get; set; }

        /// <summary>
        /// [Opcional]
        /// Identificador (NComapny + Code) del tipo de número de identificación fiscal.
        /// Null si no está especificado.
        /// (Ej: 027636 podría ser el identificador de tipo de TIN que indicase que es un RUC)
        /// </summary>
        public string TINTypeId { get; set; }

        /// <summary>
        /// [Opcional]
        /// Razon Social.
        /// Null si no existe
        /// </summary>
        public string BusinessName { get; set; }

        /// <summary>
        /// Lista de direcciones.
        /// Vacía si no existen
        /// </summary>
        public IList<Address> AddressList { get; set; } = new List<Address>();

        /// <summary>
        /// [Opcional]
        /// Número de teléfono.
        /// Null si no existe
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// ¿Está dado de baja actualmente?
        /// </summary>
        public bool IsDischarged { get; set; }
        /// <summary>
        /// Mensaje asociado al cliente
        /// </summary>
        public string CustomerMessage { get; set; }
        /// <summary>
        /// Nulo si no hay TIPO_CLIENTE
        /// </summary>
        public string CustomerType { get; set; }
    }
}
