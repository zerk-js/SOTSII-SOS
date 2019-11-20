// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.PsionicSelector
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.UI
{
	internal class PsionicSelector : PsionicTilePanel
	{
		public PsionicSelector(UICommChannel ui, string id, string psionicInfoPanel = "")
		  : base(ui, id, psionicInfoPanel)
		{
		}

		private void DisposeItems()
		{
			foreach (Panel panel in this._items.Values)
				panel.Dispose();
		}

		protected override void SelectItem(string panelID, bool eventCallback, bool rightClicked)
		{
			PsionicSelectorItem psionicSelectorItem;
			if (!this._items.TryGetValue(panelID, out psionicSelectorItem))
				return;
			if (this._selectedItem != null)
				this._selectedItem.SetSelected(false);
			this._selectedItem = psionicSelectorItem;
			if (this._selectedItem != null)
				this._selectedItem.SetSelected(true);
			if (!eventCallback)
				return;
			this.ModuleSelectionChanged((object)this, rightClicked);
		}

		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
			PsionicSelectorItem psionicSelectorItem;
			if (!this._items.TryGetValue(panelId, out psionicSelectorItem))
				return;
			if (msgType == "button_clicked")
				this.SelectItem(panelId, true, false);
			else if (msgType == "button_rclicked")
				this.SelectItem(panelId, true, true);
			else if (msgType == "mouse_enter")
			{
				this.HoverItem(psionicSelectorItem);
			}
			else
			{
				if (!(msgType == "mouse_leave"))
					return;
				this.HoverItem((PsionicSelectorItem)null);
			}
		}

		protected override void OnDisposing()
		{
			base.OnDisposing();
		}
	}
}
