﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Win32;
using NtfsSharp.Drivers;
using NtfsSharp.Explorer.Factories;
using NtfsSharp.Explorer.FileModelEntry;
using NtfsSharp.Explorer.FileModelEntry.DeepScan;
using NtfsSharp.Explorer.Properties;
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

        private void QuickScanButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!CheckCanScan())
                return;

            ((BaseFileModel) Tree.Model)?.Dispose();

            var diskDriver = DiskDriverFactory.Make(_options);
            var volume = new Volume(diskDriver);
            
            volume.Read();

            Tree.Model = new FileModelEntry.QuickScan.FileModel(volume);
        }

        private async void DeepScanButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!CheckCanScan())
                return;
            
            var scanning = new Scanning {Owner = this};

            scanning.Show();

            ((BaseFileModel) Tree.Model)?.Dispose();

            var diskDriver = DiskDriverFactory.Make(_options);
            var volume = new Volume(diskDriver);

            Tree.Model = await scanning.Scan(volume.Read() as Volume);

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
            switch (obj)
            {
                case string str:
                    return str;
                
                case Enum choice:
                    return choice.ToString();
                
                case FILETIME fileTime:
                {
                    var fileTimeLong = ((ulong)fileTime.dwHighDateTime << 32) + (uint) fileTime.dwLowDateTime;

                    var dateTime = DateTime.FromFileTimeUtc((long) fileTimeLong);
                    return dateTime.ToString(CultureInfo.InvariantCulture);
                }

                default:
                    return Convert.ToString(obj);
            }
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

            var fileExtension = Path.GetExtension(SelectedFileModelEntry.Filename);

            saveFileDialog.Filter = !string.IsNullOrEmpty(fileExtension)
                ? $"*{fileExtension}|*{fileExtension}|*.*|*.*"
                : "*.*|*.*";

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
