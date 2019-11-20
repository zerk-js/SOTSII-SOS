// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.DialogNeutronStar
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.UI
{
	internal class DialogNeutronStar : Dialog
	{
		private static readonly string panelID = "dialogSuperNova";
		private int NumColonies;

		public DialogNeutronStar(App app, int numColonies)
		  : base(app, DialogNeutronStar.panelID)
		{
			this.NumColonies = numColonies;
		}

		public override void Initialize()
		{
			this._app.UI.SetText(this._app.UI.Path(this.ID, "title"), App.Localize("@UI_NEUTRON_STAR_PRESENT"));
			this._app.UI.SetText(this._app.UI.Path(this.ID, "subTitle"), string.Format(App.Localize("@UI_NEUTRON_STAR_SYSTEMS_IN_RANGE"), (object)this.NumColonies));
			this._app.UI.SetText(this._app.UI.Path(this.ID, "description"), App.Localize("@UI_NEUTRON_STAR_IN_RANGE_DESC"));
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
