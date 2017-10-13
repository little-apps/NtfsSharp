using System;

namespace NtfsSharp.Exceptions
{
    public class InvalidAttributeException : NtfsSharpException
    {
        public InvalidAttributeException(string message) : base(message)
        {

        }
    }
}
