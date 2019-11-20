// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.RetrofitStationDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class RetrofitStationDialog : Dialog
	{
		private static string RetrofitCostID = "costvalue";
		private static string RetrofitTimeID = "etavalue";
		private static string RetrofitBankListID = "BankPanel";
		private App App;
		private ShipInfo _ship;
		private Dictionary<string, int> BankDict;
		private Dictionary<string, string> ItemIDDict;
		private Dictionary<int, string> ModuleBankdict;
		private string selecteditem;
		private DesignInfo WorkingDesign;
		private WeaponSelector _weaponSelector;
		private WeaponBankInfo _selectedWeaponBank;
		private DesignModuleInfo _selectedModule;

		public RetrofitStationDialog(App app, ShipInfo ship)
		  : base(app, "dialogRetrofitStation")
		{
			this.App = app;
			this._ship = ship;
		}

		public override void Initialize()
		{
			this.App.UI.HideTooltip();
			this.BankDict = new Dictionary<string, int>();
			this.ItemIDDict = new Dictionary<string, string>();
			this.ModuleBankdict = new Dictionary<int, string>();
			DesignInfo designInfo = this._ship.DesignInfo;
			if (designInfo.DesignSections[0].Modules.Any<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x =>
		   {
			   ModuleEnums.StationModuleType? stationModuleType = x.StationModuleType;
			   if ((stationModuleType.GetValueOrDefault() != ModuleEnums.StationModuleType.Combat ? 0 : (stationModuleType.HasValue ? 1 : 0)) != 0)
				   return !x.WeaponID.HasValue;
			   return false;
		   })))
			{
				this.UpdateStationDesignInfo(designInfo);
				this._app.GameDatabase.UpdateDesign(designInfo);
			}
			this.WorkingDesign = new DesignInfo(this._ship.DesignInfo.PlayerID, this._ship.DesignInfo.Name, new string[1]
			{
		this._ship.DesignInfo.DesignSections[0].FilePath
			});
			this.WorkingDesign.StationLevel = designInfo.StationLevel;
			this.WorkingDesign.StationType = designInfo.StationType;
			DesignLab.SummarizeDesign(this._app.AssetDatabase, this._app.GameDatabase, this.WorkingDesign);
			this.WorkingDesign.DesignSections[0].Modules = new List<DesignModuleInfo>();
			this.WorkingDesign.DesignSections[0].WeaponBanks = new List<WeaponBankInfo>();
			this.WorkingDesign.DesignSections[0].Techs = new List<int>();
			this.WorkingDesign.DesignSections[0].ShipSectionAsset = designInfo.DesignSections[0].ShipSectionAsset;
			int num = 0;
			foreach (DesignModuleInfo module in designInfo.DesignSections[0].Modules)
			{
				DesignModuleInfo designModuleInfo = new DesignModuleInfo()
				{
					MountNodeName = module.MountNodeName,
					ModuleID = module.ModuleID,
					WeaponID = module.WeaponID,
					DesignID = module.DesignID,
					StationModuleType = module.StationModuleType,
					ID = num
				};
				++num;
				this.WorkingDesign.DesignSections[0].Modules.Add(designModuleInfo);
			}
			this.UpdateStationDesignInfo(this.WorkingDesign);
			this.App.UI.ClearItems(this.App.UI.Path(this.ID, RetrofitStationDialog.RetrofitBankListID));
			DesignLab.SummarizeDesign(this._app.AssetDatabase, this._app.GameDatabase, this.WorkingDesign);
			foreach (DesignSectionInfo designSection in this.WorkingDesign.DesignSections)
			{
				foreach (DesignModuleInfo module in designSection.Modules)
				{
					if (module.WeaponID.HasValue)
					{
						this.App.UI.AddItem(this.App.UI.Path(this.ID, RetrofitStationDialog.RetrofitBankListID), "", module.ID, "");
						string itemGlobalId = this.App.UI.GetItemGlobalID(this.App.UI.Path(this.ID, RetrofitStationDialog.RetrofitBankListID), "", module.ID, "");
						string asset = this.App.GameDatabase.GetWeaponAsset(module.WeaponID.Value);
						LogicalWeapon logicalWeapon = this.App.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == asset));
						this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "wepimg"), "sprite", logicalWeapon.IconSpriteName);
						string index = "retrofitButton|" + module.ID.ToString();
						this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "btnImageButton"), "id", index);
						if (!this.BankDict.ContainsKey(index))
							this.BankDict[index] = module.ID;
						if (!this.ItemIDDict.ContainsKey(index))
							this.ItemIDDict[index] = itemGlobalId;
					}
				}
			}
			this._weaponSelector = new WeaponSelector(this.App.UI, "gameWeaponSelector", "");
			this.App.UI.SetParent(this._weaponSelector.ID, this.UI.Path(this.ID, "gameWeaponSelectorbox"));
			this._weaponSelector.SelectedWeaponChanged += new WeaponSelectionChangedEventHandler(this.WeaponSelectorSelectedWeaponChanged);
			StationInfo stationInfo = this.App.GameDatabase.GetStationInfosByPlayerID(this._ship.DesignInfo.PlayerID).FirstOrDefault<StationInfo>((Func<StationInfo, bool>)(x => x.ShipID == this._ship.ID));
			if (stationInfo != null)
				StationUI.SyncStationDetailsWidget(this.App.Game, this.UI.Path(this.ID, "stationDetails"), stationInfo.OrbitalObjectID, true);
			this.UpdateUICostETA();
			this.App.UI.SetEnabled(this.UI.Path(this.ID, "okButton"), (this.DesignChanged() ? 1 : 0) != 0);
		}

		private void UpdateListitem(bool isRightClick)
		{
			if (isRightClick)
			{
				foreach (string str in this.ItemIDDict.Values)
					this.App.UI.SetPropertyString(this.App.UI.Path(str, "wepimg"), "sprite", this._weaponSelector.SelectedWeapon.IconSpriteName);
			}
			else
				this.App.UI.SetPropertyString(this.App.UI.Path(this.selecteditem, "wepimg"), "sprite", this._weaponSelector.SelectedWeapon.IconSpriteName);
			this.App.UI.SetEnabled(this.UI.Path(this.ID, "okButton"), (this.DesignChanged() ? 1 : 0) != 0);
		}

		private void WeaponSelectorSelectedWeaponChanged(object sender, bool isRightClick)
		{
			int? nullable = new int?();
			if (this._weaponSelector.SelectedWeapon != null)
				nullable = this.App.GameDatabase.GetWeaponID(this._weaponSelector.SelectedWeapon.FileName, this.App.LocalPlayer.ID);
			if (nullable.HasValue)
			{
				this._selectedModule.WeaponID = new int?(nullable.Value);
				if (isRightClick)
				{
					foreach (DesignModuleInfo module in this.WorkingDesign.DesignSections[0].Modules)
					{
						string moduleass = this.App.GameDatabase.GetModuleAsset(module.ModuleID);
						if (((IEnumerable<LogicalBank>)this.App.AssetDatabase.Modules.First<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == moduleass)).Banks).Count<LogicalBank>() > 0)
							module.WeaponID = new int?(nullable.Value);
					}
				}
				this.UpdateListitem(isRightClick);
			}
			DesignLab.SummarizeDesign(this._app.AssetDatabase, this._app.GameDatabase, this.WorkingDesign);
			this.UpdateUICostETA();
			this.HideWeaponSelector();
		}

		private void HideWeaponSelector()
		{
			this._weaponSelector.SetVisible(false);
		}

		private void PopulateWeaponSelector(List<LogicalWeapon> weapons, LogicalWeapon selected)
		{
			this.App.UI.MovePanelToMouse(this._weaponSelector.ID, UICommChannel.AnchorPoint.TopLeft, new Vector2(-4f, -4f));
			this._weaponSelector.SetAvailableWeapons((IEnumerable<LogicalWeapon>)weapons.OrderBy<LogicalWeapon, WeaponEnums.WeaponSizes>((Func<LogicalWeapon, WeaponEnums.WeaponSizes>)(x => x.DefaultWeaponSize)), selected);
			this._weaponSelector.SetVisible(true);
		}

		private void UpdateStationDesignInfo(DesignInfo di)
		{
			int num = 0;
			foreach (DesignSectionInfo designSection in di.DesignSections)
			{
				DesignSectionInfo dsi = designSection;
				if (dsi.WeaponBanks != null)
					dsi.WeaponBanks.Clear();
				ShipSectionAsset section = this.App.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == dsi.FilePath));
				if (dsi.Modules != null)
				{
					foreach (DesignModuleInfo module in dsi.Modules)
					{
						string moduleass = this.App.GameDatabase.GetModuleAsset(module.ModuleID);
						LogicalModule logicalModule = this.App.AssetDatabase.Modules.First<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == moduleass));
						if (logicalModule != null && ((IEnumerable<LogicalBank>)logicalModule.Banks).Count<LogicalBank>() > 0)
						{
							int fireMode = 0;
							ShipSectionAsset shipSectionAsset = this._ship.DesignInfo.DesignSections[0].ShipSectionAsset;
							IEnumerable<LogicalWeapon> preferredWeapons = LogicalWeapon.EnumerateWeaponFits(shipSectionAsset.Faction, shipSectionAsset.SectionName, this.App.GameDatabase.GetAvailableWeapons(this.App.AssetDatabase, this.App.LocalPlayer.ID).Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(weapona => weapona.IsVisible)), logicalModule.Banks[0].TurretSize, logicalModule.Banks[0].TurretClass).Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x =>
						  {
							  if ((double)x.Range <= 1500.0)
								  return x.DefaultWeaponSize == WeaponEnums.WeaponSizes.VeryLight;
							  return true;
						  }));
							int designID;
							int targetFilter;
							LogicalWeapon logicalWeapon = Ship.SelectWeapon(section, logicalModule.Banks[0], (IEnumerable<WeaponAssignment>)null, preferredWeapons, this.App.GameDatabase.GetAvailableWeapons(this.App.AssetDatabase, this.App.LocalPlayer.ID), module.MountNodeName, out designID, out targetFilter, out fireMode);
							int? nullable = new int?();
							if (logicalWeapon != null && !module.WeaponID.HasValue)
								nullable = this.App.GameDatabase.GetWeaponID(logicalWeapon.FileName, this.App.LocalPlayer.ID);
							else if (module.WeaponID.HasValue)
								nullable = module.WeaponID;
							this.ModuleBankdict[module.ID] = module.MountNodeName;
							++num;
							module.WeaponID = nullable;
						}
					}
				}
			}
		}

		protected override void OnUpdate()
		{
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked"))
				return;
			if (panelName == "cancelButton")
			{
				this._app.UI.CloseDialog((Dialog)this, true);
				this.HideWeaponSelector();
			}
			if (panelName == "okButton")
			{
				this.RetrofitShips();
				this._app.UI.CloseDialog((Dialog)this, true);
				this.HideWeaponSelector();
			}
			if (!this.BankDict.ContainsKey(panelName))
				return;
			this.SelectBank(this.BankDict[panelName]);
			string asset = this.App.GameDatabase.GetWeaponAsset(this._selectedModule.WeaponID.Value);
			LogicalWeapon selected = this.App.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == asset));
			ShipSectionAsset shipSectionAsset = this._ship.DesignInfo.DesignSections[0].ShipSectionAsset;
			string moduleass = this.App.GameDatabase.GetModuleAsset(this._selectedModule.ModuleID);
			LogicalModule logicalModule = this.App.AssetDatabase.Modules.First<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == moduleass));
			this.PopulateWeaponSelector(LogicalWeapon.EnumerateWeaponFits(shipSectionAsset.Faction, shipSectionAsset.SectionName, this.App.GameDatabase.GetAvailableWeapons(this.App.AssetDatabase, this.App.LocalPlayer.ID).Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(weapon => weapon.IsVisible)), logicalModule.Banks[0].TurretSize, logicalModule.Banks[0].TurretClass).Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x =>
		  {
			  if ((double)x.Range <= 1500.0)
				  return x.DefaultWeaponSize == WeaponEnums.WeaponSizes.VeryLight;
			  return true;
		  })).ToList<LogicalWeapon>(), selected);
			this.selecteditem = this.ItemIDDict[panelName];
		}

		private bool RetrofitShips()
		{
			if (!this.DesignChanged())
				return false;
			this.App.GameDatabase.InsertStationRetrofitOrder(this.App.GameDatabase.InsertDesignByDesignInfo(this.WorkingDesign), this._ship.ID);
			return true;
		}

		private void SelectBank(int bankid)
		{
			foreach (DesignSectionInfo designSection in this.WorkingDesign.DesignSections)
			{
				foreach (WeaponBankInfo weaponBank in designSection.WeaponBanks)
				{
					if (weaponBank.ID == bankid)
						this._selectedWeaponBank = weaponBank;
				}
				this._selectedModule = designSection.Modules.First<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x => x.MountNodeName == this.ModuleBankdict[bankid]));
			}
		}

		private bool DesignChanged()
		{
			foreach (DesignModuleInfo module in this.WorkingDesign.DesignSections[0].Modules)
			{
				DesignModuleInfo dmi = module;
				if (this._ship.DesignInfo.DesignSections[0].Modules.Any<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x =>
			   {
				   if (!(x.MountNodeName == dmi.MountNodeName))
					   return false;
				   int? weaponId1 = x.WeaponID;
				   int? weaponId2 = dmi.WeaponID;
				   if (weaponId1.GetValueOrDefault() == weaponId2.GetValueOrDefault())
					   return weaponId1.HasValue != weaponId2.HasValue;
				   return true;
			   })))
					return true;
			}
			return false;
		}

		private void UpdateUICostETA()
		{
			this.App.UI.SetText(this.App.UI.Path(this.ID, RetrofitStationDialog.RetrofitTimeID), "1");
			int stationRetrofitCost = Kerberos.Sots.StarFleet.StarFleet.CalculateStationRetrofitCost(this.App, this._ship.DesignInfo, this.WorkingDesign);
			this.App.UI.SetText(this.App.UI.Path(this.ID, RetrofitStationDialog.RetrofitCostID), stationRetrofitCost.ToString());
		}

		public override string[] CloseDialog()
		{
			this._weaponSelector.ClearItems();
			this._app.GetGameState<StarMapState>()?.RefreshSystemInterface();
			return (string[])null;
		}
	}
}
