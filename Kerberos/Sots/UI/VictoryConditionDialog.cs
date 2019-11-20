// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.VictoryConditionDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections.Generic;

namespace Kerberos.Sots.UI
{
	internal class VictoryConditionDialog : Dialog
	{
		private GameMode _selectedMode;

		public VictoryConditionDialog(App game, string template = "dialogVictoryCondition")
		  : base(game, template)
		{
		}

		public override void Initialize()
		{
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked"))
				return;
			if (panelName == "modeLastSideStanding")
			{
				this._selectedMode = GameMode.LastSideStanding;
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else if (panelName == "modeLastCapitalStanding")
			{
				this._selectedMode = GameMode.LastCapitalStanding;
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else if (panelName == "modeFiveStarChambers")
			{
				this._selectedMode = GameMode.StarChamberLimit;
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else if (panelName == "modeFiveGemWorlds")
			{
				this._selectedMode = GameMode.GemWorldLimit;
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else if (panelName == "modeFiveProvinces")
			{
				this._selectedMode = GameMode.ProvinceLimit;
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else if (panelName == "modeTenLeviathans")
			{
				this._selectedMode = GameMode.LeviathanLimit;
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else
			{
				if (!(panelName == "modeLandGrab"))
					return;
				this._selectedMode = GameMode.LandGrab;
				this._app.UI.CloseDialog((Dialog)this, true);
			}
		}

		public override string[] CloseDialog()
		{
			return new List<string>()
	  {
		((int) this._selectedMode).ToString()
	  }.ToArray();
		}
	}
}
