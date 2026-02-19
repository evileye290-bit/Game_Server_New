using EnumerateUtility;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using MessagePacker;
using ServerModels;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_ShowPlayer(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOW_PLAYER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHOW_PLAYER>(stream);
            MSG_GateZ_SHOW_PLAYER requestMsg = new MSG_GateZ_SHOW_PLAYER();
            requestMsg.PlayerUid = msg.PlayerUid;
            requestMsg.SyncPlayer = msg.SyncPlayer;
            requestMsg.MainId = msg.MainId;
            WriteToZone(requestMsg);
        }


        public void OnResponse_ShowFaceIcon(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOW_FACEICON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHOW_FACEICON>(stream);
            MSG_GateZ_SHOW_FACEICON requestMsg = new MSG_GateZ_SHOW_FACEICON();
            requestMsg.PcUid = Uid;
            requestMsg.FaceIcon = msg.FaceIcon;
            WriteToZone(requestMsg);
        }

        public void OnResponse_ShowFaceJpg(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOW_FACEJPG msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHOW_FACEJPG>(stream);
            MSG_GateZ_SHOW_FACEJPG requestMsg = new MSG_GateZ_SHOW_FACEJPG();
            requestMsg.PcUid = Uid;
            requestMsg.ShowFaceJpg = msg.ShowFaceJpg;
            WriteToZone(requestMsg);
        }

        public void OnResponse_SetSex(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SET_SEX msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SET_SEX>(stream);
            MSG_GateZ_SET_SEX requestMsg = new MSG_GateZ_SET_SEX();
            requestMsg.PcUid = Uid;
            requestMsg.Sex = msg.Sex;
            WriteToZone(requestMsg);
        }

        public void OnResponse_SetBirthday(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SET_BIRTHDAY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SET_BIRTHDAY>(stream);
            MSG_GateZ_SET_BIRTHDAY requestMsg = new MSG_GateZ_SET_BIRTHDAY();
            requestMsg.PcUid = Uid;
            requestMsg.Birthday = msg.Birthday;
            WriteToZone(requestMsg);
        }

        public void OnResponse_SetSignature(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SET_SIGNATURE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SET_SIGNATURE>(stream);

            if (IsGm == 0)
            {
                //检查字长
                int nameLen = server.WordChecker.GetWordLen(msg.Signature);
                if (nameLen > WordLengthLimit.SignatureSize)
                {
                    MSG_ZGC_SET_SIGNATURE notify = new MSG_ZGC_SET_SIGNATURE();
                    notify.Result = (int)ErrorCode.NameLength;
                    Write(notify);
                    return;
                }
                //检查屏蔽字
                if (server.WordChecker.HasBadWord(msg.Signature))
                {
                    MSG_ZGC_SET_SIGNATURE notify = new MSG_ZGC_SET_SIGNATURE();
                    notify.Result = (int)ErrorCode.BadWord;
                    Write(notify);
                    return;
                }
            }
           

            MSG_GateZ_SET_SIGNATURE requestMsg = new MSG_GateZ_SET_SIGNATURE();
            requestMsg.PcUid = Uid;
            requestMsg.Signature = msg.Signature;
            WriteToZone(requestMsg);
        }

        public void OnResponse_SetWQ(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SET_SOCIAL_NUM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SET_SOCIAL_NUM>(stream);

            if (IsGm == 0)
            {
                //检查字长
                int nameLen = server.NameChecker.GetWordLen(msg.QNum);
                if (nameLen > WordLengthLimit.QNum_SIZE)
                {
                    MSG_ZGC_SET_SOCIAL_NUM notify = new MSG_ZGC_SET_SOCIAL_NUM();
                    notify.Result = (int)ErrorCode.NameLength;
                    Write(notify);
                    return;
                }
                //检查屏蔽字
                if (server.NameChecker.HasSpecialSymbol(msg.QNum))//|| server.nameChecker.HasBadWord(msg.QNum))
                {
                    MSG_ZGC_SET_SOCIAL_NUM notify = new MSG_ZGC_SET_SOCIAL_NUM();
                    notify.Result = (int)ErrorCode.BadWord;
                    Write(notify);
                    return;
                }

                //检查字长
                nameLen = server.NameChecker.GetWordLen(msg.WNum);
                if (nameLen > WordLengthLimit.WNum_SIZE)
                {
                    MSG_ZGC_SET_SOCIAL_NUM notify = new MSG_ZGC_SET_SOCIAL_NUM();
                    notify.Result = (int)ErrorCode.NameLength;
                    Write(notify);
                    return;
                }
                //检查屏蔽字
                if (server.NameChecker.HasSpecialSymbol(msg.WNum))//|| server.nameChecker.HasBadWord(msg.WNum))
                {
                    MSG_ZGC_SET_SOCIAL_NUM notify = new MSG_ZGC_SET_SOCIAL_NUM();
                    notify.Result = (int)ErrorCode.BadWord;
                    Write(notify);
                    return;
                }
            }


            MSG_GateZ_SET_SOCIAL_NUM requestMsg = new MSG_GateZ_SET_SOCIAL_NUM();
            requestMsg.PcUid = Uid;
            requestMsg.QNum = msg.QNum;
            requestMsg.WNum = msg.WNum;
            requestMsg.InfoShowType = msg.InfoShowType;
            WriteToZone(requestMsg);
        }

        public void OnResponse_GetWQ(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_SOCIAL_NUM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_SOCIAL_NUM>(stream);
            MSG_GateZ_GET_SOCIAL_NUM requestMsg = new MSG_GateZ_GET_SOCIAL_NUM();
            requestMsg.PcUid = Uid;
            requestMsg.CharacterId = msg.CharacterId;
            WriteToZone(requestMsg);
        }


        public void OnResponse_ShowVoice(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOW_VOICE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHOW_VOICE>(stream);
            MSG_GateZ_SHOW_VOICE requestMsg = new MSG_GateZ_SHOW_VOICE();
            requestMsg.PcUid = Uid;
            requestMsg.ShowVoice = msg.ShowVoice;
            WriteToZone(requestMsg);
        }

        public void OnResponse_PresentGift(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_PRESENT_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_PRESENT_GIFT>(stream);
            MSG_GateZ_PRESENT_GIFT requestMsg = new MSG_GateZ_PRESENT_GIFT();
            requestMsg.PcUid = Uid;
            requestMsg.CharacterId = msg.CharacterId;
            requestMsg.Id = msg.Id;
            requestMsg.Num = msg.Num;
            WriteToZone(requestMsg);
        }

        public void OnResponse_ChangeName(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CHANGE_NAME msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CHANGE_NAME>(stream);

            if (IsGm == 0)
            {
                //检查字长
                int nameLen = server.NameChecker.GetWordLen(msg.Name);
                if (nameLen > WordLengthLimit.CharNameLenLimit)
                {
                    MSG_ZGC_CHANGE_NAME notify = new MSG_ZGC_CHANGE_NAME();
                    notify.Result = (int)ErrorCode.NameLength;
                    Write(notify);
                    return;
                }
                ////检查屏蔽字
                //if (server.NameChecker.HasSpecialSymbol(msg.Name) || server.NameChecker.HasBadWord(msg.Name))
                //{
                //    MSG_ZGC_CHANGE_NAME notify = new MSG_ZGC_CHANGE_NAME();
                //    notify.Result = (int)ErrorCode.BadWord;
                //    Write(notify);
                //    return;
                //}
            }
 

            MSG_GateZ_CHANGE_NAME requestMsg = new MSG_GateZ_CHANGE_NAME();
            requestMsg.PcUid = Uid;
            requestMsg.Name = msg.Name;
            WriteToZone(requestMsg);
        }

        public void OnResponse_ShowCareer(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOW_CAREER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHOW_CAREER>(stream);
            MSG_GateZ_SHOW_CAREER requestMsg = new MSG_GateZ_SHOW_CAREER();
            requestMsg.PcUid = Uid;
            requestMsg.ChapterId = msg.ChapterId;
            requestMsg.ShowUid = msg.PcUid;
            WriteToZone(requestMsg);
        }

        public void OnResponse_GetGiftRecord(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_GIFTRECORD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_GIFTRECORD>(stream);
            MSG_GateZ_GET_GIFTRECORD requestMsg = new MSG_GateZ_GET_GIFTRECORD();
            requestMsg.PcUid = Uid;
            requestMsg.Page = msg.Page;
            WriteToZone(requestMsg);
        }

        public void OnResponse_GetRankingFriendList(MemoryStream stream)
        {
            if (curZone == null) return;
            //MSG_CG_GET_RANKING_FRIEND_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_RANKING_FRIEND_LIST>(stream);
            MSG_GateZ_GET_RANKING_FRIEND_LIST requestMsg = new MSG_GateZ_GET_RANKING_FRIEND_LIST();
            requestMsg.PcUid = Uid;
            WriteToZone(requestMsg);
        }

        public void OnResponse_GetRankingAllList(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_RANKING_ALL_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_RANKING_ALL_LIST>(stream);
            MSG_GateZ_GET_RANKING_ALL_LIST requestMsg = new MSG_GateZ_GET_RANKING_ALL_LIST();
            requestMsg.PcUid = Uid;
            requestMsg.Index = msg.Index;
            WriteToZone(requestMsg);
        }

        public void OnResponse_SaveGuideId(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SAVE_GUIDE_ID msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SAVE_GUIDE_ID>(stream);
            MSG_GateZ_SAVE_GUIDE_ID requestMsg = new MSG_GateZ_SAVE_GUIDE_ID();
            requestMsg.PcUid = Uid;
            requestMsg.GuideId = msg.GuideId;
            requestMsg.Time = msg.Time;
            WriteToZone(requestMsg);
        }

        public void OnResponse_SaveMainLineId(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SAVE_MAIN_LINE_ID msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SAVE_MAIN_LINE_ID>(stream);
            MSG_GateZ_SAVE_MAIN_LINE_ID requestMsg = new MSG_GateZ_SAVE_MAIN_LINE_ID();
            requestMsg.PcUid = Uid;
            requestMsg.MainLineId = msg.MainLineId;
            WriteToZone(requestMsg);
        }

        //public void OnResponse_GetRankingFriendList(MemoryStream stream)
        //{
        //    if (curZone == null) return;
        //    MSG_CG_GET_RANKING_NEARBY_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_RANKING_NEARBY_LIST>(stream);
        //    MSG_GateZ_GET_RANKING_FRIEND_LIST requestMsg = new MSG_GateZ_GET_RANKING_FRIEND_LIST();
        //    requestMsg.PcUid = Uid;
        //    requestMsg.Page = msg.Page;
        //    WriteToZone(requestMsg);
        //}
    }
}
