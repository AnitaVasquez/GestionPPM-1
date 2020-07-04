using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TemplateInicial.Models
{
    public class CargaMasivaArchivo
    {
        public CargaMasivaArchivo()
        {
            Detalles = new List<DetallesCargaMasiva>();
        }
        public bool OK;
        public List<DetallesCargaMasiva> Detalles { get; set; }
        public bool GetEstado()
        {
            if (!Detalles.Any())
                return true;
            else
                return false;
        }
    }

    public class DetallesCargaMasiva
    {
        public long Fila { get; set; }
        public long Columna { get; set; }
        public string Valor { get; set; }
        public string Error { get; set; }
    }
}