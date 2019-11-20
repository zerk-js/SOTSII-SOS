// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.SuulkaArrivalDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.UI
{
	internal class SuulkaArrivalDialog : Dialog
	{
		private int _systemID;

		public SuulkaArrivalDialog(App game, int systemID)
		  : base(game, "dialogSuulkaArrival")
		{
			this._systemID = systemID;
		}

		public override void Initialize()
		{
			this._app.UI.SetPropertyString("system_name", "text", App.Localize("@DIALOG_SUULKA_ARRIVES") + " - " + this._app.GameDatabase.GetStarSystemInfo(this._systemID).Name);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked") || !(panelName == "event_dialog_close"))
				return;
			this._app.UI.CloseDialog((Dialog)this, true);
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
