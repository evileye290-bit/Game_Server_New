using DBUtility;
using Logger;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerFrame
{
    public class BaseProgram
    {
        private static BaseApi api;
        private static BaseProgram instance;

        // 注册信号回调函数
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);
        private delegate bool EventHandler(int sig);
        static EventHandler _handler;

        private static bool Handler(int sig)
        {
            switch (sig)
            {
                // 屏蔽手抖ctrl+c复制操作导致console捕获该信号退出soulronmg
                case (int)SysCtrlType.CTRL_CLOSE_EVENT:
                    Console.WriteLine("got ctrl + close event and will ignore");
                    //api.StopServer(0);
                    //Thread.Sleep(5000);
                    break;
                default:
                    Console.WriteLine("got ctrl + default event {0}", sig);
                    break;
            }
            return true;
        }

        public static BaseProgram Instance()
        {
            if (instance == null)
            {
                instance = new BaseProgram();
            }
            return instance;
        }

        private BaseProgram()
        {
            _handler = new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);
        }

        public void Start(string[] args)
        {
            try
            {
                api = ApiFactory.CreateApi();
                api.Init(args);
            }
            catch (Exception e)
            {
                Log.Error("{0} init failed: {1}", Application.ProductName, e.ToString() );
                api.Exit();
                Thread.Sleep(2000);
                Environment.Exit(0);
                return;
            }

            try
            {
                Thread thread = new Thread(api.Run);
                thread.Start();

                while (thread.IsAlive)
                {
                    api.ProcessInput();
                    Thread.Sleep(1000);
                }

                api.Exit();
            }
            catch (Exception e)
            {
                Log.Error("{0} run failed: {1}", Application.ProductName, e.ToString());
                api.Exit();
                Thread.Sleep(2000);
                Environment.Exit(0);
                return;
            }
        }
    }
}
