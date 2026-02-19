using CommonUtility;

namespace ZoneServerLib
{
    /// <summary>
    /// 前端播放视频回放的时候创建该副本，防止在观看视频的时候被拉入其他副本中
    /// </summary>
    public class VideoPlayDungeon : DungeonMap
    {
        public VideoPlayDungeon(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
        }

        public override void OnStopBattle(PlayerChar player)
        {
            isQuitDungeon = true;
            Stop(DungeonResult.Failed);
            player.LeaveDungeon();
        }

        public override void OnPlayerLeave(PlayerChar player, bool cache = false)
        {
            base.OnPlayerLeave(player, cache);
            player.RecordLastMapInfo(player.OriginMapInfo.MapId, player.OriginMapInfo.Channel, player.OriginMapInfo.Position);
            player.SetPosition(player.OriginMapInfo.Position);
        }

        public override void Stop(DungeonResult result)
        {
            if (DungeonResult != DungeonResult.None)
            {
                return;
            }

            //副本结束取消所有trigger
            DungeonResult = result;
            State = DungeonState.Stopped;

            SaveHeroAndMonsterInfo();

            OnStopFighting();
        }

        public override bool NeedCheck()
        {
            return false;
        }
    }
}
