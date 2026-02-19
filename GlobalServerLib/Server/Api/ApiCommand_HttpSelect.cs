using DataProperty;
using Logger;
using Message.Global.Protocol.GA;
using Message.Global.Protocol.GB;
using Message.Global.Protocol.GBM;
using Message.Global.Protocol.GCross;
using Message.Global.Protocol.GGate;
using Message.Global.Protocol.GM;
using ServerFrame;
using ServerShared;
using System;
using System.Collections.Generic;

namespace GlobalServerLib
{
    partial class GlobalServerApi
    {
        public static int SessionUid = 1;
        public static Dictionary<int, AHttpSession> SelectCallBackDic = new Dictionary<int, AHttpSession>();

        public void SelectExcuteCommand(AHttpSession session)
        {
            //核心处理并且回消息
            string answer = "OK";
            Log.Write("someone httpCmd with  cmd={0} args={1}", session.Cmd, session.Args);
            switch (session.Cmd)
            {
                case "queryRoleInfo":
                    MSG_GM_queryRoleInfo msg = new MSG_GM_queryRoleInfo();
                    object data = session.Dic["data"];
                    string dataStr = data.ToString();

                    var dic = SelectSession.ConvertObjectByJson<Dictionary<string, object>>(data);
                    msg.RoleId = (int)(dic["roleId"]);
                    msg.ServerId = (int)(dic["serverId"]);
                    msg.ChannelID = (int)(dic["channelID"]);
                    msg.SessionUid = SessionUid;
                    
                    session.SessionUid = SessionUid;
                    FrontendServer mServer = ManagerServerManager.GetSinglePointServer(msg.ServerId);
                    if (mServer == null)
                    {
                        Dictionary<string, object> jsonDic = new Dictionary<string, object>();
                        jsonDic.Add("errorCode", 1);
                        jsonDic.Add("errorMessage", "fail");
                        string jsonStr = SelectSession.ObjectToJson(jsonDic);
                        session.AnswerHttpCmd(jsonStr);
                        return;
                    }
                    mServer.Write(msg);

                    CacheHttpSession(session);
                    break;
                case "queryServerList":
                    {
                        DataList xmlDataList = DataListManager.inst.GetDataList("ServerList");
                        if (xmlDataList != null)
                        {
                            Dictionary<string, object> jsonDic = new Dictionary<string, object>();
                            jsonDic.Add("errorCode", 0);
                            jsonDic.Add("errorMessage", "success");
                            List<object> dataJson = new List<object>();
                            foreach (var item in xmlDataList)
                            {
                                Dictionary<string, object> dataDic = new Dictionary<string, object>();
                                Data xmlData = item.Value;
                                dataDic.Add("serverId",xmlData.ID);
                                dataDic.Add("serverName",xmlData.Name);
                                dataJson.Add(dataDic);
                            }
                            jsonDic.Add("data", dataJson);

                            string jsonStr = SelectSession.ObjectToJson(jsonDic);
                            session.AnswerHttpCmd(jsonStr);
                        }
                    }
                    break;
                case "unknownApi":
                default:
                    //Log.Warn("command {0} not support, try command 'help' for more infomation", session.Cmd);
                    answer = String.Format("command {0} not support, try command 'help' for more infomation", session.Cmd);
                    session.AnswerHttpCmd(answer);
                    break;
            }
        }

        public static void CacheHttpSession(AHttpSession session)
        {
            SelectCallBackDic.Add(session.SessionUid, session);
            SessionUid++;
            if (SessionUid > Int32.MaxValue)
            {
                SessionUid = 1;
            }
        }

        public static AHttpSession GetCacheHttpSession(int sessionUid)
        {
            AHttpSession session;
            GlobalServerApi.SelectCallBackDic.TryGetValue(sessionUid, out session);
            return session;
        }

    }
}
