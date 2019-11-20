// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.Colony
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Collections.Generic;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class Colony : IXmlLoadSave
	{
		public List<CivilianPopulation> CivilianPopulations = new List<CivilianPopulation>();
		internal const string XmlColonyName = "Colony";
		private const string XmlSystemIdName = "SystemId";
		private const string XmlOrbitIdName = "OrbitId";
		private const string XmlImperialPopulationName = "ImperialPopulation";
		private const string XmlCivilianPopulationsName = "CivilianPopulations";
		private const string XmlInfrastructureName = "Infrastructure";
		private const string XmlIsIdealColonyName = "IsIdealColony";
		public int SystemId;
		public int OrbitId;
		public double ImperialPopulation;
		public double Infrastructure;
		public bool IsIdealColony;

		public string XmlName
		{
			get
			{
				return nameof(Colony);
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.SystemId, "SystemId", ref node);
			XmlHelper.AddNode((object)this.OrbitId, "OrbitId", ref node);
			XmlHelper.AddNode((object)this.ImperialPopulation, "ImperialPopulation", ref node);
			XmlHelper.AddNode((object)this.Infrastructure, "Infrastructure", ref node);
			XmlHelper.AddNode((object)this.IsIdealColony, "IsIdealColony", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.CivilianPopulations, "CivilianPopulations", "CivilianPopulation", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.SystemId = XmlHelper.GetData<int>(node, "SystemId");
			this.OrbitId = XmlHelper.GetData<int>(node, "OrbitId");
			this.ImperialPopulation = XmlHelper.GetData<double>(node, "ImperialPopulation");
			this.Infrastructure = XmlHelper.GetData<double>(node, "Infrastructure");
			this.IsIdealColony = XmlHelper.GetData<bool>(node, "IsIdealColony");
			this.CivilianPopulations = XmlHelper.GetDataObjectCollection<CivilianPopulation>(node, "CivilianPopulations", "CivilianPopulation");
		}
	}
}
