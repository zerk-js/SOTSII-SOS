// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.LoadGameDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.UI
{
	internal class LoadGameDialog : SaveGameDialog
	{
		public const string OKButton = "buttonOk";
		private bool _choice;

		public LoadGameDialog(App game, string defaultName)
		  : base(game, defaultName, "dialogLoadGame")
		{
		}

		protected override void OnSelectionCleared()
		{
			base.OnSelectionCleared();
			this.UI.SetEnabled("buttonOk", false);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "buttonOk")
				{
					this.Confirm();
					return;
				}
				if (panelName == "buttonCancel")
				{
					this._choice = false;
					this._app.UI.CloseDialog((Dialog)this, true);
					return;
				}
			}
			base.OnPanelMessage(panelName, msgType, msgParams);
			if (this._selectedIndex == -1)
				return;
			this._app.UI.SetEnabled(this._app.UI.Path(this.ID, "buttonOk"), true);
		}

		public override void Confirm()
		{
			if (this._selectedIndex == -1)
				return;
			this._choice = true;
			this._app.UILoadGame(this._selectionFileNames[this._selectedIndex]);
			this._app.UI.CloseDialog((Dialog)this, true);
		}

		public override void Initialize()
		{
			this._app.UI.SetEnabled(this._app.UI.Path(this.ID, "buttonOk"), false);
			base.Initialize();
		}

		public override string[] CloseDialog()
		{
			return new string[1] { this._choice.ToString() };
		}
	}
}
