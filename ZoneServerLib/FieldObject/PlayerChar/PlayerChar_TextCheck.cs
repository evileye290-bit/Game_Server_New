using EnumerateUtility;
using ServerModels;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        public Dictionary<String, String> Get163TextCheckParameters(Context163Type type, String context)
        {
            Dictionary<String, String> data = server.http163Helper.CreateTextCheckParameters(this, context, type);

            string signature = Http163CheckerHelper.GenSignature(data);
            data.Add("signature", signature);
            return data;
        }


        public Dictionary<String, String> GetSensitiveTextCheckParameters(String context,string chatChannelName,string chatChannelType,string toUid = null)
        {
            Dictionary<String, String> data = server.httpSensitiveHelper.CreateTextCheckParameters(this, context, chatChannelName,chatChannelType, toUid);
            return data;
        }
    }
}
