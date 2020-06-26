using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using Newtonsoft.Json;

namespace Music.Netease.Library
{
    public class DynamicContractResolver : DefaultContractResolver
    {
        public Dictionary<string, string>? Renames = null;

        public new NamingStrategy NamingStrategy = new CamelCaseNamingStrategy
        {
            OverrideSpecifiedNames = false
        };

        public void RenameProperty(string propertyName, string jsonPropertyName)
        {
            if (Renames != null)
            {
                Renames[propertyName] = jsonPropertyName;
            }
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (Renames != null
                && property.PropertyName != null
                && Renames.TryGetValue(property.PropertyName, out var jsonPropertyName))
            {
                property.PropertyName = jsonPropertyName;
            }
            return property;
        }
    }
}