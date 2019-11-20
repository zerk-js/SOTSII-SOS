// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.ReceiveType
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Meebey.SmartIrc4net
{
	public enum ReceiveType
	{
		Info,
		Login,
		Motd,
		List,
		Join,
		Kick,
		Part,
		Invite,
		Quit,
		Who,
		WhoIs,
		WhoWas,
		Name,
		Topic,
		BanList,
		NickChange,
		TopicChange,
		UserMode,
		UserModeChange,
		ChannelMode,
		ChannelModeChange,
		ChannelMessage,
		ChannelAction,
		ChannelNotice,
		QueryMessage,
		QueryAction,
		QueryNotice,
		CtcpReply,
		CtcpRequest,
		Error,
		ErrorMessage,
		Unknown,
	}
}
