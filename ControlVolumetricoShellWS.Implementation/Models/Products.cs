
namespace ControlVolumetricoShellWS.Implementation
{
    public class Products
    {
        public int Id_producto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Importe_Unitario { get; set; }
        public decimal Importe_Total { get; set; }
        public int Iva_producto { get; set; }
        public string Forma_Pago { get; set; }
        public decimal Monto_Pagado { get; set; }
    }
}
