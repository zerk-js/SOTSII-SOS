// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.App
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Console;
using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ScenarioFramework;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.IRC;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Steam;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Kerberos.Sots
{
	internal class App
	{
		private static List<NonClosingStreamWrapper> locks = new List<NonClosingStreamWrapper>();
		private readonly IFileSystem _fileSystem;
		private readonly GameObjectMediator _gameObjectMediator;
		private readonly GameStateMachine _gameStateMachine;
		private readonly ScriptCommChannel _scriptCommChannel;
		private readonly UICommChannel _uiCommChannel;
		private ISteam _steam;
		private SteamHelper _steamHelper;
		private Network _network;
		private bool _materialsReady;
		private bool _initialized;
		private bool _receivedDirectoryInfo;
		private GameSetup _gameSetup;
		private string _gameRoot;
		private string _profileDir;
		private string _baseSaveDir;
		private string _cacheDir;
		private string _settingsDir;
		private bool _engineExitRequested;
		private AssetDatabase _assetDatabase;
		private ConsoleApplet _consoleApplet;
		private static Random _safeRandom;
		private Profile _userProfile;
		private Settings _gameSettings;
		private string _consoleScriptFileName;
		private SotsIRC _ircChat;
		private HotKeyManager _hotkeyManager;
		private bool _profileSelected;
		private int _numExceptionErrorsDisplayed;
		private static GameUICommands m_Commands;
		private GameSession game;
		public static bool m_bAI_Enabled;
		public static bool m_bPlayerAI_Enabled;
		public static bool m_bDebugFup;

		public SteamHelper SteamHelper
		{
			get
			{
				return this._steamHelper;
			}
		}

		public ISteam Steam
		{
			get
			{
				return this._steam;
			}
		}

		public IList<TurnEvent> TurnEvents
		{
			get
			{
				return (IList<TurnEvent>)this.game.TurnEvents;
			}
		}

		public Network Network
		{
			get
			{
				return this._network;
			}
		}

		public SotsIRC IRC
		{
			get
			{
				return this._ircChat;
			}
		}

		public string CacheDir
		{
			get
			{
				return this._cacheDir;
			}
		}

		public string SettingsDir
		{
			get
			{
				return this._settingsDir;
			}
		}

		public IFileSystem FileSystem
		{
			get
			{
				return this._fileSystem;
			}
		}

		public App(ScriptHostParams scriptHostParams)
		{
			this._gameRoot = scriptHostParams.AssetDir;
			this._consoleScriptFileName = scriptHostParams.ConsoleScriptFileName;
			this._fileSystem = scriptHostParams.FileSystem;
			this._userProfile = new Profile();
			this._steam = scriptHostParams.SteamAPI;
			if (ScriptHost.AllowConsole)
			{
				this._consoleApplet = new ConsoleApplet();
				this._consoleApplet.Start();
				App.Log.MessageLogged += new MessageLoggedEventHandler(this.Log_MessageLogged);
			}
			this._gameObjectMediator = new GameObjectMediator(this);
			this._gameStateMachine = new GameStateMachine();
			this._scriptCommChannel = new ScriptCommChannel(scriptHostParams.ScriptMessageQueue);
			this._uiCommChannel = new UICommChannel(scriptHostParams.UIMessageQueue);
			this._network = (Network)this.AddObject(InteropGameObjectType.IGOT_NETWORK, (object[])null);
			this._ircChat = new SotsIRC(this);
			if (this._network != null)
				this._network.Initialize();
			this._uiCommChannel.GameEvent += new UIEventGameEvent(this.OnGameEvent);
			App.m_Commands = new GameUICommands(this);
			App.m_bAI_Enabled = true;
			App.m_bPlayerAI_Enabled = false;
			App.m_bDebugFup = false;
			this._hotkeyManager = new HotKeyManager(this, this._gameRoot);
			this._hotkeyManager.PostSetActive(true);
			this.AddGameStates();
			GameState gameState = (GameState)null;
			if (!string.IsNullOrEmpty(scriptHostParams.InitialStateName))
			{
				try
				{
					gameState = this._gameStateMachine.GetGameState(scriptHostParams.InitialStateName);
				}
				catch (ArgumentOutOfRangeException ex)
				{
				}
			}
			if (gameState == null)
				gameState = (GameState)this.GetGameState<MainMenuState>();
			this.SwitchGameStateWithoutTransitionSound<SplashState>((object)gameState);
		}

		private void OnGameEvent(string eventName, string[] eventParams)
		{
			if (!(eventName == "LocalizeText") || string.IsNullOrEmpty(eventParams[1]))
				return;
			this._uiCommChannel.LocalizeText(eventParams[0], App.Localize(eventParams[1]));
		}

		private void Log_MessageLogged(LogMessageInfo messageInfo)
		{
			string str = messageInfo.Category.Length != 0 ? messageInfo.Category : "•";
			this._consoleApplet.WriteText(messageInfo.Category, true, string.Format("{0,6}  ", (object)str), Color.Orchid);
			this._consoleApplet.WriteText(messageInfo.Category, false, messageInfo.Message, messageInfo.Severity == LogSeverity.Trace ? Color.LightBlue : Color.Orange);
			this._consoleApplet.WriteText(messageInfo.Category, false, "\r\n", Color.LightBlue);
		}

		public static Log Log
		{
			get
			{
				return ScriptHost.Log;
			}
		}

		public string GameRoot
		{
			get
			{
				return this._gameRoot;
			}
		}

		public string ProfileDir
		{
			get
			{
				return this._profileDir;
			}
		}

		public string SaveDir
		{
			get
			{
				if (this._userProfile == null || !this._userProfile.Loaded)
					return this._baseSaveDir;
				string path = this._baseSaveDir + (object)Path.DirectorySeparatorChar + this._userProfile.ProfileName;
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				return path;
			}
		}

		public string BaseSaveDir
		{
			get
			{
				return this._baseSaveDir;
			}
		}

		public UICommChannel UI
		{
			get
			{
				return this._uiCommChannel;
			}
		}

		public AssetDatabase AssetDatabase
		{
			get
			{
				return this._assetDatabase;
			}
		}

		public GameState CurrentState
		{
			get
			{
				return this._gameStateMachine.CurrentState;
			}
		}

		public GameState PendingState
		{
			get
			{
				return this._gameStateMachine.PendingState;
			}
		}

		public GameState PreviousState
		{
			get
			{
				return this._gameStateMachine.PreviousState;
			}
		}

		public IEnumerable<GameState> States
		{
			get
			{
				return (IEnumerable<GameState>)this._gameStateMachine;
			}
		}

		public GameSetup GameSetup
		{
			get
			{
				return this._gameSetup;
			}
		}

		public Profile UserProfile
		{
			get
			{
				return this._userProfile;
			}
			set
			{
				this._userProfile = value;
				this._profileSelected = true;
				this._ircChat.SetNick(value.ProfileName);
			}
		}

		public HotKeyManager HotKeyManager
		{
			get
			{
				return this._hotkeyManager;
			}
			set
			{
			}
		}

		public Settings GameSettings
		{
			get
			{
				return this._gameSettings;
			}
		}

		public bool ProfileSelected
		{
			get
			{
				return this._profileSelected;
			}
			set
			{
				this._profileSelected = value;
			}
		}

		public event Action<IGameObject> ObjectReleased;

		public GameDatabase GameDatabase
		{
			get
			{
				if (this.game == null)
					return (GameDatabase)null;
				return this.game.GameDatabase;
			}
		}

		public static string Localize(string strId)
		{
			return AssetDatabase.CommonStrings.Localize(strId);
		}

		public T GetStratModifier<T>(StratModifiers sm, int playerId)
		{
			return this.GameDatabase.GetStratModifier<T>(sm, playerId, (T)this.AssetDatabase.DefaultStratModifiers[sm]);
		}

		public Kerberos.Sots.PlayerFramework.Player GetPlayer(int playerId)
		{
			if (this.game == null)
				return (Kerberos.Sots.PlayerFramework.Player)null;
			return this.game.GetPlayerObject(playerId);
		}

		public Kerberos.Sots.PlayerFramework.Player GetPlayerByObjectID(int objectId)
		{
			if (this.game == null)
				return (Kerberos.Sots.PlayerFramework.Player)null;
			return this.game.GetPlayerObjectByObjectID(objectId);
		}

		public SavedGameFilename[] GetAllSaveGames()
		{
			string searchPattern = "*.sots2save";
			List<SavedGameFilename> savedGameFilenameList = new List<SavedGameFilename>();
			List<string> stringList = new List<string>();
			foreach (Profile availableProfile in Profile.GetAvailableProfiles())
			{
				string str = this._baseSaveDir + (object)Path.DirectorySeparatorChar + availableProfile.ProfileName;
				stringList.Add(str);
			}
			stringList.Add(this._baseSaveDir);
			foreach (string path in stringList)
			{
				try
				{
					savedGameFilenameList.AddRange(Directory.EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly).Select<string, SavedGameFilename>((Func<string, SavedGameFilename>)(x => new SavedGameFilename()
					{
						RootedFilename = x,
						IsBuiltin = false
					})));
				}
				catch (DirectoryNotFoundException ex)
				{
				}
			}
			return savedGameFilenameList.ToArray();
		}

		public SavedGameFilename[] GetAvailableSavedGames(bool IncludeAutosaves = true)
		{
			string searchPattern = "*.sots2save";
			SavedGameFilename[] savedGameFilenameArray = (SavedGameFilename[])null;
			string pattern = "\\((?:" + Regex.Replace(App.Localize("@AUTOSAVE_SUFFIX"), "([\\(\\)])", string.Empty) + "|Precombat)\\)";

			try
			{
				savedGameFilenameArray =	(from p in new DirectoryInfo(this.SaveDir).GetFiles(searchPattern, SearchOption.TopDirectoryOnly)
											where IncludeAutosaves || !Regex.IsMatch(p.Name, pattern)
											orderby p.LastWriteTime descending
											select p into x
											select new SavedGameFilename
											{
												RootedFilename = x.FullName,
												IsBuiltin = false
											}).ToArray<SavedGameFilename>();
			}
			catch (DirectoryNotFoundException ex)
			{
				if (savedGameFilenameArray == null)
					savedGameFilenameArray = new SavedGameFilename[0];
			}
			return ((IEnumerable<SavedGameFilename>)savedGameFilenameArray).ToArray<SavedGameFilename>();
		}

		public void RequestExit()
		{
			if (this._engineExitRequested)
				return;
			this._engineExitRequested = true;
			this._ircChat.Disconnect();
			this.ReleaseObject((IGameObject)this._network);
			this.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_EXIT);
			App.UnlockAllStreams();
			if (this._consoleApplet == null)
				return;
			this._consoleApplet.Stop();
		}

		public IGameObject GetGameObject(int id)
		{
			return this._gameObjectMediator.GetObject(id);
		}

		public T GetGameObject<T>(int id) where T : class, IGameObject
		{
			return this._gameObjectMediator.GetObject(id) as T;
		}

		public T GetGameState<T>() where T : GameState
		{
			return (T)this._gameStateMachine.FirstOrDefault<GameState>((Func<GameState, bool>)(x => x.GetType() == typeof(T)));
		}

		private bool SwitchGameStateViaLoadingScreen(
		  TimeSpan minTime,
		  string text,
		  string image,
		  Action action,
		  LoadingFinishedDelegate loadingFinishedDelegate,
		  GameState state,
		  params object[] parms)
		{
			this.PostRequestStopSounds();
			this.PostRequestGuiSound("universal_screentransition");
			this.PostDisableAllSounds();
			return this._gameStateMachine.SwitchGameState((GameState)this.GetGameState<LoadingScreenState>(), new object[7]
			{
		(object) (float) minTime.TotalSeconds,
		(object) text,
		(object) image,
		(object) action,
		(object) loadingFinishedDelegate,
		(object) state,
		(object) parms
			});
		}

		public bool SwitchGameStateViaLoadingScreen(
		  Action action,
		  LoadingFinishedDelegate loadingFinishedDelgate,
		  GameState state,
		  params object[] parms)
		{
			return this.SwitchGameStateViaLoadingScreen(TimeSpan.FromSeconds(1.0), App.Localize("@UI_LOADING_SCREEN_LOADING"), this._assetDatabase.GetRandomSplashScreenImageName(), action, loadingFinishedDelgate, state, parms);
		}

		public bool SwitchGameState(GameState value, params object[] parms)
		{
			return this._gameStateMachine.SwitchGameState(value, parms);
		}

		public bool SwitchGameState(string stateName)
		{
			this.PostRequestStopSounds();
			this.PostRequestGuiSound("universal_screentransition");
			this.PostDisableAllSounds();
			return this.SwitchGameState(this._gameStateMachine.GetGameState(stateName));
		}

		public bool SwitchGameState<T>(params object[] parms) where T : GameState
		{
			this.PostRequestStopSounds();
			this.PostRequestGuiSound("universal_screentransition");
			this.PostDisableAllSounds();
			return this.SwitchGameState((GameState)this.GetGameState<T>(), parms);
		}

		public bool SwitchGameStateWithoutTransitionSound<T>(params object[] parms) where T : GameState
		{
			this.PostRequestStopSounds();
			this.PostDisableAllSounds();
			return this.SwitchGameState((GameState)this.GetGameState<T>(), parms);
		}

		public bool PrepareGameState(GameState state, params object[] parms)
		{
			return this._gameStateMachine.PrepareGameState(state, parms);
		}

		public bool PrepareGameState<T>(params object[] parms) where T : GameState
		{
			return this._gameStateMachine.PrepareGameState((GameState)this.GetGameState<T>(), parms);
		}

		public void SwitchToPreparedGameState()
		{
			this.PostRequestStopSounds();
			this.PostRequestGuiSound("universal_screentransition");
			this.PostDisableAllSounds();
			this._gameStateMachine.SwitchToPreparedGameState();
		}

		public IGameObject AddObject(
		  InteropGameObjectType gameObjectType,
		  object[] initParams)
		{
			return this._gameObjectMediator.AddObject(gameObjectType, initParams);
		}

		public T AddObject<T>(params object[] initParams) where T : IGameObject
		{
			return (T)this._gameObjectMediator.AddObject(typeof(T), initParams);
		}

		public void AddExistingObject(IGameObject o, params object[] initParams)
		{
			this._gameObjectMediator.AddExistingObject(o, initParams);
		}

		public void SetObjectTag(IGameObject state, object value)
		{
			this._gameObjectMediator.SetObjectTag(state, value);
		}

		public void RemoveObjectTag(IGameObject state)
		{
			this._gameObjectMediator.RemoveObjectTag(state);
		}

		public object GetObjectTag(IGameObject state)
		{
			return this._gameObjectMediator.GetObjectTag(state);
		}

		public void PostEngineMessage(params object[] elements)
		{
			this._scriptCommChannel.SendMessage((IEnumerable)elements);
		}

		public void PostEngineMessage(IEnumerable elements)
		{
			this._scriptCommChannel.SendMessage(elements);
		}

		public void ReleaseObject(IGameObject value)
		{
			this._gameObjectMediator.ReleaseObject(value);
			if (this.ObjectReleased == null)
				return;
			this.ObjectReleased(value);
		}

		public void ReleaseObjects(IEnumerable<IGameObject> range)
		{
			this._gameObjectMediator.ReleaseObjects(range);
		}

		private void AddMaterialDictionaries(IEnumerable<string> names)
		{
			foreach (object name in names)
				this.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_MATERIALS_ADD, name);
			this.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_MATERIALS_REQ_READY);
		}

		private void AddCritHitChances(AssetDatabase.CritHitChances[] chc)
		{
			List<object> objectList = new List<object>();
			objectList.Add((object)InteropMessageID.IMID_ENGINE_INIT_STATS_PERCENTS);
			foreach (AssetDatabase.CritHitChances critHitChances in chc)
			{
				foreach (int chance in critHitChances.Chances)
					objectList.Add((object)chance);
			}
			this.PostEngineMessage(objectList.ToArray());
		}

		private NamesPool LoadNewNamesPool()
		{
			return NamesFramework.LoadFromXml("data\\Names.xml");
		}

		public void ResetGameSetup()
		{
			this._gameSetup.IsMultiplayer = false;
			this._gameSetup.Reset(this.AssetDatabase.Factions);
		}

		public void EndGame()
		{
			if (this.game != null)
			{
				this.game.Dispose();
				this.game = (GameSession)null;
			}
			this.ResetGameSetup();
		}

		public void NewGame()
		{
			if (this.game != null)
			{
				this.game.Dispose();
				this.game = (GameSession)null;
			}
			this.game = App.NewGame(this, (Random)null, this.GameSetup, this.AssetDatabase, this._gameSetup, (GameSession.Flags)0);
		}

		public void ConfirmAI()
		{
			if (this.Game == null)
				return;
			foreach (PlayerSetup player in this.GameSetup.Players)
				this.Game.GetPlayerObject(player.databaseId).SetAI(player.AI);
		}

		public void UILoadGame(string fileToLoad)
		{
			bool isMultiplayer = this.GameSetup.IsMultiplayer;
			if (!isMultiplayer)
				this.GameSetup.ResetPlayers();
			this.LoadGame(fileToLoad, this.GameSetup);
			this.GameSetup.IsMultiplayer = isMultiplayer;
			if (this.CurrentState == this.GetGameState<StarMapLobbyState>())
				return;
			this.SwitchGameState<StarMapLobbyState>((object)LobbyEntranceState.SinglePlayerLoad);
		}

		public void LoadGame(string filename, GameSetup gs)
		{
			GameDatabase db = GameDatabase.Load(filename, this.AssetDatabase);
			if (!this.Network.IsJoined)
			{
				List<FactionInfo> list = db.GetFactions().ToList<FactionInfo>();
				List<Faction> factionList = new List<Faction>();
				foreach (FactionInfo factionInfo in list)
					factionList.Add(this.AssetDatabase.GetFaction(factionInfo.Name));
				gs.Reset((IEnumerable<Faction>)factionList);
				gs.Players.Clear();
				gs.StrategicTurnLength = db.GetTurnLength(GameDatabase.TurnLengthTypes.Strategic);
				gs.CombatTurnLength = db.GetTurnLength(GameDatabase.TurnLengthTypes.Combat);
				gs.RandomEncounterFrequency = db.GetRandomEncounterFrequency();
				gs.EconomicEfficiency = db.GetEconomicEfficiency();
				gs.ResearchEfficiency = db.GetResearchEfficiency();
				foreach (PlayerInfo playerInfo in db.GetPlayerInfos())
				{
					PlayerSetup playerSetup = new PlayerSetup();
					Faction faction = (Faction)null;
					foreach (FactionInfo factionInfo in list)
					{
						if (factionInfo.ID == playerInfo.FactionID)
							faction = this.AssetDatabase.GetFaction(factionInfo.Name);
					}
					if (faction != null && faction.IsPlayable && playerInfo.isStandardPlayer)
					{
						playerSetup.Faction = faction.Name;
						playerSetup.SubfactionIndex = playerInfo.SubfactionIndex;
						playerSetup.AI = true;
						playerSetup.Avatar = Path.GetFileNameWithoutExtension(playerInfo.AvatarAssetPath);
						playerSetup.Badge = Path.GetFileNameWithoutExtension(playerInfo.BadgeAssetPath);
						playerSetup.EmpireName = playerInfo.Name;
						playerSetup.ShipColor = playerInfo.SecondaryColor;
						playerSetup.slot = playerInfo.ID - 1;
						playerSetup.Fixed = true;
						playerSetup.Team = playerInfo.Team;
						playerSetup.AIDifficulty = playerInfo.AIDifficulty;
						int num = Kerberos.Sots.PlayerFramework.Player.DefaultPrimaryPlayerColors.IndexOf(playerInfo.PrimaryColor);
						if (num != -1)
							playerSetup.EmpireColor = new int?(num);
						gs.Players.Add(playerSetup);
					}
				}
			}
			NamesPool namesPool = this.LoadNewNamesPool();
			List<Trigger> triggerList = new List<Trigger>();
			this.game = new GameSession(this, db, gs, filename, namesPool, (IList<Trigger>)triggerList, App.GetSafeRandom(), GameSession.Flags.ResumingGame);
			if (this.UserProfile == null)
				return;
			this.UserProfile.LastGamePlayed = filename;
			this.UserProfile.SaveProfile();
		}

		public void NewGame(Random initializationRandomSeed)
		{
			if (this.game != null)
			{
				this.game.GameDatabase.Dispose();
				this.game = (GameSession)null;
			}
			this.game = App.NewGame(this, initializationRandomSeed, this.GameSetup, this.AssetDatabase, this._gameSetup, (GameSession.Flags)0);
		}

		public void StartGame(Action action, LoadingFinishedDelegate del, params object[] parms)
		{
			this.SwitchGameStateViaLoadingScreen(action, del, (GameState)this.GetGameState<StarMapState>(), parms);
		}

		public static GameSession NewGame(
		  App game,
		  Random initializationRandomSeed,
		  GameSetup gameSetup,
		  AssetDatabase assetDatabase,
		  GameSetup gs,
		  GameSession.Flags flags = (GameSession.Flags)0)
		{
			flags &= ~GameSession.Flags.ResumingGame;
			gameSetup.FinalizeSetup();
			if (initializationRandomSeed == null)
				initializationRandomSeed = new Random();
			NamesPool namesPool = game.LoadNewNamesPool();
			List<Trigger> triggerList = new List<Trigger>();
			List<int> intList = new List<int>();
			string scenarioFile = gameSetup.ScenarioFile;
			Scenario s1 = (Scenario)null;
			bool flag = !string.IsNullOrEmpty(gameSetup.ScenarioFile);
			GameDatabase gameDatabase = GameDatabase.New(gameSetup.GameName, assetDatabase, true);
			gameDatabase.SetClientId(1);
			gameDatabase.InsertNameValuePair("VictoryCondition", gameSetup._mode.ToString());
			gameDatabase.InsertNameValuePair("VictoryValue", gameSetup._modeValue.ToString());
			gameDatabase.InsertNameValuePair("GMCount", 0.ToString());
			gameDatabase.InsertNameValuePair("GSGrandMenaceCount", gameSetup._grandMenaceCount.ToString());
			gameDatabase.InsertNameValuePair("ResearchEfficiency", gameSetup._researchEfficiency.ToString());
			gameDatabase.InsertNameValuePair("EconomicEfficiency", gameSetup._economicEfficiency.ToString());
			gameDatabase.InsertNameValuePair("RandomEncounterFrequency", gameSetup._randomEncounterFrequency.ToString());
			gameDatabase.SetTurnLength(GameDatabase.TurnLengthTypes.Strategic, gameSetup.StrategicTurnLength);
			gameDatabase.SetTurnLength(GameDatabase.TurnLengthTypes.Combat, gameSetup.CombatTurnLength);
			gameDatabase.SetRandomEncounterFrequency(gameSetup.RandomEncounterFrequency);
			gameDatabase.SetEconomicEfficiency(gameSetup.EconomicEfficiency);
			gameDatabase.SetResearchEfficiency(gameSetup.ResearchEfficiency);
			if ((flags & GameSession.Flags.NoTechTree) == (GameSession.Flags)0)
			{
				foreach (Kerberos.Sots.Data.TechnologyFramework.Tech technology in assetDatabase.MasterTechTree.Technologies)
					gameDatabase.InsertTech(technology.Id);
			}
			gameDatabase.InsertMissingFactions(initializationRandomSeed);
			StarSystemVars.LoadXml("data\\StarSystemVars.xml");
			string str;
			if (flag)
			{
				s1 = new Scenario();
				ScenarioXmlUtility.LoadScenarioFromXml(gameSetup.ScenarioFile, ref s1);
				gameDatabase.SetEconomicEfficiency(s1.EconomicEfficiency);
				gameDatabase.SetResearchEfficiency(s1.ResearchEfficiency);
				triggerList.AddRange((IEnumerable<Trigger>)s1.Triggers);
				gameSetup.Players.Clear();
				foreach (Kerberos.Sots.Data.ScenarioFramework.Player playerStartCondition in s1.PlayerStartConditions)
					gameSetup.Players.Add(new PlayerSetup()
					{
						EmpireName = playerStartCondition.Name,
						Faction = playerStartCondition.Faction,
						AI = playerStartCondition.isAI,
						AIDifficulty = playerStartCondition.isAI ? (AIDifficulty)Enum.Parse(typeof(AIDifficulty), playerStartCondition.AIDifficulty) : AIDifficulty.Normal,
						Avatar = playerStartCondition.Avatar,
						Badge = playerStartCondition.Badge,
						ShipColor = playerStartCondition.ShipColor,
						InitialColonies = 0,
						InitialTechs = 0,
						InitialTreasury = (int)playerStartCondition.Treasury
					});
				str = s1.Starmap;
			}
			else
			{
				str = "starmaps\\FIGHT.Starmap";
				if (!string.IsNullOrEmpty(gameSetup.StarMapFile))
					str = gameSetup.StarMapFile;
			}
			gameDatabase.InsertNameValuePair("map_name", Path.GetFileNameWithoutExtension(str));
			LegacyStarMap starMapFromFileCore = LegacyStarMap.CreateStarMapFromFileCore(initializationRandomSeed, str);
			List<App.StartLocation> startLocationList = App.RandomizeStartLocations(App.CollectStartLocations(starMapFromFileCore), (IList<PlayerSetup>)gameSetup.Players);
			if (startLocationList.Count != gameSetup.Players.Count)
				throw new InvalidDataException(string.Format("Number of randomized start locations ({0}) does not match number of players in game setup ({1}).", (object)startLocationList.Count, (object)gameSetup.Players.Count));
			App.EnsureInhabitableStartLocation(initializationRandomSeed, startLocationList, (IList<PlayerSetup>)gameSetup.Players, gameDatabase);
			starMapFromFileCore.AssignEmptySystemNames(initializationRandomSeed, namesPool);
			starMapFromFileCore.AssignEmptyPlanetTypes(initializationRandomSeed);
			starMapFromFileCore.AssignEmptyPlanetParameters(initializationRandomSeed);
			StarSystemHelper.VerifyStarMap(starMapFromFileCore);
			gameDatabase.ImportStarMap(ref starMapFromFileCore, initializationRandomSeed, flags);
			Random random = new Random();
			List<int> list1 = Enumerable.Range(0, !gameSetup.IsMultiplayer ? Kerberos.Sots.PlayerFramework.Player.DefaultPrimaryPlayerColors.Count : Kerberos.Sots.PlayerFramework.Player.DefaultMPPrimaryPlayerColors.Count).ToList<int>();
			foreach (PlayerSetup player in gameSetup.Players)
			{
				if (player.EmpireColor.HasValue)
					list1.Remove(player.EmpireColor.Value);
			}
			foreach (PlayerSetup player in gameSetup.Players)
			{
				if (!player.EmpireColor.HasValue && list1.Count > 0)
				{
					player.EmpireColor = new int?(initializationRandomSeed.Choose<int>((IList<int>)list1));
					list1.Remove(player.EmpireColor.Value);
				}
			}
			for (int index1 = 0; index1 < startLocationList.Count<App.StartLocation>(); ++index1)
			{
				PlayerSetup player = gameSetup.Players[startLocationList[index1].PlayerIndex];
				int? homeworldID = new int?(startLocationList[index1].Planet.ID);
				int? nullable = homeworldID;
				if ((nullable.GetValueOrDefault() != 0 ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
					homeworldID = new int?();
				if ((flags & GameSession.Flags.NoOrbitalObjects) == (GameSession.Flags)0 && (!homeworldID.HasValue || gameDatabase.GetPlanetInfo(homeworldID.Value) == null || gameDatabase.GetOrbitalObjectInfo(homeworldID.Value) == null))
					throw new NullReferenceException("Planet or orbital object ID missing: " + (object)homeworldID);
				if (string.IsNullOrEmpty(player.Badge))
					player.Badge = Path.GetFileNameWithoutExtension(assetDatabase.GetRandomBadgeTexture(player.Faction, initializationRandomSeed));
				if (string.IsNullOrEmpty(player.Avatar))
					player.Avatar = Path.GetFileNameWithoutExtension(assetDatabase.GetRandomAvatarTexture(player.Faction, initializationRandomSeed));
				Vector3 one = Vector3.One;
				nullable = player.EmpireColor;
				if (nullable.HasValue)
				{
					if (!gameSetup.IsMultiplayer)
					{
						List<Vector3> primaryPlayerColors = Kerberos.Sots.PlayerFramework.Player.DefaultPrimaryPlayerColors;
						nullable = player.EmpireColor;
						int index2 = nullable.Value;
						one = primaryPlayerColors[index2];
					}
					else
					{
						List<Vector3> primaryPlayerColors = Kerberos.Sots.PlayerFramework.Player.DefaultMPPrimaryPlayerColors;
						nullable = player.EmpireColor;
						int index2 = nullable.Value;
						one = primaryPlayerColors[index2];
					}
				}
				if (flag)
				{
					int num1;
					if (!player.AI)
					{
						num1 = gameDatabase.InsertPlayer(player.EmpireName, player.Faction, homeworldID, one, player.ShipColor, player.GetBadgeTextureAssetPath(assetDatabase), player.GetAvatarTextureAssetPath(assetDatabase), (double)player.InitialTreasury, 0, true, false, s1.PlayerStartConditions[index1].isAIRebellion, 0, player.AIDifficulty);
					}
					else
					{
						num1 = gameDatabase.InsertPlayer(player.EmpireName, player.Faction, homeworldID, one, player.ShipColor, player.GetBadgeTextureAssetPath(assetDatabase), player.GetAvatarTextureAssetPath(assetDatabase), (double)player.InitialTreasury, 0, true, false, s1.PlayerStartConditions[index1].isAIRebellion, 0, player.AIDifficulty);
						intList.Add(num1);
					}
					App.AddStratModifiers(gameDatabase, assetDatabase, num1);
					if ((flags & GameSession.Flags.NoTechTree) == (GameSession.Flags)0)
					{
						ResearchScreenState.BuildPlayerTechTree(game, assetDatabase, gameDatabase, num1, s1.PlayerStartConditions[index1].AvailableTechs);
						List<PlayerTechInfo> list2 = gameDatabase.GetPlayerTechInfos(num1).ToList<PlayerTechInfo>();
						foreach (Kerberos.Sots.Data.ScenarioFramework.Tech startingTech in s1.PlayerStartConditions[index1].StartingTechs)
						{
							Kerberos.Sots.Data.ScenarioFramework.Tech t = startingTech;
							PlayerTechInfo techInfo = list2.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == t.Name));
							if (techInfo != null)
							{
								techInfo.Progress = 100;
								techInfo.State = TechStates.Researched;
								gameDatabase.UpdatePlayerTechInfo(techInfo);
							}
						}
						gameDatabase.UpdateLockedTechs(assetDatabase, num1);
					}
					if ((flags & GameSession.Flags.NoOrbitalObjects) == (GameSession.Flags)0)
					{
						foreach (Kerberos.Sots.Data.ScenarioFramework.Colony colony in s1.PlayerStartConditions[index1].Colonies)
						{
							Kerberos.Sots.Data.ScenarioFramework.Colony c = colony;
							StarSystem starSystem = starMapFromFileCore.Objects.OfType<StarSystem>().First<StarSystem>((Func<StarSystem, bool>)(x =>
						   {
							   int? guid = x.Params.Guid;
							   int systemId = c.SystemId;
							   if (guid.GetValueOrDefault() == systemId)
								   return guid.HasValue;
							   return false;
						   }));
							IStellarEntity stellarEntity = starSystem.Objects.First<IStellarEntity>((Func<IStellarEntity, bool>)(x => x.Params.OrbitNumber == c.OrbitId));
							if (c.IsIdealColony)
								GameSession.MakeIdealColony(gameDatabase, assetDatabase, c.OrbitId, num1, IdealColonyTypes.Primary);
							double imperialPopulation = c.ImperialPopulation;
							foreach (CivilianPopulation civilianPopulation in c.CivilianPopulations)
								imperialPopulation += civilianPopulation.Population;
							int colonyID = gameDatabase.InsertColony(stellarEntity.ID, num1, c.ImperialPopulation, imperialPopulation == 0.0 ? 0.0f : (float)(c.ImperialPopulation / imperialPopulation), 1, (float)c.Infrastructure, true);
							ColonyInfo colonyInfo = gameDatabase.GetColonyInfo(colonyID);
							PlanetInfo planetInfo = gameDatabase.GetPlanetInfo(stellarEntity.ID);
							Kerberos.Sots.Strategy.InhabitedPlanet.Colony.SetOutputRate(gameDatabase, assetDatabase, ref colonyInfo, planetInfo, Kerberos.Sots.Strategy.InhabitedPlanet.Colony.OutputRate.Trade, 0.0f);
							gameDatabase.UpdateColony(colonyInfo);
							gameDatabase.InsertExploreRecord(starSystem.ID, num1, 1, true, true);
							foreach (CivilianPopulation civilianPopulation in c.CivilianPopulations)
								gameDatabase.InsertColonyFaction(stellarEntity.ID, gameDatabase.GetFactionIdFromName(civilianPopulation.Faction), civilianPopulation.Population, (float)(civilianPopulation.Population / imperialPopulation), 1);
						}
						Dictionary<string, int> dictionary1 = new Dictionary<string, int>();
						foreach (Station station in s1.PlayerStartConditions[index1].Stations)
						{
							Station s = station;
							OrbitalPath path = new OrbitalPath();
							path.Scale = new Vector2(10f, 10f);
							path.Rotation = new Vector3(0.0f, 0.0f, 0.0f);
							path.DeltaAngle = 10f;
							path.InitialAngle = 10f;
							StarSystem starSystem = starMapFromFileCore.Objects.OfType<StarSystem>().First<StarSystem>((Func<StarSystem, bool>)(x =>
						   {
							   int? guid = x.Params.Guid;
							   int location = s.Location;
							   if (guid.GetValueOrDefault() == location)
								   return guid.HasValue;
							   return false;
						   }));
							IStellarEntity se = starSystem.Objects.First<IStellarEntity>((Func<IStellarEntity, bool>)(x => x.Params.OrbitNumber == s.Orbit));
							OrbitalObjectInfo orbitalObjectInfo = gameDatabase.GetStarSystemOrbitalObjectInfos(starSystem.ID).ToList<OrbitalObjectInfo>().FirstOrDefault<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => (double)x.OrbitalPath.Scale.Y == (double)se.Orbit.SemiMajorAxis));
							DesignInfo stationDesignInfo = DesignLab.CreateStationDesignInfo(assetDatabase, gameDatabase, num1, ScenarioEnumerations.StationTypes[s.Type], s.Stage, true);
							int num2 = gameDatabase.InsertStation(orbitalObjectInfo.ID, starSystem.ID, path, s.Name, num1, stationDesignInfo);
							dictionary1.Add(s.Name, num2);
						}
						Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
						foreach (Kerberos.Sots.Data.ScenarioFramework.Ship shipDesign in s1.PlayerStartConditions[index1].ShipDesigns)
						{
							DesignInfo design = new DesignInfo();
							design.PlayerID = num1;
							design.Name = shipDesign.Name;
							List<DesignSectionInfo> designSectionInfoList = new List<DesignSectionInfo>();
							foreach (Kerberos.Sots.Data.ScenarioFramework.Section section in shipDesign.Sections)
							{
								DesignSectionInfo designSectionInfo = new DesignSectionInfo();
								designSectionInfo.DesignInfo = design;
								designSectionInfo.FilePath = string.Format("factions\\{0}\\sections\\{1}", (object)shipDesign.Faction, (object)section.SectionFile);
								List<WeaponBankInfo> weaponBankInfoList = new List<WeaponBankInfo>();
								foreach (Bank bank in section.Banks)
								{
									if (!string.IsNullOrEmpty(bank.GUID))
										weaponBankInfoList.Add(new WeaponBankInfo()
										{
											WeaponID = gameDatabase.GetWeaponID(bank.Weapon, num1),
											BankGUID = Guid.Parse(bank.GUID)
										});
								}
								designSectionInfo.WeaponBanks = weaponBankInfoList;
								designSectionInfoList.Add(designSectionInfo);
							}
							design.DesignSections = designSectionInfoList.ToArray();
							DesignLab.SummarizeDesign(assetDatabase, gameDatabase, design);
							dictionary2.Add(shipDesign.Name, gameDatabase.InsertDesignByDesignInfo(design));
						}
						Dictionary<string, int> dictionary3 = new Dictionary<string, int>();
						foreach (Admiral admiral in s1.PlayerStartConditions[index1].Admirals)
						{
							Admiral a = admiral;
							starMapFromFileCore.Objects.OfType<StarSystem>().First<StarSystem>((Func<StarSystem, bool>)(x =>
						   {
							   int? guid = x.Params.Guid;
							   int homePlanet = a.HomePlanet;
							   if (guid.GetValueOrDefault() == homePlanet)
								   return guid.HasValue;
							   return false;
						   }));
							int num2 = gameDatabase.InsertAdmiral(num1, new int?(), a.Name, a.Faction, (float)a.Age, a.Gender, a.ReactionRating, a.EvasionRating, 50);
							foreach (SpecialCharacteristic specialCharacteristic in a.SpecialCharacteristics)
								;
							dictionary3.Add(a.Name, num2);
						}
						foreach (Fleet fleet in s1.PlayerStartConditions[index1].Fleets)
						{
							Fleet f = fleet;
							StarSystem starSystem = starMapFromFileCore.Objects.OfType<StarSystem>().First<StarSystem>((Func<StarSystem, bool>)(x =>
						   {
							   int? guid = x.Params.Guid;
							   int location = f.Location;
							   if (guid.GetValueOrDefault() == location)
								   return guid.HasValue;
							   return false;
						   }));
							int fleetID = gameDatabase.InsertFleet(num1, dictionary3[f.Admiral], starSystem.ID, starSystem.ID, f.Name, FleetType.FL_NORMAL);
							foreach (Kerberos.Sots.Data.ScenarioFramework.Ship ship in f.Ships)
								gameDatabase.InsertShip(fleetID, dictionary2[ship.Name], null, (ShipParams)0, new int?(), 0);
							gameDatabase.LayoutFleet(fleetID);
						}
					}
					flags |= GameSession.Flags.NoDefaultFleets;
				}
				else
				{
					int num = gameDatabase.InsertPlayer(player.EmpireName, player.Faction, homeworldID, one, player.ShipColor, player.GetBadgeTextureAssetPath(assetDatabase), player.GetAvatarTextureAssetPath(assetDatabase), (double)game.GameSetup.Players[startLocationList[index1].PlayerIndex].InitialTreasury, player.SubfactionIndex, true, false, false, player.Team, player.AIDifficulty);
					if (!player.AI)
					{
						if (!player.localPlayer)
							;
					}
					else
						intList.Add(num);
					App.BuildTeamDiplomacyStates(gameDatabase);
					App.AddStratModifiers(gameDatabase, assetDatabase, num);
					if ((flags & GameSession.Flags.NoTechTree) == (GameSession.Flags)0)
						ResearchScreenState.BuildPlayerTechTree(game, assetDatabase, gameDatabase, num);
					if ((flags & GameSession.Flags.NoOrbitalObjects) == (GameSession.Flags)0)
					{
						GameSession.MakeIdealColony(gameDatabase, assetDatabase, homeworldID.Value, num, IdealColonyTypes.Primary);
						OrbitalPath path = new OrbitalPath();
						path.Scale = new Vector2(10f, 10f);
						path.Rotation = new Vector3(0.0f, 0.0f, 0.0f);
						path.DeltaAngle = 10f;
						path.InitialAngle = 10f;
						DesignInfo stationDesignInfo = DesignLab.CreateStationDesignInfo(assetDatabase, gameDatabase, num, StationType.NAVAL, 3, true);
						gameDatabase.InsertStation(homeworldID.Value, startLocationList[index1].System.ID, path, "Naval Base", num, stationDesignInfo);
					}
				}
			}
			if ((flags & GameSession.Flags.NoDefaultFleets) == (GameSession.Flags)0)
				App.AddSuulkas(assetDatabase, gameDatabase);
			if (flag)
			{
				foreach (Kerberos.Sots.Data.ScenarioFramework.Player playerStartCondition in s1.PlayerStartConditions)
				{
					foreach (PlayerRelation relation in playerStartCondition.Relations)
						gameDatabase.UpdateDiplomacyState(playerStartCondition.PlayerSlot, relation.Player, relation.DiplomacyState, relation.Relations, true);
				}
			}
			GameSession sim = new GameSession(game, gameDatabase, gs, gs.GetDefaultSaveGameFileName(), namesPool, (IList<Trigger>)triggerList, initializationRandomSeed, flags);
			foreach (ColonyInfo colony in gameDatabase.GetColonyInfos().ToList<ColonyInfo>())
			{
				colony.RepairPoints = Kerberos.Sots.Strategy.InhabitedPlanet.Colony.CalcColonyRepairPoints(sim, colony);
				colony.RepairPointsMax = colony.RepairPoints;
				gameDatabase.UpdateColony(colony);
			}
			if ((flags & GameSession.Flags.NoGameSetup) == (GameSession.Flags)0)
				gameSetup.SavePlayerSlots(gameDatabase);
			return sim;
		}

		public static void BuildTeamDiplomacyStates(GameDatabase gamedb)
		{
			List<PlayerInfo> list = gamedb.GetPlayerInfos().Where<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.isStandardPlayer)).ToList<PlayerInfo>();
			foreach (PlayerInfo playerInfo1 in list)
			{
				PlayerInfo pi = playerInfo1;
				if (pi.Team != 0)
				{
					foreach (PlayerInfo playerInfo2 in list.Where<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
				   {
					   if (x.ID != pi.ID)
						   return x.Team == pi.Team;
					   return false;
				   })))
					{
						if (gamedb.GetDiplomacyStateBetweenPlayers(pi.ID, playerInfo2.ID) != DiplomacyState.ALLIED)
						{
							gamedb.UpdateDiplomacyState(pi.ID, playerInfo2.ID, DiplomacyState.ALLIED, DiplomacyInfo.MaxDeplomacyRelations, true);
							DiplomacyInfo diplomacyInfo = gamedb.GetDiplomacyInfo(pi.ID, playerInfo2.ID);
							diplomacyInfo.isEncountered = true;
							gamedb.UpdateDiplomacyInfo(diplomacyInfo);
						}
					}
				}
			}
		}

		public static void AddSuulkas(AssetDatabase assetdb, GameDatabase gamedb)
		{
			DesignInfo[] designInfoArray = new DesignInfo[7];
			for (int index = 0; index < 7; ++index)
			{
				designInfoArray[index] = new DesignInfo();
				designInfoArray[index].PlayerID = 0;
				designInfoArray[index].DesignSections = new DesignSectionInfo[1];
				designInfoArray[index].DesignSections[0] = new DesignSectionInfo()
				{
					DesignInfo = designInfoArray[index]
				};
				switch (index)
				{
					case 0:
						designInfoArray[index].DesignSections[0].FilePath = "factions\\zuul\\sections\\lv_suulka_the_cannibal.section";
						designInfoArray[index].Name = App.Localize("@SECTIONTITLE_LV_SUULKA_THE_CANNIBAL");
						break;
					case 1:
						designInfoArray[index].DesignSections[0].FilePath = "factions\\zuul\\sections\\lv_suulka_the_deaf.section";
						designInfoArray[index].Name = App.Localize("@SECTIONTITLE_LV_SUULKA_THE_DEAF");
						break;
					case 2:
						designInfoArray[index].DesignSections[0].FilePath = "factions\\zuul\\sections\\lv_suulka_the_hidden.section";
						designInfoArray[index].Name = App.Localize("@SECTIONTITLE_LV_SUULKA_THE_HIDDEN");
						break;
					case 3:
						designInfoArray[index].DesignSections[0].FilePath = "factions\\zuul\\sections\\lv_suulka_the_immortal.section";
						designInfoArray[index].Name = App.Localize("@SECTIONTITLE_LV_SUULKA_THE_IMMORTAL");
						break;
					case 4:
						designInfoArray[index].DesignSections[0].FilePath = "factions\\zuul\\sections\\lv_suulka_the_kraken.section";
						designInfoArray[index].Name = App.Localize("@SECTIONTITLE_LV_SUULKA_THE_KRAKEN");
						break;
					case 5:
						designInfoArray[index].DesignSections[0].FilePath = "factions\\zuul\\sections\\lv_suulka_the_shaper.section";
						designInfoArray[index].Name = App.Localize("@SECTIONTITLE_LV_SUULKA_THE_SHAPER");
						break;
					case 6:
						designInfoArray[index].DesignSections[0].FilePath = "factions\\zuul\\sections\\lv_suulka_the_siren.section";
						designInfoArray[index].Name = App.Localize("@SECTIONTITLE_LV_SUULKA_THE_SIREN");
						break;
				}
				if (index != 2)
				{
					int designID = gamedb.InsertDesignByDesignInfo(designInfoArray[index]);
					int shipID = gamedb.InsertShip(0, designID, null, (ShipParams)0, new int?(), 0);
					int admiralID = gamedb.InsertAdmiral(0, new int?(), designInfoArray[index].Name, "suulka", 0.0f, "male", 100f, 100f, 0);
					gamedb.InsertSuulka(new int?(), shipID, admiralID, new int?(), -1);
				}
			}
		}

		public static void AddStratModifiers(GameDatabase gamedb, AssetDatabase assetdb, int playerID)
		{
			foreach (StratModifiers stratModifiers in Enum.GetValues(typeof(StratModifiers)))
				;
			int playerFactionId = gamedb.GetPlayerFactionID(playerID);
			foreach (KeyValuePair<StratModifiers, object> defaultStratModifier in assetdb.DefaultStratModifiers)
			{
				object factionStratModifier = assetdb.GetFactionStratModifier(playerFactionId, defaultStratModifier.Key.ToString());
				object obj = defaultStratModifier.Value;
				if (factionStratModifier != null)
					obj = Convert.ChangeType(factionStratModifier, obj.GetType());
				gamedb.SetStratModifier(defaultStratModifier.Key, playerID, obj);
			}
		}

		public static void UpdateStratModifiers(GameSession game, int playerID, int techDatabaseID)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			AssetDatabase assetDatabase = game.AssetDatabase;
			string techFileId = gameDatabase.GetPlayerTechInfo(playerID, techDatabaseID).TechFileID;
			// ISSUE: reference to a compiler-generated method
			switch (techFileId)
			{
				case "PSI_Telekinesis":
					float stratModifier1 = gameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.IndustrialOutputModifier, playerID, (object)(float)((double)stratModifier1 + (double)assetDatabase.GetTechBonus<float>("PSI_Telekinesis", "industrialoutput")));
					break;
				case "CCC_A.I._Factories":
					gameDatabase.SetStratModifier(StratModifiers.AIProductionBonus, playerID, (object)1f);
					break;
				case "PSI_MechaEmpathy":
					float stratModifier2 = gameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.IndustrialOutputModifier, playerID, (object)(float)((double)stratModifier2 + (double)assetDatabase.GetTechBonus<float>("PSI_MechaEmpathy", "industrialoutput")));
					break;
				case "IND_Hardened_Structures":
					gameDatabase.SetStratModifier(StratModifiers.AllowHardenedStructures, playerID, (object)true);
					break;
				case "POL_Comparitive_Analysis":
					gameDatabase.SetStratModifier(StratModifiers.ComparativeAnalysys, playerID, (object)true);
					break;
				case "POL_Slave_Husbandry":
					float stratModifier3 = gameDatabase.GetStratModifier<float>(StratModifiers.SlaveDeathRateModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.SlaveDeathRateModifier, playerID, (object)(float)((double)stratModifier3 + (double)assetDatabase.GetTechBonus<float>("POL_Slave_Husbandry", "slavedeathrate")));
					break;
				case "IND_Gravity_Control":
					float stratModifier4 = gameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.IndustrialOutputModifier, playerID, (object)(float)((double)stratModifier4 + (double)assetDatabase.GetTechBonus<float>("IND_Gravity_Control", "industrialoutput")));
					break;
				case "CCC_A.I._Virus":
					int[] aiOldColonyOwner1 = gameDatabase.GetAIOldColonyOwner(playerID);
					foreach (int colonyID in aiOldColonyOwner1)
						gameDatabase.RemoveColonyOnPlanet(gameDatabase.GetColonyInfo(colonyID).OrbitalObjectID);
					if (((IEnumerable<int>)aiOldColonyOwner1).Count<int>() > 0)
						gameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_AI_REBELLION_END,
							EventMessage = TurnEventMessage.EM_AI_REBELLION_END,
							PlayerID = playerID,
							TurnNumber = gameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					gameDatabase.SetStratModifier(StratModifiers.AIBenefitBonus, playerID, (object)0);
					gameDatabase.SetStratModifier(StratModifiers.AllowAIRebellion, playerID, (object)false);
					break;
				case "DRV_Standing_Neutrino_Waves":
					gameDatabase.SetStratModifier(StratModifiers.StandingNeutrinoWaves, playerID, (object)true);
					break;
				case "ENG_Modular_Construction":
					float stratModifier5 = gameDatabase.GetStratModifier<float>(StratModifiers.TradeRevenue, playerID);
					gameDatabase.SetStratModifier(StratModifiers.TradeRevenue, playerID, (object)(float)((double)stratModifier5 + (double)assetDatabase.GetTechBonus<float>("ENG_Modular_Construction", "traderevenue")));
					break;
				case "DRV_Warp_Veil":
					float stratModifier6 = gameDatabase.GetStratModifier<float>(StratModifiers.WarpDriveStratSignatureModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.WarpDriveStratSignatureModifier, playerID, (object)(float)((double)stratModifier6 + (double)assetDatabase.GetTechBonus<float>("DRV_Warp_Veil", "warpsignature")));
					break;
				case "DRV_Mass_Induction_Projectors":
					gameDatabase.SetStratModifier(StratModifiers.MassInductionProjectors, playerID, (object)true);
					break;
				case "ENG_Heavy_Platforms":
					float stratModifier7 = gameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.IndustrialOutputModifier, playerID, (object)(float)((double)stratModifier7 + (double)assetDatabase.GetTechBonus<float>("ENG_Heavy_Platforms", "industrialoutput")));
					break;
				case "WAR_MW_Warheads":
					gameDatabase.SetStratModifier(StratModifiers.AllowMirvPlanetaryMissiles, playerID, (object)true);
					break;
				case "DRV_Far_Casting":
					float stratModifier8 = gameDatabase.GetStratModifier<float>(StratModifiers.GateCastDistance, playerID);
					gameDatabase.SetStratModifier(StratModifiers.GateCastDistance, playerID, (object)(float)((double)stratModifier8 + (double)assetDatabase.GetTechBonus<float>("DRV_Far_Casting", "castdistance")));
					break;
				case "CCC_Expert_Systems":
					float stratModifier9 = gameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, playerID);
					float stratModifier10 = gameDatabase.GetStratModifier<float>(StratModifiers.ConstructionCostModifierCR, playerID);
					float stratModifier11 = gameDatabase.GetStratModifier<float>(StratModifiers.ConstructionCostModifierDN, playerID);
					float stratModifier12 = gameDatabase.GetStratModifier<float>(StratModifiers.ConstructionCostModifierLV, playerID);
					gameDatabase.SetStratModifier(StratModifiers.IndustrialOutputModifier, playerID, (object)(float)((double)stratModifier9 + 0.100000001490116));
					gameDatabase.SetStratModifier(StratModifiers.ConstructionCostModifierCR, playerID, (object)(float)((double)stratModifier10 + (double)assetDatabase.GetTechBonus<float>("CCC_Expert_Systems", "crproduction")));
					gameDatabase.SetStratModifier(StratModifiers.ConstructionCostModifierDN, playerID, (object)(float)((double)stratModifier11 + (double)assetDatabase.GetTechBonus<float>("CCC_Expert_Systems", "dnproduction")));
					gameDatabase.SetStratModifier(StratModifiers.ConstructionCostModifierLV, playerID, (object)(float)((double)stratModifier12 + (double)assetDatabase.GetTechBonus<float>("CCC_Expert_Systems", "lvproduction")));
					break;
				case "BIO_Elemental_Nanites":
					float stratModifier13 = gameDatabase.GetStratModifier<float>(StratModifiers.TerraformingModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.TerraformingModifier, playerID, (object)(float)((double)stratModifier13 + (double)assetDatabase.GetTechBonus<float>("BIO_Elemental_Nanites", "terra")));
					break;
				case "IND_Pressure_Polarization":
					int stratModifier14 = gameDatabase.GetStratModifier<int>(StratModifiers.DomeStageModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.DomeStageModifier, playerID, (object)(stratModifier14 + assetDatabase.GetTechBonus<int>("IND_Pressure_Polarization", "domestage")));
					break;
				case "IND_Quantum_Disassociation":
					float stratModifier15 = gameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, playerID);
					float stratModifier16 = gameDatabase.GetStratModifier<float>(StratModifiers.CavernDmodModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.IndustrialOutputModifier, playerID, (object)(float)((double)stratModifier15 + (double)assetDatabase.GetTechBonus<float>("IND_Quantum_Disassociation", "industrialoutput")));
					gameDatabase.SetStratModifier(StratModifiers.CavernDmodModifier, playerID, (object)(float)((double)stratModifier16 + (double)assetDatabase.GetTechBonus<float>("IND_Quantum_Disassociation", "caverndomemod")));
					break;
				case "CCC_Artificial_Intelligence":
					gameDatabase.SetStratModifier(StratModifiers.AIResearchBonus, playerID, (object)1f);
					gameDatabase.SetStratModifier(StratModifiers.AllowAIRebellion, playerID, (object)true);
					break;
				case "DRV_Specter_Camouflage":
					gameDatabase.SetStratModifier(StratModifiers.ImmuneToSpectre, playerID, (object)true);
					break;
				case "CCC_Tunneling_Sensors":
					float stratModifier17 = gameDatabase.GetStratModifier<float>(StratModifiers.NavyStationSensorCloakBonus, playerID);
					float stratModifier18 = gameDatabase.GetStratModifier<float>(StratModifiers.ScienceStationSensorCloakBonus, playerID);
					gameDatabase.SetStratModifier(StratModifiers.NavyStationSensorCloakBonus, playerID, (object)(float)((double)stratModifier17 + (double)assetDatabase.GetTechBonus<float>("CCC_Tunneling_Sensors", "sensorbonusnavy")));
					gameDatabase.SetStratModifier(StratModifiers.ScienceStationSensorCloakBonus, playerID, (object)(float)((double)stratModifier18 + (double)assetDatabase.GetTechBonus<float>("CCC_Tunneling_Sensors", "sensorbonussci")));
					break;
				case "PSI_Empathy":
					float stratModifier19 = gameDatabase.GetStratModifier<float>(StratModifiers.DiplomaticReactionBonus, playerID);
					int stratModifier20 = gameDatabase.GetStratModifier<int>(StratModifiers.MoralBonus, playerID);
					gameDatabase.SetStratModifier(StratModifiers.DiplomaticReactionBonus, playerID, (object)(float)((double)stratModifier19 + (double)assetDatabase.GetTechBonus<float>("PSI_Empathy", "diploreaction")));
					gameDatabase.SetStratModifier(StratModifiers.MoralBonus, playerID, (object)(stratModifier20 + assetDatabase.GetTechBonus<int>("PSI_Empathy", "moralbonus")));
					break;
				case "POL_Xeno-Analysis":
					float stratModifier21 = gameDatabase.GetStratModifier<float>(StratModifiers.DiplomacyPointCostModifier, playerID);
					float stratModifier22 = gameDatabase.GetStratModifier<float>(StratModifiers.NegativeRelationsModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.DiplomacyPointCostModifier, playerID, (object)(float)((double)stratModifier21 + (double)assetDatabase.GetTechBonus<float>("POL_Xeno-Analysis", "diplocost")));
					gameDatabase.SetStratModifier(StratModifiers.NegativeRelationsModifier, playerID, (object)(float)((double)stratModifier22 + (double)assetDatabase.GetTechBonus<float>("POL_Xeno-Analysis", "negativerelations")));
					break;
				case "PSI_Scientific_Prolepsis":
					float stratModifier23 = gameDatabase.GetStratModifier<float>(StratModifiers.TechFeasibilityDeviation, playerID);
					float num1 = stratModifier23 + stratModifier23 * assetDatabase.GetTechBonus<float>("PSI_Scientific_Prolepsis", "techfeasibilitydev");
					gameDatabase.SetStratModifier(StratModifiers.TechFeasibilityDeviation, playerID, (object)num1);
					break;
				case "BIO_Gravitational_Adaptation":
					float stratModifier24 = gameDatabase.GetStratModifier<float>(StratModifiers.PopulationGrowthModifier, playerID);
					float stratModifier25 = gameDatabase.GetStratModifier<float>(StratModifiers.ColonySupportCostModifier, playerID);
					int stratModifier26 = gameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, playerID);
					gameDatabase.SetStratModifier(StratModifiers.PopulationGrowthModifier, playerID, (object)(float)((double)stratModifier24 + (double)assetDatabase.GetTechBonus<float>("BIO_Gravitational_Adaptation", "popgrowth")));
					gameDatabase.SetStratModifier(StratModifiers.ColonySupportCostModifier, playerID, (object)(float)((double)stratModifier25 + (double)assetDatabase.GetTechBonus<float>("BIO_Gravitational_Adaptation", "colonysupportcost")));
					gameDatabase.SetStratModifier(StratModifiers.MaxColonizableHazard, playerID, (object)(stratModifier26 + assetDatabase.GetTechBonus<int>("BIO_Gravitational_Adaptation", "maxcolonizehazard")));
					break;
				case "DRV_Warp_Extension":
					gameDatabase.SetStratModifier(StratModifiers.UseFastestShipForFTLSpeed, playerID, (object)true);
					break;
				case "BIO_Anagathics":
					float stratModifier27 = gameDatabase.GetStratModifier<float>(StratModifiers.AdmiralCareerModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.AdmiralCareerModifier, playerID, (object)(float)((double)stratModifier27 + (double)assetDatabase.GetTechBonus<float>("BIO_Anagathics", "career")));
					break;
				case "POL_Eclipse":
					gameDatabase.SetStratModifier(StratModifiers.AllowEmpireSurrender, playerID, (object)true);
					break;
				case "POL_Slave_Functionaries":
					float stratModifier28 = gameDatabase.GetStratModifier<float>(StratModifiers.SlaveProductionModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.SlaveProductionModifier, playerID, (object)(float)((double)stratModifier28 + (double)assetDatabase.GetTechBonus<float>("POL_Slave_Functionaries", "slaveproduction")));
					break;
				case "POL_Annex":
					gameDatabase.SetStratModifier(StratModifiers.AllowProvinceSurrender, playerID, (object)true);
					break;
				case "POL_Proliferate":
					gameDatabase.SetStratModifier(StratModifiers.AllowAlienImmigration, playerID, (object)true);
					break;
				case "IND_Arcologies":
					gameDatabase.SetStratModifier(StratModifiers.AdditionalMaxCivilianPopulation, playerID, (object)400);
					gameDatabase.SetStratModifier(StratModifiers.AdditionalMaxImperialPopulation, playerID, (object)100);
					break;
				case "IND_Tractor_Beams":
					float stratModifier29 = gameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.IndustrialOutputModifier, playerID, (object)(float)((double)stratModifier29 + (double)assetDatabase.GetTechBonus<float>("IND_Tractor_Beams", "industrialoutput")));
					break;
				case "ENG_Virtual_Engineering":
					gameDatabase.SetStratModifier(StratModifiers.ShowPrototypeDesignAttributes, playerID, (object)true);
					break;
				case "CYB_InFldManip":
					float stratModifier30 = gameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.IndustrialOutputModifier, playerID, (object)(float)((double)stratModifier30 + (double)assetDatabase.GetTechBonus<float>("CYB_InFldManip", "industrialoutput")));
					break;
				case "DRV_Phase_Dislocation":
					int stratModifier31 = gameDatabase.GetStratModifier<int>(StratModifiers.PhaseDislocationARBonus, playerID);
					gameDatabase.SetStratModifier(StratModifiers.PhaseDislocationARBonus, playerID, (object)(float)((double)stratModifier31 + (double)assetDatabase.GetTechBonus<float>("DRV_Phase_Dislocation", "permarmorlayers")));
					break;
				case "ENG_Orbital_Drydocks":
					float stratModifier32 = gameDatabase.GetStratModifier<float>(StratModifiers.ConstructionCostModifierCR, playerID);
					float stratModifier33 = gameDatabase.GetStratModifier<float>(StratModifiers.ConstructionCostModifierDN, playerID);
					gameDatabase.SetStratModifier(StratModifiers.ConstructionCostModifierCR, playerID, (object)(float)((double)stratModifier32 + (double)assetDatabase.GetTechBonus<float>("ENG_Orbital_Drydocks", "crproduction")));
					gameDatabase.SetStratModifier(StratModifiers.ConstructionCostModifierDN, playerID, (object)(float)((double)stratModifier33 + (double)assetDatabase.GetTechBonus<float>("ENG_Orbital_Drydocks", "dnproduction")));
					break;
				case "CCC_A.I._Slaves":
					int[] aiOldColonyOwner2 = gameDatabase.GetAIOldColonyOwner(playerID);
					foreach (int colonyID in aiOldColonyOwner2)
					{
						ColonyInfo colonyInfo = gameDatabase.GetColonyInfo(colonyID);
						colonyInfo.PlayerID = playerID;
						gameDatabase.UpdateColony(colonyInfo);
					}
					if (((IEnumerable<int>)aiOldColonyOwner2).Count<int>() > 0)
						gameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_AI_REBELLION_END,
							EventMessage = TurnEventMessage.EM_AI_REBELLION_END,
							PlayerID = playerID,
							TurnNumber = gameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					gameDatabase.SetStratModifier(StratModifiers.AllowAIRebellion, playerID, (object)false);
					break;
				case "POL_Incorporate":
					gameDatabase.SetStratModifier(StratModifiers.AllowAlienPopulations, playerID, (object)true);
					gameDatabase.SetStratModifier(StratModifiers.AllowIncorporate, playerID, (object)true);
					break;
				case "DRV_Casting":
					float stratModifier34 = gameDatabase.GetStratModifier<float>(StratModifiers.GateCastDistance, playerID);
					gameDatabase.SetStratModifier(StratModifiers.GateCastDistance, playerID, (object)(float)((double)stratModifier34 + (double)assetDatabase.GetTechBonus<float>("DRV_Casting", "castdistance")));
					break;
				case "POL_Paramilitary_Training":
					int stratModifier35 = gameDatabase.GetStratModifier<int>(StratModifiers.PoliceMoralBonus, playerID);
					gameDatabase.SetStratModifier(StratModifiers.PoliceMoralBonus, playerID, (object)(stratModifier35 + assetDatabase.GetTechBonus<int>("POL_Paramilitary_Training", "policemoral")));
					gameDatabase.SetStratModifier(StratModifiers.AllowPoliceInCombat, playerID, (object)true);
					break;
				case "BIO_Environmental_Tailoring":
					float stratModifier36 = gameDatabase.GetStratModifier<float>(StratModifiers.TerraformingModifier, playerID);
					float stratModifier37 = gameDatabase.GetStratModifier<float>(StratModifiers.BiosphereDestructionModifier, playerID);
					float stratModifier38 = gameDatabase.GetStratModifier<float>(StratModifiers.PopulationGrowthModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.PopulationGrowthModifier, playerID, (object)(float)((double)stratModifier38 + (double)assetDatabase.GetTechBonus<float>("BIO_Environmental_Tailoring", "popgrowth")));
					gameDatabase.SetStratModifier(StratModifiers.TerraformingModifier, playerID, (object)(float)((double)stratModifier36 + (double)assetDatabase.GetTechBonus<float>("BIO_Environmental_Tailoring", "terra")));
					gameDatabase.SetStratModifier(StratModifiers.BiosphereDestructionModifier, playerID, (object)(float)((double)stratModifier37 + (double)assetDatabase.GetTechBonus<float>("BIO_Environmental_Tailoring", "biosphere")));
					break;
				case "PSI_Telepathy":
					float stratModifier39 = gameDatabase.GetStratModifier<float>(StratModifiers.PsiResearchModifier, playerID);
					float stratModifier40 = gameDatabase.GetStratModifier<float>(StratModifiers.ResearchModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.PsiResearchModifier, playerID, (object)(float)((double)stratModifier39 + (double)assetDatabase.GetTechBonus<float>("PSI_Telepathy", "psiresearch")));
					gameDatabase.SetStratModifier(StratModifiers.ResearchModifier, playerID, (object)(float)((double)stratModifier40 + (double)assetDatabase.GetTechBonus<float>("PSI_Telepathy", "research")));
					break;
				case "CCC_A.I._Administration":
					gameDatabase.SetStratModifier(StratModifiers.AIRevenueBonus, playerID, (object)1f);
					break;
				case "ENG_Turret_Installations":
					gameDatabase.SetStratModifier(StratModifiers.AllowPlanetBeam, playerID, (object)true);
					break;
				case "CCC_Deep_Survey_Sensors":
					float stratModifier41 = gameDatabase.GetStratModifier<float>(StratModifiers.SurveyTimeModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.SurveyTimeModifier, playerID, (object)(float)((double)stratModifier41 + (double)assetDatabase.GetTechBonus<float>("CCC_Deep_Survey_Sensors", "survey")));
					break;
				case "ENG_Rapid_Prototyping":
					float stratModifier42 = gameDatabase.GetStratModifier<float>(StratModifiers.PrototypeConstructionCostModifierCR, playerID);
					float stratModifier43 = gameDatabase.GetStratModifier<float>(StratModifiers.PrototypeConstructionCostModifierDN, playerID);
					float stratModifier44 = gameDatabase.GetStratModifier<float>(StratModifiers.PrototypeConstructionCostModifierLV, playerID);
					float stratModifier45 = gameDatabase.GetStratModifier<float>(StratModifiers.PrototypeConstructionCostModifierPF, playerID);
					float stratModifier46 = gameDatabase.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierCR, playerID);
					float stratModifier47 = gameDatabase.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierDN, playerID);
					float stratModifier48 = gameDatabase.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierLV, playerID);
					float stratModifier49 = gameDatabase.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierPF, playerID);
					float num2 = 1f + assetDatabase.GetTechBonus<float>("ENG_Rapid_Prototyping", "protocostscale");
					gameDatabase.SetStratModifier(StratModifiers.PrototypeConstructionCostModifierPF, playerID, (object)(float)((double)stratModifier45 * (double)num2));
					gameDatabase.SetStratModifier(StratModifiers.PrototypeConstructionCostModifierCR, playerID, (object)(float)((double)stratModifier42 * (double)num2));
					gameDatabase.SetStratModifier(StratModifiers.PrototypeConstructionCostModifierDN, playerID, (object)(float)((double)stratModifier43 * (double)num2));
					gameDatabase.SetStratModifier(StratModifiers.PrototypeConstructionCostModifierLV, playerID, (object)(float)((double)stratModifier44 * (double)num2));
					gameDatabase.SetStratModifier(StratModifiers.PrototypeSavingsCostModifierPF, playerID, (object)(float)((double)stratModifier49 * (double)num2));
					gameDatabase.SetStratModifier(StratModifiers.PrototypeSavingsCostModifierCR, playerID, (object)(float)((double)stratModifier46 * (double)num2));
					gameDatabase.SetStratModifier(StratModifiers.PrototypeSavingsCostModifierDN, playerID, (object)(float)((double)stratModifier47 * (double)num2));
					gameDatabase.SetStratModifier(StratModifiers.PrototypeSavingsCostModifierLV, playerID, (object)(float)((double)stratModifier48 * (double)num2));
					break;
				case "CCC_Convoy_Systems":
					float stratModifier50 = gameDatabase.GetStratModifier<float>(StratModifiers.ChanceOfPirates, playerID);
					float num3 = stratModifier50 + stratModifier50 * assetDatabase.GetTechBonus<float>("CCC_Convoy_Systems", "piratechance");
					gameDatabase.SetStratModifier(StratModifiers.ChanceOfPirates, playerID, (object)num3);
					break;
				case "ENG_Deep_Space_Construction":
					gameDatabase.SetStratModifier(StratModifiers.AllowDeepSpaceConstruction, playerID, (object)true);
					break;
				case "ENG_Enhanced_Design":
					int stratModifier51 = gameDatabase.GetStratModifier<int>(StratModifiers.GoodDesignAttributePercent, playerID);
					int stratModifier52 = gameDatabase.GetStratModifier<int>(StratModifiers.BadDesignAttributePercent, playerID);
					gameDatabase.SetStratModifier(StratModifiers.GoodDesignAttributePercent, playerID, (object)(stratModifier51 + 25));
					gameDatabase.SetStratModifier(StratModifiers.BadDesignAttributePercent, playerID, (object)(stratModifier52 - 25));
					break;
				case "PSI_Micro-Telekinesis":
					float stratModifier53 = gameDatabase.GetStratModifier<float>(StratModifiers.C3ResearchModifier, playerID);
					float stratModifier54 = gameDatabase.GetStratModifier<float>(StratModifiers.ResearchModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.C3ResearchModifier, playerID, (object)(float)((double)stratModifier53 + (double)assetDatabase.GetTechBonus<float>("PSI_Micro-Telekinesis", "c3research")));
					gameDatabase.SetStratModifier(StratModifiers.ResearchModifier, playerID, (object)(float)((double)stratModifier54 + (double)assetDatabase.GetTechBonus<float>("PSI_Micro-Telekinesis", "research")));
					break;
				case "PSI_Farsense":
					gameDatabase.SetStratModifier(StratModifiers.AllowFarSense, playerID, (object)true);
					break;
				case "POL_Trade_Enclaves":
					gameDatabase.SetStratModifier(StratModifiers.AllowTradeEnclave, playerID, (object)true);
					break;
				case "POL_Protectorate":
					gameDatabase.SetStratModifier(StratModifiers.AllowProtectorate, playerID, (object)true);
					break;
				case "IND_Mega-Strip_Mining":
					float stratModifier55 = gameDatabase.GetStratModifier<float>(StratModifiers.StripMiningMaximum, playerID);
					gameDatabase.SetStratModifier(StratModifiers.StripMiningMaximum, playerID, (object)(float)((double)stratModifier55 * (double)assetDatabase.GetTechBonus<float>("IND_Mega-Strip_Mining", "miningmaxscale")));
					break;
				case "POL_FTL_Economics":
					App.InitializeTrade(game, gameDatabase, playerID);
					break;
				case "IND_Zero-G_Deconstruction":
					float stratModifier56 = gameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, playerID);
					float stratModifier57 = gameDatabase.GetStratModifier<float>(StratModifiers.ScrapShipModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.IndustrialOutputModifier, playerID, (object)(float)((double)stratModifier56 + (double)assetDatabase.GetTechBonus<float>("IND_Zero-G_Deconstruction", "industrialoutput")));
					gameDatabase.SetStratModifier(StratModifiers.ScrapShipModifier, playerID, (object)(float)((double)stratModifier57 + (double)assetDatabase.GetTechBonus<float>("IND_Zero-G_Deconstruction", "scrapship")));
					break;
				case "POL_OmbudSapiens":
					gameDatabase.SetStratModifier(StratModifiers.AllowOneFightRebellionEnding, playerID, (object)true);
					break;
				case "POL_Accomodate":
					float stratModifier58 = gameDatabase.GetStratModifier<float>(StratModifiers.AlienCivilianTaxRate, playerID);
					gameDatabase.SetStratModifier(StratModifiers.AlienCivilianTaxRate, playerID, (object)(float)((double)stratModifier58 + (double)assetDatabase.GetTechBonus<float>("POL_Accomodate", "aliencivtaxrate")));
					gameDatabase.SetStratModifier(StratModifiers.AllowIdealAlienGrowthRate, playerID, (object)true);
					break;
				case "POL_Enhanced_Jurisdiction":
					int stratModifier59 = gameDatabase.GetStratModifier<int>(StratModifiers.MaxProvincePlanets, playerID);
					float stratModifier60 = gameDatabase.GetStratModifier<float>(StratModifiers.MaxProvincePlanetRange, playerID);
					gameDatabase.SetStratModifier(StratModifiers.MaxProvincePlanets, playerID, (object)(stratModifier59 + assetDatabase.GetTechBonus<int>("POL_Enhanced_Jurisdiction", "maxplanets")));
					gameDatabase.SetStratModifier(StratModifiers.MaxProvincePlanetRange, playerID, (object)(float)((double)stratModifier60 + (double)assetDatabase.GetTechBonus<float>("POL_Enhanced_Jurisdiction", "maxrange")));
					break;
				case "CCC_A.I._Autonomy":
					float stratModifier61 = gameDatabase.GetStratModifier<float>(StratModifiers.AIBenefitBonus, playerID);
					gameDatabase.SetStratModifier(StratModifiers.AIBenefitBonus, playerID, (object)(float)((double)stratModifier61 + (double)assetDatabase.GetTechBonus<float>("CCC_A.I._Autonomy", "aibenefits")));
					gameDatabase.SetStratModifier(StratModifiers.AllowAIRebellion, playerID, (object)false);
					break;
				case "BIO_Terraforming_Bacteria":
					float stratModifier62 = gameDatabase.GetStratModifier<float>(StratModifiers.TerraformingModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.TerraformingModifier, playerID, (object)(float)((double)stratModifier62 + (double)assetDatabase.GetTechBonus<float>("BIO_Terraforming_Bacteria", "terra")));
					break;
				case "CCC_Commerce_Raiding":
					gameDatabase.SetStratModifier(StratModifiers.AllowPrivateerMission, playerID, (object)true);
					break;
				case "POL_Occupy":
					gameDatabase.SetStratModifier(StratModifiers.AllowWorldSurrender, playerID, (object)true);
					break;
				case "DRV_Grav_Synergy":
					float num4 = gameDatabase.GetStratModifier<float>(StratModifiers.MaxFlockBonusMod, playerID) + (assetDatabase.GetTechBonus<float>("DRV_Grav_Synergy", "flockbonusscale") - 1f);
					gameDatabase.SetStratModifier(StratModifiers.MaxFlockBonusMod, playerID, (object)num4);
					break;
				case "POL_Cosmic_Bureaucracies":
					int stratModifier63 = gameDatabase.GetStratModifier<int>(StratModifiers.MaxProvincePlanets, playerID);
					float stratModifier64 = gameDatabase.GetStratModifier<float>(StratModifiers.MaxProvincePlanetRange, playerID);
					gameDatabase.SetStratModifier(StratModifiers.MaxProvincePlanets, playerID, (object)(stratModifier63 + assetDatabase.GetTechBonus<int>("POL_Cosmic_Bureaucracies", "maxplanets")));
					gameDatabase.SetStratModifier(StratModifiers.MaxProvincePlanetRange, playerID, (object)(float)((double)stratModifier64 + (double)assetDatabase.GetTechBonus<float>("POL_Cosmic_Bureaucracies", "maxrange")));
					break;
				case "POL_Super_Worlds":
					gameDatabase.SetStratModifier(StratModifiers.AllowSuperWorlds, playerID, (object)true);
					break;
				case "BIO_Biosphere_Preservation":
					float stratModifier65 = gameDatabase.GetStratModifier<float>(StratModifiers.BiosphereDestructionModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.BiosphereDestructionModifier, playerID, (object)(float)((double)stratModifier65 + (double)assetDatabase.GetTechBonus<float>("BIO_Biosphere_Preservation", "biosphere")));
					break;
				case "PSI_Doomsayers":
					gameDatabase.SetStratModifier(StratModifiers.GrandMenaceWarningTime, playerID, (object)5);
					float stratModifier66 = gameDatabase.GetStratModifier<float>(StratModifiers.RandomEncounterWarningPercent, playerID);
					gameDatabase.SetStratModifier(StratModifiers.RandomEncounterWarningPercent, playerID, (object)(float)((double)stratModifier66 + (double)assetDatabase.GetTechBonus<float>("PSI_Doomsayers", "randomnotification")));
					break;
				case "PSI_Precognition":
					float stratModifier67 = gameDatabase.GetStratModifier<float>(StratModifiers.AdmiralReactionModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.AdmiralReactionModifier, playerID, (object)(float)((double)stratModifier67 + (double)assetDatabase.GetTechBonus<float>("PSI_Precognition", "admiralreaction")));
					break;
				case "IND_Atmospheric_Processors":
					float stratModifier68 = gameDatabase.GetStratModifier<float>(StratModifiers.TerraformingModifier, playerID);
					float stratModifier69 = gameDatabase.GetStratModifier<float>(StratModifiers.BiosphereDestructionModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.TerraformingModifier, playerID, (object)(float)((double)stratModifier68 + (double)assetDatabase.GetTechBonus<float>("IND_Atmospheric_Processors", "terra")));
					gameDatabase.SetStratModifier(StratModifiers.BiosphereDestructionModifier, playerID, (object)(float)((double)stratModifier69 + (double)assetDatabase.GetTechBonus<float>("IND_Atmospheric_Processors", "biosphere")));
					break;
				case "POL_Disinformation_Nets":
					float stratModifier70 = gameDatabase.GetStratModifier<float>(StratModifiers.EnemyIntelSuccessModifier, playerID);
					float stratModifier71 = gameDatabase.GetStratModifier<float>(StratModifiers.EnemyOperationsSuccessModifier, playerID);
					float stratModifier72 = gameDatabase.GetStratModifier<float>(StratModifiers.CounterIntelSuccessModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.EnemyIntelSuccessModifier, playerID, (object)(float)((double)stratModifier70 + (double)assetDatabase.GetTechBonus<float>("POL_Disinformation_Nets", "enemyintel")));
					gameDatabase.SetStratModifier(StratModifiers.EnemyOperationsSuccessModifier, playerID, (object)(float)((double)stratModifier71 + (double)assetDatabase.GetTechBonus<float>("POL_Disinformation_Nets", "enemyoperations")));
					gameDatabase.SetStratModifier(StratModifiers.CounterIntelSuccessModifier, playerID, (object)(float)((double)stratModifier72 + (double)assetDatabase.GetTechBonus<float>("POL_Disinformation_Nets", "counterintel")));
					break;
				case "IND_Materials_Applications":
					float stratModifier73 = gameDatabase.GetStratModifier<float>(StratModifiers.ConstructionCostModifierCR, playerID);
					float stratModifier74 = gameDatabase.GetStratModifier<float>(StratModifiers.ConstructionCostModifierDN, playerID);
					float stratModifier75 = gameDatabase.GetStratModifier<float>(StratModifiers.ConstructionCostModifierLV, playerID);
					float stratModifier76 = gameDatabase.GetStratModifier<float>(StratModifiers.ConstructionCostModifierSN, playerID);
					gameDatabase.SetStratModifier(StratModifiers.ConstructionCostModifierCR, playerID, (object)(float)((double)stratModifier73 + (double)assetDatabase.GetTechBonus<float>("IND_Materials_Applications", "crproduction")));
					gameDatabase.SetStratModifier(StratModifiers.ConstructionCostModifierDN, playerID, (object)(float)((double)stratModifier74 + (double)assetDatabase.GetTechBonus<float>("IND_Materials_Applications", "dnproduction")));
					gameDatabase.SetStratModifier(StratModifiers.ConstructionCostModifierLV, playerID, (object)(float)((double)stratModifier75 + (double)assetDatabase.GetTechBonus<float>("IND_Materials_Applications", "lvproduction")));
					gameDatabase.SetStratModifier(StratModifiers.ConstructionCostModifierLV, playerID, (object)(float)((double)stratModifier76 + (double)assetDatabase.GetTechBonus<float>("IND_Materials_Applications", "snproduction")));
					break;
				case "PSI_Lesser_Glamour":
					float stratModifier77 = gameDatabase.GetStratModifier<float>(StratModifiers.DiplomaticOfferingModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.DiplomaticOfferingModifier, playerID, (object)(float)((double)stratModifier77 + (double)assetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering")));
					break;
				case "IND_Vacuum_Preservation":
					float stratModifier78 = gameDatabase.GetStratModifier<float>(StratModifiers.ShipSupplyModifier, playerID);
					float stratModifier79 = gameDatabase.GetStratModifier<float>(StratModifiers.WarehouseCapacityModifier, playerID);
					gameDatabase.SetStratModifier(StratModifiers.ShipSupplyModifier, playerID, (object)(float)((double)stratModifier78 + (double)assetDatabase.GetTechBonus<float>("IND_Vacuum_Preservation", "shipsupply")));
					gameDatabase.SetStratModifier(StratModifiers.WarehouseCapacityModifier, playerID, (object)(float)((double)stratModifier79 + (double)assetDatabase.GetTechBonus<float>("IND_Vacuum_Preservation", "warehousecapacity")));
					break;
			}
		}

		private static void InitializeTrade(GameSession game, GameDatabase gamedb, int playerID)
		{
			gamedb.SetStratModifier(StratModifiers.EnableTrade, playerID, (object)true);
			game.CheckForNewEquipment(playerID);
			game.AvailableShipSectionsChanged();
			DesignInfo design = DesignLab.DesignShip(game, ShipClass.Cruiser, ShipRole.FREIGHTER, WeaponRole.STAND_OFF, playerID);
			design.Name = App.Localize("@DEFAULT_SHIPNAME_FREIGHTER");
			design.isPrototyped = true;
			game.GameDatabase.InsertDesignByDesignInfo(design);
		}

		private void ValidFleets(GameSession app, PendingCombat pendingCombat)
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>((IDictionary<int, int>)pendingCombat.SelectedPlayerFleets);
			foreach (KeyValuePair<int, int> selectedPlayerFleet in pendingCombat.SelectedPlayerFleets)
			{
				FleetInfo fleetInfo1 = app.GameDatabase.GetFleetInfo(selectedPlayerFleet.Value);
				List<ShipInfo> list1 = app.GameDatabase.GetShipInfoByFleetID(selectedPlayerFleet.Value, false).ToList<ShipInfo>();
				bool flag1 = false;
				if (fleetInfo1 != null)
				{
					MissionInfo missionByFleetId = app.GameDatabase.GetMissionByFleetID(fleetInfo1.ID);
					if (missionByFleetId != null && missionByFleetId.StartingSystem != 0 && (missionByFleetId.Type == MissionType.RETREAT && missionByFleetId.StartingSystem == pendingCombat.SystemID))
						flag1 = true;
				}
				if (fleetInfo1 == null || list1.Count == 0 || flag1)
				{
					foreach (int fleetId in pendingCombat.FleetIDs)
					{
						FleetInfo fleetInfo2 = app.GameDatabase.GetFleetInfo(fleetId);
						List<ShipInfo> list2 = app.GameDatabase.GetShipInfoByFleetID(fleetId, false).ToList<ShipInfo>();
						bool flag2 = false;
						if (fleetInfo2 != null)
						{
							MissionInfo missionByFleetId = app.GameDatabase.GetMissionByFleetID(fleetInfo2.ID);
							if (missionByFleetId != null && missionByFleetId.StartingSystem != 0 && (missionByFleetId.Type == MissionType.RETREAT && missionByFleetId.StartingSystem == pendingCombat.SystemID))
								flag2 = true;
						}
						if (fleetInfo2 != null && list2.Count != 0 && (!fleetInfo2.IsGateFleet && !fleetInfo2.IsAcceleratorFleet) && (!flag2 && fleetInfo2.PlayerID == selectedPlayerFleet.Key))
						{
							dictionary[selectedPlayerFleet.Key] = fleetInfo2.ID;
							break;
						}
					}
				}
			}
			pendingCombat.SelectedPlayerFleets = dictionary;
		}

		public void LaunchCombat(
		  GameSession app,
		  PendingCombat pendingCombat,
		  bool testing,
		  bool sim,
		  bool authority)
		{
			this.ValidFleets(app, pendingCombat);
			this.SwitchGameStateViaLoadingScreen((Action)null, (LoadingFinishedDelegate)null, sim ? (GameState)this.GetGameState<SimCombatState>() : (GameState)this.GetGameState<CombatState>(), (object)pendingCombat, null, (object)testing, (object)authority);
		}

		private void AddGameStates()
		{
			this._gameStateMachine.Add((GameState)new TestAssetsState(this));
			this._gameStateMachine.Add((GameState)new TestAnimationState(this));
			this._gameStateMachine.Add((GameState)new TestShipsState(this));
			this._gameStateMachine.Add((GameState)new TestLoadCombatState(this));
			this._gameStateMachine.Add((GameState)new TestPhysicsState(this));
			this._gameStateMachine.Add((GameState)new TestUIState(this));
			this._gameStateMachine.Add((GameState)new TestPlanetState(this));
			this._gameStateMachine.Add((GameState)new CologneShipsState(this));
			this._gameStateMachine.Add((GameState)new ResearchScreenState(this));
			this._gameStateMachine.Add((GameState)new DesignScreenState(this));
			this._gameStateMachine.Add((GameState)new DiplomacyScreenState(this));
			this._gameStateMachine.Add((GameState)new BuildScreenState(this));
			this._gameStateMachine.Add((GameState)new SplashState(this));
			this._gameStateMachine.Add((GameState)new MainMenuState(this));
			this._gameStateMachine.Add((GameState)new StarMapState(this));
			this._gameStateMachine.Add((GameState)new CombatState(this));
			this._gameStateMachine.Add((GameState)new SotspediaState(this));
			this._gameStateMachine.Add((GameState)new StarSystemState(this));
			this._gameStateMachine.Add((GameState)new GameSetupState(this));
			this._gameStateMachine.Add((GameState)new LoadGameState(this));
			this._gameStateMachine.Add((GameState)new StarMapLobbyState(this));
			this._gameStateMachine.Add((GameState)new ProfilesState(this));
			this._gameStateMachine.Add((GameState)new CinematicsState(this));
			this._gameStateMachine.Add((GameState)new EmpireSummaryState(this));
			this._gameStateMachine.Add((GameState)new TestNetworkState(this));
			this._gameStateMachine.Add((GameState)new TestCombatState(this));
			this._gameStateMachine.Add((GameState)new TestScratchCombatState(this));
			this._gameStateMachine.Add((GameState)new LoadingScreenState(this));
			this._gameStateMachine.Add((GameState)new StationPlacementState(this));
			this._gameStateMachine.Add((GameState)new PlanetManagerState(this));
			this._gameStateMachine.Add((GameState)new FleetManagerState(this));
			this._gameStateMachine.Add((GameState)new DefenseManagerState(this));
			this._gameStateMachine.Add((GameState)new MoviePlayerState(this));
			this._gameStateMachine.Add((GameState)new RiderManagerState(this));
			this._gameStateMachine.Add((GameState)new SimCombatState(this));
			this._gameStateMachine.Add((GameState)new ComparativeAnalysysState(this));
		}

		internal void Initialize()
		{
			if (this._assetDatabase != null)
				throw new InvalidOperationException("Cannot initialize more than once.");
			this._assetDatabase = new AssetDatabase(this);
			this.AddMaterialDictionaries(this._assetDatabase.MaterialDictionaries);
			this.AddCritHitChances(this._assetDatabase.CriticalHitChances);
			StarSystemVars.LoadXml("data\\StarSystemVars.xml");
			this._gameSetup = new GameSetup(this);
			this.ResetGameSetup();
			this.UI.Send((object)"InitializeChatWidget");
			if (this._steam.IsAvailable)
				App.Log.Trace("Steam initialized app ID: " + (object)this._steam.GetGameID(), "steam");
			else
				App.Log.Warn("Steam not available.", "steam");
			this._steamHelper = new SteamHelper(this._steam);
			this.SteamHelper.DoAchievement(AchievementType.SOTS2_WELCOME);
			try
			{
				if (!string.IsNullOrEmpty(this._consoleScriptFileName))
					ConsoleCommandParse.Evaluate(this, File.ReadLines(this._consoleScriptFileName));
			}
			finally
			{
				this._initialized = true;
			}
			SteamDLCHelpers.LogAvailableDLC(this);
			this.PostSpeechSubtitles(this._gameSettings.SpeechSubtitles);
		}

		internal bool IsInitialized()
		{
			if (this._initialized && this._materialsReady)
				return this._receivedDirectoryInfo;
			return false;
		}

		public bool CanEndTurn()
		{
			return true;
		}

		public void EndTurn()
		{
			if (!this.game.EndTurn(this.LocalPlayer.ID))
				return;
			this.game.ProcessEndTurn();
		}

		private void Exit()
		{
			this._gameStateMachine.Exit();
			this._assetDatabase.Dispose();
		}

		private void LockFilesInFolder(string dir)
		{
			foreach (FileInfo file in new DirectoryInfo(dir).GetFiles())
			{
				if (!App.locks.Any<NonClosingStreamWrapper>((Func<NonClosingStreamWrapper, bool>)(x => ((FileStream)x.BaseStream).Name == dir)))
				{
					NonClosingStreamWrapper closingStreamWrapper = new NonClosingStreamWrapper((Stream)new FileStream(file.FullName, FileMode.Open, FileAccess.ReadWrite));
					App.locks.Add(closingStreamWrapper);
				}
			}
		}

		public static Stream GetStreamForFile(string fullFilepath)
		{
			string str1 = fullFilepath.Substring(fullFilepath.LastIndexOf('\\') + 1);
			NonClosingStreamWrapper closingStreamWrapper1 = (NonClosingStreamWrapper)null;
			foreach (NonClosingStreamWrapper closingStreamWrapper2 in App.locks)
			{
				string name = ((FileStream)closingStreamWrapper2.BaseStream).Name;
				string str2 = name.Substring(name.LastIndexOf('\\') + 1);
				if (str1 == str2)
				{
					closingStreamWrapper1 = closingStreamWrapper2;
					break;
				}
			}
			closingStreamWrapper1?.Close();
			return (Stream)closingStreamWrapper1;
		}

		public static void LockFileStream(string fullFilepath)
		{
			if (App.GetStreamForFile(fullFilepath) != null)
				return;
			NonClosingStreamWrapper closingStreamWrapper = new NonClosingStreamWrapper((Stream)new FileStream(fullFilepath, FileMode.Open, FileAccess.ReadWrite));
			App.locks.Add(closingStreamWrapper);
		}

		public static void UnLockFileStream(string fullFilepath)
		{
			string str1 = fullFilepath.Substring(fullFilepath.LastIndexOf('\\') + 1);
			NonClosingStreamWrapper closingStreamWrapper1 = (NonClosingStreamWrapper)null;
			foreach (NonClosingStreamWrapper closingStreamWrapper2 in App.locks)
			{
				string name = ((FileStream)closingStreamWrapper2.BaseStream).Name;
				string str2 = name.Substring(name.LastIndexOf('\\') + 1);
				if (str1 == str2)
				{
					closingStreamWrapper1 = closingStreamWrapper2;
					break;
				}
			}
			if (closingStreamWrapper1 == null)
				return;
			closingStreamWrapper1.CloseContainer();
			App.locks.Remove(closingStreamWrapper1);
		}

		private static void UnlockAllStreams()
		{
			foreach (NonClosingStreamWrapper closingStreamWrapper in App.locks)
				closingStreamWrapper.CloseContainer();
			App.locks.Clear();
		}

		private void ProcessEngineMessage(ScriptMessageReader mr)
		{
			InteropMessageID interopMessageId = (InteropMessageID)mr.ReadInteger();
			switch (interopMessageId)
			{
				case InteropMessageID.IMID_SCRIPT_DIRECTORIES:
					this._profileDir = mr.ReadString();
					this._baseSaveDir = mr.ReadString();
					this._cacheDir = mr.ReadString();
					this._settingsDir = mr.ReadString();
					this._receivedDirectoryInfo = true;
					this.LockFilesInFolder(this._profileDir);
					this.LockFilesInFolder(this._settingsDir);
					Profile.SetProfileDirectory(this._profileDir);
					this._gameSettings = new Settings(this._settingsDir);
					this._gameSettings.Commit(this);
					HotKeyManager.SetHotkeyProfileDirectory(this._profileDir);
					this.HotKeyManager.LoadProfile("~Default", false);
					string lastProfile = this._gameSettings.LastProfile;
					if (lastProfile != null)
					{
						this._userProfile.LoadProfile(lastProfile, false);
						if (this._userProfile.Loaded)
							this._profileSelected = true;
					}
					else
					{
						List<Profile> availableProfiles = Profile.GetAvailableProfiles();
						if (availableProfiles.Count != 0)
						{
							this._userProfile = availableProfiles.First<Profile>();
							this._gameSettings.LastProfile = this._userProfile.ProfileName;
							this._profileSelected = true;
						}
					}
					if (!this._profileSelected)
						break;
					if (!HotKeyManager.GetAvailableProfiles().Contains(this._userProfile.ProfileName))
						this._hotkeyManager.CreateProfile(this._userProfile.ProfileName);
					this._hotkeyManager.LoadProfile(this._userProfile.ProfileName, false);
					break;
				case InteropMessageID.IMID_SCRIPT_DIALOG:
					this.UI.HandleDialogMessage(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_MATERIALS_READY:
					this._materialsReady = true;
					break;
				case InteropMessageID.IMID_SCRIPT_SET_PAUSE_STATE:
				case InteropMessageID.IMID_SCRIPT_SET_COMBAT_ACTIVE:
				case InteropMessageID.IMID_SCRIPT_PLAYER_DIPLO_CHANGED:
				case InteropMessageID.IMID_SCRIPT_ZONE_OWNER_CHANGED:
				case InteropMessageID.IMID_SCRIPT_OBJECT_ADD:
				case InteropMessageID.IMID_SCRIPT_OBJECTS_ADD:
				case InteropMessageID.IMID_SCRIPT_OBJECT_RELEASE:
				case InteropMessageID.IMID_SCRIPT_OBJECTS_RELEASE:
				case InteropMessageID.IMID_SCRIPT_MOVE_ORDER:
				case InteropMessageID.IMID_SCRIPT_COMBAT_ENDED:
				case InteropMessageID.IMID_SCRIPT_START_SENDINGDATA:
				case InteropMessageID.IMID_SCRIPT_COMBAT_DATA_SHIP:
				case InteropMessageID.IMID_SCRIPT_COMBAT_DATA_PLANET:
				case InteropMessageID.IMID_SCRIPT_COMBAT_DATA_STAR:
				case InteropMessageID.IMID_SCRIPT_COMBAT_DESTROYED_SHIPS:
				case InteropMessageID.IMID_SCRIPT_COMBAT_CAPTURED_SHIPS:
				case InteropMessageID.IMID_SCRIPT_COMBAT_WEAPON_DAMAGE_STATS:
				case InteropMessageID.IMID_SCRIPT_COMBAT_ZONE_STATES:
				case InteropMessageID.IMID_SCRIPT_END_SENDINGDATA:
				case InteropMessageID.IMID_SCRIPT_END_DELAYCOMPLETE:
				case InteropMessageID.IMID_SCRIPT_SYNC_FLEET_POSITIONS:
				case InteropMessageID.IMID_SCRIPT_SYNC_DEFENSE_POSITIONS:
				case InteropMessageID.IMID_SCRIPT_SYNC_DEFENSEBOAT_DATA:
					this._gameStateMachine.OnEngineMessage(interopMessageId, mr);
					break;
				case InteropMessageID.IMID_SCRIPT_OBJECT_STATUS:
					this._gameObjectMediator.OnObjectStatus(mr.ReadInteger(), (GameObjectStatus)mr.ReadInteger());
					break;
				case InteropMessageID.IMID_SCRIPT_OBJECT_SETPROP:
				case InteropMessageID.IMID_SCRIPT_OBJECT_SETPLAYER:
				case InteropMessageID.IMID_SCRIPT_MANEUVER_INFO:
				case InteropMessageID.IMID_SCRIPT_FORMATION_REMOVE_SHIP:
					this._gameObjectMediator.OnObjectScriptMessage(interopMessageId, mr.ReadInteger(), mr);
					break;
				case InteropMessageID.IMID_SCRIPT_NETWORK:
					if (this._network == null)
						break;
					this._network.ProcessEngineMessage(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_KEYCOMBO:
				case InteropMessageID.IMID_SCRIPT_VKREPORT:
					this._hotkeyManager.OnEngineMessage(interopMessageId, mr);
					break;
				default:
					App.Log.Warn("Unhandled message (id=" + (object)interopMessageId + ").", "engine");
					break;
			}
		}

		public void Update()
		{
			try
			{
				ConsoleCommandParse.ProcessConsoleCommands(this, this._consoleApplet);
				foreach (ScriptMessageReader pumpMessage in this._scriptCommChannel.PumpMessages())
					this.ProcessEngineMessage(pumpMessage);
				this._uiCommChannel.Update();
				this._gameStateMachine.Update();
				App.m_Commands.Poll();
				this._ircChat.Update();
				if (this.GameSetup == null || this.Game == null || (!this.Game.TurnTimer.IsRunning() || (double)this.Game.TurnTimer.StrategicTurnLength == 0.0))
					return;
				TimeSpan turnTime = this.Game.TurnTimer.GetTurnTime();
				if ((double)((float)turnTime.Seconds + (float)turnTime.Minutes * 60f) >= (double)this.Game.TurnTimer.StrategicTurnLength * 60.0)
				{
					this.Game.TurnTimer.StopTurnTimer();
					this.GetGameState<StarMapState>().EndTurn(false);
				}
				if (!this.GameSetup.IsMultiplayer)
					return;
				this.Network.SetTime((float)turnTime.TotalSeconds, this.Game.TurnTimer.StrategicTurnLength);
			}
			catch (Exception ex)
			{
				App.Log.Warn(ex.ToString(), "game");
				if (this._numExceptionErrorsDisplayed >= 2)
					return;
				if (this._numExceptionErrorsDisplayed == 0 && this.Game != null)
				{
					string filename = Path.Combine(Path.GetDirectoryName(App.Log.FilePath), Path.GetFileNameWithoutExtension(App.Log.FilePath)) + "_localgame.db";
					App.Log.Warn("Writing local game database for debugging...", "game");
					this.Game.GameDatabase.Save(filename);
				}
				int num = (int)MessageBox.Show(string.Format("An error occurred:\n{0}\n\nRefer to log file for more information:\n{1}", (object)ex.ToString(), (object)App.Log.FilePath), "SotS Error");
				throw;
			}
		}

		public void Exiting()
		{
		}

		public GameSession Game
		{
			get
			{
				return this.game;
			}
		}

		public Kerberos.Sots.PlayerFramework.Player LocalPlayer
		{
			get
			{
				if (this.game == null)
					return (Kerberos.Sots.PlayerFramework.Player)null;
				return this.game.LocalPlayer;
			}
		}

		public static GameUICommands Commands
		{
			get
			{
				return App.m_Commands;
			}
		}

		private static List<App.StartLocation> CollectStartLocations(LegacyStarMap starmap)
		{
			List<App.StartLocation> startLocationList = new List<App.StartLocation>();
			foreach (StarSystem starSystem in starmap.Objects.OfType<StarSystem>().Where<StarSystem>((Func<StarSystem, bool>)(x => x.IsStartPosition)))
				startLocationList.Add(new App.StartLocation()
				{
					System = starSystem
				});
			return startLocationList;
		}

		private static List<App.StartLocation> RandomizeStartLocations(
		  List<App.StartLocation> choices,
		  IList<PlayerSetup> players)
		{
			List<App.StartLocation> startLocationList = new List<App.StartLocation>();
			for (int i = 0; i < players.Count<PlayerSetup>() && choices.Count != 0; ++i)
			{
				PlayerSetup player = players[i];
				App.StartLocation startLocation = (App.StartLocation)null;
				if (!string.IsNullOrEmpty(player.Faction))
					startLocation = choices.FirstOrDefault<App.StartLocation>((Func<App.StartLocation, bool>)(x =>
				   {
					   int? playerSlot = x.System.Params.PlayerSlot;
					   int num = i + 1;
					   if (playerSlot.GetValueOrDefault() == num)
						   return playerSlot.HasValue;
					   return false;
				   }));
				if (startLocation == null)
					startLocation = choices.FirstOrDefault<App.StartLocation>((Func<App.StartLocation, bool>)(x => !x.System.Params.PlayerSlot.HasValue));
				if (startLocation == null)
					startLocation = choices[0];
				if (startLocation != null)
				{
					startLocationList.Add(new App.StartLocation()
					{
						System = startLocation.System,
						Planet = startLocation.Planet,
						PlayerIndex = i
					});
					choices.Remove(startLocation);
				}
			}
			return startLocationList;
		}

		private static void ConfigureStartLocation(
		  Random random,
		  App.StartLocation loc,
		  IList<PlayerSetup> players,
		  GameDatabase db)
		{
			PlanetOrbit planetOrbit = (PlanetOrbit)loc.Planet.Params;
			int num = !planetOrbit.CapitalOrbit ? 1 : 0;
			if (num != 0 || string.IsNullOrEmpty(planetOrbit.PlanetType))
				planetOrbit.PlanetType = StellarBodyTypes.Normal;
			if (num != 0 || !planetOrbit.Resources.HasValue)
				planetOrbit.Resources = new int?(15000);
			if (num != 0 || !planetOrbit.Size.HasValue)
				planetOrbit.Size = new int?(10);
			if (num != 0 || !planetOrbit.Biosphere.HasValue)
				planetOrbit.Biosphere = new int?(15000);
			planetOrbit.Suitability = new float?(db.GetFactionSuitability(players[loc.PlayerIndex].Faction));
		}

		private static void EnsureInhabitableStartLocation(
		  Random random,
		  List<App.StartLocation> startLocations,
		  IList<PlayerSetup> players,
		  GameDatabase db)
		{
			foreach (App.StartLocation startLocation in startLocations)
			{
				if (startLocation.Planet == null)
				{
					IEnumerable<IStellarEntity> colonizableWorlds = startLocation.System.GetColonizableWorlds(true);
					IEnumerable<IStellarEntity> stellarEntities = colonizableWorlds.Where<IStellarEntity>((Func<IStellarEntity, bool>)(x => (x.Params as PlanetOrbit).CapitalOrbit));
					if (stellarEntities.Any<IStellarEntity>())
					{
						startLocation.Planet = random.Choose<IStellarEntity>(stellarEntities);
						App.ConfigureStartLocation(random, startLocation, players, db);
					}
					else if (colonizableWorlds.Any<IStellarEntity>())
					{
						startLocation.Planet = random.Choose<IStellarEntity>(colonizableWorlds);
						App.ConfigureStartLocation(random, startLocation, players, db);
					}
					else
					{
						int num = startLocation.System.Objects.Max<IStellarEntity>((Func<IStellarEntity, int>)(x =>
					   {
						   if (x.Orbit != null)
							   return x.Orbit.OrbitNumber;
						   return 1;
					   })) + 1;
						PlanetOrbit orbiterParams = new PlanetOrbit();
						orbiterParams.OrbitNumber = num;
						IEnumerable<IStellarEntity> planet = StarSystemHelper.CreatePlanet(random, orbiterParams, true);
						foreach (IStellarEntity stellarEntity in planet.Where<IStellarEntity>((Func<IStellarEntity, bool>)(x => x.Orbit == null)))
							stellarEntity.Orbit = StarSystem.SetOrbit(random, startLocation.System.Star.Params, stellarEntity.Params);
						startLocation.System.AddRange(planet);
						startLocation.Planet = planet.First<IStellarEntity>();
						App.ConfigureStartLocation(random, startLocation, players, db);
					}
				}
			}
		}

		public static string GetFactionIcon(string faction)
		{
			if (faction == null)
				return string.Empty;
			switch (faction)
			{
				case "human":
					return "sotspedia_humanlogo";
				case "hiver":
					return "sotspedia_hiverlogo";
				case "liir_zuul":
					return "sotspedia_liirzuullogo";
				case "morrigi":
					return "sotspedia_moriggilogo";
				case "tarkas":
					return "sotspedia_tarklogo";
				case "zuul":
					return "sotspedia_suulkazuullogo";
				case "loa":
					return "sotspedia_loalogo";
				default:
					return string.Empty;
			}
		}

		public static string GetFactionDescription(string faction)
		{
			if (faction == null)
				return string.Empty;
			switch (faction)
			{
				case "human":
					return App.Localize("@FACTION_DESCRIPTION_SOL_FORCE");
				case "hiver":
					return App.Localize("@FACTION_DESCRIPTION_HIVER_IMPERIUM");
				case "tarkas":
					return App.Localize("@FACTION_DESCRIPTION_TARKASIAN_EMPIRE");
				case "liir_zuul":
					return App.Localize("@FACTION_DESCRIPTION_LIIR_ZUUL_ALLIANCE");
				case "zuul":
					return App.Localize("@FACTION_DESCRIPTION_SUULKA_HORDE");
				case "morrigi":
					return App.Localize("@FACTION_DESCRIPTION_MORRIGI_CONFEDERATION");
				case "loa":
					return App.Localize("@FACTION_DESCRIPTION_LOA");
				default:
					return string.Empty;
			}
		}

		public static string GetLocalizedFactionName(string faction)
		{
			switch (faction)
			{
				case "human":
					return App.Localize("@FACTION_SOL_FORCE");
				case "hiver":
					return App.Localize("@FACTION_HIVER_IMPERIUM");
				case "tarkas":
					return App.Localize("@FACTION_TARKASIAN_EMPIRE");
				case "liir_zuul":
					return App.Localize("@FACTION_LIIR_ZUUL_ALLIANCE");
				case "zuul":
					return App.Localize("@FACTION_SUULKA_HORDE");
				case "morrigi":
					return App.Localize("@FACTION_MORRIGI_CONFEDERATION");
				case "loa":
					return App.Localize("@FACTION_LOA");
				default:
					return "!!EMPTY FACTION!!";
			}
		}

		public int? GetFactionID(string name)
		{
			foreach (Faction faction in this._assetDatabase.Factions)
			{
				if (faction.Name == name)
					return new int?(faction.ID);
			}
			return new int?();
		}

		public static Random GetSafeRandom()
		{
			if (App._safeRandom == null)
				App._safeRandom = new Random();
			return App._safeRandom;
		}

		private class StartLocation
		{
			public int PlayerIndex = -1;
			public StarSystem System;
			public IStellarEntity Planet;
		}
	}
}
