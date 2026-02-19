using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_GiftCodeExchangeReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GIFT_CODE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} use gift code exchange reward not find client", pcUid);
            }
        }

        private void OnResponse_CheckCodeUnique(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHECK_CODE_UNIQUE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} check gift code unique not find client", pcUid);
            }
        }

        private void OnResponse_SendGiftInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GIFT_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} send gift info not find client", pcUid);
            }
        }

        private void OnResponse_RechargeGift(MemoryStream stream, int pcUid)
        {
            MSG_ZGC_RECHARGE_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZGC_RECHARGE_GIFT>(stream);

            Log.Info($"RechargeGift notify client {pcUid} gift itemId {msg.GiftItemId}  buy count{msg.BuyCount}");

            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RECHARGE_GIFT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} recharge gift not find client", pcUid);
            }
        }

        private void OnResponse_RefreshGift(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_REFRESH_GIFT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} refresh gift not find client", pcUid);
            }
        }

        private void OnResponse_ReceiveRechargeReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RECEIVE_RECHARGE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} receive recharge reward not find client", pcUid);
            }
        }

        private void OnResponse_TestRecharge(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TEST_RECHARGE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} test recharge not find client", pcUid);
            }
        }

        private void OnResponse_GiftOpen(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GIFT_OPEN>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} gift open not find client", pcUid);
            }
        }

        private void OnResponse_LimitTimeGifts(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_LIMIT_TIME_GIFTS>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} limit time gifts not find client", pcUid);
            }
        }

        private void OnResponse_UseRechargeToken(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_USE_RECHARGE_TOKEN>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} use recharge token not find client", pcUid);
            }
        }

        private void OnResponse_ResetDoubleFlag(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RESET_DOUBLE_FLAG>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} reset recharge item double flag not find client", pcUid);
            }
        }

        private void OnResponse_ResetRechargeDiscount(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RESET_RECHARGE_DISCOUNT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} reset recharge item discount not find client", pcUid);
            }
        }

        private void OnResponse_CultivateGiftOpen(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CULTIVATE_GIFT_OPEN>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} cultivate gift open not find client", pcUid);
            }
        }

        private void OnResponse_BuyCultivateGift(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BUY_CULTIVATE_GIFT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} buy cultivate gift not find client", pcUid);
            }
        }

        private void OnResponse_CultivateGiftList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CULTIVATE_GIFT_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} cultivate gift list not find client", pcUid);
            }
        }

        private void OnResponse_PettyGiftList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PETTY_GIFT_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} petty gift list not find client", pcUid);
            }
        }

        private void OnResponse_PettyGiftRefresh(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PETTY_GIFT_REFRESH>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} petty gift refresh not find client", pcUid);
            }
        }

        private void OnResponse_BuyPettyGift(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BUY_PETTY_GIFT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} buy petty gift not find client", pcUid);
            }
        }

        private void OnResponse_ReceiveFreePettyGift(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FREE_PETTY_GIFT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} receive free petty gift not find client", pcUid);
            }
        }

        private void OnResponse_GetDailyRechargeInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DAILY_RECHARGE_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get daily recharge info not find client", pcUid);
            }
        }

        private void OnResponse_GetDailyRechargeReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_DAILY_RECHARGE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get daily recharge reward not find client", pcUid);
            }
        }

        private void OnResponse_HeroDaysRewardsInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HERO_DAYS_REWARDS_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get hero days rewards info not find client", pcUid);
            }
        }

        private void OnResponse_GetHeroDaysReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_HERO_DAYS_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get hero days reward not find client", pcUid);
            }
        }

        private void OnResponse_GetNewServerPromotionInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_NEWSERVER_PROMOTION_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get new server promotion info not find client", pcUid);
            }
        }

        private void OnResponse_GetNewServerPromotionReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_NEWSERVER_PROMOTION_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get new server promotion reward not find client", pcUid);
            }
        }

        private void OnResponse_GetLuckyFlipCardInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_LUCKY_FLIP_CARD_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get lucky flip card info not find client", pcUid);
            }
        }

        private void OnResponse_GetLuckyFlipCardReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_LUCKY_FLIP_CARD_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get lucky flip card reward not find client", pcUid);
            }
        }

        private void OnResponse_GetLuckyCardCumulateReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_LUCKY_FLIP_CARD_CUMULATE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get lucky flip card cumulate reward not find client", pcUid);
            }
        }

        private void OnResponse_IslandHighGiftInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ISLAND_HIGH_GIFT_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get island high gift info not find client", pcUid);
            }
        }
        private void OnResponse_GetTreasureFlipCardInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TREASURE_FLIP_CARD_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get treasure flip card info not find client", pcUid);
            }
        }

        private void OnResponse_GetTreasureFlipCardReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_TREASURE_FLIP_CARD_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get treasure flip card reward not find client", pcUid);
            }
        }

        private void OnResponse_GetTreasureCardCumulateReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_TREASURE_FLIP_CARD_CUMULATE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get treasure flip card cumulate reward not find client", pcUid);
            }
        }
    }
}
