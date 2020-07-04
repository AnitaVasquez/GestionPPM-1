using GestionPPM.Entidades.Modelo.PlaceToPay;
using GestionPPM.Repositorios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace GestionPPM.Entidades.Metodos
{
    public class ComercioPlaceToPayEntity
    {
        private static readonly PlaceToPay db = new PlaceToPay();
        private static readonly QPHEntities dbGxC = new QPHEntities();

        public static async Task<RespuestaTransaccion> CrearComercioPlaceToPay(ComercioPlaceToPay objeto, List<MidsComercioPlaceToPay> midsAsociados)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    objeto.Estado = true;

                    db.ComercioPlaceToPay.Add(objeto);
                    await db.SaveChangesAsync();

                    // REGISTRANDO FASE GESTION
                    db.TrackingFasesComercio.Add(new TrackingFasesComercio
                    {
                        RUC = objeto.RUC,
                        Estado = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = objeto.CreatedBy.Value,
                        FaseID = objeto.FaseGestion.Value,
                        ResponsableID = objeto.UsuarioAsignadoID,
                        Observacion = string.Empty,
                        ComercioPlaceToPayID = objeto.IDComercioPlaceToPay
                    });

                    await db.SaveChangesAsync();

                    foreach (var item in midsAsociados)
                    {
                        MidsComercioPlaceToPay mid = new MidsComercioPlaceToPay
                        {
                            IDComerciosPlaceToPay = objeto.IDComercioPlaceToPay,
                            CodigoUnico = objeto.CodigoUnico,
                            RUC = objeto.RUC,
                            Producto = item.Producto,
                            MID = item.MID
                        };

                        db.MidsComercioPlaceToPay.Add(mid);
                        await db.SaveChangesAsync();
                    }

                    transaction.Commit();

                    return new RespuestaTransaccion
                    {
                        Estado = true,
                        Respuesta = Mensajes.MensajeTransaccionExitosa
                    };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new RespuestaTransaccion
                    {
                        Estado = false,
                        Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString()
                    };
                }
            }
        }

        public static async Task<RespuestaTransaccion> CargarMIDSComercios(List<MidsComercioPlaceToPay> listado, CargaMasivaMIDS informacionCargaMasiva)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in listado)
                    {
                        var comercio = await GetComercioPlaceToPayByRUCAsync(item.RUC);

                        item.IDComerciosPlaceToPay = comercio.IDComercioPlaceToPay;
                        item.CodigoUnico = comercio.CodigoUnico;
                        //var listadoMIDS = item.MID.Split(',').ToList();

                        MidsComercioPlaceToPay entidad = new MidsComercioPlaceToPay
                        {
                            IDComerciosPlaceToPay = item.IDComerciosPlaceToPay,
                            CodigoUnico = item.CodigoUnico,
                            RUC = item.RUC,
                            Producto = item.Producto,
                            MID = item.MID
                        };

                        db.MidsComercioPlaceToPay.Add(entidad);
                        await db.SaveChangesAsync();
                    }

                    // REGISTRANDO LOG DE CARGA MASIVA
                    db.CargaMasivaMIDS.Add(informacionCargaMasiva);

                    await db.SaveChangesAsync();

                    transaction.Commit();

                    return new RespuestaTransaccion
                    {
                        Estado = true,
                        Respuesta = Mensajes.MensajeTransaccionExitosa
                    };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new RespuestaTransaccion
                    {
                        Estado = false,
                        Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString()
                    };
                }
            }
        }


        public static async Task<RespuestaTransaccion> ActualizarComercioPlaceToPay(ComercioPlaceToPay objeto, List<MidsComercioPlaceToPay> midsAsociados)
        {
            try
            {
                // assume Entity base class have an Id property for all items
                var entity = db.ComercioPlaceToPay.Find(objeto.IDComercioPlaceToPay);

                if (entity == null)
                    return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida };

                db.Entry(entity).CurrentValues.SetValues(objeto);

                await db.SaveChangesAsync();

                // Limpiar primero los detalles anteriores
                var detallesAnteriores = db.MidsComercioPlaceToPay.Where(s => s.IDComerciosPlaceToPay == objeto.IDComercioPlaceToPay).ToList();
                foreach (var item in detallesAnteriores)
                {
                    db.MidsComercioPlaceToPay.Remove(item);
                    await db.SaveChangesAsync();
                }

                foreach (var item in midsAsociados)
                {
                    MidsComercioPlaceToPay mid = new MidsComercioPlaceToPay
                    {
                        IDComerciosPlaceToPay = objeto.IDComercioPlaceToPay,
                        CodigoUnico = objeto.CodigoUnico,
                        RUC = objeto.RUC,
                        Producto = item.Producto,
                        MID = item.MID
                    };

                    db.MidsComercioPlaceToPay.Add(mid);
                    await db.SaveChangesAsync();
                }

                return new RespuestaTransaccion
                {
                    Estado = true,
                    Respuesta = Mensajes.MensajeTransaccionExitosa
                };
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion
                {
                    Estado = false,
                    Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString()
                };
            }
        }

        public static RespuestaTransaccion CambiarFaseGestionPPMComercio(ComercioPlaceToPay comercio)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var objeto = db.ComercioPlaceToPay.Find(comercio.IDComercioPlaceToPay);
                    // Editar la fase gestion
                    objeto.FaseGestion = comercio.FaseGestion;
                    //Registrar auditoria
                    objeto.UpdatedAt = comercio.UpdatedAt;
                    objeto.UpdatedBy = comercio.UpdatedBy;

                    db.Entry(objeto).State = EntityState.Modified;
                    db.SaveChanges();

                    // REGISTRANDO FASE GESTION
                    db.TrackingFasesComercio.Add(new TrackingFasesComercio
                    {
                        RUC = objeto.RUC,
                        Estado = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = objeto.CreatedBy.Value,
                        FaseID = objeto.FaseGestion.Value,
                        ResponsableID = objeto.UsuarioAsignadoID,
                        Observacion = string.Empty,
                        ComercioPlaceToPayID = objeto.IDComercioPlaceToPay
                    });

                    db.SaveChanges();

                    transaction.Commit();

                    return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new RespuestaTransaccion
                    {
                        Estado = false,
                        Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() 
                    };
                }
            }
        }


        public static RespuestaTransaccion AsignacionResponsableComercio(ComercioPlaceToPay comercio)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var objeto = db.ComercioPlaceToPay.Find(comercio.IDComercioPlaceToPay);

                    //Editar el responsable del comercio
                    objeto.UsuarioAsignadoID = comercio.UsuarioAsignadoID;
                    //Registrar auditoria
                    objeto.UpdatedAt = comercio.UpdatedAt;
                    objeto.UpdatedBy = comercio.UpdatedBy;

                    db.Entry(objeto).State = EntityState.Modified;
                    db.SaveChanges();

                    transaction.Commit();

                    return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new RespuestaTransaccion
                    {
                        Estado = false,
                        Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() 
                    };
                }
            }
        }

        public static async Task<List<ComercioPlaceToPayInfo>> ListadoComercioPlaceToPayAsync(long? pagina = null, string textoBusqueda = null, string filtro = null, int? id = null)
        {
            List<ComercioPlaceToPayInfo> listado = new List<ComercioPlaceToPayInfo>();
            try
            {
                if (!id.HasValue)
                    listado = await db.ListadoComercioPlaceToPay(pagina, textoBusqueda, filtro).AsQueryable().ToListAsync(); // Listado Completo
                else
                {
                    filtro = " WHERE IDComercioPlaceToPay = '{0}' ";

                    filtro = id.HasValue ? string.Format(filtro, id) : null;
                    listado = await db.ListadoComercioPlaceToPay(null, null, filtro).AsQueryable().ToListAsync(); // Consulta por ID
                }

                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static List<ComercioPlaceToPayInfo> ListadoComercioPlaceToPay(long? pagina = null, string textoBusqueda = null, string filtro = null, int? id = null)
        {
            List<ComercioPlaceToPayInfo> listado = new List<ComercioPlaceToPayInfo>();
            try
            {

                if (!id.HasValue)
                    listado = db.ListadoComercioPlaceToPay(pagina, textoBusqueda, filtro).ToList(); // Listado Completo
                else
                {
                    filtro = " WHERE IDComercioPlaceToPay = '{0}' ";

                    filtro = id.HasValue ? string.Format(filtro, id) : null;
                    listado = db.ListadoComercioPlaceToPay(null, null, filtro).ToList(); // Consulta por ID
                }

                var updatedList = new List<ComercioPlaceToPayInfo>();
                foreach (var item in listado)
                {
                    var gestionGxc = dbGxC.ConsultarFaseGestionActualComercioGxC(item.RUC).FirstOrDefault();
                    item.FaseGestionGxC = gestionGxc != null ? gestionGxc.Fase : string.Empty;

                    updatedList.Add(item);
                }

                listado = updatedList;

                return listado;
            }
            catch (Exception ex)
            {
                return listado;
            }
        }

        public static List<SeguimientoComercioPlaceToPayInfo> ListadoSeguimientoComercioPlaceToPay(long? pagina = null, string textoBusqueda = null, string filtro = null, int? id = null)
        {
            List<SeguimientoComercioPlaceToPayInfo> listado = new List<SeguimientoComercioPlaceToPayInfo>();
            try
            {

                if (!id.HasValue)
                    listado = db.ListadoSeguimientoComercioPlaceToPay(pagina, textoBusqueda, filtro).ToList(); // Listado Completo
                else
                {
                    filtro = " WHERE IDComercioPlaceToPay = '{0}' ";

                    filtro = id.HasValue ? string.Format(filtro, id) : null;
                    listado = db.ListadoSeguimientoComercioPlaceToPay(null, null, filtro).ToList(); // Consulta por ID
                }

                var updatedList = new List<SeguimientoComercioPlaceToPayInfo>();
                foreach (var item in listado)
                {
                    var gestionGxc = dbGxC.ConsultarFaseGestionActualComercioGxC(item.RUC).FirstOrDefault();
                    item.FaseGestionGxC = gestionGxc != null ? gestionGxc.Fase : string.Empty;

                    updatedList.Add(item);
                }

                listado = updatedList;

                return listado;
            }
            catch (Exception ex)
            {
                return listado;
            }
        }

        public static async Task<ComercioPlaceToPay> GetComercioPlaceToPayAsync(int id)
        {
            try
            {
                var entity = await db.ComercioPlaceToPay.SingleOrDefaultAsync(s => s.IDComercioPlaceToPay == id);
                return entity;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<ComercioPlaceToPay> GetComercioPlaceToPayByRUCAsync(string ruc)
        {
            try
            {
                var entity = await db.ComercioPlaceToPay.SingleOrDefaultAsync(s => s.RUC == ruc && s.Segmento.HasValue);
                return entity;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<bool> ExisteComercioPlaceToPayAsync(string ruc, int? id = null)
        {
            try
            {
                var entity = id.HasValue ? await db.ComercioPlaceToPay.SingleOrDefaultAsync(s => s.RUC == ruc && s.Segmento.HasValue && s.IDComercioPlaceToPay != id) : await db.ComercioPlaceToPay.SingleOrDefaultAsync(s => s.RUC == ruc && s.Segmento.HasValue);

                if (entity != null)
                    return true;
                else
                    return false;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static async Task<bool> ExisteMIDComercioNuevosPlaceToPayAsync(string ruc, string mid)
        {
            try
            {
                var entity = await db.MidsComercioPlaceToPay.SingleOrDefaultAsync(s => s.RUC == ruc && s.MID == mid);

                if (entity != null)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public static async Task<List<TrackingComercioInfo>> GetEstadosComerciosByRuc(string ruc)
        {
            try
            {
                return await Task.Run(() =>
                { // no await here and function as a whole is not async

                    List<TrackingComercioInfo> trackingComercioInfo = dbGxC.ConsultarTrackingComercio(ruc).ToList();
                    return trackingComercioInfo;
                });
            }
            catch (Exception ex)
            {
                return new List<TrackingComercioInfo>();
            }
        }

        public static async Task<List<ReporteExcelMidsNuevosInfo>> ObtenerReporteExcelMidsNuevos()
        {
            try
            {
                return await Task.Run(() =>
                { // no await here and function as a whole is not async

                    List<ReporteExcelMidsNuevosInfo> listado = db.ReporteExcelMidsNuevos().ToList();
                    return listado;
                });
            }
            catch (Exception ex)
            {
                return new List<ReporteExcelMidsNuevosInfo>();
            }
        }

        public static async Task<List<ReporteExcelMidsOperacionesInfo>> ObtenerReporteExcelMidsOperaciones()
        {
            try
            {
                return await Task.Run(() =>
                { // no await here and function as a whole is not async

                    List<ReporteExcelMidsOperacionesInfo> listado = db.ReporteExcelMidsOperaciones().ToList();
                    return listado;
                });
            }
            catch (Exception ex)
            {
                return new List<ReporteExcelMidsOperacionesInfo>();
            }
        }


        public static async Task<List<MidsComercioPlaceToPay>> GetMidsComercioPlaceToPayAsync(int id)
        {
            try
            {
                var entity = await db.MidsComercioPlaceToPay.Where(s => s.IDComerciosPlaceToPay == id).ToListAsync();
                return entity;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<ComercioPlaceToPay> GetInformacionComercioPlaceToPayAsync(string ruc)
        {
            try
            {
                var entity = await db.ComercioPlaceToPay.SingleOrDefaultAsync(s => s.RUC.Contains(ruc));
                return entity;
            }
            catch (Exception ex)
            {
                return new ComercioPlaceToPay();
            }
        }

        public static List<ComercioPlaceToPayInfo> ConsultarComercioPlaceToPayPorRUC(string parametro)
        {
            List<ComercioPlaceToPayInfo> listado = new List<ComercioPlaceToPayInfo>();
            try
            {
                string filtro = " WHERE RUC LIKE '%{0}%' OR Establecimiento LIKE '%{1}%' ";

                filtro = string.Format(filtro, parametro, parametro); //Por RUC o por Establecimiento
                listado = db.ListadoComercioPlaceToPay(0, null, filtro).ToList();
                return listado;
            }
            catch (Exception ex)
            {
                return listado;
            }
        }

        public static List<TrackingPPMFasesComercioInfo> ConsultarTrackingPPMFasesComercio(int id, string ruc = null)
        {
            List<TrackingPPMFasesComercioInfo> listado = new List<TrackingPPMFasesComercioInfo>();
            try
            {
                listado = db.ConsultarTrackingPPMFasesComercio(id, ruc).ToList();
                return listado;
            }
            catch (Exception ex)
            {
                return listado;
            }
        }

        public static int ObtenerTotalRegistrosListadoComercioPlaceToPay()
        {
            int total = 0;
            try
            {
                total = db.Database.SqlQuery<int>("SELECT [dbo].[ObtenerTotalRegistrosListadoComercioPlaceToPay]()").Single();
                return total;
            }
            catch (Exception ex)
            {
                return total;
            }
        }

        public static int ObtenerTotalRegistrosListadoSeguimientoComercioPlaceToPay()
        {
            int total = 0;
            try
            {
                total = db.Database.SqlQuery<int>("SELECT [dbo].[ObtenerTotalRegistrosListadoSeguimientoComercioPlaceToPay]()").Single();
                return total;
            }
            catch (Exception ex)
            {
                return total;
            }
        }
    }
}