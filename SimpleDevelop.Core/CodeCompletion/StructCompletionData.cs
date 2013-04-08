using System;

namespace SimpleDevelop.CodeCompletion
{
    class StructCompletionData : CompletionData<Type>
    {
        public StructCompletionData(Type type) : base(type)
        {
            Image = "Struct";
        }

        public override int Rank
        {
            get { return -1; }
        }
    }
}
