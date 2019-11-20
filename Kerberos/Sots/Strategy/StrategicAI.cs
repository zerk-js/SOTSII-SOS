// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.StrategicAI
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Combat;
using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kerberos.Sots.Strategy
{
	internal class StrategicAI
	{
		private readonly int MAX_BUILD_TURNS = 5;
		private const float FastResearchRate = 0.7f;
		private const float SlowResearchRate = 0.3f;
		private const double AI_RELATION0 = 0.0;
		private const double AI_RELATION1 = 2000.0;
		private const double AI_RELATION_WAR0 = 0.0;
		private const double AI_RELATION_WAR = 300.0;
		private const double AI_RELATION_WAR1 = 750.0;
		private const double AI_RELATION_NEUTRAL0 = 750.0;
		private const double AI_RELATION_NEUTRAL = 925.0;
		private const double AI_RELATION_NEUTRAL1 = 1100.0;
		private const double AI_RELATION_CEASEFIRE0 = 1100.0;
		private const double AI_RELATION_CEASEFIRE = 1250.0;
		private const double AI_RELATION_CEASEFIRE1 = 1400.0;
		private const double AI_RELATION_NAP0 = 1400.0;
		private const double AI_RELATION_NAP = 1600.0;
		private const double AI_RELATION_NAP1 = 1800.0;
		private const double AI_RELATION_ALLY0 = 1800.0;
		private const double AI_RELATION_ALLY = 1900.0;
		private const double AI_RELATION_ALLY1 = 2000.0;
		private GameSession _game;
		private Player _player;
		private GameDatabase _db;
		private Random _random;
		private List<FleetInfo> m_AvailableFullFleets;
		private List<FleetInfo> m_AvailableShortFleets;
		private List<FleetInfo> m_DefenseCombatFleets;
		private List<FleetInfo> m_FleetsInSurveyRange;
		private Dictionary<FleetInfo, FleetRangeData> m_FleetRanges;
		private FleetTemplate m_CombatFleetTemplate;
		private List<StrategicTask> m_AvailableTasks;
		private Dictionary<StrategicTask, List<StrategicTask>> m_RelocationTasks;
		private Dictionary<int, int> m_FleetCubePoints;
		private List<SystemDefendInfo> m_ColonizedSystems;
		private Budget m_TurnBudget;
		private int[] m_NumStations;
		private int m_GateCapacity;
		private int m_LoaLimit;
		private bool m_IsOldSave;
		private readonly AITechStyles _techStyles;
		private readonly MissionManager _missionMan;
		private int _dropInActivationTurn;

		public GameSession Game
		{
			get
			{
				return this._game;
			}
		}

		public Player Player
		{
			get
			{
				return this._player;
			}
		}

		public Random Random
		{
			get
			{
				return this._random;
			}
		}

		public AITechStyles TechStyles
		{
			get
			{
				return this._techStyles;
			}
		}

		public AIStance? LastStance { get; private set; }

		public StrategicAI(GameSession game, Player player)
		{
			this._missionMan = new MissionManager(this);
			this._game = game;
			this._player = player;
			this._db = game.GameDatabase;
			this._random = new Random();
			this.m_IsOldSave = this._db.GetUnixTimeCreated() == 0.0;
			this.m_AvailableFullFleets = new List<FleetInfo>();
			this.m_AvailableShortFleets = new List<FleetInfo>();
			this.m_DefenseCombatFleets = new List<FleetInfo>();
			this.m_FleetsInSurveyRange = new List<FleetInfo>();
			this.m_FleetRanges = new Dictionary<FleetInfo, FleetRangeData>();
			this.m_AvailableTasks = new List<StrategicTask>();
			this.m_RelocationTasks = new Dictionary<StrategicTask, List<StrategicTask>>();
			this.m_ColonizedSystems = new List<SystemDefendInfo>();
			this.m_FleetCubePoints = new Dictionary<int, int>();
			this.m_NumStations = new int[8];
			this.m_GateCapacity = 0;
			this.m_LoaLimit = 0;
			this.m_CombatFleetTemplate = game.AssetDatabase.FleetTemplates.FirstOrDefault<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == "DEFAULT_PATROL"));
			if (player.ID == 0)
				return;
			this._db.InsertOrIgnoreAI(player.ID, AIStance.EXPANDING);
			AIInfo aiInfo = this._db.GetAIInfo(player.ID);
			if ((aiInfo.Flags & AIInfoFlags.TechStylesGenerated) == (AIInfoFlags)0)
			{
				this._techStyles = new AITechStyles(this._game.AssetDatabase, (IEnumerable<AITechStyleInfo>)game.AssetDatabase.AIResearchFramework.AISelectTechStyles(this, player.Faction));
				foreach (AITechStyleInfo techStyleInfo in this._techStyles.TechStyleInfos)
					this._db.InsertAITechStyle(techStyleInfo);
				aiInfo.Flags |= AIInfoFlags.TechStylesGenerated;
				this._db.UpdateAIInfo(aiInfo);
			}
			else
				this._techStyles = new AITechStyles(this._game.AssetDatabase, (IEnumerable<AITechStyleInfo>)this._db.GetAITechStyles(player.ID).ToList<AITechStyleInfo>());
		}

		public static void InitializeResearch(
		  Random rand,
		  AssetDatabase assetdb,
		  GameDatabase db,
		  int playerID)
		{
			foreach (TechFamily techFamily in assetdb.MasterTechTree.TechFamilies)
			{
				float weight = 0.4f + rand.NextSingle() / 5f;
				db.InsertAITechWeight(playerID, techFamily.Id, weight);
			}
		}

		public void SetDropInActivationTurn(int turn)
		{
			this._dropInActivationTurn = turn;
		}

		public int CachedAvailableResearchPointsPerTurn { get; private set; }

		public void Update(StrategicAI.UpdateInfo updateInfo)
		{
			AIInfo aiInfo = this._db.GetAIInfo(this._player.ID);
			StrategicAI.TraceVerbose(string.Format("---- Processing AI for player {0} ----", (object)aiInfo.PlayerInfo));
			if (this._db.GetTurnCount() < this._dropInActivationTurn)
				StrategicAI.TraceVerbose(string.Format("AI processing skipped; drop-in moratorium is in place until turn {0}.", (object)this._dropInActivationTurn));
			else if (aiInfo.PlayerInfo.isDefeated || !aiInfo.PlayerInfo.isStandardPlayer && !aiInfo.PlayerInfo.isAIRebellionPlayer)
			{
				StrategicAI.TraceVerbose("AI processing skipped; player is exempt.");
			}
			else
			{
				this.LastStance = new AIStance?(aiInfo.Stance);
				if (this.m_IsOldSave)
					this._missionMan.Update();
				this.ResetData();
				double startOfTurnSavings;
				this.UpdateEmpire(aiInfo, out startOfTurnSavings);
				this.UpdateStance(aiInfo, updateInfo);
				this.ManageColonies(aiInfo);
				this.DesignShips(aiInfo);
				this.DoRepairs(aiInfo);
				this.ManageStations(aiInfo);
				if (this.m_IsOldSave)
				{
					this.ManageFleets(aiInfo);
					this.ManageDefenses(aiInfo);
					this.ManageReserves(aiInfo);
					this.SetFleetOrders(aiInfo);
				}
				else
					this.ManageDefenses(aiInfo);
				this.SetResearchOrders(aiInfo);
				this.UpdateRelations(aiInfo, updateInfo);
				this.OfferTreaties(aiInfo, updateInfo);
				if (!this.m_IsOldSave)
				{
					this.GatherAllResources(aiInfo.Stance);
					this.GatherAllTasks();
					this.ApplyScores(aiInfo.Stance);
					this.AssignFleetsToTasks(aiInfo.Stance);
					this.BuildFleets(aiInfo);
					this.ManageDebtLevels();
				}
				if (this._player.Faction.Name == "loa")
				{
					List<LoaFleetComposition> list = this._db.GetLoaFleetCompositions().Where<LoaFleetComposition>((Func<LoaFleetComposition, bool>)(x => x.PlayerID == this._player.ID)).ToList<LoaFleetComposition>();
					Dictionary<string, int> dictionary = new Dictionary<string, int>();
					foreach (LoaFleetComposition fleetComposition in list)
					{
						if (dictionary.ContainsKey(fleetComposition.Name) && dictionary[fleetComposition.Name] < fleetComposition.ID)
						{
							this._db.DeleteLoaFleetCompositon(dictionary[fleetComposition.Name]);
							dictionary[fleetComposition.Name] = fleetComposition.ID;
						}
						else
							dictionary[fleetComposition.Name] = fleetComposition.ID;
					}
				}
				if (App.Log.Level < Kerberos.Sots.Engine.LogLevel.Verbose)
					return;
				StringBuilder result = new StringBuilder();
				DesignLab.PrintPlayerDesignSummary(result, this.Game.App, this._player.ID, false);
				StrategicAI.TraceVerbose(result.ToString());
			}
		}

		private void DoRepairs(AIInfo aiInfo)
		{
			foreach (StellarInfo starSystemInfo in this._db.GetStarSystemInfos())
				this._game.RepairFleetsAtSystem(starSystemInfo.ID, this._player.ID);
		}

		private void UpdateEmpire(AIInfo aiInfo, out double startOfTurnSavings)
		{
			startOfTurnSavings = 0d;
			this.FinalizeEmpire(aiInfo, ref startOfTurnSavings);
			PlayerInfo playerInfo1 = this._db.GetPlayerInfo(this._player.ID);
			startOfTurnSavings = playerInfo1.Savings;
			if (this.m_IsOldSave && playerInfo1.Savings < 100000.0)
				this._db.UpdatePlayerSavings(this._player.ID, 100000.0);
			this.CachedAvailableResearchPointsPerTurn = this.Game.GetAvailableResearchPoints(playerInfo1);
			if (this._db.GetSliderNotchSettingInfo(this._player.ID, UISlidertype.SecuritySlider) == null)
				this._db.InsertUISliderNotchSetting(this._player.ID, UISlidertype.SecuritySlider, 0.0, 0);
			PlayerInfo playerInfo2 = this._db.GetPlayerInfo(this._player.ID);
			Budget budget = Budget.GenerateBudget(this._game, this._db.GetPlayerInfo(this._player.ID), (IEnumerable<DesignInfo>)null, BudgetProjection.Actual);
			playerInfo2.RateGovernmentSecurity = (float)budget.RequiredSecurity / 100f;
			playerInfo2.RateGovernmentStimulus = this._db.GetStratModifier<bool>(StratModifiers.EnableTrade, this._player.ID) ? Math.Min((float)(60000.0 / budget.RequestedGovernmentSpending), 0.15f) : 0.0f;
			playerInfo2.RateGovernmentSavings = (float)(1.0 - ((double)playerInfo2.RateGovernmentSecurity + (double)playerInfo2.RateGovernmentStimulus));
			if (playerInfo2.Savings < 500000.0)
			{
				playerInfo2.RateGovernmentSecurity = (float)budget.RequiredSecurity / 100f;
				playerInfo2.RateGovernmentSavings = 1f - playerInfo2.RateGovernmentSecurity;
				playerInfo2.RateGovernmentStimulus = (float)(1.0 - ((double)playerInfo2.RateGovernmentSavings + (double)playerInfo2.RateGovernmentSecurity));
			}
			playerInfo2.RateStimulusColonization = 0.0f;
			playerInfo2.RateStimulusMining = this._db.PlayerHasTech(this._player.ID, "IND_Mega-Strip_Mining") ? 1f : 0.0f;
			playerInfo2.RateStimulusTrade = this._db.GetStratModifier<bool>(StratModifiers.EnableTrade, this._player.ID) ? 1f : 0.0f;
			playerInfo2.RateSecurityCounterIntelligence = 1f;
			playerInfo2.RateSecurityIntelligence = 1f;
			playerInfo2.RateSecurityOperations = 0.0f;
			playerInfo2.RateResearchCurrentProject = 1f;
			playerInfo2.RateResearchSalvageResearch = 0.1f;
			playerInfo2.RateResearchSpecialProject = 0.1f;
			playerInfo2.NormalizeRates();
			this._db.UpdatePlayerSliders(this._game, playerInfo2);
		}

		private void UpdateRelations(AIInfo aiInfo, StrategicAI.UpdateInfo updateInfo)
		{
			if (aiInfo.Stance != AIStance.DEFENDING)
				return;
			List<CombatData> list = this._game.CombatData.GetCombatsForPlayer(this._game.GameDatabase, this._player.ID, 1).ToList<CombatData>();
			foreach (StrategicAI.UpdatePlayerInfo updatePlayerInfo in updateInfo.Players.Values)
			{
				StrategicAI.UpdatePlayerInfo updatePlayer = updatePlayerInfo;
				DiplomacyInfo relation = updatePlayer.Relations[this._player.ID];
				if (relation.State == DiplomacyState.WAR && !list.Any<CombatData>((Func<CombatData, bool>)(x => x.GetPlayers().Any<PlayerCombatData>((Func<PlayerCombatData, bool>)(y => y.PlayerID == updatePlayer.Player.ID)))))
				{
					relation.Relations += 100;
					this._db.UpdateDiplomacyInfo(relation);
				}
			}
		}

		private void FinalizeEmpire(AIInfo aiInfo, ref double startOfTurnSavings)
		{
			PlayerInfo playerInfo = this._db.GetPlayerInfo(this._player.ID);
			bool flag = false;
			if (startOfTurnSavings > 0.0 && playerInfo.Savings > 0.0)
			{
				float num1 = 1f;
				if (playerInfo.Savings <= 1500000.0)
					num1 = 0.25f;
				float researchRate = playerInfo.GetResearchRate();
				float val2 = this._player.Faction.GetAIResearchRate(aiInfo.Stance) * num1;
				if (playerInfo.Savings > 10000000.0)
					val2 = 0.75f;
				if ((double)researchRate < (double)val2)
				{
					float num2 = Math.Min(researchRate + 0.2f, val2);
					playerInfo.SetResearchRate(num2);
					flag = true;
				}
				else if ((double)researchRate > (double)val2)
				{
					float num2 = Math.Max(researchRate - 0.2f, val2);
					playerInfo.SetResearchRate(num2);
					flag = true;
				}
			}
			else
			{
				float num = 0.3f;
				if (this._player.Faction.Name == "loa" && playerInfo.Savings < 0.0)
					num = 0.0f;
				if (this._player.Faction.Name == "hiver" && playerInfo.Savings < -100000.0 || playerInfo.Savings < -500000.0)
					num *= 0.5f;
				if (this._player.Faction.Name == "hiver" && playerInfo.Savings < -250000.0 || playerInfo.Savings < -1000000.0)
					num = 0.0f;
				if ((double)playerInfo.GetResearchRate() > (double)num)
				{
					playerInfo.SetResearchRate(num);
					flag = true;
				}
			}
			if (!flag)
				return;
			this._game.GameDatabase.UpdatePlayerSliders(this._game, playerInfo);
		}

		public static int GetDiplomacyStateRank(DiplomacyState value)
		{
			switch (value)
			{
				case DiplomacyState.CEASE_FIRE:
					return 5;
				case DiplomacyState.UNKNOWN:
					return 1;
				case DiplomacyState.NON_AGGRESSION:
					return 6;
				case DiplomacyState.WAR:
					return 2;
				case DiplomacyState.ALLIED:
					return 7;
				case DiplomacyState.NEUTRAL:
					return 3;
				case DiplomacyState.PEACE:
					return 4;
				default:
					return 1;
			}
		}

		private static double LerpRange(double t, double t0, double x0, double t1, double x1)
		{
			return ScalarExtensions.Lerp(x0, x1, (t - t0) / (t1 - t0));
		}

		private DiplomacyState EvolvePreferredDiplomacyState(
		  DiplomacyState current,
		  double relation)
		{
			DiplomacyState diplomacyState1 = current;
			DiplomacyState diplomacyState2 = current;
			double odds;
			if (relation >= 0.0 && relation < 300.0)
			{
				odds = 1.0;
				diplomacyState2 = DiplomacyState.WAR;
			}
			else if (relation >= 300.0 && relation < 750.0)
			{
				odds = StrategicAI.LerpRange(relation, 300.0, 1.0, 750.0, 0.0);
				diplomacyState2 = DiplomacyState.WAR;
			}
			else if (relation >= 750.0 && relation < 925.0)
			{
				odds = StrategicAI.LerpRange(relation, 750.0, 0.0, 925.0, 1.0);
				diplomacyState2 = DiplomacyState.NEUTRAL;
			}
			else if (relation >= 925.0 && relation < 1100.0)
			{
				odds = StrategicAI.LerpRange(relation, 925.0, 1.0, 1100.0, 0.0);
				diplomacyState2 = DiplomacyState.NEUTRAL;
			}
			else if (relation >= 1100.0 && relation < 1250.0)
			{
				if (diplomacyState1 == DiplomacyState.WAR)
					diplomacyState1 = DiplomacyState.NEUTRAL;
				odds = StrategicAI.LerpRange(relation, 1100.0, 0.0, 1250.0, 1.0);
				diplomacyState2 = DiplomacyState.CEASE_FIRE;
			}
			else if (relation >= 1250.0 && relation < 1400.0)
			{
				if (diplomacyState1 == DiplomacyState.WAR)
					diplomacyState1 = DiplomacyState.NEUTRAL;
				odds = StrategicAI.LerpRange(relation, 1250.0, 1.0, 1400.0, 0.0);
				diplomacyState2 = DiplomacyState.CEASE_FIRE;
			}
			else if (relation >= 1400.0 && relation < 1600.0)
			{
				if (diplomacyState1 == DiplomacyState.WAR || diplomacyState1 == DiplomacyState.NEUTRAL)
					diplomacyState1 = DiplomacyState.CEASE_FIRE;
				odds = StrategicAI.LerpRange(relation, 1400.0, 0.0, 1600.0, 1.0);
				diplomacyState2 = DiplomacyState.NON_AGGRESSION;
			}
			else if (relation >= 1600.0 && relation < 1800.0)
			{
				if (diplomacyState1 == DiplomacyState.WAR || diplomacyState1 == DiplomacyState.NEUTRAL)
					diplomacyState1 = DiplomacyState.CEASE_FIRE;
				odds = StrategicAI.LerpRange(relation, 1600.0, 1.0, 1800.0, 0.0);
				diplomacyState2 = DiplomacyState.NON_AGGRESSION;
			}
			else if (relation >= 1800.0 && relation <= 2000.0)
			{
				if (diplomacyState1 == DiplomacyState.WAR || diplomacyState1 == DiplomacyState.NEUTRAL || diplomacyState1 == DiplomacyState.CEASE_FIRE)
					diplomacyState1 = DiplomacyState.NON_AGGRESSION;
				odds = StrategicAI.LerpRange(relation, 1800.0, 0.0, 2000.0, 1.0);
				diplomacyState2 = DiplomacyState.ALLIED;
			}
			else
				odds = 0.0;
			if (this._random.CoinToss(odds))
				diplomacyState1 = diplomacyState2;
			return diplomacyState1;
		}

		private DiplomacyState GetPreferredDiplomacyState(
		  AIInfo aiInfo,
		  StrategicAI.UpdatePlayerInfo updatePlayerInfo,
		  DiplomacyInfo di)
		{
			return this.EvolvePreferredDiplomacyState(di.State, (double)di.Relations);
		}

		private void TryDiplomacyStateAction(
		  AIInfo aiInfo,
		  StrategicAI.UpdatePlayerInfo updatePlayerInfo,
		  DiplomacyInfo di)
		{
			if (!di.isEncountered || updatePlayerInfo.Player.isDefeated || (!updatePlayerInfo.Player.isStandardPlayer || aiInfo.PlayerID == di.TowardsPlayerID))
				return;
			DiplomacyState preferredDiplomacyState = this.GetPreferredDiplomacyState(aiInfo, updatePlayerInfo, di);
			if (preferredDiplomacyState == DiplomacyState.WAR && di.State != preferredDiplomacyState)
			{
				PlayerInfo playerInfo1 = this._db.GetPlayerInfo(this._player.ID);
				PlayerInfo playerInfo2 = this._db.GetPlayerInfo(di.TowardsPlayerID);
				if (!this._game.CanPerformDiplomacyAction(playerInfo1, playerInfo2, DiplomacyAction.DECLARATION, new RequestType?(), new DemandType?()))
					return;
				int? diplomacyActionCost = this._game.GetDiplomacyActionCost(DiplomacyAction.DECLARATION, new RequestType?(), new DemandType?());
				this._db.SpendDiplomacyPoints(playerInfo1, playerInfo2.FactionID, diplomacyActionCost.Value);
				this._game.DeclareWarFormally(this._player.ID, di.TowardsPlayerID);
			}
			else
			{
				if (StrategicAI.GetDiplomacyStateRank(preferredDiplomacyState) <= StrategicAI.GetDiplomacyStateRank(di.State))
					return;
				ArmisticeTreatyInfo armisticeTreatyInfo = new ArmisticeTreatyInfo();
				armisticeTreatyInfo.Active = false;
				armisticeTreatyInfo.Removed = false;
				armisticeTreatyInfo.InitiatingPlayerId = aiInfo.PlayerID;
				armisticeTreatyInfo.ReceivingPlayerId = di.TowardsPlayerID;
				armisticeTreatyInfo.StartingTurn = this._db.GetTurnCount();
				armisticeTreatyInfo.SuggestedDiplomacyState = preferredDiplomacyState;
				armisticeTreatyInfo.Type = TreatyType.Armistice;
				if (!this._game.CanPerformTreaty((TreatyInfo)armisticeTreatyInfo))
					return;
				this._db.SpendDiplomacyPoints(this._db.GetPlayerInfo(armisticeTreatyInfo.InitiatingPlayerId), this._db.GetPlayerInfo(armisticeTreatyInfo.ReceivingPlayerId).FactionID, this._game.GetTreatyRdpCost((TreatyInfo)armisticeTreatyInfo));
				this._db.InsertTreaty((TreatyInfo)armisticeTreatyInfo);
			}
		}

		private void OfferTreaties(AIInfo aiInfo, StrategicAI.UpdateInfo updateInfo)
		{
			StrategicAI.UpdatePlayerInfo player = updateInfo.Players[aiInfo.PlayerID];
			if (App.Log.Level >= Kerberos.Sots.Engine.LogLevel.Verbose)
			{
				StrategicAI.TraceVerbose(string.Format("Relations for {0} vs...", (object)player.Player));
				foreach (StrategicAI.UpdatePlayerInfo updatePlayerInfo in updateInfo.Players.Values)
				{
					DiplomacyInfo relation = updatePlayerInfo.Relations[player.Player.ID];
					StrategicAI.TraceVerbose(string.Format("   {0}: {1} ({2}), {3}", (object)updatePlayerInfo.Player, (object)relation.GetDiplomaticMood().ToString(), (object)relation.Relations, (object)relation.State.ToString()));
				}
			}
			int turnCount = this._db.GetTurnCount();
			foreach (DiplomacyInfo di in player.Relations.Values)
			{
				if (di.TowardsPlayerID != player.Player.ID && updateInfo.Players[di.TowardsPlayerID].Player.isStandardPlayer && di.isEncountered)
				{
					if (turnCount >= 5 && (turnCount + aiInfo.PlayerID) % 5 == 0)
						this.TryDiplomacyStateAction(aiInfo, player, di);
					if (turnCount >= 10 && this._random.CoinToss(10))
						this.TryDiplomacyAction(aiInfo, updateInfo, di);
				}
			}
		}

		private void TryDiplomacyAction(
		  AIInfo aiInfo,
		  StrategicAI.UpdateInfo updateInfo,
		  DiplomacyInfo di)
		{
			Array values = Enum.GetValues(typeof(DiplomacyAction));
			Dictionary<object, float> source = new Dictionary<object, float>();
			IEnumerable<DiplomacyActionHistoryEntryInfo> diplomacyActionHistory = this._db.GetDiplomacyActionHistory(aiInfo.PlayerID, di.TowardsPlayerID, this._db.GetTurnCount(), 20);
			DiplomaticMood diplomaticMood = di.GetDiplomaticMood();
			foreach (DiplomacyAction action in values)
			{
				foreach (KeyValuePair<object, float> weight in this._player.Faction.DiplomacyWeights.GetWeights(action, diplomaticMood))
				{
					float diplomacyActionViability = this.GetDiplomacyActionViability(aiInfo, updateInfo, diplomacyActionHistory, di, action, weight.Key);
					if ((double)diplomacyActionViability != 0.0 && this._game.CanPerformDiplomacyAction(this._db.GetPlayerInfo(di.PlayerID), this._db.GetPlayerInfo(di.TowardsPlayerID), action, new RequestType?(), new DemandType?()))
						source.Add(weight.Key, weight.Value * diplomacyActionViability);
				}
			}
			if (source.Count <= 0)
				return;
			object type = WeightedChoices.Choose<object>(this._random, source.Select<KeyValuePair<object, float>, Weighted<object>>((Func<KeyValuePair<object, float>, Weighted<object>>)(x =>
		   {
			   KeyValuePair<object, float> keyValuePair = x;
			   object key = keyValuePair.Key;
			   keyValuePair = x;
			   int weight = (int)keyValuePair.Value;
			   return new Weighted<object>(key, weight);
		   })));
			this.ExecuteDiplomacyAction(aiInfo, updateInfo, di, DiplomacyActionWeights.GetActionFromType(type.GetType()), type);
		}

		private void ExecuteDiplomacyAction(
		  AIInfo aiInfo,
		  StrategicAI.UpdateInfo updateInfo,
		  DiplomacyInfo di,
		  DiplomacyAction action,
		  object type)
		{
			StrategicAI.TraceVerbose("P" + (object)aiInfo.PlayerID + " executing diplomacy action " + action.ToString() + " towards P" + di.TowardsPlayerID.ToString());
			switch (action)
			{
				case DiplomacyAction.REQUEST:
					RequestInfo request = new RequestInfo();
					request.InitiatingPlayer = aiInfo.PlayerID;
					request.ReceivingPlayer = di.TowardsPlayerID;
					request.Type = (RequestType)type;
					switch (request.Type)
					{
						case RequestType.SavingsRequest:
							request.RequestValue = (float)(100000 * (int)di.GetDiplomaticMood());
							break;
						case RequestType.SystemInfoRequest:
							StarSystemInfo starSystemInfo1 = this._db.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
						   {
							   int? systemOwningPlayer = this._db.GetSystemOwningPlayer(x.ID);
							   int towardsPlayerId = di.TowardsPlayerID;
							   return systemOwningPlayer.GetValueOrDefault() == towardsPlayerId & systemOwningPlayer.HasValue;
						   })).FirstOrDefault<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => !this._db.IsSurveyed(aiInfo.PlayerID, x.ID)));
							if (starSystemInfo1 == (StarSystemInfo)null)
								return;
							request.RequestValue = (float)starSystemInfo1.ID;
							break;
						case RequestType.ResearchPointsRequest:
							request.RequestValue = (float)(50000 * (int)di.GetDiplomaticMood());
							break;
						case RequestType.GatePermissionRequest:
							StarSystemInfo starSystemInfo2 = this._db.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
						   {
							   int? systemOwningPlayer = this._db.GetSystemOwningPlayer(x.ID);
							   int towardsPlayerId = di.TowardsPlayerID;
							   return systemOwningPlayer.GetValueOrDefault() == towardsPlayerId & systemOwningPlayer.HasValue;
						   })).FirstOrDefault<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => this._db.GetHiverGateForSystem(x.ID, aiInfo.PlayerID) == null));
							if (!(starSystemInfo2 != (StarSystemInfo)null))
								return;
							request.RequestValue = (float)starSystemInfo2.ID;
							break;
					}
					this._db.SpendDiplomacyPoints(aiInfo.PlayerInfo, updateInfo.Players[di.TowardsPlayerID].Player.FactionID, this._game.AssetDatabase.GetDiplomaticRequestPointCost(request.Type));
					this._db.InsertRequest(request);
					break;
				case DiplomacyAction.DEMAND:
					DemandInfo demand = new DemandInfo();
					demand.InitiatingPlayer = aiInfo.PlayerID;
					demand.ReceivingPlayer = di.TowardsPlayerID;
					demand.Type = (DemandType)type;
					switch (demand.Type)
					{
						case DemandType.SavingsDemand:
						case DemandType.SlavesDemand:
							demand.DemandValue = (float)(100000 * (int)(4 - di.GetDiplomaticMood()));
							break;
						case DemandType.SystemInfoDemand:
							StarSystemInfo starSystemInfo3 = this._db.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
						   {
							   int? systemOwningPlayer = this._db.GetSystemOwningPlayer(x.ID);
							   int towardsPlayerId = di.TowardsPlayerID;
							   return systemOwningPlayer.GetValueOrDefault() == towardsPlayerId & systemOwningPlayer.HasValue;
						   })).FirstOrDefault<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => !this._db.IsSurveyed(aiInfo.PlayerID, x.ID)));
							if (starSystemInfo3 == (StarSystemInfo)null)
								return;
							demand.DemandValue = (float)starSystemInfo3.ID;
							break;
						case DemandType.ResearchPointsDemand:
							demand.DemandValue = (float)(50000 * (int)(4 - di.GetDiplomaticMood()));
							break;
					}
					this._db.SpendDiplomacyPoints(aiInfo.PlayerInfo, updateInfo.Players[di.TowardsPlayerID].Player.FactionID, this._game.AssetDatabase.GetDiplomaticDemandPointCost(demand.Type));
					this._db.InsertDemand(demand);
					break;
				case DiplomacyAction.LOBBY:
					StrategicAI.UpdatePlayerInfo updatePlayer = updateInfo.Players[aiInfo.PlayerID];
					List<KeyValuePair<int, StrategicAI.UpdatePlayerInfo>> list = updateInfo.Players.Where<KeyValuePair<int, StrategicAI.UpdatePlayerInfo>>((Func<KeyValuePair<int, StrategicAI.UpdatePlayerInfo>, bool>)(x =>
				   {
					   if (x.Value.Player.isStandardPlayer && updatePlayer.Relations[x.Key].isEncountered)
						   return x.Key != di.TowardsPlayerID;
					   return false;
				   })).ToList<KeyValuePair<int, StrategicAI.UpdatePlayerInfo>>();
					switch ((LobbyType)type)
					{
						case LobbyType.LobbySelf:
							this._game.DoLobbyAction(aiInfo.PlayerID, di.TowardsPlayerID, aiInfo.PlayerID, true);
							return;
						case LobbyType.LobbyEnemy:
							KeyValuePair<int, StrategicAI.UpdatePlayerInfo> keyValuePair1 = list.First<KeyValuePair<int, StrategicAI.UpdatePlayerInfo>>((Func<KeyValuePair<int, StrategicAI.UpdatePlayerInfo>, bool>)(x => StrategicAI.GetDiplomacyStateRank(updatePlayer.Relations[x.Key].State) < StrategicAI.GetDiplomacyStateRank(DiplomacyState.NEUTRAL)));
							this._game.DoLobbyAction(aiInfo.PlayerID, di.TowardsPlayerID, keyValuePair1.Key, false);
							return;
						case LobbyType.LobbyFriendly:
							KeyValuePair<int, StrategicAI.UpdatePlayerInfo> keyValuePair2 = list.First<KeyValuePair<int, StrategicAI.UpdatePlayerInfo>>((Func<KeyValuePair<int, StrategicAI.UpdatePlayerInfo>, bool>)(x => StrategicAI.GetDiplomacyStateRank(updatePlayer.Relations[x.Key].State) > StrategicAI.GetDiplomacyStateRank(DiplomacyState.NEUTRAL)));
							this._game.DoLobbyAction(aiInfo.PlayerID, di.TowardsPlayerID, keyValuePair2.Key, true);
							return;
						default:
							return;
					}
			}
		}

		private float GetDiplomacyActionViability(
		  AIInfo aiInfo,
		  StrategicAI.UpdateInfo updateInfo,
		  IEnumerable<DiplomacyActionHistoryEntryInfo> history,
		  DiplomacyInfo di,
		  DiplomacyAction action,
		  object type)
		{
			if (history.FirstOrDefault<DiplomacyActionHistoryEntryInfo>((Func<DiplomacyActionHistoryEntryInfo, bool>)(x => x.Action == action)) != null)
				return 0.0f;
			float num = 1f;
			switch (action)
			{
				case DiplomacyAction.DECLARATION:
					num = 0.0f;
					break;
				case DiplomacyAction.REQUEST:
					num = this.GetRequestViability(aiInfo, updateInfo, di, type);
					break;
				case DiplomacyAction.DEMAND:
					num = this.GetDemandViability(aiInfo, updateInfo, di, type);
					break;
				case DiplomacyAction.TREATY:
					if (type.GetType() == typeof(LimitationTreatyType))
					{
						num = 0.0f;
						break;
					}
					if (type.GetType() == typeof(TreatyType))
					{
						num = (TreatyType)type != TreatyType.Limitation ? 0.0f : 0.0f;
						break;
					}
					break;
				case DiplomacyAction.LOBBY:
					num = this.GetLobbyViability(aiInfo, updateInfo, di, type);
					break;
				case DiplomacyAction.SPIN:
					num = 0.0f;
					break;
				case DiplomacyAction.SURPRISEATTACK:
					num = 0.0f;
					break;
				case DiplomacyAction.GIVE:
					num = 0.0f;
					break;
			}
			return num;
		}

		private float GetRequestViability(
		  AIInfo aiInfo,
		  StrategicAI.UpdateInfo updateInfo,
		  DiplomacyInfo di,
		  object type)
		{
			if (type.GetType() != typeof(RequestType))
				return 0.0f;
			RequestType rt = (RequestType)type;
			if (di.GetDiplomaticMood() <= DiplomaticMood.Indifference || aiInfo.PlayerInfo.GetTotalDiplomacyPoints(updateInfo.Players[di.TowardsPlayerID].Player.FactionID) < this._game.AssetDatabase.GetDiplomaticRequestPointCost(rt))
				return 0.0f;
			float num = 1f;
			switch (rt)
			{
				case RequestType.SavingsRequest:
					if (aiInfo.PlayerInfo.Savings < 0.0)
					{
						num = 4f;
						break;
					}
					if (aiInfo.PlayerInfo.Savings < 1000000.0)
					{
						num = 2f;
						break;
					}
					break;
				case RequestType.SystemInfoRequest:
					if (this._db.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
				   {
					   int? systemOwningPlayer = this._db.GetSystemOwningPlayer(x.ID);
					   int towardsPlayerId = di.TowardsPlayerID;
					   if (systemOwningPlayer.GetValueOrDefault() == towardsPlayerId)
						   return systemOwningPlayer.HasValue;
					   return false;
				   })).FirstOrDefault<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => !this._db.IsSurveyed(aiInfo.PlayerID, x.ID))) != (StarSystemInfo)null)
					{
						num = 2f;
						break;
					}
					break;
				case RequestType.ResearchPointsRequest:
					num = (double)aiInfo.PlayerInfo.RateResearchCurrentProject <= 0.0 ? 0.0f : 1.5f;
					break;
				case RequestType.MilitaryAssistanceRequest:
					num = 0.0f;
					break;
				case RequestType.GatePermissionRequest:
					if (this._player.Faction.Name != "Hiver")
					{
						num = 0.0f;
						break;
					}
					if (this._db.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
				   {
					   int? systemOwningPlayer = this._db.GetSystemOwningPlayer(x.ID);
					   int towardsPlayerId = di.TowardsPlayerID;
					   if (systemOwningPlayer.GetValueOrDefault() == towardsPlayerId)
						   return systemOwningPlayer.HasValue;
					   return false;
				   })).FirstOrDefault<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => this._db.GetHiverGateForSystem(x.ID, aiInfo.PlayerID) == null)) != (StarSystemInfo)null)
					{
						num = 1f;
						break;
					}
					break;
				case RequestType.EstablishEnclaveRequest:
					num = 0.0f;
					break;
			}
			return num;
		}

		private float GetLobbyViability(
		  AIInfo aiInfo,
		  StrategicAI.UpdateInfo updateInfo,
		  DiplomacyInfo di,
		  object type)
		{
			if (type.GetType() != typeof(LobbyType) || !this._game.GetPlayerObject(di.TowardsPlayerID).IsAI())
				return 0.0f;
			float num = 1f;
			StrategicAI.UpdatePlayerInfo updatePlayer = updateInfo.Players[aiInfo.PlayerID];
			IEnumerable<KeyValuePair<int, StrategicAI.UpdatePlayerInfo>> source = updateInfo.Players.Where<KeyValuePair<int, StrategicAI.UpdatePlayerInfo>>((Func<KeyValuePair<int, StrategicAI.UpdatePlayerInfo>, bool>)(x =>
		   {
			   if (x.Value.Player.isStandardPlayer && updatePlayer.Relations[x.Key].isEncountered)
				   return x.Key != di.TowardsPlayerID;
			   return false;
		   }));
			switch ((LobbyType)type)
			{
				case LobbyType.LobbyEnemy:
					if (!source.Any<KeyValuePair<int, StrategicAI.UpdatePlayerInfo>>((Func<KeyValuePair<int, StrategicAI.UpdatePlayerInfo>, bool>)(x => StrategicAI.GetDiplomacyStateRank(updatePlayer.Relations[x.Key].State) < StrategicAI.GetDiplomacyStateRank(DiplomacyState.NEUTRAL))))
					{
						num = 0.0f;
						break;
					}
					break;
				case LobbyType.LobbyFriendly:
					if (!source.Any<KeyValuePair<int, StrategicAI.UpdatePlayerInfo>>((Func<KeyValuePair<int, StrategicAI.UpdatePlayerInfo>, bool>)(x => StrategicAI.GetDiplomacyStateRank(updatePlayer.Relations[x.Key].State) > StrategicAI.GetDiplomacyStateRank(DiplomacyState.NEUTRAL))))
					{
						num = 0.0f;
						break;
					}
					break;
			}
			return num;
		}

		private float GetDemandViability(
		  AIInfo aiInfo,
		  StrategicAI.UpdateInfo updateInfo,
		  DiplomacyInfo di,
		  object type)
		{
			if (type.GetType() != typeof(DemandType))
				return 0.0f;
			DemandType dt = (DemandType)type;
			if (di.GetDiplomaticMood() > DiplomaticMood.Indifference || aiInfo.PlayerInfo.GetTotalDiplomacyPoints(updateInfo.Players[di.TowardsPlayerID].Player.FactionID) < this._game.AssetDatabase.GetDiplomaticDemandPointCost(dt))
				return 0.0f;
			float num = 1f;
			switch (dt)
			{
				case DemandType.SavingsDemand:
					if (aiInfo.PlayerInfo.Savings < 0.0)
					{
						num = 4f;
						break;
					}
					if (aiInfo.PlayerInfo.Savings < 1000000.0)
					{
						num = 2f;
						break;
					}
					break;
				case DemandType.SystemInfoDemand:
					if (this._db.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
				   {
					   int? systemOwningPlayer = this._db.GetSystemOwningPlayer(x.ID);
					   int towardsPlayerId = di.TowardsPlayerID;
					   return systemOwningPlayer.GetValueOrDefault() == towardsPlayerId & systemOwningPlayer.HasValue;
				   })).FirstOrDefault<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => this._db.IsSurveyed(aiInfo.PlayerID, x.ID))) != (StarSystemInfo)null)
					{
						num = 2f;
						break;
					}
					break;
				case DemandType.ResearchPointsDemand:
					num = (double)aiInfo.PlayerInfo.RateResearchCurrentProject <= 0.0 ? 0.0f : 1.5f;
					break;
				case DemandType.SlavesDemand:
					num = !this._db.AssetDatabase.GetFaction(this._db.GetPlayerFactionID(aiInfo.PlayerID)).HasSlaves() ? 0.0f : 1.5f;
					break;
				case DemandType.WorldDemand:
					num = 0.0f;
					break;
				case DemandType.ProvinceDemand:
					num = 0.0f;
					break;
				case DemandType.SurrenderDemand:
					num = 0.0f;
					break;
			}
			return num;
		}

		private void UpdateStance(AIInfo aiInfo, StrategicAI.UpdateInfo updateInfo)
		{
			if (this._db == null)
				this._db = this._game.GameDatabase;
			AIStance stance = aiInfo.Stance;
			float num1 = this.AssessExpansionRoom();
			float num2 = this.AssessOwnStrength();
			StrategicAI.TraceVerbose(string.Format("Comparing this Player {0} strength {1} vs...", (object)this._player.ID, (object)num2));
			float num3 = 100f;
			int num4 = this.AssessOwnTechLevel();
			int num5 = 0;
			List<int> source = new List<int>();
			foreach (PlayerInfo playerInfo in this._db.GetPlayerInfos())
			{
				if (playerInfo.ID != this._player.ID && !playerInfo.isDefeated && (playerInfo.isStandardPlayer || playerInfo.isAIRebellionPlayer))
				{
					if (this._db.GetDiplomacyStateBetweenPlayers(playerInfo.ID, this._player.ID) == DiplomacyState.WAR && (double)this.AssessPlayerStrength(playerInfo.ID) > 0.0)
					{
						StrategicAI.TraceVerbose(string.Format("  Player {0} strength: {1}", (object)playerInfo.ID, (object)this.AssessPlayerStrength(playerInfo.ID)));
						source.Add(playerInfo.ID);
					}
					int num6 = this.AssessPlayerTechLevel(playerInfo.ID);
					if (num6 > num5)
						num5 = num6;
				}
			}
			if (source.Count<int>() > 0)
			{
				int num6 = 0;
				int num7 = 0;
				float num8 = 0.0f;
				float num9 = 0.0f;
				foreach (int otherPlayerID in source)
				{
					float num10 = this.AssessPlayerStrength(otherPlayerID);
					if (num6 == 0 || (double)num10 < (double)num8)
					{
						num6 = otherPlayerID;
						num8 = num10;
					}
					if (num7 == 0 || (double)num10 > (double)num9)
					{
						num7 = otherPlayerID;
						num9 = num10;
					}
				}
				aiInfo.Stance = (double)num2 <= (double)num8 * 2.0 ? ((double)num2 < (double)num9 ? AIStance.DEFENDING : AIStance.DESTROYING) : AIStance.CONQUERING;
			}
			else if ((double)num2 >= (double)num3)
			{
				int targetPlayerId = this.PickAFight();
				if (targetPlayerId != 0)
				{
					this._game.DeclareWarFormally(this._player.ID, targetPlayerId);
					aiInfo.Stance = AIStance.CONQUERING;
				}
			}
			else if ((double)num1 > 1.0)
				aiInfo.Stance = AIStance.EXPANDING;
			else if (num4 < num5)
				aiInfo.Stance = AIStance.HUNKERING;
			else if ((double)num2 < (double)num3)
				aiInfo.Stance = AIStance.ARMING;
			if (aiInfo.Stance == stance)
				return;
			StrategicAI.TraceVerbose("Setting AI stance to: " + (object)aiInfo.Stance);
			this._db.UpdateAIInfo(aiInfo);
		}

		private void SetRequiredDefaultDesigns(AIInfo aiInfo)
		{
			foreach (ShipRole defaultAiShipRole in this._player.Faction.DefaultAIShipRoles)
				DesignLab.SetDefaultDesign(this._game, defaultAiShipRole, new WeaponRole?(), this._player.ID, null, new bool?(), this._techStyles, new AIStance?(aiInfo.Stance));
		}

		private void ManageStations(AIInfo aiInfo)
		{
			StationModuleQueue.UpdateStationMapsForFaction(this._player.Faction.Name);
			Dictionary<ModuleEnums.StationModuleType, int> queuedItemMap = new Dictionary<ModuleEnums.StationModuleType, int>();
			foreach (StationInfo station in this._db.GetStationInfosByPlayerID(this._player.ID).ToList<StationInfo>())
			{
				if (!this._game.StationIsUpgradable(station) && !this._game.StationIsUpgrading(station))
				{
					StationModuleQueue.InitializeQueuedItemMap(this._game, station, queuedItemMap);
					StationModuleQueue.AutoFillModules(this._game, station, queuedItemMap);
					if (queuedItemMap.Values.Sum() > 0)
						StationModuleQueue.ConfirmStationQueuedItems(this._game, station, queuedItemMap);
				}
			}
		}

		public void DesignShips(AIInfo aiInfo)
		{
			StrategicAI.TraceVerbose("Designing ships...");
			this.SetRequiredDefaultDesigns(aiInfo);
			List<StrategicAI.DesignPriority> unsortedPriorities = new List<StrategicAI.DesignPriority>();
			unsortedPriorities.Add(new StrategicAI.DesignPriority()
			{
				role = ShipRole.COMBAT,
				weight = 0.75f
			});
			unsortedPriorities.Add(new StrategicAI.DesignPriority()
			{
				role = ShipRole.POLICE,
				weight = 0.1f
			});
			unsortedPriorities.Add(new StrategicAI.DesignPriority()
			{
				role = ShipRole.PLATFORM,
				weight = 0.2f
			});
			StrategicAI.DesignPriority designPriority1;
			if (this._player.Faction.Name == "hiver")
			{
				List<StrategicAI.DesignPriority> designPriorityList = unsortedPriorities;
				designPriority1 = new StrategicAI.DesignPriority();
				designPriority1.role = ShipRole.GATE;
				designPriority1.weight = 0.2f;
				StrategicAI.DesignPriority designPriority2 = designPriority1;
				designPriorityList.Add(designPriority2);
			}
			if (this._player.Faction.Name == "zuul")
			{
				List<StrategicAI.DesignPriority> designPriorityList1 = unsortedPriorities;
				designPriority1 = new StrategicAI.DesignPriority();
				designPriority1.role = ShipRole.SCAVENGER;
				designPriority1.weight = 0.2f;
				StrategicAI.DesignPriority designPriority2 = designPriority1;
				designPriorityList1.Add(designPriority2);
				List<StrategicAI.DesignPriority> designPriorityList2 = unsortedPriorities;
				designPriority1 = new StrategicAI.DesignPriority();
				designPriority1.role = ShipRole.SLAVEDISK;
				designPriority1.weight = 0.2f;
				StrategicAI.DesignPriority designPriority3 = designPriority1;
				designPriorityList2.Add(designPriority3);
				List<StrategicAI.DesignPriority> designPriorityList3 = unsortedPriorities;
				designPriority1 = new StrategicAI.DesignPriority();
				designPriority1.role = ShipRole.CARRIER_BOARDING;
				designPriority1.weight = 0.2f;
				StrategicAI.DesignPriority designPriority4 = designPriority1;
				designPriorityList3.Add(designPriority4);
			}
			else
			{
				List<StrategicAI.DesignPriority> designPriorityList = unsortedPriorities;
				designPriority1 = new StrategicAI.DesignPriority();
				designPriority1.role = ShipRole.CARRIER_BOARDING;
				designPriority1.weight = 0.05f;
				StrategicAI.DesignPriority designPriority2 = designPriority1;
				designPriorityList.Add(designPriority2);
			}
			if (this._player.Faction.Name == "morrigi")
			{
				List<StrategicAI.DesignPriority> designPriorityList1 = unsortedPriorities;
				designPriority1 = new StrategicAI.DesignPriority();
				designPriority1.role = ShipRole.DRONE;
				designPriority1.weight = 0.5f;
				StrategicAI.DesignPriority designPriority2 = designPriority1;
				designPriorityList1.Add(designPriority2);
				List<StrategicAI.DesignPriority> designPriorityList2 = unsortedPriorities;
				designPriority1 = new StrategicAI.DesignPriority();
				designPriority1.role = ShipRole.CARRIER_DRONE;
				designPriority1.weight = 0.5f;
				StrategicAI.DesignPriority designPriority3 = designPriority1;
				designPriorityList2.Add(designPriority3);
			}
			else
			{
				List<StrategicAI.DesignPriority> designPriorityList1 = unsortedPriorities;
				designPriority1 = new StrategicAI.DesignPriority();
				designPriority1.role = ShipRole.DRONE;
				designPriority1.weight = 0.2f;
				StrategicAI.DesignPriority designPriority2 = designPriority1;
				designPriorityList1.Add(designPriority2);
				List<StrategicAI.DesignPriority> designPriorityList2 = unsortedPriorities;
				designPriority1 = new StrategicAI.DesignPriority();
				designPriority1.role = ShipRole.CARRIER_DRONE;
				designPriority1.weight = 0.5f;
				StrategicAI.DesignPriority designPriority3 = designPriority1;
				designPriorityList2.Add(designPriority3);
			}
			if (this._db.GetDesignInfosForPlayer(this._player.ID).Any<DesignInfo>((Func<DesignInfo, bool>)(x => x.Role == ShipRole.CARRIER)))
			{
				List<StrategicAI.DesignPriority> designPriorityList1 = unsortedPriorities;
				designPriority1 = new StrategicAI.DesignPriority();
				designPriority1.role = ShipRole.BR_PATROL;
				designPriority1.weight = 0.75f;
				StrategicAI.DesignPriority designPriority2 = designPriority1;
				designPriorityList1.Add(designPriority2);
				List<StrategicAI.DesignPriority> designPriorityList2 = unsortedPriorities;
				designPriority1 = new StrategicAI.DesignPriority();
				designPriority1.role = ShipRole.BR_SCOUT;
				designPriority1.weight = 0.75f;
				StrategicAI.DesignPriority designPriority3 = designPriority1;
				designPriorityList2.Add(designPriority3);
				List<StrategicAI.DesignPriority> designPriorityList3 = unsortedPriorities;
				designPriority1 = new StrategicAI.DesignPriority();
				designPriority1.role = ShipRole.BR_SPINAL;
				designPriority1.weight = 0.75f;
				StrategicAI.DesignPriority designPriority4 = designPriority1;
				designPriorityList3.Add(designPriority4);
				List<StrategicAI.DesignPriority> designPriorityList4 = unsortedPriorities;
				designPriority1 = new StrategicAI.DesignPriority();
				designPriority1.role = ShipRole.BR_ESCORT;
				designPriority1.weight = 0.75f;
				StrategicAI.DesignPriority designPriority5 = designPriority1;
				designPriorityList4.Add(designPriority5);
				List<StrategicAI.DesignPriority> designPriorityList5 = unsortedPriorities;
				designPriority1 = new StrategicAI.DesignPriority();
				designPriority1.role = ShipRole.BR_INTERCEPTOR;
				designPriority1.weight = 0.75f;
				StrategicAI.DesignPriority designPriority6 = designPriority1;
				designPriorityList5.Add(designPriority6);
				List<StrategicAI.DesignPriority> designPriorityList6 = unsortedPriorities;
				designPriority1 = new StrategicAI.DesignPriority();
				designPriority1.role = ShipRole.BR_TORPEDO;
				designPriority1.weight = 0.75f;
				StrategicAI.DesignPriority designPriority7 = designPriority1;
				designPriorityList6.Add(designPriority7);
				List<StrategicAI.DesignPriority> designPriorityList7 = unsortedPriorities;
				designPriority1 = new StrategicAI.DesignPriority();
				designPriority1.role = ShipRole.BATTLECRUISER;
				designPriority1.weight = 0.5f;
				StrategicAI.DesignPriority designPriority8 = designPriority1;
				designPriorityList7.Add(designPriority8);
				List<StrategicAI.DesignPriority> designPriorityList8 = unsortedPriorities;
				designPriority1 = new StrategicAI.DesignPriority();
				designPriority1.role = ShipRole.BATTLESHIP;
				designPriority1.weight = 0.5f;
				StrategicAI.DesignPriority designPriority9 = designPriority1;
				designPriorityList8.Add(designPriority9);
			}
			switch (aiInfo.Stance)
			{
				case AIStance.EXPANDING:
					List<StrategicAI.DesignPriority> designPriorityList9 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.COLONIZER;
					designPriority1.weight = 0.1f;
					StrategicAI.DesignPriority designPriority10 = designPriority1;
					designPriorityList9.Add(designPriority10);
					List<StrategicAI.DesignPriority> designPriorityList10 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.COMMAND;
					designPriority1.weight = 0.75f;
					StrategicAI.DesignPriority designPriority11 = designPriority1;
					designPriorityList10.Add(designPriority11);
					List<StrategicAI.DesignPriority> designPriorityList11 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.CONSTRUCTOR;
					designPriority1.weight = 0.01f;
					StrategicAI.DesignPriority designPriority12 = designPriority1;
					designPriorityList11.Add(designPriority12);
					List<StrategicAI.DesignPriority> designPriorityList12 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.SUPPLY;
					designPriority1.weight = 0.1f;
					StrategicAI.DesignPriority designPriority13 = designPriority1;
					designPriorityList12.Add(designPriority13);
					break;
				case AIStance.ARMING:
					List<StrategicAI.DesignPriority> designPriorityList13 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.COMMAND;
					designPriority1.weight = 0.75f;
					StrategicAI.DesignPriority designPriority14 = designPriority1;
					designPriorityList13.Add(designPriority14);
					List<StrategicAI.DesignPriority> designPriorityList14 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.SCOUT;
					designPriority1.weight = 0.1f;
					StrategicAI.DesignPriority designPriority15 = designPriority1;
					designPriorityList14.Add(designPriority15);
					List<StrategicAI.DesignPriority> designPriorityList15 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.CARRIER;
					designPriority1.weight = 0.5f;
					StrategicAI.DesignPriority designPriority16 = designPriority1;
					designPriorityList15.Add(designPriority16);
					List<StrategicAI.DesignPriority> designPriorityList16 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.SUPPLY;
					designPriority1.weight = 0.1f;
					StrategicAI.DesignPriority designPriority17 = designPriority1;
					designPriorityList16.Add(designPriority17);
					break;
				case AIStance.HUNKERING:
					List<StrategicAI.DesignPriority> designPriorityList17 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.COMMAND;
					designPriority1.weight = 0.75f;
					StrategicAI.DesignPriority designPriority18 = designPriority1;
					designPriorityList17.Add(designPriority18);
					List<StrategicAI.DesignPriority> designPriorityList18 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.CONSTRUCTOR;
					designPriority1.weight = 0.01f;
					StrategicAI.DesignPriority designPriority19 = designPriority1;
					designPriorityList18.Add(designPriority19);
					List<StrategicAI.DesignPriority> designPriorityList19 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.CARRIER;
					designPriority1.weight = 0.5f;
					StrategicAI.DesignPriority designPriority20 = designPriority1;
					designPriorityList19.Add(designPriority20);
					List<StrategicAI.DesignPriority> designPriorityList20 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.COMBAT;
					designPriority1.weight = 0.75f;
					StrategicAI.DesignPriority designPriority21 = designPriority1;
					designPriorityList20.Add(designPriority21);
					break;
				case AIStance.CONQUERING:
					List<StrategicAI.DesignPriority> designPriorityList21 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.COMMAND;
					designPriority1.weight = 0.75f;
					StrategicAI.DesignPriority designPriority22 = designPriority1;
					designPriorityList21.Add(designPriority22);
					List<StrategicAI.DesignPriority> designPriorityList22 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.SCOUT;
					designPriority1.weight = 0.1f;
					StrategicAI.DesignPriority designPriority23 = designPriority1;
					designPriorityList22.Add(designPriority23);
					List<StrategicAI.DesignPriority> designPriorityList23 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.CARRIER;
					designPriority1.weight = 0.5f;
					StrategicAI.DesignPriority designPriority24 = designPriority1;
					designPriorityList23.Add(designPriority24);
					List<StrategicAI.DesignPriority> designPriorityList24 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.CARRIER_ASSAULT;
					designPriority1.weight = 0.75f;
					StrategicAI.DesignPriority designPriority25 = designPriority1;
					designPriorityList24.Add(designPriority25);
					List<StrategicAI.DesignPriority> designPriorityList25 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.CARRIER_BIO;
					designPriority1.weight = 0.4f;
					StrategicAI.DesignPriority designPriority26 = designPriority1;
					designPriorityList25.Add(designPriority26);
					List<StrategicAI.DesignPriority> designPriorityList26 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.SUPPLY;
					designPriority1.weight = 0.1f;
					StrategicAI.DesignPriority designPriority27 = designPriority1;
					designPriorityList26.Add(designPriority27);
					List<StrategicAI.DesignPriority> designPriorityList27 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.COLONIZER;
					designPriority1.weight = 0.1f;
					StrategicAI.DesignPriority designPriority28 = designPriority1;
					designPriorityList27.Add(designPriority28);
					List<StrategicAI.DesignPriority> designPriorityList28 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.ASSAULTSHUTTLE;
					designPriority1.weight = 0.5f;
					StrategicAI.DesignPriority designPriority29 = designPriority1;
					designPriorityList28.Add(designPriority29);
					break;
				case AIStance.DESTROYING:
					List<StrategicAI.DesignPriority> designPriorityList29 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.COMMAND;
					designPriority1.weight = 0.75f;
					StrategicAI.DesignPriority designPriority30 = designPriority1;
					designPriorityList29.Add(designPriority30);
					List<StrategicAI.DesignPriority> designPriorityList30 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.SCOUT;
					designPriority1.weight = 0.1f;
					StrategicAI.DesignPriority designPriority31 = designPriority1;
					designPriorityList30.Add(designPriority31);
					List<StrategicAI.DesignPriority> designPriorityList31 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.CARRIER;
					designPriority1.weight = 0.5f;
					StrategicAI.DesignPriority designPriority32 = designPriority1;
					designPriorityList31.Add(designPriority32);
					List<StrategicAI.DesignPriority> designPriorityList32 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.CARRIER_ASSAULT;
					designPriority1.weight = 0.5f;
					StrategicAI.DesignPriority designPriority33 = designPriority1;
					designPriorityList32.Add(designPriority33);
					List<StrategicAI.DesignPriority> designPriorityList33 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.CARRIER_BIO;
					designPriority1.weight = 0.4f;
					StrategicAI.DesignPriority designPriority34 = designPriority1;
					designPriorityList33.Add(designPriority34);
					List<StrategicAI.DesignPriority> designPriorityList34 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.SUPPLY;
					designPriority1.weight = 0.1f;
					StrategicAI.DesignPriority designPriority35 = designPriority1;
					designPriorityList34.Add(designPriority35);
					List<StrategicAI.DesignPriority> designPriorityList35 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.COMBAT;
					designPriority1.weight = 0.75f;
					StrategicAI.DesignPriority designPriority36 = designPriority1;
					designPriorityList35.Add(designPriority36);
					List<StrategicAI.DesignPriority> designPriorityList36 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.ASSAULTSHUTTLE;
					designPriority1.weight = 0.5f;
					StrategicAI.DesignPriority designPriority37 = designPriority1;
					designPriorityList36.Add(designPriority37);
					break;
				case AIStance.DEFENDING:
					List<StrategicAI.DesignPriority> designPriorityList37 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.COMMAND;
					designPriority1.weight = 0.75f;
					StrategicAI.DesignPriority designPriority38 = designPriority1;
					designPriorityList37.Add(designPriority38);
					List<StrategicAI.DesignPriority> designPriorityList38 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.SCOUT;
					designPriority1.weight = 0.1f;
					StrategicAI.DesignPriority designPriority39 = designPriority1;
					designPriorityList38.Add(designPriority39);
					List<StrategicAI.DesignPriority> designPriorityList39 = unsortedPriorities;
					designPriority1 = new StrategicAI.DesignPriority();
					designPriority1.role = ShipRole.CARRIER;
					designPriority1.weight = 0.5f;
					StrategicAI.DesignPriority designPriority40 = designPriority1;
					designPriorityList39.Add(designPriority40);
					break;
			}
			if (App.Log.Level >= Kerberos.Sots.Engine.LogLevel.Verbose)
			{
				StrategicAI.TraceVerbose(string.Format("  Base design priorities for {0}...", (object)aiInfo.Stance));
				foreach (StrategicAI.DesignPriority designPriority2 in unsortedPriorities)
					StrategicAI.TraceVerbose(string.Format("    {0,15}: {1}", (object)designPriority2.role, (object)designPriority2.weight));
			}
			this.UpdateDesigns(aiInfo, unsortedPriorities);
		}

		private static bool DoesDesignModuleMatch(DesignModuleInfo a, DesignModuleInfo b)
		{
			int? designId1 = a.DesignID;
			int? designId2 = b.DesignID;
			if ((designId1.GetValueOrDefault() != designId2.GetValueOrDefault() ? 1 : (designId1.HasValue != designId2.HasValue ? 1 : 0)) != 0 || a.ModuleID != b.ModuleID)
				return false;
			int? weaponId1 = a.WeaponID;
			int? weaponId2 = b.WeaponID;
			return (weaponId1.GetValueOrDefault() != weaponId2.GetValueOrDefault() ? 1 : (weaponId1.HasValue != weaponId2.HasValue ? 1 : 0)) == 0;
		}

		private static bool DoesWeaponBankMatch(WeaponBankInfo a, WeaponBankInfo b)
		{
			int? designId1 = a.DesignID;
			int? designId2 = b.DesignID;
			if ((designId1.GetValueOrDefault() != designId2.GetValueOrDefault() ? 1 : (designId1.HasValue != designId2.HasValue ? 1 : 0)) != 0)
				return false;
			int? weaponId1 = a.WeaponID;
			int? weaponId2 = b.WeaponID;
			return (weaponId1.GetValueOrDefault() != weaponId2.GetValueOrDefault() ? 1 : (weaponId1.HasValue != weaponId2.HasValue ? 1 : 0)) == 0;
		}

		private static bool DoesDesignSectionMatch(DesignSectionInfo a, DesignSectionInfo b)
		{
			if (a.ShipSectionAsset != b.ShipSectionAsset || a.Modules.Count != b.Modules.Count || a.Techs != b.Techs && (a.Techs == null && b.Techs.Count == 0 || b.Techs == null && a.Techs.Count == 0) || (a.Techs.Count != b.Techs.Count || a.WeaponBanks.Count != b.WeaponBanks.Count))
				return false;
			for (int index = 0; index < a.Techs.Count; ++index)
			{
				if (a.Techs[index] != b.Techs[index])
					return false;
			}
			for (int index = 0; index < a.WeaponBanks.Count; ++index)
			{
				if (!StrategicAI.DoesWeaponBankMatch(a.WeaponBanks[index], b.WeaponBanks[index]))
					return false;
			}
			return true;
		}

		private static bool DoesDesignMatch(DesignInfo a, DesignInfo b)
		{
			if (a.Role != b.Role || a.Role != ShipRole.CONSTRUCTOR && a.Role != ShipRole.FREIGHTER && a.WeaponRole != b.WeaponRole || (a.Class != b.Class || a.DesignSections.Length != b.DesignSections.Length))
				return false;
			for (int index = 0; index < a.DesignSections.Length; ++index)
			{
				if (!StrategicAI.DoesDesignSectionMatch(a.DesignSections[index], b.DesignSections[index]))
					return false;
			}
			return true;
		}

		private static bool IsShipRoleEquivilant(ShipRole shipRole, ShipRole desiredRole)
		{
			if (desiredRole == shipRole)
				return true;
			if (shipRole == ShipRole.COMBAT)
			{
				switch (desiredRole)
				{
					case ShipRole.CARRIER:
					case ShipRole.SCOUT:
					case ShipRole.E_WARFARE:
					case ShipRole.SCAVENGER:
					case ShipRole.CARRIER_ASSAULT:
					case ShipRole.CARRIER_DRONE:
					case ShipRole.CARRIER_BIO:
					case ShipRole.CARRIER_BOARDING:
						return true;
					default:
						return false;
				}
			}
			else
			{
				if (desiredRole != ShipRole.COMBAT)
					return false;
				switch (shipRole)
				{
					case ShipRole.CARRIER:
					case ShipRole.SCOUT:
					case ShipRole.E_WARFARE:
					case ShipRole.SCAVENGER:
					case ShipRole.CARRIER_ASSAULT:
					case ShipRole.CARRIER_DRONE:
					case ShipRole.CARRIER_BIO:
					case ShipRole.CARRIER_BOARDING:
						return true;
					default:
						return false;
				}
			}
		}

		public static List<ShipRole> GetEquivilantShipRoles(ShipRole shipRole)
		{
			List<ShipRole> shipRoleList = new List<ShipRole>();
			shipRoleList.Add(shipRole);
			if (shipRole == ShipRole.COMBAT)
			{
				shipRoleList.Add(ShipRole.CARRIER);
				shipRoleList.Add(ShipRole.CARRIER_ASSAULT);
				shipRoleList.Add(ShipRole.CARRIER_BIO);
				shipRoleList.Add(ShipRole.CARRIER_BOARDING);
				shipRoleList.Add(ShipRole.CARRIER_DRONE);
				shipRoleList.Add(ShipRole.E_WARFARE);
			}
			return shipRoleList;
		}

		private void UpdateDesigns(
		  AIInfo aiInfo,
		  List<StrategicAI.DesignPriority> unsortedPriorities)
		{
			StrategicAI.TraceVerbose("  Retiring obsolete designs...");
			foreach (DesignInfo design in this._db.GetVisibleDesignInfosForPlayer(this._player.ID).Where<DesignInfo>((Func<DesignInfo, bool>)(x => x.Class != ShipClass.Station)))
			{
				if (!Kerberos.Sots.StarFleet.StarFleet.DesignIsSuulka(this._game.App, design) && StrategicAI.IsDesignObsolete(this._game, this._player.ID, design.ID))
				{
					if (App.Log.Level >= Kerberos.Sots.Engine.LogLevel.Verbose)
						StrategicAI.TraceVerbose(string.Format("    Obsolete: {0}", (object)design));
					this._game.GameDatabase.RemovePlayerDesign(design.ID);
				}
			}
			List<StrategicAI.DesignPriority> list = unsortedPriorities.OrderByDescending<StrategicAI.DesignPriority, float>((Func<StrategicAI.DesignPriority, float>)(x => x.weight)).ToList<StrategicAI.DesignPriority>();
			List<float> source1 = new List<float>();
			StrategicAI.TraceVerbose("  Collecting priority designs...");
			foreach (StrategicAI.DesignPriority designPriority in list)
			{
				int num1 = this._db.GetTurnCount();
				List<DesignInfo> source2 = new List<DesignInfo>(this._db.GetVisibleDesignInfosForPlayerAndRole(this._player.ID, designPriority.role, true).Where<DesignInfo>((Func<DesignInfo, bool>)(x => !x.IsSuulka())));
				foreach (DesignInfo designInfo in source2)
				{
					if (designInfo.DesignDate < num1)
						num1 = designInfo.DesignDate;
				}
				if (!source2.Any<DesignInfo>())
				{
					StrategicAI.TraceVerbose(string.Format("    Creating a new design for {0}...", (object)designPriority.role));
					ShipClass shipClass = DesignLab.SuggestShipClassForNewDesign(this._db, this._player, designPriority.role);
					WeaponRole wpnRole = DesignLab.SuggestWeaponRoleForNewDesign(aiInfo.Stance, designPriority.role, shipClass);
					DesignInfo design = (DesignInfo)null;
					if (designPriority.role == ShipRole.COMBAT)
						design = this.TryCounterDesign(shipClass);
					if (design == null)
						design = DesignLab.DesignShip(this._game, shipClass, designPriority.role, wpnRole, this._player.ID, this._techStyles);
					if (design != null)
					{
						this._db.InsertDesignByDesignInfo(design);
						StrategicAI.TraceVerbose(string.Format("    New design inserted: {0}", (object)design));
						source1.Add(0.0f);
					}
				}
				else
				{
					float num2 = designPriority.weight * (float)(this._db.GetTurnCount() - num1);
					source1.Add(num2);
					StrategicAI.TraceVerbose(string.Format("    + {0} redesign score {1}, turn {2}.", (object)designPriority.role, (object)num2, (object)num1));
				}
			}
			float num3 = 1f;
			if (aiInfo.Stance == AIStance.EXPANDING || aiInfo.Stance == AIStance.HUNKERING)
				num3 = 1f;
			StrategicAI.TraceVerbose(string.Format("  Redesign minimum weight threshold for {0}: {1}", (object)aiInfo.Stance, (object)num3));
			if (source1.Count == 0)
			{
				StrategicAI.TraceVerbose("  Stopping because there are no redesign weights.");
			}
			else
			{
				float num1 = 0.0f;
				for (int index = 0; index < source1.Count<float>(); ++index)
				{
					if ((double)source1[index] < (double)num3)
						source1[index] = 0.0f;
					num1 += source1[index];
				}
				StrategicAI.TraceVerbose(string.Format("  Filtered weight sum: {0}", (object)num1));
				if ((double)num1 < 1.0)
				{
					StrategicAI.TraceVerbose("  Stopping because the weight sum is less than one.");
				}
				else
				{
					float num2 = (float)App.GetSafeRandom().NextDouble() * num1;
					StrategicAI.TraceVerbose(string.Format("  Testing candidates for redesign based on initial roll: {0} out of {1}... ", (object)num2, (object)num1));
					for (int index = 0; index < source1.Count<float>(); ++index)
					{
						if ((double)num2 < (double)source1[index])
						{
							StrategicAI.TraceVerbose(string.Format("    {0} < {1}: Attempting redesign for {2}, {3}...", (object)num2, (object)source1[index], (object)list[index].role, (object)list[index].weight));
							ShipClass shipClass = DesignLab.SuggestShipClassForNewDesign(this._db, this._player, list[index].role);
							WeaponRole wpnRole = DesignLab.SuggestWeaponRoleForNewDesign(aiInfo.Stance, list[index].role, shipClass);
							DesignInfo designInfo1 = (DesignInfo)null;
							if (list[index].role == ShipRole.COMBAT)
								designInfo1 = this.TryCounterDesign(shipClass);
							if (designInfo1 == null)
								designInfo1 = DesignLab.DesignShip(this._game, shipClass, list[index].role, wpnRole, this._player.ID, this._techStyles);
							if (designInfo1 != null)
							{
								DesignInfo designInfo2 = (DesignInfo)null;
								foreach (DesignInfo b in this._db.GetVisibleDesignInfosForPlayerAndRole(this._player.ID, designInfo1.Role, true))
								{
									if (StrategicAI.DoesDesignMatch(designInfo1, b))
									{
										designInfo2 = b;
										break;
									}
									if (b.WeaponRole == designInfo1.WeaponRole && b.GetMissionSectionAsset() == designInfo1.GetMissionSectionAsset())
									{
										if (App.Log.Level >= Kerberos.Sots.Engine.LogLevel.Verbose)
											StrategicAI.TraceVerbose(string.Format("    Removed Older Design: {0}", (object)b));
										this._game.GameDatabase.RemovePlayerDesign(b.ID);
									}
								}
								if (designInfo2 != null)
								{
									StrategicAI.TraceVerbose(string.Format("    REJECTED new design {0} because it is too similar to existing design {1}.", (object)designInfo1, (object)designInfo2));
								}
								else
								{
									this._db.InsertDesignByDesignInfo(designInfo1);
									StrategicAI.TraceVerbose(string.Format("    New design inserted: {0}", (object)designInfo1));
								}
							}
						}
						else
							num2 -= source1[index];
					}
				}
			}
		}

		public DesignInfo TryCounterDesign(ShipClass desiredClass)
		{
			StrategicAI.DesignConfigurationInfo enemyInfo = new StrategicAI.DesignConfigurationInfo();
			IEnumerable<CombatData> combatsForPlayer = this._game.CombatData.GetCombatsForPlayer(this._game.GameDatabase, this._player.ID, 10);
			int size = 0;
			if (combatsForPlayer.Count<CombatData>() > 0)
			{
				foreach (CombatData combatData in combatsForPlayer)
				{
					foreach (PlayerCombatData playerCombatData in combatData.GetPlayers().Where<PlayerCombatData>((Func<PlayerCombatData, bool>)(x => x.PlayerID != this._player.ID)))
					{
						Player playerObject = this._game.GetPlayerObject(playerCombatData.PlayerID);
						if (playerObject.IsStandardPlayer && this._db.GetDiplomacyStateBetweenPlayers(this._player.ID, playerObject.ID) == DiplomacyState.WAR)
						{
							foreach (ShipData shipData in playerCombatData.ShipData)
							{
								DesignInfo designInfo = this._db.GetDesignInfo(shipData.designID);
								enemyInfo += StrategicAI.GetDesignConfigurationInfo(this._game, designInfo);
								++size;
							}
						}
					}
				}
				if (size > 0)
				{
					enemyInfo.Average(size);
					return DesignLab.CreateCounterDesign(this._game, desiredClass, this._player.ID, enemyInfo);
				}
			}
			return (DesignInfo)null;
		}

		public static bool IsDesignObsolete(GameSession game, int playerID, int designID)
		{
			DesignInfo designInfo1 = game.GameDatabase.GetDesignInfo(designID);
			if (designInfo1.Class != ShipClass.Leviathan)
			{
				ShipSectionAsset shipSectionAsset = DesignLab.ChooseDriveSection(game, designInfo1.Class, playerID, (List<ShipSectionAsset>)null);
				ShipSectionAsset designSection = GameDatabase.GetDesignSection(game, designID, ShipSectionType.Engine);
				if (designSection != null && designSection.FileName != shipSectionAsset.FileName && designSection.BattleRiderType != BattleRiderTypes.battlerider || designSection != null && designSection.SectionName.Contains("eng_fusion") && shipSectionAsset.SectionName.Contains("eng_antimatter") || (designSection != null && designSection.SectionName.Contains("eng_antimatter") && shipSectionAsset.SectionName.Contains("eng_antimatter_enhanced") || designSection != null && designSection.SectionName.Contains("eng_reflex") && shipSectionAsset.SectionName.Contains("eng_reflex_enhanced")))
					return true;
			}
			ShipClass shipClass = DesignLab.SuggestShipClassForNewDesign(game.GameDatabase, game.GetPlayerObject(playerID), designInfo1.Role);
			foreach (DesignInfo designInfo2 in new List<DesignInfo>(game.GameDatabase.GetVisibleDesignInfosForPlayerAndRole(playerID, designInfo1.Role, true).Where<DesignInfo>((Func<DesignInfo, bool>)(x => !x.IsSuulka()))))
			{
				if (designInfo2.ID != designInfo1.ID && designInfo1.Class != shipClass && (designInfo2.Class == shipClass && designInfo2.DesignDate > designInfo1.DesignDate))
					return true;
			}
			return false;
		}

		public bool HandleRequestRequest(RequestInfo ri)
		{
			float num1 = (float)((double)this._db.GetDiplomacyInfo(ri.ReceivingPlayer, ri.InitiatingPlayer).Relations / 2000.0 * (double)this._game.GameDatabase.GetStratModifier<float>(StratModifiers.DiplomacyRequestWeight, ri.ReceivingPlayer) + ((this._db.GetPlayerFaction(ri.InitiatingPlayer).Name == "morrigi" ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") * 0.5 : 0.0) + (this._db.PlayerHasTech(ri.InitiatingPlayer, "PSI_Lesser_Glamour") ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") : 0.0)));
			bool flag = false;
			if (ri.Type == RequestType.SavingsRequest)
			{
				PlayerInfo playerInfo = this._db.GetPlayerInfo(this._player.ID);
				if (playerInfo.Savings > (double)ri.RequestValue)
				{
					float num2 = (float)playerInfo.Savings / ri.RequestValue;
					flag = this._random.CoinToss((double)num1 * (double)num2);
				}
			}
			else
				flag = this._random.CoinToss((double)num1);
			return flag;
		}

		public bool HandleDemandRequest(DemandInfo di)
		{
			float num1 = this.AssessOwnStrength();
			float num2 = this.AssessPlayerStrength(di.InitiatingPlayer) * 1.2f / num1;
			double num3 = (double)this._db.GetDiplomacyInfo(di.ReceivingPlayer, di.InitiatingPlayer).Relations / 2000.0;
			float stratModifier = this._game.GameDatabase.GetStratModifier<float>(StratModifiers.DiplomacyDemandWeight, di.ReceivingPlayer);
			double num4 = (double)num2;
			float num5 = (float)(num3 * num4 * (double)stratModifier + ((this._db.GetPlayerFaction(di.InitiatingPlayer).Name == "morrigi" ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") * 0.5 : 0.0) + (this._db.PlayerHasTech(di.InitiatingPlayer, "PSI_Lesser_Glamour") ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") : 0.0)));
			bool flag = false;
			switch (di.Type)
			{
				case DemandType.SavingsDemand:
					if (this._db.GetPlayerInfo(this._player.ID).Savings > (double)di.DemandValue)
					{
						flag = this._random.CoinToss((double)num5);
						break;
					}
					break;
				case DemandType.WorldDemand:
					if ((double)di.DemandValue != 0.0 && this._db.GetFleetsByPlayerAndSystem(di.ReceivingPlayer, (int)di.DemandValue, FleetType.FL_NORMAL).Count<FleetInfo>() == 0)
					{
						flag = this._random.CoinToss((double)num5);
						break;
					}
					break;
				default:
					flag = this._random.CoinToss((double)num5);
					break;
			}
			return flag;
		}

		public bool HandleTreatyOffer(TreatyInfo ti)
		{
			foreach (TreatyIncentiveInfo incentive in ti.Incentives)
				this._game.AcceptIncentive(ti.InitiatingPlayerId, ti.ReceivingPlayerId, incentive);
			DiplomacyInfo diplomacyInfo = this._db.GetDiplomacyInfo(ti.ReceivingPlayerId, ti.InitiatingPlayerId);
			if (ti.Type == TreatyType.Armistice)
			{
				bool flag = StrategicAI.GetDiplomacyStateRank(this.GetPreferredDiplomacyState(this._db.GetAIInfo(ti.InitiatingPlayerId), new StrategicAI.UpdatePlayerInfo(this._db, this._db.GetPlayerInfos(), this._db.GetPlayerInfo(ti.InitiatingPlayerId)), diplomacyInfo)) > StrategicAI.GetDiplomacyStateRank(diplomacyInfo.State);
				if (flag)
					return flag;
				float num1 = this.AssessOwnStrength();
				double num2 = (double)this.AssessPlayerStrength(ti.InitiatingPlayerId) * 1.20000004768372 / (double)num1;
				float num3 = (float)this._db.GetDiplomacyInfo(ti.ReceivingPlayerId, ti.InitiatingPlayerId).Relations / 2000f;
				float num4 = Math.Abs(this._game.GameDatabase.GetStratModifier<float>(StratModifiers.DiplomacyDemandWeight, ti.ReceivingPlayerId) - this._game.GameDatabase.GetStratModifier<float>(StratModifiers.DiplomacyRequestWeight, ti.ReceivingPlayerId));
				float num5 = (float)((this._db.GetPlayerFaction(ti.InitiatingPlayerId).Name == "morrigi" ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") * 0.5 : 0.0) + (this._db.PlayerHasTech(ti.InitiatingPlayerId, "PSI_Lesser_Glamour") ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") : 0.0));
				double num6 = (double)num3;
				return this._random.CoinToss(num2 * num6 * ((double)num4 + (double)num5));
			}
			if (ti.Type != TreatyType.Trade)
				return this._random.CoinToss(0.5);
			float num = (float)((double)this._db.GetDiplomacyInfo(ti.ReceivingPlayerId, ti.InitiatingPlayerId).Relations / 2000.0 + ((this._db.GetPlayerFaction(ti.InitiatingPlayerId).Name == "morrigi" ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") * 0.5 : 0.0) + (this._db.PlayerHasTech(ti.InitiatingPlayerId, "PSI_Lesser_Glamour") ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") : 0.0)));
			return this._random.CoinToss((double)(StrategicAI.GetDiplomacyStateRank(diplomacyInfo.State) - StrategicAI.GetDiplomacyStateRank(DiplomacyState.NEUTRAL)) * (double)num);
		}

		public void HandleGive(GiveInfo gi)
		{
			DiplomacyInfo diplomacyInfo = this._db.GetDiplomacyInfo(this._player.ID, gi.InitiatingPlayer);
			diplomacyInfo.Relations += (int)(Math.Sqrt(2.0 * ((double)gi.GiveValue / 200.0) + 1.0) - 1.0);
			int reactionAmount = (int)((double)diplomacyInfo.Relations * ((this._db.GetPlayerFaction(gi.InitiatingPlayer).Name == "morrigi" ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") * 0.5 : 0.0) + (this._db.PlayerHasTech(gi.InitiatingPlayer, "PSI_Lesser_Glamour") ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") : 0.0) + 1.0));
			diplomacyInfo.Relations += reactionAmount;
			this._db.ApplyDiplomacyReaction(this._player.ID, gi.InitiatingPlayer, reactionAmount, new StratModifiers?(), 1);
		}

		private void ResetData()
		{
			StrategicAI.TraceVerbose("Assigning missions...");
			this.m_AvailableFullFleets.Clear();
			this.m_AvailableShortFleets.Clear();
			this.m_DefenseCombatFleets.Clear();
			this.m_FleetsInSurveyRange.Clear();
			this.m_AvailableTasks.Clear();
			this.m_RelocationTasks.Clear();
			this.m_ColonizedSystems.Clear();
			this.m_FleetCubePoints.Clear();
			for (int index = 0; index < 8; ++index)
				this.m_NumStations[index] = 0;
		}

		private void GetNumRequiredDefenceFleets(
		  out int forCapitalSystem,
		  out int forOtherSystem,
		  AIStance stance)
		{
			forCapitalSystem = 0;
			forOtherSystem = 0;
			switch (stance)
			{
				case AIStance.EXPANDING:
					forCapitalSystem = 1;
					forOtherSystem = 0;
					break;
				case AIStance.ARMING:
					forCapitalSystem = 1;
					forOtherSystem = 0;
					break;
				case AIStance.HUNKERING:
					forCapitalSystem = 2;
					forOtherSystem = 0;
					break;
				case AIStance.CONQUERING:
					forCapitalSystem = 1;
					forOtherSystem = 0;
					break;
				case AIStance.DESTROYING:
					forCapitalSystem = 1;
					forOtherSystem = 0;
					break;
				case AIStance.DEFENDING:
					forCapitalSystem = 2;
					forOtherSystem = 0;
					break;
			}
		}

		private void GatherAllResources(AIStance stance)
		{
			this.m_TurnBudget = Budget.GenerateBudget(this._game, this._db.GetPlayerInfo(this._player.ID), (IEnumerable<DesignInfo>)null, BudgetProjection.Actual);
			this.m_GateCapacity = GameSession.GetTotalGateCapacity(this._game, this._player.ID);
			this.m_LoaLimit = Kerberos.Sots.StarFleet.StarFleet.GetMaxLoaFleetCubeMassForTransit(this._game, this._player.ID);
			List<FleetInfo> list1 = this._game.GameDatabase.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
			List<AIFleetInfo> list2 = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
			List<AIFleetInfo> list3 = list2.Where<AIFleetInfo>((Func<AIFleetInfo, bool>)(x => !x.AdmiralID.HasValue)).ToList<AIFleetInfo>();
			List<FleetInfo> list4 = list1.Where<FleetInfo>((Func<FleetInfo, bool>)(x => x.AdmiralID == 0)).ToList<FleetInfo>();
			foreach (AIFleetInfo aiFleetInfo in list3)
			{
				this._db.RemoveAIFleetInfo(aiFleetInfo.ID);
				list2.Remove(aiFleetInfo);
			}
			foreach (FleetInfo fleetInfo in list4)
			{
				FleetInfo fleet = fleetInfo;
				AIFleetInfo aiFleetInfo = list2.FirstOrDefault<AIFleetInfo>((Func<AIFleetInfo, bool>)(x =>
			   {
				   if (x.FleetID.HasValue)
					   return x.FleetID.Value == fleet.ID;
				   return false;
			   }));
				if (aiFleetInfo != null)
				{
					this._db.RemoveAIFleetInfo(aiFleetInfo.ID);
					list2.Remove(aiFleetInfo);
				}
				list1.Remove(fleet);
			}
			foreach (AIFleetInfo aiFleetInfo in list2)
			{
				AIFleetInfo aiFleet = aiFleetInfo;
				if (!aiFleet.InvoiceID.HasValue && !aiFleet.FleetID.HasValue)
				{
					FleetTemplate fleetTemplate = this._db.AssetDatabase.FleetTemplates.First<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == aiFleet.FleetTemplate));
					aiFleet.FleetID = new int?(this._db.InsertFleet(this._player.ID, aiFleet.AdmiralID.Value, aiFleet.SystemID, aiFleet.SystemID, fleetTemplate.Name, FleetType.FL_NORMAL));
					aiFleet.InvoiceID = new int?();
					this._db.UpdateAIFleetInfo(aiFleet);
					foreach (ShipInfo shipInfo in this._db.GetShipInfoByFleetID(this._db.InsertOrGetReserveFleetInfo(aiFleet.SystemID, this._player.ID).ID, false).Where<ShipInfo>((Func<ShipInfo, bool>)(x =>
				   {
					   int? aiFleetId = x.AIFleetID;
					   int id = aiFleet.ID;
					   return aiFleetId.GetValueOrDefault() == id & aiFleetId.HasValue;
				   })).ToList<ShipInfo>())
						this._db.TransferShip(shipInfo.ID, aiFleet.FleetID.Value);
					if (this._player.Faction.Name == "loa")
					{
						this._db.SaveCurrentFleetCompositionToFleet(aiFleet.FleetID.Value);
						Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this._game, aiFleet.FleetID.Value);
					}
				}
			}
			int forCapitalSystem = 0;
			int forOtherSystem = 0;
			this.GetNumRequiredDefenceFleets(out forCapitalSystem, out forOtherSystem, stance);
			int num1 = 0;
			if (this._player.PlayerInfo.Homeworld.HasValue)
			{
				OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(this._player.PlayerInfo.Homeworld.Value);
				if (orbitalObjectInfo != null)
					num1 = orbitalObjectInfo.StarSystemID;
			}
			Dictionary<SystemDefendInfo, int> source1 = new Dictionary<SystemDefendInfo, int>();
			using (IEnumerator<ColonyInfo> enumerator = this._db.GetColonyInfos().GetEnumerator())
			{
			label_51:
				while (enumerator.MoveNext())
				{
					ColonyInfo current = enumerator.Current;
					if (current.PlayerID == this._player.ID)
					{
						OrbitalObjectInfo orbit = this._db.GetOrbitalObjectInfo(current.OrbitalObjectID);
						if (!this.m_ColonizedSystems.Any<SystemDefendInfo>((Func<SystemDefendInfo, bool>)(x => x.SystemID == orbit.StarSystemID)))
						{
							StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(orbit.StarSystemID);
							SystemDefendInfo key1 = new SystemDefendInfo();
							key1.SystemID = orbit.StarSystemID;
							key1.IsHomeWorld = num1 == orbit.StarSystemID;
							key1.IsCapital = starSystemInfo != (StarSystemInfo)null && starSystemInfo.ProvinceID.HasValue && starSystemInfo.ID == this._db.GetProvinceCapitalSystemID(starSystemInfo.ProvinceID.Value);
							key1.NumColonies = this._db.GetColonyInfosForSystem(orbit.StarSystemID).Count<ColonyInfo>();
							key1.ProductionRate = 0.0f;
							key1.ConstructionRate = 0.0;
							double totalRevenue = 0.0;
							BuildScreenState.ObtainConstructionCosts(out key1.ProductionRate, out key1.ConstructionRate, out totalRevenue, this._game.App, orbit.StarSystemID, this._player.ID);
							this.m_ColonizedSystems.Add(key1);
							int num2 = forOtherSystem;
							if (key1.IsCapital || key1.IsHomeWorld)
								num2 = forCapitalSystem;
							if (num2 > 0)
							{
								List<FleetInfo> list5 = this._db.GetFleetsByPlayerAndSystem(this._player.ID, orbit.StarSystemID, FleetType.FL_NORMAL).ToList<FleetInfo>();
								List<FleetManagement> source2 = new List<FleetManagement>();
								foreach (FleetInfo fleetInfo in list5)
								{
									FleetInfo fleet2 = fleetInfo;
									if (this._db.GetMissionByFleetID(fleet2.ID) == null)
									{
										AIFleetInfo aiFleetInfo = list2.FirstOrDefault<AIFleetInfo>((Func<AIFleetInfo, bool>)(x =>
									   {
										   int? fleetId = x.FleetID;
										   int id = fleet2.ID;
										   return fleetId.GetValueOrDefault() == id & fleetId.HasValue;
									   }));
										if (aiFleetInfo != null && (aiFleetInfo.FleetTemplate == "DEFAULT_PATROL" || aiFleetInfo.FleetTemplate == "DEFAULT_COMBAT"))
											source2.Add(new FleetManagement()
											{
												Fleet = fleet2,
												FleetStrength = this.GetFleetStrength(fleet2)
											});
									}
								}
								if (source2.Count > 0 || num2 > 0)
								{
									source2 = source2.OrderBy<FleetManagement, int>((Func<FleetManagement, int>)(x => -1 * x.FleetStrength)).ToList<FleetManagement>();
									for (int index = 0; index < num2; ++index)
									{
										FleetManagement fleetManagement = source2.FirstOrDefault<FleetManagement>();
										if (fleetManagement == null)
										{
											source1.Add(key1, num2 - index);
											break;
										}
										this.m_DefenseCombatFleets.Add(fleetManagement.Fleet);
										source2.Remove(fleetManagement);
									}
								}
								while (true)
								{
									if (source2.Count > 0 && source1.Count > 0)
									{
										FleetManagement fleetManagement = source2.FirstOrDefault<FleetManagement>();
										SystemDefendInfo key2 = source1.FirstOrDefault<KeyValuePair<SystemDefendInfo, int>>().Key;
										this.m_DefenseCombatFleets.Add(fleetManagement.Fleet);
										source2.Remove(fleetManagement);
										source1[key2]--;
										if (source1[key2] == 0)
											source1.Remove(key2);
										this.AssignFleetToTask(new StrategicTask()
										{
											Mission = MissionType.RELOCATION,
											SystemIDTarget = key2.SystemID
										}, fleetManagement.Fleet, new int?());
									}
									else
										goto label_51;
								}
							}
						}
					}
				}
			}
			if (this.m_ColonizedSystems.Count > 0)
				this.m_ColonizedSystems.Sort((IComparer<SystemDefendInfo>)new SystemDefendInfoComparision());
			foreach (FleetInfo fleetInfo in this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).ToList<FleetInfo>())
			{
				if (fleetInfo.AdmiralID != 0)
				{
					MissionInfo missionByFleetId = this._game.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
					if (this.Player.Faction.Name == "loa")
					{
						this.TransferCubesFromReserve(fleetInfo);
						this.m_FleetCubePoints.Add(fleetInfo.ID, Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._game, fleetInfo.ID));
					}
					this.FixFullFleet(fleetInfo);
					if (missionByFleetId != null && missionByFleetId.Type != MissionType.RETURN)
					{
						if (this.FleetRequiresFill(fleetInfo))
						{
							Kerberos.Sots.StarFleet.StarFleet.CancelMission(this._game, fleetInfo, true);
							this.m_AvailableShortFleets.Add(fleetInfo);
						}
					}
					else if (Kerberos.Sots.StarFleet.StarFleet.GetFleetCommandPoints(this._game.App, fleetInfo.ID, (IEnumerable<int>)null) != 0 || this.Player.Faction.Name == "loa")
					{
						if (missionByFleetId != null || this.FillFleet(fleetInfo))
							this.m_AvailableFullFleets.Add(fleetInfo);
						else
							this.m_AvailableShortFleets.Add(fleetInfo);
					}
				}
			}
			foreach (StationInfo stationInfo in this._db.GetStationInfosByPlayerID(this._player.ID))
				++this.m_NumStations[(int)stationInfo.DesignInfo.StationType];
			if (!this._player.Faction.CanUseGate() && !this._player.Faction.CanUseAccelerators())
				return;
			foreach (MissionInfo missionInfo in this._db.GetMissionInfos().Where<MissionInfo>((Func<MissionInfo, bool>)(x =>
		   {
			   if (x.Type != MissionType.COLONIZATION && x.Type != MissionType.SUPPORT)
				   return x.Type == MissionType.CONSTRUCT_STN;
			   return true;
		   })).ToList<MissionInfo>())
			{
				FleetInfo fleetInfo = this._db.GetFleetInfo(missionInfo.FleetID);
				if (fleetInfo != null && fleetInfo.PlayerID == this._player.ID && fleetInfo.SupportingSystemID != missionInfo.TargetSystemID && (this._player.Faction.CanUseGate() && !this._db.SystemHasGate(missionInfo.TargetSystemID, this._player.ID) || this._player.Faction.CanUseAccelerators() && !this._db.SystemHasAccelerator(missionInfo.TargetSystemID, this._player.ID)))
					Kerberos.Sots.StarFleet.StarFleet.CancelMission(this._game, fleetInfo, true);
			}
		}

		private void GatherAllTasks()
		{
			this.m_FleetRanges.Clear();
			foreach (FleetInfo fleetInfo in this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).Where<FleetInfo>((Func<FleetInfo, bool>)(x => x.SystemID != 0)).ToList<FleetInfo>())
			{
				FleetRangeData fleetRangeData = new FleetRangeData();
				fleetRangeData.FleetRange = Kerberos.Sots.StarFleet.StarFleet.GetFleetRange(this._game, fleetInfo);
				IEnumerable<ShipInfo> shipInfoByFleetId = this._db.GetShipInfoByFleetID(fleetInfo.ID, false);
				fleetRangeData.FleetTravelSpeed = new float?(Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this._game, fleetInfo.ID, shipInfoByFleetId, false));
				fleetRangeData.FleetNodeTravelSpeed = new float?(Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this._game, fleetInfo.ID, shipInfoByFleetId, true));
				if ((double)fleetRangeData.FleetRange > 0.0)
					this.m_FleetRanges.Add(fleetInfo, fleetRangeData);
			}
			foreach (StarSystemInfo system in this._game.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>())
			{
				this.m_FleetsInSurveyRange = Kerberos.Sots.StarFleet.StarFleet.GetFleetsInRangeOfSystem(this._game, system.ID, this.m_FleetRanges, 1f);
				this.m_FleetsInSurveyRange.RemoveAll((Predicate<FleetInfo>)(x => !this.m_AvailableFullFleets.Contains(x)));
				if (this.m_FleetsInSurveyRange.Count != 0)
				{
					List<StrategicTask> tasksForSystem = this.GetTasksForSystem(system, this.m_FleetRanges, this._game.isHostilesAtSystem(this._player.ID, system.ID));
					if (tasksForSystem.Count > 0)
						this.m_AvailableTasks.AddRange((IEnumerable<StrategicTask>)tasksForSystem);
					StrategicTask relocateTaskForSystem = this.GetPotentialRelocateTaskForSystem(system, this.m_FleetRanges, false);
					if (relocateTaskForSystem != null)
						this.m_RelocationTasks.Add(relocateTaskForSystem, new List<StrategicTask>());
				}
			}
			this.m_FleetsInSurveyRange.Clear();
		}

		private void ApplyScores(AIStance stance)
		{
			foreach (StrategicTask availableTask in this.m_AvailableTasks)
			{
				availableTask.Score = this.GetScoreForTask(availableTask, stance);
				availableTask.NumFleetsRequested = this.GetNumFleetsRequestForTask(availableTask);
				if (availableTask.Score != 0 && availableTask.NumFleetsRequested != 0)
				{
					foreach (FleetManagement useableFleet in availableTask.UseableFleets)
						useableFleet.Score = this.GetScoreForFleet(availableTask, useableFleet);
					availableTask.UseableFleets.RemoveAll((Predicate<FleetManagement>)(x => x.Score == 0));
					availableTask.UseableFleets.OrderByDescending<FleetManagement, int>((Func<FleetManagement, int>)(x => x.Score));
					availableTask.SubScore = this.GetSubScoreForTask(availableTask, stance);
				}
			}
			float supportRange = GameSession.GetSupportRange(this._db.AssetDatabase, this._db, this._player.ID);
			float num = supportRange * supportRange;
			List<StrategicTask> strategicTaskList = new List<StrategicTask>();
			foreach (KeyValuePair<StrategicTask, List<StrategicTask>> relocationTask in this.m_RelocationTasks)
			{
				Vector3 starSystemOrigin = this._db.GetStarSystemOrigin(relocationTask.Key.SystemIDTarget);
				foreach (StrategicTask availableTask in this.m_AvailableTasks)
				{
					if ((double)(this._db.GetStarSystemOrigin(availableTask.SystemIDTarget) - starSystemOrigin).LengthSquared < (double)num)
						relocationTask.Value.Add(availableTask);
				}
				if (relocationTask.Value.Count == 0 || relocationTask.Key.UseableFleets.Count == 0)
					strategicTaskList.Add(relocationTask.Key);
			}
			foreach (StrategicTask key in strategicTaskList)
				this.m_RelocationTasks.Remove(key);
			this.m_AvailableTasks.RemoveAll((Predicate<StrategicTask>)(x => x.Score == 0));
			this.m_AvailableTasks.Sort((IComparer<StrategicTask>)new StrategicTaskComparision());
		}

		private bool MustRelocateCloser(StrategicTask task, FleetManagement fleet)
		{
			int num = 10;
			switch (task.Mission)
			{
				case MissionType.SURVEY:
					if (this._player.Faction.CanUseNodeLine(new bool?(true)))
						return false;
					break;
				case MissionType.GATE:
				case MissionType.DEPLOY_NPG:
					num *= 4;
					break;
			}
			return fleet.MissionTime.TurnsToTarget > num;
		}

		private void AssignFleetsToTasks(AIStance stance)
		{
			List<int> fleetsUsed = new List<int>();
			foreach (FleetInfo availableFullFleet in this.m_AvailableFullFleets)
			{
				FleetInfo fleet = availableFullFleet;
				StrategicTask strategicTask1 = this.PickBestTaskForFleet(fleet, fleetsUsed);
				if (strategicTask1 != null)
				{
					int? rebaseTarget = new int?();
					StrategicTask strategicTask2 = this._db.GetMissionByFleetID(fleet.ID) == null ? this.PickBestRelocationTaskForFleet(fleet, strategicTask1, false) : (StrategicTask)null;
					FleetManagement fleet1 = strategicTask1.UseableFleets.First<FleetManagement>((Func<FleetManagement, bool>)(x => x.Fleet == fleet));
					if (strategicTask2 != null || this.MustRelocateCloser(strategicTask1, fleet1))
					{
						if (strategicTask2 != null && strategicTask2.SystemIDTarget == strategicTask1.SystemIDTarget)
							rebaseTarget = new int?(strategicTask2.SystemIDTarget);
						else
							strategicTask1 = strategicTask2;
					}
					if (strategicTask1 != null)
					{
						this.AssignFleetToTask(strategicTask1, fleet, rebaseTarget);
						--strategicTask1.NumFleetsRequested;
						fleetsUsed.Add(fleet.ID);
					}
				}
			}
			for (int index = 0; index < this.m_AvailableTasks.Count; ++index)
			{
				StrategicTask availableTask = this.m_AvailableTasks[index];
				if (availableTask.NumFleetsRequested > 0)
				{
					foreach (FleetManagement useableFleet in availableTask.UseableFleets)
					{
						StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(useableFleet.Fleet.SystemID);
						if (!(starSystemInfo == (StarSystemInfo)null) && !starSystemInfo.IsDeepSpace)
						{
							if (!fleetsUsed.Contains(useableFleet.Fleet.ID) && !this.m_AvailableShortFleets.Contains(useableFleet.Fleet) && !this.m_DefenseCombatFleets.Contains(useableFleet.Fleet))
							{
								StrategicTask task = this.PickBestRelocationTaskForFleet(useableFleet.Fleet, availableTask, false);
								if (task != null || this.MustRelocateCloser(availableTask, useableFleet))
								{
									if (task != null)
									{
										this.AssignFleetToTask(task, useableFleet.Fleet, new int?());
										--availableTask.NumFleetsRequested;
										fleetsUsed.Add(useableFleet.Fleet.ID);
										continue;
									}
									continue;
								}
								this.AssignFleetToTask(availableTask, useableFleet.Fleet, new int?());
								--availableTask.NumFleetsRequested;
								fleetsUsed.Add(useableFleet.Fleet.ID);
							}
							if (availableTask.NumFleetsRequested <= 0)
								break;
						}
					}
				}
			}
			foreach (FleetInfo fleet in this.m_AvailableFullFleets.Where<FleetInfo>((Func<FleetInfo, bool>)(x => !fleetsUsed.Contains(x.ID))).ToList<FleetInfo>())
			{
				StrategicTask task = this.PickBestRelocationTaskForFleet(fleet, (StrategicTask)null, false);
				if (task != null)
				{
					this.AssignFleetToTask(task, fleet, new int?());
					fleetsUsed.Add(fleet.ID);
				}
			}
			this.AssignDefenseFleetsToSystem();
		}

		private void BuildFleets(AIInfo aiInfo)
		{
			List<AIFleetInfo> myAIFleets = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
			List<int> list = new List<int>();
			using (List<FleetInfo>.Enumerator enumerator = this.m_AvailableShortFleets.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					FleetInfo fleet = enumerator.Current;
					if (!list.Contains(fleet.ID) && !this.FillFleet(fleet))
					{
						List<FleetInfo> list2 = (from x in this.m_AvailableShortFleets
												 where x.SystemID == fleet.SystemID
												 select x).ToList<FleetInfo>();
						list.AddRange((from x in list2
									   select x.ID).ToList<int>());
						if (!this.m_ColonizedSystems.Any((SystemDefendInfo x) => x.SystemID == fleet.SystemID) || !this.FillFleetsWithBuild(fleet.SystemID, list2, aiInfo.Stance))
						{
							StrategicTask strategicTask = this.PickBestRelocationTaskForFleet(fleet, null, true);
							if (strategicTask != null)
							{
								this.AssignFleetToTask(strategicTask, fleet, null);
							}
						}
					}
				}
			}
			double num = this.GetAvailableShipConstructionBudget(aiInfo.Stance);
			double num2 = this.GetAvailableFleetSupportBudget();
			List<SystemBuildInfo> list3 = new List<SystemBuildInfo>();
			foreach (SystemDefendInfo systemDefendInfo in this.m_ColonizedSystems)
			{
				if (systemDefendInfo.ConstructionRate > 0.0 && systemDefendInfo.ProductionRate > 1f)
				{
					list3.Add(new SystemBuildInfo
					{
						SystemID = systemDefendInfo.SystemID,
						SystemOrigin = this._db.GetStarSystemOrigin(systemDefendInfo.SystemID),
						AvailableSupportPoints = this._db.GetRemainingSupportPoints(this._game, systemDefendInfo.SystemID, this._player.ID),
						ProductionPerTurn = systemDefendInfo.ProductionRate,
						RemainingBuildTime = this.GetRemainingAIBuildTime(systemDefendInfo.SystemID, systemDefendInfo.ProductionRate)
					});
				}
			}
			list3.RemoveAll((SystemBuildInfo x) => x.RemainingBuildTime < this.MAX_BUILD_TURNS);
			bool flag = false;
			if (list3.Count > 0)
			{
				int num3 = 0;
				int num4 = 0;
				this.GetNumRequiredDefenceFleets(out num3, out num4, aiInfo.Stance);
				int num5 = 0;
				if (this._player.PlayerInfo.Homeworld != null)
				{
					OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(this._player.PlayerInfo.Homeworld.Value);
					if (orbitalObjectInfo != null)
					{
						num5 = orbitalObjectInfo.StarSystemID;
					}
				}
				List<SystemBuildInfo> list4 = new List<SystemBuildInfo>();
				using (List<SystemBuildInfo>.Enumerator enumerator3 = list3.GetEnumerator())
				{
					//Func<BuildScreenState.InvoiceItem, int> <> 9__8;
					while (enumerator3.MoveNext())
					{
						SystemBuildInfo build = enumerator3.Current;
						int num6 = num4;
						StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(build.SystemID);
						if ((starSystemInfo != null && starSystemInfo.ProvinceID != null && starSystemInfo.ID == this._db.GetProvinceCapitalSystemID(starSystemInfo.ProvinceID.Value)) || num5 == build.SystemID)
						{
							num6 = num3;
						}
						int num7 = this.m_DefenseCombatFleets.Count((FleetInfo x) => x.SystemID == build.SystemID);
						num6 -= num7;
						if (num6 > 0 && this.m_CombatFleetTemplate != null)
						{
							IEnumerable<AdmiralInfo> source = this._db.GetAdmiralInfosForPlayer(this._player.ID).ToList<AdmiralInfo>();
							List<FleetInfo> fleetInfos = this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
							AdmiralInfo admiralInfo = source.FirstOrDefault((AdmiralInfo x) => !fleetInfos.Any((FleetInfo y) => y.AdmiralID == x.ID) && !myAIFleets.Any(delegate (AIFleetInfo y)
							{
								int? admiralID = y.AdmiralID;
								int id = x.ID;
								return admiralID.GetValueOrDefault() == id & admiralID != null;
							}));
							StarSystemInfo starSystemInfo2 = this._db.GetStarSystemInfo(build.SystemID);
							if (admiralInfo != null && starSystemInfo2 != null)
							{
								double num8 = (num5 == build.SystemID) ? 1.75 : 1.5;
								List<BuildScreenState.InvoiceItem> buildFleetInvoice = this.GetBuildFleetInvoice(this.m_CombatFleetTemplate, aiInfo.Stance);
								int buildTime = BuildScreenState.GetBuildTime(this._game.App, buildFleetInvoice, build.ProductionPerTurn);
								if (buildTime != 0)
								{
									List<BuildScreenState.InvoiceItem> invoiceAfterReserves = this.GetInvoiceAfterInculdingReserve(this.GetUnclaimedShipsInReserve(build.SystemID), buildFleetInvoice);
									int supportRequirementsForInvoice = this.GetSupportRequirementsForInvoice(buildFleetInvoice);
									int num9 = BuildScreenState.GetBuildInvoiceCost(this._game.App, invoiceAfterReserves) / buildTime;
									IEnumerable<BuildScreenState.InvoiceItem> source2 = buildFleetInvoice;
									//Func<BuildScreenState.InvoiceItem, int> selector;
									//if ((selector = <> 9__8) == null)
									//{
									//	selector = (<> 9__8 = ((BuildScreenState.InvoiceItem x) => GameSession.CalculateUpkeepCost(x.DesignID, this._game.App)));
									//}
									int num10 = source2.Sum((BuildScreenState.InvoiceItem x) => GameSession.CalculateUpkeepCost(x.DesignID, this._game.App));
									if (supportRequirementsForInvoice <= build.AvailableSupportPoints && (double)num10 < num2 * num8)
									{
										if (!flag && (double)num9 < num * num8)
										{
											this.BuildFleet(aiInfo, admiralInfo, starSystemInfo2, this.m_CombatFleetTemplate, buildFleetInvoice, true);
											list4.Add(build);
											num -= (double)num9;
											num2 -= (double)num10;
											flag = true;
										}
										else if ((float)invoiceAfterReserves.Count / (float)buildFleetInvoice.Count < 0.5f)
										{
											this.BuildFleet(aiInfo, admiralInfo, starSystemInfo2, this.m_CombatFleetTemplate, (from x in buildFleetInvoice
																															   where !invoiceAfterReserves.Contains(x) || this._db.GetDesignInfo(x.DesignID).Role == ShipRole.COMMAND
																															   select x).ToList<BuildScreenState.InvoiceItem>(), true);
										}
									}
								}
							}
						}
					}
				}
				foreach (SystemBuildInfo item in list4)
				{
					list3.Remove(item);
				}
				if (list3.Count > 0)
				{
					myAIFleets = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
					List<AIFleetInfo> list5 = (from x in myAIFleets
											   where !this.m_DefenseCombatFleets.Any(delegate (FleetInfo y)
											   {
												   int id = y.ID;
												   int? fleetID = x.FleetID;
												   return id == fleetID.GetValueOrDefault() & fleetID != null;
											   })
											   select x).ToList<AIFleetInfo>();
					List<FleetTemplate> list6 = (from x in this._game.AssetDatabase.FleetTemplates
												 where x.CanFactionUse(this._player.Faction.Name) && x.MinFleetsForStance[aiInfo.Stance] > 0
												 select x).ToList<FleetTemplate>();
					list6.Sort(new FleetTemplateComparision(aiInfo.Stance, list5));
					bool flag2 = true;
					Dictionary<string, int> dictionary = new Dictionary<string, int>();
					using (List<FleetTemplate>.Enumerator enumerator4 = list6.GetEnumerator())
					{
						while (enumerator4.MoveNext())
						{
							FleetTemplate template = enumerator4.Current;
							Dictionary<string, int> dictionary2 = dictionary;
							string name = template.Name;
							List<AIFleetInfo> source3 = list5;
							Func<AIFleetInfo, bool> predicate = (AIFleetInfo x) => x.FleetTemplate == template.Name;
							dictionary2.Add(name, source3.Count(predicate));
							if (template.MinFleetsForStance[aiInfo.Stance] - dictionary[template.Name] > 0)
							{
								flag2 = false;
							}
						}
					}
					bool flag3 = false;
					using (List<FleetTemplate>.Enumerator enumerator4 = list6.GetEnumerator())
					{
						//Func<BuildScreenState.InvoiceItem, int> <> 9__20;
						while (enumerator4.MoveNext())
						{
							FleetTemplate template2 = enumerator4.Current;
							IEnumerable<StrategicTask> availableTasks = this.m_AvailableTasks;
							Func<StrategicTask, bool> predicate2 = (StrategicTask x) => template2.MissionTypes.Contains(x.Mission);
							StrategicTask strategicTask2 = availableTasks.FirstOrDefault(predicate2);
							if ((flag2 && strategicTask2 != null) || template2.MinFleetsForStance[aiInfo.Stance] - dictionary[template2.Name] > 0)
							{
								bool flag4 = FleetTemplateComparision.MustHaveTemplate(template2);
								if (flag3 && !flag4)
								{
									break;
								}
								List<BuildScreenState.InvoiceItem> buildFleetInvoice2 = this.GetBuildFleetInvoice(template2, aiInfo.Stance);
								int supportPointsRequired = this.GetSupportRequirementsForInvoice(buildFleetInvoice2);
								if (strategicTask2 != null)
								{
									Vector3 targetOrigin = this._db.GetStarSystemOrigin(strategicTask2.SystemIDTarget);
									list3 = (from x in list3
											 orderby (x.SystemOrigin - targetOrigin).LengthSquared
											 select x).ToList<SystemBuildInfo>();
								}
								SystemBuildInfo systemBuildInfo = list3.FirstOrDefault((SystemBuildInfo x) => x.AvailableSupportPoints >= supportPointsRequired);
								if (systemBuildInfo != null)
								{
									IEnumerable<AdmiralInfo> source4 = this._db.GetAdmiralInfosForPlayer(this._player.ID).ToList<AdmiralInfo>();
									List<FleetInfo> fleetInfos2 = this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
									AdmiralInfo admiralInfo2 = source4.FirstOrDefault((AdmiralInfo x) => !fleetInfos2.Any((FleetInfo y) => y.AdmiralID == x.ID) && !myAIFleets.Any(delegate (AIFleetInfo y)
									{
										int? admiralID = y.AdmiralID;
										int id = x.ID;
										return admiralID.GetValueOrDefault() == id & admiralID != null;
									}));
									StarSystemInfo starSystemInfo3 = this._db.GetStarSystemInfo(systemBuildInfo.SystemID);
									if (admiralInfo2 != null && starSystemInfo3 != null)
									{
										double num11 = flag4 ? 1.25 : 1.0;
										int buildTime2 = BuildScreenState.GetBuildTime(this._game.App, buildFleetInvoice2, systemBuildInfo.ProductionPerTurn);
										if (buildTime2 == 0)
										{
											continue;
										}
										List<BuildScreenState.InvoiceItem> invoiceAfterReserves2 = this.GetInvoiceAfterInculdingReserve(this.GetUnclaimedShipsInReserve(systemBuildInfo.SystemID), buildFleetInvoice2);
										int num12 = BuildScreenState.GetBuildInvoiceCost(this._game.App, invoiceAfterReserves2) / buildTime2;
										IEnumerable<BuildScreenState.InvoiceItem> source5 = buildFleetInvoice2;
										//Func<BuildScreenState.InvoiceItem, int> selector2;
										//if ((selector2 = <> 9__20) == null)
										//{
										//	selector2 = (<> 9__20 = ((BuildScreenState.InvoiceItem x) => GameSession.CalculateUpkeepCost(x.DesignID, this._game.App)));
										//}
										int num13 = source5.Sum((BuildScreenState.InvoiceItem x) => GameSession.CalculateUpkeepCost(x.DesignID, this._game.App));
										if ((double)num13 < num2 * num11)
										{
											if (!flag && supportPointsRequired <= systemBuildInfo.AvailableSupportPoints && (double)num12 < num * num11)
											{
												this.BuildFleet(aiInfo, admiralInfo2, starSystemInfo3, template2, buildFleetInvoice2, true);
												list3.Remove(systemBuildInfo);
												num -= (double)num12;
												num2 -= (double)num13;
												Dictionary<string, int> dictionary3;
												string name2;
												(dictionary3 = dictionary)[name2 = template2.Name] = dictionary3[name2] + 1;
												flag = true;
											}
											else if ((float)invoiceAfterReserves2.Count / (float)buildFleetInvoice2.Count < 0.5f)
											{
												this.BuildFleet(aiInfo, admiralInfo2, starSystemInfo3, template2, (from x in buildFleetInvoice2
																												   where !invoiceAfterReserves2.Contains(x) || this._db.GetDesignInfo(x.DesignID).Role == ShipRole.COMMAND
																												   select x).ToList<BuildScreenState.InvoiceItem>(), true);
												Dictionary<string, int> dictionary4;
												string name3;
												(dictionary4 = dictionary)[name3 = template2.Name] = dictionary4[name3] + 1;
											}
										}
										if (flag4 && dictionary[template2.Name] == 0)
										{
											flag3 = true;
										}
									}
									else if (!flag2)
									{
										break;
									}
									if (list3.Count == 0)
									{
										break;
									}
								}
							}
						}
					}
					bool flag5 = true;
					//Func<AIFleetInfo, bool> <> 9__23;
					foreach (SystemBuildInfo systemBuildInfo2 in list3)
					{
						SystemBuildInfo systemBuildInfo3 = list3.First<SystemBuildInfo>();
						IEnumerable<AdmiralInfo> source6 = this._db.GetAdmiralInfosForPlayer(this._player.ID).ToList<AdmiralInfo>();
						List<FleetInfo> fleetInfos3 = this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
						AdmiralInfo admiralInfo3 = source6.FirstOrDefault((AdmiralInfo x) => !fleetInfos3.Any((FleetInfo y) => y.AdmiralID == x.ID) && !myAIFleets.Any(delegate (AIFleetInfo y)
						{
							int? admiralID = y.AdmiralID;
							int id = x.ID;
							return admiralID.GetValueOrDefault() == id & admiralID != null;
						}));
						StarSystemInfo starSystemInfo4 = this._db.GetStarSystemInfo(systemBuildInfo3.SystemID);
						if (admiralInfo3 != null && starSystemInfo4 != null)
						{
							if (flag5)
							{
								List<FleetTemplate> list7 = list6;
								AIStance stance = aiInfo.Stance;
								IEnumerable<AIFleetInfo> aifleetInfos = this._db.GetAIFleetInfos(this._player.ID);
								/*Func<AIFleetInfo, bool> predicate3;
								if ((predicate3 = <> 9__23) == null)
								{
									predicate3 = (<> 9__23 = ((AIFleetInfo x) => !this.m_DefenseCombatFleets.Any(delegate (FleetInfo y)
									{
										int id = y.ID;
										int? fleetID = x.FleetID;
										return id == fleetID.GetValueOrDefault() & fleetID != null;
									})));
								}*/
								list7.Sort(new FleetTemplateComparision(stance, aifleetInfos.Where((AIFleetInfo x) => x.FleetID != null && !this.m_DefenseCombatFleets.Any(y => y.ID == x.FleetID.Value)).ToList<AIFleetInfo>()));
								flag5 = false;
							}
							foreach (FleetTemplate fleetTemplate in list6)
							{
								List<BuildScreenState.InvoiceItem> list8 = this.GetBuildFleetInvoice(fleetTemplate, aiInfo.Stance);
								if (BuildScreenState.GetBuildTime(this._game.App, list8, systemBuildInfo3.ProductionPerTurn) != 0)
								{
									List<BuildScreenState.InvoiceItem> invoiceAfterReserves3 = this.GetInvoiceAfterInculdingReserve(this.GetUnclaimedShipsInReserve(systemBuildInfo3.SystemID), list8);
									if ((float)invoiceAfterReserves3.Count / (float)list8.Count < 0.5f)
									{
										list8 = (from x in list8
												 where !invoiceAfterReserves3.Contains(x) || this._db.GetDesignInfo(x.DesignID).Role == ShipRole.COMMAND
												 select x).ToList<BuildScreenState.InvoiceItem>();
										if (this.GetSupportRequirementsForInvoice(list8) <= systemBuildInfo3.AvailableSupportPoints)
										{
											this.BuildFleet(aiInfo, admiralInfo3, starSystemInfo4, fleetTemplate, list8, true);
											flag5 = true;
										}
									}
								}
							}
						}
					}
				}
			}
		}

		private List<FleetTemplate> GetRequiredFleetTemplatesForStance(AIStance stance)
		{
			List<FleetTemplate> list = this._game.AssetDatabase.FleetTemplates.Where<FleetTemplate>((Func<FleetTemplate, bool>)(x =>
		   {
			   if (x.StanceWeights.ContainsKey(stance))
				   return !x.Initial;
			   return false;
		   })).ToList<FleetTemplate>();
			list.OrderByDescending<FleetTemplate, int>((Func<FleetTemplate, int>)(x => x.StanceWeights[stance]));
			return list;
		}

		private FleetTemplate GetBestFleetTemplate(
		  AIStance stance,
		  MissionType missionType)
		{
			List<FleetTemplate> list1 = this._game.AssetDatabase.FleetTemplates.Where<FleetTemplate>((Func<FleetTemplate, bool>)(x =>
		   {
			   if (x.MissionTypes.Contains(missionType))
				   return !x.Initial;
			   return false;
		   })).ToList<FleetTemplate>();
			if (list1.Count == 0)
				return (FleetTemplate)null;
			List<FleetTemplate> list2 = list1.Where<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.StanceWeights.ContainsKey(stance))).ToList<FleetTemplate>();
			List<Weighted<FleetTemplate>> list3 = list2.Select<FleetTemplate, Weighted<FleetTemplate>>((Func<FleetTemplate, Weighted<FleetTemplate>>)(x => new Weighted<FleetTemplate>(x, x.StanceWeights[stance]))).ToList<Weighted<FleetTemplate>>();
			if (list3.Count <= 0 || list2.Count <= 0)
				return list1.First<FleetTemplate>();
			return WeightedChoices.Choose<FleetTemplate>(this._random, (IEnumerable<Weighted<FleetTemplate>>)list3);
		}

		private void AssignFleetToTask(StrategicTask task, FleetInfo fleet, int? rebaseTarget = null)
		{
			MissionInfo missionByFleetId = this._db.GetMissionByFleetID(fleet.ID);
			if (missionByFleetId != null)
				this._db.RemoveMission(missionByFleetId.ID);
			if (this._player.Faction.CanUseAccelerators())
				this._game.CheckLoaFleetGateCompliancy(fleet);
			bool useDirectRoute = this._player.Faction.CanUseAccelerators();
			switch (task.Mission)
			{
				case MissionType.COLONIZATION:
					Kerberos.Sots.StarFleet.StarFleet.SetColonizationMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, task.PlanetIDTarget, (List<int>)null, rebaseTarget);
					break;
				case MissionType.SUPPORT:
					Kerberos.Sots.StarFleet.StarFleet.SetSupportMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, task.PlanetIDTarget, (List<int>)null, this._game.GetNumSupportTrips(fleet.ID, task.SystemIDTarget), rebaseTarget);
					break;
				case MissionType.SURVEY:
					Kerberos.Sots.StarFleet.StarFleet.SetSurveyMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, (List<int>)null, rebaseTarget);
					break;
				case MissionType.RELOCATION:
					Kerberos.Sots.StarFleet.StarFleet.SetRelocationMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, (List<int>)null);
					break;
				case MissionType.CONSTRUCT_STN:
					Kerberos.Sots.StarFleet.StarFleet.SetConstructionMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, task.PlanetIDTarget, (List<int>)null, task.StationType, rebaseTarget);
					using (List<StrategicTask>.Enumerator enumerator = this.m_AvailableTasks.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							StrategicTask current = enumerator.Current;
							if (current != task && (current.Mission == MissionType.CONSTRUCT_STN || current.Mission == MissionType.UPGRADE_STN))
								current.NumFleetsRequested = 0;
						}
						break;
					}
				case MissionType.UPGRADE_STN:
					Kerberos.Sots.StarFleet.StarFleet.SetUpgradeStationMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, task.StationIDTarget, (List<int>)null, task.StationType, rebaseTarget);
					using (List<StrategicTask>.Enumerator enumerator = this.m_AvailableTasks.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							StrategicTask current = enumerator.Current;
							if (current != task && (current.Mission == MissionType.CONSTRUCT_STN || current.Mission == MissionType.UPGRADE_STN))
								current.NumFleetsRequested = 0;
						}
						break;
					}
				case MissionType.PATROL:
					Kerberos.Sots.StarFleet.StarFleet.SetPatrolMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, (List<int>)null, rebaseTarget);
					break;
				case MissionType.INTERDICTION:
					Kerberos.Sots.StarFleet.StarFleet.SetInterdictionMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, 0, (List<int>)null);
					break;
				case MissionType.STRIKE:
					Kerberos.Sots.StarFleet.StarFleet.SetStrikeMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, task.PlanetIDTarget, task.FleetIDTarget, (List<int>)null);
					break;
				case MissionType.INVASION:
					Kerberos.Sots.StarFleet.StarFleet.SetInvasionMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, task.PlanetIDTarget, (List<int>)null);
					break;
				case MissionType.INTERCEPT:
					Kerberos.Sots.StarFleet.StarFleet.SetFleetInterceptMission(this._game, fleet.ID, task.FleetIDTarget, useDirectRoute, (List<int>)null);
					break;
				case MissionType.GATE:
					Kerberos.Sots.StarFleet.StarFleet.SetGateMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, (List<int>)null, rebaseTarget);
					break;
				case MissionType.DEPLOY_NPG:
					Kerberos.Sots.StarFleet.StarFleet.SetNPGMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, Kerberos.Sots.StarFleet.StarFleet.GetAccelGatePercentPointsBetweenSystems(this._db, fleet.SystemID, task.SystemIDTarget).ToList<int>(), (List<int>)null, rebaseTarget);
					break;
			}
		}

		private List<StrategicTask> GetTasksForSystem(
		  StarSystemInfo system,
		  Dictionary<FleetInfo, FleetRangeData> fleetRanges,
		  bool hostilesAtSystem)
		{
			List<StrategicTask> strategicTaskList = new List<StrategicTask>();
			StrategicTask surveyTaskForSystem = this.GetPotentialSurveyTaskForSystem(system, fleetRanges, hostilesAtSystem);
			if (surveyTaskForSystem != null)
				strategicTaskList.Add(surveyTaskForSystem);
			StrategicTask gateTaskForSystem = this.GetPotentialGateTaskForSystem(system, fleetRanges, hostilesAtSystem);
			if (gateTaskForSystem != null)
				strategicTaskList.Add(gateTaskForSystem);
			StrategicTask npgTaskForSystem = this.GetPotentialNPGTaskForSystem(system, fleetRanges, hostilesAtSystem);
			if (npgTaskForSystem != null)
				strategicTaskList.Add(npgTaskForSystem);
			List<StrategicTask> colonizeTasksForSystem = this.GetPotentialColonizeTasksForSystem(system, fleetRanges, hostilesAtSystem);
			if (colonizeTasksForSystem.Count > 0)
				strategicTaskList.AddRange((IEnumerable<StrategicTask>)colonizeTasksForSystem);
			List<StrategicTask> supportTasksForSystem = this.GetPotentialSupportTasksForSystem(system, fleetRanges, hostilesAtSystem);
			if (supportTasksForSystem.Count > 0)
				strategicTaskList.AddRange((IEnumerable<StrategicTask>)supportTasksForSystem);
			List<StrategicTask> constructionTasksForSystem = this.GetPotentialConstructionTasksForSystem(system, fleetRanges, hostilesAtSystem);
			if (constructionTasksForSystem.Count > 0)
				strategicTaskList.AddRange((IEnumerable<StrategicTask>)constructionTasksForSystem);
			List<StrategicTask> upgradeTasksForSystem = this.GetPotentialUpgradeTasksForSystem(system, fleetRanges, hostilesAtSystem);
			if (upgradeTasksForSystem.Count > 0)
				strategicTaskList.AddRange((IEnumerable<StrategicTask>)upgradeTasksForSystem);
			StrategicTask patrolTaskForSystem = this.GetPotentialPatrolTaskForSystem(system, fleetRanges, hostilesAtSystem);
			if (patrolTaskForSystem != null)
				strategicTaskList.Add(patrolTaskForSystem);
			StrategicTask interdictTaskForSystem = this.GetPotentialInterdictTaskForSystem(system, fleetRanges, hostilesAtSystem);
			if (interdictTaskForSystem != null)
				strategicTaskList.Add(interdictTaskForSystem);
			StrategicTask interceptTaskForSystem = this.GetPotentialInterceptTaskForSystem(system, fleetRanges, hostilesAtSystem);
			if (interceptTaskForSystem != null)
				strategicTaskList.Add(interceptTaskForSystem);
			StrategicTask strikeTaskForSystem = this.GetPotentialStrikeTaskForSystem(system, fleetRanges, hostilesAtSystem);
			if (strikeTaskForSystem != null)
				strategicTaskList.Add(strikeTaskForSystem);
			StrategicTask invadeTaskForSystem = this.GetPotentialInvadeTaskForSystem(system, fleetRanges, hostilesAtSystem);
			if (invadeTaskForSystem != null)
				strategicTaskList.Add(invadeTaskForSystem);
			return strategicTaskList;
		}

		private StrategicTask GetPotentialSurveyTaskForSystem(
		  StarSystemInfo system,
		  Dictionary<FleetInfo, FleetRangeData> fleetRanges,
		  bool hostilesAtSystem)
		{
			if (!this._db.CanSurvey(this._player.ID, system.ID))
				return (StrategicTask)null;
			StrategicTask strategicTask = new StrategicTask();
			strategicTask.Mission = MissionType.SURVEY;
			strategicTask.RequiredFleetTypes = FleetTypeFlags.SURVEY;
			strategicTask.SystemIDTarget = system.ID;
			strategicTask.HostilesAtSystem = hostilesAtSystem;
			foreach (FleetInfo key in this.m_FleetsInSurveyRange)
			{
				float? travelSpeed = new float?();
				float? nodeTravelSpeed = new float?();
				FleetRangeData fleetRangeData;
				if (fleetRanges.TryGetValue(key, out fleetRangeData))
				{
					float? nullable = new float?(fleetRangeData.FleetRange);
					travelSpeed = fleetRangeData.FleetTravelSpeed;
					nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
				}
				FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, key.ID, this._player.ID, this._player.Faction.Name == "loa");
				if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN)
				{
					MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.SURVEY, StationType.INVALID_TYPE, key.ID, system.ID, 0, (List<int>)null, 1, false, travelSpeed, nodeTravelSpeed);
					strategicTask.UseableFleets.Add(new FleetManagement()
					{
						Fleet = key,
						MissionTime = missionEstimate,
						FleetTypes = fleetTypeFlags
					});
				}
			}
			return strategicTask;
		}

		private StrategicTask GetPotentialGateTaskForSystem(
		  StarSystemInfo system,
		  Dictionary<FleetInfo, FleetRangeData> fleetRanges,
		  bool hostilesAtSystem)
		{
			if (!this._player.Faction.CanUseGate() || !this._game.GameDatabase.CanGate(this._player.ID, system.ID))
				return (StrategicTask)null;
			StrategicTask strategicTask = new StrategicTask();
			strategicTask.Mission = MissionType.GATE;
			strategicTask.RequiredFleetTypes = FleetTypeFlags.GATE;
			strategicTask.SystemIDTarget = system.ID;
			strategicTask.HostilesAtSystem = hostilesAtSystem;
			foreach (FleetInfo key in this.m_FleetsInSurveyRange)
			{
				float? travelSpeed = new float?();
				float? nodeTravelSpeed = new float?();
				FleetRangeData fleetRangeData;
				if (fleetRanges.TryGetValue(key, out fleetRangeData))
				{
					float? nullable = new float?(fleetRangeData.FleetRange);
					travelSpeed = fleetRangeData.FleetTravelSpeed;
					nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
				}
				FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, key.ID, this._player.ID, this._player.Faction.Name == "loa");
				if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN)
				{
					MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.GATE, StationType.INVALID_TYPE, key.ID, system.ID, 0, (List<int>)null, 1, false, travelSpeed, nodeTravelSpeed);
					strategicTask.UseableFleets.Add(new FleetManagement()
					{
						Fleet = key,
						MissionTime = missionEstimate,
						FleetTypes = fleetTypeFlags
					});
				}
			}
			return strategicTask;
		}

		private StrategicTask GetPotentialNPGTaskForSystem(
		  StarSystemInfo system,
		  Dictionary<FleetInfo, FleetRangeData> fleetRanges,
		  bool hostilesAtSystem)
		{
			if (!this._player.Faction.CanUseAccelerators() || this._db.SystemHasAccelerator(system.ID, this._player.ID))
				return (StrategicTask)null;
			if (this.m_FleetsInSurveyRange.Count == 0)
				return (StrategicTask)null;
			int productionCost = this._db.GetDesignInfosForPlayer(this._player.ID).FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.IsAccelerator())).ProductionCost;
			StrategicTask strategicTask = new StrategicTask();
			strategicTask.Mission = MissionType.DEPLOY_NPG;
			strategicTask.RequiredFleetTypes = FleetTypeFlags.NPG;
			strategicTask.SystemIDTarget = system.ID;
			strategicTask.HostilesAtSystem = hostilesAtSystem;
			foreach (FleetInfo key in this.m_FleetsInSurveyRange)
			{
				float? travelSpeed = new float?();
				float? nodeTravelSpeed = new float?();
				FleetRangeData fleetRangeData;
				if (fleetRanges.TryGetValue(key, out fleetRangeData))
				{
					float? nullable = new float?(fleetRangeData.FleetRange);
					travelSpeed = fleetRangeData.FleetTravelSpeed;
					nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
				}
				FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, key.ID, this._player.ID, this._player.Faction.Name == "loa");
				if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN)
				{
					int num = 0;
					if ((!this.m_FleetCubePoints.TryGetValue(key.ID, out num) || num >= productionCost) && this._db.GetMissionByFleetID(key.ID) == null)
					{
						MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.DEPLOY_NPG, StationType.INVALID_TYPE, key.ID, system.ID, 0, (List<int>)null, 1, false, travelSpeed, nodeTravelSpeed);
						strategicTask.UseableFleets.Add(new FleetManagement()
						{
							Fleet = key,
							MissionTime = missionEstimate,
							FleetTypes = fleetTypeFlags
						});
					}
				}
			}
			return strategicTask;
		}

		private List<StrategicTask> GetPotentialColonizeTasksForSystem(
		  StarSystemInfo system,
		  Dictionary<FleetInfo, FleetRangeData> fleetRanges,
		  bool hostilesAtSystem)
		{
			List<StrategicTask> strategicTaskList = new List<StrategicTask>();
			if (this.m_TurnBudget.CurrentSavings < -1000000.0 || !this._db.CanColonize(this._player.ID, system.ID, this._db.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, this._player.ID)) || this.m_AvailableFullFleets.Count == 0)
				return strategicTaskList;
			List<int> list = StarSystemDetailsUI.CollectPlanetListItemsForColonizeMission(this._game.App, system.ID, this._player.ID).ToList<int>();
			if (list.Count == 0)
				return strategicTaskList;
			foreach (int planetID in list)
			{
				StrategicTask strategicTask = new StrategicTask();
				strategicTask.Mission = MissionType.COLONIZATION;
				strategicTask.RequiredFleetTypes = FleetTypeFlags.COLONIZE;
				strategicTask.SystemIDTarget = system.ID;
				strategicTask.PlanetIDTarget = planetID;
				strategicTask.HostilesAtSystem = hostilesAtSystem;
				foreach (FleetInfo key in this.m_FleetsInSurveyRange)
				{
					float? travelSpeed = new float?();
					float? nodeTravelSpeed = new float?();
					FleetRangeData fleetRangeData;
					if (fleetRanges.TryGetValue(key, out fleetRangeData))
					{
						float? nullable = new float?(fleetRangeData.FleetRange);
						travelSpeed = fleetRangeData.FleetTravelSpeed;
						nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
					}
					FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, key.ID, this._player.ID, this._player.Faction.Name == "loa");
					if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN)
					{
						MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.COLONIZATION, StationType.INVALID_TYPE, key.ID, system.ID, planetID, (List<int>)null, 1, false, travelSpeed, nodeTravelSpeed);
						strategicTask.UseableFleets.Add(new FleetManagement()
						{
							Fleet = key,
							MissionTime = missionEstimate,
							FleetTypes = fleetTypeFlags
						});
					}
				}
				strategicTaskList.Add(strategicTask);
			}
			return strategicTaskList;
		}

		private List<StrategicTask> GetPotentialSupportTasksForSystem(
		  StarSystemInfo system,
		  Dictionary<FleetInfo, FleetRangeData> fleetRanges,
		  bool hostilesAtSystem)
		{
			List<StrategicTask> strategicTaskList = new List<StrategicTask>();
			if (!this._db.CanSupport(this._player.ID, system.ID) || this.m_FleetsInSurveyRange.Count == 0)
				return strategicTaskList;
			List<int> list = StarSystemDetailsUI.CollectPlanetListItemsForSupportMission(this._game.App, system.ID).ToList<int>();
			if (list.Count == 0)
				return strategicTaskList;
			foreach (int num in list)
			{
				StrategicTask strategicTask = new StrategicTask();
				strategicTask.Mission = MissionType.SUPPORT;
				strategicTask.RequiredFleetTypes = FleetTypeFlags.COLONIZE;
				strategicTask.SystemIDTarget = system.ID;
				strategicTask.PlanetIDTarget = num;
				strategicTask.HostilesAtSystem = hostilesAtSystem;
				foreach (FleetInfo key in this.m_FleetsInSurveyRange)
				{
					FleetRangeData fleetRangeData;
					if (fleetRanges.TryGetValue(key, out fleetRangeData))
					{
						float? nullable = new float?(fleetRangeData.FleetRange);
						float? fleetTravelSpeed = fleetRangeData.FleetTravelSpeed;
						float? fleetNodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
					}
					FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, key.ID, this._player.ID, this._player.Faction.Name == "loa");
					if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN)
					{
						MissionEstimate missionEstimate = new MissionEstimate();
						strategicTask.UseableFleets.Add(new FleetManagement()
						{
							Fleet = key,
							MissionTime = missionEstimate,
							FleetTypes = fleetTypeFlags
						});
					}
				}
				strategicTaskList.Add(strategicTask);
			}
			return strategicTaskList;
		}

		private List<StrategicTask> GetPotentialConstructionTasksForSystem(
		  StarSystemInfo system,
		  Dictionary<FleetInfo, FleetRangeData> fleetRanges,
		  bool hostilesAtSystem)
		{
			List<StrategicTask> strategicTaskList = new List<StrategicTask>();
			int? systemOwningPlayer = this._game.GameDatabase.GetSystemOwningPlayer(system.ID);
			if (!systemOwningPlayer.HasValue || systemOwningPlayer.Value != this._player.ID && !StarMapState.SystemHasIndependentColony(this._game, system.ID))
				return strategicTaskList;
			List<StationType> list = Kerberos.Sots.GameStates.StarSystem.GetSystemCanSupportStations(this._game, system.ID, this._player.ID).Where<StationType>((Func<StationType, bool>)(j => j != StationType.DEFENCE)).ToList<StationType>();
			if (list.Count == 0)
				return strategicTaskList;
			foreach (StationType stationType in list)
			{
				int? planetForStation = Kerberos.Sots.GameStates.StarSystem.GetSuitablePlanetForStation(this._game, this._player.ID, system.ID, stationType);
				if (planetForStation.HasValue)
				{
					StrategicTask strategicTask = new StrategicTask();
					strategicTask.Mission = MissionType.CONSTRUCT_STN;
					strategicTask.RequiredFleetTypes = FleetTypeFlags.CONSTRUCTION;
					strategicTask.SystemIDTarget = system.ID;
					strategicTask.StationType = stationType;
					strategicTask.PlanetIDTarget = planetForStation.Value;
					strategicTask.HostilesAtSystem = hostilesAtSystem;
					foreach (FleetInfo key in this.m_FleetsInSurveyRange)
					{
						float? travelSpeed = new float?();
						float? nodeTravelSpeed = new float?();
						FleetRangeData fleetRangeData;
						if (fleetRanges.TryGetValue(key, out fleetRangeData))
						{
							double fleetRange = (double)fleetRangeData.FleetRange;
							travelSpeed = fleetRangeData.FleetTravelSpeed;
							nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
						}
						FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, key.ID, this._player.ID, this._player.Faction.Name == "loa");
						if (this._player.Faction.Name == "loa" || (strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN && (double)Kerberos.Sots.StarFleet.StarFleet.GetConstructionPointsForFleet(this._game, key.ID) >= 1.0 && StrategicAI.GetTemplateForFleet(this._game, this._player.ID, key.ID).MissionTypes.Contains(MissionType.CONSTRUCT_STN))
						{
							MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.CONSTRUCT_STN, stationType, key.ID, system.ID, 0, (List<int>)null, 1, false, travelSpeed, nodeTravelSpeed);
							strategicTask.UseableFleets.Add(new FleetManagement()
							{
								Fleet = key,
								MissionTime = missionEstimate,
								FleetTypes = fleetTypeFlags
							});
						}
					}
					strategicTaskList.Add(strategicTask);
				}
			}
			return strategicTaskList;
		}

		private List<StrategicTask> GetPotentialUpgradeTasksForSystem(
		  StarSystemInfo system,
		  Dictionary<FleetInfo, FleetRangeData> fleetRanges,
		  bool hostilesAtSystem)
		{
			List<StrategicTask> strategicTaskList = new List<StrategicTask>();
			List<StationInfo> upgradableStations = this._game.GetUpgradableStations(this._game.GameDatabase.GetStationForSystemAndPlayer(system.ID, this._player.ID).ToList<StationInfo>());
			if (upgradableStations.Count == 0)
				return strategicTaskList;
			foreach (StationInfo stationInfo in upgradableStations)
			{
				StrategicTask strategicTask = new StrategicTask();
				strategicTask.Mission = MissionType.UPGRADE_STN;
				strategicTask.RequiredFleetTypes = FleetTypeFlags.CONSTRUCTION;
				strategicTask.SystemIDTarget = system.ID;
				strategicTask.StationIDTarget = stationInfo.OrbitalObjectID;
				strategicTask.StationType = stationInfo.DesignInfo.StationType;
				strategicTask.HostilesAtSystem = hostilesAtSystem;
				foreach (FleetInfo key in this.m_FleetsInSurveyRange)
				{
					float? travelSpeed = new float?();
					float? nodeTravelSpeed = new float?();
					FleetRangeData fleetRangeData;
					if (fleetRanges.TryGetValue(key, out fleetRangeData))
					{
						double fleetRange = (double)fleetRangeData.FleetRange;
						travelSpeed = fleetRangeData.FleetTravelSpeed;
						nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
					}
					FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, key.ID, this._player.ID, this._player.Faction.Name == "loa");
					if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN && (double)Kerberos.Sots.StarFleet.StarFleet.GetConstructionPointsForFleet(this._game, key.ID) >= 1.0 && StrategicAI.GetTemplateForFleet(this._game, this._player.ID, key.ID).MissionTypes.Contains(MissionType.UPGRADE_STN))
					{
						MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.UPGRADE_STN, stationInfo.DesignInfo.StationType, key.ID, system.ID, stationInfo.OrbitalObjectID, (List<int>)null, stationInfo.DesignInfo.StationLevel + 1, false, travelSpeed, nodeTravelSpeed);
						strategicTask.UseableFleets.Add(new FleetManagement()
						{
							Fleet = key,
							MissionTime = missionEstimate,
							FleetTypes = fleetTypeFlags
						});
					}
				}
				strategicTaskList.Add(strategicTask);
			}
			return strategicTaskList;
		}

		private StrategicTask GetPotentialRelocateTaskForSystem(
		  StarSystemInfo system,
		  Dictionary<FleetInfo, FleetRangeData> fleetRanges,
		  bool hostilesAtSystem)
		{
			if (this.m_AvailableFullFleets.Count + this.m_AvailableShortFleets.Count == 0)
				return (StrategicTask)null;
			StrategicTask strategicTask = new StrategicTask();
			strategicTask.Mission = MissionType.RELOCATION;
			strategicTask.RequiredFleetTypes = FleetTypeFlags.ANY;
			strategicTask.SystemIDTarget = system.ID;
			strategicTask.HostilesAtSystem = hostilesAtSystem;
			strategicTask.SupportPointsAtSystem = this._db.GetRemainingSupportPoints(this._game, system.ID, this._player.ID);
			List<FleetInfo> fleetInfoList = new List<FleetInfo>();
			fleetInfoList.AddRange((IEnumerable<FleetInfo>)this.m_AvailableFullFleets);
			fleetInfoList.AddRange((IEnumerable<FleetInfo>)this.m_AvailableShortFleets);
			foreach (FleetInfo key in fleetInfoList)
			{
				if (this.m_DefenseCombatFleets.Contains(key))
					return (StrategicTask)null;
				float? travelSpeed = new float?();
				float? nodeTravelSpeed = new float?();
				FleetRangeData fleetRangeData;
				if (fleetRanges.TryGetValue(key, out fleetRangeData))
				{
					double fleetRange = (double)fleetRangeData.FleetRange;
					travelSpeed = fleetRangeData.FleetTravelSpeed;
					nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
				}
				FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, key.ID, this._player.ID, this._player.Faction.Name == "loa");
				if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN && Kerberos.Sots.StarFleet.StarFleet.CanDoRelocationMissionToTarget(this._game, system.ID, key.ID))
				{
					MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.RELOCATION, StationType.INVALID_TYPE, key.ID, system.ID, 0, (List<int>)null, 1, false, travelSpeed, nodeTravelSpeed);
					strategicTask.UseableFleets.Add(new FleetManagement()
					{
						Fleet = key,
						MissionTime = missionEstimate,
						FleetTypes = fleetTypeFlags
					});
				}
			}
			return strategicTask;
		}

		private StrategicTask GetPotentialPatrolTaskForSystem(
		  StarSystemInfo system,
		  Dictionary<FleetInfo, FleetRangeData> fleetRanges,
		  bool hostilesAtSystem)
		{
			if (!this._db.CanPatrol(this._player.ID, system.ID))
				return (StrategicTask)null;
			StrategicTask strategicTask = new StrategicTask();
			strategicTask.Mission = MissionType.PATROL;
			strategicTask.RequiredFleetTypes = FleetTypeFlags.PATROL;
			strategicTask.SystemIDTarget = system.ID;
			strategicTask.HostilesAtSystem = hostilesAtSystem;
			foreach (FleetInfo key in this.m_FleetsInSurveyRange)
			{
				float? travelSpeed = new float?();
				float? nodeTravelSpeed = new float?();
				FleetRangeData fleetRangeData;
				if (fleetRanges.TryGetValue(key, out fleetRangeData))
				{
					float? nullable = new float?(fleetRangeData.FleetRange);
					travelSpeed = fleetRangeData.FleetTravelSpeed;
					nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
				}
				FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, key.ID, this._player.ID, this._player.Faction.Name == "loa");
				if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN)
				{
					MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.PATROL, StationType.INVALID_TYPE, key.ID, system.ID, 0, (List<int>)null, 1, false, travelSpeed, nodeTravelSpeed);
					strategicTask.UseableFleets.Add(new FleetManagement()
					{
						Fleet = key,
						MissionTime = missionEstimate,
						FleetTypes = fleetTypeFlags
					});
				}
			}
			return strategicTask;
		}

		private StrategicTask GetPotentialInterdictTaskForSystem(
		  StarSystemInfo system,
		  Dictionary<FleetInfo, FleetRangeData> fleetRanges,
		  bool hostilesAtSystem)
		{
			if (!this._db.CanInterdict(this._player.ID, system.ID))
				return (StrategicTask)null;
			StrategicTask strategicTask = new StrategicTask();
			strategicTask.Mission = MissionType.INTERDICTION;
			strategicTask.RequiredFleetTypes = FleetTypeFlags.COMBAT | FleetTypeFlags.PATROL;
			strategicTask.SystemIDTarget = system.ID;
			strategicTask.HostilesAtSystem = hostilesAtSystem;
			foreach (FleetInfo key in this.m_FleetsInSurveyRange)
			{
				float? travelSpeed = new float?();
				float? nodeTravelSpeed = new float?();
				FleetRangeData fleetRangeData;
				if (fleetRanges.TryGetValue(key, out fleetRangeData))
				{
					float? nullable = new float?(fleetRangeData.FleetRange);
					travelSpeed = fleetRangeData.FleetTravelSpeed;
					nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
				}
				FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, key.ID, this._player.ID, this._player.Faction.Name == "loa");
				if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN)
				{
					MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.INTERDICTION, StationType.INVALID_TYPE, key.ID, system.ID, 0, (List<int>)null, 1, false, travelSpeed, nodeTravelSpeed);
					strategicTask.UseableFleets.Add(new FleetManagement()
					{
						Fleet = key,
						MissionTime = missionEstimate,
						FleetTypes = fleetTypeFlags
					});
				}
			}
			return strategicTask;
		}

		private StrategicTask GetPotentialInterceptTaskForSystem(
		  StarSystemInfo system,
		  Dictionary<FleetInfo, FleetRangeData> fleetRanges,
		  bool hostilesAtSystem)
		{
			return (StrategicTask)null;
		}

		private StrategicTask GetPotentialStrikeTaskForSystem(
		  StarSystemInfo system,
		  Dictionary<FleetInfo, FleetRangeData> fleetRanges,
		  bool hostilesAtSystem)
		{
			if (!this._db.CanStrike(this._player.ID, system.ID) || !hostilesAtSystem)
				return (StrategicTask)null;
			if (this.m_FleetsInSurveyRange.Count == 0)
				return (StrategicTask)null;
			StrategicTask task = new StrategicTask();
			task.Mission = MissionType.STRIKE;
			task.RequiredFleetTypes = FleetTypeFlags.COMBAT;
			task.SystemIDTarget = system.ID;
			task.HostilesAtSystem = hostilesAtSystem;
			task.EnemyStrength = this.GetEnemyFleetStrength(system.ID);
			List<FleetInfo> list = this._game.GameDatabase.GetFleetInfoBySystemID(system.ID, FleetType.FL_NORMAL | FleetType.FL_DEFENSE).ToList<FleetInfo>();
			task.NumStandardPlayersAtSystem = this.GetNumStandardPlayersAtSystem(task, list);
			task.EasterEggsAtSystem = this.GetEncountersAtSystem(system.ID, list.Where<FleetInfo>((Func<FleetInfo, bool>)(x => x.Type == FleetType.FL_NORMAL)).ToList<FleetInfo>());
			foreach (FleetInfo fleetInfo in this.m_FleetsInSurveyRange)
			{
				float? travelSpeed = new float?();
				float? nodeTravelSpeed = new float?();
				FleetRangeData fleetRangeData;
				if (fleetRanges.TryGetValue(fleetInfo, out fleetRangeData))
				{
					double fleetRange = (double)fleetRangeData.FleetRange;
					travelSpeed = fleetRangeData.FleetTravelSpeed;
					nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
				}
				FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, fleetInfo.ID, this._player.ID, this._player.Faction.Name == "loa");
				if ((task.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN)
				{
					MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.STRIKE, StationType.INVALID_TYPE, fleetInfo.ID, system.ID, 0, (List<int>)null, 1, false, travelSpeed, nodeTravelSpeed);
					task.UseableFleets.Add(new FleetManagement()
					{
						Fleet = fleetInfo,
						MissionTime = missionEstimate,
						FleetStrength = this.GetFleetStrength(fleetInfo),
						FleetTypes = fleetTypeFlags
					});
				}
			}
			return task;
		}

		private StrategicTask GetPotentialInvadeTaskForSystem(
		  StarSystemInfo system,
		  Dictionary<FleetInfo, FleetRangeData> fleetRanges,
		  bool hostilesAtSystem)
		{
			if (!this._db.CanInvade(this._player.ID, system.ID) || !hostilesAtSystem || StarSystemDetailsUI.CollectPlanetListItemsForInvasionMission(this._game.App, system.ID).Count<int>() == 0)
				return (StrategicTask)null;
			StrategicTask strategicTask = new StrategicTask();
			strategicTask.Mission = MissionType.INVASION;
			strategicTask.RequiredFleetTypes = FleetTypeFlags.COMBAT | FleetTypeFlags.PLANETATTACK;
			strategicTask.SystemIDTarget = system.ID;
			strategicTask.HostilesAtSystem = hostilesAtSystem;
			strategicTask.EnemyStrength = this.GetEnemyFleetStrength(system.ID);
			foreach (FleetInfo fleetInfo in this.m_FleetsInSurveyRange)
			{
				float? travelSpeed = new float?();
				float? nodeTravelSpeed = new float?();
				FleetRangeData fleetRangeData;
				if (fleetRanges.TryGetValue(fleetInfo, out fleetRangeData))
				{
					float? nullable = new float?(fleetRangeData.FleetRange);
					travelSpeed = fleetRangeData.FleetTravelSpeed;
					nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
				}
				FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, fleetInfo.ID, this._player.ID, this._player.Faction.Name == "loa");
				if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN)
				{
					MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.INVASION, StationType.INVALID_TYPE, fleetInfo.ID, system.ID, 0, (List<int>)null, 1, false, travelSpeed, nodeTravelSpeed);
					strategicTask.UseableFleets.Add(new FleetManagement()
					{
						Fleet = fleetInfo,
						MissionTime = missionEstimate,
						FleetStrength = this.GetFleetStrength(fleetInfo),
						FleetTypes = fleetTypeFlags
					});
				}
			}
			return strategicTask;
		}

		private int GetScoreForTask(StrategicTask task, AIStance stance)
		{
			switch (task.Mission)
			{
				case MissionType.COLONIZATION:
					return this.ScoreTaskForColonize(task, stance);
				case MissionType.SUPPORT:
					return this.ScoreTaskForSupport(task, stance);
				case MissionType.SURVEY:
					return this.ScoreTaskForSurvey(task, stance);
				case MissionType.RELOCATION:
					return this.ScoreTaskForRelocate(task, stance);
				case MissionType.CONSTRUCT_STN:
					return this.ScoreTaskForConstruction(task, stance);
				case MissionType.UPGRADE_STN:
					return this.ScoreTaskForUpgrade(task, stance);
				case MissionType.PATROL:
					return this.ScoreTaskForPatrol(task, stance);
				case MissionType.INTERDICTION:
					return this.ScoreTaskForInterdict(task, stance);
				case MissionType.STRIKE:
					return this.ScoreTaskForStrike(task, stance);
				case MissionType.INVASION:
					return this.ScoreTaskForInvade(task, stance);
				case MissionType.INTERCEPT:
					return this.ScoreTaskForIntercept(task, stance);
				case MissionType.GATE:
					return this.ScoreTaskForGate(task, stance);
				case MissionType.DEPLOY_NPG:
					return this.ScoreTaskForNPG(task, stance);
				default:
					return 0;
			}
		}

		private int ScoreTaskForSurvey(StrategicTask task, AIStance stance)
		{
			if (task.HostilesAtSystem)
				return this._player.Faction.Name == "zuul" ? 4 : 0;
			if (!task.UseableFleets.Any<FleetManagement>((Func<FleetManagement, bool>)(x => x.Fleet.SystemID == task.SystemIDTarget)) && (this._player.Faction.CanUseAccelerators() && !this._db.SystemHasAccelerator(task.SystemIDTarget, this._player.ID) || this._player.Faction.CanUseGate() && !this._db.SystemHasGate(task.SystemIDTarget, this._player.ID)))
				return 0;
			if (this._player.Faction.CanUseNodeLine(new bool?(true)))
			{
				bool flag = false;
				foreach (NodeLineInfo nodeLineInfo in this._db.GetNodeLines().Where<NodeLineInfo>((Func<NodeLineInfo, bool>)(x =>
			   {
				   if (!x.IsPermenant || x.IsLoaLine)
					   return false;
				   if (x.System1ID != task.SystemIDTarget)
					   return x.System2ID == task.SystemIDTarget;
				   return true;
			   })).ToList<NodeLineInfo>())
				{
					if (this._db.GetExploredNodeLinesFromSystem(this._player.ID, task.SystemIDTarget).Count<NodeLineInfo>() > 0)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
					return 0;
			}
			switch (stance)
			{
				case AIStance.CONQUERING:
					return 3;
				case AIStance.DESTROYING:
					return 3;
				case AIStance.DEFENDING:
					return 3;
				default:
					return 3;
			}
		}

		private int ScoreTaskForGate(StrategicTask task, AIStance stance)
		{
			if (this._db.SystemHasGate(task.SystemIDTarget, this._player.ID) || task.EasterEggsAtSystem.Any<EasterEgg>())
				return 0;
			switch (stance)
			{
				case AIStance.CONQUERING:
					return 6;
				case AIStance.DESTROYING:
					return 5;
				case AIStance.DEFENDING:
					return 5;
				default:
					return 5;
			}
		}

		private int ScoreTaskForNPG(StrategicTask task, AIStance stance)
		{
			if (this._db.SystemHasAccelerator(task.SystemIDTarget, this._player.ID) || this._player.Faction.CanUseAccelerators() && task.UseableFleets.Count != 0 && task.UseableFleets.Min<FleetManagement>((Func<FleetManagement, int>)(x => x.MissionTime.TurnsToTarget)) > 6)
				return 0;
			switch (stance)
			{
				case AIStance.HUNKERING:
					return 3;
				case AIStance.CONQUERING:
					return 3;
				case AIStance.DESTROYING:
					return 3;
				case AIStance.DEFENDING:
					return 3;
				default:
					return 4;
			}
		}

		private int ScoreTaskForColonize(StrategicTask task, AIStance stance)
		{
			if (task.HostilesAtSystem || this.HaveNewColonizationMissionEnroute() || !task.UseableFleets.Any<FleetManagement>((Func<FleetManagement, bool>)(x => x.Fleet.SystemID == task.SystemIDTarget)) && (this._player.Faction.CanUseAccelerators() && !this._db.SystemHasAccelerator(task.SystemIDTarget, this._player.ID) || this._player.Faction.CanUseGate() && !this._db.SystemHasGate(task.SystemIDTarget, this._player.ID)))
				return 0;
			double colonySupportCost = Colony.GetColonySupportCost(this._db, this._player.ID, task.PlanetIDTarget);
			ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(task.PlanetIDTarget);
			if (colonySupportCost > 0.0 && colonySupportCost > this.GetAvailableColonySupportBudget())
				return 0;
			if (task.Mission == MissionType.SUPPORT && colonyInfoForPlanet.ImperialPop >= 1000000.0)
				return 5;
			switch (stance)
			{
				case AIStance.HUNKERING:
					return 4;
				case AIStance.CONQUERING:
					return 4;
				case AIStance.DESTROYING:
					return 4;
				case AIStance.DEFENDING:
					return 4;
				default:
					return 4;
			}
		}

		private int ScoreTaskForSupport(StrategicTask task, AIStance stance)
		{
			if (!this._db.CanSupportPlanet(this._player.ID, task.PlanetIDTarget))
				return 0;
			return this.ScoreTaskForColonize(task, stance) + 1;
		}

		private float GetStationShortFall(AIStance stance, StationType stationType)
		{
			float[] numArray = new float[8];
			numArray[5] = !this._player.Faction.CanUseGate() ? 0.0f : 1f;
			switch (stance)
			{
				case AIStance.EXPANDING:
					numArray[1] = 1f;
					numArray[3] = 1f;
					numArray[2] = 0.15f;
					numArray[4] = 0.0f;
					break;
				case AIStance.ARMING:
					numArray[1] = 1f;
					numArray[3] = 1f;
					numArray[2] = 0.3f;
					numArray[4] = 0.15f;
					break;
				case AIStance.HUNKERING:
					numArray[1] = 1f;
					numArray[3] = 1f;
					numArray[2] = 0.6f;
					numArray[4] = 0.2f;
					break;
				case AIStance.CONQUERING:
					numArray[1] = 1f;
					numArray[3] = 1f;
					numArray[2] = 0.3f;
					numArray[4] = 0.15f;
					break;
				case AIStance.DESTROYING:
					numArray[1] = 1f;
					numArray[3] = 1f;
					numArray[2] = 0.2f;
					numArray[4] = 0.0f;
					break;
				case AIStance.DEFENDING:
					numArray[1] = 1f;
					numArray[3] = 1f;
					numArray[2] = 0.3f;
					numArray[4] = 0.1f;
					break;
			}
			int count = this.m_ColonizedSystems.Count;
			float num = (float)this.m_NumStations[(int)stationType] / (float)count;
			if ((double)numArray[(int)stationType] <= 0.0)
				return 0.0f;
			return (numArray[(int)stationType] - num) / numArray[(int)stationType];
		}

		private int ScoreTaskForConstruction(StrategicTask task, AIStance stance)
		{
			if (task.HostilesAtSystem)
				return 0;
			if (!task.UseableFleets.Any<FleetManagement>((Func<FleetManagement, bool>)(x => x.Fleet.SystemID == task.SystemIDTarget)))
			{
				if (this._player.Faction.CanUseAccelerators())
				{
					if (!this._db.SystemHasAccelerator(task.SystemIDTarget, this._player.ID))
						return 0;
				}
				else if (this._player.Faction.CanUseGate() && !this._db.SystemHasGate(task.SystemIDTarget, this._player.ID))
					return 0;
			}
			int num1 = 0;
			float num2 = 1f;
			if (task.StationType == StationType.NAVAL)
				num1 = 1;
			if (task.StationType == StationType.CIVILIAN && this._game.GetMaxExportCapacity(task.SystemIDTarget) >= 5)
				num1 = 1;
			if (task.StationType == StationType.CIVILIAN && this._game.GetMaxExportCapacity(task.SystemIDTarget) >= 9)
				num1 = 2;
			if (task.StationType == StationType.SCIENCE && this._db.GetStratModifier<bool>(StratModifiers.EnableTrade, this._player.ID) && this._db.GetStationForSystemPlayerAndType(task.SystemIDTarget, this._player.ID, StationType.CIVILIAN) == null || (task.StationType == StationType.SCIENCE && this._db.GetStationForSystemPlayerAndType(task.SystemIDTarget, this._player.ID, StationType.NAVAL) == null || task.StationType == StationType.CIVILIAN && this._db.GetStationForSystemPlayerAndType(task.SystemIDTarget, this._player.ID, StationType.NAVAL) == null))
				return 0;
			if (task.StationType == StationType.DIPLOMATIC)
			{
				List<StationInfo> list = this._db.GetStationForSystemAndPlayer(task.SystemIDTarget, this._player.ID).ToList<StationInfo>();
				if (list.Count == 0)
					return 0;
				if (this._player.Faction.Name == "zuul")
				{
					if (list.Count<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == StationType.NAVAL)) == 0 || this._db.GetStationInfosByPlayerID(this._player.ID).Count<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == StationType.DIPLOMATIC)) > 3)
						return 0;
					num2 = 0.25f;
					num1 = 2;
				}
				else if (list.Count<StationInfo>((Func<StationInfo, bool>)(x =>
			   {
				   if (x.DesignInfo.StationType == StationType.NAVAL)
					   return x.DesignInfo.StationLevel >= 2;
				   return false;
			   })) == 0)
					return 0;
			}
			if (this.GetAvailableStationSupportBudget() < (double)GameSession.CalculateLVL1StationUpkeepCost(this._game.AssetDatabase, task.StationType) * (double)num2 || (double)this.GetStationShortFall(stance, task.StationType) <= 0.0)
				return 0;
			switch (stance)
			{
				case AIStance.HUNKERING:
					return 4 + num1;
				case AIStance.CONQUERING:
					return 4 + num1;
				case AIStance.DESTROYING:
					return 4 + num1;
				case AIStance.DEFENDING:
					return 4 + num1;
				default:
					return 4 + num1;
			}
		}

		private int ScoreTaskForUpgrade(StrategicTask task, AIStance stance)
		{
			if (task.HostilesAtSystem)
				return 0;
			if (!task.UseableFleets.Any<FleetManagement>((Func<FleetManagement, bool>)(x => x.Fleet.SystemID == task.SystemIDTarget)))
			{
				if (this._player.Faction.CanUseAccelerators())
				{
					if (!this._db.SystemHasAccelerator(task.SystemIDTarget, this._player.ID))
						return 0;
				}
				else if (this._player.Faction.CanUseGate() && !this._db.SystemHasGate(task.SystemIDTarget, this._player.ID))
					return 0;
			}
			StationInfo stationInfo = this._db.GetStationInfo(task.StationIDTarget);
			if (stationInfo == null || !this._game.StationIsUpgradable(stationInfo))
				return 0;
			int num1 = 0;
			if (task.StationType == StationType.CIVILIAN && this._game.GetMaxExportCapacity(task.SystemIDTarget) <= 3)
				return 0;
			if (task.StationType == StationType.CIVILIAN && this._game.GetMaxExportCapacity(task.SystemIDTarget) >= 6)
				num1 = 1;
			if (task.StationType == StationType.CIVILIAN && this._game.GetMaxExportCapacity(task.SystemIDTarget) >= 9)
				num1 = 3;
			double num2 = 1.0;
			if (this._player.Faction.Name != "zuul" || stationInfo.DesignInfo.StationType != StationType.DIPLOMATIC)
			{
				if (stationInfo.DesignInfo.StationLevel > 3 && this._db.GetTurnCount() <= 100)
					return 0;
			}
			else if (this._player.Faction.Name == "zuul" && stationInfo.DesignInfo.StationType == StationType.DIPLOMATIC)
				num2 = 0.05;
			if (this.GetAvailableStationSupportBudget() < (double)GameSession.CalculateStationUpkeepCost(this._db, this._db.AssetDatabase, stationInfo) * num2 && (this._player.Faction.Name != "hiver" || stationInfo.DesignInfo.StationType != StationType.GATE || this.m_TurnBudget.ProjectedSavings < 500000.0))
				return 0;
			switch (stance)
			{
				case AIStance.HUNKERING:
					return 3 + num1;
				case AIStance.CONQUERING:
					return 3 + num1;
				case AIStance.DESTROYING:
					return 3 + num1;
				case AIStance.DEFENDING:
					return 3 + num1;
				default:
					return 3 + num1;
			}
		}

		private int ScoreTaskForPatrol(StrategicTask task, AIStance stance)
		{
			return this.m_ColonizedSystems.Any<SystemDefendInfo>((Func<SystemDefendInfo, bool>)(x => x.SystemID == task.SystemIDTarget)) && task.EasterEggsAtSystem.Any<EasterEgg>((Func<EasterEgg, bool>)(x => x == EasterEgg.EE_SWARM)) ? 6 : 0;
		}

		private int ScoreTaskForRelocate(StrategicTask task, AIStance stance)
		{
			return 0;
		}

		private int ScoreTaskForInterdict(StrategicTask task, AIStance stance)
		{
			return 0;
		}

		private int ScoreTaskForIntercept(StrategicTask task, AIStance stance)
		{
			return 0;
		}

		private int ScoreTaskForStrike(StrategicTask task, AIStance stance)
		{
			if (task.NumStandardPlayersAtSystem == 0 && task.EasterEggsAtSystem.Any<EasterEgg>((Func<EasterEgg, bool>)(x =>
		   {
			   if (x != EasterEgg.EE_SWARM && x != EasterEgg.EE_MORRIGI_RELIC && (x != EasterEgg.EE_ASTEROID_MONITOR && x != EasterEgg.EE_GARDENERS))
				   return x == EasterEgg.EE_PIRATE_BASE;
			   return true;
		   })))
				return this._player.Faction.CanUseAccelerators() && !this._db.SystemHasAccelerator(task.SystemIDTarget, this._player.ID) && task.UseableFleets.Count != 0 && task.UseableFleets.Min<FleetManagement>((Func<FleetManagement, int>)(x => x.MissionTime.TurnsToTarget)) > 6 ? 0 : 4;
			if (this._player.Faction.CanUseAccelerators() && !this._db.SystemHasAccelerator(task.SystemIDTarget, this._player.ID) && task.UseableFleets.Count != 0)
			{
				if (task.UseableFleets.Min<FleetManagement>((Func<FleetManagement, int>)(x => x.MissionTime.TurnsToTarget)) > 8)
					return 0;
			}
			else if (this._player.Faction.CanUseNodeLine(new bool?()) && task.UseableFleets.Count != 0 && task.UseableFleets.Min<FleetManagement>((Func<FleetManagement, int>)(x => x.MissionTime.TurnsToTarget)) > 8)
				return 0;
			switch (stance)
			{
				case AIStance.CONQUERING:
					return 8;
				case AIStance.DESTROYING:
					return 8;
				case AIStance.DEFENDING:
					return 8;
				default:
					return 0;
			}
		}

		private int ScoreTaskForInvade(StrategicTask task, AIStance stance)
		{
			if (this._player.Faction.CanUseAccelerators() && !this._db.SystemHasAccelerator(task.SystemIDTarget, this._player.ID) && task.UseableFleets.Count != 0)
			{
				if (task.UseableFleets.Min<FleetManagement>((Func<FleetManagement, int>)(x => x.MissionTime.TurnsToTarget)) > 8)
					return 0;
			}
			else if (this._player.Faction.CanUseNodeLine(new bool?()) && task.UseableFleets.Count != 0 && task.UseableFleets.Min<FleetManagement>((Func<FleetManagement, int>)(x => x.MissionTime.TurnsToTarget)) > 8)
				return 0;
			switch (stance)
			{
				case AIStance.CONQUERING:
					return 25;
				case AIStance.DESTROYING:
					return 25;
				case AIStance.DEFENDING:
					return 20;
				default:
					return 0;
			}
		}

		private int GetSubScoreForTask(StrategicTask task, AIStance stance)
		{
			switch (task.Mission)
			{
				case MissionType.COLONIZATION:
					return this.SubScoreTaskForColonize(task, stance);
				case MissionType.SUPPORT:
					return this.SubScoreTaskForSupport(task, stance);
				case MissionType.SURVEY:
					return this.SubScoreTaskForSurvey(task, stance);
				case MissionType.RELOCATION:
					return this.SubScoreTaskForRelocate(task, stance);
				case MissionType.CONSTRUCT_STN:
					return this.SubScoreTaskForConstruction(task, stance);
				case MissionType.UPGRADE_STN:
					return this.SubScoreTaskForUpgrade(task, stance);
				case MissionType.PATROL:
					return this.SubScoreTaskForPatrol(task, stance);
				case MissionType.INTERDICTION:
					return this.SubScoreTaskForInterdict(task, stance);
				case MissionType.STRIKE:
					return this.SubScoreTaskForStrike(task, stance);
				case MissionType.INVASION:
					return this.SubScoreTaskForInvade(task, stance);
				case MissionType.INTERCEPT:
					return this.SubScoreTaskForIntercept(task, stance);
				case MissionType.GATE:
					return this.SubScoreTaskForGate(task, stance);
				case MissionType.DEPLOY_NPG:
					return this.SubScoreTaskForNPG(task, stance);
				default:
					return 0;
			}
		}

		private int SubScoreTaskForSurvey(StrategicTask task, AIStance stance)
		{
			if (task.UseableFleets.Count == 0)
				return -10000;
			double num = 1.0;
			if (task.Mission != MissionType.GATE && this._player.Faction.CanUseGate() && this._db.SystemHasGate(task.SystemIDTarget, this._player.ID))
				num = 0.5;
			else if (task.Mission != MissionType.DEPLOY_NPG && this._player.Faction.CanUseAccelerators() && this._db.SystemHasAccelerator(task.SystemIDTarget, this._player.ID))
				num = 0.5;
			else if (!this._player.Faction.CanUseGate() && !this._player.Faction.CanUseAccelerators())
			{
				if (StarSystemDetailsUI.CollectPlanetListItemsForColonizeMission(this._game.App, task.SystemIDTarget, this._player.ID).ToList<int>().Count == 0)
					num += 0.5;
				if (stance == AIStance.EXPANDING && this._db.GetColonyInfosForSystem(task.SystemIDTarget).Count<ColonyInfo>() > 0)
					++num;
			}
			if (task.Mission == MissionType.GATE || task.Mission == MissionType.DEPLOY_NPG)
			{
				int? systemOwningPlayer = this._db.GetSystemOwningPlayer(task.SystemIDTarget);
				if (systemOwningPlayer.HasValue)
				{
					int id = this._player.ID;
					int? nullable = systemOwningPlayer;
					if ((id != nullable.GetValueOrDefault() ? 1 : (!nullable.HasValue ? 1 : 0)) != 0 && this._db.GetDiplomacyStateBetweenPlayers(this._player.ID, systemOwningPlayer.Value) == DiplomacyState.WAR)
						num += 5.0;
				}
			}
			Vector3 starSystemOrigin = this._db.GetStarSystemOrigin(task.SystemIDTarget);
			float val2 = float.MaxValue;
			foreach (SystemDefendInfo colonizedSystem in this.m_ColonizedSystems)
				val2 = Math.Min((this._db.GetStarSystemOrigin(colonizedSystem.SystemID) - starSystemOrigin).LengthSquared, val2);
			return -(int)(Math.Sqrt((double)val2) * num);
		}

		private int SubScoreTaskForGate(StrategicTask task, AIStance stance)
		{
			return this.SubScoreTaskForSurvey(task, stance);
		}

		private int SubScoreTaskForNPG(StrategicTask task, AIStance stance)
		{
			return this.SubScoreTaskForSurvey(task, stance);
		}

		private int SubScoreTaskForColonize(StrategicTask task, AIStance stance)
		{
			PlanetInfo planetInfo = this._db.GetPlanetInfo(task.PlanetIDTarget);
			float num1 = planetInfo.Size;
			if (this._player.Faction.Name != "hiver" && stance != AIStance.DEFENDING && (stance != AIStance.HUNKERING && stance != AIStance.ARMING) && this._db.GetColonyInfosForSystem(task.SystemIDTarget).Count<ColonyInfo>() == 0)
				num1 *= 5f;
			else if (this._db.GetColonyInfosForSystem(task.SystemIDTarget).Count<ColonyInfo>() > 0)
				num1 *= 2f;
			if (this._game.CombatData.GetCombatsForPlayer(this._game.GameDatabase, this._player.ID, 3).ToList<CombatData>().Count > 0)
				num1 = 0.25f * planetInfo.Size;
			float num2 = 0.0f;
			int stratModifier = this._db.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, this._player.ID);
			if (planetInfo != null && StellarBodyTypes.IsTerrestrial(planetInfo.Type.ToLowerInvariant()))
			{
				if (this._player.Faction.Name == "loa")
				{
					num2 = (float)Math.Max((double)Colony.GetLoaGrowthPotential(this._game, planetInfo.ID, task.SystemIDTarget, this._player.ID) - 0.1 * (this._db.SystemHasAccelerator(task.SystemIDTarget, this._player.ID) ? 0.0 : 1.0), 0.0);
				}
				else
				{
					int num3 = (int)Math.Round((double)this._db.GetPlanetHazardRating(this._player.ID, planetInfo.ID, true));
					num2 = (float)(stratModifier - num3) / (float)stratModifier;
				}
			}
			int num4 = 0;
			int num5 = 0;
			foreach (FleetManagement useableFleet in task.UseableFleets)
			{
				if (useableFleet.Score > 0)
				{
					num5 += useableFleet.MissionTime.TotalTurns;
					++num4;
				}
			}
			if (num4 > 0)
				num5 = (int)Math.Round((double)num5 / (double)num4);
			return (int)((double)num1 * (double)num2 - (double)num5 / 2.0);
		}

		private int SubScoreTaskForSupport(StrategicTask task, AIStance stance)
		{
			return this.SubScoreTaskForColonize(task, stance);
		}

		private int SubScoreTaskForConstruction(StrategicTask task, AIStance stance)
		{
			float num1 = this.GetStationShortFall(stance, task.StationType) * 100f;
			float num2 = 1f;
			List<StationInfo> list = this._db.GetStationForSystemAndPlayer(task.SystemIDTarget, this._player.ID).ToList<StationInfo>();
			if (list.Count<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == task.StationType)) > 0)
				num2 -= 0.5f;
			if (this._game.CombatData.GetCombatsForPlayer(this._game.GameDatabase, this._player.ID, 3).ToList<CombatData>().Count > 0)
				num2 -= 0.5f;
			switch (task.StationType)
			{
				case StationType.NAVAL:
					int num3 = StarSystemDetailsUI.CollectPlanetListItemsForColonizeMission(this._game.App, task.SystemIDTarget, this._player.ID).ToList<int>().Count + this._db.GetColonyInfosForSystem(task.SystemIDTarget).Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == this._player.ID)).ToList<ColonyInfo>().Count;
					num2 += Math.Min((float)num3 / 3f, 1f);
					break;
				case StationType.CIVILIAN:
					int num4 = 0;
					float num5 = 0.0f;
					int stratModifier = this._db.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, this._player.ID);
					foreach (PlanetInfo planetInfo in this._db.GetPlanetInfosOrbitingStar(task.SystemIDTarget).ToList<PlanetInfo>())
					{
						if (StellarBodyTypes.IsTerrestrial(planetInfo.Type.ToLowerInvariant()))
						{
							if (this._player.Faction.Name == "loa")
							{
								float loaGrowthPotential = Colony.GetLoaGrowthPotential(this._game, planetInfo.ID, task.SystemIDTarget, this._player.ID);
								num5 += loaGrowthPotential;
							}
							else
							{
								int num6 = (int)Math.Round((double)this._db.GetPlanetHazardRating(this._player.ID, planetInfo.ID, true));
								num5 += (float)(stratModifier - num6) / (float)stratModifier;
							}
							++num4;
						}
					}
					if (num4 > 0)
						num2 += num5 / (float)num4;
					if (list.Count<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == StationType.NAVAL)) > 0 && list.Count<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == task.StationType)) == 0)
						num2 += 0.5f;
					if (this.m_TurnBudget.CurrentSavings < 1500000.0)
						++num2;
					if (this.m_TurnBudget.NetSavingsIncome < this.m_TurnBudget.NetSavingsLoss)
					{
						num2 += 0.5f;
						break;
					}
					break;
				case StationType.DIPLOMATIC:
					if (this._player.Faction.Name == "zuul")
					{
						num2 += 100f;
						break;
					}
					++num2;
					break;
			}
			return (int)((double)num1 * (double)num2);
		}

		private int SubScoreTaskForUpgrade(StrategicTask task, AIStance stance)
		{
			StationInfo stationInfo = this._db.GetStationInfo(task.StationIDTarget);
			if (stationInfo == null)
				return -10000000;
			float num = -(float)Kerberos.Sots.StarFleet.StarFleet.GetStationConstructionCost(this._game, stationInfo.DesignInfo.StationType, this._player.Faction.Name, stationInfo.DesignInfo.StationLevel + 1);
			if (task.StationType == StationType.NAVAL)
				num *= 0.75f;
			else if (task.StationType == StationType.GATE)
				num *= 0.8f;
			else if (task.StationType == StationType.CIVILIAN)
			{
				if (this.m_TurnBudget.NetSavingsIncome < this.m_TurnBudget.NetSavingsLoss)
					num *= 0.3f;
				else
					num *= 0.8f;
			}
			else if (this._player.Faction.Name == "zuul" && task.StationType == StationType.DIPLOMATIC)
				num *= 0.005f;
			return (int)num;
		}

		private int SubScoreTaskForPatrol(StrategicTask task, AIStance stance)
		{
			return this.SubScoreTaskForInvade(task, stance);
		}

		private int SubScoreTaskForRelocate(StrategicTask task, AIStance stance)
		{
			return 0;
		}

		private int SubScoreTaskForInterdict(StrategicTask task, AIStance stance)
		{
			return 0;
		}

		private int SubScoreTaskForIntercept(StrategicTask task, AIStance stance)
		{
			return 0;
		}

		private int SubScoreTaskForStrike(StrategicTask task, AIStance stance)
		{
			return this.SubScoreTaskForInvade(task, stance);
		}

		private int SubScoreTaskForInvade(StrategicTask task, AIStance stance)
		{
			if (!task.UseableFleets.Any<FleetManagement>((Func<FleetManagement, bool>)(x => x.Fleet.SystemID == task.SystemIDTarget)))
			{
				if (this._player.Faction.CanUseAccelerators())
				{
					if (!this._db.SystemHasAccelerator(task.SystemIDTarget, this._player.ID) && (task.UseableFleets.Count == 0 || task.UseableFleets.Min<FleetManagement>((Func<FleetManagement, int>)(x => x.MissionTime.TurnsToTarget)) > 8))
						return 0;
				}
				else if (this._player.Faction.CanUseGate() && !this._db.SystemHasGate(task.SystemIDTarget, this._player.ID))
					return 0;
			}
			if (task.EnemyStrength <= 0)
				return 2;
			if (task.EasterEggsAtSystem.Contains(EasterEgg.GM_SYSTEM_KILLER))
				return -(int)((double)task.EnemyStrength * 2.0);
			if (task.EasterEggsAtSystem.Contains(EasterEgg.GM_LOCUST_SWARM))
				return -(int)((double)task.EnemyStrength * 1.25);
			if (task.EasterEggsAtSystem.Any<EasterEgg>((Func<EasterEgg, bool>)(x =>
		   {
			   if (x != EasterEgg.EE_SWARM && x != EasterEgg.EE_MORRIGI_RELIC && (x != EasterEgg.EE_ASTEROID_MONITOR && x != EasterEgg.EE_GARDENERS))
				   return x == EasterEgg.EE_PIRATE_BASE;
			   return true;
		   })))
				return 1;
			return -task.EnemyStrength;
		}

		private int GetScoreForFleet(StrategicTask task, FleetManagement fleet)
		{
			if ((task.RequiredFleetTypes & fleet.FleetTypes) == FleetTypeFlags.UNKNOWN || this.m_AvailableShortFleets.Any<FleetInfo>((Func<FleetInfo, bool>)(x => x == fleet.Fleet)) || fleet.MissionTime.TurnsToTarget > 20 || this.m_DefenseCombatFleets.Any<FleetInfo>((Func<FleetInfo, bool>)(x => x == fleet.Fleet)) && fleet.MissionTime.TurnsToTarget > 5)
				return 0;
			if (this._player.Faction.Name == "loa")
				return Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._game, fleet.Fleet.ID) / (1 + fleet.MissionTime.TurnsToTarget);
			switch (task.Mission)
			{
				case MissionType.COLONIZATION:
					return this.ScoreFleetForColonize(task, fleet);
				case MissionType.SUPPORT:
					return this.ScoreFleetForSupport(task, fleet);
				case MissionType.SURVEY:
					return this.ScoreFleetForSurvey(task, fleet);
				case MissionType.RELOCATION:
					return this.ScoreFleetForRelocate(task, fleet);
				case MissionType.CONSTRUCT_STN:
					return this.ScoreFleetForConstruction(task, fleet);
				case MissionType.UPGRADE_STN:
					return this.ScoreFleetForUpgrade(task, fleet);
				case MissionType.PATROL:
					return this.ScoreFleetForPatrol(task, fleet);
				case MissionType.INTERDICTION:
					return this.ScoreFleetForInterdict(task, fleet);
				case MissionType.STRIKE:
					return this.ScoreFleetForStrike(task, fleet);
				case MissionType.INVASION:
					return this.ScoreFleetForInvade(task, fleet);
				case MissionType.INTERCEPT:
					return this.ScoreFleetForIntercept(task, fleet);
				case MissionType.GATE:
					return this.ScoreFleetForGate(task, fleet);
				case MissionType.DEPLOY_NPG:
					return this.ScoreFleetForNPG(task, fleet);
				default:
					return 0;
			}
		}

		private int ScoreFleetForSurvey(StrategicTask task, FleetManagement fleet)
		{
			if (fleet.MissionTime.TurnsAtTarget == 0)
				return 1;
			return -fleet.MissionTime.TurnsToTarget;
		}

		private int ScoreFleetForGate(StrategicTask task, FleetManagement fleet)
		{
			List<ShipInfo> list = this._game.GameDatabase.GetShipInfoByFleetID(fleet.Fleet.ID, false).ToList<ShipInfo>();
			if (!list.Any<ShipInfo>() || list.FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.Role == ShipRole.GATE)) == null)
				return 0;
			return -fleet.MissionTime.TurnsToTarget;
		}

		private int ScoreFleetForNPG(StrategicTask task, FleetManagement fleet)
		{
			return -fleet.MissionTime.TurnsToTarget;
		}

		private int ScoreFleetForColonize(StrategicTask task, FleetManagement fleet)
		{
			return (int)Math.Round(60.0 * (1.0 - (double)Math.Min((float)fleet.MissionTime.TotalTurns / 25f, 1f)) + (double)(40f * Math.Min((float)Kerberos.Sots.StarFleet.StarFleet.GetColonizationSpace(this._game, fleet.Fleet.ID) / 500f, 1f)));
		}

		private int ScoreFleetForSupport(StrategicTask task, FleetManagement fleet)
		{
			return (int)Math.Round(Kerberos.Sots.StarFleet.StarFleet.GetColonizationSpace(this._game, fleet.Fleet.ID));
		}

		private int ScoreFleetForConstruction(StrategicTask task, FleetManagement fleet)
		{
			return (int)Math.Round((double)Kerberos.Sots.StarFleet.StarFleet.GetConstructionPointsForFleet(this._game, fleet.Fleet.ID));
		}

		private int ScoreFleetForUpgrade(StrategicTask task, FleetManagement fleet)
		{
			return (int)Math.Round((double)Kerberos.Sots.StarFleet.StarFleet.GetConstructionPointsForFleet(this._game, fleet.Fleet.ID));
		}

		private int ScoreFleetForPatrol(StrategicTask task, FleetManagement fleet)
		{
			return fleet.FleetStrength;
		}

		private int ScoreFleetForRelocate(StrategicTask task, FleetManagement fleet)
		{
			return fleet.FleetStrength;
		}

		private int ScoreFleetForInterdict(StrategicTask task, FleetManagement fleet)
		{
			return fleet.FleetStrength;
		}

		private int ScoreFleetForIntercept(StrategicTask task, FleetManagement fleet)
		{
			float fleetTravelSpeed1 = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this._game, fleet.Fleet.ID, false);
			float fleetTravelSpeed2 = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this._game, fleet.Fleet.ID, true);
			float num = (double)fleetTravelSpeed2 <= 0.0 ? fleetTravelSpeed1 : fleetTravelSpeed2;
			return fleet.FleetStrength + (int)num;
		}

		private int ScoreFleetForStrike(StrategicTask task, FleetManagement fleet)
		{
			if ((fleet.FleetTypes & FleetTypeFlags.SURVEY) != FleetTypeFlags.UNKNOWN && this.m_AvailableTasks.Count<StrategicTask>((Func<StrategicTask, bool>)(x =>
		   {
			   if (x.Mission == MissionType.SURVEY)
				   return x.UseableFleets.Any<FleetManagement>((Func<FleetManagement, bool>)(y => y.Fleet == fleet.Fleet));
			   return false;
		   })) > 0)
			{
				List<AIFleetInfo> list = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
				AIFleetInfo aiFleet = list.FirstOrDefault<AIFleetInfo>((Func<AIFleetInfo, bool>)(x =>
			   {
				   int? fleetId = x.FleetID;
				   int id = fleet.Fleet.ID;
				   if (fleetId.GetValueOrDefault() == id)
					   return fleetId.HasValue;
				   return false;
			   }));
				if (aiFleet == null)
					return 0;
				this._db.GetAIInfo(this._player.ID);
				int num = (int)Math.Ceiling((double)this._game.AssetDatabase.FleetTemplates.First<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == aiFleet.FleetTemplate)).MinFleetsForStance[this._db.GetAIInfo(this._player.ID).Stance] / 2.0);
				if (list.Count<AIFleetInfo>((Func<AIFleetInfo, bool>)(x => x.FleetTemplate == aiFleet.FleetTemplate)) < num)
					return 0;
			}
			return fleet.FleetStrength;
		}

		private int ScoreFleetForInvade(StrategicTask task, FleetManagement fleet)
		{
			return fleet.FleetStrength;
		}

		private bool IsSimilarMission(MissionType mt1, MissionType mt2)
		{
			if (mt1 == mt2)
				return true;
			if (mt1 == MissionType.CONSTRUCT_STN)
				return mt2 == MissionType.UPGRADE_STN;
			if (mt1 == MissionType.UPGRADE_STN)
				return mt2 == MissionType.CONSTRUCT_STN;
			return false;
		}

		private StrategicTask PickBestTaskForFleet(
		  FleetInfo fleet,
		  List<int> fleetsAssigned)
		{
			int num1 = int.MaxValue;
			StrategicTask strategicTask = (StrategicTask)null;
			foreach (StrategicTask availableTask in this.m_AvailableTasks)
			{
				FleetManagement fleetManagement1 = availableTask.UseableFleets.FirstOrDefault<FleetManagement>((Func<FleetManagement, bool>)(x => x.Fleet == fleet));
				if (availableTask.NumFleetsRequested > 0 && availableTask.UseableFleets.Count >= availableTask.NumFleetsRequested && fleetManagement1 != null)
				{
					FleetManagement fleetManagement2 = availableTask.UseableFleets.FirstOrDefault<FleetManagement>((Func<FleetManagement, bool>)(x => !fleetsAssigned.Any<int>((Func<int, bool>)(y => y == x.Fleet.ID))));
					if (fleetManagement2 == null || fleetManagement2.Score <= fleetManagement1.Score)
					{
						int num2 = strategicTask == null ? 1 : (strategicTask.Score < availableTask.Score ? 1 : 0);
						bool flag1 = strategicTask != null && this.IsSimilarMission(strategicTask.Mission, availableTask.Mission) && strategicTask.Score < availableTask.Score;
						bool flag2 = strategicTask != null && strategicTask.SubScore < availableTask.SubScore;
						bool flag3 = strategicTask != null && strategicTask.SubScore == availableTask.SubScore && fleetManagement1.MissionTime.TurnsToTarget < num1;
						if (num2 != 0 || flag1 && flag2 | flag3)
						{
							strategicTask = availableTask;
							int score = fleetManagement1.Score;
							num1 = fleetManagement1.MissionTime.TurnsToTarget;
						}
					}
				}
			}
			return strategicTask;
		}

		private StrategicTask PickBestRelocationTaskForFleet(
		  FleetInfo fleet,
		  StrategicTask desiredTask = null,
		  bool requiresReserve = false)
		{
			if (this.m_RelocationTasks.Count == 0 || fleet.SupportingSystemID == 0)
				return (StrategicTask)null;
			FleetTypeFlags fleetFlags = FleetManagement.GetFleetTypeFlags(this._game.App, fleet.ID, this._player.ID, this._player.Faction.Name == "loa");
			int cruiserEquivalent = this._db.GetFleetCruiserEquivalent(fleet.ID);
			StrategicTask strategicTask = (StrategicTask)null;
			if (desiredTask == null)
			{
				Vector3 starSystemOrigin = this._db.GetStarSystemOrigin(fleet.SupportingSystemID);
				double supportRange = (double)GameSession.GetSupportRange(this._db.AssetDatabase, this._db, this._player.ID);
				int num1 = 0;
				if (!this.m_AvailableShortFleets.Contains(fleet))
				{
					foreach (StrategicTask availableTask in this.m_AvailableTasks)
					{
						if ((availableTask.RequiredFleetTypes & fleetFlags) != FleetTypeFlags.UNKNOWN)
							++num1;
					}
				}
				int num2 = int.MaxValue;
				float num3 = float.MaxValue;
				foreach (KeyValuePair<StrategicTask, List<StrategicTask>> relocationTask in this.m_RelocationTasks)
				{
					FleetManagement fleetManagement = relocationTask.Key.UseableFleets.FirstOrDefault<FleetManagement>((Func<FleetManagement, bool>)(x => x.Fleet == fleet));
					if (relocationTask.Key.SystemIDTarget == fleet.SupportingSystemID || fleetManagement != null && cruiserEquivalent <= relocationTask.Key.SupportPointsAtSystem)
					{
						int num4 = 0;
						if (!this.m_AvailableShortFleets.Contains(fleet))
						{
							num4 = relocationTask.Value.Count<StrategicTask>((Func<StrategicTask, bool>)(x => (uint)(x.RequiredFleetTypes & fleetFlags) > 0U));
							if (num4 == 0)
								continue;
						}
						else if (fleet.SystemID == relocationTask.Key.StationIDTarget)
							continue;
						if (!requiresReserve || this._game.GameDatabase.GetReserveFleetID(this._player.ID, fleet.SystemID).HasValue)
						{
							int num5 = fleetManagement != null ? fleetManagement.MissionTime.TurnsToTarget : int.MaxValue;
							float lengthSquared = (this._db.GetStarSystemOrigin(relocationTask.Key.SystemIDTarget) - starSystemOrigin).LengthSquared;
							if (num1 == 0 || num5 < num2 || num5 == num2 && num4 > num1 + 3 || num5 == num2 && num4 == num1 && (double)lengthSquared < (double)num3)
							{
								num3 = lengthSquared;
								num1 = relocationTask.Value.Count;
								strategicTask = relocationTask.Key;
								num2 = num5;
							}
						}
					}
				}
			}
			else
			{
				FleetManagement fleetManagement1 = desiredTask.UseableFleets.FirstOrDefault<FleetManagement>((Func<FleetManagement, bool>)(x => x.Fleet == fleet));
				int num1 = fleetManagement1.MissionTime.TurnsToTarget;
				Vector3 starSystemOrigin1 = this._db.GetStarSystemOrigin(desiredTask.SystemIDTarget);
				Vector3 starSystemOrigin2 = this._db.GetStarSystemOrigin(fleet.SupportingSystemID);
				float num2 = (starSystemOrigin1 - starSystemOrigin2).LengthSquared;
				foreach (KeyValuePair<StrategicTask, List<StrategicTask>> relocationTask in this.m_RelocationTasks)
				{
					FleetManagement fleetManagement2 = relocationTask.Key.UseableFleets.FirstOrDefault<FleetManagement>((Func<FleetManagement, bool>)(x => x.Fleet == fleet));
					if (fleetManagement2 != null && fleetManagement2.Fleet == fleet && cruiserEquivalent <= relocationTask.Key.SupportPointsAtSystem && (relocationTask.Value.Any<StrategicTask>((Func<StrategicTask, bool>)(x => x == desiredTask)) && relocationTask.Key.SystemIDTarget != fleet.SupportingSystemID))
					{
						float? travelSpeed = new float?();
						float? nodeTravelSpeed = new float?();
						FleetRangeData fleetRangeData;
						if (this.m_FleetRanges.TryGetValue(fleet, out fleetRangeData))
						{
							double fleetRange = (double)fleetRangeData.FleetRange;
							travelSpeed = fleetRangeData.FleetTravelSpeed;
							nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
						}
						int num3 = relocationTask.Key.SystemIDTarget != desiredTask.SystemIDTarget ? 1 : 0;
						if (relocationTask.Key.SystemIDTarget != desiredTask.SystemIDTarget)
						{
							int? travelTime = Kerberos.Sots.StarFleet.StarFleet.GetTravelTime(this._game, fleet, relocationTask.Key.SystemIDTarget, desiredTask.SystemIDTarget, false, travelSpeed, nodeTravelSpeed);
							if (travelTime.HasValue)
								num3 = travelTime.Value;
						}
						if (fleetManagement1.MissionTime.TripsTillSelfSufficeincy == 0)
							num3 += fleetManagement2.MissionTime.TurnsToTarget;
						float lengthSquared = (this._db.GetStarSystemOrigin(relocationTask.Key.SystemIDTarget) - starSystemOrigin1).LengthSquared;
						if (num3 < num1 || num3 == num1 && (double)lengthSquared < (double)num2)
						{
							num1 = num3;
							strategicTask = relocationTask.Key;
							num2 = lengthSquared;
						}
					}
				}
			}
			if (strategicTask != null && strategicTask.SystemIDTarget == fleet.SupportingSystemID)
				strategicTask = (StrategicTask)null;
			return strategicTask;
		}

		private void AssignDefenseFleetsToSystem()
		{
			if (this.m_ColonizedSystems.Count == 0 || this.m_DefenseCombatFleets.Count == 0)
				return;
			Dictionary<int, List<FleetInfo>> dictionary = new Dictionary<int, List<FleetInfo>>();
			foreach (SystemDefendInfo colonizedSystem in this.m_ColonizedSystems)
			{
				foreach (MoveOrderInfo moveOrderInfo in this._db.GetMoveOrdersByDestinationSystem(colonizedSystem.SystemID).ToList<MoveOrderInfo>())
				{
					FleetInfo fleetInfo = this._db.GetFleetInfo(moveOrderInfo.FleetID);
					if (fleetInfo != null && this._db.GetDiplomacyStateBetweenPlayers(this._player.ID, fleetInfo.PlayerID) == DiplomacyState.WAR)
					{
						if (!dictionary.ContainsKey(colonizedSystem.SystemID))
							dictionary.Add(colonizedSystem.SystemID, new List<FleetInfo>());
						if (!dictionary[colonizedSystem.SystemID].Contains(fleetInfo))
							dictionary[colonizedSystem.SystemID].Add(fleetInfo);
					}
				}
				foreach (FleetInfo fleetInfo in this._db.GetFleetInfoBySystemID(colonizedSystem.SystemID, FleetType.FL_NORMAL).ToList<FleetInfo>())
				{
					if (this._db.GetDiplomacyStateBetweenPlayers(this._player.ID, fleetInfo.PlayerID) == DiplomacyState.WAR)
					{
						if (!dictionary.ContainsKey(colonizedSystem.SystemID))
							dictionary.Add(colonizedSystem.SystemID, new List<FleetInfo>());
						if (!dictionary[colonizedSystem.SystemID].Contains(fleetInfo))
							dictionary[colonizedSystem.SystemID].Add(fleetInfo);
					}
				}
			}
			using (List<SystemDefendInfo>.Enumerator enumerator = this.m_ColonizedSystems.GetEnumerator())
			{
			label_56:
				while (enumerator.MoveNext())
				{
					SystemDefendInfo sdi = enumerator.Current;
					List<FleetInfo> fleetInfoList1;
					if (dictionary.TryGetValue(sdi.SystemID, out fleetInfoList1))
					{
						int num1 = this.m_DefenseCombatFleets.Where<FleetInfo>((Func<FleetInfo, bool>)(x => x.SupportingSystemID == sdi.SystemID)).Sum<FleetInfo>((Func<FleetInfo, int>)(x => this.GetFleetStrength(x)));
						foreach (MissionInfo missionInfo in this._db.GetMissionsBySystemDest(sdi.SystemID).Where<MissionInfo>((Func<MissionInfo, bool>)(x => x.Type == MissionType.RELOCATION)).ToList<MissionInfo>())
						{
							FleetInfo fleetInfo = this._db.GetFleetInfo(missionInfo.FleetID);
							if (fleetInfo != null && fleetInfo.PlayerID == this._player.ID)
								num1 += this.GetFleetStrength(fleetInfo);
						}
						this.m_DefenseCombatFleets.RemoveAll((Predicate<FleetInfo>)(x => x.SupportingSystemID == sdi.SystemID));
						int num2 = 0;
						int val2 = int.MaxValue;
						foreach (FleetInfo fleet in fleetInfoList1)
						{
							num2 += this.GetFleetStrength(fleet);
							MoveOrderInfo orderInfoByFleetId = this._db.GetMoveOrderInfoByFleetID(fleet.ID);
							int num3 = orderInfoByFleetId != null ? Math.Min(this._game.GetArrivalTurns(orderInfoByFleetId, fleet.ID), val2) : 0;
							if (num3 < val2)
								val2 = num3;
						}
						int num4 = num2 - num1;
						if (num4 <= 0)
							break;
						while (true)
						{
							if (num4 > 0 && this.m_DefenseCombatFleets.Count > 0)
							{
								int num3 = 0;
								int num5 = int.MaxValue;
								int num6 = int.MaxValue;
								FleetInfo fleetInfo = (FleetInfo)null;
								foreach (FleetInfo defenseCombatFleet in this.m_DefenseCombatFleets)
								{
									List<FleetInfo> fleetInfoList2;
									if (!dictionary.TryGetValue(defenseCombatFleet.SupportingSystemID, out fleetInfoList2) || fleetInfoList2.Count <= 0)
									{
										int fleetStrength = this.GetFleetStrength(defenseCombatFleet);
										int num7 = num4 - fleetStrength;
										int num8 = Math.Abs(num7);
										if (num8 < Math.Abs(num5) || num8 == Math.Abs(num5) && num5 < 0 && num7 > 0)
										{
											MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.RELOCATION, StationType.INVALID_TYPE, defenseCombatFleet.ID, sdi.SystemID, 0, (List<int>)null, 1, false, new float?(), new float?());
											if (val2 == 0 && missionEstimate.TurnsToTarget < num6 || missionEstimate.TurnsToTarget < val2 && missionEstimate.TurnsToTarget < num6)
											{
												fleetInfo = defenseCombatFleet;
												num5 = num7;
												num6 = missionEstimate.TurnsToTarget;
												num3 = fleetStrength;
											}
										}
									}
								}
								if (fleetInfo != null)
								{
									MissionInfo missionByFleetId = this._db.GetMissionByFleetID(fleetInfo.ID);
									if (missionByFleetId != null && missionByFleetId.Type != MissionType.RELOCATION && missionByFleetId.TargetSystemID != sdi.SystemID)
										Kerberos.Sots.StarFleet.StarFleet.CancelMission(this._game, fleetInfo, true);
									else if (fleetInfo.SupportingSystemID != sdi.SystemID)
										this.AssignFleetToTask(new StrategicTask()
										{
											Mission = MissionType.RELOCATION,
											SystemIDTarget = sdi.SystemID
										}, fleetInfo, new int?());
									this.m_DefenseCombatFleets.Remove(fleetInfo);
									num4 -= num3;
								}
								else
									goto label_56;
							}
							else
								goto label_56;
						}
					}
				}
			}
		}

		private int GetEnemyFleetStrength(int systemID)
		{
			int num = 0;
			List<FleetInfo> list = this._game.GameDatabase.GetFleetInfoBySystemID(systemID, FleetType.FL_NORMAL | FleetType.FL_DEFENSE).Where<FleetInfo>((Func<FleetInfo, bool>)(x => this._game.GameDatabase.GetDiplomacyStateBetweenPlayers(this._player.ID, x.PlayerID) == DiplomacyState.WAR)).ToList<FleetInfo>();
			if (list.Count > 0)
			{
				foreach (FleetInfo fleet in list)
					num += this.GetFleetStrength(fleet);
			}
			return num;
		}

		private int GetNumStandardPlayersAtSystem(StrategicTask task, List<FleetInfo> fleetsAtSystem)
		{
			int num = 0;
			List<int> intList = new List<int>();
			foreach (FleetInfo fleetInfo in fleetsAtSystem)
			{
				if (!intList.Contains(fleetInfo.PlayerID))
				{
					PlayerInfo playerInfo = this._db.GetPlayerInfo(fleetInfo.PlayerID);
					if (playerInfo.isStandardPlayer)
						++num;
					intList.Add(playerInfo.ID);
				}
			}
			foreach (ColonyInfo colonyInfo in this._db.GetColonyInfosForSystem(task.SystemIDTarget).ToList<ColonyInfo>())
			{
				if (!intList.Contains(colonyInfo.PlayerID))
				{
					PlayerInfo playerInfo = this._db.GetPlayerInfo(colonyInfo.PlayerID);
					if (playerInfo.isStandardPlayer)
						++num;
					intList.Add(playerInfo.ID);
				}
			}
			return num;
		}

		private List<EasterEgg> GetEncountersAtSystem(
		  int systemID,
		  List<FleetInfo> enemyFleets)
		{
			List<EasterEgg> easterEggList = new List<EasterEgg>();
			if (enemyFleets.Count > 0)
			{
				foreach (FleetInfo enemyFleet in enemyFleets)
				{
					if (this._game.ScriptModules.IsEncounterPlayer(enemyFleet.PlayerID))
					{
						EasterEgg eggTypeForPlayer = this._game.ScriptModules.GetEasterEggTypeForPlayer(enemyFleet.PlayerID);
						if (eggTypeForPlayer != EasterEgg.UNKNOWN && !easterEggList.Contains(eggTypeForPlayer))
							easterEggList.Add(eggTypeForPlayer);
					}
				}
			}
			List<StationInfo> list = this._db.GetStationForSystem(systemID).ToList<StationInfo>();
			if (list.Count > 0)
			{
				foreach (StationInfo stationInfo in list)
				{
					if (this._game.ScriptModules.IsEncounterPlayer(stationInfo.PlayerID))
					{
						EasterEgg eggTypeForPlayer = this._game.ScriptModules.GetEasterEggTypeForPlayer(stationInfo.PlayerID);
						if (eggTypeForPlayer != EasterEgg.UNKNOWN && !easterEggList.Contains(eggTypeForPlayer))
							easterEggList.Add(eggTypeForPlayer);
					}
				}
			}
			return easterEggList;
		}

		private int GetFleetStrength(FleetInfo fleet)
		{
			if (this._player.Faction.Name == "loa" && fleet.PlayerID == this._player.ID && fleet.Type != FleetType.FL_DEFENSE || fleet.PlayerID != this._player.ID && this._db.GetFactionName(this._db.GetFleetFaction(fleet.ID)) == "loa" && fleet.Type != FleetType.FL_DEFENSE)
				return Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._game, fleet.ID) / 15000 * 3;
			int num = 0;
			foreach (ShipInfo shipInfo in this._game.GameDatabase.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>())
			{
				if (fleet.Type != FleetType.FL_DEFENSE || this._game.GameDatabase.GetShipSystemPosition(shipInfo.ID).HasValue)
					num += CombatAI.GetShipStrength(shipInfo.DesignInfo.Class);
			}
			return num;
		}

		private int GetNumFleetsRequestForTask(StrategicTask task)
		{
			int num1 = 1;
			if (task.Mission == MissionType.INVASION || task.Mission == MissionType.STRIKE)
			{
				if (this._game.GameDatabase.GetColonyInfosForSystem(task.SystemIDTarget).Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => this._game.GameDatabase.GetDiplomacyStateBetweenPlayers(this._player.ID, x.PlayerID) == DiplomacyState.WAR)).ToList<ColonyInfo>().Count > 2)
					++num1;
				int num2 = 0;
				if (task.UseableFleets.Count > 0)
					num2 = task.UseableFleets.Sum<FleetManagement>((Func<FleetManagement, int>)(x => x.FleetStrength));
				if (task.EnemyStrength > num2)
					++num1;
				if (task.EnemyStrength > num2 * 2)
					++num1;
				if (this._player.Faction.Name == "loa")
					++num1;
			}
			List<MissionInfo> list = this._game.GameDatabase.GetMissionsBySystemDest(task.SystemIDTarget).ToList<MissionInfo>();
			list.RemoveAll((Predicate<MissionInfo>)(x => this._game.GameDatabase.GetFleetInfo(x.FleetID).PlayerID != this._player.ID));
			return Math.Min(task.Mission == MissionType.CONSTRUCT_STN || task.Mission == MissionType.UPGRADE_STN ? Math.Max(num1 - list.Count<MissionInfo>((Func<MissionInfo, bool>)(x =>
		   {
			   if (x.Type != MissionType.CONSTRUCT_STN)
				   return x.Type == MissionType.UPGRADE_STN;
			   return true;
		   })), 0) : Math.Max(num1 - list.Count<MissionInfo>((Func<MissionInfo, bool>)(x => x.Type == task.Mission)), 0), 3);
		}

		public static FleetTemplate GetTemplateForFleet(
		  GameSession game,
		  int playerID,
		  int fleetID)
		{
			FleetTemplate fleetTemplate = (FleetTemplate)null;
			AIFleetInfo aiFleetInfo = game.GameDatabase.GetAIFleetInfos(playerID).FirstOrDefault<AIFleetInfo>((Func<AIFleetInfo, bool>)(x =>
		   {
			   int? fleetId = x.FleetID;
			   int num = fleetID;
			   if (fleetId.GetValueOrDefault() == num)
				   return fleetId.HasValue;
			   return false;
		   }));
			if (aiFleetInfo == null)
			{
				string templateName = DesignLab.DeduceFleetTemplate(game.GameDatabase, game, fleetID);
				if (!string.IsNullOrEmpty(templateName))
					fleetTemplate = game.AssetDatabase.FleetTemplates.First<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == templateName));
			}
			if (fleetTemplate == null)
				fleetTemplate = game.AssetDatabase.FleetTemplates.FirstOrDefault<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == aiFleetInfo.FleetTemplate));
			return fleetTemplate;
		}

		private List<ShipInclude> GetRequiredShipIncludes(FleetTemplate template)
		{
			List<ShipInclude> list1 = template.ShipIncludes.Where<ShipInclude>((Func<ShipInclude, bool>)(x => x.InclusionType == ShipInclusionType.REQUIRED)).ToList<ShipInclude>();
			if (template.MissionTypes.Contains(MissionType.GATE) && this._player.Faction.CanUseGate())
			{
				float num = 0.0f;
				List<FleetInfo> list2 = this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).Where<FleetInfo>((Func<FleetInfo, bool>)(x => this._db.GetShipInfoByFleetID(x.ID, true).Any<ShipInfo>((Func<ShipInfo, bool>)(y => y.DesignInfo.Role == ShipRole.GATE)))).ToList<FleetInfo>();
				if (list2.Count > 0)
				{
					foreach (FleetInfo fleetInfo in list2)
						num = Math.Max((float)Math.Max(Kerberos.Sots.StarFleet.StarFleet.GetFleetEndurance(this._game, fleetInfo.ID) - 1, 0) * Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this._game, fleetInfo.ID, false), num);
					bool flag = false;
					foreach (SystemDefendInfo colonizedSystem in this.m_ColonizedSystems)
					{
						flag = this._db.GetSystemsInRange(this._db.GetStarSystemOrigin(colonizedSystem.SystemID), num).Any<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => !this._db.SystemHasGate(x.ID, this._player.ID)));
						if (flag)
							break;
					}
					if (!flag)
					{
						ShipInclude shipInclude = list1.FirstOrDefault<ShipInclude>((Func<ShipInclude, bool>)(x => x.ShipRole == ShipRole.SUPPLY));
						if (shipInclude != null)
							shipInclude.Amount = 3;
						else
							list1.Add(new ShipInclude()
							{
								Amount = 1,
								Faction = this._player.Faction.Name,
								InclusionType = ShipInclusionType.REQUIRED,
								WeaponRole = new WeaponRole?()
							});
					}
				}
			}
			return list1;
		}

		private bool FleetRequiresFill(FleetInfo fleet)
		{
			if (fleet == null)
				return false;
			List<AIFleetInfo> list1 = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
			FleetTemplate template = (FleetTemplate)null;
			AIFleetInfo aiFleetInfo = list1.FirstOrDefault<AIFleetInfo>((Func<AIFleetInfo, bool>)(x =>
		   {
			   int? fleetId = x.FleetID;
			   int id = fleet.ID;
			   return fleetId.GetValueOrDefault() == id & fleetId.HasValue;
		   }));
			if (aiFleetInfo != null && aiFleetInfo.InvoiceID.HasValue)
			{
				int? invoiceId = aiFleetInfo.InvoiceID;
				int num = 0;
				if (!(invoiceId.GetValueOrDefault() == num & invoiceId.HasValue))
					return false;
			}
			if (this._player.Faction.CanUseAccelerators())
				return (double)Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._game, fleet.ID) / (double)this.m_LoaLimit < 0.5;
			if (aiFleetInfo == null)
			{
				string templateName = DesignLab.DeduceFleetTemplate(this._db, this._game, fleet.ID);
				if (!string.IsNullOrEmpty(templateName))
				{
					template = this._db.AssetDatabase.FleetTemplates.First<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == templateName));
					AIFleetInfo fleetInfo = new AIFleetInfo()
					{
						AdmiralID = new int?(fleet.AdmiralID),
						FleetType = MissionTypeExtensions.SerializeList(template.MissionTypes),
						SystemID = fleet.SupportingSystemID,
						FleetTemplate = templateName
					};
					fleetInfo.ID = this._db.InsertAIFleetInfo(this._player.ID, fleetInfo);
					aiFleetInfo = fleetInfo;
				}
			}
			if (template == null)
				template = this._db.AssetDatabase.FleetTemplates.FirstOrDefault<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == aiFleetInfo.FleetTemplate));
			int num1 = 0;
			int num2 = 0;
			List<ShipInfo> list2 = this._game.GameDatabase.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>();
			foreach (ShipInclude requiredShipInclude in this.GetRequiredShipIncludes(template))
			{
				ShipInclude include = requiredShipInclude;
				if ((string.IsNullOrEmpty(include.Faction) || !(include.Faction != this._player.Faction.Name)) && !(include.FactionExclusion == this._player.Faction.Name))
				{
					num2 += include.Amount;
					int num3 = list2.Count<ShipInfo>((Func<ShipInfo, bool>)(x => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, include.ShipRole)));
					if (num3 < include.Amount)
					{
						int amount = include.Amount;
						num1 += include.Amount - num3;
					}
				}
			}
			float num4 = 0.3f;
			bool flag1 = (double)num1 / (double)num2 > (double)num4;
			if (!flag1)
			{
				bool flag2 = false;
				foreach (ShipInclude shipInclude in template.ShipIncludes.Where<ShipInclude>((Func<ShipInclude, bool>)(x => x.InclusionType == ShipInclusionType.FILL)).ToList<ShipInclude>())
				{
					if ((string.IsNullOrEmpty(shipInclude.Faction) || !(shipInclude.Faction != this._player.Faction.Name)) && !(shipInclude.FactionExclusion == this._player.Faction.Name))
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					int fleetCommandPoints = Kerberos.Sots.StarFleet.StarFleet.GetFleetCommandPoints(this._game.App, fleet.ID, (IEnumerable<int>)null);
					if ((double)Kerberos.Sots.StarFleet.StarFleet.GetFleetCommandCost(this._game.App, fleet.ID, (IEnumerable<int>)null) / (double)fleetCommandPoints < 0.5)
						flag1 = true;
				}
			}
			return flag1;
		}

		private void TransferCubesFromReserve(FleetInfo fleet)
		{
			if (fleet == null || fleet.SystemID == 0)
				return;
			int? nullable;
			if (this._db.GetMissionByFleetID(fleet.ID) != null)
			{
				nullable = this._db.GetSystemOwningPlayer(this._db.GetMissionByFleetID(fleet.ID).TargetSystemID);
				int id = this._player.ID;
				if (!(nullable.GetValueOrDefault() == id & nullable.HasValue))
					return;
			}
			this._db.GetMissionByFleetID(fleet.ID);
			int? reserveFleetId = this._game.GameDatabase.GetReserveFleetID(this._player.ID, fleet.SystemID);
			if (!reserveFleetId.HasValue)
				return;
			int remainingCubes = this.m_LoaLimit - Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._game, fleet.ID);
			if (remainingCubes <= 0)
				return;
			List<ShipInfo> list = this._game.GameDatabase.GetShipInfoByFleetID(reserveFleetId.Value, true).Where<ShipInfo>((Func<ShipInfo, bool>)(x =>
		   {
			   if (x.AIFleetID.HasValue)
			   {
				   int? aiFleetId = x.AIFleetID;
				   int num = 0;
				   if (!(aiFleetId.GetValueOrDefault() == num & aiFleetId.HasValue))
					   return false;
			   }
			   return true;
		   })).ToList<ShipInfo>();
			while (remainingCubes > 0)
			{
				ShipInfo shipInfo1 = list.FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x =>
			   {
				   if (Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, x.ID) <= remainingCubes && x.DesignInfo.IsLoaCube())
					   return true;
				   if (Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, x.ID) > remainingCubes)
					   return x.DesignInfo.IsLoaCube();
				   return false;
			   }));
				if (shipInfo1 == null)
					break;
				if (Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, shipInfo1.ID) <= remainingCubes)
				{
					ShipInfo shipInfo2 = this._db.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>().FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.IsLoaCube()));
					if (shipInfo2 != null)
					{
						this._db.UpdateShipLoaCubes(shipInfo2.ID, Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, shipInfo2.ID) + Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, shipInfo1.ID));
					}
					else
					{
						GameDatabase db = this._db;
						int id1 = fleet.ID;
						int id2 = this._db.GetDesignInfosForPlayer(fleet.PlayerID).FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.IsLoaCube())).ID;
						nullable = new int?();
						int? aiFleetID = nullable;
						int shipLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, shipInfo1.ID);
						db.InsertShip(id1, id2, "Cube", (ShipParams)0, aiFleetID, shipLoaCubeValue);
					}
					remainingCubes -= Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, shipInfo1.ID);
					this._db.RemoveShip(shipInfo1.ID);
					list.Remove(shipInfo1);
				}
				else
				{
					this._db.UpdateShipLoaCubes(shipInfo1.ID, Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, shipInfo1.ID) - remainingCubes);
					ShipInfo shipInfo2 = this._db.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>().FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.IsLoaCube()));
					if (shipInfo2 != null)
					{
						this._db.UpdateShipLoaCubes(shipInfo2.ID, Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, shipInfo2.ID) + remainingCubes);
					}
					else
					{
						GameDatabase db = this._db;
						int id1 = fleet.ID;
						int id2 = this._db.GetDesignInfosForPlayer(fleet.PlayerID).FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.IsLoaCube())).ID;
						nullable = new int?();
						int? aiFleetID = nullable;
						int Loacubes = remainingCubes;
						db.InsertShip(id1, id2, "Cube", (ShipParams)0, aiFleetID, Loacubes);
					}
					remainingCubes = 0;
				}
			}
		}

		private void FixFullFleet(FleetInfo fleet)
		{
			if (fleet == null)
				return;
			List<AIFleetInfo> list1 = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
			FleetTemplate fleetTemplate = (FleetTemplate)null;
			AIFleetInfo aiFleetInfo = list1.FirstOrDefault<AIFleetInfo>((Func<AIFleetInfo, bool>)(x =>
		   {
			   int? fleetId = x.FleetID;
			   int id = fleet.ID;
			   return fleetId.GetValueOrDefault() == id & fleetId.HasValue;
		   }));
			int? nullable1;
			if (aiFleetInfo != null && aiFleetInfo.InvoiceID.HasValue)
			{
				nullable1 = aiFleetInfo.InvoiceID;
				int num = 0;
				if (!(nullable1.GetValueOrDefault() == num & nullable1.HasValue))
					return;
			}
			if (aiFleetInfo == null)
			{
				string templateName = DesignLab.DeduceFleetTemplate(this._db, this._game, fleet.ID);
				if (!string.IsNullOrEmpty(templateName))
				{
					fleetTemplate = this._db.AssetDatabase.FleetTemplates.First<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == templateName));
					AIFleetInfo fleetInfo = new AIFleetInfo()
					{
						AdmiralID = new int?(fleet.AdmiralID),
						FleetType = MissionTypeExtensions.SerializeList(fleetTemplate.MissionTypes),
						SystemID = fleet.SupportingSystemID,
						FleetTemplate = templateName
					};
					fleetInfo.ID = this._db.InsertAIFleetInfo(this._player.ID, fleetInfo);
					aiFleetInfo = fleetInfo;
				}
			}
			if (fleetTemplate == null)
				fleetTemplate = this._db.AssetDatabase.FleetTemplates.FirstOrDefault<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == aiFleetInfo.FleetTemplate));
			if (fleetTemplate == null)
				return;
			bool flag = this._db.GetMissionByFleetID(fleet.ID) != null;
			int? nullable2 = new int?();
			if (!flag)
			{
				nullable2 = this._game.GameDatabase.GetReserveFleetID(this._player.ID, fleet.SystemID);
				if (!nullable2.HasValue || nullable2.Value == 0)
					return;
			}
			if (this.Player.Faction.Name == "loa" && nullable2.HasValue)
			{
				Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this._game, fleet.ID);
				int num = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._game, fleet.ID) - this.m_LoaLimit;
				if (num <= 0)
					return;
				ShipInfo shipInfo1 = this._db.GetShipInfoByFleetID(fleet.ID, true).FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.IsLoaCube()));
				if (shipInfo1 == null)
					return;
				ShipInfo shipInfo2 = this._db.GetShipInfoByFleetID(nullable2.Value, true).FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.IsLoaCube()));
				if (shipInfo2 != null)
				{
					this._db.UpdateShipLoaCubes(shipInfo2.ID, Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, shipInfo2.ID) + num);
				}
				else
				{
					GameDatabase db = this._db;
					int fleetID = nullable2.Value;
					int id = this._db.GetDesignInfosForPlayer(fleet.PlayerID).FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.IsLoaCube())).ID;
					nullable1 = new int?();
					int? aiFleetID = nullable1;
					int Loacubes = num;
					db.InsertShip(fleetID, id, "Cube", (ShipParams)0, aiFleetID, Loacubes);
				}
				this._db.UpdateShipLoaCubes(shipInfo1.ID, Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, shipInfo1.ID) - num);
				this.m_FleetCubePoints[fleet.ID] = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._game, fleet.ID);
				if (Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, shipInfo1.ID) - num > 0)
					return;
				this._db.RemoveShipFromFleet(shipInfo1.ID);
			}
			else
			{
				if (!(this.Player.Faction.Name != "loa"))
					return;
				int num1 = 0;
				List<ShipInfo> list2 = this._game.GameDatabase.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>();
				int num2 = this._db.GetFleetCommandPointQuota(fleet.ID) - this._db.GetFleetCommandPointCost(fleet.ID);
				if (num2 < 0)
				{
					foreach (ShipInclude shipInclude in fleetTemplate.ShipIncludes.Where<ShipInclude>((Func<ShipInclude, bool>)(x => x.InclusionType == ShipInclusionType.FILL)).ToList<ShipInclude>())
					{
						ShipInclude include2 = shipInclude;
						if ((string.IsNullOrEmpty(include2.Faction) || !(include2.Faction != this._player.Faction.Name)) && !(include2.FactionExclusion == this._player.Faction.Name))
						{
							List<ShipInfo> list3 = list2.Where<ShipInfo>((Func<ShipInfo, bool>)(x => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, include2.ShipRole))).OrderBy<ShipInfo, int>((Func<ShipInfo, int>)(x => this._db.GetShipCruiserEquivalent(x.DesignInfo))).ThenBy<ShipInfo, int>((Func<ShipInfo, int>)(x => x.DesignInfo.DesignDate)).ToList<ShipInfo>();
							while (num2 < 0)
							{
								ShipInfo shipInfo = list3.FirstOrDefault<ShipInfo>();
								if (shipInfo != null && list3.Count > include2.Amount)
								{
									num2 += this._db.GetShipCommandPointCost(shipInfo.ID, false);
									++num1;
									list2.Remove(shipInfo);
									list3.Remove(shipInfo);
									if (nullable2.HasValue)
										this._db.TransferShip(shipInfo.ID, nullable2.Value);
								}
								else
									break;
							}
						}
					}
				}
				if (!flag || num1 <= 3)
					return;
				Kerberos.Sots.StarFleet.StarFleet.CancelMission(this._game, fleet, true);
			}
		}

		private void ConsumeRemainingLoaCubes(FleetInfo fleet, AIStance stance)
		{
			if (fleet == null)
			{
				return;
			}
			List<ShipInfo> list = this._db.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>();
			ShipInfo shipInfo = list.FirstOrDefault((ShipInfo x) => x.DesignInfo.IsLoaCube());
			if (shipInfo == null || shipInfo.LoaCubes == 0)
			{
				return;
			}
			int num = shipInfo.LoaCubes;
			List<AIFleetInfo> source = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
			FleetTemplate fleetTemplate = null;
			AIFleetInfo aiFleetInfo = source.FirstOrDefault((AIFleetInfo x) => x.FleetID == fleet.ID);
			if (aiFleetInfo != null && aiFleetInfo.InvoiceID != null && aiFleetInfo.InvoiceID != 0)
			{
				return;
			}
			if (aiFleetInfo == null)
			{
				string templateName = DesignLab.DeduceFleetTemplate(this._db, this._game, fleet.ID);
				if (!string.IsNullOrEmpty(templateName))
				{
					fleetTemplate = this._db.AssetDatabase.FleetTemplates.First((FleetTemplate x) => x.Name == templateName);
					AIFleetInfo aifleetInfo = new AIFleetInfo();
					aifleetInfo.AdmiralID = new int?(fleet.AdmiralID);
					aifleetInfo.FleetType = MissionTypeExtensions.SerializeList(fleetTemplate.MissionTypes);
					aifleetInfo.SystemID = fleet.SupportingSystemID;
					aifleetInfo.FleetTemplate = templateName;
					aifleetInfo.ID = this._db.InsertAIFleetInfo(this._player.ID, aifleetInfo);
					aiFleetInfo = aifleetInfo;
				}
			}
			if (fleetTemplate == null)
			{
				fleetTemplate = this._db.AssetDatabase.FleetTemplates.FirstOrDefault((FleetTemplate x) => x.Name == aiFleetInfo.FleetTemplate);
			}
			List<DesignInfo> list2 = (from X in this._db.GetDesignInfosForPlayer(this._player.ID)
									  where X.Class == ShipClass.BattleRider
									  select X).ToList<DesignInfo>();
			using (List<ShipInclude>.Enumerator enumerator = this.GetRequiredShipIncludes(fleetTemplate).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ShipInclude include = enumerator.Current;
					if ((string.IsNullOrEmpty(include.Faction) || !(include.Faction != this._player.Faction.Name)) && !(include.FactionExclusion == this._player.Faction.Name))
					{
						int num2 = list.Count((ShipInfo x) => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, include.ShipRole));
						if (num2 < include.Amount)
						{
							DesignInfo designInfo = DesignLab.GetBestDesignByRole(this._game, this._player, new AIStance?(stance), include.ShipRole, StrategicAI.GetEquivilantShipRoles(include.ShipRole), include.WeaponRole);
							if (designInfo == null)
							{
								designInfo = DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(stance), include.ShipRole, include.WeaponRole);
							}
							if (designInfo != null)
							{
								int playerProductionCost = designInfo.GetPlayerProductionCost(this._game.GameDatabase, this._player.ID, !designInfo.isPrototyped, null);
								int num3 = include.Amount - num2;
								while (num3 > 0 && num >= playerProductionCost)
								{
									int shipID = this._db.InsertShip(fleet.ID, designInfo.ID, null, (ShipParams)0, new int?((aiFleetInfo != null) ? aiFleetInfo.ID : 0), 0);
									ShipInfo shipInfo2 = this._db.GetShipInfo(shipID, true);
									if (shipInfo2 == null)
									{
										break;
									}
									num3--;
									num -= playerProductionCost;
									list.Add(shipInfo2);
									this._game.GameDatabase.TransferShip(shipInfo2.ID, fleet.ID);
									List<CarrierWingData> list3 = RiderManager.GetDesignBattleriderWingData(this._game.App, designInfo).ToList<CarrierWingData>();
									using (List<CarrierWingData>.Enumerator enumerator2 = list3.GetEnumerator())
									{
										while (enumerator2.MoveNext())
										{
											CarrierWingData wd = enumerator2.Current;
											List<DesignInfo> classriders = (from x in list2
																			where x.GetMissionSectionAsset().BattleRiderType != BattleRiderTypes.escort && StrategicAI.BattleRiderMountSet.GetMatchingTurretClass(x) == wd.Class
																			select x).ToList<DesignInfo>();
											if (classriders.Any<DesignInfo>() && wd.SlotIndexes.Any<int>())
											{
												BattleRiderTypes SelectedType = (from x in classriders
																				 orderby x.DesignDate
																				 select x).First<DesignInfo>().GetMissionSectionAsset().BattleRiderType;
												DesignInfo designInfo2 = classriders.FirstOrDefault((DesignInfo x) => x.GetMissionSectionAsset().BattleRiderType == SelectedType && classriders.Count((DesignInfo j) => j.ID == x.ID) >= wd.SlotIndexes.Count);
												int num4 = (designInfo2 != null) ? designInfo2.GetPlayerProductionCost(this._db, this._player.ID, !designInfo2.isPrototyped, null) : 0;
												foreach (int index in wd.SlotIndexes)
												{
													if (designInfo2 == null || num < num4)
													{
														break;
													}
													int num5 = this._db.InsertShip(fleet.ID, designInfo2.ID, designInfo2.Name, (ShipParams)0, null, 0);
													this._game.AddDefaultStartingRiders(fleet.ID, designInfo2.ID, num5);
													this._db.SetShipParent(num5, shipInfo2.ID);
													this._db.UpdateShipRiderIndex(num5, index);
													list2.Remove(designInfo2);
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			if (list.Any((ShipInfo x) => x.DesignInfo.Role == ShipRole.COMMAND))
			{
				int num6 = this._db.GetFleetCommandPointQuota(fleet.ID) - this._db.GetFleetCommandPointCost(fleet.ID);
				if (num6 > 0 && num > 0)
				{
					using (List<ShipInclude>.Enumerator enumerator4 = (from x in fleetTemplate.ShipIncludes
																	   where x.InclusionType == ShipInclusionType.FILL
																	   select x).ToList<ShipInclude>().GetEnumerator())
					{
						while (enumerator4.MoveNext())
						{
							ShipInclude include = enumerator4.Current;
							if ((string.IsNullOrEmpty(include.Faction) || !(include.Faction != this._player.Faction.Name)) && !(include.FactionExclusion == this._player.Faction.Name))
							{
								int num7 = 0;
								if (include.Amount > 0)
								{
									int num8 = list.Count((ShipInfo x) => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, include.ShipRole));
									if (num8 < include.Amount)
									{
										num7 = include.Amount - num8;
									}
								}
								DesignInfo designInfo3 = DesignLab.GetBestDesignByRole(this._game, this._player, new AIStance?(stance), include.ShipRole, StrategicAI.GetEquivilantShipRoles(include.ShipRole), include.WeaponRole);
								if (designInfo3 == null)
								{
									designInfo3 = DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(stance), include.ShipRole, include.WeaponRole);
								}
								if (designInfo3 != null)
								{
									int playerProductionCost2 = designInfo3.GetPlayerProductionCost(this._game.GameDatabase, this._player.ID, !designInfo3.isPrototyped, null);
									while (num >= playerProductionCost2 && ((include.Amount == 0 && num6 > 0) || (include.Amount > 0 && num7 > 0)))
									{
										int shipID2 = this._db.InsertShip(fleet.ID, designInfo3.ID, null, (ShipParams)0, new int?((aiFleetInfo != null) ? aiFleetInfo.ID : 0), 0);
										ShipInfo shipInfo3 = this._db.GetShipInfo(shipID2, true);
										if (shipInfo3 == null)
										{
											break;
										}
										num7--;
										num -= playerProductionCost2;
										num6 -= shipInfo3.DesignInfo.CommandPointCost;
										list.Add(shipInfo3);
										this._game.GameDatabase.TransferShip(shipInfo3.ID, fleet.ID);
										List<CarrierWingData> list4 = RiderManager.GetDesignBattleriderWingData(this._game.App, designInfo3).ToList<CarrierWingData>();
										using (List<CarrierWingData>.Enumerator enumerator5 = list4.GetEnumerator())
										{
											while (enumerator5.MoveNext())
											{
												CarrierWingData wd = enumerator5.Current;
												List<DesignInfo> classriders = (from x in list2
																				where x.GetMissionSectionAsset().BattleRiderType != BattleRiderTypes.escort && StrategicAI.BattleRiderMountSet.GetMatchingTurretClass(x) == wd.Class
																				select x).ToList<DesignInfo>();
												if (classriders.Any<DesignInfo>() && wd.SlotIndexes.Any<int>())
												{
													BattleRiderTypes SelectedType = (from x in classriders
																					 orderby x.DesignDate
																					 select x).First<DesignInfo>().GetMissionSectionAsset().BattleRiderType;
													DesignInfo designInfo4 = classriders.FirstOrDefault((DesignInfo x) => x.GetMissionSectionAsset().BattleRiderType == SelectedType && classriders.Count((DesignInfo j) => j.ID == x.ID) >= wd.SlotIndexes.Count);
													int num9 = (designInfo4 != null) ? designInfo4.GetPlayerProductionCost(this._db, this._player.ID, !designInfo4.isPrototyped, null) : 0;
													foreach (int index2 in wd.SlotIndexes)
													{
														if (designInfo4 == null || num < num9)
														{
															break;
														}
														int num10 = this._db.InsertShip(fleet.ID, designInfo4.ID, designInfo4.Name, (ShipParams)0, null, 0);
														this._game.AddDefaultStartingRiders(fleet.ID, designInfo4.ID, num10);
														this._db.SetShipParent(num10, shipInfo3.ID);
														this._db.UpdateShipRiderIndex(num10, index2);
														list2.Remove(designInfo4);
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			if (shipInfo.LoaCubes <= 0)
			{
				this._db.RemoveShip(shipInfo.ID);
				return;
			}
			this._db.UpdateShipLoaCubes(shipInfo.ID, num);
		}

		private float RequiredRatio(FleetTemplate template)
		{
			float num = 0.0f;
			foreach (MissionType missionType in template.MissionTypes)
			{
				switch (missionType)
				{
					case MissionType.COLONIZATION:
						num += 0.75f;
						continue;
					case MissionType.SUPPORT:
						num += 0.75f;
						continue;
					case MissionType.SURVEY:
						num += 0.75f;
						continue;
					case MissionType.CONSTRUCT_STN:
						num += 0.75f;
						continue;
					case MissionType.UPGRADE_STN:
						num += 0.75f;
						continue;
					case MissionType.PATROL:
						num += 0.75f;
						continue;
					case MissionType.STRIKE:
						num += 0.75f;
						continue;
					case MissionType.INVASION:
						num += 0.75f;
						continue;
					case MissionType.GATE:
						num += 0.75f;
						continue;
					case MissionType.DEPLOY_NPG:
						num += 0.75f;
						continue;
					default:
						continue;
				}
			}
			return num / (float)template.MissionTypes.Count;
		}

		private bool FillFleet(FleetInfo fleet)
		{
			if (fleet == null || fleet.SystemID == 0)
				return false;
			List<AIFleetInfo> list1 = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
			FleetTemplate fleetTemplate = (FleetTemplate)null;
			AIFleetInfo aiFleetInfo = list1.FirstOrDefault<AIFleetInfo>((Func<AIFleetInfo, bool>)(x =>
		   {
			   int? fleetId = x.FleetID;
			   int id = fleet.ID;
			   return fleetId.GetValueOrDefault() == id & fleetId.HasValue;
		   }));
			if (aiFleetInfo != null && aiFleetInfo.InvoiceID.HasValue)
			{
				int? invoiceId = aiFleetInfo.InvoiceID;
				int num1 = 0;
				if (!(invoiceId.GetValueOrDefault() == num1 & invoiceId.HasValue))
					return false;
			}
			if (aiFleetInfo == null)
			{
				string templateName = DesignLab.DeduceFleetTemplate(this._db, this._game, fleet.ID);
				if (!string.IsNullOrEmpty(templateName))
				{
					fleetTemplate = this._db.AssetDatabase.FleetTemplates.First<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == templateName));
					AIFleetInfo fleetInfo = new AIFleetInfo()
					{
						AdmiralID = new int?(fleet.AdmiralID),
						FleetType = MissionTypeExtensions.SerializeList(fleetTemplate.MissionTypes),
						SystemID = fleet.SupportingSystemID,
						FleetTemplate = templateName
					};
					fleetInfo.ID = this._db.InsertAIFleetInfo(this._player.ID, fleetInfo);
					aiFleetInfo = fleetInfo;
				}
			}
			if (fleetTemplate == null)
				fleetTemplate = this._db.AssetDatabase.FleetTemplates.FirstOrDefault<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == aiFleetInfo.FleetTemplate));
			if (fleetTemplate == null)
				return false;
			int? reserveFleetId = this._game.GameDatabase.GetReserveFleetID(this._player.ID, fleet.SystemID);
			int remainingCELimit = this._player.Faction.CanUseGate() ? this.m_GateCapacity - this._db.GetFleetCruiserEquivalent(fleet.ID) : int.MaxValue;
			int num = this._player.Faction.CanUseAccelerators() ? this.m_LoaLimit - Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._game, fleet.ID) : int.MaxValue;
			bool flag1 = true;
			bool flag2 = false;
			bool flag3 = false;
			if (this._player.Faction.Name == "loa")
			{
				int id = this._db.GetLoaFleetCompositions().FirstOrDefault<LoaFleetComposition>((Func<LoaFleetComposition, bool>)(x =>
			   {
				   if (x.Name == fleetTemplate.Name)
					   return x.PlayerID == this._player.ID;
				   return false;
			   })).ID;
				this._db.UpdateFleetCompositionID(fleet.ID, new int?(id));
				Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromCompositionID(this._game, fleet.ID, new int?(id), MissionType.NO_MISSION);
			}
			List<ShipInfo> list2 = this._game.GameDatabase.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>();
			List<ShipInfo> source = !reserveFleetId.HasValue || reserveFleetId.Value == 0 ? new List<ShipInfo>() : this._game.GameDatabase.GetShipInfoByFleetID(reserveFleetId.Value, true).Where<ShipInfo>((Func<ShipInfo, bool>)(x =>
		   {
			   if (x.AIFleetID.HasValue)
			   {
				   int? aiFleetId = x.AIFleetID;
				   int num1 = 0;
				   if (!(aiFleetId.GetValueOrDefault() == num1 & aiFleetId.HasValue))
				   {
					   aiFleetId = x.AIFleetID;
					   int id = aiFleetInfo.ID;
					   return aiFleetId.GetValueOrDefault() == id & aiFleetId.HasValue;
				   }
			   }
			   return true;
		   })).ToList<ShipInfo>();
			foreach (ShipInclude requiredShipInclude in this.GetRequiredShipIncludes(fleetTemplate))
			{
				ShipInclude include = requiredShipInclude;
				if ((string.IsNullOrEmpty(include.Faction) || !(include.Faction != this._player.Faction.Name)) && !(include.FactionExclusion == this._player.Faction.Name))
				{
					int num1 = list2.Count<ShipInfo>((Func<ShipInfo, bool>)(x => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, include.ShipRole)));
					if (num1 < include.Amount)
					{
						int num2 = include.Amount - num1;
						while (num2 > 0 && source.Count > 0)
						{
							ShipInfo shipInfo = source.Where<ShipInfo>((Func<ShipInfo, bool>)(x =>
						   {
							   int? aiFleetId = x.AIFleetID;
							   int id = aiFleetInfo.ID;
							   if (!(aiFleetId.GetValueOrDefault() == id & aiFleetId.HasValue))
								   return !x.AIFleetID.HasValue;
							   return true;
						   })).FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, include.ShipRole))) ?? (!(this._player.Faction.Name == "loa") ? source.FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, include.ShipRole))) : source.FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, x.DesignInfo) <= num)));
							if (shipInfo != null)
							{
								--num2;
								source.Remove(shipInfo);
								list2.Add(shipInfo);
								this._game.GameDatabase.TransferShip(shipInfo.ID, fleet.ID);
							}
							else
								break;
						}
						if (num2 > 0 && (include.ShipRole != ShipRole.GATE || num1 == 0))
							flag1 = false;
					}
				}
			}
			int remainingPoints = this._db.GetFleetCommandPointQuota(fleet.ID) - this._db.GetFleetCommandPointCost(fleet.ID);
			if (remainingPoints > 0)
			{
				foreach (ShipInclude shipInclude in fleetTemplate.ShipIncludes.Where<ShipInclude>((Func<ShipInclude, bool>)(x => x.InclusionType == ShipInclusionType.FILL)).ToList<ShipInclude>())
				{
					ShipInclude include2 = shipInclude;
					if ((string.IsNullOrEmpty(include2.Faction) || !(include2.Faction != this._player.Faction.Name)) && !(include2.FactionExclusion == this._player.Faction.Name))
					{
						flag2 = true;
						int num1 = 0;
						if (include2.Amount > 0)
						{
							int num2 = list2.Count<ShipInfo>((Func<ShipInfo, bool>)(x => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, include2.ShipRole)));
							if (num2 < include2.Amount)
								num1 = include2.Amount - num2;
						}
						while (source.Count > 0 && (include2.Amount == 0 && remainingPoints > 0 || include2.Amount > 0 && num1 > 0))
						{
							ShipInfo shipInfo = source.FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x =>
						   {
							   if (StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, include2.ShipRole) && x.DesignInfo.CommandPointCost <= remainingPoints)
							   {
								   int? aiFleetId = x.AIFleetID;
								   int id = aiFleetInfo.ID;
								   if (aiFleetId.GetValueOrDefault() == id & aiFleetId.HasValue || !x.AIFleetID.HasValue)
								   {
									   if (this._player.Faction.CanUseGate())
										   return this._db.GetShipCruiserEquivalent(x.DesignInfo) <= remainingCELimit;
									   return true;
								   }
							   }
							   return false;
						   }));
							if (shipInfo != null)
							{
								if (this._player.Faction.CanUseGate())
								{
									int cruiserEquivalent = this._db.GetShipCruiserEquivalent(shipInfo.DesignInfo);
									remainingCELimit -= cruiserEquivalent;
									if (remainingCELimit < 0)
									{
										remainingPoints = 0;
										num1 = 0;
										break;
									}
								}
								else if (this._player.Faction.CanUseAccelerators())
								{
									int shipLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, shipInfo.ID);
									num -= shipLoaCubeValue;
									if (num < 0)
									{
										num = 0;
										num1 = 0;
										break;
									}
								}
								--num1;
								remainingPoints -= shipInfo.DesignInfo.CommandPointCost;
								source.Remove(shipInfo);
								list2.Add(shipInfo);
								this._game.GameDatabase.TransferShip(shipInfo.ID, fleet.ID);
							}
							else
								break;
						}
						if (include2.Amount == 0 || num1 > 0)
							flag3 = true;
					}
				}
			}
			if ((double)this.FillFleetRiders(fleet) > 0.0)
				flag1 = false;
			if (flag2 & flag1 & flag3 && remainingCELimit > 0 && num > 0)
			{
				int fleetCommandPoints = Kerberos.Sots.StarFleet.StarFleet.GetFleetCommandPoints(this._game.App, fleet.ID, (IEnumerable<int>)null);
				float num1 = (float)Kerberos.Sots.StarFleet.StarFleet.GetFleetCommandCost(this._game.App, fleet.ID, (IEnumerable<int>)null) / (float)fleetCommandPoints;
				float num2 = 0.0f;
				if (this._player.Faction.Name == "loa")
					num2 = (float)(this.m_LoaLimit - num) / (float)this.m_LoaLimit;
				if (this._player.Faction.Name != "loa" && (double)num1 < (double)this.RequiredRatio(fleetTemplate) || (double)num1 < (double)this.RequiredRatio(fleetTemplate) && (double)num2 < (double)this.RequiredRatio(fleetTemplate))
				{
					StrategicAI.TraceVerbose("Fleet " + fleet.Name + " does not meet minimum command point/loa cube fill! (has: " + (object)num1 + " needs: " + (object)this.RequiredRatio(fleetTemplate));
					flag1 = false;
				}
			}
			return flag1;
		}

		private int GetRemainingAIBuildTime(int systemId, float productionPerTurn)
		{
			int maxBuildTurns = this.MAX_BUILD_TURNS;
			int num1 = 0;
			int num2 = 0;
			foreach (BuildOrderInfo buildOrderInfo in this._db.GetBuildOrdersForSystem(systemId).ToList<BuildOrderInfo>())
			{
				num2 += buildOrderInfo.Progress;
				num1 += buildOrderInfo.ProductionTarget;
			}
			if ((double)productionPerTurn > 0.0)
			{
				int num3 = (int)Math.Ceiling((double)(num1 - num2) / (double)productionPerTurn);
				maxBuildTurns -= num3;
			}
			return maxBuildTurns;
		}

		private bool FillFleetsWithBuild(int systemId, List<FleetInfo> fleetsAtSystem, AIStance stance)
		{
			float productionRate = 0.0f;
			double constructionRate = 0.0;
			double totalRevenue = 0.0;
			BuildScreenState.ObtainConstructionCosts(out productionRate, out constructionRate, out totalRevenue, this._game.App, systemId, this._player.ID);
			int remainingAiBuildTime = this.GetRemainingAIBuildTime(systemId, productionRate);
			if (remainingAiBuildTime <= 0)
				return false;
			double constructionBudget = this.GetAvailableFillFleetConstructionBudget();
			List<BuildOrderInfo> list1 = this._db.GetBuildOrdersForSystem(systemId).ToList<BuildOrderInfo>();
			List<AIFleetInfo> list2 = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
			List<BuildManagement> source = new List<BuildManagement>();
			foreach (FleetInfo fleetInfo1 in fleetsAtSystem)
			{
				FleetInfo fleet = fleetInfo1;
				FleetTemplate template = (FleetTemplate)null;
				AIFleetInfo aiFleetInfo = list2.FirstOrDefault<AIFleetInfo>((Func<AIFleetInfo, bool>)(x =>
			   {
				   int? fleetId = x.FleetID;
				   int id = fleet.ID;
				   return fleetId.GetValueOrDefault() == id & fleetId.HasValue;
			   }));
				if (aiFleetInfo == null)
				{
					string templateName = DesignLab.DeduceFleetTemplate(this._db, this._game, fleet.ID);
					if (!string.IsNullOrEmpty(templateName))
					{
						template = this._db.AssetDatabase.FleetTemplates.First<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == templateName));
						AIFleetInfo fleetInfo2 = new AIFleetInfo()
						{
							AdmiralID = new int?(fleet.AdmiralID),
							FleetType = MissionTypeExtensions.SerializeList(template.MissionTypes),
							SystemID = fleet.SupportingSystemID,
							FleetTemplate = templateName
						};
						fleetInfo2.ID = this._db.InsertAIFleetInfo(this._player.ID, fleetInfo2);
						aiFleetInfo = fleetInfo2;
						list2.Add(aiFleetInfo);
					}
				}
				if (template == null)
					template = this._db.AssetDatabase.FleetTemplates.FirstOrDefault<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == aiFleetInfo.FleetTemplate));
				if (!list1.Any<BuildOrderInfo>((Func<BuildOrderInfo, bool>)(x =>
			   {
				   if (x.AIFleetID.HasValue)
					   return x.AIFleetID.Value == aiFleetInfo.ID;
				   return false;
			   })))
				{
					if (aiFleetInfo.InvoiceID.HasValue)
					{
						int? invoiceId = aiFleetInfo.InvoiceID;
						if (invoiceId.GetValueOrDefault() != 0 || !invoiceId.HasValue)
							continue;
					}
					List<InvoiceInstanceInfo> list3 = this._db.GetInvoicesForSystem(this._player.ID, systemId).ToList<InvoiceInstanceInfo>();
					BuildManagement buildManagement = new BuildManagement();
					List<ShipInfo> list4 = this._game.GameDatabase.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>();
					foreach (ShipInfo ship in list4)
						buildManagement.Invoices.AddRange((IEnumerable<BuildScreenState.InvoiceItem>)this.BuildRidersForShip(stance, ship.DesignInfo, list3, ship));
					int num1 = this._player.Faction.CanUseAccelerators() ? this.m_LoaLimit - Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._game, fleet.ID) : 0;
					int num2 = this._player.Faction.CanUseGate() ? this.m_GateCapacity - this._db.GetFleetCruiserEquivalent(fleet.ID) : 0;
					foreach (ShipInclude shipInclude in template.ShipIncludes)
					{
						ShipInclude ship = shipInclude;
						if ((string.IsNullOrEmpty(ship.Faction) || !(ship.Faction != this._player.Faction.Name)) && !(ship.FactionExclusion == this._player.Faction.Name))
						{
							DesignInfo selectedDesign = DesignLab.GetBestDesignByRole(this._game, this._player, new AIStance?(stance), ship.ShipRole, StrategicAI.GetEquivilantShipRoles(ship.ShipRole), ship.WeaponRole);
							if (selectedDesign == null)
								selectedDesign = DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(stance), ship.ShipRole, ship.WeaponRole);
							int val1;
							if (ship.InclusionType == ShipInclusionType.FILL)
							{
								DesignInfo commandDesign = (this._db.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>().Select<ShipInfo, DesignInfo>((Func<ShipInfo, DesignInfo>)(x => x.DesignInfo)).FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.Role == ShipRole.COMMAND)) ?? DesignLab.GetBestDesignByRole(this._game, this._player, new AIStance?(stance), ShipRole.COMMAND, StrategicAI.GetEquivilantShipRoles(ShipRole.COMMAND), new WeaponRole?())) ?? DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(stance), ShipRole.COMMAND, new WeaponRole?());
								val1 = DesignLab.GetTemplateFillAmount(this._db, template, commandDesign, selectedDesign);
								if (ship.Amount > 0 || ship.InclusionType == ShipInclusionType.FILL)
								{
									int num3 = list4.Count<ShipInfo>((Func<ShipInfo, bool>)(x => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, ship.ShipRole)));
									int val2 = Math.Max(ship.Amount - num3, val1 - num3);
									val1 = Math.Min(val1, val2);
								}
								if (this._player.Faction.CanUseGate())
								{
									int cruiserEquivalent = this._db.GetShipCruiserEquivalent(selectedDesign);
									if (cruiserEquivalent > 0)
										val1 = Math.Min(val1, num2 / cruiserEquivalent);
								}
							}
							else
							{
								int num3 = list4.Count<ShipInfo>((Func<ShipInfo, bool>)(x => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, ship.ShipRole)));
								val1 = ship.Amount - num3;
							}
							for (int index = 0; index < val1; ++index)
							{
								if (ship.InclusionType == ShipInclusionType.FILL && this._player.Faction.CanUseAccelerators())
								{
									int shipLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, selectedDesign);
									num1 -= shipLoaCubeValue;
									if (num1 < 0)
									{
										num1 = 0;
										break;
									}
								}
								bool flag1 = !buildManagement.Invoices.Any<BuildScreenState.InvoiceItem>((Func<BuildScreenState.InvoiceItem, bool>)(x =>
							   {
								   if (x.DesignID == selectedDesign.ID)
									   return x.isPrototypeOrder;
								   return false;
							   }));
								bool flag2 = !this._db.GetDesignBuildOrders(selectedDesign).Any<BuildOrderInfo>();
								buildManagement.Invoices.Add(new BuildScreenState.InvoiceItem()
								{
									DesignID = selectedDesign.ID,
									ShipName = selectedDesign.Name,
									Progress = -1,
									isPrototypeOrder = !selectedDesign.isPrototyped & flag1 & flag2,
									LoaCubes = 0
								});
							}
							buildManagement.Invoices.AddRange((IEnumerable<BuildScreenState.InvoiceItem>)this.BuildRidersForShip(stance, selectedDesign, list3, (ShipInfo)null));
						}
					}
					if (aiFleetInfo != null)
						buildManagement.AIFleetID = new int?(aiFleetInfo.ID);
					buildManagement.FleetToFill = fleet;
					buildManagement.BuildTime = BuildScreenState.GetBuildTime(this._game.App, (IEnumerable<BuildScreenState.InvoiceItem>)buildManagement.Invoices, productionRate);
					source.Add(buildManagement);
				}
			}
			source.RemoveAll((Predicate<BuildManagement>)(x => x.Invoices.Count == 0));
			foreach (BuildManagement buildManagement in (IEnumerable<BuildManagement>)source.OrderBy<BuildManagement, int>((Func<BuildManagement, int>)(x => x.BuildTime)))
			{
				BuildManagement build = buildManagement;
				AdmiralInfo admiralInfo = this._db.GetAdmiralInfo(build.FleetToFill.AdmiralID);
				if (admiralInfo != null)
				{
					int buildTime = BuildScreenState.GetBuildTime(this._game.App, (IEnumerable<BuildScreenState.InvoiceItem>)build.Invoices, productionRate);
					if (buildTime > 0)
					{
						int num = BuildScreenState.GetBuildInvoiceCost(this._game.App, build.Invoices) / buildTime;
						if (constructionBudget >= (double)num)
						{
							AIFleetInfo fleetInfo = list2.FirstOrDefault<AIFleetInfo>((Func<AIFleetInfo, bool>)(x => x.ID == build.AIFleetID.Value));
							if (fleetInfo == null || !fleetInfo.InvoiceID.HasValue)
							{
								int invoiceId;
								int instanceId;
								this.OpenInvoice(systemId, admiralInfo.Name, out invoiceId, out instanceId);
								if (fleetInfo != null)
								{
									fleetInfo.InvoiceID = new int?(instanceId);
									this._db.UpdateAIFleetInfo(fleetInfo);
								}
								foreach (BuildScreenState.InvoiceItem invoice in build.Invoices)
									this.AddShipToInvoice(systemId, this._db.GetDesignInfo(invoice.DesignID), invoiceId, instanceId, build.AIFleetID);
								constructionBudget -= (double)num;
								remainingAiBuildTime -= build.BuildTime;
								if (remainingAiBuildTime <= 0)
									break;
							}
						}
					}
				}
			}
			return true;
		}

		private List<BuildScreenState.InvoiceItem> BuildRidersForShip(
		  AIStance stance,
		  DesignInfo design,
		  List<InvoiceInstanceInfo> orders,
		  ShipInfo ship = null)
		{
			List<ShipInfo> source1 = ship != null ? this._db.GetBattleRidersByParentID(ship.ID).ToList<ShipInfo>() : new List<ShipInfo>();
			List<BuildScreenState.InvoiceItem> source2 = new List<BuildScreenState.InvoiceItem>();
			Dictionary<LogicalBank, StrategicAI.BankRiderInfo> dictionary = new Dictionary<LogicalBank, StrategicAI.BankRiderInfo>();
			foreach (KeyValuePair<LogicalMount, int> battleRiderMount in StrategicAI.BattleRiderMountSet.EnumerateBattleRiderMounts(design))
			{
				KeyValuePair<LogicalMount, int> mountIndex = battleRiderMount;
				ShipInfo shipInfo = source1.FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.RiderIndex == mountIndex.Value));
				StrategicAI.BankRiderInfo bankRiderInfo;
				if (!dictionary.TryGetValue(mountIndex.Key.Bank, out bankRiderInfo))
				{
					bankRiderInfo = new StrategicAI.BankRiderInfo()
					{
						Bank = mountIndex.Key.Bank
					};
					dictionary.Add(bankRiderInfo.Bank, bankRiderInfo);
					if (shipInfo != null)
						bankRiderInfo.AllocatedRole = new ShipRole?(shipInfo.DesignInfo.Role);
				}
				if (shipInfo == null)
					bankRiderInfo.FreeRiderIndices.Add(mountIndex.Value);
				else
					bankRiderInfo.FilledRiderIndices.Add(mountIndex.Value);
			}
			foreach (StrategicAI.BankRiderInfo bankRiderInfo in dictionary.Values)
			{
				ShipRole shipRole = !bankRiderInfo.AllocatedRole.HasValue ? this._random.Choose<ShipRole>(StrategicAI.BattleRiderMountSet.EnumerateShipRolesByTurretClass(bankRiderInfo.Bank.TurretClass)) : bankRiderInfo.AllocatedRole.Value;
				int count = bankRiderInfo.FreeRiderIndices.Count;
				for (int index = 0; index < count; ++index)
				{
					DesignInfo buildDesign = DesignLab.GetBestDesignByRole(this._game, this._player, new AIStance?(stance), shipRole, StrategicAI.GetEquivilantShipRoles(shipRole), new WeaponRole?());
					if (buildDesign == null)
						buildDesign = DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(stance), shipRole, new WeaponRole?());
					if (buildDesign != null)
						source2.Add(new BuildScreenState.InvoiceItem()
						{
							DesignID = buildDesign.ID,
							ShipName = buildDesign.Name,
							Progress = -1,
							isPrototypeOrder = !buildDesign.isPrototyped && !source2.Any<BuildScreenState.InvoiceItem>((Func<BuildScreenState.InvoiceItem, bool>)(x =>
						   {
							   if (x.DesignID == buildDesign.ID)
								   return x.isPrototypeOrder;
							   return false;
						   })) && !orders.Any<InvoiceInstanceInfo>((Func<InvoiceInstanceInfo, bool>)(x => this._db.GetBuildOrdersForInvoiceInstance(x.ID).Any<BuildOrderInfo>((Func<BuildOrderInfo, bool>)(y => y.DesignID == buildDesign.ID)))),
							LoaCubes = 0
						});
				}
			}
			return source2;
		}

		private int GetSupportRequirementsForInvoice(List<BuildScreenState.InvoiceItem> items)
		{
			int num = 0;
			foreach (BuildScreenState.InvoiceItem invoiceItem in items)
				num += this._db.GetShipCruiserEquivalent(this._db.GetDesignInfo(invoiceItem.DesignID));
			return num;
		}

		private List<BuildScreenState.InvoiceItem> GetBuildFleetInvoice(
		  FleetTemplate template,
		  AIStance stance)
		{
			int num = this.m_LoaLimit;
			int gateCapacity = this.m_GateCapacity;
			List<BuildScreenState.InvoiceItem> source = new List<BuildScreenState.InvoiceItem>();
			foreach (ShipInclude shipInclude in template.ShipIncludes)
			{
				if ((string.IsNullOrEmpty(shipInclude.Faction) || !(shipInclude.Faction != this._player.Faction.Name)) && !(shipInclude.FactionExclusion == this._player.Faction.Name))
				{
					DesignInfo designInfo = DesignLab.GetBestDesignByRole(this._game, this._player, new AIStance?(stance), shipInclude.ShipRole, StrategicAI.GetEquivilantShipRoles(shipInclude.ShipRole), shipInclude.WeaponRole);
					if (designInfo == null)
						designInfo = DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(stance), shipInclude.ShipRole, shipInclude.WeaponRole);
					int val1;
					if (shipInclude.InclusionType == ShipInclusionType.FILL)
					{
						DesignInfo commandDesign = DesignLab.GetBestDesignByRole(this._game, this._player, new AIStance?(stance), ShipRole.COMMAND, StrategicAI.GetEquivilantShipRoles(ShipRole.COMMAND), new WeaponRole?()) ?? DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(stance), ShipRole.COMMAND, new WeaponRole?());
						val1 = DesignLab.GetTemplateFillAmount(this._db, template, commandDesign, designInfo);
						if (shipInclude.Amount > 0)
							val1 = Math.Min(val1, shipInclude.Amount);
						if (this._player.Faction.CanUseGate())
						{
							int cruiserEquivalent = this._db.GetShipCruiserEquivalent(designInfo);
							if (cruiserEquivalent > 0)
								val1 = Math.Min(val1, gateCapacity / cruiserEquivalent);
						}
					}
					else
						val1 = shipInclude.Amount;
					for (int index = 0; index < val1; ++index)
					{
						if (shipInclude.InclusionType == ShipInclusionType.FILL && this._player.Faction.CanUseAccelerators())
						{
							int shipLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, designInfo);
							num -= shipLoaCubeValue;
							if (num < 0)
							{
								num = 0;
								break;
							}
						}
						bool flag1 = !source.Any<BuildScreenState.InvoiceItem>((Func<BuildScreenState.InvoiceItem, bool>)(x =>
					   {
						   if (x.DesignID == designInfo.ID)
							   return x.isPrototypeOrder;
						   return false;
					   }));
						bool flag2 = !this._db.GetDesignBuildOrders(designInfo).Any<BuildOrderInfo>();
						source.Add(new BuildScreenState.InvoiceItem()
						{
							DesignID = designInfo.ID,
							ShipName = designInfo.Name,
							Progress = -1,
							isPrototypeOrder = !designInfo.isPrototyped & flag1 & flag2,
							LoaCubes = 0
						});
						source.AddRange((IEnumerable<BuildScreenState.InvoiceItem>)this.BuildRidersForShip(stance, designInfo, new List<InvoiceInstanceInfo>(), (ShipInfo)null));
						if (this._player.Faction.CanUseGate())
							gateCapacity -= this._db.GetShipCruiserEquivalent(designInfo);
					}
				}
			}
			return source;
		}

		private float FillFleetRiders(FleetInfo fleet)
		{
			int? reserveFleetId = this._game.GameDatabase.GetReserveFleetID(this._player.ID, fleet.SystemID);
			if (!reserveFleetId.HasValue || reserveFleetId.Value == 0)
				return 0.0f;
			List<ShipInfo> list1 = this._game.GameDatabase.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>();
			List<ShipInfo> list2 = this._game.GameDatabase.GetShipInfoByFleetID(reserveFleetId.Value, true).ToList<ShipInfo>();
			int num1 = 0;
			int num2 = 0;
			StrategicAI.BattleRiderMountSet battleRiderMountSet = new StrategicAI.BattleRiderMountSet((IEnumerable<ShipInfo>)list2);
			foreach (ShipInfo carrier in list1)
			{
				List<ShipInfo> list3 = this._db.GetBattleRidersByParentID(carrier.ID).ToList<ShipInfo>();
				Dictionary<LogicalBank, StrategicAI.BankRiderInfo> dictionary = new Dictionary<LogicalBank, StrategicAI.BankRiderInfo>();
				foreach (KeyValuePair<LogicalMount, int> battleRiderMount in StrategicAI.BattleRiderMountSet.EnumerateBattleRiderMounts(carrier.DesignInfo))
				{
					KeyValuePair<LogicalMount, int> mountIndex = battleRiderMount;
					ShipInfo shipInfo = list3.FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.RiderIndex == mountIndex.Value));
					StrategicAI.BankRiderInfo bankRiderInfo;
					if (!dictionary.TryGetValue(mountIndex.Key.Bank, out bankRiderInfo))
					{
						bankRiderInfo = new StrategicAI.BankRiderInfo()
						{
							Bank = mountIndex.Key.Bank
						};
						dictionary.Add(bankRiderInfo.Bank, bankRiderInfo);
						if (shipInfo != null)
							bankRiderInfo.AllocatedRole = new ShipRole?(shipInfo.DesignInfo.Role);
					}
					if (shipInfo == null)
						bankRiderInfo.FreeRiderIndices.Add(mountIndex.Value);
					else
						bankRiderInfo.FilledRiderIndices.Add(mountIndex.Value);
				}
				foreach (StrategicAI.BankRiderInfo bankRiderInfo in dictionary.Values.Where<StrategicAI.BankRiderInfo>((Func<StrategicAI.BankRiderInfo, bool>)(x => x.FreeRiderIndices.Count > 0)))
				{
					if (!bankRiderInfo.AllocatedRole.HasValue)
					{
						ShipInfo shipInfo = battleRiderMountSet.EnumerateByTurretClass(bankRiderInfo.Bank.TurretClass).FirstOrDefault<ShipInfo>();
						if (shipInfo != null)
							bankRiderInfo.AllocatedRole = new ShipRole?(shipInfo.DesignInfo.Role);
					}
					if (bankRiderInfo.AllocatedRole.HasValue)
					{
						while (bankRiderInfo.FreeRiderIndices.Count > 0)
						{
							ShipInfo shipByRole = battleRiderMountSet.FindShipByRole(bankRiderInfo.AllocatedRole.Value);
							if (shipByRole == null)
							{
								num1 += bankRiderInfo.FreeRiderIndices.Count;
								break;
							}
							battleRiderMountSet.AttachBattleRiderToShip(this._db, bankRiderInfo, shipByRole, carrier, bankRiderInfo.FreeRiderIndices[0]);
						}
					}
				}
			}
			if (num2 > 0)
				return (float)num1 / (float)num2;
			return 0.0f;
		}

		private void SetFleetOrders(AIInfo aiInfo)
		{
			StrategicAI.TraceVerbose("Assigning missions...");
			SortedDictionary<MissionType, int> source = new SortedDictionary<MissionType, int>();
			foreach (MissionType index in Enum.GetValues(typeof(MissionType)))
				source[index] = 0;
			switch (aiInfo.Stance)
			{
				case AIStance.CONQUERING:
					source[MissionType.GATE] = 6;
					source[MissionType.DEPLOY_NPG] = 6;
					source[MissionType.INVASION] = 5;
					source[MissionType.STRIKE] = 4;
					source[MissionType.PATROL] = 3;
					source[MissionType.COLONIZATION] = 2;
					source[MissionType.SURVEY] = 2;
					source[MissionType.UPGRADE_STN] = 1;
					source[MissionType.CONSTRUCT_STN] = 1;
					break;
				case AIStance.DESTROYING:
					source[MissionType.GATE] = 5;
					source[MissionType.DEPLOY_NPG] = 5;
					source[MissionType.INVASION] = 4;
					source[MissionType.STRIKE] = 4;
					source[MissionType.PATROL] = 3;
					source[MissionType.COLONIZATION] = 2;
					source[MissionType.SURVEY] = 2;
					source[MissionType.UPGRADE_STN] = 1;
					source[MissionType.CONSTRUCT_STN] = 1;
					break;
				case AIStance.DEFENDING:
					source[MissionType.PATROL] = 4;
					source[MissionType.GATE] = 3;
					source[MissionType.DEPLOY_NPG] = 3;
					source[MissionType.INVASION] = 2;
					source[MissionType.STRIKE] = 2;
					source[MissionType.SURVEY] = 1;
					source[MissionType.COLONIZATION] = 1;
					source[MissionType.UPGRADE_STN] = 1;
					source[MissionType.CONSTRUCT_STN] = 1;
					break;
				default:
					source[MissionType.GATE] = 5;
					source[MissionType.CONSTRUCT_STN] = 4;
					source[MissionType.DEPLOY_NPG] = 3;
					source[MissionType.COLONIZATION] = 3;
					source[MissionType.UPGRADE_STN] = 2;
					source[MissionType.SURVEY] = 1;
					break;
			}
			if (App.Log.Level >= Kerberos.Sots.Engine.LogLevel.Verbose)
			{
				StrategicAI.TraceVerbose(string.Format("  General priorities for stance {0} are:", (object)aiInfo.Stance));
				foreach (KeyValuePair<MissionType, int> keyValuePair in (IEnumerable<KeyValuePair<MissionType, int>>)source.OrderByDescending<KeyValuePair<MissionType, int>, int>((Func<KeyValuePair<MissionType, int>, int>)(x => x.Value)))
					StrategicAI.TraceVerbose(string.Format("    {0}: {1}", (object)keyValuePair.Value, (object)keyValuePair.Key));
			}
			foreach (FleetInfo fleetInfo in this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL))
			{
				MissionInfo missionByFleetId = this._db.GetMissionByFleetID(fleetInfo.ID);
				if (missionByFleetId != null && missionByFleetId.Type == MissionType.PATROL && aiInfo.Stance != AIStance.DEFENDING)
				{
					Kerberos.Sots.StarFleet.StarFleet.CancelMission(this._game, fleetInfo, true);
					StrategicAI.TraceVerbose(string.Format("  Cancelled existing patrol mission for fleet {0} because stance is not DEFENDING.", (object)fleetInfo));
				}
			}
			IEnumerable<AIFleetInfo> aiFleetInfos = this._db.GetAIFleetInfos(this._player.ID);
			foreach (FleetInfo fleetInfo in this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).ToList<FleetInfo>())
			{
				FleetInfo fleet = fleetInfo;
				int supportingSystemId = fleet.SupportingSystemID;
				if (fleet.AdmiralID == 0 || fleet.SupportingSystemID == 0 || (this._db.GetMissionByFleetID(fleet.ID) != null || this._db.GetShipInfoByFleetID(fleet.ID, false).Count<ShipInfo>() < 1) || fleet.SystemID != supportingSystemId)
				{
					StrategicAI.TraceVerbose(string.Format("  Skipping busy fleet {0}.", (object)fleet));
				}
				else
				{
					AIFleetInfo aiFleetInfo = aiFleetInfos.FirstOrDefault<AIFleetInfo>((Func<AIFleetInfo, bool>)(x =>
				   {
					   if (x.FleetID.HasValue)
						   return x.FleetID.Value == fleet.ID;
					   return false;
				   }));
					if (aiFleetInfo == null || aiFleetInfo.InvoiceID.HasValue)
					{
						StrategicAI.TraceVerbose(string.Format("  Skipping unqualified fleet {0}.", (object)fleet));
					}
					else
					{
						List<MissionType> missionTypeList = (List<MissionType>)null;
						if (aiFleetInfo != null)
							missionTypeList = MissionTypeExtensions.DeserializeList(aiFleetInfo.FleetType);
						IOrderedEnumerable<KeyValuePair<MissionType, int>> orderedEnumerable = source.OrderByDescending<KeyValuePair<MissionType, int>, int>((Func<KeyValuePair<MissionType, int>, int>)(x => x.Value));
						bool flag = false;
						StrategicAI.TraceVerbose(string.Format("  Trying missions for fleet {0}...", (object)fleet));
						foreach (KeyValuePair<MissionType, int> keyValuePair in (IEnumerable<KeyValuePair<MissionType, int>>)orderedEnumerable)
						{
							if (!flag)
							{
								if (keyValuePair.Value >= 1 && (missionTypeList == null || missionTypeList.Contains(keyValuePair.Key)))
								{
									StrategicAI.TraceVerbose(string.Format("    {0}: {1}...", (object)keyValuePair.Value, (object)keyValuePair.Key));
									switch (keyValuePair.Key)
									{
										case MissionType.COLONIZATION:
											flag = this.TryColonizationMission(fleet.ID);
											break;
										case MissionType.SURVEY:
											flag = this.TrySurveyMission(fleet.ID);
											break;
										case MissionType.CONSTRUCT_STN:
											flag = this.TryConstructionMission(aiInfo, fleet.ID);
											break;
										case MissionType.UPGRADE_STN:
											flag = this.TryUpgradeMission(aiInfo, fleet.ID);
											break;
										case MissionType.PATROL:
											flag = this.TryPatrolMission(fleet.ID);
											break;
										case MissionType.STRIKE:
											flag = this.TryStrikeMission(fleet.ID);
											break;
										case MissionType.INVASION:
											flag = this.TryInvasionMission(fleet.ID);
											break;
										case MissionType.GATE:
											if (this._player.Faction.Name == "hiver")
											{
												flag = this.TryGateMission(fleet.ID);
												break;
											}
											break;
										case MissionType.DEPLOY_NPG:
											if (this._player.Faction.Name == "loa")
											{
												flag = this.TryDeployNPGMission(fleet.ID);
												break;
											}
											break;
									}
									if (flag)
										StrategicAI.TraceVerbose("      OK.");
								}
							}
							else
								break;
						}
						if (!flag)
							StrategicAI.TraceVerbose("     No missions could be assigned.");
					}
				}
			}
		}

		private void ManageColonies(AIInfo aiInfo)
		{
			PlayerInfo playerInfo = this._db.GetPlayerInfo(this._player.ID);
			float playerSuitability = this._db.GetPlayerSuitability(this._player.ID);
			int val1_1 = 100;
			float val1_2 = 1f;
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			List<double> source1 = new List<double>();
			foreach (ColonyInfo colony in this._db.GetPlayerColoniesByPlayerId(this._player.ID).ToList<ColonyInfo>())
			{
				float val2_1 = 0.0f;
				float val2_2 = 0.0f;
				float val2_3 = 0.0f;
				float val2_4 = 0.0f;
				if (this._player.Faction.Name == "loa")
				{
					double maxImperialPop = Colony.GetMaxImperialPop(this._db, this._db.GetPlanetInfo(colony.OrbitalObjectID));
					double num = this._db.GetPlanetInfo(colony.OrbitalObjectID).Biosphere > 0 ? 0.0 : Math.Max(maxImperialPop - colony.ImperialPop, 0.0);
					source1.Add(num / maxImperialPop);
				}
				PlanetInfo planetInfo = this._db.GetPlanetInfo(colony.OrbitalObjectID);
				int num1;
				if (!dictionary.ContainsKey(this._db.GetOrbitalObjectInfo(colony.OrbitalObjectID).StarSystemID))
				{
					dictionary[this._db.GetOrbitalObjectInfo(colony.OrbitalObjectID).StarSystemID] = 0;
					num1 = 0;
				}
				else
					num1 = dictionary[this._db.GetOrbitalObjectInfo(colony.OrbitalObjectID).StarSystemID];
				if ((double)planetInfo.Infrastructure < 1.0)
					val2_2 = Math.Max(1f - planetInfo.Infrastructure, 0.1f);
				float num2 = Math.Abs(planetInfo.Suitability - playerSuitability);
				if (this._player.Faction.Name == "loa")
					num2 = 0.0f;
				if ((double)num2 > 0.0)
					val2_1 = Math.Max(1f - val2_2, 0.1f);
				double industrialOutput = Colony.GetIndustrialOutput(this._game, colony, planetInfo);
				IEnumerable<FreighterInfo> source2 = this._db.GetFreighterInfosForSystem(colony.CachedStarSystemID).Where<FreighterInfo>((Func<FreighterInfo, bool>)(x => x.PlayerId == aiInfo.PlayerID));
				if (source2.Count<FreighterInfo>() > 0)
				{
					if (industrialOutput <= 1000.0)
					{
						val2_4 = 0.0f;
					}
					else
					{
						List<double> exportsForColony = this._game.GetTradeRatesForWholeExportsForColony(colony.ID);
						if (exportsForColony.Count<double>() > source2.Count<FreighterInfo>() - num1)
						{
							val2_4 = (float)exportsForColony.ElementAt<double>(Math.Max(source2.Count<FreighterInfo>() - num1, 0));
							dictionary[this._db.GetOrbitalObjectInfo(colony.OrbitalObjectID).StarSystemID] = source2.Count<FreighterInfo>() - num1 + dictionary[this._db.GetOrbitalObjectInfo(colony.OrbitalObjectID).StarSystemID];
						}
						else
						{
							val2_4 = (float)exportsForColony.Last<double>();
							dictionary[this._db.GetOrbitalObjectInfo(colony.OrbitalObjectID).StarSystemID] = Math.Max(exportsForColony.Count<double>() - num1, 0) + dictionary[this._db.GetOrbitalObjectInfo(colony.OrbitalObjectID).StarSystemID];
						}
					}
				}
				if ((double)planetInfo.Infrastructure > 0.800000011920929 && (double)num2 < 100.0 && industrialOutput > 1000.0)
				{
					val2_3 = Math.Max((float)(1.0 - ((double)val2_1 + (double)val2_2 + (double)val2_4)), 0.0f);
					if (aiInfo.Stance == AIStance.ARMING)
					{
						if ((double)val2_3 < 0.300000011920929)
							val2_3 = 0.3f;
					}
					else if (aiInfo.Stance == AIStance.CONQUERING || aiInfo.Stance == AIStance.DESTROYING)
					{
						if ((double)val2_3 < 0.5)
							val2_3 = 0.5f;
					}
					else if (aiInfo.Stance == AIStance.DEFENDING && (double)val2_3 < 0.699999988079071)
						val2_3 = 0.7f;
				}
				if (playerInfo.Savings < -1000000.0)
				{
					val2_3 = 0.0f;
					if (source2.Count<FreighterInfo>() > 0)
						val2_4 = 0.0f;
				}
				float num3 = Math.Max(0.0f, val2_1);
				float num4 = Math.Max(0.0f, val2_2);
				float num5 = Math.Max(0.0f, val2_3);
				float num6 = Math.Max(0.0f, val2_4);
				float num7 = num3 + num4 + num5 + num6;
				if ((double)num7 > 0.0 && (double)num7 != 1.0)
				{
					float num8 = 1f / num7;
					num3 *= num8;
					num4 *= num8;
					num5 *= num8;
					num6 *= num8;
				}
				colony.TerraRate = num3;
				colony.InfraRate = num4;
				colony.ShipConRate = num5;
				colony.TradeRate = num6;
				colony.CivilianWeight = 0.75f;
				if (((IEnumerable<ColonyFactionInfo>)colony.Factions).Any<ColonyFactionInfo>())
					val1_1 = Math.Min(val1_1, (int)((IEnumerable<ColonyFactionInfo>)colony.Factions).Average<ColonyFactionInfo>((Func<ColonyFactionInfo, int>)(x => x.Morale)));
				val1_2 = Math.Min(val1_2, colony.EconomyRating);
				this._db.UpdateColony(colony);
			}
			double num9 = source1.Count > 0 ? source1.Average() : 0.0;
			List<AssetDatabase.MoralModifier> moraleEffects = GameSession.GetMoraleEffects(this._game.App, MoralEvent.ME_TAX_INCREASED, this._player.ID);
			List<AssetDatabase.MoralModifier> moralModifierList = new List<AssetDatabase.MoralModifier>();
			if ((double)val1_2 >= 1.0)
				moralModifierList = GameSession.GetMoraleEffects(this._game.App, MoralEvent.ME_ECONOMY_100, this._player.ID);
			else if ((double)val1_2 >= 0.850000023841858)
				moralModifierList = GameSession.GetMoraleEffects(this._game.App, MoralEvent.ME_ECONOMY_ABOVE_85, this._player.ID);
			int val1_3 = 0;
			foreach (AssetDatabase.MoralModifier moralModifier in moraleEffects)
				val1_3 = Math.Max(val1_3, Math.Abs(moralModifier.value));
			int val1_4 = int.MaxValue;
			foreach (AssetDatabase.MoralModifier moralModifier in moralModifierList)
				val1_4 = Math.Min(val1_4, moralModifier.value);
			if (this._player.Faction.Name == "loa" && num9 > 0.0)
			{
				this._db.UpdateTaxRate(this._player.ID, (float)Math.Max(0.05 - num9 * 0.05, 0.02));
			}
			else
			{
				float val2 = this._player.Faction.Name == "zuul" ? 0.07f : 0.08f;
				float num1 = 0.06f;
				if (val1_4 < 20)
				{
					if (val1_3 == 0)
					{
						num1 = val2;
					}
					else
					{
						int num2;
						for (num2 = 0; num2 < 5; ++num2)
						{
							float num3 = (float)num2 * 2f;
							if ((double)Math.Abs(val1_3) * (double)num3 > (double)val1_4)
							{
								num2 = Math.Max(num2 - 1, 0);
								break;
							}
						}
						num1 = Math.Min((float)(0.0500000007450581 + 0.00999999977648258 * (double)num2), val2);
					}
				}
				float num4 = (double)val1_2 >= 0.850000023841858 && val1_1 > 60 || (double)val1_2 >= 1.0 ? num1 : 0.05f;
				float taxRate = val2;
				if (val1_1 < 30)
					taxRate = 0.02f;
				else if (val1_1 < 40)
					taxRate = 0.03f;
				else if (val1_1 < 50)
					taxRate = num4;
				else if (val1_1 < 70)
					taxRate = (double)playerInfo.RateTax < (double)val2 ? num4 : val2;
				if ((double)Math.Abs(playerInfo.RateTax - taxRate) < 0.00499999988824129)
					return;
				this._db.UpdateTaxRate(this._player.ID, taxRate);
			}
		}

		private int GetNearestUngatedSystem(FleetInfo fleetInfo)
		{
			if (fleetInfo == null)
				return 0;
			StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(fleetInfo.SupportingSystemID);
			if (starSystemInfo == (StarSystemInfo)null)
				return 0;
			Faction faction = this.Game.AssetDatabase.GetFaction(this.Game.GameDatabase.GetPlayerFactionID(fleetInfo.PlayerID));
			Vector3 origin = starSystemInfo.Origin;
			int num = 0;
			int maxValue = int.MaxValue;
			foreach (StarSystemInfo visibleStarSystemInfo in this._db.GetVisibleStarSystemInfos(this._player.ID))
			{
				if (!this._db.SystemHasGate(visibleStarSystemInfo.ID, this._player.ID))
				{
					bool flag = false;
					foreach (MissionInfo missionInfo in this._db.GetMissionInfos())
					{
						if (missionInfo.TargetSystemID == visibleStarSystemInfo.ID && missionInfo.Type == MissionType.SURVEY && this._db.GetFleetInfo(missionInfo.FleetID).PlayerID == this._player.ID)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						int? travelTime = Kerberos.Sots.StarFleet.StarFleet.GetTravelTime(this.Game, fleetInfo, visibleStarSystemInfo.ID, faction.CanUseNodeLine(new bool?(true)), new float?(), new float?());
						if (travelTime.HasValue && travelTime.Value < maxValue)
						{
							num = visibleStarSystemInfo.ID;
							maxValue = travelTime.Value;
						}
					}
				}
			}
			return num;
		}

		private int GetNearestNPGRouteSystem(FleetInfo fleetInfo)
		{
			if (fleetInfo == null)
				return 0;
			StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(fleetInfo.SupportingSystemID);
			if (starSystemInfo == (StarSystemInfo)null)
				return 0;
			Faction faction = this.Game.AssetDatabase.GetFaction(this.Game.GameDatabase.GetPlayerFactionID(fleetInfo.PlayerID));
			Vector3 origin = starSystemInfo.Origin;
			int num1 = 0;
			int maxValue = int.MaxValue;
			foreach (StarSystemInfo visibleStarSystemInfo in this._db.GetVisibleStarSystemInfos(this._player.ID))
			{
				if (visibleStarSystemInfo.ID != fleetInfo.SystemID && this._db.GetNodeLineBetweenSystems(fleetInfo.PlayerID, starSystemInfo.ID, visibleStarSystemInfo.ID, false, true) == null)
				{
					bool flag = false;
					foreach (MissionInfo missionInfo in this._db.GetMissionInfos())
					{
						if (missionInfo.TargetSystemID == visibleStarSystemInfo.ID && missionInfo.Type == MissionType.DEPLOY_NPG && this._db.GetFleetInfo(missionInfo.FleetID).PlayerID == this._player.ID)
						{
							flag = true;
							break;
						}
					}
					ExploreRecordInfo exploreRecord = this._db.GetExploreRecord(visibleStarSystemInfo.ID, this._player.ID);
					if (exploreRecord != null && exploreRecord.Explored && !flag)
					{
						int? travelTime = Kerberos.Sots.StarFleet.StarFleet.GetTravelTime(this.Game, fleetInfo, visibleStarSystemInfo.ID, faction.CanUseNodeLine(new bool?(true)), new float?(), new float?());
						float num2 = Math.Abs((starSystemInfo.Origin - visibleStarSystemInfo.Origin).Length);
						if (travelTime.HasValue && travelTime.Value < maxValue && (double)num2 <= 15.0)
						{
							num1 = visibleStarSystemInfo.ID;
							maxValue = travelTime.Value;
						}
					}
				}
			}
			return num1;
		}

		private int GetBestUngatedSystem(FleetInfo fleet)
		{
			int unexploredSystem = this.GetNearestUnexploredSystem(fleet);
			if (unexploredSystem != 0)
				return unexploredSystem;
			return this.GetNearestUngatedSystem(fleet);
		}

		private bool TryGateMission(int fleetID)
		{
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			int targetSystem = this.GetBestUngatedSystem(fleetInfo);
			if (targetSystem != 0)
			{
				IEnumerable<MissionInfo> missionInfos = this._db.GetMissionInfos();
				if (missionInfos != null)
				{
					foreach (MissionInfo missionInfo in missionInfos.Where<MissionInfo>((Func<MissionInfo, bool>)(x =>
				   {
					   if (x.TargetSystemID == targetSystem)
						   return x.Type == MissionType.GATE;
					   return false;
				   })))
					{
						if (this._db.GetFleetInfo(missionInfo.FleetID).PlayerID == fleetInfo.PlayerID)
							return false;
					}
				}
				if (!this._db.SystemHasGate(targetSystem, this._player.ID))
				{
					List<int> designIds = (List<int>)null;
					if (Kerberos.Sots.StarFleet.StarFleet.CanDoGateMissionToTarget(this._game, targetSystem, fleetID, new float?(), new float?(), new float?()))
					{
						Kerberos.Sots.StarFleet.StarFleet.SetGateMission(this._game, fleetID, targetSystem, false, designIds, new int?());
						return true;
					}
				}
				this.TryRelocateMissionCloserToTargetSystem(fleetID, targetSystem);
			}
			return false;
		}

		private bool TryDeployNPGMission(int fleetID)
		{
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			int nearestNpgRouteSystem = this.GetNearestNPGRouteSystem(fleetInfo);
			if (nearestNpgRouteSystem == 0)
				return false;
			List<int> list = Kerberos.Sots.StarFleet.StarFleet.GetAccelGatePercentPointsBetweenSystems(this._db, fleetInfo.SystemID, nearestNpgRouteSystem).ToList<int>();
			if (list.Count < 1)
				list.Clear();
			Kerberos.Sots.StarFleet.StarFleet.SetNPGMission(this._game, fleetID, nearestNpgRouteSystem, true, list, new List<int>(), new int?());
			return true;
		}

		private bool TryTransferMission(int fleetID)
		{
			FleetInfo fleetInfo1 = this._db.GetFleetInfo(fleetID);
			if (this._game.GameDatabase.GetRemainingSupportPoints(this._game, fleetInfo1.SupportingSystemID, fleetInfo1.PlayerID) == 0)
			{
				int forTransferMission = this.FindSystemForTransferMission(fleetID);
				if (forTransferMission <= 0 || !Kerberos.Sots.StarFleet.StarFleet.CanDoTransferMissionToTarget(this._game, forTransferMission, fleetID))
					return false;
				Kerberos.Sots.StarFleet.StarFleet.SetRelocationMission(this._game, fleetID, forTransferMission, false, (List<int>)null);
				return true;
			}
			int num = 0;
			foreach (FleetInfo fleetInfo2 in this._db.GetFleetsBySupportingSystem(fleetInfo1.SupportingSystemID, FleetType.FL_NORMAL))
			{
				if (!fleetInfo2.IsReserveFleet)
					++num;
			}
			if (num <= 1)
				return false;
			int forTransferMission1 = this.FindSystemForTransferMission(fleetID);
			if (forTransferMission1 <= 0 || !Kerberos.Sots.StarFleet.StarFleet.CanDoTransferMissionToTarget(this._game, forTransferMission1, fleetID))
				return false;
			Kerberos.Sots.StarFleet.StarFleet.SetRelocationMission(this._game, fleetID, forTransferMission1, false, (List<int>)null);
			return true;
		}

		private bool HaveNewColonizationMissionEnroute()
		{
			return this._db.GetMissionInfos().Where<MissionInfo>((Func<MissionInfo, bool>)(x =>
		   {
			   if (x.Type != MissionType.COLONIZATION || x.TargetOrbitalObjectID == 0 || this._db.GetFleetInfo(x.FleetID).PlayerID != this._player.ID)
				   return false;
			   if (this._db.GetColonyInfoForPlanet(x.TargetOrbitalObjectID) != null)
				   return this._db.GetColonyInfoForPlanet(x.TargetOrbitalObjectID).PlayerID != this._player.ID;
			   return true;
		   })).Any<MissionInfo>();
		}

		private void ManageDebtLevels()
		{
			if (this._player.Faction.CanUseGate() && this.m_TurnBudget.CurrentSavings < -500000.0 && this.m_ColonizedSystems.Count > 3)
			{
				foreach (SystemDefendInfo colonizedSystem in this.m_ColonizedSystems)
				{
					SystemDefendInfo system = colonizedSystem;
					int? homeworld = this._player.PlayerInfo.Homeworld;
					int systemId = system.SystemID;
					if ((homeworld.GetValueOrDefault() != systemId ? 0 : (homeworld.HasValue ? 1 : 0)) == 0 && !this._db.SystemHasGate(system.SystemID, this._player.ID))
					{
						bool flag = true;
						foreach (MissionInfo missionInfo in this._db.GetMissionsBySystemDest(system.SystemID).Where<MissionInfo>((Func<MissionInfo, bool>)(x =>
					   {
						   if (x.Type == MissionType.GATE)
							   return x.TargetSystemID == system.SystemID;
						   return false;
					   })).ToList<MissionInfo>())
						{
							FleetInfo fleetInfo = this._db.GetFleetInfo(missionInfo.FleetID);
							if (fleetInfo != null && fleetInfo.PlayerID == this._player.ID)
							{
								flag = false;
								break;
							}
						}
						if (flag)
						{
							float playerSuitability = this._db.GetPlayerSuitability(this._player.ID);
							foreach (ColonyInfo colonyInfo in this._db.GetColonyInfosForSystem(system.SystemID).ToList<ColonyInfo>())
							{
								PlanetInfo planetInfo = this._db.GetPlanetInfo(colonyInfo.OrbitalObjectID);
								if ((double)planetInfo.Infrastructure < 0.800000011920929 || (double)Math.Abs(planetInfo.Suitability - playerSuitability) > 100.0)
									GameSession.AbandonColony(this._game.App, colonyInfo.ID);
							}
						}
					}
				}
			}
			if (this.m_TurnBudget.CurrentSavings >= -1000000.0)
				return;
			List<MissionInfo> list1 = this._db.GetMissionInfos().Where<MissionInfo>((Func<MissionInfo, bool>)(x =>
		   {
			   if (x.Type != MissionType.CONSTRUCT_STN)
				   return x.Type == MissionType.UPGRADE_STN;
			   return true;
		   })).ToList<MissionInfo>();
			if (this.m_TurnBudget.CurrentSavings < -2000000.0)
				list1.AddRange((IEnumerable<MissionInfo>)this._db.GetMissionInfos().Where<MissionInfo>((Func<MissionInfo, bool>)(x =>
			  {
				  if (x.Type != MissionType.COLONIZATION)
					  return x.Type == MissionType.SUPPORT;
				  return true;
			  })).ToList<MissionInfo>());
			foreach (MissionInfo missionInfo in list1)
			{
				FleetInfo fleetInfo = this._db.GetFleetInfo(missionInfo.FleetID);
				if (fleetInfo != null && fleetInfo.PlayerID == this._player.ID)
					Kerberos.Sots.StarFleet.StarFleet.CancelMission(this._game, fleetInfo, true);
			}
			foreach (SystemDefendInfo colonizedSystem in this.m_ColonizedSystems)
			{
				foreach (BuildOrderInfo buildOrderInfo in this._db.GetBuildOrdersForSystem(colonizedSystem.SystemID).ToList<BuildOrderInfo>())
				{
					BuildOrderInfo bi = buildOrderInfo;
					if ((double)bi.Progress / (double)bi.ProductionTarget <= 0.899999976158142)
					{
						if (bi.InvoiceID.HasValue)
						{
							List<BuildOrderInfo> list2 = this._db.GetBuildOrdersForInvoiceInstance(bi.InvoiceID.Value).ToList<BuildOrderInfo>();
							DesignInfo designInfo = this._db.GetDesignInfo(bi.DesignID);
							if (!designInfo.isPrototyped && list2.Any<BuildOrderInfo>((Func<BuildOrderInfo, bool>)(x =>
						   {
							   if (x != bi)
								   return x.DesignID == bi.DesignID;
							   return false;
						   })))
							{
								BuildOrderInfo buildOrder = list2.First<BuildOrderInfo>((Func<BuildOrderInfo, bool>)(x =>
							   {
								   if (x != bi)
									   return x.DesignID == bi.DesignID;
								   return false;
							   }));
								buildOrder.ProductionTarget = designInfo.GetPlayerProductionCost(this._db, this._player.ID, true, new float?());
								this._db.UpdateBuildOrder(buildOrder);
							}
						}
						this._db.RemoveBuildOrder(bi.ID);
					}
				}
			}
		}

		private double GetUsableSavings(Budget b)
		{
			if (b.NetSavingsIncome < b.NetSavingsLoss)
				return 0.0;
			return b.CurrentSavings - 2000000.0;
		}

		private double GetUsableBuildFunds(Budget b, double minSavings)
		{
			if (b.NetSavingsIncome < b.NetSavingsLoss && b.CurrentSavings < 4000000.0)
				return 0.0;
			return b.CurrentSavings - minSavings + b.NetSavingsIncome;
		}

		private bool canBudget(Budget b)
		{
			if (b.NetSavingsIncome >= b.NetSavingsLoss || b.CurrentSavings >= 4000000.0)
				return b.CurrentSavings >= 500000.0;
			return false;
		}

		private double GetAvailableColonySupportBudget()
		{
			double num1 = 0.4;
			double num2;
			if (this.m_IsOldSave)
			{
				Budget budget = Budget.GenerateBudget(this._game, this._db.GetPlayerInfo(this._player.ID), (IEnumerable<DesignInfo>)null, BudgetProjection.Actual);
				num2 = (budget.TotalRevenue - budget.ColonySupportExpenses) * num1;
			}
			else
				num2 = this.m_TurnBudget.TotalRevenue * num1 - this.m_TurnBudget.ColonySupportExpenses;
			return num2;
		}

		private double GetAvailableStationSupportBudget()
		{
			if (this.m_TurnBudget.CurrentSavings < 500000.0)
				return 0.0;
			return Math.Max((this.m_TurnBudget.TotalRevenue - this.m_TurnBudget.ColonySupportExpenses) * 0.5 - this.m_TurnBudget.CurrentStationUpkeepExpenses, 0.0);
		}

		private double GetAvailableFleetSupportBudget()
		{
			return Math.Max(this.m_TurnBudget.TotalRevenue * 0.5 * 0.6 - this.m_TurnBudget.CurrentShipUpkeepExpenses, 0.0);
		}

		private double GetAvailableDefenseBudget()
		{
			return Math.Max(this.GetUsableBuildFunds(Budget.GenerateBudget(this._game, this._db.GetPlayerInfo(this._player.ID), (IEnumerable<DesignInfo>)null, BudgetProjection.Actual), 1500000.0) * 0.2, 0.0);
		}

		private double GetAvailableFillFleetConstructionBudget()
		{
			double projectedSavings = this.m_TurnBudget.ProjectedSavings;
			if (this._player.Faction.Name != "hiver")
				projectedSavings -= 250000.0;
			return Math.Max(projectedSavings * 0.2, 0.0);
		}

		private double GetAvailablePrototypeShipConstructionBudget()
		{
			if (!this.canBudget(this.m_TurnBudget))
				return 0.0;
			double val1 = this.GetUsableBuildFunds(this.m_TurnBudget, 5000000.0);
			if (val1 > 0.0)
			{
				double num1 = 600000.0;
				double num2 = val1 - num1;
				if (num2 > 0.0)
					val1 = num1 * 0.8 + num2 * 0.4;
				else
					val1 *= 0.8;
			}
			return Math.Max(val1, 0.0);
		}

		private double GetAvailableShipConstructionBudget(AIStance stance)
		{
			if (!this.canBudget(this.m_TurnBudget))
				return 0.0;
			return Math.Max(this.GetUsableBuildFunds(this.m_TurnBudget, 0.0) * 0.75, 0.0);
		}

		private bool TryColonizationMission(int fleetID)
		{
			if (this._db.GetMissionByFleetID(fleetID) != null)
				return false;
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			int num1 = 0;
			if (!this.HaveNewColonizationMissionEnroute())
				num1 = this.GetBestPlanetToColonize(this.GetAvailableColonySupportBudget());
			if (num1 > 0)
			{
				int starSystemId = this._db.GetOrbitalObjectInfo(num1).StarSystemID;
				bool flag = false;
				List<int> intList = new List<int>();
				int num2 = 10;
				int designForColonizer = Kerberos.Sots.StarFleet.StarFleet.GetDesignForColonizer(this._game, this._player.ID);
				if (designForColonizer != 0)
				{
					for (int index = 0; index <= num2; index += 2)
					{
						if (index > 0)
							intList.Add(designForColonizer);
						flag = Kerberos.Sots.StarFleet.StarFleet.CanDoColonizeMissionToTarget(this._game, starSystemId, fleetInfo.ID, new float?(), new float?(), new float?());
						if (flag && Kerberos.Sots.StarFleet.StarFleet.ProjectNumColonizationRuns(this._game, num1, fleetInfo.ID, intList, 10) > 10)
							flag = false;
						if (flag)
							break;
					}
				}
				else
				{
					flag = Kerberos.Sots.StarFleet.StarFleet.CanDoColonizeMissionToTarget(this._game, starSystemId, fleetInfo.ID, new float?(), new float?(), new float?());
					if (flag && Kerberos.Sots.StarFleet.StarFleet.ProjectNumColonizationRuns(this._game, num1, fleetInfo.ID, intList, 10) > 10)
						flag = false;
				}
				if (flag)
				{
					OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(num1);
					Kerberos.Sots.StarFleet.StarFleet.SetColonizationMission(this._game, fleetInfo.ID, orbitalObjectInfo.StarSystemID, false, num1, intList, new int?());
					return true;
				}
				this.TryRelocateMissionCloserToTargetSystem(fleetID, this._db.GetOrbitalObjectInfo(num1).StarSystemID);
			}
			return false;
		}

		private static void TraceVerbose(string message)
		{
			App.Log.Trace(message, "ai", Kerberos.Sots.Engine.LogLevel.Verbose);
		}

		private bool TrySurveyMission(int fleetID)
		{
			if (this._db.GetMissionByFleetID(fleetID) != null)
				return false;
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			int unexploredSystem = this.GetNearestUnexploredSystem(fleetInfo);
			if (unexploredSystem != 0)
			{
				if (Kerberos.Sots.StarFleet.StarFleet.CanDoSurveyMissionToTarget(this._game, unexploredSystem, fleetInfo.ID, new float?(), new float?(), new float?()))
				{
					MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.SURVEY, StationType.INVALID_TYPE, fleetInfo.ID, unexploredSystem, 0, (List<int>)null, 1, false, new float?(), new float?());
					if (missionEstimate.TurnsToTarget >= 0)
					{
						int num = missionEstimate.TurnsToTarget + missionEstimate.TurnsToReturn + missionEstimate.TurnsAtTarget;
						Kerberos.Sots.StarFleet.StarFleet.SetSurveyMission(this._game, fleetInfo.ID, unexploredSystem, false, (List<int>)null, new int?());
						StrategicAI.TraceVerbose("Mission estimate, total " + (object)num + " turns.");
						return true;
					}
				}
				this.TryRelocateMissionCloserToTargetSystem(fleetID, unexploredSystem);
			}
			return false;
		}

		public void HandleSurveyMissionCompleted(int fleetID, int systemID)
		{
			List<ShipInfo> source = new List<ShipInfo>();
			foreach (ShipInfo shipInfo in this._game.GameDatabase.GetShipInfoByFleetID(fleetID, true).ToList<ShipInfo>())
			{
				if (((IEnumerable<DesignSectionInfo>)shipInfo.DesignInfo.DesignSections).Select<DesignSectionInfo, ShipSectionAsset>((Func<DesignSectionInfo, ShipSectionAsset>)(x => x.ShipSectionAsset)).FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.IsTrapShip)) != null)
					source.Add(shipInfo);
			}
			if (source.Count <= 0)
				return;
			List<PlanetInfo> planetInfoList = new List<PlanetInfo>();
			foreach (PlanetInfo planetInfo in ((IEnumerable<PlanetInfo>)this._game.GameDatabase.GetStarSystemPlanetInfos(systemID)).ToList<PlanetInfo>())
			{
				if (this._game.AssetDatabase.IsPotentialyHabitable(planetInfo.Type) && this._game.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID) == null)
					planetInfoList.Add(planetInfo);
			}
			if (planetInfoList.Count <= 0)
				return;
			int num = this._random.NextInclusive(0, Math.Min(source.Count, planetInfoList.Count));
			for (int index = 0; index < num; ++index)
			{
				PlanetInfo planetInfo = this._random.Choose<PlanetInfo>((IList<PlanetInfo>)planetInfoList);
				ShipInfo trapShip = source.First<ShipInfo>();
				this._game.SetAColonyTrap(trapShip, this._player.ID, systemID, planetInfo.ID);
				planetInfoList.Remove(planetInfo);
				source.Remove(trapShip);
				if (planetInfoList.Count == 0 || source.Count == 0)
					break;
			}
		}

		private bool ValidateConstructionFleet(int fleetID)
		{
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			FleetInfo reserveFleetInfo = this._db.InsertOrGetReserveFleetInfo(fleetInfo.SupportingSystemID, fleetInfo.PlayerID);
			int constructionShips = Kerberos.Sots.StarFleet.StarFleet.GetNumConstructionShips(this._game, fleetInfo.ID);
			int num = 0;
			if (reserveFleetInfo != null)
				num = Kerberos.Sots.StarFleet.StarFleet.GetNumConstructionShips(this._game, reserveFleetInfo.ID);
			if (constructionShips + num < 1)
				return false;
			if (constructionShips < 5 && num > 0)
			{
				foreach (ShipInfo shipInfo in this._db.GetShipInfoByFleetID(reserveFleetInfo.ID, false).ToList<ShipInfo>())
				{
					foreach (string designSectionName in this._game.GameDatabase.GetDesignSectionNames(shipInfo.DesignID))
					{
						string sectionName = designSectionName;
						ShipSectionAsset shipSectionAsset = this._game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sectionName));
						if (shipSectionAsset != null && shipSectionAsset.isConstructor)
						{
							this._db.TransferShip(shipInfo.ID, fleetInfo.ID);
							++constructionShips;
							break;
						}
					}
					if (constructionShips >= 5)
						break;
				}
			}
			return true;
		}

		private bool TryUpgradeMission(AIInfo aiInfo, int fleetID)
		{
			if (this._db.GetMissionByFleetID(fleetID) != null || !this.ValidateConstructionFleet(fleetID))
				return false;
			return this.CreateConstructionMission(aiInfo, fleetID, true);
		}

		private bool TryConstructionMission(AIInfo aiInfo, int fleetID)
		{
			if (this._db.GetMissionByFleetID(fleetID) != null || !this.ValidateConstructionFleet(fleetID))
				return false;
			return this.CreateConstructionMission(aiInfo, fleetID, false);
		}

		private bool OptimizeFleetForMission(FleetInfo fleet, MissionType missionType)
		{
			return true;
		}

		private bool TryGateConstructionMission(int fleetID)
		{
			if (this._db.GetMissionByFleetID(fleetID) != null)
				return false;
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			int systemForHiverGate = this.GetSystemForHiverGate(fleetInfo.SystemID);
			if (systemForHiverGate <= 0 || !Kerberos.Sots.StarFleet.StarFleet.CanDoConstructionMissionToTarget(this._game, systemForHiverGate, fleetInfo.ID, false))
				return false;
			Kerberos.Sots.StarFleet.StarFleet.SetConstructionMission(this._game, fleetInfo.ID, systemForHiverGate, false, 0, (List<int>)null, StationType.GATE, new int?());
			return true;
		}

		private bool TryPatrolMission(int fleetID)
		{
			if (this._db.GetMissionByFleetID(fleetID) != null)
				return false;
			int patrolTargetSystem = this.GetBestPatrolTargetSystem(fleetID);
			if (patrolTargetSystem <= 0)
				return false;
			if (Kerberos.Sots.StarFleet.StarFleet.CanDoPatrolMissionToTarget(this._game, patrolTargetSystem, fleetID, new float?(), new float?(), new float?()))
			{
				Kerberos.Sots.StarFleet.StarFleet.SetPatrolMission(this._game, fleetID, patrolTargetSystem, false, (List<int>)null, new int?());
				return true;
			}
			this.TryRelocateMissionCloserToTargetSystem(fleetID, patrolTargetSystem);
			return false;
		}

		private bool VerifyStrikeMission(int orbitalObject)
		{
			return this.VerifyStrikeMission(this._db.GetOrbitalObjectInfo(orbitalObject));
		}

		private bool VerifyStrikeMission(OrbitalObjectInfo orbit)
		{
			if (orbit == null)
				return false;
			foreach (MissionInfo missionInfo in this._db.GetMissionInfos().Where<MissionInfo>((Func<MissionInfo, bool>)(x => x.TargetSystemID == orbit.StarSystemID)))
			{
				if (this._db.GetFleetInfo(missionInfo.FleetID).PlayerID == this._player.ID && missionInfo.Type == MissionType.STRIKE)
					return false;
			}
			return true;
		}

		private bool TryStrikeMission(int fleetID)
		{
			if (this._db.GetMissionByFleetID(fleetID) != null)
				return false;
			int targetOrbitalObject = this.GetBestStrikeTargetOrbitalObject(fleetID);
			if (targetOrbitalObject == 0)
				return false;
			OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(targetOrbitalObject);
			if (Kerberos.Sots.StarFleet.StarFleet.CanDoStrikeMissionToTarget(this._game, orbitalObjectInfo.StarSystemID, fleetID, new float?(), new float?(), new float?()))
			{
				Kerberos.Sots.StarFleet.StarFleet.SetStrikeMission(this._game, fleetID, orbitalObjectInfo.StarSystemID, false, targetOrbitalObject, 0, (List<int>)null);
				return true;
			}
			this.TryRelocateMissionCloserToTargetSystem(fleetID, orbitalObjectInfo.StarSystemID);
			return false;
		}

		private int GetBestStrikeTargetOrbitalObject(int fleetID)
		{
			Vector3 origin1 = this._db.GetStarSystemInfo(this._db.GetFleetSupportingSystem(fleetID)).Origin;
			float driveSpeedForPlayer = this._db.FindCurrentDriveSpeedForPlayer(this._player.ID);
			int num1 = 0;
			List<int> source1 = new List<int>();
			List<int> list = this._db.GetPlayerIDs().Where<int>((Func<int, bool>)(x => x != this._player.ID)).ToList<int>();
			List<int> allies = list.Where<int>((Func<int, bool>)(x => this._db.GetDiplomacyStateBetweenPlayers(this._player.ID, x) == DiplomacyState.ALLIED)).ToList<int>();
			list.RemoveAll((Predicate<int>)(x => allies.Contains(x)));
			foreach (int num2 in list)
			{
				int pID = num2;
				if (this._db.GetDiplomacyStateBetweenPlayers(this._player.ID, pID) == DiplomacyState.WAR)
				{
					bool flag = true;
					foreach (int num3 in allies)
					{
						int ally = num3;
						if (this._db.GetTreatyInfos().Where<TreatyInfo>((Func<TreatyInfo, bool>)(x =>
					   {
						   if (!x.Active || x.InitiatingPlayerId != pID && x.InitiatingPlayerId != ally)
							   return false;
						   if (x.ReceivingPlayerId != pID)
							   return x.ReceivingPlayerId == ally;
						   return true;
					   })).ToList<TreatyInfo>().Any<TreatyInfo>((Func<TreatyInfo, bool>)(x => x.Type == TreatyType.Protectorate)))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						int forStrikeMission = this.GetTargetForStrikeMission(fleetID, pID);
						if (forStrikeMission != 0)
							source1.Add(forStrikeMission);
					}
				}
			}
			if (source1.Count<int>() > 0)
			{
				if (source1.Count<int>() > 1)
				{
					int num2 = 0;
					float num3 = 0.0f;
					foreach (int orbitalObjectID in source1)
					{
						float length = (this._db.GetStarSystemInfo(this._db.GetOrbitalObjectInfo(orbitalObjectID).StarSystemID).Origin - origin1).Length;
						if (num2 == 0 || (double)length < (double)num3)
						{
							num2 = orbitalObjectID;
							num3 = length;
						}
					}
					List<int> source2 = new List<int>();
					foreach (int orbitalObjectID in source1)
					{
						if (orbitalObjectID != num2)
						{
							OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(orbitalObjectID);
							Vector3 origin2 = this._db.GetStarSystemInfo(orbitalObjectInfo.StarSystemID).Origin;
							bool flag = false;
							if ((double)(origin2 - origin1).Length > (double)driveSpeedForPlayer * 2.0)
								flag = true;
							if (!this.VerifyStrikeMission(orbitalObjectInfo))
								flag = true;
							if (flag)
								source2.Add(orbitalObjectID);
						}
					}
					while (source2.Count<int>() > 0)
					{
						source1.Remove(source2.FirstOrDefault<int>());
						source2.Remove(source2.FirstOrDefault<int>());
					}
				}
				else if (this.VerifyStrikeMission(source1.First<int>()))
				{
					if (source1.Count<int>() == 1)
					{
						num1 = source1.FirstOrDefault<int>();
					}
					else
					{
						int index = this._random.Next(source1.Count<int>());
						num1 = source1[index];
					}
				}
			}
			return num1;
		}

		internal bool IsValidInvasionTarget(int orbitalObjectID)
		{
			AIColonyIntel colonyIntelForPlanet = this._db.GetColonyIntelForPlanet(this._player.ID, orbitalObjectID);
			return colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID != 0 && this._db.GetDiplomacyStateBetweenPlayers(this._player.ID, colonyIntelForPlanet.OwningPlayerID) == DiplomacyState.WAR;
		}

		private bool CanSendAnotherInvasionFleet(int targetOrbitalObjectID)
		{
			int[] recentCombatTurns = this._db.GetRecentCombatTurns(this._db.GetOrbitalObjectInfo(targetOrbitalObjectID).StarSystemID, this._db.GetTurnCount() - 5);
			if (recentCombatTurns.Length > 50)
				return false;
			int num1 = recentCombatTurns.Length + 1;
			int num2 = 0;
			foreach (MissionInfo missionInfo in this._db.GetMissionsByPlanetDest(targetOrbitalObjectID))
			{
				FleetInfo fleetInfo = this._db.GetFleetInfo(missionInfo.FleetID);
				if (missionInfo.Type == MissionType.INVASION && fleetInfo.PlayerID == this._player.ID)
					++num2;
			}
			return num2 < num1;
		}

		private int GetBestInvasionTargetOrbitalObject(int fleetID)
		{
			List<int> targets = new List<int>();
			foreach (int playerId in this._db.GetPlayerIDs())
			{
				if (this._db.GetDiplomacyStateBetweenPlayers(this._player.ID, playerId) == DiplomacyState.WAR)
				{
					int? targetForInvasion = this.GetTargetForInvasion(fleetID, playerId);
					if (targetForInvasion.HasValue && this.CanSendAnotherInvasionFleet(targetForInvasion.Value))
						targets.Add(targetForInvasion.Value);
				}
			}
			List<TmpWeightedCheck> source = new List<TmpWeightedCheck>();
			int num1 = int.MaxValue;
			int num2 = int.MaxValue;
			foreach (MissionManagerTargetInfo managerTargetInfo in this._missionMan.Targets.Where<MissionManagerTargetInfo>((Func<MissionManagerTargetInfo, bool>)(x => targets.Any<int>((Func<int, bool>)(y => x.OrbitalObjectID == y)))))
			{
				MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.INVASION, StationType.INVALID_TYPE, fleetID, this._db.GetOrbitalObjectInfo(managerTargetInfo.OrbitalObjectID).StarSystemID, managerTargetInfo.OrbitalObjectID, (List<int>)null, 1, false, new float?(), new float?());
				int num3 = missionEstimate.TurnsForConstruction + missionEstimate.TurnsToTarget + this._db.GetTurnCount();
				if (num3 <= managerTargetInfo.ArrivalTurn)
				{
					if (num3 < num1)
					{
						num1 = num3;
						num2 = managerTargetInfo.ArrivalTurn;
					}
					source.Add(new TmpWeightedCheck()
					{
						TargetID = managerTargetInfo.OrbitalObjectID,
						ETA = num3,
						MissionETA = managerTargetInfo.ArrivalTurn
					});
				}
			}
			if (num1 < num2 || source.Count == 0)
				return 0;
			source.OrderBy<TmpWeightedCheck, int>((Func<TmpWeightedCheck, int>)(x => x.ETA));
			return source[this._random.Next(Math.Min(source.Count - 1, 5))].TargetID;
		}

		private int GetBestPatrolTargetSystem(int fleetID)
		{
			this._db.GetFleetInfo(fleetID);
			IEnumerable<MissionInfo> missionInfos1 = this._db.GetMissionInfos();
			int num = 0;
			foreach (ColonyInfo colonyInfo in this._db.GetPlayerColoniesByPlayerId(this._player.ID))
			{
				OrbitalObjectInfo ooi = this._db.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID);
				if (missionInfos1 != null)
				{
					IEnumerable<MissionInfo> missionInfos2 = missionInfos1.Where<MissionInfo>((Func<MissionInfo, bool>)(x =>
				   {
					   if (x.TargetSystemID != ooi.StarSystemID)
						   return x.TargetOrbitalObjectID == ooi.ID;
					   return true;
				   }));
					bool flag = false;
					foreach (MissionInfo missionInfo in missionInfos2)
					{
						FleetInfo fleetInfo = this._db.GetFleetInfo(missionInfo.FleetID);
						if (missionInfo.Type == MissionType.PATROL && fleetInfo.PlayerID == this._player.ID)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						foreach (MissionInfo missionInfo in missionInfos2)
						{
							if (missionInfo.Type == MissionType.INVASION || missionInfo.Type == MissionType.STRIKE)
							{
								num = ooi.StarSystemID;
								break;
							}
						}
						if (num > 0)
							break;
					}
				}
			}
			return num;
		}

		private bool TryInvasionMission(int fleetID)
		{
			if (this._db.GetMissionByFleetID(fleetID) != null)
				return false;
			int targetOrbitalObject = this.GetBestInvasionTargetOrbitalObject(fleetID);
			if (targetOrbitalObject == 0)
				return false;
			OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(targetOrbitalObject);
			if (Kerberos.Sots.StarFleet.StarFleet.CanDoInvasionMissionToTarget(this._game, orbitalObjectInfo.StarSystemID, fleetID, new float?(), new float?(), new float?()))
			{
				Kerberos.Sots.StarFleet.StarFleet.SetInvasionMission(this._game, fleetID, orbitalObjectInfo.StarSystemID, false, targetOrbitalObject, (List<int>)null);
				return true;
			}
			this.TryRelocateMissionCloserToTargetSystem(fleetID, orbitalObjectInfo.StarSystemID);
			return false;
		}

		private bool TryInterdictionMission(int fleetID)
		{
			if (this._db.GetMissionByFleetID(fleetID) != null)
				return false;
			this._db.GetFleetInfo(fleetID);
			return false;
		}

		private void SetResearchOrders(AIInfo aiInfo)
		{
			int researchingTechId = this._db.GetPlayerResearchingTechID(this._player.ID);
			int feasibilityStudyTechId = this._db.GetPlayerFeasibilityStudyTechId(this._player.ID);
			if (researchingTechId != 0 || feasibilityStudyTechId != 0)
				return;
			List<PlayerTechInfo> desiredTech = new List<PlayerTechInfo>();
			if (this._player.Faction.Name == "zuul")
			{
				PlayerTechInfo playerTechInfo = this._db.GetPlayerTechInfos(this._player.ID).FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "POL_Tribute_Systems"));
				if (playerTechInfo != null && playerTechInfo.State != TechStates.Researching && playerTechInfo.State != TechStates.Researched)
					desiredTech.Add(playerTechInfo);
			}
			Kerberos.Sots.Data.TechnologyFramework.Tech tech = this.Game.AssetDatabase.AIResearchFramework.AISelectNextTech(this, desiredTech, (Dictionary<string, int>)null);
			PlayerTechInfo playerTechInfo1 = (PlayerTechInfo)null;
			if (tech != null)
				playerTechInfo1 = this._db.GetPlayerTechInfo(this._player.ID, this._db.GetTechID(tech.Id));
			if (playerTechInfo1 != null)
			{
				if (playerTechInfo1.State == TechStates.Branch)
				{
					ResearchScreenState.StartFeasibilityStudy(this._db, this._player.ID, playerTechInfo1.TechID);
					StrategicAI.TraceVerbose("AI Player " + (object)this._player.ID + " studying feasibility " + playerTechInfo1.TechFileID);
				}
				else
				{
					this._db.UpdatePlayerTechState(this._player.ID, playerTechInfo1.TechID, TechStates.Researching);
					StrategicAI.TraceVerbose("AI Player " + (object)this._player.ID + " researching " + playerTechInfo1.TechFileID);
				}
			}
			else
				StrategicAI.TraceVerbose("AI Player " + (object)this._player.ID + " has no research!");
		}

		public List<PlayerTechInfo> GetDesiredTechs()
		{
			List<PlayerTechInfo> source = new List<PlayerTechInfo>();
			StrategicAI.DesignConfigurationInfo configurationInfo1 = new StrategicAI.DesignConfigurationInfo();
			StrategicAI.DesignConfigurationInfo configurationInfo2 = new StrategicAI.DesignConfigurationInfo();
			IEnumerable<CombatData> combatsForPlayer = this._game.CombatData.GetCombatsForPlayer(this._game.GameDatabase, this._player.ID, 10);
			StrategicAI.TraceVerbose("Entered StrategicAI.GetDesiredTechs");
			int size1 = 0;
			int size2 = 0;
			if (combatsForPlayer.Count<CombatData>() > 0)
			{
				foreach (CombatData combatData in combatsForPlayer)
				{
					foreach (PlayerCombatData playerCombatData in combatData.GetPlayers().ToList<PlayerCombatData>())
					{
						Player playerObject = this._game.GetPlayerObject(playerCombatData.PlayerID);
						if (playerObject.IsStandardPlayer)
						{
							if (playerObject.ID == this._player.ID)
							{
								foreach (ShipData shipData in playerCombatData.ShipData)
								{
									DesignInfo designInfo = this._db.GetDesignInfo(shipData.designID);
									configurationInfo2 += StrategicAI.GetDesignConfigurationInfo(this._game, designInfo);
									++size2;
								}
							}
							else if (this._db.GetDiplomacyStateBetweenPlayers(this._player.ID, playerObject.ID) == DiplomacyState.WAR)
							{
								foreach (ShipData shipData in playerCombatData.ShipData)
								{
									DesignInfo designInfo = this._db.GetDesignInfo(shipData.designID);
									configurationInfo1 += StrategicAI.GetDesignConfigurationInfo(this._game, designInfo);
									++size1;
								}
							}
						}
					}
				}
			}
			if (size1 == 0)
				return source;
			if (size2 > 0)
				configurationInfo2.Average(size2);
			configurationInfo1.Average(size1);
			float num = 0.5f;
			List<PlayerTechInfo> list = this._db.GetPlayerTechInfos(this._player.ID).ToList<PlayerTechInfo>();
			if ((double)configurationInfo1.PointDefense > (double)num)
			{
				foreach (LogicalWeapon logicalWeapon in this._db.AssetDatabase.Weapons.Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => !((IEnumerable<WeaponEnums.WeaponTraits>)x.Traits).Any<WeaponEnums.WeaponTraits>((Func<WeaponEnums.WeaponTraits, bool>)(y => y == WeaponEnums.WeaponTraits.Tracking)))).ToList<LogicalWeapon>())
				{
					if (!((IEnumerable<WeaponEnums.WeaponTraits>)logicalWeapon.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.PlanetKilling))
					{
						foreach (Kerberos.Sots.Data.WeaponFramework.Tech requiredTech in logicalWeapon.RequiredTechs)
						{
							Kerberos.Sots.Data.WeaponFramework.Tech req = requiredTech;
							if (!source.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == req.Name)))
							{
								PlayerTechInfo tech = list.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == req.Name));
								if (this.ShouldResearchTech(tech))
									source.Add(tech);
							}
						}
					}
				}
			}
			if ((double)configurationInfo1.BallisticsDefense > (double)num)
			{
				foreach (LogicalWeapon logicalWeapon in this._db.AssetDatabase.Weapons.Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x =>
			   {
				   if (!((IEnumerable<WeaponEnums.WeaponTraits>)x.Traits).Any<WeaponEnums.WeaponTraits>((Func<WeaponEnums.WeaponTraits, bool>)(y => y == WeaponEnums.WeaponTraits.Ballistic)))
					   return x.PayloadType != WeaponEnums.PayloadTypes.Missile;
				   return false;
			   })).ToList<LogicalWeapon>())
				{
					if (!((IEnumerable<WeaponEnums.WeaponTraits>)logicalWeapon.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.PlanetKilling))
					{
						foreach (Kerberos.Sots.Data.WeaponFramework.Tech requiredTech in logicalWeapon.RequiredTechs)
						{
							Kerberos.Sots.Data.WeaponFramework.Tech req = requiredTech;
							if (!source.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == req.Name)))
							{
								PlayerTechInfo tech = list.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == req.Name));
								if (this.ShouldResearchTech(tech))
									source.Add(tech);
							}
						}
					}
				}
			}
			if ((double)configurationInfo1.EnergyDefense > (double)num)
			{
				foreach (LogicalWeapon logicalWeapon in this._db.AssetDatabase.Weapons.Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => !((IEnumerable<WeaponEnums.WeaponTraits>)x.Traits).Any<WeaponEnums.WeaponTraits>((Func<WeaponEnums.WeaponTraits, bool>)(y =>
			 {
				 if (y != WeaponEnums.WeaponTraits.Energy)
					 return y == WeaponEnums.WeaponTraits.Laser;
				 return true;
			 })))).ToList<LogicalWeapon>())
				{
					if (!((IEnumerable<WeaponEnums.WeaponTraits>)logicalWeapon.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.PlanetKilling))
					{
						foreach (Kerberos.Sots.Data.WeaponFramework.Tech requiredTech in logicalWeapon.RequiredTechs)
						{
							Kerberos.Sots.Data.WeaponFramework.Tech req = requiredTech;
							if (!source.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == req.Name)))
							{
								PlayerTechInfo tech = list.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == req.Name));
								if (this.ShouldResearchTech(tech))
									source.Add(tech);
							}
						}
					}
				}
			}
			if ((double)configurationInfo1.MissileWeapons > (double)num)
			{
				foreach (LogicalWeapon logicalWeapon in this._db.AssetDatabase.Weapons.Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.IsPDWeapon())).ToList<LogicalWeapon>())
				{
					if (!((IEnumerable<WeaponEnums.WeaponTraits>)logicalWeapon.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.PlanetKilling))
					{
						foreach (Kerberos.Sots.Data.WeaponFramework.Tech requiredTech in logicalWeapon.RequiredTechs)
						{
							Kerberos.Sots.Data.WeaponFramework.Tech req = requiredTech;
							if (!source.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == req.Name)))
							{
								PlayerTechInfo tech = list.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == req.Name));
								if (this.ShouldResearchTech(tech))
									source.Add(tech);
							}
						}
					}
				}
			}
			if ((double)configurationInfo1.BallisticsWeapons > (double)num)
			{
				foreach (string str in new List<string>()
		{
		  "IND_Polysteel",
		  "IND_MagnoCeramic_Latices",
		  "IND_Quark_Resonators",
		  "IND_Adamantine_Alloys",
		  "SLD_Shield_Mk._I",
		  "SLD_Shield_Mk._II",
		  "SLD_Shield_Mk._III",
		  "SLD_Shield_Mk._IV",
		  "SLD_Deflector_Shields",
		  "LD_Grav_Shields"
		})
				{
					string techID = str;
					if (!source.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == techID)))
					{
						PlayerTechInfo tech = list.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == techID));
						if (this.ShouldResearchTech(tech))
							source.Add(tech);
					}
				}
			}
			if ((double)configurationInfo1.EnergyWeapons > (double)num)
			{
				foreach (string str in new List<string>()
		{
		  "IND_Polysteel",
		  "IND_MagnoCeramic_Latices",
		  "IND_Quark_Resonators",
		  "IND_Adamantine_Alloys",
		  "SLD_Shield_Mk._I",
		  "SLD_Shield_Mk._II",
		  "SLD_Shield_Mk._III",
		  "SLD_Shield_Mk._IV",
		  "SLD_Meson_Shields",
		  "IND_Reflective",
		  "IND_Improved_Reflective",
		  "SLD_Meson_Shields",
		  "SLD_Disruptor_Shields",
		  "NRG_Energy_Absorbers"
		})
				{
					string techID = str;
					if (!source.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == techID)))
					{
						PlayerTechInfo tech = list.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == techID));
						if (this.ShouldResearchTech(tech))
							source.Add(tech);
					}
				}
			}
			if ((double)configurationInfo1.HeavyBeamWeapons > (double)num)
			{
				foreach (string str in new List<string>()
		{
		  "SLD_Meson_Shields",
		  "SLD_Disruptor_Shields",
		  "NRG_Energy_Absorbers"
		})
				{
					string techID = str;
					if (!source.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == techID)))
					{
						PlayerTechInfo tech = list.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == techID));
						if (this.ShouldResearchTech(tech))
							source.Add(tech);
					}
				}
			}
			if (size2 > 0 && (double)configurationInfo1.Maxspeed > (double)configurationInfo2.Maxspeed * 1.10000002384186)
			{
				foreach (string str in new List<string>()
		{
		  "NRG_Ionic_Thruster",
		  "NRG_Small_Scale_Fusion"
		})
				{
					string techID = str;
					if (!source.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == techID)))
					{
						PlayerTechInfo tech = list.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == techID));
						if (this.ShouldResearchTech(tech))
							source.Add(tech);
					}
				}
			}
			return source;
		}

		private bool ShouldResearchTech(PlayerTechInfo tech)
		{
			return tech != null && tech.State != TechStates.Researching && tech.State != TechStates.Researched;
		}

		private int GetTechFamilyScore(
		  TechFamilies family,
		  AIStance stance,
		  GameMode mode,
		  List<PlayerTechInfo> playerTechs)
		{
			if ((uint)family <= 15U && mode == GameMode.LeviathanLimit)
			{
				PlayerTechInfo playerTechInfo = playerTechs.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == ""));
				if (playerTechInfo != null && playerTechInfo.State != TechStates.Researched && (playerTechInfo.State != TechStates.Researching && this._db.GetTurnCount() > 75))
					return 50;
			}
			return 1;
		}

		private int PickAFight()
		{
			int num1 = 0;
			float num2 = 0.0f;
			foreach (int playerId1 in this._db.GetPlayerIDs())
			{
				if (playerId1 != this._player.ID && this._db.GetPlayerInfo(playerId1).isStandardPlayer && this._db.IsPlayerAdjacent(this._game, this._player.ID, playerId1))
				{
					switch (this._db.GetDiplomacyStateBetweenPlayers(this._player.ID, playerId1))
					{
						case DiplomacyState.NON_AGGRESSION:
						case DiplomacyState.WAR:
						case DiplomacyState.ALLIED:
						case DiplomacyState.PEACE:
							continue;
						default:
							Player playerObject = this._game.GetPlayerObject(playerId1);
							if (playerObject == null || GameSession.SimAITurns != 0 || !playerObject.IsAI())
							{
								bool flag = false;
								foreach (int playerId2 in this._db.GetPlayerIDs())
								{
									if (playerId1 != playerId2 && this._player.ID != playerId2 && this._db.GetDiplomacyStateBetweenPlayers(playerId1, playerId2) == DiplomacyState.ALLIED)
									{
										flag = true;
										break;
									}
								}
								if (!flag)
								{
									float num3 = this.AssessPlayerStrength(playerId1);
									if ((num1 == 0 || (double)num3 < (double)num2) && this.Random.CoinToss((double)((float)this._db.GetDiplomacyInfo(this._player.ID, playerId1).Relations / (float)((double)DiplomacyInfo.MaxDeplomacyRelations / 2.0 + 100.0))))
									{
										num1 = playerId1;
										num2 = num3;
										continue;
									}
									continue;
								}
								continue;
							}
							continue;
					}
				}
			}
			return num1;
		}

		private int GetNearestUnexploredSystem(FleetInfo fleetInfo)
		{
			if (fleetInfo == null)
				return 0;
			StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(fleetInfo.SupportingSystemID);
			if (starSystemInfo == (StarSystemInfo)null)
				return 0;
			Faction faction = this.Game.AssetDatabase.GetFaction(this.Game.GameDatabase.GetPlayerFactionID(fleetInfo.PlayerID));
			Vector3 origin = starSystemInfo.Origin;
			int num = 0;
			int maxValue = int.MaxValue;
			foreach (StarSystemInfo visibleStarSystemInfo in this._db.GetVisibleStarSystemInfos(this._player.ID))
			{
				if (this._db.GetLastTurnExploredByPlayer(this._player.ID, visibleStarSystemInfo.ID) == 0 && visibleStarSystemInfo.ID != starSystemInfo.ID)
				{
					bool flag = false;
					foreach (MissionInfo missionInfo in this._db.GetMissionInfos())
					{
						if (missionInfo.TargetSystemID == visibleStarSystemInfo.ID && missionInfo.Type == MissionType.SURVEY && this._db.GetFleetInfo(missionInfo.FleetID).PlayerID == this._player.ID)
						{
							flag = true;
							break;
						}
					}
					foreach (FleetInfo fleetInfo1 in this._db.GetFleetInfoBySystemID(visibleStarSystemInfo.ID, FleetType.FL_NORMAL))
					{
						if (this._db.GetDiplomacyInfo(this._player.ID, fleetInfo1.PlayerID).State == DiplomacyState.WAR)
						{
							flag = true;
							break;
						}
					}
					foreach (ColonyInfo colonyInfo in this._db.GetColonyInfosForSystem(visibleStarSystemInfo.ID))
					{
						if (this._db.GetDiplomacyInfo(this._player.ID, colonyInfo.PlayerID).State == DiplomacyState.WAR)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						int? travelTime = Kerberos.Sots.StarFleet.StarFleet.GetTravelTime(this.Game, fleetInfo, visibleStarSystemInfo.ID, faction.CanUseNodeLine(new bool?(true)), new float?(), new float?());
						if (travelTime.HasValue && travelTime.Value < maxValue)
						{
							num = visibleStarSystemInfo.ID;
							maxValue = travelTime.Value;
						}
					}
				}
			}
			return num;
		}

		private int FindSystemForTransferMission(int fleetID)
		{
			int num1 = 0;
			FleetInfo fleetInfo1 = this._db.GetFleetInfo(fleetID);
			List<int> source = new List<int>();
			int[] array = this._db.GetPlayerColonySystemIDs(this._player.ID).ToArray<int>();
			foreach (int systemId in array)
			{
				if (systemId != fleetInfo1.SupportingSystemID)
				{
					int num2 = 0;
					foreach (FleetInfo fleetInfo2 in this._db.GetFleetsBySupportingSystem(systemId, FleetType.FL_NORMAL))
					{
						if (!fleetInfo2.IsReserveFleet)
							++num2;
					}
					if (num2 == 0)
						source.Add(systemId);
				}
			}
			if (source.Count<int>() > 0)
			{
				float num2 = 0.0f;
				foreach (int systemId in source)
				{
					float num3 = this._db.ScoreSystemAsPeripheral(this._db.GetStarSystemInfo(systemId).ID, this._player.ID);
					if (num1 == 0 || (double)num3 < (double)num2)
					{
						num1 = systemId;
						num2 = num3;
					}
				}
				return num1;
			}
			float num4 = 0.0f;
			foreach (int systemId in array)
			{
				float num2 = this._db.ScoreSystemAsPeripheral(this._db.GetStarSystemInfo(systemId).ID, this._player.ID);
				if (num1 == 0 || (double)num2 < (double)num4)
				{
					num1 = systemId;
					num4 = num2;
				}
			}
			return num1;
		}

		private int GetBestUpgradeTargetSystem(AIInfo aiInfo, int fleetID, out StationInfo station)
		{
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			if (fleetInfo.SystemID == 0)
			{
				station = (StationInfo)null;
				return 0;
			}
			Vector3 fleetpos = this._db.GetStarSystemInfo(fleetInfo.SystemID).Origin;
			List<MissionInfo> stationMissions = this._db.GetMissionInfos().Where<MissionInfo>((Func<MissionInfo, bool>)(x =>
		   {
			   if (x.Type != MissionType.CONSTRUCT_STN)
				   return x.Type == MissionType.UPGRADE_STN;
			   return true;
		   })).ToList<MissionInfo>();
			foreach (IGrouping<int, StationInfo> grouping in this._db.GetStationInfosByPlayerID(this._player.ID).Where<StationInfo>((Func<StationInfo, bool>)(x =>
		   {
			   if (this._game.StationIsUpgradable(x))
				   return !stationMissions.Any<MissionInfo>((Func<MissionInfo, bool>)(y => y.TargetOrbitalObjectID == x.OrbitalObjectID));
			   return false;
		   })).OrderBy<StationInfo, float>((Func<StationInfo, float>)(z => (float)z.DesignInfo.StationLevel * (this._db.GetStarSystemInfo(z.OrbitalObjectInfo.StarSystemID).Origin - fleetpos).Length)).GroupBy<StationInfo, int>((Func<StationInfo, int>)(y => y.DesignInfo.StationLevel <= 3 ? 0 : 1)))
			{
				foreach (StationInfo stationInfo in (IEnumerable<StationInfo>)grouping)
				{
					if (grouping.Key != 0)
					{
						if (this._db.GetTurnCount() >= 100)
						{
							station = stationInfo;
							return stationInfo.OrbitalObjectInfo.StarSystemID;
						}
					}
					else
					{
						station = stationInfo;
						return stationInfo.OrbitalObjectInfo.StarSystemID;
					}
				}
			}
			station = (StationInfo)null;
			return 0;
		}

		private int GetBestConstructionTargetSystem(
		  AIInfo aiInfo,
		  int fleetID,
		  out StationType stationType)
		{
			int[] numArray1 = new int[8];
			foreach (StationInfo stationInfo in this._db.GetStationInfosByPlayerID(this._player.ID))
				++numArray1[(int)stationInfo.DesignInfo.StationType];
			string factionName = this._db.GetFactionName(this._db.GetPlayerInfo(this._player.ID).FactionID);
			float[] numArray2 = new float[8];
			numArray2[5] = !factionName.Contains("hiver") ? 0.0f : 1f;
			switch (aiInfo.Stance)
			{
				case AIStance.EXPANDING:
					numArray2[1] = 0.4f;
					numArray2[3] = 0.2f;
					numArray2[2] = 0.15f;
					numArray2[4] = 0.0f;
					break;
				case AIStance.ARMING:
					numArray2[1] = 1f;
					numArray2[3] = 0.3f;
					numArray2[2] = 0.3f;
					numArray2[4] = 0.15f;
					break;
				case AIStance.HUNKERING:
					numArray2[1] = 1f;
					numArray2[3] = 0.6f;
					numArray2[2] = 0.6f;
					numArray2[4] = 0.2f;
					break;
				case AIStance.CONQUERING:
					numArray2[1] = 1f;
					numArray2[3] = 0.3f;
					numArray2[2] = 0.3f;
					numArray2[4] = 0.15f;
					break;
				case AIStance.DESTROYING:
					numArray2[1] = 1f;
					numArray2[3] = 0.2f;
					numArray2[2] = 0.2f;
					numArray2[4] = 0.0f;
					break;
				case AIStance.DEFENDING:
					numArray2[1] = 1f;
					numArray2[3] = 0.1f;
					numArray2[2] = 0.3f;
					numArray2[4] = 0.1f;
					break;
			}
			List<int> source = new List<int>();
			foreach (ColonyInfo colonyInfo in this._db.GetColonyInfos())
			{
				if (colonyInfo.PlayerID == this._player.ID)
				{
					OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID);
					if (!source.Contains(orbitalObjectInfo.StarSystemID))
						source.Add(orbitalObjectInfo.StarSystemID);
				}
			}
			int num1 = source.Count<int>();
			float[] numArray3 = new float[8];
			for (int index = 0; index < 8; ++index)
				numArray3[index] = (float)numArray1[index] / (float)num1;
			float[] numArray4 = new float[8];
			for (int index = 0; index < 8; ++index)
			{
				numArray4[index] = (double)numArray2[index] <= 0.0 ? 0.0f : (numArray2[index] - numArray3[index]) / numArray2[index];
				numArray4[index] = Math.Min(numArray4[index], 1f);
			}
			float num2 = 0.0f;
			StationType stationType1 = StationType.INVALID_TYPE;
			for (int index = 0; index < 8; ++index)
			{
				if (stationType1 == StationType.INVALID_TYPE || (double)num2 < (double)numArray4[index])
				{
					stationType1 = (StationType)index;
					num2 = numArray4[index];
				}
			}
			if (stationType1 == StationType.INVALID_TYPE)
			{
				stationType = stationType1;
				return 0;
			}
			int systemId = this._db.GetFleetInfo(fleetID).SystemID;
			int num3 = 0;
			switch (stationType1 - 1)
			{
				case StationType.INVALID_TYPE:
					num3 = this.GetSystemForNavalStation(systemId);
					break;
				case StationType.NAVAL:
					num3 = this.GetSystemForScienceStation(systemId);
					break;
				case StationType.SCIENCE:
					num3 = this.GetSystemForCivilianStation(systemId);
					break;
				case StationType.CIVILIAN:
					num3 = this.GetSystemForDiplomaticStation(systemId);
					break;
				case StationType.GATE:
					num3 = this.GetSystemForMiningStation(systemId);
					break;
			}
			if (num3 == 0 && factionName.Contains("hiver") && stationType1 != StationType.GATE)
			{
				num3 = this.GetSystemForHiverGate(systemId);
				stationType1 = StationType.GATE;
			}
			if (num3 == 0 && stationType1 != StationType.NAVAL)
			{
				num3 = this.GetSystemForNavalStation(systemId);
				stationType1 = StationType.NAVAL;
			}
			stationType = stationType1;
			return num3;
		}

		private StarSystemInfo TryRelocateMissionCloserToTargetSystem_FindBestSystem(
		  FleetInfo fleetInfo,
		  int targetSystemID)
		{
			IEnumerable<StarSystemInfo> starSystemInfos = this._db.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
		   {
			   if (fleetInfo.SupportingSystemID == x.ID)
				   return true;
			   if (Kerberos.Sots.StarFleet.StarFleet.CanDoRelocationMissionToTarget(this._game, x.ID, fleetInfo.ID))
				   return Kerberos.Sots.StarFleet.StarFleet.CanSystemSupportFleet(this._game, x.ID, fleetInfo.ID);
			   return false;
		   }));
			StarSystemInfo starSystemInfo1 = (StarSystemInfo)null;
			float num = float.MaxValue;
			foreach (StarSystemInfo starSystemInfo2 in starSystemInfos)
			{
				int tripTime;
				float tripDistance;
				Kerberos.Sots.StarFleet.StarFleet.GetBestTravelPath(this._game, fleetInfo.ID, starSystemInfo2.ID, targetSystemID, out tripTime, out tripDistance, false, new float?(), new float?());
				if ((double)tripDistance < (double)num)
				{
					num = tripDistance;
					starSystemInfo1 = starSystemInfo2;
				}
			}
			return starSystemInfo1;
		}

		private bool TryRelocateMissionCloserToTargetSystem(int fleetID, int targetSystemID)
		{
			if (fleetID < 0 || targetSystemID < 0)
				return false;
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			StarSystemInfo systemFindBestSystem = this.TryRelocateMissionCloserToTargetSystem_FindBestSystem(fleetInfo, targetSystemID);
			if (!(systemFindBestSystem != (StarSystemInfo)null))
				return false;
			if (fleetInfo.SupportingSystemID != systemFindBestSystem.ID)
				Kerberos.Sots.StarFleet.StarFleet.SetRelocationMission(this._game, fleetID, systemFindBestSystem.ID, false, (List<int>)null);
			return true;
		}

		private bool CreateConstructionMission(AIInfo aiInfo, int fleetID, bool isUpgradeMission)
		{
			StationType stationType = StationType.CIVILIAN;
			StationInfo station = (StationInfo)null;
			int num = !isUpgradeMission ? this.GetBestConstructionTargetSystem(aiInfo, fleetID, out stationType) : this.GetBestUpgradeTargetSystem(aiInfo, fleetID, out station);
			if (num > 0)
			{
				if (stationType != StationType.MINING)
				{
					foreach (StationInfo stationInfo in this._db.GetStationForSystem(num))
					{
						if (stationInfo.DesignInfo.StationType == stationType)
							return false;
					}
				}
				if (Kerberos.Sots.StarFleet.StarFleet.CanDoConstructionMissionToTarget(this._game, num, fleetID, false))
				{
					if (isUpgradeMission)
					{
						if (station != null)
						{
							Kerberos.Sots.StarFleet.StarFleet.SetUpgradeStationMission(this._game, fleetID, num, false, station.ID, (List<int>)null, station.DesignInfo.StationType, new int?());
							return true;
						}
					}
					else
					{
						int? planetForStation = Kerberos.Sots.GameStates.StarSystem.GetSuitablePlanetForStation(this._game, this._player.ID, num, stationType);
						if (planetForStation.HasValue)
						{
							Kerberos.Sots.StarFleet.StarFleet.SetConstructionMission(this._game, fleetID, num, false, planetForStation.Value, (List<int>)null, stationType, new int?());
							return true;
						}
					}
				}
				this.TryRelocateMissionCloserToTargetSystem(fleetID, num);
			}
			return false;
		}

		private int GetBestPlanetToColonize(double maxSupportCost)
		{
			int num1 = 0;
			double num2 = double.MaxValue;
			foreach (StarSystemInfo visibleStarSystemInfo in this._db.GetVisibleStarSystemInfos(this._player.ID))
			{
				if (!this._game.isHostilesAtSystem(this._player.ID, visibleStarSystemInfo.ID) && this._db.GetLastTurnExploredByPlayer(this._player.ID, visibleStarSystemInfo.ID) > 0)
				{
					foreach (PlanetInfo systemPlanetInfo in this._db.GetStarSystemPlanetInfos(visibleStarSystemInfo.ID))
					{
						if (this._game.GameDatabase.CanColonizePlanet(this._player.ID, systemPlanetInfo.ID, this._game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, this._player.ID)))
						{
							bool flag = false;
							foreach (MissionInfo missionInfo in this._db.GetMissionInfos())
							{
								if (missionInfo.TargetOrbitalObjectID == systemPlanetInfo.ID && missionInfo.Type == MissionType.COLONIZATION && this._db.GetFleetInfo(missionInfo.FleetID).PlayerID == this._player.ID)
								{
									flag = true;
									break;
								}
							}
							if (!flag && this._db.GetColonyInfoForPlanet(systemPlanetInfo.ID) == null)
							{
								double colonySupportCost = Colony.GetColonySupportCost(this._db, this._player.ID, systemPlanetInfo.ID);
								if (colonySupportCost <= maxSupportCost && colonySupportCost < num2)
								{
									int id = visibleStarSystemInfo.ID;
									num1 = systemPlanetInfo.ID;
									num2 = colonySupportCost;
								}
							}
						}
					}
				}
			}
			return num1;
		}

		private int GetTargetForStrikeMission(int fleetID, int targetPlayerID)
		{
			int num1 = 0;
			float num2 = 0.0f;
			this._db.GetFleetInfo(fleetID);
			int supportingSystem = this._db.GetFleetSupportingSystem(fleetID);
			foreach (AIColonyIntel aiColonyIntel in this._db.GetColonyIntelOfTargetPlayer(this._player.ID, targetPlayerID).ToList<AIColonyIntel>())
			{
				if (aiColonyIntel.ColonyID.HasValue)
				{
					ColonyInfo colonyInfo = this._db.GetColonyInfo(aiColonyIntel.ColonyID.Value);
					if (colonyInfo != null)
					{
						this._db.GetPlanetInfo(colonyInfo.OrbitalObjectID);
						AIPlanetIntel planetIntel = this._db.GetPlanetIntel(targetPlayerID, colonyInfo.OrbitalObjectID);
						int turnsToTarget = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.STRIKE, StationType.INVALID_TYPE, fleetID, supportingSystem, colonyInfo.OrbitalObjectID, (List<int>)null, 1, false, new float?(), new float?()).TurnsToTarget;
						float num3 = 1000f;
						if (turnsToTarget > 0)
							num3 /= (float)turnsToTarget;
						float num4 = num3 + (float)planetIntel.Resources / 200f;
						if ((double)num4 > (double)num2)
						{
							num2 = num4;
							num1 = colonyInfo.OrbitalObjectID;
						}
					}
				}
			}
			foreach (AIStationIntel aiStationIntel in this._db.GetStationIntelsOfTargetPlayer(this._player.ID, targetPlayerID).ToList<AIStationIntel>())
			{
				StationInfo stationInfo = this._db.GetStationInfo(aiStationIntel.StationID);
				int turnsToTarget = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.STRIKE, StationType.INVALID_TYPE, fleetID, supportingSystem, stationInfo.OrbitalObjectID, (List<int>)null, 1, false, new float?(), new float?()).TurnsToTarget;
				float num3 = 1000f;
				if (turnsToTarget > 0)
					num3 /= (float)turnsToTarget;
				float num4 = num3 + 100f + (float)aiStationIntel.Level * 10f;
				if ((double)num4 > (double)num2)
				{
					num2 = num4;
					num1 = stationInfo.OrbitalObjectID;
				}
			}
			return num1;
		}

		internal IEnumerable<TargetOrbitalObjectScore> GetTargetsForInvasion(
		  int fleetID,
		  int targetPlayerID)
		{
			FleetInfo fleet = this._db.GetFleetInfo(fleetID);
			if (fleet != null)
			{
				int fromSystemID = this._db.GetFleetSupportingSystem(fleetID);
				float tolerance = 200f;
				float idealSuitability = this._db.GetPlayerSuitability(this._player.ID);
				foreach (AIColonyIntel aiColonyIntel in this._db.GetColonyIntelOfTargetPlayer(this._player.ID, targetPlayerID))
				{
					if (aiColonyIntel.ColonyID.HasValue)
					{
						ColonyInfo colonyInfo = this._db.GetColonyInfo(aiColonyIntel.ColonyID.Value);
						if (colonyInfo != null)
						{
							OrbitalObjectInfo orbitInfo = this._db.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID);
							this._db.GetPlanetInfo(colonyInfo.OrbitalObjectID);
							AIPlanetIntel planetIntel = this._db.GetPlanetIntel(targetPlayerID, colonyInfo.OrbitalObjectID);
							MissionEstimate estimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.STRIKE, StationType.INVALID_TYPE, fleetID, fromSystemID, colonyInfo.OrbitalObjectID, (List<int>)null, 1, false, new float?(), new float?());
							int travelTurns = estimate.TurnsToTarget;
							float score = 1000f;
							if (travelTurns > 0)
								score /= (float)travelTurns;
							foreach (PlanetInfo systemPlanetInfo in this._db.GetStarSystemPlanetInfos(orbitInfo.StarSystemID))
							{
								ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(systemPlanetInfo.ID);
								if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == this._player.ID)
									score += 200f;
							}
							if (this._db.GetNavalStationForSystemAndPlayer(orbitInfo.StarSystemID, this._player.ID) != null)
								score += 250f;
							score += (float)planetIntel.Resources / 200f;
							int[] recentCombatTurns = this._db.GetRecentCombatTurns(orbitInfo.StarSystemID, this._db.GetTurnCount() - 10);
							score += (float)recentCombatTurns.Length * 100f;
							if ((double)Math.Abs(idealSuitability - planetIntel.Suitability) > (double)tolerance)
								score /= 10f;
							yield return new TargetOrbitalObjectScore()
							{
								OrbitalObjectID = colonyInfo.OrbitalObjectID,
								Score = score
							};
						}
					}
				}
			}
		}

		private int? GetTargetForInvasion(int fleetID, int targetPlayerID)
		{
			int? nullable = new int?();
			float num = 0.0f;
			foreach (TargetOrbitalObjectScore orbitalObjectScore in this.GetTargetsForInvasion(fleetID, targetPlayerID))
			{
				if ((double)orbitalObjectScore.Score > (double)num)
				{
					num = orbitalObjectScore.Score;
					nullable = new int?(orbitalObjectScore.OrbitalObjectID);
				}
			}
			return nullable;
		}

		private int GetSystemForHiverGate(int fromSystemID)
		{
			int num1 = 0;
			float num2 = 0.0f;
			Vector3 origin = this._db.GetStarSystemInfo(fromSystemID).Origin;
			foreach (ColonyInfo colonyInfo in this._db.GetColonyInfos())
			{
				if (colonyInfo.PlayerID == this._player.ID)
				{
					OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID);
					StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
					bool flag = false;
					foreach (MissionInfo missionInfo in this._db.GetMissionInfos())
					{
						if (missionInfo.TargetSystemID == starSystemInfo.ID && missionInfo.Type == MissionType.GATE && this._db.GetFleetInfo(missionInfo.FleetID).PlayerID == this._player.ID)
						{
							flag = true;
							break;
						}
					}
					if (!flag && this._db.GetHiverGateForSystem(orbitalObjectInfo.StarSystemID, this._player.ID) == null)
					{
						float num3 = 1000f - (starSystemInfo.Origin - origin).Length;
						if (num1 == 0 || (double)num3 > (double)num2)
						{
							num1 = starSystemInfo.ID;
							num2 = num3;
						}
					}
				}
			}
			return num1;
		}

		private int GetSystemForNavalStation(int fromSystemID)
		{
			Vector3 origin = this._db.GetStarSystemInfo(fromSystemID).Origin;
			int num1 = 0;
			float num2 = float.MinValue;
			foreach (StarSystemInfo visibleStarSystemInfo in this._db.GetVisibleStarSystemInfos(this._player.ID))
			{
				if (this._db.GetLastTurnExploredByPlayer(this._player.ID, visibleStarSystemInfo.ID) > 0 && this._db.GetNavalStationForSystemAndPlayer(visibleStarSystemInfo.ID, this._player.ID) == null && Kerberos.Sots.GameStates.StarSystem.GetSystemCanSupportStations(this._game, visibleStarSystemInfo.ID, this._player.ID).Contains(StationType.NAVAL) && Kerberos.Sots.GameStates.StarSystem.GetSuitablePlanetForStation(this._game, this._player.ID, visibleStarSystemInfo.ID, StationType.NAVAL).HasValue)
				{
					bool flag = false;
					foreach (MissionInfo missionInfo in this._db.GetMissionInfos())
					{
						if (missionInfo.TargetSystemID == visibleStarSystemInfo.ID && GameSession.GetConstructionMissionStationType(this._db, missionInfo) == StationType.NAVAL && this._db.GetFleetInfo(missionInfo.FleetID).PlayerID == this._player.ID)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						float num3 = 0.0f;
						foreach (PlanetInfo systemPlanetInfo in this._db.GetStarSystemPlanetInfos(visibleStarSystemInfo.ID))
						{
							ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(systemPlanetInfo.ID);
							if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == this._player.ID)
								num3 += 100f;
						}
						if ((double)num3 >= 0.0)
						{
							float num4 = 5f - Math.Abs((visibleStarSystemInfo.Origin - origin).Length - 2f * this._db.FindCurrentDriveSpeedForPlayer(this._player.ID));
							float num5 = num3 * num4;
							if (num1 == 0 || (double)num5 > (double)num2)
							{
								num1 = visibleStarSystemInfo.ID;
								num2 = num5;
							}
						}
					}
				}
			}
			return num1;
		}

		private int GetSystemForCivilianStation(int fromSystemID)
		{
			int num1 = 0;
			Vector3 origin = this._db.GetStarSystemInfo(fromSystemID).Origin;
			double num2 = 0.0;
			foreach (StarSystemInfo visibleStarSystemInfo in this._db.GetVisibleStarSystemInfos(this._player.ID))
			{
				float num3 = (origin - visibleStarSystemInfo.Origin).Length / this._db.FindCurrentDriveSpeedForPlayer(this._player.ID);
				if (this._db.GetFactionName(this._db.GetPlayerInfo(this._player.ID).FactionID).Contains("hiver") && this._db.GetHiverGateForSystem(visibleStarSystemInfo.ID, this._player.ID) != null)
					num3 = 1f;
				if (this._db.GetLastTurnExploredByPlayer(this._player.ID, visibleStarSystemInfo.ID) > 0 && this._db.GetCivilianStationForSystemAndPlayer(visibleStarSystemInfo.ID, this._player.ID) == null)
				{
					bool flag = false;
					foreach (MissionInfo missionInfo in this._db.GetMissionInfos())
					{
						if (missionInfo.TargetSystemID == visibleStarSystemInfo.ID && GameSession.GetConstructionMissionStationType(this._db, missionInfo) == StationType.CIVILIAN && this._db.GetFleetInfo(missionInfo.FleetID).PlayerID == this._player.ID)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						double num4 = 0.0;
						foreach (PlanetInfo systemPlanetInfo in this._db.GetStarSystemPlanetInfos(visibleStarSystemInfo.ID))
						{
							ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(systemPlanetInfo.ID);
							if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == this._player.ID)
								num4 += Colony.GetIndustrialOutput(this._game, colonyInfoForPlanet, systemPlanetInfo);
						}
						if ((double)num3 > 2.0)
							num4 /= (double)num3 - 1.0;
						if (num4 > 0.0 && (num1 == 0 || num4 > num2))
						{
							num1 = visibleStarSystemInfo.ID;
							num2 = num4;
						}
					}
				}
			}
			return num1;
		}

		private int GetSystemForDiplomaticStation(int fromSystemID)
		{
			int num1 = 0;
			Vector3 origin = this._db.GetStarSystemInfo(fromSystemID).Origin;
			double num2 = 0.0;
			foreach (StarSystemInfo visibleStarSystemInfo in this._db.GetVisibleStarSystemInfos(this._player.ID))
			{
				float length = (origin - visibleStarSystemInfo.Origin).Length;
				float driveSpeedForPlayer = this._db.FindCurrentDriveSpeedForPlayer(this._player.ID);
				float num3 = length / driveSpeedForPlayer;
				if (this._db.GetFactionName(this._db.GetPlayerInfo(this._player.ID).FactionID).Contains("hiver") && this._db.GetHiverGateForSystem(visibleStarSystemInfo.ID, this._player.ID) != null)
					num3 = 1f;
				if (this._db.GetLastTurnExploredByPlayer(this._player.ID, visibleStarSystemInfo.ID) > 0 && this._db.GetDiplomaticStationForSystemAndPlayer(visibleStarSystemInfo.ID, this._player.ID) == null)
				{
					bool flag1 = false;
					foreach (MissionInfo missionInfo in this._db.GetMissionInfos())
					{
						if (missionInfo.TargetSystemID == visibleStarSystemInfo.ID && GameSession.GetConstructionMissionStationType(this._db, missionInfo) == StationType.DIPLOMATIC && this._db.GetFleetInfo(missionInfo.FleetID).PlayerID == this._player.ID)
						{
							flag1 = true;
							break;
						}
					}
					if (!flag1)
					{
						float num4 = 0.0f;
						bool flag2 = false;
						foreach (PlanetInfo systemPlanetInfo in this._db.GetStarSystemPlanetInfos(visibleStarSystemInfo.ID))
						{
							ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(systemPlanetInfo.ID);
							if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == this._player.ID)
							{
								flag2 = true;
								break;
							}
						}
						if (flag2)
						{
							float range = Math.Max(driveSpeedForPlayer * 4f, 20f);
							foreach (StarSystemInfo starSystemInfo in this._db.GetSystemsInRange(visibleStarSystemInfo.Origin, range))
							{
								foreach (PlanetInfo systemPlanetInfo in this._db.GetStarSystemPlanetInfos(starSystemInfo.ID))
								{
									AIColonyIntel colonyIntelForPlanet = this._db.GetColonyIntelForPlanet(this._player.ID, systemPlanetInfo.ID);
									if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID != this._player.ID)
										num4 += 100f;
								}
								if (this._db.GetDiplomaticStationForSystemAndPlayer(starSystemInfo.ID, this._player.ID) != null)
									num4 -= 150f;
							}
						}
						if ((double)num3 > 2.0)
							num4 /= num3 - 1f;
						if ((double)num4 > 0.0 && (num1 == 0 || (double)num4 > num2))
						{
							num1 = visibleStarSystemInfo.ID;
							num2 = (double)num4;
						}
					}
				}
			}
			return num1;
		}

		private int GetSystemForScienceOrMiningStation(int fromSystemID)
		{
			int num1 = 0;
			Vector3 origin = this._db.GetStarSystemInfo(fromSystemID).Origin;
			double num2 = 0.0;
			foreach (StarSystemInfo visibleStarSystemInfo in this._db.GetVisibleStarSystemInfos(this._player.ID))
			{
				float num3 = (origin - visibleStarSystemInfo.Origin).Length / this._db.FindCurrentDriveSpeedForPlayer(this._player.ID);
				if (this._db.GetFactionName(this._db.GetPlayerInfo(this._player.ID).FactionID).Contains("hiver") && this._db.GetHiverGateForSystem(visibleStarSystemInfo.ID, this._player.ID) != null)
					num3 = 1f;
				if (this._db.GetLastTurnExploredByPlayer(this._player.ID, visibleStarSystemInfo.ID) > 0 && this._db.GetScienceStationForSystemAndPlayer(visibleStarSystemInfo.ID, this._player.ID) == null)
				{
					bool flag = false;
					foreach (MissionInfo missionInfo in this._db.GetMissionInfos())
					{
						if (missionInfo.TargetSystemID == visibleStarSystemInfo.ID && GameSession.GetConstructionMissionStationType(this._db, missionInfo) == StationType.SCIENCE && this._db.GetFleetInfo(missionInfo.FleetID).PlayerID == this._player.ID)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						double num4 = 0.0;
						foreach (PlanetInfo systemPlanetInfo in this._db.GetStarSystemPlanetInfos(visibleStarSystemInfo.ID))
						{
							ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(systemPlanetInfo.ID);
							if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == this._player.ID)
								num4 += Colony.GetIndustrialOutput(this._game, colonyInfoForPlanet, systemPlanetInfo);
						}
						if ((double)num3 > 2.0)
							num4 /= (double)num3 - 1.0;
						if (num4 > 0.0 && (num1 == 0 || num4 > num2))
						{
							num1 = visibleStarSystemInfo.ID;
							num2 = num4;
						}
					}
				}
			}
			return num1;
		}

		private int GetSystemForScienceStation(int fromSystemID)
		{
			return this.GetSystemForScienceOrMiningStation(fromSystemID);
		}

		private int GetSystemForMiningStation(int fromSystemID)
		{
			return !Player.CanBuildMiningStations(this._db, this._player.ID) ? 0 : 0;
		}

		private void EnsureOpenDefenseInvoice(
		  int systemId,
		  ref bool isOpened,
		  ref int invoiceId,
		  ref int instanceId)
		{
			if (isOpened)
				return;
			this.OpenInvoice(systemId, "Defenses", out invoiceId, out instanceId);
			isOpened = true;
		}

		private void ManageDefenses(AIInfo aiInfo)
		{
			double availableDefenseBudget = this.GetAvailableDefenseBudget();
			foreach (StarSystemInfo system in this._db.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => this._db.GetColonyInfosForSystem(x.ID).Any<ColonyInfo>((Func<ColonyInfo, bool>)(y => y.PlayerID == this._player.ID)))))
			{
				int num1 = this._db.GetSystemDefensePoints(system.ID, this._player.ID) - this._db.GetAllocatedSystemDefensePoints(system, this._player.ID);
				if (num1 > 0)
				{
					FleetInfo defenseFleetInfo = this._db.GetDefenseFleetInfo(system.ID, this._player.ID);
					if (defenseFleetInfo != null)
					{
						FleetInfo reserveFleetInfo = this._db.InsertOrGetReserveFleetInfo(system.ID, this._player.ID);
						foreach (ShipInfo shipInfo in this._db.GetShipInfoByFleetID(reserveFleetInfo.ID, false).Where<ShipInfo>((Func<ShipInfo, bool>)(x => x.IsPoliceShip())).ToList<ShipInfo>())
							this._db.TransferShip(shipInfo.ID, defenseFleetInfo.ID);
						int num2 = num1;
						foreach (ShipInfo shipInfo in this._db.GetShipInfoByFleetID(defenseFleetInfo.ID, false).Where<ShipInfo>((Func<ShipInfo, bool>)(x => !x.IsPlaced())).ToList<ShipInfo>())
						{
							int defenseAssetCpCost = this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(shipInfo.DesignInfo);
							if (defenseAssetCpCost <= num2 && Kerberos.Sots.StarFleet.StarFleet.AutoPlaceDefenseAsset(this._game.App, shipInfo.ID, system.ID))
								num2 -= defenseAssetCpCost;
						}
						int num3 = this._db.GetSystemDefensePoints(system.ID, this._player.ID) - this._db.GetAllocatedSystemDefensePoints(system, this._player.ID);
						IEnumerable<DesignInfo> source = this._db.GetBuildOrdersForSystem(system.ID).Select<BuildOrderInfo, DesignInfo>((Func<BuildOrderInfo, DesignInfo>)(x => this._db.GetDesignInfo(x.DesignID))).Where<DesignInfo>((Func<DesignInfo, bool>)(y => y.PlayerID == this._player.ID));
						int num4 = this._db.GetShipInfoByFleetID(defenseFleetInfo.ID, false).Where<ShipInfo>((Func<ShipInfo, bool>)(x =>
					   {
						   if (x.IsPlaced())
							   return x.IsPlatform();
						   return false;
					   })).Sum<ShipInfo>((Func<ShipInfo, int>)(y => this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(y.DesignInfo)));
						int num5 = this._db.GetShipInfoByFleetID(defenseFleetInfo.ID, false).Where<ShipInfo>((Func<ShipInfo, bool>)(x =>
					   {
						   if (!x.IsPlaced())
							   return x.IsPlatform();
						   return false;
					   })).Sum<ShipInfo>((Func<ShipInfo, int>)(y => this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(y.DesignInfo)));
						int num6 = source.Where<DesignInfo>((Func<DesignInfo, bool>)(x => x.IsPlatform())).Sum<DesignInfo>((Func<DesignInfo, int>)(y => this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(y)));
						int num7 = this._db.GetMostRecentCombatData(system.ID) != null ? int.MaxValue : 1;
						int num8 = this._db.GetShipInfoByFleetID(defenseFleetInfo.ID, false).Where<ShipInfo>((Func<ShipInfo, bool>)(x =>
					   {
						   if (x.IsPlaced())
							   return x.IsPoliceShip();
						   return false;
					   })).Sum<ShipInfo>((Func<ShipInfo, int>)(y => this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(y.DesignInfo)));
						int num9 = this._db.GetShipInfoByFleetID(defenseFleetInfo.ID, false).Concat<ShipInfo>(this._db.GetShipInfoByFleetID(reserveFleetInfo.ID, false)).Where<ShipInfo>((Func<ShipInfo, bool>)(x =>
					   {
						   if (!x.IsPlaced())
							   return x.IsPoliceShip();
						   return false;
					   })).Sum<ShipInfo>((Func<ShipInfo, int>)(y => this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(y.DesignInfo)));
						int num10 = source.Where<DesignInfo>((Func<DesignInfo, bool>)(x => x.IsPoliceShip())).Sum<DesignInfo>((Func<DesignInfo, int>)(y => this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(y)));
						int num11 = 1;
						if (num5 <= 0 && num9 <= 0)
						{
							int val2_1 = num3 - num5 - num6 - num9 - num10;
							float productionRate = 0.0f;
							double constructionRate = 0.0;
							double totalRevenue = 0.0;
							BuildScreenState.ObtainConstructionCosts(out productionRate, out constructionRate, out totalRevenue, this._game.App, system.ID, this._player.ID);
							if (constructionRate != 0.0 && (double)productionRate > 0.0 && (availableDefenseBudget > 0.0 && val2_1 > 0))
							{
								bool isOpened = false;
								int invoiceId = 0;
								int instanceId = 0;
								int val2_2 = Math.Max(3 - (num6 + num10), 0);
								int num12 = Math.Min(num11 - num9 - num10 - num8, val2_2);
								if (num12 > 0 && num12 < val2_1)
								{
									List<DesignInfo> list = this._db.GetVisibleDesignInfosForPlayer(this._player.ID).Where<DesignInfo>((Func<DesignInfo, bool>)(x => x.IsPoliceShip())).ToList<DesignInfo>();
									if (list.Count > 0)
									{
										DesignInfo designInfo = this._game.Random.Choose<DesignInfo>((IList<DesignInfo>)list);
										double designCost = (double)BuildScreenState.GetDesignCost(this._game.App, designInfo, 0);
										int defenseAssetCpCost = this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(designInfo);
										while (defenseAssetCpCost <= val2_1 && availableDefenseBudget >= designCost)
										{
											this.EnsureOpenDefenseInvoice(system.ID, ref isOpened, ref invoiceId, ref instanceId);
											this.AddShipToInvoice(system.ID, designInfo, invoiceId, instanceId, new int?());
											num12 -= defenseAssetCpCost;
											val2_1 -= defenseAssetCpCost;
											availableDefenseBudget -= designCost;
											--val2_2;
											if (num12 <= 0 || val2_2 <= 0)
												break;
										}
									}
								}
								int num13 = Math.Min(Math.Min(num7 - num5 - num6 - num4, val2_1), val2_2);
								if (num13 > 0)
								{
									List<DesignInfo> list = this._db.GetVisibleDesignInfosForPlayer(this._player.ID).Where<DesignInfo>((Func<DesignInfo, bool>)(x =>
								   {
									   RealShipClasses? realShipClass = x.GetRealShipClass();
									   if (realShipClass.GetValueOrDefault() == RealShipClasses.Platform)
										   return realShipClass.HasValue;
									   return false;
								   })).ToList<DesignInfo>();
									if (list.Count > 0)
									{
										do
										{
											DesignInfo designInfo = this._game.Random.Choose<DesignInfo>((IList<DesignInfo>)list);
											int defenseAssetCpCost = this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(designInfo);
											double designCost = (double)BuildScreenState.GetDesignCost(this._game.App, designInfo, 0);
											if (defenseAssetCpCost <= num13 && availableDefenseBudget >= designCost)
											{
												this.EnsureOpenDefenseInvoice(system.ID, ref isOpened, ref invoiceId, ref instanceId);
												this.AddShipToInvoice(system.ID, designInfo, invoiceId, instanceId, new int?());
												num13 -= defenseAssetCpCost;
												val2_1 -= defenseAssetCpCost;
												availableDefenseBudget -= designCost;
												--val2_2;
											}
											else
												break;
										}
										while (num13 > 0 && val2_2 > 0);
									}
								}
							}
						}
					}
				}
			}
		}

		private void ManageReserves(AIInfo aiInfo)
		{
			foreach (FleetInfo fleetInfo in this._db.GetFleetInfosByPlayerID(aiInfo.PlayerID, FleetType.FL_RESERVE).ToList<FleetInfo>())
			{
				List<ShipInfo> list = this._db.GetShipInfoByFleetID(fleetInfo.ID, false).ToList<ShipInfo>();
				StrategicAI.BattleRiderMountSet battleRiderMountSet = new StrategicAI.BattleRiderMountSet((IEnumerable<ShipInfo>)list);
				foreach (ShipInfo shipInfo in list)
				{
					if (!battleRiderMountSet.Contains(shipInfo) && (!shipInfo.AIFleetID.HasValue || shipInfo.AIFleetID.Value == 0))
						this._db.RemoveShip(shipInfo.ID);
				}
			}
		}

		private void ManageFleets(AIInfo aiInfo)
		{
			StrategicAI.TraceVerbose("Maintaining fleets...");
			List<int> mySystemIDs = this._db.GetPlayerColonySystemIDs(this._player.ID).ToList<int>();
			List<StarSystemInfo> list1 = this._db.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => mySystemIDs.Contains(x.ID))).ToList<StarSystemInfo>();
			List<AIFleetInfo> myAIFleets = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
			List<FleetInfo> list2 = this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
			StrategicAI.TraceVerbose("Forgetting AIFleetInfos without admirals...");
			foreach (AIFleetInfo aiFleetInfo in myAIFleets.ToList<AIFleetInfo>())
			{
				if (!aiFleetInfo.AdmiralID.HasValue)
				{
					StrategicAI.TraceVerbose("  " + aiFleetInfo.ToString());
					this._db.RemoveAIFleetInfo(aiFleetInfo.ID);
					myAIFleets.Remove(aiFleetInfo);
				}
			}
			StrategicAI.TraceVerbose("Forgetting FleetInfos without admirals...");
			foreach (FleetInfo fleetInfo in list2.ToList<FleetInfo>())
			{
				FleetInfo fleet = fleetInfo;
				if (fleet.AdmiralID == 0)
				{
					StrategicAI.TraceVerbose("  " + fleet.ToString());
					this._db.RemoveAIFleetInfo(fleet.ID);
					myAIFleets.RemoveAll((Predicate<AIFleetInfo>)(x =>
				   {
					   int? fleetId = x.FleetID;
					   int id = fleet.ID;
					   if (fleetId.GetValueOrDefault() == id)
						   return fleetId.HasValue;
					   return false;
				   }));
					list2.Remove(fleet);
				}
			}
			StrategicAI.TraceVerbose("Assigning fleet templates...");
			foreach (FleetInfo fleetInfo1 in list2)
			{
				FleetInfo fleet = fleetInfo1;
				FleetTemplate fleetTemplate = (FleetTemplate)null;
				AIFleetInfo aiFleetInfo = myAIFleets.FirstOrDefault<AIFleetInfo>((Func<AIFleetInfo, bool>)(x =>
			   {
				   int? fleetId = x.FleetID;
				   int id = fleet.ID;
				   if (fleetId.GetValueOrDefault() == id)
					   return fleetId.HasValue;
				   return false;
			   }));
				if (aiFleetInfo == null)
				{
					string templateName = DesignLab.DeduceFleetTemplate(this._db, this._game, fleet.ID);
					if (!string.IsNullOrEmpty(templateName))
					{
						fleetTemplate = this._db.AssetDatabase.FleetTemplates.First<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == templateName));
						AIFleetInfo fleetInfo2 = new AIFleetInfo()
						{
							AdmiralID = new int?(fleet.AdmiralID),
							FleetType = MissionTypeExtensions.SerializeList(fleetTemplate.MissionTypes),
							SystemID = fleet.SupportingSystemID,
							FleetTemplate = templateName
						};
						fleetInfo2.ID = this._db.InsertAIFleetInfo(this._player.ID, fleetInfo2);
						aiFleetInfo = fleetInfo2;
						StrategicAI.TraceVerbose(string.Format("  Assigned template {0} to clean fleet {1}", (object)fleetTemplate, (object)fleet));
					}
				}
				if (fleetTemplate == null)
					fleetTemplate = this._db.AssetDatabase.FleetTemplates.FirstOrDefault<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == aiFleetInfo.FleetTemplate));
				if (fleetTemplate != null && fleet.SystemID == fleet.SupportingSystemID)
				{
					StrategicAI.TraceVerbose(string.Format("  Performing maintenance on fleet {0} (template {1}) at supporting system...", (object)fleet, (object)fleetTemplate));
					List<DesignInfo> source = new List<DesignInfo>();
					IEnumerable<ShipInfo> shipInfoByFleetId1 = this._db.GetShipInfoByFleetID(fleet.ID, true);
					if (aiFleetInfo.InvoiceID.HasValue)
					{
						StrategicAI.TraceVerbose(string.Format("    Waiting for completion of invoice {0}.", (object)aiFleetInfo.InvoiceID));
					}
					else
					{
						if (aiFleetInfo.FleetID.HasValue)
						{
							List<ShipInfo> list3 = this._db.GetShipInfoByFleetID(this._db.InsertOrGetReserveFleetInfo(fleet.SupportingSystemID, this._player.ID).ID, false).Where<ShipInfo>((Func<ShipInfo, bool>)(x =>
						   {
							   int? aiFleetId = x.AIFleetID;
							   int id = aiFleetInfo.ID;
							   if (aiFleetId.GetValueOrDefault() == id)
								   return aiFleetId.HasValue;
							   return false;
						   })).ToList<ShipInfo>();
							bool flag = true;
							foreach (ShipInfo shipInfo in list3)
							{
								if (flag)
								{
									flag = false;
									StrategicAI.TraceVerbose("    Transferring ships to fleet:");
								}
								this._db.TransferShip(shipInfo.ID, aiFleetInfo.FleetID.Value);
								StrategicAI.TraceVerbose(string.Format("      + Ship {0}", (object)shipInfo));
							}
						}
						Dictionary<ShipInfo, Dictionary<LogicalBank, StrategicAI.BankRiderInfo>> dictionary1 = new Dictionary<ShipInfo, Dictionary<LogicalBank, StrategicAI.BankRiderInfo>>();
						if (aiFleetInfo.FleetID.HasValue)
						{
							IEnumerable<ShipInfo> shipInfoByFleetId2 = this._db.GetShipInfoByFleetID(aiFleetInfo.FleetID.Value, false);
							StrategicAI.BattleRiderMountSet battleRiderMountSet = new StrategicAI.BattleRiderMountSet(shipInfoByFleetId2);
							foreach (ShipInfo shipInfo1 in shipInfoByFleetId2)
							{
								if (!battleRiderMountSet.Contains(shipInfo1))
								{
									List<ShipInfo> list3 = this._db.GetBattleRidersByParentID(shipInfo1.ID).ToList<ShipInfo>();
									Dictionary<LogicalBank, StrategicAI.BankRiderInfo> dictionary2 = new Dictionary<LogicalBank, StrategicAI.BankRiderInfo>();
									dictionary1.Add(shipInfo1, dictionary2);
									foreach (KeyValuePair<LogicalMount, int> battleRiderMount in StrategicAI.BattleRiderMountSet.EnumerateBattleRiderMounts(shipInfo1.DesignInfo))
									{
										KeyValuePair<LogicalMount, int> mountIndex = battleRiderMount;
										ShipInfo shipInfo2 = list3.FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.RiderIndex == mountIndex.Value));
										StrategicAI.BankRiderInfo bankRiderInfo;
										if (!dictionary2.TryGetValue(mountIndex.Key.Bank, out bankRiderInfo))
										{
											bankRiderInfo = new StrategicAI.BankRiderInfo()
											{
												Bank = mountIndex.Key.Bank
											};
											dictionary2.Add(bankRiderInfo.Bank, bankRiderInfo);
											if (shipInfo2 != null)
												bankRiderInfo.AllocatedRole = new ShipRole?(shipInfo2.DesignInfo.Role);
										}
										if (shipInfo2 == null)
											bankRiderInfo.FreeRiderIndices.Add(mountIndex.Value);
										else
											bankRiderInfo.FilledRiderIndices.Add(mountIndex.Value);
									}
									foreach (StrategicAI.BankRiderInfo bankRiderInfo in dictionary2.Values.Where<StrategicAI.BankRiderInfo>((Func<StrategicAI.BankRiderInfo, bool>)(x => x.FreeRiderIndices.Count > 0)))
									{
										if (!bankRiderInfo.AllocatedRole.HasValue)
										{
											ShipInfo shipInfo2 = battleRiderMountSet.EnumerateByTurretClass(bankRiderInfo.Bank.TurretClass).FirstOrDefault<ShipInfo>();
											if (shipInfo2 != null)
												bankRiderInfo.AllocatedRole = new ShipRole?(shipInfo2.DesignInfo.Role);
										}
										if (bankRiderInfo.AllocatedRole.HasValue)
										{
											while (bankRiderInfo.FreeRiderIndices.Count > 0)
											{
												ShipInfo shipByRole = battleRiderMountSet.FindShipByRole(bankRiderInfo.AllocatedRole.Value);
												if (shipByRole != null)
													battleRiderMountSet.AttachBattleRiderToShip(this._db, bankRiderInfo, shipByRole, shipInfo1, bankRiderInfo.FreeRiderIndices[0]);
												else
													break;
											}
										}
									}
								}
							}
						}
						List<ShipInclude> shipIncludeList = new List<ShipInclude>((IEnumerable<ShipInclude>)fleetTemplate.ShipIncludes);
						foreach (Dictionary<LogicalBank, StrategicAI.BankRiderInfo> dictionary2 in dictionary1.Values)
						{
							foreach (StrategicAI.BankRiderInfo bankRiderInfo in dictionary2.Values)
							{
								ShipRole shipRole = !bankRiderInfo.AllocatedRole.HasValue ? this._random.Choose<ShipRole>(StrategicAI.BattleRiderMountSet.EnumerateShipRolesByTurretClass(bankRiderInfo.Bank.TurretClass)) : bankRiderInfo.AllocatedRole.Value;
								shipIncludeList.Add(new ShipInclude()
								{
									Amount = bankRiderInfo.FilledRiderIndices.Count + bankRiderInfo.FreeRiderIndices.Count,
									Faction = this._player.Faction.Name,
									InclusionType = ShipInclusionType.REQUIRED,
									ShipRole = shipRole,
									WeaponRole = new WeaponRole?()
								});
							}
						}
						foreach (ShipInclude shipInclude in shipIncludeList)
						{
							ShipInclude include = shipInclude;
							if (include.InclusionType == ShipInclusionType.REQUIRED && (include.Faction == null || !(include.Faction != this._player.Faction.Name)) && !(include.FactionExclusion == this._player.Faction.Name))
							{
								int num1 = shipInfoByFleetId1.Count<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.Role == include.ShipRole));
								if (num1 < include.Amount)
								{
									int num2 = include.Amount - num1;
									for (int index = 0; index < num2; ++index)
									{
										DesignInfo designByRole = DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(aiInfo.Stance), include.ShipRole, include.WeaponRole);
										if (designByRole != null)
											source.Add(designByRole);
									}
								}
							}
						}
						if (source.Count<DesignInfo>() > 0)
						{
							StrategicAI.TraceVerbose("    Requesting additional ships:");
							AdmiralInfo admiralInfo = this._db.GetAdmiralInfo(fleet.AdmiralID);
							int num = this._db.InsertInvoiceInstance(this._player.ID, fleet.SupportingSystemID, admiralInfo.Name);
							foreach (DesignInfo designInfo in source)
							{
								string shipName = this._game.NamesPool.GetShipName(this._game, this._player.ID, designInfo.Class, (IEnumerable<string>)null);
								this._db.InsertBuildOrder(fleet.SupportingSystemID, designInfo.ID, 0, 0, shipName, designInfo.GetPlayerProductionCost(this._db, this._player.ID, !designInfo.isPrototyped, new float?()), new int?(num), new int?(aiFleetInfo.ID), 0);
								StrategicAI.TraceVerbose(string.Format("      + Design {0}", (object)designInfo));
							}
							aiFleetInfo.InvoiceID = new int?(num);
							this._db.UpdateAIFleetInfo(aiFleetInfo);
						}
					}
				}
			}
			StrategicAI.TraceVerbose("Checking progress of fleet creation...");
			foreach (AIFleetInfo aiFleetInfo in myAIFleets)
			{
				AIFleetInfo aiFleet = aiFleetInfo;
				if (!aiFleet.InvoiceID.HasValue && !aiFleet.FleetID.HasValue)
				{
					FleetTemplate fleetTemplate = this._db.AssetDatabase.FleetTemplates.First<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == aiFleet.FleetTemplate));
					aiFleet.FleetID = new int?(this._db.InsertFleet(this._player.ID, aiFleet.AdmiralID.Value, aiFleet.SystemID, aiFleet.SystemID, fleetTemplate.FleetName, FleetType.FL_NORMAL));
					this._db.UpdateAIFleetInfo(aiFleet);
					StrategicAI.TraceVerbose(string.Format("  Creating new fleet for {0}...", (object)aiFleet));
					foreach (ShipInfo shipInfo in this._db.GetShipInfoByFleetID(this._db.InsertOrGetReserveFleetInfo(aiFleet.SystemID, this._player.ID).ID, false).Where<ShipInfo>((Func<ShipInfo, bool>)(x =>
				   {
					   int? aiFleetId = x.AIFleetID;
					   int id = aiFleet.ID;
					   if (aiFleetId.GetValueOrDefault() == id)
						   return aiFleetId.HasValue;
					   return false;
				   })).ToList<ShipInfo>())
					{
						this._db.TransferShip(shipInfo.ID, aiFleet.FleetID.Value);
						StrategicAI.TraceVerbose(string.Format("    + Ship {0}", (object)shipInfo));
					}
				}
			}
			StrategicAI.TraceVerbose("Issuing build orders...");
			double num3 = -1000000.0;
			if (this._db.GetPlayerInfo(this._player.ID).Savings <= num3)
			{
				StrategicAI.TraceVerbose(string.Format("  Savings at ship building debt cutoff ({0}). New orders will not be issued this turn.", (object)num3));
			}
			else
			{
				List<StarSystemInfo> list3 = list1.Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => myAIFleets.FirstOrDefault<AIFleetInfo>((Func<AIFleetInfo, bool>)(y =>
			  {
				  if (y.SystemID == x.ID)
					  return y.InvoiceID.HasValue;
				  return false;
			  })) == null)).ToList<StarSystemInfo>();
				if (list3.Count == 0)
				{
					StrategicAI.TraceVerbose("  All pending systems are already busy building ships.");
				}
				else
				{
					IEnumerable<AdmiralInfo> admiralInfosForPlayer = this._db.GetAdmiralInfosForPlayer(this._player.ID);
					List<FleetInfo> fleetInfos = this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
					AdmiralInfo admiral = admiralInfosForPlayer.FirstOrDefault<AdmiralInfo>((Func<AdmiralInfo, bool>)(x =>
				   {
					   if (!fleetInfos.Any<FleetInfo>((Func<FleetInfo, bool>)(y => y.AdmiralID == x.ID)))
						   return !myAIFleets.Any<AIFleetInfo>((Func<AIFleetInfo, bool>)(y =>
				 {
					 int? admiralId = y.AdmiralID;
					 int id = x.ID;
					 if (admiralId.GetValueOrDefault() == id)
						 return admiralId.HasValue;
					 return false;
				 }));
					   return false;
				   }));
					StarSystemInfo system = list3.LastOrDefault<StarSystemInfo>();
					if (admiral != null && system != (StarSystemInfo)null)
						this.BuildFleet(aiInfo, admiral, system, (FleetTemplate)null, (List<BuildScreenState.InvoiceItem>)null, false);
					else
						StrategicAI.TraceVerbose("  Missing an available admiral or star system to build a fleet at.");
				}
			}
		}

		private void OpenInvoice(int systemId, string name, out int invoiceId, out int instanceId)
		{
			invoiceId = this._db.InsertInvoice(name, this._player.ID, false);
			instanceId = this._db.InsertInvoiceInstance(this._player.ID, systemId, name);
			StrategicAI.TraceVerbose(string.Format("    Opening a new build invoice instance {0}...", (object)instanceId));
		}

		private void AddShipToInvoice(
		  int systemId,
		  DesignInfo selectedDesign,
		  int invoiceId,
		  int instanceId,
		  int? aiFleetID)
		{
			BuildScreenState.InvoiceItem invoiceItem = new BuildScreenState.InvoiceItem();
			invoiceItem.DesignID = selectedDesign.ID;
			invoiceItem.isPrototypeOrder = false;
			invoiceItem.ShipName = this._game.NamesPool.GetShipName(this._game, this._player.ID, selectedDesign.Class, (IEnumerable<string>)null);
			/*if (selectedDesign.GetRealShipClass().Value == RealShipClasses.Drone)
			{
				int num = 0 + 1;
			}*/
			this._db.InsertInvoiceBuildOrder(invoiceId, invoiceItem.DesignID, invoiceItem.ShipName, 0);
			this._db.InsertBuildOrder(systemId, invoiceItem.DesignID, 0, 0, invoiceItem.ShipName, selectedDesign.GetPlayerProductionCost(this._db, this._player.ID, !selectedDesign.isPrototyped, new float?()), new int?(instanceId), aiFleetID, 0);
			StrategicAI.TraceVerbose(string.Format("      + Design {0} (ship name '{1}')", (object)selectedDesign, (object)invoiceItem.ShipName));
		}

		private List<ShipInclude> TryFillWithShipFromReserve(
		  AIFleetInfo aifleet,
		  List<ShipInfo> freeReserveShips,
		  List<ShipInclude> templateIncludes)
		{
			List<ShipInclude> shipIncludeList = new List<ShipInclude>();
			shipIncludeList.AddRange((IEnumerable<ShipInclude>)templateIncludes);
			if (freeReserveShips.Count > 0)
			{
				foreach (ShipInclude templateInclude in templateIncludes)
				{
					ShipInclude inc = templateInclude;
					ShipInfo shipInfo = freeReserveShips.FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x =>
				   {
					   if (!StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, inc.ShipRole))
						   return false;
					   WeaponRole weaponRole1 = x.DesignInfo.WeaponRole;
					   WeaponRole? weaponRole2 = inc.WeaponRole;
					   if (weaponRole1 == weaponRole2.GetValueOrDefault())
						   return weaponRole2.HasValue;
					   return false;
				   })) ?? freeReserveShips.FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, inc.ShipRole)));
					if (shipInfo != null)
					{
						this._db.UpdateShipAIFleetID(shipInfo.ID, new int?(aifleet.ID));
						shipIncludeList.Remove(inc);
					}
				}
			}
			return shipIncludeList;
		}

		private List<BuildScreenState.InvoiceItem> GetInvoiceAfterInculdingReserve(
		  List<ShipInfo> freeReserveShips,
		  List<BuildScreenState.InvoiceItem> desiredItems)
		{
			List<BuildScreenState.InvoiceItem> invoiceItemList = new List<BuildScreenState.InvoiceItem>();
			invoiceItemList.AddRange((IEnumerable<BuildScreenState.InvoiceItem>)desiredItems);
			if (freeReserveShips.Count > 0)
			{
				List<ShipInfo> source = new List<ShipInfo>();
				source.AddRange((IEnumerable<ShipInfo>)freeReserveShips);
				foreach (BuildScreenState.InvoiceItem desiredItem in desiredItems)
				{
					DesignInfo desiredDesign = this._db.GetDesignInfo(desiredItem.DesignID);
					if (desiredDesign != null)
					{
						ShipInfo shipInfo = source.FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x =>
					   {
						   if (StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, desiredDesign.Role))
							   return x.DesignInfo.WeaponRole == desiredDesign.WeaponRole;
						   return false;
					   })) ?? source.FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, desiredDesign.Role)));
						if (shipInfo != null)
						{
							source.Remove(shipInfo);
							invoiceItemList.Remove(desiredItem);
						}
					}
				}
			}
			return invoiceItemList;
		}

		private List<BuildScreenState.InvoiceItem> TryFillWithShipFromReserve(
		  AIFleetInfo aifleet,
		  List<ShipInfo> freeReserveShips,
		  List<BuildScreenState.InvoiceItem> desiredItems)
		{
			List<BuildScreenState.InvoiceItem> invoiceItemList = new List<BuildScreenState.InvoiceItem>();
			invoiceItemList.AddRange((IEnumerable<BuildScreenState.InvoiceItem>)desiredItems);
			if (freeReserveShips.Count > 0)
			{
				List<ShipInfo> source = new List<ShipInfo>();
				source.AddRange((IEnumerable<ShipInfo>)freeReserveShips);
				foreach (BuildScreenState.InvoiceItem desiredItem in desiredItems)
				{
					DesignInfo desiredDesign = this._db.GetDesignInfo(desiredItem.DesignID);
					if (desiredDesign != null)
					{
						ShipInfo shipInfo = source.FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x =>
					   {
						   if (StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, desiredDesign.Role))
							   return x.DesignInfo.WeaponRole == desiredDesign.WeaponRole;
						   return false;
					   })) ?? source.FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, desiredDesign.Role)));
						if (shipInfo != null)
						{
							this._db.UpdateShipAIFleetID(shipInfo.ID, new int?(aifleet.ID));
							source.Remove(shipInfo);
							invoiceItemList.Remove(desiredItem);
						}
					}
				}
			}
			return invoiceItemList;
		}

		private int GetBuildInvoiceCost(App app, List<BuildScreenState.InvoiceItem> items)
		{
			int num = 0;
			foreach (BuildScreenState.InvoiceItem invoiceItem in items)
			{
				DesignInfo designInfo = app.GameDatabase.GetDesignInfo(invoiceItem.DesignID);
				num += designInfo.GetPlayerProductionCost(this._db, this._player.ID, true, new float?());
			}
			return num;
		}

		private List<ShipInfo> GetUnclaimedShipsInReserve(int systemId)
		{
			int? reserveFleetId = this._db.GetReserveFleetID(this._player.ID, systemId);
			if (reserveFleetId.HasValue)
				return this._db.GetShipInfoByFleetID(reserveFleetId.Value, true).Where<ShipInfo>((Func<ShipInfo, bool>)(x =>
			   {
				   int? aiFleetId = x.AIFleetID;
				   if (((aiFleetId.GetValueOrDefault() != 0 ? 0 : (aiFleetId.HasValue ? 1 : 0)) != 0 || !x.AIFleetID.HasValue) && x.DesignInfo.GetRealShipClass().HasValue)
					   return !ShipSectionAsset.IsWeaponBattleRiderClass(x.DesignInfo.GetRealShipClass().Value);
				   return false;
			   })).ToList<ShipInfo>();
			return new List<ShipInfo>();
		}

		private void BuildFleet(
		  AIInfo aiInfo,
		  AdmiralInfo admiral,
		  StarSystemInfo system,
		  FleetTemplate fleetTemplate = null,
		  List<BuildScreenState.InvoiceItem> desiredInvoice = null,
		  bool tryReserve = false)
		{
			StrategicAI.TraceVerbose(string.Format("  Building a new fleet with Admiral {0} at system {1}...", (object)admiral, (object)system));
			FleetTemplate template = fleetTemplate;
			if (template == null)
			{
				List<FleetTemplate> list = this._db.AssetDatabase.FleetTemplates.Where<FleetTemplate>((Func<FleetTemplate, bool>)(x =>
			   {
				   if (x.StanceWeights.ContainsKey(aiInfo.Stance))
					   return !x.Initial;
				   return false;
			   })).ToList<FleetTemplate>();
				IEnumerable<Weighted<FleetTemplate>> weighteds = list.Select<FleetTemplate, Weighted<FleetTemplate>>((Func<FleetTemplate, Weighted<FleetTemplate>>)(x => new Weighted<FleetTemplate>(x, x.StanceWeights[aiInfo.Stance])));
				if (weighteds.Count<Weighted<FleetTemplate>>() <= 0 || list.Count<FleetTemplate>() <= 0)
				{
					StrategicAI.TraceVerbose("    Cannot proceed: no available fleet templates.");
					return;
				}
				template = WeightedChoices.Choose<FleetTemplate>(this._random, weighteds);
			}
			StrategicAI.TraceVerbose(string.Format("    Selected fleet template: {0}", (object)template.Name));
			AIFleetInfo aiFleetInfo = new AIFleetInfo()
			{
				AdmiralID = new int?(admiral.ID),
				FleetType = MissionTypeExtensions.SerializeList(template.MissionTypes),
				SystemID = system.ID,
				FleetTemplate = template.Name
			};
			aiFleetInfo.ID = this._db.InsertAIFleetInfo(this._player.ID, aiFleetInfo);
			List<ShipInfo> freeReserveShips = new List<ShipInfo>();
			if (tryReserve)
				freeReserveShips = this.GetUnclaimedShipsInReserve(system.ID);
			if (desiredInvoice != null && desiredInvoice.Count > 0)
			{
				List<BuildScreenState.InvoiceItem> invoiceItemList = desiredInvoice;
				if (tryReserve)
					invoiceItemList = this.TryFillWithShipFromReserve(aiFleetInfo, freeReserveShips, desiredInvoice);
				if (invoiceItemList.Count == 0)
					return;
				int invoiceId;
				int instanceId;
				this.OpenInvoice(system.ID, admiral.Name, out invoiceId, out instanceId);
				aiFleetInfo.InvoiceID = new int?(instanceId);
				this._db.UpdateAIFleetInfo(aiFleetInfo);
				foreach (BuildScreenState.InvoiceItem invoiceItem in invoiceItemList)
					this.AddShipToInvoice(system.ID, this._db.GetDesignInfo(invoiceItem.DesignID), invoiceId, instanceId, new int?(aiFleetInfo.ID));
			}
			else
			{
				List<ShipInclude> shipIncludeList = template.ShipIncludes;
				if (tryReserve)
					shipIncludeList = this.TryFillWithShipFromReserve(aiFleetInfo, freeReserveShips, template.ShipIncludes);
				if (shipIncludeList.Count == 0)
					return;
				int invoiceId;
				int instanceId;
				this.OpenInvoice(system.ID, admiral.Name, out invoiceId, out instanceId);
				aiFleetInfo.InvoiceID = new int?(instanceId);
				this._db.UpdateAIFleetInfo(aiFleetInfo);
				foreach (ShipInclude shipInclude in shipIncludeList)
				{
					if ((string.IsNullOrEmpty(shipInclude.Faction) || !(shipInclude.Faction != this._player.Faction.Name)) && !(shipInclude.FactionExclusion == this._player.Faction.Name))
					{
						DesignInfo designByRole1 = DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(aiInfo.Stance), shipInclude.ShipRole, shipInclude.WeaponRole);
						int num;
						if (shipInclude.InclusionType == ShipInclusionType.FILL)
						{
							DesignInfo designByRole2 = DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(aiInfo.Stance), ShipRole.COMMAND, new WeaponRole?());
							num = DesignLab.GetTemplateFillAmount(this._db, template, designByRole2, designByRole1);
						}
						else
							num = shipInclude.Amount;
						for (int index = 0; index < num; ++index)
						{
							if (designByRole1 != null)
								this.AddShipToInvoice(system.ID, designByRole1, invoiceId, instanceId, new int?(aiFleetInfo.ID));
						}
					}
				}
			}
		}

		private float AssessExpansionRoom()
		{
			string factionName = this._db.GetFactionName(this._db.GetPlayerFactionID(this._player.ID));
			float factionSuitability = this._db.GetFactionSuitability(factionName);
			List<int> intList = new List<int>();
			float range = this._db.FindCurrentDriveSpeedForPlayer(this._player.ID) * 6f;
			if (factionName.Contains("hiver"))
				range *= 6f;
			foreach (StationInfo stationInfo in this._db.GetStationInfosByPlayerID(this._player.ID))
			{
				foreach (StarSystemInfo starSystemInfo in this._db.GetSystemsInRange(this._db.GetStarSystemOrigin(this._db.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID).StarSystemID), range))
				{
					if (!intList.Contains(starSystemInfo.ID))
						intList.Add(starSystemInfo.ID);
				}
			}
			int num1 = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (int systemId in intList)
			{
				StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(systemId);
				int exploredByPlayer = this._db.GetLastTurnExploredByPlayer(this._player.ID, starSystemInfo.ID);
				if (exploredByPlayer > 0)
				{
					int num4 = 0;
					foreach (PlanetInfo systemPlanetInfo in this._db.GetStarSystemPlanetInfos(starSystemInfo.ID))
					{
						ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(systemPlanetInfo.ID);
						if (colonyInfoForPlanet == null)
						{
							if ((double)Math.Abs(systemPlanetInfo.Suitability - factionSuitability) < 200.0)
								++num4;
						}
						else if (colonyInfoForPlanet.PlayerID != this._player.ID && colonyInfoForPlanet.TurnEstablished < exploredByPlayer)
						{
							++num3;
							break;
						}
					}
					num1 += num4;
				}
				else
					++num2;
			}
			return num3 <= 0 ? (float)(num1 + num2) : (float)(num1 + num2) / (float)num3;
		}

		private float AssessPlayerStrength(int otherPlayerID)
		{
			int num1 = 0;
			using (IEnumerator<ColonyInfo> enumerator = this._db.GetColonyInfos().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.PlayerID == otherPlayerID && (this._db.IsStarSystemVisibleToPlayer(this._player.ID, enumerator.Current.CachedStarSystemID) || this._db.GetLastTurnExploredByPlayer(this._player.ID, enumerator.Current.CachedStarSystemID) > 0))
						++num1;
				}
			}
			if (num1 == 0)
				return 0.0f;
			float num2 = (float)this.AssessPlayerTechLevel(otherPlayerID);
			return (float)((double)num1 * 10.0 + (double)num2 * 50.0);
		}

		private float AssessOwnStrength()
		{
			int num1 = 0;
			foreach (ColonyInfo colonyInfo in this._db.GetColonyInfos())
			{
				if (colonyInfo.PlayerID == this._player.ID)
					++num1;
			}
			int num2 = this.AssessOwnTechLevel();
			return (float)((double)num1 * 10.0 + (double)num2 * 50.0);
		}

		private int AssessPlayerTechLevel(int otherPlayerID)
		{
			int num = 1;
			if (this._db.PlayerHasAntimatter(otherPlayerID))
				num += 3;
			if (this._db.PlayerHasDreadnoughts(otherPlayerID))
				++num;
			if (this._db.PlayerHasLeviathans(otherPlayerID))
				++num;
			return num;
		}

		private int AssessOwnTechLevel()
		{
			int num = 1;
			if (this._db.PlayerHasAntimatter(this._player.ID))
				num += 3;
			if (this._db.PlayerHasDreadnoughts(this._player.ID))
				++num;
			if (this._db.PlayerHasLeviathans(this._player.ID))
				++num;
			return num;
		}

		public static StrategicAI.DesignConfigurationInfo GetDesignConfigurationInfo(
		  GameSession game,
		  DesignInfo design)
		{
			StrategicAI.DesignConfigurationInfo configurationInfo = new StrategicAI.DesignConfigurationInfo();
			configurationInfo.Maxspeed += design.TopSpeed;
			foreach (DesignSectionInfo designSection in design.DesignSections)
			{
				if (((IEnumerable<string>)designSection.ShipSectionAsset.RequiredTechs).Contains<string>("SLD_Disruptor_Shields"))
					configurationInfo.EnergyDefense += 10f;
				foreach (int tech in designSection.Techs)
				{
					switch (game.GameDatabase.GetTechFileID(tech))
					{
						case "IND_Reflective":
							configurationInfo.EnergyDefense += 5f;
							continue;
						case "IND_Improved_Reflective":
							configurationInfo.EnergyDefense += 10f;
							continue;
						case "IND_Polysteel":
							++configurationInfo.Defensive;
							continue;
						case "IND_MagnoCeramic_Latices":
							configurationInfo.Defensive += 2f;
							continue;
						case "IND_Quark_Resonators":
							configurationInfo.Defensive += 3f;
							continue;
						case "IND_Adamantine_Alloys":
							configurationInfo.Defensive += 6f;
							continue;
						case "SLD_Shield_Mk._I":
							++configurationInfo.Defensive;
							continue;
						case "SLD_Shield_Mk._II":
							configurationInfo.Defensive += 2f;
							continue;
						case "SLD_Shield_Mk._III":
							configurationInfo.Defensive += 3f;
							continue;
						case "SLD_Shield_Mk._IV":
							configurationInfo.Defensive += 4f;
							continue;
						case "LD_Grav_Shields":
							configurationInfo.BallisticsDefense += 100f;
							continue;
						case "SLD_Meson_Shields":
							configurationInfo.EnergyDefense += 100f;
							continue;
						default:
							continue;
					}
				}
				foreach (WeaponBankInfo weaponBank1 in designSection.WeaponBanks)
				{
					WeaponBankInfo weaponBank = weaponBank1;
					if (weaponBank.WeaponID.HasValue)
					{
						string weaponFile = game.GameDatabase.GetWeaponAsset(weaponBank.WeaponID.Value);
						LogicalWeapon logicalWeapon = game.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == weaponFile));
						if (logicalWeapon != null)
						{
							LogicalBank logicalBank = ((IEnumerable<LogicalBank>)designSection.ShipSectionAsset.Banks).FirstOrDefault<LogicalBank>((Func<LogicalBank, bool>)(x => x.GUID == weaponBank.BankGUID));
							if (logicalBank != null)
							{
								switch (logicalBank.TurretClass)
								{
									case WeaponEnums.TurretClasses.Standard:
										if (logicalWeapon.PayloadType == WeaponEnums.PayloadTypes.Bolt)
										{
											if (((IEnumerable<WeaponEnums.WeaponTraits>)logicalWeapon.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Energy))
											{
												++configurationInfo.EnergyWeapons;
												continue;
											}
											++configurationInfo.BallisticsWeapons;
											continue;
										}
										continue;
									case WeaponEnums.TurretClasses.Missile:
										++configurationInfo.MissileWeapons;
										continue;
									case WeaponEnums.TurretClasses.HeavyBeam:
										configurationInfo.HeavyBeamWeapons += 10f;
										continue;
									default:
										continue;
								}
							}
						}
					}
				}
			}
			return configurationInfo;
		}

		public int GetLoaCompositionIDByMission(MissionType missionType)
		{
			if (!(this._player.Faction.Name == "loa"))
				return 0;
			int num1 = 0;
			string str = null;
			List<LoaFleetComposition> list1 = this._db.GetLoaFleetCompositions().Where<LoaFleetComposition>((Func<LoaFleetComposition, bool>)(x => x.PlayerID == this._player.ID)).ToList<LoaFleetComposition>();
			List<FleetTemplate> list2 = this._db.AssetDatabase.FleetTemplates.Where<FleetTemplate>((Func<FleetTemplate, bool>)(x =>
		   {
			   if (x.MissionTypes.Contains(missionType))
				   return x.CanFactionUse("loa");
			   return false;
		   })).ToList<FleetTemplate>();
			int num2 = 0;
			foreach (FleetTemplate fleetTemplate in list2)
			{
				Dictionary<AIStance, int> stanceWeights1 = fleetTemplate.StanceWeights;
				AIStance? stance = this._player.Stance;
				int num3 = (int)stance.Value;
				if (stanceWeights1.ContainsKey((AIStance)num3))
				{
					Dictionary<AIStance, int> stanceWeights2 = fleetTemplate.StanceWeights;
					stance = this._player.Stance;
					int num4 = (int)stance.Value;
					if (stanceWeights2[(AIStance)num4] > num2)
					{
						str = fleetTemplate.Name;
						Dictionary<AIStance, int> stanceWeights3 = fleetTemplate.StanceWeights;
						stance = this._player.Stance;
						int num5 = (int)stance.Value;
						num2 = stanceWeights3[(AIStance)num5];
					}
				}
			}
			foreach (LoaFleetComposition fleetComposition in list1.AsEnumerable<LoaFleetComposition>().Reverse<LoaFleetComposition>())
			{
				if (fleetComposition.Name == str)
				{
					num1 = fleetComposition.ID;
					break;
				}
			}
			if (num1 != 0)
				return num1;
			if (missionType == MissionType.COLONIZATION || missionType == MissionType.SUPPORT)
			{
				foreach (LoaFleetComposition fleetComposition in list1.AsEnumerable<LoaFleetComposition>().Reverse<LoaFleetComposition>())
				{
					if (fleetComposition.Name == "DEFAULT_COLONIZATION")
						return fleetComposition.ID;
				}
			}
			if (missionType == MissionType.CONSTRUCT_STN || missionType == MissionType.UPGRADE_STN)
			{
				foreach (LoaFleetComposition fleetComposition in list1.AsEnumerable<LoaFleetComposition>().Reverse<LoaFleetComposition>())
				{
					if (fleetComposition.Name == "DEFAULT_CONSTRUCTION")
						return fleetComposition.ID;
				}
			}
			foreach (LoaFleetComposition fleetComposition in list1.AsEnumerable<LoaFleetComposition>().Reverse<LoaFleetComposition>())
			{
				if (fleetComposition.Name == "DEFAULT_COMBAT")
					return fleetComposition.ID;
			}
			return 0;
		}

		private struct DesignPriority
		{
			public ShipRole role;
			public float weight;
		}

		public class UpdatePlayerInfo
		{
			public readonly Dictionary<int, DiplomacyInfo> Relations = new Dictionary<int, DiplomacyInfo>();
			public readonly GameDatabase _db;
			public readonly PlayerInfo Player;

			internal UpdatePlayerInfo(
			  GameDatabase db,
			  IEnumerable<PlayerInfo> players,
			  PlayerInfo player)
			{
				this._db = db;
				this.Player = player;
				foreach (PlayerInfo player1 in players)
					this.Relations[player1.ID] = this._db.GetDiplomacyInfo(player.ID, player1.ID);
			}
		}

		public class UpdateInfo
		{
			public readonly Dictionary<int, StrategicAI.UpdatePlayerInfo> Players = new Dictionary<int, StrategicAI.UpdatePlayerInfo>();

			public UpdateInfo(GameDatabase db)
			{
				List<PlayerInfo> list = db.GetPlayerInfos().ToList<PlayerInfo>();
				foreach (PlayerInfo player in list)
					this.Players[player.ID] = new StrategicAI.UpdatePlayerInfo(db, (IEnumerable<PlayerInfo>)list, player);
			}
		}

		public class BattleRiderMountSet
		{
			private Dictionary<WeaponEnums.TurretClasses, List<ShipInfo>> tcmap;
			private List<ShipInfo> set;

			public static WeaponEnums.TurretClasses? GetMatchingTurretClass(DesignInfo design)
			{
				BattleRiderTypes battleRiderType = design.GetMissionSectionAsset().BattleRiderType;
				if (battleRiderType.IsBattleRiderType() && design.Class == ShipClass.BattleRider)
					return new WeaponEnums.TurretClasses?(WeaponEnums.TurretClasses.DestroyerRider);
				if (battleRiderType.IsControllableBattleRider())
				{
					RealShipClasses? realShipClass = design.GetRealShipClass();
					if ((realShipClass.GetValueOrDefault() != RealShipClasses.BattleCruiser ? 0 : (realShipClass.HasValue ? 1 : 0)) != 0)
						return new WeaponEnums.TurretClasses?(WeaponEnums.TurretClasses.CruiserRider);
				}
				if (battleRiderType.IsControllableBattleRider())
				{
					RealShipClasses? realShipClass = design.GetRealShipClass();
					if ((realShipClass.GetValueOrDefault() != RealShipClasses.BattleShip ? 0 : (realShipClass.HasValue ? 1 : 0)) != 0)
						return new WeaponEnums.TurretClasses?(WeaponEnums.TurretClasses.DreadnoughtRider);
				}
				return new WeaponEnums.TurretClasses?();
			}

			public static IEnumerable<ShipRole> EnumerateShipRolesByTurretClass(
			  WeaponEnums.TurretClasses value)
			{
				switch (value)
				{
					case WeaponEnums.TurretClasses.DestroyerRider:
						yield return ShipRole.BR_PATROL;
						yield return ShipRole.BR_SCOUT;
						yield return ShipRole.BR_SPINAL;
						yield return ShipRole.BR_ESCORT;
						yield return ShipRole.BR_INTERCEPTOR;
						yield return ShipRole.BR_TORPEDO;
						break;
					case WeaponEnums.TurretClasses.CruiserRider:
						yield return ShipRole.BATTLECRUISER;
						break;
					case WeaponEnums.TurretClasses.DreadnoughtRider:
						yield return ShipRole.BATTLESHIP;
						break;
				}
			}

			public static bool IsBattleRiderTurretClass(WeaponEnums.TurretClasses value)
			{
				switch (value)
				{
					case WeaponEnums.TurretClasses.DestroyerRider:
					case WeaponEnums.TurretClasses.CruiserRider:
					case WeaponEnums.TurretClasses.DreadnoughtRider:
						return true;
					default:
						return false;
				}
			}

			private void FilterAdd(ShipInfo value)
			{
				if (value.ParentID != 0)
					return;
				WeaponEnums.TurretClasses? matchingTurretClass = StrategicAI.BattleRiderMountSet.GetMatchingTurretClass(value.DesignInfo);
				if (!matchingTurretClass.HasValue)
					return;
				this.tcmap[matchingTurretClass.Value].Add(value);
				this.set.Add(value);
			}

			public BattleRiderMountSet(IEnumerable<ShipInfo> ships)
			{
				this.tcmap = new Dictionary<WeaponEnums.TurretClasses, List<ShipInfo>>();
				this.tcmap[WeaponEnums.TurretClasses.DestroyerRider] = new List<ShipInfo>();
				this.tcmap[WeaponEnums.TurretClasses.CruiserRider] = new List<ShipInfo>();
				this.tcmap[WeaponEnums.TurretClasses.DreadnoughtRider] = new List<ShipInfo>();
				this.set = new List<ShipInfo>();
				foreach (ShipInfo ship in ships)
					this.FilterAdd(ship);
			}

			public IEnumerable<ShipInfo> EnumerateByTurretClass(
			  WeaponEnums.TurretClasses value)
			{
				return (IEnumerable<ShipInfo>)this.tcmap[value];
			}

			public bool Contains(ShipInfo value)
			{
				return this.set.Contains(value);
			}

			public void Remove(ShipInfo value)
			{
				this.set.Remove(value);
				foreach (List<ShipInfo> shipInfoList in this.tcmap.Values)
					shipInfoList.Remove(value);
			}

			public static IEnumerable<KeyValuePair<LogicalMount, int>> EnumerateBattleRiderMounts(
			  DesignInfo design)
			{
				int riderIndex = 0;
				foreach (DesignSectionInfo designSection in design.DesignSections)
				{
					foreach (LogicalMount mount in designSection.ShipSectionAsset.Mounts)
					{
						if (WeaponEnums.IsBattleRider(mount.Bank.TurretClass))
						{
							if (StrategicAI.BattleRiderMountSet.IsBattleRiderTurretClass(mount.Bank.TurretClass))
								yield return new KeyValuePair<LogicalMount, int>(mount, riderIndex);
							++riderIndex;
						}
					}
				}
			}

			public ShipInfo FindShipByRole(ShipRole role)
			{
				return this.set.FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.Role == role));
			}

			public void AttachBattleRiderToShip(
			  GameDatabase db,
			  StrategicAI.BankRiderInfo bankRiderInfo,
			  ShipInfo battleRider,
			  ShipInfo carrier,
			  int riderIndex)
			{
				this.Remove(battleRider);
				bankRiderInfo.FreeRiderIndices.Remove(riderIndex);
				bankRiderInfo.FilledRiderIndices.Add(riderIndex);
				bankRiderInfo.AllocatedRole = new ShipRole?(battleRider.DesignInfo.Role);
				db.SetShipParent(battleRider.ID, carrier.ID);
				db.UpdateShipRiderIndex(battleRider.ID, riderIndex);
			}
		}

		public class BankRiderInfo
		{
			public List<int> FilledRiderIndices = new List<int>();
			public List<int> FreeRiderIndices = new List<int>();
			public LogicalBank Bank;
			public ShipRole? AllocatedRole;
		}

		public struct DesignConfigurationInfo
		{
			public float Maxspeed;
			public float PointDefense;
			public float Defensive;
			public float MissileWeapons;
			public float EnergyDefense;
			public float BallisticsDefense;
			public float EnergyWeapons;
			public float HeavyBeamWeapons;
			public float BallisticsWeapons;

			public void Average(int size)
			{
				this.Maxspeed /= (float)size;
				this.PointDefense /= (float)size;
				this.Defensive /= (float)size;
				this.MissileWeapons /= (float)size;
				this.EnergyDefense /= (float)size;
				this.BallisticsDefense /= (float)size;
				this.EnergyWeapons /= (float)size;
				this.HeavyBeamWeapons /= (float)size;
				this.BallisticsWeapons /= (float)size;
			}

			public static StrategicAI.DesignConfigurationInfo operator +(
			  StrategicAI.DesignConfigurationInfo c1,
			  StrategicAI.DesignConfigurationInfo c2)
			{
				return new StrategicAI.DesignConfigurationInfo()
				{
					Maxspeed = c1.Maxspeed + c2.Maxspeed,
					PointDefense = c1.PointDefense + c2.PointDefense,
					Defensive = c1.Defensive + c2.Defensive,
					MissileWeapons = c1.MissileWeapons + c2.MissileWeapons,
					EnergyDefense = c1.EnergyDefense + c2.EnergyDefense,
					BallisticsDefense = c1.BallisticsDefense + c2.BallisticsDefense,
					EnergyWeapons = c1.EnergyWeapons + c2.EnergyWeapons,
					HeavyBeamWeapons = c1.HeavyBeamWeapons + c2.HeavyBeamWeapons,
					BallisticsWeapons = c1.BallisticsWeapons + c2.BallisticsWeapons
				};
			}
		}
	}
}
