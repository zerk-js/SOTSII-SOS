// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Engine.LogMessageInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Engine
{
	public class LogMessageInfo
	{
		public readonly LogLevel Level;
		public readonly LogSeverity Severity;
		public readonly char SeverityGlyph;
		public readonly string TimeStamp;
		public readonly string Category;
		public readonly string Message;

		public LogMessageInfo()
		{
		}

		public LogMessageInfo(
		  LogLevel level,
		  LogSeverity severity,
		  char severityGlyph,
		  string timeStamp,
		  string category,
		  string message)
		{
			this.Level = level;
			this.Severity = severity;
			this.SeverityGlyph = severityGlyph;
			this.TimeStamp = timeStamp;
			this.Category = category;
			this.Message = message;
		}
	}
}
