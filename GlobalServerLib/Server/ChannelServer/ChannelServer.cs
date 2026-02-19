using System;
using System.Collections.Generic;
using System.IO;
using Engine;
using ServerShared;
using SocketShared;
using Logger;
using System.Web.Script.Serialization;
using System.Text;
using DataProperty;
using Message.Global.Protocol.GM;
using System.Security.Cryptography;
using Message.Global.Protocol.GR;
using ServerFrame;
using DBUtility;
using ServerModels;
using Message.IdGenerator;

namespace GlobalServerLib
{
    public class ChannelServer
    {
        private GlobalServerApi api;

       JavaScriptSerializer jser = new JavaScriptSerializer();
        private string channelIp = "";
        public string ChannelIP
        { get { return channelIp; } }

        private ushort channelPort = 0;
        public ushort ChannelPort
        { get { return channelPort; } }

        private Tcp channelTcp = new Tcp();
        public Tcp ChannelTcp
        {
            get { return channelTcp; }
        }

        public ChannelServer(string ip, ushort port)
        {
            this.channelIp = ip;
            this.channelPort = port;
            LogList.Add(LogType.INFO, new Queue<string>());
            LogList.Add(LogType.WARN, new Queue<string>());
            LogList.Add(LogType.ERROR, new Queue<string>());
        }

        public delegate void Responser(MemoryStream stream);
        private Dictionary<uint, Responser> responsers = new Dictionary<uint, Responser>();

        private ServerState state = ServerState.Stopped;
        public ServerState State
        { get { return state; } }

        private DateTime nextConnectTime = DateTime.MaxValue;
        public DateTime NextConnectTime
        {
            get { return nextConnectTime; }
            set { nextConnectTime = value; }
        }

        JavaScriptSerializer serializer = new JavaScriptSerializer();
        public Dictionary<LogType, Queue<string>> LogList = new Dictionary<LogType, Queue<string>>();

        public void Init(GlobalServerApi server)
        {
            this.api = server;
            channelTcp.OnRead = OnRead;
            channelTcp.OnDisconnect = OnDisconnect;
            channelTcp.OnConnect = OnConnect;
            channelTcp.OnAccept = OnAccept;

            //StartListen(channelPort);
            BindResponser();
        }
        public void NotifyInitDone()
        {
            channelTcp.Connect(channelIp, channelPort);
            state = ServerState.Started;
        }

        public void ConnectToChannelServer()
        {
            channelTcp.Connect(channelIp, channelPort);
        }

        public void StartListen(ushort port)
        {
            channelTcp.NeedListenHeartbeat = false;
            channelTcp.Accept(port);
        }

        enum MSG_ID
        {
            CHARACTER_LIST = 1,
            CAN_RECEIVE_REWARD = 2,
            RECEIVE_REWARD = 3,

            // 10000 - 20000 HK版本
            UserInfo = 10001, // 角色列表
            SendProps = 10002,
        }
        public void BindResponser()
        {
            AddResponser((uint)MSG_ID.CHARACTER_LIST, OnResponse_CharacterList);
            AddResponser((uint)MSG_ID.CAN_RECEIVE_REWARD, OnResponse_CanReceiveReward);
            //AddResponser((uint)MSG_ID.RECEIVE_REWARD, OnResponse_ReceiveReward);

            // HK
            AddResponser((uint)MSG_ID.UserInfo, OnResponse_UserInfo);
            AddResponser((uint)MSG_ID.SendProps, OnResponse_SendProps);
        }

        public void AddResponser(uint id, Responser responser)
        {
            responsers.Add(id, responser);
        }

        protected void OnConnect(bool ret)
        {
            if (ret == true)
            {
                string log = "Connected to channel server";
                lock (LogList[LogType.INFO])
                {
                    LogList[LogType.INFO].Enqueue(log);
                }
                NextConnectTime = DateTime.MaxValue;
                state = ServerState.Started;
            }
            else
            {
                state = ServerState.DisConnect;
                NextConnectTime = DateTime.Now.AddSeconds(1);
            }
        }

