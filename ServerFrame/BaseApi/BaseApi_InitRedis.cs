using DataProperty;
using Logger;
using RedisUtility;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerFrame
{
    partial class BaseApi
    {
        protected RedisErrorMark redisErrorMark = new RedisErrorMark();
        /// <summary>
        /// 初始化redis
        /// </summary>
        public virtual void InitRedis()
        {
            Data data = DataListManager.inst.GetData("PersistentConfig", serverType.ToString());
            if (data == null)
            {
                Log.Warn("{0} init redis failed: no such data", serverName);
                return;
            }
            // 不需要初始化Redis
            if (data.GetBoolean("GameRedis"))
            {
                DataList dbList = DataListManager.inst.GetDataList("RedisConfig");
                data = dbList.Get("game");
                if (data == null)
                {
                    Log.Error("init redis failed: get game redis no game data in RedisConfig.xml failed");
                    return;
                }
                string redisIp = data.GetString("ip");
                int redisPort = data.GetInt("port");
                string redisPassword = data.GetString("password");
                int dbnum = data.GetInt("dbnum");
                int threadcount = data.GetInt("threadcount");

                redisErrorMark.Init();

                RedisConfigOptions.Init(redisIp, redisPassword, redisPort, dbnum);

                RedisManager.Instance.Conn.ConnectionFailed += MuxerConnectionFailed;
                RedisManager.Instance.Conn.ConnectionRestored += MuxerConnectionRestored;
                RedisManager.Instance.Conn.ErrorMessage += MuxerErrorMessage;
                RedisManager.Instance.Conn.ConfigurationChanged += MuxerConfigurationChanged;
                RedisManager.Instance.Conn.HashSlotMoved += MuxerHashSlotMoved;
                RedisManager.Instance.Conn.InternalError += MuxerInternalError;

                gameRedis = new RedisOperatePool(threadcount);
                gameRedis.Init(redisIp, redisPassword, redisPort, dbnum);
            }

            if (data.GetBoolean("CrossRedis"))
            {
                DataList dbList = DataListManager.inst.GetDataList("RedisConfig");
                data = dbList.Get("cross");
                if (data == null)
                {
                    Log.Error("init redis failed: get game redis no cross data in RedisConfig.xml failed");
                    return;
                }
                string redisIp = data.GetString("ip");
                int redisPort = data.GetInt("port");
                string redisPassword = data.GetString("password");
                int dbnum = data.GetInt("dbnum");
                int threadcount = data.GetInt("threadcount");

                redisErrorMark.Init();

                RedisConfigOptions.Init(redisIp, redisPassword, redisPort, dbnum);

                RedisManager.Instance.Conn.ConnectionFailed += MuxerConnectionFailed;
                RedisManager.Instance.Conn.ConnectionRestored += MuxerConnectionRestored;
                RedisManager.Instance.Conn.ErrorMessage += MuxerErrorMessage;
                RedisManager.Instance.Conn.ConfigurationChanged += MuxerConfigurationChanged;
                RedisManager.Instance.Conn.HashSlotMoved += MuxerHashSlotMoved;
                RedisManager.Instance.Conn.InternalError += MuxerInternalError;

                crossRedis = new RedisOperatePool(threadcount);
                crossRedis.Init(redisIp, redisPassword, redisPort, dbnum);
            }
        }

        /// <summary>
        /// 配置更改时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MuxerConfigurationChanged(object sender, EndPointEventArgs e)
        {
            //Console.WriteLine("Configuration changed: " + e.EndPoint);
            Log.WarnLine("{0} Configuration changed: {1}", serverName, e.EndPoint);
        }

        /// <summary>
        /// 发生错误时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MuxerErrorMessage(object sender, RedisErrorEventArgs e)
        {
            //Console.WriteLine("ErrorMessage: " + e.Message);
            Log.ErrorLine("ErrorMessage: {0}", e.Message);
            if (redisErrorMark.CheckShutDown(now, e.Message))
            {
                Log.Error("{0} stop because redis cluster error!", serverName);
                StopServer(0);
            }
        }

        /// <summary>
        /// 重新建立连接之前的错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MuxerConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            //Console.WriteLine("ConnectionRestored: " + e.EndPoint);
            Log.ErrorLine("{0} ConnectionRestored:{1} ", serverName, e.EndPoint);
        }

        /// <summary>
        /// 连接失败 ， 如果重新连接成功你将不会收到这个通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MuxerConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            string log = string.Format("{0} Reconnect：Endpoint failed: {1},{2}:{3}", 
                serverName, e.EndPoint, e.FailureType, (e.Exception == null ? "" : e.Exception.Message));
            Log.ErrorLine(log);
        }

        /// <summary>
        /// 更改集群
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MuxerHashSlotMoved(object sender, HashSlotMovedEventArgs e)
        {
            //Console.WriteLine("HashSlotMoved:NewEndPoint" + e.NewEndPoint + ", OldEndPoint" + e.OldEndPoint);
            Log.WarnLine("{0} HashSlotMoved:NewEndPoint {1}, OldEndPoint {2}", serverName, e.NewEndPoint, e.OldEndPoint);
        }

        /// <summary>
        /// redis类库错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MuxerInternalError(object sender, InternalErrorEventArgs e)
        {
            //Console.WriteLine("InternalError:Message" + e.Exception.Message);
            Log.ErrorLine("{0} InternalError:Message {1}", serverName, e.Exception.Message);
        }
    }
}
