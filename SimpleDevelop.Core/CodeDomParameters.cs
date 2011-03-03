namespace SimpleDevelop.Core
{
    public class CodeDomParameters
    {
        private string _mainClass;

        public string MainClass
        {
            get { return _mainClass ?? ""; }
            set { _mainClass = value; }
        }
    }
}
