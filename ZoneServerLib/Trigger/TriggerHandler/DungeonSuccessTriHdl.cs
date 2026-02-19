using CommonUtility;
using ServerFrame;
using ServerShared;
using System;
using System.Threading;

namespace ZoneServerLib
{
    public class DungeonSuccessTriHdl : BaseTriHdl
    {
        private int delayTime = 3000;//ms
        private DungeonMap dungeon;

        public DungeonSuccessTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            dungeon = CurMap as DungeonMap;
            if(dungeon == null)
            {
                //Log.Debug("trigger handler type {0} in init failed: cur map is not dungeon", handlerType);
                return;
            }
            float time;
            if (float.TryParse(handlerParam, out time))
            {
                delayTime = (int)(float.Parse(handlerParam) * 1000);
            }
            else
            {
                Logger.Log.Debug("trigger handler type {0} in init failed: cur map is not dungeon", handlerType);
            }
        }

        public override void Handle()
        {
            if (dungeon == null || dungeon.State != DungeonState.Started)
            {
                return;
            }

            dungeon.SetSpeedUp(false);

            ulong time = Timestamp.GetUnixTimeStamp(DateTime.Now.AddMilliseconds(delayTime));

            //delaytime 之后进行副本结算
            TimerManager.Instance.NewOnceTimer(time, DoHandler);
        }

        private void DoHandler(object param)
        {
            dungeon.Stop(DungeonResult.Success);
        }
    }
}
