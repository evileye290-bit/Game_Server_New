using EnumerateUtility;
using Message.Corss.Protocol.CorssR;
using RedisUtility;
using ServerModels;
using ServerModels.HidderWeapon;
using ServerShared;
using System;
using System.Collections.Generic;

namespace CrossServerLib
{
    public class NotesManager
    {
        private CrossServerApi server { get; set; }

        private Dictionary<int, Dictionary<NotesType, BaseNotes>> notesList = new Dictionary<int, Dictionary<NotesType, BaseNotes>>();
        public NotesManager(CrossServerApi server)
        {
            this.server = server;

            Init();
        }

        private void Init()
        {
            foreach (var group in CrossBattleLibrary.GroupList)
            {
                AddCrossNotesList(group.Key);
            }
        }

        public void AddCrossNotesList(int group)
        {
            Dictionary<NotesType, BaseNotes> dic = new Dictionary<NotesType, BaseNotes>();
            //foreach (NotesType notesType in Enum.GetValues(typeof(NotesType)))
            foreach (var item in HidderWeaponLibrary.NotesConfigs)
            {
                BaseNotes crossBoss = new BaseNotes(server, item.Value, group);
                dic.Add(item.Value.Type, crossBoss);
            }
            notesList.Add(group, dic);
        }

        public Dictionary<NotesType, BaseNotes> GetNotesList(int groupId)
        {
            Dictionary<NotesType, BaseNotes> value;
            notesList.TryGetValue(groupId, out value);
            return value;
        }

        public void AddNotesInfo(int groupId, NotesType notesType, NotesModel notes)
        {
            Dictionary<NotesType, BaseNotes> dio = GetNotesList(groupId);
            if (dio != null)
            {
                BaseNotes baseNotes;
                if (dio.TryGetValue(notesType, out baseNotes))
                {
                    baseNotes.AddNewNotes(notes);
                }
            }
        }

        public void PushRankListMsg(int groupId, NotesType notesType, int uid, int mainId)
        {
            Dictionary<NotesType, BaseNotes> dio = GetNotesList(groupId);
            if (dio != null)
            {
                BaseNotes baseNotes;
                if (dio.TryGetValue(notesType, out baseNotes))
                {
                    baseNotes.PushRankListMsg(uid, mainId);
                }
            }
        }

        public void Clear(NotesType notesType)
        {
            foreach (var kv in notesList)
            {
                foreach (var notes in kv.Value)
                {
                    if (notes.Key == notesType)
                    {
                        notes.Value.Clear();
                    }
                }
            }
        }
    }
}
