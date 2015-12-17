using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamWebApi.Steam;
using Dota2;
using Dota2.GC;

namespace SteamWebApi.Dota
{
    public class DotaClientHandler
    {
        private static SteamClientHandler Client;

        public DotaClientHandler(SteamClientHandler client)
        {
            Client = client;
        }

        private void Initialize()
        {
            DotaGCHandler.Bootstrap(Client.SteamClient);
            dota = Client.SteamClient.GetHandler<DotaGCHandler>();

            // When Steam is connected
            
        }
    }
}
