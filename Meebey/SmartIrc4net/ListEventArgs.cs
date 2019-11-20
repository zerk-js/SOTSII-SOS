// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.ListEventArgs
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Meebey.SmartIrc4net
{
	public class ListEventArgs : IrcEventArgs
	{
		private ChannelInfo f_ListInfo;

		public ChannelInfo ListInfo
		{
			get
			{
				return this.f_ListInfo;
			}
		}

		internal ListEventArgs(IrcMessageData data, ChannelInfo listInfo)
		  : base(data)
		{
			this.f_ListInfo = listInfo;
		}
	}
}
