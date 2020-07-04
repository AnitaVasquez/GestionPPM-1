using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;

//Extensión para Query string dinámico
using System.Linq.Dynamic;
using System.Web;
using GestionPPM.Entidades.Metodos;
using GestionPPM.Entidades.Modelo.PlaceToPay;
using GestionPPM.Repositorios;
using Seguridad.Helper;
using GestionPPM.Entidades.Modelo;
using System.IO;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace TemplateInicial.Controllers
{
    [Autenticado]
    public class SeguimientoPPMController : BaseAppController
    {
        private List<string> columnasReportesBasicos = new List<string> { "FECHA DE SOLICITUD", "USUARIO", "EQUIPOS", "HERRAMIENTAS ADICIONALES", "EMPRESA", "CARGO", "DEPARTAMENTO" };

        // GET: ComercioPlaceToPay
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<PartialViewResult> _IndexGrid(string search, string sort = "", string order = "", long? page = 1)
        {
            page = page > 0 ? page - 1 : page;
            int totalPaginas = 1;
            var listado = new List<SeguimientoComercioPlaceToPayInfo>();


            ViewBag.NombreListado = Etiquetas.TituloGridComercioPlaceToPay;
            //Controlar permisos
            var user = ViewData["usuario"] = System.Web.HttpContext.Current.Session["usuario"];
            var usuario = int.Parse(user.ToString());
            string nombreControlador = ControllerContext.RouteData.Values["controller"].ToString();
            ViewBag.NombreControlador = nombreControlador;

            ViewBag.AccionesUsuario = ManejoPermisosEntity.ListadoAccionesCatalogoUsuario(usuario, nombreControlador);

            //Obtener Acciones del controlador
            ViewBag.AccionesControlador = GetMetodosControlador(nombreControlador);

            try
            {
                var query = (HttpContext.Request.Params.Get("QUERY_STRING") ?? "").ToString();

                var dynamicQueryString = GetQueryString(query);
                var whereClause = BuildWhereDynamicClause(dynamicQueryString);

                //Siempre y cuando no haya filtros definidos en el Grid
                if (string.IsNullOrEmpty(whereClause))
                {
                    if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
                        listado = ComercioPlaceToPayEntity.ListadoSeguimientoComercioPlaceToPay(page.Value).OrderBy(sort + " " + order).ToList();
                    else
                        listado = ComercioPlaceToPayEntity.ListadoSeguimientoComercioPlaceToPay(page.Value).ToList();
                }

                search = !string.IsNullOrEmpty(search) ? search.Trim() : "";

                if (!string.IsNullOrEmpty(search))//filter
                {
                    listado = ComercioPlaceToPayEntity.ListadoSeguimientoComercioPlaceToPay(null, search);
                }

                if (!string.IsNullOrEmpty(whereClause) && string.IsNullOrEmpty(search))
                {
                    if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
                        listado = ComercioPlaceToPayEntity.ListadoSeguimientoComercioPlaceToPay(null, null, whereClause).OrderBy(sort + " " + order).ToList();
                    else
                        listado = ComercioPlaceToPayEntity.ListadoSeguimientoComercioPlaceToPay(null, null, whereClause);
                }
                else
                {

                    if (string.IsNullOrEmpty(search))
                        totalPaginas = ComercioPlaceToPayEntity.ObtenerTotalRegistrosListadoSeguimientoComercioPlaceToPay();
                }

                ViewBag.TotalPaginas = totalPaginas;

                // Only grid query values will be available here.
                return PartialView(await Task.Run(() => listado));
            }
            catch (Exception ex)
            {
                ViewBag.TotalPaginas = totalPaginas;
                // Only grid query values will be available here.
                return PartialView(await Task.Run(() => listado));
            }
        }

        public async Task<ActionResult> Formulario(int? id)
        {
            try
            {
                var usuario = UsuarioEntity.ConsultarUsuario(GetCurrentUser());

                ComercioPlaceToPay model = new ComercioPlaceToPay { FechaAfiliacion = DateTime.Now, Fecha = DateTime.Now, NombresEjecutivo = usuario.nombre_usuario, ApellidosEjecutivo = usuario.apellido_usuario, CorreoElectronicoEjecutivo = usuario.mail_usuario };
                List<MidsComercioPlaceToPay> detalles = new List<MidsComercioPlaceToPay>();

                if (id.HasValue)
                {
                    model = await ComercioPlaceToPayEntity.GetComercioPlaceToPayAsync(id.Value);
                    detalles = await ComercioPlaceToPayEntity.GetMidsComercioPlaceToPayAsync(id.Value);
                }

                //Si es un nuevo registro a crear o si es nuevo comercio a crear que todavia no tiene segmento asociado
                if (!id.HasValue || !model.Segmento.HasValue)
                {
                    //SELECCIONAR FASE GESTION APROBADA POR DEFECTO CUANDO ES UN NUEVO REGISTRO
                    var catalogo = CatalogoEntity.ConsultarCatalogoPorCodigo("COD-GESTION-APROBADA");
                    int idCatalogoPreSeleccionado = catalogo != null ? catalogo.id_catalogo : 0;
                    model.FaseGestion = idCatalogoPreSeleccionado;
                }

                ViewBag.ListadoDetalles = detalles;

                return View(model);
            }
            catch (Exception ex)
            {
                return View(new ComercioPlaceToPay { FechaAfiliacion = DateTime.Now, Fecha = DateTime.Now });
            }
        }

        public async Task<ActionResult> _ConsultarFasesGxC(string id)
        {
            ViewBag.TituloModal = string.Format("Consultar Fases o Estados GXC del comercio {0}", id);
            var model = await ComercioPlaceToPayEntity.GetEstadosComerciosByRuc(id);
            return PartialView(model);
        }


        //Busqueda por RUC
        public JsonResult _GetInformacionPrincipalComercio(string busqueda)
        {
            List<AutoCompleteUI> items = new List<AutoCompleteUI>();
            busqueda = (busqueda ?? "").ToLower().Trim();

            Regex trimmer = new Regex(@"\s\s+");

            var results = ComercioPlaceToPayEntity.ConsultarComercioPlaceToPayPorRUC(busqueda).GroupBy(p => p.CodigoUnico).Select(g => g.First()).ToList();
            //var results = ComercioPlaceToPayEntity.ConsultarComercioPlaceToPayPorRUC(busqueda).ToList();

            items = results.Select(o => new AutoCompleteUI(o.IDComercioPlaceToPay, o.RUC + " - " + o.Establecimiento + " - " + o.MID, o.FechaAfiliacion.HasValue ? o.FechaAfiliacion.Value.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd"), new Dictionary<string, ComercioPlaceToPayInfo>(){
                { o.IDComercioPlaceToPay.ToString(), new ComercioPlaceToPayInfo {
                    IDComercioPlaceToPay = o.IDComercioPlaceToPay,
                    RUC = o.RUC,
                    TipoCodigo = o.TipoCodigo,
                    TextoCatalogoTipoCodigo = o.TextoCatalogoTipoCodigo,
                    CodigoUnico = o.CodigoUnico,
                    FechaAfiliacion = o.FechaAfiliacion ?? DateTime.Now,
                    Establecimiento = o.Establecimiento,
                    MID = o.MID,
                    Especialidad = o.Especialidad,
                    NombreRepresentanteLegal = o.NombreRepresentanteLegal,
                    RazonSocial = o.RazonSocial,
                    DireccionComercio =  trimmer.Replace((o.DireccionComercio ?? string.Empty), " "),
                    Mail = o.Mail,
                    Marca = o.Marca,
                    Prefijo1 = o.Prefijo1,
                    Prefijo2 = o.Prefijo2,
                    Prefijo3 = o.Prefijo3,
                    Prefijo4 = o.Prefijo4,
                    Telefono1 = o.Telefono1,
                    Telefono2 = o.Telefono2,
                    Telefono3 = o.Telefono3,
                    Telefono4 = o.Telefono4,
                    TelefonoCompleto1 = o.TelefonoCompleto1,
                    TelefonoCompleto2 = o.TelefonoCompleto2,
                    TelefonoCompleto3 = o.TelefonoCompleto3,
                    TelefonoCompleto4 = o.TelefonoCompleto4,
                    MarcaTarjeta = o.MarcaTarjeta
                } }
            })).ToList();

            return Json(new { results = items }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> Create(ComercioPlaceToPay formulario, List<string> archivos, List<MidsComercioPlaceToPay> midsAsociados)
        {
            try
            {
                if (!Validaciones.VerificaIdentificacion(formulario.IdentificacionRepresentanteLegal))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = string.Format(Mensajes.MensajeIdentificacionIncorrecto, formulario.IdentificacionRepresentanteLegal) } }, JsonRequestBehavior.AllowGet);

                if (!Validaciones.VerificaIdentificacion(formulario.IdentificacionAdministrador))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = string.Format(Mensajes.MensajeIdentificacionIncorrecto, formulario.IdentificacionAdministrador) } }, JsonRequestBehavior.AllowGet);

                if (!Validaciones.ValidarMail(formulario.CorreoElectronicoEjecutivo))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = string.Format(Mensajes.MensajeEmailIncorrecto, formulario.CorreoElectronicoEjecutivo) } }, JsonRequestBehavior.AllowGet);

                if (!Validaciones.ValidarMail(formulario.CorreoElectronicoLiderProyectoComercio))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = string.Format(Mensajes.MensajeEmailIncorrecto, formulario.CorreoElectronicoLiderProyectoComercio) } }, JsonRequestBehavior.AllowGet);

                //if(midsAsociados == null)
                //    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeDatosObligatorios } }, JsonRequestBehavior.AllowGet);

                #region Guardar archivos adjuntos

                string mensajeAdvertenciaAdjuntoNoGenerado = string.Empty;
                bool ok = false;

                if (archivos != null)
                {
                    string rutaBase = basePathRepositorioDocumentos + "\\GESTION_PPM\\PLACETOPAY\\PropuestasAdjuntas";
                    bool existeRutaDisco = Directory.Exists(rutaBase); // VERIFICAR SI ESA RUTA EXISTE

                    if (!existeRutaDisco)
                        Directory.CreateDirectory(rutaBase);

                    string adjuntoDetalle = archivos.ElementAt(0);
                    string identificadorArchivo = !string.IsNullOrEmpty(formulario.CodigoUnico) ? formulario.CodigoUnico : Guid.NewGuid().ToString().Substring(0, 10);

                    string pathFinal = Path.Combine(rutaBase, "PropuestaComercio " + identificadorArchivo + ".pdf");

                    //Decodificar y guardar en ruta.
                    ok = Auxiliares.Base64Decode(adjuntoDetalle, pathFinal);

                    //Solo si el archivo se logra decodificar correctamente
                    if (ok)
                        formulario.PropuestaAdjuntada = pathFinal;
                    else
                        mensajeAdvertenciaAdjuntoNoGenerado = Mensajes.MensajeAdjuntoFallido;
                }
                else
                {
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeArchivoNoExiste } }, JsonRequestBehavior.AllowGet);
                }

                #endregion

                formulario.CreatedAt = DateTime.Now;
                formulario.CreatedBy = GetCurrentUser();

                var Resultado = await ComercioPlaceToPayEntity.CrearComercioPlaceToPay(formulario, midsAsociados ?? new List<MidsComercioPlaceToPay>());

                //Agregar mensaje de advertencia si el archivo no fue generado correctamente
                if (!ok)
                    Resultado.Respuesta += mensajeAdvertenciaAdjuntoNoGenerado;

                return Json(new { Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message + " ; " + ex.InnerException.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Edit(ComercioPlaceToPay formulario, List<string> archivos, List<MidsComercioPlaceToPay> midsAsociados)
        {
            try
            {
                if (!Validaciones.VerificaIdentificacion(formulario.IdentificacionRepresentanteLegal))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = string.Format(Mensajes.MensajeIdentificacionIncorrecto, formulario.IdentificacionRepresentanteLegal) } }, JsonRequestBehavior.AllowGet);

                if (!Validaciones.VerificaIdentificacion(formulario.IdentificacionAdministrador))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = string.Format(Mensajes.MensajeIdentificacionIncorrecto, formulario.IdentificacionAdministrador) } }, JsonRequestBehavior.AllowGet);

                if (!Validaciones.ValidarMail(formulario.CorreoElectronicoEjecutivo))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = string.Format(Mensajes.MensajeEmailIncorrecto, formulario.CorreoElectronicoEjecutivo) } }, JsonRequestBehavior.AllowGet);

                if (!Validaciones.ValidarMail(formulario.CorreoElectronicoLiderProyectoComercio))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = string.Format(Mensajes.MensajeEmailIncorrecto, formulario.CorreoElectronicoLiderProyectoComercio) } }, JsonRequestBehavior.AllowGet);

                //if (midsAsociados == null)
                //    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeDatosObligatorios } }, JsonRequestBehavior.AllowGet);

                #region Guardar archivos adjuntos

                string mensajeAdvertenciaAdjuntoNoGenerado = string.Empty;
                bool ok = false;
                if (archivos != null)
                {
                    string rutaBase = basePathRepositorioDocumentos + "\\GESTION_PPM\\PLACETOPAY\\PropuestasAdjuntas";
                    bool existeRutaDisco = Directory.Exists(rutaBase); // VERIFICAR SI ESA RUTA EXISTE

                    if (!existeRutaDisco)
                        Directory.CreateDirectory(rutaBase);

                    string adjuntoDetalle = archivos.ElementAt(0);
                    string identificadorArchivo = !string.IsNullOrEmpty(formulario.CodigoUnico) ? formulario.CodigoUnico : Guid.NewGuid().ToString().Substring(0, 10);

                    string pathFinal = Path.Combine(rutaBase, "PropuestaComercio " + identificadorArchivo + ".pdf");

                    //Decodificar y guardar en ruta.
                    ok = Auxiliares.Base64Decode(adjuntoDetalle, pathFinal);

                    //Solo si el archivo se logra decodificar correctamente
                    if (ok)
                        formulario.PropuestaAdjuntada = pathFinal;
                    else
                        mensajeAdvertenciaAdjuntoNoGenerado = Mensajes.MensajeAdjuntoFallido;
                }

                #endregion

                formulario.UpdatedAt = DateTime.Now;
                formulario.UpdatedBy = GetCurrentUser();

                var Resultado = await ComercioPlaceToPayEntity.ActualizarComercioPlaceToPay(formulario, midsAsociados ?? new List<MidsComercioPlaceToPay>());

                return Json(new { Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> _CambiarFaseGestion(int id, string ruc)
        {
            ViewBag.TituloModal = "Cambiar Fase de Gestión PPM";
            ComercioPlaceToPay model = await ComercioPlaceToPayEntity.GetComercioPlaceToPayAsync(id);

            ViewBag.TrackingFasesComercio = ComercioPlaceToPayEntity.ConsultarTrackingPPMFasesComercio(id, ruc);

            return PartialView(model);
        }

        public async Task<ActionResult> _AsignarEjecutivoComercialResponsable(int id)
        {
            ViewBag.TituloModal = "ASIGNACION DE RESPONSABLE";
            ComercioPlaceToPay model = await ComercioPlaceToPayEntity.GetComercioPlaceToPayAsync(id);

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult RegistrarFaseGestion(ComercioPlaceToPay formulario)
        {
            try
            {
                formulario.UpdatedAt = DateTime.Now;
                formulario.UpdatedBy = GetCurrentUser();

                RespuestaTransaccion resultado = ComercioPlaceToPayEntity.CambiarFaseGestionPPMComercio(formulario);
                return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AsignarResponsableComercio(ComercioPlaceToPay formulario)
        {
            try
            {
                formulario.UpdatedAt = DateTime.Now;
                formulario.UpdatedBy = GetCurrentUser();

                RespuestaTransaccion resultado = ComercioPlaceToPayEntity.AsignacionResponsableComercio(formulario);
                return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        #region Metodos sin uso para que funcionen los permisos
        public ActionResult IndexGrid()
        {
            return View();
        }
        #endregion

        #region REPORTES BASICOS
        public ActionResult DescargarReporteFormatoExcel()
        {
            var collection = ComercioPlaceToPayEntity.ListadoComercioPlaceToPay();
            var package = GetEXCEL(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(package.GetAsByteArray(), XlsxContentType, "ListadoReporte.xlsx");
        }

        public ActionResult DescargarReporteFormatoCSV()
        {
            var collection = ComercioPlaceToPayEntity.ListadoComercioPlaceToPay();
            byte[] buffer = GetCSV(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(buffer, CSVContentType, $"ListadoReporte.csv");
        }
        #endregion
    }
}