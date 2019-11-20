// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.GameSetupState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.ScenarioFramework;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal class GameSetupState : GameState
	{
		private const string UIBackButton = "gameBackButton";
		private const string UIPlayerSetupButton = "gamePlayerSetupButton";
		private const string UIStarMapList = "gameStarMapList";
		private const string UIScenarioList = "gameScenarioList";
		private const string UIMapsCheckbox = "gameMapsCheckbox";
		private const string UIScenariosCheckbox = "gameScenariosCheckbox";
		private Dictionary<int, Starmap.StarmapInfo> _starmapIdMap;
		private Dictionary<int, Scenario.ScenarioInfo> _scenarioIdMap;
		private StarMapPreview _starmapPreview;
		private TimeSlider _strategicTurnLengthSlider;
		private TimeSlider _combatTurnLengthSlider;
		private PercentageSlider _economicEfficiencySlider;
		private PercentageSlider _researchEfficiencySlider;
		private ValueBoundSlider _planetResourcesSlider;
		private ValueBoundSlider _planetSizeSlider;
		private TreasurySlider _initialTreasurySlider;
		private ValueBoundSpinner _numPlayersSpinner;
		private ValueBoundSpinner _initialSystemsSpinner;
		private ValueBoundSpinner _initialTechnologiesSpinner;
		private PercentageSlider _randomEncounterFrequencySlider;
		private ValueBoundSlider _grandMenaceSlider;
		private bool _creatingGame;
		private int _maxPlayers;
		private GameSetup _tempGameSetup;
		private string _victoryConditionDialog;
		private int _modeSliderVal;
		private GameMode _gameMode;

		public GameSetupState(App game)
		  : base(game)
		{
		}

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this._creatingGame = false;
			if (((IEnumerable<object>)parms).Count<object>() > 0)
				this._creatingGame = (bool)parms[0];
			if (this._tempGameSetup == null)
				this._tempGameSetup = new GameSetup(this.App);
			this.App.UI.LoadScreen("GameSetup");
			this.RecreateStarmapPreview();
		}

		protected override void OnEnter()
		{
			this.App.PostPlayMusic("Ambient_GameSetup");
			this.App.UI.SetScreen("GameSetup");
			this.App.UI.SetEnabled("gamePlayerSetupButton", false);
			this.RecreateStarmapPreview();
			if (this._creatingGame)
			{
				this.App.UI.SetText("gamePlayerSetupButton", App.Localize("@UI_GAMESETUPSTATE_CONFIRM"));
				this.App.UI.SetVisible("gameBackButton", true);
			}
			else
			{
				this.App.UI.SetText("gamePlayerSetupButton", "Done");
				this.App.UI.SetVisible("gameBackButton", false);
			}
			this.CreateUI();
			this.SetGameMode(GameMode.LastSideStanding);
		}

		private void CreateUI()
		{
			this._strategicTurnLengthSlider = new TimeSlider(this.App.UI, "gameStrategyTurnLength", this.App.GameSetup.StrategicTurnLength, 0.25f, 15f, 0.25f, true);
			this._combatTurnLengthSlider = new TimeSlider(this.App.UI, "gameCombatTurnLength", this.App.GameSetup.CombatTurnLength, 3f, 12f, 1f, false);
			this._economicEfficiencySlider = new PercentageSlider(this.App.UI, "gameEconomicEfficiency", this.App.GameSetup.EconomicEfficiency, 50, 200);
			this._researchEfficiencySlider = new PercentageSlider(this.App.UI, "gameResearchEfficiency", this.App.GameSetup.ResearchEfficiency, 50, 200);
			this._planetResourcesSlider = (ValueBoundSlider)new PercentageSlider(this.App.UI, "gameStarMapPlanetResSlider", this.App.GameSetup.PlanetResources, 50, 150);
			this._planetSizeSlider = (ValueBoundSlider)new PercentageSlider(this.App.UI, "gameStarMapPlanetSizeSlider", this.App.GameSetup.PlanetSize, 50, 150);
			this._initialTreasurySlider = new TreasurySlider(this.App.UI, "gameInitialTreasury", this.App.GameSetup.InitialTreasury, 0, 1000000);
			this._numPlayersSpinner = new ValueBoundSpinner(this.App.UI, "gameNumPlayers", 2.0, 8.0, (double)this.App.GameSetup.Players.Count, 1.0);
			this._initialSystemsSpinner = new ValueBoundSpinner(this.App.UI, "gameInitialSystems", 3.0, 9.0, (double)this.App.GameSetup.InitialSystems, 1.0);
			this._initialTechnologiesSpinner = new ValueBoundSpinner(this.App.UI, "gameInitialTechs", 0.0, 10.0, (double)this.App.GameSetup.InitialTechnologies, 1.0);
			this._randomEncounterFrequencySlider = new PercentageSlider(this.App.UI, "gameRandomEncounterFrequency", this.App.GameSetup.RandomEncounterFrequency, 0, 200);
			this._grandMenaceSlider = new ValueBoundSlider(this.App.UI, "gameGrandMenaces", 0, 5, this.App.GameSetup.GrandMenaceCount);
			foreach (Faction faction in this.App.AssetDatabase.Factions)
			{
				if (faction.IsPlayable)
					this.App.UI.SetChecked(GameSetupState.UIAvailableFactionCheckBox(faction), this.App.GameSetup.AvailablePlayerFeatures.Factions.ContainsKey(faction));
			}
			string[] strArray = new string[1] { "loa" };
			foreach (string str in strArray)
			{
				string expansionFactionName = str;
				if (!this.App.AssetDatabase.Factions.Any<Faction>((Func<Faction, bool>)(x => x.Name == expansionFactionName)))
				{
					this.App.UI.SetChecked(GameSetupState.UIAvailableFactionCheckBox(expansionFactionName), false);
					this.App.UI.SetVisible(GameSetupState.UIAvailableFactionFrame(expansionFactionName), false);
				}
			}
			this.App.UI.SetVisible("gameScenarioList", false);
			this.App.UI.SetVisible("gameStarMapList", true);
			this.App.UI.SetChecked("gameScenariosCheckbox", false);
			this.App.UI.SetChecked("gameMapsCheckbox", true);
			this.PopulateScenarioList();
			this.PopulateStarMapList();
			if (this._starmapIdMap.Count <= 0)
				return;
			if (ScriptHost.AllowConsole && this._starmapIdMap.Any<KeyValuePair<int, Starmap.StarmapInfo>>((Func<KeyValuePair<int, Starmap.StarmapInfo>, bool>)(x => x.Value.Title == "@STARMAP_TITLE_FIGHT")))
				this.App.UI.SetSelection("gameStarMapList", this._starmapIdMap.FirstOrDefault<KeyValuePair<int, Starmap.StarmapInfo>>((Func<KeyValuePair<int, Starmap.StarmapInfo>, bool>)(x => x.Value.Title == "@STARMAP_TITLE_FIGHT")).Key);
			else
				this.App.UI.SetSelection("gameStarMapList", this._starmapIdMap.Keys.First<int>());
		}

		private void PopulateStarMapList()
		{
			this.App.UI.ClearItems("gameStarMapList");
			this._starmapIdMap = new Dictionary<int, Starmap.StarmapInfo>();
			string[] array = ((IEnumerable<string>)ScriptHost.FileSystem.FindFiles("starmaps\\*.starmap")).OrderBy<string, string>((Func<string, string>)(y => Path.GetFileNameWithoutExtension(y))).ToArray<string>();
			for (int userItemId = 0; userItemId < array.Length; ++userItemId)
			{
				this._starmapIdMap[userItemId] = new Starmap.StarmapInfo(array[userItemId]);
				this.App.UI.AddItem("gameStarMapList", string.Empty, userItemId, string.Format("{0} [{1}]", (object)App.Localize(this._starmapIdMap[userItemId].GetFallbackTitle()), (object)this._starmapIdMap[userItemId].NumPlayers));
			}
		}

		private void PopulateScenarioList()
		{
			this.App.UI.ClearItems("gameScenarioList");
			this._scenarioIdMap = new Dictionary<int, Scenario.ScenarioInfo>();
			string[] array = ((IEnumerable<string>)ScriptHost.FileSystem.FindFiles("scenarios\\*.scenario")).OrderBy<string, string>((Func<string, string>)(y => Path.GetFileNameWithoutExtension(y))).ToArray<string>();
			for (int userItemId = 0; userItemId < array.Length; ++userItemId)
			{
				this._scenarioIdMap[userItemId] = new Scenario.ScenarioInfo(array[userItemId]);
				this.App.UI.AddItem("gameScenarioList", string.Empty, userItemId, string.Format("{0} [?]", (object)App.Localize(this._scenarioIdMap[userItemId].GetFallbackTitle())));
			}
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this.App.UI.DeleteScreen("GameSetup");
			if (this._starmapPreview != null)
			{
				this._starmapPreview.Dispose();
				this._starmapPreview = (StarMapPreview)null;
			}
			this._strategicTurnLengthSlider = (TimeSlider)null;
			this._combatTurnLengthSlider = (TimeSlider)null;
			this._economicEfficiencySlider = (PercentageSlider)null;
			this._researchEfficiencySlider = (PercentageSlider)null;
			this._planetResourcesSlider = (ValueBoundSlider)null;
			this._planetSizeSlider = (ValueBoundSlider)null;
			this._initialTreasurySlider = (TreasurySlider)null;
			this._numPlayersSpinner = (ValueBoundSpinner)null;
			this._initialSystemsSpinner = (ValueBoundSpinner)null;
			this._initialTechnologiesSpinner = (ValueBoundSpinner)null;
			this._randomEncounterFrequencySlider = (PercentageSlider)null;
			this._grandMenaceSlider = (ValueBoundSlider)null;
			this._starmapIdMap = (Dictionary<int, Starmap.StarmapInfo>)null;
			this._scenarioIdMap = (Dictionary<int, Scenario.ScenarioInfo>)null;
		}

		private static string UIAvailableFactionFrame(string factionName)
		{
			return string.Format("gameFaction_{0}_frame", (object)factionName);
		}

		private static string UIAvailableFactionCheckBox(string factionName)
		{
			return string.Format("gameFaction_{0}", (object)factionName);
		}

		private static string UIAvailableFactionCheckBox(Faction faction)
		{
			return GameSetupState.UIAvailableFactionCheckBox(faction.Name);
		}

		private void UpdateData()
		{
			this.App.GameSetup.StrategicTurnLength = this._strategicTurnLengthSlider.TimeInMinutes;
			this.App.GameSetup.CombatTurnLength = this._combatTurnLengthSlider.TimeInMinutes;
			this.App.GameSetup.EconomicEfficiency = this._economicEfficiencySlider.Value;
			this.App.GameSetup.ResearchEfficiency = this._researchEfficiencySlider.Value;
			this.App.GameSetup.PlanetResources = this._planetResourcesSlider.Value;
			this.App.GameSetup.PlanetSize = this._planetSizeSlider.Value;
			this.App.GameSetup.InitialTreasury = this._initialTreasurySlider.Value;
			this.App.GameSetup.InitialSystems = (int)this._initialSystemsSpinner.Value;
			this.App.GameSetup.InitialTechnologies = (int)this._initialTechnologiesSpinner.Value;
			this.App.GameSetup.RandomEncounterFrequency = this._randomEncounterFrequencySlider.Value;
			this.App.GameSetup.GrandMenaceCount = this._grandMenaceSlider.Value;
			this.App.GameSetup.SetPlayerCount((int)this._numPlayersSpinner.Value);
		}

		private void SelectStarMapOrScenario(int? nullableId, bool isMapFile)
		{
			if (!nullableId.HasValue)
			{
				this._tempGameSetup.StarMapFile = string.Empty;
				this._tempGameSetup.ScenarioFile = string.Empty;
				if (this._starmapPreview == null)
					return;
				this._starmapPreview.Dispose();
				this._starmapPreview = (StarMapPreview)null;
			}
			else
			{
				int index = nullableId.Value;
				string fallbackTitle;
				string strId;
				if (isMapFile)
				{
					fallbackTitle = this._starmapIdMap[index].GetFallbackTitle();
					strId = this._starmapIdMap[index].Description;
				}
				else
				{
					fallbackTitle = this._scenarioIdMap[index].GetFallbackTitle();
					strId = string.Empty;
				}
				this.App.UI.SetPropertyString("mapLabel", "text", App.Localize(fallbackTitle));
				this.App.UI.SetText("mapsummary_Content", App.Localize(strId));
				if (isMapFile)
				{
					foreach (Faction faction in this.App.AssetDatabase.Factions)
					{
						this.App.GameSetup.AvailablePlayerFeatures.TryAddFaction(faction);
						if (faction.IsPlayable)
						{
							this.App.UI.SetChecked(GameSetupState.UIAvailableFactionCheckBox(faction), true);
							this.App.UI.SetEnabled(GameSetupState.UIAvailableFactionCheckBox(faction), true);
						}
					}
					this._numPlayersSpinner.SetEnabled(true);
					this._initialSystemsSpinner.SetEnabled(true);
					this._initialTechnologiesSpinner.SetEnabled(true);
					this._initialTreasurySlider.SetEnabled(true);
					this.App.UI.SetEnabled("victoryToggle", true);
					Starmap.StarmapInfo starmapId = this._starmapIdMap[index];
					this._tempGameSetup.StarMapFile = starmapId.FileName;
					this._tempGameSetup.ScenarioFile = string.Empty;
					this._numPlayersSpinner.SetValue((double)starmapId.NumPlayers);
					this._maxPlayers = starmapId.NumPlayers;
				}
				else
				{
					Scenario.ScenarioInfo scenarioId = this._scenarioIdMap[index];
					this._tempGameSetup.ScenarioFile = scenarioId.FileName;
					this._tempGameSetup.StarMapFile = scenarioId.StarmapInfo.FileName;
					this._numPlayersSpinner.SetValue((double)scenarioId.StarmapInfo.NumPlayers);
					Scenario s = new Scenario();
					ScenarioXmlUtility.LoadScenarioFromXml(scenarioId.FileName, ref s);
					foreach (Faction faction in this.App.AssetDatabase.Factions)
					{
						this.App.GameSetup.AvailablePlayerFeatures.TryRemoveFaction(faction);
						if (faction.IsPlayable)
						{
							this.App.UI.SetChecked(GameSetupState.UIAvailableFactionCheckBox(faction), false);
							this.App.UI.SetEnabled(GameSetupState.UIAvailableFactionCheckBox(faction), false);
						}
					}
					foreach (Kerberos.Sots.Data.ScenarioFramework.Player playerStartCondition in s.PlayerStartConditions)
					{
						Faction faction = this.App.AssetDatabase.GetFaction(playerStartCondition.Faction);
						this.App.GameSetup.AvailablePlayerFeatures.TryAddFaction(faction);
						if (faction.IsPlayable)
							this.App.UI.SetChecked(GameSetupState.UIAvailableFactionCheckBox(faction), true);
					}
					this._numPlayersSpinner.SetEnabled(false);
					this._initialSystemsSpinner.SetEnabled(false);
					this._initialTechnologiesSpinner.SetEnabled(false);
					this._initialTreasurySlider.SetEnabled(false);
					this.App.UI.SetEnabled("victoryToggle", false);
				}
				this.RecreateStarmapPreview();
			}
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (this._numPlayersSpinner.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self | PanelBinding.PanelMessageTargetFlags.Recursive))
			{
				this._numPlayersSpinner.SetValue(Math.Max(0.0, Math.Min((double)this._maxPlayers, this._numPlayersSpinner.Value)));
			}
			else
			{
				if (this._strategicTurnLengthSlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self) || this._combatTurnLengthSlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self) || (this._economicEfficiencySlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self) || this._researchEfficiencySlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self)) || (this._planetResourcesSlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self) || this._planetSizeSlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self) || (this._initialTreasurySlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self) || this._initialSystemsSpinner.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self | PanelBinding.PanelMessageTargetFlags.Recursive))) || (this._initialTechnologiesSpinner.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self | PanelBinding.PanelMessageTargetFlags.Recursive) || this._randomEncounterFrequencySlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self) || this._grandMenaceSlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self)))
					return;
				if (msgType == "slider_value")
				{
					if (!(panelName == "gameModeSlider"))
						return;
					this._modeSliderVal = int.Parse(msgParams[0]);
					this.SetGameMode(this._gameMode);
				}
				else if (msgType == "button_clicked")
				{
					if (panelName == "gameBackButton")
						this.GoBack();
					else if (panelName == "gamePlayerSetupButton")
					{
						this.App.GameSetup._mode = this._tempGameSetup._mode;
						this.App.GameSetup._modeValue = this._tempGameSetup._modeValue;
						this.App.GameSetup.StarMapFile = this._tempGameSetup.StarMapFile;
						this.App.GameSetup.ScenarioFile = this._tempGameSetup.ScenarioFile;
						if (this.App.GameSetup.HasScenarioFile())
						{
							Scenario s = new Scenario();
							ScenarioXmlUtility.LoadScenarioFromXml(this.App.GameSetup.ScenarioFile, ref s);
							int index = 0;
							foreach (Kerberos.Sots.Data.ScenarioFramework.Player playerStartCondition in s.PlayerStartConditions)
							{
								PlayerSetup player = this.App.GameSetup.Players[index];
								player.EmpireName = playerStartCondition.Name;
								player.Avatar = playerStartCondition.Avatar;
								player.Badge = playerStartCondition.Badge;
								player.ShipColor = playerStartCondition.ShipColor;
								player.Faction = playerStartCondition.Faction;
								player.AI = playerStartCondition.isAI;
								player.Fixed = true;
								player.InitialColonies = playerStartCondition.Colonies.Count;
								player.InitialTechs = playerStartCondition.StartingTechs.Count;
								player.InitialTreasury = (int)playerStartCondition.Treasury;
								++index;
							}
						}
						this.UpdateData();
						if (this.App.GameSetup.IsMultiplayer)
							this.App.SwitchGameState(this.App.PreviousState, (object)LobbyEntranceState.Multiplayer);
						else
							this.App.SwitchGameState<StarMapLobbyState>((object)LobbyEntranceState.SinglePlayer);
					}
					else if (panelName == "victoryToggle")
						this._victoryConditionDialog = this.App.UI.CreateDialog((Dialog)new VictoryConditionDialog(this.App, "dialogVictoryCondition"), null);
					else if (panelName == "gameMapsCheckbox")
					{
						this.App.UI.SetChecked("gameMapsCheckbox", true);
						this.App.UI.SetChecked("gameScenariosCheckbox", false);
						this.App.UI.SetVisible("gameScenarioList", false);
						this.App.UI.SetVisible("gameStarMapList", true);
						this._tempGameSetup.ScenarioFile = string.Empty;
						this._tempGameSetup.StarMapFile = string.Empty;
						if (this._starmapIdMap.Count > 0)
						{
							if (ScriptHost.AllowConsole && this._starmapIdMap.Any<KeyValuePair<int, Starmap.StarmapInfo>>((Func<KeyValuePair<int, Starmap.StarmapInfo>, bool>)(x => x.Value.FileName == "FIGHT.starmap")))
								this.App.UI.SetSelection("gameStarMapList", this._starmapIdMap.FirstOrDefault<KeyValuePair<int, Starmap.StarmapInfo>>((Func<KeyValuePair<int, Starmap.StarmapInfo>, bool>)(x => x.Value.FileName == "FIGHT.starmap")).Key);
							else
								this.App.UI.SetSelection("gameStarMapList", this._starmapIdMap.Keys.First<int>());
						}
						else
							this.App.UI.ClearSelection("gameStarMapList");
					}
					else if (!(panelName == "gameScenariosCheckbox"))
						;
				}
				else if (msgType == "checkbox_clicked")
				{
					foreach (Faction faction in this.App.AssetDatabase.Factions)
					{
						if (panelName == GameSetupState.UIAvailableFactionCheckBox(faction))
						{
							if (int.Parse(msgParams[0]) != 0)
								this.App.GameSetup.AvailablePlayerFeatures.TryAddFaction(faction);
							else if (this.App.GameSetup.AvailablePlayerFeatures.Factions.Keys.Count<Faction>() >= 2)
								this.App.GameSetup.AvailablePlayerFeatures.TryRemoveFaction(faction);
							else
								this.App.UI.SetChecked(GameSetupState.UIAvailableFactionCheckBox(faction), true);
						}
					}
				}
				else if (msgType == "list_sel_changed")
				{
					if (!(panelName == "gameStarMapList") && !(panelName == "gameScenarioList"))
						return;
					bool isMapFile = panelName == "gameStarMapList";
					if (msgParams.Length == 1)
						this.SelectStarMapOrScenario(new int?(int.Parse(msgParams[0])), isMapFile);
					else
						this.SelectStarMapOrScenario(new int?(), false);
					this.App.UI.SetEnabled("gamePlayerSetupButton", this._tempGameSetup.HasStarMapFile() || this._tempGameSetup.HasScenarioFile());
				}
				else
				{
					if (!(msgType == "dialog_closed") || !(panelName == this._victoryConditionDialog))
						return;
					GameMode mode = (GameMode)int.Parse(msgParams[0]);
					if (mode == GameMode.LandGrab)
					{
						this.App.UI.SetPropertyInt("gameModeSlider", "value", 67);
						this._modeSliderVal = 67;
					}
					else
					{
						this.App.UI.SetPropertyInt("gameModeSlider", "value", 100);
						this._modeSliderVal = 100;
					}
					this.SetGameMode(mode);
				}
			}
		}

		private int GetModeSliderValue(GameMode mode)
		{
			switch (mode)
			{
				case GameMode.LastSideStanding:
					return -1;
				case GameMode.LastCapitalStanding:
					return -1;
				case GameMode.StarChamberLimit:
					return (int)((double)this._modeSliderVal / 100.0 * 4.0) + 1;
				case GameMode.GemWorldLimit:
					return (int)((double)this._modeSliderVal / 100.0 * 4.0) + 1;
				case GameMode.ProvinceLimit:
					return (int)((double)this._modeSliderVal / 100.0 * 4.0) + 1;
				case GameMode.LeviathanLimit:
					return (int)((double)this._modeSliderVal / 100.0 * 9.0) + 1;
				case GameMode.LandGrab:
					return (int)((double)this._modeSliderVal / 100.0 * 60.0) + 20;
				default:
					return -1;
			}
		}

		private void SetGameMode(GameMode mode)
		{
			this._gameMode = mode;
			int modeSliderValue = this.GetModeSliderValue(this._gameMode);
			this._tempGameSetup._mode = this._gameMode;
			this._tempGameSetup._modeValue = modeSliderValue;
			if (modeSliderValue < 0)
				this.App.UI.SetVisible("gameModeSlider", false);
			else
				this.App.UI.SetVisible("gameModeSlider", true);
			switch (mode)
			{
				case GameMode.LastSideStanding:
					this.App.UI.SetPropertyString("victoryLabel", "text", App.Localize("@UI_GAMESETUP_LASTSIDESTANDING"));
					break;
				case GameMode.LastCapitalStanding:
					this.App.UI.SetPropertyString("victoryLabel", "text", App.Localize("@UI_GAMESETUP_LASTCAPITALSTANDING"));
					break;
				case GameMode.StarChamberLimit:
					this.App.UI.SetPropertyString("victoryLabel", "text", string.Format(App.Localize("@UI_GAMESETUP_XSTARCHAMBERS"), (object)modeSliderValue));
					break;
				case GameMode.GemWorldLimit:
					this.App.UI.SetPropertyString("victoryLabel", "text", string.Format(App.Localize("@UI_GAMESETUP_XGEMWORLDS"), (object)modeSliderValue));
					break;
				case GameMode.ProvinceLimit:
					this.App.UI.SetPropertyString("victoryLabel", "text", string.Format(App.Localize("@UI_GAMESETUP_XPROVINCES"), (object)modeSliderValue));
					break;
				case GameMode.LeviathanLimit:
					this.App.UI.SetPropertyString("victoryLabel", "text", string.Format(App.Localize("@UI_GAMESETUP_XLEVIATHANS"), (object)modeSliderValue));
					break;
				case GameMode.LandGrab:
					this.App.UI.SetPropertyString("victoryLabel", "text", string.Format(App.Localize("@UI_GAMESETUP_XLANDGRAB"), (object)modeSliderValue));
					break;
			}
		}

		private void RecreateStarmapPreview()
		{
			if (this._starmapPreview != null)
			{
				this._starmapPreview.Dispose();
				this.App.UI.Send((object)"SetGameObject", (object)"starmapPreviewImage", (object)0);
			}
			if (string.IsNullOrEmpty(this._tempGameSetup.StarMapFile))
				return;
			this._starmapPreview = new StarMapPreview(this.App, this._tempGameSetup);
			this.App.UI.Send((object)"SetGameObject", (object)"starmapPreviewImage", (object)this._starmapPreview.StarMap.ObjectID);
		}

		private void GoBack()
		{
			if (this.App.GameSetup.IsMultiplayer)
				this.App.SwitchGameState(this.App.PreviousState, (object)LobbyEntranceState.Browser);
			else
				this.App.SwitchGameState((GameState)this.App.GetGameState<MainMenuState>());
		}

		protected override void OnUpdate()
		{
			if (this._numPlayersSpinner.Value > (double)this._maxPlayers)
				this._numPlayersSpinner.SetValue((double)this._maxPlayers);
			if (this._starmapPreview == null)
				return;
			this._starmapPreview.Update();
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
	}
}
