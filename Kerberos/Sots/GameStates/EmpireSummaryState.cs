// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.EmpireSummaryState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal class EmpireSummaryState : GameState, IKeyBindListener
	{
		private static readonly string UIExitButton = "gameExitButton";
		private static readonly string UIGovernmentType = "governmenttype_title";
		private static readonly string UIGovernmentResearchSlider = "governmentResearchSlider";
		private static readonly string UISecuritySlider = "securitySlider";
		private static readonly string UIOperationsSlider = "operationsSlider";
		private static readonly string UIIntelSlider = "intelSlider";
		private static readonly string UICounterIntelSlider = "counterIntelSlider";
		private static readonly string UIStimulusSlider = "stimulusSlider";
		private static readonly string UIMiningSlider = "miningSlider";
		private static readonly string UIColonizationSlider = "colonizationSlider";
		private static readonly string UITradeSlider = "tradeSlider";
		private static readonly string UISavingsSlider = "savingsSlider";
		private static readonly string UICurrentProjectSlider = "currentProjectSlider";
		private static readonly string UISpecialProjectSlider = "specialProjectSlider";
		private static readonly string UISalvageResearchSlider = "salvageResearchSlider";
		private static readonly string UIEmpireButton = "btnEmpire";
		private static readonly string UIGovernmentButton = "btnGovernment";
		private static readonly string UIGOVScreenPanel = "governmentScreen";
		private static readonly string UIShipInvoicesList = "shipinvoicelist";
		private static readonly string UIShipInvoicesCost = "shipinvoicecost";
		private static readonly string UIStationInvoicesList = "stationinvoicelist";
		private static readonly string UIStationInvoicesCost = "stationinvoicecost";
		private static readonly string UIColonyDevList = "colonydevlist";
		private static readonly string UIColonyDevCost = "colonydevcost";
		private static readonly string UIColonyIncomeList = "colonyincomelist";
		private static readonly string UIColonyIncome = "colonyincome";
		private static readonly string UIFinancialDue = "financialDue_Amount";
		private static readonly string UIFinancialDueTotal = "financialCommit_Value";
		private static readonly string[] UIFactionRelation = new string[7]
		{
	  "factionRelation1",
	  "factionRelation2",
	  "factionRelation3",
	  "factionRelation4",
	  "factionRelation5",
	  "factionRelation6",
	  "factionRelation7"
		};
		private static readonly string[] UIFactionImage = new string[7]
		{
	  "factionImage1",
	  "factionImage2",
	  "factionImage3",
	  "factionImage4",
	  "factionImage5",
	  "factionImage6",
	  "factionImage7"
		};
		private static readonly string[] UIFactionBadge = new string[7]
		{
	  "factionBadge1",
	  "factionBadge2",
	  "factionBadge3",
	  "factionBadge4",
	  "factionBadge5",
	  "factionBadge6",
	  "factionBadge7"
		};
		private static readonly string UITaxesSlider = "taxslider";
		private static readonly string UIImmigrationSlider = "immigrationslider";
		private static readonly string UIImmigrationValue = "immigrationvalue";
		private static readonly string UIImmigrationLabel = "immigrationlabel";
		private BudgetPiechart _piechart;
		private GameState _prev;
		private GameObjectSet _crits;
		private TechCube _techCube;

		public EmpireSummaryState(App game)
		  : base(game)
		{
		}

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			if (this.App.GameDatabase == null)
				this.App.NewGame();
			this._prev = prev;
			this.App.UI.LoadScreen("EmpireSummary");
			this._crits = new GameObjectSet(this.App);
			this._techCube = new TechCube(this.App);
			this._crits.Add((IGameObject)this._techCube);
		}

		protected override void OnEnter()
		{
			if (this.App.LocalPlayer == null)
				this.App.NewGame();
			this.App.UI.SetScreen("EmpireSummary");
			this.App.UI.SetVisible(EmpireSummaryState.UIGOVScreenPanel, false);
			this._piechart = new BudgetPiechart(this.App.UI, "piechart", this.App.AssetDatabase);
			EmpireBarUI.SyncTitleFrame(this.App);
			this.App.UI.InitializeSlider(EmpireSummaryState.UIGovernmentResearchSlider, 0, 100, 0);
			this.App.UI.InitializeSlider(EmpireSummaryState.UISecuritySlider, 0, 100, 0);
			this.App.UI.InitializeSlider(EmpireSummaryState.UIOperationsSlider, 0, 100, 0);
			this.App.UI.InitializeSlider(EmpireSummaryState.UIIntelSlider, 0, 100, 0);
			this.App.UI.InitializeSlider(EmpireSummaryState.UICounterIntelSlider, 0, 100, 0);
			this.App.UI.InitializeSlider(EmpireSummaryState.UIStimulusSlider, 0, 100, 0);
			this.App.UI.InitializeSlider(EmpireSummaryState.UIMiningSlider, 0, 100, 0);
			this.App.UI.InitializeSlider(EmpireSummaryState.UIColonizationSlider, 0, 100, 0);
			this.App.UI.InitializeSlider(EmpireSummaryState.UITradeSlider, 0, 100, 0);
			this.App.UI.InitializeSlider(EmpireSummaryState.UISavingsSlider, 0, 100, 0);
			this.App.UI.InitializeSlider(EmpireSummaryState.UICurrentProjectSlider, 0, 100, 0);
			this.App.UI.InitializeSlider(EmpireSummaryState.UISpecialProjectSlider, 0, 100, 0);
			this.App.UI.InitializeSlider(EmpireSummaryState.UISalvageResearchSlider, 0, 100, 0);
			List<PlayerInfo> list1 = this.App.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo1 = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID);
			for (int index = 0; index < ((IEnumerable<string>)EmpireSummaryState.UIFactionImage).Count<string>(); ++index)
			{
				if (list1.Count > index)
				{
					PlayerInfo playerInfo2 = list1[index];
					if (playerInfo2.ID == this.App.LocalPlayer.ID)
					{
						list1.Remove(playerInfo2);
						--index;
					}
					else
					{
						DiplomaticMood diplomaticMood = this.App.GameDatabase.GetDiplomacyInfo(this.App.LocalPlayer.ID, playerInfo2.ID).GetDiplomaticMood();
						this.App.UI.SetVisible(EmpireSummaryState.UIFactionRelation[index], true);
						switch (diplomaticMood)
						{
							case DiplomaticMood.Hatred:
								this.App.UI.SetPropertyString(EmpireSummaryState.UIFactionRelation[index], "sprite", "Hate");
								break;
							case DiplomaticMood.Love:
								this.App.UI.SetPropertyString(EmpireSummaryState.UIFactionRelation[index], "sprite", "Love");
								break;
							default:
								this.App.UI.SetVisible(EmpireSummaryState.UIFactionRelation[index], false);
								break;
						}
						this.App.UI.SetVisible(EmpireSummaryState.UIFactionImage[index], true);
						this.App.UI.SetVisible(EmpireSummaryState.UIFactionBadge[index], true);
						this.App.UI.SetPropertyString(EmpireSummaryState.UIFactionImage[index], "sprite", Path.GetFileNameWithoutExtension(playerInfo2.AvatarAssetPath));
						this.App.UI.SetPropertyString(EmpireSummaryState.UIFactionBadge[index], "sprite", Path.GetFileNameWithoutExtension(playerInfo2.BadgeAssetPath));
					}
				}
				else
				{
					this.App.UI.SetVisible(EmpireSummaryState.UIFactionRelation[index], false);
					this.App.UI.SetVisible(EmpireSummaryState.UIFactionImage[index], false);
					this.App.UI.SetVisible(EmpireSummaryState.UIFactionBadge[index], false);
				}
			}
			int maxValue = this.App.GameDatabase.GetFactionName(playerInfo1.FactionID) == "zuul" ? 7 : 10;
			this.App.UI.InitializeSlider(EmpireSummaryState.UITaxesSlider, 0, maxValue, (int)Math.Round((double)playerInfo1.RateTax * 100.0));
			this.App.UI.InitializeSlider(EmpireSummaryState.UIImmigrationSlider, 0, 10, (int)Math.Round((double)playerInfo1.RateImmigration * 100.0));
			this.App.UI.SetVisible(EmpireSummaryState.UIImmigrationSlider, this.App.GetStratModifier<bool>(StratModifiers.AllowAlienImmigration, playerInfo1.ID));
			this.App.UI.SetVisible(EmpireSummaryState.UIImmigrationLabel, this.App.GetStratModifier<bool>(StratModifiers.AllowAlienImmigration, playerInfo1.ID));
			this.App.UI.SetVisible(EmpireSummaryState.UIImmigrationValue, this.App.GetStratModifier<bool>(StratModifiers.AllowAlienImmigration, playerInfo1.ID));
			List<GovernmentActionInfo> list2 = this.App.GameDatabase.GetGovernmentActions(this.App.LocalPlayer.ID).ToList<GovernmentActionInfo>();
			this.App.UI.ClearItems("eventlist");
			int num = this.App.GameDatabase.GetTurnCount() - 30;
			for (int index = list2.Count - 1; index >= 0; --index)
			{
				GovernmentActionInfo governmentActionInfo = list2[index];
				if (governmentActionInfo.Turn >= num)
				{
					string str1 = "";
					if (governmentActionInfo.AuthoritarianismChange > 0)
						str1 = string.Format("+" + App.Localize("@UI_GOVERNMENT_AUTHORITARIANISM"), (object)governmentActionInfo.AuthoritarianismChange);
					else if (governmentActionInfo.AuthoritarianismChange < 0)
						str1 = string.Format(App.Localize("@UI_GOVERNMENT_AUTHORITARIANISM"), (object)governmentActionInfo.AuthoritarianismChange);
					string str2 = "";
					if (governmentActionInfo.EconLiberalismChange > 0)
						str2 = string.Format("+" + App.Localize("@UI_GOVERNMENT_ECON_LIBERALISM"), (object)governmentActionInfo.EconLiberalismChange);
					else if (governmentActionInfo.EconLiberalismChange < 0)
						str2 = string.Format(App.Localize("@UI_GOVERNMENT_ECON_LIBERALISM"), (object)governmentActionInfo.EconLiberalismChange);
					if (!string.IsNullOrEmpty(str1))
					{
						string text = string.Format("{0} - {1}", (object)governmentActionInfo.Description, (object)str1);
						this.App.UI.AddItem("eventlist", "", governmentActionInfo.ID, "");
						this.App.UI.SetText(this.App.UI.Path(this.App.UI.GetItemGlobalID("eventlist", "", governmentActionInfo.ID, ""), "lblHeader"), text);
					}
					if (!string.IsNullOrEmpty(str2))
					{
						string text = string.Format("{0} - {1}", (object)governmentActionInfo.Description, (object)str2);
						this.App.UI.AddItem("eventlist", "", -governmentActionInfo.ID, "");
						this.App.UI.SetText(this.App.UI.Path(this.App.UI.GetItemGlobalID("eventlist", "", -governmentActionInfo.ID, ""), "lblHeader"), text);
					}
				}
			}
			GovernmentInfo governmentInfo = this.App.GameDatabase.GetGovernmentInfo(this.App.LocalPlayer.ID);
			this.App.UI.SetPropertyString(EmpireSummaryState.UIGovernmentType, "text", App.Localize(string.Format("@UI_EMPIRESUMMARY_{0}", (object)governmentInfo.CurrentType.ToString().ToUpper())));
			this.App.UI.SetPropertyString("govDescrip", "text", App.Localize("@GOV_DESC_" + governmentInfo.CurrentType.ToString().ToUpper()));
			this.App.UI.Send((object)"SetMarkerPosition", (object)"pol_spectrum", (object)(float)((double)governmentInfo.EconomicLiberalism / (double)this.App.AssetDatabase.MaxGovernmentShift), (object)(float)(-(double)governmentInfo.Authoritarianism / (double)this.App.AssetDatabase.MaxGovernmentShift));
			this.App.UI.SetPropertyBool(EmpireSummaryState.UIGovernmentResearchSlider, "only_user_events", true);
			this.App.UI.SetPropertyBool(EmpireSummaryState.UISecuritySlider, "only_user_events", true);
			this.App.UI.SetPropertyBool(EmpireSummaryState.UIOperationsSlider, "only_user_events", true);
			this.App.UI.SetPropertyBool(EmpireSummaryState.UIIntelSlider, "only_user_events", true);
			this.App.UI.SetPropertyBool(EmpireSummaryState.UICounterIntelSlider, "only_user_events", true);
			this.App.UI.SetPropertyBool(EmpireSummaryState.UIStimulusSlider, "only_user_events", true);
			this.App.UI.SetPropertyBool(EmpireSummaryState.UIMiningSlider, "only_user_events", true);
			this.App.UI.SetPropertyBool(EmpireSummaryState.UIColonizationSlider, "only_user_events", true);
			this.App.UI.SetPropertyBool(EmpireSummaryState.UITradeSlider, "only_user_events", true);
			this.App.UI.SetPropertyBool(EmpireSummaryState.UISavingsSlider, "only_user_events", true);
			this.App.UI.SetPropertyBool(EmpireSummaryState.UICurrentProjectSlider, "only_user_events", true);
			this.App.UI.SetPropertyBool(EmpireSummaryState.UISpecialProjectSlider, "only_user_events", true);
			this.App.UI.SetPropertyBool(EmpireSummaryState.UISalvageResearchSlider, "only_user_events", true);
			this.App.UI.SetPropertyBool(EmpireSummaryState.UITaxesSlider, "only_user_events", true);
			this.App.UI.SetPropertyBool(EmpireSummaryState.UIImmigrationSlider, "only_user_events", true);
			this.App.UI.SetEnabled(EmpireSummaryState.UIOperationsSlider, false);
			this.RefreshAll(string.Empty, true);
			this._crits.Activate();
			this._techCube.SpinSpeed = (float)((1.0 - (double)this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID).RateGovernmentResearch) * 100.0 * (1.0 / 500.0));
			this._techCube.UpdateResearchProgress();
			this._techCube.RefreshResearchingTech();
			this.UpdateTechCubeToolTip();
			this.App.HotKeyManager.AddListener((IKeyBindListener)this);
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this.App.HotKeyManager.RemoveListener((IKeyBindListener)this);
			if (this._crits != null)
			{
				this._crits.Deactivate();
				this._crits.Dispose();
				this._crits = (GameObjectSet)null;
			}
			this._piechart = (BudgetPiechart)null;
		}

		public static void DistributeGovernmentSpending(
		  GameSession App,
		  EmpireSummaryState.GovernmentSpendings lockedBar,
		  float newValue,
		  PlayerInfo pi)
		{
			Dictionary<EmpireSummaryState.GovernmentSpendings, float> ratios = new Dictionary<EmpireSummaryState.GovernmentSpendings, float>()
	  {
		{
		  EmpireSummaryState.GovernmentSpendings.Savings,
		  pi.RateGovernmentSavings
		},
		{
		  EmpireSummaryState.GovernmentSpendings.Security,
		  pi.RateGovernmentSecurity
		},
		{
		  EmpireSummaryState.GovernmentSpendings.Stimulus,
		  pi.RateGovernmentStimulus
		}
	  };
			AlgorithmExtensions.DistributePercentages<EmpireSummaryState.GovernmentSpendings>(ref ratios, lockedBar, newValue);
			pi.RateGovernmentSavings = ratios[EmpireSummaryState.GovernmentSpendings.Savings];
			pi.RateGovernmentSecurity = ratios[EmpireSummaryState.GovernmentSpendings.Security];
			pi.RateGovernmentStimulus = ratios[EmpireSummaryState.GovernmentSpendings.Stimulus];
			App.GameDatabase.UpdatePlayerSliders(App, pi);
		}

		private void DistributeSecuritySpending(
		  EmpireSummaryState.SecuritySpendings lockedBar,
		  float newValue,
		  PlayerInfo pi)
		{
			Dictionary<EmpireSummaryState.SecuritySpendings, float> ratios = new Dictionary<EmpireSummaryState.SecuritySpendings, float>()
	  {
		{
		  EmpireSummaryState.SecuritySpendings.CounterIntel,
		  pi.RateSecurityCounterIntelligence
		},
		{
		  EmpireSummaryState.SecuritySpendings.Intel,
		  pi.RateSecurityIntelligence
		},
		{
		  EmpireSummaryState.SecuritySpendings.Operations,
		  pi.RateSecurityOperations
		}
	  };
			ratios.Remove(EmpireSummaryState.SecuritySpendings.Operations);
			AlgorithmExtensions.DistributePercentages<EmpireSummaryState.SecuritySpendings>(ref ratios, lockedBar, newValue);
			pi.RateSecurityCounterIntelligence = ratios[EmpireSummaryState.SecuritySpendings.CounterIntel];
			pi.RateSecurityIntelligence = ratios[EmpireSummaryState.SecuritySpendings.Intel];
			this.App.GameDatabase.UpdatePlayerSliders(this.App.Game, pi);
		}

		private void DistributeStimulusSpending(
		  EmpireSummaryState.StimulusSpendings lockedBar,
		  float newValue,
		  PlayerInfo pi,
		  bool enableTrade)
		{
			Dictionary<EmpireSummaryState.StimulusSpendings, float> ratios = new Dictionary<EmpireSummaryState.StimulusSpendings, float>()
	  {
		{
		  EmpireSummaryState.StimulusSpendings.Colonization,
		  pi.RateStimulusColonization
		},
		{
		  EmpireSummaryState.StimulusSpendings.Mining,
		  pi.RateStimulusMining
		},
		{
		  EmpireSummaryState.StimulusSpendings.Trade,
		  pi.RateStimulusTrade
		}
	  };
			if (!enableTrade)
				ratios.Remove(EmpireSummaryState.StimulusSpendings.Trade);
			if (!this.App.GameDatabase.PlayerHasTech(this.App.LocalPlayer.ID, "IND_Mega-Strip_Mining"))
				ratios.Remove(EmpireSummaryState.StimulusSpendings.Mining);
			AlgorithmExtensions.DistributePercentages<EmpireSummaryState.StimulusSpendings>(ref ratios, lockedBar, newValue);
			if (this.App.GameDatabase.PlayerHasTech(this.App.LocalPlayer.ID, "IND_Mega-Strip_Mining"))
				pi.RateStimulusMining = ratios[EmpireSummaryState.StimulusSpendings.Mining];
			pi.RateStimulusColonization = ratios[EmpireSummaryState.StimulusSpendings.Colonization];
			if (enableTrade)
				pi.RateStimulusTrade = ratios[EmpireSummaryState.StimulusSpendings.Trade];
			this.App.GameDatabase.UpdatePlayerSliders(this.App.Game, pi);
		}

		public static void DistibuteResearchSpending(
		  GameSession game,
		  GameDatabase db,
		  EmpireSummaryState.ResearchSpendings lockedBar,
		  float newValue,
		  PlayerInfo pi)
		{
			Dictionary<EmpireSummaryState.ResearchSpendings, float> ratios = new Dictionary<EmpireSummaryState.ResearchSpendings, float>()
	  {
		{
		  EmpireSummaryState.ResearchSpendings.CurrentProject,
		  pi.RateResearchCurrentProject
		},
		{
		  EmpireSummaryState.ResearchSpendings.SalvageResearch,
		  pi.RateResearchSalvageResearch
		},
		{
		  EmpireSummaryState.ResearchSpendings.SpecialProject,
		  pi.RateResearchSpecialProject
		}
	  };
			AlgorithmExtensions.DistributePercentages<EmpireSummaryState.ResearchSpendings>(ref ratios, lockedBar, newValue);
			pi.RateResearchCurrentProject = ratios[EmpireSummaryState.ResearchSpendings.CurrentProject];
			pi.RateResearchSalvageResearch = ratios[EmpireSummaryState.ResearchSpendings.SalvageResearch];
			pi.RateResearchSpecialProject = ratios[EmpireSummaryState.ResearchSpendings.SpecialProject];
			db.UpdatePlayerSliders(game, pi);
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (this._piechart.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self))
				return;
			if (msgType == "button_clicked")
			{
				if (panelName == EmpireSummaryState.UIExitButton)
				{
					if (this._prev != null)
					{
						object[] objArray = new object[0];
						if (this._prev.Name == "StarSystemState")
							objArray = new object[2]
							{
				(object) ((BasicStarSystemState) this._prev).CurrentSystem,
				(object) ((BasicStarSystemState) this._prev).SelectedObject
							};
						this.App.SwitchGameState(this._prev, objArray);
					}
					else
						this.App.SwitchGameState<StarMapState>();
				}
				else if (panelName == EmpireSummaryState.UIEmpireButton)
				{
					this.App.UI.SetVisible(EmpireSummaryState.UIGOVScreenPanel, false);
				}
				else
				{
					if (!(panelName == EmpireSummaryState.UIGovernmentButton))
						return;
					this.App.UI.SetVisible(EmpireSummaryState.UIGOVScreenPanel, true);
				}
			}
			else if (msgType == "mouse_enter")
			{
				if (!panelName.StartsWith("government_"))
					return;
				string upper = panelName.Split('_')[1].ToUpper();
				this.App.UI.SetPropertyString(EmpireSummaryState.UIGovernmentType, "text", App.Localize(string.Format("@UI_EMPIRESUMMARY_{0}", (object)upper)));
				this.App.UI.SetPropertyString("govDescrip", "text", App.Localize("@GOV_DESC_" + upper));
			}
			else if (msgType == "mouse_leave")
			{
				if (!panelName.StartsWith("government_"))
					return;
				GovernmentInfo governmentInfo = this.App.GameDatabase.GetGovernmentInfo(this.App.LocalPlayer.ID);
				this.App.UI.SetPropertyString(EmpireSummaryState.UIGovernmentType, "text", App.Localize(string.Format("@UI_EMPIRESUMMARY_{0}", (object)governmentInfo.CurrentType.ToString().ToUpper())));
				this.App.UI.SetPropertyString("govDescrip", "text", App.Localize("@GOV_DESC_" + governmentInfo.CurrentType.ToString().ToUpper()));
			}
			else if (msgType == "slider_value")
			{
				PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID);
				if (panelName == EmpireSummaryState.UITaxesSlider)
				{
					float num = float.Parse(msgParams[0]) / 100f;
					this.App.GameDatabase.UpdateTaxRate(playerInfo.ID, (double)num != 0.0 ? num : 0.0f);
					this.RefreshAll(panelName, false);
				}
				else if (panelName == EmpireSummaryState.UIImmigrationSlider)
				{
					float num = float.Parse(msgParams[0]);
					this.App.GameDatabase.UpdateImmigrationRate(playerInfo.ID, (double)num != 0.0 ? num / 100f : 0.0f);
					this.RefreshAll(panelName, false);
				}
				else if (panelName == EmpireSummaryState.UIGovernmentResearchSlider)
				{
					float num = ((float)int.Parse(msgParams[0]) / 100f).Clamp(0.0f, 1f);
					playerInfo.RateGovernmentResearch = 1f - num;
					this.App.GameDatabase.UpdatePlayerSliders(this.App.Game, playerInfo);
					this.RefreshAll(panelName, false);
					this._techCube.SpinSpeed = (float)int.Parse(msgParams[0]) * (1f / 500f);
					this.UpdateTechCubeToolTip();
				}
				else if (panelName == EmpireSummaryState.UISecuritySlider)
				{
					EmpireSummaryState.DistributeGovernmentSpending(this.App.Game, EmpireSummaryState.GovernmentSpendings.Security, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
					this.RefreshAll(panelName, false);
				}
				else if (panelName == EmpireSummaryState.UIStimulusSlider)
				{
					EmpireSummaryState.DistributeGovernmentSpending(this.App.Game, EmpireSummaryState.GovernmentSpendings.Stimulus, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
					this.RefreshAll(panelName, false);
				}
				else if (panelName == EmpireSummaryState.UISavingsSlider)
				{
					EmpireSummaryState.DistributeGovernmentSpending(this.App.Game, EmpireSummaryState.GovernmentSpendings.Savings, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
					this.RefreshAll(panelName, false);
				}
				else if (panelName == EmpireSummaryState.UIOperationsSlider)
				{
					this.DistributeSecuritySpending(EmpireSummaryState.SecuritySpendings.Operations, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
					this.RefreshAll(panelName, false);
				}
				else if (panelName == EmpireSummaryState.UIIntelSlider)
				{
					this.DistributeSecuritySpending(EmpireSummaryState.SecuritySpendings.Intel, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
					this.RefreshAll(panelName, false);
				}
				else if (panelName == EmpireSummaryState.UICounterIntelSlider)
				{
					this.DistributeSecuritySpending(EmpireSummaryState.SecuritySpendings.CounterIntel, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
					this.RefreshAll(panelName, false);
				}
				else if (panelName == EmpireSummaryState.UIMiningSlider)
				{
					this.DistributeStimulusSpending(EmpireSummaryState.StimulusSpendings.Mining, (float)int.Parse(msgParams[0]) / 100f, playerInfo, this.App.GetStratModifier<bool>(StratModifiers.EnableTrade, this.App.LocalPlayer.ID));
					this.RefreshAll(panelName, false);
				}
				else if (panelName == EmpireSummaryState.UIColonizationSlider)
				{
					this.DistributeStimulusSpending(EmpireSummaryState.StimulusSpendings.Colonization, (float)int.Parse(msgParams[0]) / 100f, playerInfo, this.App.GetStratModifier<bool>(StratModifiers.EnableTrade, this.App.LocalPlayer.ID));
					this.RefreshAll(panelName, false);
				}
				else if (panelName == EmpireSummaryState.UITradeSlider)
				{
					this.DistributeStimulusSpending(EmpireSummaryState.StimulusSpendings.Trade, (float)int.Parse(msgParams[0]) / 100f, playerInfo, this.App.GetStratModifier<bool>(StratModifiers.EnableTrade, this.App.LocalPlayer.ID));
					this.RefreshAll(panelName, false);
				}
				else if (panelName == EmpireSummaryState.UICurrentProjectSlider)
				{
					EmpireSummaryState.DistibuteResearchSpending(this.App.Game, this.App.GameDatabase, EmpireSummaryState.ResearchSpendings.CurrentProject, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
					this.RefreshAll(panelName, false);
				}
				else if (panelName == EmpireSummaryState.UISpecialProjectSlider)
				{
					EmpireSummaryState.DistibuteResearchSpending(this.App.Game, this.App.GameDatabase, EmpireSummaryState.ResearchSpendings.SpecialProject, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
					this.RefreshAll(panelName, false);
				}
				else
				{
					if (!(panelName == EmpireSummaryState.UISalvageResearchSlider))
						return;
					EmpireSummaryState.DistibuteResearchSpending(this.App.Game, this.App.GameDatabase, EmpireSummaryState.ResearchSpendings.SalvageResearch, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
					this.RefreshAll(panelName, false);
				}
			}
			else
			{
				if (!(msgType == "slider_notched"))
					return;
				int num = int.Parse(msgParams[0]);
				if (!(panelName == EmpireSummaryState.UISecuritySlider))
					return;
				if (num != -1)
					this.App.GameDatabase.InsertUISliderNotchSetting(this.App.LocalPlayer.ID, UISlidertype.SecuritySlider, 0.0, 0);
				else
					this.App.GameDatabase.DeleteUISliderNotchSetting(this.App.LocalPlayer.ID, UISlidertype.SecuritySlider);
			}
		}

		private void RefreshAll(string panelName, bool BuildLists = false)
		{
			bool stratModifier = this.App.GetStratModifier<bool>(StratModifiers.EnableTrade, this.App.LocalPlayer.ID);
			PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID);
			Budget budget = Budget.GenerateBudget(this.App.Game, playerInfo, (IEnumerable<DesignInfo>)null, BudgetProjection.Pessimistic);
			int numColonies = this.App.GameDatabase.GetNumColonies(this.App.LocalPlayer.ID);
			int numProvinces = this.App.GameDatabase.GetNumProvinces(this.App.LocalPlayer.ID);
			int num1 = this.App.GameDatabase.GetStationInfosByPlayerID(this.App.LocalPlayer.ID).Count<StationInfo>();
			int num2 = this.App.GameDatabase.GetFleetInfosByPlayerID(this.App.LocalPlayer.ID, FleetType.FL_NORMAL).Count<FleetInfo>();
			int numShips = this.App.GameDatabase.GetNumShips(this.App.LocalPlayer.ID);
			double empirePopulation = this.App.GameDatabase.GetEmpirePopulation(this.App.LocalPlayer.ID);
			float empireEconomy = this.App.GameDatabase.GetEmpireEconomy(this.App.LocalPlayer.ID);
			int empireBiosphere = this.App.GameDatabase.GetEmpireBiosphere(this.App.LocalPlayer.ID);
			double tradeRevenue = budget.TradeRevenue;
			int? empireMorale1 = this.App.GameDatabase.GetEmpireMorale(this.App.LocalPlayer.ID);
			double num3 = (1.0 - (double)playerInfo.RateGovernmentResearch) * 100.0;
			double num4 = (double)playerInfo.RateGovernmentSecurity * 100.0;
			double num5 = (double)playerInfo.RateSecurityOperations * 100.0;
			double num6 = (double)playerInfo.RateSecurityIntelligence * 100.0;
			double num7 = (double)playerInfo.RateSecurityCounterIntelligence * 100.0;
			double num8 = (double)playerInfo.RateGovernmentStimulus * 100.0;
			double num9 = (double)playerInfo.RateStimulusMining * 100.0;
			double num10 = (double)playerInfo.RateStimulusColonization * 100.0;
			double num11 = (double)playerInfo.RateStimulusTrade * 100.0;
			double num12 = (double)playerInfo.RateGovernmentSavings * 100.0;
			double governmentResearch = (double)playerInfo.RateGovernmentResearch;
			double num13 = (double)playerInfo.RateResearchCurrentProject * 100.0;
			double num14 = (double)playerInfo.RateResearchSpecialProject * 100.0;
			double num15 = (double)playerInfo.RateResearchSalvageResearch * 100.0;
			string text1 = budget.TotalRevenue.ToString("N0");
			string text2 = budget.ProjectedGovernmentSpending.ToString("N0");
			string text3 = budget.ResearchSpending.ProjectedTotal.ToString("N0");
			string text4 = budget.SecuritySpending.ProjectedTotal.ToString("N0");
			string text5 = budget.SecuritySpending.ProjectedOperations.ToString("N0");
			string text6 = budget.SecuritySpending.ProjectedIntelligence.ToString("N0");
			string text7 = budget.SecuritySpending.ProjectedCounterIntelligence.ToString("N0");
			string text8 = budget.StimulusSpending.ProjectedTotal.ToString("N0");
			string text9 = budget.StimulusSpending.ProjectedMining.ToString("N0");
			string text10 = budget.StimulusSpending.ProjectedColonization.ToString("N0");
			string text11 = budget.StimulusSpending.ProjectedTrade.ToString("N0");
			string text12 = budget.NetSavingsIncome.ToString("N0");
			string text13 = budget.CurrentSavings.ToString("N0");
			string text14 = budget.SavingsInterest.ToString("N0");
			string text15 = budget.ProjectedSavings.ToString("N0");
			string text16 = budget.ResearchSpending.ProjectedCurrentProject.ToString("N0");
			string text17 = budget.ResearchSpending.ProjectedSpecialProject.ToString("N0");
			string text18 = budget.ResearchSpending.ProjectedSalvageResearch.ToString("N0");
			string text19 = (budget.ColonySupportExpenses + budget.CurrentShipUpkeepExpenses + budget.CurrentStationUpkeepExpenses + budget.CorruptionExpenses + budget.DebtInterest).ToString("N0");
			string text20 = (budget.TotalExpenses + budget.PendingBuildStationsCost + budget.PendingStationsModulesCost + budget.PendingBuildShipsCost).ToString("N0");
			string text21 = budget.ColonySupportExpenses.ToString("N0");
			string text22 = budget.CurrentShipUpkeepExpenses.ToString("N0");
			string text23 = budget.CurrentStationUpkeepExpenses.ToString("N0");
			string text24 = budget.CorruptionExpenses.ToString("N0");
			string text25 = budget.DebtInterest.ToString("N0");
			string text26 = (budget.IORevenue + budget.TaxRevenue).ToString("N0");
			string text27 = text2;
			string text28 = text20;
			string text29 = text3;
			string text30 = text13;
			string text31 = text15;
			string str1 = numColonies.ToString("N0");
			string str2 = numProvinces.ToString("N0");
			string text32 = num1.ToString("N0");
			string text33 = num2.ToString("N0");
			string text34 = numShips.ToString("N0");
			string text35 = empirePopulation.ToString("N0");
			string text36 = (empireEconomy * 100f).ToString("N0");
			string text37 = empireBiosphere.ToString("N0");
			string text38 = tradeRevenue.ToString("N0");
			int turnCount;
			string str3;
			if (!empireMorale1.HasValue)
			{
				str3 = "n/a";
			}
			else
			{
				turnCount = empireMorale1.Value;
				str3 = turnCount.ToString("N0");
			}
			string text39 = str3;
			string text40 = budget.PendingBuildShipsCost.ToString("N0");
			string text41 = (budget.PendingBuildStationsCost + budget.PendingStationsModulesCost).ToString("N0");
			string text42 = (budget.TotalExpenses + budget.TotalBuildShipCosts + budget.TotalBuildStationsCost + budget.TotalStationsModulesCost).ToString("N0");
			turnCount = this.App.GameDatabase.GetTurnCount();
			string text43 = "Turn " + turnCount.ToString("N0");
			this.App.UI.SetEnabled(EmpireSummaryState.UITradeSlider, stratModifier);
			this.App.UI.SetEnabled(EmpireSummaryState.UIMiningSlider, this.App.GameDatabase.PlayerHasTech(this.App.LocalPlayer.ID, "IND_Mega-Strip_Mining"));
			if (this.App.LocalPlayer.Faction.Name == "loa" && this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID).Savings < 0.0)
				this.App.UI.SetEnabled(EmpireSummaryState.UIGovernmentResearchSlider, false);
			else
				this.App.UI.SetEnabled(EmpireSummaryState.UIGovernmentResearchSlider, true);
			if (panelName != EmpireSummaryState.UIGovernmentResearchSlider)
				this.App.UI.SetSliderValue(EmpireSummaryState.UIGovernmentResearchSlider, (int)num3);
			if (panelName != EmpireSummaryState.UISecuritySlider)
				this.App.UI.SetSliderValue(EmpireSummaryState.UISecuritySlider, (int)num4);
			if (budget.ProjectedGovernmentSpending > 0.0)
			{
				this.App.UI.ClearSliderNotches(EmpireSummaryState.UISecuritySlider);
				this.App.UI.AddSliderNotch(EmpireSummaryState.UISecuritySlider, budget.RequiredSecurity);
				if (this.App.GameDatabase.GetSliderNotchSettingInfo(this.App.LocalPlayer.ID, UISlidertype.SecuritySlider) != null)
				{
					this.App.UI.SetSliderValue(EmpireSummaryState.UISecuritySlider, budget.RequiredSecurity);
					EmpireSummaryState.DistributeGovernmentSpending(this.App.Game, EmpireSummaryState.GovernmentSpendings.Security, (float)Math.Min((double)budget.RequiredSecurity / 100.0, 1.0), playerInfo);
				}
			}
			if (panelName != EmpireSummaryState.UIOperationsSlider)
				this.App.UI.SetSliderValue(EmpireSummaryState.UIOperationsSlider, (int)num5);
			if (panelName != EmpireSummaryState.UIIntelSlider)
				this.App.UI.SetSliderValue(EmpireSummaryState.UIIntelSlider, (int)num6);
			if (panelName != EmpireSummaryState.UICounterIntelSlider)
				this.App.UI.SetSliderValue(EmpireSummaryState.UICounterIntelSlider, (int)num7);
			if (panelName != EmpireSummaryState.UIStimulusSlider)
				this.App.UI.SetSliderValue(EmpireSummaryState.UIStimulusSlider, (int)num8);
			if (panelName != EmpireSummaryState.UIMiningSlider)
				this.App.UI.SetSliderValue(EmpireSummaryState.UIMiningSlider, (int)num9);
			if (panelName != EmpireSummaryState.UIColonizationSlider)
				this.App.UI.SetSliderValue(EmpireSummaryState.UIColonizationSlider, (int)num10);
			if (panelName != EmpireSummaryState.UITradeSlider)
				this.App.UI.SetSliderValue(EmpireSummaryState.UITradeSlider, (int)num11);
			if (panelName != EmpireSummaryState.UISavingsSlider)
				this.App.UI.SetSliderValue(EmpireSummaryState.UISavingsSlider, (int)num12);
			if (panelName != EmpireSummaryState.UICurrentProjectSlider)
				this.App.UI.SetSliderValue(EmpireSummaryState.UICurrentProjectSlider, (int)num13);
			if (panelName != EmpireSummaryState.UISpecialProjectSlider)
				this.App.UI.SetSliderValue(EmpireSummaryState.UISpecialProjectSlider, (int)num14);
			if (panelName != EmpireSummaryState.UISalvageResearchSlider)
				this.App.UI.SetSliderValue(EmpireSummaryState.UISalvageResearchSlider, (int)num15);
			this.App.UI.SetText("incomeValue", text1);
			this.App.UI.SetText("governmentValue", text2);
			this.App.UI.SetText("researchValue", text3);
			this.App.UI.SetText("researchTotalValue", text3);
			this.App.UI.SetText("securityValue", text4);
			this.App.UI.SetText("operationsValue", text5);
			this.App.UI.SetText("intelValue", text6);
			this.App.UI.SetText("counterIntelValue", text7);
			this.App.UI.SetText("stimulusValue", text8);
			this.App.UI.SetText("miningValue", text9);
			this.App.UI.SetText("colonizationValue", text10);
			this.App.UI.SetText("tradeValue", text11);
			this.App.UI.SetText("savingsValue", text12);
			this.App.UI.SetText("interestValue", text14);
			this.App.UI.SetText("treasuryValue", text13);
			this.App.UI.SetText("projectedTreasuryValue", text15);
			this.App.UI.SetText("currentProjectValue", text16);
			this.App.UI.SetText("specialProjectValue", text17);
			this.App.UI.SetText("salvageResearchValue", text18);
			this.App.UI.SetText("expensesValue", text19);
			this.App.UI.SetText("colonyDevelopmentValue", text21);
			this.App.UI.SetText("fleetMaintenanceValue", text22);
			this.App.UI.SetText("stationMaintenanceValue", text23);
			this.App.UI.SetText("embezzlementValue", text24);
			this.App.UI.SetText("debtInterestValue", text25);
			this.App.UI.SetVisible("embezzlementValue", this.App.LocalPlayer.Faction.Name != "loa");
			this.App.UI.SetVisible("debtInterestValue", this.App.LocalPlayer.Faction.Name != "loa");
			this.App.UI.SetVisible("interestValue", this.App.LocalPlayer.Faction.Name != "loa");
			this.App.UI.SetVisible("InterestLabel", this.App.LocalPlayer.Faction.Name != "loa");
			this.App.UI.SetVisible("embezzlementString", this.App.LocalPlayer.Faction.Name != "loa");
			this.App.UI.SetVisible("debtString", this.App.LocalPlayer.Faction.Name != "loa");
			this.App.UI.SetText("summaryGovernmentValue", text27);
			this.App.UI.SetText("summaryExpensesValue", text28);
			this.App.UI.SetText("summaryResearchValue", text29);
			this.App.UI.SetText("summarySavingsValue", text30);
			this.App.UI.SetText("summaryProjectedValue", text31);
			this.App.UI.SetText("turn_label", text43);
			this.App.UI.SetText(EmpireSummaryState.UIShipInvoicesCost, text40);
			this.App.UI.SetText(EmpireSummaryState.UIStationInvoicesCost, text41);
			this.App.UI.SetText(EmpireSummaryState.UIColonyDevCost, text21);
			this.App.UI.SetText(EmpireSummaryState.UIColonyIncome, text26);
			this.App.UI.SetText(EmpireSummaryState.UIFinancialDue, text20);
			this.App.UI.SetText(EmpireSummaryState.UIFinancialDueTotal, text42);
			this.App.UI.SetPropertyColor("summaryProjectedValue", "color", (float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue);
			this.App.UI.SetText("coloniesvalue", str1 + " " + App.Localize("@UI_PLANET_MANAGER_OWNED_PLANETS"));
			this.App.UI.SetText("provincesvalue", str2 + " " + App.Localize("@UI_STARMAPVIEW_PROVINCE_DISPLAY"));
			this.App.UI.SetText("basesvalue", text32);
			this.App.UI.SetText("fleetsvalue", text33);
			this.App.UI.SetText("shipsvalue", text34);
			this.App.UI.SetText("populationvalue", text35);
			this.App.UI.SetText("economyvalue", text36);
			this.App.UI.SetText("biospherevalue", text37);
			this.App.UI.SetText("tradeunitsvalue", text38);
			this.App.UI.SetText("averagemoralevalue", text39);
			if (BuildLists)
			{
				List<int> list1 = this.App.GameDatabase.GetPlayerColonySystemIDs(playerInfo.ID).ToList<int>();
				this.App.UI.ClearItems(EmpireSummaryState.UIShipInvoicesList);
				List<int> ShipOrdersDueNextTurn = new List<int>();
				foreach (int num16 in list1)
				{
					List<BuildOrderInfo> list2 = this.App.GameDatabase.GetBuildOrdersForSystem(num16).ToList<BuildOrderInfo>();
					float num17 = 0.0f;
					foreach (ColonyInfo colony in this.App.GameDatabase.GetColonyInfosForSystem(num16).ToList<ColonyInfo>())
					{
						if (colony.PlayerID == playerInfo.ID)
							num17 += Colony.GetConstructionPoints(this.App.Game, colony);
					}
					float num18 = num17 * this.App.Game.GetStationBuildModifierForSystem(num16, playerInfo.ID);
					foreach (BuildOrderInfo buildOrderInfo in list2)
					{
						DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(buildOrderInfo.DesignID);
						if (designInfo.PlayerID == playerInfo.ID)
						{
							int num19 = designInfo.SavingsCost;
							if (designInfo.IsLoaCube())
								num19 = buildOrderInfo.LoaCubes * this.App.AssetDatabase.LoaCostPerCube;
							int num20 = buildOrderInfo.ProductionTarget - buildOrderInfo.Progress;
							if ((double)num20 <= (double)num18)
							{
								ShipOrdersDueNextTurn.Add(buildOrderInfo.ID);
								this.App.UI.AddItem(EmpireSummaryState.UIShipInvoicesList, "", buildOrderInfo.ID, "");
								string itemGlobalId = this.App.UI.GetItemGlobalID(EmpireSummaryState.UIShipInvoicesList, "", buildOrderInfo.ID, "");
								this.App.UI.SetText(this.App.UI.Path(itemGlobalId, "designName"), designInfo.Name + " - " + buildOrderInfo.ShipName);
								this.App.UI.SetPropertyColor(this.App.UI.Path(itemGlobalId, "designName"), "color", 50f, 202f, 240f);
								this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_idle"), num19.ToString("N0") + " | " + this.App.GameDatabase.GetStarSystemInfo(num16).Name);
								this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_sel"), num19.ToString("N0") + " | " + this.App.GameDatabase.GetStarSystemInfo(num16).Name);
								this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "designDeleteButton"), false);
								num18 -= (float)num20;
							}
						}
					}
				}
				foreach (int systemId in list1)
				{
					foreach (BuildOrderInfo buildOrderInfo in this.App.GameDatabase.GetBuildOrdersForSystem(systemId).Where<BuildOrderInfo>((Func<BuildOrderInfo, bool>)(x => !ShipOrdersDueNextTurn.Contains(x.ID))).ToList<BuildOrderInfo>())
					{
						DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(buildOrderInfo.DesignID);
						if (designInfo.PlayerID == playerInfo.ID)
						{
							int num16 = designInfo.SavingsCost;
							if (designInfo.IsLoaCube())
								num16 = buildOrderInfo.LoaCubes * this.App.AssetDatabase.LoaCostPerCube;
							this.App.UI.AddItem(EmpireSummaryState.UIShipInvoicesList, "", buildOrderInfo.ID, "");
							string itemGlobalId = this.App.UI.GetItemGlobalID(EmpireSummaryState.UIShipInvoicesList, "", buildOrderInfo.ID, "");
							this.App.UI.SetText(this.App.UI.Path(itemGlobalId, "designName"), designInfo.Name + " - " + buildOrderInfo.ShipName);
							this.App.UI.SetPropertyColor(this.App.UI.Path(itemGlobalId, "designName"), "color", 250f, 170f, 50f);
							this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_idle"), num16.ToString("N0") + " | " + this.App.GameDatabase.GetStarSystemInfo(systemId).Name);
							this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_sel"), num16.ToString("N0") + " | " + this.App.GameDatabase.GetStarSystemInfo(systemId).Name);
							this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "designDeleteButton"), false);
						}
					}
				}
				this.App.UI.ClearItems(EmpireSummaryState.UIStationInvoicesList);
				List<int> ConstMissionsDue = new List<int>();
				List<int> ConstModulesDue = new List<int>();
				foreach (MissionInfo missionInfo in this.App.GameDatabase.GetMissionInfos().Where<MissionInfo>((Func<MissionInfo, bool>)(x => x.Type == MissionType.CONSTRUCT_STN)))
				{
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(missionInfo.FleetID);
					if (fleetInfo.PlayerID == playerInfo.ID)
					{
						MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this.App.Game, missionInfo.Type, (StationType)missionInfo.StationType.Value, fleetInfo.ID, missionInfo.TargetSystemID, missionInfo.TargetOrbitalObjectID, (List<int>)null, 1, false, new float?(), new float?());
						if (missionEstimate.TotalTurns - 1 - missionEstimate.TurnsToReturn <= 1)
						{
							ConstMissionsDue.Add(missionInfo.ID);
							this.App.UI.AddItem(EmpireSummaryState.UIStationInvoicesList, "", missionInfo.ID, "");
							string itemGlobalId = this.App.UI.GetItemGlobalID(EmpireSummaryState.UIStationInvoicesList, "", missionInfo.ID, "");
							this.App.UI.SetText(this.App.UI.Path(itemGlobalId, "designName"), "Build " + ((StationType)missionInfo.StationType.Value).ToDisplayText(this.App.AssetDatabase.GetFaction(playerInfo.FactionID).Name));
							this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_idle"), missionEstimate.ConstructionCost.ToString("N0") + " | " + this.App.GameDatabase.GetStarSystemInfo(missionInfo.TargetSystemID).Name);
							this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_sel"), missionEstimate.ConstructionCost.ToString("N0") + " | " + this.App.GameDatabase.GetStarSystemInfo(missionInfo.TargetSystemID).Name);
							this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "designDeleteButton"), false);
							this.App.UI.SetPropertyColor(this.App.UI.Path(itemGlobalId, "designName"), "color", 50f, 202f, 240f);
						}
					}
				}
				foreach (MissionInfo missionInfo in this.App.GameDatabase.GetMissionInfos().Where<MissionInfo>((Func<MissionInfo, bool>)(x =>
			   {
				   if (x.Type == MissionType.UPGRADE_STN)
					   return x.Duration > 0;
				   return false;
			   })))
				{
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(missionInfo.FleetID);
					if (this.App.GameDatabase.GetStationInfo(missionInfo.TargetOrbitalObjectID) != null && fleetInfo.PlayerID == playerInfo.ID && missionInfo.Duration > 0)
					{
						StationInfo stationInfo = this.App.GameDatabase.GetStationInfo(missionInfo.TargetOrbitalObjectID);
						if (stationInfo.DesignInfo.StationLevel + 1 <= 5)
						{
							MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this.App.Game, missionInfo.Type, stationInfo.DesignInfo.StationType, fleetInfo.ID, missionInfo.TargetSystemID, missionInfo.TargetOrbitalObjectID, (List<int>)null, stationInfo.DesignInfo.StationLevel + 1, false, new float?(), new float?());
							if (missionEstimate.TotalTurns - 1 - missionEstimate.TurnsToReturn <= 1)
							{
								DesignInfo stationDesignInfo = DesignLab.CreateStationDesignInfo(this.App.AssetDatabase, this.App.GameDatabase, fleetInfo.PlayerID, stationInfo.DesignInfo.StationType, stationInfo.DesignInfo.StationLevel + 1, false);
								ConstMissionsDue.Add(missionInfo.ID);
								this.App.UI.AddItem(EmpireSummaryState.UIStationInvoicesList, "", missionInfo.ID, "");
								string itemGlobalId = this.App.UI.GetItemGlobalID(EmpireSummaryState.UIStationInvoicesList, "", missionInfo.ID, "");
								this.App.UI.SetText(this.App.UI.Path(itemGlobalId, "designName"), "Upgrade " + stationInfo.DesignInfo.Name);
								this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_idle"), stationDesignInfo.SavingsCost.ToString("N0") + " | " + this.App.GameDatabase.GetStarSystemInfo(missionInfo.TargetSystemID).Name);
								this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_sel"), stationDesignInfo.SavingsCost.ToString("N0") + " | " + this.App.GameDatabase.GetStarSystemInfo(missionInfo.TargetSystemID).Name);
								this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "designDeleteButton"), false);
								this.App.UI.SetPropertyColor(this.App.UI.Path(itemGlobalId, "designName"), "color", 50f, 202f, 240f);
							}
						}
					}
				}
				foreach (StationInfo stationInfo in this.App.GameDatabase.GetStationInfosByPlayerID(playerInfo.ID))
				{
					List<DesignModuleInfo> queuedModules = this.App.GameDatabase.GetQueuedStationModules(stationInfo.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
					if (queuedModules.Count > 0)
					{
						LogicalModule logicalModule = this.App.AssetDatabase.Modules.First<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == this.App.GameDatabase.GetModuleAsset(queuedModules.First<DesignModuleInfo>().ModuleID)));
						if (logicalModule != null)
						{
							StationModules.StationModule stationModule = ((IEnumerable<StationModules.StationModule>)StationModules.Modules).Where<StationModules.StationModule>((Func<StationModules.StationModule, bool>)(x =>
						  {
							  ModuleEnums.StationModuleType smType = x.SMType;
							  ModuleEnums.StationModuleType? stationModuleType = queuedModules.First<DesignModuleInfo>().StationModuleType;
							  if (smType == stationModuleType.GetValueOrDefault())
								  return stationModuleType.HasValue;
							  return false;
						  })).First<StationModules.StationModule>();
							ConstModulesDue.Add(queuedModules.First<DesignModuleInfo>().ID);
							this.App.UI.AddItem(EmpireSummaryState.UIStationInvoicesList, "", 100000 + queuedModules.First<DesignModuleInfo>().ID, "");
							string itemGlobalId = this.App.UI.GetItemGlobalID(EmpireSummaryState.UIStationInvoicesList, "", 100000 + queuedModules.First<DesignModuleInfo>().ID, "");
							this.App.UI.SetText(this.App.UI.Path(itemGlobalId, "designName"), stationInfo.DesignInfo.Name + " - " + stationModule.Name);
							this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_idle"), logicalModule.SavingsCost.ToString("N0"));
							this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_sel"), logicalModule.SavingsCost.ToString("N0"));
							this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "designDeleteButton"), false);
							this.App.UI.SetPropertyColor(this.App.UI.Path(itemGlobalId, "designName"), "color", 50f, 202f, 240f);
						}
					}
				}
				foreach (MissionInfo missionInfo in this.App.GameDatabase.GetMissionInfos().Where<MissionInfo>((Func<MissionInfo, bool>)(x =>
			   {
				   if (x.Type == MissionType.CONSTRUCT_STN)
					   return !ConstMissionsDue.Contains(x.ID);
				   return false;
			   })))
				{
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(missionInfo.FleetID);
					if (fleetInfo.PlayerID == playerInfo.ID)
					{
						MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this.App.Game, missionInfo.Type, (StationType)missionInfo.StationType.Value, fleetInfo.ID, missionInfo.TargetSystemID, missionInfo.TargetOrbitalObjectID, (List<int>)null, 1, false, new float?(), new float?());
						this.App.UI.AddItem(EmpireSummaryState.UIStationInvoicesList, "", missionInfo.ID, "");
						string itemGlobalId = this.App.UI.GetItemGlobalID(EmpireSummaryState.UIStationInvoicesList, "", missionInfo.ID, "");
						this.App.UI.SetText(this.App.UI.Path(itemGlobalId, "designName"), "Build " + ((StationType)missionInfo.StationType.Value).ToDisplayText(this.App.AssetDatabase.GetFaction(playerInfo.FactionID).Name));
						this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_idle"), missionEstimate.ConstructionCost.ToString("N0") + " | " + this.App.GameDatabase.GetStarSystemInfo(missionInfo.TargetSystemID).Name);
						this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_sel"), missionEstimate.ConstructionCost.ToString("N0") + " | " + this.App.GameDatabase.GetStarSystemInfo(missionInfo.TargetSystemID).Name);
						this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "designDeleteButton"), false);
						this.App.UI.SetPropertyColor(this.App.UI.Path(itemGlobalId, "designName"), "color", 250f, 170f, 50f);
					}
				}
				foreach (MissionInfo missionInfo in this.App.GameDatabase.GetMissionInfos().Where<MissionInfo>((Func<MissionInfo, bool>)(x =>
			   {
				   if (x.Type == MissionType.UPGRADE_STN)
					   return !ConstMissionsDue.Contains(x.ID);
				   return false;
			   })))
				{
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(missionInfo.FleetID);
					if (this.App.GameDatabase.GetStationInfo(missionInfo.TargetOrbitalObjectID) != null && fleetInfo.PlayerID == playerInfo.ID && missionInfo.Duration > 0)
					{
						StationInfo stationInfo = this.App.GameDatabase.GetStationInfo(missionInfo.TargetOrbitalObjectID);
						if (stationInfo.DesignInfo.StationLevel + 1 <= 5)
						{
							Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this.App.Game, missionInfo.Type, stationInfo.DesignInfo.StationType, fleetInfo.ID, missionInfo.TargetSystemID, missionInfo.TargetOrbitalObjectID, (List<int>)null, stationInfo.DesignInfo.StationLevel + 1, false, new float?(), new float?());
							DesignInfo stationDesignInfo = DesignLab.CreateStationDesignInfo(this.App.AssetDatabase, this.App.GameDatabase, fleetInfo.PlayerID, stationInfo.DesignInfo.StationType, stationInfo.DesignInfo.StationLevel + 1, false);
							this.App.UI.AddItem(EmpireSummaryState.UIStationInvoicesList, "", missionInfo.ID, "");
							string itemGlobalId = this.App.UI.GetItemGlobalID(EmpireSummaryState.UIStationInvoicesList, "", missionInfo.ID, "");
							this.App.UI.SetText(this.App.UI.Path(itemGlobalId, "designName"), "Upgrade " + stationInfo.DesignInfo.Name);
							this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_idle"), stationDesignInfo.SavingsCost.ToString("N0") + " | " + this.App.GameDatabase.GetStarSystemInfo(missionInfo.TargetSystemID).Name);
							this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_sel"), stationDesignInfo.SavingsCost.ToString("N0") + " | " + this.App.GameDatabase.GetStarSystemInfo(missionInfo.TargetSystemID).Name);
							this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "designDeleteButton"), false);
							this.App.UI.SetPropertyColor(this.App.UI.Path(itemGlobalId, "designName"), "color", 250f, 170f, 50f);
						}
					}
				}
				foreach (StationInfo stationInfo in this.App.GameDatabase.GetStationInfosByPlayerID(playerInfo.ID))
				{
					foreach (DesignModuleInfo designModuleInfo in this.App.GameDatabase.GetQueuedStationModules(stationInfo.DesignInfo.DesignSections[0]).Where<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x => !ConstModulesDue.Contains(x.ID))).ToList<DesignModuleInfo>())
					{
						DesignModuleInfo smod = designModuleInfo;
						LogicalModule logicalModule = this.App.AssetDatabase.Modules.First<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == this.App.GameDatabase.GetModuleAsset(smod.ModuleID)));
						if (logicalModule != null)
						{
							StationModules.StationModule stationModule = ((IEnumerable<StationModules.StationModule>)StationModules.Modules).Where<StationModules.StationModule>((Func<StationModules.StationModule, bool>)(x =>
						  {
							  ModuleEnums.StationModuleType smType = x.SMType;
							  ModuleEnums.StationModuleType? stationModuleType = smod.StationModuleType;
							  if (smType == stationModuleType.GetValueOrDefault())
								  return stationModuleType.HasValue;
							  return false;
						  })).First<StationModules.StationModule>();
							this.App.UI.AddItem(EmpireSummaryState.UIStationInvoicesList, "", 100000 + smod.ID, "");
							string itemGlobalId = this.App.UI.GetItemGlobalID(EmpireSummaryState.UIStationInvoicesList, "", 100000 + smod.ID, "");
							this.App.UI.SetText(this.App.UI.Path(itemGlobalId, "designName"), stationInfo.DesignInfo.Name + " - " + stationModule.Name);
							this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_idle"), logicalModule.SavingsCost.ToString("N0"));
							this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_sel"), logicalModule.SavingsCost.ToString("N0"));
							this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "designDeleteButton"), false);
							this.App.UI.SetPropertyColor(this.App.UI.Path(itemGlobalId, "designName"), "color", 250f, 170f, 50f);
						}
					}
				}
				List<ColonyInfo> list3 = this.App.GameDatabase.GetColonyInfos().Where<ColonyInfo>((Func<ColonyInfo, bool>)(x =>
			   {
				   if (x.PlayerID == this.App.LocalPlayer.ID)
					   return Colony.GetColonySupportCost(this.App.AssetDatabase, this.App.GameDatabase, x) > 0.0;
				   return false;
			   })).ToList<ColonyInfo>();
				FleetUI.SyncPlanetListControl(this.App.Game, EmpireSummaryState.UIColonyDevList, list3.Select<ColonyInfo, int>((Func<ColonyInfo, int>)(x => x.OrbitalObjectID)));
				foreach (ColonyInfo colonyInfo in list3)
				{
					string itemGlobalId = this.App.UI.GetItemGlobalID(EmpireSummaryState.UIColonyDevList, "", colonyInfo.OrbitalObjectID, "");
					this.App.UI.SetText(this.App.UI.Path(itemGlobalId, "colonycostlbl"), Colony.GetColonySupportCost(this.App.AssetDatabase, this.App.GameDatabase, colonyInfo).ToString("N0"));
					StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(colonyInfo.CachedStarSystemID);
					this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_sel"), starSystemInfo.Name);
					this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_idle"), starSystemInfo.Name);
				}
				List<ColonyInfo> list4 = this.App.GameDatabase.GetColonyInfos().Where<ColonyInfo>((Func<ColonyInfo, bool>)(x =>
			   {
				   if (x.PlayerID == this.App.LocalPlayer.ID)
					   return Colony.GetTaxRevenue(this.App, playerInfo, x) >= 1.0;
				   return false;
			   })).ToList<ColonyInfo>();
				FleetUI.SyncPlanetListControl(this.App.Game, EmpireSummaryState.UIColonyIncomeList, list4.Select<ColonyInfo, int>((Func<ColonyInfo, int>)(x => x.OrbitalObjectID)));
				foreach (ColonyInfo colony in list4)
				{
					string itemGlobalId = this.App.UI.GetItemGlobalID(EmpireSummaryState.UIColonyIncomeList, "", colony.OrbitalObjectID, "");
					this.App.UI.SetText(this.App.UI.Path(itemGlobalId, "colonycostlbl"), Colony.GetTaxRevenue(this.App, playerInfo, colony).ToString("N0"));
					StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(colony.CachedStarSystemID);
					this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_sel"), starSystemInfo.Name);
					this.App.UI.SetTooltip(this.App.UI.Path(itemGlobalId, "header_idle"), starSystemInfo.Name);
				}
			}
			foreach (ColonyInfo colony in this.App.GameDatabase.GetColonyInfos().Where<ColonyInfo>((Func<ColonyInfo, bool>)(x =>
		   {
			   if (x.PlayerID == this.App.LocalPlayer.ID)
				   return Colony.GetTaxRevenue(this.App, playerInfo, x) >= 1.0;
			   return false;
		   })).ToList<ColonyInfo>())
				this.App.UI.SetText(this.App.UI.Path(this.App.UI.GetItemGlobalID(EmpireSummaryState.UIColonyIncomeList, "", colony.OrbitalObjectID, ""), "colonycostlbl"), Colony.GetTaxRevenue(this.App, playerInfo, colony).ToString("N0"));
			EmpireHistoryData historyForPlayer = this.App.GameDatabase.GetLastEmpireHistoryForPlayer(this.App.LocalPlayer.ID);
			Vector3 vector3_1 = new Vector3((float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue);
			Vector3 vector3_2 = new Vector3(0.0f, (float)byte.MaxValue, 0.0f);
			Vector3 vector3_3 = new Vector3((float)byte.MaxValue, 0.0f, 0.0f);
			if (historyForPlayer != null)
			{
				if (numColonies > historyForPlayer.colonies)
					this.App.UI.SetPropertyColor("coloniesvalue", "color", vector3_2);
				else if (numColonies < historyForPlayer.colonies)
					this.App.UI.SetPropertyColor("coloniesvalue", "color", vector3_3);
				else
					this.App.UI.SetPropertyColor("coloniesvalue", "color", vector3_1);
				if (numProvinces > historyForPlayer.provinces)
					this.App.UI.SetPropertyColor("provincesvalue", "color", vector3_2);
				else if (numProvinces < historyForPlayer.provinces)
					this.App.UI.SetPropertyColor("provincesvalue", "color", vector3_3);
				else
					this.App.UI.SetPropertyColor("provincesvalue", "color", vector3_1);
				if (num1 > historyForPlayer.bases)
					this.App.UI.SetPropertyColor("basesvalue", "color", vector3_2);
				else if (num1 < historyForPlayer.bases)
					this.App.UI.SetPropertyColor("basesvalue", "color", vector3_3);
				else
					this.App.UI.SetPropertyColor("basesvalue", "color", vector3_1);
				if (num2 > historyForPlayer.fleets)
					this.App.UI.SetPropertyColor("fleetsvalue", "color", vector3_2);
				else if (num2 < historyForPlayer.fleets)
					this.App.UI.SetPropertyColor("fleetsvalue", "color", vector3_3);
				else
					this.App.UI.SetPropertyColor("fleetsvalue", "color", vector3_1);
				if (numShips > historyForPlayer.ships)
					this.App.UI.SetPropertyColor("shipsvalue", "color", vector3_2);
				else if (numShips < historyForPlayer.ships)
					this.App.UI.SetPropertyColor("shipsvalue", "color", vector3_3);
				else
					this.App.UI.SetPropertyColor("shipsvalue", "color", vector3_1);
				if (empirePopulation > historyForPlayer.empire_pop)
					this.App.UI.SetPropertyColor("populationvalue", "color", vector3_2);
				else if (empirePopulation < historyForPlayer.empire_pop)
					this.App.UI.SetPropertyColor("populationvalue", "color", vector3_3);
				else
					this.App.UI.SetPropertyColor("populationvalue", "color", vector3_1);
				if ((double)empireEconomy > (double)historyForPlayer.empire_economy)
					this.App.UI.SetPropertyColor("economyvalue", "color", vector3_2);
				else if ((double)empireEconomy < (double)historyForPlayer.empire_economy)
					this.App.UI.SetPropertyColor("economyvalue", "color", vector3_3);
				else
					this.App.UI.SetPropertyColor("economyvalue", "color", vector3_1);
				if (empireBiosphere > historyForPlayer.empire_biosphere)
					this.App.UI.SetPropertyColor("biospherevalue", "color", vector3_2);
				else if (empireBiosphere < historyForPlayer.empire_biosphere)
					this.App.UI.SetPropertyColor("biospherevalue", "color", vector3_3);
				else
					this.App.UI.SetPropertyColor("biospherevalue", "color", vector3_1);
				if (tradeRevenue > historyForPlayer.empire_trade)
					this.App.UI.SetPropertyColor("tradeunitsvalue", "color", vector3_2);
				else if (tradeRevenue < historyForPlayer.empire_trade)
					this.App.UI.SetPropertyColor("tradeunitsvalue", "color", vector3_3);
				else
					this.App.UI.SetPropertyColor("tradeunitsvalue", "color", vector3_1);
				int? nullable = empireMorale1;
				int empireMorale2 = historyForPlayer.empire_morale;
				if ((nullable.GetValueOrDefault() <= empireMorale2 ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
				{
					this.App.UI.SetPropertyColor("averagemoralevalue", "color", vector3_2);
				}
				else
				{
					nullable = empireMorale1;
					int empireMorale3 = historyForPlayer.empire_morale;
					if ((nullable.GetValueOrDefault() >= empireMorale3 ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
						this.App.UI.SetPropertyColor("averagemoralevalue", "color", vector3_3);
					else
						this.App.UI.SetPropertyColor("averagemoralevalue", "color", vector3_1);
				}
			}
			this._piechart.SetSlices(budget);
		}

		protected override void OnUpdate()
		{
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}

		public override bool IsReady()
		{
			return this._crits != null && this._crits.IsReady() && base.IsReady();
		}

		private void UpdateTechCubeToolTip()
		{
			string str1 = App.Localize("@UI_RESEARCH_RESEARCHING");
			bool flag = true;
			int techId = this.App.GameDatabase.GetPlayerResearchingTechID(this.App.LocalPlayer.ID);
			if (techId == 0)
			{
				techId = this.App.GameDatabase.GetPlayerFeasibilityStudyTechId(this.App.LocalPlayer.ID);
				str1 = App.Localize("@UI_RESEARCH_STUDYING");
				flag = false;
			}
			string techIdStr = this.App.GameDatabase.GetTechFileID(techId);
			PlayerTechInfo playerTechInfo = this.App.GameDatabase.GetPlayerTechInfo(this.App.LocalPlayer.ID, techId);
			Tech tech = this.App.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault<Tech>((Func<Tech, bool>)(x => x.Id == techIdStr));
			if (tech != null && playerTechInfo != null)
			{
				string str2 = "";
				if (flag)
					str2 = " -  " + ResearchScreenState.GetTurnsToCompleteString(this.App, this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID), playerTechInfo);
				this.App.UI.SetTooltip("researchCubeButton", str1 + " " + tech.Name + str2);
			}
			else
				this.App.UI.SetTooltip("researchCubeButton", App.Localize("@UI_TOOLTIP_RESEARCHCUBE"));
		}

		public bool OnKeyBindPressed(HotKeyManager.HotKeyActions action, string gamestates)
		{
			if (gamestates.Contains(this.Name))
			{
				switch (action)
				{
					case HotKeyManager.HotKeyActions.State_Starmap:
						this.App.UI.LockUI();
						this.App.SwitchGameState<StarMapState>();
						return true;
					case HotKeyManager.HotKeyActions.State_BuildScreen:
						return false;
					case HotKeyManager.HotKeyActions.State_DesignScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<DesignScreenState>((object)false, (object)this.Name);
						return true;
					case HotKeyManager.HotKeyActions.State_ResearchScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<ResearchScreenState>();
						return true;
					case HotKeyManager.HotKeyActions.State_ComparativeAnalysysScreen:
						return false;
					case HotKeyManager.HotKeyActions.State_EmpireSummaryScreen:
						return false;
					case HotKeyManager.HotKeyActions.State_SotspediaScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<SotspediaState>();
						return true;
					case HotKeyManager.HotKeyActions.State_DiplomacyScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<DiplomacyScreenState>();
						return true;
				}
			}
			return false;
		}

		public enum GovernmentSpendings
		{
			Security,
			Stimulus,
			Savings,
		}

		private enum SecuritySpendings
		{
			Operations,
			Intel,
			CounterIntel,
		}

		private enum StimulusSpendings
		{
			Mining,
			Colonization,
			Trade,
		}

		public enum ResearchSpendings
		{
			CurrentProject,
			SpecialProject,
			SalvageResearch,
		}
	}
}
