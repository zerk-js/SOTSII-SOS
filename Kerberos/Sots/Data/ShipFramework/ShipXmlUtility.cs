// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ShipFramework.ShipXmlUtility
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System.Xml;

namespace Kerberos.Sots.Data.ShipFramework
{
	public class ShipXmlUtility
	{
		public static void LoadShipSectionFromXml(string filename, ref ShipSection ss)
		{
			XmlDocument document = new XmlDocument();
			document.Load(ScriptHost.FileSystem, filename);
			ss.LoadFromXmlNode(document["ShipSection"]);
		}

		public static void LoadShipSectionFromXmlForTools(string filename, ref ShipSection ss)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(filename);
			ss.LoadFromXmlNode(xmlDocument["ShipSection"]);
		}

		public static void SaveShipSectionToXmlForTools(string filename, ShipSection ss)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement element = xmlDocument.CreateElement("ShipSection");
			ss.AttachToXmlNode(ref element);
			xmlDocument.AppendChild((XmlNode)element);
			xmlDocument.Save(filename);
		}
	}
}
