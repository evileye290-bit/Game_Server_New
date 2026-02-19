using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GlobalServerLib
{
    public class GmSession : AHttpSession
    {
        public GmSession(SessionType sessionType):base(sessionType)
        {
        }

        public string MD5Encode(string sourceStr)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] src = Encoding.UTF8.GetBytes(sourceStr);
            byte[] res = md5.ComputeHash(src, 0, src.Length);
            return BitConverter.ToString(res).Replace("-", "").ToLower();
        }

        internal override bool CheckToken(string userToken)
        {
            if (userToken != null)
            {
                string tempPass = MD5Encode(HttpCommondHelper.password);
                string tempAns = MD5Encode(tempPass + HttpCommondHelper.token);
                if (userToken.Equals(tempAns))
                {
                    HttpCommondHelper.UpdateToken();
                    return true;
                }
            }
            return false;
        }

        internal override string GetSessionKey()
        {
            object passWord;
            if (!Dic.TryGetValue("password", out passWord))
            {
                Log.Info("got httpGmCode without password");

                string answer = "got httpGmCode without password";
                AnswerHttpCmd(answer);
                return "";
            }
            return passWord.ToString();
        }

        internal void SetData(HttpListenerContext context, Dictionary<string, object> dic, string cmdStr, string argsStr)
        {
            this.dic = dic;
            this.context = context;
            this.cmd = cmdStr;
            this.args = argsStr.Split('_');
        }
    }
}
