// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.DefenseWidget
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.UI
{
	[GameObjectType(InteropGameObjectType.IGOT_DEFENSEWIDGET)]
	internal class DefenseWidget : GameObject, IDisposable
	{
		private static int _nextWidgetID = 100000;
		private App _game;
		private string _rootName;
		private int _widgetID;
		private bool _ready;
		private int _syncedFleet;

		public DefenseWidget(App game, string rootList)
		{
			this._game = game;
			game.AddExistingObject((IGameObject)this, (object)rootList, (object)DefenseWidget._nextWidgetID, (object)this._game.LocalPlayer.ID);
			this._widgetID = DefenseWidget._nextWidgetID;
			++DefenseWidget._nextWidgetID;
			this._rootName = rootList;
			this._game.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._game.UI.PanelMessage += new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
		}

		private void Refresh()
		{
			if (!this._ready)
				return;
			this.SyncFleet();
		}

		private void SyncFleet()
		{
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._syncedFleet);
			if (fleetInfo == null)
				return;
			IEnumerable<ShipInfo> shipInfoByFleetId = this.App.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, true);
			List<object> objectList = new List<object>();
			int num1 = 0;
			foreach (ShipInfo shipInfo1 in shipInfoByFleetId)
			{
				ShipInfo ship = shipInfo1;
				bool flag1 = true;
				++num1;
				objectList.Add((object)true);
				objectList.Add((object)ship.DesignID);
				objectList.Add((object)ship.ID);
				objectList.Add((object)ship.DesignInfo.Name);
				objectList.Add((object)ship.ShipName);
				bool flag2 = false;
				string str = "";
				PlatformTypes? platformType = ship.DesignInfo.GetPlatformType();
				bool flag3 = false;
				bool flag4 = ship.IsPoliceShip();
				int defenseAssetCpCost = this.App.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(ship.DesignInfo);
				if (ship.IsMinelayer())
				{
					flag2 = true;
					foreach (DesignSectionInfo designSection in ship.DesignInfo.DesignSections)
					{
						foreach (WeaponBankInfo weaponBank in designSection.WeaponBanks)
						{
							string wasset = this.App.GameDatabase.GetWeaponAsset(weaponBank.WeaponID.Value);
							if (wasset.Contains("Min_"))
							{
								LogicalWeapon logicalWeapon = this.App.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == wasset));
								if (logicalWeapon != null)
								{
									str = logicalWeapon.IconSpriteName;
									break;
								}
							}
						}
					}
				}
				else if (ship.IsSDB())
					flag3 = true;
				else if (ship.IsPoliceShip())
					flag4 = true;
				objectList.Add((object)flag2);
				objectList.Add((object)flag3);
				objectList.Add((object)str);
				objectList.Add(platformType.HasValue ? (object)platformType.Value.ToString() : (object)string.Empty);
				objectList.Add((object)flag4);
				if (defenseAssetCpCost == 0)
					objectList.Add((object)this.App.GameDatabase.GetShipCommandPointCost(ship.ID, true));
				else
					objectList.Add((object)defenseAssetCpCost);
				objectList.Add((object)this.App.GameDatabase.GetDesignCommandPointQuota(this.App.AssetDatabase, ship.DesignInfo.ID));
				objectList.Add((object)flag1);
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				int num5 = 0;
				int num6 = 0;
				int num7 = 0;
				ShipSectionAsset shipSectionAsset1 = (ShipSectionAsset)null;
				List<SectionInstanceInfo> list1 = this.App.GameDatabase.GetShipSectionInstances(ship.ID).ToList<SectionInstanceInfo>();
				if (list1.Count != ship.DesignInfo.DesignSections.Length)
					throw new InvalidDataException(string.Format("Mismatched design section vs ship section instance count for designId={0} and shipId={1}.", (object)ship.DesignInfo.ID, (object)ship.ID));
				for (int i = 0; i < ((IEnumerable<DesignSectionInfo>)ship.DesignInfo.DesignSections).Count<DesignSectionInfo>(); ++i)
				{
					if (list1.Count <= i)
					{
						App.Log.Warn("Tried syncing ship with no section", "game");
					}
					else
					{
						ShipSectionAsset shipSectionAsset2 = this.App.AssetDatabase.GetShipSectionAsset(ship.DesignInfo.DesignSections[i].FilePath);
						if (shipSectionAsset2.Type == ShipSectionType.Mission)
							shipSectionAsset1 = shipSectionAsset2;
						SectionInstanceInfo sectionInstanceInfo = list1.First<SectionInstanceInfo>((Func<SectionInstanceInfo, bool>)(x => x.SectionID == ship.DesignInfo.DesignSections[i].ID));
						num6 += shipSectionAsset2.ConstructionPoints;
						num7 += shipSectionAsset2.ColonizationSpace;
						num5 += shipSectionAsset2.Structure;
						num3 += shipSectionAsset2.RepairPoints;
						num4 += sectionInstanceInfo.Structure;
						num2 += sectionInstanceInfo.RepairPoints;
						Dictionary<ArmorSide, DamagePattern> armorInstances = this.App.GameDatabase.GetArmorInstances(sectionInstanceInfo.ID);
						if (armorInstances.Count > 0)
						{
							for (int index = 0; index < 4; ++index)
							{
								num5 += armorInstances[(ArmorSide)index].Width * armorInstances[(ArmorSide)index].Height * 3;
								for (int x = 0; x < armorInstances[(ArmorSide)index].Width; ++x)
								{
									for (int y = 0; y < armorInstances[(ArmorSide)index].Height; ++y)
									{
										if (!armorInstances[(ArmorSide)index].GetValue(x, y))
											num4 += 3;
									}
								}
							}
						}
						List<ModuleInstanceInfo> list2 = this.App.GameDatabase.GetModuleInstances(sectionInstanceInfo.ID).ToList<ModuleInstanceInfo>();
						List<DesignModuleInfo> module = ship.DesignInfo.DesignSections[i].Modules;
						for (int mod = 0; mod < module.Count; ++mod)
						{
							ModuleInstanceInfo moduleInstanceInfo = list2.First<ModuleInstanceInfo>((Func<ModuleInstanceInfo, bool>)(x => x.ModuleNodeID == module[mod].MountNodeName));
							string modAsset = this.App.GameDatabase.GetModuleAsset(module[mod].ModuleID);
							LogicalModule logicalModule = this.App.AssetDatabase.Modules.Where<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == modAsset)).First<LogicalModule>();
							num5 += (int)logicalModule.Structure;
							num4 += moduleInstanceInfo.Structure;
							num3 += logicalModule.RepairPointsBonus;
							num2 += moduleInstanceInfo.RepairPoints;
						}
						foreach (WeaponInstanceInfo weaponInstanceInfo in this.App.GameDatabase.GetWeaponInstances(list1[i].ID).ToList<WeaponInstanceInfo>())
						{
							num5 += (int)weaponInstanceInfo.MaxStructure;
							num4 += (int)weaponInstanceInfo.Structure;
						}
					}
				}
				objectList.Add((object)num4);
				objectList.Add((object)num5);
				objectList.Add((object)num2);
				objectList.Add((object)num3);
				objectList.Add((object)num6);
				objectList.Add((object)num7);
				IEnumerable<ShipInfo> ridersByParentId = this.App.GameDatabase.GetBattleRidersByParentID(ship.ID);
				objectList.Add((object)ridersByParentId.Count<ShipInfo>());
				foreach (ShipInfo shipInfo2 in ridersByParentId)
					objectList.Add((object)shipInfo2.ID);
				objectList.Add((object)0);
				objectList.Add((object)shipSectionAsset1.RealClass);
				objectList.Add((object)shipSectionAsset1.BattleRiderType);
				Matrix? shipSystemPosition = this.App.GameDatabase.GetShipSystemPosition(ship.ID);
				objectList.Add((object)(shipSystemPosition.HasValue ? 1 : 0));
				if (shipSystemPosition.HasValue)
					objectList.Add((object)shipSystemPosition.Value);
			}
			objectList.Insert(0, (object)num1);
			int systemDefensePoints = this.App.GameDatabase.GetSystemDefensePoints(fleetInfo.SystemID, this.App.LocalPlayer.ID);
			objectList.Insert(1, (object)systemDefensePoints);
			objectList.Insert(2, (object)fleetInfo.ID);
			this.PostSetProp("SyncShips", objectList.ToArray());
		}

		public void SetSyncedFleet(int fleetID)
		{
			this._syncedFleet = fleetID;
			this.Refresh();
		}

		public int GetSynchedFleet()
		{
			return this._syncedFleet;
		}

		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			if (!(eventName == "DefenseWidgetReady") || int.Parse(eventParams[0]) != this._widgetID)
				return;
			this._ready = true;
			this.Refresh();
		}

		protected void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
		}

		public void Dispose()
		{
			this._game.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._game.UI.PanelMessage -= new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
			if (this._game == null)
				return;
			this._game.ReleaseObject((IGameObject)this);
		}
	}
}
