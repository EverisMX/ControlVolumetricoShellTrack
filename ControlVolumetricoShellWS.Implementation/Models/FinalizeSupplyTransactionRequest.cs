
namespace ControlVolumetricoShellWS.Implementation
{
    public class FinalizeSupplyTransactionRequest
    {
        /// <summary>
        /// Identificador del operador que gestionó esta transacción de suministro.
        /// </summary>
        public string OperatorId { get; set; }

        /// <summary>
        /// Identificador del cliente al que se le está cobrando esta transacción de suministro
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Identificador interno en el controlador de estación que identifica esta transacción de suministro.
        /// </summary>
        public int SupplyTransactionId { get; set; }

        /// <summary>
        /// Identificador interno en el controlador de estación que identifica este surtidor.
        /// </summary>
        public int FuellingPointId { get; set; }

        /// <summary>
        /// Identificador provisional asociado a este transacción.
        /// </summary>
        public string ProvisionalId { get; set; }

        /// <summary>
        /// Posible número de documento al que irá vinculado el suministro que se generará.
        /// Este número de documento es posible que no sea el definitivo, o que nunca pueda crearse (por ejemplo porque haya fallado la impresión),
        /// en tal caso el sumnistro creado quedará huérfano (sin un documento asociado).
        /// </summary>
        public string PossibleDocumentId { get; set; }

        /// <summary>
        /// Posición que ocupa este suministro dentro del documento que se está vendiendo.
        /// Índice basado en 1.
        /// </summary>
        public int LineNumberInDocument { get; set; }

        /// <summary>
        /// Identificador del contacto.
        /// (Incluye el NComapny y es el antiguamente denominado NContact_Cli).
        ///
        /// El contacto relaciona un cliente con una tarjeta o algún otro
        /// medio usado para su identificación al realizar la venta.
        ///
        /// Null si no se ha utilizado ninguno de estos medios opcionales.
        /// </summary>
        public string ContactId { get; set; }

        /// <summary>
        /// Medidor del odómetro (cuenta kilómetros / cuenta millas / ...) que marcaba el vehículo del cliente que realizó este suministro.
        /// (ie: Kilómetros cliente tarjeta de flota, ...)
        /// Nulo si no existe.
        /// </summary>
        public decimal? OdometerMeasurement { get; set; }

        /// <summary>
        /// Placa / Matrícula del vehículo del cliente que realizó este suministro.
        /// Nulo si no existe.
        /// </summary>
        public string VehicleLicensePlate { get; set; }
    }
}
