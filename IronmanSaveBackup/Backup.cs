using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xaml.Schema;
using IronmanSaveBackup.Properties;

namespace IronmanSaveBackup
{
    internal class Backup
    {
        public string BackupParentFolder { get; set; }
        public string SaveParentFolder { get; set; }
        public string RestoreFile { get; set; }
        public int MaxBackups { get; set; }
        public int BackupInterval { get; set; }
        public int Campaign { get; set; }
        public string RestoreName { get; set; }
        private bool _backupActive { get; set; }
        public bool BackupActive
        {
            get { return _backupActive; }
            set
            {
                _backupActive = value; 
                if (_backupActive)
                {
                    StartBackup();
                }
            }
        }

        public bool EventDrivenBackups { get; set; }
        public DateTime LastUpdated { get; set; }

        ~Backup()
        {
            UpdateSettings();
        }

        public void UpdateSettings()
        {
            Settings.Default.BackupParentFolder = BackupParentFolder;
            Settings.Default.SaveInterval = BackupInterval;
            Settings.Default.SaveParentFolder = SaveParentFolder;
            Settings.Default.MaxBackups = MaxBackups;
            Settings.Default.EnableOnEventBackup = EventDrivenBackups;
            Settings.Default.LastUpdated = LastUpdated;
            Settings.Default.MostRecentCampaign = Campaign;
            Settings.Default.Save();
        }

        public void RestoreBackup()
        {
            this.RestoreName = BuildRestoreName();
            var restorePath = Path.Combine(this.SaveParentFolder, this.RestoreName);
            if (this.Campaign == -1)
            {
                MessageOperations.UserMessage(
                    "Could not determine the Campaign that this backup belongs to. Backup was not restored.",
                    MessageOperations.MessageTypeEnum.RestoreError);
            }
            try
            {
                File.Copy(this.RestoreFile, restorePath, true);
                MessageOperations.UserMessage($"Succesfully restored save for Campaign {this.Campaign}",
                    MessageOperations.MessageTypeEnum.RestoreSuccess);
            }
            catch (IOException)
            {
                MessageOperations.UserMessage(
                    "Could not restore the selected backup because the Ironman Save and/or the Backup Save is in use.",
                    MessageOperations.MessageTypeEnum.FileInUseError);
            }
        }

        public DateTime ForceCreateBackup()
        {
            //Grabs the most recently updated IronMan save in the save folder
            var regex = new Regex(@"(^save_IRONMAN- Campaign .*$)");
            var saveDirectoryInfo = new DirectoryInfo(this.SaveParentFolder);
            var files = saveDirectoryInfo.GetFiles().OrderByDescending(x => x.LastAccessTime).ToList();
            var fileName = files.FirstOrDefault(x => regex.IsMatch(Path.GetFileName(x.FullName)));

            if (fileName != null)
            {
                this.Campaign = GetCampaignFromFileName(fileName.Name);
                if (this.Campaign == -1)
                {
                    MessageOperations.UserMessage("Could not determine the Campaign for the Ironman Saves Found.",
                        MessageOperations.MessageTypeEnum.BackupError);
                    return DateTime.Now;
                }

                //Create our backup directory and filenames
                var backupFileName = BuildBackupName();
                var backupChildDirectory = BuildBackupLocation();
                var backupFullName = Path.Combine(backupChildDirectory, backupFileName);

                try
                {
                    fileName.LastWriteTime = DateTime.Now;
                    File.Copy(fileName.FullName, backupFullName, false);

                    //Only delete additional backups if the new backup was copied sucesfully
                    if (this.MaxBackups > 0)
                    {
                        DeleteAdditionalBackups(backupChildDirectory);
                    }
                    MessageOperations.UserMessage($"Succesfully created backup for Campaign {this.Campaign}",
                        MessageOperations.MessageTypeEnum.BackupSuccess);
                    return DateTime.Now;
                }
                catch (Exception)
                {
                    MessageOperations.UserMessage("", MessageOperations.MessageTypeEnum.FileInUseError);
                    return DateTime.Now;
                }
            }
            MessageOperations.UserMessage("No Ironman Saves found in the selected location.",
                MessageOperations.MessageTypeEnum.BackupError);
            return DateTime.Now;
        }

