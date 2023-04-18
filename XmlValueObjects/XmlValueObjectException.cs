using System;
using System.Runtime.Serialization;

namespace XFrame.ValueObjects.XmlValueObjects
{
    [Serializable]
    public class XmlValueObjectException : Exception
    {
        #region Constructors
        public XmlValueObjectException() : base() { }

        public XmlValueObjectException(string message) : base(message) { }

        public XmlValueObjectException(string message, Exception innerExceptions) : base(message, innerExceptions) { }

        protected XmlValueObjectException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }

        #endregion
    }
}
