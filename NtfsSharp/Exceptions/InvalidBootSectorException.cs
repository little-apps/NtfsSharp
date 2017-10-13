using System;

namespace NtfsSharp.Exceptions
{
    public class InvalidBootSectorException : NtfsSharpException
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
