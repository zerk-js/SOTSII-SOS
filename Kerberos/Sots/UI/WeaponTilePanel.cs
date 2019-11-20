// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.WeaponTilePanel
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
	internal class WeaponTilePanel : Panel
	{
		protected readonly Dictionary<string, WeaponSelectorItem> _items = new Dictionary<string, WeaponSelectorItem>();
		protected readonly WeaponInfoPanel _weaponInfo;
		protected readonly WeaponSelectorPage _page;
		protected WeaponSelectorItem _selectedItem;

		public LogicalWeapon SelectedWeapon
		{
			get
			{
				if (this._selectedItem == null)
					return (LogicalWeapon)null;
				return this._selectedItem.Weapon;
			}
		}

		public virtual event WeaponSelectionChangedEventHandler SelectedWeaponChanged;

		protected void WeaponSelectionChanged(object sender, bool isRightClick)
		{
			if (this.SelectedWeaponChanged == null)
				return;
			this.SelectedWeaponChanged(sender, isRightClick);
		}

		public WeaponTilePanel(UICommChannel ui, string id, string weaponInfoPanel = "")
		  : base(ui, id, "WeaponSelector")
		{
			this.UI.ParentToMainPanel(this.ID);
			this.UI.SetDrawLayer(this.ID, 1);
			UICommChannel ui1 = this.UI;
			string id1;
			if (weaponInfoPanel.Length <= 0)
				id1 = this.UI.Path(this.ID, "weaponInfo");
			else
				id1 = weaponInfoPanel;
			this._weaponInfo = new WeaponInfoPanel(ui1, id1);
			this._page = new WeaponSelectorPage(this.UI, this.UI.Path(this.ID, "page"));
			this.UI.PanelMessage += new UIEventPanelMessage(this.UIPanelMessage);
		}

		private void UIPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			this.OnPanelMessage(panelName, msgType, msgParams);
		}

		private void DisposeItems()
		{
			foreach (Panel panel in this._items.Values)
				panel.Dispose();
		}

		public void ClearItems()
		{
			this._page.DetachItems();
			this.DisposeItems();
			this._items.Clear();
		}

		public void SetAvailableWeapons(IEnumerable<LogicalWeapon> weapons, bool detach = true)
		{
			if (detach)
				this._page.DetachItems();
			this.DisposeItems();
			this._items.Clear();
			foreach (WeaponSelectorItem weaponSelectorItem in weapons.Select<LogicalWeapon, WeaponSelectorItem>((Func<LogicalWeapon, WeaponSelectorItem>)(weapon => new WeaponSelectorItem(this.UI, Guid.NewGuid().ToString(), weapon))))
				this._items[weaponSelectorItem.ID] = weaponSelectorItem;
			this._page.ReplaceItems((IEnumerable<WeaponSelectorItem>)this._items.Values);
		}

		protected virtual void SelectItem(string panelID, bool eventCallback, bool rightClicked)
		{
			WeaponSelectorItem weaponSelectorItem;
			if (!this._items.TryGetValue(panelID, out weaponSelectorItem))
				return;
			this._selectedItem = weaponSelectorItem;
			if (!eventCallback)
				return;
			this.WeaponSelectionChanged((object)this, rightClicked);
		}

		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
		}

		protected override void OnDisposing()
		{
			this.UI.PanelMessage -= new UIEventPanelMessage(this.UIPanelMessage);
			this.DisposeItems();
			base.OnDisposing();
		}
	}
}
