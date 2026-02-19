using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{

    /// <summary>
    /// 默认的FormUrlEncodedContent碰到超长的文本会出现uri too long的异常，这里自己封装一个
    /// 参考来自 stackoverflow
    /// </summary>
    public class TextFromUrlEncodeContent : ByteArrayContent
    {
        public TextFromUrlEncodeContent(IEnumerable<KeyValuePair<string, string>> nameValueCollection)
            : base(TextFromUrlEncodeContent.GetContentByteArray(nameValueCollection))
        {
            base.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
        }
        private static byte[] GetContentByteArray(IEnumerable<KeyValuePair<string, string>> nameValueCollection)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> current in nameValueCollection)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append('&');
                }

                stringBuilder.Append(TextFromUrlEncodeContent.Encode(current.Key));
                stringBuilder.Append('=');
                stringBuilder.Append(TextFromUrlEncodeContent.Encode(current.Value));
            }
            return Encoding.Default.GetBytes(stringBuilder.ToString());
        }

        private static string Encode(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return string.Empty;
            }
            return WebUtility.UrlEncode(data);
        }
    }
}
