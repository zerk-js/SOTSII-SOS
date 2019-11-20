// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.AdmiralDeathCondition
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class AdmiralDeathCondition : EventTriggerCondition
	{
		public string AdmiralName = "";
		internal const string XmlAdmiralDeathConditionName = "AdmiralDeath";
		private const string XmlAdmiralNameName = "AdmiralName";

		public override string XmlName
		{
			get
			{
				return "AdmiralDeath";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.AdmiralName, "AdmiralName", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.AdmiralName = XmlHelper.GetData<string>(node, "AdmiralName");
		}
	}
}
