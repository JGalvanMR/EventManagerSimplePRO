using System;
using System.Collections.Generic;

namespace EventManager.Models
{
    public class Invitado
    {
        public int ID { get; set; }
        public string ?Nombre { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string ?Correo { get; set; }
        public string ?UnidadNegocio { get; set; }

        // Propiedades calculadas
        public int AniosServicio
        {
            get
            {
                DateTime today = DateTime.Today;
                int years = today.Year - FechaIngreso.Year;

                // Ajustar si el cumpleaños aún no ha llegado este año
                if (FechaIngreso.Date > today.AddYears(-years))
                    years--;

                return Math.Max(0, years);
            }
        }

        public int Boletos
        {
            get
            {
                return AniosServicio switch
                {
                    >= 1 and <= 5 => 1,
                    >= 6 and <= 10 => 2,
                    >= 11 => 3,
                    _ => 0
                };
            }
        }

        public List<string> NumerosBoletos { get; set; } = new List<string>();
        public byte[] ?QRCode { get; set; }
        public string ?QRCodePath { get; set; }

        // Para la UI
        public bool Seleccionado { get; set; }

        public bool InvitacionEnviada { get; set; }
        public DateTime? InvitacionEnviadaFecha { get; set; }
        public string CodigoVerificacion { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    }
}

