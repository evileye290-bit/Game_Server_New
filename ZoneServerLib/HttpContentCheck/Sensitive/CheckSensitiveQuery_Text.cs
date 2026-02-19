using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class CheckSensitiveQuery_Text : ABoilHttpQuery
    {
        public bool IsSensitive;
        public string Word;

        public int AdvertCheckedResult;
        public int ErrorCode;
        public int Confidence;

        public override void SetResponse(string resultStr)
        {
            base.SetResponse(resultStr);
            if (resultStr != null)
            {
                JObject ret = JObject.Parse(resultStr);
                int code = ret.GetValue("code").ToObject<int>();
                String msg = ret.GetValue("msg").ToObject<string>();
                if (code == 200)
                {
                    JObject dataObject = (JObject)ret["data"];

                    JObject advertCheckedObj = (JObject)dataObject["advertChecked"];
                    AdvertCheckedResult = advertCheckedObj["result"].ToObject<int>();

                    ErrorCode = advertCheckedObj["errCode"].ToObject<int>();

                    //  Confidence = advertCheckedObj["confidence"]?.ToObject<int>();


                    JObject sensitiveCheckedObj = (JObject)dataObject["sensitiveChecked"];
                    IsSensitive = sensitiveCheckedObj["isSensitive"].ToObject<bool>();

                    Word = sensitiveCheckedObj["word"]?.ToObject<string>();
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
