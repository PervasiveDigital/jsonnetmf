using System;
using System.Text;

namespace PervasiveDigital.Json
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class JsonIgnoreAttribute : Attribute
    {
    }
}
