// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.SmartIrc4netException
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Runtime.Serialization;

namespace Meebey.SmartIrc4net
{
	[Serializable]
	public class SmartIrc4netException : ApplicationException
	{
		public SmartIrc4netException()
		{
		}

		public SmartIrc4netException(string message)
		  : base(message)
		{
		}

		public SmartIrc4netException(string message, Exception e)
		  : base(message, e)
		{
		}

		protected SmartIrc4netException(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{
		}
	}
}
