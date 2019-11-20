// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.ModuleHoverPanel
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.UI
{
	internal class ModuleHoverPanel : ModuleTilePanel
	{
		public ModuleHoverPanel(UICommChannel ui, string id, string moduleInfoPanel = "")
		  : base(ui, id, moduleInfoPanel)
		{
		}

		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
			ModuleSelectorItem moduleSelectorItem;
			if (!this._items.TryGetValue(panelId, out moduleSelectorItem))
				return;
			if (msgType == "mouse_enter")
			{
				this.SelectItem(panelId, true, false);
				this._moduleInfo.SetModule(this._selectedItem.Module);
				this._moduleInfo.SetVisible(true);
			}
			else
			{
				if (!(msgType == "mouse_leave"))
					return;
				this._moduleInfo.SetVisible(false);
			}
		}
	}
}
