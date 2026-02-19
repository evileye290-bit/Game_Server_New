using DataProperty;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using RedisUtility;
using ServerModels;
using ServerShared;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //private List<int> HasGotRadioRewards = new List<int>();

        //public void BindRadioGotRadioRewards(RepeatedField<int> rewards)
        //{
        //    HasGotRadioRewards.Clear();
        //    HasGotRadioRewards.AddRange(rewards);
        //}

        ////获得主播信息人气排行
        //public void GetRadioAnchorList()
        //{
        //    //主播 id,  人气
        //    OperateGetRadioAllRankInfos operate = new OperateGetRadioAllRankInfos();
        //    server.Redis.Call(operate, ret =>
        //    {
        //        if ((int)ret == 1)
        //        {
        //            if (operate.List == null)
        //            {
        //                Log.Warn("player {0} GetRadioAnchorList not find list error", Uid);
        //                return;
        //            }
        //            else
        //            {
        //                DataList dataList = DataListManager.inst.GetDataList("RadioCaster");
        //                foreach (var item in dataList)
        //                {
        //                    if (!operate.List.ContainsKey(item.Value.ID))
        //                    {
        //                        operate.List.Add(item.Value.ID, 0);
        //                    }
        //                }

        //                MSG_ZGC_RADIO_ALL_ANCHOR_RANK msg = new MSG_ZGC_RADIO_ALL_ANCHOR_RANK();
        //                foreach (var item in operate.List)
        //                {
        //                    Data data = DataListManager.inst.GetData("RadioCaster", item.Key);
        //                    if (data == null)
        //                    {
        //                        Log.Warn("player {0} GetRadioAnchorList not find anchor {1} error", Uid, item.Key);
        //                        continue;
        //                    }

        //                    MSG_ZGC_ANCHOR_INFO info = new MSG_ZGC_ANCHOR_INFO();
        //                    info.Id = item.Key;
        //                    info.Num = item.Value;
        //                    msg.List.Add(info);
        //                }
        //                Write(msg);
        //                return;
        //            }
        //        }
        //    });
        //}

        ////获取主播贡献排行
        //public void GetRadioAnchorList(int anchorId)
        //{
        //    int showCount = 20;
        //    Data data = DataListManager.inst.GetData("RadioConfig", 1);
        //    if (data != null)
        //    {
        //        showCount = data.GetInt("ShowContributionRankCount");
        //        if (showCount <= 0)
        //        {
        //            showCount = 20;
        //        }
        //    }
        //    //主播 id,  人气
        //    OperateGetRadioAnchorRankInfos operate = new OperateGetRadioAnchorRankInfos(anchorId, 0, showCount - 1, Uid);
        //    server.Redis.Call(operate, ret =>
        //    {
        //        if ((int)ret == 1)
        //        {
        //            if (operate.List == null)
        //            {
        //                Log.Warn("player {0} GetRadioAnchorList not find list error", Uid);
        //                return;
        //            }
        //            else
        //            {
        //                var uids = operate.List.Select(x => (RedisValue)x.Key);
        //                OperateGetBaseInfoByIds operateSimpleInfo = new OperateGetBaseInfoByIds(uids);
        //                server.Redis.Call(operateSimpleInfo, ret1 =>
        //                {
        //                    if ((int)ret == 1)
        //                    {
        //                        MSG_ZGC_RADIO_ANCHOR_CONTRIBUTION_RANK msg = new MSG_ZGC_RADIO_ANCHOR_CONTRIBUTION_RANK();
        //                        msg.AnchorId = anchorId;
        //                        ulong value;
        //                        int rank = 0;
        //                        foreach (var pc in operateSimpleInfo.Characters)
        //                        {
        //                            if (operate.List.TryGetValue(pc.Uid, out value))
        //                            {
        //                                rank++;
        //                                PLAYER_BASE_INFO info = PlayerInfo.GetPlayerBaseInfo(pc);
        //                                info.Contribution = value;
        //                                info.Rank = rank;
        //                                msg.List.Add(info);

        //                                if (info.Uid == Uid)
        //                                {
        //                                    msg.MyInfo = info;
        //                                }
        //                            }
        //                        }
        //                        if (msg.MyInfo == null)
        //                        {
        //                            msg.MyInfo = PlayerInfo.GetPlayerBaseInfo(this);
        //                            msg.MyInfo.Contribution = operate.score;
        //                            msg.MyInfo.Rank = operate.rank;
        //                        }
        //                        Write(msg);
        //                    }
        //                    else
        //                    {
        //                        Log.Error("GetRadioAnchorList execute OperateGetFriendSimpleInfosByIds fail: redis data error!");
        //                        return;
        //                    }
        //                });
        //                return;
        //            }
        //        }
        //    });
        //}

        ////获取贡献总榜
        //public void GetRadioAllContributionList()
        //{
        //    //主播 id,  人气
        //    OperateGetRadioContributionRankInfos operate = new OperateGetRadioContributionRankInfos(0, 19);
        //    server.Redis.Call(operate, ret =>
        //    {
        //        if ((int)ret == 1)
        //        {
        //            if (operate.List == null)
        //            {
        //                Log.Warn("player {0} GetRadioAllContributionList not find list error", Uid);
        //                return;
        //            }
        //            else
        //            {
        //                var uids = operate.List.Select(x => (RedisValue)x.Key);
        //                OperateGetBaseInfoByIds operateSimpleInfo = new OperateGetBaseInfoByIds(uids);
        //                server.Redis.Call(operateSimpleInfo, ret1 =>
        //                {
        //                    if ((int)ret == 1)
        //                    {
        //                        MSG_ZGC_RADIO_ALL_CONTRIBUTION_RANK msg = new MSG_ZGC_RADIO_ALL_CONTRIBUTION_RANK();
        //                        ulong value;
        //                        foreach (var pc in operateSimpleInfo.Characters)
        //                        {
        //                            if (operate.List.TryGetValue(pc.Uid, out value))
        //                            {
        //                                PLAYER_BASE_INFO info = PlayerInfo.GetPlayerBaseInfo(pc);
        //                                info.Contribution = value;
        //                                msg.List.Add(info);
        //                            }
        //                        }
        //                        Write(msg);
        //                    }
        //                    else
        //                    {
        //                        Log.Error("GetRadioAllContributionList execute OperateGetFriendSimpleInfosByIds fail: redis data error!");
        //                        return;
        //                    }
        //                });
        //                return;
        //            }
        //        }
        //    });

        //}
    }
}
