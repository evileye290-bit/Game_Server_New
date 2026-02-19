using CommonUtility;
using DBUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class GodPathAcrossOceanTask : BaseGodPathTask
    {
        private List<int> puzzle = new List<int>();
        public List<int> Puzzle => puzzle;

        public GodPathAcrossOceanTask(GodPathHero goldPathHero, GodPathTaskModel model, GodPathDBInfo info) : base(goldPathHero, model)
        {
            ParsePuzzleItem(info.AcroessOceanPuzzle);
        }

        public override void Init()
        {
            puzzle.Clear();
        }

        public void SetLightedPuzzle(int index)
        {
            if (!puzzle.Contains(index))
            {
                puzzle.Add(index);
            }
        }

        public override bool Check(HeroInfo hero)
        {
            return puzzle.Count == GodPathLibrary.AcrossOceanPuzzleCount;
        }

        public bool IsLighted(int index)
        {
            return puzzle.Contains(index);
        }

        public override void Reset()
        {
            puzzle.Clear();
        }

        public override void GenerateDBInfo(GodPathDBInfo info)
        {
            info.AcroessOceanPuzzle = PuzzleItemToString();
        }

        public override void GenerateMsg(MSG_GOD_HERO_INFO msg)
        {
            msg.AcroessOceanState = Check(null) ? 1 : 0;
            msg.AcroessOceanPuzzle = PuzzleItemToString();
        }

        public override void GenerateTransformInfo(ZMZ_GOD_HERO_INFO msg)
        {
            msg.AcroessOceanPuzzle = PuzzleItemToString();
        }


        public void SyncDBAcrossOceanInfo()
        {
            QueryUpdateGodHeroAcrossOceanPuzzle query = new QueryUpdateGodHeroAcrossOceanPuzzle(GodPathHero.Manager.Owner.Uid, GodPathHero.HeroId, PuzzleItemToString());
            GodPathHero.Manager.Owner.server.GameDBPool.Call(query);
        }

        private void ParsePuzzleItem(string puzzleItemStr)
        {
            if (string.IsNullOrEmpty(puzzleItemStr)) return;
            List<int> items = StringSplit.GetInts(puzzleItemStr);

            if (items.Count != GodPathLibrary.AcrossOceanPuzzleCount)
            {
                Log.Error($"player {GodPathHero.Manager.Owner.Uid} hero {GodPathHero.HeroId} ParsePuzzleItem error, data error");
                return;
            }

            for (int i = 1; i <= GodPathLibrary.AcrossOceanPuzzleCount; ++i)
            {
                if (items[i - 1] == 1)
                {
                    puzzle.Add(i);
                }
            }
        }

        public string PuzzleItemToString()
        {
            List<int> items = new List<int>();
            for (int i = 1; i <= GodPathLibrary.AcrossOceanPuzzleCount; ++i)
            {
                items.Add(puzzle.Contains(i) ? 1 : 0);
            }
            return string.Join("|", items);
        }
    }
}
