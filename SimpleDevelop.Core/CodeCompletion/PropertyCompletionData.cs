using System.Reflection;

namespace SimpleDevelop.CodeCompletion
{
    class PropertyCompletionData : CompletionData<PropertyInfo>
    {
        public PropertyCompletionData(PropertyInfo propertyInfo) : base(propertyInfo)
        {
            Image = "Property";
        }

        public override object Description
        {
            get { return string.Format("{0} {1}", _memberInfo.PropertyType.Name, _memberInfo.Name); }
        }
    }
}
