// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Engine.FileSystemHelpers
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Engine
{
	internal static class FileSystemHelpers
	{
		public static string StripMountName(string path)
		{
			if (!path.StartsWith("\\"))
				return path;
			int num = path.IndexOf("\\", 1);
			return path.Substring(num + 1);
		}
	}
}
