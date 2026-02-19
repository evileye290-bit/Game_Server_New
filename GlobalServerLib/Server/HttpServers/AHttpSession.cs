using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace GlobalServerLib
{
    public enum SessionType 
    {
        GMRequest,
        SelectRequest,
    }

    public abstract class AHttpSession
    {
        public int SessionUid;

        protected SessionType sessionType;
        public SessionType Type { get { return sessionType; } }

        protected HttpListenerContext context;
        public HttpListenerContext Context
        {
            get { return context; }
            set { context = value; }
        }

        protected string cmd = null;

        public string Cmd
        {
            get { return cmd; }
        }

        protected string[] args = null;

        public string[] Args
        {
            get { return args; }
        }

        protected Dictionary<string, object> dic = null;

        public Dictionary<string, object> Dic 
        {
            get { return dic; }
        }

        protected bool answerd = false;

        protected AHttpSession(SessionType sessionType)
        {
            this.sessionType = sessionType;
        }

        public void AnswerHttpCmd(string answer)
        {
            if (!answerd)
            {
                using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write(answer);
                }
                answerd = true;
            }
        }

        internal abstract string GetSessionKey();

        internal abstract bool CheckToken(string v);
       
    }
}