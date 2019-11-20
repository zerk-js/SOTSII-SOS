// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.PathHelpers
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.IO;

namespace Kerberos.Sots.Framework
{
	public class PathHelpers
	{
		public static string Combine(params string[] parts)
		{
			string path1 = null;
			foreach (string part in parts)
				path1 = path1 != null ? Path.Combine(path1, part) : part;
			return path1;
		}

		public static string FixSeparators(string path)
		{
			return path.Replace('/', '\\');
		}
	}
}
