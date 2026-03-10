using EventManager.Models;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EventManager.Services
{
    public class QRService
    {
        public byte[] GenerarQRCode(string data, int size = 300)
        {
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new PngByteQRCode(qrCodeData))
            {
                return qrCode.GetGraphic(20);
            }
        }

        public string GenerarDatosQR(Invitado invitado, EventoConfig config)
        {
            var datos = new
            {
                Evento = config.NombreEvento,
                invitado.ID,
                invitado.Nombre,
                Unidad = invitado.UnidadNegocio,
                Fecha = DateTime.Now.ToString("yyyyMMddHHmmss"),
                Hash = GenerarHashSeguro(invitado)
            };

            return JsonSerializer.Serialize(datos);
        }

        private string GenerarHashSeguro(Invitado invitado)
        {
            string data = $"{invitado.ID}|{invitado.Nombre}|{invitado.FechaIngreso:yyyyMMdd}|{DateTime.Now.Ticks}";
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes)[..16]; // Tomar primeros 16 caracteres
            }
        }

        public void GuardarQRComoImagen(byte[] qrBytes, string filePath)
        {
            File.WriteAllBytes(filePath, qrBytes);
        }

        public Image ByteArrayToImage(byte[] byteArray)
        {
            using (var ms = new MemoryStream(byteArray))
            {
                return Image.FromStream(ms);
            }
        }
    }

    public class TicketService
    {
        public List<string> GenerarNumerosBoletos(Invitado invitado, int cantidad)
        {
            var numeros = new List<string>();

            for (int i = 1; i <= cantidad; i++)
            {
                // Formato: EMP-ID-001-ABC123
                string numero = $"EMP-{invitado.ID:00000}-{i:000}-{GenerarCodigoAleatorio(6)}";
                numeros.Add(numero);
            }

            return numeros;
        }

        private string GenerarCodigoAleatorio(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Sin 0,O,I,1 para claridad
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public void ExportarBoletosCSV(List<UnidadNegocio> unidades, string filePath)
        {
            var lines = new List<string>();
            lines.Add("UNIDAD_NEGOCIO,ID_INVITADO,NOMBRE,BOLETO_NUMERO");

            foreach (var unidad in unidades)
            {
                foreach (var invitado in unidad.Invitados)
                {
                    foreach (var boleto in invitado.NumerosBoletos)
                    {
                        lines.Add($"\"{unidad.Nombre}\",{invitado.ID},\"{invitado.Nombre}\",{boleto}");
                    }
                }
            }

            File.WriteAllLines(filePath, lines, Encoding.UTF8);
        }
    }
}