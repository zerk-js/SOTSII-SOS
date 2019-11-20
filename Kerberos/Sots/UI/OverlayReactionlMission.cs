// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.OverlayReactionlMission
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class OverlayReactionlMission : OverlayMission
	{
		private static string UI_REACTIONLIST_PANEL = "ReactionList_Panel";
		private static string UI_CURRENTFLEETLIST = "gameReactionCurrentFleet";
		private List<OverlayReactionlMission.ReactionUIContainer> _containers;
		private OverlayReactionlMission.ReactionUIContainer _selectedReaction;
		private List<ReactionInfo> _reactions;
		protected FleetWidget _reactionfleet;

		public OverlayReactionlMission(App game, StarMapState state, StarMap starmap, string template = "OverlayReaction")
		  : base(game, state, starmap, MissionType.REACTION, template)
		{
			this._containers = new List<OverlayReactionlMission.ReactionUIContainer>();
		}

		protected override void OnCanConfirmMissionChanged(bool newValue)
		{
		}

		protected override bool CanConfirmMission()
		{
			return true;
		}

		protected override void OnEnter()
		{
			base.OnEnter();
			this._reactionfleet = new FleetWidget(this.App, this.App.UI.Path(this.ID, OverlayReactionlMission.UI_CURRENTFLEETLIST));
			this._reactionfleet.ScrapEnabled = false;
			this._reactionfleet.SetEnabled(false);
			this.PathDrawEnabled = false;
			this._containers.Clear();
			this._reactions = this._app.Game.GetPendingReactions().Where<ReactionInfo>((Func<ReactionInfo, bool>)(x => x.fleet.PlayerID == this._app.LocalPlayer.ID)).ToList<ReactionInfo>();
			foreach (ReactionInfo reaction in this._reactions)
			{
				this._app.UI.AddItem(OverlayReactionlMission.UI_REACTIONLIST_PANEL, "", reaction.fleet.ID, "");
				string itemGlobalId = this._app.UI.GetItemGlobalID(OverlayReactionlMission.UI_REACTIONLIST_PANEL, "", reaction.fleet.ID, "");
				this._app.UI.SetText(this.App.UI.Path(itemGlobalId, "fleetname"), reaction.fleet.Name);
				string propertyValue = "ReactionButton|" + reaction.fleet.ID.ToString();
				this._app.UI.SetPropertyString(this.UI.Path(itemGlobalId, "reaction_button"), "id", propertyValue);
				this._containers.Add(new OverlayReactionlMission.ReactionUIContainer()
				{
					Reaction = reaction,
					ListItemID = itemGlobalId,
					buttonID = propertyValue,
					TargetFleet = new int?()
				});
			}
			this._fleetWidget.EnemySelectionEnabled = true;
			this._fleetWidget.OnFleetSelectionChanged += new FleetWidget.FleetSelectionChangedDelegate(this.OnFleetSelectionChanged);
			this._fleetWidget.MissionMode = MissionType.NO_MISSION;
			this._starMap.FocusEnabled = true;
			this._starMap.SelectEnabled = true;
			foreach (int key in this._starMap.Systems.Reverse.Keys)
			{
				bool flag = false;
				foreach (OverlayReactionlMission.ReactionUIContainer container in this._containers)
				{
					foreach (FleetInfo fleetInfo in container.Reaction.fleetsInRange)
					{
						if (fleetInfo.SystemID == key)
						{
							flag = true;
							break;
						}
					}
					if (container.Reaction.fleet.SystemID == key)
						flag = true;
					if (flag)
						break;
				}
				this._starMap.Systems.Reverse[key].SetIsEnabled(flag);
				this._starMap.Systems.Reverse[key].SetIsSelectable(flag);
			}
			this.SelectReaction(this._containers.First<OverlayReactionlMission.ReactionUIContainer>());
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if (!this.GetShown() || this._selectedReaction == null || (this._fleetWidget.SelectedFleet != 0 || !this._selectedReaction.TargetFleet.HasValue))
				return;
			this.OnFleetSelectionChanged(this._app, 0);
		}

		protected override void RefreshMissionDetails(StationType type = StationType.INVALID_TYPE, int stationLevel = 1)
		{
			if (this.TargetSystem == 0)
				return;
			string missionDetailsTitle = this.GetMissionDetailsTitle();
			if (this._selectedReaction != null && this._selectedReaction.Reaction != null && this._selectedReaction.TargetFleet.HasValue)
			{
				int systemId = this._selectedReaction.Reaction.fleetsInRange.FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(x => x.ID == this._selectedReaction.TargetFleet.Value)).SystemID;
				this.App.UI.ClearItems("gameMissionTimes");
				this.App.UI.ClearItems("gameMissionNotes");
				MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this.App.Game, MissionType.REACTION, type, this._selectedReaction.Reaction.fleet.ID, systemId, this.SelectedPlanet, this.GetDesignsToBuild(), stationLevel, false, new float?(), new float?());
				if (missionEstimate != null)
					this._missionEstimate = missionEstimate;
				missionDetailsTitle += string.Format(App.Localize("@UI_MISSION_ETA_TURNS"), (object)this._missionEstimate.TurnsToTarget);
				this.OnRefreshMissionDetails(this._missionEstimate);
				this.App.UI.AutoSizeContents("gameMissionDetails");
			}
			this.App.UI.SetText(this.App.UI.Path(this.ID, "gameMissionTitle"), missionDetailsTitle);
		}

		public void OnFleetSelectionChanged(App game, int selectedFleet)
		{
			if (selectedFleet == 0 && this._selectedReaction != null || selectedFleet == this._selectedReaction.Reaction.fleet.ID)
			{
				this.SelectedFleet = this._selectedReaction.Reaction.fleet.ID;
				this._selectedReaction.TargetFleet = new int?();
				this.FocusOnStarSystem(this._selectedReaction.Reaction.fleet.SystemID);
				this._app.UI.SetVisible(this.UI.Path(this._selectedReaction.ListItemID, "reaction_anim"), false);
			}
			else
			{
				if (this._selectedReaction == null)
					return;
				this._selectedReaction.TargetFleet = new int?(selectedFleet);
				this.SelectedFleet = this._selectedReaction.TargetFleet.Value;
				this.FocusOnStarSystem(this._selectedReaction.Reaction.fleetsInRange.FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(x => x.ID == selectedFleet)).SystemID);
				this._app.UI.SetVisible(this.UI.Path(this._selectedReaction.ListItemID, "reaction_anim"), true);
			}
		}

		private void SelectReaction(
		  OverlayReactionlMission.ReactionUIContainer reaction)
		{
			if (this._selectedReaction != null)
				this._app.UI.SetVisible(this.UI.Path(this._selectedReaction.ListItemID, "reaction_selection"), false);
			this._selectedReaction = reaction;
			if (this._selectedReaction == null)
				return;
			this.SelectedFleet = this._selectedReaction.Reaction.fleet.ID;
			this._app.UI.SetVisible(this.UI.Path(this._selectedReaction.ListItemID, "reaction_selection"), true);
			this._reactionfleet.SetSyncedFleets(this._selectedReaction.Reaction.fleet.ID);
			OverlayMission.RefreshFleetAdmiralDetails(this.App, this.ID, this._selectedReaction.Reaction.fleet.ID, "admiralDetails1");
			this.SelectedFleet = 0;
			this.SelectedPlanet = 0;
			this._fleetWidget.Selected = -1;
			this._fleetWidget.SelectedFleet = 0;
			this._fleetWidget.SetSyncedFleets(this._selectedReaction.Reaction.fleetsInRange);
			if (this._selectedReaction.TargetFleet.HasValue)
			{
				this._fleetWidget.Selected = this._selectedReaction.TargetFleet.Value;
				this._fleetWidget.SelectedFleet = this._selectedReaction.TargetFleet.Value;
			}
			else
			{
				this.SelectedFleet = 0;
				this.SelectedPlanet = 0;
				this._fleetWidget.Selected = -1;
				this._fleetWidget.SelectedFleet = 0;
			}
			this.FocusOnStarSystem(this._selectedReaction.Reaction.fleet.SystemID);
			this._systemWidget.Sync(this._selectedReaction.Reaction.fleet.SystemID);
		}

		protected override void OnExit()
		{
			if (this._reactionfleet != null)
				this._reactionfleet.Dispose();
			base.OnExit();
		}

		protected override void OnCommitMission()
		{
			foreach (OverlayReactionlMission.ReactionUIContainer container in this._containers)
			{
				OverlayReactionlMission.ReactionUIContainer rui = container;
				if (rui.TargetFleet.HasValue)
				{
					FleetInfo fleetInfo = rui.Reaction.fleetsInRange.FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(x =>
				   {
					   int id = x.ID;
					   int? targetFleet = rui.TargetFleet;
					   if (id == targetFleet.GetValueOrDefault())
						   return targetFleet.HasValue;
					   return false;
				   }));
					int systemId = rui.Reaction.fleet.SystemID;
					this._app.GameDatabase.ChangeDiplomacyState(rui.Reaction.fleet.PlayerID, fleetInfo.PlayerID, DiplomacyState.WAR);
					this._app.GameDatabase.UpdateFleetLocation(rui.Reaction.fleet.ID, fleetInfo.SystemID, new int?());
					MissionInfo missionByFleetId = this._app.GameDatabase.GetMissionByFleetID(rui.Reaction.fleet.ID);
					if (missionByFleetId == null)
					{
						Kerberos.Sots.StarFleet.StarFleet.SetRelocationMission(this._app.Game, rui.Reaction.fleet.ID, systemId, false, (List<int>)null);
					}
					else
					{
						List<WaypointInfo> list = this._app.GameDatabase.GetWaypointsByMissionID(missionByFleetId.ID).ToList<WaypointInfo>();
						foreach (WaypointInfo waypointInfo in list)
							this._app.GameDatabase.RemoveWaypoint(waypointInfo.ID);
						this._app.GameDatabase.InsertWaypoint(missionByFleetId.ID, WaypointType.TravelTo, new int?(systemId));
						foreach (WaypointInfo waypointInfo in list)
							this._app.GameDatabase.InsertWaypoint(missionByFleetId.ID, waypointInfo.Type, waypointInfo.SystemID);
					}
				}
				this._app.Game.RemoveReaction(rui.Reaction);
			}
			if (!this._app.GameSetup.IsMultiplayer)
				this._app.Game.Phase4_Combat();
			else if (this._app.Network.IsJoined)
			{
				this._app.GameDatabase.LogComment("SYNC REACTIONS");
				this._app.Network.SendHistory(this._app.GameDatabase.GetTurnCount());
			}
			else
			{
				if (!this._app.Network.IsHosting)
					return;
				this._app.Network.ReactionComplete();
			}
		}

		protected override string GetMissionDetailsTitle()
		{
			return string.Format(App.Localize("@UI_REACTIONS_PENDING"));
		}

		protected override void OnRefreshMissionDetails(MissionEstimate estimate)
		{
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (this._containers.Any<OverlayReactionlMission.ReactionUIContainer>((Func<OverlayReactionlMission.ReactionUIContainer, bool>)(x => x.buttonID == panelName)))
					this.SelectReaction(this._containers.FirstOrDefault<OverlayReactionlMission.ReactionUIContainer>((Func<OverlayReactionlMission.ReactionUIContainer, bool>)(x => x.buttonID == panelName)));
				else if (panelName == "selectionClear")
				{
					this.SelectedFleet = 0;
					this.SelectedPlanet = 0;
					this._fleetWidget.Selected = -1;
					this._fleetWidget.SelectedFleet = 0;
				}
			}
			base.OnPanelMessage(panelName, msgType, msgParams);
		}

		protected override IEnumerable<int> GetMissionTargetPlanets()
		{
			return Enumerable.Empty<int>();
		}

		private class ReactionUIContainer
		{
			public string buttonID;
			public string ListItemID;
			public ReactionInfo Reaction;
			public int? TargetFleet;
		}
	}
}
