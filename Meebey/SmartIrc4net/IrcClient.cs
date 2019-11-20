﻿// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.IrcClient
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Threading;

namespace Meebey.SmartIrc4net
{
	public class IrcClient : IrcCommands
	{
		private static Regex _ReplyCodeRegex = new Regex("^:[^ ]+? ([0-9]{3}) .+$", RegexOptions.Compiled);
		private static Regex _PingRegex = new Regex("^PING :.*", RegexOptions.Compiled);
		private static Regex _ErrorRegex = new Regex("^ERROR :.*", RegexOptions.Compiled);
		private static Regex _ActionRegex = new Regex("^:.*? PRIVMSG (.).* :\x0001ACTION .*\x0001$", RegexOptions.Compiled);
		private static Regex _CtcpRequestRegex = new Regex("^:.*? PRIVMSG .* :\x0001.*\x0001$", RegexOptions.Compiled);
		private static Regex _MessageRegex = new Regex("^:.*? PRIVMSG (.).* :.*$", RegexOptions.Compiled);
		private static Regex _CtcpReplyRegex = new Regex("^:.*? NOTICE .* :\x0001.*\x0001$", RegexOptions.Compiled);
		private static Regex _NoticeRegex = new Regex("^:.*? NOTICE (.).* :.*$", RegexOptions.Compiled);
		private static Regex _InviteRegex = new Regex("^:.*? INVITE .* .*$", RegexOptions.Compiled);
		private static Regex _JoinRegex = new Regex("^:.*? JOIN .*$", RegexOptions.Compiled);
		private static Regex _TopicRegex = new Regex("^:.*? TOPIC .* :.*$", RegexOptions.Compiled);
		private static Regex _NickRegex = new Regex("^:.*? NICK .*$", RegexOptions.Compiled);
		private static Regex _KickRegex = new Regex("^:.*? KICK .* .*$", RegexOptions.Compiled);
		private static Regex _PartRegex = new Regex("^:.*? PART .*$", RegexOptions.Compiled);
		private static Regex _ModeRegex = new Regex("^:.*? MODE (.*) .*$", RegexOptions.Compiled);
		private static Regex _QuitRegex = new Regex("^:.*? QUIT :.*$", RegexOptions.Compiled);
		private string _Nickname = string.Empty;
		private string _Realname = string.Empty;
		private string _Usermode = string.Empty;
		private string _Username = string.Empty;
		private string _Password = string.Empty;
		private StringDictionary _AutoRejoinChannels = new StringDictionary();
		private bool _AutoNickHandling = true;
		private StringCollection _Motd = new StringCollection();
		private Array _ReplyCodes = Enum.GetValues(typeof(ReplyCode));
		private StringCollection _JoinedChannels = new StringCollection();
		private Hashtable _Channels = Hashtable.Synchronized(new Hashtable((IHashCodeProvider)new CaseInsensitiveHashCodeProvider(), (IComparer)new CaseInsensitiveComparer()));
		private Hashtable _IrcUsers = Hashtable.Synchronized(new Hashtable((IHashCodeProvider)new CaseInsensitiveHashCodeProvider(), (IComparer)new CaseInsensitiveComparer()));
		private object _ChannelListSyncRoot = new object();
		private object _WhoListSyncRoot = new object();
		private object _BanListSyncRoot = new object();
		private string[] _NicknameList;
		private int _CurrentNickname;
		private int _IUsermode;
		private bool _IsAway;
		private string _CtcpVersion;
		private bool _ActiveChannelSyncing;
		private bool _PassiveChannelSyncing;
		private bool _AutoJoinOnInvite;
		private bool _AutoRejoin;
		private bool _AutoRejoinChannelsWithKeys;
		private bool _AutoRejoinOnKick;
		private bool _AutoRelogin;
		private bool _SupportNonRfc;
		private bool _SupportNonRfcLocked;
		private bool _MotdReceived;
		private List<ChannelInfo> _ChannelList;
		private AutoResetEvent _ChannelListReceivedEvent;
		private List<WhoInfo> _WhoList;
		private AutoResetEvent _WhoListReceivedEvent;
		private List<BanInfo> _BanList;
		private AutoResetEvent _BanListReceivedEvent;

		public event EventHandler OnRegistered;

		public event PingEventHandler OnPing;

		public event PongEventHandler OnPong;

		public event IrcEventHandler OnRawMessage;

		public event ErrorEventHandler OnError;

		public event IrcEventHandler OnErrorMessage;

		public event JoinEventHandler OnJoin;

		public event NamesEventHandler OnNames;

		public event ListEventHandler OnList;

		public event PartEventHandler OnPart;

		public event QuitEventHandler OnQuit;

		public event KickEventHandler OnKick;

		public event AwayEventHandler OnAway;

		public event IrcEventHandler OnUnAway;

		public event IrcEventHandler OnNowAway;

		public event InviteEventHandler OnInvite;

		public event BanEventHandler OnBan;

		public event UnbanEventHandler OnUnban;

		public event OpEventHandler OnOp;

		public event DeopEventHandler OnDeop;

		public event HalfopEventHandler OnHalfop;

		public event DehalfopEventHandler OnDehalfop;

		public event VoiceEventHandler OnVoice;

		public event DevoiceEventHandler OnDevoice;

		public event WhoEventHandler OnWho;

		public event MotdEventHandler OnMotd;

		public event TopicEventHandler OnTopic;

		public event TopicChangeEventHandler OnTopicChange;

		public event NickChangeEventHandler OnNickChange;

		public event IrcEventHandler OnModeChange;

		public event IrcEventHandler OnUserModeChange;

		public event IrcEventHandler OnChannelModeChange;

		public event IrcEventHandler OnChannelMessage;

		public event ActionEventHandler OnChannelAction;

		public event IrcEventHandler OnChannelNotice;

		public event IrcEventHandler OnChannelActiveSynced;

		public event IrcEventHandler OnChannelPassiveSynced;

		public event IrcEventHandler OnQueryMessage;

		public event ActionEventHandler OnQueryAction;

		public event IrcEventHandler OnQueryNotice;

		public event CtcpEventHandler OnCtcpRequest;

		public event CtcpEventHandler OnCtcpReply;

		public bool ActiveChannelSyncing
		{
			get
			{
				return this._ActiveChannelSyncing;
			}
			set
			{
				this._ActiveChannelSyncing = value;
			}
		}

		public bool PassiveChannelSyncing
		{
			get
			{
				return this._PassiveChannelSyncing;
			}
		}

		public string CtcpVersion
		{
			get
			{
				return this._CtcpVersion;
			}
			set
			{
				this._CtcpVersion = value;
			}
		}

		public bool AutoJoinOnInvite
		{
			get
			{
				return this._AutoJoinOnInvite;
			}
			set
			{
				this._AutoJoinOnInvite = value;
			}
		}

		public bool AutoRejoin
		{
			get
			{
				return this._AutoRejoin;
			}
			set
			{
				this._AutoRejoin = value;
			}
		}

		public bool AutoRejoinOnKick
		{
			get
			{
				return this._AutoRejoinOnKick;
			}
			set
			{
				this._AutoRejoinOnKick = value;
			}
		}

		public bool AutoRelogin
		{
			get
			{
				return this._AutoRelogin;
			}
			set
			{
				this._AutoRelogin = value;
			}
		}

