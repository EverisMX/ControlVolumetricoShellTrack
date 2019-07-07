using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using ControlVolumetricoShellWS.Contract;
using ControlVolumetricoShellWS.Dominio;
using Conection.HubbleWS;

namespace ControlVolumetricoShellWS.Implementation
{
    public class Service : IService
    {
        //public Salida_Obtiene_Tran Obtiene_Tran(string serial, string PTID, int Pos_Carga, string idpos, int nHD, string pss, string Id_teller)
        //{
        //    return null;
        //}
        public Salida_Obtiene_Tran Obtiene_Tran(Entrada_Obtiene_Tran request)
        {

            Entrada_Obtiene_Tran requestNew = new Entrada_Obtiene_Tran
            {
                idpos = request.idpos,
                Id_teller = request.Id_teller,
                nHD = request.nHD,
                Pos_Carga = request.Pos_Carga,
                pss = request.pss,
                PTID = request.PTID,
                serial = request.serial

            };

            var salida = new Salida_Obtiene_Tran {
                Resultado=true,
                ID_Interno=1223,
                Msj="mensaje",
                Estacion=1213,
                importe=3.5,
                Litros=15.3,
                Num_Operacion=23123,
                Parcial =true,
                PosID=2321,
                Precio_Uni=33.5,
                Producto="producto"

                
            };

            return salida;
        }


        public Salida_Info_Forma_Pago Info_Forma_Pago(Entrada_Info_Forma_Pagos request)
        {
            Entrada_Info_Forma_Pagos requestNew = new Entrada_Info_Forma_Pagos{
                Id_Transaccion = request.Id_Transaccion,
                Info_Forma_Pago =request.Info_Forma_Pago,
                nHD =request.nHD,
                parciales=request.parciales
                 
            };
            var salida = new Salida_Info_Forma_Pago {
                Msj="este es un msj",
                Resultado=true
            };

             return salida;
        }
        public Salida_LiberaCarga LiberaCarga(Entrada_LiberaCarga request)
        {
            Entrada_LiberaCarga requestNew = new Entrada_LiberaCarga
            {
                idpos = request.idpos,
                Pos_Carga = request.Pos_Carga,
                nHD = request.nHD,
                pss = request.pss,
                PTID = request.PTID,
                serial = request.serial

            };
            var salida = new Salida_LiberaCarga {
                Msj="este es un msj",
                Resultado=true
                
            };

            return salida;
        }
        public async Task<Salida_getProductInfo> getProductInfo(Entrada_getProductInfo request)
        {
            // SHELLMX- Se consigue el Token del TPV para hacer las pruebas. 
            var jsonTPVToken = System.IO.File.ReadAllText("C:/dist/tpv.config.json");
            TokenTPV bsObj = JsonConvert.DeserializeObject<TokenTPV>(jsonTPVToken);

            GetProductForSaleRequest getProductForSaleRequest = new GetProductForSaleRequest()
            {
                ProductId = request.IdProduct,
                Quantity = 1,
                Identity = bsObj.Identity,
            };
            InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
            GetProductForSaleResponse getProductForSaleResponse = await invokeHubbleWebAPIServices.GetProductForSale(getProductForSaleRequest);

            Salida_getProductInfo salida = new Salida_getProductInfo();
            if (getProductForSaleResponse.Status == 1)
            {
                salida.Resultado = true;
                salida.Msj = getProductForSaleResponse.Message;
                salida.producto = getProductForSaleResponse.ProductName;
                salida.importe = getProductForSaleResponse.FinalAmount;
                salida.precio_Uni = getProductForSaleResponse.FinalAmount;
                salida.mensajePromocion = "";
            }else if(getProductForSaleResponse.Status < 0) {
                salida.Resultado = false;
                salida.Msj = "Producto no existente";
                salida.producto = "";
                salida.importe = 0;
                salida.precio_Uni = 0;
                salida.mensajePromocion = "";
            }
            return salida;
        }
        public Salida_Electronic_billing Electronic_billing(Entrada_Electronic_billing request)
        {
            Entrada_Electronic_billing requestNew = new Entrada_Electronic_billing
            {
                NoCliente = request.NoCliente,
                Pos_Carga = request.Pos_Carga,
                nHD = request.nHD,
                pss = request.pss,
                PTID = request.PTID,
                Serial = request.Serial,
                idpos = request.idpos,
                Nticket=request.Nticket
                
            };
            Productos productos = new Productos
            {
                Nombre = "",
                Cantidad = 0,
                Importe = 0,
                Precio = 0
            };

            Electronic_billingJSON billingJSON = new Electronic_billingJSON
            {
                DatetimeCert = "",
                Fecha = "",
                Folio = "",
                FolioFiscal = "",
                FormaPago = "",
                Iva = 0,
                NumSat = "",
                Operador = "",
                Producto = productos,
                RegFiscal = "",
                RfcPro = "",
                SelloCdfi = "",
                SelloSat = "",
                Subtotal = 0,
                Terminal = "",
                Ticket = "",
                Tienda = "",
                Timbre ="",
                Total = 0,
                WebID = ""
            };

            var salida = new Salida_Electronic_billing {
                Msj = "Impresion de prueva",
                Resultado = true,
                Text = JsonConvert.SerializeObject(billingJSON)
            };

            return salida;
        }
        
    }
    }
