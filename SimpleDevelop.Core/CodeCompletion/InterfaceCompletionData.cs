using System;

namespace SimpleDevelop.CodeCompletion
{
    class InterfaceCompletionData : CompletionData<Type>
    {
        public InterfaceCompletionData(Type type) : base(type)
        {
            Image = "Interface";
        }

        public override int Rank
        {
            get { return 0; }
        }
    }
}
