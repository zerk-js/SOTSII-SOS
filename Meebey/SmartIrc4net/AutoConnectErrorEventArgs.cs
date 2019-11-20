// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.AutoConnectErrorEventArgs
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Meebey.SmartIrc4net
{
	public class AutoConnectErrorEventArgs : EventArgs
	{
		private Exception _Exception;
		private string _Address;
		private int _Port;

		public Exception Exception
		{
			get
			{
				return this._Exception;
			}
		}

		public string Address
		{
			get
			{
				return this._Address;
			}
		}

		public int Port
		{
			get
			{
				return this._Port;
			}
		}

		internal AutoConnectErrorEventArgs(string address, int port, Exception ex)
		{
			this._Address = address;
			this._Port = port;
			this._Exception = ex;
		}
	}
}
