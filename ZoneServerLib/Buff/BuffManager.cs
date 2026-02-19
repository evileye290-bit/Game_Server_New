using CommonUtility;
using EpPathFinding;
using Logger;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class BuffManager
    {
        private FieldObject owner;
        private List<BaseBuff> buffList;
        private List<BaseBuff> removeBuffList;
        private List<BaseBuff> addBuffList;

        private List<DelayBuff> delayBuffList;
        private List<DelayBuff> removeDelayBuffList;

        // buffList中，该BuffType叠加的次数，如果为0则删除掉
        private Dictionary<BuffType, int> overlayList;
        public BuffManager(FieldObject owner)
        {
            this.owner = owner;
            buffList = new List<BaseBuff>();
            overlayList = new Dictionary<BuffType, int>();
            removeBuffList = new List<BaseBuff>();
            addBuffList = new List<BaseBuff>();

            delayBuffList = new List<DelayBuff>();
            removeDelayBuffList = new List<DelayBuff>();
        }

        public void Update(float deltaTime)
        {
            foreach (var buff in buffList)
            {
                buff.OnUpdate(deltaTime);
                if (buff.IsEnd)
                {
                    removeBuffList.Add(buff);
                }
            }

            if (removeBuffList.Count > 0)
            {
                List<BaseBuff> tempList = removeBuffList;
                removeBuffList = new List<BaseBuff>();
                foreach (var buff in tempList)
                {
                    if (buffList.Remove(buff))
                    {
                        UpdateOvlayerCount(buff.Model, -1);

                        buff.OnEnd();
                        owner.BroadcastRemoveBuff(buff.Model);
                    }
                }
                //removeBuffList.Clear();
            }

            if (addBuffList.Count > 0)
            {
                List<BaseBuff> tempList = new List<BaseBuff>(addBuffList);

                foreach (var buff in tempList)
                {
                    addBuffList.Remove(buff);

                    buffList.Add(buff);
                    owner.BroadcastAddBuff(buff.Model);
                    UpdateOvlayerCount(buff.Model, 1);
                    buff.OnStart();

                    if (buffList.Count > 200)
                    {
                        Log.Warn($"player {owner.Uid} fieldobgect {owner.FieldObjectType} id {owner.GetHeroId()} buff count error : add buff id {buff.Model.Id} current buff count {buffList.Count}");
                        break;
                    }
                }

                //不能用clear如果当前帧 buff.OnStart(); 出发了加buff，则增加的buff也会被添加到addBuffList，如果clear则会导致把下一帧要增加的清除了
                //最后在清除，防止在templistbuff生效的时候触发addbuff导致 BuffOverlayType = ReplaceById = 4, 重复叠加
                //addBuffList.Clear();
            }

            // 延迟buff 到时间则添加
            foreach (var buff in delayBuffList)
            {
                if(buff.Check(deltaTime))
                {
                    removeDelayBuffList.Add(buff);
                    AddBuff(buff.Caster, buff.BuffId, buff.SkillLevel);
                }
            }
            if (removeDelayBuffList.Count > 0)
            {
                foreach (var buff in removeDelayBuffList)
                {
                    delayBuffList.Remove(buff);
                }
                removeDelayBuffList.Clear();
            }
        }

        public void Stop()
        {
            foreach(var buff in buffList)
            {
                buff.OnEnd();
                owner.BroadcastRemoveBuff(buff.Model);
                UpdateOvlayerCount(buff.Model, -1);
            }
            
            buffList.Clear();
            overlayList.Clear();
            removeBuffList.Clear();
            addBuffList.Clear();
            delayBuffList.Clear();
            removeDelayBuffList.Clear();
        }

        public void AddBuff(FieldObject caster, int buffId, int skillLevel, int pileCount = 1)
        {
            Log.Debug($"caster {caster.InstanceId} addBuff {buffId} to {owner.InstanceId}");
            BuffModel model = BuffLibrary.GetBuffModel(buffId);
            if (model == null)
            {
                return;
            }

            switch (model.SpecConType)
            {
                case BuffSpecConType.LessHpRate:
                    if (owner.GetHpRate() * 10000 > model.SpecParam)
                    {
                        return;
                    }
                    break;
                case BuffSpecConType.GreaterHpRate:
                    if (owner.GetHpRate() * 10000 < model.SpecParam)
                    {
                        return;
                    }
                    break;
                default:
                    break;
            }

            if (model.Debuff && CanRefuseDebuff())
            {
#if DEBUG
                Log.Warn($"instance {owner.InstanceId}  refuse buff {model.Id}");
#endif
                return;
            }

            // 对于控制类buff 尝试拒绝
            if (model.ControlledBuff)
            {
                if (CanRefuseControlledBuff(model.BuffType))
                {
#if DEBUG
                    Log.Warn($"instance {owner.InstanceId}  refuse controlle buff {model.Id}");
#endif
                    return;
                }
            }

            switch (model.OverlayType)
            {
                case BuffOverlayType.NoEffect:
                    // 存在同类型buff，则不再添加
                    if (InBuffState(model.BuffType))
                    {
                        return;
                    }
                    break;
                case BuffOverlayType.ReplaceByType:
                    // 替代所有同类型的buff
                    RemoveBuffsByType(model.BuffType);
                    break;
                case BuffOverlayType.Add:
                    // 直接添加，与已存在的buff无任何关系
                    break;
                case BuffOverlayType.ReplaceById:
                    RemoveBuffsById(buffId);

                    //已经在待加入的队列中了，就不在重复添加，防止移除相同id的buff，在待加入列表中而导致的重复添加
                    if (InAddBuffList(buffId)) return;
                    break;
                case BuffOverlayType.NotAddById:
                    if (HaveBuff(model.Id)) return;
                    break;
                case BuffOverlayType.PileById:
                    {
                        BaseBuff baseBuff = GetBuff(buffId);
                        if (baseBuff == null) break;

                        //直接叠加
                        baseBuff.AddPileNum(1);
                        return;
                    }
                case BuffOverlayType.ResetTimeById:
                    {
                        BaseBuff baseBuff = GetBuff(buffId);
                        if (baseBuff != null)
                        {
                            baseBuff.ResetTime();
                            return;
                        }
                    }
                    break;

            }

            BaseBuff buff = BuffFactory.CreateBuff(caster, owner, skillLevel, model);
            if (buff == null)
            {
                return;
            }

            //可叠加buff，初始层数
            if (model.OverlayType == BuffOverlayType.PileById)
            {
                buff.SetPileNum(pileCount);
            }

            if (addBuffList.Count < 100)
            {
                addBuffList.Add(buff);
            }
            else
            {
                Log.Warn($"player {owner.Uid} fieldobgect {owner.FieldObjectType} id {owner.GetHeroId()} buff count error : add buff id {buff.Model.Id} add buff count {addBuffList.Count}");
            }
            //buffList.Add(buff);
            //owner.BroadcastAddBuff(model);
            //UpdateOvlayerCount(model, 1);
            //buff.OnStart();

#if DEBUG
            if (buffId == 10021141)
            {
                Log.Warn($"caster {caster?.GetHeroId()} cast buff {buffId} to {owner.GetHeroId()}");
            }
#endif
        }

        public bool InAddBuffList(int buffId)
        {
            return addBuffList.FirstOrDefault(x => x.Id == buffId) != null;
        }

        public bool HaveBuff(int id)
        {
            return buffList.FirstOrDefault(x => x.Id == id) != null || addBuffList.FirstOrDefault(x => x.Id == id) != null;
        }

        public bool HaveDeBuff()
        {
            return buffList.FirstOrDefault(x => x.Debuff && x.CleanUp) != null || addBuffList.FirstOrDefault(x => x.Debuff && x.CleanUp) != null;
        }

        public void AddBuffDelay(FieldObject caster, int buffId, int skillLevel, float delayTime)
        {
            Logger.Log.Debug($"caster {caster.InstanceId} addBuff {buffId} delay to {owner.InstanceId}");
            DelayBuff delayBuff = new DelayBuff(caster, buffId, skillLevel, delayTime);

            if (delayBuffList.Count < 100)
            {
                delayBuffList.Add(delayBuff);
            }
            else
            {
                Log.Warn($"player {owner.Uid} fieldobgect {owner.FieldObjectType} id {owner.GetHeroId()} buff count error : add buff id {buffId} add delay buff count {delayBuffList.Count}");
            }
        }

        private void UpdateOvlayerCount(BuffModel model, int count)
        {
            if (count == 0) return;
            int originCount = 0;
            if (overlayList.TryGetValue(model.BuffType, out originCount))
            {
                originCount += count;
                if (originCount <= 0)
                {
                    overlayList.Remove(model.BuffType);
                }
                else
                {
                    overlayList[model.BuffType] = originCount;
                }
            }
            else
            {
                overlayList.Add(model.BuffType, count);
            }
        }

        public void RemoveBuffsByType(BuffType buffType)
        {
            foreach (var buff in buffList)
            {
                if (buff.BuffType == buffType && buff.RefuseRepalceType < BuffRefuseReplace.RefuseByType && !buff.IsEnd)
                {
                    RemoveBuff(buff);
                }
            }
        }

        public void RemoveBuffsById(int id)
        {
            foreach(var buff in buffList)
            {
                if (buff.Id == id && buff.RefuseRepalceType < BuffRefuseReplace.RefuseById && !buff.IsEnd)
                {
                    RemoveBuff(buff);
                }
            }
        }

        public void RemoveBuffsByIdAndInstanceId(int id, int instanceId)
        {
            foreach (var buff in buffList)
            {
                if (buff.Id == id  && buff.Caster.InstanceId == instanceId && buff.RefuseRepalceType < BuffRefuseReplace.RefuseById && !buff.IsEnd)
                {
                    RemoveBuff(buff);
                }
            }
        }

        public bool InBuffState(BuffType buffType)
        {
            int count = 0;
            overlayList.TryGetValue(buffType, out count);
            return count > 0;
        }

        public int GetBuffTotal_M(BuffType buffType)
        {
            int m = 0;
            foreach (var buff in buffList)
            {
                if (buff.BuffType == buffType && !buff.IsEnd)
                {
                    m += buff.M;
                }
            }
            return m;
        }

        public List<BaseBuff> GetBuffsByType(BuffType buffType)
        {
            List<BaseBuff> list = new List<BaseBuff>();
            foreach (var buff in buffList)
            {
                if (buff.BuffType == buffType && !buff.IsEnd)
                {
                    list.Add(buff);
                }
            }
            return list;
        }

        public void DoSpecLogic(BuffType buffType, object param)
        {
            foreach (var buff in buffList)
            {
                if (buff.BuffType == buffType && !buff.IsEnd)
                {
                    buff.SpecLogic(param);
                }
            }
        }

        public bool CanRefuseDebuff()
        {
            if(InBuffState(BuffType.Invincible) || InBuffState(BuffType.IgnoreDebuff))
            {
#if DEBUG
                Log.Warn($"instance {owner.InstanceId} hero {owner.GetHeroId()} in buff state Invincible refuse buff");
#endif
                return true;
            }

            if (!InBuffState(BuffType.RefuseDebuff))
            {
                return false;
            }

            foreach (var buff in buffList)
            {
                if (buff.BuffType == BuffType.RefuseDebuff && !buff.IsEnd)
                {
                    if(buff.ProbabilityHappened())
                    {
#if DEBUG
                        Log.Warn($"instance {owner.InstanceId} hero {owner.GetHeroId()} in buff state RefuseDebuff buff id {buff.Id} refuse buff");
#endif
                        owner.DispatchRefuseDebuffMsg();
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CanRefuseControlledBuff(BuffType buffType)
        {
            if (InBuffState(BuffType.IgnoreControl))
            {
#if DEBUG
                Log.Warn($"instance {owner.InstanceId} hero {owner.GetHeroId()} in IgnoreControl refuse buff");
#endif
                return true; //免疫控制
            }
        

            if (owner.InRealBody && BuffType.Dizzy == buffType)//FIXME:目前，这里基于武魂真身是无敌状态（眩晕）
            {
#if DEBUG
                Log.Warn($"instance {owner.InstanceId} hero {owner.GetHeroId()} in InRealBody refuse buff");
#endif
                return true;
            }

            if (!InBuffState(BuffType.DeControlledBuff)) return false;//概率免疫控制


            foreach (var buff in buffList)
            {
                if (buff.BuffType == BuffType.DeControlledBuff && !buff.IsEnd)
                {
                    if (buff.ProbabilityHappened())
                    {
#if DEBUG
                        Log.Warn($"instance {owner.InstanceId} hero {owner.GetHeroId()} in DeControlledBuff refuse buff");
#endif
                        return true;
                    }
                }
            }

            int probability = owner.GetRefuseControlledNature(buffType);
            if (probability > 0 && (probability >= 10000 || RAND.Range(1, 10000) <= probability))
            {
#if DEBUG
                Log.Warn($"instance {owner.InstanceId} hero {owner.GetHeroId()} in nature PRO_REFUSE_DIZZY refuse buff");
#endif
                return true;
            }
            return false;
        }

        public void CleanAllBuff()
        {
            foreach (var buff in buffList)
            {
                if (!buff.Debuff && buff.CleanUp)
                {
                    RemoveBuff(buff);
                }
            }
        }

        public void CleanAllDebuff()
        {
            foreach(var buff in buffList)
            {
                if(buff.Debuff && buff.CleanUp)
                {
                    RemoveBuff(buff);
                }
            }
        }

        public void CleanAllDispelBuff()
        {
            foreach (var buff in buffList)
            {
                if (buff.Dispel)
                {
                    RemoveBuff(buff);
                }
            }
        }

        public BaseBuff GetBuff(int buffId)
        {
            return buffList.FirstOrDefault(x => x.Id == buffId && !x.IsEnd);
        }

        public BaseBuff GetBuffByType(int type)
        {
            return GetOneBuffByType((BuffType)type);
        }

        
        public BaseBuff GetOneBuffByType(BuffType buffType)
        {
            return buffList.FirstOrDefault(x => x.BuffType == buffType && !x.IsEnd);
        }
        
        public void EnhanceDebuffBuffTime(float time)
        {
            foreach (var buff in buffList)
            {
                if (buff.Debuff)
                {
                    buff.AddTime(time);
                }
            }
        }

        public void EnhanceTypefBuffTime(BuffType buffType, float time)
        {
            buffList.ForEach(x =>
            {
                if (x.BuffType == buffType && !x.IsEnd) x.AddTime(time);
            });
        }

        public bool InControlledBuff()
        {
            foreach(var buff in buffList)
            {
                if(buff.ControlledBuff && !buff.IsEnd)
                {
                    return true;
                }
            }
            return false;
        }

        public void RemoveBuff(BaseBuff buff)
        {
            if (removeBuffList.Contains(buff)) return;

            removeBuffList.Add(buff);
        }

        public void RemoveRandomDebuff()
        {
            List<BaseBuff> debuffList = new List<BaseBuff>();
            foreach(var buff in buffList)
            {
                if(buff.Debuff && !buff.IsEnd && buff.CleanUp)
                {
                    debuffList.Add(buff);
                }
            }
            if(debuffList.Count == 0)
            {
                return;
            }
            int index = RAND.Range(0, debuffList.Count - 1);
            RemoveBuff(debuffList[index]);
        }

        public void RemoveRandomBuff()
        {
            List<BaseBuff> debuffList = new List<BaseBuff>();
            foreach (var buff in buffList)
            {
                if (!buff.Debuff && !buff.IsEnd&& buff.Dispel)
                {
                    debuffList.Add(buff);
                }
            }
            if (debuffList.Count == 0)
            {
                return;
            }
            int index = RAND.Range(0, debuffList.Count - 1);
            RemoveBuff(debuffList[index]);
        }

        public List<int> GetBuffIds()
        {
            List<int> list = new List<int>();
            foreach (var buff in buffList)
            {
                list.Add(buff.Id);
            }
            return list;
        }

        public List<BaseBuff> GetBuffList()
        {
            return buffList;
        }

        public int GetDebuffCount()
        {
            return buffList.Where(x => x.Model.Debuff).Count();
        }

        public int GetCanCleanUpDebuffCount()
        {
            return buffList.Where(x => x.Model.Debuff && x.Model.CleanUp).Count();
        }

        public int GetPoisonBuffCount()
        {
            return buffList.Where(x => x.BuffType == BuffType.Poison || x.BuffType == BuffType.PoisonDGB).Count();
        }
    }
}
