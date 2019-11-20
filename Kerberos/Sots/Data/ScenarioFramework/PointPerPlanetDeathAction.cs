// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.PointPerPlanetDeathAction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class PointPerPlanetDeathAction : TriggerAction
	{
		public string ScalarName = "";
		internal const string XmlPointPerPlanetDeathActionName = "PointPerPlanetDeathAction";
		private const string XmlScalarNameName = "Name";
		private const string XmlAmountPerPlanetName = "AmountPerPlanet";
		public float AmountPerPlanet;

		public override string XmlName
		{
			get
			{
				return nameof(PointPerPlanetDeathAction);
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.ScalarName, "Name", ref node);
			XmlHelper.AddNode((object)this.AmountPerPlanet, "AmountPerPlanet", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.ScalarName = XmlHelper.GetData<string>(node, "Name");
			this.AmountPerPlanet = XmlHelper.GetData<float>(node, "AmountPerPlanet");
		}
	}
}
