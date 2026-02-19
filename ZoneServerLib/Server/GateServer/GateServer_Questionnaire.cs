using EnumerateUtility.Activity;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_QuestionnaireComplete(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_QUESTIONNAIRE_COMPLETE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_QUESTIONNAIRE_COMPLETE>(stream);
            ////Log.Write("player {0} complete quest {1}", pc.Uid, pks.quest_id);
            //PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            //if (player == null)
            //{
            //    Log.Warn("player {0} complete questionnaire not in gateid {1} pc list", pks.PcUid, SubId);
            //    return;
            //}
            //if (player.CurrentMap == null)
            //{
            //    Log.Warn("player {0} complete questionnaire not in map ", pks.PcUid);
            //    return;
            //}

            //MSG_ZGC_QUESTIONNAIRE_COMPLETE msg = new MSG_ZGC_QUESTIONNAIRE_COMPLETE();

            ////判断是否存在问卷
            //List<int> activitiesAvailable = ActivityLibrary.GetTodayActivityItemForType(ActivityAction.Questionnaire);
            //if (activitiesAvailable == null || activitiesAvailable.Count == 0)
            //{
            //    msg.Result = MSG_ZGC_QUESTIONNAIRE_COMPLETE.Types.RESULT.Error;
            //    player.Write(msg);
            //    return;
            //}


            ////拿出问卷
            //List<int> items = player.ActivityMng.GetActivityItemForType(ActivityAction.Questionnaire);
            //ActivityItem item = new ActivityItem();
            //if (items != null)
            //{
            //    item = player.ActivityMng.GetActivityItemForId(items[0]);
            //}

            //List<int> availableId = null;
            //if (item != null)
            //{
            //    availableId = QuestionnaireLibrary.GetAvailableIds(item.CurNum, player.Level, player.TimeCreated);
            //}


            
            ////判断是否做过
            //if (availableId != null && availableId.Contains(pks.QuestionnaireId))
            //{

            //    player.AddActivityNumForType(ActivityAction.Questionnaire, pks.QuestionnaireId);
            //    msg.Result = MSG_ZGC_QUESTIONNAIRE_COMPLETE.Types.RESULT.Success;
            //    //统计
            //    player.QuestionnaireComplete(pks);

            //    //邮件
            //    int index = pks.QuestionnaireId % 100 - 1;
            //    if (index < 0)
            //    {
            //        Logger.Log.Warn("{0} get email reward error {1}", player.Name, index);
            //    }
            //    int emailId = index + 4001;

            //    QuestionnaireInfo qinfo = QuestionnaireLibrary.GetQuestionnaireInfo(pks.QuestionnaireId);

            //    EmailInfo info = EmailLibrary.GetEmailInfo(emailId);
            //    if (info != null && qinfo != null)
            //    {
            //        player.SendPersonEmail(info.Id, info.Body, qinfo.Reward, 0);
            //    }
            //    else
            //    {
            //        Log.Warn("player {0} send email not find email id:{1}", player.Name, emailId);
            //    }
            //}
            //else
            //{
            //    msg.Result = MSG_ZGC_QUESTIONNAIRE_COMPLETE.Types.RESULT.Error;
            //}

            ////回复消息
            //player.Write(msg);
        }

    }
}
