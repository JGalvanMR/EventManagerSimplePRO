using EventManager;
using EventManager.Models;
using EventManager.Services;
using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;

namespace EventManagerSimple
{
    public partial class Form1 : Form
    {
        private List<UnidadNegocio> unidadesNegocio = new List<UnidadNegocio>();
        private ExcelService excelService = new ExcelService();
        private QRService qrService = new QRService();
        private TicketService ticketService = new TicketService();
        private EventoConfig config = new EventoConfig();

        #region CONTROLES
        // Declaración de controles
        private TabControl tabUnidades;
        private Label lblEstado;
        private Label lblTotalBoletos;
        private ListBox listBoxGanadores;

        // Controles de configuración
        private TextBox txtNombreEvento;
        private DateTimePicker dateFechaEvento;
        private NumericUpDown numBoletos1a5;
        private NumericUpDown numBoletos6a10;
        private NumericUpDown numBoletos11plus;

        // Controles del TabControl principal (declarados en el diseñador)
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private TabPage tabPage4;
        private TabPage tabPage5;

        // Controles de la pestaña Email
        private TabPage tabPageEmail;
        private Button btnGenerarHTML;
        private Button btnEnviarIndividual;
        private Button btnEnviarLote;
        private Button btnProbarConexion;
        private Button btnGuardarConfigEmail;
        private ProgressBar progressBarEmail;
        private Label lblProgressEmail;
        private TextBox txtLogEmail;
        private TextBox txtSmtpServer;
        private TextBox txtSmtpPort;
        private TextBox txtSenderEmail;
        private TextBox txtSenderName;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private CheckBox chkUseSSL;
        private CheckBox chkSoloNoEnviados;
        private CheckBox chkIncluirQR;
        private CheckBox chkAdjuntarPDF;
        private CheckBox chkLogDetallado;

        #endregion

        // Variables para el sistema de email
        private EmailGenerator emailGenerator;
        private EmailConfig emailConfig;
        private CancellationTokenSource cancellationTokenSource;

        // Para el sistema de rifa
        private List<string> todosLosBoletos = new List<string>();
        private Random rnd = new Random();

        public Form1()
        {
            InitializeComponent();
            ConfigurarUI();
            ConfigurarTabEmail();
            // Inicializar el generador de emails
            emailGenerator = new EmailGenerator();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            this.Load += Form1_Load;
        }

        private void ConfigurarUI()
        {
            this.Text = "Gestor de Eventos - Versión Offline";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Configurar controles
            tabControl1.Dock = DockStyle.Fill;

            // Pestaña 1: Carga de Archivo
            tabPage1.Text = "📁 Cargar Archivo";
            ConfigurarTabCarga();

            // Pestaña 2: Visualización
            tabPage2.Text = "👥 Invitados";
            ConfigurarTabVisualizacion();

            // Pestaña 3: Rifa
            tabPage3.Text = "🎰 Sistema de Rifa";
            ConfigurarTabRifa();

            // Pestaña 4: Exportar
            tabPage4.Text = "📤 Exportar";
            ConfigurarTabExportar();

            // Pestaña 5: Configuración
            tabPage5.Text = "⚙️ Configuración";
            ConfigurarTabConfiguracion();
        }

