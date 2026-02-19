using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public ShrekInvitationManager ShrekInvitationMng { get; private set; }

        public void InitShrekInvitationManager()
        {
            ShrekInvitationMng = new ShrekInvitationManager(this);
        }

        public void SendShrekInvitationInfoByLoading()
        {
            if (RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.ShrekInvitation, ZoneServerApi.now))
            {
                SendShrekInvitationInfo();
            }
        }

        public void SendShrekInvitationInfo()
        {
            MSG_ZGC_SHREK_INVITATION_INFO msg = new MSG_ZGC_SHREK_INVITATION_INFO();
            foreach (var kv in ShrekInvitationMng.InfoDic)
            {
                msg.InfoList.Add(GenerateShrekInvitationInfo(kv.Value));
            }
            Write(msg);
        }

        private ZGC_SHREK_INVITATION_INFO GenerateShrekInvitationInfo(ShrekInvitationInfo info)
        {
            ZGC_SHREK_INVITATION_INFO msg = new ZGC_SHREK_INVITATION_INFO()
            {
                Id = info.Id,
                GetState = (int)info.GetState,
                GetTime = info.GetTime
            };
            return msg;
        }

        public void GetShrekInvitationReward(int id, int type)
        {
            MSG_ZGC_GET_SHREK_INVITAION_REWARD response = new MSG_ZGC_GET_SHREK_INVITAION_REWARD();
            response.Id = id;
            response.Type = type;

            RechargeGiftModel activityModel;
            if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.ShrekInvitation, ZoneServerApi.now, out activityModel))
            {
                response.Result = (int)ErrorCode.NotOnTime;
                Log.Warn($"player {Uid} get shrek invitation reward failed: activity not open");
                Write(response);
                return;
            }

            ShrekInvitationModel model = ShrekInvitationLibrary.GetShrekInvitationModel(id);
            if (model == null)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} get shrek invitation reward failed: not find reward in xml");
                Write(response);
                return;
            }

            if (server.Now().Date < activityModel.StartTime.Date.AddDays(model.Days - 1))
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} get shrek invitation reward failed: not on time");
                Write(response);
                return;
            }

            //判断是否是补签
            bool needCost = false;
            int coinType = 0;
            int costNum = 0;

            if (type == 1 && server.Now().Date > activityModel.StartTime.Date.AddDays(model.Days - 1))
            {
                if (model.CostCoin.Length < 3)
                {
                    Log.Warn($"player {Uid} get shrek invitation reward failed: costCoin param error");
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }

                coinType = int.Parse(model.CostCoin[0]);
                costNum = int.Parse(model.CostCoin[2]);

                if (GetCoins((CurrenciesType)coinType) < costNum)
                {
                    Log.Warn($"player {Uid} get shrek invitation reward failed: costCoin not enough");
                    response.Result = (int)ErrorCode.DiamondNotEnough;
                    Write(response);
                    return;
                }
                needCost = true;
            }

            ShrekInvitationInfo info;
            ShrekInvitationMng.InfoDic.TryGetValue(id, out info);
            if (info == null)
            {
                if (type != 1)
                {
                    response.Result = (int)ErrorCode.Fail;
                    Log.Warn($"player {Uid} get shrek invitation reward failed: type {type} param error");
                    Write(response);
                    return;
                }
                info = ShrekInvitationMng.AddShrekInvitationInfo(id);
                SyncDbInsertShrekInvitationInfo(info);
            }
            else
            {            
                if (type == 1)
                {
                    //检查状态
                    if (info.GetState != RewardGetState.None)
                    {
                        response.Result = (int)ErrorCode.Fail;
                        Log.Warn($"player {Uid} get shrek invitation reward failed: type {type} reward already got");
                        Write(response);
                        return;
                    }
                    info.GetState = RewardGetState.GetOnce;
                    info.GetTime = Timestamp.GetUnixTimeStampSeconds(server.Now());
                }
                else if (type == 2)
                {
                    //检查状态和时间                    
                    if (info.GetState != RewardGetState.GetOnce)
                    {
                        response.Result = (int)ErrorCode.Fail;
                        Log.Warn($"player {Uid} get shrek invitation reward failed: type {type} reward already got or error");
                        Write(response);
                        return;
                    }
                    if (info.GetTime + model.CountDownTime >= Timestamp.GetUnixTimeStampSeconds(server.Now()))
                    {
                        response.Result = (int)ErrorCode.Fail;
                        Log.Warn($"player {Uid} get shrek invitation reward failed: not reach count down time");
                        Write(response);
                        return;
                    }
                    info.GetState = RewardGetState.GetTwice;
                }
                else
                {
                    response.Result = (int)ErrorCode.Fail;
                    Log.Warn($"player {Uid} get shrek invitation reward failed: type {type} error");
                    Write(response);
                    return;
                }
                SyncDbUpdateShrekInvitationInfo(info);
            }
            response.GetState = (int)info.GetState;
            response.GetTime = info.GetTime;

            //扣钻
            if (type == 1 && needCost)
            {
                DelCoins((CurrenciesType)coinType, costNum, ConsumeWay.ShrekInvitation, id.ToString());
            }
                          
            string reward = string.Empty;
            if (type == 1)
            {
                reward = model.BasicRewards;
            }
            else if (type == 2)
            {
                reward = model.ExpressRewards;
            }
            //按有装备和魂骨生成奖励
            if (!string.IsNullOrEmpty(reward))
            {
                RewardManager manager = new RewardManager();
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                List<ItemBasicInfo> rewardItems = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                manager.AddReward(rewardItems);
                manager.BreakupRewards(true);
                AddRewards(manager, ObtainWay.ShrekInvitation);
                manager.GenerateRewardMsg(response.Rewards);
            }
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void ClearShrekInvitationInfo()
        {
            ShrekInvitationMng.Clear();
            SendShrekInvitationInfo();
        }

        private void SyncDbInsertShrekInvitationInfo(ShrekInvitationInfo info)
        {
            server.GameDBPool.Call(new QueryInsertShrekInvitationInfo(Uid, info.Id, (int)info.GetState, info.GetTime));
        }

        private void SyncDbUpdateShrekInvitationInfo(ShrekInvitationInfo info)
        {
            server.GameDBPool.Call(new QueryUpdateShrekInvitationInfo(Uid, info.Id, (int)info.GetState, info.GetTime));
        }

        public MSG_ZMZ_SHREK_INVITATION_INFO GenerateShrekInvitationTransformMsg()
        {
            MSG_ZMZ_SHREK_INVITATION_INFO msg = new MSG_ZMZ_SHREK_INVITATION_INFO();
            foreach (var item in ShrekInvitationMng.InfoDic)
            {
                ZMZ_SHREK_INVITATION_INFO info = new ZMZ_SHREK_INVITATION_INFO()
                {
                    Id = item.Value.Id,
                    GetState = (int)item.Value.GetState,
                    GetTime = item.Value.GetTime
                };
                msg.List.Add(info);
            }
            return msg;
        }

        public void LoadShrekInvitationTransform(MSG_ZMZ_SHREK_INVITATION_INFO msg)
        {
            foreach (var item in msg.List)
            {
                ShrekInvitationMng.AddShrekInvitationInfo(item.Id, item.GetState, item.GetTime);
            }
        }
    }
}
