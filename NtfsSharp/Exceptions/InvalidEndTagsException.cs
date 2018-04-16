using System;

namespace NtfsSharp.Exceptions
{
    public class InvalidEndTagsException : Exception
    {
        public readonly int InvalidSector;

        public InvalidEndTagsException(int sector)
        {
            InvalidSector = sector;
        }
    }
}
