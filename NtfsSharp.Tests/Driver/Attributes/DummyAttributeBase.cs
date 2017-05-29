using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace NtfsSharp.Tests.Driver.Attributes
{
    public abstract class DummyAttributeBase : Byteable
    {
        public NTFS_ATTRIBUTE_HEADER Header;
        public readonly byte[] NameBytes;

        protected uint HeaderLength => (uint) (Marshal.SizeOf(Header) + NameBytes.Length * 2);

        /// <summary>
        /// Any additional clusters that are associated with this cluster. These are usually needed for non-resident attributes.
        /// </summary>
        public readonly SortedDictionary<ulong, BaseDriverCluster> AdditionalClusters = new SortedDictionary<ulong, BaseDriverCluster>();

        protected DummyAttributeBase(string name = null)
        {
            if (!string.IsNullOrEmpty(name))
            {
                NameBytes = Encoding.Unicode.GetBytes(name);
                Header.NameLength = (byte) (NameBytes.Length / 2);
                Header.NameOffset = (ushort) Marshal.SizeOf(Header);
            }
            else
            {
                NameBytes = new byte[0];
            }
        }

        /// <summary>
        /// Gets the bytes for the header
        /// </summary>
        /// <param name="bodyLength">The total number of bytes used by the (resident) attribute data (not including the header).</param>
        /// <returns>Header as bytes</returns>
        protected byte[] GetHeaderBytes(uint bodyLength)
        {
            Header.Length = HeaderLength + bodyLength;

            var bytes = new byte[HeaderLength];

            var headerSize = Marshal.SizeOf(Header);
            var ptr = Marshal.AllocHGlobal(headerSize);

            // Get NTFS_ATTRIBUTE_HEADER
            Marshal.StructureToPtr(Header, ptr, true);
            Marshal.Copy(ptr, bytes, 0, headerSize);
            Marshal.FreeHGlobal(ptr);

            // If theres a name -> append it
            if (NameBytes != null && NameBytes.Length > 0)
                Array.Copy(NameBytes, 0, bytes, headerSize, NameBytes.Length);

            return bytes;
        }

        /// <summary>
        /// Gets the header data for the attribute
        /// </summary>
        /// <returns></returns>
        public abstract byte[] GetData();

        public enum NTFS_ATTR_TYPE : uint
        {
            STANDARD_INFORMATION = 0x10,
            ATTRIBUTE_LIST = 0x20,
            FILE_NAME = 0x30,
            OBJECT_ID = 0x40,
            SECURITY_DESCRIPTOR = 0x50,
            VOLUME_NAME = 0x60,
            VOLUME_INFORMATION = 0x70,
            DATA = 0x80,
            INDEX_ROOT = 0x90,
            INDEX_ALLOCATION = 0xA0,
            BITMAP = 0xB0,
            REPARSE_POINT = 0xC0,
            EA_INFORMATION = 0xD0,
            EA = 0xE0,
            PROPERTY_SET = 0xF0,
            LOGGED_UTILITY_STREAM = 0x100
        }

        [Flags]
        public enum Flags : ushort
        {
            IsCompressed = 1 << 0,
            // Skip 13 bits
            IsEncrypted = 1 << 14,
            IsSparse = 1 << 15
        }

        public struct NTFS_ATTRIBUTE_HEADER
        {
            public NTFS_ATTR_TYPE Type;
            public uint Length;
            [MarshalAs(UnmanagedType.U1)]
            public bool NonResident;
            public byte NameLength;
            public ushort NameOffset;
            public Flags Flags;
            public ushort AttributeID;
        }
    }
}
