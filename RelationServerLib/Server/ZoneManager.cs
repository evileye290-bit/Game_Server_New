using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Logger;
using CommonUtility;
using ServerShared;
using DBUtility;
using ServerFrame;
using EnumerateUtility.Timing;
using EnumerateUtility;
using Message.Relation.Protocol.RZ;

namespace RelationServerLib
{
    public partial class ZoneServerManager : FrontendServerManager
    {
        public ZoneServerManager(BaseApi api, ServerType serverType):base(api, serverType)
        {
            familyManager = new FamilyManager(Api);
            teamManager = new TeamManager(this);
            showMng = new ShowManager();
            BroadCastCount = 0;

            InitTimerManager(RelationServerApi.now);

            InitRechargeTimerManager(RelationServerApi.now, 0);
        }

        private new RelationServerApi Api
        { get { return (RelationServerApi)api; } }
      
        MySqlDataReader reader;

        private TeamManager teamManager;
        public TeamManager TeamManager
        { get { return teamManager; } }

        private FamilyManager familyManager;
        public FamilyManager FamilyMng
        { get { return familyManager; } }

        private ShowManager showMng;
        public ShowManager ShowMng
        { get { return showMng; } }

        public int BroadCastCount { get; set; }

        public override void DestroyServer(FrontendServer server)
        {
            RemoveClients((ZoneServer)server);
            base.DestroyServer(server);
        }

        public override void UpdateServers(double dt)
        {
            base.UpdateServers(dt);

            //定时检测删除缓存玩家信息数据
            ShowMng.DeletePlayerShowInfo(dt);
            //定时检测删除缓存挑战信息数据
            DeleteArenaChallengerInfo(dt);

            ////每日定时器
            //UpdateDalyRefresh(dt);
        }

        public void BroadcastAnnouncement(ANNOUNCEMENT_TYPE type, List<string> list)
        {
            MSG_RZ_BROADCAST_ANNOUNCEMENT msg = new MSG_RZ_BROADCAST_ANNOUNCEMENT();
            msg.Type = (int)type;
            foreach (var item in list)
            {
                msg.List.Add(item);
            }
            FrontendServer server = Api.ZoneManager.GetOneServer();
            if (server != null)
            {
                server.Write(msg);
                Log.Write("server {0} BroadcastAnnouncement type {1} list count {2}", server.SubId, type, list.Count);
            }
        }

        //public void LoadFamilys()
        //{
        //    //Family nonefamily = new Family(server, 0, "", 0, 0, null, "", "");
        //    //FamilyMng.AddFamily(nonefamily);

        //    //HashSet<int> FamilyUidList = new HashSet<int>();
        //    //DBManager db = server.DB.GetDbByTable(DBProxyDefault.DefaultTableName, DBProxyDefault.DefaultOperateType).GetOneDBManager();
        //    //MySqlCommand cmdFamilys = db.Conn.CreateCommand();
        //    //cmdFamilys.CommandTimeout = 0;
        //    //cmdFamilys.CommandText = "SELECT `uid`, `name`,`contribution`, `chiefUid`, `declaration`, `notice`, `week_contribution` FROM `family` WHERE mainId = @MAIN_ID ;";
        //    //cmdFamilys.Parameters.AddWithValue("@MAIN_ID", mainId);
        //    //cmdFamilys.CommandType = System.Data.CommandType.Text;
        //    //try
        //    //{
        //    //    reader = cmdFamilys.ExecuteReader();
        //    //    while (reader.Read())
        //    //    {
        //    //        int uid = reader.GetInt32(0);
        //    //        if (uid > server.MaxFid)
        //    //        {
        //    //            server.MaxFid = uid;
        //    //        }
        //    //        string name = reader.GetString(1);
        //    //        int contribution = reader.GetInt32(2);
        //    //        int chiefUid = reader.GetInt32(3);
        //    //        int weekContribution = reader.GetInt32(6);
        //    //        Client chief = GetClient(chiefUid);
        //    //        if (chief == null)
        //    //        {
        //    //            Log.Warn("family {0} find chief {1} failed, check it", uid, chiefUid);
        //    //            continue;
        //    //        }
        //    //        string declaration = reader.GetString(4);
        //    //        string notice = reader.GetString(5);
        //    //        Family family = new Family(server, uid, name, contribution, weekContribution, chief, declaration, notice);
        //    //        FamilyUidList.Add(uid);
        //    //        FamilyMng.AddFamily(family);
        //    //    }
        //    //    // 按照阵营排序
        //    //    //FamilyMng.Sort();
        //    //}
        //    //catch (Exception e)
        //    //{
        //    //    Log.ErrorLine(e.ToString());
        //    //}
        //    //finally
        //    //{
        //    //    if (reader != null)
        //    //    {
        //    //        reader.Close();
        //    //    }
        //    //}

