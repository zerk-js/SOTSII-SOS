// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.IrcUser
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections.Specialized;

namespace Meebey.SmartIrc4net
{
	public class IrcUser
	{
		private int _HopCount = -1;
		private IrcClient _IrcClient;
		private string _Nick;
		private string _Ident;
		private string _Host;
		private string _Realname;
		private bool _IsIrcOp;
		private bool _IsAway;
		private string _Server;

		internal IrcUser(string nickname, IrcClient ircclient)
		{
			this._IrcClient = ircclient;
			this._Nick = nickname;
		}

		public string Nick
		{
			get
			{
				return this._Nick;
			}
			set
			{
				this._Nick = value;
			}
		}

		public string Ident
		{
			get
			{
				return this._Ident;
			}
			set
			{
				this._Ident = value;
			}
		}

		public string Host
		{
			get
			{
				return this._Host;
			}
			set
			{
				this._Host = value;
			}
		}

		public string Realname
		{
			get
			{
				return this._Realname;
			}
			set
			{
				this._Realname = value;
			}
		}

		public bool IsIrcOp
		{
			get
			{
				return this._IsIrcOp;
			}
			set
			{
				this._IsIrcOp = value;
			}
		}

		public bool IsAway
		{
			get
			{
				return this._IsAway;
			}
			set
			{
				this._IsAway = value;
			}
		}

		public string Server
		{
			get
			{
				return this._Server;
			}
			set
			{
				this._Server = value;
			}
		}

		public int HopCount
		{
			get
			{
				return this._HopCount;
			}
			set
			{
				this._HopCount = value;
			}
		}

		public string[] JoinedChannels
		{
			get
			{
				string[] channels = this._IrcClient.GetChannels();
				StringCollection stringCollection = new StringCollection();
				foreach (string channelname in channels)
				{
					if (this._IrcClient.GetChannel(channelname).UnsafeUsers.ContainsKey((object)this._Nick))
						stringCollection.Add(channelname);
				}
				string[] array = new string[stringCollection.Count];
				stringCollection.CopyTo(array, 0);
				return array;
			}
		}
	}
}
