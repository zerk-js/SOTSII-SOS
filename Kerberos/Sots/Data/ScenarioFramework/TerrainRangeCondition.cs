// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.TerrainRangeCondition
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class TerrainRangeCondition : TriggerCondition
	{
		public string TerrainName = "";
		internal const string XmlTerrainRangeConditionName = "TerrainRange";
		private const string XmlTerrainNameName = "TerrainName";
		private const string XmlDistanceName = "Distance";
		public float Distance;

		public override string XmlName
		{
			get
			{
				return "TerrainRange";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.TerrainName, "TerrainName", ref node);
			XmlHelper.AddNode((object)this.Distance, "Distance", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.TerrainName = XmlHelper.GetData<string>(node, "TerrainName");
			this.Distance = XmlHelper.GetData<float>(node, "Distance");
		}
	}
}
