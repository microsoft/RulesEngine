// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.HelperFunctions;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;

namespace RulesEngine;

public class CustomTypeProvider : DefaultDynamicLinqCustomTypeProvider
{
    private readonly HashSet<Type> _types;

    /// <inheritdoc />
    public CustomTypeProvider(ParsingConfig config, Type[] types) : base(config)
    {
        _types = [
            ..types ?? [],
            typeof(ExpressionUtils)
        ];
    }

    public override HashSet<Type> GetCustomTypes()
    {
        return _types;
    }
}
