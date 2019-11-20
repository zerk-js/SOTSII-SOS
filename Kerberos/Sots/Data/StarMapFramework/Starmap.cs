// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.StarMapFramework.Starmap
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Kerberos.Sots.Data.StarMapFramework
{
	public class Starmap
	{
		public int NumPlayers = 8;
		public string Title = string.Empty;
		public string Description = string.Empty;
		public List<Feature> Features = new List<Feature>();
		public List<NodeLine> NodeLines = new List<NodeLine>();
		public List<Province> Provinces = new List<Province>();
		internal const string XmlNumPlayersName = "NumPlayers";
		internal const string XmlStarmapName = "Starmap";
		internal const string XmlPreviewTextureName = "PreviewTexture";
		internal const string XmlFeaturesName = "Features";
		internal const string XmlNodeLinesName = "NodeLines";
		internal const string XmlProvincesName = "Provinces";
		internal const string XmlTitleName = "Title";
		internal const string XmlDescriptionName = "Description";
		public string PreviewTexture;

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.NumPlayers, "NumPlayers", ref node);
			XmlHelper.AddNode((object)this.PreviewTexture, "PreviewTexture", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.Features, "Features", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.NodeLines, "NodeLines", "NodeLine", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.Provinces, "Provinces", "Province", ref node);
			XmlHelper.AddNode((object)this.Title, "Title", ref node);
			XmlHelper.AddNode((object)this.Description, "Description", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			if (node == null)
				return;
			this.NumPlayers = XmlHelper.GetData<int>(node, "NumPlayers");
			if (this.NumPlayers == 0)
				this.NumPlayers = 8;
			this.PreviewTexture = XmlHelper.GetData<string>(node, "PreviewTexture");
			this.Features = XmlHelper.GetDataObjectCollection<Feature>(node, "Features", Feature.TypeMap);
			this.NodeLines = XmlHelper.GetDataObjectCollection<NodeLine>(node, "NodeLines", "NodeLine");
			this.Provinces = XmlHelper.GetDataObjectCollection<Province>(node, "Provinces", "Province");
			this.Title = XmlHelper.GetData<string>(node, "Title");
			this.Description = XmlHelper.GetData<string>(node, "Description");
		}

		public class StarmapInfo
		{
			public string FileName;
			public string Title;
			public string Description;
			public int NumPlayers;
			public string PreviewTexture;

			public string GetFallbackTitle()
			{
				if (!string.IsNullOrEmpty(this.Title))
					return this.Title;
				return Path.GetFileNameWithoutExtension(this.FileName);
			}

			public StarmapInfo(string filename)
			{
				XmlDocument document = new XmlDocument();
				document.Load(ScriptHost.FileSystem, filename);
				XmlElement xmlElement = document[nameof(Starmap)];
				this.FileName = filename;
				this.NumPlayers = xmlElement[nameof(NumPlayers)].ExtractIntegerOrDefault(0);
				this.PreviewTexture = xmlElement[nameof(PreviewTexture)].ExtractStringOrDefault(string.Empty);
				this.Title = xmlElement[nameof(Title)].ExtractStringOrDefault(string.Empty);
				this.Description = xmlElement[nameof(Description)].ExtractStringOrDefault(string.Empty);
			}
		}
	}
}
