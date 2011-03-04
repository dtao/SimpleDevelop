﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;

using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Ast;
using SimpleDevelop.CodeCompletion;
using SimpleDevelop.Collections;

namespace SimpleDevelop
{
    class CodeCompletionHelper
    {
        struct Loc : IComparable<Loc>
        {
            public readonly int Line;
            public readonly int Column;

            public Loc(int line, int column)
            {
                Line = line;
                Column = column;
            }

            public int CompareTo(Loc other)
            {
                int result = Line.CompareTo(other.Line);
                if (result != 0)
                {
                    return result;
                }

                return Column.CompareTo(other.Column);
            }
        }

        private static readonly ICompletionData[] EmptyData = new ICompletionData[0];

        private SortedList<Loc, Dictionary<string, string>> _fields = new SortedList<Loc, Dictionary<string, string>>();
        private object _fieldsLock = new object();
        private SortedList<Loc, Dictionary<string, string>> _locals = new SortedList<Loc, Dictionary<string, string>>();
        private object _localsLock = new object();
        private ConcurrentDictionary<string, ICompletionData[]> _staticCompletionItems = new ConcurrentDictionary<string, ICompletionData[]>();
        private ConcurrentDictionary<string, ICompletionData[]> _instanceCompletionItems = new ConcurrentDictionary<string, ICompletionData[]>();

        public IList<ICompletionData> GetCompletionData(string token, int line, int column)
        {
            // Is this a static item?
            ICompletionData[] completionData;
            if (_staticCompletionItems.TryGetValue(token, out completionData))
            {
                return Array.AsReadOnly(completionData);
            }

            // Nope... is it a local variable?
            // Ugh, have to lock this...
            string typeName = null;
            lock (_localsLock)
            {
                if (_locals.Count > 0)
                {
                    var loc = new Loc(line, column);
                    int index = _locals.Keys.BinarySearch(loc);
                    if (index > 0)
                    {
                        --index;
                    }

                    Dictionary<string, string> variables = _locals.Values[index];
                    variables.TryGetValue(token, out typeName);
                }
            }

            if (!string.IsNullOrEmpty(typeName))
            {
                if (_instanceCompletionItems.TryGetValue(typeName, out completionData))
                {
                    return Array.AsReadOnly(completionData);
                }
            }

            // Nope! Is it a field???
            // Lock again!
            lock (_fieldsLock)
            {
                if (_fields.Count > 0)
                {
                    var loc = new Loc(line, column);
                    int index = _fields.Keys.BinarySearch(loc);
                    if (index > 0)
                    {
                        --index;
                    }

                    Dictionary<string, string> variables = _fields.Values[index];
                    variables.TryGetValue(token, out typeName);
                }
            }

            if (!string.IsNullOrEmpty(typeName))
            {
                if (_instanceCompletionItems.TryGetValue(typeName, out completionData))
                {
                    return Array.AsReadOnly(completionData);
                }
            }

            return EmptyData;
        }

        public void ProcessCode(string code)
        {
            using (var reader = new StringReader(code))
            using (IParser parser = ParserFactory.CreateParser(SupportedLanguage.CSharp, reader))
            {
                try
                {
                    parser.Parse();

                    // Very naive approach -- just re-parse the entire document.
                    // ...will want to change this eventually.
                    _locals = new SortedList<Loc, Dictionary<string, string>>();

                    ProcessNode(parser.CompilationUnit);
                }
                catch
                { }
            }
        }

        private void ProcessNode(INode node)
        {
            string value = node.ToString();

            var type = node as TypeDeclaration;
            if (type != null)
            {
                ProcessType(type);
            }

            var method = node as MethodDeclaration;
            if (method != null)
            {
                ProcessMethod(method);
                return;
            }

            foreach (INode child in node.Children)
            {
                ProcessNode(child);
            }
        }

        private void ProcessType(TypeDeclaration type)
        {
            var fields = type.Children.OfType<FieldDeclaration>();
            if (fields.Any())
            {
                var variables = new Dictionary<string, string>();
                foreach (FieldDeclaration field in fields)
                {
                    TypeReference typeRef = field.TypeReference;
                    string typeKey = GetTypeKey(typeRef.Type, typeRef.GenericTypes.Count);

                    foreach (VariableDeclaration variable in field.Fields)
                    {
                        variables[variable.Name] = typeKey;
                    }
                }

                Location location = type.StartLocation;
                var loc = new Loc(location.Line, location.Column);
                lock (_fieldsLock)
                {
                    _fields[loc] = variables;
                }
            }
        }

