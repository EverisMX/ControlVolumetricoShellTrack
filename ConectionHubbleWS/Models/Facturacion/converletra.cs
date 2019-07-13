using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models.Facturacion
{
    public class converletra
    {
       
            public static string numbersToLetter(double cantidad)
            {
                //debugger;
                if (cantidad == 0.00 || cantidad == 0)
                {
                    return "cero pesos 00/100 M.N.";
                }
                else
                {
                    string cant = Convert.ToString(cantidad);
                    // let numbers = cant.split(".");
                    // let ent = numbers[0];

                    string[] numbers = new string[] { };
                    numbers = cant.Split('.');
                    string ent = numbers[0];
                    string letterMoneda = "";

                    switch (ent.Length)
                    {
                        case 1:
                            letterMoneda = unidades(Convert.ToInt32(ent));
                            break;
                        case 2:
                            // letterMoneda = decenas(Math.Floor((Convert.ToDouble(ent) / 10) % 10), Math.Floor((Convert.ToDouble(ent) / 1) % 10));
                            letterMoneda = decenas(Convert.ToInt32((Math.Floor(Convert.ToDouble(ent) / 10) % 10)), (Convert.ToInt32((Math.Floor(Convert.ToDouble(ent) / 10) % 10))));
                            break;
                        case 3:
                            letterMoneda = centenas(Convert.ToInt32(Math.Floor((Convert.ToDouble(ent) / 100) % 10)), Convert.ToInt32(Math.Floor((Convert.ToDouble(ent) / 10) % 10)), Convert.ToInt32(Math.Floor((Convert.ToDouble(ent) / 1) % 10)));
                            break;
                        case 4:
                            letterMoneda = unidadesdemillar(Convert.ToInt32(Math.Floor((Convert.ToDouble(ent) / 1000) % 10)), Convert.ToInt32(Math.Floor((Convert.ToDouble(ent) / 100) % 10)), Convert.ToInt32(Math.Floor((Convert.ToDouble(ent) / 10) % 10)), Convert.ToInt32(Math.Floor((Convert.ToDouble(ent) / 1) % 10)));
                            break;
                        case 5:
                            letterMoneda = decenasdemillar(Convert.ToInt32(Math.Floor((Convert.ToDouble(ent) / 10000) % 10)), Convert.ToInt32(Math.Floor((Convert.ToDouble(ent) / 1000) % 10)), Convert.ToInt32(Math.Floor((Convert.ToDouble(ent) / 100) % 10)), Convert.ToInt32(Math.Floor((Convert.ToDouble(ent) / 10) % 10)), Convert.ToInt32(Math.Floor((Convert.ToDouble(ent) / 1) % 10)));
                            break;
                    }
                    if (numbers.Length == 1)
                    {
                        return ent == "1" ? letterMoneda + "peso " + "00" + "/100 M.N." : letterMoneda + "pesos " + "00" + "/100 M.N.";
                    }
                    else
                    {
                        string centavo = numbers[1];
                        return ent == "1" ? letterMoneda + "peso " + centavo + "/100 M.N." : letterMoneda + "pesos " + centavo + "/100 M.N.";
                    }
                }
            }
            private static string unidades(double unidad)
            {
                string[] unidades = {"",
              "un ",
              "dos ",
              "tres ",
              "cuatro ",
              "cinco ",
              "seis ",
              "siete ",
              "ocho ",
              "nueve "};
                return unidades[Convert.ToInt32(unidad)];
            }
            private static string decenas(double decena, double unidad)
            {
                string[] diez = {"once ",
              "doce ",
              "trece ",
              "catorce ",
              "quince ",
              "dieciseis ",
              "diecisiete ",
              "dieciocho ",
              "diecinueve " };
                string[] decenas = {"diez ",
              "veinte ",
              "treinta ",
              "cuarenta ",
              "cincuenta ",
              "sesenta ",
              "setenta ",
              "ochenta ",
              "noventa " };

                if (decena == 1)
                {
                    if (unidad == 0)
                    {
                        return decenas[Convert.ToInt32(decena - 1)];
                    }
                    else
                    {
                        return diez[Convert.ToInt32(unidad - 1)];
                    }
                }
                else if (decena == 2)
                {
                    if (unidad == 0)
                    {
                        return decenas[Convert.ToInt32(decena - 1)];
                    }
                    else
                    {
                        return "veinti" + unidades(unidad);
                    }
                }
                else
                {
                    if (unidad != 0)
                    {
                        return decenas[Convert.ToInt32(decena - 1)] + "y " + unidades(unidad);
                    }
                    else
                    {
                        return decenas[Convert.ToInt32(decena - 1)];
                    }
                }
            }
            public static string centenas(double centena, double decena, double unidad)
            {
                //debugger;
                string[] centenas = {"",
              "ciento ",
              "doscientos ",
              "trecientos ",
              "cuatrocientos ",
              "quinientos ",
              "seiscientos ",
              "setecientos ",
              "ochocientos ",
              "novecientos " };

                if (centena == 1 && decena == 0 && unidad == 0)
                {
                    return "cien ";
                }
                else if (decena == 0)
                {
                    return centenas[Convert.ToInt32(centena)] + "" + unidades(unidad);
                }
                else
                {
                    return centenas[Convert.ToInt32(centena)] + "" + decenas(decena, unidad);
                }
            }
            private static string unidadesdemillar(double unimill, double centena, double decena, double unidad)
            {
                if (unimill == 1 && centena == 0 && decena == 0 && unidad == 0)
                {
                    return "mil ";
                }
                else if (centena == 0 && unimill != 0 && decena != 0)
                {
                    return unidades(unimill) == "un " ? "mil " + decenas(decena, unidad) : unidades(unimill) + "mil " + decenas(decena, unidad);
                }
                else
                {
                    return unidades(unimill) == "un " ? "mil " + centenas(centena, decena, unidad) : unidades(unimill) + "mil " + centenas(centena, decena, unidad);
                }
            }
            private static string decenasdemillar(int decemill, int unimill, int centena, int decena, int unidad)
            {
                if (decemill != 0 && unimill == 0 && centena == 0 && decena == 0 && unidad == 0)
                {
                    return decenas(decemill, unimill) + "mil ";
                }
                else if (decemill != 0 && unimill != 0 && centena == 0)
                {
                    return decenas(decemill, unimill) + "mil " + decenas(decena, unidad);
                }
                else
                {
                    return decenas(decemill, unimill) + "mil " + centenas(centena, decena, unidad);
                }
            }
        }
    
}
