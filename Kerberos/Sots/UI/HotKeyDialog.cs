// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.HotKeyDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Kerberos.Sots.UI
{
	internal class HotKeyDialog : Dialog, IHotkeyVKListener
	{
		private HotKeyManager.HotKeyActions _bindingaction = HotKeyManager.HotKeyActions.NoAction;
		public const string UIItemAltToggle = "alt_toggle";
		public const string UIItemCtrlToggle = "ctrl_toggle";
		public const string UIItemShiftToggle = "shift_toggle";
		public const string UIItemHotkey = "gameHotKey";
		public const string UIItemClearBinding = "clearBinding";
		public const string UIHotKeyList = "hotkey_list";
		public const string okaybtn = "hotKeyOptions_ok";
		public const string defaultbtn = "hotKeyOptions_default";
		public const string backgroundpanel = "altbackgrnd";

		public HotKeyDialog(App game)
		  : base(game, "dialogHotKeyMenu")
		{
		}

		public override void Initialize()
		{
			this.PopulateHotKeyList();
			this._app.HotKeyManager.AddVKListener((IHotkeyVKListener)this);
		}

		private void PopulateHotKeyList()
		{
			this._app.UI.ClearItems(this._app.UI.Path(this.ID, "hotkey_list"));
			Array values = Enum.GetValues(typeof(HotKeyManager.HotKeyActions));
			string str1 = "";
			this._app.UI.AddItem(this._app.UI.Path(this.ID, "hotkey_list"), "", 9998, "");
			string itemGlobalId1 = this._app.UI.GetItemGlobalID(this._app.UI.Path(this.ID, "hotkey_list"), "", 9998, "");
			this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "inputrow"), false);
			this._app.UI.SetText(this._app.UI.Path(itemGlobalId1, "keyOption"), "");
			this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "altbackgrnd"), false);
			foreach (HotKeyManager.HotKeyActions action in values)
			{
				if (action != HotKeyManager.HotKeyActions.NoAction)
				{
					string str2 = action.ToString().Split('_')[0];
					if (str2 != str1)
					{
						this._app.UI.AddItem(this._app.UI.Path(this.ID, "hotkey_list"), "", 9999 * (int)action + 1, "");
						string itemGlobalId2 = this._app.UI.GetItemGlobalID(this._app.UI.Path(this.ID, "hotkey_list"), "", 9999 * (int)action + 1, "");
						this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId2, "inputrow"), false);
						this._app.UI.SetText(this._app.UI.Path(itemGlobalId2, "keyOption"), "------ " + App.Localize("@UI_HOTKEY_SUB_" + str2.ToUpper()) + " ------");
						this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId2, "keyOption"), "color", 13f, 220f, (float)byte.MaxValue);
						this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId2, "altbackgrnd"), false);
						str1 = str2;
					}
					this._app.UI.AddItem(this._app.UI.Path(this.ID, "hotkey_list"), "", (int)(action + 1), "");
					string itemGlobalId3 = this._app.UI.GetItemGlobalID(this._app.UI.Path(this.ID, "hotkey_list"), "", (int)(action + 1), "");
					HotKeyManager.KeyCombo keyCombo = this._app.HotKeyManager.GetHotKeyCombo(action) ?? new HotKeyManager.KeyCombo();
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId3, "inputrow"), true);
					this._app.UI.SetChecked(this._app.UI.Path(itemGlobalId3, "alt_toggle"), (keyCombo.alt ? 1 : 0) != 0);
					this._app.UI.SetChecked(this._app.UI.Path(itemGlobalId3, "ctrl_toggle"), (keyCombo.control ? 1 : 0) != 0);
					this._app.UI.SetChecked(this._app.UI.Path(itemGlobalId3, "shift_toggle"), (keyCombo.shift ? 1 : 0) != 0);
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId3, "altbackgrnd"), ((int)action % 2 == 0 ? 1 : 0) != 0);
					this._app.UI.SetText(this._app.UI.Path(itemGlobalId3, "keyOption"), App.Localize("@UI_HOTKEY_" + action.ToString().ToUpper()));
					this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId3, "alt_toggle"), "id", "alt_toggle|" + ((int)action).ToString());
					this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId3, "ctrl_toggle"), "id", "ctrl_toggle|" + ((int)action).ToString());
					this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId3, "shift_toggle"), "id", "shift_toggle|" + ((int)action).ToString());
					this._app.UI.SetText(this._app.UI.Path(itemGlobalId3, "gameHotKey", "keyLabel"), this._app.HotKeyManager.GetStringforKey(keyCombo.key));
					this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId3, "keyOption"), "color", (float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue);
					this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId3, "gameHotKey"), "id", "gameHotKey|" + ((int)action).ToString());
					this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId3, "clearBinding"), "id", "clearBinding|" + ((int)action).ToString());
				}
			}
		}

		public void UpdateHotkeyUI(List<HotKeyManager.HotKeyActions> hotkeys)
		{
			foreach (HotKeyManager.HotKeyActions hotkey in hotkeys)
			{
				if (hotkey != HotKeyManager.HotKeyActions.NoAction)
				{
					HotKeyManager.KeyCombo hotKeyCombo = this._app.HotKeyManager.GetHotKeyCombo(hotkey);
					string itemGlobalId = this._app.UI.GetItemGlobalID(this._app.UI.Path(this.ID, "hotkey_list"), "", (int)(hotkey + 1), "");
					this._app.UI.SetChecked(this._app.UI.Path(itemGlobalId, "alt_toggle|" + ((int)hotkey).ToString()), (hotKeyCombo.alt ? 1 : 0) != 0);
					this._app.UI.SetChecked(this._app.UI.Path(itemGlobalId, "ctrl_toggle|" + ((int)hotkey).ToString()), (hotKeyCombo.control ? 1 : 0) != 0);
					this._app.UI.SetChecked(this._app.UI.Path(itemGlobalId, "shift_toggle|" + ((int)hotkey).ToString()), (hotKeyCombo.shift ? 1 : 0) != 0);
					this._app.UI.SetText(this._app.UI.Path(itemGlobalId, "gameHotKey|" + ((int)hotkey).ToString(), "keyLabel"), this._app.HotKeyManager.GetStringforKey(hotKeyCombo.key));
				}
			}
		}

		public bool OnVKReported(Keys key, bool shift, bool ctrl, bool alt)
		{
			this._app.HotKeyManager.SetVkReportMode(false);
			if (this._bindingaction != HotKeyManager.HotKeyActions.NoAction)
			{
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "dialogBinding"), false);
				HotKeyManager.KeyCombo hotKeyCombo = this._app.HotKeyManager.GetHotKeyCombo(this._bindingaction);
				hotKeyCombo.shift = shift;
				hotKeyCombo.control = ctrl;
				hotKeyCombo.alt = alt;
				hotKeyCombo.key = key;
				this.UpdateHotkeyUI(this._app.HotKeyManager.SetHotKeyCombo(this._bindingaction, hotKeyCombo));
				this._bindingaction = HotKeyManager.HotKeyActions.NoAction;
			}
			return true;
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "hotKeyOptions_ok")
				{
					this._app.HotKeyManager.SaveProfile();
					this._app.HotKeyManager.SyncKeyProfile("");
					this._app.UI.CloseDialog((Dialog)this, true);
				}
				else if (panelName == "hotKeyOptions_default")
				{
					this._app.HotKeyManager.DeleteProfile();
					this._app.HotKeyManager.CreateProfile(this._app.UserProfile.ProfileName);
					this.PopulateHotKeyList();
				}
				else if (panelName.Contains("clearBinding"))
				{
					HotKeyManager.HotKeyActions action = (HotKeyManager.HotKeyActions)int.Parse(panelName.Split('|')[1]);
					HotKeyManager.KeyCombo hotKeyCombo = this._app.HotKeyManager.GetHotKeyCombo(action);
					hotKeyCombo.alt = false;
					hotKeyCombo.control = false;
					hotKeyCombo.shift = false;
					hotKeyCombo.key = Keys.None;
					this.UpdateHotkeyUI(this._app.HotKeyManager.SetHotKeyCombo(action, hotKeyCombo));
				}
				else
				{
					if (!panelName.Contains("gameHotKey"))
						return;
					HotKeyManager.HotKeyActions action = (HotKeyManager.HotKeyActions)int.Parse(panelName.Split('|')[1]);
					this._app.HotKeyManager.GetHotKeyCombo(action);
					this._app.UI.SetVisible(this._app.UI.Path(this.ID, "dialogBinding"), true);
					this._app.UI.SetText(this._app.UI.Path(this.ID, "dialogBinding", "bindtext"), "Press modifiers + key to bind for action - " + App.Localize("@UI_HOTKEY_" + action.ToString().ToUpper()));
					this._bindingaction = action;
					this._app.HotKeyManager.SetVkReportMode(true);
				}
			}
			else
			{
				if (msgType == "dialog_closed" || !(msgType == "checkbox_clicked"))
					return;
				if (panelName.StartsWith("alt_toggle"))
				{
					HotKeyManager.HotKeyActions action = (HotKeyManager.HotKeyActions)int.Parse(panelName.Split('|')[1]);
					bool flag = msgParams[0] == "1";
					HotKeyManager.KeyCombo hotKeyCombo = this._app.HotKeyManager.GetHotKeyCombo(action);
					if (action == HotKeyManager.HotKeyActions.NoAction)
						return;
					hotKeyCombo.alt = flag;
					this.UpdateHotkeyUI(this._app.HotKeyManager.SetHotKeyCombo(action, hotKeyCombo));
				}
				else if (panelName.StartsWith("ctrl_toggle"))
				{
					HotKeyManager.HotKeyActions action = (HotKeyManager.HotKeyActions)int.Parse(panelName.Split('|')[1]);
					bool flag = msgParams[0] == "1";
					HotKeyManager.KeyCombo hotKeyCombo = this._app.HotKeyManager.GetHotKeyCombo(action);
					if (action == HotKeyManager.HotKeyActions.NoAction)
						return;
					hotKeyCombo.control = flag;
					this.UpdateHotkeyUI(this._app.HotKeyManager.SetHotKeyCombo(action, hotKeyCombo));
				}
				else
				{
					if (!panelName.StartsWith("shift_toggle"))
						return;
					HotKeyManager.HotKeyActions action = (HotKeyManager.HotKeyActions)int.Parse(panelName.Split('|')[1]);
					bool flag = msgParams[0] == "1";
					HotKeyManager.KeyCombo hotKeyCombo = this._app.HotKeyManager.GetHotKeyCombo(action);
					if (action == HotKeyManager.HotKeyActions.NoAction)
						return;
					hotKeyCombo.shift = flag;
					this.UpdateHotkeyUI(this._app.HotKeyManager.SetHotKeyCombo(action, hotKeyCombo));
				}
			}
		}

		public override string[] CloseDialog()
		{
			this._app.HotKeyManager.RemoveVKListener((IHotkeyVKListener)this);
			return (string[])null;
		}
	}
}
