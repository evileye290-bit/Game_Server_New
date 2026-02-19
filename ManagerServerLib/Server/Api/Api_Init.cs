using DataProperty;
using DBUtility;
using Logger;
using Message.Manager.Protocol.MG;
using Message.Manager.Protocol.MZ;
using ServerFrame;
using ServerShared;
using ServerShared.Map;
using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace ManagerServerLib
{
    partial class ManagerServerApi
    {
        private DataList mapDataList;
        public DataList MapDataList
        { get { return mapDataList; } }

        private MapBallenceProxy mapBallenceProxy;
        public MapBallenceProxy BallenceProxy
        { get { return mapBallenceProxy; } }

        //key mainId, value ZoneManager <subId, ZServer>
        public ZoneServerManager ZoneServerManager
        { get { return (ZoneServerManager)serverManagerProxy.GetFrontendServerManager(ServerType.ZoneServer); } }

        public BackendServer GlobalServer
        { get { return serverManagerProxy.GetSinglePointBackendServer(ServerType.GlobalServer, ClusterId); } }

        public BackendServerManager BarrackServerManager
        { get { return serverManagerProxy.GetBackendServerManager(ServerType.BarrackServer); } }

        public RelationServer RelationServer
        { get { return (RelationServer)serverManagerProxy.GetSinglePointFrontendServer(ServerType.RelationServer, MainId); } }

        public FrontendServerManager GateServerManager
        { get { return serverManagerProxy.GetFrontendServerManager(ServerType.GateServer); } }

        private RechargeManager rechargeMng;
        public RechargeManager RechargeMng
        { get { return rechargeMng; } }

        private SchoolManager schoolManager;
        public SchoolManager SchoolManager => schoolManager;

        public AddictionManager AddictionMng = null;

        //// key map id
        //private Dictionary<int, MapLimit> mapLimitList = new Dictionary<int, MapLimit>();

        // character max uid
        private int maxCharUid = 0;
        public int MaxCharUid
        {
            get { return maxCharUid; }
            set { maxCharUid = value; }
        }

        private DateTime nextSendTime = now;
        private int registCharacterCount = 0;
        public int RegistCharacterCount => registCharacterCount;


        public void InitMapBallenceProxy()
        {
            mapBallenceProxy = new MapBallenceProxy(this);
        }

        private void InitRechargeManager()
        {
            rechargeMng = new RechargeManager(this);
        }

        private void InitSchoolManager()
        {
            schoolManager = new SchoolManager();
        }

        public void InitAddictionManager()
        {
            AddictionMng = new AddictionManager(this);
        }

        public override void InitData()
        {
            base.InitData();
            //RechargeLibrary.BindDatas();
            MapLibrary.Init();
            DataList gameConfig = DataListManager.inst.GetDataList("ConstConfig");
            foreach (var item in gameConfig)
            {
                Data data = item.Value;
                GameConfig.TrackingLogSwitch = data.GetBoolean("TrackingLogSwitch");
            }
            HuntingLibrary.Init();
        }

        public override void InitConfig()
        {
            base.InitConfig();
            mapDataList = DataListManager.inst.GetDataList("Zone");
            //serverDataList = DataListManager.inst.GetDataList("ServerConfig");
        }

        public override void InitProtocol()
        {
            base.InitProtocol();
            Message.Manager.Protocol.MZ.MZIdGenerator.GenerateId();
            Message.Manager.Protocol.MR.MRIdGenerator.GenerateId();
            Message.Manager.Protocol.MB.MBIdGenerator.GenerateId();
            Message.Manager.Protocol.MG.MGIdGenerator.GenerateId();
            Message.Manager.Protocol.MGate.MGateIdGenerator.GenerateId();
            Message.Manager.Protocol.MM.MMIdGenerator.GenerateId();
            Message.Manager.Protocol.MP.MPIdGenerator.GenerateId();

            Message.Zone.Protocol.ZM.ZMIdGenerator.GenerateId();
            Message.Relation.Protocol.RM.RMIdGenerator.GenerateId();
            Message.Barrack.Protocol.BM.BMIdGenerator.GenerateId();
            Message.Global.Protocol.GM.GMIdGenerator.GenerateId();
            Message.Gate.Protocol.GateM.GateMIdGenerator.GenerateId();
            Message.Pay.Protocol.PM.PMIdGenerator.GenerateId();
        }

        public override void InitTrackingLog()
        {
            string statLogPrefix = string.Format("_{0}_{1}", MainId, SubId);
            string logServerKey = string.Empty;
            DataList gameConfig = DataListManager.inst.GetDataList("ConstConfig");
            foreach (var item in gameConfig)
            {
                Data data = item.Value;
                logServerKey = data.GetString("LogServerKey");
            }
            trackingLoggerManager = new TrackingLoggerManager(logServerKey, statLogPrefix);
            TrackingLoggerMng.CreateLogger(TrackingLogType.ONLINE);//在线
            TrackingLoggerMng.CreateLogger(TrackingLogType.RECHARGE);///
        }

        public override void InitBILog()
        {
            string statLogPrefix = string.Format("_{0}_{1}", mainId, subId);

            string logServerKey = string.Empty;
            DataList gameConfig = DataListManager.inst.GetDataList("ConstConfig");
            foreach (var item in gameConfig)
            {
                Data data = item.Value;
                logServerKey = data.GetString("LogServerKey");
            }

            //biLoggerManager = new BILoggerManager(logServerKey, statLogPrefix, this);
            biLoggerManager = new BILoggerManager(logServerKey,ServerType.ToString(), statLogPrefix, this);
            //BILoggerMng.CreateLogger(BILoggerType.ONLINE);//

            BIXmlUpdate();
        }


        public override void InitDB()
        {
            base.InitDB();

            List<AbstractDBQuery> queries = new List<AbstractDBQuery>();

            QueryMaxCharUid queryMaxCharUid = new QueryMaxCharUid();
            queries.Add(queryMaxCharUid);

            QueryUidCount queryUidCount = new QueryUidCount();
            queries.Add(queryUidCount);

            QueryLoadSchoolStudentCount queryLoadSchool = new QueryLoadSchoolStudentCount();
            queries.Add(queryLoadSchool);

            DBQueryTransaction transaction = new DBQueryTransaction(queries, true);

            //string charTableName = "character";
            gameDBPool.Call(transaction, ret =>
            {
                if ((int)ret == 0)
                {
                    int max = queryMaxCharUid.MaxUid;
                    max = max > GetInitialOffsetId() ? max : GetInitialOffsetId();
                    maxCharUid = maxCharUid > max ? maxCharUid : max;
                    Log.Write("table character max char uid {0}", maxCharUid);

                    registCharacterCount = queryUidCount.CharacterCount;

                    schoolManager.SetSchoolStudentCount(queryLoadSchool.StudentCount);
                }
            });
        }

        public void NotifyGlobalAlarm(AlarmType type, int main, int sub, string content)
        {
            MSG_MG_ALARM_NOTIFY alarm = new MSG_MG_ALARM_NOTIFY();
            alarm.Type = (int)type;
            alarm.Main = main;
            alarm.Sub = sub;
            alarm.Time = ManagerServerApi.now.ToString();
            alarm.Content = content;
            if (GlobalServer != null)
            {
                GlobalServer.Write(alarm);
            }
        }

        public void SendAlarmMail(string title, string content)
        {
            if (CONST.ALARM_OPEN == false)
            {
                return;
            }
            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
            msg.To.Add("75706748@qq.com");
            //msg.CC.Add("307112866@qNum.com");
            /* 上面3个参数分别是发件人地址，发件人姓名，编码*/
            msg.From = new MailAddress("wanxingame@163.com", "Trail", System.Text.Encoding.UTF8);
            msg.Subject = title;//邮件标题    
            msg.SubjectEncoding = System.Text.Encoding.UTF8;//邮件标题编码    
            msg.Body = content;
            msg.BodyEncoding = System.Text.Encoding.UTF8;//邮件内容编码    
            msg.IsBodyHtml = false;//是否是HTML邮件    
            msg.Priority = MailPriority.High;//邮件优先级    

            SmtpClient client = new SmtpClient();
            client.Credentials = new System.Net.NetworkCredential("wanxingame@163.com", "ruaf001");
            client.Host = "smtp.163.com";
            object userState = msg;
            try
            {
                client.SendAsync(msg, userState);
                //Log.Warn("发送成功");
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                Log.Error(ex.Message, "发送邮件出错");
            }
        }

        public override void UpdateXml()
        {
            //base.UpdateXml();
            InitData();
            BIXmlUpdate();
            RechargeLibrary.Init();

            UpdateMyCardCount();
            HuntingLibrary.Init();
        }

        public void UpdateMyCardCount()
        {
            QueryGetMyCardCount query = new QueryGetMyCardCount();
            AccountDBPool.Call(query, ret2 =>
            {
                MSG_MZ_GET_SPECIAL_ACTIVITY_ITEM msg = new MSG_MZ_GET_SPECIAL_ACTIVITY_ITEM();
                msg.TotalCount = query.TotalCount;
                msg.UseCount = query.UseCount;
                ZoneServerManager.Broadcast(msg);
            });
        }

        private void BIXmlUpdate()
        {
            var biConfigData = DataListManager.inst.GetData("BILogConfig", 1);
            int fileSize = biConfigData.GetInt("FileSize");
            int refreshTime = biConfigData.GetInt("RefreshTime");
            BILoggerMng.UpdateXml(refreshTime, fileSize);
        }
        internal void OnRegistNewCharacter()
        {
            registCharacterCount++;
            AccountDBPool.Call(new QueryUpdateServersRegistCharacterCount(MainId, registCharacterCount));
        }
    }

}
