using System.IO;
using Message.IdGenerator;
using ServerShared;
using Logger;
using Message.Manager.Protocol.MR;
using EnumerateUtility;
using Message.Relation.Protocol.RM;
using ServerFrame;
using DBUtility;
using RedisUtility;
using CommonUtility;

namespace RelationServerLib
{
    public class ManagerServer : BackendServer 
    {
        private RelationServerApi Api
        { get { return (RelationServerApi)api; } }

        public ManagerServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_MR_GM_SEND_EMAIL>.Value, OnResponse_GmSendEmail);
            AddResponser(Id<MSG_MR_GM_REISSUE_ARENA_EMAIL>.Value, OnResponse_GmReissueArenaEmail);
            AddResponser(Id<MSG_MR_GM_UPDATE_XML>.Value, OnResponse_UpdateXml);
            AddResponser(Id<MSG_MR_UPDATE_RANKLIST>.Value, OnResponse_UpdateRankList);
            AddResponser(Id<MSG_MR_SHUTDOWN>.Value, OnResponse_Shutdown);
            AddResponser(Id<MSG_MR_RELOAD_FAMILY>.Value, OnResponse_ReloadFamily);
            AddResponser(Id<MSG_MR_GM_FAMILY_INFO>.Value, OnResponse_GMFamilyInfo);
            AddResponser(Id<MSG_MR_CHANGE_FAMILY_NAME>.Value, OnResponse_ChangeFamilyName);
            AddResponser(Id<MSG_MR_SET_FPS>.Value, OnResponse_SetFPS);
            AddResponser(Id<MSG_MR_SEND_VWALL_REWARD_EMAI>.Value, OnResponse_SendVWallEmailReward);
            AddResponser(Id<MSG_MR_UPDATE_RANK_VALUE>.Value, OnResponse_UpdateRankValue);
            //ResponserEnd
        }

        private void OnResponse_GmSendEmail(MemoryStream stream, int uid = 0)
        {
            var pks = MessagePacker.ProtobufHelper.Deserialize<MSG_MR_GM_SEND_EMAIL>(stream);
            int emailId = pks.EmailId;
            int saveTime = pks.SaveTime;
            string sqlConditions = pks.SqlConditions;
            if (!string.IsNullOrEmpty(sqlConditions))
            {
                //    Log.Write("GM semd email {0} save {1} days main id {2}", emailId, saveTime, pks.MainId);
                //    Api.EmailMng.SendSystemEmailAll(emailId, saveTime);
                //}
                //else
                //{
                Api.EmailMng.GmSendEmail(emailId, saveTime, pks.MainId, sqlConditions);
            }
        }

        private void OnResponse_GmReissueArenaEmail(MemoryStream stream, int uid = 0)
        {
            var pks = MessagePacker.ProtobufHelper.Deserialize<MSG_MR_GM_REISSUE_ARENA_EMAIL>(stream);
            string reissueTime = pks.ReissueTime;
            Log.Write("GM reissue arena email date {0}", reissueTime);
            //server.SendArenaEmail(reissueTime, pks.MainId);
        }

       
        private void OnResponse_UpdateXml(MemoryStream stream, int uid = 0)
        {
            var pks = MessagePacker.ProtobufHelper.Deserialize<MSG_MR_GM_UPDATE_XML>(stream);
            Log.Write("GM update xml main id {0}", pks.Type);
            //MSG_RZ_UPDATE_XML msg = new MSG_RZ_UPDATE_XML();
            //server.ZoneManagerBroadCast(msg, pks.MainId);

            if (pks.Type == 1)
            {
                Api.InitData();
                Api.InitOpenServerTime(true);
            }
            else
            {
                Api.UpdateXml();
            }
        }

        private void OnResponse_UpdateRankList(MemoryStream stream, int uid = 0)
        {
            var pks = MessagePacker.ProtobufHelper.Deserialize<MSG_MR_UPDATE_RANKLIST>(stream);
            Log.Write("GM update rank list");
            //MSG_RZ_UPDATE_XML msg = new MSG_RZ_UPDATE_XML();
            //server.ZoneManagerBroadCast(msg, pks.MainId);
            //Api.SeasonMng.InitRankingList();
        }

        private void OnResponse_Shutdown(MemoryStream stream, int uid = 0)
        {
            Log.Warn("manager request to shutdown relation {0}", Api.MainId);
            if (Api.State != ServerState.Stopping && Api.State != ServerState.Stopped)
            {
                Api.State = ServerState.Stopping;
                Api.StoppingTime = RelationServerApi.now.AddMinutes(1);
            }
        }

      



        public void OnResponse_ReloadFamily(MemoryStream stream, int uid = 0)
        {
            MSG_MR_RELOAD_FAMILY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MR_RELOAD_FAMILY>(stream);
            Log.Write("manager request reload family main {0} familyId {1}", msg.MainId, msg.FamillyId);
            ZoneServerManager zoneManager = Api.ZoneManager;
            if (msg.MainId != Api.MainId)
            {
                Log.Warn("manager request reload family main {0} familyId {1} failed: main not exist", msg.MainId, msg.FamillyId);
                return;
            }
            //QueryReloadFamily query = new QueryReloadFamily(msg.famillyId, msg.MainId);
            //server.DB.Call(query, DBProxyDefault.DefaultTableName, DBProxyDefault.DefaultOperateType, ret =>
            //{
            //    if (String.IsNullOrEmpty(query.FamilyName) == false)
            //    {
            //        Family family = zoneManager.FamilyMng.FindFamily(query.Uid);
            //        if (family == null)
            //        {
            //            Log.Warn("manager request reload family main {0} familyId {1} failed: family not exist", msg.MainId, msg.famillyId);
            //            return;
            //        }
            //        if (family.Name != query.FamilyName)
            //        {
            //            zoneManager.FamilyMng.FamilyNameList.Remove(family.Name);
            //            zoneManager.FamilyMng.FamilyNameList.Add(query.FamilyName, family);
            //            family.Name = query.FamilyName;
            //        }
            //    }
            //});
        }

        public void OnResponse_GMFamilyInfo(MemoryStream stream, int uid = 0)
        {
            MSG_MR_GM_FAMILY_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MR_GM_FAMILY_INFO>(stream);
            MSG_RM_GM_FAMLIY_INFO response = new MSG_RM_GM_FAMLIY_INFO();
            response.CustomUid = msg.CustomUid;
            response.FamilyName = msg.FamilyName;
            ZoneServerManager zoneManager = Api.ZoneManager;
            if (msg.MainId != Api.MainId)
            {
                Log.Warn("manager request gm family info main {0} name {1} failed: main not exist", msg.MainId, msg.FamilyName);
                Write(response);
                return;
            }
            Family family = zoneManager.FamilyMng.FindFamilyByName(msg.FamilyName);
            if (family == null)
            {
                Write(response);
                return;
            }
            response.Rank = family.Rank;
            response.Contribution = family.Contribution;
            response.Level = family.Level;
            foreach(var member in family.MemberList)
            {
               //if(member.Value.FamilyTitle <= FamilyTitle.ViceChief)
               //{
               //    MSG_RM_GM_FAMLIY_INFO.FAMILY_MEMBER memberInfo = new MSG_RM_GM_FAMLIY_INFO.FAMILY_MEMBER();
               //    memberInfo.name = member.Value.Name;
               //    memberInfo.title = member.Value.FamilyTitle.ToString();
               //    response.memberList.Add(memberInfo);
               //}
            }
            foreach (var bossState in family.BossStates)
            {
                MSG_RM_GM_FAMLIY_INFO.Types.FAMILY_DUNGEON dungeonInfo = new MSG_RM_GM_FAMLIY_INFO.Types.FAMILY_DUNGEON();
                dungeonInfo.CurHp = bossState.Value[0];
                dungeonInfo.MaxHp = bossState.Value[1];
                dungeonInfo.DungeonId= bossState.Key;
                response.DungeonList.Add(dungeonInfo);
            }
            Write(response);
        }

        public void OnResponse_ChangeFamilyName(MemoryStream stream, int uid = 0)
        {
            MSG_MR_CHANGE_FAMILY_NAME msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MR_CHANGE_FAMILY_NAME>(stream);
            Log.Write("manager request change main " + msg.MainId + " old family name " + msg.OldFamilyName +  " new family name " + msg.NewFamliyName);
            ZoneServerManager zoneManager = Api.ZoneManager;
            if (msg.MainId != Api.MainId)
            {
                return;
            }
            MSG_RM_CHANGE_FAMLIY_NAME response = new MSG_RM_CHANGE_FAMLIY_NAME();
            response.CustomUid = msg.CustomUid;
            response.OldFamilyName = msg.OldFamilyName;
            response.NewFamilyName = msg.NewFamliyName;
            Family family = zoneManager.FamilyMng.FindFamilyByName(msg.OldFamilyName);
            if (family == null)
            {
                //response.Result = (int)ErrorCode.FamilyNotExist;
                Write(response);
                return;
            }
            Family otherFamily = zoneManager.FamilyMng.FindFamilyByName(msg.NewFamliyName);
            if (otherFamily != null)
            {
                //response.Result = (int)ErrorCode.FamilyNameExist;
                Write(response);
                return;
            }
            zoneManager.FamilyMng.FamilyNameList.Remove(family.Name);
            zoneManager.FamilyMng.FamilyNameList.Add(msg.NewFamliyName, family);
            family.Name = msg.NewFamliyName;
            //server.DB.Call(new QueryChangeFamilyName(family.Uid, family.Name));
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }



        private void OnResponse_SetFPS(MemoryStream stream, int uid = 0)
        {
            MSG_MR_SET_FPS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MR_SET_FPS>(stream);
            Api.Fps.SetFPS(msg.FPS);
        }


        public void OnResponse_SendVWallEmailReward(MemoryStream stream, int uid = 0)
        {
            MSG_MR_SEND_VWALL_REWARD_EMAI msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MR_SEND_VWALL_REWARD_EMAI>(stream);
            if (msg.Uid > 0)
            {
                //发送邮件
                Api.EmailMng.SendPersonEmail(msg.Uid, msg.EmailId, msg.Reward);
                Api.GameDBPool.Call(new QueryUpdateVMallOrder(msg.OrderId, Api.Now()));
            }    
        }

        private void OnResponse_UpdateRankValue(MemoryStream stream, int uid = 0)
        {
            MSG_MR_UPDATE_RANK_VALUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MR_UPDATE_RANK_VALUE>(stream);
            if (uid > 0)
            {
                switch ((RankType)msg.RankType)
                {
                    case RankType.Hunting:
                        OperateGetRankScoreByUid query = new OperateGetRankScoreByUid(RankType.Hunting, msg.MainId, uid);
                        Api.GameRedis.Call(query, obj =>
                        {
                            if (query.Score < msg.Value)
                            {
                                Api.GameRedis.Call(new OperateHuntingResearch(uid, msg.Value));
                                Api.GameRedis.Call(new OperateUpdateRankScore(RankType.Hunting, msg.MainId, uid, msg.Value, api.Now()));
                                Api.RankMng.RankReward.CheckAdd(uid, RankType.Hunting, msg.Value);
                            }
                        });
                        break;
                }

            }
        }
    }
}