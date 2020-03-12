using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class ClousureCashBoxTerminalRequest
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

        public string Operario { get; set; }

        public string Caja { get; set; }

        public DateTime Datefecha { get; set; }

        public DateTime Horafecha { get; set; }

        public string Empresa { get; set; }

        public string Caja_Operario { get; set; }
        public string UriModular { get; set; }
    }
}
