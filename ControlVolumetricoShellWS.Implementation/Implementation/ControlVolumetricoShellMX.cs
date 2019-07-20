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
using NodaTime;

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
                    Producto = lockTransactionInformation.ProductReference,
                    //Id_product = lockTransactionInformation.ProductReference
                };
            }
            return salida_Obtiene_Tran;
        }

        public async Task<Salida_Info_Forma_Pago> Info_Forma_Pago(Entrada_Info_Forma_Pagos request)
        {
            if (request.Id_teller == null)
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

            #region CONFIGURACION PARA EL DOMS
            ConectionSignalRDoms conectionSignalRDomsInform = new ConectionSignalRDoms();
            if (conectionSignalRDomsInform.StatusConectionHubbleR() < 0)
            {
                //SHELLMX- Se manda una excepccion de que no esta lleno el valor del Inform.
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "SHELLHUBLE- Fallo la conexion con el DOMS Verificar que este conectado!",
                };
            }

            #endregion

            //SHELLMX- Se crea el vaciado de la informacion de la venta.
            List<Products> InformListProducts = new List<Products>();

            List<string[]> Combustible = new List<string[]>();
            List<string[]> Products = new List<string[]>();
            List<string[]> ProcessPayments = new List<string[]>();
            List<string[]> ProcessAmountOfSale = new List<string[]>();

            List<List<string[]>> CombustibleGlobal = new List<List<string[]>>();
            List<List<string[]>> ProductsGlobal = new List<List<string[]>>();

            #region PRODUCTO QUE SE ALMACENA EN PLATAFORMA
            //Es el nombre porque no recibo la parte de del ID.
            string[] Id_product = null;
            string[] cantidad = null;

            string[] importe_Unitario = null;
            string[] importe_Total = null;
            #endregion

            #region FORMA DE PAGO Y MONTO TOTAL
            string[] forma_Pago = null;
            string[] monto_Pagado = null;
            #endregion

            #region PEfectivo
            string[] forma_PagoCombu = null;
            string[] monto_PagadoCombu = null;
            #endregion

            #region PTarjeta
            string[] forma_PagoPeri = null;
            string[] monto_PagadoPeri = null;
            #endregion

            #region listForSale
            List<string[]> ProcessPaymentsCombu = new List<string[]>();
            List<string[]> ProcessAmountOfSaleCombu = new List<string[]>();

            List<string[]> ProcessPaymentsPeri = new List<string[]>();
            List<string[]> ProcessAmountOfSalePeri = new List<string[]>();
            #endregion

            List<int> countCombustible = new List<int>();
            bool flagCountCombustible = false;

            List<int> countProducts = new List<int>();
            bool flagCountProduct = false;

            List<int> countFormaPago = new List<int>();
            //bool flagCountFormaPago = false;

            List<int> countMontoPagar = new List<int>();
            //bool flagCountMontoPagar = false;

            #region PROCESO DE SEPARACION DE LOS PRODUCTOS Y ALMACENAR LOS EN UNA LISTA
            try
            {
                foreach (Entrada_Info_Forma_Pago_List varPrincipal in request.Info_Forma_Pago)
                {
                    //Se verifica si es producto o combustible.
                    if (varPrincipal.Producto)
                    {
                        //nNum_autorizacions = varPrincipal.nNum_autorizacions.Split('|');
                        //Ultimos_Digitoss = varPrincipal.Ultimos_Digitoss.Split('|');
                        Id_product = varPrincipal.Id_product.Split('|');
                        //productosName = varPrincipal.Id_product.Split('|');
                        cantidad = varPrincipal.Cantidad.Split('|');
                        importe_Unitario = varPrincipal.Importe_Unitario.Split('|');
                        importe_Total = varPrincipal.Importetotal.Split('|');
                        //forma_Pago = varPrincipal.formapagos.Split('|');
                        //monto_Pagado = varPrincipal.montoPagadoParcial.Split('|');
                        forma_PagoCombu = varPrincipal.formapagos.Split('|');
                        monto_PagadoCombu = varPrincipal.montoPagadoParcial.Split('|');
                        //iva = varPrincipal.IvaProducto.Split('|');

                        countCombustible.Add(Id_product.Length);
                        //countCombustible.Add(productosName.Length);
                        countCombustible.Add(cantidad.Length);
                        countCombustible.Add(importe_Unitario.Length);
                        countCombustible.Add(importe_Total.Length);

                        countFormaPago.Add(forma_Pago.Length);
                        countMontoPagar.Add(monto_Pagado.Length);
                        //Combustible.Add(nNum_autorizacions);
                        //Combustible.Add(Ultimos_Digitoss);
                        Combustible.Add(Id_product);
                        //Combustible.Add(productosName);
                        Combustible.Add(cantidad);
                        Combustible.Add(importe_Unitario);
                        Combustible.Add(importe_Total);
                        //ProcessPayments.Add(forma_Pago);
                        //ProcessAmountOfSale.Add(monto_Pagado);

                        ProcessPaymentsCombu.Add(forma_PagoCombu);
                        ProcessAmountOfSaleCombu.Add(monto_PagadoCombu);
                        //Combustible.Add(iva);
                        CombustibleGlobal.Add(Combustible);
                    }
                    else
                    {
                        //nNum_autorizacions = varPrincipal.nNum_autorizacions.Split('|');
                        //Ultimos_Digitoss = varPrincipal.Ultimos_Digitoss.Split('|');
                        Id_product = varPrincipal.Id_product.Split('|');
                        //productosName = varPrincipal.Id_product.Split('|');
                        cantidad = varPrincipal.Cantidad.Split('|');
                        importe_Unitario = varPrincipal.Importe_Unitario.Split('|');
                        importe_Total = varPrincipal.Importetotal.Split('|');
                        forma_Pago = varPrincipal.formapagos.Split('|');
                        monto_Pagado = varPrincipal.montoPagadoParcial.Split('|');

                        forma_PagoPeri = varPrincipal.formapagos.Split('|');
                        monto_PagadoPeri = varPrincipal.montoPagadoParcial.Split('|');
                        //iva = varPrincipal.IvaProducto.Split('|');

                        countProducts.Add(Id_product.Length);
                        //countProducts.Add(productosName.Length);
                        countProducts.Add(cantidad.Length);
                        countProducts.Add(importe_Unitario.Length);
                        countProducts.Add(importe_Total.Length);

                        countFormaPago.Add(forma_Pago.Length);
                        countMontoPagar.Add(monto_Pagado.Length);
                        //Products.Add(nNum_autorizacions);
                        //Products.Add(Ultimos_Digitoss);
                        Products.Add(Id_product);
                        //Products.Add(productosName);
                        Products.Add(cantidad);
                        Products.Add(importe_Unitario);
                        Products.Add(importe_Total);
                        //ProcessPayments.Add(forma_Pago);
                        //ProcessAmountOfSale.Add(monto_Pagado);

                        ProcessPaymentsPeri.Add(forma_PagoPeri);
                        ProcessAmountOfSalePeri.Add(monto_PagadoPeri);
                        //Products.Add(iva);
                        ProductsGlobal.Add(Products);
                    }
                }
            } catch (Exception e)
            {
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "@SHELLMX- ERRORES EN LA ENTREGA DE Info_Forma_PagoList.",
                };
                throw e;
            }

            #region VERIFICAR LA LONGITUD QUE COINCIDAN EN LOS PRODUCTOS.
            int countUniversalCombu = 0;
            int valOldCombu = -1;
            foreach (int lengthCombu in countCombustible)
            {
                countUniversalCombu = lengthCombu;
                valOldCombu = valOldCombu == -1 ? lengthCombu : valOldCombu;
                flagCountCombustible = countUniversalCombu == valOldCombu ? true : false;
                if (!flagCountCombustible)
                {
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "@SHELLMX- NO COINCIDE LA CANTIDAD DE PRODUCTOS CARBURANTES EN LA SOLICITUD VERIFICAR",
                    };
                }
                valOldCombu = lengthCombu;
            }

            int countUniversalProduc = 0;
            int valOldProduct = -1;
            foreach (int lengthProduc in countProducts)
            {
                countUniversalProduc = lengthProduc;
                valOldProduct = valOldProduct == -1 ? lengthProduc : valOldProduct;
                flagCountProduct = countUniversalProduc == valOldProduct ? true : false;
                if (!flagCountProduct)
                {
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "@SHELLMX- NO COINCIDE LA CANTIDAD DE PRODUCTOS PERIFARICOS EN LA SOLICITUD VERIFICAR",
                    };
                }
                valOldProduct = lengthProduc;
            }

            #endregion

            #region VERIFICAR LA LONGITUD DE FORMAPAGO Y MONTOPAGAR PARA PROCESO VENTA.
            /*int countUniversalCountMontoP = 0;
            int valOldMontoP = -1;
            foreach (int lengthMontoPagar in countMontoPagar)
            {
                countUniversalCountMontoP = lengthMontoPagar;
                valOldMontoP = valOldMontoP == -1 ? lengthMontoPagar : valOldMontoP;
                flagCountMontoPagar = countUniversalCountMontoP == valOldMontoP ? true : false;
                if (!flagCountMontoPagar)
                {
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "@SHELLMX- NO COINCIDE LA CANTIDAD DE LAS MONTOPAGAR REALIZADAS EN LA SOLICITUD VERIFICAR",
                    };
                }
                valOldMontoP = lengthMontoPagar;
            }

            int countUniversalFormPago;
            int valOldFormPago = -1;
            foreach (int lengthFormPago in countFormaPago)
            {
                countUniversalFormPago = lengthFormPago;
                valOldFormPago = valOldFormPago == -1 ? lengthFormPago : valOldFormPago;
                flagCountFormaPago = countUniversalFormPago == valOldFormPago ? true : false;
                if (!flagCountFormaPago)
                {
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "@SHELLMX- NO COINCIDE LA CANTIDAD DE LAS FORMAPAGO REALIZADAS EN LA SOLICITUD VERIFICAR",
                    };
                }
                valOldFormPago = lengthFormPago;
            }*/

            #endregion



            //Se separa para poder extraer la informacion sobre los productos y almacenarlos.
            #region COMBUSTIBLE.
            try
            {
                for (int intCombu = 0; intCombu < countUniversalCombu; intCombu++)
                {
                    int flagContCombu = 0;
                    foreach (List<string[]> combustibleGlobal in CombustibleGlobal)
                    {
                        Products producto = new Products();
                        foreach (string[] combustible in Combustible)
                        {
                            for (int intValueCombustible = 0; intValueCombustible <= intCombu; intValueCombustible++)
                            {
                                switch (flagContCombu)
                                {
                                    case 0:
                                        producto.Id_producto = combustible[intValueCombustible];
                                        break;
                                    case 1:
                                        producto.Cantidad = Convert.ToDecimal(combustible[intValueCombustible]);
                                        break;
                                    case 2:
                                        producto.Importe_Unitario = Convert.ToDecimal(combustible[intValueCombustible]);
                                        break;
                                    case 3:
                                        producto.Importe_Total = Convert.ToDecimal(combustible[intValueCombustible]);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            flagContCombu++;
                        }
                        InformListProducts.Add(producto);
                        producto = null;
                    }
                }
            } catch (Exception e)
            {
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "@SHELLMX- ERRORES DE CONVERSION DE ID_PRODUCTO | CANTIDAD | IMPORTE_UNITARIO | IMPORTE_TOTAL VERIFICAR DATOS.",
                };
                throw e;
            }

            #endregion

            #region PERIFERICOS.
            try
            {
                for (int intPerife = 0; intPerife < countUniversalProduc; intPerife++)
                {
                    int flagContPerife = 0;
                    foreach (List<string[]> productsGlobal in ProductsGlobal)
                    {
                        Products producto = new Products();
                        foreach (string[] products in Products)
                        {
                            for (int intValueProducts = 0; intValueProducts <= intPerife; intValueProducts++)
                            {
                                switch (flagContPerife)
                                {
                                    case 0:
                                        producto.Id_producto = products[intValueProducts];
                                        break;
                                    case 1:
                                        producto.Cantidad = Convert.ToDecimal(products[intValueProducts]);
                                        break;
                                    case 2:
                                        producto.Importe_Unitario = Convert.ToDecimal(products[intValueProducts]);
                                        break;
                                    case 3:
                                        producto.Importe_Total = Convert.ToDecimal(products[intValueProducts]);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            flagContPerife++;
                        }
                        InformListProducts.Add(producto);
                        producto = null;
                    }
                }
            } catch (Exception e)
            {
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "@SHELLMX- ERRORES DE CONVERSION DE ID_PRODUCTO | CANTIDAD | IMPORTE_UNITARIO | IMPORTE_TOTAL VERIFICAR DATOS.",
                };
                throw e;
            }
            #endregion

            #region MONTO
            decimal TotalAmountWithTaxMonto = 0M;
            try
            {
                foreach (string[] monto in ProcessAmountOfSale)
                {
                    int countMonto = monto.Length;
                    for (int i = 0; i < countMonto; i++)
                    {
                        TotalAmountWithTaxMonto = TotalAmountWithTaxMonto + Convert.ToDecimal(monto[i]);
                    }
                }
            }catch(Exception e)
            {
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "@SHELLMX- ERRORES DE CONVERSION DE MONTOAPAGAR VERIFICAR DATOS.",
                };
                throw e;
            }

            #endregion

            #endregion

            //SHELMX- Se realiza los calculos para el iva y el total de la venta y otros procesos para CreateD.
            //decimal totalAmountWithTax;
            //decimal totalTax;
            //decimal taxableAmount;

            #region SERIE & CLIENTE CONTADO & POSID DE TPV

            string serieId = null;
            string customerId;
            string posId;
            string currencyId = null;

            GetSeriesRequest getSeriesRequest = new GetSeriesRequest { Identity = bsObj.Identity };
            GetSeriesResponse getSeriesResponse = await invokeHubbleWebAPIServices.GetSeries(getSeriesRequest);

            foreach (var series in getSeriesResponse.SeriesList)
            {
                serieId = series.Id;
            }
            GetPosInformationRequest getPosInformationRequest = new GetPosInformationRequest { Identity = bsObj.Identity };
            GetPOSInformationResponse getPOSInformationResponse = await invokeHubbleWebAPIServices.GetPOSInformation(getPosInformationRequest);
            posId = getPOSInformationResponse.PosInformation.CompanyCode + getPOSInformationResponse.PosInformation.Code;

            GetPOSConfigurationRequest getPOSConfigurationRequest = new GetPOSConfigurationRequest { Identity = bsObj.Identity };
            GetPOSConfigurationResponse getPOSConfigurationResponse = await invokeHubbleWebAPIServices.GetPOSConfiguration(getPOSConfigurationRequest);
            customerId = getPOSConfigurationResponse.UnknownCustomerId;

            #endregion

            #region HORA UTC POR LA VENTA

            string emissionLocalDateTime;
            string emissionUTCDateTime;
            //SHLMX - Se coloca la hora de la venta. 
            DateTime horaCreacionVentalocal = DateTime.Now;
            DateTime horaCreacionVentaUniversalUTC = horaCreacionVentalocal.ToUniversalTime();

            Instant instanthoraCreacionVentalocal = Instant.FromUtc(horaCreacionVentalocal.Year, horaCreacionVentalocal.Month, horaCreacionVentalocal.Day, horaCreacionVentalocal.Hour, horaCreacionVentalocal.Minute, horaCreacionVentalocal.Second);
            Instant instanthoraCreacionVentaUniversalUTC = Instant.FromUtc(horaCreacionVentaUniversalUTC.Year, horaCreacionVentaUniversalUTC.Month, horaCreacionVentaUniversalUTC.Day, horaCreacionVentaUniversalUTC.Hour, horaCreacionVentaUniversalUTC.Minute, horaCreacionVentaUniversalUTC.Second);

            emissionLocalDateTime = instanthoraCreacionVentalocal.ToString();
            emissionUTCDateTime = instanthoraCreacionVentaUniversalUTC.ToString();

            #endregion

            #region LLENADO DE UNA LISTA DE PRODUCTOS PARA CREATEDOCUMENT

            #region SE BUSCA Y SE TRANFORMA EL PAYMENTS DE LA VENTA.

            GetPaymentMethodsRequest getPaymentMethodsRequest = new GetPaymentMethodsRequest { Identity = bsObj.Identity };
            GetPaymentMethodsResponse getPaymentMethodsResponse = await invokeHubbleWebAPIServices.GetPaymentMethods(getPaymentMethodsRequest);

            GetCurrenciesRequest getCurrenciesRequest = new GetCurrenciesRequest { Identity = bsObj.Identity };
            GetCurrenciesResponse getCurrenciesResponse = await invokeHubbleWebAPIServices.GetCurrencies(getCurrenciesRequest);

            //SHLMX- Se llena el paymentsList<> con las ventas.
            List<CreateDocumentPaymentDetailDAO> PaymentDetailList = new List<CreateDocumentPaymentDetailDAO>();

            //CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();

            #region TARJETA
            //List<CreateDocumentPaymentDetailDAO> DetailsCardSale = new List<CreateDocumentPaymentDetailDAO>();
            //bool isValidFormaPagoT = false;
            foreach (string[] processPaymentsCard in ProcessPayments)
            {
                foreach (string[] processAmountOfSaleC in ProcessAmountOfSale)
                {
                    int processPaymentsCardCount = processPaymentsCard.Length;
                    int processAmountOfSaleCount = processAmountOfSaleC.Length;
                    foreach (var paymentMethods in getPaymentMethodsResponse.PaymentMethodList)
                    {
                        //CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                        if (paymentMethods.Description.ToUpper() == "TARJETA")
                        {
                            for (int i = 0; i < processPaymentsCardCount; i++)
                            {
                                CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                                foreach (var CurrenciesBase in getCurrenciesResponse.CurrencyList)
                                {
                                    if (processPaymentsCard[i].ToUpper() == "TARJETA")
                                    {
                                        if (CurrenciesBase.PriorityType == CurrencyPriorityType.Base)
                                        {
                                            if (i < processAmountOfSaleCount)
                                            {
                                                createDocumentPaymentDetailDAO.PrimaryCurrencyGivenAmount = Convert.ToDecimal(processAmountOfSaleC[i]);
                                                createDocumentPaymentDetailDAO.PrimaryCurrencyTakenAmount = Convert.ToDecimal(processAmountOfSaleC[i]);
                                                createDocumentPaymentDetailDAO.PaymentMethodId = paymentMethods.Id;
                                                createDocumentPaymentDetailDAO.CurrencyId = CurrenciesBase.Id;
                                                createDocumentPaymentDetailDAO.ChangeFactorFromBase = Convert.ToDecimal(CurrenciesBase.ChangeFactorFromBase);
                                                createDocumentPaymentDetailDAO.UsageType = CreatePaymentUsageType.PendingPayment;
                                                PaymentDetailList.Add(createDocumentPaymentDetailDAO);
                                                currencyId = CurrenciesBase.Id;
                                                //isValidFormaPagoT = true;
                                            }
                                        }
                                        //createDocumentPaymentDetailDAO = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            /*if(!isValidFormaPagoT)
            {
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "@SHELLMX- FORMA DE PAGO DESCONOCIDO VERIFICAR.",
                };
            }*/

            #endregion

            #region EFECTIVO
            //List<CreateDocumentPaymentDetailDAO> DetailsCashSale = new List<CreateDocumentPaymentDetailDAO>();
            //bool isValidFormaPagoE = false;
            foreach (string[] processPaymentsCash in ProcessPayments)
            {
                foreach (string[] processAmountOfSaleC in ProcessAmountOfSale)
                {
                    int processPaymentsCashCount = processPaymentsCash.Length;
                    int processAmountOfSaleCount = processAmountOfSaleC.Length;
                    foreach (var paymentMethods in getPaymentMethodsResponse.PaymentMethodList)
                    {
                        //CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                        if (paymentMethods.Description.ToUpper() == "EFECTIVO")
                        {
                            for (int i = 0; i < processPaymentsCashCount; i++)
                            {
                                CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                                foreach (var CurrenciesBase in getCurrenciesResponse.CurrencyList)
                                {
                                    if (processPaymentsCash[i].ToUpper() == "EFECTIVO")
                                    {
                                        if (CurrenciesBase.PriorityType == CurrencyPriorityType.Base)
                                        {
                                            if (i < processPaymentsCashCount)
                                            {
                                                createDocumentPaymentDetailDAO.PrimaryCurrencyGivenAmount = Convert.ToDecimal(processAmountOfSaleC[i]);
                                                createDocumentPaymentDetailDAO.PrimaryCurrencyTakenAmount = Convert.ToDecimal(processAmountOfSaleC[i]);
                                                createDocumentPaymentDetailDAO.PaymentMethodId = paymentMethods.Id;
                                                createDocumentPaymentDetailDAO.CurrencyId = CurrenciesBase.Id;
                                                createDocumentPaymentDetailDAO.ChangeFactorFromBase = Convert.ToDecimal(CurrenciesBase.ChangeFactorFromBase);
                                                createDocumentPaymentDetailDAO.UsageType = CreatePaymentUsageType.PendingPayment;
                                                PaymentDetailList.Add(createDocumentPaymentDetailDAO);
                                                currencyId = CurrenciesBase.Id;
                                                //isValidFormaPagoE = true;
                                            }
                                        }
                                        //createDocumentPaymentDetailDAO = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            /*if (!isValidFormaPagoE)
            {
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "@SHELLMX- FORMA DE PAGO DESCONOCIDO VERIFICAR.",
                };
            }*/

            #endregion






            #endregion

            #region SE LLENA LA LINEAS DE LA VENTA
            List<CreateDocumentLineDAO> LineList = new List<CreateDocumentLineDAO>();

            #region SE EMPIEZA A CONSTRUIR LA LISTA DE PRODUCTOS.
            Dictionary<decimal, decimal> TotalTaxListSale = new Dictionary<decimal, decimal>();
            //List<decimal[]> ListIvas = new List<decimal[]>();

            int lineNumber = 1;
            bool isValidIVAZERO = false;
            bool ZERO = true;
            foreach (Products informListProducts in InformListProducts)
            {
                CreateDocumentLineDAO createDocumentLineDAO = new CreateDocumentLineDAO();
                GetProductForSaleRequest getProductForSaleRequest = new GetProductForSaleRequest { ProductId = informListProducts.Id_producto.ToString(), Quantity = informListProducts.Cantidad, Identity = bsObj.Identity };
                GetProductForSaleResponse getProductForSaleResponse = await invokeHubbleWebAPIServices.GetProductForSale(getProductForSaleRequest);
                decimal IvaProducto = 0M;

                if (getProductForSaleResponse.TaxPercentage == Convert.ToDecimal(0))
                {
                    //SHLMX- Se contruye el product, para el arreglo.
                    createDocumentLineDAO.LineNumber = lineNumber;
                    createDocumentLineDAO.ProductId = informListProducts.Id_producto;
                    createDocumentLineDAO.Quantity = informListProducts.Cantidad;
                    createDocumentLineDAO.UnitaryPriceWithTax = informListProducts.Importe_Unitario;
                    createDocumentLineDAO.ProductName = getProductForSaleResponse.ProductName;
                    createDocumentLineDAO.TotalAmountWithTax = informListProducts.Importe_Total;
                    createDocumentLineDAO.PriceWithoutTax = informListProducts.Importe_Total;
                    //IvaProductos[0] = getProductForSaleResponse.TaxPercentage;
                    //IvaProductos[1] = getProductForSaleResponse.TaxPercentage;
                    //ListIvas.Add(IvaProductos);
                    //IvaProductos = null;
                    isValidIVAZERO = true;
                }
                else
                {
                    //SHLMX- Se contruye el product, para el arreglo.
                    createDocumentLineDAO.LineNumber = lineNumber;
                    createDocumentLineDAO.ProductId = informListProducts.Id_producto;
                    createDocumentLineDAO.Quantity = informListProducts.Cantidad;
                    createDocumentLineDAO.UnitaryPriceWithTax = informListProducts.Importe_Unitario;
                    createDocumentLineDAO.TaxPercentage = getProductForSaleResponse.TaxPercentage;

                    decimal priceWithoutTaxW = informListProducts.Importe_Total / ((getProductForSaleResponse.TaxPercentage / 100) + 1);
                    decimal priceWithoutTax = Math.Round(priceWithoutTaxW, 6);
                    createDocumentLineDAO.PriceWithoutTax = priceWithoutTax;
                    createDocumentLineDAO.TaxAmount = informListProducts.Importe_Total - createDocumentLineDAO.PriceWithoutTax;

                    createDocumentLineDAO.ProductName = getProductForSaleResponse.ProductName;
                    createDocumentLineDAO.TotalAmountWithTax = informListProducts.Importe_Total;
                    IvaProducto = Convert.ToDecimal(getProductForSaleResponse.TaxPercentage);
                    //IvaProductos[1] = informListProducts.Importe_Total - createDocumentLineDAO.PriceWithoutTax; ListIvas.Add(IvaProductos);
                    //IvaProductos = null;

                    decimal ivaaplicado = 0M;
                    foreach (Products informListPro in InformListProducts)
                    {
                        GetProductForSaleRequest getProduct = new GetProductForSaleRequest { ProductId = informListPro.Id_producto.ToString(), Quantity = informListPro.Cantidad, Identity = bsObj.Identity };
                        GetProductForSaleResponse getProductFor = await invokeHubbleWebAPIServices.GetProductForSale(getProductForSaleRequest);

                        if (IvaProducto == getProductFor.TaxPercentage)
                        {
                            decimal priceTaxW = getProductForSaleResponse.FinalAmount / ((getProductForSaleResponse.TaxPercentage / 100) + 1);
                            decimal priceWTax = Math.Round(priceWithoutTaxW, 6);
                            decimal taxAmount = informListProducts.Importe_Total - createDocumentLineDAO.PriceWithoutTax;
                            ivaaplicado = ivaaplicado + taxAmount;
                        }
                    }
                    if(ZERO)
                    {
                        TotalTaxListSale.Add(IvaProducto, ivaaplicado);
                        ZERO = false;
                    }
                    IvaProducto = 0M;
                }

                LineList.Add(createDocumentLineDAO);
                lineNumber++;
                createDocumentLineDAO = null;
            }
            if(isValidIVAZERO)
            {
                TotalTaxListSale.Add(Convert.ToDecimal(0), Convert.ToDecimal(0));
            }
            #endregion

            #endregion

            #endregion

            #region VALIDACION DE LAS IVAS
            decimal ivaTotal = 0M;
            foreach (KeyValuePair<decimal, decimal> result in TotalTaxListSale)
            {
                ivaTotal = ivaTotal + result.Value;
            }
            #endregion

            #region LLAMAR EL API DE EVERILION PARA LA VENTA
            CreateDocumentDAO createDocumentDAO = new CreateDocumentDAO {               
                ProvisionalId = 1,
                SerieId = serieId,
                EmissionLocalDateTime = emissionLocalDateTime,
                EmissionUTCDateTime = emissionUTCDateTime,
                TaxableAmount = ivaTotal,
                TotalTaxList = TotalTaxListSale,
                TotalAmountWithTax = TotalAmountWithTaxMonto,
                PaymentDetailList = PaymentDetailList,
                LineList = LineList,
                OperatorId = getOperatorResponse.Operator.Id,
                CustomerId = customerId,
                ExtraData = null,
                CurrencyId = currencyId, 
                PosId = posId,
            };
            List<CreateDocumentDAO> createDocumentDAOs = new List<CreateDocumentDAO>();
            createDocumentDAOs.Add(createDocumentDAO);
            CreateDocumentsRequest createDocumentsRequest = new CreateDocumentsRequest { CreateDAOList = createDocumentDAOs , Identity = bsObj.Identity };

            CreateDocumentsResponse createDocumentsResponse = await invokeHubbleWebAPIServices.CreateDocuments(createDocumentsRequest);

            string possibleDocumentId = null;
            foreach (KeyValuePair<int, string> resultCreateDocuments in createDocumentsResponse.ProvisionalToDefinitiveDocumentIdDictionary)
            {
                if(createDocumentsResponse.ProvisionalToDefinitiveDocumentIdDictionary.Count == 1)
                {
                    possibleDocumentId = resultCreateDocuments.Key == 1 ? resultCreateDocuments.Value : null;
                }
            }
            #endregion

            #region COMUNICACION CON EL DOMS

            var guidDOMS = Guid.NewGuid().ToString();
            FinalizeSupplyTransactionRequest finalizeSupplyTransactionRequest = new FinalizeSupplyTransactionRequest
            {
                ContactId = null,
                CustomerId = customerId,
                FuellingPointId = request.Pos_Carga,
                LineNumberInDocument = 1,
                OdometerMeasurement = null,
                OperatorId = getOperatorResponse.Operator.Id,
                PossibleDocumentId = possibleDocumentId,
                ProvisionalId = guidDOMS,
                SupplyTransactionId = Convert.ToInt32(request.Id_Transaccion),
                VehicleLicensePlate = null
            };

            FinalizeSupplyTransactionResponse finalizeSupplyTransactionResponse = conectionSignalRDomsInform.FinalizeSupplyTransactionWS(finalizeSupplyTransactionRequest);
            string supplyTransactionIdDOMS = null;

            if(finalizeSupplyTransactionResponse.Status < 0)
            {
                finalizeSupplyTransactionResponse = null;
                finalizeSupplyTransactionResponse = conectionSignalRDomsInform.FinalizeSupplyTransactionWS(finalizeSupplyTransactionRequest);
                if(finalizeSupplyTransactionResponse.Status < 0)
                {
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "SHELLHUBLE- Fallo la conexion con el DOMS Verificar que este conectado AVISAR AL ADMINSTRADOR CENTRAL!",
                    };
                }
            }
            else
            {
                foreach(KeyValuePair<string,string> resultsupplyTransactionIdDOMS in finalizeSupplyTransactionResponse.ProvisionalSupplyIdToDefinitiveSupplyIdMapping)
                {
                    supplyTransactionIdDOMS = resultsupplyTransactionIdDOMS.Value;
                }
            }
            #endregion

            #region FINALIZADO DEL DOMS Y LIBERACION DE LA BOMBA Y LA VENTA

            List<string> SupplyTransactionIdListWS = new List<string>()
            {
                supplyTransactionIdDOMS
            };
            SetDefinitiveDocumentIdForSupplyTransactionsRequest setDefinitiveDocumentIdForSupplyTransactionsRequest = new SetDefinitiveDocumentIdForSupplyTransactionsRequest
            {
                OperatorId = getOperatorResponse.Operator.Id,
                DefinitiveDocumentId = possibleDocumentId,
                SupplyTransactionIdList = SupplyTransactionIdListWS
            };

            SetDefinitiveDocumentIdForSupplyTransactionsResponse setDefinitiveDocumentIdForSupplyTransactionsResponse = conectionSignalRDomsInform.SetDefinitiveDocumentIdForSupplyTransactionsWS(setDefinitiveDocumentIdForSupplyTransactionsRequest);

            #endregion

            DateTimeOffset fechaticketstring = DateTimeOffset.Parse(emissionLocalDateTime);
            string fechaticket = Convert.ToString(fechaticketstring.DateTime);

            string nticketco = possibleDocumentId;
            string horaformatnews = fechaticket.Replace(" ", "");

            string wid = horaformatnews.Substring(10, 2);
            string wid2 = nticketco.Substring(0, 5);
            string wid3 = horaformatnews.Substring(13, 2);
            string wid4 = nticketco.Substring(5, 4);
            string wid5 = horaformatnews.Substring(16, 2);

            string webidnwe = string.Concat(wid + wid2 + wid3 + wid4 + wid5);

            if (setDefinitiveDocumentIdForSupplyTransactionsResponse.Status == 1)
            {

               return new Salida_Info_Forma_Pago
                {
                    Msj = "SHELLHUBBLE- VENTA SATISFACTORIA",
                    Resultado = true,
                    EESS = getPOSInformationResponse.PosInformation.ShopCode,
                    Nticket = possibleDocumentId,
                    WebId = webidnwe
               };
            }else if(setDefinitiveDocumentIdForSupplyTransactionsResponse.Status < 0)
            {
                return new Salida_Info_Forma_Pago
                {
                    Msj = "SHELLHUBBLE- ERROR REVISAR FALLA DE CIERRE DEL DOMS Y VENTA VERIFICAR BOS",
                    Resultado = true,
                };
            }
            return null;
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
                salida.Id_producto = getProductForSaleResponse.ProductReference;
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

                GetCompanyRequest GetCompanyreques = new GetCompanyRequest
                {
                    Identity = bsObj.Identity
                };

                //InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
                GetCompanyResponse responseCompany = await invokeHubbleWebAPIServices.GetCompany(GetCompanyreques);



                #region facturacion
                //se nesesitan estos datos para facturar agregamos
                var res = new ListTicketDAO
                {
                    EESS = request.EESS,
                    NTicket = request.Nticket,
                    //RFC = "AAA010101AAA",
                    RFC = responsecustomer.Customer.TIN,
                    WebID = request.WebID
                };

                //despues de crear la lista agregamos los siguientes campos para finalizar el reques de consumo en facturacion 

                GenerateElectronicInvoice requestfac = new GenerateElectronicInvoice
                {
                    EmpresaPortal = "01",
                    Company = responseCompany.Company.Id,
                    ListTicket = new List<ListTicketDAO>
                    {
                        new ListTicketDAO
                        {
                            EESS =res.EESS,
                            NTicket=res.NTicket,
                            RFC=res.RFC,
                            WebID=res.WebID
                        }
                    }
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


            string conletra = Convert.ToString(responsegetdocument.Document.TotalAmountWithTax);
            converletra nunletra = new converletra();
            //double numletra = Convert.ToDouble(conletra);
            string letraconvert = nunletra.enletras(conletra);

            // decimal caliva = (responsegetdocument.Document.TotalAmountWithTax) - (responsegetdocument.Document.TaxableAmount);


            //responseGetPrinting.GlobalSettings.Values;

            IList<Productos> listan = new List<Productos>();
            //lista = responsegetdocument.Document.LineList();
            foreach (DocumentLine item in responsegetdocument.Document.LineList)
            {
                listan.Add(new Productos { ProductName = item.ProductName, Quantity = Convert.ToInt32(item.Quantity), TotalAmountWithTax = (Math.Truncate(item.TotalAmountWithTax * 100) / 100).ToString("N2"), UnitaryPriceWithTax = (Math.Truncate(item.UnitaryPriceWithTax * 100) / 100).ToString("N2") });
            }

            IList<Iva> porcentaje = new List<Iva>();
            foreach (DocumentLine item in responsegetdocument.Document.LineList)
            {
                porcentaje.Add(new Iva { TaxPercentage = Convert.ToInt32(item.TaxPercentage), TaxAmount = (Math.Truncate(item.TaxAmount * 100) / 100).ToString("N2") });
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
            string salidaiva = strImprime.ToString();
            //----------------------------------------------------termina iva--------------------------------------------------------------------



            //---------------------------------------------------mrtodo de pago------------------------------------------------------------------
            IList<PaymentDetail> paymentDetails = new List<PaymentDetail>();
            //lista = responsegetdocument.Document.LineList();
            foreach (DocumentPaymentDetail item in responsegetdocument.Document.PaymentDetailList)
            {
                paymentDetails.Add(new PaymentDetail { PaymentMethodId = item.PaymentMethodId });
            }
            string metodospayment = paymentDetails.ToString();



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
