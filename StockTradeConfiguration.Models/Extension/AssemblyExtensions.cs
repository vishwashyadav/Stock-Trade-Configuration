using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Models.Extension
{
    public static class AssemblyExtensions
    {
        public static List<KeyValuePair<T, Type>> GetAllClassByAttribute<T>(this IEnumerable<Assembly> assemblies) where T : Attribute
        {
            List<KeyValuePair<T, Type>> attributes = new List<KeyValuePair<T, Type>>();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes().Where(s => s.CustomAttributes.Any(c => c.AttributeType == typeof(T)));
                foreach (var jobbingTpe in types)
                {
                    var attrib = jobbingTpe.GetCustomAttributes(true).FirstOrDefault(s => s.GetType() == typeof(T));
                    if (attrib != null)
                    {
                        attributes.Add(new KeyValuePair<T, Type>(attrib as T, jobbingTpe));
                    }
                }
            }
            return attributes;
        }
    }
}
