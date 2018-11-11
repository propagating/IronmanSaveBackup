using IronmanSaveBackup.Properties;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

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
        public CancellationTokenSource CancelBackupSource { get; set; }
        private bool _backupActive;

        private Regex SavePattern { get; }

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
            SavePattern = new Regex(@"^save_IRONMAN- Campaign .*$");
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
                MessageOperations.UserMessage(
                    Resources.CampaignNotFound,
                    MessageOperations.MessageTypeEnum.RestoreError);
            }
            try
            {
                File.Copy(this.RestoreFile, restorePath, true);
                MessageOperations.UserMessage(
                    string.Format(Resources.SaveRestoredSuccess, this.Campaign),
                    MessageOperations.MessageTypeEnum.RestoreSuccess);
            }
            catch (IOException)
            {
                MessageOperations.UserMessage(
                    Resources.FileInUse,
                    MessageOperations.MessageTypeEnum.FileInUseError);
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
                        MessageOperations.MessageTypeEnum.BackupError);
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
                        MessageOperations.MessageTypeEnum.BackupSuccess);
                    return DateTime.Now;
                }
                catch (Exception)
                {
                    MessageOperations.UserMessage("", MessageOperations.MessageTypeEnum.FileInUseError);
                    return null;
                }
            }
            MessageOperations.UserMessage(Resources.NoIronmanSaves,
                MessageOperations.MessageTypeEnum.BackupError);
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

        private static int GetCampaignFromFileName(string fileName)
        {
            var regex    = new Regex(@"^save_IRONMAN- Campaign (.*)$");
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
            return string.Format(Resources.SaveRestoreName, this.Campaign);
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

                watcher.Filter = "save_IRONMAN*";
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
                MessageOperations.UserMessage(Resources.NoIronmanSaves,
                    MessageOperations.MessageTypeEnum.BackupError);
                ResetBackup();
                return null;
            }
            this.Campaign = GetCampaignFromFileName(fileName.Name);
            if (this.Campaign == -1)
            {
                MessageOperations.UserMessage(
                    Resources.CampaignNotFound,
                    MessageOperations.MessageTypeEnum.BackupError);
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
                    MessageOperations.MessageTypeEnum.BackupError);
               ResetBackup();
                return null;
            }
        }

        private void ResetBackup()
        {
            this.CancelBackupSource.Cancel();
            this.BackupActive                             = false;
            MainWindow._MainWindow.SaveTextbox.IsEnabled   = true;
            MainWindow._MainWindow.BackupTextbox.IsEnabled = true;
            MainWindow._MainWindow.StartButton.IsEnabled   = true;
            MainWindow._MainWindow.StopButton.IsEnabled    = false;
        }

    }
}
