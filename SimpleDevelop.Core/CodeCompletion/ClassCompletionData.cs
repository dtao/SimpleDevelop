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
    }
}
