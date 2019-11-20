// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.PlayerFramework.Race
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System.IO;
using System.Xml;

namespace Kerberos.Sots.PlayerFramework
{
	internal class Race
	{
		public string Name;
		public string RaceFileName;
		public string Directory;
		public string AssetPath;
		private XmlElement Variables;

		public string GetVariable(string name)
		{
			if (this.Variables == null)
				return null;
			return this.Variables[name]?.InnerText.ToString();
		}

		public void LoadXml(string filename, string name)
		{
			this.RaceFileName = filename;
			this.Directory = Path.GetDirectoryName(filename);
			this.Name = name;
			this.AssetPath = Path.Combine("races", this.Name);
			XmlDocument document = new XmlDocument();
			document.Load(ScriptHost.FileSystem, filename);
			this.Variables = document[nameof(Race)]["Variables"];
		}
	}
}
