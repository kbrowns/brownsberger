using System;

namespace Simple.NH.Exceptions
{
    public class EntityNotFoundException : SimpleNHException
    {
        private readonly string _messageOverride;

        public EntityNotFoundException(Type type, object id)
        {
            _messageOverride = "No entity of type {0} found for id '{1}'".FormatWith(type, id);
        }

        public override string Message
        {
            get { return _messageOverride; }
        }

        public EntityNotFoundException(string message) : base(message)
        {
            _messageOverride = message;
        }

        public EntityNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}