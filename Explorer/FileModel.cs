using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Aga.Controls.Tree;
using NtfsSharp;
using NtfsSharp.FileRecords;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.FileRecords.Attributes.IndexAllocation;

namespace Explorer
{
    class FileModel : ITreeModel
    {
        private const uint RootRecordNum = 5;

        private readonly Volume _volume;

        public FileModel(Volume volume)
        {
            _volume = volume;
        }

        public IEnumerable GetChildren(object parent)
        {
            var parentFileRecord = parent == null
                ? _volume.ReadFileRecord(RootRecordNum, true)
                : (parent as FileModelEntry)?.FileRecord;

            var sortedList = new SortedList<string, FileModelEntry>();

            foreach (var fileNameIndex in GetFileNameIndices(parentFileRecord))
            {
                foreach (var fileNameEntry in fileNameIndex.FileNameEntries)
                {
                    if (fileNameEntry.Header.FileReference.FileRecordNumber == parentFileRecord.Header.MFTRecordNumber)
                        continue;

                    var fileName = fileNameEntry.FileName.Filename;
                    var fileRecord = _volume.ReadFileRecord(fileNameEntry.Header.FileReference.FileRecordNumber, true);
                    var fileEntry = new FileModelEntry(fileRecord);

                    if (!sortedList.ContainsValue(fileEntry))
                        sortedList.Add(fileName, fileEntry);
                }
            }

            return sortedList.Values;
        }

        public bool HasChildren(object parent)
        {
            var parentFileRecord = parent as FileModelEntry;

            return parentFileRecord == null ||
                   parentFileRecord.FileRecord.Header.Flags.HasFlag(FileRecord.Flags.IsDirectory);
        }

        private static IEnumerable<FileIndex> GetFileNameIndices(FileRecord fileRecord)
        {
            var indexAlloc =
                fileRecord.FindAttributeByType(AttributeHeaderBase.NTFS_ATTR_TYPE.INDEX_ALLOCATION);

            return indexAlloc == null ? new List<FileIndex>() : (indexAlloc.Body as IndexAllocation)?.ReadFileIndices();
        }
    }
}
