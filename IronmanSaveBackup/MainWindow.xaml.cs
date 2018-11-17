using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using IronmanSaveBackup.Enums;
using IronmanSaveBackup.Properties;
using Path = System.IO.Path;

namespace IronmanSaveBackup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        readonly Backup _runningBackup = new Backup();

        internal string RecentBackup
        {
            get => MostRecentBackup.Content.ToString();
            set { this.Dispatcher.Invoke((() => { MostRecentBackup.Content = value; })); }
        }

        internal static MainWindow _MainWindow;
        public MainWindow()
        {
            InitializeComponent();
            _MainWindow                        = this;
            OnEventSaves.IsChecked            = Settings.Default.EnableOnEventBackup;
            BackupTextbox.Text                = Settings.Default.BackupParentFolder;
            SaveTextbox.Text                  = Settings.Default.SaveParentFolder;
            IntervalSlider.Value              = Settings.Default.SaveInterval;
            MaxBackupSlider.Value             = Settings.Default.MaxBackups;
            MostRecentBackup.Content          = $"Campaign {Settings.Default.MostRecentCampaign} @ {Settings.Default.LastUpdated}";

            _runningBackup.EventDrivenBackups = (bool) OnEventSaves.IsChecked;
            _runningBackup.BackupParentFolder = BackupTextbox.Text;
            _runningBackup.SaveParentFolder   = SaveTextbox.Text;

            _runningBackup.Campaign           = Settings.Default.MostRecentCampaign;
            _runningBackup.LastUpdated        = Settings.Default.LastUpdated;
            _runningBackup.MaxBackups         = (int) MaxBackupSlider.Value;
            _runningBackup.BackupInterval     = (int) IntervalSlider.Value;

            _runningBackup.BackupActive       = false;

            StartButton.IsEnabled             = true;
            StopButton.IsEnabled              = false;

            if (OnEventSaves.IsChecked != true) return;
            IntervalSlider.IsEnabled          = false;
            IntervalTextbox.IsEnabled         = false;
        }

        private void BackupTextbox_Click(object sender, MouseButtonEventArgs e)
        {
            var dialog = new FolderBrowserDialog { SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) };
            if (string.IsNullOrWhiteSpace(_runningBackup.BackupParentFolder))
            {
                dialog.SelectedPath = _runningBackup.BackupParentFolder;
            }
            var result         = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;
            BackupTextbox.Text = dialog.SelectedPath;
            if (BackupTextbox.Text == null) return;
            _runningBackup.BackupParentFolder = BackupTextbox.Text;
            _runningBackup.UpdateSettings();
        }

        private void SaveTextbox_Click(object sender, MouseButtonEventArgs e)
        {
            var dialog = new FolderBrowserDialog { SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) };
           if (!string.IsNullOrWhiteSpace(_runningBackup.SaveParentFolder))
            {
                dialog.SelectedPath = _runningBackup.SaveParentFolder;
            }

            var result       = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;
            SaveTextbox.Text = dialog.SelectedPath;
            if (SaveTextbox.Text == null) return;
            _runningBackup.SaveParentFolder = SaveTextbox.Text;
            _runningBackup.UpdateSettings();
        }

        private void OnEventSaves_OnClick(object sender, RoutedEventArgs e)
        {
            IntervalSlider.IsEnabled  = OnEventSaves.IsChecked != true;
            IntervalTextbox.IsEnabled = OnEventSaves.IsChecked != true;
            if (OnEventSaves.IsChecked != null) _runningBackup.EventDrivenBackups = (bool) OnEventSaves.IsChecked;
            _runningBackup.UpdateSettings();
        }

        private void IntervalSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _runningBackup.BackupInterval = (int) IntervalSlider.Value;
        }

        private void BackupKeepSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _runningBackup.MaxBackups = (int) MaxBackupSlider.Value;
        }

        private void OpenSaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SaveTextbox.Text))
            {
                FolderOperations.OpenFolder(SaveTextbox.Text);
            }
            else
            {
                MessageOperations.UserMessage(Properties.Resources.FolderNotFound, MessageTypeEnum.DoesNotExistError);
            }
        }

        private void OpenBackupButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_runningBackup.BackupParentFolder))
            {
                FolderOperations.OpenFolder(_runningBackup.BackupParentFolder);
            }
            else
            {
                MessageOperations.UserMessage(Properties.Resources.FolderNotFound, MessageTypeEnum.DoesNotExistError);
            }
        }

        private void DeleteBackupButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(BackupTextbox.Text))
            {
                if (!MessageOperations.ConfirmChoice(MessageChoiceEnum.DeleteChoice)) return;
                var backupList = Directory.GetFiles(BackupTextbox.Text, "*.isb", SearchOption.AllDirectories);
                foreach (var backup in backupList)
                {
                    try
                    {
                        File.Delete(backup);
                    }
                    catch (ArgumentNullException)
                    {
                        MessageOperations.UserMessage(Properties.Resources.FilepathNotFound, MessageTypeEnum.DoesNotExistError);
                    }
                    catch (ArgumentException)
                    {
                        MessageOperations.UserMessage(Properties.Resources.UnableToDelete, MessageTypeEnum.BackupError);
                    }
                    catch (Exception)
                    {
                        MessageOperations.UserMessage(Properties.Resources.ExceptionOnDelete);
                    }

                }

                MessageOperations.UserMessage(Properties.Resources.DeleteAllSuccess, MessageTypeEnum.BackupSuccess);
            }
            else
            {
                MessageOperations.UserMessage(Properties.Resources.FolderNotFound, MessageTypeEnum.DoesNotExistError);
            }


        }

        private void RestoreBackupText_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = @"Backup Files (*.isb)|*.isb",
                InitialDirectory = _runningBackup.BackupParentFolder
            };
            var result                 = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;
            RestoreBackupText.Text = dialog.FileName;
            _runningBackup.RestoreFile = RestoreBackupText.Text;
            _runningBackup.UpdateSettings();

        }

        private void RestoreBackupButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_runningBackup.RestoreFile!= null && File.Exists(_runningBackup.RestoreFile))
            {
                if (Path.GetExtension(_runningBackup.RestoreFile).ToLower() != ".isb")
                {
                    MessageOperations.UserMessage(Properties.Resources.InvalidRestoreFile, MessageTypeEnum.RestoreError);
                    RestoreBackupText.Text     = "";
                    _runningBackup.RestoreFile = "";
                }
                else if (MessageOperations.ConfirmChoice(MessageChoiceEnum.ReplaceChoice))
                {
                    _runningBackup.RestoreBackup();
                    _runningBackup.UpdateSettings();
                }
            }
            else
            {
                MessageOperations.UserMessage(Properties.Resources.MissingRestoreFile, MessageTypeEnum.DoesNotExistError);
            }

        }

        private void ForceBackupButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_runningBackup.BackupParentFolder) && !string.IsNullOrEmpty(_runningBackup.SaveParentFolder))
            {
                _runningBackup.LastUpdated = _runningBackup.ForceCreateBackup();
            }
            else
            {
                MessageOperations.UserMessage(Properties.Resources.FolderNotFound, MessageTypeEnum.DoesNotExistError);
            }
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            SaveTextbox.IsEnabled       = false;
            BackupTextbox.IsEnabled     = false;
            StartButton.IsEnabled       = false;
            StopButton.IsEnabled        = true;
            _runningBackup.BackupActive = true;
            _runningBackup.UpdateSettings();
        }

        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            _runningBackup.CancelBackupSource.Cancel();
            _runningBackup.UpdateSettings();
            _runningBackup.BackupActive = false;
            SaveTextbox.IsEnabled       = true;
            BackupTextbox.IsEnabled     = true;
            StartButton.IsEnabled       = true;
            StopButton.IsEnabled        = false;
        }
    }
}
