// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.ActionEventArgs
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Meebey.SmartIrc4net
{
	public class ActionEventArgs : CtcpEventArgs
	{
		private string _ActionMessage;

		public string ActionMessage
		{
			get
			{
				return this._ActionMessage;
			}
		}

		internal ActionEventArgs(IrcMessageData data, string actionmsg)
		  : base(data, "ACTION", actionmsg)
		{
			this._ActionMessage = actionmsg;
		}
	}
}
