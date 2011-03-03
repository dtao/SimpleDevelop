using System;

namespace SimpleDevelop.Core
{
    public class BuildOutputEventArgs : EventArgs
    {
        public BuildOutputEventArgs(string message)
        {
            message.ThrowIfNull("message");
            Message = message;
        }

        public string Message { get; private set; }
    }
}
