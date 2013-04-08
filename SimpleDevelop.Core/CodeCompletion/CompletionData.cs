using System;
using System.Reflection;

namespace SimpleDevelop.CodeCompletion
{
    public abstract class CompletionData
    {
        public abstract int Rank { get; }
        public abstract string Text { get; }
    }

    public abstract class CompletionData<TMemberInfo> : CompletionData where TMemberInfo : MemberInfo
    {
        protected readonly TMemberInfo _memberInfo;

        public CompletionData(TMemberInfo memberInfo)
        {
            _memberInfo = memberInfo;
        }

        public override string Text
        {
            get { return _memberInfo.Name; }
        }

        public string Image { get; protected set; }

        public virtual double Priority
        {
            get { return 1.0; }
        }

        public virtual object Content
        {
            get { return Text; }
        }

        public virtual object Description
        {
            get { return Text; }
        }

        protected string GetFriendlyTypeName(string typeName)
        {
            switch (typeName)
            {
                case "Void":
                case "String":
                case "Object":
                case "Double":
                case "Decimal": return typeName.ToLower();
                case "Boolean": return "bool";
                case "Int64": return "long";
                case "Int32": return "int";
                case "UInt32": return "uint";
                case "UInt64": return "ulong";
                case "Single": return "float";
            }

            return typeName;
        }
    }
}
