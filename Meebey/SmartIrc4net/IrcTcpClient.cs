// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.IrcTcpClient
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Net.Sockets;

namespace Meebey.SmartIrc4net
{
	internal class IrcTcpClient : TcpClient
	{
		public Socket Socket
		{
			get
			{
				return this.Client;
			}
		}
	}
}
