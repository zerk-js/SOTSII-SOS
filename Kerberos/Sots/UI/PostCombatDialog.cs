// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.PostCombatDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class PostCombatDialog : Dialog
	{
		private int _systemID;
		private int _turn;
		private int _combatID;

		public PostCombatDialog(App game, int systemID, int combatID, int turn, string template = "CombatSummaryDialog")
		  : base(game, template)
		{
			this._turn = turn;
			this._systemID = systemID;
			this._combatID = combatID;
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked") || !(panelName == "okButton"))
				return;
			this._app.UI.CloseDialog((Dialog)this, true);
		}

		public override void Initialize()
		{
			CombatData combat = this._app.Game.CombatData.GetCombat(this._app.GameDatabase, this._combatID, this._systemID, this._turn);
			PlayerCombatData player1 = combat.GetPlayer(this._app.LocalPlayer.ID);
			if (player1 == null)
			{
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else
			{
				IEnumerable<PlayerInfo> playerInfos = this._app.GameDatabase.GetPlayerInfos();
				StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(combat.SystemID);
				string str1 = App.Localize("@ADMIRAL_LOCATION_DEEP_SPACE");
				if (starSystemInfo != (StarSystemInfo)null)
					str1 = starSystemInfo.Name;
				this._app.UI.SetPropertyString("summaryText", "text", "Combat - " + player1.VictoryStatus.ToString() + " at " + str1);
				int num1 = 0;
				int num2 = 0;
				float val2_1 = 0.0f;
				float val2_2 = 0.0f;
				double num3 = 0.0;
				double num4 = 0.0;
				float num5 = 0.0f;
				double num6 = 0.0;
				double num7 = 0.0;
				float num8 = 0.0f;
				int num9 = 0;
				int num10 = 0;
				Dictionary<int, float> dictionary1 = new Dictionary<int, float>();
				Dictionary<int, float> dictionary2 = new Dictionary<int, float>();
				foreach (PlayerInfo playerInfo in playerInfos)
				{
					PlayerInfo player = playerInfo;
					PlayerCombatData player2 = combat.GetPlayer(player.ID);
					if (player2 != null)
					{
						DiplomacyInfo diplomacyInfo = this._app.GameDatabase.GetDiplomacyInfo(player1.PlayerID, player.ID);
						string itemGlobalId1;
						if (diplomacyInfo.State == DiplomacyState.WAR)
						{
							this._app.UI.AddItem("enemiesAvatars", "", player.ID, "");
							itemGlobalId1 = this._app.UI.GetItemGlobalID("enemiesAvatars", "", player.ID, "");
						}
						else
						{
							this._app.UI.AddItem("alliesAvatars", "", player.ID, "");
							itemGlobalId1 = this._app.UI.GetItemGlobalID("alliesAvatars", "", player.ID, "");
						}
						PlayerSetup playerSetup = this._app.GameSetup.Players.FirstOrDefault<PlayerSetup>((Func<PlayerSetup, bool>)(x => x.databaseId == player.ID));
						string propertyValue = playerSetup == null ? player.Name : playerSetup.Name;
						this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "name"), "text", propertyValue);
						this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "playeravatar"), "texture", player.AvatarAssetPath);
						this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "badge"), "texture", player.BadgeAssetPath);
						this._app.UI.SetPropertyColorNormalized(this._app.UI.Path(itemGlobalId1, "primaryColor"), "color", player.PrimaryColor);
						this._app.UI.SetPropertyColorNormalized(this._app.UI.Path(itemGlobalId1, "secondaryColor"), "color", player.SecondaryColor);
						this._app.UI.AddItem("combatSummary", "", player.ID, "");
						string itemGlobalId2 = this._app.UI.GetItemGlobalID("combatSummary", "", player.ID, "");
						this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId2, "itemName"), "text", propertyValue);
						List<ShipData> shipData1 = player2.ShipData;
						List<WeaponData> weaponData1 = player2.WeaponData;
						List<PlanetData> planetData1 = player2.PlanetData;
						int count = shipData1.Count;
						int num11 = 0;
						float val1_1 = 0.0f;
						float val1_2 = 0.0f;
						Dictionary<int, string> dictionary3 = new Dictionary<int, string>();
						foreach (ShipData shipData2 in shipData1)
						{
							num11 += shipData2.killCount;
							val1_1 += shipData2.damageDealt;
							val1_2 += shipData2.damageReceived;
							if (diplomacyInfo.State == DiplomacyState.WAR)
								num10 += shipData2.destroyed ? 1 : 0;
							else
								num9 += shipData2.destroyed ? 1 : 0;
							if (!dictionary3.ContainsKey(shipData2.designID))
							{
								this._app.UI.AddItem(this._app.UI.Path(itemGlobalId2, "shipList"), "", shipData2.designID, "");
								dictionary3.Add(shipData2.designID, this._app.UI.GetItemGlobalID(this._app.UI.Path(itemGlobalId2, "shipList"), "", shipData2.designID, ""));
							}
						}
						foreach (WeaponData weaponData2 in weaponData1)
						{
							if (diplomacyInfo.State == DiplomacyState.WAR)
							{
								if (!dictionary2.ContainsKey(weaponData2.weaponID))
									dictionary2.Add(weaponData2.weaponID, 0.0f);
								Dictionary<int, float> dictionary4;
								int weaponId;
								(dictionary4 = dictionary2)[weaponId = weaponData2.weaponID] = dictionary4[weaponId] + weaponData2.damageDealt;
							}
							else
							{
								if (!dictionary1.ContainsKey(weaponData2.weaponID))
									dictionary1.Add(weaponData2.weaponID, 0.0f);
								Dictionary<int, float> dictionary4;
								int weaponId;
								(dictionary4 = dictionary1)[weaponId = weaponData2.weaponID] = dictionary4[weaponId] + weaponData2.damageDealt;
							}
						}
						foreach (PlanetData planetData2 in planetData1)
						{
							ColonyInfo colonyInfoForPlanet = this._app.GameDatabase.GetColonyInfoForPlanet(planetData2.orbitalObjectID);
							if (colonyInfoForPlanet != null)
							{
								if (colonyInfoForPlanet.PlayerID != this._app.LocalPlayer.ID)
								{
									num3 += planetData2.imperialDamage;
									num4 += planetData2.civilianDamage.Sum<PopulationData>((Func<PopulationData, double>)(x => x.damage));
									num5 += planetData2.infrastructureDamage;
								}
								else
								{
									num6 += planetData2.imperialDamage;
									num7 += planetData2.civilianDamage.Sum<PopulationData>((Func<PopulationData, double>)(x => x.damage));
									num8 += planetData2.infrastructureDamage;
								}
							}
						}
						foreach (int key in dictionary3.Keys)
						{
							int des = key;
							DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(des);
							int num12 = shipData1.Where<ShipData>((Func<ShipData, bool>)(x => x.designID == des)).Count<ShipData>();
							this._app.UI.SetSliderRange(this._app.UI.Path(dictionary3[des], "stotalUnits"), 0, count);
							this._app.UI.SetSliderValue(this._app.UI.Path(dictionary3[des], "stotalUnits"), num12);
							this._app.UI.SetPropertyString(this._app.UI.Path(dictionary3[des], "stotalUnitsLabel"), "text", num12.ToString());
							int num13 = shipData1.Where<ShipData>((Func<ShipData, bool>)(x => x.designID == des)).Sum<ShipData>((Func<ShipData, int>)(x => x.killCount));
							this._app.UI.SetSliderRange(this._app.UI.Path(dictionary3[des], "sdestroyedUnits"), 0, num11);
							this._app.UI.SetSliderValue(this._app.UI.Path(dictionary3[des], "sdestroyedUnits"), num13);
							this._app.UI.SetPropertyString(this._app.UI.Path(dictionary3[des], "sdestroyedUnitsLabel"), "text", num13.ToString());
							float num14 = shipData1.Where<ShipData>((Func<ShipData, bool>)(x => x.designID == des)).Sum<ShipData>((Func<ShipData, float>)(x => x.damageDealt));
							this._app.UI.SetSliderRange(this._app.UI.Path(dictionary3[des], "sdamageInflicted"), 0, (int)val1_1);
							this._app.UI.SetSliderValue(this._app.UI.Path(dictionary3[des], "sdamageInflicted"), (int)num14);
							this._app.UI.SetPropertyString(this._app.UI.Path(dictionary3[des], "sdamageInflictedLabel"), "text", num14.ToString("N0"));
							float num15 = shipData1.Where<ShipData>((Func<ShipData, bool>)(x => x.designID == des)).Sum<ShipData>((Func<ShipData, float>)(x => x.damageReceived));
							this._app.UI.SetSliderRange(this._app.UI.Path(dictionary3[des], "sdamageTaken"), 0, (int)val1_2);
							this._app.UI.SetSliderValue(this._app.UI.Path(dictionary3[des], "sdamageTaken"), (int)num15);
							this._app.UI.SetPropertyString(this._app.UI.Path(dictionary3[des], "sdamageTakenLabel"), "text", num15.ToString("N0"));
							this._app.UI.SetPropertyString(this._app.UI.Path(dictionary3[des], "subitem_label"), "text", designInfo.Name);
						}
						num1 = Math.Max(count, num1);
						num2 = Math.Max(num11, num2);
						val2_1 = Math.Max(val1_1, val2_1);
						val2_2 = Math.Max(val1_2, val2_2);
						this._app.UI.SetSliderRange(this._app.UI.Path(itemGlobalId2, "totalUnits"), 0, count);
						this._app.UI.SetSliderValue(this._app.UI.Path(itemGlobalId2, "totalUnits"), count);
						this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId2, "totalUnitsLabel"), "text", count.ToString());
						this._app.UI.SetSliderRange(this._app.UI.Path(itemGlobalId2, "destroyedUnits"), 0, num11);
						this._app.UI.SetSliderValue(this._app.UI.Path(itemGlobalId2, "destroyedUnits"), num11);
						this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId2, "destroyedUnitsLabel"), "text", num11.ToString());
						this._app.UI.SetSliderRange(this._app.UI.Path(itemGlobalId2, "damageInflicted"), 0, (int)val1_1);
						this._app.UI.SetSliderValue(this._app.UI.Path(itemGlobalId2, "damageInflicted"), (int)val1_1);
						this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId2, "damageInflictedLabel"), "text", val1_1.ToString("N0"));
						this._app.UI.SetSliderRange(this._app.UI.Path(itemGlobalId2, "damageTaken"), 0, (int)val1_2);
						this._app.UI.SetSliderValue(this._app.UI.Path(itemGlobalId2, "damageTaken"), (int)val1_2);
						this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId2, "damageTakenLabel"), "text", val1_2.ToString("N0"));
					}
				}
				foreach (PlayerInfo playerInfo in playerInfos)
				{
					if (combat.GetPlayer(playerInfo.ID) != null)
					{
						string itemGlobalId = this._app.UI.GetItemGlobalID("combatSummary", "", playerInfo.ID, "");
						this._app.UI.SetSliderRange(this._app.UI.Path(itemGlobalId, "totalUnits"), 0, num1);
						this._app.UI.SetSliderRange(this._app.UI.Path(itemGlobalId, "destroyedUnits"), 0, num2);
						this._app.UI.SetSliderRange(this._app.UI.Path(itemGlobalId, "damageInflicted"), 0, (int)val2_1);
						this._app.UI.SetSliderRange(this._app.UI.Path(itemGlobalId, "damageTaken"), 0, (int)val2_2);
					}
				}
				foreach (int key in dictionary1.Keys)
				{
					int weapon = key;
					this._app.UI.AddItem("alliedWeaponList", "", weapon, "");
					string itemGlobalId = this._app.UI.GetItemGlobalID("alliedWeaponList", "", weapon, "");
					this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "damageDealt"), "text", dictionary1[weapon].ToString("N0"));
					LogicalWeapon logicalWeapon = this._app.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.UniqueWeaponID == weapon));
					string iconSpriteName = logicalWeapon.IconSpriteName;
					this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "weaponIcon"), "sprite", iconSpriteName);
					this._app.UI.SetPropertyString(itemGlobalId, "tooltip", logicalWeapon.WeaponName);
				}
				foreach (int key in dictionary2.Keys)
				{
					int weapon = key;
					this._app.UI.AddItem("enemyWeaponList", "", weapon, "");
					string itemGlobalId = this._app.UI.GetItemGlobalID("enemyWeaponList", "", weapon, "");
					this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "damageDealt"), "text", dictionary2[weapon].ToString("N0"));
					string iconSpriteName = this._app.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.UniqueWeaponID == weapon)).IconSpriteName;
					this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "weaponIcon"), "sprite", iconSpriteName);
				}
				int num16 = 0;
				string text1 = "";
				if (num7 > 0.0 && num6 > 0.0)
					text1 = text1 + num6.ToString("N0") + " Imperialists and " + num7.ToString("N0") + " civilians lost. ";
				else if (num7 > 0.0)
					text1 = text1 + num7.ToString("N0") + " civilians lost. ";
				else if (num6 > 0.0)
					text1 = text1 + num6.ToString("N0") + " Imperialists lost. ";
				if (num4 > 0.0 && num3 > 0.0)
					text1 = text1 + num3.ToString("N0") + " enemy Imperialists and " + num4.ToString("N0") + " enemy civilians killed.";
				else if (num4 > 0.0)
					text1 = text1 + num4.ToString("N0") + " enemy civilians killed.";
				else if (num3 > 0.0)
					text1 = text1 + num3.ToString("N0") + " enemy Imperialists killed.";
				if (text1.Length > 0)
					this._app.UI.AddItem("happenings", "", num16++, text1);
				string text2 = "";
				float num17;
				if ((double)num8 > 0.01)
				{
					string str2 = text2;
					num17 = num8 * 100f;
					string str3 = num17.ToString("#0.00");
					text2 = str2 + "Infrastructure reduced by " + str3 + "%";
				}
				if ((double)num5 > 0.01)
				{
					string str2 = text2;
					num17 = num5 * 100f;
					string str3 = num17.ToString("#0.00");
					text2 = str2 + " Enemy infrastructure reduced by " + str3 + "%";
				}
				if (text2.Length > 0)
					this._app.UI.AddItem("happenings", "", num16++, text2);
				string str4 = "";
				if (num9 > 0)
					str4 = num9 <= 1 ? str4 + num9.ToString() + " friendly ship lost. " : str4 + num9.ToString() + " friendly ships lost. ";
				if (num10 > 0)
					str4 = num9 <= 1 ? str4 + num10.ToString() + " enemy ship destroyed." : str4 + num10.ToString() + " enemy ships destroyed.";
				if (str4.Length <= 0)
					return;
				UICommChannel ui = this._app.UI;
				int userItemId = num16;
				int num18 = userItemId + 1;
				string text3 = str4;
				ui.AddItem("happenings", "", userItemId, text3);
			}
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
