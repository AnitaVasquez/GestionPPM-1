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
    using System.Collections.Generic;
    
    public partial class CodigoProducto
    {
        public int id_codigo_producto { get; set; }
        public Nullable<int> id_bodega { get; set; }
        public Nullable<int> id_catalogo { get; set; }
        public string nombre_producto { get; set; }
        public string codigo_producto { get; set; }
        public Nullable<bool> estado { get; set; }
    }
}
