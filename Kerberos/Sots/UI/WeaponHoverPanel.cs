// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.WeaponHoverPanel
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;

namespace Kerberos.Sots.UI
{
	internal class WeaponHoverPanel : WeaponTilePanel
	{
		public WeaponHoverPanel(UICommChannel ui, string id, string weaponInfoPanel = "")
		  : base(ui, id, weaponInfoPanel)
		{
		}

		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
			WeaponSelectorItem weaponSelectorItem;
			if (!this._items.TryGetValue(panelId, out weaponSelectorItem))
				return;
			if (msgType == "mouse_enter")
			{
				this.SelectItem(panelId, true, false);
				this._weaponInfo.SetWeapons(this._selectedItem.Weapon, (LogicalWeapon)null);
				this._weaponInfo.SetVisible(true);
			}
			else
			{
				if (!(msgType == "mouse_leave"))
					return;
				this._weaponInfo.SetVisible(false);
			}
		}
	}
}
