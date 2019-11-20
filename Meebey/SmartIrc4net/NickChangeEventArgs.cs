// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.NickChangeEventArgs
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Meebey.SmartIrc4net
{
	public class NickChangeEventArgs : IrcEventArgs
	{
		private string _OldNickname;
		private string _NewNickname;

		public string OldNickname
		{
			get
			{
				return this._OldNickname;
			}
		}

		public string NewNickname
		{
			get
			{
				return this._NewNickname;
			}
		}

		internal NickChangeEventArgs(IrcMessageData data, string oldnick, string newnick)
		  : base(data)
		{
			this._OldNickname = oldnick;
			this._NewNickname = newnick;
		}
	}
}
