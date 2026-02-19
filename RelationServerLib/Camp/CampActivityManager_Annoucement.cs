using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerModels;
using ServerModels.Monster;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public partial class CampActivityManager
    {

        public CAMPBATTLE_ANNOUNCE MakeAnnounceMentMsg(CampBattleAnnouncementType type, params object[] args)
        {
            CAMPBATTLE_ANNOUNCE msg = new CAMPBATTLE_ANNOUNCE();
            msg.Type = (int)type;
            foreach (var item in args)
            {
                msg.ParamList.Add(item.ToString());
            }
            return msg;
        }
        private void RecordCampBattleHoldAnnouncement(CampFort fort, PLAY_BASE_INFO attackerInfo)
        {
            CAMPBATTLE_ANNOUNCE msg;
            if (fort.CampType == CampType.None)
            {
                //打怪物
                msg = MakeAnnounceMentMsg(CampBattleAnnouncementType.Monster, Timestamp.GetUnixTimeStampSeconds(server.Now()), attackerInfo.Camp, attackerInfo.Name, fort.Id);
            }
            else
            {
                int uid = fort.GetDefenderUid();
                if (uid > 0)
                {
                    //玩家互攻
                    msg = MakeAnnounceMentMsg(CampBattleAnnouncementType.Defender, Timestamp.GetUnixTimeStampSeconds(server.Now()), attackerInfo.Camp, attackerInfo.Name, (int)fort.CampType, fort.GetDefenderName(), fort.Id);

                    EmailInfo email = EmailLibrary.GetEmailInfo(CampBattleLibrary.GetFortHoldEmailId());
                    if (email != null)
                    {
                        string param = $"{CommonConst.FORT_ID}:{fort.Id}|{CommonConst.CAMP_ID}:{attackerInfo.Camp}|{CommonConst.NAME}:{attackerInfo.Name}";
                        server.EmailMng.SendPersonEmail(uid, email, email.Body,"",0, param);
                    }
                    else
                    {
                        Log.Warn("gm send email not find email id:{0}", CampBattleLibrary.GetFortHoldEmailId());
                    }
                }
                else
                {
                    //空据点
                    msg = MakeAnnounceMentMsg(CampBattleAnnouncementType.None, Timestamp.GetUnixTimeStampSeconds(server.Now()), attackerInfo.Camp, attackerInfo.Name, fort.Id);
                }
            }
            /// MessagePacker.ProtobufHelper.DeserializeFromString<MSG_ZGC_CAMPBATTLE_ANNOUNCE>(forStr)
            string strMsg = MessagePacker.ProtobufHelper.Serialize2String(msg);
            OperateCampBattleAnnoucementAdd oper = new OperateCampBattleAnnoucementAdd(server.MainId, strMsg);
            server.GameRedis.Call(oper);
        }

        private void RecordCampBattleGiveUpAnnouncement(CampFort campFort)
        {
            //放弃据点
            CAMPBATTLE_ANNOUNCE msg = MakeAnnounceMentMsg(CampBattleAnnouncementType.GiveUp, Timestamp.GetUnixTimeStampSeconds(server.Now()), campFort.CampType, campFort.GetDefenderName(), campFort.Id);
            string strMsg = MessagePacker.ProtobufHelper.Serialize2String(msg);
            OperateCampBattleAnnoucementAdd oper = new OperateCampBattleAnnoucementAdd(server.MainId, strMsg);
            server.GameRedis.Call(oper);
        }

        public void RecordCampBattleInspireAnnouncement()
        {
            CAMPBATTLE_ANNOUNCE msg = MakeAnnounceMentMsg(CampBattleAnnouncementType.Inspire, Timestamp.GetUnixTimeStampSeconds(server.Now()),(int)InspireCamp,InspireDValue);
            string strMsg = MessagePacker.ProtobufHelper.Serialize2String(msg);
            OperateCampBattleAnnoucementAdd oper = new OperateCampBattleAnnoucementAdd(server.MainId, strMsg);
            server.GameRedis.Call(oper);
        }

    }



}
