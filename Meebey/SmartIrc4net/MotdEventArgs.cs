// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.MotdEventArgs
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Meebey.SmartIrc4net
{
	public class MotdEventArgs : IrcEventArgs
	{
		private string _MotdMessage;

		public string MotdMessage
		{
			get
			{
				return this._MotdMessage;
			}
		}

		internal MotdEventArgs(IrcMessageData data, string motdmsg)
		  : base(data)
		{
			this._MotdMessage = motdmsg;
		}
	}
}