        protected virtual void OnAccept(bool ret)
        {
            lock (this)
            {
                state = ServerState.Starting;
            }
        }

        protected void OnDisconnect()
        {
            if (channelIp == "127.0.0.1")
            {
                return;
            }
            string log = "channel server disconnect";
            lock (LogList[LogType.ERROR])
            {
                LogList[LogType.ERROR].Enqueue(log);
            }
            state = ServerState.DisConnect;
            NextConnectTime = DateTime.Now.AddSeconds(1);
        }

        Queue<KeyValuePair<UInt32, MemoryStream>> m_msgQueue = new Queue<KeyValuePair<uint, MemoryStream>>();
        Queue<KeyValuePair<UInt32, MemoryStream>> m_msgQueue2 = new Queue<KeyValuePair<uint, MemoryStream>>();
        Queue<KeyValuePair<UInt32, MemoryStream>> deal_msgQueue = new Queue<KeyValuePair<uint, MemoryStream>>();
        protected int OnRead(MemoryStream transferred)
        {
            int offset = 0;
            byte[] buffer = transferred.GetBuffer();
            //lock (this)
            {
                while ((transferred.Length - offset) > sizeof(UInt16))
                {
                    UInt16 size = BitConverter.ToUInt16(buffer, offset);
                    if (size + SocketHeader.Size > transferred.Length - offset)
                    {
                        break;
                    }

                    UInt32 msg_id = BitConverter.ToUInt32(buffer, offset + sizeof(UInt16));
                    MemoryStream msg = new MemoryStream(buffer, offset + SocketHeader.Size, size, true, true);
                    /*
                    StreamReader reader = new StreamReader(msg, Encoding.UTF8);
                    string jsonStr = reader.ReadToEnd();
                    Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                    */

                    lock (m_msgQueue)
                    {
                        m_msgQueue.Enqueue(new KeyValuePair<UInt32, MemoryStream>(msg_id, msg));
                    }
                    offset += (size + SocketHeader.Size);
                }
            }

            transferred.Seek(offset, SeekOrigin.Begin);
            return 0;
        }

        public void OnProcessProtocol()
        {
            lock (m_msgQueue)
            {
                while (m_msgQueue.Count > 0)
                {
                    var msg = m_msgQueue.Dequeue();
                    deal_msgQueue.Enqueue(msg);
                }
                //deal_msgQueue = m_msgQueue;
                //m_msgQueue = m_msgQueue2;
            }
            while (deal_msgQueue.Count > 0)
            {
                var msg = deal_msgQueue.Dequeue();
                OnResponse(msg.Key, msg.Value);
            }
            //m_msgQueue2 = deal_msgQueue;
        }

        public bool Write<T>(T msg) where T : Google.Protobuf.IMessage
        {
            MemoryStream body = new MemoryStream();
            MessagePacker.ProtobufHelper.Serialize(body, msg);

            MemoryStream header = new MemoryStream(sizeof(ushort) + sizeof(uint));
            ushort len = (ushort)body.Length;
            header.Write(BitConverter.GetBytes(len), 0, 2);
            header.Write(BitConverter.GetBytes(Id<T>.Value), 0, 4);

            return Write(header, body);
        }

        public bool Write(MemoryStream header, MemoryStream body)
        {
            if (channelTcp == null)
            {
                return false;
            }
            return channelTcp.Write(header, body);
        }

        public void OnResponse(uint id, MemoryStream stream)
        {
            Responser responser = null;
            if (responsers.TryGetValue(id, out responser) == true)
            {
                responser(stream);
            }
            else
            {
                Log.Warn("global got channel  server unsupported package id {0}", id);
            }
        }

