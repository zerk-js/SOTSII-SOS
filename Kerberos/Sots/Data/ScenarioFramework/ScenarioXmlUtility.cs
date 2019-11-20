// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.ScenarioXmlUtility
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class ScenarioXmlUtility
	{
		private const string XmlScenarioName = "Scenario";

		public static void LoadScenarioFromXml(string filename, ref Scenario s)
		{
			XmlDocument document = new XmlDocument();
			document.Load(ScriptHost.FileSystem, filename);
			s.LoadFromXmlNode(document["Scenario"]);
		}

		public static void LoadScenarioFromXmlForTools(string filename, ref Scenario s)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(filename);
			s.LoadFromXmlNode(xmlDocument["Scenario"]);
		}

		public static void SaveScenarioToXmlForTools(string filename, Scenario s)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement element = xmlDocument.CreateElement("Scenario");
			s.AttachToXmlNode(ref element);
			xmlDocument.AppendChild((XmlNode)element);
			xmlDocument.Save(filename);
		}
	}
}
