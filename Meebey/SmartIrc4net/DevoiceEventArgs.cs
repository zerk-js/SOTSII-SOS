// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.DevoiceEventArgs
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Meebey.SmartIrc4net
{
	public class DevoiceEventArgs : IrcEventArgs
	{
		private string _Channel;
		private string _Who;
		private string _Whom;

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

		public string Whom
		{
			get
			{
				return this._Whom;
			}
		}

		internal DevoiceEventArgs(IrcMessageData data, string channel, string who, string whom)
		  : base(data)
		{
			this._Channel = channel;
			this._Who = who;
			this._Whom = whom;
		}
	}
}
