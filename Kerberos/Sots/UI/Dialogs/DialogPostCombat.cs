// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.Dialogs.DialogPostCombat
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI.Dialogs
{
	internal class DialogPostCombat : Dialog
	{
		private int _systemID;
		private int _turn;
		private int _combatID;
		private List<PlanetWidget> _PlayerSideplanetWidgets;
		private List<PlanetWidget> _EnemySideplanetWidgets;
		private List<int> EnemyPlayers;

		public DialogPostCombat(App game, int systemID, int combatID, int turn, string template = "postCombat")
		  : base(game, template)
		{
			this._turn = turn;
			this._systemID = systemID;
			this._combatID = combatID;
			this._PlayerSideplanetWidgets = new List<PlanetWidget>();
			this._EnemySideplanetWidgets = new List<PlanetWidget>();
			this.EnemyPlayers = new List<int>();
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked"))
				return;
			if (panelName == "btnOK")
				this._app.UI.CloseDialog((Dialog)this, true);
			if (!panelName.StartsWith("avatarButton"))
				return;
			string[] strArray = panelName.Split('|');
			if (((IEnumerable<string>)strArray).Count<string>() != 3)
				return;
			bool flag = strArray[1] == "A";
			int playerid = int.Parse(strArray[2]);
			if (flag)
				this.SyncPlayerSide(playerid);
			else
				this.SyncEnemySide(playerid);
		}

		public override void Initialize()
		{
			CombatData combat = this._app.Game.CombatData.GetCombat(this._app.GameDatabase, this._combatID, this._systemID, this._turn);
			PlayerCombatData player = combat.GetPlayer(this._app.LocalPlayer.ID);
			if (player == null)
			{
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else
			{
				this._app.GameDatabase.GetPlayerInfos();
				StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(combat.SystemID);
				string str = App.Localize("@ADMIRAL_LOCATION_DEEP_SPACE");
				if (starSystemInfo != (StarSystemInfo)null)
					str = starSystemInfo.Name;
				this._app.UI.SetPropertyString("title", "text", "Combat - " + player.VictoryStatus.ToString() + " at " + str);
				this.syncCombatantsLists();
				this.SyncPlayerSide(player.PlayerID);
				if (this.EnemyPlayers.Any<int>())
					this.SyncEnemySide(this.EnemyPlayers.First<int>());
				StarSystemMapUI.Sync(this._app, this._systemID, "systemMapContent", false);
			}
		}

		public void syncCombatantsLists()
		{
			CombatData combat = this._app.Game.CombatData.GetCombat(this._app.GameDatabase, this._combatID, this._systemID, this._turn);
			PlayerCombatData player1 = combat.GetPlayer(this._app.LocalPlayer.ID);
			foreach (PlayerInfo playerInfo in this._app.GameDatabase.GetPlayerInfos())
			{
				PlayerInfo player = playerInfo;
				if (combat.GetPlayer(player.ID) != null)
				{
					DiplomacyInfo diplomacyInfo = this._app.GameDatabase.GetDiplomacyInfo(player1.PlayerID, player.ID);
					string itemGlobalId;
					if (diplomacyInfo.State == DiplomacyState.WAR)
					{
						this._app.UI.AddItem("enemiesList", "", player.ID, "");
						itemGlobalId = this._app.UI.GetItemGlobalID("enemiesList", "", player.ID, "");
						this.EnemyPlayers.Add(player.ID);
					}
					else
					{
						this._app.UI.AddItem("alliesList", "", player.ID, "");
						itemGlobalId = this._app.UI.GetItemGlobalID("alliesList", "", player.ID, "");
					}
					PlayerSetup playerSetup = this._app.GameSetup.Players.FirstOrDefault<PlayerSetup>((Func<PlayerSetup, bool>)(x => x.databaseId == player.ID));
					string propertyValue = playerSetup == null || !(playerSetup.Name != "") ? player.Name : playerSetup.Name;
					this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "name"), "text", propertyValue);
					this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "playeravatar"), "texture", player.AvatarAssetPath);
					this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "badge"), "texture", player.BadgeAssetPath);
					this._app.UI.SetPropertyColorNormalized(this._app.UI.Path(itemGlobalId, "primaryColor"), "color", player.PrimaryColor);
					this._app.UI.SetPropertyColorNormalized(this._app.UI.Path(itemGlobalId, "secondaryColor"), "color", player.SecondaryColor);
					this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "avatarButton"), "id", "avatarButton|" + (diplomacyInfo.State == DiplomacyState.WAR ? "E|" : "A|") + player.ID.ToString());
				}
			}
		}

		public void SyncPlayerSide(int playerid)
		{
			CombatData combat = this._app.Game.CombatData.GetCombat(this._app.GameDatabase, this._combatID, this._systemID, this._turn);
			PlayerCombatData player1 = combat.GetPlayer(this._app.LocalPlayer.ID);
			if (player1 == null)
			{
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else
			{
				foreach (PlanetWidget sideplanetWidget in this._PlayerSideplanetWidgets)
					sideplanetWidget.Terminate();
				this._PlayerSideplanetWidgets.Clear();
				IEnumerable<PlayerInfo> playerInfos = this._app.GameDatabase.GetPlayerInfos();
				StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(combat.SystemID);
				App.Localize("@ADMIRAL_LOCATION_DEEP_SPACE");
				if (starSystemInfo != (StarSystemInfo)null)
				{
					string name = starSystemInfo.Name;
				}
				int val2_1 = 0;
				int val2_2 = 0;
				float val2_3 = 0.0f;
				float val2_4 = 0.0f;
				double num1 = 0.0;
				double num2 = 0.0;
				float num3 = 0.0f;
				int num4 = 0;
				Dictionary<int, float> dictionary1 = new Dictionary<int, float>();
				PlayerInfo player = playerInfos.FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID == playerid));
				PlayerCombatData player2 = combat.GetPlayer(player.ID);
				if (player2 != null)
				{
					this._app.GameDatabase.GetDiplomacyInfo(player1.PlayerID, player.ID);
					PlayerSetup playerSetup = this._app.GameSetup.Players.FirstOrDefault<PlayerSetup>((Func<PlayerSetup, bool>)(x => x.databaseId == player.ID));
					string propertyValue1 = playerSetup == null || !(playerSetup.Name != "") || playerSetup.AI ? player.Name : playerSetup.Name;
					this._app.UI.SetPropertyString(this._app.UI.Path("playerSide", "alliesAvatars", "name"), "text", propertyValue1);
					this._app.UI.SetPropertyString(this._app.UI.Path("playerSide", "alliesAvatars", "playeravatar"), "texture", player.AvatarAssetPath);
					this._app.UI.SetPropertyString(this._app.UI.Path("playerSide", "alliesAvatars", "badge"), "texture", player.BadgeAssetPath);
					this._app.UI.SetPropertyColorNormalized(this._app.UI.Path("playerSide", "alliesAvatars", "primaryColor"), "color", player.PrimaryColor);
					this._app.UI.SetPropertyColorNormalized(this._app.UI.Path("playerSide", "alliesAvatars", "secondaryColor"), "color", player.SecondaryColor);
					this._app.UI.SetPropertyColorNormalized(this._app.UI.Path("playerSide", "empireColor"), "color", player.PrimaryColor);
					this._app.UI.SetPropertyString(this._app.UI.Path("playerSide", "playerName"), "text", propertyValue1);
					List<ShipData> shipData1 = player2.ShipData;
					List<WeaponData> weaponData1 = player2.WeaponData;
					List<PlanetData> planetData1 = player2.PlanetData;
					int count = shipData1.Count;
					int val1_1 = 0;
					float val1_2 = 0.0f;
					float val1_3 = 0.0f;
					Dictionary<int, string> dictionary2 = new Dictionary<int, string>();
					int userItemId1 = 0;
					this._app.UI.ClearItems(this._app.UI.Path("playerSide", "fleetDamage"));
					foreach (ShipData shipData2 in shipData1)
					{
						val1_1 += shipData2.killCount;
						val1_2 += shipData2.damageDealt;
						val1_3 += shipData2.damageReceived;
						num4 += shipData2.destroyed ? 1 : 0;
						DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(shipData2.designID);
						if (designInfo != null)
						{
							RealShipClasses? realShipClass = designInfo.GetRealShipClass();
							if ((realShipClass.GetValueOrDefault() != RealShipClasses.AssaultShuttle ? 1 : (!realShipClass.HasValue ? 1 : 0)) != 0)
							{
								realShipClass = designInfo.GetRealShipClass();
								if ((realShipClass.GetValueOrDefault() != RealShipClasses.Drone ? 1 : (!realShipClass.HasValue ? 1 : 0)) != 0)
								{
									realShipClass = designInfo.GetRealShipClass();
									if ((realShipClass.GetValueOrDefault() != RealShipClasses.BoardingPod ? 1 : (!realShipClass.HasValue ? 1 : 0)) != 0 && this._app.Game.ScriptModules.MeteorShower.PlayerID != player.ID)
									{
										string propertyValue2 = "";
										if (shipData2.destroyed)
											propertyValue2 = designInfo.Name + " class ship has been destroyed.";
										else if ((double)shipData2.damageReceived > 0.0)
											propertyValue2 = designInfo.Name + " class ship has been damaged.";
										if (propertyValue2 != "")
										{
											this._app.UI.AddItem(this._app.UI.Path("playerSide", "fleetDamage"), "", userItemId1, "");
											this._app.UI.SetPropertyString(this._app.UI.Path(this._app.UI.GetItemGlobalID(this._app.UI.Path("playerSide", "fleetDamage"), "", userItemId1, ""), "name"), "text", propertyValue2);
											++userItemId1;
										}
									}
								}
							}
						}
					}
					this._app.UI.SetPropertyString(this._app.UI.Path("playerSide", "playerData", "playerScore"), "text", (shipData1.Count - num4).ToString() + "/" + shipData1.Count.ToString());
					this._app.UI.SetSliderRange(this._app.UI.Path("playerSide", "playerData", "assets"), 0, shipData1.Count);
					this._app.UI.SetSliderValue(this._app.UI.Path("playerSide", "playerData", "assets"), shipData1.Count - num4);
					foreach (WeaponData weaponData2 in weaponData1)
					{
						if (!dictionary1.ContainsKey(weaponData2.weaponID))
							dictionary1.Add(weaponData2.weaponID, 0.0f);
						Dictionary<int, float> dictionary3;
						int weaponId;
						(dictionary3 = dictionary1)[weaponId = weaponData2.weaponID] = dictionary3[weaponId] + weaponData2.damageDealt;
					}
					this._app.UI.ClearItems(this._app.UI.Path("playerSide", "weaponDamage"));
					int num5 = 0;
					int userItemId2 = 0;
					string str = null;
					foreach (int key in dictionary1.Keys)
					{
						int weapon = key;
						if (num5 == 5 || str == null)
						{
							this._app.UI.AddItem(this._app.UI.Path("playerSide", "weaponDamage"), "", userItemId2, "");
							str = this._app.UI.GetItemGlobalID(this._app.UI.Path("playerSide", "weaponDamage"), "", userItemId2, "");
							++userItemId2;
							num5 = 0;
							for (int index = 0; index < 5; ++index)
								this._app.UI.SetVisible(this._app.UI.Path(str, "weapon" + index.ToString()), false);
						}
						this._app.UI.SetPropertyString(this._app.UI.Path(str, "weapon" + num5.ToString(), "damageDealt"), "text", dictionary1[weapon].ToString("N0"));
						LogicalWeapon logicalWeapon = this._app.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.UniqueWeaponID == weapon));
						string iconSpriteName = logicalWeapon.IconSpriteName;
						this._app.UI.SetPropertyString(this._app.UI.Path(str, "weapon" + num5.ToString(), "weaponIcon"), "sprite", iconSpriteName);
						this._app.UI.SetPropertyString(this._app.UI.Path(str, "weapon" + num5.ToString()), "tooltip", logicalWeapon.WeaponName);
						this._app.UI.SetVisible(this._app.UI.Path(str, "weapon" + num5.ToString()), true);
						++num5;
					}
					this._app.UI.ClearItems(this._app.UI.Path("playerSide", "planetDamage"));
					foreach (PlanetData planetData2 in planetData1)
					{
						ColonyInfo colonyInfoForPlanet = this._app.GameDatabase.GetColonyInfoForPlanet(planetData2.orbitalObjectID);
						if (colonyInfoForPlanet != null)
						{
							num1 += planetData2.imperialDamage;
							num2 += planetData2.civilianDamage.Sum<PopulationData>((Func<PopulationData, double>)(x => x.damage));
							num3 += planetData2.infrastructureDamage;
							this._app.UI.AddItem(this._app.UI.Path("playerSide", "planetDamage"), "", colonyInfoForPlanet.ID, "");
							string itemGlobalId = this._app.UI.GetItemGlobalID(this._app.UI.Path("playerSide", "planetDamage"), "", colonyInfoForPlanet.ID, "");
							OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(colonyInfoForPlanet.OrbitalObjectID);
							PlanetInfo planetInfo = this._app.GameDatabase.GetPlanetInfo(colonyInfoForPlanet.OrbitalObjectID);
							Faction faction = this._app.AssetDatabase.GetFaction(this._app.GameDatabase.GetPlayerFactionID(colonyInfoForPlanet.PlayerID));
							double civilianPopulation = this._app.GameDatabase.GetCivilianPopulation(colonyInfoForPlanet.OrbitalObjectID, faction.ID, faction.HasSlaves());
							float num6 = planetInfo != null ? planetInfo.Infrastructure : 0.0f;
							this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "planetName"), "text", orbitalObjectInfo != null ? orbitalObjectInfo.Name : "?");
							this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "civslbl"), "text", civilianPopulation.ToString("N0"));
							this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "implbl"), "text", colonyInfoForPlanet.ImperialPop.ToString("N0"));
							this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "infralbl"), "text", num6.ToString());
							double num7 = civilianPopulation + planetData2.civilianDamage.Sum<PopulationData>((Func<PopulationData, double>)(x => x.damage));
							this._app.UI.SetSliderRange(this._app.UI.Path(itemGlobalId, "civAmount"), 0, (int)num7);
							this._app.UI.SetSliderValue(this._app.UI.Path(itemGlobalId, "civAmount"), (int)(num7 - planetData2.civilianDamage.Sum<PopulationData>((Func<PopulationData, double>)(x => x.damage))));
							double num8 = colonyInfoForPlanet.ImperialPop + planetData2.imperialDamage;
							this._app.UI.SetSliderRange(this._app.UI.Path(itemGlobalId, "impAmount"), 0, (int)num8);
							this._app.UI.SetSliderValue(this._app.UI.Path(itemGlobalId, "impAmount"), (int)(num8 - planetData2.imperialDamage));
							float num9 = num6 + planetData2.infrastructureDamage;
							this._app.UI.SetSliderRange(this._app.UI.Path(itemGlobalId, "infraAmount"), 0, (int)(100.0 * (double)num9));
							this._app.UI.SetSliderValue(this._app.UI.Path(itemGlobalId, "infraAmount"), (int)(100.0 * ((double)num9 - (double)planetData2.infrastructureDamage)));
							if (planetInfo != null)
							{
								this._PlayerSideplanetWidgets.Add(new PlanetWidget(this._app, itemGlobalId));
								this._PlayerSideplanetWidgets.Last<PlanetWidget>().Sync(planetInfo.ID, false, false);
							}
						}
					}
					Math.Max(count, val2_1);
					Math.Max(val1_1, val2_2);
					Math.Max(val1_2, val2_3);
					Math.Max(val1_3, val2_4);
				}
				this._app.UI.AutoSizeContents(this._app.UI.Path(this.ID, "playerSide"));
			}
		}

		protected override void OnUpdate()
		{
			foreach (PlanetWidget sideplanetWidget in this._PlayerSideplanetWidgets)
				sideplanetWidget.Update();
			foreach (PlanetWidget sideplanetWidget in this._EnemySideplanetWidgets)
				sideplanetWidget.Update();
		}

		public void SyncEnemySide(int playerid)
		{
			CombatData combat = this._app.Game.CombatData.GetCombat(this._app.GameDatabase, this._combatID, this._systemID, this._turn);
			PlayerCombatData player1 = combat.GetPlayer(this._app.LocalPlayer.ID);
			if (player1 == null)
			{
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else
			{
				foreach (PlanetWidget sideplanetWidget in this._EnemySideplanetWidgets)
					sideplanetWidget.Terminate();
				this._EnemySideplanetWidgets.Clear();
				IEnumerable<PlayerInfo> playerInfos = this._app.GameDatabase.GetPlayerInfos();
				StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(combat.SystemID);
				App.Localize("@ADMIRAL_LOCATION_DEEP_SPACE");
				if (starSystemInfo != (StarSystemInfo)null)
				{
					string name = starSystemInfo.Name;
				}
				int val2_1 = 0;
				int val2_2 = 0;
				float val2_3 = 0.0f;
				float val2_4 = 0.0f;
				double num1 = 0.0;
				double num2 = 0.0;
				float num3 = 0.0f;
				int num4 = 0;
				Dictionary<int, float> dictionary1 = new Dictionary<int, float>();
				PlayerInfo player = playerInfos.FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID == playerid));
				PlayerCombatData player2 = combat.GetPlayer(player.ID);
				if (player2 != null)
				{
					this._app.GameDatabase.GetDiplomacyInfo(player1.PlayerID, player.ID);
					PlayerSetup playerSetup = this._app.GameSetup.Players.FirstOrDefault<PlayerSetup>((Func<PlayerSetup, bool>)(x => x.databaseId == player.ID));
					string propertyValue1 = playerSetup == null || !(playerSetup.Name != "") || playerSetup.AI ? player.Name : playerSetup.Name;
					this._app.UI.SetPropertyString(this._app.UI.Path("enemySide", "alliesAvatars", "name"), "text", propertyValue1);
					this._app.UI.SetPropertyString(this._app.UI.Path("enemySide", "alliesAvatars", "playeravatar"), "texture", player.AvatarAssetPath);
					this._app.UI.SetPropertyString(this._app.UI.Path("enemySide", "alliesAvatars", "badge"), "texture", player.BadgeAssetPath);
					this._app.UI.SetPropertyColorNormalized(this._app.UI.Path("enemySide", "alliesAvatars", "primaryColor"), "color", player.PrimaryColor);
					this._app.UI.SetPropertyColorNormalized(this._app.UI.Path("enemySide", "alliesAvatars", "secondaryColor"), "color", player.SecondaryColor);
					this._app.UI.SetPropertyColorNormalized(this._app.UI.Path("enemySide", "empireColor"), "color", player.PrimaryColor);
					this._app.UI.SetPropertyString(this._app.UI.Path("enemySide", "playerData", "playerName"), "text", propertyValue1);
					List<ShipData> shipData1 = player2.ShipData;
					List<WeaponData> weaponData1 = player2.WeaponData;
					List<PlanetData> planetData1 = player2.PlanetData;
					int count = shipData1.Count;
					int val1_1 = 0;
					float val1_2 = 0.0f;
					float val1_3 = 0.0f;
					Dictionary<int, string> dictionary2 = new Dictionary<int, string>();
					int userItemId1 = 0;
					this._app.UI.ClearItems(this._app.UI.Path("enemySide", "fleetDamage"));
					foreach (ShipData shipData2 in shipData1)
					{
						val1_1 += shipData2.killCount;
						val1_2 += shipData2.damageDealt;
						val1_3 += shipData2.damageReceived;
						num4 += shipData2.destroyed ? 1 : 0;
						DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(shipData2.designID);
						if (designInfo != null)
						{
							RealShipClasses? realShipClass = designInfo.GetRealShipClass();
							if ((realShipClass.GetValueOrDefault() != RealShipClasses.AssaultShuttle ? 1 : (!realShipClass.HasValue ? 1 : 0)) != 0)
							{
								realShipClass = designInfo.GetRealShipClass();
								if ((realShipClass.GetValueOrDefault() != RealShipClasses.Drone ? 1 : (!realShipClass.HasValue ? 1 : 0)) != 0)
								{
									realShipClass = designInfo.GetRealShipClass();
									if ((realShipClass.GetValueOrDefault() != RealShipClasses.BoardingPod ? 1 : (!realShipClass.HasValue ? 1 : 0)) != 0 && this._app.Game.ScriptModules.MeteorShower.PlayerID != player.ID)
									{
										string propertyValue2 = "";
										if (shipData2.destroyed)
											propertyValue2 = designInfo.Name + " class ship has been destroyed.";
										else if ((double)shipData2.damageReceived > 0.0)
											propertyValue2 = designInfo.Name + " class ship has been damaged.";
										if (propertyValue2 != "")
										{
											this._app.UI.AddItem(this._app.UI.Path("enemySide", "fleetDamage"), "", userItemId1, "");
											this._app.UI.SetPropertyString(this._app.UI.Path(this._app.UI.GetItemGlobalID(this._app.UI.Path("enemySide", "fleetDamage"), "", userItemId1, ""), "name"), "text", propertyValue2);
											++userItemId1;
										}
									}
								}
							}
						}
					}
					this._app.UI.SetPropertyString(this._app.UI.Path("enemySide", "playerData", "playerScore"), "text", (shipData1.Count - num4).ToString() + "/" + shipData1.Count.ToString());
					this._app.UI.SetSliderRange(this._app.UI.Path("enemySide", "playerData", "assets"), 0, shipData1.Count);
					this._app.UI.SetSliderValue(this._app.UI.Path("enemySide", "playerData", "assets"), shipData1.Count - num4);
					foreach (WeaponData weaponData2 in weaponData1)
					{
						if (!dictionary1.ContainsKey(weaponData2.weaponID))
							dictionary1.Add(weaponData2.weaponID, 0.0f);
						Dictionary<int, float> dictionary3;
						int weaponId;
						(dictionary3 = dictionary1)[weaponId = weaponData2.weaponID] = dictionary3[weaponId] + weaponData2.damageDealt;
					}
					this._app.UI.ClearItems(this._app.UI.Path("enemySide", "weaponDamage"));
					int num5 = 0;
					int userItemId2 = 0;
					string str = null;
					foreach (int key in dictionary1.Keys)
					{
						int weapon = key;
						if (num5 == 5 || str == null)
						{
							this._app.UI.AddItem(this._app.UI.Path("enemySide", "weaponDamage"), "", userItemId2, "");
							str = this._app.UI.GetItemGlobalID(this._app.UI.Path("enemySide", "weaponDamage"), "", userItemId2, "");
							++userItemId2;
							num5 = 0;
							for (int index = 0; index < 5; ++index)
								this._app.UI.SetVisible(this._app.UI.Path(str, "weapon" + index.ToString()), false);
						}
						this._app.UI.SetPropertyString(this._app.UI.Path(str, "weapon" + num5.ToString(), "damageDealt"), "text", dictionary1[weapon].ToString("N0"));
						LogicalWeapon logicalWeapon = this._app.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.UniqueWeaponID == weapon));
						string iconSpriteName = logicalWeapon.IconSpriteName;
						this._app.UI.SetPropertyString(this._app.UI.Path(str, "weapon" + num5.ToString(), "weaponIcon"), "sprite", iconSpriteName);
						this._app.UI.SetPropertyString(this._app.UI.Path(str, "weapon" + num5.ToString()), "tooltip", logicalWeapon.WeaponName);
						this._app.UI.SetVisible(this._app.UI.Path(str, "weapon" + num5.ToString()), true);
						++num5;
					}
					this._app.UI.ClearItems(this._app.UI.Path("enemySide", "planetDamage"));
					foreach (PlanetData planetData2 in planetData1)
					{
						ColonyInfo colonyInfoForPlanet = this._app.GameDatabase.GetColonyInfoForPlanet(planetData2.orbitalObjectID);
						if (colonyInfoForPlanet != null)
						{
							num1 += planetData2.imperialDamage;
							num2 += planetData2.civilianDamage.Sum<PopulationData>((Func<PopulationData, double>)(x => x.damage));
							num3 += planetData2.infrastructureDamage;
							this._app.UI.AddItem(this._app.UI.Path("enemySide", "planetDamage"), "", colonyInfoForPlanet.ID, "");
							string itemGlobalId = this._app.UI.GetItemGlobalID(this._app.UI.Path("enemySide", "planetDamage"), "", colonyInfoForPlanet.ID, "");
							OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(colonyInfoForPlanet.OrbitalObjectID);
							PlanetInfo planetInfo = this._app.GameDatabase.GetPlanetInfo(colonyInfoForPlanet.OrbitalObjectID);
							Faction faction = this._app.AssetDatabase.GetFaction(this._app.GameDatabase.GetPlayerFactionID(colonyInfoForPlanet.PlayerID));
							double civilianPopulation = this._app.GameDatabase.GetCivilianPopulation(colonyInfoForPlanet.OrbitalObjectID, faction.ID, faction.HasSlaves());
							float num6 = planetInfo != null ? planetInfo.Infrastructure : 0.0f;
							this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "planetName"), "text", orbitalObjectInfo != null ? orbitalObjectInfo.Name : "?");
							this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "civslbl"), "text", civilianPopulation.ToString("N0"));
							this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "implbl"), "text", colonyInfoForPlanet.ImperialPop.ToString("N0"));
							this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "infralbl"), "text", num6.ToString());
							double num7 = civilianPopulation + planetData2.civilianDamage.Sum<PopulationData>((Func<PopulationData, double>)(x => x.damage));
							this._app.UI.SetSliderRange(this._app.UI.Path(itemGlobalId, "civAmount"), 0, (int)num7);
							this._app.UI.SetSliderValue(this._app.UI.Path(itemGlobalId, "civAmount"), (int)(num7 - planetData2.civilianDamage.Sum<PopulationData>((Func<PopulationData, double>)(x => x.damage))));
							double num8 = colonyInfoForPlanet.ImperialPop + planetData2.imperialDamage;
							this._app.UI.SetSliderRange(this._app.UI.Path(itemGlobalId, "impAmount"), 0, (int)num8);
							this._app.UI.SetSliderValue(this._app.UI.Path(itemGlobalId, "impAmount"), (int)(num8 - planetData2.imperialDamage));
							float num9 = num6 + planetData2.infrastructureDamage;
							this._app.UI.SetSliderRange(this._app.UI.Path(itemGlobalId, "infraAmount"), 0, (int)(100.0 * (double)num9));
							this._app.UI.SetSliderValue(this._app.UI.Path(itemGlobalId, "infraAmount"), (int)(100.0 * ((double)num9 - (double)planetData2.infrastructureDamage)));
							if (planetInfo != null)
							{
								this._EnemySideplanetWidgets.Add(new PlanetWidget(this._app, itemGlobalId));
								this._EnemySideplanetWidgets.Last<PlanetWidget>().Sync(planetInfo.ID, false, false);
							}
						}
					}
					Math.Max(count, val2_1);
					Math.Max(val1_1, val2_2);
					Math.Max(val1_2, val2_3);
					Math.Max(val1_3, val2_4);
				}
				this._app.UI.AutoSizeContents(this._app.UI.Path(this.ID, "enemySide"));
			}
		}

		public override string[] CloseDialog()
		{
			foreach (PlanetWidget sideplanetWidget in this._PlayerSideplanetWidgets)
				sideplanetWidget.Terminate();
			this._PlayerSideplanetWidgets.Clear();
			foreach (PlanetWidget sideplanetWidget in this._EnemySideplanetWidgets)
				sideplanetWidget.Terminate();
			this._EnemySideplanetWidgets.Clear();
			return (string[])null;
		}
	}
}
