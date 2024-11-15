namespace SlimeFwd
{
    internal class CmdArgumentException : Exception
    {
        public CmdArgumentException() { }

        public CmdArgumentException(string? message)
            : base(message) { }

        public CmdArgumentException(string? message, Exception? innerException)
            : base(message, innerException) { }
    }
}
