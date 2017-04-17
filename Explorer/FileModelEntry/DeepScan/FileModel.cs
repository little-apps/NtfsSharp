using System.Collections;
using System.Collections.Generic;
using Aga.Controls.Tree;
using NtfsSharp;
using NtfsSharp.FileRecords;
using NtfsSharp.FileRecords.Attributes;
using NtfsSharp.FileRecords.Attributes.Base;

namespace Explorer.FileModelEntry.DeepScan
{
    public class FileModel : ITreeModel
    {
        public const uint RootRecordNum = 5;

        private readonly SortedList<FileModelEntry, List<FileModelEntry>> _fileRecordNums;
        private readonly Volume _volume;
        
        public FileModel(SortedList<FileModelEntry, List<FileModelEntry>> fileRecordNums, Volume vol)
        {
            _fileRecordNums = fileRecordNums;
            _volume = vol;
        }

        public IEnumerable GetChildren(object parent)
        {
            FileModelEntry parentFileModelEntry;

            if (parent == null)
            {
                parentFileModelEntry = new FileModelEntry(RootRecordNum);
            }
            else
            {
                parentFileModelEntry = parent as FileModelEntry;
            }

            var sortedFileModelEntries = new SortedSet<FileModelEntry>(new FileModelEntryByFileName());

            if (parentFileModelEntry == null)
                return sortedFileModelEntries;

            if (!_fileRecordNums.TryGetValue(parentFileModelEntry, out List<FileModelEntry> childFileRecords))
                return sortedFileModelEntries;

            foreach (var childFileRecord in childFileRecords)
            {
                childFileRecord.ReadFileRecord(_volume);

                sortedFileModelEntries.Add(childFileRecord);
            }

            return sortedFileModelEntries;
        }

        public bool HasChildren(object parent)
        {
            if (parent == null)
                return true;

            var parentFileModelEntry = parent as FileModelEntry;

            return parentFileModelEntry != null && _fileRecordNums.ContainsKey(parentFileModelEntry);
        }

        public static FileModel CreateFileModel(Volume vol)
        {
            var parentRecordNums = new SortedList<FileModelEntry, List<FileModelEntry>>();

            foreach (var fileRecord in vol.ReadFileRecords(true))
            {
                var fileRecordNum = fileRecord.Header.MFTRecordNumber;
                var fileModelEntry = new FileModelEntry(fileRecordNum);

                if (fileRecord.Header.Flags.HasFlag(FileRecord.Flags.IsDirectory))
                {
                    parentRecordNums.Add(fileModelEntry, new List<FileModelEntry>());
                }
                else
                {
                    var fileNameAttr =
                        fileRecord.FindAttributeBodyByType(AttributeHeaderBase.NTFS_ATTR_TYPE
                            .FILE_NAME) as FileNameAttribute;

                    var parentFileRecordNum = (fileNameAttr?.FileName.Data.FileReference.FileRecordNumber)
                        .GetValueOrDefault();

                    if (parentFileRecordNum == default(ulong))
                        continue;

                    var parentFileModelEntry = new FileModelEntry(parentFileRecordNum);
                    
                    if (!parentRecordNums.ContainsKey(parentFileModelEntry))
                        parentRecordNums.Add(parentFileModelEntry, new List<FileModelEntry>());

                    parentRecordNums[parentFileModelEntry].Add(fileModelEntry);
                }
            }

            return new FileModel(parentRecordNums, vol);
        }
    }
}
