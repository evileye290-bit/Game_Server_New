/*************************************************
    文 件 : GateServer_DomainBenediction.cs
    日 期 : 2022年4月7日 14:38:45
    作 者 : jinzi
    策 划 : 
    说 明 : 神域赐福协议处理
*************************************************/
using System.IO;
using Logger;
using Message.Gate.Protocol.GateZ;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        /// <summary>
        /// 领取阶段奖励
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        public void OnResponse_HandleGetStageAward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_DOMAIN_BENEDICTION_GET_STAGE_AWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DOMAIN_BENEDICTION_GET_STAGE_AWARD>(stream);
            Log.Write($"player [{uid}] HandleGetStageAward");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn($"player [{uid}] HandleGetStageAward not in gateid [{SubId}] pc list");
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} HandleGetStageAward not in map ", uid);
                return;
            }

            player.HandleGetStageAward(pks.Id);
        }
        
        /// <summary>
        /// 领取祈愿奖励
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        public void OnResponse_HandleGetBaseCurrencyAward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_DOMAIN_BENEDICTION_GET_BASE_CURRENCY_AWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DOMAIN_BENEDICTION_GET_BASE_CURRENCY_AWARD>(stream);
            Log.Write($"player [{uid}] HandleGetBaseCurrencyAward");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn($"player [{uid}] HandleGetBaseCurrencyAward not in gateid [{SubId}] pc list");
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} HandleGetBaseCurrencyAward not in map ", uid);
                return;
            }

            player.HandleGetBaseCurrencyAward();
        }
        
        /// <summary>
        /// 进行祈福操作
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        public void OnResponse_HandlePrayOperation(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_DOMAIN_BENEDICTION_PRAY_OPERATION pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DOMAIN_BENEDICTION_PRAY_OPERATION>(stream);
            Log.Write($"player [{uid}] HandlePrayOperation");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn($"player [{uid}] HandlePrayOperation not in gateid [{SubId}] pc list");
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} HandlePrayOperation not in map ", uid);
                return;
            }

            player.HandlePrayOperation();
        }
        
        /// <summary>
        /// 处理抽取操作
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        public void OnResponse_HandleDrawOperation(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_DOMAIN_BENEDICTION_DRAW_OPERATION pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DOMAIN_BENEDICTION_DRAW_OPERATION>(stream);
            Log.Write($"player [{uid}] HandleDrawOperation");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn($"player [{uid}] HandleDrawOperation not in gateid [{SubId}] pc list");
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} HandleDrawOperation not in map ", uid);
                return;
            }

            player.HandleDrawOperation(pks.Id);
        }
        
    }
}