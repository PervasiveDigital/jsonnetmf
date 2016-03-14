using System;
using System.Text;

namespace PervasiveDigital.Json
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class JsonPropertyAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
