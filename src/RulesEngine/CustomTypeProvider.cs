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
        private readonly bool _enableAssemblyScanning;

        public CustomTypeProvider(Type[] types) : this(types, true)
        {
        }

        public CustomTypeProvider(Type[] types, bool enableAssemblyScanning) : base(ParsingConfig.Default)
        {
            _enableAssemblyScanning = enableAssemblyScanning;
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

        private HashSet<Type> _mergedTypes;

        public override HashSet<Type> GetCustomTypes()
        {
            // base.GetCustomTypes() scans every assembly in the AppDomain for [DynamicLinqType].
            // The provider's type set is fixed after construction, so merge exactly once.
            if (_mergedTypes == null)
            {
                var all = _enableAssemblyScanning
                    ? new HashSet<Type>(base.GetCustomTypes())
                    : new HashSet<Type>();
                all.UnionWith(_types);
                _mergedTypes = all;
            }
            return _mergedTypes;
        }
    }
}