		public bool AutoNickHandling
		{
			get
			{
				return this._AutoNickHandling;
			}
			set
			{
				this._AutoNickHandling = value;
			}
		}

		public bool SupportNonRfc
		{
			get
			{
				return this._SupportNonRfc;
			}
			set
			{
				if (this._SupportNonRfcLocked)
					return;
				this._SupportNonRfc = value;
			}
		}

		public string Nickname
		{
			get
			{
				return this._Nickname;
			}
		}

		public string[] NicknameList
		{
			get
			{
				return this._NicknameList;
			}
		}

		public string Realname
		{
			get
			{
				return this._Realname;
			}
		}

		public string Username
		{
			get
			{
				return this._Username;
			}
		}

		public string Usermode
		{
			get
			{
				return this._Usermode;
			}
		}

		public int IUsermode
		{
			get
			{
				return this._IUsermode;
			}
		}

		public bool IsAway
		{
			get
			{
				return this._IsAway;
			}
		}

		public string Password
		{
			get
			{
				return this._Password;
			}
		}

		public StringCollection JoinedChannels
		{
			get
			{
				return this._JoinedChannels;
			}
		}

		public StringCollection Motd
		{
			get
			{
				return this._Motd;
			}
		}

		public object BanListSyncRoot
		{
			get
			{
				return this._BanListSyncRoot;
			}
		}

		public IrcClient()
		{
			this.OnReadLine += new ReadLineEventHandler(this._Worker);
			this.OnDisconnected += new EventHandler(this._OnDisconnected);
			this.OnConnectionError += new EventHandler(this._OnConnectionError);
		}

		public new void Connect(string[] addresslist, int port)
		{
			this._SupportNonRfcLocked = true;
			base.Connect(addresslist, port);
		}

		public void Reconnect(bool login, bool channels)
		{
			if (channels)
				this._StoreChannelsToRejoin();
			this.Reconnect();
			if (login)
			{
				this._CurrentNickname = 0;
				this.Login(this._NicknameList, this.Realname, this.IUsermode, this.Username, this.Password);
			}
			if (!channels)
				return;
			this._RejoinChannels();
		}

		public void Reconnect(bool login)
		{
			this.Reconnect(login, true);
		}

		public void Login(
		  string[] nicklist,
		  string realname,
		  int usermode,
		  string username,
		  string password)
		{
			this._NicknameList = (string[])nicklist.Clone();
			this._Nickname = this._NicknameList[0].Replace(" ", "");
			this._Realname = realname;
			this._IUsermode = usermode;
			this._Username = username == null || username.Length <= 0 ? Environment.UserName.Replace(" ", "") : username.Replace(" ", "");
			if (password != null && password.Length > 0)
			{
				this._Password = password;
				this.RfcPass(this.Password, Priority.Critical);
			}
			this.RfcNick(this.Nickname, Priority.Critical);
			this.RfcUser(this.Username, this.IUsermode, this.Realname, Priority.Critical);
		}

		public void Login(string[] nicklist, string realname, int usermode, string username)
		{
			this.Login(nicklist, realname, usermode, username, "");
		}

		public void Login(string[] nicklist, string realname, int usermode)
		{
			this.Login(nicklist, realname, usermode, "", "");
		}

		public void Login(string[] nicklist, string realname)
		{
			this.Login(nicklist, realname, 0, "", "");
		}

		public void Login(
		  string nick,
		  string realname,
		  int usermode,
		  string username,
		  string password)
		{
			this.Login(new string[3]
			{
		nick,
		nick + "_",
		nick + "__"
			}, realname, usermode, username, password);
		}

		public void Login(string nick, string realname, int usermode, string username)
		{
			this.Login(new string[3]
			{
		nick,
		nick + "_",
		nick + "__"
			}, realname, usermode, username, "");
		}

		public void Login(string nick, string realname, int usermode)
		{
			this.Login(new string[3]
			{
		nick,
		nick + "_",
		nick + "__"
			}, realname, usermode, "", "");
		}

		public void Login(string nick, string realname)
		{
			this.Login(new string[3]
			{
		nick,
		nick + "_",
		nick + "__"
			}, realname, 0, "", "");
		}

		public bool IsMe(string nickname)
		{
			return this.Nickname == nickname;
		}

		public bool IsJoined(string channelname)
		{
			return this.IsJoined(channelname, this.Nickname);
		}

		public bool IsJoined(string channelname, string nickname)
		{
			if (channelname == null)
				throw new ArgumentNullException(nameof(channelname));
			if (nickname == null)
				throw new ArgumentNullException(nameof(nickname));
			Channel channel = this.GetChannel(channelname);
			return channel != null && channel.UnsafeUsers != null && channel.UnsafeUsers.ContainsKey((object)nickname);
		}

		public IrcUser GetIrcUser(string nickname)
		{
			if (nickname == null)
				throw new ArgumentNullException(nameof(nickname));
			return (IrcUser)this._IrcUsers[(object)nickname];
		}

		public ChannelUser GetChannelUser(string channelname, string nickname)
		{
			if (channelname == null)
				throw new ArgumentNullException("channel");
			if (nickname == null)
				throw new ArgumentNullException(nameof(nickname));
			Channel channel = this.GetChannel(channelname);
			if (channel != null)
				return (ChannelUser)channel.UnsafeUsers[(object)nickname];
			return (ChannelUser)null;
		}

		public Channel GetChannel(string channelname)
		{
			if (channelname == null)
				throw new ArgumentNullException(nameof(channelname));
			return (Channel)this._Channels[(object)channelname];
		}

		public string[] GetChannels()
		{
			string[] strArray = new string[this._Channels.Values.Count];
			int num = 0;
			foreach (Channel channel in (IEnumerable)this._Channels.Values)
				strArray[num++] = channel.Name;
			return strArray;
		}

		public IList<ChannelInfo> GetChannelList(string mask)
		{
			List<ChannelInfo> channelInfoList = new List<ChannelInfo>();
			lock (this._ChannelListSyncRoot)
			{
				this._ChannelList = channelInfoList;
				this._ChannelListReceivedEvent = new AutoResetEvent(false);
				this.RfcList(mask);
				this._ChannelListReceivedEvent.WaitOne();
				this._ChannelListReceivedEvent = (AutoResetEvent)null;
				this._ChannelList = (List<ChannelInfo>)null;
			}
			return (IList<ChannelInfo>)channelInfoList;
		}

		public IList<WhoInfo> GetWhoList(string mask)
		{
			List<WhoInfo> whoInfoList = new List<WhoInfo>();
			lock (this._WhoListSyncRoot)
			{
				this._WhoList = whoInfoList;
				this._WhoListReceivedEvent = new AutoResetEvent(false);
				this.RfcWho(mask);
				this._WhoListReceivedEvent.WaitOne();
				this._WhoListReceivedEvent = (AutoResetEvent)null;
				this._WhoList = (List<WhoInfo>)null;
			}
			return (IList<WhoInfo>)whoInfoList;
		}

		public IList<BanInfo> GetBanList(string channel)
		{
			List<BanInfo> banInfoList = new List<BanInfo>();
			lock (this._BanListSyncRoot)
			{
				this._BanList = banInfoList;
				this._BanListReceivedEvent = new AutoResetEvent(false);
				this.Ban(channel);
				this._BanListReceivedEvent.WaitOne();
				this._BanListReceivedEvent = (AutoResetEvent)null;
				this._BanList = (List<BanInfo>)null;
			}
			return (IList<BanInfo>)banInfoList;
		}

