using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SteamWebApi.Steam.Messages
{
	public class Successes
	{
		public static Dictionary<string, string> SteamConnected
		{
			get
			{
				return new Dictionary<string, string>()
				{
					{"success", "connected to steam"}
				};
			}
		}

		public static Dictionary<string, string> ReplayRequested
		{
			get
			{
				return new Dictionary<string, string>()
				{
					{"success", "replay has been requested"}
				};
			}
		}

		public static Dictionary<string, string> ProfileRequested
		{
			get
			{
				return new Dictionary<string, string>()
				{
					{"success", "profile has been requested"}
				};
			}
		}
	}
}