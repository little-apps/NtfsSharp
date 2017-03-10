using System;

namespace NtfsSharp.Exceptions
{
    class InvalidFileRecordException : Exception
    {
        public readonly string ParamName;

        public InvalidFileRecordException(string paramName)
        {
            ParamName = paramName;
        }

        public InvalidFileRecordException(string paramName, string message) : base(message)
        {
            ParamName = paramName;
        }
    }
}
