using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManager.Models
{
    public class EventoConfig
    {
        public string NombreEvento { get; set; } = "Evento Empresarial 2024";
        public DateTime FechaEvento { get; set; } = DateTime.Now.AddDays(30);
        public string Lugar { get; set; } = "Salón La Gran Vía - DoubleTree by Hilton Celaya";
        public int Boletos1a5Anios { get; set; } = 1;
        public int Boletos6a10Anios { get; set; } = 2;
        public int Boletos11PlusAnios { get; set; } = 3;

        public void Guardar(string filePath)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static EventoConfig Cargar(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<EventoConfig>(json);
            }
            return new EventoConfig();
        }
    }
}
