using CommonUtility;
using DataProperty;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class FaceFrameItem : BaseItem
    {
        public int ActivateState { get; set; }//激活状态

        private FaceFrameInfo faceFrameInfo;
        private FaceFrameModel model;

        public FaceFrameItem(FaceFrameInfo faceFrameInfo) : base(faceFrameInfo)
        {
            this.faceFrameInfo = faceFrameInfo;
            this.MainType = MainType.FaceFrame;
            this.BindData(faceFrameInfo.TypeId);
        }

        public FaceFrameItem(FaceFrameInfo faceFrameInfo, FaceFrameModel model) : base(faceFrameInfo)
        {
            this.model = model;
            this.MainType = MainType.FaceFrame;
            this.faceFrameInfo = faceFrameInfo;
        }

        public override bool BindData(int id)
        {
            this.model = BagLibrary.GetFaceFrameModel(id);
            if (this.model != null)
            {
                //TODO 绑定差异数据
                return true;
            }
            else
            {
                Logger.Log.Warn($"have no this faceframe model id {id}");
                return false;
            }
        }

        public ITEM GenerateSyncMessage()
        {
            ITEM syncMsg = new ITEM()
            {
                UidHigh = this.Uid.GetHigh(),
                UidLow = this.Uid.GetLow(),
                Id = this.Id,
                PileNum = this.PileNum,
                ActivateState = this.ActivateState,
                GenerateTime = this.GenerateTime,
            };
            return syncMsg;
        }

        public ZMZ_ITEM GenerateTransformMessage()
        {
            ZMZ_ITEM syncMsg = new ZMZ_ITEM()
            {
                Uid = this.Uid,
                Id = this.Id,
                PileNum = this.PileNum,
                ActivateState = this.ActivateState,
                GenerateTime = this.GenerateTime,
            };
            return syncMsg;
        }
    }
}
