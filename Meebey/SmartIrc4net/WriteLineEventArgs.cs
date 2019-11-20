// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.WriteLineEventArgs
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Meebey.SmartIrc4net
{
	public class WriteLineEventArgs : EventArgs
	{
		private string _Line;

		public string Line
		{
			get
			{
				return this._Line;
			}
		}

		internal WriteLineEventArgs(string line)
		{
			this._Line = line;
		}
	}
}
