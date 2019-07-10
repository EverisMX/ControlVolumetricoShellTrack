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
using Conection.HubbleWS.Models.Facturacion;

namespace ControlVolumetricoShellWS.Implementation
{
    public class Service : IService
    {
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

            Random r = new Random();
            double a = r.NextDouble();
            int b = r.Next(1001, 5890);

            double num = (Math.Truncate(a * 20000) / 100);
            double lit = (Math.Truncate(a * 10000) / 100);
            int variableA = r.Next(1, 4);
            string type;
            switch (variableA)
            {
                case 1:
                    type = "95";
                    break;
                case 2:
                    type = "97";
                    break;
                case 3:
                    type = "90";
                    break;
                default:
                    type = "UNDEFINED";
                    break;
            }

            // SHELLMX- Al momento de traer la informacion sobre la transaccion que hay en parte sobre un surtidor, bloquea en el TVP que Action lo este usando, Se contruye el objeto
            //          a llenar de lock para traer la demas informacion sobre la transaccion del Surtidor seleccinado.

            /*ConectionSignalRDoms conectionSignalRDoms = new ConectionSignalRDoms();
            var lockTransactionInformation = conectionSignalRDoms.LockSupplyTransactionOfFuellingPoint("");
            Salida_Obtiene_Tran salida_Obtiene_Tran;

            if (!lockTransactionInformation.Equals("ERROR"))
            {
                salida_Obtiene_Tran = new Salida_Obtiene_Tran
                {
                    Resultado = true,
                    ID_Interno = 0,
                    Msj = lockTransactionInformation,
                    Estacion = 1213,
                    Importe = 0,
                    Litros = 0,
                    Num_Operacion = 0,
                    Parcial = false,
                    PosID = 0,
                    Precio_Uni = 0,
                    Producto = ""
                };
            }
            else
            {
                LockSupplyTransactionOfFuellingPointResponse lockSupplyTransactionOfFuellingPointResponse = JsonConvert.DeserializeObject<LockSupplyTransactionOfFuellingPointResponse>(lockTransactionInformation);
                salida_Obtiene_Tran = new Salida_Obtiene_Tran
                {
                    Resultado = true,
                    ID_Interno = b,
                    Msj = "mensaje",
                    Estacion = 1213,
                    Importe = num,
                    Litros = lit,
                    Num_Operacion = b,
                    Parcial = true,
                    PosID = b + 1,
                    Precio_Uni = num,
                    Producto = type
                };
            }*/

            Salida_Obtiene_Tran salida_Obtiene_Tran = new Salida_Obtiene_Tran
            {
                Resultado = true,
                ID_Interno = b,
                Msj = "mensaje",
                Estacion = 1213,
                Importe = num,
                Litros = lit,
                Num_Operacion = b,
                Parcial = true,
                PosID = b + 1,
                Precio_Uni = num,
                Producto = type
            };
            return salida_Obtiene_Tran;
        }

        public Salida_Info_Forma_Pago Info_Forma_Pago(Entrada_Info_Forma_Pagos request)
        {
            Entrada_Info_Forma_Pagos requestNew = new Entrada_Info_Forma_Pagos
            {
                Id_Transaccion = request.Id_Transaccion,
                Info_Forma_Pago = request.Info_Forma_Pago,
                Info_Pagos_Parciales = request.Info_Pagos_Parciales,
                nHD = request.nHD,
                Pos_Carga = request.Pos_Carga,
                idpos = request.idpos,
                Id_teller = request.Id_teller,
            };
            //SHELLMX - Se verifica si es parcial la venta.
            /*if(request.parciales)
            {

            }
            else
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
            }*/
            var nticket = "";
            Random r = new Random();
            int b = r.Next(1, 100);

            nticket = "000ABC010190000" + b;

            var salida = new Salida_Info_Forma_Pago
            {
                Msj = "Entrega Satisfactoria",
                Resultado = true,
                EESS = "0007",
                Nticket = nticket,
                WebId = "02156WE545W56"
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
            var salida = new Salida_LiberaCarga
            {
                Msj = "Liberacion de bomba",
                Resultado = true
            };
            return salida;
        }

        //public async Task<Salida_getProductInfo> getProductInfo(Entrada_ValidCustumer request)
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
            }
            else if (getProductForSaleResponse.Status < 0)
            {
                salida.Resultado = false;
                salida.Msj = "Producto no existente";
                salida.producto = "";
                salida.importe = 0;
                salida.precio_Uni = 0;
                salida.mensajePromocion = "";
            }

