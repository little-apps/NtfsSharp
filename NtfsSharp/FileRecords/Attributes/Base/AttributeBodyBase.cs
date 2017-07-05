using NtfsSharp.Helpers;

namespace NtfsSharp.FileRecords.Attributes.Base
{
    /// <summary>
    /// Abstract class that is used after the attribute header(s) have been parsed
    /// </summary>
    public abstract class AttributeBodyBase
    {
        public uint CurrentOffset { get; protected set; }

        public readonly AttributeHeaderBase Header;

        public uint OffsetWithHeader => CurrentOffset + Header.Header.Length;

        /// <summary>
        /// The body data (in residual or non-residual space)
        /// </summary>
        public readonly byte[] Body;

        /// <summary>
        /// Constructor for AttributeBodyBase
        /// </summary>
        /// <param name="header"><see cref="AttributeHeaderBase"/> that contains this body</param>
        /// <param name="mustBe">Whether body data must be resident and/or non-resident</param>
        /// <param name="readBody">If false, the data is not read in constructor. Useful for when dealing with large amounts of data.</param>
        protected AttributeBodyBase(AttributeHeaderBase header, bool readBody = true)
        {
            Header = header;
            CurrentOffset = 0;

            if (readBody)
                Body = header.ReadBody();
        }

        protected byte[] GetBytesFromCurrentOffset(uint length)
        {
            return Body.GetBytesAtOffset(CurrentOffset, length);
        }
    }
}
