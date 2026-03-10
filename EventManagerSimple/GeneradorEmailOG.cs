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

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Invitación al Evento</title>
    <style>
        body {{ font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #2c3e50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background: #f9f9f9; }}
        .qr {{ text-align: center; margin: 20px 0; }}
        .boletos {{ background: #ecf0f1; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .footer {{ text-align: center; color: #7f8c8d; font-size: 12px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🎉 ¡Estás Invitado! 🎉</h1>
    </div>
    
    <div class='content'>
        <p>Hola <strong>{invitado.Nombre}</strong>,</p>
        
        <p>Estás cordialmente invitado a nuestro:</p>
        <h2>{config.NombreEvento}</h2>
        
        <p><strong>📅 Fecha:</strong> {config.FechaEvento.ToString("dddd, dd MMMM yyyy")}</p>
        <p><strong>⏰ Hora:</strong> 19:00 horas</p>
        <p><strong>📍 Lugar:</strong> {config.Lugar}</p>
        
        <div class='qr'>
            <h3>Tu código de acceso:</h3>
            <img src='data:image/png;base64,{qrBase64}' alt='Código QR' width='200' height='200'/>
            <p>Presenta este código QR en la entrada del evento</p>
        </div>
        
        <div class='boletos'>
            <h3>🎫 Tus boletos para la rifa:</h3>
            <p><strong>Cantidad:</strong> {invitado.Boletos} boletos</p>
            <p><strong>Números:</strong><br>{string.Join("<br>", invitado.NumerosBoletos)}</p>
            <p>¡Suerte en la rifa! 🍀</p>
        </div>
        
        <p>Esperamos contar con tu presencia en este evento especial.</p>
        
        <p>Atentamente,<br>
        <strong>Comité Organizador</strong></p>
    </div>
    
    <div class='footer'>
        <p>Este correo fue generado automáticamente. Por favor no responder.</p>
        <p>Event Manager Simple - 2025</p>
    </div>
</body>
</html>";
        }

        public void GuardarInvitacionComoArchivo(Invitado invitado, EventoConfig config, string folderPath)
        {
            string html = GenerarInvitacionHTML(invitado, config);
            string fileName = Path.Combine(folderPath, $"Invitacion_{invitado.Nombre.Replace(" ", "_")}.html");
            File.WriteAllText(fileName, html, Encoding.UTF8);
        }
    }
}
