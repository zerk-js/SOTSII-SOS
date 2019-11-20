// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.GameSession
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Combat;
using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.ScenarioFramework;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.GameTriggers;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Steam;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text.RegularExpressions;

namespace Kerberos.Sots.Strategy
{
	internal class GameSession : IDisposable
	{
		public List<TurnEvent> TurnEvents = new List<TurnEvent>();
		public readonly List<Trigger> ActiveTriggers = new List<Trigger>();
		public List<EventStub> TriggerEvents = new List<EventStub>();
		public readonly Dictionary<string, float> TriggerScalars = new Dictionary<string, float>();
		public readonly IList<Kerberos.Sots.PlayerFramework.Player> OtherPlayers = (IList<Kerberos.Sots.PlayerFramework.Player>)new List<Kerberos.Sots.PlayerFramework.Player>();
		public object StarMapSelectedObject = (object)0;
		private Dictionary<int, int> _playerGateMap = new Dictionary<int, int>();
		private readonly Dictionary<Kerberos.Sots.PlayerFramework.Player, GameSession.SimPlayerInfo> PlayerInfos = new Dictionary<Kerberos.Sots.PlayerFramework.Player, GameSession.SimPlayerInfo>();
		private readonly List<Kerberos.Sots.PlayerFramework.Player> m_Players = new List<Kerberos.Sots.PlayerFramework.Player>();
		private readonly List<Kerberos.Sots.StarSystem> m_Systems = new List<Kerberos.Sots.StarSystem>();
		private readonly Dictionary<Faction, float> m_SpeciesIdealSuitability = new Dictionary<Faction, float>();
		public const int IntelMissionCriticalSuccessPercent = 5;
		public const int IntelMissionSuccessPercent = 25;
		internal const float InstaBuildConstructionPoints = 1E+09f;
		private const double DebtInterestRate = 0.15;
		private const double SavingsInterestRate = 0.01;
		public static bool ForceIntelMissionCriticalSuccessHack;
		public PendingCombat _currentCombat;
		public SimState State;
		private string _saveGameName;
		public OrbitCameraController StarMapCamera;
		protected GameObjectSet _crits;
		private TurnTimer _turnTimer;
		private Dialog DialogCombatsPending;
		private CombatDataHelper _combatData;
		public static int _muniquecombatID;
		private Dictionary<int, double> _incomeFromTrade;
		public static bool ForceReactionHack;
		public static bool SkipCombatHack;
		internal static bool InstaBuildHackEnabled;
		internal static int SimAITurns;
		private App _app;
		private GameDatabase _db;
		private Random _random;
		private uint m_GameID;
		private List<PendingCombat> m_Combats;
		private List<FleetInfo> fleetsInCombat;
		private List<ReactionInfo> _reactions;
		private MultiCombatCarryOverData m_MCCarryOverData;
		private OpenCloseSystemToggleData m_OCSystemToggleData;

		private void VerifyNotTreaty(DiplomacyAction action)
		{
			if (action == DiplomacyAction.TREATY)
				throw new ArgumentException("TREATY is not supported here. Use other methods to gather the needed information instead.");
		}

		public int? GetDiplomacyActionCost(
		  DiplomacyAction action,
		  RequestType? request,
		  DemandType? demand)
		{
			switch (action)
			{
				case DiplomacyAction.DECLARATION:
				case DiplomacyAction.SURPRISEATTACK:
					return new int?(action == DiplomacyAction.DECLARATION ? this.AssetDatabase.DeclareWarPointCost : 0);
				case DiplomacyAction.REQUEST:
					if (request.HasValue)
						return new int?(this.AssetDatabase.GetDiplomaticRequestPointCost(request.Value));
					return new int?(0);
				case DiplomacyAction.DEMAND:
					if (demand.HasValue)
						return new int?(this.AssetDatabase.GetDiplomaticDemandPointCost(demand.Value));
					return new int?(0);
				case DiplomacyAction.TREATY:
					return new int?(0);
				case DiplomacyAction.LOBBY:
					return new int?(50);
				case DiplomacyAction.SPIN:
					return new int?();
				default:
					return new int?();
			}
		}

		private int GetArmisticeStepCost(DiplomacyState lowerState, DiplomacyState upperState)
		{
			DiplomacyStateChange index = this.AssetDatabase.DiplomacyStateChangeMap.Keys.FirstOrDefault<DiplomacyStateChange>((Func<DiplomacyStateChange, bool>)(x =>
		   {
			   if (x.lower == lowerState)
				   return x.upper == upperState;
			   return false;
		   }));
			if (index != null)
				return this.AssetDatabase.DiplomacyStateChangeMap[index];
			int val1 = int.MaxValue;
			foreach (KeyValuePair<DiplomacyStateChange, int> keyValuePair in this.AssetDatabase.DiplomacyStateChangeMap.Where<KeyValuePair<DiplomacyStateChange, int>>((Func<KeyValuePair<DiplomacyStateChange, int>, bool>)(x => x.Key.lower == lowerState)).ToDictionary<KeyValuePair<DiplomacyStateChange, int>, DiplomacyStateChange, int>((Func<KeyValuePair<DiplomacyStateChange, int>, DiplomacyStateChange>)(y => y.Key), (Func<KeyValuePair<DiplomacyStateChange, int>, int>)(y => y.Value)))
				val1 = Math.Min(val1, keyValuePair.Value + this.GetArmisticeStepCost(keyValuePair.Key.upper, upperState));
			if (val1 == int.MaxValue)
				val1 = 0;
			return val1;
		}

		public int GetTreatyRdpCost(TreatyInfo ti)
		{
			if (ti.Type == TreatyType.Armistice)
				return this.GetArmisticeStepCost(this.GameDatabase.GetDiplomacyStateBetweenPlayers(ti.InitiatingPlayerId, ti.ReceivingPlayerId), (ti as ArmisticeTreatyInfo).SuggestedDiplomacyState);
			if (ti.Type == TreatyType.Incorporate)
				return this.AssetDatabase.TreatyIncorporatePointCost;
			if (ti.Type == TreatyType.Protectorate)
				return this.AssetDatabase.TreatyProtectoratePointCost;
			if (ti.Type == TreatyType.Trade)
				return this.AssetDatabase.TreatyTradePointCost;
			if (ti.Type != TreatyType.Limitation)
				return 0;
			int duration = ti.Duration;
			switch ((ti as LimitationTreatyInfo).LimitationType)
			{
				case LimitationTreatyType.FleetSize:
					duration += this.AssetDatabase.TreatyLimitationFleetsPointCost;
					break;
				case LimitationTreatyType.ShipClass:
					duration += this.AssetDatabase.TreatyLimitationShipClassPointCost;
					break;
				case LimitationTreatyType.Weapon:
					duration += this.AssetDatabase.TreatyLimitationWeaponsPointCost;
					break;
				case LimitationTreatyType.ResearchTree:
					duration += this.AssetDatabase.TreatyLimitationResearchTreePointCost;
					break;
				case LimitationTreatyType.ResearchTech:
					duration += this.AssetDatabase.TreatyLimitationResearchTechPointCost;
					break;
				case LimitationTreatyType.EmpireSize:
					duration += this.AssetDatabase.TreatyLimitationColoniesPointCost;
					break;
				case LimitationTreatyType.ForgeGemWorlds:
					duration += this.AssetDatabase.TreatyLimitationForgeGemWorldsPointCost;
					break;
				case LimitationTreatyType.StationType:
					duration += this.AssetDatabase.TreatyLimitationStationType;
					break;
			}
			return duration;
		}

		private bool CanAffordDiplomacyAction(
		  PlayerInfo self,
		  DiplomacyAction action,
		  RequestType? request,
		  DemandType? demand)
		{
			int totalDiplomacyPoints = self.GetTotalDiplomacyPoints(self.FactionID);
			int? diplomacyActionCost = this.GetDiplomacyActionCost(action, request, demand);
			if (!diplomacyActionCost.HasValue)
				return false;
			return diplomacyActionCost.Value <= totalDiplomacyPoints;
		}

		private bool CanAffordTreaty(TreatyInfo treatyInfo)
		{
			PlayerInfo playerInfo1 = this.GameDatabase.GetPlayerInfo(treatyInfo.InitiatingPlayerId);
			PlayerInfo playerInfo2 = this.GameDatabase.GetPlayerInfo(treatyInfo.ReceivingPlayerId);
			return this.GetTreatyRdpCost(treatyInfo) <= playerInfo1.GetTotalDiplomacyPoints(playerInfo2.FactionID);
		}

		public bool CanPerformTreaty(TreatyInfo treatyInfo)
		{
			return this.CanAffordTreaty(treatyInfo) && this.CanPerformDiplomacyAction(this.GameDatabase.GetPlayerInfo(treatyInfo.InitiatingPlayerId), this.GameDatabase.GetPlayerInfo(treatyInfo.ReceivingPlayerId), DiplomacyAction.TREATY, new RequestType?(), new DemandType?());
		}

		private bool CanPerformDiplomacyRequestAction(PlayerInfo target, RequestType request)
		{
			switch (request)
			{
				case RequestType.SavingsRequest:
				case RequestType.SystemInfoRequest:
				case RequestType.ResearchPointsRequest:
				case RequestType.MilitaryAssistanceRequest:
					return true;
				case RequestType.GatePermissionRequest:
					return this.AssetDatabase.GetFaction(target.FactionID).CanUseGate();
				case RequestType.EstablishEnclaveRequest:
					return !this.AssetDatabase.GetFaction(target.FactionID).IsIndependent();
				default:
					throw new ArgumentOutOfRangeException(nameof(request));
			}
		}

		private void VerifyDiplomacyActionContext(
		  DiplomacyAction action,
		  RequestType? request,
		  DemandType? demand)
		{
			switch (action)
			{
				case DiplomacyAction.DECLARATION:
				case DiplomacyAction.TREATY:
				case DiplomacyAction.LOBBY:
				case DiplomacyAction.SPIN:
				case DiplomacyAction.SURPRISEATTACK:
					if (!demand.HasValue && !request.HasValue)
						break;
					throw new ArgumentException("This DiplomacyAction allows neither request nor demand contexts.");
				case DiplomacyAction.REQUEST:
					if (!demand.HasValue)
						break;
					throw new ArgumentException("REQUEST action does not allow demand context.");
				case DiplomacyAction.DEMAND:
					if (!request.HasValue)
						break;
					throw new ArgumentException("DEMAND action does not allow request context.");
				default:
					throw new ArgumentOutOfRangeException(nameof(action));
			}
		}

		public bool CanPerformDiplomacyAction(
		  PlayerInfo self,
		  PlayerInfo target,
		  DiplomacyAction action,
		  RequestType? request,
		  DemandType? demand)
		{
			this.VerifyDiplomacyActionContext(action, request, demand);
			if (self.ID == target.ID || !this.CanAffordDiplomacyAction(self, action, request, demand) || target.isDefeated)
				return false;
			switch (action)
			{
				case DiplomacyAction.DECLARATION:
				case DiplomacyAction.SURPRISEATTACK:
					IEnumerable<DiplomacyActionHistoryEntryInfo> source = this.GameDatabase.GetDiplomacyActionHistory(self.ID, target.ID, this.GameDatabase.GetTurnCount(), 1).Where<DiplomacyActionHistoryEntryInfo>((Func<DiplomacyActionHistoryEntryInfo, bool>)(x => x.Action == DiplomacyAction.DECLARATION));
					if (!self.IsOnTeam(target) && this.GameDatabase.GetDiplomacyStateBetweenPlayers(self.ID, target.ID) != DiplomacyState.WAR)
						return !source.Any<DiplomacyActionHistoryEntryInfo>();
					return false;
				case DiplomacyAction.REQUEST:
					if (!target.isStandardPlayer || request.HasValue && !this.CanPerformDiplomacyRequestAction(target, request.Value))
						return false;
					switch (this.GameDatabase.GetDiplomacyStateBetweenPlayers(self.ID, target.ID))
					{
						case DiplomacyState.NON_AGGRESSION:
						case DiplomacyState.ALLIED:
						case DiplomacyState.PEACE:
							return true;
						default:
							return false;
					}
				case DiplomacyAction.DEMAND:
					if (!target.isStandardPlayer)
						return false;
					switch (this.GameDatabase.GetDiplomacyStateBetweenPlayers(self.ID, target.ID))
					{
						case DiplomacyState.CEASE_FIRE:
						case DiplomacyState.WAR:
						case DiplomacyState.NEUTRAL:
							return true;
						default:
							return false;
					}
				case DiplomacyAction.TREATY:
					if (!target.isStandardPlayer && !this.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowProtectorate, self.ID))
						return this.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowIncorporate, self.ID);
					return true;
				case DiplomacyAction.LOBBY:
					return this.GetPlayerObject(target.ID).IsAI();
				case DiplomacyAction.SPIN:
					return false;
				default:
					return false;
			}
		}

		public bool CanPerformLocalDiplomacyAction(
		  PlayerInfo target,
		  DiplomacyAction action,
		  RequestType? request,
		  DemandType? demand)
		{
			return this.CanPerformDiplomacyAction(this.GameDatabase.GetPlayerInfo(this.LocalPlayer.ID), target, action, request, demand);
		}

		public void DoLobbyAction(
		  int issuingplayer,
		  int targetplayerid,
		  int towardsplayerID,
		  bool improverelation)
		{
			this._app.GameDatabase.SpendDiplomacyPoints(this._app.GameDatabase.GetPlayerInfo(issuingplayer), this._app.GameDatabase.GetPlayerFactionID(targetplayerid), this._app.Game.GetDiplomacyActionCost(DiplomacyAction.LOBBY, new RequestType?(), new DemandType?()).Value);
			if ((double)new Random().Next(0, 100) < (double)this._app.GameDatabase.GetDiplomacyInfo(targetplayerid, issuingplayer).Relations / 2000.0 * 100.0 + (double)((float)((this._db.GetPlayerFaction(issuingplayer).Name == "morrigi" ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") * 0.5 : 0.0) + (this._db.PlayerHasTech(issuingplayer, "PSI_Lesser_Glamour") ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") : 0.0)) * 100f))
			{
				int reactionAmount = 75;
				if (towardsplayerID == issuingplayer)
					reactionAmount = 50;
				if (!improverelation)
					reactionAmount *= -1;
				if (towardsplayerID == issuingplayer)
					this._db.ApplyDiplomacyReaction(targetplayerid, issuingplayer, reactionAmount, new StratModifiers?(StratModifiers.DiplomacyReactionSelfLobby), 1);
				if (towardsplayerID != issuingplayer)
				{
					this._db.ApplyDiplomacyReaction(targetplayerid, towardsplayerID, reactionAmount, new StratModifiers?(), 1);
					this._db.ApplyDiplomacyReaction(targetplayerid, issuingplayer, -10, new StratModifiers?(StratModifiers.DiplomacyReactionOtherLobby), 1);
				}
				if (this.App.Game.GetPlayerObject(issuingplayer).IsAI())
					return;
				if (towardsplayerID != issuingplayer)
					this.App.UI.CreateDialog((Dialog)new GenericTextDialog(this._app, App.Localize("@UI_LOBBY_SUCCESS"), App.Localize("@UI_LOBBY_SUCCESS_MSG"), "dialogGenericMessage"), null);
				else
					this.App.UI.CreateDialog((Dialog)new GenericTextDialog(this._app, App.Localize("@UI_LOBBY_SUCCESS"), App.Localize("@UI_LOBBY_SELF_SUCCESS_MSG"), "dialogGenericMessage"), null);
			}
			else
			{
				this._db.ApplyDiplomacyReaction(targetplayerid, issuingplayer, -25, new StratModifiers?(StratModifiers.DiplomacyReactionFailedLobby), 1);
				if (this.App.Game.GetPlayerObject(issuingplayer).IsAI())
					return;
				if (towardsplayerID != issuingplayer)
					this.App.UI.CreateDialog((Dialog)new GenericTextDialog(this._app, App.Localize("@UI_LOBBY_FAILURE"), App.Localize("@UI_LOBBY_FAILURE_MSG"), "dialogGenericMessage"), null);
				else
					this.App.UI.CreateDialog((Dialog)new GenericTextDialog(this._app, App.Localize("@UI_LOBBY_FAILURE"), App.Localize("@UI_LOBBY_SELF_FAILURE_MSG"), "dialogGenericMessage"), null);
			}
		}

		private void ProcessTurnEventForEspionage(TurnEvent e)
		{
			foreach (IntelMissionDesc intelMissionDesc in this.AssetDatabase.IntelMissions.ByTurnEventType(e.EventType))
				intelMissionDesc.OnProcessTurnEvent(this, e);
		}

		public static float GetIntelSuccessRollChance(
		  AssetDatabase assetdb,
		  GameDatabase db,
		  int player,
		  int otherPlayer)
		{
			return Math.Min(25f + 25f * assetdb.GetFaction(db.GetPlayerFactionID(player)).GetSpyingBonusValueForFaction(assetdb.GetFaction(db.GetPlayerFactionID(otherPlayer))) + (float)(0.200000002980232 * ((double)db.GetPlayerInfo(otherPlayer).RateImmigration * 100.0)) + 25f * db.GetStratModifierFloatToApply(StratModifiers.EnemyIntelSuccessModifier, otherPlayer) + (float)(25.0 * ((double)db.GetStratModifierFloatToApply(StratModifiers.IntelSuccessModifier, player) - 1.0)), 100f);
		}

		private static GameSession.IntelMissionResults RollForIntelMission(
		  AssetDatabase assetdb,
		  GameDatabase db,
		  Random r,
		  int player,
		  int otherPlayer)
		{
			float successRollChance = GameSession.GetIntelSuccessRollChance(assetdb, db, player, otherPlayer);
			int num = r.Next(0, 100);
			if (GameSession.ForceIntelMissionCriticalSuccessHack || (double)num <= (double)successRollChance * 0.1)
				return GameSession.IntelMissionResults.CriticalSuccess;
			if ((double)num >= (double)successRollChance * 2.0 || num >= 99)
				return GameSession.IntelMissionResults.CriticalFailure;
			return (double)num >= (double)successRollChance ? GameSession.IntelMissionResults.Failure : GameSession.IntelMissionResults.Success;
		}

		public void DoIntelMission(int targetPlayer)
		{
			PlayerInfo playerInfo = this.GameDatabase.GetPlayerInfo(this.LocalPlayer.ID);
			playerInfo.IntelPoints = Math.Max(playerInfo.IntelPoints - this.AssetDatabase.RequiredIntelPointsForMission, 0);
			this.GameDatabase.UpdatePlayerIntelPoints(this.LocalPlayer.ID, playerInfo.IntelPoints);
			switch (GameSession.RollForIntelMission(this.AssetDatabase, this.GameDatabase, this._random, this.LocalPlayer.ID, targetPlayer))
			{
				case GameSession.IntelMissionResults.CriticalSuccess:
					this.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_INTEL_MISSION_CRITICAL_SUCCESS,
						EventMessage = TurnEventMessage.EM_INTEL_MISSION_CRITICAL_SUCCESS,
						PlayerID = this.LocalPlayer.ID,
						TargetPlayerID = targetPlayer,
						TurnNumber = this.GameDatabase.GetTurnCount(),
						ShowsDialog = true
					});
					this.GameDatabase.InsertGovernmentAction(this.LocalPlayer.ID, App.Localize("@GA_INTEL"), "Intel", 0, 0);
					break;
				case GameSession.IntelMissionResults.Success:
					this.GameDatabase.InsertIntelMission(playerInfo.ID, targetPlayer, this.AssetDatabase.IntelMissions.Choose(this._random).ID, new int?());
					break;
				case GameSession.IntelMissionResults.Failure:
					this.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_INTEL_MISSION_FAILED,
						EventMessage = TurnEventMessage.EM_INTEL_MISSION_FAILED,
						PlayerID = this.LocalPlayer.ID,
						TargetPlayerID = targetPlayer,
						TurnNumber = this.GameDatabase.GetTurnCount()
					});
					break;
				case GameSession.IntelMissionResults.CriticalFailure:
					this.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_INTEL_MISSION_CRITICAL_FAILED,
						EventMessage = TurnEventMessage.EM_INTEL_MISSION_CRITICAL_FAILED,
						PlayerID = this.LocalPlayer.ID,
						TargetPlayerID = targetPlayer,
						TurnNumber = this.GameDatabase.GetTurnCount()
					});
					this.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_INTEL_MISSION_CRITICAL_FAILED_LEAK,
						EventMessage = TurnEventMessage.EM_INTEL_MISSION_CRITICAL_FAILED_LEAK,
						PlayerID = targetPlayer,
						TargetPlayerID = this.LocalPlayer.ID,
						TurnNumber = this.GameDatabase.GetTurnCount()
					});
					break;
			}
		}

		public void DoIntelMissionCriticalSuccess(
		  int targetPlayerId,
		  IEnumerable<IntelMissionDesc> selectedMissions,
		  PlayerInfo blamePlayer)
		{
			foreach (IntelMissionDesc selectedMission in selectedMissions)
				this.GameDatabase.InsertIntelMission(this.LocalPlayer.ID, targetPlayerId, selectedMission.ID, blamePlayer?.ID);
		}

		private static GameSession.IntelMissionResults RollForCounterIntelMission(Random r)
		{
			int num = r.Next(0, 100);
			if (num <= 5)
			{
				GameSession.Warn("CounterIntel Critical Success!! (rolled between 0-5)");
				return GameSession.IntelMissionResults.CriticalSuccess;
			}
			if (num <= 35)
			{
				GameSession.Warn("CounterIntel Success (rolled between 6-35)");
				return GameSession.IntelMissionResults.Success;
			}
			if (num < 95)
				return GameSession.IntelMissionResults.Failure;
			GameSession.Warn("CounterIntel Critical Failure!! (rolled between 95-100)");
			return GameSession.IntelMissionResults.CriticalFailure;
		}

		public void DoCounterIntelMission(
		  int SpyPlayer,
		  int DefendingPlayer,
		  CounterIntelStingMission intelmission)
		{
			GameSession.Warn("Enter DoCounterIntelMission");
			PlayerInfo playerInfo = this.GameDatabase.GetPlayerInfo(DefendingPlayer);
			List<CounterIntelStingMission> list = this.GameDatabase.GetCountIntelStingsForPlayerAgainstPlayer(SpyPlayer, DefendingPlayer).ToList<CounterIntelStingMission>();
			if (!list.Any<CounterIntelStingMission>())
				return;
			int count = list.Count;
			int num1 = 0;
			foreach (CounterIntelStingMission intelStingMission in list)
			{
				GameSession.Warn("Rolling the Dice...");
				GameSession.IntelMissionResults intelMissionResults = GameSession.RollForIntelMission(this.AssetDatabase, this.GameDatabase, this._random, SpyPlayer, DefendingPlayer);
				if (intelMissionResults == GameSession.IntelMissionResults.Failure && count > 1)
				{
					GameSession.Warn("Mission Failure but still more to process... Removing Entry.");
					--count;
					++num1;
					this.GameDatabase.RemoveCounterIntelSting(intelStingMission.ID);
				}
				else
				{
					switch (intelMissionResults)
					{
						case GameSession.IntelMissionResults.CriticalSuccess:
							this.GameDatabase.RemoveCounterIntelSting(intelStingMission.ID);
							GameSession.Warn("Adding Mission Critical Success Event");
							double researchBoostFunds1 = playerInfo.ResearchBoostFunds;
							playerInfo.ResearchBoostFunds = 10000000.0;
							float num2 = this.CalcBoostResearchAccidentOdds(DefendingPlayer) * 100f;
							GameSession.Warn("Rolled odds for research accident: " + (object)num2);
							this.DoBoostResearchAccidentRoll(DefendingPlayer);
							GameDatabase gameDatabase1 = this.GameDatabase;
							TurnEvent ev1 = new TurnEvent();
							ev1.FeasibilityPercent = (int)num2;
							ev1.EventType = TurnEventType.EV_COUNTER_INTEL_CRITICAL_SUCCESS;
							ev1.EventMessage = TurnEventMessage.EM_COUNTER_INTEL_CRITICAL_SUCCESS;
							ev1.PlayerID = SpyPlayer;
							ev1.TargetPlayerID = DefendingPlayer;
							ev1.TurnNumber = this.GameDatabase.GetTurnCount();
							ev1.ShowsDialog = false;
							playerInfo.ResearchBoostFunds = researchBoostFunds1;
							PlayerTechInfo playerTechInfo = this.GameDatabase.GetPlayerTechInfo(DefendingPlayer, this.GameDatabase.GetPlayerResearchingTechID(DefendingPlayer));
							GameSession.Warn("Current Enemy Research Progress: " + (object)playerTechInfo.Progress);
							playerTechInfo.Progress = 0;
							this.GameDatabase.UpdatePlayerTechInfo(playerTechInfo);
							GameSession.Warn("Updated Enemy Research Progress: " + (object)playerTechInfo.Progress);
							gameDatabase1.InsertTurnEvent(ev1);
							this.GameDatabase.InsertGovernmentAction(DefendingPlayer, App.Localize("@GA_COUNTERINTEL"), "CounterIntel", 0, 0);
							return;
						case GameSession.IntelMissionResults.Success:
							this.GameDatabase.RemoveCounterIntelSting(intelStingMission.ID);
							GameSession.Warn("Adding Mission Success Event");
							double researchBoostFunds2 = playerInfo.ResearchBoostFunds;
							playerInfo.ResearchBoostFunds = 100000.0;
							float num3 = this.CalcBoostResearchAccidentOdds(DefendingPlayer) * 100f;
							GameSession.Warn("Rolled odds for research accident: " + (object)num3);
							this.DoBoostResearchAccidentRoll(DefendingPlayer);
							GameDatabase gameDatabase2 = this.GameDatabase;
							TurnEvent turnEvent = new TurnEvent();
							turnEvent.FeasibilityPercent = (int)num3;
							turnEvent.EventType = TurnEventType.EV_COUNTER_INTEL_SUCCESS;
							turnEvent.EventMessage = TurnEventMessage.EM_COUNTER_INTEL_SUCCESS;
							turnEvent.PlayerID = SpyPlayer;
							turnEvent.TargetPlayerID = DefendingPlayer;
							turnEvent.TurnNumber = this.GameDatabase.GetTurnCount();
							turnEvent.ShowsDialog = false;
							playerInfo.ResearchBoostFunds = researchBoostFunds2;
							TurnEvent ev2 = turnEvent;
							gameDatabase2.InsertTurnEvent(ev2);
							this.GameDatabase.InsertGovernmentAction(DefendingPlayer, App.Localize("@GA_COUNTERINTEL"), "CounterIntel", 0, 0);
							return;
						case GameSession.IntelMissionResults.Failure:
							this.GameDatabase.RemoveCounterIntelSting(intelStingMission.ID);
							GameSession.Warn("Adding Mission Failure Event");
							this.GameDatabase.InsertTurnEvent(new TurnEvent()
							{
								EventType = TurnEventType.EV_COUNTER_INTEL_FAILURE,
								EventMessage = TurnEventMessage.EM_COUNTER_INTEL_FAILURE,
								PlayerID = SpyPlayer,
								TargetPlayerID = DefendingPlayer,
								TurnNumber = this.GameDatabase.GetTurnCount(),
								ShowsDialog = false
							});
							return;
						case GameSession.IntelMissionResults.CriticalFailure:
							this.GameDatabase.RemoveCounterIntelSting(intelStingMission.ID);
							GameSession.Warn("Adding Mission Critical Failure Event");
							this.GameDatabase.InsertTurnEvent(new TurnEvent()
							{
								EventType = TurnEventType.EV_COUNTER_INTEL_CRITICAL_FAILURE,
								EventMessage = TurnEventMessage.EM_COUNTER_INTEL_CRITICAL_FAILURE,
								PlayerID = SpyPlayer,
								TargetPlayerID = DefendingPlayer,
								TurnNumber = this.GameDatabase.GetTurnCount(),
								ShowsDialog = false
							});
							this._db.GetDiplomacyInfo(DefendingPlayer, SpyPlayer);
							this._db.ApplyDiplomacyReaction(DefendingPlayer, SpyPlayer, -500, new StratModifiers?(StratModifiers.DiplomacyReactionCounterIntelCritFail), 1);
							return;
						default:
							continue;
					}
				}
			}
		}

		public void DoOperationsMission()
		{
		}

		public Kerberos.Sots.PlayerFramework.Player LocalPlayer { get; private set; }

		public NamesPool NamesPool { get; private set; }

		public bool IsMultiplayer { get; private set; }

		public ScriptModules ScriptModules { get; private set; }

		public bool HomeworldNamed { get; set; }

		public Random Random
		{
			get
			{
				return this._random;
			}
		}

		public string SaveGameName
		{
			get
			{
				return this._saveGameName;
			}
		}

		public AssetDatabase AssetDatabase
		{
			get
			{
				return this._app.AssetDatabase;
			}
		}

		public App App
		{
			get
			{
				return this._app;
			}
		}

		public UICommChannel UI
		{
			get
			{
				return this._app.UI;
			}
		}

		public TurnTimer TurnTimer
		{
			get
			{
				return this._turnTimer;
			}
		}

		public void KillCombatDialog()
		{
			if (this.DialogCombatsPending == null)
				return;
			this.UI.CloseDialog(this.DialogCombatsPending, true);
			this.DialogCombatsPending = (Dialog)null;
		}

		public void ShowCombatDialog(bool visible, GameState state = null)
		{
			if (state == null)
				state = this.App.CurrentState;
			if (visible && (state == this.App.GetGameState<StarMapState>() || state == this.App.GetGameState<CommonCombatState>() || state == this.App.GetGameState<SimCombatState>()) && this.App.Game.GetPendingCombats().Any<PendingCombat>())
			{
				if (state == this.App.GetGameState<CommonCombatState>() || state == this.App.GetGameState<SimCombatState>())
				{
					if (!((CommonCombatState)state).PlayersInCombat.Any<Kerberos.Sots.PlayerFramework.Player>((Func<Kerberos.Sots.PlayerFramework.Player, bool>)(x => x.ID == this.App.LocalPlayer.ID)))
					{
						if (this.DialogCombatsPending != null)
						{
							this.App.UI.SetVisible(this.DialogCombatsPending.ID, true);
						}
						else
						{
							this.DialogCombatsPending = (Dialog)new Kerberos.Sots.UI.DialogCombatsPending(this.App);
							this.App.UI.CreateDialog(this.DialogCombatsPending, null);
						}
					}
					else
					{
						if (this.DialogCombatsPending == null)
							return;
						this.App.UI.SetVisible(this.DialogCombatsPending.ID, false);
						this.App.UI.CloseDialog(this.DialogCombatsPending, true);
						this.DialogCombatsPending = (Dialog)null;
					}
				}
				else
				{
					if (state != this.App.GetGameState<StarMapState>() && state != this.App.GetGameState<CommonCombatState>() && state != this.App.GetGameState<SimCombatState>() || this.App.UI.GetTopDialog() != null)
						return;
					if (this.DialogCombatsPending != null)
					{
						this.App.UI.SetVisible(this.DialogCombatsPending.ID, true);
					}
					else
					{
						this.DialogCombatsPending = (Dialog)new Kerberos.Sots.UI.DialogCombatsPending(this.App);
						this.App.UI.CreateDialog(this.DialogCombatsPending, null);
					}
				}
			}
			else
			{
				if (this.DialogCombatsPending == null)
					return;
				this.App.UI.SetVisible(this.DialogCombatsPending.ID, false);
				this.App.UI.CloseDialog(this.DialogCombatsPending, true);
				this.DialogCombatsPending = (Dialog)null;
			}
		}

		public CombatDataHelper CombatData
		{
			get
			{
				return this._combatData;
			}
		}

		public static int GetNextUniqueCombatID()
		{
			return ++GameSession._muniquecombatID;
		}

		public void SetLocalPlayer(Kerberos.Sots.PlayerFramework.Player player)
		{
			if (player == null)
				return;
			App.Log.Trace("***** CHANGING LOCAL PLAYER FROM " + (object)(this.LocalPlayer != null ? this.LocalPlayer.ID : 0) + " TO " + (object)player.ID, "net");
			if (this.GameDatabase != null)
				this.GameDatabase.SetClientId(player.ID);
			this.LocalPlayer = player;
			this.LocalPlayer.SetAI(false);
			this.App.PostSetLocalPlayer(this.LocalPlayer.ObjectID);
		}

		public void CollectTurnEvents()
		{
			this.TurnEvents = this.GameDatabase.GetTurnEventsByTurnNumber(this.GameDatabase.GetTurnCount() - 1, this.LocalPlayer.ID).OrderByDescending<TurnEvent, int>((Func<TurnEvent, int>)(x =>
		   {
			   if (!x.ShowsDialog)
				   return 0;
			   return !x.IsCombatEvent ? 1 : 2;
		   })).ToList<TurnEvent>();
			for (int index = 0; index < this.TurnEvents.Count<TurnEvent>() - 1; ++index)
			{
				if (!this.TurnEvents[index].ShowsDialog && this.TurnEvents[index + 1].ShowsDialog)
				{
					TurnEvent turnEvent = this.TurnEvents[index + 1];
					this.TurnEvents[index + 1] = this.TurnEvents[index];
					this.TurnEvents[index] = turnEvent;
					index = -1;
				}
			}
			foreach (TurnEvent turnEvent in this.TurnEvents)
				this.ProcessTurnEventForEspionage(turnEvent);
		}

		public static string GetSpecialProjectDescription(SpecialProjectType type)
		{
			return type == SpecialProjectType.Salvage ? "Research Project - Completing this project will allow you to research a tech that you could not research before." : "";
		}

		private ShipSectionCollection GetAvailableShipSectionsCore(Kerberos.Sots.PlayerFramework.Player player)
		{
			return this.PlayerInfos[player].AvailableShipSections;
		}

		public IEnumerable<ShipSectionAsset> GetAvailableShipSections(
		  int playerID,
		  ShipSectionType type,
		  ShipClass shipClass)
		{
			return (IEnumerable<ShipSectionAsset>)this.GetAvailableShipSectionsCore(this.GetPlayerObject(playerID)).GetSectionsByType(shipClass, type);
		}

		public IEnumerable<ShipSectionAsset> GetAvailableShipSections(
		  int playerID)
		{
			return (IEnumerable<ShipSectionAsset>)this.GetAvailableShipSectionsCore(this.GetPlayerObject(playerID)).GetAllSections();
		}

		public double GetPlayerIncomeFromTrade(int playerID)
		{
			double num = 0.0;
			this._incomeFromTrade.TryGetValue(playerID, out num);
			return num;
		}

		public void CheckForNewSections(
		  int player,
		  IEnumerable<PlayerTechInfo> researchedTechs,
		  HashSet<string> researchedGroups)
		{
			string faction = this.GameDatabase.GetFactionName(this.GameDatabase.GetPlayerFactionID(player));
			List<ShipSectionAsset> shipSectionAssetList = new List<ShipSectionAsset>(this.GetAvailableShipSections(player));
			foreach (ShipSectionAsset shipSectionAsset in new List<ShipSectionAsset>(this.AssetDatabase.ShipSections.Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.Faction == faction))))
			{
				if (!shipSectionAssetList.Contains(shipSectionAsset))
				{
					bool flag = true;
					foreach (string requiredTech in shipSectionAsset.RequiredTechs)
					{
						string t = requiredTech;
						if (!researchedTechs.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == t)) && !researchedGroups.Contains(t))
							flag = false;
					}
					if (flag)
						this.GameDatabase.InsertSectionAsset(shipSectionAsset.FileName, player);
				}
			}
		}

		private void SetupStartMapCamera()
		{
			this._crits = new GameObjectSet(this._app);
			this.StarMapCamera = this._crits.Add<OrbitCameraController>();
			this.StarMapCamera.MinDistance = 5f;
			this.StarMapCamera.MaxDistance = 400f;
			this.StarMapCamera.DesiredDistance = 13f;
			this.StarMapCamera.DesiredYaw = MathHelper.DegreesToRadians(45f);
			this.StarMapCamera.DesiredPitch = -MathHelper.DegreesToRadians(25f);
			this.StarMapCamera.SnapToDesiredPosition();
		}

		public GameSession(
		  App app,
		  GameDatabase db,
		  GameSetup gs,
		  string saveGameFileName,
		  NamesPool namesPool,
		  IList<Trigger> activeTriggers,
		  Random rand,
		  GameSession.Flags flags = (GameSession.Flags)0)
		{
			GameSession.SimAITurns = 0;
			string pattern = "(?:\\s*\\((?:" + Regex.Replace(App.Localize("@AUTOSAVE_SUFFIX"), "([\\(\\)])", string.Empty) + "|Precombat)\\).*|\\.sots2save)";

			saveGameFileName = Regex.Replace(Path.GetFileName(saveGameFileName) ?? string.Empty, pattern, string.Empty);
			if (string.IsNullOrEmpty(saveGameFileName))
				throw new ArgumentNullException(nameof(saveGameFileName), "GameSession must be constructed with a valid save game filename. By default this could be gameSetup.GetDefaultSaveGameFileName().");
			this._db = db;
			this._random = new Random();
			
			this._saveGameName = saveGameFileName;
			this._app = app;
			this.NamesPool = namesPool;
			this.IsMultiplayer = gs.IsMultiplayer;
			this._db.QueryLogging(this.IsMultiplayer);
			this._combatData = new CombatDataHelper();
			this._turnTimer = new TurnTimer(app);
			this._turnTimer.StrategicTurnLength = gs.StrategicTurnLength;
			GameSession.Trace("Creating game turn length: " + (object)gs.StrategicTurnLength);
			this.m_Combats = new List<PendingCombat>();
			this.fleetsInCombat = new List<FleetInfo>();
			this._reactions = new List<ReactionInfo>();
			this.m_MCCarryOverData = new MultiCombatCarryOverData();
			this.m_OCSystemToggleData = new OpenCloseSystemToggleData();
			this.m_GameID = 0U;
			if ((flags & GameSession.Flags.ResumingGame) == (GameSession.Flags)0)
				this._db.InsertTurnOne();
			else if ((flags & GameSession.Flags.NoScriptModules) == (GameSession.Flags)0)
				this.ScriptModules = ScriptModules.Resume(this._db);
			foreach (PlayerInfo playerInfo in this._db.GetPlayerInfos())
				this.AddPlayerObject(playerInfo, Kerberos.Sots.PlayerFramework.Player.ClientTypes.User);
			IEnumerable<PlayerInfo> playerInfos = db.GetPlayerInfos();
			int num1 = 1;
			foreach (PlayerSetup player in gs.Players)
			{
				if (player.localPlayer)
				{
					num1 = player.databaseId;
					break;
				}
			}
			db.SetClientId(num1);
			Kerberos.Sots.PlayerFramework.Player playerObject1 = this.GetPlayerObject(num1);
			App.Log.Trace("***** CHANGING LOCAL PLAYER FROM " + (object)(this.LocalPlayer != null ? this.LocalPlayer.ID : 0) + " TO " + (object)playerObject1.ID, "net");
			if (this.GameDatabase != null)
				this.GameDatabase.SetClientId(playerObject1.ID);
			this.LocalPlayer = playerObject1;
			if ((flags & GameSession.Flags.NoNewGameMessage) == (GameSession.Flags)0)
				this.App.PostNewGame(this.LocalPlayer.ObjectID);
			foreach (PlayerInfo playerInfo in playerInfos)
			{
				if (playerInfo.ID != num1)
				{
					Kerberos.Sots.PlayerFramework.Player playerObject2 = this.GetPlayerObject(playerInfo.ID);
					playerObject2.SetAI(!this.App.GameSetup.IsMultiplayer || this.App.Network.IsHosting);
					this.OtherPlayers.Add(playerObject2);
					Thread.Sleep(100);
				}
			}
			if ((flags & GameSession.Flags.ResumingGame) == (GameSession.Flags)0)
			{
				List<int> list = this._db.GetStandardPlayerIDs().ToList<int>();
				if ((flags & GameSession.Flags.NoOrbitalObjects) == (GameSession.Flags)0 && !gs.HasScenarioFile())
				{
					GameSession.AddAdditionalColonies(app, this._db, app.AssetDatabase, list);
					foreach (int num2 in list)
					{
						string factionName = this._db.GetFactionName(this._db.GetPlayerFactionID(num2));
						IEnumerable<int> systems = this._db.GetPlayerColonySystemIDs(num2);
						HomeworldInfo playerHomeworld = this._db.GetPlayerHomeworld(num2);
						Vector3 origin = this._db.GetStarSystemInfo(playerHomeworld.SystemID).Origin;
						int stratModifier = this._db.GetStratModifier<int>(StratModifiers.MaxProvincePlanets, num2, (int)this.AssetDatabase.DefaultStratModifiers[StratModifiers.MaxProvincePlanets]);
						int count = this._db.GetStratModifier<int>(StratModifiers.StartProvincePlanets, num2, (int)this.AssetDatabase.DefaultStratModifiers[StratModifiers.StartProvincePlanets]);
						if (count > stratModifier)
							count = stratModifier;
						string provinceName = namesPool.GetProvinceName(factionName);
						int[] array = this._db.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(starSystemInfo => systems.Any<int>((Func<int, bool>)(colonySystemID => starSystemInfo.ID == colonySystemID)))).OrderBy<StarSystemInfo, float>((Func<StarSystemInfo, float>)(starSystemInfo => (starSystemInfo.Origin - origin).LengthSquared)).Take<StarSystemInfo>(count).Select<StarSystemInfo, int>((Func<StarSystemInfo, int>)(starSystemInfo => starSystemInfo.ID)).ToArray<int>();
						db.InsertProvince(provinceName, num2, (IEnumerable<int>)array, playerHomeworld.SystemID);
					}
				}
				if ((flags & GameSession.Flags.NoScriptModules) == (GameSession.Flags)0)
				{
					this.ScriptModules = ScriptModules.New(this._random, db, app.AssetDatabase, this, namesPool, gs);
					foreach (PlayerInfo playerInfo in this._db.GetPlayerInfos())
					{
						if (this.GetPlayerObject(playerInfo.ID) == null)
						{
							this.AddPlayerObject(playerInfo, Kerberos.Sots.PlayerFramework.Player.ClientTypes.AI);
							Thread.Sleep(100);
						}
					}
				}
			}
			foreach (PlayerInfo playerInfo in this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				if ((flags & GameSession.Flags.ResumingGame) == (GameSession.Flags)0 && (flags & GameSession.Flags.NoGameSetup) == (GameSession.Flags)0)
					this.GameDatabase.AcquireAdditionalInitialTechs(this.AssetDatabase, playerInfo.ID, gs.Players[playerInfo.ID - 1].InitialTechs);
			}
			this.AvailableShipSectionsChanged();
			foreach (int standardPlayerId in this.GameDatabase.GetStandardPlayerIDs())
				this.CheckForNewEquipment(standardPlayerId);
			this.AvailableShipSectionsChanged();
			if ((flags & GameSession.Flags.ResumingGame) == (GameSession.Flags)0 && (flags & GameSession.Flags.NoDefaultFleets) == (GameSession.Flags)0)
			{
				foreach (int standardPlayerId in this.GameDatabase.GetStandardPlayerIDs())
				{
					GameSession.AddDefaultGenerals(this.AssetDatabase, this.GameDatabase, standardPlayerId, namesPool);
					this.AddDefaultStartingFleets(this.GetPlayerObject(standardPlayerId));
					this.AddStartingDeployedShips(this.GameDatabase, standardPlayerId);
				}
			}
			this.SetupStartMapCamera();
			this.PullTradeIncome();
			this._turnTimer.StartTurnTimer();
			if ((flags & GameSession.Flags.ResumingGame) != (GameSession.Flags)0)
			{
				foreach (PlayerInfo playerInfo in this.GameDatabase.GetPlayerInfos())
				{
					if (this.AssetDatabase.GetFaction(this.GameDatabase.GetFactionInfo(playerInfo.FactionID).Name).IsPlayable && playerInfo.ID != this.LocalPlayer.ID)
						this.GetPlayerObject(playerInfo.ID).SetAI(!this.App.GameSetup.IsMultiplayer || this.App.Network.IsHosting);
				}
				PlayerInfo playerInfo1 = this.GameDatabase.GetPlayerInfo(this.LocalPlayer.ID);
				this.UI.Send((object)"SetLocalPlayerColor", (object)playerInfo1.PrimaryColor);
				this.UI.Send((object)"SetLocalSecondaryColor", (object)playerInfo1.SecondaryColor);
				if (gs.IsMultiplayer)
					this.App.Network.DatabaseLoaded();
				else
					gs.SetAllPlayerStatus(NPlayerStatus.PS_TURN);
			}
			else if ((flags & GameSession.Flags.NoGameSetup) == (GameSession.Flags)0)
			{
				this.ActiveTriggers.AddRange((IEnumerable<Trigger>)activeTriggers);
				app.PostNewGame(this.LocalPlayer.ObjectID);
				this.AvailableShipSectionsChanged();
				foreach (PlayerInfo standardPlayerInfo in this.GameDatabase.GetStandardPlayerInfos())
				{
					this.CheckForNewEquipment(standardPlayerInfo.ID);
					foreach (PlayerTechInfo playerTechInfo in this.GameDatabase.GetPlayerTechInfos(standardPlayerInfo.ID).Where<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.State == TechStates.Researched)).ToList<PlayerTechInfo>())
						App.UpdateStratModifiers(this, standardPlayerInfo.ID, playerTechInfo.TechID);
				}
				this.AvailableShipSectionsChanged();
				List<int> list = this.GameDatabase.GetStandardPlayerIDs().ToList<int>();
				foreach (int playerID in list)
				{
					foreach (StationInfo stationInfo in this.GameDatabase.GetStationInfosByPlayerID(playerID).ToList<StationInfo>())
						this.FillUpgradeModules(stationInfo.OrbitalObjectID);
				}
				foreach (int playerID in list)
					StrategicAI.InitializeResearch(rand, this.AssetDatabase, this.GameDatabase, playerID);
				GameSession.UpdatePlayerFleets(this.GameDatabase, this);
				this.UpdateFleetSupply();
			}
			this.UpdateProfileTechs();
			this.UpdatePlayerViews();
			if (this.App.Network != null && this.App.Network.IsHosting)
				this._db.SaveMultiplayerSyncPoint(this.App.CacheDir);
			if ((flags & GameSession.Flags.ResumingGame) != (GameSession.Flags)0)
				GameSession.Trace("========================= RESUMING GAME SESSION AT TURN " + this._db.GetTurnCount().ToString() + " =========================");
			else
				GameSession.Trace("========================= NEW GAME SESSION INITIALIZED =========================");
			if ((flags & GameSession.Flags.NoDefaultFleets) == (GameSession.Flags)0)
				this.SetRequiredDefaultDesigns();
			Kerberos.Sots.StarSystemPathing.StarSystemPathing.LoadAllNodes(this, this._db);
		}

		public void UpdateProfileTechs()
		{
			foreach (PlayerTechInfo playerTechInfo in this.GameDatabase.GetPlayerTechInfos(this.LocalPlayer.ID).Where<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.State == TechStates.Researched)).ToList<PlayerTechInfo>())
			{
				if (!this.App.UserProfile.ResearchedTechs.Contains(playerTechInfo.TechFileID))
					this.App.UserProfile.ResearchedTechs.Add(playerTechInfo.TechFileID);
			}
		}

		public static void AddDefaultGenerals(
		  AssetDatabase assetdb,
		  GameDatabase gamedb,
		  int playerID,
		  NamesPool namesPool)
		{
			int playerMaxAdmirals = GameSession.GetPlayerMaxAdmirals(gamedb, playerID);
			for (int index = 0; index < playerMaxAdmirals; ++index)
				GameSession.GenerateNewAdmiral(assetdb, playerID, gamedb, new AdmiralInfo.TraitType?(), namesPool);
		}

		private static void AddAdditionalColonies(
		  App game,
		  GameDatabase gamedb,
		  AssetDatabase assetdb,
		  List<int> players)
		{
			List<HomeworldInfo> homeworlds = gamedb.GetHomeworlds().ToList<HomeworldInfo>();
			List<StarSystemInfo> list1 = gamedb.GetStarSystemInfos().ToList<StarSystemInfo>();
			list1.RemoveAll((Predicate<StarSystemInfo>)(x => homeworlds.Any<HomeworldInfo>((Func<HomeworldInfo, bool>)(y => y.SystemID == x.ID))));
			Dictionary<int, HomeworldInfo> homeworldMap = new Dictionary<int, HomeworldInfo>();
			Dictionary<int, List<StarSystemInfo>> dictionary = new Dictionary<int, List<StarSystemInfo>>();
			foreach (int player in players)
			{
				int p = player;
				homeworldMap.Add(p, homeworlds.First<HomeworldInfo>((Func<HomeworldInfo, bool>)(x => x.PlayerID == p)));
				dictionary.Add(p, list1.OrderBy<StarSystemInfo, float>((Func<StarSystemInfo, float>)(x => (x.Origin - gamedb.GetStarSystemOrigin(homeworldMap[p].SystemID)).Length)).ToList<StarSystemInfo>());
			}
			Math.Max(game.GameSetup.Players.Max<PlayerSetup>((Func<PlayerSetup, int>)(x => x.InitialColonies)) - 1, 0);
			foreach (int player in players)
			{
				int p = player;
				int num = Math.Max(game.GameSetup.Players.First<PlayerSetup>((Func<PlayerSetup, bool>)(x => x.databaseId == p)).InitialColonies - 1, 0);
				for (int index = 0; index < num; ++index)
				{
					if (game.GameSetup.Players.First<PlayerSetup>((Func<PlayerSetup, bool>)(x => x.databaseId == p)).InitialColonies >= index)
					{
						StarSystemInfo starSystemInfo;
						List<PlanetInfo> list2;
						do
						{
							starSystemInfo = dictionary[p].FirstOrDefault<StarSystemInfo>();
							if (!(starSystemInfo == (StarSystemInfo)null))
							{
								list2 = ((IEnumerable<PlanetInfo>)gamedb.GetStarSystemPlanetInfos(starSystemInfo.ID)).ToList<PlanetInfo>();
								foreach (int key in dictionary.Keys)
									dictionary[key].Remove(starSystemInfo);
							}
							else
								goto label_25;
						}
						while (!list2.Any<PlanetInfo>((Func<PlanetInfo, bool>)(x =>
					   {
						   if (x.Type != "gaseous")
							   return x.Type != "barren";
						   return false;
					   })));
						goto label_17;
					label_25:
						return;
					label_17:
						PlanetInfo planetInfo = list2.First<PlanetInfo>((Func<PlanetInfo, bool>)(x =>
					   {
						   if (x.Type != "gaseous")
							   return x.Type != "barren";
						   return false;
					   }));
						GameSession.MakeIdealColony(gamedb, assetdb, planetInfo.ID, p, IdealColonyTypes.Secondary);
						Faction faction = assetdb.GetFaction(gamedb.GetPlayerFactionID(p));
						if (faction.CanUseNodeLine(new bool?(true)) && GameSession.GetSystemPermenantNodeLine(gamedb, homeworldMap[p].SystemID, starSystemInfo.ID) == null)
							gamedb.InsertNodeLine(homeworldMap[p].SystemID, starSystemInfo.ID, -1);
						else if (faction.CanUseNodeLine(new bool?(false)) && GameSession.GetSystemsNonPermenantNodeLine(gamedb, homeworldMap[p].SystemID, starSystemInfo.ID) == null)
							gamedb.InsertNodeLine(homeworldMap[p].SystemID, starSystemInfo.ID, 1000);
						gamedb.IsSurveyed(p, homeworldMap[p].SystemID);
					}
				}
			}
		}

		public static int MakeIdealColony(
		  GameDatabase gamedb,
		  AssetDatabase assetdb,
		  int planetID,
		  int playerID,
		  IdealColonyTypes idealColonyType)
		{
			PlanetInfo planetInfo = gamedb.GetPlanetInfo(planetID);
			planetInfo.Size = idealColonyType != IdealColonyTypes.Primary ? (float)App.GetSafeRandom().Next(3, 9) : 10f;
			if (gamedb.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, playerID))
				planetInfo.Biosphere = 0;
			double maxImperialPop = Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetMaxImperialPop(gamedb, planetInfo);
			double num = Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetMaxCivilianPop(gamedb, planetInfo) / 4.0;
			double d1 = maxImperialPop + App.GetSafeRandom().NextDouble() * maxImperialPop * (double)assetdb.PopulationNoise - maxImperialPop * (double)assetdb.PopulationNoise / 2.0;
			int colonyID = gamedb.InsertColony(planetID, playerID, Math.Truncate(d1), 0.5f, 1, 1f, true);
			planetInfo.Suitability = gamedb.GetFactionSuitability(gamedb.GetPlayerFactionID(playerID));
			planetInfo.Infrastructure = 1f;
			gamedb.UpdatePlanet(planetInfo);
			ColonyInfo colonyInfo = gamedb.GetColonyInfo(colonyID);
			colonyInfo.CurrentStage = Kerberos.Sots.Data.ColonyStage.Developed;
			Kerberos.Sots.Strategy.InhabitedPlanet.Colony.SetOutputRate(gamedb, assetdb, ref colonyInfo, planetInfo, Kerberos.Sots.Strategy.InhabitedPlanet.Colony.OutputRate.Trade, 0.0f);
			gamedb.UpdateColony(colonyInfo);
			gamedb.InsertExploreRecord(gamedb.GetOrbitalObjectInfo(planetID).StarSystemID, playerID, 1, true, true);
			double d2 = num + App.GetSafeRandom().NextDouble() * num * (double)assetdb.PopulationNoise - num * (double)assetdb.PopulationNoise / 2.0;
			gamedb.InsertColonyFaction(planetID, gamedb.GetPlayerFactionID(playerID), Math.Truncate(d2), 1f, 1);
			return colonyID;
		}

		public void DoModuleBuiltGovernmentShift(
		  StationType stationType,
		  ModuleEnums.StationModuleType moduleType,
		  int playerId)
		{
			string name = ((IEnumerable<StationModules.StationModule>)StationModules.Modules).FirstOrDefault<StationModules.StationModule>((Func<StationModules.StationModule, bool>)(x => x.SMType == moduleType)).Name;
			ModuleEnums.StationModuleType stationModuleType = moduleType;
			if (ModuleEnums.FactionHabitationModules.ContainsValue(stationModuleType))
				stationModuleType = stationModuleType != ModuleEnums.FactionHabitationModules[this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(playerId))] ? ModuleEnums.StationModuleType.AlienHabitation : ModuleEnums.StationModuleType.Habitation;
			else if (ModuleEnums.FactionLargeHabitationModules.ContainsValue(stationModuleType))
				stationModuleType = stationModuleType != ModuleEnums.FactionLargeHabitationModules[this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(playerId))] ? ModuleEnums.StationModuleType.LargeAlienHabitation : ModuleEnums.StationModuleType.LargeHabitation;
			switch (stationType)
			{
				case StationType.NAVAL:
					switch (stationModuleType)
					{
						case ModuleEnums.StationModuleType.Sensor:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Naval_Sensor", 0, 0);
							return;
						case ModuleEnums.StationModuleType.Customs:
							return;
						case ModuleEnums.StationModuleType.Combat:
							return;
						case ModuleEnums.StationModuleType.Repair:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Naval_Repair", 0, 0);
							return;
						case ModuleEnums.StationModuleType.Warehouse:
							return;
						case ModuleEnums.StationModuleType.Command:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Naval_Command", 0, 0);
							return;
						case ModuleEnums.StationModuleType.Dock:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Naval_Dock", 0, 0);
							return;
						default:
							return;
					}
				case StationType.SCIENCE:
					switch (stationModuleType)
					{
						case ModuleEnums.StationModuleType.Habitation:
						case ModuleEnums.StationModuleType.LargeHabitation:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Sci_Hab", 0, 0);
							return;
						case ModuleEnums.StationModuleType.EWPLab:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Sci_EWPLab", 0, 0);
							return;
						case ModuleEnums.StationModuleType.TRPLab:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Sci_TRPLab", 0, 0);
							return;
						case ModuleEnums.StationModuleType.NRGLab:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Sci_NRGLab", 0, 0);
							return;
						case ModuleEnums.StationModuleType.WARLab:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Sci_WARLab", 0, 0);
							return;
						case ModuleEnums.StationModuleType.BALLab:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Sci_BALLab", 0, 0);
							return;
						case ModuleEnums.StationModuleType.BIOLab:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Sci_BIOLab", 0, 0);
							return;
						case ModuleEnums.StationModuleType.INDLab:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Sci_INDLab", 0, 0);
							return;
						case ModuleEnums.StationModuleType.CCCLab:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Sci_CCCLab", 0, 0);
							return;
						case ModuleEnums.StationModuleType.DRVLab:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Sci_DRVLab", 0, 0);
							return;
						case ModuleEnums.StationModuleType.POLLab:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Sci_POLLab", 0, 0);
							return;
						case ModuleEnums.StationModuleType.PSILab:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Sci_PSILab", 0, 0);
							return;
						default:
							return;
					}
				case StationType.CIVILIAN:
					switch (stationModuleType)
					{
						case ModuleEnums.StationModuleType.Warehouse:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Civ_Warehouse", 0, 0);
							return;
						case ModuleEnums.StationModuleType.Command:
							return;
						case ModuleEnums.StationModuleType.Dock:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Civ_Dock", 0, 0);
							return;
						case ModuleEnums.StationModuleType.Terraform:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Civ_Terraform", 0, 0);
							return;
						case ModuleEnums.StationModuleType.AlienHabitation:
						case ModuleEnums.StationModuleType.LargeAlienHabitation:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Civ_AlienHab", 0, 0);
							return;
						case ModuleEnums.StationModuleType.Habitation:
						case ModuleEnums.StationModuleType.LargeHabitation:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Civ_Hab", 0, 0);
							return;
						default:
							return;
					}
				case StationType.DIPLOMATIC:
					switch (stationModuleType)
					{
						case ModuleEnums.StationModuleType.Sensor:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Dip_Sensor", 0, 0);
							return;
						case ModuleEnums.StationModuleType.AlienHabitation:
						case ModuleEnums.StationModuleType.LargeAlienHabitation:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Dip_AlienHab", 0, 0);
							return;
						case ModuleEnums.StationModuleType.Habitation:
						case ModuleEnums.StationModuleType.LargeHabitation:
							this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), (object)name), "ModuleBuilt_Dip_Hab", 0, 0);
							return;
						default:
							return;
					}
			}
		}

		public void FillUpgradeModules(int stationId)
		{
			StationInfo si = this.GameDatabase.GetStationInfo(stationId);
			if (si.DesignInfo.StationType == StationType.MINING || si.DesignInfo.StationType == StationType.DEFENCE)
				return;
			Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>> upgradeRequirements = this.AssetDatabase.GetStationUpgradeRequirements(si.DesignInfo.StationType);
			List<LogicalModuleMount> list1 = ((IEnumerable<LogicalModuleMount>)this.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == si.DesignInfo.DesignSections[0].FilePath)).Modules).ToList<LogicalModuleMount>();
			foreach (KeyValuePair<int, Dictionary<ModuleEnums.StationModuleType, int>> keyValuePair1 in upgradeRequirements)
			{
				if (keyValuePair1.Key > si.DesignInfo.StationLevel)
					break;
				foreach (KeyValuePair<ModuleEnums.StationModuleType, int> keyValuePair2 in keyValuePair1.Value)
				{
					KeyValuePair<ModuleEnums.StationModuleType, int> kvp = keyValuePair2;
					for (int index = 0; index < kvp.Value; ++index)
					{
						if (list1.Any<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => x.ModuleType == kvp.Key.ToString())))
						{
							LogicalModuleMount mount = list1.First<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => x.ModuleType == kvp.Key.ToString()));
							string factionName = this.GameDatabase.GetFactionName(this.GameDatabase.GetPlayerFactionID(si.PlayerID));
							string moduleasset = this.AssetDatabase.GetStationModuleAsset(kvp.Key, factionName);
							int moduleId = this.GameDatabase.GetModuleID(moduleasset, si.PlayerID);
							ModuleEnums.StationModuleType stationModuleType = kvp.Key;
							if (stationModuleType == ModuleEnums.StationModuleType.Habitation)
								stationModuleType = ModuleEnums.FactionHabitationModules[this._db.GetFactionName(this._db.GetPlayerFactionID(si.PlayerID))];
							else if (stationModuleType == ModuleEnums.StationModuleType.AlienHabitation)
							{
								List<PlayerInfo> list2 = this._db.GetStandardPlayerInfos().ToList<PlayerInfo>();
								list2.RemoveAll((Predicate<PlayerInfo>)(x => x.ID == si.PlayerID));
								stationModuleType = ModuleEnums.FactionHabitationModules[this._db.GetFactionName(this._random.Choose<PlayerInfo>((IList<PlayerInfo>)list2).FactionID)];
								if (stationModuleType == ModuleEnums.FactionHabitationModules[this._db.GetFactionName(this._db.GetPlayerFactionID(si.PlayerID))])
									stationModuleType = (ModuleEnums.StationModuleType)Enum.Parse(typeof(ModuleEnums.StationModuleType), stationModuleType.ToString() + "Foreign");
							}
							if (stationModuleType == ModuleEnums.StationModuleType.LargeHabitation)
								stationModuleType = ModuleEnums.FactionLargeHabitationModules[this._db.GetFactionName(this._db.GetPlayerFactionID(si.PlayerID))];
							else if (stationModuleType == ModuleEnums.StationModuleType.LargeAlienHabitation)
							{
								List<PlayerInfo> list2 = this._db.GetStandardPlayerInfos().ToList<PlayerInfo>();
								list2.RemoveAll((Predicate<PlayerInfo>)(x => x.ID == si.PlayerID));
								stationModuleType = ModuleEnums.FactionLargeHabitationModules[this._db.GetFactionName(this._random.Choose<PlayerInfo>((IList<PlayerInfo>)list2).FactionID)];
								if (stationModuleType == ModuleEnums.FactionLargeHabitationModules[this._db.GetFactionName(this._db.GetPlayerFactionID(si.PlayerID))])
									stationModuleType = (ModuleEnums.StationModuleType)Enum.Parse(typeof(ModuleEnums.StationModuleType), stationModuleType.ToString() + "Foreign");
							}
							LogicalModule module = this._app.AssetDatabase.Modules.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == moduleasset));
							SectionInstanceInfo sectionInstanceInfo = this._db.GetShipSectionInstances(si.ShipID).First<SectionInstanceInfo>();
							this.GameDatabase.InsertDesignModule(new DesignModuleInfo()
							{
								DesignSectionInfo = si.DesignInfo.DesignSections[0],
								MountNodeName = mount.NodeName,
								ModuleID = moduleId,
								PsionicAbilities = new List<ModulePsionicInfo>(),
								StationModuleType = new ModuleEnums.StationModuleType?(stationModuleType)
							});
							this.GameDatabase.InsertModuleInstance(mount, module, sectionInstanceInfo.ID);
							list1.Remove(mount);
						}
					}
				}
			}
		}

		public GameDatabase GameDatabase
		{
			get
			{
				return this._db;
			}
		}

		public uint GetGameID()
		{
			return this.m_GameID;
		}

		private static void Trace(string message)
		{
			App.Log.Trace(message, "game");
		}

		private static void Warn(string message)
		{
			App.Log.Warn(message, "game");
		}

		internal Kerberos.Sots.PlayerFramework.Player AddPlayerObject(
		  PlayerInfo pi,
		  Kerberos.Sots.PlayerFramework.Player.ClientTypes clientType)
		{
			Kerberos.Sots.PlayerFramework.Player player = new Kerberos.Sots.PlayerFramework.Player(this._app, this, pi, clientType);
			this.m_Players.Add(player);
			GameSession.Trace("Player " + (object)this.m_Players.Count + " added.");
			return player;
		}

		public Kerberos.Sots.PlayerFramework.Player GetPlayerObject(int playerId)
		{
			return this.m_Players.FirstOrDefault<Kerberos.Sots.PlayerFramework.Player>((Func<Kerberos.Sots.PlayerFramework.Player, bool>)(x => x.ID == playerId));
		}

		public Kerberos.Sots.PlayerFramework.Player GetPlayerObjectByObjectID(int objectId)
		{
			return this.m_Players.FirstOrDefault<Kerberos.Sots.PlayerFramework.Player>((Func<Kerberos.Sots.PlayerFramework.Player, bool>)(x => x.ObjectID == objectId));
		}

		public bool AreCombatsPending()
		{
			return this.m_Combats.Count<PendingCombat>() > 0;
		}

		public bool EndTurn(int playerID)
		{
			return true;
		}

		public void AvailableShipSectionsChanged()
		{
			foreach (Kerberos.Sots.PlayerFramework.Player player in this.m_Players)
			{
				GameSession.SimPlayerInfo simPlayerInfo;
				if (!this.PlayerInfos.TryGetValue(player, out simPlayerInfo))
				{
					simPlayerInfo = new GameSession.SimPlayerInfo();
					this.PlayerInfos.Add(player, simPlayerInfo);
				}
				string[] array = this._db.GetGetAllPlayerSectionIds(player.ID).ToArray<string>();
				simPlayerInfo.AvailableShipSections = new ShipSectionCollection(this._db, this._app.AssetDatabase, player, array);
			}
		}

		public void CheckForNewEquipment(int player)
		{
			HashSet<string> researchedTechGroups = this.GameDatabase.GetResearchedTechGroups(player);
			List<PlayerTechInfo> playerTechInfoList = new List<PlayerTechInfo>(this.GameDatabase.GetPlayerTechInfos(player).Where<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.State == TechStates.Researched)));
			this.CheckForNewSections(player, (IEnumerable<PlayerTechInfo>)playerTechInfoList, researchedTechGroups);
			this.CheckForNewModules(player, (IEnumerable<PlayerTechInfo>)playerTechInfoList, researchedTechGroups);
			this.CheckForNewWeapons(player, (IEnumerable<PlayerTechInfo>)playerTechInfoList, researchedTechGroups);
		}

		public void CheckForNewModules(
		  int player,
		  IEnumerable<PlayerTechInfo> researchedTechs,
		  HashSet<string> researchedGroups)
		{
			string factionName = this.GameDatabase.GetFactionName(this.GameDatabase.GetPlayerFactionID(player));
			List<LogicalModule> logicalModuleList1 = new List<LogicalModule>(this.AssetDatabase.Modules.Where<LogicalModule>((Func<LogicalModule, bool>)(x => x.Faction == factionName)));
			List<LogicalModule> logicalModuleList2 = new List<LogicalModule>(this.GameDatabase.GetAvailableModules(this.AssetDatabase, player));
			foreach (LogicalModule module in logicalModuleList1)
			{
				if (!logicalModuleList2.Contains(module))
				{
					bool flag = true;
					foreach (Kerberos.Sots.Data.ShipFramework.Tech tech in module.Techs)
					{
						Kerberos.Sots.Data.ShipFramework.Tech t = tech;
						if (!researchedTechs.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == t.Name)) && !researchedGroups.Contains(t.Name))
							flag = false;
					}
					if (flag)
						this.GameDatabase.InsertModule(module, player);
				}
			}
		}

		public void CheckForNewWeapons(
		  int player,
		  IEnumerable<PlayerTechInfo> researchedTechs,
		  HashSet<string> researchedGroups)
		{
			string factionName = this._db.GetFactionName(this._db.GetPlayerFactionID(player));
			List<LogicalWeapon> logicalWeaponList = new List<LogicalWeapon>(this.GameDatabase.GetAvailableWeapons(this.AssetDatabase, player));
			foreach (LogicalWeapon weapon in this.AssetDatabase.Weapons)
			{
				if (weapon.IsVisible && !logicalWeaponList.Contains(weapon))
				{
					bool flag = true;
					foreach (Kerberos.Sots.Data.WeaponFramework.Tech requiredTech in weapon.RequiredTechs)
					{
						Kerberos.Sots.Data.WeaponFramework.Tech t = requiredTech;
						if (!researchedTechs.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == t.Name)) && !researchedGroups.Contains(t.Name))
							flag = false;
					}
					if (flag && (((IEnumerable<string>)weapon.CompatibleFactions).Count<string>() == 0 || ((IEnumerable<string>)weapon.CompatibleFactions).Contains<string>(factionName)))
						this.GameDatabase.InsertWeapon(weapon, player);
				}
			}
		}

		public void ProcessEndTurn()
		{
			this.TurnTimer.StopTurnTimer();
			this.App.HotKeyManager.SetEnabled(false);
			if (this.m_Combats.Count<PendingCombat>() > 0)
				return;
			if (this.App.Network.IsHosting || !this.App.GameSetup.IsMultiplayer)
			{
				StrategicAI.UpdateInfo updateInfo = new StrategicAI.UpdateInfo(this.GameDatabase);
				foreach (Kerberos.Sots.PlayerFramework.Player player in this.m_Players)
				{
					if (player.IsAI() && player.Faction.IsPlayable)
					{
						if (ScriptHost.AllowConsole)
						{
							Stopwatch stopwatch = Stopwatch.StartNew();
							player.GetAI().Update(updateInfo);
							App.Log.Trace("End AI Player " + (object)player.ID + " (" + player.Faction.Name + ") Turn: " + (object)stopwatch.ElapsedMilliseconds + "ms", "ai");
						}
						else
							player.GetAI().Update(updateInfo);
					}
				}
			}
			if (GameSession.SimAITurns == 0 && this.App.GameSettings.AutoSave)
				this.Autosave("(Precombat)");
			this.State = SimState.SS_COMBAT;
			if (this.App.GameSetup.IsMultiplayer)
				return;
			this.ProcessMidTurn();
		}

		public void ProcessMidTurn()
		{
			if (ScriptHost.AllowConsole)
			{
				Stopwatch stopwatch = Stopwatch.StartNew();
				this.Phase2_FleetMovement();
				App.Log.Trace("End Fleet Movement: " + (object)stopwatch.ElapsedMilliseconds + "ms", "ai");
				stopwatch.Restart();
				this.Phase3_ReactionMovement();
				App.Log.Trace("End Reaction Movement: " + (object)stopwatch.ElapsedMilliseconds + "ms", "ai");
			}
			else
			{
				this.Phase2_FleetMovement();
				this.Phase3_ReactionMovement();
			}
		}

		private void PushTradeIncome(Dictionary<int, double> tradeIncome)
		{
			foreach (int playerId in this._db.GetPlayerIDs())
			{
				if (tradeIncome.ContainsKey(playerId))
					this._db.UpdatePlayerCurrentTradeIncome(playerId, tradeIncome[playerId]);
				else
					this._db.UpdatePlayerCurrentTradeIncome(playerId, 0.0);
			}
		}

		private void PullTradeIncome()
		{
			if (this._incomeFromTrade == null)
				this._incomeFromTrade = new Dictionary<int, double>();
			else
				this._incomeFromTrade.Clear();
			foreach (int playerId in this._db.GetPlayerIDs())
				this._incomeFromTrade[playerId] = this._db.GetPlayerCurrentTradeIncome(playerId);
		}

		public void UpdateOpenCloseSystemToggle()
		{
			foreach (OpenCloseSystemInfo toggledSystem in this.m_OCSystemToggleData.ToggledSystems)
			{
				if (toggledSystem.IsOpen)
				{
					GameSession.ApplyMoralEvent(this._app, MoralEvent.ME_SYSTEM_OPEN, toggledSystem.PlayerID, new int?(), new int?(), new int?(toggledSystem.SystemID));
					this._db.InsertGovernmentAction(toggledSystem.PlayerID, App.Localize("@GA_DECLARESYSTEMOPEN"), "DeclareSystemOpen", 0, 0);
				}
				else
				{
					GameSession.ApplyMoralEvent(this._app, MoralEvent.ME_SYSTEM_CLOSE, toggledSystem.PlayerID, new int?(), new int?(), new int?(toggledSystem.SystemID));
					this._db.InsertGovernmentAction(toggledSystem.PlayerID, App.Localize("@GA_DECLARESYSTEMCLOSED"), "DeclareSystemClosed", 0, 0);
				}
			}
		}

		public void NextTurn()
		{
			this.m_MCCarryOverData.ClearData();
			this.m_OCSystemToggleData.ClearData();
			this.App.GameDatabase.ClearTempMoveOrders();
			this.App.Game.ShowCombatDialog(false, (GameState)null);
			this.App.Game.KillCombatDialog();
			this.State = SimState.SS_PLAYER;
			this.App.GetGameState<StarMapState>().TurnEnded();
			if (this.App.Network.IsHosting || !this.App.GameSetup.IsMultiplayer)
			{
				this.Phase5_Results();
				this.Phase6_EndOfTurnBookKeeping();
			}
			this.PullTradeIncome();
			this.UpdatePlayerViews();
			this._db.CullQueryHistory(2);
			if (this.App.Network.IsHosting || !this.App.GameSetup.IsMultiplayer)
				this._db.IncrementTurnCount();
			this._app.GameSetup.SavePlayerSlots(this._db);
			this.CollectTurnEvents();
			if (this.App.Network.IsHosting || !this.App.GameSetup.IsMultiplayer)
			{
				List<int> intList = new List<int>();
				foreach (int playerID in this.GameDatabase.GetStandardPlayerIDs().ToList<int>())
				{
					foreach (TurnEvent turnEvent in this.GameDatabase.GetTurnEventsByTurnNumber(this.GameDatabase.GetTurnCount() - 1, playerID).Where<TurnEvent>((Func<TurnEvent, bool>)(x => x.EventType == TurnEventType.EV_SUULKA_LEAVES)).ToList<TurnEvent>())
					{
						int num = !string.IsNullOrEmpty(turnEvent.Param1) ? int.Parse(turnEvent.Param1) : 0;
						if (num != 0 && !intList.Contains(num))
							intList.Add(num);
					}
				}
				foreach (int suulkaID in intList)
					this._app.GameDatabase.ReturnSuulka(this, suulkaID);
			}
			if (this.App.CurrentState == this.App.GetGameState<StarMapState>())
				this.App.GetGameState<StarMapState>().TurnStarted();
			this._turnTimer.StartTurnTimer();
			this.App.PostRequestSpeech(string.Format("Newturn_{0}", (object)this.App.LocalPlayer.Faction.Name), 50, 120, 0.0f);
			this.Autosave(App.Localize("@AUTOSAVE_SUFFIX"));
			if (this.App.Network != null && this.App.Network.IsHosting)
				this._db.SaveMultiplayerSyncPoint(this.App.CacheDir);
			this.SetRequiredDefaultDesigns();
			this.App.HotKeyManager.SetEnabled(true);
			Kerberos.Sots.StarSystemPathing.StarSystemPathing.LoadAllNodes(this, this._db);
			GameSession.Trace("========================= TURN " + this._db.GetTurnCount().ToString() + " =========================");
		}

		public void Autosave(string suffix)
		{
			//string pattern = "\\s*\\((?:" + Regex.Replace(App.Localize("@AUTOSAVE_SUFFIX"), "([\\(\\)])", string.Empty) + "|Precombat)\\).*";
			//string str = Regex.Replace(Path.GetFileNameWithoutExtension(this._saveGameName), pattern, string.Empty);
			string str = Path.GetFileNameWithoutExtension(this._saveGameName);
			str = str + " " + suffix;
			this.Save(Path.Combine(this.App.SaveDir, str + ".sots2save"));
		}

		public void Save(string filename)
		{
			this._db.Save(filename);
			if (ScriptHost.AllowConsole)
				this._db.Save(Path.Combine(Path.GetDirectoryName(filename), string.Format("T{0}P{1}-{2}.db", (object)this._db.GetTurnCount(), (object)this.LocalPlayer.ID, (object)Path.GetFileName(filename))));
			if (this._app.UserProfile == null)
				return;
			this._app.UserProfile.LastGamePlayed = filename;
			this._app.UserProfile.SaveProfile();
		}

		private void UpdatePlayerViews()
		{
			List<StarSystemInfo> list = this._db.GetStarSystemInfos().ToList<StarSystemInfo>();
			Dictionary<int, List<ShipInfo>> cachedFleetShips = new Dictionary<int, List<ShipInfo>>();
			foreach (ShipInfo shipInfo in this._db.GetShipInfos(false))
			{
				List<ShipInfo> shipInfoList;
				if (!cachedFleetShips.TryGetValue(shipInfo.FleetID, out shipInfoList))
				{
					shipInfoList = new List<ShipInfo>();
					cachedFleetShips[shipInfo.FleetID] = shipInfoList;
				}
				shipInfoList.Add(shipInfo);
			}
			for (int index = 0; index < this.m_Players.Count; ++index)
			{
				Kerberos.Sots.PlayerFramework.Player player = this.m_Players[index];
				this._db.PurgeOwnedColonyIntel(player.ID);
				foreach (StarSystemInfo ssi in list)
				{
					if (StarMap.IsInRange(this.GameDatabase, player.ID, ssi, cachedFleetShips))
						this._db.UpdatePlayerViewWithStarSystem(player.ID, ssi.ID);
				}
			}
			IEnumerable<int> standardPlayerIds = this._db.GetStandardPlayerIDs();
			foreach (int num1 in standardPlayerIds)
			{
				foreach (int num2 in standardPlayerIds)
				{
					if (num1 != num2 && this._db.GetDiplomacyStateBetweenPlayers(num1, num2) == DiplomacyState.ALLIED)
						this._db.ShareSensorData(num1, num2);
				}
			}
		}

		private void UpdateIsEncounteredStates()
		{
			for (int index1 = 0; index1 < this.m_Players.Count; ++index1)
			{
				PlayerInfo playerInfo1 = this._db.GetPlayerInfo(this.m_Players[index1].ID);
				if (playerInfo1.isStandardPlayer || playerInfo1.includeInDiplomacy)
				{
					Kerberos.Sots.PlayerFramework.Player player1 = this.m_Players[index1];
					for (int index2 = 0; index2 < this.m_Players.Count; ++index2)
					{
						if (index1 != index2)
						{
							PlayerInfo playerInfo2 = this._db.GetPlayerInfo(this.m_Players[index2].ID);
							if (playerInfo2.isStandardPlayer || playerInfo2.includeInDiplomacy)
							{
								Kerberos.Sots.PlayerFramework.Player player2 = this.m_Players[index2];
								DiplomacyInfo diplomacyInfo = this.GameDatabase.GetDiplomacyInfo(player2.ID, player1.ID);
								if (diplomacyInfo != null && !diplomacyInfo.isEncountered)
								{
									bool flag = false;
									if (!flag)
									{
										foreach (int systemId in this._db.GetPlayerColonySystemIDs(player2.ID).ToList<int>())
										{
											StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(systemId);
											if (StarMap.IsInRange(this.GameDatabase, player1.ID, starSystemInfo, (Dictionary<int, List<ShipInfo>>)null))
											{
												flag = true;
												break;
											}
										}
									}
									if (!flag)
									{
										foreach (FleetInfo fleetInfo in this._db.GetFleetInfosByPlayerID(player2.ID, FleetType.FL_ALL_COMBAT | FleetType.FL_NORMAL).ToList<FleetInfo>())
										{
											FleetLocation fleetLocation = this._db.GetFleetLocation(fleetInfo.ID, false);
											if (fleetLocation != null && StarMap.IsInRange(this.GameDatabase, player1.ID, fleetLocation.Coords, 1f, (Dictionary<int, List<ShipInfo>>)null))
											{
												flag = true;
												break;
											}
										}
									}
									if (flag && diplomacyInfo != null)
									{
										diplomacyInfo.isEncountered = true;
										this.GameDatabase.UpdateDiplomacyInfo(diplomacyInfo);
										this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
										{
											EventType = TurnEventType.EV_EMPIRE_ENCOUNTERED,
											EventMessage = TurnEventMessage.EM_EMPIRE_ENCOUNTERED,
											TurnNumber = this._db.GetTurnCount(),
											PlayerID = diplomacyInfo.PlayerID,
											TargetPlayerID = diplomacyInfo.TowardsPlayerID
										});
									}
								}
							}
						}
					}
				}
			}
		}

		public void CheckGovernmentTradeChanges(TradeResultsTable trt)
		{
			TradeResultsTable tradeResultsTable = this._db.GetTradeResultsTable();
			Dictionary<int, int> dictionary1 = new Dictionary<int, int>();
			foreach (KeyValuePair<int, TradeNode> tradeNode in tradeResultsTable.TradeNodes)
			{
				int? systemOwningPlayer = this._db.GetSystemOwningPlayer(tradeNode.Key);
				if (systemOwningPlayer.HasValue)
				{
					if (dictionary1.ContainsKey(systemOwningPlayer.Value))
					{
						Dictionary<int, int> dictionary2;
						int index;
						(dictionary2 = dictionary1)[index = systemOwningPlayer.Value] = dictionary2[index] + tradeNode.Value.Produced;
					}
					else
						dictionary1.Add(systemOwningPlayer.Value, tradeNode.Value.Produced);
				}
			}
			Dictionary<int, int> dictionary3 = new Dictionary<int, int>();
			foreach (KeyValuePair<int, TradeNode> tradeNode in trt.TradeNodes)
			{
				int? systemOwningPlayer = this._db.GetSystemOwningPlayer(tradeNode.Key);
				if (systemOwningPlayer.HasValue)
				{
					if (dictionary3.ContainsKey(systemOwningPlayer.Value))
					{
						Dictionary<int, int> dictionary2;
						int index;
						(dictionary2 = dictionary3)[index = systemOwningPlayer.Value] = dictionary2[index] + tradeNode.Value.Produced;
					}
					else
						dictionary3.Add(systemOwningPlayer.Value, tradeNode.Value.Produced);
				}
			}
			foreach (KeyValuePair<int, int> keyValuePair in dictionary3)
			{
				int num = 0;
				if (dictionary1.ContainsKey(keyValuePair.Key))
				{
					num = dictionary1[keyValuePair.Key];
					dictionary1.Remove(keyValuePair.Key);
				}
				if (num > keyValuePair.Value)
					this._app.GameDatabase.InsertGovernmentAction(keyValuePair.Key, App.Localize("@GA_TRADEDECREASED"), "", 0, 2 * (keyValuePair.Value - num));
				else if (num < keyValuePair.Value)
					this._app.GameDatabase.InsertGovernmentAction(keyValuePair.Key, App.Localize("@GA_TRADEINCREASED"), "", keyValuePair.Value - num, -(keyValuePair.Value - num));
			}
			foreach (KeyValuePair<int, int> keyValuePair in dictionary1)
				this._app.GameDatabase.InsertGovernmentAction(keyValuePair.Key, App.Localize("@GA_TRADEDECREASED"), "", 0, 2 * keyValuePair.Value);
		}

		private void DoAutoOptionActions()
		{
			foreach (PlayerInfo playerInfo1 in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				PlayerInfo playerInfo = playerInfo1;
				if (playerInfo.isStandardPlayer && !this.GetPlayerObject(playerInfo.ID).IsAI())
				{
					foreach (StarSystemInfo starSystemInfo in this._db.GetStarSystemInfos())
					{
						if (playerInfo.AutoPlaceDefenseAssets)
						{
							if (this._db.GetColonyInfosForSystem(starSystemInfo.ID).Any<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == playerInfo.ID)) || this._db.GetNavalStationForSystemAndPlayer(starSystemInfo.ID, playerInfo.ID) != null)
							{
								int num1 = this._db.GetSystemDefensePoints(starSystemInfo.ID, playerInfo.ID) - this._db.GetAllocatedSystemDefensePoints(starSystemInfo, playerInfo.ID);
								if (num1 > 0)
								{
									FleetInfo defenseFleetInfo = this._db.GetDefenseFleetInfo(starSystemInfo.ID, playerInfo.ID);
									if (defenseFleetInfo != null)
									{
										int num2 = num1;
										foreach (ShipInfo shipInfo in this._db.GetShipInfoByFleetID(defenseFleetInfo.ID, false).Where<ShipInfo>((Func<ShipInfo, bool>)(x => !x.IsPlaced())).ToList<ShipInfo>())
										{
											int defenseAssetCpCost = this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(shipInfo.DesignInfo);
											if (defenseAssetCpCost <= num2 && Kerberos.Sots.StarFleet.StarFleet.AutoPlaceDefenseAsset(this._app, shipInfo.ID, starSystemInfo.ID))
												num2 -= defenseAssetCpCost;
										}
									}
									else
										continue;
								}
								else
									continue;
							}
							else
								continue;
						}
						if (playerInfo.AutoRepairShips)
							this.RepairFleetsAtSystem(starSystemInfo.ID, playerInfo.ID);
					}
				}
			}
		}

		public void Phase6_EndOfTurnBookKeeping()
		{
			foreach (MissionInfo missionInfo in this._db.GetMissionInfos().ToList<MissionInfo>())
			{
				foreach (WaypointInfo waypointInfo in this._db.GetWaypointsByMissionID(missionInfo.ID).Where<WaypointInfo>((Func<WaypointInfo, bool>)(x => x.Type == WaypointType.Intercepted)).ToList<WaypointInfo>())
					this._db.RemoveWaypoint(waypointInfo.ID);
			}
			this.UpdateIsEncounteredStates();
			this.UpdatePlayerEmpireHistoryData();
			this._db.RemoveAllMoraleEventsHistory();
			int turnCount = this._db.GetTurnCount();
			foreach (PlayerInfo playerInfo in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(playerInfo.ID);
				IEnumerable<PlayerTechInfo> playerTechInfos = this._db.GetPlayerTechInfos(playerObject.ID);
				playerObject._techPointsAtStartOfTurn = 0;
				foreach (PlayerTechInfo playerTechInfo in playerTechInfos)
					playerObject._techPointsAtStartOfTurn += playerTechInfo.Progress;
				this.ValidateHomeWorld(playerInfo.ID);
				this.HandleIntelMissionsForPlayer(playerInfo.ID);
				this.HandleCounterIntelMissions(playerInfo.ID);
				foreach (StellarInfo starSystemInfo in this._db.GetStarSystemInfos())
					this.DetectPiracyFleets(starSystemInfo.ID, playerInfo.ID);
			}
			TradeResultsTable tradeResult;
			this._incomeFromTrade = this.Trade(out tradeResult);
			this.CheckGovernmentTradeChanges(tradeResult);
			this.PushTradeIncome(this._incomeFromTrade);
			this._db.SyncTradeNodes(tradeResult);
			this.CollectTaxes();
			this.App.GameDatabase.RemoveAllColonyHistory();
			foreach (ColonyInfo colonyInfo1 in this._db.GetColonyInfos().ToList<ColonyInfo>())
			{
				ColonyInfo colony = colonyInfo1;
				OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(colony.OrbitalObjectID);
				this._db.InsertExploreRecord(orbitalObjectInfo.StarSystemID, colony.PlayerID, turnCount, true, true);
				List<PlagueInfo> list1 = this._db.GetPlagueInfoByColony(colonyInfo1.ID).ToList<PlagueInfo>();
				bool achievedSuperWorld = false;
				PlanetInfo planetInfo = this._db.GetPlanetInfo(colony.OrbitalObjectID);
				int colonySupportCost = (int)Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetColonySupportCost(this.App.AssetDatabase, this._db, colony);
				Faction faction1 = this.App.AssetDatabase.GetFaction(this.App.GameDatabase.GetPlayerFactionID(colony.PlayerID));
				ColonyHistoryData history = new ColonyHistoryData()
				{
					colonyID = colonyInfo1.ID,
					resources = planetInfo.Resources,
					biosphere = planetInfo.Biosphere,
					infrastructure = planetInfo.Infrastructure,
					hazard = this.App.GameDatabase.GetPlanetHazardRating(colonyInfo1.PlayerID, planetInfo.ID, false),
					life_support_cost = colonySupportCost,
					income = (int)Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetTaxRevenue(this.App, colony) - colonySupportCost,
					econ_rating = colony.EconomyRating,
					industrial_output = (int)Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetIndustrialOutput(this, colony, planetInfo),
					civ_pop = this.App.GameDatabase.GetCivilianPopulation(colony.OrbitalObjectID, faction1.ID, faction1.HasSlaves()),
					imp_pop = colony.ImperialPop,
					slave_pop = this.App.GameDatabase.GetSlavePopulation(colony.OrbitalObjectID, faction1.ID)
				};
				history.civ_pop -= history.slave_pop;
				this.App.GameDatabase.InsertColonyHistory(history);
				foreach (ColonyFactionInfo colonyFactionInfo in this.App.GameDatabase.GetCivilianPopulations(colony.OrbitalObjectID).ToList<ColonyFactionInfo>())
				{
					this.App.GameDatabase.RemoveColonyMoraleHistory(colony.ID);
					this.App.GameDatabase.InsertColonyMoraleHistory(new ColonyFactionMoraleHistory()
					{
						colonyID = colony.ID,
						factionid = colonyFactionInfo.FactionID,
						morale = colonyFactionInfo.Morale,
						population = colonyFactionInfo.CivilianPop
					});
				}
				int resources = planetInfo.Resources;
				double num1 = 0.0;
				int factionId = this._db.GetPlayerFactionID(colonyInfo1.PlayerID);
				if (this.AssetDatabase.GetFaction(factionId).HasSlaves())
				{
					foreach (ColonyFactionInfo faction2 in colony.Factions)
					{
						if (faction2.FactionID != factionId)
							num1 += faction2.CivilianPop;
					}
				}
				double totalPopulation = this._db.GetTotalPopulation(colony);
				List<ColonyFactionInfo> civPopulation;
				Kerberos.Sots.Strategy.InhabitedPlanet.Colony.MaintainColony(this, ref colony, ref planetInfo, ref list1, 0.0, 0.0, (FleetInfo)null, out civPopulation, out achievedSuperWorld, false);
				double num2 = this._db.GetTotalPopulation(colony) - totalPopulation;
				if (num2 < -50000.0)
					colony.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.PopulationReductionOver50K, 1);
				else if (num2 > 30000.0)
					colony.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.PopulationGrowthOver30K, 1);
				if (planetInfo.Resources == 0 && resources != 0)
					this.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_PLANET_NO_RESOURCES,
						EventMessage = TurnEventMessage.EM_PLANET_NO_RESOURCES,
						PlayerID = colony.PlayerID,
						SystemID = orbitalObjectInfo.StarSystemID,
						OrbitalID = orbitalObjectInfo.ID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				double num3 = civPopulation.Sum<ColonyFactionInfo>((Func<ColonyFactionInfo, double>)(x => x.CivilianPop));
				double num4 = (double)colony.CivilianWeight * Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetMaxCivilianPop(this._db, planetInfo);
				if (colony.CurrentStage == Kerberos.Sots.Data.ColonyStage.GemWorld)
					num4 *= (double)this.AssetDatabase.GemWorldCivMaxBonus;
				if (num4 < num3)
				{
					if (num4 + 100.0 < num3)
					{
						if ((double)colony.CivilianWeight == 1.0)
							GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_OVERPOPULATION_PLANET, colony.PlayerID, new int?(colony.ID), new int?(), new int?());
						else
							GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_OVERPOPULATION_PLAYER, colony.PlayerID, new int?(colony.ID), new int?(), new int?());
					}
					double num5 = num4 / num3;
					foreach (ColonyFactionInfo civPop in civPopulation)
					{
						civPop.CivilianPop *= num5;
						this._db.UpdateCivilianPopulation(civPop);
					}
				}
				if (colony.ReplicantsOn)
					GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_REPLICANTS_ON, colony.PlayerID, new int?(colony.ID), new int?(), new int?());
				if ((double)colony.InfraRate == 0.0)
					Kerberos.Sots.Strategy.InhabitedPlanet.Colony.SetOutputRate(this._db, this.AssetDatabase, ref colony, planetInfo, Kerberos.Sots.Strategy.InhabitedPlanet.Colony.OutputRate.Infra, 0.0f);
				if ((double)colony.OverdevelopRate == 0.0)
					Kerberos.Sots.Strategy.InhabitedPlanet.Colony.SetOutputRate(this._db, this.AssetDatabase, ref colony, planetInfo, Kerberos.Sots.Strategy.InhabitedPlanet.Colony.OutputRate.OverDev, 0.0f);
				if ((double)colony.TerraRate == 0.0)
					Kerberos.Sots.Strategy.InhabitedPlanet.Colony.SetOutputRate(this._db, this.AssetDatabase, ref colony, planetInfo, Kerberos.Sots.Strategy.InhabitedPlanet.Colony.OutputRate.Terra, 0.0f);
				if ((double)colony.OverharvestRate > 0.0)
				{
					switch (colony.CurrentStage)
					{
						case Kerberos.Sots.Data.ColonyStage.Colony:
							this._app.GameDatabase.InsertGovernmentAction(colony.PlayerID, App.Localize("@GA_UNDERDEVELOPEDCOLONY_OVERHARVEST"), "UnderDevelopedColony_OverHarvest", 0, 0);
							break;
						case Kerberos.Sots.Data.ColonyStage.Developed:
							this._app.GameDatabase.InsertGovernmentAction(colony.PlayerID, App.Localize("@GA_DEVELOPEDCOLONY_OVERHARVEST"), "DevelopedColony_OverHarvest", 0, 0);
							break;
						case Kerberos.Sots.Data.ColonyStage.GemWorld:
						case Kerberos.Sots.Data.ColonyStage.ForgeWorld:
							this._app.GameDatabase.InsertGovernmentAction(colony.PlayerID, App.Localize("@GA_OVERDEVELOPEDCOLONY_OVERHARVEST"), "OverDevelopedColony_OverHarvest", 0, 0);
							break;
					}
				}
				if (colony.ImperialPop <= 0.0)
					this._db.RemoveColonyOnPlanet(planetInfo.ID);
				else
					this._db.UpdateColony(colony);
				UISliderNotchInfo settingInfoForColony = this.App.GameDatabase.GetSliderNotchSettingInfoForColony(colony.PlayerID, colony.ID, UISlidertype.TradeSlider);
				if (settingInfoForColony != null)
				{
					colony = this.App.GameDatabase.GetColonyInfo(colony.ID);
					List<double> exportsForColony = this.App.Game.GetTradeRatesForWholeExportsForColony(colony.ID);
					if (exportsForColony.Count - 1 >= (int)settingInfoForColony.SliderValue)
					{
						double num5 = (double)(int)Math.Ceiling(exportsForColony[(int)settingInfoForColony.SliderValue] * 100.0) / 100.0;
						Kerberos.Sots.Strategy.InhabitedPlanet.Colony.SetOutputRate(this.App.GameDatabase, this.App.AssetDatabase, ref colony, planetInfo, Kerberos.Sots.Strategy.InhabitedPlanet.Colony.OutputRate.Trade, (float)num5);
						this.App.GameDatabase.UpdateColony(colony);
					}
					else if (exportsForColony.Count != 0)
					{
						double num5 = (double)(int)Math.Ceiling(exportsForColony.Last<double>() * 100.0) / 100.0;
						Kerberos.Sots.Strategy.InhabitedPlanet.Colony.SetOutputRate(this.App.GameDatabase, this.App.AssetDatabase, ref colony, planetInfo, Kerberos.Sots.Strategy.InhabitedPlanet.Colony.OutputRate.Trade, (float)num5);
						this.App.GameDatabase.UpdateColony(colony);
					}
				}
				foreach (PlagueInfo pi in list1)
				{
					bool flag = pi.InfectedPopulationCivilian <= 0.0 && pi.InfectedPopulationImperial <= 0.0;
					if (!flag && pi.PlagueType == WeaponEnums.PlagueType.ASSIM)
					{
						ColonyInfo colonyInfo2 = this._db.GetColonyInfo(pi.ColonyId);
						if (colonyInfo2 != null && colonyInfo2.PlayerID == pi.LaunchingPlayer)
							flag = true;
					}
					if (flag)
					{
						this._db.RemovePlagueInfo(pi.ID);
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_PLAGUE_ENDED,
							EventMessage = TurnEventMessage.EM_PLAGUE_ENDED,
							PlagueType = pi.PlagueType,
							ColonyID = pi.ColonyId,
							PlayerID = colony.PlayerID,
							TurnNumber = this.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
					else
						this._db.UpdatePlagueInfo(pi);
				}
				this._db.UpdatePlanet(planetInfo);
				if (achievedSuperWorld)
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_SUPERWORLD_COMPLETE,
						EventMessage = TurnEventMessage.EM_SUPERWORLD_COMPLETE,
						PlayerID = colony.PlayerID,
						SystemID = orbitalObjectInfo.StarSystemID,
						OrbitalID = orbitalObjectInfo.ID,
						ColonyID = colony.ID,
						ShowsDialog = true,
						TurnNumber = this.App.GameDatabase.GetTurnCount()
					});
				List<ColonyFactionInfo> list2 = this._db.GetCivilianPopulations(planetInfo.ID).ToList<ColonyFactionInfo>();
				foreach (ColonyFactionInfo colonyFactionInfo in list2)
				{
					ColonyFactionInfo cfi = colonyFactionInfo;
					if (!civPopulation.Any<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID == cfi.FactionID)))
						this._db.RemoveCivilianPopulation(cfi.OrbitalObjectID, cfi.FactionID);
				}
				foreach (ColonyFactionInfo colonyFactionInfo in civPopulation)
				{
					ColonyFactionInfo cfi = colonyFactionInfo;
					if (list2.Any<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID == cfi.FactionID)))
						this._db.UpdateCivilianPopulation(cfi);
					else
						this._db.InsertColonyFaction(cfi.OrbitalObjectID, cfi.FactionID, cfi.CivilianPop, cfi.CivPopWeight, cfi.TurnEstablished);
				}
				factionId = this._db.GetPlayerFactionID(colonyInfo1.PlayerID);
				if (this.AssetDatabase.GetFaction(factionId).HasSlaves())
				{
					civPopulation.RemoveAll((Predicate<ColonyFactionInfo>)(x => x.FactionID == factionId));
					if (civPopulation.Sum<ColonyFactionInfo>((Func<ColonyFactionInfo, double>)(x => x.CivilianPop)) == 0.0 && num1 > 0.0)
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_SLAVES_DEAD,
							EventMessage = TurnEventMessage.EM_SLAVES_DEAD,
							PlayerID = colonyInfo1.PlayerID,
							SystemID = colonyInfo1.CachedStarSystemID,
							ColonyID = colonyInfo1.ID,
							TurnNumber = this.App.GameDatabase.GetTurnCount()
						});
				}
			}
			this.CollectDiplomacyPoints();
			this.UpdateMorale();
			this.AvailableShipSectionsChanged();
			foreach (PlayerInfo playerInfo in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
				this.CheckForNewEquipment(playerInfo.ID);
			this.AvailableShipSectionsChanged();
			this.UpdateConsumableShipStats();
			this.UpdateRepairPoints();
			this.HandleGives();
			foreach (PlayerInfo p in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				if (this.AssetDatabase.GetFaction(p.FactionID).Name == "loa" && p.Savings < 0.0)
				{
					p.SetResearchRate(0.0f);
					this.GameDatabase.UpdatePlayerSliders(this, p);
				}
				else
				{
					foreach (StationInfo station in this._db.GetStationInfosByPlayerID(p.ID))
					{
						foreach (DesignModuleInfo module in station.DesignInfo.DesignSections[0].Modules)
						{
							if (this.GameDatabase.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, p.ID))
							{
								ModuleEnums.StationModuleType? stationModuleType = module.StationModuleType;
								if ((stationModuleType.GetValueOrDefault() != ModuleEnums.StationModuleType.Terraform ? 0 : (stationModuleType.HasValue ? 1 : 0)) != 0)
								{
									float global = this.AssetDatabase.GetGlobal<float>("SterileModuleBiosphereBonus");
									foreach (PlanetInfo systemPlanetInfo in this._db.GetStarSystemPlanetInfos(station.OrbitalObjectInfo.StarSystemID))
									{
										if (systemPlanetInfo.Biosphere != 0)
										{
											systemPlanetInfo.Biosphere -= (int)global;
											this._db.UpdatePlanet(systemPlanetInfo);
										}
									}
								}
							}
						}
						List<DesignModuleInfo> list1 = this._db.GetQueuedStationModules(station.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
						if (list1.Count > 0)
						{
							StationModuleQueue.UpdateStationMapsForFaction(this.GetPlayerObject(p.ID).Faction.Name);
							bool flag = this.StationIsUpgradable(station);
							List<LogicalModuleMount> list2 = ((IEnumerable<LogicalModuleMount>)station.DesignInfo.DesignSections[0].ShipSectionAsset.Modules).ToList<LogicalModuleMount>();
							DesignModuleInfo dmi = list1.First<DesignModuleInfo>();
							LogicalModuleMount mount = list2.FirstOrDefault<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => x.NodeName == dmi.MountNodeName));
							while (mount == null && list1.Count > 1)
							{
								list1.RemoveAt(0);
								this._db.RemoveQueuedStationModule(dmi.ID);
								mount = list2.FirstOrDefault<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => x.NodeName == dmi.MountNodeName));
								dmi = list1.First<DesignModuleInfo>();
							}
							if (mount == null)
							{
								this._db.RemoveQueuedStationModule(dmi.ID);
							}
							else
							{
								dmi.DesignSectionInfo = station.DesignInfo.DesignSections[0];
								LogicalModule module = this.AssetDatabase.Modules.First<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == this._db.GetModuleAsset(dmi.ModuleID)));
								SectionInstanceInfo sectionInstanceInfo = this._app.GameDatabase.GetShipSectionInstances(station.ShipID).First<SectionInstanceInfo>();
								this.DoModuleBuiltGovernmentShift(station.DesignInfo.StationType, dmi.StationModuleType.Value, station.PlayerID);
								p.Savings -= (double)module.SavingsCost;
								this._db.UpdatePlayerSavings(p.ID, p.Savings);
								this._db.InsertDesignModule(dmi);
								this._db.InsertModuleInstance(mount, module, sectionInstanceInfo.ID);
								this._db.RemoveQueuedStationModule(dmi.ID);
								if (!flag && this.StationIsUpgradable(station))
									this._db.InsertTurnEvent(new TurnEvent()
									{
										EventType = TurnEventType.EV_STATION_UPGRADABLE,
										EventMessage = TurnEventMessage.EM_STATION_UPGRADABLE,
										PlayerID = station.PlayerID,
										OrbitalID = station.OrbitalObjectID,
										SystemID = this.App.GameDatabase.GetOrbitalObjectInfo(station.OrbitalObjectID).StarSystemID,
										TurnNumber = this.App.GameDatabase.GetTurnCount(),
										ShowsDialog = false
									});
							}
						}
					}
					foreach (int SystemId in (IEnumerable<int>)this._db.GetPlayerColonySystemIDs(p.ID).ToList<int>())
					{
						this.BuildAtSystem(SystemId, p.ID);
						this.RetrofitAtSystem(SystemId, p.ID);
					}
				}
			}
			foreach (StationRetrofitOrderInfo retrofitOrderInfo in this.App.GameDatabase.GetStationRetrofitOrders().ToList<StationRetrofitOrderInfo>())
			{
				ShipInfo ship = this.App.GameDatabase.GetShipInfo(retrofitOrderInfo.ShipID, true);
				DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(retrofitOrderInfo.DesignID);
				PlayerInfo playerInfo1 = this.GameDatabase.GetPlayerInfo(designInfo.PlayerID);
				if (!(this.AssetDatabase.GetFaction(playerInfo1.FactionID).Name == "loa") || playerInfo1.Savings >= 0.0)
				{
					StationInfo station = this.App.GameDatabase.GetStationInfosByPlayerID(designInfo.PlayerID).FirstOrDefault<StationInfo>((Func<StationInfo, bool>)(x => x.ShipID == ship.ID));
					if (ship != null && designInfo != null && (station != null && station.DesignInfo != null) && (ship.DesignInfo != null && ship.DesignInfo.StationLevel == designInfo.StationLevel && (station.DesignInfo.StationLevel == designInfo.StationLevel && station.DesignInfo.DesignSections[0].Modules.Count == designInfo.DesignSections[0].Modules.Count)))
					{
						int stationRetrofitCost = Kerberos.Sots.StarFleet.StarFleet.CalculateStationRetrofitCost(this.App, ship.DesignInfo, designInfo);
						PlayerInfo playerInfo2 = this.App.GameDatabase.GetPlayerInfo(designInfo.PlayerID);
						playerInfo2.Savings -= (double)stationRetrofitCost;
						this.App.GameDatabase.UpdatePlayerSavings(playerInfo2.ID, playerInfo2.Savings);
						this.App.GameDatabase.UpdateShipDesign(ship.ID, designInfo.ID, new int?());
						station.DesignInfo = designInfo;
						this.App.GameDatabase.UpdateStation(station);
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_RETROFIT_COMPLETE_STATION,
							EventMessage = TurnEventMessage.EM_RETROFIT_COMPLETE_STATION,
							PlayerID = station.PlayerID,
							ShipID = station.ShipID,
							TurnNumber = this.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
					this.App.GameDatabase.RemoveStationRetrofitOrder(retrofitOrderInfo.ID);
				}
			}
			this.CheckAdmiralRetirement();
			foreach (PlayerInfo playerInfo in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				if (this.GetNumAdmirals(playerInfo.ID) < GameSession.GetPlayerMaxAdmirals(this._db, playerInfo.ID))
				{
					List<ColonyInfo> list = this._db.GetPlayerColoniesByPlayerId(playerInfo.ID).ToList<ColonyInfo>();
					Random random = new Random();
					foreach (ColonyInfo colonyInfo in list)
					{
						if (random.CoinToss(20))
						{
							int newAdmiral = GameSession.GenerateNewAdmiral(this.AssetDatabase, playerInfo.ID, this.GameDatabase, new AdmiralInfo.TraitType?(), this.NamesPool);
							this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
							{
								EventType = TurnEventType.EV_ADMIRAL_PROMOTED,
								EventMessage = TurnEventMessage.EM_ADMIRAL_PROMOTED,
								PlayerID = playerInfo.ID,
								TurnNumber = this.App.GameDatabase.GetTurnCount(),
								ShowsDialog = false,
								AdmiralID = newAdmiral
							});
							GameSession.Trace("New Admiral generated for player " + (object)playerInfo.ID);
							break;
						}
					}
				}
			}
			GameSession.UpdatePlayerFleets(this.GameDatabase, this);
			this.UpdateTreaties(this.GameDatabase);
			this.UpdateRequests(this.GameDatabase);
			this.UpdateDemands(this.GameDatabase);
			this.CheckAIRebellions(this.GameDatabase);
			this.CheckTriggers(TurnStage.STG_STAGE1);
			this.AvailableShipSectionsChanged();
			IEnumerable<Kerberos.Sots.Strategy.CombatData> combats = this._combatData.GetCombats(this.App.GameDatabase.GetTurnCount());
			foreach (PlayerInfo playerInfo in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				PlayerInfo player = playerInfo;
				GovernmentInfo governmentInfo = this._db.GetGovernmentInfo(player.ID);
				this._db.UpdateGovernmentInfo(governmentInfo);
				if (this._db.GetGovernmentInfo(player.ID).CurrentType != governmentInfo.CurrentType)
					this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_GOVERNMENT_TYPE_CHANGED,
						EventMessage = TurnEventMessage.EM_GOVERNMENT_TYPE_CHANGED,
						PlayerID = this.App.LocalPlayer.ID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				foreach (Kerberos.Sots.Strategy.CombatData combatData in combats.Where<Kerberos.Sots.Strategy.CombatData>((Func<Kerberos.Sots.Strategy.CombatData, bool>)(x =>
			   {
				   if (x.GetPlayer(player.ID) != null)
					   return x.GetPlayer(player.ID).VictoryStatus != GameSession.VictoryStatus.Loss;
				   return false;
			   })))
				{
					IEnumerable<PlayerCombatData> vsPlayers = combatData.GetPlayers().Where<PlayerCombatData>((Func<PlayerCombatData, bool>)(x => x.PlayerID != player.ID));
					IEnumerable<ShipData> myShips = combatData.GetPlayer(player.ID).ShipData.Where<ShipData>((Func<ShipData, bool>)(x => !x.destroyed));
					Kerberos.Sots.StarFleet.StarFleet.CheckSystemCanSupportResidentFleets(this.App, combatData.SystemID, player.ID);
					if (this._db.GetStratModifier<bool>(StratModifiers.ComparativeAnalysys, player.ID))
					{
						foreach (PlayerCombatData playerCombatData in vsPlayers)
						{
							PlayerCombatData vsPlayer = playerCombatData;
							if (vsPlayer.PlayerID != player.ID && this.App.GameDatabase.GetStandardPlayerIDs().Any<int>((Func<int, bool>)(x => x == vsPlayer.PlayerID)))
							{
								foreach (ShipData shipData in vsPlayer.ShipData.ToList<ShipData>())
									this._app.GameDatabase.InsertDesignEncountered(player.ID, shipData.designID);
							}
						}
					}
					this.AttemptToSalvageCombat(player, myShips, vsPlayers);
				}
			}
			this.DoAutoOptionActions();
			this.UpdateColonyEconomyRating();
			this.CheckDestroyedIndys();
			this.CheckMoraleShipBonuses();
			this.CheckStationSlots();
			this.ClampMoraleValues();
			this.CheckEndGame();
			if (!ScriptHost.AllowConsole)
				return;
			App.Log.Trace("Diplomacy State Info Turn: " + (object)this.App.GameDatabase.GetTurnCount(), "data");
			List<PlayerInfo> list3 = this._db.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo playerInfo1 in list3)
			{
				foreach (PlayerInfo playerInfo2 in list3)
				{
					if (playerInfo2 != playerInfo1)
					{
						DiplomacyInfo diplomacyInfo = this._db.GetDiplomacyInfo(playerInfo2.ID, playerInfo1.ID);
						if (diplomacyInfo != null)
						{
							if (!diplomacyInfo.isEncountered)
							{
								App.Log.Trace("P " + (object)playerInfo1.ID + " and P " + (object)playerInfo2.ID + " have not yet encountered", "data");
							}
							else
							{
								App.Log.Trace("P " + (object)playerInfo1.ID + " and P " + (object)playerInfo2.ID + " Relations[" + (object)diplomacyInfo.Relations + "] : Mood[" + diplomacyInfo.GetDiplomaticMood().ToString() + "] : State[" + diplomacyInfo.State.ToString() + "]", "data");
								List<DiplomacyReactionHistoryEntryInfo> list1 = this._db.GetDiplomacyReactionHistory(playerInfo1.ID, playerInfo2.ID, this.App.GameDatabase.GetTurnCount(), 1).ToList<DiplomacyReactionHistoryEntryInfo>();
								if (list1.Count > 0)
								{
									App.Log.Trace("Reaction Updates This Turn Between Players", "data");
									foreach (DiplomacyReactionHistoryEntryInfo historyEntryInfo in list1)
										App.Log.Trace("Reaction Type [" + historyEntryInfo.Reaction.ToString() + "] difference [" + (object)historyEntryInfo.Difference + "]", "data");
								}
							}
						}
					}
				}
			}
		}

		private bool AttemptToSalvageCombat(
		  PlayerInfo player,
		  IEnumerable<ShipData> myShips,
		  IEnumerable<PlayerCombatData> vsPlayers)
		{
			Faction faction = this.App.AssetDatabase.GetFaction(player.FactionID);
			List<ShipData> source = new List<ShipData>();
			if (faction.Name == "zuul")
			{
				source = myShips.ToList<ShipData>();
			}
			else
			{
				foreach (ShipData myShip in myShips)
				{
					DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(myShip.designID);
					if (designInfo != null && Kerberos.Sots.StarFleet.StarFleet.GetIsSalavageCapable(this.App, designInfo))
						source.Add(myShip);
				}
			}
			if (!source.Any<ShipData>())
				return false;
			bool AlreadySalavaging = this.SalavageColonies(player, (IEnumerable<ShipData>)source, vsPlayers, false);
			return this.SalvageShips(player, (IEnumerable<ShipData>)source, vsPlayers, AlreadySalavaging);
		}

		private bool SalvageShips(
		  PlayerInfo player,
		  IEnumerable<ShipData> SalavageShips,
		  IEnumerable<PlayerCombatData> vsPlayers,
		  bool AlreadySalavaging)
		{
			List<string> stringList = new List<string>();
			IEnumerable<SpecialProjectInfo> source = this._app.GameDatabase.GetSpecialProjectInfosByPlayerID(player.ID, true).Where<SpecialProjectInfo>((Func<SpecialProjectInfo, bool>)(x => x.Type == SpecialProjectType.Salvage));
			bool flag1 = AlreadySalavaging;
			if (SalavageShips.Count<ShipData>() > 0)
			{
				foreach (PlayerCombatData vsPlayer in vsPlayers)
				{
					foreach (ShipData shipData in vsPlayer.ShipData.Where<ShipData>((Func<ShipData, bool>)(x => x.destroyed)).ToList<ShipData>())
					{
						foreach (DesignSectionInfo designSection in this.App.GameDatabase.GetDesignInfo(shipData.designID).DesignSections)
						{
							foreach (WeaponBankInfo weaponBank in designSection.WeaponBanks)
							{
								if (weaponBank.WeaponID.HasValue)
								{
									string wepAsset = this.App.GameDatabase.GetWeaponAsset(weaponBank.WeaponID.Value);
									LogicalWeapon wep = this.App.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == wepAsset));
									if (wep != null)
									{
										foreach (PlayerTechInfo playerTechInfo in this.App.GameDatabase.GetPlayerTechInfos(player.ID).Where<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x =>
									   {
										   if (x.State != TechStates.Researched)
											   return x.State != TechStates.Locked;
										   return false;
									   })).ToList<PlayerTechInfo>().Where<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => ((IEnumerable<Kerberos.Sots.Data.WeaponFramework.Tech>)wep.RequiredTechs).Any<Kerberos.Sots.Data.WeaponFramework.Tech>((Func<Kerberos.Sots.Data.WeaponFramework.Tech, bool>)(y => y.Name == x.TechFileID)))).ToList<PlayerTechInfo>())
										{
											PlayerTechInfo pTech = playerTechInfo;
											if ((double)(pTech.Progress / pTech.ResearchCost) < 0.5 && !stringList.Contains(pTech.TechFileID))
											{
												pTech.Progress += new Random().NextInclusive(this.App.AssetDatabase.AccumulatedKnowledgeWeaponPerBattleMin, this.App.AssetDatabase.AccumulatedKnowledgeWeaponPerBattleMax);
												this.App.GameDatabase.UpdatePlayerTechInfo(pTech);
												stringList.Add(pTech.TechFileID);
												bool flag2 = false;
												if (!flag1 && !source.Any<SpecialProjectInfo>((Func<SpecialProjectInfo, bool>)(x => x.TechID == pTech.TechID)))
												{
													foreach (ShipData salavageShip in SalavageShips)
													{
														DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(salavageShip.designID);
														if (designInfo != null)
														{
															int salvageChance = Kerberos.Sots.StarFleet.StarFleet.GetSalvageChance(this._app, designInfo);
															int num = salvageChance + (int)((double)salvageChance * (double)this.GameDatabase.GetStratModifierFloatToApply(StratModifiers.SalvageModifier, player.ID));
															if (App.GetSafeRandom().NextInclusive(1, 100) <= Kerberos.Sots.StarFleet.StarFleet.GetSalvageChance(this._app, designInfo))
															{
																flag2 = true;
																break;
															}
														}
													}
													if (flag2 && pTech.State == TechStates.PendingFeasibility || (pTech.State == TechStates.LowFeasibility || pTech.State == TechStates.HighFeasibility) || (pTech.State == TechStates.Branch || pTech.State != TechStates.Core))
													{
														int num = GameSession.InsertNewSalvageProject(this.App, player.ID, pTech.TechID);
														this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
														{
															EventType = TurnEventType.EV_NEW_SALVAGE_PROJECT,
															EventMessage = TurnEventMessage.EM_NEW_SALVAGE_PROJECT,
															PlayerID = player.ID,
															TechID = pTech.TechID,
															TurnNumber = this._app.GameDatabase.GetTurnCount(),
															SpecialProjectID = num,
															ShowsDialog = false
														});
														flag1 = true;
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
			return flag1;
		}

		private bool SalavageColonies(
		  PlayerInfo player,
		  IEnumerable<ShipData> SalavageShips,
		  IEnumerable<PlayerCombatData> vsPlayers,
		  bool AlreadySalavaging)
		{
			// ISSUE: object of a compiler-generated type is created
			// ISSUE: variable of a compiler-generated type
			// GameSession.<>c__DisplayClassc3 cDisplayClassc3_1 = new GameSession.<>c__DisplayClassc3();
			// ISSUE: reference to a compiler-generated field
			// cDisplayClassc3_1.player = player;
			// ISSUE: reference to a compiler-generated field
			// IEnumerable<SpecialProjectInfo> source = this._app.GameDatabase.GetSpecialProjectInfosByPlayerID(cDisplayClassc3_1.player.ID, true).Where<SpecialProjectInfo>((Func<SpecialProjectInfo, bool>) (x => x.Type == SpecialProjectType.Salvage));
			IEnumerable<SpecialProjectInfo> source = this._app.GameDatabase.GetSpecialProjectInfosByPlayerID(player.ID, true).Where<SpecialProjectInfo>((Func<SpecialProjectInfo, bool>)(x => x.Type == SpecialProjectType.Salvage));
			bool flag1 = AlreadySalavaging;
			foreach (PlayerCombatData vsPlayer in vsPlayers)
			{
				if (!flag1)
				{
					if (vsPlayer.PlanetData.Count != 0)
					{
						// ISSUE: reference to a compiler-generated method
						Faction faction = this._app.AssetDatabase.Factions.First<Faction>(x => player.FactionID == x.ID);
						// ISSUE: reference to a compiler-generated field
						List<PlayerTechInfo> list1 = this.App.GameDatabase.GetPlayerTechInfos(player.ID).Where<PlayerTechInfo>(x => x.State != TechStates.Researched && x.State != TechStates.Locked).ToList<PlayerTechInfo>();
						List<PlayerTechInfo> vstechs = this.App.GameDatabase.GetPlayerTechInfos(vsPlayer.PlayerID).Where<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.State == TechStates.Researched)).ToList<PlayerTechInfo>();
						List<PlayerTechInfo> list2 = list1.Where<PlayerTechInfo>(x => vstechs.Any<PlayerTechInfo>(y => y.TechFileID == x.TechFileID)).ToList<PlayerTechInfo>();
						foreach (PlanetData planetData in vsPlayer.PlanetData)
						{
							if (!flag1)
							{
								if (this._app.GameDatabase.GetColonyInfoForPlanet(planetData.orbitalObjectID) == null)
								{
									foreach (PlayerTechInfo playerTechInfo in list2)
									{
										PlayerTechInfo pTech = playerTechInfo;
										if (faction.CanFactionObtainTechBranch(pTech.TechFileID.Substring(0, 3)))
										{
											bool flag2 = false;
											if (!flag1 && !source.Any<SpecialProjectInfo>(x => x.TechID == pTech.TechID))
											{
												foreach (ShipData salavageShip in SalavageShips)
												{
													DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(salavageShip.designID);
													if (designInfo != null)
													{
														int salvageChance = Kerberos.Sots.StarFleet.StarFleet.GetSalvageChance(this._app, designInfo);
														// ISSUE: reference to a compiler-generated field
														int num = salvageChance + (int)((double)salvageChance * (double)this.GameDatabase.GetStratModifierFloatToApply(StratModifiers.SalvageModifier, player.ID));
														if (App.GetSafeRandom().NextInclusive(1, 100) <= Kerberos.Sots.StarFleet.StarFleet.GetSalvageChance(this._app, designInfo))
														{
															flag2 = true;
															break;
														}
													}
												}
												if (flag2 && pTech.State == TechStates.PendingFeasibility || (pTech.State == TechStates.LowFeasibility || pTech.State == TechStates.HighFeasibility) || (pTech.State == TechStates.Branch || pTech.State != TechStates.Core))
												{
													// ISSUE: reference to a compiler-generated field
													int num = GameSession.InsertNewSalvageProject(this.App, player.ID, pTech.TechID);
													// ISSUE: reference to a compiler-generated field
													this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
													{
														EventType = TurnEventType.EV_NEW_SALVAGE_PROJECT,
														EventMessage = TurnEventMessage.EM_NEW_SALVAGE_PROJECT,
														PlayerID = player.ID,
														TechID = pTech.TechID,
														TurnNumber = this._app.GameDatabase.GetTurnCount(),
														SpecialProjectID = num,
														ShowsDialog = false
													});
													flag1 = true;
												}
											}
										}
									}
								}
							}
							else
								break;
						}
					}
				}
				else
					break;
			}
			return flag1;
		}

		private void UpdateColonyEconomyRating()
		{
			foreach (ColonyInfo colony in this._db.GetColonyInfos().ToList<ColonyInfo>())
			{
				int num1 = this._db.GetStarSystemInfo(colony.CachedStarSystemID).IsOpen ? 1 : 0;
				PlayerInfo playerInfo = this._db.GetPlayerInfo(colony.PlayerID);
				if (playerInfo.Savings <= -500000.0)
					colony.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.EmpireInDebt500K, 1);
				if (playerInfo.Savings <= -1000000.0)
					colony.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.EmpireInDebt1Mil, 1);
				if (playerInfo.Savings <= -2000000.0)
					colony.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.EmpireInDebt2Mil, 1);
				if (playerInfo.Savings >= 500000.0)
					colony.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.Savings500K, 1);
				if (playerInfo.Savings >= 1000000.0)
					colony.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.Savings1Mil, 1);
				if (playerInfo.Savings >= 3000000.0)
					colony.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.Savings3Mil, 1);
				List<FleetInfo> list = this._db.GetFleetInfoBySystemID(colony.CachedStarSystemID, FleetType.FL_NORMAL).ToList<FleetInfo>();
				bool flag1 = true;
				bool flag2 = false;
				StarSystemInfo system = this._db.GetStarSystemInfo(colony.CachedStarSystemID);
				if (system.ProvinceID.HasValue)
				{
					flag1 = false;
					foreach (StarSystemInfo starSystemInfo in this._db.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
				   {
					   int? provinceId1 = x.ProvinceID;
					   int? provinceId2 = system.ProvinceID;
					   if (provinceId1.GetValueOrDefault() == provinceId2.GetValueOrDefault())
						   return provinceId1.HasValue == provinceId2.HasValue;
					   return false;
				   })).ToList<StarSystemInfo>())
						list.AddRange((IEnumerable<FleetInfo>)this._db.GetFleetInfoBySystemID(starSystemInfo.ID, FleetType.FL_NORMAL).ToList<FleetInfo>());
				}
				foreach (FleetInfo fleetInfo in list)
				{
					if (colony.PlayerID != fleetInfo.PlayerID && this._db.GetDiplomacyStateBetweenPlayers(colony.PlayerID, fleetInfo.PlayerID) == DiplomacyState.WAR)
					{
						if (!flag2 && fleetInfo.SystemID == colony.CachedStarSystemID)
						{
							flag2 = true;
							colony.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.EnemyInSystem, 1);
						}
						if (!flag1)
						{
							flag1 = true;
							colony.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.EnemyInProvince, 1);
						}
						if (flag2)
						{
							if (flag1)
								break;
						}
					}
				}
				if (((IEnumerable<ColonyFactionInfo>)colony.Factions).Any<ColonyFactionInfo>())
				{
					int num2 = (int)((IEnumerable<ColonyFactionInfo>)colony.Factions).Average<ColonyFactionInfo>((Func<ColonyFactionInfo, int>)(x => x.Morale));
					if (num2 < 20)
						colony.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.MoraleUnder20, 1);
					if (num2 >= 80)
						colony.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.MoraleMin80, 1);
				}
				if (this._db.GetMissionsBySystemDest(colony.CachedStarSystemID).ToList<MissionInfo>().Any<MissionInfo>((Func<MissionInfo, bool>)(x =>
			   {
				   if (x.Type != MissionType.CONSTRUCT_STN)
					   return x.Type == MissionType.UPGRADE_STN;
				   return true;
			   })))
					colony.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.ConstructionOrUpgradeInSystem, 1);
				colony.EconomyRating = Math.Min(Math.Max(colony.EconomyRating, 0.0f), 1f);
				this._db.UpdateColony(colony);
			}
		}

		private void ValidateHomeWorld(int playerid)
		{
			HomeworldInfo hw = this._app.GameDatabase.GetPlayerHomeworld(playerid);
			ColonyInfo colonyInfo1 = (ColonyInfo)null;
			if (hw != null)
				colonyInfo1 = this._app.GameDatabase.GetColonyInfo(hw.ColonyID);
			if (hw != null && colonyInfo1 != null && colonyInfo1.PlayerID == playerid)
				return;
			if (hw == null)
				hw = new HomeworldInfo() { PlayerID = playerid };
			List<ColonyInfo> list = this._app.GameDatabase.GetColonyInfos().Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == playerid)).ToList<ColonyInfo>();
			ColonyInfo ci1 = (ColonyInfo)null;
			foreach (ColonyInfo ci2 in list)
			{
				if (ci1 == null)
					ci1 = ci2;
				else if (this._app.GameDatabase.GetTotalPopulation(ci1) < this._app.GameDatabase.GetTotalPopulation(ci2))
					ci1 = ci2;
			}
			if (ci1 == null)
				return;
			int? nullable = new int?();
			foreach (StarSystemInfo starSystemInfo in this._app.GameDatabase.GetVisibleStarSystemInfos(playerid).ToList<StarSystemInfo>())
			{
				foreach (ColonyInfo colonyInfo2 in this._app.GameDatabase.GetColonyInfosForSystem(starSystemInfo.ID))
				{
					if (colonyInfo2.ID == ci1.ID)
					{
						nullable = new int?(starSystemInfo.ID);
						break;
					}
				}
				if (nullable.HasValue)
					break;
			}
			if (!nullable.HasValue)
				return;
			hw.SystemID = nullable.Value;
			hw.ColonyID = ci1.ID;
			this._app.GameDatabase.UpdateHomeworldInfo(hw);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_HOMEWORLD_REESTABLISHED,
				EventMessage = TurnEventMessage.EM_HOMEWORLD_REESTABLISHED,
				PlayerID = hw.PlayerID,
				ColonyID = hw.ColonyID,
				SystemID = hw.SystemID,
				TurnNumber = this.App.GameDatabase.GetTurnCount()
			});
		}

		private void UpdatePlayerEmpireHistoryData()
		{
			foreach (PlayerInfo playerInfo in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				EmpireHistoryData history = new EmpireHistoryData();
				Budget budget = Budget.GenerateBudget(this.App.Game, playerInfo, (IEnumerable<DesignInfo>)null, BudgetProjection.Pessimistic);
				history.playerID = playerInfo.ID;
				history.colonies = this.App.GameDatabase.GetNumColonies(playerInfo.ID);
				history.provinces = this.App.GameDatabase.GetNumProvinces(playerInfo.ID);
				history.bases = this.App.GameDatabase.GetStationInfosByPlayerID(playerInfo.ID).Count<StationInfo>();
				history.fleets = this.App.GameDatabase.GetFleetInfosByPlayerID(playerInfo.ID, FleetType.FL_NORMAL).Count<FleetInfo>();
				history.ships = this.App.GameDatabase.GetNumShips(playerInfo.ID);
				history.empire_pop = this.App.GameDatabase.GetEmpirePopulation(playerInfo.ID);
				history.empire_economy = this.App.GameDatabase.GetEmpireEconomy(playerInfo.ID);
				history.empire_biosphere = this.App.GameDatabase.GetEmpireBiosphere(playerInfo.ID);
				history.empire_trade = budget.TradeRevenue;
				history.empire_morale = !this.App.GameDatabase.GetEmpireMorale(playerInfo.ID).HasValue ? 0 : this.App.GameDatabase.GetEmpireMorale(playerInfo.ID).Value;
				history.savings = playerInfo.Savings;
				history.psi_potential = playerInfo.PsionicPotential;
				this.App.GameDatabase.InsertEmpireHistoryForPlayer(history);
			}
		}

		private void CheckMoraleShipBonuses()
		{
			foreach (PlayerInfo playerInfo in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				foreach (ColonyInfo colonyInfo in this._db.GetPlayerColoniesByPlayerId(playerInfo.ID).ToList<ColonyInfo>())
				{
					OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID);
					int stratModifier = this._db.GetStratModifier<int>(StratModifiers.PoliceMoralBonus, colonyInfo.PlayerID);
					int num = GameSession.NumPoliceInSystem(this.App, orbitalObjectInfo.StarSystemID) > 0 ? stratModifier : 0;
					int propagandaBonusInSystem = GameSession.GetPropagandaBonusInSystem(this.App, orbitalObjectInfo.StarSystemID, colonyInfo.PlayerID);
					if (num > 0)
						num += stratModifier * (this._db.GetStarSystemInfo(colonyInfo.CachedStarSystemID).IsOpen ? 1 : 2);
					foreach (ColonyFactionInfo faction in colonyInfo.Factions)
					{
						if (faction.LastMorale > faction.Morale)
							faction.Morale += num;
						faction.Morale += propagandaBonusInSystem;
						faction.LastMorale = faction.Morale;
						this._db.UpdateCivilianPopulation(faction);
					}
				}
			}
		}

		private void CheckStationSlots()
		{
			Dictionary<int, StationTypeFlags> dictionary = new Dictionary<int, StationTypeFlags>();
			foreach (StationInfo stationInfo in this._db.GetStationInfos().ToList<StationInfo>())
			{
				OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID);
				if (orbitalObjectInfo != null && orbitalObjectInfo.ParentID.HasValue)
				{
					StationTypeFlags stationTypeFlags = (StationTypeFlags)0;
					if (!dictionary.TryGetValue(orbitalObjectInfo.ParentID.Value, out stationTypeFlags))
					{
						PlanetInfo planetInfo = this._db.GetPlanetInfo(orbitalObjectInfo.ParentID.Value);
						if (planetInfo != null)
						{
							stationTypeFlags = Kerberos.Sots.GameStates.StarSystem.GetSupportedStationTypesForPlanet(this._db, planetInfo);
							dictionary.Add(orbitalObjectInfo.ParentID.Value, stationTypeFlags);
						}
						else
							continue;
					}
					if ((stationTypeFlags & (StationTypeFlags)(1 << (int)(stationInfo.DesignInfo.StationType & (StationType)31))) == (StationTypeFlags)0)
						this._db.DestroyStation(this, stationInfo.OrbitalObjectID, 0);
				}
			}
		}

		private void CheckDestroyedIndys()
		{
			List<PlayerInfo> list = this._db.GetPlayerInfos().ToList<PlayerInfo>();
			list.RemoveAll((Predicate<PlayerInfo>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return !x.includeInDiplomacy;
			   return true;
		   }));
			foreach (PlayerInfo playerInfo in list)
			{
				if (this._db.GetPlayerColoniesByPlayerId(playerInfo.ID).Count<ColonyInfo>() == 0)
					this._db.SetPlayerDefeated(this, playerInfo.ID);
			}
		}

		private void ClampMoraleValues()
		{
			foreach (PlayerInfo playerInfo in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				foreach (ColonyInfo colonyInfo in this._db.GetPlayerColoniesByPlayerId(playerInfo.ID).ToList<ColonyInfo>())
				{
					foreach (ColonyFactionInfo faction in colonyInfo.Factions)
					{
						int morale = faction.Morale;
						faction.Morale = Math.Min(Math.Max(morale, 0), 100);
						if (faction.Morale != morale)
							this._db.UpdateCivilianPopulation(faction);
					}
				}
			}
		}

		private void CheckEndGame()
		{
			List<PlayerInfo> playerInfoList = new List<PlayerInfo>();
			foreach (PlayerInfo playerInfo in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				if (this._db.GetPlayerBankruptcyTurns(playerInfo.ID) > this.AssetDatabase.BankruptcyTurns || this._db.GetPlayerColoniesByPlayerId(playerInfo.ID).Count<ColonyInfo>() <= 0)
					playerInfoList.Add(playerInfo);
			}
			string nameValue = this._app.GameDatabase.GetNameValue("VictoryCondition");
			GameMode gameMode = GameMode.LastSideStanding;
			int num1 = -1;
			if (!string.IsNullOrEmpty(nameValue))
			{
				gameMode = (GameMode)Enum.Parse(typeof(GameMode), nameValue);
				num1 = int.Parse(this._app.GameDatabase.GetNameValue("VictoryValue"));
			}
			switch (gameMode)
			{
				case GameMode.LastSideStanding:
					this.CheckLastSideStanding();
					break;
				case GameMode.LastCapitalStanding:
					int? homeworld = this._app.GameDatabase.GetPlayerInfo(this.LocalPlayer.ID).Homeworld;
					if (!homeworld.HasValue || this._app.GameDatabase.GetColonyInfoForPlanet(homeworld.Value) == null)
						this._app.UI.CreateDialog((Dialog)new EndGameDialog(this._app, App.Localize("@UI_ENDGAME_DEFEAT"), string.Format(App.Localize("@UI_ENDGAME_DEFEAT_CONQUEST"), (object)this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName), "loseScreen"), null);
					bool flag = false;
					List<PlayerInfo> list1 = this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
					list1.RemoveAll((Predicate<PlayerInfo>)(x => x.ID == this.LocalPlayer.ID));
					foreach (PlayerInfo playerInfo in list1)
					{
						if (playerInfo.ID != this.LocalPlayer.ID && this.GameDatabase.GetDiplomacyStateBetweenPlayers(playerInfo.ID, this.LocalPlayer.ID) != DiplomacyState.ALLIED)
						{
							if (playerInfo.Homeworld.HasValue && this.GameDatabase.GetColonyInfoForPlanet(playerInfo.Homeworld.Value) != null)
								flag = true;
							else
								playerInfoList.Add(playerInfo);
						}
					}
					if (!flag)
					{
						this._app.UI.CreateDialog((Dialog)new EndGameDialog(this._app, App.Localize("@UI_ENDGAME_VICTORY"), string.Format(App.Localize("@UI_ENDGAME_VICTORY_CONQUEST"), (object)this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName), "winScreen"), null);
						break;
					}
					break;
				case GameMode.StarChamberLimit:
					List<PlayerInfo> list2 = this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
					using (List<PlayerInfo>.Enumerator enumerator = list2.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							PlayerInfo pi = enumerator.Current;
							if (this.GameDatabase.GetStationInfosByPlayerID(pi.ID).ToList<StationInfo>().Count<StationInfo>((Func<StationInfo, bool>)(x =>
						   {
							   if (x.DesignInfo.StationLevel == 5)
								   return x.DesignInfo.StationType == StationType.DIPLOMATIC;
							   return false;
						   })) >= num1)
								this.SendEndGameDialog(list2.Where<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID == pi.ID)).Select<PlayerInfo, int>((Func<PlayerInfo, int>)(x => x.ID)).ToList<int>(), list2.Where<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID != pi.ID)).Select<PlayerInfo, int>((Func<PlayerInfo, int>)(x => x.ID)).ToList<int>(), App.Localize("@UI_ENDGAME_VICTORY"), string.Format(App.Localize("@UI_ENDGAME_VICTORY_CONQUEST"), (object)this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName), App.Localize("@UI_ENDGAME_DEFEAT"), string.Format(App.Localize("@UI_ENDGAME_DEFEAT_CONQUEST"), (object)this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName));
						}
						break;
					}
				case GameMode.GemWorldLimit:
					List<PlayerInfo> list3 = this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
					using (List<PlayerInfo>.Enumerator enumerator = list3.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							PlayerInfo pi = enumerator.Current;
							if (this.GameDatabase.GetPlayerColoniesByPlayerId(pi.ID).ToList<ColonyInfo>().Count<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.CurrentStage == Kerberos.Sots.Data.ColonyStage.GemWorld)) >= num1)
								this.SendEndGameDialog(list3.Where<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID == pi.ID)).Select<PlayerInfo, int>((Func<PlayerInfo, int>)(x => x.ID)).ToList<int>(), list3.Where<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID != pi.ID)).Select<PlayerInfo, int>((Func<PlayerInfo, int>)(x => x.ID)).ToList<int>(), App.Localize("@UI_ENDGAME_VICTORY"), string.Format(App.Localize("@UI_ENDGAME_VICTORY_CONQUEST"), (object)this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName), App.Localize("@UI_ENDGAME_DEFEAT"), string.Format(App.Localize("@UI_ENDGAME_DEFEAT_CONQUEST"), (object)this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName));
						}
						break;
					}
				case GameMode.ProvinceLimit:
					List<PlayerInfo> list4 = this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
					List<ProvinceInfo> list5 = this.GameDatabase.GetProvinceInfos().ToList<ProvinceInfo>();
					using (List<PlayerInfo>.Enumerator enumerator = list4.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							PlayerInfo pi = enumerator.Current;
							if (list5.Count<ProvinceInfo>((Func<ProvinceInfo, bool>)(x => x.PlayerID == pi.ID)) >= num1)
								this.SendEndGameDialog(list4.Where<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID == pi.ID)).Select<PlayerInfo, int>((Func<PlayerInfo, int>)(x => x.ID)).ToList<int>(), list4.Where<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID != pi.ID)).Select<PlayerInfo, int>((Func<PlayerInfo, int>)(x => x.ID)).ToList<int>(), App.Localize("@UI_ENDGAME_VICTORY"), string.Format(App.Localize("@UI_ENDGAME_VICTORY_CONQUEST"), (object)this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName), App.Localize("@UI_ENDGAME_DEFEAT"), string.Format(App.Localize("@UI_ENDGAME_DEFEAT_CONQUEST"), (object)this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName));
						}
						break;
					}
				case GameMode.LeviathanLimit:
					List<PlayerInfo> list6 = this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
					using (List<PlayerInfo>.Enumerator enumerator = list6.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							PlayerInfo pi = enumerator.Current;
							int num2 = 0;
							foreach (FleetInfo fleetInfo in this.GameDatabase.GetFleetInfosByPlayerID(pi.ID, FleetType.FL_NORMAL).ToList<FleetInfo>())
							{
								List<ShipInfo> list7 = this.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, true).ToList<ShipInfo>();
								num2 += list7.Count<ShipInfo>((Func<ShipInfo, bool>)(x =>
							   {
								   if (x.DesignInfo.Class == ShipClass.Leviathan)
									   return !x.DesignInfo.IsSuulka();
								   return false;
							   }));
							}
							if (num2 >= num1)
								this.SendEndGameDialog(list6.Where<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID == pi.ID)).Select<PlayerInfo, int>((Func<PlayerInfo, int>)(x => x.ID)).ToList<int>(), list6.Where<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID != pi.ID)).Select<PlayerInfo, int>((Func<PlayerInfo, int>)(x => x.ID)).ToList<int>(), App.Localize("@UI_ENDGAME_VICTORY"), string.Format(App.Localize("@UI_ENDGAME_VICTORY_CONQUEST"), (object)this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName), App.Localize("@UI_ENDGAME_DEFEAT"), string.Format(App.Localize("@UI_ENDGAME_DEFEAT_CONQUEST"), (object)this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName));
						}
						break;
					}
				case GameMode.LandGrab:
					List<PlayerInfo> list8 = this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
					Dictionary<int, int> dictionary = new Dictionary<int, int>();
					int num3 = 0;
					foreach (PlayerInfo playerInfo in list8)
					{
						List<int> list7 = this.GameDatabase.GetPlayerColonySystemIDs(playerInfo.ID).ToList<int>();
						num3 += list7.Count;
						dictionary.Add(playerInfo.ID, list7.Count);
					}
					using (List<PlayerInfo>.Enumerator enumerator = list8.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							PlayerInfo pi = enumerator.Current;
							if ((double)dictionary[pi.ID] / (double)num3 >= (double)num1 / 100.0)
								this.SendEndGameDialog(list8.Where<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID == pi.ID)).Select<PlayerInfo, int>((Func<PlayerInfo, int>)(x => x.ID)).ToList<int>(), list8.Where<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID != pi.ID)).Select<PlayerInfo, int>((Func<PlayerInfo, int>)(x => x.ID)).ToList<int>(), App.Localize("@UI_ENDGAME_VICTORY"), string.Format(App.Localize("@UI_ENDGAME_VICTORY_CONQUEST"), (object)this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName), App.Localize("@UI_ENDGAME_DEFEAT"), string.Format(App.Localize("@UI_ENDGAME_DEFEAT_CONQUEST"), (object)this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName));
						}
						break;
					}
			}
			if (gameMode != GameMode.LastSideStanding)
				this.CheckLastSideStanding();
			foreach (PlayerInfo playerInfo in playerInfoList)
			{
				if (!playerInfo.isDefeated)
				{
					GameSession.SimAITurns = 0;
					this._db.SetPlayerDefeated(this, playerInfo.ID);
					this.App.GameSetup.Players[playerInfo.ID - 1].Status = NPlayerStatus.PS_DEFEATED;
				}
			}
		}

		private void CheckLastSideStanding()
		{
			if (this.GameDatabase.GetPlayerColoniesByPlayerId(this.LocalPlayer.ID).ToList<ColonyInfo>().Count == 0)
				this._app.UI.CreateDialog((Dialog)new EndGameDialog(this._app, App.Localize("@UI_ENDGAME_DEFEAT"), string.Format(App.Localize("@UI_ENDGAME_DEFEAT_CONQUEST"), (object)this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName), "loseScreen"), null);
			bool flag = false;
			List<PlayerInfo> list = this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
			list.RemoveAll((Predicate<PlayerInfo>)(x => x.ID == this.LocalPlayer.ID));
			foreach (PlayerInfo playerInfo in list)
			{
				if (this.GameDatabase.GetDiplomacyStateBetweenPlayers(playerInfo.ID, this.LocalPlayer.ID) != DiplomacyState.ALLIED && this.GameDatabase.GetPlayerColoniesByPlayerId(playerInfo.ID).Count<ColonyInfo>() > 0)
					flag = true;
			}
			if (flag)
				return;
			this._app.UI.CreateDialog((Dialog)new EndGameDialog(this._app, App.Localize("@UI_ENDGAME_VICTORY"), string.Format(App.Localize("@UI_ENDGAME_VICTORY_CONQUEST"), (object)this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName), "winScreen"), null);
			GameSession.SimAITurns = 0;
		}

		public void SendEndGameDialog(
		  List<int> winners,
		  List<int> losers,
		  string victoryTitle,
		  string victoryDesc,
		  string defeatTitle,
		  string defeatDesc)
		{
			if (winners.Contains(this.LocalPlayer.ID))
			{
				this._app.UI.CreateDialog((Dialog)new EndGameDialog(this._app, victoryTitle, victoryDesc, "winScreen"), null);
			}
			else
			{
				if (!losers.Contains(this.LocalPlayer.ID))
					return;
				this._app.UI.CreateDialog((Dialog)new EndGameDialog(this._app, defeatTitle, defeatDesc, "loseScreen"), null);
			}
		}

		public static void ApplyConsequences(
		  GameDatabase db,
		  int offendingPlayer,
		  int receivingPlayer,
		  List<TreatyConsequenceInfo> consequences)
		{
			foreach (TreatyConsequenceInfo consequence in consequences)
			{
				PlayerInfo playerInfo1 = db.GetPlayerInfo(offendingPlayer);
				PlayerInfo playerInfo2 = db.GetPlayerInfo(receivingPlayer);
				switch (consequence.Type)
				{
					case ConsequenceType.Fine:
						db.UpdatePlayerSavings(offendingPlayer, playerInfo1.Savings - (double)consequence.ConsequenceValue);
						db.UpdatePlayerSavings(receivingPlayer, playerInfo2.Savings + (double)consequence.ConsequenceValue);
						continue;
					case ConsequenceType.DiplomaticStatusPenalty:
						DiplomacyState state = db.GetDiplomacyStateBetweenPlayers(offendingPlayer, receivingPlayer);
						switch (state)
						{
							case DiplomacyState.CEASE_FIRE:
								state = DiplomacyState.NEUTRAL;
								break;
							case DiplomacyState.NON_AGGRESSION:
								state = DiplomacyState.CEASE_FIRE;
								break;
							case DiplomacyState.ALLIED:
								state = DiplomacyState.PEACE;
								break;
							case DiplomacyState.NEUTRAL:
								state = DiplomacyState.WAR;
								db.InsertGovernmentAction(offendingPlayer, App.Localize("@GA_DECLAREWAR"), "DeclareWar", 0, 0);
								break;
							case DiplomacyState.PEACE:
								state = DiplomacyState.NON_AGGRESSION;
								break;
						}
						db.ChangeDiplomacyState(offendingPlayer, receivingPlayer, state);
						continue;
					case ConsequenceType.Trade:
						using (List<TreatyInfo>.Enumerator enumerator = db.GetTreatyInfos().ToList<TreatyInfo>().GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								TreatyInfo current = enumerator.Current;
								if (current.Type == TreatyType.Trade && (current.ReceivingPlayerId == offendingPlayer && current.InitiatingPlayerId == receivingPlayer || current.InitiatingPlayerId == receivingPlayer && current.ReceivingPlayerId == receivingPlayer))
									db.RemoveTreatyInfo(current.ID);
							}
							continue;
						}
					case ConsequenceType.Sanction:
						List<int> list = db.GetStandardPlayerIDs().ToList<int>();
						List<int> intList = new List<int>();
						foreach (int otherPlayerID in list)
						{
							if (otherPlayerID != receivingPlayer && otherPlayerID != offendingPlayer && db.GetDiplomacyStateBetweenPlayers(receivingPlayer, otherPlayerID) == DiplomacyState.ALLIED)
								intList.Add(otherPlayerID);
						}
						using (List<TreatyInfo>.Enumerator enumerator = db.GetTreatyInfos().ToList<TreatyInfo>().GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								TreatyInfo current = enumerator.Current;
								if (current.Type == TreatyType.Trade && (current.ReceivingPlayerId == offendingPlayer && intList.Contains(current.InitiatingPlayerId) || current.InitiatingPlayerId == offendingPlayer && intList.Contains(current.ReceivingPlayerId)))
									db.RemoveTreatyInfo(current.ID);
							}
							continue;
						}
					case ConsequenceType.DiplomaticPointPenalty:
						db.UpdateGenericDiplomacyPoints(offendingPlayer, playerInfo1.GenericDiplomacyPoints - (int)consequence.ConsequenceValue);
						db.UpdateGenericDiplomacyPoints(offendingPlayer, playerInfo2.GenericDiplomacyPoints + (int)consequence.ConsequenceValue);
						continue;
					case ConsequenceType.War:
						db.ChangeDiplomacyState(offendingPlayer, receivingPlayer, DiplomacyState.WAR);
						db.InsertGovernmentAction(offendingPlayer, App.Localize("@GA_DECLAREWAR"), "DeclareWar", 0, 0);
						continue;
					default:
						continue;
				}
			}
		}

		public static bool CheckLimitationTreaty(
		  GameDatabase db,
		  LimitationTreatyInfo lti,
		  out int playerId)
		{
			playerId = 0;
			switch (lti.LimitationType)
			{
				case LimitationTreatyType.FleetSize:
					List<FleetInfo> list1 = db.GetFleetInfos(FleetType.FL_NORMAL).ToList<FleetInfo>();
					if ((double)list1.Where<FleetInfo>((Func<FleetInfo, bool>)(x => x.PlayerID == lti.ReceivingPlayerId)).ToList<FleetInfo>().Sum<FleetInfo>((Func<FleetInfo, int>)(x => db.GetFleetCruiserEquivalent(x.ID))) > (double)lti.LimitationAmount)
					{
						playerId = lti.ReceivingPlayerId;
						return false;
					}
					if ((double)list1.Where<FleetInfo>((Func<FleetInfo, bool>)(x => x.PlayerID == lti.InitiatingPlayerId)).ToList<FleetInfo>().Sum<FleetInfo>((Func<FleetInfo, int>)(x => db.GetFleetCruiserEquivalent(x.ID))) <= (double)lti.LimitationAmount)
						return true;
					playerId = lti.InitiatingPlayerId;
					return false;
				case LimitationTreatyType.ShipClass:
					List<FleetInfo> list2 = db.GetFleetInfos(FleetType.FL_NORMAL).ToList<FleetInfo>();
					List<FleetInfo> list3 = list2.Where<FleetInfo>((Func<FleetInfo, bool>)(x => x.PlayerID == lti.ReceivingPlayerId)).ToList<FleetInfo>();
					int num1 = 0;
					foreach (FleetInfo fleetInfo in list3)
					{
						foreach (ShipInfo shipInfo in db.GetShipInfoByFleetID(fleetInfo.ID, true).ToList<ShipInfo>())
						{
							if (shipInfo.DesignInfo.Class == (ShipClass)int.Parse(lti.LimitationGroup))
								++num1;
						}
					}
					if ((double)num1 > (double)lti.LimitationAmount)
					{
						playerId = lti.ReceivingPlayerId;
						return false;
					}
					List<FleetInfo> list4 = list2.Where<FleetInfo>((Func<FleetInfo, bool>)(x => x.PlayerID == lti.InitiatingPlayerId)).ToList<FleetInfo>();
					int num2 = 0;
					foreach (FleetInfo fleetInfo in list4)
					{
						foreach (ShipInfo shipInfo in db.GetShipInfoByFleetID(fleetInfo.ID, true).ToList<ShipInfo>())
						{
							if (shipInfo.DesignInfo.Class == (ShipClass)int.Parse(lti.LimitationGroup))
								++num2;
						}
					}
					if ((double)num2 <= (double)lti.LimitationAmount)
						return true;
					playerId = lti.InitiatingPlayerId;
					return false;
				case LimitationTreatyType.Weapon:
					foreach (DesignInfo designInfo in db.GetDesignInfosForPlayer(lti.ReceivingPlayerId).ToList<DesignInfo>())
					{
						foreach (DesignSectionInfo designSection in designInfo.DesignSections)
						{
							foreach (WeaponBankInfo weaponBank in designSection.WeaponBanks)
							{
								int? weaponId = weaponBank.WeaponID;
								int num3 = int.Parse(lti.LimitationGroup);
								if ((weaponId.GetValueOrDefault() != num3 ? 0 : (weaponId.HasValue ? 1 : 0)) != 0)
								{
									playerId = lti.ReceivingPlayerId;
									return false;
								}
							}
						}
					}
					foreach (DesignInfo designInfo in db.GetDesignInfosForPlayer(lti.InitiatingPlayerId).ToList<DesignInfo>())
					{
						foreach (DesignSectionInfo designSection in designInfo.DesignSections)
						{
							foreach (WeaponBankInfo weaponBank in designSection.WeaponBanks)
							{
								int? weaponId = weaponBank.WeaponID;
								int num3 = int.Parse(lti.LimitationGroup);
								if ((weaponId.GetValueOrDefault() != num3 ? 0 : (weaponId.HasValue ? 1 : 0)) != 0)
								{
									playerId = lti.InitiatingPlayerId;
									return false;
								}
							}
						}
					}
					return true;
				case LimitationTreatyType.ResearchTree:
					int researchingTechId1 = db.GetPlayerResearchingTechID(lti.ReceivingPlayerId);
					if (researchingTechId1 == 0)
						return true;
					string fileId = db.GetTechFileID(researchingTechId1);
					if (db.AssetDatabase.MasterTechTree.Technologies.First<Kerberos.Sots.Data.TechnologyFramework.Tech>((Func<Kerberos.Sots.Data.TechnologyFramework.Tech, bool>)(x => x.Id == fileId)).Family == lti.LimitationGroup)
					{
						playerId = lti.ReceivingPlayerId;
						return false;
					}
					int researchingTechId2 = db.GetPlayerResearchingTechID(lti.InitiatingPlayerId);
					if (researchingTechId2 == 0)
						return true;
					fileId = db.GetTechFileID(researchingTechId2);
					if (!(db.AssetDatabase.MasterTechTree.Technologies.First<Kerberos.Sots.Data.TechnologyFramework.Tech>((Func<Kerberos.Sots.Data.TechnologyFramework.Tech, bool>)(x => x.Id == fileId)).Family == lti.LimitationGroup))
						return true;
					playerId = lti.InitiatingPlayerId;
					return false;
				case LimitationTreatyType.ResearchTech:
					if (db.GetPlayerResearchingTechID(lti.ReceivingPlayerId) == int.Parse(lti.LimitationGroup))
					{
						playerId = lti.ReceivingPlayerId;
						return false;
					}
					if (db.GetPlayerResearchingTechID(lti.InitiatingPlayerId) != int.Parse(lti.LimitationGroup))
						return true;
					playerId = lti.InitiatingPlayerId;
					return false;
				case LimitationTreatyType.EmpireSize:
					List<ColonyInfo> list5 = db.GetColonyInfos().ToList<ColonyInfo>();
					if (list5.Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == lti.InitiatingPlayerId)).Count<ColonyInfo>() > (int)lti.LimitationAmount)
					{
						playerId = lti.InitiatingPlayerId;
						return false;
					}
					if (list5.Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == lti.ReceivingPlayerId)).Count<ColonyInfo>() <= (int)lti.LimitationAmount)
						return true;
					playerId = lti.ReceivingPlayerId;
					return false;
				case LimitationTreatyType.ForgeGemWorlds:
					List<ColonyInfo> list6 = db.GetColonyInfos().ToList<ColonyInfo>();
					if (list6.Where<ColonyInfo>((Func<ColonyInfo, bool>)(x =>
				   {
					   if (x.CurrentStage == Kerberos.Sots.Data.ColonyStage.GemWorld || x.CurrentStage == Kerberos.Sots.Data.ColonyStage.ForgeWorld)
						   return x.PlayerID == lti.InitiatingPlayerId;
					   return false;
				   })).Count<ColonyInfo>() > (int)lti.LimitationAmount)
					{
						playerId = lti.InitiatingPlayerId;
						return false;
					}
					if (list6.Where<ColonyInfo>((Func<ColonyInfo, bool>)(x =>
				   {
					   if (x.CurrentStage == Kerberos.Sots.Data.ColonyStage.GemWorld || x.CurrentStage == Kerberos.Sots.Data.ColonyStage.ForgeWorld)
						   return x.PlayerID == lti.ReceivingPlayerId;
					   return false;
				   })).Count<ColonyInfo>() <= (int)lti.LimitationAmount)
						return true;
					playerId = lti.ReceivingPlayerId;
					return false;
				case LimitationTreatyType.StationType:
					List<StationInfo> list7 = db.GetStationInfos().ToList<StationInfo>();
					if ((double)list7.Where<StationInfo>((Func<StationInfo, bool>)(x => x.PlayerID == lti.ReceivingPlayerId)).ToList<StationInfo>().Count<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == (StationType)int.Parse(lti.LimitationGroup))) > (double)lti.LimitationAmount)
					{
						playerId = lti.ReceivingPlayerId;
						return false;
					}
					if ((double)list7.Where<StationInfo>((Func<StationInfo, bool>)(x => x.PlayerID == lti.InitiatingPlayerId)).ToList<StationInfo>().Count<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == (StationType)int.Parse(lti.LimitationGroup))) <= (double)lti.LimitationAmount)
						return true;
					playerId = lti.InitiatingPlayerId;
					return false;
				default:
					return true;
			}
		}

		public void UpdateRequests(GameDatabase db)
		{
			foreach (RequestInfo ri in db.GetRequestInfos().ToList<RequestInfo>().Where<RequestInfo>((Func<RequestInfo, bool>)(x => x.State == AgreementState.Unrequested)).ToList<RequestInfo>())
			{
				Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(db.GetPlayerInfo(ri.ReceivingPlayer).ID);
				if (playerObject.IsAI())
				{
					if (playerObject.GetAI().HandleRequestRequest(ri))
						this.AcceptRequest(ri);
					else
						this.DeclineRequest(ri);
				}
				else
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_REQUEST_REQUESTED,
						EventMessage = TurnEventMessage.EM_REQUEST_REQUESTED,
						PlayerID = ri.ReceivingPlayer,
						TargetPlayerID = ri.InitiatingPlayer,
						TreatyID = ri.ID,
						TurnNumber = this._app.GameDatabase.GetTurnCount(),
						ShowsDialog = true
					});
			}
		}

		public int GetLOAPlayerForRebellion(PlayerInfo poorSap)
		{
			List<PlayerInfo> list = this._db.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = list.FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (x.isAIRebellionPlayer)
				   return x.FactionID == poorSap.FactionID;
			   return false;
		   }));
			if (playerInfo != null)
				return playerInfo.ID;
			int num = this._db.InsertPlayer(App.Localize("@AI_REBELLION_NAME"), this._db.GetFactionInfo(poorSap.FactionID).Name, new int?(), new Vector3(0.19f, 0.04f, 0.04f), Vector3.Zero, "", "", 0.0, 0, false, true, true, 0, AIDifficulty.VeryHard);
			this.AddPlayerObject(this._db.GetPlayerInfo(num), Kerberos.Sots.PlayerFramework.Player.ClientTypes.AI);
			this._db.DuplicateStratModifiers(num, poorSap.ID);
			this._db.DuplicateTechs(num, poorSap.ID);
			this.AvailableShipSectionsChanged();
			this.CheckForNewEquipment(num);
			this.AvailableShipSectionsChanged();
			float stratModifier1 = this._db.GetStratModifier<float>(StratModifiers.AIProductionBonus, num);
			float stratModifier2 = this._db.GetStratModifier<float>(StratModifiers.AIResearchBonus, num);
			float stratModifier3 = this._db.GetStratModifier<float>(StratModifiers.ConstructionPointBonus, num);
			this._db.SetStratModifier(StratModifiers.AIProductionBonus, num, (object)(float)((double)stratModifier1 + 2.0));
			this._db.SetStratModifier(StratModifiers.AIResearchBonus, num, (object)(float)((double)stratModifier2 + 2.0));
			this._db.SetStratModifier(StratModifiers.ConstructionPointBonus, num, (object)(float)((double)stratModifier3 + 2.0));
			using (List<PlayerInfo>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					PlayerInfo current = enumerator.Current;
					if (current.isAIRebellionPlayer)
						this._db.UpdateDiplomacyState(current.ID, num, DiplomacyState.ALLIED, 2000, true);
					else
						this._db.UpdateDiplomacyState(current.ID, num, DiplomacyState.WAR, 0, true);
				}
				return num;
			}
		}

		public void DoAIRebellion(PlayerInfo poorSap, bool researchtrigger = true)
		{
			int playerForRebellion = this.GetLOAPlayerForRebellion(poorSap);
			List<int> list1 = this._db.GetPlayerColonySystemIDs(poorSap.ID).ToList<int>();
			HomeworldInfo playerHomeworld = this._db.GetPlayerHomeworld(poorSap.ID);
			list1.Remove(playerHomeworld.SystemID);
			int num1 = (int)Math.Floor((double)list1.Count * (double)this.AssetDatabase.AIRebellionColonyPercent);
			List<string> stringList = new List<string>();
			for (int index = 0; index < num1; ++index)
			{
				int num2 = this._random.Choose<int>((IList<int>)list1);
				list1.Remove(num2);
				foreach (ColonyInfo colony in this._db.GetColonyInfosForSystem(num2).ToList<ColonyInfo>())
				{
					PlanetInfo planetInfo = this._db.GetPlanetInfo(colony.OrbitalObjectID);
					foreach (ColonyFactionInfo colonyFactionInfo in this._db.GetCivilianPopulations(colony.OrbitalObjectID).ToList<ColonyFactionInfo>())
						this._db.RemoveCivilianPopulation(colony.OrbitalObjectID, colonyFactionInfo.FactionID);
					colony.PlayerID = playerForRebellion;
					planetInfo.Biosphere = 0;
					this._db.UpdateColony(colony);
					this._db.UpdatePlanet(planetInfo);
					this._db.InsertAIOldColonyOwner(colony.ID, poorSap.ID);
				}
				stringList.Add(this._db.GetStarSystemInfo(num2).Name);
			}
			if (num1 > 0)
			{
				string str1 = "";
				for (int index = 0; index < stringList.Count - 1; ++index)
					str1 = str1 + stringList[index] + ", ";
				string str2 = str1 + stringList[stringList.Count - 1];
				if (researchtrigger)
					this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_RESEARCH_AI_DISASTER,
						EventMessage = TurnEventMessage.EM_RESEARCH_AI_DISASTER,
						NamesList = str2,
						PlayerID = poorSap.ID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				else
					this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_AI_REBELLION_START,
						EventMessage = TurnEventMessage.EM_AI_REBELLION_START,
						NamesList = str2,
						PlayerID = poorSap.ID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
			}
			foreach (ShipInfo ship in this._db.GetShipInfos(true).ToList<ShipInfo>())
			{
				ShipSectionAsset commandSectionAsset = ship.DesignInfo.GetCommandSectionAsset(this.AssetDatabase);
				if (ship.DesignInfo.PlayerID == poorSap.ID && commandSectionAsset != null && commandSectionAsset.IsAIControl)
				{
					FleetInfo fleetInfo1 = this._db.GetFleetInfo(ship.FleetID);
					List<FleetInfo> list2 = this.App.GameDatabase.GetFleetInfoBySystemID(fleetInfo1.SystemID, FleetType.FL_NORMAL).ToList<FleetInfo>();
					int? nullable = new int?();
					foreach (FleetInfo fleetInfo2 in list2)
					{
						MissionInfo missionByFleetId = this.App.GameDatabase.GetMissionByFleetID(fleetInfo2.ID);
						if (missionByFleetId != null && missionByFleetId.Type == MissionType.RETREAT && fleetInfo2.PlayerID == playerForRebellion)
						{
							nullable = new int?(fleetInfo2.ID);
							break;
						}
					}
					if (!nullable.HasValue)
					{
						nullable = new int?(this.App.GameDatabase.InsertFleet(fleetInfo1.PlayerID, 0, fleetInfo1.SystemID, 0, App.Localize("@FLEET_RETREAT"), FleetType.FL_NORMAL));
						int missionID = this.App.GameDatabase.InsertMission(nullable.Value, MissionType.RETREAT, 0, 0, 0, 0, false, new int?());
						this.App.GameDatabase.InsertWaypoint(missionID, WaypointType.ReturnHome, new int?());
						this.App.GameDatabase.InsertWaypoint(missionID, WaypointType.DisbandFleet, new int?());
					}
					this._db.TransferShipToPlayer(ship, playerForRebellion);
					this._db.TransferShip(ship.ID, nullable.Value);
				}
			}
		}

		public void CheckAIRebellions(GameDatabase db)
		{
			foreach (PlayerInfo poorSap in db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				bool flag = false;
				int researchingTechId = db.GetPlayerResearchingTechID(poorSap.ID);
				if (researchingTechId != 0 || flag)
				{
					string techFileId = db.GetTechFileID(researchingTechId);
					Kerberos.Sots.Data.TechnologyFramework.Tech tech = this.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault<Kerberos.Sots.Data.TechnologyFramework.Tech>((Func<Kerberos.Sots.Data.TechnologyFramework.Tech, bool>)(x => x.Id == techFileId));
					if ((tech != null && tech.AllowAIRebellion && this._db.GetStratModifier<bool>(StratModifiers.AllowAIRebellion, poorSap.ID) || flag) && (this._random.CoinToss((double)this.AssetDatabase.AIRebellionChance) || flag))
						this.DoAIRebellion(poorSap, false);
				}
			}
		}

		public void UpdateDemands(GameDatabase db)
		{
			foreach (DemandInfo di in db.GetDemandInfos().ToList<DemandInfo>().Where<DemandInfo>((Func<DemandInfo, bool>)(x => x.State == AgreementState.Unrequested)).ToList<DemandInfo>())
			{
				Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(db.GetPlayerInfo(di.ReceivingPlayer).ID);
				if (playerObject.IsAI())
				{
					if (playerObject.GetAI().HandleDemandRequest(di))
						this.AcceptDemand(di);
					else
						this.DeclineDemand(di);
				}
				else
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_DEMAND_REQUESTED,
						EventMessage = TurnEventMessage.EM_DEMAND_REQUESTED,
						PlayerID = di.ReceivingPlayer,
						TargetPlayerID = di.InitiatingPlayer,
						TreatyID = di.ID,
						TurnNumber = this._app.GameDatabase.GetTurnCount(),
						ShowsDialog = true
					});
			}
		}

		public void AcceptRequest(RequestInfo ri)
		{
			this._db.SetRequestState(AgreementState.Accepted, ri.ID);
			PlayerInfo playerInfo1 = this._db.GetPlayerInfo(ri.ReceivingPlayer);
			PlayerInfo playerInfo2 = this._db.GetPlayerInfo(ri.InitiatingPlayer);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_REQUEST_ACCEPTED,
				EventMessage = TurnEventMessage.EM_REQUEST_ACCEPTED,
				PlayerID = ri.InitiatingPlayer,
				TargetPlayerID = ri.ReceivingPlayer,
				TreatyID = ri.ID,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				ShowsDialog = true
			});
			switch (ri.Type)
			{
				case RequestType.SavingsRequest:
					this._db.UpdatePlayerSavings(playerInfo2.ID, playerInfo2.Savings + (double)ri.RequestValue);
					this._db.UpdatePlayerSavings(playerInfo1.ID, playerInfo1.Savings - (double)ri.RequestValue);
					if (this.GetPlayerObject(ri.ReceivingPlayer).IsAI() && this.GetPlayerObject(ri.ReceivingPlayer).IsStandardPlayer)
					{
						int reactionAmount = (int)((double)(int)Math.Min(ri.RequestValue / 20000f, 100f) * ((this._db.GetPlayerFaction(ri.InitiatingPlayer).Name == "morrigi" ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") * 0.5 : 0.0) + (this._db.PlayerHasTech(ri.InitiatingPlayer, "PSI_Lesser_Glamour") ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") : 0.0) - 1.0) * 0.5);
						this.App.GameDatabase.ApplyDiplomacyReaction(ri.ReceivingPlayer, ri.InitiatingPlayer, reactionAmount, new StratModifiers?(StratModifiers.DiplomacyReactionMoney), 1);
					}
					if (!this.GetPlayerObject(ri.InitiatingPlayer).IsAI() || !this.GetPlayerObject(ri.InitiatingPlayer).IsStandardPlayer)
						break;
					int reactionAmount1 = (int)((double)(int)Math.Min(ri.RequestValue / 20000f, 100f) * ((this._db.GetPlayerFaction(ri.ReceivingPlayer).Name == "morrigi" ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") * 0.5 : 0.0) + (this._db.PlayerHasTech(ri.ReceivingPlayer, "PSI_Lesser_Glamour") ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") : 0.0) + 1.0) * 0.5);
					this.App.GameDatabase.ApplyDiplomacyReaction(ri.InitiatingPlayer, ri.ReceivingPlayer, reactionAmount1, new StratModifiers?(StratModifiers.DiplomacyReactionMoney), 1);
					break;
				case RequestType.SystemInfoRequest:
					int requestValue = (int)ri.RequestValue;
					if (requestValue == 0)
						break;
					this._db.UpdatePlayerViewWithStarSystem(ri.InitiatingPlayer, requestValue);
					int turnCount = this._db.GetTurnCount();
					this._db.InsertExploreRecord(requestValue, ri.InitiatingPlayer, turnCount, true, true);
					break;
				case RequestType.ResearchPointsRequest:
					this._db.UpdatePlayerSavings(playerInfo1.ID, playerInfo1.Savings - (double)ri.RequestValue);
					this._db.UpdatePlayerAdditionalResearchPoints(ri.InitiatingPlayer, playerInfo2.AdditionalResearchPoints + this.ConvertToResearchPoints(playerInfo1.ID, (double)ri.RequestValue));
					if (this.GetPlayerObject(ri.ReceivingPlayer).IsAI() && this.GetPlayerObject(ri.ReceivingPlayer).IsStandardPlayer)
					{
						int reactionAmount2 = (int)((double)(int)Math.Min(ri.RequestValue / 20000f, 100f) * ((this._db.GetPlayerFaction(ri.InitiatingPlayer).Name == "morrigi" ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") * 0.5 : 0.0) + (this._db.PlayerHasTech(ri.InitiatingPlayer, "PSI_Lesser_Glamour") ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") : 0.0) - 1.0) * 0.5);
						this.App.GameDatabase.ApplyDiplomacyReaction(ri.ReceivingPlayer, ri.InitiatingPlayer, reactionAmount2, new StratModifiers?(StratModifiers.DiplomacyReactionResearch), 1);
					}
					if (!this.GetPlayerObject(ri.InitiatingPlayer).IsAI() || !this.GetPlayerObject(ri.InitiatingPlayer).IsStandardPlayer)
						break;
					int reactionAmount3 = (int)((double)(int)Math.Min(ri.RequestValue / 20000f, 100f) * ((this._db.GetPlayerFaction(ri.ReceivingPlayer).Name == "morrigi" ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") * 0.5 : 0.0) + (this._db.PlayerHasTech(ri.ReceivingPlayer, "PSI_Lesser_Glamour") ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") : 0.0) + 1.0) * 0.5);
					this.App.GameDatabase.ApplyDiplomacyReaction(ri.InitiatingPlayer, ri.ReceivingPlayer, reactionAmount3, new StratModifiers?(StratModifiers.DiplomacyReactionResearch), 1);
					break;
				case RequestType.EstablishEnclaveRequest:
					this._app.GameDatabase.InsertGovernmentAction(ri.ReceivingPlayer, App.Localize("@GA_ENCLAVEACCEPTED"), "EnclaveAccepted", 0, 0);
					break;
			}
		}

		public void AcceptDemand(DemandInfo di)
		{
			this._db.SetDemandState(AgreementState.Accepted, di.ID);
			PlayerInfo rplayer = this._db.GetPlayerInfo(di.ReceivingPlayer);
			PlayerInfo playerInfo = this._db.GetPlayerInfo(di.InitiatingPlayer);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_DEMAND_ACCEPTED,
				EventMessage = TurnEventMessage.EM_DEMAND_ACCEPTED,
				PlayerID = di.InitiatingPlayer,
				TargetPlayerID = di.ReceivingPlayer,
				TreatyID = di.ID,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				ShowsDialog = true
			});
			switch (di.Type)
			{
				case DemandType.SavingsDemand:
					this._db.UpdatePlayerSavings(playerInfo.ID, playerInfo.Savings + (double)di.DemandValue);
					this._db.UpdatePlayerSavings(rplayer.ID, rplayer.Savings - (double)di.DemandValue);
					if (this.GetPlayerObject(di.ReceivingPlayer).IsAI() && this.GetPlayerObject(di.ReceivingPlayer).IsStandardPlayer)
					{
						int reactionAmount = (int)((double)(int)Math.Min(di.DemandValue / 20000f, 100f) * ((this._db.GetPlayerFaction(di.InitiatingPlayer).Name == "morrigi" ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") * 0.5 : 0.0) + (this._db.PlayerHasTech(di.InitiatingPlayer, "PSI_Lesser_Glamour") ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") : 0.0) - 1.0));
						this.App.GameDatabase.ApplyDiplomacyReaction(di.ReceivingPlayer, di.InitiatingPlayer, reactionAmount, new StratModifiers?(StratModifiers.DiplomacyReactionMoney), 1);
					}
					if (!this.GetPlayerObject(di.InitiatingPlayer).IsAI() || !this.GetPlayerObject(di.InitiatingPlayer).IsStandardPlayer)
						break;
					int reactionAmount1 = (int)((double)(int)Math.Min(di.DemandValue / 20000f, 100f) * ((this._db.GetPlayerFaction(di.ReceivingPlayer).Name == "morrigi" ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") * 0.5 : 0.0) + (this._db.PlayerHasTech(di.ReceivingPlayer, "PSI_Lesser_Glamour") ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") : 0.0) + 1.0));
					this.App.GameDatabase.ApplyDiplomacyReaction(di.InitiatingPlayer, di.ReceivingPlayer, reactionAmount1, new StratModifiers?(StratModifiers.DiplomacyReactionMoney), 1);
					break;
				case DemandType.SystemInfoDemand:
					int demandValue1 = (int)di.DemandValue;
					if (demandValue1 == 0)
						break;
					this._db.UpdatePlayerViewWithStarSystem(di.InitiatingPlayer, demandValue1);
					int turnCount = this._db.GetTurnCount();
					this._db.InsertExploreRecord(demandValue1, di.InitiatingPlayer, turnCount, true, true);
					break;
				case DemandType.ResearchPointsDemand:
					this._db.UpdatePlayerSavings(rplayer.ID, rplayer.Savings - (double)di.DemandValue);
					this._db.UpdatePlayerAdditionalResearchPoints(di.InitiatingPlayer, playerInfo.AdditionalResearchPoints + this.ConvertToResearchPoints(rplayer.ID, (double)di.DemandValue));
					if (this.GetPlayerObject(di.ReceivingPlayer).IsAI() && this.GetPlayerObject(di.ReceivingPlayer).IsStandardPlayer)
					{
						int reactionAmount2 = (int)((double)(int)Math.Min(di.DemandValue / 20000f, 100f) * ((this._db.GetPlayerFaction(di.InitiatingPlayer).Name == "morrigi" ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") * 0.5 : 0.0) + (this._db.PlayerHasTech(di.InitiatingPlayer, "PSI_Lesser_Glamour") ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") : 0.0) - 1.0));
						this.App.GameDatabase.ApplyDiplomacyReaction(di.ReceivingPlayer, di.InitiatingPlayer, reactionAmount2, new StratModifiers?(StratModifiers.DiplomacyReactionResearch), 1);
					}
					if (!this.GetPlayerObject(di.InitiatingPlayer).IsAI() || !this.GetPlayerObject(di.InitiatingPlayer).IsStandardPlayer)
						break;
					int reactionAmount3 = (int)((double)(int)Math.Min(di.DemandValue / 20000f, 100f) * ((this._db.GetPlayerFaction(di.ReceivingPlayer).Name == "morrigi" ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") * 0.5 : 0.0) + (this._db.PlayerHasTech(di.ReceivingPlayer, "PSI_Lesser_Glamour") ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") : 0.0) + 1.0));
					this.App.GameDatabase.ApplyDiplomacyReaction(di.InitiatingPlayer, di.ReceivingPlayer, reactionAmount3, new StratModifiers?(StratModifiers.DiplomacyReactionResearch), 1);
					break;
				case DemandType.SlavesDemand:
					HomeworldInfo playerHomeworld1 = this._db.GetPlayerHomeworld(playerInfo.ID);
					HomeworldInfo playerHomeworld2 = this._db.GetPlayerHomeworld(rplayer.ID);
					ColonyInfo colonyInfo1 = this._db.GetColonyInfo(playerHomeworld1.ColonyID);
					ColonyInfo colonyInfo2 = this._db.GetColonyInfo(playerHomeworld2.ColonyID);
					colonyInfo2.ImperialPop -= (double)di.DemandValue;
					ColonyFactionInfo civPop = ((IEnumerable<ColonyFactionInfo>)colonyInfo1.Factions).FirstOrDefault<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID == rplayer.FactionID));
					if (civPop != null)
					{
						civPop.CivilianPop += (double)di.DemandValue;
						this._db.UpdateCivilianPopulation(civPop);
					}
					else
						this._db.InsertColonyFaction(colonyInfo1.OrbitalObjectID, rplayer.FactionID, (double)di.DemandValue, 0.5f, this._db.GetTurnCount());
					if (this.GetPlayerObject(di.ReceivingPlayer).IsAI() && this.GetPlayerObject(di.ReceivingPlayer).IsStandardPlayer)
					{
						int reactionAmount2 = (int)((double)(int)Math.Min(di.DemandValue / 20000f, 100f) * ((this._db.GetPlayerFaction(di.InitiatingPlayer).Name == "morrigi" ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") * 0.5 : 0.0) + (this._db.PlayerHasTech(di.InitiatingPlayer, "PSI_Lesser_Glamour") ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") : 0.0) - 1.0));
						this.App.GameDatabase.ApplyDiplomacyReaction(di.ReceivingPlayer, di.InitiatingPlayer, reactionAmount2, new StratModifiers?(StratModifiers.DiplomacyReactionSlave), 1);
					}
					if (this.GetPlayerObject(di.InitiatingPlayer).IsAI() && this.GetPlayerObject(di.InitiatingPlayer).IsStandardPlayer)
					{
						int reactionAmount2 = (int)((double)(int)Math.Min(di.DemandValue / 20000f, 100f) * ((this._db.GetPlayerFaction(di.ReceivingPlayer).Name == "morrigi" ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") * 0.5 : 0.0) + (this._db.PlayerHasTech(di.ReceivingPlayer, "PSI_Lesser_Glamour") ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") : 0.0) + 1.0));
						this.App.GameDatabase.ApplyDiplomacyReaction(di.InitiatingPlayer, di.ReceivingPlayer, reactionAmount2, new StratModifiers?(StratModifiers.DiplomacyReactionSlave), 1);
					}
					this._db.UpdateColony(colonyInfo2);
					break;
				case DemandType.WorldDemand:
					StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo((int)di.DemandValue);
					if (starSystemInfo.ProvinceID.HasValue)
						this._db.RemoveProvince(starSystemInfo.ProvinceID.Value);
					foreach (ColonyInfo colony in this._db.GetColonyInfosForSystem((int)di.DemandValue).ToList<ColonyInfo>())
					{
						colony.PlayerID = di.InitiatingPlayer;
						colony.ImperialPop = 100.0;
						this._db.UpdateColony(colony);
					}
					this._db.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_SYSTEM_SURRENDERED_TO_YOU,
						EventMessage = TurnEventMessage.EM_SYSTEM_SURRENDERED_TO_YOU,
						PlayerID = di.InitiatingPlayer,
						TargetPlayerID = di.ReceivingPlayer,
						SystemID = starSystemInfo.ID,
						TurnNumber = this._db.GetTurnCount(),
						ShowsDialog = false
					});
					break;
				case DemandType.ProvinceDemand:
					int demandValue2 = (int)di.DemandValue;
					ProvinceInfo pi = this._db.GetProvinceInfo(demandValue2);
					pi.PlayerID = di.InitiatingPlayer;
					this._db.UpdateProvinceInfo(pi);
					foreach (StellarInfo stellarInfo in this._db.GetStarSystemInfos().ToList<StarSystemInfo>().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
				   {
					   int? provinceId = x.ProvinceID;
					   int id = pi.ID;
					   return provinceId.GetValueOrDefault() == id & provinceId.HasValue;
				   })).ToList<StarSystemInfo>())
					{
						foreach (ColonyInfo colony in this._db.GetColonyInfosForSystem(stellarInfo.ID).ToList<ColonyInfo>())
						{
							colony.PlayerID = di.InitiatingPlayer;
							colony.ImperialPop = 100.0;
							this._db.UpdateColony(colony);
						}
					}
					this._db.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_PROVINCE_SURRENDERED_TO_YOU,
						EventMessage = TurnEventMessage.EM_PROVINCE_SURRENDERED_TO_YOU,
						PlayerID = di.InitiatingPlayer,
						TargetPlayerID = di.ReceivingPlayer,
						ProvinceID = demandValue2,
						TurnNumber = this._db.GetTurnCount(),
						ShowsDialog = false
					});
					break;
				case DemandType.SurrenderDemand:
					foreach (ColonyInfo colony in this._db.GetPlayerColoniesByPlayerId(di.ReceivingPlayer).ToList<ColonyInfo>())
					{
						colony.PlayerID = di.InitiatingPlayer;
						colony.ImperialPop = 100.0;
						this._db.UpdateColony(colony);
					}
					foreach (FleetInfo fleetInfo in this._db.GetFleetInfosByPlayerID(di.ReceivingPlayer, FleetType.FL_ALL).ToList<FleetInfo>())
						this._db.RemoveFleet(fleetInfo.ID);
					foreach (StationInfo stationInfo in this._db.GetStationInfosByPlayerID(di.ReceivingPlayer).ToList<StationInfo>())
						this._db.DestroyStation(this, stationInfo.OrbitalObjectID, 0);
					this._db.UpdatePlayerSavings(playerInfo.ID, playerInfo.Savings + rplayer.Savings);
					this._db.UpdatePlayerSavings(rplayer.ID, 0.0);
					float stratModifier = this._db.GetStratModifier<float>(StratModifiers.ResearchModifier, playerInfo.ID);
					this._db.SetStratModifier(StratModifiers.ResearchModifier, playerInfo.ID, (object)(float)((double)stratModifier + 0.0500000007450581));
					this._db.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_PLAYER_SURRENDERED_TO_YOU,
						EventMessage = TurnEventMessage.EM_PLAYER_SURRENDERED_TO_YOU,
						PlayerID = di.InitiatingPlayer,
						TargetPlayerID = di.ReceivingPlayer,
						TurnNumber = this._db.GetTurnCount(),
						ShowsDialog = false
					});
					this._db.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_YOU_SURRENDER,
						EventMessage = TurnEventMessage.EM_YOU_SURRENDER,
						PlayerID = di.ReceivingPlayer,
						TargetPlayerID = di.InitiatingPlayer,
						TurnNumber = this._db.GetTurnCount(),
						ShowsDialog = false
					});
					this._db.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_SAVINGS_SURRENDERED_TO_YOU,
						EventMessage = TurnEventMessage.EM_SAVINGS_SURRENDERED_TO_YOU,
						PlayerID = di.InitiatingPlayer,
						TargetPlayerID = di.ReceivingPlayer,
						Savings = rplayer.Savings,
						TurnNumber = this._db.GetTurnCount(),
						ShowsDialog = false
					});
					break;
			}
		}

		public void DeclineRequest(RequestInfo ri)
		{
			this._db.SetRequestState(AgreementState.Rejected, ri.ID);
			this._db.GetPlayerInfo(ri.ReceivingPlayer);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_REQUEST_DECLINED,
				EventMessage = TurnEventMessage.EM_REQUEST_DECLINED,
				PlayerID = ri.InitiatingPlayer,
				TargetPlayerID = ri.ReceivingPlayer,
				TreatyID = ri.ID,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				ShowsDialog = true
			});
		}

		public void DeclineDemand(DemandInfo di)
		{
			this._db.SetDemandState(AgreementState.Rejected, di.ID);
			this._db.GetPlayerInfo(di.ReceivingPlayer);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_DEMAND_DECLINED,
				EventMessage = TurnEventMessage.EM_DEMAND_DECLINED,
				PlayerID = di.InitiatingPlayer,
				TargetPlayerID = di.ReceivingPlayer,
				TreatyID = di.ID,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				ShowsDialog = true
			});
		}

		public void UpdateTreaties(GameDatabase db)
		{
			List<TreatyInfo> list = db.GetTreatyInfos().ToList<TreatyInfo>();
			foreach (TreatyInfo treatyInfo in list.Where<TreatyInfo>((Func<TreatyInfo, bool>)(x => x.Removed)))
				db.DeleteTreatyInfo(treatyInfo.ID);
			foreach (TreatyInfo treatyInfo in list.Where<TreatyInfo>((Func<TreatyInfo, bool>)(x => !x.Removed)))
			{
				if (treatyInfo.Active && !treatyInfo.Removed)
				{
					int playerId;
					if (treatyInfo.Type == TreatyType.Limitation && !GameSession.CheckLimitationTreaty(db, (LimitationTreatyInfo)treatyInfo, out playerId))
					{
						this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_TREATY_BROKEN_OFFENDER,
							EventMessage = TurnEventMessage.EM_TREATY_BROKEN_OFFENDER,
							PlayerID = playerId,
							TreatyID = treatyInfo.ID,
							TurnNumber = this._app.GameDatabase.GetTurnCount(),
							ShowsDialog = true
						});
						this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_TREATY_BROKEN_VICTIM,
							EventMessage = TurnEventMessage.EM_TREATY_BROKEN_VICTIM,
							PlayerID = playerId == treatyInfo.InitiatingPlayerId ? treatyInfo.ReceivingPlayerId : treatyInfo.InitiatingPlayerId,
							TreatyID = treatyInfo.ID,
							TurnNumber = this._app.GameDatabase.GetTurnCount(),
							ShowsDialog = true
						});
						GameSession.ApplyConsequences(db, playerId, treatyInfo.InitiatingPlayerId == playerId ? treatyInfo.ReceivingPlayerId : treatyInfo.InitiatingPlayerId, treatyInfo.Consequences);
						treatyInfo.Removed = true;
						this._app.GameDatabase.RemoveTreatyInfo(treatyInfo.ID);
					}
					if (treatyInfo.StartingTurn + treatyInfo.Duration < db.GetTurnCount() && treatyInfo.Type != TreatyType.Protectorate && (!treatyInfo.Removed && treatyInfo.Duration != 0))
					{
						this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_TREATY_EXPIRED,
							EventMessage = TurnEventMessage.EM_TREATY_EXPIRED,
							PlayerID = treatyInfo.InitiatingPlayerId,
							TreatyID = treatyInfo.ID,
							TurnNumber = this._app.GameDatabase.GetTurnCount(),
							ShowsDialog = true
						});
						this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_TREATY_EXPIRED,
							EventMessage = TurnEventMessage.EM_TREATY_EXPIRED,
							PlayerID = treatyInfo.ReceivingPlayerId,
							TreatyID = treatyInfo.ID,
							TurnNumber = this._app.GameDatabase.GetTurnCount(),
							ShowsDialog = true
						});
						db.RemoveTreatyInfo(treatyInfo.ID);
					}
				}
				else if (!treatyInfo.Active && !treatyInfo.Removed)
				{
					Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(db.GetPlayerInfo(treatyInfo.ReceivingPlayerId).ID);
					if (playerObject.IsAI())
					{
						if (playerObject.GetAI().HandleTreatyOffer(treatyInfo))
							this.AcceptTreaty(treatyInfo);
						else
							this.DeclineTreaty(treatyInfo);
					}
					else
						db.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_TREATY_REQUESTED,
							EventMessage = TurnEventMessage.EM_TREATY_REQUESTED,
							PlayerID = treatyInfo.ReceivingPlayerId,
							TreatyID = treatyInfo.ID,
							TurnNumber = db.GetTurnCount(),
							ShowsDialog = true
						});
				}
			}
		}

		public void AcceptIncentive(int sender, int receiver, TreatyIncentiveInfo tii)
		{
			PlayerInfo playerInfo1 = this._db.GetPlayerInfo(sender);
			PlayerInfo playerInfo2 = this._db.GetPlayerInfo(receiver);
			switch (tii.Type)
			{
				case IncentiveType.ResearchPoints:
					this._db.UpdatePlayerSavings(playerInfo1.ID, playerInfo1.Savings - (double)tii.IncentiveValue);
					this._db.UpdatePlayerAdditionalResearchPoints(receiver, playerInfo2.AdditionalResearchPoints + this.ConvertToResearchPoints(playerInfo2.ID, (double)tii.IncentiveValue));
					this.App.GameDatabase.ApplyDiplomacyReaction(receiver, sender, StratModifiers.DiplomacyReactionResearch, (int)Math.Min(tii.IncentiveValue / 20000f, 100f));
					break;
				case IncentiveType.Savings:
					this._db.UpdatePlayerSavings(playerInfo2.ID, playerInfo2.Savings + (double)tii.IncentiveValue);
					this._db.UpdatePlayerSavings(playerInfo1.ID, playerInfo1.Savings - (double)tii.IncentiveValue);
					this.App.GameDatabase.ApplyDiplomacyReaction(receiver, sender, StratModifiers.DiplomacyReactionMoney, (int)Math.Min(tii.IncentiveValue / 20000f, 100f));
					break;
			}
		}

		public void AcceptTreaty(TreatyInfo _treatyInfo)
		{
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_TREATY_ACCEPTED,
				EventMessage = TurnEventMessage.EM_TREATY_ACCEPTED,
				PlayerID = _treatyInfo.InitiatingPlayerId,
				TargetPlayerID = _treatyInfo.ReceivingPlayerId,
				TreatyID = _treatyInfo.ID,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				ShowsDialog = true
			});
			if (_treatyInfo.Type == TreatyType.Armistice)
			{
				ArmisticeTreatyInfo armisticeTreatyInfo = (ArmisticeTreatyInfo)_treatyInfo;
				this.GameDatabase.ChangeDiplomacyState(armisticeTreatyInfo.InitiatingPlayerId, armisticeTreatyInfo.ReceivingPlayerId, armisticeTreatyInfo.SuggestedDiplomacyState);
				this.GameDatabase.RemoveTreatyInfo(armisticeTreatyInfo.ID);
				if (armisticeTreatyInfo.SuggestedDiplomacyState == DiplomacyState.PEACE || armisticeTreatyInfo.SuggestedDiplomacyState == DiplomacyState.ALLIED)
				{
					foreach (int standardPlayerId in this.GameDatabase.GetStandardPlayerIDs())
					{
						if (this.App.GameDatabase.GetDiplomacyInfo(standardPlayerId, armisticeTreatyInfo.InitiatingPlayerId).isEncountered)
							this.App.GameDatabase.ApplyDiplomacyReaction(standardPlayerId, armisticeTreatyInfo.InitiatingPlayerId, StratModifiers.DiplomacyReactionPeaceTreaty, 1);
					}
				}
			}
			else if (_treatyInfo.Type == TreatyType.Incorporate)
			{
				foreach (ColonyInfo colony in this.GameDatabase.GetPlayerColoniesByPlayerId(_treatyInfo.ReceivingPlayerId).ToList<ColonyInfo>())
				{
					colony.PlayerID = _treatyInfo.InitiatingPlayerId;
					this.GameDatabase.UpdateColony(colony);
					OrbitalObjectInfo orbitalObjectInfo = this.GameDatabase.GetOrbitalObjectInfo(colony.OrbitalObjectID);
					this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_INDY_ASSIMILATED,
						EventMessage = TurnEventMessage.EM_INDY_ASSIMILATED,
						PlayerID = _treatyInfo.InitiatingPlayerId,
						TargetPlayerID = _treatyInfo.ReceivingPlayerId,
						SystemID = orbitalObjectInfo.StarSystemID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
				this.GameDatabase.SetPlayerDefeated(this, _treatyInfo.ReceivingPlayerId);
				this._app.GameDatabase.InsertGovernmentAction(_treatyInfo.InitiatingPlayerId, App.Localize("@GA_INDEPENDANTASSIMILATED"), "IndependantAssimilated", 0, 0);
			}
			else if (_treatyInfo.Type == TreatyType.Protectorate)
			{
				foreach (ColonyInfo colonyInfo in this.GameDatabase.GetPlayerColoniesByPlayerId(_treatyInfo.ReceivingPlayerId).ToList<ColonyInfo>())
				{
					OrbitalObjectInfo orbitalObjectInfo = this.GameDatabase.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID);
					this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_INDY_PROTECTORATE,
						EventMessage = TurnEventMessage.EM_INDY_PROTECTORATE,
						PlayerID = _treatyInfo.InitiatingPlayerId,
						TargetPlayerID = _treatyInfo.ReceivingPlayerId,
						SystemID = orbitalObjectInfo.StarSystemID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
			}
			this._app.GameDatabase.SetTreatyActive(_treatyInfo.ID);
		}

		public void DeclineTreaty(TreatyInfo _treatyInfo)
		{
			this._app.GameDatabase.RemoveTreatyInfo(_treatyInfo.ID);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_TREATY_DECLINED,
				EventMessage = TurnEventMessage.EM_TREATY_DECLINED,
				PlayerID = _treatyInfo.InitiatingPlayerId,
				TargetPlayerID = _treatyInfo.ReceivingPlayerId,
				TreatyID = _treatyInfo.ID,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				ShowsDialog = true
			});
		}

		public static bool CanRemoveShipFromDefenseFleet(DesignInfo di)
		{
			if (di == null)
				return true;
			bool flag = true;
			foreach (ShipSectionAsset shipSectionAsset in ((IEnumerable<DesignSectionInfo>)di.DesignSections).Select<DesignSectionInfo, ShipSectionAsset>((Func<DesignSectionInfo, ShipSectionAsset>)(x => x.ShipSectionAsset)).ToList<ShipSectionAsset>())
			{
				if (shipSectionAsset.CombatAIType == SectionEnumerations.CombatAiType.CommandMonitor || shipSectionAsset.CombatAIType == SectionEnumerations.CombatAiType.NormalMonitor)
				{
					flag = false;
					break;
				}
			}
			return flag;
		}

		public static void UpdatePlayerFleets(GameDatabase db, GameSession game)
		{
			List<PlayerInfo> list1 = db.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo playerInfo1 in list1)
			{
				List<FleetInfo> list2 = db.GetFleetInfosByPlayerID(playerInfo1.ID, FleetType.FL_DEFENSE).ToList<FleetInfo>();
				foreach (FleetInfo fleetInfo in list2)
				{
					if (!GameSession.CanPlayerSupportDefenseAssets(db, playerInfo1.ID, fleetInfo.SystemID))
					{
						List<ShipInfo> list3 = db.GetShipInfoByFleetID(fleetInfo.ID, true).ToList<ShipInfo>();
						List<ShipInfo> shipInfoList = new List<ShipInfo>();
						shipInfoList.AddRange((IEnumerable<ShipInfo>)list3);
						foreach (ShipInfo shipInfo in shipInfoList)
						{
							if (GameSession.CanRemoveShipFromDefenseFleet(shipInfo.DesignInfo))
							{
								db.RemoveShip(shipInfo.ID);
								list3.Remove(shipInfo);
							}
						}
						if (list3.Count == 0)
							db.RemoveFleet(fleetInfo.ID);
					}
				}
				List<int> list4 = db.GetPlayerColonySystemIDs(playerInfo1.ID).ToList<int>();
				List<FleetInfo> list5 = list2.ToList<FleetInfo>();
				foreach (int num in (IEnumerable<int>)list4)
				{
					int system = num;
					if (list5.Where<FleetInfo>((Func<FleetInfo, bool>)(x => x.SystemID == system)).Count<FleetInfo>() == 0)
						db.InsertDefenseFleet(playerInfo1.ID, system);
				}
				foreach (FleetInfo fleetInfo in db.GetFleetInfosByPlayerID(playerInfo1.ID, FleetType.FL_NORMAL).ToList<FleetInfo>())
				{
					FleetInfo fleet = fleetInfo;
					PlayerInfo playerInfo2 = list1.FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID == fleet.PlayerID));
					if (playerInfo2 != null && db.AssetDatabase.GetFaction(playerInfo2.FactionID).Name == "loa")
					{
						MissionInfo missionByFleetId1 = db.GetMissionByFleetID(fleet.ID);
						if (missionByFleetId1 == null || missionByFleetId1.Type != MissionType.RETURN && missionByFleetId1.Type != MissionType.RETREAT)
						{
							LoaFleetComposition fleetComposition = Kerberos.Sots.StarFleet.StarFleet.ObtainFleetComposition(game, fleet, fleet.FleetConfigID);
							if (fleetComposition != null && fleetComposition.designs.Count != 0)
							{
								MissionType mission_type = MissionType.NO_MISSION;
								if (missionByFleetId1 != null)
									mission_type = missionByFleetId1.Type;
								List<DesignInfo> list3 = Kerberos.Sots.StarFleet.StarFleet.GetDesignBuildOrderForComposition(game, fleet.ID, fleetComposition, mission_type).ToList<DesignInfo>();
								int fleetLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(game, fleet.ID);
								DesignInfo designInfo = list3.FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.GetCommandPoints() > 0));
								if (designInfo == null || fleetLoaCubeValue < designInfo.GetPlayerProductionCost(db, fleet.PlayerID, !designInfo.isPrototyped, new float?()))
								{
									Kerberos.Sots.StarFleet.StarFleet.CancelMission(game, fleet, true);
									MissionInfo missionByFleetId2 = db.GetMissionByFleetID(fleet.ID);
									if (missionByFleetId2 != null)
										game.GameDatabase.InsertWaypoint(missionByFleetId2.ID, WaypointType.DisbandFleet, new int?());
									else if (fleet.SystemID != 0)
									{
										int? reserveFleetId = db.GetReserveFleetID(fleet.PlayerID, fleet.SystemID);
										if (reserveFleetId.HasValue)
										{
											foreach (ShipInfo shipInfo in db.GetShipInfoByFleetID(fleet.ID, false).ToList<ShipInfo>())
											{
												db.UpdateShipAIFleetID(shipInfo.ID, new int?());
												db.TransferShip(shipInfo.ID, reserveFleetId.Value);
											}
											db.RemoveFleet(fleet.ID);
										}
										else
										{
											int newHomeSystem = db.FindNewHomeSystem(fleet);
											if (newHomeSystem != 0)
											{
												int missionID = db.InsertMission(fleet.ID, MissionType.RELOCATION, newHomeSystem, 0, 0, 1, false, new int?());
												db.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(newHomeSystem));
												db.InsertWaypoint(missionID, WaypointType.DisbandFleet, new int?());
											}
											else
												db.RemoveFleet(fleet.ID);
										}
									}
								}
							}
							else
								continue;
						}
						else
							continue;
					}
					if (db.GetAdmiralTraits(fleet.AdmiralID).Contains<AdmiralInfo.TraitType>(AdmiralInfo.TraitType.Sherman))
					{
						foreach (PlayerInfo playerInfo3 in list1)
						{
							if (playerInfo3 != playerInfo1)
								db.ApplyDiplomacyReaction(playerInfo3.ID, playerInfo1.ID, -2, new StratModifiers?(StratModifiers.DiplomacyReactionShermanAdmiral), 1);
						}
					}
				}
			}
		}

		public bool FleetAtSystem(FleetInfo f, int targetSystem)
		{
			return f.SystemID == targetSystem;
		}

		public int GetArrivalTurns(MoveOrderInfo moi, int fleetID)
		{
			FleetInfo fleetInfo = this.GameDatabase.GetFleetInfo(fleetID);
			Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(fleetInfo.PlayerID);
			if (playerObject.Faction.CanUseGate() && fleetInfo.SystemID != 0 && this.GameDatabase.SystemHasGate(moi.ToSystemID, playerObject.ID))
				return 1;
			bool nodeTravel = false;
			if (playerObject.Faction.CanUseNodeLine(new bool?()) && !playerObject.Faction.CanUseAccelerators())
			{
				if (playerObject.Faction.CanUseNodeLine(new bool?(false)))
					nodeTravel = this.GameDatabase.GetNodeLineBetweenSystems(playerObject.ID, moi.FromSystemID, moi.ToSystemID, false, false) != null;
				else if (playerObject.Faction.CanUseNodeLine(new bool?(true)))
					nodeTravel = this.GameDatabase.GetNodeLineBetweenSystems(playerObject.ID, moi.FromSystemID, moi.ToSystemID, true, false) != null;
			}
			FleetLocation fleetLocation = this.GameDatabase.GetFleetLocation(fleetID, false);
			float fleetTravelSpeed = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this, moi.FleetID, nodeTravel);
			return (int)Math.Ceiling((double)(fleetLocation.Coords - (moi.ToSystemID != 0 ? this.GameDatabase.GetStarSystemOrigin(moi.ToSystemID) : moi.ToCoords)).Length / (double)fleetTravelSpeed);
		}

		public void ProcessWaypoint(WaypointInfo wi, FleetInfo fi, bool useDirectRoute)
		{
			bool flag = false;
			if (wi == null || fi.Type == FleetType.FL_ACCELERATOR)
				return;
			if (this._app.AssetDatabase.GetFaction(this.GameDatabase.GetPlayerInfo(fi.PlayerID).FactionID).CanUseAccelerators())
			{
				if (fi.SystemID != 0)
				{
					if (this._app.GameDatabase.GetFleetsByPlayerAndSystem(fi.PlayerID, fi.SystemID, FleetType.FL_ACCELERATOR).Any<FleetInfo>())
					{
						this._app.GameDatabase.UpdateFleetAccelerated(this, fi.ID, new int?());
						fi.LastTurnAccelerated = this._app.GameDatabase.GetTurnCount();
					}
				}
				else if (fi.SystemID == 0 && this._app.GameDatabase.IsInAccelRange(fi.ID))
				{
					this._app.GameDatabase.UpdateFleetAccelerated(this, fi.ID, new int?());
					fi.LastTurnAccelerated = this._app.GameDatabase.GetTurnCount();
				}
			}
			switch (wi.Type)
			{
				case WaypointType.TravelTo:
				case WaypointType.ReturnHome:
					MissionInfo missionByFleetId1 = this._db.GetMissionByFleetID(fi.ID);
					if (fi.Type != FleetType.FL_CARAVAN && missionByFleetId1 != null && (missionByFleetId1.Type == MissionType.RELOCATION && this.GameDatabase.GetRemainingSupportPoints(this, missionByFleetId1.TargetSystemID, fi.PlayerID) < 0))
					{
						Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fi, true);
						flag = true;
						break;
					}
					flag = this.MoveFleet(wi, fi, useDirectRoute);
					if (flag && this.App.Game.ScriptModules != null && (this.App.Game.ScriptModules.Swarmers != null && this.App.Game.ScriptModules.Swarmers.PlayerID == fi.PlayerID))
					{
						SwarmerInfo si = this.App.GameDatabase.GetSwarmerInfos().FirstOrDefault<SwarmerInfo>((Func<SwarmerInfo, bool>)(x =>
					   {
						   if (x.QueenFleetId.HasValue)
							   return x.QueenFleetId.Value == fi.ID;
						   return false;
					   }));
						if (si != null)
							Swarmers.SetInitialSwarmerPosition(this, si, fi.SystemID);
					}
					MoveOrderInfo orderInfoByFleetId = this._db.GetMoveOrderInfoByFleetID(fi.ID);
					if (orderInfoByFleetId != null)
					{
						List<int> intList = new List<int>();
						using (List<ColonyInfo>.Enumerator enumerator = this._db.GetColonyInfosForSystem(orderInfoByFleetId.ToSystemID).ToList<ColonyInfo>().GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								ColonyInfo current = enumerator.Current;
								DiplomacyState stateBetweenPlayers = this._db.GetDiplomacyStateBetweenPlayers(current.PlayerID, fi.PlayerID);
								if (current.PlayerID != fi.PlayerID && !intList.Contains(current.PlayerID) && (this._db.GetPlayerInfo(current.PlayerID).isStandardPlayer && stateBetweenPlayers != DiplomacyState.NON_AGGRESSION) && (stateBetweenPlayers != DiplomacyState.PEACE && stateBetweenPlayers != DiplomacyState.ALLIED && StarMap.IsInRange(this._db, current.PlayerID, this._db.GetFleetLocation(fi.ID, false).Coords, 1f, (Dictionary<int, List<ShipInfo>>)null)))
								{
									intList.Add(current.PlayerID);
									if (fi.PlayerID == this.App.Game.ScriptModules.Locust.PlayerID)
										this._db.InsertTurnEvent(new TurnEvent()
										{
											EventType = TurnEventType.EV_LOCUST_INCOMING,
											EventMessage = TurnEventMessage.EM_LOCUST_INCOMING,
											PlayerID = current.PlayerID,
											SystemID = orderInfoByFleetId.ToSystemID,
											ArrivalTurns = this.GetArrivalTurns(orderInfoByFleetId, fi.ID),
											TurnNumber = this._db.GetTurnCount()
										});
									else if (fi.PlayerID == this.App.Game.ScriptModules.Swarmers.PlayerID)
										this._db.InsertTurnEvent(new TurnEvent()
										{
											EventType = TurnEventType.EV_SWARM_QUEEN_INCOMING,
											EventMessage = TurnEventMessage.EM_SWARM_QUEEN_INCOMING,
											PlayerID = current.PlayerID,
											SystemID = orderInfoByFleetId.ToSystemID,
											ArrivalTurns = this.GetArrivalTurns(orderInfoByFleetId, fi.ID),
											TurnNumber = this._db.GetTurnCount()
										});
									else if (fi.PlayerID == this.App.Game.ScriptModules.SystemKiller.PlayerID)
										this._db.InsertTurnEvent(new TurnEvent()
										{
											EventType = TurnEventType.EV_SYS_KILLER_INCOMING,
											EventMessage = TurnEventMessage.EM_SYS_KILLER_INCOMING,
											PlayerID = current.PlayerID,
											SystemID = orderInfoByFleetId.ToSystemID,
											ArrivalTurns = this.GetArrivalTurns(orderInfoByFleetId, fi.ID),
											TurnNumber = this._db.GetTurnCount()
										});
									else
										this._db.InsertTurnEvent(new TurnEvent()
										{
											EventType = TurnEventType.EV_INCOMING_ALIEN_FLEET,
											EventMessage = TurnEventMessage.EM_INCOMING_ALIEN_FLEET,
											PlayerID = current.PlayerID,
											SystemID = orderInfoByFleetId.ToSystemID,
											FleetID = fi.ID,
											TargetPlayerID = fi.PlayerID,
											ArrivalTurns = this.GetArrivalTurns(orderInfoByFleetId, fi.ID),
											TurnNumber = this._db.GetTurnCount()
										});
								}
							}
							break;
						}
					}
					else
						break;
				case WaypointType.DoMission:
					MissionInfo missionByFleetId2 = this._db.GetMissionByFleetID(fi.ID);
					if (this.GetPlayerObject(fi.PlayerID).IsAI() && this._app.AssetDatabase.GetFaction(this._db.GetPlayerInfo(fi.PlayerID).FactionID).Name == "loa" && missionByFleetId2.Type != MissionType.DEPLOY_NPG)
						Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromCompositionID(this._app.Game, fi.ID, new int?(this.GetPlayerObject(fi.PlayerID).GetAI().GetLoaCompositionIDByMission(missionByFleetId2.Type)), MissionType.NO_MISSION);
					switch (missionByFleetId2.Type)
					{
						case MissionType.COLONIZATION:
							flag = this.DoColonizationMission(missionByFleetId2, fi);
							break;
						case MissionType.SUPPORT:
							flag = this.DoSupportMission(missionByFleetId2, fi);
							break;
						case MissionType.SURVEY:
							flag = this.DoSurveyMission(missionByFleetId2, fi);
							break;
						case MissionType.RELOCATION:
							flag = this.DoRelocationMission(missionByFleetId2, fi);
							break;
						case MissionType.CONSTRUCT_STN:
						case MissionType.UPGRADE_STN:
							flag = this.DoConstructionMission(missionByFleetId2, fi);
							break;
						case MissionType.PATROL:
							flag = this.DoPatrolMission(missionByFleetId2, fi);
							break;
						case MissionType.INTERDICTION:
							flag = this.DoInterdictMission(missionByFleetId2, fi);
							break;
						case MissionType.STRIKE:
							flag = true;
							break;
						case MissionType.INVASION:
							flag = this.DoInvasionMission(missionByFleetId2, fi);
							break;
						case MissionType.INTERCEPT:
							flag = this.DoInterceptMission(missionByFleetId2, wi, fi);
							break;
						case MissionType.GATE:
							flag = this.DoGateMission(missionByFleetId2, fi);
							break;
						case MissionType.PIRACY:
							flag = this.DoPiracyMission(missionByFleetId2, fi);
							break;
						case MissionType.DEPLOY_NPG:
							flag = this.DoDeployNPGMission(missionByFleetId2, fi);
							break;
						case MissionType.EVACUATE:
							flag = this.DoEvacuationMission(missionByFleetId2, fi);
							break;
						case MissionType.SPECIAL_CONSTRUCT_STN:
							flag = this.DoSpecialConstructionMission(missionByFleetId2, fi);
							break;
					}
					if (flag && fi.Type != FleetType.FL_CARAVAN && this._app.GameDatabase.GetFleetInfo(fi.ID) != null)
					{
						if (this._db.GetPlayerInfo(fi.PlayerID).AutoPatrol && missionByFleetId2.TargetSystemID != 0)
						{
							if (Kerberos.Sots.StarFleet.StarFleet.DoAutoPatrol(this, fi, missionByFleetId2))
								return;
							break;
						}
						if (Kerberos.Sots.StarFleet.StarFleet.SetReturnMission(this, fi, missionByFleetId2))
							return;
						break;
					}
					break;
				case WaypointType.DisbandFleet:
					if (Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(this._db, fi))
					{
						ShipInfo fleetSuulkaShipInfo = Kerberos.Sots.StarFleet.StarFleet.GetFleetSuulkaShipInfo(this._db, fi);
						SuulkaInfo suulkaByShipId = this._db.GetSuulkaByShipID(fleetSuulkaShipInfo.ID);
						if (suulkaByShipId != null)
						{
							fi.AdmiralID = suulkaByShipId.AdmiralID;
							fi.Name = fleetSuulkaShipInfo.DesignInfo.Name;
							this._db.UpdateFleetInfo(fi);
							flag = true;
						}
						else
							this._db.RemoveShip(fleetSuulkaShipInfo.ID);
					}
					if (!flag)
					{
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_FLEET_DISBANDED,
							EventMessage = TurnEventMessage.EM_FLEET_DISBANDED,
							PlayerID = fi.PlayerID,
							FleetID = fi.ID,
							SystemID = fi.SystemID,
							TurnNumber = this.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
						int? reserveFleetId = this._db.GetReserveFleetID(fi.PlayerID, fi.SystemID);
						if (reserveFleetId.HasValue)
						{
							foreach (ShipInfo shipInfo in this._db.GetShipInfoByFleetID(fi.ID, false).ToList<ShipInfo>())
							{
								this._db.UpdateShipAIFleetID(shipInfo.ID, new int?());
								this._db.TransferShip(shipInfo.ID, reserveFleetId.Value);
							}
							flag = true;
						}
						this._db.RemoveFleet(fi.ID);
						break;
					}
					break;
				case WaypointType.CheckSupportColony:
					MissionInfo missionByFleetId3 = this._db.GetMissionByFleetID(fi.ID);
					if (missionByFleetId3.Type != MissionType.SUPPORT)
					{
						missionByFleetId3.Type = MissionType.SUPPORT;
						this._db.UpdateMission(missionByFleetId3);
					}
					this.CompleteColonizationMission(missionByFleetId3, fi);
					flag = true;
					break;
				case WaypointType.CheckEvacuate:
					MissionInfo missionByFleetId4 = this._db.GetMissionByFleetID(fi.ID);
					if (missionByFleetId4.Type != MissionType.EVACUATE)
					{
						missionByFleetId4.Type = MissionType.EVACUATE;
						this._db.UpdateMission(missionByFleetId4);
					}
					this.CompleteEvecuateMission(missionByFleetId4, fi);
					flag = true;
					break;
				case WaypointType.Intercepted:
					flag = true;
					break;
				case WaypointType.ReBase:
					flag = this.DoRelocationMission((MissionInfo)null, fi);
					break;
			}
			if (!flag)
				return;
			this._db.RemoveWaypoint(wi.ID);
		}

		public int GetNumSupportTrips(int fleetId, int systemId)
		{
			FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(fleetId);
			if (fleetInfo == null)
				return 0;
			float sensorTravelDistance = Kerberos.Sots.StarFleet.StarFleet.GetSensorTravelDistance(this._app.Game, fleetInfo.SystemID, systemId, fleetInfo.ID);
			float fleetRange = Kerberos.Sots.StarFleet.StarFleet.GetFleetRange(this._app.Game, fleetInfo);
			if ((double)sensorTravelDistance == 0.0)
				return 5;
			return Math.Max((int)Math.Floor((double)fleetRange / (double)sensorTravelDistance), 1);
		}

		public int GetNumSupportTrips(MissionInfo mission)
		{
			if (mission == null)
				return 1;
			return this.GetNumSupportTrips(mission.FleetID, mission.TargetSystemID);
		}

		public bool ProcessMoveOrder(MoveOrderInfo moi, FleetInfo fi, ref float remainingNodeDistance)
		{
			if (Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(this.App.GameDatabase, this.GameDatabase.GetFleetInfo(moi.FleetID)))
			{
				moi.Progress = 1.1f;
				if (fi != null && moi.ToSystemID != 0)
				{
					fi.SupportingSystemID = moi.ToSystemID;
					this.GameDatabase.UpdateFleetInfo(fi);
				}
				return true;
			}
			if (moi.ToSystemID != 0 && fi.SystemID == moi.ToSystemID)
				return true;
			Faction faction = this._app.AssetDatabase.GetFaction(this._db.GetPlayerFactionID(fi.PlayerID));
			if (!faction.CanUseGate() || !this._db.SystemHasGate(moi.FromSystemID, fi.PlayerID) || (double)moi.Progress != 0.0)
			{
				if (faction.CanUseNodeLine(new bool?(true)) && this._db.GetNodeLineBetweenSystems(fi.PlayerID, moi.FromSystemID, moi.ToSystemID, true, false) != null)
					return this.UpdateLinearMove(moi, fi, true, false, false, ref remainingNodeDistance);
				if (faction.CanUseNodeLine(new bool?(false)) && this._db.GetNodeLineBetweenSystems(fi.PlayerID, moi.FromSystemID, moi.ToSystemID, false, false) != null)
					return this.UpdateLinearMove(moi, fi, false, true, false, ref remainingNodeDistance);
				return this.UpdateLinearMove(moi, fi, false, false, false, ref remainingNodeDistance);
			}
			if (!this._db.SystemHasGate(moi.ToSystemID, fi.PlayerID))
				return this.UpdateLinearMove(moi, fi, false, false, true, ref remainingNodeDistance);
			int cruiserEquivalent = this._db.GetFleetCruiserEquivalent(moi.FleetID);
			int num = 0;
			this._playerGateMap.TryGetValue(fi.PlayerID, out num);
			if (cruiserEquivalent > num)
			{
				this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_GATE_CAPACITY_REACHED,
					EventMessage = TurnEventMessage.EM_GATE_CAPACITY_REACHED,
					PlayerID = fi.PlayerID,
					SystemID = moi.FromSystemID,
					FleetID = fi.ID,
					ShowsDialog = false,
					TurnNumber = this.App.GameDatabase.GetTurnCount()
				});
				return false;
			}
			Dictionary<int, int> playerGateMap;
			int playerId;
			(playerGateMap = this._playerGateMap)[playerId = fi.PlayerID] = playerGateMap[playerId] - cruiserEquivalent;
			return true;
		}

		public MoveOrderInfo GetNextMoveOrder(
		  WaypointInfo wi,
		  FleetInfo fi,
		  bool useDirectRoute)
		{
			if (this._db.GetMoveOrderInfoByFleetID(fi.ID) == null)
			{
				int toSystem = 0;
				if (wi.Type == WaypointType.ReturnHome)
					toSystem = this._db.GetHomeSystem(this, wi.MissionID, fi);
				else if (wi.SystemID.HasValue)
					toSystem = wi.SystemID.Value;
				if (fi.SystemID == toSystem || toSystem == 0)
					return (MoveOrderInfo)null;
				if (toSystem == 0)
				{
					this._db.RemoveFleet(fi.ID);
					return (MoveOrderInfo)null;
				}
				int tripTime;
				float tripDistance;
				List<int> bestTravelPath = Kerberos.Sots.StarFleet.StarFleet.GetBestTravelPath(this, fi.ID, fi.SystemID, toSystem, out tripTime, out tripDistance, useDirectRoute, new float?(), new float?());
				if (bestTravelPath[0] == 0)
					this._db.InsertMoveOrder(fi.ID, 0, this._db.GetFleetLocation(fi.ID, false).Coords, bestTravelPath[1], Vector3.Zero);
				else
					this._db.InsertMoveOrder(fi.ID, bestTravelPath[0], Vector3.Zero, bestTravelPath[1], Vector3.Zero);
			}
			return this._db.GetMoveOrderInfoByFleetID(fi.ID);
		}

		public bool MoveFleet(WaypointInfo wi, FleetInfo fleet, bool useDirectRoute)
		{
			int systemID = 0;
			float fleetTravelSpeed = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this, fleet.ID, true);
			if (this.IsFleetIntercepted(fleet))
				systemID = fleet.SystemID;
			MoveOrderInfo nextMoveOrder = this.GetNextMoveOrder(wi, fleet, useDirectRoute);
			bool flag = true;
			while (nextMoveOrder != null & flag)
			{
				if (systemID != 0)
					this.GameDatabase.UpdateFleetLocation(fleet.ID, systemID, new int?());
				flag = this.ProcessMoveOrder(nextMoveOrder, fleet, ref fleetTravelSpeed);
				if (flag)
				{
					this.FinishMove(nextMoveOrder, fleet);
					nextMoveOrder = this.GetNextMoveOrder(wi, fleet, useDirectRoute);
					if (nextMoveOrder != null && this.isHostilesAtSystem(fleet.PlayerID, nextMoveOrder.FromSystemID) || this._db.AssetDatabase.GetFaction(this._db.GetPlayerFactionID(fleet.PlayerID)).CanUseGate() && this._db.GetMissionByFleetID(fleet.ID).TargetSystemID != fleet.SystemID)
					{
						flag = false;
						break;
					}
				}
			}
			return nextMoveOrder == null & flag;
		}

		public bool isHostilesAtSystem(int playerId, int systemId)
		{
			List<StationInfo> list1 = this.App.GameDatabase.GetStationForSystem(systemId).ToList<StationInfo>();
			List<FleetInfo> list2 = this.App.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			List<ColonyInfo> list3 = this.App.GameDatabase.GetColonyInfosForSystem(systemId).ToList<ColonyInfo>();
			List<EncounterInfo> list4 = this._db.GetEncounterInfos().ToList<EncounterInfo>();
			List<AsteroidMonitorInfo> source1 = new List<AsteroidMonitorInfo>();
			List<MorrigiRelicInfo> source2 = new List<MorrigiRelicInfo>();
			foreach (EncounterInfo encounterInfo in list4)
			{
				if (encounterInfo.Type == EasterEgg.EE_ASTEROID_MONITOR)
				{
					AsteroidMonitorInfo asteroidMonitorInfo = this._app.GameDatabase.GetAsteroidMonitorInfo(encounterInfo.Id);
					if (asteroidMonitorInfo != null)
						source1.Add(asteroidMonitorInfo);
				}
				else if (encounterInfo.Type == EasterEgg.EE_MORRIGI_RELIC)
				{
					MorrigiRelicInfo morrigiRelicInfo = this._app.GameDatabase.GetMorrigiRelicInfo(encounterInfo.Id);
					if (morrigiRelicInfo != null)
						source2.Add(morrigiRelicInfo);
				}
			}
			int num1 = this._app.Game.ScriptModules.AsteroidMonitor != null ? this._app.Game.ScriptModules.AsteroidMonitor.PlayerID : 0;
			int num2 = this._app.Game.ScriptModules.MorrigiRelic != null ? this._app.Game.ScriptModules.MorrigiRelic.PlayerID : 0;
			List<int> intList = new List<int>();
			foreach (StationInfo stationInfo in list1)
			{
				if (!intList.Contains(stationInfo.PlayerID) && stationInfo.PlayerID != playerId)
					intList.Add(stationInfo.PlayerID);
			}
			foreach (FleetInfo fleetInfo in list2)
			{
				if (fleetInfo.PlayerID != playerId && !intList.Contains(fleetInfo.PlayerID) && (!fleetInfo.IsReserveFleet && !fleetInfo.IsDefenseFleet) && (!fleetInfo.IsLimboFleet && !fleetInfo.IsTrapFleet))
				{
					bool flag = true;
					if (fleetInfo.PlayerID == num1)
					{
						AsteroidMonitorInfo asteroidMonitorInfo = source1.FirstOrDefault<AsteroidMonitorInfo>((Func<AsteroidMonitorInfo, bool>)(x => x.SystemId == systemId));
						if (asteroidMonitorInfo != null && !asteroidMonitorInfo.IsAggressive)
							flag = false;
					}
					else if (fleetInfo.PlayerID == num2)
					{
						MorrigiRelicInfo morrigiRelicInfo = source2.FirstOrDefault<MorrigiRelicInfo>((Func<MorrigiRelicInfo, bool>)(x => x.SystemId == systemId));
						if (morrigiRelicInfo != null && !morrigiRelicInfo.IsAggressive)
							flag = false;
					}
					if (flag)
						intList.Add(fleetInfo.PlayerID);
				}
			}
			foreach (ColonyInfo colonyInfo in list3)
			{
				if (!intList.Contains(colonyInfo.PlayerID) && colonyInfo.PlayerID != playerId)
					intList.Add(colonyInfo.PlayerID);
			}
			Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(playerId);
			foreach (int playerID in intList)
			{
				if ((playerID != num2 || !playerObject.InstantDefeatMorrigiRelics()) && this.App.GameDatabase.GetDiplomacyStateBetweenPlayers(playerID, playerId) == DiplomacyState.WAR)
					return true;
			}
			return false;
		}

		public bool isTrapAtPlanet(int systemId, int planetId, int playerId)
		{
			ColonyTrapInfo systemIdAndPlanetId = this.GameDatabase.GetColonyTrapInfoBySystemIDAndPlanetID(systemId, planetId);
			if (systemIdAndPlanetId == null)
				return false;
			FleetInfo fleetInfo = this.GameDatabase.GetFleetInfo(systemIdAndPlanetId.FleetID);
			return fleetInfo != null && fleetInfo.PlayerID != playerId && this.GameDatabase.GetDiplomacyStateBetweenPlayers(playerId, fleetInfo.PlayerID) != DiplomacyState.ALLIED;
		}

		public int IsIndependtRacePresent(int playerId, int systemId, out int planetid)
		{
			foreach (ColonyInfo colonyInfo in this.App.GameDatabase.GetColonyInfosForSystem(systemId).ToList<ColonyInfo>())
			{
				if (colonyInfo.PlayerID != playerId)
				{
					PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(colonyInfo.PlayerID);
					if (!playerInfo.isStandardPlayer && this.App.AssetDatabase.GetFaction(playerInfo.FactionID).IsIndependent())
					{
						planetid = colonyInfo.ID;
						return playerInfo.ID;
					}
				}
			}
			planetid = 0;
			return 0;
		}

		public bool DoSurveyMission(MissionInfo mission, FleetInfo fleet)
		{
			if (this.ScriptModules.Locust != null && this.ScriptModules.Locust.PlayerID == fleet.PlayerID)
			{
				this.ScriptModules.Locust.UpdateScoutedSystems(this, fleet.SystemID);
				return true;
			}
			if (this.isHostilesAtSystem(fleet.PlayerID, fleet.SystemID))
				return false;
			MorrigiRelicInfo relicInfo = this.GameDatabase.GetMorrigiRelicInfos().FirstOrDefault<MorrigiRelicInfo>((Func<MorrigiRelicInfo, bool>)(x => x.SystemId == mission.TargetSystemID));
			if (relicInfo != null)
			{
				Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(fleet.PlayerID);
				if (playerObject.InstantDefeatMorrigiRelics())
					this.ScriptModules.MorrigiRelic.ApplyRewardsToPlayers(this.App, relicInfo, this.GameDatabase.GetShipInfoByFleetID(relicInfo.FleetId, true).ToList<ShipInfo>(), new List<Kerberos.Sots.PlayerFramework.Player>()
		  {
			playerObject
		  });
			}
			int num1 = (int)((double)Kerberos.Sots.StarFleet.StarFleet.GetFleetSurveyPoints(this._db, fleet.ID) * (double)this._db.GetStratModifierFloatToApply(StratModifiers.SurveyTimeModifier, fleet.PlayerID));
			mission.Duration -= num1;
			this._db.UpdateMission(mission);
			if (mission.Duration <= 0)
			{
				int turnCount = this._db.GetTurnCount();
				this._db.InsertExploreRecord(mission.TargetSystemID, fleet.PlayerID, turnCount, true, true);
				this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_SYSTEM_SURVEYED,
					EventMessage = TurnEventMessage.EM_SYSTEM_SURVEYED,
					PlayerID = fleet.PlayerID,
					SystemID = mission.TargetSystemID,
					FleetID = fleet.ID,
					TurnNumber = this.App.GameDatabase.GetTurnCount(),
					ShowsDialog = true
				});
				int planetid;
				int num2 = this.IsIndependtRacePresent(fleet.PlayerID, fleet.SystemID, out planetid);
				if (num2 != 0 && planetid != 0)
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_SURVEY_INDEPENDENT_RACE_FOUND,
						EventMessage = TurnEventMessage.EM_SURVEY_INDEPENDENT_RACE_FOUND,
						PlayerID = fleet.PlayerID,
						TargetPlayerID = num2,
						SystemID = mission.TargetSystemID,
						FleetID = fleet.ID,
						ColonyID = planetid,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = true
					});
				if (fleet.PlayerID == this.LocalPlayer.ID)
					this.App.SteamHelper.DoAchievement(AchievementType.SOTS2_TO_BOLDY_GO);
				Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(fleet.PlayerID);
				if (playerObject != null && playerObject.IsAI() && playerObject.IsStandardPlayer)
					playerObject.GetAI().HandleSurveyMissionCompleted(fleet.ID, mission.TargetSystemID);
				GameSession.Trace("Fleet " + (object)fleet.ID + " has finished surveying system " + (object)mission.TargetSystemID);
				return true;
			}
			if (num1 == 0)
				return false;
			int num3 = mission.Duration / num1;
			if (mission.Duration % num1 != 0)
				++num3;
			GameSession.Trace("Fleet " + (object)fleet.ID + " surveying system " + (object)mission.TargetSystemID + ", " + (object)num3 + " turns left.");
			return false;
		}

		public void CheckLoaFleetGateCompliancy(FleetInfo fi)
		{
			if (!(this._app.AssetDatabase.GetFaction(this._db.GetPlayerInfo(fi.PlayerID).FactionID).Name == "loa"))
				return;
			int cubeMassForTransit = Kerberos.Sots.StarFleet.StarFleet.GetMaxLoaFleetCubeMassForTransit(this, fi.PlayerID);
			int fleetLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this, fi.ID);
			if (fleetLoaCubeValue <= cubeMassForTransit)
				return;
			int Loacubes = fleetLoaCubeValue - cubeMassForTransit;
			ShipInfo shipInfo1 = this._app.GameDatabase.GetShipInfoByFleetID(fi.ID, false).FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.IsLoaCube()));
			if (shipInfo1 != null && this._app.GameDatabase.GetShipInfoByFleetID(fi.ID, false).Count<ShipInfo>() == 1)
				this._app.GameDatabase.UpdateShipLoaCubes(shipInfo1.ID, fleetLoaCubeValue - Loacubes);
			else
				this._app.GameDatabase.UpdateShipLoaCubes(Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this, fi.ID), fleetLoaCubeValue - Loacubes);
			if (fi.SystemID != 0)
			{
				FleetInfo fleetInfo = this._app.GameDatabase.GetFleetsByPlayerAndSystem(fi.PlayerID, fi.SystemID, FleetType.FL_NORMAL | FleetType.FL_RESERVE).FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(x => x.ID != fi.ID));
				if (fleetInfo == null)
				{
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_LOA_CUBES_ABANDONED,
						EventMessage = TurnEventMessage.EM_LOA_CUBES_ABANDONED,
						FleetID = fi.ID,
						PlayerID = fi.PlayerID,
						SystemID = fi.SystemID,
						Savings = (double)Loacubes,
						TurnNumber = this._app.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
				else
				{
					ShipInfo shipInfo2 = this._app.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false).FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.IsLoaCube()));
					if (shipInfo2 != null)
					{
						this._app.GameDatabase.UpdateShipLoaCubes(shipInfo2.ID, shipInfo2.LoaCubes + Loacubes);
					}
					else
					{
						DesignInfo designInfo = this._app.GameDatabase.GetDesignInfosForPlayer(fi.PlayerID).FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.IsLoaCube()));
						if (designInfo == null)
							return;
						this._app.GameDatabase.InsertShip(fleetInfo.ID, designInfo.ID, "Cube", (ShipParams)0, new int?(), Loacubes);
					}
				}
			}
			else
				this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_LOA_CUBES_ABANDONED_DEEPSPACE,
					EventMessage = TurnEventMessage.EM_LOA_CUBES_ABANDONED_DEEPSPACE,
					FleetID = fi.ID,
					PlayerID = fi.PlayerID,
					Savings = (double)Loacubes,
					TurnNumber = this._app.GameDatabase.GetTurnCount(),
					ShowsDialog = false
				});
		}

		public void SetAColonyTrap(ShipInfo trapShip, int playerID, int systemID, int planetID)
		{
			ColonyTrapInfo systemIdAndPlanetId = this.GameDatabase.GetColonyTrapInfoBySystemIDAndPlanetID(systemID, planetID);
			if (systemIdAndPlanetId != null)
				this.GameDatabase.RemoveColonyTrapInfo(systemIdAndPlanetId.ID);
			Matrix? nullable = new Matrix?(this._db.GetOrbitalTransform(planetID));
			PlanetInfo planetInfo = this._db.GetPlanetInfo(planetID);
			float num1 = planetInfo != null ? StarSystemVars.Instance.SizeToRadius(planetInfo.Size) : 0.0f;
			int num2 = this._db.InsertFleet(playerID, 0, systemID, systemID, "TRAP", FleetType.FL_TRAP);
			DesignInfo designInfo = this._db.GetVisibleDesignInfosForPlayerAndRole(playerID, ShipRole.TRAPDRONE, true).FirstOrDefault<DesignInfo>();
			if (designInfo == null)
				return;
			int num3 = 10;
			for (int index = 0; index < num3; ++index)
			{
				int shipID = this._db.InsertShip(num2, designInfo.ID, null, (ShipParams)0, new int?(), 0);
				this._db.TransferShip(shipID, num2);
				Matrix matrix = Matrix.Identity;
				Vector3 vector3 = new Vector3();
				vector3.X = this.Random.NextInclusive(-1f, 1f);
				vector3.Y = this.Random.NextInclusive(-1f, 1f);
				vector3.Z = this.Random.NextInclusive(-1f, 1f);
				if ((double)vector3.LengthSquared > 1.40129846432482E-45)
				{
					double num4 = (double)vector3.Normalize();
					matrix.Position = vector3 * this.Random.NextInclusive(0.0f, num1 * 0.75f);
					matrix = Matrix.CreateWorld(matrix.Position, -Vector3.Normalize(matrix.Position), Vector3.UnitY);
					if (nullable.HasValue)
						matrix *= nullable.Value;
				}
				this._db.UpdateShipSystemPosition(shipID, new Matrix?(matrix));
			}
			this._db.InsertColonyTrap(systemID, planetID, num2);
			this._db.RemoveShip(trapShip.ID);
		}

		private Vector3 GetBestInterceptPoint(FleetInfo interceptingFleet, FleetInfo targetFleet)
		{
			MoveOrderInfo orderInfoByFleetId = this._db.GetMoveOrderInfoByFleetID(targetFleet.ID);
			this._db.GetMoveOrderInfoByFleetID(interceptingFleet.ID);
			Vector3 coords1 = this._db.GetFleetLocation(targetFleet.ID, true).Coords;
			Vector3 coords2 = this._db.GetFleetLocation(interceptingFleet.ID, false).Coords;
			if (orderInfoByFleetId == null || targetFleet.IsAcceleratorFleet)
				return coords1;
			float fleetTravelSpeed1 = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this, targetFleet.ID, false);
			float num = fleetTravelSpeed1 * 0.5f;
			Vector3 vector3_1 = orderInfoByFleetId.ToSystemID != 0 ? this._db.GetStarSystemOrigin(orderInfoByFleetId.ToSystemID) : orderInfoByFleetId.ToCoords;
			Vector3 vector3_2 = vector3_1 - coords1;
			float val2 = vector3_2.Normalize();
			if ((double)val2 < (double)num)
				return vector3_1;
			float fleetTravelSpeed2 = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this, interceptingFleet.ID, false);
			Vector3 vector3_3 = coords1 + vector3_2 * Math.Min(fleetTravelSpeed1, val2);
			Vector3 vector3_4 = coords1 + vector3_2 * num;
			return (double)(coords2 - vector3_4).LengthSquared < (double)fleetTravelSpeed2 * (double)fleetTravelSpeed2 ? vector3_4 : vector3_3;
		}

		public bool DoInterceptMission(MissionInfo mission, WaypointInfo waypoint, FleetInfo fleet)
		{
			FleetInfo fi = this._db.GetFleetInfo(mission.TargetFleetID);
			if (fi == null)
				return true;
			this.AssetDatabase.GetFaction(this._db.GetPlayerInfo(fi.PlayerID).FactionID);
			this.AssetDatabase.GetFaction(this._db.GetPlayerInfo(fleet.PlayerID).FactionID);
			MoveOrderInfo orderInfoByFleetId1 = this._db.GetMoveOrderInfoByFleetID(fleet.ID);
			Vector3 coords1 = this._db.GetFleetLocation(fi.ID, true).Coords;
			Vector3 targetPosition = coords1;
			Vector3 coords2 = this._db.GetFleetLocation(fleet.ID, false).Coords;
			Vector3 bestInterceptPoint = this.GetBestInterceptPoint(fleet, fi);
			if (!StarMap.IsInRange(this._db, fleet.PlayerID, targetPosition, 1f, (Dictionary<int, List<ShipInfo>>)null) || Kerberos.Sots.StarFleet.StarFleet.IsFleetExhausted(this, fleet))
			{
				if (orderInfoByFleetId1 != null)
				{
					this._db.RemoveMoveOrder(orderInfoByFleetId1.ID);
					this._db.InsertMoveOrder(orderInfoByFleetId1.FleetID, 0, coords2, fleet.SupportingSystemID, Vector3.Zero);
				}
				return true;
			}
			if ((double)(coords2 - bestInterceptPoint).Length < (double)Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this, fleet.ID, false))
				targetPosition = bestInterceptPoint;
			if (orderInfoByFleetId1 != null)
				this._db.RemoveMoveOrder(orderInfoByFleetId1.ID);
			this._db.InsertMoveOrder(fleet.ID, 0, coords2, 0, targetPosition);
			float remainingNodeDistance = 0.0f;
			MoveOrderInfo orderInfoByFleetId2 = this._db.GetMoveOrderInfoByFleetID(fleet.ID);
			if (!this.ProcessMoveOrder(orderInfoByFleetId2, fleet, ref remainingNodeDistance))
				return false;
			PendingCombat pendingCombat = new PendingCombat();
			bool flag1 = this.App.Game.ScriptModules.NeutronStar != null && this.App.Game.ScriptModules.NeutronStar.PlayerID == fi.PlayerID;
			bool flag2 = this.App.Game.ScriptModules.Gardeners != null && this.App.Game.ScriptModules.Gardeners.PlayerID == fi.PlayerID;
			if (flag1 || flag2)
			{
				if (flag1)
				{
					NeutronStarInfo neutronStarInfo = this.App.GameDatabase.GetNeutronStarInfos().FirstOrDefault<NeutronStarInfo>((Func<NeutronStarInfo, bool>)(x => x.FleetId == fi.ID));
					if (neutronStarInfo != null && neutronStarInfo.DeepSpaceSystemId.HasValue)
						pendingCombat.SystemID = neutronStarInfo.DeepSpaceSystemId.Value;
				}
				else
				{
					GardenerInfo gardenerInfo = this.App.GameDatabase.GetGardenerInfos().FirstOrDefault<GardenerInfo>((Func<GardenerInfo, bool>)(x => x.FleetId == fi.ID));
					if (gardenerInfo != null && gardenerInfo.DeepSpaceSystemId.HasValue)
						pendingCombat.SystemID = gardenerInfo.DeepSpaceSystemId.Value;
				}
			}
			if (pendingCombat.SystemID == 0)
			{
				StarSystemInfo starSystemInfo1 = this.GameDatabase.GetStarSystemInfo(fi.SystemID);
				if (starSystemInfo1 == (StarSystemInfo)null || !starSystemInfo1.IsDeepSpace)
				{
					StarSystemInfo starSystemInfo2 = this.GameDatabase.GetDeepspaceStarSystemInfos().ToList<StarSystemInfo>().FirstOrDefault<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => (double)(x.Origin - targetPosition).Length < 9.99999974737875E-05));
					if (starSystemInfo2 == (StarSystemInfo)null)
					{
						StarSystemInfo starSystemInfo3 = this.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>().FirstOrDefault<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => (double)(x.Origin - targetPosition).Length < 9.99999974737875E-05));
						if (starSystemInfo3 == (StarSystemInfo)null)
						{
							int num = this.GameDatabase.InsertStarSystem(new int?(), App.Localize("@UI_STARMAP_ENCOUNTER_DEEPSPACE"), new int?(), "Deepspace", targetPosition, false, false, new int?());
							pendingCombat.SystemID = num;
						}
						else
							pendingCombat.SystemID = starSystemInfo3.ID;
					}
					else
						pendingCombat.SystemID = starSystemInfo2.ID;
				}
				else
					pendingCombat.SystemID = fi.SystemID;
			}
			this.GameDatabase.UpdateFleetLocation(fleet.ID, pendingCombat.SystemID, new int?());
			this.GameDatabase.UpdateFleetLocation(fi.ID, pendingCombat.SystemID, new int?());
			int num1 = 0;
			List<GardenerInfo> list1 = this.GameDatabase.GetGardenerInfos().ToList<GardenerInfo>();
			if (list1.FirstOrDefault<GardenerInfo>((Func<GardenerInfo, bool>)(x => x.FleetId == fi.ID)) != null)
			{
				GardenerInfo gardenerInfo = list1.FirstOrDefault<GardenerInfo>((Func<GardenerInfo, bool>)(x =>
			   {
				   if (x.GardenerFleetId == fi.ID)
					   return x.TurnsToWait <= 0;
				   return false;
			   }));
				if (gardenerInfo != null)
				{
					num1 = gardenerInfo.FleetId;
					this.GameDatabase.UpdateFleetLocation(gardenerInfo.FleetId, pendingCombat.SystemID, new int?());
				}
			}
			MissionInfo missionByFleetId = this.GameDatabase.GetMissionByFleetID(fi.ID);
			if (missionByFleetId != null)
			{
				List<WaypointInfo> list2 = this.GameDatabase.GetWaypointsByMissionID(missionByFleetId.ID).ToList<WaypointInfo>();
				foreach (WaypointInfo waypointInfo in list2)
					this.GameDatabase.RemoveWaypoint(waypointInfo.ID);
				this.GameDatabase.InsertWaypoint(missionByFleetId.ID, WaypointType.Intercepted, new int?(pendingCombat.SystemID));
				foreach (WaypointInfo waypointInfo in list2)
					this.GameDatabase.InsertWaypoint(waypointInfo.MissionID, waypointInfo.Type, waypointInfo.SystemID);
			}
			this.GameDatabase.RemoveMoveOrder(orderInfoByFleetId2.ID);
			MoveOrderInfo orderInfoByFleetId3 = this._db.GetMoveOrderInfoByFleetID(mission.TargetFleetID);
			if (orderInfoByFleetId3 != null && pendingCombat.SystemID != 0 && orderInfoByFleetId3.ToSystemID != pendingCombat.SystemID)
			{
				this._db.RemoveMoveOrder(orderInfoByFleetId3.ID);
				this._db.InsertMoveOrder(mission.TargetFleetID, 0, coords1, pendingCombat.SystemID, Vector3.Zero);
				this._db.InsertMoveOrder(mission.TargetFleetID, 0, this._db.GetStarSystemOrigin(pendingCombat.SystemID), orderInfoByFleetId3.ToSystemID, orderInfoByFleetId3.ToCoords);
			}
			pendingCombat.FleetIDs.Add(fleet.ID);
			pendingCombat.FleetIDs.Add(fi.ID);
			if (num1 != 0)
				pendingCombat.FleetIDs.Add(num1);
			List<Kerberos.Sots.PlayerFramework.Player> list3 = GameSession.GetPlayersWithCombatAssets(this.App, pendingCombat.SystemID).ToList<Kerberos.Sots.PlayerFramework.Player>();
			List<int> intList = new List<int>();
			foreach (Kerberos.Sots.PlayerFramework.Player player in list3)
			{
				if (!player.IsStandardPlayer)
					intList.Add(player.ID);
			}
			pendingCombat.Type = CombatType.CT_Meeting;
			pendingCombat.PlayersInCombat = list3.Select<Kerberos.Sots.PlayerFramework.Player, int>((Func<Kerberos.Sots.PlayerFramework.Player, int>)(x => x.ID)).ToList<int>();
			pendingCombat.NPCPlayersInCombat = intList;
			pendingCombat.ConflictID = GameSession.GetNextUniqueCombatID();
			pendingCombat.CardID = 1;
			this.m_Combats.Add(pendingCombat);
			foreach (Kerberos.Sots.PlayerFramework.Player player in list3)
			{
				if (player.ID != fleet.PlayerID && player.IsStandardPlayer)
				{
					if (flag1 || flag2)
					{
						if (player.ID != fi.PlayerID)
						{
							this.GameDatabase.UpdateDiplomacyState(fleet.PlayerID, player.ID, DiplomacyState.WAR, 500, true);
							this.GameDatabase.InsertGovernmentAction(fleet.PlayerID, App.Localize("@GA_DECLAREWAR"), "DeclareWar", 0, 0);
						}
					}
					else
					{
						this.GameDatabase.UpdateDiplomacyState(fleet.PlayerID, player.ID, DiplomacyState.WAR, 500, true);
						this.GameDatabase.InsertGovernmentAction(fleet.PlayerID, App.Localize("@GA_DECLAREWAR"), "DeclareWar", 0, 0);
					}
				}
			}
			return true;
		}

		public bool DoInterdictMission(MissionInfo mission, FleetInfo fleet)
		{
			GameSession.Trace("Fleet " + (object)fleet.ID + " blockading system " + (object)mission.TargetSystemID);
			return Kerberos.Sots.StarFleet.StarFleet.IsFleetExhausted(this, fleet);
		}

		public bool DoInvasionMission(MissionInfo mission, FleetInfo fleet)
		{
			AIColonyIntel aiColonyIntel = this._db.GetColonyIntelForPlanet(fleet.PlayerID, mission.TargetOrbitalObjectID);
			if (aiColonyIntel == null)
			{
				foreach (int planetID in StarSystemDetailsUI.CollectPlanetListItemsForInvasionMission(this._app, mission.TargetSystemID).ToList<int>())
				{
					aiColonyIntel = this._db.GetColonyIntelForPlanet(fleet.PlayerID, planetID);
					if (aiColonyIntel != null)
					{
						if (aiColonyIntel.OwningPlayerID != fleet.PlayerID)
							break;
					}
					aiColonyIntel = (AIColonyIntel)null;
				}
			}
			if (aiColonyIntel == null || aiColonyIntel.OwningPlayerID == fleet.PlayerID)
				return true;
			if (this._app.GameDatabase.GetDiplomacyStateBetweenPlayers(aiColonyIntel.OwningPlayerID, fleet.PlayerID) != DiplomacyState.WAR)
				this.DeclareWarInformally(fleet.PlayerID, aiColonyIntel.OwningPlayerID);
			GameSession.Trace("Fleet " + (object)fleet.ID + " is invading system " + (object)mission.TargetSystemID);
			return Kerberos.Sots.StarFleet.StarFleet.IsFleetExhausted(this, fleet);
		}

		public bool DoConstructionMission(MissionInfo mission, FleetInfo fleet)
		{
			if (this.isHostilesAtSystem(fleet.PlayerID, fleet.SystemID))
				return false;
			if (this.App.GameDatabase.GetStationInfo(mission.TargetOrbitalObjectID) == null)
			{
				if (this.App.GameDatabase.GetOrbitalObjectInfo(mission.TargetOrbitalObjectID) != null)
				{
					if (mission.StationType.HasValue)
					{
						int num = this.ConstructStation(DesignLab.CreateStationDesignInfo(this.App.AssetDatabase, this.App.GameDatabase, fleet.PlayerID, (StationType)mission.StationType.Value, 0, true), mission.TargetOrbitalObjectID, false);
						mission.TargetOrbitalObjectID = num;
					}
				}
				else
				{
					Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
					return true;
				}
			}
			int num1 = (int)((double)Kerberos.Sots.StarFleet.StarFleet.GetConstructionPointsForFleet(this, fleet.ID) * (double)this._db.GetStratModifierFloatToApply(StratModifiers.ConstructionPointBonus, fleet.PlayerID));
			if (num1 == 0)
			{
				Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
				return true;
			}
			mission.Duration -= num1;
			if (mission.Duration <= 0)
			{
				StationInfo stationInfo = this.App.GameDatabase.GetStationInfo(mission.TargetOrbitalObjectID);
				if (stationInfo == null)
					return false;
				if (stationInfo.DesignInfo.StationLevel > 4)
				{
					Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
					return true;
				}
				DesignInfo stationDesignInfo = DesignLab.CreateStationDesignInfo(this._app.AssetDatabase, this._app.GameDatabase, fleet.PlayerID, stationInfo.DesignInfo.StationType, stationInfo.DesignInfo.StationLevel + 1, true);
				this.UpgradeStation(stationInfo, stationDesignInfo);
				PlayerInfo playerInfo1 = this._db.GetPlayerInfo(stationInfo.PlayerID);
				if (playerInfo1 != null)
					this._db.UpdatePlayerSavings(stationInfo.PlayerID, playerInfo1.Savings - (double)stationDesignInfo.SavingsCost);
				if (stationInfo.DesignInfo.StationLevel == 1)
				{
					if (stationInfo.DesignInfo.StationType == StationType.CIVILIAN)
						GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_CIVILIAN_STATION_BUILT, fleet.PlayerID, new int?(), this.App.GameDatabase.GetStarSystemInfo(fleet.SystemID).ProvinceID, new int?());
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_STATION_BUILT,
						EventMessage = TurnEventMessage.EM_STATION_BUILT,
						EventSoundCueName = this.GetUpgradeStationSoundCueName(stationDesignInfo),
						TurnNumber = this._db.GetTurnCount(),
						PlayerID = fleet.PlayerID,
						SystemID = fleet.SystemID,
						OrbitalID = stationInfo.OrbitalObjectID,
						FleetID = fleet.ID,
						ShowsDialog = true
					});
					Faction faction = this.App.AssetDatabase.GetFaction(playerInfo1.FactionID);
					string localizedStationTypeName = this.App.AssetDatabase.GetLocalizedStationTypeName(stationInfo.DesignInfo.StationType, faction.HasSlaves());
					switch (stationInfo.DesignInfo.StationType)
					{
						case StationType.NAVAL:
							this.App.GameDatabase.InsertGovernmentAction(playerInfo1.ID, string.Format(App.Localize("@GA_STATIONBUILT"), (object)localizedStationTypeName), "StationBuilt_Naval", 0, 0);
							break;
						case StationType.SCIENCE:
							this.App.GameDatabase.InsertGovernmentAction(playerInfo1.ID, string.Format(App.Localize("@GA_STATIONBUILT"), (object)localizedStationTypeName), "StationBuilt_Sci", 0, 0);
							break;
						case StationType.CIVILIAN:
							this.App.GameDatabase.InsertGovernmentAction(playerInfo1.ID, string.Format(App.Localize("@GA_STATIONBUILT"), (object)localizedStationTypeName), "StationBuilt_Civ", 0, 0);
							break;
						case StationType.DIPLOMATIC:
							this.App.GameDatabase.InsertGovernmentAction(playerInfo1.ID, string.Format(App.Localize("@GA_STATIONBUILT"), (object)localizedStationTypeName), "StationBuilt_Dip", 0, 0);
							break;
						case StationType.MINING:
							this.App.GameDatabase.InsertGovernmentAction(playerInfo1.ID, string.Format(App.Localize("@GA_STATIONBUILT"), (object)localizedStationTypeName), "StationBuilt_Mine", 0, 0);
							break;
					}
					if (stationInfo.DesignInfo.StationType == StationType.GATE && stationInfo.DesignInfo.StationLevel > 0)
					{
						foreach (FleetInfo fleetInfo in this._app.GameDatabase.GetFleetInfoBySystemID(fleet.SystemID, FleetType.FL_GATE).ToList<FleetInfo>())
						{
							if (fleetInfo.PlayerID == stationInfo.PlayerID)
								this._app.GameDatabase.RemoveFleet(fleetInfo.ID);
						}
					}
					if (stationInfo.DesignInfo.StationType == StationType.SCIENCE && stationInfo.DesignInfo.StationLevel > 0)
					{
						OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID);
						if (this._app.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ParentID.Value) != null)
						{
							ColonyInfo colonyInfoForPlanet = this._app.GameDatabase.GetColonyInfoForPlanet(orbitalObjectInfo.ParentID.Value);
							if (colonyInfoForPlanet != null)
							{
								PlayerInfo playerInfo2 = this._app.GameDatabase.GetPlayerInfo(colonyInfoForPlanet.PlayerID);
								if (this.App.AssetDatabase.GetFaction(playerInfo2.FactionID).IsIndependent() && !this.App.GameDatabase.GetHasPlayerStudyingIndependentRace(fleet.PlayerID, playerInfo2.ID))
									this.InsertNewIndependentPlanetResearchProject(fleet.PlayerID, playerInfo2.ID);
							}
						}
					}
					int? systemOwningPlayer = this.App.GameDatabase.GetSystemOwningPlayer(fleet.SystemID);
					if (systemOwningPlayer.HasValue)
					{
						int? nullable = systemOwningPlayer;
						int playerId = fleet.PlayerID;
						if ((nullable.GetValueOrDefault() != playerId ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
							goto label_42;
					}
					if (stationInfo.DesignInfo.StationType == StationType.NAVAL)
						Kerberos.Sots.GameStates.StarSystem.PaintSystemPlayerColor(this._app.GameDatabase, fleet.SystemID, fleet.PlayerID);
				}
				else
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_STATION_UPGRADED,
						EventMessage = TurnEventMessage.EM_STATION_UPGRADED,
						TurnNumber = this._db.GetTurnCount(),
						PlayerID = fleet.PlayerID,
						SystemID = fleet.SystemID,
						OrbitalID = stationInfo.OrbitalObjectID,
						FleetID = fleet.ID,
						ShowsDialog = false
					});
			}
		label_42:
			this._db.UpdateMission(mission);
			return mission.Duration <= 0;
		}

		public bool DoSpecialConstructionMission(MissionInfo mission, FleetInfo fleet)
		{
			MoveOrderInfo orderInfoByFleetId = this._db.GetMoveOrderInfoByFleetID(fleet.ID);
			Vector3 coords1 = this._db.GetFleetLocation(fleet.ID, false).Coords;
			FleetInfo fleetInfo = this._db.GetFleetInfo(mission.TargetFleetID);
			if (fleetInfo == null)
			{
				Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
				return true;
			}
			if (mission.TargetSystemID == 0)
			{
				this._db.GetMoveOrderInfoByFleetID(mission.TargetFleetID);
				Vector3 coords2 = this._db.GetFleetLocation(fleetInfo.ID, true).Coords;
				if (!StarMap.IsInRange(this._db, fleet.PlayerID, coords2, 1f, (Dictionary<int, List<ShipInfo>>)null) || Kerberos.Sots.StarFleet.StarFleet.IsFleetExhausted(this, fleet))
				{
					Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
					return true;
				}
				if (orderInfoByFleetId != null)
					this._db.RemoveMoveOrder(orderInfoByFleetId.ID);
				this._db.InsertMoveOrder(fleet.ID, 0, coords1, 0, coords2);
				float remainingNodeDistance = 0.0f;
				orderInfoByFleetId = this._db.GetMoveOrderInfoByFleetID(fleet.ID);
				if (!this.ProcessMoveOrder(orderInfoByFleetId, fleet, ref remainingNodeDistance))
					return false;
				if (this._app.Game.ScriptModules.NeutronStar != null && this._app.Game.ScriptModules.NeutronStar.PlayerID == fleetInfo.PlayerID)
				{
					NeutronStarInfo neutronStarInfo = this._db.GetNeutronStarInfos().FirstOrDefault<NeutronStarInfo>((Func<NeutronStarInfo, bool>)(x => x.FleetId == mission.TargetFleetID));
					if (neutronStarInfo == null)
					{
						Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
						return true;
					}
					if (neutronStarInfo.DeepSpaceSystemId.HasValue)
					{
						mission.TargetSystemID = neutronStarInfo.DeepSpaceSystemId.Value;
						mission.TargetOrbitalObjectID = neutronStarInfo.DeepSpaceOrbitalId.Value;
						this.GameDatabase.UpdateFleetLocation(fleet.ID, neutronStarInfo.DeepSpaceSystemId.Value, new int?());
					}
				}
				else if (this._app.Game.ScriptModules.Gardeners != null && this._app.Game.ScriptModules.Gardeners.PlayerID == fleetInfo.PlayerID)
				{
					GardenerInfo gardenerInfo = this._db.GetGardenerInfos().FirstOrDefault<GardenerInfo>((Func<GardenerInfo, bool>)(x => x.FleetId == mission.TargetFleetID));
					if (gardenerInfo == null)
					{
						Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
						return true;
					}
					if (gardenerInfo.DeepSpaceSystemId.HasValue)
					{
						mission.TargetSystemID = gardenerInfo.DeepSpaceSystemId.Value;
						mission.TargetOrbitalObjectID = gardenerInfo.DeepSpaceOrbitalId.Value;
						this.GameDatabase.UpdateFleetLocation(fleet.ID, gardenerInfo.DeepSpaceSystemId.Value, new int?());
					}
				}
				this._db.UpdateMission(mission);
			}
			if (mission.TargetSystemID == 0 || mission.TargetOrbitalObjectID == 0)
			{
				Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
				return true;
			}
			List<StationInfo> list1 = this.App.GameDatabase.GetStationForSystem(mission.TargetSystemID).ToList<StationInfo>();
			if (list1.Count > 0)
			{
				if (list1.Any<StationInfo>((Func<StationInfo, bool>)(x => this.App.GameDatabase.GetDiplomacyStateBetweenPlayers(fleet.PlayerID, x.PlayerID) != DiplomacyState.WAR)))
				{
					Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
					return true;
				}
				PendingCombat pendingCombat = new PendingCombat();
				pendingCombat.SystemID = mission.TargetSystemID;
				pendingCombat.FleetIDs.Add(fleet.ID);
				pendingCombat.FleetIDs.Add(fleetInfo.ID);
				List<Kerberos.Sots.PlayerFramework.Player> list2 = GameSession.GetPlayersWithCombatAssets(this.App, pendingCombat.SystemID).ToList<Kerberos.Sots.PlayerFramework.Player>();
				List<int> intList = new List<int>();
				foreach (Kerberos.Sots.PlayerFramework.Player player in list2)
				{
					if (!player.IsStandardPlayer)
						intList.Add(player.ID);
				}
				pendingCombat.Type = CombatType.CT_Meeting;
				pendingCombat.PlayersInCombat = list2.Select<Kerberos.Sots.PlayerFramework.Player, int>((Func<Kerberos.Sots.PlayerFramework.Player, int>)(x => x.ID)).ToList<int>();
				pendingCombat.NPCPlayersInCombat = intList;
				pendingCombat.ConflictID = GameSession.GetNextUniqueCombatID();
				pendingCombat.CardID = 1;
				this.m_Combats.Add(pendingCombat);
				return false;
			}
			if (this.App.GameDatabase.GetStationInfo(mission.TargetOrbitalObjectID) == null)
			{
				if (this.App.GameDatabase.GetOrbitalObjectInfo(mission.TargetOrbitalObjectID) != null)
				{
					if (mission.StationType.HasValue)
					{
						int num = this.ConstructStation(DesignLab.CreateStationDesignInfo(this.App.AssetDatabase, this.App.GameDatabase, fleet.PlayerID, (StationType)mission.StationType.Value, 0, true), mission.TargetOrbitalObjectID, false);
						mission.TargetOrbitalObjectID = num;
					}
				}
				else
				{
					Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
					return true;
				}
			}
			int num1 = (int)((double)Kerberos.Sots.StarFleet.StarFleet.GetConstructionPointsForFleet(this, fleet.ID) * (double)this._db.GetStratModifierFloatToApply(StratModifiers.ConstructionPointBonus, fleet.PlayerID));
			if (num1 == 0)
			{
				Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
				return true;
			}
			mission.Duration -= num1;
			if (mission.Duration <= 0)
			{
				StationInfo stationInfo = this.App.GameDatabase.GetStationInfo(mission.TargetOrbitalObjectID);
				if (stationInfo == null)
					return false;
				DesignInfo stationDesignInfo = DesignLab.CreateStationDesignInfo(this._app.AssetDatabase, this._app.GameDatabase, fleet.PlayerID, stationInfo.DesignInfo.StationType, stationInfo.DesignInfo.StationLevel + 1, true);
				this.UpgradeStation(stationInfo, stationDesignInfo);
				PlayerInfo playerInfo = this._db.GetPlayerInfo(stationInfo.PlayerID);
				if (playerInfo != null)
					this._db.UpdatePlayerSavings(stationInfo.PlayerID, playerInfo.Savings - (double)stationDesignInfo.SavingsCost);
				if (stationInfo.DesignInfo.StationLevel == 1)
				{
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_STATION_BUILT,
						EventMessage = TurnEventMessage.EM_STATION_BUILT,
						EventSoundCueName = this.GetUpgradeStationSoundCueName(stationDesignInfo),
						TurnNumber = this._db.GetTurnCount(),
						PlayerID = fleet.PlayerID,
						SystemID = fleet.SystemID,
						OrbitalID = stationInfo.OrbitalObjectID,
						FleetID = fleet.ID,
						ShowsDialog = true
					});
					Faction faction = this.App.AssetDatabase.GetFaction(playerInfo.FactionID);
					string localizedStationTypeName = this.App.AssetDatabase.GetLocalizedStationTypeName(stationInfo.DesignInfo.StationType, faction.HasSlaves());
					if (stationInfo.DesignInfo.StationType == StationType.SCIENCE)
						this.App.GameDatabase.InsertGovernmentAction(playerInfo.ID, string.Format(App.Localize("@GA_STATIONBUILT"), (object)localizedStationTypeName), "StationBuilt_Sci", 0, 0);
				}
				if (fleetInfo != null)
				{
					if (this._app.Game.ScriptModules.NeutronStar != null && this._app.Game.ScriptModules.NeutronStar.PlayerID == fleetInfo.PlayerID)
					{
						NeutronStarInfo neutronStarInfo = this._db.GetNeutronStarInfos().FirstOrDefault<NeutronStarInfo>((Func<NeutronStarInfo, bool>)(x => x.DeepSpaceSystemId.Value == mission.TargetSystemID));
						if (neutronStarInfo != null)
							this.InsertNewNeutronStarResearchProject(fleet.PlayerID, neutronStarInfo.Id);
					}
					else if (this._app.Game.ScriptModules.Gardeners != null && this._app.Game.ScriptModules.Gardeners.PlayerID == fleetInfo.PlayerID)
					{
						GardenerInfo gardenerInfo = this._db.GetGardenerInfos().FirstOrDefault<GardenerInfo>((Func<GardenerInfo, bool>)(x =>
					   {
						   if (x.DeepSpaceSystemId.HasValue)
							   return x.DeepSpaceSystemId.Value == mission.TargetSystemID;
						   return false;
					   }));
						if (gardenerInfo != null)
							this.InsertNewGardenerResearchProject(fleet.PlayerID, gardenerInfo.Id);
					}
				}
			}
			if (mission.Duration <= 0)
			{
				if (this.App.GameDatabase.GetMoveOrderInfoByFleetID(fleet.ID) != null)
				{
					this._db.RemoveMoveOrder(orderInfoByFleetId.ID);
					this._db.InsertMoveOrder(orderInfoByFleetId.FleetID, 0, this.GameDatabase.GetStarSystemOrigin(mission.TargetSystemID), fleet.SupportingSystemID, Vector3.Zero);
				}
				mission.TargetSystemID = 0;
				mission.TargetOrbitalObjectID = 0;
				mission.TargetFleetID = 0;
				this._db.UpdateMission(mission);
				return true;
			}
			this._db.UpdateMission(mission);
			return false;
		}

		public bool DoColonizationMission(MissionInfo mission, FleetInfo fleet)
		{
			if (this.isHostilesAtSystem(fleet.PlayerID, fleet.SystemID) || this.isTrapAtPlanet(fleet.SystemID, mission.TargetOrbitalObjectID, fleet.PlayerID))
				return false;
			List<ColonyInfo> list = this._db.GetColonyInfosForSystem(fleet.SystemID).ToList<ColonyInfo>();
			int? systemOwningPlayer = this.App.GameDatabase.GetSystemOwningPlayer(fleet.SystemID);
			if (list.Count > 0 && systemOwningPlayer.HasValue)
			{
				int? nullable = systemOwningPlayer;
				int playerId = fleet.PlayerID;
				if (!(nullable.GetValueOrDefault() == playerId & nullable.HasValue) && !this.App.GameDatabase.hasPermissionToBuildEnclave(fleet.PlayerID, mission.TargetOrbitalObjectID))
					return true;
			}
			ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(mission.TargetOrbitalObjectID);
			PlanetInfo planetInfo = this._db.GetPlanetInfo(mission.TargetOrbitalObjectID);
			if (colonyInfoForPlanet == null)
			{
				double colonizationSpace = Kerberos.Sots.StarFleet.StarFleet.GetColonizationSpace(this, fleet.ID);
				if (colonizationSpace > 0.0)
				{
					int? owningPlayer = this._app.GameDatabase.GetSystemOwningPlayer(fleet.SystemID);
					if (owningPlayer.HasValue)
					{
						int? nullable1 = owningPlayer;
						int playerId = fleet.PlayerID;
						if (!(nullable1.GetValueOrDefault() == playerId & nullable1.HasValue) && this._db.GetRequestInfos().ToList<RequestInfo>().FirstOrDefault<RequestInfo>((Func<RequestInfo, bool>)(x =>
					   {
						   if (x.Type == RequestType.EstablishEnclaveRequest && x.InitiatingPlayer == fleet.PlayerID)
						   {
							   int receivingPlayer = x.ReceivingPlayer;
							   int? nullable = owningPlayer;
							   int valueOrDefault = nullable.GetValueOrDefault();
							   if (receivingPlayer == valueOrDefault & nullable.HasValue)
								   return (double)x.RequestValue == (double)fleet.SystemID;
						   }
						   return false;
					   })) != null)
							this._app.GameDatabase.InsertGovernmentAction(fleet.PlayerID, App.Localize("@GA_ENCLAVEBUILT"), "EnclaveBuilt", 0, 0);
					}
					int colonyID = this._db.InsertColony(mission.TargetOrbitalObjectID, fleet.PlayerID, colonizationSpace, 0.5f, this._db.GetTurnCount(), planetInfo.Infrastructure + (float)(colonizationSpace * 0.0001), true);
					foreach (MissionInfo missionInfo in this._db.GetMissionsByPlanetDest(mission.TargetOrbitalObjectID).ToList<MissionInfo>())
					{
						FleetInfo fleetInfo = this._db.GetFleetInfo(missionInfo.FleetID);
						if (missionInfo.Type == MissionType.COLONIZATION && fleetInfo.PlayerID != fleet.PlayerID)
							this._db.ApplyDiplomacyReaction(fleet.PlayerID, fleetInfo.PlayerID, StratModifiers.DiplomacyReactionColonizeClaimedWorld, 1);
					}
					ColonyInfo colonyInfo = this._db.GetColonyInfo(colonyID);
					GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_WORLD_COLONIZED, fleet.PlayerID, new int?(), this.App.GameDatabase.GetStarSystemInfo(this.App.GameDatabase.GetOrbitalObjectInfo(planetInfo.ID).StarSystemID).ProvinceID, new int?());
					Kerberos.Sots.Strategy.InhabitedPlanet.Colony.SetOutputRate(this._db, this.AssetDatabase, ref colonyInfo, planetInfo, Kerberos.Sots.Strategy.InhabitedPlanet.Colony.OutputRate.Trade, 0.0f);
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_COLONY_ESTABLISHED,
						EventMessage = TurnEventMessage.EM_COLONY_ESTABLISHED,
						EventSoundCueName = string.Format("STRAT_016-01_{0}_ColonyEstablished", (object)this._db.GetFactionName(this._db.GetPlayerFactionID(fleet.PlayerID))),
						PlayerID = fleet.PlayerID,
						SystemID = mission.TargetSystemID,
						FleetID = fleet.ID,
						OrbitalID = mission.TargetOrbitalObjectID,
						ColonyID = colonyID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = true
					});
					this.App.GameDatabase.InsertGovernmentAction(fleet.PlayerID, App.Localize("@GA_COLONYSTARTED"), "ColonyStarted", 0, 0);
					if (fleet.PlayerID == this.LocalPlayer.ID)
						this.App.SteamHelper.DoAchievement(AchievementType.SOTS2_WE_WILL_CALL_IT);
					double terraformingSpace = Kerberos.Sots.StarFleet.StarFleet.GetTerraformingSpace(this, fleet.ID);
					Kerberos.Sots.StarFleet.StarFleet.GetNumColonizationShips(this, fleet.ID);
					List<PlagueInfo> plagues = new List<PlagueInfo>();
					int resources = planetInfo.Resources;
					if (this._db.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, fleet.PlayerID))
					{
						ShipInfo constructionShip = Kerberos.Sots.StarFleet.StarFleet.GetFirstConstructionShip(this, fleet);
						if (constructionShip != null)
							this._db.RemoveShip(constructionShip.ID);
						planetInfo.Infrastructure += this._app.AssetDatabase.GetGlobal<float>("AssimilatorInitialValue");
					}
					List<ColonyFactionInfo> civPopulation;
					bool achievedSuperWorld;
					Kerberos.Sots.Strategy.InhabitedPlanet.Colony.MaintainColony(this, ref colonyInfo, ref planetInfo, ref plagues, colonizationSpace, terraformingSpace, fleet, out civPopulation, out achievedSuperWorld, true);
					this._db.UpdateColony(colonyInfo);
					this._db.UpdatePlanet(planetInfo);
					if (planetInfo.Resources == 0 && resources != 0)
					{
						OrbitalObjectInfo orbitalObjectInfo = this.GameDatabase.GetOrbitalObjectInfo(planetInfo.ID);
						this.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_PLANET_NO_RESOURCES,
							EventMessage = TurnEventMessage.EM_PLANET_NO_RESOURCES,
							PlayerID = colonyInfo.PlayerID,
							SystemID = orbitalObjectInfo.StarSystemID,
							OrbitalID = orbitalObjectInfo.ID,
							TurnNumber = this.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
					foreach (ColonyFactionInfo colonyFactionInfo in civPopulation)
					{
						ColonyFactionInfo cfi = colonyFactionInfo;
						if (((IEnumerable<ColonyFactionInfo>)colonyInfo.Factions).Any<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID == cfi.FactionID)))
						{
							cfi.LastMorale = this._app.AssetDatabase.CivilianPopulationStartMoral;
							cfi.Morale = this._app.AssetDatabase.CivilianPopulationStartMoral;
							this._db.UpdateCivilianPopulation(cfi);
						}
						else
							this._db.InsertColonyFaction(cfi.OrbitalObjectID, cfi.FactionID, cfi.CivilianPop, cfi.CivPopWeight, cfi.TurnEstablished);
					}
					GameSession.Trace("Colony established in system " + (object)mission.TargetSystemID);
				}
				else
				{
					Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
					GameSession.Trace("Colonization mission to " + this._db.GetStarSystemInfo(mission.TargetSystemID).Name + " cancelled. No Colonizers.");
				}
			}
			if (!this._db.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, fleet.PlayerID) || !this._db.CanSupportPlanet(fleet.PlayerID, mission.TargetOrbitalObjectID))
				return true;
			this.DoSupportMission(mission, fleet);
			return false;
		}

		public bool DoEvacuationMission(MissionInfo mission, FleetInfo fleet)
		{
			if (this.isHostilesAtSystem(fleet.PlayerID, fleet.SystemID))
				return false;
			List<ColonyInfo> list = this._db.GetColonyInfosForSystem(fleet.SystemID).Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == fleet.PlayerID)).OrderBy<ColonyInfo, double>((Func<ColonyInfo, double>)(x => this.GameDatabase.GetCivilianPopulation(x.OrbitalObjectID, 0, false))).ToList<ColonyInfo>();
			ColonyInfo colony1 = this._db.GetColonyInfoForPlanet(mission.TargetOrbitalObjectID);
			if (colony1 == null && list.Count > 0)
			{
				colony1 = list.Last<ColonyInfo>();
				mission.TargetOrbitalObjectID = colony1.OrbitalObjectID;
				this._db.UpdateMission(mission);
			}
			if (colony1 != null)
			{
				int colonizationShips = Kerberos.Sots.StarFleet.StarFleet.GetNumColonizationShips(this, fleet.ID);
				if (colonizationShips > 0)
				{
					double val1 = Math.Min(this.GameDatabase.GetCivilianPopulation(colony1.OrbitalObjectID, 0, false), (double)colonizationShips * (double)this.AssetDatabase.EvacCivPerCol);
					bool flag1 = val1 > 0.0;
					int factionId = this.GameDatabase.GetPlayerFactionID(fleet.PlayerID);
					ColonyFactionInfo civPop1 = ((IEnumerable<ColonyFactionInfo>)colony1.Factions).FirstOrDefault<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID == factionId));
					if (civPop1 != null)
					{
						double num = Math.Min(val1, civPop1.CivilianPop);
						civPop1.CivilianPop -= num;
						val1 -= num;
						this.GameDatabase.UpdateCivilianPopulation(civPop1);
					}
					if (val1 > 0.0)
					{
						foreach (ColonyFactionInfo faction in colony1.Factions)
						{
							if (faction != civPop1)
							{
								double num = Math.Min(val1, faction.CivilianPop);
								faction.CivilianPop -= num;
								val1 -= num;
								this.GameDatabase.UpdateCivilianPopulation(faction);
								if (val1 <= 0.0)
									break;
							}
						}
					}
					if (flag1)
					{
						this._db.UpdateColony(colony1);
						this._db.UpdatePlanet(this._db.GetPlanetInfo(colony1.OrbitalObjectID));
					}
					if (val1 > 0.0)
					{
						list.RemoveAll((Predicate<ColonyInfo>)(x => x.OrbitalObjectID == mission.TargetOrbitalObjectID));
						this.GameDatabase.RemoveColonyOnPlanet(mission.TargetOrbitalObjectID);
						foreach (ColonyInfo colony2 in list)
						{
							if (val1 > 0.0)
							{
								bool flag2 = val1 > 0.0;
								ColonyFactionInfo civPop2 = ((IEnumerable<ColonyFactionInfo>)colony2.Factions).FirstOrDefault<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID == factionId));
								if (civPop2 != null)
								{
									double num = Math.Min(val1, civPop2.CivilianPop);
									civPop2.CivilianPop -= num;
									val1 -= num;
									this.GameDatabase.UpdateCivilianPopulation(civPop2);
								}
								if (val1 > 0.0)
								{
									foreach (ColonyFactionInfo faction in colony1.Factions)
									{
										if (faction != civPop2)
										{
											double num = Math.Min(val1, faction.CivilianPop);
											faction.CivilianPop -= num;
											val1 -= num;
											this.GameDatabase.UpdateCivilianPopulation(faction);
											if (val1 <= 0.0)
												break;
										}
									}
								}
								if (flag2)
								{
									this._db.UpdateColony(colony2);
									this._db.UpdatePlanet(this._db.GetPlanetInfo(colony2.OrbitalObjectID));
								}
								if (val1 > 0.0)
									this.GameDatabase.RemoveColonyOnPlanet(colony2.OrbitalObjectID);
							}
							else
								break;
						}
					}
				}
				else
					Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
			}
			return true;
		}

		public bool DoPiracyMission(MissionInfo mission, FleetInfo fleet)
		{
			return Kerberos.Sots.StarFleet.StarFleet.IsFleetExhausted(this, fleet);
		}

		public bool DoDeployNPGMission(MissionInfo mission, FleetInfo fleet)
		{
			List<WaypointInfo> list1 = this._app.GameDatabase.GetWaypointsByMissionID(mission.ID).ToList<WaypointInfo>();
			Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this, fleet.ID);
			ShipInfo shipInfo = this._app.GameDatabase.GetShipInfoByFleetID(fleet.ID, true).FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.IsLoaCube()));
			if (shipInfo == null)
			{
				Kerberos.Sots.StarFleet.StarFleet.CancelMission(this.App.Game, fleet, true);
				return true;
			}
			DesignInfo designInfo = this._app.GameDatabase.GetDesignInfosForPlayer(fleet.PlayerID).FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => ((IEnumerable<DesignSectionInfo>)x.DesignSections).First<DesignSectionInfo>().ShipSectionAsset.IsAccelerator));
			if (shipInfo.LoaCubes < designInfo.ProductionCost)
			{
				Kerberos.Sots.StarFleet.StarFleet.CancelMission(this.App.Game, fleet, true);
				return true;
			}
			if (!list1.Any<WaypointInfo>())
			{
				this._db.InsertMoveOrder(fleet.ID, 0, this._db.GetFleetLocation(fleet.ID, false).Coords, fleet.SupportingSystemID, Vector3.Zero);
				return true;
			}
			if (fleet.SystemID != 0 && this.App.GameDatabase.GetStarSystemInfo(fleet.SystemID) != (StarSystemInfo)null && !this.App.GameDatabase.GetStarSystemInfo(fleet.SystemID).IsDeepSpace)
			{
				if (!this._app.GameDatabase.GetFleetsByPlayerAndSystem(fleet.PlayerID, fleet.SystemID, FleetType.FL_ACCELERATOR).Any<FleetInfo>())
				{
					int num = this._app.GameDatabase.InsertFleet(fleet.PlayerID, 0, fleet.SystemID, fleet.SupportingSystemID, "ACCELERATOR FLEET", FleetType.FL_ACCELERATOR);
					this._app.GameDatabase.InsertShip(num, designInfo.ID, "NPG", ShipParams.HS_GATE_DEPLOYED, new int?(), 0);
					if (fleet.PreviousSystemID.HasValue)
						this._app.GameDatabase.InsertLoaLineFleetRecord((this._app.GameDatabase.GetNodeLineBetweenSystems(fleet.PlayerID, fleet.PreviousSystemID.Value, mission.TargetSystemID, false, true) ?? this._app.GameDatabase.GetNodeLine(this._app.GameDatabase.InsertNodeLine(fleet.PreviousSystemID.Value, mission.TargetSystemID, 90))).ID, num);
					shipInfo.LoaCubes -= designInfo.ProductionCost;
					this._app.GameDatabase.UpdateShipLoaCubes(shipInfo.ID, shipInfo.LoaCubes);
				}
				this._app.GameDatabase.UpdateFleetAccelerated(this, fleet.ID, new int?());
			}
			bool useDirectRoute = true;
			WaypointInfo wi = list1.First<WaypointInfo>();
			float fleetTravelSpeed = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this, fleet.ID, false);
			MoveOrderInfo moi = this.GetNextMoveOrder(wi, fleet, useDirectRoute);
			bool flag = true;
			while (moi != null & flag)
			{
				flag = this.ProcessMoveOrder(moi, fleet, ref fleetTravelSpeed);
				if (flag)
				{
					int num1 = moi.ToSystemID;
					if (moi.ToSystemID == 0)
					{
						List<StarSystemInfo> list2 = this.GameDatabase.GetDeepspaceStarSystemInfos().ToList<StarSystemInfo>();
						list2.AddRange(this.GameDatabase.GetStarSystemInfos());
						StarSystemInfo starSystemInfo = list2.FirstOrDefault<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => (double)(x.Origin - moi.ToCoords).Length < 0.0001));
						num1 = !(starSystemInfo == (StarSystemInfo)null) ? starSystemInfo.ID : this._app.GameDatabase.InsertStarSystem(new int?(), "Accelerator Node", new int?(), "Deepspace", moi.ToCoords, false, false, new int?());
					}
					if (!this._app.GameDatabase.GetFleetsByPlayerAndSystem(fleet.PlayerID, num1, FleetType.FL_ACCELERATOR).Any<FleetInfo>())
					{
						int num2 = this._app.GameDatabase.InsertFleet(fleet.PlayerID, 0, num1, fleet.SupportingSystemID, "ACCELERATOR FLEET", FleetType.FL_ACCELERATOR);
						int shipID = this._app.GameDatabase.InsertShip(num2, designInfo.ID, "NPG", ShipParams.HS_GATE_DEPLOYED, new int?(), 0);
						if (moi.ToSystemID != 0)
						{
							Matrix? gateShipTransform = GameSession.GetValidGateShipTransform(this.App.Game, moi.ToSystemID, num2);
							if (gateShipTransform.HasValue)
								this.App.GameDatabase.UpdateShipSystemPosition(shipID, new Matrix?(gateShipTransform.Value));
						}
						if (moi.ToSystemID == 0)
							this._app.GameDatabase.InsertMoveOrder(num2, num1, moi.FromCoords, mission.TargetSystemID, moi.ToCoords);
						if (fleet.PreviousSystemID.HasValue)
							this._app.GameDatabase.InsertLoaLineFleetRecord((this._app.GameDatabase.GetNodeLineBetweenSystems(fleet.PlayerID, fleet.PreviousSystemID.Value, mission.TargetSystemID, false, true) ?? this._app.GameDatabase.GetNodeLine(this._app.GameDatabase.InsertNodeLine(fleet.PreviousSystemID.Value, mission.TargetSystemID, 90))).ID, num2);
						shipInfo.LoaCubes -= designInfo.ProductionCost;
						this._app.GameDatabase.UpdateShipLoaCubes(shipInfo.ID, shipInfo.LoaCubes);
					}
					this._app.GameDatabase.UpdateFleetAccelerated(this, fleet.ID, new int?());
					this.FinishMove(moi, fleet);
					moi = this.GetNextMoveOrder(wi, fleet, useDirectRoute);
					if (moi != null)
						return false;
					if (moi == null)
						this.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_NPG_DEPLOYED,
							EventMessage = TurnEventMessage.EM_NPG_DEPLOYED,
							PlayerID = fleet.PlayerID,
							SystemID = fleet.SystemID,
							SystemID2 = fleet.SupportingSystemID,
							FleetID = fleet.ID,
							MissionID = mission.ID,
							TurnNumber = this.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
				}
			}
			return moi == null;
		}

		public bool DoSupportMission(MissionInfo mission, FleetInfo fleet)
		{
			if (this.isHostilesAtSystem(fleet.PlayerID, fleet.SystemID))
				return false;
			ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(mission.TargetOrbitalObjectID);
			PlanetInfo planetInfo = this._db.GetPlanetInfo(mission.TargetOrbitalObjectID);
			if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == fleet.PlayerID)
			{
				double colonizationSpace = Kerberos.Sots.StarFleet.StarFleet.GetColonizationSpace(this, fleet.ID);
				double terraformingSpace = Kerberos.Sots.StarFleet.StarFleet.GetTerraformingSpace(this, fleet.ID);
				Kerberos.Sots.StarFleet.StarFleet.GetNumColonizationShips(this, fleet.ID);
				List<PlagueInfo> plagues = new List<PlagueInfo>();
				int resources = planetInfo.Resources;
				List<ColonyFactionInfo> civPopulation;
				bool achievedSuperWorld;
				Kerberos.Sots.Strategy.InhabitedPlanet.Colony.MaintainColony(this, ref colonyInfoForPlanet, ref planetInfo, ref plagues, colonizationSpace, terraformingSpace, fleet, out civPopulation, out achievedSuperWorld, true);
				if (colonyInfoForPlanet.CurrentStage == Kerberos.Sots.Data.ColonyStage.Colony)
					this._app.GameDatabase.InsertGovernmentAction(colonyInfoForPlanet.PlayerID, App.Localize("@GA_UNDERDEVELOPEDCOLONY"), "UnderDevelopedColony", 0, 0);
				if (planetInfo.Resources == 0 && resources != 0)
				{
					OrbitalObjectInfo orbitalObjectInfo = this.GameDatabase.GetOrbitalObjectInfo(planetInfo.ID);
					this.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_PLANET_NO_RESOURCES,
						EventMessage = TurnEventMessage.EM_PLANET_NO_RESOURCES,
						PlayerID = colonyInfoForPlanet.PlayerID,
						SystemID = orbitalObjectInfo.StarSystemID,
						OrbitalID = orbitalObjectInfo.ID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
				this._db.UpdateColony(colonyInfoForPlanet);
				this._db.UpdatePlanet(planetInfo);
				foreach (ColonyFactionInfo colonyFactionInfo in civPopulation)
				{
					ColonyFactionInfo cfi = colonyFactionInfo;
					if (((IEnumerable<ColonyFactionInfo>)colonyInfoForPlanet.Factions).Any<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID == cfi.FactionID)))
						this._db.UpdateCivilianPopulation(cfi);
					else
						this._db.InsertColonyFaction(cfi.OrbitalObjectID, cfi.FactionID, cfi.CivilianPop, cfi.CivPopWeight, cfi.TurnEstablished);
				}
				if (!this._db.CanSupportPlanet(colonyInfoForPlanet.PlayerID, colonyInfoForPlanet.OrbitalObjectID))
				{
					this._db.ClearWaypoints(mission.ID);
					this._db.InsertWaypoint(mission.ID, WaypointType.ReturnHome, new int?());
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_COLONY_SUPPORT_COMPLETE,
						EventMessage = TurnEventMessage.EM_COLONY_SUPPORT_COMPLETE,
						PlayerID = fleet.PlayerID,
						SystemID = mission.TargetSystemID,
						FleetID = fleet.ID,
						OrbitalID = planetInfo.ID,
						ColonyID = colonyInfoForPlanet.ID,
						MissionID = mission.ID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
				else
				{
					TurnEventMessage turnEventMessage = TurnEventMessage.EM_COLONY_SUPPORT;
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_COLONY_SUPPORT,
						EventMessage = turnEventMessage,
						PlayerID = fleet.PlayerID,
						SystemID = mission.TargetSystemID,
						FleetID = fleet.ID,
						OrbitalID = planetInfo.ID,
						ColonyID = colonyInfoForPlanet.ID,
						MissionID = mission.ID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
			}
			else
				Kerberos.Sots.StarFleet.StarFleet.CancelMission(this._app.Game, fleet, true);
			return true;
		}

		public bool DoGateMission(MissionInfo mission, FleetInfo fleet)
		{
			if (fleet.SystemID != mission.TargetSystemID)
				return false;
			if (mission.Duration > 0)
			{
				--mission.Duration;
				this._db.UpdateMission(mission);
				return false;
			}
			List<ShipInfo> list = this._db.GetShipInfoByFleetID(fleet.ID, true).Where<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.Role == ShipRole.GATE)).ToList<ShipInfo>();
			if (list.Count<ShipInfo>() == 0)
				return true;
			ShipInfo shipInfo = list.First<ShipInfo>();
			this._db.UpdateShipParams(shipInfo.ID, ShipParams.HS_GATE_DEPLOYED);
			int num = this._db.InsertFleet(fleet.PlayerID, 0, mission.TargetSystemID, fleet.SupportingSystemID, "GATE", FleetType.FL_GATE);
			this._db.TransferShip(shipInfo.ID, num);
			Matrix? gateShipTransform = GameSession.GetValidGateShipTransform(this.App.Game, mission.TargetSystemID, num);
			if (gateShipTransform.HasValue)
				this.App.GameDatabase.UpdateShipSystemPosition(shipInfo.ID, new Matrix?(gateShipTransform.Value));
			this.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_GATE_DEPLOYED,
				EventMessage = TurnEventMessage.EM_GATE_SHIP_DEPLOYED,
				PlayerID = fleet.PlayerID,
				SystemID = fleet.SystemID,
				SystemID2 = fleet.SupportingSystemID,
				FleetID = fleet.ID,
				MissionID = mission.ID,
				TurnNumber = this.App.GameDatabase.GetTurnCount(),
				ShowsDialog = false
			});
			return true;
		}

		public static Matrix? GetValidGateShipTransform(
		  GameSession game,
		  int systemId,
		  int gateFleet)
		{
			List<CombatZonePositionInfo> combatZonesForSystem = Kerberos.Sots.GameStates.StarSystem.GetCombatZonesForSystem(game, systemId, 1f);
			if (combatZonesForSystem.Count == 0)
			{
				GameSession.Warn(">>>Gate Mission Error: no combat zones in system");
				return new Matrix?();
			}
			FleetInfo fleetInfo1 = game.GameDatabase.GetFleetInfo(gateFleet);
			if (fleetInfo1 == null)
			{
				GameSession.Warn(">>>Gate Mission Error: GateFleet [" + (object)gateFleet + "] is null");
				return new Matrix?();
			}
			if (combatZonesForSystem.Count <= 0)
				return new Matrix?();
			CombatZonePositionInfo zonePositionInfo = combatZonesForSystem.Last<CombatZonePositionInfo>();
			Vector3 position1 = zonePositionInfo.Center;
			if (fleetInfo1.PreviousSystemID.HasValue)
			{
				int? previousSystemId = fleetInfo1.PreviousSystemID;
				int num1 = systemId;
				if ((previousSystemId.GetValueOrDefault() != num1 ? 1 : (!previousSystemId.HasValue ? 1 : 0)) != 0)
				{
					string factionName = fleetInfo1.Type == FleetType.FL_ACCELERATOR ? "loa" : "hiver";
					Faction faction = game.AssetDatabase.Factions.FirstOrDefault<Faction>((Func<Faction, bool>)(x => x.Name == factionName));
					Vector3 starSystemOrigin = game.GameDatabase.GetStarSystemOrigin(systemId);
					Vector3 vector3 = game.GameDatabase.GetStarSystemOrigin(fleetInfo1.PreviousSystemID.Value) - starSystemOrigin;
					vector3.Y = 0.0f;
					double num2 = (double)vector3.Normalize();
					position1 = vector3 * (zonePositionInfo.RadiusLower + faction.EntryPointOffset);
				}
			}
			Vector3 forward = -position1;
			double num3 = (double)forward.Normalize();
			Matrix world = Matrix.CreateWorld(position1, forward, Vector3.UnitY);
			List<EntrySpawnLocation> source = new List<EntrySpawnLocation>();
			for (int index = 0; index < 9; ++index)
			{
				int num1 = index / 3;
				int num2 = (index % 3 + 1) / 2;
				int num4 = index % 2 == 0 ? 1 : -1;
				Vector3 vector3 = Vector3.Transform(new Vector3()
				{
					X = (float)num4 * 3000f * (float)num2,
					Y = 0.0f,
					Z = 3000f * (float)num1
				}, world);
				source.Add(new EntrySpawnLocation()
				{
					Position = vector3
				});
			}
			foreach (FleetInfo fleetInfo2 in game.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_GATE | FleetType.FL_ACCELERATOR).ToList<FleetInfo>())
			{
				if (fleetInfo2 != null && (fleetInfo2.Type == FleetType.FL_GATE || fleetInfo2.Type == FleetType.FL_ACCELERATOR) && fleetInfo2.ID != gateFleet)
				{
					List<EntrySpawnLocation> entrySpawnLocationList = new List<EntrySpawnLocation>();
					foreach (ShipInfo shipInfo in game.GameDatabase.GetShipInfoByFleetID(fleetInfo2.ID, false).ToList<ShipInfo>())
					{
						Matrix? shipSystemPosition = game.GameDatabase.GetShipSystemPosition(shipInfo.ID);
						if (shipSystemPosition.HasValue && shipSystemPosition.HasValue)
						{
							foreach (EntrySpawnLocation entrySpawnLocation in source)
							{
								if ((double)(entrySpawnLocation.Position - shipSystemPosition.Value.Position).LengthSquared < 9000000.0)
									entrySpawnLocationList.Add(entrySpawnLocation);
							}
							foreach (EntrySpawnLocation entrySpawnLocation in entrySpawnLocationList)
								source.Remove(entrySpawnLocation);
						}
					}
				}
			}
			Vector3 position2 = zonePositionInfo.Center;
			if (source.Count > 0)
				position2 = source.First<EntrySpawnLocation>().Position;
			return new Matrix?(Matrix.CreateWorld(position2, forward, Vector3.UnitY));
		}

		public bool DoPatrolMission(MissionInfo mission, FleetInfo fleet)
		{
			return Kerberos.Sots.StarFleet.StarFleet.IsFleetExhausted(this, fleet);
		}

		public bool DoReturnMission(MissionInfo mission, FleetInfo fleet)
		{
			return mission.TargetSystemID == fleet.SystemID;
		}

		public bool DoRelocationMission(MissionInfo mission, FleetInfo fleet)
		{
			if (fleet.Type != FleetType.FL_CARAVAN && this.GameDatabase.GetRemainingSupportPoints(this, fleet.SystemID, fleet.PlayerID) < 0 && (!Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(this.App.GameDatabase, fleet) && !Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(this.App.Game, fleet)))
			{
				Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
				return true;
			}
			this.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_RELOCATION_COMPLETE,
				EventMessage = TurnEventMessage.EM_RELOCATION_COMPLETE,
				PlayerID = fleet.PlayerID,
				SystemID = fleet.SystemID,
				FleetID = fleet.ID,
				TurnNumber = this.App.GameDatabase.GetTurnCount(),
				ShowsDialog = false
			});
			if (fleet.Type == FleetType.FL_CARAVAN)
			{
				foreach (ShipInfo ship in this._app.GameDatabase.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>())
				{
					if (((IEnumerable<DesignSectionInfo>)ship.DesignInfo.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ShipSectionAsset.IsFreighter)))
					{
						this._app.GameDatabase.TransferShip(ship.ID, this._app.GameDatabase.InsertOrGetLimboFleetID(fleet.SystemID, fleet.PlayerID));
						this._app.GameDatabase.InsertFreighterInfo(fleet.SystemID, fleet.PlayerID, ship, true);
					}
					else if (ship.IsSDB() || ship.IsPlatform())
						this._app.GameDatabase.TransferShip(ship.ID, this._app.GameDatabase.InsertOrGetDefenseFleetInfo(fleet.SystemID, fleet.PlayerID).ID);
					else
						this._app.GameDatabase.TransferShip(ship.ID, this._app.GameDatabase.InsertOrGetReserveFleetInfo(fleet.SystemID, fleet.PlayerID).ID);
				}
				if (mission != null)
					this._db.RemoveMission(mission.ID);
				this._app.GameDatabase.RemoveAdmiral(fleet.AdmiralID);
				this._app.GameDatabase.RemoveFleet(fleet.ID);
			}
			else
			{
				fleet.SupportingSystemID = fleet.SystemID;
				this._db.UpdateFleetInfo(fleet);
				if (mission != null)
					this._db.RemoveMission(mission.ID);
			}
			return true;
		}

		public void CompleteMission(FleetInfo fleet)
		{
			double fleetSlaves = Kerberos.Sots.StarFleet.StarFleet.GetFleetSlaves(this, fleet.ID);
			if (fleetSlaves != 0.0)
			{
				List<ColonyInfo> list1 = this.App.GameDatabase.GetColonyInfosForSystem(fleet.SystemID).Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == fleet.PlayerID)).ToList<ColonyInfo>();
				if (list1.Count != 0)
				{
					ColonyInfo colonyInfo = this.Random.Choose<ColonyInfo>((IList<ColonyInfo>)list1);
					List<PlayerInfo> list2 = this._db.GetStandardPlayerInfos().Where<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.FactionID != this._db.GetPlayerFactionID(fleet.PlayerID))).ToList<PlayerInfo>();
					if (list2.Count > 0)
					{
						PlayerInfo rPlayer = this.Random.Choose<PlayerInfo>((IList<PlayerInfo>)list2);
						if (((IEnumerable<ColonyFactionInfo>)colonyInfo.Factions).Any<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID == rPlayer.FactionID)))
						{
							ColonyFactionInfo civPop = ((IEnumerable<ColonyFactionInfo>)colonyInfo.Factions).FirstOrDefault<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID == rPlayer.FactionID));
							if (civPop != null)
							{
								foreach (int shipId in this._db.GetShipsByFleetID(fleet.ID))
									this._db.UpdateShipObtainedSlaves(shipId, 0.0);
								civPop.CivilianPop += fleetSlaves;
								this._db.UpdateCivilianPopulation(civPop);
								this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
								{
									EventType = TurnEventType.EV_SLAVES_DELIVERED,
									EventMessage = TurnEventMessage.EM_SLAVES_DELIVERED,
									PlayerID = fleet.PlayerID,
									ColonyID = colonyInfo.ID,
									CivilianPop = (float)fleetSlaves,
									TurnNumber = this.App.GameDatabase.GetTurnCount(),
									ShowsDialog = false
								});
							}
						}
						else
						{
							foreach (int shipId in this._db.GetShipsByFleetID(fleet.ID))
								this._db.UpdateShipObtainedSlaves(shipId, 0.0);
							this._db.InsertColonyFaction(colonyInfo.OrbitalObjectID, rPlayer.FactionID, fleetSlaves, 0.0f, this._db.GetTurnCount());
							this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
							{
								EventType = TurnEventType.EV_SLAVES_DELIVERED,
								EventMessage = TurnEventMessage.EM_SLAVES_DELIVERED,
								PlayerID = fleet.PlayerID,
								ColonyID = colonyInfo.ID,
								CivilianPop = (float)fleetSlaves,
								TurnNumber = this.App.GameDatabase.GetTurnCount(),
								ShowsDialog = false
							});
						}
					}
				}
			}
			this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_MISSION_COMPLETE,
				EventMessage = TurnEventMessage.EM_MISSION_COMPLETE,
				PlayerID = fleet.PlayerID,
				FleetID = fleet.ID,
				SystemID = fleet.SystemID,
				TurnNumber = this.App.GameDatabase.GetTurnCount(),
				ShowsDialog = false
			});
		}

		public void ProcessMission(MissionInfo mission)
		{
			FleetInfo fleetInfo = this._db.GetFleetInfo(mission.FleetID);
			WaypointInfo waypointForMission = this._db.GetNextWaypointForMission(mission.ID);
			if (fleetInfo == null)
				this._db.RemoveMission(mission.ID);
			else if (waypointForMission == null)
			{
				this._db.RemoveMission(mission.ID);
				GameSession.Trace("Fleet " + fleetInfo.Name + " has completed " + (object)mission.Type + " mission to system " + (object)mission.TargetSystemID);
				this.CompleteMission(fleetInfo);
			}
			else
			{
				if (!Kerberos.Sots.StarFleet.StarFleet.IsFleetWaitingForBuildOrders(this._app, mission.ID, mission.FleetID))
				{
					Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
					foreach (int standardPlayerId in this._db.GetStandardPlayerIDs())
						dictionary.Add(standardPlayerId, StarMap.IsInRange(this._db, standardPlayerId, this._db.GetFleetLocation(fleetInfo.ID, false).Coords, 1f, (Dictionary<int, List<ShipInfo>>)null));
					if (!this.IsFleetInterdicted(fleetInfo) && (waypointForMission.Type == WaypointType.TravelTo || waypointForMission.Type == WaypointType.ReturnHome))
						this.ProcessWaypoint(waypointForMission, fleetInfo, mission.UseDirectRoute);
					waypointForMission = this._db.GetNextWaypointForMission(mission.ID);
					if (waypointForMission == null)
					{
						this._db.RemoveMission(mission.ID);
						GameSession.Trace("Fleet " + fleetInfo.Name + " has completed " + (object)mission.Type + " mission to system " + (object)mission.TargetSystemID);
						this.CompleteMission(fleetInfo);
						return;
					}
					if (waypointForMission.Type != WaypointType.TravelTo && waypointForMission.Type != WaypointType.ReturnHome)
					{
						if (waypointForMission.Type == WaypointType.DoMission && mission.Type == MissionType.SUPPORT && mission.TargetSystemID == fleetInfo.SupportingSystemID)
						{
							this.ProcessWaypoint(waypointForMission, fleetInfo, mission.UseDirectRoute);
							waypointForMission = this._db.GetNextWaypointForMission(mission.ID);
							this.ProcessWaypoint(waypointForMission, fleetInfo, mission.UseDirectRoute);
						}
						else
							this.ProcessWaypoint(waypointForMission, fleetInfo, mission.UseDirectRoute);
					}
					foreach (int standardPlayerId in this._db.GetStandardPlayerIDs())
					{
						if ((fleetInfo.PlayerID == this.ScriptModules.Locust.PlayerID || fleetInfo.PlayerID == this.ScriptModules.Swarmers.PlayerID || fleetInfo.PlayerID == this.ScriptModules.SystemKiller.PlayerID) && (!dictionary[standardPlayerId] && StarMap.IsInRange(this._db, standardPlayerId, this._db.GetFleetLocation(fleetInfo.ID, false).Coords, 1f, (Dictionary<int, List<ShipInfo>>)null)))
						{
							MoveOrderInfo orderInfoByFleetId = this._db.GetMoveOrderInfoByFleetID(fleetInfo.ID);
							if (orderInfoByFleetId != null && StarMap.IsInRange(this._db, standardPlayerId, orderInfoByFleetId.ToSystemID == 0 ? orderInfoByFleetId.ToCoords : this._db.GetStarSystemOrigin(orderInfoByFleetId.ToSystemID), 1f, (Dictionary<int, List<ShipInfo>>)null))
							{
								if (fleetInfo.PlayerID == this.ScriptModules.Locust.PlayerID)
									this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
									{
										EventType = TurnEventType.EV_LOCUST_SPOTTED,
										EventMessage = TurnEventMessage.EM_LOCUST_SPOTTED,
										PlayerID = standardPlayerId,
										TurnNumber = this.App.GameDatabase.GetTurnCount(),
										ShowsDialog = false
									});
								else if (fleetInfo.PlayerID == this.ScriptModules.Swarmers.PlayerID)
									this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
									{
										EventType = TurnEventType.EV_SWARM_QUEEN_SPOTTED,
										EventMessage = TurnEventMessage.EM_SWARM_QUEEN_SPOTTED,
										PlayerID = standardPlayerId,
										TurnNumber = this.App.GameDatabase.GetTurnCount(),
										ShowsDialog = false
									});
								else if (fleetInfo.PlayerID == this.ScriptModules.SystemKiller.PlayerID)
									this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
									{
										EventType = TurnEventType.EV_SYS_KILLER_SPOTTED,
										EventMessage = TurnEventMessage.EM_SYS_KILLER_SPOTTED,
										PlayerID = standardPlayerId,
										TurnNumber = this.App.GameDatabase.GetTurnCount(),
										ShowsDialog = false
									});
							}
						}
					}
				}
				if (waypointForMission != null)
					return;
				this._db.RemoveMission(mission.ID);
				GameSession.Trace("Fleet " + fleetInfo.Name + " has completed " + (object)mission.Type + " mission to system " + (object)mission.TargetSystemID);
				this.CompleteMission(fleetInfo);
			}
		}

		public void Phase2_FleetMovement()
		{
			List<MissionInfo> list1 = this._db.GetMissionInfos().ToList<MissionInfo>();
			this._playerGateMap.Clear();
			foreach (PlayerInfo playerInfo in this._app.GameDatabase.GetPlayerInfos())
				this._playerGateMap.Add(playerInfo.ID, GameSession.GetTotalGateCapacity(this, playerInfo.ID));
			List<MissionInfo> missionInfoList1 = new List<MissionInfo>();
			missionInfoList1.AddRange((IEnumerable<MissionInfo>)list1);
			List<MissionInfo> missionInfoList2 = new List<MissionInfo>();
			foreach (MissionInfo missionInfo in missionInfoList1)
			{
				if (missionInfo.Type == MissionType.INTERCEPT)
				{
					WaypointInfo waypointForMission = this._app.GameDatabase.GetNextWaypointForMission(missionInfo.ID);
					if (waypointForMission == null)
					{
						list1.Remove(missionInfo);
						Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, this._db.GetFleetInfo(missionInfo.FleetID), true);
					}
					else if (waypointForMission.Type == WaypointType.DoMission)
					{
						missionInfoList2.Add(missionInfo);
						list1.Remove(missionInfo);
					}
				}
			}
			list1.InsertRange(0, (IEnumerable<MissionInfo>)missionInfoList2);
			List<MissionInfo> list2 = list1.Where<MissionInfo>((Func<MissionInfo, bool>)(x => x.Type == MissionType.SPECIAL_CONSTRUCT_STN)).ToList<MissionInfo>();
			list1.RemoveAll((Predicate<MissionInfo>)(x => x.Type == MissionType.SPECIAL_CONSTRUCT_STN));
			list1.AddRange((IEnumerable<MissionInfo>)list2);
			foreach (MissionInfo mission in list1)
				this.ProcessMission(mission);
			this.UpdateFleetSupply();
			foreach (NodeLineInfo nodeLineInfo in this.GameDatabase.GetNonPermenantNodeLines().ToList<NodeLineInfo>())
			{
				bool flag = false;
				foreach (MoveOrderInfo moveOrderInfo in this._db.GetMoveOrdersByDestinationSystem(nodeLineInfo.System2ID).ToList<MoveOrderInfo>().Union<MoveOrderInfo>((IEnumerable<MoveOrderInfo>)this._db.GetMoveOrdersByDestinationSystem(nodeLineInfo.System1ID).ToList<MoveOrderInfo>()).ToList<MoveOrderInfo>())
				{
					if ((moveOrderInfo.FromSystemID == nodeLineInfo.System1ID || moveOrderInfo.FromSystemID == nodeLineInfo.System2ID) && (moveOrderInfo.ToSystemID == nodeLineInfo.System1ID || moveOrderInfo.ToSystemID == nodeLineInfo.System2ID))
					{
						flag = GameSession.FleetHasBore(this._db, moveOrderInfo.FleetID);
						if (flag)
							break;
					}
				}
				if (!flag)
				{
					nodeLineInfo.Health -= 6;
					if (nodeLineInfo.Health <= 0)
						this.DissolveNodeLine(nodeLineInfo.ID);
					else
						this._db.UpdateNodeLineHealth(nodeLineInfo.ID, nodeLineInfo.Health);
				}
			}
			foreach (StationInfo stationInfo in this.GameDatabase.GetStationInfos())
			{
				if (stationInfo.DesignInfo.StationType == StationType.DIPLOMATIC && stationInfo.DesignInfo.StationLevel == 5 && this.GameDatabase.GetFactionName(this.GameDatabase.GetPlayerFactionID(stationInfo.PlayerID)) == "zuul")
				{
					SuulkaInfo suulkaByStationId = this.GameDatabase.GetSuulkaByStationID(stationInfo.OrbitalObjectID);
					if (suulkaByStationId == null)
					{
						List<SuulkaInfo> list3 = this.GameDatabase.GetSuulkas().ToList<SuulkaInfo>().Where<SuulkaInfo>((Func<SuulkaInfo, bool>)(x =>
					   {
						   if (x.StationID.HasValue)
						   {
							   int? stationId = x.StationID;
							   if ((stationId.GetValueOrDefault() != 0 ? 0 : (stationId.HasValue ? 1 : 0)) == 0)
								   return false;
						   }
						   if (!x.PlayerID.HasValue)
							   return true;
						   int? playerId = x.PlayerID;
						   if (playerId.GetValueOrDefault() == 0)
							   return playerId.HasValue;
						   return false;
					   })).ToList<SuulkaInfo>();
						if (list3.Count > 0)
						{
							SuulkaInfo suulkaInfo = list3.ElementAt<SuulkaInfo>(new Random().NextInclusive(0, list3.Count - 1));
							this.GameDatabase.UpdateSuulkaStation(suulkaInfo.ID, stationInfo.OrbitalObjectID);
							this.GameDatabase.UpdateSuulkaArrivalTurns(suulkaInfo.ID, new Random().NextInclusive(1, 5));
						}
					}
					else if (suulkaByStationId.ArrivalTurns > 0)
						this.GameDatabase.UpdateSuulkaArrivalTurns(suulkaByStationId.ID, suulkaByStationId.ArrivalTurns - 1);
					else if (!suulkaByStationId.PlayerID.HasValue)
					{
						this.GameDatabase.UpdateSuulkaArrivalTurns(suulkaByStationId.ID, -1);
						int num = this.InsertSuulkaFleet(stationInfo.PlayerID, suulkaByStationId.ID);
						this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_SUULKA_ARRIVES,
							EventMessage = TurnEventMessage.EM_SUULKA_ARRIVES,
							PlayerID = stationInfo.PlayerID,
							SystemID = this.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID).StarSystemID,
							FleetID = num,
							ShipID = suulkaByStationId.ShipID,
							ShowsDialog = true,
							TurnNumber = this.App.GameDatabase.GetTurnCount()
						});
					}
				}
			}
		}

		public bool CompleteColonizationMission(MissionInfo mission, FleetInfo fleet)
		{
			ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(mission.TargetOrbitalObjectID);
			if (colonyInfoForPlanet != null)
			{
				PlanetInfo planetInfo = this._db.GetPlanetInfo(colonyInfoForPlanet.OrbitalObjectID);
				if (!this._db.CanSupportPlanet(colonyInfoForPlanet.PlayerID, colonyInfoForPlanet.OrbitalObjectID))
					return true;
				bool flag = Kerberos.Sots.Strategy.InhabitedPlanet.Colony.IsColonySelfSufficient(this, colonyInfoForPlanet, planetInfo);
				if (colonyInfoForPlanet.PlayerID == fleet.PlayerID && !flag)
				{
					Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(this._app.Game, MissionType.SUPPORT, mission.ID, fleet.ID, mission.TargetSystemID, 1, new int?());
					return false;
				}
				if (flag && !this._app.GetPlayer(colonyInfoForPlanet.PlayerID).IsAI())
				{
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_COLONY_SELF_SUFFICIENT,
						EventMessage = TurnEventMessage.EM_COLONY_SELF_SUFFICIENT,
						EventSoundCueName = string.Format("STRAT_017-01_{0}_ColonySelf-Sufficient", (object)this._db.GetFactionName(this._db.GetPlayerFactionID(colonyInfoForPlanet.PlayerID))),
						PlayerID = fleet.PlayerID,
						SystemID = mission.TargetSystemID,
						OrbitalID = planetInfo.ID,
						ColonyID = colonyInfoForPlanet.ID,
						FleetID = fleet.ID,
						MissionID = mission.ID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = true
					});
					return false;
				}
			}
			return true;
		}

		public bool CompleteEvecuateMission(MissionInfo mission, FleetInfo fleet)
		{
			int colonizationShips = Kerberos.Sots.StarFleet.StarFleet.GetNumColonizationShips(this, fleet.ID);
			if (colonizationShips > 0)
			{
				List<ColonyInfo> list1 = this._db.GetColonyInfosForSystem(fleet.SystemID).Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == fleet.PlayerID)).ToList<ColonyInfo>();
				list1.OrderBy<ColonyInfo, double>((Func<ColonyInfo, double>)(x => Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetMaxCivilianPop(this._db, this._db.GetPlanetInfo(x.OrbitalObjectID)) - this._db.GetCivilianPopulation(x.OrbitalObjectID, 0, false)));
				if (list1.Count == 0)
					return true;
				ColonyInfo colonyInfo1 = list1.Last<ColonyInfo>();
				List<ColonyFactionInfo> list2 = ((IEnumerable<ColonyFactionInfo>)colonyInfo1.Factions).ToList<ColonyFactionInfo>();
				if (list2.Count == 0)
					return true;
				double num = (double)colonizationShips * (double)this._app.AssetDatabase.EvacCivPerCol / (double)list2.Count;
				foreach (ColonyFactionInfo civPop in list2)
				{
					civPop.CivilianPop += num;
					this._db.UpdateCivilianPopulation(civPop);
				}
				if (this._db.GetCivilianPopulation(colonyInfo1.OrbitalObjectID, 0, false) >= (double)colonyInfo1.CivilianWeight * Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetMaxCivilianPop(this._db, this._db.GetPlanetInfo(colonyInfo1.OrbitalObjectID)))
					GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_EVAC_OVERPOPULATION_PLANET, colonyInfo1.PlayerID, new int?(colonyInfo1.ID), new int?(), new int?());
				List<ColonyInfo> list3 = this._db.GetColonyInfosForSystem(mission.TargetSystemID).Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == fleet.PlayerID)).OrderBy<ColonyInfo, double>((Func<ColonyInfo, double>)(x => this.GameDatabase.GetCivilianPopulation(x.OrbitalObjectID, 0, false))).ToList<ColonyInfo>();
				if (list3.Count > 0 && this.GameDatabase.GetCivilianPopulation(list3.Last<ColonyInfo>().OrbitalObjectID, 0, false) > 0.0)
				{
					ColonyInfo colonyInfo2 = list3.Last<ColonyInfo>();
					mission.TargetOrbitalObjectID = colonyInfo2.OrbitalObjectID;
					this.GameDatabase.UpdateMission(mission);
					Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(this._app.Game, MissionType.EVACUATE, mission.ID, fleet.ID, mission.TargetSystemID, 0, new int?());
				}
			}
			return true;
		}

		public void Phase3_ReactionMovement()
		{
			this._reactions.Clear();
			List<FleetInfo> source = new List<FleetInfo>();
			foreach (FleetInfo fleetInfo in this._db.GetFleetInfos(FleetType.FL_NORMAL))
			{
				MissionInfo missionByFleetId = this._db.GetMissionByFleetID(fleetInfo.ID);
				if (missionByFleetId != null && fleetInfo.SystemID != 0 && missionByFleetId.TargetSystemID == fleetInfo.SystemID)
					source.Add(fleetInfo);
			}
			Stopwatch stopwatch = Stopwatch.StartNew();
			if (source.Count<FleetInfo>() == 0 && !this.App.GameSetup.IsMultiplayer)
			{
				if (ScriptHost.AllowConsole)
				{
					stopwatch.Restart();
					this.Phase4_Combat();
					App.Log.Trace("  End Mission Combat: " + (object)stopwatch.ElapsedMilliseconds + "ms", "ai");
				}
				else
					this.Phase4_Combat();
			}
			else
			{
				stopwatch.Restart();
				foreach (FleetInfo fleetInfo1 in this._db.GetFleetInfos(FleetType.FL_NORMAL))
				{
					if (fleetInfo1.SystemID != 0)
					{
						MissionInfo missionByFleetId = this._db.GetMissionByFleetID(fleetInfo1.ID);
						if (fleetInfo1.AdmiralID != 0 && !fleetInfo1.IsDefenseFleet && (!fleetInfo1.IsReserveFleet && !fleetInfo1.IsLimboFleet) && fleetInfo1.Type != FleetType.FL_CARAVAN)
						{
							bool flag = false;
							if (missionByFleetId != null)
							{
								if (missionByFleetId.Type == MissionType.PATROL && missionByFleetId.TargetSystemID == fleetInfo1.SystemID)
									flag = true;
								else
									continue;
							}
							Faction faction = this.AssetDatabase.GetFaction(this._db.GetPlayerFactionID(fleetInfo1.PlayerID));
							int num1 = 0;
							if (!faction.CanUseGate() || !this._playerGateMap.TryGetValue(fleetInfo1.PlayerID, out num1) || this._db.GetFleetCruiserEquivalent(fleetInfo1.ID) <= num1)
							{
								AdmiralInfo admiralInfo = this._db.GetAdmiralInfo(fleetInfo1.AdmiralID);
								List<AdmiralInfo.TraitType> list = this._db.GetAdmiralTraits(fleetInfo1.AdmiralID).ToList<AdmiralInfo.TraitType>();
								float num2 = (float)((double)admiralInfo.ReactionBonus * (double)this._db.GetStratModifier<float>(StratModifiers.AdmiralReactionModifier, fleetInfo1.PlayerID) * (flag ? 1.5 : 1.0));
								float fleetTravelSpeed1 = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this, fleetInfo1.ID, false);
								float fleetTravelSpeed2 = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this, fleetInfo1.ID, true);
								ReactionInfo reactionInfo = new ReactionInfo();
								reactionInfo.fleetsInRange = new List<FleetInfo>();
								reactionInfo.fleet = fleetInfo1;
								foreach (FleetInfo fleetInfo2 in source)
								{
									if (fleetInfo2.PlayerID != fleetInfo1.PlayerID && fleetInfo2.SystemID != fleetInfo1.SystemID && StrategicAI.GetDiplomacyStateRank(this._db.GetDiplomacyStateBetweenPlayers(fleetInfo1.PlayerID, fleetInfo2.PlayerID)) < StrategicAI.GetDiplomacyStateRank(DiplomacyState.PEACE))
									{
										float num3 = num2;
										if (list.Contains(AdmiralInfo.TraitType.Technophobe) && this._db.GetFactionName(this._db.GetPlayerInfo(fleetInfo2.PlayerID).FactionID) == "loa")
											num3 -= 15f;
										if ((double)num3 >= 0.0 && ((GameSession.ForceReactionHack ? 1.0 : (double)this._random.Next(1, 100)) <= (double)num3 && StarMap.IsInRange(this._db, fleetInfo1.PlayerID, this._db.GetFleetLocation(fleetInfo2.ID, false).Coords, 1f, (Dictionary<int, List<ShipInfo>>)null)))
										{
											int? travelTime = Kerberos.Sots.StarFleet.StarFleet.GetTravelTime(this, fleetInfo1, fleetInfo2.SystemID, faction.CanUseNodeLine(new bool?(true)), new float?(fleetTravelSpeed1), new float?(fleetTravelSpeed2));
											if (GameSession.ForceReactionHack || travelTime.HasValue && travelTime.Value <= 1)
												reactionInfo.fleetsInRange.Add(fleetInfo2);
										}
									}
								}
								if (reactionInfo.fleetsInRange.Count > 0)
									this._reactions.Add(reactionInfo);
							}
						}
					}
				}
				if (ScriptHost.AllowConsole)
					App.Log.Trace("  Reactions Calculated: " + (object)stopwatch.ElapsedMilliseconds + "ms", "ai");
				if (this._app.GameSetup.IsMultiplayer && !this._app.Network.IsHosting)
					return;
				ReactionInfo reactionForPlayer = this.GetNextReactionForPlayer(this._app.LocalPlayer.ID);
				if (reactionForPlayer != null)
				{
					this._app.GetGameState<StarMapState>().ShowReactionOverlay(reactionForPlayer.fleet.SystemID);
				}
				else
				{
					if (this._app.GameSetup.IsMultiplayer)
						return;
					stopwatch.Restart();
					this.Phase4_Combat();
					if (!ScriptHost.AllowConsole)
						return;
					App.Log.Trace("  End Reaction Combat: " + (object)stopwatch.ElapsedMilliseconds + "ms", "ai");
				}
			}
		}

		public void Phase4_Combat()
		{
			if (ScriptHost.AllowConsole && GameSession.SkipCombatHack)
				return;
			this.CheckRandomEncounters();
			this.CheckGMEncounters();
			this.CheckSpecialCaseEncounters();
			this.ScriptModules.UpdateEasterEggs(this);
			int num1 = 3;
			List<StarSystemInfo> starSystemInfoList = new List<StarSystemInfo>();
			List<int> intList1 = new List<int>();
			List<EncounterInfo> list1 = this._db.GetEncounterInfos().ToList<EncounterInfo>();
			List<AsteroidMonitorInfo> source1 = new List<AsteroidMonitorInfo>();
			List<MorrigiRelicInfo> source2 = new List<MorrigiRelicInfo>();
			foreach (EncounterInfo encounterInfo in list1)
			{
				if (encounterInfo.Type == EasterEgg.EE_ASTEROID_MONITOR)
				{
					AsteroidMonitorInfo asteroidMonitorInfo = this._app.GameDatabase.GetAsteroidMonitorInfo(encounterInfo.Id);
					if (asteroidMonitorInfo != null)
						source1.Add(asteroidMonitorInfo);
				}
				else if (encounterInfo.Type == EasterEgg.EE_MORRIGI_RELIC)
				{
					MorrigiRelicInfo morrigiRelicInfo = this._app.GameDatabase.GetMorrigiRelicInfo(encounterInfo.Id);
					if (morrigiRelicInfo != null)
						source2.Add(morrigiRelicInfo);
				}
			}
			int num2 = this._app.Game.ScriptModules.AsteroidMonitor != null ? this._app.Game.ScriptModules.AsteroidMonitor.PlayerID : 0;
			int num3 = this._app.Game.ScriptModules.MorrigiRelic != null ? this._app.Game.ScriptModules.MorrigiRelic.PlayerID : 0;
			List<PirateBaseInfo> list2 = this._app.GameDatabase.GetPirateBaseInfos().ToList<PirateBaseInfo>();
			foreach (PendingCombat combat in this.m_Combats)
			{
				if (this._db.GetStarSystemInfo(combat.SystemID) == (StarSystemInfo)null)
					combat.SystemID = 0;
			}
			this.m_Combats.RemoveAll((Predicate<PendingCombat>)(x => x.SystemID == 0));
			foreach (StarSystemInfo starSystemInfo in this._db.GetStarSystemInfos().ToList<StarSystemInfo>())
			{
				StarSystemInfo system = starSystemInfo;
				if (!system.IsDeepSpace)
				{
					PendingCombat pcom = new PendingCombat();
					pcom.SystemID = system.ID;
					List<int> playersPresent = new List<int>();
					List<int> intList2 = new List<int>();
					bool flag1 = false;
					List<FleetInfo> list3 = this._db.GetFleetInfoBySystemID(system.ID, FleetType.FL_NORMAL | FleetType.FL_GATE | FleetType.FL_ACCELERATOR).ToList<FleetInfo>();
					foreach (FleetInfo fleet in list3)
					{
						if (!fleet.IsReserveFleet && this._db.GetShipInfoByFleetID(fleet.ID, false).Any<ShipInfo>())
						{
							if (this.IsFleetInterdicted(fleet))
							{
								if (this._db.GetMissionByFleetID(fleet.ID) != null)
								{
									List<int> intList3 = this.CheckForInterdiction(fleet, fleet.SystemID);
									if (!playersPresent.Contains(fleet.PlayerID))
										playersPresent.Add(fleet.PlayerID);
									foreach (int fleetID in intList3)
									{
										int playerId = this._db.GetFleetInfo(fleetID).PlayerID;
										if (!playersPresent.Contains(playerId))
											playersPresent.Add(playerId);
									}
									flag1 = true;
								}
							}
							else if (!playersPresent.Contains(fleet.PlayerID))
							{
								bool flag2 = true;
								if (fleet.PlayerID == num2)
								{
									AsteroidMonitorInfo asteroidMonitorInfo = source1.FirstOrDefault<AsteroidMonitorInfo>((Func<AsteroidMonitorInfo, bool>)(x => x.SystemId == system.ID));
									if (asteroidMonitorInfo != null && !asteroidMonitorInfo.IsAggressive)
										flag2 = false;
								}
								else if (fleet.PlayerID == num3)
								{
									MorrigiRelicInfo morrigiRelicInfo = source2.FirstOrDefault<MorrigiRelicInfo>((Func<MorrigiRelicInfo, bool>)(x => x.SystemId == system.ID));
									if (morrigiRelicInfo != null && !morrigiRelicInfo.IsAggressive)
										flag2 = false;
								}
								if (flag2)
								{
									MissionInfo missionByFleetId = this._db.GetMissionByFleetID(fleet.ID);
									if (missionByFleetId != null && missionByFleetId.Type != MissionType.INTERDICTION && missionByFleetId.Type != MissionType.PIRACY || missionByFleetId == null)
									{
										playersPresent.Add(fleet.PlayerID);
										flag1 = true;
									}
								}
							}
						}
					}
					foreach (PlanetInfo systemPlanetInfo in this._db.GetStarSystemPlanetInfos(system.ID))
					{
						ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(systemPlanetInfo.ID);
						if (colonyInfoForPlanet != null && !playersPresent.Contains(colonyInfoForPlanet.PlayerID))
							playersPresent.Add(colonyInfoForPlanet.PlayerID);
						if (colonyInfoForPlanet != null && !intList2.Contains(colonyInfoForPlanet.PlayerID))
							intList2.Add(colonyInfoForPlanet.PlayerID);
					}
					foreach (StationInfo stationInfo in this._db.GetStationForSystem(system.ID))
					{
						if (!playersPresent.Contains(stationInfo.PlayerID))
							playersPresent.Add(stationInfo.PlayerID);
						if (!intList2.Contains(stationInfo.PlayerID))
							intList2.Add(stationInfo.PlayerID);
					}
					bool flag3 = false;
					foreach (FleetInfo fleetInfo in list3)
					{
						if (!playersPresent.Contains(fleetInfo.PlayerID))
						{
							MissionInfo missionByFleetId = this.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
							if (missionByFleetId != null && missionByFleetId.Type == MissionType.PIRACY && this.GameDatabase.GetPiracyFleetDetectionInfoForFleet(fleetInfo.ID).Any<PiracyFleetDetectionInfo>((Func<PiracyFleetDetectionInfo, bool>)(x => playersPresent.Any<int>((Func<int, bool>)(y => y == x.PlayerID)))))
							{
								playersPresent.Add(fleetInfo.PlayerID);
								flag3 = true;
							}
							else if (missionByFleetId != null && missionByFleetId.Type == MissionType.PIRACY)
								flag3 = true;
						}
					}
					List<int> intList4 = new List<int>();
					foreach (int num4 in intList2)
					{
						PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(num4);
						foreach (int playerID2 in intList2)
						{
							if (num4 != playerID2 && playerInfo.isStandardPlayer && (this._db.ShouldPlayersFight(num4, playerID2, system.ID) && !intList4.Contains(num4)))
								intList4.Add(num4);
						}
					}
					foreach (int num4 in intList4)
						intList2.Remove(num4);
					foreach (int playerID1 in playersPresent)
					{
						foreach (int playerID2 in playersPresent)
						{
							if (playerID1 != playerID2 && this._db.ShouldPlayersFight(playerID1, playerID2, system.ID))
							{
								flag3 = true;
								break;
							}
						}
					}
					if (!flag3)
						playersPresent.Clear();
					if (playersPresent.Count<int>() > 1 & flag1)
					{
						foreach (FleetInfo fleetInfo in this._db.GetFleetInfoBySystemID(system.ID, FleetType.FL_NORMAL | FleetType.FL_GATE | FleetType.FL_ACCELERATOR))
						{
							if (playersPresent.Contains(fleetInfo.PlayerID))
							{
								MissionInfo missionByFleetId = this.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
								if (missionByFleetId == null || missionByFleetId.Type != MissionType.PIRACY || this.GameDatabase.GetPiracyFleetDetectionInfoForFleet(fleetInfo.ID).Any<PiracyFleetDetectionInfo>((Func<PiracyFleetDetectionInfo, bool>)(x => playersPresent.Any<int>((Func<int, bool>)(y => y == x.PlayerID)))))
								{
									pcom.FleetIDs.Add(fleetInfo.ID);
									GameSession.Trace("Adding fleet " + (object)fleetInfo.ID + " for player " + (object)fleetInfo.PlayerID);
								}
							}
						}
						PirateBaseInfo pirateBaseInfo = list2.FirstOrDefault<PirateBaseInfo>((Func<PirateBaseInfo, bool>)(x => x.SystemId == system.ID));
						if (pirateBaseInfo != null)
							pcom.FleetIDs.Add(this._app.Game.ScriptModules.Pirates.SpawnPirateFleet(this._app.Game, system.ID, pirateBaseInfo.NumShips));
						pcom.Type = CombatType.CT_Meeting;
						pcom.PlayersInCombat = playersPresent;
						pcom.NPCPlayersInCombat = this._db.GetNPCPlayersBySystem(pcom.SystemID);
						if (pcom.FleetIDs.Count<int>() > 0)
						{
							int num4 = 0;
							foreach (int num5 in pcom.PlayersInCombat)
							{
								int player = num5;
								if (this._app.GameDatabase.GetPlayerInfo(player).isStandardPlayer && !intList2.Contains(player) && list3.Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
							   {
								   if (x.PlayerID == player)
									   return pcom.FleetIDs.Contains(x.ID);
								   return false;
							   })).Count<FleetInfo>() > num4)
									num4 = list3.Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
								   {
									   if (x.PlayerID == player)
										   return pcom.FleetIDs.Contains(x.ID);
									   return false;
								   })).Count<FleetInfo>();
							}
							if (num4 > num1)
								num4 = num1;
							int num6 = 1;
							bool flag2 = false;
							bool flag4 = false;
							bool flag5 = false;
							bool flag6 = false;
							foreach (int fleetId in pcom.FleetIDs)
							{
								FleetInfo fleetInfo = this._db.GetFleetInfo(fleetId);
								PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(fleetInfo.PlayerID);
								bool flag7 = false;
								if (fleetInfo.IsGateFleet || fleetInfo.IsAcceleratorFleet)
								{
									flag2 = true;
								}
								else
								{
									if (this._app.Game.ScriptModules.Swarmers != null && this._app.Game.ScriptModules.Swarmers.PlayerID == playerInfo.ID || this._app.Game.ScriptModules.MorrigiRelic != null && this._app.Game.ScriptModules.MorrigiRelic.PlayerID == playerInfo.ID || this._app.Game.ScriptModules.AsteroidMonitor != null && this._app.Game.ScriptModules.AsteroidMonitor.PlayerID == playerInfo.ID)
									{
										flag5 = true;
										flag7 = true;
									}
									if (fleetInfo.IsNormalFleet && !flag7)
										flag6 = true;
									if (!playerInfo.isStandardPlayer && !flag4 && !flag7 && this.m_Combats.Where<PendingCombat>((Func<PendingCombat, bool>)(x => x.SystemID == pcom.SystemID)).Count<PendingCombat>() == 0)
									{
										if (intList2.Count > 0)
										{
											PendingCombat pendingCombat = new PendingCombat();
											pendingCombat.FleetIDs = pcom.FleetIDs;
											pendingCombat.NPCPlayersInCombat = pcom.NPCPlayersInCombat;
											pendingCombat.PlayersInCombat = pcom.PlayersInCombat;
											pendingCombat.SystemID = pcom.SystemID;
											pendingCombat.ConflictID = GameSession.GetNextUniqueCombatID();
											pendingCombat.CardID = num6;
											++num6;
											this.m_Combats.Add(pendingCombat);
											flag4 = true;
										}
									}
									else if (!flag7 && !intList2.Contains(fleetInfo.PlayerID) && (fleetInfo.Type != FleetType.FL_GATE && fleetInfo.Type != FleetType.FL_ACCELERATOR) && this.m_Combats.Where<PendingCombat>((Func<PendingCombat, bool>)(x => x.SystemID == pcom.SystemID)).Count<PendingCombat>() < num4)
									{
										PendingCombat pendingCombat = new PendingCombat();
										pendingCombat.FleetIDs = pcom.FleetIDs;
										pendingCombat.NPCPlayersInCombat = pcom.NPCPlayersInCombat;
										pendingCombat.PlayersInCombat = pcom.PlayersInCombat;
										pendingCombat.SystemID = pcom.SystemID;
										pendingCombat.ConflictID = GameSession.GetNextUniqueCombatID();
										pendingCombat.CardID = num6;
										++num6;
										this.m_Combats.Add(pendingCombat);
									}
								}
							}
							if ((flag2 | flag5) & flag6 && this.m_Combats.Where<PendingCombat>((Func<PendingCombat, bool>)(x => x.SystemID == pcom.SystemID)).Count<PendingCombat>() == 0 && intList2.Count > 0)
							{
								PendingCombat pendingCombat = new PendingCombat();
								pendingCombat.FleetIDs = pcom.FleetIDs;
								pendingCombat.NPCPlayersInCombat = pcom.NPCPlayersInCombat;
								pendingCombat.PlayersInCombat = pcom.PlayersInCombat;
								pendingCombat.SystemID = pcom.SystemID;
								pendingCombat.ConflictID = GameSession.GetNextUniqueCombatID();
								pendingCombat.CardID = num6;
								int num5 = num6 + 1;
								this.m_Combats.Add(pendingCombat);
							}
							foreach (PendingCombat pendingCombat in this.m_Combats.ToList<PendingCombat>())
							{
								if (pendingCombat.CardID > 1)
								{
									foreach (int fleetId in pendingCombat.FleetIDs)
									{
										if (!this._app.GameDatabase.GetPlayerInfo(this._app.GameDatabase.GetFleetInfo(fleetId).PlayerID).isStandardPlayer)
											this.m_Combats.Remove(pendingCombat);
									}
								}
							}
							foreach (int towardsPlayerID in playersPresent)
							{
								foreach (int playerID in playersPresent)
								{
									if (towardsPlayerID != playerID && this.GameDatabase.GetDiplomacyInfo(playerID, towardsPlayerID).State == DiplomacyState.UNKNOWN)
										this.GameDatabase.ChangeDiplomacyState(playerID, towardsPlayerID, DiplomacyState.NEUTRAL);
								}
							}
							foreach (int num5 in playersPresent)
							{
								if (!intList1.Contains(num5))
									intList1.Add(num5);
							}
							GameSession.Trace("Combat pending in " + system.Name + " system.");
						}
					}
					if (!this.m_Combats.Any<PendingCombat>((Func<PendingCombat, bool>)(x => x.SystemID == system.ID)))
					{
						foreach (FleetInfo fleetInfo in list3)
						{
							MissionInfo missionByFleetId = this.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
							if (missionByFleetId != null && missionByFleetId.Type == MissionType.PIRACY)
								playersPresent.Add(fleetInfo.PlayerID);
							if (playersPresent.Contains(fleetInfo.PlayerID))
							{
								pcom.FleetIDs.Add(fleetInfo.ID);
								GameSession.Trace("PIRACY COMBATS - Adding fleet " + (object)fleetInfo.ID + " for player " + (object)fleetInfo.PlayerID);
							}
						}
						foreach (FleetInfo fleetInfo in list3)
						{
							MissionInfo missionByFleetId = this.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
							if (missionByFleetId != null && missionByFleetId.Type == MissionType.PIRACY && !this.GameDatabase.GetPiracyFleetDetectionInfoForFleet(fleetInfo.ID).Any<PiracyFleetDetectionInfo>((Func<PiracyFleetDetectionInfo, bool>)(x => playersPresent.Any<int>((Func<int, bool>)(y => y == x.PlayerID)))))
							{
								List<FreighterInfo> list4 = this.GameDatabase.GetFreighterInfosForSystem(system.ID).ToList<FreighterInfo>();
								if (list4.Count != 0)
								{
									foreach (FreighterInfo freighterInfo in list4)
									{
										if (!playersPresent.Contains(freighterInfo.PlayerId))
											playersPresent.Add(freighterInfo.PlayerId);
									}
									if (!playersPresent.Contains(fleetInfo.PlayerID))
										playersPresent.Add(fleetInfo.PlayerID);
									this.m_Combats.Add(new PendingCombat()
									{
										FleetIDs = pcom.FleetIDs,
										NPCPlayersInCombat = this._db.GetNPCPlayersBySystem(pcom.SystemID),
										PlayersInCombat = playersPresent,
										SystemID = pcom.SystemID,
										ConflictID = GameSession.GetNextUniqueCombatID(),
										CardID = 1,
										Type = CombatType.CT_Piracy
									});
									break;
								}
							}
						}
					}
				}
			}
			List<int> list5 = this.App.GameDatabase.GetStandardPlayerIDs().ToList<int>();
			foreach (int num4 in list5)
			{
				float stratModifier = this._db.GetStratModifier<float>(StratModifiers.OddsOfRandomEncounter, num4);
				if (intList1.Contains(num4))
				{
					this.App.GameDatabase.UpdatePlayerLastCombatTurn(num4, this.App.GameDatabase.GetTurnCount());
					float val1 = stratModifier - this.App.AssetDatabase.RandomEncDecOddsCombat * (this._db.GetNameValue<float>("RandomEncounterFrequency") / 100f);
					this.App.GameDatabase.SetStratModifier(StratModifiers.OddsOfRandomEncounter, num4, (object)Math.Max(val1, 0.0f));
				}
				else
				{
					float val2 = stratModifier + this.App.AssetDatabase.RandomEncIncOddsRounds * (float)(this.m_Combats.Count - 1) + this.App.AssetDatabase.RandomEncIncOddsIdle * (this._db.GetNameValue<float>("RandomEncounterFrequency") / 100f);
					this.App.GameDatabase.SetStratModifier(StratModifiers.OddsOfRandomEncounter, num4, (object)Math.Max(0.0f, val2));
				}
			}
			if (this.m_Combats.Count != 0)
				this.App.GameDatabase.SetLastTurnWithCombat(this.App.GameDatabase.GetTurnCount());
			if (this.App.AssetDatabase.RandomEncTurnsToResetOdds <= this.App.GameDatabase.GetTurnCount() - this.App.GameDatabase.GetLastTurnWithCombat())
			{
				foreach (int playerId in list5)
					this.App.GameDatabase.SetStratModifier(StratModifiers.OddsOfRandomEncounter, playerId, (object)(float)((double)this.App.AssetDatabase.RandomEncBaseOdds * ((double)this._db.GetNameValue<float>("RandomEncounterFrequency") / 100.0)));
			}
			if (ScriptHost.AllowConsole)
			{
				foreach (PendingCombat combat in this.m_Combats)
				{
					foreach (int fleetId in combat.FleetIDs)
					{
						if (this._db.GetFleetInfo(fleetId) == null)
							GameSession.Warn("Fleet " + (object)fleetId + " does not exist!");
					}
				}
			}
			List<int> intList5 = new List<int>();
			List<ColonyTrapInfo> list6 = this.GameDatabase.GetColonyTrapInfos().ToList<ColonyTrapInfo>();
			foreach (ColonyTrapInfo colonyTrapInfo in list6)
			{
				ColonyTrapInfo cti = colonyTrapInfo;
				if (!intList5.Contains(cti.SystemID))
				{
					intList5.Add(cti.SystemID);
					if (!this.m_Combats.Any<PendingCombat>((Func<PendingCombat, bool>)(x => x.SystemID == cti.SystemID)))
					{
						Dictionary<int, int> dictionary1 = new Dictionary<int, int>();
						Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
						List<int> intList2 = new List<int>();
						List<int> intList3 = new List<int>();
						List<ColonyTrapInfo> list3 = list6.Where<ColonyTrapInfo>((Func<ColonyTrapInfo, bool>)(x => x.SystemID == cti.SystemID)).ToList<ColonyTrapInfo>();
						List<FleetInfo> list4 = list6.Select<ColonyTrapInfo, FleetInfo>((Func<ColonyTrapInfo, FleetInfo>)(x => this._db.GetFleetInfo(x.FleetID))).ToList<FleetInfo>();
						foreach (FleetInfo fleetInfo1 in this._db.GetFleetInfoBySystemID(cti.SystemID, FleetType.FL_NORMAL).ToList<FleetInfo>())
						{
							PlayerInfo pi = this._db.GetPlayerInfo(fleetInfo1.PlayerID);
							if (pi != null && pi.isStandardPlayer)
							{
								Faction faction = this._app.AssetDatabase.Factions.FirstOrDefault<Faction>((Func<Faction, bool>)(x => x.ID == pi.FactionID));
								if (faction != null && !(faction.Name == "morrigi"))
								{
									intList2.Add(fleetInfo1.ID);
									if (!intList3.Contains(fleetInfo1.PlayerID))
										intList3.Add(fleetInfo1.PlayerID);
									MissionInfo mi = this._db.GetMissionByFleetID(fleetInfo1.ID);
									if (mi != null && mi.Type == MissionType.COLONIZATION)
									{
										ColonyTrapInfo targetTrap = list3.FirstOrDefault<ColonyTrapInfo>((Func<ColonyTrapInfo, bool>)(x => x.PlanetID == mi.TargetOrbitalObjectID));
										if (targetTrap != null)
										{
											FleetInfo fleetInfo2 = list4.FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(x => x.ID == targetTrap.FleetID));
											if (fleetInfo2 != null && fleetInfo2.PlayerID != fleetInfo1.PlayerID && (this._db.GetDiplomacyStateBetweenPlayers(fleetInfo2.PlayerID, fleetInfo1.PlayerID) != DiplomacyState.ALLIED && !dictionary1.Keys.Contains<int>(fleetInfo1.ID)))
											{
												dictionary1.Add(fleetInfo1.ID, fleetInfo2.ID);
												dictionary2.Add(targetTrap.ID, fleetInfo2.PlayerID);
												intList2.Add(fleetInfo2.ID);
												if (!intList3.Contains(fleetInfo2.PlayerID))
													intList3.Add(fleetInfo2.PlayerID);
											}
										}
									}
								}
							}
						}
						if (dictionary1.Count > 0)
							this.m_Combats.Add(new PendingCombat()
							{
								FleetIDs = intList2,
								Type = CombatType.CT_Colony_Trap,
								NPCPlayersInCombat = new List<int>(),
								PlayersInCombat = intList3,
								SystemID = cti.SystemID,
								ConflictID = GameSession.GetNextUniqueCombatID(),
								CardID = 1
							});
					}
				}
			}
			this.fleetsInCombat.Clear();
			List<int> list7 = this.GameDatabase.GetStandardPlayerIDs().ToList<int>();
			foreach (PendingCombat combat in this.m_Combats)
			{
				foreach (int playerid in list7)
				{
					if (!combat.PlayersInCombat.Contains(playerid) && StarMap.IsInRange(this.GameDatabase, playerid, combat.SystemID))
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_COMBAT_DETECTED,
							EventMessage = TurnEventMessage.EM_COMBAT_DETECTED,
							PlayerID = playerid,
							SystemID = combat.SystemID,
							TurnNumber = this.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
				}
			}
			foreach (PendingCombat combat in this.m_Combats)
			{
				if (combat.Type != CombatType.CT_Colony_Trap)
				{
					bool flag = true;
					List<FleetInfo> fleets = new List<FleetInfo>();
					int num4 = 0;
					int num5 = 0;
					int num6 = 0;
					foreach (int num7 in combat.PlayersInCombat)
					{
						int playerId = num7;
						Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(playerId);
						if (playerObject == null || !playerObject.IsStandardPlayer || !playerObject.IsAI())
							flag = false;
						FleetInfo selectedFleetInfo = combat.FleetIDs.Select<int, FleetInfo>((Func<int, FleetInfo>)(fleetId => this._db.GetFleetInfo(fleetId))).FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(fleetInfo =>
					  {
						  if (fleetInfo != null && fleetInfo.PlayerID == playerId)
							  return fleetInfo.IsNormalFleet;
						  return false;
					  }));
						if (selectedFleetInfo != null)
						{
							fleets.Add(selectedFleetInfo);
							if (this.fleetsInCombat.Any<FleetInfo>((Func<FleetInfo, bool>)(x => x.ID == selectedFleetInfo.ID)))
								this.SubtractExtendedCombatEndurance(selectedFleetInfo);
							else
								this.fleetsInCombat.Add(selectedFleetInfo);
							if (playerObject.Faction.Name == "loa" && playerObject.IsAI())
							{
								MissionInfo missionByFleetId = this._db.GetMissionByFleetID(selectedFleetInfo.ID);
								if (missionByFleetId != null)
									Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromCompositionID(this, selectedFleetInfo.ID, new int?(playerObject.GetAI().GetLoaCompositionIDByMission(missionByFleetId.Type)), MissionType.NO_MISSION);
							}
						}
						if (this.ScriptModules != null && this.ScriptModules.IsEncounterPlayer(playerObject.ID))
						{
							++num4;
						}
						else
						{
							++num5;
							if (playerObject.IsAI())
								++num6;
						}
					}
					foreach (FleetInfo fleetInfo in this._db.GetFleetInfoBySystemID(combat.SystemID, FleetType.FL_GATE | FleetType.FL_ACCELERATOR).ToList<FleetInfo>())
					{
						if (!fleets.Contains(fleetInfo))
							fleets.Add(fleetInfo);
					}
					if (num5 == 1 && num5 == num6 && (num4 > 0 || this._db.GetColonyInfosForSystem(combat.SystemID).Count<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.IsIndependentColony(this._app))) > 0) && CombatSimulatorRandoms.Simulate(this, combat.SystemID, fleets))
						combat.complete = true;
					else if (flag)
					{
						CombatSimulator.Simulate(this, combat.SystemID, fleets);
						combat.complete = true;
					}
				}
			}
			this.m_Combats.RemoveAll((Predicate<PendingCombat>)(x => x.complete));
			List<int> assignedFleets = new List<int>();
			foreach (PendingCombat combat in this.m_Combats)
			{
				foreach (int num4 in combat.PlayersInCombat)
				{
					int playerId2 = num4;
					Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(playerId2);
					if (playerObject.Faction.Name == "loa")
					{
						foreach (int fleetId in combat.FleetIDs)
						{
							FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(fleetId);
							if (fleetInfo != null && !Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(this, fleetInfo) && (!fleetInfo.IsAcceleratorFleet && fleetInfo.PlayerID == playerId2) && this._app.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false).Any<ShipInfo>((Func<ShipInfo, bool>)(x => !x.ShipSystemPosition.HasValue)))
								Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromComposition(this, fleetInfo.ID, MissionType.NO_MISSION);
						}
					}
					if (playerObject.IsAI())
					{
						combat.CombatResolutionSelections[playerId2] = ResolutionType.AUTO_RESOLVE;
						combat.CombatStanceSelections[playerId2] = AutoResolveStance.AGGRESSIVE;
						IEnumerable<FleetInfo> source3 = combat.FleetIDs.Select<int, FleetInfo>((Func<int, FleetInfo>)(x => this._db.GetFleetInfo(x))).Where<FleetInfo>((Func<FleetInfo, bool>)(x => x.PlayerID == playerId2));
						IEnumerable<FleetInfo> source4 = source3.Where<FleetInfo>((Func<FleetInfo, bool>)(x => !assignedFleets.Contains(x.ID)));
						FleetInfo fleetInfo = !source4.Any<FleetInfo>() ? this.GetBestCombatFleet(source3.ToList<FleetInfo>()) : this.GetBestCombatFleet(source4.ToList<FleetInfo>());
						if (fleetInfo != null)
						{
							combat.SelectedPlayerFleets[playerId2] = fleetInfo.ID;
							if (!assignedFleets.Contains(fleetInfo.ID))
								assignedFleets.Add(fleetInfo.ID);
						}
						else
							combat.SelectedPlayerFleets[playerId2] = 0;
					}
				}
			}
			if (this.App.GameSetup.IsMultiplayer)
			{
				foreach (Kerberos.Sots.PlayerFramework.Player player in this.m_Players)
				{
					if (player.IsAI())
						this.App.Network.SendCombatResponses((IEnumerable<PendingCombat>)this.m_Combats, player.ID);
				}
			}
			if (this.m_Combats.Any<PendingCombat>((Func<PendingCombat, bool>)(x =>
		   {
			   if (x.CombatResults == null)
				   return x.PlayersInCombat.Contains(this.LocalPlayer.ID);
			   return false;
		   })))
				this.App.UI.CreateDialog((Dialog)new EncounterDialog(this._app, this.m_Combats.Where<PendingCombat>((Func<PendingCombat, bool>)(x => x.PlayersInCombat.Contains(this.LocalPlayer.ID))).ToList<PendingCombat>()), null);
			else if (!this.App.GameSetup.IsMultiplayer)
			{
				if (this.m_Combats.Count<PendingCombat>() > 0)
					this.LaunchNextCombat();
				else
					this.NextTurn();
			}
			else
				this.App.Game.ShowCombatDialog(true, (GameState)null);
		}

		private float scoreFleet(FleetInfo fleet)
		{
			float num1 = 0.0f;
			float num2 = 0.0f;
			int num3 = 0;
			int num4 = 0;
			foreach (ShipInfo shipInfo in this._db.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>())
			{
				List<SectionInstanceInfo> list = this._db.GetShipSectionInstances(shipInfo.ID).ToList<SectionInstanceInfo>();
				if (list.Count > 0)
				{
					foreach (SectionInstanceInfo sectionInstanceInfo in list)
						num1 += (float)sectionInstanceInfo.Structure;
				}
				else
					num1 += shipInfo.DesignInfo.Structure;
				num2 += shipInfo.DesignInfo.Structure;
				num3 += CombatAI.GetShipStrength(shipInfo.DesignInfo.Class);
				foreach (DesignSectionInfo designSection in shipInfo.DesignInfo.DesignSections)
				{
					foreach (LogicalBank bank in designSection.ShipSectionAsset.Banks)
					{
						switch (bank.TurretClass)
						{
							case WeaponEnums.TurretClasses.Standard:
								num4 += 3;
								break;
							case WeaponEnums.TurretClasses.Missile:
								++num4;
								break;
							case WeaponEnums.TurretClasses.IOBM:
							case WeaponEnums.TurretClasses.PolarisMissile:
								num4 += 2;
								break;
							case WeaponEnums.TurretClasses.FreeBeam:
							case WeaponEnums.TurretClasses.Spinal:
							case WeaponEnums.TurretClasses.Strafe:
								num4 += 4;
								break;
							case WeaponEnums.TurretClasses.HeavyBeam:
								num4 += 5;
								break;
							case WeaponEnums.TurretClasses.Torpedo:
								num4 += 5;
								break;
							case WeaponEnums.TurretClasses.Drone:
								num4 += 3;
								break;
							case WeaponEnums.TurretClasses.DestroyerRider:
								num4 += 6;
								break;
							case WeaponEnums.TurretClasses.CruiserRider:
								num4 += 9;
								break;
							case WeaponEnums.TurretClasses.DreadnoughtRider:
								num4 += 12;
								break;
						}
					}
				}
			}
			FleetTemplate templateForFleet = StrategicAI.GetTemplateForFleet(this, fleet.PlayerID, fleet.ID);
			float num5 = 10f * Math.Min(num1 / num2, 1f);
			float num6 = 20f * Math.Min((float)num3 / 50f, 1f);
			float num7 = 0.0f;
			if (templateForFleet != null)
			{
				if (templateForFleet.Name == "DEFAULT_PATROL")
					num7 = 1f;
				else if (templateForFleet.Name == "DEFAULT_COMBAT")
					num7 = 0.75f;
				else if (templateForFleet.Name == "DEFAULT_INVASION" || templateForFleet.Name == "SMALL_COMBAT")
					num7 = 0.5f;
				else if (templateForFleet.Name == "DEFAULT_SURVEY")
					num7 = 0.25f;
			}
			return ((float)num4 + num5 + num6) * num7;
		}

		private FleetInfo GetBestCombatFleet(List<FleetInfo> fleets)
		{
			FleetInfo fleetInfo = (FleetInfo)null;
			float num1 = 0.0f;
			foreach (FleetInfo fleet in fleets)
			{
				if (fleet.IsNormalFleet)
				{
					float num2 = this.scoreFleet(fleet);
					if ((double)num2 > (double)num1)
					{
						fleetInfo = fleet;
						num1 = num2;
					}
				}
			}
			return fleetInfo;
		}

		public void SubtractExtendedCombatEndurance(FleetInfo fleet)
		{
			if (this.IsFleetInSupplyRange(fleet.ID))
				return;
			fleet.SupplyRemaining -= Kerberos.Sots.StarFleet.StarFleet.GetSupplyConsumption(this, fleet.ID) * 0.5f;
			this._db.UpdateFleetInfo(fleet);
		}

		public void Phase5_Results()
		{
			List<MissionInfo> list1 = this._db.GetMissionInfos().ToList<MissionInfo>();
			foreach (MissionInfo mission in list1.Where<MissionInfo>((Func<MissionInfo, bool>)(x =>
		   {
			   if (x.Type == MissionType.GATE)
				   return x.Duration == 0;
			   return false;
		   })).ToList<MissionInfo>())
				this.ProcessMission(mission);
			foreach (MissionInfo missionInfo in list1.Where<MissionInfo>((Func<MissionInfo, bool>)(x => x.Type == MissionType.SURVEY)).ToList<MissionInfo>())
			{
				WaypointInfo waypointForMission = this._db.GetNextWaypointForMission(missionInfo.ID);
				if (waypointForMission != null && waypointForMission.Type == WaypointType.DoMission)
				{
					FleetInfo fleetInfo = this._db.GetFleetInfo(missionInfo.FleetID);
					if (fleetInfo != null && this.isHostilesAtSystem(fleetInfo.PlayerID, fleetInfo.SystemID))
						Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleetInfo, true);
				}
			}
			List<FleetInfo> fleetInfoList = new List<FleetInfo>();
			foreach (FleetInfo fleetInfo in this._db.GetFleetInfos(FleetType.FL_NORMAL))
			{
				List<ShipInfo> list2 = this._db.GetShipInfoByFleetID(fleetInfo.ID, true).ToList<ShipInfo>();
				if (list2.Count == 0)
				{
					fleetInfoList.Add(fleetInfo);
				}
				else
				{
					int num = 0;
					foreach (ShipInfo shipInfo in list2)
					{
						if (shipInfo.ParentID == 0 && shipInfo.DesignInfo.DesignSections[0].ShipSectionAsset.Class == ShipClass.BattleRider)
							++num;
					}
					if (num == list2.Count)
					{
						foreach (ShipInfo shipInfo in list2)
							this._db.RemoveShip(shipInfo.ID);
						fleetInfoList.Add(fleetInfo);
					}
				}
			}
			foreach (FleetInfo fleet in fleetInfoList)
			{
				this.FleetLeftSystem(fleet, fleet.SystemID, GameSession.FleetLeaveReason.KILLED);
				this._db.RemoveFleet(fleet.ID);
				GameTrigger.PushEvent(EventType.EVNT_FLEETDIED, (object)fleet.Name, this);
				this._db.RemoveAdmiral(fleet.AdmiralID);
			}
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			foreach (PlayerInfo playerInfo in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
				dictionary.Add(playerInfo.ID, this._db.GetPlayerColoniesByPlayerId(playerInfo.ID).Count<ColonyInfo>());
			List<PlayerInfo> list3 = this._db.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo playerInfo1 in list3)
			{
				foreach (PlayerInfo playerInfo2 in list3)
				{
					if (playerInfo2.ID != playerInfo1.ID && this._db.GetDiplomacyInfo(playerInfo1.ID, playerInfo2.ID).isEncountered)
					{
						int num1 = dictionary[playerInfo1.ID];
						int num2 = dictionary[playerInfo2.ID];
						if (num2 > num1)
							this._db.ApplyDiplomacyReaction(playerInfo1.ID, playerInfo2.ID, StratModifiers.DiplomacyReactionBiggerEmpire, 1);
						else if (num2 < num1)
							this._db.ApplyDiplomacyReaction(playerInfo1.ID, playerInfo2.ID, StratModifiers.DiplomacyReactionSmallerEmpire, 1);
					}
				}
			}
			foreach (PlayerInfo playerInfo1 in list3)
			{
				Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(playerInfo1.ID);
				IEnumerable<PlayerTechInfo> playerTechInfos = this._db.GetPlayerTechInfos(playerObject.ID);
				int num1 = 0;
				foreach (PlayerTechInfo playerTechInfo in playerTechInfos)
					num1 += playerTechInfo.Progress;
				int num2 = playerObject._techPointsAtStartOfTurn / 5000;
				if (num1 / 5000 - num2 > 0)
				{
					foreach (PlayerInfo playerInfo2 in list3)
					{
						int id1 = playerObject.ID;
						int id2 = playerInfo2.ID;
					}
				}
			}
			foreach (Kerberos.Sots.Strategy.CombatData combatData in this.CombatData.GetCombats(this._db.GetTurnCount()).ToList<Kerberos.Sots.Strategy.CombatData>())
			{
				foreach (int standardPlayerId in this.App.GameDatabase.GetStandardPlayerIDs())
				{
					PlayerCombatData player = combatData.GetPlayer(standardPlayerId);
					if (player != null)
					{
						if (player.VictoryStatus == GameSession.VictoryStatus.Win)
							this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
							{
								EventType = TurnEventType.EV_COMBAT_WIN,
								EventMessage = TurnEventMessage.EM_COMBAT_WIN,
								TurnNumber = this._db.GetTurnCount(),
								PlayerID = standardPlayerId,
								SystemID = combatData.SystemID,
								CombatID = combatData.CombatID,
								ShowsDialog = true
							});
						else if (player.VictoryStatus == GameSession.VictoryStatus.Loss)
							this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
							{
								EventType = TurnEventType.EV_COMBAT_LOSS,
								EventMessage = TurnEventMessage.EM_COMBAT_LOSS,
								TurnNumber = this._db.GetTurnCount(),
								PlayerID = standardPlayerId,
								SystemID = combatData.SystemID,
								CombatID = combatData.CombatID,
								ShowsDialog = true
							});
						else if (player.VictoryStatus == GameSession.VictoryStatus.Draw)
							this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
							{
								EventType = TurnEventType.EV_COMBAT_DRAW,
								EventMessage = TurnEventMessage.EM_COMBAT_DRAW,
								TurnNumber = this._db.GetTurnCount(),
								PlayerID = standardPlayerId,
								SystemID = combatData.SystemID,
								CombatID = combatData.CombatID,
								ShowsDialog = true
							});
					}
				}
			}
		}

		public static void AbandonColony(App game, int colonyID)
		{
			ColonyInfo colonyInfo = game.GameDatabase.GetColonyInfo(colonyID);
			OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID);
			game.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_COLONY_ABANDONED,
				EventMessage = TurnEventMessage.EM_COLONY_ABANDONED,
				PlayerID = game.LocalPlayer.ID,
				SystemID = orbitalObjectInfo.StarSystemID,
				OrbitalID = orbitalObjectInfo.ID,
				ColonyID = colonyInfo.ID,
				TurnNumber = game.GameDatabase.GetTurnCount(),
				ShowsDialog = false
			});
			game.GameDatabase.RemoveColonyOnPlanet(colonyInfo.OrbitalObjectID);
			if (colonyInfo == null)
				return;
			GameSession.ApplyMoralEvent(game, MoralEvent.ME_ABANDON_COLONY, colonyInfo.PlayerID, new int?(colonyInfo.ID), new int?(), new int?(colonyInfo.CachedStarSystemID));
		}

		public GameSession.VictoryStatus GetPlayerVictoryStatus(int playerId, int systemId)
		{
			List<Kerberos.Sots.PlayerFramework.Player> list = GameSession.GetPlayersWithCombatAssets(this.App, systemId).ToList<Kerberos.Sots.PlayerFramework.Player>();
			bool flag1 = list.Any<Kerberos.Sots.PlayerFramework.Player>((Func<Kerberos.Sots.PlayerFramework.Player, bool>)(x => this.GameDatabase.GetDiplomacyStateBetweenPlayers(x.ID, playerId) == DiplomacyState.WAR));
			bool flag2 = list.Any<Kerberos.Sots.PlayerFramework.Player>((Func<Kerberos.Sots.PlayerFramework.Player, bool>)(x =>
		   {
			   if (x.ID != playerId)
				   return this.GameDatabase.GetDiplomacyStateBetweenPlayers(x.ID, playerId) == DiplomacyState.ALLIED;
			   return true;
		   }));
			if (flag1 && flag2)
				return GameSession.VictoryStatus.Draw;
			return flag2 ? GameSession.VictoryStatus.Win : GameSession.VictoryStatus.Loss;
		}

		public static IEnumerable<Kerberos.Sots.PlayerFramework.Player> GetPlayersWithCombatAssets(
		  App Game,
		  int systemId)
		{
			List<Kerberos.Sots.PlayerFramework.Player> playerList = new List<Kerberos.Sots.PlayerFramework.Player>();
			foreach (StationInfo stationInfo in Game.GameDatabase.GetStationForSystem(systemId).ToList<StationInfo>())
			{
				if (!playerList.Contains(Game.GetPlayer(stationInfo.PlayerID)))
					playerList.Add(Game.GetPlayer(stationInfo.PlayerID));
			}
			foreach (ColonyInfo colonyInfo in Game.GameDatabase.GetColonyInfosForSystem(systemId).ToList<ColonyInfo>())
			{
				if (!playerList.Contains(Game.GetPlayer(colonyInfo.PlayerID)))
					playerList.Add(Game.GetPlayer(colonyInfo.PlayerID));
			}
			foreach (FleetInfo fleetInfo in Game.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_NORMAL | FleetType.FL_GATE | FleetType.FL_ACCELERATOR).ToList<FleetInfo>())
			{
				if (!playerList.Contains(Game.GetPlayer(fleetInfo.PlayerID)))
					playerList.Add(Game.GetPlayer(fleetInfo.PlayerID));
			}
			return (IEnumerable<Kerberos.Sots.PlayerFramework.Player>)playerList;
		}

		public static bool PlayerPresentInSystem(GameDatabase db, int playerID, int systemID)
		{
			bool flag1 = db.GetPlayerColonySystemIDs(playerID).Contains<int>(systemID);
			bool flag2 = db.GetFleetsByPlayerAndSystem(playerID, systemID, FleetType.FL_NORMAL | FleetType.FL_RESERVE).Count<FleetInfo>() > 0;
			if (!flag1)
				return flag2;
			return true;
		}

		public Dictionary<EasterEgg, int> GetAvailableGMOdds()
		{
			Dictionary<EasterEgg, int> dictionary = new Dictionary<EasterEgg, int>();
			foreach (KeyValuePair<EasterEgg, int> gmOdd in this._app.AssetDatabase.GMOdds)
			{
				if (GameSession.IsEasterEggAvailable(this.GameDatabase, gmOdd.Key) && (gmOdd.Key == EasterEgg.GM_COMET || this._db.GetNumEncountersOfType(gmOdd.Key) == 0))
					dictionary.Add(gmOdd.Key, gmOdd.Value);
			}
			return dictionary;
		}

		public Dictionary<EasterEgg, int> GetAvailableEEOdds()
		{
			Dictionary<EasterEgg, int> dictionary = new Dictionary<EasterEgg, int>();
			foreach (KeyValuePair<EasterEgg, int> easterEggOdd in this._app.AssetDatabase.EasterEggOdds)
			{
				if (GameSession.IsEasterEggAvailable(this.GameDatabase, easterEggOdd.Key))
					dictionary.Add(easterEggOdd.Key, easterEggOdd.Value);
			}
			return dictionary;
		}

		public static bool IsEasterEggAvailable(GameDatabase db, EasterEgg ee)
		{
			if (ee == EasterEgg.GM_NEUTRON_STAR || ee == EasterEgg.GM_GARDENER || ee == EasterEgg.EE_PIRATE_BASE)
				return db.HasEndOfFleshExpansion();
			return true;
		}

		public Dictionary<RandomEncounter, int> GetAvailableRandomOdds(
		  PlayerInfo player)
		{
			Dictionary<RandomEncounter, int> dictionary = new Dictionary<RandomEncounter, int>();
			foreach (KeyValuePair<RandomEncounter, int> randomEncounterOdd in this._app.AssetDatabase.RandomEncounterOdds)
			{
				if (GameSession.IsRandomAvailable(this._db, player, randomEncounterOdd.Key))
					dictionary.Add(randomEncounterOdd.Key, randomEncounterOdd.Value);
			}
			return dictionary;
		}

		public static bool IsRandomAvailable(GameDatabase db, PlayerInfo player, RandomEncounter rand)
		{
			if (rand == RandomEncounter.RE_SPECTORS)
				return db.GetFactionInfo(player.FactionID).Name != "loa";
			return true;
		}

		public static bool CanPlayerSupportDefenseAssets(GameDatabase db, int playerID, int systemID)
		{
			bool flag1 = db.GetPlayerColonySystemIDs(playerID).Contains<int>(systemID);
			bool flag2 = db.GetStationForSystemPlayerAndType(systemID, playerID, StationType.NAVAL) != null;
			if (!flag1)
				return flag2;
			return true;
		}

		public void CheckGMEncounters()
		{
			string nameValue = this.GameDatabase.GetNameValue("GMCount");
			if (string.IsNullOrEmpty(nameValue))
			{
				this.GameDatabase.InsertNameValuePair("GMCount", "0");
				nameValue = this.GameDatabase.GetNameValue("GMCount");
			}
			int num1 = int.Parse(nameValue);
			Random safeRandom = App.GetSafeRandom();
			if (num1 < this._db.GetNameValue<int>("GSGrandMenaceCount") && this.GameDatabase.GetTurnCount() > this.AssetDatabase.GrandMenaceMinTurn && safeRandom.CoinToss(this.AssetDatabase.GrandMenaceChance))
			{
				Dictionary<EasterEgg, int> availableGmOdds = this.GetAvailableGMOdds();
				int maxValue = availableGmOdds.Sum<KeyValuePair<EasterEgg, int>>((Func<KeyValuePair<EasterEgg, int>, int>)(x => x.Value));
				if (maxValue == 0)
					return;
				int num2 = this._random.Next(maxValue);
				int num3 = 0;
				EasterEgg easterEgg = EasterEgg.EE_SWARM;
				foreach (KeyValuePair<EasterEgg, int> keyValuePair in availableGmOdds)
				{
					num3 += keyValuePair.Value;
					if (num3 > num2)
					{
						easterEgg = keyValuePair.Key;
						break;
					}
				}
				switch (easterEgg)
				{
					case EasterEgg.GM_SYSTEM_KILLER:
						if (this.ScriptModules.SystemKiller != null)
						{
							this.ScriptModules.SystemKiller.AddInstance(this.GameDatabase, this.AssetDatabase, new int?());
							break;
						}
						break;
					case EasterEgg.GM_LOCUST_SWARM:
						if (this.ScriptModules.Locust != null)
						{
							this.ScriptModules.Locust.AddInstance(this.GameDatabase, this.AssetDatabase, new int?());
							break;
						}
						break;
					case EasterEgg.GM_COMET:
						if (this.ScriptModules.Comet != null)
						{
							this.ScriptModules.Comet.AddInstance(this.GameDatabase, this.AssetDatabase, new int?());
							break;
						}
						break;
					case EasterEgg.GM_NEUTRON_STAR:
						if (this.ScriptModules.NeutronStar != null)
						{
							this.ScriptModules.NeutronStar.AddInstance(this.GameDatabase, this.AssetDatabase, new int?());
							break;
						}
						break;
					case EasterEgg.GM_GARDENER:
						if (this.ScriptModules.Gardeners != null)
						{
							this.ScriptModules.Gardeners.AddInstance(this.GameDatabase, this.AssetDatabase, 0);
							break;
						}
						break;
				}
				if (easterEgg != EasterEgg.GM_COMET)
				{
					int num4;
					this.GameDatabase.UpdateNameValuePair("GMCount", (num4 = num1 + 1).ToString());
				}
			}
			foreach (IncomingGMInfo incomingGmInfo in this.GameDatabase.GetIncomingGMsThisTurn().ToList<IncomingGMInfo>())
			{
				switch (incomingGmInfo.type)
				{
					case EasterEgg.GM_SYSTEM_KILLER:
						if (this.ScriptModules.SystemKiller != null)
						{
							this.ScriptModules.SystemKiller.ExecuteInstance(this.GameDatabase, this.AssetDatabase, incomingGmInfo.SystemId);
							break;
						}
						break;
					case EasterEgg.GM_LOCUST_SWARM:
						if (this.ScriptModules.Locust != null)
						{
							this.ScriptModules.Locust.ExecuteInstance(this.GameDatabase, this.AssetDatabase, incomingGmInfo.SystemId);
							break;
						}
						break;
					case EasterEgg.GM_COMET:
						if (this.ScriptModules.Comet != null)
						{
							this.ScriptModules.Comet.ExecuteInstance(this.GameDatabase, this.AssetDatabase, incomingGmInfo.SystemId);
							break;
						}
						break;
					case EasterEgg.GM_NEUTRON_STAR:
						if (this.ScriptModules.NeutronStar != null)
						{
							this.ScriptModules.NeutronStar.ExecuteInstance(this.GameDatabase, this.AssetDatabase, incomingGmInfo.SystemId);
							break;
						}
						break;
					case EasterEgg.GM_SUPER_NOVA:
						if (this.ScriptModules.SuperNova != null)
						{
							this.ScriptModules.SuperNova.ExecuteInstance(this, this.GameDatabase, this.AssetDatabase, incomingGmInfo.SystemId);
							break;
						}
						break;
				}
				this.GameDatabase.RemoveIncomingGM(incomingGmInfo.ID);
			}
		}

		public void CheckRandomEncounters()
		{
			if (this.ScriptModules.Spectre != null)
				this.ScriptModules.Spectre.UpdateTurn(this);
			if (this.ScriptModules.MeteorShower != null)
				this.ScriptModules.MeteorShower.UpdateTurn(this);
			if (this.ScriptModules.Slaver != null)
				this.ScriptModules.Slaver.UpdateTurn(this);
			if (this.ScriptModules.Pirates != null)
				this.ScriptModules.Pirates.UpdateTurn(this);
			if (this.ScriptModules.GhostShip != null)
				this.ScriptModules.GhostShip.UpdateTurn(this);
			if (this.ScriptModules.Comet != null)
				this.ScriptModules.Comet.UpdateTurn(this);
			if (Spectre.ForceEncounter && this.ScriptModules.Spectre != null)
				this.ScriptModules.Spectre.AddEncounter(this, this.GameDatabase.GetPlayerInfo(this.LocalPlayer.ID), new int?());
			if (Slaver.ForceEncounter && this.ScriptModules.Slaver != null)
				this.ScriptModules.Slaver.AddEncounter(this, this.GameDatabase.GetPlayerInfo(this.LocalPlayer.ID));
			if (GhostShip.ForceEncounter && this.ScriptModules.GhostShip != null)
				this.ScriptModules.GhostShip.AddEncounter(this, this.GameDatabase.GetPlayerInfo(this.LocalPlayer.ID));
			if (MeteorShower.ForceEncounter && this.ScriptModules.MeteorShower != null)
				this.ScriptModules.MeteorShower.AddEncounter(this, this.GameDatabase.GetPlayerInfo(this.LocalPlayer.ID), new int?());
			if (this.App.GameDatabase.GetTurnCount() > this.App.AssetDatabase.RandomEncMinTurns)
			{
				foreach (PlayerInfo playerInfo in this.App.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>())
				{
					Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(playerInfo.ID);
					if ((playerObject == null || !playerObject.IsAI()) && playerInfo.LastEncounterTurn <= this.App.GameDatabase.GetTurnCount() - this.App.AssetDatabase.RandomEncTurnsToExclude)
					{
						float num1 = Math.Max(Math.Min(this._db.GetStratModifier<float>(StratModifiers.OddsOfRandomEncounter, playerInfo.ID) * (this._db.GetNameValue<float>("RandomEncounterFrequency") / 100f), this.App.AssetDatabase.RandomEncMaxOdds), this.App.AssetDatabase.RandomEncMinOdds);
						if (this._random.CoinToss((double)num1))
						{
							this._app.GameDatabase.UpdateLastEncounterTurn(playerInfo.ID, this._app.GameDatabase.GetTurnCount());
							this._app.GameDatabase.SetStratModifier(StratModifiers.OddsOfRandomEncounter, playerInfo.ID, (object)(float)((double)num1 - (double)this._app.AssetDatabase.RandomEncDecOddsCombat * ((double)this._db.GetNameValue<float>("RandomEncounterFrequency") / 100.0)));
							Dictionary<RandomEncounter, int> availableRandomOdds = this.GetAvailableRandomOdds(playerInfo);
							int maxValue = availableRandomOdds.Sum<KeyValuePair<RandomEncounter, int>>((Func<KeyValuePair<RandomEncounter, int>, int>)(x => x.Value));
							if (maxValue == 0)
								return;
							int num2 = this._random.Next(maxValue);
							int num3 = 0;
							RandomEncounter randomEncounter = RandomEncounter.RE_ASTEROID_SHOWER;
							foreach (KeyValuePair<RandomEncounter, int> keyValuePair in availableRandomOdds)
							{
								num3 += keyValuePair.Value;
								if (num3 > num2)
								{
									randomEncounter = keyValuePair.Key;
									break;
								}
							}
							switch (randomEncounter)
							{
								case RandomEncounter.RE_ASTEROID_SHOWER:
									if (this.ScriptModules.MeteorShower != null)
									{
										this.ScriptModules.MeteorShower.AddEncounter(this, playerInfo, new int?());
										continue;
									}
									continue;
								case RandomEncounter.RE_SPECTORS:
									this.ScriptModules.Spectre.AddEncounter(this, playerInfo, new int?());
									continue;
								case RandomEncounter.RE_SLAVERS:
									if (this.ScriptModules.Slaver != null)
									{
										this.ScriptModules.Slaver.AddEncounter(this, playerInfo);
										continue;
									}
									continue;
								case RandomEncounter.RE_GHOST_SHIP:
									if (this.GameDatabase.GetFactionName(playerInfo.FactionID) != "zuul")
									{
										this.ScriptModules.GhostShip.AddEncounter(this, playerInfo);
										continue;
									}
									continue;
								default:
									continue;
							}
						}
					}
				}
			}
			else
			{
				foreach (PlayerInfo standardPlayerInfo in this.App.GameDatabase.GetStandardPlayerInfos())
					this._app.GameDatabase.SetStratModifier(StratModifiers.OddsOfRandomEncounter, standardPlayerInfo.ID, (object)(float)((double)this._app.AssetDatabase.RandomEncBaseOdds * ((double)this._db.GetNameValue<float>("RandomEncounterFrequency") / 100.0)));
			}
			foreach (PlayerInfo targetPlayer in this.App.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				foreach (IncomingRandomInfo incomingRandomInfo in this.GameDatabase.GetIncomingRandomsForPlayerThisTurn(targetPlayer.ID).ToList<IncomingRandomInfo>())
				{
					switch (incomingRandomInfo.type)
					{
						case RandomEncounter.RE_ASTEROID_SHOWER:
							if (this.ScriptModules.MeteorShower != null)
							{
								this.ScriptModules.MeteorShower.ExecuteEncounter(this, incomingRandomInfo.SystemId, 1f, false);
								break;
							}
							break;
						case RandomEncounter.RE_SPECTORS:
							this.ScriptModules.Spectre.ExecuteEncounter(this, targetPlayer, incomingRandomInfo.SystemId, false);
							break;
						case RandomEncounter.RE_SLAVERS:
							if (this.ScriptModules.Slaver != null)
							{
								this.ScriptModules.Slaver.ExecuteEncounter(this, targetPlayer, incomingRandomInfo.SystemId);
								break;
							}
							break;
						case RandomEncounter.RE_GHOST_SHIP:
							if (this.GameDatabase.GetFactionName(targetPlayer.FactionID) != "zuul")
							{
								this.ScriptModules.GhostShip.ExecuteEncounter(this, targetPlayer, incomingRandomInfo.SystemId);
								break;
							}
							break;
					}
					this.GameDatabase.RemoveIncomingRandom(incomingRandomInfo.ID);
				}
			}
		}

		private void CheckSpecialCaseEncounters()
		{
			if (!this.App.GameDatabase.HasEndOfFleshExpansion())
				return;
			if (this.ScriptModules.NeutronStar != null)
				NeutronStar.GenerateMeteorAndCometEncounters(this.App);
			if (this.ScriptModules.SuperNova == null)
				return;
			SuperNova.AddSuperNovas(this.App.Game, this.App.GameDatabase, this.App.AssetDatabase);
		}

		private void AddAsteroidShower(PlayerInfo pi)
		{
			this.App.GameDatabase.GetPlayerColonySystemIDs(pi.ID).ToList<int>();
		}

		private void AddFlyingDutchman(PlayerInfo pi)
		{
			this.App.GameDatabase.GetPlayerColonySystemIDs(pi.ID).ToList<int>();
		}

		private void AddPirates(PlayerInfo pi)
		{
			this.App.GameDatabase.GetPlayerColonySystemIDs(pi.ID).ToList<int>();
		}

		private void AddRefugees(PlayerInfo pi)
		{
			this.App.GameDatabase.GetPlayerColonySystemIDs(pi.ID).ToList<int>();
		}

		private void AddSlavers(PlayerInfo pi)
		{
			this.App.GameDatabase.GetPlayerColonySystemIDs(pi.ID).ToList<int>();
		}

		private void AddSpectors(PlayerInfo pi)
		{
			this.App.GameDatabase.GetPlayerColonySystemIDs(pi.ID).ToList<int>();
		}

		private void AddVonNeumann(PlayerInfo pi)
		{
			this.App.GameDatabase.GetPlayerColonySystemIDs(pi.ID).ToList<int>();
		}

		public void CheckTriggers(TurnStage ts)
		{
			foreach (Trigger t in new List<Trigger>((IEnumerable<Trigger>)this.ActiveTriggers.OrderBy<Trigger, int>((Func<Trigger, int>)(x => GameTrigger.GetTriggerTriggeredDepth(this.ActiveTriggers, x)))))
			{
				if (GameTrigger.Evaluate(t.Context, this, t))
				{
					IEnumerable<TriggerCondition> source = t.Conditions.Where<TriggerCondition>((Func<TriggerCondition, bool>)(x => x.isEventDriven));
					IEnumerable<TriggerCondition> triggerConditions = t.Conditions.Where<TriggerCondition>((Func<TriggerCondition, bool>)(x => !x.isEventDriven));
					bool flag = true;
					foreach (TriggerCondition tc in triggerConditions)
					{
						if (!GameTrigger.Evaluate(tc, this, t))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						int val2 = source.Count<TriggerCondition>() == 0 ? 1 : int.MaxValue;
						foreach (TriggerCondition tc in source)
						{
							int val1 = 0;
							List<EventStub> list = this.TriggerEvents.ToList<EventStub>();
							while (GameTrigger.Evaluate(tc, this, t))
								++val1;
							val2 = Math.Min(val1, val2);
							this.TriggerEvents = list;
						}
						if (val2 != int.MaxValue)
						{
							for (int index = 0; index < val2; ++index)
							{
								foreach (TriggerAction action in t.Actions)
									GameTrigger.Evaluate(action, this, t);
							}
							if (!t.IsRecurring)
								this.ActiveTriggers.Remove(t);
							GameTrigger.PushEvent(EventType.EVNT_TRIGGERTRIGGERED, (object)t.Name, this);
						}
					}
				}
			}
			GameTrigger.ClearEvents(this);
		}

		public bool LaunchNextCombat()
		{
			if (this.m_Combats.Count == 0 && this._currentCombat == null)
				return false;
			if (this._currentCombat == null)
			{
				this._currentCombat = this.m_Combats.First<PendingCombat>((Func<PendingCombat, bool>)(x => x.CombatResults == null));
				foreach (int fleetID in this._currentCombat.SelectedPlayerFleets.Values)
				{
					FleetInfo fi = this._db.GetFleetInfo(fleetID);
					if (fi != null)
					{
						if (this.fleetsInCombat.Any<FleetInfo>((Func<FleetInfo, bool>)(x => x.ID == fi.ID)))
							this.SubtractExtendedCombatEndurance(fi);
						else
							this.fleetsInCombat.Add(fi);
					}
				}
				if (this._currentCombat != null)
				{
					foreach (int num in this._currentCombat.PlayersInCombat)
					{
						int index = num - 1;
						if (this.App.GameSetup.Players.Count > index)
							this.App.GameSetup.Players[index].Status = NPlayerStatus.PS_COMBAT;
					}
					bool sim = true;
					if (this._currentCombat.CombatResolutionSelections.ContainsKey(this.App.LocalPlayer.ID) && this._currentCombat.CombatResolutionSelections[this.App.LocalPlayer.ID] == ResolutionType.FIGHT)
						sim = false;
					this.LaunchCombat(this._currentCombat, false, sim, true);
					GameSession.Trace("Launching combat in " + this._db.GetStarSystemInfo(this._currentCombat.SystemID).Name + " system.");
				}
			}
			return true;
		}

		public void LaunchCombat(PendingCombat pendingCombat, bool testing, bool sim, bool authority)
		{
			this._currentCombat = pendingCombat;
			this._app.LaunchCombat(this._app.Game, pendingCombat, testing, sim, authority);
		}

		public static float GetSystemToSystemDistance(GameDatabase _db, int system1, int system2)
		{
			return (_db.GetStarSystemInfo(system1).Origin - _db.GetStarSystemInfo(system2).Origin).Length;
		}

		private bool UpdateLinearMove(
		  MoveOrderInfo moveOrder,
		  FleetInfo fi,
		  bool usePermNodeLines,
		  bool useTempNodeLines,
		  bool useFarcast,
		  ref float remainingNodeDistance)
		{
			float num1 = usePermNodeLines | useTempNodeLines ? remainingNodeDistance : Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this, moveOrder.FleetID, false);
			float val2;
			Vector3 vector3_1;
			if (moveOrder.FromSystemID != 0 && moveOrder.ToSystemID != 0)
			{
				val2 = !this._app.AssetDatabase.GetFaction(this._db.GetPlayerFactionID(fi.PlayerID)).CanUseGravityWell() || this._db.FleetHasCurvatureComp(fi) ? GameSession.GetSystemToSystemDistance(this.App.Game.GameDatabase, moveOrder.FromSystemID, moveOrder.ToSystemID) : GameSession.GetGravityWellTravelDistance(this._db, moveOrder);
			}
			else
			{
				Vector3 vector3_2 = moveOrder.FromSystemID == 0 ? moveOrder.FromCoords : this._db.GetStarSystemInfo(moveOrder.FromSystemID).Origin;
				vector3_1 = (moveOrder.ToSystemID == 0 ? moveOrder.ToCoords : this._db.GetStarSystemInfo(moveOrder.ToSystemID).Origin) - vector3_2;
				val2 = vector3_1.Length;
			}
			if ((double)val2 > 0.0)
			{
				float num2 = num1 / val2;
				moveOrder.Progress += num2;
				int systemId = fi.SystemID;
				if (fi.SystemID != 0)
				{
					this._db.UpdateFleetLocation(fi.ID, 0, new int?(fi.SystemID));
					if (useFarcast)
					{
						float hiverCastingDistance = this._db.GetHiverCastingDistance(fi.PlayerID);
						if ((double)hiverCastingDistance > 0.0)
						{
							Vector3 starSystemOrigin = this._db.GetStarSystemOrigin(moveOrder.FromSystemID);
							vector3_1 = this._db.GetStarSystemOrigin(moveOrder.ToSystemID) - starSystemOrigin;
							float length = vector3_1.Length;
							if ((double)length <= (double)hiverCastingDistance)
							{
								moveOrder.Progress = 1.1f;
								this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
								{
									EventType = TurnEventType.EV_FARCAST_SUCCESS,
									EventMessage = TurnEventMessage.EM_FARCAST_SUCCESS,
									SystemID = this._db.GetMissionByFleetID(fi.ID).TargetSystemID,
									SystemID2 = fi.PreviousSystemID.Value,
									PlayerID = fi.PlayerID,
									FleetID = fi.ID,
									TurnNumber = this.App.GameDatabase.GetTurnCount(),
									ShowsDialog = false
								});
							}
							else
							{
								moveOrder.Progress = hiverCastingDistance / length;
								this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
								{
									EventType = TurnEventType.EV_FARCAST_FAILED,
									EventMessage = TurnEventMessage.EM_FARCAST_FAILED,
									PlayerID = fi.PlayerID,
									SystemID = this._db.GetMissionByFleetID(fi.ID).TargetSystemID,
									SystemID2 = fi.PreviousSystemID.Value,
									FleetID = fi.ID,
									Savings = (double)length - (double)hiverCastingDistance,
									TurnNumber = this.App.GameDatabase.GetTurnCount(),
									ShowsDialog = false
								});
							}
						}
					}
					this.FleetLeftSystem(fi, systemId, GameSession.FleetLeaveReason.TRAVEL);
				}
				remainingNodeDistance -= Math.Min(remainingNodeDistance, val2);
			}
			else if (moveOrder.FromSystemID == moveOrder.ToSystemID)
			{
				moveOrder.Progress = 1.1f;
				return true;
			}
			if ((double)moveOrder.Progress > 1.0)
				return true;
			if ((double)val2 == 0.0)
			{
				moveOrder.Progress = 1.1f;
				return true;
			}
			this._db.UpdateMoveOrder(moveOrder.ID, moveOrder.Progress);
			return false;
		}

		public void FleetLeftSystem(FleetInfo fleet, int systemID, GameSession.FleetLeaveReason reason = GameSession.FleetLeaveReason.TRAVEL)
		{
			if (systemID == 0 || fleet == null)
				return;
			if (!GameSession.PlayerPresentInSystem(this.GameDatabase, fleet.PlayerID, systemID))
				Kerberos.Sots.GameStates.StarSystem.RemoveSystemPlayerColor(this.GameDatabase, systemID, fleet.PlayerID);
			this.GameDatabase.UpdateFleetPreferred(fleet.ID, false);
		}

		private void FinishMove(MoveOrderInfo moveOrder, FleetInfo fi)
		{
			if (moveOrder.ToSystemID != 0)
				this.ArriveAtSystem(moveOrder, fi);
			this._db.RemoveMoveOrder(moveOrder.ID);
		}

		public void InsertNewMonitorSpecialProject(int playerid, int encounterid, int fleetid)
		{
			Random random = new Random();
			FleetLocation fleetLocation = this.App.GameDatabase.GetFleetLocation(fleetid, false);
			string str = "";
			if (fleetLocation.SystemID != 0)
				str = this.App.GameDatabase.GetStarSystemInfo(fleetLocation.SystemID).Name;
			int cost = random.NextInclusive(this.App.AssetDatabase.SpecialProjectData.MinimumAsteroidMonitorStudy, this.App.AssetDatabase.SpecialProjectData.MaximumAsteroidMonitorStudy);
			int num = this._db.InsertSpecialProject(playerid, App.Localize("@EVENTMSG_SPECPRJ_ASTEROIDMONITOR") + " - " + str, cost, SpecialProjectType.AsteroidMonitor, 0, encounterid, fleetid, 0);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_NEW_SPECIAL_PROJECT,
				EventMessage = TurnEventMessage.EM_NEW_SPECIAL_PROJECT,
				PlayerID = playerid,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				SpecialProjectID = num,
				ShowsDialog = false
			});
		}

		public void InsertNewIndependentPlanetResearchProject(int PlayerID, int TargetPlayerID)
		{
			int cost = new Random().NextInclusive(this.App.AssetDatabase.SpecialProjectData.MinimumIndyInvestigate, this.App.AssetDatabase.SpecialProjectData.MaximumIndyInvestigate);
			PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(TargetPlayerID);
			int num = this._db.InsertSpecialProject(PlayerID, string.Format(App.Localize("@EVENTMSG_SPECPRJ_INVESTIGATEINDEPENDENT"), (object)playerInfo.Name), cost, SpecialProjectType.IndependentStudy, 0, 0, 0, TargetPlayerID);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_NEW_SPECIAL_PROJECT,
				EventMessage = TurnEventMessage.EM_NEW_SPECIAL_PROJECT,
				PlayerID = PlayerID,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				SpecialProjectID = num,
				ShowsDialog = false
			});
		}

		public void InsertNewRadiationShieldResearchProject(int PlayerID)
		{
			if (this._db.GetHasPlayerStudiedSpecialProject(PlayerID, SpecialProjectType.RadiationShielding) || this._db.GetHasPlayerStudyingSpecialProject(PlayerID, SpecialProjectType.RadiationShielding))
				return;
			int cost = new Random().NextInclusive(this.App.AssetDatabase.SpecialProjectData.MinimumRadiationShieldingStudy, this.App.AssetDatabase.SpecialProjectData.MaximumRadiationShieldingStudy);
			int num = this._db.InsertSpecialProject(PlayerID, App.Localize("@EVENTMSG_SPECPRJ_RADIATIONSHIELDING"), cost, SpecialProjectType.RadiationShielding, 0, 0, 0, 0);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_NEW_SPECIAL_PROJECT,
				EventMessage = TurnEventMessage.EM_NEW_SPECIAL_PROJECT,
				PlayerID = PlayerID,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				SpecialProjectID = num,
				ShowsDialog = false
			});
		}

		public void InsertNewNeutronStarResearchProject(int PlayerID, int encounterId)
		{
			if (this._db.GetHasPlayerStudiedSpecialProject(PlayerID, SpecialProjectType.NeutronStar) || this._db.GetHasPlayerStudyingSpecialProject(PlayerID, SpecialProjectType.NeutronStar))
				return;
			int cost = new Random().NextInclusive(this.App.AssetDatabase.SpecialProjectData.MinimumRadiationShieldingStudy, this.App.AssetDatabase.SpecialProjectData.MaximumRadiationShieldingStudy);
			int num = this._db.InsertSpecialProject(PlayerID, App.Localize("@EVENTMSG_SPECPRJ_NEUTRONSTAR"), cost, SpecialProjectType.NeutronStar, 0, encounterId, 0, 0);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_NEW_SPECIAL_PROJECT,
				EventMessage = TurnEventMessage.EM_NEW_SPECIAL_PROJECT,
				PlayerID = PlayerID,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				SpecialProjectID = num,
				ShowsDialog = false
			});
		}

		public void InsertNewGardenerResearchProject(int PlayerID, int encounterId)
		{
			if (this._db.GetHasPlayerStudiedSpecialProject(PlayerID, SpecialProjectType.Gardener) || this._db.GetHasPlayerStudyingSpecialProject(PlayerID, SpecialProjectType.Gardener))
				return;
			int cost = new Random().NextInclusive(this.App.AssetDatabase.SpecialProjectData.MinimumRadiationShieldingStudy, this.App.AssetDatabase.SpecialProjectData.MaximumRadiationShieldingStudy);
			int num = this._db.InsertSpecialProject(PlayerID, App.Localize("@EVENTMSG_SPECPRJ_GARDENER"), cost, SpecialProjectType.Gardener, 0, encounterId, 0, 0);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_NEW_SPECIAL_PROJECT,
				EventMessage = TurnEventMessage.EM_NEW_SPECIAL_PROJECT,
				PlayerID = PlayerID,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				SpecialProjectID = num,
				ShowsDialog = false
			});
		}

		private static int GetNewBoreLineHealth(GameDatabase db, int playerid)
		{
			return (int)(450.0 * (double)GameSession.GetBoreHealthMultiplier(db, playerid) + (double)new Random().NextInclusive(0, 50));
		}

		private static int GetNewUnstableBoreLineHealth(GameDatabase db, int playerid)
		{
			return (int)(20.0 * (double)GameSession.GetBoreHealthMultiplier(db, playerid) + (double)new Random().NextInclusive(0, 30));
		}

		public static int GetPlayerSystemBoreLineLimit(GameDatabase db, int playerid)
		{
			return !db.PlayerHasTech(playerid, "DRV_Radiant_Drive") ? 4 : 5;
		}

		public List<MoveOrderInfo> GetMoveOrdersBetweenSystems(
		  int system1,
		  int system2)
		{
			List<MoveOrderInfo> moveOrderInfoList = new List<MoveOrderInfo>();
			foreach (MoveOrderInfo moveOrderInfo in this._db.GetMoveOrdersByDestinationSystem(system1).ToList<MoveOrderInfo>().Union<MoveOrderInfo>((IEnumerable<MoveOrderInfo>)this._db.GetMoveOrdersByDestinationSystem(system2).ToList<MoveOrderInfo>()))
			{
				if ((moveOrderInfo.FromSystemID == system1 || moveOrderInfo.FromSystemID == system2) && (moveOrderInfo.ToSystemID == system1 || moveOrderInfo.ToSystemID == system2))
					moveOrderInfoList.Add(moveOrderInfo);
			}
			return moveOrderInfoList;
		}

		private void DissolveNodeLine(int lineID)
		{
			NodeLineInfo nodeLine = this._db.GetNodeLine(lineID);
			if (nodeLine == null)
				return;
			StarSystemInfo starSystemInfo1 = this._db.GetStarSystemInfo(nodeLine.System1ID);
			StarSystemInfo starSystemInfo2 = this._db.GetStarSystemInfo(nodeLine.System2ID);
			foreach (PlayerInfo playerInfo in this._db.GetPlayerInfos())
			{
				if (this._app.GameDatabase.GetFactionName(playerInfo.FactionID) == "zuul" && (StarMap.IsInRange(this.GameDatabase, playerInfo.ID, starSystemInfo1, (Dictionary<int, List<ShipInfo>>)null) || StarMap.IsInRange(this.GameDatabase, playerInfo.ID, starSystemInfo2, (Dictionary<int, List<ShipInfo>>)null)))
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_NODE_LINE_COLLAPSE,
						EventMessage = TurnEventMessage.EM_NODE_LINE_COLLAPSE,
						PlayerID = playerInfo.ID,
						SystemID = nodeLine.System1ID,
						SystemID2 = nodeLine.System2ID,
						ShowsDialog = false,
						TurnNumber = this.App.GameDatabase.GetTurnCount()
					});
			}
			foreach (MoveOrderInfo moveOrderInfo in this._db.GetMoveOrdersByDestinationSystem(nodeLine.System2ID).ToList<MoveOrderInfo>().Union<MoveOrderInfo>((IEnumerable<MoveOrderInfo>)this._db.GetMoveOrdersByDestinationSystem(nodeLine.System1ID).ToList<MoveOrderInfo>()).ToList<MoveOrderInfo>())
			{
				if ((moveOrderInfo.FromSystemID == nodeLine.System1ID || moveOrderInfo.FromSystemID == nodeLine.System2ID) && (moveOrderInfo.ToSystemID == nodeLine.System1ID || moveOrderInfo.ToSystemID == nodeLine.System2ID))
				{
					FleetInfo fleetInfo = this._db.GetFleetInfo(moveOrderInfo.FleetID);
					if (this._db.GetFactionName(this._db.GetPlayerFactionID(fleetInfo.PlayerID)) == "zuul")
					{
						this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_NODE_LINE_COLLAPSE_FLEET_LOSS,
							EventMessage = TurnEventMessage.EM_NODE_LINE_COLLAPSE_FLEET_LOSS,
							PlayerID = fleetInfo.PlayerID,
							ShowsDialog = false,
							TurnNumber = this.App.GameDatabase.GetTurnCount()
						});
						foreach (ShipInfo shipInfo in this._db.GetShipInfoByFleetID(fleetInfo.ID, false).ToList<ShipInfo>())
							this._db.RemoveShip(shipInfo.ID);
						this._db.RemoveFleet(fleetInfo.ID);
					}
				}
			}
			this._db.RemoveNodeLine(lineID);
		}

		private void ArriveAtSystem(MoveOrderInfo moveOrder, FleetInfo fleet)
		{
			if (moveOrder.ToSystemID == 0)
				throw new InvalidDataException("moveOrder.ToSystemID cannot be 0 when arriving at a system.");
			fleet.PreviousSystemID = new int?(moveOrder.FromSystemID);
			fleet.SystemID = moveOrder.ToSystemID;
			this._db.UpdateFleetLocation(moveOrder.FleetID, moveOrder.ToSystemID, new int?(moveOrder.FromSystemID));
			GameSession.Trace("Fleet " + (object)moveOrder.FleetID + " has arrived at system " + (object)moveOrder.ToSystemID);
			if (this._app.AssetDatabase.GetFaction(this._app.GameDatabase.GetPlayerInfo(fleet.PlayerID).FactionID).CanUseAccelerators())
			{
				if (this._db.GetFleetsByPlayerAndSystem(fleet.PlayerID, fleet.SystemID, FleetType.FL_ACCELERATOR).Any<FleetInfo>())
					this._db.UpdateFleetAccelerated(this, fleet.ID, new int?());
				else
					this._db.UpdateFleetAccelerated(this, fleet.ID, new int?(-10));
			}
			NodeLineInfo permenantNodeLine = GameSession.GetSystemsNonPermenantNodeLine(this._db, moveOrder.FromSystemID, moveOrder.ToSystemID);
			if (!Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(this.App.GameDatabase, fleet) && GameSession.FleetHasBore(this._app.GameDatabase, fleet.ID))
			{
				if (permenantNodeLine != null)
				{
					this._db.UpdateNodeLineHealth(permenantNodeLine.ID, GameSession.GetNewBoreLineHealth(this._db, fleet.PlayerID));
				}
				else
				{
					StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(moveOrder.FromSystemID);
					if (starSystemInfo != (StarSystemInfo)null && !starSystemInfo.IsDeepSpace)
					{
						NodeLineInfo nodeLineInfo = this._db.GetExploredNodeLinesFromSystem(fleet.PlayerID, fleet.SystemID).Where<NodeLineInfo>((Func<NodeLineInfo, bool>)(x => x.Health > -1)).ToList<NodeLineInfo>().Count < GameSession.GetPlayerSystemBoreLineLimit(this._db, fleet.PlayerID) ? this._db.GetNodeLine(this._db.InsertNodeLine(moveOrder.FromSystemID, moveOrder.ToSystemID, GameSession.GetNewBoreLineHealth(this._db, fleet.PlayerID))) : this._db.GetNodeLine(this._db.InsertNodeLine(moveOrder.FromSystemID, moveOrder.ToSystemID, GameSession.GetNewUnstableBoreLineHealth(this._db, fleet.PlayerID)));
					}
				}
			}
			else if (!Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(this.App.GameDatabase, fleet) && permenantNodeLine != null && this.AssetDatabase.GetFaction(this._db.GetPlayerInfo(fleet.PlayerID).FactionID).CanUseNodeLine(new bool?(false)))
			{
				int num = (int)(((double)this._db.GetFleetCommandPointCost(fleet.ID) + 4.0) / 5.0);
				permenantNodeLine.Health -= num;
				this._db.UpdateNodeLineHealth(permenantNodeLine.ID, permenantNodeLine.Health);
			}
			int turnCount = this._db.GetTurnCount();
			if (this._db.GetLastTurnExploredByPlayer(fleet.PlayerID, moveOrder.ToSystemID) != 0)
				this._db.InsertExploreRecord(moveOrder.ToSystemID, fleet.PlayerID, turnCount, true, true);
			foreach (ShipInfo shipInfo in this._db.GetShipInfoByFleetID(fleet.ID, false))
				this._db.UpdateShipSystemPosition(shipInfo.ID, new Matrix?());
		}

		public List<StarSystemInfo> GetClosestGates(
		  int playerId,
		  StarSystemInfo targetSystem,
		  float MaxDist)
		{
			return this._db.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
		   {
			   if ((double)(x.Origin - targetSystem.Origin).Length <= (double)MaxDist)
				   return this._db.SystemHasGate(x.ID, playerId);
			   return false;
		   })).OrderBy<StarSystemInfo, float>((Func<StarSystemInfo, float>)(y => (y.Origin - targetSystem.Origin).Length)).ToList<StarSystemInfo>();
		}

		public int RequestSuulkaFleet(int playerID, int systemID)
		{
			IEnumerable<SuulkaInfo> playerSuulkas = this.GameDatabase.GetPlayerSuulkas(new int?(0));
			if (playerSuulkas.Count<SuulkaInfo>() <= 0)
				return 0;
			SuulkaInfo suulkaInfo = playerSuulkas.ElementAt<SuulkaInfo>(new Random().NextInclusive(0, playerSuulkas.Count<SuulkaInfo>() - 1));
			this.GameDatabase.UpdateSuulkaPlayer(suulkaInfo.ID, playerID);
			DesignInfo designInfo = this.GameDatabase.GetDesignInfo(this.GameDatabase.GetShipInfo(suulkaInfo.ShipID, false).DesignID);
			int toFleetID = this.GameDatabase.InsertFleet(playerID, suulkaInfo.AdmiralID, systemID, systemID, designInfo.Name, FleetType.FL_NORMAL);
			this.GameDatabase.TransferShip(suulkaInfo.ShipID, toFleetID);
			AdmiralInfo admiralInfo = this.GameDatabase.GetAdmiralInfo(suulkaInfo.AdmiralID);
			admiralInfo.PlayerID = playerID;
			this.GameDatabase.UpdateAdmiralInfo(admiralInfo);
			designInfo.PlayerID = playerID;
			this.GameDatabase.UpdateDesign(designInfo);
			return toFleetID;
		}

		public int InsertSuulkaFleet(int playerID, int suulkaID)
		{
			SuulkaInfo suulka = this.GameDatabase.GetSuulka(suulkaID);
			StarSystemInfo starSystemInfo = this.GameDatabase.GetStarSystemInfo(this.GameDatabase.GetOrbitalObjectInfo((suulka.StationID.HasValue ? this.GameDatabase.GetStationInfo(suulka.StationID.Value) : (StationInfo)null).OrbitalObjectID).StarSystemID);
			this.GameDatabase.UpdateSuulkaPlayer(suulka.ID, playerID);
			DesignInfo designInfo = this.GameDatabase.GetDesignInfo(this.GameDatabase.GetShipInfo(suulka.ShipID, false).DesignID);
			int num = this.GameDatabase.InsertFleet(playerID, suulka.AdmiralID, starSystemInfo.ID, starSystemInfo.ID, designInfo.Name, FleetType.FL_NORMAL);
			this.GameDatabase.TransferShip(suulka.ShipID, num);
			designInfo.PlayerID = playerID;
			GameSession.InsertDefaultSuulkaEquipmentInstances(this.App, designInfo, suulka.ShipID, playerID, num);
			return num;
		}

		private static void InsertDefaultSuulkaEquipmentInstances(
		  App game,
		  DesignInfo design,
		  int shipId,
		  int playerId,
		  int fleetId)
		{
			if (design == null)
				return;
			string name = game.GameDatabase.GetPlayerFaction(playerId).Name;
			List<LogicalWeapon> list1 = game.GameDatabase.GetAvailableWeapons(game.AssetDatabase, playerId).ToList<LogicalWeapon>();
			List<SectionInstanceInfo> list2 = game.GameDatabase.GetShipSectionInstances(shipId).ToList<SectionInstanceInfo>();
			for (int i = 0; i < list2.Count; ++i)
			{
				List<WeaponBankInfo> weaponBankInfoList = new List<WeaponBankInfo>();
				List<DesignModuleInfo> designModuleInfoList = new List<DesignModuleInfo>();
				ShipSectionAsset sectionAsset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == design.DesignSections[i].FilePath));
				weaponBankInfoList.AddRange((IEnumerable<WeaponBankInfo>)DesignLab.ChooseWeapons(game.Game, (IList<LogicalWeapon>)list1, ShipRole.COMMAND, WeaponRole.BRAWLER, sectionAsset, playerId, (AITechStyles)null).ToList<WeaponBankInfo>());
				foreach (LogicalModuleMount module1 in sectionAsset.Modules)
				{
					LogicalModuleMount lm = module1;
					if (!string.IsNullOrEmpty(lm.AssignedModuleName))
					{
						LogicalModule module2 = game.AssetDatabase.Modules.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleName == lm.AssignedModuleName));
						if (module2 != null)
						{
							int num = game.GameDatabase.GetModuleID(module2.ModulePath, playerId);
							if (num == 0)
								num = game.GameDatabase.InsertModule(module2, playerId);
							int? nullable1 = new int?();
							int? nullable2 = new int?();
							LogicalBank logicalBank = ((IEnumerable<LogicalBank>)module2.Banks).FirstOrDefault<LogicalBank>();
							if (logicalBank != null)
							{
								string defaultWeaponName = logicalBank.DefaultWeaponName;
								LogicalWeapon weapon = game.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.WeaponName == defaultWeaponName));
								if (WeaponEnums.IsWeaponBattleRider(logicalBank.TurretClass))
								{
									if (weapon == null)
										weapon = LogicalWeapon.EnumerateWeaponFits(sectionAsset.Faction, sectionAsset.SectionName, game.AssetDatabase.Weapons, logicalBank.TurretSize, logicalBank.TurretClass).FirstOrDefault<LogicalWeapon>();
									ShipRole role = ShipRole.UNDEFINED;
									WeaponRole wpnRole = WeaponRole.UNDEFINED;
									switch (logicalBank.TurretClass)
									{
										case WeaponEnums.TurretClasses.Biomissile:
											role = ShipRole.BIOMISSILE;
											wpnRole = WeaponRole.PLANET_ATTACK;
											break;
										case WeaponEnums.TurretClasses.Drone:
											role = ShipRole.DRONE;
											wpnRole = WeaponRole.BRAWLER;
											break;
										case WeaponEnums.TurretClasses.AssaultShuttle:
											role = ShipRole.ASSAULTSHUTTLE;
											wpnRole = WeaponRole.PLANET_ATTACK;
											break;
										case WeaponEnums.TurretClasses.BoardingPod:
											role = ShipRole.BOARDINGPOD;
											wpnRole = WeaponRole.DISABLING;
											break;
									}
									nullable2 = DesignLab.ChooseBattleRider(game.Game, role, wpnRole, playerId);
								}
								if (weapon != null)
								{
									nullable1 = game.GameDatabase.GetWeaponID(weapon.FileName, playerId);
									if (!nullable1.HasValue)
										nullable1 = new int?(game.GameDatabase.InsertWeapon(weapon, playerId));
								}
								else
									nullable1 = DesignLab.PickBestWeaponForRole(game.Game, (IList<LogicalWeapon>)LogicalWeapon.EnumerateWeaponFits(sectionAsset.Faction, sectionAsset.SectionName, (IEnumerable<LogicalWeapon>)list1, logicalBank.TurretSize, logicalBank.TurretClass).ToList<LogicalWeapon>(), playerId, WeaponRole.BRAWLER, (AITechStyles)null, logicalBank.TurretClass, name);
							}
							DesignModuleInfo designModuleInfo = new DesignModuleInfo()
							{
								DesignSectionInfo = design.DesignSections[i],
								MountNodeName = lm.NodeName,
								ModuleID = num,
								PsionicAbilities = new List<ModulePsionicInfo>(),
								WeaponID = nullable1,
								DesignID = nullable2
							};
							designModuleInfo.ID = game.GameDatabase.InsertDesignModule(designModuleInfo);
							game.GameDatabase.InsertModuleInstance(lm, module2, list2[i].ID);
							designModuleInfoList.Add(designModuleInfo);
						}
					}
				}
				design.DesignSections[i].WeaponBanks = weaponBankInfoList;
				design.DesignSections[i].Modules = designModuleInfoList;
			}
			DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design);
			game.GameDatabase.UpdateDesign(design);
			if (design == null)
				return;
			game.Game.AddDefaultStartingRiders(fleetId, design.ID, shipId);
		}

		public static string GetBestEngineTechString(App game, int playerId)
		{
			string str = "Unknown";
			string factionName = game.GameDatabase.GetFactionName(game.GameDatabase.GetPlayerFactionID(playerId));
			List<PlayerTechInfo> source = new List<PlayerTechInfo>(game.GameDatabase.GetPlayerTechInfos(playerId).Where<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.State == TechStates.Researched)));
			switch (factionName)
			{
				case "human":
					str = App.Localize("@TECH_DRIVE_NODE_FOCUS");
					if (source.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "DRV_Node_Pathing")))
					{
						str = App.Localize("@TECH_DRIVE_NODE_PATHING");
						break;
					}
					break;
				case "tarkas":
					str = App.Localize("@TECH_DRIVE_HYPER_FIELD");
					if (source.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "DRV_Warp")))
					{
						str = App.Localize("@TECH_DRIVE_WARP");
						break;
					}
					break;
				case "zuul":
					str = App.Localize("@TECH_DRIVE_REND_DRIVE");
					if (source.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "DRV_Radiant_Drive")))
					{
						str = App.Localize("@TECH_DRIVE_RADIANT_DRIVE");
						break;
					}
					break;
				case "morrigi":
					str = App.Localize("@TECH_DRIVE_VOID_CARVER");
					if (source.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "DRV_Void_Mastery")))
					{
						str = App.Localize("@TECH_DRIVE_VOID_MASTERY");
						break;
					}
					break;
				case "liir_zuul":
					str = App.Localize("@TECH_DRIVE_STUTTER_WARP");
					if (source.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "DRV_Flicker_Warp")))
					{
						str = App.Localize("@TECH_DRIVE_FLICKER_WARP");
						break;
					}
					break;
				case "hiver":
					str = App.Localize("@TECH_DRIVE_GATE");
					if (source.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "DRV_Casting")))
						str = App.Localize("@TECH_DRIVE_CASTING");
					if (source.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "DRV_Far_Casting")))
					{
						str = App.Localize("@TECH_DRIVE_FAR_CASTING");
						break;
					}
					break;
				case "loa":
					str = App.Localize("@TECH_DRIVE_NEUTRINO_PULSE_ACCELERATORS");
					if (source.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "DRV_Standing_Neutrino_Waves")))
					{
						str = App.Localize("@TECH_DRIVE_STANDING_NEUTRINO_WAVES");
						break;
					}
					break;
			}
			return str;
		}

		public static string GetBestEnginePlantTechString(App game, int playerId)
		{
			string str = App.Localize("@TECH_DRIVE_FUSION");
			if (new List<PlayerTechInfo>(game.GameDatabase.GetPlayerTechInfos(playerId).Where<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.State == TechStates.Researched))).Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "NRG_Anti-Matter")))
				str = App.Localize("@TECH_DRIVE_ANTIMATTER");
			return str;
		}

		public static float GetGravityWellTravelDistance(GameDatabase gamedb, MoveOrderInfo moveOrder)
		{
			if (moveOrder.FromSystemID != 0 && moveOrder.FromSystemID == moveOrder.ToSystemID)
				return 0.0f;
			Vector3 from = moveOrder.FromSystemID != 0 ? gamedb.GetStarSystemInfo(moveOrder.FromSystemID).Origin : moveOrder.FromCoords;
			Vector3 to = moveOrder.ToSystemID != 0 ? gamedb.GetStarSystemInfo(moveOrder.ToSystemID).Origin : moveOrder.ToCoords;
			float num1 = (from - to).Length;
			float num2 = num1;
			double progress = (double)moveOrder.Progress;
			foreach (GameSession.IntersectingSystem intersectingSystem in GameSession.GetIntersectingSystems(gamedb, from, to, 2f))
			{
				if (intersectingSystem.SystemID == moveOrder.FromSystemID || intersectingSystem.SystemID == moveOrder.ToSystemID)
					num1 += 4f;
				else if (intersectingSystem.StartOrEnd)
				{
					num1 += (float)(2.0 * (2.0 - (double)intersectingSystem.Distance));
				}
				else
				{
					float val1 = 2f * (float)Math.Sqrt(4.0 - (double)intersectingSystem.Distance * (double)intersectingSystem.Distance);
					float num3 = 2f - intersectingSystem.Distance;
					float val2 = (float)((Math.Sqrt((double)num3 * 2.0 * ((double)num3 * 2.0) + (double)num3 * (double)num3) - (double)num3 * 2.0) * 2.0);
					num1 += Math.Min(val1, val2);
				}
			}
			if ((double)num1 > 3.0 * (double)num2)
				num1 = 3f * num2;
			return num1;
		}

		public static bool SystemsLinkedByNonPermenantNodes(GameSession game, int systemA, int systemB)
		{
			foreach (NodeLineInfo nodeLine in game.GameDatabase.GetNodeLines())
			{
				if (!nodeLine.IsPermenant && (nodeLine.System1ID == systemA && nodeLine.System2ID == systemB || nodeLine.System1ID == systemB && nodeLine.System2ID == systemA))
					return true;
			}
			return false;
		}

		public static NodeLineInfo GetSystemPermenantNodeLine(
		  GameDatabase db,
		  int SystemA,
		  int SystemB)
		{
			foreach (NodeLineInfo nodeLine in db.GetNodeLines())
			{
				if (nodeLine.IsPermenant && (nodeLine.System1ID == SystemA && nodeLine.System2ID == SystemB || nodeLine.System1ID == SystemB && nodeLine.System2ID == SystemA))
					return nodeLine;
			}
			return (NodeLineInfo)null;
		}

		public static NodeLineInfo GetSystemsNonPermenantNodeLine(
		  GameDatabase db,
		  int systemA,
		  int systemB)
		{
			foreach (NodeLineInfo nodeLine in db.GetNodeLines())
			{
				if (!nodeLine.IsPermenant && (nodeLine.System1ID == systemA && nodeLine.System2ID == systemB || nodeLine.System1ID == systemB && nodeLine.System2ID == systemA))
					return nodeLine;
			}
			return (NodeLineInfo)null;
		}

		public static bool FleetHasBore(GameDatabase db, int fleetid)
		{
			foreach (ShipInfo shipInfo in db.GetShipInfoByFleetID(fleetid, false))
			{
				DesignInfo designInfo = db.GetDesignInfo(shipInfo.DesignID);
				if (designInfo.Class == ShipClass.Cruiser)
				{
					ShipSectionAsset shipSectionAsset1 = (ShipSectionAsset)null;
					foreach (DesignSectionInfo designSection in designInfo.DesignSections)
					{
						DesignSectionInfo sect = designSection;
						ShipSectionAsset shipSectionAsset2 = db.AssetDatabase.ShipSections.FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sect.FilePath));
						if (shipSectionAsset2 != null && shipSectionAsset2.Type == ShipSectionType.Mission)
						{
							shipSectionAsset1 = shipSectionAsset2;
							break;
						}
					}
					if (shipSectionAsset1 != null && shipSectionAsset1.FileName.Contains("bore"))
						return true;
				}
				else if (designInfo.Class == ShipClass.Dreadnought)
				{
					ShipSectionAsset shipSectionAsset1 = (ShipSectionAsset)null;
					foreach (DesignSectionInfo designSection in designInfo.DesignSections)
					{
						DesignSectionInfo sect = designSection;
						ShipSectionAsset shipSectionAsset2 = db.AssetDatabase.ShipSections.FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sect.FilePath));
						if (shipSectionAsset2 != null && shipSectionAsset2.Type == ShipSectionType.Mission)
						{
							shipSectionAsset1 = shipSectionAsset2;
							break;
						}
					}
					if (shipSectionAsset1 != null && shipSectionAsset1.FileName.Contains("bore"))
						return true;
				}
			}
			return false;
		}

		public static float GetBoreHealthMultiplier(GameDatabase db, int playerid)
		{
			return db.PlayerHasTech(playerid, "DRV_Radiant_Drive") ? 2f : 1f;
		}

		public static int GetPlayerMaxAdmirals(GameDatabase gamedb, int playerID)
		{
			int num = 0;
			foreach (int playerColonySystemId in gamedb.GetPlayerColonySystemIDs(playerID))
			{
				foreach (ColonyInfo colonyInfo in gamedb.GetColonyInfosForSystem(playerColonySystemId))
				{
					PlanetInfo planetInfo = gamedb.GetPlanetInfo(colonyInfo.OrbitalObjectID);
					if ((double)planetInfo.Size <= 5.0)
						++num;
					else if ((double)planetInfo.Size <= 9.0)
						num += 2;
					else if ((double)planetInfo.Size >= 10.0)
						num += 3;
				}
				StationInfo forSystemAndPlayer = gamedb.GetNavalStationForSystemAndPlayer(playerColonySystemId, playerID);
				if (forSystemAndPlayer != null && forSystemAndPlayer.DesignInfo.StationLevel >= 4)
					++num;
			}
			return num;
		}

		public static float GetStationTacSensorRange(GameSession app, StationInfo station)
		{
			return GameSession.GetStationBaseTacSensorRange(app, station) + GameSession.GetStationAdditionalTacSensorRange(app, station);
		}

		public static float GetStationBaseTacSensorRange(GameSession app, StationInfo station)
		{
			ShipSectionAsset shipSectionAsset = app.AssetDatabase.GetShipSectionAsset(station.DesignInfo.DesignSections[0].FilePath);
			if (shipSectionAsset != null)
				return shipSectionAsset.TacticalSensorRange;
			return 0.0f;
		}

		public static float GetStationAdditionalTacSensorRange(GameSession app, StationInfo station)
		{
			app.GameDatabase.GetDesignInfo(station.DesignInfo.ID);
			return station.DesignInfo.TacSensorRange;
		}

		public static float GetFleetSensorRange(AssetDatabase assetdb, GameDatabase db, int fleetid)
		{
			FleetInfo fleetInfo = db.GetFleetInfo(fleetid);
			return GameSession.GetFleetSensorRange(assetdb, db, fleetInfo, (List<ShipInfo>)null);
		}

		public static float GetFleetSensorRange(
		  AssetDatabase assetdb,
		  GameDatabase db,
		  FleetInfo fleet,
		  List<ShipInfo> cachedShips = null)
		{
			float val1 = 0.0f;
			foreach (ShipInfo shipInfo in cachedShips != null ? (IEnumerable<ShipInfo>)cachedShips : db.GetShipInfoByFleetID(fleet.ID, true))
			{
				if (shipInfo.DesignInfo == null)
					shipInfo.DesignInfo = db.GetDesignInfo(shipInfo.DesignID);
				if (!ShipSectionAsset.IsBattleRiderClass(shipInfo.DesignInfo.GetRealShipClass().Value))
				{
					foreach (DesignSectionInfo designSection in db.GetDesignInfo(shipInfo.DesignID).DesignSections)
						val1 = Math.Max(val1, designSection.ShipSectionAsset.StrategicSensorRange);
				}
			}
			return val1;
		}

		public bool SystemHasPlayerColony(int systemid, int playerid)
		{
			IEnumerable<ColonyInfo> colonyInfosForSystem = this.GameDatabase.GetColonyInfosForSystem(systemid);
			if (colonyInfosForSystem.Any<ColonyInfo>())
			{
				foreach (ColonyInfo colonyInfo in colonyInfosForSystem)
				{
					if (colonyInfo.PlayerID == playerid)
						return true;
				}
			}
			return false;
		}

		public static float GetSupportRange(GameSession game, int playerID)
		{
			return GameSession.GetSupportRange(game.AssetDatabase, game.GameDatabase, playerID);
		}

		public static float GetSupportRange(AssetDatabase assetdb, GameDatabase db, int playerID)
		{
			return db.FindCurrentDriveSpeedForPlayer(playerID) * assetdb.StationSupportRangeModifier + db.GetStratModifier<float>(StratModifiers.GateCastDistance, playerID);
		}

		public static HashSet<int> GetStarSystemsWithGates(
		  GameDatabase db,
		  int playerId,
		  IEnumerable<int> systemIds)
		{
			HashSet<int> intSet = new HashSet<int>();
			foreach (int systemId in systemIds)
			{
				if (db.SystemHasGate(systemId, playerId))
					intSet.Add(systemId);
			}
			return intSet;
		}

		public static int GetStationGateCapacity(GameSession game, StationInfo stationInfo)
		{
			int num = 0;
			if (stationInfo.DesignInfo.StationType != StationType.GATE)
				return 0;
			switch (stationInfo.DesignInfo.StationLevel)
			{
				case 1:
					num = 10;
					break;
				case 2:
					num = 15;
					break;
				case 3:
					num = 25;
					break;
				case 4:
					num = 30;
					break;
				case 5:
					num = 50;
					break;
			}
			Dictionary<ModuleEnums.StationModuleType, int> dictionary = GameSession.CountStationModuleTypes(stationInfo, game.GameDatabase, true);
			if (dictionary.ContainsKey(ModuleEnums.StationModuleType.Amp))
				num += dictionary[ModuleEnums.StationModuleType.Amp] * 2;
			return num;
		}

		public static int GetTotalGateCapacity(GameSession game, int playerid)
		{
			int num = 0;
			foreach (StationInfo stationInfo in game.GameDatabase.GetStationInfosByPlayerID(playerid))
			{
				if (stationInfo.DesignInfo.StationType == StationType.GATE && stationInfo.DesignInfo.StationLevel > 0)
					num += GameSession.GetStationGateCapacity(game, stationInfo);
			}
			List<FleetInfo> list = game.GameDatabase.GetFleetInfosByPlayerID(playerid, FleetType.FL_GATE).ToList<FleetInfo>();
			return num + list.Count * 6;
		}

		public static int GetTotalGateUsage(GameSession game, int playerid)
		{
			int num = 0;
			foreach (FleetInfo fi in game.GameDatabase.GetFleetInfosByPlayerID(playerid, FleetType.FL_NORMAL))
			{
				if (game.GameDatabase.GetMoveOrderInfoByFleetID(fi.ID) == null)
				{
					MissionInfo missionByFleetId = game.GameDatabase.GetMissionByFleetID(fi.ID);
					if (missionByFleetId != null)
					{
						List<WaypointInfo> list = game.GameDatabase.GetWaypointsByMissionID(missionByFleetId.ID).ToList<WaypointInfo>();
						if (list.Count > 0)
						{
							WaypointInfo waypointInfo = list.First<WaypointInfo>();
							if (waypointInfo.Type == WaypointType.TravelTo)
							{
								int? systemId1 = waypointInfo.SystemID;
								int systemId2 = fi.SystemID;
								if ((systemId1.GetValueOrDefault() != systemId2 ? 1 : (!systemId1.HasValue ? 1 : 0)) != 0)
									goto label_8;
							}
							if (waypointInfo.Type != WaypointType.ReturnHome || fi.SupportingSystemID == fi.SystemID)
								continue;
							label_8:
							int tripTime;
							float tripDistance;
							List<int> bestTravelPath = Kerberos.Sots.StarFleet.StarFleet.GetBestTravelPath(game, fi.ID, fi.SystemID, waypointInfo.Type == WaypointType.ReturnHome ? game.GameDatabase.GetHomeSystem(game, waypointInfo.MissionID, fi) : waypointInfo.SystemID.Value, out tripTime, out tripDistance, missionByFleetId.UseDirectRoute, new float?(), new float?());
							if (game.GameDatabase.SystemHasGate(bestTravelPath.First<int>(), fi.PlayerID) && game.GameDatabase.SystemHasGate(bestTravelPath[1], playerid))
								num += game.GameDatabase.GetFleetCruiserEquivalent(fi.ID);
						}
					}
				}
			}
			return num;
		}

		public static IEnumerable<GameSession.IntersectingSystem> GetIntersectingSystems(
		  GameDatabase gamedb,
		  Vector3 from,
		  Vector3 to,
		  float radius)
		{
			List<GameSession.IntersectingSystem> intersectingSystemList = new List<GameSession.IntersectingSystem>();
			foreach (StarSystemInfo starSystemInfo in gamedb.GetStarSystemInfos())
			{
				float length1 = (starSystemInfo.Origin - from).Length;
				float length2 = (starSystemInfo.Origin - to).Length;
				if ((double)length1 < (double)radius)
				{
					GameSession.IntersectingSystem intersectingSystem;
					intersectingSystem.SystemID = starSystemInfo.ID;
					intersectingSystem.Distance = length1;
					intersectingSystem.StartOrEnd = true;
					intersectingSystemList.Add(intersectingSystem);
				}
				else if ((double)length2 < (double)radius)
				{
					GameSession.IntersectingSystem intersectingSystem;
					intersectingSystem.SystemID = starSystemInfo.ID;
					intersectingSystem.Distance = length2;
					intersectingSystem.StartOrEnd = true;
					intersectingSystemList.Add(intersectingSystem);
				}
				else
				{
					float num1 = Vector3.Dot(Vector3.Normalize(from - to), Vector3.Normalize(from - starSystemInfo.Origin));
					if ((double)num1 > 0.0)
					{
						float length3 = (from - starSystemInfo.Origin).Length;
						float num2 = length3 * num1;
						float num3 = (float)Math.Sqrt((double)length3 * (double)length3 - (double)num2 * (double)num2);
						if ((double)num3 < (double)radius)
						{
							GameSession.IntersectingSystem intersectingSystem;
							intersectingSystem.SystemID = starSystemInfo.ID;
							intersectingSystem.Distance = num3;
							intersectingSystem.StartOrEnd = false;
							intersectingSystemList.Add(intersectingSystem);
						}
					}
				}
			}
			return (IEnumerable<GameSession.IntersectingSystem>)intersectingSystemList;
		}

		public void UpdateFleetSupply()
		{
			List<FleetInfo> fleetInfoList = new List<FleetInfo>();
			foreach (FleetInfo fleetInfo in this._db.GetFleetInfos(FleetType.FL_NORMAL))
			{
				if (fleetInfo.SupportingSystemID != 0 || !fleetInfo.IsReserveFleet && this._db.GetPlayerInfo(fleetInfo.PlayerID).isStandardPlayer)
				{
					if (fleetInfo.SystemID == fleetInfo.SupportingSystemID)
					{
						fleetInfo.SupplyRemaining = Kerberos.Sots.StarFleet.StarFleet.GetSupplyCapacity(this.GameDatabase, fleetInfo.ID);
						fleetInfo.TurnsAway = 0;
					}
					else
					{
						if (!this.IsFleetInSupplyRange(fleetInfo.ID))
							fleetInfo.SupplyRemaining -= Kerberos.Sots.StarFleet.StarFleet.GetSupplyConsumption(this, fleetInfo.ID) * 0.5f;
						++fleetInfo.TurnsAway;
					}
					fleetInfoList.Add(fleetInfo);
				}
			}
			foreach (FleetInfo fleet in fleetInfoList)
				this._db.UpdateFleetInfo(fleet);
		}

		public bool IsFleetInSupplyRange(int fleetID)
		{
			FleetInfo fleetInfo = this.GameDatabase.GetFleetInfo(fleetID);
			if (fleetInfo == null)
				return false;
			PlayerInfo playerInfo = this.GameDatabase.GetPlayerInfo(fleetInfo.PlayerID);
			if (playerInfo != null)
			{
				FactionInfo factionInfo = this.GameDatabase.GetFactionInfo(playerInfo.FactionID);
				if (factionInfo != null && factionInfo.Name == "hiver" && this.GameDatabase.SystemHasGate(fleetInfo.SystemID, playerInfo.ID))
					return true;
			}
			FleetLocation fleetLocation = this.GameDatabase.GetFleetLocation(fleetID, false);
			float supportRange = GameSession.GetSupportRange(this, fleetInfo.PlayerID);
			foreach (int playerColonySystemId in this.GameDatabase.GetPlayerColonySystemIDs(fleetInfo.PlayerID))
			{
				if ((double)(this.GameDatabase.GetStarSystemOrigin(playerColonySystemId) - fleetLocation.Coords).Length <= (double)supportRange)
					return true;
			}
			return false;
		}

		public bool IsSystemInSupplyRange(int systemID, int playerID)
		{
			PlayerInfo playerInfo = this.GameDatabase.GetPlayerInfo(playerID);
			if (playerInfo != null)
			{
				FactionInfo factionInfo = this.GameDatabase.GetFactionInfo(playerInfo.FactionID);
				if (factionInfo != null && factionInfo.Name == "hiver" && this.GameDatabase.SystemHasGate(systemID, playerInfo.ID))
					return true;
			}
			List<int> list = this.GameDatabase.GetPlayerColonySystemIDs(playerID).ToList<int>();
			if (list.Contains(systemID))
				return true;
			float supportRange = GameSession.GetSupportRange(this, playerID);
			Vector3 starSystemOrigin = this.GameDatabase.GetStarSystemOrigin(systemID);
			foreach (int systemId in list)
			{
				if ((double)(this.GameDatabase.GetStarSystemOrigin(systemId) - starSystemOrigin).Length <= (double)supportRange)
					return true;
			}
			return false;
		}

		public bool IsSystemInSupplyRangeOfSupportingSystem(
		  int supportingSystemID,
		  int systemID,
		  int playerID)
		{
			GameDatabase gameDatabase = this.App.GameDatabase;
			StarSystemInfo starSystemInfo1 = gameDatabase.GetStarSystemInfo(systemID);
			if (starSystemInfo1 == (StarSystemInfo)null)
				return false;
			StarSystemInfo starSystemInfo2 = gameDatabase.GetStarSystemInfo(supportingSystemID);
			if (starSystemInfo2 == (StarSystemInfo)null)
			{
				GameSession.Warn("Unable to find supporting system for player.");
				return false;
			}
			PlayerInfo playerInfo = gameDatabase.GetPlayerInfo(playerID);
			if (playerInfo != null)
			{
				FactionInfo factionInfo = gameDatabase.GetFactionInfo(playerInfo.FactionID);
				if (factionInfo != null && factionInfo.Name == "hiver" && gameDatabase.SystemHasGate(systemID, playerInfo.ID))
					return true;
			}
			return (double)(starSystemInfo2.Origin - starSystemInfo1.Origin).Length < (double)GameSession.GetSupportRange(this, playerID);
		}

		public float GetStationBuildModifierForSystem(int systemId, int playerId)
		{
			float num = 1f;
			foreach (StationInfo station in this._db.GetStationForSystemAndPlayer(systemId, playerId).ToList<StationInfo>())
			{
				Dictionary<ModuleEnums.StationModuleType, int> dictionary = GameSession.CountStationModuleTypes(station, this._db, true);
				if (station.DesignInfo.StationType == StationType.NAVAL && dictionary.ContainsKey(ModuleEnums.StationModuleType.Dock))
					num += (float)dictionary[ModuleEnums.StationModuleType.Dock] * 0.02f;
			}
			return num;
		}

		public float GetStationRepairModifierForSystem(int systemId, int playerId)
		{
			float num = 1f;
			foreach (StationInfo station in this._db.GetStationForSystemAndPlayer(systemId, playerId).ToList<StationInfo>())
			{
				Dictionary<ModuleEnums.StationModuleType, int> dictionary = GameSession.CountStationModuleTypes(station, this._db, true);
				if (station.DesignInfo.StationType == StationType.NAVAL && dictionary.ContainsKey(ModuleEnums.StationModuleType.Repair))
					num += (float)dictionary[ModuleEnums.StationModuleType.Repair] * 0.02f;
			}
			return num;
		}

		public void BuildAtSystem(int SystemId, int playerId)
		{
			float num1 = 0.0f;
			foreach (ColonyInfo colony in this._db.GetColonyInfosForSystem(SystemId).ToList<ColonyInfo>())
			{
				if (colony.PlayerID == playerId)
					num1 += Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetConstructionPoints(this, colony);
			}
			float num2 = num1 * this.GetStationBuildModifierForSystem(SystemId, playerId);
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int? reserveFleetId = this._db.GetReserveFleetID(playerId, SystemId);
			List<ShipInfo> shipInfoList = new List<ShipInfo>();
			if (reserveFleetId.HasValue)
				this._db.GetShipInfoByFleetID(reserveFleetId.Value, true).Where<ShipInfo>((Func<ShipInfo, bool>)(x =>
			   {
				   int? aiFleetId = x.AIFleetID;
				   int num6 = 0;
				   if (aiFleetId.GetValueOrDefault() == num6 & aiFleetId.HasValue || !x.AIFleetID.HasValue)
				   {
					   RealShipClasses? realShipClass = x.DesignInfo.GetRealShipClass();
					   if (realShipClass.HasValue)
					   {
						   realShipClass = x.DesignInfo.GetRealShipClass();
						   return !ShipSectionAsset.IsWeaponBattleRiderClass(realShipClass.Value);
					   }
				   }
				   return false;
			   })).ToList<ShipInfo>();
			foreach (BuildOrderInfo buildOrder in this._db.GetBuildOrdersForSystem(SystemId).ToList<BuildOrderInfo>())
			{
				DesignInfo designInfo = this._db.GetDesignInfo(buildOrder.DesignID);
				if (designInfo.PlayerID == playerId)
				{
					int num6 = buildOrder.ProductionTarget - buildOrder.Progress;
					float num7 = 0.0f;
					if (!designInfo.isPrototyped)
						num7 = (float)(int)((double)num2 * ((double)this.GameDatabase.GetStratModifierFloatToApply(StratModifiers.PrototypeTimeModifier, playerId) - 1.0));
					if ((double)num6 > (double)num2 - (double)num7)
					{
						buildOrder.Progress += (int)((double)num2 - (double)num7);
						this._db.UpdateBuildOrder(buildOrder);
						break;
					}
					if ((double)num6 <= (double)num2 - (double)num7)
					{
						if (playerId == this.LocalPlayer.ID)
						{
							this._app.SteamHelper.DoAchievement(AchievementType.SOTS2_STEEL_PUSHER);
							if (designInfo.Class == ShipClass.Dreadnought && this.LocalPlayer.Faction.Name == "human")
								this._app.SteamHelper.DoAchievement(AchievementType.SOTS2_SWORD_OF_STARS);
							if (designInfo.Class == ShipClass.Leviathan && this.LocalPlayer.Faction.Name == "human" && designInfo.Name == "Leviathan")
								this._app.SteamHelper.DoAchievement(AchievementType.SOTS2_IMITATION);
						}
						this.ConstructShip(designInfo, playerId, buildOrder.SystemID, buildOrder.MissionID, buildOrder.ShipName, buildOrder.AIFleetID, buildOrder.LoaCubes);
						num2 -= (float)num6;
						this._db.RemoveBuildOrder(buildOrder.ID);
						if (buildOrder.InvoiceID.HasValue)
						{
							bool flag = false;
							foreach (BuildOrderInfo buildOrderInfo in this._db.GetBuildOrdersForInvoiceInstance(buildOrder.InvoiceID.Value).ToList<BuildOrderInfo>())
							{
								if (buildOrderInfo.Progress != buildOrderInfo.ProductionTarget)
									flag = true;
							}
							if (!flag)
							{
								this._db.RemoveInvoiceInstance(buildOrder.InvoiceID.Value);
								if (this._db.GetInvoicesForSystem(playerId, SystemId).Count<InvoiceInstanceInfo>() == 0)
									this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
									{
										EventType = TurnEventType.EV_INVOICES_COMPLETE,
										EventMessage = TurnEventMessage.EM_INVOICES_COMPLETE,
										TurnNumber = this._db.GetTurnCount(),
										PlayerID = playerId,
										SystemID = SystemId
									});
							}
						}
						if (((IEnumerable<DesignSectionInfo>)designInfo.DesignSections).Sum<DesignSectionInfo>((Func<DesignSectionInfo, int>)(x => x.ShipSectionAsset.FreighterSpace)) > 0)
							++num4;
						else if (((IEnumerable<DesignSectionInfo>)designInfo.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.FilePath.ToLower().Contains("_sdb"))))
							++num5;
						else
							++num3;
					}
				}
			}
			if (num3 > 0)
				this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_SHIPS_BUILT,
					EventMessage = num3 > 1 ? TurnEventMessage.EM_SHIPS_BUILT_MULTIPLE : TurnEventMessage.EM_SHIPS_BUILT_SINGLE,
					TurnNumber = this._db.GetTurnCount(),
					PlayerID = playerId,
					SystemID = SystemId,
					NumShips = num3
				});
			if (num4 > 0)
				this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_FREIGHTER_BUILT,
					EventMessage = num4 > 1 ? TurnEventMessage.EM_FREIGHTER_BUILT : TurnEventMessage.EM_FREIGHTERS_BUILT,
					TurnNumber = this._db.GetTurnCount(),
					PlayerID = playerId,
					SystemID = SystemId,
					NumShips = num4
				});
			if (num5 <= 0)
				return;
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_SDB_BUILT,
				EventMessage = num5 > 1 ? TurnEventMessage.EM_SDB_BUILT : TurnEventMessage.EM_SDBS_BUILT,
				TurnNumber = this._db.GetTurnCount(),
				PlayerID = playerId,
				SystemID = SystemId,
				NumShips = num5
			});
		}

		public void RetrofitShip(int shipid, int designid, float costmult = 1f)
		{
			ShipInfo shipInfo1 = this._db.GetShipInfo(shipid, true);
			DesignInfo designInfo = this._db.GetDesignInfo(designid);
			if (shipInfo1 == null || designInfo == null)
				return;
			foreach (ShipInfo shipInfo2 in this._app.GameDatabase.GetBattleRidersByParentID(shipInfo1.ID).ToList<ShipInfo>())
			{
				RealShipClasses? realShipClass = shipInfo2.DesignInfo.GetRealShipClass();
				if (realShipClass.HasValue && ShipSectionAsset.IsWeaponBattleRiderClass(realShipClass.Value))
					this._db.RemoveShip(shipInfo2.ID);
			}
			this._app.GameDatabase.UpdateShipDesign(shipid, designid, new int?());
			this.AddDefaultStartingRiders(shipInfo1.FleetID, designid, shipInfo1.ID);
			double num = (double)Kerberos.Sots.StarFleet.StarFleet.CalculateRetrofitCost(this._app, shipInfo1.DesignInfo, designInfo) * (double)costmult;
			PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(designInfo.PlayerID);
			this._db.UpdatePlayerSavings(designInfo.PlayerID, playerInfo.Savings - num);
		}

		public void RetrofitAtSystem(int SystemId, int playerId)
		{
			int num = 0;
			bool flag = Kerberos.Sots.StarFleet.StarFleet.SystemSupportsRetrofitting(this._app, SystemId, playerId);
			List<RetrofitOrderInfo> list = this._db.GetRetrofitOrdersForSystem(SystemId).ToList<RetrofitOrderInfo>();
			if (flag)
			{
				int retrofitCapacity = Kerberos.Sots.StarFleet.StarFleet.GetSystemRetrofitCapacity(this._app, SystemId, playerId);
				Dictionary<int, int> dictionary1 = new Dictionary<int, int>();
				foreach (RetrofitOrderInfo retrofitOrderInfo in list)
				{
					if (retrofitCapacity > 0)
					{
						if (!dictionary1.ContainsKey(retrofitOrderInfo.DesignID))
							dictionary1.Add(retrofitOrderInfo.DesignID, 0);
						Dictionary<int, int> dictionary2;
						int designId;
						(dictionary2 = dictionary1)[designId = retrofitOrderInfo.DesignID] = dictionary2[designId] + 1;
						ShipInfo shipInfo = this._db.GetShipInfo(retrofitOrderInfo.ShipID, true);
						this.RetrofitShip(retrofitOrderInfo.ShipID, retrofitOrderInfo.DesignID, 1f);
						bool defenseasset = false;
						if (shipInfo.IsPlatform() || shipInfo.IsSDB())
							defenseasset = true;
						this._db.RemoveRetrofitOrder(retrofitOrderInfo.ID, false, defenseasset);
						--retrofitCapacity;
						++num;
						if (this._db.GetRetrofitOrdersForInvoiceInstance(retrofitOrderInfo.InvoiceID.Value).ToList<RetrofitOrderInfo>().Count <= 0)
							this._db.RemoveInvoiceInstance(retrofitOrderInfo.InvoiceID.Value);
					}
					else
						break;
				}
				foreach (KeyValuePair<int, int> keyValuePair in dictionary1)
				{
					KeyValuePair<int, int> kvp = keyValuePair;
					if (kvp.Value > 1)
					{
						this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_RETROFIT_COMPLETE,
							EventMessage = TurnEventMessage.EM_RETROFIT_COMPLETE_MULTI,
							TurnNumber = this._db.GetTurnCount(),
							PlayerID = playerId,
							SystemID = SystemId,
							DesignID = kvp.Key
						});
					}
					else
					{
						RetrofitOrderInfo retrofitOrderInfo = list.First<RetrofitOrderInfo>((Func<RetrofitOrderInfo, bool>)(x => x.DesignID == kvp.Key));
						this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_RETROFIT_COMPLETE,
							EventMessage = TurnEventMessage.EM_RETROFIT_COMPLETE_MULTI,
							TurnNumber = this._db.GetTurnCount(),
							PlayerID = playerId,
							SystemID = SystemId,
							DesignID = kvp.Key,
							ShipID = retrofitOrderInfo.ShipID
						});
					}
				}
			}
			else
			{
				foreach (RetrofitOrderInfo retrofitOrderInfo in list)
				{
					this._db.RemoveRetrofitOrder(retrofitOrderInfo.ID, false, false);
					if (this._db.GetRetrofitOrdersForInvoiceInstance(retrofitOrderInfo.InvoiceID.Value).ToList<RetrofitOrderInfo>().Count <= 0)
						this._db.RemoveInvoiceInstance(retrofitOrderInfo.InvoiceID.Value);
				}
			}
		}

		public void DetectPiracyFleets(int systemid, int playerid)
		{
			List<MissionInfo> list1 = this._db.GetMissionsBySystemDest(systemid).Where<MissionInfo>((Func<MissionInfo, bool>)(x => x.Type == MissionType.PIRACY)).ToList<MissionInfo>();
			MissionInfo missionInfo1 = (MissionInfo)null;
			foreach (MissionInfo missionInfo2 in list1)
			{
				FleetInfo fleetInfo = this._db.GetFleetInfo(missionInfo2.FleetID);
				if (this._db.GetAdmiralInfo(fleetInfo.AdmiralID) == null)
					Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleetInfo, true);
				else if (fleetInfo.PlayerID != playerid && missionInfo1 == null)
					missionInfo1 = missionInfo2;
			}
			if (missionInfo1 == null)
				return;
			FleetInfo fleetInfo1 = this._db.GetFleetInfo(missionInfo1.FleetID);
			AdmiralInfo admiralInfo = this._db.GetAdmiralInfo(fleetInfo1.AdmiralID);
			if (fleetInfo1.PlayerID == playerid || (this._db.GetPiracyFleetDetectionInfoForFleet(fleetInfo1.ID).Any<PiracyFleetDetectionInfo>((Func<PiracyFleetDetectionInfo, bool>)(x => x.PlayerID == playerid)) || fleetInfo1.SystemID != systemid))
				return;
			int num1 = 0;
			MissionInfo missionInfo3 = this._db.GetMissionsBySystemDest(systemid).Where<MissionInfo>((Func<MissionInfo, bool>)(x => x.Type == MissionType.PATROL)).FirstOrDefault<MissionInfo>((Func<MissionInfo, bool>)(x => this._db.GetFleetInfo(x.FleetID).PlayerID == playerid));
			if (missionInfo3 != null)
			{
				FleetInfo fleetInfo2 = this._db.GetFleetInfo(missionInfo3.FleetID);
				if (this._db.GetAdmiralInfo(fleetInfo2.AdmiralID).ReactionBonus > admiralInfo.EvasionBonus)
					num1 += 50;
				if (this._db.GetShipInfoByFleetID(fleetInfo2.ID, false).ToList<ShipInfo>().Any<ShipInfo>((Func<ShipInfo, bool>)(x => ((IEnumerable<DesignSectionInfo>)x.DesignInfo.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(j => j.ShipSectionAsset.isDeepScan)))))
					num1 += 30;
			}
			if (this._db.IsStealthFleet(fleetInfo1.ID))
				num1 -= 25;
			List<ShipInfo> list2 = this._db.GetShipInfoByFleetID(fleetInfo1.ID, false).ToList<ShipInfo>();
			if (list2.Where<ShipInfo>((Func<ShipInfo, bool>)(x => ((IEnumerable<DesignSectionInfo>)x.DesignInfo.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(j =>
		 {
			 if (j.ShipSectionAsset.cloakingType != CloakingType.Cloaking)
				 return j.ShipSectionAsset.cloakingType == CloakingType.ImprovedCloaking;
			 return true;
		 })))).Count<ShipInfo>() == list2.Count)
				num1 -= 50;
			int? defenseFleetId = this._db.GetDefenseFleetID(systemid, playerid);
			if (defenseFleetId.HasValue)
			{
				this._db.GetFleetInfo(defenseFleetId.Value);
				int num2 = this._db.GetShipInfoByFleetID(fleetInfo1.ID, false).ToList<ShipInfo>().Where<ShipInfo>((Func<ShipInfo, bool>)(x =>
			   {
				   if (x.DesignInfo.IsPlatform() && x.DesignInfo.GetPlatformType().HasValue)
					   return x.DesignInfo.GetPlatformType().Value == PlatformTypes.scansat;
				   return false;
			   })).Count<ShipInfo>();
				num1 += num2 * 5;
			}
			if (new Random().Next(0, 100) >= num1)
				return;
			this._db.InsertPiracyFleetDetectionInfo(fleetInfo1.ID, playerid);
		}

		public int ConstructStation(DesignInfo designInfo, int parentOrbitalId, bool free = false)
		{
			PlayerInfo playerInfo = this._db.GetPlayerInfo(designInfo.PlayerID);
			if (!free)
			{
				this._db.UpdatePlayerSavings(designInfo.PlayerID, playerInfo.Savings - (double)designInfo.SavingsCost);
				if (playerInfo.ID == this.LocalPlayer.ID)
					this.App.SteamHelper.DoAchievement(AchievementType.SOTS2_STATION);
			}
			return this.InsertStation(designInfo, parentOrbitalId, playerInfo.ID);
		}

		public int ConstructShip(
		  DesignInfo design,
		  int playerID,
		  int SystemID,
		  int missionID,
		  string shipName,
		  int? aiFleetID,
		  int LoaCubes)
		{
			TradeResultsTable tradeResultsTable = this._db.GetTradeResultsTable();
			if (!tradeResultsTable.TradeNodes.ContainsKey(SystemID) || tradeResultsTable.TradeNodes[SystemID].ProductionCapacity == tradeResultsTable.TradeNodes[SystemID].ExportInt + tradeResultsTable.TradeNodes[SystemID].ExportLoc + tradeResultsTable.TradeNodes[SystemID].ExportProv)
			{
				int num = this._db.GetStarSystemInfo(SystemID).IsOpen ? 1 : 0;
				foreach (ColonyInfo colony in this._db.GetColonyInfosForSystem(SystemID).ToList<ColonyInfo>())
				{
					colony.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.ShipsBuiltWithTradeMaxed, 1);
					this._db.UpdateColony(colony);
				}
			}
			PlayerInfo playerInfo = this._db.GetPlayerInfo(playerID);
			this.GetPlayerObject(playerID);
			if (design.IsLoaCube())
				design.SavingsCost = LoaCubes * this.AssetDatabase.LoaCostPerCube;
			int num1 = design.SavingsCost;
			if (!design.isPrototyped)
			{
				switch (design.Class)
				{
					case ShipClass.Cruiser:
						num1 = (int)((double)design.SavingsCost * (double)this._db.GetStratModifierFloatToApply(StratModifiers.PrototypeSavingsCostModifierCR, playerID));
						break;
					case ShipClass.Dreadnought:
						num1 = (int)((double)design.SavingsCost * (double)this._db.GetStratModifierFloatToApply(StratModifiers.PrototypeSavingsCostModifierDN, playerID));
						break;
					case ShipClass.Leviathan:
						num1 = (int)((double)design.SavingsCost * (double)this._db.GetStratModifierFloatToApply(StratModifiers.PrototypeSavingsCostModifierLV, playerID));
						break;
					case ShipClass.Station:
						RealShipClasses? realShipClass = design.GetRealShipClass();
						RealShipClasses realShipClasses = RealShipClasses.Platform;
						if (realShipClass.GetValueOrDefault() == realShipClasses & realShipClass.HasValue)
						{
							num1 = (int)((double)design.SavingsCost * (double)this._db.GetStratModifierFloatToApply(StratModifiers.PrototypeSavingsCostModifierPF, playerID));
							break;
						}
						break;
				}
			}
			this._db.UpdatePlayerSavings(playerID, playerInfo.Savings - (double)num1);
			GameSession.Trace("Ship of type " + design.Name + " constructed at " + this._db.GetStarSystemInfo(SystemID).Name);
			bool flag1 = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			foreach (DesignSectionInfo designSection in design.DesignSections)
			{
				DesignSectionInfo dsi = designSection;
				ShipSectionAsset shipSectionAsset = this.AssetDatabase.ShipSections.FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == dsi.FilePath));
				if (shipSectionAsset.IsFreighter)
					flag1 = true;
				if (dsi.FilePath.ToLower().Contains("_sdb"))
					flag2 = true;
				if (shipSectionAsset.isPolice)
					flag3 = true;
				int num2 = shipSectionAsset.isMineLayer ? 1 : 0;
				if (shipSectionAsset.ColonizationSpace != 0)
					flag4 = true;
			}
			if (flag3 && this._app.GetStratModifier<bool>(StratModifiers.AllowPoliceInCombat, playerID))
				this._app.GameDatabase.InsertGovernmentAction(playerID, App.Localize("@GA_POLICEBUILT"), "PoliceBuilt", 0, 0);
			if (flag4)
				this._app.GameDatabase.InsertGovernmentAction(playerID, App.Localize("@GA_COLONIZERBUILT"), "ColonizerBuilt", 0, 0);
			int carrierID;
			if (flag1)
			{
				carrierID = this._db.InsertFreighter(SystemID, playerID, design.ID, true);
				this._app.GameDatabase.InsertGovernmentAction(playerID, App.Localize("@GA_BUILTFREIGHTER"), "BuiltFreighter", 0, 0);
			}
			else
			{
				ShipSectionAsset shipSectionAsset = this._app.AssetDatabase.ShipSections.FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == design.DesignSections[0].FilePath));
				int fleetId;
				int fleetID;
				if (shipSectionAsset != null && ((IEnumerable<DesignSectionInfo>)design.DesignSections).Count<DesignSectionInfo>() > 0 && shipSectionAsset.RealClass == RealShipClasses.Platform)
				{
					MissionInfo missionInfo = this._db.GetMissionInfo(missionID);
					if (missionInfo != null)
						fleetId = missionInfo.FleetID;
					fleetID = this._db.InsertOrGetDefenseFleetInfo(SystemID, playerID).ID;
				}
				else if (flag2)
				{
					MissionInfo missionInfo = this._db.GetMissionInfo(missionID);
					if (missionInfo != null)
						fleetId = missionInfo.FleetID;
					fleetID = this._db.InsertOrGetDefenseFleetInfo(SystemID, playerID).ID;
				}
				else if (missionID > 0)
				{
					MissionInfo missionInfo = this._db.GetMissionInfo(missionID);
					fleetID = missionInfo == null ? this._db.InsertOrGetReserveFleetInfo(SystemID, playerID).ID : missionInfo.FleetID;
				}
				else
					fleetID = this._db.InsertOrGetReserveFleetInfo(SystemID, playerID).ID;
				if (design.IsLoaCube())
				{
					ShipInfo shipInfo = this._db.GetShipInfoByFleetID(fleetID, false).ToList<ShipInfo>().FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.IsLoaCube()));
					if (shipInfo != null)
					{
						this._db.UpdateShipLoaCubes(shipInfo.ID, shipInfo.LoaCubes + LoaCubes);
						return shipInfo.ID;
					}
				}
				carrierID = this._db.InsertShip(fleetID, design.ID, shipName, (ShipParams)0, aiFleetID, LoaCubes);
				if (design.Class == ShipClass.Leviathan)
					GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_LEVIATHAN_BUILT, playerID, new int?(), new int?(), new int?(SystemID));
				if (((IEnumerable<DesignSectionInfo>)design.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.FilePath.Contains("cnc"))))
					GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_FLAGSHIP_BUILT, playerID, new int?(), new int?(), new int?(SystemID));
				this.AddDefaultStartingRiders(fleetID, design.ID, carrierID);
			}
			if (!design.isPrototyped)
			{
				design.isPrototyped = true;
				this._app.GameDatabase.UpdateDesignPrototype(design.ID, design.isPrototyped);
				if (design.CanHaveAttributes())
				{
					int modifierIntToApply1 = this._db.GetStratModifierIntToApply(StratModifiers.BadDesignAttributePercent, playerInfo.ID);
					int modifierIntToApply2 = this._db.GetStratModifierIntToApply(StratModifiers.GoodDesignAttributePercent, playerInfo.ID);
					int num2 = this._random.Next(100);
					if (num2 < modifierIntToApply1)
					{
						SectionEnumerations.DesignAttribute da = this._random.Choose<SectionEnumerations.DesignAttribute>((IList<SectionEnumerations.DesignAttribute>)SectionEnumerations.BadDesignAttributes);
						this._app.GameDatabase.InsertDesignAttribute(design.ID, da);
						GameSession.Trace("Ship of type " + design.Name + " was designed with a bad attribute: " + da.ToString());
					}
					else if (num2 < modifierIntToApply1 + modifierIntToApply2)
					{
						SectionEnumerations.DesignAttribute da = this._random.Choose<SectionEnumerations.DesignAttribute>((IList<SectionEnumerations.DesignAttribute>)SectionEnumerations.GoodDesignAttributes);
						this._app.GameDatabase.InsertDesignAttribute(design.ID, da);
						GameSession.Trace("Ship of type " + design.Name + " was designed with a good attribute: " + da.ToString());
					}
					if (this._db.GetStratModifier<bool>(StratModifiers.ShowPrototypeDesignAttributes, playerInfo.ID))
						this._db.UpdateDesignAttributeDiscovered(design.ID, true);
				}
				this._app.TurnEvents.Add(new TurnEvent()
				{
					EventType = TurnEventType.EV_PROTOTYPE_COMPLETE,
					EventMessage = TurnEventMessage.EM_PROTOTYPE_COMPLETE,
					TurnNumber = this._db.GetTurnCount(),
					PlayerID = playerID,
					SystemID = SystemID,
					DesignID = design.ID
				});
			}
			return carrierID;
		}

		public void UpgradeStation(StationInfo s, DesignInfo newDesign)
		{
			if (newDesign.StationType == StationType.DIPLOMATIC && newDesign.StationLevel == 5)
			{
				foreach (PlayerInfo playerInfo in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
				{
					if (newDesign.PlayerID != playerInfo.ID)
						this._db.ApplyDiplomacyReaction(playerInfo.ID, newDesign.PlayerID, StratModifiers.DiplomacyReactionStarChamber, 1);
				}
			}
			newDesign.ID = s.DesignInfo.ID;
			newDesign.DesignSections[0].ID = s.DesignInfo.DesignSections[0].ID;
			List<LogicalModuleMount> list = ((IEnumerable<LogicalModuleMount>)this.App.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == newDesign.DesignSections[0].FilePath)).Modules).ToList<LogicalModuleMount>();
			string name = this.AssetDatabase.GetFaction(this.GameDatabase.GetPlayerFactionID(s.PlayerID)).Name;
			StationModuleQueue.UpdateStationMapsForFaction(name);
			foreach (DesignModuleInfo module in s.DesignInfo.DesignSections[0].Modules)
			{
				if (module.StationModuleType.HasValue && module.StationModuleType.Value < ModuleEnums.StationModuleType.NumModuleTypes)
				{
					ModuleEnums.ModuleSlotTypes desiredModuleType = AssetDatabase.StationModuleTypeToMountTypeMap[module.StationModuleType.Value];
					if (desiredModuleType == ModuleEnums.ModuleSlotTypes.Habitation && name != AssetDatabase.GetModuleFactionName(module.StationModuleType.Value))
						desiredModuleType = ModuleEnums.ModuleSlotTypes.AlienHabitation;
					LogicalModuleMount logicalModuleMount = list.FirstOrDefault<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => x.ModuleType == desiredModuleType.ToString()));
					if (logicalModuleMount != null)
					{
						module.MountNodeName = logicalModuleMount.NodeName;
						module.DesignSectionInfo = s.DesignInfo.DesignSections[0];
						this._db.UpdateDesignModuleNodeName(module);
						list.Remove(logicalModuleMount);
					}
				}
			}
			foreach (DesignModuleInfo module in this._db.GetQueuedStationModules(s.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>())
			{
				if (module.StationModuleType.HasValue)
				{
					ModuleEnums.ModuleSlotTypes desiredModuleType = AssetDatabase.StationModuleTypeToMountTypeMap[module.StationModuleType.Value];
					if (desiredModuleType == ModuleEnums.ModuleSlotTypes.Habitation && this.AssetDatabase.GetFaction(this.GameDatabase.GetPlayerFactionID(s.PlayerID)).Name != AssetDatabase.GetModuleFactionName(module.StationModuleType.Value))
						desiredModuleType = ModuleEnums.ModuleSlotTypes.AlienHabitation;
					LogicalModuleMount logicalModuleMount = list.FirstOrDefault<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => x.ModuleType == desiredModuleType.ToString()));
					if (logicalModuleMount != null)
					{
						module.MountNodeName = logicalModuleMount.NodeName;
						module.DesignSectionInfo = s.DesignInfo.DesignSections[0];
						this._db.UpdateQueuedModuleNodeName(module);
						list.Remove(logicalModuleMount);
					}
				}
			}
			s.DesignInfo = newDesign;
			this._db.UpdateStation(s);
			this._db.UpdateShipDesign(s.ShipID, newDesign.ID, new int?(s.ID));
		}

		public string GetUpgradeStationSoundCueName(DesignInfo newDesign)
		{
			string name = this._app.Game.GetPlayerObject(newDesign.PlayerID).Faction.Name;
			string str = "";
			switch (newDesign.StationType)
			{
				case StationType.NAVAL:
					switch (newDesign.StationLevel)
					{
						case 1:
							str = string.Format("STRAT_040-01_{0}_NavalOutpostComplete", (object)name);
							break;
						case 2:
							str = string.Format("STRAT_041-01_{0}_OutpostUpgradedToForwardBase", (object)name);
							break;
						case 3:
							str = string.Format("STRAT_042-01_{0}_ForwardBaseUpgradedToNavalBase", (object)name);
							break;
						case 4:
							str = string.Format("STRAT_043-01_{0}_NavalBaseUpgradedToStarBase", (object)name);
							break;
						case 5:
							str = string.Format("STRAT_044-01_{0}_StarBaseUpgradedToSectorBase", (object)name);
							break;
					}
					break;
				case StationType.SCIENCE:
					switch (newDesign.StationLevel)
					{
						case 1:
							str = string.Format("STRAT_082-01_{0}_FieldStationComplete", (object)name);
							break;
						case 2:
							str = string.Format("STRAT_083-01_{0}_FieldStationUpgradedToStarLab", (object)name);
							break;
						case 3:
							str = string.Format("STRAT_084-01_{0}_StarLabUpgradedToResearchBase", (object)name);
							break;
						case 4:
							str = string.Format("STRAT_085-01_{0}_ResearchBaseUpgradedToPolytechnic", (object)name);
							break;
						case 5:
							str = string.Format("STRAT_112-01_{0}_PolytechnicUpgradedToScienceCenter", (object)name);
							break;
					}
					break;
				case StationType.CIVILIAN:
					switch (newDesign.StationLevel)
					{
						case 1:
							str = string.Format("STRAT_019-01_{0}_WayStationComplete", (object)name);
							break;
						case 2:
							str = string.Format("STRAT_020-01_{0}_WayStationUpgradedToTradingPost", (object)name);
							break;
						case 3:
							str = string.Format("STRAT_021-01_{0}_TradingPostUpgradedToMerchanterStation", (object)name);
							break;
						case 4:
							str = string.Format("STRAT_022-01_{0}_MerchanterStationUpgradedToNexus", (object)name);
							break;
						case 5:
							str = string.Format("STRAT_023-01_{0}_NexusUpgradedToStarCity", (object)name);
							break;
					}
					break;
				case StationType.DIPLOMATIC:
					switch (newDesign.StationLevel)
					{
						case 1:
							str = string.Format("STRAT_063-01_{0}_CustomsStationComplete", (object)name);
							break;
						case 2:
							str = string.Format("STRAT_111-01_{0}_CustomsStationUpgradedToConsulate", (object)name);
							break;
						case 3:
							str = string.Format("STRAT_064-01_{0}_ConsulateUpgradedToEmbassy", (object)name);
							break;
						case 4:
							str = string.Format("STRAT_065-01_{0}_EmbassyUpgradedToCouncilStation", (object)name);
							break;
						case 5:
							str = string.Format("STRAT_066-01_{0}_CouncilStationUpgradedToStarChamber", (object)name);
							break;
					}
					break;
				case StationType.GATE:
					switch (newDesign.StationLevel)
					{
						case 1:
							str = string.Format("STRAT_020-01_{0}_GatewayComplete", (object)name);
							break;
						case 2:
							str = string.Format("STRAT_020-01_{0}_GatewayUpgradedToCaster", (object)name);
							break;
						case 3:
							str = string.Format("STRAT_020-01_{0}_CasterUpgradedToFarCaster", (object)name);
							break;
						case 4:
							str = string.Format("STRAT_020-01_{0}_FarCasterUpgradedToLense", (object)name);
							break;
						case 5:
							str = string.Format("STRAT_020-01_{0}_LenseUpgradedToMirrorOfCreation", (object)name);
							break;
					}
					break;
			}
			return str;
		}

		public int InsertStation(DesignInfo di, int orbitalObjectID, int playerID)
		{
			OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(orbitalObjectID);
			int parentOrbitalObjectID = 0;
			if (orbitalObjectID != 0)
				parentOrbitalObjectID = orbitalObjectID;
			OrbitalPath path = new OrbitalPath();
			path.Scale = new Vector2(10f, 10f);
			path.Rotation = new Vector3(0.0f, 0.0f, 0.0f);
			path.DeltaAngle = 10f;
			path.InitialAngle = 10f;
			string displayText = di.StationType.ToDisplayText(this.LocalPlayer.Faction.Name);
			string name = displayText + " Station";
			int num = this._db.InsertStation(parentOrbitalObjectID, orbitalObjectInfo.StarSystemID, path, name, playerID, di);
			GameSession.Trace("New " + displayText + " station constructed in System " + (object)orbitalObjectInfo.StarSystemID + ".");
			return num;
		}

		public static int InsertNewSalvageProject(App app, int playerid, int techid)
		{
			string projtype = app.GameDatabase.GetTechFileID(techid).Substring(0, 3);
			PlayerTechInfo playerTechInfo = app.GameDatabase.GetPlayerTechInfo(playerid, techid);
			return app.GameDatabase.InsertSpecialProject(playerid, app.Game.NamesPool.GetSalvageProjectName(projtype), (int)((double)playerTechInfo.ResearchCost * (double)new Random().NextInclusive(0.5f, 1.5f)), SpecialProjectType.Salvage, techid, 0, 0, 0);
		}

		private void CompleteSpecialProject(SpecialProjectInfo project)
		{
			if (project.Type == SpecialProjectType.Salvage)
			{
				this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_SALVAGE_PROJECT_COMPLETE,
					EventMessage = TurnEventMessage.EM_SALVAGE_PROJECT_COMPLETE,
					PlayerID = project.PlayerID,
					TechID = project.TechID,
					TurnNumber = this._app.GameDatabase.GetTurnCount(),
					SpecialProjectID = project.ID,
					ShowsDialog = true
				});
				PlayerTechInfo playerTechInfo = this._app.GameDatabase.GetPlayerTechInfo(project.PlayerID, project.TechID);
				if (playerTechInfo == null || playerTechInfo.State == TechStates.Researched || playerTechInfo.State == TechStates.Researching)
					return;
				playerTechInfo.PlayerFeasibility = 1f;
				playerTechInfo.Feasibility = 1f;
				playerTechInfo.TurnResearched = new int?(this.GameDatabase.GetTurnCount());
				this._app.GameDatabase.UpdatePlayerTechInfo(playerTechInfo);
				this._app.GameDatabase.UpdatePlayerTechState(project.PlayerID, playerTechInfo.TechID, TechStates.Core);
			}
			else if (project.Type == SpecialProjectType.AsteroidMonitor)
			{
				FleetInfo fleetInfo1 = this.App.GameDatabase.GetFleetInfo(project.FleetID);
				if (fleetInfo1 == null)
					return;
				fleetInfo1.PlayerID = project.PlayerID;
				this.App.GameDatabase.UpdateFleetInfo(fleetInfo1);
				FleetInfo defenseFleetInfo = this.App.GameDatabase.InsertOrGetDefenseFleetInfo(fleetInfo1.SystemID, project.PlayerID);
				foreach (int shipID in this.App.GameDatabase.GetShipsByFleetID(fleetInfo1.ID).ToList<int>())
				{
					if (shipID != 0)
					{
						Matrix? shipSystemPosition = this.App.GameDatabase.GetShipSystemPosition(shipID);
						this.App.GameDatabase.TransferShip(shipID, defenseFleetInfo.ID);
						this.App.GameDatabase.UpdateShipSystemPosition(shipID, shipSystemPosition);
					}
				}
				this.App.GameDatabase.RemoveFleet(fleetInfo1.ID);
				this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_MONITOR_PROJECT_COMPLETE,
					EventMessage = TurnEventMessage.EM_MONITOR_PROJECT_COMPLETE,
					PlayerID = project.PlayerID,
					SystemID = fleetInfo1.SystemID,
					TurnNumber = this._app.GameDatabase.GetTurnCount(),
					SpecialProjectID = project.ID,
					ShowsDialog = false
				});
				foreach (int standardPlayerId in this.App.GameDatabase.GetStandardPlayerIDs())
				{
					foreach (SpecialProjectInfo specialProjectInfo in this.App.GameDatabase.GetSpecialProjectInfosByPlayerID(standardPlayerId, true).ToList<SpecialProjectInfo>())
					{
						if (specialProjectInfo.FleetID == project.FleetID)
						{
							FleetInfo fleetInfo2 = this.GameDatabase.GetFleetInfo(project.FleetID);
							this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
							{
								EventType = TurnEventType.EV_MONITOR_CAPTURED,
								EventMessage = TurnEventMessage.EM_MONITOR_CAPTURED,
								PlayerID = project.PlayerID,
								SystemID = fleetInfo2.SystemID,
								TurnNumber = this._app.GameDatabase.GetTurnCount(),
								SpecialProjectID = project.ID,
								ShowsDialog = false
							});
							this.GameDatabase.RemoveSpecialProject(specialProjectInfo.ID);
						}
					}
				}
			}
			else if (project.Type == SpecialProjectType.IndependentStudy)
			{
				if (project.PlayerID != this._app.LocalPlayer.ID || this._app.GameDatabase.GetPlayerInfo(project.TargetPlayerID) == null)
					return;
				this.App.UI.CreateDialog((Dialog)new IndependentStudied(this._app, project.TargetPlayerID), null);
			}
			else if (project.Type == SpecialProjectType.RadiationShielding)
				this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_RADIATION_SHIELDING_PROJECT_COMPLETE,
					EventMessage = TurnEventMessage.EM_RADIATION_SHIELDING_PROJECT_COMPLETE,
					PlayerID = project.PlayerID,
					TurnNumber = this._app.GameDatabase.GetTurnCount(),
					SpecialProjectID = project.ID,
					ShowsDialog = false
				});
			else if (project.Type == SpecialProjectType.NeutronStar)
			{
				NeutronStarInfo neutronStarInfo = this._app.GameDatabase.GetNeutronStarInfo(project.EncounterID);
				if (neutronStarInfo != null)
				{
					this._app.GameDatabase.RemoveFleet(neutronStarInfo.FleetId);
					this._app.GameDatabase.RemoveEncounter(neutronStarInfo.Id);
				}
				this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_NEUTRONSTAR_PROJECT_COMPLETE,
					EventMessage = TurnEventMessage.EM_NEUTRONSTAR_PROJECT_COMPLETE,
					PlayerID = project.PlayerID,
					TurnNumber = this._app.GameDatabase.GetTurnCount(),
					SpecialProjectID = project.ID,
					ShowsDialog = false
				});
				foreach (int standardPlayerId in this.App.GameDatabase.GetStandardPlayerIDs())
				{
					foreach (SpecialProjectInfo specialProjectInfo in this.App.GameDatabase.GetSpecialProjectInfosByPlayerID(standardPlayerId, true).ToList<SpecialProjectInfo>())
					{
						if (specialProjectInfo.EncounterID == project.EncounterID)
						{
							this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
							{
								EventType = TurnEventType.EV_NEUTRONSTAR_DESTROYED,
								EventMessage = TurnEventMessage.EM_NEUTRONSTAR_DESTROYED,
								PlayerID = project.PlayerID,
								TurnNumber = this._app.GameDatabase.GetTurnCount(),
								SpecialProjectID = project.ID,
								ShowsDialog = false
							});
							this.GameDatabase.RemoveSpecialProject(specialProjectInfo.ID);
						}
					}
				}
				if (!neutronStarInfo.DeepSpaceSystemId.HasValue)
					return;
				foreach (StationInfo stationInfo in this._app.GameDatabase.GetStationForSystem(neutronStarInfo.DeepSpaceSystemId.Value).ToList<StationInfo>())
					this._app.GameDatabase.DestroyStation(this._app.Game, stationInfo.ID, 0);
				foreach (FleetInfo fleetInfo in this._app.GameDatabase.GetFleetInfoBySystemID(neutronStarInfo.DeepSpaceSystemId.Value, FleetType.FL_NORMAL).ToList<FleetInfo>())
					Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleetInfo, true);
				foreach (MissionInfo missionInfo in this._app.GameDatabase.GetMissionsBySystemDest(neutronStarInfo.DeepSpaceSystemId.Value).ToList<MissionInfo>())
				{
					FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(missionInfo.FleetID);
					if (fleetInfo != null)
						Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleetInfo, true);
					else
						this._app.GameDatabase.RemoveMission(missionInfo.ID);
				}
				this._app.GameDatabase.DestroyStarSystem(this, neutronStarInfo.DeepSpaceSystemId.Value);
				foreach (int playerID in this._db.GetStandardPlayerIDs().ToList<int>())
				{
					foreach (TurnEvent turnEvent in this._app.GameDatabase.GetTurnEventsByTurnNumber(this._app.GameDatabase.GetTurnCount(), playerID).Where<TurnEvent>((Func<TurnEvent, bool>)(x => x.EventType == TurnEventType.EV_NEUTRON_STAR_NEARBY)).ToList<TurnEvent>())
						this._app.GameDatabase.RemoveTurnEvent(turnEvent.ID);
				}
			}
			else
			{
				if (project.Type != SpecialProjectType.Gardener)
					return;
				if (this._app.GameDatabase.GetGardenerInfo(project.EncounterID) != null)
				{
					this._app.Game.ScriptModules.Gardeners.HandleGardenerCaptured(this._app.Game, this._app.GameDatabase, project.PlayerID, project.EncounterID);
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_GARDENER_PROJECT_COMPLETE,
						EventMessage = TurnEventMessage.EM_GARDENER_PROJECT_COMPLETE,
						PlayerID = project.PlayerID,
						TurnNumber = this._app.GameDatabase.GetTurnCount(),
						SpecialProjectID = project.ID,
						ShowsDialog = false
					});
				}
				foreach (int standardPlayerId in this.App.GameDatabase.GetStandardPlayerIDs())
				{
					foreach (SpecialProjectInfo specialProjectInfo in this.App.GameDatabase.GetSpecialProjectInfosByPlayerID(standardPlayerId, true).ToList<SpecialProjectInfo>())
					{
						if (specialProjectInfo.EncounterID == project.EncounterID)
						{
							this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
							{
								EventType = TurnEventType.EV_GARDENER_CAPTURED,
								EventMessage = TurnEventMessage.EM_GARDENER_CAPTURED,
								PlayerID = project.PlayerID,
								TurnNumber = this._app.GameDatabase.GetTurnCount(),
								SpecialProjectID = project.ID,
								ShowsDialog = false
							});
							this.GameDatabase.RemoveSpecialProject(specialProjectInfo.ID);
						}
					}
				}
			}
		}

		private int UpdateResearchProjects(int playerId, int availableResearchPoints)
		{
			int num1 = 0;
			PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(playerId);
			if (playerInfo != null)
			{
				foreach (ResearchProjectInfo projectInfo in this._db.GetResearchProjectInfos(playerId).ToList<ResearchProjectInfo>())
				{
					FeasibilityStudyInfo feasibilityStudyInfo = this._db.GetFeasibilityStudyInfo(projectInfo.ID);
					if (feasibilityStudyInfo != null)
						num1 += this.UpdateFeasibilityStudy(projectInfo, feasibilityStudyInfo, availableResearchPoints);
				}
				int num2 = (int)((double)playerInfo.RateResearchSalvageResearch * (double)availableResearchPoints);
				int num3 = num2;
				foreach (SpecialProjectInfo project in this._app.GameDatabase.GetSpecialProjectInfosByPlayerID(playerId, true).Where<SpecialProjectInfo>((Func<SpecialProjectInfo, bool>)(x =>
			   {
				   if (x.Type == SpecialProjectType.Salvage)
					   return x.Progress >= 0;
				   return false;
			   })))
				{
					if (num3 > 0)
					{
						int num4 = (int)((double)project.Rate * (double)num3);
						if (num3 - num4 < 0)
						{
							num4 = num3;
							num3 = 0;
						}
						else
							num3 -= num4;
						project.Progress += num4;
						if (project.Progress > project.Cost)
						{
							num3 += project.Progress - project.Cost;
							project.Progress = project.Cost;
						}
						this._app.GameDatabase.UpdateSpecialProjectProgress(project.ID, project.Progress);
						if (project.Progress == project.Cost)
							this.CompleteSpecialProject(project);
					}
					else
						break;
				}
				int num5 = num1 + (num2 - num3);
				int num6 = (int)((double)playerInfo.RateResearchSpecialProject * (double)availableResearchPoints);
				int num7 = num6;
				foreach (SpecialProjectInfo specialProjectInfo in this._app.GameDatabase.GetSpecialProjectInfosByPlayerID(playerId, true).Where<SpecialProjectInfo>((Func<SpecialProjectInfo, bool>)(x =>
			   {
				   if (x.Type != SpecialProjectType.Salvage)
					   return x.Progress >= 0;
				   return false;
			   })).ToList<SpecialProjectInfo>())
				{
					SpecialProjectInfo project = specialProjectInfo;
					if (num7 > 0)
					{
						int num4 = (int)((double)project.Rate * (double)num7);
						if (project.Type == SpecialProjectType.AsteroidMonitor)
							num4 = (int)((double)num4 * (double)this._db.GetStratModifierFloatToApply(StratModifiers.AsteroidMonitorResearchModifier, playerId));
						if (project.Type == SpecialProjectType.NeutronStar)
						{
							NeutronStarInfo neutronStarInfo = this._app.GameDatabase.GetNeutronStarInfo(project.EncounterID);
							if (neutronStarInfo == null || !neutronStarInfo.DeepSpaceSystemId.HasValue)
							{
								this._app.GameDatabase.RemoveSpecialProject(project.ID);
								continue;
							}
							List<StationInfo> list = this._app.GameDatabase.GetStationForSystem(neutronStarInfo.DeepSpaceSystemId.Value).ToList<StationInfo>();
							if (list.Count == 0 || !list.Any<StationInfo>((Func<StationInfo, bool>)(x => x.PlayerID == project.PlayerID)))
								continue;
						}
						else if (project.Type == SpecialProjectType.Gardener)
						{
							GardenerInfo gardenerInfo = this._app.GameDatabase.GetGardenerInfo(project.EncounterID);
							if (gardenerInfo == null || !gardenerInfo.DeepSpaceSystemId.HasValue)
							{
								this._app.GameDatabase.RemoveSpecialProject(project.ID);
								continue;
							}
							List<StationInfo> list = this._app.GameDatabase.GetStationForSystem(gardenerInfo.DeepSpaceSystemId.Value).ToList<StationInfo>();
							if (list.Count == 0 || !list.Any<StationInfo>((Func<StationInfo, bool>)(x => x.PlayerID == project.PlayerID)))
								continue;
						}
						if (num7 - num4 < 0)
						{
							num4 = num7;
							num7 = 0;
						}
						else
							num7 -= num4;
						project.Progress += num4;
						if (project.Progress > project.Cost)
						{
							num7 += project.Progress - project.Cost;
							project.Progress = project.Cost;
						}
						this._app.GameDatabase.UpdateSpecialProjectProgress(project.ID, project.Progress);
						if (project.Progress == project.Cost)
							this.CompleteSpecialProject(project);
					}
					else
						break;
				}
				num1 = num5 + (num6 - num7);
			}
			return num1;
		}

		private float CalcResearchOdds(PlayerTechInfo techInfo)
		{
			if ((double)techInfo.Feasibility <= 0.0)
				return 0.0f;
			int num1 = (int)(0.5 * (double)techInfo.ResearchCost);
			int num2 = (int)(1.5 * (double)techInfo.ResearchCost);
			if (techInfo.Progress < num1)
				return 0.0f;
			if (techInfo.Progress >= num2)
				return 1f;
			return (float)(techInfo.Progress - num1) / (float)num2;
		}

		public int ConvertToResearchPoints(int playerId, double credits)
		{
			return (int)(credits / 100.0 * (double)this.GetGeneralResearchModifier(playerId, false));
		}

		internal static double SplitResearchRevenue(PlayerInfo player, double availableRevenue)
		{
			if (availableRevenue < 0.0)
				return 0.0;
			return Math.Floor(availableRevenue * (1.0 - (double)player.RateGovernmentResearch) * (double)player.RateResearchCurrentProject);
		}

		internal static double SplitSpecialProjectRevenue(PlayerInfo player, double availableRevenue)
		{
			if (availableRevenue < 0.0)
				return 0.0;
			return Math.Floor(availableRevenue * (1.0 - (double)player.RateGovernmentResearch) * (double)player.RateResearchSpecialProject);
		}

		internal static double SplitSalvageProjectRevenue(PlayerInfo player, double availableRevenue)
		{
			if (availableRevenue < 0.0)
				return 0.0;
			return Math.Floor(availableRevenue * (1.0 - (double)player.RateGovernmentResearch) * (double)player.RateResearchSalvageResearch);
		}

		public int GetAvailableResearchPoints(PlayerInfo player)
		{
			double netRevenue = this.CalculateNetRevenue(player);
			double credits = GameSession.SplitResearchRevenue(player, Math.Max(netRevenue, 0.0));
			return this.ConvertToResearchPoints(player.ID, credits) + this.ConvertToResearchPoints(player.ID, player.ResearchBoostFunds) * 2;
		}

		private float UpdateResearch(int playerId, float availableCredits)
		{
			float num1 = 0.0f;
			PlayerInfo playerInfo = this._db.GetPlayerInfo(playerId);
			int num2 = (int)GameSession.SplitSalvageProjectRevenue(playerInfo, (double)availableCredits);
			int num3 = (int)GameSession.SplitSpecialProjectRevenue(playerInfo, (double)availableCredits);
			int num4 = (int)GameSession.SplitResearchRevenue(playerInfo, (double)availableCredits);
			if (num4 + num2 + num3 <= 0 && playerInfo.ResearchBoostFunds <= 0.0)
				return num1;
			int num5 = this.ConvertToResearchPoints(playerId, (double)num4) + this.ConvertToResearchPoints(playerId, (double)num3) + this.ConvertToResearchPoints(playerId, (double)num2);
			if (this.GetPlayerObject(playerId).IsAI())
				num5 = (int)((double)num5 * 1.0);
			int num6 = this.ConvertToResearchPoints(playerId, playerInfo.ResearchBoostFunds) * 2;
			this.App.GameDatabase.UpdatePlayerSavings(playerId, playerInfo.Savings - playerInfo.ResearchBoostFunds);
			int num7 = num5 + num6 + playerInfo.AdditionalResearchPoints;
			this._db.UpdatePlayerAdditionalResearchPoints(playerId, 0);
			int availableResearchPoints = (int)((double)num7 * ((double)this._db.GetNameValue<float>("ResearchEfficiency") / 100.0));
			int num8 = availableResearchPoints - this.UpdateResearchProjects(playerId, availableResearchPoints);
			if (num8 <= 0)
				return (float)num4 + num1;
			int researchingTechId = this._db.GetPlayerResearchingTechID(playerId);
			int feasibilityStudyTechId = this._db.GetPlayerFeasibilityStudyTechId(playerId);
			if (researchingTechId == 0 && feasibilityStudyTechId == 0)
			{
				if (!this._app.GameDatabase.GetTurnHasEventType(playerId, this._app.GameDatabase.GetTurnCount(), TurnEventType.EV_NO_RESEARCH))
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_NO_RESEARCH,
						EventMessage = TurnEventMessage.EM_NO_RESEARCH,
						PlayerID = playerId,
						TurnNumber = this._app.GameDatabase.GetTurnCount()
					});
				return num1;
			}
			if (this.DoBoostResearchAccidentRoll(playerId))
				return num1;
			PlayerTechInfo techInfo = this._db.GetPlayerTechInfo(playerId, researchingTechId);
			string techFamily = techInfo.TechFileID.Substring(0, 3);
			if (techInfo.TechFileID == "ENG_Leviathian_Construction")
				num8 = (int)((double)num8 * (double)this._db.GetStratModifierFloatToApply(StratModifiers.LeviathanResearchModifier, playerId));
			int totalModulesCounted;
			techInfo.Progress += (int)((double)num8 * (1.0 + (double)this.GetFamilySpecificResearchModifier(playerId, techFamily, out totalModulesCounted)));
			float num9 = num1 + (float)num4;
			bool flag1 = false;
			if (techInfo.Progress > techInfo.ResearchCost / 2)
			{
				float num10 = this.CalcResearchOdds(techInfo);
				if (this._random.CoinToss((double)num10 * (double)this.GameDatabase.GetStratModifierFloatToApply(StratModifiers.ResearchBreakthroughModifier, playerId)))
				{
					techInfo.State = TechStates.Researched;
					if ((double)num10 < 0.5)
						flag1 = true;
				}
			}
			if (techInfo.Progress > techInfo.ResearchCost * 2 && techInfo.State != TechStates.Researched)
			{
				techInfo.State = TechStates.Locked;
				this._db.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_RESEARCH_NEVER_COMPLETE,
					EventMessage = TurnEventMessage.EM_RESEARCH_NEVER_COMPLETE,
					PlayerID = playerInfo.ID,
					TechID = techInfo.TechID,
					TurnNumber = this._app.GameDatabase.GetTurnCount(),
					ShowsDialog = false
				});
			}
			if (techInfo.State == TechStates.Researched)
				techInfo.TurnResearched = new int?(this.GameDatabase.GetTurnCount());
			this._db.UpdatePlayerTechInfo(techInfo);
			if (techInfo.State == TechStates.Researched)
			{
				this._db.UpdateLockedTechs(this._app.AssetDatabase, playerId);
				if (this.GetPlayerObject(playerInfo.ID).IsAI())
				{
					Kerberos.Sots.Data.TechnologyFramework.Tech tech = this._app.AssetDatabase.MasterTechTree.Technologies.First<Kerberos.Sots.Data.TechnologyFramework.Tech>((Func<Kerberos.Sots.Data.TechnologyFramework.Tech, bool>)(x => x.Id == techInfo.TechFileID));
					AITechWeightInfo aiTechWeightInfo = this._db.GetAITechWeightInfo(playerInfo.ID, tech.Family);
					if (aiTechWeightInfo != null)
					{
						aiTechWeightInfo.TotalSpent += (double)techInfo.ResearchCost;
						this._db.UpdateAITechWeight(aiTechWeightInfo);
					}
				}
				string str = !flag1 ? string.Format("STRAT_047-01_{0}_ResearchComplete", (object)this.GameDatabase.GetFactionName(this.GameDatabase.GetPlayerFactionID(playerInfo.ID))) : string.Format("STRAT_051-01_{0}_ResearchCompletedEarly", (object)this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID)));
				this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_RESEARCH_COMPLETE,
					EventMessage = TurnEventMessage.EM_RESEARCH_COMPLETE,
					EventSoundCueName = str,
					PlayerID = playerInfo.ID,
					TechID = techInfo.TechID,
					TurnNumber = this._app.GameDatabase.GetTurnCount(),
					ShowsDialog = true
				});
				App.UpdateStratModifiers(this, playerInfo.ID, techInfo.TechID);
				if (playerInfo.ID == this.LocalPlayer.ID)
				{
					if (techInfo.TechFileID == "BIO_Xombie_Plague")
						this.App.SteamHelper.DoAchievement(AchievementType.SOTS2_ZWORD);
					bool flag2 = true;
					foreach (PlayerTechInfo playerTechInfo in this._db.GetPlayerTechInfos(playerId))
					{
						if (playerTechInfo.State != TechStates.Researched)
							flag2 = false;
					}
					if (flag2)
						this.App.SteamHelper.DoAchievement(AchievementType.SOTS2_DR_SLEEPLESS);
				}
			}
			return num9;
		}

		private int UpdateFeasibilityStudy(
		  ResearchProjectInfo projectInfo,
		  FeasibilityStudyInfo feasibilityInfo,
		  int availableResearchPoints)
		{
			int num1 = 0;
			PlayerTechInfo playerTechInfo = this._db.GetPlayerTechInfo(projectInfo.PlayerID, feasibilityInfo.TechID);
			float num2 = (float)availableResearchPoints / feasibilityInfo.ResearchCost;
			int num3;
			if ((double)num2 + (double)projectInfo.Progress > 1.0)
			{
				num3 = num1 + (int)((1.0 - (double)projectInfo.Progress) * (double)feasibilityInfo.ResearchCost);
				float stratModifier = this._db.GetStratModifier<float>(StratModifiers.TechFeasibilityDeviation, projectInfo.PlayerID);
				playerTechInfo.PlayerFeasibility = Math.Min(Math.Max((float)((double)playerTechInfo.Feasibility + (double)this._random.NextSingle() % (double)stratModifier - (double)stratModifier / 2.0), 0.01f), 0.99f);
				playerTechInfo.Progress += (int)((double)feasibilityInfo.ResearchCost / 2.0);
				this._db.UpdatePlayerTechInfo(playerTechInfo);
				string factionName = this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID));
				string str = (double)playerTechInfo.PlayerFeasibility >= 0.300000011920929 ? ((double)playerTechInfo.PlayerFeasibility <= 0.800000011920929 ? string.Format("STRAT_049-01_{0}_FeasibilityStudyComplete-31-79Percent", (object)factionName) : string.Format("STRAT_048-01_{0}_FeasibilityStudyComplete-Over80Percent", (object)factionName)) : string.Format("STRAT_050-01_{0}_FeasibilityStudyComplete-Less30Percent", (object)factionName);
				this._db.UpdatePlayerTechState(projectInfo.PlayerID, feasibilityInfo.TechID, (double)playerTechInfo.PlayerFeasibility >= 0.5 ? TechStates.HighFeasibility : TechStates.LowFeasibility);
				this._db.RemoveFeasibilityStudy(projectInfo.ID);
				int num4 = (int)Math.Round((double)playerTechInfo.PlayerFeasibility * 100.0);
				TurnEventMessage turnEventMessage = num4 >= 26 ? (num4 >= 51 ? (num4 >= 81 ? TurnEventMessage.EM_FEASIBILITY_STUDY_COMPLETE_VERY_GOOD : TurnEventMessage.EM_FEASIBILITY_STUDY_COMPLETE_GOOD) : TurnEventMessage.EM_FEASIBILITY_STUDY_COMPLETE_BAD) : TurnEventMessage.EM_FEASIBILITY_STUDY_COMPLETE_VERY_BAD;
				this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_FEASIBILITY_STUDY_COMPLETE,
					EventMessage = turnEventMessage,
					EventSoundCueName = str,
					PlayerID = projectInfo.PlayerID,
					FeasibilityPercent = num4,
					TechID = playerTechInfo.TechID,
					TurnNumber = this._app.GameDatabase.GetTurnCount(),
					ShowsDialog = true
				});
			}
			else
			{
				projectInfo.Progress += num2;
				num3 = availableResearchPoints;
				this._db.UpdateResearchProjectInfo(projectInfo);
			}
			return num3;
		}

		public static Dictionary<ModuleEnums.StationModuleType, int> CountStationModuleTypes(
		  StationInfo station,
		  GameDatabase gamedb,
		  bool groupModules = true)
		{
			string factionName = gamedb.GetFactionName(gamedb.GetPlayerFactionID(station.PlayerID));
			Dictionary<ModuleEnums.StationModuleType, int> dictionary1 = new Dictionary<ModuleEnums.StationModuleType, int>();
			foreach (DesignModuleInfo module in station.DesignInfo.DesignSections[0].Modules)
			{
				if (groupModules)
				{
					if (ModuleEnums.LabTypes.Contains(module.StationModuleType.Value))
					{
						if (dictionary1.ContainsKey(ModuleEnums.StationModuleType.Lab))
						{
							Dictionary<ModuleEnums.StationModuleType, int> dictionary2;
							(dictionary2 = dictionary1)[ModuleEnums.StationModuleType.Lab] = dictionary2[ModuleEnums.StationModuleType.Lab] + 1;
						}
						else
							dictionary1.Add(ModuleEnums.StationModuleType.Lab, 1);
					}
					else if (ModuleEnums.HabitationModuleTypes.Contains(module.StationModuleType.Value))
					{
						ModuleEnums.StationModuleType habitationModule = ModuleEnums.FactionHabitationModules[factionName];
						ModuleEnums.StationModuleType? stationModuleType = module.StationModuleType;
						ModuleEnums.StationModuleType key = (habitationModule != stationModuleType.GetValueOrDefault() ? 0 : (stationModuleType.HasValue ? 1 : 0)) != 0 ? ModuleEnums.StationModuleType.Habitation : ModuleEnums.StationModuleType.AlienHabitation;
						if (dictionary1.ContainsKey(key))
						{
							Dictionary<ModuleEnums.StationModuleType, int> dictionary2;
							ModuleEnums.StationModuleType index;
							(dictionary2 = dictionary1)[index = key] = dictionary2[index] + 1;
						}
						else
							dictionary1.Add(key, 1);
					}
					else if (ModuleEnums.TradeModuleTypes.Contains(module.StationModuleType.Value))
					{
						ModuleEnums.StationModuleType key = ModuleEnums.StationModuleType.Trade;
						if (dictionary1.ContainsKey(key))
						{
							Dictionary<ModuleEnums.StationModuleType, int> dictionary2;
							ModuleEnums.StationModuleType index;
							(dictionary2 = dictionary1)[index = key] = dictionary2[index] + 1;
						}
						else
							dictionary1.Add(key, 1);
					}
					else if (ModuleEnums.LargeHabitationModuleTypes.Contains(module.StationModuleType.Value))
					{
						ModuleEnums.StationModuleType habitationModule = ModuleEnums.FactionLargeHabitationModules[factionName];
						ModuleEnums.StationModuleType? stationModuleType = module.StationModuleType;
						ModuleEnums.StationModuleType key = (habitationModule != stationModuleType.GetValueOrDefault() ? 0 : (stationModuleType.HasValue ? 1 : 0)) != 0 ? ModuleEnums.StationModuleType.LargeHabitation : ModuleEnums.StationModuleType.LargeAlienHabitation;
						if (dictionary1.ContainsKey(key))
						{
							Dictionary<ModuleEnums.StationModuleType, int> dictionary2;
							ModuleEnums.StationModuleType index;
							(dictionary2 = dictionary1)[index = key] = dictionary2[index] + 1;
						}
						else
							dictionary1.Add(key, 1);
					}
					else if (dictionary1.ContainsKey(module.StationModuleType.Value))
					{
						Dictionary<ModuleEnums.StationModuleType, int> dictionary2;
						ModuleEnums.StationModuleType index;
						(dictionary2 = dictionary1)[index = module.StationModuleType.Value] = dictionary2[index] + 1;
					}
					else
						dictionary1.Add(module.StationModuleType.Value, 1);
				}
				else if (dictionary1.ContainsKey(module.StationModuleType.Value))
				{
					Dictionary<ModuleEnums.StationModuleType, int> dictionary2;
					ModuleEnums.StationModuleType index;
					(dictionary2 = dictionary1)[index = module.StationModuleType.Value] = dictionary2[index] + 1;
				}
				else
					dictionary1.Add(module.StationModuleType.Value, 1);
			}
			return dictionary1;
		}

		private float CalculateStationUpgradeProgress(
		  StationInfo station,
		  Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>> typeRequirements,
		  out Dictionary<ModuleEnums.StationModuleType, int> requiredModules)
		{
			Dictionary<ModuleEnums.StationModuleType, int> dictionary1 = GameSession.CountStationModuleTypes(station, this.GameDatabase, true);
			int num1 = 0;
			Dictionary<ModuleEnums.StationModuleType, int> source = (Dictionary<ModuleEnums.StationModuleType, int>)null;
			foreach (KeyValuePair<int, Dictionary<ModuleEnums.StationModuleType, int>> typeRequirement in typeRequirements)
			{
				if (typeRequirement.Key == station.DesignInfo.StationLevel)
				{
					source = typeRequirement.Value;
					num1 = source.Sum<KeyValuePair<ModuleEnums.StationModuleType, int>>((Func<KeyValuePair<ModuleEnums.StationModuleType, int>, int>)(x => x.Value));
					break;
				}
				foreach (KeyValuePair<ModuleEnums.StationModuleType, int> keyValuePair in typeRequirement.Value)
				{
					if (dictionary1.ContainsKey(keyValuePair.Key))
						dictionary1[keyValuePair.Key] = Math.Max(0, dictionary1[keyValuePair.Key] - keyValuePair.Value);
				}
			}
			if (source == null)
			{
				requiredModules = new Dictionary<ModuleEnums.StationModuleType, int>();
				return 0.0f;
			}
			requiredModules = new Dictionary<ModuleEnums.StationModuleType, int>((IDictionary<ModuleEnums.StationModuleType, int>)source);
			int num2 = 0;
			foreach (KeyValuePair<ModuleEnums.StationModuleType, int> keyValuePair in dictionary1)
			{
				if (source.ContainsKey(keyValuePair.Key))
				{
					int num3 = Math.Min(keyValuePair.Value, source[keyValuePair.Key]);
					Dictionary<ModuleEnums.StationModuleType, int> dictionary2;
					ModuleEnums.StationModuleType key;
					(dictionary2 = requiredModules)[key = keyValuePair.Key] = dictionary2[key] - num3;
					num2 += num3;
				}
			}
			foreach (KeyValuePair<ModuleEnums.StationModuleType, int> keyValuePair in new Dictionary<ModuleEnums.StationModuleType, int>((IDictionary<ModuleEnums.StationModuleType, int>)requiredModules))
			{
				if (keyValuePair.Value == 0)
					requiredModules.Remove(keyValuePair.Key);
			}
			return (float)num2 / (float)num1;
		}

		public List<LogicalModuleMount> GetAvailableStationModuleMounts(
		  StationInfo si)
		{
			List<LogicalModuleMount> stationModuleMounts = this.GetStationModuleMounts(si);
			foreach (DesignModuleInfo module in si.DesignInfo.DesignSections[0].Modules)
			{
				DesignModuleInfo dmi = module;
				LogicalModuleMount logicalModuleMount = stationModuleMounts.FirstOrDefault<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => x.NodeName == dmi.MountNodeName));
				if (logicalModuleMount != null)
					stationModuleMounts.Remove(logicalModuleMount);
			}
			return stationModuleMounts;
		}

		public List<LogicalModuleMount> GetStationModuleMounts(StationInfo si)
		{
			return new List<LogicalModuleMount>((IEnumerable<LogicalModuleMount>)this.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == si.DesignInfo.DesignSections[0].FilePath)).Modules);
		}

		public float GetStationUpgradeProgress(
		  StationInfo station,
		  out Dictionary<ModuleEnums.StationModuleType, int> requiredModules)
		{
			switch (station.DesignInfo.StationType)
			{
				case StationType.NAVAL:
					return this.CalculateStationUpgradeProgress(station, this.AssetDatabase.NavalStationUpgradeRequirements, out requiredModules);
				case StationType.SCIENCE:
					return this.CalculateStationUpgradeProgress(station, this.AssetDatabase.ScienceStationUpgradeRequirements, out requiredModules);
				case StationType.CIVILIAN:
					return this.CalculateStationUpgradeProgress(station, this.AssetDatabase.CivilianStationUpgradeRequirements, out requiredModules);
				case StationType.DIPLOMATIC:
					return this.CalculateStationUpgradeProgress(station, this.AssetDatabase.DiplomaticStationUpgradeRequirements, out requiredModules);
				case StationType.GATE:
					return this.CalculateStationUpgradeProgress(station, this.AssetDatabase.GateStationUpgradeRequirements, out requiredModules);
				default:
					requiredModules = new Dictionary<ModuleEnums.StationModuleType, int>();
					return 0.0f;
			}
		}

		public static int CalculateLVL1StationUpkeepCost(AssetDatabase assetdb, StationType stationType)
		{
			switch (stationType)
			{
				case StationType.NAVAL:
					return assetdb.UpkeepNavalStation[0];
				case StationType.SCIENCE:
					return assetdb.UpkeepScienceStation[0];
				case StationType.CIVILIAN:
					return assetdb.UpkeepCivilianStation[0];
				case StationType.DIPLOMATIC:
					return assetdb.UpkeepDiplomaticStation[0];
				case StationType.GATE:
					return assetdb.UpkeepGateStation[0];
				default:
					return 0;
			}
		}

		public static int CalculateStationUpkeepCost(
		  GameDatabase gamedb,
		  AssetDatabase assetdb,
		  StationInfo si)
		{
			int num1 = 0;
			foreach (DesignModuleInfo module in si.DesignInfo.DesignSections[0].Modules)
			{
				string moduleAsset = gamedb.GetModuleAsset(module.ModuleID);
				LogicalModule logicalModule = assetdb.Modules.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == moduleAsset));
				if (logicalModule != null)
					num1 += logicalModule.UpkeepCost;
			}
			int index = si.DesignInfo.StationLevel - 1;
			if (index >= 0)
			{
				switch (si.DesignInfo.StationType)
				{
					case StationType.NAVAL:
						return num1 + assetdb.UpkeepNavalStation[index];
					case StationType.SCIENCE:
						int num2 = num1 + assetdb.UpkeepScienceStation[index];
						float num3 = 0.0f;
						foreach (DesignModuleInfo module in si.DesignInfo.DesignSections[0].Modules)
						{
							ModuleEnums.StationModuleType? stationModuleType = module.StationModuleType;
							if ((stationModuleType.GetValueOrDefault() != ModuleEnums.StationModuleType.Dock ? 0 : (stationModuleType.HasValue ? 1 : 0)) != 0)
								num3 += (float)num2 * 0.02f;
						}
						return num2 - (int)num3;
					case StationType.CIVILIAN:
						return num1 + assetdb.UpkeepCivilianStation[index];
					case StationType.DIPLOMATIC:
						int num4 = num1 + assetdb.UpkeepDiplomaticStation[index];
						float num5 = 0.0f;
						foreach (DesignModuleInfo module in si.DesignInfo.DesignSections[0].Modules)
						{
							ModuleEnums.StationModuleType? stationModuleType = module.StationModuleType;
							if ((stationModuleType.GetValueOrDefault() != ModuleEnums.StationModuleType.Dock ? 0 : (stationModuleType.HasValue ? 1 : 0)) != 0)
								num5 += (float)num4 * 0.02f;
						}
						return num4 - (int)num5;
					case StationType.GATE:
						int num6 = num1 + assetdb.UpkeepGateStation[index];
						float num7 = 0.0f;
						foreach (DesignModuleInfo module in si.DesignInfo.DesignSections[0].Modules)
						{
							ModuleEnums.StationModuleType? stationModuleType = module.StationModuleType;
							if ((stationModuleType.GetValueOrDefault() != ModuleEnums.StationModuleType.Dock ? 0 : (stationModuleType.HasValue ? 1 : 0)) != 0)
								num7 += (float)num6 * 0.02f;
						}
						return num6 - (int)num7;
				}
			}
			return 0;
		}

		public static int CalculateUpkeepCost(int DesignId, App game)
		{
			switch (game.GameDatabase.GetDesignInfo(DesignId).Class)
			{
				case ShipClass.Cruiser:
					return game.AssetDatabase.UpkeepCruiser;
				case ShipClass.Dreadnought:
					return game.AssetDatabase.UpkeepDreadnaught;
				case ShipClass.Leviathan:
					return game.AssetDatabase.UpkeepLeviathan;
				case ShipClass.BattleRider:
					return game.AssetDatabase.UpkeepBattleRider;
				default:
					return 0;
			}
		}

		public static double CalculateStationUpkeepCosts(
		  GameDatabase gamedb,
		  AssetDatabase assetdb,
		  IEnumerable<StationInfo> stationInfos)
		{
			return (double)stationInfos.Sum<StationInfo>((Func<StationInfo, int>)(x => GameSession.CalculateStationUpkeepCost(gamedb, assetdb, x)));
		}

		public static double CalculateShipUpkeepCost(
		  AssetDatabase assetdb,
		  DesignInfo shipDesignInfo,
		  float scale,
		  bool isInReserveFleet)
		{
			double num = 0.0;
			switch (shipDesignInfo.Class)
			{
				case ShipClass.Cruiser:
					num = (double)assetdb.UpkeepCruiser;
					break;
				case ShipClass.Dreadnought:
					num = (double)assetdb.UpkeepDreadnaught;
					break;
				case ShipClass.Leviathan:
					num = (double)assetdb.UpkeepLeviathan;
					break;
				case ShipClass.BattleRider:
					num = (double)assetdb.UpkeepBattleRider;
					break;
				case ShipClass.Station:
					if (shipDesignInfo.StationType == StationType.INVALID_TYPE)
					{
						num = (double)assetdb.UpkeepDefensePlatform;
						break;
					}
					break;
			}
			if (isInReserveFleet)
				num /= 3.0;
			return num * (double)scale;
		}

		public static double CalculateShipUpkeepCosts(
		  AssetDatabase assetdb,
		  IEnumerable<DesignInfo> shipDesignInfos,
		  float scale,
		  bool areInReserveFleets)
		{
			if (shipDesignInfos == null)
				return 0.0;
			return shipDesignInfos.Sum<DesignInfo>((Func<DesignInfo, double>)(x => GameSession.CalculateShipUpkeepCost(assetdb, x, scale, areInReserveFleets)));
		}

		public static List<DesignInfo> MergeShipDesignInfos(
		  GameDatabase db,
		  IEnumerable<FleetInfo> allFleetInfos,
		  bool reserveFleets)
		{
			return allFleetInfos.Where<FleetInfo>((Func<FleetInfo, bool>)(x => x.IsReserveFleet == reserveFleets)).SelectMany<FleetInfo, ShipInfo>((Func<FleetInfo, IEnumerable<ShipInfo>>)(x => db.GetShipInfoByFleetID(x.ID, false))).Select<ShipInfo, DesignInfo>((Func<ShipInfo, DesignInfo>)(x => db.GetDesignInfo(x.DesignID))).ToList<DesignInfo>();
		}

		public static double CalculateFleetUpkeepCosts(
		  AssetDatabase assetdb,
		  IEnumerable<DesignInfo> reserveShipDesignInfos,
		  IEnumerable<DesignInfo> shipDesignInfos,
		  IEnumerable<DesignInfo> eliteShipDesignInfos)
		{
			return GameSession.CalculateShipUpkeepCosts(assetdb, eliteShipDesignInfos, assetdb.EliteUpkeepCostScale, false) + GameSession.CalculateShipUpkeepCosts(assetdb, shipDesignInfos, 1f, false) + GameSession.CalculateShipUpkeepCosts(assetdb, reserveShipDesignInfos, 1f, true);
		}

		public static double CalculateUpkeepCosts(AssetDatabase assetdb, GameDatabase db, int playerId)
		{
			List<StationInfo> list1 = db.GetStationInfosByPlayerID(playerId).ToList<StationInfo>();
			List<FleetInfo> list2 = db.GetFleetInfosByPlayerID(playerId, FleetType.FL_ALL).ToList<FleetInfo>();
			List<FleetInfo> eliteFleetInfos = list2.Where<FleetInfo>((Func<FleetInfo, bool>)(x => db.GetAdmiralTraits(x.AdmiralID).Contains<AdmiralInfo.TraitType>(AdmiralInfo.TraitType.Elite))).ToList<FleetInfo>();
			if (eliteFleetInfos.Count > 0)
				list2.RemoveAll((Predicate<FleetInfo>)(x => eliteFleetInfos.Contains(x)));
			List<DesignInfo> designInfoList1 = GameSession.MergeShipDesignInfos(db, (IEnumerable<FleetInfo>)list2, true);
			List<DesignInfo> designInfoList2 = GameSession.MergeShipDesignInfos(db, (IEnumerable<FleetInfo>)list2, false);
			List<DesignInfo> designInfoList3 = GameSession.MergeShipDesignInfos(db, (IEnumerable<FleetInfo>)eliteFleetInfos, false);
			return GameSession.CalculateFleetUpkeepCosts(assetdb, (IEnumerable<DesignInfo>)designInfoList1, (IEnumerable<DesignInfo>)designInfoList2, (IEnumerable<DesignInfo>)designInfoList3) + GameSession.CalculateStationUpkeepCosts(db, assetdb, (IEnumerable<StationInfo>)list1);
		}

		public static double CalculateDebtInterest(GameSession game, PlayerInfo ply)
		{
			if (game.AssetDatabase.GetFaction(ply.FactionID).Name == "loa")
				return 0.0;
			double num = 0.15;
			Kerberos.Sots.PlayerFramework.Player playerObject = game.GetPlayerObject(ply.ID);
			if (playerObject != null && playerObject.IsAI())
				num = 0.05;
			return Math.Max(-ply.Savings, 0.0) * num;
		}

		public static double CalculateSavingsInterest(GameSession game, PlayerInfo ply)
		{
			if (game.AssetDatabase.GetFaction(ply.FactionID).Name == "loa")
				return 0.0;
			return Math.Max(ply.Savings, 0.0) * 0.01;
		}

		private void CollectDiplomacyPoints()
		{
			foreach (PlayerInfo playerInfo in this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				PlayerInfo p = playerInfo;
				List<ProvinceInfo> list = this.GameDatabase.GetProvinceInfos().Where<ProvinceInfo>((Func<ProvinceInfo, bool>)(x => x.PlayerID == p.ID)).ToList<ProvinceInfo>();
				int newNumPoints = p.GenericDiplomacyPoints + list.Count * this.AssetDatabase.DiplomacyPointsPerProvince;
				Dictionary<int, int> factionDiplomacyPoints = this.App.GameDatabase.GetFactionDiplomacyPoints(p.ID);
				foreach (StationInfo stationInfo in this.GameDatabase.GetStationInfosByPlayerID(p.ID).Where<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == StationType.DIPLOMATIC)).ToList<StationInfo>())
				{
					if (stationInfo.DesignInfo.StationLevel > 0)
					{
						newNumPoints += this.AssetDatabase.DiplomacyPointsPerStation[stationInfo.DesignInfo.StationLevel - 1];
						foreach (DesignModuleInfo designModuleInfo in stationInfo.DesignInfo.DesignSections[0].Modules.ToList<DesignModuleInfo>())
						{
							if (stationInfo.DesignInfo.StationType == StationType.DIPLOMATIC)
							{
								if (AssetDatabase.StationModuleTypeToMountTypeMap[designModuleInfo.StationModuleType.Value] == ModuleEnums.ModuleSlotTypes.Habitation || AssetDatabase.StationModuleTypeToMountTypeMap[designModuleInfo.StationModuleType.Value] == ModuleEnums.ModuleSlotTypes.AlienHabitation)
								{
									ModuleEnums.StationModuleType? stationModuleType = designModuleInfo.StationModuleType;
									ModuleEnums.StationModuleType habitationModule = ModuleEnums.FactionHabitationModules[this.App.GameDatabase.GetFactionName(p.FactionID)];
									if ((stationModuleType.GetValueOrDefault() != habitationModule ? 0 : (stationModuleType.HasValue ? 1 : 0)) != 0)
									{
										++newNumPoints;
									}
									else
									{
										ModuleEnums.StationModuleType smt = (ModuleEnums.StationModuleType)Enum.Parse(typeof(ModuleEnums.StationModuleType), designModuleInfo.StationModuleType.ToString().Replace("Foreign", ""));
										Dictionary<int, int> dictionary1;
										Dictionary<int, int> dictionary2 = dictionary1 = factionDiplomacyPoints;
										GameDatabase gameDatabase = this.App.GameDatabase;
										string key = ModuleEnums.FactionHabitationModules.First<KeyValuePair<string, ModuleEnums.StationModuleType>>((Func<KeyValuePair<string, ModuleEnums.StationModuleType>, bool>)(x => x.Value == smt)).Key;
										int factionIdFromName;
										int index = factionIdFromName = gameDatabase.GetFactionIdFromName(key);
										int num = dictionary2[index] + 1;
										dictionary1[factionIdFromName] = num;
									}
								}
								else if (AssetDatabase.StationModuleTypeToMountTypeMap[designModuleInfo.StationModuleType.Value] == ModuleEnums.ModuleSlotTypes.LargeHabitation || AssetDatabase.StationModuleTypeToMountTypeMap[designModuleInfo.StationModuleType.Value] == ModuleEnums.ModuleSlotTypes.LargeAlienHabitation)
								{
									ModuleEnums.StationModuleType? stationModuleType = designModuleInfo.StationModuleType;
									ModuleEnums.StationModuleType habitationModule = ModuleEnums.FactionLargeHabitationModules[this.App.GameDatabase.GetFactionName(p.FactionID)];
									if ((stationModuleType.GetValueOrDefault() != habitationModule ? 0 : (stationModuleType.HasValue ? 1 : 0)) != 0)
									{
										newNumPoints += 3;
									}
									else
									{
										ModuleEnums.StationModuleType smt = (ModuleEnums.StationModuleType)Enum.Parse(typeof(ModuleEnums.StationModuleType), designModuleInfo.StationModuleType.ToString().Replace("Foreign", ""));
										Dictionary<int, int> dictionary1;
										Dictionary<int, int> dictionary2 = dictionary1 = factionDiplomacyPoints;
										GameDatabase gameDatabase = this.App.GameDatabase;
										string key = ModuleEnums.FactionLargeHabitationModules.First<KeyValuePair<string, ModuleEnums.StationModuleType>>((Func<KeyValuePair<string, ModuleEnums.StationModuleType>, bool>)(x => x.Value == smt)).Key;
										int factionIdFromName;
										int index = factionIdFromName = gameDatabase.GetFactionIdFromName(key);
										int num = dictionary2[index] + 3;
										dictionary1[factionIdFromName] = num;
									}
								}
							}
						}
					}
				}
				foreach (KeyValuePair<int, int> keyValuePair in factionDiplomacyPoints)
					this._db.UpdateFactionDiplomacyPoints(p.ID, keyValuePair.Key, keyValuePair.Value);
				this._db.UpdateGenericDiplomacyPoints(p.ID, newNumPoints);
			}
		}

		internal double CalculateNetRevenue(PlayerInfo player)
		{
			double tradeRevenue = 0.0;
			this._incomeFromTrade.TryGetValue(player.ID, out tradeRevenue);
			return new GameSession.NetRevenueSummary(this._app, player.ID, tradeRevenue).GetNetRevenue();
		}

		private void CollectTaxes()
		{
			foreach (PlayerInfo playerInfo1 in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				if (!playerInfo1.isDefeated)
				{
					if ((double)playerInfo1.RateTaxPrev != (double)playerInfo1.RateTax)
					{
						int num = (int)Math.Floor(((double)playerInfo1.RateTaxPrev - (double)playerInfo1.RateTax) * 100.0 + 0.5);
						if (num != 0)
							this.App.GameDatabase.InsertGovernmentAction(this.App.LocalPlayer.ID, num > 0 ? App.Localize("@GA_TAXDECREASED") : App.Localize("@GA_TAXINCREASED"), "", -num, -num);
						playerInfo1.RateTaxPrev = playerInfo1.RateTax;
						this._db.UpdatePreviousTaxRate(playerInfo1.ID, playerInfo1.RateTaxPrev);
					}
					double netRevenue = this.CalculateNetRevenue(playerInfo1);
					double debtInterest = GameSession.CalculateDebtInterest(this, playerInfo1);
					int playerBankruptcyTurns = this._db.GetPlayerBankruptcyTurns(playerInfo1.ID);
					if (playerInfo1.Savings < -5000000.0 && netRevenue + debtInterest < debtInterest)
					{
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_BANKRUPTCY_IMMINENT,
							EventMessage = TurnEventMessage.EM_BANKRUPTCY_IMMINENT,
							PlayerID = playerInfo1.ID,
							TurnNumber = this.App.GameDatabase.GetTurnCount()
						});
						this._db.UpdatePlayerBankruptcyTurns(playerInfo1.ID, playerBankruptcyTurns + 1);
						GameSession.SimAITurns = 0;
					}
					else if (playerBankruptcyTurns != 0)
					{
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_BANKRUPTCY_AVOIDED,
							EventMessage = TurnEventMessage.EM_BANKRUPTCY_AVOIDED,
							PlayerID = playerInfo1.ID,
							TurnNumber = this.App.GameDatabase.GetTurnCount()
						});
						this._db.UpdatePlayerBankruptcyTurns(playerInfo1.ID, 0);
					}
					if (netRevenue > 0.0 || playerInfo1.ResearchBoostFunds > 0.0 && (!(this.AssetDatabase.GetFaction(playerInfo1.FactionID).Name == "loa") || playerInfo1.Savings >= 0.0))
					{
						double revenue = netRevenue - (double)this.UpdateResearch(playerInfo1.ID, (float)netRevenue);
						playerInfo1.Savings = this.GameDatabase.GetPlayerInfo(playerInfo1.ID).Savings;
						if (revenue > 0.0)
							this.UpdateGovernmentSpending(playerInfo1, revenue);
						else
							this.UpdateSavingsSpending(playerInfo1, revenue);
					}
					else
						this.UpdateSavingsSpending(playerInfo1, netRevenue);
					this.GameDatabase.UpdatePlayerResearchBoost(playerInfo1.ID, 0.0);
					PlayerInfo playerInfo2 = this._db.GetPlayerInfo(playerInfo1.ID);
					Budget budget = Budget.GenerateBudget(this.App.Game, playerInfo2, (IEnumerable<DesignInfo>)null, BudgetProjection.Pessimistic);
					if (this._db.GetSliderNotchSettingInfo(playerInfo2.ID, UISlidertype.SecuritySlider) != null)
						EmpireSummaryState.DistributeGovernmentSpending(this.App.Game, EmpireSummaryState.GovernmentSpendings.Security, (float)Math.Min((double)budget.RequiredSecurity / 100.0, 1.0), playerInfo2);
				}
			}
		}

		private void UpdateConsumableShipStats()
		{
			foreach (PlayerInfo playerInfo in this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				List<ColonyInfo> list = this._db.GetPlayerColoniesByPlayerId(playerInfo.ID).ToList<ColonyInfo>();
				float num1 = 0.0f;
				double num2 = 0.0;
				foreach (ColonyInfo colonyInfo in list)
				{
					PlanetInfo planetInfo = this._db.GetPlanetInfo(colonyInfo.OrbitalObjectID);
					num1 += (float)planetInfo.Biosphere;
					num2 += colonyInfo.ImperialPop * (double)this.AssetDatabase.GetFaction(playerInfo.FactionID).PsionicPowerModifier;
					foreach (ColonyFactionInfo faction in colonyInfo.Factions)
						num2 += faction.CivilianPop * (double)this.AssetDatabase.GetFaction(faction.FactionID).PsionicPowerModifier;
				}
				int newPotential = (int)(((double)num1 / 10.0 + num2 / 10000000.0) * (double)this.GameDatabase.GetStratModifierFloatToApply(StratModifiers.PsiPotentialModifier, playerInfo.ID));
				this._db.UpdatePsionicPotential(playerInfo.ID, newPotential);
			}
			foreach (ShipInfo shipInfo in this._db.GetShipInfos(true).ToList<ShipInfo>())
			{
				ShipInfo si = shipInfo;
				if (si.DesignInfo.PlayerID != 0)
				{
					FleetInfo fleetInfo = this._db.GetFleetInfo(si.FleetID);
					List<AdmiralInfo.TraitType> admiralTraits = fleetInfo != null ? this._db.GetAdmiralTraits(fleetInfo.AdmiralID).ToList<AdmiralInfo.TraitType>() : new List<AdmiralInfo.TraitType>();
					int maxPsionicPower = ShipInfo.GetMaxPsionicPower(this.App, si.DesignInfo, admiralTraits);
					if (si.PsionicPower != maxPsionicPower)
					{
						float modifierFloatToApply = this.GameDatabase.GetStratModifierFloatToApply(StratModifiers.PsiPotentialApplyModifier, si.DesignInfo.PlayerID);
						si.PsionicPower += (int)((double)this._db.GetPlayerInfo(si.DesignInfo.PlayerID).PsionicPotential / (double)modifierFloatToApply);
						si.PsionicPower = Math.Min(si.PsionicPower, ShipInfo.GetMaxPsionicPower(this.App, si.DesignInfo, admiralTraits));
						this._db.UpdateShipPsionicPower(si.ID, si.PsionicPower);
					}
					StationInfo stationInfo = this._db.GetStationInfos().FirstOrDefault<StationInfo>((Func<StationInfo, bool>)(x => x.ShipID == si.ID));
					if (fleetInfo != null && fleetInfo.SupportingSystemID == fleetInfo.SystemID || stationInfo != null)
					{
						foreach (SectionInstanceInfo sectionInstanceInfo in this._db.GetShipSectionInstances(si.ID).ToList<SectionInstanceInfo>())
						{
							SectionInstanceInfo sii = sectionInstanceInfo;
							DesignSectionInfo designSectionInfo = ((IEnumerable<DesignSectionInfo>)si.DesignInfo.DesignSections).FirstOrDefault<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ID == sii.SectionID));
							List<string> techs = new List<string>();
							if (designSectionInfo != null && designSectionInfo.Techs.Count > 0)
							{
								foreach (int tech in designSectionInfo.Techs)
									techs.Add(this._db.GetTechFileID(tech));
							}
							ShipSectionAsset shipSectionAsset = ((IEnumerable<DesignSectionInfo>)si.DesignInfo.DesignSections).First<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ID == sii.SectionID)).ShipSectionAsset;
							int supplyWithTech = Kerberos.Sots.GameObjects.Ship.GetSupplyWithTech(this.AssetDatabase, techs, shipSectionAsset.Supply);
							if (sii.Crew != shipSectionAsset.Crew || sii.Supply != supplyWithTech)
							{
								sii.Crew = shipSectionAsset.Crew;
								sii.Supply = supplyWithTech;
								this._db.UpdateSectionInstance(sii);
							}
						}
					}
				}
			}
		}

		private void UpdateRepairPoints()
		{
			foreach (ShipInfo shipInfo in this._db.GetShipInfos(true).ToList<ShipInfo>())
			{
				foreach (SectionInstanceInfo sectionInstanceInfo in this._db.GetShipSectionInstances(shipInfo.ID).ToList<SectionInstanceInfo>())
				{
					SectionInstanceInfo sii = sectionInstanceInfo;
					DesignSectionInfo designSectionInfo = ((IEnumerable<DesignSectionInfo>)shipInfo.DesignInfo.DesignSections).FirstOrDefault<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ID == sii.SectionID));
					if (designSectionInfo != null)
					{
						if (sii.RepairPoints != designSectionInfo.ShipSectionAsset.RepairPoints)
						{
							sii.RepairPoints = designSectionInfo.ShipSectionAsset.RepairPoints;
							this._db.UpdateSectionInstance(sii);
						}
						foreach (ModuleInstanceInfo moduleInstance in sii.ModuleInstances)
						{
							ModuleInstanceInfo mii = moduleInstance;
							DesignModuleInfo designModuleInfo = designSectionInfo.Modules.FirstOrDefault<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x => x.MountNodeName == mii.ModuleNodeID));
							if (designModuleInfo != null)
							{
								string path = this._db.GetModuleAsset(designModuleInfo.ModuleID);
								LogicalModule logicalModule = this.AssetDatabase.Modules.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == path));
								if (mii.RepairPoints != logicalModule.RepairPointsBonus)
								{
									mii.RepairPoints = logicalModule.RepairPointsBonus;
									this._db.UpdateModuleInstance(mii);
								}
							}
						}
					}
				}
			}
		}

		private void HandleGives()
		{
			List<PlayerInfo> source = new List<PlayerInfo>();
			foreach (GiveInfo giveInfo in this.GameDatabase.GetGiveInfos())
			{
				GiveInfo give = giveInfo;
				if (!source.Any<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID == give.ReceivingPlayer)))
					source.Add(this.GameDatabase.GetPlayerInfo(give.ReceivingPlayer));
				int reactionAmount = 0;
				if (this.GetPlayerObject(give.ReceivingPlayer).IsAI() && this.GetPlayerObject(give.ReceivingPlayer).IsStandardPlayer)
				{
					DiplomacyInfo diplomacyInfo = this._db.GetDiplomacyInfo(give.ReceivingPlayer, give.InitiatingPlayer);
					reactionAmount = (int)((double)(int)Math.Min(give.GiveValue / 20000f, 100f) * ((this._db.GetPlayerFaction(give.InitiatingPlayer).Name == "morrigi" ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") * 0.5 : 0.0) + (this._db.PlayerHasTech(give.InitiatingPlayer, "PSI_Lesser_Glamour") ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") : 0.0) + 1.0));
					diplomacyInfo.Relations += reactionAmount;
				}
				switch (give.Type)
				{
					case GiveType.GiveSavings:
						source.FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID == give.ReceivingPlayer)).Savings += (double)give.GiveValue;
						if (this.GetPlayerObject(give.ReceivingPlayer).IsAI() && this.GetPlayerObject(give.ReceivingPlayer).IsStandardPlayer)
							this._db.ApplyDiplomacyReaction(give.ReceivingPlayer, give.InitiatingPlayer, reactionAmount, new StratModifiers?(StratModifiers.DiplomacyReactionGiveSavings), 1);
						this.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_RECEIVED_MONEY,
							EventMessage = TurnEventMessage.EM_RECEIVED_MONEY,
							PlayerID = give.ReceivingPlayer,
							TargetPlayerID = give.InitiatingPlayer,
							Savings = (double)give.GiveValue,
							TurnNumber = this.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
						continue;
					case GiveType.GiveResearchPoints:
						source.FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID == give.ReceivingPlayer)).AdditionalResearchPoints += this.ConvertToResearchPoints(give.ReceivingPlayer, (double)give.GiveValue);
						if (this.GetPlayerObject(give.ReceivingPlayer).IsAI() && this.GetPlayerObject(give.ReceivingPlayer).IsStandardPlayer)
							this._db.ApplyDiplomacyReaction(give.ReceivingPlayer, give.InitiatingPlayer, reactionAmount, new StratModifiers?(StratModifiers.DiplomacyReactionGiveResearch), 1);
						this.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_RECEIVED_RESEARCH_MONEY,
							EventMessage = TurnEventMessage.EM_RECEIVED_RESEARCH_MONEY,
							PlayerID = give.ReceivingPlayer,
							TargetPlayerID = give.InitiatingPlayer,
							Savings = (double)give.GiveValue,
							TurnNumber = this.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
						continue;
					default:
						continue;
				}
			}
			foreach (PlayerInfo playerInfo in source)
			{
				this.GameDatabase.UpdatePlayerAdditionalResearchPoints(playerInfo.ID, playerInfo.AdditionalResearchPoints);
				this.GameDatabase.UpdatePlayerSavings(playerInfo.ID, playerInfo.Savings);
			}
			this.GameDatabase.ClearGiveInfos();
		}

		private void UpdateMorale()
		{
			foreach (PlayerInfo playerInfo in this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				if (playerInfo.Savings < -3000000.0)
					GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_3MILLION_DEBT, playerInfo.ID, new int?(), new int?(), new int?());
				else if (playerInfo.Savings < -1000000.0)
					GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_1MILLION_DEBT, playerInfo.ID, new int?(), new int?(), new int?());
				else if (playerInfo.Savings > 25000000.0)
					GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_25MILLION_SAVINGS, playerInfo.ID, new int?(), new int?(), new int?());
				else if (playerInfo.Savings > 10000000.0)
					GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_10MILLION_SAVINGS, playerInfo.ID, new int?(), new int?(), new int?());
				if ((double)playerInfo.RateTax != 0.0500000007450581)
				{
					int multiplier = (int)Math.Round((double)Math.Abs((float)(((double)playerInfo.RateTax - 0.0500000007450581) * 100.0)));
					MoralEvent eventType = MoralEvent.ME_TAX_DECREASED;
					if ((double)playerInfo.RateTax > 0.0500000007450581)
					{
						eventType = MoralEvent.ME_TAX_INCREASED;
						multiplier *= 2;
					}
					GameSession.ApplyMoralEvent(this.App, multiplier, eventType, playerInfo.ID, new int?(), new int?(), new int?());
				}
				List<EncounterInfo> list = this.GameDatabase.GetEncounterInfos().ToList<EncounterInfo>();
				List<AsteroidMonitorInfo> source1 = new List<AsteroidMonitorInfo>();
				List<MorrigiRelicInfo> source2 = new List<MorrigiRelicInfo>();
				foreach (EncounterInfo encounterInfo in list)
				{
					if (encounterInfo.Type == EasterEgg.EE_ASTEROID_MONITOR)
					{
						AsteroidMonitorInfo asteroidMonitorInfo = this.GameDatabase.GetAsteroidMonitorInfo(encounterInfo.Id);
						if (asteroidMonitorInfo != null)
							source1.Add(asteroidMonitorInfo);
					}
					if (encounterInfo.Type == EasterEgg.EE_MORRIGI_RELIC)
					{
						MorrigiRelicInfo morrigiRelicInfo = this.GameDatabase.GetMorrigiRelicInfo(encounterInfo.Id);
						if (morrigiRelicInfo != null)
							source2.Add(morrigiRelicInfo);
					}
				}
				foreach (ColonyInfo colonyInfo in this.GameDatabase.GetPlayerColoniesByPlayerId(playerInfo.ID).ToList<ColonyInfo>())
				{
					OrbitalObjectInfo ooi = this.GameDatabase.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID);
					foreach (FleetInfo fleetInfo in this.App.GameDatabase.GetFleetInfoBySystemID(ooi.StarSystemID, FleetType.FL_NORMAL).ToList<FleetInfo>())
					{
						if (fleetInfo.PlayerID != playerInfo.ID && this.GameDatabase.GetDiplomacyStateBetweenPlayers(fleetInfo.PlayerID, playerInfo.ID) == DiplomacyState.WAR)
						{
							if (this.ScriptModules.AsteroidMonitor.PlayerID == fleetInfo.PlayerID || this.ScriptModules.MorrigiRelic.PlayerID == fleetInfo.PlayerID)
							{
								AsteroidMonitorInfo asteroidMonitorInfo = source1.FirstOrDefault<AsteroidMonitorInfo>((Func<AsteroidMonitorInfo, bool>)(x => x.SystemId == ooi.StarSystemID));
								MorrigiRelicInfo morrigiRelicInfo = source2.FirstOrDefault<MorrigiRelicInfo>((Func<MorrigiRelicInfo, bool>)(x => x.SystemId == ooi.StarSystemID));
								if (morrigiRelicInfo != null && morrigiRelicInfo.IsAggressive || asteroidMonitorInfo != null && asteroidMonitorInfo.IsAggressive)
									GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_ENEMY_IN_SYSTEM, playerInfo.ID, new int?(), new int?(), new int?(ooi.StarSystemID));
							}
							else
								GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_ENEMY_IN_SYSTEM, playerInfo.ID, new int?(), new int?(), new int?(ooi.StarSystemID));
						}
					}
					if (((IEnumerable<ColonyFactionInfo>)colonyInfo.Factions).Count<ColonyFactionInfo>() > 0 && ((IEnumerable<ColonyFactionInfo>)colonyInfo.Factions).Sum<ColonyFactionInfo>((Func<ColonyFactionInfo, double>)(x => x.CivilianPop)) > 0.0)
					{
						if ((double)colonyInfo.EconomyRating <= 0.0)
							GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_ECONOMY_0, playerInfo.ID, new int?(colonyInfo.ID), new int?(), new int?());
						else if ((double)colonyInfo.EconomyRating <= 0.150000005960464)
							GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_ECONOMY_BELOW_15, playerInfo.ID, new int?(colonyInfo.ID), new int?(), new int?());
						else if ((double)colonyInfo.EconomyRating >= 1.0)
							GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_ECONOMY_100, playerInfo.ID, new int?(colonyInfo.ID), new int?(), new int?());
						else if ((double)colonyInfo.EconomyRating > 0.850000023841858)
							GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_ECONOMY_ABOVE_85, playerInfo.ID, new int?(colonyInfo.ID), new int?(), new int?());
					}
				}
			}
		}

		private double UpdateGovernmentSpending(PlayerInfo pi, double revenue)
		{
			this.UpdateSavingsSpending(pi, revenue * (double)pi.RateGovernmentSavings);
			this.UpdateSecuritySpending(pi, revenue * (double)pi.RateGovernmentSecurity);
			this.UpdateStimulusSpending(pi, revenue * (double)pi.RateGovernmentStimulus);
			return revenue;
		}

		private void UpdateSavingsSpending(PlayerInfo pi, double revenue)
		{
			pi.Savings += revenue;
			this._db.UpdatePlayerSavings(pi.ID, pi.Savings);
		}

		private void UpdateSecuritySpending(PlayerInfo pi, double revenue)
		{
			double num1 = revenue * (double)pi.RateSecurityOperations;
			double num2 = revenue * (double)pi.RateSecurityIntelligence;
			double num3 = revenue * (double)pi.RateSecurityCounterIntelligence;
			foreach (StationInfo stationInfo in this.App.GameDatabase.GetStationInfosByPlayerID(pi.ID).ToList<StationInfo>())
			{
				if (stationInfo.DesignInfo.StationType == StationType.DIPLOMATIC && stationInfo.DesignInfo.StationLevel > 0)
				{
					List<DesignModuleInfo> list = stationInfo.DesignInfo.DesignSections[0].Modules.Where<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x =>
				   {
					   ModuleEnums.StationModuleType? stationModuleType = x.StationModuleType;
					   if (stationModuleType.GetValueOrDefault() == ModuleEnums.StationModuleType.Sensor)
						   return stationModuleType.HasValue;
					   return false;
				   })).ToList<DesignModuleInfo>();
					pi.IntelPoints += list.Count;
				}
			}
			pi.IntelAccumulator += (int)num2;
			pi.CounterIntelAccumulator += (int)num3;
			pi.OperationsAccumulator += (int)num1;
			int num4 = (int)Math.Floor((double)pi.IntelAccumulator / (double)this.App.AssetDatabase.SecurityPointCost);
			int num5 = (int)Math.Floor((double)pi.CounterIntelAccumulator / (double)this.App.AssetDatabase.SecurityPointCost);
			int num6 = (int)Math.Floor((double)pi.OperationsAccumulator / (double)this.App.AssetDatabase.SecurityPointCost);
			pi.IntelPoints += num4;
			pi.CounterIntelPoints += num5;
			pi.OperationsPoints += num6;
			pi.IntelAccumulator -= num4 * this.App.AssetDatabase.SecurityPointCost;
			pi.CounterIntelAccumulator -= num5 * this.App.AssetDatabase.SecurityPointCost;
			pi.OperationsAccumulator -= num6 * this.App.AssetDatabase.SecurityPointCost;
			this.App.GameDatabase.UpdatePlayerIntelPoints(pi.ID, pi.IntelPoints);
			this.App.GameDatabase.UpdatePlayerCounterintelPoints(pi.ID, pi.CounterIntelPoints);
			this.App.GameDatabase.UpdatePlayerOperationsPoints(pi.ID, pi.OperationsPoints);
			this.App.GameDatabase.UpdatePlayerIntelAccumulator(pi.ID, pi.IntelAccumulator);
			this.App.GameDatabase.UpdatePlayerCounterintelAccumulator(pi.ID, pi.CounterIntelAccumulator);
			this.App.GameDatabase.UpdatePlayerOperationsAccumulator(pi.ID, pi.OperationsAccumulator);
		}

		private bool DoStimulusMiningRoll(float miningChance, PlayerInfo player)
		{
			if (!this._random.CoinToss((double)miningChance))
				return false;
			List<int> list1 = this.GameDatabase.GetPlayerColonySystemIDs(player.ID).ToList<int>();
			list1.RemoveAll((Predicate<int>)(x => !Kerberos.Sots.GameStates.StarSystem.GetSystemCanSupportStations(this, x, player.ID).Contains(StationType.MINING)));
			if (list1.Count == 0)
			{
				list1 = this.GameDatabase.GetStarSystemIDs().Where<int>((Func<int, bool>)(x =>
			   {
				   if (this.GameDatabase.GetExploreRecord(x, player.ID) != null)
					   return this.GameDatabase.GetExploreRecord(x, player.ID).Explored;
				   return false;
			   })).ToList<int>();
				list1.RemoveAll((Predicate<int>)(x => !Kerberos.Sots.GameStates.StarSystem.GetSystemCanSupportStations(this, x, player.ID).Contains(StationType.MINING)));
			}
			list1.RemoveAll((Predicate<int>)(x => !this.GameDatabase.GetStarSystemInfo(x).IsOpen));
			if (list1.Count == 0)
				return false;
			int num = this._random.Choose<int>((IList<int>)list1);
			List<OrbitalObjectInfo> list2 = this.GameDatabase.GetStarSystemOrbitalObjectInfos(num).ToList<OrbitalObjectInfo>();
			List<StationInfo> list3 = this.GameDatabase.GetStationForSystem(num).ToList<StationInfo>();
			int parentOrbitalId = 0;
			foreach (OrbitalObjectInfo orbitalObjectInfo in list2)
			{
				PlanetInfo pi = this.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID);
				LargeAsteroidInfo li = this.GameDatabase.GetLargeAsteroidInfo(orbitalObjectInfo.ID);
				if (pi != null && pi.Type == "barren" && !list3.Any<StationInfo>((Func<StationInfo, bool>)(x =>
			   {
				   int? parentId = this.GameDatabase.GetOrbitalObjectInfo(x.OrbitalObjectID).ParentID;
				   int id = pi.ID;
				   if ((parentId.GetValueOrDefault() != id ? 0 : (parentId.HasValue ? 1 : 0)) != 0)
					   return x.DesignInfo.StationType == StationType.MINING;
				   return false;
			   })))
				{
					parentOrbitalId = pi.ID;
					break;
				}
				if (li != null && !list3.Any<StationInfo>((Func<StationInfo, bool>)(x =>
			   {
				   int? parentId = this.GameDatabase.GetOrbitalObjectInfo(x.OrbitalObjectID).ParentID;
				   int id = li.ID;
				   if ((parentId.GetValueOrDefault() != id ? 0 : (parentId.HasValue ? 1 : 0)) != 0)
					   return x.DesignInfo.StationType == StationType.MINING;
				   return false;
			   })))
				{
					parentOrbitalId = li.ID;
					break;
				}
			}
			if (parentOrbitalId == 0)
				return false;
			DesignInfo stationDesignInfo = DesignLab.CreateStationDesignInfo(this.AssetDatabase, this.GameDatabase, player.ID, StationType.MINING, 1, true);
			this.ConstructStation(stationDesignInfo, parentOrbitalId, true);
			Faction faction = this.App.AssetDatabase.GetFaction(player.FactionID);
			string localizedStationTypeName = this.App.AssetDatabase.GetLocalizedStationTypeName(stationDesignInfo.StationType, faction.HasSlaves());
			this.App.GameDatabase.InsertGovernmentAction(player.ID, string.Format(App.Localize("@GA_STATIONBUILT"), (object)localizedStationTypeName), "StationBuilt_Mine_Stimulus", 0, 0);
			this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_MINING_STIMULUS,
				EventMessage = TurnEventMessage.EM_MINING_STIMULUS,
				PlayerID = player.ID,
				SystemID = num,
				TurnNumber = this.App.GameDatabase.GetTurnCount(),
				ShowsDialog = false
			});
			return true;
		}

		private bool DoStimulusColonizationRoll(float colonizationChance, PlayerInfo player)
		{
			if (!this._random.CoinToss((double)colonizationChance))
				return false;
			List<int> list1 = this.GameDatabase.GetStarSystemIDs().Where<int>((Func<int, bool>)(x =>
		   {
			   if (this.GameDatabase.GetExploreRecord(x, player.ID) != null)
				   return this.GameDatabase.GetExploreRecord(x, player.ID).Explored;
			   return false;
		   })).ToList<int>();
			List<PlanetInfo> source1 = new List<PlanetInfo>();
			foreach (int num in list1)
			{
				if (this.GameDatabase.GetStarSystemInfo(num).IsOpen && this.GameDatabase.GetColonyInfosForSystem(num).Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID != player.ID)).Count<ColonyInfo>() == 0)
					source1.AddRange((IEnumerable<PlanetInfo>)((IEnumerable<PlanetInfo>)this.GameDatabase.GetStarSystemPlanetInfos(num)).ToList<PlanetInfo>());
			}
			source1.RemoveAll((Predicate<PlanetInfo>)(x =>
		   {
			   if (!(x.Type == "gaseous"))
				   return x.Type == "barren";
			   return true;
		   }));
			source1.RemoveAll((Predicate<PlanetInfo>)(x => (double)this.GameDatabase.GetPlanetHazardRating(player.ID, x.ID, false) > (double)this.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, player.ID)));
			source1.RemoveAll((Predicate<PlanetInfo>)(x => this.GameDatabase.GetColonyInfoForPlanet(x.ID) != null));
			if (source1.Count == 0)
				return false;
			List<PlanetInfo> list2 = source1.OrderBy<PlanetInfo, float>((Func<PlanetInfo, float>)(x => this.GameDatabase.GetPlanetHazardRating(player.ID, x.ID, false))).ToList<PlanetInfo>();
			int index = this._random.Next(0, Math.Min(list2.Count, 3));
			PlanetInfo planetInfo = list2[index];
			OrbitalObjectInfo ooi = this.GameDatabase.GetOrbitalObjectInfo(planetInfo.ID);
			List<int> source2 = new List<int>();
			foreach (ProvinceInfo provinceInfo in this.GameDatabase.GetProvinceInfos().ToList<ProvinceInfo>())
			{
				if (provinceInfo.PlayerID == player.ID)
					source2.Add(provinceInfo.CapitalSystemID);
			}
			HomeworldInfo playerHomeworld = this.GameDatabase.GetPlayerHomeworld(player.ID);
			if (playerHomeworld != null)
				source2.Add(playerHomeworld.SystemID);
			List<int> list3 = source2.OrderBy<int, float>((Func<int, float>)(x => (this.GameDatabase.GetStarSystemOrigin(x) - this.GameDatabase.GetStarSystemOrigin(ooi.StarSystemID)).Length)).ToList<int>();
			string factionName = string.Format("{0} {1}", (object)ooi.Name, (object)App.Localize("@UI_PLAYER_NAME_CIVILIAN_COLONY"));
			Faction faction = this.AssetDatabase.GetFaction(player.FactionID);
			int insertIndyPlayerId = this.GameDatabase.GetOrInsertIndyPlayerId(this.App.Game, player.FactionID, factionName, faction.SplinterAvatarPath());
			this.GameDatabase.UpdateDiplomacyState(insertIndyPlayerId, player.ID, DiplomacyState.ALLIED, 1500 - (int)(this.GameDatabase.GetStarSystemOrigin(list3.First<int>()) - this.GameDatabase.GetStarSystemOrigin(ooi.StarSystemID)).Length, true);
			DiplomacyInfo diplomacyInfo1 = this.GameDatabase.GetDiplomacyInfo(player.ID, insertIndyPlayerId);
			diplomacyInfo1.isEncountered = true;
			this.GameDatabase.UpdateDiplomacyInfo(diplomacyInfo1);
			ColonyInfo colonyInfo = this.GameDatabase.GetColonyInfo(this.GameDatabase.InsertColony(planetInfo.ID, insertIndyPlayerId, 500.0, 0.5f, this.GameDatabase.GetTurnCount(), planetInfo.Infrastructure, false));
			if (((IEnumerable<ColonyFactionInfo>)colonyInfo.Factions).Any<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID == player.FactionID)))
			{
				ColonyFactionInfo civPop = ((IEnumerable<ColonyFactionInfo>)colonyInfo.Factions).First<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID == player.FactionID));
				civPop.CivilianPop += (double)this.GameDatabase.AssetDatabase.StimulusColonizationBonus;
				this.GameDatabase.UpdateCivilianPopulation(civPop);
			}
			else
				this.GameDatabase.InsertColonyFaction(planetInfo.ID, player.FactionID, (double)this.GameDatabase.AssetDatabase.StimulusColonizationBonus, 0.0f, this.GameDatabase.GetTurnCount());
			foreach (PlayerInfo playerInfo in this.GameDatabase.GetPlayerInfos().ToList<PlayerInfo>())
			{
				DiplomacyInfo diplomacyInfo2 = this.GameDatabase.GetDiplomacyInfo(player.ID, playerInfo.ID);
				if (this.GameDatabase.GetFactionName(playerInfo.FactionID) == "morrigi")
					this.GameDatabase.UpdateDiplomacyState(insertIndyPlayerId, playerInfo.ID, diplomacyInfo2.State, diplomacyInfo2.Relations + 300, true);
				else
					this.GameDatabase.UpdateDiplomacyState(insertIndyPlayerId, playerInfo.ID, diplomacyInfo2.State, diplomacyInfo2.Relations, true);
			}
			this.App.GameDatabase.InsertGovernmentAction(player.ID, App.Localize("@GA_CIVCOLONYSTARTED"), "Civ_ColonyStarted", 0, 0);
			this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_COLONY_STIMULUS,
				EventMessage = TurnEventMessage.EM_COLONY_STIMULUS,
				PlayerID = player.ID,
				SystemID = ooi.StarSystemID,
				TurnNumber = this.App.GameDatabase.GetTurnCount(),
				ShowsDialog = false
			});
			this.GameDatabase.DuplicateStratModifiers(insertIndyPlayerId, player.ID);
			this._db.InsertTreaty(new TreatyInfo()
			{
				Active = false,
				Removed = false,
				Type = TreatyType.Trade,
				ReceivingPlayerId = player.ID,
				InitiatingPlayerId = insertIndyPlayerId,
				StartingTurn = this._db.GetTurnCount(),
				Duration = 0
			});
			colonyInfo.ShipConRate = 0.0f;
			colonyInfo.TerraRate = 0.75f;
			colonyInfo.InfraRate = 0.25f;
			colonyInfo.TradeRate = 0.0f;
			this.GameDatabase.UpdateColony(colonyInfo);
			return true;
		}

		private bool DoStimulusTradeRoll(float tradeChance, PlayerInfo player)
		{
			if (!this._random.CoinToss((double)tradeChance))
				return false;
			List<StationInfo> list1 = this._db.GetStationInfosByPlayerID(player.ID).Where<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == StationType.CIVILIAN)).ToList<StationInfo>();
			foreach (StationInfo si in list1.ToList<StationInfo>())
			{
				OrbitalObjectInfo orbitalObjectInfo = this.GameDatabase.GetOrbitalObjectInfo(si.OrbitalObjectID);
				bool flag = orbitalObjectInfo != null;
				if (flag)
				{
					StarSystemInfo starSystemInfo = this.GameDatabase.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
					flag = starSystemInfo != (StarSystemInfo)null && starSystemInfo.IsOpen;
					if (flag && this.GetTradeDockCapacity(si) <= this._db.GetFreighterInfosForSystem(starSystemInfo.ID).Count<FreighterInfo>())
						flag = false;
				}
				if (!flag)
					list1.Remove(si);
			}
			StationInfo stationInfo = (StationInfo)null;
			if (list1.Count != 0)
				stationInfo = this._random.Choose<StationInfo>((IList<StationInfo>)list1);
			int? nullable = new int?();
			if (stationInfo != null)
				nullable = new int?(this._db.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID).StarSystemID);
			if (nullable.HasValue)
			{
				List<DesignInfo> list2 = this._db.GetVisibleDesignInfosForPlayerAndRole(player.ID, ShipRole.FREIGHTER, true).Where<DesignInfo>((Func<DesignInfo, bool>)(x => x.isPrototyped)).ToList<DesignInfo>().OrderBy<DesignInfo, int>((Func<DesignInfo, int>)(x => x.DesignDate)).ToList<DesignInfo>();
				if (list2.Count == 0)
					return false;
				this._db.InsertFreighter(nullable.Value, player.ID, list2.Last<DesignInfo>().ID, false);
				this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_TRADE_STIMULUS,
					EventMessage = TurnEventMessage.EM_TRADE_STIMULUS,
					PlayerID = player.ID,
					SystemID = nullable.Value,
					TurnNumber = this.App.GameDatabase.GetTurnCount(),
					ShowsDialog = false
				});
				this._app.GameDatabase.InsertGovernmentAction(player.ID, App.Localize("@GA_BUILTFREIGHTER"), "BuiltFreighter_Trade", 0, 0);
			}
			return true;
		}

		private void UpdateStimulusSpending(PlayerInfo pi, double revenue)
		{
			double num1 = 300000.0;
			int multiplier = (int)Math.Floor(revenue / num1);
			if (multiplier > 0)
			{
				foreach (ColonyInfo colony in this._db.GetPlayerColoniesByPlayerId(pi.ID).ToList<ColonyInfo>())
				{
					colony.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.Stimulus300K, multiplier);
					this._db.UpdateColony(colony);
				}
			}
			double num2 = revenue * (double)pi.RateStimulusMining;
			double num3 = revenue * (double)pi.RateStimulusColonization;
			double num4 = revenue * (double)pi.RateStimulusTrade;
			pi.CivilianMiningAccumulator += (int)num2;
			pi.CivilianColonizationAccumulator += (int)num3;
			pi.CivilianTradeAccumulator += (int)num4;
			float miningChance = ((float)pi.CivilianMiningAccumulator - (float)this.AssetDatabase.StimulusMiningMin) / (float)this.AssetDatabase.StimulusMiningMax;
			float colonizationChance = ((float)pi.CivilianColonizationAccumulator - (float)this.AssetDatabase.StimulusColonizationMin) / (float)this.AssetDatabase.StimulusColonizationMax;
			float tradeChance = ((float)pi.CivilianTradeAccumulator - (float)this.AssetDatabase.StimulusTradeMin) / (float)this.AssetDatabase.StimulusTradeMax;
			if ((double)miningChance > 0.0)
				pi.CivilianMiningAccumulator = this.DoStimulusMiningRoll(miningChance, pi) ? 0 : pi.CivilianMiningAccumulator;
			if ((double)colonizationChance > 0.0)
				pi.CivilianColonizationAccumulator = this.DoStimulusColonizationRoll(colonizationChance, pi) ? 0 : pi.CivilianColonizationAccumulator;
			if ((double)tradeChance > 0.0)
				pi.CivilianTradeAccumulator = this.DoStimulusTradeRoll(tradeChance, pi) ? 0 : pi.CivilianTradeAccumulator;
			this.App.GameDatabase.UpdatePlayerCivilianMiningAccumulator(pi.ID, pi.CivilianMiningAccumulator);
			this.App.GameDatabase.UpdatePlayerCivilianColonizationAccumulator(pi.ID, pi.CivilianColonizationAccumulator);
			this.App.GameDatabase.UpdatePlayerCivilianTradeAccumulator(pi.ID, pi.CivilianTradeAccumulator);
		}

		private List<int> CheckForInterdiction(FleetInfo fleet, int systemID)
		{
			List<int> intList = new List<int>();
			foreach (FleetInfo fleetInfo in this._db.GetFleetInfoBySystemID(systemID, FleetType.FL_NORMAL))
			{
				if (fleetInfo.PlayerID != fleet.PlayerID && this._db.GetDiplomacyStateBetweenPlayers(fleet.PlayerID, fleetInfo.PlayerID) == DiplomacyState.WAR)
				{
					MissionInfo missionByFleetId = this._db.GetMissionByFleetID(fleetInfo.ID);
					if (missionByFleetId != null && missionByFleetId.Type == MissionType.INTERDICTION)
						intList.Add(fleetInfo.ID);
				}
			}
			return intList;
		}

		private bool IsFleetIntercepted(FleetInfo fleet)
		{
			MissionInfo missionByFleetId = this._db.GetMissionByFleetID(fleet.ID);
			if (missionByFleetId != null)
				return this._db.GetWaypointsByMissionID(missionByFleetId.ID).Any<WaypointInfo>((Func<WaypointInfo, bool>)(x => x.Type == WaypointType.Intercepted));
			return false;
		}

		private bool IsFleetInterdicted(FleetInfo fleet)
		{
			foreach (FleetInfo fleetInfo in this._db.GetFleetInfoBySystemID(fleet.SystemID, FleetType.FL_NORMAL))
			{
				if (fleetInfo.PlayerID != fleet.PlayerID)
				{
					MissionInfo missionByFleetId = this._db.GetMissionByFleetID(fleetInfo.ID);
					if (missionByFleetId != null && missionByFleetId.Type == MissionType.INTERDICTION && this._db.GetDiplomacyStateBetweenPlayers(fleetInfo.PlayerID, fleet.PlayerID) == DiplomacyState.WAR)
						return true;
				}
			}
			return false;
		}

		public List<int> GetAvailableFleetsForPlayer(int playerID, int systemID)
		{
			List<int> intList = new List<int>();
			foreach (FleetInfo fleetInfo in this._db.GetFleetInfosByPlayerID(playerID, FleetType.FL_NORMAL))
			{
				if (this._db.GetMissionByFleetID(fleetInfo.ID) == null && fleetInfo.SystemID == fleetInfo.SupportingSystemID)
					intList.Add(fleetInfo.ID);
			}
			return intList;
		}

		public static int NumPoliceInSystem(App game, int systemId)
		{
			int num = 0;
			foreach (FleetInfo fleetInfo in game.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_NORMAL).ToList<FleetInfo>())
			{
				if (fleetInfo.IsReserveFleet)
				{
					foreach (ShipInfo shipInfo in game.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false).ToList<ShipInfo>())
					{
						if (shipInfo.IsPoliceShip())
							++num;
					}
				}
			}
			return num;
		}

		public static int GetPropagandaBonusInSystem(App game, int systemId, int playerId)
		{
			int num = 0;
			List<int> intList = new List<int>();
			foreach (FleetInfo fleetInfo in game.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_NORMAL).ToList<FleetInfo>())
			{
				if (!fleetInfo.IsReserveFleet && !intList.Contains(fleetInfo.PlayerID))
				{
					foreach (ShipInfo shipInfo in game.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, true).ToList<ShipInfo>())
					{
						foreach (DesignSectionInfo designSection in shipInfo.DesignInfo.DesignSections)
						{
							if (designSection.ShipSectionAsset.SectionName.Contains("propaganda") && !intList.Contains(fleetInfo.PlayerID))
							{
								intList.Add(fleetInfo.PlayerID);
								break;
							}
						}
						if (intList.Contains(fleetInfo.PlayerID))
							break;
					}
				}
			}
			foreach (int playerID in intList)
			{
				if (game.GameDatabase.GetDiplomacyStateBetweenPlayers(playerID, playerId) == DiplomacyState.WAR)
					num -= 2;
				else if (playerID == playerId)
					num += 2;
			}
			return num;
		}

		public static StationType GetConstructionMissionStationType(
		  GameDatabase db,
		  MissionInfo mission)
		{
			if (mission != null && mission.Type == MissionType.CONSTRUCT_STN && mission.StationType.HasValue)
				return (StationType)mission.StationType.Value;
			return StationType.INVALID_TYPE;
		}

		public List<StationInfo> GetUpgradableStations(List<StationInfo> stations)
		{
			List<StationInfo> stationInfoList = new List<StationInfo>();
			foreach (StationInfo station in stations)
			{
				if (this.StationIsUpgradable(station))
					stationInfoList.Add(station);
			}
			return stationInfoList;
		}

		public bool StationIsUpgradable(StationInfo station)
		{
			if (station.DesignInfo.StationLevel > 4 || station.DesignInfo.StationLevel == 0 || (station.DesignInfo.StationType == StationType.MINING || station.DesignInfo.StationType == StationType.DEFENCE) || station.DesignInfo.StationType == StationType.GATE && station.DesignInfo.StationLevel == 2 && !this.GameDatabase.PlayerHasTech(station.PlayerID, "DRV_Far_Casting"))
				return false;
			((IEnumerable<LogicalModuleMount>)this.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == station.DesignInfo.DesignSections[0].FilePath)).Modules).ToList<LogicalModuleMount>();
			List<DesignModuleInfo> modules = station.DesignInfo.DesignSections[0].Modules;
			Dictionary<ModuleEnums.StationModuleType, int> requiredModules = new Dictionary<ModuleEnums.StationModuleType, int>();
			if ((double)this.GetStationUpgradeProgress(station, out requiredModules) >= 1.0)
				return !this.StationIsUpgrading(station);
			return false;
		}

		public bool StationIsUpgrading(StationInfo station)
		{
			foreach (MissionInfo missionInfo in this.GameDatabase.GetMissionInfos())
			{
				if (missionInfo.TargetOrbitalObjectID == station.OrbitalObjectID && missionInfo.Type == MissionType.UPGRADE_STN)
					return true;
			}
			return false;
		}

		public static int ApplyParamilitaryToMorale(App game, int value, int playerId, int systemId)
		{
			if (value >= 0)
				return value;
			int paramilitaryTechMoralBonus = GameSession.GetParamilitaryTechMoralBonus(game, playerId, systemId);
			return Math.Min(value + paramilitaryTechMoralBonus, 0);
		}

		public static void ApplyMoralEventToSystem(
		  App game,
		  int value,
		  int? playerId,
		  int? colonyId,
		  int? systemId,
		  int? provinceId)
		{
			int morale = GameSession.ApplyParamilitaryToMorale(game, value, playerId.Value, systemId.Value);
			foreach (ColonyInfo colonyInfo in game.GameDatabase.GetColonyInfosForSystem(systemId.Value).ToList<ColonyInfo>())
			{
				int playerId1 = colonyInfo.PlayerID;
				int? nullable = playerId;
				if ((playerId1 != nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
				{
					foreach (ColonyFactionInfo civPop in game.GameDatabase.GetCivilianPopulations(colonyInfo.OrbitalObjectID).ToList<ColonyFactionInfo>())
					{
						if (civPop.CivilianPop > 0.0)
						{
							civPop.Morale += morale;
							game.GameDatabase.UpdateCivilianPopulation(civPop);
						}
					}
				}
			}
		}

		public static void ApplyMoralEventToProvince(
		  App game,
		  int value,
		  int? playerId,
		  int? colonyId,
		  int? systemId,
		  int? provinceId)
		{
			if (!provinceId.HasValue)
				return;
			foreach (ColonyInfo colonyInfo in game.GameDatabase.GetColonyInfosForProvince(provinceId.Value).ToList<ColonyInfo>())
			{
				int playerId1 = colonyInfo.PlayerID;
				int? nullable = playerId;
				if ((playerId1 != nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
				{
					int morale = GameSession.ApplyParamilitaryToMorale(game, value, playerId.Value, colonyInfo.CachedStarSystemID);
					foreach (ColonyFactionInfo civPop in game.GameDatabase.GetCivilianPopulations(colonyInfo.OrbitalObjectID).ToList<ColonyFactionInfo>())
					{
						if (civPop.CivilianPop > 0.0)
						{
							civPop.Morale += morale;
							game.GameDatabase.UpdateCivilianPopulation(civPop);
						}
					}
				}
			}
		}

		public static void ApplyMoralEventToColony(
		  App game,
		  int value,
		  int? playerId,
		  int? colonyId,
		  int? systemId,
		  int? provinceId)
		{
			if (!colonyId.HasValue)
				return;
			ColonyInfo colonyInfo = game.GameDatabase.GetColonyInfo(colonyId.Value);
			if (colonyInfo == null)
				return;
			int morale = GameSession.ApplyParamilitaryToMorale(game, value, playerId.Value, colonyInfo.CachedStarSystemID);
			foreach (ColonyFactionInfo civPop in game.GameDatabase.GetCivilianPopulations(colonyInfo.OrbitalObjectID).ToList<ColonyFactionInfo>())
			{
				if (civPop.CivilianPop > 0.0)
				{
					civPop.Morale += morale;
					game.GameDatabase.UpdateCivilianPopulation(civPop);
				}
			}
		}

		public static void ApplyMoralEventToAllColonies(
		  App game,
		  int value,
		  int? playerId,
		  int? colonyId,
		  int? systemId,
		  int? provinceId)
		{
			foreach (ColonyInfo colonyInfo in game.GameDatabase.GetColonyInfos().ToList<ColonyInfo>())
			{
				int playerId1 = colonyInfo.PlayerID;
				int? nullable = playerId;
				if ((playerId1 != nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
				{
					int morale = GameSession.ApplyParamilitaryToMorale(game, value, playerId.Value, colonyInfo.CachedStarSystemID);
					foreach (ColonyFactionInfo civPop in game.GameDatabase.GetCivilianPopulations(colonyInfo.OrbitalObjectID).ToList<ColonyFactionInfo>())
					{
						if (civPop.CivilianPop > 0.0)
						{
							civPop.Morale += morale;
							game.GameDatabase.UpdateCivilianPopulation(civPop);
						}
					}
				}
			}
		}

		public static List<AssetDatabase.MoralModifier> GetMoraleEffects(
		  App game,
		  MoralEvent eventType,
		  int player)
		{
			GovernmentInfo.GovernmentType currentType = game.GameDatabase.GetGovernmentInfo(player).CurrentType;
			return (!AssetDatabase.MoralModifierMap[currentType].ContainsKey(eventType) ? AssetDatabase.MoralModifierMap[GovernmentInfo.GovernmentType.Centrism] : AssetDatabase.MoralModifierMap[currentType])[eventType];
		}

		public static void ApplyMoralEvent(
		  App game,
		  int multiplier,
		  MoralEvent eventType,
		  int player,
		  int? colonyId = null,
		  int? provinceId = null,
		  int? systemId = null)
		{
			GovernmentInfo.GovernmentType currentType = game.GameDatabase.GetGovernmentInfo(player).CurrentType;
			GameSession.ApplyMoraleEvent((!AssetDatabase.MoralModifierMap[currentType].ContainsKey(eventType) ? AssetDatabase.MoralModifierMap[GovernmentInfo.GovernmentType.Centrism] : AssetDatabase.MoralModifierMap[currentType])[eventType], game, currentType, eventType, multiplier, player, colonyId, systemId, provinceId);
		}

		public static void ApplyMoralEvent(
		  App game,
		  MoralEvent eventType,
		  int player,
		  int? colonyId = null,
		  int? provinceId = null,
		  int? systemId = null)
		{
			GovernmentInfo.GovernmentType governmentType = game.GameDatabase.GetGovernmentInfo(player).CurrentType;
			if (!AssetDatabase.MoralModifierMap[governmentType].ContainsKey(eventType))
				governmentType = GovernmentInfo.GovernmentType.Centrism;
			GameSession.ApplyMoraleEvent(AssetDatabase.MoralModifierMap[governmentType][eventType], game, governmentType, eventType, 1, player, colonyId, systemId, provinceId);
		}

		public static void ApplyMoraleEvent(
		  List<AssetDatabase.MoralModifier> modifiers,
		  App game,
		  GovernmentInfo.GovernmentType governmentType,
		  MoralEvent eventType,
		  int multiplier,
		  int playerId,
		  int? colonyId,
		  int? systemId,
		  int? provinceId)
		{
			Kerberos.Sots.PlayerFramework.Player playerObject = game.Game.GetPlayerObject(playerId);
			int stratModifier = game.GetStratModifier<int>(StratModifiers.MoralBonus, playerId);
			if (modifiers.Count == 1)
			{
				int moral = playerObject.Faction.GetMoralValue(governmentType, eventType, modifiers[0].value) * multiplier + stratModifier;
				int moralTotal = game.AssetDatabase.GovEffects.GetMoralTotal(game.GameDatabase, governmentType, eventType, playerId, moral);
				switch (modifiers[0].type)
				{
					case AssetDatabase.MoraleModifierType.AllColonies:
						GameSession.ApplyMoralEventToAllColonies(game, moralTotal, new int?(playerId), colonyId, systemId, provinceId);
						break;
					case AssetDatabase.MoraleModifierType.Province:
						GameSession.ApplyMoralEventToProvince(game, moralTotal, new int?(playerId), colonyId, systemId, provinceId);
						break;
					case AssetDatabase.MoraleModifierType.System:
						GameSession.ApplyMoralEventToSystem(game, moralTotal, new int?(playerId), colonyId, systemId, provinceId);
						break;
					case AssetDatabase.MoraleModifierType.Colony:
						GameSession.ApplyMoralEventToColony(game, moralTotal, new int?(playerId), colonyId, systemId, provinceId);
						break;
				}
				game.GameDatabase.InsertMoralHistoryEvent(eventType, playerId, moralTotal, colonyId, systemId, provinceId);
			}
			else
			{
				foreach (ColonyInfo colonyInfo in game.GameDatabase.GetColonyInfos().ToList<ColonyInfo>())
				{
					if (colonyInfo.PlayerID == playerId)
					{
						int originalValue = 0;
						bool flag = false;
						if (modifiers.Any<AssetDatabase.MoralModifier>((Func<AssetDatabase.MoralModifier, bool>)(x => x.type == AssetDatabase.MoraleModifierType.Colony)) && colonyId.HasValue && colonyInfo.ID == colonyId.Value)
						{
							flag = true;
							originalValue = modifiers.First<AssetDatabase.MoralModifier>((Func<AssetDatabase.MoralModifier, bool>)(x => x.type == AssetDatabase.MoraleModifierType.Colony)).value;
						}
						if (!flag && (modifiers.Any<AssetDatabase.MoralModifier>((Func<AssetDatabase.MoralModifier, bool>)(x => x.type == AssetDatabase.MoraleModifierType.System)) && systemId.HasValue && colonyInfo.CachedStarSystemID == systemId.Value))
						{
							flag = true;
							originalValue = modifiers.First<AssetDatabase.MoralModifier>((Func<AssetDatabase.MoralModifier, bool>)(x => x.type == AssetDatabase.MoraleModifierType.System)).value;
						}
						if (!flag && (modifiers.Any<AssetDatabase.MoralModifier>((Func<AssetDatabase.MoralModifier, bool>)(x => x.type == AssetDatabase.MoraleModifierType.Province)) && provinceId.HasValue))
						{
							StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(colonyInfo.CachedStarSystemID);
							if (starSystemInfo != (StarSystemInfo)null && starSystemInfo.ProvinceID.HasValue && starSystemInfo.ProvinceID.Value == provinceId.Value)
							{
								flag = true;
								originalValue = modifiers.First<AssetDatabase.MoralModifier>((Func<AssetDatabase.MoralModifier, bool>)(x => x.type == AssetDatabase.MoraleModifierType.Province)).value;
							}
						}
						if (!flag && modifiers.Any<AssetDatabase.MoralModifier>((Func<AssetDatabase.MoralModifier, bool>)(x => x.type == AssetDatabase.MoraleModifierType.AllColonies)))
						{
							flag = true;
							originalValue = modifiers.First<AssetDatabase.MoralModifier>((Func<AssetDatabase.MoralModifier, bool>)(x => x.type == AssetDatabase.MoraleModifierType.AllColonies)).value;
						}
						if (flag)
						{
							int moral = playerObject.Faction.GetMoralValue(governmentType, eventType, originalValue) * multiplier + stratModifier;
							int moralTotal = game.AssetDatabase.GovEffects.GetMoralTotal(game.GameDatabase, governmentType, eventType, playerId, moral);
							int morale = GameSession.ApplyParamilitaryToMorale(game, moralTotal, playerId, colonyInfo.CachedStarSystemID);
							foreach (ColonyFactionInfo civPop in game.GameDatabase.GetCivilianPopulations(colonyInfo.OrbitalObjectID).ToList<ColonyFactionInfo>())
							{
								if (civPop.CivilianPop > 0.0)
								{
									civPop.Morale = Math.Min(100, Math.Max(civPop.Morale + morale, 0));
									game.GameDatabase.UpdateCivilianPopulation(civPop);
								}
							}
							game.GameDatabase.InsertMoralHistoryEvent(eventType, playerId, morale, new int?(colonyInfo.ID), new int?(), new int?());
						}
					}
				}
			}
		}

		public static int GetParamilitaryTechMoralBonus(App game, int player, int system)
		{
			int num = 0;
			PlayerTechInfo playerTechInfo = game.GameDatabase.GetPlayerTechInfos(player).FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "POL_Paramilitary_Training"));
			if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
			{
				foreach (FleetInfo fleetInfo in game.GameDatabase.GetFleetsByPlayerAndSystem(player, system, FleetType.FL_DEFENSE).ToList<FleetInfo>())
				{
					foreach (ShipInfo shipInfo in game.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false).ToList<ShipInfo>())
					{
						if (shipInfo.IsPoliceShip() && game.GameDatabase.GetShipSystemPosition(shipInfo.ID).HasValue)
							++num;
					}
				}
			}
			return num;
		}

		public void DeclareWarInformally(int playerId, int targetPlayerId)
		{
			if (this.GameDatabase.GetPlayerInfo(playerId).IsOnTeam(this.GameDatabase.GetPlayerInfo(targetPlayerId)))
				return;
			this.GameDatabase.InsertDiplomacyActionHistoryEntry(playerId, targetPlayerId, this.GameDatabase.GetTurnCount(), DiplomacyAction.SURPRISEATTACK, new int?(), new float?(), new int?(), new int?(), new float?());
			this.GameDatabase.ChangeDiplomacyState(playerId, targetPlayerId, DiplomacyState.WAR);
			this.GameDatabase.InsertGovernmentAction(playerId, App.Localize("@GA_BETRAYAL"), "Betrayal", 0, 0);
			foreach (int standardPlayerId in this.GameDatabase.GetStandardPlayerIDs())
			{
				if (standardPlayerId != playerId && standardPlayerId != targetPlayerId)
					this.GameDatabase.ApplyDiplomacyReaction(standardPlayerId, playerId, StratModifiers.DiplomacyReactionBetrayal, 1);
			}
			GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_BETRAYAL, playerId, new int?(), new int?(), new int?());
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_BETRAYAL,
				EventMessage = TurnEventMessage.EM_BETRAYAL,
				PlayerID = playerId,
				TargetPlayerID = targetPlayerId,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				ShowsDialog = false
			});
			foreach (int standardPlayerId in this.GameDatabase.GetStandardPlayerIDs())
			{
				if (standardPlayerId != playerId && standardPlayerId != targetPlayerId)
					this.GameDatabase.ApplyDiplomacyReaction(standardPlayerId, targetPlayerId, StratModifiers.DiplomacyReactionBetrayed, 1);
			}
			if (this.GameDatabase.GetPlayerColoniesByPlayerId(playerId).Count<ColonyInfo>() > this.GameDatabase.GetPlayerColoniesByPlayerId(targetPlayerId).Count<ColonyInfo>())
				GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_BETRAYED_LARGER, playerId, new int?(), new int?(), new int?());
			else
				GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_BETRAYED_SMALLER, playerId, new int?(), new int?(), new int?());
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_BETRAYED,
				EventMessage = TurnEventMessage.EM_BETRAYED,
				PlayerID = targetPlayerId,
				TargetPlayerID = playerId,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				ShowsDialog = false
			});
		}

		public void DeclareWarFormally(int playerId, int targetPlayerId)
		{
			if (this.GameDatabase.GetPlayerInfo(playerId).IsOnTeam(this.GameDatabase.GetPlayerInfo(targetPlayerId)))
				return;
			this.GameDatabase.InsertDiplomacyActionHistoryEntry(playerId, targetPlayerId, this.GameDatabase.GetTurnCount(), DiplomacyAction.DECLARATION, new int?(), new float?(), new int?(), new int?(), new float?());
			this.GameDatabase.ChangeDiplomacyState(playerId, targetPlayerId, DiplomacyState.WAR);
			this.GameDatabase.InsertGovernmentAction(playerId, App.Localize("@GA_DECLAREWAR"), "DeclareWar", 0, 0);
			this.GameDatabase.ApplyDiplomacyReaction(playerId, targetPlayerId, StratModifiers.DiplomacyReactionDeclareWar, 1);
			GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_WAR_DECLARED, playerId, new int?(), new int?(), new int?());
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				PlayerID = targetPlayerId,
				TargetPlayerID = playerId,
				EventMessage = TurnEventMessage.EM_WAR_DECLARED_DEFENDER,
				EventType = TurnEventType.EV_WAR_DECLARED,
				TurnNumber = this.GameDatabase.GetTurnCount()
			});
			this.GameDatabase.ApplyDiplomacyReaction(targetPlayerId, playerId, -2000, new StratModifiers?(), 1);
			GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_WAR_DECLARED, playerId, new int?(), new int?(), new int?());
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				PlayerID = playerId,
				TargetPlayerID = targetPlayerId,
				EventMessage = TurnEventMessage.EM_WAR_DECLARED_AGGRESSOR,
				EventType = TurnEventType.EV_WAR_DECLARED,
				TurnNumber = this.GameDatabase.GetTurnCount()
			});
		}

		public int GetNumAdmirals(int playerID)
		{
			return this._db.GetAdmiralInfosForPlayer(playerID).Count<AdmiralInfo>();
		}

		public static string GetMissionDesc(GameDatabase gamedb, MissionInfo mission)
		{
			string str = "" + mission.Type.ToDisplayText();
			if (mission.Type == MissionType.CONSTRUCT_STN || mission.Type == MissionType.UPGRADE_STN)
			{
				StationInfo stationInfo = gamedb.GetStationInfo(mission.TargetOrbitalObjectID);
				if (stationInfo != null)
					str = str + " " + stationInfo.DesignInfo.StationType.ToDisplayText(gamedb.GetFactionName(gamedb.GetPlayerFactionID(gamedb.GetFleetInfo(mission.FleetID).PlayerID)));
			}
			if (mission.Type == MissionType.COLONIZATION || mission.Type == MissionType.SUPPORT)
			{
				OrbitalObjectInfo orbitalObjectInfo = gamedb.GetOrbitalObjectInfo(mission.TargetOrbitalObjectID);
				if (orbitalObjectInfo != null)
					str = str + " - " + orbitalObjectInfo.Name;
			}
			return str;
		}

		public static int GetAvailableAdmiral(GameDatabase gamedb, int playerID)
		{
			foreach (AdmiralInfo admiralInfo in gamedb.GetAdmiralInfosForPlayer(playerID))
			{
				if (gamedb.GetFleetInfoByAdmiralID(admiralInfo.ID, FleetType.FL_NORMAL) == null)
					return admiralInfo.ID;
			}
			return 0;
		}

		public static int GenerateNewAdmiral(
		  AssetDatabase assetdb,
		  int playerID,
		  GameDatabase _db,
		  AdmiralInfo.TraitType? inheritedType,
		  NamesPool namesPool)
		{
			Random safeRandom = App.GetSafeRandom();
			IEnumerable<ColonyInfo> coloniesByPlayerId = _db.GetPlayerColoniesByPlayerId(playerID);
			List<ColonyInfo> source = new List<ColonyInfo>();
			List<string> racesForFaction = ScenarioEnumerations.GetRacesForFaction(_db.GetFactionName(_db.GetPlayerInfo(playerID).FactionID));
			string str1 = racesForFaction[safeRandom.Next(racesForFaction.Count)];
			Race race = assetdb.GetRace(str1);
			float minValue = float.Parse(race.GetVariable("AdmiralMinStartAge"));
			float maxValue = float.Parse(race.GetVariable("AdmiralMaxStartAge"));
			float age = safeRandom.NextInclusive(minValue, maxValue);
			foreach (ColonyInfo colonyInfo in coloniesByPlayerId)
			{
				if (colonyInfo.TurnEstablished == 1 || (double)(_db.GetTurnCount() - colonyInfo.TurnEstablished) > (double)age)
					source.Add(colonyInfo);
			}
			int? homeworldID = new int?(0);
			if (source.Any<ColonyInfo>())
			{
				int index = safeRandom.NextInclusive(0, source.Count<ColonyInfo>() - 1);
				homeworldID = new int?(source.ElementAt<ColonyInfo>(index).OrbitalObjectID);
			}
			else
				homeworldID = new int?();
			string str2 = safeRandom.NextInclusive(0, 1) == 0 ? "female" : "male";
			int num1 = safeRandom.NextInclusive(1, 25);
			int num2 = safeRandom.NextInclusive(1, 99);
			int loyalty = safeRandom.NextInclusive(1, 99);
			string admiralName = namesPool.GetAdmiralName(str1, str2);
			int admiralID = _db.InsertAdmiral(playerID, homeworldID, admiralName, str1, age, str2, (float)num1, (float)num2, loyalty);
			if (inheritedType.HasValue)
				_db.AddAdmiralTrait(admiralID, inheritedType.Value, 1);
			AdmiralInfo.TraitType traitType;
			do
			{
				if (str1 == "presterzuul" || str1 == "liir")
				{
					if (safeRandom.NextInclusive(1, 10) > 8)
					{
						traitType = AdmiralInfo.TraitType.Evangelist;
						break;
					}
				}
				else if (str1 == "hordezuul" && safeRandom.NextInclusive(1, 10) > 8)
				{
					traitType = AdmiralInfo.TraitType.Inquisitor;
					break;
				}
				traitType = safeRandom.Choose<AdmiralInfo.TraitType>((IEnumerable<AdmiralInfo.TraitType>)Enum.GetValues(typeof(AdmiralInfo.TraitType)));
			}
			while (!AdmiralInfo.CanRaceHaveTrait(traitType, str1) || inheritedType.HasValue && traitType == inheritedType.Value);
			_db.AddAdmiralTrait(admiralID, traitType, 1);
			return admiralID;
		}

		public static int? CalculateTurnsToCompleteResearch(
		  int researchCostInPoints,
		  int progressInPoints,
		  int researchPointsPerTurn)
		{
			if (researchPointsPerTurn > 0)
				return new int?((Math.Max(researchCostInPoints - progressInPoints, 0) + (researchPointsPerTurn - 1)) / researchPointsPerTurn);
			return new int?();
		}

		private void CheckAdmiralRetirement()
		{
			foreach (int standardPlayerId in this._db.GetStandardPlayerIDs())
			{
				foreach (AdmiralInfo admiralInfo in this._db.GetAdmiralInfosForPlayer(standardPlayerId))
				{
					AdmiralInfo admiral = admiralInfo;
					admiral.Age += 0.25f;
					this._db.UpdateAdmiralInfo(admiral);
					Race race = this._app.AssetDatabase.GetRace(admiral.Race);
					float num1 = float.Parse(race.GetVariable("AdmiralRetirementAge")) * this._db.GetStratModifierFloatToApply(StratModifiers.AdmiralCareerModifier, standardPlayerId);
					float num2 = float.Parse(race.GetVariable("AdmiralDeathAge")) * this._db.GetStratModifierFloatToApply(StratModifiers.AdmiralCareerModifier, standardPlayerId);
					bool flag1 = false;
					bool flag2 = false;
					if ((double)num2 > 0.0 && (double)admiral.Age >= (double)num2 && (!admiral.Engram && this._random.NextNormal(0.0, 100.0) < (double)(int)(float)(10.0 * ((double)admiral.Age - (double)num2))))
						flag2 = true;
					if (!flag2 && (double)admiral.Age >= (double)num1 && !admiral.Engram)
					{
						bool flag3 = false;
						FleetInfo fleetInfoByAdmiralId = this._db.GetFleetInfoByAdmiralID(admiral.ID, FleetType.FL_NORMAL);
						if (fleetInfoByAdmiralId != null && this._db.GetMissionByFleetID(fleetInfoByAdmiralId.ID) != null)
							flag3 = true;
						if (!flag3 && this._random.NextNormal(0.0, 100.0) < (double)(int)Math.Max(0.0f, Math.Min(100f, (float)(5.0 * ((double)admiral.Age - (double)num1)) + (float)this._db.GetLevelForAdmiralTrait(admiral.ID, AdmiralInfo.TraitType.Conscript) * 20f - (float)this._db.GetLevelForAdmiralTrait(admiral.ID, AdmiralInfo.TraitType.TrueBeliever) * 20f)))
							flag1 = true;
					}
					TurnEvent ev = (TurnEvent)null;
					if (flag2)
						ev = new TurnEvent()
						{
							EventType = TurnEventType.EV_ADMIRAL_DEAD,
							EventMessage = TurnEventMessage.EM_ADMIRAL_DEAD,
							AdmiralID = admiral.ID,
							PlayerID = admiral.PlayerID,
							TurnNumber = this._app.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						};
					else if (flag1)
						ev = new TurnEvent()
						{
							EventType = TurnEventType.EV_ADMIRAL_RETIRED,
							EventMessage = TurnEventMessage.EM_ADMIRAL_RETIRED,
							AdmiralID = admiral.ID,
							PlayerID = admiral.PlayerID,
							TurnNumber = this._app.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						};
					if (ev != null)
						this._app.GameDatabase.InsertTurnEvent(ev);
					if (flag2 || flag1)
					{
						AdmiralInfo.TraitType? inheritedType = new AdmiralInfo.TraitType?();
						if (this._random.Next(100) < (!flag2 ? int.Parse(race.GetVariable("AdmiralInheritTraitChance")) : int.Parse(race.GetVariable("AdmiralDeathInheritTraitChance"))))
						{
							IEnumerable<AdmiralInfo.TraitType> admiralTraits = this._db.GetAdmiralTraits(admiral.ID);
							List<AdmiralInfo.TraitType> traitTypeList = new List<AdmiralInfo.TraitType>();
							foreach (AdmiralInfo.TraitType t in admiralTraits)
							{
								if (AdmiralInfo.IsGoodTrait(t))
									traitTypeList.Add(t);
							}
							if (traitTypeList.Count > 0)
								inheritedType = new AdmiralInfo.TraitType?(traitTypeList[this._random.Next(traitTypeList.Count)]);
						}
						int newAdmiral = GameSession.GenerateNewAdmiral(this.AssetDatabase, standardPlayerId, this.GameDatabase, inheritedType, this.NamesPool);
						FleetInfo fleetInfoByAdmiralId = this._db.GetFleetInfoByAdmiralID(admiral.ID, FleetType.FL_NORMAL);
						if (fleetInfoByAdmiralId != null)
						{
							fleetInfoByAdmiralId.AdmiralID = newAdmiral;
							this._db.UpdateFleetInfo(fleetInfoByAdmiralId);
						}
						AIFleetInfo fleetInfo = this._db.GetAIFleetInfos(admiral.PlayerID).ToList<AIFleetInfo>().FirstOrDefault<AIFleetInfo>((Func<AIFleetInfo, bool>)(x =>
					   {
						   int? admiralId = x.AdmiralID;
						   int id = admiral.ID;
						   if (admiralId.GetValueOrDefault() == id)
							   return admiralId.HasValue;
						   return false;
					   }));
						if (fleetInfo != null)
						{
							fleetInfo.AdmiralID = new int?(newAdmiral);
							this._db.UpdateAIFleetInfo(fleetInfo);
						}
						this._db.RemoveAdmiral(admiral.ID);
					}
				}
			}
		}

		public float GetFamilySpecificResearchModifier(
		  int playerId,
		  string techFamily,
		  out int totalModulesCounted)
		{
			totalModulesCounted = 0;
			float num = 0.0f;
			foreach (StationInfo station in this._db.GetStationInfosByPlayerID(playerId).ToList<StationInfo>())
			{
				Dictionary<ModuleEnums.StationModuleType, int> source = GameSession.CountStationModuleTypes(station, this._db, false);
				switch (station.DesignInfo.StationType)
				{
					case StationType.SCIENCE:
						if (source.Keys.Any<ModuleEnums.StationModuleType>((Func<ModuleEnums.StationModuleType, bool>)(x => x.ToString().Substring(0, 3) == techFamily)))
						{
							num += (float)source.First<KeyValuePair<ModuleEnums.StationModuleType, int>>((Func<KeyValuePair<ModuleEnums.StationModuleType, int>, bool>)(x => x.Key.ToString().Substring(0, 3) == techFamily)).Value * 0.03f;
							totalModulesCounted += source.First<KeyValuePair<ModuleEnums.StationModuleType, int>>((Func<KeyValuePair<ModuleEnums.StationModuleType, int>, bool>)(x => x.Key.ToString().Substring(0, 3) == techFamily)).Value;
							continue;
						}
						continue;
					case StationType.GATE:
						if (source.ContainsKey(ModuleEnums.StationModuleType.GateLab) && techFamily == "DRV")
						{
							num += (float)source[ModuleEnums.StationModuleType.GateLab] * 0.03f;
							totalModulesCounted += source[ModuleEnums.StationModuleType.GateLab];
							continue;
						}
						continue;
					default:
						continue;
				}
			}
			if (techFamily == "PSI")
				num += this._db.GetStratModifierFloatToApply(StratModifiers.PsiResearchModifier, playerId) - 1f;
			else if (techFamily == "CCC")
				num += this._db.GetStratModifierFloatToApply(StratModifiers.C3ResearchModifier, playerId) - 1f;
			return num;
		}

		public float GetGeneralResearchModifier(int playerId, bool forDisplay)
		{
			float num1 = 1f;
			foreach (StationInfo station in this._db.GetStationInfosByPlayerID(playerId).ToList<StationInfo>())
			{
				Dictionary<ModuleEnums.StationModuleType, int> dictionary = GameSession.CountStationModuleTypes(station, this._db, true);
				if (station.DesignInfo.StationType == StationType.SCIENCE)
				{
					if (dictionary.ContainsKey(ModuleEnums.StationModuleType.Habitation))
						num1 += (float)dictionary[ModuleEnums.StationModuleType.Habitation] * 0.01f;
					if (dictionary.ContainsKey(ModuleEnums.StationModuleType.AlienHabitation))
						num1 += (float)dictionary[ModuleEnums.StationModuleType.AlienHabitation] * 0.02f;
				}
			}
			float num2 = num1 + this._db.GetStratModifierFloatToApply(StratModifiers.AIResearchBonus, playerId) * this.App.GetStratModifier<float>(StratModifiers.AIBenefitBonus, playerId) + (this._db.GetStratModifierFloatToApply(StratModifiers.ResearchModifier, playerId) - 1f) + this._app.AssetDatabase.GetAIModifier(this._app, DifficultyModifiers.ResearchBonus, playerId);
			if (!forDisplay)
				num2 += this._db.GetStratModifierFloatToApply(StratModifiers.GlobalResearchModifier, playerId) - 1f;
			return num2;
		}

		private float GetStationTradeModifierForSystem(int playerId, int systemId, bool isLocalTrade)
		{
			float num = 1f;
			foreach (StationInfo station in this._db.GetStationForSystemAndPlayer(systemId, playerId).ToList<StationInfo>())
			{
				Dictionary<ModuleEnums.StationModuleType, int> dictionary = GameSession.CountStationModuleTypes(station, this._db, true);
				switch (station.DesignInfo.StationType)
				{
					case StationType.CIVILIAN:
						if (isLocalTrade && dictionary.ContainsKey(ModuleEnums.StationModuleType.Habitation))
							num += (float)dictionary[ModuleEnums.StationModuleType.Habitation] * 0.01f;
						if (!isLocalTrade && dictionary.ContainsKey(ModuleEnums.StationModuleType.AlienHabitation))
							num += (float)dictionary[ModuleEnums.StationModuleType.AlienHabitation] * 0.02f;
						if (isLocalTrade && dictionary.ContainsKey(ModuleEnums.StationModuleType.LargeHabitation))
							num += (float)dictionary[ModuleEnums.StationModuleType.LargeHabitation] * 0.05f;
						if (!isLocalTrade && dictionary.ContainsKey(ModuleEnums.StationModuleType.LargeAlienHabitation))
						{
							num += (float)dictionary[ModuleEnums.StationModuleType.LargeAlienHabitation] * 0.1f;
							continue;
						}
						continue;
					case StationType.DIPLOMATIC:
						if (dictionary.ContainsKey(ModuleEnums.StationModuleType.Customs))
						{
							num += (float)dictionary[ModuleEnums.StationModuleType.Customs] * 0.01f;
							continue;
						}
						continue;
					default:
						continue;
				}
			}
			return num;
		}

		private int GetStationTradeRouteCapabilityBonus(int playerId, int systemId)
		{
			int num = 0;
			foreach (StationInfo station in this._db.GetStationForSystemAndPlayer(systemId, playerId).ToList<StationInfo>())
			{
				Dictionary<ModuleEnums.StationModuleType, int> dictionary = GameSession.CountStationModuleTypes(station, this._db, true);
				if (station.DesignInfo.StationType == StationType.CIVILIAN && dictionary.ContainsKey(ModuleEnums.StationModuleType.Dock))
					num += dictionary[ModuleEnums.StationModuleType.Dock];
			}
			return num;
		}

		public int GetExportCapacity(int SystemId)
		{
			int num = 0;
			foreach (ColonyInfo ci in this._db.GetColonyInfosForSystem(SystemId).ToList<ColonyInfo>())
				num = (int)Math.Round(this._db.GetTotalPopulation(ci) * Math.Round((double)ci.TradeRate, 4) / (double)this.AssetDatabase.PopulationPerTradePoint, 1) + num;
			return num;
		}

		public List<double> GetTradeRatesForWholeExports(int SystemId)
		{
			List<double> doubleList = new List<double>();
			int maxExportCapacity = this.GetMaxExportCapacity(SystemId);
			if (maxExportCapacity > 0)
			{
				double num1 = 100.0 / (double)maxExportCapacity;
				int num2 = (int)(100.0 / num1);
				for (int index = 0; index <= num2; ++index)
				{
					double num3 = num1 * (double)index / 100.0;
					doubleList.Add(num3);
				}
			}
			return doubleList;
		}

		public double GetColonyExportCapacity(int ColonyID)
		{
			double num = 0.0;
			ColonyInfo colonyInfo = this._db.GetColonyInfo(ColonyID);
			if (colonyInfo != null)
				num += this._db.GetTotalPopulation(colonyInfo) * 1.0 / (double)this.AssetDatabase.PopulationPerTradePoint;
			return num;
		}

		public List<double> GetTradeRatesForWholeExportsForColony(int ColonyId)
		{
			List<double> doubleList = new List<double>();
			double colonyExportCapacity = this.GetColonyExportCapacity(ColonyId);
			if (colonyExportCapacity > 0.0)
			{
				double num1 = 100.0 / colonyExportCapacity;
				int num2 = (int)(100.0 / num1);
				for (int index = 0; index <= num2; ++index)
				{
					double num3 = num1 * (double)index / 100.0;
					doubleList.Add(num3);
				}
			}
			return doubleList;
		}

		public int GetMaxExportCapacity(int SystemId)
		{
			int num = 0;
			foreach (ColonyInfo ci in this._db.GetColonyInfosForSystem(SystemId).ToList<ColonyInfo>())
				num += (int)(this._db.GetTotalPopulation(ci) * 1.0 / (double)this.AssetDatabase.PopulationPerTradePoint);
			return num;
		}

		private Dictionary<int, int> GetImportCapacity(int SystemId)
		{
			Dictionary<int, int> dictionary1 = new Dictionary<int, int>();
			foreach (ColonyInfo ci in this._db.GetColonyInfosForSystem(SystemId).ToList<ColonyInfo>())
			{
				if (!dictionary1.ContainsKey(ci.PlayerID))
					dictionary1.Add(ci.PlayerID, 0);
				double totalPopulation = this._db.GetTotalPopulation(ci);
				Dictionary<int, int> dictionary2;
				int playerId;
				(dictionary2 = dictionary1)[playerId = ci.PlayerID] = dictionary2[playerId] + (int)Math.Ceiling(totalPopulation / (double)this.AssetDatabase.PopulationPerTradePoint);
			}
			return dictionary1;
		}

		private bool IsStationInterdicted(StationInfo info)
		{
			foreach (MissionInfo missionInfo in this._db.GetMissionsByPlanetDest(info.OrbitalObjectID))
			{
				if (missionInfo.Type == MissionType.INTERDICTION)
				{
					FleetInfo fleetInfo = this._db.GetFleetInfo(missionInfo.FleetID);
					if (this._db.GetDiplomacyStateBetweenPlayers(info.PlayerID, fleetInfo.PlayerID) == DiplomacyState.WAR)
						return true;
				}
			}
			return false;
		}

		private int GetTradeDockCapacity(StationInfo si)
		{
			int num = 1;
			Dictionary<ModuleEnums.StationModuleType, int> dictionary = GameSession.CountStationModuleTypes(si, this._db, true);
			if (dictionary.ContainsKey(ModuleEnums.StationModuleType.Dock))
				num += dictionary[ModuleEnums.StationModuleType.Dock];
			return num;
		}

		private int GetFreighterCapacity(int systemId)
		{
			int num = 0;
			foreach (FreighterInfo freighterInfo in this._db.GetFreighterInfosForSystem(systemId).ToList<FreighterInfo>())
			{
				foreach (DesignSectionInfo designSection in freighterInfo.Design.DesignSections)
				{
					DesignSectionInfo dsi = designSection;
					ShipSectionAsset shipSectionAsset = this.AssetDatabase.ShipSections.FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == dsi.FilePath));
					num += shipSectionAsset.FreighterSpace;
				}
			}
			return num;
		}

		private int GetDockExportCapacity(int numDocks, int systemId)
		{
			List<FreighterInfo> list = this.App.GameDatabase.GetFreighterInfosForSystem(systemId).ToList<FreighterInfo>();
			Dictionary<FreighterInfo, int> source = new Dictionary<FreighterInfo, int>();
			foreach (FreighterInfo key in list)
				source.Add(key, ((IEnumerable<DesignSectionInfo>)key.Design.DesignSections).Sum<DesignSectionInfo>((Func<DesignSectionInfo, int>)(x => x.ShipSectionAsset.FreighterSpace)));
			Dictionary<FreighterInfo, int> dictionary = source.OrderByDescending<KeyValuePair<FreighterInfo, int>, int>((Func<KeyValuePair<FreighterInfo, int>, int>)(x => x.Value)).ToDictionary<KeyValuePair<FreighterInfo, int>, FreighterInfo, int>((Func<KeyValuePair<FreighterInfo, int>, FreighterInfo>)(y => y.Key), (Func<KeyValuePair<FreighterInfo, int>, int>)(y => y.Value));
			int num = 0;
			for (int index = 0; index < numDocks && dictionary.Count != 0; ++index)
			{
				num += dictionary.First<KeyValuePair<FreighterInfo, int>>().Value;
				dictionary.Remove(dictionary.First<KeyValuePair<FreighterInfo, int>>().Key);
			}
			return num;
		}

		private Dictionary<int, double> Trade(out TradeResultsTable tradeResult)
		{
			List<TreatyInfo> list1 = this._db.GetTreatyInfos().ToList<TreatyInfo>();
			int turn = this._db.GetTurnCount();
			List<TreatyInfo> list2 = list1.Where<TreatyInfo>((Func<TreatyInfo, bool>)(x =>
		   {
			   if (x.Type == TreatyType.Trade && x.StartingTurn < turn && x.StartingTurn + x.Duration > turn)
				   return x.Active;
			   return false;
		   })).ToList<TreatyInfo>();
			tradeResult = new TradeResultsTable();
			Dictionary<int, double> dictionary1 = new Dictionary<int, double>();
			List<StationInfo> list3 = this._db.GetStationInfos().ToList<StationInfo>().Where<StationInfo>((Func<StationInfo, bool>)(x =>
		   {
			   if (x.DesignInfo.StationType == StationType.CIVILIAN)
				   return x.DesignInfo.StationLevel > 0;
			   return false;
		   })).ToList<StationInfo>();
			if (list3.Count == 0)
				return dictionary1;
			Dictionary<int, Dictionary<int, double>> dictionary2 = new Dictionary<int, Dictionary<int, double>>();
			List<GameSession.ExportNode> exportNodeList = new List<GameSession.ExportNode>();
			foreach (StationInfo stationInfo in list3)
			{
				if (!this.IsStationInterdicted(stationInfo))
				{
					int tradeDockCapacity = this.GetTradeDockCapacity(stationInfo);
					StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(this._db.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID).StarSystemID);
					int val1 = this.GetExportCapacity(starSystemInfo.ID) + stationInfo.WarehousedGoods;
					float num = this._db.FindCurrentDriveSpeedForPlayer(stationInfo.PlayerID) * this.GameDatabase.GetStratModifierFloatToApply(StratModifiers.TradeRangeModifier, stationInfo.PlayerID);
					int freighterCapacity = this.GetFreighterCapacity(starSystemInfo.ID);
					int dockExportCapacity = this.GetDockExportCapacity(tradeDockCapacity, starSystemInfo.ID);
					exportNodeList.Add(new GameSession.ExportNode()
					{
						System = starSystemInfo,
						Station = stationInfo,
						ExportPoints = Math.Min(val1, dockExportCapacity),
						Range = num
					});
					if (!tradeResult.TradeNodes.ContainsKey(starSystemInfo.ID))
					{
						tradeResult.TradeNodes.Add(starSystemInfo.ID, new TradeNode()
						{
							Produced = val1,
							ProductionCapacity = this.GetMaxExportCapacity(starSystemInfo.ID),
							Freighters = freighterCapacity,
							DockCapacity = tradeDockCapacity,
							DockExportCapacity = dockExportCapacity,
							Range = num
						});
					}
					else
					{
						tradeResult.TradeNodes[starSystemInfo.ID].Produced = val1;
						tradeResult.TradeNodes[starSystemInfo.ID].ProductionCapacity = this.GetMaxExportCapacity(starSystemInfo.ID);
						tradeResult.TradeNodes[starSystemInfo.ID].Freighters = freighterCapacity;
						tradeResult.TradeNodes[starSystemInfo.ID].DockCapacity = tradeDockCapacity;
						tradeResult.TradeNodes[starSystemInfo.ID].DockExportCapacity = dockExportCapacity;
						tradeResult.TradeNodes[starSystemInfo.ID].Range = num;
					}
				}
			}
			List<StarSystemInfo> list4 = this._db.GetStarSystemInfos().ToList<StarSystemInfo>();
			List<GameSession.ImportNode> source = new List<GameSession.ImportNode>();
			foreach (StarSystemInfo starSystemInfo in list4)
			{
				Dictionary<int, int> importCapacity = this.GetImportCapacity(starSystemInfo.ID);
				source.Add(new GameSession.ImportNode()
				{
					System = starSystemInfo,
					ImportCapacity = importCapacity
				});
				if (!tradeResult.TradeNodes.ContainsKey(starSystemInfo.ID))
				{
					Dictionary<int, TradeNode> tradeNodes = tradeResult.TradeNodes;
					int id = starSystemInfo.ID;
					TradeNode tradeNode1 = new TradeNode();
					tradeNode1.Consumption = importCapacity.Sum<KeyValuePair<int, int>>((Func<KeyValuePair<int, int>, int>)(x => x.Value));
					int key = id;
					TradeNode tradeNode2 = tradeNode1;
					tradeNodes.Add(key, tradeNode2);
				}
				else
					tradeResult.TradeNodes[starSystemInfo.ID].Consumption = importCapacity.Sum<KeyValuePair<int, int>>((Func<KeyValuePair<int, int>, int>)(x => x.Value));
			}
			foreach (GameSession.ExportNode exportNode in exportNodeList)
			{
				GameSession.ExportNode en = exportNode;
				if (this.AssetDatabase.GetFaction(this._db.GetPlayerFactionID(en.Station.PlayerID)).CanUseGate())
					en.GenericSystems = source.Where<GameSession.ImportNode>((Func<GameSession.ImportNode, bool>)(x =>
				   {
					   if (x.System != en.System)
						   return this._db.SystemHasGate(x.System.ID, en.Station.PlayerID);
					   return false;
				   })).ToList<GameSession.ImportNode>();
				en.GenericSystems.AddRange((IEnumerable<GameSession.ImportNode>)source.Where<GameSession.ImportNode>((Func<GameSession.ImportNode, bool>)(x =>
			  {
				  if (x.System != en.System)
					  return (double)(x.System.Origin - en.System.Origin).Length < (double)en.Range;
				  return false;
			  })).ToList<GameSession.ImportNode>());
				en.InternationalSystems = en.GenericSystems.Where<GameSession.ImportNode>((Func<GameSession.ImportNode, bool>)(x => x.ImportCapacity.Keys.Any<int>((Func<int, bool>)(y => y != en.Station.PlayerID)))).ToList<GameSession.ImportNode>();
				en.InterprovincialSystems = en.System.ProvinceID.HasValue ? en.GenericSystems.Where<GameSession.ImportNode>((Func<GameSession.ImportNode, bool>)(x =>
			   {
				   int? provinceId1 = x.System.ProvinceID;
				   int? provinceId2 = en.System.ProvinceID;
				   return !(provinceId1.GetValueOrDefault() == provinceId2.GetValueOrDefault() & provinceId1.HasValue == provinceId2.HasValue);
			   })).ToList<GameSession.ImportNode>() : new List<GameSession.ImportNode>();
				en.GenericSystems.RemoveAll((Predicate<GameSession.ImportNode>)(x =>
			   {
				   if (!en.InternationalSystems.Contains(x))
					   return en.InterprovincialSystems.Contains(x);
				   return true;
			   }));
			}
			for (int index1 = 0; index1 < exportNodeList.Count; ++index1)
			{
				int index2 = this._random.Next(exportNodeList.Count);
				GameSession.ExportNode exportNode = exportNodeList[index2];
				exportNodeList[index2] = exportNodeList[index1];
				exportNodeList[index1] = exportNode;
			}
			using (List<GameSession.ExportNode>.Enumerator enumerator = exportNodeList.GetEnumerator())
			{
			label_54:
				while (enumerator.MoveNext())
				{
					GameSession.ExportNode en = enumerator.Current;
					while (true)
					{
						if (en.ExportPoints > 0 && en.InternationalSystems.Count > 0)
						{
							GameSession.ImportNode importNode = en.InternationalSystems.First<GameSession.ImportNode>();
							foreach (KeyValuePair<int, int> keyValuePair1 in importNode.ImportCapacity)
							{
								KeyValuePair<int, int> kvp = keyValuePair1;
								if (kvp.Key != en.Station.PlayerID && list2.Any<TreatyInfo>((Func<TreatyInfo, bool>)(x =>
							   {
								   if (x.InitiatingPlayerId == kvp.Key && x.ReceivingPlayerId == en.Station.PlayerID)
									   return true;
								   if (x.ReceivingPlayerId == kvp.Key)
									   return x.InitiatingPlayerId == en.Station.PlayerID;
								   return false;
							   })))
								{
									int num1 = Math.Min(importNode.ImportCapacity[kvp.Key], en.ExportPoints);
									en.ExportPoints -= num1;
									if (tradeResult.TradeNodes.ContainsKey(en.System.ID))
										tradeResult.TradeNodes[en.System.ID].ExportInt += num1;
									else
										tradeResult.TradeNodes.Add(en.System.ID, new TradeNode()
										{
											ExportInt = num1
										});
									if (tradeResult.TradeNodes.ContainsKey(importNode.System.ID))
										tradeResult.TradeNodes[importNode.System.ID].ImportInt += num1;
									else
										tradeResult.TradeNodes.Add(importNode.System.ID, new TradeNode()
										{
											ImportInt = num1
										});
									float num2 = (float)num1 * this.AssetDatabase.IncomePerInternationalTradePointMoved;
									if (importNode.System.IsOpen)
										num2 *= 1.1f;
									float num3 = num2 * 0.7f;
									float num4 = num2 * 0.3f;
									float num5 = num3 * this.GetStationTradeModifierForSystem(en.Station.PlayerID, en.System.ID, false);
									float num6 = num4 * this.GetStationTradeModifierForSystem(kvp.Key, importNode.System.ID, false);
									this.GameDatabase.UpdatePlayerViewWithStarSystem(en.Station.PlayerID, importNode.System.ID);
									GameDatabase gameDatabase = this.GameDatabase;
									KeyValuePair<int, int> keyValuePair2 = kvp;
									int key1 = keyValuePair2.Key;
									int id = importNode.System.ID;
									gameDatabase.UpdatePlayerViewWithStarSystem(key1, id);
									if (!dictionary1.ContainsKey(en.Station.PlayerID))
									{
										dictionary1.Add(en.Station.PlayerID, (double)num5);
									}
									else
									{
										Dictionary<int, double> dictionary3;
										int playerId;
										(dictionary3 = dictionary1)[playerId = en.Station.PlayerID] = dictionary3[playerId] + (double)num5;
									}
									Dictionary<int, double> dictionary4 = dictionary1;
									keyValuePair2 = kvp;
									int key2 = keyValuePair2.Key;
									if (!dictionary4.ContainsKey(key2))
									{
										Dictionary<int, double> dictionary3 = dictionary1;
										keyValuePair2 = kvp;
										int key3 = keyValuePair2.Key;
										double num7 = (double)num6;
										dictionary3.Add(key3, num7);
									}
									else
									{
										Dictionary<int, double> dictionary3;
										Dictionary<int, double> dictionary5 = dictionary3 = dictionary1;
										keyValuePair2 = kvp;
										int key3;
										int index = key3 = keyValuePair2.Key;
										double num7 = dictionary5[index] + (double)num6;
										dictionary3[key3] = num7;
									}
									if (!dictionary2.ContainsKey(en.Station.PlayerID))
									{
										dictionary2.Add(en.Station.PlayerID, new Dictionary<int, double>());
										dictionary2[en.Station.PlayerID].Add(keyValuePair2.Key, (double)num6);
									}
									else
										dictionary2[en.Station.PlayerID][keyValuePair2.Key] = !dictionary2[en.Station.PlayerID].ContainsKey(keyValuePair2.Key) ? (double)num6 : dictionary2[en.Station.PlayerID][keyValuePair2.Key] + (double)num6;
								}
							}
							en.InternationalSystems.Remove(importNode);
						}
						else
							goto label_54;
					}
				}
			}
			exportNodeList.RemoveAll((Predicate<GameSession.ExportNode>)(x => x.ExportPoints == 0));
			using (List<GameSession.ExportNode>.Enumerator enumerator = exportNodeList.GetEnumerator())
			{
			label_74:
				while (enumerator.MoveNext())
				{
					GameSession.ExportNode current = enumerator.Current;
					while (true)
					{
						if (current.ExportPoints > 0 && current.InterprovincialSystems.Count > 0)
						{
							GameSession.ImportNode importNode = current.InterprovincialSystems.First<GameSession.ImportNode>();
							int num1 = 0;
							if (importNode.ImportCapacity.ContainsKey(current.Station.PlayerID))
								num1 = Math.Min(importNode.ImportCapacity[current.Station.PlayerID], current.ExportPoints);
							current.ExportPoints -= num1;
							if (tradeResult.TradeNodes.ContainsKey(current.System.ID))
								tradeResult.TradeNodes[current.System.ID].ExportProv += num1;
							else
								tradeResult.TradeNodes.Add(current.System.ID, new TradeNode()
								{
									ExportProv = num1
								});
							if (tradeResult.TradeNodes.ContainsKey(importNode.System.ID))
								tradeResult.TradeNodes[importNode.System.ID].ImportProv += num1;
							else
								tradeResult.TradeNodes.Add(importNode.System.ID, new TradeNode()
								{
									ImportProv = num1
								});
							float num2 = (float)num1 * this.AssetDatabase.IncomePerProvincialTradePointMoved * this.GetStationTradeModifierForSystem(current.Station.PlayerID, current.System.ID, true);
							if (importNode.System.IsOpen)
								num2 *= 1.1f;
							if (!dictionary1.ContainsKey(current.Station.PlayerID))
							{
								dictionary1.Add(current.Station.PlayerID, (double)num2);
							}
							else
							{
								Dictionary<int, double> dictionary3;
								int playerId;
								(dictionary3 = dictionary1)[playerId = current.Station.PlayerID] = dictionary3[playerId] + (double)num2;
							}
							current.InterprovincialSystems.Remove(importNode);
						}
						else
							goto label_74;
					}
				}
			}
			exportNodeList.RemoveAll((Predicate<GameSession.ExportNode>)(x => x.ExportPoints == 0));
			using (List<GameSession.ExportNode>.Enumerator enumerator = exportNodeList.GetEnumerator())
			{
			label_94:
				while (enumerator.MoveNext())
				{
					GameSession.ExportNode current = enumerator.Current;
					while (true)
					{
						if (current.ExportPoints > 0 && current.GenericSystems.Count > 0)
						{
							GameSession.ImportNode importNode = current.GenericSystems.First<GameSession.ImportNode>();
							int num1 = 0;
							if (importNode.ImportCapacity.ContainsKey(current.Station.PlayerID))
								num1 = Math.Min(importNode.ImportCapacity[current.Station.PlayerID], current.ExportPoints);
							current.ExportPoints -= num1;
							if (tradeResult.TradeNodes.ContainsKey(current.System.ID))
								tradeResult.TradeNodes[current.System.ID].ExportLoc += num1;
							else
								tradeResult.TradeNodes.Add(current.System.ID, new TradeNode()
								{
									ExportLoc = num1
								});
							if (tradeResult.TradeNodes.ContainsKey(importNode.System.ID))
								tradeResult.TradeNodes[importNode.System.ID].ImportLoc += num1;
							else
								tradeResult.TradeNodes.Add(importNode.System.ID, new TradeNode()
								{
									ImportLoc = num1
								});
							float num2 = (float)num1 * this.AssetDatabase.IncomePerGenericTradePointMoved * this.GetStationTradeModifierForSystem(current.Station.PlayerID, current.System.ID, true);
							if (importNode.System.IsOpen)
								num2 *= 1.1f;
							if (!dictionary1.ContainsKey(current.Station.PlayerID))
							{
								dictionary1.Add(current.Station.PlayerID, (double)num2);
							}
							else
							{
								Dictionary<int, double> dictionary3;
								int playerId;
								(dictionary3 = dictionary1)[playerId = current.Station.PlayerID] = dictionary3[playerId] + (double)num2;
							}
							current.GenericSystems.Remove(importNode);
						}
						else
							goto label_94;
					}
				}
			}
			exportNodeList.RemoveAll((Predicate<GameSession.ExportNode>)(x => x.ExportPoints == 0));
			foreach (GameSession.ExportNode exportNode in exportNodeList)
			{
				Dictionary<ModuleEnums.StationModuleType, int> dictionary3 = GameSession.CountStationModuleTypes(exportNode.Station, this._db, true);
				exportNode.Station.WarehousedGoods = !dictionary3.ContainsKey(ModuleEnums.StationModuleType.Warehouse) ? 0 : Math.Min((int)((double)dictionary3[ModuleEnums.StationModuleType.Warehouse] * (double)this._db.GetStratModifierFloatToApply(StratModifiers.WarehouseCapacityModifier, exportNode.Station.PlayerID)), exportNode.ExportPoints);
				exportNode.ExportPoints -= exportNode.Station.WarehousedGoods;
				foreach (ColonyInfo colony in this._db.GetColonyInfosForSystem(exportNode.System.ID).ToList<ColonyInfo>())
				{
					colony.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.SpoiledGoods, exportNode.ExportPoints);
					this._db.UpdateColony(colony);
				}
			}
			foreach (KeyValuePair<int, Dictionary<int, double>> keyValuePair1 in dictionary2)
			{
				foreach (KeyValuePair<int, double> keyValuePair2 in keyValuePair1.Value)
				{
					if (this.GetPlayerObject(keyValuePair2.Key).IsAI() && this.GetPlayerObject(keyValuePair2.Key).IsStandardPlayer)
					{
						DiplomacyInfo diplomacyInfo = this._db.GetDiplomacyInfo(keyValuePair2.Key, keyValuePair1.Key);
						diplomacyInfo.Relations += Math.Min((int)Math.Max(keyValuePair2.Value / 20000.0, 100.0), 5);
						int reactionAmount = (int)((double)diplomacyInfo.Relations * ((this._db.GetPlayerFaction(keyValuePair1.Key).Name == "morrigi" ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") * 0.5 : 0.0) + (this._db.PlayerHasTech(keyValuePair1.Key, "PSI_Lesser_Glamour") ? (double)this._db.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") : 0.0) + 1.0));
						diplomacyInfo.Relations += reactionAmount;
						this._db.ApplyDiplomacyReaction(keyValuePair2.Key, keyValuePair1.Key, reactionAmount, new StratModifiers?(StratModifiers.DiplomacyReactionTrade), 1);
					}
				}
			}
			return dictionary1;
		}

		public bool DetectedIncomingRandom(int playerid, int systemid)
		{
			StationInfo systemPlayerAndType = this.GameDatabase.GetStationForSystemPlayerAndType(systemid, playerid, StationType.SCIENCE);
			if (systemPlayerAndType == null)
				return false;
			int num = 0;
			foreach (DesignSectionInfo designSection in systemPlayerAndType.DesignInfo.DesignSections)
				num += designSection.Modules.Where<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x =>
			   {
				   if (!x.StationModuleType.HasValue)
					   return false;
				   ModuleEnums.StationModuleType? stationModuleType = x.StationModuleType;
				   if (stationModuleType.GetValueOrDefault() == ModuleEnums.StationModuleType.Sensor)
					   return stationModuleType.HasValue;
				   return false;
			   })).Count<DesignModuleInfo>();
			return App.GetSafeRandom().Next(0, 100) < num * 10;
		}

		public List<PendingCombat> GetPendingCombats()
		{
			return this.m_Combats;
		}

		public void OrderCombatsByResponse()
		{
			if (this.App.GameSetup.IsMultiplayer)
				return;
			this.m_Combats = this.m_Combats.OrderBy<PendingCombat, bool>((Func<PendingCombat, bool>)(x =>
		   {
			   if (x.PlayersInCombat.Contains(this.App.LocalPlayer.ID) && !this.m_Combats.Any<PendingCombat>((Func<PendingCombat, bool>)(y =>
		 {
			 if (y.SystemID == x.SystemID && y.PlayersInCombat.Contains(this.App.LocalPlayer.ID))
				 return y.CombatResolutionSelections[this.App.LocalPlayer.ID] == ResolutionType.AUTO_RESOLVE;
			 return false;
		 })))
				   return x.CombatResolutionSelections[this.App.LocalPlayer.ID] == ResolutionType.AUTO_RESOLVE;
			   return true;
		   })).ToList<PendingCombat>();
		}

		public PendingCombat GetPendingCombatBySystemID(int systemId)
		{
			return this.m_Combats.First<PendingCombat>((Func<PendingCombat, bool>)(x => x.SystemID == systemId));
		}

		public PendingCombat GetPendingCombatByUniqueID(int Id)
		{
			return this.m_Combats.First<PendingCombat>((Func<PendingCombat, bool>)(x => x.ConflictID == Id));
		}

		public PendingCombat GetCurrentCombat()
		{
			return this._currentCombat;
		}

		public List<ReactionInfo> GetPendingReactions()
		{
			return this._reactions;
		}

		public void SetPendingReactions(List<ReactionInfo> reactions)
		{
			this._reactions = reactions;
		}

		public ReactionInfo GetNextReactionForPlayer(int playerID)
		{
			foreach (ReactionInfo reaction in this._reactions)
			{
				if (reaction.fleet.PlayerID == playerID)
					return reaction;
			}
			return (ReactionInfo)null;
		}

		public void RemoveReaction(ReactionInfo info)
		{
			this._reactions.Remove(info);
		}

		public void CombatComplete()
		{
			if (this._currentCombat != null)
			{
				foreach (int index in this._currentCombat.PlayersInCombat)
				{
					if (index < this.App.GameSetup.Players.Count<PlayerSetup>() && index > 0 && !this.App.GameSetup.IsMultiplayer)
						this.App.GameSetup.Players[index].Status = NPlayerStatus.PS_TURN;
				}
				this._currentCombat = (PendingCombat)null;
			}
			if (!this.m_Combats.Any<PendingCombat>((Func<PendingCombat, bool>)(x => x.CombatResults == null)))
				this.m_Combats.Clear();
			else if (!this.App.GameSetup.IsMultiplayer)
			{
				this.LaunchNextCombat();
				return;
			}
			if (this.App.GameSetup.IsMultiplayer)
				return;
			this._app.Game.NextTurn();
		}

		public MultiCombatCarryOverData MCCarryOverData
		{
			get
			{
				return this.m_MCCarryOverData;
			}
		}

		public OpenCloseSystemToggleData OCSystemToggleData
		{
			get
			{
				return this.m_OCSystemToggleData;
			}
		}

		public float GetIdealSuitability(Faction faction)
		{
			return this.m_SpeciesIdealSuitability[faction];
		}

		private Dictionary<ShipRole, List<DesignInfo>> GetFactionShipDesigns(
		  IList<ShipRole> roles,
		  IEnumerable<DesignInfo> initialDesigns,
		  Kerberos.Sots.PlayerFramework.Player player)
		{
			Dictionary<ShipRole, List<DesignInfo>> dictionary = new Dictionary<ShipRole, List<DesignInfo>>();
			foreach (ShipRole role1 in (IEnumerable<ShipRole>)roles)
			{
				ShipRole role = role1;
				DesignInfo designInfo = initialDesigns.FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.Role == role));
				if (designInfo == null)
				{
					string optionalName;
					switch (role)
					{
						case ShipRole.COMMAND:
							optionalName = App.Localize("@DEFAULT_SHIPNAME_COMMAND");
							break;
						case ShipRole.COLONIZER:
							optionalName = App.Localize("@DEFAULT_SHIPNAME_COLONIZER");
							break;
						case ShipRole.CONSTRUCTOR:
							optionalName = App.Localize("@DEFAULT_SHIPNAME_CONSTRUCTOR");
							break;
						case ShipRole.SUPPLY:
							optionalName = App.Localize("@DEFAULT_SHIPNAME_SUPPLY");
							break;
						case ShipRole.GATE:
							optionalName = App.Localize("@DEFAULT_SHIPNAME_GATE");
							break;
						case ShipRole.BORE:
							optionalName = App.Localize("@DEFAULT_SHIPNAME_BORE");
							break;
						case ShipRole.FREIGHTER:
							optionalName = App.Localize("@DEFAULT_SHIPNAME_FREIGHTER");
							break;
						case ShipRole.ACCELERATOR_GATE:
							optionalName = App.Localize("@DEFAULT_SHIPNAME_ACCELERATOR");
							break;
						case ShipRole.LOA_CUBE:
							optionalName = App.Localize("@DEFAULT_SHIPNAME_LOA_CUBE");
							break;
						default:
							optionalName = null;
							break;
					}
					designInfo = DesignLab.SetDefaultDesign(this, role, new WeaponRole?(), player.ID, optionalName, new bool?(true), (AITechStyles)null, new AIStance?());
				}
				if (designInfo != null)
				{
					List<DesignInfo> designInfoList;
					if (!dictionary.TryGetValue(role, out designInfoList))
					{
						designInfoList = new List<DesignInfo>();
						dictionary[role] = designInfoList;
					}
					designInfoList.Add(designInfo);
				}
			}
			return dictionary;
		}

		public Dictionary<int, int> GetFleetDesignsFromTemplate(
		  Kerberos.Sots.PlayerFramework.Player player,
		  string templateName)
		{
			Dictionary<int, int> dictionary1 = new Dictionary<int, int>();
			FleetTemplate template = this._db.AssetDatabase.FleetTemplates.FirstOrDefault<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == templateName));
			if (template == null)
				return dictionary1;
			foreach (ShipInclude shipInclude in template.ShipIncludes)
			{
				if ((string.IsNullOrEmpty(shipInclude.Faction) || !(shipInclude.Faction != player.Faction.Name)) && !(shipInclude.FactionExclusion == player.Faction.Name))
				{
					DesignInfo fillDesign = DesignLab.GetBestDesignByRole(this, player, new AIStance?(), shipInclude.ShipRole, StrategicAI.GetEquivilantShipRoles(shipInclude.ShipRole), shipInclude.WeaponRole) ?? DesignLab.GetDesignByRole(this, player, (AITechStyles)null, new AIStance?(), shipInclude.ShipRole, shipInclude.WeaponRole);
					if (fillDesign != null)
					{
						int num;
						if (shipInclude.InclusionType == ShipInclusionType.FILL)
						{
							DesignInfo commandDesign = DesignLab.GetBestDesignByRole(this, player, new AIStance?(), ShipRole.COMMAND, StrategicAI.GetEquivilantShipRoles(ShipRole.COMMAND), new WeaponRole?()) ?? DesignLab.GetDesignByRole(this, player, (AITechStyles)null, new AIStance?(), ShipRole.COMMAND, new WeaponRole?());
							num = DesignLab.GetTemplateFillAmount(this._db, template, commandDesign, fillDesign);
						}
						else
							num = shipInclude.Amount;
						if (dictionary1.ContainsKey(fillDesign.ID))
						{
							Dictionary<int, int> dictionary2;
							int id;
							(dictionary2 = dictionary1)[id = fillDesign.ID] = dictionary2[id] + num;
						}
						else
							dictionary1.Add(fillDesign.ID, num);
					}
				}
			}
			return dictionary1;
		}

		private int CreateFleetFromTemplate(
		  Kerberos.Sots.PlayerFramework.Player player,
		  string fleetName,
		  string templateName,
		  int admiralID,
		  int systemID)
		{
			FleetTemplate template = this._db.AssetDatabase.FleetTemplates.FirstOrDefault<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == templateName));
			if (template == null)
				return 0;
			AIFleetInfo fleetInfo = new AIFleetInfo();
			fleetInfo.AdmiralID = new int?(admiralID);
			fleetInfo.FleetType = MissionTypeExtensions.SerializeList(template.MissionTypes);
			fleetInfo.SystemID = systemID;
			fleetInfo.FleetTemplate = template.Name;
			fleetInfo.FleetID = new int?(this._db.InsertFleet(player.ID, admiralID, systemID, systemID, fleetName, FleetType.FL_NORMAL));
			int num1 = this._db.InsertAIFleetInfo(player.ID, fleetInfo);
			foreach (ShipInclude shipInclude in template.ShipIncludes)
			{
				if ((string.IsNullOrEmpty(shipInclude.Faction) || !(shipInclude.Faction != player.Faction.Name)) && !(shipInclude.FactionExclusion == player.Faction.Name))
				{
					DesignInfo fillDesign = DesignLab.GetBestDesignByRole(this, player, new AIStance?(), shipInclude.ShipRole, StrategicAI.GetEquivilantShipRoles(shipInclude.ShipRole), shipInclude.WeaponRole) ?? DesignLab.GetDesignByRole(this, player, (AITechStyles)null, new AIStance?(), shipInclude.ShipRole, shipInclude.WeaponRole);
					int num2;
					if (shipInclude.InclusionType == ShipInclusionType.FILL)
					{
						DesignInfo commandDesign = DesignLab.GetBestDesignByRole(this, player, new AIStance?(), ShipRole.COMMAND, StrategicAI.GetEquivilantShipRoles(ShipRole.COMMAND), new WeaponRole?()) ?? DesignLab.GetDesignByRole(this, player, (AITechStyles)null, new AIStance?(), ShipRole.COMMAND, new WeaponRole?());
						num2 = DesignLab.GetTemplateFillAmount(this._db, template, commandDesign, fillDesign);
					}
					else
						num2 = shipInclude.Amount;
					for (int index = 0; index < num2; ++index)
					{
						if (fillDesign != null)
						{
							int shipsBuiltFromDesign = this._db.GetNumShipsBuiltFromDesign(fillDesign.ID);
							string defaultShipName = this._db.GetDefaultShipName(fillDesign.ID, shipsBuiltFromDesign);
							string shipName = this._db.ResolveNewShipName(player.ID, defaultShipName);
							int carrierID = this._db.InsertShip(fleetInfo.FleetID.Value, fillDesign.ID, shipName, (ShipParams)0, new int?(num1), 0);
							this.AddDefaultStartingRiders(fleetInfo.FleetID.Value, fillDesign.ID, carrierID);
						}
					}
				}
			}
			return fleetInfo.FleetID.Value;
		}

		private void SetRequiredDefaultDesigns()
		{
			foreach (int standardPlayerId in this.GameDatabase.GetStandardPlayerIDs())
			{
				Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(standardPlayerId);
				foreach (ShipRole battleRiderShipRole in playerObject.Faction.DefaultBattleRiderShipRoles)
					DesignLab.SetDefaultDesign(this, battleRiderShipRole, new WeaponRole?(), playerObject.ID, null, new bool?(), playerObject.TechStyles, playerObject.Stance);
			}
		}

		public void AddStartingDeployedShips(GameDatabase db, int playerid)
		{
			if (db.GetFactionName(db.GetPlayerFactionID(playerid)) == "hiver")
			{
				DesignInfo designInfo = db.GetDesignInfosForPlayer(playerid, RealShipClasses.Cruiser, true).FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => ((IEnumerable<DesignSectionInfo>)x.DesignSections).First<DesignSectionInfo>().ShipSectionAsset.IsGateShip));
				foreach (StarSystemInfo starSystemInfo in db.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
			   {
				   if (!db.GetSystemOwningPlayer(x.ID).HasValue)
					   return false;
				   int? systemOwningPlayer = db.GetSystemOwningPlayer(x.ID);
				   int num = playerid;
				   if (systemOwningPlayer.GetValueOrDefault() == num)
					   return systemOwningPlayer.HasValue;
				   return false;
			   })).ToList<StarSystemInfo>())
				{
					int num = db.InsertFleet(playerid, 0, starSystemInfo.ID, starSystemInfo.ID, "GATE", FleetType.FL_GATE);
					int shipID = db.InsertShip(num, designInfo.ID, "Gate", ShipParams.HS_GATE_DEPLOYED, new int?(), 0);
					Matrix? gateShipTransform = GameSession.GetValidGateShipTransform(this, starSystemInfo.ID, num);
					if (gateShipTransform.HasValue)
						db.UpdateShipSystemPosition(shipID, new Matrix?(gateShipTransform.Value));
				}
			}
			if (!(db.GetFactionName(db.GetPlayerFactionID(playerid)) == "loa"))
				return;
			DesignInfo designInfo1 = db.GetDesignInfosForPlayer(playerid, RealShipClasses.Cruiser, true).FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.IsAccelerator()));
			StarSystemInfo starSystemInfo1 = this._db.GetStarSystemInfo(this._db.GetHomeworlds().FirstOrDefault<HomeworldInfo>((Func<HomeworldInfo, bool>)(x => x.PlayerID == playerid)).SystemID);
			int num1 = db.InsertFleet(playerid, 0, starSystemInfo1.ID, starSystemInfo1.ID, "ACCELERATOR FLEET", FleetType.FL_ACCELERATOR);
			int shipID1 = db.InsertShip(num1, designInfo1.ID, "NPG", ShipParams.HS_GATE_DEPLOYED, new int?(), 0);
			Matrix? gateShipTransform1 = GameSession.GetValidGateShipTransform(this, starSystemInfo1.ID, num1);
			if (gateShipTransform1.HasValue)
				db.UpdateShipSystemPosition(shipID1, new Matrix?(gateShipTransform1.Value));
			foreach (StarSystemInfo starSystemInfo2 in db.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
		   {
			   if (!db.GetSystemOwningPlayer(x.ID).HasValue)
				   return false;
			   int? systemOwningPlayer = db.GetSystemOwningPlayer(x.ID);
			   int num2 = playerid;
			   if (systemOwningPlayer.GetValueOrDefault() == num2)
				   return systemOwningPlayer.HasValue;
			   return false;
		   })).ToList<StarSystemInfo>())
			{
				if (starSystemInfo2.ID != starSystemInfo1.ID && db.GetSystemOwningPlayer(starSystemInfo2.ID).HasValue)
				{
					int? systemOwningPlayer = db.GetSystemOwningPlayer(starSystemInfo2.ID);
					int num2 = playerid;
					if ((systemOwningPlayer.GetValueOrDefault() != num2 ? 1 : (!systemOwningPlayer.HasValue ? 1 : 0)) == 0)
					{
						foreach (Vector3 slotsBetweenSystem in Kerberos.Sots.StarFleet.StarFleet.GetAccelGateSlotsBetweenSystems(db, starSystemInfo1.ID, starSystemInfo2.ID))
						{
							int num3 = db.InsertStarSystem(new int?(), "Accelerator Node", new int?(), "Deepspace", slotsBetweenSystem, false, false, new int?());
							int num4 = db.InsertFleet(playerid, 0, num3, num3, "ACCELERATOR FLEET", FleetType.FL_ACCELERATOR);
							db.InsertShip(num4, designInfo1.ID, "NPG", ShipParams.HS_GATE_DEPLOYED, new int?(), 0);
							db.InsertMoveOrder(num4, num3, slotsBetweenSystem, starSystemInfo2.ID, starSystemInfo2.Origin);
							NodeLineInfo nodeLineInfo = db.GetNodeLineBetweenSystems(playerid, starSystemInfo1.ID, starSystemInfo2.ID, false, true);
							if (nodeLineInfo == null)
							{
								int id = db.InsertNodeLine(starSystemInfo1.ID, starSystemInfo2.ID, 100);
								nodeLineInfo = db.GetNodeLine(id);
							}
							db.InsertLoaLineFleetRecord(nodeLineInfo.ID, num4);
						}
						int num5 = db.InsertFleet(playerid, 0, starSystemInfo2.ID, starSystemInfo2.ID, "ACCELERATOR FLEET", FleetType.FL_ACCELERATOR);
						int shipID2 = db.InsertShip(num5, designInfo1.ID, "NPG", ShipParams.HS_GATE_DEPLOYED, new int?(), 0);
						NodeLineInfo nodeLineInfo1 = db.GetNodeLineBetweenSystems(playerid, starSystemInfo1.ID, starSystemInfo2.ID, false, true);
						if (nodeLineInfo1 == null)
						{
							int id = db.InsertNodeLine(starSystemInfo1.ID, starSystemInfo2.ID, 100);
							nodeLineInfo1 = db.GetNodeLine(id);
						}
						db.InsertLoaLineFleetRecord(nodeLineInfo1.ID, num5);
						Matrix? gateShipTransform2 = GameSession.GetValidGateShipTransform(this, starSystemInfo2.ID, num5);
						if (gateShipTransform2.HasValue)
							db.UpdateShipSystemPosition(shipID2, new Matrix?(gateShipTransform2.Value));
					}
				}
			}
		}

		public void AddDefaultStartingFleets(Kerberos.Sots.PlayerFramework.Player player)
		{
			List<DesignInfo> designInfoList = new List<DesignInfo>();
			if (player.Faction.InitialDesigns != null)
			{
				foreach (InitialDesign initialDesign in player.Faction.InitialDesigns)
				{
					AITechStyles optionalAITechStyles = (AITechStyles)null;
					if (!string.IsNullOrEmpty(initialDesign.WeaponBiasTechFamilyID))
						optionalAITechStyles = new AITechStyles(this.AssetDatabase, (IEnumerable<AITechStyleInfo>)new AITechStyleInfo[1]
						{
			  new AITechStyleInfo()
			  {
				CostFactor = 0.1f,
				PlayerID = player.ID,
				TechFamily = this.AssetDatabase.MasterTechTree.GetTechFamilyEnumFromName(initialDesign.WeaponBiasTechFamilyID)
			  }
						});
					DesignInfo initialShipDesign = DesignLab.CreateInitialShipDesign(this, initialDesign.Name, initialDesign.EnumerateShipSectionAssets(this.AssetDatabase, player.Faction), player.ID, optionalAITechStyles);
					designInfoList.Add(this._db.GetDesignInfo(this._db.InsertDesignByDesignInfo(initialShipDesign)));
				}
			}
			Dictionary<ShipRole, List<DesignInfo>> factionShipDesigns = this.GetFactionShipDesigns((IList<ShipRole>)player.Faction.DefaultCombinedShipRoles, (IEnumerable<DesignInfo>)designInfoList, player);
			Dictionary<ShipRole, int> dictionary = new Dictionary<ShipRole, int>();
			foreach (KeyValuePair<ShipRole, List<DesignInfo>> keyValuePair in factionShipDesigns)
				dictionary[keyValuePair.Key] = keyValuePair.Value[0].ID;
			int systemId = this.GameDatabase.GetPlayerHomeworld(player.ID).SystemID;
			int orbitalObjectId = this.GameDatabase.GetNavalStationForSystemAndPlayer(systemId, player.ID).OrbitalObjectID;
			if (player.Faction.Name == "loa")
			{
				foreach (FleetTemplate fleetTemplate in this.AssetDatabase.FleetTemplates)
				{
					if (fleetTemplate.CanFactionUse(player.Faction.Name))
					{
						List<int> source = new List<int>();
						foreach (ShipInclude shipInclude in fleetTemplate.ShipIncludes)
						{
							if ((string.IsNullOrEmpty(shipInclude.Faction) || !(shipInclude.Faction != player.Faction.Name)) && !(shipInclude.FactionExclusion == player.Faction.Name))
							{
								DesignInfo fillDesign = DesignLab.GetBestDesignByRole(this, player, new AIStance?(), shipInclude.ShipRole, StrategicAI.GetEquivilantShipRoles(shipInclude.ShipRole), shipInclude.WeaponRole) ?? DesignLab.GetBestDesignByRole(this, player, new AIStance?(), shipInclude.ShipRole, StrategicAI.GetEquivilantShipRoles(shipInclude.ShipRole), new WeaponRole?());
								int num;
								if (shipInclude.InclusionType == ShipInclusionType.FILL && fillDesign != null)
								{
									DesignInfo commandDesign = DesignLab.GetBestDesignByRole(this, player, new AIStance?(), ShipRole.COMMAND, StrategicAI.GetEquivilantShipRoles(ShipRole.COMMAND), new WeaponRole?()) ?? DesignLab.GetDesignByRole(this, player, (AITechStyles)null, new AIStance?(), ShipRole.COMMAND, new WeaponRole?());
									num = DesignLab.GetTemplateFillAmount(this._db, fleetTemplate, commandDesign, fillDesign);
								}
								else
									num = shipInclude.Amount;
								for (int index = 0; index < num; ++index)
								{
									if (fillDesign != null)
										source.Add(fillDesign.ID);
								}
							}
						}
						if (source.Count<int>() > 0)
							this.GameDatabase.InsertLoaFleetComposition(player.ID, fleetTemplate.Name, source.AsEnumerable<int>());
					}
				}
			}
			int newAdmiral1 = GameSession.GenerateNewAdmiral(this.AssetDatabase, player.ID, this.GameDatabase, new AdmiralInfo.TraitType?(), this.NamesPool);
			if (newAdmiral1 == 0)
				return;
			this._db.ClearAdmiralTraits(newAdmiral1);
			this._db.AddAdmiralTrait(newAdmiral1, AdmiralInfo.TraitType.Pathfinder, 0);
			if (player.Faction.Name != "hiver")
			{
				int fleetFromTemplate1 = this.CreateFleetFromTemplate(player, "1st Survey Fleet", "DEFAULT_SURVEY", newAdmiral1, systemId);
				if (fleetFromTemplate1 != 0)
				{
					this.GameDatabase.LayoutFleet(fleetFromTemplate1);
					if (player.Faction.Name == "loa")
					{
						FleetInfo fleetInfo = this.GameDatabase.GetFleetInfo(fleetFromTemplate1);
						fleetInfo.Name = "DEFAULT_SURVEY";
						this.GameDatabase.UpdateFleetInfo(fleetInfo);
						this.GameDatabase.GetShipInfoByFleetID(fleetFromTemplate1, false).ToList<ShipInfo>();
						this.GameDatabase.UpdateShipLoaCubes(Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this, fleetFromTemplate1), 70000);
						this.GameDatabase.UpdateFleetCompositionID(fleetFromTemplate1, new int?(this._db.GetLoaFleetCompositions().FirstOrDefault<LoaFleetComposition>((Func<LoaFleetComposition, bool>)(x =>
					   {
						   if (x.Name == "DEFAULT_SURVEY")
							   return x.PlayerID == player.ID;
						   return false;
					   })).ID));
					}
				}
				int newAdmiral2 = GameSession.GenerateNewAdmiral(this.AssetDatabase, player.ID, this.GameDatabase, new AdmiralInfo.TraitType?(), this.NamesPool);
				if (newAdmiral2 == 0)
					return;
				this._db.ClearAdmiralTraits(newAdmiral2);
				this._db.AddAdmiralTrait(newAdmiral2, AdmiralInfo.TraitType.Pathfinder, 0);
				int fleetFromTemplate2 = this.CreateFleetFromTemplate(player, "2nd Survey Fleet", "DEFAULT_SURVEY", newAdmiral2, systemId);
				if (fleetFromTemplate2 != 0)
				{
					this.GameDatabase.LayoutFleet(fleetFromTemplate2);
					if (player.Faction.Name == "loa")
					{
						FleetInfo fleetInfo = this.GameDatabase.GetFleetInfo(fleetFromTemplate2);
						fleetInfo.Name = "DEFAULT_SURVEY";
						this.GameDatabase.UpdateFleetInfo(fleetInfo);
						this.GameDatabase.UpdateShipLoaCubes(Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this, fleetFromTemplate2), 70000);
						this.GameDatabase.UpdateFleetCompositionID(fleetFromTemplate2, new int?(this._db.GetLoaFleetCompositions().FirstOrDefault<LoaFleetComposition>((Func<LoaFleetComposition, bool>)(x =>
					   {
						   if (x.Name == "DEFAULT_SURVEY")
							   return x.PlayerID == player.ID;
						   return false;
					   })).ID));
					}
				}
			}
			else
			{
				int newAdmiral2 = GameSession.GenerateNewAdmiral(this.AssetDatabase, player.ID, this.GameDatabase, new AdmiralInfo.TraitType?(), this.NamesPool);
				if (newAdmiral2 == 0)
					return;
				this._db.ClearAdmiralTraits(newAdmiral2);
				this._db.AddAdmiralTrait(newAdmiral2, AdmiralInfo.TraitType.Pathfinder, 0);
				int fleetFromTemplate = this.CreateFleetFromTemplate(player, "1st Survey Fleet", "HIVER_SURVEY", newAdmiral2, systemId);
				if (fleetFromTemplate != 0)
					this.GameDatabase.LayoutFleet(fleetFromTemplate);
			}
			int newAdmiral3 = GameSession.GenerateNewAdmiral(this.AssetDatabase, player.ID, this.GameDatabase, new AdmiralInfo.TraitType?(), this.NamesPool);
			if (newAdmiral3 == 0)
				return;
			this._db.ClearAdmiralTraits(newAdmiral3);
			this._db.AddAdmiralTrait(newAdmiral3, AdmiralInfo.TraitType.Architect, 0);
			int fleetFromTemplate3 = this.CreateFleetFromTemplate(player, "1st Construction Fleet", "DEFAULT_CONSTRUCTION", newAdmiral3, systemId);
			if (fleetFromTemplate3 != 0)
			{
				this.GameDatabase.LayoutFleet(fleetFromTemplate3);
				if (player.Faction.Name == "loa")
				{
					FleetInfo fleetInfo = this.GameDatabase.GetFleetInfo(fleetFromTemplate3);
					fleetInfo.Name = "DEFAULT_CONSTRUCTION";
					this.GameDatabase.UpdateFleetInfo(fleetInfo);
					this.GameDatabase.GetShipInfoByFleetID(fleetFromTemplate3, false).ToList<ShipInfo>();
					this.GameDatabase.UpdateShipLoaCubes(Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this, fleetFromTemplate3), 70000);
					this.GameDatabase.UpdateFleetCompositionID(fleetFromTemplate3, new int?(this._db.GetLoaFleetCompositions().FirstOrDefault<LoaFleetComposition>((Func<LoaFleetComposition, bool>)(x =>
				   {
					   if (x.Name == "DEFAULT_CONSTRUCTION")
						   return x.PlayerID == player.ID;
					   return false;
				   })).ID));
				}
			}
			int newAdmiral4 = GameSession.GenerateNewAdmiral(this.AssetDatabase, player.ID, this.GameDatabase, new AdmiralInfo.TraitType?(), this.NamesPool);
			if (newAdmiral4 == 0)
				return;
			this._db.ClearAdmiralTraits(newAdmiral4);
			this._db.AddAdmiralTrait(newAdmiral4, AdmiralInfo.TraitType.GreenThumb, 0);
			int fleetFromTemplate4 = this.CreateFleetFromTemplate(player, "1st Colonization Fleet", "DEFAULT_COLONIZATION", newAdmiral4, systemId);
			if (fleetFromTemplate4 != 0)
			{
				this.GameDatabase.LayoutFleet(fleetFromTemplate4);
				if (player.Faction.Name == "loa")
				{
					FleetInfo fleetInfo = this.GameDatabase.GetFleetInfo(fleetFromTemplate4);
					fleetInfo.Name = "DEFAULT_COLONIZATION";
					this.GameDatabase.UpdateFleetInfo(fleetInfo);
					this.GameDatabase.GetShipInfoByFleetID(fleetFromTemplate4, false).ToList<ShipInfo>();
					this.GameDatabase.UpdateShipLoaCubes(Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this, fleetFromTemplate4), 70000);
					this.GameDatabase.UpdateFleetCompositionID(fleetFromTemplate4, new int?(this._db.GetLoaFleetCompositions().FirstOrDefault<LoaFleetComposition>((Func<LoaFleetComposition, bool>)(x =>
				   {
					   if (x.Name == "DEFAULT_COLONIZATION")
						   return x.PlayerID == player.ID;
					   return false;
				   })).ID));
				}
			}
			if (!(player.Faction.Name == "hiver"))
				return;
			int newAdmiral5 = GameSession.GenerateNewAdmiral(this.AssetDatabase, player.ID, this.GameDatabase, new AdmiralInfo.TraitType?(), this.NamesPool);
			if (newAdmiral5 != 0)
			{
				this._db.ClearAdmiralTraits(newAdmiral5);
				this._db.AddAdmiralTrait(newAdmiral5, AdmiralInfo.TraitType.Pathfinder, 0);
				int fleetFromTemplate1 = this.CreateFleetFromTemplate(player, "1st Gate Fleet", "DEFAULT_GATE", newAdmiral5, systemId);
				if (fleetFromTemplate1 != 0)
					this.GameDatabase.LayoutFleet(fleetFromTemplate1);
			}
			int newAdmiral6 = GameSession.GenerateNewAdmiral(this.AssetDatabase, player.ID, this.GameDatabase, new AdmiralInfo.TraitType?(), this.NamesPool);
			if (newAdmiral6 == 0)
				return;
			this._db.ClearAdmiralTraits(newAdmiral6);
			this._db.AddAdmiralTrait(newAdmiral6, AdmiralInfo.TraitType.Pathfinder, 0);
			int fleetFromTemplate5 = this.CreateFleetFromTemplate(player, "2nd Gate Fleet", "DEFAULT_GATE", newAdmiral6, systemId);
			if (fleetFromTemplate5 == 0)
				return;
			this.GameDatabase.LayoutFleet(fleetFromTemplate5);
		}

		private void AddDefaultShip(int fleetID, int designID)
		{
			int carrierID = this.GameDatabase.InsertShip(fleetID, designID, null, (ShipParams)0, new int?(), 0);
			this.AddDefaultStartingRiders(fleetID, designID, carrierID);
		}

		public void AddDefaultStartingRiders(int fleetID, int designID, int carrierID)
		{
			bool flag = false;
			int index1 = 0;
			DesignInfo designInfo = this._db.GetDesignInfo(designID);
			foreach (DesignSectionInfo designSection in designInfo.DesignSections)
			{
				foreach (WeaponBankInfo weaponBank in designSection.WeaponBanks)
				{
					WeaponBankInfo wpnBank = weaponBank;
					ShipSectionAsset shipSectionAsset = designSection.ShipSectionAsset;
					if (shipSectionAsset != null)
					{
						LogicalBank lb = ((IEnumerable<LogicalBank>)shipSectionAsset.Banks).FirstOrDefault<LogicalBank>((Func<LogicalBank, bool>)(x => x.GUID == wpnBank.BankGUID));
						if (lb != null)
						{
							if (wpnBank.DesignID.HasValue)
							{
								int? designId = wpnBank.DesignID;
								if ((designId.GetValueOrDefault() != 0 ? 1 : (!designId.HasValue ? 1 : 0)) != 0)
								{
									if (shipSectionAsset != null)
									{
										int num = ((IEnumerable<LogicalMount>)shipSectionAsset.Mounts).Where<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank == lb)).Count<LogicalMount>();
										for (int index2 = 0; index2 < num; ++index2)
										{
											int shipID = this._db.InsertShip(fleetID, wpnBank.DesignID.Value, null, (ShipParams)0, new int?(), 0);
											this._db.SetShipParent(shipID, carrierID);
											this._db.UpdateShipRiderIndex(shipID, index1);
											++index1;
										}
										continue;
									}
									continue;
								}
							}
							if (WeaponEnums.IsBattleRider(lb.TurretClass))
								index1 += ((IEnumerable<LogicalMount>)shipSectionAsset.Mounts).Where<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank == lb)).Count<LogicalMount>();
						}
					}
				}
				foreach (DesignModuleInfo module in designSection.Modules)
				{
					string moduleAsset = this._db.GetModuleAsset(module.ModuleID);
					LogicalModule logicalModule = this.AssetDatabase.Modules.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == moduleAsset));
					if (logicalModule != null)
					{
						if (((IEnumerable<LogicalBank>)logicalModule.Banks).Count<LogicalBank>() > 0 && WeaponEnums.IsWeaponBattleRider(((IEnumerable<LogicalBank>)logicalModule.Banks).First<LogicalBank>().TurretClass) && (!module.DesignID.HasValue || module.DesignID.Value == 0))
						{
							module.DesignID = DesignLab.ChooseBattleRider(this, DesignLab.GetWeaponRiderShipRole(((IEnumerable<LogicalBank>)logicalModule.Banks).First<LogicalBank>().TurretClass, designSection.ShipSectionAsset.IsScavenger), designInfo.WeaponRole, designInfo.PlayerID);
							if (module.DesignID.HasValue)
								flag = true;
						}
						if (module.DesignID.HasValue)
						{
							int? designId = module.DesignID;
							if ((designId.GetValueOrDefault() != 0 ? 1 : (!designId.HasValue ? 1 : 0)) != 0)
							{
								if (logicalModule != null)
								{
									int num = ((IEnumerable<LogicalMount>)logicalModule.Mounts).Count<LogicalMount>();
									for (int index2 = 0; index2 < num; ++index2)
									{
										int shipID = this._db.InsertShip(fleetID, module.DesignID.Value, null, (ShipParams)0, new int?(), 0);
										this._db.SetShipParent(shipID, carrierID);
										this._db.UpdateShipRiderIndex(shipID, index1);
										++index1;
									}
									continue;
								}
								continue;
							}
						}
						if (((IEnumerable<LogicalBank>)logicalModule.Banks).Count<LogicalBank>() > 0 && WeaponEnums.IsBattleRider(((IEnumerable<LogicalBank>)logicalModule.Banks).First<LogicalBank>().TurretClass))
							index1 += ((IEnumerable<LogicalMount>)logicalModule.Mounts).Count<LogicalMount>();
					}
				}
			}
			if (!flag)
				return;
			this._db.UpdateDesign(designInfo);
		}

		private void HandleIntelMissionsForPlayer(int playerid)
		{
			List<IntelMissionInfo> list = this.GameDatabase.GetIntelInfosForPlayer(playerid).Where<IntelMissionInfo>((Func<IntelMissionInfo, bool>)(x => x.Turn == this.GameDatabase.GetTurnCount() - 1)).ToList<IntelMissionInfo>();
			List<int> intList = new List<int>();
			foreach (IntelMissionInfo intelMissionInfo in list)
			{
				IntelMissionInfo mis = intelMissionInfo;
				this.AssetDatabase.IntelMissions.First<IntelMissionDesc>((Func<IntelMissionDesc, bool>)(x => x.ID == mis.MissionType)).OnCommit(this, playerid, mis.TargetPlayerId, new int?(mis.ID));
				if (mis.BlamePlayer.HasValue && !intList.Contains(mis.BlamePlayer.Value))
				{
					intList.Add(mis.BlamePlayer.Value);
					this.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_INTEL_MISSION_CRITICAL_FAILED_LEAK,
						EventMessage = TurnEventMessage.EM_INTEL_MISSION_CRITICAL_FAILED_LEAK,
						PlayerID = mis.TargetPlayerId,
						TargetPlayerID = mis.BlamePlayer.Value,
						TurnNumber = this.GameDatabase.GetTurnCount()
					});
				}
				this.GameDatabase.RemoveIntelMission(mis.ID);
			}
		}

		private void HandleCounterIntelMissions(int playerid)
		{
			foreach (CounterIntelStingMission intelmission in this.GameDatabase.GetCountIntelStingsForPlayer(playerid).ToList<CounterIntelStingMission>())
				this.DoCounterIntelMission(playerid, intelmission.TargetPlayerId, intelmission);
		}

		public int GetSystemRepairPoints(int systemid, int playerid)
		{
			List<ColonyInfo> list = this._db.GetColonyInfosForSystem(systemid).Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == playerid)).ToList<ColonyInfo>();
			int num = 0;
			foreach (ColonyInfo colonyInfo in list)
				num += colonyInfo.RepairPoints;
			foreach (StationInfo stationInfo in this._db.GetStationForSystemAndPlayer(systemid, playerid).ToList<StationInfo>())
			{
				foreach (SectionInstanceInfo sectionInstanceInfo in this._db.GetShipSectionInstances(stationInfo.ShipID).ToList<SectionInstanceInfo>())
					num += sectionInstanceInfo.RepairPoints;
			}
			foreach (FleetInfo fleetInfo in this._db.GetFleetInfoBySystemID(systemid, FleetType.FL_NORMAL).ToList<FleetInfo>())
			{
				foreach (ShipInfo shipInfo in this._db.GetShipInfoByFleetID(fleetInfo.ID, false).ToList<ShipInfo>())
				{
					foreach (SectionInstanceInfo sectionInstanceInfo in this._db.GetShipSectionInstances(shipInfo.ID).ToList<SectionInstanceInfo>())
						num += sectionInstanceInfo.RepairPoints;
				}
			}
			return num;
		}

		public void UseSystemRepairPoints(int systemid, int playerid, int points)
		{
			foreach (ColonyInfo colony in this._db.GetColonyInfosForSystem(systemid).Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == playerid)).ToList<ColonyInfo>())
			{
				if (points <= 0)
					return;
				int num = colony.RepairPoints - points;
				if (num < 0)
					num = 0;
				points -= colony.RepairPoints;
				colony.RepairPoints = num;
				this.App.GameDatabase.UpdateColony(colony);
			}
			foreach (StationInfo stationInfo in this._db.GetStationForSystemAndPlayer(systemid, playerid).ToList<StationInfo>())
			{
				foreach (SectionInstanceInfo section in this._db.GetShipSectionInstances(stationInfo.ShipID).ToList<SectionInstanceInfo>())
				{
					if (points <= 0)
						return;
					int num = section.RepairPoints - points;
					if (num < 0)
						num = 0;
					points -= section.RepairPoints;
					section.RepairPoints -= num;
					this.App.GameDatabase.UpdateSectionInstance(section);
				}
			}
			foreach (FleetInfo fleetInfo in this._db.GetFleetInfoBySystemID(systemid, FleetType.FL_NORMAL).ToList<FleetInfo>())
			{
				foreach (ShipInfo shipInfo in this._db.GetShipInfoByFleetID(fleetInfo.ID, false).ToList<ShipInfo>())
				{
					foreach (SectionInstanceInfo section in this._db.GetShipSectionInstances(shipInfo.ID).ToList<SectionInstanceInfo>())
					{
						if (points <= 0)
							return;
						int num = section.RepairPoints - points;
						if (num < 0)
							num = 0;
						points -= section.RepairPoints;
						section.RepairPoints -= num;
						this.App.GameDatabase.UpdateSectionInstance(section);
					}
				}
			}
		}

		private int RepairFleet(int fleetid, int availpoints)
		{
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetid);
			if (fleetInfo == null || fleetInfo.SystemID == 0 || availpoints <= 0)
				return availpoints;
			List<ShipInfo> list = this._db.GetShipInfoByFleetID(fleetInfo.ID, true).ToList<ShipInfo>();
			int points1 = 0;
			foreach (ShipInfo ship in list)
			{
				int[] healthAndHealthMax = Kerberos.Sots.StarFleet.StarFleet.GetHealthAndHealthMax(this, ship.DesignInfo, ship.ID);
				int points2 = healthAndHealthMax[1] - healthAndHealthMax[0];
				if (availpoints > 0)
				{
					if (points2 > 0)
					{
						if (points2 > availpoints)
							points2 = availpoints;
						if (points2 <= availpoints)
						{
							points1 += points2;
							availpoints -= points2;
							Kerberos.Sots.StarFleet.StarFleet.RepairShip(this.App, ship, points2);
						}
						else
							break;
					}
				}
				else
					break;
			}
			this.UseSystemRepairPoints(fleetInfo.SystemID, fleetInfo.PlayerID, points1);
			return availpoints - points1;
		}

		public void RepairFleet(int fleetid)
		{
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetid);
			int systemRepairPoints = this.GetSystemRepairPoints(fleetInfo.SystemID, fleetInfo.PlayerID);
			if (systemRepairPoints <= 0)
				return;
			this.RepairFleet(fleetInfo.ID, systemRepairPoints);
		}

		public void RepairFleetsAtSystem(int systemid, int playerid)
		{
			int availpoints = this.GetSystemRepairPoints(systemid, playerid);
			if (availpoints <= 0)
				return;
			foreach (FleetInfo fleetInfo in this._db.GetFleetInfoBySystemID(systemid, FleetType.FL_ALL | FleetType.FL_STATION).Where<FleetInfo>((Func<FleetInfo, bool>)(x => x.PlayerID == playerid)).ToList<FleetInfo>())
				availpoints = this.RepairFleet(fleetInfo.ID, availpoints);
		}

		private float CalcBoostResearchAccidentOdds(int playerid)
		{
			PlayerInfo playerInfo = this.GameDatabase.GetPlayerInfo(playerid);
			Faction faction = this.AssetDatabase.GetFaction(playerInfo.FactionID);
			double researchBoostFunds = playerInfo.ResearchBoostFunds;
			if (researchBoostFunds <= 0.0)
				return 0.0f;
			double savings = playerInfo.Savings;
			if (savings <= 0.0)
				return 1f;
			float num = (float)researchBoostFunds / (float)savings * 2f * faction.ResearchBoostFailureMod;
			if ((double)num < 0.0)
				num = 0.0f;
			if ((double)num > 1.0)
				num = 1f;
			return num;
		}

		private bool DoResearchAccident(int playerid)
		{
			if (!this.GameDatabase.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
		   {
			   int? systemOwningPlayer = this.GameDatabase.GetSystemOwningPlayer(x.ID);
			   int num = playerid;
			   if (systemOwningPlayer.GetValueOrDefault() == num)
				   return systemOwningPlayer.HasValue;
			   return false;
		   })).ToList<StarSystemInfo>().Any<StarSystemInfo>())
				return false;
			PlayerInfo playerInfo = this.GameDatabase.GetPlayerInfo(playerid);
			int researchingTechId = this.GameDatabase.GetPlayerResearchingTechID(playerid);
			if (researchingTechId == 0)
				return false;
			string techID = this.App.GameDatabase.GetTechFileID(researchingTechId);
			Kerberos.Sots.Data.TechnologyFramework.Tech techno = this.App.AssetDatabase.MasterTechTree.Technologies.First<Kerberos.Sots.Data.TechnologyFramework.Tech>((Func<Kerberos.Sots.Data.TechnologyFramework.Tech, bool>)(tech => tech.Id == techID));
			if (techno.Group == "Group Bioweapons")
			{
				LogicalWeapon logicalWeapon = this.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x =>
			   {
				   if (((IEnumerable<Kerberos.Sots.Data.WeaponFramework.Tech>)x.RequiredTechs).Any<Kerberos.Sots.Data.WeaponFramework.Tech>((Func<Kerberos.Sots.Data.WeaponFramework.Tech, bool>)(j => j.Name == techno.Name)))
					   return x.PlagueType != WeaponEnums.PlagueType.NONE;
				   return false;
			   }));
				if (logicalWeapon == null)
					return false;
				PlayerTechInfo playerTechInfo = this.GameDatabase.GetPlayerTechInfo(playerid, researchingTechId);
				playerTechInfo.Progress = 0;
				this.GameDatabase.UpdatePlayerTechInfo(playerTechInfo);
				ColonyInfo colonyInfo = this.GameDatabase.GetColonyInfos().FirstOrDefault<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == playerid));
				if (colonyInfo == null)
					return false;
				GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_PLAGUE_OUTBREAK, colonyInfo.PlayerID, new int?(colonyInfo.ID), new int?(), new int?());
				this.App.GameDatabase.InsertPlagueInfo(new PlagueInfo()
				{
					PlagueType = logicalWeapon.PlagueType,
					ColonyId = colonyInfo.ID,
					LaunchingPlayer = playerid,
					InfectedPopulationCivilian = Math.Floor((double)logicalWeapon.PopDamage * 0.75),
					InfectedPopulationImperial = Math.Floor((double)logicalWeapon.PopDamage * 0.25),
					InfectionRate = this.App.AssetDatabase.GetPlagueInfectionRate(logicalWeapon.PlagueType)
				});
				this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_RESEARCH_PLAGUE_DISASTER,
					EventMessage = TurnEventMessage.EM_RESEARCH_PLAGUE_DISASTER,
					PlagueType = logicalWeapon.PlagueType,
					ColonyID = colonyInfo.ID,
					PlayerID = colonyInfo.PlayerID,
					TurnNumber = this.App.GameDatabase.GetTurnCount(),
					ShowsDialog = false
				});
				return true;
			}
			if (!(techno.Group == "Group AI") || !techno.AllowAIRebellion)
				return false;
			this.DoAIRebellion(playerInfo, true);
			PlayerTechInfo playerTechInfo1 = this.GameDatabase.GetPlayerTechInfo(playerid, researchingTechId);
			playerTechInfo1.Progress = 0;
			this.GameDatabase.UpdatePlayerTechInfo(playerTechInfo1);
			return true;
		}

		private bool DoBoostResearchAccidentRoll(int playerid)
		{
			PlayerInfo playerInfo = this.GameDatabase.GetPlayerInfo(playerid);
			int researchingTechId = this.GameDatabase.GetPlayerResearchingTechID(playerid);
			this.AssetDatabase.GetFaction(playerInfo.FactionID);
			if (researchingTechId == 0)
				return false;
			string techID = this.App.GameDatabase.GetTechFileID(researchingTechId);
			Kerberos.Sots.Data.TechnologyFramework.Tech tech1 = this.App.AssetDatabase.MasterTechTree.Technologies.First<Kerberos.Sots.Data.TechnologyFramework.Tech>((Func<Kerberos.Sots.Data.TechnologyFramework.Tech, bool>)(tech => tech.Id == techID));
			int num1 = (int)((double)this.CalcBoostResearchAccidentOdds(playerid) * 100.0);
			if (num1 <= 0 || new Random().NextInclusive(0, 100) > num1)
				return false;
			if (this.DoResearchAccident(playerid))
				return true;
			Kerberos.Sots.Data.TechnologyFramework.Tech.TechThreatDamage techThreatDamage = Kerberos.Sots.Data.TechnologyFramework.Tech.GetTechThreatDamage(tech1.DangerLevel - 1);
			StarSystemInfo starSystemInfo = (StarSystemInfo)null;
			ColonyInfo colonyInfo = (ColonyInfo)null;
			if (techThreatDamage.m_bRequiresSystem)
			{
				List<StarSystemInfo> list = this.GameDatabase.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
			   {
				   int? systemOwningPlayer = this.GameDatabase.GetSystemOwningPlayer(x.ID);
				   int num2 = playerid;
				   if (systemOwningPlayer.GetValueOrDefault() == num2)
					   return systemOwningPlayer.HasValue;
				   return false;
			   })).ToList<StarSystemInfo>();
				if (!list.Any<StarSystemInfo>())
					return false;
				starSystemInfo = list[new Random().Next(list.Count)];
				colonyInfo = this.GameDatabase.GetColonyInfosForSystem(starSystemInfo.ID).FirstOrDefault<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == playerid));
				if (colonyInfo == null)
					return false;
			}
			float num3 = (float)new Random().Next(1, 5);
			techThreatDamage.m_PopulationMin *= num3;
			techThreatDamage.m_PopulationMax *= num3;
			techThreatDamage.m_InfraMin *= num3;
			techThreatDamage.m_InfraMax *= num3;
			double num4 = 0.0;
			double num5 = 0.0;
			PlayerTechInfo playerTechInfo = this.GameDatabase.GetPlayerTechInfo(playerid, researchingTechId);
			int progress = playerTechInfo.Progress;
			int num6 = (int)((double)new Random().Next(techThreatDamage.m_ProgressMin, techThreatDamage.m_ProgressMax) / 100.0 * (double)playerTechInfo.Progress);
			int num7 = Math.Max(playerTechInfo.Progress - num6, 0);
			playerTechInfo.Progress = num7;
			this.GameDatabase.UpdatePlayerTechInfo(playerTechInfo);
			int num8 = progress <= 0 ? 100 : num6 / progress * 100;
			float num9 = 0f;
			if (starSystemInfo != (StarSystemInfo)null && colonyInfo != null)
			{
				PlanetInfo planetInfo = this.GameDatabase.GetPlanetInfo(colonyInfo.OrbitalObjectID);
				num4 = this.GameDatabase.GetTotalPopulation(colonyInfo);
				double infrastructure1 = (double)planetInfo.Infrastructure;
				float num2 = techThreatDamage.m_PopulationMin + (float)new Random().NextDouble() * (techThreatDamage.m_PopulationMax - techThreatDamage.m_PopulationMin);
				double imperialPop = colonyInfo.ImperialPop;
				double num10 = (double)num2 * imperialPop;
				double num11 = imperialPop - num10;
				colonyInfo.ImperialPop = num11;
				this.GameDatabase.UpdateColony(colonyInfo);
				float num12 = techThreatDamage.m_InfraMin + (float)new Random().NextDouble() * (techThreatDamage.m_InfraMax - techThreatDamage.m_InfraMin);
				float infrastructure2 = planetInfo.Infrastructure;
				float num13 = num12 * infrastructure2;
				num9 = infrastructure2 - num13;
				planetInfo.Infrastructure = num9;
				this.GameDatabase.UpdatePlanet(planetInfo);
				num5 = this.GameDatabase.GetTotalPopulation(colonyInfo);
				double infrastructure3 = (double)planetInfo.Infrastructure;
			}
			if (tech1.DangerLevel == 1)
				this._db.InsertTurnEvent(new TurnEvent()
				{
					FeasibilityPercent = num8,
					EventType = TurnEventType.EV_BOOSTFAILED_1,
					EventMessage = TurnEventMessage.EM_BOOSTFAILED_1,
					PlayerID = playerid,
					TechID = playerTechInfo.TechID,
					TurnNumber = this._app.GameDatabase.GetTurnCount(),
					ShowsDialog = false
				});
			else if (tech1.DangerLevel == 2 && colonyInfo != null)
				this._db.InsertTurnEvent(new TurnEvent()
				{
					FeasibilityPercent = num8,
					Infrastructure = num9,
					EventType = TurnEventType.EV_BOOSTFAILED_2,
					EventMessage = TurnEventMessage.EM_BOOSTFAILED_2,
					PlayerID = playerid,
					TechID = playerTechInfo.TechID,
					ColonyID = colonyInfo.ID,
					ImperialPop = (float)(num4 - num5),
					TurnNumber = this._app.GameDatabase.GetTurnCount(),
					ShowsDialog = false
				});
			else if (tech1.DangerLevel == 3 && colonyInfo != null)
				this._db.InsertTurnEvent(new TurnEvent()
				{
					FeasibilityPercent = num8,
					Infrastructure = num9,
					EventType = TurnEventType.EV_BOOSTFAILED_3,
					EventMessage = TurnEventMessage.EM_BOOSTFAILED_3,
					PlayerID = playerid,
					TechID = playerTechInfo.TechID,
					ColonyID = colonyInfo.ID,
					ImperialPop = (float)(num4 - num5),
					TurnNumber = this._app.GameDatabase.GetTurnCount(),
					ShowsDialog = false
				});
			return true;
		}

		public void Dispose()
		{
			if (this._crits != null)
			{
				this._crits.Dispose();
				this._crits = (GameObjectSet)null;
			}
			if (this._db == null)
				return;
			this._db.Dispose();
			this._db = (GameDatabase)null;
		}

		private enum IntelMissionResults
		{
			CriticalSuccess,
			Success,
			Failure,
			CriticalFailure,
		}

		private class SimPlayerInfo
		{
			public ShipSectionCollection AvailableShipSections;
		}

		[System.Flags]
		public enum Flags
		{
			NoNewGameMessage = 1,
			ResumingGame = 2,
			NoTechTree = 4,
			NoScriptModules = 8,
			NoDefaultFleets = 16, // 0x00000010
			NoOrbitalObjects = 32, // 0x00000020
			NoGameSetup = 64, // 0x00000040
		}

		public enum VictoryStatus
		{
			Win,
			Loss,
			Draw,
		}

		public enum FleetLeaveReason
		{
			TRAVEL,
			KILLED,
		}

		public struct IntersectingSystem
		{
			public int SystemID;
			public float Distance;
			public bool StartOrEnd;
		}

		private sealed class NetRevenueSummary
		{
			public readonly double TradeRevenue;
			public readonly double TaxRevenue;
			public readonly double IORevenue;
			public readonly double SavingsInterest;
			public readonly double ColonySupportCost;
			public readonly double UpkeepCost;
			public readonly double CorruptionExpenses;
			public readonly double DebtInterest;

			public double GetNetRevenue()
			{
				return this.TradeRevenue + this.TaxRevenue + this.IORevenue + this.SavingsInterest - this.ColonySupportCost - this.UpkeepCost - this.CorruptionExpenses - this.DebtInterest;
			}

			public NetRevenueSummary(App game, int playerId, double tradeRevenue)
			{
				GameDatabase db = game.GameDatabase;
				AssetDatabase assetdb = game.AssetDatabase;
				List<ColonyInfo> list = db.GetPlayerColoniesByPlayerId(playerId).ToList<ColonyInfo>();
				List<ColonyInfo> source = new List<ColonyInfo>();
				foreach (TreatyInfo treatyInfo in db.GetTreatyInfos().ToList<TreatyInfo>().Where<TreatyInfo>((Func<TreatyInfo, bool>)(x =>
			   {
				   if (x.Type == TreatyType.Protectorate)
					   return x.InitiatingPlayerId == playerId;
				   return false;
			   })).ToList<TreatyInfo>())
				{
					if (treatyInfo.Active)
						source.AddRange(db.GetPlayerColoniesByPlayerId(treatyInfo.ReceivingPlayerId));
				}
				PlayerInfo playerInfo = db.GetPlayerInfo(playerId);
				this.IORevenue = this.GetBaseIndustrialOutputRevenue(game.Game, db, playerInfo) * (double)game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.IORevenue, playerInfo.ID);
				this.TradeRevenue = tradeRevenue;
				this.TaxRevenue = list.Sum<ColonyInfo>((Func<ColonyInfo, double>)(x => Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetTaxRevenue(game, x))) + source.Sum<ColonyInfo>((Func<ColonyInfo, double>)(x => Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetTaxRevenue(game, x)));
				float num = db.GetNameValue<float>("EconomicEfficiency") / 100f;
				this.TradeRevenue *= (double)num;
				this.TradeRevenue *= (double)game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.TradeRevenue, playerId);
				this.TaxRevenue *= (double)num;
				this.SavingsInterest = GameSession.CalculateSavingsInterest(game.Game, playerInfo);
				this.DebtInterest = GameSession.CalculateDebtInterest(game.Game, playerInfo);
				this.ColonySupportCost = list.Sum<ColonyInfo>((Func<ColonyInfo, double>)(x => Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetColonySupportCost(assetdb, db, x)));
				Kerberos.Sots.PlayerFramework.Player player = game.GetPlayer(playerId);
				this.UpkeepCost = player == null || player.IsAI() ? 0.0 : GameSession.CalculateUpkeepCosts(assetdb, db, playerId);
				if (game.AssetDatabase.GetFaction(playerInfo.FactionID).Name == "loa")
					this.CorruptionExpenses = 0.0;
				else
					this.CorruptionExpenses = (this.TradeRevenue + this.TaxRevenue + this.IORevenue + this.SavingsInterest) * (double)Math.Max(0.0f, (float)((double)assetdb.BaseCorruptionRate + 0.0199999995529652 * ((double)playerInfo.RateImmigration * 10.0) - 2.0 * ((double)playerInfo.RateGovernmentResearch * (double)playerInfo.RateGovernmentSecurity)));
			}

			public double GetBaseIndustrialOutputRevenue(
			  GameSession game,
			  GameDatabase db,
			  PlayerInfo player)
			{
				double num1 = 0.0;
				foreach (int num2 in db.GetPlayerColonySystemIDs(player.ID).ToList<int>())
				{
					List<BuildOrderInfo> list = db.GetBuildOrdersForSystem(num2).ToList<BuildOrderInfo>();
					float num3 = 0.0f;
					foreach (ColonyInfo colony in db.GetColonyInfosForSystem(num2).ToList<ColonyInfo>())
					{
						if (colony.PlayerID == player.ID)
							num3 += Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetConstructionPoints(game, colony);
					}
					float num4 = num3 * game.GetStationBuildModifierForSystem(num2, player.ID);
					foreach (BuildOrderInfo buildOrderInfo in list)
					{
						DesignInfo designInfo = db.GetDesignInfo(buildOrderInfo.DesignID);
						if (designInfo.PlayerID == player.ID)
						{
							int savingsCost = designInfo.SavingsCost;
							if (designInfo.IsLoaCube())
							{
								int loaCubes = buildOrderInfo.LoaCubes;
								int loaCostPerCube = game.AssetDatabase.LoaCostPerCube;
							}
							int num5 = buildOrderInfo.ProductionTarget - buildOrderInfo.Progress;
							float num6 = 0.0f;
							if (!designInfo.isPrototyped)
								num6 = (float)(int)((double)num4 * ((double)db.GetStratModifierFloatToApply(StratModifiers.PrototypeTimeModifier, player.ID) - 1.0));
							if ((double)num5 <= (double)num4 - (double)num6)
								num4 -= (float)num5;
						}
					}
					num1 += (double)num4;
				}
				return num1;
			}
		}

		private class ImportNode
		{
			public StarSystemInfo System;
			public Dictionary<int, int> ImportCapacity;
		}

		private class ExportNode
		{
			public List<GameSession.ImportNode> InternationalSystems = new List<GameSession.ImportNode>();
			public List<GameSession.ImportNode> InterprovincialSystems = new List<GameSession.ImportNode>();
			public List<GameSession.ImportNode> GenericSystems = new List<GameSession.ImportNode>();
			public StarSystemInfo System;
			public StationInfo Station;
			public int ExportPoints;
			public float Range;
		}
	}
}
