using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IronmanSaveBackup.Properties;
using Path = System.IO.Path;

namespace IronmanSaveBackup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Backup runningBackup = new Backup();
        public MainWindow()
        {
            InitializeComponent();

            OnEventSaves.IsChecked = Settings.Default.EnableOnEventBackup;
            BackupTextbox.Text = Settings.Default.BackupParentFolder;
            SaveTextbox.Text = Settings.Default.SaveParentFolder;
            IntervalSlider.Value = Settings.Default.SaveInterval;
            MaxBackupSlider.Value = Settings.Default.MaxBackups;
            MostRecentBackup.Content = $"Campaign {Settings.Default.MostRecentCampaign} @ {Settings.Default.LastUpdated}";

            runningBackup.EventDrivenBackups = (bool) OnEventSaves.IsChecked;
            runningBackup.BackupParentFolder = BackupTextbox.Text;
            runningBackup.SaveParentFolder = SaveTextbox.Text;

            runningBackup.Campaign = Settings.Default.MostRecentCampaign;
            runningBackup.LastUpdated = Settings.Default.LastUpdated;
            runningBackup.MaxBackups = (int) MaxBackupSlider.Value;
            runningBackup.BackupInterval = (int) IntervalSlider.Value;

            runningBackup.BackupActive = false;

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;

            if (OnEventSaves.IsChecked != true) return;
            IntervalSlider.IsEnabled = false;
            IntervalTextbox.IsEnabled = false;
        }

        private void BackupTextbox_Click(object sender, MouseButtonEventArgs e)
        {
            var dialog = new FolderBrowserDialog { SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) };
            if (runningBackup.BackupParentFolder != string.Empty)
            {
                dialog.SelectedPath = runningBackup.BackupParentFolder;
            }
            var result = dialog.ShowDialog();
            BackupTextbox.Text = dialog.SelectedPath;
            if (BackupTextbox.Text == null) return;
            runningBackup.BackupParentFolder = BackupTextbox.Text;
            runningBackup.UpdateSettings();
        }

        private void SaveTextbox_Click(object sender, MouseButtonEventArgs e)
        {
            var dialog = new FolderBrowserDialog { SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) };
            if (runningBackup.SaveParentFolder != string.Empty)
            {
                dialog.SelectedPath = runningBackup.SaveParentFolder;
            }

            var result = dialog.ShowDialog();
            SaveTextbox.Text = dialog.SelectedPath;
            if (SaveTextbox.Text == null) return;
            runningBackup.SaveParentFolder = SaveTextbox.Text;
            runningBackup.UpdateSettings();
        }

        private void OnEventSaves_OnClick(object sender, RoutedEventArgs e)
        {
            IntervalSlider.IsEnabled = OnEventSaves.IsChecked != true;
            IntervalTextbox.IsEnabled = OnEventSaves.IsChecked != true;
            runningBackup.EventDrivenBackups = (bool) OnEventSaves.IsChecked;
            runningBackup.UpdateSettings();
        }

        private void IntervalSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            runningBackup.BackupInterval = (int) IntervalSlider.Value;
        }

        private void BackupKeepSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            runningBackup.MaxBackups = (int) MaxBackupSlider.Value;
        }

        private void OpenSaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SaveTextbox.Text))
            {
                FolderOperations.OpenFolder(SaveTextbox.Text);
            }
            else
            {
                MessageOperations.UserMessage("The folder you selected does not exist or cannot be found.", MessageOperations.MessageTypeEnum.DoesNotExistError);
            }
        }

        private void OpenBackupButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(runningBackup.BackupParentFolder))
            {
                FolderOperations.OpenFolder(runningBackup.BackupParentFolder);
            }
            else
            {
                MessageOperations.UserMessage("The folder you selected does not exist or cannot be found.", MessageOperations.MessageTypeEnum.DoesNotExistError);
            }
        }

        private void DeleteBackupButton_OnClick(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrEmpty(BackupTextbox.Text))
            {
                if (!MessageOperations.ConfirmChoice(MessageOperations.MessageChoiceEnum.DeleteChoice)) return;
                var backupList = Directory.GetFiles(BackupTextbox.Text, "*.isb", SearchOption.AllDirectories);
                foreach (var backup in backupList)
                {
                    try
                    {
                        File.Delete(backup);
                    }
                    catch (ArgumentNullException)
                    {
                        MessageOperations.UserMessage("No filepath found for the selected backup.", MessageOperations.MessageTypeEnum.DoesNotExistError);
                    }
                    catch (ArgumentException)
                    {
                        MessageOperations.UserMessage("Could not all backups in selected bakcup folder.", MessageOperations.MessageTypeEnum.BackupError);
                    }
                    catch (Exception)
                    {
                        MessageOperations.UserMessage("An exception occurred while trying to delete existing backups. Please close XCOM 2, restart this application and try again.", MessageOperations.MessageTypeEnum.GenericError);
                    }

                }

                MessageOperations.UserMessage("All Backups Successfully Deleted.", MessageOperations.MessageTypeEnum.BackupSuccess);
            }
            else
            {
                MessageOperations.UserMessage("The selected Backup Folder does not exist.", MessageOperations.MessageTypeEnum.DoesNotExistError);
            }


        }

        private void RestoreBackupText_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = @"Backup Files (*.isb)|*.isb",
                InitialDirectory = runningBackup.BackupParentFolder
            };
            var result = dialog.ShowDialog();
            RestoreBackupText.Text = dialog.FileName;
            runningBackup.RestoreFile = RestoreBackupText.Text;
            runningBackup.UpdateSettings();
        }

        private void RestoreBackupButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (File.Exists(runningBackup.RestoreFile))
            {
                if (Path.GetExtension(runningBackup.RestoreFile).ToLower() != ".isb")
                {
                    MessageOperations.UserMessage("You've selected an invalid file for restoration.", MessageOperations.MessageTypeEnum.RestoreError);
                    RestoreBackupText.Text = "";
                    runningBackup.RestoreFile = "";
                }
                else if (MessageOperations.ConfirmChoice(MessageOperations.MessageChoiceEnum.ReplaceChoice))
                {
                    runningBackup.RestoreBackup();
                    runningBackup.UpdateSettings();
                }
            }
            else
            {
                MessageOperations.UserMessage("The selected restore file does not exist.", MessageOperations.MessageTypeEnum.DoesNotExistError);
            }

        }

        private void ForceBackupButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(runningBackup.BackupParentFolder) && !string.IsNullOrEmpty(runningBackup.SaveParentFolder))
            {
                runningBackup.LastUpdated = runningBackup.ForceCreateBackup();
                MostRecentBackup.Content = $"Campaign {runningBackup.Campaign} @ {runningBackup.LastUpdated}";
                runningBackup.UpdateSettings();
            }
            else
            {
                MessageOperations.UserMessage("The selected Backup Folder and/or Save Folder does not exist. Please verify the locations selected.", MessageOperations.MessageTypeEnum.DoesNotExistError);
            }
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            runningBackup.BackupActive = true;
            MostRecentBackup.Content = $"Campaign {runningBackup.Campaign} @ {runningBackup.LastUpdated}";
            runningBackup.UpdateSettings();
            SaveTextbox.IsEnabled = false;
            BackupTextbox.IsEnabled = false;
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
        }

        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            MostRecentBackup.Content = $"Campaign {runningBackup.Campaign} @ {runningBackup.LastUpdated}";
            runningBackup.CancelBackupSource.Cancel();
            runningBackup.UpdateSettings();
            SaveTextbox.IsEnabled = true;
            BackupTextbox.IsEnabled = true;
            runningBackup.BackupActive = false;
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }
    }
}
