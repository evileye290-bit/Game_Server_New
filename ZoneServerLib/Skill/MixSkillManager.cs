using CommonUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ScriptFighting;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class MixSkillComposeHero
    {
        public FieldObject Hero_1 { get; private set; }
        public FieldObject Hero_2 { get; private set; }
        public int ComposeId { get; private set; }

        public MixSkillComposeHero(FieldObject hero_1, FieldObject hero_2, int composeId)
        {
            Hero_1 = hero_1;
            Hero_2 = hero_2;
            ComposeId = composeId;
        }

    }


    public class MixSkillManager
    {
        DungeonMap curDungeon;
        MixSkillComposeHero heroCompose;
        MixSkillModel model;

        MixSkillTree tree;

        private MixSkillState state;

        // 触发次数 即生成Tree的次数 不是技能结算次数
        private int startedCount;

        // 一堆计时信息
        private float startedTimeLeft; // 副本开始一段时间后才开始计算
        private float cdTimeLeft; // 上次弹出弹版后，下次弹出弹版的CD时间
        private float checkProbabilityTimeLeft; //每隔1秒检查一次概率
        private float lastStopedElapsed; // 上次技能版关闭后的累计时间，在Start状态下重新进入Stop状态下流逝的时间
        private float enableTimeLeft; // 点亮倒计时

        public MixSkillManager(DungeonMap dungeon, MixSkillModel model)
        {
            curDungeon = dungeon;
            this.model = model;
            startedTimeLeft = model.S;
            cdTimeLeft = model.F;
            checkProbabilityTimeLeft = 1;
            state = MixSkillState.Stopped;
        }

        public bool GenerateMixSkill()
        {
            // 第一步 随出谁的融合技
            heroCompose = GeneratelHeroCompose();
            if (heroCompose == null)
            {
                return false;
            }

            // 第二步 随出要用的buff
            List<int> buffList = model.GenerateBuffs(4);
            if (buffList == null)
            {
                return false;
            }

            // 第三步 生产技能树
            tree = new MixSkillTree((int)JobType.SingleAttack, (int)JobType.Control, 3, buffList);
            if (tree == null)
            {
                return false;
            }
            return true;
        }

        private MixSkillComposeHero GeneratelHeroCompose()
        {
            List<FieldObject> heros = new List<FieldObject>();
            foreach (var player in curDungeon.PcList)
            {
                if (!player.Value.IsDead)
                {
                    heros.Add(player.Value);
                }
            }
            foreach (var hero in curDungeon.HeroList)
            {
                if (!hero.Value.IsDead)
                {
                    heros.Add(hero.Value);
                }
            }

            if (heros.Count <= 1) return null;

            heros.Sort((left, right) =>
            {
                if (left.GetHeroId() <= right.GetHeroId())
                {
                    return -1;
                }
                return 1;
            });

            List<MixSkillComposeHero> candidate = new List<MixSkillComposeHero>();
            for (int i = 0; i < heros.Count; i++)
            {
                for (int j = i + 1; j < heros.Count; j++)
                {
                    int composeId;
                    if (MixSkillLibrary.TryGetHeroComposeId(heros[i].GetHeroId(), heros[j].GetHeroId(), out composeId))
                    {
                        candidate.Add(new MixSkillComposeHero(heros[i], heros[j], composeId));
                    }
                }
            }

            if (candidate.Count > 0)
            {
                int index = RAND.Range(0, candidate.Count - 1);
                MixSkillComposeHero composeHero = candidate[index];
                // 一半概率前后置换
                if (RAND.Range(1, 10000) <= 5000)
                {
                    return composeHero;
                }
                else
                {
                    return new MixSkillComposeHero(composeHero.Hero_2, composeHero.Hero_1, composeHero.ComposeId);
                }
            }

            return null;
        }

        public void Update(float dt)
        {
            if (curDungeon == null || curDungeon.State != DungeonState.Started)
            {
                return;
            }
            switch (state)
            {
                case MixSkillState.Stopped:
                    CheckInStoppedState(dt);
                    break;
                case MixSkillState.Started:
                    CheckInStartedState(dt);
                    break;
                default:
                    break;
            }

        }

        // 在stop状态下，更新时间相关参数，判断是否可以刷出融合技树
        private void CheckInStoppedState(float dt)
        {
            // 副本刚开始，未到触发时间
            if (startedTimeLeft > 0)
            {
                startedTimeLeft -= dt;
                return;
            }
            // 本次触发的次数达到上限
            if (startedCount >= model.L)
            {
                return;
            }

            // 距离上次释放经历的时间增加
            lastStopedElapsed += dt;

            // 如果起效过 需要检查cd时间
            if (startedCount > 0 && cdTimeLeft > 0)
            {
                cdTimeLeft -= dt;
                return;
            }

            // 下次尝试刷出融合技剩余时间
            if (checkProbabilityTimeLeft > 0)
            {
                checkProbabilityTimeLeft -= dt;
                return;
            }

            // 时间限制检查通过 看概率能否触发
            // 重置下次检查概率时间
            checkProbabilityTimeLeft = 1;
            if (CheckProbability() && GenerateMixSkill())
            {
                GoToStartState();
            }
        }

        private void GoToStopState()
        {
            state = MixSkillState.Stopped;
            cdTimeLeft = model.F;
            lastStopedElapsed = 0;
            checkProbabilityTimeLeft = 1;
            tree = null;
            heroCompose = null;
            // 通知客户端 关闭融合技面板
            BroadcastTreeInfo();
        }

        private void GoToStartState()
        {
            state = MixSkillState.Started;
            startedCount++;

            // 计时器重置
            enableTimeLeft = model.EnableLeftTime;
            BroadcastTreeInfo();
        }

        private bool CheckProbability()
        {
            int p = 0;
            if (startedCount == 0)
            {
                p = (int)((lastStopedElapsed) * model.K);
            }
            else
            {
                int tmp = Math.Max(startedCount - model.L + 1, 0);
                p = (int)((lastStopedElapsed - model.F) * model.K) - tmp * 100;
            }
            return RAND.Range(1, 100) <= p;
        }

        private void CheckInStartedState(float dt)
        {
            enableTimeLeft -= dt;
            if (enableTimeLeft <= 0 || heroCompose.Hero_1.IsDead)
            {
                GoToStopState();
                return;
            }
        }


        public void CheckEnableMixSkill(int jobValue)
        {
            if (state != MixSkillState.Started || tree == null) return;

            // 成功点亮 则重置点亮倒计时
            MixSkillNode enableNode = tree.EnbaleNodeByValue(tree.Root, jobValue);
            if (enableNode != null)
            {
                BroadcastTreeInfo();
                enableTimeLeft = model.EnableLeftTime;
                // 如果有叶子节点被点亮 则可以释放
                if (enableNode.IsLeaf())
                {
                    // 重新进入关闭状态
                    MixSkillEffect(enableNode);
                    GoToStopState();
                }
            }
        }

        public MSG_ZGC_MIX_SKILL GenerateMixSkillMsg()
        {
            MSG_ZGC_MIX_SKILL msg = new MSG_ZGC_MIX_SKILL();
            if (heroCompose == null || tree == null || state != MixSkillState.Started)
            {
                return msg;
            }

            msg.InstanceId1 = heroCompose.Hero_1.InstanceId;
            msg.InstanceId2 = heroCompose.Hero_2.InstanceId;
            msg.ComposeId = heroCompose.ComposeId;
            msg.LeftTime = (int)(enableTimeLeft * 1000);
            msg.TotalTime = (int)(model.EnableLeftTime * 1000);
            foreach (var node in tree.ToList)
            {
                MSG_ZGC_MIX_SKILL_NODE nodeMsg = new MSG_ZGC_MIX_SKILL_NODE();
                nodeMsg.Job = node.Value;
                nodeMsg.Enabled = node.Enabled;
                BuffModel buffModel = BuffLibrary.GetBuffModel(node.BuffId);
                if (buffModel != null)
                {
                    nodeMsg.BuffId = buffModel.Id;
                }
                msg.Tree.Add(nodeMsg);
            }
            return msg;
        }

        private void BroadcastTreeInfo()
        {
            MSG_ZGC_MIX_SKILL msg = GenerateMixSkillMsg();
            curDungeon.BroadCast(msg);
        }

        private void MixSkillEffect(MixSkillNode leaf)
        {
            if (leaf == null || heroCompose == null || heroCompose.Hero_1 == null)
            {
                return;
            }

            FieldObject caster = heroCompose.Hero_1;
            if (caster.IsDead)
            {
                return;
            }

            int casterLevel = 1;
            if (caster.IsHero)
            {
                Hero hero = (caster as Hero);
                casterLevel = hero.Level;
            }
            else if (caster.IsPlayer)
            {
                PlayerChar player = caster as PlayerChar;
                HeroInfo heroInfo = player.HeroMng.GetHeroInfo(player.HeroId);
                casterLevel = heroInfo.Level;
            }
            else if (caster.IsRobot)
            {
                Robot robot = caster as Robot;
                HeroInfo heroInfo = robot.Info;
                if (heroInfo != null)
                {
                    casterLevel = heroInfo.Level;
                }
            }

            MSG_ZGC_MIX_SKILL_EFFECT notify = new MSG_ZGC_MIX_SKILL_EFFECT();
            notify.InstanceId1 = heroCompose.Hero_1.InstanceId;
            notify.InstanceId2 = heroCompose.Hero_2.InstanceId;
            notify.ComposeId = heroCompose.ComposeId;
            curDungeon.BroadCast(notify);

            int damage = MixSkillDamage.CalcDamage(casterLevel);
            List<FieldObject> targetList = new List<FieldObject>();
            caster.GetEnemyInSplash(caster, SplashType.Map, caster.Position, new Vec2(0, 0), 999, 0, 0, targetList, 999);
            foreach (var target in targetList)
            {
                bool immune = false;
                target.OnHit(caster, DamageType.MixSKill, damage, ref immune);
                target.AddBuff(caster, leaf.BuffId, casterLevel);
            }
        }

    }
}