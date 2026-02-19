using System.IO;
using Message.IdGenerator;
using Message.Zone.Protocol.ZG;
using System.Web.Script.Serialization;
using ServerFrame;
using ServerModels;
using System.Collections.Generic;
using DBUtility;
using Message.Global.Protocol.GZ;
using EnumerateUtility;

namespace GlobalServerLib
{
    public partial class ZoneServer : FrontendServer
    {
        public GlobalServerApi Api
        { get { return (GlobalServerApi)api; } }

        public ZoneServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_ZG_GM_CHARACTER_INFO>.Value, OnResponse_GMCharacterInfo);
            AddResponser(Id<MSG_ZG_GM_HERO_LIST>.Value, OnResponse_GMHeroList);
            AddResponser(Id<MSG_ZG_REBATE_UPDATE>.Value, OnResponse_RebateUpdate);
        }
        
        public void OnResponse_GMCharacterInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZG_GM_CHARACTER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZG_GM_CHARACTER_INFO>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client == null)
            {
                return;
            }
            CharAllInfo response = new CharAllInfo();
            response.uid = msg.Uid;
            response.name = msg.Name;
            response.level = msg.Level;
            response.accountName = msg.AccountName;
            response.sex = msg.Sex;
            response.camp = msg.Camp;
            response.battlePower = msg.BattlePower;
            response.timeCreated = msg.TimeCreated;
            response.lastLoginTime = msg.LastLoginTime;
            response.lastOfflineTime = msg.LastOfflineTime;
            response.freezeState = msg.FreezeState;
            response.freezeTime = msg.FreezeTime;
            response.freezeReason = msg.FreezeReason;
            response.silenceTime = msg.SilenceTime;
            response.silenceReason = msg.SilenceReason;

            response.exp = msg.Exp;
            response.gold = msg.Gold;
            response.diamond = msg.Diamond;
            response.soulCrystal = msg.SoulCrystal;
            response.soulPower = msg.SoulPower;
            response.soulDust = msg.SoulDust;
            response.soulBreath = msg.SoulBreath;
            response.friendlyHeart = msg.FriendlyHeart;
            response.arenaCoin = msg.ArenaCoin;
            response.secretAreaCoin = msg.SecretAreaCoin;
            response.spaceTimePower = msg.SpaceTimePower;
            response.smeltCoin = msg.SmeltCoin;
            response.accumulateTotal = msg.AccumulateTotal;

            var jser = new JavaScriptSerializer();
            string json = jser.Serialize(response);
            client.WriteString(json);
        }

        public void OnResponse_GMHeroList(MemoryStream stream, int uid = 0)
        {
            MSG_ZG_GM_HERO_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZG_GM_HERO_LIST>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client == null)
            {
                return;
            }
            foreach (var item in msg.Heros)
            {

            }
            HeroList response = new HeroList();
            response.uid = msg.Uid;
            response.list = new List<Hero>();
            foreach (var heroInfo in msg.Heros)
            {
                Hero hero = new Hero();
                hero.id= heroInfo.Id;
                hero.equipIndex = heroInfo.EquipIndex;
                hero.state = heroInfo.State;
                hero.level = heroInfo.Level;
                hero.exp = heroInfo.Exp;
                hero.awakenLevel = heroInfo.AwakenLevel;
                hero.titleLevel = heroInfo.TitleLevel;
                hero.stepsLevel = heroInfo.StepsLevel;
                hero.strength = heroInfo.Strength;
                hero.physical = heroInfo.Physical;
                hero.agility = heroInfo.Agility;
                hero.outburst = heroInfo.Outburst;
                hero.isGod = heroInfo.IsGod;
                response.list.Add(hero);
            }
           
            var jser = new JavaScriptSerializer();
            string json = jser.Serialize(response);
            client.WriteString(json);
        }

        public void OnResponse_RebateUpdate(MemoryStream stream, int uid = 0)
        {
            MSG_ZG_REBATE_UPDATE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZG_REBATE_UPDATE>(stream);
            QueryLoadRebate query = new QueryLoadRebate(msg.Account, msg.Channel);

            MSG_GZ_REBATE_UPDATE response = new MSG_GZ_REBATE_UPDATE()
            {
                Account = msg.Account,
                Uid = msg.Uid,
                ModelId = msg.ModelId,
            };

            Api.AccountDBPool.Call(query, ret =>
            {
                if ((int)ret != 1)
                {
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }

                if (query.IsRebated)
                {
                    response.Result = (int)ErrorCode.RebateHadReward;
                    Write(response);
                    return;
                }

                Api.AccountDBPool.Call(new QueryUpdateRebateInfo(msg.Account, msg.Channel, Api.Now()), obj=>
                {
                    if ((int)obj == 0)
                    {
                        response.Result = (int)ErrorCode.Success;
                    }
                    else
                    { 
                        response.Result = (int)ErrorCode.Fail;
                    }
                    Write(response);
                });
            });
        }
    }
}