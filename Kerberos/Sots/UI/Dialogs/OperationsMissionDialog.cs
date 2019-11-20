// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.Dialogs.OperationsMissionDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.UI.Dialogs
{
	internal class OperationsMissionDialog : Dialog
	{
		public OperationsMissionDialog(App game)
		  : base(game, "dialogOperationsSummary")
		{
		}

		public override void Initialize()
		{
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
