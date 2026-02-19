using System.Collections.Generic;
using System.Net;

namespace PayServerLib.Server.SDK
{
    public class PayMessage
    {
        public string Data { get; set; }
        public HttpListenerContext Context { get; set; }
        public Dictionary<string, string> Info { get; set; }
        public Dictionary<string, object> OriginalInfo { get; set; }
        public int ServerId { get; set; }
    }
}
