// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Engine.IFileSystem
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.IO;

namespace Kerberos.Sots.Engine
{
	public interface IFileSystem
	{
		Stream CreateStream(string path);

		void SplitBasePath(string path, out string mountName, out string suffix);

		bool IsBasePath(string path);

		bool IsRootedPath(string path);

		bool FileExists(string path);

		string[] FindFiles(string pattern);

		string[] FindDirectories(string pattern);

		bool TryResolveBaseFilePath(string path, out string result);

		bool TryResolveAbsoluteFilePath(string path, out string result);
	}
}
