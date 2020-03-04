using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Conection.HubbleWS.Helpers
{
    public class GenericHelper
    {

        #region Methods 

        /// <summary>
        /// Method to generate a security hash to be used as signature
        /// </summary>
        /// <param name="oHash">Object which contains the JSON data</param>
        /// <param name="sTimeStamp">The time to use to generate the hash</param>
        /// <param name="sClaveCompartida">A shared key to use to generate the hash</param>
        /// <returns>A sha256 hash generated from oHash, sTimeStamp and sClaveCompartida</returns>
        internal static string GeneratedSignature(Object oHash, String sTimeStamp, string claveCompartida)
        {
            return GenerateSHA256(OrderObjectPropertiesValuesAlphabetically(oHash) + sTimeStamp + claveCompartida);
        }

        /// <summary>
        /// Method to generate a security hash
        /// </summary>
        /// <param name="oHash"></param>
        /// <param name="sTimeStamp"></param>
        /// <returns></returns>
        internal static string GeneratedSignature(Object oHash, String sTimeStamp)
        {
            return GenerateSHA256(OrderObjectPropertiesValuesAlphabetically(oHash) + sTimeStamp + "IntegracionWSREST");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oHash"></param>
        /// <param name="sTimeStamp"></param>
        /// <returns></returns>
        internal static string GeneratedSignatureToXML(string strHash, String sTimeStamp)
        {
            return GenerateSHA256(strHash + sTimeStamp + "IntegracionWSREST");
        }

        /// <summary>
        /// Orders alphabetically given object properties and return the concatenation of its values
        /// </summary>
        /// <param name="obj">Object to process</param>
        /// <returns>Resulting concatenation string</returns>
        internal static string OrderObjectPropertiesValuesAlphabetically(object obj)
        {
            try
            {

                StringBuilder result = new StringBuilder();
                if (obj != null)
                {
                    List<PropertyInfo> props = obj.GetType().GetProperties().OrderBy(x => x.Name).ToList();
                    foreach (PropertyInfo pi in props)
                    {
                        if (pi.Name != "Signature")
                        {
                            Type type = pi.PropertyType;
                            if (pi.GetValue(obj, null) != null)
                            {
                                if (type.IsPrimitive || type.Equals(typeof(string)))
                                {
                                    result.Append(pi.GetValue(obj, null));
                                }
                                else if (type.Equals(typeof(decimal)) || type.Equals(typeof(decimal?)))
                                {
                                    result.Append(pi.GetValue(obj, null).ToString().Replace(",", "."));
                                }
                                else if (type.Equals(typeof(Boolean?)))
                                {
                                    result.Append(pi.GetValue(obj, null));
                                }
                                else if (type.Equals(typeof(Int16?)))
                                {
                                    result.Append(pi.GetValue(obj, null));
                                }
                                else if (type.Equals(typeof(Int32?)))
                                {
                                    result.Append(pi.GetValue(obj, null));
                                }
                                else if (type.Equals(typeof(DateTime)))
                                {
                                    result.Append(pi.GetValue(obj, null));
                                }
                                else if (type.IsArray)
                                {
                                    foreach (var p in (Array)pi.GetValue(obj, null))
                                    {
                                        Type type2 = p.GetType();

                                        if (type2.IsPrimitive || type2.Equals(typeof(string)))
                                        {
                                            result.Append(p);
                                        }
                                        else
                                        {
                                            result.Append(OrderObjectPropertiesValuesAlphabetically(p));
                                        }
                                    }
                                }
                                else if (type.IsGenericType)
                                {
                                    foreach (var g in (IEnumerable)pi.GetValue(obj, null))
                                    {
                                        Type type2 = g.GetType();

                                        if (type2.IsPrimitive || type2.Equals(typeof(string)))
                                        {
                                            result.Append(g);
                                        }
                                        else
                                        {
                                            result.Append(OrderObjectPropertiesValuesAlphabetically(g));
                                        }
                                    }
                                }
                                else if (type.IsEnum)
                                {
                                    result.Append((int)pi.GetValue(obj, null));
                                }
                                else
                                {
                                    result.Append(OrderObjectPropertiesValuesAlphabetically(pi.GetValue(obj, null)));
                                }
                            }
                            string strProcesoif = result.ToString();
                        }
                    }
                }

                string strProceso = result.ToString();
                return result.ToString();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// Generates a SHA256 string
        /// </summary>
        /// <param name="stringToHash">string to hash</param>
        /// <returns>hashed string</returns>
        internal static string GenerateSHA256(string stringToHash)
        {
            string hashedString = null;

            SHA256 mySHA256 = SHA256Managed.Create();
            byte[] hashedSignature = mySHA256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(stringToHash));
            hashedString = ByteArrayToHex(hashedSignature);

            return hashedString;
        }

        /// <summary>
        /// Converts a byte array to hex string.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns>
        /// hex string
        /// </returns>
        internal static string ByteArrayToHex(byte[] array)
        {
            if (array != null)
            {
                return BitConverter.ToString(array).Replace("-", string.Empty);
            }

            return null;
        }

        /// <summary>
        /// Replaces in a string the decimal separator corresponding to the culture
        /// </summary>
        /// <param name="pstrAtributo">Chain to which the replacement is to be made</param>
        /// <param name="pstrSeparadorDecimal">Culture-indicated decimal</param>
        /// <returns>String with the replacement done</returns>
        internal static string fnReplaceCultureInfo(string pstrAtributo, string pstrSeparadorDecimal)
        {
            if (pstrAtributo == null || pstrAtributo.Trim() == "")
                return "0";
            else
                return pstrSeparadorDecimal == "." ? pstrAtributo.Replace(",", ".") : pstrAtributo.Replace(".", ",");
        }
        #endregion

    }
}
