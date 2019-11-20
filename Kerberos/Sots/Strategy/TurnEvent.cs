// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.TurnEvent
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.Strategy
{
	internal class TurnEvent
	{
		public string EventDesc = string.Empty;
		public string EventName = string.Empty;
		public string EventSoundCueName = string.Empty;
		public string Param1 = string.Empty;
		public string NamesList = string.Empty;
		public int ID;
		public TurnEventType EventType;
		public TurnEventMessage EventMessage;
		public int TurnNumber;
		public bool ShowsDialog;
		public bool dialogShown;
		public bool EventViewed;
		public int PlayerID;
		public int SystemID;
		public int SystemID2;
		public int OrbitalID;
		public int ColonyID;
		public int FleetID;
		public int AdmiralID;
		public int TechID;
		public int MissionID;
		public int DesignID;
		public int TargetPlayerID;
		public int TreatyID;
		public int FeasibilityPercent;
		public int SpecialProjectID;
		public int ShipID;
		public int ProvinceID;
		public WeaponEnums.PlagueType PlagueType;
		public float ImperialPop;
		public float CivilianPop;
		public float Infrastructure;
		public SectionEnumerations.DesignAttribute DesignAttribute;
		public int ArrivalTurns;
		public int NumShips;
		public double Savings;
		public int CombatID;

		public bool IsCombatEvent
		{
			get
			{
				if (this.EventType != TurnEventType.EV_COMBAT_WIN && this.EventType != TurnEventType.EV_COMBAT_LOSS)
					return this.EventType == TurnEventType.EV_COMBAT_DRAW;
				return true;
			}
		}

		public PlayerTechInfo.PrimaryKey PlayerTechID
		{
			get
			{
				return new PlayerTechInfo.PrimaryKey()
				{
					PlayerID = this.PlayerID,
					TechID = this.TechID
				};
			}
		}

		public void RebuildEventDesc(GameDatabase db)
		{
			this.EventDesc = App.Localize("@" + this.EventMessage.ToString()).Inject((object)new TurnEventMacros(db.AssetDatabase, db, this));
		}

		public void RebuildEventName(GameDatabase db)
		{
			this.EventName = App.Localize("@" + this.EventType.ToString()).Inject((object)new TurnEventMacros(db.AssetDatabase, db, this));
		}

		public string GetEventName(GameSession game)
		{
			if (string.IsNullOrEmpty(this.EventName))
				this.RebuildEventName(game.GameDatabase);
			return this.EventName;
		}

		public string GetEventMessage(GameSession game)
		{
			if (string.IsNullOrEmpty(this.EventDesc))
				this.RebuildEventDesc(game.GameDatabase);
			return this.EventDesc;
		}

		public static string GetTurnEventSprite(GameSession game, TurnEvent e)
		{
			string factionName1 = game.GameDatabase.GetFactionName(game.GameDatabase.GetPlayerFactionID(e.PlayerID));
			string factionName2 = game.GameDatabase.GetFactionName(game.GameDatabase.GetPlayerFactionID(e.TargetPlayerID));
			switch (e.EventMessage)
			{
				case TurnEventMessage.EM_SYSTEM_SURVEYED:
					return "ui\\events\\system_explored.tga";
				case TurnEventMessage.EM_SHIPS_BUILT_SINGLE:
					return string.Format("ui\\events\\single_{0}_shipbuilt.tga", (object)factionName1);
				case TurnEventMessage.EM_SHIPS_BUILT_MULTIPLE:
					return string.Format("ui\\events\\multi_{0}_shipsbuilt.tga", (object)factionName1);
				case TurnEventMessage.EM_PROTOTYPE_COMPLETE:
					return "ui\\events\\prototype_completed.tga";
				case TurnEventMessage.EM_NO_RESEARCH:
					return "ui\\events\\no_research.tga";
				case TurnEventMessage.EM_RESEARCH_COMPLETE:
					return "ui\\events\\research_completed.tga";
				case TurnEventMessage.EM_FEASIBILITY_STUDY_COMPLETE_VERY_BAD:
					return "ui\\events\\feasibility_very_bad.tga";
				case TurnEventMessage.EM_FEASIBILITY_STUDY_COMPLETE_BAD:
					return "ui\\events\\feasibility_bad.tga";
				case TurnEventMessage.EM_FEASIBILITY_STUDY_COMPLETE_GOOD:
					return "ui\\events\\feasibility_good.tga";
				case TurnEventMessage.EM_FEASIBILITY_STUDY_COMPLETE_VERY_GOOD:
					return "ui\\events\\feasibility_very_good.tga";
				case TurnEventMessage.EM_GATE_SHIP_DEPLOYED:
					return "ui\\events\\gate_ship_deployed.tga";
				case TurnEventMessage.EM_GATE_STATION_DEPLOYED:
					return "ui\\events\\gate_station_deployed.tga";
				case TurnEventMessage.EM_COLONY_ESTABLISHED:
					return string.Format("ui\\events\\colony_estab_{0}.tga", (object)factionName1);
				case TurnEventMessage.EM_COLONY_SUPPORT:
					if (!(factionName1 == "loa"))
						return string.Format("ui\\events\\colony_supplied_{0}.tga", (object)factionName1);
					return "ui\\events\\colony_sup_loa.tga";
				case TurnEventMessage.EM_COLONY_SUPPORT_COMPLETE:
					if (!(factionName1 == "loa"))
						return string.Format("ui\\events\\colony_supplied_{0}_ended.tga", (object)factionName1);
					return "ui\\events\\colony_sup_loa.tga";
				case TurnEventMessage.EM_COLONY_SELF_SUFFICIENT:
					return string.Format("ui\\events\\colony_selfsuf_{0}.tga", (object)factionName1);
				case TurnEventMessage.EM_COLONY_ABANDONED:
					return string.Format("ui\\events\\colony_abandoned_{0}.tga", (object)factionName1);
				case TurnEventMessage.EM_STATION_BUILT:
					return "ui\\events\\station_built.tga";
				case TurnEventMessage.EM_STATION_UPGRADED:
					return "ui\\events\\station_upgraded.tga";
				case TurnEventMessage.EM_SUPERWORLD_COMPLETE:
					return "ui\\events\\gemworld_created.tga";
				case TurnEventMessage.EM_COMBAT_DRAW:
					return "ui\\events\\event_combat_draw.tga";
				case TurnEventMessage.EM_COMBAT_WIN:
					return "ui\\events\\event_combat_win.tga";
				case TurnEventMessage.EM_COMBAT_LOSS:
					return "ui\\events\\event_combat_lose.tga";
				case TurnEventMessage.EM_BETRAYAL:
				case TurnEventMessage.EM_BETRAYED:
					return "ui\\events\\Betrayal.tga";
				case TurnEventMessage.EM_RELOCATION_COMPLETE:
					return "ui\\events\\ev_relocation_complete.tga";
				case TurnEventMessage.EM_OVERHARVEST_WARNING:
					return "ui\\events\\overharvest_warning.tga";
				case TurnEventMessage.EM_REBELLION_STARTING:
					return "ui\\events\\civil_disorder.tga";
				case TurnEventMessage.EM_REBELLION_STARTED:
					return "ui\\events\\revolution_begun.tga";
				case TurnEventMessage.EM_REBELLION_ONGOING:
					return "ui\\events\\revolution_begun.tga";
				case TurnEventMessage.EM_REBELLION_ENDED_WIN:
					return "ui\\events\\revolution_defeated.tga";
				case TurnEventMessage.EM_REBELLION_ENDED_LOSS:
					return "ui\\events\\revolution_won.tga";
				case TurnEventMessage.EM_GOVERNMENT_TYPE_CHANGED:
					return "ui\\events\\govt_type_changed.tga";
				case TurnEventMessage.EM_INTEL_MISSION_FAILED:
					return "ui\\events\\spy_caught.tga";
				case TurnEventMessage.EM_RESEARCH_NEVER_COMPLETE:
					return "ui\\events\\research_incompleteable.tga";
				case TurnEventMessage.EM_GATE_CAPACITY_REACHED:
					return "ui\\events\\gate_capacity_reached.tga";
				case TurnEventMessage.EM_NODE_LINE_COLLAPSE:
					return "ui\\events\\em_node_line_collapse.tga";
				case TurnEventMessage.EM_NODE_LINE_COLLAPSE_FLEET_LOSS:
					return "ui\\events\\em_node_line_collapse_fleet_loss.tga";
				case TurnEventMessage.EM_SUULKA_ARRIVES:
					return string.Format("ui\\events\\{0}_arrives.tga", game.GameDatabase.GetShipInfo(e.ShipID, true) != null ? (object)game.GameDatabase.GetShipInfo(e.ShipID, true).DesignInfo.Name.ToLower().Replace(" ", "") : (object)"?");
				case TurnEventMessage.EM_SUULKA_LEAVES:
					return string.Format("ui\\events\\{0}_leaves.tga", game.GameDatabase.GetShipInfo(e.ShipID, true) != null ? (object)game.GameDatabase.GetShipInfo(e.ShipID, true).DesignInfo.Name.ToLower().Replace(" ", "") : (object)"?");
				case TurnEventMessage.EM_ADMIRAL_PROMOTED:
					return string.Format("ui\\events\\promote_admiral_{0}.tga", (object)factionName1);
				case TurnEventMessage.EM_TREATY_REQUESTED:
					return "ui\\events\\ev_treaty_requested.tga";
				case TurnEventMessage.EM_TREATY_ACCEPTED:
					return "ui\\events\\treaty_formed.tga";
				case TurnEventMessage.EM_TREATY_DECLINED:
					return "ui\\events\\ev_treaty_declined.tga";
				case TurnEventMessage.EM_TREATY_EXPIRED:
					return "ui\\events\\ev_treaty_expired.tga";
				case TurnEventMessage.EM_TREATY_BROKEN_OFFENDER:
					return "ui\\events\\treaty_broken_by_you.tga";
				case TurnEventMessage.EM_TREATY_BROKEN_VICTIM:
					return "ui\\events\\treaty_broken_by_others.tga";
				case TurnEventMessage.EM_REQUEST_REQUESTED:
					return "ui\\events\\ev_request_requested.tga";
				case TurnEventMessage.EM_REQUEST_DECLINED:
					return "ui\\events\\ev_request_declined.tga";
				case TurnEventMessage.EM_REQUEST_ACCEPTED:
					return "ui\\events\\ev_request_declined.tga";
				case TurnEventMessage.EM_DEMAND_REQUESTED:
					return "ui\\events\\ev_demand_requested.tga";
				case TurnEventMessage.EM_DEMAND_DECLINED:
					return "ui\\events\\ev_demand_declined.tga";
				case TurnEventMessage.EM_DEMAND_ACCEPTED:
					return "ui\\events\\ev_demand_accepted.tga";
				case TurnEventMessage.EM_PLAGUE_STARTED:
				case TurnEventMessage.EM_RESEARCH_PLAGUE_DISASTER:
					return "ui\\events\\plague_starts.tga";
				case TurnEventMessage.EM_PLAGUE_ENDED:
					return "ui\\events\\plague_ended.tga";
				case TurnEventMessage.EM_PLAGUE_DAMAGE_POP:
					return "ui\\events\\em_plague_damage_pop.tga";
				case TurnEventMessage.EM_PLAGUE_DAMAGE_STRUCT:
					return "ui\\events\\em_plague_damage_struct.tga";
				case TurnEventMessage.EM_PLAGUE_DAMAGE_POP_STRUCT:
					return "ui\\events\\em_plague_damage_pop_struct.tga";
				case TurnEventMessage.EM_ADMIRAL_DEAD:
					return "ui\\events\\ev_admiral_dead.tga";
				case TurnEventMessage.EM_ADMIRAL_RETIRED:
					return "ui\\events\\ev_admiral_retired.tga";
				case TurnEventMessage.EM_SALVAGE_PROJECT_COMPLETE:
					return "ui\\events\\ev_salvage_project_complete.tga";
				case TurnEventMessage.EM_NEW_SALVAGE_PROJECT:
					return "ui\\events\\ev_new_salvage_project.tga";
				case TurnEventMessage.EM_NEW_SPECIAL_PROJECT:
					return "ui\\events\\ev_new_special_project.tga";
				case TurnEventMessage.EM_MONITOR_PROJECT_COMPLETE:
					return "ui\\events\\ev_monitor_project_complete.tga";
				case TurnEventMessage.EM_GARDENER_PROJECT_COMPLETE:
				case TurnEventMessage.EM_GARDENER_SYSTEM_FOUND:
				case TurnEventMessage.EM_GARDENER_CAPTURED:
				case TurnEventMessage.EM_INCOMING_GARDENER:
					return "ui\\events\\ev_gardener_system_found.tga";
				case TurnEventMessage.EM_INTEL_MISSION_CRITICAL_FAILED:
					return "ui\\events\\spy_failed.tga";
				case TurnEventMessage.EM_INTEL_MISSION_CRITICAL_FAILED_LEAK:
					return "ui\\events\\spy_caught.tga";
				case TurnEventMessage.EM_INTEL_MISSION_NO_RANDOM_SYSTEM:
					return "ui\\events\\ev_mission_no_random_system.tga";
				case TurnEventMessage.EM_INTEL_MISSION_RANDOM_SYSTEM:
					return "ui\\events\\ev_mission_random_system.tga";
				case TurnEventMessage.EM_INTEL_MISSION_NO_HIGHEST_TRADE_SYSTEM:
					return "ui\\events\\ev_mission_no_highest_trade_system.tga";
				case TurnEventMessage.EM_INTEL_MISSION_HIGHEST_TRADE_SYSTEM:
					return "ui\\events\\ev_mission_highest_trade_system.tga";
				case TurnEventMessage.EM_INTEL_MISSION_NO_NEWEST_COLONY_SYSTEM:
					return "ui\\events\\ev_mission_no_newest_colony_system.tga";
				case TurnEventMessage.EM_INTEL_MISSION_NEWEST_COLONY_SYSTEM:
					return "ui\\events\\ev_mission_newest_colony_system.tga";
				case TurnEventMessage.EM_INTEL_MISSION_CURRENT_TECH:
					return "ui\\events\\ev_mission_current_tech.tga";
				case TurnEventMessage.EM_INTEL_MISSION_NO_COMPLETE_TECHS:
					return "ui\\events\\ev_mission_no_complete_techs.tga";
				case TurnEventMessage.EM_INTEL_MISSION_RECENT_TECH:
					return "ui\\events\\ev_mission_recent_tech.tga";
				case TurnEventMessage.EM_INTEL_MISSION_CRITICAL_SUCCESS:
					return "ui\\events\\spy_successful.tga";
				case TurnEventMessage.EM_WAR_DECLARED_DEFENDER:
					return "ui\\events\\war_declared_on_you.tga";
				case TurnEventMessage.EM_WAR_DECLARED_AGGRESSOR:
					return "ui\\events\\war_declared_by_you.tga";
				case TurnEventMessage.EM_ADMIRAL_CAPTURED:
					return string.Format("ui\\events\\{0}_admiral_captured.tga", (object)factionName1);
				case TurnEventMessage.EM_ADMIRAL_ESCAPES:
					return string.Format("ui\\events\\{0}_admiral_escapes.tga", (object)factionName1);
				case TurnEventMessage.EM_ADMIRAL_DEFECTS:
					return string.Format("ui\\events\\{0}_admiral_joins_rebels.tga", (object)factionName1);
				case TurnEventMessage.EM_ASTEROID_STORM:
					return "ui\\events\\asteroid_storm.tga";
				case TurnEventMessage.EM_ATTRIBUTES_DISCOVERED:
					return "ui\\events\\attribute_discovered_from_design.tga";
				case TurnEventMessage.EM_INCOMING_ALIEN_FLEET:
					return "ui\\events\\incoming_alien_fleet.tga";
				case TurnEventMessage.EM_INDY_ASSIMILATED:
					return "ui\\events\\indy_assimilated.tga";
				case TurnEventMessage.EM_INDY_PROTECTORATE:
					return "ui\\events\\indy_joins_your_protectorate.tga";
				case TurnEventMessage.EM_LOCUST_INFESTATION_DEFEATED:
					return "ui\\events\\locust_infestation_defeated.tga";
				case TurnEventMessage.EM_LOCUST_SHIP_DESTROYED:
					return "ui\\events\\locusts_ship_destroyed.tga";
				case TurnEventMessage.EM_LOCUST_INCOMING:
					return "ui\\events\\locusts_incoming.tga";
				case TurnEventMessage.EM_LOCUST_SPOTTED:
					return "ui\\events\\locusts_spotted.tga";
				case TurnEventMessage.EM_MONITOR_FOUND:
					return "ui\\events\\monitor_complex_found.tga";
				case TurnEventMessage.EM_MONITOR_PROJECT_AVAILABLE:
					return "ui\\events\\monitor_complex_project_available.tga";
				case TurnEventMessage.EM_MONITOR_CAPTURED:
					return "ui\\events\\monitor_taken_over.tga";
				case TurnEventMessage.EM_PIRATE_RAID:
				case TurnEventMessage.EM_PIRATE_BASE_DESTROYED:
					return "ui\\events\\pirate_raid.tga";
				case TurnEventMessage.EM_PROTEANS_REMOVED:
					return "ui\\events\\proteans_cleared.tga";
				case TurnEventMessage.EM_RESEARCH_FAILED:
					return "ui\\events\\research_failed.tga";
				case TurnEventMessage.EM_RETROFIT_COMPLETE_SINGLE:
					return "ui\\events\\retrofit_ship_complete.tga";
				case TurnEventMessage.EM_RETROFIT_COMPLETE_MULTI:
					return "ui\\events\\retrofit_multiships_complete.tga";
				case TurnEventMessage.EM_RETROFIT_COMPLETE_STATION:
					return "ui\\events\\retrofit_station_complete.tga";
				case TurnEventMessage.EM_SHIPS_SCATTERED_NODE_CANNON:
					return "ui\\events\\ships_scattered_by_nodecannon.tga";
				case TurnEventMessage.EM_SHIPS_RECYCLED:
					return "ui\\events\\recycle_ships.tga";
				case TurnEventMessage.EM_SLAVER_ATTACK:
					return "ui\\events\\slavers_attack.tga";
				case TurnEventMessage.EM_SLAVES_DEAD:
					return "ui\\events\\slaves_dead.tga";
				case TurnEventMessage.EM_SPECTRE_ATTACK:
					return "ui\\events\\specters.tga";
				case TurnEventMessage.EM_SWARM_DESTROYED:
					return "ui\\events\\swarm_cleared.tga";
				case TurnEventMessage.EM_SWARM_ENCOUNTERED:
					return "ui\\events\\swarm_encountered.tga";
				case TurnEventMessage.EM_SWARM_INFESTATION:
					return "ui\\events\\swarm_infests_system.tga";
				case TurnEventMessage.EM_SWARM_QUEEN_DESTROYED:
					return "ui\\events\\swarm_queen_destroyed.tga";
				case TurnEventMessage.EM_SWARM_QUEEN_INCOMING:
					return "ui\\events\\swarm_queen_incoming.tga";
				case TurnEventMessage.EM_SWARM_QUEEN_SPOTTED:
					return "ui\\events\\swarm_queen_spotted.tga";
				case TurnEventMessage.EM_SYS_KILLER_DESTROYED:
					return "ui\\events\\systemkiller_destroyed.tga";
				case TurnEventMessage.EM_SYS_KILLER_INCOMING:
					return "ui\\events\\systemkiller_incoming.tga";
				case TurnEventMessage.EM_SYS_KILLER_LEAVING:
					return "ui\\events\\systemkiller_leaves.tga";
				case TurnEventMessage.EM_SYS_KILLER_SPOTTED:
					return "ui\\events\\systemkiller_spotted.tga";
				case TurnEventMessage.EM_TOMB_DEFENDERS_DESTROYED:
					return "ui\\events\\tomb_defenders_destroyed.tga";
				case TurnEventMessage.EM_TOMB_DESTROYED:
					return "ui\\events\\tombs_destroyed.tga";
				case TurnEventMessage.EM_TOMB_DISCOVERED:
					return "ui\\events\\tombs_discovered.tga";
				case TurnEventMessage.EM_VN_COLLECTOR_ATTACK:
					return "ui\\events\\ev_vn_collector_attack.tga";
				case TurnEventMessage.EM_VN_SEEKER_ATTACK:
					return "ui\\events\\ev_vn_seeker_attack.tga";
				case TurnEventMessage.EM_VN_BERSERKER_ATTACK:
					return "ui\\events\\vn_berserker_attack.tga";
				case TurnEventMessage.EM_VN_SYS_KILLER_ATTACK:
					return "ui\\events\\vn_systemkiller_attack.tga";
				case TurnEventMessage.EM_VN_HW_DEFEATED:
					return "ui\\events\\ev_defeated_enemy_homeworld.tga";
				case TurnEventMessage.EM_SUULKA_DIES:
					return string.Format("ui\\events\\{0}_dies.tga", game.GameDatabase.GetDesignInfo(e.DesignID) != null ? (object)game.GameDatabase.GetDesignInfo(e.DesignID).Name.ToLower().Replace(" ", "") : (object)"?");
				case TurnEventMessage.EM_MISSION_COMPLETE:
					return "ui\\events\\fleet_arrives.tga";
				case TurnEventMessage.EM_AI_REBELLION_END:
					return "ui\\events\\ev_ai_rebellion_end.tga";
				case TurnEventMessage.EM_AI_REBELLION_START:
				case TurnEventMessage.EM_RESEARCH_AI_DISASTER:
					return "ui\\events\\ev_ai_rebellion_start.tga";
				case TurnEventMessage.EM_SLAVES_DELIVERED:
					return "ui\\events\\slaves_delivered.tga";
				case TurnEventMessage.EM_ALLIANCE_CREATED:
					return "ui\\events\\alliance_created.tga";
				case TurnEventMessage.EM_ALLIANCE_DISSOLVED:
					return "ui\\events\\alliance_disbanded.tga";
				case TurnEventMessage.EM_ASSIMILATION_PLAGUE_PLANET_GAINED:
					return "ui\\events\\plague_assimilated.tga";
				case TurnEventMessage.EM_ASSIMILATION_PLAGUE_PLANET_LOST:
					return "ui\\events\\plague_assimilated.tga";
				case TurnEventMessage.EM_BANKRUPTCY_IMMINENT:
					return "ui\\events\\bankruptcy_imminent.tga";
				case TurnEventMessage.EM_BANKRUPTCY_AVOIDED:
					return "ui\\events\\bankruptcy_avoided.tga";
				case TurnEventMessage.EM_BANKRUPTCY_COLONY_LOST:
					return string.Format("ui\\events\\colony_abandoned_{0}.tga", (object)factionName1);
				case TurnEventMessage.EM_BIOWEAPON_STRIKE:
					return "ui\\events\\bioweapon_strike.tga";
				case TurnEventMessage.EM_INVOICES_COMPLETE:
					return "ui\\events\\buildorder_complete.tga";
				case TurnEventMessage.EM_COLONY_ACQUIRED:
					return "ui\\events\\colony_aquired.tga";
				case TurnEventMessage.EM_EMPIRE_DESTROYED:
					return "ui\\events\\empire_eliminated.tga";
				case TurnEventMessage.EM_EMPIRE_ENCOUNTERED:
					return "ui\\events\\peaceful_encounter_deepspace.tga";
				case TurnEventMessage.EM_FARCAST_FAILED:
					return "ui\\events\\farcast_fail.tga";
				case TurnEventMessage.EM_FARCAST_SUCCESS:
					return "ui\\events\\farcast_success.tga";
				case TurnEventMessage.EM_FLEET_DESTROYED:
					return "ui\\events\\event_singlefleet_destroyed.tga";
				case TurnEventMessage.EM_FREIGHTER_BUILT:
				case TurnEventMessage.EM_TRADE_STIMULUS:
					return string.Format("ui\\events\\{0}_freighter_built.tga", (object)factionName1);
				case TurnEventMessage.EM_FREIGHTERS_BUILT:
					return string.Format("ui\\events\\{0}_multi_freighters_built.tga", (object)factionName1);
				case TurnEventMessage.EM_GATE_DESTROYED:
					return "ui\\events\\gate_capacity_reached.tga";
				case TurnEventMessage.EM_GATE_JUMP_ABORTED:
					return "ui\\events\\gate_jump_aborted.tga";
				case TurnEventMessage.EM_COLONY_STIMULUS:
					return "ui\\events\\ev_colony_stimulus.tga";
				case TurnEventMessage.EM_MINING_STIMULUS:
					return "ui\\events\\ev_mining_stimulus.tga";
				case TurnEventMessage.EM_LEFT_ALLIANCE:
					return "ui\\events\\left_alliance.tga";
				case TurnEventMessage.EM_PLANET_DESTROYED:
					return "ui\\events\\planet_destroyed.tga";
				case TurnEventMessage.EM_PLANET_LIFE_DRAINED:
					return "ui\\events\\planet_life_drained.tga";
				case TurnEventMessage.EM_PLANET_PSI_DRAINED:
					return "ui\\events\\planet_psi_drained.tga";
				case TurnEventMessage.EM_PLANET_NO_RESOURCES:
					return "ui\\events\\ev_planet_no_resources.tga";
				case TurnEventMessage.EM_PLAYER_SURRENDERED_TO_YOU:
					return "ui\\events\\player_surrendered.tga";
				case TurnEventMessage.EM_YOU_SURRENDER:
					return "ui\\events\\you_surrendered_to_player.tga";
				case TurnEventMessage.EM_PROVINCE_SURRENDERED_TO_YOU:
					return "ui\\events\\ev_province_surrendered_to_you.tga";
				case TurnEventMessage.EM_SYSTEM_SURRENDERED_TO_YOU:
					return "ui\\events\\systems_surrendered.tga";
				case TurnEventMessage.EM_SAVINGS_SURRENDERED_TO_YOU:
					return "ui\\events\\savings_surrendered.tga";
				case TurnEventMessage.EM_STATION_DESTROYED:
					return "ui\\events\\savings_surrendered.tga";
				case TurnEventMessage.EM_STATION_UPGRADABLE:
					return "ui\\events\\station_upgrade_ready.tga";
				case TurnEventMessage.EM_SDB_BUILT:
					return "ui\\events\\sdb_station_built.tga";
				case TurnEventMessage.EM_SDBS_BUILT:
					return "ui\\events\\sdb_station_built.tga";
				case TurnEventMessage.EM_COMBAT_DETECTED:
					return "ui\\events\\combat_detected.tga";
				case TurnEventMessage.EM_FLEET_DISBANDED:
					return "ui\\events\\ev_fleet_disbanded.tga";
				case TurnEventMessage.EM_FLEET_REDIRECTED:
					return "ui\\events\\syskill_fleet_rerouted.tga";
				case TurnEventMessage.EM_HOMEWORLD_REESTABLISHED:
					return "ui\\events\\ev_homeworld_reestablished.tga";
				case TurnEventMessage.EM_COLONY_DESTROYED:
					if (!(factionName1 == "loa"))
						return "ui\\events\\ev_colony_destroyed.tga";
					return string.Format("ui\\events\\colony_destroyed_{0}.tga", (object)factionName1);
				case TurnEventMessage.EM_SURVEY_INDEPENDENT_RACE_FOUND:
					return string.Format("ui\\events\\encounter_{0}.tga", (object)factionName2);
				case TurnEventMessage.EM_ADMIRAL_INTEL_LEAK_GIVE:
					return "ui\\events\\admiral_info_leaked.tga";
				case TurnEventMessage.EM_ADMIRAL_INTEL_LEAK_TAKE:
					return "ui\\events\\admiral_info_taken.tga";
				case TurnEventMessage.EM_COUNTER_INTEL_SUCCESS:
					return "ui\\events\\ev_counter_intel_success.tga";
				case TurnEventMessage.EM_COUNTER_INTEL_CRITICAL_SUCCESS:
					return "ui\\events\\ev_counter_intel_critical_success.tga";
				case TurnEventMessage.EM_COUNTER_INTEL_FAILURE:
					return "ui\\events\\ev_counter_intel_failure.tga";
				case TurnEventMessage.EM_COUNTER_INTEL_CRITICAL_FAILURE:
					return "ui\\events\\ev_counter_intel_critical_failure.tga";
				case TurnEventMessage.EM_INCOMING_ASTEROID_SHOWER:
					return "ui\\events\\asteroid_storm.tga";
				case TurnEventMessage.EM_INCOMING_SPECTORS:
					return "ui\\events\\ev_incoming_spectors.tga";
				case TurnEventMessage.EM_INCOMING_GHOST_SHIP:
					return "ui\\events\\ev_incoming_ghostship.tga";
				case TurnEventMessage.EM_INCOMING_SLAVERS:
					return "ui\\events\\ev_incoming_slavers.tga";
				case TurnEventMessage.EM_INCOMING_LOCUST:
					return "ui\\events\\locusts_incoming.tga";
				case TurnEventMessage.EM_INCOMING_COMET:
					return "ui\\events\\ev_incoming_comet.tga";
				case TurnEventMessage.EM_INCOMING_SYSTEMKILLER:
					return "ui\\events\\systemkiller_incoming.tga";
				case TurnEventMessage.EM_BOOSTFAILED_1:
					return "ui\\events\\research_disaster_lvl1.tga";
				case TurnEventMessage.EM_BOOSTFAILED_2:
					return "ui\\events\\research_disaster_lvl2.tga";
				case TurnEventMessage.EM_BOOSTFAILED_3:
					return "ui\\events\\research_disaster_lvl3.tga";
				case TurnEventMessage.EM_RECEIVED_MONEY:
					return "ui\\events\\aid_money.tga";
				case TurnEventMessage.EM_RECEIVED_RESEARCH_MONEY:
					return "ui\\events\\aid_research.tga";
				case TurnEventMessage.EM_LOA_CUBES_ABANDONED:
				case TurnEventMessage.EM_LOA_CUBES_ABANDONED_DEEPSPACE:
					return "ui\\events\\loa_cubes_abandoned.tga";
				case TurnEventMessage.EM_NPG_DEPLOYED:
					return "ui\\events\\npg_deployed.tga";
				default:
					return "ui\\events\\events_static_noise.tga";
			}
		}

		public static void DebugPrintNewsEventArt(GameSession game)
		{
			List<TurnEventMessage> turnEventMessageList1 = new List<TurnEventMessage>();
			List<TurnEventMessage> turnEventMessageList2 = new List<TurnEventMessage>();
			foreach (TurnEventMessage turnEventMessage in Enum.GetValues(typeof(TurnEventMessage)))
			{
				TurnEvent e = new TurnEvent();
				e.TargetPlayerID = game.LocalPlayer.ID;
				e.PlayerID = game.LocalPlayer.ID;
				e.EventMessage = turnEventMessage;
				SuulkaInfo suulkaInfo = game.GameDatabase.GetSuulkas().FirstOrDefault<SuulkaInfo>();
				if (suulkaInfo != null)
				{
					e.ShipID = suulkaInfo.ShipID;
					e.DesignID = game.GameDatabase.GetShipInfo(suulkaInfo.ShipID, false).DesignID;
				}
				string turnEventSprite = TurnEvent.GetTurnEventSprite(game, e);
				if (turnEventSprite == "ui\\events\\events_static_noise.tga")
					turnEventMessageList2.Add(turnEventMessage);
				else if (turnEventSprite == "" || !File.Exists(game.App.GameRoot + "\\" + turnEventSprite + "~"))
					turnEventMessageList1.Add(turnEventMessage);
			}
			App.Log.Trace("==MISSING NEWS EVENT HOOKUPS==", "log");
			foreach (int num in turnEventMessageList2)
				App.Log.Trace(((TurnEventMessage)num).ToString(), "log");
			App.Log.Trace("==MISSING NEWS EVENT ART==", "log");
			foreach (int num in turnEventMessageList1)
				App.Log.Trace(((TurnEventMessage)num).ToString(), "log");
		}
	}
}
