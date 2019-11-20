// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.DiplomacyChangedAction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class DiplomacyChangedAction : TriggerAction
	{
		public DiplomacyRule OldDiplomacyRule = new DiplomacyRule();
		public DiplomacyRule NewDiplomacyRule = new DiplomacyRule();
		internal const string XmlDiplomacyChangedActionName = "DiplomacyChanged";
		private const string XmlOldDiplomacyRuleName = "OldDiplomacyRule";
		private const string XmlNewDiplomacyRuleName = "NewDiplomacyRule";

		public override string XmlName
		{
			get
			{
				return "DiplomacyChanged";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((IXmlLoadSave)this.OldDiplomacyRule, "OldDiplomacyRule", ref node);
			XmlHelper.AddNode((IXmlLoadSave)this.NewDiplomacyRule, "NewDiplomacyRule", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.OldDiplomacyRule.LoadFromXmlNode(node["OldDiplomacyRule"]);
			this.NewDiplomacyRule.LoadFromXmlNode(node["NewDiplomacyRule"]);
		}
	}
}
