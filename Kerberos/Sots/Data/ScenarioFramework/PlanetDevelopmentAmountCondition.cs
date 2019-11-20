// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.PlanetDevelopmentAmountCondition
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class PlanetDevelopmentAmountCondition : TriggerCondition
	{
		internal const string XmlPlanetDevelopmentAmountConditionName = "PlanetDevelopmentAmount";

		public override string XmlName
		{
			get
			{
				return "PlanetDevelopmentAmount";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
		}
	}
}
