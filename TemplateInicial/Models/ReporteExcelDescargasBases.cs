using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TemplateInicial.Models
{
    public class ReporteExcelMidsOperaciones
    {
        public string RUC { get; set; }
        public string Establecimiento { get; set; }
        public string Especialidad { get; set; }
        public string Dimension { get; set; }
        public DateTime? FechaAfiliacion { get; set; }
    }
    public class ReporteExcelMidsNuevos
    {
        public string RUC { get; set; }
        public string Establecimiento { get; set; }
        public string Sector { get; set; }
        public string Dimension { get; set; }
        public DateTime? FechaAfiliacion { get; set; }
        public string RazonSocial { get; set; }
        public string NombreComercial { get; set; }
        public string DireccionComercio { get; set; }
        public string Telefono1 { get; set; }
        public string IdentificacionRepresentanteLegal { get; set; }
        public string NombreRepresentanteLegal { get; set; }
        public string IdentificacionAdministrador { get; set; }
        public string NombresAdministrador { get; set; }
        public string NumeroCuentaBancaria { get; set; }
        public int? TipoCuentaBancaria { get; set; }
        public string BancoCuentaBancaria { get; set; }
    }
}