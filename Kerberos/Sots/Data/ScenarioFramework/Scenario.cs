// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.Scenario
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class Scenario : IXmlLoadSave
	{
		public string Name = "";
		public string Starmap = "";
		public List<Player> PlayerStartConditions = new List<Player>();
		public List<DiplomacyRule> DiplomacyRules = new List<DiplomacyRule>();
		public List<Trigger> Triggers = new List<Trigger>();
		internal const string XmlScenarioName = "Scenario";
		private const string XmlNameName = "Name";
		private const string XmlStarmapName = "Starmap";
		private const string XmlPlayerStartConditionsName = "PlayerStartConditions";
		private const string XmlDiplomacyRulesName = "DiplomacyRules";
		private const string XmlTriggersName = "Triggers";
		private const string XmlEconomicEfficiencyName = "EconomicEfficiency";
		private const string XmlResearchEfficiencyName = "ResearchEfficiency";
		public int EconomicEfficiency;
		public int ResearchEfficiency;

		public string XmlName
		{
			get
			{
				return nameof(Scenario);
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Name, "Name", ref node);
			XmlHelper.AddNode((object)this.Starmap, "Starmap", ref node);
			XmlHelper.AddNode((object)this.EconomicEfficiency, "EconomicEfficiency", ref node);
			XmlHelper.AddNode((object)this.ResearchEfficiency, "ResearchEfficiency", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.PlayerStartConditions, "PlayerStartConditions", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.DiplomacyRules, "DiplomacyRules", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.Triggers, "Triggers", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.Starmap = XmlHelper.GetData<string>(node, "Starmap");
			this.EconomicEfficiency = XmlHelper.GetDataOrDefault<int>(node["EconomicEfficiency"], 100);
			this.ResearchEfficiency = XmlHelper.GetDataOrDefault<int>(node["ResearchEfficiency"], 100);
			this.PlayerStartConditions = XmlHelper.GetDataObjectCollection<Player>(node, "PlayerStartConditions", "Player");
			this.DiplomacyRules = XmlHelper.GetDataObjectCollection<DiplomacyRule>(node, "DiplomacyRules", "DiplomacyRule");
			this.Triggers = XmlHelper.GetDataObjectCollection<Trigger>(node, "Triggers", "Trigger");
		}

		public class ScenarioInfo
		{
			public string FileName;
			public string Title;
			public Kerberos.Sots.Data.StarMapFramework.Starmap.StarmapInfo StarmapInfo;

			public string GetFallbackTitle()
			{
				if (!string.IsNullOrEmpty(this.Title))
					return this.Title;
				return Path.GetFileNameWithoutExtension(this.FileName);
			}

			public ScenarioInfo(string filename)
			{
				XmlDocument document = new XmlDocument();
				document.Load(ScriptHost.FileSystem, filename);
				XmlElement xmlElement = document[nameof(Scenario)];
				this.FileName = filename;
				this.Title = xmlElement["Name"].ExtractStringOrDefault(string.Empty);
				this.StarmapInfo = new Kerberos.Sots.Data.StarMapFramework.Starmap.StarmapInfo(xmlElement["Starmap"].ExtractStringOrDefault(string.Empty));
			}
		}
	}
}
