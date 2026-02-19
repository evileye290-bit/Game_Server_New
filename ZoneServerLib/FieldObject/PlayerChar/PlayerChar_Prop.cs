using DataProperty;
using ServerShared;
using System;
using System.Collections.Generic;
using CommonUtility;
using Message.Relation.Protocol.RZ;
using ServerModels;
using EnumerateUtility;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        #region 人物游戏属性
        /// <summary>
        /// 是否是GM
        /// </summary>
        public int IsGm = 0;

        /// <summary>
        /// 角色账号
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 渠道
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// SDK
        /// </summary>
        public string RegisterId { get; set; }

        /// <summary>
        /// IP
        /// </summary>
        public string ClientIp { get; set; }

        /// <summary>
        /// 设备Id
        /// </summary>
        public string DeviceId = string.Empty;

        public string SDKUuid = string.Empty;

        public string ChannelId = string.Empty;
        public string Idfa = string.Empty;     //苹果设备创建角色时使用
        public string Idfv = string.Empty;    //苹果设备创建角色时使用
        public string Imei = string.Empty;    //安卓设备创建角色时使用
        public string Imsi = string.Empty;      //安卓设备创建角色时使用
        public string Anid = string.Empty;    //安卓设备创建角色时使用
        public string Oaid = string.Empty;   //安卓设备创建角色时使用
        public string PackageName = string.Empty;//包名
        public string ExtendId = string.Empty;  //广告Id，暂时不使用
        public string Caid = string.Empty;      //暂时不使用

        public int Tour;                         //是否是游客账号（0:非游客，1：游客）
        public string Platform = string.Empty;   //平台名称	统一：ios|android|windows
        public string ClientVersion = string.Empty;   //游戏的迭代版本，例如1.0.3
        public string DeviceModel = string.Empty;     //设备的机型，例如Samsung GT-I9208
        public string OsVersion = string.Empty;  //操作系统版本，例如13.0.2	
        public string Network = string.Empty;    //网络信息	4G/3G/WIFI/2G
        public string Mac = string.Empty;        //局域网地址
        public int GameId;

        /// <summary>
        /// 角色名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是不是观战者
        /// </summary>
        public bool IsObserver { get; set; }

        public bool IsVisable = true;
        /// <summary>
        /// 角色等级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 系统头像id
        /// </summary>
        public int Icon { get; set; }

        /// <summary>
        /// 显示自定义头像
        /// </summary>
        public bool ShowDIYIcon { get; set; }


        /// <summary>
        /// 性别
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 公会ID
        /// </summary>
        public int GuideId { get; set; }

        /// <summary>
        /// 主任务ID
        /// </summary>
        public int MainTaskId { get; set; }
        /// <summary>
        /// 分支任务ID
        /// </summary>
        public List<int> BranchTaskIds = new List<int>();
        /// <summary>
        /// 主线ID
        /// </summary>
        public int MainLineId { get; set; }

        /// <summary>
        /// 历史最高竞技等级
        /// </summary>
        public int ladderHistoryMaxLevel { get; set; }

        /// <summary>
        /// 角色创建时间
        /// </summary>
        public DateTime TimeCreated { get; set; }
        //public UInt64 AccountCreateTimestamp { get; set; }
  
        //离开状态
        public bool LeavedWorld { get; set; }
     
        public int NextAngle { get; set; }

        public VIP Vip { get; set; }

        public JobType Job { get; set; }

        public int BagSpace { get; set; }
  
        /// <summary>
        /// 通行证等级
        /// </summary>
        public int PassLevel { get; set; }
        /// <summary>
        /// 是否返利过
        /// </summary>
        public bool IsRebated { get; set; }

        public int BattlePower { get; set; }
        /// <summary>
        /// 累计活跃天数
        /// </summary>
        public int CumulateDays { get; set; }
        /// <summary>
        /// 累计在线时长
        /// </summary>
        public int CumulateOnlineTime { get; set; }
        //public int JobLevel { get { return jobData.GetInt("Level"); } }

        //public int Angle { get; set; }

        //public float CurPosX { get; set; }

        //public float CurPosY { get; set; }

        public List<string> localSoftwares = new List<string>();

        #endregion
        /*---------------------------------------------------------华丽的分割线-----------------------------------------------------------*/
        #region 人物空间属性

        //private int spaceSex = 0;
        //    /// <summary>
        //    /// 空间性别
        //    /// </summary>
        //    public int SpaceSex
        //    {
        //        get { return spaceSex; }
        //        set { spaceSex =value; }
        //    }

        //    private string birthday = string.Empty;
        //    /// <summary>
        //    /// 空间生日
        //    /// </summary>
        //    public string Birthday
        //    {
        //        get { return birthday; }
        //        set {  birthday =value; }
        //    }

        //    private bool showVoice = false;
        //    /// <summary>
        //    /// 展示音频
        //    /// </summary>
        //    public bool ShowVoice
        //    {
        //        get { return showVoice; }
        //        set { showVoice = value; }
        //    }

        //string signature = string.Empty;
        ///// <summary>
        ///// 个性签名
        ///// </summary>
        //public string Signature
        //{
        //    get { return signature; }
        //    set { signature = value; }
        //}

        //int infoShowType = 0;
        ///// <summary>
        ///// 微信 QQ 展示类型
        ///// </summary>
        //public int InfoShowType
        //{
        //    get { return infoShowType; }
        //    set { infoShowType = value; }
        //}

        //string qNum = string.Empty;
        ///// <summary>
        ///// Q号
        ///// </summary>
        //public string QNum
        //{
        //     get { return qNum; }
        //    set { qNum = value; }
        //}

        //string wNum = string.Empty;
        ///// <summary>
        ///// W号
        ///// </summary>
        //public string WNum
        //{
        //    get { return wNum; }
        //    set { wNum = value; }
        //}

        public int PopScore = 0;
        public int HighestPopScore = 0;
        #endregion
        /*---------------------------------------------------------华丽的分割线-----------------------------------------------------------*/
        //#region 地理位置
        ///// <summary>
        ///// 经度
        ///// </summary>
        //public Geography geography = new Geography();
        //#endregion
    }
}