// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.FleetRangeCondition
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class FleetRangeCondition : TriggerCondition
	{
		public string FleetName = "";
		internal const string XmlFleetRangeConditionName = "FleetRange";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlFleetNameName = "FleetName";
		private const string XmlDistanceName = "Distance";
		public int PlayerSlot;
		public float Distance;

		public override string XmlName
		{
			get
			{
				return "FleetRange";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode((object)this.FleetName, "FleetName", ref node);
			XmlHelper.AddNode((object)this.Distance, "Distance", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.FleetName = XmlHelper.GetData<string>(node, "FleetName");
			this.Distance = (float)XmlHelper.GetData<int>(node, "Distance");
		}
	}
}
