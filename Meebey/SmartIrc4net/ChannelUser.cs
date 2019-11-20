// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.ChannelUser
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Meebey.SmartIrc4net
{
	public class ChannelUser
	{
		private string _Channel;
		private IrcUser _IrcUser;
		private bool _IsOp;
		private bool _IsVoice;

		internal ChannelUser(string channel, IrcUser ircuser)
		{
			this._Channel = channel;
			this._IrcUser = ircuser;
		}

		public string Channel
		{
			get
			{
				return this._Channel;
			}
		}

		public bool IsIrcOp
		{
			get
			{
				return this._IrcUser.IsIrcOp;
			}
		}

		public bool IsOp
		{
			get
			{
				return this._IsOp;
			}
			set
			{
				this._IsOp = value;
			}
		}

		public bool IsVoice
		{
			get
			{
				return this._IsVoice;
			}
			set
			{
				this._IsVoice = value;
			}
		}

		public bool IsAway
		{
			get
			{
				return this._IrcUser.IsAway;
			}
		}

		public IrcUser IrcUser
		{
			get
			{
				return this._IrcUser;
			}
		}

		public string Nick
		{
			get
			{
				return this._IrcUser.Nick;
			}
		}

		public string Ident
		{
			get
			{
				return this._IrcUser.Ident;
			}
		}

		public string Host
		{
			get
			{
				return this._IrcUser.Host;
			}
		}

		public string Realname
		{
			get
			{
				return this._IrcUser.Realname;
			}
		}

		public string Server
		{
			get
			{
				return this._IrcUser.Server;
			}
		}

		public int HopCount
		{
			get
			{
				return this._IrcUser.HopCount;
			}
		}

		public string[] JoinedChannels
		{
			get
			{
				return this._IrcUser.JoinedChannels;
			}
		}
	}
}
