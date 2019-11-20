// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.WeaponFramework.WeaponXmlUtility
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System.Xml;

namespace Kerberos.Sots.Data.WeaponFramework
{
	public class WeaponXmlUtility
	{
		private const string XmlWeaponName = "Weapon";

		public static void LoadWeaponFromXml(string filename, ref Weapon w)
		{
			XmlDocument document = new XmlDocument();
			document.Load(ScriptHost.FileSystem, filename);
			w.LoadFromXmlNode(document["Weapon"]);
		}

		public static void LoadWeaponFromXmlForTools(string filename, ref Weapon w)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(filename);
			w.LoadFromXmlNode(xmlDocument["Weapon"]);
		}

		public static void SaveWeaponToXmlForTools(string filename, Weapon w)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement element = xmlDocument.CreateElement("Weapon");
			w.AttachToXmlNode(ref element);
			xmlDocument.AppendChild((XmlNode)element);
			xmlDocument.Save(filename);
		}
	}
}
