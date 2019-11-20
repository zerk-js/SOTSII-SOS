// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.ShellHelper
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Diagnostics;

namespace Kerberos.Sots.Framework
{
	internal static class ShellHelper
	{
		public static void ShellOpen(string pathname)
		{
			Process.Start(new ProcessStartInfo(pathname)
			{
				UseShellExecute = true,
				Verb = "open"
			});
		}

		public static void ShellExplore(string filename)
		{
			Process.Start("explorer.exe", "/select," + filename);
		}
	}
}
