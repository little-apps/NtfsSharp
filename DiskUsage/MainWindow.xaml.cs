using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using DiskUsage.Annotations;
using NtfsSharp;

namespace DiskUsage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private Volume _volume;
        private readonly UsageCollection _usageCollection = new UsageCollection();
        private readonly Timer _timer = new Timer();
        private readonly MenuItem[] _updateIntervalItems;

        public UsageCollection UsageCollection
        {
            get { return _usageCollection; }
        }

        public string TotalBytes
        {
            get
            {
                return _volume == null
                    ? string.Empty
                    : (_volume.BootSector.TotalSectors * _volume.BytesPerSector).ToString("N0");
            }
        }

        public string UsedBytes
        {
            get { return $"{UsageCollection.Used.Value:N0}"; }
        }

        public string FreeBytes
        {
            get { return $"{UsageCollection.Free.Value:N0}"; }
        }

        public string Clusters
        {
            get
            {
                return _volume == null ? string.Empty : (_volume.BootSector.TotalSectors / _volume.SectorsPerCluster).ToString("N0");
            }
        }
        

        public ObservableCollection<PieDataPoint> DataPoints => new ObservableCollection<PieDataPoint>();

        public MainWindow()
        {
            InitializeComponent();

            _updateIntervalItems = new[]
            {
                UpdateIntervalNever,
                UpdateInterval5Seconds,
                UpdateInterval10Seconds,
                UpdateInterval15Seconds,
                UpdateInterval30Seconds,
                UpdateInterval1Minute,
                UpdateInterval5Minutes
            };

            DataContext = this;
            
            PopulateDrives();

            _timer.Elapsed += TimerOnElapsed;
            SetUpdateInterval(UpdateInterval10Seconds);
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            RefreshUsage();
        }

        private void PopulateDrives()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady || drive.DriveType != DriveType.Fixed || drive.TotalFreeSpace == 0)
                    continue;

                DriveComboBox.Items.Add(drive.Name);
            }

            DriveComboBox.SelectedIndex = 0;
            ChangeVolume();
        }

        private void ChangeVolume()
        {
            var selectedDrive = DriveComboBox.SelectedItem as string;

            if (string.IsNullOrEmpty(selectedDrive))
            {
                MessageBox.Show(this, "No drive is selected.", "NtfsSharp DiskUsage", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var volumeChar = selectedDrive[0];

            try
            {
                _volume = new Volume(volumeChar);
            }
            catch (Exception)
            {
                MessageBox.Show(this, "An error occurred opening the volume. Please try again.", "NtfsSharp DiskUsage",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            RefreshUsage();
        }

        private async void RefreshUsage()
        {
            await UsageCollection.UpdateVolume(_volume);

            Dispatcher.Invoke(() =>
            {
                PieSeries1.ItemsSource = UsageCollection;

                OnPropertyChanged(nameof(TotalBytes));
                OnPropertyChanged(nameof(FreeBytes));
                OnPropertyChanged(nameof(UsedBytes));
                OnPropertyChanged(nameof(Clusters));
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void Update_OnClicked(object sender, RoutedEventArgs e)
        {
            ChangeVolume();
        }

        private void UpdateInterval_OnChecked(object sender, RoutedEventArgs e)
        {
            if (ReferenceEquals(sender, UpdateIntervalNever))
                return;
        }

        private void UpdateInterval_OnClick(object sender, RoutedEventArgs e)
        {
            MenuItem newMenuItem;
            var menuItem = sender as MenuItem;

            if (menuItem == null)
                return;

            if (!menuItem.IsChecked)
            {
                if (ReferenceEquals(sender, UpdateIntervalNever))
                {
                    MessageBox.Show(this,
                        "The \"Never\" option cannot be unchecked. Please choose a different interval.",
                        "NtfsSharp DiskUsage", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                newMenuItem = UpdateIntervalNever;
            }
            else
            {
                newMenuItem = menuItem;
            }

            SetUpdateInterval(newMenuItem);

            MessageBox.Show(this, $"Updated update interval: {menuItem.Header}", "NtfsSharp DiskUsage",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetUpdateInterval(MenuItem menuItem)
        {
            var newIntervalSecs = uint.Parse(menuItem.Tag.ToString());

            foreach (var item in _updateIntervalItems)
            {
                item.IsChecked = false;
            }

            menuItem.IsChecked = true;

            if (_timer.Enabled)
                _timer.Stop();

            if (newIntervalSecs == 0)
                return;

            _timer.Interval = newIntervalSecs * 1000;
            _timer.Start();
        }

        private void MenuItemExit_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure you want to exit?", "NtfsSharp DiskUsage", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                Close();
        }
    }
}
