// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.PongEventArgs
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Meebey.SmartIrc4net
{
	public class PongEventArgs : IrcEventArgs
	{
		private TimeSpan _Lag;

		public TimeSpan Lag
		{
			get
			{
				return this._Lag;
			}
		}

		internal PongEventArgs(IrcMessageData data, TimeSpan lag)
		  : base(data)
		{
			this._Lag = lag;
		}
	}
}
