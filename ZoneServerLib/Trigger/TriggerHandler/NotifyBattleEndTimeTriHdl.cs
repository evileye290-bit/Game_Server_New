using CommonUtility;
using Message.Gate.Protocol.GateC;
using ServerFrame;

namespace ZoneServerLib
{
    class NotifyBattleEndTimeTriHdl : BaseTriHdl
    {
        public NotifyBattleEndTimeTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
        }

        public override void Handle()
        {
            DungeonMap dungeon = CurMap as DungeonMap;
            if (dungeon == null)
            {
                return;
            }
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            // 通知前端战斗结束时间
            int stopTime = (int)dungeon.StopTime;
            foreach (var player in dungeon.PcList)
            {
                MSG_ZGC_BATTLE_END_TIME msg = new MSG_ZGC_BATTLE_END_TIME()
                {
                    Time = stopTime
                };
                player.Value.Write(msg);
            }
        }
    }
}
