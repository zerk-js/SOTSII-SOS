// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ModuleFramework.ShipModuleXmlUtility
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System.Xml;

namespace Kerberos.Sots.Data.ModuleFramework
{
	public class ShipModuleXmlUtility
	{
		private const string XmlShipModuleName = "ShipModule";

		public static void LoadShipModuleFromXml(string filename, ref ShipModule sm)
		{
			XmlDocument document = new XmlDocument();
			document.Load(ScriptHost.FileSystem, filename);
			sm.LoadFromXmlNode(document["ShipModule"]);
			sm.SavePath = filename;
		}

		public static void LoadShipModuleFromXmlForTools(string filename, ref ShipModule sm)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(filename);
			sm.LoadFromXmlNode(xmlDocument["ShipModule"]);
			sm.SavePath = filename;
		}

		public static void SaveShipModuleToXmlForTools(string filename, ShipModule ss)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement element = xmlDocument.CreateElement("ShipModule");
			ss.AttachToXmlNode(ref element);
			xmlDocument.AppendChild((XmlNode)element);
			xmlDocument.Save(filename);
		}
	}
}
