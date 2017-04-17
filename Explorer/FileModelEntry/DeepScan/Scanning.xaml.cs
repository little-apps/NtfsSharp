using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Explorer.Annotations;
using NtfsSharp;
using NtfsSharp.FileRecords;
using NtfsSharp.FileRecords.Attributes;
using NtfsSharp.FileRecords.Attributes.Base;

namespace Explorer.FileModelEntry.DeepScan
{
    /// <summary>
    /// Interaction logic for Scanning.xaml
    /// </summary>
    public partial class Scanning : INotifyPropertyChanged
    {
        private int _read;
        private int _total;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public int Read
        {
            get => _read;
            set
            {
                _read = value;

                OnPropertyChanged(nameof(FilesRead));
                OnPropertyChanged(nameof(Read));
            }
        }

        public int Total
        {
            get => _total;
            set
            {
                _total = value;
                OnPropertyChanged(nameof(FilesRead));
                OnPropertyChanged(nameof(Total));
            }
        }

        public string FilesRead => $"Files read: {Read} / {Total}";

        public Scanning()
        {
            InitializeComponent();
        }

        public async Task<FileModel> Scan(Volume vol)
        {
            Read = 0;
            Total = (int) vol.TotalInodes;

            var cancelToken = _cancellationTokenSource.Token;
            var parentRecordNums =
                new SortedList<FileModelEntry, List<FileModelEntry>>(new FileModelEntryByRecordNumComparer());

            await Task.Run(() =>
            {
                var rootFileModelEntry = new FileModelEntry(FileModel.RootRecordNum);

                parentRecordNums.Add(rootFileModelEntry, new List<FileModelEntry>());

                foreach (var mftEntry in vol.MFT)
                {
                    if (cancelToken.IsCancellationRequested)
                        break;

                    var mftFileRecord = mftEntry.Value;

                    // Add to root directory
                    parentRecordNums[rootFileModelEntry]
                        .Add(new FileModelEntry(mftFileRecord.Header.MFTRecordNumber));
                }

                foreach (var fileRecord in vol.ReadFileRecords(true))
                {
                    if (cancelToken.IsCancellationRequested)
                        break;

                    var fileRecordNum = fileRecord.Header.MFTRecordNumber;
                    var fileModelEntry = new FileModelEntry(fileRecordNum);

                    if (fileRecord.Header.Flags.HasFlag(FileRecord.Flags.IsDirectory))
                    {
                        if (!parentRecordNums.ContainsKey(fileModelEntry))
                            parentRecordNums.Add(fileModelEntry, new List<FileModelEntry>());
                    }

                    var fileNameAttr =
                        fileRecord.FindAttributeBodyByType(AttributeHeaderBase.NTFS_ATTR_TYPE
                            .FILE_NAME) as FileNameAttribute;

                    var parentFileRecordNum = (fileNameAttr?.FileName.Data.FileReference.FileRecordNumber)
                        .GetValueOrDefault();

                    if (parentFileRecordNum == default(ulong) || fileRecordNum == parentFileRecordNum)
                        continue;

                    var parentFileModelEntry = new FileModelEntry(parentFileRecordNum);

                    if (!parentRecordNums.ContainsKey(parentFileModelEntry))
                        parentRecordNums.Add(parentFileModelEntry, new List<FileModelEntry>());

                    parentRecordNums[parentFileModelEntry].Add(fileModelEntry);

                    Read++;
                }
            }, cancelToken);
            
            return new FileModel(parentRecordNums, vol);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource.Cancel();

            MessageBox.Show(Owner, "Deep scan was cancelled. Not all files will be available.", "NtfsSharp Explorer",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
