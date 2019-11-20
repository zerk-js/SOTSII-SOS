// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.RetrofitShipDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class RetrofitShipDialog : Dialog
	{
		private static string currentnameID = "current_shipname";
		private static string retrofitnameID = "retrofit_shipname";
		private static string RetrofitCostID = "costvalue";
		private static string RetrofitTimeID = "etavalue";
		private App App;
		private ShipInfo _ship;
		private bool Allshiptog;
		private WeaponHoverPanel _weaponTooltip;
		private ModuleHoverPanel _moduleTooltip;
		private WeaponHoverPanel _oldweaponTooltip;
		private ModuleHoverPanel _oldmoduleTooltip;

		public RetrofitShipDialog(App app, ShipInfo ship)
		  : base(app, "dialogRetrofitShip")
		{
			this.App = app;
			this._ship = ship;
		}

		public override void Initialize()
		{
			this.App.UI.HideTooltip();
			this.Allshiptog = false;
			DesignInfo newestRetrofitDesign = Kerberos.Sots.StarFleet.StarFleet.GetNewestRetrofitDesign(this._ship.DesignInfo, this.App.GameDatabase.GetVisibleDesignInfosForPlayer(this.App.LocalPlayer.ID));
			this.App.UI.SetText(this.App.UI.Path(this.ID, RetrofitShipDialog.currentnameID), this._ship.DesignInfo.Name);
			this.App.UI.SetText(this.App.UI.Path(this.ID, RetrofitShipDialog.retrofitnameID), newestRetrofitDesign.Name);
			this.App.UI.SetText(this.App.UI.Path(this.ID, RetrofitShipDialog.RetrofitCostID), Kerberos.Sots.StarFleet.StarFleet.CalculateRetrofitCost(this.App, this._ship.DesignInfo, newestRetrofitDesign).ToString());
			this.App.UI.SetText(this.App.UI.Path(this.ID, RetrofitShipDialog.RetrofitTimeID), Kerberos.Sots.StarFleet.StarFleet.GetTimeRequiredToRetrofit(this.App, this._ship, 1).ToString());
			this.App.GameDatabase.GetFleetInfo(this._ship.FleetID);
			List<ShipInfo> list = this.App.GameDatabase.GetShipInfoByFleetID(this._ship.FleetID, false).ToList<ShipInfo>();
			List<int> intList = new List<int>();
			foreach (ShipInfo shipInfo in list)
			{
				if (shipInfo.DesignID == this._ship.DesignID)
					intList.Add(shipInfo.ID);
			}
			if (Kerberos.Sots.StarFleet.StarFleet.FleetCanFunctionWithoutShips(this.App, this._ship.FleetID, (IEnumerable<int>)intList))
				this.App.UI.SetVisible(this.App.UI.Path(this.ID, "allships"), true);
			else
				this.App.UI.SetVisible(this.App.UI.Path(this.ID, "allships"), false);
			if (this._weaponTooltip == null)
				this._weaponTooltip = new WeaponHoverPanel(this.App.UI, this.App.UI.Path(this.ID, "WeaponPanel"), "weaponInfo");
			if (this._moduleTooltip == null)
				this._moduleTooltip = new ModuleHoverPanel(this.App.UI, this.App.UI.Path(this.ID, "WeaponPanel"), "moduleInfo");
			List<LogicalWeapon> source1 = new List<LogicalWeapon>();
			foreach (DesignSectionInfo designSection in newestRetrofitDesign.DesignSections)
			{
				foreach (WeaponBankInfo weaponBank in (IEnumerable<WeaponBankInfo>)designSection.WeaponBanks)
				{
					if (weaponBank.WeaponID.HasValue)
					{
						string weaponPath = this.App.GameDatabase.GetWeaponAsset(weaponBank.WeaponID.Value);
						LogicalWeapon weapon = this.App.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == weaponPath));
						if (weapon != null && source1.Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == weapon.FileName)).Count<LogicalWeapon>() == 0)
							source1.Add(weapon);
					}
				}
			}
			this._weaponTooltip.SetAvailableWeapons((IEnumerable<LogicalWeapon>)source1, true);
			List<LogicalModule> source2 = new List<LogicalModule>();
			foreach (DesignSectionInfo designSection in newestRetrofitDesign.DesignSections)
			{
				foreach (DesignModuleInfo module1 in (IEnumerable<DesignModuleInfo>)designSection.Modules)
				{
					string modulePath = this.App.GameDatabase.GetModuleAsset(module1.ModuleID);
					LogicalModule module = this.App.AssetDatabase.Modules.First<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == modulePath));
					if (module != null && source2.Where<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == module.ModulePath)).Count<LogicalModule>() == 0)
						source2.Add(module);
				}
			}
			this._moduleTooltip.SetAvailableModules((IEnumerable<LogicalModule>)source2, (LogicalModule)null, false);
			DesignInfo designInfo = this._ship.DesignInfo;
			if (this._oldweaponTooltip == null)
				this._oldweaponTooltip = new WeaponHoverPanel(this.App.UI, this.App.UI.Path(this.ID, "OldWeaponPanel"), "oldweaponInfo");
			if (this._oldmoduleTooltip == null)
				this._oldmoduleTooltip = new ModuleHoverPanel(this.App.UI, this.App.UI.Path(this.ID, "OldWeaponPanel"), "oldmoduleInfo");
			List<LogicalWeapon> source3 = new List<LogicalWeapon>();
			foreach (DesignSectionInfo designSection in designInfo.DesignSections)
			{
				foreach (WeaponBankInfo weaponBank in (IEnumerable<WeaponBankInfo>)designSection.WeaponBanks)
				{
					if (weaponBank.WeaponID.HasValue)
					{
						string weaponPath = this.App.GameDatabase.GetWeaponAsset(weaponBank.WeaponID.Value);
						LogicalWeapon weapon = this.App.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == weaponPath));
						if (weapon != null && source3.Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == weapon.FileName)).Count<LogicalWeapon>() == 0)
							source3.Add(weapon);
					}
				}
			}
			this._oldweaponTooltip.SetAvailableWeapons((IEnumerable<LogicalWeapon>)source3, true);
			List<LogicalModule> source4 = new List<LogicalModule>();
			foreach (DesignSectionInfo designSection in designInfo.DesignSections)
			{
				foreach (DesignModuleInfo module1 in (IEnumerable<DesignModuleInfo>)designSection.Modules)
				{
					string modulePath = this.App.GameDatabase.GetModuleAsset(module1.ModuleID);
					LogicalModule module = this.App.AssetDatabase.Modules.First<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == modulePath));
					if (module != null && source4.Where<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == module.ModulePath)).Count<LogicalModule>() == 0)
						source4.Add(module);
				}
			}
			this._oldmoduleTooltip.SetAvailableModules((IEnumerable<LogicalModule>)source4, (LogicalModule)null, false);
		}

		protected override void OnUpdate()
		{
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked"))
				return;
			if (panelName == "cancelButton")
				this._app.UI.CloseDialog((Dialog)this, true);
			if (panelName == "okButton")
			{
				this.RetrofitShips();
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			if (!(panelName == "allships"))
				return;
			this.Allshiptog = !this.Allshiptog;
			this.UpdateUICostETA();
		}

		private bool RetrofitShips()
		{
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._ship.FleetID);
			if (fleetInfo == null)
				return false;
			if (this.Allshiptog)
			{
				List<ShipInfo> list = this.App.GameDatabase.GetShipInfoByFleetID(this._ship.FleetID, false).ToList<ShipInfo>();
				List<int> intList = new List<int>();
				foreach (ShipInfo shipInfo in list)
				{
					if (shipInfo.DesignID == this._ship.DesignID)
						intList.Add(shipInfo.ID);
				}
				if (!Kerberos.Sots.StarFleet.StarFleet.FleetCanFunctionWithoutShips(this.App, this._ship.FleetID, (IEnumerable<int>)intList))
					return false;
				this._app.GameDatabase.RetrofitShips((IEnumerable<int>)intList, fleetInfo.SystemID, this.App.LocalPlayer.ID);
				return true;
			}
			if (!Kerberos.Sots.StarFleet.StarFleet.FleetCanFunctionWithoutShip(this.App, this._ship.FleetID, this._ship.ID))
				return false;
			this._app.GameDatabase.RetrofitShip(this._ship.ID, fleetInfo.SystemID, this.App.LocalPlayer.ID, new int?());
			return true;
		}

		private void UpdateUICostETA()
		{
			if (this.Allshiptog)
			{
				this.App.GameDatabase.GetFleetInfo(this._ship.FleetID);
				int numships = this.App.GameDatabase.GetShipInfoByFleetID(this._ship.FleetID, false).ToList<ShipInfo>().Where<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignID == this._ship.DesignID)).Count<ShipInfo>();
				this.App.UI.SetText(this.App.UI.Path(this.ID, RetrofitShipDialog.RetrofitTimeID), Kerberos.Sots.StarFleet.StarFleet.GetTimeRequiredToRetrofit(this.App, this._ship, numships).ToString());
				DesignInfo newestRetrofitDesign = Kerberos.Sots.StarFleet.StarFleet.GetNewestRetrofitDesign(this._ship.DesignInfo, this.App.GameDatabase.GetVisibleDesignInfosForPlayer(this.App.LocalPlayer.ID));
				this.App.UI.SetText(this.App.UI.Path(this.ID, RetrofitShipDialog.RetrofitCostID), (Kerberos.Sots.StarFleet.StarFleet.CalculateRetrofitCost(this.App, this._ship.DesignInfo, newestRetrofitDesign) * numships).ToString());
			}
			else
			{
				this.App.UI.SetText(this.App.UI.Path(this.ID, RetrofitShipDialog.RetrofitTimeID), Kerberos.Sots.StarFleet.StarFleet.GetTimeRequiredToRetrofit(this.App, this._ship, 1).ToString());
				DesignInfo newestRetrofitDesign = Kerberos.Sots.StarFleet.StarFleet.GetNewestRetrofitDesign(this._ship.DesignInfo, this.App.GameDatabase.GetVisibleDesignInfosForPlayer(this.App.LocalPlayer.ID));
				this.App.UI.SetText(this.App.UI.Path(this.ID, RetrofitShipDialog.RetrofitCostID), Kerberos.Sots.StarFleet.StarFleet.CalculateRetrofitCost(this.App, this._ship.DesignInfo, newestRetrofitDesign).ToString());
			}
		}

		public override string[] CloseDialog()
		{
			this.Allshiptog = false;
			this._app.GetGameState<StarMapState>()?.RefreshSystemInterface();
			return (string[])null;
		}
	}
}
