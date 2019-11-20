// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.ShieldLibrary
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots.ShipFramework
{
	internal static class ShieldLibrary
	{
		public static IEnumerable<LogicalShield> Enumerate(XmlDocument doc)
		{
			XmlElement listnode = doc["CommonAssets"];
			XmlElement shieldNodes = listnode["Shields"];
			foreach (XmlElement xmlElement in shieldNodes.OfType<XmlElement>())
				yield return new LogicalShield()
				{
					Name = xmlElement.GetAttribute("name"),
					TechID = xmlElement.GetAttribute("techID"),
					Type = (LogicalShield.ShieldType)Enum.Parse(typeof(LogicalShield.ShieldType), xmlElement.GetAttribute("type")),
					CRShieldData = {
			Structure = float.Parse(xmlElement.GetAttribute("crHealth")),
			RechargeTime = float.Parse(xmlElement.GetAttribute("crRechargeTime")),
			RicochetMod = float.Parse(xmlElement.GetAttribute("crRicochetMod")),
			ModelFileName = xmlElement.GetAttribute("crModelFileName"),
			ImpactEffectName = xmlElement.GetAttribute("crImpactEffectName")
		  },
					DNShieldData = {
			Structure = float.Parse(xmlElement.GetAttribute("dnHealth")),
			RechargeTime = float.Parse(xmlElement.GetAttribute("dnRechargeTime")),
			RicochetMod = float.Parse(xmlElement.GetAttribute("dnRicochetMod")),
			ModelFileName = xmlElement.GetAttribute("dnModelFileName"),
			ImpactEffectName = xmlElement.GetAttribute("dnImpactEffectName")
		  }
				};
		}
	}
}
