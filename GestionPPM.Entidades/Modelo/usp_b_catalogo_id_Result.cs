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
    
    public partial class usp_b_catalogo_id_Result
    {
        public int id_catalogo { get; set; }
        public string codigo_catalogo { get; set; }
        public string nombre_catalgo { get; set; }
        public string descripcion_catalogo { get; set; }
        public Nullable<int> id_catalogo_padre { get; set; }
        public Nullable<bool> estado_catalogo { get; set; }
        public Nullable<int> id_empresa { get; set; }
    }
}
