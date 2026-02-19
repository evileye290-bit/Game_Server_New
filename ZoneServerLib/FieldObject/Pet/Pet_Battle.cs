using CommonUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    partial class Pet
    {
        //public override bool IsEnemy(FieldObject target)
        //{
        //    return owner.IsEnemy(target);
        //}

        //public override bool IsAlly(FieldObject target)
        //{
        //    return owner.IsAlly(target);
        //}

        public bool InBattleField()
        {
            if (InBattle)
            {
                return true;
            }
            return false;
        }

        public bool FindTarget()
        {
            List<FieldObject> tempTargetList = new List<FieldObject>();
            FieldObject target = null;
            Vec2 targetPos = null;

            if (HateManager.Target == null)
            {
                GetEnemyInSplash(this, SplashType.Circle, Position, new Vec2(0, 0), hateRange, 0f, 0f, tempTargetList, 10, -1, true);
                if (tempTargetList.Count > 0)
                {
                    //FieldObject temp = null;
                    //float hateRatio = 0;
                    //foreach (var item in tempTargetList)
                    //{
                    //    if (item.HateRatio >= hateRatio)
                    //    {
                    //        temp = item;
                    //    }
                    //}
                    FieldObject temp = null;

                    float curMaxHateRatio = 0f;
                    float curMinLengthPow = float.MaxValue;
                    foreach (var item in tempTargetList)
                    {
                        float tempHateRatio = 0f;
                        float tempLengthPow = 0f;
                        if (temp == null)
                        {
                            temp = item;
                            curMaxHateRatio = item.HateRatio;
                            curMinLengthPow = (Owner.Position - item.Position).magnitudePower;
                            continue;
                        }

                        tempHateRatio = item.HateRatio;
                        tempLengthPow = (Owner.Position - item.Position).magnitudePower;
                        if (tempLengthPow < curMinLengthPow || (tempLengthPow == curMinLengthPow && tempHateRatio > curMaxHateRatio))
                        {
                            temp = item;
                            curMaxHateRatio = tempHateRatio;
                            curMinLengthPow = tempLengthPow;
                        }
                    }
                    HateManager.SetTarget(temp);
                }
            }

            Skill skill;
            if (SkillEngine == null)
            {
                return false;
            }
            if (SkillEngine.TryFetchOneSkill(out skill, out target, out targetPos))
            {
                // 有准备好的技能，且该技能满足释放条件
                return true;
            }
            return false;
        }

        private void BindTriggers()
        {
            foreach (var triggerId in petModel.Triggers)
            {
                BaseTrigger trigger = new TriggerInPet(this, triggerId);
                AddPetTrigger(trigger);
            }
        }

        public override void BindSkills()
        {
            foreach (int skillId in Skills)
            {
                SkillModel skillModel = SkillLibrary.GetSkillModel(skillId);
                if (skillModel == null)
                {
                    continue;
                }
                int skillLevel = PetInfo.GetInbornSkillLevel(skillId);
                SkillManager.Add(skillId, skillLevel);
            }
        }

        public override void StartFighting()
        {
            if (CurDungeon == null || InBattle) return;

            base.StartFighting();
            UpdateProSpd(NatureType.PRO_RUN_IN_BATTLE);
            InitAI();

            //检测存货单位
            DispatchAliveCountMessage();

            //#if DEBUG

            //            Log.Info($"--------------------------------------------------------------------------------------------------------------------------nature1-----instanceId :  {HeroId} instanceId {instanceId}");

            //            Log.Info1(JsonConvert.SerializeObject(GetNature().GetNatureList()));
            //            Log.Info("--------------------------------------------------------------------------------------------------------------------------nature2-----instanceId :" + instanceId);

            //            Log.Info("--------------------------------------------------------------------------------------------------------------------------trigger1-----instanceId :" + instanceId);
            //            Log.Info1("trigger count " + triggerManager.GetTriggerCount());
            //            Log.Info("--------------------------------------------------------------------------------------------------------------------------trigger2-----instanceId :" + instanceId);

            //            Log.Info("--------------------------------------------------------------------------------------------------------------------------skill1-----instanceId :" + instanceId);
            //            Log.Info1("skill level " + heroInfo.SoulSkillLevel);
            //            Log.Info("--------------------------------------------------------------------------------------------------------------------------skill2-----instanceId :" + instanceId);
            //#endif

            //战力压制
            //CurDungeon.CheckBattlePowerSuppress(this);
        }

        public bool CheckCollision(Vec2 pos)
        {
            foreach (var kv in currentMap.HeroList)
            {
                Hero hero = kv.Value;
                if (GetOwner() != hero.GetOwner())
                {
                    continue;
                }
                if (hero.CollisionPriority > CollisionPriority)
                {
                    //计算是否碰撞
                    if (Vec2.GetDistance(pos, hero.Position) < Radius + hero.Radius + PetLibrary.PetConfig.CollisionRadius)
                    {
                        Logger.Log.Debug($"{hero.HeroId} radius {hero.Radius} collision with {PetId} radius {Radius}");
                        return true;
                    }
                }
            }
            return false;
        }

        public Tuple<bool, Vec2> GetNonCollisionPos(FieldObject target, Vec2 pos, float skillDis, float deltaLength = 0.1f)
        {
            //float dis = target.Radius + HeroModel.Radius + skillDis;
            //int count = maxCount;
            int allCount = 0;
            float temp = Radius;

            temp = target.Radius + Radius + skillDis - deltaLength;

            Vec2 delta = Position - target.Position;
            delta = delta * temp / (float)delta.GetLength();
            float rad = 0;
            while (rad < 360f)
            {
                List<Vec2> availableVecs = new List<Vec2>();

                rad += 5f;
                allCount++;

                Vec2 tempPos = GetVec2FromTo(delta, rad) + target.Position;
                Vec2 tempPos1 = GetVec2FromTo(delta, -rad) + target.Position;
                if (!CheckCollision(tempPos))
                {
                    Logger.Log.Debug($"pet {InstanceId} get non collision pos for monster {target.InstanceId} with {allCount} allcount");
                    availableVecs.Add(tempPos);
                }
                else if (!CheckCollision(tempPos1))
                {
                    Logger.Log.Debug($"pet {InstanceId} get non collision pos for monster {target.InstanceId} with {allCount} allcount");
                    availableVecs.Add(tempPos1);
                }
                else
                {
                    Logger.Log.Debug($"pet {InstanceId} randomPos {tempPos1} collision for monster {target.InstanceId} with {allCount} allcount");
                    Logger.Log.Debug($"pet {InstanceId} randomPos {tempPos} collision for monster {target.InstanceId} with {allCount} allcount");
                }

                bool got = false;
                Vec2 ans = null;
                if (availableVecs.Count > 0)
                {
                    got = true;
                    ans = availableVecs[0];
                }
                if (got)
                {
                    return Tuple.Create(true, ans);
                }
                //return Tuple.Create(true, tempPos + target.Position);
            }
            Logger.Log.Debug($"pet {InstanceId} get non collision pos for monster {target.InstanceId} with {allCount} allcount in default");
            return Tuple.Create(false, Position);
        }

        private Vec2 GetVec2FromTo(Vec2 temp, double rad)
        {
            rad = rad * Math.PI / 180;
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);
            double x = temp.X * cos - temp.Y * sin;
            double y = temp.X * sin + temp.Y * cos;

            return new Vec2((float)x, (float)y);
        }

        public override void StopFighting()
        {
            base.StopFighting();
            ClearBasicBattleState();
        }
    }
}
