using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Conection.HubbleWS.Models.Facturacion;

namespace Conection.HubbleWS
{
    public class InvokeWebAPIServicesExternalProy
    {
        #region PROPIAS DEL CIERRE DE CAJA WS
        public async Task<AreThereAnyPendingToCloseItemByCashBoxIdResponse> AreThereAnyPendingToCloseItemByCashBox(AreThereAnyPendingToCloseItemByCashBoxIdRequest request)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(request.UriModular);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("api/CashBox/PendingToClosebyCashbox", request))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        AreThereAnyPendingToCloseItemByCashBoxIdResponse deserializeJson = JsonConvert.DeserializeObject<AreThereAnyPendingToCloseItemByCashBoxIdResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }

        public async Task<ClousureCashBoxTerminalResponse> ClousureCashBoxTerminal(ClousureCashBoxTerminalRequest request)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(request.UriModular);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("api/CashBoxClousure/GetCashOperator", request))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        ClousureCashBoxTerminalResponse deserializeJson = JsonConvert.DeserializeObject<ClousureCashBoxTerminalResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }

        public async Task<PreViewInsertClousureCashBoxResponse> PreViewInsertClousureCashBox(PreViewInsertClousureCashBoxRequest request)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(request.UriModular);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("api/CashBox/PreviewInsertClousureCashBox", request))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        PreViewInsertClousureCashBoxResponse deserializeJson = JsonConvert.DeserializeObject<PreViewInsertClousureCashBoxResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }

        public async Task<GetCashBoxTotalResponse> GetTempCierresTpagoFromCaja(ClousureCashBoxTerminalRequest request)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(request.UriModular);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("api/CashBox/GetTempCierresTpago", request))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        GetCashBoxTotalResponse deserializeJson = JsonConvert.DeserializeObject<GetCashBoxTotalResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }

        public async Task<GetTempCierresSeriesGlobResponse> GetTMPCierresSerieFromCaja(ClousureCashBoxTerminalRequest request)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(request.UriModular);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("api/CashBox/GetTempCierresSerieFC", request))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        GetTempCierresSeriesGlobResponse deserializeJson = JsonConvert.DeserializeObject<GetTempCierresSeriesGlobResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }

        public async Task<GetTempCierresSeriesGlobResponse> GetTempCierresV_TpagoFromCaja(ClousureCashBoxTerminalRequest request)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(request.UriModular);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("api/CashBox/GetTempCierresVTpagoFC", request))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        GetTempCierresSeriesGlobResponse deserializeJson = JsonConvert.DeserializeObject<GetTempCierresSeriesGlobResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }

        public async Task<GetTempCierresSeriesGlobResponse> GetTempCierresOperadorFromCaja(ClousureCashBoxTerminalRequest request)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(request.UriModular);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("api/CashBox/GetTempCierresOperadorFC", request))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        GetTempCierresSeriesGlobResponse deserializeJson = JsonConvert.DeserializeObject<GetTempCierresSeriesGlobResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }

        public async Task<GetCheckCierresTempFromCajaResponse> GetCheckCierresTempFromCaja(GetCheckCierresTempFromCajaRequest request)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(request.UriModular);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("api/CashBox/GetCheckCierresTempFromCaja", request))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        GetCheckCierresTempFromCajaResponse deserializeJson = JsonConvert.DeserializeObject<GetCheckCierresTempFromCajaResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }

        public async Task<DateFromTicketResponse> GetTimesTicketsForOperador(DateFromTicketRequest request)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(request.UriModular);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("api/CashBox/GetTimeTickets", request))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        DateFromTicketResponse deserializeJson = JsonConvert.DeserializeObject<DateFromTicketResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }

        public async Task<GetDocumentsForCashboxClosureExtendedResponse> GetDocumentsForCashboxClosureExtended(GetDocumentsForCashboxClosureExtendedRequest request)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(request.UriModular);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("api/CashBox/GetDocumentsForCashbox", request))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        GetDocumentsForCashboxClosureExtendedResponse deserializeJson = JsonConvert.DeserializeObject<GetDocumentsForCashboxClosureExtendedResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }

        public async Task<InsertClousureCashBoxResponse> InsertClousureCashBoxForOperator(InsertClousureCashBoxRequest request)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(request.UriModular);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("api/CashBoxClousure/InsertClousureCashBox", request))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        InsertClousureCashBoxResponse deserializeJson = JsonConvert.DeserializeObject<InsertClousureCashBoxResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }

        #endregion
    }
}
