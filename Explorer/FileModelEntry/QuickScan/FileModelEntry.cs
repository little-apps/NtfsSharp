using System;
using System.Collections.ObjectModel;
using System.Globalization;
using NtfsSharp.FileRecords;
using NtfsSharp.FileRecords.Attributes;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.FileRecords.Attributes.Base.NonResident;

namespace Explorer.FileModelEntry.QuickScan
{
    public class FileModelEntry : BaseFileModelEntry
    {
        private readonly FileRecord _fileRecord;

        public override FileRecord FileRecord
        {
            get { return _fileRecord; }
        }

        public override ulong FileRecordNum
        {
            get { return FileRecord.Header.MFTRecordNumber; }
        }

        public override string Filename
        {
            get
            {
                return !string.IsNullOrEmpty(FileRecord.Filename) ? FileRecord.Filename : "(Unknown)";
            }
        }

        private AttributeBase StandardInfoAttribute
        {
            get { return FileRecord.FindAttributeByType(AttributeHeaderBase.NTFS_ATTR_TYPE.STANDARD_INFORMATION); }
        }

        private AttributeBase DataAttribute
        {
            get { return FileRecord.FindAttributeByType(AttributeHeaderBase.NTFS_ATTR_TYPE.DATA); }
        }

        public override string DateModified
        {
            get
            {
                var standardInfo = StandardInfoAttribute?.Body as StandardInformation;

                if (standardInfo == null)
                    return "(Unknown)";

                var fileTime = standardInfo.Data.ModifiedTime;
                var fileTimeLong = ((ulong)fileTime.dwHighDateTime << 32) + (uint)fileTime.dwLowDateTime;

                var dateTime = DateTime.FromFileTimeUtc((long)fileTimeLong);
                return dateTime.ToString(CultureInfo.InvariantCulture);
            }
        }

        public override string ActualSize
        {
            get
            {
                if (FileRecord.Header.Flags.HasFlag(FileRecord.Flags.IsDirectory))
                    return string.Empty;

                if (DataAttribute == null)
                    return "(Unknown)";

                if (DataAttribute.Header.Header.NonResident)
                {
                    var nonResidentAttr = DataAttribute.Header as NonResident;

                    return nonResidentAttr == null
                        ? "(Unknown)"
                        : BaseFileModelEntry.SizeToString(nonResidentAttr.SubHeader.AttributeAllocated);
                }

                var residentAttr = DataAttribute.Header as Resident;

                return residentAttr == null ? "(Unknown)" : BaseFileModelEntry.SizeToString(residentAttr.SubHeader.AttributeLength);
            }
        }

        public override string AllocatedSize
        {
            get
            {
                if (FileRecord.Header.Flags.HasFlag(FileRecord.Flags.IsDirectory))
                    return string.Empty;

                if (DataAttribute == null)
                    return "(Unknown)";

                if (DataAttribute.Header.Header.NonResident)
                {
                    var nonResidentAttr = DataAttribute.Header as NonResident;

                    return nonResidentAttr == null
                        ? "(Unknown)"
                        : BaseFileModelEntry.SizeToString(nonResidentAttr.SubHeader.AttributeSize);
                }

                var residentAttr = DataAttribute.Header as Resident;

                return residentAttr == null ? "(Unknown)" : BaseFileModelEntry.SizeToString(residentAttr.SubHeader.AttributeLength);
            }
        }

        public override string Attributes
        {
            get
            {
                var standardInfo = StandardInfoAttribute?.Body as StandardInformation;

                if (standardInfo == null)
                    return "(Unknown)";

                return standardInfo.Data.DosPermissions.ToString();
            }
        }

        /// <summary>
        /// Gets the complete file path of the file with a leading backslash (\) and trailing backslash (if it is a directory)
        /// </summary>
        /// <example>\Windows\System32\kernel32.dll</example>
        /// <example>\Windows\System32\</example>
        public override string FilePath
        {
            get
            {
                var filePath = Filename;
                var currentFileModelEntry = ParentFileModelEntry;

                if (FileRecord.Header.Flags.HasFlag(FileRecord.Flags.IsDirectory))
                    filePath += "\\";

                while (currentFileModelEntry != null)
                {
                    filePath = $"{currentFileModelEntry.Filename}\\{filePath}";

                    currentFileModelEntry = currentFileModelEntry.ParentFileModelEntry;
                }

                return $"\\{filePath}";
            }
        }

        public readonly FileModelEntry ParentFileModelEntry;

        public readonly ObservableCollection<FileAttribute> FileAttributes = new ObservableCollection<FileAttribute>();

        public FileModelEntry(FileRecord fileRecord, FileModelEntry parentFileModelEntry)
        {
            _fileRecord = fileRecord;
            ParentFileModelEntry = parentFileModelEntry;

            foreach (var attr in FileRecord.Attributes)
            {
                var attrBody = attr.Body;

                FileAttributes.Add(new FileAttribute {FileRecord = fileRecord, Attribute = attrBody});
            }
        }

        public int Compare(FileModelEntry x, FileModelEntry y)
        {
            if (x == null)
                return -1;

            if (y == null)
                return 1;

            return x.FileRecord.CompareTo(y.FileRecord);
        }

        public bool Equals(FileModelEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return FileRecord.CompareTo(other.FileRecord) == 0;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FileModelEntry) obj);
        }
        
        public class FileAttribute
        {

            public FileRecord FileRecord;
            public AttributeBodyBase Attribute;

            public string Name => Attribute.ToString();
        }
    }
}
