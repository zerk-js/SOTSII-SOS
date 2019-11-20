// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.RiderManager
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	[GameObjectType(InteropGameObjectType.IGOT_RIDERMANAGER)]
	internal class RiderManager : GameObject, IDisposable
	{
		private App App;
		private bool _ready;
		private List<int> _syncedFleets;
		private bool _contentChanged;

		public RiderManager(App game, string rootPanel)
		{
			this.App = game;
			game.AddExistingObject((IGameObject)this, (object)rootPanel);
			this.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			this.App.UI.PanelMessage += new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
		}

		public void SetSyncedFleets(List<FleetInfo> fleets)
		{
			List<int> fleets1 = new List<int>();
			foreach (FleetInfo fleet in fleets)
				fleets1.Add(fleet.ID);
			this.SetSyncedFleets(fleets1);
		}

		public void SetSyncedFleets(List<int> fleets)
		{
			this._syncedFleets = fleets;
			this._contentChanged = true;
			this.Refresh();
		}

		private void Refresh()
		{
			if (!this._contentChanged || !this._ready)
				return;
			foreach (int syncedFleet in this._syncedFleets)
				this.SyncFleet(syncedFleet);
			this._contentChanged = false;
		}

		private void ClearFleets()
		{
			this.PostSetProp(nameof(ClearFleets));
		}

		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			if (eventName == "RiderManagerReady")
			{
				this._ready = true;
				this.Refresh();
			}
			else
			{
				if (!(eventName == "RiderParentEvent"))
					return;
				int shipID = int.Parse(eventParams[0]);
				int num = int.Parse(eventParams[1]);
				int index = int.Parse(eventParams[2]);
				this.App.GameDatabase.SetShipParent(shipID, num);
				this.App.GameDatabase.UpdateShipRiderIndex(shipID, index);
				ShipInfo shipInfo1 = this.App.GameDatabase.GetShipInfo(num, false);
				if (shipInfo1 != null)
				{
					this.App.GameDatabase.TransferShip(shipID, shipInfo1.FleetID);
				}
				else
				{
					ShipInfo shipInfo2 = this.App.GameDatabase.GetShipInfo(shipID, false);
					if (shipInfo2 != null)
					{
						FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(shipInfo2.FleetID);
						if (fleetInfo != null)
							this.App.GameDatabase.TransferShip(shipID, this.App.GameDatabase.InsertOrGetReserveFleetInfo(fleetInfo.SystemID, this.App.LocalPlayer.ID).ID);
					}
				}
				this.Refresh();
			}
		}

		protected void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (!(msgType == "button_clicked") || !panelName.StartsWith("btnRemoveRider"))
				return;
			int shipID = int.Parse(panelName.Split('|')[1]);
			this.App.GameDatabase.RemoveShip(shipID);
			this.PostSetProp("RemoveRider", shipID);
		}

		public List<int> GetSyncedShips()
		{
			List<int> intList = new List<int>();
			foreach (int syncedFleet in this._syncedFleets)
			{
				this.App.GameDatabase.GetFleetInfo(syncedFleet);
				foreach (ShipInfo shipInfo in this.App.GameDatabase.GetShipInfoByFleetID(syncedFleet, false))
				{
					DesignInfo designInfo = shipInfo.DesignInfo;
					int num1 = 0;
					BattleRiderTypes type = BattleRiderTypes.Unspecified;
					foreach (DesignSectionInfo designSection in designInfo.DesignSections)
					{
						ShipSectionAsset shipSectionAsset = designSection.ShipSectionAsset;
						if (shipSectionAsset.BattleRiderType != BattleRiderTypes.Unspecified)
							type = shipSectionAsset.BattleRiderType;
						num1 += RiderManager.GetNumRiderSlots(this.App, designSection);
					}
					int num2 = 0;
					foreach (DesignSectionInfo designSection in designInfo.DesignSections)
					{
						ShipSectionAsset shipSectionAsset = designSection.ShipSectionAsset;
						num2 += shipSectionAsset.ReserveSize;
					}
					if (num2 > 0 || num1 > 0 || type.IsBattleRiderType())
						intList.Add(shipInfo.ID);
				}
			}
			return intList;
		}

		public static bool IsRiderMount(LogicalMount mount)
		{
			return RiderManager.IsRiderBank(mount.Bank);
		}

		public static bool IsRiderBank(LogicalBank bank)
		{
			switch (bank.TurretClass)
			{
				case WeaponEnums.TurretClasses.DestroyerRider:
				case WeaponEnums.TurretClasses.CruiserRider:
				case WeaponEnums.TurretClasses.DreadnoughtRider:
					return true;
				default:
					return false;
			}
		}

		public static int GetNumRiderSlots(App game, DesignSectionInfo info)
		{
			ShipSectionAsset shipSectionAsset = game.AssetDatabase.GetShipSectionAsset(info.FilePath);
			int num = 0;
			foreach (LogicalMount mount in shipSectionAsset.Mounts)
			{
				if (RiderManager.IsRiderMount(mount))
					++num;
			}
			foreach (DesignModuleInfo module in info.Modules)
			{
				string path = game.GameDatabase.GetModuleAsset(module.ModuleID);
				foreach (LogicalMount mount in game.AssetDatabase.Modules.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == path)).Mounts)
				{
					if (RiderManager.IsRiderMount(mount))
						++num;
				}
			}
			return num;
		}

		public static IEnumerable<int> GetBattleriderIndexes(App app, ShipInfo ship)
		{
			List<SectionInstanceInfo> list1 = app.GameDatabase.GetShipSectionInstances(ship.ID).ToList<SectionInstanceInfo>();
			List<DesignSectionInfo> sections = ((IEnumerable<DesignSectionInfo>)app.GameDatabase.GetShipInfo(ship.ID, true).DesignInfo.DesignSections).ToList<DesignSectionInfo>();
			List<int> intList = new List<int>();
			int num = 0;
			for (int j = 0; j < sections.Count; ++j)
			{
				SectionInstanceInfo sectionInstanceInfo = list1.First<SectionInstanceInfo>((Func<SectionInstanceInfo, bool>)(x => x.SectionID == sections[j].ID));
				List<ModuleInstanceInfo> list2 = app.GameDatabase.GetModuleInstances(sectionInstanceInfo.ID).ToList<ModuleInstanceInfo>();
				foreach (LogicalMount mount in app.AssetDatabase.GetShipSectionAsset(sections[j].FilePath).Mounts)
				{
					if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
						++num;
					else if (WeaponEnums.IsBattleRider(mount.Bank.TurretClass))
					{
						intList.Add(num);
						++num;
					}
				}
				if (list2.Count > 0)
				{
					foreach (ModuleInstanceInfo moduleInstanceInfo in list2)
					{
						ModuleInstanceInfo mii = moduleInstanceInfo;
						DesignModuleInfo designModuleInfo = sections[j].Modules.First<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x => x.MountNodeName == mii.ModuleNodeID));
						if (designModuleInfo.DesignID.HasValue)
						{
							string modAsset = app.GameDatabase.GetModuleAsset(designModuleInfo.ModuleID);
							foreach (LogicalMount mount in app.AssetDatabase.Modules.Where<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == modAsset)).First<LogicalModule>().Mounts)
							{
								if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
									++num;
								else if (WeaponEnums.IsBattleRider(mount.Bank.TurretClass))
								{
									intList.Add(num);
									++num;
								}
							}
						}
					}
				}
			}
			return (IEnumerable<int>)intList;
		}

		public static IEnumerable<CarrierWingData> GetDesignBattleriderWingData(
		  App App,
		  DesignInfo des)
		{
			int num1 = 0;
			int num2 = 0;
			List<CarrierWingData> source = new List<CarrierWingData>();
			foreach (DesignSectionInfo designSection in des.DesignSections)
			{
				ShipSectionAsset shipSectionAsset = App.AssetDatabase.GetShipSectionAsset(designSection.FilePath);
				if (shipSectionAsset.Type == ShipSectionType.Mission)
				{
					int battleRiderType = (int)shipSectionAsset.BattleRiderType;
				}
				num1 += RiderManager.GetNumRiderSlots(App, designSection);
				foreach (LogicalBank bank1 in shipSectionAsset.Banks)
				{
					LogicalBank bank = bank1;
					if (RiderManager.IsRiderBank(bank))
					{
						List<LogicalMount> list = ((IEnumerable<LogicalMount>)shipSectionAsset.Mounts).Where<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank == bank)).ToList<LogicalMount>();
						WeaponEnums.TurretClasses mountClass = bank.TurretClass;
						int count = list.Count;
						int riderSlotsPerSquad = BattleRiderSquad.GetMinRiderSlotsPerSquad(mountClass, des.Class);
						int numRidersPerSquad = BattleRiderSquad.GetNumRidersPerSquad(mountClass, des.Class, Math.Max(count, riderSlotsPerSquad));
						int num3 = numRidersPerSquad > count ? 1 : count / numRidersPerSquad;
						for (int index1 = 0; index1 < num3; ++index1)
						{
							int num4 = Math.Min(count, numRidersPerSquad);
							List<int> intList = new List<int>();
							for (int index2 = 0; index2 < num4; ++index2)
							{
								intList.Add(num2);
								++num2;
							}
							CarrierWingData carrierWingData = source.FirstOrDefault<CarrierWingData>((Func<CarrierWingData, bool>)(x =>
						   {
							   if (x.Class == mountClass)
								   return x.SlotIndexes.Count < numRidersPerSquad;
							   return false;
						   }));
							if (carrierWingData != null)
								carrierWingData.SlotIndexes.AddRange((IEnumerable<int>)intList);
							else
								source.Add(new CarrierWingData()
								{
									SlotIndexes = intList,
									Class = mountClass
								});
						}
					}
					else if (WeaponEnums.IsWeaponBattleRider(bank.TurretClass))
						num2 += ((IEnumerable<LogicalMount>)shipSectionAsset.Mounts).Count<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank == bank));
				}
				foreach (DesignModuleInfo module in designSection.Modules)
				{
					string path = App.GameDatabase.GetModuleAsset(module.ModuleID);
					LogicalModule logicalModule = App.AssetDatabase.Modules.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == path));
					foreach (LogicalBank bank1 in logicalModule.Banks)
					{
						LogicalBank bank = bank1;
						if (RiderManager.IsRiderBank(bank))
						{
							int count = ((IEnumerable<LogicalMount>)logicalModule.Mounts).Where<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank == bank)).ToList<LogicalMount>().Count;
							List<int> intList = new List<int>();
							for (int index = 0; index < count; ++index)
							{
								intList.Add(num2);
								++num2;
							}
							source.Add(new CarrierWingData()
							{
								SlotIndexes = intList,
								Class = bank.TurretClass,
								DefaultType = logicalModule.AbilityType == ModuleEnums.ModuleAbilities.KingfisherRider ? BattleRiderTypes.scout : BattleRiderTypes.Unspecified
							});
						}
						else if (WeaponEnums.IsWeaponBattleRider(bank.TurretClass))
							num2 += ((IEnumerable<LogicalMount>)logicalModule.Mounts).Count<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank == bank));
					}
				}
			}
			return (IEnumerable<CarrierWingData>)source;
		}

		private void SyncFleet(int fleetID)
		{
			this.App.GameDatabase.GetFleetInfo(fleetID);
			IEnumerable<ShipInfo> shipInfoByFleetId = this.App.GameDatabase.GetShipInfoByFleetID(fleetID, false);
			List<object> objectList = new List<object>();
			int num1 = 0;
			foreach (ShipInfo shipInfo in shipInfoByFleetId)
			{
				DesignInfo designInfo = shipInfo.DesignInfo;
				int num2 = 0;
				int count1 = objectList.Count;
				BattleRiderTypes type = BattleRiderTypes.Unspecified;
				int num3 = 0;
				List<CarrierWingData> source = new List<CarrierWingData>();
				foreach (DesignSectionInfo designSection in designInfo.DesignSections)
				{
					ShipSectionAsset shipSectionAsset = this.App.AssetDatabase.GetShipSectionAsset(designSection.FilePath);
					if (shipSectionAsset.Type == ShipSectionType.Mission)
						type = shipSectionAsset.BattleRiderType;
					num2 += RiderManager.GetNumRiderSlots(this.App, designSection);
					foreach (LogicalBank bank1 in shipSectionAsset.Banks)
					{
						LogicalBank bank = bank1;
						if (RiderManager.IsRiderBank(bank))
						{
							List<LogicalMount> list = ((IEnumerable<LogicalMount>)shipSectionAsset.Mounts).Where<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank == bank)).ToList<LogicalMount>();
							WeaponEnums.TurretClasses mountClass = bank.TurretClass;
							int count2 = list.Count;
							int riderSlotsPerSquad = BattleRiderSquad.GetMinRiderSlotsPerSquad(mountClass, designInfo.Class);
							int numRidersPerSquad = BattleRiderSquad.GetNumRidersPerSquad(mountClass, designInfo.Class, Math.Max(count2, riderSlotsPerSquad));
							int num4 = numRidersPerSquad > count2 ? 1 : count2 / numRidersPerSquad;
							for (int index1 = 0; index1 < num4; ++index1)
							{
								int num5 = Math.Min(count2, numRidersPerSquad);
								List<int> intList = new List<int>();
								for (int index2 = 0; index2 < num5; ++index2)
								{
									intList.Add(num3);
									++num3;
								}
								CarrierWingData carrierWingData = source.FirstOrDefault<CarrierWingData>((Func<CarrierWingData, bool>)(x =>
							   {
								   if (x.Class == mountClass)
									   return x.SlotIndexes.Count < numRidersPerSquad;
								   return false;
							   }));
								if (carrierWingData != null)
									carrierWingData.SlotIndexes.AddRange((IEnumerable<int>)intList);
								else
									source.Add(new CarrierWingData()
									{
										SlotIndexes = intList,
										Class = mountClass
									});
							}
							foreach (LogicalMount logicalMount in list)
								objectList.Add((object)(int)bank.TurretClass);
						}
						else if (WeaponEnums.IsWeaponBattleRider(bank.TurretClass))
							num3 += ((IEnumerable<LogicalMount>)shipSectionAsset.Mounts).Count<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank == bank));
					}
					foreach (DesignModuleInfo module in designSection.Modules)
					{
						string path = this.App.GameDatabase.GetModuleAsset(module.ModuleID);
						LogicalModule logicalModule = this.App.AssetDatabase.Modules.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == path));
						foreach (LogicalBank bank1 in logicalModule.Banks)
						{
							LogicalBank bank = bank1;
							if (RiderManager.IsRiderBank(bank))
							{
								List<LogicalMount> list = ((IEnumerable<LogicalMount>)logicalModule.Mounts).Where<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank == bank)).ToList<LogicalMount>();
								int count2 = list.Count;
								List<int> intList = new List<int>();
								for (int index = 0; index < count2; ++index)
								{
									intList.Add(num3);
									++num3;
								}
								source.Add(new CarrierWingData()
								{
									SlotIndexes = intList,
									Class = bank.TurretClass,
									DefaultType = logicalModule.AbilityType == ModuleEnums.ModuleAbilities.KingfisherRider ? BattleRiderTypes.scout : BattleRiderTypes.Unspecified
								});
								foreach (LogicalMount logicalMount in list)
									objectList.Add((object)(int)bank.TurretClass);
							}
							else if (WeaponEnums.IsWeaponBattleRider(bank.TurretClass))
								num3 += ((IEnumerable<LogicalMount>)logicalModule.Mounts).Count<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank == bank));
						}
					}
				}
				objectList.Insert(count1, (object)num2);
				int num6 = 0;
				string str1 = "";
				string str2 = "";
				foreach (DesignSectionInfo designSection in designInfo.DesignSections)
				{
					ShipSectionAsset shipSectionAsset = designSection.ShipSectionAsset;
					num6 += shipSectionAsset.ReserveSize;
					if (shipSectionAsset.Type == ShipSectionType.Mission)
						str1 = App.Localize(shipSectionAsset.Title);
					if (shipSectionAsset.Type == ShipSectionType.Engine)
						str2 = App.Localize(shipSectionAsset.Title);
				}
				if (num6 > 0 || num2 > 0 || type.IsBattleRiderType())
				{
					objectList.Add((object)shipInfo.DesignID);
					objectList.Add((object)shipInfo.ID);
					objectList.Add((object)designInfo.Name);
					objectList.Add((object)shipInfo.ShipName);
					objectList.Add((object)num6);
					objectList.Add((object)(int)designInfo.Class);
					objectList.Add((object)(int)type);
					objectList.Add((object)shipInfo.ParentID);
					objectList.Add((object)shipInfo.RiderIndex);
					objectList.Add((object)str1);
					objectList.Add((object)str2);
					if (num2 > 0)
					{
						objectList.Add((object)source.Count);
						foreach (CarrierWingData carrierWingData in source)
						{
							objectList.Add((object)carrierWingData.SlotIndexes.Count);
							foreach (int slotIndex in carrierWingData.SlotIndexes)
								objectList.Add((object)slotIndex);
							objectList.Add((object)(int)carrierWingData.Class);
							objectList.Add((object)(int)carrierWingData.DefaultType);
						}
					}
					else if (num6 > 0)
						objectList.Add((object)0);
					++num1;
				}
				else
					objectList.RemoveRange(count1, objectList.Count - count1);
			}
			objectList.Insert(0, (object)num1);
			this.PostSetProp("SyncShips", objectList.ToArray());
		}

		public void Dispose()
		{
			this.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this.App.UI.PanelMessage -= new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
			if (this.App == null)
				return;
			this.App.ReleaseObject((IGameObject)this);
		}
	}
}
