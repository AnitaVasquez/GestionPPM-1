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
    
    public partial class OrganigramaInfo
    {
        public int IDOrganigrama { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string EstructuraOrganigrama { get; set; }
        public string Codigo { get; set; }
        public Nullable<int> EmpresaID { get; set; }
        public string nombre_comercial { get; set; }
        public int TipoOrganigramaID { get; set; }
        public bool EstadoOrganigrama { get; set; }
        public Nullable<int> IDTipoOrganigrama { get; set; }
        public string NombreTipoOrganigrama { get; set; }
        public string DescripcionTipoOrganigrama { get; set; }
        public string CodigoTipoOrganigrama { get; set; }
        public Nullable<bool> EstadoTipoOrganigrama { get; set; }
    }
}
