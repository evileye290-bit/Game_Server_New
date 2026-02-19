using CommonUtility;
using Message.Gate.Protocol.GateZ;
using System.IO;
using MessagePacker;
using Message.Gate.Protocol.GateC;
using Logger;
using EnumerateUtility;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        private void OnResponse_CastSkill(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CAST_SKILL msg = ProtobufHelper.Deserialize<MSG_GateZ_CAST_SKILL>(stream);
            Log.Write($"player {msg.Uid} cast skill {msg.SkillId}");
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null) return;

            MSG_ZGC_CAST_SKILL response = new MSG_ZGC_CAST_SKILL();
            response.SkillId = msg.SkillId;
            response.Result = (int)ErrorCode.Success;
            //Logger.Log.Write($"player {player.Uid} caster skill {msg.SkillId} target {msg.TargetId}");
            if(!player.CastSkill(msg.SkillId, new Vec2(msg.AngleX, msg.AngleY), new Vec2(msg.TargetPosX, msg.TargetPosY), msg.TargetId))
            {
                response.Result = (int)ErrorCode.Fail;
            }

#if DEBUG
            Log.WarnLine($"player cast skill {msg.SkillId} skill type result {response.Result}");
#endif

            player.Write(response);
        }

        private void OnResponse_CastHeroSkill(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CAST_HERO_SKILL msg = ProtobufHelper.Deserialize<MSG_GateZ_CAST_HERO_SKILL>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null) return;

            Log.Write($"player {msg.Uid} cast hero {msg.HeroId} skill {msg.SkillId}");
            MSG_ZGC_CAST_HERO_SKILL response = new MSG_ZGC_CAST_HERO_SKILL();
            response.HeroId = msg.HeroId;
            response.SkillId = msg.SkillId;

            Hero hero = player.HeroMng.GetHero(msg.HeroId);
            if(hero == null)
            {
                Log.Warn($"player {msg.Uid} cast hero {msg.HeroId} skill {msg.SkillId} failed: hero not exist");
                response.Result = (int)ErrorCode.NoSuchHero;
                player.Write(response);
                return;
            }
            if(player.IsDead || hero.IsDead)
            {
                Log.Warn($"player {msg.Uid} cast hero {msg.HeroId} skill {msg.SkillId} failed: player or hero is dead");
                response.Result = (int)ErrorCode.Dead;
                player.Write(response);
                return;
            }
            Skill skill = hero.SkillManager.GetSkill(msg.SkillId);
            if(skill == null)
            {
                Log.Warn($"player {msg.Uid} cast hero {msg.HeroId} skill {msg.SkillId} failed: hero not exist");
                response.Result = (int)ErrorCode.SkillNotExist;
                player.Write(response);
                return;
            }
            if(!hero.SkillManager.Check(skill))
            {
                //Log.Warn($"player {msg.Uid} cast hero {msg.HeroId} skill {msg.SkillId} failed: check failed");
                response.Result = (int)ErrorCode.CheckSkillFailed;
                player.Write(response);
                return;
            }
           
            if(!skill.SkillModel.CastedByOwner)
            {
                Log.Warn($"player {msg.Uid} cast hero {msg.HeroId} skill {msg.SkillId} failed: not casted by owner");
                response.Result = (int)ErrorCode.CheckSkillFailed;
                player.Write(response);
                return;
            }
            if(hero.SkillEngine.InReadyList(msg.SkillId))
            {
                Log.Debug($"player {msg.Uid} cast hero {msg.HeroId} skill {msg.SkillId} failed: already in skill engine");
                response.Result = (int)ErrorCode.CheckSkillFailed;
                player.Write(response);
                return;
            }
            if (player.CurrentMap.Model.IsAutoBattle)
            {
                Log.Debug($"player {msg.Uid} cast hero {msg.HeroId} skill {msg.SkillId} failed: auto battle dungeon");
                response.Result = (int)ErrorCode.CheckSkillFailed;
                player.Write(response);
                return;
            }

            hero.SkillManager.CheckBreakNormalSkill(skill);

            //FieldObject target;
            //Vec2 targetPos;
            //if (!player.TryGetCastSkillParam(skill.SkillModel, out target, out targetPos))
            //{
            //    response.Result = (int)ErrorCode.TargetNotFind;
            //    player.Write(response);
            //    return;
            //}

            response.Result = (int)ErrorCode.Success;
            player.Write(response);
            hero.SkillEngine.AddSkill(msg.SkillId, null);

            hero.FsmManager.SetNextFsmStateType(FsmStateType.HERO_ATTACK);
        }

    }
}