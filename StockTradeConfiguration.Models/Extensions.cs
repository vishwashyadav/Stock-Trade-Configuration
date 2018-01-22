using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace StockTradeConfiguration.Models
{
    public static class Extensions
    {
        public static decimal GetNextValidPrice(this decimal value, bool up)
        {
            if(value % 0.05m ==0)
            {
                return value;
            }
            if (value.ToString().Contains("."))
            {
                var val = value.ToString().Split(new char[] { '.' });
                var last = Convert.ToDecimal(val[1]);
                Divide:
                if (last % 5 != 0)
                {
                    if (up)
                        last++;
                    else last--;
                    goto Divide;
                }
                return Convert.ToDecimal(val[0] + "." + last);
            }
            return value;
        }

        public static decimal FindPercentagValue(this decimal value, decimal percentage)
        {
            return GetNextValidPrice(Math.Round(value * (percentage / 100.0m),2),true);
        }


        public static List<List<T>> GetListItemsInChunk<T>(this IEnumerable<T> symbols, int chunkSize)
        {
            List<List<T>> stockSymbolsList = new List<List<T>>();
            for (int i = 0; i < (symbols.Count() / chunkSize) + 1; i++)
            {
                var tempsymbols = symbols.Skip((i * chunkSize)).Take(chunkSize);
                stockSymbolsList.Add(new List<T>(tempsymbols.Select(s => s)));
            }
            return stockSymbolsList;
        }

        /// <summary>
        /// Creates clone of the supplied object irrespective of thier type.(object may or may not be decorated with serializable attribute)
        /// For Ex: If two different class(types) has same property then it copies the common property value from one object to another
        /// </summary>
        /// <typeparam name="T">Type in which object will be cloned.</typeparam>
        /// <param name="objectFrom">object to be cloned</param>
        /// <returns></returns>
        public static T CloneObject<T>(this object objectFrom)
        {
            if (objectFrom != null)
            {
                T instance = Activator.CreateInstance<T>();
                PropertyInfo[] properties = objectFrom.GetType().GetProperties();
                PropertyInfo[] newProperties = instance.GetType().GetProperties();
                foreach (var item in properties)
                {
                    PropertyInfo newPropInfo = newProperties.FirstOrDefault(s => s.Name == item.Name);
                    if (newPropInfo != null)
                    {
                        if (item.GetMethod != null && newPropInfo.SetMethod != null && item.PropertyType == newPropInfo.PropertyType)
                            newProperties.FirstOrDefault(s => s.Name == item.Name).SetValue(instance, item.GetValue(objectFrom));
                    }
                }
                return instance;
            }
            return default(T);
        }

        /// <summary>
        /// Creates clone of the supplied object irrespective of thier type.(object may or may not be decorated with serializable attribute)
        /// For Ex: If two different class(types) has same property then it copies the common property value from one object to another
        /// </summary>
        /// <typeparam name="T">Type in which object will be cloned.</typeparam>
        /// <param name="objectFrom">object to be cloned</param>
        /// <returns></returns>
        public static T DeepCopy<T>(this object objectFrom)
        {
            if (objectFrom != null)
            {
                T instance = Activator.CreateInstance<T>();

                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, objectFrom);
                    stream.Position = 0;
                    instance = (T)serializer.Deserialize(stream);
                }
                return instance;
            }
            return default(T);
        }

        /// <summary>
        /// Creates clone of the supplied object irrespective of thier type.(object may or may not be decorated with serializable attribute)
        /// For Ex: If two different class(types) has same property then it copies the common property value from one object to another
        /// </summary>
        /// <typeparam name="T">Type in which object will be cloned.</typeparam>
        /// <param name="objectFrom">object to be cloned</param>
        /// <returns></returns>
        public static object CloneObject(this object objectFrom)
        {
            
            if (objectFrom != null)
            {
                var instance = Activator.CreateInstance(objectFrom.GetType());
                PropertyInfo[] properties = objectFrom.GetType().GetProperties();
                PropertyInfo[] newProperties = instance.GetType().GetProperties();
                foreach (var item in properties)
                {
                    PropertyInfo newPropInfo = newProperties.FirstOrDefault(s => s.Name == item.Name);
                    if (newPropInfo != null)
                    {
                        if (item.GetMethod != null && newPropInfo.SetMethod != null && item.PropertyType == newPropInfo.PropertyType)
                            newProperties.FirstOrDefault(s => s.Name == item.Name).SetValue(instance, item.GetValue(objectFrom));
                    }
                }
                return instance;
            }
            return null;
        }

    }
}
