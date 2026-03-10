using EventManager.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EventManager.Services
{
    /// <summary>
    /// Servicio para generar y enviar invitaciones por email
    /// </summary>
    public class EmailGenerator
    {
        private EmailConfig _config;
        private readonly QRService _qrService;

        public EmailGenerator()
        {
            _qrService = new QRService();
            _config = new EmailConfig();
        }
        public EmailGenerator(EmailConfig config)
        {
            _config = config;
            _qrService = new QRService();
        }

        // ============================================
        // 1. GENERACIÓN DE HTML
        // ============================================

        /// <summary>
        /// Genera el HTML de la invitación para un invitado específico
        /// </summary>
        public string GenerarInvitacionHTML(Invitado invitado, EventoConfig eventoConfig, bool incluirQR = true)
        {
            string qrBase64 = invitado.QRCode != null && incluirQR
                ? Convert.ToBase64String(invitado.QRCode)
                : string.Empty;

            // Formatear fecha en español
            var cultura = new System.Globalization.CultureInfo("es-MX");
            string fechaEvento = eventoConfig.FechaEvento.ToString("dddd, dd 'de' MMMM 'de' yyyy", cultura);

            // Formatear números de boletos como lista HTML
            string htmlBoletos = "";
            if (invitado.NumerosBoletos != null && invitado.NumerosBoletos.Count > 0)
            {
                foreach (var boleto in invitado.NumerosBoletos)
                {
                    htmlBoletos += $"<div class='ticket-number'>{boleto}</div>";
                }
            }
            else
            {
                htmlBoletos = "<p>No se asignaron boletos</p>";
            }

            return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Invitación al Evento</title>
    <style>
        /* RESET BÁSICO */
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333333;
            background-color: #f8f9fa;
            margin: 0;
            padding: 0;
            -webkit-font-smoothing: antialiased;
        }}
        
        /* CONTENEDOR PRINCIPAL */
        .email-container {{
            max-width: 650px;
            margin: 0 auto;
            background: #ffffff;
            border-radius: 16px;
            overflow: hidden;
            box-shadow: 0 8px 30px rgba(0, 0, 0, 0.08);
        }}
        
        /* ENCABEZADO CON GRADIENTE */
        .header {{
            background: linear-gradient(135deg, #1a2980 0%, #26d0ce 100%);
            color: white;
            padding: 40px 30px;
            text-align: center;
            position: relative;
            overflow: hidden;
        }}
        
        .header::before {{
            content: '';
            position: absolute;
            top: -50%;
            right: -50%;
            width: 200%;
            height: 200%;
            background: rgba(255, 255, 255, 0.1);
            transform: rotate(30deg);
        }}
        
        .header h1 {{
            font-size: 32px;
            margin-bottom: 10px;
            color: white;
            font-weight: 700;
            position: relative;
            z-index: 1;
            text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
        }}
        
        .header-subtitle {{
            font-size: 16px;
            opacity: 0.9;
            position: relative;
            z-index: 1;
        }}
        
        /* CONTENIDO PRINCIPAL */
        .content {{
            padding: 40px 35px;
        }}
        
        /* SALUDO PERSONALIZADO */
        .greeting {{
            font-size: 18px;
            margin-bottom: 25px;
            color: #2c3e50;
            border-bottom: 2px solid #f0f0f0;
            padding-bottom: 15px;
        }}
        
        .greeting strong {{
            color: #1a2980;
            font-size: 20px;
        }}
        
        /* DETALLES DEL EVENTO */
        .event-details {{
            background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
            border-radius: 12px;
            padding: 25px;
            margin: 25px 0;
            border-left: 5px solid #1a2980;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
        }}
        
        .event-title {{
            color: #1a2980;
            margin-bottom: 20px;
            font-size: 24px;
            font-weight: 600;
        }}
        
        .detail-item {{
            margin-bottom: 12px;
            display: flex;
            align-items: flex-start;
        }}
        
        .detail-icon {{
            width: 24px;
            margin-right: 12px;
            text-align: center;
            font-size: 18px;
            color: #26d0ce;
            flex-shrink: 0;
        }}
        
        .detail-text {{
            flex: 1;
            color: #2c3e50;
        }}
        
        /* SECCIÓN QR */
        .qr-section {{
            text-align: center;
            padding: 30px;
            background: linear-gradient(to right, #ffffff, #f8f9fa);
            border-radius: 12px;
            margin: 30px 0;
            border: 2px solid #e9ecef;
            position: relative;
        }}
        
        .qr-section::before {{
            content: '⚠️ IMPORTANTE';
            position: absolute;
            top: -12px;
            left: 50%;
            transform: translateX(-50%);
            background: #ff6b6b;
            color: white;
            padding: 5px 15px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: bold;
        }}
        
        .qr-code {{
            max-width: 220px;
            height: auto;
            margin: 20px auto;
            border: 4px solid white;
            border-radius: 10px;
            box-shadow: 0 6px 20px rgba(0, 0, 0, 0.1);
            transition: transform 0.3s ease;
        }}
        
        .qr-code:hover {{
            transform: scale(1.03);
        }}
        
        .qr-instruction {{
            color: #666;
            font-size: 14px;
            margin-top: 15px;
            background: #f8f9fa;
            padding: 10px;
            border-radius: 6px;
            display: inline-block;
        }}
        
        /* SECCIÓN DE BOLETOS */
        .tickets-section {{
            background: linear-gradient(135deg, #2ecc71 0%, #27ae60 100%);
            color: white;
            padding: 25px;
            border-radius: 12px;
            margin: 25px 0;
            position: relative;
            overflow: hidden;
        }}
        
        .tickets-section::before {{
            content: '🎫';
            position: absolute;
            top: -20px;
            right: -20px;
            font-size: 80px;
            opacity: 0.2;
            transform: rotate(15deg);
        }}
        
        .tickets-title {{
            margin-bottom: 15px;
            font-size: 22px;
            font-weight: 600;
            position: relative;
            z-index: 1;
        }}
        
        .years-service {{
            font-size: 16px;
            margin-bottom: 10px;
            opacity: 0.9;
            position: relative;
            z-index: 1;
        }}
        
        .ticket-count {{
            font-size: 32px;
            margin: 15px 0;
            font-weight: 700;
            text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
            position: relative;
            z-index: 1;
        }}
        
        .ticket-list {{
            background: rgba(255, 255, 255, 0.15);
            padding: 20px;
            border-radius: 8px;
            margin: 20px 0;
            backdrop-filter: blur(10px);
        }}
        
        .ticket-number {{
            background: white;
            color: #2c3e50;
            padding: 10px 15px;
            margin: 8px;
            border-radius: 6px;
            display: inline-block;
            font-family: 'Courier New', monospace;
            font-weight: 600;
            box-shadow: 0 3px 6px rgba(0, 0, 0, 0.1);
            font-size: 14px;
            transition: all 0.3s ease;
        }}
        
        .ticket-number:hover {{
            transform: translateY(-2px);
            box-shadow: 0 5px 10px rgba(0, 0, 0, 0.15);
        }}
        
        /* NOTAS IMPORTANTES */
        .important-notes {{
            background: #fff3cd;
            border-left: 5px solid #ffc107;
            padding: 20px;
            margin: 25px 0;
            border-radius: 8px;
        }}
        
        .notes-title {{
            color: #856404;
            margin-bottom: 15px;
            font-size: 18px;
            font-weight: 600;
            display: flex;
            align-items: center;
        }}
        
        .notes-title::before {{
            content: '📋';
            margin-right: 10px;
            font-size: 20px;
        }}
        
        .notes-list {{
            padding-left: 20px;
        }}
        
        .notes-list li {{
            margin-bottom: 8px;
            color: #856404;
        }}
        
        /* PIE DE PÁGINA */
        .footer {{
            background: linear-gradient(135deg, #2c3e50 0%, #34495e 100%);
            color: white;
            padding: 25px;
            text-align: center;
            font-size: 13px;
        }}
        
        .footer p {{
            margin: 5px 0;
            opacity: 0.8;
        }}
        
        .event-id {{
            margin-top: 15px;
            padding-top: 15px;
            border-top: 1px solid rgba(255, 255, 255, 0.1);
            font-size: 11px;
            color: #bbb;
            font-family: monospace;
        }}
        
        /* RESPONSIVE */
        @media (max-width: 600px) {{
            .content {{
                padding: 20px;
            }}
            
            .header h1 {{
                font-size: 26px;
            }}
            
            .qr-code {{
                max-width: 180px;
            }}
            
            .ticket-number {{
                display: block;
                margin: 8px 0;
            }}
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <!-- ENCABEZADO -->
        <div class='header'>
            <h1>🎉 ¡ESTÁS INVITADO! 🎉</h1>
            <p class='header-subtitle'>Un evento especial para nuestros colaboradores</p>
        </div>
        
        <!-- CONTENIDO -->
        <div class='content'>
            <!-- SALUDO PERSONAL -->
            <div class='greeting'>
                <p>Estimado(a) <strong>{invitado.Nombre}</strong>,</p>
                <p>Es un honor para nosotros invitarte a nuestro evento anual como reconocimiento a tu dedicación y esfuerzo.</p>
            </div>
            
            <!-- DETALLES DEL EVENTO -->
            <div class='event-details'>
                <h2 class='event-title'>{eventoConfig.NombreEvento}</h2>
                
                <div class='detail-item'>
                    <span class='detail-icon'>📅</span>
                    <span class='detail-text'><strong>Fecha:</strong> {fechaEvento}</span>
                </div>
                
                <div class='detail-item'>
                    <span class='detail-icon'>🕒</span>
                    <span class='detail-text'><strong>Hora:</strong> 19:00 horas (registro desde 18:30)</span>
                </div>
                
                <div class='detail-item'>
                    <span class='detail-icon'>📍</span>
                    <span class='detail-text'><strong>Lugar:</strong> {eventoConfig.Lugar}</span>
                </div>
                
                <div class='detail-item'>
                    <span class='detail-icon'>👔</span>
                    <span class='detail-text'><strong>Código de Vestimenta:</strong> Formal / Etiqueta</span>
                </div>
            </div>
            
            <!-- CÓDIGO QR -->
            <div class='qr-section'>
                <h3 style='color: #1a2980; margin-bottom: 15px;'>TU CÓDIGO DE ACCESO</h3>
                {(incluirQR && !string.IsNullOrEmpty(qrBase64)
                    ? $@"<img class='qr-code' src='data:image/png;base64,{qrBase64}' alt='Código QR del Evento' />"
                    : "<p style='color: #666;'>El código QR será proporcionado en la entrada del evento</p>")}
                <p class='qr-instruction'>
                    <strong>IMPORTANTE:</strong> Presenta este código QR en la entrada para tu registro rápido.
                </p>
            </div>
            
            <!-- BOLETOS PARA LA RIFA -->
            <div class='tickets-section'>
                <h3 class='tickets-title'>🎫 TUS BOLETOS PARA LA RIFA</h3>
                <p class='years-service'>Por tus <strong>{invitado.AniosServicio} años</strong> de servicio continuo:</p>
                <p class='ticket-count'>{invitado.Boletos} BOLETO(S)</p>
                
                <div class='ticket-list'>
                    <p style='margin-bottom: 15px;'><strong>Números asignados:</strong></p>
                    {htmlBoletos}
                </div>
                
                <p style='margin-top: 15px; font-style: italic; opacity: 0.9;'>
                    ¡Cada boleto es una oportunidad para ganar increíbles premios! 🏆
                </p>
            </div>
            
            <!-- NOTAS IMPORTANTES -->
            <div class='important-notes'>
                <h4 class='notes-title'>INFORMACIÓN IMPORTANTE</h4>
                <ul class='notes-list'>
                    <li>✅ Presenta tu código QR al ingresar (físico o digital)</li>
                    <li>✅ Los boletos son personales e intransferibles</li>
                    <li>✅ La rifa se realizará durante la cena</li>
                    <li>✅ Es necesario estar presente para reclamar premios</li>
                    <li>✅ Confirma tu asistencia antes del {eventoConfig.FechaEvento.AddDays(-3):dd/MM/yyyy}</li>
                    <li>✅ Estacionamiento gratuito disponible</li>
                </ul>
            </div>
            
            <!-- MENSAJE FINAL -->
            <div style='text-align: center; margin: 35px 0 20px; padding: 20px; border-top: 2px solid #f0f0f0;'>
                <p style='font-size: 16px; color: #2c3e50; margin-bottom: 10px;'>
                    Esperamos contar con tu presencia para hacer de este evento<br>
                    una celebración inolvidable.
                </p>
                <p style='margin-top: 20px;'>
                    <strong style='color: #1a2980; font-size: 18px;'>Comité Organizador</strong><br>
                    <span style='color: #666;'>{eventoConfig.NombreEvento}</span>
                </p>
            </div>
        </div>
        
        <!-- PIE DE PÁGINA -->
        <div class='footer'>
            <p>© {DateTime.Now.Year} {eventoConfig.NombreEvento}. Todos los derechos reservados.</p>
            <p>Este es un correo automático generado por nuestro sistema de gestión de eventos.</p>
            <p>Por favor no responder a este mensaje.</p>
            
            <div class='event-id'>
                ID: {invitado.ID} | Unidad: {invitado.UnidadNegocio} | Código: {invitado.CodigoVerificacion}
            </div>
        </div>
    </div>
</body>
</html>";
        }

        // ============================================
        // 2. GUARDAR COMO ARCHIVO HTML (OFFLINE)
        // ============================================

        /// <summary>
        /// Guarda la invitación como archivo HTML en el disco
        /// </summary>
        public void GuardarInvitacionComoArchivo(Invitado invitado, EventoConfig config, string folderPath)
        {
            try
            {
                // Crear carpeta si no existe
                Directory.CreateDirectory(folderPath);

                // Generar HTML
                string html = GenerarInvitacionHTML(invitado, config);

                // Crear nombre de archivo seguro
                string nombreArchivo = $"Invitacion_{SanitizeFileName(invitado.UnidadNegocio)}_{invitado.ID}_{SanitizeFileName(invitado.Nombre)}.html";
                string filePath = Path.Combine(folderPath, nombreArchivo);

                // Guardar archivo
                File.WriteAllText(filePath, html, Encoding.UTF8);

                // Crear también una versión en texto plano para imprimir
                string textoPlano = GenerarTextoPlano(invitado, config);
                string txtPath = Path.Combine(folderPath, $"Invitacion_{invitado.ID}_resumen.txt");
                File.WriteAllText(txtPath, textoPlano, Encoding.UTF8);

                Console.WriteLine($"✅ Invitación guardada: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error guardando invitación: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Genera una versión en texto plano para impresión
        /// </summary>
        private string GenerarTextoPlano(Invitado invitado, EventoConfig config)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=".PadRight(50, '='));
            sb.AppendLine($"INVITACIÓN OFICIAL - {config.NombreEvento}");
            sb.AppendLine("=".PadRight(50, '='));
            sb.AppendLine();
            sb.AppendLine($"Para: {invitado.Nombre}");
            sb.AppendLine($"ID: {invitado.ID}");
            sb.AppendLine($"Unidad: {invitado.UnidadNegocio}");
            sb.AppendLine($"Correo: {invitado.Correo}");
            sb.AppendLine();
            sb.AppendLine("DETALLES DEL EVENTO:");
            sb.AppendLine($"  • Evento: {config.NombreEvento}");
            sb.AppendLine($"  • Fecha: {config.FechaEvento:dddd, dd 'de' MMMM 'de' yyyy}");
            sb.AppendLine($"  • Hora: 19:00 horas");
            sb.AppendLine($"  • Lugar: {config.Lugar}");
            sb.AppendLine();
            sb.AppendLine("INFORMACIÓN DE ASISTENCIA:");
            sb.AppendLine($"  • Años de servicio: {invitado.AniosServicio} años");
            sb.AppendLine($"  • Boletos para rifa: {invitado.Boletos}");

            if (invitado.NumerosBoletos != null && invitado.NumerosBoletos.Count > 0)
            {
                sb.AppendLine($"  • Números de boletos:");
                foreach (var boleto in invitado.NumerosBoletos)
                {
                    sb.AppendLine($"    - {boleto}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("INSTRUCCIONES:");
            sb.AppendLine("  1. Presentar código QR en la entrada");
            sb.AppendLine("  2. Los boletos son personales");
            sb.AppendLine("  3. Necesario estar presente para premios");
            sb.AppendLine();
            sb.AppendLine("=".PadRight(50, '='));
            sb.AppendLine($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Código: {invitado.CodigoVerificacion}");

            return sb.ToString();
        }

        /// <summary>
        /// Limpia el nombre de archivo de caracteres inválidos
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "invitado";

            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries))
                .Replace(" ", "_")
                .Trim();
        }

        // ============================================
        // 3. ENVÍO POR EMAIL (ONLINE)
        // ============================================

        /// <summary>
        /// Envía la invitación por correo electrónico
        /// </summary>
        public async Task<bool> EnviarInvitacionPorCorreo(Invitado invitado, EventoConfig eventoConfig, EmailConfig emailConfig = null)
        {
            var configToUse = emailConfig ?? _config;

            if (configToUse == null)
                throw new InvalidOperationException("❌ La configuración de correo no está establecida.");

            if (string.IsNullOrWhiteSpace(invitado.Correo))
                throw new ArgumentException("❌ El invitado no tiene correo electrónico válido.");

            try
            {
                using (var smtpClient = new SmtpClient(configToUse.SmtpServer, configToUse.SmtpPort))
                {
                    // Configurar credenciales
                    smtpClient.Credentials = new NetworkCredential(configToUse.Username, configToUse.Password);
                    smtpClient.EnableSsl = configToUse.UseSsl;
                    smtpClient.Timeout = 30000; // 30 segundos

                    // Crear mensaje
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(configToUse.SenderEmail, configToUse.SenderName),
                        Subject = $"{eventoConfig.NombreEvento} - Tu Invitación Oficial",
                        Body = GenerarInvitacionHTML(invitado, eventoConfig),
                        IsBodyHtml = true,
                        Priority = MailPriority.High,
                        Headers = {
                            { "X-Event-ID", eventoConfig.NombreEvento.Replace(" ", "_") },
                            { "X-Invitee-ID", invitado.ID.ToString() }
                        }
                    };

                    // Agregar destinatario
                    mailMessage.To.Add(new MailAddress(invitado.Correo, invitado.Nombre));

                    // Adjuntar código QR como imagen embebida
                    if (invitado.QRCode != null && invitado.QRCode.Length > 0)
                    {
                        var qrAttachment = CreateQRCodeAttachment(invitado);
                        mailMessage.Attachments.Add(qrAttachment);
                    }

                    // Adjuntar versión en texto plano
                    var textoPlano = GenerarTextoPlano(invitado, eventoConfig);
                    var textoAttachment = CreateTextAttachment(invitado, textoPlano);
                    mailMessage.Attachments.Add(textoAttachment);

                    // Enviar email
                    await smtpClient.SendMailAsync(mailMessage);

                    // Registrar envío exitoso
                    LogEmailEnviado(invitado, true, null);

                    return true;
                }
            }
            catch (Exception ex)
            {
                // Registrar error
                LogEmailEnviado(invitado, false, ex.Message);

                // Lanzar excepción con mensaje amigable
                throw new EmailException(
                    $"❌ Error enviando correo a {invitado.Correo}:\n" +
                    $"{GetFriendlyErrorMessage(ex)}",
                    ex);
            }
        }

        /// <summary>
        /// Crea un adjunto con el código QR
        /// </summary>
        private Attachment CreateQRCodeAttachment(Invitado invitado)
        {
            using (var stream = new MemoryStream(invitado.QRCode))
            {
                var attachment = new Attachment(stream,
                    $"QR_{invitado.Nombre.Replace(" ", "_")}.png",
                    "image/png");

                // Configurar como adjunto inline
                attachment.ContentDisposition.Inline = true;
                attachment.ContentDisposition.DispositionType = "inline";
                attachment.ContentId = $"qr_{invitado.ID}";

                return attachment;
            }
        }

        /// <summary>
        /// Crea un adjunto con el texto plano
        /// </summary>
        private Attachment CreateTextAttachment(Invitado invitado, string texto)
        {
            var bytes = Encoding.UTF8.GetBytes(texto);
            using (var stream = new MemoryStream(bytes))
            {
                return new Attachment(stream,
                    $"Invitacion_{invitado.Nombre.Replace(" ", "_")}.txt",
                    "text/plain");
            }
        }

        /// <summary>
        /// Obtiene un mensaje de error amigable
        /// </summary>
        private string GetFriendlyErrorMessage(Exception ex)
        {
            if (ex is SmtpException smtpEx)
            {
                switch (smtpEx.StatusCode)
                {
                    case SmtpStatusCode.MailboxBusy:
                    case SmtpStatusCode.MailboxUnavailable:
                        return "El buzón del destinatario no está disponible. Por favor intenta más tarde.";

                    case SmtpStatusCode.ExceededStorageAllocation:
                        return "El buzón del destinatario está lleno.";

                    case SmtpStatusCode.ClientNotPermitted:
                        return "No tienes permiso para enviar correos desde esta cuenta.";

                    case SmtpStatusCode.TransactionFailed:
                        if (ex.Message.Contains("5.7.0") || ex.Message.Contains("5.7.1"))
                            return "Error de autenticación. Verifica usuario y contraseña.";
                        return "Error en la transacción SMTP.";

                    default:
                        return $"Error SMTP: {smtpEx.StatusCode}";
                }
            }

            if (ex.Message.Contains("5.7.0") || ex.Message.Contains("5.7.1"))
                return "Error de autenticación. Para Gmail, necesitas usar 'Contraseña de aplicación'.";

            if (ex.Message.Contains("timed out"))
                return "Timeout. Verifica tu conexión a internet.";

            return ex.Message;
        }

        // ============================================
        // 4. ENVÍO MASIVO (BATCH)
        // ============================================

        /// <summary>
        /// Envía invitaciones a múltiples invitados
        /// </summary>
        public async Task<EmailBatchResult> EnviarInvitacionesLote(
            List<Invitado> invitados,
            EventoConfig eventoConfig,
            EmailConfig emailConfig = null,
            IProgress<EmailProgress> progress = null,
            int delayBetweenEmails = 1000,
            int batchSize = 10)
        {
            var configToUse = emailConfig ?? _config;

            var result = new EmailBatchResult
            {
                TotalInvitados = invitados.Count,
                StartTime = DateTime.Now
            };

            try
            {
                // Filtrar solo emails válidos
                var invitadosValidos = invitados
                    .Where(i => !string.IsNullOrWhiteSpace(i.Correo) &&
                               i.Correo.Contains("@") &&
                               i.Correo.Contains("."))
                    .ToList();

                result.InvitadosValidos = invitadosValidos.Count;

                // Procesar en lotes
                int enviados = 0;
                int fallidos = 0;

                for (int i = 0; i < invitadosValidos.Count; i++)
                {
                    var invitado = invitadosValidos[i];

                    try
                    {
                        // Enviar con reintentos
                        bool enviado = await EnviarConReintentos(invitado, eventoConfig, 2);

                        if (enviado)
                        {
                            invitado.InvitacionEnviada = true;
                            invitado.InvitacionEnviadaFecha = DateTime.Now;
                            enviados++;

                            // Reportar progreso
                            progress?.Report(new EmailProgress
                            {
                                Current = i + 1,
                                Total = invitadosValidos.Count,
                                CurrentInvited = invitado.Nombre,
                                Status = $"✅ Enviado a {invitado.Correo}",
                                Success = true
                            });
                        }
                        else
                        {
                            fallidos++;
                            result.Errores.Add(new EmailError
                            {
                                Invitado = invitado,
                                ErrorMessage = "Falló después de reintentos"
                            });

                            progress?.Report(new EmailProgress
                            {
                                Current = i + 1,
                                Total = invitadosValidos.Count,
                                CurrentInvited = invitado.Nombre,
                                Status = $"❌ Error con {invitado.Correo}",
                                Success = false
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        fallidos++;
                        result.Errores.Add(new EmailError
                        {
                            Invitado = invitado,
                            ErrorMessage = ex.Message
                        });

                        progress?.Report(new EmailProgress
                        {
                            Current = i + 1,
                            Total = invitadosValidos.Count,
                            CurrentInvited = invitado.Nombre,
                            Status = $"❌ Error: {ex.Message}",
                            Success = false
                        });
                    }

                    // Delay entre emails (evitar bloqueo por spam)
                    if (i < invitadosValidos.Count - 1)
                        await Task.Delay(delayBetweenEmails);

                    // Pausa más larga después de cada lote
                    if ((i + 1) % batchSize == 0 && i < invitadosValidos.Count - 1)
                    {
                        progress?.Report(new EmailProgress
                        {
                            Current = i + 1,
                            Total = invitadosValidos.Count,
                            CurrentInvited = "Pausa...",
                            Status = $"⏸️  Pausa de 5 segundos...",
                            Success = true
                        });

                        await Task.Delay(5000);
                    }
                }

                result.EnviadosExitosos = enviados;
                result.EnviadosFallidos = fallidos;

            }
            catch (Exception ex)
            {
                result.ErrorGeneral = $"Error en el proceso: {ex.Message}";
            }
            finally
            {
                result.EndTime = DateTime.Now;
                result.Duracion = result.EndTime - result.StartTime;
            }

            return result;
        }

        /// <summary>
        /// Envía con reintentos en caso de error temporal
        /// </summary>
        private async Task<bool> EnviarConReintentos(Invitado invitado, EventoConfig config, int maxRetries)
        {
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    await EnviarInvitacionPorCorreo(invitado, config);
                    return true;
                }
                catch (SmtpException smtpEx) when (
                    smtpEx.StatusCode == SmtpStatusCode.MailboxBusy ||
                    smtpEx.StatusCode == SmtpStatusCode.MailboxUnavailable ||
                    smtpEx.StatusCode == SmtpStatusCode.ServiceNotAvailable)
                {
                    // Error temporal, reintentar
                    retryCount++;
                    if (retryCount >= maxRetries)
                        throw;

                    // Backoff exponencial
                    int delay = (int)Math.Pow(2, retryCount) * 1000;
                    await Task.Delay(delay);
                }
                catch
                {
                    // Otro tipo de error, no reintentar
                    throw;
                }
            }

            return false;
        }

        // ============================================
        // 5. LOGGING Y REGISTRO
        // ============================================

        /// <summary>
        /// Registra el resultado del envío de email
        /// </summary>
        private void LogEmailEnviado(Invitado invitado, bool exitoso, string error)
        {
            try
            {
                string logPath = Path.Combine(Application.StartupPath, "logs", "email_log.csv");
                Directory.CreateDirectory(Path.GetDirectoryName(logPath));

                // Si el archivo no existe, crear encabezados
                if (!File.Exists(logPath))
                {
                    File.WriteAllText(logPath,
                        "FechaHora;ID;Nombre;Correo;Unidad;Estado;Error\n",
                        Encoding.UTF8);
                }

                // Agregar registro
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss};" +
                                $"{invitado.ID};" +
                                $"\"{invitado.Nombre}\";" +
                                $"{invitado.Correo};" +
                                $"{invitado.UnidadNegocio};" +
                                $"{(exitoso ? "ENVIADO" : "FALLIDO")};" +
                                $"\"{error ?? ""}\"\n";

                File.AppendAllText(logPath, logEntry, Encoding.UTF8);
            }
            catch
            {
                // Si falla el logging, no afectar la aplicación
            }
        }

        /// <summary>
        /// Genera un reporte del proceso de envío
        /// </summary>
        public string GenerarReporteEnvio(EmailBatchResult resultado)
        {
            var sb = new StringBuilder();

            sb.AppendLine("=".PadRight(60, '='));
            sb.AppendLine("📊 REPORTE DE ENVÍO DE INVITACIONES");
            sb.AppendLine("=".PadRight(60, '='));
            sb.AppendLine();
            sb.AppendLine($"📅 Fecha del proceso: {resultado.StartTime:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"⏱️  Duración total: {resultado.Duracion:mm\\:ss}");
            sb.AppendLine();
            sb.AppendLine("📈 ESTADÍSTICAS:");
            sb.AppendLine($"   • Total de invitados: {resultado.TotalInvitados}");
            sb.AppendLine($"   • Emails válidos: {resultado.InvitadosValidos}");
            sb.AppendLine($"   • ✅ Enviados exitosamente: {resultado.EnviadosExitosos}");
            sb.AppendLine($"   • ❌ Enviados fallidos: {resultado.EnviadosFallidos}");
            sb.AppendLine($"   • 📊 Tasa de éxito: {(resultado.InvitadosValidos > 0 ?
                (resultado.EnviadosExitosos * 100.0 / resultado.InvitadosValidos).ToString("F1") : "0")}%");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(resultado.ErrorGeneral))
            {
                sb.AppendLine("⚠️ ERROR GENERAL:");
                sb.AppendLine($"   {resultado.ErrorGeneral}");
                sb.AppendLine();
            }

            if (resultado.Errores.Any())
            {
                sb.AppendLine("❌ ERRORES DETECTADOS:");
                int errorCount = 1;
                foreach (var error in resultado.Errores.Take(20)) // Mostrar solo primeros 20
                {
                    sb.AppendLine($"   {errorCount++}. {error.Invitado.Nombre} ({error.Invitado.Correo})");
                    sb.AppendLine($"      → {error.ErrorMessage}");
                }

                if (resultado.Errores.Count > 20)
                {
                    sb.AppendLine($"   ... y {resultado.Errores.Count - 20} errores más.");
                }
                sb.AppendLine();
            }

            sb.AppendLine("📋 RECOMENDACIONES:");
            if (resultado.EnviadosFallidos > 0)
            {
                sb.AppendLine("   • Revisar los correos marcados como fallidos");
                sb.AppendLine("   • Verificar la configuración SMTP");
                sb.AppendLine("   • Reintentar el envío para los fallidos");
            }
            else
            {
                sb.AppendLine("   • ¡Todos los correos fueron enviados exitosamente!");
            }

            sb.AppendLine();
            sb.AppendLine("=".PadRight(60, '='));
            sb.AppendLine("✅ PROCESO COMPLETADO");
            sb.AppendLine("=".PadRight(60, '='));

            return sb.ToString();
        }

        /// <summary>
        /// Guarda el reporte en un archivo
        /// </summary>
        public void GuardarReporteEnvio(EmailBatchResult resultado, string carpetaDestino)
        {
            try
            {
                Directory.CreateDirectory(carpetaDestino);

                string reporte = GenerarReporteEnvio(resultado);
                string nombreArchivo = $"Reporte_Envio_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

                File.WriteAllText(rutaCompleta, reporte, Encoding.UTF8);

                Console.WriteLine($"✅ Reporte guardado: {rutaCompleta}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error guardando reporte: {ex.Message}");
            }
        }

        // ============================================
        // 6. MÉTODOS AUXILIARES
        // ============================================

        /// <summary>
        /// Prueba la conexión SMTP con la configuración actual
        /// </summary>
        public async Task<bool> ProbarConexionSMTP(EmailConfig emailConfig)
        {
            var configToUse = emailConfig ?? _config;

            if (configToUse == null)
                throw new InvalidOperationException("La configuración SMTP no está establecida.");

            try
            {
                using (var client = new SmtpClient(configToUse.SmtpServer, configToUse.SmtpPort))
                {
                    client.Credentials = new NetworkCredential(configToUse.Username, configToUse.Password);
                    client.EnableSsl = configToUse.UseSsl;
                    client.Timeout = 10000;

                    // Crear mensaje de prueba
                    var mensaje = new MailMessage(
                        configToUse.SenderEmail,
                        configToUse.SenderEmail, // Enviarse a sí mismo
                        "Prueba de Conexión - Sistema de Eventos",
                        "✅ Esta es una prueba de conexión SMTP exitosa.\n\n" +
                        $"Servidor: {configToUse.SmtpServer}:{configToUse.SmtpPort}\n" +
                        $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}"
                    );

                    await client.SendMailAsync(mensaje);
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new EmailException($"❌ Error en conexión SMTP: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene consejos para configurar diferentes proveedores de email
        /// </summary>
        public string ObtenerConsejosConfiguracion(string proveedor)
        {
            switch (proveedor.ToLower())
            {
                case "gmail":
                    return @"🔧 CONFIGURACIÓN GMAIL:
• Servidor SMTP: smtp.gmail.com
• Puerto: 587 (con SSL) o 465 (con SSL)
• Usuario: tuemail@gmail.com
• Contraseña: Usa 'CONTRASEÑA DE APLICACIÓN' (recomendado)
• Cómo obtener contraseña de aplicación:
  1. Ve a https://myaccount.google.com/security
  2. Activa 'Verificación en 2 pasos'
  3. Ve a 'Contraseñas de aplicación'
  4. Genera una nueva para 'Correo'
  5. Usa esa contraseña aquí";

                case "outlook":
                case "hotmail":
                    return @"🔧 CONFIGURACIÓN OUTLOOK/HOTMAIL:
• Servidor SMTP: smtp-mail.outlook.com
• Puerto: 587 (con STARTTLS)
• Usuario: tuemail@outlook.com
• Contraseña: Tu contraseña normal
• Nota: Asegúrate de que SMTP esté habilitado";

                case "office365":
                    return @"🔧 CONFIGURACIÓN OFFICE 365:
• Servidor SMTP: smtp.office365.com
• Puerto: 587 (con STARTTLS)
• Usuario: tuemail@empresa.com
• Contraseña: Tu contraseña de Office 365
• Autenticación requerida: Sí";

                default:
                    return @"🔧 CONFIGURACIÓN GENERAL:
• Puerto 25: Usualmente bloqueado por ISPs
• Puerto 587: Recomendado (con STARTTLS)
• Puerto 465: Para SSL/TLS
• Asegúrate de que tu proveedor permita SMTP
• Verifica que el firewall no bloquee el puerto";
            }
        }
    }

    // ============================================
    // CLASES AUXILIARES
    // ============================================

    /// <summary>
    /// Configuración para el servidor SMTP
    /// </summary>
    public class EmailConfig
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public bool UseSsl { get; set; } = true;
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(SmtpServer) &&
                   SmtpPort > 0 &&
                   !string.IsNullOrWhiteSpace(SenderEmail) &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password);
        }
    }

    /// <summary>
    /// Resultado del envío masivo
    /// </summary>
    public class EmailBatchResult
    {
        public int TotalInvitados { get; set; }
        public int InvitadosValidos { get; set; }
        public int EnviadosExitosos { get; set; }
        public int EnviadosFallidos { get; set; }
        public List<EmailError> Errores { get; set; } = new List<EmailError>();
        public string ErrorGeneral { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duracion { get; set; }
    }

    /// <summary>
    /// Error específico de envío
    /// </summary>
    public class EmailError
    {
        public Invitado Invitado { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Información de progreso
    /// </summary>
    public class EmailProgress
    {
        public int Current { get; set; }
        public int Total { get; set; }
        public string CurrentInvited { get; set; }
        public string Status { get; set; }
        public bool Success { get; set; }

        public int Porcentaje => Total > 0 ? (int)(Current * 100.0 / Total) : 0;
    }

    /// <summary>
    /// Excepción personalizada para errores de email
    /// </summary>
    public class EmailException : Exception
    {
        public EmailException(string message) : base(message) { }
        public EmailException(string message, Exception innerException) : base(message, innerException) { }
    }
}