        private void ProcessMethod(MethodDeclaration method)
        {
            ProcessBlock(method.Body);
        }

        private void ProcessBlock(BlockStatement block)
        {
            var locals = block.Children.OfType<LocalVariableDeclaration>();
            if (locals.Any())
            {
                var variables = new Dictionary<string, string>();
                foreach (LocalVariableDeclaration local in locals)
                {
                    TypeReference typeRef = local.TypeReference;
                    string typeKey = GetTypeKey(typeRef.Type, typeRef.GenericTypes.Count);

                    foreach (VariableDeclaration variable in local.Variables)
                    {
                        if (typeKey == "var")
                        {
                            if (variable.Initializer is PrimitiveExpression)
                            {
                                typeKey = GetTypeKey(((PrimitiveExpression)variable.Initializer).Value.GetType());
                            }
                            else if (variable.Initializer is ObjectCreateExpression)
                            {
                                TypeReference variableTypeRef = ((ObjectCreateExpression)variable.Initializer).CreateType;
                                typeKey = GetTypeKey(variableTypeRef.Type, variableTypeRef.GenericTypes.Count);
                            }
                            else if (variable.Initializer is InvocationExpression)
                            {
                                // OK, this case is too complicated to tackle.
                            }
                        }

                        if (typeKey != "var")
                        {
                            variables[variable.Name] = typeKey;
                        }
                    }
                }

                Location location = block.StartLocation;
                var loc = new Loc(location.Line, location.Column);
                lock (_localsLock)
                {
                    _locals[loc] = variables;
                }
            }

            var nestedBlocks = block.Children.OfType<BlockStatement>();
            foreach (BlockStatement nestedBlock in nestedBlocks)
            {
                ProcessBlock(nestedBlock);
            }
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
            BindingFlags staticFlags = BindingFlags.Static | BindingFlags.Public;
            LoadCompletionData(types, staticFlags, _staticCompletionItems);

            BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.Public;
            LoadCompletionData(types, instanceFlags, _instanceCompletionItems);
        }

        private void LoadCompletionData(IEnumerable<Type> types, BindingFlags flags, IDictionary<string, ICompletionData[]> completionItems)
        {
            foreach (Type type in types)
            {
                var completionData = new List<ICompletionData>();

                Type[] nestedTypes = type.GetNestedTypes(flags);
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

                FieldInfo[] fields = type.GetFields(flags);
                if (fields.Length > 0)
                {
                    var constants = from f in fields
                                    where f.IsLiteral
                                    orderby f.Name
                                    select new ConstantCompletionData(f);

                    if (constants.Any())
                    {
                        completionData.AddRange(constants);
                    }

                    var normalFields = from f in fields
                                 where !f.IsLiteral
                                 orderby f.Name
                                 select new CompletionData(f);

                    if (normalFields.Any())
                    {
                        completionData.AddRange(normalFields);
                    }
                }

                EventInfo[] events = type.GetEvents(flags);
                if (events.Length > 0)
                {
                    var allEvents = from e in events
                                    orderby e.Name
                                    select new CompletionData(e);

                    completionData.AddRange(allEvents);
                }

                PropertyInfo[] properties = type.GetProperties(flags);
                if (properties.Length > 0)
                {
                    var allProperties = from p in properties
                                        orderby p.Name
                                        select new CompletionData(p);

                    completionData.AddRange(allProperties);
                }

                MethodInfo[] methods = type.GetMethods(flags);
                if (methods.Length > 0)
                {
                    var allMethods = from m in methods
                                     where !m.IsSpecialName
                                     orderby m.Name
                                     select new CompletionData(m);

                    if (allMethods.Any())
                    {
                        completionData.AddRange(allMethods);
                    }
                }

                if (completionData.Count > 0)
                {
                    var distinctData = from d in completionData
                                       group d by d.Text into g
                                       select g.First();

                    completionItems[GetTypeKey(type)] = distinctData.ToArray();
                }
            }
        }

        private static string GetTypeKey(Type type)
        {
            string typeName = type.FullName;
            return GetTypeKey(typeName);
        }

        private static string GetTypeKey(string typeName, int genericTypeCount = 0)
        {
            string typeKey = typeName;

            int lastPeriodLocation = typeKey.LastIndexOf('.');
            if (lastPeriodLocation != -1)
            {
                typeKey = typeKey.Substring(lastPeriodLocation + 1);
            }

            if (typeKey.Contains('+'))
            {
                typeKey = typeKey.Replace('+', '.');
            }

            if (genericTypeCount > 0)
            {
                typeKey += ("`" + genericTypeCount);
            }

            return typeKey;
        }
    }
}