        //    //LoadFamilyMembers(FamilyUidList);
        //    //LoadFamilyFirstWinLists();
        //    //LoadFamilyDamageLists();
        //    //LoadFamilyWarLists();
        //}

        ///// <summary>
        ///// 家族成员
        ///// </summary>
        ///// <param name="FamilyUidList"></param>
        //private void LoadFamilyMembers(HashSet<int> FamilyUidList)
        //{
        //    // 加载所有家族成员
        //    string fids = "";
        //    int i = 0;
        //    int querySize = 50;
        //    foreach (var uid in FamilyUidList)
        //    {
        //        // 每次搜索500条  
        //        if (i % querySize == 0)
        //        {
        //            fids = uid.ToString();
        //        }
        //        else
        //        {
        //            fids += "," + uid.ToString();
        //        }
        //        i++;
        //        if (i % querySize == 0 || i == FamilyUidList.Count)
        //        {
        //            // 1. 加载角色基础信息
        //            DBManager db = Api.GameDBPool.GetOneDBManager();
        //            MySqlConnection conn = db.GetOneConnection();
        //            MySqlCommand cmdFamilyPlayers = conn.CreateCommand();
        //            cmdFamilyPlayers.CommandTimeout = 0;
        //            cmdFamilyPlayers.CommandType = System.Data.CommandType.Text;
        //            cmdFamilyPlayers.CommandText = @"SELECT `uid`,`fid`,`contributed`,`title` 
        //            FROM `family_member`  WHERE `fid` IN (" + fids + ") ;";
        //            try
        //            {
        //                /* * uid 0 * fid 1 * contributed 2 * title 3 */
        //                conn.Open();
        //                reader = cmdFamilyPlayers.ExecuteReader();
        //                while (reader.Read())
        //                {
        //                    int clientUid = reader.GetInt32(0);
        //                    int familyUid = reader.GetInt32(1);
        //                    Client client = GetClient(clientUid);
        //                    Family family = GetFamily(familyUid);
        //                    if (client == null)
        //                    {
        //                        Log.Warn("load family {0} member {1} failed: member not exist", familyUid, clientUid);
        //                        continue;
        //                    }
        //                    if (family == null)
        //                    {
        //                        Log.Warn("load family {0} member {1} failed: family not exist", familyUid, clientUid);
        //                        continue;
        //                    }
        //                    int contributed = reader.GetInt32(2);
        //                    FamilyTitle title = (FamilyTitle)reader.GetInt32(3);
        //                    //client.InitFamilyInfo(family, contributed, title);
        //                    family.AddMember(client);
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                Log.ErrorLine(e.ToString());
        //            }
        //            finally
        //            {
        //                if (reader != null)
        //                {
        //                    reader.Close();
        //                }
        //                conn.Close();
        //            }
        //        }
        //    }
        //}

        //public Family GetFamily(int uid)
        //{
        //    Family family = null;
        //    FamilyMng.FamilyList.TryGetValue(uid, out family);
        //    return family;
        //}

        //public Family GetFamilyByName(string name)
        //{
        //    Family family = null;
        //    FamilyMng.FamilyNameList.TryGetValue(name, out family);
        //    return family;
        //}

    }
}