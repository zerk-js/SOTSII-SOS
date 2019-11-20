// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.NetProcessNetworkDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.UI
{
	internal class NetProcessNetworkDialog : Dialog
	{
		private string _text = "";
		public const string OKButton = "buttonProcess";
		public const string TextArea = "process_text";

		public NetProcessNetworkDialog(App game)
		  : base(game, "dialogNetworkProcess")
		{
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked") || !(panelName == "buttonProcess"))
				return;
			this._app.UI.CloseDialog((Dialog)this, true);
		}

		public void AddDialogString(string val)
		{
			NetProcessNetworkDialog processNetworkDialog = this;
			processNetworkDialog._text = processNetworkDialog._text + val + "\n";
			this._app.UI.SetText(this._app.UI.Path(this.ID, "process_text"), this._text);
		}

		public void ShowButton(bool val)
		{
			this._app.UI.SetVisible(this._app.UI.Path(this.ID, "buttonProcess"), (val ? 1 : 0) != 0);
		}

		public override void Initialize()
		{
			base.Initialize();
		}

		public override string[] CloseDialog()
		{
			return new string[0];
		}
	}
}
