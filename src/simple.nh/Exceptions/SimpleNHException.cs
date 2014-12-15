using System;

namespace Simple.NH.Exceptions
{
    public class SimpleNHException : Exception
    {
        protected SimpleNHException() { }

        public SimpleNHException(string message) : base(message)
        {
        }

        public SimpleNHException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
