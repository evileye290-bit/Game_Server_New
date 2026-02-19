using System.IO;
using Engine;
using Message.IdGenerator;
using System;
using SocketShared;
using System.Collections.Generic;
using Logger;
using ServerShared;
using System.Text;
using System.Web.Script.Serialization;
using Message.Global.Protocol.GM;
using Message.Global.Protocol.GR;
using EnumerateUtility;
using Message.Global.Protocol.GGate;
using ServerFrame;
using ServerModels;
using CommonUtility;

namespace GlobalServerLib
{
    public partial class Client
    {
        Tcp m_tcp = new Tcp();
        private JavaScriptSerializer serializer = new JavaScriptSerializer();
        public void CheckWaitStreams()
        {
            if (m_tcp.WaitStreamsCount >= 4000)
            {
                Logger.Log.Warn("client {0} wait streams too many, will disconnect", Uid);
                api.ClientMng.RemoveClient(this);
            }
        }

        public delegate void Responser(MemoryStream stream);
        private Dictionary<uint, Responser> responsers = new Dictionary<uint, Responser>();

        public void BindResponser()
        {
            //AddResponser((uint)CustomProtocolID.CharacterList, OnResponse_CharacterList);
            //AddResponser((uint)CustomProtocolID.CharacterInfo, OnResponse_CharacterInfo);
            //AddResponser((uint)CustomProtocolID.AccountId, OnResponse_AccountId);
            //AddResponser((uint)CustomProtocolID.UnVoice, OnResponse_UnVoice);
            //AddResponser((uint)CustomProtocolID.Voice, OnResponse_Voice);
            //AddResponser((uint)CustomProtocolID.Announcement, OnResponse_Announcement);
            //AddResponser((uint)CustomProtocolID.City, OnResponse_City);
            //AddResponser((uint)CustomProtocolID.Freeze, OnResponse_Freeze);
            //AddResponser((uint)CustomProtocolID.UnFreeze, OnResponse_UnFreeze);
            //AddResponser((uint)CustomProtocolID.Bag, OnResponse_Bag);
            //AddResponser((uint)CustomProtocolID.OrderState, OnResponse_OrderState);
            //AddResponser((uint)CustomProtocolID.RepairOrder, OnResponse_RepairOrder);
            //AddResponser((uint)CustomProtocolID.DeleteBatItem, OnResponse_DelateBagItem);
            //AddResponser((uint)CustomProtocolID.SendItem, OnResponse_SendItem);
            //AddResponser((uint)CustomProtocolID.VirtualRecharge, OnResponse_VirtualRecharge);
            //AddResponser((uint)CustomProtocolID.SendMail, OnResponse_SendMail);
            //AddResponser((uint)CustomProtocolID.BadWords, OnResponse_BadWorlds);

            AddResponser((uint)GMProtocolID.CharacterList, OnResponse_CharacterListByAccountName);
            AddResponser((uint)GMProtocolID.CharacterInfo, OnResponse_CharAllInfo); //玩家角色信息
            AddResponser((uint)GMProtocolID.AccountId, OnResponse_AccountId);
            AddResponser((uint)GMProtocolID.UnVoice, OnResponse_UnVoice);
            AddResponser((uint)GMProtocolID.Voice, OnResponse_Voice);
            AddResponser((uint)GMProtocolID.Announcement, OnResponse_Announcement);
            AddResponser((uint)GMProtocolID.City, OnResponse_City);
            AddResponser((uint)GMProtocolID.Freeze, OnResponse_Freeze);
            AddResponser((uint)GMProtocolID.UnFreeze, OnResponse_UnFreeze);
            AddResponser((uint)GMProtocolID.Bag, OnResponse_Bag);
            AddResponser((uint)GMProtocolID.OrderState, OnResponse_OrderState);
            AddResponser((uint)GMProtocolID.RepairOrder, OnResponse_RepairOrder);
            AddResponser((uint)GMProtocolID.DeleteBatItem, OnResponse_DelateBagItem);
            AddResponser((uint)GMProtocolID.SendItem, OnResponse_SendItem);
            AddResponser((uint)GMProtocolID.VirtualRecharge, OnResponse_VirtualRecharge);
            AddResponser((uint)GMProtocolID.SendMail, OnResponse_SendMail);
            AddResponser((uint)GMProtocolID.BadWords, OnResponse_GMBadWords);
            AddResponser((uint)GMProtocolID.ArenaInfo, OnResponse_ArenaInfo);
            AddResponser((uint)GMProtocolID.FamilyInfo, OnResponse_FamilyInfo);
            AddResponser((uint)GMProtocolID.ServerInfo, OnResponse_ServerInfo);
            AddResponser((uint)GMProtocolID.GiftCode, OnResponse_GiftCode);
            AddResponser((uint)GMProtocolID.GameCounter, OnResponse_GameCounter);
            AddResponser((uint)GMProtocolID.ChangeFamilyName, OnResponse_ChangeFamilyName);
            AddResponser((uint)GMProtocolID.RecommendServer, OnResponse_RecommendServer);
            AddResponser((uint)GMProtocolID.ItemTypeList, OnResponse_ItemTypeList);
            AddResponser((uint)GMProtocolID.PetTypeList, OnResponse_PetTypeList);
            AddResponser((uint)GMProtocolID.PetMountList, OnResponse_PetMountList);
            AddResponser((uint)GMProtocolID.DeletePet, OnResponse_DeletePet);
            AddResponser((uint)GMProtocolID.DeletePetMount, OnResponse_DeletePetMount);
            AddResponser((uint)GMProtocolID.EquipList, OnResponse_EquipList);
            AddResponser((uint)GMProtocolID.PetList, OnResponse_PetList);
            AddResponser((uint)GMProtocolID.PetMountStrength, OnResponse_PetMountStrength);
            AddResponser((uint)GMProtocolID.DeleteItem, OnResponse_DeleteItem);
            AddResponser((uint)GMProtocolID.DeleteChar, OnResponse_DeleteChar);
            AddResponser((uint)GMProtocolID.ServerList, OnResponse_ServerList);
            AddResponser((uint)GMProtocolID.RecentLoginServers, OnResponse_RecentLoginServers);
            AddResponser((uint)GMProtocolID.OrderList, OnResponse_OrderList);
            AddResponser((uint)GMProtocolID.SpecItem, OnResponse_SpecItem);
            AddResponser((uint)GMProtocolID.SpecPet, OnResponse_SpecPet);
            AddResponser((uint)GMProtocolID.UpdateItemCount, OnResponse_UpdateItemCount);
            AddResponser((uint)GMProtocolID.SpecEmail, OnResponse_SpecEmail);
            AddResponser((uint)GMProtocolID.UpdateCharData, OnResponse_UpdateCharData);

            AddResponser((uint)GMProtocolID.HeroList, OnResponse_HeroList);
            AddResponser((uint)GMProtocolID.ZoneTransform, OnResponse_ZoneTransform);
            

            // ProjectX
            AddResponser((uint)GMProtocolID.SetPlayerLevel, OnResponse_PlayerLevel);
            AddResponser((uint)GMProtocolID.SetPlayerExp, OnResponse_PlayerExp);

            //Welfare
            AddResponser((uint)GMProtocolID.WelfareStallAdd, OnResponse_AddWelfareStall);
            AddResponser((uint)GMProtocolID.WelfareStallDelete, OnResponse_DeleteWelfareStall);
            AddResponser((uint)GMProtocolID.WelfareStallGet, OnResponse_FindWelfareStall);
            AddResponser((uint)GMProtocolID.WelfareStallModify, OnResponse_ModifyWelfareStall);

            AddResponser((uint)GMProtocolID.WelfarePlayerAdd, OnResponse_AddWelfarePlayer);
            AddResponser((uint)GMProtocolID.WelfarePlayerDelete, OnResponse_DeleteWelfarePlayer);
            AddResponser((uint)GMProtocolID.WelfarePlayerGet, OnResponse_FindWelfarePlayer);
            //AddResponser((uint)GMProtocolID.WelfarePlayerModify, OnResponse_PlayerExp);

            AddResponser((uint)GMProtocolID.ItemProduce, OnResponse_ItemProduce);
            AddResponser((uint)GMProtocolID.ItemConsume, OnResponse_ItemConsume);
            AddResponser((uint)GMProtocolID.CurrencyProduce, OnResponse_CurrencyProduce);
            AddResponser((uint)GMProtocolID.CurrencyConsume, OnResponse_CurrencyConsume);
            AddResponser((uint)GMProtocolID.LoginOrLogOut, OnResponse_LoginOrLogout);

            AddResponser((uint)GMProtocolID.GetServerState, OnResponse_GetServerState);
            AddResponser((uint)GMProtocolID.SetServerState, OnResponse_SetServerState);

            AddResponser((uint)GMProtocolID.TipOffInfo, OnResponse_TipOffInfo);
            AddResponser((uint)GMProtocolID.IgnoreTipOff, OnResponse_IgnoreTipOff);
            AddResponser((uint)GMProtocolID.SendPersonEmail, OnResponse_SendPeronEmail);

            //游戏道具修改
            AddResponser((uint)GMProtocolID.GetItemInfo, OnResponse_GetItemInfo);
            AddResponser((uint)GMProtocolID.ChangeItemNum, OnResponse_ChangeItemNum);
            AddResponser((uint)GMProtocolID.DelActiveProgress, OnResponse_DeleteActiveProgress);
            //End
        }

