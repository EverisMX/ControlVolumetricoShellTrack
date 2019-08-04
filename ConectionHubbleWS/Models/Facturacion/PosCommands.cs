using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models.Facturacion
{
    public class PosCommands
    {
        public string OpenDrawer { get; set; }
        public string PaperCut { get; set; }
        public IList<string> Others { get; set; } = new List<string>();
    }
}
