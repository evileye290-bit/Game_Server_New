using DBUtility;
using Logger;
using Message.Global.Protocol.GZ;
using Message.IdGenerator;
using Message.Zone.Protocol.ZG;
using ServerFrame;
using ServerModels;
using ServerShared;
using System.IO;
using System.Linq;

namespace ZoneServerLib
{
    public partial class GlobalServer : BaseGlobalServer
    {
        private ZoneServerApi Api
        { get { return (ZoneServerApi)api; } }

        public GlobalServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_GZ_SHUTDOWN_ZONE>.Value, OnResponse_ShutDown);
            AddResponser(Id<MSG_GZ_GM_CHARACTER_INFO>.Value, OnResponse_GMCharacterInfo);
            AddResponser(Id<MSG_GZ_GM_HERO_LIST>.Value, OnResponse_GMHeroList);
            AddResponser(Id<MSG_GZ_REBATE_UPDATE>.Value, OnResponse_RebateUpdate);

            //ResponserEnd 
        }

        private void OnResponse_ShutDown(MemoryStream stream, int uid = 0)
        {
            Log.Warn("global request shutdown zone");
            CONST.ALARM_OPEN = false;
            Api.State = ServerState.Stopping;
            Api.StoppingTime = ZoneServerApi.now.AddMinutes(1);
        }


        public void OnResponse_GMCharacterInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GZ_GM_CHARACTER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GZ_GM_CHARACTER_INFO>(stream);
            Log.Write("global gm request player {0} info", msg.Uid);
            MSG_ZG_GM_CHARACTER_INFO response = new MSG_ZG_GM_CHARACTER_INFO();
            response.Uid = msg.Uid;
            response.Name = msg.Name;
            response.CustomUid = msg.CustomUid;
            QueryGMCharacterBase queryBase = new QueryGMCharacterBase(msg.Uid, msg.Name);
            Api.GameDBPool.Call(queryBase, ret =>
            {
                if (ret == null)
                {
                    response.Uid = 0;
                    response.Name = string.Empty;
                    Write(response);
                    return;
                }
                response.Uid = queryBase.charAllInfo.uid;
                response.Name = queryBase.charAllInfo.name;
                response.Sex = queryBase.charAllInfo.sex;

                response.AccountName = queryBase.charAllInfo.accountName;
                response.Level = queryBase.charAllInfo.level;
                response.Camp = queryBase.charAllInfo.camp;
                response.BattlePower = queryBase.charAllInfo.battlePower;
                response.TimeCreated = queryBase.charAllInfo.timeCreated;
                response.LastLoginTime = queryBase.charAllInfo.lastLoginTime;
                response.LastOfflineTime = queryBase.charAllInfo.lastOfflineTime;
                response.FreezeState = queryBase.charAllInfo.freezeState;
                response.FreezeTime = queryBase.charAllInfo.freezeTime;
                response.FreezeReason = queryBase.charAllInfo.freezeReason;
                response.SilenceTime = queryBase.charAllInfo.silenceTime;
                response.SilenceReason = queryBase.charAllInfo.silenceReason;

                response.Exp = queryBase.charAllInfo.exp;
                response.Gold = queryBase.charAllInfo.gold;
                response.Diamond = queryBase.charAllInfo.diamond;
                response.SoulCrystal = queryBase.charAllInfo.soulCrystal;
                response.SoulPower = queryBase.charAllInfo.soulPower;
                response.SoulDust = queryBase.charAllInfo.soulDust;
                response.SoulBreath = queryBase.charAllInfo.soulBreath;
                response.FriendlyHeart = queryBase.charAllInfo.friendlyHeart;
                response.ArenaCoin = queryBase.charAllInfo.arenaCoin;
                response.SecretAreaCoin = queryBase.charAllInfo.secretAreaCoin;
                response.SpaceTimePower = queryBase.charAllInfo.spaceTimePower;
                response.SmeltCoin = queryBase.charAllInfo.smeltCoin;
                response.AccumulateTotal = queryBase.charAllInfo.accumulateTotal;

                Write(response);
                return;

                // 后面其他信息待运营确定后再处理
                //string resourceTableName = "character_resource";
                //QueryGMCharacterResource queryResource = new QueryGMCharacterResource(msg.Uid, resourceTableName);
                //Api.GameDBPool.Call(queryResource, ret2 =>
                //{
                //    if ((int)ret2 <= 0)
                //    {
                //        Write(response);
                //        return;
                //    }
                //    response.Exp = queryResource.Exp;
                //    response.LadderScore = queryResource.LadderScore;
                //    response.Gold = queryResource.Gold;
                //    response.Diamond = queryResource.Diamond;

                //    OperateGMCharacter operate = new OperateGMCharacter(msg.Uid);
                //    Api.Redis.Call(operate, ret3 =>
                //    {
                //        if ((int)ret3 <= 0)
                //        {
                //            Write(response);
                //            return;
                //        }
                //        else
                //        {
                //            //没找到对应id的信息
                //            response.Online = operate.Online;
                //            response.FashionCount = operate.FashionCount;
                //            Write(response);
                //        }
                //    });
                //});
            });
        }

        public void OnResponse_GMHeroList(MemoryStream stream, int uid = 0)
        {
            MSG_GZ_GM_HERO_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GZ_GM_HERO_LIST>(stream);
            Log.Write("global gm request player {0} hero list", msg.Uid);
            MSG_ZG_GM_HERO_LIST response = new MSG_ZG_GM_HERO_LIST();
            response.Uid = msg.Uid;
            response.CustomUid = msg.CustomUid;
            QueryGMHeroList query = new QueryGMHeroList(msg.Uid);
            Api.GameDBPool.Call(query, ret =>
            {
                //if (ret == null)
                //{
                //    response.CustomUid = msg.CustomUid;
                //    response.Uid = 0;
                //    Write(response);
                //    return;
                //}
                foreach (var hero in query.HeroList.list)
                {
                    GM_HERO_INFO heroInfo = new GM_HERO_INFO();
                    heroInfo.Id = hero.id;
                    heroInfo.EquipIndex = hero.equipIndex;
                    heroInfo.State = hero.state;
                    heroInfo.Level = hero.level;
                    heroInfo.Exp = hero.exp;
                    heroInfo.AwakenLevel = hero.awakenLevel;
                    heroInfo.TitleLevel = hero.titleLevel;
                    heroInfo.StepsLevel = hero.stepsLevel;
                    heroInfo.Strength = hero.strength;
                    heroInfo.Physical = hero.physical;
                    heroInfo.Agility = hero.agility;
                    heroInfo.Outburst = hero.outburst;
                    heroInfo.IsGod = hero.isGod;
                    response.Heros.Add(heroInfo);
                }
                Write(response);
                return;

                // 后面其他信息待运营确定后再处理
                //string resourceTableName = "character_resource";
                //QueryGMCharacterResource queryResource = new QueryGMCharacterResource(msg.Uid, resourceTableName);
                //Api.GameDBPool.Call(queryResource, ret2 =>
                //{
                //    if ((int)ret2 <= 0)
                //    {
                //        Write(response);
                //        return;
                //    }
                //    response.Exp = queryResource.Exp;
                //    response.LadderScore = queryResource.LadderScore;
                //    response.Gold = queryResource.Gold;
                //    response.Diamond = queryResource.Diamond;

                //    OperateGMCharacter operate = new OperateGMCharacter(msg.Uid);
                //    Api.Redis.Call(operate, ret3 =>
                //    {
                //        if ((int)ret3 <= 0)
                //        {
                //            Write(response);
                //            return;
                //        }
                //        else
                //        {
                //            //没找到对应id的信息
                //            response.Online = operate.Online;
                //            response.FashionCount = operate.FashionCount;
                //            Write(response);
                //        }
                //    });
                //});
            });
        }

        public void OnResponse_RebateUpdate(MemoryStream stream, int uid = 0)
        {
            MSG_GZ_REBATE_UPDATE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GZ_REBATE_UPDATE>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null)
            {
                Log.Error($"RebateUpdate error not find player {msg.Uid} at zone {Api.SubId}");
                return;
            }

            player.RechargeRebateRewardFromGlobal(msg);
        }
    }
}