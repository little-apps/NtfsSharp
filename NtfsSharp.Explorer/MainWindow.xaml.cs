using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Win32;
using NtfsSharp.Drivers;
using NtfsSharp.Explorer.FileModelEntry;
using NtfsSharp.Explorer.FileModelEntry.DeepScan;
using NtfsSharp.Explorer.Properties;
using NtfsSharp.Volumes;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace NtfsSharp.Explorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        public BaseFileModelEntry SelectedFileModelEntry => Tree.SelectedNode?.Tag as BaseFileModelEntry;

        private readonly Options _options = new Options();

        public MainWindow()
        {
            InitializeComponent();
        }

        private bool CheckCanScan()
        {
            try
            {
                _options.IsValid();

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(this, e.Message, "NtfsSharp Explorer", MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
        }

        private BaseDiskDriver CreateDiskDriver()
        {
            switch (_options.MediaType)
            {
                case Options.MediaTypes.Drive:
                    return new PartitionDriver($@"\\.\{_options.SelectedDriveLetter}:");

                case Options.MediaTypes.VhdFile: 
                    return new VhdDriver(_options.VhdFile);

                default:
                    throw new Exception("Unknown media type selected.");
            }
        }

        private void QuickScanButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!CheckCanScan())
                return;

            ((BaseFileModel) Tree.Model)?.Dispose();
            
            Tree.Model =
                new FileModelEntry.QuickScan.FileModel(
                    new Volume(CreateDiskDriver()));
        }

        private async void DeepScanButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!CheckCanScan())
                return;
            
            var scanning = new Scanning {Owner = this};

            scanning.Show();

            ((BaseFileModel) Tree.Model)?.Dispose();

            Tree.Model = await scanning.Scan(new Volume(CreateDiskDriver()));

            scanning.Close();
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private static string FieldValueToString(object obj)
        {
            if (obj is string)
                return (string) obj;

            if (obj is Enum)
                return ((Enum) obj).ToString();

            if (obj is FILETIME)
            {
                var fileTime = (FILETIME) obj;
                var fileTimeLong = ((ulong)fileTime.dwHighDateTime << 32) + (uint) fileTime.dwLowDateTime;

                var dateTime = DateTime.FromFileTimeUtc((long) fileTimeLong);
                return dateTime.ToString(CultureInfo.InvariantCulture);
            }

            return Convert.ToString(obj);
        }

        private void Save_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedFileModelEntry == null)
            {
                MessageBox.Show(this, "No file selected", "NtfsSharp Explorer", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var dataStream = SelectedFileModelEntry.FileRecord.FileStream;

            if (dataStream == null)
            {
                MessageBox.Show(this, "Cannot find $DATA attribute for selected file.", "NtfsSharp Explorer",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                CreatePrompt = true,
                OverwritePrompt = true
            };
            
            var firstDotIndex = SelectedFileModelEntry.Filename.IndexOf('.');
            if (firstDotIndex >= 0)
            {
                var selectedFileExt =
                    SelectedFileModelEntry.Filename.Substring(firstDotIndex + 1);

                saveFileDialog.Filter = $"*.{selectedFileExt}|*.{selectedFileExt}|*.*|*.*";
            }
            else
            {
                saveFileDialog.Filter = "*.*|*.*";
            }

            if (saveFileDialog.ShowDialog(this) == true)
            {
                var fileStream = saveFileDialog.OpenFile();

                dataStream.CopyTo(fileStream);

                fileStream.Flush();
                fileStream.Close();

                MessageBox.Show(this, "Copied $DATA stream to file.", "NtfsSharp Explorer", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void OpenExplorer_OnClick(object sender, RoutedEventArgs e)
        {
            if (_options.MediaType != Options.MediaTypes.Drive)
            {
                MessageBox.Show(this, "Open in explorer is only available with drive media.", "NtfsSharp Explorer",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (SelectedFileModelEntry == null)
            {
                MessageBox.Show(this, "No file selected", "NtfsSharp Explorer", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var explorerPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.Windows)}\\explorer.exe";

            if (!File.Exists(explorerPath))
            {
                MessageBox.Show(this, "Unable to locate \"explorer.exe\" in Windows directory.", "NtfsSharp Explorer",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            var drive = $"{_options.SelectedDriveLetter}:\\";
            var filePath = $"{drive}{SelectedFileModelEntry.FilePath}";

            if (!filePath.EndsWith("\\"))
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show(this,
                        "File does not seem to exist. It may be hidden as part of the master file table.",
                        "NtfsSharp Explorer", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                if (!Directory.Exists(filePath))
                {
                    MessageBox.Show(this,
                        "Directory does not seem to exist. It may be hidden as part of the master file table.",
                        "NtfsSharp Explorer", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
            }
            
            Process.Start(explorerPath, $"/select, \"{filePath}\"");
        }

        private void Open_OnClick(object sender, RoutedEventArgs e)
        {
            var openWindow = new OpenMedia(_options) {Owner = this};
            
            openWindow.ShowDialog();
        }
    }
}
