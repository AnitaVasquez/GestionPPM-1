//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GestionPPM.Entidades.Modelo
{
    using System;
    
    public partial class ListadoPresupuestoPrefacturaSAFI
    {
        public int Secuencial { get; set; }
        public string Cliente { get; set; }
        public string Identificacion { get; set; }
        public string Correos { get; set; }
        public string Direccion { get; set; }
        public string Comentario1 { get; set; }
        public string Comentario2 { get; set; }
        public string Comentario3 { get; set; }
        public string CodigoProdcuto { get; set; }
        public string NombreProducto { get; set; }
        public string Bodega { get; set; }
        public Nullable<int> IdBodega { get; set; }
        public Nullable<int> Id_Producto { get; set; }
        public Nullable<int> IdFormaPago { get; set; }
        public Nullable<int> IdCentroCosto { get; set; }
        public string CodigoCentroCosto { get; set; }
        public int Cantidad { get; set; }
        public Nullable<decimal> PrecioUnitario { get; set; }
        public Nullable<decimal> Subtotal { get; set; }
        public Nullable<decimal> Descuento { get; set; }
        public Nullable<decimal> Total { get; set; }
        public Nullable<int> Pago { get; set; }
        public Nullable<decimal> PorcentajeIva { get; set; }
        public string Fecha { get; set; }
        public string CodigoFormaPago { get; set; }
    }
}
