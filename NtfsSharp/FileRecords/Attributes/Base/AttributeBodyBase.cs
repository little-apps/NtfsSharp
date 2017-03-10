using NtfsSharp.Exceptions;
using System;

namespace NtfsSharp.FileRecords.Attributes.Base
{
    public abstract class AttributeBodyBase : AttributeBase
    {
        public MustBe TypeMustBe { get; } = MustBe.Resident | MustBe.NonResident;

        public readonly AttributeHeader Header;

        protected AttributeBodyBase(AttributeHeader header) : base(header.Bytes)
        {
            Header = header;
            CurrentOffset = header.CurrentOffset;

            if (TypeMustBe.HasFlag(MustBe.Resident) && TypeMustBe.HasFlag(MustBe.NonResident))
                return;

            if (TypeMustBe == MustBe.Resident && header.Header.NonResident)
                throw new InvalidAttributeException("Attribute can only be resident");
            if (TypeMustBe == MustBe.NonResident && !header.Header.NonResident)
                throw new InvalidAttributeException("Attribute can only be non-resident");
        }

        [Flags]
        public enum MustBe
        {
            Resident,
            NonResident
        }
    }
}
