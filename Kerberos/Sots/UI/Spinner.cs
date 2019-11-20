// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.Spinner
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;

namespace Kerberos.Sots.UI
{
	internal class Spinner : PanelBinding
	{
		private readonly Button _upButton;
		private readonly Button _downButton;

		public override void SetEnabled(bool value)
		{
			base.SetEnabled(value);
			this._upButton.SetEnabled(value);
			this._downButton.SetEnabled(value);
		}

		protected virtual void OnClick(Spinner.Direction direction)
		{
		}

		public Spinner(UICommChannel ui, string id)
		  : base(ui, id)
		{
			this._upButton = new Button(this.UI, id + "_up", null);
			this._upButton.Clicked += new EventHandler(this.ButtonClicked);
			this._downButton = new Button(this.UI, id + "_down", null);
			this._downButton.Clicked += new EventHandler(this.ButtonClicked);
			this.AddPanels((PanelBinding)this._upButton, (PanelBinding)this._downButton);
		}

		private void ButtonClicked(object sender, EventArgs e)
		{
			if (sender == this._upButton)
			{
				this.OnClick(Spinner.Direction.Up);
			}
			else
			{
				if (sender != this._downButton)
					return;
				this.OnClick(Spinner.Direction.Down);
			}
		}

		public enum Direction
		{
			Up,
			Down,
		}
	}
}
