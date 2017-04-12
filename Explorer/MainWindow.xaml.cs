using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Controls;
using Explorer.Annotations;
using Microsoft.Win32;
using NtfsSharp;
using NtfsSharp.FileRecords.Attributes;
using NtfsSharp.FileRecords.Attributes.Base;

namespace Explorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        public FileModelEntry SelectedFileModelEntry => Tree.SelectedNode?.Tag as FileModelEntry;

        public MainWindow()
        {
            InitializeComponent();

            foreach (var drive in DriveInfo.GetDrives())
            {
                Drives.Items.Add(drive.Name);
            }

            Drives.SelectedIndex = 0;
        }

        private void ScanButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedDrive = Drives.Text[0];
            Tree.Model = new FileModel(new Volume(selectedDrive));
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
            
            var filePath = $"{Drives.Text.Substring(0, 2)}{SelectedFileModelEntry.FilePath}";

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
    }
}
