// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.StarMapLobbyState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Steam;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal class StarMapLobbyState : GameState
	{
		private static int _selectedIndex = -1;
		private static string _lastIPTyped = "";
		private List<ServerInfo> _servers = new List<ServerInfo>();
		private Random _rand = new Random();
		private string _loginGUID = "";
		private string _empireGUID = "";
		private string _createGameGUID = "";
		private string _directConnectGUID = "";
		private string _passwordGUID = "";
		private Dictionary<int, bool> _settingsDirty = new Dictionary<int, bool>();
		private const int PLAYER_UPDATE_INTERVAL = 60;
		private const string DebugTestCombatButton = "debugTestCombatButton";
		private const string UIEmpireBar = "gameEmpireBar";
		public const string UIProvinceSelectDetailsPanel = "gameProvinceSelectDetails";
		private const string UIColonyDetailsWidget = "colonyDetailsWidget";
		private const string UIExitButton = "gameExitButton";
		private const string UIOptionsButton = "gameOptionsButton";
		private const string UISaveGameButton = "gameSaveGameButton";
		private const string UIEndTurnButton = "gameEndTurnButton";
		private const string UIEmpireSummaryButton = "gameEmpireSummaryButton";
		private const string UIResearchButton = "gameResearchButton";
		private const string UIDiplomacyButton = "gameDiplomacyButton";
		private const string UIDesignButton = "gameDesignButton";
		private const string UIBuildButton = "gameBuildButton";
		private const string UISystemButton = "gameSystemButton";
		private const string UISotspediaButton = "gameSotspediaButton";
		private const string UIProvinceModeButton = "gameProvinceModeButton";
		public const string UIPlayerList = "lstPlayers";
		private const string UISurveyButton = "gameSurveyButton";
		private const string UIColonizeButton = "gameColonizeButton";
		private const string UIRelocateButton = "gameRelocateButton";
		private const string UIPatrolButton = "gamePatrolButton";
		private const string UIInterdictButton = "gameInterdictButton";
		private const string UIStrikeButton = "gameStrikeButton";
		private const string UIInvadeButton = "gameInvadeButton";
		private const string UIConstructStationButton = "gameConstructStationButton";
		private const string UIStationManagerButton = "gameStationManagerButton";
		private const string UIFleetManagerButton = "gameFleetManagerButton";
		private const string UIDefenseManagerButton = "gameDefenseManagerButton";
		private LobbyEntranceState _enterState;
		private GameObjectSet _crits;
		private ArrowPainter _painter;
		private Sky _sky;
		private StarMapLobby _starmap;
		private List<Vector3> _serverPositions;
		private OrbitCameraController _camera;
		private int _selectedPlanet;
		private GameState _previousState;
		private string _contextMenuID;
		private int _frameCount;
		private int _numPlayerSlots;
		private int _selectedSlot;
		private int _contextSlot;
		private TreasurySlider _playerInitialTreasurySlider;
		private ValueBoundSpinner _playerInitialTechnologiesSpinner;
		private ValueBoundSpinner _playerInitialSystemsSpinner;
		private ShipHoloView _shipHoloView;
		private ShipBuilder _builder;
		private OrbitCameraController _shipCamera;
		private GameObjectSet _shipCrits;
		private Player _tempPlayer;
		private PlayerInfo _tempPlayerInfo;
		private bool _dlgSelectEmpireColorVisible;
		private bool _dlgSelectShipColorVisible;
		private bool _inPlayerSetup;
		private bool _betaDisable;
		private bool _refreshing;

		public int SelectedServer
		{
			get
			{
				return StarMapLobbyState._selectedIndex;
			}
			set
			{
				StarMapLobbyState._selectedIndex = value;
			}
		}

		private int SelectedPlanet
		{
			get
			{
				return this._selectedPlanet;
			}
			set
			{
				this._selectedPlanet = value;
			}
		}

		public int SelectedSlot
		{
			get
			{
				return this._selectedSlot;
			}
			private set
			{
				if (this._selectedSlot == value)
					return;
				this.SetSelectedSlot(value);
			}
		}

		private int NumPlayerSlots
		{
			get
			{
				return this._numPlayerSlots;
			}
			set
			{
				if (value == this._numPlayerSlots)
					return;
				this._numPlayerSlots = value;
				this.App.GameSetup.SetPlayerCount(this._numPlayerSlots);
			}
		}

		private string SelectedAvatar { get; set; }

		private string SelectedBadge { get; set; }

		private string SelectedFaction { get; set; }

		private int SelectedSubfactionIndex { get; set; }

		public bool InPlayerSetup
		{
			get
			{
				return this._inPlayerSetup;
			}
			set
			{
				this._inPlayerSetup = value;
			}
		}

		private void SelectBadge(string badgeItemId)
		{
			if (!string.IsNullOrEmpty(this.SelectedFaction))
			{
				this.SelectedBadge = this.App.GameSetup.SetBadge(this.SelectedSlot, this.SelectedFaction, badgeItemId);
				if (!string.IsNullOrEmpty(this.SelectedBadge))
					this._tempPlayerInfo.BadgeAssetPath = Path.Combine("factions", this.SelectedFaction, "badges", this.SelectedBadge + ".tga");
				this.UpdateShipPreview(StarMapLobbyState._selectedIndex);
			}
			else
				this._tempPlayerInfo.BadgeAssetPath = string.Empty;
		}

		private void SelectAvatar(string avatarItemId)
		{
			if (!string.IsNullOrEmpty(this.SelectedFaction))
			{
				this.SelectedAvatar = this.App.GameSetup.SetAvatar(this.SelectedSlot, this.SelectedFaction, avatarItemId);
				if (string.IsNullOrEmpty(this.SelectedAvatar))
					return;
				this._tempPlayerInfo.AvatarAssetPath = Path.Combine("factions", this.SelectedFaction, "avatars", this.SelectedAvatar + ".tga");
			}
			else
				this._tempPlayerInfo.AvatarAssetPath = string.Empty;
		}

		private void SelectFaction(string factionItemId, int subfactionIndex)
		{
			if (this.App.GameSetup.Players[this.SelectedSlot].Faction == factionItemId && this.App.GameSetup.Players[this.SelectedSlot].SubfactionIndex == subfactionIndex)
				return;
			this.SelectedFaction = factionItemId;
			this.SelectedSubfactionIndex = subfactionIndex;
			this.App.GameSetup.SetBadge(this.SelectedSlot, this.App.GameSetup.Players[this.SelectedSlot].Faction, null);
			this.App.GameSetup.SetAvatar(this.SelectedSlot, this.App.GameSetup.Players[this.SelectedSlot].Faction, null);
			this.App.GameSetup.Players[this.SelectedSlot].Faction = this.SelectedFaction;
			this.App.GameSetup.Players[this.SelectedSlot].SubfactionIndex = this.SelectedSubfactionIndex;
		}

		private void SelectDifficulty(string difficultyItemId)
		{
			this.App.GameSetup.SetDifficulty(this.SelectedSlot, difficultyItemId);
		}

		private void SetEmpireColor(int slot, int? empireColorId)
		{
			this.App.GameSetup.SetEmpireColor(slot, empireColorId);
			if (!empireColorId.HasValue)
				return;
			this._tempPlayerInfo.PrimaryColor = Player.DefaultPrimaryPlayerColors[empireColorId.Value];
			if (this._tempPlayer != null)
				this._tempPlayer.SetEmpireColor(empireColorId.Value);
			this.App.UI.SetPropertyColor("imgEmpireColor", "color", this._tempPlayerInfo.PrimaryColor * (float)byte.MaxValue);
		}

		private void SetShipColor(int slot, Vector3 shipColor, bool setColorSample = true)
		{
			if (shipColor != this.App.GameSetup.Players[slot].ShipColor)
				this._settingsDirty[slot] = true;
			this.App.GameSetup.SetShipColor(slot, shipColor);
			this._tempPlayerInfo.SecondaryColor = shipColor;
			if (this._tempPlayer != null)
				this._tempPlayer.SetPlayerColor(this._tempPlayerInfo.SecondaryColor);
			if (!setColorSample)
				return;
			this.App.UI.SetPropertyColor("pickerSecondaryColor", "color", this._tempPlayerInfo.SecondaryColor * (float)byte.MaxValue);
		}

		private void SetSelectedSlot(int value)
		{
			this._selectedSlot = value;
			PlayerSetup player = this.App.GameSetup.Players[this._selectedSlot];
			this.EnablePlayerSetup(!player.Fixed);
			this._playerInitialSystemsSpinner.SetValue((double)player.InitialColonies);
			this._playerInitialTechnologiesSpinner.SetValue((double)player.InitialTechs);
			this._playerInitialTreasurySlider.SetValue(player.InitialTreasury);
			this.SelectedFaction = this.App.GameSetup.Players[value].Faction;
			this.SelectedSubfactionIndex = this.App.GameSetup.Players[value].SubfactionIndex;
			this.SelectedAvatar = this.App.GameSetup.Players[value].Avatar;
			this.SelectedBadge = this.App.GameSetup.Players[value].Badge;
			bool flag = !this.App.GameSetup.IsMultiplayer || this.App.Network.IsHosting;
			this._playerInitialSystemsSpinner.SetEnabled(flag && !player.Fixed);
			this._playerInitialTechnologiesSpinner.SetEnabled(flag && !player.Fixed);
			this._playerInitialTreasurySlider.SetEnabled(flag && !player.Fixed);
		}

		protected void UpdateShipColors(int index)
		{
			if (this.App.GameSetup.Players.Count <= index || index < 0)
				return;
			this.SetEmpireColor(index, this.App.GameSetup.Players[index].EmpireColor);
			this.SetShipColor(index, this.App.GameSetup.Players[index].ShipColor, true);
		}

		protected void OnPlayerChanged(int slot, bool updateShip = true, bool rebuildPlayerList = true)
		{
			GameSetupUI.SyncPlayerListWidget(this.App, "lstPlayers", this.App.GameSetup.Players, rebuildPlayerList);
			GameSetupUI.SyncPlayerSetupWidget(this.App, "pnlPlayerSetup", this.App.GameSetup.Players[slot]);
			if (updateShip)
				this.UpdateShipPreview(slot);
			if (!this.App.GameSetup.IsMultiplayer)
				return;
			this.App.Network.SetPlayerInfo(this.App.GameSetup.Players[slot], slot);
		}

		protected void DefaultPlayer(int iPlayer, bool isScenario)
		{
			PlayerSetup player = this.App.GameSetup.Players[iPlayer];
			if (!isScenario)
			{
				player.InitialColonies = this.App.GameSetup.InitialSystems;
				player.InitialTechs = this.App.GameSetup.InitialTechnologies;
				player.InitialTreasury = this.App.GameSetup.InitialTreasury;
			}
			Faction faction = this.App.AssetDatabase.GetFaction(player.Faction);
			if (faction == null || !this.App.GameSetup.AvailablePlayerFeatures.Factions.ContainsKey(faction))
			{
				List<string> list = this.App.GameSetup.AvailablePlayerFeatures.Factions.Keys.Select<Faction, string>((Func<Faction, string>)(x => x.Name)).ToList<string>();
				Vector3 shipColor;
				shipColor.X = App.GetSafeRandom().NextSingle();
				shipColor.Y = App.GetSafeRandom().NextSingle();
				shipColor.Z = App.GetSafeRandom().NextSingle();
				this.SetShipColor(iPlayer, shipColor, true);
				player.Faction = list[App.GetSafeRandom().Next(list.Count)];
				player.Badge = null;
				player.Avatar = null;
				this.SetEmpireColor(iPlayer, new int?());
			}
			else
			{
				this.App.GameSetup.SetEmpireColor(iPlayer, player.EmpireColor);
				this.App.GameSetup.SetAvatar(iPlayer, player.Faction, player.Avatar);
			}
			this.UpdateShipColors(iPlayer);
		}

		public StarMapLobbyState(App game)
		  : base(game)
		{
		}

		private List<Vector3> GenerateMeASpiralGalaxyPlease()
		{
			List<float> floatList = new List<float>();
			List<Vector3> source = new List<Vector3>();
			int num1 = 12;
			float num2 = 0.0f;
			float num3 = 360f / (float)num1;
			for (int index = 0; index < num1; ++index)
			{
				floatList.Add(num2 + (float)this._rand.Next(0, (int)num3));
				num2 += num3;
			}
			float num4 = 0.5f;
			float num5 = 0.05f;
			float num6 = 2f;
			float num7 = 3.5f;
			float num8 = 300f;
			float num9 = 0.0f;
			float num10 = 0.0f;
			int num11 = (int)Math.Floor(((double)num8 - (double)num6) / (double)num7);
			for (int index1 = 0; index1 < num11; ++index1)
			{
				for (int index2 = 0; index2 < num1; ++index2)
				{
					float num12 = floatList[index2];
					float num13 = (float)this._rand.Next(0, (int)num3);
					float num14 = num12 + num13 / 2f;
					Vector3 vector3 = new Vector3(num9 + (float)Math.Cos((double)num14 * 0.0174444448202848) * num6, 0.0f, num10 + (float)-Math.Sin((double)num14 * 0.0174444448202848) * num6);
					float num15 = 5f;
					bool flag = false;
					for (int index3 = 0; index3 < source.Count<Vector3>(); ++index3)
					{
						if ((double)(vector3 - source[index3]).Length < (double)num15)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
						source.Add(vector3);
					float num16 = (float)((int)((double)num12 + (double)num4) % 360);
					if (this._rand.Next(0, 100) == 0)
					{
						float num17 = this._rand.NextSingle() * 5f;
						num16 = (float)((int)((double)num16 + (double)num17) % 360);
					}
					floatList[index2] = num16;
				}
				num4 += num5;
				num6 += num7;
			}
			return source;
		}

		public void AddServer(
		  ulong id,
		  string name,
		  string map,
		  string version,
		  int players,
		  int maxPlayers,
		  int ping,
		  bool passworded,
		  List<PlayerSetup> playerInfo)
		{
			ServerInfo si = this._servers.FirstOrDefault<ServerInfo>((Func<ServerInfo, bool>)(x => (long)x.serverID == (long)id));
			if (si == null)
			{
				this._servers.Add(new ServerInfo());
				si = this._servers.Last<ServerInfo>();
			}
			int index = this._servers.Count - 1;
			si.name = name;
			si.map = map;
			si.version = version;
			si.players = players;
			si.maxPlayers = maxPlayers;
			si.ping = ping;
			si.serverID = id;
			si.ID = index;
			si.passworded = passworded;
			si.playerInfo = playerInfo;
			App.Log.Trace("Adding server, " + si.name + ", to lobby @ pos: " + (object)id, "net");
			Vector3 vector3 = this._serverPositions[index] * 15f;
			vector3.Y = this._rand.NextSingle() * 3f;
			si.Origin = vector3;
			this._starmap.AddServer(this._crits, si);
			if (this._servers.Count < 2)
				this.FocusOnServer(index);
			if (this.SelectedServer != index || playerInfo.Count<PlayerSetup>() <= 0 || (this.App.Network.IsJoined || this.App.Network.IsHosting))
				return;
			GameSetupUI.SyncPlayerListWidget(this.App, "lstPlayers", playerInfo, true);
		}

		protected void FocusOnServer(int index)
		{
			StarMapServer starMapServer = (StarMapServer)null;
			if (!this._starmap.Servers.Reverse.TryGetValue(index, out starMapServer))
				return;
			this._starmap.SetFocus((IGameObject)starMapServer, 100f);
		}

		protected void SetGameDetails(int serverId)
		{
			ServerInfo serverInfo = this._servers.FirstOrDefault<ServerInfo>((Func<ServerInfo, bool>)(x => x.ID == serverId));
			if (serverInfo == null)
				return;
			this.App.UI.ClearItems("lstGameSettings");
			this.App.UI.AddItem("lstGameSettings", "property", 0, "Map:");
			this.App.UI.SetItemText("lstGameSettings", "value", 0, serverInfo.map);
			this.App.UI.AddItem("lstGameSettings", "property", 1, "Players:");
			this.App.UI.SetItemText("lstGameSettings", "value", 1, serverInfo.players.ToString());
			this.App.UI.AddItem("lstGameSettings", "property", 2, "Max Players:");
			this.App.UI.SetItemText("lstGameSettings", "value", 2, serverInfo.maxPlayers.ToString());
		}

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this._previousState = prev;
			if (((IEnumerable<object>)parms).Count<object>() > 0)
				this._enterState = (LobbyEntranceState)parms[0];
			this._camera = new OrbitCameraController(this.App);
			this._crits = new GameObjectSet(this.App);
			this._sky = new Sky(this.App, SkyUsage.InSystem, 3);
			this._crits.Add((IGameObject)this._sky);
			this._starmap = new StarMapLobby(this.App, this._sky);
			this._starmap.SetCamera(this._camera);
			this._painter = new ArrowPainter(this.App);
			this._shipCrits = new GameObjectSet(this.App);
			this._shipCamera = new OrbitCameraController(this.App);
			this._shipCrits.Add((IGameObject)this._shipCamera);
			this._shipHoloView = new ShipHoloView(this.App, this._shipCamera);
			this._shipCrits.Add((IGameObject)this._shipHoloView);
			this._builder = new ShipBuilder(this.App);
			this._tempPlayerInfo = new PlayerInfo();
			this._tempPlayerInfo.BadgeAssetPath = "";
			this._tempPlayerInfo.AvatarAssetPath = "";
			this._contextMenuID = this.App.UI.CreatePanelFromTemplate("SlotSwapContextMenu", null);
			this.App.UI.LoadScreen("StarMapLobby");
			this._starmap.Initialize(this._crits);
			this._crits.Add((IGameObject)this._starmap);
			this.App.Network.EnableChatWidget(true);
			this._settingsDirty.Clear();
			GameSetupUI.ClearPlayerListWidget(this.App, "lstPlayers");
		}

		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			if (eventName == "ObjectClicked")
				this.ProcessGameEvent_ObjectClicked(eventParams);
			else if (eventName == "ListContextMenu")
				this.ShowSlotSwapPopup(eventParams);
			else if (eventName == "ContextMenu")
				this.ProcessGameEvent_ContextMenu(eventParams);
			else if (eventName == "MouseOver")
			{
				this.ProcessGameEvent_MouseOver(eventParams);
			}
			else
			{
				if (!(eventName == "DragAndDropEvent"))
					return;
				string eventParam1 = eventParams[0];
				string eventParam2 = eventParams[1];
				string eventParam3 = eventParams[2];
				int.Parse(eventParams[3]);
			}
		}

		private void SetSelectedPlanet(int value, string trigger)
		{
			if (this._selectedPlanet == value)
				return;
			this._selectedPlanet = value;
		}

		public GameObjectSet GetCrits()
		{
			return this._crits;
		}

		private int InferServer(IGameObject obj)
		{
			int num;
			if (obj is StarMapServer && this._starmap.Servers.Forward.TryGetValue((StarMapServer)obj, out num))
				return num;
			return 0;
		}

		private void SelectObject(IGameObject o)
		{
			if (o == null)
			{
				this.SelectedServer = -1;
				GameSetupUI.ClearPlayerListWidget(this.App, "lstPlayers");
				this.App.UI.ClearItems("lstGameSettings");
			}
			else
			{
				this.SelectedServer = this.InferServer(o);
				this.App.Network.SelectServer(this.SelectedServer);
				this.SetGameDetails(this.SelectedServer);
				GameSetupUI.SyncPlayerListWidget(this.App, "lstPlayers", this._servers[this.SelectedServer].playerInfo, true);
			}
		}

		private void ProcessGameEvent_ContextMenu(string[] eventParams)
		{
			int id = int.Parse(eventParams[0]);
			if (id == 0)
				return;
			this.App.GetGameObject<StarMapSystem>(id);
		}

		private void ProcessGameEvent_ObjectClicked(string[] eventParams)
		{
			this.SelectObject(this.App.GetGameObject(int.Parse(eventParams[0])));
		}

		private void ProcessGameEvent_MouseOver(string[] eventParams)
		{
			int.Parse(eventParams[0]);
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (this._playerInitialSystemsSpinner.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self | PanelBinding.PanelMessageTargetFlags.Recursive) || this._playerInitialTechnologiesSpinner.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self | PanelBinding.PanelMessageTargetFlags.Recursive) || this._playerInitialTreasurySlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self | PanelBinding.PanelMessageTargetFlags.Recursive))
				return;
			if (msgType == "ChatMessage")
				this.App.UI.SetPropertyBool("btnChat", "flashing", true);
			if (msgType == "button_clicked")
			{
				if (panelName == "selectBackground")
				{
					this.App.UI.SetVisible("dlgSelectBadge", false);
					this.App.UI.SetVisible("dlgSelectAvatar", false);
				}
				else if (panelName == "buttonProcess")
				{
					this.App.UI.SetText("process_text", "");
					this.App.UI.SetVisible("buttonProcess", false);
					this.App.UI.SetVisible("process_dialog", false);
				}
				else if (panelName.EndsWith("btnFactionSelected"))
				{
					string factionItemId = panelName.Split('|')[0];
					int subfactionIndex = 0;
					if (factionItemId.EndsWith("_dlc"))
					{
						subfactionIndex = 1;
						factionItemId = factionItemId.Substring(0, factionItemId.Length - "_dlc".Length);
					}
					this.SelectFaction(factionItemId, subfactionIndex);
					this.App.UI.SetVisible("dlgSelectFaction", false);
					this.OnPlayerChanged(this.SelectedSlot, true, true);
				}
				else if (panelName.EndsWith("btnDifficultySelected"))
				{
					if (this.App.GameSetup.LocalPlayer.slot == this.SelectedSlot)
					{
						for (int slot = 0; slot < this.App.GameSetup.Players.Count; ++slot)
							this.App.GameSetup.SetDifficulty(slot, panelName.Split('|')[0]);
						this.App.UI.SetVisible("dlgSelectDifficulty", false);
						this.OnPlayerChanged(this.SelectedSlot, true, true);
					}
					else
					{
						this.SelectDifficulty(panelName.Split('|')[0]);
						this.App.UI.SetVisible("dlgSelectDifficulty", false);
						this.OnPlayerChanged(this.SelectedSlot, true, true);
					}
				}
				else if (panelName.EndsWith("btnAvatarSelected"))
				{
					this.SelectAvatar(panelName.Split('|')[0]);
					this.App.UI.SetVisible("dlgSelectAvatar", false);
					this.OnPlayerChanged(this.SelectedSlot, true, true);
				}
				else if (panelName.EndsWith("btnBadgeSelected"))
				{
					this.SelectBadge(panelName.Split('|')[0]);
					this.App.UI.SetVisible("dlgSelectBadge", false);
					this.OnPlayerChanged(this.SelectedSlot, true, true);
				}
				else if (panelName.EndsWith("btnEmpireColorSelected"))
				{
					this.App.UI.SetVisible("dlgSelectEmpireColor", false);
					this._dlgSelectEmpireColorVisible = false;
					this.SetEmpireColor(this.SelectedSlot, new int?(int.Parse(panelName.Split('|')[0])));
					this.OnPlayerChanged(this.SelectedSlot, false, false);
				}
				else if (panelName.EndsWith("btnShipColorAccept"))
				{
					this.App.UI.SetVisible("dlgSelectShipColor", false);
					this._dlgSelectShipColorVisible = false;
					this.OnPlayerChanged(this.SelectedSlot, false, false);
				}
				else if (panelName.StartsWith("team_button"))
				{
					string[] strArray = panelName.Split('|');
					if (((IEnumerable<string>)strArray).Count<string>() == 2)
					{
						int slot = int.Parse(strArray[1]);
						this.App.GameSetup.NextTeam(slot);
						GameSetupUI.SyncPlayerListWidget(this.App, "lstPlayers", this.App.GameSetup.Players, false);
						if (this.App.GameSetup.IsMultiplayer)
							this.App.Network.SetPlayerInfo(this.App.GameSetup.Players[slot], slot);
					}
				}
				// ISSUE: reference to a compiler-generated method
				switch (panelName)
				{
					case "btnSelectFaction":
						this.DisableAllFactionButtons();
						foreach (Faction key in this.App.GameSetup.AvailablePlayerFeatures.Factions.Keys)
						{
							this.App.UI.SetVisible(key.Name + "|btnFactionSelected", true);
							this.App.UI.SetEnabled(key.Name + "|btnFactionSelected", true);
							SteamDLCIdentifiers? identifierFromFaction = SteamDLCHelpers.GetDLCIdentifierFromFaction(key);
							if (identifierFromFaction.HasValue && this.App.Steam.HasDLC((int)identifierFromFaction.Value))
								this.App.UI.SetEnabled(key.Name + "_dlc|btnFactionSelected", true);
						}
						this.App.UI.SetVisible("dlgSelectFaction", true);
						return;
					case "btnHostGame":
						if (this.App.Network.IsHosting)
						{
							this.App.SwitchGameState<GameSetupState>();
							return;
						}
						this._createGameGUID = this.App.UI.CreateDialog((Dialog)new NetCreateGameDialog(this.App), null);
						return;
					case "btnSelectEmpireColor":
						if (this._dlgSelectEmpireColorVisible)
						{
							this._dlgSelectEmpireColorVisible = false;
							this.App.UI.SetVisible("dlgSelectEmpireColor", false);
						}
						else
						{
							this._dlgSelectEmpireColorVisible = true;
							int index1 = 0;
							this.App.UI.ClearItems("lstEmpireColors");
							foreach (int index2 in this.App.GameSetup.AvailablePlayerFeatures.EmpireColors.Select<GrabBagItem<int>, int>((Func<GrabBagItem<int>, int>)(x => x.Value)))
							{
								string globalId = this.App.UI.GetGlobalID(string.Format("dlgSelectEmpireColor.color{0}", (object)index2));
								if (this.App.GameSetup.IsEmpireColorUsed(index1))
								{
									this.App.UI.SetPropertyColor(this.App.UI.Path(globalId, "imgItemImage"), "color", Player.DefaultPrimaryPlayerColors[index2] * (float)byte.MaxValue);
									this.App.UI.SetVisible(this.App.UI.Path(globalId, "imgDisabled"), true);
									this.App.UI.SetEnabled(this.App.UI.Path(globalId, "btnImageButton"), false);
									this.App.UI.SetEnabled(this.App.UI.Path(globalId, string.Format("{0}|btnEmpireColorSelected", (object)index2)), false);
								}
								else
								{
									this.App.UI.SetVisible(this.App.UI.Path(globalId, "imgDisabled"), false);
									this.App.UI.SetEnabled(this.App.UI.Path(globalId, "btnImageButton"), true);
									this.App.UI.SetEnabled(this.App.UI.Path(globalId, string.Format("{0}|btnEmpireColorSelected", (object)index2)), true);
									this.App.UI.SetPropertyColor(this.App.UI.Path(globalId, "imgItemImage"), "color", Player.DefaultPrimaryPlayerColors[index2] * (float)byte.MaxValue);
									this.App.UI.SetPropertyString(this.App.UI.Path(globalId, "btnImageButton"), "id", string.Format("{0}|btnEmpireColorSelected", (object)index2));
								}
								++index1;
							}
							this.App.UI.SetVisible("dlgSelectEmpireColor", true);
						}
						if (!this._dlgSelectShipColorVisible)
							return;
						this._dlgSelectShipColorVisible = false;
						this.App.UI.SetVisible("dlgSelectShipColor", false);
						return;
					case "btnBack":
						if (this.App.Network.IsJoined || this.App.Network.IsHosting)
						{
							this.Reset();
							this.App.Network.Disconnect();
							this.HidePlayerSetup();
							this.MultiplayerButtonState();
							this.App.ResetGameSetup();
							this.App.GameSetup.IsMultiplayer = true;
							this.HideShipPreview();
							GameSetupUI.ClearPlayerListWidget(this.App, "lstPlayers");
							this.RefreshServers();
							return;
						}
						if (this._refreshing)
							this.RefreshServers();
						this.App.SwitchGameState("MainMenuState");
						return;
					case "buttonGameSettings":
						this.App.SwitchGameState<GameSetupState>();
						return;
					case "designShipClick":
						this.HideColorPicker();
						return;
					case "btnDirectConnect":
						this._directConnectGUID = this.App.UI.CreateDialog((Dialog)new DirectConnectDialog(this.App), null);
						return;
					case "btnEmpireName":
						this._empireGUID = this.App.UI.CreateDialog((Dialog)new GenericTextEntryDialog(this.App, App.Localize("@UI_EMPIRE_NAME"), App.Localize("@UI_ENTER_NEW_EMPIRE_NAME"), "", 32, 2, true, EditBoxFilterMode.None), null);
						return;
					case "btnStart":
						break;
					case "btnSelectBadge":
						int userItemId1 = 0;
						this.App.UI.ClearItems("lstBadges");
						foreach (string availableBadge in this.App.GameSetup.GetAvailableBadges(this.SelectedFaction))
						{
							string lower = availableBadge.ToLower();
							Faction faction = this.App.AssetDatabase.GetFaction(this.SelectedFaction);
							bool flag = false;
							int? dlcId = faction.DlcID;
							if (dlcId.HasValue)
							{
								ISteam steam = this.App.Steam;
								dlcId = faction.DlcID;
								int dlcID = dlcId.Value;
								if (steam.HasDLC(dlcID))
									flag = true;
							}
							if (!lower.Contains("dlc") | flag && availableBadge != this.App.GameSetup.Players[this.SelectedSlot].Badge)
							{
								this.App.UI.AddItem("lstBadges", string.Empty, userItemId1, string.Empty);
								string itemGlobalId = this.App.UI.GetItemGlobalID("lstBadges", string.Empty, userItemId1, string.Empty);
								this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "imgItemImage"), "sprite", availableBadge);
								this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "btnImageButton"), "id", string.Format("{0}|btnBadgeSelected", (object)availableBadge));
								++userItemId1;
							}
						}
						this.App.UI.SetPropertyString(this.App.UI.Path("dlgSelectBadge", "imgSelectedBadge"), "sprite", this.App.GameSetup.Players[this.SelectedSlot].Badge ?? string.Empty);
						this.App.UI.SetVisible("dlgSelectBadge", true);
						return;
					case "gameSwapButton":
						int slot1 = this.App.GameSetup.LocalPlayer.slot;
						int databaseId = this.App.GameSetup.Players[this._contextSlot].databaseId;
						if (this.App.GameSetup.IsMultiplayer)
						{
							this.App.Network.ChangeSlot(slot1, this._contextSlot);
							return;
						}
						if (this.App.Game != null && this.App.Game.LocalPlayer.ID != databaseId)
						{
							Player playerObject = this.App.Game.GetPlayerObject(databaseId);
							if (playerObject != null)
								this.App.Game.SetLocalPlayer(playerObject);
						}
						this.App.GameSetup.Players[this._contextSlot].Name = this.App.GameSetup.Players[slot1].Name;
						this.App.GameSetup.Players[this._contextSlot].localPlayer = true;
						this.App.GameSetup.Players[this._contextSlot].AI = false;
						this.App.GameSetup.Players[slot1].Name = string.Empty;
						this.App.GameSetup.Players[slot1].localPlayer = false;
						this.App.GameSetup.Players[slot1].AI = true;
						this.OnPlayerChanged(this._contextSlot, true, true);
						this.OnPlayerChanged(slot1, true, true);
						this.SetSelectedSlot(this._contextSlot);
						GameSetupUI.SyncPlayerSetupWidget(this.App, "pnlPlayerSetup", this.App.GameSetup.Players[this._contextSlot]);
						this.UpdateShipColors(this._contextSlot);
						this.UpdateShipPreview(this._contextSlot);
						return;
					case "gameKickButton":
						this.App.Network.Kick(this._contextSlot);
						return;
					case "btnPlayerSetupOk":
						this.HidePlayerSetup();
						return;
					case "btnChat":
						this.App.Network.SetChatWidgetVisibility(new bool?());
						this.App.UI.SetPropertyBool("btnChat", "flashing", false);
						return;
					case "btnSelectDifficulty":
						this.App.UI.SetVisible("dlgSelectDifficulty", true);
						return;
					case "btnRefreshServers":
						this._servers.Clear();
						this._starmap.ClearServers(this._crits);
						return;
					case "btnRefresh":
						this.RefreshServers();
						return;
					case "btnSelectSecondaryColor":
						this._dlgSelectShipColorVisible = !this._dlgSelectShipColorVisible;
						this.App.UI.SetVisible("dlgSelectShipColor", this._dlgSelectShipColorVisible);
						if (!this._dlgSelectEmpireColorVisible)
							return;
						this._dlgSelectEmpireColorVisible = false;
						this.App.UI.SetVisible("dlgSelectEmpireColor", false);
						return;
					case "btnJoinGame":
						break;
					case "btnSelectAvatar":
						int userItemId2 = 0;
						this.App.UI.ClearItems("lstAvatars");
						foreach (string availableAvatar in this.App.GameSetup.GetAvailableAvatars(this.SelectedFaction))
						{
							string lower = availableAvatar.ToLower();
							Faction faction = this.App.AssetDatabase.GetFaction(this.SelectedFaction);
							bool flag = false;
							int? dlcId = faction.DlcID;
							if (dlcId.HasValue)
							{
								ISteam steam = this.App.Steam;
								dlcId = faction.DlcID;
								int dlcID = dlcId.Value;
								if (steam.HasDLC(dlcID))
									flag = true;
							}
							if (!lower.Contains("dlc") | flag && availableAvatar != this.App.GameSetup.Players[this.SelectedSlot].Avatar && !this.App.GameSetup.IsAvatarUsed(availableAvatar))
							{
								this.App.UI.AddItem("lstAvatars", string.Empty, userItemId2, string.Empty);
								string itemGlobalId = this.App.UI.GetItemGlobalID("lstAvatars", string.Empty, userItemId2, string.Empty);
								this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "imgItemImage"), "sprite", availableAvatar);
								this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "btnImageButton"), "id", string.Format("{0}|btnAvatarSelected", (object)availableAvatar));
								++userItemId2;
							}
						}
						this.App.UI.SetPropertyString(this.App.UI.Path("dlgSelectAvatar", "imgSelectedAvatar"), "sprite", this.App.GameSetup.Players[this.SelectedSlot].Avatar ?? string.Empty);
						this.App.UI.SetVisible("dlgSelectAvatar", true);
						return;
					default:
						return;
				}
				if (!this.App.GameSetup.IsMultiplayer)
				{
					Action action = (Action)null;
					if (this._enterState == LobbyEntranceState.SinglePlayerLoad)
						action = new Action(this.LoadSinglePlayer);
					else if (this._enterState == LobbyEntranceState.SinglePlayer)
						action = new Action(this.NewSinglePlayer);
					this.App.SwitchGameStateViaLoadingScreen(action, (LoadingFinishedDelegate)null, (GameState)this.App.GetGameState<StarMapState>(), (object[])null);
				}
				else if (this.App.Network.IsJoined || this.App.Network.IsHosting)
				{
					this.App.Network.Ready();
				}
				else
				{
					this.App.GameSetup.ClearUsedAvatars();
					this.App.GameSetup.ClearUsedBadges();
					this.App.GameSetup.ClearUsedEmpireColors();
					this.App.UI.UnlockUI();
					ServerInfo server = this._servers[StarMapLobbyState._selectedIndex];
					if (server == null)
						return;
					if (server.passworded)
						this._passwordGUID = this.App.UI.CreateDialog((Dialog)new GenericTextEntryDialog(this.App, "Protected Game", "Enter Password: ", "", 30, 1, true, EditBoxFilterMode.None), null);
					else
						this.App.Network.JoinGame(server.serverID, "");
				}
			}
			else if (msgType == "dialog_closed")
			{
				if (panelName == this._loginGUID && ((IEnumerable<string>)msgParams).Count<string>() > 0)
				{
					if (msgParams[0] == "True")
					{
						if (this.App.GameSetup.IsMultiplayer)
							this.App.SwitchGameState<MainMenuState>();
						this.App.Network.SetChatWidgetVisibility(new bool?(false));
					}
					else
						this.RefreshServers();
				}
				else if (panelName == this._directConnectGUID)
				{
					if (!(msgParams[0] == "True"))
						return;
					StarMapLobbyState._lastIPTyped = msgParams[1];
					this.App.Network.DirectConnect(msgParams[1], msgParams[2]);
				}
				else if (panelName == this._passwordGUID)
				{
					if (!(msgParams[0] == "True"))
						return;
					this.App.Network.JoinGame(this._servers[StarMapLobbyState._selectedIndex].serverID, msgParams[1]);
				}
				else if (panelName == this._createGameGUID)
				{
					if (!(msgParams[0] == "False"))
						return;
					if (msgParams[1] == "False")
					{
						this.App.Network.UpdateGameInfo(msgParams[2], msgParams[3]);
						this.App.UI.SetVisible("buttonGameSettings", true);
						this.App.UI.SetVisible("buttonPlayerSettings", true);
						this.App.GameSetup.IsMultiplayer = true;
						this.CreateGame();
						this.App.Network.SetChatWidgetVisibility(new bool?(false));
					}
					else
					{
						this.App.Network.UpdateGameInfo(msgParams[2], msgParams[3]);
						this.App.Network.Host();
						this.App.Network.GameLoaded = true;
						this.EnablePlayerSetup(false);
						for (int index = 0; index < this.App.GameSetup.Players.Count<PlayerSetup>(); ++index)
						{
							if (this.App.GameDatabase.GetPlayerInfo(index + 1).isDefeated)
								this.App.GameSetup.Players[index].Locked = true;
						}
						this.App.Network.SetGameInfo(this.App.GameSetup);
						for (int slot = 0; slot < this.App.GameSetup.Players.Count<PlayerSetup>(); ++slot)
						{
							this.SetSelectedSlot(slot);
							this.OnPlayerChanged(slot, true, true);
						}
						int? lastClientPlayerId = this.App.GameDatabase.GetLastClientPlayerID(this.App.Network.Username);
						int? nullable = lastClientPlayerId.HasValue ? new int?(lastClientPlayerId.GetValueOrDefault() - 1) : new int?();
						if (!nullable.HasValue)
							nullable = new int?(0);
						this.App.GameSetup.Players[nullable.Value].AI = false;
						this.App.GameSetup.Players[nullable.Value].localPlayer = true;
						this.SetSelectedSlot(nullable.Value);
						this.OnPlayerChanged(nullable.Value, true, true);
						this.ShowPlayerSetup(nullable.Value);
						this.HostButtonState();
					}
				}
				else
				{
					if (!(panelName == this._empireGUID) || !(msgParams[0] == "True"))
						return;
					this.App.GameSetup.Players[this.SelectedSlot].EmpireName = msgParams[1];
					App.Log.Trace("Setting empire name to " + msgParams[1], "net");
					this.OnPlayerChanged(this.SelectedSlot, true, true);
				}
			}
			else if (msgType == "list_sel_changed")
			{
				if (panelName == "lstPlayers")
				{
					int index = int.Parse(msgParams[0]);
					if ((this.App.GameSetup.LocalPlayer == null || this.App.GameSetup.LocalPlayer.slot != index) && (!this.App.Network.IsHosting && this.App.GameSetup.IsMultiplayer))
						return;
					this.ShowPlayerSetup(index);
				}
				else
				{
					if (!(panelName == "lstServers"))
						return;
					int serverId = int.Parse(msgParams[0]);
					this.SelectedServer = serverId;
					this.FocusOnServer(serverId + 1);
					this.SetGameDetails(serverId);
				}
			}
			else
			{
				if (!(msgType == "color_changed") || !(panelName == "pickerSecondaryColor"))
					return;
				this.SetShipColor(this.SelectedSlot, new Vector3(float.Parse(msgParams[0]), float.Parse(msgParams[1]), float.Parse(msgParams[2])), false);
				this.OnPlayerChanged(this.SelectedSlot, false, false);
			}
		}

		protected override void OnEnter()
		{
			this.App.PostPlayMusic("Ambient_GameSetup");
			this.App.UI.UnlockUI();
			this.App.UI.SetScreen("StarMapLobby");
			this.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			this.App.UI.Send((object)"SetGameObject", (object)"ohStarMapLobby", (object)this._starmap.ObjectID);
			this.App.UI.SetListCleanClear("lstPlayers", true);
			this.App.Network.StarMapLobby = this;
			this.App.Network.InLobby(true);
			this._serverPositions = this.GenerateMeASpiralGalaxyPlease();
			this._crits.Activate();
			this._starmap.ClearServers(this._crits);
			this._servers.Clear();
			this._shipCamera.Active = false;
			this.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			this.App.UI.Send((object)"SetGameObject", (object)"designShip", (object)this._shipHoloView.ObjectID);
			this._camera.Active = true;
			this._camera.MaxDistance = 800f;
			this._camera.DesiredDistance = 200f;
			this._camera.DesiredYaw = MathHelper.DegreesToRadians(-90f);
			this._playerInitialTreasurySlider = new TreasurySlider(this.App.UI, "sldInitialTreasury", 0, this.App.GameSetup.HasScenarioFile() ? 0 : 0, this.App.GameSetup.HasScenarioFile() ? int.MaxValue : 1000000);
			this._playerInitialTreasurySlider.ValueChanged += new ValueChangedEventHandler(this.PanelValueChanged);
			this._playerInitialSystemsSpinner = new ValueBoundSpinner(this.App.UI, "sldInitialSystems", this.App.GameSetup.HasScenarioFile() ? 0.0 : 3.0, this.App.GameSetup.HasScenarioFile() ? (double)int.MaxValue : 9.0, (double)this.App.GameSetup.InitialSystems, 1.0);
			this._playerInitialSystemsSpinner.ValueChanged += new ValueChangedEventHandler(this.PanelValueChanged);
			this._playerInitialTechnologiesSpinner = new ValueBoundSpinner(this.App.UI, "sldInitialTechs", this.App.GameSetup.HasScenarioFile() ? 0.0 : 0.0, this.App.GameSetup.HasScenarioFile() ? (double)int.MaxValue : 10.0, (double)this.App.GameSetup.InitialTechnologies, 1.0);
			this._playerInitialTechnologiesSpinner.ValueChanged += new ValueChangedEventHandler(this.PanelValueChanged);
			this.HideColorPicker();
			this.App.IRC.SetupIRCClient(this.App.UserProfile.ProfileName);
			if (this._enterState == LobbyEntranceState.Browser)
			{
				GameSetupUI.ClearPlayerListWidget(this.App, "lstPlayers");
				this.Reset();
			}
			else
			{
				if (this._enterState == LobbyEntranceState.Multiplayer)
				{
					this.App.Network.Host();
					this.App.Network.SetGameInfo(this.App.GameSetup);
				}
				this.App.GameSetup.ClearUsedAvatars();
				this.App.GameSetup.ClearUsedEmpireColors();
				this._numPlayerSlots = 0;
				this.NumPlayerSlots = this.App.GameSetup.Players.Count;
				for (int index = 0; index < this.NumPlayerSlots; ++index)
				{
					if (index != 0)
					{
						this.App.GameSetup.Players[index].AI = true;
						this._settingsDirty[index] = true;
					}
					else
					{
						this.App.GameSetup.Players[0].localPlayer = true;
						this.App.GameSetup.Players[0].AI = false;
					}
					this.DefaultPlayer(index, this.App.GameSetup.HasScenarioFile());
					this.SetSelectedSlot(index);
					this.OnPlayerChanged(index, true, true);
				}
				this.ShowPlayerSetup(0);
			}
			if (this._enterState == LobbyEntranceState.Multiplayer || this._enterState == LobbyEntranceState.Browser)
			{
				if (this._enterState == LobbyEntranceState.Browser)
					this.App.Network.Login(this.App.UserProfile.ProfileName);
				if (this.App.Network.IsHosting)
					this.HostButtonState();
				else if (this.App.Network.IsJoined)
					this.JoinButtonState();
				else
					this.MultiplayerButtonState();
			}
			else
			{
				int index = 0;
				if (this._enterState == LobbyEntranceState.SinglePlayerLoad)
				{
					int? lastClientPlayerId = this.App.GameDatabase.GetLastClientPlayerID(this.App.UserProfile.ProfileName);
					if (lastClientPlayerId.HasValue)
					{
						index = lastClientPlayerId.Value - 1;
						this.App.Game.SetLocalPlayer(this.App.Game.GetPlayerObject(lastClientPlayerId.Value));
					}
				}
				this.App.GameSetup.Players[index].Name = this.App.UserProfile.ProfileName ?? string.Empty;
				if (this.App.GameSetup.Players[index].Name == null)
					this.App.SwitchGameState<MainMenuState>();
				this.App.GameSetup.Players[index].AI = false;
				this.App.GameSetup.Players[index].localPlayer = true;
				if (index != 0)
				{
					this.App.GameSetup.Players[0].AI = true;
					this.App.GameSetup.Players[0].localPlayer = false;
				}
				this.SinglePlayerButtonState();
				this.SetSelectedSlot(index);
				this.OnPlayerChanged(index, true, true);
				this.ShowPlayerSetup(index);
			}
			if (this._enterState == LobbyEntranceState.SinglePlayerLoad)
			{
				foreach (PlayerInfo playerInfo in this.App.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>())
					this.SetSlotEnabled(playerInfo.ID - 1, !playerInfo.isDefeated);
			}
			else
			{
				foreach (PlayerSetup player in this.App.GameSetup.Players)
					this.SetSlotEnabled(player.databaseId, true);
			}
			this.App.UI.SetEnabled("btnBack", true);
			this.App.UI.SetEnabled("btnStart", true);
			this.App.Network.InLobby(true);
		}

		private void SetSlotEnabled(int slot, bool enabled)
		{
			this.App.UI.SetVisible(this.App.UI.Path(this.App.UI.GetItemGlobalID("lstPlayers", string.Empty, slot, ""), "eliminatedState"), (!enabled ? 1 : 0) != 0);
		}

		public void OnRefreshComplete()
		{
			this._refreshing = false;
			this.App.UI.SetText("btnRefresh", "@UI_GAMEMODEBUTTONS_REFRESH");
		}

		protected void RefreshServers()
		{
			if (this._betaDisable)
				return;
			if (!this._refreshing)
			{
				this.App.UI.SetText("btnRefresh", "Stop Refresh");
				this._servers.Clear();
				this._starmap.ClearServers(this._crits);
				GameSetupUI.ClearPlayerListWidget(this.App, "lstPlayers");
				this._refreshing = true;
			}
			else
				this.OnRefreshComplete();
			this.App.Network.RefreshServers(!this._refreshing);
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this.Reset();
			this.App.Network.StarMapLobby = (StarMapLobbyState)null;
			this.App.Network.InLobby(false);
			this.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._playerInitialSystemsSpinner.ValueChanged -= new ValueChangedEventHandler(this.PanelValueChanged);
			this._playerInitialSystemsSpinner = (ValueBoundSpinner)null;
			this._playerInitialTechnologiesSpinner.ValueChanged -= new ValueChangedEventHandler(this.PanelValueChanged);
			this._playerInitialTechnologiesSpinner = (ValueBoundSpinner)null;
			this._playerInitialTreasurySlider.ValueChanged -= new ValueChangedEventHandler(this.PanelValueChanged);
			this._playerInitialTreasurySlider = (TreasurySlider)null;
			this.App.Network.InLobby(false);
			this.HidePlayerSetup();
			this._camera.Active = false;
			this._camera.TargetID = 0;
			this._builder.Dispose();
			if (this._crits != null)
			{
				this._crits.Dispose();
				this._crits = (GameObjectSet)null;
			}
			if (this._shipCrits != null)
			{
				this._shipCrits.Dispose();
				this._shipCrits = (GameObjectSet)null;
			}
			this.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this.App.UI.DestroyPanel(this._contextMenuID);
			this._painter.Dispose();
			this._starmap = (StarMapLobby)null;
			this.App.UI.DeleteScreen("StarMapLobby");
		}

		protected override void OnUpdate()
		{
			if (this.App.Network.IsHosting)
			{
				++this._frameCount;
				if (this._frameCount > 60)
				{
					foreach (KeyValuePair<int, bool> keyValuePair in this._settingsDirty)
						this.App.Network.SetPlayerInfo(this.App.GameSetup.Players[keyValuePair.Key], keyValuePair.Key);
					this._settingsDirty.Clear();
					this._frameCount = 0;
				}
			}
			if (this.App.GameSetup.IsMultiplayer)
				this.App.UI.SetEnabled("btnStart", StarMapLobbyState._selectedIndex != -1 || this.App.Network.IsHosting || this.App.Network.IsJoined);
			this._builder.Update();
			if (this._builder.Ship != null && !this._builder.Loading && (this._builder.Ship.Active && this._shipCamera.TargetID != this._builder.Ship.ObjectID))
			{
				this._shipCamera.TargetID = this._builder.Ship.ObjectID;
				this._shipHoloView.SetUseViewport(true);
				this._shipHoloView.SetShip(this._builder.Ship);
			}
			if (this._painter.ObjectStatus != GameObjectStatus.Ready || this._painter.Active)
				return;
			this._painter.Active = true;
			this._starmap.PostObjectAddObjects((IGameObject)this._painter);
		}

		public override bool IsReady()
		{
			return this._crits != null && this._crits.IsReady() && base.IsReady();
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}

		private void ShowSlotSwapPopup(string[] eventParams)
		{
			this.App.UI.AutoSize(this._contextMenuID);
			this._contextSlot = int.Parse(eventParams[0]);
			if (this._contextSlot < 0 || this._contextSlot > this.App.GameSetup.Players.Count<PlayerSetup>())
				return;
			if (!this.App.Network.IsHosting)
			{
				if (this.App.GameSetup.Players[this._contextSlot].localPlayer)
					return;
				this.App.UI.SetEnabled("gameKickButton", false);
				if (!this.App.GameSetup.Players[this._contextSlot].AI)
					return;
				this.App.UI.SetEnabled("gameSwapButton", true);
			}
			else
			{
				if (!this.App.Network.IsHosting || this.App.GameSetup.Players[this._contextSlot].localPlayer)
					return;
				if (!this.App.GameSetup.Players[this._contextSlot].AI)
					this.App.UI.SetEnabled("gameKickButton", true);
				else
					this.App.UI.SetEnabled("gameKickButton", false);
				this.App.UI.SetEnabled("gameSwapButton", true);
			}
			this.App.UI.ShowTooltip(this._contextMenuID, float.Parse(eventParams[1]), float.Parse(eventParams[2]));
		}

		private void PanelValueChanged(object sender, ValueChangedEventArgs e)
		{
			PlayerSetup player = this.App.GameSetup.Players[this.SelectedSlot];
			if (sender == this._playerInitialTreasurySlider)
				player.InitialTreasury = this._playerInitialTreasurySlider.Value;
			else if (sender == this._playerInitialSystemsSpinner)
				player.InitialColonies = (int)this._playerInitialSystemsSpinner.Value;
			else if (sender == this._playerInitialTechnologiesSpinner)
				player.InitialTechs = (int)this._playerInitialTechnologiesSpinner.Value;
			this._settingsDirty[this.SelectedSlot] = true;
		}

		protected void HostButtonState()
		{
			this.App.UI.SetText("btnStart", AssetDatabase.CommonStrings.Localize("@UI_GAMESETUP_READY"));
			this.App.UI.SetText("btnBack", AssetDatabase.CommonStrings.Localize("@UI_GAMESETUP_STOP_GAME"));
			this.App.UI.SetVisible("btnHostGame", false);
			this.App.UI.SetVisible("btnDirectConnect", false);
			this.App.UI.SetVisible("btnRefresh", false);
		}

		protected void JoinButtonState()
		{
			this.App.UI.SetText("btnStart", AssetDatabase.CommonStrings.Localize("@UI_GAMESETUP_READY"));
			this.App.UI.SetText("btnBack", AssetDatabase.CommonStrings.Localize("@UI_GAMESETUP_LEAVE_GAME"));
			this.App.UI.SetVisible("pnlMultiplayerGameType", false);
		}

		protected void MultiplayerButtonState()
		{
			this.App.UI.SetText("btnStart", AssetDatabase.CommonStrings.Localize("@UI_GAMESETUP_JOIN_GAME"));
			this.App.UI.SetText("btnBack", AssetDatabase.CommonStrings.Localize("@UI_GAMESETUP_CANCEL_GAME"));
			this.App.UI.SetText("btnHostGame", AssetDatabase.CommonStrings.Localize("@UI_GAMESETUP_HOST_GAME"));
			this.App.UI.SetPropertyBool("btnStart", "lockout_button", false);
			this.App.UI.SetVisible("btnHostGame", true);
			this.App.UI.SetVisible("btnDirectConnect", true);
			this.App.UI.SetVisible("pnlMultiplayerBar", true);
			this.App.UI.SetVisible("pnlSingleplayerBar", false);
			this.App.UI.SetVisible("pnlMultiplayerGameType", true);
			this.App.UI.SetVisible("btnRefresh", true);
			if (!this._betaDisable)
				return;
			this.App.UI.SetEnabled("btnHostGame", false);
			this.App.UI.SetEnabled("btnRefresh", false);
		}

		protected void SinglePlayerButtonState()
		{
			this.App.UI.SetText("btnStart", AssetDatabase.CommonStrings.Localize("@UI_GAMESETUP_START_GAME"));
			this.App.UI.SetText("btnBack", AssetDatabase.CommonStrings.Localize("@UI_GAMESETUP_CANCEL_GAME"));
			this.App.UI.SetVisible("pnlMultiplayerGameType", false);
			this.App.UI.SetVisible("pnlMultiplayerBar", false);
			this.App.UI.SetVisible("pnlSingleplayerBar", true);
			this.App.UI.SetPropertyBool("btnStart", "lockout_button", true);
		}

		protected void DisableAllFactionButtons()
		{
			this.App.UI.SetEnabled("human|btnFactionSelected", false);
			this.App.UI.SetEnabled("human_dlc|btnFactionSelected", false);
			this.App.UI.SetEnabled("hiver|btnFactionSelected", false);
			this.App.UI.SetEnabled("hiver_dlc|btnFactionSelected", false);
			this.App.UI.SetEnabled("tarkas|btnFactionSelected", false);
			this.App.UI.SetEnabled("tarkas_dlc|btnFactionSelected", false);
			this.App.UI.SetEnabled("zuul|btnFactionSelected", false);
			this.App.UI.SetEnabled("zuul_dlc|btnFactionSelected", false);
			this.App.UI.SetEnabled("morrigi|btnFactionSelected", false);
			this.App.UI.SetEnabled("morrigi_dlc|btnFactionSelected", false);
			this.App.UI.SetEnabled("liir_zuul|btnFactionSelected", false);
			this.App.UI.SetEnabled("liir_zuul_dlc|btnFactionSelected", false);
		}

		public void EnablePlayerSetup(bool enable)
		{
			this.App.UI.SetEnabled("btnEmpireName", enable);
			this.App.UI.SetEnabled("btnSelectAvatar", enable);
			this.App.UI.SetEnabled("btnSelectEmpireColor", enable);
			this.App.UI.SetEnabled("btnSelectSecondaryColor", enable);
			this.App.UI.SetEnabled("btnSelectBadge", enable);
			this.App.UI.SetEnabled("btnSelectFaction", enable);
			this.App.UI.SetEnabled("btnSelectDifficulty", enable);
			if (this._playerInitialTreasurySlider != null)
				this._playerInitialTreasurySlider.SetEnabled(enable);
			if (this._playerInitialSystemsSpinner != null)
				this._playerInitialSystemsSpinner.SetEnabled(enable);
			if (this._playerInitialTechnologiesSpinner == null)
				return;
			this._playerInitialTechnologiesSpinner.SetEnabled(enable);
		}

		public int GameInProgressTurn { get; set; }

		public bool IsLocalPlayerReady { get; set; }

		public void EnableGameInProgress(bool enable)
		{
			this.App.UI.SetVisible("pnlGameInProgressStatus", enable);
		}

		public void UpdateGameInProgress()
		{
			string text = string.Format("Game in progress: Turn {0}", (object)this.GameInProgressTurn);
			if (this.IsLocalPlayerReady)
				text = text + " " + string.Format("(You will join on the next turn update.)");
			this.App.UI.SetText("lblGameInProgressStatus", text);
		}

		private void HideShipPreview()
		{
			this._shipHoloView.HideViewport(true);
		}

		public void ShowPlayerSetup(int index)
		{
			this._camera.Active = false;
			this._shipCamera.DesiredYaw = 1.570796f;
			this._shipCamera.Active = true;
			this.App.UI.SetVisible("ohStarMapLobby", false);
			this.App.UI.SetVisible("pnlPlayerSetup", true);
			this._inPlayerSetup = true;
			PlayerSetup player = this.App.GameSetup.Players[index];
			this._tempPlayerInfo.BadgeAssetPath = string.IsNullOrEmpty(player.Badge) ? string.Empty : Path.Combine("factions", player.Faction, "badges", player.Badge + ".tga");
			this.SetSelectedSlot(index);
			this.UpdatePlayerSetupWidget(index);
		}

		protected void HidePlayerSetup()
		{
			this._camera.Active = true;
			this._shipCamera.Active = false;
			this.App.UI.SetVisible("pnlPlayerSetup", false);
			this.App.UI.SetVisible("ohStarMapLobby", true);
			this.HideShipPreview();
			this._inPlayerSetup = false;
		}

		protected void UpdatePlayerSetupWidget(int playerIndex)
		{
			this.UpdateShipColors(playerIndex);
			this.UpdateShipPreview(playerIndex);
			GameSetupUI.SyncPlayerSetupWidget(this.App, "pnlPlayerSetup", this.App.GameSetup.Players[playerIndex]);
		}

		private void CreateGame()
		{
			this.App.GameSetup.IsMultiplayer = true;
			this.App.Network.InLobby(false);
			this.App.SwitchGameState<GameSetupState>((object)true);
		}

		private void LoadSinglePlayer()
		{
			this.App.ConfirmAI();
		}

		private void NewSinglePlayer()
		{
			this.App.NewGame();
			this.App.ConfirmAI();
		}

		public void ClearStatus()
		{
			StarMapLobbyState._selectedIndex = -1;
			this.App.UI.SetVisible("servers", true);
		}

		public void OnNetworkError()
		{
			this.Reset();
		}

		public void OnJoined()
		{
			this.JoinButtonState();
		}

		public void Reset()
		{
			this._enterState = LobbyEntranceState.Browser;
			StarMapLobbyState._selectedIndex = 0;
			this._selectedSlot = 0;
			this.EnableGameInProgress(false);
			this.EnablePlayerSetup(true);
			this.ClearStatus();
			GameSetupUI.ClearPlayerListWidget(this.App, "lstPlayers");
			this.HideShipPreview();
			this.HideColorPicker();
			this.HidePlayerSetup();
			if (this.App.GameSetup.IsMultiplayer)
				this.MultiplayerButtonState();
			else
				this.SinglePlayerButtonState();
		}

		private void UpdateShipPreview(int index)
		{
			if (this.App.GameSetup.Players.Count <= index || index < 0)
				return;
			PlayerSetup player = this.App.GameSetup.Players[index];
			if (player.Faction == null || player.Faction == "")
				return;
			this._shipHoloView.HideViewport(false);
			this._tempPlayerInfo.SubfactionIndex = this.SelectedSubfactionIndex;
			this._tempPlayer = new Player(this.App, (GameSession)null, this._tempPlayerInfo, Player.ClientTypes.User);
			this._builder.Clear();
			this._builder.New(this._tempPlayer, this.App.AssetDatabase.ShipSections.Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
		   {
			   if (!(x.Faction == player.Faction) || x.Class != ShipClass.Cruiser)
				   return false;
			   if (!x.FileName.Contains("cr_cmd.section") && !x.FileName.Contains("cr_eng_fusion.section"))
				   return x.FileName.Contains("cr_mis_armor.section");
			   return true;
		   })), this.App.AssetDatabase.TurretHousings, this.App.AssetDatabase.Weapons, (IEnumerable<LogicalWeapon>)null, (IEnumerable<WeaponAssignment>)null, (IEnumerable<LogicalModule>)null, (IEnumerable<LogicalModule>)null, (IEnumerable<ModuleAssignment>)null, (IEnumerable<LogicalPsionic>)null, new DesignSectionInfo[0], this.App.AssetDatabase.Factions.Where<Faction>((Func<Faction, bool>)(x => x.Name == player.Faction)).First<Faction>(), "", "");
		}

		private void HideColorPicker()
		{
			this.App.UI.SetVisible("dlgSelectEmpireColor", false);
			this._dlgSelectEmpireColorVisible = false;
			this.App.UI.SetVisible("dlgSelectShipColor", false);
			this._dlgSelectShipColorVisible = false;
		}
	}
}
