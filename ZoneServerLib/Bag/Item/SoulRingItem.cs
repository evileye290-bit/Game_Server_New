using CommonUtility;
using DataProperty;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System;
using ScriptFunctions;

namespace ZoneServerLib
{
    public class SoulRingItem : BaseItem
    {
        private SoulRingModel model;
        private SoulRingInfo soulLinkInfo;
        public SoulRingInfo SoulRingInfo
        {
            get { return this.soulLinkInfo; }
        }

        public SoulRingModel Model => model;
        public int MaxYear => model.MaxYear;
        public int modelId { get { return this.model.Id; } }
        public int EquipHeroId { get { return this.soulLinkInfo.EquipHeroId; } }
        public int AbsorbState { get { return this.soulLinkInfo.AbsorbState; } }

        public int Year { get { return this.soulLinkInfo.Year; } }
        public int Level { get { return this.soulLinkInfo.Level; } }
        public int IniLevel { get { return this.model.IniLevel; } }
        public int Position { get { return this.soulLinkInfo.Position; } }
        public int Element { get { return this.soulLinkInfo.Element; } }
        public bool Delete = false;
        public int SpecId { get { return model.SPAttrType; } }
        public Dictionary<int, int> AdditionalNatures { get { return this.soulLinkInfo.AdditionalNatures; } }

        public SoulRingItem(SoulRingInfo soulLinkInfo) : base(soulLinkInfo)
        {
            this.soulLinkInfo = soulLinkInfo;
            this.MainType = MainType.SoulRing;
            this.BindData(soulLinkInfo.TypeId);
        }

        public SoulRingItem(SoulRingModel model, SoulRingInfo soulLinkInfo) : base(soulLinkInfo)
        {
            this.model = model;
            this.soulLinkInfo = soulLinkInfo;
            this.MainType = MainType.SoulRing;
        }

        public SoulRingItem(SoulRingInfo soulLinkInfo, bool delete) : this(soulLinkInfo)
        {
            this.soulLinkInfo = soulLinkInfo.Clone();
            Delete = true;
            this.soulLinkInfo.Position = 0;
            this.soulLinkInfo.AbsorbState = -1;
        }

        public override bool BindData(int id)
        {
            this.model = SoulRingLibrary.GetSoulRingMode(id);
            if (this.model != null)
            {
                //foreach (var item in model.MainAttrTypes)
                //{

                //}

                return true;
            }
            else
            {
                Logger.Log.Warn($"have no this SoulRingModel model id {id}");
                return false;
            }
        }

        public MSG_ZGC_ITEM_SOULRING GenerateSyncMessage()
        {
            MSG_ZGC_ITEM_SOULRING syncMsg = new MSG_ZGC_ITEM_SOULRING()
            {
                UidHigh = this.Uid.GetHigh(),
                UidLow = this.Uid.GetLow(),
                Id = this.Id,
                PileNum = this.PileNum,
                Level = this.Level,
                Year = this.Year,
                EquipedHeroId = this.EquipHeroId,
                AbsorbState = this.AbsorbState,
                Position = this.Position,
                Delete = this.Delete,
                Element = this.Element
            };
            Dictionary<NatureType, long> dic = GetMainAttrs();
            syncMsg.Score = ScriptManager.BattlePower.CaculateItemScore2(dic);

            return syncMsg;
        }

        public MSG_ZGC_ITEM_SOULRING GenerateSyncShowMessage()
        {
            MSG_ZGC_ITEM_SOULRING syncMsg = new MSG_ZGC_ITEM_SOULRING()
            {
                Id = this.Id,
                Level = this.Level,
                Year = this.Year,
                EquipedHeroId = this.EquipHeroId,
                Position = this.Position,
                Element = Element,
            };
            Dictionary<NatureType, long> dic = GetMainAttrs();
            syncMsg.Score = ScriptManager.BattlePower.CaculateItemScore2(dic);
            return syncMsg;
        }


        /// <summary>
        /// 强化属性变更
        /// </summary>
        public void Enhance()
        {
            //强化&突破相关属性变化
            SetLevel(Level + 1) ;
        }

        public ZMZ_ITEM_SOULRING GenerateTransformMessage()
        {
            ZMZ_ITEM_SOULRING syncMsg = new ZMZ_ITEM_SOULRING()
            {
                Uid = this.Uid,
                Id = this.Id,
                PileNum = this.PileNum,
                Level = this.Level,
                Year = this.Year,
                EquipedHeroId = this.EquipHeroId,
                AbsorbState = this.AbsorbState,
                Position = this.Position,
                Element = this.Element,
            };

            return syncMsg;
        }


        internal void SetAbsorbState(SoulRingAbsorbState state)
        {
            SoulRingInfo.AbsorbState = (int)state;
        }

        internal void SetPosition(int pos)
        {
            soulLinkInfo.Position = pos;
        }

