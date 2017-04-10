using NtfsSharp.FileRecords.Attributes.IndexAllocation;

namespace NtfsSharp.Exceptions
{
    public class InvalidIndexAllocationException : InvalidAttributeException
    {
        public readonly FileIndex FileIndex;

        public InvalidIndexAllocationException(string message) : base(message)
        {
        }

        public InvalidIndexAllocationException(FileIndex fileIndex, string message) : base(message)
        {
            FileIndex = fileIndex;
        }
    }
}
