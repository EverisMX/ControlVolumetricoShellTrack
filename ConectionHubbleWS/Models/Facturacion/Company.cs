using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models.Facturacion
{
    public class Company
    {
        /// <summary>
        /// Identificador
        /// (NCompany)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// N{umero de identificacion fiscal (cif - tin - ruc - ...) 
        /// </summary>
        public string Tin { get; set; }

        /// <summary>
        /// Raz{on Social
        /// </summary>
        public string BusinessName { get; set; }

        /// <summary>
        /// Direccion comercial
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Ciudad
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Codigo Postal
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Telefono de contacto
        /// </summary>
        public string Telephone { get; set; }
    }
}
