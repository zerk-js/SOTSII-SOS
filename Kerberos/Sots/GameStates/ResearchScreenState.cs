// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.ResearchScreenState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal class ResearchScreenState : GameState, IKeyBindListener
	{
		private static readonly string UICurrentProjectSlider = "currentProjectSliderR";
		private static readonly string UISpecialProjectSlider = "specialProjectSliderR";
		private static readonly string UISalvageResearchSlider = "salvageResearchSliderR";
		private string _selectedTech = string.Empty;
		private Dictionary<string, int> FamilyIndex = new Dictionary<string, int>();
		private Random _rand = new Random();
		private const string UIFeasibilityDetails = "feasibility_details";
		private const string UIFeasibilityTechName = "feasibility_tech_name";
		private const string UIFeasibilityTechIcon = "feasibility_tech_icon";
		private const string UIResearchingDetails = "researching_details";
		private const string UIResearchingTechName = "researching_tech_name";
		private const string UIResearchingTechIcon = "researching_tech_icon";
		private const string UIResearchingTechProgress = "researching_progress";
		private const string UISelectResearchingTechButton = "select_researching_tech";
		private const string UISelectFeasibilityTechButton = "select_feasibility_tech";
		private const string UISelectedTechName = "selected_tech_name";
		private const string UISelectedTechStatus = "selected_tech_status";
		private const string UISelectedTechIcon = "selected_tech_icon";
		private const string UISelectedTechFamilyName = "selected_tech_family_name";
		private const string UISelectedTechFamilyIcon = "discipline_icon";
		private const string UISelectedTechDesc = "selected_tech_desc";
		private const string UICancelResearchButton = "cancel_research_button";
		private const string UICancelFeasibilityButton = "cancel_feasibility_button";
		private const string UIStartResearchButton = "start_research_button";
		private const string UIStartFeasibilityButton = "start_feasibility_button";
		private const string UIStartResearchPanel = "start_research";
		private const string UIStartFeasibilityPanel = "start_feasibility";
		private const string UIResearchSlider = "research_slider";
		private const string UIBoostResearchButton = "boost_research_button";
		private const string UIBoostResearchPanel = "BoostResearchPanel";
		private const string UIBoostValue = "boost_value";
		private const string UIBoostSlider = "boost_slider";
		private const string UIProjectList = "research_projects";
		private const string UIStartProjectButton = "start_project_button";
		private const string UICancelProjectButton = "cancel_project_button";
		private const string UIModuleCountLabel = "moduleCountLabel";
		private const string UIModuleBonusLabel = "moduleBonusLabel";
		private const string UISelectedTechDetailsButton = "selectedTechDetailsButton";
		private const string UISelectedTechInfoButton = "selectedTechInfoButton";
		private const string UIFamilyList = "familyList";
		private GameObjectSet _crits;
		private OrbitCameraController _camera;
		private ResearchState _research;
		private bool _showDebugControls;
		private BudgetPiechart _piechart;
		private WeaponInfoPanel _weaponInfoPanel;
		private bool _inState;
		private bool _selectedTechDetailsVisible;
		private int _leftButton;
		private bool _enteredButton;
		private bool _showBoostDialog;
		private int _selectedProject;
		private bool _budgetChanged;

		private static void AddTechTreeBranches(
		  TechTree tree,
		  Kerberos.Sots.Data.TechnologyFramework.Tech tech,
		  Dictionary<Kerberos.Sots.Data.TechnologyFramework.Tech, Dictionary<Allowable, ResearchScreenState.TechBranch>> acquired,
		  string faction,
		  Random rng,
		  bool forceBranches)
		{
			if (!acquired.ContainsKey(tech))
				acquired.Add(tech, new Dictionary<Allowable, ResearchScreenState.TechBranch>());
			foreach (Allowable allow in tech.Allows)
			{
				if (!acquired[tech].ContainsKey(allow))
				{
					float num = allow.GetFactionProbabilityPercentage(faction) / 100f;
					if ((double)num != 0.0)
					{
						bool flag = forceBranches || rng.CoinToss((double)num);
						acquired[tech][allow] = new ResearchScreenState.TechBranch()
						{
							Feasibility = flag ? num : 0.0f,
							Allows = allow
						};
						ResearchScreenState.AddTechTreeBranches(tree, tree[allow.Id], acquired, faction, rng, forceBranches);
					}
				}
			}
		}

		public static void BuildPlayerTechTree(
		  App game,
		  AssetDatabase assetdb,
		  GameDatabase gamedb,
		  int playerId)
		{
			List<Kerberos.Sots.Data.ScenarioFramework.Tech> AvailableList = new List<Kerberos.Sots.Data.ScenarioFramework.Tech>();
			foreach (Kerberos.Sots.Data.TechnologyFramework.Tech technology in assetdb.MasterTechTree.Technologies)
			{
				Kerberos.Sots.Data.ScenarioFramework.Tech tech = new Kerberos.Sots.Data.ScenarioFramework.Tech();
				tech.Name = technology.Id;
				AvailableList.Add(tech);
			}
			ResearchScreenState.BuildPlayerTechTree(game, assetdb, gamedb, playerId, AvailableList);
		}

		private static void BuildFactionTechTree(
		  AssetDatabase assetdb,
		  GameDatabase gamedb,
		  string faction,
		  out Dictionary<Kerberos.Sots.Data.TechnologyFramework.Tech, Dictionary<Allowable, ResearchScreenState.TechBranch>> acquired)
		{
			Random rng = new Random();
			acquired = new Dictionary<Kerberos.Sots.Data.TechnologyFramework.Tech, Dictionary<Allowable, ResearchScreenState.TechBranch>>();
			foreach (Kerberos.Sots.Data.TechnologyFramework.Tech masterTechTreeRoot in assetdb.MasterTechTreeRoots)
				ResearchScreenState.AddTechTreeBranches(assetdb.MasterTechTree, masterTechTreeRoot, acquired, faction, rng, true);
		}

		public static void BuildPlayerTechTree(
		  App game,
		  AssetDatabase assetdb,
		  GameDatabase gamedb,
		  int playerId,
		  List<Kerberos.Sots.Data.ScenarioFramework.Tech> AvailableList)
		{
			Random rng = new Random();
			string factionName = gamedb.GetFactionName(gamedb.GetPlayerFactionID(playerId));
			TechTree masterTechTree = assetdb.MasterTechTree;
			Dictionary<Kerberos.Sots.Data.TechnologyFramework.Tech, Dictionary<Allowable, ResearchScreenState.TechBranch>> acquired = new Dictionary<Kerberos.Sots.Data.TechnologyFramework.Tech, Dictionary<Allowable, ResearchScreenState.TechBranch>>();
			foreach (Kerberos.Sots.Data.TechnologyFramework.Tech masterTechTreeRoot in assetdb.MasterTechTreeRoots)
				ResearchScreenState.AddTechTreeBranches(masterTechTree, masterTechTreeRoot, acquired, factionName, rng, false);
			foreach (Kerberos.Sots.Data.TechnologyFramework.Tech key in acquired.Keys)
			{
				gamedb.InsertPlayerTech(playerId, key.Id, TechStates.Locked, 0.0, 0.0, new int?());
				foreach (ResearchScreenState.TechBranch techBranch in acquired[key].Values)
					gamedb.InsertPlayerTechBranch(playerId, gamedb.GetTechID(key.Id), gamedb.GetTechID(techBranch.Allows.Id), techBranch.Allows.ResearchPoints, techBranch.Feasibility);
			}
			gamedb.UpdateLockedTechs(assetdb, playerId);
		}

		public static void AcquireAllTechs(GameSession game, int playerId)
		{
			foreach (PlayerTechInfo playerTechInfo in game.GameDatabase.GetPlayerTechInfos(playerId))
			{
				if (playerTechInfo.State != TechStates.Researched)
				{
					game.GameDatabase.UpdatePlayerTechState(playerId, playerTechInfo.TechID, TechStates.Researched);
					App.UpdateStratModifiers(game, playerId, playerTechInfo.TechID);
				}
			}
		}

		public ResearchScreenState(App game)
		  : base(game)
		{
		}

		private static List<string> GetTechTreeModels(AssetDatabase assetdb, Faction faction)
		{
			List<string> stringList = new List<string>();
			stringList.AddRange(assetdb.TechTreeModels);
			if (faction != null)
			{
				stringList.AddRange(faction.TechTreeModels);
			}
			else
			{
				foreach (Faction faction1 in assetdb.Factions)
					stringList.AddRange(faction1.TechTreeModels);
			}
			return stringList;
		}

		private static List<string> GetTechTreeRoots(AssetDatabase assetdb, Faction faction)
		{
			List<string> stringList = new List<string>();
			stringList.AddRange(assetdb.TechTreeRoots);
			if (faction != null)
			{
				stringList.AddRange(faction.TechTreeRoots);
			}
			else
			{
				foreach (Faction faction1 in assetdb.Factions)
					stringList.AddRange(faction1.TechTreeRoots);
			}
			return stringList;
		}

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			if (this.App.GameDatabase == null)
			{
				this.App.NewGame();
				this._showDebugControls = ScriptHost.AllowConsole;
			}
			List<string> techTreeModels = ResearchScreenState.GetTechTreeModels(this.App.AssetDatabase, this.App.LocalPlayer.Faction);
			this._crits = new GameObjectSet(this.App);
			this._camera = this._crits.Add<OrbitCameraController>();
			this._research = this._crits.Add<ResearchState>(((IEnumerable<object>)new object[1]
			{
		(object) techTreeModels.Count
			}).Concat<object>(techTreeModels.Cast<object>()).ToArray<object>());
			this._research.PostSetProp("CameraController", this._camera.ObjectID);
			this._camera.MinDistance = 15f;
			this._camera.DesiredDistance = 80f;
			this._camera.MaxDistance = 600f;
			this._camera.DesiredYaw = MathHelper.DegreesToRadians(45f);
			this._camera.DesiredPitch = -MathHelper.DegreesToRadians(45f);
			this.SyncTechTree(this.App.LocalPlayer.ID);
			this.App.UI.LoadScreen("Research");
		}

		public void ShowDebugControls()
		{
			this._showDebugControls = true;
			if (!this._inState)
				return;
			this.App.UI.SetVisible("debugControls", true);
		}

		protected override void OnEnter()
		{
			this.App.UI.UnlockUI();
			this.App.UI.SetScreen("Research");
			this.App.UI.SetPropertyBool("gameExitButton", "lockout_button", true);
			this.App.UI.SetVisible("selectedTechDetailsButton", false);
			this.App.UI.SetVisible("selectedTechInfoButton", false);
			this.App.UI.SetPropertyBool("familyList", "only_user_events", true);
			this.PopulateFamilyList();
			this._piechart = new BudgetPiechart(this.App.UI, "piechart", this.App.AssetDatabase);
			this.App.UI.Send((object)"SetGameObject", (object)"Research", (object)this._research.ObjectID);
			this.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			if (this._showDebugControls)
				this.App.UI.SetVisible("debugControls", true);
			this._crits.Activate();
			EmpireBarUI.SyncTitleFrame(this.App);
			EmpireBarUI.SyncResearchSlider(this.App, "research_slider", this._piechart);
			this.RefreshResearchButton();
			this.RefreshResearchingTech();
			this.RefreshFeasibilityStudy();
			this.RefreshProjectList();
			this._weaponInfoPanel = new WeaponInfoPanel(this.App.UI, "selectedTechWeaponDetails");
			this.SetSelectedTechDetailsVisible(false);
			this.HideSelectedTechDetails();
			PlayerTechInfo[] array = this.App.GameDatabase.GetPlayerTechInfos(this.App.LocalPlayer.ID).ToArray<PlayerTechInfo>();
			PlayerTechInfo playerTechInfo = (((((IEnumerable<PlayerTechInfo>)array).FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x =>
		  {
			  if ((x.State == TechStates.Researching || x.State == TechStates.PendingFeasibility) && !x.TechFileID.StartsWith("PSI"))
				  return !x.TechFileID.StartsWith("CYB");
			  return false;
		  })) ?? ((IEnumerable<PlayerTechInfo>)array).FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x =>
	{
			  if (x.State == TechStates.HighFeasibility && !x.TechFileID.StartsWith("PSI"))
				  return !x.TechFileID.StartsWith("CYB");
			  return false;
		  }))) ?? ((IEnumerable<PlayerTechInfo>)array).FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x =>
	{
			  if (x.State == TechStates.Core && !x.TechFileID.StartsWith("PSI"))
				  return !x.TechFileID.StartsWith("CYB");
			  return false;
		  }))) ?? ((IEnumerable<PlayerTechInfo>)array).FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x =>
	{
			  if (x.State == TechStates.Researched && !x.TechFileID.StartsWith("PSI"))
				  return !x.TechFileID.StartsWith("CYB");
			  return false;
		  }))) ?? ((IEnumerable<PlayerTechInfo>)array).First<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x =>
	{
			  if (!x.TechFileID.StartsWith("PSI"))
				  return !x.TechFileID.StartsWith("CYB");
			  return false;
		  }));
			if (playerTechInfo != null)
				this._research.PostSetProp("Select", playerTechInfo.TechFileID);
			if (this.App.GameSettings.AudioEnabled)
				this.App.PostEnableAllSounds();
			this._research.PostSetProp("SetTechFloater", "techFloater");
			this._inState = true;
			this.UpdateResearchSliders(this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID), "");
			PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID);
			this.SetBoost(((float)(playerInfo.ResearchBoostFunds / Math.Max(0.0, playerInfo.Savings))).Clamp(0.0f, 1f), false);
			this.App.HotKeyManager.AddListener((IKeyBindListener)this);
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this.App.HotKeyManager.RemoveListener((IKeyBindListener)this);
			this._inState = false;
			this.SetSelectedTechDetailsVisible(false);
			this._piechart = (BudgetPiechart)null;
			this.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._crits.Dispose();
			this._crits = (GameObjectSet)null;
		}

		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			this._piechart.TryGameEvent(eventName, eventParams);
			if (eventName == "TechClicked")
			{
				this.ProcessGameEvent_TechClicked(eventParams);
			}
			else
			{
				if (!(eventName == "TechMouseOver"))
					return;
				this.App.PostRequestGuiSound("universal_mouseover");
			}
		}

		private void ProcessGameEvent_TechClicked(string[] eventParams)
		{
			this.SetSelectedTech(eventParams[0], "TechClicked");
			this.App.PostRequestGuiSound(this.GetSelectedTechFamilySound(eventParams[0].Substring(0, 3)));
		}

		private string GetSelectedTechFamilySound(string familyId)
		{
			return "universal_click";
		}

		private void DoResearchWhooshAnimation()
		{
			this.App.UI.SetVisible("research_mask", false);
			this.App.UI.SetVisible("research_mask", true);
		}

		private void ShowSelectedTechDetails()
		{
			if (string.IsNullOrEmpty(this._selectedTech))
				return;
			this.App.UI.SetVisible("weaponFader", true);
		}

		private void HideSelectedTechDetails()
		{
			this.App.UI.SetVisible("weaponFader", false);
		}

		private void SetSelectedTechDetailsVisible(bool value)
		{
			if (this._selectedTechDetailsVisible == value)
				return;
			this._selectedTechDetailsVisible = value;
			if (this._selectedTechDetailsVisible)
				this.ShowSelectedTechDetails();
			else
				this.HideSelectedTechDetails();
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (this._piechart.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self))
				return;
			if (msgType == "mouse_enter")
			{
				if (!panelName.Contains("techButton"))
					return;
				int id = int.Parse(panelName.Split('|')[1]);
				this.SetSelectedFamily(this.FamilyIndex.FirstOrDefault<KeyValuePair<string, int>>((Func<KeyValuePair<string, int>, bool>)(x => x.Value == id)).Key);
				this._enteredButton = true;
			}
			else if (msgType == "mouse_leave")
				this._leftButton = 3;
			else if (msgType == "list_sel_changed")
			{
				if (!(panelName == "familyList"))
					return;
				int index = int.Parse(msgParams[0]);
				this._research.PostSetProp("SelectFamily", this.FamilyIndex.FirstOrDefault<KeyValuePair<string, int>>((Func<KeyValuePair<string, int>, bool>)(x => x.Value == index)).Key);
			}
			else if (msgType == "button_clicked")
			{
				if (panelName == "gameExitButton")
					this.App.SwitchGameState<StarMapState>();
				else if (panelName == "gameTutorialButton")
					this.App.UI.SetVisible("ResearchScreenTutorial", true);
				else if (panelName == "researchScreenTutImage")
					this.App.UI.SetVisible("ResearchScreenTutorial", false);
				else if (panelName == "selectedTechDetailsButton")
					this.SetSelectedTechDetailsVisible(!this._selectedTechDetailsVisible);
				else if (panelName == "selectedTechInfoButton")
				{
					if (this.SelectedTech != null)
						SotspediaState.NavigateToLink(this.App, "#" + this.SelectedTech);
				}
				else if (panelName == "gameSalvageProjectsButton")
					this.App.UI.CreateDialog((Dialog)new SalvageProjectDialog(this.App, "dialogSpecialProjects"), null);
				else if (panelName == "gameSpecialProjectsButton")
					this.App.UI.CreateDialog((Dialog)new SpecialProjectDialog(this.App, "dialogSpecialProjects"), null);
				else if (panelName == "start_research_button")
					this.StartResearch(this.SelectedTech);
				else if (panelName == "cancel_research_button")
					this.CancelResearch();
				else if (panelName == "boost_research_button")
				{
					this._showBoostDialog = !this._showBoostDialog;
					this.App.UI.SetVisible("BoostResearchPanel", this._showBoostDialog);
				}
				else if (panelName == "cancel_feasibility_button")
					this.CancelFeasibility();
				else if (panelName == "select_researching_tech")
					this.SelectResearchingTech();
				else if (panelName == "select_feasibility_tech")
					this.SelectFeasibilityTech();
				else if (panelName == "start_feasibility_button")
					this.StartFeasibilityStudy(this.SelectedTech);
				else if (panelName == "start_project_button")
					this.StartProject(this.SelectedProject);
				else if (panelName == "cancel_project_button")
					this.CancelProject(this.SelectedProject);
				else if (panelName == "prevTechFamily")
					this._research.NextTechFamily();
				else if (panelName == "nextTechFamily")
					this._research.PrevTechFamily();
				else if (panelName == "game_budget_pie")
				{
					this.App.UI.LockUI();
					this.App.SwitchGameState<EmpireSummaryState>();
				}
				if (!this._showDebugControls)
					return;
				if (panelName == "debugShowNormal")
					this.SyncTechTree(this.App.LocalPlayer.ID);
				else if (panelName == "debugShowAll")
					this.DebugShowAllTechs();
				else if (panelName == "debugShowHuman")
					this.SyncTechTree("human");
				else if (panelName == "debugShowHiver")
					this.SyncTechTree("hiver");
				else if (panelName == "debugShowTarkas")
					this.SyncTechTree("tarkas");
				else if (panelName == "debugShowLiir")
					this.SyncTechTree("liir_zuul");
				else if (panelName == "debugShowZuul")
					this.SyncTechTree("zuul");
				else if (panelName == "debugShowMorrigi")
				{
					this.SyncTechTree("morrigi");
				}
				else
				{
					if (!(panelName == "debugShowLoa"))
						return;
					this.SyncTechTree("loa");
				}
			}
			else if (msgType == "slider_value")
			{
				PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID);
				if (panelName == "research_slider")
				{
					StarMapState.SetEmpireResearchRate(this.App.Game, msgParams[0], this._piechart);
					this._budgetChanged = true;
				}
				else if (panelName == ResearchScreenState.UICurrentProjectSlider)
				{
					EmpireSummaryState.DistibuteResearchSpending(this.App.Game, this.App.GameDatabase, EmpireSummaryState.ResearchSpendings.CurrentProject, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
					this._budgetChanged = true;
					this.UpdateResearchSliders(playerInfo, panelName);
				}
				else if (panelName == ResearchScreenState.UISpecialProjectSlider)
				{
					EmpireSummaryState.DistibuteResearchSpending(this.App.Game, this.App.GameDatabase, EmpireSummaryState.ResearchSpendings.SpecialProject, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
					this._budgetChanged = true;
					this.UpdateResearchSliders(playerInfo, panelName);
				}
				else if (panelName == ResearchScreenState.UISalvageResearchSlider)
				{
					EmpireSummaryState.DistibuteResearchSpending(this.App.Game, this.App.GameDatabase, EmpireSummaryState.ResearchSpendings.SalvageResearch, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
					this._budgetChanged = true;
					this.UpdateResearchSliders(playerInfo, panelName);
				}
				else
				{
					if (!(panelName == "boost_slider"))
						return;
					this.SetBoost((float)int.Parse(msgParams[0]) / 100f, true);
				}
			}
			else
			{
				if (!(msgType == "list_sel_changed") || !(panelName == "research_projects"))
					return;
				int id = 0;
				this.App.UI.ParseListItemId(msgParams[0], out id);
				this.SetSelectedProject(id, "research_projects");
			}
		}

		private void SetBoost(float ratio, bool setboostvalue)
		{
			PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID);
			if (setboostvalue)
			{
				playerInfo.ResearchBoostFunds = Math.Max(0.0, playerInfo.Savings) * (double)ratio;
				this.App.GameDatabase.UpdatePlayerResearchBoost(playerInfo.ID, playerInfo.ResearchBoostFunds);
			}
			else
				this.App.UI.SetSliderValue("boost_slider", (int)(100.0 * (double)ratio));
			this.App.UI.SetText("boost_value", playerInfo.ResearchBoostFunds.ToString("N0"));
			float num = 1f - ratio;
			this.App.UI.SetPropertyColor("boost_value", "color", (float)byte.MaxValue, (float)byte.MaxValue * num, (float)byte.MaxValue * num);
			this.App.UI.SetPropertyColor("boost_title", "color", (float)byte.MaxValue, (float)byte.MaxValue * num, (float)byte.MaxValue * num);
			this.RefreshResearchingTech();
		}

		private void SelectResearchingTech()
		{
			this.SetSelectedTech(this.App.GameDatabase.GetTechFileID(this.App.GameDatabase.GetPlayerResearchingTechID(this.App.LocalPlayer.ID)), string.Empty);
		}

		private void SelectFeasibilityTech()
		{
			FeasibilityStudyInfo studyInfoByPlayer = this.App.GameDatabase.GetFeasibilityStudyInfoByPlayer(this.App.LocalPlayer.ID);
			if (studyInfoByPlayer == null)
				return;
			this.SetSelectedTech(this.App.GameDatabase.GetTechFileID(studyInfoByPlayer.TechID), string.Empty);
		}

		private void RefreshProjectList()
		{
			IEnumerable<ResearchProjectInfo> researchProjectInfos = this.App.GameDatabase.GetResearchProjectInfos(this.App.LocalPlayer.ID);
			FeasibilityStudyInfo studyInfoByPlayer = this.App.GameDatabase.GetFeasibilityStudyInfoByPlayer(this.App.LocalPlayer.ID);
			this.App.UI.ClearItems("research_projects");
			foreach (ResearchProjectInfo researchProjectInfo in researchProjectInfos)
			{
				if (studyInfoByPlayer == null || studyInfoByPlayer.ProjectID != researchProjectInfo.ID)
				{
					string text = researchProjectInfo.Name;
					switch (researchProjectInfo.State)
					{
						case ProjectStates.InProgress:
							text = text + " (" + App.Localize("@UI_RESEARCH_FEASIBILITY_IN_PROGRESS") + ")";
							break;
						case ProjectStates.Paused:
							text = text + " (" + App.Localize("@UI_RESEARCH_FEASIBILITY_PAUSED") + ")";
							break;
					}
					this.App.UI.AddItem("research_projects", string.Empty, researchProjectInfo.ID, text);
				}
			}
			this.App.UI.SetEnabled("start_project_button", false);
			this.App.UI.SetEnabled("cancel_project_button", false);
		}

		public static int? GetTurnsToCompleteResearch(App App, PlayerInfo player, PlayerTechInfo tech)
		{
			if (tech == null)
				return new int?();
			if (tech.State == TechStates.Researched)
				return new int?(0);
			string techFamily = tech.TechFileID.Substring(0, 3);
			int totalModulesCounted;
			float num1 = 1f + App.Game.GetFamilySpecificResearchModifier(player.ID, techFamily, out totalModulesCounted);
			float num2 = (float)App.Game.GetAvailableResearchPoints(player) * (App.GameDatabase.GetNameValue<float>("ResearchEfficiency") / 100f * num1);
			return GameSession.CalculateTurnsToCompleteResearch(tech.ResearchCost, tech.Progress, (int)num2);
		}

		public static string GetTurnsToCompleteString(App App, PlayerInfo player, PlayerTechInfo tech)
		{
			int? completeResearch = ResearchScreenState.GetTurnsToCompleteResearch(App, player, tech);
			if (!completeResearch.HasValue || completeResearch.Value <= 0)
				return string.Empty;
			string str = completeResearch.Value.ToString("N0");
			int? nullable = completeResearch;
			return string.Format((nullable.GetValueOrDefault() != 1 ? 0 : (nullable.HasValue ? 1 : 0)) != 0 ? AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_TECHSTATUS_ONE_TURN") : AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_TECHSTATUS_MANY_TURNS"), (object)str);
		}

		private void RefreshResearchButton()
		{
			PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID);
			PlayerTechInfo tech1 = (PlayerTechInfo)null;
			Kerberos.Sots.Data.TechnologyFramework.Tech tech2 = (Kerberos.Sots.Data.TechnologyFramework.Tech)null;
			int techId = 0;
			if (!string.IsNullOrEmpty(this.SelectedTech))
			{
				techId = this.App.GameDatabase.GetTechID(this.SelectedTech);
				tech1 = this.App.GameDatabase.GetPlayerTechInfo(this.App.LocalPlayer.ID, techId);
				tech2 = this.App.AssetDatabase.MasterTechTree.Technologies.First<Kerberos.Sots.Data.TechnologyFramework.Tech>((Func<Kerberos.Sots.Data.TechnologyFramework.Tech, bool>)(x => x.Id == this.SelectedTech));
			}
			bool flag1 = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			string text1 = string.Empty;
			if (tech2 != null)
				text1 = this.App.AssetDatabase.GetLocalizedTechnologyName(tech2.Id);
			string text2 = string.Empty;
			if (tech1 != null)
			{
				switch (tech1.State)
				{
					case TechStates.Core:
						flag4 = true;
						flag5 = true;
						flag3 = true;
						break;
					case TechStates.Branch:
						flag2 = true;
						flag1 = true;
						text2 = AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_TECHSTATUS_FEASIBILITY_UNKNOWN");
						break;
					case TechStates.LowFeasibility:
						flag4 = true;
						flag5 = true;
						flag3 = true;
						text2 = string.Format(AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_TECHSTATUS_LOW_FEASIBILITY_PERCENT"), (object)(int)((double)tech1.PlayerFeasibility * 100.0));
						break;
					case TechStates.HighFeasibility:
						flag4 = true;
						flag5 = true;
						flag3 = true;
						text2 = string.Format(AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_TECHSTATUS_HIGH_FEASIBILITY_PERCENT"), (object)(int)((double)tech1.PlayerFeasibility * 100.0));
						break;
					case TechStates.PendingFeasibility:
						flag2 = true;
						flag1 = false;
						text2 = AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_TECHSTATUS_FEASIBILITY_PENDING");
						break;
					case TechStates.Researching:
						flag4 = true;
						flag3 = false;
						text2 = AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_RESEARCHING");
						break;
					case TechStates.Researched:
						text2 = AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_RESEARCHED");
						break;
					case TechStates.Locked:
						text2 = AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_TECHSTATUS_LOCKED");
						break;
				}
				if (flag4 && flag5)
				{
					string toCompleteString = ResearchScreenState.GetTurnsToCompleteString(this.App, playerInfo, tech1);
					if (string.IsNullOrEmpty(text2))
						text2 = toCompleteString;
					else if (!string.IsNullOrEmpty(toCompleteString))
						text2 = text2 + ", " + toCompleteString;
				}
			}
			int researchingTechId = this.App.GameDatabase.GetPlayerResearchingTechID(this.App.LocalPlayer.ID);
			int feasibilityStudyTechId = this.App.GameDatabase.GetPlayerFeasibilityStudyTechId(this.App.LocalPlayer.ID);
			if (techId != 0 && (researchingTechId != 0 || feasibilityStudyTechId != 0))
			{
				flag1 = false;
				flag3 = false;
			}
			this.App.UI.SetVisible("start_feasibility", flag2);
			this.App.UI.SetVisible("start_research", flag4);
			this.App.UI.SetEnabled("start_feasibility_button", flag1);
			this.App.UI.SetEnabled("start_research_button", flag3);
			if (flag3)
				text1 += string.Format(" ( {0}% )", (object)(int)Math.Round((double)tech1.PlayerFeasibility * 100.0));
			this.App.UI.SetText("selected_tech_name", text1);
			this.App.UI.SetText("selected_tech_status", text2);
		}

		private void RefreshFeasibilityStudy()
		{
			FeasibilityStudyInfo studyInfoByPlayer = this.App.GameDatabase.GetFeasibilityStudyInfoByPlayer(this.App.LocalPlayer.ID);
			if (studyInfoByPlayer == null)
			{
				this.App.UI.SetVisible("feasibility_details", false);
			}
			else
			{
				int techId = studyInfoByPlayer.TechID;
				string techIdStr = this.App.GameDatabase.GetTechFileID(techId);
				this.App.GameDatabase.GetPlayerTechInfo(this.App.LocalPlayer.ID, techId);
				Kerberos.Sots.Data.TechnologyFramework.Tech tech = this.App.AssetDatabase.MasterTechTree.Technologies.First<Kerberos.Sots.Data.TechnologyFramework.Tech>((Func<Kerberos.Sots.Data.TechnologyFramework.Tech, bool>)(x => x.Id == techIdStr));
				string spriteName = ResearchScreenState.IconTextureToSpriteName(tech.Icon);
				this.App.UI.SetVisible("feasibility_details", true);
				this.App.UI.SetVisible("feasibility_tech_icon", !string.IsNullOrEmpty(spriteName));
				this.App.UI.SetPropertyString("feasibility_tech_name", "text", App.Localize("@UI_RESEARCH_STUDYING") + " " + this.App.AssetDatabase.GetLocalizedTechnologyName(tech.Id));
				this.App.UI.SetPropertyString("feasibility_tech_icon", "sprite", spriteName);
				this.App.UI.AutoSize("feasibility_details");
			}
		}

		private void RefreshResearchingTech()
		{
			int researchingTechId = this.App.GameDatabase.GetPlayerResearchingTechID(this.App.LocalPlayer.ID);
			if (researchingTechId == 0)
			{
				this.App.UI.SetVisible("researching_details", false);
			}
			else
			{
				PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID);
				string techIdStr = this.App.GameDatabase.GetTechFileID(researchingTechId);
				PlayerTechInfo playerTechInfo = this.App.GameDatabase.GetPlayerTechInfo(this.App.LocalPlayer.ID, researchingTechId);
				float num = 1f;
				if (playerTechInfo.ResearchCost > 0)
					num = (float)playerTechInfo.Progress / (float)playerTechInfo.ResearchCost;
				Kerberos.Sots.Data.TechnologyFramework.Tech tech = this.App.AssetDatabase.MasterTechTree.Technologies.First<Kerberos.Sots.Data.TechnologyFramework.Tech>((Func<Kerberos.Sots.Data.TechnologyFramework.Tech, bool>)(x => x.Id == techIdStr));
				string str = string.Format(": {0}% ({1})", (object)(int)Math.Ceiling((double)num * 100.0), (object)ResearchScreenState.GetTurnsToCompleteString(this.App, playerInfo, playerTechInfo));
				string spriteName = ResearchScreenState.IconTextureToSpriteName(tech.Icon);
				this.App.UI.SetVisible("researching_details", true);
				this.App.UI.SetVisible("researching_tech_icon", !string.IsNullOrEmpty(spriteName));
				this.App.UI.SetPropertyString("researching_tech_name", "text", AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_RESEARCHING") + " " + this.App.AssetDatabase.GetLocalizedTechnologyName(tech.Id));
				this.App.UI.SetPropertyString("researching_progress", "text", AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_PROGRESS") + str);
				this.App.UI.SetPropertyString("researching_tech_icon", "sprite", spriteName);
				this.App.UI.SetSliderValue("researchProgress", (int)Math.Ceiling((double)num * 100.0));
				this.App.UI.AutoSize("researching_details");
			}
		}

		private static string IconTextureToSpriteName(string texture)
		{
			return Path.GetFileNameWithoutExtension(texture);
		}

		private void SyncTech(Kerberos.Sots.Data.TechnologyFramework.Tech tech, TechStates state, PlayerTechInfo techInf = null)
		{
			TechFamily techFamily = this.App.AssetDatabase.MasterTechTree.TechFamilies.First<TechFamily>((Func<TechFamily, bool>)(x => x.Id == tech.Family));
			string str1 = string.Empty;
			if (!string.IsNullOrEmpty(tech.Icon))
				str1 = tech.GetProperIconPath();
			string str2 = string.Empty;
			if (!string.IsNullOrEmpty(techFamily.Icon))
				str2 = techFamily.GetProperIconPath();
			bool flag = ((IEnumerable<Kerberos.Sots.Data.TechnologyFramework.Tech>)this.App.AssetDatabase.MasterTechTreeRoots).Contains<Kerberos.Sots.Data.TechnologyFramework.Tech>(tech);
			int num1 = state == TechStates.Researched ? 1 : 0;
			int num2 = 1;
			if (state != TechStates.Researched && techInf != null)
			{
				num1 = techInf.Progress;
				num2 = techInf.ResearchCost;
			}
			this._research.PostSetProp("Tech", (object)tech.Id, (object)flag, (object)str1, (object)str2, (object)state, (object)num1, (object)num2, (object)tech.Family, (object)ResearchScreenState.GetTechTreeRoots(this.App.AssetDatabase, this.App.LocalPlayer.Faction).Contains(tech.Id));
		}

		private void SyncBranch(string fromTechId, string toTechId, TechStates state)
		{
			this._research.PostSetProp("Branch", (object)fromTechId, (object)toTechId, (object)state);
		}

		private void DebugShowAllTechs()
		{
			foreach (Kerberos.Sots.Data.TechnologyFramework.Tech technology in this.App.AssetDatabase.MasterTechTree.Technologies)
			{
				this.SyncTech(technology, TechStates.Researched, (PlayerTechInfo)null);
				foreach (Allowable allow in technology.Allows)
					this.SyncBranch(technology.Id, allow.Id, TechStates.Researched);
			}
			this._research.RebindModels();
		}

		private void SyncTechTree(int playerId)
		{
			this._research.Clear();
			foreach (PlayerTechInfo playerTechInfo in this.App.GameDatabase.GetPlayerTechInfos(this.App.LocalPlayer.ID))
			{
				if (playerTechInfo.State != TechStates.Locked)
					this.SyncTechState(playerTechInfo.TechID);
			}
			this._research.RebindModels();
		}

		private void SyncTechTree(string faction)
		{
			this._research.Clear();
			Dictionary<Kerberos.Sots.Data.TechnologyFramework.Tech, Dictionary<Allowable, ResearchScreenState.TechBranch>> acquired;
			ResearchScreenState.BuildFactionTechTree(this.App.AssetDatabase, this.App.GameDatabase, faction, out acquired);
			foreach (Kerberos.Sots.Data.TechnologyFramework.Tech key in acquired.Keys)
			{
				this.SyncTech(key, TechStates.Researched, (PlayerTechInfo)null);
				foreach (ResearchScreenState.TechBranch techBranch in acquired[key].Values)
					this.SyncBranch(key.Id, techBranch.Allows.Id, TechStates.Researched);
			}
			this._research.RebindModels();
		}

		private void SyncTechState(int techId)
		{
			PlayerTechInfo playerTechInfo = this.App.GameDatabase.GetPlayerTechInfo(this.App.LocalPlayer.ID, techId);
			string techFileId = playerTechInfo.TechFileID;
			this.SyncTech(this.App.AssetDatabase.MasterTechTree.Technologies.First<Kerberos.Sots.Data.TechnologyFramework.Tech>((Func<Kerberos.Sots.Data.TechnologyFramework.Tech, bool>)(x => x.Id == techFileId)), playerTechInfo.State, playerTechInfo);
			foreach (PlayerBranchInfo playerBranchInfo in this.App.GameDatabase.GetUnlockedBranchesToTech(this.App.LocalPlayer.ID, playerTechInfo.TechID))
				this.SyncBranch(this.App.GameDatabase.GetTechFileID(playerBranchInfo.FromTechID), techFileId, playerTechInfo.State);
		}

		private void SetTechState(string techIdStr, TechStates techState)
		{
			int techId = this.App.GameDatabase.GetTechID(techIdStr);
			if (techId == 0 || this.App.GameDatabase.GetPlayerTechInfo(this.App.LocalPlayer.ID, techId).State == TechStates.Locked)
				return;
			this.App.GameDatabase.UpdatePlayerTechState(this.App.LocalPlayer.ID, techId, techState);
			this.SyncTechState(techId);
		}

		private void CancelFeasibility()
		{
			FeasibilityStudyInfo studyInfoByPlayer = this.App.GameDatabase.GetFeasibilityStudyInfoByPlayer(this.App.LocalPlayer.ID);
			if (studyInfoByPlayer != null)
				this.CancelProject(studyInfoByPlayer.ProjectID);
			this.App.PostRequestSpeech(string.Format("STRAT_031-01_{0}_CancelFeasibilityStudy", (object)this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID))), 50, 120, 0.0f);
		}

		private void CancelResearchCore()
		{
			int researchingTechId = this.App.GameDatabase.GetPlayerResearchingTechID(this.App.LocalPlayer.ID);
			if (researchingTechId == 0)
				return;
			string techFileId = this.App.GameDatabase.GetTechFileID(researchingTechId);
			PlayerTechInfo playerTechInfo = this.App.GameDatabase.GetPlayerTechInfo(this.App.LocalPlayer.ID, researchingTechId);
			if ((double)playerTechInfo.Feasibility >= 1.0)
				this.SetTechState(techFileId, TechStates.Core);
			else if ((double)playerTechInfo.PlayerFeasibility < 0.300000011920929)
				this.SetTechState(techFileId, TechStates.LowFeasibility);
			else
				this.SetTechState(techFileId, TechStates.HighFeasibility);
		}

		public static int StartFeasibilityStudy(GameDatabase db, int playerId, int techId)
		{
			if (db.GetFeasibilityStudyInfo(playerId, techId) != null)
				return 0;
			PlayerTechInfo playerTechInfo = db.GetPlayerTechInfo(playerId, techId);
			if (playerTechInfo.State != TechStates.Branch)
				return 0;
			string projectName = string.Format(App.Localize("@UI_RESEARCH_FEASABILITY_STUDY"), (object)db.AssetDatabase.GetLocalizedTechnologyName(playerTechInfo.TechFileID));
			int num = db.InsertFeasibilityStudy(playerId, techId, projectName);
			db.UpdatePlayerTechState(playerId, techId, TechStates.PendingFeasibility);
			return num;
		}

		public static int StartFeasibilityStudy(GameDatabase db, int playerId, string techIdStr)
		{
			int techId = db.GetTechID(techIdStr);
			return ResearchScreenState.StartFeasibilityStudy(db, playerId, techId);
		}

		private void StartFeasibilityStudy(string techIdStr)
		{
			if (this.isPlayerBusyResearching(this.App.Game.LocalPlayer.ID))
				return;
			int projectId = ResearchScreenState.StartFeasibilityStudy(this.App.GameDatabase, this.App.LocalPlayer.ID, techIdStr);
			this.SyncTechState(this.App.GameDatabase.GetTechID(techIdStr));
			this.RefreshResearchButton();
			this.RefreshFeasibilityStudy();
			this.RefreshProjectList();
			this.SetSelectedProject(projectId, string.Empty);
			this.App.PostRequestSpeech(string.Format("STRAT_028-01_{0}_StartFeasibilityStudy", (object)this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID))), 50, 120, 0.0f);
			this.DoResearchWhooshAnimation();
		}

		private int SelectedProject
		{
			get
			{
				return this._selectedProject;
			}
		}

		private void SetSelectedProject(int projectId, string trigger)
		{
			ResearchProjectInfo researchProjectInfo = (ResearchProjectInfo)null;
			if (projectId != 0)
				researchProjectInfo = this.App.GameDatabase.GetResearchProjectInfo(this.App.LocalPlayer.ID, projectId);
			this._selectedProject = projectId;
			if (trigger != "research_projects")
				this.App.UI.SetSelection("research_projects", projectId);
			this.App.UI.SetEnabled("start_project_button", researchProjectInfo != null && researchProjectInfo.State == ProjectStates.Available);
			this.App.UI.SetEnabled("cancel_project_button", researchProjectInfo != null && researchProjectInfo.State == ProjectStates.InProgress);
			if (projectId == 0)
				return;
			FeasibilityStudyInfo feasibilityStudyInfo = this.App.GameDatabase.GetFeasibilityStudyInfo(projectId);
			if (feasibilityStudyInfo == null)
				return;
			this.SetSelectedTech(this.App.GameDatabase.GetTechFileID(feasibilityStudyInfo.TechID), string.Empty);
		}

		public bool isPlayerBusyResearching(int playerid)
		{
			return this.App.GameDatabase.GetPlayerResearchingTechID(playerid) != 0 || this.App.GameDatabase.GetPlayerFeasibilityStudyTechId(playerid) != 0;
		}

		private void StartProject(int projectId)
		{
			if (projectId == 0 || this.isPlayerBusyResearching(this.App.Game.LocalPlayer.ID))
				return;
			this.App.GameDatabase.UpdateResearchProjectState(projectId, ProjectStates.InProgress);
			this.DoResearchWhooshAnimation();
		}

		public static void CancelResearchProject(App game, int playerId, int projectId)
		{
			FeasibilityStudyInfo feasibilityStudyInfo = game.GameDatabase.GetFeasibilityStudyInfo(projectId);
			if (feasibilityStudyInfo != null)
			{
				game.GameDatabase.RemoveFeasibilityStudy(projectId);
				game.GameDatabase.UpdatePlayerTechState(playerId, feasibilityStudyInfo.TechID, TechStates.Branch);
			}
			else
			{
				ResearchProjectInfo researchProjectInfo = game.GameDatabase.GetResearchProjectInfo(playerId, projectId);
				ProjectStates state = ProjectStates.Available;
				if ((double)researchProjectInfo.Progress > 0.0)
					state = ProjectStates.Paused;
				game.GameDatabase.UpdateResearchProjectState(projectId, state);
			}
		}

		private void CancelProject(int projectId)
		{
			if (projectId == 0)
				return;
			FeasibilityStudyInfo feasibilityStudyInfo = this.App.GameDatabase.GetFeasibilityStudyInfo(projectId);
			ResearchScreenState.CancelResearchProject(this.App, this.App.LocalPlayer.ID, projectId);
			if (feasibilityStudyInfo != null)
			{
				this.SyncTechState(feasibilityStudyInfo.TechID);
				this.RefreshResearchButton();
				this.RefreshFeasibilityStudy();
			}
			this.RefreshProjectList();
			this.SetSelectedProject(0, string.Empty);
		}

		private void StartResearch(string techIdStr)
		{
			int techId = this.App.GameDatabase.GetTechID(techIdStr);
			if (techId == 0 || this.App.GameDatabase.GetPlayerTechInfo(this.App.LocalPlayer.ID, techId).State == TechStates.Branch)
				return;
			this.CancelResearchCore();
			this.SetTechState(techIdStr, TechStates.Researching);
			this.RefreshResearchButton();
			this.RefreshResearchingTech();
			this.App.PostRequestSpeech(string.Format("STRAT_029-01_{0}_StartResearch", (object)this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID))), 50, 120, 0.0f);
			this.DoResearchWhooshAnimation();
		}

		private void CancelResearch()
		{
			this.CancelResearchCore();
			this.RefreshResearchButton();
			this.RefreshResearchingTech();
			this.App.PostRequestSpeech(string.Format("STRAT_030-01_{0}_CancelResearch", (object)this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID))), 50, 120, 0.0f);
		}

		private string SelectedTech
		{
			get
			{
				return this._selectedTech;
			}
		}

		private void PopulateFamilyList()
		{
			this.App.UI.ClearItems("familyList");
			this.FamilyIndex.Clear();
			int userItemId = 0;
			foreach (TechFamily techFamily in this.App.AssetDatabase.MasterTechTree.TechFamilies)
			{
				TechFamily family = techFamily;
				if (!family.FactionDefined || this.App.LocalPlayer.Faction.TechTreeRoots.Any<string>((Func<string, bool>)(x => x.StartsWith(family.Id))))
				{
					this.App.UI.AddItem("familyList", "", userItemId, "");
					string itemGlobalId = this.App.UI.GetItemGlobalID("familyList", "", userItemId, "");
					this.FamilyIndex[family.Id] = userItemId;
					this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "header_idle.idle"), "sprite", ResearchScreenState.IconTextureToSpriteName(family.Icon));
					this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "header_idle.mouse_over"), "sprite", ResearchScreenState.IconTextureToSpriteName(family.Icon));
					this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "header_idle.pressed"), "sprite", ResearchScreenState.IconTextureToSpriteName(family.Icon));
					this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "header_idle.disabled"), "sprite", ResearchScreenState.IconTextureToSpriteName(family.Icon));
					this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "header_idle"), "id", "techButton|" + userItemId.ToString());
					this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "header_sel.idle"), "sprite", ResearchScreenState.IconTextureToSpriteName(family.Icon));
					this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "header_sel.mouse_over"), "sprite", ResearchScreenState.IconTextureToSpriteName(family.Icon));
					this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "header_sel.pressed"), "sprite", ResearchScreenState.IconTextureToSpriteName(family.Icon));
					this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "header_sel.disabled"), "sprite", ResearchScreenState.IconTextureToSpriteName(family.Icon));
					this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "header_sel"), "id", "techButton|" + userItemId.ToString());
					++userItemId;
				}
			}
		}

		private void UpdateSelectedTech()
		{
			int techId = this.App.GameDatabase.GetTechID(this._selectedTech);
			this.App.GameDatabase.GetPlayerTechInfo(this.App.LocalPlayer.ID, techId);
			Kerberos.Sots.Data.TechnologyFramework.Tech tech = this.App.AssetDatabase.MasterTechTree.Technologies.First<Kerberos.Sots.Data.TechnologyFramework.Tech>((Func<Kerberos.Sots.Data.TechnologyFramework.Tech, bool>)(x => x.Id == this._selectedTech));
			ResearchScreenState.IconTextureToSpriteName(tech.Icon);
			TechFamily techFamily = this.App.AssetDatabase.MasterTechTree.TechFamilies.First<TechFamily>((Func<TechFamily, bool>)(x => x.Id == tech.Family));
			ResearchScreenState.IconTextureToSpriteName(techFamily.Icon);
			this.App.UI.SetSelection("familyList", this.FamilyIndex[techFamily.Id]);
			PlayerTechInfo playerTechInfo = this.App.GameDatabase.GetPlayerTechInfo(this.App.LocalPlayer.ID, techId);
			if (playerTechInfo == null)
				return;
			if (ResearchScreenState.GetTurnsToCompleteResearch(this.App, this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID), playerTechInfo).HasValue)
				this.App.UI.SetPropertyString("selected_tech_time", "text", ResearchScreenState.GetTurnsToCompleteString(this.App, this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID), playerTechInfo));
			else
				this.App.UI.SetPropertyString("selected_tech_time", "text", "∞ Turns");
		}

		private void SetSelectedTech(string techIdStr, string trigger)
		{
			this.SetSelectedTechDetailsVisible(false);
			this._selectedTech = techIdStr;
			int techId = this.App.GameDatabase.GetTechID(techIdStr);
			this.App.GameDatabase.GetPlayerTechInfo(this.App.LocalPlayer.ID, techId);
			Kerberos.Sots.Data.TechnologyFramework.Tech tech = this.App.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault<Kerberos.Sots.Data.TechnologyFramework.Tech>((Func<Kerberos.Sots.Data.TechnologyFramework.Tech, bool>)(x => x.Id == techIdStr));
			if (tech == null)
				return;
			string spriteName1 = ResearchScreenState.IconTextureToSpriteName(tech.Icon);
			TechFamily family = this.App.AssetDatabase.MasterTechTree.TechFamilies.First<TechFamily>((Func<TechFamily, bool>)(x => x.Id == tech.Family));
			string spriteName2 = ResearchScreenState.IconTextureToSpriteName(family.Icon);
			this.App.UI.SetSelection("familyList", this.FamilyIndex[family.Id]);
			if (trigger != "TechClicked")
				this._research.PostSetProp("Select", techIdStr);
			this.App.UI.SetVisible("selected_tech_icon", !string.IsNullOrEmpty(spriteName1));
			this.App.UI.SetVisible("discipline_icon", !string.IsNullOrEmpty(spriteName2));
			if (family == null)
			{
				this.App.UI.SetVisible("moduleCountLabel", false);
				this.App.UI.SetVisible("moduleBonusLabel", false);
			}
			else
			{
				int modulesInstalled = 0;
				int percentBonusToResearch = this.GetPercentBonusToResearch(family, out modulesInstalled);
				if (modulesInstalled == 1)
					this.App.UI.SetText("moduleCountLabel", "1 " + AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_MODULE_INSTALLED"));
				else
					this.App.UI.SetText("moduleCountLabel", modulesInstalled.ToString() + " " + AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_MODULES_INSTALLED"));
				this.App.UI.SetText("moduleBonusLabel", percentBonusToResearch.ToString() + "% " + AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_BONUS_TO_RESEARCH_PROJECTS"));
			}
			this.App.UI.SetPropertyString("selected_tech_icon", "sprite", spriteName1);
			this.App.UI.SetPropertyString("selected_tech_family_name", "text", AssetDatabase.CommonStrings.Localize("@TECH_FAMILY_" + family.Id));
			this.App.UI.SetPropertyString("discipline_icon", "sprite", spriteName2);
			this.App.UI.SetPropertyString("selected_tech_desc", "text", App.Localize("@TECH_DESC_" + tech.Id));
			PlayerTechInfo playerTechInfo = this.App.GameDatabase.GetPlayerTechInfo(this.App.LocalPlayer.ID, techId);
			if (playerTechInfo != null)
			{
				if (ResearchScreenState.GetTurnsToCompleteResearch(this.App, this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID), playerTechInfo).HasValue)
				{
					this.App.UI.SetPropertyString("selected_tech_time", "text", ResearchScreenState.GetTurnsToCompleteString(this.App, this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID), playerTechInfo));
					this.App.UI.SetPropertyString("selected_tech_time_right", "text", ResearchScreenState.GetTurnsToCompleteString(this.App, this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID), playerTechInfo));
				}
				else
				{
					this.App.UI.SetPropertyString("selected_tech_time", "text", "∞ Turns");
					this.App.UI.SetPropertyString("selected_tech_time_right", "text", "∞ Turns");
				}
			}
			if (tech != null)
			{
				LogicalWeapon weaponUnlockedByTech = this.GetWeaponUnlockedByTech(tech);
				if (weaponUnlockedByTech != null)
				{
					this.App.UI.SetVisible("selectedTechDetailsButton", true);
					this._weaponInfoPanel.SetWeapons(weaponUnlockedByTech, (LogicalWeapon)null);
				}
				else
					this.App.UI.SetVisible("selectedTechDetailsButton", false);
				if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
					this.App.UI.SetVisible("selectedTechInfoButton", ScriptHost.AllowConsole);
			}
			else
			{
				this.App.UI.SetVisible("selectedTechDetailsButton", false);
				this.App.UI.SetVisible("selectedTechInfoButton", false);
			}
			this.RefreshResearchButton();
		}

		private void SetSelectedFamily(string familyId)
		{
			TechFamily family = this.App.AssetDatabase.MasterTechTree.TechFamilies.First<TechFamily>((Func<TechFamily, bool>)(x => x.Id == familyId));
			string spriteName = ResearchScreenState.IconTextureToSpriteName(family.Icon);
			this.App.UI.SetVisible("discipline_icon", !string.IsNullOrEmpty(spriteName));
			if (family == null)
			{
				this.App.UI.SetVisible("moduleCountLabel", false);
				this.App.UI.SetVisible("moduleBonusLabel", false);
			}
			else
			{
				int modulesInstalled = 0;
				int percentBonusToResearch = this.GetPercentBonusToResearch(family, out modulesInstalled);
				if (modulesInstalled == 1)
					this.App.UI.SetText("moduleCountLabel", "1 " + AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_MODULE_INSTALLED"));
				else
					this.App.UI.SetText("moduleCountLabel", modulesInstalled.ToString() + " " + AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_MODULES_INSTALLED"));
				this.App.UI.SetText("moduleBonusLabel", percentBonusToResearch.ToString() + "% " + AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_BONUS_TO_RESEARCH_PROJECTS"));
			}
			this.App.UI.SetPropertyString("selected_tech_family_name", "text", AssetDatabase.CommonStrings.Localize("@TECH_FAMILY_" + family.Id));
			this.App.UI.SetPropertyString("discipline_icon", "sprite", spriteName);
			this.App.UI.SetSelection("familyList", this.FamilyIndex[familyId]);
		}

		private LogicalWeapon GetWeaponUnlockedByTech(Kerberos.Sots.Data.TechnologyFramework.Tech tech)
		{
			return this.App.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => ((IEnumerable<Kerberos.Sots.Data.WeaponFramework.Tech>)x.RequiredTechs).Any<Kerberos.Sots.Data.WeaponFramework.Tech>((Func<Kerberos.Sots.Data.WeaponFramework.Tech, bool>)(y => y.Name == tech.Id))));
		}

		private ShipSectionAsset GetShipSectionUnlockedByTech(Kerberos.Sots.Data.TechnologyFramework.Tech tech)
		{
			return this.App.AssetDatabase.ShipSections.FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
		   {
			   if (x.Faction == this.App.LocalPlayer.Faction.Name)
				   return ((IEnumerable<string>)x.RequiredTechs).Any<string>((Func<string, bool>)(y => y == tech.Id));
			   return false;
		   }));
		}

		private ShipClass? GetShipClassUnlockedByTech(Kerberos.Sots.Data.TechnologyFramework.Tech tech)
		{
			if (tech.Id == "ENG_Leviathian_Construction")
				return new ShipClass?(ShipClass.Leviathan);
			if (tech.Id == "ENG_Dreadnought_Construction")
				return new ShipClass?(ShipClass.Dreadnought);
			if (tech.Id == "ENG_Cruiser_Construction")
				return new ShipClass?(ShipClass.Cruiser);
			if (tech.Id == "BRD_BattleRiders")
				return new ShipClass?(ShipClass.BattleRider);
			return new ShipClass?();
		}

		private int GetPercentBonusToResearch(TechFamily family, out int modulesInstalled)
		{
			modulesInstalled = 0;
			return (int)Math.Round((double)(this.App.Game.GetFamilySpecificResearchModifier(this.App.LocalPlayer.ID, family.Id, out modulesInstalled) + (this.App.Game.GetGeneralResearchModifier(this.App.LocalPlayer.ID, true) - 1f)) * 100.0);
		}

		protected override void OnUpdate()
		{
			if (this._budgetChanged)
			{
				this._budgetChanged = false;
				this.RefreshResearchButton();
				this.RefreshResearchingTech();
				this.UpdateSelectedTech();
			}
			if (this._leftButton > 0 && this._enteredButton)
				this._leftButton = 0;
			if (this._leftButton == 1 && !this._enteredButton)
			{
				this.App.GameDatabase.GetTechID(this.SelectedTech);
				Kerberos.Sots.Data.TechnologyFramework.Tech tech = this.App.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault<Kerberos.Sots.Data.TechnologyFramework.Tech>((Func<Kerberos.Sots.Data.TechnologyFramework.Tech, bool>)(x => x.Id == this.SelectedTech));
				if (tech != null)
					this.SetSelectedFamily(tech.Family);
			}
			this._enteredButton = false;
			if (this._leftButton <= 0)
				return;
			--this._leftButton;
		}

		public override bool IsReady()
		{
			if (this._crits.IsReady())
				return base.IsReady();
			return false;
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}

		private void UpdateResearchSliders(PlayerInfo playerInfo, string iChanged)
		{
			double num1 = (1.0 - (double)playerInfo.RateGovernmentResearch) * 100.0;
			double num2 = (double)playerInfo.RateResearchCurrentProject * 100.0;
			double num3 = (double)playerInfo.RateResearchSpecialProject * 100.0;
			double num4 = (double)playerInfo.RateResearchSalvageResearch * 100.0;
			if (iChanged != "research_slider")
				this.App.UI.SetSliderValue("research_slider", (int)num1);
			if (iChanged != ResearchScreenState.UICurrentProjectSlider)
				this.App.UI.SetSliderValue(ResearchScreenState.UICurrentProjectSlider, (int)num2);
			if (iChanged != ResearchScreenState.UISpecialProjectSlider)
				this.App.UI.SetSliderValue(ResearchScreenState.UISpecialProjectSlider, (int)num3);
			if (!(iChanged != ResearchScreenState.UISalvageResearchSlider))
				return;
			this.App.UI.SetSliderValue(ResearchScreenState.UISalvageResearchSlider, (int)num4);
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
						return false;
					case HotKeyManager.HotKeyActions.State_ComparativeAnalysysScreen:
						return false;
					case HotKeyManager.HotKeyActions.State_EmpireSummaryScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<EmpireSummaryState>();
						return true;
					case HotKeyManager.HotKeyActions.State_SotspediaScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<SotspediaState>();
						return true;
					case HotKeyManager.HotKeyActions.State_DiplomacyScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<DiplomacyScreenState>();
						return true;
					case HotKeyManager.HotKeyActions.Research_NextTree:
						this._research.PrevTechFamily();
						return true;
					case HotKeyManager.HotKeyActions.Research_LastTree:
						this._research.NextTechFamily();
						return true;
				}
			}
			return false;
		}

		private class TechBranch
		{
			public float Feasibility = 1f;
			public Allowable Allows;
		}
	}
}
