using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models.Facturacion
{
    public class Address
    {
        /// <summary>
        /// Identificador
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Calle, pudiendo incluir el número, piso, letra...
        /// (Ej: C/ Juan de la Cierva, 12 pido 5ª A)
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// [Opcional]
        /// Identificador de la ciudad/población.
        /// Null si no existe.
        /// </summary>
        public int? CityId { get; set; }

        /// <summary>
        /// [Opcional]
        /// Nombre de la ciudad/población. Null si no existe.
        /// 
        /// CAUTION!!
        /// Por diseño del origen de datos (ilion), puede existir el nombre y no el id de ciudad correspondiente, 
        /// así como haber en este modelo una correspondencia Id/Nombre de ciudad distinta a la que haya en el ente ciudad
        /// </summary>
        public string CityName { get; set; }

        /// <summary>
        /// [Opcional]
        /// Código postal. Null si no existe.
        /// 
        /// CAUTION!!
        /// Por diseño del origen de datos (ilion), puede existir un código postal y no el nombre y/o id de ciudad correspondiente, 
        /// así como haber en este modelo una correspondencia Id/Nombre/CP de ciudad distinta a la que haya en el ente ciudad
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// [Opcional]
        /// Identificador de la provincia/región. Null si no existe
        /// 
        /// CAUTION!!
        /// Por diseño del origen de datos (ilion), puede ser un Id que no se corresponda con el que tenga el ente ciudad que corresponda a la informada en este modelo
        /// </summary>
        public int? ProvinceId { get; set; }

        /// <summary>
        /// [Opcional]
        /// Nombre de la provincia/región. Null si no existe
        /// 
        /// CAUTION!!
        /// Por el diseño del origen de datos (ilion), puede existir el nombre y no el id de provincia correspondiente,
        /// así como haber en este modelo una correspondencia Id/Nombre de provincia distinta a la que haya en el ente provincia o 
        /// a la que se llegue a través del ente ciudad que corresponda al informado en este modelo.
        /// </summary>
        public string ProvinceName { get; set; }

        /// <summary>
        /// [Opcional]
        /// Identificador del país. Null si no existe
        /// 
        /// CAUTION!!
        /// Por el diseño del origen de datos (ilion), puede ser un Id que no se corresponda con el que tenga el ente provincia que corresponda a la informada en este modelo.
        /// </summary>
        public int? CountryId { get; set; }

        /// <summary>
        /// [Opcional]
        /// Nombre del país. Null si no existe
        /// 
        /// CAUTION!!
        /// Por el diseño del origen de datos (ilion), puede existir el nombre y no el id de país correspondiente,
        /// así com ohaber en este modelo una correspondencia Id/Nombre de país distinta a la que haya en el ente país o
        /// a la que se llegue a través del ente provincia que corresponda al informado en este modelo
        /// </summary>
        public string CountryName { get; set; }

        /// <summary>
        /// [Opcional]
        /// Número de teléfono.
        /// Null si no existe
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Tipo de dirección
        /// </summary>
        public AddressType Type { get; set; }

        /// <summary>
        /// Identificador de la entidad a la que pertenece esta dirección.
        /// El tipo de entidad vendrá determinada por la propiedad Type, si es que es conocida.
        /// (Ej: OwnerEntityId=0276322 y Type=CustomerMain significa el domicilio principal para el cliente con id 0276322)
        /// </summary>
        public string OwnerEntityId { get; set; }
    }
}
