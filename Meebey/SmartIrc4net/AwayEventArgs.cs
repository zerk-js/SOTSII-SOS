// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.AwayEventArgs
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Meebey.SmartIrc4net
{
	public class AwayEventArgs : IrcEventArgs
	{
		private string _Who;
		private string _AwayMessage;

		public string Who
		{
			get
			{
				return this._Who;
			}
		}

		public string AwayMessage
		{
			get
			{
				return this._AwayMessage;
			}
		}

		internal AwayEventArgs(IrcMessageData data, string who, string awaymessage)
		  : base(data)
		{
			this._Who = who;
			this._AwayMessage = awaymessage;
		}
	}
}
