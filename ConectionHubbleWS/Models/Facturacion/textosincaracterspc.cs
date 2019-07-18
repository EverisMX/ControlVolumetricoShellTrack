using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models.Facturacion
{
    public class textosincaracterspc
    {
        public string transformtext(string textoini)
        {
            var inputString = textoini;
            var normalizedString = inputString.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            for (int i = 0; i < normalizedString.Length; i++)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(normalizedString[i]);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(normalizedString[i]);
                }
            }
            string nuevotexto = Convert.ToString((sb.ToString().Normalize(NormalizationForm.FormC)));
            nuevotexto = Regex.Replace(nuevotexto, @"[^a-zA-z0-9 ]+", "");
            
            return nuevotexto;
        }
    }
}
