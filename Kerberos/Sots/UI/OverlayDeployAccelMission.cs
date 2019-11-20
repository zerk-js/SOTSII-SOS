// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.OverlayDeployAccelMission
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class OverlayDeployAccelMission : OverlayMission
	{
		private List<int> _GatePoints = new List<int>();
		private List<int> _PotentialGates = new List<int>();
		private int _currentslider;
		private int gatecost;

		public OverlayDeployAccelMission(
		  App game,
		  StarMapState state,
		  StarMap starmap,
		  string template = "OverlayAcceleratorMission")
		  : base(game, state, starmap, MissionType.DEPLOY_NPG, template)
		{
		}

		protected override void OnCanConfirmMissionChanged(bool newValue)
		{
		}

		protected override bool CanConfirmMission()
		{
			int num1 = this.gatecost * (this._GatePoints.Count + 1);
			int num2 = this.IsValidFleetID(this.SelectedFleet) ? Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this.App.Game, this.SelectedFleet) : 0;
			if (this.IsValidFleetID(this.SelectedFleet) && this.TargetSystem != 0)
				return num1 <= num2;
			return false;
		}

		protected override void OnEnter()
		{
			base.OnEnter();
			StarSystemUI.SyncSystemDetailsWidget(this.App, "systemDetailsWidget", this.TargetSystem, false, true);
			this._currentslider = 0;
			this._GatePoints.Clear();
			this.gatecost = this.App.GameDatabase.GetDesignInfosForPlayer(this.App.LocalPlayer.ID).FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.IsAccelerator())).ProductionCost;
		}

		private void DistributeSliderNotches()
		{
			this.App.UI.ClearSliderNotches(this.UI.Path(this.ID, "LYSlider"));
			if (this.SelectedFleet != 0 && this.TargetSystem != 0)
			{
				this.App.UI.SetVisible(this.UI.Path(this.ID, "LYSlider"), true);
				this.App.UI.SetVisible(this.UI.Path(this.ID, "gamePlaceAccelerator"), true);
				this.App.UI.SetVisible(this.UI.Path(this.ID, "AccelListBox"), true);
				StarSystemInfo starSystemInfo1 = this._app.GameDatabase.GetStarSystemInfo(this._app.GameDatabase.GetFleetInfo(this.SelectedFleet).SystemID);
				StarSystemInfo starSystemInfo2 = this._app.GameDatabase.GetStarSystemInfo(this.TargetSystem);
				float length = (starSystemInfo1.Origin - starSystemInfo2.Origin).Length;
				List<int> intList = new List<int>();
				intList.Add(0);
				List<Vector3> list = Kerberos.Sots.StarFleet.StarFleet.GetAccelGateSlotsBetweenSystems(this._app.GameDatabase, starSystemInfo1.ID, starSystemInfo2.ID).ToList<Vector3>();
				int count = list.Count;
				for (int index = 0; index < count; ++index)
					intList.Add((int)((double)(starSystemInfo1.Origin - list[index]).Length / (double)length * 100.0));
				intList.Add(100);
				foreach (int num in intList)
					this.App.UI.AddSliderNotch(this.UI.Path(this.ID, "LYSlider"), num);
				this._PotentialGates = intList;
				this.App.UI.SetSliderAutoSnap(this.UI.Path(this.ID, "LYSlider"), true);
				this._app.UI.SetText(this._app.UI.Path(this.ID, "LYSlider", "right_label"), this.GetLYValueFromPercent(this._currentslider).ToString("0.00"));
			}
			else
			{
				this.App.UI.SetVisible(this.UI.Path(this.ID, "LYSlider"), false);
				this.App.UI.SetVisible(this.UI.Path(this.ID, "gamePlaceAccelerator"), false);
				this.App.UI.SetVisible(this.UI.Path(this.ID, "AccelListBox"), false);
			}
		}

		private void UpdateAccelList()
		{
			this._app.UI.ClearItems("AccelList");
			foreach (int potentialGate in this._PotentialGates)
			{
				this._app.UI.AddItem("AccelList", "", potentialGate, "", "gate_Toggle");
				string itemGlobalId = this._app.UI.GetItemGlobalID("AccelList", "", potentialGate, "");
				this.App.UI.SetText(this._app.UI.Path(itemGlobalId, "loa_gate_button", "idle", "menulabel"), this.GetLYValueFromPercent(potentialGate).ToString("0.00") + "LY - Gate");
				this.App.UI.SetText(this._app.UI.Path(itemGlobalId, "loa_gate_button", "mouse_over", "menulabel"), this.GetLYValueFromPercent(potentialGate).ToString("0.00") + "LY - Gate");
				this.App.UI.SetText(this._app.UI.Path(itemGlobalId, "loa_gate_button", "pressed", "menulabel"), this.GetLYValueFromPercent(potentialGate).ToString("0.00") + "LY - Gate");
				this.App.UI.SetText(this._app.UI.Path(itemGlobalId, "loa_gate_button", "disabled", "menulabel"), this.GetLYValueFromPercent(potentialGate).ToString("0.00") + "LY - Gate");
				this.App.UI.SetChecked(this._app.UI.Path(itemGlobalId, "Loa_gate_toggle"), (potentialGate == 0 || potentialGate == 100 ? 1 : (this._GatePoints.Contains(potentialGate) ? 1 : 0)) != 0);
				this.App.UI.SetEnabled(this._app.UI.Path(itemGlobalId, "Loa_gate_toggle"), (potentialGate == 0 ? 0 : (potentialGate != 100 ? 1 : 0)) != 0);
				if (this._currentslider == potentialGate)
				{
					this.App.UI.SetVisible(this._app.UI.Path(itemGlobalId, "selected"), true);
					this.App.UI.SetVisible(this._app.UI.Path(itemGlobalId, "unselected"), false);
				}
				else
				{
					this.App.UI.SetVisible(this._app.UI.Path(itemGlobalId, "selected"), false);
					this.App.UI.SetVisible(this._app.UI.Path(itemGlobalId, "unselected"), true);
				}
				this.App.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "loa_gate_button"), "id", "loa_gate_button|" + potentialGate.ToString());
				this.App.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "Loa_gate_toggle"), "id", "Loa_gate_toggle|" + potentialGate.ToString());
			}
			DesignInfo designInfo = this._app.GameDatabase.GetDesignInfosForPlayer(this._app.LocalPlayer.ID, RealShipClasses.Cruiser, true).FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.IsAccelerator()));
			int num = 0;
			if (designInfo != null)
				num = (this._GatePoints.Count<int>() + 1) * designInfo.ProductionCost;
			this.App.UI.SetText("accelCubeCost", string.Format(App.Localize("@UI_MISSION_DEPLOYACCEL_REQUIREDCUBES"), (object)num.ToString("N0")));
			this.UpdateCanConfirmMission();
		}

		protected override void OnCommitMission()
		{
			Kerberos.Sots.StarFleet.StarFleet.SetNPGMission(this.App.Game, this.SelectedFleet, this.TargetSystem, this._useDirectRoute, this._GatePoints, this.GetDesignsToBuild(), new int?());
			this.App.GetGameState<StarMapState>().RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_NORMAL);
		}

		protected override void RefreshMissionDetails(StationType stationType = StationType.INVALID_TYPE, int stationLevel = 1)
		{
			base.RefreshMissionDetails(stationType, stationLevel);
			this.DistributeSliderNotches();
			this.UpdateAccelList();
		}

		protected override string GetMissionDetailsTitle()
		{
			return string.Format("DEPLOY NPG {0}", (object)this.App.GameDatabase.GetStarSystemInfo(this.TargetSystem).Name.ToUpperInvariant());
		}

		protected override void OnRefreshMissionDetails(MissionEstimate estimate)
		{
			this.AddCommonMissionTimes(estimate);
			string hint = "NPG MISSION";
			this.AddMissionTime(2, App.Localize("DEPLOY NPG MISSION"), estimate.TurnsAtTarget, hint);
			this.AddMissionCost(estimate);
			this.UpdateCanConfirmMission();
		}

		protected override IEnumerable<int> GetMissionTargetPlanets()
		{
			return Enumerable.Empty<int>();
		}

		private float GetLYValueFromPercent(int percent)
		{
			FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(this.SelectedFleet);
			if (fleetInfo == null)
				return 0.0f;
			StarSystemInfo starSystemInfo1 = this._app.GameDatabase.GetStarSystemInfo(fleetInfo.SystemID);
			StarSystemInfo starSystemInfo2 = this._app.GameDatabase.GetStarSystemInfo(this.TargetSystem);
			if (fleetInfo == null || starSystemInfo1 == (StarSystemInfo)null || starSystemInfo2 == (StarSystemInfo)null)
				return 0.0f;
			float length = (starSystemInfo2.Origin - starSystemInfo1.Origin).Length;
			float num = length * ((float)percent / 100f);
			return ((starSystemInfo2.Origin - starSystemInfo1.Origin) * (num / length) + starSystemInfo1.Origin - starSystemInfo1.Origin).Length;
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "slider_notched")
			{
				if (panelName == "LYSlider")
				{
					this._currentslider = int.Parse(msgParams[0]);
					this._app.UI.SetText(this._app.UI.Path(this.ID, "LYSlider", "right_label"), this.GetLYValueFromPercent(this._currentslider).ToString("0.00"));
					this.UpdateAccelList();
				}
			}
			else if (msgType == "button_clicked")
			{
				if (panelName == "gamePlaceAccelerator")
				{
					if (this._currentslider != 0 && this._currentslider != 100)
					{
						if (!this._GatePoints.Contains(this._currentslider))
							this._GatePoints.Add(this._currentslider);
						else
							this._GatePoints.Remove(this._currentslider);
						this.UpdateAccelList();
					}
				}
				else if (panelName.StartsWith("Loa_gate_toggle"))
				{
					int num = int.Parse(panelName.Split('|')[1]);
					if (this._PotentialGates.Contains(num))
					{
						if (this._GatePoints.Contains(num))
							this._GatePoints.Remove(num);
						else
							this._GatePoints.Add(num);
						this._currentslider = num;
						this.App.UI.SetSliderValue(this.UI.Path(this.ID, "LYSlider"), this._currentslider);
						this.UpdateAccelList();
					}
				}
				else if (panelName.StartsWith("loa_gate_button"))
				{
					int num = int.Parse(panelName.Split('|')[1]);
					if (this._PotentialGates.Contains(num))
					{
						this._currentslider = num;
						this.App.UI.SetSliderValue(this.UI.Path(this.ID, "LYSlider"), this._currentslider);
						this.UpdateAccelList();
					}
				}
			}
			base.OnPanelMessage(panelName, msgType, msgParams);
		}
	}
}