        public void AddResponser(uint id, Responser responser)
        {
            responsers.Add(id, responser);
        }

        public void OnResponse(uint id, MemoryStream stream)
        {
            Responser responser = null;
            if (responsers.TryGetValue(id, out responser) == true)
            {
                Log.Info("global got client  {0}  package id {1}", Uid, id);

                responser(stream);
            }
            else
            {
                Log.Warn("global got client  {0}  unsupported package id {1}", Uid, id);
            }
        }

        // NOTE : 目前tcp客户端通过端口连接
        public void Listen(ushort port)
        {
            m_tcp.Accept(port);
        }

        // NOTE : 调用对象的属性
        void InitTcp()
        {
            m_tcp.OnRead = OnRead;
            m_tcp.OnDisconnect = OnDisconnect;
            m_tcp.OnAccept = OnAccept;
        }

        public bool IsConnected()
        {
            if (m_tcp != null)
            {
                return !m_tcp.IsClosed();
            }

            return false;
        }

        // NOTE : tcp连接连接到客户端
        private void OnAccept(bool ret)
        {
            if (ret == true)
            {
                api.ClientMng.BindClient(this);

                //调整发送缓冲区大小
                m_tcp.SendBufferSize = 8388608;
            }
        }

        // NOTE : 访问结束后通过调用IOCP
        // IOCP Threading
        public void OnDisconnect()
        {
            api.ClientMng.RemoveClient(this);
        }

