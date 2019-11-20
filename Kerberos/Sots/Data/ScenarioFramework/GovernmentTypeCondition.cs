// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.GovernmentTypeCondition
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class GovernmentTypeCondition : TriggerCondition
	{
		public string GovernmentType = "";
		internal const string XmlGovernmentTypeConditionName = "GovernmentType";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlGovernmentTypeName = "GovernmentType";
		public int PlayerSlot;

		public override string XmlName
		{
			get
			{
				return "GovernmentType";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode((object)this.GovernmentType, "GovernmentType", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.GovernmentType = XmlHelper.GetData<string>(node, "GovernmentType");
		}
	}
}
