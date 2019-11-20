// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.ShipDeathCondition
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.ShipFramework;
using System;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class ShipDeathCondition : EventTriggerCondition
	{
		internal const string XmlShipDeathConditionName = "ShipDeath";
		private const string XmlShipClassName = "Name";
		public ShipClass ShipClass;

		public override string XmlName
		{
			get
			{
				return "ShipDeath";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.ShipClass, "Name", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			Enum.TryParse<ShipClass>(XmlHelper.GetData<string>(node, "Name"), out this.ShipClass);
		}
	}
}
