// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.WeaponFramework.RangeTable
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.WeaponFramework
{
	public class RangeTable : IXmlLoadSave
	{
		private const string XmlPbRangeName = "PbRange";
		private const string XmlPbDeviationName = "PbDeviation";
		private const string XmlPbDamageName = "PbDamage";
		private const string XmlEffectiveRangeName = "EffectiveRange";
		private const string XmlEffectiveDeviationName = "EffectiveDeviation";
		private const string XmlEffectiveDamageName = "EffectiveDamage";
		private const string XmlMaxRangeName = "MaxRange";
		private const string XmlMaxDeviationName = "MaxDeviation";
		private const string XmlMaxDamageName = "MaxDamage";
		private const string XmlPlanetRangeName = "PlanetRange";
		public float PbRange;
		public float PbDeviation;
		public float PbDamage;
		public float EffectiveRange;
		public float EffectiveDeviation;
		public float EffectiveDamage;
		public float MaxRange;
		public float MaxDeviation;
		public float MaxDamage;
		public float PlanetRange;

		public string XmlName
		{
			get
			{
				throw XmlHelper.NoXmlNameException;
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.PbRange, "PbRange", ref node);
			XmlHelper.AddNode((object)this.PbDeviation, "PbDeviation", ref node);
			XmlHelper.AddNode((object)this.PbDamage, "PbDamage", ref node);
			XmlHelper.AddNode((object)this.EffectiveRange, "EffectiveRange", ref node);
			XmlHelper.AddNode((object)this.EffectiveDeviation, "EffectiveDeviation", ref node);
			XmlHelper.AddNode((object)this.EffectiveDamage, "EffectiveDamage", ref node);
			XmlHelper.AddNode((object)this.MaxRange, "MaxRange", ref node);
			XmlHelper.AddNode((object)this.MaxDeviation, "MaxDeviation", ref node);
			XmlHelper.AddNode((object)this.MaxDamage, "MaxDamage", ref node);
			XmlHelper.AddNode((object)this.PlanetRange, "PlanetRange", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			if (node == null)
				return;
			this.PbRange = XmlHelper.GetData<float>(node, "PbRange");
			this.PbDeviation = XmlHelper.GetData<float>(node, "PbDeviation");
			this.PbDamage = XmlHelper.GetData<float>(node, "PbDamage");
			this.EffectiveRange = XmlHelper.GetData<float>(node, "EffectiveRange");
			this.EffectiveDeviation = XmlHelper.GetData<float>(node, "EffectiveDeviation");
			this.EffectiveDamage = XmlHelper.GetData<float>(node, "EffectiveDamage");
			this.MaxRange = XmlHelper.GetData<float>(node, "MaxRange");
			this.MaxDeviation = XmlHelper.GetData<float>(node, "MaxDeviation");
			this.MaxDamage = XmlHelper.GetData<float>(node, "MaxDamage");
			this.PlanetRange = XmlHelper.GetData<float>(node, "PlanetRange");
		}
	}
}
