using CommonUtility;
using DataProperty;
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
        public DragonBoatManager DragonBoatManager { get; private set; }
        public void InitDragonBoatManager()
        {
            DragonBoatManager = new DragonBoatManager(this);
        }   
        
        public void SendDragonBoatInfo()
        {
            if (RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.DragonBoat, ZoneServerApi.now))
            {              
                MSG_ZGC_DRAGON_BOAT_INFO msg = DragonBoatManager.GenerateDragonBoatInfo();
                Write(msg);
            }           
        }

        public void DragonBoatGameStart()
        {
            MSG_ZGC_DRAGON_BOAT_GAME_START response = new MSG_ZGC_DRAGON_BOAT_GAME_START();

            RechargeGiftModel model;
            if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.DragonBoat, ZoneServerApi.now, out model))
            {
                Log.Warn($"player {Uid} DragonBoatGameStart failed: time is error");
                response.Result = (int)ErrorCode.NotOnTime;
                Log.Write(response);
                return;
            }

            //检查门票
            DragonBoatTicketModel ticket = DragonBoatLibrary.GetTicketModelByPeriod(model.SubType);
            if (ticket == null)
            {
                Log.Warn($"player {Uid} DragonBoatGameStart failed: not find ticket in xml");
                response.Result = (int)ErrorCode.Fail;
                Log.Write(response);
                return;
            }

            BaseItem item = BagManager.GetItem(MainType.Consumable, ticket.TicketId);
            if (item == null || item.PileNum < ticket.ConsumeCount)
            {
                Log.Warn($"player {Uid} DragonBoatGameStart failed: ticket not enough");
                response.Result = (int)ErrorCode.ItemNotEnough;
                Log.Write(response);
                return;
            }

            List<int> directionList = DragonBoatLibrary.RandDirections();
            response.Directions.AddRange(directionList);
            response.Result = (int)ErrorCode.Success;

            Write(response);
        }

        public void DragonBoatGameEnd(int index)
        {
            MSG_ZGC_DRAGON_BOAT_GAME_END response = new MSG_ZGC_DRAGON_BOAT_GAME_END();

            RechargeGiftModel model;
            if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.DragonBoat, ZoneServerApi.now, out model))
            {
                Log.Warn($"player {Uid} DragonBoatGameEnd failed: time is error");
                response.Result = (int)ErrorCode.NotOnTime;
                Log.Write(response);
                return;
            }

            //检查门票
            DragonBoatTicketModel ticket = DragonBoatLibrary.GetTicketModelByPeriod(model.SubType);
            if (ticket == null)
            {
                Log.Warn($"player {Uid} DragonBoatGameEnd failed: not find ticket in xml");
                response.Result = (int)ErrorCode.Fail;
                Log.Write(response);
                return;
            }

            BaseItem item = BagManager.GetItem(MainType.Consumable, ticket.TicketId);
            if (item == null || item.PileNum < ticket.ConsumeCount)
            {
                Log.Warn($"player {Uid} DragonBoatGameEnd failed: ticket not enough");
                response.Result = (int)ErrorCode.ItemNotEnough;
                Log.Write(response);
                return;
            }

            if (index > DragonBoatLibrary.MaxOperateCount)
            {
                Log.Warn($"player {Uid} DragonBoatGameEnd failed: illegal operate");
                response.Result = (int)ErrorCode.Fail;
                Log.Write(response);
                return;
            }
            int lastDistance = DragonBoatManager.Info.CurDistance;

            int addDistance = DragonBoatLibrary.BasicDistance + index * DragonBoatLibrary.SpeedUpDistance;
            DragonBoatManager.UpdateCurDistance(addDistance);           

            BaseItem it = DelItem2Bag(item, RewardType.NormalItem, ticket.ConsumeCount, ConsumeWay.DragonBoat);
            if (it != null)
            {
                SyncClientItemInfo(it);
            }

            List<string> rewards = DragonBoatLibrary.GetCurDistanceLeftRewards(model.SubType, DragonBoatManager.Info.CurDistance, lastDistance, DragonBoatManager.Info.Bought);
            if (rewards.Count > 0)
            {
                RewardManager manager = new RewardManager();
                List<ItemBasicInfo> rewardItems = new List<ItemBasicInfo>();
                //按有装备和魂骨生成奖励
                foreach (var reward in rewards)
                {
                    if (!string.IsNullOrEmpty(reward))
                    {
                        RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                        List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                        rewardItems.AddRange(items);
                    }
                }
                manager.AddReward(rewardItems);
                manager.BreakupRewards(true);
                AddRewards(manager, ObtainWay.DragonBoat);

                manager.GenerateRewardMsg(response.Rewards);
            }

            response.CurDistance = DragonBoatManager.Info.CurDistance;
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void DragonBoatBuyTicket(int id, int count)
        {
            MSG_ZGC_DRAGON_BOAT_BUY_TICKET response = new MSG_ZGC_DRAGON_BOAT_BUY_TICKET();
            response.Id = id;
            response.Count = count;

            RechargeGiftModel model;
            if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.DragonBoat, ZoneServerApi.now, out model))
            {
                Log.Warn($"player {Uid} DragonBoatBuyTicket failed: time is error");
                response.Result = (int)ErrorCode.NotOnTime;
                Log.Write(response);
                return;
            }

            //检查门票           
            DragonBoatTicketModel ticket = DragonBoatLibrary.GetTicketModelByPeriod(model.SubType);
            if (ticket == null)
            {
                Log.Warn($"player {Uid} DragonBoatBuyTicket failed: not find ticket in xml");
                response.Result = (int)ErrorCode.Fail;
                Log.Write(response);
                return;
            }

            if (id != ticket.TicketId || count > DragonBoatLibrary.OnceTicketMaxBuyCount || count <= 0)
            {
                Log.Warn($"player {Uid} DragonBoatBuyTicket failed: illegal id {id} right param {ticket.TicketId}");
                response.Result = (int)ErrorCode.Fail;
                Log.Write(response);
                return;
            }

            Data buyData = DataListManager.inst.GetData("Counter", (int)CounterType.DragonBoatTicketBuyCount);
            if (buyData == null)
            {
                Log.Warn($"player {Uid} DragonBoatBuyTicket failed: not find counter in xml");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            //最低一次
            if (count <= 0) count = 1;

            int buyedCount = GetCounterValue(CounterType.DragonBoatTicketBuyCount);

            string costStr = buyData.GetString("Price");
            if (string.IsNullOrEmpty(costStr))
            {
                Log.Warn($"player {Uid} DragonBoatBuyTicket failed: not have price");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            int costCoin = 0;
            for (int i = 1; i <= count; i++)
            {
                costCoin += CounterLibrary.GetBuyCountCost(costStr, buyedCount + i);
            }

            if (!CheckCoins(CurrenciesType.diamond, costCoin))
            {
                Log.Warn($"player {uid} DragonBoatBuyTicket error: coins not enough, curCoin {GetCoins(CurrenciesType.diamond)} cost {costCoin}");
                response.Result = (int)ErrorCode.DiamondNotEnough;
                Write(response);
                return;
            }

            DelCoins(CurrenciesType.diamond, costCoin, ConsumeWay.DragonBoatBuyTicket, id.ToString());

            UpdateCounter(CounterType.DragonBoatTicketBuyCount, count);

            RewardManager manager = new RewardManager();
            manager.AddReward(new ItemBasicInfo((int)RewardType.NormalItem, id, count));
            manager.BreakupRewards();
            // 发放奖励
            AddRewards(manager, ObtainWay.DragonBoatBuyTicket);
            //通知前端奖励
            manager.GenerateRewardMsg(response.Rewards);
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public bool CheckCanBuyDragonBoatRights(RechargeItemModel rechargeItem)
        {
            RechargeGiftModel model;
            if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.DragonBoat, ZoneServerApi.now, out model))
            {
                return false;
            }

            if (rechargeItem.SubType != model.SubType)
            {
                return false;
            }

            if (DragonBoatManager.Info.Bought == 1)
            {
                return false;
            }
            return true;
        }

        public void UpdateDragonBoatBuyInfo(RechargeItemModel rechargeItem, string giftReward)
        {
            DragonBoatManager.UpdateBuyInfo();

            RewardManager manager = new RewardManager();
            List<string> rewards = new List<string>();
            List<string> rightsRewards = DragonBoatLibrary.GetAvailableRightsRewards(rechargeItem.SubType, DragonBoatManager.Info.CurDistance);
            rewards.AddRange(rightsRewards);
            if (rewards.Count > 0)
            {
                List<ItemBasicInfo> rewardItems = new List<ItemBasicInfo>();
                //按有装备和魂骨生成奖励
                foreach (var reward in rewards)
                {
                    if (!string.IsNullOrEmpty(reward))
                    {
                        RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                        List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                        rewardItems.AddRange(items);
                    }
                }
                manager.AddReward(rewardItems);
                manager.BreakupRewards(true);
                AddRewards(manager, ObtainWay.DragonBoat);
                rewards.Clear();
            }
            
            string[] giftRewardArr = StringSplit.GetArray("|", giftReward);
            foreach (var item in giftRewardArr)
            {
                rewards.Add(item);
            }

            if (rewards.Count > 0)
            {
                List<ItemBasicInfo> boughtRewards = new List<ItemBasicInfo>();
                foreach (var reward in rewards)
                {
                    if (!string.IsNullOrEmpty(reward))
                    {
                        RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                        List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                        boughtRewards.AddRange(items);
                    }
                }
                manager.AddReward(boughtRewards);
            }
            
            MSG_ZGC_RECHARGE_GIFT response = new MSG_ZGC_RECHARGE_GIFT();
            response.GiftItemId = rechargeItem.Id;
            response.BuyCount = 1;
            response.Result = (int)ErrorCode.Success;
            manager.GenerateRewardMsg(response.Rewards);  
            Write(response);

            MSG_ZGC_DRAGON_BOAT_INFO msg = DragonBoatManager.GenerateDragonBoatInfo();
            Write(msg);
        }

        public void RefreshDragonBoatFreeTicketState()
        {
            RechargeGiftModel model;
            if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.DragonBoat, ZoneServerApi.now, out model))
            {            
                return;
            }
            if (DragonBoatManager.Info.FreeTicketState == 1)
            {
                DragonBoatManager.UpdateFreeTicketState(0);

                MSG_ZGC_DRAGON_BOAT_INFO msg = DragonBoatManager.GenerateDragonBoatInfo();
                Write(msg);
            }
        }

        public void DragonBoatGetFreeTicket()
        {
            MSG_ZGC_DRAGON_BOAT_FREE_TICKET response = new MSG_ZGC_DRAGON_BOAT_FREE_TICKET();

            RechargeGiftModel model;
            if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.DragonBoat, ZoneServerApi.now, out model))
            {
                Log.Warn($"player {Uid} DragonBoatGetFreeTicket failed: time is error");
                response.Result = (int)ErrorCode.NotOnTime;
                Log.Write(response);
                return;
            }

            DragonBoatTicketModel ticket = DragonBoatLibrary.GetTicketModelByPeriod(model.SubType);
            if (ticket == null)
            {
                Log.Warn($"player {Uid} DragonBoatGetFreeTicket failed: not find ticket in xml");
                response.Result = (int)ErrorCode.Fail;
                Log.Write(response);
                return;
            }

            if (DragonBoatManager.Info.FreeTicketState == 1)
            {
                Log.Warn($"player {Uid} DragonBoatGetFreeTicket failed: already got");
                response.Result = (int)ErrorCode.AlreadyGot;
                Log.Write(response);
                return;
            }

            DragonBoatManager.UpdateFreeTicketState(1);

            RewardManager manager = new RewardManager();
            manager.AddReward(new ItemBasicInfo((int)RewardType.NormalItem, ticket.TicketId, ticket.GiveCount));
            manager.BreakupRewards();
            // 发放奖励
            AddRewards(manager, ObtainWay.DragonBoat);
            //通知前端奖励
            manager.GenerateRewardMsg(response.Rewards);           
            response.Result = (int)ErrorCode.Success;
            Write(response);

            MSG_ZGC_DRAGON_BOAT_INFO msg = DragonBoatManager.GenerateDragonBoatInfo();
            Write(msg);
        }

        public MSG_ZMZ_DRAGON_BOAT_INFO GenerateDragonBoatTransformMsg()
        {
            MSG_ZMZ_DRAGON_BOAT_INFO msg = new MSG_ZMZ_DRAGON_BOAT_INFO();
            msg.CurDistance = DragonBoatManager.Info.CurDistance;
            msg.Bought = DragonBoatManager.Info.Bought;
            msg.FreeTicketState = DragonBoatManager.Info.FreeTicketState;
            return msg;
        }

        public void LoadDragonBoatTransform(MSG_ZMZ_DRAGON_BOAT_INFO msg)
        {
            DragonBoatManager.LoadDragonBoatTransform(msg);
        }
    }
}
