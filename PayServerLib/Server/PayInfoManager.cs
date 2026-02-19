using System;
using System.Collections.Generic;

namespace PayServerLib
{
    public class PayInfoManager
    {
        //作为访问后的后续更新队列
        public Queue<PayInfo> payList = new Queue<PayInfo>();
        public Queue<PayInfo> PayListAfter = new Queue<PayInfo>();

        private PayServerApi server;

        public PayInfoManager(PayServerApi server)
        {
            this.server = server;
        }

        public void Add(PayInfo enter)
        {
            lock (payList)
            {
                payList.Enqueue(enter);
            }
        }


        public void UpdatePayInfo()
        {
            lock (payList)
            {
                while (payList.Count > 0)
                {
                    PayInfo enter = payList.Dequeue();
                    PayListAfter.Enqueue(enter);
                }
            }

            while (PayListAfter.Count > 0)
            {
                try
                {
                    PayInfo temp = PayListAfter.Dequeue();
                    temp.DoPay();
                }
                catch (Exception e)
                {
                    Logger.Log.Warn("pay with exception {0} ", e.ToString());
                }
            }
        }
    }
}
