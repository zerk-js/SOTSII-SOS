// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.PsionicSelectorPage
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class PsionicSelectorPage : PanelBinding
	{
		private readonly List<PsionicSelectorItem> _items = new List<PsionicSelectorItem>();
		private readonly string _itemsPanelID;

		public PsionicSelectorPage(UICommChannel ui, string id)
		  : base(ui, id)
		{
			this._itemsPanelID = this.UI.Path(this.ID, "items");
		}

		public void DetachItems()
		{
			if (this._items.Count > 0)
				this._items.Clear();
			this.UI.Send((object)nameof(DetachItems), (object)this._itemsPanelID);
		}

		public void ReplaceItems(IEnumerable<PsionicSelectorItem> range, bool detach = true)
		{
			if (detach)
				this.DetachItems();
			this._items.Clear();
			this._items.AddRange(range);
			List<object> objectList = new List<object>();
			objectList.Add((object)"AttachItems");
			objectList.Add((object)this._itemsPanelID);
			objectList.Add((object)this._items.Count);
			objectList.AddRange((IEnumerable<object>)this._items.Select<PsionicSelectorItem, string>((Func<PsionicSelectorItem, string>)(x => x.ID)));
			this.UI.Send(objectList.ToArray());
		}
	}
}
