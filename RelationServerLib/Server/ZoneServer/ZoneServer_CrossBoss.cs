using Logger;
using System.IO;
using Message.Relation.Protocol.RZ;
using Message.Relation.Protocol.RR;
using Message.Zone.Protocol.ZR;
using EnumerateUtility;
using ServerFrame;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RC;
using System.Collections.Generic;
using CommonUtility;
using ServerModels;

namespace RelationServerLib
{
    public partial class ZoneServer
    {
        public void OnResponse_GetCrossBossInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZR_GET_CROSS_BOSS_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_CROSS_BOSS_INFO>(stream);
            Log.Write("player {0} GetCrossBossInfo.", uid);
            MSG_RC_GET_CROSS_BOSS_INFO msg = new MSG_RC_GET_CROSS_BOSS_INFO();
            Api.WriteToCross(msg, uid);
        }

        public void OnResponse_StartChallengeCrossBoss(MemoryStream stream, int uid = 0)
        {
            //MSG_ZR_ENTER_CROSS_BOSS_MAP pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_ENTER_CROSS_BOSS_MAP>(stream);
            Log.Write("player {0} StartChallengeCrossBoss.", uid);
            MSG_RC_ENTER_CROSS_BOSS_MAP msg = new MSG_RC_ENTER_CROSS_BOSS_MAP();
            Api.WriteToCross(msg, uid);
        }

        public void OnResponse_ReturnCrossBossPlayerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_BOSS_PLAYER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_BOSS_PLAYER_INFO>(stream);
            Log.Write("player {0} ReturnCrossBossPlayerInfo.", uid);
            Api.WriteToCross(pks, uid);
        }

        public void OnResponse_GetCrossBossChallenger(MemoryStream stream, int uid = 0)
        {
            //MSG_ZR_CROSS_BOSS_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CROSS_BOSS_CHALLENGER>(stream);
            Log.Write("player {0} GetCrossBossChallenger.", uid);
            MSG_RC_CROSS_BOSS_CHALLENGER msg = new MSG_RC_CROSS_BOSS_CHALLENGER();
            Api.WriteToCross(msg, uid);
        }

        public void OnResponse_ChallengeCrossBoss(MemoryStream stream, int uid = 0)
        {
            //MSG_ZR_CHALLENGE_CROSS_BOSS_MAP pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CHALLENGE_CROSS_BOSS_MAP>(stream);
            Log.Write("player {0} ChallengeCrossBoss.", uid);
            MSG_RC_CHALLENGE_CROSS_BOSS_MAP msg = new MSG_RC_CHALLENGE_CROSS_BOSS_MAP();
            Api.WriteToCross(msg, uid);
        }

        public void OnResponse_AddCrossBossScore(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CHANGE_CROSS_BOSS_SCORE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CHANGE_CROSS_BOSS_SCORE>(stream);
            Log.Write("player {0} AddCrossBossScore.", uid);
            MSG_RC_CHANGE_CROSS_BOSS_SCORE msg = new MSG_RC_CHANGE_CROSS_BOSS_SCORE();
            msg.SiteId = pks.SiteId;
            msg.ScoreHp = pks.ScoreHp;
            msg.DefenseUid = pks.DefenseUid;

            //RedisPlayerInfo baseInfo = Api.RPlayerInfoMng.GetPlayerInfo(uid);
            //if (baseInfo != null)
            //{
            //    msg.BaseInfo.Add(GetHWPlayerBaseInfoMsg(HFPlayerInfo.Uid, uid));
            //    msg.BaseInfo.Add(GetHWPlayerBaseInfoMsg(HFPlayerInfo.MainId, baseInfo.GetIntValue(HFPlayerInfo.MainId)));
            //    msg.BaseInfo.Add(GetHWPlayerBaseInfoMsg(HFPlayerInfo.Name, baseInfo.GetStringValue(HFPlayerInfo.Name)));
            //    msg.BaseInfo.Add(GetHWPlayerBaseInfoMsg(HFPlayerInfo.HeroId, baseInfo.GetIntValue(HFPlayerInfo.HeroId)));
            //    msg.BaseInfo.Add(GetHWPlayerBaseInfoMsg(HFPlayerInfo.GodType, baseInfo.GetIntValue(HFPlayerInfo.GodType)));
            //}
            //else
            //{
            //    Log.Warn("player {0} AddCrossBossScore not find base info .", uid);
            //}

            Api.WriteToCross(msg, uid);
        }
    }
}
