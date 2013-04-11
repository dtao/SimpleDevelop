using System;

namespace SimpleDevelop
{
    public static class StringExtensions
    {
        public static string F(this string format, params object[] args)
        {
            return string.Format(format, args);
        }
    }
}
