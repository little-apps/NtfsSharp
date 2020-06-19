using System;
using System.Linq;
using System.Runtime.InteropServices;
using NtfsSharp.Helpers;
using System.Collections.Generic;
using NtfsSharp.FileRecords.Attributes;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.FileRecords.Attributes.Base.NonResident;
using NtfsSharp.FileRecords.Attributes.Shared;
using NtfsSharp.Volumes;

namespace NtfsSharp.FileRecords
{
    /// <summary>
    /// Represents a FILE record
    /// </summary>
    public class FileRecord : IComparer<FileRecord>, IComparable<FileRecord>, IEquatable<FileRecord>
    {
        public readonly Volume Volume;

        public FILE_RECORD_HEADER_NTFS Header { get; private set; }
        public readonly List<Attributes.Attribute> Attributes = new List<Attributes.Attribute>();

        /// <summary>
        /// Fixuable instance used on file record (if any)
        /// </summary>
        public Fixupable Fixupable { get; set; }

        public string Filename
        {
            get
            {
                var defaultFilename = string.Empty;

                foreach (
                    var attr in
                    FindAttributesByType(AttributeHeaderBase.NTFS_ATTR_TYPE.FILE_NAME))
                {
                    var fileNameAttr = attr.Body as FileNameAttribute;

                    if (fileNameAttr == null)
                        continue;

                    if (fileNameAttr.FileName.Data.Namespace != FileName.NTFS_NAMESPACE.Dos)
                        return fileNameAttr.FileName.Filename;

                    defaultFilename = fileNameAttr.FileName.Filename;
                }

                return defaultFilename;
            }
        }

        public DataStream FileStream
        {
            get
            {
                var dataAttr = FindAttributeByType(AttributeHeaderBase.NTFS_ATTR_TYPE.DATA);

                if (dataAttr == null)
                    return null;

                return new DataStream(dataAttr);
            }
        }

        /// <summary>
        /// Constructor for FileRecord object
        /// </summary>
        /// <param name="header">File record header</param>
        /// <param name="reader">Reader that read the file record</param>
        /// <remarks>Use the <see cref="Facades.FileRecordAttributesFacade"/> to create a FileRecord object</remarks>
        public FileRecord(FILE_RECORD_HEADER_NTFS header, Volume reader)
        {
            Header = header;
            Volume = reader ?? throw new ArgumentNullException(nameof(reader), "Reader cannot be null");
        }

        /// <summary>
        /// Checks if file record has attribute with type
        /// </summary>
        /// <param name="attrType">Attribute type</param>
        /// <returns>True if file record has any attributes of type</returns>
        public bool HasAttribute(AttributeHeaderBase.NTFS_ATTR_TYPE attrType)
        {
            return Attributes.Any(attr => attr.Header.Header.Type == attrType);
        }

        /// <summary>
        /// Finds attributes with specified type and returns list of ones matching it
        /// </summary>
        /// <param name="attrType">Attribute type</param>
        /// <returns>Matching attributes or empty list if none found</returns>
        public IEnumerable<Attributes.Attribute> FindAttributesByType(AttributeHeaderBase.NTFS_ATTR_TYPE attrType)
        {
            return Attributes.Where(attr => attr.Header.Header.Type == attrType);
        }

        /// <summary>
        /// Finds first attribute with specified type
        /// </summary>
        /// <param name="attrType">Attribute type</param>
        /// <returns>First matching attribute or null none found</returns>
        public Attributes.Attribute FindAttributeByType(AttributeHeaderBase.NTFS_ATTR_TYPE attrType)
        {
            return Attributes.FirstOrDefault(attr => attr.Header.Header.Type == attrType);
        }

        /// <summary>
        /// Finds body of first attribute with specified type
        /// </summary>
        /// <param name="attrType">Attribute type</param>
        /// <returns>First matching attributes body or null none found</returns>
        public AttributeBodyBase FindAttributeBodyByType(AttributeHeaderBase.NTFS_ATTR_TYPE attrType)
        {
            var attr = Attributes.FirstOrDefault(a => a.Header.Header.Type == attrType);

            return attr?.Body;
        }

        /// <summary>
        /// Tries to find an attribute in the file record
        /// </summary>
        /// <param name="attrNum">Attribute number</param>
        /// <param name="attrType">Attribute type</param>
        /// <param name="name">Name to match in attribute</param>
        /// <returns>Matching AttributeBase or null if it wasn't found</returns>
        public Attributes.Attribute FindAttribute(ushort attrNum, AttributeHeaderBase.NTFS_ATTR_TYPE attrType, string name)
        {
            foreach (var attr in Attributes)
            {
                if (attr.Header.Header.Type != attrType)
                    continue;

                if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(attr.Header.Name) &&
                    attr.Header.Header.AttributeID == attrNum)
                    return attr;

                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(attr.Header.Name) &&
                    name == attr.Header.Name)
                    return attr;
            }

            return null;
        }

        [Flags]
        public enum Flags : ushort
        {
            InUse = 1 << 0,
            IsDirectory = 1 << 1
        }

        #region IComparer Implementation
        public int Compare(FileRecord x, FileRecord y)
        {
            if (x == null)
                return -1;

            if (y == null)
                return 1;

            return x.Volume.CompareTo(y.Volume) + (int)(x.Header.MFTRecordNumber - y.Header.MFTRecordNumber);
        }
        #endregion

        #region IComparable Implementation
        public int CompareTo(FileRecord other)
        {
            if (other == null)
                return -1;

            return Compare(this, other);
        }
        #endregion

        #region IEquatable Implementation
        public bool Equals(FileRecord other)
        {
            return CompareTo(other) == 0;
        }
        #endregion

        [StructLayout(LayoutKind.Sequential)]
        public struct FILE_RECORD_HEADER_NTFS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public readonly byte[] Magic;
            public readonly ushort UpdateSequenceOffset;
            public readonly ushort UpdateSequenceSize;
            public readonly ulong LogFileSequenceNumber;
            public readonly ushort SequenceNumber;
            public readonly ushort HardLinkCount;
            public readonly ushort FirstAttributeOffset;
            public readonly Flags Flags;
            public readonly uint UsedSize;
            public readonly uint AllocateSize;
            public readonly ulong FileReference;
            public readonly ushort NextAttributeID;
            public readonly ushort Align;
            public readonly uint MFTRecordNumber;
        }
    }
}