        private void DeleteAdditionalBackups(string backupChildDirectory)
        {
            var backupChildInfo = new DirectoryInfo(backupChildDirectory);
            var backupFiles = backupChildInfo.GetFiles().OrderBy(x => x.CreationTime).ToList();
            var numToDelete = backupFiles.Count() - this.MaxBackups;
            if (numToDelete <= 0) return;
            foreach (var file in backupFiles.Take(numToDelete).ToList())
            {
                File.Delete(file.FullName);
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

        private int GetCampaignFromBackup()
        {
            var idString = Directory.GetParent(this.RestoreFile).Name;
            int idValue;
            if (int.TryParse(idString, out idValue))
            {
                return idValue;
            }
            return -1;
        }

        private string BuildRestoreName()
        {
            this.Campaign = GetCampaignFromBackup();
            return $@"save_IRONMAN- Campaign {this.Campaign}";
        }

        private string BuildBackupLocation()
        {
            var childDirectory = Path.Combine(this.BackupParentFolder, this.Campaign.ToString());

            if (Directory.Exists(childDirectory))
            {
                return childDirectory;
            }

            Directory.CreateDirectory(childDirectory);
            return childDirectory;
        }

        private string BuildBackupName()
        {
            return $@"save_IRONMAN-Campaign {this.Campaign} - {DateTime.UtcNow.Ticks}.isb";
        }

        private async void StartBackup()
        {
            if (EventDrivenBackups)
            {
                await EventBackup();
            }
            await IntervalBackup();
        }


        private async Task IntervalBackup()
        {
            TimeSpan interval;
            while (this.BackupActive)
            {
                interval = TimeSpan.FromMinutes(this.BackupInterval);
                this.LastUpdated = CreateBackup();
                await Task.Delay(interval);
            }
        }

        private Task EventBackup()
        {
            var tcs = new TaskCompletionSource<bool>();
            var watcher = new FileSystemWatcher
            {
                Path = SaveParentFolder,
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.Size | NotifyFilters.Attributes |
                               NotifyFilters.LastAccess | NotifyFilters.FileName | NotifyFilters.Size,
                Filter = "save_IRONMAN"
            };

            watcher.Renamed += new RenamedEventHandler(OnRename);
            watcher.EnableRaisingEvents = true;
            return tcs.Task;

        }

        private void OnRename(object sender, RenamedEventArgs e)
        {
                Thread.Sleep(200);
                this.LastUpdated = CreateBackup();
        }

        private DateTime CreateBackup()
        {
            var regex = new Regex(@"(^save_IRONMAN- Campaign .*$)");
            var saveDirectoryInfo = new DirectoryInfo(this.SaveParentFolder);
            var files = saveDirectoryInfo.GetFiles().OrderByDescending(x => x.LastAccessTime).ToList();
            var fileName = files.FirstOrDefault(x => regex.IsMatch(Path.GetFileName(x.FullName)));

            if (fileName == null)
            {
                MessageOperations.UserMessage("No Ironman Saves Found. Backup Operation Stopped.",
                    MessageOperations.MessageTypeEnum.BackupError);
                this.BackupActive = false;
                return DateTime.Now;
            }
            this.Campaign = GetCampaignFromFileName(fileName.Name);
            if (this.Campaign == -1)
            {
                MessageOperations.UserMessage(
                    "Could not determine the Campaign for the Ironman Saves Found. Backup operation stopped.",
                    MessageOperations.MessageTypeEnum.BackupError);
                this.BackupActive = false;
                return DateTime.Now;
            }

            //Create our backup directory and filenames
            var backupFileName = BuildBackupName();
            var backupChildDirectory = BuildBackupLocation();
            var backupFullName = Path.Combine(backupChildDirectory, backupFileName);

            try
            {
                fileName.LastWriteTime = DateTime.Now;
                File.Copy(fileName.FullName, backupFullName, false);

                //Only delete additional backups if the new backup was copied sucesfully
                if (this.MaxBackups > 0)
                {
                    DeleteAdditionalBackups(backupChildDirectory);
                }
                return DateTime.Now;
            }
            catch (Exception)
            {
                MessageOperations.UserMessage("Exception occured. Backup operation stopped.",
                    MessageOperations.MessageTypeEnum.BackupError);
                this.BackupActive = false;
                return DateTime.Now;
            }
        }
    }
}
