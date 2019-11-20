// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.CivilianPopulation
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class CivilianPopulation : IXmlLoadSave
	{
		public string Faction = "";
		internal const string XmlCivilianPopulationName = "CivilianPopulation";
		private const string XmlFactionName = "Faction";
		private const string XmlPopulationName = "Population";
		public double Population;

		public string XmlName
		{
			get
			{
				return nameof(CivilianPopulation);
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Faction, "Faction", ref node);
			XmlHelper.AddNode((object)this.Population, "Population", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.Faction = XmlHelper.GetData<string>(node, "Faction");
			this.Population = XmlHelper.GetData<double>(node, "Population");
		}
	}
}
