using System;

namespace SimpleDevelop
{
    public class ReferenceAddedEventArgs : EventArgs
    {
        public ReferenceAddedEventArgs(string reference)
        {
            Reference = reference ?? "";
        }

        public string Reference { get; private set; }
    }
}
