using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class RegisterTipResponse : BaseResponse
    {
        /// <summary>
        /// Response: Id de propina
        /// </summary>
        public int Signature { get; set; } = 0;
        public string Timestamp { get; set; }
        public TipResponse ObjResponse { get; set; }
    }
    public class TipResponse
    {
        public bool Guardado { get; set; }
    }
}
