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
    
    public partial class GeneraFactruasPTOPSAFI
    {
        public int ID { get; set; }
        public string Cliente { get; set; }
        public string Identificacion { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Correos { get; set; }
        public Nullable<int> cantidad { get; set; }
        public Nullable<decimal> precio_unitario { get; set; }
        public Nullable<decimal> subtotal { get; set; }
        public Nullable<decimal> descuento { get; set; }
        public Nullable<decimal> total { get; set; }
        public Nullable<System.DateTime> fecha_factura { get; set; }
        public string observaciones { get; set; }
        public string detalle { get; set; }
        public Nullable<int> idCliente { get; set; }
        public Nullable<int> Secuencial { get; set; }
    }
}
