using System;
using System.Collections.Generic;
using DBUtility;
using Message.Barrack.Protocol.BM;
using Message.Pay.Protocol.PM;
using ServerFrame;
using ServerModels;
using ServerShared;

namespace PayServerLib
{
    public partial class VMallApiHandler
    {
        private PayServerApi server;

        public VMallApiHandler(PayServerApi server)
        {
            this.server = server;
            BindListener();
        }

        public void DistributeMessage(VMallSession session)
        {
            vmallCallBack[session.ApiName].Invoke(session);
        }

        private void QueryServerList(VMallSession session)
        {
            try
            {
                object uid;
                if (!session.Dic.TryGetValue("uid", out uid))
                {
                    VMResponse response = VMResponse.GetFail(VMallErrorCode.NoAccount, "server list error: not find uid ");
                    session.WriteResponse(response);
                    return;
                }

                object gameId;
                if (!session.Dic.TryGetValue("game_id", out gameId))
                {
                    VMResponse response = VMResponse.GetFail(VMallErrorCode.NoAccount, "server list error: not find game id ");
                    session.WriteResponse(response);
                }

                object lang;
                if (!session.Header.TryGetValue("lang", out lang))
                {
                    //VMResponse response = VMResponse.GetFail(VMallErrorCode.NoAccount, "server list error: not find lang ");
                    //session.WriteResponse(response);
                    VMallServer.LogList[LogType.WARN].Enqueue($"uid {uid} server list error: not find lang");
                    lang = "default";
                }

                QueryLoadAllAccount queryLoad = new QueryLoadAllAccount(uid.ToString());
                server.AccountDBPool.Call(queryLoad, (ret) =>
                {
                    string langKey = lang.ToString();
                    SortedDictionary<int, List<SimpleCharacterInfo>> infos = SimpleCharacterInfo.GetSimpleCharacterInfos(queryLoad.LoginServers);

                    Dictionary<string, object> dataDic = new Dictionary<string, object>();
                    List<Dictionary<string, object>> rolesDic = new List<Dictionary<string, object>>();
                    Dictionary<string, object> roleDic = new Dictionary<string, object>();

                    List<Dictionary<string, object>> dataJson = new List<Dictionary<string, object>>();
                    foreach (var infoList in infos)
                    {
                        foreach (var info in infoList.Value)
                        {
                            if (info.Uid > 0)
                            {
                                dataDic = new Dictionary<string, object>();

                                dataDic.Add("cp_server_id", info.ServerId);
                                FrontendServer mServer = server.ManagerServerManager.GetSinglePointServer(info.ServerId);
                                ServerItemModel serverModel = server.ServersConfig.Get(info.ServerId);
                                if (serverModel != null)
                                {
                                    //dataDic.Add("cp_server_name", serverModel.Name);
                                    dataDic.Add("cp_server_name", serverModel.GetName(langKey));
                                    if (mServer != null)
                                    {
                                        if (serverModel.IsOpen)
                                        {
                                            //开服了
                                            if (serverModel.IsMaintaining)
                                            {
                                                dataDic.Add("cp_server_status", 1);
                                            }
                                            else
                                            {
                                                dataDic.Add("cp_server_status", 0);
                                            }
                                        }
                                        else
                                        {
                                            //没开服不加载
                                            dataDic.Add("cp_server_status", 1);
                                        }
                                    }
                                    else
                                    {
                                        VMallServer.LogList[LogType.WARN].Enqueue($"uid {uid} not find server {info.ServerId}");
                                        //没开服不加载
                                        dataDic.Add("cp_server_status", 1);
                                    }
                                }
                          
                                rolesDic = new List<Dictionary<string, object>>();
                                roleDic = new Dictionary<string, object>();
                                roleDic.Add("cp_role_id", info.Uid);
                                roleDic.Add("cp_role_name", info.Name);
                                if (mServer == null || serverModel == null)
                                {
                                    roleDic.Add("cp_role_pay_status", 1);
                                }
                                else
                                {
                                    roleDic.Add("cp_role_pay_status", 0);
                                }
                                rolesDic.Add(roleDic);

                                dataDic.Add("cp_roles", rolesDic);

                                dataJson.Add(dataDic);

                            }
                        }
                    }

                    //string jsonStr = VMallHelper.JsonSerialize(dataJson);
                    VMResponse response = VMResponse.GetSuccess();
                    response.data = dataJson;
                    session.WriteResponse(response);
                });
            }
            catch (Exception e)
            {
                VMallServer.LogList[LogType.WARN].Enqueue($"http QueryServerList error  {e}");
            }

        }