		public IrcMessageData MessageParser(string rawline)
		{
			string nick = null;
			string ident = null;
			string host = null;
			string channel = null;
			string message = null;
			string str = rawline.Length <= 0 || rawline[0] != ':' ? rawline : rawline.Substring(1);
			string[] strArray = str.Split(' ');
			string from = strArray[0];
			string s = strArray[1];
			int length = from.IndexOf("!");
			int num1 = from.IndexOf("@");
			int num2 = str.IndexOf(" :");
			if (num2 != -1)
				++num2;
			if (length != -1)
				nick = from.Substring(0, length);
			if (num1 != -1 && length != -1)
				ident = from.Substring(length + 1, num1 - length - 1);
			if (num1 != -1)
				host = from.Substring(num1 + 1);
			int result = 0;
			ReplyCode replycode = ReplyCode.Null;
			if (int.TryParse(s, out result) && Enum.IsDefined(typeof(ReplyCode), (object)result))
				replycode = (ReplyCode)result;
			ReceiveType messageType = this._GetMessageType(rawline);
			if (num2 != -1)
				message = str.Substring(num2 + 1);
			switch (messageType)
			{
				case ReceiveType.Join:
				case ReceiveType.Kick:
				case ReceiveType.Part:
				case ReceiveType.TopicChange:
				case ReceiveType.ChannelModeChange:
				case ReceiveType.ChannelMessage:
				case ReceiveType.ChannelAction:
				case ReceiveType.ChannelNotice:
					channel = strArray[2];
					break;
				case ReceiveType.Invite:
				case ReceiveType.Who:
				case ReceiveType.Topic:
				case ReceiveType.BanList:
				case ReceiveType.ChannelMode:
					channel = strArray[3];
					break;
				case ReceiveType.Name:
					channel = strArray[4];
					break;
			}
			switch (replycode)
			{
				case ReplyCode.List:
				case ReplyCode.ListEnd:
				case ReplyCode.ErrorNoChannelModes:
					channel = strArray[3];
					break;
			}
			if (channel != null && channel[0] == ':')
				channel = channel.Substring(1);
			return new IrcMessageData(this, from, nick, ident, host, channel, message, rawline, messageType, replycode);
		}

		protected virtual IrcUser CreateIrcUser(string nickname)
		{
			return new IrcUser(nickname, this);
		}

		protected virtual Channel CreateChannel(string name)
		{
			if (this._SupportNonRfc)
				return (Channel)new NonRfcChannel(name);
			return new Channel(name);
		}

		protected virtual ChannelUser CreateChannelUser(string channel, IrcUser ircUser)
		{
			if (this._SupportNonRfc)
				return (ChannelUser)new NonRfcChannelUser(channel, ircUser);
			return new ChannelUser(channel, ircUser);
		}

		private void _Worker(object sender, ReadLineEventArgs e)
		{
			this._HandleEvents(this.MessageParser(e.Line));
		}

		private void _OnDisconnected(object sender, EventArgs e)
		{
			if (this.AutoRejoin)
				this._StoreChannelsToRejoin();
			this._SyncingCleanup();
		}

		private void _OnConnectionError(object sender, EventArgs e)
		{
			try
			{
				if (this.AutoReconnect && this.AutoRelogin)
					this.Login(this._NicknameList, this.Realname, this.IUsermode, this.Username, this.Password);
				if (!this.AutoReconnect || !this.AutoRejoin)
					return;
				this._RejoinChannels();
			}
			catch (NotConnectedException ex)
			{
			}
		}

		private void _StoreChannelsToRejoin()
		{
			this._AutoRejoinChannels.Clear();
			if (this.ActiveChannelSyncing || this.PassiveChannelSyncing)
			{
				foreach (Channel channel in (IEnumerable)this._Channels.Values)
				{
					if (channel.Key.Length > 0)
					{
						this._AutoRejoinChannels.Add(channel.Name, channel.Key);
						this._AutoRejoinChannelsWithKeys = true;
					}
					else
						this._AutoRejoinChannels.Add(channel.Name, "nokey");
				}
			}
			else
			{
				foreach (string joinedChannel in this._JoinedChannels)
					this._AutoRejoinChannels.Add(joinedChannel, "nokey");
			}
		}

		private void _RejoinChannels()
		{
			int count = this._AutoRejoinChannels.Count;
			string[] channels = new string[count];
			this._AutoRejoinChannels.Keys.CopyTo((Array)channels, 0);
			if (this._AutoRejoinChannelsWithKeys)
			{
				string[] keys = new string[count];
				this._AutoRejoinChannels.Values.CopyTo((Array)keys, 0);
				this.RfcJoin(channels, keys, Priority.High);
			}
			else
				this.RfcJoin(channels, Priority.High);
			this._AutoRejoinChannelsWithKeys = false;
			this._AutoRejoinChannels.Clear();
		}

		private void _SyncingCleanup()
		{
			this._JoinedChannels.Clear();
			if (this.ActiveChannelSyncing)
			{
				this._Channels.Clear();
				this._IrcUsers.Clear();
			}
			this._IsAway = false;
			this._MotdReceived = false;
			this._Motd.Clear();
		}

		private string _NextNickname()
		{
			++this._CurrentNickname;
			if (this._CurrentNickname >= this._NicknameList.Length)
				--this._CurrentNickname;
			return this.NicknameList[this._CurrentNickname];
		}

