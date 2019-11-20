// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.SuperWorldDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Strategy;

namespace Kerberos.Sots.UI
{
	internal class SuperWorldDialog : Dialog
	{
		private static readonly string UIGemWorld = "btnGem";
		private static readonly string UIForgeWorld = "btnForge";
		private static readonly string UIGemDesc = "lblGemDesc";
		private static readonly string UIForgeDesc = "lblForgeDesc";
		private static readonly string UILocation = "lblLocation";
		private int colonyId;

		public SuperWorldDialog(App game, int colonyId)
		  : base(game, "dialogSuperWorld")
		{
			this.colonyId = colonyId;
		}

		public override void Initialize()
		{
			this._app.UI.SetText(this._app.UI.Path(this.ID, SuperWorldDialog.UIGemDesc), string.Format(App.Localize("@UI_DIALOGSUPERWORLD_GEM_DESC"), (object)this._app.AssetDatabase.GemWorldCivMaxBonus));
			this._app.UI.SetText(this._app.UI.Path(this.ID, SuperWorldDialog.UIForgeDesc), string.Format(App.Localize("@UI_DIALOGSUPERWORLD_FORGE_DESC"), (object)this._app.AssetDatabase.ForgeWorldImpMaxBonus, (object)this._app.AssetDatabase.ForgeWorldIOBonus));
			OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(this._app.GameDatabase.GetColonyInfo(this.colonyId).OrbitalObjectID);
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
			this._app.UI.SetText(this._app.UI.Path(this.ID, SuperWorldDialog.UILocation), string.Format(App.Localize("@UI_DIALOGSUPERWORLD_LOCATION"), (object)orbitalObjectInfo.Name, (object)starSystemInfo.Name));
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked"))
				return;
			if (panelName == SuperWorldDialog.UIGemWorld)
			{
				ColonyInfo colonyInfo = this._app.GameDatabase.GetColonyInfo(this.colonyId);
				colonyInfo.CurrentStage = ColonyStage.GemWorld;
				colonyInfo.CivilianWeight /= 2f;
				this._app.GameDatabase.UpdateColony(colonyInfo);
				StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(this._app.GameDatabase.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID).StarSystemID);
				GameSession.ApplyMoralEvent(this._app, MoralEvent.ME_GEM_WORLD_FORMED, colonyInfo.PlayerID, new int?(colonyInfo.ID), starSystemInfo.ProvinceID, new int?(starSystemInfo.ID));
				this._app.GameDatabase.InsertGovernmentAction(colonyInfo.PlayerID, App.Localize("@GA_GEMWORLD"), "GemWorld", 0, 0);
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else
			{
				if (!(panelName == SuperWorldDialog.UIForgeWorld))
					return;
				ColonyInfo colonyInfo = this._app.GameDatabase.GetColonyInfo(this.colonyId);
				PlanetInfo planetInfo = this._app.GameDatabase.GetPlanetInfo(colonyInfo.OrbitalObjectID);
				colonyInfo.CurrentStage = ColonyStage.ForgeWorld;
				colonyInfo.CivilianWeight = 1f;
				planetInfo.Biosphere = 0;
				this._app.GameDatabase.UpdatePlanet(planetInfo);
				this._app.GameDatabase.UpdateColony(colonyInfo);
				StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(this._app.GameDatabase.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID).StarSystemID);
				GameSession.ApplyMoralEvent(this._app, MoralEvent.ME_FORGE_WORLD_FORMED, colonyInfo.PlayerID, new int?(colonyInfo.ID), starSystemInfo.ProvinceID, new int?(starSystemInfo.ID));
				this._app.GameDatabase.InsertGovernmentAction(colonyInfo.PlayerID, App.Localize("@GA_FORGEWORLD"), "ForgeWorld", 0, 0);
				this._app.UI.CloseDialog((Dialog)this, true);
			}
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
