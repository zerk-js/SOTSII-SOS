// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Console.CommandHistory
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections;

namespace Kerberos.Sots.Console
{
	internal class CommandHistory
	{
		private ArrayList commandHistory = new ArrayList();
		private int currentPosn;
		private string lastCommand;

		internal CommandHistory()
		{
		}

		internal void Add(string command)
		{
			if (!(command != this.lastCommand))
				return;
			this.commandHistory.Add((object)command);
			this.lastCommand = command;
			this.currentPosn = this.commandHistory.Count;
		}

		internal bool DoesPreviousCommandExist()
		{
			return this.currentPosn > 0;
		}

		internal bool DoesNextCommandExist()
		{
			return this.currentPosn < this.commandHistory.Count - 1;
		}

		internal string GetPreviousCommand()
		{
			this.lastCommand = this.commandHistory[--this.currentPosn] as string;
			return this.lastCommand;
		}

		internal string GetNextCommand()
		{
			this.lastCommand = this.commandHistory[++this.currentPosn] as string;
			return this.LastCommand;
		}

		internal string LastCommand
		{
			get
			{
				return this.lastCommand;
			}
		}

		internal string[] GetCommandHistory()
		{
			return (string[])this.commandHistory.ToArray(typeof(string));
		}
	}
}
