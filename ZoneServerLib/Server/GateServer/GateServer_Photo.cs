using CommonUtility;
using DataProperty;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Zone.Protocol.ZR;
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
        //private void OnResponse_UploadPhoto(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_UPLOAD_PHOTO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_UPLOAD_PHOTO>(stream);
        //    Log.Write("player {0} upload photo {1}", msg.Uid, msg.PhotoName);
        //    PlayerChar player = Api.PCManager.FindPc(msg.Uid);
        //    if(player == null)
        //    {
        //        return;
        //    }
        //    // 1. 获取角色photo list 长度 如果超过上限， 则随机删除一个
        //    long maxCount = CONST.MAX_PHOTO_COUNT;
        //    Data data = DataListManager.inst.GetData("PhotoConfig", 1);
        //    if (data != null)
        //    {
        //        maxCount = (long)data.GetInt("maxPhotoCount");
        //    }
        //    OperateGetPhotoCount operate = new OperateGetPhotoCount(msg.Uid);
        //    Api.Redis.Call(operate, ret =>
        //    {
        //        if ((int)ret == 1)
        //        {
        //            if (operate.Count >= maxCount)
        //            {
        //                // 随机删除一张照片
        //                Api.Redis.Call(new OperateRemveRandomPhoto(msg.Uid));
        //            }
        //        }
               
        //        // 2. 加入照片
        //        Api.Redis.Call(new OperateAddPhoto(msg.Uid, msg.PhotoName));
        //    });
        //    MSG_ZGC_UPLOAD_PHOTO response = new MSG_ZGC_UPLOAD_PHOTO();
        //    response.PhothName = msg.PhotoName;
        //    response.Result = (int)ErrorCode.Success;
        //    player.Write(response);
        //}

        //private void OnResponse_RemovePhoto(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_REMOVE_PHOTO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_REMOVE_PHOTO>(stream);
        //    Log.Write("player {0} remove photo {1}", msg.Uid, msg.PhotoName);
        //    PlayerChar player = Api.PCManager.FindPc(msg.Uid);
        //    if (player == null)
        //    {
        //        return;
        //    }
        //    Api.Redis.Call(new OperateRemovePhoto(msg.Uid, msg.PhotoName));
        //    MSG_ZGC_REMOVE_PHOTO response = new MSG_ZGC_REMOVE_PHOTO();
        //    response.Result = (int)ErrorCode.Success;
        //    response.PhotoName = msg.PhotoName;
        //    player.Write(response);
        //}

        //private void OnResponse_PhotoList(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_PHOTO_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_PHOTO_LIST>(stream);
        //    Log.Write("player {0} request player {1} photo list", msg.RequestUid, msg.OwnerUid);
        //    PlayerChar player = Api.PCManager.FindPc(msg.RequestUid);
        //    if (player == null)
        //    {
        //        return;
        //    }
        //    MSG_ZGC_PHOTO_LIST response = new MSG_ZGC_PHOTO_LIST();
        //    response.OwnerUid = msg.OwnerUid;
        //    OperateGetPhotoList operate =new OperateGetPhotoList(msg.OwnerUid);

        //    Api.Redis.Call(operate, ret =>
        //    {
        //        if ((int)ret == 1)
        //        {
        //            foreach (var item in operate.PotoList)
        //            {
        //                response.PhotoList.Add(item);
        //            }
        //            player.Write(response);
        //        }
        //    });
        //}

        //public void OnResponse_PopRank(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_POP_RANK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_POP_RANK>(stream);
        //    Log.Write("player {0} request pop rank page {1}", msg.Uid, msg.Page);
        //    PlayerChar player = Api.PCManager.FindPc(msg.Uid);
        //    if (player == null)
        //    {
        //        return;
        //    }
        //    MSG_ZGC_POP_RANK response = new MSG_ZGC_POP_RANK();
        //    response.Page = msg.Page;
        //    response.TotalCount = Api.PopRankMng.RankList.Count;
        //    List<PopRankPlayer> list = Api.PopRankMng.GetRankList(response.Page);
        //    response.MyRank = Api.PopRankMng.GetPlayerRank(msg.Uid);
        //    foreach (var item in list)
        //    {
        //        POP_RANK_PLAYER rankPlayer = new POP_RANK_PLAYER();
        //        rankPlayer.Uid = item.Uid;
        //        rankPlayer.Name = item.Name;
        //        rankPlayer.FaceIcon = item.FaceIcon;
        //        rankPlayer.ShowFaceJpg = item.ShowFaceJpg;
        //        rankPlayer.FaceFrame = item.FaceFrame;
        //        rankPlayer.Level = item.Level;
        //        rankPlayer.Score = item.Score;
        //        rankPlayer.FamilyName = item.FamilyName;
        //        rankPlayer.LadderLevel = item.LadderLevel;
        //        response.PlayerList.Add(rankPlayer);
        //    }
        //    player.Write(response);
        //}
    }
}
