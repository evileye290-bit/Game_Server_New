using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_ShowPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHOW_PLAYER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOW_PLAYER>(stream);
            Log.Write("player {0} show player info: playerUid {1} SyncPlayer {2}", uid, msg.PlayerUid, msg.SyncPlayer);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.ShowPlayerInfo(msg.PlayerUid, msg.SyncPlayer, msg.MainId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("show baseinfo fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("show baseinfo fail,can not find player {0}.", uid);
                }
            }
        }


        //public void OnResponse_ShowFaceIcon(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_SHOW_FACEICON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOW_FACEICON>(stream);

        //    PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
        //    if (player != null)
        //    {
        //        player.ShowFaceIcon(msg.FaceIcon);
        //    }
        //    else
        //    {
        //        player = Api.PCManager.FindOfflinePc(msg.PcUid);
        //        if (player != null)
        //        {
        //            Log.WarnLine("show faceicon fail, player {0} is offline.", msg.PcUid);
        //        }
        //        else
        //        {
        //            Log.WarnLine("show faceicon fail, can not find player {0} .", msg.PcUid);
        //        }
        //    }
        //}

        //public void OnResponse_ShowFaceJpg(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_SHOW_FACEJPG msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOW_FACEJPG>(stream);

        //    PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
        //    if (player != null)
        //    {
        //        player.SetShowFaceJpg(msg.ShowFaceJpg);
        //    }
        //    else
        //    {
        //        player = Api.PCManager.FindOfflinePc(msg.PcUid);
        //        if (player != null)
        //        {
        //            Log.WarnLine("show facejpg fail, player {0} is offline.", msg.PcUid);
        //        }
        //        else
        //        {
        //            Log.WarnLine("show facejpg fail, can not find player {0} .", msg.PcUid);
        //        }
        //    }
        //}

        //public void OnResponse_SetSex(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_SET_SEX msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SET_SEX>(stream);

        //    PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
        //    if (player != null)
        //    {
        //        player.SetSex(msg.Sex);

        //    }
        //    else
        //    {
        //        player = Api.PCManager.FindOfflinePc(msg.PcUid);
        //        if (player != null)
        //        {
        //            Log.WarnLine("set sex fail, player {0} is offline.", msg.PcUid);
        //        }
        //        else
        //        {
        //            Log.WarnLine("set sex fail, can not find player {0} .", msg.PcUid);
        //        }
        //    }
        //}

        //public void OnResponse_SetBirthday(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_SET_BIRTHDAY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SET_BIRTHDAY>(stream);

        //    PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
        //    if (player != null)
        //    {
        //        player.SetBirthday(msg.Birthday);

        //    }
        //    else
        //    {
        //        player = Api.PCManager.FindOfflinePc(msg.PcUid);
        //        if (player != null)
        //        {
        //            Log.WarnLine("set birthday fail, player {0} is offline.", msg.PcUid);
        //        }
        //        else
        //        {
        //            Log.WarnLine("set birthday fail, can not find player {0} .", msg.PcUid);
        //        }
        //    }
        //}

        //public void OnResponse_SetSignature(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_SET_SIGNATURE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SET_SIGNATURE>(stream);

        //    PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
        //    if (player != null)
        //    {
        //        MSG_ZGC_SET_SIGNATURE response = new MSG_ZGC_SET_SIGNATURE();
        //        player.SetSignature(msg.Signature);
        //        response.Result = (int)ErrorCode.Success;
        //        player.Write(response);
        //    }
        //    else
        //    {
        //        player = Api.PCManager.FindOfflinePc(msg.PcUid);
        //        if (player != null)
        //        {
        //            Log.WarnLine("set signature fail, player {0} is offline.", msg.PcUid);
        //        }
        //        else
        //        {
        //            Log.WarnLine("set signature fail, can not find player {0} .", msg.PcUid);
        //        }
        //    }
        //}


        //public void OnResponse_SetWQ(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_SET_SOCIAL_NUM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SET_SOCIAL_NUM>(stream);

        //    PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
        //    if (player != null)
        //    {
        //        //wNum&qq
        //        MSG_ZGC_SET_SOCIAL_NUM response = new MSG_ZGC_SET_SOCIAL_NUM();
        //        player.SetSocialNum(msg.WNum, msg.QNum, msg.InfoShowType);
        //        response.Result = (int)ErrorCode.Success;
        //        player.Write(response);
        //    }
        //    else
        //    {
        //        player = Api.PCManager.FindOfflinePc(msg.PcUid);
        //        if (player != null)
        //        {
        //            Log.WarnLine("set wNum&qNum fail, player {0} is offline.", msg.PcUid);
        //        }
        //        else
        //        {
        //            Log.WarnLine("set wNum&qNum fail, can not find player {0} .", msg.PcUid);
        //        }
        //    }
        //}

        //public void OnResponse_GetWQ(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_GET_SOCIAL_NUM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_SOCIAL_NUM>(stream);

        //    PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
        //    if (player != null)
        //    {
        //        //wNum&qq
        //        player.GetWNumAndQNum(msg.CharacterId);
        //    }
        //    else
        //    {
        //        player = Api.PCManager.FindOfflinePc(msg.PcUid);
        //        if (player != null)
        //        {
        //            Log.WarnLine("set wNum&qq fail, player {0} is offline.", msg.PcUid);
        //        }
        //        else
        //        {
        //            Log.WarnLine("set wNum&qq fail, can not find player {0} .", msg.PcUid);
        //        }
        //    }
        //}


        //public void OnResponse_ShowVoice(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_SHOW_VOICE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOW_VOICE>(stream);

        //    PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
        //    if (player != null)
        //    {
        //        player.SetShowVoice(msg.ShowVoice);
        //    }
        //    else
        //    {
        //        player = Api.PCManager.FindOfflinePc(msg.PcUid);
        //        if (player != null)
        //        {
        //            Log.WarnLine("set birthday fail, player {0} is offline.", msg.PcUid);
        //        }
        //        else
        //        {
        //            Log.WarnLine("set birthday fail, can not find player {0} .", msg.PcUid);
        //        }
        //    }
        //}

        //public void OnResponse_PresentGift(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_PRESENT_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_PRESENT_GIFT>(stream);

        //    PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
        //    if (player != null)
        //    {
        //        player.PresentGift(msg.CharacterId,msg.Id,msg.Num);
        //    }
        //    else
        //    {
        //        player = Api.PCManager.FindOfflinePc(msg.PcUid);
        //        if (player != null)
        //        {
        //            Log.WarnLine("present gift fail, player {0} is offline.", msg.PcUid);
        //        }
        //        else
        //        {
        //            Log.WarnLine("present gift fail, can not find player {0} .", msg.PcUid);
        //        }
        //    }
        //}

        //public void OnResponse_GetGiftRecord(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_GET_GIFTRECORD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_GIFTRECORD>(stream);

        //    PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
        //    if (player != null)
        //    {
        //        player.GetGiftRecord(msg.Page);
        //    }
        //    else
        //    {
        //        player = Api.PCManager.FindOfflinePc(msg.PcUid);
        //        if (player != null)
        //        {
        //            Log.WarnLine("get gift record fail, player {0} is offline.", msg.PcUid);
        //        }
        //        else
        //        {
        //            Log.WarnLine("get gift record fail, can not find player {0} .", msg.PcUid);
        //        }
        //    }
        //}

        public void OnResponse_ChangeName(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CHANGE_NAME msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CHANGE_NAME>(stream);
            Log.Write("player {0} change name {1}", msg.PcUid, msg.Name);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                if (player.Name.Equals(CharacterInitLibrary.InitName))
                {
                    //首次改名
                    CheckChangeName(player, msg.Name);
                }
                else
                {
                    if (player.server.http163Helper.CheckOpen == 1)
                    {
                        Check163Query_Text check = new Check163Query_Text();
                        var paramsArr = player.Get163TextCheckParameters(Context163Type.Chat, msg.Name);
                        player.server.http163Helper.PostTextAsync(paramsArr, check, () =>
                        {
                            if (check.action == 0) //通过
                            {
                                CheckChangeName(player, msg.Name);
                            }
                            else
                            {
                                //TODO:BOIL 1：嫌疑，2：不通过
                                MSG_ZGC_CHANGE_NAME notify = new MSG_ZGC_CHANGE_NAME();
                                notify.Result = (int)ErrorCode.BadWord;
                                player.Write(notify);
                            }
                            return check.Result;
                        });
                    }
                    else
                    {
                        CheckChangeName(player, msg.Name);
                    }
                }
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("show changename fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("show changename fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

        private void CheckChangeName(PlayerChar player, string name)
        {

            //检查屏蔽字
            if (Api.NameChecker.HasSpecialSymbol(name) || Api.NameChecker.HasBadWord(name))
            {
                MSG_ZGC_CHANGE_NAME notify = new MSG_ZGC_CHANGE_NAME();
                notify.Result = (int)ErrorCode.BadWord;
                player.Write(notify);
                return;
            }

            QueryGetCharacterIdByName queryId = new QueryGetCharacterIdByName(name);

            Api.GameDBPool.Call(queryId, ret =>
            {
                if (queryId.PlayUid > 0)
                {
                    //名字已经存在
                    MSG_ZGC_CHANGE_NAME response = new MSG_ZGC_CHANGE_NAME();
                    response.Result = (int)ErrorCode.NameExisted;
                    player.Write(response);
                    Log.Warn($"player {player.Uid} change name {name} failed: name exists");
                    return;
                }
                else
                {
                    //名字是合法的 ,并且不重名
                    player.ChangeName(name);
                    return;
                }
            });
        }

        //public void OnResponse_ShowCareer(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_SHOW_CAREER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOW_CAREER>(stream);
        //    //int showUid = pks.ShowUid;
        //    //int chapterId = pks.ChapterId;
        //    //PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
        //    //if (player != null)
        //    //{
        //    //    Dictionary<int, List<string>> statData = new Dictionary<int, List<string>>();
        //    //    if (showUid == pks.PcUid)
        //    //    {
        //    //        //statData = player.GetStatData(chapterId);
        //    //        MSG_ZGC_SHOW_CAREER msg = GetStatDataMessage(showUid, statData);
        //    //        msg.ChapterId = chapterId;
        //    //        player.Write(msg);
        //    //    }
        //    //    else
        //    //    {
        //    //        PlayerChar showPlayer = Api.PCManager.FindPc(showUid);
        //    //        if (showPlayer != null)
        //    //        {
        //    //            statData = showPlayer.GetStatData(chapterId);
        //    //            MSG_ZGC_SHOW_CAREER msg = GetStatDataMessage(showUid, statData);
        //    //            msg.ChapterId = chapterId;
        //    //            player.Write(msg);
        //    //        }
        //    //        else
        //    //        {
        //    //            showPlayer = Api.PCManager.FindOfflinePc(showUid);
        //    //            if (showPlayer != null)
        //    //            {
        //    //                statData = showPlayer.GetStatData(chapterId);
        //    //                MSG_ZGC_SHOW_CAREER msg = GetStatDataMessage(showUid, statData);
        //    //                msg.ChapterId = chapterId;
        //    //                player.Write(msg);
        //    //            }
        //    //            else
        //    //            {
        //    //                //现提取数据
        //    //                OperateGetStatData operateStatData = new OperateGetStatData(showUid);
        //    //                Api.Redis.Call(operateStatData, ret =>
        //    //                {
        //    //                    if ((int)ret == 1)
        //    //                    {
        //    //                        Dictionary<int, List<string>> showStatData = GetStatData(showUid, chapterId, operateStatData.DataList);
        //    //                        MSG_ZGC_SHOW_CAREER msg = GetStatDataMessage(showUid, showStatData);
        //    //                        msg.ChapterId = chapterId;
        //    //                        player.Write(msg);
        //    //                        return;
        //    //                    }
        //    //                    else
        //    //                    {
        //    //                        Log.Warn("player {0} ShowCareer OperateGetStatData {0} not find data", showUid);
        //    //                    }
        //    //                });
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    player = Api.PCManager.FindOfflinePc(pks.PcUid);
        //    //    if (player != null)
        //    //    {
        //    //        Log.WarnLine("show curCardQueue fail, player {0} is offline.", pks.PcUid);
        //    //    }
        //    //    else
        //    //    {
        //    //        Log.WarnLine("show curCardQueue fail, can not find player {0} .", pks.PcUid);
        //    //    }
        //    //}
        //}

        //private static MSG_ZGC_SHOW_CAREER GetStatDataMessage(int showUid, Dictionary<int, List<string>> statData)
        //{
        //    MSG_ZGC_SHOW_CAREER msg = new MSG_ZGC_SHOW_CAREER();
        //    msg.PcUid = showUid;
        //    foreach (var data in statData)
        //    {
        //        MSG_ZGC_CAREER_ITEM item = new MSG_ZGC_CAREER_ITEM();
        //        item.ContentId = data.Key;
        //        item.Params.AddRange(data.Value);
        //        msg.ContentList.Add(item);
        //    }

        //    return msg;
        //}

        //public Dictionary<int, List<string>> GetStatData(int showUid, int chapterId, Dictionary<HashField_StatData, object> dataList)
        //{
        //    Dictionary<int, List<string>> contentList = new Dictionary<int, List<string>>();
        //    CareerInfo career = ShowLibrary.GetCareerInfo(chapterId);
        //    if (career != null)
        //    {
        //        foreach (var content in career.ContentList)
        //        {
        //            List<string> list = new List<string>();
        //            foreach (var type in content.Value.StatDatas)
        //            {
        //                string stat = string.Empty;
        //                object data;
        //                if (dataList.TryGetValue(type, out data))
        //                {
        //                    stat = data.ToString();
        //                }
        //                list.Add(stat);
        //            }
        //            contentList.Add(content.Key, list);
        //        }
        //    }
        //    else
        //    {
        //        Log.Warn("player {0} ShowCareer get stat data from show libarary not find id {1}", showUid, chapterId);
        //    }
        //    return contentList;
        //}


        //public void OnResponse_GetRankingFriendList(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_GET_RANKING_FRIEND_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_RANKING_FRIEND_LIST>(stream);
        //    int pcUid = msg.PcUid;

        //    PlayerChar player = Api.PCManager.FindPc(pcUid);
        //    if (player != null)
        //    {
        //        player.GetRankingFriendList();
        //    }
        //    else
        //    {
        //        player = Api.PCManager.FindOfflinePc(pcUid);
        //        if (player != null)
        //        {
        //            Log.WarnLine("player {0} is offline .can not get ranking friend list", pcUid);
        //        }
        //        else
        //        {
        //            Log.WarnLine("player {0} get friend list can not ranking friend find player.", pcUid);
        //        }
        //    }
        //}

        //public void OnResponse_GetRankingAllList(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_GET_RANKING_ALL_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_RANKING_ALL_LIST>(stream);
        //    int pcUid = msg.PcUid;

        //    PlayerChar player = Api.PCManager.FindPc(pcUid);
        //    if (player != null)
        //    {
        //        player.GetRankingAllList(msg.Index);
        //    }
        //    else
        //    {
        //        player = Api.PCManager.FindOfflinePc(pcUid);
        //        if (player != null)
        //        {
        //            Log.WarnLine("player {0} is offline .can not get ranking all list", pcUid);
        //        }
        //        else
        //        {
        //            Log.WarnLine("player {0} get friend list can not ranking all find player.", pcUid);
        //        }
        //    }
        //}

        //public void OnResponse_GetRankingFriendList(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_GET_RANKING_FRIEND_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_RANKING_FRIEND_LIST>(stream);
        //    int pcUid = msg.PcUid;

        //    PlayerChar player = server.PCManager.FindPc(pcUid);
        //    if (player != null)
        //    {
        //        player.GetRankingNearbyList();
        //    }
        //    else
        //    {
        //        player = server.PCManager.FindOfflinePc(pcUid);
        //        if (player != null)
        //        {
        //            Log.WarnLine("player {0} is offline .can not get ranking friend list", pcUid);
        //        }
        //        else
        //        {
        //            Log.WarnLine("player {0} get friend list can not ranking friend find player.", pcUid);
        //        }
        //    }
        //}
    }
}
