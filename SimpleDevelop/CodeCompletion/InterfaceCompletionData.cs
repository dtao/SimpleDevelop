using System;

namespace SimpleDevelop.CodeCompletion
{
    class InterfaceCompletionData : CompletionData<Type>
    {
        public InterfaceCompletionData(Type type) : base(type)
        {
            Image = InterfaceImage;
        }

        public override object Description
        {
            get { return string.Format("interfae {0}", _memberInfo.Name); }
        }
    }
}
