// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.Fleet
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Collections.Generic;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class Fleet : IXmlLoadSave
	{
		public List<Ship> Ships = new List<Ship>();
		internal const string XmlFleetName = "Fleet";
		private const string XmlNameName = "Name";
		private const string XmlAdmiralName = "Admiral";
		private const string XmlSupportingStationName = "SupportingStation";
		private const string XmlLocationName = "Location";
		private const string XmlShipsName = "Ships";
		public string Name;
		public string Admiral;
		public string SupportingStation;
		public int Location;

		public string XmlName
		{
			get
			{
				return nameof(Fleet);
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Name, "Name", ref node);
			XmlHelper.AddNode((object)this.Admiral, "Admiral", ref node);
			XmlHelper.AddNode((object)this.SupportingStation, "SupportingStation", ref node);
			XmlHelper.AddNode((object)this.Location, "Location", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.Ships, "Ships", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.Admiral = XmlHelper.GetData<string>(node, "Admiral");
			this.SupportingStation = XmlHelper.GetData<string>(node, "SupportingStation");
			this.Location = XmlHelper.GetData<int>(node, "Location");
			this.Ships = XmlHelper.GetDataObjectCollection<Ship>(node, "Ships", "Ship");
		}
	}
}
