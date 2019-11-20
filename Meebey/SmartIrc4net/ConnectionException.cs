// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.ConnectionException
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Runtime.Serialization;

namespace Meebey.SmartIrc4net
{
	[Serializable]
	public class ConnectionException : SmartIrc4netException
	{
		public ConnectionException()
		{
		}

		public ConnectionException(string message)
		  : base(message)
		{
		}

		public ConnectionException(string message, Exception e)
		  : base(message, e)
		{
		}

		protected ConnectionException(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{
		}
	}
}
