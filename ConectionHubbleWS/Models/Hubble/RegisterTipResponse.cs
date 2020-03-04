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
        public int Id { get; set; } = 0;
    }
}
