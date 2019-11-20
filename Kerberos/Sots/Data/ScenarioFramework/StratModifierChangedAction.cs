// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.StratModifierChangedAction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class StratModifierChangedAction : TriggerAction
	{
		public string StratModifier = "";
		internal const string XmlStratModifierChangedActionName = "StratModifierChanged";
		private const string XmlStratModifierName = "StratModifier";
		private const string XmlAmountChangedName = "AmountChanged";
		public float AmountChanged;

		public override string XmlName
		{
			get
			{
				return "StratModifierChanged";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.StratModifier, "StratModifier", ref node);
			XmlHelper.AddNode((object)this.AmountChanged, "AmountChanged", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.StratModifier = XmlHelper.GetData<string>(node, "StratModifier");
			this.AmountChanged = XmlHelper.GetData<float>(node, "AmountChanged");
		}
	}
}
