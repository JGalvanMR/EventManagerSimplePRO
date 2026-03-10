using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManager
{
    // Coloca esta clase DENTRO del namespace pero FUERA de la clase Form1
    public class QrResult
    {
        public string FileName { get; set; }
        public string DecodedData { get; set; }
        public string Timestamp { get; set; }
    }
}
