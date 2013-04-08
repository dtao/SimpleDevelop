using System;

namespace SimpleDevelop.CodeCompletion
{
    class ClassCompletionData : CompletionData<Type>
    {
        public ClassCompletionData(Type type) : base(type)
        {
            Image = "NestedTypeImage";
        }

        public override int Rank
        {
            get { return -2; }
        }
        
        public override object Description
        {
            get { return string.Format("class {0}", _memberInfo.Name); }
        }
    }
}
