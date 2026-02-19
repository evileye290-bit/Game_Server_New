using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RZ;
using RedisUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossServerLib
{
    public class BaseNotes
    {
        protected NotesType notesType;
        protected CrossServerApi server;
        NotesConfig configInfo;
        int notesUid = 0;
        protected int groupId;

        protected Queue<NotesModel> InfoDic = new Queue<NotesModel>();

        public BaseNotes(CrossServerApi server, NotesConfig notesConfig, int groupId)
        {
            this.notesType = notesConfig.Type;
            this.configInfo = notesConfig;
            this.server = server;
            this.groupId = groupId;

            LoadInitRankFromRedis();
        }

        public int GetNewUid()
        {
            if (notesUid >= 1000000)
            {
                notesUid = 0;
            }
            return ++notesUid;
        }

        public void AddNewNotes(NotesModel notes)
        {
            if (InfoDic.Count >= configInfo.SaveCount)
            {
                NotesModel deleteNotes = InfoDic.Dequeue();
                Delete(deleteNotes.Uid);
            }
            notes.Uid = GetNewUid();
            InfoDic.Enqueue(notes);
            server.CrossRedis.Call(new OperateAddCrossNotesJsonInfo(groupId, notesType, notes));
        }

        public void Clear()
        {
            InfoDic.Clear();
            server.CrossRedis.Call(new OperateDeleteCrossNotes(groupId, notesType));
        }

        public void Delete(int notesUid)
        {
            server.CrossRedis.Call(new OperateDeleteCrossNotesJsonInfo(groupId, notesType, notesUid));
        }

        public void LoadInitRankFromRedis()
        {
            if (configInfo == null)
            {
                return;
            }
            OperateGetCrossNotesList op = new OperateGetCrossNotesList(groupId, notesType);
            server.CrossRedis.Call(op, ret =>
            {
                InfoDic = op.NotesList;
            });
        }

        public void PushRankListMsg(int uid, int mainId)
        {
            MSG_ZGC_CROSS_NOTES_LIST rankMsg = new MSG_ZGC_CROSS_NOTES_LIST();
            rankMsg.Type = (int)notesType;

            List<NotesModel> list = InfoDic.ToList();
            int index = Math.Max(0, InfoDic.Count - configInfo.ShowCount);
            for (int i = index; i < list.Count; i++)
            {
                ZGC_NOTES_INFO info = new ZGC_NOTES_INFO();
                NotesModel notes = list[i];
                info.Time = notes.Time;
                info.List.AddRange(notes.List);
                rankMsg.List.Add(info);
            }
            server.RelationManager.WriteToRelation(rankMsg, mainId, uid);
        }

    }
}