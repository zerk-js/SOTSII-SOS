// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.GenericTextEntryDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class GenericTextEntryDialog : GenericQuestionDialog
	{
		public const string EditBoxPanel = "edit_text";
		private string _enteredText;
		private int _maxChars;
		private int _minChars;
		private bool _cancelEnabled;
		private EditBoxFilterMode filterMode;

		public GenericTextEntryDialog(
		  App game,
		  string title,
		  string text,
		  string defaultText = "",
		  int maxChars = 1024,
		  int minChars = 0,
		  bool cancelEnabled = true,
		  EditBoxFilterMode filterMode = EditBoxFilterMode.None)
		  : base(game, title, text, "dialogGenericTextEntry")
		{
			this._maxChars = maxChars;
			this._minChars = minChars;
			this._enteredText = defaultText;
			this._cancelEnabled = cancelEnabled;
			this.filterMode = filterMode;
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName == "okButton")
			{
				this.Confirm();
			}
			else
			{
				if (msgType == "edit_confirmed")
				{
					if (panelName == "edit_text")
					{
						this.Confirm();
						return;
					}
				}
				else if (msgType == "text_changed" && panelName == "edit_text")
				{
					this._enteredText = msgParams[0];
					return;
				}
				base.OnPanelMessage(panelName, msgType, msgParams);
			}
		}

		public void Confirm()
		{
			if (this._enteredText.Count<char>() < this._minChars)
			{
				this._app.UI.CreateDialog((Dialog)new GenericTextDialog(this._app, App.Localize("@INVALID_NAME"), string.Format(App.Localize("@INVALID_NAME_TEXT"), (object)this._minChars), "dialogGenericMessage"), null);
			}
			else
			{
				this._choice = true;
				this._app.UI.CloseDialog((Dialog)this, true);
			}
		}

		public override void Initialize()
		{
			base.Initialize();
			this._app.UI.Send((object)"SetMaxChars", (object)"edit_text", (object)this._maxChars);
			this._app.UI.SetPropertyString("edit_text", "text", this._enteredText);
			this._app.UI.Send((object)"SetFilterMode", (object)"edit_text", (object)this.filterMode.ToString());
			if (this._cancelEnabled)
				return;
			this._app.UI.SetVisible(this._app.UI.Path(this.ID, "cancelButton"), false);
		}

		public override string[] CloseDialog()
		{
			return new string[2]
			{
		this._choice.ToString(),
		this._enteredText
			};
		}
	}
}
