using System.Runtime.Serialization;

namespace SlimeVRTrackerProxy
{
    internal class CmdArgumentException : Exception
    {
        public CmdArgumentException() { }

        public CmdArgumentException(string? message)
            : base(message) { }

        public CmdArgumentException(string? message, Exception? innerException)
            : base(message, innerException) { }

        protected CmdArgumentException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
