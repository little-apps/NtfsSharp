using System;
using NtfsSharp.FileRecords;

namespace NtfsSharp.Exceptions
{
    public class InvalidMasterFileTableException : NtfsSharpException
    {
        public readonly string ParamName;
        public readonly FileRecord FileRecord;

        public InvalidMasterFileTableException(string paramName, FileRecord fileRecord)
        {
            ParamName = paramName;
            FileRecord = fileRecord;
        }

        public InvalidMasterFileTableException(string paramName, string message, FileRecord fileRecord) : base(message)
        {
            ParamName = paramName;
            FileRecord = fileRecord;
        }
    }
}
