using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SteamWebApi.Steam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SteamWebApi.Web.Controllers
{
    [System.Web.Http.RoutePrefix("steam")]
    public class SteamController : ApiController
    {
		private SteamClientHandler Client
		{
			get
			{
				return (SteamClientHandler) HttpContext.Current.Application["SteamClientHandler"];
			}
			set
			{
				HttpContext.Current.Application["SteamClientHandler"] = value;
			}
		}

		public SteamController()
		{
			if (Client == null)
			{
				Client = new SteamClientHandler();
			}
		}

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("connected")]
		public JObject Connected()
		{
			try
			{
				Client.Connect();
				return Client.CheckSteamConnection();
			}
			catch(Exception e)
			{
				return JObject.FromObject(e);
			}
		}

		[System.Web.Http.HttpGet]
		[System.Web.Http.Route("friends")]
		public JObject FriendsList()
		{
			try
			{
				return Client.GetFriendsList();
			}
			catch(Exception e)
			{
				return JObject.FromObject(e);
			}
		}

		[System.Web.Http.HttpGet]
		[System.Web.Http.Route("dota/connected")]
		public JObject DotaConnected()
		{
			try
			{
				return Client.CheckDotaConnection();
			}
			catch(Exception e)
			{
				return JObject.FromObject(e);
			}
		}

		//[System.Web.Http.HttpGet]
		//[System.Web.Http.Route("dota/replays/{matchId}")]
		//public JObject RequestReplayData(ulong matchId)
		//{
		//	JObject message;

		//	if(Client.Connected)
		//	{
		//		Client.RequestDotaReplay(matchId);
		//		message = JObject.FromObject(Messages.Successes.ReplayRequested);
		//	}
		//	else
		//	{
		//		message = JObject.FromObject(Messages.Errors.SteamNotConnected);
		//	}

		//	return message;
		//}

		//[System.Web.Http.HttpGet]
		//[System.Web.Http.Route("dota/replays")]
		//public JObject GetReplayList()
		//{
		//	JObject message;

		//	if(Client.Connected)
		//	{
		//		return JObject.FromObject(Client.Replays);
		//	}
		//	else
		//	{
		//		message = JObject.FromObject(Messages.Errors.SteamNotConnected);
		//	}

		//	return message;
		//}

		//[System.Web.Http.HttpGet]
		//[System.Web.Http.Route("dota/profiles/{accountId}")]
		//public JObject RequestProfileData(uint accountId)
		//{
		//	JObject message;

		//	if(Client.Connected)
		//	{
		//		Client.RequestDotaProfile(accountId);
		//		message = JObject.FromObject(Messages.Successes.ProfileRequested);
		//	}
		//	else
		//	{
		//		message = JObject.FromObject(Messages.Errors.SteamNotConnected);
		//	}

		//	return message;
		//}

		//[System.Web.Http.HttpGet]
		//[System.Web.Http.Route("dota/profiles")]
		//public JObject GetProfileList()
		//{
		//	JObject message;

		//	if(Client.Connected)
		//	{
		//		return JObject.FromObject(Client.Profiles);
		//	}
		//	else
		//	{
		//		message = JObject.FromObject(Messages.Errors.SteamNotConnected);
		//	}

		//	return message;
		//}
    }
}
