// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.StarMapFramework.TerrainLibXmlUtility
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.IO;
using System.Xml;

namespace Kerberos.Sots.Data.StarMapFramework
{
	public class TerrainLibXmlUtility
	{
		public static void SaveTerrainLibToXml(string filename, TerrainLibrary tl)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement element = xmlDocument.CreateElement("TerrainLibrary");
			tl.AttachToXmlNode(ref element);
			xmlDocument.AppendChild((XmlNode)element);
			xmlDocument.Save(filename);
		}

		public static void LoadTerrainLibFromXml(string filename, ref TerrainLibrary tl)
		{
			if (!File.Exists(filename))
				return;
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(filename);
			tl.LoadFromXmlNode(xmlDocument["TerrainLibrary"]);
		}
	}
}