		private ReceiveType _GetMessageType(string rawline)
		{
			Match match1 = IrcClient._ReplyCodeRegex.Match(rawline);
			if (match1.Success)
			{
				ReplyCode replyCode = (ReplyCode)int.Parse(match1.Groups[1].Value);
				if (Array.IndexOf(this._ReplyCodes, (object)replyCode) == -1)
					return ReceiveType.Unknown;
				switch (replyCode)
				{
					case ReplyCode.Welcome:
					case ReplyCode.YourHost:
					case ReplyCode.Created:
					case ReplyCode.MyInfo:
					case ReplyCode.Bounce:
						return ReceiveType.Login;
					case ReplyCode.UserModeIs:
						return ReceiveType.UserMode;
					case ReplyCode.LuserClient:
					case ReplyCode.LuserOp:
					case ReplyCode.LuserUnknown:
					case ReplyCode.LuserChannels:
					case ReplyCode.LuserMe:
						return ReceiveType.Info;
					case ReplyCode.WhoIsUser:
					case ReplyCode.WhoIsServer:
					case ReplyCode.WhoIsOperator:
					case ReplyCode.WhoIsIdle:
					case ReplyCode.EndOfWhoIs:
					case ReplyCode.WhoIsChannels:
						return ReceiveType.WhoIs;
					case ReplyCode.WhoWasUser:
					case ReplyCode.EndOfWhoWas:
						return ReceiveType.WhoWas;
					case ReplyCode.EndOfWho:
					case ReplyCode.WhoReply:
						return ReceiveType.Who;
					case ReplyCode.ListStart:
					case ReplyCode.List:
					case ReplyCode.ListEnd:
						return ReceiveType.List;
					case ReplyCode.ChannelModeIs:
						return ReceiveType.ChannelMode;
					case ReplyCode.NoTopic:
					case ReplyCode.Topic:
						return ReceiveType.Topic;
					case ReplyCode.NamesReply:
					case ReplyCode.EndOfNames:
						return ReceiveType.Name;
					case ReplyCode.BanList:
					case ReplyCode.EndOfBanList:
						return ReceiveType.BanList;
					case ReplyCode.Motd:
					case ReplyCode.MotdStart:
					case ReplyCode.EndOfMotd:
						return ReceiveType.Motd;
					default:
						return replyCode >= (ReplyCode)400 && replyCode <= (ReplyCode)599 ? ReceiveType.ErrorMessage : ReceiveType.Unknown;
				}
			}
			else
			{
				if (IrcClient._PingRegex.Match(rawline).Success)
					return ReceiveType.Unknown;
				if (IrcClient._ErrorRegex.Match(rawline).Success)
					return ReceiveType.Error;
				Match match2 = IrcClient._ActionRegex.Match(rawline);
				if (match2.Success)
				{
					switch (match2.Groups[1].Value)
					{
						case "#":
						case "!":
						case "&":
						case "+":
							return ReceiveType.ChannelAction;
						default:
							return ReceiveType.QueryAction;
					}
				}
				else
				{
					if (IrcClient._CtcpRequestRegex.Match(rawline).Success)
						return ReceiveType.CtcpRequest;
					Match match3 = IrcClient._MessageRegex.Match(rawline);
					if (match3.Success)
					{
						switch (match3.Groups[1].Value)
						{
							case "#":
							case "!":
							case "&":
							case "+":
								return ReceiveType.ChannelMessage;
							default:
								return ReceiveType.QueryMessage;
						}
					}
					else
					{
						if (IrcClient._CtcpReplyRegex.Match(rawline).Success)
							return ReceiveType.CtcpReply;
						Match match4 = IrcClient._NoticeRegex.Match(rawline);
						if (match4.Success)
						{
							switch (match4.Groups[1].Value)
							{
								case "#":
								case "!":
								case "&":
								case "+":
									return ReceiveType.ChannelNotice;
								default:
									return ReceiveType.QueryNotice;
							}
						}
						else
						{
							if (IrcClient._InviteRegex.Match(rawline).Success)
								return ReceiveType.Invite;
							if (IrcClient._JoinRegex.Match(rawline).Success)
								return ReceiveType.Join;
							if (IrcClient._TopicRegex.Match(rawline).Success)
								return ReceiveType.TopicChange;
							if (IrcClient._NickRegex.Match(rawline).Success)
								return ReceiveType.NickChange;
							if (IrcClient._KickRegex.Match(rawline).Success)
								return ReceiveType.Kick;
							if (IrcClient._PartRegex.Match(rawline).Success)
								return ReceiveType.Part;
							Match match5 = IrcClient._ModeRegex.Match(rawline);
							if (match5.Success)
								return match5.Groups[1].Value == this._Nickname ? ReceiveType.UserModeChange : ReceiveType.ChannelModeChange;
							return IrcClient._QuitRegex.Match(rawline).Success ? ReceiveType.Quit : ReceiveType.Unknown;
						}
					}
				}
			}
		}

		private void _HandleEvents(IrcMessageData ircdata)
		{
			if (this.OnRawMessage != null)
				this.OnRawMessage((object)this, new IrcEventArgs(ircdata));
			switch (ircdata.RawMessageArray[0])
			{
				case "PING":
					this._Event_PING(ircdata);
					break;
				case "ERROR":
					this._Event_ERROR(ircdata);
					break;
			}
			switch (ircdata.RawMessageArray[1])
			{
				case "PRIVMSG":
					this._Event_PRIVMSG(ircdata);
					break;
				case "NOTICE":
					this._Event_NOTICE(ircdata);
					break;
				case "JOIN":
					this._Event_JOIN(ircdata);
					break;
				case "PART":
					this._Event_PART(ircdata);
					break;
				case "KICK":
					this._Event_KICK(ircdata);
					break;
				case "QUIT":
					this._Event_QUIT(ircdata);
					break;
				case "TOPIC":
					this._Event_TOPIC(ircdata);
					break;
				case "NICK":
					this._Event_NICK(ircdata);
					break;
				case "INVITE":
					this._Event_INVITE(ircdata);
					break;
				case "MODE":
					this._Event_MODE(ircdata);
					break;
				case "PONG":
					this._Event_PONG(ircdata);
					break;
			}
			if (ircdata.ReplyCode != ReplyCode.Null)
			{
				switch (ircdata.ReplyCode)
				{
					case ReplyCode.Welcome:
						this._Event_RPL_WELCOME(ircdata);
						break;
					case ReplyCode.TryAgain:
						this._Event_RPL_TRYAGAIN(ircdata);
						break;
					case ReplyCode.Away:
						this._Event_RPL_AWAY(ircdata);
						break;
					case ReplyCode.UnAway:
						this._Event_RPL_UNAWAY(ircdata);
						break;
					case ReplyCode.NowAway:
						this._Event_RPL_NOWAWAY(ircdata);
						break;
					case ReplyCode.EndOfWho:
						this._Event_RPL_ENDOFWHO(ircdata);
						break;
					case ReplyCode.List:
						this._Event_RPL_LIST(ircdata);
						break;
					case ReplyCode.ListEnd:
						this._Event_RPL_LISTEND(ircdata);
						break;
					case ReplyCode.ChannelModeIs:
						this._Event_RPL_CHANNELMODEIS(ircdata);
						break;
					case ReplyCode.NoTopic:
						this._Event_RPL_NOTOPIC(ircdata);
						break;
					case ReplyCode.Topic:
						this._Event_RPL_TOPIC(ircdata);
						break;
					case ReplyCode.WhoReply:
						this._Event_RPL_WHOREPLY(ircdata);
						break;
					case ReplyCode.NamesReply:
						this._Event_RPL_NAMREPLY(ircdata);
						break;
					case ReplyCode.EndOfNames:
						this._Event_RPL_ENDOFNAMES(ircdata);
						break;
					case ReplyCode.BanList:
						this._Event_RPL_BANLIST(ircdata);
						break;
					case ReplyCode.EndOfBanList:
						this._Event_RPL_ENDOFBANLIST(ircdata);
						break;
					case ReplyCode.Motd:
						this._Event_RPL_MOTD(ircdata);
						break;
					case ReplyCode.EndOfMotd:
						this._Event_RPL_ENDOFMOTD(ircdata);
						break;
					case ReplyCode.ErrorNicknameInUse:
						this._Event_ERR_NICKNAMEINUSE(ircdata);
						break;
					case ReplyCode.ErrorNoChannelModes:
						this._Event_ERR_NOCHANMODES(ircdata);
						break;
				}
			}
			if (ircdata.Type != ReceiveType.ErrorMessage)
				return;
			this._Event_ERR(ircdata);
		}

		private bool _RemoveIrcUser(string nickname)
		{
			if (this.GetIrcUser(nickname).JoinedChannels.Length != 0)
				return false;
			this._IrcUsers.Remove((object)nickname);
			return true;
		}

		private void _RemoveChannelUser(string channelname, string nickname)
		{
			Channel channel = this.GetChannel(channelname);
			channel.UnsafeUsers.Remove((object)nickname);
			channel.UnsafeOps.Remove((object)nickname);
			channel.UnsafeVoices.Remove((object)nickname);
			if (!this.SupportNonRfc)
				return;
			((NonRfcChannel)channel).UnsafeHalfops.Remove((object)nickname);
		}

