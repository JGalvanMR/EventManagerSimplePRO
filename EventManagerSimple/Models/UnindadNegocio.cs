using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManager.Models
{
    public class UnidadNegocio
    {
        public string ?Nombre { get; set; }
        public List<Invitado> Invitados { get; set; } = new List<Invitado>();
        public int TotalInvitados => Invitados?.Count ?? 0;
        public int TotalBoletos => Invitados?.Sum(i => i.Boletos) ?? 0;
    }
}
