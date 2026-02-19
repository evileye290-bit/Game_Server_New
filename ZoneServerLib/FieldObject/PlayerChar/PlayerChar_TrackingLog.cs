using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateZ;
using ServerModels;
using ServerShared;
using System.Linq;

namespace ZoneServerLib
{
    public partial class PlayerChar : FieldObject
    {
        public const string DATETIME_TO_STRING = "yyyy-MM-dd HH:mm:ss";

        /*
      * consumeWay 消耗方式
      * consume_type 消耗类型 为对应的货币枚举值或者可消耗材料item type id
      * cur_count 当前个数
      * consume_count 消耗的个数
      * extraParam 附加区分值 比如：宝箱开启获取途径，param传具体宝箱类型
      */
        public void RecordConsumeLog(ConsumeWay consumeWay, RewardType rewardType, int consume_type, int cur_count, int consume_count, string extraParam)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)return;
            
            string log = $"{uid}|{AccountName}|{Name}|{Level}|{server.MainId}|{ChannelName}|{consumeWay}|{consume_type}|{rewardType}|{cur_count}|{consume_count}|{extraParam}|{GetTimeStr()}";
            server.TrackingLoggerMng.Write(log, TrackingLogType.CONSUME);
        }

        /*
       * consumeWay 消耗方式
       * consume_type 消耗类型 为对应的货币枚举值或者可消耗材料item type id
       * cur_count 当前个数 long类型
       * consume_count 消耗的个数
       * extraParam 附加区分值 比如：宝箱开启获取途径，param传具体宝箱类型
       */
        public void RecordConsumeLog(ConsumeWay consumeWay, RewardType rewardType, int consume_type, long cur_count, int consume_count, string extraParam)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch) return;

            string log = $"{uid}|{AccountName}|{Name}|{Level}|{server.MainId}|{ChannelName}|{consumeWay}|{consume_type}|{rewardType}|{cur_count}|{consume_count}|{extraParam}|{GetTimeStr()}";
            server.TrackingLoggerMng.Write(log, TrackingLogType.CONSUME);
        }  

        /*
        * obtainWay 获取方式
        * obtain_type 获取类型 为对应的货币枚举值或者可消耗材料item type id
        * cur_count 当前个数
        * obtain_count 获取的个数
        * extraParam 附加说明参数
        */
        public void RecordObtainLog(ObtainWay obtainWay, RewardType rewardType, int obtain_type, int cur_count, int obtain_count, string extraParam="")
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)return;
            
            string log = $"{uid}|{AccountName}|{Name}|{Level}|{server.MainId}|{ChannelName}|{obtainWay}|{rewardType}|{obtain_type}|{cur_count}|{obtain_count}|{extraParam}|{GetTimeStr()}";
            server.TrackingLoggerMng.Write(log, TrackingLogType.OBTAIN);
        }

        /*
       * obtainWay 获取方式
       * obtain_type 获取类型 为对应的货币枚举值或者可消耗材料item type id
       * cur_count 当前个数 long类型
       * obtain_count 获取的个数
       * extraParam 附加说明参数
       */
        public void RecordObtainLog(ObtainWay obtainWay, RewardType rewardType, int obtain_type, long cur_count, int obtain_count, string extraParam = "")
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch) return;

            string log = $"{uid}|{AccountName}|{Name}|{Level}|{server.MainId}|{ChannelName}|{obtainWay}|{rewardType}|{obtain_type}|{cur_count}|{obtain_count}|{extraParam}|{GetTimeStr()}";
            server.TrackingLoggerMng.Write(log, TrackingLogType.OBTAIN);
        }

        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="Ip">客户端ip</param>
        /// <param name="deviceId">客户端设备</param>
        public void RecordLoginLog(string Ip,string deviceId)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)return;
            
            string log = $"{uid}|{AccountName}|{Name}|{Level}|{server.MainId}|{ChannelName}|{deviceId}|{Ip}|{GetCoins(CurrenciesType.exp)}|{GetCoins(CurrenciesType.diamond)}|{GetCoins(CurrenciesType.gold)}|{GetTimeStr()}";
            server.TrackingLoggerMng.Write(log, TrackingLogType.LOGIN);
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <param name="Ip">客户端ip</param>
        /// <param name="deviceId">客户端设备</param>
        public void RecordLogoutLog(string Ip, string deviceId)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch) return;
            
            string log = $"{uid}|{AccountName}|{Name}|{Level}|{server.MainId}|{ChannelName}|{deviceId}|{Ip}|{TimeCreated}|{GetCoins(CurrenciesType.exp)}|{GetCoins(CurrenciesType.diamond)}|{GetCoins(CurrenciesType.gold)}|{GetTimeStr()}";
            server.TrackingLoggerMng.Write(log, TrackingLogType.LOGOUT);
        }

        public void RecordQuestionnaireLog(Question question)
        {
            if (!GameConfig.TrackingLogSwitch) return;
            
            string log;
            int answer = 0;
            string answers = "";
            if (question.Answers.Count() == 1)
            {
                //为0说明没有选择，但是要埋入，保证格式
                answer = question.Answers[0];
            }
            else
            {
                answers += question.Answers[0];
                for (int i = 1; i < question.Answers.Count(); i++)
                {
                    answers += ":" + question.Answers[i];
                }
            }
            log = $"{uid}|{AccountName}|{Name}|{Level}|{server.MainId}|{ChannelName}|{(ZoneServerApi.now - TimeCreated).Days}|{question.Id}|{question.QuestionnaireId}|{answer}|{answers}|{question.Input}|{question.Type}|{GetTimeStr()}";
            server.TrackingLoggerMng.Write(log, TrackingLogType.QUESTION);
        }

        public void RecordGameCommentLog(MSG_GateZ_SUGGEST pks)
        {
            //游戏体验：等级信息，主线id   游戏资源：gold，diamond，fishcoin
            string log = $"{uid}|{AccountName}|{Name}|{Level}|{server.MainId}|{ChannelName}{pks.Suggest}|{PasscardLevel}|{MainLineId}|{GetCoins(CurrenciesType.diamond)}|{GetCoins(CurrenciesType.gold)}|{GetCoins(CurrenciesType.friendlyHeart)}|{GetTimeStr()}";

            server.TrackingLoggerMng.Write(log, TrackingLogType.SUGGEST);
        }

        public void RecordTaskLog(int taskId)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch) return;
            
            string log = $"{uid}|{AccountName}|{Name}|{Level}|{server.MainId}|{ChannelName}{taskId}{GetTimeStr()}";
            server.TrackingLoggerMng.Write(log, TrackingLogType.TASK);
        }

        /// <summary>
        /// 商店
        /// </summary>
        public void RecordShopByItemLog(ShopType shopType, string currencyType, int currencyCount, ObtainWay obtainType, RewardType moduleId, int itemType, int itemCount)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch) return;
            
            string log = $"{uid}|{AccountName}|{Name}|{Level}|{server.MainId}|{ChannelName}|{shopType}|{currencyType}|{currencyCount}|{obtainType}|{moduleId}|{itemType}|{itemCount}|{GetTimeStr()}";
            server.TrackingLoggerMng.Write(log, TrackingLogType.SHOP);
        }


        /*
         * 充值
         */
        public void RecordRechargeLog(float money, string gameOrderId, string sdkOrderId, string payOrderId, string moneyType, string payWay, string productId)
        {
            if (!GameConfig.TrackingLogSwitch) return;

            int isSuccess = 1;
            string log = $"{uid}|{AccountName}|{Name}|{Level}|{server.MainId}|{ChannelName}|{money}|{gameOrderId}|{sdkOrderId}|{payOrderId}|{moneyType}|{isSuccess}|{payWay}|{productId}|{SDKUuid}|{GetTimeStr()}";
            server.TrackingLoggerMng.Write(log, TrackingLogType.RECHARGE);
        }


        private string GetTimeStr()
        {
            return ZoneServerApi.now.ToString(DATETIME_TO_STRING);
        }
    }
}
