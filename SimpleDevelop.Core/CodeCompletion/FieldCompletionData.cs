using System.Reflection;

namespace SimpleDevelop.CodeCompletion
{
    class FieldCompletionData : CompletionData<FieldInfo>
    {
        public FieldCompletionData(FieldInfo fieldInfo) : base(fieldInfo)
        {
            Image = "Field";
        }

        public override int Rank
        {
            get { return 6; }
        }
        
        public override object Description
        {
            get { return GetFriendlyTypeName(_memberInfo.FieldType.Name); }
        }
    }
}