        private void Recharge(VMallSession session)
        {

            object roleIdObject;
            if (!session.Dic.TryGetValue("role_id", out roleIdObject))
            {
                VMResponse response = VMResponse.GetFail(VMallErrorCode.NoPlayer, "session not find role id");
                session.WriteResponse(response);
                return;
            }

            object serverIdObject;
            if (!session.Dic.TryGetValue("cp_server_id", out serverIdObject))
            {
                VMResponse response = VMResponse.GetFail(VMallErrorCode.NoServer, "session not find seerver id");
                session.WriteResponse(response);
                return;
            }

            object accountObject;
            if (!session.Dic.TryGetValue("uid", out accountObject))
            {
                VMResponse response = VMResponse.GetFail(VMallErrorCode.NoAccount, "session not find uid ");
                session.WriteResponse(response);
                return;
            }

            object gameIdObject;
            if (!session.Dic.TryGetValue("game_id", out gameIdObject))
            {
                VMResponse response = VMResponse.GetFail(VMallErrorCode.NoAccount, "session not find game id ");
                session.WriteResponse(response);
            }


            object payModeObject;
            if (!session.Dic.TryGetValue("pay_mode", out payModeObject))
            {
                //VMResponse response = VMResponse.GetFail(VMallErrorCode.OtherError, "serverId param wrong");
                //session.WriteResponse(response);
                //return;
            }

            object productIdObject;
            if (!session.Dic.TryGetValue("product_id", out productIdObject))
            {
                VMResponse response = VMResponse.GetFail(VMallErrorCode.NoAccount, "session not find product id ");
                session.WriteResponse(response);
                return;
            }

            try
            {
                int serverId = Convert.ToInt32(serverIdObject);
                var manager = server.ManagerServerManager.GetSinglePointServer(serverId);
                if (manager != null)
                {
                    MSG_PM_WEB_RECHAEGE aInfo2M = new MSG_PM_WEB_RECHAEGE();
                    aInfo2M.RoleId = Convert.ToInt32(roleIdObject);
                    aInfo2M.SessionUid = session.SessionUid;

                    aInfo2M.ProductId = Convert.ToString(productIdObject);
                    aInfo2M.ServerId = serverId;
                    aInfo2M.AccountUid = Convert.ToString(accountObject);
                    aInfo2M.GameId = Convert.ToString(gameIdObject);
                    aInfo2M.PayMode = Convert.ToInt32(payModeObject);
                    manager.Write(aInfo2M);
                }
                else
                {
                    VMResponse response = VMResponse.GetFail(VMallErrorCode.NoServer, "can not find server,please check server id");
                    session.WriteResponse(response);
                }
            }
            catch (Exception e)
            {
                VMallServer.LogList[LogType.WARN].Enqueue($"http Recharge error {e}");
            }
        }

        //private void QueryServerList(VMallSession session)
        //{
        //    //DataList xmlDataList = DataListManager.inst.GetDataList("ServerList");
        //    //if (xmlDataList != null)
        //    {
        //        List<object> dataJson = new List<object>();
        //        foreach (var item in server.ServersConfig.List)
        //        {
        //            Dictionary<string, object> dataDic = new Dictionary<string, object>();
        //            //Data xmlData = item.Value;
        //            dataDic.Add("cp_server_id", item.Value.Id);
        //            dataDic.Add("cp_server_name", item.Value.Name);
        //            dataDic.Add("cp_server_status", item.Value.IsOpen);
        //            dataJson.Add(dataDic);
        //        }
        //        string jsonStr = VMallHelper.JsonSerialize(dataJson);

        //        VMResponse response = VMResponse.GetSuccess();
        //        response.data = jsonStr;
        //        session.WriteResponse(response);
        //    }
        //}

