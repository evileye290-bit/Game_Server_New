using Message.Gate.Protocol.GateC;
using Logger;
using Message.Relation.Protocol.RZ;
using EnumerateUtility;
using System.IO;
using RedisUtility;

namespace ZoneServerLib
{
    public partial class RelationServer
    {
        private void OnResponse_BrotherInvite(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_BROTHERS_INVITE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_BROTHERS_INVITE>(stream);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.AddBrotherInviteList(msg.InviterUid);
            }
            else
            {
                Log.Error("player {0} mainId {1} OnResponse_BrotherInvite player {2} fail,can not find player", msg.InviterUid, MainId,uid);
            }
        }

        private void OnResponse_BrotherResponse(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_BROTHERS_RESPONSE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_BROTHERS_RESPONSE>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.NotiyClientAdd2BrotherList(uid, msg.BrotherUid, msg.Agree);
                if (msg.Agree)
                {
                    player.Add2BrotherList(msg.BrotherUid);

                    player.AddTaskNumForType(TaskType.BrotherNum, player.BrotherCount, false);

                    //完成义结金兰发称号卡
                    player.TitleMng.UpdateTitleConditionCount(TitleObtainCondition.Sworn);
                }
            }
            else
            {
                Log.Error("player {0} mainId {1} OnResponse_BrotherResponse player {2} fail,can not find player", msg.BrotherUid, MainId,uid);
            }
        }


        private void OnResponse_BrotherRemove(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_BROTHERS_REMOVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_BROTHERS_REMOVE>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.RemoveFromBrotherList(msg.BrotherUid);
            }
            else
            {
                Log.Error("player {0} mainId {1} remove brother {2} fail,can not find player {3}", uid, MainId,msg.BrotherUid, uid);
            }

        }

    }



}