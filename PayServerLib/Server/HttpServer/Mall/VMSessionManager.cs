using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Logger;

namespace PayServerLib
{
    public class VMSessionManager
    {

        private VMallServer server;


        private ConcurrentQueue<VMallSession> waitingSessions = new ConcurrentQueue<VMallSession>();
        //处理中的
        private Queue<VMallSession> processingSessions = new Queue<VMallSession>();

        private List<VMallSession> expireSessions = new List<VMallSession>();
        private Dictionary<int, VMallSession> cacheSessions = new Dictionary<int, VMallSession>();

        public VMSessionManager(VMallServer server)
        {
            this.server = server;
        }

        public void Update()
        {
            VMallSession session;
            while (waitingSessions.TryDequeue(out session))
            {
                processingSessions.Enqueue(session);
            }

            foreach (var kv in cacheSessions)
            {
                session = kv.Value;
                if (session.ExpireTime < server.Now)
                {
                    expireSessions.Add(session);
                }
            }

            while (processingSessions.Count>0)
            {
                session = processingSessions.Dequeue();
                if (session.ExpireTime < server.Now)
                {
                    expireSessions.Add(session);
                }
                else
                {
                    server.DistributeMessage(session);
                    CacheProcessingInfo(session);
                }
            }
           
            if (expireSessions.Count > 0)
            {
                foreach (var kv in expireSessions)
                {
                    session = kv;
                    session.WriteResponse(VMResponse.GetFail(VMallErrorCode.OtherError, "out of time"));
                    cacheSessions.Remove(session.SessionUid);
                }
                expireSessions.Clear();
            }
        }


        public VMallSession CreateSession(string apiName, Dictionary<string, object> dic, Dictionary<string, object> header)
        {
            int id = IdHelper.GenerateId();
            VMallSession session = new VMallSession(id);
            session.BindParams(apiName, dic, header);
            waitingSessions.Enqueue(session);
            return session;
        }

        private void CacheProcessingInfo(VMallSession session)
        {
            if (cacheSessions.ContainsKey(session.SessionUid))
            {
                Log.Error("CacheProcessingInfo error repeated session id " + session.SessionUid);
                return;
            }
            cacheSessions.Add(session.SessionUid, session);
        }


        public VMallSession GetCacheHttpSession(int sessionUid)
        {
            VMallSession session;
            cacheSessions.TryGetValue(sessionUid, out session);
            return session;
        }

    }
}