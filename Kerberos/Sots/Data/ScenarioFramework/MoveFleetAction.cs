// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.MoveFleetAction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class MoveFleetAction : TriggerAction
	{
		public string FleetName = "";
		internal const string XmlMoveFleetActionName = "MoveFleet";
		private const string XmlFleetNameName = "Name";
		private const string XmlDestinationIdName = "DestinationId";
		public int DestinationId;

		public override string XmlName
		{
			get
			{
				return "MoveFleet";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.FleetName, "Name", ref node);
			XmlHelper.AddNode((object)this.DestinationId, "DestinationId", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.FleetName = XmlHelper.GetData<string>(node, "Name");
			this.DestinationId = XmlHelper.GetData<int>(node, "DestinationId");
		}
	}
}
