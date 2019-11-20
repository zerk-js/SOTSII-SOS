// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.PsionicTilePanel
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
	internal class PsionicTilePanel : Panel
	{
		protected readonly Dictionary<string, PsionicSelectorItem> _items = new Dictionary<string, PsionicSelectorItem>();
		protected readonly PsionicInfoPanel _moduleInfo;
		protected readonly PsionicSelectorPage _page;
		protected PsionicSelectorItem _selectedItem;

		public LogicalPsionic SelectedPsionic
		{
			get
			{
				if (this._selectedItem == null)
					return (LogicalPsionic)null;
				return this._selectedItem.Psionic;
			}
		}

		public event PsionicSelectionChangedEventHandler SelectedPsionicChanged;

		protected void ModuleSelectionChanged(object sender, bool isRightClick)
		{
			if (this.SelectedPsionicChanged == null)
				return;
			this.SelectedPsionicChanged(sender, isRightClick);
		}

		public PsionicTilePanel(UICommChannel ui, string id, string moduleInfoPanel = "")
		  : base(ui, id, "PsionicSelector")
		{
			this.UI.ParentToMainPanel(this.ID);
			this.UI.SetDrawLayer(this.ID, 1);
			UICommChannel ui1 = this.UI;
			string id1;
			if (moduleInfoPanel.Length <= 0)
				id1 = this.UI.Path(this.ID, "psionicInfo");
			else
				id1 = moduleInfoPanel;
			this._moduleInfo = new PsionicInfoPanel(ui1, id1);
			this._page = new PsionicSelectorPage(this.UI, this.UI.Path(this.ID, "page"));
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

		public void SetAvailablePsionics(
		  IEnumerable<LogicalPsionic> modules,
		  LogicalPsionic selected,
		  bool enableSelection = true)
		{
			if (enableSelection)
				this._page.DetachItems();
			this.DisposeItems();
			this._items.Clear();
			foreach (PsionicSelectorItem psionicSelectorItem in modules.Select<LogicalPsionic, PsionicSelectorItem>((Func<LogicalPsionic, PsionicSelectorItem>)(module => new PsionicSelectorItem(this.UI, Guid.NewGuid().ToString(), module))))
				this._items[psionicSelectorItem.ID] = psionicSelectorItem;
			if (enableSelection)
			{
				PsionicSelectorItem psionicSelectorItem1 = new PsionicSelectorItem(this.UI, Guid.NewGuid().ToString(), new LogicalPsionic()
				{
					Name = "No Psionic",
					PsionicTitle = "No Psionic",
					Description = "The selected psionic slot will be empty.",
					Icon = "moduleicon_no_selection"
				});
				this._items[psionicSelectorItem1.ID] = psionicSelectorItem1;
				bool flag = false;
				if (selected != null)
				{
					PsionicSelectorItem psionicSelectorItem2 = this._items.Values.FirstOrDefault<PsionicSelectorItem>((Func<PsionicSelectorItem, bool>)(x => x.Psionic == selected));
					if (psionicSelectorItem2 != null)
					{
						this.HoverItem(psionicSelectorItem2);
						flag = true;
					}
				}
				if (!flag)
					this.HoverItem(psionicSelectorItem1);
			}
			this._page.ReplaceItems((IEnumerable<PsionicSelectorItem>)this._items.Values, enableSelection);
		}

		protected virtual void HoverItem(PsionicSelectorItem item)
		{
			if (item != null)
				this._moduleInfo.SetPsionic(item.Psionic);
			else
				this._moduleInfo.SetPsionic((LogicalPsionic)null);
		}

		protected virtual void SelectItem(string panelID, bool eventCallback, bool rightClicked)
		{
			PsionicSelectorItem psionicSelectorItem;
			if (panelID == null || !this._items.TryGetValue(panelID, out psionicSelectorItem) || this._selectedItem == psionicSelectorItem)
				return;
			this._selectedItem = psionicSelectorItem;
			if (!eventCallback || this.SelectedPsionicChanged == null)
				return;
			this.SelectedPsionicChanged((object)this, rightClicked);
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
