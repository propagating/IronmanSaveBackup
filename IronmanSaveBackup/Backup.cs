using IronmanSaveBackup.Properties;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using IronmanSaveBackup.Enums;

namespace IronmanSaveBackup
{
    public class Backup
    {
        public string BackupParentFolder { get; set; }
        private string _saveParentFolder;

        public string SaveParentFolder
        {
            get => _saveParentFolder;
            set
            {
                _saveParentFolder = value;

                if (_saveParentFolder.Contains("War of the Chosen"))
                {
                    this.SavePattern = new Regex(@"^save_IRONMAN- Campaign .*$");
                    this._saveType = SaveTypeEnum.WotC;
                }
                else
                {
                    this.SavePattern = new Regex(@"^save.*$");
                    this._saveType = SaveTypeEnum.Original;
                }
            }

        }
        public string RestoreFile { get; set; }
        public int MaxBackups { get; set; }
        public int BackupInterval { get; set; }
        public int Campaign { get; set; }
        public string RestoreName { get; set; }
        private SaveTypeEnum _saveType { get; set; }
        public CancellationTokenSource CancelBackupSource { get; set; }
        private bool _backupActive;

        private Regex SavePattern { get; set; }

        public bool BackupActive
        {
            get => _backupActive;
            set
            {
                _backupActive = value; 
                if (_backupActive)
                {
                    StartBackup();
                }
            }
        }

        private DateTime? _lastUpdated;

        public bool EventDrivenBackups { get; set; }

        public DateTime? LastUpdated
        {
            get => _lastUpdated;
            set
            {
                _lastUpdated = value;
                MainWindow._MainWindow.RecentBackup = value == null ? "Backup Failed" : $"Campaign {this.Campaign} @ {value}";
            }
        }

        public Backup()
        {
            CancelBackupSource = new CancellationTokenSource();
        }

        ~Backup()
        {
            UpdateSettings();
        }

        public void UpdateSettings()
        {
            Settings.Default.BackupParentFolder  = BackupParentFolder;
            Settings.Default.SaveInterval        = BackupInterval;
            Settings.Default.SaveParentFolder    = SaveParentFolder;
            Settings.Default.MaxBackups          = MaxBackups;
            Settings.Default.EnableOnEventBackup = EventDrivenBackups;
            Settings.Default.LastUpdated         = LastUpdated ?? DateTime.MinValue;
            Settings.Default.MostRecentCampaign  = Campaign;
            Settings.Default.Save();
        }

        public void RestoreBackup()
        {
            this.RestoreName = BuildRestoreName();
            var restorePath = Path.Combine(this.SaveParentFolder, this.RestoreName);
            if (this.Campaign == -1)
            {
                MessageOperations.UserMessage(Resources.CampaignNotFound, MessageTypeEnum.RestoreError);
            }
            try
            {
                File.Copy(this.RestoreFile, restorePath, true);
                MessageOperations.UserMessage(string.Format(Resources.SaveRestoredSuccess, this.Campaign), MessageTypeEnum.RestoreSuccess);
            }
            catch (IOException)
            {
                MessageOperations.UserMessage(Resources.FileInUse, MessageTypeEnum.FileInUseError);
            }
        }

        public DateTime? ForceCreateBackup()
        {
            //Grabs the most recently updated IronMan save in the save folder
            var saveDirectoryInfo = new DirectoryInfo(this.SaveParentFolder);
            var files             = saveDirectoryInfo.GetFiles().OrderByDescending(x => x.LastAccessTime).ToList();
            var fileName          = files.FirstOrDefault(x => SavePattern.IsMatch(Path.GetFileName(x.FullName)));

            if (fileName != null)
            {
                this.Campaign = GetCampaignFromFileName(fileName.Name);
                if (this.Campaign == -1)
                {
                    MessageOperations.UserMessage(Resources.CampaignNotFound,
                        MessageTypeEnum.BackupError);
                    return null;
                }

                //Create our backup directory and file names
                var backupFileName       = BuildBackupName();
                var backupChildDirectory = BuildBackupLocation();
                var backupFullName       = Path.Combine(backupChildDirectory, backupFileName);

                try
                {
                    File.Copy(fileName.FullName, backupFullName, false);

                    //Only delete additional backups if the new backup was copied successfully
                    if (this.MaxBackups > 0)
                    {
                        DeleteAdditionalBackups(backupChildDirectory);
                    }
                    MessageOperations.UserMessage(
                        string.Format(Resources.BackupCreatedSuccess, this.Campaign),
                        MessageTypeEnum.BackupSuccess);
                    return DateTime.Now;
                }
                catch (Exception)
                {
                    MessageOperations.UserMessage("", MessageTypeEnum.FileInUseError);
                    return null;
                }
            }
            MessageOperations.UserMessage(Resources.NoIronmanSaves,
                MessageTypeEnum.BackupError);
            return null;
        }

