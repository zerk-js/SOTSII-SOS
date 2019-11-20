// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.Ship
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Collections.Generic;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class Ship : IXmlLoadSave
	{
		public string Name = "";
		public string Class = "";
		public string Faction = "";
		public List<Section> Sections = new List<Section>();
		internal const string XmlShipName = "Ship";
		private const string XmlNameName = "Name";
		private const string XmlClassName = "Class";
		private const string XmlFactionFame = "Faction";
		private const string XmlAvailableToPlayerName = "AvailableToPlayer";
		private const string XmlSectionsName = "Sections";
		public bool AvailableToPlayer;

		public string XmlName
		{
			get
			{
				return nameof(Ship);
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Name, "Name", ref node);
			XmlHelper.AddNode((object)this.Class, "Class", ref node);
			XmlHelper.AddNode((object)this.Faction, "Faction", ref node);
			XmlHelper.AddNode((object)this.AvailableToPlayer, "AvailableToPlayer", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.Sections, "Sections", "Section", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.Class = XmlHelper.GetData<string>(node, "Class");
			this.Faction = XmlHelper.GetData<string>(node, "Faction");
			this.AvailableToPlayer = XmlHelper.GetData<bool>(node, "AvailableToPlayer");
			this.Sections = XmlHelper.GetDataObjectCollection<Section>(node, "Sections", "Section");
		}
	}
}
