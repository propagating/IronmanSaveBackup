using IronmanSaveBackup.Properties;
using System;
using System.Globalization;
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
                    SavePattern = new Regex(@"^save_IRONMAN- Campaign .*$");
                    _saveType = SaveType.WotC;
                }
                else if (_saveParentFolder.Contains("Chimera Squad"))
                {
                    SavePattern = new Regex(@"^.*-IronMan$");
                    _saveType = SaveType.Chimera;
                }
                else
                {
                    SavePattern = new Regex(@"^save.*$");
                    _saveType = SaveType.Original;
                }
            }

        }
        public string RestoreFile { get; set; }
        public int MaxBackups { get; set; }
        public int BackupInterval { get; set; }
        public long Campaign { get; set; }
        public string RestoreName { get; set; }
        private SaveType _saveType { get; set; }
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
                MainWindow._MainWindow.RecentBackup = value == null ? "Backup Failed" : $"Campaign {Campaign} @ {value}";
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
            RestoreName = BuildRestoreName();
            var restorePath = Path.Combine(SaveParentFolder, RestoreName);
            if (Campaign == -1)
            {
                MessageOperations.UserMessage(Resources.CampaignNotFound, MessageType.RestoreError);
            }
            try
            {
                File.Copy(RestoreFile, restorePath, true);
                MessageOperations.UserMessage(string.Format(Resources.SaveRestoredSuccess, Campaign, CultureInfo.InvariantCulture), MessageType.RestoreSuccess);
            }
            catch (IOException)
            {
                MessageOperations.UserMessage(Resources.FileInUse, MessageType.FileInUseError);
            }
        }

        public DateTime? ForceCreateBackup()
        {
            //Grabs the most recently updated IronMan save in the save folder
            var saveDirectoryInfo = new DirectoryInfo(SaveParentFolder);
            var files         = saveDirectoryInfo.GetFiles().OrderByDescending(x => x.LastAccessTime).ToList();
            var fileName          = files.FirstOrDefault(x => SavePattern.IsMatch(Path.GetFileName(x.FullName)));

            if (fileName != null)
            {
                Campaign = GetCampaignFromFileName(fileName.Name);
                if (Campaign == -1)
                {
                    MessageOperations.UserMessage(Resources.CampaignNotFound,
                        MessageType.BackupError);
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
                    if (MaxBackups > 0)
                    {
                        DeleteAdditionalBackups(backupChildDirectory);
                    }
                    MessageOperations.UserMessage(
                        string.Format(Resources.BackupCreatedSuccess, Campaign, CultureInfo.InvariantCulture),
                        MessageType.BackupSuccess);
                    return DateTime.Now;
                }
                catch (IOException)
                {
                    MessageOperations.UserMessage("", MessageType.FileInUseError);
                    return null;
                }
            }
            MessageOperations.UserMessage(Resources.NoIronmanSaves,
                MessageType.BackupError);
            return null;
        }

        private void DeleteAdditionalBackups(string backupChildDirectory)
        {
            var backupChildInfo = new DirectoryInfo(backupChildDirectory);
            var backupFiles     = backupChildInfo.GetFiles().OrderBy(x => x.CreationTime).ToList();
            var numToDelete     = backupFiles.Count - MaxBackups;
            if (numToDelete <= 0) return;
            foreach (var file in backupFiles.Take(numToDelete).ToList())
            {
                File.Delete(file.FullName);
            }
        }

        private long GetCampaignFromFileName(string fileName)
        {
            Regex regex;
            switch (_saveType)
            {
                case SaveType.Original:
                    regex = new Regex(@"^save(.*)$");
                    break;
                case SaveType.WotC:
                    regex = new Regex(@"^save_IRONMAN- Campaign (.*)$");
                    break;
                case SaveType.Chimera:
                    regex = new Regex(@"^.*?(?=-IronMan)");
                    break;
                default:
                    return -1;
            } 
            
            var match    = regex.Match(fileName);
            var idString = _saveType == SaveType.Chimera ? match.Value : match.Groups[1].Value;
            long idValue;
            if (long.TryParse(idString, out idValue))
            {
                return idValue;
            }

            return -1;
        }

        private long GetCampaignFromBackup()
        {
            var idString = Directory.GetParent(RestoreFile).Name;
            long idValue;
            if (long.TryParse(idString, out idValue))
            {
                return idValue;
            }
            return -1;
        }

        private string BuildRestoreName()
        {
            Campaign = GetCampaignFromBackup();
            switch (_saveType)
            {
                case SaveType.Original:
                    return string.Format(Resources.SaveRestoreNameVanilla, Campaign, CultureInfo.InvariantCulture);
                case SaveType.WotC:
                    return string.Format(Resources.SaveRestoreNameWotC, Campaign, CultureInfo.InvariantCulture);
                case SaveType.Chimera:
                    return string.Format(Resources.SaveRestoreNameChimera, Campaign, CultureInfo.InvariantCulture);
                default:
                    return string.Format(Campaign.ToString(), CultureInfo.InvariantCulture);
            }
        }

        private string BuildBackupLocation()
        {
            string childDirectory;

            if (SaveParentFolder.Contains("Enemy Unknown"))
            {
                childDirectory = Path.Combine(BackupParentFolder,"XEU", Campaign.ToString());
            }
            else if (SaveParentFolder.Contains("Enemy Within"))
            {
                childDirectory = Path.Combine(BackupParentFolder, "XEW", Campaign.ToString());
            }
            else if (SaveParentFolder.Contains("War of the Chosen"))
            {
                childDirectory = Path.Combine(BackupParentFolder, "WotC", Campaign.ToString());
            }
            else if (SaveParentFolder.Contains("Chimera Squad"))
            {
                childDirectory = Path.Combine(BackupParentFolder, "Chimera Squad", Campaign.ToString());
            }
            else
            {
                childDirectory = Path.Combine(BackupParentFolder, "X2", Campaign.ToString());
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
                    await Task.Run(EventBackup, cancellationToken);
                }
            }
            else
            {
                var interval = TimeSpan.FromMinutes(BackupInterval);
                while (!cancellationToken.IsCancellationRequested)
                {
                    LastUpdated = CreateBackup();
                    try
                    {
                        await Task.Delay(interval, cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                        BackupActive = false;
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
                                       NotifyFilters.LastWrite | NotifyFilters.Attributes | NotifyFilters.Size;

                watcher.Filter = "";
                switch (_saveType)
                {
                    case SaveType.Original:
                        watcher.Filter = "save";
                        break;
                    case SaveType.WotC:
                        watcher.Filter = "save_IRONMAN*";
                        break;
                    case SaveType.Chimera:
                        watcher.Filter = "*IronMan*";
                        break;
                    default:
                        watcher.Filter = "*IronMan*";
                        break;
                }
                watcher.Changed += OnEvent;
                watcher.EnableRaisingEvents = true;
                while (BackupActive)
                {
                }
            }

        }

        private void OnEvent(object sender, FileSystemEventArgs e)
        {
            LastUpdated = CreateBackup();
        }

        private DateTime? CreateBackup()
        {
            var saveDirectoryInfo = new DirectoryInfo(SaveParentFolder);
            var files         = saveDirectoryInfo.GetFiles().OrderByDescending(x => x.LastAccessTime).ToList();
            var fileName          = files.FirstOrDefault(x => SavePattern.IsMatch(Path.GetFileName(x.FullName)));

            if (fileName == null)
            {
                MessageOperations.UserMessage(Resources.NoIronmanSaves,MessageType.BackupError);
                ResetBackup();
                return null;
            }
            Campaign = GetCampaignFromFileName(fileName.Name);

            if (Campaign == -1)
            {
                MessageOperations.UserMessage(Resources.CampaignNotFound,MessageType.BackupError);
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
                if (MaxBackups > 0)
                {
                    DeleteAdditionalBackups(backupChildDirectory);
                }
                return DateTime.Now;
            }
            catch (IOException)
            {
                MessageOperations.UserMessage(Resources.AutoBackupException,
                    MessageType.BackupError);
               ResetBackup();
                return null;
            }
        }

        private void ResetBackup()
        {
            CancelBackupSource.Cancel();
            BackupActive                                   = false;
            MainWindow._MainWindow.SaveTextbox.IsEnabled   = true;
            MainWindow._MainWindow.BackupTextbox.IsEnabled = true;
            MainWindow._MainWindow.StartButton.IsEnabled   = true;
            MainWindow._MainWindow.StopButton.IsEnabled    = false;
        }

    }
}
