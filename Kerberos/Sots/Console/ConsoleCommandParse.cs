// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Console.ConsoleCommandParse
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Kerberos.Sots.Console
{
	internal static class ConsoleCommandParse
	{
		private static readonly ConsoleCommandParse.CommandInfo[] _commandInfos = new ConsoleCommandParse.CommandInfo[69]
		{
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[2]{ "help", "?" },
		Description = "Display available commands.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  StringBuilder stringBuilder = new StringBuilder();
		  IOrderedEnumerable<string> source = ((IEnumerable<ConsoleCommandParse.CommandInfo>) ConsoleCommandParse._commandInfos).SelectMany<ConsoleCommandParse.CommandInfo, string>((Func<ConsoleCommandParse.CommandInfo, IEnumerable<string>>) (x => (IEnumerable<string>) x.Aliases)).Distinct<string>().OrderBy<string, string>((Func<string, string>) (y => y));
		  string str1 = source.Last<string>();
		  foreach (string str2 in (IEnumerable<string>) source)
		  {
			stringBuilder.Append(str2);
			if (str2 != str1)
			  stringBuilder.Append(", ");
			stringBuilder.AppendLine();
		  }
		  ConsoleCommandParse.Trace(stringBuilder.ToString());
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[2]{ "help", "?" },
		Description = "Display information about a command.",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  StringBuilder stringBuilder = new StringBuilder();
		  foreach (ConsoleCommandParse.CommandInfo commandInfo in ((IEnumerable<ConsoleCommandParse.CommandInfo>) ConsoleCommandParse._commandInfos).Where<ConsoleCommandParse.CommandInfo>((Func<ConsoleCommandParse.CommandInfo, bool>) (x => ((IEnumerable<string>) x.Aliases).Any<string>((Func<string, bool>) (y => y.Equals(parms.First<string>(), StringComparison.InvariantCulture))))))
		  {
			string str = ((IEnumerable<string>) commandInfo.Aliases).Last<string>();
			foreach (string aliase in commandInfo.Aliases)
			{
			  stringBuilder.Append(aliase);
			  if (aliase != str)
				stringBuilder.Append(", ");
			}
			stringBuilder.AppendLine(": (" + (object) commandInfo.ParameterCount + ")");
			stringBuilder.AppendLine(commandInfo.Description);
			stringBuilder.AppendLine();
		  }
		  ConsoleCommandParse.Trace(stringBuilder.ToString());
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[2]{ "exit", "quit" },
		Description = "Exit the game application.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) => game.RequestExit())
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "state" },
		Description = "Display the current and pending game states.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  ConsoleCommandParse.Trace(" Current state: " + (object) game.CurrentState ?? "none");
		  ConsoleCommandParse.Trace(" Pending state: " + (object) game.PendingState ?? "none");
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "state" },
		Description = "Switch to the specified game state.",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) => game.SwitchGameState(parms.First<string>()))
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "states" },
		Description = "Display all game states.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  ConsoleCommandParse.Trace("Available game states:");
		  foreach (GameState state in game.States)
			ConsoleCommandParse.Trace(" " + state.Name);
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "msglog" },
		Description = "Enable or disable the logging of engine messages.",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  bool flag = bool.Parse(parms.First<string>());
		  UICommChannel.LogEnable = flag;
		  ScriptCommChannel.LogEnable = flag;
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "load_tac_targeting" },
		Description = "Loads combat with two hiver cruisers for target practice.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  XmlDocument xmlDocument = new XmlDocument();
		  xmlDocument.LoadXml(ConsoleResources.load_tac_targeting_config);
		  CombatState gameState = game.GetGameState<CombatState>();
		  int num = 0;
		  game.SwitchGameState((GameState) gameState, (object) num, (object) xmlDocument);
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "toggleAI" },
		Description = "Turn AI on/off.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  App.m_bAI_Enabled = !App.m_bAI_Enabled;
		  if (App.m_bAI_Enabled)
			ConsoleCommandParse.Trace("AI enabled");
		  else
			ConsoleCommandParse.Trace("AI disabled");
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "togglePlayerAI" },
		Description = "Turn AI on/off for human player.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  App.m_bPlayerAI_Enabled = !App.m_bPlayerAI_Enabled;
		  if (App.m_bPlayerAI_Enabled)
			ConsoleCommandParse.Trace("Human Player's AI enabled");
		  else
			ConsoleCommandParse.Trace("Human Player's AI disabled");
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "debugFUP" },
		Description = "Make all ships path to a Forming Up Point appropriate for the destination, rather than the destination itself.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  App.m_bDebugFup = !App.m_bDebugFup;
		  if (App.m_bDebugFup)
			ConsoleCommandParse.Trace("Form Up Point debugging enabled");
		  else
			ConsoleCommandParse.Trace("Form Up Point debugging disabled");
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "gamedbloc" },
		Description = "Relocate the live game database to a file or :memory: depending on diagnostic needs.",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  if (game.GameDatabase == null)
			return;
		  game.GameDatabase.ChangeLiveLocationAndOpen(parms.First<string>());
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "gamedbloc" },
		Description = "Display the current location of the live game database.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) => ConsoleCommandParse.Trace("  " + (object) game.GameDatabase != null ? game.GameDatabase.LiveLocation : "(not available)"))
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "players" },
		Description = "Display live game players.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  foreach (PlayerInfo playerInfo in game.GameDatabase.GetPlayerInfos())
		  {
			string factionName = game.GameDatabase.GetFactionName(playerInfo.FactionID);
			ConsoleCommandParse.Trace(string.Format(" {0}: {1}, {2}, {3}", (object) playerInfo.ID, (object) playerInfo.Name, (object) factionName, game.Game.GetPlayerObject(playerInfo.ID).IsAI() ? (object) "ai" : (object) "NO ai"));
		  }
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "addaiplayer" },
		Description = "Add an AI player to the game with random starting conditions.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  Random random = new Random();
		  HashSet<int> source = new HashSet<int>(game.GameDatabase.GetStarSystemIDs());
		  foreach (int homeWorldId in game.GameDatabase.GetHomeWorldIDs())
			source.Remove(homeWorldId);
		  int num = game.GameDatabase.InsertPlayer("AI Controlled " + Guid.NewGuid().ToString(), "human", new int?(source.First<int>()), new Vector3(random.NextSingle(), random.NextSingle(), random.NextSingle()), new Vector3(1f, 1f, 1f), game.AssetDatabase.GetRandomBadgeTexture("human", random), game.AssetDatabase.GetRandomAvatarTexture("human", random), 100000.0, 0, true, false, false, 0, AIDifficulty.Normal);
		  ResearchScreenState.BuildPlayerTechTree(game, game.AssetDatabase, game.GameDatabase, num);
		  game.GameDatabase.InsertOrIgnoreAI(num, AIStance.EXPANDING);
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "pcoverride" },
		Description = "Override the existing primary and secondary colors for all players created after this point. (r1 g1 b1 r2 g2 b2)",
		ParameterCount = 6,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  string[] array = parms.Take<string>(6).ToArray<string>();
		  Player.OverridePlayerColors(new Vector3(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2])), new Vector3(float.Parse(array[3]), float.Parse(array[4]), float.Parse(array[5])));
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "advmove" },
		Description = "Set advanced formation movement enabled (true) or disabled (false).",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  bool flag = bool.Parse(parms.First<string>());
		  FormationDefinition.IsAdvancedFormationMovementEnabled = flag;
		  if (flag)
			ConsoleCommandParse.Trace("Advanced formation movement is ENABLED.");
		  else
			ConsoleCommandParse.Trace("Advanced formation movement is DISABLED.");
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "advmove" },
		Description = "Displays the state of advanced formation movement.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  if (FormationDefinition.IsAdvancedFormationMovementEnabled)
			ConsoleCommandParse.Trace("Advanced formation movement is ENABLED.");
		  else
			ConsoleCommandParse.Trace("Advanced formation movement is DISABLED.");
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "savedb" },
		Description = "Saves the current game database to the given file.",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) => game.GameDatabase.Save(parms.First<string>()))
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "savecombat" },
		Description = "Writes out a partial combat config file (xml) with the current positions of active ships. Uses default human cruiser ",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  ConsoleCommandParse.Trace("Writing current combat configuration to '" + parms.First<string>() + "'...");
		  if (!(game.CurrentState is CommonCombatState))
			throw new InvalidOperationException("The current game state must be a CommonCombatState in order to write a CombatConfig file.");
		  (game.CurrentState as CommonCombatState).SaveCombatConfig(parms.First<string>());
		  ConsoleCommandParse.Trace("OK.");
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "endcombat" },
		Description = "Forces an end condition in the running combat state and subsequent beaming of information back to the strat game.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  if (!(game.CurrentState is CommonCombatState))
			throw new InvalidOperationException("Not in combat.");
		  if (!(game.CurrentState as CommonCombatState).EndCombat())
			ConsoleCommandParse.Trace("Combat is already ending.");
		  else
			ConsoleCommandParse.Trace("OK.");
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "retain_combat_config" },
		Description = "Specifies whether combat states should retain the configuration xml document for modification (true/false). Must be true before entering combat for 'savecombat' to work.",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  CommonCombatState.RetainCombatConfig = bool.Parse(parms.First<string>());
		  ConsoleCommandParse.Trace("CommonCombatState.RetainCombatConfig is now " + CommonCombatState.RetainCombatConfig.ToString().ToUpper() + ".");
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "toggle_ai" },
		Description = "Will enable Strategic AI control on the specified player.",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  int playerId = int.Parse(parms.First<string>());
		  Player player = game.GetPlayer(playerId);
		  if (player == null)
			return;
		  ConsoleCommandParse.Trace("Toggling AI (" + (object) !player.IsAI() + ") on player: " + (object) playerId);
		  player.SetAI(!player.IsAI());
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "TechsDialog" },
		Description = "Spawns dialog with all techs.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) => game.UI.CreateDialog((Dialog) new DebugTechDialog(game.Game, 0), null))
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "possess_player" },
		Description = "Allows the player to take control of another player.",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  int playerId = int.Parse(parms.First<string>());
		  Player player = game.GetPlayer(playerId);
		  if (player == null)
			return;
		  ConsoleCommandParse.Trace("Possessing player ID: " + (object) playerId);
		  player.SetAI(false);
		  game.Game.SetLocalPlayer(player);
		  game.GetGameState<StarMapState>().RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_ALL);
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "add_demo_ships" },
		Description = "This HACK adds demo ships to the first couple players (morrigi and zuul) at the first player's system.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  ConsoleCommandParse.Trace("Adding a bunch of new designs and ships and stuff...");
		  GameDatabase gameDatabase = game.GameDatabase;
		  int p1 = game.LocalPlayer.ID;
		  int id1 = game.GameDatabase.GetFleetInfosByPlayerID(p1, FleetType.FL_NORMAL).First<FleetInfo>().ID;
		  int systemId = game.GameDatabase.GetHomeworlds().First<HomeworldInfo>((Func<HomeworldInfo, bool>) (x => x.PlayerID == p1)).SystemID;
		  DesignInfo design1 = new DesignInfo(p1, "WingStrike", new string[3]
		  {
			"factions\\morrigi\\sections\\cr_cmd.section",
			"factions\\morrigi\\sections\\cr_mis_armor.section",
			"factions\\morrigi\\sections\\cr_eng_fusion.section"
		  });
		  DesignInfo design2 = new DesignInfo(p1, "Darkness Shatters", new string[3]
		  {
			"factions\\morrigi\\sections\\cr_cmd.section",
			"factions\\morrigi\\sections\\cr_mis_carrier.section",
			"factions\\morrigi\\sections\\cr_eng_fusion.section"
		  });
		  DesignInfo design3 = new DesignInfo(p1, "War's End", new string[3]
		  {
			"factions\\morrigi\\sections\\cr_cmd_assault.section",
			"factions\\morrigi\\sections\\cr_mis_armor.section",
			"factions\\morrigi\\sections\\cr_eng_fusion.section"
		  });
		  DesignInfo design4 = new DesignInfo(p1, "Ke'Kona's Sorrow", new string[3]
		  {
			"factions\\morrigi\\sections\\cr_cmd.section",
			"factions\\morrigi\\sections\\cr_mis_barrage.section",
			"factions\\morrigi\\sections\\cr_eng_fusion.section"
		  });
		  DesignInfo design5 = new DesignInfo(p1, "Khan's Fist", new string[3]
		  {
			"factions\\morrigi\\sections\\cr_cmd.section",
			"factions\\morrigi\\sections\\cr_mis_cnc.section",
			"factions\\morrigi\\sections\\cr_eng_fusion.section"
		  });
		  DesignInfo design6 = new DesignInfo(p1, "Drone", new string[1]
		  {
			"factions\\morrigi\\sections\\br_drone.section"
		  });
		  DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design1);
		  DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design2);
		  DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design3);
		  DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design4);
		  DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design5);
		  DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design6);
		  int designID1 = gameDatabase.InsertDesignByDesignInfo(design1);
		  int designID2 = gameDatabase.InsertDesignByDesignInfo(design2);
		  int designID3 = gameDatabase.InsertDesignByDesignInfo(design3);
		  int designID4 = gameDatabase.InsertDesignByDesignInfo(design4);
		  int designID5 = gameDatabase.InsertDesignByDesignInfo(design5);
		  int designID6 = gameDatabase.InsertDesignByDesignInfo(design6);
		  gameDatabase.InsertShip(id1, designID1, null, (ShipParams) 0, new int?(), 0);
		  gameDatabase.InsertShip(id1, designID2, null, (ShipParams) 0, new int?(), 0);
		  gameDatabase.InsertShip(id1, designID3, null, (ShipParams) 0, new int?(), 0);
		  gameDatabase.InsertShip(id1, designID4, null, (ShipParams) 0, new int?(), 0);
		  gameDatabase.InsertShip(id1, designID5, null, (ShipParams) 0, new int?(), 0);
		  gameDatabase.InsertShip(id1, designID6, null, (ShipParams) 0, new int?(), 0);
		  int id2 = game.GameDatabase.GetPlayerInfos().First<PlayerInfo>((Func<PlayerInfo, bool>) (x => game.Game.GetPlayerObject(x.ID).IsAI())).ID;
		  int id3 = game.GameDatabase.GetFleetInfosByPlayerID(id2, FleetType.FL_NORMAL).First<FleetInfo>().ID;
		  DesignInfo design7 = new DesignInfo(id2, "The Pure", new string[3]
		  {
			"factions\\zuul\\sections\\cr_cmd.section",
			"factions\\zuul\\sections\\cr_mis_armor.section",
			"factions\\zuul\\sections\\cr_eng_fusion.section"
		  });
		  DesignInfo design8 = new DesignInfo(id2, "The Divine", new string[3]
		  {
			"factions\\zuul\\sections\\cr_cmd.section",
			"factions\\zuul\\sections\\cr_mis_cnc.section",
			"factions\\zuul\\sections\\cr_eng_fusion.section"
		  });
		  DesignInfo design9 = new DesignInfo(id2, "The Seekers", new string[3]
		  {
			"factions\\zuul\\sections\\cr_cmd_deepscan.section",
			"factions\\zuul\\sections\\cr_mis_armor.section",
			"factions\\zuul\\sections\\cr_eng_fusion.section"
		  });
		  DesignInfo design10 = new DesignInfo(id2, "The First-Born", new string[1]
		  {
			"factions\\zuul\\sections\\cr_mis_rending_bore.section"
		  });
		  DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design7);
		  DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design8);
		  DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design9);
		  DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design10);
		  int designID7 = game.GameDatabase.InsertDesignByDesignInfo(design7);
		  int designID8 = game.GameDatabase.InsertDesignByDesignInfo(design8);
		  int designID9 = game.GameDatabase.InsertDesignByDesignInfo(design9);
		  int designID10 = game.GameDatabase.InsertDesignByDesignInfo(design10);
		  gameDatabase.InsertShip(id3, designID7, null, (ShipParams) 0, new int?(), 0);
		  gameDatabase.InsertShip(id3, designID8, null, (ShipParams) 0, new int?(), 0);
		  gameDatabase.InsertShip(id3, designID9, null, (ShipParams) 0, new int?(), 0);
		  gameDatabase.InsertShip(id3, designID10, null, (ShipParams) 0, new int?(), 0);
		  gameDatabase.UpdateFleetLocation(id3, systemId, new int?());
		  ConsoleCommandParse.Trace("OK.");
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "show_research_debug" },
		Description = "Causes the research screen debugging buttons to appear.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) => game.GetGameState<ResearchScreenState>().ShowDebugControls())
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "alltech" },
		Description = "Gives player all technologies.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  ResearchScreenState.AcquireAllTechs(game.Game, game.LocalPlayer.ID);
		  game.Game.CheckForNewEquipment(game.LocalPlayer.ID);
		  game.Game.UpdateProfileTechs();
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "debug_newseventart" },
		Description = "Prints Debug feed for news event art.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) => TurnEvent.DebugPrintNewsEventArt(game.Game))
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "pimpedfleet" },
		Description = "Gives you a pimped fleet",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((game, parms) =>
		{
		  int systemId = game.GameDatabase.GetPlayerHomeworld(game.LocalPlayer.ID).SystemID;
		  int admiralID = game.GameDatabase.InsertAdmiral(game.LocalPlayer.ID, new int?(systemId), "Joe Kickass", "human", 10f, "male", 100f, 100f, 100);
		  int fleetID = game.GameDatabase.InsertFleet(game.LocalPlayer.ID, admiralID, systemId, systemId, "1337 Fleet", FleetType.FL_NORMAL);
		  DesignInfo design1 = new DesignInfo();
		  design1.PlayerID = game.LocalPlayer.ID;
		  design1.Name = "Fit Shucker MKII";
		  design1.Role = ShipRole.CARRIER;
		  design1.DesignSections = new DesignSectionInfo[3];
		  design1.DesignSections[0] = new DesignSectionInfo();
		  design1.DesignSections[0].FilePath = "factions\\" + game.LocalPlayer.Faction.Name + "\\sections\\dn_eng_fusion.section";
		  design1.DesignSections[1] = new DesignSectionInfo();
		  design1.DesignSections[1].FilePath = "factions\\" + game.LocalPlayer.Faction.Name + "\\sections\\dn_mis_armor.section";
		  design1.DesignSections[2] = new DesignSectionInfo();
		  design1.DesignSections[2].FilePath = "factions\\" + game.LocalPlayer.Faction.Name + "\\sections\\dn_cmd_assault.section";
		  DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design1);
		  int designID1 = game.GameDatabase.InsertDesignByDesignInfo(design1);
		  for (int index = 0; index < 3; ++index)
			game.GameDatabase.InsertShip(fleetID, designID1, null, (ShipParams) 0, new int?(), 0);
		  DesignInfo designInfo = new DesignInfo();
		  design1.PlayerID = game.LocalPlayer.ID;
		  design1.Name = "Little MEEP";
		  design1.DesignSections = new DesignSectionInfo[2];
		  design1.DesignSections[0] = new DesignSectionInfo();
		  design1.DesignSections[0].FilePath = "factions\\" + game.LocalPlayer.Faction.Name + "\\sections\\br_eng_fusion.section";
		  design1.DesignSections[1] = new DesignSectionInfo();
		  design1.DesignSections[1].FilePath = "factions\\" + game.LocalPlayer.Faction.Name + "\\sections\\br_msn_spinal.section";
		  DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design1);
		  int designID2 = game.GameDatabase.InsertDesignByDesignInfo(design1);
		  DesignInfo design2 = new DesignInfo();
		  design2.PlayerID = game.LocalPlayer.ID;
		  design2.Name = "MEEP";
		  design2.Role = ShipRole.CARRIER;
		  design2.DesignSections = new DesignSectionInfo[3];
		  design2.DesignSections[0] = new DesignSectionInfo();
		  design2.DesignSections[0].FilePath = "factions\\" + game.LocalPlayer.Faction.Name + "\\sections\\cr_eng_fusion.section";
		  design2.DesignSections[1] = new DesignSectionInfo();
		  design2.DesignSections[1].FilePath = "factions\\" + game.LocalPlayer.Faction.Name + "\\sections\\cr_mis_brcarrier.section";
		  design2.DesignSections[2] = new DesignSectionInfo();
		  design2.DesignSections[2].FilePath = "factions\\" + game.LocalPlayer.Faction.Name + "\\sections\\cr_cmd_assault.section";
		  int designID3 = game.GameDatabase.InsertDesignByDesignInfo(design2);
		  for (int index1 = 0; index1 < 8; ++index1)
		  {
			int parentID = game.GameDatabase.InsertShip(fleetID, designID3, null, (ShipParams) 0, new int?(), 0);
			for (int index2 = 0; index2 < 3; ++index2)
			{
			  int shipID = game.GameDatabase.InsertShip(fleetID, designID2, null, (ShipParams) 0, new int?(), 0);
			  game.GameDatabase.SetShipParent(shipID, parentID);
			  game.GameDatabase.UpdateShipRiderIndex(shipID, index2);
			}
		  }
		  DesignInfo design3 = new DesignInfo();
		  design3.PlayerID = game.LocalPlayer.ID;
		  design3.Name = "Fun Master Flex";
		  design3.Role = ShipRole.CARRIER;
		  design3.Class = ShipClass.Leviathan;
		  design3.DesignSections = new DesignSectionInfo[1];
		  design3.DesignSections[0] = new DesignSectionInfo();
		  design3.DesignSections[0].FilePath = "factions\\" + game.LocalPlayer.Faction.Name + "\\sections\\lv_carrier.section";
		  DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design3);
		  int designID4 = game.GameDatabase.InsertDesignByDesignInfo(design3);
		  game.GameDatabase.InsertShip(fleetID, designID4, null, (ShipParams) 0, new int?(), 0);
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "allencountered" },
		Description = "Sets all players in the game to be already encountered.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  List<int> list = Game.GameDatabase.GetStandardPlayerIDs().ToList<int>();
		  foreach (int playerID in list)
		  {
			foreach (int towardsPlayerID in list)
			{
			  if (playerID != towardsPlayerID)
			  {
				DiplomacyInfo diplomacyInfo = Game.GameDatabase.GetDiplomacyInfo(playerID, towardsPlayerID);
				diplomacyInfo.isEncountered = true;
				Game.GameDatabase.UpdateDiplomacyInfo(diplomacyInfo);
			  }
			}
		  }
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "adddiplomacypoints" },
		Description = "Adds 100 points to diplomacy, generic diplomacy, intel, counter intel, and operations for the local player.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  PlayerInfo playerInfo = Game.GameDatabase.GetPlayerInfo(Game.LocalPlayer.ID);
		  foreach (int factionId in playerInfo.FactionDiplomacyPoints.Keys.ToList<int>())
		  {
			Dictionary<int, int> factionDiplomacyPoints;
			int index;
			(factionDiplomacyPoints = playerInfo.FactionDiplomacyPoints)[index = factionId] = factionDiplomacyPoints[index] + 100;
			Game.GameDatabase.UpdateFactionDiplomacyPoints(playerInfo.ID, factionId, playerInfo.FactionDiplomacyPoints[factionId]);
		  }
		  playerInfo.GenericDiplomacyPoints += 100;
		  playerInfo.IntelPoints += 100;
		  playerInfo.CounterIntelPoints += 100;
		  playerInfo.OperationsPoints += 100;
		  Game.GameDatabase.UpdatePlayerIntelPoints(playerInfo.ID, playerInfo.IntelPoints);
		  Game.GameDatabase.UpdatePlayerCounterintelPoints(playerInfo.ID, playerInfo.CounterIntelPoints);
		  Game.GameDatabase.UpdatePlayerOperationsPoints(playerInfo.ID, playerInfo.OperationsPoints);
		  Game.GameDatabase.UpdateGenericDiplomacyPoints(playerInfo.ID, playerInfo.GenericDiplomacyPoints);
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "forcevn" },
		Description = "Forces a von neumann collector attack on your homeworld",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  if (Game.Game.ScriptModules.VonNeumann == null)
			return;
		  Game.Game.ScriptModules.VonNeumann.ForceVonNeumannAttack = true;
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "forcevncycle" },
		Description = "Gives vonneumann a lot of resources and targets your homeworld",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  if (Game.Game.ScriptModules.VonNeumann == null)
			return;
		  HomeworldInfo playerHomeworld = Game.GameDatabase.GetPlayerHomeworld(Game.LocalPlayer.ID);
		  List<VonNeumannInfo> list = Game.GameDatabase.GetVonNeumannInfos().ToList<VonNeumannInfo>();
		  if (list.Count > 0)
		  {
			VonNeumannInfo vonNeumannInfo = list.First<VonNeumannInfo>();
			vonNeumannInfo.Resources += 200000;
			vonNeumannInfo.SystemId = playerHomeworld.SystemID;
			Game.GameDatabase.UpdateVonNeumannInfo(list.First<VonNeumannInfo>());
		  }
		  else
			ConsoleCommandParse.Trace("Failed to create VN with top resources, try again next turn");
		  Game.Game.ScriptModules.VonNeumann.ForceVonNeumannAttackCycle = true;
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "forcemonitor" },
		Description = "Spawns a monitor encounter at the closest available system",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  StarSystemInfo home = Game.GameDatabase.GetStarSystemInfo(Game.GameDatabase.GetOrbitalObjectInfo(Game.GameDatabase.GetPlayerInfo(Game.LocalPlayer.ID).Homeworld.Value).StarSystemID);
		  foreach (StarSystemInfo starSystemInfo in Game.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>().OrderBy<StarSystemInfo, float>((Func<StarSystemInfo, float>) (x => (x.Origin - home.Origin).Length)).ToList<StarSystemInfo>())
		  {
			if (Game.GameDatabase.GetColonyInfosForSystem(starSystemInfo.ID).ToList<ColonyInfo>().Count == 0)
			{
			  List<int> list = Game.GameDatabase.GetStarSystemOrbitalObjectIDs(starSystemInfo.ID).ToList<int>();
			  if (list.Count > 0 && Game.Game.ScriptModules.AsteroidMonitor != null)
				Game.Game.ScriptModules.AsteroidMonitor.AddInstance(Game.GameDatabase, Game.AssetDatabase, starSystemInfo.ID, list.First<int>());
			}
		  }
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "surveyall" },
		Description = "Know everything about every system",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  foreach (StarSystemInfo starSystemInfo in Game.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>())
			Game.GameDatabase.InsertExploreRecord(starSystemInfo.ID, Game.LocalPlayer.ID, 1, true, true);
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "forceprotean" },
		Description = "Spawns a protean at selected system",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  if (!(Game.CurrentState.GetType() == typeof (StarMapState)))
			return;
		  int selectedSystem = ((StarMapState) Game.CurrentState).GetSelectedSystem();
		  if (selectedSystem == 0)
			return;
		  Game.Game.ScriptModules.Gardeners.AddInstance(Game.GameDatabase, Game.AssetDatabase, selectedSystem);
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "forcerelic" },
		Description = "Spawns a relic encounter at the closest available system",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  StarSystemInfo home = Game.GameDatabase.GetStarSystemInfo(Game.GameDatabase.GetOrbitalObjectInfo(Game.GameDatabase.GetPlayerInfo(Game.LocalPlayer.ID).Homeworld.Value).StarSystemID);
		  foreach (StarSystemInfo starSystemInfo in Game.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>().OrderBy<StarSystemInfo, float>((Func<StarSystemInfo, float>) (x => (x.Origin - home.Origin).Length)).ToList<StarSystemInfo>())
		  {
			if (Game.GameDatabase.GetColonyInfosForSystem(starSystemInfo.ID).ToList<ColonyInfo>().Count == 0)
			{
			  List<int> list = Game.GameDatabase.GetStarSystemOrbitalObjectIDs(starSystemInfo.ID).ToList<int>();
			  if (list.Count > 0 && Game.Game.ScriptModules.MorrigiRelic != null)
				Game.Game.ScriptModules.MorrigiRelic.AddInstance(Game.GameDatabase, Game.AssetDatabase, starSystemInfo.ID, list.First<int>());
			}
		  }
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "forcemeteor" },
		Description = "Spawns a meteor encounter at the closest local players' available system",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  bool flag = bool.Parse(parms.First<string>());
		  MeteorShower.ForceEncounter = flag;
		  ConsoleCommandParse.Trace("MeteorShower.ForceEncounter: " + flag.ToString());
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "forcecomet" },
		Description = "Spawns a comet encounter at the closest local players' available system",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  if (Game.Game.ScriptModules.Comet != null)
			Game.Game.ScriptModules.Comet.AddInstance(Game.GameDatabase, Game.AssetDatabase, new int?(Game.GameDatabase.GetOrbitalObjectInfo(Game.GameDatabase.GetPlayerInfo(Game.LocalPlayer.ID).Homeworld.Value).StarSystemID));
		  ConsoleCommandParse.Trace("Comet.ForceEncounter");
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "forceghostship" },
		Description = "Spawns a ghost ship at the closest local players' available system",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  bool flag = bool.Parse(parms.First<string>());
		  GhostShip.ForceEncounter = flag;
		  ConsoleCommandParse.Trace("GhostShip.ForceEncounter: " + flag.ToString());
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "forcespectre" },
		Description = "Spawns a spectre haunt at the targeted system.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  int selectedSystem = Game.GetGameState<StarMapState>().GetSelectedSystem();
		  if (selectedSystem == 0)
			return;
		  Game.Game.ScriptModules.Spectre.ExecuteEncounter(Game.Game, Game.LocalPlayer.PlayerInfo, selectedSystem, true);
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "forceslaver" },
		Description = "Spawns a slaver band at the closest local players' available system",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  bool flag = bool.Parse(parms.First<string>());
		  Slaver.ForceEncounter = flag;
		  ConsoleCommandParse.Trace("Slaver.ForceEncounter: " + flag.ToString());
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "forcepirates" },
		Description = "Spawns pirates at every trading system",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  bool flag = bool.Parse(parms.First<string>());
		  Pirates.ForceEncounter = flag;
		  ConsoleCommandParse.Trace("Pirates.ForceEncounter: " + flag.ToString());
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "forcelocust" },
		Description = "Spawns a locust GM",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  if (Game.Game.ScriptModules.Locust == null)
			return;
		  int starSystemId = Game.GameDatabase.GetOrbitalObjectInfo(Game.GameDatabase.GetPlayerInfo(Game.LocalPlayer.ID).Homeworld.Value).StarSystemID;
		  List<StarSystemInfo> list = EncounterTools.GetClosestStars(Game.GameDatabase, starSystemId).Where<StarSystemInfo>((Func<StarSystemInfo, bool>) (x => Game.GameDatabase.GetColonyInfosForSystem(x.ID).Count<ColonyInfo>() == 0)).ToList<StarSystemInfo>();
		  if (list.Count <= 0)
			return;
		  Game.Game.ScriptModules.Locust.AddInstance(Game.GameDatabase, Game.AssetDatabase, new int?(list.First<StarSystemInfo>().ID));
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "forceneutronstar" },
		Description = "Spawns a neutron star GM",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  if (Game.Game.ScriptModules.NeutronStar == null)
			return;
		  int num = 0;
		  if ((object) (Game.Game.StarMapSelectedObject as StarSystemInfo) != null)
			num = (Game.Game.StarMapSelectedObject as StarSystemInfo).ID;
		  if (num == 0)
			num = Game.GameDatabase.GetOrbitalObjectInfo(Game.GameDatabase.GetPlayerInfo(Game.LocalPlayer.ID).Homeworld.Value).StarSystemID;
		  Game.Game.ScriptModules.NeutronStar.AddInstance(Game.GameDatabase, Game.AssetDatabase, new int?(num));
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "forcegardener" },
		Description = "Spawns a gardener GM",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  if (Game.Game.ScriptModules.Gardeners == null)
			return;
		  Game.Game.ScriptModules.Gardeners.AddInstance(Game.GameDatabase, Game.AssetDatabase, 0);
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "forcesupernova" },
		Description = "Spawns a supernova GM",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  if (Game.Game.ScriptModules.SuperNova == null)
			return;
		  int starSystemId = Game.GameDatabase.GetOrbitalObjectInfo(Game.GameDatabase.GetPlayerInfo(Game.LocalPlayer.ID).Homeworld.Value).StarSystemID;
		  List<StarSystemInfo> list = EncounterTools.GetClosestStars(Game.GameDatabase, starSystemId).Where<StarSystemInfo>((Func<StarSystemInfo, bool>) (x => Game.GameDatabase.GetColonyInfosForSystem(x.ID).Count<ColonyInfo>() > 0)).ToList<StarSystemInfo>();
		  List<SuperNovaInfo> superNovas = Game.GameDatabase.GetSuperNovaInfos().ToList<SuperNovaInfo>();
		  list.RemoveAll((Predicate<StarSystemInfo>) (x =>
		  {
			StarSystemInfo x1 = x;
			return superNovas.Any<SuperNovaInfo>((Func<SuperNovaInfo, bool>) (y => y.SystemId == x1.ID));
		  }));
		  if (list.Count <= 0)
			return;
		  Game.Game.ScriptModules.SuperNova.AddInstance(Game.GameDatabase, Game.AssetDatabase, list.First<StarSystemInfo>().ID);
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "forcesk" },
		Description = "Spawns a system killer GM",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  if (Game.Game.ScriptModules.SystemKiller == null)
			return;
		  Game.Game.ScriptModules.SystemKiller.AddInstance(Game.GameDatabase, Game.AssetDatabase, new int?(Game.GameDatabase.GetOrbitalObjectInfo(Game.GameDatabase.GetPlayerInfo(Game.LocalPlayer.ID).Homeworld.Value).StarSystemID));
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "colonize" },
		Description = "Colonizes/develops planet with the given ID for the local player.",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) => GameSession.MakeIdealColony(Game.GameDatabase, Game.AssetDatabase, int.Parse(parms.First<string>()), Game.LocalPlayer.ID, IdealColonyTypes.Secondary))
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "skipcombat" },
		Description = "Displays true/false depending on whether the skip combat hack flag is currently set.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) => ConsoleCommandParse.Trace("Skip combat: " + GameSession.SkipCombatHack.ToString()))
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "skipcombat" },
		Description = "Sets the state of the skip combat hack flage true/false.",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  bool flag = bool.Parse(parms.First<string>());
		  GameSession.SkipCombatHack = flag;
		  ConsoleCommandParse.Trace("Skip combat: " + flag.ToString());
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "forcereaction" },
		Description = "Sets the state of the force reaction (interception) hack flage true/false.",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  bool flag = bool.Parse(parms.First<string>());
		  GameSession.ForceReactionHack = flag;
		  ConsoleCommandParse.Trace("Force reaction: " + flag.ToString());
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "testcombat" },
		Description = "From starmap enters debug combat state.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  if ((object) (Game.Game.StarMapSelectedObject as StarSystemInfo) != null)
		  {
			StarSystemInfo mapSelectedObject = Game.Game.StarMapSelectedObject as StarSystemInfo;
			Game.LaunchCombat(Game.Game, new PendingCombat()
			{
			  SystemID = mapSelectedObject.ID
			}, true, false, true);
		  }
		  else
		  {
			if (!(Game.Game.StarMapSelectedObject is FleetInfo))
			  return;
			FleetInfo mapSelectedObject = Game.Game.StarMapSelectedObject as FleetInfo;
			FleetLocation fl = Game.GameDatabase.GetFleetLocation(mapSelectedObject.ID, false);
			StarSystemInfo starSystemInfo = Game.GameDatabase.GetDeepspaceStarSystemInfos().ToList<StarSystemInfo>().FirstOrDefault<StarSystemInfo>((Func<StarSystemInfo, bool>) (x => (double) (x.Origin - fl.Coords).Length < 9.99999974737875E-05));
			if (starSystemInfo == (StarSystemInfo) null)
			  starSystemInfo.ID = Game.GameDatabase.InsertStarSystem(new int?(), App.Localize("@UI_STARMAP_ENCOUNTER_DEEPSPACE"), new int?(), "Deepspace", fl.Coords, false, false, new int?());
			Game.GameDatabase.UpdateFleetLocation(fl.FleetID, starSystemInfo.ID, new int?());
			Game.LaunchCombat(Game.Game, new PendingCombat()
			{
			  SystemID = starSystemInfo.ID
			}, true, false, true);
		  }
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "extremestars" },
		Description = "Replaces all stars in the active game with smallest/largest for UI testing.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  Game.GameDatabase.ReplaceMapWithExtremeStars();
		  if (Game.CurrentState == Game.GetGameState<StarMapState>())
			Game.GetGameState<StarMapState>().RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_ALL);
		  ConsoleCommandParse.Trace("Stars have gone EXTREME!");
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "nav" },
		Description = "Navigates to the specified page in the encyclopedia.",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) => SotspediaState.NavigateToLink(Game, parms.First<string>()))
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "techstyles" },
		Description = "Tests generation of tech styles for a particular named faction. First param is the faction name, and second param is the number of iterations",
		ParameterCount = 2,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  string[] array = parms.ToArray<string>();
		  if (Game.AssetDatabase == null)
			return;
		  Game.AssetDatabase.AIResearchFramework.TestTechStyleSelection(array[0], int.Parse(array[1]));
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "forceintelcritsuccess" },
		Description = "Sets the state of the force intel critical success hack flage true/false.",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  bool flag = bool.Parse(parms.First<string>());
		  GameSession.ForceIntelMissionCriticalSuccessHack = flag;
		  ConsoleCommandParse.Trace("ForceIntelMissionCriticalSuccessHack: " + flag.ToString());
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "instabuild" },
		Description = "Accelerated construction true/false.",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  bool flag = bool.Parse(parms.First<string>());
		  GameSession.InstaBuildHackEnabled = flag;
		  ConsoleCommandParse.Trace("instabuild: " + flag.ToString());
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "print_designs" },
		Description = "Prints out the list of ship designs for the given player number.",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  int playerid = int.Parse(parms.First<string>());
		  StringBuilder result = new StringBuilder();
		  DesignLab.PrintPlayerDesignSummary(result, Game, playerid, false);
		  ConsoleCommandParse.Trace(result.ToString());
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "netplayers" },
		Description = "Prints out a summary of player states belonging to the active network controller.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  if (Game.Network == null)
			ConsoleCommandParse.Warn("No network object available.");
		  else
			Game.Network.PostLogPlayerInfo();
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "buildcounterdesign" },
		Description = "Prints out a summary of player states belonging to the active network controller.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  int system = Game.GetGameState<StarMapState>().GetSelectedSystem();
		  if (system == 0)
			return;
		  IEnumerable<FleetInfo> source = Game.GameDatabase.GetFleetInfos(FleetType.FL_NORMAL).Where<FleetInfo>((Func<FleetInfo, bool>) (x => x.SystemID == system));
		  if (source.Count<FleetInfo>() <= 0)
			return;
		  int size = 0;
		  StrategicAI.DesignConfigurationInfo enemyInfo = new StrategicAI.DesignConfigurationInfo();
		  foreach (FleetInfo fleetInfo in source)
		  {
			foreach (ShipInfo shipInfo in Game.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, true))
			{
			  StrategicAI.DesignConfigurationInfo configurationInfo = StrategicAI.GetDesignConfigurationInfo(Game.Game, shipInfo.DesignInfo);
			  enemyInfo += configurationInfo;
			  ++size;
			}
		  }
		  enemyInfo.Average(size);
		  DesignInfo counterDesign = DesignLab.CreateCounterDesign(Game.Game, ShipClass.Cruiser, Game.LocalPlayer.ID, enemyInfo);
		  Game.GameDatabase.InsertDesignByDesignInfo(counterDesign);
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "addsuulka" },
		Description = "Adds suulka to player.",
		ParameterCount = 2,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  int index = int.Parse(parms.ElementAt<string>(0));
		  int playerID = int.Parse(parms.ElementAt<string>(1));
		  SuulkaInfo suulkaInfo = Game.GameDatabase.GetSuulkas().ToList<SuulkaInfo>()[index];
		  List<StationInfo> list = Game.GameDatabase.GetStationInfosByPlayerID(playerID).ToList<StationInfo>();
		  StationInfo stationInfo = list.Count == 1 ? list[0] : list[1];
		  Game.GameDatabase.UpdateSuulkaStation(suulkaInfo.ID, stationInfo.OrbitalObjectID);
		  Game.GameDatabase.UpdateSuulkaArrivalTurns(suulkaInfo.ID, -1);
		  Game.Game.InsertSuulkaFleet(playerID, suulkaInfo.ID);
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "suulkaids" },
		Description = "displays suulka indexes and names.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  int num = 0;
		  foreach (SuulkaInfo suulkaInfo in Game.GameDatabase.GetSuulkas().ToList<SuulkaInfo>())
		  {
			ShipInfo shipInfo = Game.GameDatabase.GetShipInfo(suulkaInfo.ShipID, false);
			ConsoleCommandParse.Trace("[" + (object) num + "] Suulka '" + shipInfo.ShipName);
			++num;
		  }
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "movefleet" },
		Description = "Moves a player fleet to target system.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  int selectedSystem = Game.GetGameState<StarMapState>().GetSelectedSystem();
		  if (selectedSystem == 0)
			return;
		  FleetInfo fleetInfo = Game.GameDatabase.GetFleetInfos(FleetType.FL_NORMAL).Where<FleetInfo>((Func<FleetInfo, bool>) (x => x.PlayerID == Game.LocalPlayer.ID)).FirstOrDefault<FleetInfo>();
		  if (fleetInfo == null)
			return;
		  Game.GameDatabase.UpdateFleetLocation(fleetInfo.ID, selectedSystem, new int?());
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "comeatmebro" },
		Description = "Declares war on everyone.",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  int id = Game.LocalPlayer.ID;
		  foreach (PlayerInfo playerInfo in Game.GameDatabase.GetPlayerInfos())
		  {
			if (playerInfo.ID != id)
			  Game.GameDatabase.UpdateDiplomacyState(id, playerInfo.ID, DiplomacyState.WAR, 0, true);
		  }
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "addsavings" },
		Description = "addsavings 'value' adds specified value to savings.",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  int num = int.Parse(parms.First<string>());
		  Game.GameDatabase.UpdatePlayerSavings(Game.LocalPlayer.ID, Game.LocalPlayer.PlayerInfo.Savings += (double) num);
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "simturns" },
		Description = "sims a number of set turns",
		ParameterCount = 1,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) =>
		{
		  foreach (Player player in Game.GameDatabase.GetStandardPlayerIDs().Select<int, Player>((Func<int, Player>) (x => Game.GetPlayer(x))).ToList<Player>())
			player.SetAI(true);
		  GameSession.SimAITurns = Math.Max(int.Parse(parms.First<string>()), 0);
		})
	  },
	  new ConsoleCommandParse.CommandInfo()
	  {
		Aliases = new string[1]{ "stopsim" },
		Description = "stops simturns",
		ParameterCount = 0,
		Action = (Action<App, IEnumerable<string>>) ((Game, parms) => GameSession.SimAITurns = 0)
	  }
		};

		private static void Trace(string message)
		{
			App.Log.Trace(message, "con");
		}

		private static void Warn(string message)
		{
			App.Log.Warn(message, "con");
		}

		public static void Evaluate(App game, IEnumerable<string> cmds)
		{
			foreach (string str in cmds.Select<string, string>((Func<string, string>)(x => x.Trim())).Where<string>((Func<string, bool>)(y => !y.StartsWith("//"))))
			{
				ConsoleCommandParse.Trace("Console: " + str);
				string[] parts = str.Split(' ');
				if (!string.IsNullOrWhiteSpace(parts[0]))
				{
					int paramCount = parts.Length - 1;
					ConsoleCommandParse.CommandInfo commandInfo = ((IEnumerable<ConsoleCommandParse.CommandInfo>)ConsoleCommandParse._commandInfos).FirstOrDefault<ConsoleCommandParse.CommandInfo>((Func<ConsoleCommandParse.CommandInfo, bool>)(x =>
				  {
					  if (x.ParameterCount == paramCount)
						  return ((IEnumerable<string>)x.Aliases).Any<string>((Func<string, bool>)(y => y.Equals(parts[0], StringComparison.InvariantCulture)));
					  return false;
				  }));
					if (commandInfo == null)
						ConsoleCommandParse.Warn(" Syntax error.");
					else
						commandInfo.Action(game, ((IEnumerable<string>)parts).Skip<string>(1));
				}
			}
		}

		public static void ProcessConsoleCommands(App game, ConsoleApplet applet)
		{
			if (applet == null)
				return;
			ConsoleCommandParse.Evaluate(game, (IEnumerable<string>)applet.FlushCommands());
		}

		private class CommandInfo
		{
			public string[] Aliases;
			public string Description;
			public int ParameterCount;
			public Action<App, IEnumerable<string>> Action;
		}
	}
}
