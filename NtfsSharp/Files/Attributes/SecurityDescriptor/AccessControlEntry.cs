using System;

namespace NtfsSharp.Files.Attributes.SecurityDescriptor
{
    /// <summary>
    /// Access control entries (ACE) are entries in an access control list containing
    /// information describing the access rights related to a particular security
    /// identifier or user. Each access control entry contains an ID, which identifies
    /// the subject group or individual.
    /// </summary>
    public class AccessControlEntry
    {
        /// <summary>
        /// Access Control Entry Header
        /// </summary>
        public ACEHeader Header { get; }

        /// <summary>
        /// Security identifier that access control entry applies to.
        /// </summary>
        public SecurityIdentifier SID { get; }

        public AccessControlEntry(ACEHeader header, SecurityIdentifier sid)
        {
            Header = header;
            SID = sid;
        }

        public struct ACEHeader
        {
            public readonly byte Type;
            public readonly ACEFlags Flags;
            public readonly byte Size;
            public readonly AccessMask AccessMask;
        }

        [Flags]
        public enum ACEFlags : byte
        {
            AccessAllowed = 0,
            AccessDenied = 1,
            SystemAudit = 2
        }

        [Flags]
        public enum AccessMask : uint
        {
            FileReadData = 0x0001,
            FileListDirectory = 0x0001,
            FileWriteData = 0x0002,
            FileAddFile = 0x0002,
            FileAppendData = 0x0004,
            FileAddSubDirectory = 0x0004,
            FileCreatePipeInstance = 0x0004,
            FileReadEa = 0x0008,
            FileWriteEa = 0x0010,
            FileExecute = 0x0020,
            FileTraverse = 0x0020,
            FileDeleteChild = 0x0040,
            FileReadAttributes = 0x0080,
            FileWriteAttributes = 0x0100,

            ReadControl = 0x00020000,

            StandardRightsRequired = 0x000F0000,
            StandardRightsRead = ReadControl,
            StandardRightsWrite = ReadControl,
            StandardRightsExecute = ReadControl,
            Synchronize = 0x00100000,

            FileAllAccess = StandardRightsRequired | Synchronize | 0x1FF,
            FileGenericRead = StandardRightsRead | FileReadData | FileReadAttributes | FileReadEa | Synchronize,
            FileGenericWrite = StandardRightsWrite | FileWriteData | FileWriteAttributes | FileWriteEa | FileAppendData | Synchronize,
            FileGenericExecute = StandardRightsExecute | FileReadAttributes | FileExecute | Synchronize
        }
    }
}