		private void _InterpretChannelMode(IrcMessageData ircdata, string mode, string parameter)
		{
			string[] strArray = parameter.Split(' ');
			bool flag1 = false;
			bool flag2 = false;
			int length = mode.Length;
			Channel channel = (Channel)null;
			if (this.ActiveChannelSyncing)
				channel = this.GetChannel(ircdata.Channel);
			IEnumerator enumerator = strArray.GetEnumerator();
			enumerator.MoveNext();
			for (int index = 0; index < length; ++index)
			{
				switch (mode[index])
				{
					case '+':
						flag1 = true;
						flag2 = false;
						break;
					case '-':
						flag1 = false;
						flag2 = true;
						break;
					case 'b':
						string current1 = enumerator.Current as string;
						enumerator.MoveNext();
						if (flag1)
						{
							if (this.ActiveChannelSyncing)
							{
								try
								{
									channel.Bans.Add(current1);
								}
								catch (ArgumentException ex)
								{
								}
							}
							if (this.OnBan != null)
								this.OnBan((object)this, new BanEventArgs(ircdata, ircdata.Channel, ircdata.Nick, current1));
						}
						if (flag2)
						{
							if (this.ActiveChannelSyncing)
								channel.Bans.Remove(current1);
							if (this.OnUnban != null)
							{
								this.OnUnban((object)this, new UnbanEventArgs(ircdata, ircdata.Channel, ircdata.Nick, current1));
								break;
							}
							break;
						}
						break;
					case 'h':
						if (this.SupportNonRfc)
						{
							string current2 = enumerator.Current as string;
							enumerator.MoveNext();
							if (flag1)
							{
								if (this.ActiveChannelSyncing)
								{
									if (this.GetChannelUser(ircdata.Channel, current2) != null)
									{
										try
										{
											((NonRfcChannel)channel).UnsafeHalfops.Add((object)current2, (object)this.GetIrcUser(current2));
										}
										catch (ArgumentException ex)
										{
										}
									  ((NonRfcChannelUser)this.GetChannelUser(ircdata.Channel, current2)).IsHalfop = true;
									}
								}
								if (this.OnHalfop != null)
									this.OnHalfop((object)this, new HalfopEventArgs(ircdata, ircdata.Channel, ircdata.Nick, current2));
							}
							if (flag2)
							{
								if (this.ActiveChannelSyncing && this.GetChannelUser(ircdata.Channel, current2) != null)
								{
									((NonRfcChannel)channel).UnsafeHalfops.Remove((object)current2);
									((NonRfcChannelUser)this.GetChannelUser(ircdata.Channel, current2)).IsHalfop = false;
								}
								if (this.OnDehalfop != null)
								{
									this.OnDehalfop((object)this, new DehalfopEventArgs(ircdata, ircdata.Channel, ircdata.Nick, current2));
									break;
								}
								break;
							}
							break;
						}
						break;
					case 'k':
						string current3 = enumerator.Current as string;
						enumerator.MoveNext();
						if (flag1 && this.ActiveChannelSyncing)
							channel.Key = current3;
						if (flag2 && this.ActiveChannelSyncing)
						{
							channel.Key = "";
							break;
						}
						break;
					case 'l':
						string current4 = enumerator.Current as string;
						enumerator.MoveNext();
						if (flag1)
						{
							if (this.ActiveChannelSyncing)
							{
								try
								{
									channel.UserLimit = int.Parse(current4);
								}
								catch (FormatException ex)
								{
								}
							}
						}
						if (flag2 && this.ActiveChannelSyncing)
						{
							channel.UserLimit = 0;
							break;
						}
						break;
					case 'o':
						string current5 = enumerator.Current as string;
						enumerator.MoveNext();
						if (flag1)
						{
							if (this.ActiveChannelSyncing)
							{
								if (this.GetChannelUser(ircdata.Channel, current5) != null)
								{
									try
									{
										channel.UnsafeOps.Add((object)current5, (object)this.GetIrcUser(current5));
									}
									catch (ArgumentException ex)
									{
									}
									this.GetChannelUser(ircdata.Channel, current5).IsOp = true;
								}
							}
							if (this.OnOp != null)
								this.OnOp((object)this, new OpEventArgs(ircdata, ircdata.Channel, ircdata.Nick, current5));
						}
						if (flag2)
						{
							if (this.ActiveChannelSyncing && this.GetChannelUser(ircdata.Channel, current5) != null)
							{
								channel.UnsafeOps.Remove((object)current5);
								this.GetChannelUser(ircdata.Channel, current5).IsOp = false;
							}
							if (this.OnDeop != null)
							{
								this.OnDeop((object)this, new DeopEventArgs(ircdata, ircdata.Channel, ircdata.Nick, current5));
								break;
							}
							break;
						}
						break;
					case 'v':
						string current6 = enumerator.Current as string;
						enumerator.MoveNext();
						if (flag1)
						{
							if (this.ActiveChannelSyncing)
							{
								if (this.GetChannelUser(ircdata.Channel, current6) != null)
								{
									try
									{
										channel.UnsafeVoices.Add((object)current6, (object)this.GetIrcUser(current6));
									}
									catch (ArgumentException ex)
									{
									}
									this.GetChannelUser(ircdata.Channel, current6).IsVoice = true;
								}
							}
							if (this.OnVoice != null)
								this.OnVoice((object)this, new VoiceEventArgs(ircdata, ircdata.Channel, ircdata.Nick, current6));
						}
						if (flag2)
						{
							if (this.ActiveChannelSyncing && this.GetChannelUser(ircdata.Channel, current6) != null)
							{
								channel.UnsafeVoices.Remove((object)current6);
								this.GetChannelUser(ircdata.Channel, current6).IsVoice = false;
							}
							if (this.OnDevoice != null)
							{
								this.OnDevoice((object)this, new DevoiceEventArgs(ircdata, ircdata.Channel, ircdata.Nick, current6));
								break;
							}
							break;
						}
						break;
					default:
						if (flag1 && this.ActiveChannelSyncing && channel.Mode.IndexOf(mode[index]) == -1)
							channel.Mode += mode[index].ToString();
						if (flag2 && this.ActiveChannelSyncing)
						{
							channel.Mode = channel.Mode.Replace(mode[index].ToString(), string.Empty);
							break;
						}
						break;
				}
			}
		}

		private void _Event_PING(IrcMessageData ircdata)
		{
			string str = ircdata.RawMessageArray[1].Substring(1);
			this.RfcPong(str, Priority.Critical);
			if (this.OnPing == null)
				return;
			this.OnPing((object)this, new PingEventArgs(ircdata, str));
		}

		private void _Event_PONG(IrcMessageData ircdata)
		{
			if (this.OnPong == null)
				return;
			this.OnPong((object)this, new PongEventArgs(ircdata, ircdata.Irc.Lag));
		}

		private void _Event_ERROR(IrcMessageData ircdata)
		{
			string message = ircdata.Message;
			if (this.OnError == null)
				return;
			this.OnError((object)this, new ErrorEventArgs(ircdata, message));
		}

