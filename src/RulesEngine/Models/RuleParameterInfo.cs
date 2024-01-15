using System;
using System.Linq.Expressions;

internal class RuleParameterInfo{
    public Type Type { get; private set; }
    public string Name { get; private set; }
    internal ParameterExpression ParameterExpression { get; private set; }

    public RuleParameterInfo(string name, Type type)
    {
        Name = name;
        Type = type ?? typeof(object);
        ParameterExpression = Expression.Parameter(Type, Name);
    }
}
