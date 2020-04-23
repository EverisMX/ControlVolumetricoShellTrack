using System.Threading.Tasks;
using ControlVolumetricoShellWS.Dominio;
using System.ServiceModel;

namespace ControlVolumetricoShellWS.Contract
{
    [ServiceContract]
    public interface IControlVolumetricoBilletero
    {
        [OperationContract]
        Salida_ConexionBilletero ConexionBilletero(Entrada_ConexionBilletero request);
        [OperationContract]
        Salida_RecibirBilletes RecibirBilletes(Entrada_RecibirBilletes request);
        [OperationContract]
        Salida_AlmacenarStatusBill AlmacenarStatusBill(Entrada_AlmacenarStatusBill request);
    }
}
