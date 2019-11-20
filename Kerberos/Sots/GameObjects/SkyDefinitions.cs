// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.SkyDefinitions
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots.GameObjects
{
	internal static class SkyDefinitions
	{
		public static SkyDefinition[] LoadFromXml()
		{
			XmlDocument document = new XmlDocument();
			document.Load(ScriptHost.FileSystem, "commonassets.xml");
			XmlElement source = document["CommonAssets"];
			if (source == null)
				return new SkyDefinition[0];
			List<SkyDefinition> skyDefinitionList = new List<SkyDefinition>();
			foreach (XmlElement xmlElement in source.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(element => element.Name == "sky")))
			{
				SkyDefinition skyDefinition = new SkyDefinition();
				skyDefinition.MaterialName = xmlElement.GetAttribute("material");
				string attribute = xmlElement.GetAttribute("usage");
				if (!string.IsNullOrEmpty(attribute))
					skyDefinition.Usage = (SkyUsage)Enum.Parse(typeof(SkyUsage), attribute);
				skyDefinitionList.Add(skyDefinition);
			}
			return skyDefinitionList.ToArray();
		}
	}
}