        public string EncrypToMD5(string str)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] str1 = Encoding.UTF8.GetBytes(str);
            byte[] str2 = md5.ComputeHash(str1, 0, str1.Length);
            md5.Clear();
            md5.Dispose();
            return Convert.ToBase64String(str2);
        }

        public void WriteString(string response)
        {
            MemoryStream content = new MemoryStream();
            StreamWriter writer = new StreamWriter(content);
            writer.Write(response);
            writer.Flush();
            channelTcp.Write(content);
        }

        public void SendResult(int res_index, int err_code, string err_string)
        {
            GeneralResult result = new GeneralResult();
            result.resIndex = res_index;
            result.errcode = err_code;
            result.errmsg = err_string;
            string json = jser.Serialize(result);
            WriteString(json);
        }

        public void OnResponse_CharacterList(MemoryStream msg)
        {
            StreamReader reader = new StreamReader(msg, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                object qidObj;
                object resIndexObj;
                if (dicMsg.TryGetValue("qid", out qidObj) && dicMsg.TryGetValue("resIndex", out resIndexObj))
                {
                    int resIndex = Convert.ToInt32(resIndexObj);
                    string accountName = qidObj.ToString() + "$361";
                    ChannelCharacterList characterList = new ChannelCharacterList();
                    characterList.resIndex = resIndex;
                    characterList.errcode = 0;
                    characterList.errmsg = "ok";
                    characterList.data = new List<Service>();
                    QueryChannelCharacterList query = new QueryChannelCharacterList(accountName, characterList);
                    //server.DB.Call(new QueryChannelCharacterList(accountName, characterList), ret) =>
                    api.AccountDBPool.Call(query, (ret) =>
                    {
                        foreach (var item in characterList.data)
                        {
                            Data data = DataListManager.inst.GetData("ServerList", api.MainId);
                            if (data != null)
                            {
                                item.service_name = data.Name;
                            }
                            else
                            {
                                item.service_name = item.service.ToString();
                            }
                        }
                        string json = jser.Serialize(characterList);
                        WriteString(json);
                    });
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_CanReceiveReward(MemoryStream msg)
        {
            StreamReader reader = new StreamReader(msg, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                object qidObj;
                object resIndexObj;
                object serviceObj;
                object roleObj;
                object conditionsObj;
                int resIndex = 0;
                if(dicMsg.TryGetValue("resIndex", out resIndexObj) == false)
                {
                    return;
                }
                resIndex = Convert.ToInt32(resIndexObj);
                //{"qid": "156473811", "game": "com.game.shns.a360", "service": "70", "role": "trail", "conditions": "1007"}’
                if (dicMsg.TryGetValue("qid", out qidObj) && dicMsg.TryGetValue("service", out serviceObj) &&
                    dicMsg.TryGetValue("role", out roleObj) && dicMsg.TryGetValue("conditions", out conditionsObj))
                {
                    string accountName = qidObj.ToString() + "$361";
                    int serverId = Convert.ToInt32(serviceObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_CAN_RECEIVE_CHANNEL_TASK request = new MSG_GM_CAN_RECEIVE_CHANNEL_TASK();
                        request.ResIndex = resIndex;
                        request.AccountName = accountName;
                        request.CharName = roleObj.ToString();
                        request.TaskId = Convert.ToInt32(conditionsObj);
                        manager.Write(request);
                    }
                    else 
                    {
                        SendResult(resIndex, -3, "条件不足");
                    }
                }
                else
                {
                    SendResult(resIndex, -3, "条件不足");
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void SendHKResult(int ret, int index)
        {
            ResponseHKResult result = new ResponseHKResult();
            result.ret = ret;
            result.resIndex = index;
            switch (ret)
            {
                case 0:
                    result.msg = "ok";
                    break;
                case 1001:
                    result.msg = "參數錯誤";
                    break;
                case 1002:
                    result.msg = "不存在的用戶";
                    break;
                case 1004:
                    result.msg = "重複的訂單號";
                    break;
                case 1008:
                    result.msg = "帳號已禁用";
                    break;
                case 65535:
                    result.msg = "服務器錯誤";
                    break;
                default:
                    Log.Warn("HK Invalid Result: {0}", ret);
                    result.msg = ret.ToString();
                    break;
            }
            var jser = new JavaScriptSerializer();
            string json = jser.Serialize(result);
            WriteString(json);
        }

        public void OnResponse_UserInfo(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"uid":1, sc:"mcwyhk1001", ts:"1378456412008", sn:"1faec808b862af2d5a726f993477529b" } sn = MD5(uid+sc+KEY+ts)
                object channelUidObj;
                object scObj;
                object tsObj;
                object snObj;
                object resIndexObj;
                if (!dicMsg.TryGetValue("resIndex", out resIndexObj))
                {
                    Log.Warn("HK User Info failed: resIndex not exist");
                    return;
                }
                int resIndex;
                if (int.TryParse(resIndexObj.ToString(), out resIndex) == false)
                {
                    // 参数错误
                    Log.Warn("HK User Info failed: resIndex {0} can not convert to int", resIndexObj.ToString());
                    return;
                }
                if (dicMsg.TryGetValue("uid", out channelUidObj) && dicMsg.TryGetValue("sc", out scObj) &&
                    dicMsg.TryGetValue("ts", out tsObj) && dicMsg.TryGetValue("sn", out snObj))
                {
                    //mcwyhk1001 截取1001
                    if (scObj.ToString().Length <= 6)
                    {
                        // 参数错误
                        SendHKResult(1001, resIndex);
                        return;
                    }
                    int serverId;
                    string serverIdString = scObj.ToString().Substring(6);
                    if (int.TryParse(serverIdString, out serverId) == false)
                    {
                        // 参数错误
                        SendHKResult(1001, resIndex);
                        return;
                    }
                    int channelUid;
                    if (int.TryParse(channelUidObj.ToString(), out channelUid) == false)
                    {
                        // 参数错误
                        SendHKResult(1001, resIndex);
                        return;
                    }

                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        // check MD5 sn = MD5(uid+sc+KEY+ts)
                        string md5 = EncrypToMD5(channelUidObj.ToString() + scObj.ToString() + CONST.HKSDKKEY + tsObj.ToString());
                        if (md5 != snObj.ToString())
                        {
                            SendHKResult(1003, resIndex);
                            return;
                        }
                        MSG_GM_HK_USER_INFO request = new MSG_GM_HK_USER_INFO();
                        request.ChannelUid = channelUid;
                        request.MainId = serverId;
                        request.ResIndex = resIndex;
                        manager.Write(request);
                    }
                    else
                    {
                        SendHKResult(1001, resIndex);
                    }
                }
                else
                {
                    // 参数错误
                    SendHKResult(1001, resIndex);
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_SendProps(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"uid":1,sc:"mcwyhk1001",element_type:"0",element_id:"2",count:"5", ts:"1378456412008", 
                // sn:"1faec808b862af2d5a726f993477529b" roleId:"123"}
                /* sc	伺服器CODE	必填
                uid	用戶ID(用户的平台ID)	必填
                element_type	發放物品類型	選填
                element_id	發放物品編號[对应资料里可查]	必填
                count	發放物品數量	必填
                ts 时间戳	必填
                sn	簽名MD5(sc + uid + element_type + element_id + count + KEY+ timestamp)	必填
                title	郵件標題	選填
                content	郵件內容	選填
                roleId	角色ID	選填
                */

                object channelUidObj;
                object scObj;
                object tsObj;
                object snObj;
                object resIndexObj;
                object elementIdObj;
                object elementTypeObj;
                object countObj;
                object roleIdObj;
                if (!dicMsg.TryGetValue("resIndex", out resIndexObj))
                {
                    Log.Warn("HK Send Props Info failed: resIndex not exist");
                    return;
                }
                int resIndex;
                if (int.TryParse(resIndexObj.ToString(), out resIndex) == false)
                {
                    // 参数错误
                    Log.Warn("HK Send Props failed: resIndex {0} can not convert to int", resIndexObj.ToString());
                    return;
                }
                //{"uid":1,sc:"mcwyhk1001",element_type:"0",element_id:"2",count:"5", timestamp:"1378456412008", 
                // sn:"1faec808b862af2d5a726f993477529b" roleId:"123"}
                if (dicMsg.TryGetValue("uid", out channelUidObj) && dicMsg.TryGetValue("sc", out scObj) && dicMsg.TryGetValue("element_type", out elementTypeObj) &&
                    dicMsg.TryGetValue("element_id", out elementIdObj) && dicMsg.TryGetValue("count", out countObj) &&
                    dicMsg.TryGetValue("timestamp", out tsObj) && dicMsg.TryGetValue("roleId", out roleIdObj) && dicMsg.TryGetValue("sn", out snObj))
                {
                    //mcwyhk1001 截取1001
                    if (scObj.ToString().Length <= 6)
                    {
                        // 参数错误
                        SendHKResult(1001, resIndex);
                        return;
                    }
                    int serverId;
                    string serverIdString = scObj.ToString().Substring(6);
                    if (int.TryParse(serverIdString, out serverId) == false)
                    {
                        // 参数错误
                        SendHKResult(1001, resIndex);
                        return;
                    }
                    int channelUid;
                    if (int.TryParse(channelUidObj.ToString(), out channelUid) == false)
                    {
                        // 参数错误
                        SendHKResult(1001, resIndex);
                        return;
                    }

                    Log.Warn("sdk request send email to server {0} uid {1} item {2} count {3}", scObj.ToString(), roleIdObj.ToString(), elementIdObj.ToString(), countObj.ToString());

                    FrontendServer rServer = api.RelationServerManager.GetWatchDogServer();
                    if (rServer != null)
                    {
                        //MD5(sc + uid + element_type + element_id + count + KEY+ timestamp)
                        string md5 = scObj.ToString() + channelUidObj.ToString() + elementTypeObj.ToString() +
                            elementIdObj.ToString() + countObj.ToString() + CONST.HKSDKKEY + tsObj.ToString();
                        md5 = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(md5, "MD5").ToLower();
                        if (md5 != snObj.ToString())
                        {
                            SendHKResult(1003, resIndex);
                            return;
                        }
                        MSG_GR_SEND_ITEM request = new MSG_GR_SEND_ITEM();
                        request.Uid = Convert.ToInt32(roleIdObj);
                        request.ServerId = serverId;
                        request.Item = elementIdObj.ToString() + ":" + countObj.ToString();
                        rServer.Write(request);
                        SendHKResult(0, resIndex);
                    }
                    else
                    {
                        SendHKResult(1001, resIndex);
                    }

                    //MServer mserver;
                    //if (server.GetManagerServer(serverId, out mserver))
                    //{
                    //    //MD5(sc + uid + element_type + element_id + count + KEY+ timestamp)
                    //    string md5 = scObj.ToString() + channelUidObj.ToString() + elementTypeObj.ToString() +
                    //        elementIdObj.ToString() + countObj.ToString() + CONST.HKSDKKEY + tsObj.ToString();
                    //    md5 = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(md5, "MD5").ToLower();
                    //    if (md5 != snObj.ToString())
                    //    {
                    //        SendHKResult(1003, resIndex);
                    //        return;
                    //    }
                    //    MSG_GM_SEND_ITEM request = new MSG_GM_SEND_ITEM();
                    //    request.Uid = Convert.ToInt32(roleIdObj);
                    //    request.ServerId = serverId;
                    //    request.item = elementIdObj.ToString() + ":" + countObj.ToString();
                    //    mserver.Write(request);
                    //    SendHKResult(0, resIndex);
                    //}
                    //else
                    //{
                    //    SendHKResult(1001, resIndex);
                    //}
                }
                else
                {
                    // 参数错误
                    SendHKResult(1001, resIndex);
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }
    }
}
