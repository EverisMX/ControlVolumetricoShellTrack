using System;
using System.Collections.Generic;

namespace Conection.HubbleWS
{
    public class RegisterTipRequest : BaseRequest
    {
        public RegisterTipDAO RegisterTip { get; set; } = new RegisterTipDAO();
    }
}
