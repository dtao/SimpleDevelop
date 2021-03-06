﻿using System.Reflection;

namespace SimpleDevelop.CodeCompletion
{
    class PropertyCompletionData : CompletionData<PropertyInfo>
    {
        public PropertyCompletionData(PropertyInfo propertyInfo) : base(propertyInfo)
        {
            Image = "Property";
        }

        public override int Rank
        {
            get { return 2; }
        }

        public override object Description
        {
            get { return GetFriendlyTypeName(_memberInfo.PropertyType.Name); }
        }
    }
}
