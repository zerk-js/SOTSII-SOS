// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.WhoEventArgs
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Meebey.SmartIrc4net
{
	public class WhoEventArgs : IrcEventArgs
	{
		private WhoInfo f_WhoInfo;

		[Obsolete("Use WhoEventArgs.WhoInfo instead.")]
		public string Channel
		{
			get
			{
				return this.f_WhoInfo.Channel;
			}
		}

		[Obsolete("Use WhoEventArgs.WhoInfo instead.")]
		public string Nick
		{
			get
			{
				return this.f_WhoInfo.Nick;
			}
		}

		[Obsolete("Use WhoEventArgs.WhoInfo instead.")]
		public string Ident
		{
			get
			{
				return this.f_WhoInfo.Ident;
			}
		}

		[Obsolete("Use WhoEventArgs.WhoInfo instead.")]
		public string Host
		{
			get
			{
				return this.f_WhoInfo.Host;
			}
		}

		[Obsolete("Use WhoEventArgs.WhoInfo instead.")]
		public string Realname
		{
			get
			{
				return this.f_WhoInfo.Realname;
			}
		}

		[Obsolete("Use WhoEventArgs.WhoInfo instead.")]
		public bool IsAway
		{
			get
			{
				return this.f_WhoInfo.IsAway;
			}
		}

		[Obsolete("Use WhoEventArgs.WhoInfo instead.")]
		public bool IsOp
		{
			get
			{
				return this.f_WhoInfo.IsOp;
			}
		}

		[Obsolete("Use WhoEventArgs.WhoInfo instead.")]
		public bool IsVoice
		{
			get
			{
				return this.f_WhoInfo.IsVoice;
			}
		}

		[Obsolete("Use WhoEventArgs.WhoInfo instead.")]
		public bool IsIrcOp
		{
			get
			{
				return this.f_WhoInfo.IsIrcOp;
			}
		}

		[Obsolete("Use WhoEventArgs.WhoInfo instead.")]
		public string Server
		{
			get
			{
				return this.f_WhoInfo.Server;
			}
		}

		[Obsolete("Use WhoEventArgs.WhoInfo instead.")]
		public int HopCount
		{
			get
			{
				return this.f_WhoInfo.HopCount;
			}
		}

		public WhoInfo WhoInfo
		{
			get
			{
				return this.f_WhoInfo;
			}
		}

		internal WhoEventArgs(IrcMessageData data, WhoInfo whoInfo)
		  : base(data)
		{
			this.f_WhoInfo = whoInfo;
		}
	}
}
