using Dota2.GC;
using Dota2.GC.Dota;
using SteamKit2;
using SteamKit2.Internal;
using SteamKit2.GC;
using SteamKit2.GC.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SteamWebApi.Steam
{
    public class SteamClientHandler
	{
		#region Classes

		private class DebugListener : IDebugListener
		{
			public void WriteLine(string category, string msg)
			{
				Dictionary<string, string> buffer = new Dictionary<string,string>();

				buffer.Add("category", category);
				buffer.Add("message", msg);
				buffer.Add("datetime", DateTime.Now.ToString());

				_errors.Add(_errors.Count.ToString(), buffer);
			}
		}

		#endregion

		#region Variables

		private static CallbackManager CallbackManager;
		private static SteamClient Client;
		private static SteamUser User;
		private static SteamFriends Friends;
        private static DotaGCHandler Dota;
		
		private static bool _connected, _connecting;
		private static Dictionary<string, Dictionary<string, string>> _errors = new Dictionary<string, Dictionary<string, string>>();

		private static string _username, _password;

		private static Dictionary<string, string[]> _replays = new Dictionary<string,string[]>();

		#endregion

		#region Properties

		public string Username
		{
			set
			{
				_username = value;
			}
		}

		public string Password
		{
			set
			{
				_password = value;
			}
		}

		public bool Connected
		{
			get
			{
				return _connected;
			}
		}

		public bool Connecting
		{
			get
			{
				return _connecting;
			}
		}

		public Dictionary<string, Dictionary<string, string>> Errors
		{
			get
			{
				return _errors;
			}
		}

        public SteamClient SteamClient
        {
            get
            {
                return Client;
            }
        }

		public Dictionary<string, string[]> Replays
		{
			get
			{
				return _replays;
			}
		}

		#endregion

		public SteamClientHandler()
		{
			// Initialize
			Client = new SteamClient(ProtocolType.Tcp);
            CallbackManager = new CallbackManager(Client);
            DotaGCHandler.Bootstrap(Client);
			User = Client.GetHandler<SteamUser>();
            Friends = Client.GetHandler<SteamFriends>();
            Dota = Client.GetHandler<DotaGCHandler>();
			DebugLog.AddListener(new DebugListener());
			DebugLog.Enabled = true;

            _username = "dotalabsbot";
            _password = "Cooldogbro1";
            Connect();
		}

		/// <summary>
		/// Used to connect to the Steam client
		/// </summary>
		/// <returns>True if it connected, otherwise false</returns>
		public void Connect()
		{
			if (!_connected && !_connecting && !String.IsNullOrEmpty(_username) && !String.IsNullOrEmpty(_password))
			{
				// Register client handlers
				CallbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
				CallbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

				// Register user handlers
				CallbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
				CallbackManager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
				CallbackManager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);

				// Register friend handlers
				CallbackManager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);
				CallbackManager.Subscribe<SteamFriends.PersonaStateCallback>(OnPersonaState);
				CallbackManager.Subscribe<SteamFriends.FriendAddedCallback>(OnFriendAdded);

				CallbackManager.Subscribe<DotaGCHandler.MatchResultResponse>(OnMatchDownload);

				// Attempt to load previously used servers
                if (File.Exists("servers.bin"))
                {
                    ReadServerFile();
                }
                else
                {
                    int cellid = 0;

                    if (File.Exists("cellid.txt") && int.TryParse(File.ReadAllText("cellid.txt"), out cellid))
                    {
                        var loadServersTask = SteamDirectory.Initialize(cellid);
                        loadServersTask.Wait();

                        if (loadServersTask.IsFaulted)
                        {
                            throw new Exception("Failed to load server list.");
                        }
                    }
                }

				// Connect client
				Client.Connect();
				_connecting = true;

				Thread callbackThread = new Thread(new ThreadStart(HandleCallbacks));
				callbackThread.Start();

				WriteServerFile();
			}
		}

        private void HandleCallbacks()
        {
			while(_connected || _connecting)
            {
                CallbackManager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }

			// Add a message to the log for the disconnection
			Dictionary<string, string> buffer = new Dictionary<string,string>();
			buffer.Add("category", "RetryConnection");
			buffer.Add("message", "Disconnected from Steam. Attempting to reconnect in 30 seconds.");
			buffer.Add("datetime", DateTime.Now.ToString());
			_errors.Add(_errors.Count.ToString(), buffer);

			// Wait 30 seconds and try again
			Thread.Sleep(30000);
			Connect();
        }

        /// <summary>
        /// Get all friends of the current user
        /// </summary>
        /// <returns>A JSON-convertable dictionary with friend IDs</returns>
        public Dictionary<string, SteamID> GetFriendsList()
        {
            Dictionary<string, SteamID> friendsList = new Dictionary<string, SteamID>();
            int friendCount = Friends.GetFriendCount();

            for(int i = 0; i < friendCount; i++)
            {
                SteamID steamIdFriend = Friends.GetFriendByIndex(i);
                friendsList.Add(steamIdFriend.AccountID.ToString(), steamIdFriend);
            }

            return friendsList;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        public Dictionary<string, string> CheckDotaConnection()
        {
            Dictionary<string, string> returnMessage = new Dictionary<string, string>();

            if(Dota.Ready)
            {
                returnMessage.Add("connected", "game coordinator is ready");
            }
            else
            {
                returnMessage.Add("error", "game coordinator is not connected");
            }

            return returnMessage;
        }

		/// <summary>
		/// Sends a request to the client for a match's data
		/// </summary>
		/// <param name="matchId">The ID of the requested match</param>
		public void RequestDotaReplay(ulong matchId)
		{
			Dota.RequestMatchResult(matchId);
		}

		#region Server File

		/// <summary>
		/// Reads the list of servers into CMClient
		/// </summary>
		private void ReadServerFile()
		{
			try
			{
				using(var fs = File.OpenRead("servers.bin"))
				using(var reader = new BinaryReader(fs))
				{
					while(fs.Position < fs.Length)
					{
						var numAddressBytes = reader.ReadInt32();
						var addressBytes = reader.ReadBytes(numAddressBytes);
						var port = reader.ReadInt32();
						var ipaddress = new IPAddress(addressBytes);
						var endPoint = new IPEndPoint(ipaddress, port);

						CMClient.Servers.TryAdd(endPoint);
					}
				}
			}
			catch(Exception)
			{}
		}

		/// <summary>
		/// Writes the last-used server to a file
		/// </summary>
		private void WriteServerFile()
		{
			try
			{
				using(var fs = File.OpenWrite("servers.bin"))
				using(var writer = new BinaryWriter(fs))
				{
					foreach(var endPoint in CMClient.Servers.GetAllEndPoints())
					{
						var addressBytes = endPoint.Address.GetAddressBytes();
						writer.Write(addressBytes.Length);
						writer.Write(addressBytes);
						writer.Write(endPoint.Port);
					}
				}
			}
			catch(Exception)
			{ }
		}

		#endregion

		#region Event handlers

		/// <summary>
		/// Client connected
		/// </summary>
		private static void OnConnected(SteamClient.ConnectedCallback callback)
		{
			if(callback.Result != EResult.OK)
			{
				_connected = false;
			}
			else
			{
				_connected = true;
				_connecting = false;
				User.LogOn(new SteamUser.LogOnDetails
				{
					Username = _username,
					Password = _password
				});
			}
		}

		/// <summary>
		/// Client disconnected
		/// </summary>
		private static void OnDisconnected(SteamClient.DisconnectedCallback callback)
		{
			_connected = false;
			_connecting = false;
		}

		/// <summary>
		/// Performed after user has logged on
		/// </summary>
		/// <param name="callback"></param>
		private static void OnLoggedOn(SteamUser.LoggedOnCallback callback)
		{
			if(callback.Result != EResult.OK)
			{
				_connected = false;
			}
			else
			{
				File.WriteAllText("cellid.txt", callback.CellID.ToString());
                Dota.Start();
			}
		}

		/// <summary>
		/// Performed after user has logged off
		/// </summary>
		/// <param name="callback"></param>
		private static void OnLoggedOff(SteamUser.LoggedOffCallback callback)
		{
			// Hit after we've logged off
		}

		/// <summary>
		/// Account info is returned by the client immediately after a successful user logon.
		/// </summary>
		private static void OnAccountInfo(SteamUser.AccountInfoCallback callback)
		{
			Friends.SetPersonaState(EPersonaState.Online);
		}

		/// <summary>
		/// Friends list data is automatically pulled down by the client after a successful user logon.
		/// </summary>
		private static void OnFriendsList(SteamFriends.FriendsListCallback callback)
		{
			foreach(var friend in callback.FriendList)
			{
				if(friend.Relationship == EFriendRelationship.RequestRecipient)
				{
					Friends.AddFriend(friend.SteamID);
				}
			}
		}

		/// <summary>
		/// Hit when we accept a friend request, or someone else has accepted ours
		/// </summary>
		private static void OnFriendAdded(SteamFriends.FriendAddedCallback callback)
		{
		}

		/// <summary>
		/// Hit when a friend changes their persona state
		/// </summary>
		private static void OnPersonaState(SteamFriends.PersonaStateCallback callback)
		{
		}

		/// <summary>
		/// Adds the match data sent by the client to an array
		/// </summary>
		private static void OnMatchDownload(DotaGCHandler.MatchResultResponse callback)
		{
			string[] replayData = new string[2];

			replayData[0] = callback.result.match.cluster.ToString();
			replayData[1] = callback.result.match.replay_salt.ToString();

			_replays.Add(callback.result.match.match_id.ToString(), replayData);
		}

		#endregion
    }
}
