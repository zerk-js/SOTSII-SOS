// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.BanEventArgs
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Meebey.SmartIrc4net
{
	public class BanEventArgs : IrcEventArgs
	{
		private string _Channel;
		private string _Who;
		private string _Hostmask;

		public string Channel
		{
			get
			{
				return this._Channel;
			}
		}

		public string Who
		{
			get
			{
				return this._Who;
			}
		}

		public string Hostmask
		{
			get
			{
				return this._Hostmask;
			}
		}

		internal BanEventArgs(IrcMessageData data, string channel, string who, string hostmask)
		  : base(data)
		{
			this._Channel = channel;
			this._Who = who;
			this._Hostmask = hostmask;
		}
	}
}
