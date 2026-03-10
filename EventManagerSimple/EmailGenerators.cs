using EventManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManager
{
    public class EmailGenerators
    {
        public string GenerarInvitacionHTML(Invitado invitado, EventoConfig config)
        {
            string logo = "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";
            string qrBase64 = Convert.ToBase64String(invitado.QRCode);
            string fechaFormateada = config.FechaEvento.ToString("dddd, dd 'de' MMMM 'de' yyyy",
                new System.Globalization.CultureInfo("es-MX"));

            return $@"
<!DOCTYPE html>
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <title>Invitación al Evento - {config.NombreEvento}</title>
    <!--[if mso]>
    <style>
        .header-table {{mso-padding-alt: 30px 0;}}
        .qr-container {{mso-padding-alt: 25px;}}
        .tickets-container {{mso-padding-alt: 25px;}}
        .info-container {{mso-padding-alt: 20px;}}
        .icon-cell {{mso-padding-right: 15px;}}
    </style>
    <![endif]-->
</head>
<body style=""margin: 0; padding: 0; background-color: #f4f4f4; font-family: 'Segoe UI', Arial, sans-serif; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" bgcolor=""#f4f4f4"" style=""min-width: 100%;"">
        <tr>
            <td align=""center"" style=""padding: 20px;"">
                <!-- CONTENEDOR PRINCIPAL -->
                <table role=""presentation"" width=""650"" cellspacing=""0"" cellpadding=""0"" border=""0"" bgcolor=""#ffffff""
                    style=""border-collapse: collapse; border: 1px solid #e0e0e0;"">
                    <!-- ENCABEZADO -->
                    <tr>
                        <td align=""center"" bgcolor=""#0055a5"" class=""header-table"" 
                            style=""padding: 30px 0; color: white; font-family: 'Segoe UI', Arial, sans-serif;"">
                            <table role=""presentation"" width=""90%"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                                <tr>
                                    <td align=""center"" style=""padding: 0;"">
                                        <h1 style=""margin: 0; font-size: 28px; font-weight: 700; letter-spacing: 1px; line-height: 1.2; color: #ffffff"">
                                            🎉 ¡ESTÁS INVITADO! 🎉
                                        </h1>
                                        <p style=""margin: 10px 0 0; font-size: 16px; color: #e0e0e0;"">
                                            Evento Exclusivo
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <!-- INFORMACIÓN DEL EVENTO -->
                    <tr>
                        <td style=""padding: 30px 40px;"" bgcolor=""#ffffff"">
                            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                                <tr>
                                    <td align=""center"" style=""padding: 0 0 20px;"">
                                        <h2 style=""color: #333333; font-size: 24px; margin: 0; font-weight: 600;"">
                                            {config.NombreEvento}
                                        </h2>
                                    </td>
                                </tr>
                            </table>
                            
                            <!-- DETALLES DEL EVENTO -->
                            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0""
                                style=""margin: 25px 0; border-collapse: collapse;"">
                                
                                <!-- INVITADO -->
                                <tr>
                                    <td style=""padding: 15px 0; border-bottom: 1px solid #eeeeee;"">
                                        <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                                            <tr>
                                                <td width=""50"" style=""vertical-align: top; padding-right: 15px;"" class=""icon-cell"">
                                                    <div style=""background: #f0f7ff; width: 40px; height: 40px; display: table-cell; vertical-align: middle; text-align: center;"">
                                                        <span style=""font-size: 18px;"">👤</span>
                                                    </div>
                                                </td>
                                                <td style=""vertical-align: middle;"">
                                                    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                                                        <tr>
                                                            <td style=""color: #555555; font-size: 14px; padding-bottom: 2px;"">
                                                                <strong>Invitado:</strong>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td style=""font-size: 18px; color: #222222; font-weight: 600;"">
                                                                {invitado.Nombre}
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                
                                <!-- FECHA -->
                                <tr>
                                    <td style=""padding: 15px 0;"">
                                        <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                                            <tr>
                                                <td width=""50"" style=""vertical-align: top; padding-right: 15px;"" class=""icon-cell"">
                                                    <div style=""background: #fff0f0; width: 40px; height: 40px; display: table-cell; vertical-align: middle; text-align: center;"">
                                                        <span style=""font-size: 18px;"">📅</span>
                                                    </div>
                                                </td>
                                                <td style=""vertical-align: middle;"">
                                                    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                                                        <tr>
                                                            <td style=""color: #555555; font-size: 14px; padding-bottom: 2px;"">
                                                                <strong>Fecha:</strong>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td style=""font-size: 16px; color: #222222;"">
                                                                {fechaFormateada}
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                
                                <!-- HORA -->
                                <tr>
                                    <td style=""padding: 15px 0;"">
                                        <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                                            <tr>
                                                <td width=""50"" style=""vertical-align: top; padding-right: 15px;"" class=""icon-cell"">
                                                    <div style=""background: #f0fff4; width: 40px; height: 40px; display: table-cell; vertical-align: middle; text-align: center;"">
                                                        <span style=""font-size: 18px;"">⏰</span>
                                                    </div>
                                                </td>
                                                <td style=""vertical-align: middle;"">
                                                    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                                                        <tr>
                                                            <td style=""color: #555555; font-size: 14px; padding-bottom: 2px;"">
                                                                <strong>Hora:</strong>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td style=""font-size: 16px; color: #222222;"">
                                                                15:00 horas
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                
                                <!-- LUGAR -->
                                <tr>
                                    <td style=""padding: 15px 0 0;"">
                                        <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                                            <tr>
                                                <td width=""50"" style=""vertical-align: top; padding-right: 15px;"" class=""icon-cell"">
                                                    <div style=""background: #fff7f0; width: 40px; height: 40px; display: table-cell; vertical-align: middle; text-align: center;"">
                                                        <span style=""font-size: 18px;"">📍</span>
                                                    </div>
                                                </td>
                                                <td style=""vertical-align: middle;"">
                                                    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                                                        <tr>
                                                            <td style=""color: #555555; font-size: 14px; padding-bottom: 2px;"">
                                                                <strong>Lugar:</strong>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td style=""font-size: 16px; color: #222222;"">
                                                                Salón La Gran Vía - DoubleTree by Hilton Celaya
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <!-- CÓDIGO QR -->
                    <tr>
                        <td align=""center"" style=""padding: 0 40px 30px;"">
                            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" 
                                bgcolor=""#f8f9fa"" class=""qr-container""
                                style=""padding: 25px; border: 2px dashed #dee2e6;"">
                                <tr>
                                    <td align=""center"" style=""padding: 0;"">
                                        <h3 style=""margin: 0 0 15px; color: #444444; font-size: 18px; font-weight: 600;"">
                                            🎫 Tu Código de Acceso
                                        </h3>
                                        <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                                            <tr>
                                                <td style=""padding: 0; background: white; border: 1px solid #dddddd;"">
                                                    <img src='data:image/png;base64,{qrBase64}' 
                                                         alt='Código QR' 
                                                         width='180' 
                                                         height='180'
                                                         style=""display: block; border: 0; outline: none;"">
                                                </td>
                                            </tr>
                                        </table>
                                        <p style=""margin: 15px 0 0; color: #666666; font-size: 14px; line-height: 1.5;"">
                                            Presenta este código QR en la entrada del evento
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <!-- BOLETOS PARA LA RIFA -->
                    <tr>
                        <td style=""padding: 0 40px 30px;"">
                            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" 
                                bgcolor=""#fdfcfb"" class=""tickets-container""
                                style=""padding: 25px; border-left: 4px solid #28a745;"">
                                <tr>
                                    <td style=""padding: 0;"">                                        
                                        <p style=""margin: 15px 0 0; color: #155724; font-size: 15px; text-align: center; font-weight: 600;"">
                                            🍀 ¡Mucha suerte en la rifa! 🎊
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <!-- INFORMACIÓN ADICIONAL -->
                    <tr>
                        <td style=""padding: 0 40px 30px;"">
                            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" 
                                bgcolor=""#e8f4fc"" class=""info-container""
                                style=""padding: 20px; border-left: 4px solid #0055a5;"">
                                <tr>
                                    <td style=""padding: 0;"">
                                        <h4 style=""margin: 0 0 10px; color: #0055a5; font-size: 16px; font-weight: 600;"">
                                            📋 Información Importante:
                                        </h4>
                                        <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                                            <tr>
                                                <td style=""padding: 5px 0; color: #555555;"">
                                                    • Llegar <b>puntual al evento</b>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding: 5px 0; color: #555555;"">
                                                    • Presentar <b>tu código QR</b>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding: 5px 0; color: #555555;"">
                                                    • <b>Nota:</b> El código QR es personal e intransferible
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <!-- MENSAJE DE DESPEDIDA -->
                    <tr>
                        <td style=""padding: 0 40px 30px;"">
                            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                                <tr>
                                    <td align=""center"" style=""padding: 0;"">
                                        <p
                                            style=""margin: 0; color: #111111; font-size: 16px; line-height: 1.6; text-align: center; font-weight: 600;"">
                                            ¡No faltes!
                                        </p>
                                    </td>
                                </tr>
<tr>
                                    <td align=""center"" style=""padding: 0;"">
                                        <p
                                            style=""margin: 0; color: #111111; font-size: 16px; line-height: 1.6; text-align: justify; font-weight: 600;"">
                                            Este año, en el evento de rifas, por cada 5 años de antigüedad tendrás derecho a un boleto adicional para el sorteo.
                                            ¡Los premios estarán increíbles! Viajes, pantallas, computadoras, tabletas, bocinas, Xbox y mucho más.
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <!-- FIRMA -->
                    <tr>
                        <td style=""padding: 0 40px 30px; border-top: 1px solid #eeeeee;"">
                            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                                <tr>
                                    <td align=""center"" style=""padding: 20px 0 0;"">
                                        <p style=""margin: 0; color: #666666; font-size: 16px; line-height: 1.5;"">
                                            Atentamente,<br>
                                            <strong style=""color: #333333; font-size: 18px;"">Comité Organizador</strong><br>
                                            <span style=""font-size: 14px;"">{config.NombreEvento}</span>
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <!-- PIE DE PÁGINA -->
                    <tr>
                        <td bgcolor=""#0055a5"" style=""padding: 20px 30px;"">
                            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                                <tr>
                                    <td align=""center"" style=""padding: 0;"">
                                        <p style=""margin: 0 0 10px; color: #bdc3c7; font-size: 12px; line-height: 1.5;"">
                                            Este correo es una notificación automática de invitación al evento.<br>
                                            Para cualquier duda o aclaración, contactar al comité organizador.
                                        </p>
                                        <p style=""margin: 0; color: #95a5a6; font-size: 11px;"">
                                            © {DateTime.Now.Year} {config.NombreEvento} - Todos los derechos reservados
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        private string GenerarFilasBoletos(List<string> numerosBoletos)
        {
            var filas = new StringBuilder();
            for (int i = 0; i < numerosBoletos.Count; i++)
            {
                string bgColor = i % 2 == 0 ? "#f8f9fa" : "#ffffff";
                filas.Append($@"
                <tr bgcolor=""{bgColor}"">
                    <td style=""padding: 8px; border: 1px solid #dee2e6; text-align: center; font-weight: bold;"">
                        {i + 1}
                    </td>
                    <td style=""padding: 8px; border: 1px solid #dee2e6; text-align: center;"">
                        {numerosBoletos[i]}
                    </td>
                    <td style=""padding: 8px; border: 1px solid #dee2e6; text-align: center;"">
                        <span style=""background: #d4edda; color: #155724; padding: 4px 8px; font-size: 12px; display: inline-block;"">
                            Activo
                        </span>
                    </td>
                </tr>");
            }
            return filas.ToString();
        }

        public void GuardarInvitacionComoArchivo(Invitado invitado, EventoConfig config, string folderPath)
        {
            string html = GenerarInvitacionHTML(invitado, config);
            string safeFileName = Path.GetInvalidFileNameChars()
                .Aggregate(invitado.Nombre, (current, c) => current.Replace(c, '_'));
            string fileName = Path.Combine(folderPath, $"Invitacion_{safeFileName}.html");
            File.WriteAllText(fileName, html, Encoding.UTF8);
        }
    }
}