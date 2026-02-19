using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace PayServerLib
{
    public class VMallSession
    {
        public int SessionUid { get;private set; }
        public HttpListenerContext Context { get; private set; }
        public DateTime ExpireTime { get;private set; }
        public Dictionary<string, object> Dic { get; private set; }
        public Dictionary<string, object> Header { get; private set; }
        public string ApiName { get; private set; }
        public string Version { get; private set; }
        public string Sign { get; private set; }

        public VMallSession(int sessionUid)
        {
            ExpireTime = ServerFrame.BaseApi.now.AddMilliseconds(VMallHelper.Timeout);
            SessionUid = sessionUid;
        }

        protected bool answerd = false;

        public void BindContext(HttpListenerContext context)
        {
            Context = context;
        }


        public void WriteResponse(object answer)
        {
            if (!answerd)
            {
                VMallHelper.WriteResponse(Context, answer);
                answerd = true;
                ExpireTime = ServerFrame.BaseApi.now;
            }
        }

        public void BindParams(string apiName, Dictionary<string, object> dic, Dictionary<string, object> header)
        {
            Dic = dic;
            ApiName = apiName;
            Header = header;
            //Version = version;
            //Data = data;
            //Sign = sign;
        }
    }
}