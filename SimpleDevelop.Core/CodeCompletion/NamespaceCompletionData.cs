using System.Reflection;

namespace SimpleDevelop.CodeCompletion
{
    class NamespaceCompletionData : CompletionData<MemberInfo>
    {
        private string _namespace;

        public NamespaceCompletionData(string ns) : base(null)
        {
            _namespace = ns;
            Image = "Namespace";
        }

        public override string Text
        {
            get { return _namespace; }
        }
    }
}
