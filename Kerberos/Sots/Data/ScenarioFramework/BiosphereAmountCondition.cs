// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.BiosphereAmountCondition
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class BiosphereAmountCondition : TriggerCondition
	{
		internal const string XmlBiosphereAmountConditionName = "BiosphereAmount";
		private const string XmlSystemIdName = "SystemId";
		private const string XmlOrbitNameName = "OrbitName";
		private const string XmlBiosphereAmountName = "BiosphereAmount";
		public int SystemId;
		public string OrbitName;
		public float BiosphereAmount;

		public override string XmlName
		{
			get
			{
				return "BiosphereAmount";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.SystemId, "SystemId", ref node);
			XmlHelper.AddNode((object)this.OrbitName, "OrbitName", ref node);
			XmlHelper.AddNode((object)this.BiosphereAmount, "BiosphereAmount", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.SystemId = XmlHelper.GetData<int>(node, "SystemId");
			this.OrbitName = XmlHelper.GetData<string>(node, "OrbitName");
			this.BiosphereAmount = XmlHelper.GetData<float>(node, "BiosphereAmount");
		}
	}
}
