// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.ErrorEventArgs
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Meebey.SmartIrc4net
{
	public class ErrorEventArgs : IrcEventArgs
	{
		private string _ErrorMessage;

		public string ErrorMessage
		{
			get
			{
				return this._ErrorMessage;
			}
		}

		internal ErrorEventArgs(IrcMessageData data, string errormsg)
		  : base(data)
		{
			this._ErrorMessage = errormsg;
		}
	}
}
