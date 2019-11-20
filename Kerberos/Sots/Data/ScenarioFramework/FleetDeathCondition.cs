// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.FleetDeathCondition
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class FleetDeathCondition : EventTriggerCondition
	{
		public string FleetName = "";
		internal const string XmlFleetDeathConditionName = "FleetDeath";
		private const string XmlFleetNameName = "Name";

		public override string XmlName
		{
			get
			{
				return "FleetDeath";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.FleetName, "Name", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			XmlHelper.GetData<string>(node, "Name");
		}
	}
}
