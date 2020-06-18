using System;
using NtfsSharp.Helpers;
using SID = System.Security.Principal.SecurityIdentifier;

namespace NtfsSharp.FileRecords.Attributes.SecurityDescriptor
{
    /// <summary>
    /// A data structure of variable length that identifies user, group, and
    /// computer accounts. Every account on a network is issued a unique SID
    /// when the account is first created. Internal processes in Windows refer
    /// to an account's SID rather than the account's user or group name.
    /// </summary>
    public sealed class SecurityIdentifier : IComparable<SecurityIdentifier>
    {
        public byte Revision { get; }
        public byte SubAuthorityCount { get; }
        public byte[] NtAuthority { get; }
        public uint[] SubAuthorities { get; }

        /// <summary>
        /// The <seealso cref="System.Security.Principal.SecurityIdentifier"/> instance for the SID.
        /// </summary>
        public SID SID { get; }

        /// <summary>
        /// Creates an instance of <see cref="SecurityIdentifier"/> using the raw bytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bytes"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="bytes"/> length is less than 8.</exception>
        public SecurityIdentifier(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if (bytes.Length < 8)
                throw new ArgumentOutOfRangeException(nameof(bytes), "Must be at least 8 bytes to parse SID.");

            Revision = bytes[0];
            SubAuthorityCount = bytes[1];

            NtAuthority = bytes.GetBytesAtOffset(2, 6);

            SubAuthorities = new uint[SubAuthorityCount];

            for (var sidPart = 0; sidPart < SubAuthorityCount; sidPart++)
            {
                var partStartOffset = 8 + sidPart * 4;

                SubAuthorities[sidPart] = BitConverter.ToUInt32(bytes, partStartOffset);
            }

            SID = new SID(bytes, 0);
        }

        public int CompareTo(SecurityIdentifier other)
        {
            return SID.CompareTo(other.SID);
        }

        public override bool Equals(object obj)
        {
            return SID.Equals(obj);
        }

        public override int GetHashCode()
        {
            return SID.GetHashCode();
        }

        public override string ToString()
        {
            return SID.ToString();
        }

        /// <summary>
        /// Create <see cref="SecurityIdentifier"/> from offset in bytes.
        /// </summary>
        /// <param name="bytes">Bytes containing SID.</param>
        /// <param name="offset">Offset of SID in bytes.</param>
        /// <returns>Instance of <seealso cref="SecurityIdentifier"/></returns>
        public static SecurityIdentifier MakeFromBytes(byte[] bytes, uint offset)
        {
            var subAuthorityCount = bytes[offset + 1];

            var sidSize = 8 + ((uint) subAuthorityCount * 4);
            var sidBytes = bytes.GetBytesAtOffset(offset, sidSize);

            return new SecurityIdentifier(sidBytes);
        }
    }
}
