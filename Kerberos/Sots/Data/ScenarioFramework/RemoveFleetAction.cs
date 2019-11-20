// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.RemoveFleetAction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class RemoveFleetAction : TriggerAction
	{
		public string FleetName = "";
		internal const string XmlRemoveFleetActionName = "RemoveFleetAction";
		private const string XmlFleetNameName = "FleetName";

		public override string XmlName
		{
			get
			{
				return nameof(RemoveFleetAction);
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.FleetName, "FleetName", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.FleetName = XmlHelper.GetData<string>(node, "FleetName");
		}
	}
}
