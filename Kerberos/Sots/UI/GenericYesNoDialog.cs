// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.GenericYesNoDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.UI
{
	internal class GenericYesNoDialog : GenericTextDialog
	{
		public const string CancelButton = "cancelButton";
		public const string NoButton = "noButton";
		public GenericYesNoDialog.YesNoDialogResult Result;

		public GenericYesNoDialog(App game, string title, string text, string template)
		  : base(game, title, text, template)
		{
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked"))
				return;
			if (panelName == "okButton")
				this.Result = GenericYesNoDialog.YesNoDialogResult.Yes;
			else if (panelName == "cancelButton")
				this.Result = GenericYesNoDialog.YesNoDialogResult.Cancel;
			else if (panelName == "noButton")
				this.Result = GenericYesNoDialog.YesNoDialogResult.No;
			this._app.UI.CloseDialog((Dialog)this, true);
		}

		public override void Initialize()
		{
			base.Initialize();
		}

		public override string[] CloseDialog()
		{
			return new string[1] { this.Result.ToString() };
		}

		public enum YesNoDialogResult
		{
			Yes,
			No,
			Cancel,
		}
	}
}
