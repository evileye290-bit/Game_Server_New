using System;
using System.Threading;
using System.Collections.Generic;
using ServerShared;
using Logger;
using DBUtility;
using CommonUtility;
using ScriptFunctions;
using ServerFrame;

namespace BattleManagerServerLib
{
    public partial class BattleManagerServerApi : BaseApi
    {
        // args [mainId]
        public override void Init(string[] args)
        {
            base.Init(args);

            // init阶段结束，起服完成
            InitDone();
        }

    }
}
