using System;
using System.Runtime.Serialization;

namespace NtfsSharp.Exceptions
{
    /// <inheritdoc />
    /// <summary>
    /// The parent exception for all exceptions thrown by NtfsSharp
    /// </summary>
    public abstract class NtfsSharpException : Exception
    {
        public NtfsSharpException()
        {
        }

        public NtfsSharpException(string message) : base(message)
        {
            
        }

        public NtfsSharpException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NtfsSharpException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
