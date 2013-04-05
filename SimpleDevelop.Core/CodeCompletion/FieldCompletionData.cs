using System.Reflection;

namespace SimpleDevelop.CodeCompletion
{
    class FieldCompletionData : CompletionData<FieldInfo>
    {
        public FieldCompletionData(FieldInfo fieldInfo) : base(fieldInfo)
        {
            Image = "FieldImage";
        }

        public override object Description
        {
            get { return string.Format("{0} {1}", _memberInfo.FieldType.Name, _memberInfo.Name); }
        }
    }
}
