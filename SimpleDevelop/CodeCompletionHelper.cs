using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

using ICSharpCode.AvalonEdit.CodeCompletion;
using SimpleDevelop.CodeCompletion;

namespace SimpleDevelop
{
    class CodeCompletionHelper
    {
        private static readonly ICompletionData[] EmptyData = new ICompletionData[0];

        private ConcurrentDictionary<string, ICompletionData[]> _completionItems = new ConcurrentDictionary<string, ICompletionData[]>();

        public IList<ICompletionData> GetCompletionData(string token)
        {
            ICompletionData[] completionData;
            if (_completionItems.TryGetValue(token, out completionData))
            {
                return Array.AsReadOnly(completionData);
            }

            return EmptyData;
        }

        public void AddReference(string assemblyPath)
        {
            Action<string> loadReference = LoadReference;
            loadReference.BeginInvoke(assemblyPath, loadReference.EndInvoke, null);
        }

        private void LoadReference(string assemblyPath)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(assemblyPath);

                var publicTypes = from t in assembly.GetTypes()
                                  where t.IsPublic
                                  select t;

                LoadTypes(publicTypes);
            }
            catch
            { }
        }

        private void LoadTypes(IEnumerable<Type> types)
        {
            foreach (Type type in types)
            {
                var completionData = new List<ICompletionData>();

                BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public;

                Type[] nestedTypes = type.GetNestedTypes(bindingFlags);
                if (nestedTypes.Length > 0)
                {
                    var enums = from t in nestedTypes
                                where t.IsEnum
                                orderby t.Name
                                select new EnumCompletionData(t);

                    if (enums.Any())
                    {
                        completionData.AddRange(enums);
                    }

                    var structs = from t in nestedTypes
                                  where t.IsValueType && !t.IsEnum
                                  orderby t.Name
                                  select new StructCompletionData(t);

                    if (structs.Any())
                    {
                        completionData.AddRange(structs);
                    }

                    var classes = from t in nestedTypes
                                  where t.IsClass
                                  orderby t.Name
                                  select new CompletionData(t);

                    if (classes.Any())
                    {
                        completionData.AddRange(classes);
                    }

                    var interfaces = from t in nestedTypes
                                     where t.IsInterface
                                     orderby t.Name
                                     select new CompletionData(t);

                    if (interfaces.Any())
                    {
                        completionData.AddRange(interfaces);
                    }

                    LoadTypes(nestedTypes);
                }

                FieldInfo[] staticFields = type.GetFields(bindingFlags);
                if (staticFields.Length > 0)
                {
                    var constants = from f in staticFields
                                    where f.IsLiteral
                                    orderby f.Name
                                    select new ConstantCompletionData(f);

                    if (constants.Any())
                    {
                        completionData.AddRange(constants);
                    }

                    var fields = from f in staticFields
                                 where !f.IsLiteral
                                 orderby f.Name
                                 select new CompletionData(f);

                    if (fields.Any())
                    {
                        completionData.AddRange(fields);
                    }
                }

                EventInfo[] staticEvents = type.GetEvents(bindingFlags);
                if (staticEvents.Length > 0)
                {
                    var events = from e in staticEvents
                                 orderby e.Name
                                 select new CompletionData(e);

                    completionData.AddRange(events);
                }

                PropertyInfo[] staticProperties = type.GetProperties(bindingFlags);
                if (staticProperties.Length > 0)
                {
                    var properties = from p in staticProperties
                                     orderby p.Name
                                     select new CompletionData(p);

                    completionData.AddRange(properties);
                }

                MethodInfo[] staticMethods = type.GetMethods(bindingFlags);
                if (staticMethods.Length > 0)
                {
                    var methods = from m in staticMethods
                                  where !m.IsSpecialName
                                  orderby m.Name
                                  select new CompletionData(m);

                    if (methods.Any())
                    {
                        completionData.AddRange(methods);
                    }
                }

                if (completionData.Count > 0)
                {
                    var distinctData = from d in completionData
                                       group d by d.Text into g
                                       select g.First();

                    _completionItems[GetTypeKey(type)] = distinctData.ToArray();
                }
            }
        }

        private static string GetTypeKey(Type type)
        {
            string typeKey = type.FullName;

            int lastPeriodLocation = typeKey.LastIndexOf('.');
            if (lastPeriodLocation != -1)
            {
                typeKey = typeKey.Substring(lastPeriodLocation + 1);
            }

            if (typeKey.Contains('+'))
            {
                typeKey = typeKey.Replace('+', '.');
            }

            return typeKey;
        }
    }
}
