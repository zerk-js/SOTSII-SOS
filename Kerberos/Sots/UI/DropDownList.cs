// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.DropDownList
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class DropDownList : PanelBinding
	{
		private readonly BidirMap<int, object> _items = new BidirMap<int, object>();
		private object[] _selection = new object[0];
		private int _nextItemId;

		public object SelectedItem
		{
			get
			{
				if (this._selection.Length == 0)
					return (object)null;
				return this._selection[0];
			}
		}

		public event EventHandler SelectionChanged;

		protected virtual void OnSelectionChanged()
		{
		}

		public DropDownList(UICommChannel ui, string id)
		  : base(ui, id)
		{
			this.UI.SetPropertyBool(this.ID, "only_user_events", true);
		}

		public bool AddItem(object item, string text)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));
			if (text == null)
				throw new ArgumentNullException(nameof(text));
			if (this._items.Reverse.ContainsKey(item))
				return false;
			this._items.Insert(this._nextItemId, item);
			this.UI.AddItem(this.ID, string.Empty, this._nextItemId, text);
			++this._nextItemId;
			return true;
		}

		public int GetLastItemID()
		{
			if (this._items.Reverse.Count == 0)
				return -1;
			return this._items.Reverse.Last<KeyValuePair<object, int>>().Value;
		}

		public bool RemoveItem(object item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));
			int num;
			if (!this._items.Reverse.TryGetValue(item, out num))
				return false;
			this._items.Remove(num, item);
			this.UI.RemoveItems(this.ID, num);
			return true;
		}

		public void Clear()
		{
			this._items.Clear();
			this.UI.ClearItems(this.ID);
			this._nextItemId = 0;
		}

		public bool SetSelection(object item)
		{
			int userItemId;
			if (item == null)
			{
				this._selection = new object[0];
				userItemId = -1;
			}
			else
			{
				if (!this._items.Reverse.TryGetValue(item, out userItemId))
					return false;
				this._selection = new object[1] { item };
			}
			this.UI.SetSelection(this.ID, userItemId);
			return true;
		}

		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
			if (!(msgType == "list_sel_changed"))
				return;
			object obj;
			if (this._items.Forward.TryGetValue(int.Parse(msgParams[0]), out obj))
				this._selection = new object[1] { obj };
			else
				this._selection = new object[0];
			this.OnSelectionChanged();
			if (this.SelectionChanged == null)
				return;
			this.SelectionChanged((object)this, EventArgs.Empty);
		}
	}
}
