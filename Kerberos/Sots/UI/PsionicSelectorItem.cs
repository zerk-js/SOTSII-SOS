﻿// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.PsionicSelectorItem
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;

namespace Kerberos.Sots.UI
{
	internal class PsionicSelectorItem : ImageButton
	{
		private readonly Image _selectedOverlayImage;

		public LogicalPsionic Psionic { get; private set; }

		public bool IsSelected { get; private set; }

		public void SetSelected(bool value)
		{
			if (value == this.IsSelected)
				return;
			this.IsSelected = value;
			this._selectedOverlayImage.SetVisible(value);
		}

		public PsionicSelectorItem(UICommChannel ui, string id, LogicalPsionic module)
		  : base(ui, id, "WeaponSelectorIcon")
		{
			if (module == null)
				throw new ArgumentNullException(nameof(module));
			this.Psionic = module;
			this._selectedOverlayImage = new Image(ui, this.UI.Path(id, "selectedOverlay"));
			this.SetSprite(module.Icon);
			this.UI.SetPostMouseOverEvents(this.ID, true);
		}
	}
}
