using System;
using System.Collections.Generic;
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

namespace IronmanSaveBackup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string xcom2RegexFind= @"(^save_IRONMAN_Campaign_.*$)";
        public const string xcom2RegexCampaign = @"^save_IRONMAN_Campaign_(.*)$"; //Match[0] for save
        public MainWindow()
        {
            InitializeComponent();
            OnEventSaves.IsChecked = Settings.Default.EnableOnEventBackup;
            BackupTextbox.Text = Settings.Default.BackupLocation;
            SaveTextbox.Text = Settings.Default.SaveLocation;
            IntervalSlider.Value = Settings.Default.SaveInterval;
            BackupKeepSlider.Value = Settings.Default.KeepBackupNumber;
            if (OnEventSaves.IsChecked == true)
            {
                IntervalSlider.IsEnabled = false;
                IntervalTextbox.IsEnabled = false;
            }


        }

        private void BackupTextbox_Click(object sender, MouseButtonEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.RootFolder = Environment.SpecialFolder.Desktop;
            DialogResult result = dialog.ShowDialog();
            BackupTextbox.Text = dialog.SelectedPath;
            if (BackupTextbox.Text != null)
            {
                Settings.Default.BackupLocation = BackupTextbox.Text;
                Settings.Default.Save();
            }
        }

        private void SaveTextbox_Click(object sender, MouseButtonEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.RootFolder = Environment.SpecialFolder.Desktop;
            DialogResult result = dialog.ShowDialog();
            SaveTextbox.Text = dialog.SelectedPath;
            if (SaveTextbox.Text != null)
            {
                Settings.Default.SaveLocation = SaveTextbox.Text;
                Settings.Default.Save();
            }
        }

        private void OnEventSaves_OnClick(object sender, RoutedEventArgs e)
        {
            IntervalSlider.IsEnabled = OnEventSaves.IsChecked != true;
            IntervalTextbox.IsEnabled = OnEventSaves.IsChecked != true;
            Settings.Default.EnableOnEventBackup = OnEventSaves.IsChecked == true;
            Settings.Default.Save();
        }

        private void IntervalSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Settings.Default.SaveInterval = (int) IntervalSlider.Value;
            Settings.Default.Save();
        }

        private void BackupKeepSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Settings.Default.KeepBackupNumber = (int) BackupKeepSlider.Value;
            Settings.Default.Save();
        }

        private void OpenSaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            var saveLocation = "";
            if (!string.IsNullOrEmpty(SaveTextbox.Text))
            {
                saveLocation = SaveTextbox.Text;
            }
            else if (!string.IsNullOrEmpty(Settings.Default.SaveLocation))
            {
                saveLocation = Settings.Default.SaveLocation;
            }

            if (Directory.Exists(saveLocation))
            {
                System.Diagnostics.Process.Start(saveLocation);
            }
            else
            {
                
            }
        }

        private void OpenBackupButton_OnClick(object sender, RoutedEventArgs e)
        {
            
            var backupLocationEmpty = "";
            if (!string.IsNullOrEmpty(SaveTextbox.Text))
            {
                backupLocationEmpty = BackupTextbox.Text;
            }
            else if (!string.IsNullOrEmpty(Settings.Default.BackupLocation))
            {
                backupLocationEmpty = Settings.Default.BackupLocation;
            }

            if (Directory.Exists(backupLocationEmpty))
            {
                System.Diagnostics.Process.Start(backupLocationEmpty);
            }
            else
            {
                
            }
        }
    }
}
