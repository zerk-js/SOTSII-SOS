// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.StarMapFramework.StarMapXmlUtility
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System.IO;
using System.Xml;

namespace Kerberos.Sots.Data.StarMapFramework
{
	public class StarMapXmlUtility
	{
		public static void SaveStarmapToXmlForTools(string filename, Starmap s)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement element = xmlDocument.CreateElement("Starmap");
			s.AttachToXmlNode(ref element);
			xmlDocument.AppendChild((XmlNode)element);
			xmlDocument.Save(filename);
		}

		public static void LoadStarmapFromXmlForTools(string filename, ref Starmap s)
		{
			if (!File.Exists(filename))
				return;
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(filename);
			s.LoadFromXmlNode(xmlDocument["Starmap"]);
		}

		public static void LoadStarmapFromXml(string filename, ref Starmap s)
		{
			XmlDocument document = new XmlDocument();
			document.Load(ScriptHost.FileSystem, filename);
			s.LoadFromXmlNode(document["Starmap"]);
		}
	}
}
