// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.NonRfcChannelUser
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Meebey.SmartIrc4net
{
	public class NonRfcChannelUser : ChannelUser
	{
		private bool _IsHalfop;
		private bool _IsOwner;
		private bool _IsAdmin;

		internal NonRfcChannelUser(string channel, IrcUser ircuser)
		  : base(channel, ircuser)
		{
		}

		public bool IsHalfop
		{
			get
			{
				return this._IsHalfop;
			}
			set
			{
				this._IsHalfop = value;
			}
		}
	}
}
