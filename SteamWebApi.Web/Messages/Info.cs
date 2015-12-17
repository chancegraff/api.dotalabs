using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SteamWebApi.Web.Messages
{
	public class Info
	{
		public static Dictionary<string, string> SteamConnecting
		{
			get
			{
				return new Dictionary<string, string>()
				{
					{"wait", "connecting to steam"}
				};
			}
		}
	}
}