        // NOTE : 客户端对象删除前直接终止处理连接
        public void Disconnect()
        {
            lock (this)
            {
                if (m_tcp != null && IsConnected())
                {
                    m_tcp.Disconnect();
                }
            }
        }

        /// <summary>
        ///The following code was written by Kumo.If you encountered any problem, please contact him by QQ479813005.
        ///2015年4月14日09:17:47 修改
        /// </summary>
        Queue<KeyValuePair<UInt32, MemoryStream>> m_msgQueue = new Queue<KeyValuePair<uint, MemoryStream>>();
        Queue<KeyValuePair<UInt32, MemoryStream>> m_msgQueue2 = new Queue<KeyValuePair<uint, MemoryStream>>();
        Queue<KeyValuePair<UInt32, MemoryStream>> deal_msgQueue = new Queue<KeyValuePair<uint, MemoryStream>>();
        private int OnRead(MemoryStream transferred)
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

        public bool Write(uint pid, MemoryStream body)
        {
            MemoryStream header = new MemoryStream(sizeof(ushort) + sizeof(uint));
            ushort len = (ushort)body.Length;
            header.Write(BitConverter.GetBytes(len), 0, 2);
            header.Write(BitConverter.GetBytes(pid), 0, 4);

            return Write(header, body);
        }

        public bool Write<T>(T msg)
            where T : Google.Protobuf.IMessage
        {
            MemoryStream body = new MemoryStream();
            MessagePacker.ProtobufHelper.Serialize(body, msg);

            MemoryStream header = new MemoryStream(sizeof(ushort) + sizeof(uint));
            ushort len = (ushort)body.Length;
            header.Write(BitConverter.GetBytes(len), 0, 2);
            header.Write(BitConverter.GetBytes(Id<T>.Value), 0, 4);

            return Write(header, body);
        }

        public bool Write(MemoryStream msg)
        {
            MemoryStream body = new MemoryStream();
            return m_tcp.Write(msg, body);
        }

        public bool Write(MemoryStream header, MemoryStream body)
        {
            return m_tcp.Write(header, body);
        }

        public bool Write(ArraySegment<byte> first, ArraySegment<byte> second)
        {
            return m_tcp.Write(first, second);
        }

        /// <summary>
        /// 为广播Msg获取字节数组
        /// </summary>
        /// <typeparam name="T">泛型Msg类型</typeparam>
        /// <param name="msg">Msg实体</param>
        /// <param name="first">out 报头数组</param>
        /// <param name="second">out 报文数组</param>
        public static void BroadCastMsgMemoryMaker<T>(T msg, out ArraySegment<byte> first, out ArraySegment<byte> second) where T : Google.Protobuf.IMessage
        {
            MemoryStream body = new MemoryStream();
            MessagePacker.ProtobufHelper.Serialize(body, msg);

            MemoryStream header = new MemoryStream(sizeof(ushort) + sizeof(uint));
            ushort len = (ushort)body.Length;
            header.Write(BitConverter.GetBytes(len), 0, 2);
            header.Write(BitConverter.GetBytes(Id<T>.Value), 0, 4);
            Tcp.MakeArray(header, body, out first, out second);
        }

