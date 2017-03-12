using NtfsSharp.Exceptions;
using NtfsSharp.Helpers;
using System;

namespace NtfsSharp.FileRecords.Attributes.Base
{
    public abstract class AttributeBodyBase : AttributeBase
    {
        public readonly AttributeHeader Header;

        public uint OffsetWithHeader => CurrentOffset + Header.Header.Length;

        /// <summary>
        /// The body data (in residual or non-residual space)
        /// </summary>
        public readonly byte[] Body;

        protected AttributeBodyBase(AttributeHeader header, MustBe mustBe = MustBe.Resident | MustBe.NonResident, bool readBody = true)
        {
            Header = header;
            CurrentOffset = 0;
            
            if (mustBe == MustBe.Resident && header.Header.NonResident)
                throw new InvalidAttributeException("Attribute can only be resident");
            if (mustBe == MustBe.NonResident && !header.Header.NonResident)
                throw new InvalidAttributeException("Attribute can only be non-resident");

            if (readBody)
                Body = header.ReadBody();
        }

        protected byte[] GetBytesFromCurrentOffset(uint length)
        {
            return Body.GetBytesAtOffset(CurrentOffset, length);
        }

        [Flags]
        public enum MustBe
        {
            Resident = 1 << 0,
            NonResident = 1 << 1
        }
    }
}
