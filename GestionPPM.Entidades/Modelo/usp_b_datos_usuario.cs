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
    
    public partial class usp_b_datos_usuario
    {
        public int id_usuario { get; set; }
        public string nombre_usuario { get; set; }
        public string apellido_usuario { get; set; }
        public Nullable<int> cliente_asociado { get; set; }
        public Nullable<int> tipo_usuario { get; set; }
        public string area_departamento { get; set; }
        public Nullable<int> pais { get; set; }
        public Nullable<int> ciudad { get; set; }
        public string direccion_usuario { get; set; }
        public string mail_usuario { get; set; }
        public string telefono_usuario { get; set; }
        public string celular_usuario { get; set; }
        public Nullable<int> rol_id { get; set; }
        public string codigo_usuario { get; set; }
        public string clave_usuario { get; set; }
        public Nullable<bool> estado_usuario { get; set; }
        public Nullable<bool> activo_usuario { get; set; }
        public string cargo_usuario { get; set; }
        public Nullable<int> id_empresa { get; set; }
        public Nullable<int> secu_usua { get; set; }
        public Nullable<bool> reset_clave { get; set; }
    }
}