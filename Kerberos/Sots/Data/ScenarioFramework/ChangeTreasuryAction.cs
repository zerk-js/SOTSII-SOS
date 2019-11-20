// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.ChangeTreasuryAction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class ChangeTreasuryAction : TriggerAction
	{
		internal const string XmlChangeTreasuryActionName = "ChangedTreasury";
		private const string XmlPlayerName = "Player";
		private const string XmlAmountToAddName = "AmountToAdd";
		public int Player;
		public float AmountToAdd;

		public override string XmlName
		{
			get
			{
				return "ChangedTreasury";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Player, "Player", ref node);
			XmlHelper.AddNode((object)this.AmountToAdd, "AmountToAdd", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.Player = XmlHelper.GetData<int>(node, "Player");
			this.AmountToAdd = XmlHelper.GetData<float>(node, "AmountToAdd");
		}
	}
}
