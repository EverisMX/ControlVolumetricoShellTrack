﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Conection.HubbleWS.Models.Facturacion;

namespace Conection.HubbleWS
{
    public class InvokeHubbleWebAPIServices
    {
        #region BASE WEBAPI
        // SHELLMX- Este metodo se encuentra en <<<HubblePOSWebAPI/Controller/MainController.cs>>> se extrajo del metodo Original del GetProductForSaleRequest
        public async Task<GetProductForSaleResponse> GetProductForSale(GetProductForSaleRequest getProductForSaleRequest)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8091");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("/main/GetProductForSale", getProductForSaleRequest))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;
                       
                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        GetProductForSaleResponse deserializeJson = JsonConvert.DeserializeObject<GetProductForSaleResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }
        #endregion

        #region
        public async Task<facresponse> tpvfacturacionn(GenerateElectronicInvoice requestfac)
        {



            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8091");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("/main/Electronicbill", requestfac))
                {
                    
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;
                        //como es un objeto nos genera basura por lo que la remplazamos 
                        string responsejsonn = responseJson.Replace("\\","").Replace("\"{", "{").Replace("}\"", "}");
                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        facresponse deserializeJson = JsonConvert.DeserializeObject<facresponse>(responsejsonn);


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