        public void SendFailed()
        {
            ResponseResult result = new ResponseResult();
            result.result = 0;
            var jser = new JavaScriptSerializer();
            string json = jser.Serialize(result);
            WriteString(json);
        }

        public void SendSuccess()
        {
            ResponseResult result = new ResponseResult();
            result.result = 1;
            var jser = new JavaScriptSerializer();
            string json = jser.Serialize(result);
            WriteString(json);
        }

        public void SendItemFailed(string item)
        {
            SendItemFailed result = new SendItemFailed();
            result.result = 0;
            result.item = item;
            var jser = new JavaScriptSerializer();
            string json = jser.Serialize(result);
            WriteString(json);
        }

        public void WriteString(string response)
        {
            MemoryStream content = new MemoryStream();
            StreamWriter writer = new StreamWriter(content);
            writer.Write(response);
            writer.Flush();
            Write(content);
        }

        public void OnResponse_CharacterList(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"accountId":1,"serverId":2}
                object accountIdObj;
                object serverIdObj;
                if (dicMsg.TryGetValue("accountId", out accountIdObj) && dicMsg.TryGetValue("serverId", out serverIdObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int accountId = Convert.ToInt32(accountIdObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_CHARACTER_LIST request = new MSG_GM_CHARACTER_LIST();
                        request.CustomUid = Uid;
                        request.AccountId = accountId;
                        manager.Write(request);
                    }
                    else
                    {
                        SendFailed();
                    }
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception)
            {
                SendFailed();
            }
        }

        public void OnResponse_CharacterInfo(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverID":1,"uid":100,charName:"aaa"}
                object uidObj;
                object charNameObj;
                object serverIdObj;
                if (dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("serverId", out serverIdObj)
                     && dicMsg.TryGetValue("charName", out charNameObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    string charName = charNameObj.ToString();
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_CHARACTER_INFO request = new MSG_GM_CHARACTER_INFO();
                        request.CustomUid = Uid;
                        request.Uid = uid;
                        request.Name = charName;
                        manager.Write(request);
                    }
                    else
                    {
                        SendFailed();
                    }
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception)
            {
                SendFailed();
            }
        }

        public void OnResponse_AccountId(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverID":1,"uid":100,charName:"aaa"}
                object uidObj;
                object charNameObj;
                //object serverIdObj;
                if (dicMsg.TryGetValue("uid", out uidObj) //&& dicMsg.TryGetValue("serverId", out serverIdObj)
                     && dicMsg.TryGetValue("name", out charNameObj))
                {
                    //int serverId = Convert.ToInt32(serverIdObj);
                    //MServer mserver;
                    //if (server.GetManagerServer(serverId, out mserver))
                    //{
                    //    MSG_GM_ACCOUNT_ID request = new MSG_GM_ACCOUNT_ID();
                    //    request.CustomUid = Uid;
                    //    request.Uid = uid;
                    //    request.Name = charName;
                    //    mserver.Write(request);
                    //}
                    //else
                    //{
                    //    SendFailed();
                    //}
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception)
            {
                SendFailed();
            }
        }

        public void OnResponse_UnVoice(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1001,"uid":"2","minutes":3,"reason":1}
                object serverIdObj;
                object uidObj;
                object minutesObj;
                object reasonObj;
                if (dicMsg.TryGetValue("serverId", out serverIdObj) && dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("minutes", out minutesObj) && dicMsg.TryGetValue("reason", out reasonObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    int minutes = Convert.ToInt32(minutesObj);
                    string reason = reasonObj.ToString();

                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if(manager == null)
                    {
                        SendFailed();
                        return;
                    }
                    MSG_GM_UNVOICE request = new MSG_GM_UNVOICE();
                    request.CustomUid = Uid;
                    request.Uid = uid;
                    request.Minutes = minutes;
                    request.Reason = reason;
                    request.ServerId = serverId;
                    manager.Write(request);
                    SendSuccess();
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_Voice(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1001, "uid":"2"}
                object serverIdObj;
                object uidObj;
                if (dicMsg.TryGetValue("serverId", out serverIdObj)  && dicMsg.TryGetValue("uid", out uidObj)) 
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);

                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager == null)
                    {
                        SendFailed();
                        return;
                    }

