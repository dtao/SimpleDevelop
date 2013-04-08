using System.Reflection;

namespace SimpleDevelop.CodeCompletion
{
    class ConstantCompletionData : CompletionData<FieldInfo>
    {
        public ConstantCompletionData(FieldInfo fieldInfo) : base(fieldInfo)
        {
            Image = "Constant";
        }

        public override object Description
        {
            get { return string.Format("{0} {1}", _memberInfo.FieldType.Name, _memberInfo.Name); }
        }
    }
}
