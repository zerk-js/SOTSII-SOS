// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.Trigger
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Collections.Generic;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class Trigger : IXmlLoadSave
	{
		public TriggerContext Context = (TriggerContext)new AlwaysContext();
		public List<TriggerCondition> Conditions = new List<TriggerCondition>();
		public List<TriggerAction> Actions = new List<TriggerAction>();
		internal List<FleetInfo> RangeTriggeredFleets = new List<FleetInfo>();
		internal const string XmlTriggerName = "Trigger";
		private const string XmlNameName = "Name";
		private const string XmlIsRecurring = "IsRecurring";
		private const string XmlContextName = "Context";
		private const string XmlConditionsName = "Conditions";
		private const string XmlActionsName = "Actions";
		public string Name;
		public bool IsRecurring;

		public string XmlName
		{
			get
			{
				return nameof(Trigger);
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Name, "Name", ref node);
			XmlHelper.AddNode((object)this.IsRecurring, "IsRecurring", ref node);
			XmlHelper.AddObjectNode((IXmlLoadSave)this.Context, "Context", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.Conditions, "Conditions", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.Actions, "Actions", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.IsRecurring = XmlHelper.GetData<bool>(node, "IsRecurring");
			this.Context = XmlHelper.GetDataObject<TriggerContext>(node, "Context", ScenarioEnumerations.ContextTypeMap);
			this.Conditions = XmlHelper.GetDataObjectCollection<TriggerCondition>(node, "Conditions", ScenarioEnumerations.ConditionTypeMap);
			this.Actions = XmlHelper.GetDataObjectCollection<TriggerAction>(node, "Actions", ScenarioEnumerations.ActionTypeMap);
		}
	}
}
