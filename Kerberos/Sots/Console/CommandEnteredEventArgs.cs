// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Console.CommandEnteredEventArgs
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Kerberos.Sots.Console
{
	public class CommandEnteredEventArgs : EventArgs
	{
		private string command;

		public CommandEnteredEventArgs(string command)
		{
			this.command = command;
		}

		public string Command
		{
			get
			{
				return this.command;
			}
		}
	}
}
