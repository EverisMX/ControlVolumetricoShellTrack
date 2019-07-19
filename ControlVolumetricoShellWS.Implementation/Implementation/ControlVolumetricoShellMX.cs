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
    public class ControlVolumetricoShellMX : IControlVolumetricoShellMX
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
                    Msj = "SHELLMX- OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA"
                };
            }else if(request.Pos_Carga <= 0)
            {
                //SHELLMX- Se manda una excepccion de que no corresponde el tipo de surtidor.
                return new Salida_Obtiene_Tran
                {
                    Resultado = false,
                    Msj = "SHELLMX- NO SE HA INSERTADO UN SURTIDOR NUMBER CORRECTO VERIFICAR!"
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
                    Msj = "@ SHELLMX - Transaccion Bloqueada por otra Terminal: LockSupplyTransactionOfFuellingPoint IN IDTRANSACTION @145",
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
                    Msj = "@ SHELLMX-  OPERADOR ESTA VACIO EN LA ENTRADA VALIDAR",
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
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "SHELLMX- ERROR OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA"
                };
            }

            if (request.Info_Forma_Pago == null || request.Info_Forma_Pago.Count == 0)
            {
                //SHELLMX- Se manda una excepccion de que no esta lleno el valor del Inform.
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "@ SHELLMX- INFORM DE LA VENTA VACIO CARGAR LOS DATOS DE VENTA",
                };
            }

            //SHELLMX- Se crea el vaciado de la informacion de la venta.
            List<Products> InformListProducts = new List<Products>();
            List<string[]> Products = new List<string[]>();
            List<List<string[]>> ProductsGlobal = new List<List<string[]>>();
            string[] productosName = null;
            string[] cantidad = null;
            string[] importe_Unitario = null;
            string[] importe_Total = null;
            string[] forma_Pago = null;
            string[] monto_Pagado = null;
            string[] iva = null;

            foreach (Entrada_Info_Forma_Pago_List varPrincipal in request.Info_Forma_Pago)
            {
                productosName = varPrincipal.Id_product.Split('|');
                cantidad = varPrincipal.Cantidad.Split('|');
                importe_Unitario = varPrincipal.Importe_Unitario.Split('|');
                importe_Total = varPrincipal.Importetotal.Split('|');
                forma_Pago = varPrincipal.formapagos.Split('|');
                monto_Pagado = varPrincipal.montoPagadoParcial.Split('|');
                iva = varPrincipal.IvaProducto.Split('|');
                Products.Add(productosName);
                Products.Add(cantidad);
                Products.Add(importe_Unitario);
                Products.Add(importe_Total);
                Products.Add(forma_Pago);
                Products.Add(monto_Pagado);
                Products.Add(iva);
                ProductsGlobal.Add(Products);
            }
            int countProductoTotals = productosName.Length;

            for(int iteratorProducts = 0; iteratorProducts < countProductoTotals; iteratorProducts++)
            {
                int flagContProduct = 0;
                foreach (List<string[]> productsGlobals in ProductsGlobal)
                {
                    Products producto = new Products();
                    foreach (string[] product in Products)
                    {
                        for (int intValueProducts = 0; intValueProducts <= iteratorProducts; intValueProducts++)
                        {
                            switch(flagContProduct)
                            {
                                case 0:
                                    producto.Id_producto = Convert.ToInt32(product[intValueProducts]);
                                    break;
                                case 1:
                                    producto.Cantidad = Convert.ToDecimal(product[intValueProducts]);
                                    break;
                                case 2:
                                    producto.Importe_Unitario = Convert.ToDecimal(product[intValueProducts]);
                                    break;
                                case 3:
                                    producto.Importe_Total = Convert.ToDecimal(product[intValueProducts]);
                                    break;
                                case 4:
                                    producto.Forma_Pago = product[intValueProducts];
                                    break;
                                case 5:
                                    producto.Monto_Pagado = Convert.ToDecimal(product[intValueProducts]);
                                    break;
                                case 6:
                                    producto.Iva_producto = Convert.ToInt32(product[intValueProducts]);
                                    break;
                                default:
                                    break;
                            }
                        }
                        flagContProduct++;
                    }
                    InformListProducts.Add(producto);
                    producto = null;
                }
            }
            //END del Bucle de los productos.

            //SHELMX- Se realiza los calculos para el iva y el total de la venta y otros procesos para CreateD.
            //decimal totalAmountWithTax;
            //decimal totalTax;
            //string emissionLocalDateTime;
            //string emissionUTCDateTime;
            string serieId;
            //decimal taxableAmount;
            string customerId;
            string posId;

            GetSeriesRequest getSeriesRequest = new GetSeriesRequest { Identity = bsObj.Identity };
            GetSeriesResponse getSeriesResponse = await invokeHubbleWebAPIServices.GetSeries(getSeriesRequest);

            foreach(var series in getSeriesResponse.SeriesList)
            {
                serieId = series.Id;
            }
            GetPosInformationRequest getPosInformationRequest = new GetPosInformationRequest { Identity = bsObj.Identity };
            GetPOSInformationResponse getPOSInformationResponse = await invokeHubbleWebAPIServices.GetPOSInformation(getPosInformationRequest);
            posId = getPOSInformationResponse.PosInformation.CashboxCode + getPOSInformationResponse.PosInformation.Code;

            GetPOSConfigurationRequest getPOSConfigurationRequest = new GetPOSConfigurationRequest { Identity = bsObj.Identity };
            GetPOSConfigurationResponse getPOSConfigurationResponse = await invokeHubbleWebAPIServices.GetPOSConfiguration(getPOSConfigurationRequest);
            customerId = getPOSConfigurationResponse.UnknownCustomerId;

            //SHLMX - Se coloca la hora de la venta. 
            DateTime horaCreacionVentalocal = DateTime.Now;
            DateTime horaCreacionVentaUniversalUTC = horaCreacionVentalocal.ToUniversalTime();

           // Instant instant = Instant.FromUtc(horaconver.Year, horaconver.Month, horaconver.Day, horaconver.Hour, horaconver.Minute, horaconver.Second);
         

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

        public async Task<Salida_getProductInfo> getProductInfo(Entrada_getProductInfo request)
        {
            // SHELLMX- Se consigue el Token del TPV para hacer las pruebas. 
            var jsonTPVToken = System.IO.File.ReadAllText("C:/dist/tpv.config.json");
            TokenTPV bsObj = JsonConvert.DeserializeObject<TokenTPV>(jsonTPVToken);

            InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
            GetOperatorRequest getOperatorRequest = new GetOperatorRequest { Id = request.Id_teller, Identity = bsObj.Identity };
            GetOperatorResponse getOperatorResponse = await invokeHubbleWebAPIServices.GetOperator(getOperatorRequest);

            if (getOperatorResponse.Operator == null)
            {
                //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                return new Salida_getProductInfo
                {
                    Resultado = false,
                    Msj = "SHELLMX- OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA"
                };
            }

            GetProductForSaleRequest getProductForSaleRequest = new GetProductForSaleRequest()
            {
                ProductId = request.IdProduct,
                Quantity = 1,
                Identity = bsObj.Identity,
            };
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
            textosincaracterspc textosincarspecial = new textosincaracterspc();

            var jsonTPVToken = System.IO.File.ReadAllText("C:/dist/tpv.config.json");
            TokenTPV bsObj = JsonConvert.DeserializeObject<TokenTPV>(jsonTPVToken);
            bool isFacturar = false;

            if (request.NoCliente == null)
            {
                isFacturar = false;
                salida.Msj = "INTRODUSCA UN NUMERO DE CLIENTE";
                salida.Resultado = false;
                return salida;
            }
            if (request.TipoOperacion == 0)
            {
                salida.Msj = "INTRODUSCA UNA OPERACION VALIDA PORFAVOR";
                salida.Resultado = false;
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
                salida.Resultado = false;
                salida.Msj = "NO SE PUDO ENCONTRAR LA INFORMACION DEL CLIENTE";
                return salida;
            }

            #endregion
            if (request.TipoOperacion == 1)
            {
                isFacturar = false;
                salida.RazonSocial = textosincarspecial.transformtext(responsecustomer.Customer.BusinessName);
                salida.RFC = responsecustomer.Customer.TIN;
                salida.Resultado = true;
                salida.Msj = "OPERACION REALIZADA CON EXITO";
            }
            else if (request.TipoOperacion == 2)
            {
                isFacturar = true;
                salida.RazonSocial = textosincarspecial.transformtext(responsecustomer.Customer.BusinessName);
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
                    //RFC = "AAA010101AAA",
                    RFC = "XAXX010101000",
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
                    salida.Msj = "NO SE PUDO REALIZARR LA FACTURACION INTENTELO MAS TARDE";
                    salida.Resultado = false;
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
                    salida.Msj = "FACTURACION EXITOSA";
                    salida.Resultado = true;

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

            if (responsegetdocument.Document.Id == null && responsegetdocument.Document.OperatorId == null && responsegetdocument.Document.LineList == null)
            {
                salida.FormaPago = null;
                salida.Subtotal = null;
                salida.Terminal = null;
                salida.Operador = null;
                salida.Total = null;
            }


            string nticketorigin = responsegetdocument.Document.Id;
            int ntiquetn = nticketorigin.Length;
            string Folioidticket = nticketorigin.Substring((ntiquetn - 9), 9);


            string conletra =Convert.ToString(responsegetdocument.Document.TotalAmountWithTax);
            converletra nunletra = new converletra();
            //double numletra = Convert.ToDouble(conletra);
            string letraconvert = nunletra.enletras(conletra);

           // decimal caliva = (responsegetdocument.Document.TotalAmountWithTax) - (responsegetdocument.Document.TaxableAmount);


            //responseGetPrinting.GlobalSettings.Values;

            IList<Productos> listan = new List<Productos>();
            //lista = responsegetdocument.Document.LineList();
            foreach (DocumentLine item in responsegetdocument.Document.LineList)
            {
                listan.Add(new Productos { ProductName = item.ProductName, Quantity =Convert.ToInt32(item.Quantity), TotalAmountWithTax = (Math.Truncate(item.TotalAmountWithTax * 100) / 100).ToString("N2") , UnitaryPriceWithTax =  (Math.Truncate(item.UnitaryPriceWithTax * 100) / 100).ToString("N2") });
            }

            IList<Iva> porcentaje = new List<Iva>();
            foreach (DocumentLine item in responsegetdocument.Document.LineList)
            {
                porcentaje.Add(new Iva { TaxPercentage=Convert.ToInt32( item.TaxPercentage), TaxAmount= (Math.Truncate(item.TaxAmount * 100)/100).ToString("N2")});
            }

//--------------------------------empieza iva ------------------------------------------------------------------------------
            string strImprime = String.Empty;
            int recorreUnicoIva = 0;
            string[] taxes;
            taxes = new string[porcentaje.Count()];
            IList<IvaUnico> ivaUnico = new List<IvaUnico>();
            int cuenta = 0;
            decimal decSumaIva = 0;
            foreach (var item in porcentaje)
            {
                if (Array.IndexOf(taxes, item.TaxPercentage.ToString()) == -1)
                {
                    ivaUnico.Add(new IvaUnico { Iva = item.TaxPercentage });
                    taxes[cuenta] = item.TaxPercentage.ToString();
                }
                cuenta += 1;
            }
            foreach (var item in ivaUnico)
            {
                foreach (var item2 in porcentaje)
                {
                    if (item.Iva.ToString() == item2.TaxPercentage.ToString())
                        decSumaIva += decimal.Parse(item2.TaxAmount);
                }
                if (recorreUnicoIva == 0)
                {
                    strImprime = "IVA " + item.Iva.ToString() + "%:     " + (Math.Truncate(decSumaIva * 100) / 100).ToString("N2");
                }
                else
                    strImprime += " | IVA " + item.Iva.ToString() + "%:     " + (Math.Truncate(decSumaIva * 100) / 100).ToString("N2");

                decSumaIva = 0;
                recorreUnicoIva += 1;
            }
            string salidaiva=strImprime.ToString();
            //----------------------------------------------------termina iva--------------------------------------------------------------------



            //---------------------------------------------------mrtodo de pago------------------------------------------------------------------
            IList<PaymentDetail> paymentDetails = new List<PaymentDetail>();
            //lista = responsegetdocument.Document.LineList();
            foreach (DocumentPaymentDetail item in responsegetdocument.Document.PaymentDetailList)
            {
                paymentDetails.Add(new PaymentDetail { PaymentMethodId = item.PaymentMethodId});
            }
           string metodospayment= paymentDetails.ToString();



            string[] arraymetodopago = new string[paymentDetails.Count];
            int i = 0;
            foreach (PaymentDetail item in paymentDetails)
            {
                arraymetodopago[i++] = item.PaymentMethodId;
            }

            string metodopago = String.Join(" | ", arraymetodopago);

            metodopago = metodopago.Replace("0393301", "EFECTIVO").Replace("0393308", "TARJETA").Replace("0393309", "FUGA").Replace("0393310", "PROPINA");


            //---------------------------------------------------termina metodo de pago---------------------------------------------------------







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
            GetPosInformationRequest getPosInformationRequest = new GetPosInformationRequest
            {
                Identity = bsObj.Identity
            };

            //   //   InvokeHubbleWebAPIServices invokeHubbleWebAPIServices3 = new InvokeHubbleWebAPIServices();
            //InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
            GetPOSInformationResponse informationresponses = await invokeHubbleWebAPIServices.GetPOSInformation(getPosInformationRequest);

            #endregion

            salida.HeaderTick1 = textosincarspecial.transformtext(deserializeJsonheader.Header1);
            salida.HeaderTick2 = textosincarspecial.transformtext(deserializeJsonheader.Header2);
            salida.HeaderTick3 = deserializeJsonheader.Header3;
            salida.HedaerTick4 = textosincarspecial.transformtext(deserializeJsonheader.Header4);
            salida.FooterTick1 = textosincarspecial.transformtext(deserializeJsonfooter.Footer1);
            salida.FooterTick2 = textosincarspecial.transformtext(deserializeJsonfooter.Footer2);
            salida.FooterTick3 = textosincarspecial.transformtext(deserializeJsonfooter.Footer3);
            salida.FooterTick4 = textosincarspecial.transformtext(deserializeJsonfooter.Footer4);
            salida.FooterTick5 = deserializeJsonfooter.Footer5;
            salida.CodigoPostalCompania = textosincarspecial.transformtext(listaPrinting[17]);
            salida.CodigoPostalTienda = textosincarspecial.transformtext(listaPrinting[27]);
            salida.ColoniaCompania = textosincarspecial.transformtext(listaPrinting[16]);
            salida.ColoniaTienda = textosincarspecial.transformtext(listaPrinting[29]);
            salida.DireccionCompania = textosincarspecial.transformtext(listaPrinting[14]);
            salida.DireccionTienda = textosincarspecial.transformtext(listaPrinting[24]);
            salida.EstadoCompania = textosincarspecial.transformtext(listaPrinting[19]);
            salida.EstadoTienda = textosincarspecial.transformtext(listaPrinting[31]);
            salida.ExpedicionTienda = textosincarspecial.transformtext(listaPrinting[27]);
            salida.MunicipioCompania = textosincarspecial.transformtext(listaPrinting[16]);
            salida.MunicipioTienda = textosincarspecial.transformtext(listaPrinting[26]);
            salida.NombreCompania = textosincarspecial.transformtext(listaPrinting[15]);
            salida.PaisCompania = textosincarspecial.transformtext(listaPrinting[20]);
            salida.PaisTienda = textosincarspecial.transformtext(listaPrinting[30]);
            salida.PermisoCRE = listaPrinting[32];
            salida.Tienda = textosincarspecial.transformtext(listaPrinting[33]);
            salida.RegFiscal = "REGIMEN GENERAL DE LEY PERSONAS MORALES";
            salida.RfcCompania = textosincarspecial.transformtext(listaPrinting[5]);
            salida.Ticket = request.Nticket;
            salida.FormaPago = metodopago;//"EFECTIVO"; //(responsegetdocument.Document.PaymentDetailList[0].PaymentMethodId);//pendiente por modificar
            salida.Subtotal = (Math.Truncate(responsegetdocument.Document.TaxableAmount * 100) / 100).ToString("N2");
            salida.Terminal = responsegetdocument.Document.PosId;
            salida.Operador = responsegetdocument.Document.OperatorName;
            salida.Folio = Folioidticket;
            salida.Total = (Math.Truncate(responsegetdocument.Document.TotalAmountWithTax * 100) / 100).ToString("N2");
            salida.ImporteEnLetra = letraconvert;
            salida.iva = salidaiva;
            salida.productos = listan;
            salida.Fecha = fechaticket;
            salida.WebID = webidnwe;
            salida.Estacion = informationresponses.PosInformation.ShopCode;




            return salida;
        }
    }
}