        //private void QueryRoleInfo(VMallSession session)
        //{
        //    Dictionary<string, object> dic = VMallHelper.JsonToDictionary(session.Data);

        //    object roleIdObject;
        //    if (!dic.TryGetValue("roleId" ,out roleIdObject))
        //    {
        //        VMResponse response = VMResponse.GetFail(VMallErrorCode.OtherError, "roleId param wrong");
        //        session.WriteResponse(response);
        //        return;
        //    }

        //    object serverIdObject;
        //    if (!dic.TryGetValue("serverId", out serverIdObject))
        //    {
        //        VMResponse response = VMResponse.GetFail(VMallErrorCode.OtherError, "serverId param wrong");
        //        session.WriteResponse(response);
        //        return;
        //    }

        //    try
        //    {
        //        int serverId = Convert.ToInt32(serverIdObject);
        //        var manager = server.ManagerServerManager.GetSinglePointServer(serverId);
        //        if (manager !=null)
        //        {
        //            MSG_BM_GET_ROLE_INFO aInfo2M = new MSG_BM_GET_ROLE_INFO();
        //            aInfo2M.RoleId = Convert.ToInt32(roleIdObject);
        //            aInfo2M.SessionUid = session.SessionUid;
        //            manager.Write(aInfo2M);
        //        }
        //        else
        //        {
        //            VMResponse response = VMResponse.GetFail(VMallErrorCode.UnknownError, "can not find role ");
        //            session.WriteResponse(response);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw;
        //    }
        //}

        //private void Recharge(VMallSession session)
        //{

        //    Dictionary<string, object> dic = VMallHelper.JsonToDictionary(session.Data);

        //    object roleIdObject;
        //    if (!dic.TryGetValue("roleId", out roleIdObject))
        //    {
        //        VMResponse response = VMResponse.GetFail(VMallErrorCode.OtherError, "roleId param wrong");
        //        session.WriteResponse(response);
        //        return;
        //    }

        //    object serverIdObject;
        //    if (!dic.TryGetValue("serverId", out serverIdObject))
        //    {
        //        VMResponse response = VMResponse.GetFail(VMallErrorCode.OtherError, "serverId param wrong");
        //        session.WriteResponse(response);
        //        return;
        //    }

        //    object itemIDObject;
        //    if (!dic.TryGetValue("ItemID", out itemIDObject))
        //    {
        //        VMResponse response = VMResponse.GetFail(VMallErrorCode.OtherError, "itemId param wrong");
        //        session.WriteResponse(response);
        //        return;
        //    }

        //    object numObject;
        //    if (!dic.TryGetValue("num", out numObject))
        //    {
        //        VMResponse response = VMResponse.GetFail(VMallErrorCode.OtherError, "serverId param wrong");
        //        session.WriteResponse(response);
        //        return;
        //    }

        //    object orderNoObject;
        //    if (!dic.TryGetValue("orderNo", out orderNoObject))
        //    {
        //        VMResponse response = VMResponse.GetFail(VMallErrorCode.OtherError, "roleId param wrong");
        //        session.WriteResponse(response);
        //        return;
        //    }

        //    object commentObject;
        //    if (!dic.TryGetValue("comment", out commentObject))
        //    {
        //    }

        //    try
        //    {
        //        int serverId = Convert.ToInt32(serverIdObject);
        //        var manager = server.ManagerServerManager.GetSinglePointServer(serverId);
        //        if (manager != null)
        //        {
        //            MSG_BM_RECHAEGE aInfo2M = new MSG_BM_RECHAEGE();
        //            aInfo2M.RoleId = Convert.ToInt32(roleIdObject);
        //            aInfo2M.SessionUid = session.SessionUid;

        //            aInfo2M.ItemId = Convert.ToInt32(itemIDObject);
        //            aInfo2M.Num = Convert.ToInt32(numObject);
        //            aInfo2M.OrderNo = Convert.ToInt64(orderNoObject);
        //            aInfo2M.Comment = Convert.ToString(commentObject);
        //            manager.Write(aInfo2M);
        //        }
        //        else
        //        {
        //            VMResponse response = VMResponse.GetFail(VMallErrorCode.UnknownError, "can not find server,please check server id");
        //            session.WriteResponse(response);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw;
        //    }

        //}
    }
}