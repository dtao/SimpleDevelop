using System;

namespace SimpleDevelop
{
    public class NullInterface : INativeInterface
    {
        public NullInterface()
        {
            Title = "";
        }

        public string Title { get; set; }
    }
}
