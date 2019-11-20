// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.PointPerShipTypeAction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class PointPerShipTypeAction : TriggerAction
	{
		public string Fleet = "";
		public string ShipType = "";
		public string ScalarName = "";
		internal const string XmlPointPerShipTypeActionName = "PointPerShipTypeAction";
		private const string XmlFleetName = "Fleet";
		private const string XmlShipTypeName = "ShipType";
		private const string XmlAmountPerShipName = "AmountPerShip";
		private const string XmlScalarNameName = "ScalarName";
		public float AmountPerShip;

		public override string XmlName
		{
			get
			{
				return nameof(PointPerShipTypeAction);
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Fleet, "Fleet", ref node);
			XmlHelper.AddNode((object)this.ShipType, "ShipType", ref node);
			XmlHelper.AddNode((object)this.AmountPerShip, "AmountPerShip", ref node);
			XmlHelper.AddNode((object)this.ScalarName, "ScalarName", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.Fleet = XmlHelper.GetData<string>(node, "Fleet");
			this.ShipType = XmlHelper.GetData<string>(node, "ShipType");
			this.AmountPerShip = XmlHelper.GetData<float>(node, "AmountPerShip");
			this.ScalarName = XmlHelper.GetData<string>(node, "ScalarName");
		}
	}
}
