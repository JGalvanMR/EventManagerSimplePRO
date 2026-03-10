using EventManagerSimple;
using System;
using System.Windows.Forms;

namespace EventManager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Crear carpeta de datos si no existe
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EventManager");
            Directory.CreateDirectory(appData);

            Application.Run(new Form1());
        }
    }
}