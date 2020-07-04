using GestionPPM.Entidades.Metodos;
using GestionPPM.Entidades.Modelo.PlaceToPay;
using GestionPPM.Repositorios;
using HtmlTableHelper;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Seguridad.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using TemplateInicial.Models;

namespace TemplateInicial.Controllers
{
    [Autenticado]
    public class CargasDescargasComerciosController : BaseAppController
    {
        private List<string> columnasReportesBasicosOperaciones = new List<string> { "RUC", "ESTABLECIMIENTO", "ESPECIALIDAD", "DIMENSION", "FECHA DE INGRESO" };
        private List<string> columnasReportesBasicosMidsNuevos = new List<string> { "RUC", "ESTABLECIMIENTO", "SECTOR", "DIMENSION", "FECHA DE INGRESO", "RAZON SOCIAL", "NOMBRE COMERCIAL DEL ESTABLECIMIENTO", "DIRECCION DEL ESTABLECIMIENTO", "TELEFONO CONVENCIONAL", "CEDULA REPRESENTANTE LEGAL", "NOMBRE REPRESENTANTE LEGAL", "CEDULA ADMINISTRADOR", "NOMBRES DE ADMINISTRADOR", "NRO DE CUENTA BANCARA", "TIPO DE CUENTA", "BANCO DE CUENTA BANCARIA" };

        // GET: CargasDescargasComercios
        public ActionResult Index()
        {
            //FORMATO PARA CARGAS MASIVAS DE MIDS
            string rutaBaseDocumentosIngreso = AppDomain.CurrentDomain.BaseDirectory + "Plantillas/";
            string pathDocumentosIngreso = Path.Combine(rutaBaseDocumentosIngreso, "FORMATO CARGA MIDS.xlsx");
            ViewBag.FormatoCargaMids = pathDocumentosIngreso;

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Create()
        {
            CargaMasivaArchivo carga = new CargaMasivaArchivo();
            try
            {
                //En caso de que no haya ningún archivo cargado
                if (Request.Files.Count == 0)
                    return Json(new { error = Mensajes.MensajeAdjuntoFallido + " Por favor, verificar el archivo.", Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeAdjuntoFallido } }, JsonRequestBehavior.AllowGet);


                List<MidsComercioPlaceToPay> listadoFinalMIDS = new List<MidsComercioPlaceToPay>();
                string fileName = string.Empty;
                string pathServidor = string.Empty;

                foreach (string item in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[item] as HttpPostedFileBase;
                    fileName = file.FileName;

                    string extension = Path.GetExtension(fileName);

                    //Formato incorrecto
                    if (extension != ".xlsx" && extension != ".xls")
                        return Json(new { error = "Formato no permitido.", Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeAdjuntoFallido } }, JsonRequestBehavior.AllowGet);


                    //SI LA RUTA EN DISCO NO EXISTE LOS ARCHIVOS SE ALMACENAN EN LA CARPETA MISMO DEL PROYECTO
                    string rutaBase = basePathRepositorioDocumentos + "\\GESTION_PPM\\ArchivosCargasMasivasMIDS";
                    bool directorio = Directory.Exists(rutaBase);
                    // En caso de que no exista el directorio, crearlo.
                    if (!directorio)
                        Directory.CreateDirectory(rutaBase);

                    pathServidor = Path.Combine(rutaBase, fileName);

                    if (file.ContentLength > 0)
                    {
                        file.SaveAs(pathServidor);

                        FileInfo existingFile = new FileInfo(pathServidor);

                        using (ExcelPackage package = new ExcelPackage(existingFile))
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets.First();

                            if (worksheet != null)
                            {
                                int colCount = worksheet.Dimension.End.Column;  //get Column Count
                                int rowCount = worksheet.Dimension.End.Row;      //get row count - Cabecera
                                for (int row = 2; row <= rowCount; row++)
                                {
                                    MidsComercioPlaceToPay objeto = new MidsComercioPlaceToPay();

                                    for (int col = 1; col <= colCount; col++)
                                    {
                                        var error = string.Empty;
                                        string columna = (worksheet.Cells[1, col].Value ?? "").ToString().Trim(); // Nombre de la Columna
                                        string valorColumna = (worksheet.Cells[row, col].Value ?? "").ToString().Trim();

                                        //CONTROL VALORES VACIOS
                                        if (string.IsNullOrEmpty(valorColumna))
                                        {
                                            carga.Detalles.Add(new DetallesCargaMasiva { Fila = row, Columna = col, Valor = "NULO", Error = Mensajes.MensajeDatosObligatorios });
                                            continue; //Come back to init iterate
                                        }

                                        //CONTROL DE VALIDACIONES DE VALORES POR COLUMNA
                                        switch (columna)
                                        {
                                            case "RUC":
                                                //Validar si el comercio ya existe
                                                bool existeComercio = await ComercioPlaceToPayEntity.ExisteComercioPlaceToPayAsync(valorColumna);
                                                objeto.RUC = valorColumna;

                                                if (!existeComercio)
                                                    carga.Detalles.Add(new DetallesCargaMasiva { Fila = row, Columna = col, Valor = valorColumna, Error = string.Format("El RUC {0} no se encuentra asociado a ningún comercio existente.", valorColumna) });
                                                break;
                                            case "PRODUCTO":
                                                objeto.Producto = valorColumna;
                                                break;
                                            case "MID":
                                                objeto.MID = valorColumna;

                                                string valorColumnaRUC = (worksheet.Cells[row, 1].Value ?? "").ToString().Trim();
                                                bool existeMID = await ComercioPlaceToPayEntity.ExisteMIDComercioNuevosPlaceToPayAsync(valorColumnaRUC, valorColumna);
                                                if (existeMID)
                                                    carga.Detalles.Add(new DetallesCargaMasiva { Fila = row, Columna = col, Valor = valorColumna, Error = string.Format("El MID {0} ya se encuentra asociado al RUC {1}.", valorColumna, valorColumnaRUC) });

                                                break;
                                            default:
                                                break;
                                        }
                                    }

                                    listadoFinalMIDS.Add(objeto);

                                }
                            }
                        }

                    }
                }

