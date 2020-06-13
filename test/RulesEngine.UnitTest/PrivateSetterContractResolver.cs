using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RulesEngine.UnitTest
{
    /// <summary>Class PrivateSetterContractResolver.
    /// Implements the <see cref="Newtonsoft.Json.Serialization.DefaultContractResolver" /></summary>
    public class PrivateSetterContractResolver : DefaultContractResolver
    {
        /// <summary>Creates a <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> for the given <see cref="T:System.Reflection.MemberInfo">MemberInfo</see>.</summary>
        /// <param name="member">The member to create a <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> for.</param>
        /// <param name="memberSerialization">The member's parent <see cref="T:Newtonsoft.Json.MemberSerialization" />.</param>
        /// <returns>A created <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> for the given <see cref="T:System.Reflection.MemberInfo">MemberInfo</see>.</returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jsonProperty = base.CreateProperty(member, memberSerialization);
            if (!jsonProperty.Writable)
            {
                if (member is PropertyInfo propertyInfo)
                {
                    jsonProperty.Writable = propertyInfo.GetSetMethod(true) != null;
                }
            }

            return jsonProperty;
        }
    }
}
