// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.HelperFunctions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;

namespace RulesEngine
{
    public class CustomTypeProvider : DefaultDynamicLinqCustomTypeProvider
    {
        private readonly HashSet<Type> _types;

        public CustomTypeProvider(Type[] types) : base(ParsingConfig.Default)
        {
            _types = new HashSet<Type>(types ?? Array.Empty<Type>());

            _types.Add(typeof(ExpressionUtils));

            _types.Add(typeof(Enumerable));

            var queue = new Queue<Type>(_types);
            while (queue.Count > 0)
            {
                var t = queue.Dequeue();

                var baseType = t.BaseType;
                if (baseType != null && _types.Add(baseType))
                    queue.Enqueue(baseType);

                foreach (var interfaceType in t.GetInterfaces())
                {
                    if (_types.Add(interfaceType))
                        queue.Enqueue(interfaceType);
                }
            }

            _types.Add(typeof(IEnumerable));
        }

        public override HashSet<Type> GetCustomTypes()
        {
            var all = new HashSet<Type>(base.GetCustomTypes());
            all.UnionWith(_types);
            return all;
        }
    }
}
