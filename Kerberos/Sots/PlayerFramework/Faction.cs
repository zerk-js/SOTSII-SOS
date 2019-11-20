// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.PlayerFramework.Faction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Steam;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots.PlayerFramework
{
	internal class Faction
	{
		public readonly float[] MaxPopulationMod = new float[3];
		public readonly float[] MaxAlienPopulationMod = new float[3];
		public readonly float[] AIFastResearchRate = new float[6];
		private Dictionary<string, int> _defaultReactionValue = new Dictionary<string, int>();
		private Dictionary<string, float> _ImmigrationPopBonusValue = new Dictionary<string, float>();
		private Dictionary<string, float> _SpyingBonusValue = new Dictionary<string, float>();
		private Dictionary<GovernmentInfo.GovernmentType, Dictionary<MoralEvent, int>> _factionSpecificMoral = new Dictionary<GovernmentInfo.GovernmentType, Dictionary<MoralEvent, int>>();
		public int RepSel = 3;
		public string FactionFileName;
		public Subfaction[] Subfactions;
		public string Directory;
		public string AssetPath;
		public string WeaponModelPath;
		public string Name;
		public int ID;
		public bool IsPlayable;
		public bool UsesNPCCombatAI;
		public string NoAvatar;
		public string[] MaterialDictionaries;
		public string[] BadgeTexturePaths;
		public string[] AvatarTexturePaths;
		public float EntryPointOffset;
		public float StarTearTechEnteryPointOffset;
		public float PsionicPowerPerCrew;
		public float PsionicPowerModifier;
		public float CrewEfficiencyValue;
		public BoardingActionModifiers BoardingActionMods;
		public FactionDecalInfo[] StructDecalInfo;
		public FactionDecalInfo[] ScorchDecalInfo;
		public readonly ShipRole[] DefaultBattleRiderShipRoles;
		public readonly ShipRole[] DefaultStandardShipRoles;
		public readonly ShipRole[] DefaultAIShipRoles;
		public readonly InitialDesign[] InitialDesigns;
		public readonly DiplomacyActionWeights DiplomacyWeights;
		public readonly IndyDesc IndyDescrition;
		public readonly float ResearchBoostFailureMod;
		public readonly ShipRole[] DefaultCombinedShipRoles;
		private XmlElement StratModifiers;
		public int DefaultRepSel;
		public int? _dlcID;
		private FactionObject _factionObject;
		private int _factionObjectCount;

		public IEnumerable<string> TechTreeModels { get; private set; }

		public IEnumerable<string> TechTreeRoots { get; private set; }

		public LocalizedNameGrabBag DesignNames { get; private set; }

		public LocalizedNameGrabBag EmpireNames { get; private set; }

		public int GetDefaultReactionToFaction(Faction faction)
		{
			int num = 0;
			if (this._defaultReactionValue.TryGetValue(faction.Name, out num))
				return num;
			return DiplomacyInfo.DefaultDeplomacyRelations;
		}

		public float GetImmigrationPopBonusValueForFaction(Faction faction)
		{
			float num = 1f;
			if (this._ImmigrationPopBonusValue.TryGetValue(faction.Name, out num))
				return num;
			return 1f;
		}

		public float GetSpyingBonusValueForFaction(Faction faction)
		{
			float num = 0.0f;
			if (this._SpyingBonusValue.TryGetValue(faction.Name, out num))
				return num;
			return 0.0f;
		}

		public int GetMoralValue(GovernmentInfo.GovernmentType gt, MoralEvent me, int originalValue)
		{
			Dictionary<MoralEvent, int> dictionary;
			if (this._factionSpecificMoral.TryGetValue(gt, out dictionary))
			{
				int num = 0;
				if (this._factionSpecificMoral[gt].TryGetValue(me, out num))
					return num;
			}
			return originalValue;
		}

		public bool CanFactionObtainTechBranch(string branchType)
		{
			if (!(this.Name == "loa"))
				return true;
			if (branchType != "PSI")
				return branchType != "CYB";
			return false;
		}

		public int? DlcID
		{
			get
			{
				return this._dlcID;
			}
		}

		public FactionObject FactionObj
		{
			get
			{
				return this._factionObject;
			}
		}

		private static IEnumerable<ShipRole> EnumerateDefaultBattleRiderShipRoles(
		  string factionName)
		{
			if (factionName == "zuul")
				yield return ShipRole.SLAVEDISK;
			yield return ShipRole.DRONE;
			yield return ShipRole.ASSAULTSHUTTLE;
			yield return ShipRole.BOARDINGPOD;
			yield return ShipRole.BIOMISSILE;
		}

		private static IEnumerable<ShipRole> EnumerateDefaultStandardShipRoles(
		  string factionName)
		{
			yield return ShipRole.COMMAND;
			yield return ShipRole.COMBAT;
			yield return ShipRole.COLONIZER;
			yield return ShipRole.SUPPLY;
			yield return ShipRole.CONSTRUCTOR;
			yield return ShipRole.TRAPDRONE;
			if (factionName == "hiver")
				yield return ShipRole.GATE;
			if (factionName == "zuul")
				yield return ShipRole.BORE;
			if (factionName == "morrigi")
				yield return ShipRole.GRAVBOAT;
			if (factionName == "loa")
			{
				yield return ShipRole.ACCELERATOR_GATE;
				yield return ShipRole.LOA_CUBE;
			}
			yield return ShipRole.POLICE;
		}

		private static IEnumerable<ShipRole> EnumerateDefaultAIShipRoles(string factionName)
		{
			yield return ShipRole.BR_ESCORT;
			yield return ShipRole.BR_INTERCEPTOR;
			yield return ShipRole.BR_PATROL;
			yield return ShipRole.BR_SCOUT;
			yield return ShipRole.BR_SPINAL;
			yield return ShipRole.BR_TORPEDO;
			yield return ShipRole.BATTLECRUISER;
			yield return ShipRole.BATTLESHIP;
			foreach (ShipRole x in Faction.EnumerateDefaultStandardShipRoles(factionName))
			{
				yield return x;
			}
			yield break;
		}

		private static IEnumerable<ShipRole> EnumerateDefaultCombinedShipRoles(
		  string factionName)
		{
			return Faction.EnumerateDefaultBattleRiderShipRoles(factionName).Concat<ShipRole>(Faction.EnumerateDefaultStandardShipRoles(factionName));
		}

		private Faction(string filename)
		{
			for (int index = 0; index < 3; ++index)
			{
				this.MaxPopulationMod[index] = 1f;
				this.MaxAlienPopulationMod[index] = 1f;
			}
			for (int index = 0; index < 6; ++index)
				this.AIFastResearchRate[index] = 0.6f;
			this.FactionFileName = filename;
			this.Directory = Path.GetDirectoryName(filename);
			this.Name = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(filename));
			this.DefaultBattleRiderShipRoles = Faction.EnumerateDefaultBattleRiderShipRoles(this.Name).ToArray<ShipRole>();
			this.DefaultStandardShipRoles = Faction.EnumerateDefaultStandardShipRoles(this.Name).ToArray<ShipRole>();
			this.DefaultCombinedShipRoles = Faction.EnumerateDefaultCombinedShipRoles(this.Name).ToArray<ShipRole>();
			this.DefaultAIShipRoles = Faction.EnumerateDefaultAIShipRoles(this.Name).ToArray<ShipRole>();
			switch (this.Name)
			{
				case "human":
				case "tarkas":
				case "liir_zuul":
				case "zuul":
				case "morrigi":
				case "hiver":
				case "loa":
					this.IsPlayable = true;
					break;
				default:
					this.IsPlayable = false;
					break;
			}
			switch (this.Name)
			{
				case "human":
					this._dlcID = new int?(202240);
					this.Subfactions = new Subfaction[2]
					{
			new Subfaction(),
			new Subfaction()
			{
			  DlcID = new SteamDLCIdentifiers?(SteamDLCIdentifiers.SolForceImmersionPack),
			  MountName = "dlc_human"
			}
					};
					break;
				case "tarkas":
					this._dlcID = new int?(202220);
					this.Subfactions = new Subfaction[2]
					{
			new Subfaction(),
			new Subfaction()
			{
			  DlcID = new SteamDLCIdentifiers?(SteamDLCIdentifiers.HiverAndTarkasImmersionPack),
			  MountName = "dlc_tarkas"
			}
					};
					break;
				case "hiver":
					this._dlcID = new int?(202220);
					this.Subfactions = new Subfaction[2]
					{
			new Subfaction(),
			new Subfaction()
			{
			  DlcID = new SteamDLCIdentifiers?(SteamDLCIdentifiers.HiverAndTarkasImmersionPack),
			  MountName = "dlc_hiver"
			}
					};
					break;
				case "liir_zuul":
					this._dlcID = new int?(202230);
					this.Subfactions = new Subfaction[2]
					{
			new Subfaction(),
			new Subfaction()
			{
			  DlcID = new SteamDLCIdentifiers?(SteamDLCIdentifiers.LiirAndMorrigiImmersionPack),
			  MountName = "dlc_liir_zuul"
			}
					};
					break;
				case "morrigi":
					this._dlcID = new int?(202230);
					this.Subfactions = new Subfaction[2]
					{
			new Subfaction(),
			new Subfaction()
			{
			  DlcID = new SteamDLCIdentifiers?(SteamDLCIdentifiers.LiirAndMorrigiImmersionPack),
			  MountName = "dlc_morrigi"
			}
					};
					break;
				case "zuul":
					this._dlcID = new int?(203050);
					this.Subfactions = new Subfaction[2]
					{
			new Subfaction(),
			new Subfaction()
			{
			  DlcID = new SteamDLCIdentifiers?(SteamDLCIdentifiers.TheHordeImmersionPack),
			  MountName = "dlc_zuul"
			}
					};
					break;
				case "loa":
					this.Subfactions = new Subfaction[1]
					{
			new Subfaction()
			{
			  DlcID = new SteamDLCIdentifiers?(),
			  MountName = "eof"
			}
					};
					break;
				default:
					this.Subfactions = new Subfaction[1]
					{
			new Subfaction()
					};
					break;
			}
			this.AssetPath = Path.Combine("factions", this.Name);
			this.WeaponModelPath = Path.Combine(this.AssetPath, "models", "weapons");
			XmlElement source1 = Faction.LoadMergedXMLDocument(filename)[nameof(Faction)];
			XmlElement xmlElement1 = source1["DiplomacyActionWeights"];
			if (xmlElement1 != null)
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(ScriptHost.FileSystem, xmlElement1.InnerText);
				this.DiplomacyWeights = new DiplomacyActionWeights(xmlDocument);
			}
			else
				this.DiplomacyWeights = new DiplomacyActionWeights();
			this.IndyDescrition = (IndyDesc)null;
			if (this.IsIndependent())
			{
				XmlElement xmlElement2 = source1["IndyDescriptions"];
				if (xmlElement2 != null)
				{
					this.IndyDescrition = new IndyDesc();
					this.IndyDescrition.CoreSpecialAttributes = new List<SpecialAttribute>();
					this.IndyDescrition.RandomSpecialAttributes = new List<SpecialAttribute>();
					this.IndyDescrition.TechLevel = xmlElement2["TechLevel"] != null ? int.Parse(xmlElement2["TechLevel"].InnerText) : 1;
					this.IndyDescrition.MinPlanetSize = xmlElement2["MinPlanetSize"] != null ? int.Parse(xmlElement2["MinPlanetSize"].InnerText) : 0;
					this.IndyDescrition.MaxPlanetSize = xmlElement2["MaxPlanetSize"] != null ? int.Parse(xmlElement2["MaxPlanetSize"].InnerText) : 0;
					this.IndyDescrition.StellarBodyType = xmlElement2["StellarBodyType"] != null ? xmlElement2["StellarBodyType"].InnerText : string.Empty;
					this.IndyDescrition.BaseFactionSuitability = xmlElement2["Hazard"] != null ? xmlElement2["Hazard"].GetAttribute("faction").ToLower() : string.Empty;
					this.IndyDescrition.Suitability = xmlElement2["Hazard"] != null ? (float)int.Parse(xmlElement2["Hazard"].GetAttribute("deviation")) : 0.0f;
					this.IndyDescrition.BasePopulationMod = xmlElement2["BasePopulationMod"] != null ? float.Parse(xmlElement2["BasePopulationMod"].InnerText) : 1f;
					this.IndyDescrition.BiosphereMod = xmlElement2["BiosphereMod"] != null ? float.Parse(xmlElement2["BiosphereMod"].InnerText) : 0.0f;
					this.IndyDescrition.TradeFTL = xmlElement2["TradeFTL"] != null ? float.Parse(xmlElement2["TradeFTL"].InnerText) : 0.0f;
				}
			}
			this.ID = int.Parse(source1.GetAttribute(nameof(ID)));
			this.MaterialDictionaries = source1.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name.Equals("MaterialDictionary", StringComparison.InvariantCulture))).Select<XmlElement, string>((Func<XmlElement, string>)(x => x.InnerText)).ToArray<string>();
			this.BadgeTexturePaths = source1.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name.Equals("Badge", StringComparison.InvariantCulture))).Select<XmlElement, string>((Func<XmlElement, string>)(x => x.GetAttribute("texture").ToLowerInvariant())).ToArray<string>();
			this.AvatarTexturePaths = source1.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name.Equals("Avatar", StringComparison.InvariantCulture))).Select<XmlElement, string>((Func<XmlElement, string>)(x => x.GetAttribute("texture").ToLowerInvariant())).ToArray<string>();
			List<string> stringList1 = new List<string>();
			List<string> stringList2 = new List<string>();
			foreach (XmlElement element in source1.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "TechTree")))
			{
				stringList1.AddRange((IEnumerable<string>)AssetDatabase.LoadTechTreeModels(element));
				stringList2.AddRange((IEnumerable<string>)AssetDatabase.LoadTechTreeRoots(element));
			}
			this.TechTreeModels = (IEnumerable<string>)stringList1;
			this.TechTreeRoots = (IEnumerable<string>)stringList2;
			Random random = new Random();
			this.DesignNames = new LocalizedNameGrabBag(source1[nameof(DesignNames)], random);
			this.EmpireNames = new LocalizedNameGrabBag(source1[nameof(EmpireNames)], random);
			this.UsesNPCCombatAI = bool.Parse(source1["UseNPCCombatAI"].InnerText);
			this.EntryPointOffset = source1[nameof(EntryPointOffset)] != null ? float.Parse(source1[nameof(EntryPointOffset)].InnerText) : 0.0f;
			this.StarTearTechEnteryPointOffset = source1[nameof(StarTearTechEnteryPointOffset)] != null ? float.Parse(source1[nameof(StarTearTechEnteryPointOffset)].InnerText) : 0.0f;
			this.PsionicPowerPerCrew = source1["PsiPowerPerCrew"] != null ? float.Parse(source1["PsiPowerPerCrew"].InnerText) : 0.0f;
			this.PsionicPowerModifier = source1["PsiPowerModifier"] != null ? float.Parse(source1["PsiPowerModifier"].InnerText) : 1f;
			this.CrewEfficiencyValue = source1[nameof(CrewEfficiencyValue)] != null ? float.Parse(source1[nameof(CrewEfficiencyValue)].InnerText) : 1f;
			this.ResearchBoostFailureMod = source1["ResearchBoostAccidentMod"] != null ? float.Parse(source1["ResearchBoostAccidentMod"].InnerText) : 1f;
			if (source1[nameof(NoAvatar)] != null)
				this.NoAvatar = source1[nameof(NoAvatar)].GetAttribute("texture");
			List<FactionDecalInfo> source2 = new List<FactionDecalInfo>();
			List<FactionDecalInfo> source3 = new List<FactionDecalInfo>();
			XmlElement source4 = source1["FactionDamageDecals"];
			if (source4 != null)
			{
				IEnumerable<XmlElement> source5 = source4.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name.Equals("Structure", StringComparison.InvariantCulture)));
				foreach (XmlElement xmlElement2 in source5)
				{
					FactionDecalInfo fdi = new FactionDecalInfo();
					fdi.DecalShipClass = (ShipClass)Enum.Parse(typeof(ShipClass), xmlElement2.GetAttribute("class"));
					if (!source2.Any<FactionDecalInfo>((Func<FactionDecalInfo, bool>)(x => x.DecalShipClass == fdi.DecalShipClass)))
					{
						List<DecalStageInfo> decalStageInfoList = new List<DecalStageInfo>();
						foreach (XmlElement xmlElement3 in source5.Where<XmlElement>((Func<XmlElement, bool>)(x => (ShipClass)Enum.Parse(typeof(ShipClass), x.GetAttribute("class")) == fdi.DecalShipClass)))
						{
							DecalStageInfo decalStageInfo;
							decalStageInfo.DecalStage = int.Parse(xmlElement3.GetAttribute("stage"));
							decalStageInfo.DecalSize = float.Parse(xmlElement3.GetAttribute("size"));
							decalStageInfo.DecalMaterial = xmlElement3.GetAttribute("material");
							decalStageInfoList.Add(decalStageInfo);
						}
						fdi.DecalStages = decalStageInfoList.ToArray();
						source2.Add(fdi);
					}
				}
				foreach (XmlElement xmlElement2 in source4.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name.Equals("Scorch", StringComparison.InvariantCulture))))
				{
					FactionDecalInfo fdi = new FactionDecalInfo();
					fdi.DecalShipClass = (ShipClass)Enum.Parse(typeof(ShipClass), xmlElement2.GetAttribute("class"));
					if (!source3.Any<FactionDecalInfo>((Func<FactionDecalInfo, bool>)(x => x.DecalShipClass == fdi.DecalShipClass)))
					{
						List<DecalStageInfo> decalStageInfoList = new List<DecalStageInfo>();
						foreach (XmlElement xmlElement3 in source5.Where<XmlElement>((Func<XmlElement, bool>)(x => (ShipClass)Enum.Parse(typeof(ShipClass), x.GetAttribute("class")) == fdi.DecalShipClass)))
						{
							DecalStageInfo decalStageInfo;
							decalStageInfo.DecalStage = int.Parse(xmlElement3.GetAttribute("stage"));
							decalStageInfo.DecalSize = float.Parse(xmlElement3.GetAttribute("size"));
							decalStageInfo.DecalMaterial = xmlElement3.GetAttribute("material");
							decalStageInfoList.Add(decalStageInfo);
						}
						fdi.DecalStages = decalStageInfoList.ToArray();
						source3.Add(fdi);
					}
				}
			}
			this.StructDecalInfo = source2.ToArray();
			this.ScorchDecalInfo = source3.ToArray();
			XmlElement xmlElement4 = source1["BoardingActionModifiers"];
			if (xmlElement4 != null)
			{
				this.BoardingActionMods.FreshAgentStrength = xmlElement4["FreshAgentStrength"] != null ? float.Parse(xmlElement4["FreshAgentStrength"].InnerText) : 1f;
				this.BoardingActionMods.TiredAgentStrength = xmlElement4["TiredAgentStrength"] != null ? float.Parse(xmlElement4["TiredAgentStrength"].InnerText) : 0.5f;
				this.BoardingActionMods.ExhaustedAgentStrength = xmlElement4["ExhaustedAgentStrength"] != null ? float.Parse(xmlElement4["ExhaustedAgentStrength"].InnerText) : 0.25f;
				this.BoardingActionMods.LocationStrength.Default = xmlElement4["AgentLocationStrength"] != null ? float.Parse(xmlElement4["AgentLocationStrength"].GetAttribute("default")) : 1f;
				this.BoardingActionMods.LocationStrength.Cruiser = xmlElement4["AgentLocationStrength"] != null ? float.Parse(xmlElement4["AgentLocationStrength"].GetAttribute("cruiser")) : 1f;
				this.BoardingActionMods.LocationStrength.Dreadnought = xmlElement4["AgentLocationStrength"] != null ? float.Parse(xmlElement4["AgentLocationStrength"].GetAttribute("dreadnought")) : 1f;
				this.BoardingActionMods.LocationStrength.Leviathan = xmlElement4["AgentLocationStrength"] != null ? float.Parse(xmlElement4["AgentLocationStrength"].GetAttribute("leviathan")) : 1f;
				this.BoardingActionMods.EfficiencyVSBoarding.Default = xmlElement4["EfficiencyVSBoarding"] != null ? float.Parse(xmlElement4["EfficiencyVSBoarding"].GetAttribute("default")) : 0.5f;
				this.BoardingActionMods.EfficiencyVSBoarding.Cruiser = xmlElement4["EfficiencyVSBoarding"] != null ? float.Parse(xmlElement4["EfficiencyVSBoarding"].GetAttribute("cruiser")) : 0.5f;
				this.BoardingActionMods.EfficiencyVSBoarding.Dreadnought = xmlElement4["EfficiencyVSBoarding"] != null ? float.Parse(xmlElement4["EfficiencyVSBoarding"].GetAttribute("dreadnought")) : 0.5f;
				this.BoardingActionMods.EfficiencyVSBoarding.Leviathan = xmlElement4["EfficiencyVSBoarding"] != null ? float.Parse(xmlElement4["EfficiencyVSBoarding"].GetAttribute("leviathan")) : 0.5f;
			}
			else
			{
				this.BoardingActionMods.FreshAgentStrength = 1f;
				this.BoardingActionMods.TiredAgentStrength = 0.5f;
				this.BoardingActionMods.ExhaustedAgentStrength = 0.25f;
				this.BoardingActionMods.LocationStrength.Default = 1f;
				this.BoardingActionMods.LocationStrength.Cruiser = 1f;
				this.BoardingActionMods.LocationStrength.Dreadnought = 1f;
				this.BoardingActionMods.LocationStrength.Leviathan = 1f;
				this.BoardingActionMods.EfficiencyVSBoarding.Default = 0.5f;
				this.BoardingActionMods.EfficiencyVSBoarding.Cruiser = 0.5f;
				this.BoardingActionMods.EfficiencyVSBoarding.Dreadnought = 0.5f;
				this.BoardingActionMods.EfficiencyVSBoarding.Leviathan = 0.5f;
			}
			XmlElement source6 = source1["DefaultDiplomacyReactions"];
			if (source6 != null)
			{
				foreach (XmlElement xmlElement2 in source6.OfType<XmlElement>())
				{
					string attribute = xmlElement2.GetAttribute("faction");
					if (!this._defaultReactionValue.ContainsKey(attribute))
						this._defaultReactionValue.Add(attribute, Math.Min(Math.Max(int.Parse(xmlElement2.GetAttribute("value")), DiplomacyInfo.MinDeplomacyRelations), DiplomacyInfo.MaxDeplomacyRelations));
				}
			}
			XmlElement source7 = source1["ImmigrationPopBonus"];
			if (source7 != null)
			{
				foreach (XmlElement xmlElement2 in source7.OfType<XmlElement>())
				{
					string attribute = xmlElement2.GetAttribute("faction");
					if (!this._ImmigrationPopBonusValue.ContainsKey(attribute))
						this._ImmigrationPopBonusValue.Add(attribute, float.Parse(xmlElement2.GetAttribute("value")));
				}
			}
			XmlElement source8 = source1["SpyingBonus"];
			if (source8 != null)
			{
				foreach (XmlElement xmlElement2 in source8.OfType<XmlElement>())
				{
					string attribute = xmlElement2.GetAttribute("faction");
					if (!this._SpyingBonusValue.ContainsKey(attribute))
						this._SpyingBonusValue.Add(attribute, float.Parse(xmlElement2.GetAttribute("value")));
				}
			}
			this.StratModifiers = source1[nameof(StratModifiers)];
			XmlElement xmlElement5 = source1["SalvageModifiers"];
			if (xmlElement5 != null)
			{
				this.RepSel = int.Parse(xmlElement5["RepSal"].InnerText);
				this.DefaultRepSel = int.Parse(xmlElement5["default"].InnerText);
			}
			XmlElement xmlElement6 = source1["MoralEventModifiers"];
			if (xmlElement6 != null)
			{
				foreach (GovernmentInfo.GovernmentType key1 in Enum.GetValues(typeof(GovernmentInfo.GovernmentType)))
				{
					XmlElement xmlElement2 = xmlElement6[key1.ToString()];
					if (xmlElement2 != null)
					{
						foreach (MoralEvent key2 in Enum.GetValues(typeof(MoralEvent)))
						{
							XmlElement xmlElement3 = xmlElement2[key2.ToString()];
							if (xmlElement3 != null)
							{
								if (!this._factionSpecificMoral.ContainsKey(key1))
									this._factionSpecificMoral.Add(key1, new Dictionary<MoralEvent, int>());
								this._factionSpecificMoral[key1].Add(key2, int.Parse(xmlElement3.InnerText));
							}
						}
					}
				}
			}
			XmlElement xmlElement7 = source1["ResearchRates"];
			if (xmlElement7 != null)
			{
				foreach (AIStance aiStance in Enum.GetValues(typeof(AIStance)))
				{
					XmlElement xmlElement2 = xmlElement7[aiStance.ToString()];
					if (xmlElement2 != null)
					{
						float val1 = float.Parse(xmlElement2.InnerText);
						this.AIFastResearchRate[(int)aiStance] = Math.Max(Math.Min(val1, 1f), 0.0f);
					}
				}
			}
			XmlElement source9 = source1[nameof(InitialDesigns)];
			if (source9 != null)
			{
				string weaponBiasTechFamilyID = source9.GetAttribute("weaponbias");
				this.InitialDesigns = source9.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "Design")).Select<XmlElement, InitialDesign>((Func<XmlElement, InitialDesign>)(y => new InitialDesign()
				{
					Name = y.GetAttribute("name"),
					WeaponBiasTechFamilyID = weaponBiasTechFamilyID,
					Sections = y.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(z => z.Name == "Section")).Select<XmlElement, string>((Func<XmlElement, string>)(w => w.GetAttribute("name"))).ToArray<string>()
				})).ToArray<InitialDesign>();
			}
			else
				this.InitialDesigns = (InitialDesign[])null;
		}

		public override string ToString()
		{
			return this.Name + ", " + this.FactionFileName;
		}

		public object GetStratModifier(string name)
		{
			if (this.StratModifiers == null)
				return (object)null;
			XmlElement stratModifier = this.StratModifiers[name];
			if (stratModifier == null)
				return (object)null;
			return (object)stratModifier.InnerText;
		}

		public static XmlDocument LoadMergedXMLDocument(string filename)
		{
			string[] files = ScriptHost.FileSystem.FindFiles(filename);
			XmlDocument document1 = new XmlDocument();
			document1.Load(ScriptHost.FileSystem, files[0]);
			for (int index = 1; index < files.Length; ++index)
			{
				XmlDocument document2 = new XmlDocument();
				document2.Load(ScriptHost.FileSystem, files[index]);
				foreach (XmlNode node in document2.DocumentElement.ChildNodes.OfType<XmlElement>())
					document1.DocumentElement.AppendChild(document1.ImportNode(node, true));
			}
			return document1;
		}

		public static Faction LoadXml(string filename)
		{
			return new Faction(filename);
		}

		public string GetWeaponModelPath(string filenameWithoutDirectory)
		{
			return Path.Combine(this.WeaponModelPath, filenameWithoutDirectory);
		}

		public bool CanSupportPopulation(PopulationType type)
		{
			return (double)this.MaxPopulationMod[(int)type] > 0.0;
		}

		public float GetAIResearchRate(AIStance stance)
		{
			return this.AIFastResearchRate[(int)stance];
		}

		public bool IsPlagueImmune(WeaponEnums.PlagueType pt)
		{
			return this.Name == "zuul" && pt != WeaponEnums.PlagueType.XOMBIE || this.Name == "loa" && pt != WeaponEnums.PlagueType.NANO;
		}

		public string SplinterAvatarPath()
		{
			return "Independent_Avatar_" + (object)char.ToUpper(this.Name[0]) + this.Name.Substring(1);
		}

		public bool IsIndependent()
		{
			if (!(this.Name == "enki") && !(this.Name == "kaeru") && (!(this.Name == "mindi") && !(this.Name == "nandi")) && (!(this.Name == "tatzel") && !(this.Name == "utukku") && !(this.Name == "deeroz")))
				return this.Name == "m'kkkose";
			return true;
		}

		public bool IsFactionIndependentTrader()
		{
			return this.Name == "zuul";
		}

		public bool HasSlaves()
		{
			return this.Name == "zuul";
		}

		public bool CanUseGate()
		{
			return this.Name == "hiver";
		}

		public bool CanUseAccelerators()
		{
			return this.Name == "loa";
		}

		public bool CanUseNodeLine(bool? permanent = null)
		{
			if (!permanent.HasValue)
			{
				if (!(this.Name == "human") && !(this.Name == "zuul"))
					return this.Name == "loa";
				return true;
			}
			if (permanent.Value)
				return this.Name == "human";
			if (!(this.Name == "zuul"))
				return this.Name == "loa";
			return true;
		}

		public bool CanUseGravityWell()
		{
			return this.Name == "liir_zuul";
		}

		public bool CanUseFlockBonus()
		{
			return this.Name == "morrigi";
		}

		public float ChooseIdealSuitability(Random random)
		{
			int num = 500;
			return random.NextInclusive(Constants.MinSuitability + (float)num, Constants.MaxSuitability - (float)num);
		}

		public void AddFactionReference(App game)
		{
			++this._factionObjectCount;
			if (this._factionObjectCount != 1)
				return;
			List<object> objectList = new List<object>();
			objectList.Add((object)this.Name);
			objectList.Add((object)this.StructDecalInfo.Length);
			foreach (FactionDecalInfo factionDecalInfo in this.StructDecalInfo)
			{
				objectList.Add((object)factionDecalInfo.DecalShipClass);
				objectList.Add((object)factionDecalInfo.DecalStages.Length);
				foreach (DecalStageInfo decalStage in factionDecalInfo.DecalStages)
				{
					objectList.Add((object)decalStage.DecalStage);
					objectList.Add((object)decalStage.DecalSize);
					objectList.Add((object)decalStage.DecalMaterial);
				}
			}
			objectList.Add((object)this.ScorchDecalInfo.Length);
			foreach (FactionDecalInfo factionDecalInfo in this.ScorchDecalInfo)
			{
				objectList.Add((object)factionDecalInfo.DecalShipClass);
				objectList.Add((object)factionDecalInfo.DecalStages.Length);
				foreach (DecalStageInfo decalStage in factionDecalInfo.DecalStages)
				{
					objectList.Add((object)decalStage.DecalStage);
					objectList.Add((object)decalStage.DecalSize);
					objectList.Add((object)decalStage.DecalMaterial);
				}
			}
			objectList.Add((object)this.PsionicPowerPerCrew);
			objectList.Add((object)this.CrewEfficiencyValue);
			objectList.Add((object)this.BoardingActionMods.FreshAgentStrength);
			objectList.Add((object)this.BoardingActionMods.TiredAgentStrength);
			objectList.Add((object)this.BoardingActionMods.ExhaustedAgentStrength);
			objectList.Add((object)this.BoardingActionMods.LocationStrength.Default);
			objectList.Add((object)this.BoardingActionMods.LocationStrength.Cruiser);
			objectList.Add((object)this.BoardingActionMods.LocationStrength.Dreadnought);
			objectList.Add((object)this.BoardingActionMods.LocationStrength.Leviathan);
			objectList.Add((object)this.BoardingActionMods.EfficiencyVSBoarding.Default);
			objectList.Add((object)this.BoardingActionMods.EfficiencyVSBoarding.Cruiser);
			objectList.Add((object)this.BoardingActionMods.EfficiencyVSBoarding.Dreadnought);
			objectList.Add((object)this.BoardingActionMods.EfficiencyVSBoarding.Leviathan);
			this._factionObject = game.AddObject<FactionObject>(objectList.ToArray());
		}

		public void ReleaseFactionReference(App game)
		{
			if (this._factionObject == null)
				return;
			if (this._factionObjectCount == 0)
				throw new InvalidOperationException("Weapon reference count already 0.");
			--this._factionObjectCount;
			if (this._factionObjectCount != 0)
				return;
			game.ReleaseObject((IGameObject)this._factionObject);
			this._factionObject = (FactionObject)null;
		}
	}
}
