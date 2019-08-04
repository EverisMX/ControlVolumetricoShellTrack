using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models.Facturacion
{

        public class GetPrintingConfigurationRequest : BaseRequest
        {
            public IList<UsecasePrintingConfiguration> UsecasesPrintingConfigurationList { get; set; } = new List<UsecasePrintingConfiguration>();
        }
    
}
