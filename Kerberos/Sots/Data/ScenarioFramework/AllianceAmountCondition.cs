// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.AllianceAmountCondition
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class AllianceAmountCondition : TriggerCondition
	{
		internal const string XmlAllianceAmountConditionName = "AllianceAmount";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlNumAlliancesName = "NumAlliances";
		public int PlayerSlot;
		public int NumberOfAlliances;

		public override string XmlName
		{
			get
			{
				return "AllianceAmount";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode((object)this.NumberOfAlliances, "NumAlliances", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.NumberOfAlliances = XmlHelper.GetData<int>(node, "NumAlliances");
		}
	}
}