		private void _Event_JOIN(IrcMessageData ircdata)
		{
			string nick = ircdata.Nick;
			string channel1 = ircdata.Channel;
			if (this.IsMe(nick))
				this._JoinedChannels.Add(channel1);
			if (this.ActiveChannelSyncing)
			{
				if (this.IsMe(nick))
				{
					Channel channel2 = this.CreateChannel(channel1);
					this._Channels.Add((object)channel1, (object)channel2);
					this.RfcMode(channel1);
					this.RfcWho(channel1);
					this.Ban(channel1);
				}
				else
					this.RfcWho(nick);
				Channel channel3 = this.GetChannel(channel1);
				IrcUser ircUser = this.GetIrcUser(nick);
				if (ircUser == null)
				{
					ircUser = new IrcUser(nick, this);
					ircUser.Ident = ircdata.Ident;
					ircUser.Host = ircdata.Host;
					this._IrcUsers.Add((object)nick, (object)ircUser);
				}
				ChannelUser channelUser = this.CreateChannelUser(channel1, ircUser);
				channel3.UnsafeUsers.Add((object)nick, (object)channelUser);
			}
			if (this.OnJoin == null)
				return;
			this.OnJoin((object)this, new JoinEventArgs(ircdata, channel1, nick));
		}

		private void _Event_PART(IrcMessageData ircdata)
		{
			string nick = ircdata.Nick;
			string channel = ircdata.Channel;
			string message = ircdata.Message;
			if (this.IsMe(nick))
				this._JoinedChannels.Remove(channel);
			if (this.ActiveChannelSyncing)
			{
				if (this.IsMe(nick))
				{
					this._Channels.Remove((object)channel);
				}
				else
				{
					this._RemoveChannelUser(channel, nick);
					this._RemoveIrcUser(nick);
				}
			}
			if (this.OnPart == null)
				return;
			this.OnPart((object)this, new PartEventArgs(ircdata, channel, nick, message));
		}

		private void _Event_KICK(IrcMessageData ircdata)
		{
			string channel1 = ircdata.Channel;
			string nick = ircdata.Nick;
			string rawMessage = ircdata.RawMessageArray[3];
			string message = ircdata.Message;
			bool flag = this.IsMe(rawMessage);
			if (flag)
				this._JoinedChannels.Remove(channel1);
			if (this.ActiveChannelSyncing)
			{
				if (flag)
				{
					Channel channel2 = this.GetChannel(channel1);
					this._Channels.Remove((object)channel1);
					if (this._AutoRejoinOnKick)
						this.RfcJoin(channel2.Name, channel2.Key);
				}
				else
				{
					this._RemoveChannelUser(channel1, rawMessage);
					this._RemoveIrcUser(rawMessage);
				}
			}
			else if (flag && this.AutoRejoinOnKick)
				this.RfcJoin(channel1);
			if (this.OnKick == null)
				return;
			this.OnKick((object)this, new KickEventArgs(ircdata, channel1, nick, rawMessage, message));
		}

		private void _Event_QUIT(IrcMessageData ircdata)
		{
			string nick = ircdata.Nick;
			string message = ircdata.Message;
			if (this.ActiveChannelSyncing)
			{
				IrcUser ircUser = this.GetIrcUser(nick);
				if (ircUser != null)
				{
					string[] joinedChannels = ircUser.JoinedChannels;
					if (joinedChannels != null)
					{
						foreach (string channelname in joinedChannels)
							this._RemoveChannelUser(channelname, nick);
						this._RemoveIrcUser(nick);
					}
				}
			}
			if (this.OnQuit == null)
				return;
			this.OnQuit((object)this, new QuitEventArgs(ircdata, nick, message));
		}

		private void _Event_PRIVMSG(IrcMessageData ircdata)
		{
			if (ircdata.Type == ReceiveType.CtcpRequest)
			{
				if (ircdata.Message.StartsWith("\x0001PING"))
				{
					if (ircdata.Message.Length > 7)
						this.SendMessage(SendType.CtcpReply, ircdata.Nick, "PING " + ircdata.Message.Substring(6, ircdata.Message.Length - 7));
					else
						this.SendMessage(SendType.CtcpReply, ircdata.Nick, "PING");
				}
				else if (ircdata.Message.StartsWith("\x0001VERSION"))
				{
					string str = this._CtcpVersion != null ? this._CtcpVersion : this.VersionString;
					this.SendMessage(SendType.CtcpReply, ircdata.Nick, "VERSION " + str);
				}
				else if (ircdata.Message.StartsWith("\x0001CLIENTINFO"))
					this.SendMessage(SendType.CtcpReply, ircdata.Nick, "CLIENTINFO PING VERSION CLIENTINFO");
			}
			switch (ircdata.Type)
			{
				case ReceiveType.ChannelMessage:
					if (this.OnChannelMessage == null)
						break;
					this.OnChannelMessage((object)this, new IrcEventArgs(ircdata));
					break;
				case ReceiveType.ChannelAction:
					if (this.OnChannelAction == null)
						break;
					string actionmsg1 = ircdata.Message.Substring(8, ircdata.Message.Length - 9);
					this.OnChannelAction((object)this, new ActionEventArgs(ircdata, actionmsg1));
					break;
				case ReceiveType.QueryMessage:
					if (this.OnQueryMessage == null)
						break;
					this.OnQueryMessage((object)this, new IrcEventArgs(ircdata));
					break;
				case ReceiveType.QueryAction:
					if (this.OnQueryAction == null)
						break;
					string actionmsg2 = ircdata.Message.Substring(8, ircdata.Message.Length - 9);
					this.OnQueryAction((object)this, new ActionEventArgs(ircdata, actionmsg2));
					break;
				case ReceiveType.CtcpRequest:
					if (this.OnCtcpRequest == null)
						break;
					int num = ircdata.Message.IndexOf(' ');
					string ctcpparam = "";
					string ctcpcmd;
					if (num != -1)
					{
						ctcpcmd = ircdata.Message.Substring(1, num - 1);
						ctcpparam = ircdata.Message.Substring(num + 1, ircdata.Message.Length - num - 2);
					}
					else
						ctcpcmd = ircdata.Message.Substring(1, ircdata.Message.Length - 2);
					this.OnCtcpRequest((object)this, new CtcpEventArgs(ircdata, ctcpcmd, ctcpparam));
					break;
			}
		}

		private void _Event_NOTICE(IrcMessageData ircdata)
		{
			switch (ircdata.Type)
			{
				case ReceiveType.ChannelNotice:
					if (this.OnChannelNotice == null)
						break;
					this.OnChannelNotice((object)this, new IrcEventArgs(ircdata));
					break;
				case ReceiveType.QueryNotice:
					if (this.OnQueryNotice == null)
						break;
					this.OnQueryNotice((object)this, new IrcEventArgs(ircdata));
					break;
				case ReceiveType.CtcpReply:
					if (this.OnCtcpReply == null)
						break;
					int num = ircdata.Message.IndexOf(' ');
					string ctcpparam = "";
					string ctcpcmd;
					if (num != -1)
					{
						ctcpcmd = ircdata.Message.Substring(1, num - 1);
						ctcpparam = ircdata.Message.Substring(num + 1, ircdata.Message.Length - num - 2);
					}
					else
						ctcpcmd = ircdata.Message.Substring(1, ircdata.Message.Length - 2);
					this.OnCtcpReply((object)this, new CtcpEventArgs(ircdata, ctcpcmd, ctcpparam));
					break;
			}
		}

