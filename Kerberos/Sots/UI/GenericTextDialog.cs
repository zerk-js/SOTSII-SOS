// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.GenericTextDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.UI
{
	internal class GenericTextDialog : Dialog
	{
		public const string OKButton = "okButton";
		private string _title;
		private string _text;

		public GenericTextDialog(App game, string title, string text, string template = "dialogGenericMessage")
		  : base(game, template)
		{
			this._title = title;
			this._text = text;
		}

		public override void Initialize()
		{
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "title"), "text", this._title);
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "textbox"), "text", this._text);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked") || !(panelName == "okButton"))
				return;
			this._app.UI.CloseDialog((Dialog)this, true);
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