                    MSG_GM_VOICE request = new MSG_GM_VOICE();
                    request.CustomUid = Uid;
                    request.ServerId = serverId;
                    request.Uid = uid;
                    //api.ManagerServerManager.Broadcast(request);
                    manager.Write(request);
                    SendSuccess();
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_Announcement(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1001, "content":"2"}
                object contentObj;
                object serverIdObj;
                //object bottomObj;
                if (dicMsg.TryGetValue("content", out contentObj)&& dicMsg.TryGetValue("serverId", out serverIdObj))
                {
                    MSG_GGate_ANNOUNCEMENT announcement = new MSG_GGate_ANNOUNCEMENT();
                    announcement.Type = (int)ANNOUNCEMENT_TYPE.CUSTOM_SYSTEM;
                    announcement.List.Add(contentObj.ToString());
                    //if (Convert.ToInt32(bottomObj) == 0)
                    //{
                    //    announcement.Bottom = false;
                    //}
                    //else
                    //{
                    //    announcement.Bottom = true;
                    //}

                    int serverId = Convert.ToInt32(serverIdObj);
                    api.GateServerManager.Broadcast(announcement, serverId);
                    SendSuccess();
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_City(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1,"uid":"2"}
                object serverIdObj;
                object uidObj;
                if (dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("serverId", out serverIdObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_MOVE_PLAYER_CITY request = new MSG_GM_MOVE_PLAYER_CITY();
                        request.ServerId = serverId;
                        request.Uid = uid;
                        manager.Write(request);
                        SendSuccess();
                    }
                    else
                    {
                        SendFailed();
                    }
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_Freeze(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1001,"uid":"2","hours":3,"reason":1}
                object serverIdObj;
                object uidObj;
                object hoursObj;
                object reasonObj;
                if (dicMsg.TryGetValue("serverId", out serverIdObj) && dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("hours", out hoursObj) && dicMsg.TryGetValue("reason", out reasonObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    int hours = Convert.ToInt32(hoursObj);
                    string reason = reasonObj.ToString();

                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager == null)
                    {
                        SendFailed();
                        return;
                    }
                    MSG_GM_FREEZE request = new MSG_GM_FREEZE();
                    request.Uid = uid;
                    request.Hours = hours;
                    request.Reason = reason;
                    request.ServerId = serverId;
                    manager.Write(request);
                    SendSuccess();
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_UnFreeze(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1001, "uid":"2"}
                object serverIdObj;
                object uidObj;
                if (dicMsg.TryGetValue("serverId", out serverIdObj) && dicMsg.TryGetValue("uid", out uidObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);

                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager == null)
                    {
                        SendFailed();
                        return;
                    }
                    MSG_GM_UNFREEZE request = new MSG_GM_UNFREEZE();
                    request.Uid = uid;               
                    request.ServerId = serverId;
                    manager.Write(request);
                    SendSuccess();
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_Bag(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1,"uid":"2"}
                object serverIdObj;
                object uidObj;
                if (dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("serverId", out serverIdObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_BAG request = new MSG_GM_BAG();
                        request.Uid = uid;
                        request.ServerId = serverId;
                        request.CustomUid = Uid;
                        manager.Write(request);
                    }
                    else
                    {
                        SendFailed();
                    }
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_OrderState(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1,"orderId:"PB123", uid":"2"}
                object serverIdObj;
                object uidObj;
                object orderIdObj;
                if (dicMsg.TryGetValue("uid", out uidObj) && 
                    dicMsg.TryGetValue("serverId", out serverIdObj) &&
                    dicMsg.TryGetValue("orderId", out orderIdObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_ORDER_STATE request = new MSG_GM_ORDER_STATE();
                        request.Uid = uid;
                        request.ServerId = serverId;
                        request.CustomUid = Uid;
                        request.OrderId = orderIdObj.ToString();
                        manager.Write(request);
                    }
                    else
                    {
                        SendFailed();
                    }
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_RepairOrder(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1, "uid":1001001510, "orderInfo:"test_1001001510_5191", "orderId":10010000000427, amount:198.9}
                object serverIdObj;
                object uidObj;
                object orderInfoObj;
                object orderIdObj;
                object amountObj;
                float amount = -1; // HK补单部分需要amount
                if (dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("serverId", out serverIdObj) &&
                    dicMsg.TryGetValue("orderInfo", out orderInfoObj) && dicMsg.TryGetValue("orderId", out orderIdObj))
                {
                    if (dicMsg.TryGetValue("amount", out amountObj))
                    {
                        amount = Convert.ToSingle(amountObj);
                    }

                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    long orderId = Convert.ToInt64(orderIdObj);
                    Log.Warn("custom client request repair order main {0} uid {1} orderId {2} orderInfo {3}", serverId, uid, orderId, orderInfoObj.ToString());
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_REPAIR_ORDER request = new MSG_GM_REPAIR_ORDER();
                        request.Uid = uid;
                        request.ServerId = serverId;
                        request.CustomUid = Uid;
                        request.OrderInfo = orderInfoObj.ToString();
                        request.Amount = amount;                   
                        request.OrderId = (int)(orderId - serverId * CONST.RechargeOrderTempNum);
                        manager.Write(request);
                    }
                    else
                    {
                        SendFailed();
                    }
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_DelateBagItem(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1,"uid":1，"itemId":2}
                object serverIdObj;
                object uidObj;
                object itemIdObj;
                if (dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("serverId", out serverIdObj) &&
                    dicMsg.TryGetValue("itemId", out itemIdObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    UInt64 itemId = Convert.ToUInt64(itemIdObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_DELETE_BAG_ITEM request = new MSG_GM_DELETE_BAG_ITEM();
                        request.Uid = uid;
                        request.ServerId = serverId;
                        request.CustomUid = Uid;
                        request.ItemId = itemId;
                        manager.Write(request);
                    }
                    else
                    {
                        SendFailed();
                    }
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_VirtualRecharge(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1,"uid":1，serverId:2,"cash":10}
                object serverIdObj;
                object uidObj;
                object cashObj;
                if (dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("serverId", out serverIdObj) &&
                    dicMsg.TryGetValue("cash", out cashObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    int cash = Convert.ToInt32(cashObj);
                    Log.Warn("custom client request add virtual recharge main {0} uid {1} money {2}", serverId, uid, cash);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_VIRTUAL_RECHARGE request = new MSG_GM_VIRTUAL_RECHARGE();
                        request.Uid = uid;
                        request.ServerId = serverId;
                        request.Cash = cash;
                        manager.Write(request);
                        SendSuccess();
                    }
                    else
                    {
                        SendFailed();
                    }
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_SendItem(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{serverId:"1","uid":2,"item":"id:num@id:num@id:num"}
                //object serverIdObj;
                object uidObj;
                object itemObj = new object();
                if (dicMsg.TryGetValue("uid", out uidObj) && //dicMsg.TryGetValue("serverId", out serverIdObj) &&
                    dicMsg.TryGetValue("item", out itemObj))
                {
                    //int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    Log.Warn("custom client request send item to main {0} uid {1} item {2}", 0, uid, itemObj.ToString());

                    FrontendServer rServer = api.RelationServerManager.GetWatchDogServer();
                    if (rServer != null)
                    {
                        MSG_GR_SEND_ITEM notify = new MSG_GR_SEND_ITEM();
                        notify.Uid = uid;
                        notify.Item = itemObj.ToString();
                        rServer.Write(notify);
                        SendSuccess();
                    }
                    else
                    {
                        SendFailed();
                    }

                    //MServer mserver;
                    //if (server.GetManagerServer(serverId, out mserver))
                    //{
                    //    MSG_GM_SEND_ITEM request = new MSG_GM_SEND_ITEM();
                    //    request.Uid = uid;
                    //    request.ServerId = serverId;
                    //    request.item = itemObj.ToString();
                    //    mserver.Write(request);
                    //    SendSuccess();
                    //}
                    //else
                    //{
                    //    SendItemFailed(itemObj.ToString());
                    //}
                }
                else
                {
                    itemObj = new object();
                    SendItemFailed(itemObj.ToString());
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_SendMail(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                object serverIdObj;
                object uidObj;
                object mailIdObj;
                object rewardObj;
                if (dicMsg.TryGetValue("serverId", out serverIdObj) && dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("mailId", out mailIdObj))
                {
                    string reward = string.Empty;
                    if(dicMsg.TryGetValue("reward", out rewardObj))
                    {
                        reward = rewardObj.ToString();
                    }
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    int mailId = Convert.ToInt32(mailIdObj);
                    Log.Warn("custom client request send email server id {0} uid {1} mail {2} reward {3}", serverId, uid, mailId, reward);

                    if(serverId == 0)
                    {
                    
                        foreach(var relation in api.RelationServerManager.ServerList)
                        {
                            MSG_GR_SEND_MAIL notify = new MSG_GR_SEND_MAIL();
                            notify.Uid = uid;
                            //notify.MailId = relation.Value.MainId;
                            notify.MailId = mailId;
                            notify.Reward = reward;
                            relation.Value.Write(notify);
                        }
                        SendSuccess();
                    }
                    else
                    {
                        FrontendServer rServer = api.RelationServerManager.GetSinglePointServer(serverId);
                        if (rServer != null)
                        {
                            MSG_GR_SEND_MAIL notify = new MSG_GR_SEND_MAIL();
                            notify.Uid = uid;
                            //notify.MailId = serverId;
                            notify.MailId = mailId;
                            notify.Reward = reward;
                            rServer.Write(notify);
                            SendSuccess();
                        }
                        else
                        {
                            SendFailed();
                        }
                    }
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_BadWorlds(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1,"uid":1，serverId:2,"mailId":10}
                object contentObj;
                if (dicMsg.TryGetValue("content", out contentObj))
                {
                    MSG_GM_BAD_WORDS notify = new MSG_GM_BAD_WORDS();
                    notify.Content = contentObj.ToString();
                    api.ManagerServerManager.Broadcast(notify);
                    SendSuccess();
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }


        public void OnResponse_PlayerLevel(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1,"uid":1,"level":2}
                object serverIdObj;
                object uidObj;
                object levelObj;
                FrontendServer manager = null;
                if (dicMsg.TryGetValue("serverId", out serverIdObj) && dicMsg.TryGetValue("uid", out uidObj)
                    && dicMsg.TryGetValue("level", out levelObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    int level = Convert.ToInt32(levelObj);
                    manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager == null)
                    { 
                        SendFailed();
                        return;
                    }
                    MSG_GM_PLAYER_LEVEL request = new MSG_GM_PLAYER_LEVEL();
                    request.MainId = serverId;
                    request.Uid = uid;
                    request.Level = level;
                    manager.Write(request);
                    SendSuccess();
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_PlayerExp(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1,"uid":1,"hero":2}
                object serverIdObj;
                object uidObj;
                object expObj;
                FrontendServer manager = null;
                if (dicMsg.TryGetValue("serverId", out serverIdObj) && dicMsg.TryGetValue("uid", out uidObj)
                    && dicMsg.TryGetValue("exp", out expObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    uint exp = Convert.ToUInt32(expObj);
                    manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if(manager == null)
                    {
                        SendFailed();
                        return;
                    }
                    MSG_GM_PLAYER_EXP request = new MSG_GM_PLAYER_EXP();
                    request.MainId = serverId;
                    request.Uid = uid;
                    request.Exp = exp;
                    manager.Write(request);
                    SendSuccess();
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }
      
        public void OnResponse_TipOffInfo(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"type":1, "startServerId":1001, "endServerId":1002, "curPage":1, "pageSize":20, "time:"yyyy-MM-dd HH:mm:ss"}
                object typeObj;
                object startServerIdObj;
                object endServerIdObj;
                object curPageObj;
                object pageSizeObj;
                //object timeObj;
                if (dicMsg.TryGetValue("type", out typeObj) && dicMsg.TryGetValue("startServerId", out startServerIdObj) && dicMsg.TryGetValue("endServerId", out endServerIdObj)
                    && dicMsg.TryGetValue("curPage", out curPageObj) && dicMsg.TryGetValue("pageSize", out pageSizeObj))//&& dicMsg.TryGetValue("time", out timeObj)
                {
                    int type = Convert.ToInt32(typeObj);
                    int startServerId = Convert.ToInt32(startServerIdObj);
                    int endServerId = Convert.ToInt32(endServerIdObj);
                    int curPage = Convert.ToInt32(curPageObj);
                    int pageSize = Convert.ToInt32(pageSizeObj);
                    //DateTime.TryParse(timeObj.ToString(), out time);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(startServerId);//
                    if (manager != null)
                    {
                        MSG_GM_TIP_OFF_INFO request = new MSG_GM_TIP_OFF_INFO();
                        request.CustomUid = Uid;
                        request.Type = type;
                        request.StartServerId = startServerId;
                        request.EndServerId = endServerId;
                        request.CurPage = curPage;
                        request.PageSize = pageSize;
                        //request.Time = time.ToString();
                        manager.Write(request);
                    }
                    else
                    {
                        SendFailed();
                    }
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_IgnoreTipOff(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"id":1, "serverId": 1001}
                object idObj;
                object serverIdObj;
                if (dicMsg.TryGetValue("id", out idObj) && dicMsg.TryGetValue("serverId", out serverIdObj))
                {
                    int id = Convert.ToInt32(idObj);
                    int serverId = Convert.ToInt32(serverIdObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_IGNORE_TIP_OFF request = new MSG_GM_IGNORE_TIP_OFF();
                        request.CustomUid = Uid;
                        request.Id = id;                                
                        manager.Write(request);
                    }
                    else
                    {
                        SendFailed();
                    }
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_SendPeronEmail(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"mailId": 1, "serverIdAndUid":"1001:100100002|1002:100100003", "reward": ""}
                object serverIdAndUidObj;              
                object mailIdObj;
                object rewardObj;
                if (dicMsg.TryGetValue("serverIdAndUid", out serverIdAndUidObj) && dicMsg.TryGetValue("mailId", out mailIdObj))
                {
                    string reward = string.Empty;
                    if (dicMsg.TryGetValue("reward", out rewardObj))
                    {
                        reward = rewardObj.ToString();
                    }
                    string serverIdAndUid = serverIdAndUidObj.ToString();
                    int mailId = Convert.ToInt32(mailIdObj);
                    Log.Write("custom client request send person email server id and uid {0} mail {1} reward {2}", serverIdAndUid, mailId, reward);
                 
                    string[] serverAndUidArr = StringSplit.GetArray("|", serverIdAndUid);
                    if (serverAndUidArr.Length > 0)
                    {
                        bool hasSuccess = false;
                        string[] tempArr;
                        foreach (string item in serverAndUidArr)
                        {
                            tempArr = StringSplit.GetArray(":", item);
                            if (tempArr.Length == 2)
                            {
                                int serverId;
                                int.TryParse(tempArr[0], out serverId);
                                int uid;
                                int.TryParse(tempArr[1], out uid);
                                FrontendServer rServer = api.RelationServerManager.GetSinglePointServer(serverId);
                                if (rServer != null)
                                {
                                    MSG_GR_SEND_MAIL notify = new MSG_GR_SEND_MAIL();
                                    notify.Uid = uid;
                                    notify.MailId = mailId;
                                    notify.Reward = reward;
                                    rServer.Write(notify);
                                    hasSuccess = true;
                                }
                                else
                                {
                                    Log.Warn($"custom client request send person email serverId {serverId} uid {uid} reward {reward} failed: not find rServer");
                                }
                            }
                        }
                        if (hasSuccess)
                        {
                            SendSuccess();
                        }
                        else
                        {
                            SendFailed();
                        }
                    }
                    else
                    {
                        SendFailed();
                    }
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_GetItemInfo(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                ReqGetItemInfo reqMsg = serializer.Deserialize<ReqGetItemInfo>(jsonStr);
                if (reqMsg == null)
                {
                    SendFailed();
                    return;
                }

                FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(reqMsg.ServerId);
                if (manager == null)
                {
                    SendFailed();
                    return;
                }

                MSG_GM_GET_ITEM_INFO request = new MSG_GM_GET_ITEM_INFO();
                request.ServerId = reqMsg.ServerId;
                request.UserId = reqMsg.UserId;
                request.RewardType = reqMsg.RewardType;
                request.ItemId = reqMsg.ItemId;

                manager.Write(request, Uid);
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_ChangeItemNum(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                ReqDeleteItemNum reqMsg = serializer.Deserialize<ReqDeleteItemNum>(jsonStr);
                if (reqMsg == null)
                {
                    SendFailed();
                    return;
                }

                FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(reqMsg.ServerId);
                if (manager == null)
                {
                    SendFailed();
                    return;
                }

                MSG_GM_DEL_ITEM_NUM request = new MSG_GM_DEL_ITEM_NUM
                {
                    UserId = reqMsg.UserId,
                    RewardType = reqMsg.RewardType,
                    ItemId = reqMsg.ItemId,
                    Num = reqMsg.ItemNum,
                    ItemUid = reqMsg.ItemUid
                };

                manager.Write(request, Uid);
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public void OnResponse_DeleteActiveProgress(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                ReqDelActivityProgress reqMsg = serializer.Deserialize<ReqDelActivityProgress>(jsonStr);
                if (reqMsg == null)
                {
                    SendFailed();
                    return;
                }

                FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(reqMsg.ServerId);
                if (manager == null)
                {
                    SendFailed();
                    return;
                }

                MSG_GM_DEL_ACTIVITY_PROGRESS request = new MSG_GM_DEL_ACTIVITY_PROGRESS
                {
                    ServerId = reqMsg.ServerId,
                    UserId = reqMsg.UserId,
                    ActivityType = reqMsg.ActivityType,
                    Num = reqMsg.Num,
                    Price = reqMsg.Price,
                };

                manager.Write(request, Uid);
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }


    }
}