		private void _Event_TOPIC(IrcMessageData ircdata)
		{
			string nick = ircdata.Nick;
			string channel = ircdata.Channel;
			string message = ircdata.Message;
			if (this.ActiveChannelSyncing && this.IsJoined(channel))
				this.GetChannel(channel).Topic = message;
			if (this.OnTopicChange == null)
				return;
			this.OnTopicChange((object)this, new TopicChangeEventArgs(ircdata, channel, nick, message));
		}

		private void _Event_NICK(IrcMessageData ircdata)
		{
			string nick = ircdata.Nick;
			string newnick = ircdata.RawMessageArray[2];
			if (newnick.StartsWith(":"))
				newnick = newnick.Substring(1);
			if (this.IsMe(ircdata.Nick))
				this._Nickname = newnick;
			if (this.ActiveChannelSyncing)
			{
				IrcUser ircUser = this.GetIrcUser(nick);
				if (ircUser != null)
				{
					string[] joinedChannels = ircUser.JoinedChannels;
					ircUser.Nick = newnick;
					this._IrcUsers.Remove((object)nick);
					this._IrcUsers.Add((object)newnick, (object)ircUser);
					foreach (string channelname in joinedChannels)
					{
						Channel channel = this.GetChannel(channelname);
						ChannelUser channelUser = this.GetChannelUser(channelname, nick);
						channel.UnsafeUsers.Remove((object)nick);
						channel.UnsafeUsers.Add((object)newnick, (object)channelUser);
						if (channelUser.IsOp)
						{
							channel.UnsafeOps.Remove((object)nick);
							channel.UnsafeOps.Add((object)newnick, (object)channelUser);
						}
						if (this.SupportNonRfc && ((NonRfcChannelUser)channelUser).IsHalfop)
						{
							NonRfcChannel nonRfcChannel = (NonRfcChannel)channel;
							nonRfcChannel.UnsafeHalfops.Remove((object)nick);
							nonRfcChannel.UnsafeHalfops.Add((object)newnick, (object)channelUser);
						}
						if (channelUser.IsVoice)
						{
							channel.UnsafeVoices.Remove((object)nick);
							channel.UnsafeVoices.Add((object)newnick, (object)channelUser);
						}
					}
				}
			}
			if (this.OnNickChange == null)
				return;
			this.OnNickChange((object)this, new NickChangeEventArgs(ircdata, nick, newnick));
		}

		private void _Event_INVITE(IrcMessageData ircdata)
		{
			string channel = ircdata.Channel;
			string nick = ircdata.Nick;
			if (this.AutoJoinOnInvite && channel.Trim() != "0")
				this.RfcJoin(channel);
			if (this.OnInvite == null)
				return;
			this.OnInvite((object)this, new InviteEventArgs(ircdata, channel, nick));
		}

		private void _Event_MODE(IrcMessageData ircdata)
		{
			if (this.IsMe(ircdata.RawMessageArray[2]))
			{
				this._Usermode = ircdata.RawMessageArray[3].Substring(1);
			}
			else
			{
				string rawMessage = ircdata.RawMessageArray[3];
				string parameter = string.Join(" ", ircdata.RawMessageArray, 4, ircdata.RawMessageArray.Length - 4);
				this._InterpretChannelMode(ircdata, rawMessage, parameter);
			}
			if (ircdata.Type == ReceiveType.UserModeChange && this.OnUserModeChange != null)
				this.OnUserModeChange((object)this, new IrcEventArgs(ircdata));
			if (ircdata.Type == ReceiveType.ChannelModeChange && this.OnChannelModeChange != null)
				this.OnChannelModeChange((object)this, new IrcEventArgs(ircdata));
			if (this.OnModeChange == null)
				return;
			this.OnModeChange((object)this, new IrcEventArgs(ircdata));
		}

		private void _Event_RPL_CHANNELMODEIS(IrcMessageData ircdata)
		{
			if (!this.ActiveChannelSyncing || !this.IsJoined(ircdata.Channel))
				return;
			this.GetChannel(ircdata.Channel).Mode = string.Empty;
			string rawMessage = ircdata.RawMessageArray[4];
			string parameter = string.Join(" ", ircdata.RawMessageArray, 5, ircdata.RawMessageArray.Length - 5);
			this._InterpretChannelMode(ircdata, rawMessage, parameter);
		}

		private void _Event_RPL_WELCOME(IrcMessageData ircdata)
		{
			this._Nickname = ircdata.RawMessageArray[2];
			if (this.OnRegistered == null)
				return;
			this.OnRegistered((object)this, EventArgs.Empty);
		}

		private void _Event_RPL_TOPIC(IrcMessageData ircdata)
		{
			string message = ircdata.Message;
			string channel = ircdata.Channel;
			if (this.ActiveChannelSyncing && this.IsJoined(channel))
				this.GetChannel(channel).Topic = message;
			if (this.OnTopic == null)
				return;
			this.OnTopic((object)this, new TopicEventArgs(ircdata, channel, message));
		}

		private void _Event_RPL_NOTOPIC(IrcMessageData ircdata)
		{
			string channel = ircdata.Channel;
			if (this.ActiveChannelSyncing && this.IsJoined(channel))
				this.GetChannel(channel).Topic = "";
			if (this.OnTopic == null)
				return;
			this.OnTopic((object)this, new TopicEventArgs(ircdata, channel, ""));
		}

		private void _Event_RPL_NAMREPLY(IrcMessageData ircdata)
		{
			string channel1 = ircdata.Channel;
			string[] messageArray = ircdata.MessageArray;
			if (this.ActiveChannelSyncing && this.IsJoined(channel1))
			{
				foreach (string str in messageArray)
				{
					if (str.Length > 0)
					{
						bool flag1 = false;
						bool flag2 = false;
						bool flag3 = false;
						string nickname;
						switch (str[0])
						{
							case '%':
								flag2 = true;
								nickname = str.Substring(1);
								break;
							case '&':
								nickname = str.Substring(1);
								break;
							case '+':
								flag3 = true;
								nickname = str.Substring(1);
								break;
							case '@':
								flag1 = true;
								nickname = str.Substring(1);
								break;
							case '~':
								nickname = str.Substring(1);
								break;
							default:
								nickname = str;
								break;
						}
						IrcUser ircUser = this.GetIrcUser(nickname);
						ChannelUser channelUser = this.GetChannelUser(channel1, nickname);
						if (ircUser == null)
						{
							ircUser = new IrcUser(nickname, this);
							this._IrcUsers.Add((object)nickname, (object)ircUser);
						}
						if (channelUser == null)
						{
							channelUser = this.CreateChannelUser(channel1, ircUser);
							Channel channel2 = this.GetChannel(channel1);
							channel2.UnsafeUsers.Add((object)nickname, (object)channelUser);
							if (flag1)
								channel2.UnsafeOps.Add((object)nickname, (object)channelUser);
							if (this.SupportNonRfc && flag2)
								((NonRfcChannel)channel2).UnsafeHalfops.Add((object)nickname, (object)channelUser);
							if (flag3)
								channel2.UnsafeVoices.Add((object)nickname, (object)channelUser);
						}
						channelUser.IsOp = flag1;
						channelUser.IsVoice = flag3;
						if (this.SupportNonRfc)
							((NonRfcChannelUser)channelUser).IsHalfop = flag2;
					}
				}
			}
			if (this.OnNames == null)
				return;
			this.OnNames((object)this, new NamesEventArgs(ircdata, channel1, messageArray));
		}

