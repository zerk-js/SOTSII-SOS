// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Console.ConsoleApplet
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Kerberos.Sots.Console
{
	internal class ConsoleApplet
	{
		private readonly List<string> _commands = new List<string>();
		private bool _started;
		private Thread _thread;
		private Kerberos.Sots.Console.Console _con;

		private void Worker(object param)
		{
			this._con = new Kerberos.Sots.Console.Console();
			this._con.Show();
			this._con.CommandEntered += new EventCommandEntered(this.ShellControl_CommandEntered);
			Application.Run((Form)this._con);
			this._con.CommandEntered -= new EventCommandEntered(this.ShellControl_CommandEntered);
			this._con.Dispose();
			this._con = (Kerberos.Sots.Console.Console)null;
		}

		private void ShellControl_CommandEntered(object sender, CommandEnteredEventArgs e)
		{
			lock (this._commands)
				this._commands.Add(e.Command);
		}

		public void Start()
		{
			if (this._started)
				throw new InvalidOperationException("Already started.");
			this._started = true;
			this._thread = ScriptHost.CreateThread(new ParameterizedThreadStart(this.Worker));
			this._thread.Priority = ThreadPriority.BelowNormal;
			this._thread.SetApartmentState(ApartmentState.STA);
			this._thread.Start();
		}

		public void Stop()
		{
			Application.Exit();
			this._commands.Clear();
			this._started = false;
		}

		public string[] FlushCommands()
		{
			lock (this._commands)
			{
				string[] array = this._commands.ToArray();
				this._commands.Clear();
				return array;
			}
		}

		public void WriteText(string category, bool registerHit, string s, Color color)
		{
			if (this._con == null || s.Length == 0)
				return;
			lock (this._con)
				this._con.WriteText(category, registerHit, s, color);
		}
	}
}
