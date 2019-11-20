// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.Admiral
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Collections.Generic;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class Admiral : IXmlLoadSave
	{
		public string Name = "";
		public string Portrait = "";
		public string Faction = "";
		public string Gender = "";
		public List<SpecialCharacteristic> SpecialCharacteristics = new List<SpecialCharacteristic>();
		internal const string XmlAdmiralName = "Admiral";
		private const string XmlNameName = "Name";
		private const string XmlPortraitName = "Portrait";
		private const string XmlAgeName = "Age";
		private const string XmlFactionName = "Faction";
		private const string XmlGenderName = "Gender";
		private const string XmlHomePlanetName = "HomePlanet";
		private const string XmlReactionRatingName = "ReactionRating";
		private const string XmlEvasionRatingName = "EvasionRating";
		private const string XmlSpecialCharacteristicsName = "SpecialCharacteristics";
		private const string XmlSpecialCharacteristicName = "Characteristic";
		public int Age;
		public int HomePlanet;
		public float ReactionRating;
		public float EvasionRating;

		public string XmlName
		{
			get
			{
				return nameof(Admiral);
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Name, "Name", ref node);
			XmlHelper.AddNode((object)this.Portrait, "Portrait", ref node);
			XmlHelper.AddNode((object)this.Age, "Age", ref node);
			XmlHelper.AddNode((object)this.Faction, "Faction", ref node);
			XmlHelper.AddNode((object)this.Gender, "Gender", ref node);
			XmlHelper.AddNode((object)this.HomePlanet, "HomePlanet", ref node);
			XmlHelper.AddNode((object)this.ReactionRating, "ReactionRating", ref node);
			XmlHelper.AddNode((object)this.EvasionRating, "EvasionRating", ref node);
			XmlHelper.AddCollectionNode<SpecialCharacteristic>((IEnumerable<SpecialCharacteristic>)this.SpecialCharacteristics, "SpecialCharacteristics", "Characteristic", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.Portrait = XmlHelper.GetData<string>(node, "Portrait");
			this.Age = XmlHelper.GetData<int>(node, "Age");
			this.Faction = XmlHelper.GetData<string>(node, "Faction");
			this.Gender = XmlHelper.GetData<string>(node, "Gender");
			this.HomePlanet = XmlHelper.GetData<int>(node, "HomePlanet");
			this.ReactionRating = XmlHelper.GetData<float>(node, "ReactionRating");
			this.EvasionRating = XmlHelper.GetData<float>(node, "EvasionRating");
			this.SpecialCharacteristics = XmlHelper.GetDataCollection<SpecialCharacteristic>(node, "SpecialCharacteristics", "Characteristic");
		}
	}
}
