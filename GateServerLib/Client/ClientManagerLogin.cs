using ServerShared;
using System;
using System.Collections.Generic;
using Logger;

namespace GateServerLib
{
    public partial class ClientManager
    {
        public OnlinePlayerState OnlineState = OnlinePlayerState.NORMAL;
        public Queue<Client> WaitQueue = new Queue<Client>();
        private double loginWaitQueueTime = CONST.LOGIN_QUEUE_PERIOD;
        private double loginDeltaTime = 0;
        private DateTime nextNotifyWaitingTime = DateTime.MinValue;
        public void LoginEnqueue(Client client)
        {
            WaitQueue.Enqueue(client);
        }

        public void CalcLoginDeltaTime(int total_count)
        {
            // 根据在线人数 判断状态
            OnlinePlayerState oldState = OnlineState;
            if (total_count >= CONST.ONLINE_COUNT_FULL_COUNT)
            {
                OnlineState = OnlinePlayerState.FULL;
            }
            else if (total_count >= CONST.ONLINE_COUNT_WAIT_COUNT)
            {
                OnlineState = OnlinePlayerState.WAIT;
            }
            else
            {
                OnlineState = OnlinePlayerState.NORMAL;
            }
            if (oldState != OnlineState && server.BarrackServerManager != null)
            {
                BarrackServer barrackServer = server.BarrackServerWatchDog;
                // 状态发生变化
                loginWaitQueueTime = CONST.LOGIN_QUEUE_PERIOD * barrackServer.GateCount;
                Log.Warn($"gate online state changed from {oldState} to {OnlineState} total in game count {total_count} gates count {barrackServer.GateCount} wait time {loginWaitQueueTime}");
            }
        }


        private void NotifyWaitingClients()
        {
            if (GateServerApi.now > nextNotifyWaitingTime && WaitQueue.Count > 0)
            {
                // 整理排队信息 清除在排队时间内离开的client
                Queue<Client> tmpQueue = new Queue<Client>();
                while (WaitQueue.Count > 0)
                {
                    Client client = WaitQueue.Dequeue();
                    if (client.IsConnected())
                    {
                        tmpQueue.Enqueue(client);
                    }
                }
                WaitQueue = tmpQueue;
                int index = 0;
                if (server.BarrackServerWatchDog != null)
                {
                    foreach (var item in tmpQueue)
                    {
                        item.NotifyWaitingTime((++index) * server.BarrackServerWatchDog.GateCount);
                    }
                }
                // 通知排队情况
                nextNotifyWaitingTime = GateServerApi.now.AddSeconds(CONST.NOTIFY_WAITING_TIME_PERIOD);
            }
        }
    }
}
