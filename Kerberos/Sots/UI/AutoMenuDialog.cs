// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.AutoMenuDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.UI
{
	internal class AutoMenuDialog : Dialog
	{
		public const string UIAutoDefenseTog = "autoDefences";
		public const string UIAutoRepairTog = "autoRepair";
		public const string UIAutoGoopTog = "autoGoop";
		public const string UIAutoJokerTog = "autoJoker";
		public const string UIAutoAOETog = "autoAOE";
		public const string UIAutoPatrolTog = "autoPatrol";
		public const string okaybtn = "autoOptions_ok";

		public AutoMenuDialog(App game)
		  : base(game, "dialog_autoOptions")
		{
		}

		public override void Initialize()
		{
			this._app.UI.SetChecked(this._app.UI.Path(this.ID, "autoDefences"), (this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoPlaceDefenseAssets ? 1 : 0) != 0);
			this._app.UI.SetChecked(this._app.UI.Path(this.ID, "autoRepair"), (this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoRepairShips ? 1 : 0) != 0);
			this._app.UI.SetChecked(this._app.UI.Path(this.ID, "autoGoop"), (this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoUseGoopModules ? 1 : 0) != 0);
			this._app.UI.SetChecked(this._app.UI.Path(this.ID, "autoJoker"), (this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoUseJokerModules ? 1 : 0) != 0);
			this._app.UI.SetChecked(this._app.UI.Path(this.ID, "autoAOE"), (this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoAoe ? 1 : 0) != 0);
			this._app.UI.SetChecked(this._app.UI.Path(this.ID, "autoPatrol"), (this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoPatrol ? 1 : 0) != 0);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (!(panelName == "autoOptions_ok"))
					return;
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else
			{
				if (msgType == "dialog_closed" || !(msgType == "checkbox_clicked"))
					return;
				if (panelName == "autoDefences")
				{
					bool flag = msgParams[0] == "1";
					if (flag == this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoPlaceDefenseAssets)
						return;
					this._app.GameDatabase.UpdatePlayerAutoPlaceDefenses(this._app.LocalPlayer.ID, flag);
					this._app.UserProfile.AutoPlaceDefenseAssets = flag;
					this._app.UserProfile.SaveProfile();
				}
				else if (panelName == "autoRepair")
				{
					bool flag = msgParams[0] == "1";
					if (flag == this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoRepairShips)
						return;
					this._app.GameDatabase.UpdatePlayerAutoRepairFleets(this._app.LocalPlayer.ID, flag);
					this._app.UserProfile.AutoRepairFleets = flag;
					this._app.UserProfile.SaveProfile();
				}
				else if (panelName == "autoGoop")
				{
					bool flag = msgParams[0] == "1";
					if (flag == this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoUseGoopModules)
						return;
					this._app.GameDatabase.UpdatePlayerAutoUseGoop(this._app.LocalPlayer.ID, flag);
					this._app.UserProfile.AutoUseGoop = flag;
					this._app.UserProfile.SaveProfile();
				}
				else if (panelName == "autoJoker")
				{
					bool flag = msgParams[0] == "1";
					if (flag == this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoUseJokerModules)
						return;
					this._app.GameDatabase.UpdatePlayerAutoUseJoker(this._app.LocalPlayer.ID, flag);
					this._app.UserProfile.AutoUseJoker = flag;
					this._app.UserProfile.SaveProfile();
				}
				else if (panelName == "autoAOE")
				{
					bool flag = msgParams[0] == "1";
					if (flag == this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoAoe)
						return;
					this._app.GameDatabase.UpdatePlayerAutoUseAOE(this._app.LocalPlayer.ID, flag);
					this._app.UserProfile.AutoAOE = flag;
					this._app.UserProfile.SaveProfile();
				}
				else
				{
					if (!(panelName == "autoPatrol"))
						return;
					bool flag = msgParams[0] == "1";
					if (flag == this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoPatrol)
						return;
					this._app.GameDatabase.UpdatePlayerAutoPatrol(this._app.LocalPlayer.ID, flag);
					this._app.UserProfile.AutoPatrol = flag;
					this._app.UserProfile.SaveProfile();
				}
			}
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