        internal void SetEquipHeroId(int heroId)
        {
            SoulRingInfo.EquipHeroId = heroId;
        }

        internal void SetLevel(int level)
        {
            SoulRingInfo.Level = level;
        }

        internal void SetElement(int elementId)
        {
            SoulRingInfo.Element = elementId;
        }

        ///// <summary>
        ///// 获取魂环基础主属性
        ///// </summary>
        ///// <returns></returns>
        //private Dictionary<NatureType,int> GetBaseMainAttrs()
        //{
        //    Dictionary<NatureType, int> narures = new Dictionary<NatureType, int>();

        //    BasicFloatNatureModel natureModel = SoulRingbrary.GetBasicNatureModel(model.Id);
        //    if (natureModel == null) return narures;

        //    Dictionary<NatureType, float> allNature = natureModel.ToDictionary();
        //    model.MainAttrTypes.ForEach((x)=> { if (allNature.ContainsKey(x)) narures.Add(x, (int)allNature[x]); });

        //    return narures;
        //}

        /// <summary>
        /// 获取魂环主属性
        /// </summary>
        /// <returns></returns>
        public Dictionary<NatureType, long>  GetMainAttrs(int addYearRatio = 0)
        {
            Dictionary<NatureType, long> narures = new Dictionary<NatureType, long>();

            NatureDataModel natureModel = SoulRingLibrary.GetBasicNatureModel(model.Id);

            int currentYear = SoulRingManager.GetAffterAddYear(Year, addYearRatio);

            NatureDataModel incrModel = SoulRingLibrary.GetBasicNatureIncrModel(Level);
            if (natureModel == null || incrModel == null) return narures;

            int growth = ScriptManager.SoulRing.GetNatureGrowthFactor(currentYear);

            narures = GetSoulRingBasicNatureList(natureModel, incrModel, growth);

            //Dictionary<NatureType, int> allNature = final.ToDictionary();
            //model.MainAttrTypes.ForEach((x)=>
            //{
            //    if (allNature.ContainsKey(x))
            //    {
            //        narures.Add(x, growth + allNature[x]);
            //    }
            //});

            return narures;
        }

        public Dictionary<NatureType, long> GetSoulRingBasicNatureList(NatureDataModel basicModel, NatureDataModel incrModel, int extraValue)
        {
            Dictionary<NatureType, long> NatureList = new Dictionary<NatureType, long>();

            if (basicModel != null && incrModel != null)
            {
                foreach (var basic in basicModel.NatureList)
                {
                    float incrValue;
                    if (incrModel.NatureList.TryGetValue(basic.Key, out incrValue))
                    {
                        NatureList[basic.Key] = (long)(basic.Value * (incrValue + extraValue));
                    }
                    else
                    {
                        NatureList[basic.Key] = (long)(basic.Value * extraValue);
                    }
                }
            }
            else if (basicModel != null)
            {
                foreach (var basic in basicModel.NatureList)
                {
                    NatureList[basic.Key] = (long)(basic.Value * extraValue);
                }
            }
            else if (incrModel != null)
            {
                foreach (var incr in incrModel.NatureList)
                {
                    NatureList[incr.Key] = (long)(incr.Value * extraValue);
                }
            }

            return NatureList;
        }

        public int GetMaxLevel()
        {
            return ScriptManager.SoulRing.GetMaxLevel(Year);
        }

        /// <summary>
        /// 获取魂环 附加属性
        /// </summary>
        /// <returns></returns>
        //public Dictionary<NatureType, long> GetUltAttrs()
        //{
        //    if (Year == model.MaxYear)
        //    {
        //        return model.UltAttrValue;
        //    }
        //    return null;
        //}

        public int GetAbsorbHeroLevel()
        {
            return ScriptManager.SoulRing.GetAbsorbHeroLevel(Year);
        }

        public SoulRingItem GenerateDeleteInfo(SoulRingInfo soulLinkInfo)
        {
            SoulRingItem item = new SoulRingItem(soulLinkInfo, true);
            return item;
        }

        public void CheckAddAdditionalNatures()
        {
            if (Model.AdditionalNature == null || Model.AdditionalNature.Length == 0)
            {
                return;
            }
            for (int i = 0; i < Model.AdditionalNature.Length; i++)
            {
                int yearLimit = SoulRingLibrary.SoulRingConfig.AdditionalNatureYearLimit[i];
                if (Year >= yearLimit)
                {
                    SoulRingAdditionalNatrueModel natureModel = SoulRingLibrary.GetSoulRingAdditionalNatrueModel(Model.AdditionalNature[i]);
                    if (natureModel != null)
                    {
                        soulLinkInfo.AdditionalNatures.Add(natureModel.NatureType, natureModel.Ratio);
                    }
                }
            }
        }
    }
}
