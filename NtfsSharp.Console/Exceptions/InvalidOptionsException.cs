using System;

namespace NtfsSharp.Console.Exceptions
{
    internal class InvalidOptionsException : Exception
    {
        internal InvalidOptionsException(string message) : base(message)
        {

        }

        internal InvalidOptionsException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
