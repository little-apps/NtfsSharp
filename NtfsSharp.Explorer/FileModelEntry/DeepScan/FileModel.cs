using System.Collections;
using System.Collections.Generic;
using NtfsSharp.Files;
using NtfsSharp.Files.Attributes;
using NtfsSharp.Files.Attributes.Base;

namespace NtfsSharp.Explorer.FileModelEntry.DeepScan
{
    public class FileModel : BaseFileModel
    {
        public const uint RootRecordNum = 5;

        private readonly SortedList<FileModelEntry, List<FileModelEntry>> _fileRecordNums;

        public FileModel(SortedList<FileModelEntry, List<FileModelEntry>> fileRecordNums, Volume vol) : base(vol)
        {
            _fileRecordNums = fileRecordNums;
        }

        public override IEnumerable GetChildren(object parent)
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
                if (childFileRecord == parentFileModelEntry)
                    continue;

                childFileRecord.ReadFileRecord(Volume);
                childFileRecord.SetFilePath(parentFileModelEntry.FilePath);

                sortedFileModelEntries.Add(childFileRecord);
            }

            return sortedFileModelEntries;
        }

        public override bool HasChildren(object parent)
        {
            if (parent == null)
                return true;

            var parentFileModelEntry = parent as FileModelEntry;

            return parentFileModelEntry != null && _fileRecordNums.ContainsKey(parentFileModelEntry);
        }

        public override void Dispose()
        {
            Volume.Dispose();
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

                    if (parentFileRecordNum == default)
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
