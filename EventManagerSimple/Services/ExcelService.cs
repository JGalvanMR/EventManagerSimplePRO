using ClosedXML.Excel;
using EventManager.Models;
using System.Data;
using System.Globalization;

namespace EventManager.Services
{
    public class ExcelService
    {
        public List<UnidadNegocio> ProcesarArchivo(string filePath)
        {
            var unidadesNegocio = new List<UnidadNegocio>();

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Archivo no encontrado", filePath);

            using (var workbook = new XLWorkbook(filePath))
            {
                foreach (var worksheet in workbook.Worksheets)
                {
                    var unidad = ProcesarHoja(worksheet);
                    if (unidad != null && unidad.Invitados.Any())
                    {
                        unidadesNegocio.Add(unidad);
                    }
                }
            }

            return unidadesNegocio;
        }

        private UnidadNegocio ProcesarHoja(IXLWorksheet worksheet)
        {
            var unidad = new UnidadNegocio
            {
                Nombre = worksheet.Name.Trim()
            };

            // Encontrar fila de encabezados
            var firstRow = worksheet.FirstRowUsed();
            if (firstRow == null) return unidad;

            // Buscar índices de columnas
            Dictionary<string, int> columnas = ObtenerIndicesColumnas(firstRow);

            // Validar columnas requeridas
            if (!ValidarColumnasRequeridas(columnas, worksheet.Name))
                return unidad;

            // Procesar filas
            var lastRow = worksheet.LastRowUsed();
            for (int row = firstRow.RowNumber() + 1; row <= lastRow.RowNumber(); row++)
            {
                var invitado = ProcesarFila(worksheet, row, columnas, unidad.Nombre);
                if (invitado != null)
                {
                    unidad.Invitados.Add(invitado);
                }
            }

            return unidad;
        }

        private Dictionary<string, int> ObtenerIndicesColumnas(IXLRow headerRow)
        {
            var indices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var cell in headerRow.CellsUsed())
            {
                string header = cell.GetString().Trim();
                if (!string.IsNullOrEmpty(header))
                {
                    indices[header] = cell.Address.ColumnNumber;
                }
            }

            return indices;
        }

        private bool ValidarColumnasRequeridas(Dictionary<string, int> columnas, string nombreHoja)
        {
            var requeridas = new[] { "ID", "NOMBRE", "INGRESO", "CORREO" };

            foreach (var req in requeridas)
            {
                if (!columnas.ContainsKey(req))
                {
                    MessageBox.Show($"Hoja '{nombreHoja}': Falta columna requerida '{req}'",
                                  "Error de Formato", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            return true;
        }

        private Invitado ProcesarFila(IXLWorksheet worksheet, int row, Dictionary<string, int> columnas, string unidadNegocio)
        {
            try
            {
                // Obtener valores
                int id = worksheet.Cell(row, columnas["ID"]).GetValue<int>();
                string nombre = worksheet.Cell(row, columnas["NOMBRE"]).GetString().Trim();
                string fechaStr = worksheet.Cell(row, columnas["INGRESO"]).GetString().Trim();
                string correo = worksheet.Cell(row, columnas["CORREO"]).GetString().Trim();

                // Validaciones básicas
                if (id <= 0 || string.IsNullOrWhiteSpace(nombre) ||
                    string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(fechaStr))
                    return null;

                // Parsear fecha (aceptar múltiples formatos)
                DateTime fechaIngreso;
                if (!DateTime.TryParseExact(fechaStr,
                    new[] { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "yyyy-MM-dd" },
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaIngreso))
                {
                    // Si no se puede parsear, intentar con formato genérico
                    if (!DateTime.TryParse(fechaStr, out fechaIngreso))
                    {
                        return null;
                    }
                }

                // Validar email básico
                if (!correo.Contains("@") || !correo.Contains("."))
                    return null;

                return new Invitado
                {
                    ID = id,
                    Nombre = nombre,
                    FechaIngreso = fechaIngreso,
                    Correo = correo,
                    UnidadNegocio = unidadNegocio
                };
            }
            catch
            {
                return null;
            }
        }

        public void ExportarExcel(List<UnidadNegocio> unidades, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                foreach (var unidad in unidades)
                {
                    var worksheet = workbook.Worksheets.Add(unidad.Nombre);

                    // Encabezados
                    worksheet.Cell(1, 1).Value = "ID";
                    worksheet.Cell(1, 2).Value = "NOMBRE";
                    worksheet.Cell(1, 3).Value = "INGRESO";
                    worksheet.Cell(1, 4).Value = "CORREO";
                    worksheet.Cell(1, 5).Value = "UNIDAD NEGOCIO";
                    worksheet.Cell(1, 6).Value = "AÑOS SERVICIO";
                    worksheet.Cell(1, 7).Value = "BOLETOS";
                    worksheet.Cell(1, 8).Value = "NÚMEROS DE BOLETOS";
                    worksheet.Cell(1, 9).Value = "CÓDIGO QR";

                    // Datos
                    int row = 2;
                    foreach (var invitado in unidad.Invitados)
                    {
                        worksheet.Cell(row, 1).Value = invitado.ID;
                        worksheet.Cell(row, 2).Value = invitado.Nombre;
                        worksheet.Cell(row, 3).Value = invitado.FechaIngreso;
                        worksheet.Cell(row, 4).Value = invitado.Correo;
                        worksheet.Cell(row, 5).Value = invitado.UnidadNegocio;
                        worksheet.Cell(row, 6).Value = invitado.AniosServicio;
                        worksheet.Cell(row, 7).Value = invitado.Boletos;
                        worksheet.Cell(row, 8).Value = string.Join(", ", invitado.NumerosBoletos);
                        worksheet.Cell(row, 9).Value = invitado.QRCodePath ?? "No generado";
                        row++;
                    }

                    // Autoajustar columnas
                    worksheet.Columns().AdjustToContents();
                }

                workbook.SaveAs(filePath);
            }
        }
    }
}