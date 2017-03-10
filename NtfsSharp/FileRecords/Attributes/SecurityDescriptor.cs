using static NtfsSharp.FileRecords.Attributes.Base.AttributeHeader;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.Helpers;
using System.Runtime.InteropServices;

namespace NtfsSharp.FileRecords.Attributes
{
    public class SecurityDescriptor : AttributeBodyBase
    {
        public static uint SubHeaderSize => (uint) Marshal.SizeOf<NTFS_SECURITY_DESCRIPTOR>();
        public NTFS_SECURITY_DESCRIPTOR SubHeader { get; private set; }

        // See https://0cch.com/ntfsdoc/attributes/security_descriptor.html for explaination
        public SecurityDescriptor(AttributeHeader header) : base(header)
        {
            SubHeader = Bytes.ToStructure<NTFS_SECURITY_DESCRIPTOR>(CurrentOffset);
            CurrentOffset += SubHeaderSize;

            ReadAcl(SubHeader.SaclOffset);
        }

        private ACL ReadAcl(uint offset)
        {
            var acl = Bytes.ToStructure<ACL>(CurrentOffset + offset);

            return acl;
        }

        public struct ACL
        {
            public readonly byte Revision;
            public readonly byte Padding1;
            public readonly ushort AclSize;
            public readonly ushort AceCount;
            public readonly ushort Padding2;
        }

        public struct NTFS_SECURITY_DESCRIPTOR
        {
            public readonly byte Revision;
            public readonly byte Padding;
            public readonly ushort ControlFlags;
            public readonly uint UserSidOffset;
            public readonly uint GroupSidOffset;
            public readonly uint SaclOffset;
            public readonly uint DaclOffset;
        }
    }
}
