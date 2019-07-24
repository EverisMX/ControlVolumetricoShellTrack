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
            #region VALIDACIONES DE LAS ENTRADAS DE OBTENER_TRAN
            try
            {
                if (request.Pos_Carga < 0)
                {
                    //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                    return new Salida_Obtiene_Tran
                    {
                        Resultado = false,
                        Msj = "SHELLMX- DEBE DE INSERTAR UN SURTIDOR QUE ESTA LIGADO!!"
                    };
                }
            }
            catch (Exception e)
            {
                //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                return new Salida_Obtiene_Tran
                {
                    Resultado = false,
                    Msj = "SHELLMX- DEBE DE INTRODUCIR EL FORMATO CORRECTO DE SURTIDOR NUMERO!!"
                };
                throw e;
            }
            if (request.nHD < 0 || request.idpos == null)
            {
                //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                return new Salida_Obtiene_Tran
                {
                    Resultado = false,
                    Msj = "SHELLMX- DEBE DE INSERTAR VALORES VALIDOS CON EL FORMATO nHD | idpos .. Verificar!!"
                };
            }
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
            #endregion

            //SHELLMX- Indentificamos que el Operador este registrado en el Sistema de Everilion.Shell
            // SHELLMX- Se consigue el Token del TPV para hacer las pruebas. 
            var jsonTPVToken = System.IO.File.ReadAllText("C:/dist/tpv.config.json");
            TokenTPV bsObj = JsonConvert.DeserializeObject<TokenTPV>(jsonTPVToken);
            InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();

            //GetOperatorRequest getOperatorRequest = new GetOperatorRequest { Id = request.Id_teller, Identity = bsObj.Identity };
            //GetOperatorResponse getOperatorResponse = await invokeHubbleWebAPIServices.GetOperator(getOperatorRequest);

            /*if (getOperatorResponse.Operator == null)
            {
                //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                return new Salida_Obtiene_Tran
                {
                    Resultado = false,
                    Msj = "SHELLMX- OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA"
                };
            }*/

            #region VALIDACION DEL OPERADOR ID | CODE
            List<SearchOperatorCriteria> SearchOperatorCriteriaOperator = new List<SearchOperatorCriteria>
            {
                new SearchOperatorCriteria{ Text = request.Id_teller , Field = SearchOperatorCriteriaFieldType.Id , MatchingType = SearchOperatorCriteriaMatchingType.Exact },
                new SearchOperatorCriteria{ Text = request.Id_teller , Field = SearchOperatorCriteriaFieldType.Code , MatchingType = SearchOperatorCriteriaMatchingType.Exact }
            };
            SearchOperatorRequest searchOperatorRequest = new SearchOperatorRequest
            {
                Identity = bsObj.Identity,
                CriteriaList = SearchOperatorCriteriaOperator,
                CriteriaRelationshipType = SearchCriteriaRelationshipType.Or,
                MustIncludeDischarged = false
            };

            SearchOperatorResponse searchOperatorResponse = await invokeHubbleWebAPIServices.SearchOperator(searchOperatorRequest);
            string idOperatorObtTran = null;
            string codeOperatorOntTran = null;

            if (searchOperatorResponse.OperatorList.Count == 0)
            {
                //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                return new Salida_Obtiene_Tran
                {
                    Resultado = false,
                    Msj = "SHELLMX- OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA"
                };
            }
            foreach (var searchOperator in searchOperatorResponse.OperatorList)
            {
                if (searchOperatorResponse.OperatorList.Count == 1)
                {
                    idOperatorObtTran = searchOperator.Id;
                    codeOperatorOntTran = searchOperator.Code;
                }
            }

            #endregion

            GetAllSupplyTransactionsOfFuellingPointRequest getAllSupplyTransactionsOfFuellingPointRequest = new GetAllSupplyTransactionsOfFuellingPointRequest { OperatorId = idOperatorObtTran, FuellingPointId = request.Pos_Carga };
            LockSupplyTransactionOfFuellingPointResponse lockTransactionInformation = await conectionSignalRDoms.LockSupplyTransactionOfFuellingPoint(bsObj.Identity, getAllSupplyTransactionsOfFuellingPointRequest);

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
                    Producto = Convert.ToString(lockTransactionInformation.GradeId),      //lockTransactionInformation.ProductReference,
                    //Id_product = lockTransactionInformation.ProductReference
                };
            }
            return salida_Obtiene_Tran;
        }

        public async Task<Salida_Info_Forma_Pago> Info_Forma_Pago(Entrada_Info_Forma_Pagos request)
        {
            if (request.Id_Transaccion == null)
            {
                //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "@ SHELLMX-  NUMERO DE TRANSACCION VACIO INTRODUCIR EL SURTIDOR !!",
                };
            }
            try
            {
                if(Convert.ToInt32(request.Id_Transaccion) <= 0)
                {
                    //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "@ SHELLMX-  INTRODUCIR UN NUMERO DE SURTIDOR VALIDO. !!",
                    };
                }
            }catch(Exception e)
            {
                //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "@ SHELLMX- NO ES UN VALOR VALIDO ID_TRANSACTION VERIFICAR! LOG :: "
                };
                throw e;
            }

            try
            {
                if (request.idpos == null || request.nHD <= 0 || request.PorpagarEntrada <= -1)
                {
                    //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "@ SHELLMX- DATOS VALIDOS EN IDPOS | nHD | PorpagarEntrada VALIDAR !!",
                    };
                }
            }catch(Exception e)
            {
                //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "@ SHELLMX- DATOS CON FORMATO INCORRECTO EN IDPOS | nHD | PorpagarEntrada VALIDAR !!",
                };
                throw e;
            }
            
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

            //GetOperatorRequest getOperatorRequest = new GetOperatorRequest { Id = request.Id_teller, Identity = bsObj.Identity };
            //GetOperatorResponse getOperatorResponse = await invokeHubbleWebAPIServices.GetOperator(getOperatorRequest);

            /*if (getOperatorResponse.Operator == null)
            {
                //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "SHELLMX- ERROR OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA"
                };
            }*/

            #region VALIDACION DEL OPERADOR ID | CODE
            List<SearchOperatorCriteria> SearchOperatorCriteriaOperator = new List<SearchOperatorCriteria>
            {
                new SearchOperatorCriteria{ Text = request.Id_teller , Field = SearchOperatorCriteriaFieldType.Id , MatchingType = SearchOperatorCriteriaMatchingType.Exact },
                new SearchOperatorCriteria{ Text = request.Id_teller , Field = SearchOperatorCriteriaFieldType.Code , MatchingType = SearchOperatorCriteriaMatchingType.Exact }
            };
            SearchOperatorRequest searchOperatorRequest = new SearchOperatorRequest {
                Identity = bsObj.Identity,
                CriteriaList = SearchOperatorCriteriaOperator,
                CriteriaRelationshipType = SearchCriteriaRelationshipType.Or,
                MustIncludeDischarged = false
            };

            SearchOperatorResponse searchOperatorResponse = await invokeHubbleWebAPIServices.SearchOperator(searchOperatorRequest);
            string idOperator = null;
            string codeOperator = null;

            if(searchOperatorResponse.OperatorList.Count == 0)
            {
                //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "SHELLMX- ERROR OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA"
                };
            }
            foreach(var searchOperator in searchOperatorResponse.OperatorList)
            {
                if(searchOperatorResponse.OperatorList.Count == 1)
                {
                    idOperator = searchOperator.Id;
                    codeOperator = searchOperator.Code;
                }
            }

            #endregion


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

            GetPosInformationRequest getPosInformationRequest = new GetPosInformationRequest { Identity = bsObj.Identity };
            GetPOSInformationResponse getPOSInformationResponse = await invokeHubbleWebAPIServices.GetPOSInformation(getPosInformationRequest);

            #region SE VALIDA EL ID_TRANSACTION QUE CORRESPONDA AL SURTIDOR

            GetAllSupplyTransactionsOfFuellingPointRequest getAllSupplyTransactionsOfFuellingPoint = new GetAllSupplyTransactionsOfFuellingPointRequest
            {
                OperatorId = idOperator,
                FuellingPointId = request.Pos_Carga
            };

            int[] validateFuellingPointO = conectionSignalRDomsInform.ValidateSupplyTransactionOfFuellingPoint(bsObj.Identity, getAllSupplyTransactionsOfFuellingPoint);
            if (validateFuellingPointO[0] <= 0)
            {
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "SHELLMX- EL ID_TRANSACTION NO EXISTE EN EL SURTIDOR INTENTAR NUEVAMENTE.!",
                };
            }
            if (validateFuellingPointO[1] <= -1)
            {
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "SHELLMX- NO EXISTE UN ID DE BLOQUEO EN EL SURTIDOR OBTENER LA INFORMACION DEL SURTIDOR PARA SER BLOQUEADO.!",
                };
            }

            if (validateFuellingPointO[0] != Convert.ToInt32(request.Id_Transaccion))
            {
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "SHELLMX- EL ID_TRANSACTION NO CORRESPONDE CON EL ID DEL SURTIDOR EN TURNO INTENTAR NUEVAMENTE CON EL CORRECTO.!",
                };
            }

            try
            {
                if (validateFuellingPointO[1] != Convert.ToInt32(getPOSInformationResponse.PosInformation.Code))
                {
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "SHELLMX- EL ID dE BLOQUEO DEL SURTIDR NO CORRESPONDE CON EL POSID INTENTAR NUEVAMENTE CON EL CORRECTO.!",
                    };
                }
            }
            catch (Exception e)
            {
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "SHELLMX- EL POSID NO ES UN NUMERICO INTENTAR NUEVAMENTE CON EL CORRECTO.!",
                };
                throw e;
            }

            #endregion

            #endregion

            #region VALIDACION SOBRE EL PARCIAL DE LA ENTRADA.
            if(request.parciales)
            {
                //SHELLMX- Se manda una excepccion de que no esta lleno el valor del Inform.
                return new Salida_Info_Forma_Pago
                {
                    Resultado = true,
                    Msj = "@ SHELLMX- INFORM VAliDACION DE PRIMERA ENTRADA"
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

                        //countFormaPago.Add(forma_PagoCombu.Length);
                        //countMontoPagar.Add(monto_PagadoCombu.Length);
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

                        //countFormaPago.Add(forma_Pago.Length);
                        //countMontoPagar.Add(monto_Pagado.Length);
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
                    Msj = "@SHELLMX- ERRORES EN LA ENTREGA DE Info_Forma_PagoList. LOG :: "
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
            //Se agrega el IDCOMPANY para que se haga la venta segura y no truene.
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
                                        producto.Id_producto = getPOSInformationResponse.PosInformation.CompanyCode +  combustible[intValueCombustible];
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
                //OIL
                foreach (string[] montoComb in ProcessAmountOfSaleCombu)
                {
                    int countMontoComb = montoComb.Length;
                    for (int i = 0; i < countMontoComb; i++)
                    {
                        TotalAmountWithTaxMonto = TotalAmountWithTaxMonto + Convert.ToDecimal(montoComb[i]);
                    }
                }
                //PaymentSale
                foreach (string[] montoPeri in ProcessAmountOfSalePeri)
                {
                    int countMontoPeri = montoPeri.Length;
                    for (int i = 0; i < countMontoPeri; i++)
                    {
                        TotalAmountWithTaxMonto = TotalAmountWithTaxMonto + Convert.ToDecimal(montoPeri[i]);
                    }
                }
            }
            catch(Exception e)
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
            string serieWebId = null;
            string customerId;
            string posId;
            string currencyId = null;

            GetSeriesRequest getSeriesRequest = new GetSeriesRequest { Identity = bsObj.Identity };
            GetSeriesResponse getSeriesResponse = await invokeHubbleWebAPIServices.GetSeries(getSeriesRequest);

            foreach (var series in getSeriesResponse.SeriesList)
            {
                serieId = series.Id;
                serieWebId = series.Code;
            }
            
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
            List<CreateDocumentPaymentDetailDAO> PaymentDetailListPreview = new List<CreateDocumentPaymentDetailDAO>();

            //CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
            string paymentCash = null;
            string paymentCard = null;
            string paymentany = null;

            #region OIL STATION
            //List<CreateDocumentPaymentDetailDAO> DetailsCardSale = new List<CreateDocumentPaymentDetailDAO>();
            //bool isValidFormaPagoT = false;
            try
            {
                foreach (string[] processPaymentsCombu in ProcessPaymentsCombu)
                {
                    foreach (string[] processAmountOfSaleCombu in ProcessAmountOfSaleCombu)
                    {
                        int processPaymentsCombuCount = processPaymentsCombu.Length;
                        int processAmountOfSaleCount = processAmountOfSaleCombu.Length;
                        foreach (var paymentMethods in getPaymentMethodsResponse.PaymentMethodList)
                        {
                            //CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                            if (paymentMethods.Description.ToUpper() == "TARJETA")
                            {
                                for (int i = 0; i < processPaymentsCombuCount; i++)
                                {
                                    CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                                    foreach (var CurrenciesBase in getCurrenciesResponse.CurrencyList)
                                    {
                                        if (Convert.ToInt32(processPaymentsCombu[i]) == 32) //if (processPaymentsCombu[i].ToUpper() == "TARJETA")
                                        {
                                            if (CurrenciesBase.PriorityType == CurrencyPriorityType.Base)
                                            {
                                                if (i < processAmountOfSaleCount)
                                                {
                                                    createDocumentPaymentDetailDAO.PrimaryCurrencyGivenAmount = Convert.ToDecimal(processAmountOfSaleCombu[i]);
                                                    createDocumentPaymentDetailDAO.PrimaryCurrencyTakenAmount = Convert.ToDecimal(processAmountOfSaleCombu[i]);
                                                    createDocumentPaymentDetailDAO.PaymentMethodId = paymentMethods.Id;
                                                    createDocumentPaymentDetailDAO.CurrencyId = CurrenciesBase.Id;
                                                    createDocumentPaymentDetailDAO.ChangeFactorFromBase = Convert.ToDecimal(CurrenciesBase.ChangeFactorFromBase);
                                                    createDocumentPaymentDetailDAO.UsageType = CreatePaymentUsageType.PendingPayment;
                                                    PaymentDetailListPreview.Add(createDocumentPaymentDetailDAO);
                                                    currencyId = CurrenciesBase.Id;
                                                    paymentCard = paymentMethods.Id;
                                                }
                                            }
                                            //createDocumentPaymentDetailDAO = null;
                                        }
                                    }
                                }
                            }
                            if (paymentMethods.Description.ToUpper() == "EFECTIVO")
                            {
                                for (int i = 0; i < processPaymentsCombuCount; i++)
                                {
                                    CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                                    foreach (var CurrenciesBase in getCurrenciesResponse.CurrencyList)
                                    {
                                        if (Convert.ToInt32(processPaymentsCombu[i]) == 3) //if (processPaymentsCombu[i].ToUpper() == "EFECTIVO")
                                        {
                                            if (CurrenciesBase.PriorityType == CurrencyPriorityType.Base)
                                            {
                                                if (i < processAmountOfSaleCount)
                                                {
                                                    createDocumentPaymentDetailDAO.PrimaryCurrencyGivenAmount = Convert.ToDecimal(processAmountOfSaleCombu[i]);
                                                    createDocumentPaymentDetailDAO.PrimaryCurrencyTakenAmount = Convert.ToDecimal(processAmountOfSaleCombu[i]);
                                                    createDocumentPaymentDetailDAO.PaymentMethodId = paymentMethods.Id;
                                                    createDocumentPaymentDetailDAO.CurrencyId = CurrenciesBase.Id;
                                                    createDocumentPaymentDetailDAO.ChangeFactorFromBase = Convert.ToDecimal(CurrenciesBase.ChangeFactorFromBase);
                                                    createDocumentPaymentDetailDAO.UsageType = CreatePaymentUsageType.PendingPayment;
                                                    PaymentDetailListPreview.Add(createDocumentPaymentDetailDAO);
                                                    currencyId = CurrenciesBase.Id;
                                                    paymentCash = paymentMethods.Id;
                                                }
                                            }
                                            //createDocumentPaymentDetailDAO = null;
                                        }
                                    }
                                }
                            }
                            if (paymentMethods.Description.ToUpper() == "VARIOS")
                            {
                                for (int i = 0; i < processPaymentsCombuCount; i++)
                                {
                                    CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                                    foreach (var CurrenciesBase in getCurrenciesResponse.CurrencyList)
                                    {
                                        if (Convert.ToInt32(processPaymentsCombu[i]) != 3 && Convert.ToInt32(processPaymentsCombu[i]) != 32)
                                        {
                                            if (CurrenciesBase.PriorityType == CurrencyPriorityType.Base)
                                            {
                                                if (i < processAmountOfSaleCount)
                                                {
                                                    createDocumentPaymentDetailDAO.PrimaryCurrencyGivenAmount = Convert.ToDecimal(processAmountOfSaleCombu[i]);
                                                    createDocumentPaymentDetailDAO.PrimaryCurrencyTakenAmount = Convert.ToDecimal(processAmountOfSaleCombu[i]);
                                                    createDocumentPaymentDetailDAO.PaymentMethodId = paymentMethods.Id;
                                                    createDocumentPaymentDetailDAO.CurrencyId = CurrenciesBase.Id;
                                                    createDocumentPaymentDetailDAO.ChangeFactorFromBase = Convert.ToDecimal(CurrenciesBase.ChangeFactorFromBase);
                                                    createDocumentPaymentDetailDAO.UsageType = CreatePaymentUsageType.PendingPayment;
                                                    PaymentDetailListPreview.Add(createDocumentPaymentDetailDAO);
                                                    currencyId = CurrenciesBase.Id;
                                                    paymentany = paymentMethods.Id;
                                                }
                                            }
                                            //createDocumentPaymentDetailDAO = null;
                                        }
                                    }
                                }
                            }//end
                        }
                    }
                }
            }catch(Exception e)
            {
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "@SHELLMX- ERRORES DE CONVERSION EN MONTOPAGO O FOMAPAGO NO TIENE EL IDCATALOGO ADECUADO VERIFICAR!! LOG:: "
                };
                throw e;
            }

            #endregion

            #region PERIPHERICS STATION
            //List<CreateDocumentPaymentDetailDAO> DetailsCashSale = new List<CreateDocumentPaymentDetailDAO>();
            //bool isValidFormaPagoE = false;
            try
            {
                foreach (string[] processPaymentsPeri in ProcessPaymentsPeri)
                {
                    foreach (string[] processAmountOfSalePeri in ProcessAmountOfSalePeri)
                    {
                        int processPaymentsPeriCount = processPaymentsPeri.Length;
                        int processAmountOfSaleCount = processAmountOfSalePeri.Length;
                        foreach (var paymentMethods in getPaymentMethodsResponse.PaymentMethodList)
                        {
                            //CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                            if (paymentMethods.Description.ToUpper() == "TARJETA")
                            {
                                for (int i = 0; i < processPaymentsPeriCount; i++)
                                {
                                    CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                                    foreach (var CurrenciesBase in getCurrenciesResponse.CurrencyList)
                                    {
                                        if (Convert.ToInt32(processPaymentsPeri[i]) == 32) //if (processPaymentsPeri[i].ToUpper() == "TARJETA")
                                        {
                                            if (CurrenciesBase.PriorityType == CurrencyPriorityType.Base)
                                            {
                                                if (i < processAmountOfSaleCount)
                                                {
                                                    createDocumentPaymentDetailDAO.PrimaryCurrencyGivenAmount = Convert.ToDecimal(processAmountOfSalePeri[i]);
                                                    createDocumentPaymentDetailDAO.PrimaryCurrencyTakenAmount = Convert.ToDecimal(processAmountOfSalePeri[i]);
                                                    createDocumentPaymentDetailDAO.PaymentMethodId = paymentMethods.Id;
                                                    createDocumentPaymentDetailDAO.CurrencyId = CurrenciesBase.Id;
                                                    createDocumentPaymentDetailDAO.ChangeFactorFromBase = Convert.ToDecimal(CurrenciesBase.ChangeFactorFromBase);
                                                    createDocumentPaymentDetailDAO.UsageType = CreatePaymentUsageType.PendingPayment;
                                                    PaymentDetailListPreview.Add(createDocumentPaymentDetailDAO);
                                                    currencyId = CurrenciesBase.Id;
                                                    paymentCard = paymentMethods.Id;                                                }
                                            }
                                            //createDocumentPaymentDetailDAO = null;
                                        }
                                    }
                                }
                            }
                            if (paymentMethods.Description.ToUpper() == "EFECTIVO")
                            {
                                for (int i = 0; i < processPaymentsPeriCount; i++)
                                {
                                    CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                                    foreach (var CurrenciesBase in getCurrenciesResponse.CurrencyList)
                                    {
                                        if (Convert.ToInt32(processPaymentsPeri[i]) == 3) //if (processPaymentsPeri[i].ToUpper() == "EFECTIVO")
                                        {
                                            if (CurrenciesBase.PriorityType == CurrencyPriorityType.Base)
                                            {
                                                if (i < processAmountOfSaleCount)
                                                {
                                                    createDocumentPaymentDetailDAO.PrimaryCurrencyGivenAmount = Convert.ToDecimal(processAmountOfSalePeri[i]);
                                                    createDocumentPaymentDetailDAO.PrimaryCurrencyTakenAmount = Convert.ToDecimal(processAmountOfSalePeri[i]);
                                                    createDocumentPaymentDetailDAO.PaymentMethodId = paymentMethods.Id;
                                                    createDocumentPaymentDetailDAO.CurrencyId = CurrenciesBase.Id;
                                                    createDocumentPaymentDetailDAO.ChangeFactorFromBase = Convert.ToDecimal(CurrenciesBase.ChangeFactorFromBase);
                                                    createDocumentPaymentDetailDAO.UsageType = CreatePaymentUsageType.PendingPayment;
                                                    PaymentDetailListPreview.Add(createDocumentPaymentDetailDAO);
                                                    currencyId = CurrenciesBase.Id;
                                                    paymentCash = paymentMethods.Id;
                                                }
                                            }
                                            //createDocumentPaymentDetailDAO = null;
                                        }
                                    }
                                }
                            }
                            if (paymentMethods.Description.ToUpper() == "VARIOS")
                            {
                                for (int i = 0; i < processPaymentsPeriCount; i++)
                                {
                                    CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                                    foreach (var CurrenciesBase in getCurrenciesResponse.CurrencyList)
                                    {
                                        if (Convert.ToInt32(processPaymentsPeri[i]) != 3 && Convert.ToInt32(processPaymentsPeri[i]) != 32)
                                        {
                                            if (CurrenciesBase.PriorityType == CurrencyPriorityType.Base)
                                            {
                                                if (i < processAmountOfSaleCount)
                                                {
                                                    createDocumentPaymentDetailDAO.PrimaryCurrencyGivenAmount = Convert.ToDecimal(processAmountOfSalePeri[i]);
                                                    createDocumentPaymentDetailDAO.PrimaryCurrencyTakenAmount = Convert.ToDecimal(processAmountOfSalePeri[i]);
                                                    createDocumentPaymentDetailDAO.PaymentMethodId = paymentMethods.Id;
                                                    createDocumentPaymentDetailDAO.CurrencyId = CurrenciesBase.Id;
                                                    createDocumentPaymentDetailDAO.ChangeFactorFromBase = Convert.ToDecimal(CurrenciesBase.ChangeFactorFromBase);
                                                    createDocumentPaymentDetailDAO.UsageType = CreatePaymentUsageType.PendingPayment;
                                                    PaymentDetailListPreview.Add(createDocumentPaymentDetailDAO);
                                                    currencyId = CurrenciesBase.Id;
                                                    paymentany = paymentMethods.Id; ;
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
            }catch(Exception e)
            {
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "@SHELLMX- ERRORES DE CONVERSION EN MONTOPAGO O FOMAPAGO NO TIENE EL IDCATALOGO ADECUADO VERIFICAR!! LOG:: "
                };
                throw e;
            }

            #endregion

            #region PREPARE OF UNIFIQUE PAYMENTS OF THE DOCUMENTS
            List<CreateDocumentPaymentDetailDAO> PaymentDetailList = new List<CreateDocumentPaymentDetailDAO>();
            bool iscash = false;
            bool isCard = false;
            bool isAny = false;

            bool cashT = false;
            bool cardT = false;
            bool anyT = false;
            foreach (var paymentsU in PaymentDetailListPreview)
            {
                decimal paymentIncrementyGivenAmount = 0M;
                decimal paymentIncrementyTakenAmount = 0M;
                foreach (var paymentsU1 in PaymentDetailListPreview)
                {
                    if(paymentCash == paymentsU.PaymentMethodId && paymentsU1.PaymentMethodId == paymentCash)
                    {
                        paymentIncrementyGivenAmount = paymentIncrementyGivenAmount + paymentsU1.PrimaryCurrencyGivenAmount;
                        paymentIncrementyTakenAmount = paymentIncrementyTakenAmount + paymentsU1.PrimaryCurrencyTakenAmount;
                        isCard = false;
                        iscash = true;
                        isAny = false;
                    }
                    if (paymentCard == paymentsU.PaymentMethodId && paymentsU1.PaymentMethodId == paymentCard)
                    {
                        paymentIncrementyGivenAmount = paymentIncrementyGivenAmount + paymentsU1.PrimaryCurrencyGivenAmount;
                        paymentIncrementyTakenAmount = paymentIncrementyTakenAmount + paymentsU1.PrimaryCurrencyTakenAmount;
                        isCard = true;
                        iscash = false;
                        isAny = false;
                    }
                    if (paymentany == paymentsU.PaymentMethodId && paymentsU1.PaymentMethodId == paymentany)
                    {
                        paymentIncrementyGivenAmount = paymentIncrementyGivenAmount + paymentsU1.PrimaryCurrencyGivenAmount;
                        paymentIncrementyTakenAmount = paymentIncrementyTakenAmount + paymentsU1.PrimaryCurrencyTakenAmount;
                        isCard = false;
                        iscash = false;
                        isAny = true;
                    }
                }
                if(iscash && !cashT)
                {
                    paymentsU.PrimaryCurrencyGivenAmount = paymentIncrementyGivenAmount;
                    paymentsU.PrimaryCurrencyTakenAmount = paymentIncrementyTakenAmount;
                    PaymentDetailList.Add(paymentsU);
                    cashT = true;
                }
                if(isCard && !cardT)
                {
                    paymentsU.PrimaryCurrencyGivenAmount = paymentIncrementyGivenAmount;
                    paymentsU.PrimaryCurrencyTakenAmount = paymentIncrementyTakenAmount;
                    PaymentDetailList.Add(paymentsU);
                    cardT = true;
                }
                if(isAny && !anyT)
                {
                    paymentsU.PrimaryCurrencyGivenAmount = paymentIncrementyGivenAmount;
                    paymentsU.PrimaryCurrencyTakenAmount = paymentIncrementyTakenAmount;
                    PaymentDetailList.Add(paymentsU);
                    anyT = true;
                }
            }

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
            try
            {
                foreach (Products informListProducts in InformListProducts)
                {
                    CreateDocumentLineDAO createDocumentLineDAO = new CreateDocumentLineDAO();
                    GetProductForSaleRequest getProductForSaleRequest = new GetProductForSaleRequest { ProductId = informListProducts.Id_producto.ToString(), Quantity = informListProducts.Cantidad, Identity = bsObj.Identity };
                    GetProductForSaleResponse getProductForSaleResponse = await invokeHubbleWebAPIServices.GetProductForSale(getProductForSaleRequest);

                    if (getProductForSaleResponse.Status < 0)
                    {
                        return new Salida_Info_Forma_Pago
                        {
                            Resultado = false,
                            Msj = "@SHELLMX- LOS PRODUCTOS INTRODUCIDOS NO EXISTEN O SON INCORRECTOS VERIFICAR!!"
                        };
                    }
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
                        if (ZERO)
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
                if (isValidIVAZERO)
                {
                    TotalTaxListSale.Add(Convert.ToDecimal(0), Convert.ToDecimal(0));
                }
            }
            catch(Exception e)
            {
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "@SHELLMX- LOS DATOS INTRODUCIDOS NO TIENEN EL FORMATO ESPECIFICO VERIFICAR E INTERTAR NUEVAMENTE! lOG:: "
                };
                throw e;
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
                OperatorId = idOperator,
                CustomerId = customerId,
                ExtraData = null,
                CurrencyId = currencyId, 
                PosId = posId,
            };
            List<CreateDocumentDAO> createDocumentDAOs = new List<CreateDocumentDAO>();
            createDocumentDAOs.Add(createDocumentDAO);
            CreateDocumentsRequest createDocumentsRequest = new CreateDocumentsRequest { CreateDAOList = createDocumentDAOs , Identity = bsObj.Identity };
            CreateDocumentsResponse createDocumentsResponse = await invokeHubbleWebAPIServices.CreateDocuments(createDocumentsRequest);
            if(createDocumentsResponse.Status < 0)
            {
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "@SHELLMX- FALLO EN PROCESO INTERNO HUBBLE NO SE ALMACENO EN BDEVERILION REINTENTAR Y VERIFICAR LOS DATOS CORRECTOS DE ENTRADA ::"
                };
            }

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
                OperatorId = idOperator,
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
                        Msj = "SHELLHUBLE- FALLO LA CONEXION DE CERRAR LA BOMBA EN DOMS PROBLEMAS DE DUPLICIDAD O FALLOS AL SURTIDOR VERIFICARLOS! LOG:: "
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
                OperatorId =idOperator,
                DefinitiveDocumentId = possibleDocumentId,
                SupplyTransactionIdList = SupplyTransactionIdListWS
            };

            SetDefinitiveDocumentIdForSupplyTransactionsResponse setDefinitiveDocumentIdForSupplyTransactionsResponse = conectionSignalRDomsInform.SetDefinitiveDocumentIdForSupplyTransactionsWS(setDefinitiveDocumentIdForSupplyTransactionsRequest);
            if (setDefinitiveDocumentIdForSupplyTransactionsResponse.Status < 0)
            {
                return new Salida_Info_Forma_Pago
                {
                    Resultado = false,
                    Msj = "SHELLHUBLE- FALLO EN LA LIBERACION DE BOMBA VERIFICAR SURTIDOR EN TPV! LOG:: " + finalizeSupplyTransactionResponse.Message,
                };
            }

            #endregion

            DateTimeOffset fechaTicketSale = DateTimeOffset.Parse(emissionLocalDateTime);
            string fechaTicketFact = Convert.ToString(fechaTicketSale.DateTime);
            string horaFormatFact = fechaTicketFact.Replace(" ", "");

            string hourWebID = horaFormatFact.Substring(10, 2);
            string companyEESS = getPOSInformationResponse.PosInformation.CompanyCode;
            string minutWebID = horaFormatFact.Substring(13, 2);
            string serieTicket = serieWebId;
            string secontWebID = horaFormatFact.Substring(16, 2);

            string webIDFact = string.Concat(hourWebID + companyEESS + minutWebID + serieTicket + secontWebID);

            if (setDefinitiveDocumentIdForSupplyTransactionsResponse.Status == 1)
            {
               return new Salida_Info_Forma_Pago
                {
                    Msj = "SHELLHUBBLE- VENTA SATISFACTORIA",
                    Resultado = true,
                    EESS = getPOSInformationResponse.PosInformation.ShopCode,
                    Nticket = possibleDocumentId,
                    WebId = webIDFact
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
            try
            {
                if (request.Pos_Carga < 0)
                {
                    //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                    return new Salida_getProductInfo
                    {
                        Resultado = false,
                        Msj = "SHELLMX- DEBE DE INSERTAR UN SURTIDOR QUE ESTA LIGADO!!"
                    };
                }
            }catch(Exception e)
            {
                //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                return new Salida_getProductInfo
                {
                    Resultado = false,
                    Msj = "SHELLMX- DEBE DE INTRODUCIR EL FORMATO CORRECTO DE SURTIDOR NUMERO!!"
                };
                throw e;
            }

            // SHELLMX- Se consigue el Token del TPV para hacer las pruebas. 
            var jsonTPVToken = System.IO.File.ReadAllText("C:/dist/tpv.config.json");
            TokenTPV bsObj = JsonConvert.DeserializeObject<TokenTPV>(jsonTPVToken);
            InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();

            //GetOperatorRequest getOperatorRequest = new GetOperatorRequest { Id = request.Id_teller, Identity = bsObj.Identity };
            //GetOperatorResponse getOperatorResponse = await invokeHubbleWebAPIServices.GetOperator(getOperatorRequest);
            //if (getOperatorResponse.Operator == null)
            //{
            //    //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
            //    return new Salida_getProductInfo
            //    {
            //        Resultado = false,
            //        Msj = "SHELLMX- OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA"
            //    };
            //}

            #region VALIDACION DEL OPERADOR ID | CODE
            List<SearchOperatorCriteria> SearchOperatorCriteriaOperator = new List<SearchOperatorCriteria>
            {
                new SearchOperatorCriteria{ Text = request.Id_teller , Field = SearchOperatorCriteriaFieldType.Id , MatchingType = SearchOperatorCriteriaMatchingType.Exact },
                new SearchOperatorCriteria{ Text = request.Id_teller , Field = SearchOperatorCriteriaFieldType.Code , MatchingType = SearchOperatorCriteriaMatchingType.Exact }
            };
            SearchOperatorRequest searchOperatorRequest = new SearchOperatorRequest
            {
                Identity = bsObj.Identity,
                CriteriaList = SearchOperatorCriteriaOperator,
                CriteriaRelationshipType = SearchCriteriaRelationshipType.Or,
                MustIncludeDischarged = false
            };

            SearchOperatorResponse searchOperatorResponse = await invokeHubbleWebAPIServices.SearchOperator(searchOperatorRequest);
            string idOperatorObtTran = null;
            string codeOperatorOntTran = null;

            if (searchOperatorResponse.OperatorList.Count == 0)
            {
                //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                return new Salida_getProductInfo
                {
                    Resultado = false,
                    Msj = "SHELLMX- OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA"
                };
            }
            foreach (var searchOperator in searchOperatorResponse.OperatorList)
            {
                if (searchOperatorResponse.OperatorList.Count == 1)
                {
                    idOperatorObtTran = searchOperator.Id;
                    codeOperatorOntTran = searchOperator.Code;
                }
            }

            #endregion

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
            }
            return salida;
        }

        public async Task<Salida_Electronic_billing> Electronic_billing(Entrada_Electronic_billing request)
        {
            #region globales
            InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
            Salida_Electronic_billing salida = new Salida_Electronic_billing();
            textosincaracterspc textosincarspecial = new textosincaracterspc();

            var jsonTPVToken = System.IO.File.ReadAllText("C:/dist/tpv.config.json");
            TokenTPV bsObj = JsonConvert.DeserializeObject<TokenTPV>(jsonTPVToken);
            bool isFacturar = false;
            #endregion


            if (request.EESS == null && request.NoCliente == null && request.Nticket == null && request.WebID == null)
            {
                salida.Msj = "DATOS INCORRECTOS";
                salida.Resultado = false;
            }

            if (request.EESS == null)
            {
                isFacturar = false;
                salida.Msj = "INTRODUSCA UN NUMERO DE ESTACION";
                salida.Resultado = false;
                return salida;
            }
            /* if (request.NoCliente == null)
             {
                 isFacturar = false;
                 salida.Msj = "INTRODUSCA UN NUMERO DE CLIENTE";
                 salida.Resultado = false;
                 return salida;
             }*/
            if (request.Nticket == null)
            {
                isFacturar = false;
                salida.Msj = "INTRODUSCA UN NUMERO DE TICKET";
                salida.Resultado = false;
                return salida;
            }
            if (request.WebID == null)
            {
                isFacturar = false;
                salida.Msj = "INTRODUSCA UN WEBID";
                salida.Resultado = false;
                return salida;
            }

            if (request.TipoOperacion == 0)
            {
                salida.Msj = "INTRODUSCA UNA OPERACION VALIDA PORFAVOR";
                salida.Resultado = false;
                return salida;
            }

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

            #region GetDocument validacion del ticket y obtencion de datos ticket

            //despues de crear la lista agregamos los siguientes campos para finalizar el reques de consumo en facturacion 
            GetDocumentRequest requesgetdocument = new GetDocumentRequest
            {
                Identity = bsObj.Identity,
                Id = request.Nticket,
                UsageType = DocumentUsageType.PrintCopy,
            };

            GetDocumentResponse responsegetdocument = await invokeHubbleWebAPIServices.GetDocument(requesgetdocument);

            if (responsegetdocument.Document == null)
            {
                salida.Msj = "Ticket o Numero de cliente no valido";
                salida.Resultado = false;
                return salida;
            }

            #region datos del ticket si es !null

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
            string strImprime2 = String.Empty;
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
                    //strImprime = "IVA " + item.Iva.ToString() + "%:     " + (Math.Truncate(decSumaIva * 100) / 100).ToString("N2");
                    strImprime = item.Iva.ToString() + "%:";
                    strImprime2 = (Math.Truncate(decSumaIva * 100) / 100).ToString("N2");
                }
                else
                    //strImprime += " | IVA " + item.Iva.ToString() + "%:     " + (Math.Truncate(decSumaIva * 100) / 100).ToString("N2");
                    strImprime += item.Iva.ToString() + "%:";
                strImprime2 = (Math.Truncate(decSumaIva * 100) / 100).ToString("N2");
                decSumaIva = 0;
                recorreUnicoIva += 1;
            }
            string salidaiva = strImprime.ToString();
            string salidaivamonto = strImprime2.ToString();
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







            GetPosInformationRequest getPosInformationRequest = new GetPosInformationRequest { Identity = bsObj.Identity };
            GetPOSInformationResponse getPOSInformationResponse = await invokeHubbleWebAPIServices.GetPOSInformation(getPosInformationRequest);



            string serieWebId = null;




            GetSeriesRequest getSeriesRequest = new GetSeriesRequest { Identity = bsObj.Identity };
            GetSeriesResponse getSeriesResponse = await invokeHubbleWebAPIServices.GetSeries(getSeriesRequest);



            foreach (var series in getSeriesResponse.SeriesList)
            {
                serieWebId = series.Code;
            }






            string formatofecha = Convert.ToString(responsegetdocument.Document.EmissionLocalDateTime);
            DateTimeOffset fechaticketstring = DateTimeOffset.Parse(formatofecha);
            string fechaticket = Convert.ToString(fechaticketstring.DateTime);
            string nticketco = responsegetdocument.Document.Id;
            string horaFormatFact = fechaticket.Replace(" ", "");



            string hourWebID = horaFormatFact.Substring(10, 2);
            string companyEESS = getPOSInformationResponse.PosInformation.CompanyCode;
            string minutWebID = horaFormatFact.Substring(13, 2);
            string serieTicket = serieWebId;
            string secontWebID = horaFormatFact.Substring(16, 2);



            string webidnwe = string.Concat(hourWebID + companyEESS + minutWebID + serieTicket + secontWebID);
            #endregion


            #endregion


            #region cliente
            GetCustomerRequest resquestcustomer = new GetCustomerRequest
            {
                Id = request.NoCliente,
                Identity = bsObj.Identity
            };



            //InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
            GetCustomerResponse responsecustomer = await invokeHubbleWebAPIServices.GetCustomer(resquestcustomer);
            string rfccliente = "";
            string razoonsocial = "";
            if (responsecustomer.Customer != null)
            {
                rfccliente = responsecustomer.Customer.TIN;
                razoonsocial = responsecustomer.Customer.BusinessName;

            }
            if (responsecustomer.Customer == null)
            {
                rfccliente = null;
                razoonsocial = null;

            }

            //if (responsecustomer.Customer.BusinessName == null && responsecustomer.Customer.TIN == null)
            //{
            //    isFacturar = false;
            //    salida.Resultado = false;
            //    salida.Msj = "NO SE PUDO ENCONTRAR LA INFORMACION DEL CLIENTE";
            //    return salida;
            //}

            #endregion


            if (request.TipoOperacion == 1)
            {
                if (responsegetdocument.Document == null && responsecustomer.Customer == null)
                {
                    salida.Msj = "Numero de cliente y numero de ticket no valido";
                    salida.Resultado = false;
                    return salida;
                }


                isFacturar = false;
                salida.RazonSocial = textosincarspecial.transformtext(razoonsocial);
                salida.RFC = rfccliente;
                salida.Resultado = true;
                salida.Msj = "OPERACION REALIZADA CON EXITO";
            }

            if (request.TipoOperacion == 1 && responsecustomer.Customer == null || responsegetdocument.Document == null)
            {
                isFacturar = false;

                salida.Resultado = false;
                salida.Msj = "El cliente y el ticket no son validos";
                return salida;
            }







            if (request.TipoOperacion == 2)
            {


                if (responsecustomer.Customer == null)
                {

                    isFacturar = false;
                    rfccliente = null;
                    razoonsocial = null;
                    salida.Msj = "OPERACION REALIZADA CON EXITO";
                    salida.Resultado = true;


                }
                if (responsecustomer.Customer != null)
                {
                    isFacturar = true;
                    salida.RazonSocial = textosincarspecial.transformtext(razoonsocial);
                    salida.RFC = rfccliente;
                }

            }




            #region information

            //   //   InvokeHubbleWebAPIServices invokeHubbleWebAPIServices3 = new InvokeHubbleWebAPIServices();
            //InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
            GetPOSInformationResponse informationresponses = await invokeHubbleWebAPIServices.GetPOSInformation(getPosInformationRequest);

            #endregion




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
                    RFC = rfccliente,
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



                if (responsefacturacion.mensaje == "DATOS DEL TICKET NO VALIDOS PARA FACTURAR")
                {
                    //salida.Ticket = request.Nticket;
                    //salida.FormaPago = metodopago;//"EFECTIVO"; //(responsegetdocument.Document.PaymentDetailList[0].PaymentMethodId);//pendiente por modificar
                    //salida.Subtotal = (Math.Truncate(responsegetdocument.Document.TaxableAmount * 100) / 100).ToString("N2");
                    //salida.Terminal = responsegetdocument.Document.PosId;
                    //salida.Operador = responsegetdocument.Document.OperatorName;
                    //salida.Folio = Folioidticket;
                    //salida.Total = (Math.Truncate(responsegetdocument.Document.TotalAmountWithTax * 100) / 100).ToString("N2");
                    //salida.ImporteEnLetra = letraconvert;
                    //salida.iva = salidaiva;
                    //salida.ivaMonto = salidaivamonto;
                    //salida.productos = listan;
                    //salida.Fecha = fechaticket;
                    //salida.WebID = webidnwe;
                    //salida.Estacion = informationresponses.PosInformation.ShopCode;
                    salida.Msj = responsefacturacion.mensaje;
                    salida.Resultado = false;
                    return salida;
                }
                if (responsefacturacion.mensaje == "DATOS DEL TICKET INCORRECTO PARA FACTURAR")
                {
                    //salida.Ticket = request.Nticket;
                    //salida.FormaPago = metodopago;//"EFECTIVO"; //(responsegetdocument.Document.PaymentDetailList[0].PaymentMethodId);//pendiente por modificar
                    //salida.Subtotal = (Math.Truncate(responsegetdocument.Document.TaxableAmount * 100) / 100).ToString("N2");
                    //salida.Terminal = responsegetdocument.Document.PosId;
                    //salida.Operador = responsegetdocument.Document.OperatorName;
                    //salida.Folio = Folioidticket;
                    //salida.Total = (Math.Truncate(responsegetdocument.Document.TotalAmountWithTax * 100) / 100).ToString("N2");
                    //salida.ImporteEnLetra = letraconvert;
                    //salida.iva = salidaiva;
                    //salida.ivaMonto = salidaivamonto;
                    //salida.productos = listan;wa
                    //salida.Fecha = fechaticket;
                    //salida.WebID = webidnwe;
                    //salida.Estacion = informationresponses.PosInformation.ShopCode;
                    salida.Msj = responsefacturacion.mensaje;
                    salida.Resultado = false;
                    return salida;
                }
                if (responsefacturacion.mensaje == "NO SE PUDO ENCONTRAR EL SERVICIO DE FACTURACION")
                {
                    salida.Msj = responsefacturacion.mensaje;
                    salida.Resultado = true;
                    return salida;
                }
                if (responsefacturacion.mensaje == "FACTURACION CORRECTA")
                {
                    salida.SelloDigitaSAT = responsefacturacion.SelloDigitaSAT;
                    salida.SelloDigitaCFDI = responsefacturacion.SelloDigitaCFDI;
                    salida.CadenaOrigTimbre = responsefacturacion.CadenaOrigTimbre;
                    salida.FolioFiscal = responsefacturacion.FolioFiscal;
                    salida.RFCProveedorCert = responsefacturacion.RFCProveedorCert;
                    salida.NumCertificado = responsefacturacion.NumCertificado;
                    salida.DateCertificacion = responsefacturacion.DateCertificacion;
                    salida.Msj = responsefacturacion.mensaje;
                    salida.Resultado = true;
                }

                if (responsefacturacion.mensaje == "ERROR DE TIMBRADO AL FACTURAR")
                {
                    salida.Msj = responsefacturacion.mensaje;
                    salida.Resultado = false;
                    return salida;
                }

                if (responsefacturacion.mensaje == "NO SE PUDO ENCONTRAR EL SERVICIO DE FACTURACION")
                {
                    salida.Msj = "NO SE PUDO FACTURAR  INTENTELO MAS TARDE";
                    salida.Resultado = false;
                    return salida;

                }
                //else
                //{


                //    salida.Msj = "NO SE PUDO FACTURAR";
                //    salida.Resultado = false;

                //}
                #endregion




            }

            salida.Ticket = request.Nticket;
            salida.FormaPago = metodopago;//"EFECTIVO"; //(responsegetdocument.Document.PaymentDetailList[0].PaymentMethodId);//pendiente por modificar
            salida.Subtotal = (Math.Truncate(responsegetdocument.Document.TaxableAmount * 100) / 100).ToString("N2");
            salida.Terminal = responsegetdocument.Document.PosId;
            salida.Operador = responsegetdocument.Document.OperatorName;
            salida.Folio = Folioidticket;
            salida.Total = (Math.Truncate(responsegetdocument.Document.TotalAmountWithTax * 100) / 100).ToString("N2");
            salida.ImporteEnLetra = letraconvert;
            salida.iva = salidaiva;
            salida.ivaMonto = salidaivamonto;
            salida.productos = listan;
            salida.Fecha = fechaticket;
            salida.WebID = webidnwe;
            salida.Estacion = informationresponses.PosInformation.ShopCode;
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
            return salida;
        }
    }
}
