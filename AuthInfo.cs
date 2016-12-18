using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yashbot
{
    struct AuthInfo
    {
        public string Username;
        public string SteamId;
        public string SteamTicket; //from Steam
        public string Session; //from as2

        public AuthInfo(string steamId, string steamTicket, string session)
        {
            Username = "yashbot";
            SteamId = steamId;
            SteamTicket = steamTicket;
            Session = session;
        }
    }
}
