using System;

namespace SimpleDevelop.Core
{
    public static class ThrowExtensions
    {
        public static void ThrowIfNull<T>(this T obj, string paramName) where T : class
        {
            if (obj == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void ThrowIfOutside<T>(this T value, T min, T max, string paramName) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) >= 0)
            {
                throw new ArgumentOutOfRangeException(paramName);
            }
        }
    }
}
