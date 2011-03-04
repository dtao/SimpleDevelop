using System.Reflection;

namespace SimpleDevelop.CodeCompletion
{
    class EnumCompletionData : CompletionData
    {
        public EnumCompletionData(MemberInfo memberInfo) : base(memberInfo)
        {
            Image = EnumImage;
        }
    }
}