		private void _Event_RPL_LIST(IrcMessageData ircdata)
		{
			string channel = ircdata.Channel;
			int userCount = int.Parse(ircdata.RawMessageArray[4]);
			string message = ircdata.Message;
			ChannelInfo listInfo = (ChannelInfo)null;
			if (this.OnList != null || this._ChannelList != null)
				listInfo = new ChannelInfo(channel, userCount, message);
			if (this._ChannelList != null)
				this._ChannelList.Add(listInfo);
			if (this.OnList == null)
				return;
			this.OnList((object)this, new ListEventArgs(ircdata, listInfo));
		}

		private void _Event_RPL_LISTEND(IrcMessageData ircdata)
		{
			if (this._ChannelListReceivedEvent == null)
				return;
			this._ChannelListReceivedEvent.Set();
		}

		private void _Event_RPL_TRYAGAIN(IrcMessageData ircdata)
		{
			if (this._ChannelListReceivedEvent == null)
				return;
			this._ChannelListReceivedEvent.Set();
		}

		private void _Event_RPL_ENDOFNAMES(IrcMessageData ircdata)
		{
			string rawMessage = ircdata.RawMessageArray[3];
			if (!this.ActiveChannelSyncing || !this.IsJoined(rawMessage) || this.OnChannelPassiveSynced == null)
				return;
			this.OnChannelPassiveSynced((object)this, new IrcEventArgs(ircdata));
		}

		private void _Event_RPL_AWAY(IrcMessageData ircdata)
		{
			string rawMessage = ircdata.RawMessageArray[3];
			string message = ircdata.Message;
			if (this.ActiveChannelSyncing)
			{
				IrcUser ircUser = this.GetIrcUser(rawMessage);
				if (ircUser != null)
					ircUser.IsAway = true;
			}
			if (this.OnAway == null)
				return;
			this.OnAway((object)this, new AwayEventArgs(ircdata, rawMessage, message));
		}

		private void _Event_RPL_UNAWAY(IrcMessageData ircdata)
		{
			this._IsAway = false;
			if (this.OnUnAway == null)
				return;
			this.OnUnAway((object)this, new IrcEventArgs(ircdata));
		}

		private void _Event_RPL_NOWAWAY(IrcMessageData ircdata)
		{
			this._IsAway = true;
			if (this.OnNowAway == null)
				return;
			this.OnNowAway((object)this, new IrcEventArgs(ircdata));
		}

		private void _Event_RPL_WHOREPLY(IrcMessageData ircdata)
		{
			WhoInfo whoInfo = WhoInfo.Parse(ircdata);
			string channel = whoInfo.Channel;
			string nick = whoInfo.Nick;
			if (this._WhoList != null)
				this._WhoList.Add(whoInfo);
			if (this.ActiveChannelSyncing && this.IsJoined(channel))
			{
				IrcUser ircUser = this.GetIrcUser(nick);
				ChannelUser channelUser = this.GetChannelUser(channel, nick);
				if (ircUser != null)
				{
					ircUser.Ident = whoInfo.Ident;
					ircUser.Host = whoInfo.Host;
					ircUser.Server = whoInfo.Server;
					ircUser.Nick = whoInfo.Nick;
					ircUser.HopCount = whoInfo.HopCount;
					ircUser.Realname = whoInfo.Realname;
					ircUser.IsAway = whoInfo.IsAway;
					ircUser.IsIrcOp = whoInfo.IsIrcOp;
					switch (channel[0])
					{
						case '!':
						case '#':
						case '&':
						case '+':
							if (channelUser != null)
							{
								channelUser.IsOp = whoInfo.IsOp;
								channelUser.IsVoice = whoInfo.IsVoice;
								break;
							}
							break;
					}
				}
			}
			if (this.OnWho == null)
				return;
			this.OnWho((object)this, new WhoEventArgs(ircdata, whoInfo));
		}

		private void _Event_RPL_ENDOFWHO(IrcMessageData ircdata)
		{
			if (this._WhoListReceivedEvent == null)
				return;
			this._WhoListReceivedEvent.Set();
		}

		private void _Event_RPL_MOTD(IrcMessageData ircdata)
		{
			if (!this._MotdReceived)
				this._Motd.Add(ircdata.Message);
			if (this.OnMotd == null)
				return;
			this.OnMotd((object)this, new MotdEventArgs(ircdata, ircdata.Message));
		}

		private void _Event_RPL_ENDOFMOTD(IrcMessageData ircdata)
		{
			this._MotdReceived = true;
		}

		private void _Event_RPL_BANLIST(IrcMessageData ircdata)
		{
			string channel1 = ircdata.Channel;
			BanInfo banInfo = BanInfo.Parse(ircdata);
			if (this._BanList != null)
				this._BanList.Add(banInfo);
			if (!this.ActiveChannelSyncing || !this.IsJoined(channel1))
				return;
			Channel channel2 = this.GetChannel(channel1);
			if (channel2.IsSycned)
				return;
			channel2.Bans.Add(banInfo.Mask);
		}

		private void _Event_RPL_ENDOFBANLIST(IrcMessageData ircdata)
		{
			string channel1 = ircdata.Channel;
			if (this._BanListReceivedEvent != null)
				this._BanListReceivedEvent.Set();
			if (!this.ActiveChannelSyncing || !this.IsJoined(channel1))
				return;
			Channel channel2 = this.GetChannel(channel1);
			if (channel2.IsSycned)
				return;
			channel2.ActiveSyncStop = DateTime.Now;
			channel2.IsSycned = true;
			if (this.OnChannelActiveSynced == null)
				return;
			this.OnChannelActiveSynced((object)this, new IrcEventArgs(ircdata));
		}

		private void _Event_ERR_NOCHANMODES(IrcMessageData ircdata)
		{
			string rawMessage = ircdata.RawMessageArray[3];
			if (!this.ActiveChannelSyncing || !this.IsJoined(rawMessage))
				return;
			Channel channel = this.GetChannel(rawMessage);
			if (channel.IsSycned)
				return;
			channel.ActiveSyncStop = DateTime.Now;
			channel.IsSycned = true;
			if (this.OnChannelActiveSynced == null)
				return;
			this.OnChannelActiveSynced((object)this, new IrcEventArgs(ircdata));
		}

		private void _Event_ERR(IrcMessageData ircdata)
		{
			if (this.OnErrorMessage == null)
				return;
			this.OnErrorMessage((object)this, new IrcEventArgs(ircdata));
		}

		private void _Event_ERR_NICKNAMEINUSE(IrcMessageData ircdata)
		{
			if (!this.AutoNickHandling)
				return;
			string newnickname;
			if (this._CurrentNickname == this.NicknameList.Length - 1)
			{
				int num = new Random().Next(999);
				newnickname = this.Nickname.Length <= 5 ? this.Nickname.Substring(0, this.Nickname.Length - 1) + (object)num : this.Nickname.Substring(0, 5) + (object)num;
			}
			else
				newnickname = this._NextNickname();
			this.RfcNick(newnickname, Priority.Critical);
		}
	}
}