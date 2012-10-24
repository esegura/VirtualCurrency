using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Collections.Generic;

namespace CommonUtils
{
    /// <summary>
    /// Summary description for JSONHelper
    /// </summary>
    public class JSONHelper
    {
        private static Encoding encoding = Encoding.UTF8;

        public static string Serialize<T>(T obj)
        {
            return Serialize(obj, new Type[] {});
        }

        public static string Serialize<T>(T obj, IEnumerable<Type> knownTypes)
        {
            if ((typeof(T).IsClass) && (obj == null))
                throw new ArgumentNullException();

            using (MemoryStream ms = new MemoryStream())
            {
                Serialize<T>(obj, ms, knownTypes);
                return encoding.GetString(ms.ToArray());
            }
        }

        public static Stream Serialize<T>(T obj, Stream stream)
        {
            return Serialize(obj, stream, new Type[] {});
        }

        public static Stream Serialize<T>(T obj, Stream stream, IEnumerable<Type> knownTypes)
        {
            if ((typeof(T).IsClass) && (obj == null))
                throw new ArgumentNullException("obj");

            if ((stream == null) || (!stream.CanWrite))
                throw new ArgumentNullException("stream");

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T), knownTypes);
            serializer.WriteObject(stream, obj);
            return stream;
        }

        public static T Deserialize<T>(string json)
        {
            return Deserialize<T>(json, new Type[] {});
        }

        public static T Deserialize<T>(string json, IEnumerable<Type> knownTypes)
        {
            using (MemoryStream ms = new MemoryStream(encoding.GetBytes(json)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T), knownTypes);
                return (T)serializer.ReadObject(ms);
            }
        }


    }
}