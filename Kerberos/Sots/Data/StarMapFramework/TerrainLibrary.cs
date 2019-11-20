// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.StarMapFramework.TerrainLibrary
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Collections.Generic;
using System.Xml;

namespace Kerberos.Sots.Data.StarMapFramework
{
	public class TerrainLibrary
	{
		public List<Feature> Features = new List<Feature>();
		internal const string XmlTerrainLibraryName = "TerrainLibrary";
		internal const string XmlFeaturesName = "Features";

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.Features, "Features", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			if (node == null)
				return;
			this.Features = XmlHelper.GetDataObjectCollection<Feature>(node, "Features", Feature.TypeMap);
		}
	}
}
