using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BarrackServerLib.Server.HttpServer
{
    public class HttpUtils
    {
        //MD5编码
        public static string MD5Encode(string sourceStr)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] src = Encoding.UTF8.GetBytes(sourceStr);
            byte[] res = md5.ComputeHash(src, 0, src.Length);
            return BitConverter.ToString(res).ToLower().Replace("-", "");
        }
        //将参数名从小到大排序，结果如：adfd,bcdr,bff,zx
        public static void sortParamNames(List<string> paramNames)
        {
            paramNames.Sort((string str1, string str2) => { return str1.CompareTo(str2); });
        }
    }
}
