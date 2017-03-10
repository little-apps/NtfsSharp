using System;

namespace NtfsSharp.Exceptions
{
    public class InvalidAttributeException : Exception
    {
        public InvalidAttributeException(string message) : base(message)
        {

        }
    }
}
