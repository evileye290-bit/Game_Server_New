using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
namespace ServerShared
{
    public class HttpRequest
    {
        public static async Task SendPost(string url, Dictionary<string, string> paramList)
        {
            using (var client = new HttpClient())
            {
                var values = new List<KeyValuePair<string, string>>();
                foreach (var item in paramList)
                {
                    values.Add(new KeyValuePair<string, string>(item.Key, item.Value));
                }
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(url, content);
                string responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseString);
            }
        }

        //后边需要根据业务调整内容，比如token的获取过程
        public static async Task Get(string url, Dictionary<string, string> dic)
        {
            using (var client = new HttpClient())
            {
                StringBuilder builder = new StringBuilder(url);
                builder.Append("?");
                int i = 0;
                foreach (var item in dic)
                {
                    if (i > 0)
                        builder.Append("&");
                    string content= HttpUtility.UrlEncode(item.Value);
                    builder.AppendFormat("{0}={1}", item.Key, content);
                    i++;
                }
                string uri = builder.ToString();

                var response = await client.GetAsync(uri);
                string responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseString);
            }
        }
        //推送并且获取answer
        public static async Task<string> GetPushAnswer(string url, Dictionary<string, string> dic)
        {
            using (var client = new HttpClient())
            {
                StringBuilder builder = new StringBuilder(url);
                builder.Append("?");
                int i = 0;
                foreach (var item in dic)
                {
                    if (i > 0)
                        builder.Append("&");
                    string content = HttpUtility.UrlEncode(item.Value);
                    builder.AppendFormat("{0}={1}", item.Key, content);
                    i++;
                }
                string uri = builder.ToString().Replace("\r\n","");

                var response = await client.GetAsync(uri);
                string responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseString);
                return responseString;
            }
        }
    }
}
