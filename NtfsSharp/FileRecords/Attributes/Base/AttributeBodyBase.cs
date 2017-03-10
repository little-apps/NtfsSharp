using NtfsSharp.Exceptions;
using NtfsSharp.Helpers;
using System;

namespace NtfsSharp.FileRecords.Attributes.Base
{
    public abstract class AttributeBodyBase : AttributeBase
    {
        public MustBe TypeMustBe { get; } = MustBe.Resident | MustBe.NonResident;

        public readonly AttributeHeader Header;

        public uint OffsetWithHeader => CurrentOffset + Header.Header.Length;

        /// <summary>
        /// The body data (in residual or non-residual space)
        /// </summary>
        public readonly byte[] Body;

        protected AttributeBodyBase(AttributeHeader header)
        {
            Header = header;
            CurrentOffset = 0;

            if (header.ReadData)
                Body = header.BodyData;

            if (TypeMustBe.HasFlag(MustBe.Resident) && TypeMustBe.HasFlag(MustBe.NonResident))
                return;

            if (TypeMustBe == MustBe.Resident && header.Header.NonResident)
                throw new InvalidAttributeException("Attribute can only be resident");
            if (TypeMustBe == MustBe.NonResident && !header.Header.NonResident)
                throw new InvalidAttributeException("Attribute can only be non-resident");
        }

        protected byte[] GetBytesFromCurrentOffset(uint length)
        {
            return Body.GetBytesAtOffset(CurrentOffset, length);
        }

        [Flags]
        public enum MustBe
        {
            Resident,
            NonResident
        }
    }
}
