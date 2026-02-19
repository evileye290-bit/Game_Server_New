using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using RedisUtility;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class PushFigureManager
    {
        private PlayerChar owner;

        public int Id { get; private set; }
        public PushFigureStatus PushFigureStatus { get; private set; }

        public PushFigureManager(PlayerChar owner)
        {
            this.owner = owner;
        }

        public void Init(int id, PushFigureStatus pushFigureStatus)
        {
            Id = id;
            PushFigureStatus = pushFigureStatus;
        }

        private void SetOpenning()
        {
            PushFigureStatus = PushFigureStatus.Opening;
            SyncPushFigureInfo2DB();
            owner.SendPushFigurateInfo();
        }

        public void SetFinished()
        {
            PushFigureStatus = PushFigureStatus.Finished;
            bool openNew = CheckAndOpenNext();
            if (!openNew)
            {
                SyncPushFigureInfo2DB();
                owner.SendPushFigurateInfo();
            }
        }

        public void CheckAndOpenNext(int taskId)
        {
            int newId;
            if (PushFigureLibrary.CheckOpenNew(taskId, out newId))
            {
                //首次开启
                if (IsFirstOpen())
                {
                    OpenNew(PushFigureLibrary.GetPushFigureModel(PushFigureLibrary.FirstId));
                    return;
                }
                else
                {
                    //开启当前关
                    if (Id <= newId && PushFigureStatus == PushFigureStatus.NotOpen)
                    {
                        SetOpenning();
                    }
                }
            }
        }


        private bool IsFirstOpen()
        {
            return Id == 0 || (Id == PushFigureLibrary.FirstId && PushFigureStatus != PushFigureStatus.Finished);
        }

        public bool CheckAndOpenNext()
        {
            bool openNew = false;
            PushFigureModel model = PushFigureLibrary.GetPushFigureModel(Id);
            if (model == null)
            {
                Log.ErrorLine($"{owner.Uid} push figure error : model id {Id}");
                return openNew;
            }

            PushFigureModel newModel = GetNextOpenModel(Id);
            if (newModel?.Id > Id)
            {
                openNew = true;
                OpenNew(newModel);
            }

            return openNew;
        }

        public PushFigureModel GetNextOpenModel(int id)
        {
            PushFigureModel model = PushFigureLibrary.GetPushFigureModel(id);
            if (model == null) return null;

            PushFigureModel nextModel = PushFigureLibrary.GetPushFigureModel(model.NextId);

             //如果是特殊类型，则跳过，继续获取下一个
            if (nextModel != null && nextModel.Type == PushFigureType.Special)
            {
                return GetNextOpenModel(nextModel.Id);
            }
            else
            {
                //该关不是特殊特殊关，返回改关
                if (nextModel != null)
                {
                    return nextModel;
                }

                //不能开启下一关 返回当前关
                return model;
            }
        }

        private void OpenNew(PushFigureModel model)
        {
            Id = model.Id;
            PushFigureStatus = CheckTaskFinished(model.TaskLimit) ? PushFigureStatus.Opening : PushFigureStatus.NotOpen;

            SyncPushFigureInfo2DB();
            owner.SendPushFigurateInfo();

            owner.SerndUpdateRankValue(RankType.PushFigure, Id);
        }

        private bool CheckTaskFinished(int taskId)
        {
            return taskId <= owner.MainTaskId;
        }

        private void SyncPushFigureInfo2DB()
        {
            QueryUpdatePushFigure query = new QueryUpdatePushFigure(owner.Uid, Id, (int)PushFigureStatus);
            owner.server.GameDBPool.Call(query);

            owner.server.GameRedis.Call(new OperateUpdateRankScore(RankType.PushFigure, owner.server.MainId, owner.Uid, Id, owner.server.Now()));
        }
    }
}
