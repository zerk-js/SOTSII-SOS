// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.SectionLibrary
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots.ShipFramework
{
	internal static class SectionLibrary
	{
		private static IEnumerable<ShipSectionAsset> LoadXml(
		  AssetDatabase assetdb,
		  string filename,
		  string faction)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(ScriptHost.FileSystem, filename);
			XmlElement listnode = doc["SectionList"];
			XmlElement sectionsnode = listnode["Sections"];
			foreach (XmlElement xmlElement in sectionsnode.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "Section")))
			{
				string sectionFileName = PathHelpers.FixSeparators(xmlElement.GetAttribute("File"));
				ShipSectionAsset logsection = new ShipSectionAsset()
				{
					FileName = sectionFileName
				};
				logsection.LoadFromXml(assetdb, sectionFileName, faction, (ShipSectionType)Enum.Parse(typeof(ShipSectionType), xmlElement.GetAttribute("Type")), (ShipClass)Enum.Parse(typeof(ShipClass), xmlElement.GetAttribute("Class")));
				yield return logsection;
			}
		}

		public static IEnumerable<ShipSectionAsset> Enumerate(
		  AssetDatabase assetdb)
		{
			foreach (string directory in ScriptHost.FileSystem.FindDirectories("factions\\*"))
			{
				string filename = PathHelpers.Combine(directory, "sections\\_sections.xml");
				if (ScriptHost.FileSystem.FileExists(filename))
				{
					string faction = Path.GetFileNameWithoutExtension(directory);
					IEnumerator<ShipSectionAsset> enumerator = SectionLibrary.LoadXml(assetdb, filename, faction).GetEnumerator();
					while (enumerator.MoveNext())
					{
						ShipSectionAsset section = enumerator.Current;
						yield return section;
					}
				}
			}
		}
	}
}