                List<string> erroresValidacionGenerales = new List<string>();


                string errores = carga.Detalles.ToHtmlTable(tableAttributes: new { @class = "table table-hover table-fixed table-responsive table-bordered" } //this is dynamic type, support all attribute 
    , trAttributes: new { ID = "tabla-errores-carga" }, tdAttributes: new { TextAlign = "center" }, thAttributes: new { @class = "dark-theme", TextAlign = "center" }
);

                if (!carga.GetEstado())
                    return Json(new { error = "ERRORES DE VALIDACION EN EL FORMATO DE LA CARGA.", ErroresCarga = errores, Resultado = new RespuestaTransaccion { Estado = false, Adicional = "PRUEBA PRUEBA", Respuesta = Mensajes.MensajeCargaMasivaFallida } }, JsonRequestBehavior.AllowGet);


                var resultado = await ComercioPlaceToPayEntity.CargarMIDSComercios(listadoFinalMIDS, new CargaMasivaMIDS { Archivo = fileName, UbicacionRespaldo = pathServidor, CantidadRegistros = listadoFinalMIDS.Count, Fecha = DateTime.Now, UsuarioID = GetCurrentUser() });
                return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #region REPORTES BASICOS
        public async Task<ActionResult> DescargarReporteFormatoExcelOperaciones()
        {
            var collection = await ComercioPlaceToPayEntity.ObtenerReporteExcelMidsOperaciones();
            var package = GetEXCEL(columnasReportesBasicosOperaciones, collection.Cast<object>().ToList());
            return File(package.GetAsByteArray(), XlsxContentType, "ListadoReporteBaseMidsOperaciones.xlsx");
        }

        public async Task<ActionResult> DescargarReporteFormatoExcelMidsNuevos()
        {
            var collection = await ComercioPlaceToPayEntity.ObtenerReporteExcelMidsNuevos();
            var package = GetEXCEL(columnasReportesBasicosMidsNuevos, collection.Cast<object>().ToList());
            return File(package.GetAsByteArray(), XlsxContentType, "ListadoReporteBaseMidsNuevos.xlsx");
        }

        #endregion

        #region Metodos sin uso para que funcionen los permisos
        public ActionResult IndexGrid()
        {
            return View();
        }
        #endregion

    }
}