// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.TerrainColonizedCondition
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class TerrainColonizedCondition : TriggerCondition
	{
		public string TerrainName = "";
		internal const string XmlTerrainColonizedConditionName = "TerrainColonized";
		private const string XmlTerrainNameName = "TerrainName";
		private const string XmlColonizedPercentageName = "ColonizedPercentage";
		public float ColonizedPercentage;

		public override string XmlName
		{
			get
			{
				return "TerrainColonized";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.TerrainName, "TerrainName", ref node);
			XmlHelper.AddNode((object)this.ColonizedPercentage, "ColonizedPercentage", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.TerrainName = XmlHelper.GetData<string>(node, "TerrainName");
			this.ColonizedPercentage = XmlHelper.GetData<float>(node, "ColonizedPercentage");
		}
	}
}
