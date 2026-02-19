using System.Collections.Generic;
using CommonUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    class MarkInfo
    {
        public FieldObject FieldObject;
        public int Id;
        public int Count;
    }

    public class MarkManager
    {
        private FieldObject owner;
        private Dictionary<int, Mark> markList;

        private Queue<int> removeList;
        private Queue<int> removeListTemp;
        private Queue<MarkInfo> addMarkInfos;
        private Queue<MarkInfo> addMarkInfosTemp;

        public MarkManager(FieldObject owner)
        {
            this.owner = owner;
            markList = new Dictionary<int, Mark>();
            removeList = new Queue<int>();
            removeListTemp = new Queue<int>();
            addMarkInfos = new Queue<MarkInfo>();
            addMarkInfosTemp = new Queue<MarkInfo>();
        }


        public Mark GetMark(int id)
        {
            Mark mark;
            markList.TryGetValue(id, out mark);
            return mark;
        }

        public void AddMark(FieldObject field, int id, int count)
        {
            addMarkInfos.Enqueue(new MarkInfo() { FieldObject = field, Id = id, Count = count });
        }

        public void RemoveMark(int id)
        {
            Mark mark = null;
            if (markList.TryGetValue(id, out mark))
            {
                removeList.Enqueue(id);
            }
        }

        public void ReduceMarkCount(int markId, int num)
        {
            Mark mark = GetMark(markId);
            if (mark != null)
            {
                mark.Reduce(num);
                BroadcastMark(mark);
            }
        }

        private void BroadcastMark(Mark mark)
        {
            if (mark == null) return;
            MSG_ZGC_MARK msg = new MSG_ZGC_MARK()
            {
                InstanceId = owner.InstanceId,
                MarkId = mark.Id,
                MarkType = (int)mark.Type,
                Count = mark.CurCount
            };
            owner.BroadCast(msg);
        }

        public void Update(float dt)
        {
            foreach(var kv in markList)
            {
                Mark mark = kv.Value;
                int lastCount = mark.CurCount;
                mark.Update(dt);
                if(mark.CurCount != lastCount)
                {
                    BroadcastMark(mark);
                }
                if (mark.CurCount <= 0)
                {
                    removeList.Enqueue(mark.Id);
                }
                else
                {
                    if (mark.Enough())
                    {
                        owner.DispatchMessage(TriggerMessageType.MarkEnough, mark.Id);
                        mark.Lock();
                    }
                }
            }

            ObjectHelper.Swap(ref removeList, ref removeListTemp);

            while (removeListTemp.Count>0)
            {
                Remove(removeListTemp.Dequeue());
            }

            ObjectHelper.Swap(ref addMarkInfos, ref addMarkInfosTemp);

            while (addMarkInfosTemp.Count > 0)
            {
                AddMark(addMarkInfosTemp.Dequeue());
            }
        }

        private void Remove(int id)
        {
            Mark mark;
            if (markList.TryGetValue(id, out mark))
            {
                mark.Reset();
                BroadcastMark(mark);
                markList.Remove(id);
            }
        }

        private void AddMark(MarkInfo info)
        {
            Mark mark;
            if (!markList.TryGetValue(info.Id, out mark))
            {
                MarkModel model = MarkLibrary.GetMarkModel(info.Id);
                if (model == null)
                {
                    Log.Warn($"create mark {info.Id} failed: no such model");
                    return;
                }
                mark = Mark.CreateMark(model);
                markList.Add(info.Id, mark);
            }

            if (mark.Locked)
            {
                return;
            }

            if (mark.Add(info.FieldObject.InstanceId, info.Count))
            {
                BroadcastMark(mark);

                if (mark.Enough())
                {
                    owner.DispatchMessage(TriggerMessageType.MarkEnough, mark.Id);
                    mark.Lock();
                }

                if (info.FieldObject.SubcribedMessage(TriggerMessageType.AddMark))
                {
                    info.FieldObject.DispatchMessage(TriggerMessageType.AddMark, info.Id);
                }
            }
        }
    }
}
