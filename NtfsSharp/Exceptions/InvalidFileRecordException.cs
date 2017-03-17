using System;
using NtfsSharp.FileRecords;

namespace NtfsSharp.Exceptions
{
    class InvalidFileRecordException : Exception
    {
        public readonly string ParamName;
        public readonly FileRecord FileRecord;

        public InvalidFileRecordException(string paramName, FileRecord fileRecord)
        {
            ParamName = paramName;
            FileRecord = fileRecord;
        }

        public InvalidFileRecordException(string paramName, string message, FileRecord fileRecord) : base(message)
        {
            ParamName = paramName;
            FileRecord = fileRecord;
        }
    }
}
