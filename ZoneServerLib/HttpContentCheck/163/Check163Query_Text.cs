using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class Check163Query_Text : ABoilHttpQuery
    {
        public String taskId;
        public int action = -1;
        public JArray labelArray;

        public override void SetResponse(string result)
        {
            base.SetResponse(result);
            if (result != null)
            {
                JObject ret = JObject.Parse(result);
                int code = ret.GetValue("code").ToObject<Int32>();
                String msg = ret.GetValue("msg").ToObject<String>();
                if (code == 200)
                {
                    JObject resultObject = (JObject)ret["result"];
                    JObject antispamObj = (JObject)resultObject["antispam"];
                    taskId = antispamObj["taskId"].ToObject<String>();
                    action = antispamObj["action"].ToObject<Int32>();                   
                    labelArray = (JArray)antispamObj.SelectToken("labels");
                    //if (action == 0)
                    //{
                    //    Console.WriteLine(String.Format("taskId={0}，文本机器检测结果：通过", taskId));
                    //}
                    //else if (action == 1)
                    //{
                    //    Console.WriteLine(String.Format("taskId={0}，文本机器检测结果：嫌疑，需人工复审，分类信息如下：{1}", taskId, labelArray));
                    //}
                    //else if (action == 2)
                    //{
                    //    Console.WriteLine(String.Format("taskId={0}，文本机器检测结果：不通过，分类信息如下：{1}", taskId, labelArray));
                    //}
                }
                else
                {
                    Console.WriteLine(String.Format("ERROR: code={0}, msg={1}", code, msg));
                }
            }
            else
            {
                Console.WriteLine("Request failed!");
            }
        }
    }
}
