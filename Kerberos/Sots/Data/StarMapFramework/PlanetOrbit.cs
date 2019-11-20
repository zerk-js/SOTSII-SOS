// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.StarMapFramework.PlanetOrbit
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.StarMapFramework
{
	public class PlanetOrbit : Orbit
	{
		public string PlanetType = "";
		public string MaterialName = "";
		public const string XmlPlanetOrbitName = "Planet";
		public const string XmlSizeName = "Size";
		public const string XmlSuitabilityName = "Suitability";
		public const string XmlResourcesName = "Resources";
		public const string XmlPlanetTypeName = "PlanetType";
		public const string XmlInhabitedByPlayerName = "InhabitedByPlayer";
		public const string XmlCapitalOrbitName = "CapitalOrbit";
		public const string XmlBiosphereName = "Biosphere";
		public const string XmlMaterialName = "MaterialName";
		private int? _size;
		public float? Suitability;
		public int? Resources;
		public int? InhabitedByPlayer;
		public bool CapitalOrbit;
		public int? Biosphere;

		public int? Size
		{
			get
			{
				return this._size;
			}
			set
			{
				if (value.HasValue && value.Value > 0)
					this._size = value;
				else
					this._size = new int?();
			}
		}

		public override string XmlName
		{
			get
			{
				return "Planet";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			base.AttachToXmlNode(ref node);
			XmlHelper.AddNode((object)this.Size, "Size", ref node);
			XmlHelper.AddNode((object)this.Suitability, "Suitability", ref node);
			XmlHelper.AddNode((object)this.Resources, "Resources", ref node);
			XmlHelper.AddNode((object)this.PlanetType, "PlanetType", ref node);
			XmlHelper.AddNode((object)this.InhabitedByPlayer, "InhabitedByPlayer", ref node);
			XmlHelper.AddNode((object)this.CapitalOrbit, "CapitalOrbit", ref node);
			XmlHelper.AddNode((object)this.Biosphere, "Biosphere", ref node);
			XmlHelper.AddNode((object)this.MaterialName, "MaterialName", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			if (node == null)
				return;
			base.LoadFromXmlNode(node);
			this.Size = XmlHelper.GetData<int?>(node, "Size");
			int? data = XmlHelper.GetData<int?>(node, "Suitability");
			this.Suitability = data.HasValue ? new float?((float)data.GetValueOrDefault()) : new float?();
			this.Resources = XmlHelper.GetData<int?>(node, "Resources");
			this.PlanetType = XmlHelper.GetData<string>(node, "PlanetType");
			this.InhabitedByPlayer = XmlHelper.GetData<int?>(node, "InhabitedByPlayer");
			this.CapitalOrbit = XmlHelper.GetData<bool>(node, "CapitalOrbit");
			this.Biosphere = XmlHelper.GetData<int?>(node, "Biosphere");
			this.MaterialName = XmlHelper.GetData<string>(node, "MaterialName");
		}
	}
}
