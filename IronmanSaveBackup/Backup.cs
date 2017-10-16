using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Xaml.Schema;
using IronmanSaveBackup.Properties;

namespace IronmanSaveBackup
{
    class Backup
    {
        public string Name { get; set; }
        public int Version { get; set; }
        public String Created { get; set; }
        public string BackupLocation { get; set; }
        public int Campaign { get; set; }
        public string RestoreName { get; set; }
        public bool BackupActive { get; set; }
        public Backup()
        {
            this.BackupLocation = Settings.Default.BackupLocation+$@"/{Version}/";
            this.Campaign = Settings.Default.MostRecentCampaign;
            this.Version = Settings.Default.MostRecentVersion;
            this.Created = DateTime.Now.ToString("yyyy MMMM dd");
            this.Name = $@"save_IRONMAN_Campaign_{Campaign}-{Created}-{Version}.isb";
            this.RestoreName = $@"save_IRONMAN_Campaign_{Campaign}";
            this.BackupActive = false;
        }

        public void RestoreBackup(string filePath)
        {
            
        }
    }
}
