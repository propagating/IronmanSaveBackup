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
        public DateTime LastUpdated { get; set; }
        public Backup()
        {
            this.BackupLocation = BuildBackupLocation(Settings.Default.BackupLocation,
                Settings.Default.MostRecentCampaign);
            this.SaveLocation = Settings.Default.SaveLocation;
            this.BackupName = $@"save_IRONMAN- Campaign {Campaign}-.isb";
            this.RestoreName = BuildRestoreName(this.Campaign);
            this.BackupActive = false;
            LastUpdated = DateTime.Now;
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

        public DateTime CreateBackup(string saveParent, string backupParent, int maxBackups)
        {
            //Grabs the most recently updated IronMan save in the save folder
            var regex = new Regex(@"(^save_IRONMAN- Campaign .*$)");
            var saveDirectoryInfo = new DirectoryInfo(saveParent);
            var files = saveDirectoryInfo.GetFiles().OrderByDescending(x => x.LastAccessTime).ToList();
            var fileName = files.FirstOrDefault(x=> regex.IsMatch(Path.GetFileName(x.FullName)));

            if (fileName != null)
            {
                var campaignId = GetCampaignFromFileName(fileName.Name);
                if (campaignId == -1)
                {
                    MessageOperations.UserMessage(MessageOperations.MessageTypeEnum.IronmanSaveNotFoundError);
                    return DateTime.MinValue;
                }

                Settings.Default.MostRecentCampaign = campaignId;
                Settings.Default.Save();

                //Create our backup directory and filenames
                var backupFileName = BuildBackupName(campaignId);
                var backupChildDirectory = BuildBackupLocation(backupParent, campaignId);
                var backupFullName = backupChildDirectory + "\\" + backupFileName;

                try
                {
                    fileName.LastWriteTime = DateTime.Now;
                    File.Copy(fileName.FullName, backupFullName, false);

                    //Only delete additional backups if the new backup was copied sucesfully
                    if(maxBackups > 0) { DeleteAdditionalBackups(maxBackups, backupChildDirectory);}
                    MessageOperations.UserMessage(MessageOperations.MessageTypeEnum.BackupSuccess);
                    return DateTime.Now;
                }
                catch (Exception)
                {
                    MessageOperations.UserMessage(MessageOperations.MessageTypeEnum.FileInUseError);
                    return DateTime.MinValue;
                }
            }
            MessageOperations.UserMessage(MessageOperations.MessageTypeEnum.DoesNotExistError);
            return DateTime.MinValue;
        }

        private static void DeleteAdditionalBackups(int maxBackups, string backupChildDirectory)
        {
            var backupChildInfo = new DirectoryInfo(backupChildDirectory);
            var backupFiles = backupChildInfo.GetFiles().OrderBy(x => x.CreationTime).ToList();
            var numToDelete = backupFiles.Count() - maxBackups;
            if (numToDelete > 0)
            {
                foreach (var file in backupFiles.Take(numToDelete).ToList())
                {
                    File.Delete(file.FullName);
                }
            }

        }

        private static int GetCampaignFromFileName(string fileName)
        {
            var regex = new Regex(@"^save_IRONMAN- Campaign (.*)$");
            var match = regex.Match(fileName);
            var idString = match.Groups[1].Value;
            int idValue;
            if (int.TryParse(idString, out idValue))
            {
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

        private static string BuildBackupLocation(string backuplocation, int campaign)
        {
            var childDirectory = backuplocation + "\\" + campaign + "\\";

            if (!Directory.Exists(childDirectory))
            {
                Directory.CreateDirectory(childDirectory);
            }

            return childDirectory;
        }

        private static string BuildBackupName(int campaign)
        {
            var date = DateTime.UtcNow;
            return $@"save_IRONMAN-Campaign {campaign} - {date.Ticks}.isb";
        }

        public void StartBackup(bool isChecked, double intervalSliderValue, double value)
        {
            if (!isChecked)
            {
                while (this.BackupActive == true)
                {
                    
                }
            }
        }
    }
}
