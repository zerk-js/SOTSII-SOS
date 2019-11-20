// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.SystemRangeCondition
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class SystemRangeCondition : TriggerCondition
	{
		internal const string XmlSystemRangeConditionName = "SystemRange";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlPlanetName = "SystemId";
		private const string XmlDistanceName = "Distance";
		public int PlayerSlot;
		public int SystemId;
		public float Distance;

		public override string XmlName
		{
			get
			{
				return "SystemRange";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode((object)this.SystemId, "SystemId", ref node);
			XmlHelper.AddNode((object)this.Distance, "Distance", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.SystemId = XmlHelper.GetData<int>(node, "SystemId");
			this.Distance = XmlHelper.GetData<float>(node, "Distance");
		}
	}
}
