using Message.Server.Game.Protocol.ZC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoneServerLib.DB;
using ZoneServerLib.Model;

namespace GlobalServerLib.Timer
{
    public class EmailWrapper
    {
        public void EmailSend(Api server, EmailDataModel email)
        {
            server.db.Call(new QuerySystemEmailSend(email), (ret) =>
            {
                int result = (int)ret;
                if (result == 1)
                {
                    PKS_ZC_EMAIL_REMIND msg = new PKS_ZC_EMAIL_REMIND();
                    msg.IsNew = 1;
                    server.BroadCastZServer(msg);
                }
            });

        }

        public void JjcSend(Api server, JjcEmailDataModel email, bool isEnd)
        {
            server.db.Call(new QueryJjcEmailSend(email), (ret) =>
            {
                int result = (int)ret;
                if (result == 1 && isEnd)
                {
                    PKS_ZC_EMAIL_REMIND msg = new PKS_ZC_EMAIL_REMIND();
                    msg.IsNew = 1;
                    server.BroadCastZServer(msg);
                }
            });
        }

        public void JjcSaveYesterdayRank(Api server)
        {
            server.db.Call(new QueryJjcSaveYesterdayRank(), (ret) => { });
        }
    }
}
