﻿using System.Collections;
using System.Collections.Generic;
using NtfsSharp.Files;
using NtfsSharp.Files.Attributes.Base;
using NtfsSharp.Files.Attributes.IndexAllocation;
using NtfsSharp.Files.Attributes.IndexRoot;
using NtfsSharp.PInvoke;

namespace NtfsSharp.Explorer.FileModelEntry.QuickScan
{
    class FileModel : BaseFileModel
    {
        private const uint RootRecordNum = 5;
        
        public FileModel(Volume volume) : base(volume)
        {
        }

        /// <summary>
        /// Gets the children (files and folders) of file record
        /// </summary>
        /// <param name="parent"><see cref="FileModelEntry"/> or null if it is the root directory</param>
        /// <returns>File records contained in <see cref="FileModelEntry"/></returns>
        /// <remarks>
        /// This does not utilize the B+ tree structure of the NTFS properly.
        /// It will use the index allocation. If it doesn't exist, it will attempt to use the index root.
        /// </remarks>
        public override IEnumerable GetChildren(object parent)
        {
            var parentFileModelEntry = parent as FileModelEntry;
            var parentFileRecord = parentFileModelEntry == null
                ? Volume.ReadFileRecord(RootRecordNum, true)
                : parentFileModelEntry.FileRecord;

            var sortedList = new SortedList<string, FileModelEntry>();

            if (parentFileRecord == null)
                return sortedList;

            if (parentFileRecord.HasAttribute(AttributeHeaderBase.NTFS_ATTR_TYPE.INDEX_ALLOCATION))
            {
                foreach (var fileIndex in GetFileIndices(parentFileRecord))
                {
                    foreach (var fileNameEntry in fileIndex.FileNameEntries)
                    {
                        if (fileNameEntry.Header.FileReference.FileRecordNumber ==
                            parentFileRecord.Header.MFTRecordNumber)
                            continue;

                        var fileName = fileNameEntry.FileName.Filename;
                        var fileRecord =
                            Volume.ReadFileRecord(fileNameEntry.Header.FileReference.FileRecordNumber, true);
                        var fileEntry = new FileModelEntry(fileRecord, parentFileModelEntry);

                        if (!sortedList.ContainsValue(fileEntry))
                            sortedList.Add(fileName, fileEntry);
                    }
                }
            }
            else
            {
                foreach (var fileNameIndex in GetFileNameIndices(parentFileRecord))
                {
                    if (fileNameIndex.Header.FileReference.FileRecordNumber == 0)
                        continue;

                    if (fileNameIndex.Header.Flags.HasFlag(Enums.IndexEntryFlags.IsLastEntry))
                        break;

                    var fileName = fileNameIndex.FileName.Filename;
                    var fileRecord = Volume.ReadFileRecord(fileNameIndex.Header.FileReference.FileRecordNumber, true);
                    var fileEntry = new FileModelEntry(fileRecord, parentFileModelEntry);

                    if (!sortedList.ContainsValue(fileEntry))
                        sortedList.Add(fileName, fileEntry);
                }
            }
            
            return sortedList.Values;
        }

        /// <summary>
        /// Checks if specified object has children or is directory
        /// </summary>
        /// <param name="parent"><see cref="FileModelEntry"/> or null if it's the root</param>
        /// <returns>True if the <see cref="FileRecord"/> has the IsDirectory flag</returns>
        public override bool HasChildren(object parent)
        {
            var parentFileRecord = parent as FileModelEntry;

            return parentFileRecord == null ||
                   parentFileRecord.FileRecord.Header.Flags.HasFlag(FileRecord.Flags.IsDirectory);
        }

        public override void Dispose()
        {
            Volume.Dispose();
        }


        /// <summary>
        /// Gets the file indices inside the index allocation attribute
        /// </summary>
        /// <param name="fileRecord"><see cref="FileRecord"/> to get file indices from</param>
        /// <returns>List of file indices (if any)</returns>
        private static IEnumerable<FileIndex> GetFileIndices(FileRecord fileRecord)
        {
            var indexAlloc =
                fileRecord.FindAttributeByType(AttributeHeaderBase.NTFS_ATTR_TYPE.INDEX_ALLOCATION);

            return (indexAlloc.Body as IndexAllocation)?.ReadFileIndices() ?? new List<FileIndex>();
        }

        /// <summary>
        /// Gets the filename indices inside the index root attribute of the file
        /// </summary>
        /// <param name="fileRecord"><see cref="FileRecord"/> to get the filename indices from</param>
        /// <returns>List of filename indices (if any)</returns>
        private static IEnumerable<FileNameIndex> GetFileNameIndices(FileRecord fileRecord)
        {
            var indexRoot = fileRecord.FindAttributeBodyByType(AttributeHeaderBase.NTFS_ATTR_TYPE.INDEX_ROOT) as Root;

            return indexRoot?.FileNameEntries ?? new List<FileNameIndex>();
        }
    }
}
