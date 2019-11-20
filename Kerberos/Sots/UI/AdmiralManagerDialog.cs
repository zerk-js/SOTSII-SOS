// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.AdmiralManagerDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class AdmiralManagerDialog : Dialog
	{
		private List<int> _buttonMap = new List<int>();
		public const string OKButton = "okButton";
		public const string AdmiralList = "pnlAdmirals.admiralList";
		private int _playerID;
		private int _fleetToDissolve;
		private int _fleetToCancel;
		private int _currentSystemID;
		private int _currentAdmiralID;
		private int _currentShipID;
		private int _currentDesignID;
		private bool ComposeFleet;
		private string _confirmFleetDisolveDialog;
		private string _confirmCancelMissionDialog;
		private string _confirmCreateAdmiralEngram;
		private string _nameFleetDialog;
		private string _transfercubesDialog;
		private AdmiralManagerDialog.AdmiralFilterMode _currentFilterMode;

		public AdmiralManagerDialog(
		  App game,
		  int playerID,
		  int targetSystem,
		  bool composefleet = false,
		  string template = "AdmiralManagerDialog")
		  : base(game, template)
		{
			this._playerID = playerID;
			this._currentSystemID = targetSystem;
			this.ComposeFleet = composefleet;
		}

		public override void Initialize()
		{
			this.PopulateFilters();
			this.RefreshDisplay();
		}

		private void PopulateFilters()
		{
			this._app.UI.AddItem(this._app.UI.Path(this.ID, "filterDropdown"), "", 0, App.Localize("@ADMIRAL_DIALOG_NONE"));
			this._app.UI.AddItem(this._app.UI.Path(this.ID, "filterDropdown"), "", 1, App.Localize("@ADMIRAL_DIALOG_AGE"));
			this._app.UI.AddItem(this._app.UI.Path(this.ID, "filterDropdown"), "", 2, App.Localize("@ADMIRAL_DIALOG_NAME"));
			this._app.UI.AddItem(this._app.UI.Path(this.ID, "filterDropdown"), "", 3, App.Localize("@ADMIRAL_DIALOG_LOCATION"));
			this._app.UI.AddItem(this._app.UI.Path(this.ID, "filterDropdown"), "", 4, App.Localize("@ADMIRAL_DIALOG_FLEET"));
			this._app.UI.SetSelection(this._app.UI.Path(this.ID, "filterDropdown"), 0);
		}

		private string GetLocation(int admiralID)
		{
			string str = App.Localize("@ADMIRAL_LOCATION_DEEP_SPACE");
			FleetInfo fleetInfoByAdmiralId = this._app.GameDatabase.GetFleetInfoByAdmiralID(admiralID, FleetType.FL_NORMAL);
			if (fleetInfoByAdmiralId != null)
			{
				StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(fleetInfoByAdmiralId.SystemID);
				if (starSystemInfo != (StarSystemInfo)null)
					str = starSystemInfo.Name;
			}
			return str;
		}

		private string GetFleetName(int admiralID)
		{
			string str = "zzzzzzzzzzzzzz";
			FleetInfo fleetInfoByAdmiralId = this._app.GameDatabase.GetFleetInfoByAdmiralID(admiralID, FleetType.FL_NORMAL);
			if (fleetInfoByAdmiralId != null)
				str = fleetInfoByAdmiralId.Name;
			return str;
		}

		public void RefreshDisplay()
		{
			List<AdmiralInfo> list1 = this._app.GameDatabase.GetAdmiralInfosForPlayer(this._playerID).ToList<AdmiralInfo>();
			IEnumerable<SuulkaInfo> playerSuulkas = this._app.GameDatabase.GetPlayerSuulkas(new int?(this._playerID));
			foreach (SuulkaInfo suulkaInfo in playerSuulkas)
			{
				if (suulkaInfo.AdmiralID != 0)
					list1.Add(this._app.GameDatabase.GetAdmiralInfo(suulkaInfo.AdmiralID));
			}
			List<AdmiralInfo> list2 = list1.ToList<AdmiralInfo>();
			switch (this._currentFilterMode)
			{
				case AdmiralManagerDialog.AdmiralFilterMode.Age:
					list2 = list2.OrderBy<AdmiralInfo, float>((Func<AdmiralInfo, float>)(x => x.Age)).ToList<AdmiralInfo>();
					break;
				case AdmiralManagerDialog.AdmiralFilterMode.Name:
					list2 = list2.OrderBy<AdmiralInfo, string>((Func<AdmiralInfo, string>)(x => x.Name)).ToList<AdmiralInfo>();
					break;
				case AdmiralManagerDialog.AdmiralFilterMode.Location:
					list2 = list2.OrderBy<AdmiralInfo, string>((Func<AdmiralInfo, string>)(x => this.GetLocation(x.ID))).ToList<AdmiralInfo>();
					break;
				case AdmiralManagerDialog.AdmiralFilterMode.Fleet:
					list2 = list2.OrderBy<AdmiralInfo, string>((Func<AdmiralInfo, string>)(x => this.GetFleetName(x.ID))).ToList<AdmiralInfo>();
					break;
			}
			this._app.UI.ClearItemsTopLayer(this._app.UI.Path(this.ID, "pnlAdmirals.admiralList"));
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "currentNumAdmirals"), "text", string.Format(App.Localize("@NUMBER_OF_ADMIRALS"), (object)list2.Count<AdmiralInfo>().ToString(), (object)GameSession.GetPlayerMaxAdmirals(this._app.GameDatabase, this._playerID).ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "capturedAdmirals"), "text", string.Format(App.Localize("@NUMBER_OF_CAPTURED_ADMIRALS"), (object)list2.Count<AdmiralInfo>().ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "enemyAdmirals"), "text", string.Format(App.Localize("@NUMBER_OF_ENEMY_ADMIRALS")));
			int index = 0;
			foreach (AdmiralInfo admiralInfo in list2)
			{
				AdmiralInfo admiral = admiralInfo;
				if (index + 1 > this._buttonMap.Count)
					this._buttonMap.Add(admiral.ID);
				else
					this._buttonMap[index] = admiral.ID;
				this._app.UI.AddItem(this._app.UI.Path(this.ID, "pnlAdmirals.admiralList"), "", admiral.ID, "");
				string itemGlobalId1 = this._app.UI.GetItemGlobalID(this._app.UI.Path(this.ID, "pnlAdmirals.admiralList"), "", admiral.ID, "");
				this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "fleetitem.expand_button"), false);
				string str = "Deep Space";
				if (admiral.HomeworldID.HasValue)
					str = this._app.GameDatabase.GetOrbitalObjectInfo(admiral.HomeworldID.Value).Name;
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "dissolveButton"), "id", "dissolveButton" + index.ToString());
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "cancelMissionButton"), "id", "cancelMissionButton" + index.ToString());
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "createFleetButton"), "id", "createFleetButton" + index.ToString());
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "createEngramButton"), "id", "createEngramButton" + index.ToString());
				if (!admiral.Engram && this._app.GameDatabase.PlayerHasTech(this._app.LocalPlayer.ID, "CCC_Personality_Engrams"))
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "createEngramButton" + index.ToString()), true);
				else
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "createEngramButton" + index.ToString()), false);
				FleetInfo fleetInfoByAdmiralId = this._app.GameDatabase.GetFleetInfoByAdmiralID(admiral.ID, FleetType.FL_NORMAL);
				if (fleetInfoByAdmiralId != null)
				{
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "fleetitem.expand_button"), false);
					this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.idle.list_item.listitem_name"), "text", fleetInfoByAdmiralId.Name);
					this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.mouse_over.list_item.listitem_name"), "text", fleetInfoByAdmiralId.Name);
					this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "fleetitem.header_sel.idle.list_item.listitem_name"), "text", fleetInfoByAdmiralId.Name);
					this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "fleetitem.header_sel.mouse_over.list_item.listitem_name"), "text", fleetInfoByAdmiralId.Name);
					PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(fleetInfoByAdmiralId.PlayerID);
					this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.idle.list_item.colony_insert.LC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
					this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.idle.list_item.colony_insert.RC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
					this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.idle.list_item.colony_insert.BG"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
					this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.mouse_over.list_item.colony_insert.LC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
					this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.mouse_over.list_item.colony_insert.RC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
					this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.mouse_over.list_item.colony_insert.BG"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
					this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.disabled.list_item.colony_insert.LC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue * 0.5f);
					this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.disabled.list_item.colony_insert.RC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue * 0.5f);
					this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.disabled.list_item.colony_insert.BG"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue * 0.5f);
					this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_sel.idle.list_item.colony_insert.LC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
					this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_sel.idle.list_item.colony_insert.RC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
					this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_sel.idle.list_item.colony_insert.BG"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
					this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_sel.mouse_over.list_item.colony_insert.LC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
					this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_sel.mouse_over.list_item.colony_insert.RC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
					this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_sel.mouse_over.list_item.colony_insert.BG"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
					StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(fleetInfoByAdmiralId.SystemID);
					str = !(starSystemInfo != (StarSystemInfo)null) ? "Deep Space" : starSystemInfo.Name;
					MissionInfo missionByFleetId = this._app.GameDatabase.GetMissionByFleetID(fleetInfoByAdmiralId.ID);
					if (missionByFleetId != null)
					{
						this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "fleetitem.sub_title"), "text", fleetInfoByAdmiralId.Name);
						this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "cancelMissionButton" + index.ToString()), true);
					}
					else
					{
						this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "fleetitem.on_mission"), false);
						this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "cancelMissionButton" + index.ToString()), false);
					}
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "createFleetButton" + index.ToString()), false);
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "fleetitem"), true);
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "dissolveButton" + index.ToString()), (missionByFleetId != null ? 0 : (!playerSuulkas.Any<SuulkaInfo>((Func<SuulkaInfo, bool>)(x => x.AdmiralID == admiral.ID)) ? 1 : 0)) != 0);
				}
				else
				{
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "fleetitem"), false);
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "dissolveButton" + index.ToString()), false);
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "createFleetButton" + index.ToString()), (!playerSuulkas.Any<SuulkaInfo>((Func<SuulkaInfo, bool>)(x => x.AdmiralID == admiral.ID)) ? 1 : 0) != 0);
				}
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "admiralName"), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_NAME_COLON"), (object)admiral.Name));
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "admiralLocation"), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_LOCATION_COLON"), (object)str));
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "admiralAge"), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_AGE_COLON"), (object)((int)admiral.Age).ToString()));
				string admiralAvatar = Kerberos.Sots.StarFleet.StarFleet.GetAdmiralAvatar(this._app, admiral.ID);
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "admiralImage"), "sprite", admiralAvatar);
				IEnumerable<AdmiralInfo.TraitType> admiralTraits = this._app.GameDatabase.GetAdmiralTraits(admiral.ID);
				this._app.UI.ClearItems(this._app.UI.Path(itemGlobalId1, "admiralTraits"));
				int userItemId = 0;
				foreach (AdmiralInfo.TraitType traitType in admiralTraits)
				{
					string admiralTraitText = OverlayMission.GetAdmiralTraitText(traitType);
					if (traitType != admiralTraits.Last<AdmiralInfo.TraitType>())
						admiralTraitText += ", ";
					this._app.UI.AddItem(this._app.UI.Path(itemGlobalId1, "admiralTraits"), "", userItemId, "");
					string itemGlobalId2 = this._app.UI.GetItemGlobalID(this._app.UI.Path(itemGlobalId1, "admiralTraits"), "", userItemId, "");
					++userItemId;
					this._app.UI.SetPropertyString(itemGlobalId2, "text", admiralTraitText);
					if (AdmiralInfo.IsGoodTrait(traitType))
						this._app.UI.SetPropertyColorNormalized(itemGlobalId2, "color", new Vector3(0.0f, 1f, 0.0f));
					else
						this._app.UI.SetPropertyColorNormalized(itemGlobalId2, "color", new Vector3(1f, 0.0f, 0.0f));
					this._app.UI.SetTooltip(itemGlobalId2, AdmiralInfo.GetTraitDescription(traitType, this._app.GameDatabase.GetLevelForAdmiralTrait(admiral.ID, traitType)));
				}
				string itemGlobalId3 = this._app.UI.GetItemGlobalID(this._app.UI.Path(itemGlobalId1, "infoList"), "", 0, "");
				this._app.UI.Send((object)"SetSelected", (object)itemGlobalId3, (object)true);
				string propertyValue = admiral.HomeworldID.HasValue ? string.Format(App.Localize("@ADMIRAL_BIRTHPLANET_X"), (object)this._app.GameDatabase.GetOrbitalObjectInfo(admiral.HomeworldID.Value).Name) : App.Localize("@ADMIRAL_BORN_IN_SPACE");
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId3, "admiralBirthPlanet"), "text", propertyValue);
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId3, "admiralYears"), "text", string.Format(App.Localize("@YEARS_AS_ADMIRAL"), (object)((this._app.GameDatabase.GetTurnCount() - admiral.TurnCreated) / 4).ToString()));
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId3, "admiralLoyalty"), "text", string.Format(App.Localize("@ADMIRAL_LOYALTY"), (object)admiral.Loyalty.ToString()));
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId3, "admiralReaction"), "text", string.Format(App.Localize("@ADMIRAL_REACTION"), (object)admiral.ReactionBonus.ToString()));
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId3, "admiralEvasion"), "text", string.Format(App.Localize("@ADMIRAL_EVASION"), (object)admiral.EvasionBonus.ToString()));
				string itemGlobalId4 = this._app.UI.GetItemGlobalID(this._app.UI.Path(itemGlobalId1, "infoList"), "", 1, "");
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId4, "battlesWon"), "text", string.Format(App.Localize("@ADMIRAL_BATTLES_WON"), (object)admiral.BattlesWon.ToString()));
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId4, "battlesFought"), "text", string.Format(App.Localize("@ADMIRAL_BATTLES_FOUGHT"), (object)admiral.BattlesFought.ToString()));
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId4, "missionsAssigned"), "text", string.Format(App.Localize("@ADMIRAL_MISSIONS_ASSIGNED"), (object)admiral.MissionsAssigned.ToString()));
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId4, "missionsAccomplished"), "text", string.Format(App.Localize("@ADMIRAL_MISSIONS_ACCOMPLISHED"), (object)admiral.MissionsAccomplished.ToString()));
				++index;
			}
		}

		public void RefreshAdmiral(int admiralID)
		{
			string itemGlobalId1 = this._app.UI.GetItemGlobalID(this._app.UI.Path(this.ID, "pnlAdmirals.admiralList"), "", admiralID, "");
			AdmiralInfo admiralInfo = this._app.GameDatabase.GetAdmiralInfo(admiralID);
			int num1 = this._buttonMap.IndexOf(admiralID);
			this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "fleetitem.expand_button"), false);
			string str = App.Localize("@ADMIRAL_IN_DEEP_SPACE");
			FleetInfo fleetInfoByAdmiralId = this._app.GameDatabase.GetFleetInfoByAdmiralID(admiralID, FleetType.FL_NORMAL);
			if (fleetInfoByAdmiralId != null)
			{
				this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "fleetitem.expand_button"), false);
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.idle.list_item.listitem_name"), "text", fleetInfoByAdmiralId.Name);
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.mouse_over.list_item.listitem_name"), "text", fleetInfoByAdmiralId.Name);
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "fleetitem.header_sel.idle.list_item.listitem_name"), "text", fleetInfoByAdmiralId.Name);
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "fleetitem.header_sel.mouse_over.list_item.listitem_name"), "text", fleetInfoByAdmiralId.Name);
				StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(fleetInfoByAdmiralId.SystemID);
				str = !(starSystemInfo != (StarSystemInfo)null) ? App.Localize("@ADMIRAL_IN_DEEP_SPACE") : starSystemInfo.Name;
				MissionInfo missionByFleetId = this._app.GameDatabase.GetMissionByFleetID(fleetInfoByAdmiralId.ID);
				if (missionByFleetId != null)
				{
					this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "fleetitem.sub_title"), "text", fleetInfoByAdmiralId.Name);
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "cancelMissionButton" + num1.ToString()), true);
				}
				else
				{
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "fleetitem.on_mission"), false);
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "cancelMissionButton" + num1.ToString()), false);
				}
				this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "createFleetButton" + num1.ToString()), false);
				this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "fleetitem"), true);
				this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "dissolveButton" + num1.ToString()), (missionByFleetId == null ? 1 : 0) != 0);
			}
			else
			{
				this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "fleetitem"), false);
				this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "dissolveButton" + num1.ToString()), false);
				this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "createFleetButton" + num1.ToString()), true);
			}
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "admiralName"), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_NAME_COLON"), (object)admiralInfo.Name));
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "admiralLocation"), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_LOCATION_COLON"), (object)str));
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "admiralAge"), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_AGE_COLON"), (object)((int)admiralInfo.Age).ToString()));
			string admiralAvatar = Kerberos.Sots.StarFleet.StarFleet.GetAdmiralAvatar(this._app, admiralInfo.ID);
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "admiralImage"), "sprite", admiralAvatar);
			if (!admiralInfo.Engram && this._app.GameDatabase.PlayerHasTech(this._app.LocalPlayer.ID, "CCC_Personality_Engrams"))
				this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "createEngramButton" + num1.ToString()), true);
			else
				this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "createEngramButton" + num1.ToString()), false);
			IEnumerable<AdmiralInfo.TraitType> admiralTraits = this._app.GameDatabase.GetAdmiralTraits(admiralInfo.ID);
			this._app.UI.ClearItems(this._app.UI.Path(itemGlobalId1, "admiralTraits"));
			int userItemId = 0;
			foreach (AdmiralInfo.TraitType traitType in admiralTraits)
			{
				string admiralTraitText = OverlayMission.GetAdmiralTraitText(traitType);
				if (traitType != admiralTraits.Last<AdmiralInfo.TraitType>())
					admiralTraitText += ", ";
				this._app.UI.AddItem(this._app.UI.Path(itemGlobalId1, "admiralTraits"), "", userItemId, "");
				string itemGlobalId2 = this._app.UI.GetItemGlobalID(this._app.UI.Path(itemGlobalId1, "admiralTraits"), "", userItemId, "");
				++userItemId;
				this._app.UI.SetPropertyString(itemGlobalId2, "text", admiralTraitText);
				if (AdmiralInfo.IsGoodTrait(traitType))
					this._app.UI.SetPropertyColorNormalized(itemGlobalId2, "color", new Vector3(0.0f, 1f, 0.0f));
				else
					this._app.UI.SetPropertyColorNormalized(itemGlobalId2, "color", new Vector3(1f, 0.0f, 0.0f));
				this._app.UI.SetPropertyString(itemGlobalId2, "tooltip", AdmiralInfo.GetTraitDescription(traitType, this._app.GameDatabase.GetLevelForAdmiralTrait(admiralInfo.ID, traitType)));
			}
			string itemGlobalId3 = this._app.UI.GetItemGlobalID(this._app.UI.Path(itemGlobalId1, "infoList"), "", 0, "");
			this._app.UI.Send((object)"SetSelected", (object)itemGlobalId3, (object)true);
			string propertyValue = admiralInfo.HomeworldID.HasValue ? "Birth Planet: " + this._app.GameDatabase.GetOrbitalObjectInfo(admiralInfo.HomeworldID.Value).Name : "Born in Deep Space";
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId3, "admiralBirthPlanet"), "text", propertyValue);
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId3, "admiralYears"), "text", string.Format(App.Localize("@YEARS_AS_ADMIRAL"), (object)((this._app.GameDatabase.GetTurnCount() - admiralInfo.TurnCreated) / 4).ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId3, "admiralLoyalty"), "text", string.Format(App.Localize("@ADMIRAL_LOYALTY"), (object)admiralInfo.Loyalty.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId3, "admiralReaction"), "text", string.Format(App.Localize("@ADMIRAL_REACTION"), (object)admiralInfo.ReactionBonus.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId3, "admiralEvasion"), "text", string.Format(App.Localize("@ADMIRAL_EVASION"), (object)admiralInfo.EvasionBonus.ToString()));
			string itemGlobalId4 = this._app.UI.GetItemGlobalID(this._app.UI.Path(itemGlobalId1, "infoList"), "", 1, "");
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId4, "battlesWon"), "text", string.Format(App.Localize("@ADMIRAL_BATTLES_WON"), (object)admiralInfo.BattlesWon.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId4, "battlesFought"), "text", string.Format(App.Localize("@ADMIRAL_BATTLES_FOUGHT"), (object)admiralInfo.BattlesFought.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId4, "missionsAssigned"), "text", string.Format(App.Localize("@ADMIRAL_MISSIONS_ASSIGNED"), (object)admiralInfo.MissionsAssigned.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId4, "missionsAccomplished"), "text", string.Format(App.Localize("@ADMIRAL_MISSIONS_ACCOMPLISHED"), (object)admiralInfo.MissionsAccomplished.ToString()));
			int num2 = num1 + 1;
			PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(admiralInfo.PlayerID);
			this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.idle.list_item.colony_insert.LC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.idle.list_item.colony_insert.RC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.idle.list_item.colony_insert.BG"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.mouse_over.list_item.colony_insert.LC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.mouse_over.list_item.colony_insert.RC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.mouse_over.list_item.colony_insert.BG"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.disabled.list_item.colony_insert.LC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue * 0.5f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.disabled.list_item.colony_insert.RC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue * 0.5f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_idle.disabled.list_item.colony_insert.BG"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue * 0.5f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_sel.idle.list_item.colony_insert.LC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_sel.idle.list_item.colony_insert.RC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_sel.idle.list_item.colony_insert.BG"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_sel.mouse_over.list_item.colony_insert.LC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_sel.mouse_over.list_item.colony_insert.RC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId1, "fleetitem.header_sel.mouse_over.list_item.colony_insert.BG"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "okButton")
					this._app.UI.CloseDialog((Dialog)this, true);
				else if (panelName.StartsWith("createEngramButton"))
				{
					this._currentAdmiralID = this._buttonMap[int.Parse(panelName.Substring(18))];
					this._confirmCreateAdmiralEngram = this._app.UI.CreateDialog((Dialog)new GenericQuestionDialog(this._app, App.Localize("@UI_FLEET_DIALOG_CREATE_ENGRAM_TITLE"), string.Format(App.Localize("@UI_FLEET_DIALOG_CREATE_ENGRAM_DESC"), (object)this._app.GameDatabase.GetAdmiralInfo(this._currentAdmiralID).Name), "dialogGenericQuestion"), null);
				}
				else if (panelName.StartsWith("cancelMissionButton"))
				{
					int index = int.Parse(panelName.Substring(19));
					this._fleetToCancel = this._app.GameDatabase.GetFleetInfoByAdmiralID(this._buttonMap[index], FleetType.FL_NORMAL).ID;
					this._currentAdmiralID = this._buttonMap[index];
					this._confirmCancelMissionDialog = this._app.UI.CreateDialog((Dialog)new GenericQuestionDialog(this._app, App.Localize("@UI_FLEET_DIALOG_CANCELMISSION_TITLE"), string.Format(App.Localize("@UI_FLEET_DIALOG_CANCELMISSION_DESC"), (object)this._app.GameDatabase.GetFleetInfo(this._fleetToCancel).Name), "dialogGenericQuestion"), null);
				}
				else if (panelName.StartsWith("dissolveButton"))
				{
					int index = int.Parse(panelName.Substring(14));
					this._fleetToDissolve = this._app.GameDatabase.GetFleetInfoByAdmiralID(this._buttonMap[index], FleetType.FL_NORMAL).ID;
					this._currentAdmiralID = this._buttonMap[index];
					if (this._app.GameDatabase.GetMissionByFleetID(this._fleetToDissolve) == null && !Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(this._app.GameDatabase, this._app.GameDatabase.GetFleetInfo(this._fleetToDissolve)))
						this._confirmFleetDisolveDialog = this._app.UI.CreateDialog((Dialog)new GenericQuestionDialog(this._app, App.Localize("@UI_FLEET_DIALOG_DISSOLVEFLEET_TITLE"), string.Format(App.Localize("@UI_FLEET_DIALOG_DISSOLVEFLEET_DESC"), (object)this._app.GameDatabase.GetFleetInfo(this._fleetToDissolve).Name), "dialogGenericQuestion"), null);
					else
						this._app.UI.CreateDialog((Dialog)new GenericTextDialog(this._app, App.Localize("@UI_FLEET_DIALOG_CANNOTDISSOLVE_TITLE"), App.Localize("@UI_FLEET_DIALOG_CANNOTDISSOLVE_DESC"), "dialogGenericMessage"), null);
				}
				else
				{
					if (!panelName.StartsWith("createFleetButton"))
						return;
					AdmiralInfo admiralInfo = this._app.GameDatabase.GetAdmiralInfo(this._buttonMap[int.Parse(panelName.Substring(17))]);
					ShipInfo shipInfo1 = (ShipInfo)null;
					DesignInfo designInfo1 = (DesignInfo)null;
					int? reserveFleetId = this._app.GameDatabase.GetReserveFleetID(this._playerID, this._currentSystemID);
					if (reserveFleetId.HasValue)
					{
						if (this._app.LocalPlayer.Faction.Name == "loa")
						{
							Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this._app.Game, reserveFleetId.Value);
							int fleetLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._app.Game, reserveFleetId.Value);
							designInfo1 = (DesignInfo)null;
							foreach (DesignInfo designInfo2 in this._app.GameDatabase.GetDesignInfosForPlayer(this._playerID).Where<DesignInfo>((Func<DesignInfo, bool>)(x =>
						   {
							   if (x.Class == ShipClass.Cruiser)
								   return x.GetCommandPoints() > 0;
							   return false;
						   })).ToList<DesignInfo>())
							{
								if (designInfo1 == null)
									designInfo1 = designInfo2;
								else if (designInfo1.ProductionCost > designInfo2.ProductionCost)
									designInfo1 = designInfo2;
							}
							if (designInfo1 != null && designInfo1.ProductionCost > fleetLoaCubeValue)
								designInfo1 = (DesignInfo)null;
						}
						else
						{
							foreach (ShipInfo shipInfo2 in this._app.GameDatabase.GetShipInfoByFleetID(reserveFleetId.Value, false))
							{
								if (this._app.GameDatabase.GetShipCommandPointQuota(shipInfo2.ID) > 0)
								{
									shipInfo1 = shipInfo2;
									break;
								}
							}
						}
					}
					if (shipInfo1 == null && designInfo1 == null)
					{
						this._app.UI.CreateDialog((Dialog)new GenericTextDialog(this._app, App.Localize("@UI_FLEET_DIALOG_CANNOTCREATEFLEET_TITLE"), App.Localize("@UI_FLEET_DIALOG_CANNOTCREATEFLEET_DESC"), "dialogGenericMessage"), null);
					}
					else
					{
						this._currentAdmiralID = admiralInfo.ID;
						if (shipInfo1 != null)
							this._currentShipID = shipInfo1.ID;
						if (designInfo1 != null)
							this._currentDesignID = designInfo1.ID;
						this._nameFleetDialog = this._app.UI.CreateDialog((Dialog)new GenericTextEntryDialog(this._app, App.Localize("@UI_FLEET_DIALOG_FLEETNAME_TITLE"), App.Localize("@UI_FLEET_DIALOG_FLEETNAME_DESC"), this._app.GameDatabase.ResolveNewFleetName(this._app, this._playerID, this._app.Game.NamesPool.GetFleetName(this._app.GameDatabase.GetFactionName(this._app.GameDatabase.GetPlayerFactionID(this._playerID)))), 24, 1, true, EditBoxFilterMode.None), null);
					}
				}
			}
			else if (msgType == "dialog_closed")
			{
				if (panelName == this._confirmCancelMissionDialog)
				{
					if (!bool.Parse(msgParams[0]))
						return;
					FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(this._fleetToCancel);
					this._app.PostRequestSpeech(string.Format("STRAT_008-01_{0}_{1}UniversalMissionNegation", (object)this._app.GameDatabase.GetFactionName(this._app.GameDatabase.GetPlayerFactionID(this._app.LocalPlayer.ID)), (object)this._app.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID).GetAdmiralSoundCueContext(this._app.AssetDatabase)), 50, 120, 0.0f);
					Kerberos.Sots.StarFleet.StarFleet.CancelMission(this._app.Game, fleetInfo, true);
				}
				else if (panelName == this._confirmCreateAdmiralEngram)
				{
					if (!bool.Parse(msgParams[0]))
						return;
					AdmiralInfo admiralInfo = this._app.GameDatabase.GetAdmiralInfo(this._currentAdmiralID);
					if (admiralInfo != null)
						this._app.GameDatabase.UpdateEngram(admiralInfo.ID, true);
					this.RefreshAdmiral(this._currentAdmiralID);
				}
				else if (panelName == this._confirmFleetDisolveDialog)
				{
					if (!bool.Parse(msgParams[0]))
						return;
					FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(this._fleetToDissolve);
					int? reserveFleetId = this._app.GameDatabase.GetReserveFleetID(this._playerID, fleetInfo.SystemID);
					if (!reserveFleetId.HasValue)
						return;
					foreach (ShipInfo shipInfo in (IEnumerable<ShipInfo>)this._app.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false).ToList<ShipInfo>())
						this._app.GameDatabase.TransferShip(shipInfo.ID, reserveFleetId.Value);
					this._app.GameDatabase.RemoveFleet(this._fleetToDissolve);
					this.RefreshAdmiral(this._currentAdmiralID);
				}
				else if (panelName == this._nameFleetDialog)
				{
					if (!bool.Parse(msgParams[0]))
						return;
					int num = this._app.GameDatabase.InsertFleet(this._playerID, this._currentAdmiralID, this._currentSystemID, this._currentSystemID, this._app.GameDatabase.ResolveNewFleetName(this._app, this._playerID, msgParams[1]), FleetType.FL_NORMAL);
					if (this._app.LocalPlayer.Faction.Name == "loa")
					{
						FleetInfo fleetInfo = this._app.GameDatabase.GetFleetsByPlayerAndSystem(this._playerID, this._currentSystemID, FleetType.FL_RESERVE).First<FleetInfo>();
						if (fleetInfo != null)
						{
							ShipInfo shipInfo = this._app.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, true).FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.IsLoaCube()));
							DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(this._currentDesignID);
							if (shipInfo != null && designInfo != null)
								this._transfercubesDialog = this._app.UI.CreateDialog((Dialog)new DialogLoaShipTransfer(this._app, num, fleetInfo.ID, shipInfo.ID, designInfo.ProductionCost), null);
						}
					}
					else
					{
						this._app.GameDatabase.TransferShip(this._currentShipID, num);
						this._app.UI.CreateDialog((Dialog)new FleetCompositorDialog(this._app, this._currentSystemID, num, "dialogFleetCompositor"), null);
					}
					this.RefreshAdmiral(this._currentAdmiralID);
				}
				else
				{
					if (!(panelName == this._transfercubesDialog) || ((IEnumerable<string>)msgParams).Count<string>() != 4)
						return;
					int fleetID = int.Parse(msgParams[0]);
					int.Parse(msgParams[1]);
					int shipID = int.Parse(msgParams[2]);
					int Loacubes = int.Parse(msgParams[3]);
					ShipInfo shipInfo1 = this._app.GameDatabase.GetShipInfo(shipID, true);
					ShipInfo shipInfo2 = this._app.GameDatabase.GetShipInfoByFleetID(fleetID, false).FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.IsLoaCube()));
					if (shipInfo2 == null)
						this._app.GameDatabase.InsertShip(fleetID, shipInfo1.DesignInfo.ID, "Cube", (ShipParams)0, new int?(), Loacubes);
					else
						this._app.GameDatabase.UpdateShipLoaCubes(shipInfo2.ID, shipInfo2.LoaCubes + Loacubes);
					if (shipInfo1.LoaCubes <= Loacubes)
						this._app.GameDatabase.RemoveShip(shipInfo1.ID);
					else
						this._app.GameDatabase.UpdateShipLoaCubes(shipInfo1.ID, shipInfo1.LoaCubes - Loacubes);
					if (!this.ComposeFleet)
						return;
					if (this._app.LocalPlayer.Faction.Name != "loa")
					{
						this._app.UI.CreateDialog((Dialog)new FleetCompositorDialog(this._app, this._currentSystemID, fleetID, "dialogFleetCompositor"), null);
					}
					else
					{
						if (this._app.CurrentState == this._app.GetGameState<StarMapState>())
							this._app.GetGameState<StarMapState>().RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_NORMAL);
						if (this._app.CurrentState == this._app.GetGameState<FleetManagerState>())
							this._app.GetGameState<FleetManagerState>().Refresh();
					}
					this._app.UI.CloseDialog((Dialog)this, true);
				}
			}
			else
			{
				if (!(msgType == "list_sel_changed") || !(panelName == "filterDropdown"))
					return;
				this._currentFilterMode = (AdmiralManagerDialog.AdmiralFilterMode)int.Parse(msgParams[0]);
				this.RefreshDisplay();
			}
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}

		private enum AdmiralFilterMode
		{
			None,
			Age,
			Name,
			Location,
			Fleet,
		}
	}
}