        private void ConfigurarTabCarga()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var labelTitulo = new Label
            {
                Text = "CARGAR LISTA DE INVITADOS",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            var labelInfo = new Label
            {
                Text = "Sube tu archivo Excel con la lista de invitados. El archivo debe tener las siguientes columnas:\n" +
                       "• ID (numérico)\n• NOMBRE (texto)\n• INGRESO (fecha dd/mm/aaaa)\n• CORREO (texto)\n\n" +
                       "Puede contener múltiples hojas, cada una representando una unidad de negocio.",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = false,
                Size = new Size(700, 120),
                Location = new Point(20, 60)
            };

            var btnSeleccionar = new Button
            {
                Text = "📂 Seleccionar Archivo Excel",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                Size = new Size(250, 45),
                Location = new Point(20, 200),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSeleccionar.FlatAppearance.BorderSize = 0;
            btnSeleccionar.Click += BtnSeleccionarArchivo_Click;

            var btnEjemplo = new Button
            {
                Text = "📋 Descargar Plantilla",
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.FromArgb(0, 102, 204),
                Size = new Size(180, 35),
                Location = new Point(280, 210),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnEjemplo.FlatAppearance.BorderColor = Color.FromArgb(0, 102, 204);
            btnEjemplo.Click += BtnDescargarPlantilla_Click;

            lblEstado = new Label
            {
                Text = "Esperando archivo...",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(20, 260)
            };

            panel.Controls.AddRange(new Control[] { labelTitulo, labelInfo, btnSeleccionar, btnEjemplo, lblEstado });
            tabPage1.Controls.Add(panel);
        }

        private void ConfigurarTabVisualizacion()
        {
            var panel = new Panel { Dock = DockStyle.Fill };

            // TabControl para unidades de negocio
            tabUnidades = new TabControl
            {
                Dock = DockStyle.Fill,
                Appearance = TabAppearance.FlatButtons
            };

            panel.Controls.Add(tabUnidades);
            tabPage2.Controls.Add(panel);
        }

        private void ConfigurarTabRifa()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var labelTitulo = new Label
            {
                Text = "SISTEMA DE RIFA",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 0, 0),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            lblTotalBoletos = new Label
            {
                Text = "Total de boletos: 0",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.Green,
                AutoSize = true,
                Location = new Point(20, 60)
            };

            var btnPreparar = new Button
            {
                Text = "🛠️ Preparar Rifa",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.White,
                Size = new Size(180, 45),
                Location = new Point(20, 100),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPreparar.FlatAppearance.BorderSize = 0;
            btnPreparar.Click += BtnPrepararRifa_Click;

            var btnRealizarSorteo = new Button
            {
                Text = "🎯 Realizar Sorteo",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(220, 0, 0),
                ForeColor = Color.White,
                Size = new Size(180, 45),
                Location = new Point(220, 100),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRealizarSorteo.FlatAppearance.BorderSize = 0;
            btnRealizarSorteo.Click += BtnRealizarSorteo_Click;

            // Lista de ganadores
            listBoxGanadores = new ListBox
            {
                Font = new Font("Consolas", 10),
                BackColor = Color.FromArgb(255, 255, 240),
                ForeColor = Color.FromArgb(0, 77, 0),
                Size = new Size(700, 300),
                Location = new Point(20, 160),
                ScrollAlwaysVisible = true
            };

            var btnGuardarResultados = new Button
            {
                Text = "💾 Guardar Resultados",
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                Size = new Size(180, 35),
                Location = new Point(20, 470),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGuardarResultados.Click += BtnGuardarResultados_Click;

            panel.Controls.AddRange(new Control[]
            {
                labelTitulo, lblTotalBoletos, btnPreparar, btnRealizarSorteo,
                listBoxGanadores, btnGuardarResultados
            });
            tabPage3.Controls.Add(panel);
        }

        private void ConfigurarTabExportar()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var labelTitulo = new Label
            {
                Text = "EXPORTAR DATOS",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 0),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Botones de exportación
            var btnExportarExcel = new Button
            {
                Text = "📊 Exportar a Excel",
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(0, 102, 0),
                ForeColor = Color.White,
                Size = new Size(200, 45),
                Location = new Point(20, 80),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Tag = "excel"
            };
            btnExportarExcel.Click += BtnExportar_Click;

            var btnExportarCSV = new Button
            {
                Text = "📄 Exportar Boletos (CSV)",
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(0, 102, 0),
                ForeColor = Color.White,
                Size = new Size(200, 45),
                Location = new Point(240, 80),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Tag = "csv"
            };
            btnExportarCSV.Click += BtnExportar_Click;

            var btnExportarQR = new Button
            {
                Text = "🖼️ Exportar Códigos QR",
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(0, 102, 0),
                ForeColor = Color.White,
                Size = new Size(200, 45),
                Location = new Point(460, 80),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Tag = "qr"
            };
            btnExportarQR.Click += BtnExportar_Click;

            var btnGenerarHTML = new Button
            {
                Text = "🌐 Generar Página Web",
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                Size = new Size(200, 45),
                Location = new Point(20, 140),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Tag = "html"
            };
            btnGenerarHTML.Click += BtnExportar_Click;

            var btnGenerarJSON = new Button
            {
                Text = "📲 Exportar Códigos QR Para App Movil",
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                Size = new Size(300, 45),
                Location = new Point(240, 140),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Tag = "JSON"
            };
            btnGenerarJSON.Click += btnProcesar_Click;

            panel.Controls.AddRange(new Control[]
            {
                labelTitulo, btnExportarExcel, btnExportarCSV, btnExportarQR, btnGenerarHTML, btnGenerarJSON
            });
            tabPage4.Controls.Add(panel);
        }
        private void btnProcesar_Click(object sender, EventArgs e)
        {
            // 1. Seleccionar la Carpeta
            using (var fbd = new FolderBrowserDialog())
            {
                // Abre el diálogo para que el usuario elija la carpeta
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string carpetaEntrada = fbd.SelectedPath;
                    string archivoSalida = Path.Combine(carpetaEntrada, "resultados_qr.csv");

                    // 2. Ejecutar la decodificación
                    DecodificarYGuardarQRs(carpetaEntrada, archivoSalida);
                }
            }
        }

        private void DecodificarYGuardarQRs(string carpetaEntrada, string archivoSalida)
        {
            var log = new StringBuilder();
            var resultadosLista = new List<QrResult>();

            // 1. Declaración del Lector (SOLUCIÓN FINAL para CS1503):
            // Se usa la clase concreta BarcodeReader de compatibilidad de Windows.
            var lector = new ZXing.Windows.Compatibility.BarcodeReader // <-- CORRECCIÓN APLICADA
            {
                Options = new DecodingOptions { PossibleFormats = new[] { BarcodeFormat.QR_CODE } }
            };

            log.AppendLine($"Iniciando búsqueda en: {carpetaEntrada}");

            foreach (string rutaArchivo in Directory.EnumerateFiles(carpetaEntrada, "*.*", SearchOption.TopDirectoryOnly))
            {
                string nombreArchivo = Path.GetFileName(rutaArchivo);
                if (!IsImageFile(nombreArchivo)) continue;

                log.AppendLine($"Procesando: {nombreArchivo}");

                try
                {
                    using (var imagen = (Bitmap)Image.FromFile(rutaArchivo))
                    {
                        // Decodificación: El lector ahora puede usar la sobrecarga Decode(Bitmap)
                        var resultado = lector.Decode(imagen); // <-- Línea 427 ya no debería dar error CS1503

                        if (resultado != null)
                        {
                            string datosQr = resultado.Text;
                            log.AppendLine($"  -> Decodificado: {datosQr}");

                            // Almacenar el resultado en la lista de objetos
                            resultadosLista.Add(new QrResult
                            {
                                FileName = nombreArchivo,
                                DecodedData = datosQr,
                                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            });
                        }
                        else
                        {
                            log.AppendLine("  -> No se encontró código QR.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.AppendLine($"  -> ERROR al procesar {nombreArchivo}: {ex.Message}");
                }
            }

            // 4. Serializar y Guardar en JSON
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(resultadosLista, options);

                // Cambia la extensión del archivo a .json
                string archivoSalidaJson = Path.ChangeExtension(archivoSalida, ".json");

                File.WriteAllText(archivoSalidaJson, jsonString, Encoding.UTF8);

                log.AppendLine($"\n\n✅ Proceso completado. Resultados guardados en JSON: {archivoSalidaJson}");
            }
            catch (Exception ex)
            {
                log.AppendLine($"\n❌ ERROR al serializar o guardar el archivo JSON: {ex.Message}");
            }

            // Muestra el log
            // txtLog.Text = log.ToString();
        }

        private void DecodificarYGuardarQRs9(string carpetaEntrada, string archivoSalida)
        {
            var log = new StringBuilder();
            var resultadosCSV = new StringBuilder();

            resultadosCSV.AppendLine("Nombre_Archivo,Datos_Decodificados_QR");

            // 1. Declaración del Lector (usando 'var' o la clase directa, no la interfaz IBarcodeReader)
            var lector = new ZXing.Windows.Compatibility.BarcodeReader
            {
                Options = new DecodingOptions { PossibleFormats = new[] { BarcodeFormat.QR_CODE } }
            };

            log.AppendLine($"Iniciando búsqueda en: {carpetaEntrada}");

            // 2. Recorrer la carpeta
            foreach (string rutaArchivo in Directory.EnumerateFiles(carpetaEntrada, "*.*", SearchOption.TopDirectoryOnly))
            {
                string nombreArchivo = Path.GetFileName(rutaArchivo);

                if (!IsImageFile(nombreArchivo)) continue;

                log.AppendLine($"Procesando: {nombreArchivo}");

                try
                {
                    using (var imagen = (Bitmap)Image.FromFile(rutaArchivo))
                    {
                        // 3. Decodificar: Usar directamente el Bitmap.
                        // El lector de Windows Compatibility hará la conversión interna (Bitmap -> LuminanceSource -> BinaryBitmap).
                        var resultado = lector.Decode(imagen); // <-- SOLUCIÓN

                        if (resultado != null)
                        {
                            string datosQr = resultado.Text;
                            log.AppendLine($"  -> Decodificado: {datosQr}");
                            resultadosCSV.AppendLine($"\"{nombreArchivo}\",\"{datosQr}\"");
                        }
                        else
                        {
                            log.AppendLine("  -> No se encontró código QR.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.AppendLine($"  -> ERROR al procesar {nombreArchivo}: {ex.Message}");
                }
            }

            // 5. Guardar resultados
            try
            {
                File.WriteAllText(archivoSalida, resultadosCSV.ToString(), Encoding.UTF8);
                log.AppendLine($"\n\n✅ Proceso completado. Resultados guardados en: {archivoSalida}");
            }
            catch (Exception ex)
            {
                log.AppendLine($"\n❌ ERROR al guardar el archivo de salida: {ex.Message}");
            }

            // 6. Mostrar Log (Descomenta esta línea si 'txtLog' es tu control de texto)
            // txtLog.Text = log.ToString(); 
        }

        // Función auxiliar para filtrar por extensiones de imagen
        private bool IsImageFile(string fileName)
        {
            string extension = Path.GetExtension(fileName)?.ToLower();
            return extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".gif" || extension == ".bmp";
        }


        private void ConfigurarTabConfiguracion()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var labelTitulo = new Label
            {
                Text = "CONFIGURACIÓN",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(102, 0, 204),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Configuración del evento
            var labelEvento = new Label
            {
                Text = "Nombre del Evento:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 70),
                AutoSize = true
            };

            txtNombreEvento = new TextBox
            {
                Text = config.NombreEvento,
                Font = new Font("Segoe UI", 10),
                Size = new Size(300, 30),
                Location = new Point(180, 65)
            };

            var labelFecha = new Label
            {
                Text = "Fecha del Evento:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 110),
                AutoSize = true
            };

            dateFechaEvento = new DateTimePicker
            {
                Value = config.FechaEvento,
                Font = new Font("Segoe UI", 10),
                Size = new Size(150, 30),
                Location = new Point(180, 105)
            };

            // Configuración de boletos
            var labelBoletos = new Label
            {
                Text = "CONFIGURACIÓN DE BOLETOS",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                Location = new Point(20, 160),
                AutoSize = true
            };

            var label1a5 = new Label
            {
                Text = "1 a 5 años:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 200),
                AutoSize = true
            };

            numBoletos1a5 = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 10,
                Value = config.Boletos1a5Anios,
                Size = new Size(60, 30),
                Location = new Point(180, 195)
            };

            var label6a10 = new Label
            {
                Text = "6 a 10 años:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 240),
                AutoSize = true
            };

            numBoletos6a10 = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 10,
                Value = config.Boletos6a10Anios,
                Size = new Size(60, 30),
                Location = new Point(180, 235)
            };

            var label11plus = new Label
            {
                Text = "11+ años:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 280),
                AutoSize = true
            };

            numBoletos11plus = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 10,
                Value = config.Boletos11PlusAnios,
                Size = new Size(60, 30),
                Location = new Point(180, 275)
            };

            var btnGuardarConfig = new Button
            {
                Text = "💾 Guardar Configuración",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(102, 0, 204),
                ForeColor = Color.White,
                Size = new Size(200, 45),
                Location = new Point(20, 340),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGuardarConfig.Click += BtnGuardarConfig_Click;

            panel.Controls.AddRange(new Control[]
            {
                labelTitulo, labelEvento, txtNombreEvento, labelFecha, dateFechaEvento,
                labelBoletos, label1a5, numBoletos1a5, label6a10, numBoletos6a10,
                label11plus, numBoletos11plus, btnGuardarConfig
            });
            tabPage5.Controls.Add(panel);
        }

        #region EMAIL
        private void ConfigurarTabEmail()
        {
            // Crear nueva pestaña
            tabPageEmail = new TabPage();
            tabPageEmail.Text = "📧 Enviar Invitaciones";
            tabPageEmail.BackColor = Color.White;
            tabControl1.TabPages.Add(tabPageEmail);

            // Panel principal con scroll
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(0, 0, 15, 0)
            };

            var mainPanel = new Panel
            {
                Dock = DockStyle.Top,
                //Height = 1300, // Alto suficiente para todo el contenido
                Padding = new Padding(20),
                BackColor = Color.White,
                AutoSize = true
            };

            // ============================================
            // TÍTULO PRINCIPAL
            // ============================================
            var labelTitulo = new Label
            {
                Text = "📨 SISTEMA DE ENVÍO DE INVITACIONES",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // ============================================
            // SECCIÓN 1: CONFIGURACIÓN SMTP
            // ============================================
            #region CONFIGURACION SMTP 
            var groupConfig = new GroupBox
            {
                Text = "🔧 Configuración del Servidor de Correo",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(750, 220),
                Location = new Point(20, 70),
                ForeColor = Color.FromArgb(0, 102, 204)
            };

            var panelConfig = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(248, 249, 250)
            };

            // Crear controles de configuración
            int yPos = 15;

            // Fila 1: Servidor y Puerto
            var lblServer = new Label { Text = "Servidor SMTP:", Location = new Point(10, yPos), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtSmtpServer = new TextBox
            {
                Text = "smtp.gmail.com",
                Size = new Size(250, 28),
                Location = new Point(130, yPos - 3),
                Font = new Font("Segoe UI", 9)
            };

            var lblPort = new Label { Text = "Puerto:", Location = new Point(400, yPos), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtSmtpPort = new TextBox
            {
                Text = "587",
                Size = new Size(80, 28),
                Location = new Point(450, yPos - 3),
                Font = new Font("Segoe UI", 9)
            };

            yPos += 35;

            // Fila 2: Email y Nombre del Remitente
            var lblSenderEmail = new Label { Text = "Email Remitente:", Location = new Point(10, yPos), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtSenderEmail = new TextBox
            {
                Text = "eventos@empresa.com",
                Size = new Size(250, 28),
                Location = new Point(130, yPos - 3),
                Font = new Font("Segoe UI", 9)
            };

            var lblSenderName = new Label { Text = "Nombre Remitente:", Location = new Point(400, yPos), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtSenderName = new TextBox
            {
                Text = "Departamento de Eventos",
                Size = new Size(200, 28),
                Location = new Point(520, yPos - 3),
                Font = new Font("Segoe UI", 9)
            };

            yPos += 35;

            // Fila 3: Usuario y Contraseña
            var lblUsername = new Label { Text = "Usuario SMTP:", Location = new Point(10, yPos), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtUsername = new TextBox
            {
                Text = "eventos@empresa.com",
                Size = new Size(250, 28),
                Location = new Point(130, yPos - 3),
                Font = new Font("Segoe UI", 9)
            };

            var lblPassword = new Label { Text = "Contraseña:", Location = new Point(400, yPos), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtPassword = new TextBox
            {
                Text = "",
                Size = new Size(200, 28),
                Location = new Point(480, yPos - 3),
                Font = new Font("Segoe UI", 9),
                UseSystemPasswordChar = true
            };

            yPos += 35;

            // Fila 4: Checkboxes
            chkUseSSL = new CheckBox
            {
                Text = "Usar SSL/TLS",
                Checked = true,
                Location = new Point(10, yPos),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            chkLogDetallado = new CheckBox
            {
                Text = "Log detallado",
                Checked = true,
                Location = new Point(150, yPos),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            // Botón para probar conexión
            btnProbarConexion = new Button
            {
                Text = "🔍 Probar Conexión",
                Size = new Size(140, 32),
                Location = new Point(400, yPos - 5),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnProbarConexion.FlatAppearance.BorderSize = 0;
            btnProbarConexion.Click += BtnProbarConexion_Click;

            // Botón para guardar configuración
            btnGuardarConfigEmail = new Button
            {
                Text = "💾 Guardar Config",
                Size = new Size(140, 32),
                Location = new Point(550, yPos - 5),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(103, 58, 183),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGuardarConfigEmail.FlatAppearance.BorderSize = 0;
            btnGuardarConfigEmail.Click += BtnGuardarConfigEmail_Click;

            // Agregar controles al panel de configuración
            panelConfig.Controls.AddRange(new Control[]
            {
        lblServer, txtSmtpServer, lblPort, txtSmtpPort,
        lblSenderEmail, txtSenderEmail, lblSenderName, txtSenderName,
        lblUsername, txtUsername, lblPassword, txtPassword,
        chkUseSSL, chkLogDetallado, btnProbarConexion, btnGuardarConfigEmail
            });

            groupConfig.Controls.Add(panelConfig);
            #endregion

            // ============================================
            // SECCIÓN 2: OPCIONES DE ENVÍO
            // ============================================
            #region OPCIONES DE ENVIO
            var groupOpciones = new GroupBox
            {
                Text = "⚙️ Opciones de Envío",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(750, 130),
                Location = new Point(20, 300),
                ForeColor = Color.FromArgb(0, 102, 204)
            };

            var panelOpciones = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(248, 249, 250)
            };

            chkSoloNoEnviados = new CheckBox
            {
                Text = "Enviar solo a quienes NO han recibido invitación",
                Checked = true,
                Location = new Point(10, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            chkIncluirQR = new CheckBox
            {
                Text = "Incluir código QR en el email",
                Checked = true,
                Location = new Point(10, 45),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            chkAdjuntarPDF = new CheckBox
            {
                Text = "Adjuntar versión para imprimir (TXT)",
                Checked = true,
                Location = new Point(10, 75),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            panelOpciones.Controls.AddRange(new Control[] { chkSoloNoEnviados, chkIncluirQR, chkAdjuntarPDF });
            groupOpciones.Controls.Add(panelOpciones);
            #endregion

            // ============================================
            // SECCIÓN 3: BOTONES DE ACCIÓN
            // ============================================
            #region BOTONES DE ACCION
            var groupAcciones = new GroupBox
            {
                Text = "🚀 Acciones Disponibles",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(750, 180),
                Location = new Point(20, 440),
                ForeColor = Color.FromArgb(0, 102, 204)
            };

            var panelAcciones = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(248, 249, 250)
            };

            // Botón 1: Generar HTML
            btnGenerarHTML = new Button
            {
                Text = "💾 Generar Archivos HTML (Offline)",
                Size = new Size(280, 50),
                Location = new Point(10, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGenerarHTML.FlatAppearance.BorderSize = 0;
            btnGenerarHTML.Click += BtnGenerarHTML_Click;

            // Botón 2: Enviar Individual
            btnEnviarIndividual = new Button
            {
                Text = "📨 Enviar Invitación Individual",
                Size = new Size(280, 50),
                Location = new Point(310, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnEnviarIndividual.FlatAppearance.BorderSize = 0;
            btnEnviarIndividual.Click += BtnEnviarIndividual_Click;

            // Botón 3: Enviar Lote
            btnEnviarLote = new Button
            {
                Text = "🚀 Enviar Todas las Invitaciones",
                Size = new Size(280, 50),
                Location = new Point(10, 80),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnEnviarLote.FlatAppearance.BorderSize = 0;
            btnEnviarLote.Click += BtnEnviarLote_Click;

            // Botón 4: Cancelar
            var btnCancelar = new Button
            {
                Text = "❌ Cancelar Envío",
                Size = new Size(280, 50),
                Location = new Point(310, 80),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += BtnCancelarEnvio_Click;

            panelAcciones.Controls.AddRange(new Control[]
            {
        btnGenerarHTML, btnEnviarIndividual, btnEnviarLote, btnCancelar
            });
            groupAcciones.Controls.Add(panelAcciones);
            #endregion

            // ============================================
            // SECCIÓN 4: BARRA DE PROGRESO
            // ============================================
            #region BARRA DE PROGRESO
            var groupProgreso = new GroupBox
            {
                Text = "📊 Progreso del Envío",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(750, 120),
                Location = new Point(20, 660),
                ForeColor = Color.FromArgb(0, 102, 204)
            };

            var panelProgreso = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(248, 249, 250)
            };

            progressBarEmail = new ProgressBar
            {
                Size = new Size(600, 30),
                Location = new Point(10, 15),
                Style = ProgressBarStyle.Continuous,
                ForeColor = Color.FromArgb(41, 128, 185)
            };

            lblProgressEmail = new Label
            {
                Text = "Esperando acción...",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(10, 55)
            };

            panelProgreso.Controls.AddRange(new Control[] { progressBarEmail, lblProgressEmail });
            groupProgreso.Controls.Add(panelProgreso);
            #endregion

            // ============================================
            // SECCIÓN 5: LOG DE ACTIVIDADES
            // ============================================
            #region LOG DE ACTIVIDADES
            var groupLog = new GroupBox
            {
                Text = "📝 Log de Actividades",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(750, 350),
                Location = new Point(20, 850),
                ForeColor = Color.FromArgb(0, 102, 204)
            };

            var panelLog = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(248, 249, 250)
            };

            txtLogEmail = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 30, 40),
                ForeColor = Color.FromArgb(200, 220, 255),
                WordWrap = false,
                Location = new Point(700, 40)
            };

            // Botones para el log
            var btnLimpiarLog = new Button
            {
                Text = "🧹 Limpiar Log",
                Size = new Size(100, 28),
                Location = new Point(530, 5),
                Font = new Font("Segoe UI", 8),
                BackColor = Color.FromArgb(108, 122, 137),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLimpiarLog.Click += (s, e) => txtLogEmail.Clear();

            var btnGuardarLog = new Button
            {
                Text = "💾 Guardar Log",
                Size = new Size(100, 28),
                Location = new Point(420, 5),
                Font = new Font("Segoe UI", 8),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGuardarLog.Click += BtnGuardarLog_Click;

            panelLog.Controls.AddRange(new Control[] { txtLogEmail, btnLimpiarLog, btnGuardarLog });
            groupLog.Controls.Add(panelLog);
            #endregion

            // ============================================
            // SECCIÓN 6: CONSEJOS Y AYUDA
            // ============================================
            #region CONSEJOS Y AYUDA
            var groupAyuda = new GroupBox
            {
                Text = "❓ Consejos y Ayuda",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(750, 120),
                Location = new Point(20, 1200),
                ForeColor = Color.FromArgb(0, 102, 204)
            };

            var panelAyuda = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(248, 249, 250)
            };

            var lblAyuda = new Label
            {
                Text = "💡 Para Gmail: Usa 'Contraseña de aplicación' en lugar de tu contraseña normal.\n" +
                       "🔒 El puerto 587 es recomendado. Usa SSL/TLS.\n" +
                       "⏱️  El envío masivo incluye pausas automáticas para evitar bloqueos.\n" +
                       "📁 Los archivos HTML se guardan con todos los datos incluyendo QR codes.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = true,
                Location = new Point(10, 10)
            };

            panelAyuda.Controls.Add(lblAyuda);
            groupAyuda.Controls.Add(panelAyuda);
            #endregion

            // ============================================
            // ENSAMBLAR TODO
            // ============================================
            mainPanel.Controls.AddRange(new Control[]
            {
        labelTitulo,
        groupConfig,
        groupOpciones,
        groupAcciones,
        groupProgreso,
        groupLog,
        groupAyuda
            });

            scrollPanel.Controls.Add(mainPanel);
            tabPageEmail.Controls.Add(scrollPanel);

            // Cargar configuración guardada
            CargarConfiguracionEmail();
        }

        // ============================================
        // EVENTOS PRINCIPALES
        // ============================================

        private void BtnProbarConexion_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                lblProgressEmail.Text = "Probando conexión SMTP...";

                var config = ObtenerConfiguracionEmailDelFormulario();
                if (!config.IsValid())
                {
                    MessageBox.Show("Por favor completa todos los campos de configuración SMTP.",
                                  "Configuración Incompleta",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var generator = new EmailGenerator();

                // Ejecutar prueba en segundo plano para no bloquear la UI
                Task.Run(async () =>
                {
                    try
                    {
                        bool conexionExitosa = false;
                        try
                        {
                            using (var client = new System.Net.Mail.SmtpClient(config.SmtpServer, config.SmtpPort))
                            {
                                client.EnableSsl = config.UseSsl;
                                client.Credentials = new System.Net.NetworkCredential(config.Username, config.Password);
                                // El método Send no debe usarse para enviar un correo real, solo para probar la conexión.
                                // Se puede usar client.SendMailAsync con un mensaje de prueba.
                                var mail = new System.Net.Mail.MailMessage(config.SenderEmail, config.SenderEmail)
                                {
                                    Subject = "Prueba de conexión SMTP",
                                    Body = "Este es un correo de prueba para verificar la configuración SMTP."
                                };
                                await client.SendMailAsync(mail);
                                conexionExitosa = true;
                            }
                        }
                        catch
                        {
                            conexionExitosa = false;
                        }

                        this.Invoke((MethodInvoker)delegate
                        {
                            if (conexionExitosa)
                            {
                                MessageBox.Show("✅ Conexión SMTP exitosa!\n\n" +
                                              $"Servidor: {config.SmtpServer}:{config.SmtpPort}\n" +
                                              $"Remitente: {config.SenderName} <{config.SenderEmail}>",
                                              "Conexión Exitosa",
                                              MessageBoxButtons.OK, MessageBoxIcon.Information);

                                AgregarLog("✅ Conexión SMTP probada exitosamente");
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            MessageBox.Show($"❌ Error de conexión:\n\n{ex.Message}\n\n" +
                                          "Verifica:\n" +
                                          "1. Usuario y contraseña\n" +
                                          "2. Puerto y servidor SMTP\n" +
                                          "3. Conexión a internet\n" +
                                          "4. SSL/TLS habilitado",
                                          "Error de Conexión",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);

                            AgregarLog($"❌ Error probando conexión: {ex.Message}");
                        });
                    }
                    finally
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            Cursor = Cursors.Default;
                            lblProgressEmail.Text = "Listo";
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                Cursor = Cursors.Default;
            }
        }

        private void BtnGuardarConfigEmail_Click(object sender, EventArgs e)
        {
            try
            {
                var config = ObtenerConfiguracionEmailDelFormulario();

                // Validar
                if (string.IsNullOrWhiteSpace(config.SmtpServer))
                {
                    MessageBox.Show("El servidor SMTP es requerido.",
                                  "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(config.SenderEmail))
                {
                    MessageBox.Show("El email del remitente es requerido.",
                                  "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Guardar en archivo JSON
                string configPath = Path.Combine(Application.StartupPath, "email_config.json");
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(configPath, json);

                MessageBox.Show("✅ Configuración de email guardada exitosamente.",
                              "Configuración Guardada",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);

                AgregarLog("✅ Configuración de email guardada en email_config.json");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error guardando configuración: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGenerarHTML_Click(object sender, EventArgs e)
        {
            if (!unidadesNegocio.Any() || unidadesNegocio.Sum(u => u.Invitados.Count) == 0)
            {
                MessageBox.Show("Primero carga un archivo Excel con la lista de invitados.",
                              "Datos Requeridos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Seleccionar carpeta para guardar las invitaciones HTML";
                folderDialog.ShowNewFolderButton = true;
                folderDialog.RootFolder = Environment.SpecialFolder.Desktop;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Cursor = Cursors.WaitCursor;
                        btnGenerarHTML.Enabled = false;
                        progressBarEmail.Value = 0;
                        lblProgressEmail.Text = "Generando archivos HTML...";
                        txtLogEmail.Clear();

                        // Obtener todos los invitados
                        var allInvitados = unidadesNegocio.SelectMany(u => u.Invitados).ToList();
                        int total = allInvitados.Count;

                        AgregarLog($"📁 Iniciando generación de {total} invitaciones HTML...");

                        var generator = new EmailGenerator();
                        int completados = 0;

                        // Crear subcarpetas por unidad de negocio
                        foreach (var unidad in unidadesNegocio)
                        {
                            string unidadFolder = Path.Combine(folderDialog.SelectedPath,
                                                             SanitizeFolderName(unidad.Nombre));
                            Directory.CreateDirectory(unidadFolder);

                            foreach (var invitado in unidad.Invitados)
                            {
                                try
                                {
                                    // Generar y guardar HTML
                                    generator.GuardarInvitacionComoArchivo(invitado, config, unidadFolder);

                                    completados++;
                                    progressBarEmail.Value = (int)((completados * 100.0) / total);
                                    lblProgressEmail.Text = $"Generando... {completados}/{total}";

                                    // Actualizar log cada 10 invitados
                                    if (completados % 10 == 0)
                                    {
                                        AgregarLog($"   ✅ {completados}/{total} completados...");
                                    }

                                    Application.DoEvents(); // Permitir que la UI se actualice
                                }
                                catch (Exception ex)
                                {
                                    AgregarLog($"   ❌ Error con {invitado.Nombre}: {ex.Message}");
                                }
                            }
                        }

                        // También crear un índice HTML
                        CrearIndiceHTML(folderDialog.SelectedPath, unidadesNegocio, config);

                        AgregarLog($"\n🎉 GENERACIÓN COMPLETADA!");
                        AgregarLog($"📂 Carpeta: {folderDialog.SelectedPath}");
                        AgregarLog($"📄 Archivos generados: {completados} de {total}");

                        MessageBox.Show($"✅ Se generaron {completados} invitaciones HTML en:\n\n" +
                                      folderDialog.SelectedPath +
                                      "\n\nSe ha creado un archivo 'index.html' con la lista completa.",
                                      "Generación Completada",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Abrir la carpeta
                        System.Diagnostics.Process.Start("explorer.exe", folderDialog.SelectedPath);
                    }
                    catch (Exception ex)
                    {
                        AgregarLog($"❌ ERROR CRÍTICO: {ex.Message}");
                        MessageBox.Show($"Error generando archivos: {ex.Message}",
                                      "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                        btnGenerarHTML.Enabled = true;
                        lblProgressEmail.Text = "Completado";
                    }
                }
            }
        }

        private void BtnEnviarIndividual_Click(object sender, EventArgs e)
        {
            if (!unidadesNegocio.Any() || unidadesNegocio.Sum(u => u.Invitados.Count) == 0)
            {
                MessageBox.Show("Primero carga un archivo Excel con la lista de invitados.",
                              "Datos Requeridos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Verificar configuración SMTP
            var configSMTP = ObtenerConfiguracionEmailDelFormulario();
            if (!configSMTP.IsValid())
            {
                MessageBox.Show("Primero configura los datos del servidor SMTP.",
                              "Configuración Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Crear formulario de selección
            using (var formSeleccion = new Form())
            {
                formSeleccion.Text = "📧 Seleccionar Invitado para Enviar";
                formSeleccion.Size = new Size(500, 600);
                formSeleccion.StartPosition = FormStartPosition.CenterParent;
                formSeleccion.FormBorderStyle = FormBorderStyle.FixedDialog;
                formSeleccion.MaximizeBox = false;
                formSeleccion.BackColor = Color.White;

                // ListBox para seleccionar invitado
                var listBox = new ListBox
                {
                    Dock = DockStyle.Fill,
                    DisplayMember = "DisplayText",
                    Font = new Font("Segoe UI", 10),
                    IntegralHeight = false,
                    Margin = new Padding(10)
                };

                // Cargar todos los invitados
                var allInvitados = unidadesNegocio
                    .SelectMany(u => u.Invitados)
                    .OrderBy(i => i.UnidadNegocio)
                    .ThenBy(i => i.Nombre)
                    .ToList();

                // Filtrar si es necesario
                if (chkSoloNoEnviados.Checked)
                {
                    allInvitados = allInvitados.Where(i => !i.InvitacionEnviada).ToList();
                }

                if (!allInvitados.Any())
                {
                    MessageBox.Show(chkSoloNoEnviados.Checked
                        ? "Todos los invitados ya han recibido su invitación."
                        : "No hay invitados cargados.",
                        "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Configurar texto de display
                foreach (var invitado in allInvitados)
                {
                    string estado = invitado.InvitacionEnviada ? "✓" : "○";
                    listBox.Items.Add(new
                    {
                        Invitado = invitado,
                        DisplayText = $"{estado} {invitado.Nombre} | {invitado.Correo} | {invitado.UnidadNegocio}"
                    });
                }

                // Botones
                var panelBotones = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 50,
                    Padding = new Padding(10)
                };

                var btnSeleccionar = new Button
                {
                    Text = "📨 Enviar Invitación",
                    DialogResult = DialogResult.OK,
                    Size = new Size(120, 35),
                    Location = new Point(10, 5),
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat
                };

                var btnCancelar = new Button
                {
                    Text = "Cancelar",
                    DialogResult = DialogResult.Cancel,
                    Size = new Size(120, 35),
                    Location = new Point(140, 5),
                    FlatStyle = FlatStyle.Flat
                };

                panelBotones.Controls.AddRange(new Control[] { btnSeleccionar, btnCancelar });

                // Panel de búsqueda
                var panelBusqueda = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 50,
                    Padding = new Padding(10)
                };

                var txtBusqueda = new TextBox
                {
                    PlaceholderText = "Buscar por nombre, email o unidad...",
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 10)
                };

                txtBusqueda.TextChanged += (s, args) =>
                {
                    string texto = txtBusqueda.Text.ToLower();
                    listBox.Items.Clear();

                    var filtrados = allInvitados.Where(i =>
                        i.Nombre.ToLower().Contains(texto) ||
                        i.Correo.ToLower().Contains(texto) ||
                        i.UnidadNegocio.ToLower().Contains(texto)).ToList();

                    foreach (var invitado in filtrados)
                    {
                        string estado = invitado.InvitacionEnviada ? "✓" : "○";
                        listBox.Items.Add(new
                        {
                            Invitado = invitado,
                            DisplayText = $"{estado} {invitado.Nombre} | {invitado.Correo} | {invitado.UnidadNegocio}"
                        });
                    }
                };

                panelBusqueda.Controls.Add(txtBusqueda);

                // Contenedor principal
                var panelPrincipal = new Panel { Dock = DockStyle.Fill };
                panelPrincipal.Controls.Add(listBox);

                formSeleccion.Controls.AddRange(new Control[] { panelBusqueda, panelPrincipal, panelBotones });

                if (formSeleccion.ShowDialog() == DialogResult.OK && listBox.SelectedItem != null)
                {
                    dynamic selectedItem = listBox.SelectedItem;
                    var invitadoSeleccionado = selectedItem.Invitado as Invitado;

                    // Confirmar envío
                    var confirmacion = MessageBox.Show(
                        $"¿Enviar invitación a:\n\n" +
                        $"👤 {invitadoSeleccionado.Nombre}\n" +
                        $"📧 {invitadoSeleccionado.Correo}\n" +
                        $"🏢 {invitadoSeleccionado.UnidadNegocio}\n\n" +
                        $"Esta acción enviará un email real.",
                        "Confirmar Envío",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (confirmacion == DialogResult.Yes)
                    {
                        // Ejecutar envío en segundo plano
                        Task.Run(() => EnviarEmailIndividual(invitadoSeleccionado, configSMTP));
                    }
                }
            }
        }

        private async Task EnviarEmailIndividual(Invitado invitado, EmailConfig configSMTP)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    Cursor = Cursors.WaitCursor;
                    lblProgressEmail.Text = $"Enviando a {invitado.Nombre}...";
                    progressBarEmail.Style = ProgressBarStyle.Marquee;
                    btnEnviarIndividual.Enabled = false;
                    btnEnviarLote.Enabled = false;
                });

                AgregarLog($"📨 Iniciando envío individual a: {invitado.Nombre} <{invitado.Correo}>");

                var generator = new EmailGenerator();
                //await generator.EnviarInvitacionPorCorreo(invitado, config, configSMTP);
                string html = generator.GenerarInvitacionHTML(invitado, config);
                // Aquí deberías implementar el envío de correo usando SmtpClient directamente,
                // ya que EmailGenerator no tiene el método EnviarInvitacionPorCorreo.
                // Ejemplo básico:
                using (var client = new System.Net.Mail.SmtpClient(configSMTP.SmtpServer, configSMTP.SmtpPort))
                {
                    client.EnableSsl = configSMTP.UseSsl;
                    client.Credentials = new System.Net.NetworkCredential(configSMTP.Username, configSMTP.Password);
                    var mail = new System.Net.Mail.MailMessage(configSMTP.SenderEmail, invitado.Correo)
                    {
                        Subject = $"Invitación: {config.NombreEvento}",
                        Body = html,
                        IsBodyHtml = true
                    };
                    await client.SendMailAsync(mail);
                }


                // Marcar como enviado
                invitado.InvitacionEnviada = true;
                invitado.InvitacionEnviadaFecha = DateTime.Now;

                this.Invoke((MethodInvoker)delegate
                {
                    AgregarLog($"✅ INVITACIÓN ENVIADA: {invitado.Nombre} <{invitado.Correo}>");

                    MessageBox.Show($"✅ Invitación enviada exitosamente a:\n\n" +
                                  $"{invitado.Nombre}\n" +
                                  $"{invitado.Correo}",
                                  "Envío Exitoso",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    AgregarLog($"❌ ERROR enviando a {invitado.Correo}: {ex.Message}");

                    MessageBox.Show($"❌ Error enviando invitación:\n\n{ex.Message}",
                                  "Error de Envío",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
            finally
            {
                this.Invoke((MethodInvoker)delegate
                {
                    Cursor = Cursors.Default;
                    lblProgressEmail.Text = "Listo";
                    progressBarEmail.Style = ProgressBarStyle.Continuous;
                    progressBarEmail.Value = 0;
                    btnEnviarIndividual.Enabled = true;
                    btnEnviarLote.Enabled = true;
                });
            }
        }

        private void BtnEnviarLote_Click(object sender, EventArgs e)
        {
            if (!unidadesNegocio.Any() || unidadesNegocio.Sum(u => u.Invitados.Count) == 0)
            {
                MessageBox.Show("Primero carga un archivo Excel con la lista de invitados.",
                              "Datos Requeridos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Verificar configuración SMTP
            var configSMTP = ObtenerConfiguracionEmailDelFormulario();
            if (!configSMTP.IsValid())
            {
                MessageBox.Show("Primero configura los datos del servidor SMTP.",
                              "Configuración Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Obtener todos los invitados
            var allInvitados = unidadesNegocio.SelectMany(u => u.Invitados).ToList();

            // Filtrar según opciones
            if (chkSoloNoEnviados.Checked)
            {
                allInvitados = allInvitados.Where(i => !i.InvitacionEnviada).ToList();
            }

            if (!allInvitados.Any())
            {
                MessageBox.Show(chkSoloNoEnviados.Checked
                    ? "Todos los invitados ya han recibido su invitación."
                    : "No hay invitados cargados.",
                    "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Mostrar diálogo de confirmación
            using (var formConfirmacion = new Form())
            {
                formConfirmacion.Text = "🚀 Confirmar Envío Masivo";
                formConfirmacion.Size = new Size(500, 400);
                formConfirmacion.StartPosition = FormStartPosition.CenterParent;
                formConfirmacion.FormBorderStyle = FormBorderStyle.FixedDialog;

                var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

                var lblTitulo = new Label
                {
                    Text = "⚠️ ENVÍO MASIVO DE INVITACIONES",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    ForeColor = Color.FromArgb(231, 76, 60),
                    AutoSize = true,
                    Location = new Point(10, 10)
                };

                var lblDetalles = new Label
                {
                    Text = $"Se enviarán invitaciones a {allInvitados.Count} invitados:\n\n" +
                           $"• Servidor SMTP: {configSMTP.SmtpServer}:{configSMTP.SmtpPort}\n" +
                           $"• Remitente: {configSMTP.SenderName} <{configSMTP.SenderEmail}>\n" +
                           $"• Incluir QR: {(chkIncluirQR.Checked ? "Sí" : "No")}\n" +
                           $"• Solo no enviados: {(chkSoloNoEnviados.Checked ? "Sí" : "No")}\n\n" +
                           $"⏱️  Tiempo estimado: {TimeSpan.FromSeconds(allInvitados.Count * 2):mm\\:ss} minutos\n" +
                           $"📧 Se enviarán emails REALES.",
                    Font = new Font("Segoe UI", 10),
                    AutoSize = false,
                    Size = new Size(440, 200),
                    Location = new Point(10, 50)
                };

                var chkConfirmar = new CheckBox
                {
                    Text = "✅ Confirmo que tengo permiso para enviar estos emails",
                    Location = new Point(10, 260),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };

                // Botones
                var btnIniciar = new Button
                {
                    Text = "🚀 INICIAR ENVÍO MASIVO",
                    Size = new Size(200, 40),
                    Location = new Point(50, 300),
                    BackColor = Color.FromArgb(231, 76, 60),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    Enabled = false
                };

                var btnCancelar = new Button
                {
                    Text = "Cancelar",
                    Size = new Size(200, 40),
                    Location = new Point(260, 300),
                    DialogResult = DialogResult.Cancel,
                    FlatStyle = FlatStyle.Flat
                };

                chkConfirmar.CheckedChanged += (s, ev) => btnIniciar.Enabled = chkConfirmar.Checked;

                btnIniciar.Click += (s, ev) =>
                {
                    formConfirmacion.DialogResult = DialogResult.OK;
                    formConfirmacion.Close();
                };

                panel.Controls.AddRange(new Control[]
                {
            lblTitulo, lblDetalles, chkConfirmar, btnIniciar, btnCancelar
                });

                formConfirmacion.Controls.Add(panel);

                if (formConfirmacion.ShowDialog() == DialogResult.OK)
                {
                    // Iniciar envío masivo en segundo plano
                    cancellationTokenSource = new CancellationTokenSource();
                    Task.Run(() => EnviarEmailsEnLote(allInvitados, configSMTP, cancellationTokenSource.Token));
                }
            }
        }

        private async Task<EmailBatchResult> EnviarInvitacionesLote(
    List<Invitado> invitados,
    EventoConfig configEvento,
    EmailConfig configSMTP,
    IProgress<EmailProgress> progress,
    int delayMs,
    int batchSize)
        {
            var result = new EmailBatchResult();
            int total = invitados.Count;
            int current = 0;
            var generator = new EmailGenerator();

            foreach (var invitado in invitados)
            {
                if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
                    throw new OperationCanceledException();

                try
                {
                    string html = generator.GenerarInvitacionHTML(invitado, configEvento);
                    using (var client = new System.Net.Mail.SmtpClient(configSMTP.SmtpServer, configSMTP.SmtpPort))
                    {
                        client.EnableSsl = configSMTP.UseSsl;
                        client.Credentials = new System.Net.NetworkCredential(configSMTP.Username, configSMTP.Password);
                        var mail = new System.Net.Mail.MailMessage(configSMTP.SenderEmail, invitado.Correo)
                        {
                            Subject = $"Invitación: {configEvento.NombreEvento}",
                            Body = html,
                            IsBodyHtml = true
                        };
                        await client.SendMailAsync(mail);
                    }
                    invitado.InvitacionEnviada = true;
                    invitado.InvitacionEnviadaFecha = DateTime.Now;
                    result.EnviadosExitosos++;
                    progress?.Report(new EmailProgress
                    {
                        Current = ++current,
                        Total = total,
                        CurrentInvited = invitado.Nombre,
                        Status = $"Enviado a {invitado.Nombre}",
                        Success = true
                    });
                }
                catch (Exception ex)
                {
                    result.EnviadosFallidos++;
                    result.Errores.Add(new EmailError { Invitado = invitado, ErrorMessage = ex.Message });
                    progress?.Report(new EmailProgress
                    {
                        Current = ++current,
                        Total = total,
                        CurrentInvited = invitado.Nombre,
                        Status = $"Error con {invitado.Nombre}: {ex.Message}",
                        Success = false
                    });
                }
                await Task.Delay(delayMs);
            }
            result.InvitadosValidos = total;
            result.Duracion = TimeSpan.FromSeconds(total * delayMs / 1000);
            return result;
        }

        private async Task EnviarEmailsEnLote(List<Invitado> invitados, EmailConfig configSMTP, CancellationToken cancellationToken)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    Cursor = Cursors.WaitCursor;
                    lblProgressEmail.Text = "Preparando envío masivo...";
                    progressBarEmail.Value = 0;
                    txtLogEmail.Clear();

                    btnEnviarIndividual.Enabled = false;
                    btnEnviarLote.Enabled = false;
                    btnGenerarHTML.Enabled = false;
                });

                AgregarLog($"🚀 INICIANDO ENVÍO MASIVO");
                AgregarLog($"📧 Total de invitados: {invitados.Count}");
                AgregarLog($"⚙️  Configuración SMTP: {configSMTP.SmtpServer}:{configSMTP.SmtpPort}");
                AgregarLog($"");

                var generator = new EmailGenerator();

                // Configurar el progreso
                var progress = new Progress<EmailProgress>(p =>
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        progressBarEmail.Value = p.Porcentaje;
                        lblProgressEmail.Text = $"{p.Current}/{p.Total} - {p.CurrentInvited}";

                        if (!string.IsNullOrEmpty(p.Status))
                        {
                            string icon = p.Success ? "✅" : "❌";
                            AgregarLog($"{icon} {p.Status}");

                            // Auto-scroll
                            txtLogEmail.SelectionStart = txtLogEmail.Text.Length;
                            txtLogEmail.ScrollToCaret();
                        }
                    });
                });

                // Ejecutar envío en lote
                //var batchResult = await generator.EnviarInvitacionesLote(invitados, config, configSMTP, progress, 1500, 15);
                var batchResult = await EnviarInvitacionesLote(invitados, config, configSMTP, progress, 1500, 15);

                // Mostrar resumen
                this.Invoke((MethodInvoker)delegate
                {
                    AgregarLog("");
                    AgregarLog("=".PadRight(60, '='));
                    AgregarLog("📊 RESUMEN DEL ENVÍO MASIVO");
                    AgregarLog("=".PadRight(60, '='));
                    AgregarLog($"✅ Enviados exitosamente: {batchResult.EnviadosExitosos}");
                    AgregarLog($"❌ Enviados fallidos: {batchResult.EnviadosFallidos}");
                    AgregarLog($"⏱️  Duración: {batchResult.Duracion:mm\\:ss}");
                    AgregarLog($"📈 Tasa de éxito: {(batchResult.InvitadosValidos > 0 ?
                        (batchResult.EnviadosExitosos * 100.0 / batchResult.InvitadosValidos).ToString("F1") : "0")}%");

                    if (batchResult.Errores.Any())
                    {
                        AgregarLog("");
                        AgregarLog("❌ ERRORES DETECTADOS:");
                        foreach (var error in batchResult.Errores.Take(5))
                        {
                            AgregarLog($"   • {error.Invitado.Nombre}: {error.ErrorMessage}");
                        }
                        if (batchResult.Errores.Count > 5)
                        {
                            AgregarLog($"   ... y {batchResult.Errores.Count - 5} errores más.");
                        }
                    }

                    AgregarLog("=".PadRight(60, '='));

                    // Generar y guardar reporte completo
                    //string reporte = generator.GenerarReporteEnvio(batchResult);
                    string reporte = batchResult.ToString();
                    string reportePath = Path.Combine(Application.StartupPath,
                        "Reportes",
                        $"Reporte_Envio_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

                    Directory.CreateDirectory(Path.GetDirectoryName(reportePath));
                    File.WriteAllText(reportePath, reporte, Encoding.UTF8);

                    AgregarLog($"📄 Reporte guardado en: {reportePath}");

                    // Mostrar mensaje final
                    MessageBox.Show(
                        $"✅ Envío masivo completado!\n\n" +
                        $"📊 Estadísticas:\n" +
                        $"• Total procesados: {batchResult.InvitadosValidos}\n" +
                        $"• Enviados exitosamente: {batchResult.EnviadosExitosos}\n" +
                        $"• Enviados fallidos: {batchResult.EnviadosFallidos}\n" +
                        $"• Tiempo total: {batchResult.Duracion:mm\\:ss}\n\n" +
                        $"Se ha generado un reporte detallado.",
                        "Envío Masivo Completado",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
            }
            catch (OperationCanceledException)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    AgregarLog("⏹️  ENVÍO CANCELADO POR EL USUARIO");
                    MessageBox.Show("El envío masivo ha sido cancelado.",
                                  "Envío Cancelado",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    AgregarLog($"💥 ERROR CRÍTICO: {ex.Message}");
                    MessageBox.Show($"Error crítico durante el envío masivo:\n\n{ex.Message}",
                                  "Error Crítico",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
            finally
            {
                this.Invoke((MethodInvoker)delegate
                {
                    Cursor = Cursors.Default;
                    lblProgressEmail.Text = "Completado";
                    progressBarEmail.Value = 100;

                    btnEnviarIndividual.Enabled = true;
                    btnEnviarLote.Enabled = true;
                    btnGenerarHTML.Enabled = true;

                    cancellationTokenSource?.Dispose();
                    cancellationTokenSource = null;
                });
            }
        }

        private void BtnCancelarEnvio_Click(object sender, EventArgs e)
        {
            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                var result = MessageBox.Show(
                    "¿Estás seguro de cancelar el envío masivo?\n\n" +
                    "Los emails que ya se enviaron no se pueden deshacer.",
                    "Confirmar Cancelación",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    cancellationTokenSource.Cancel();
                    AgregarLog("⏹️  Cancelación solicitada por el usuario...");
                }
            }
            else
            {
                MessageBox.Show("No hay envío en progreso para cancelar.",
                              "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnGuardarLog_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Archivo de texto|*.txt|Archivo log|*.log";
                saveDialog.FileName = $"Email_Log_{DateTime.Now:yyyyMMdd_HHmmss}";
                saveDialog.DefaultExt = ".txt";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveDialog.FileName, txtLogEmail.Text, Encoding.UTF8);

                    MessageBox.Show($"Log guardado en:\n{saveDialog.FileName}",
                                  "Log Guardado",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // ============================================
        // MÉTODOS AUXILIARES
        // ============================================

        private EmailConfig ObtenerConfiguracionEmailDelFormulario()
        {
            return new EmailConfig
            {
                SmtpServer = txtSmtpServer?.Text?.Trim() ?? "smtp.gmail.com",
                SmtpPort = int.TryParse(txtSmtpPort?.Text, out int port) ? port : 587,
                UseSsl = chkUseSSL?.Checked ?? true,
                SenderEmail = txtSenderEmail?.Text?.Trim() ?? "",
                SenderName = txtSenderName?.Text?.Trim() ?? "Departamento de Eventos",
                Username = txtUsername?.Text?.Trim() ?? "",
                Password = txtPassword?.Text?.Trim() ?? ""
            };
        }

        private void CargarConfiguracionEmail()
        {
            try
            {
                string configFile = Path.Combine(Application.StartupPath, "email_config.json");
                if (File.Exists(configFile))
                {
                    string json = File.ReadAllText(configFile);
                    var config = Newtonsoft.Json.JsonConvert.DeserializeObject<EmailConfig>(json);

                    if (config != null)
                    {
                        // Asignar a controles si existen
                        if (txtSmtpServer != null) txtSmtpServer.Text = config.SmtpServer;
                        if (txtSmtpPort != null) txtSmtpPort.Text = config.SmtpPort.ToString();
                        if (txtSenderEmail != null) txtSenderEmail.Text = config.SenderEmail;
                        if (txtSenderName != null) txtSenderName.Text = config.SenderName;
                        if (txtUsername != null) txtUsername.Text = config.Username;
                        if (txtPassword != null) txtPassword.Text = config.Password;
                        if (chkUseSSL != null) chkUseSSL.Checked = config.UseSsl;

                        //AgregarLog("✅ Configuración de email cargada desde archivo");
                    }
                }
            }
            catch (Exception ex)
            {
                AgregarLog($"⚠️ Error cargando configuración: {ex.Message}");
            }
        }

        private void AgregarLog(string mensaje)
        {
            if (txtLogEmail != null)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    string timestamp = DateTime.Now.ToString("HH:mm:ss");
                    txtLogEmail.AppendText($"[{timestamp}] {mensaje}\r\n");

                    // Auto-scroll
                    txtLogEmail.SelectionStart = txtLogEmail.Text.Length;
                    txtLogEmail.ScrollToCaret();
                });
            }
        }

        private string SanitizeFolderName(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
                return "Sin_Nombre";

            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("_", folderName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries))
                .Replace(" ", "_")
                .Trim();
        }

        private void CrearIndiceHTML(string carpetaBase, List<UnidadNegocio> unidades, EventoConfig configEvento)
        {
            try
            {
                string html = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <title>Índice de Invitaciones - {configEvento.NombreEvento}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; background: #f5f5f5; }}
        .container {{ max-width: 1200px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 5px 15px rgba(0,0,0,0.1); }}
        h1 {{ color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }}
        .unidad {{ background: #ecf0f1; margin: 20px 0; padding: 15px; border-radius: 5px; }}
        .invitado {{ padding: 10px; border-bottom: 1px solid #ddd; }}
        .invitado:hover {{ background: #f8f9fa; }}
        .stats {{ background: #2ecc71; color: white; padding: 15px; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>📋 Índice de Invitaciones - {configEvento.NombreEvento}</h1>
        <p><strong>Fecha de generación:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
        
        <div class='stats'>
            <strong>ESTADÍSTICAS:</strong><br>
            Total de unidades: {unidades.Count}<br>
            Total de invitados: {unidades.Sum(u => u.Invitados.Count)}<br>
            Total de boletos: {unidades.Sum(u => u.Invitados.Sum(i => i.Boletos))}
        </div>
";

                foreach (var unidad in unidades)
                {
                    html += $@"
        <div class='unidad'>
            <h2>🏢 {unidad.Nombre} ({unidad.Invitados.Count} invitados)</h2>";

                    foreach (var invitado in unidad.Invitados)
                    {
                        string fileName = $"Invitacion_{SanitizeFolderName(unidad.Nombre)}_{invitado.ID}_{SanitizeFolderName(invitado.Nombre)}.html";
                        string filePath = Path.Combine(SanitizeFolderName(unidad.Nombre), fileName);

                        html += $@"
            <div class='invitado'>
                <strong>{invitado.Nombre}</strong> (ID: {invitado.ID})<br>
                Email: {invitado.Correo}<br>
                Años: {invitado.AniosServicio} | Boletos: {invitado.Boletos}<br>
                <a href='{filePath}' target='_blank'>📄 Abrir invitación</a> | 
                <a href='{filePath}' download>💾 Descargar</a>
            </div>";
                    }

                    html += @"
        </div>";
                }

                html += $@"
        <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; color: #7f8c8d; font-size: 12px;'>
            <p>Generado automáticamente por EventManagerSimple</p>
            <p>Evento: {configEvento.NombreEvento} | Fecha: {configEvento.FechaEvento:dd/MM/yyyy}</p>
        </div>
    </div>
</body>
</html>";

                string indexPath = Path.Combine(carpetaBase, "index.html");
                File.WriteAllText(indexPath, html, Encoding.UTF8);

                AgregarLog($"📄 Índice HTML creado: {indexPath}");
            }
            catch (Exception ex)
            {
                AgregarLog($"⚠️ Error creando índice: {ex.Message}");
            }
        }

        // Método para actualizar la UI cuando se cargan datos
        private void ActualizarContadoresEmail()
        {
            if (unidadesNegocio.Any())
            {
                int totalInvitados = unidadesNegocio.Sum(u => u.Invitados.Count);
                int enviados = unidadesNegocio.Sum(u => u.Invitados.Count(i => i.InvitacionEnviada));
                int pendientes = totalInvitados - enviados;

                // Actualizar tooltip o mostrar en algún label si lo tienes
                if (tabPageEmail != null)
                {
                    tabPageEmail.Text = $"📧 Email ({enviados}/{totalInvitados})";
                }

                AgregarLog($"📊 Actualización: {enviados} enviados, {pendientes} pendientes");
            }
        }

        #endregion

        // Event Handlers
        private void BtnSeleccionarArchivo_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Archivos Excel (*.xlsx;*.xls;*.csv)|*.xlsx;*.xls;*.csv|Todos los archivos (*.*)|*.*";
                openFileDialog.Title = "Seleccionar archivo de invitados";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Cursor = Cursors.WaitCursor;
                        lblEstado.Text = "Procesando archivo...";
                        lblEstado.ForeColor = Color.Blue;

                        // Procesar archivo
                        unidadesNegocio = excelService.ProcesarArchivo(openFileDialog.FileName);

                        if (unidadesNegocio.Any())
                        {
                            // Generar QR y boletos
                            GenerarQRyBoletos();

                            // Actualizar UI
                            ActualizarTabVisualizacion();
                            ActualizarEstadisticas();

                            lblEstado.Text = $"✅ Archivo procesado: {unidadesNegocio.Sum(u => u.TotalInvitados)} invitados cargados";
                            lblEstado.ForeColor = Color.Green;

                            // Cambiar a pestaña de visualización
                            tabControl1.SelectedTab = tabPage2;
                        }
                        else
                        {
                            lblEstado.Text = "❌ No se encontraron datos válidos en el archivo";
                            lblEstado.ForeColor = Color.Red;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error procesando archivo:\n{ex.Message}",
                                      "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        lblEstado.Text = "❌ Error procesando archivo";
                        lblEstado.ForeColor = Color.Red;
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                }
            }
        }

        private void GenerarQRyBoletos()
        {
            string qrFolder = Path.Combine(Application.StartupPath, "QR_Codes");
            Directory.CreateDirectory(qrFolder);

            foreach (var unidad in unidadesNegocio)
            {
                foreach (var invitado in unidad.Invitados)
                {
                    // Generar números de boletos
                    invitado.NumerosBoletos = ticketService.GenerarNumerosBoletos(invitado, invitado.Boletos);

                    // Generar código QR
                    string qrData = qrService.GenerarDatosQR(invitado, config);
                    invitado.QRCode = qrService.GenerarQRCode(qrData);

                    // Guardar como imagen
                    string qrFileName = $"QR_{invitado.UnidadNegocio}_{invitado.ID}_{invitado.Nombre.Replace(" ", "_")}.png";
                    string qrPath = Path.Combine(qrFolder, qrFileName);
                    qrService.GuardarQRComoImagen(invitado.QRCode, qrPath);
                    invitado.QRCodePath = qrPath;
                }
            }

            // Preparar lista de todos los boletos para la rifa
            todosLosBoletos.Clear();
            foreach (var unidad in unidadesNegocio)
            {
                foreach (var invitado in unidad.Invitados)
                {
                    todosLosBoletos.AddRange(invitado.NumerosBoletos);
                }
            }
        }

        private void ActualizarTabVisualizacion()
        {
            tabUnidades.TabPages.Clear();

            foreach (var unidad in unidadesNegocio)
            {
                var tabPage = new TabPage(unidad.Nombre);
                tabPage.BackColor = Color.White;

                // Crear DataGridView
                var dgv = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    AutoGenerateColumns = false,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    ReadOnly = true,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    RowHeadersVisible = false,
                    BackgroundColor = Color.White
                };

                // Configurar columnas
                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "ID",
                    HeaderText = "ID",
                    Width = 60
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Nombre",
                    HeaderText = "Nombre",
                    Width = 200
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "FechaIngreso",
                    HeaderText = "Ingreso",
                    Width = 100
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Correo",
                    HeaderText = "Correo",
                    Width = 200
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "AniosServicio",
                    HeaderText = "Años",
                    Width = 60
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Boletos",
                    HeaderText = "Boletos",
                    Width = 60
                });

                // Columna para ver QR
                var btnQR = new DataGridViewButtonColumn
                {
                    HeaderText = "QR",
                    Text = "Ver QR",
                    UseColumnTextForButtonValue = true,
                    Width = 70
                };
                dgv.Columns.Add(btnQR);

                dgv.DataSource = unidad.Invitados;

                // Manejar clic en botón QR
                dgv.CellClick += (s, e) =>
                {
                    if (e.ColumnIndex == 6 && e.RowIndex >= 0) // Columna del botón QR
                    {
                        var invitado = (Invitado)dgv.Rows[e.RowIndex].DataBoundItem;
                        MostrarQR(invitado);
                    }
                };

                // Panel de estadísticas
                var panelStats = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 40,
                    BackColor = Color.FromArgb(240, 240, 240),
                    Padding = new Padding(10)
                };

                var lblStats = new Label
                {
                    Text = $"Invitados: {unidad.TotalInvitados} | Boletos: {unidad.TotalBoletos}",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 102, 204),
                    Dock = DockStyle.Left
                };

                panelStats.Controls.Add(lblStats);

                // Contenedor principal
                var container = new Panel { Dock = DockStyle.Fill };
                container.Controls.Add(dgv);
                container.Controls.Add(panelStats);

                tabPage.Controls.Add(container);
                tabUnidades.TabPages.Add(tabPage);
            }
        }

        private void MostrarQR(Invitado invitado)
        {
            var formQR = new Form
            {
                Text = $"Código QR - {invitado.Nombre}",
                Size = new Size(500, 600),
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // Imagen QR
            var pictureBox = new PictureBox
            {
                Size = new Size(200, 200),
                Location = new Point(30, 20),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BorderStyle = BorderStyle.FixedSingle
            };

            if (invitado.QRCode != null && invitado.QRCode.Length > 0)
            {
                using (var ms = new MemoryStream(invitado.QRCode))
                {
                    pictureBox.Image = Image.FromStream(ms);
                }
            }

            // Información
            var lblInfo = new Label
            {
                Text = $"Nombre: {invitado.Nombre}\n" +
                       $"ID: {invitado.ID}\n" +
                       $"Unidad: {invitado.UnidadNegocio}\n" +
                       $"Años: {invitado.AniosServicio}\n" +
                       $"Boletos: {invitado.Boletos}\n" +
                       $"Números: {string.Join(", ", invitado.NumerosBoletos)}",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(30, 340)
            };

            // Botones
            var btnGuardar = new Button
            {
                Text = "💾 Guardar Imagen",
                Size = new Size(120, 35),
                Location = new Point(30, 420),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White
            };
            btnGuardar.Click += (s, e) =>
            {
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "PNG Image|*.png";
                    saveDialog.FileName = $"QR_{invitado.Nombre.Replace(" ", "_")}.png";
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        File.Copy(invitado.QRCodePath, saveDialog.FileName, true);
                        MessageBox.Show("Imagen guardada exitosamente", "Éxito",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };

            var btnImprimir = new Button
            {
                Text = "🖨️ Imprimir",
                Size = new Size(120, 35),
                Location = new Point(160, 420),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(102, 0, 204),
                ForeColor = Color.White
            };
            btnImprimir.Click += (s, e) => ImprimirQR(invitado);

            var btnCerrar = new Button
            {
                Text = "Cerrar",
                Size = new Size(120, 35),
                Location = new Point(290, 420),
                FlatStyle = FlatStyle.Flat
            };
            btnCerrar.Click += (s, e) => formQR.Close();

            panel.Controls.AddRange(new Control[] { pictureBox, lblInfo, btnGuardar, btnImprimir, btnCerrar });
            formQR.Controls.Add(panel);

            formQR.ShowDialog();
        }

        private void ImprimirQR(Invitado invitado)
        {
            // Implementación simple de impresión
            var printDoc = new System.Drawing.Printing.PrintDocument();
            printDoc.PrintPage += (s, e) =>
            {
                // Dibujar QR
                if (invitado.QRCode != null)
                {
                    using (var ms = new MemoryStream(invitado.QRCode))
                    using (var img = Image.FromStream(ms))
                    {
                        e.Graphics.DrawImage(img, 50, 50, 200, 200);
                    }
                }

                // Información
                var font = new Font("Arial", 12);
                e.Graphics.DrawString($"Evento: {config.NombreEvento}", font, Brushes.Black, 50, 270);
                e.Graphics.DrawString($"Nombre: {invitado.Nombre}", font, Brushes.Black, 50, 300);
                e.Graphics.DrawString($"ID: {invitado.ID}", font, Brushes.Black, 50, 330);
                e.Graphics.DrawString($"Boletos: {string.Join(", ", invitado.NumerosBoletos)}",
                                     font, Brushes.Black, 50, 360);
            };

            using (PrintDialog printDialog = new PrintDialog())
            {
                printDialog.Document = printDoc;
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDoc.Print();
                }
            }
        }

        private void ActualizarEstadisticas()
        {
            if (unidadesNegocio.Any())
            {
                int totalInvitados = unidadesNegocio.Sum(u => u.TotalInvitados);
                int totalBoletos = unidadesNegocio.Sum(u => u.TotalBoletos);

                // Actualizar pestaña de rifa
                lblTotalBoletos.Text = $"Total de boletos para rifa: {totalBoletos}";
            }
        }

        private void BtnPrepararRifa_Click(object sender, EventArgs e)
        {
            if (!todosLosBoletos.Any())
            {
                MessageBox.Show("No hay boletos cargados para la rifa", "Información",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MessageBox.Show($"✅ Rifa preparada con {todosLosBoletos.Count} boletos\n\n" +
                          "Los boletos han sido mezclados aleatoriamente para garantizar un sorteo justo.",
                          "Rifa Preparada", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Mezclar boletos (algoritmo Fisher-Yates)
            for (int i = todosLosBoletos.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                var temp = todosLosBoletos[i];
                todosLosBoletos[i] = todosLosBoletos[j];
                todosLosBoletos[j] = temp;
            }
        }

        private void BtnRealizarSorteo_Click(object sender, EventArgs e)
        {
            if (!todosLosBoletos.Any())
            {
                MessageBox.Show("Primero carga un archivo y prepara la rifa", "Información",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Pedir cantidad de ganadores
            using (var form = new Form())
            {
                form.Text = "Configurar Sorteo";
                form.Size = new Size(300, 200);
                form.StartPosition = FormStartPosition.CenterParent;

                var lbl = new Label
                {
                    Text = "¿Cuántos ganadores deseas seleccionar?",
                    Location = new Point(20, 20),
                    AutoSize = true
                };

                var numGanadores = new NumericUpDown
                {
                    Minimum = 1,
                    Maximum = Math.Min(50, todosLosBoletos.Count),
                    Value = 10,
                    Location = new Point(20, 60),
                    Size = new Size(100, 30)
                };

                var btnAceptar = new Button
                {
                    Text = "Realizar Sorteo",
                    DialogResult = DialogResult.OK,
                    Location = new Point(20, 100)
                };

                var btnCancelar = new Button
                {
                    Text = "Cancelar",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(140, 100)
                };

                form.Controls.AddRange(new Control[] { lbl, numGanadores, btnAceptar, btnCancelar });

                if (form.ShowDialog() == DialogResult.OK)
                {
                    int cantidadGanadores = (int)numGanadores.Value;
                    RealizarSorteo(cantidadGanadores);
                }
            }
        }

        private void RealizarSorteo(int cantidadGanadores)
        {
            listBoxGanadores.Items.Clear();

            // Seleccionar ganadores aleatorios
            var boletosMezclados = new List<string>(todosLosBoletos);
            var ganadores = new List<string>();

            // Mezclar nuevamente para mayor aleatoriedad
            for (int i = boletosMezclados.Count - 1; i > 0 && ganadores.Count < cantidadGanadores; i--)
            {
                int j = rnd.Next(i + 1);
                var temp = boletosMezclados[i];
                boletosMezclados[i] = boletosMezclados[j];
                boletosMezclados[j] = temp;

                // Tomar como ganador
                ganadores.Add(boletosMezclados[i]);
            }

            // Mostrar ganadores
            listBoxGanadores.Items.Add($"🎉 RESULTADOS DEL SORTEO - {DateTime.Now:dd/MM/yyyy HH:mm}");
            listBoxGanadores.Items.Add("");

            for (int i = 0; i < ganadores.Count; i++)
            {
                // Encontrar al invitado dueño del boleto
                var boletoGanador = ganadores[i];
                var invitadoGanador = EncontrarInvitadoPorBoleto(boletoGanador);

                string infoGanador = invitadoGanador != null
                    ? $"{i + 1}. 🏆 {invitadoGanador.Nombre} (ID: {invitadoGanador.ID})"
                    : $"{i + 1}. 🏆 Boleto: {boletoGanador}";

                listBoxGanadores.Items.Add(infoGanador);
                listBoxGanadores.Items.Add($"   🎫 Boleto ganador: {boletoGanador}");

                if (invitadoGanador != null)
                {
                    listBoxGanadores.Items.Add($"   📧 Correo: {invitadoGanador.Correo}");
                    listBoxGanadores.Items.Add($"   🏢 Unidad: {invitadoGanador.UnidadNegocio}");
                }
                listBoxGanadores.Items.Add("");
            }

            MessageBox.Show($"🎊 Sorteo completado: {cantidadGanadores} ganadores seleccionados",
                          "Sorteo Realizado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private Invitado EncontrarInvitadoPorBoleto(string numeroBoleto)
        {
            foreach (var unidad in unidadesNegocio)
            {
                foreach (var invitado in unidad.Invitados)
                {
                    if (invitado.NumerosBoletos.Contains(numeroBoleto))
                        return invitado;
                }
            }
            return null;
        }

        private void BtnGuardarResultados_Click(object sender, EventArgs e)
        {
            if (listBoxGanadores.Items.Count == 0)
            {
                MessageBox.Show("No hay resultados para guardar", "Información",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Archivo de texto|*.txt|Archivo CSV|*.csv";
                saveDialog.FileName = $"Resultados_Rifa_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in listBoxGanadores.Items)
                    {
                        sb.AppendLine(item.ToString());
                    }

                    File.WriteAllText(saveDialog.FileName, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show($"Resultados guardados en:\n{saveDialog.FileName}",
                                  "Guardado Exitoso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnExportar_Click(object sender, EventArgs e)
        {
            if (!unidadesNegocio.Any())
            {
                MessageBox.Show("Primero carga un archivo con invitados", "Información",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var btn = (Button)sender;
            string tipo = btn.Tag.ToString();

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                switch (tipo)
                {
                    case "excel":
                        saveDialog.Filter = "Archivo Excel|*.xlsx";
                        saveDialog.FileName = $"Invitados_Evento_{DateTime.Now:yyyyMMdd}.xlsx";
                        break;

                    case "csv":
                        saveDialog.Filter = "Archivo CSV|*.csv";
                        saveDialog.FileName = $"Boletos_Rifa_{DateTime.Now:yyyyMMdd}.csv";
                        break;

                    case "qr":
                        saveDialog.Filter = "Carpeta|*.";
                        saveDialog.FileName = "Seleccionar carpeta para QR";
                        break;

                    case "html":
                        saveDialog.Filter = "Archivo HTML|*.html";
                        saveDialog.FileName = $"Invitados_Evento_{DateTime.Now:yyyyMMdd}.html";
                        break;
                }

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    Cursor = Cursors.WaitCursor;
                    try
                    {
                        switch (tipo)
                        {
                            case "excel":
                                excelService.ExportarExcel(unidadesNegocio, saveDialog.FileName);
                                break;

                            case "csv":
                                ticketService.ExportarBoletosCSV(unidadesNegocio, saveDialog.FileName);
                                break;

                            case "qr":
                                // Crear carpeta y copiar todos los QR
                                string destino = Path.GetDirectoryName(saveDialog.FileName);
                                string qrFolder = Path.Combine(Application.StartupPath, "QR_Codes");
                                if (Directory.Exists(qrFolder))
                                {
                                    CopiarDirectorio(qrFolder, destino);
                                }
                                break;

                            case "html":
                                GenerarPaginaHTML(saveDialog.FileName);
                                break;
                        }

                        MessageBox.Show($"Exportación completada:\n{saveDialog.FileName}",
                                      "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exportando:\n{ex.Message}", "Error",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                }
            }
        }

        private void CopiarDirectorio(string origen, string destino)
        {
            if (!Directory.Exists(destino))
                Directory.CreateDirectory(destino);

            foreach (string file in Directory.GetFiles(origen))
            {
                string destFile = Path.Combine(destino, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }
        }

        private void GenerarPaginaHTML(string filePath)
        {
            string html = @"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Lista de Invitados - Evento</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; background-color: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; background: white; padding: 20px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        h1 { color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }
        .unidad { background: #ecf0f1; margin: 20px 0; padding: 15px; border-radius: 5px; }
        .unidad h2 { color: #2980b9; margin-top: 0; }
        table { width: 100%; border-collapse: collapse; margin: 10px 0; }
        th { background: #3498db; color: white; padding: 10px; text-align: left; }
        td { padding: 8px; border-bottom: 1px solid #ddd; }
        tr:hover { background: #f8f9fa; }
        .qr-cell { text-align: center; }
        .qr-img { max-width: 100px; height: auto; }
        .stats { background: #2ecc71; color: white; padding: 10px; border-radius: 5px; margin: 10px 0; }
        .footer { margin-top: 30px; text-align: center; color: #7f8c8d; font-size: 12px; }
    </style>
</head>
<body>
    <div class='container'>
        <h1>📋 Lista de Invitados - " + config.NombreEvento + @"</h1>
        <p><strong>Fecha del evento:</strong> " + config.FechaEvento.ToString("dd/MM/yyyy") + @"</p>
        
        <div class='stats'>
            <strong>ESTADÍSTICAS:</strong> 
            Total Invitados: " + unidadesNegocio.Sum(u => u.TotalInvitados) + @" | 
            Total Boletos: " + unidadesNegocio.Sum(u => u.TotalBoletos) + @"
        </div>";

            foreach (var unidad in unidadesNegocio)
            {
                html += $@"
        <div class='unidad'>
            <h2>🏢 {unidad.Nombre} ({unidad.TotalInvitados} invitados, {unidad.TotalBoletos} boletos)</h2>
            <table>
                <tr>
                    <th>ID</th>
                    <th>Nombre</th>
                    <th>Ingreso</th>
                    <th>Correo</th>
                    <th>Años</th>
                    <th>Boletos</th>
                    <th>Números de Boletos</th>
                    <th>Código QR</th>
                </tr>";

                foreach (var invitado in unidad.Invitados)
                {
                    // Convertir QR a Base64 para mostrar en HTML
                    string qrBase64 = invitado.QRCode != null
                        ? Convert.ToBase64String(invitado.QRCode)
                        : "";

                    html += $@"
                <tr>
                    <td>{invitado.ID}</td>
                    <td>{invitado.Nombre}</td>
                    <td>{invitado.FechaIngreso.ToString("dd/MM/yyyy")}</td>
                    <td>{invitado.Correo}</td>
                    <td>{invitado.AniosServicio}</td>
                    <td>{invitado.Boletos}</td>
                    <td>{string.Join(", ", invitado.NumerosBoletos)}</td>
                    <td class='qr-cell'>
                        {(invitado.QRCode != null ? $"<img class='qr-img' src='data:image/png;base64,{qrBase64}' alt='QR Code'/>" : "No generado")}
                    </td>
                </tr>";
                }

                html += @"
            </table>
        </div>";
            }

            html += @"
        <div class='footer'>
            <p>Generado automáticamente por EventManagerSimple - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + @"</p>
            <p>Este archivo funciona completamente offline</p>
        </div>
    </div>
</body>
</html>";

            File.WriteAllText(filePath, html, Encoding.UTF8);
        }

        private void CargarConfiguracion()
        {
            string configFile = Path.Combine(Application.StartupPath, "config.json");
            if (File.Exists(configFile))
            {
                try
                {
                    string json = File.ReadAllText(configFile);
                    config = Newtonsoft.Json.JsonConvert.DeserializeObject<EventoConfig>(json);

                    // Actualizar controles si existen
                    if (txtNombreEvento != null) txtNombreEvento.Text = config.NombreEvento;
                    if (dateFechaEvento != null) dateFechaEvento.Value = config.FechaEvento;
                    if (numBoletos1a5 != null) numBoletos1a5.Value = config.Boletos1a5Anios;
                    if (numBoletos6a10 != null) numBoletos6a10.Value = config.Boletos6a10Anios;
                    if (numBoletos11plus != null) numBoletos11plus.Value = config.Boletos11PlusAnios;
                }
                catch
                {
                    // Si hay error, usar configuración por defecto
                }
            }
        }

        private void BtnGuardarConfig_Click(object sender, EventArgs e)
        {
            config.NombreEvento = txtNombreEvento.Text;
            config.FechaEvento = dateFechaEvento.Value;
            config.Boletos1a5Anios = (int)numBoletos1a5.Value;
            config.Boletos6a10Anios = (int)numBoletos6a10.Value;
            config.Boletos11PlusAnios = (int)numBoletos11plus.Value;

            string configFile = Path.Combine(Application.StartupPath, "config.json");
            config.Guardar(configFile);

            MessageBox.Show("✅ Configuración guardada exitosamente", "Configuración",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDescargarPlantilla_Click(object sender, EventArgs e)
        {
            try
            {
                string plantillaPath = Path.Combine(Application.StartupPath, "plantilla_invitados.xlsx");

                using (var workbook = new ClosedXML.Excel.XLWorkbook())
                {
                    // Hoja de ejemplo TGAB
                    var ws1 = workbook.Worksheets.Add("TGAB");
                    ws1.Cell("A1").Value = "ID";
                    ws1.Cell("B1").Value = "NOMBRE";
                    ws1.Cell("C1").Value = "INGRESO";
                    ws1.Cell("D1").Value = "CORREO";

                    ws1.Cell("A2").Value = 1001;
                    ws1.Cell("B2").Value = "Juan Pérez López";
                    ws1.Cell("C2").Value = "15/03/2020";
                    ws1.Cell("D2").Value = "juan.perez@empresa.com";

                    // Hoja de ejemplo COVEMEX
                    var ws2 = workbook.Worksheets.Add("COVEMEX");
                    ws2.Cell("A1").Value = "ID";
                    ws2.Cell("B1").Value = "NOMBRE";
                    ws2.Cell("C1").Value = "INGRESO";
                    ws2.Cell("D1").Value = "CORREO";

                    ws2.Cell("A2").Value = 2001;
                    ws2.Cell("B2").Value = "María García Sánchez";
                    ws2.Cell("C2").Value = "01/06/2018";
                    ws2.Cell("D2").Value = "maria.garcia@empresa.com";

                    // Autoajustar columnas
                    ws1.Columns().AdjustToContents();
                    ws2.Columns().AdjustToContents();

                    workbook.SaveAs(plantillaPath);
                }

                MessageBox.Show($"✅ Plantilla creada en:\n{plantillaPath}\n\n" +
                              "Puedes usar este archivo como ejemplo para tu lista de invitados.",
                              "Plantilla Creada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creando plantilla:\n{ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static class EmailTemplates
        {
            public static string PlantillaElegante(Invitado invitado, EventoConfig config, string qrBase64)
            {
                return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        .invitation {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 40px;
            border-radius: 20px;
            max-width: 600px;
            margin: 0 auto;
        }}
        .gold-text {{ color: #FFD700; }}
    </style>
</head>
<body>
    <div class='invitation'>
        <h1 class='gold-text'>INVITACIÓN EXCLUSIVA</h1>
        <h2>{config.NombreEvento}</h2>
        <p>Para: {invitado.Nombre}</p>
        <img src='data:image/png;base64,{qrBase64}' width='150'/>
    </div>
</body>
</html>";
            }

            public static string PlantillaMinimalista(Invitado invitado, EventoConfig config, string qrBase64)
            {
                return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Helvetica Neue', Arial, sans-serif; }}
        .card {{ 
            border: 1px solid #e0e0e0; 
            border-radius: 8px; 
            padding: 30px; 
            max-width: 500px;
        }}
    </style>
</head>
<body>
    <div class='card'>
        <h2>Invitación</h2>
        <p><strong>{invitado.Nombre}</strong></p>
        <p>{config.NombreEvento}</p>
        <img src='data:image/png;base64,{qrBase64}' width='120'/>
    </div>
</body>
</html>";
            }
        }



        // InicializarComponent (generado automáticamente)
        private void InitializeComponent()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(Form1));
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            tabPage3 = new TabPage();
            tabPage4 = new TabPage();
            tabPage5 = new TabPage();
            tabControl1.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Controls.Add(tabPage5);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1184, 761);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Size = new Size(1176, 733);
            tabPage1.TabIndex = 0;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Size = new Size(1176, 733);
            tabPage2.TabIndex = 1;
            // 
            // tabPage3
            // 
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(1176, 733);
            tabPage3.TabIndex = 2;
            // 
            // tabPage4
            // 
            tabPage4.Location = new Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(1176, 733);
            tabPage4.TabIndex = 3;
            // 
            // tabPage5
            // 
            tabPage5.Location = new Point(4, 24);
            tabPage5.Name = "tabPage5";
            tabPage5.Size = new Size(1176, 733);
            tabPage5.TabIndex = 4;
            // 
            // Form1
            // 
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ClientSize = new Size(1184, 761);
            Controls.Add(tabControl1);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Gestor de Eventos - Versión Offline";
            Load += Form1_Load;
            tabControl1.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CargarConfiguracion();
        }


    }
}