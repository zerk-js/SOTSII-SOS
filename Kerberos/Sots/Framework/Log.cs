// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.Log
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.Framework
{
	internal class Log
	{
		private ILogHost _logHost;

		public event MessageLoggedEventHandler MessageLogged
		{
			add
			{
				this._logHost.MessageLogged += value;
			}
			remove
			{
				this._logHost.MessageLogged -= value;
			}
		}

		public string FilePath
		{
			get
			{
				return this._logHost.FilePath;
			}
		}

		public LogLevel Level
		{
			get
			{
				return this._logHost.Level;
			}
			set
			{
				this._logHost.Level = value;
			}
		}

		public Log(ILogHost loghost)
		{
			this._logHost = loghost;
		}

		public void Trace(string message, string category, LogLevel level)
		{
			this._logHost.LogMessage(level, LogSeverity.Trace, category, message);
		}

		public void Trace(string message, string category)
		{
			this._logHost.LogMessage(LogLevel.Normal, LogSeverity.Trace, category, message);
		}

		public void Warn(string message, string category)
		{
			this._logHost.LogMessage(LogLevel.Normal, LogSeverity.Warn, category, message);
		}
	}
}
