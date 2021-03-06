﻿// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.ModuleSelectorPage
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class ModuleSelectorPage : PanelBinding
	{
		private readonly List<ModuleSelectorItem> _items = new List<ModuleSelectorItem>();
		private readonly string _itemsPanelID;

		public ModuleSelectorPage(UICommChannel ui, string id)
		  : base(ui, id)
		{
			this._itemsPanelID = this.UI.Path(this.ID, "items");
		}

		public void DetachItems()
		{
			if (this._items.Count <= 0)
				return;
			this.UI.Send((object)nameof(DetachItems), (object)this._itemsPanelID);
			this._items.Clear();
		}

		public void ReplaceItems(IEnumerable<ModuleSelectorItem> range, bool detach = true)
		{
			if (detach)
				this.DetachItems();
			this._items.Clear();
			this._items.AddRange(range);
			List<object> objectList = new List<object>();
			objectList.Add((object)"AttachItems");
			objectList.Add((object)this._itemsPanelID);
			objectList.Add((object)this._items.Count);
			objectList.AddRange((IEnumerable<object>)this._items.Select<ModuleSelectorItem, string>((Func<ModuleSelectorItem, string>)(x => x.ID)));
			this.UI.Send(objectList.ToArray());
		}
	}
}
