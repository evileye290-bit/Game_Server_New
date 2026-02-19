using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PayServerLib
{
    public static class EncryptHelper
    {
        //MD5编码
        public static string MD5Encode(string sourceStr)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] src = Encoding.UTF8.GetBytes(sourceStr);
            byte[] res = md5.ComputeHash(src, 0, src.Length);
            return BitConverter.ToString(res).ToLower().Replace("-", "");
        }

        public static string Md5(string str)
        {
            try
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                 byte[] bytValue = System.Text.Encoding.UTF8.GetBytes(str);
                 byte[]  bytHash = md5.ComputeHash(bytValue);

                md5.Clear();
                string sTemp = "";
                for (int i = 0; i < bytHash.Length; i++)
                {
                    sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
                }
                str = sTemp.ToLower();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return str;
        }

        public static string EncodeBase64(string code)
        {
            string encode;
            byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(code);
            try
            {
                encode = Convert.ToBase64String(bytes);
            }
            catch
            {
                encode = code;
            }
            return encode;
        }


        public static string Encode(string src, string key)
        {
            try
            {
                byte[] data = Encoding.Default.GetBytes(src);
                byte[] keys = Encoding.Default.GetBytes(key);
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    int n = (0xff & data[i]) + (0xff & keys[i % keys.Length]);
                    sb.Append("@" + n);
                }
                return sb.ToString();
            }
            catch (Exception e)
            {
                return src;
            }
        }

        public static string Decode(string src, string key)
        {
            if (string.IsNullOrEmpty(src)) return src;

            string pattern = "\\d+";
            MatchCollection results = Regex.Matches(src, pattern);

            ArrayList list = new ArrayList();
            for (int i = 0; i < results.Count; i++)
            {
                try
                {
                    String group = results[i].ToString();
                    list.Add((Object)group);
                }
                catch (Exception e)
                {
                    return src;
                }
            }

            if (list.Count > 0)
            {
                try
                {
                    byte[] data = new byte[list.Count];
                    byte[] keys = System.Text.Encoding.Default.GetBytes(key);

                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = (byte)(Convert.ToInt32(list[i]) - (0xff & Convert.ToInt32(keys[i % keys.Length])));
                    }
                    return System.Text.Encoding.Default.GetString(data);
                }
                catch (Exception e)
                {
                    return src;
                }
            }
            else
            {
                return src;
            }
        }

    }
}
