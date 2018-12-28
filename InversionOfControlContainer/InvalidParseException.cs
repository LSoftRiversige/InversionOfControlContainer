using System;
using System.Runtime.Serialization;

namespace InversionOfControlContainer
{
    [Serializable]
    internal class InvalidParseException : Exception
    {
        public InvalidParseException()
        {
        }

        public InvalidParseException(string message) : base(message)
        {
        }

        public InvalidParseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidParseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}