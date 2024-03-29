﻿using System;
using System.Collections.Generic;
using System.Globalization;
using NtfsSharp.Files;
using NtfsSharp.Files.Attributes;
using NtfsSharp.Files.Attributes.Base;
using NtfsSharp.Files.Attributes.Base.NonResident;

namespace NtfsSharp.Explorer.FileModelEntry.DeepScan
{
    public class FileModelEntry : BaseFileModelEntry
    {
        private readonly ulong _fileRecordNum;
        private FileRecord _fileRecord;

        private string _filename = "(Unknown)";
        private string _dateModified = "(Unknown)";
        private string _actualSize = "(Unknown)";
        private string _allocatedSize = "(Unknown)";
        private string _attributes = "(Unknown)";
        private string _filePath = "";

        public override ulong FileRecordNum
        {
            get { return _fileRecordNum; }
        }

        public override FileRecord FileRecord
        {
            get { return _fileRecord; }
        }

        public override string Filename
        {
            get { return _filename; }
        }

        public override string DateModified
        {
            get { return _dateModified; }
        }

        public override string ActualSize
        {
            get { return _actualSize; }
        }

        public override string AllocatedSize
        {
            get { return _allocatedSize; }
        }

        public override string Attributes
        {
            get { return _attributes; }
        }

        public override string FilePath
        {
            get { return _filePath; }
        }

        public FileModelEntry(ulong fileRecordNum)
        {
            _fileRecordNum = fileRecordNum;
        }

        public void ReadFileRecord(Volume vol)
        {
            _fileRecord = vol.ReadFileRecord(FileRecordNum, true);

            if (!string.IsNullOrEmpty(_fileRecord.Filename))
                _filename = _fileRecord.Filename;

            var standardInfo = _fileRecord.FindAttributeBodyByType(AttributeHeaderBase.NTFS_ATTR_TYPE.STANDARD_INFORMATION) as StandardInformation;

            if (standardInfo != null)
            {
                var fileTime = standardInfo.Data.ModifiedTime;
                var fileTimeLong = ((ulong)fileTime.dwHighDateTime << 32) + (uint)fileTime.dwLowDateTime;

                var dateTime = DateTime.FromFileTimeUtc((long)fileTimeLong);
                _dateModified = dateTime.ToString(CultureInfo.InvariantCulture);

                _attributes = standardInfo.Data.DosPermissions.ToString();
            }

            var dataAttribute = _fileRecord.FindAttributeByType(AttributeHeaderBase.NTFS_ATTR_TYPE.DATA);

            if (dataAttribute == null)
                return;

            if (!dataAttribute.Header.Header.NonResident)
            {
                var residentAttr = dataAttribute.Header as Resident;

                if (residentAttr != null)
                {
                    _actualSize = SizeToString(residentAttr.SubHeader.AttributeLength);
                    _allocatedSize = SizeToString(residentAttr.SubHeader.AttributeLength);
                }
            }
            else
            {
                var nonResidentAttr = dataAttribute.Header as NonResident;

                if (nonResidentAttr != null)
                {
                    _actualSize = SizeToString(nonResidentAttr.SubHeader.AttributeAllocated);
                    _allocatedSize = SizeToString(nonResidentAttr.SubHeader.AttributeSize);
                }
            }


        }

        public void SetFilePath(string prepend)
        {
            _filePath = prepend + "\\" + Filename;
        }
    }


    public class FileModelEntryByFileName : IComparer<FileModelEntry>
    {
        public int Compare(FileModelEntry x, FileModelEntry y)
        {
            if (x?.Filename == null)
                return -1;

            if (y?.Filename == null)
                return 1;

            return string.Compare(x.Filename, y.Filename, StringComparison.Ordinal);
        }
    }
}
