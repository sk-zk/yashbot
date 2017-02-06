using System;
using System.Runtime.Serialization;

namespace yashbot
{
    class YashNotFoundException : Exception
    {
        public YashNotFoundException()
        {
        }

        public YashNotFoundException(string message) : base(message)
        {
        }

        public YashNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected YashNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}