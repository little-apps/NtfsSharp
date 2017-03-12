using System;
using System.Runtime.InteropServices;
using System.Text;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.Helpers;

namespace NtfsSharp.FileRecords.Attributes
{
    public class ExtendedAttribute : AttributeBodyBase
    {
        public readonly NTFS_EA Data;
        public readonly string Name;
        public readonly byte[] Value;

        public ExtendedAttribute(AttributeHeader header) : base(header)
        {
            Data = Body.ToStructure<NTFS_EA>();
            CurrentOffset += (uint) Marshal.SizeOf<NTFS_EA>();

            if (Data.NameLength > 0)
            {
                Name = Encoding.ASCII.GetString(Body, (int) CurrentOffset, Data.NameLength);
                CurrentOffset += Data.NameLength;
            }

            Value = new byte[Data.ValueLength];
            Array.Copy(Body, CurrentOffset, Value, 0, Data.ValueLength);
        }

        public enum Flags : byte
        {
            NeedEA = 0x80
        }

        public struct NTFS_EA
        {
            public readonly uint NextEAOffset;
            public readonly Flags Flags;
            public readonly byte NameLength;
            public readonly ushort ValueLength;
        }
    }
}
