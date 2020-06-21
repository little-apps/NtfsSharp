using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.FileRecords.Attributes.MetaData;
using NtfsSharp.Helpers;

namespace NtfsSharp.FileRecords.Attributes.SecurityDescriptor
{
    /// <summary>
    ///   The security descriptor is necessary to prevent unauthorized access to files. It stores information about:
    ///
    ///    The owner of the file
    ///    Permissions the owner has granted to other users
    ///    What actions should be logged (auditing)
    /// </summary>
    [Resident(AttributeHeaderBase.NTFS_ATTR_TYPE.SECURITY_DESCRIPTOR)]
    [NonResident(AttributeHeaderBase.NTFS_ATTR_TYPE.SECURITY_DESCRIPTOR)]
    public sealed class SecurityDescriptor : AttributeBodyBase
    {
        public static uint SubHeaderSize => (uint) Marshal.SizeOf<NTFS_SECURITY_DESCRIPTOR>();
        public NTFS_SECURITY_DESCRIPTOR SubHeader { get; private set; }

        public AccessControlList DiscretionaryAccessControlList =>
            SubHeader.ControlFlags.HasFlag(ControlFlags.DACLPresent) ? ReadAcl(SubHeader.DaclOffset) : null;

        public AccessControlList SystemAccessControlList => SubHeader.ControlFlags.HasFlag(ControlFlags.SACLPresent)
            ? ReadAcl(SubHeader.SaclOffset)
            : null;

        public SecurityIdentifier UserSID => SubHeader.UserSidOffset > 0
            ? SecurityIdentifier.MakeFromBytes(Body, SubHeader.UserSidOffset)
            : null;

        public SecurityIdentifier GroupSID => SubHeader.GroupSidOffset > 0
            ? SecurityIdentifier.MakeFromBytes(Body, SubHeader.GroupSidOffset)
            : null;

        public SecurityDescriptor(AttributeHeaderBase header) : base(header)
        {
            SubHeader = Body.ToStructure<NTFS_SECURITY_DESCRIPTOR>(CurrentOffset);
            CurrentOffset += SubHeaderSize;
        }

        /// <summary>
        /// Reads an access control list starting at offset in body.
        /// </summary>
        /// <param name="offset">Offset of ACL</param>
        /// <returns>Instance of <seealso cref="AccessControlList"/></returns>
        private AccessControlList ReadAcl(uint offset)
        {
            var aclHeader = Body.ToStructure<AccessControlList.ACLHeader>(offset);

            var aceList = new List<AccessControlEntry>();

            var aceStartOffset = (uint) (offset + Marshal.SizeOf<AccessControlList.ACLHeader>());
            var aceLength = (uint) (aclHeader.AclSize - Marshal.SizeOf<AccessControlList.ACLHeader>());

            var aceData = Body.GetBytesAtOffset(aceStartOffset, aceLength);
            uint aceOffset = 0;

            for (var i = 0; i < aclHeader.AceCount; i++)
            {
                var aceHeader = aceData.ToStructure<AccessControlEntry.ACEHeader>(aceOffset);

                var sidOffset = aceOffset + (uint) Marshal.SizeOf<AccessControlEntry.ACEHeader>();

                var sid = SecurityIdentifier.MakeFromBytes(aceData, sidOffset);

                var ace = new AccessControlEntry(aceHeader, sid);
                aceList.Add(ace);

                aceOffset += aceHeader.Size;
            }

            var acl = new AccessControlList(this, aclHeader, aceList.ToArray());

            return acl;
        }

        public struct NTFS_SECURITY_DESCRIPTOR
        {
            public readonly byte Revision;
            public readonly byte Padding;
            public readonly ControlFlags ControlFlags;
            public readonly uint UserSidOffset;
            public readonly uint GroupSidOffset;
            public readonly uint SaclOffset;
            public readonly uint DaclOffset;
        }

        [Flags]
        public enum ControlFlags : ushort
        {
            DACLAutoInheritReq = 0x0100,
            DACLAutoInherited = 0x0400,
            DACLDefaulted = 0x0008,
            DACLPresent = 0x0004,
            DACLProtected = 0x1000,
            GroupDefaulted = 0x0002,
            OwnerDefaulted = 0x0001,
            ResourceManagerControlValid = 0x4000,
            SACLAutoInheritReq = 0x0200,
            SACLAutoInherited = 0x0800,
            SACLDefaulted = 0x0008,
            SACLPresent = 0x0010,
            SACLProtected = 0x2000,
            SelfRelative = 0x8000
        }

        public override string ToString()
        {
            return "$SECURITY_DESCRIPTOR (0x50)";
        }
    }
}
