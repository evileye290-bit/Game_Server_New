using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Logger;

namespace ServerShared
{
    public class HttpHelper
    {
        private static readonly object LockObj = new object();
        private static HttpClient client = null;

        public static HttpClient GetClientInstance()
        {
            if (client == null)
            {
                lock (LockObj)
                {
                    if (client == null)
                    {
                        client = new HttpClient();
                        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 GameServer");
                        client.Timeout = new TimeSpan(5 * TimeSpan.TicksPerSecond);
                    }
                }
            }
            return client;
        }

        public static string PostAsyncWithNoResultWait(Dictionary<string, string> info, string url)
        {
            try
            {
                HttpContent contents = new FormUrlEncodedContent(info);
                contents.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
#if DEBUG
                PostLogin(contents, url);
                //var response = await GetClientInstance().PostAsync(url, contents);
                //var responseString = await response.Content.ReadAsStringAsync();
                //return responseString;
                return "not check";
#else
                GetClientInstance().PostAsync(url, contents);
                return "not check";
#endif
            }
            catch (Exception ex)
            {
                return "PostLoginAsync error" + ex.ToString();
            }
        }

        private static async void PostLogin(HttpContent contents, string url)
        {
            var response = await GetClientInstance().PostAsync(url, contents);
            var responseString = await response.Content.ReadAsStringAsync();

            Log.Info($"login response {responseString}");
        }

        public static async Task<string> PostAsync(string info, string url)
        {
            try
            {
                HttpContent contents = new StringContent(info);
                contents.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                HttpClient client = GetClientInstance();
                var response = await client.PostAsync(url, contents);
                var responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return string.Empty;
            }
        }

        public static async Task<string> GetAsync(Dictionary<string, object> info, string url)
        {
            try
            {
                url = BuildHttpGetUrl(info, url);
                HttpClient client = GetClientInstance();
                var response = await client.GetAsync(url);
                var responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return string.Empty;
            }
        }

        private static string BuildHttpGetUrl(Dictionary<string, object> info, string url)
        { 
            if(info.Count ==0) return url;

            string paramList = string.Join("&", info.Select(x => $"{x.Key}={x.Value}"));
            url = $"{url}?{paramList}";

            return url;
        }

        public static string Sign_SEA(Dictionary<string, string> paramList, string secretKey)
        {
            StringBuilder builder = new StringBuilder();
            paramList.OrderBy(x => x.Key).ForEach(x => builder.Append(x.Value));
            builder.Append(secretKey);

            string tempStr = builder.ToString();
            return EncryptHelper.MD5Encode(tempStr);
        }

        public static string Sign_SEA(Dictionary<string, object> paramList, string secretKey)
        {
            StringBuilder builder = new StringBuilder();
            paramList.OrderBy(x => x.Key).ForEach(x => builder.Append(x.Value));
            builder.Append(secretKey);

            string tempStr = builder.ToString();
            return EncryptHelper.MD5Encode(tempStr);
        }
    }
}
