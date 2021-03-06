﻿// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.TradePointsAmountCondition
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class TradePointsAmountCondition : TriggerCondition
	{
		internal const string XmlTradePointsAmountConditionName = "TradePointsAmount";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlTradePointAmountName = "TradePointAmount";
		public int PlayerSlot;
		public float TradePointAmount;

		public override string XmlName
		{
			get
			{
				return "TradePointsAmount";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode((object)this.TradePointAmount, "TradePointAmount", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.TradePointAmount = XmlHelper.GetData<float>(node, "TradePointAmount");
		}
	}
}
