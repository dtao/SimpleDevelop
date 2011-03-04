using System;

namespace SimpleDevelop.CodeCompletion
{
    class StructCompletionData : CompletionData<Type>
    {
        public StructCompletionData(Type type) : base(type)
        {
            Image = StructImage;
        }

        public override object Description
        {
            get { return string.Format("struct {0}", _memberInfo.Name); }
        }
    }
}
