using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SteamWebApi.Web.Messages
{
	public class Successes
	{
		public static Dictionary<string, string> SteamConnected
		{
			get
			{
				return new Dictionary<string, string>()
				{
					{"success", "now connected to steam!"}
				};
			}
		}

		public static Dictionary<string, string> ReplayRequested
		{
			get
			{
				return new Dictionary<string,string>()
				{
					{"success", "replay has been requested"}
				};
			}
		}
	}
}