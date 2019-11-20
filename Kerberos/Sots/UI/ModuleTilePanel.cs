// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.ModuleTilePanel
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
	internal class ModuleTilePanel : Panel
	{
		protected readonly Dictionary<string, ModuleSelectorItem> _items = new Dictionary<string, ModuleSelectorItem>();
		protected readonly ModuleInfoPanel _moduleInfo;
		protected readonly ModuleSelectorPage _page;
		protected ModuleSelectorItem _selectedItem;

		public LogicalModule SelectedModule
		{
			get
			{
				if (this._selectedItem == null)
					return (LogicalModule)null;
				return this._selectedItem.Module;
			}
		}

		public event ModuleSelectionChangedEventHandler SelectedModuleChanged;

		protected void ModuleSelectionChanged(object sender, bool isRightClick)
		{
			if (this.SelectedModuleChanged == null)
				return;
			this.SelectedModuleChanged(sender, isRightClick);
		}

		public ModuleTilePanel(UICommChannel ui, string id, string moduleInfoPanel = "")
		  : base(ui, id, "ModuleSelector")
		{
			this.UI.ParentToMainPanel(this.ID);
			this.UI.SetDrawLayer(this.ID, 1);
			UICommChannel ui1 = this.UI;
			string id1;
			if (moduleInfoPanel.Length <= 0)
				id1 = this.UI.Path(this.ID, "moduleInfo");
			else
				id1 = moduleInfoPanel;
			this._moduleInfo = new ModuleInfoPanel(ui1, id1);
			this._page = new ModuleSelectorPage(this.UI, this.UI.Path(this.ID, "page"));
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

		public void SetAvailableModules(
		  IEnumerable<LogicalModule> modules,
		  LogicalModule selected,
		  bool enableSelection = true)
		{
			if (enableSelection)
				this._page.DetachItems();
			this.DisposeItems();
			this._items.Clear();
			foreach (ModuleSelectorItem moduleSelectorItem in modules.Select<LogicalModule, ModuleSelectorItem>((Func<LogicalModule, ModuleSelectorItem>)(module => new ModuleSelectorItem(this.UI, Guid.NewGuid().ToString(), module))))
				this._items[moduleSelectorItem.ID] = moduleSelectorItem;
			if (enableSelection)
			{
				ModuleSelectorItem moduleSelectorItem1 = new ModuleSelectorItem(this.UI, Guid.NewGuid().ToString(), new LogicalModule()
				{
					ModuleName = App.Localize("@UI_MODULENAME_NO_MODULE"),
					ModuleTitle = App.Localize("@UI_MODULENAME_NO_MODULE"),
					Description = App.Localize("@UI_NO_MODULE_DESC"),
					Icon = "moduleicon_no_selection"
				});
				this._items[moduleSelectorItem1.ID] = moduleSelectorItem1;
				bool flag = false;
				if (selected != null)
				{
					ModuleSelectorItem moduleSelectorItem2 = this._items.Values.FirstOrDefault<ModuleSelectorItem>((Func<ModuleSelectorItem, bool>)(x => x.Module == selected));
					if (moduleSelectorItem2 != null)
					{
						this.HoverItem(moduleSelectorItem2);
						flag = true;
					}
				}
				if (!flag)
					this.HoverItem(moduleSelectorItem1);
			}
			this._page.ReplaceItems((IEnumerable<ModuleSelectorItem>)this._items.Values, enableSelection);
		}

		protected virtual void HoverItem(ModuleSelectorItem item)
		{
			if (item != null)
				this._moduleInfo.SetModule(item.Module);
			else
				this._moduleInfo.SetModule((LogicalModule)null);
		}

		protected virtual void SelectItem(string panelID, bool eventCallback, bool rightClicked)
		{
			ModuleSelectorItem moduleSelectorItem;
			if (panelID == null || !this._items.TryGetValue(panelID, out moduleSelectorItem) || this._selectedItem == moduleSelectorItem)
				return;
			this._selectedItem = moduleSelectorItem;
			if (!eventCallback || this.SelectedModuleChanged == null)
				return;
			this.SelectedModuleChanged((object)this, rightClicked);
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
