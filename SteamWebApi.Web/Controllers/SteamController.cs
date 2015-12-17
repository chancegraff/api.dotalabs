using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SteamWebApi.Steam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SteamWebApi.Web.Controllers
{
    [RoutePrefix("steam")]
    public class SteamController : ApiController
    {
		private static SteamClientHandler Client = new SteamClientHandler();

        [System.Web.Http.HttpGet]
        [Route("connected")]
		public JObject Connected()
		{
			JObject message;

            if (Client.Connected)
			{
                message = JObject.FromObject(Messages.Successes.SteamConnected);
			}
			else
			{
				if(Client.Connecting)
				{
					message = JObject.FromObject(Messages.Info.SteamConnecting);
				}
				else if(Client.Errors.Count > 0)
				{
					message = JObject.FromObject(Client.Errors);
				}
				else
				{
					message = JObject.FromObject(Messages.Errors.SteamConnectionFailed);
				}
			}

			return message;
		}

		[System.Web.Http.HttpGet]
		[Route("friends")]
		public JObject FriendsList()
		{
			JObject message;

			if(Client.Connected)
			{
				message = JObject.FromObject(Client.GetFriendsList());
			}
			else
			{
				message = JObject.FromObject(Messages.Errors.SteamNotConnected);
			}

			return message;
		}

        [System.Web.Http.HttpGet]
        [Route("dota/connected")]
        public JObject DotaConnected()
        {
			JObject message;

            if (Client.Connected)
			{
				message = JObject.FromObject(Client.CheckDotaConnection());
			}
			else
			{
				message = JObject.FromObject(Messages.Errors.SteamNotConnected);
			}

			return message;
		}

		[System.Web.Http.HttpGet]
		[Route("dota/replays/{matchId}")]
		public JObject RequestReplayData(ulong matchId)
		{
			JObject message;

			if(Client.Connected)
			{
				Client.RequestDotaReplay(matchId);
				message = JObject.FromObject(Messages.Successes.ReplayRequested);
			}
			else
			{
				message = JObject.FromObject(Messages.Errors.SteamNotConnected);
			}

			return message;
		}

		[System.Web.Http.HttpGet]
		[Route("dota/replays")]
		public JObject GetReplayList()
		{
			JObject message;

			if(Client.Connected)
			{
				return JObject.FromObject(Client.Replays);
			}
			else
			{
				message = JObject.FromObject(Messages.Errors.SteamNotConnected);
			}

			return message;
		}

		[System.Web.Http.HttpGet]
		[Route("dota/profiles/{accountId}")]
		public JObject RequestProfileData(uint accountId)
		{
			JObject message;

			if(Client.Connected)
			{
				Client.RequestDotaProfile(accountId);
				message = JObject.FromObject(Messages.Successes.ProfileRequested);
			}
			else
			{
				message = JObject.FromObject(Messages.Errors.SteamNotConnected);
			}

			return message;
		}

		[System.Web.Http.HttpGet]
		[Route("dota/profiles")]
		public JObject GetProfileList()
		{
			JObject message;

			if(Client.Connected)
			{
				return JObject.FromObject(Client.Profiles);
			}
			else
			{
				message = JObject.FromObject(Messages.Errors.SteamNotConnected);
			}

			return message;
		}
    }
}
