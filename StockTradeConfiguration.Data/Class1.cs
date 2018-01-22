using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace StockTradeConfiguration.Data
{
    public class XSerializer
    {
        private static XSerializer _instance = new XSerializer();

        public static XSerializer Instance
        {
            get { return _instance; }
        }


        private XSerializer()
        {

        }

        public void SaveConfiguration<T>(string path, T objectToSerialize)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StreamWriter writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, objectToSerialize);
            }
        }

        public T GetConfiguration<T>(string path)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (StreamReader reader = new StreamReader(path))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {

                return default(T);
            }
        }
    }
}
