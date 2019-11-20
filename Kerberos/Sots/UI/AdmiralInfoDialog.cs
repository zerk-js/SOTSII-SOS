// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.AdmiralInfoDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class AdmiralInfoDialog : Dialog
	{
		private static string panel = "admiralContent";
		private int _admiralID;

		public AdmiralInfoDialog(App game, int admiralID, string template = "admiralPopUp")
		  : base(game, template)
		{
			this._admiralID = admiralID;
		}

		public override void Initialize()
		{
			this.RefreshAdmiral(this._admiralID);
		}

		public void RefreshAdmiral(int admiralID)
		{
			string panel = AdmiralInfoDialog.panel;
			AdmiralInfo admiralInfo = this._app.GameDatabase.GetAdmiralInfo(admiralID);
			this._app.UI.SetVisible(this._app.UI.Path(panel, "fleetitem.expand_button"), false);
			string str = App.Localize("@ADMIRAL_IN_DEEP_SPACE");
			FleetInfo fleetInfoByAdmiralId = this._app.GameDatabase.GetFleetInfoByAdmiralID(admiralID, FleetType.FL_NORMAL);
			if (fleetInfoByAdmiralId != null)
			{
				this._app.UI.SetVisible(this._app.UI.Path(panel, "fleetitem.expand_button"), false);
				this._app.UI.SetPropertyString(this._app.UI.Path(panel, "fleetitem.header_idle.idle.list_item.listitem_name"), "text", fleetInfoByAdmiralId.Name);
				this._app.UI.SetPropertyString(this._app.UI.Path(panel, "fleetitem.header_idle.mouse_over.list_item.listitem_name"), "text", fleetInfoByAdmiralId.Name);
				this._app.UI.SetPropertyString(this._app.UI.Path(panel, "fleetitem.header_sel.idle.list_item.listitem_name"), "text", fleetInfoByAdmiralId.Name);
				this._app.UI.SetPropertyString(this._app.UI.Path(panel, "fleetitem.header_sel.mouse_over.list_item.listitem_name"), "text", fleetInfoByAdmiralId.Name);
				StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(fleetInfoByAdmiralId.SystemID);
				str = !(starSystemInfo != (StarSystemInfo)null) ? App.Localize("@ADMIRAL_IN_DEEP_SPACE") : starSystemInfo.Name;
				if (this._app.GameDatabase.GetMissionByFleetID(fleetInfoByAdmiralId.ID) != null)
				{
					this._app.UI.SetPropertyString(this._app.UI.Path(panel, "fleetitem.sub_title"), "text", fleetInfoByAdmiralId.Name);
					this._app.UI.SetVisible(this._app.UI.Path(panel, "cancelMissionButton"), false);
				}
				else
				{
					this._app.UI.SetVisible(this._app.UI.Path(panel, "fleetitem.on_mission"), false);
					this._app.UI.SetVisible(this._app.UI.Path(panel, "cancelMissionButton"), false);
				}
				this._app.UI.SetVisible(this._app.UI.Path(panel, "createFleetButton"), false);
				this._app.UI.SetVisible(this._app.UI.Path(panel, "fleetitem"), true);
				this._app.UI.SetVisible(this._app.UI.Path(panel, "dissolveButton"), false);
			}
			else
			{
				this._app.UI.SetVisible(this._app.UI.Path(panel, "fleetitem"), false);
				this._app.UI.SetVisible(this._app.UI.Path(panel, "dissolveButton"), false);
				this._app.UI.SetVisible(this._app.UI.Path(panel, "createFleetButton"), false);
			}
			this._app.UI.SetPropertyString(this._app.UI.Path(panel, "admiralName"), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_NAME_COLON"), (object)admiralInfo.Name));
			this._app.UI.SetPropertyString(this._app.UI.Path(panel, "admiralLocation"), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_LOCATION_COLON"), (object)str));
			this._app.UI.SetPropertyString(this._app.UI.Path(panel, "admiralAge"), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_AGE_COLON"), (object)((int)admiralInfo.Age).ToString()));
			IEnumerable<AdmiralInfo.TraitType> admiralTraits = this._app.GameDatabase.GetAdmiralTraits(admiralInfo.ID);
			this._app.UI.ClearItems(this._app.UI.Path(this.ID, panel, "admiralTraits"));
			int userItemId = 1;
			foreach (AdmiralInfo.TraitType traitType in admiralTraits)
			{
				string admiralTraitText = OverlayMission.GetAdmiralTraitText(traitType);
				if (traitType != admiralTraits.Last<AdmiralInfo.TraitType>())
					admiralTraitText += ", ";
				this._app.UI.AddItem(this._app.UI.Path(this.ID, panel, "admiralTraits"), "", userItemId, "");
				string itemGlobalId = this._app.UI.GetItemGlobalID(this._app.UI.Path(this.ID, panel, "admiralTraits"), "", userItemId, "");
				++userItemId;
				this._app.UI.SetPropertyString(itemGlobalId, "text", admiralTraitText);
				if (AdmiralInfo.IsGoodTrait(traitType))
					this._app.UI.SetPropertyColorNormalized(itemGlobalId, "color", new Vector3(0.0f, 1f, 0.0f));
				else
					this._app.UI.SetPropertyColorNormalized(itemGlobalId, "color", new Vector3(1f, 0.0f, 0.0f));
				this._app.UI.SetTooltip(itemGlobalId, AdmiralInfo.GetTraitDescription(traitType, this._app.GameDatabase.GetLevelForAdmiralTrait(admiralInfo.ID, traitType)));
				this._app.UI.SetPropertyString(itemGlobalId, "tooltip", AdmiralInfo.GetTraitDescription(traitType, this._app.GameDatabase.GetLevelForAdmiralTrait(admiralInfo.ID, traitType)));
			}
			string admiralAvatar = Kerberos.Sots.StarFleet.StarFleet.GetAdmiralAvatar(this._app, admiralInfo.ID);
			this._app.UI.SetPropertyString(this._app.UI.Path(panel, "admiralImage"), "sprite", admiralAvatar);
			if (!admiralInfo.Engram && this._app.GameDatabase.PlayerHasTech(this._app.LocalPlayer.ID, "CCC_Personality_Engrams"))
				this._app.UI.SetVisible(this._app.UI.Path(panel, "createEngramButton"), false);
			else
				this._app.UI.SetVisible(this._app.UI.Path(panel, "createEngramButton"), false);
			string itemGlobalId1 = this._app.UI.GetItemGlobalID(this._app.UI.Path(panel, "infoList"), "", 0, "");
			this._app.UI.Send((object)"SetSelected", (object)itemGlobalId1, (object)true);
			string propertyValue = admiralInfo.HomeworldID.HasValue ? "Birth Planet: " + this._app.GameDatabase.GetOrbitalObjectInfo(admiralInfo.HomeworldID.Value).Name : "Born in Deep Space";
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "admiralBirthPlanet"), "text", propertyValue);
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "admiralYears"), "text", string.Format(App.Localize("@YEARS_AS_ADMIRAL"), (object)((this._app.GameDatabase.GetTurnCount() - admiralInfo.TurnCreated) / 4).ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "admiralLoyalty"), "text", string.Format(App.Localize("@ADMIRAL_LOYALTY"), (object)admiralInfo.Loyalty.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "admiralReaction"), "text", string.Format(App.Localize("@ADMIRAL_REACTION"), (object)admiralInfo.ReactionBonus.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "admiralEvasion"), "text", string.Format(App.Localize("@ADMIRAL_EVASION"), (object)admiralInfo.EvasionBonus.ToString()));
			string itemGlobalId2 = this._app.UI.GetItemGlobalID(this._app.UI.Path(panel, "infoList"), "", 1, "");
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId2, "battlesWon"), "text", string.Format(App.Localize("@ADMIRAL_BATTLES_WON"), (object)admiralInfo.BattlesWon.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId2, "battlesFought"), "text", string.Format(App.Localize("@ADMIRAL_BATTLES_FOUGHT"), (object)admiralInfo.BattlesFought.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId2, "missionsAssigned"), "text", string.Format(App.Localize("@ADMIRAL_MISSIONS_ASSIGNED"), (object)admiralInfo.MissionsAssigned.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId2, "missionsAccomplished"), "text", string.Format(App.Localize("@ADMIRAL_MISSIONS_ACCOMPLISHED"), (object)admiralInfo.MissionsAccomplished.ToString()));
			PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(admiralInfo.PlayerID);
			if (playerInfo == null)
				return;
			this._app.UI.SetPropertyColor(this._app.UI.Path(panel, "fleetitem.header_idle.idle.list_item.colony_insert.LC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(panel, "fleetitem.header_idle.idle.list_item.colony_insert.RC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(panel, "fleetitem.header_idle.idle.list_item.colony_insert.BG"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(panel, "fleetitem.header_idle.mouse_over.list_item.colony_insert.LC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(panel, "fleetitem.header_idle.mouse_over.list_item.colony_insert.RC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(panel, "fleetitem.header_idle.mouse_over.list_item.colony_insert.BG"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(panel, "fleetitem.header_idle.disabled.list_item.colony_insert.LC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue * 0.5f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(panel, "fleetitem.header_idle.disabled.list_item.colony_insert.RC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue * 0.5f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(panel, "fleetitem.header_idle.disabled.list_item.colony_insert.BG"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue * 0.5f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(panel, "fleetitem.header_sel.idle.list_item.colony_insert.LC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(panel, "fleetitem.header_sel.idle.list_item.colony_insert.RC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(panel, "fleetitem.header_sel.idle.list_item.colony_insert.BG"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(panel, "fleetitem.header_sel.mouse_over.list_item.colony_insert.LC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(panel, "fleetitem.header_sel.mouse_over.list_item.colony_insert.RC"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
			this._app.UI.SetPropertyColor(this._app.UI.Path(panel, "fleetitem.header_sel.mouse_over.list_item.colony_insert.BG"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (!(panelName == "okbtn"))
					return;
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else
			{
				int num = msgType == "dialog_closed" ? 1 : 0;
			}
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
