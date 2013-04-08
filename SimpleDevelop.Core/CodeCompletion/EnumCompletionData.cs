using System.Reflection;

namespace SimpleDevelop.CodeCompletion
{
    class EnumCompletionData : CompletionData<MemberInfo>
    {
        public EnumCompletionData(MemberInfo enumInfo) : base(enumInfo)
        {
            Image = "Enum";
        }

        public override int Rank
        {
            get { return 5; }
        }
    }
}