            /*Entrada_getProductInfo requestNew = new Entrada_getProductInfo
            {
                IdProduct = request.IdProduct,
                Id_teller = request.Id_teller,
                nHD = request.nHD,
                pss = request.pss,
                PTID = request.PTID,
                idpos = request.idpos,
                Pos_Carga = request.Pos_Carga,
                serial = request.serial
            };
            Random r = new Random();
            double a = r.NextDouble();

            double num = (Math.Truncate(a * 100) / 100);

            var salida = new Salida_getProductInfo
            {
                Resultado = true,
                Msj = "Validacion corrcta",
                producto = "Producto a",
                importe = Convert.ToDecimal(num),
                precio_Uni = Convert.ToDecimal(num),
                mensajePromocion = "No exixte promocion",
            };*/
            return salida;
        }

        public async Task<Salida_Electronic_billing> Electronic_billing(Entrada_Electronic_billing request)
        {
            Entrada_Electronic_billing requestNew = new Entrada_Electronic_billing
            {
                NoCliente = request.NoCliente,
                nHD = request.nHD,
                pss = request.pss,
                PTID = request.PTID,
                Serial = request.Serial,
                idpos = request.idpos,
                Nticket = request.Nticket,
                WebID = request.WebID,
                EESS = request.EESS,
                TipoOperacion = request.TipoOperacion
            };

//se nesesitan estos datos para facturar agregamos
            var res = new ListTicketDAO {
                EESS = request.EESS,
                NTicket = request.Nticket,
                RFC = "AAA010101AAA",
                WebID = request.WebID
            };

            //despues de crear la lista agregamos los siguientes campos para finalizar el reques de consumo en facturacion 

            GenerateElectronicInvoice requestfac = new GenerateElectronicInvoice {
                EmpresaPortal = "01",
                ListTicket = new List<ListTicketDAO> { new ListTicketDAO {
                EESS =res.EESS,
                NTicket=res.NTicket,
                RFC=res.RFC,
                WebID=res.WebID} }
            };


            InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
            facresponse responsefacturacion = await invokeHubbleWebAPIServices.tpvfacturacionn(requestfac);



            Salida_Electronic_billing salida = new Salida_Electronic_billing {

                SelloDigitaSAT = responsefacturacion.SelloDigitaSAT,
                SelloDigitaCFDI=responsefacturacion.SelloDigitaCFDI,
                CadenaOrigTimbre=responsefacturacion.CadenaOrigTimbre,
                FolioFiscal = responsefacturacion.FolioFiscal,
                RFCProveedorCert = responsefacturacion.RFCProveedorCert,
                NumCertificado = responsefacturacion.NumCertificado,
                DateCertificacion=responsefacturacion.DateCertificacion
            };

            //if (responsefacturacion.SelloDigitaSAT !=null)
            //{
                
            //    salida.SelloDigitaSAT = responsefacturacion.SelloDigitaSAT;
            //    salida.SelloDigitaCFDI = responsefacturacion.SelloDigitaCFDI;
               
            //}



            //var aaa = new Salida_Electronic_billing {
            //    SelloDigitaSAT=responsefacturacion.SelloDigitaSAT


            //};













            //Random r = new Random();
            //double a = r.NextDouble();
            //int b = r.Next(100, 999);

            //double num = (Math.Truncate(a * 100) / 100);
            //Productos productos = new Productos
            //{
            //    Nombre = "Producto" + b,
            //    Cantidad = 0,
            //    Precioimporte = Convert.ToDecimal(num),
            //    Precio = Convert.ToDecimal(a),
            //    IvaProducto = Convert.ToDecimal(16)
                
                
            //};

            //var salida = new Salida_Electronic_billing
            //{
            //    Msj = "Impresion de prueva",
            //    Resultado = true,
            //    RazonSocial = "Punto Clave",
            //    RFC = "PC0001",
            //    SelloDigitaSAT = "",
            //    CadenaOrigTimbre = "",
            //    DateCertificacion = "",
            //    FolioFiscal = "",
            //    NumCertificado = "",
            //    RFCProveedorCert = "",
            //    SelloDigitaCFDI = "",
            //    CodigoPostalCompania = "37238",
            //    CodigoPostalTienda = "03310",
            //    ColoniaCompania = "CANADA DE ALFARO",
            //    ColoniaTienda = "SANTA CRUZ ATOYAC",
            //    DireccionCompania = "BLVD. JOSE MARIA MORELOS 3702",
            //    DireccionTienda = "AV. POPOCATEPETL",
            //    Estacion = "P00963",
            //    EstadoCompania = "GUANAJUATO",
            //    EstadoTienda = "CIUDAD DE MEXICO",
            //    ExpedicionTienda = "03310",
            //    FooterTick1 = "GRACIAS POR SU PREFERENCIA",
            //    FooterTick2 = "FACTURACION EN LINEA",
            //    FooterTick3 = "https//www.shell.com.mx",
            //    FooterTick4 = "Descarga FACTURAS",
            //    FooterTick5 = "facturar donde sea.",
            //    HeaderTick1 = "Clientes dudas respecto a nuestros productos y servicios?",
            //    HeaderTick2 = "Ingresa a:",
            //    HeaderTick3 = " https://support.shell.com.mx/hces-mx",
            //    HedaerTick4 = "y responde tus dudas en Soporte Shell",
            //    MunicipioCompania = "LEON",
            //    MunicipioTienda = "BENITO JUEREZ",
            //    NombreCompania = "MEGA GASOLINERAS SA DE CV",
            //    PaisCompania = "MEXICO",
            //    PaisTienda = "MEXICO",
            //    PermisoCRE = "PL/963/EXP/ES/2015",
            //    RfcCompania = "MGA110810CC3",
            //    Fecha = "",
            //    Folio = "",
            //    FormaPago = "",
            //    Iva = 0,
            //    Operador = "",
            //    Producto = productos,
            //    RegFiscal = "",
            //    Subtotal = 0,
            //    Terminal = "",
            //    Ticket = "",
            //    Tienda = "SHELL POPO",
            //    Total = 0,
            //    WebID = "",
            //    ImporteEnLetra = ""
            //};
            return salida;
        }
    }
}
