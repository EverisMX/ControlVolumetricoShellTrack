using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class InsertClousureCashBoxRequest
    {
        /// <summary>
        /// Identificador
        /// (NCompany + Code)
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Identificador de UsuarioBBDD
        /// 
        /// </summary>
        public string User { get; set; }
        public string NcierreTPV { get; set; }
        public string Operario { get; set; }
        public string Caja { get; set; }
        public DateTime Dfecha { get; set; }
        public DateTime Hfecha { get; set; }
        public string Nempresa { get; set; }
        public string CajaOperario { get; set; }
        public decimal SaldoApertura { get; set; }
        public decimal SaldoMetalico { get; set; }
        public decimal MetalicoReal { get; set; }
        public decimal Descuadre { get; set; }
        public decimal SalidaCaja { get; set; }
        public decimal SaldoCierre { get; set; }
        public DateTime FechaSalida { get; set; }
        public string PagoSalida { get; set; }
        public Int32 AnotarSalidasNoMetalicos { get; set; }
        public string UriModular { get; set; }

    }
}
