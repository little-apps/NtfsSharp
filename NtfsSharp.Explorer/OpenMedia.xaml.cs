using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Win32;
using NtfsSharp.Explorer.Properties;

namespace NtfsSharp.Explorer
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// Interaction logic for Open.xaml
    /// </summary>
    public sealed partial class OpenMedia : INotifyPropertyChanged
    {
        private readonly Options _options;

        public Visibility DriveVisibility
        {
            get { return RadioButtonDrive.IsChecked.GetValueOrDefault() ? Visibility.Visible : Visibility.Collapsed; }
            set
            {

            }
        }

        public Visibility FileVisibility
        {
            get { return RadioButtonVhd.IsChecked.GetValueOrDefault() ? Visibility.Visible : Visibility.Collapsed; }
            set
            {

            }
        }

        internal OpenMedia(Options options)
        {
            InitializeComponent();

            _options = options;

            Populate();
        }

        private void Populate()
        {
            foreach (var driveInfo in _options.AvailableDrives)
            {
                ComboBoxDrive.Items.Add(driveInfo.Name);
            }

            RadioButtonDrive.IsChecked = false;
            RadioButtonVhd.IsChecked = false;

            switch (_options.MediaType)
            {
                case Options.MediaTypes.VhdFile:
                {
                    RadioButtonVhd.IsChecked = true;
                    TextBoxFile.Text = _options.VhdFile ?? "";
                    break;
                }

                default:
                {
                    RadioButtonDrive.IsChecked = true;
                    ComboBoxDrive.SelectedIndex = (int) _options.SelectedDriveIndex;
                    break;
                }
            }
        }

        /// <summary>
        /// Persist changes to options
        /// </summary>
        private void Save()
        {
            if (RadioButtonDrive.IsChecked.GetValueOrDefault())
            {
                _options.MediaType = Options.MediaTypes.Drive;
                _options.SelectedDriveIndex = (uint) ComboBoxDrive.SelectedIndex;
            } 
            else if (RadioButtonVhd.IsChecked.GetValueOrDefault())
            {
                _options.MediaType = Options.MediaTypes.VhdFile;
                _options.VhdFile = TextBoxFile.Text;
            }
            else
            {
                throw new Exception("No radio button selected.");
            }
            

            _options.IsValid();
        }

        private void ButtonChooseFile_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "VHD Files|*.vhd|All Files|*.*",
                CheckFileExists = true,
                FileName = TextBoxFile.Text
            };

            if (openFileDialog.ShowDialog(this).GetValueOrDefault())
            {
                TextBoxFile.Text = openFileDialog.FileName;
            }
        }

        private void ButtonOpen_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Save();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Open Media", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Open Media", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
            }

            base.OnClosing(e);
        }

        private void RadioButtonTypeClicked(object sender, RoutedEventArgs e)
        {
            OnPropertyChanged(nameof(DriveVisibility));
            OnPropertyChanged(nameof(FileVisibility));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
