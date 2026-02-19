using DBUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class GodPathSevenDouluoFightTask : BaseGodPathTask
    {
        public static readonly int ReadyState = 0;
        public static readonly int WinState = 1;
        public static readonly int LoseState = 2;

        public int SevenFightStage { get; private set; }
        public int SevenFightState { get; private set; }
        public int SevenFightWinCount { get; private set; }
        public int SevenFightHP { get; private set; }


        public GodPathSevenDouluoFightTask(GodPathHero goldPathHero, GodPathTaskModel model, GodPathDBInfo info) : base(goldPathHero, model)
        {
            SevenFightStage = info.SevenFightStage;
            SevenFightState = info.SevenFightState;
            SevenFightWinCount = info.SevenFightWinCount;
            SevenFightHP = info.SevenFightHP;
        }

        public override bool Check(HeroInfo hero)
        {
            return SevenFightStage== GodPathLibrary.SevenFightMaxStage && SevenFightState == WinState &&
                SevenFightWinCount == GodPathLibrary.SevenFightWinCountToNextStage;
        }

        public override void GenerateDBInfo(GodPathDBInfo info)
        {
            info.SevenFightStage = SevenFightStage;
            info.SevenFightState = SevenFightState;
            info.SevenFightWinCount = SevenFightWinCount;
            info.SevenFightHP = SevenFightHP;
        }

        public override void GenerateMsg(MSG_GOD_HERO_INFO msg)
        {
            msg.SevenFightStage = SevenFightStage;
            msg.SevenFightState = SevenFightState;
            msg.SevenFightWinCount = SevenFightWinCount;
            msg.SevenFightHP = SevenFightHP;
        }

        public override void GenerateTransformInfo(ZMZ_GOD_HERO_INFO msg)
        {
            msg.SevenFightStage = SevenFightStage;
            msg.SevenFightState = SevenFightState;
            msg.SevenFightWinCount = SevenFightWinCount;
            msg.SevenFightHP = SevenFightHP;
        }

        public override void Init()
        {
            SevenFightStage = 1;
            SevenFightState = ReadyState;
            SevenFightWinCount = 0;
            SevenFightHP = 0;
        }

        public override void Reset()
        {
            SevenFightStage = 1;
            SevenFightState = ReadyState;
            SevenFightWinCount = 0;
            SevenFightHP = 0;
        }

        public override void DailyReset()
        {
            SevenFightHP = GodPathLibrary.SevenFightPower;
            //SyncDBSevenFightInfo();
        }

        public void AddWinCount()
        {
            if (SevenFightWinCount >= GodPathLibrary.SevenFightWinCountToNextStage) return;

            SevenFightState = WinState;
            SevenFightWinCount += 1;

            GotoNextStage();
        }

        public void GotoNextStage()
        {
            if (SevenFightWinCount >= GodPathLibrary.SevenFightWinCountToNextStage)
            {
                if (SevenFightStage >= GodPathLibrary.SevenFightMaxStage) return;

                SevenFightStage += 1;
                SevenFightWinCount = 0;
            }
            SevenFightState = ReadyState;
            SyncDBSevenFightInfo();
        }

        public void SetWinState()
        {
            SevenFightState = WinState;
            SyncDBSevenFightInfo();
        }

        public void SetLoseState()
        {
            SevenFightState = LoseState;
            SyncDBSevenFightInfo();
        }

        public void AddHp(int hp)
        {
            SevenFightHP += hp;
            SyncDBSevenFightInfo();
        }

        private void SyncDBSevenFightInfo()
        {
            QueryUpdateGodHeroSevenFight query = new QueryUpdateGodHeroSevenFight(GodPathHero.Manager.Owner.Uid, 
                GodPathHero.HeroId, SevenFightStage, SevenFightWinCount, SevenFightState, SevenFightHP);
            GodPathHero.Manager.Owner.server.GameDBPool.Call(query);
        }
    }
}
