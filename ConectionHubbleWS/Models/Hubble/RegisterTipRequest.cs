using System;
using System.Collections.Generic;

namespace Conection.HubbleWS
{
    public class RegisterTipRequest : BaseRequest
    {
        public RegisterTipDAO RegisterTip { get; set; } = new RegisterTipDAO();
        
        #region Campos para DSN
        public string Company { get; set; }
        public string User { get; set; }
        public string nCliente { get; set; }
        #endregion Campos para DSN
    }
}
