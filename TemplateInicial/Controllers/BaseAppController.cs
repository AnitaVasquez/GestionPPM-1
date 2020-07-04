using GestionPPM.Entidades.Metodos;
using GestionPPM.Repositorios;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace TemplateInicial.Controllers
{
    public class BaseAppController : Controller
    {
        //Salida de archivos de reportes
        public const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string PDFContentType = "application/pdf";
        public const string CSVContentType = "text/csv";

        //Repositorio en el servidor para guardar y  buscar archivos
        public string basePathRepositorioDocumentos = ConfigurationManager.AppSettings["RepositorioDocumentos"];

        #region Funcionalidades Genéricas para Grids
        public string BuildWhereDynamicClause(Dictionary<string, object> queryString)
        {
            string query = string.Empty;
            try
            {
                List<string> clausulas = new List<string>();

                string contains = "{0} LIKE '%{1}%'";
                string equals = "{0} = '{1}'";
                string NotEquals = "{0} != '{1}'";
                string StartsWith = "{0} LIKE '{1}%'";
                string EndWith = "{0} LIKE '%{1}'";

                string where = string.Empty;

                foreach (KeyValuePair<string, object> item in queryString)
                {
                    string columna = item.Key.Split('-').FirstOrDefault();

                    if (item.Key.Contains("contains"))
                    {
                        //query += string.Format(contains, columna, (item.Value ?? "").ToString());
                        clausulas.Add(string.Format(contains, columna, (item.Value ?? "").ToString().Trim()));
                    }
                    if (item.Key.Contains("equals"))
                    {
                        //query += string.Format(equals, columna, (item.Value ?? "").ToString());
                        clausulas.Add(string.Format(equals, columna, item.Value.ToString().Trim()));
                    }
                    if (item.Key.Contains("not-equals"))
                    {
                        //query += string.Format(NotEquals, columna, (item.Value ?? "").ToString());
                        clausulas.Add(string.Format(NotEquals, columna, (item.Value ?? "").ToString().Trim()));
                    }
                    if (item.Key.Contains("starts-with"))
                    {
                        //query += string.Format(StartsWith, columna, (item.Value ?? "").ToString());
                        clausulas.Add(string.Format(StartsWith, columna, (item.Value ?? "").ToString().Trim()));
                    }
                    if (item.Key.Contains("ends-with"))
                    {
                        //query += string.Format(EndWith, columna, (item.Value ?? "").ToString());
                        clausulas.Add(string.Format(EndWith, columna, (item.Value ?? "").ToString().Trim()));
                    }

                    where = " WHERE ";
                }

                query += clausulas.Any() ? where + string.Join(" AND ", clausulas.ToArray()) : string.Empty;

                return query;
            }
            catch (Exception ex)
            {
                return query;
            }
        }

        public Dictionary<string, object> GetQueryString(string queryString)
        {
            Dictionary<string, object> querystringDic = new Dictionary<string, object>();
            try
            {
                var parsed = HttpUtility.ParseQueryString(queryString);
                querystringDic = parsed.AllKeys.ToDictionary(k => k, k => (object)parsed[k]);

                querystringDic.Remove("_");

                //Parametros ya incluidos en el request del método IndexGrid
                querystringDic.Remove("search");
                querystringDic.Remove("page");
                querystringDic.Remove("order");
                querystringDic.Remove("sort");

                return querystringDic;
            }
            catch (Exception ex)
            {
                return querystringDic;
            }
        }
        #endregion

        public List<string> GetMetodosControlador(string controlador)
        {
            List<string> NavItems = new List<string>();

            ReflectedControllerDescriptor controllerDesc = new ReflectedControllerDescriptor(this.GetType());
            foreach (ActionDescriptor action in controllerDesc.GetCanonicalActions())
            {
                bool validAction = true;

                object[] attributes = action.GetCustomAttributes(false);

                // Look at each attribute
                foreach (object filter in attributes)
                {
                    // Can we navigate to the action?
                    if (filter is ChildActionOnlyAttribute)
                    {
                        validAction = false;
                        break;
                    }
                }
                if (validAction)
                    NavItems.Add(action.ActionName);
            }

            return NavItems;

        }

        //Get User Session
        public int GetCurrentUser() {
            try
            {
                var user = ViewData["usuario"] = System.Web.HttpContext.Current.Session["usuario"];
                var usuario = int.Parse(user.ToString());
                return usuario;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public ExcelPackage GetEXCEL(List<string> columnas, List<object> collection, string nombreHoja = "Listado")
        {
            ExcelPackage package = new ExcelPackage();
            var workSheet = package.Workbook.Worksheets.Add(nombreHoja);
            try
            {
                workSheet.TabColor = System.Drawing.Color.Black;
                workSheet.DefaultRowHeight = 10;

                workSheet.Row(1).Height = 20;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;

                int contador = 0;
                for (int i = 1; i <= columnas.Count; i++)
                {
                    workSheet.Cells[1, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Cells[1, i].Style.Font.Name = "Raleway";
                    workSheet.Cells[1, i].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    workSheet.Cells[1, i].Value = columnas.ElementAt(contador);
                    contador++;
                }

                //Body of table  
                CambiarColorFila(workSheet, 1, columnas.Count, System.Drawing.Color.FromArgb(240, 240, 240));

                int fila = 2;
                foreach (var item in collection)
                {
                    var objeto = Auxiliares.GetValoresCamposObjeto(item);
                    int columna = 1;
                    foreach (var valor in objeto)
                    {
                        workSheet.Cells[fila, columna].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        workSheet.Cells[fila, columna].Style.Font.Name = "Raleway";
                        workSheet.Cells[fila, columna].Value = valor;
                        columna++;
                    }
                    fila++;
                }

                for (int i = 1; i <= columnas.Count; i++)
                {
                    workSheet.Column(i).AutoFit();
                    workSheet.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                return package;
            }
            catch (Exception ex)
            {
                return package;
            }
        }

        private static void CambiarColorFila(ExcelWorksheet hoja, int fila, int totalColumnas, System.Drawing.Color color)
        {
            for (int i = 1; i <= totalColumnas; i++)
            {
                using (ExcelRange rowRange = hoja.Cells[fila, i])
                {
                    rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rowRange.Style.Fill.BackgroundColor.SetColor(color);
                }
            }
        }

        public byte[] GetCSV(List<string> columnas, List<object> collection)
        {
            // Build the file content
            var listadoCSV = new StringBuilder();
            collection.ForEach(line =>
            {
                listadoCSV.AppendLine(string.Join(",", line));
            });

            byte[] buffer = Encoding.Default.GetBytes($"{string.Join(",", columnas)}\r\n{listadoCSV.ToString()}");

            return buffer;
        }



    }
}