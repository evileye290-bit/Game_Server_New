using System;
using System.Collections.Generic;
using Logger;
using ServerShared;
using System.Threading;
using DBUtility;
using CommonUtility;
using RedisUtility;
using ServerFrame;

namespace GlobalServerLib
{
    public partial class GlobalServerApi : BaseApi
    {
        ushort customPort = 0;
        // 处理GM命令 没有后台 临时用控制台输入替代
        public Queue<string> cmdList = new Queue<string>();

        public override void Init(string[] args)
        {
            base.Init(args);

            InitCommand();

            InitBasic();
            InitSocket();

            InitGmHttpServer();
            InitDone();

        }

        public override void SpecUpdate(double dt)
        {
            clientManager.Update(dt);
            ConsoleCommondExcute();
            HttpGmCommandExcute();
            GMRecordCache.Instance.Update(now);
        }

        public override void ProcessInput()
        {
            try
            {
                string cmd = Console.ReadLine().ToLower().Trim();
                lock (cmdList)
                {
                    cmdList.Enqueue(cmd);
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        private void HttpGmCommandExcute()
        {
            if (httpGmServer != null)
            {
                //更新控制台
                httpGmServer.UpdateCmdList();
                //刷log
                httpGmServer.UpdateLogList();
            }
        }

        private void ConsoleCommondExcute()
        {
            lock (cmdList)
            {
                while (cmdList.Count > 0)
                {
                    try
                    {
                        string cmd = cmdList.Dequeue();
                        ExcuteCommand(cmd);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
            }
        }


    }
}