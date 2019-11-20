// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.AllianceBrokenCondition
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class AllianceBrokenCondition : TriggerCondition
	{
		internal const string XmlAllianceBrokenConditionName = "AllianceBroken";
		private const string XmlPlayerSlot1Name = "PlayerSlot1";
		private const string XmlPlayerSlot2Name = "PlayerSlot2";
		public int PlayerSlot1;
		public int PlayerSlot2;

		public override string XmlName
		{
			get
			{
				return "AllianceBroken";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.PlayerSlot1, "PlayerSlot1", ref node);
			XmlHelper.AddNode((object)this.PlayerSlot2, "PlayerSlot2", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot1 = XmlHelper.GetData<int>(node, "PlayerSlot1");
			this.PlayerSlot2 = XmlHelper.GetData<int>(node, "PlayerSlot2");
		}
	}
}
