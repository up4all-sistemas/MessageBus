using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Up4All.Framework.MessageBus.RabbitMQ.Extensions
{
    public static class ObjectExtensions
    {
        public static byte[] ToByteArray(this object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static string FromObject(this object obj)
        {
            var x = Convert.ChangeType(obj, typeof(byte[]));
            return Encoding.UTF8.GetString((byte[])x);
        }

        public static object ToObject(this string str)
        {
            var barray = Encoding.UTF8.GetBytes(str);
            return barray.ToObject();
        }

        public static object ToObject(this byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }
    }
}
