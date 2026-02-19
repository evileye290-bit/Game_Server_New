namespace BattleServerLib
{
    static public class TargetSelect
    {
        public const int Enemy = 0;
        public const int Ally = 1;
        public const int All = 2;
    }
    public enum ActorType
    {
        NONE = -1,
        PC = 0,
        HERO = 1,
        ALL = 10,
    };
    static public class DEF
    {
        public const string True = "true";
        public const string False = "false";
        public const float MaxWeight = 1.99f;
    }
    static public class ANIMID
    {
        public const string STD = "std";
        public const string SIT = "sit";
        public const string ASTD = "astd";
        public const string HIT = "hit";

        public const string RUN_STD = "run_std";
        public const string RUN = "run";
        public const string RUN_MAD = "run_madly";          //快跑
        public const string RUN_STOP = "run_stop";          //滑停
        public const string ATK = "atk1";
        public const string ATK2 = "atk2";
        public const string ATK3 = "atk3";
        public const string CURE = "cure";

        public const string DEAD = "dead";
        public const string Reviving = "reviving";

        public const string SummonStart = "summon_start";
        public const string SummonLoop = "summon_loop";
        public const string SummonCall = "summon_call";
        public const string SummonEnd = "summon_end";
        public const string Revive = "revive";
        public const string Born = "born";
        public const string Sleep = "sleep";
        public const string None = "none";

        public const string ASTD_LEV = "astd_lev";
    }
    public class DelayInfo
    {
        public const float SummonCall = 0.76f;
    }
    public class EffectName
    {
        public const string SummonStart = "ZhaoHuanQiLin_Start";
        public const string SummonLoop = "ZhaoHuanQiLin_Loop";
        public const string SummonCall = "ZhaoHuanQiLin_Call";
        public const string SummonEnd = "ZhaoHuanQiLin_End";
        public const string ShieldStart = "Fanghuzhao_shifang_Start";
        public const string ShieldLoop = "Fanghuzhao_Xunhuan01_Loop";
        public const string ShieldHit = "Fanghuzhao_Hit";
        public const string DragHero = "DragHero";
        public const string CivetC_buff = "CivetC_buff";
        public const string Minotaur_buff = "Minotaur_buff";
    }
    static public class MainHero
    {
        public const int MaleId = 1;
        public const int FeMaleId = 2;
    }

    public enum SummonStage
    {
        None = 0,
        Start,
        Loop,
        Call,
        End,
    }
    public enum ActiveBHType
    {
        Position = 0,
        Target = 1,
        Revive = 2,
        Switch = 3,
        Image = 4,
        Click = 5,
    }
    public enum BreakType
    {
        Dead,
        Swallow,
        Leave,
        Silence,
        Dizzy,
        Summon,
        Force,
        Hit,
        Sleep,
        Frozen,
        All,
    }
}
