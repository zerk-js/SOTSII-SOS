// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.StarMapFramework.Terrain
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Collections.Generic;
using System.Xml;

namespace Kerberos.Sots.Data.StarMapFramework
{
	public class Terrain : Feature
	{
		public List<Feature> Features = new List<Feature>();
		public List<NodeLine> NodeLines = new List<NodeLine>();
		public List<Province> Provinces = new List<Province>();
		internal const string XmlTerrainName = "Terrain";
		private const string XmlFeaturesName = "Features";
		private const string XmlNodeLinesName = "NodeLines";
		private const string XmlProvincesName = "Provinces";

		public override string XmlName
		{
			get
			{
				return nameof(Terrain);
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			base.AttachToXmlNode(ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.Features, "Features", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.NodeLines, "NodeLines", "NodeLine", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.Provinces, "Provinces", "Province", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			if (node == null)
				return;
			base.LoadFromXmlNode(node);
			this.Features = XmlHelper.GetDataObjectCollection<Feature>(node, "Features", Feature.TypeMap);
			this.NodeLines = XmlHelper.GetDataObjectCollection<NodeLine>(node, "NodeLines", "NodeLine");
			this.Provinces = XmlHelper.GetDataObjectCollection<Province>(node, "Provinces", "Province");
		}
	}
}
