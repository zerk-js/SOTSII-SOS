// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.AlreadyConnectedException
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Runtime.Serialization;

namespace Meebey.SmartIrc4net
{
	[Serializable]
	public class AlreadyConnectedException : ConnectionException
	{
		public AlreadyConnectedException()
		{
		}

		public AlreadyConnectedException(string message)
		  : base(message)
		{
		}

		public AlreadyConnectedException(string message, Exception e)
		  : base(message, e)
		{
		}

		protected AlreadyConnectedException(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{
		}
	}
}
