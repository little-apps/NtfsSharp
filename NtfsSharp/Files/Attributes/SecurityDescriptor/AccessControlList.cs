using System;

namespace NtfsSharp.Files.Attributes.SecurityDescriptor
{
    /// <summary>
    /// An ACL is an ordered list of ACEs (Access Control Entries) that define the access
    /// attributes that apply to an object and its properties. Each ACE identifies a security
    /// principal (user or group account) and specifies a set of access rights that are allowed,
    /// denied, or audited for that security principal.
    /// </summary>
    public class AccessControlList
    {
        /// <summary>
        /// <seealso cref="SecurityDescriptor"/> that ACL belongs to.
        /// </summary>
        public SecurityDescriptor SecurityDescriptor { get; }

        /// <summary>
        /// Access Control List Header
        /// </summary>
        public ACLHeader Header { get; }

        /// <summary>
        /// Access Control Entries
        /// </summary>
        public AccessControlEntry[] AccessControlEntries { get; }

        /// <summary>
        /// Creates an AccessControlList instance.
        /// </summary>
        /// <param name="securityDescriptor">The <seealso cref="SecurityDescriptor"/> that the ACL belongs to.</param>
        /// <param name="aclHeader">The header for the ACL.</param>
        /// <param name="accessControlEntries">Access control entries that the ACL contains.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="accessControlEntries"/> is null.</exception>
        public AccessControlList(SecurityDescriptor securityDescriptor, ACLHeader aclHeader, AccessControlEntry[] accessControlEntries)
        {
            SecurityDescriptor = securityDescriptor;
            Header = aclHeader;
            AccessControlEntries = accessControlEntries ?? throw new ArgumentNullException(nameof(accessControlEntries));
        }

        public struct ACLHeader
        {
            public readonly byte Revision;
            public readonly byte Padding1;
            public readonly ushort AclSize;
            public readonly ushort AceCount;
            public readonly ushort Padding2;
        }
    }
}
