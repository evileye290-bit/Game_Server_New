using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Global.Protocol.GA;
using Message.Global.Protocol.GB;
using Message.Global.Protocol.GM;
using Message.Global.Protocol.GR;
using Message.Global.Protocol.GZ;
using ServerFrame;
using ServerModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace GlobalServerLib
{
    public partial class Client
    {
        public void OnResponse_ArenaInfo(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1,"uid":1}
                object serverIdObj;
                object uidObj;
                if (dicMsg.TryGetValue("serverId", out serverIdObj) && dicMsg.TryGetValue("uid", out uidObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager == null)
                    {
                        SendFailed();
                        return;
                    }
                    MSG_GM_ARENA_INFO notify = new MSG_GM_ARENA_INFO();
                    notify.MainId = serverId;
                    notify.Uid = Convert.ToInt32(uidObj);
                    notify.CustomUid = Uid;
                    manager.Write(notify);
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

        public void OnResponse_FamilyInfo(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1,"uid":1}
                object serverIdObj;
                object familyNameObj;
                if (dicMsg.TryGetValue("serverId", out serverIdObj) && dicMsg.TryGetValue("familyName", out familyNameObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager == null)
                    {
                        SendFailed();
                        return;
                    }
                    MSG_GM_FAMILY_INFO notify = new MSG_GM_FAMILY_INFO();
                    notify.CustomUid = Uid;
                    notify.MainId = serverId;
                    notify.FamilyName = familyNameObj.ToString();
                    manager.Write(notify);
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

        public void OnResponse_ServerInfo(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1,"uid":1}
                object serverIdObj;
                if (dicMsg.TryGetValue("serverId", out serverIdObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager == null)
                    {
                        SendFailed();
                        return;
                    }
                    MSG_GM_SERVER_INFO notify = new MSG_GM_SERVER_INFO();
                    notify.CustomUid = Uid;
                    notify.MainId = serverId;
                    manager.Write(notify);
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

        public void OnResponse_GiftCode(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1,"uid":1}
                object serverIdObj;
                object uidObj;
                object giftCodeObj;
                if (dicMsg.TryGetValue("serverId", out serverIdObj) && dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("giftCode", out giftCodeObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager == null)
                    {
                        SendFailed();
                        return;
                    }
                    MSG_GM_GIFT_CODE notify = new MSG_GM_GIFT_CODE();
                    notify.CustomUid = Uid;
                    notify.MainId = serverId;
                    notify.Uid = Convert.ToInt32(uidObj);
                    notify.Code = giftCodeObj.ToString();
                    manager.Write(notify);
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

        public void OnResponse_GameCounter(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1,"uid":1}
                object serverIdObj;
                object uidObj;
                if (dicMsg.TryGetValue("serverId", out serverIdObj) && dicMsg.TryGetValue("uid", out uidObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager == null)
                    {
                        SendFailed();
                        return;
                    }
                    MSG_GM_GAME_COUNTER notify = new MSG_GM_GAME_COUNTER();
                    notify.CustomUid = Uid;
                    notify.MainId = serverId;
                    notify.Uid = Convert.ToInt32(uidObj);
                    manager.Write(notify);
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

        public void OnResponse_ChangeFamilyName(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1,"oldName":"a", "newName":"b"}
                object serverIdObj;
                object oldNameObj;
                object newNameObj;
                if (dicMsg.TryGetValue("serverId", out serverIdObj) && dicMsg.TryGetValue("oldName", out oldNameObj) && dicMsg.TryGetValue("newName", out newNameObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager == null)
                    {
                        SendFailed();
                        return;
                    }
                    MSG_GM_CHANGE_FAMLIY_NAME notify = new MSG_GM_CHANGE_FAMLIY_NAME();
                    notify.CustomUid = Uid;
                    notify.MainId = serverId;
                    notify.OldFamilyName = oldNameObj.ToString();
                    notify.NewFamilyName = newNameObj.ToString();
                    manager.Write(notify);
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

        public void OnResponse_RecommendServer(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1,"oldName":"a", "newName":"b"}
                object serversObj;
                if (dicMsg.TryGetValue("servers", out serversObj))
                {
                    //{servers:"1|2"}
                    MSG_GB_RECOMMEND_SERVERS request = new MSG_GB_RECOMMEND_SERVERS();
                    string[] servers = serversObj.ToString().Split('|');
                    foreach (var item in servers)
                    {
                        request.Servers.Add(int.Parse(item));
                    }
                    api.BarrackServerManager.Broadcast(request);
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

        public void OnResponse_CharAllInfo(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1001,"uid":100,charName:"aaa"}
                object serverIdObj;
                object uidObj;
                object charNameObj;
                if (dicMsg.TryGetValue("serverId", out serverIdObj) && dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("name", out charNameObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    FrontendServer zone = api.ZoneServerManager.GetOneServer(serverId);
                    if (zone == null)
                    {
                        SendFailed();
                        return;
                    }
                    MSG_GZ_GM_CHARACTER_INFO request = new MSG_GZ_GM_CHARACTER_INFO();
                    request.Uid = uid;
                    request.CustomUid = Uid;
                    request.Name = charNameObj.ToString();
                    zone.Write(request);
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


        public void OnResponse_HeroList(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1001,"uid":100,charName:"aaa"}
                object serverIdObj;
                object uidObj;
                object charNameObj;
                if (dicMsg.TryGetValue("serverId", out serverIdObj) && dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("name", out charNameObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    FrontendServer zone = api.ZoneServerManager.GetOneServer(serverId);
                    if (zone == null)
                    {
                        SendFailed();
                        return;
                    }
                    MSG_GZ_GM_HERO_LIST request = new MSG_GZ_GM_HERO_LIST();
                    request.Uid = uid;
                    request.CustomUid = Uid;
                    zone.Write(request);
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


        public void OnResponse_ItemTypeList(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverID":1,"uid":100,charName:"aaa"}
                object uidObj;
                object typeObj;
                object serverIdObj;
                if (dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("serverId", out serverIdObj)
                     && dicMsg.TryGetValue("itemType", out typeObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_ITEM_TYPE_LIST request = new MSG_GM_ITEM_TYPE_LIST();
                        request.CustomUid = Uid;
                        request.Uid = uid;
                        request.ItemType = Convert.ToInt32(typeObj);
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

        public void OnResponse_PetTypeList(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverID":1,"uid":100,charName:"aaa"}
                object uidObj;
                object typeObj;
                object serverIdObj;
                if (dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("serverId", out serverIdObj)
                     && dicMsg.TryGetValue("petType", out typeObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_PET_TYPE_LIST request = new MSG_GM_PET_TYPE_LIST();
                        request.CustomUid = Uid;
                        request.Uid = uid;
                        request.PetType = Convert.ToInt32(typeObj);
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

        public void OnResponse_PetMountList(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverID":1,"uid":100,charName:"aaa"}
                object uidObj;
                object serverIdObj;
                if (dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("serverId", out serverIdObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_PET_MOUNT_LIST request = new MSG_GM_PET_MOUNT_LIST();
                        request.CustomUid = Uid;
                        request.Uid = uid;
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

        public void OnResponse_DeletePet(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverID":1,"uid":100,charName:"aaa"}
                object uidObj;
                object serverIdObj;
                object petUidObj;
                if (dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("serverId", out serverIdObj) && dicMsg.TryGetValue("petUid", out petUidObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_DELETE_PET request = new MSG_GM_DELETE_PET();
                        request.CustomUid = Uid;
                        request.Uid = uid;
                        request.PetUid = Convert.ToUInt64(petUidObj);
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

        public void OnResponse_DeletePetMount(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverID":1,"uid":100,charName:"aaa"}
                object uidObj;
                object serverIdObj;
                object petMountTypeObj;
                if (dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("serverId", out serverIdObj) && dicMsg.TryGetValue("petMountType", out petMountTypeObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_DELETE_PET_MOUNT request = new MSG_GM_DELETE_PET_MOUNT();
                        request.CustomUid = Uid;
                        request.Uid = uid;
                        request.PetMountType = Convert.ToInt32(petMountTypeObj);
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

        public void OnResponse_EquipList(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverID":1,"uid":100,charName:"aaa"}
                object uidObj;
                object serverIdObj;
                if (dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("serverId", out serverIdObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_EQUIP_LIST request = new MSG_GM_EQUIP_LIST();
                        request.CustomUid = Uid;
                        request.Uid = uid;
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

        public void OnResponse_PetList(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverID":1,"uid":100,charName:"aaa"}
                object uidObj;
                object serverIdObj;
                if (dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("serverId", out serverIdObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_PET_LIST request = new MSG_GM_PET_LIST();
                        request.CustomUid = Uid;
                        request.Uid = uid;
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

        public void OnResponse_PetMountStrength(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverID":1,"uid":100,charName:"aaa"}
                object uidObj;
                object serverIdObj;
                if (dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("serverId", out serverIdObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_PET_MOUNT_STRENGTH request = new MSG_GM_PET_MOUNT_STRENGTH();
                        request.CustomUid = Uid;
                        request.Uid = uid;
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

        public void OnResponse_DeleteItem(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                object uidObj;
                object serverIdObj;
                object itemUidObj;
                if (dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("serverId", out serverIdObj) && dicMsg.TryGetValue("itemUid", out itemUidObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    ulong itemUid = Convert.ToUInt64(itemUidObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_DELETE_ITEM request = new MSG_GM_DELETE_ITEM();
                        request.CustomUid = Uid;
                        request.Uid = uid;
                        request.ItemUid = itemUid;
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

        public void OnResponse_DeleteChar(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                object uidObj;
                object serverIdObj;
                if (dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("serverId", out serverIdObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_DELETE_CHAR request = new MSG_GM_DELETE_CHAR();
                        request.CustomUid = Uid;
                        request.Uid = uid;
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

        public void OnResponse_ServerList(MemoryStream stream)
        {
            ServerList result = new ServerList();
            foreach (var item in api.ManagerServerManager.ServerList)
            {
                ManagerServer server = (ManagerServer)item.Value;
                result.servers.Add(server.MainId);
            }
            result.servers.Sort((left, right) =>
            {
                if (left < right)
                {
                    return -1;
                }
                return 1;
            });
            var jser = new JavaScriptSerializer();
            string json = jser.Serialize(result);
            WriteString(json);
        }

        public void OnResponse_RecentLoginServers(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                object accountNameObj;
                if (dicMsg.TryGetValue("accountName", out accountNameObj))
                {
                    RecentLoginServers result = new RecentLoginServers();
                    result.accountName = accountNameObj.ToString();
                    result.recentServers = new List<int>();

                    QueryRecentLoginServers query = new QueryRecentLoginServers(result.accountName, result.recentServers);
                    api.AccountDBPool.Call(query, (ret) =>
                     {
                         var jser = new JavaScriptSerializer();
                         string json = jser.Serialize(result);
                         WriteString(json);
                     });
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

        public void OnResponse_CharacterListByAccountName(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                object accountNameObj;
                //object serverIdObj;
                if (dicMsg.TryGetValue("accountName", out accountNameObj))// && dicMsg.TryGetValue("serverId", out serverIdObj))
                {
                    string accountName = accountNameObj.ToString();

                    //int serverId = Convert.ToInt32(serverIdObj);
                    //MServer mserver;
                    //if (server.GetManagerServer(serverId, out mserver))
                    //{
                    //    MSG_GM_CHARACTER_LIST_BY_ACCOUNT_NAME request = new MSG_GM_CHARACTER_LIST_BY_ACCOUNT_NAME();
                    //    request.CustomUid = Uid;
                    //    request.AccountName = accountNameObj.ToString();
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

        public void OnResponse_OrderList(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                object serverIdObj, uidObj, startTimeObj, endTimeObj, pageObj, pageSize, orderIdObj, orderInfoObj;
                DateTime startTime = DateTime.MinValue;
                DateTime endTime = DateTime.MaxValue;
                int page = 0;
                if (dicMsg.TryGetValue("serverId", out serverIdObj) && 
                    dicMsg.TryGetValue("uid", out uidObj) && 
                    dicMsg.TryGetValue("startTime", out startTimeObj)&& 
                    dicMsg.TryGetValue("endTime", out endTimeObj) && 
                    DateTime.TryParse(startTimeObj.ToString(), out startTime)&& 
                    DateTime.TryParse(endTimeObj.ToString(), out endTime) && 
                    dicMsg.TryGetValue("orderId", out orderIdObj)&& 
                    dicMsg.TryGetValue("orderInfo", out orderInfoObj)&& 
                    dicMsg.TryGetValue("page", out pageObj) && 
                    dicMsg.TryGetValue("pageSize", out pageSize) &&
                    int.TryParse(pageObj.ToString(), out page))
                {

                    int serverId = Convert.ToInt32(serverIdObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {

                        MSG_GM_ORDER_LIST request = new MSG_GM_ORDER_LIST();
                        request.CustomUid = Uid;
                        request.Uid = Convert.ToInt32(uidObj);
                        request.Page = page;
                        request.PageSize = Convert.ToInt32(pageSize);
                        request.OrderId = orderIdObj.ToString();
                        request.OrderInfo = orderInfoObj.ToString();
                        request.StartTime = startTime.ToString();
                        request.EndTime = endTime.ToString();
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

        public void OnResponse_GMBadWords(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"content":"gm"}
                object contentObj;
                if (dicMsg.TryGetValue("content", out contentObj))
                {
                    MSG_GM_BAD_WORDS notify = new MSG_GM_BAD_WORDS();
                    notify.Content = contentObj.ToString();
                    api.ManagerServerManager.Broadcast(notify);
                    // 同步db
                    api.AccountDBPool.Call(new QueryInsertBadWord(contentObj.ToString()));
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

        public void OnResponse_SpecItem(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                object serverIdObj;
                object itemUidObj;
                DateTime startTime = new DateTime();
                startTime = DateTime.MinValue;
                DateTime endTime = new DateTime();
                endTime = DateTime.MaxValue;
                if (dicMsg.TryGetValue("serverId", out serverIdObj) && dicMsg.TryGetValue("itemUid", out itemUidObj))
                {

                    int serverId = Convert.ToInt32(serverIdObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_SPEC_ITEM request = new MSG_GM_SPEC_ITEM();
                        request.CustomUid = Uid;
                        request.ItemUid = Convert.ToUInt64(itemUidObj);
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

        public void OnResponse_SpecPet(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                object serverIdObj;
                object petUidObj;
                if (dicMsg.TryGetValue("serverId", out serverIdObj) && dicMsg.TryGetValue("petUid", out petUidObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_SPEC_PET request = new MSG_GM_SPEC_PET();
                        request.CustomUid = Uid;
                        request.PetUid = Convert.ToUInt64(petUidObj);
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

        public void OnResponse_UpdateItemCount(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1,"itemUid":1，count:2}
                object serverIdObj;
                object itemUidObj;
                object countObj;
                if (dicMsg.TryGetValue("itemUid", out itemUidObj) && dicMsg.TryGetValue("serverId", out serverIdObj)
                    && dicMsg.TryGetValue("count", out countObj))
                {
                    MSG_GM_UPDATE_ITEM_COUNT notify = new MSG_GM_UPDATE_ITEM_COUNT();
                    notify.ItemUid = Convert.ToUInt64(itemUidObj);
                    notify.Count = Convert.ToInt32(countObj);
                    int serverId = Convert.ToInt32(serverIdObj);
                    if (notify.Count <= 0)
                    {
                        SendFailed();
                        return;
                    }
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        manager.Write(notify);
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

        public void OnResponse_SpecEmail(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                //{"serverId":1,"itemUid":1，count:2}
                //object serverIdObj;
                object titleObj;
                object contentObj;
                object uidObj;
                object rewardObj;
                object senderNameObj;
                if (dicMsg.TryGetValue("title", out titleObj) && dicMsg.TryGetValue("content", out contentObj) && dicMsg.TryGetValue("uid", out uidObj)
                    && dicMsg.TryGetValue("reward", out rewardObj) && dicMsg.TryGetValue("senderName", out senderNameObj))
                {
                    //int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);

                    FrontendServer rServer = api.RelationServerManager.GetWatchDogServer();
                    if (rServer != null)
                    {
                        MSG_GR_SPEC_EMAIL notify = new MSG_GR_SPEC_EMAIL();
                        //notify.serverId = serverId;
                        notify.Uid = uid;
                        notify.Title = titleObj.ToString();
                        notify.Content = contentObj.ToString();
                        notify.CustomUid = Uid;
                        notify.Reward = rewardObj.ToString();
                        notify.SenderName = rewardObj.ToString();
                        rServer.Write(notify);
                        SendSuccess();
                    }
                    else
                    {
                        SendFailed();
                    }

                    //if (serverId == 0)
                    //{
                    //    foreach(var item in server.ManagerManager.AllManagers)
                    //    {

                    //        MSG_GM_SPEC_EMAIL notify = new MSG_GM_SPEC_EMAIL();
                    //        notify.serverId = serverId;
                    //        notify.Uid = uid;
                    //        notify.title = titleObj.ToString();
                    //        notify.content = contentObj.ToString();
                    //        notify.CustomUid = Uid;
                    //        notify.reward = rewardObj.ToString();
                    //        notify.senderName = rewardObj.ToString();
                    //        item.Write(notify);
                    //    }
                    //    SendSuccess();
                    //    return;
                    //}
                    //else
                    //{

                    //    MServer mserver;
                    //    MSG_GM_SPEC_EMAIL notify = new MSG_GM_SPEC_EMAIL();
                    //    notify.serverId = serverId;
                    //    notify.Uid = uid;
                    //    notify.title = titleObj.ToString();
                    //    notify.content = contentObj.ToString();
                    //    notify.CustomUid = Uid;
                    //    notify.reward = rewardObj.ToString();
                    //    if (server.GetManagerServer(serverId, out mserver))
                    //    {
                    //        mserver.Write(notify);
                    //        SendSuccess();
                    //    }
                    //    else
                    //    {
                    //        SendFailed();
                    //    }
                    //}
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

        public void OnResponse_UpdateCharData(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);
                object serverIdObj;
                object uidObj;
                object dataTypeObj;
                object dataValueObj;
                if (dicMsg.TryGetValue("serverId", out serverIdObj) && dicMsg.TryGetValue("uid", out uidObj) && dicMsg.TryGetValue("dataType", out dataTypeObj)
                    && dicMsg.TryGetValue("dataValue", out dataValueObj))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);
                    FrontendServer manager = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (manager != null)
                    {
                        MSG_GM_UPDATE_CHAR_DATA notify = new MSG_GM_UPDATE_CHAR_DATA();
                        notify.CustomUid = Uid;
                        notify.ServerId = serverId;
                        notify.Uid = uid;
                        notify.DataType = Convert.ToInt32(dataTypeObj);
                        notify.DataValue = Convert.ToInt32(dataValueObj);
                        manager.Write(notify);
                        Log.Write("gm request update server {0} uid {1} type {2} value {3}", serverId, uid, notify.DataType, notify.DataValue);
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

        public void OnResponse_ZoneTransform(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                //{"serverId":1001,isForce:false,"fromZones":"1-2-3-4-5",toZones:"6-7-8-9-10"}
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);

                object serverIdObj, fromStr, toStr, isForce;
                if (dicMsg.TryGetValue("serverId", out serverIdObj)
                    && dicMsg.TryGetValue("isForce", out isForce)
                    && dicMsg.TryGetValue("fromZones", out fromStr)
                    && dicMsg.TryGetValue("toZones", out toStr))
                {
                    int serverId = Convert.ToInt32(serverIdObj);
                    List<int> fromZones = fromStr.ToString().Split('-').ToList().ConvertAll(x => int.Parse(x));
                    List<int> toZones = toStr.ToString().Split('-').ToList().ConvertAll(x => int.Parse(x));

                    bool success = fromZones.Count != 0 && toZones.Count != 0;
                    List<FrontendServer> zoneServeres = new List<FrontendServer>();
                    foreach (var kv in fromZones)
                    {
                        FrontendServer zone = api.ZoneServerManager.GetServer(serverId, kv);
                        if (zone == null)
                        {
                            success = false;
                            break;
                        }
                        zoneServeres.Add(zone);
                    }

                    if (!success)
                    {
                        SendFailed();
                        return;
                    }

                    FrontendServer managerServer = api.ManagerServerManager.GetSinglePointServer(serverId);
                    if (managerServer == null)
                    {
                        SendFailed();
                        return;
                    }

                    //发送当当前server manager
                    MSG_GM_ZONE_TRANSFORM requestM = new MSG_GM_ZONE_TRANSFORM() { MainId = serverId, IsForce = (bool)isForce };
                    requestM.FromZones.AddRange(fromZones);
                    requestM.ToZones.AddRange(toZones);
                    managerServer.Write(requestM);
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

        public void OnResponse_AddWelfareStall(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                //{"serverId":1001,isForce:false,"fromZones":"1-2-3-4-5",toZones:"6-7-8-9-10"}
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);

                object nameObj, firstEmailObj, fixedEmailObj, sendTypeObj, dayObj, timeObj, idObj;
                if (dicMsg.TryGetValue("name", out nameObj)
                    && dicMsg.TryGetValue("firstEmail", out firstEmailObj)
                    && dicMsg.TryGetValue("fixedEmail", out fixedEmailObj)
                    && dicMsg.TryGetValue("sendType", out sendTypeObj)
                    && dicMsg.TryGetValue("day", out dayObj)
                    && dicMsg.TryGetValue("time", out timeObj)
                    && dicMsg.TryGetValue("id", out idObj))
                {
                    int id = Convert.ToInt32(idObj);
                    string name = nameObj.ToString();
                    int firstEmail = Convert.ToInt32(firstEmailObj);
                    int fixedEmail = Convert.ToInt32(fixedEmailObj);
                    int sendType = Convert.ToInt32(sendTypeObj);
                    int day = Convert.ToInt32(dayObj);
                    string time = timeObj.ToString();

                    WelfareStallItem model = new WelfareStallItem();
                    model.Id = id;
                    model.Name = name;
                    model.FirstEmail = firstEmail;
                    model.FixedEmail = fixedEmail;
                    model.Type = (WelfareSendType)sendType;
                    model.Day = day;
                    model.Time = TimeSpan.Parse(time);

                    bool success = api.WelfareMng.AddStall(model);

                    if (!success)
                    {
                        SendFailed();
                        return;
                    }
                    else
                    {
                        SendSuccess();
                    }
                    //WelfareStallInfoList result = api.WelfareMng.GetWelfareStallInfoList();
                    //var jser = new JavaScriptSerializer();
                    //string json = jser.Serialize(result);
                    //WriteString(json);

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

        public void OnResponse_AddWelfarePlayer(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                //{"serverId":1001,isForce:false,"fromZones":"1-2-3-4-5",toZones:"6-7-8-9-10"}
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);

                object idObj, serverIdObj, uidObj;
                if (dicMsg.TryGetValue("id", out idObj)
                    && dicMsg.TryGetValue("serverId", out serverIdObj)
                    && dicMsg.TryGetValue("uid", out uidObj))
                {
                    int id = Convert.ToInt32(idObj);
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);

                    WelfarePlayerItem model = new WelfarePlayerItem();
                    model.Id = id;
                    model.ServerId = serverId;
                    model.Uid = uid;

                    bool success = api.WelfareMng.AddPlayer(model);
                    if (!success)
                    {
                        SendFailed();
                        return;
                    }
                    else
                    {
                        SendSuccess();
                    }
                    //WelfarePlayerInfoList result = api.WelfareMng.GetWelfarePlayerInfoList();
                    //var jser = new JavaScriptSerializer();
                    //string json = jser.Serialize(result);
                    //WriteString(json);
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

        public void OnResponse_DeleteWelfareStall(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                //{"serverId":1001,isForce:false,"fromZones":"1-2-3-4-5",toZones:"6-7-8-9-10"}
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);

                object idObj;
                if (dicMsg.TryGetValue("id", out idObj))
                {
                    int id = Convert.ToInt32(idObj);

                    bool success = api.WelfareMng.DeleteStall(id);

                    if (!success)
                    {
                        SendFailed();
                        return;
                    }
                    else
                    {
                        SendSuccess();
                    }
                    //WelfareStallInfoList result = api.WelfareMng.GetWelfareStallInfoList();
                    //var jser = new JavaScriptSerializer();
                    //string json = jser.Serialize(result);
                    //WriteString(json);

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

        public void OnResponse_DeleteWelfarePlayer(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                //{"serverId":1001,isForce:false,"fromZones":"1-2-3-4-5",toZones:"6-7-8-9-10"}
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);

                object idObj, serverIdObj, uidObj;
                if (dicMsg.TryGetValue("id", out idObj)
                    && dicMsg.TryGetValue("serverId", out serverIdObj)
                    && dicMsg.TryGetValue("uid", out uidObj))
                {
                    int id = Convert.ToInt32(idObj);
                    int serverId = Convert.ToInt32(serverIdObj);
                    int uid = Convert.ToInt32(uidObj);

                    bool success = api.WelfareMng.DeletePlayer(id, serverId, uid);
                    if (!success)
                    {
                        SendFailed();
                        return;
                    }
                    else
                    {
                        SendSuccess();
                    }
                    //WelfarePlayerInfoList result = api.WelfareMng.GetWelfarePlayerInfoList();
                    //var jser = new JavaScriptSerializer();
                    //string json = jser.Serialize(result);
                    //WriteString(json);
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

        public void OnResponse_FindWelfareStall(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                //{"serverId":1001,isForce:false,"fromZones":"1-2-3-4-5",toZones:"6-7-8-9-10"}
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);

                object idObj;
                if (dicMsg.TryGetValue("id", out idObj))
                {
                    int id;
                    int.TryParse(idObj.ToString(), out id);

                    WelfareStallInfoList result = api.WelfareMng.FindStall(id);
                    var jser = new JavaScriptSerializer();
                    string json = jser.Serialize(result);
                    WriteString(json);
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

        public void OnResponse_FindWelfarePlayer(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                //{"serverId":1001,isForce:false,"fromZones":"1-2-3-4-5",toZones:"6-7-8-9-10"}
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);

                object serverIdObj, uidObj;
                if (dicMsg.TryGetValue("serverId", out serverIdObj)
                    && dicMsg.TryGetValue("uid", out uidObj))
                {
                    int serverId;
                    int.TryParse(serverIdObj.ToString(), out serverId);
                    int uid ;
                    int.TryParse(uidObj.ToString(), out uid);

                    WelfarePlayerInfoList result = api.WelfareMng.FindPlayer(serverId, uid);
                    var jser = new JavaScriptSerializer();
                    string json = jser.Serialize(result);
                    WriteString(json);
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

        public void OnResponse_ModifyWelfareStall(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                //{"serverId":1001,isForce:false,"fromZones":"1-2-3-4-5",toZones:"6-7-8-9-10"}
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);

                object idObj, nameObj, firstEmailObj, fixedEmailObj;
                if (dicMsg.TryGetValue("id", out idObj)
                    && dicMsg.TryGetValue("name", out nameObj)
                    && dicMsg.TryGetValue("firstEmail", out firstEmailObj)
                    && dicMsg.TryGetValue("fixedEmail", out fixedEmailObj))
                {
                    int id = Convert.ToInt32(idObj);
                    string name = nameObj.ToString();
                    int firstEmail = Convert.ToInt32(firstEmailObj);
                    int fixedEmail = Convert.ToInt32(fixedEmailObj);


                    bool success = api.WelfareMng.ModifyStall(id, name, firstEmail, fixedEmail);

                    if (!success)
                    {
                        SendFailed();
                        return;
                    }
                    else
                    {
                        SendSuccess();
                    }
                    //WelfareStallInfoList result = api.WelfareMng.GetWelfareStallInfoList();
                    //var jser = new JavaScriptSerializer();
                    //string json = jser.Serialize(result);
                    //WriteString(json);

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

        private bool CheckParamAndBuildInfo(Dictionary<string, object> dicMsg, MSG_GA_COMMON_INFO info)
        {
            object serverIdObj, uidObj, fromTime, toTime, page, pageSize;
            if (dicMsg.TryGetValue("serverId", out serverIdObj)
                && dicMsg.TryGetValue("uid", out uidObj)
                && dicMsg.TryGetValue("fromTime", out fromTime)
                && dicMsg.TryGetValue("endTime", out toTime)
                && dicMsg.TryGetValue("page", out page)
                && dicMsg.TryGetValue("pageSize", out pageSize))
            {
                DateTime startTime = Convert.ToDateTime(fromTime);
                DateTime endTime = Convert.ToDateTime(toTime);

                info.ServerId = Convert.ToInt32(serverIdObj);
                info.Uid = Convert.ToInt32(uidObj);
                info.FromTime = Timestamp.GetUnixTimeStampSeconds(startTime);
                info.ToTime = Timestamp.GetUnixTimeStampSeconds(endTime);
                info.Page = Convert.ToInt32(page);
                info.PageSize = Convert.ToInt32(pageSize);

                GMRecordCache.Instance.PageSize = info.PageSize;
                return true;
            }

            return false;
        }

        public void OnResponse_ItemProduce(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);

                MSG_GA_ITEM_PRODUCE msg = new MSG_GA_ITEM_PRODUCE() { Info = new MSG_GA_COMMON_INFO() };
                if (!CheckParamAndBuildInfo(dicMsg, msg.Info))
                {
                    Log.Warn("gm request ItemProduce {0}", jsonStr);
                    SendFailed();
                    return;
                }

                Log.Info("gm request ItemProduce {0}", jsonStr);

                if (msg.Info.Page == 1 || !GMRecordCache.Instance.SendClientItemProduce(this, msg.Info.Uid, msg.Info.Page))
                {
                    GMRecordCache.Instance.ClearItemProduce(msg.Info.Uid);
                    
                    object produceWay;
                    dicMsg.TryGetValue("produceWay", out produceWay);
                    msg.Way = produceWay.ToString();

                    SendToAnalysis(msg.Info.ServerId, msg);
                }
            }
            catch (Exception ex)
            {
                Log.Error("gm request ItemProduce {0} ex {1}", jsonStr, ex);
                SendFailed();
            }
        }

        public void OnResponse_ItemConsume(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);

                MSG_GA_ITEM_CONSUME msg = new MSG_GA_ITEM_CONSUME() { Info = new MSG_GA_COMMON_INFO() };
                if (!CheckParamAndBuildInfo(dicMsg, msg.Info))
                {
                    Log.Warn("gm request ItemConsume {0}", jsonStr);
                    SendFailed();
                    return;
                }

                Log.Info("gm request CurrencyProduce {0}", jsonStr);

                if (msg.Info.Page == 1 || !GMRecordCache.Instance.SendClientItemConsume(this, msg.Info.Uid, msg.Info.Page))
                {
                    GMRecordCache.Instance.ClearItemConsume(msg.Info.Uid);

                    object produceWay;
                    dicMsg.TryGetValue("consumeWay", out produceWay);
                    msg.Way = produceWay.ToString();

                    SendToAnalysis(msg.Info.ServerId, msg);
                }
            }
            catch (Exception ex)
            {
                Log.Error("gm request ItemConsume {0} ex {1}", jsonStr, ex);
                SendFailed();
            }
        }

        public void OnResponse_CurrencyProduce(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);

                MSG_GA_CURRENCY_PRODUCE msg = new MSG_GA_CURRENCY_PRODUCE() { Info = new MSG_GA_COMMON_INFO() };
                if (!CheckParamAndBuildInfo(dicMsg, msg.Info))
                {
                    Log.Warn("gm request CurrencyProduce {0}", jsonStr);
                    SendFailed();
                    return;
                }

                Log.Info("gm request CurrencyProduce {0}", jsonStr);

                if (msg.Info.Page == 1 || !GMRecordCache.Instance.SendClientCurrencyProduce(this, msg.Info.Uid, msg.Info.Page))
                {
                    GMRecordCache.Instance.ClearCurrencyProduce(msg.Info.Uid);

                    object produceWay;
                    dicMsg.TryGetValue("produceWay", out produceWay);
                    msg.Way = produceWay.ToString();

                    SendToAnalysis(msg.Info.ServerId, msg);
                }
            }
            catch (Exception ex)
            {
                SendFailed();
                Log.Error("gm request CurrencyProduce {0} ex {1}", jsonStr, ex);
            }
        }

        public void OnResponse_CurrencyConsume(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);

                MSG_GA_CURRENCY_CONSUME msg = new MSG_GA_CURRENCY_CONSUME() { Info = new MSG_GA_COMMON_INFO() };
                if (!CheckParamAndBuildInfo(dicMsg, msg.Info))
                {
                    Log.Warn("gm request CurrencyConsume {0}", jsonStr);
                    SendFailed();
                    return;
                }

                Log.Info("gm request CurrencyConsume {0}", jsonStr);
                
                if (msg.Info.Page == 1 || !GMRecordCache.Instance.SendClientCurrencyConsume(this, msg.Info.Uid, msg.Info.Page))
                {
                    GMRecordCache.Instance.ClearCurrencyConsume(msg.Info.Uid);

                    object produceWay;
                    dicMsg.TryGetValue("consumeWay", out produceWay);
                    msg.Way = produceWay.ToString();

                    SendToAnalysis(msg.Info.ServerId, msg);
                }
            }
            catch (Exception ex)
            {
                SendFailed();
                Log.Error("gm request CurrencyConsume {0} ex {1}", jsonStr, ex);
            }
        }

        public void OnResponse_LoginOrLogout(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);

                MSG_GA_LOGINORLOGOUT msg = new MSG_GA_LOGINORLOGOUT() { Info = new MSG_GA_COMMON_INFO() };
                if (!CheckParamAndBuildInfo(dicMsg, msg.Info))
                {
                    Log.Warn("gm request LoginOrLogout {0}", jsonStr);
                    SendFailed();
                    return;
                }

                Log.Info("gm request LoginOrLogout {0}", jsonStr);

                if (msg.Info.Page == 1 || !GMRecordCache.Instance.SendClientLoginLogout(this, msg.Info.Uid, msg.Info.Page))
                {
                    GMRecordCache.Instance.ClearLoginLogout(msg.Info.Uid);
                    
                    object login;
                    dicMsg.TryGetValue("isLogin", out login);
                    msg.IsLogin = "login".Equals(login);

                    SendToAnalysis(msg.Info.ServerId, msg);
                }
            }
            catch (Exception ex)
            {
                Log.Error("gm request LoginOrLogout {0} ex {1}", jsonStr, ex);
                SendFailed();
            }
        }

        private void SendToAnalysis<T>(int serverId, T msg) where T : Google.Protobuf.IMessage
        {
            AnalysisServer server = api.AnalysisServerManager.GetOneServer(serverId) as AnalysisServer;
            if (server == null)
            {
                Log.Info("not find AnalysisServer serverId {0}", serverId);
                SendFailed();
                return;
            }
            Log.Info("send to AnalysisServer serverId {0}", serverId);
            server.Write(msg, Uid);
        }

        private void OnResponse_GetServerState(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                //{page:1,pageCount:100}
                Dictionary<string, string> dicMsg = serializer.Deserialize<Dictionary<string, string>>(jsonStr);
                string pageStr, pageSizeStr;

                if (dicMsg.TryGetValue("page", out pageStr) &&
                    dicMsg.TryGetValue("pageSize", out pageSizeStr))
                {
                    int serverBegin = dicMsg.ContainsKey("serverBegin") && !string.IsNullOrEmpty(dicMsg["serverBegin"]) ? dicMsg["serverBegin"].ToInt() : 0;
                    int serverEnd = dicMsg.ContainsKey("serverEnd") && !string.IsNullOrEmpty(dicMsg["serverEnd"]) ? dicMsg["serverEnd"].ToInt() : int.MaxValue;
                    if (serverEnd <= 0)
                    {
                        serverEnd = int.MaxValue;
                    }

                    MSG_GB_GET_SERVER_STATE msg = new MSG_GB_GET_SERVER_STATE()
                    {
                        Uid = Uid,
                        Page = int.Parse(pageStr),
                        PageCount = int.Parse(pageSizeStr),
                        ServerBegin = serverBegin,
                        ServerEnd = serverEnd
                    } ;
                    api.BarrackServerManager.Broadcast(msg);
                }
                else
                {
                    SendFailed();
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
                SendFailed();
            }
        }

        private  void OnResponse_SetServerState(MemoryStream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            try
            {
                //{open:1,recommend:1,newServer:1, maintain:1,maintainAll:true}
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(jsonStr);

                object open = 0;
                object newServer = 0;
                object recommend = 0;
                object maintain = 0;
                object maintainAll = 0;
                object maintainAllCancel = 0;

                if (dicMsg.ContainsKey("open")) open = dicMsg["open"];
                if (dicMsg.ContainsKey("new")) newServer = dicMsg["new"];
                if (dicMsg.ContainsKey("recommend")) recommend = dicMsg["recommend"];
                if (dicMsg.ContainsKey("maintain")) maintain = dicMsg["maintain"];
                if (dicMsg.ContainsKey("maintainAll")) maintainAll = dicMsg["maintainAll"];
                if (dicMsg.ContainsKey("maintainAllCancel")) maintainAllCancel = dicMsg["maintainAllCancel"];

                MSG_GB_SET_SERVER_STATE request = new MSG_GB_SET_SERVER_STATE() { Uid = Uid };
                request.New=Convert.ToInt32( newServer);
                request.Open= Convert.ToInt32(open);
                request.Recommend= Convert.ToInt32(recommend);
                request.Mintain = Convert.ToInt32(maintain);
                request.MaintainAll = Convert.ToBoolean(maintainAll);
                request.CancelMaintainAll = Convert.ToBoolean(maintainAllCancel);

                api.BarrackServerManager.Broadcast(request);
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
                SendFailed();
            }
        }
    }
}