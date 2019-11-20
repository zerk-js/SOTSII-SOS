// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.ModuleSelector
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.UI
{
	internal class ModuleSelector : ModuleTilePanel
	{
		public ModuleSelector(UICommChannel ui, string id, string moduleInfoPanel = "")
		  : base(ui, id, moduleInfoPanel)
		{
		}

		private void DisposeItems()
		{
			foreach (Panel panel in this._items.Values)
				panel.Dispose();
		}

		protected override void SelectItem(string panelID, bool eventCallback, bool rightClicked)
		{
			ModuleSelectorItem moduleSelectorItem;
			if (!this._items.TryGetValue(panelID, out moduleSelectorItem))
				return;
			if (this._selectedItem != null)
				this._selectedItem.SetSelected(false);
			this._selectedItem = moduleSelectorItem;
			if (this._selectedItem != null)
				this._selectedItem.SetSelected(true);
			if (!eventCallback)
				return;
			this.ModuleSelectionChanged((object)this, rightClicked);
		}

		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
			ModuleSelectorItem moduleSelectorItem;
			if (!this._items.TryGetValue(panelId, out moduleSelectorItem))
				return;
			if (msgType == "button_clicked")
				this.SelectItem(panelId, true, false);
			else if (msgType == "button_rclicked")
				this.SelectItem(panelId, true, true);
			else if (msgType == "mouse_enter")
			{
				this.HoverItem(moduleSelectorItem);
			}
			else
			{
				if (!(msgType == "mouse_leave"))
					return;
				this.HoverItem((ModuleSelectorItem)null);
			}
		}

		protected override void OnDisposing()
		{
			base.OnDisposing();
		}
	}
}
