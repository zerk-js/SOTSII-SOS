// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.PlayerInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using System.Collections.Generic;

namespace Kerberos.Sots.Data
{
	internal class PlayerInfo : IIDProvider
	{
		public bool isStandardPlayer = true;
		public Dictionary<int, int> FactionDiplomacyPoints = new Dictionary<int, int>();
		public string Name;
		public int FactionID;
		public int SubfactionIndex;
		public Vector3 PrimaryColor;
		public Vector3 SecondaryColor;
		public string BadgeAssetPath;
		public string AvatarAssetPath;
		public int? Homeworld;
		public double Savings;
		public int LastCombatTurn;
		public int LastEncounterTurn;
		public AIDifficulty AIDifficulty;
		public double ResearchBoostFunds;
		public bool AutoPlaceDefenseAssets;
		public bool AutoRepairShips;
		public bool AutoUseGoopModules;
		public bool AutoUseJokerModules;
		public bool AutoAoe;
		public int Team;
		public bool AutoPatrol;
		public float RateGovernmentResearch;
		public float RateResearchCurrentProject;
		public float RateResearchSpecialProject;
		public float RateResearchSalvageResearch;
		public float RateGovernmentStimulus;
		public float RateGovernmentSecurity;
		public float RateGovernmentSavings;
		public float RateStimulusMining;
		public float RateStimulusColonization;
		public float RateStimulusTrade;
		public float RateSecurityOperations;
		public float RateSecurityIntelligence;
		public float RateSecurityCounterIntelligence;
		public int GenericDiplomacyPoints;
		public float RateTax;
		public float RateTaxPrev;
		public float RateImmigration;
		public int IntelPoints;
		public int CounterIntelPoints;
		public int OperationsPoints;
		public int IntelAccumulator;
		public int CounterIntelAccumulator;
		public int OperationsAccumulator;
		public int CivilianMiningAccumulator;
		public int CivilianColonizationAccumulator;
		public int CivilianTradeAccumulator;
		public int AdditionalResearchPoints;
		public int PsionicPotential;
		public bool isDefeated;
		public double CurrentTradeIncome;
		public bool includeInDiplomacy;
		public bool isAIRebellionPlayer;

		public int ID { get; set; }

		private static void Normalize3(ref float a, ref float b, ref float c)
		{
			float num = a + b + c;
			if ((double)num < 1.40129846432482E-45)
			{
				a = 0.0f;
				b = 0.0f;
				c = 0.0f;
			}
			else
			{
				a /= num;
				b /= num;
				c /= num;
			}
		}

		public void NormalizeRates()
		{
			PlayerInfo.Normalize3(ref this.RateGovernmentStimulus, ref this.RateGovernmentSecurity, ref this.RateGovernmentSavings);
			PlayerInfo.Normalize3(ref this.RateResearchCurrentProject, ref this.RateResearchSpecialProject, ref this.RateResearchSalvageResearch);
			PlayerInfo.Normalize3(ref this.RateStimulusMining, ref this.RateStimulusColonization, ref this.RateStimulusTrade);
			PlayerInfo.Normalize3(ref this.RateSecurityOperations, ref this.RateSecurityIntelligence, ref this.RateSecurityCounterIntelligence);
		}

		public int GetTotalDiplomacyPoints(int factionID)
		{
			return this.GenericDiplomacyPoints / 2 + (this.FactionDiplomacyPoints.ContainsKey(factionID) ? this.FactionDiplomacyPoints[factionID] : 0);
		}

		public override string ToString()
		{
			return string.Format("{0},{1}", (object)this.ID, (object)this.Name);
		}

		public void SetResearchRate(float value)
		{
			this.RateGovernmentResearch = 1f - value;
		}

		public float GetResearchRate()
		{
			return 1f - this.RateGovernmentResearch;
		}

		public bool IsOnTeam(PlayerInfo otherplayer)
		{
			if (this.Team != 0)
				return this.Team == otherplayer.Team;
			return false;
		}

		public bool CanDebtSpend(AssetDatabase assetdb)
		{
			if (!(assetdb.GetFaction(this.FactionID).Name == "loa"))
				return true;
			return this.Savings > 0.0;
		}
	}
}
