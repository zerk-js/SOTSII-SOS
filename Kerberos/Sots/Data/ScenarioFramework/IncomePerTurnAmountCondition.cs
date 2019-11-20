// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.IncomePerTurnAmountCondition
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class IncomePerTurnAmountCondition : TriggerCondition
	{
		internal const string XmlIncomePerTurnConditionName = "IncomePerTurn";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlIncomeAmountName = "IncomeAmount";
		public int PlayerSlot;
		public float IncomeAmount;

		public override string XmlName
		{
			get
			{
				return "IncomePerTurn";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode((object)this.IncomeAmount, "IncomeAmount", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.IncomeAmount = XmlHelper.GetData<float>(node, "IncomeAmount");
		}
	}
}
