﻿using System;
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
        public async Task<Salida_Obtiene_Tran> Obtiene_Tran(Entrada_Obtiene_Tran request)
        {
            #region CODIGO MOCK
            /*
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
            };*/
            #endregion

            // SHELLMX- Al momento de traer la informacion sobre la transaccion que hay en parte sobre un surtidor, bloquea en el TVP que Action lo este usando, Se contruye el objeto
            //          a llenar de lock para traer la demas informacion sobre la transaccion del Surtidor seleccinado.

            ConectionSignalRDoms conectionSignalRDoms = new ConectionSignalRDoms();
            Salida_Obtiene_Tran salida_Obtiene_Tran;

            if (conectionSignalRDoms.StatusConectionHubbleR() < 0)
            {
                return new Salida_Obtiene_Tran
                {
                    Resultado = false,
                    Msj = "SHELLHUBLE- Fallo la conexion con el DOMS Verificar que este conectado!",
                };
            }

            //SHELLMX- Indentificamos que el Operador este registrado en el Sistema de Everilion.Shell
            // SHELLMX- Se consigue el Token del TPV para hacer las pruebas. 
            var jsonTPVToken = System.IO.File.ReadAllText("C:/dist/tpv.config.json");
            TokenTPV bsObj = JsonConvert.DeserializeObject<TokenTPV>(jsonTPVToken);

            InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
            GetOperatorRequest getOperatorRequest = new GetOperatorRequest { Id = request.Id_teller, Identity = bsObj.Identity };
            GetOperatorResponse getOperatorResponse = await invokeHubbleWebAPIServices.GetOperator(getOperatorRequest);

            if (getOperatorResponse.Operator == null)
            {
                //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                return new Salida_Obtiene_Tran
                {
                    Resultado = false,
                    Msj = "SHELLMX- ERROR OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA"
                };
            }else if(request.Pos_Carga <= 0)
            {
                //SHELLMX- Se manda una excepccion de que no corresponde el tipo de surtidor.
                return new Salida_Obtiene_Tran
                {
                    Resultado = false,
                    Msj = "SHELLMX- ERROR NO SE HA INSERTADO UN SURTIDOR NUMBER CORRECTO VERIFICAR!"
                };
            }

            GetAllSupplyTransactionsOfFuellingPointRequest getAllSupplyTransactionsOfFuellingPointRequest = new GetAllSupplyTransactionsOfFuellingPointRequest { OperatorId = request.Id_teller, FuellingPointId = request.Pos_Carga };
            LockSupplyTransactionOfFuellingPointResponse lockTransactionInformation = await conectionSignalRDoms.LockSupplyTransactionOfFuellingPoint(bsObj.Identity, getAllSupplyTransactionsOfFuellingPointRequest);
            //Salida_Obtiene_Tran salida_Obtiene_Tran;

            if (lockTransactionInformation.Status < 0)
            {
                return new Salida_Obtiene_Tran
                {
                    Resultado = false,
                    Msj = lockTransactionInformation.Message,
                };
            }
            else if (lockTransactionInformation.CorrespondingVolume == 0 && lockTransactionInformation.DiscountedAmount == 0 && lockTransactionInformation.DiscountPercentage == 0 && lockTransactionInformation.FinalAmount == 0 && lockTransactionInformation.ProductName == null && lockTransactionInformation.ProductReference == null)
            {
                return new Salida_Obtiene_Tran
                {
                    Resultado = false,
                    Msj = "@ SHELLMX - ERROR FATAL LockSupplyTransactionOfFuellingPoint IN IDTRANSACTION @145",
                };
            }
            else
            {
                var nameProduct = "";
                switch (lockTransactionInformation.GradeId)
                {
                    case 1:
                        nameProduct = "95";
                        break;
                    case 2:
                        nameProduct = "97";
                        break;
                    case 3:
                        nameProduct = "90";
                        break;
                    default:
                        break;
                }

                GetPosInformationRequest getPosInformationRequest = new GetPosInformationRequest { Identity = bsObj.Identity };
                GetPOSInformationResponse getPOSInformationResponse = await invokeHubbleWebAPIServices.GetPOSInformation(getPosInformationRequest);
                salida_Obtiene_Tran = new Salida_Obtiene_Tran
                {
                    Resultado = true,
                    ID_Interno = lockTransactionInformation.Id,
                    Msj = lockTransactionInformation.Message,
                    Estacion = Convert.ToInt32(getPOSInformationResponse.PosInformation.ShopCode),
                    Importe = lockTransactionInformation.FinalAmount,
                    Litros = lockTransactionInformation.CorrespondingVolume,
                    Num_Operacion = request.Pos_Carga,
                    Parcial = false,
                    PosID = Convert.ToInt32(getPOSInformationResponse.PosInformation.Code),
                    Precio_Uni = lockTransactionInformation.GradeUnitPrice,
                    IvaPorcentaje = lockTransactionInformation.TaxPercentage,
                    Producto = nameProduct,
                };
            }
            return salida_Obtiene_Tran;
        }

        public async Task<Salida_Info_Forma_Pago> Info_Forma_Pago(Entrada_Info_Forma_Pagos request)
        {
            /*Entrada_Info_Forma_Pagos requestNew = new Entrada_Info_Forma_Pagos
            {
                Id_Transaccion = request.Id_Transaccion,
                Info_Forma_Pago = request.Info_Forma_Pago,
                Info_Pagos_Parciales = request.Info_Pagos_Parciales,
                nHD = request.nHD,
                Pos_Carga = request.Pos_Carga,
                idpos = request.idpos,
                Id_teller = request.Id_teller,
            };*/

            if(request.Id_teller == null)
            {
                //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "SHELLMX- ERROR OPERADOR ESTA VACIO EN LA ENTRADA",
                };
            }

            //SHELLMX- Indentificamos que el Operador este registrado en el Sistema de Everilion.Shell
            // SHELLMX- Se consigue el Token del TPV para hacer las pruebas. 
            var jsonTPVToken = System.IO.File.ReadAllText("C:/dist/tpv.config.json");
            TokenTPV bsObj = JsonConvert.DeserializeObject<TokenTPV>(jsonTPVToken);

            InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
            GetOperatorRequest getOperatorRequest = new GetOperatorRequest { Id = request.Id_teller, Identity = bsObj.Identity };
            GetOperatorResponse getOperatorResponse = await invokeHubbleWebAPIServices.GetOperator(getOperatorRequest);

            if(getOperatorResponse.Operator == null)
            {
                //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "SHELLMX- ERROR OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA"
                };
            }

            //SHELLMX- Se verifica el parcial parar poder almacenar en la Plataforma.

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
            // SHELLMX- Se manda a consumir el Identity del POS a activar.
            var jsonTPVToken = System.IO.File.ReadAllText("C:/dist/tpv.config.json");
            TokenTPV bsObj = JsonConvert.DeserializeObject<TokenTPV>(jsonTPVToken);

            //ConectionSignalRDoms conectionSignalRDoms = new ConectionSignalRDoms();
            //await conectionSignalRDoms.CancelAuthorizationOfFuellingPoint("", 4);

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
            InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
            Salida_Electronic_billing salida = new Salida_Electronic_billing();

            var jsonTPVToken = System.IO.File.ReadAllText("C:/dist/tpv.config.json");
            TokenTPV bsObj = JsonConvert.DeserializeObject<TokenTPV>(jsonTPVToken);
            bool isFacturar = false;

            if(request.NoCliente == null)
            {
                isFacturar = false;
            }
            if (request.TipoOperacion == 0)
            {
                return salida;
            }

            #region cliente
            GetCustomerRequest resquestcustomer = new GetCustomerRequest
            {
                Id = request.NoCliente,
                Identity = bsObj.Identity
            };

            //InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
            GetCustomerResponse responsecustomer = await invokeHubbleWebAPIServices.GetCustomer(resquestcustomer);

            if (responsecustomer.Customer.BusinessName == null && responsecustomer.Customer.TIN == null)
            {
                isFacturar = false;
            }
    
            #endregion
            if (request.TipoOperacion == 1)
            {
                isFacturar = false;
                salida.RazonSocial = responsecustomer.Customer.BusinessName;
                salida.RFC = responsecustomer.Customer.TIN;
            }else if(request.TipoOperacion == 2)
            {
                isFacturar = true;
                salida.RazonSocial = responsecustomer.Customer.BusinessName;
                salida.RFC = responsecustomer.Customer.TIN;
            }

            if (isFacturar)
            {
                #region facturacion
                //se nesesitan estos datos para facturar agregamos
                var res = new ListTicketDAO
                {
                    EESS = request.EESS,
                    NTicket = request.Nticket,
                    RFC = "AAA010101AAA",
                    WebID = request.WebID
                };

                //despues de crear la lista agregamos los siguientes campos para finalizar el reques de consumo en facturacion 

                GenerateElectronicInvoice requestfac = new GenerateElectronicInvoice
                {
                    EmpresaPortal = "01",
                    ListTicket = new List<ListTicketDAO> { new ListTicketDAO {
                EESS =res.EESS,
                NTicket=res.NTicket,
                RFC=res.RFC,
                WebID=res.WebID} }
                };



                facresponse responsefacturacion = await invokeHubbleWebAPIServices.tpvfacturacionn(requestfac);
                if (responsefacturacion == null)
                {
                    return new Salida_Electronic_billing();
                }
                else
                {
                    salida.SelloDigitaSAT = responsefacturacion.SelloDigitaSAT;
                    salida.SelloDigitaCFDI = responsefacturacion.SelloDigitaCFDI;
                    salida.CadenaOrigTimbre = responsefacturacion.CadenaOrigTimbre;
                    salida.FolioFiscal = responsefacturacion.FolioFiscal;
                    salida.RFCProveedorCert = responsefacturacion.RFCProveedorCert;
                    salida.NumCertificado = responsefacturacion.NumCertificado;
                    salida.DateCertificacion = responsefacturacion.DateCertificacion;
                }
                #endregion
            }
          
            #region GetDocument

            //despues de crear la lista agregamos los siguientes campos para finalizar el reques de consumo en facturacion 
                GetDocumentRequest requesgetdocument = new GetDocumentRequest
                {
                    Identity = bsObj.Identity,
                    Id = request.Nticket,
                    UsageType = DocumentUsageType.PrintCopy,
                };

                GetDocumentResponse responsegetdocument = await invokeHubbleWebAPIServices.GetDocument(requesgetdocument);

            if(responsegetdocument.Document.Id == null && responsegetdocument.Document.OperatorId == null && responsegetdocument.Document.LineList == null)
            {
                salida.FormaPago = null;
                salida.Subtotal = 0;
                salida.Terminal = null;
                salida.Operador = null;
                salida.Total = 0;
            }


            string nticketorigin = responsegetdocument.Document.Id;
            int ntiquetn = nticketorigin.Length;
            string Folioidticket = nticketorigin.Substring((ntiquetn - 9), 9);


            decimal conletra = responsegetdocument.Document.TotalAmountWithTax;

            double numletra = Convert.ToDouble(conletra);
            string letraconvert = converletra.numbersToLetter(numletra);

            decimal caliva = (responsegetdocument.Document.TotalAmountWithTax) - (responsegetdocument.Document.TaxableAmount);


            //responseGetPrinting.GlobalSettings.Values;

            IList<Productos> listan = new List<Productos>();
            //lista = responsegetdocument.Document.LineList();
            foreach (DocumentLine item in responsegetdocument.Document.LineList)
            {
                listan.Add(new Productos { ProductName = item.ProductName, Quantity = item.Quantity, TotalAmountWithTax = item.TotalAmountWithTax, UnitaryPriceWithTax = item.UnitaryPriceWithTax });
            }



            string formatofecha = Convert.ToString(responsegetdocument.Document.EmissionLocalDateTime);
            DateTimeOffset fechaticketstring = DateTimeOffset.Parse(formatofecha);
            string fechaticket = Convert.ToString(fechaticketstring.DateTime);

            //string horaorig = "2019 - 07 - 12T10: 28:50";

            //DateTimeOffset formatoffset = DateTimeOffset.Parse(horaorig);
            //string horaformatnew = Convert.ToString(formatoffset.DateTime);


            string nticketco = responsegetdocument.Document.Id;
            string horaformatnews = fechaticket.Replace(" ", "");

            string wid = horaformatnews.Substring(10, 2);
            string wid2 = nticketco.Substring(0, 5);
            string wid3 = horaformatnews.Substring(13, 2);
            string wid4 = nticketco.Substring(5, 4);
            string wid5 = horaformatnews.Substring(16, 2);

            string webidnwe = string.Concat(wid + wid2 + wid3 + wid4 + wid5);
            #endregion


            #region getprintingconfiguration

            //despues de crear la lista agregamos los siguientes campos para finalizar el reques de consumo en facturacion 
            GetPrintingConfigurationRequest requesGetPrinting = new GetPrintingConfigurationRequest
            {
                Identity = bsObj.Identity,
                UsecasesPrintingConfigurationList = new List<UsecasePrintingConfiguration> {
                    new UsecasePrintingConfiguration {
                        UseCase="SALE",
                        PrintingTemplatePlatformType= "VENTA HUBBLEPOS",
                        DefaultNumberOfCopies= 1,
                        Required= true
                    },
                    new UsecasePrintingConfiguration {
                        UseCase= "CLOSURE",
                        PrintingTemplatePlatformType= "CIERRE DE CAJA",
                        DefaultNumberOfCopies= 1,
                        Required= true
                    },
                    new UsecasePrintingConfiguration {
                          UseCase= "CASHBOX_RECORD",
                          PrintingTemplatePlatformType= "MOVIMIENTO DE CAJA HUBBLEPOS",
                          DefaultNumberOfCopies= 1,
                          Required= true
                    },
                    new UsecasePrintingConfiguration {
                          UseCase= "FUELLINGPOINT_TEST",
                          PrintingTemplatePlatformType= "SURTIDOR HUBBLEPOS",
                          DefaultNumberOfCopies= 1,
                          Required= false
                    },
                    new UsecasePrintingConfiguration {
                          UseCase= "REFUND",
                          PrintingTemplatePlatformType= "ANULACION HUBBLEPOS",
                          DefaultNumberOfCopies= 1,
                          Required= true
                    },
                    new UsecasePrintingConfiguration {
                        UseCase= "RUNAWAY",
                        PrintingTemplatePlatformType= "FUGA HUBBLEPOS",
                        DefaultNumberOfCopies= 1,
                        Required= true


                    },
                    new UsecasePrintingConfiguration {
                        UseCase= "COLLECTION",
                        PrintingTemplatePlatformType= "JUSTIFICANTE DE PAGO",
                        DefaultNumberOfCopies= 1,
                        Required= false
                    }

                }
            };



            GetPrintingConfigurationResponse responseGetPrinting = await invokeHubbleWebAPIServices.GetPrintingConfiguration(requesGetPrinting);

            #region recorrido del diccionario
            //object ret = responseGetPrinting.GlobalSettings.Keys;

            //if (responseGetPrinting.GlobalSettings == null)
            //{
            //    foreach (var keys in responseGetPrinting.GlobalSettings.Keys)
            //    {
            //        if (responseGetPrinting.GlobalSettings != null)
            //        {
            //            string a = responseGetPrinting.GlobalSettings[keys];

            //            if (a == "text03")
            //            {
            //                ret = new { a };
            //            }


            //        }

            //    }
            //}
            #endregion


            List<string> listaPrinting = new List<string>();
            string key;
            foreach (var item in responseGetPrinting.GlobalSettings)
            {
                key = item.Value;
                listaPrinting.Add(key);
            }
            listaPrinting.ToArray();

            //string Headerprin = listaPrinting[1];

            Headeresponse deserializeJsonheader = JsonConvert.DeserializeObject<Headeresponse>(listaPrinting[1]);
            footeresponse deserializeJsonfooter = JsonConvert.DeserializeObject<footeresponse>(listaPrinting[2]);



            // var responseGlobalSettings = responseGetPrinting.GlobalSettings.Values;


            //string ess = Count[1]=responseGlobalSettings.Values;
            #endregion

            #region information
            //// GetPOSInformationResponse  GetPOSInformation(GetPosInformationRequest getPosInformationRequest
            GetPosInformationRequest getPosInformationRequest = new GetPosInformationRequest {
               Identity = bsObj.Identity };

            //   //   InvokeHubbleWebAPIServices invokeHubbleWebAPIServices3 = new InvokeHubbleWebAPIServices();
            //InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
            GetPOSInformationResponse informationresponses = await invokeHubbleWebAPIServices.GetPOSInformation(getPosInformationRequest);

            #endregion

            salida.HeaderTick1 = deserializeJsonheader.Header1;
            salida.HeaderTick2 = deserializeJsonheader.Header2;
            salida.HeaderTick3 = deserializeJsonheader.Header3;
            salida.HedaerTick4 = deserializeJsonheader.Header4;
            salida.FooterTick1 = deserializeJsonfooter.Footer1;
            salida.FooterTick2 = deserializeJsonfooter.Footer2;
            salida.FooterTick3 = deserializeJsonfooter.Footer3;
            salida.FooterTick4 = deserializeJsonfooter.Footer4;
            salida.FooterTick5 = deserializeJsonfooter.Footer5;
            salida.CodigoPostalCompania = listaPrinting[17];
            salida.CodigoPostalTienda = listaPrinting[27];
            salida.ColoniaCompania = listaPrinting[16];
            salida.ColoniaTienda = listaPrinting[29];
            salida.DireccionCompania = listaPrinting[14];
            salida.DireccionTienda = listaPrinting[24];
            salida.EstadoCompania = listaPrinting[19];
            salida.EstadoTienda = listaPrinting[31];
            salida.ExpedicionTienda = listaPrinting[27];
            salida.MunicipioCompania = listaPrinting[16];
            salida.MunicipioTienda = listaPrinting[26];
            salida.NombreCompania = listaPrinting[15];
            salida.PaisCompania = listaPrinting[20];
            salida.PaisTienda = listaPrinting[30];
            salida.PermisoCRE = listaPrinting[32];
            salida.Tienda = listaPrinting[33];
            salida.RegFiscal = "REGIMEN GENERAL DE LEY PERSONAS MORALES";
            salida.RfcCompania = listaPrinting[5];
            salida.Ticket = request.Nticket;
            salida.FormaPago = responsegetdocument.Document.PaymentDetailList[0].PaymentMethodId;
            salida.Subtotal = responsegetdocument.Document.TaxableAmount;
            salida.Terminal = responsegetdocument.Document.PosId;
            salida.Operador = responsegetdocument.Document.OperatorName;
            salida.Folio = Folioidticket;
            salida.Total = responsegetdocument.Document.TotalAmountWithTax;
            salida.ImporteEnLetra = letraconvert;
            salida.Iva = caliva;
            salida.productos = listan;
            salida.Fecha = fechaticket;
            salida.WebID = webidnwe;
            salida.Estacion = informationresponses.PosInformation.ShopCode;

            #region comentarios
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
             //   Producto = productos,
            //    RegFiscal = "",
            //    Subtotal = 0,
            //    Terminal = "",
            //    Ticket = "",
            //    Tienda = "SHELL POPO",
            //    Total = 0,
            //    WebID = "",
            //    ImporteEnLetra = ""
            //};
            #endregion


            return salida;
        }
    }
}