        private void DeleteAdditionalBackups(string backupChildDirectory)
        {
            var backupChildInfo = new DirectoryInfo(backupChildDirectory);
            var backupFiles     = backupChildInfo.GetFiles().OrderBy(x => x.CreationTime).ToList();
            var numToDelete     = backupFiles.Count - this.MaxBackups;
            if (numToDelete <= 0) return;
            foreach (var file in backupFiles.Take(numToDelete).ToList())
            {
                File.Delete(file.FullName);
            }
        }

        private int GetCampaignFromFileName(string fileName)
        {
            Regex regex;
            regex = this._saveType == SaveTypeEnum.WotC ? new Regex(@"^save_IRONMAN- Campaign (.*)$") : new Regex(@"^save(.*)$");
            var match    = regex.Match(fileName);
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
            return string.Format(this._saveType == SaveTypeEnum.Original ? Resources.SaveRestoreNameVanilla : Resources.SaveRestoreNameWotC, this.Campaign);
        }

        private string BuildBackupLocation()
        {
            string childDirectory;

            if (this.SaveParentFolder.Contains("Enemy Unknown"))
            {
                childDirectory = Path.Combine(this.BackupParentFolder,"XEU", this.Campaign.ToString());
            }
            else if (this.SaveParentFolder.Contains("Enemy Within"))
            {
                childDirectory = Path.Combine(this.BackupParentFolder, "XEW", this.Campaign.ToString());
            }
            else if (this.SaveParentFolder.Contains("War of the Chosen"))
            {
                childDirectory = Path.Combine(this.BackupParentFolder, "WotC", this.Campaign.ToString());
            }
            else
            {
                childDirectory = Path.Combine(this.BackupParentFolder, "X2", this.Campaign.ToString());
            }

            if (Directory.Exists(childDirectory))
            {
                return childDirectory;
            }

            Directory.CreateDirectory(childDirectory);
            return childDirectory;
        }

        private static string BuildBackupName()
        {
            return $@"{DateTime.Now:yyyy-dd-MM-HH-mm-ss}.isb";
        }

        private async void StartBackup()
        {
            var cancellationToken = CancelBackupSource.Token;
            if (EventDrivenBackups)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Run(()=> EventBackup(), cancellationToken);
                }
            }
            else
            {
                var interval = TimeSpan.FromMinutes(this.BackupInterval);
                while (!cancellationToken.IsCancellationRequested)
                {
                    this.LastUpdated = CreateBackup();
                    try
                    {
                        await Task.Delay(interval, cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                        this.BackupActive = false;
                    }
                   
                }
            }
        }

        private void EventBackup()
        {
            using (var watcher = new FileSystemWatcher())
            {
                watcher.Path = SaveParentFolder;
                watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastAccess |
                                       NotifyFilters.LastWrite | NotifyFilters.Attributes | NotifyFilters.Size | NotifyFilters.LastWrite;

                watcher.Filter = this._saveType == SaveTypeEnum.WotC ? "save_IRONMAN*" : "save*";
                watcher.Changed += OnEvent;
                watcher.EnableRaisingEvents = true;
                while (this.BackupActive)
                {

                }
            }

        }

        private void OnEvent(object sender, FileSystemEventArgs e)
        {
            this.LastUpdated = CreateBackup();
        }

        private DateTime? CreateBackup()
        {
            var saveDirectoryInfo = new DirectoryInfo(this.SaveParentFolder);
            var files             = saveDirectoryInfo.GetFiles().OrderByDescending(x => x.LastAccessTime).ToList();
            var fileName          = files.FirstOrDefault(x => SavePattern.IsMatch(Path.GetFileName(x.FullName)));

            if (fileName == null)
            {
                MessageOperations.UserMessage(Resources.NoIronmanSaves,MessageTypeEnum.BackupError);
                ResetBackup();
                return null;
            }
            this.Campaign = GetCampaignFromFileName(fileName.Name);

            if (this.Campaign == -1)
            {
                MessageOperations.UserMessage(Resources.CampaignNotFound,MessageTypeEnum.BackupError);
                ResetBackup();
                return null;
            }

            //Create our backup directory and file names
            var backupFileName       = BuildBackupName();
            var backupChildDirectory = BuildBackupLocation();
            var backupFullName       = Path.Combine(backupChildDirectory, backupFileName);

            try
            {
                File.Copy(fileName.FullName, backupFullName, false);

                //Only delete additional backups if the new backup was copied successfully
                if (this.MaxBackups > 0)
                {
                    DeleteAdditionalBackups(backupChildDirectory);
                }
                return DateTime.Now;
            }
            catch (Exception)
            {
                MessageOperations.UserMessage(Resources.AutoBackupException,
                    MessageTypeEnum.BackupError);
               ResetBackup();
                return null;
            }
        }

        private void ResetBackup()
        {
            this.CancelBackupSource.Cancel();
            this.BackupActive                              = false;
            MainWindow._MainWindow.SaveTextbox.IsEnabled   = true;
            MainWindow._MainWindow.BackupTextbox.IsEnabled = true;
            MainWindow._MainWindow.StartButton.IsEnabled   = true;
            MainWindow._MainWindow.StopButton.IsEnabled    = false;
        }

    }
}
