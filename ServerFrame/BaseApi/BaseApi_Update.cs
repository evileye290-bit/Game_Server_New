using Logger;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ServerFrame
{
    partial class BaseApi
    {
        // 轮询
        public virtual void CommonUpdate(double dt)
        {
            Engine.System.Update();
            serverManagerProxy.Update(dt);
            ProcessDBPostUpdate();
            ProcessDBExceptionLog();
            ProcessRedisUpdate();
            ProcessRedisExceptionLog();
            ProcessTaskTimerUpdate();
            ProcessTaskTimerExceptionLog();

            if (State == ServerState.Stopping)
            {
                if (StoppingTime < now)
                {
                    State = ServerState.Stopped;
                    Log.Error("{0} stop!", ServerName);
                    Exit();
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                    return;
                }
            }
        }

        public virtual void SpecUpdate(double dt)
        {
        }

        // mysql相关轮询
        public virtual void ProcessDBPostUpdate()
        {
            if (gameDBPool != null)
            {
                gameDBPool.Update();
            }
            if (accountDBPool != null)
            {
                accountDBPool.Update();
            }
        }

        public virtual void ProcessDBExceptionLog()
        {
            if (gameDBPool != null)
            {
                foreach (var item in gameDBPool.DBManagerList)
                {
                    try
                    {
                        Queue<string> queue = item.GetExceptionLogQueue();
                        if (queue != null && queue.Count != 0)
                        {
                            string log = queue.Dequeue();
                            Log.Error(log);
                            lock (item.ReconnectInfo)
                            {
                                if (State != ServerState.Stopping && item.ReconnectInfo.TryConnectTime >= item.ReconnectInfo.MaxConnectTime)
                                {
                                    // DB断开连接 则立即终止服务 方式回档
                                    Log.Error("{0} stop  because game db disconnect!", serverName);
                                    StopServer(1);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }

                if (gameDBPool.ReadDbManager != null)
                {
                    try
                    {
                        Queue<string> queue = gameDBPool.ReadDbManager.GetExceptionLogQueue();
                        if (queue != null && queue.Count != 0)
                        {
                            string log = queue.Dequeue();
                            Log.Error(log);
                            lock (gameDBPool.ReadDbManager.ReconnectInfo)
                            {
                                if (State != ServerState.Stopping && gameDBPool.ReadDbManager.ReconnectInfo.TryConnectTime >= gameDBPool.ReadDbManager.ReconnectInfo.MaxConnectTime)
                                {
                                    // DB断开连接 则立即终止服务 方式回档
                                    Log.Error("{0} stop  because game read db disconnect!", serverName);
                                    StopServer(1);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
            }

            if (accountDBPool != null)
            {
                foreach (var item in accountDBPool.DBManagerList)
                {
                    try
                    {
                        Queue<string> queue = item.GetExceptionLogQueue();
                        if (queue != null && queue.Count != 0)
                        {
                            string log = queue.Dequeue();
                            Log.Error(log);
                            lock (item.ReconnectInfo)
                            {
                                if (State != ServerState.Stopping && item.ReconnectInfo.TryConnectTime >= item.ReconnectInfo.MaxConnectTime)
                                {
                                    // DB断开连接 则立即终止服务 方式回档
                                    Log.Error("{0} stop  because account db disconnect!", serverName);
                                    StopServer(1);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
            }
        }

        //redis相关轮询
        public virtual void ProcessRedisUpdate()
        {
            if (gameRedis != null)
            {
                gameRedis.Update();
            }
            if (crossRedis != null)
            {
                crossRedis.Update();
            }
        }

        public virtual void ProcessRedisExceptionLog()
        {
            if (gameRedis != null)
            {
                foreach (var oq in gameRedis.RedisQueryList)
                {
                    var queue = oq.GetExceptionLogQueue();
                    if (queue != null && queue.Count != 0)
                    {
                        string log = queue.Dequeue();
                        Log.Error(log);
                        lock (oq.ReconnectInfo)
                        {
                            if (State != ServerState.Stopping && oq.ReconnectInfo.TryConnectTime >= oq.ReconnectInfo.MaxConnectTime)
                            {
                                // Redis 无法连接 则立即终止服务 
                                Log.Error("{0} stop because redis disconnect!", serverName);
                                StopServer(0);
                            }
                        }
                    }
                }
            }

            if (crossRedis != null)
            {
                foreach (var oq in crossRedis.RedisQueryList)
                {
                    var queue = oq.GetExceptionLogQueue();
                    if (queue != null && queue.Count != 0)
                    {
                        string log = queue.Dequeue();
                        Log.Error(log);
                        lock (oq.ReconnectInfo)
                        {
                            if (State != ServerState.Stopping && oq.ReconnectInfo.TryConnectTime >= oq.ReconnectInfo.MaxConnectTime)
                            {
                                // Redis 无法连接 则立即终止服务 
                                Log.Error("{0} stop because cross redis disconnect!", serverName);
                                StopServer(0);
                            }
                        }
                    }
                }
            }
        }

        //timer相关轮询
        public virtual void ProcessTaskTimerUpdate()
        {
            if (taskTimerMng == null)
            {
                return;
            }
            var queue = taskTimerMng.GetPostUpdateQueue();
            while (queue.Count != 0)
            {
                try
                {
                    var oprate = queue.Dequeue();
                    oprate.PostUpdate();
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
        }

        public virtual void ProcessTaskTimerExceptionLog()
        {
            if (taskTimerMng == null)
            {
                return;
            }
            var queue = taskTimerMng.GetExceptionLogQueue();
            if (queue != null && queue.Count != 0)
            {
                string log = queue.Dequeue();
                Log.Error(log);
            }
        }
    }
}
