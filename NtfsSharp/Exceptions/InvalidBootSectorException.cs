using System;

namespace NtfsSharp.Exceptions
{
    public class InvalidBootSectorException : Exception
    {
        public readonly string FieldName;
        public readonly string Message;

        public InvalidBootSectorException(string fieldName, string message)
        {
            FieldName = fieldName;
            Message = message;
        }
    }
}
