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
    
    public partial class DatosProyecto
    {
        public string NombreProyecto { get; set; }
        public string Cliente { get; set; }
        public string Ejecutivo { get; set; }
        public Nullable<int> Fase { get; set; }
        public Nullable<int> General { get; set; }
        public Nullable<int> Estado { get; set; }
        public Nullable<int> EstadoGeneral { get; set; }
        public int CodigoProyecto { get; set; }
        public Nullable<int> Cumplimiento { get; set; }
        public string FechaInicioProgramado { get; set; }
        public string FechaFinProgramado { get; set; }
        public string FechaInicioReal { get; set; }
        public string FechaFinReal { get; set; }
        public Nullable<int> NumeroHorasProgramado { get; set; }
        public Nullable<int> NumeroHorasReal { get; set; }
        public string EstadoCodigo { get; set; }
        public int Editar { get; set; }
        public int CodigoCotizacion { get; set; }
    }
}
