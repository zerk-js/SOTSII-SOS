// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.WeaponSelector
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class WeaponSelector : WeaponTilePanel
	{
		public WeaponSelector(UICommChannel ui, string id, string weaponInfoPanel = "")
		  : base(ui, id, weaponInfoPanel)
		{
		}

		private void DisposeItems()
		{
			foreach (Panel panel in this._items.Values)
				panel.Dispose();
		}

		public void SetAvailableWeapons(IEnumerable<LogicalWeapon> weapons, LogicalWeapon selected)
		{
			this.SetAvailableWeapons(weapons, true);
			if (selected != null)
				this._weaponInfo.SetWeapons(selected, (LogicalWeapon)null);
			WeaponSelectorItem weaponSelectorItem = this._items.Values.FirstOrDefault<WeaponSelectorItem>((Func<WeaponSelectorItem, bool>)(x => x.Weapon == selected));
			if (weaponSelectorItem == null)
				return;
			this.SelectItem(weaponSelectorItem.ID, false, false);
		}

		private void SelectComparativeItem(string panelID)
		{
			LogicalWeapon logicalWeapon = this._selectedItem != null ? this._selectedItem.Weapon : (LogicalWeapon)null;
			WeaponSelectorItem weaponSelectorItem;
			if (logicalWeapon != null && !string.IsNullOrEmpty(panelID) && this._items.TryGetValue(panelID, out weaponSelectorItem))
				this._weaponInfo.SetWeapons(weaponSelectorItem.Weapon, logicalWeapon);
			else
				this._weaponInfo.SetWeapons(logicalWeapon, (LogicalWeapon)null);
		}

		protected override void SelectItem(string panelID, bool eventCallback, bool rightClicked)
		{
			WeaponSelectorItem weaponSelectorItem;
			if (!this._items.TryGetValue(panelID, out weaponSelectorItem))
				return;
			if (this._selectedItem != null)
				this._selectedItem.SetSelected(false);
			this._selectedItem = weaponSelectorItem;
			if (this._selectedItem != null)
				this._selectedItem.SetSelected(true);
			if (!eventCallback)
				return;
			this.WeaponSelectionChanged((object)this, rightClicked);
		}

		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
			WeaponSelectorItem weaponSelectorItem;
			if (!this._items.TryGetValue(panelId, out weaponSelectorItem))
				return;
			if (msgType == "button_clicked")
				this.SelectItem(panelId, true, false);
			else if (msgType == "button_rclicked")
				this.SelectItem(panelId, true, true);
			else if (msgType == "mouse_enter")
			{
				this.SelectComparativeItem(panelId);
			}
			else
			{
				if (!(msgType == "mouse_leave"))
					return;
				this.SelectComparativeItem(null);
			}
		}

		protected override void OnDisposing()
		{
			base.OnDisposing();
		}
	}
}
