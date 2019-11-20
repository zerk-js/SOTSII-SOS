// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.GenericQuestionDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.UI
{
	internal class GenericQuestionDialog : GenericTextDialog
	{
		public const string CancelButton = "cancelButton";
		public bool _choice;

		public GenericQuestionDialog(App game, string title, string text, string template = "dialogGenericQuestion")
		  : base(game, title, text, template)
		{
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked"))
				return;
			if (panelName == "okButton")
			{
				this._choice = true;
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else
			{
				if (!(panelName == "cancelButton"))
					return;
				this._choice = false;
				this._app.UI.CloseDialog((Dialog)this, true);
			}
		}

		public override void Initialize()
		{
			base.Initialize();
		}

		public override string[] CloseDialog()
		{
			return new string[1] { this._choice.ToString() };
		}
	}
}
