using System;
using System.IO;

namespace SimpleDevelop
{
    class Reference : IComparable<Reference>
    {
        public Reference(string filePath)
        {
            FilePath = filePath ?? "";
            FileName = Path.GetFileName(FilePath);
        }

        public string FilePath { get; private set; }
        public string FileName { get; private set; }

        public int CompareTo(Reference other)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(FilePath, other.FilePath);
        }

        public override string ToString()
        {
            return FileName;
        }
    }
}
