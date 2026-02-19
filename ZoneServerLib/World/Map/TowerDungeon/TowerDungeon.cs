using System;
using CommonUtility;
using EnumerateUtility;
using Logger;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    partial class TowerDungeon : DungeonMap
    {
        //外部传入 不可修改该集合内容
        private Dictionary<int, float> heroHpRatio = new Dictionary<int, float>();
        private Dictionary<int, float> monsterHpRatio = new Dictionary<int, float>();
        Dictionary<int, Dictionary<int, int>> heroSkillEnergy = new Dictionary<int, Dictionary<int, int>>();

        //传出内容，血量信息保存
        private Dictionary<int, float> heroHp = new Dictionary<int, float>();
        private Dictionary<int, float> monsterHp = new Dictionary<int, float>();

        private int thisPeriod;
        private bool checkedDead = false;
        private float checkDeadObjects = 9999f;
        private List<FieldObject> deadFieldObjects = new List<FieldObject>();

        public int MonsterHeroSkillLevel = 1;
        public int MonsterHeroYears = 1;
        public int MonsterHeroSoulRingCount = 1;
        public TowerDungeon(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
            //添加消息订阅处理保存死亡单位血量信息
            GetMessageDispatcher().AddListener(TriggerMessageType.Dead, OnFieldObjectDead);
        }
        
        private void OnFieldObjectDead(object field)
        {
            FieldObject fieldObject = field as FieldObject;
            if (fieldObject == null) return;
            switch (fieldObject.FieldObjectType)
            {
                case TYPE.HERO:
                    if (fieldObject.GetOwner() is PlayerChar)
                    {
                        heroHp[fieldObject.GetHeroId()] = 0;
                    }
                    else
                    {
                        Hero hero = fieldObject as Hero;
                        if (hero != null)
                        {
                            if (hero.IsMonsterHero && hero.MonGenerator != null)
                            {
                                monsterHp[hero.MonGenerator.Id] = 0;
                            }
                        }
                    }
                    break;
                case TYPE.MONSTER:
                    Monster monster = fieldObject as Monster;
                    monsterHp[monster.Generator.Id] = 0;
                    break;
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            
            if(checkedDead) return;
            
            if (deadFieldObjects.Count > 0)
            {
                checkDeadObjects -= dt;
                if (checkDeadObjects <= 0)
                {
                    checkedDead = true;
                    CheckDeadFieldObjects();
                }
            }
        }

        public override void CreateHero(Hero hero, bool add2Aoi = true)
        {
            if (hero == null) return;

            PlayerChar player = hero.Owner as PlayerChar;

            // 加到地图里
            AddHero(hero);

            if (hero.HeroInfo.SoulSkillLevel > MonsterHeroSkillLevel)
            {
                MonsterHeroSkillLevel = hero.HeroInfo.SoulSkillLevel;
            }

            player.SoulRingManager.GetAllEquipedSoulRings(hero.HeroInfo.Id)?.ForEach(soulRing =>
            {
                if (soulRing.Value.Year > MonsterHeroYears)
                {
                    MonsterHeroYears = soulRing.Value.Year;
                }
            });

            if (IsDungeon)
            {
                DungeonMap map = this as DungeonMap;
                DungeonModel model = map.DungeonModel;

                int pos = player.TowerManager.GetHeroPos(hero.HeroId);
                hero.CollisionPriority = HeroLibrary.GetHeroPosCollisions(pos);

                //设置位置，一定在aoi前
                Vec2 temp = HeroLibrary.GetHeroPos(pos);
                if (temp != null)
                {
                    temp = model.GetPosition4Count(1, temp);
                }

                hero.SetPosition(temp ?? hero.Position);

                hero.InitBaseBattleInfo();
            }

            if (heroHpRatio.ContainsKey(hero.HeroId))
            {
                long hp = (long)(hero.GetMaxHp() * heroHpRatio[hero.HeroId]);

                hero.SetNatureBaseValue(NatureType.PRO_HP, hp);
            }

            hero.AddToAoi();
            hero.BroadCastHp();

            if (HeroList.Count >= player.TowerManager.HeroPos.Count())
            {
                OnePlayerDone = true;//此时至少有一个玩家连同其hero加载完了
            }
        }

        public override Monster CreateMonster(int id, Vec2 position, BaseMonsterGen monGenerator, long hp)
        {
            float hpRatio = 1f;
            if (monsterHpRatio.ContainsKey(monGenerator.Id))
            {
                hpRatio = monsterHpRatio[monGenerator.Id];
            }
            MonsterModel monsterModel = MonsterLibrary.GetMonsterModel(id);
            if (monsterModel == null)
            {
                Log.Warn($"create monster {id} failed: no such model");
                return null;
            }

            Monster monster = new Monster(server);
            if (hpRatio == 0)
            {
                //手动触发怪物死亡消息，避免副本无法结算问题，只能在所有怪物生成完成之后再检测，否则当地一个怪上次已经死亡时候就会触发副本结算
                //map trigger
                monster.SetCurrMap(this);
                monster.SetInstanceId(TokenId);
                monster.SetMonsterGenerator(monGenerator);
                CacheDeadFieldObject(monster);
                return null;
            }

            monster.Init(TokenId, this, monsterModel, monGenerator);
            monster.SetGenPos(position);

            hp = (long)(monster.GetMaxHp() * hpRatio);
            monster.SetNatureBaseValue(NatureType.PRO_HP, hp);

            //添加buff
            MonsterAddBuff(monster);

            monster.AddToAoi();
            monsterAddList.Add(monster);
            if (IsDungeon)
            {
                DungeonMap dungeon = monster.CurrentMap as DungeonMap;
                if (dungeon != null && dungeon.State > DungeonState.Open)
                {
                    monster.StartFighting();
                }
            }
            return monster;
        }

        public override Hero CreateMonsterHero(int id, Vec2 position, BaseMonsterGen monGenerator, long hp)
        {
            float hpRatio = 1f;

            MonsterHeroModel monsterModel = MonsterLibrary.GetMonsterHeroModel(id);
            if (monsterModel == null)
            {
                Log.Warn($"create monster hero {id} failed: no such model");
                return null;
            }

            if (monsterHpRatio.ContainsKey(monGenerator.Id))
            {
                hpRatio = monsterHpRatio[monGenerator.Id];
            }

            HeroInfo info = InitFromRobotInfo(monsterModel);

            Robot robot = GetMonsterRobot(info);
            Hero hero = robot.NewHero(server, robot, info);
            hero.InitMonsterHero(monsterModel, monGenerator);
            hero.GenAngle = monGenerator.Model.GenAngle;
            hero.CollisionPriority = monsterModel.PosCollision;
            hero.MonsterHeroSoulRingCount = MonsterHeroSoulRingCount;
            hero.MonsterHeroSkillLevel = MonsterHeroSkillLevel / 10 + 1;

            if (hpRatio == 0)
            {
                //手动触发怪物死亡消息，避免副本无法结算问题
                //map trigger
                hero.SetCurrMap(this);
                hero.SetInstanceId(TokenId);
                CacheDeadFieldObject(hero);
                return null;
            }

            List<int> years = new List<int>();
            for (int i = 0; i < hero.MonsterHeroSoulRingCount; i++)
            {
                years.Add(MonsterHeroYears);
            }
            hero.MonsterHeroYears = years;
            hero.SetPosition(position);


            hp = (long)(hero.GetMaxHp() * hpRatio);
            hero.SetNatureBaseValue(NatureType.PRO_HP, hp);

            //添加buff
            MonsterAddBuff(hero);

            // 加到地图里
            AddHero(hero);

            hero.InitBaseBattleInfo();
            hero.AddToAoi();
            //hero.BroadCastHp();
            //hero.StartFighting();

            //monsterAddList.Add(monster);
            if (IsDungeon)
            {
                DungeonMap dungeon = hero.CurrentMap as DungeonMap;
                if (dungeon != null && dungeon.State > DungeonState.Open)
                {
                    hero.StartFighting();
                }
            }

            return hero;
        }

        public void SetPeriod(int period)
        {
            thisPeriod = period;
        }

        public void SetHeroHp(Dictionary<int, float> heroHpRatio)
        {
            this.heroHpRatio = heroHpRatio;
        }

        public void SetMonsterHp(Dictionary<int, float> monsterHpRatio)
        {
            this.monsterHpRatio = monsterHpRatio;
        }

        public void SetSkillEnergy(Dictionary<int, Dictionary<int, int>> heroSkillEnergy)
        {
            this.heroSkillEnergy = heroSkillEnergy;
        }

        protected override void Start()
        {
            base.Start();

            InitTowerHeroStatus();
        }

        private void CheckDeadFieldObjects()
        {
            deadFieldObjects.ForEach(x=> DispatchFieldObjectDeadMsg(x));
        }

        private void CacheDeadFieldObject(FieldObject fieldObject)
        {
            checkDeadObjects = 2f;
            deadFieldObjects.Add(fieldObject);
        }

        public void AddSkillEnergy(Hero hero)
        {
            Dictionary<int, int> skillEnergy;
            if (heroSkillEnergy.TryGetValue(hero.HeroId, out skillEnergy))
            {
                skillEnergy.ForEach(x => hero.SkillManager.AddEnergy(x.Key, x.Value));
            }
        }

        protected override void SaveHeroAndMonsterInfo()
        {
            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player == null) return;

            //手动结束不同步血量
            if (isQuitDungeon) return;

            UpdateBattleItemInfo(player);
        }

        protected override void Success()
        {
            DoReward();
            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player != null)
            {
                RewardManager mng = GetFinalReward(player.Uid);

                //爬塔
                player?.TowerSuccess(mng, DungeonModel.Id, 0, thisPeriod);


                //副本类型任务计数
                PlayerAddTaskNum(player);

                //增加伙伴经验
                player.AddHeroExp(DungeonModel.HeroExp);

                //日志
                player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());

                //komoelog
                player.KomoeLogRecordPveFight(5, 1, DungeonModel.Id.ToString(), mng.RewardList, 1, GetFinishTime(), null, 0, towerBuffList.Count > 0 ? 1 : 0);
            }

            ResetReward();

        }

        protected override void Failed()
        {
            base.Failed();
            //日志
            int pointState = 2;
            if (isQuitDungeon)
            {
                pointState = 3;
            }
            PcList.Values.FirstOrDefault()?.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());

            //komoelog
            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player != null)
            {
                player.KomoeLogRecordPveFight(5, 1, DungeonModel.Id.ToString(), null, pointState, GetFinishTime(), null, 0, towerBuffList.Count > 0 ? 1 : 0);            
            }
        }

        private void UpdateBattleItemInfo(PlayerChar player)
        {
            bool isFailed = DungeonResult != DungeonResult.Success;

            //取消消息订阅
            GetMessageDispatcher().RemoveListener(TriggerMessageType.Dead, OnFieldObjectDead);

            //周期变动，血量信息不保存
            if (player.TowerManager.Period != this.thisPeriod) return;

            Dictionary<int, Dictionary<int, int>> tempHeroSkillEnergy = new Dictionary<int, Dictionary<int, int>>(); 

            long hp, maxHp;
            foreach (var kv in HeroList)
            {
                if (kv.Value.GetOwner() is PlayerChar)
                {
                    int heroId = kv.Value.HeroId;
                    maxHp = kv.Value.GetNatureValue(NatureType.PRO_MAX_HP);

                    Dictionary<int, int> skillEnergy;
                    if (!tempHeroSkillEnergy.TryGetValue(heroId, out skillEnergy))
                    {
                        skillEnergy = new Dictionary<int, int>();
                        tempHeroSkillEnergy[heroId] = skillEnergy;
                    }

                    //战斗超时我方算死亡
                    if (kv.Value.IsDead || maxHp == 0 || isFailed)
                    {
                        heroHp[heroId] = 0f;
                    }
                    else
                    {
                        hp = kv.Value.GetNatureValue(NatureType.PRO_HP);
                        heroHp[heroId] = (float)Decimal.Round(new Decimal(hp * 1f / maxHp), 2);

                        foreach (var skill in kv.Value.SkillManager.SkillList)
                        {
                            if (skill.Value.Energy <= 0) continue;
                            skillEnergy.Add(skill.Key, skill.Value.Energy);
                        }
                    }
                }
                else
                {
                    if (kv.Value.IsMonsterHero)
                    {
                        maxHp = kv.Value.GetNatureValue(NatureType.PRO_MAX_HP);
                        if (kv.Value.IsDead || maxHp == 0)
                        {
                            monsterHp[kv.Value.MonGenerator.Id] = 0f;
                        }
                        else
                        {
                            hp = kv.Value.GetNatureValue(NatureType.PRO_HP);
                            monsterHp[kv.Value.MonGenerator.Id] = (float)Decimal.Round(new Decimal(hp * 1f / maxHp), 2);
                        }
                    }
                }
            }

            if (DungeonResult != DungeonResult.Success)
            {
                foreach (var kv in MonsterList)
                {
                    maxHp = kv.Value.GetNatureValue(NatureType.PRO_MAX_HP);
                    if (kv.Value.IsDead || maxHp == 0)
                    {
                        monsterHp[kv.Value.Generator.Id] = 0f;
                    }
                    else
                    {
                        hp = kv.Value.GetNatureValue(NatureType.PRO_HP);
                        monsterHp[kv.Value.Generator.Id] = (float)Decimal.Round(new Decimal(hp * 1f / maxHp), 2);
                    }
                }
            }

            player.TowerManager.UpdateHeroSkillEnergy(tempHeroSkillEnergy);
            player.TowerManager.SetTowerHeroAndMonsterHP(heroHp, monsterHp);
            player.TowerManager.Owner.SendTowerHeroInfo();
        }
    }
}
