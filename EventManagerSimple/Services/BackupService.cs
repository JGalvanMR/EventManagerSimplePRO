using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManager.Services
{
    public class BackupService
    {
        public void CrearBackup(string sourceFolder, string backupFolder)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupPath = Path.Combine(backupFolder, $"backup_{timestamp}");

            Directory.CreateDirectory(backupPath);

            foreach (string file in Directory.GetFiles(sourceFolder))
            {
                string destFile = Path.Combine(backupPath, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            // Mantener solo los últimos 5 backups
            var backups = Directory.GetDirectories(backupFolder)
                .OrderByDescending(d => d)
                .Skip(5)
                .ToList();

            foreach (var oldBackup in backups)
            {
                Directory.Delete(oldBackup, true);
            }
        }
    }
}
