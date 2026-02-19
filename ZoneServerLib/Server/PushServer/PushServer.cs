using DataProperty;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class PushServer
    {
        public static string username = "";
        public static string password = "";
        public static string token = "wanxin";
        //public static PushAnswer pushAnswer;

        public static void Init()
        {
            DataList pushConfig = DataListManager.inst.GetDataList("PushConfig");
            Data registerInfo = pushConfig.Get(1);
            username = registerInfo.GetString("Username");
            password = registerInfo.GetString("Password");

            //Console.WriteLine("PushServer.Username={0},PushServer.Password={1}", username, password);
        }

        public static async System.Threading.Tasks.Task Push(string url, Dictionary<string, string> dic)
        {
            dic.Add("token", token);
            string answer = await HttpRequest.GetPushAnswer(url, dic);
            
        }

        //public static string Push(string url, Dictionary<string, string> dic)
        //{
        //    try
        //    {
        //        string result = "ok";
        //        string mytype = "to_all";
        //        foreach (KeyValuePair<string, string> pair in dic)
        //        {
        //            if (pair.Key.Equals("registerId") && !pair.Value.Equals(""))
        //            {
        //                mytype = "one_to_one";
        //            }

        //        }
        //        dic.Add("type", mytype);
        //        Dictionary<string, string> tempDic = new Dictionary<string, string>();
        //        foreach (KeyValuePair<string, string> pair in dic)
        //        {
        //            tempDic.Add(pair.Key, pair.Value);
        //        }
        //        dic.Add("token", token);

        //        IslandHighManager<string> answer = HttpRequest.GetPushAnswer(url, dic);
        //        answer.Wait();
        //        string type = answer.Result;

        //        string temptoken = "";

        //        switch (type)
        //        {
        //            case "no_token":
        //                token = GetToken(url);
        //                result = Push(url, tempDic);
        //                break;
        //            case "token_error":
        //                temptoken = GetToken(url);
        //                if (!temptoken.Equals(""))
        //                {
        //                    token = temptoken;
        //                }
        //                else
        //                {
        //                    throw new Exception("token error");
        //                }
        //                result = Push(url, tempDic);
        //                break;
        //            case "no_op":
        //                temptoken = GetToken(url);
        //                if (!temptoken.Equals(""))
        //                {
        //                    token = temptoken;
        //                }
        //                else
        //                {
        //                    throw new Exception("token error");
        //                }
        //                result = Push(url, tempDic);
        //                break;
        //            case "ok":
        //                break;
        //        }

        //        return result;
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.Log.Write("PushServer.Push Exception" + e);
        //        return "PushServer.Push error";
        //    }
        //}

        //private static string GetToken(string url)
        //{
        //    try
        //    {
        //        Dictionary<string, string> dic = new Dictionary<string, string>();
        //        dic.Add("username", username);
        //        dic.Add("password", password);
        //        dic.Add("type", "get_token");

        //        IslandHighManager<string> answer = HttpRequest.GetPushAnswer(url, dic);
        //        answer.Wait();
        //        //token = answer.Result;
        //        return answer.Result;
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.Log.Write("PushServer.GetToken Exception" + e);
        //        return "";
        //    }
        //}
    }
    //public delegate String PushAnswer(string name);
}
