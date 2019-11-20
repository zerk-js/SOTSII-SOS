// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.ChannelInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Meebey.SmartIrc4net
{
	public class ChannelInfo
	{
		private string f_Channel;
		private int f_UserCount;
		private string f_Topic;

		public string Channel
		{
			get
			{
				return this.f_Channel;
			}
		}

		public int UserCount
		{
			get
			{
				return this.f_UserCount;
			}
		}

		public string Topic
		{
			get
			{
				return this.f_Topic;
			}
		}

		internal ChannelInfo(string channel, int userCount, string topic)
		{
			this.f_Channel = channel;
			this.f_UserCount = userCount;
			this.f_Topic = topic;
		}
	}
}
