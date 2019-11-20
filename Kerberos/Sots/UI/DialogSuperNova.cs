// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.DialogSuperNova
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Strategy;

namespace Kerberos.Sots.UI
{
	internal class DialogSuperNova : Dialog
	{
		private static readonly string panelID = "dialogSuperNova";
		private string SystemName;
		private int TurnsRemaining;
		private int NumColonies;

		public DialogSuperNova(App app, string systemName, int turnsRemaing, int numColonies)
		  : base(app, DialogSuperNova.panelID)
		{
			this.SystemName = systemName;
			this.TurnsRemaining = turnsRemaing;
			this.NumColonies = numColonies;
		}

		public override void Initialize()
		{
			if (this.TurnsRemaining > 0)
			{
				this._app.UI.SetText(this._app.UI.Path(this.ID, "title"), string.Format(App.Localize("@UI_SUPER_NOVA_COUNTDOWN_TITLE"), (object)this.TurnsRemaining));
				this._app.UI.SetText(this._app.UI.Path(this.ID, "subTitle"), string.Format(App.Localize("@UI_SUPER_NOVA_COUNTDOWN_SUBTITLE"), (object)this.NumColonies));
				if (this._app.GameDatabase.GetHasPlayerStudiedSpecialProject(this._app.LocalPlayer.ID, SpecialProjectType.RadiationShielding))
					this._app.UI.SetText(this._app.UI.Path(this.ID, "description"), string.Format(App.Localize("@UI_SUPER_NOVA_COUNTDOWN_DESC_RESEARCHED"), (object)this.SystemName, (object)this.NumColonies, (object)this.TurnsRemaining));
				else
					this._app.UI.SetText(this._app.UI.Path(this.ID, "description"), string.Format(App.Localize("@UI_SUPER_NOVA_COUNTDOWN_DESC_NOT_RESEARCHED"), (object)this.SystemName, (object)this.NumColonies, (object)this.TurnsRemaining));
			}
			else
			{
				this._app.UI.SetText(this._app.UI.Path(this.ID, "title"), string.Format(App.Localize("@UI_SUPER_NOVA_EXPLODE_TITLE"), (object)this.SystemName));
				this._app.UI.SetText(this._app.UI.Path(this.ID, "subTitle"), string.Format(App.Localize("@UI_SUPER_NOVA_EXPLODE_SUBTITLE"), (object)this.NumColonies));
				if (this._app.GameDatabase.GetHasPlayerStudiedSpecialProject(this._app.LocalPlayer.ID, SpecialProjectType.RadiationShielding))
					this._app.UI.SetText(this._app.UI.Path(this.ID, "description"), string.Format(App.Localize("@UI_SUPER_NOVA_EXPLODE_DESC_RESEARCHED"), (object)this.SystemName));
				else
					this._app.UI.SetText(this._app.UI.Path(this.ID, "description"), string.Format(App.Localize("@UI_SUPER_NOVA_EXPLODE_DESC_NOT_RESEARCHED"), (object)this.SystemName, (object)this.NumColonies));
			}
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked") || !(panelName == "btnOK"))
				return;
			this._app.UI.CloseDialog((Dialog)this, true);
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
