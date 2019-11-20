// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.TopicEventArgs
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Meebey.SmartIrc4net
{
	public class TopicEventArgs : IrcEventArgs
	{
		private string _Channel;
		private string _Topic;

		public string Channel
		{
			get
			{
				return this._Channel;
			}
		}

		public string Topic
		{
			get
			{
				return this._Topic;
			}
		}

		internal TopicEventArgs(IrcMessageData data, string channel, string topic)
		  : base(data)
		{
			this._Channel = channel;
			this._Topic = topic;
		}
	}
}
