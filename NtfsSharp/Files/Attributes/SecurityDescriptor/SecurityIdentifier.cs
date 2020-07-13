using System;
using System.Collections.Generic;
using NtfsSharp.Helpers;

namespace NtfsSharp.Files.Attributes.SecurityDescriptor
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
        }

        public int CompareTo(SecurityIdentifier other)
        {
            if (other == null)
                return -1;

            return ReferenceEquals(this, other) ? 0 : GetHashCode().CompareTo(other.GetHashCode());
        }

        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return GetHashCode().Equals(other.GetHashCode());
        }

        public override int GetHashCode()
        {
            var hashCode = -2102657097;
            hashCode = hashCode * -1521134295 + Revision.GetHashCode();
            hashCode = hashCode * -1521134295 + SubAuthorityCount.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(NtAuthority);
            hashCode = hashCode * -1521134295 + EqualityComparer<uint[]>.Default.GetHashCode(SubAuthorities);
            return hashCode;
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

        /// <summary>
        /// Gets the bytes that represent a Security Identifier
        /// </summary>
        /// <param name="sid">Security Identifier</param>
        /// <returns>Bytes representing SID.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="sid"/> is null.</exception>
        /// <remarks>This can be used to create a <seealso cref="System.Security.Principal.SecurityIdentifier"/> instance.</remarks>
        public static byte[] GetBytes(SecurityIdentifier sid)
        {
            if (sid == null)
                throw new ArgumentNullException(nameof(sid));

            var sidSize = 8 + ((uint) sid.SubAuthorityCount * 4);
            var bytes = new byte[sidSize];

            bytes[0] = sid.Revision;
            bytes[1] = sid.SubAuthorityCount;
            Array.Copy(sid.NtAuthority, 0, bytes, 2, sid.NtAuthority.Length);

            for (var i = 0; i < sid.SubAuthorities.Length; i++)
            {
                var subAuthorityBytes = BitConverter.GetBytes(sid.SubAuthorities[i]);

                Array.Copy(subAuthorityBytes, 0, bytes, 8 + i * 4, bytes.Length);
            }

            return bytes;
        }
    }
}
