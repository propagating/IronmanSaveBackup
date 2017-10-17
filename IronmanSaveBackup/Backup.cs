using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xaml.Schema;
using IronmanSaveBackup.Properties;

namespace IronmanSaveBackup
{
    class Backup
    {
        public string BackupName { get; set; }
        public string BackupLocation { get; set; }
        public string SaveLocation { get; set; }
        public int Campaign { get; set; }
        public string RestoreName { get; set; }
        public bool BackupActive { get; set; }
        public Backup()
        {
            this.BackupLocation = BuildBackupLocation(Settings.Default.BackupLocation,
                Settings.Default.MostRecentCampaign);
            this.SaveLocation = Settings.Default.SaveLocation;
            this.BackupName = $@"save_IRONMAN- Campaign {Campaign}-.isb";
            this.RestoreName = BuildRestoreName(this.Campaign);
            this.BackupActive = false;
        }

        public void RestoreBackup(string filePath, string restoreName)
        {
            var restorePath = SaveLocation + "\\" + restoreName;
            try
            {
                File.Copy(filePath, restorePath, true);
                MessageOperations.UserMessage(MessageOperations.MessageTypeEnum.RestoreSuccess);
            }
            catch (IOException)
            {
                MessageOperations.UserMessage(MessageOperations.MessageTypeEnum.FileInUseError);
            }
        }

        public DateTime CreateBackup(string saveParent, string backupParent)
        {
            var regex = new Regex(@"(^save_IRONMAN- Campaign .*$)");
            var directory = new DirectoryInfo(saveParent);
            var files = directory.GetFiles().OrderByDescending(x => x.LastAccessTime).ToList();
            var fileName = files.FirstOrDefault(x=> regex.IsMatch(Path.GetFileName(x.FullName)));
            if (fileName != null)
            {
                var campaignId = GetCampaignFromFileName(fileName.Name);
                if (campaignId == -1)
                {
                    MessageOperations.UserMessage(MessageOperations.MessageTypeEnum.BackupError);
                    return DateTime.MinValue;
                }

                var backupFileName = BuildBackupName(campaignId);
                var backupChildDirectory = BuildBackupLocation(backupParent, campaignId);
                var backupFullName = backupChildDirectory + "\\" + backupFileName;

                try
                {
                    fileName.LastWriteTime = DateTime.Now;
                    File.Copy(fileName.FullName, backupFullName,false);
                    MessageOperations.UserMessage(MessageOperations.MessageTypeEnum.BackupSuccess);
                    return DateTime.Now;
                }
                catch (Exception e)
                {
                    MessageOperations.UserMessage(MessageOperations.MessageTypeEnum.FileInUseError);
                    return DateTime.MinValue;
                }
            }
            else
            {
                MessageOperations.UserMessage(MessageOperations.MessageTypeEnum.DoesNotExistError);
                return DateTime.MinValue;
            }

        }

        private int GetCampaignFromFileName(string fileName)
        {
            var regex = new Regex(@"^save_IRONMAN- Campaign (.*)$");
            var match = regex.Match(fileName);
            var idString = match.Groups[1].Value;
            int idValue;
            if (int.TryParse(idString, out idValue))
            {
                Settings.Default.MostRecentCampaign = idValue;
                Settings.Default.Save();
                return idValue;
            }
            return -1;
        }

        public string BuildRestoreName(int campaign)
        {
            Settings.Default.MostRecentCampaign = campaign;
            Settings.Default.Save();
            return $@"save_IRONMAN- Campaign {campaign}";
        }

        public string BuildBackupLocation(string backuplocation, int campaign)
        {
            var childDirectory = backuplocation + "\\" + campaign + "\\";
            if (!Directory.Exists(childDirectory))
            {
                Directory.CreateDirectory(childDirectory);
            }
            return childDirectory;
        }

        public string BuildBackupName(int campaign)
        {
            var date = DateTime.UtcNow;
            return $@"save_IRONMAN-Campaign {campaign} - {date.Ticks}.isb";
        }
    }
}
