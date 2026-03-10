using EventManagerSimple.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagerSimple
{
    public class EmailGenerator
    {
        public string GenerarInvitacionHTML(Invitado invitado, EventoConfig config)
        {
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
</head>
<body style=""margin: 0; padding: 0; background-color: #f4f4f4; font-family: 'Segoe UI', Arial, sans-serif;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" bgcolor=""#f4f4f4"">
        <tr>
            <td align=""center"" style=""padding: 20px;"">
                <table role=""presentation"" width=""650"" cellspacing=""0"" cellpadding=""0"" border=""0"" bgcolor=""#ffffff""
                    style=""border-radius: 12px; box-shadow: 0px 8px 20px rgba(0,0,0,0.1); border: 1px solid #e0e0e0; overflow: hidden;"">
                    <!-- Encabezado con gradiente -->
                    <tr>
                        <td align=""center""
                            style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; color: white;"">
                            <h1 style=""margin: 0; font-size: 28px; font-weight: 700; letter-spacing: 1px;"">
                                🎉 ¡ESTÁS INVITADO! 🎉
                            </h1>
                            <p style=""margin: 10px 0 0; font-size: 16px; opacity: 0.9;"">
                                Evento Exclusivo
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Información del evento -->
                    <tr>
                        <td style=""padding: 30px 40px;"">
                            <h2 style=""color: #333; font-size: 24px; margin: 0 0 20px; text-align: center;"">
                                {config.NombreEvento}
                            </h2>
                            
                            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0""
                                style=""margin: 25px 0;"">
                                <tr>
                                    <td style=""padding: 10px 0; border-bottom: 1px solid #eee;"">
                                        <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"">
                                            <tr>
                                                <td width=""40"" style=""vertical-align: top;"">
                                                    <div style=""background: #f0f7ff; width: 36px; height: 36px; border-radius: 50%; display: flex; align-items: center; justify-content: center;"">
                                                        👤
                                                    </div>
                                                </td>
                                                <td style=""padding-left: 15px;"">
                                                    <strong style=""color: #555;"">Invitado:</strong><br>
                                                    <span style=""font-size: 18px; color: #222; font-weight: 600;"">{invitado.Nombre}</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                
                                <tr>
                                    <td style=""padding: 15px 0;"">
                                        <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"">
                                            <tr>
                                                <td width=""40"" style=""vertical-align: top;"">
                                                    <div style=""background: #fff0f0; width: 36px; height: 36px; border-radius: 50%; display: flex; align-items: center; justify-content: center;"">
                                                        📅
                                                    </div>
                                                </td>
                                                <td style=""padding-left: 15px;"">
                                                    <strong style=""color: #555;"">Fecha:</strong><br>
                                                    <span style=""font-size: 16px; color: #222;"">{fechaFormateada}</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                
                                <tr>
                                    <td style=""padding: 15px 0;"">
                                        <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"">
                                            <tr>
                                                <td width=""40"" style=""vertical-align: top;"">
                                                    <div style=""background: #f0fff4; width: 36px; height: 36px; border-radius: 50%; display: flex; align-items: center; justify-content: center;"">
                                                        ⏰
                                                    </div>
                                                </td>
                                                <td style=""padding-left: 15px;"">
                                                    <strong style=""color: #555;"">Hora:</strong><br>
                                                    <span style=""font-size: 16px; color: #222;"">19:00 horas</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                
                                <tr>
                                    <td style=""padding: 15px 0 0;"">
                                        <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"">
                                            <tr>
                                                <td width=""40"" style=""vertical-align: top;"">
                                                    <div style=""background: #fff7f0; width: 36px; height: 36px; border-radius: 50%; display: flex; align-items: center; justify-content: center;"">
                                                        📍
                                                    </div>
                                                </td>
                                                <td style=""padding-left: 15px;"">
                                                    <strong style=""color: #555;"">Lugar:</strong><br>
                                                    <span style=""font-size: 16px; color: #222;"">{config.Lugar}</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <!-- Código QR -->
                    <tr>
                        <td align=""center"" style=""padding: 0 40px 30px;"">
                            <div style=""background: #f8f9fa; padding: 25px; border-radius: 10px; border: 2px dashed #dee2e6;"">
                                <h3 style=""margin: 0 0 15px; color: #444; font-size: 18px;"">
                                    🎫 Tu Código de Acceso
                                </h3>
                                <img src='data:image/png;base64,{qrBase64}' 
                                     alt='Código QR' 
                                     width='180' 
                                     height='180'
                                     style=""display: block; margin: 0 auto; border: 1px solid #ddd; border-radius: 8px; padding: 5px; background: white;"">
                                <p style=""margin: 15px 0 0; color: #666; font-size: 14px;"">
                                    Presenta este código QR en la entrada del evento
                                </p>
                            </div>
                        </td>
                    </tr>
                    
                    <!-- Boletos para la rifa -->
                    <tr>
                        <td style=""padding: 0 40px 30px;"">
                            <div style=""background: linear-gradient(135deg, #fdfcfb 0%, #e2d1c3 100%); padding: 25px; border-radius: 10px; border-left: 4px solid #28a745;"">
                                <h3 style=""margin: 0 0 15px; color: #155724; font-size: 20px; display: flex; align-items: center;"">
                                    🍀 Boletos para la Rifa
                                    <span style=""margin-left: auto; background: #28a745; color: white; padding: 4px 12px; border-radius: 20px; font-size: 14px;"">
                                        {invitado.Boletos} boletos
                                    </span>
                                </h3>
                                
                                <div style=""background: white; padding: 15px; border-radius: 8px; margin-top: 10px;"">
                                    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0""
                                        style=""font-family: 'Segoe UI', Arial, sans-serif; font-size: 14px; border-collapse: collapse;"">
                                        <tr style=""background-color: #28a745; color: white; font-weight: bold;"">
                                            <td style=""padding: 10px; border: 1px solid white; text-align: center;"">#</td>
                                            <td style=""padding: 10px; border: 1px solid white; text-align: center;"">Número de Boleto</td>
                                            <td style=""padding: 10px; border: 1px solid white; text-align: center;"">Estado</td>
                                        </tr>
                                        {GenerarFilasBoletos(invitado.NumerosBoletos)}
                                    </table>
                                </div>
                                
                                <p style=""margin: 15px 0 0; color: #155724; font-size: 15px; text-align: center;"">
                                    <strong>¡Mucha suerte en la rifa! 🎊</strong>
                                </p>
                            </div>
                        </td>
                    </tr>
                    
                    <!-- Información adicional -->
                    <tr>
                        <td style=""padding: 0 40px 30px;"">
                            <div style=""background: #e8f4fc; padding: 20px; border-radius: 8px; border-left: 4px solid #0a85ea;"">
                                <h4 style=""margin: 0 0 10px; color: #004085;"">📋 Información Importante:</h4>
                                <ul style=""margin: 0; padding-left: 20px; color: #555;"">
                                    <li style=""margin-bottom: 8px;"">Llegar 15 minutos antes del inicio del evento</li>
                                    <li style=""margin-bottom: 8px;"">Presentar identificación oficial</li>
                                    <li style=""margin-bottom: 8px;"">El código QR es personal e intransferible</li>
                                    <li>Vestimenta: Formal</li>
                                </ul>
                            </div>
                        </td>
                    </tr>
                    
                    <!-- Mensaje de despedida -->
                    <tr>
                        <td style=""padding: 0 40px 30px;"">
                            <p style=""margin: 0; color: #333; font-size: 16px; text-align: center; line-height: 1.6;"">
                                Estamos emocionados de contar con tu presencia en este evento especial.<br>
                                Será una noche inolvidable llena de sorpresas y momentos memorables.
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Firma -->
                    <tr>
                        <td style=""padding: 0 40px 30px; border-top: 1px solid #eee;"">
                            <p style=""margin: 20px 0 0; color: #666; font-size: 16px; text-align: center;"">
                                Atentamente,<br>
                                <strong style=""color: #333; font-size: 18px;"">Comité Organizador</strong><br>
                                <span style=""font-size: 14px;"">{config.NombreEvento}</span>
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Pie de página -->
                    <tr>
                        <td style=""padding: 20px 30px; background: #2c3e50; color: #bdc3c7; font-size: 12px; text-align: center;"">
                            <p style=""margin: 0 0 10px;"">
                                Este correo es una notificación automática de invitación al evento.<br>
                                Para cualquier duda o aclaración, contactar al comité organizador.
                            </p>
                            <p style=""margin: 0;"">
                                © {DateTime.Now.Year} {config.NombreEvento} - Todos los derechos reservados
                            </p>
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
                string colorFondo = (i % 2 == 0) ? "#f8f9fa" : "#ffffff";
                filas.Append($@"
                <tr style=""background-color: {colorFondo};"">
                    <td style=""padding: 8px; border: 1px solid #dee2e6; text-align: center; font-weight: bold;"">{i + 1}</td>
                    <td style=""padding: 8px; border: 1px solid #dee2e6; text-align: center;"">{numerosBoletos[i]}</td>
                    <td style=""padding: 8px; border: 1px solid #dee2e6; text-align: center;"">
                        <span style=""background: #d4edda; color: #155724; padding: 4px 8px; border-radius: 4px; font-size: 12px;"">
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
            string safeFileName = System.IO.Path.GetInvalidFileNameChars()
                .Aggregate(invitado.Nombre, (current, c) => current.Replace(c, '_'));
            string fileName = System.IO.Path.Combine(folderPath, $"Invitacion_{safeFileName}.html");
            System.IO.File.WriteAllText(fileName, html, Encoding.UTF8);
        }
    }
}