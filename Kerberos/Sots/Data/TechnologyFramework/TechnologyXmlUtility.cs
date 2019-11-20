// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.TechnologyFramework.TechnologyXmlUtility
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System.Xml;

namespace Kerberos.Sots.Data.TechnologyFramework
{
	public class TechnologyXmlUtility
	{
		public static void LoadTechTreeFromXml(string filename, ref TechTree tt)
		{
			XmlDocument document = new XmlDocument();
			document.Load(ScriptHost.FileSystem, filename);
			tt.LoadFromXmlNode(document["TechTree"]);
		}

		public static void LoadTechTreeFromXmlForTools(string filename, ref TechTree tt)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(filename);
			tt.LoadFromXmlNode(xmlDocument["TechTree"]);
		}

		public static void SaveTechTreeToXmlForTools(string filename, TechTree tt)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement element = xmlDocument.CreateElement("TechTree");
			tt.AttachToXmlNode(ref element);
			xmlDocument.AppendChild((XmlNode)element);
			xmlDocument.Save(filename);
		}
	}
}
