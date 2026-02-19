using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class HttpResponseQueue
    {
        private ConcurrentQueue<ABoilHttpQuery> postUpdateQueue = new ConcurrentQueue<ABoilHttpQuery>();

        private ConcurrentQueue<string> exceptionLogQueue = new ConcurrentQueue<string>();

        public bool Opened = false;

        public bool Init()
        {
            postUpdateQueue = new ConcurrentQueue<ABoilHttpQuery>();
            Opened = true;
            return true;
        }

        public bool Exit()
        {
            Opened = false;
            return true;
        }

        private void Add(ABoilHttpQuery query)
        {
            postUpdateQueue.Enqueue(query);
        }

        public void SetCallBack(ABoilHttpQuery query, Func<string> callback = null)
        {
            query.SetCallBack(callback);
            Add(query);
        }

        public Queue<ABoilHttpQuery> GetPostUpdateQueue()
        {
            Queue<ABoilHttpQuery> ret = new Queue<ABoilHttpQuery>();

            ABoilHttpQuery query;
            while (postUpdateQueue.TryDequeue(out query))
            {
                ret.Enqueue(query);
            }

            return ret;
        }

        public Queue<string> GetExceptionLogQueue()
        {
            Queue<string> ret = new Queue<string>();
            string str;
            while (exceptionLogQueue.TryDequeue(out str))
            {
                ret.Enqueue(str);
            }
            return ret;
        }

        public void AddExceptionLog(string log = "")
        {
            if (!string.IsNullOrEmpty(log))
            {
                exceptionLogQueue.Enqueue(log);
            }
        }


    }
}
