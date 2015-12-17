using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SteamWebApi.Web.Messages
{
	public class Errors
    {
        public static Dictionary<string, string> SteamConnectionFailed
        {
            get
            {
                return new Dictionary<string, string>()
				{
					{"error", "unable to connect to steam"}
				};
            }
        }

        public static Dictionary<string, string> SteamNotConnected
        {
            get
            {
                return new Dictionary<string, string>()
				{
					{"error", "not yet connected to steam"}
				};
            }
        }
	}
}