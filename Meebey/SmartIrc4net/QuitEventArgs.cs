// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.QuitEventArgs
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Meebey.SmartIrc4net
{
	public class QuitEventArgs : IrcEventArgs
	{
		private string _Who;
		private string _QuitMessage;

		public string Who
		{
			get
			{
				return this._Who;
			}
		}

		public string QuitMessage
		{
			get
			{
				return this._QuitMessage;
			}
		}

		internal QuitEventArgs(IrcMessageData data, string who, string quitmessage)
		  : base(data)
		{
			this._Who = who;
			this._QuitMessage = quitmessage;
		}
	}
}
