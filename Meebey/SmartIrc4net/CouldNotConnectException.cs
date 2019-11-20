// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.CouldNotConnectException
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Runtime.Serialization;

namespace Meebey.SmartIrc4net
{
	[Serializable]
	public class CouldNotConnectException : ConnectionException
	{
		public CouldNotConnectException()
		{
		}

		public CouldNotConnectException(string message)
		  : base(message)
		{
		}

		public CouldNotConnectException(string message, Exception e)
		  : base(message, e)
		{
		}

		protected CouldNotConnectException(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{
		}
	}
}
