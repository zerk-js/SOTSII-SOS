// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.ChangeResourcesAction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class ChangeResourcesAction : TriggerAction
	{
		public string OrbitName = "";
		internal const string XmlChangeResourcesActionName = "ChangeResources";
		private const string XmlSystemIdName = "SystemId";
		private const string XmlOrbitNameName = "OrbitName";
		private const string XmlAmountToAddName = "AmountToAdd";
		public int SystemId;
		public float AmountToAdd;

		public override string XmlName
		{
			get
			{
				return "ChangeResources";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.SystemId, "SystemId", ref node);
			XmlHelper.AddNode((object)this.OrbitName, "OrbitName", ref node);
			XmlHelper.AddNode((object)this.AmountToAdd, "AmountToAdd", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.SystemId = XmlHelper.GetData<int>(node, "SystemId");
			this.OrbitName = XmlHelper.GetData<string>(node, "OrbitName");
			this.AmountToAdd = XmlHelper.GetData<float>(node, "AmountToAdd");
		}
	}
}
