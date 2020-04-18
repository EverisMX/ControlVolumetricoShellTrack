using System.Threading.Tasks;
using ControlVolumetricoShellWS.Dominio;
using System.ServiceModel;

namespace ControlVolumetricoShellWS.Contract
{
    [ServiceContract]
    public interface IControlVolumetricoPaymentHP
    {
        [OperationContract]
        Task<Salida_PaymentPinpadHP> PaymentPinpadHP(Entrada_PaymentPinpadHP request);
    }
}
