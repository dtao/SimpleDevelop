using System.Collections.Generic;

namespace SimpleDevelop.Core
{
    public class CodeDomParameters
    {
        private string _mainClass;
        private HashSet<string> _references = new HashSet<string>();

        public string MainClass
        {
            get { return _mainClass ?? ""; }
            set { _mainClass = value; }
        }

        public ICollection<string> References
        {
            get { return _references; }
        }
    }
}
