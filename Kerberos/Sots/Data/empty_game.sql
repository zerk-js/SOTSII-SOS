PRAGMA cache_size = 50000; 
PRAGMA wal_autocheckpoint=100000;
PRAGMA temp_store = 2;

CREATE TABLE [admiral_traits] 
(
	[admiral_id] INTEGER NOT NULL REFERENCES [admirals](id) ON DELETE CASCADE,
	[type] INTEGER NOT NULL,
	[level] INTEGER,
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE
);

CREATE TABLE [admirals] 
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[player_id] INTEGER REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[orbital_object_id] INTEGER REFERENCES [orbital_objects](id) ON UPDATE CASCADE ON DELETE SET NULL,
	[name] TEXT,
	[race] TEXT,

	[age] NUMERIC,
	[gender] INTEGER,
	[reaction] INTEGER,
	[evasion] INTEGER,
	[loyalty] INTEGER,

	[battles_fought] INTEGER,
	[battles_won] INTEGER,
	[missions_assigned] INTEGER,
	[missions_accomplished] INTEGER,
	[turn_created] INTEGER,

	[engram] BOOLEAN NOT NULL DEFAULT 'False'
);

CREATE TABLE [sting_operations]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[target_player] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE [intel_missions]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[target_player] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[blame_player] INTEGER REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[turn] INTEGER NOT NULL,
	[type] INTEGER NOT NULL
);

CREATE TABLE [intel_responses]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[intel_mission_id] INTEGER REFERENCES [intel_missions](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[auto] BOOLEAN NOT NULL,
	[value] TEXT
);

CREATE TABLE [ai] 
(
	[player_id] INTEGER NOT NULL PRIMARY KEY REFERENCES [players](id) ON DELETE CASCADE,
	[stance] INTEGER NOT NULL,
	[flags] INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE [ai_tech_styles]
(
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE,
	[tech_family] INTEGER NOT NULL,
	[cost_factor] NUMERIC NOT NULL
);

CREATE TABLE [ai_fleet_info]
(
	[id] INTEGER PRIMARY KEY,
	[player_id] INTEGER REFERENCES [players](id) ON DELETE CASCADE,
	[fleet_id] INTEGER REFERENCES [fleets](id) ON DELETE CASCADE,
	[fleet_type] INTEGER,
	[system_id] INTEGER REFERENCES [star_systems](id) ON DELETE CASCADE,
	[invoice_id] INTEGER REFERENCES [invoice_instances](id) ON DELETE SET NULL,
	[admiral_id] INTEGER REFERENCES [admirals](id) ON DELETE SET NULL,
	[fleet_template] TEXT NOT NULL
);

CREATE TABLE [ai_intel_battle_reports]
(
	[player_id] INTEGER NOT NULL REFERENCES [players](id)
);

CREATE TABLE [ai_intel_colonies]
(
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[owning_player] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[planet_id] INTEGER NOT NULL REFERENCES [planets](orbital_object_id) ON DELETE CASCADE ON UPDATE CASCADE,
	[colony_id] INTEGER REFERENCES [colonies](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[last_seen] INTEGER NOT NULL,

	[imperial_population] NUMERIC NOT NULL,

	UNIQUE ([player_id], [planet_id]) PRIMARY KEY ([player_id], [planet_id])
);

CREATE TABLE [ai_intel_planets]
(
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[planet_id] INTEGER NOT NULL REFERENCES [planets](orbital_object_id) ON DELETE CASCADE ON UPDATE CASCADE,
	[last_seen] INTEGER NOT NULL,

	[biosphere] INTEGER NOT NULL,
	[resources] INTEGER NOT NULL,
	[infrastructure] NUMERIC NOT NULL,
	[suitability] NUMERIC NOT NULL,

	UNIQUE ([player_id], [planet_id]) PRIMARY KEY ([player_id], [planet_id])
);

CREATE TABLE [ai_intel_designs] 
(
	[player_id] INTEGER NOT NULL REFERENCES [players](id),
	[intel_on_player_id] INTEGER NOT NULL REFERENCES [players](id),
	[design_id] INTEGER NOT NULL REFERENCES [designs](id),
	[first_seen] INTEGER,
	[last_seen] INTEGER,

	[salvaged] BOOLEAN
);

CREATE TABLE [ai_intel_factions]
(
	[player_id] INTEGER NOT NULL REFERENCES [players](id),
	[intel_on_player_id] INTEGER NOT NULL REFERENCES [players](id),
	[favoured_wpn] TEXT,
	[has_antimatter] BOOLEAN,
	[has_dreadnoughts] BOOLEAN,

	[has_leviathans] BOOLEAN,
	[most_advanced_wpn_tech] INTEGER REFERENCES [techs](id)
);

CREATE TABLE [ai_intel_fleets] 
(
	[player_id] INTEGER NOT NULL REFERENCES [players](id),
	[intel_on_player_id] INTEGER NOT NULL REFERENCES [players](id),
	[last_seen] INTEGER,
	[last_seen_system] INTEGER REFERENCES [star_systems](id) ON DELETE CASCADE,
	[last_seen_coords] TEXT,

	[num_destroyers] INTEGER,
	[num_cruisers] INTEGER,
	[num_dreadnoughts] INTEGER,
	[num_leviathans] INTEGER
);

CREATE TABLE [ai_intel_stations]
(
	[player_id] INTEGER NOT NULL REFERENCES [players](id),
	[owning_player] INTEGER NOT NULL REFERENCES [players](id),
	[station_id] INTEGER NOT NULL REFERENCES [stations](orbital_object_id) ON DELETE CASCADE ON UPDATE CASCADE,
	[last_seen] INTEGER,
	[level] INTEGER
);

CREATE TABLE [ai_tech_weights]
(
	[tech_family] TEXT NOT NULL,
	[ai_id] INTEGER NOT NULL REFERENCES [players](id),
	[total_spent] NUMERIC NOT NULL,
	[preference] NUMERIC NOT NULL
);

CREATE TABLE [alliances]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
);

CREATE TABLE [artifacts] 
(
	[orbital_object_id] INTEGER NOT NULL PRIMARY KEY REFERENCES [orbital_objects](id) ON UPDATE CASCADE ON DELETE CASCADE
);

CREATE TABLE [assets]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[path] TEXT NOT NULL UNIQUE
);

CREATE TABLE [asteroid_belts]
(
	[orbital_object_id] INTEGER NOT NULL PRIMARY KEY REFERENCES [orbital_objects](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[random_seed] INTEGER NOT NULL
);

CREATE TABLE [build_orders] 
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[design_id] INTEGER NOT NULL REFERENCES [designs](id) ON DELETE CASCADE,
	[build_priority] INTEGER NOT NULL,
	[star_system_id] INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[progress] INTEGER,

	[mission_id] INTEGER REFERENCES [missions](id) ON DELETE CASCADE,
	[ship_name] TEXT NOT NULL,
	[production_target] INTEGER NOT NULL,
	[invoice_instance_id] INTEGER REFERENCES [invoice_instances](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[ai_fleet_id] INTEGER REFERENCES [ai_fleet_info](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[loa_cubes] INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE [retrofit_orders] 
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[design_id] INTEGER NOT NULL REFERENCES [designs](id) ON DELETE CASCADE,
	[ship_id] INTEGER REFERENCES [ships](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[star_system_id] INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,

	[invoice_instance_id] INTEGER REFERENCES [invoice_instances](id) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE [retrofitstation_orders]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[design_id] INTEGER NOT NULL REFERENCES [designs](id) ON DELETE CASCADE,
	[ship_id] INTEGER REFERENCES [ships](id) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE [clientinfo] 
(
	[id] INTEGER
);

CREATE TABLE [colonies] 
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[orbital_object_id] INTEGER NOT NULL REFERENCES [orbital_objects](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE RESTRICT ON UPDATE CASCADE,	
	[imp_pop] NUMERIC NOT NULL,
	[civ_pop_weight] NUMERIC NOT NULL DEFAULT '0.5',
	
	[turn_established] INTEGER NOT NULL,
	[terra_rate] NUMERIC NOT NULL DEFAULT '0.50',
	[infra_rate] NUMERIC NOT NULL DEFAULT '0.50',
	[shipcon_rate] NUMERIC NOT NULL DEFAULT '0.0',
	[overharvest_rate] NUMERIC NOT NULL DEFAULT '0.0',
	
	[economy_rating] NUMERIC NOT NULL DEFAULT '0.5',
	[colony_stage] INTEGER NOT NULL DEFAULT 0,
	[overdevelop_progress] NUMERIC NOT NULL DEFAULT '0.0',
	[overdevelop_rate] NUMERIC NOT NULL DEFAULT '0.0',
	[population_biosphere_rate] NUMERIC NOT NULL DEFAULT '0.5',
	
	[hardened_structures] BOOLEAN NOT NULL DEFAULT 'False',
	[rebellion_type] INTEGER NOT NULL DEFAULT 0,
	[rebellion_turns] INTEGER NOT NULL DEFAULT 0,
	[turns_overharvesting] INTEGER NOT NULL DEFAULT 0,
	[repair_points] INTEGER NOT NULL DEFAULT 0,
	
	[slavework_rate] NUMERIC NOT NULL DEFAULT '0.0',
	[damaged_last_turn] BOOLEAN NOT NULL DEFAULT 'False',
	[repair_points_max] INTEGER NOT NULL DEFAULT 0,
	[owning_system] BOOLEAN NOT NULL DEFAULT 'False',
	[replicants_on] BOOLEAN NOT NULL DEFAULT 'False'
);

CREATE TABLE [colony_factions]
(
	[orbital_object_id] INTEGER NOT NULL REFERENCES [orbital_objects](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[faction_id] INT NOT NULL REFERENCES [factions](id) ON DELETE RESTRICT ON UPDATE CASCADE,
	[civ_pop] NUMERIC NOT NULL DEFAULT 0,
	[civ_morale] INTEGER NOT NULL DEFAULT 100,
	[civ_pop_weight] NUMERIC NOT NULL,

	[turn_established] INTEGER NOT NULL, 
	[civ_last_morale] INTEGER NOT NULL DEFAULT 100,

	CONSTRAINT "colony_faction_id" PRIMARY KEY ([faction_id], [orbital_object_id]) ON CONFLICT ABORT, 
	CONSTRAINT "colony_faction_id" UNIQUE ([faction_id], [orbital_object_id]) ON CONFLICT ABORT
);

CREATE TABLE [combat_data]
(
	[system_id] INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE ON INSERT CASCADE,
	[conflict_id] INTEGER NOT NULL DEFAULT 1,
	[turn] INTEGER NOT NULL, 
	[data] BLOB NOT NULL,
	[version] INTEGER NOT NULL DEFAULT 0,
	PRIMARY KEY( system_id, turn )
);

CREATE TABLE [control_zones]
(
	[system_id] INTEGER NOT NULL PRIMARY KEY UNIQUE REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE ON INSERT CASCADE,
	[player_id] INTEGER UNIQUE REFERENCES [players](id) ON DELETE SET NULL ON UPDATE CASCADE ON INSERT CASCADE,
	[index] TEXT NOT NULL
);

CREATE TABLE [design_attributes]
(
	[design_id] INTEGER REFERENCES [designs](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[attribute] INTEGER
);

CREATE TABLE [design_modules]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[design_section_id] INTEGER NOT NULL REFERENCES [design_sections](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[module_id] INTEGER NOT NULL REFERENCES [modules](id) ON DELETE RESTRICT ON UPDATE CASCADE,
	[weapon_id] INTEGER REFERENCES [weapons](id) ON DELETE RESTRICT ON UPDATE CASCADE,
	[mount_node] TEXT NOT NULL,

	[station_module_type] INTEGER,
	[design_id] INTEGER DEFAULT NULL REFERENCES [designs](id) ON DELETE RESTRICT ON UPDATE CASCADE
);

CREATE TABLE [module_psionics]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[design_module_id] INTEGER NOT NULL REFERENCES [design_modules](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[psionic] INTEGER NOT NULL
);

CREATE TABLE [design_section_weapon_banks]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[design_section_id] INTEGER NOT NULL REFERENCES [design_sections](id),
	[weapon_id] INTEGER REFERENCES [weapons](id),
	[design_id] INTEGER REFERENCES [designs](id),
	[bank_guid] TEXT NOT NULL,
	[firing_mode] INTEGER,
	[filter_mode] INTEGER
);

CREATE TABLE [design_sections]
(
	[id] INTEGER PRIMARY KEY,
	[design_id] INTEGER NOT NULL REFERENCES [designs](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[asset_id] INTEGER NOT NULL REFERENCES [assets](id) ON DELETE RESTRICT ON UPDATE CASCADE
);

CREATE TABLE [design_section_techs]
(
	[id] REFERENCES [design_sections](id) ON DELETE CASCADE,
	[tech_id] REFERENCES [techs](id) ON DELETE CASCADE
);

CREATE TABLE [designs_encountered]
(
	[player_id] INTEGER REFERENCES [players](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[design_id] INTEGER NOT NULL REFERENCES [designs](id) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE [designs]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[player_id] INTEGER REFERENCES [players](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[name] TEXT NOT NULL,
	[armour] INTEGER,
	[structure] NUMERIC,

	[num_turrets] INTEGER,
	[mass] NUMERIC,
	[range] NUMERIC,
	[thrust] NUMERIC,
	[top_speed] NUMERIC,

	[savings_cost] INTEGER,
	[production_cost] INTEGER,
	[class] INTEGER,
	[crew_avail] INTEGER NOT NULL DEFAULT 0,
	[power_avail] INTEGER NOT NULL DEFAULT 0,

	[supply_avail] INTEGER NOT NULL DEFAULT 0,
	[crew_req] INTEGER NOT NULL DEFAULT 0,
	[power_req] INTEGER NOT NULL DEFAULT 0,
	[supply_req] INTEGER NOT NULL DEFAULT 0,
	[num_built] INTEGER NOT NULL DEFAULT 0,

	[design_date] INTEGER NOT NULL,
	[role] INTEGER,
	[weapon_role] INTEGER,
	[is_prototyped] BOOLEAN NOT NULL,
	[is_attributes_discovered] BOOLEAN NOT NULL,

	[station_type] INTEGER,
	[station_level] INTEGER,
	[visible_to_player] TEXT NOT NULL DEFAULT 'True',
	[priority_weapon_name] TEXT NOT NULL,

	[num_destroyed] INTEGER NOT NULL DEFAULT 0,

	[retrofit_design_id] INTEGER DEFAULT NULL REFERENCES [designs](id)
);

CREATE TABLE [requests]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[initiating_player_id] INTEGER NOT NULL REFERENCES [players](id) ON UPDATE CASCADE,
	[receiving_player_id] INTEGER NOT NULL REFERENCES [players](id) ON UPDATE CASCADE,
	[request_type] INTEGER NOT NULL,
	[request_value] NUMERIC,
	[request_state] INTEGER NOT NULL
);

CREATE TABLE [gives]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[initiating_player_id] INTEGER NOT NULL REFERENCES [players](id) ON UPDATE CASCADE,
	[receiving_player_id] INTEGER NOT NULL REFERENCES [players](id) ON UPDATE CASCADE,
	[give_type] INTEGER NOT NULL,
	[give_value] NUMERIC
);

CREATE TABLE [demands]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[initiating_player_id] INTEGER NOT NULL REFERENCES [players](id) ON UPDATE CASCADE,
	[receiving_player_id] INTEGER NOT NULL REFERENCES [players](id) ON UPDATE CASCADE,
	[demand_type] INTEGER NOT NULL,
	[demand_value] NUMERIC,
	[demand_state] INTEGER NOT NULL
);

CREATE TABLE [treaties]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[initiating_player_id] INTEGER NOT NULL REFERENCES [players](id) ON UPDATE CASCADE,
	[receiving_player_id] INTEGER NOT NULL REFERENCES [players](id) ON UPDATE CASCADE,
	[treaty_type] INTEGER NOT NULL,
	[duration] INTEGER NOT NULL,
	[starting_turn] INTEGER NOT NULL,
	[active] BOOLEAN NOT NULL DEFAULT 'False',
	[removed] BOOLEAN NOT NULL DEFAULT 'False'
);

CREATE TABLE [armistice_treaties]
(
	[treaty_id] INTEGER NOT NULL REFERENCES [treaties](id) ON UPDATE CASCADE ON DELETE CASCADE,
	[suggested_diplomacy_state] INTEGER NOT NULL
);

CREATE TABLE [limitation_treaties]
(
	[treaty_id] INTEGER NOT NULL REFERENCES [treaties](id) ON UPDATE CASCADE ON DELETE CASCADE,
	[limitation_type] INTEGER NOT NULL,
	[limitation_amount] NUMERIC NOT NULL DEFAULT 0,
	[limitation_group] TEXT
);

CREATE TABLE [treaty_consequences]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[treaty_id] INTEGER NOT NULL REFERENCES [treaties](id) ON UPDATE CASCADE ON DELETE CASCADE,
	[consequence_type] INTEGER NOT NULL,
	[consequence_value] NUMERIC
);

CREATE TABLE [treaty_incentives]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[treaty_id] INTEGER NOT NULL REFERENCES [treaties](id) ON UPDATE CASCADE ON DELETE CASCADE,
	[incentive_type] INTEGER NOT NULL,
	[incentive_value] NUMERIC
);

CREATE TABLE [diplomacy_action_history] 
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON UPDATE CASCADE,
	[towards_player_id] INTEGER NOT NULL,
	[turn_count] INTEGER,
	[action] INTEGER NOT NULL,

	[action_sub_type] INTEGER,
	[action_data] NUMERIC,
	[duration] INTEGER,
	[consequence_type] INTEGER,
	[consequence_data] NUMERIC
);

CREATE TABLE [diplomacy_reaction_history] 
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON UPDATE CASCADE,
	[towards_player_id] INTEGER NOT NULL,
	[turn_count] INTEGER,
	[difference] INTEGER NOT NULL,

	[reaction] INTEGER NOT NULL
);

CREATE TABLE [diplomacy_states]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[towards_player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[diplomacy_type] INTEGER NOT NULL,
	[relations] INTEGER NOT NULL DEFAULT 0,

	[is_encountered] BOOLEAN NOT NULL DEFAULT 'False'
);

CREATE TABLE [encounters] 
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[type] TEXT NOT NULL
);

CREATE TABLE [explore_records]
(
	[system_id] INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[last_seen] INTEGER NOT NULL,
	[visible] BOOLEAN NOT NULL DEFAULT 'True',
	[explored] BOOLEAN NOT NULL DEFAULT 'True',

	PRIMARY KEY ([player_id], [system_id]) ON CONFLICT ABORT, UNIQUE ([player_id], [system_id]) ON CONFLICT ABORT
);

CREATE TABLE [factions]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[name] TEXT NOT NULL UNIQUE,
	[suitability] NUMERIC NOT NULL
);

CREATE TABLE [feasibility_projects]
(
	[project_id] INTEGER NOT NULL PRIMARY KEY REFERENCES [research_projects](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[tech_id] INTEGER NOT NULL REFERENCES [techs](id) ON DELETE RESTRICT ON UPDATE CASCADE,
	[research_cost] NUMERIC NOT NULL
);

CREATE TABLE [fleets] 
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE RESTRICT ON UPDATE CASCADE,
	[admiral_id] INTEGER REFERENCES [admirals](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[system_id] INTEGER REFERENCES [star_systems](id) ON DELETE RESTRICT ON UPDATE CASCADE,
	[supporting_system_id] INTEGER REFERENCES [star_systems](id) ON DELETE RESTRICT ON UPDATE CASCADE,

	[name] TEXT,
	[turns_away] INTEGER NOT NULL,
	[supply_remaining] NUMERIC NOT NULL,
	[fleet_type] INTEGER NOT NULL,	
	[prev_system_id] INTEGER DEFAULT NULL REFERENCES [star_systems](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[preferred] BOOLEAN NOT NULL DEFAULT 'False',
	[last_accel] INTEGER NOT NULL DEFAULT 0,
	[loa_fleet_config] INTEGER DEFAULT NULL REFERENCES [loa_fleet_compositions](id) ON DELETE SET NULL ON UPDATE CASCADE
);

CREATE TABLE [gen_unique]
(
	[generator] TEXT,
	[player_id] INTEGER,
	[current] INTEGER DEFAULT 0,
	PRIMARY KEY ([player_id], [generator])
);

CREATE TABLE [government_actions]
(
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[id] INTEGER NOT NULL PRIMARY KEY ASC AUTOINCREMENT UNIQUE,
	[description] TEXT NOT NULL,
	[authoritarianism_change] NUMERIC NOT NULL DEFAULT 0,
	[econ_liberalism_change] NUMERIC NOT NULL DEFAULT 0,
	[turn] INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE [governments]
(
	[player_id] INTEGER NOT NULL REFERENCES [players](id),
	[authoritarianism] NUMERIC NOT NULL DEFAULT 0,
	[economic_liberalism] NUMERIC NOT NULL DEFAULT 0,
	[current_type] TEXT NOT NULL DEFAULT 'Centrism'
);

CREATE TABLE [independent_colonies]
(
	[orbital_object_id] INTEGER NOT NULL PRIMARY KEY REFERENCES [orbital_objects](id) ON UPDATE CASCADE ON DELETE CASCADE,
	[race_id] INTEGER NOT NULL REFERENCES [independent_races](id),
	[population] NUMERIC NOT NULL
);

CREATE TABLE [independent_races]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[name] TEXT NOT NULL,
	[orbital_object_id] INTEGER NOT NULL REFERENCES [orbital_objects](id) ON UPDATE CASCADE ON DELETE CASCADE,
	[population] NUMERIC NOT NULL,
	[tech_level] INTEGER NOT NULL,

	[reaction_human] INTEGER,
	[reaction_tarka] INTEGER,
	[reaction_liir] INTEGER,
	[reaction_zuul] INTEGER,
	[reaction_morrigi] INTEGER,

	[reaction_hiver] INTEGER
);

CREATE TABLE [invoice_build_orders] 
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[design_id] INTEGER NOT NULL,
	[ship_name] TEXT NOT NULL,
	[invoice_id] INTEGER NOT NULL REFERENCES [invoices](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[loa_cubes]	INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE [invoice_instances]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[system_id] INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[name] TEXT NOT NULL
);

CREATE TABLE [invoices]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[invoice_name] TEXT,
	[is_favorite] BOOLEAN NOT NULL
);

CREATE TABLE [loa_fleet_compositions]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[invoice_name] TEXT,
	[visible] BOOLEAN NOT NULL DEFAULT 'True'
);

CREATE TABLE [loa_fleet_ship_def]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[composition_id] INTEGER NOT NULL REFERENCES [loa_fleet_compositions](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[design_id] INTEGER REFERENCES [designs](id) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE [large_asteroids] 
(
	[orbital_object_id] INTEGER NOT NULL REFERENCES [orbital_objects](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[resources] INTEGER NOT NULL
);

CREATE TABLE [player_empire_history]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[turn_id] INTEGER NOT NULL DEFAULT 0,
	[colonies] INTEGER NOT NULL DEFAULT 0,
	[provinces] INTEGER NOT NULL DEFAULT 0,
	[bases] INTEGER NOT NULL DEFAULT 0,
	[fleets] INTEGER NOT NULL DEFAULT 0,
	[ships] INTEGER NOT NULL DEFAULT 0,
	[empire_pop] DOUBLE NOT NULL DEFAULT 0,
	[empire_economy] FLOAT NOT NULL DEFAULT 0,
	[empire_biosphere] INTEGER NOT NULL DEFAULT 0,
	[empire_trade] DOUBLE NOT NULL DEFAULT 0,
	[empire_morale] INTEGER NOT NULL DEFAULT 0,
	[savings] DOUBLE NOT NULL DEFAULT 0,
	[psi_potential] INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE [player_colony_history]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[colony_id] INTEGER NOT NULL,
	[turn_id] INTEGER NOT NULL DEFAULT 0,
	[resources] INTEGER NOT NULL DEFAULT 0,
	[biosphere] INTEGER NOT NULL DEFAULT 0,
	[infrastructure] FLOAT NOT NULL DEFAULT 0,
	[hazard] FLOAT NOT NULL DEFAULT 0,
	[income] INTEGER NOT NULL DEFAULT 0,
	[econ_rating] FLOAT NOT NULL DEFAULT 0,
	[life_support_cost] INTEGER NOT NULL DEFAULT 0,
	[industrial_output] INTEGER NOT NULL DEFAULT 0,
	[civ_pop] DOUBLE NOT NULL DEFAULT 0,
	[imp_pop] DOUBLE NOT NULL DEFAULT 0,
	[slave_pop] DOUBLE NOT NULL DEFAULT 0
);

CREATE TABLE [colony_morale_history]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[colony_id] INTEGER REFERENCES [colonies](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[turn_id] INTEGER NOT NULL DEFAULT 0,
	[faction_id] INTEGER NOT NULL,
	[morale] INTEGER NOT NULL,
	[population] DOUBLE NOT NULL
);

Create Table [morale_event_history]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[turn] INTEGER NOT NULL DEFAULT 0,
	[event_type] INTEGER NOT NULL,
	[player_id] INTEGER NOT NULL,
	[value] INTEGER NOT NULL DEFAULT 0,
	[colony_id] INTEGER,
	[system_id] INTEGER,
	[province_id] INTEGER
);

CREATE TABLE [missions] 
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[fleet_id] INTEGER NOT NULL REFERENCES [fleets](id) ON DELETE CASCADE,
	[type] INTEGER NOT NULL DEFAULT 0,
	[target_system_id] INTEGER REFERENCES [star_systems](id) ON UPDATE CASCADE ON DELETE RESTRICT,
	[target_orbital_object_id] INTEGER REFERENCES [orbital_objects](id) ON UPDATE CASCADE ON DELETE RESTRICT,

	[target_fleet_id] INTEGER REFERENCES [fleets](id) ON UPDATE CASCADE ON DELETE SET NULL,
	[duration] INTEGER,
	[use_direct_route] BOOLEAN NOT NULL DEFAULT 'False',
	[turn_started] INTEGER NOT NULL DEFAULT 1,
	[system_started] INTEGER REFERENCES [star_systems](id) ON UPDATE CASCADE ON DELETE SET NULL,
	[station_type] INTEGER DEFAULT NULL
);

CREATE TABLE [colony_traps] 
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[system_id] INTEGER REFERENCES [star_systems](id),
	[planet_id] INTEGER REFERENCES [planets](orbital_object_id),
	[fleet_id] INTEGER NOT NULL REFERENCES [fleets](id) ON DELETE CASCADE
);

CREATE TABLE [modules]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[asset_id] INTEGER NOT NULL REFERENCES [assets](id),
	[player_id] INTEGER REFERENCES [players](id)
);

CREATE TABLE [move_orders]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[fleet_id] INTEGER NOT NULL REFERENCES [fleets](id) ON DELETE CASCADE,
	[from_system] INTEGER REFERENCES [star_systems](id) ON DELETE RESTRICT ON UPDATE CASCADE,
	[from_coord] TEXT,
	[to_system] INTEGER REFERENCES [star_systems](id) ON DELETE RESTRICT ON UPDATE CASCADE,

	[to_coord] TEXT,
	[progress] NUMERIC
);

CREATE TABLE [name_value_pairs]
(
	[name] TEXT NOT NULL PRIMARY KEY UNIQUE,
	[value] TEXT
);

CREATE TABLE [orbital_objects]
(
	[id] INTEGER NOT NULL PRIMARY KEY,
	[parent_id] INTEGER REFERENCES [orbital_objects](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[star_system_id] INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[orbital_path] TEXT NOT NULL DEFAULT '0,0,0,0,0,0,0',
	[name] TEXT
);

CREATE TABLE [planets]
(
	[orbital_object_id] INTEGER NOT NULL PRIMARY KEY UNIQUE REFERENCES [orbital_objects](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[type] TEXT NOT NULL,
	[ring_id] INTEGER REFERENCES [orbital_objects](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[suitability] NUMERIC,
	[biosphere] INTEGER,

	[resources] INTEGER,
	[size] NUMERIC NOT NULL DEFAULT 1,
	[infrastructure] NUMERIC NOT NULL DEFAULT 0,
	[max_resources] INTEGER
);

CREATE TABLE [player_diplomacy_points] 
(
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[faction_id] INTEGER NOT NULL REFERENCES [factions](id),
	[points] INTEGER NOT NULL DEFAULT 0,
	PRIMARY KEY ([faction_id], [player_id]), UNIQUE ([faction_id], [player_id])
);

CREATE TABLE [player_tech_branches] 
(
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE ON INSERT CASCADE,
	[from_id] INTEGER NOT NULL REFERENCES [techs](id) ON DELETE CASCADE ON UPDATE CASCADE ON INSERT CASCADE,
	[to_id] INTEGER NOT NULL REFERENCES [techs](id) ON DELETE CASCADE ON UPDATE CASCADE ON INSERT CASCADE,
	[research_cost] NONE NOT NULL,
	[feasibility] NUMERIC NOT NULL,

	UNIQUE ([from_id], [to_id],	[player_id]) PRIMARY KEY ([from_id], [to_id], [player_id])
);

CREATE TABLE [player_techs]
(
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE ON INSERT CASCADE,
	[tech_id] INTEGER NOT NULL REFERENCES [techs](id) ON DELETE CASCADE ON UPDATE CASCADE ON INSERT CASCADE,
	[state] INTEGER NOT NULL CHECK(state >= 0 AND state <= 7) DEFAULT 0,
	[progress] INTEGER NOT NULL CHECK(progress >= 0) DEFAULT 0,
	[research_cost] INTEGER NOT NULL,

	[feasibility] NUMERIC NOT NULL,
	[player_feasibility] NUMERIC NOT NULL,
	[turn_researched] NUMERIC,
	PRIMARY KEY ([research_cost], [tech_id], [player_id]), UNIQUE ([research_cost],	[tech_id], [player_id])
);

CREATE TABLE [special_projects] 
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[player_id] INTEGER REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE ON INSERT CASCADE,
	[name] TEXT NOT NULL,
	[progress] INT DEFAULT -1,
	[cost] INT DEFAULT 0,
	[type] INT NOT NULL DEFAULT 0,
	[tech_id] INTEGER NOT NULL DEFAULT 0,
	[rate] FLOAT NOT NULL DEFAULT 0,
	[encounter_id] INTEGER REFERENCES [encounters](id) ON DELETE CASCADE ON UPDATE CASCADE ON INSERT CASCADE,
	[fleet_id] INTEGER REFERENCES [fleets](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[target_player_id] INTEGER REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE ON INSERT CASCADE
);

CREATE TABLE [player_client_info]
(
	[player_id] INTEGER PRIMARY_KEY REFERENCES [players](id) ON DELETE CASCADE,
	[username] TEXT NOT NULL
);

CREATE TABLE [players] 
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[alliance_id] INTEGER REFERENCES [alliances](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[name] TEXT NOT NULL ON CONFLICT FAIL UNIQUE ON CONFLICT FAIL,
	[faction_id] INTEGER NOT NULL REFERENCES [factions](id) ON DELETE RESTRICT ON UPDATE CASCADE,
	[is_ai] BOOLEAN NOT NULL,

	[homeworld_id] INTEGER REFERENCES [orbital_objects](id) ON DELETE SET NULL,
	[primary_color_id] INTEGER NOT NULL REFERENCES [primary_player_colors](id) ON DELETE RESTRICT ON UPDATE CASCADE,
	[secondary_color] TEXT NOT NULL,
	[badge_asset_id] INTEGER NOT NULL DEFAULT 'ON' REFERENCES [assets](id) ON DELETE SET DEFAULT ON UPDATE CASCADE,
	[savings] NUMERIC,

	[avatar_asset_id] INTEGER NOT NULL REFERENCES [assets](id),
	[last_combat_turn] INTEGER,
	[last_encounter_turn] INTEGER,
	[rate_government_research] NUMERIC,
	[rate_research_current_project] NUMERIC,

	[rate_research_special_projects] NUMERIC,
	[rate_research_salvage_research] NUMERIC,
	[rate_government_stimulus] NUMERIC,
	[rate_government_security] NUMERIC,
	[rate_government_savings] NUMERIC,

	[rate_stimulus_mining] NUMERIC,
	[rate_stimulus_colonization] NUMERIC,
	[rate_stimulus_trade] NUMERIC,
	[rate_security_operations] NUMERIC,
	[rate_security_intelligence] NUMERIC,

	[rate_security_counter_intelligence] NUMERIC,
	[is_standard_player] BOOLEAN NOT NULL DEFAULT 'True',
	[generic_diplomacy_points] INTEGER NOT NULL DEFAULT 0,
	[rate_tax] NUMERIC NOT NULL DEFAULT '0.05',
	[rate_immigration] NUMERIC NOT NULL DEFAULT '0.05',

	[intel_points] INTEGER NOT NULL DEFAULT 0,
	[counterintel_points] INTEGER NOT NULL DEFAULT 0,
	[operations_points] INTEGER NOT NULL DEFAULT 0,
	[intel_accumulator] INTEGER NOT NULL DEFAULT 0,
	[counterintel_accumulator] INTEGER NOT NULL DEFAULT 0,

	[operations_accumulator] INTEGER NOT NULL DEFAULT 0,
	[civilian_mining_accumulator] INTEGER NOT NULL DEFAULT 0,
	[civilian_colonization_accumulator] INTEGER NOT NULL DEFAULT 0,
	[civilian_trade_accumulator] INTEGER NOT NULL DEFAULT 0,
	[subfaction_index] INTEGER NOT NULL DEFAULT 0,
	[additional_research_points] INTEGER NOT NULL DEFAULT 0,

	[bankruptcy_turns] INTEGER NOT NULL DEFAULT 0,
	[psionic_potential] INTEGER NOT NULL DEFAULT 0,
	[is_defeated] BOOLEAN NOT NULL DEFAULT 'False',
	[current_trade_income] NUMERIC NOT NULL DEFAULT 0,
	[include_in_diplomacy] BOOLEAN NOT NULL DEFAULT 'False',
	[is_ai_rebellion_player] BOOLEAN NOT NULL DEFAULT 'False',
	[auto_placedefenses] BOOLEAN NOT NULL DEFAULT 'False',
	[auto_repairfleets] BOOLEAN NOT NULL DEFAULT 'False',
	[auto_usegoop] BOOLEAN NOT NULL DEFAULT 'False',
	[auto_usejoker] BOOLEAN NOT NULL DEFAULT 'False',

	[research_boost_funds] NUMERIC NOT NULL DEFAULT 0,
	[auto_aoe] BOOLEAN NOT NULL DEFAULT 'False',
	[team] INTEGER NOT NULL DEFAULT 0,

	[auto_patrol] BOOLEAN NOT NULL DEFAULT 'False',
	[ai_difficulty] TEXT NOT NULL DEFAULT 'Normal',
	[rate_tax_prev] NUMERIC NOT NULL DEFAULT '0.05'
);

CREATE TABLE [primary_player_colors]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[color] TEXT NOT NULL UNIQUE
);

CREATE TABLE [provinces]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[name] TEXT UNIQUE ON CONFLICT ABORT,
	[capital_id] INTEGER UNIQUE ON CONFLICT ABORT
);

CREATE TABLE [query_history] 
(
	[id] INTEGER NOT NULL PRIMARY KEY ASC AUTOINCREMENT UNIQUE,
	[turn] INTEGER,
	[query] TEXT
);

CREATE TABLE [research_projects]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE RESTRICT ON UPDATE CASCADE,
	[state] INTEGER NOT NULL DEFAULT 0,
	[name] TEXT NOT NULL,
	[progress] NUMERIC NOT NULL DEFAULT 0
);

CREATE TABLE [satellites]
(
	[orbital_object_id] INTEGER NOT NULL PRIMARY KEY REFERENCES [orbital_objects](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE RESTRICT ON UPDATE CASCADE
);

-- 3/5/2012 DG: station_id is no longer used
CREATE TABLE [section_instances] 
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[section_id] INTEGER NOT NULL REFERENCES [design_sections](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[ship_id] INTEGER REFERENCES [ships](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[station_id] INTEGER REFERENCES [stations](orbital_object_id) ON DELETE CASCADE ON UPDATE CASCADE,
	[structure] INTEGER NOT NULL,

	[supply] NUMERIC NOT NULL,
	[crew] INTEGER NOT NULL,
	[signature] NUMERIC NOT NULL,
	[repair_points] INTEGER NOT NULL
);

CREATE TABLE [armor_instances]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[section_instance_id] INTEGER NOT NULL REFERENCES [section_instances](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[side] INTEGER NOT NULL,
	[data] TEXT
);

CREATE TABLE [sections] 
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[asset_id] INTEGER NOT NULL REFERENCES [assets](id),
	[player_id] INTEGER REFERENCES [players](id)
);

CREATE TABLE [ships] 
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[fleet_id] INTEGER REFERENCES [fleets](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[design_id] INTEGER NOT NULL REFERENCES [designs](id) ON DELETE RESTRICT ON UPDATE CASCADE,
	[parent_id] INTEGER,
	[force_dmg] NUMERIC NOT NULL,

	[torque_dmg] NUMERIC NOT NULL,
	[speed_dmg] NUMERIC NOT NULL,
	[rot_speed_dmg] NUMERIC NOT NULL,
	[helm_dmg] NUMERIC NOT NULL,
	[name] TEXT,

	[serial_num] INTEGER NOT NULL DEFAULT 0,
	[fleet_position] TEXT,
	[system_position] TEXT,
	[params] INTEGER NOT NULL DEFAULT 0,
	[rider_index] INTEGER NOT NULL DEFAULT -1,

	[psionic_power] INTEGER NOT NULL DEFAULT 0,
	[ai_fleet_id] INTEGER REFERENCES [ai_fleet_info](id) ON DELETE SET NULL,

	[comission_date] INTEGER NOT NULL DEFAULT 0,
	[obtained_slaves] NUMERIC NOT NULL DEFAULT 0,
	[loa_cubes] NUMERIC NOT NULL DEFAULT 0
);

CREATE TABLE [star_systems] 
(
	[id] INTEGER NOT NULL PRIMARY KEY REFERENCES [stellar_info](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[province_id] INTEGER REFERENCES [provinces](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[name] TEXT NOT NULL,
	[stellar_class] TEXT NOT NULL,

	[visible] BOOLEAN NOT NULL DEFAULT 'True',
	[terrain_id] INTEGER REFERENCES [terrain](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[control_zones] TEXT, 
	[is_open] BOOLEAN NOT NULL DEFAULT 'True'
);

CREATE TABLE [stellar_info]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[origin] TEXT NOT NULL UNIQUE
);

CREATE TABLE [station_queued_modules]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[design_section_id] INTEGER NOT NULL REFERENCES [design_sections](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[module_id] INTEGER NOT NULL REFERENCES [modules](id) ON DELETE RESTRICT ON UPDATE CASCADE,
	[weapon_id] INTEGER,
	[mount_node] TEXT NOT NULL,

	[station_module_type] INTEGER
);

CREATE TABLE [stations]
(
	[orbital_object_id] INTEGER NOT NULL PRIMARY KEY REFERENCES [orbital_objects](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE RESTRICT ON UPDATE CASCADE,
	[design_id] INTEGER REFERENCES [designs](id) ON DELETE RESTRICT ON UPDATE CASCADE,
	[warehoused_goods] INTEGER NOT NULL DEFAULT 0,
	[ship_id] INTEGER REFERENCES [ships](id) ON DELETE CASCADE
);

CREATE TABLE [stellar_props]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	[model_asset_id] INTEGER NOT NULL REFERENCES [assets](id) ON DELETE CASCADE,
	[transform] TEXT NOT NULL
);

CREATE TABLE [strat_modifiers]
(
	[key] TEXT NOT NULL,
	[value] TEXT,
	[player_id] INTEGER NOT NULL REFERENCES [players](id),
	PRIMARY KEY ([player_id],
	[key])
);

CREATE TABLE [suulkas]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[player_id] INTEGER REFERENCES [players](id) ON UPDATE CASCADE,
	[ship_id] INTEGER NOT NULL REFERENCES [ships](id) ON DELETE RESTRICT ON UPDATE CASCADE,
	[admiral_id] INTEGER REFERENCES [admirals](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[station_id] INTEGER REFERENCES [stations](orbital_object_id) ON DELETE SET NULL ON UPDATE CASCADE,

	[arrival_turns] INTEGER DEFAULT -1
);

CREATE TABLE [techs] 
(
	[id] INTEGER PRIMARY KEY UNIQUE,
	[id_from_file] TEXT NOT NULL UNIQUE
);

CREATE TABLE [terrain]
(
	[id] INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE,
	[name] TEXT,
	[origin] TEXT
);

CREATE TABLE [travel_nodes]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[system1_id] INTEGER REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[system2_id] INTEGER REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[health] INTEGER NOT NULL DEFAULT -1
);

CREATE TABLE [loa_line_records]
(
	[node_line_id] INTEGER NOT NULL REFERENCES [travel_nodes](id) ON DELETE RESTRICT ON UPDATE CASCADE,
	[fleet_id] INTEGER NOT NULL REFERENCES [fleets](id) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE [turn_events] 
(
	[id] INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE,
	[event_type] INTEGER NOT NULL,
	[event_message] INTEGER NOT NULL,
	[event_desc] TEXT NOT NULL,
	[player_id] INTEGER REFERENCES [players](id) ON DELETE SET NULL ON UPDATE CASCADE,

	[system_id] INTEGER REFERENCES [star_systems](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[orbital_id] INTEGER REFERENCES [orbital_objects](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[colony_id] INTEGER REFERENCES [colonies](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[fleet_id] INTEGER REFERENCES [fleets](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[tech_id] INTEGER REFERENCES [techs](id) ON DELETE SET NULL ON UPDATE CASCADE,

	[mission_id] INTEGER REFERENCES [missions](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[design_id] INTEGER REFERENCES [designs](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[feasibility_percent] INTEGER NOT NULL DEFAULT 0,
	[turn_number] INTEGER NOT NULL,
	[shows_dialog] BOOLEAN NOT NULL DEFAULT 'False',

	[admiral_id] INTEGER REFERENCES [admirals](id) ON DELETE SET NULL ON UPDATE CASCADE, 
	[treaty_id] INTEGER,
	[special_project_id] INTEGER REFERENCES [special_projects](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[system2_id] INTEGER REFERENCES [star_systems](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[plague_type] INTEGER,
	[plague_imp_pop_damage] NUMERIC,
	[plague_civ_pop_damage] NUMERIC,
	[plague_infra_damage] NUMERIC,
	[combat_id] INTEGER DEFAULT 1,
	[target_player_id] INTEGER REFERENCES [players](id) ON DELETE SET NULL ON UPDATE CASCADE,
	
	[ship_id] INTEGER REFERENCES [ships](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[province_id] INTEGER REFERENCES [provinces](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[design_attribute] INTEGER,
	[arrival_turns] INTEGER,
	[names_list] TEXT NOT NULL DEFAULT '',
	[num_ships] INTEGER,
	[savings] NUMERIC,
	[event_sound_cue_name] TEXT NOT NULL DEFAULT '',
	[param1] TEXT,
	[dialog_shown] BOOLEAN NOT NULL DEFAULT 'False',
	[event_name] TEXT NOT NULL
);

CREATE TABLE [morrigi_relic]
(
	[encounter_id] INTEGER NOT NULL REFERENCES [encounters](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[system_id] INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[fleet_id] INTEGER NOT NULL REFERENCES [fleets](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[is_aggressive] BOOLEAN NOT NULL DEFAULT 'True'
);

CREATE TABLE [asteroid_monitors]
(
	[encounter_id] INTEGER NOT NULL REFERENCES [encounters](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[system_id] INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[orbital_object_id] INTEGER NOT NULL REFERENCES [orbital_objects](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[is_aggressive] BOOLEAN NOT NULL DEFAULT 'True'
);

CREATE TABLE [system_killers] 
(
	[encounter_id] INTEGER NOT NULL REFERENCES [encounters](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[fleet_id] INTEGER REFERENCES [fleets](id) ON DELETE SET NULL ON UPDATE CASCADE,	
	[target] TEXT NOT NULL
);

CREATE TABLE [locust_swarms]
(
    [encounter_id] INTEGER NOT NULL REFERENCES [encounters](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[fleet_id] INTEGER REFERENCES [fleets](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[resources] INTEGER NOT NULL,
	[num_drones] INTEGER NOT NULL
);

CREATE TABLE [locust_swarm_targets]
(
	[system_id] INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE                        
);

CREATE TABLE [locust_swarm_scouts]
(
    [id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[locust_nest_id] INTEGER NOT NULL REFERENCES [encounters](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[ship_id] INTEGER NOT NULL REFERENCES [ships](id) ON DELETE CASCADE ON UPDATE CASCADE,
    [target_system_id] INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[num_drones] INTEGER NOT NULL
);

CREATE TABLE [locust_swarm_scout_targets]
(
    [system_id] INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
    [is_hostile] BOOLEAN NOT NULL DEFAULT 'False'                   
);

CREATE TABLE [neutron_stars]
(
    [encounter_id] INTEGER NOT NULL REFERENCES [encounters](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[fleet_id] INTEGER REFERENCES [fleets](id) ON DELETE SET NULL ON UPDATE CASCADE,
    [target_system_id] INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
    [deep_space_system_id] INTEGER DEFAULT NULL REFERENCES [star_systems](id) ON DELETE SET NULL ON UPDATE CASCADE,
    [deep_space_orbital_id] INTEGER DEFAULT NULL REFERENCES [orbital_objects](id) ON DELETE SET NULL ON UPDATE CASCADE
);

CREATE TABLE [super_novas]
(
    [encounter_id] INTEGER NOT NULL REFERENCES [encounters](id) ON DELETE CASCADE ON UPDATE CASCADE,
    [target_system_id] INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[turns_remaining] INTEGER NOT NULL
);

CREATE TABLE [pirate_bases]
(
	[encounter_id] INTEGER NOT NULL REFERENCES [encounters](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[system_id] INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[base_station_id] INTEGER NOT NULL REFERENCES [stations](orbital_object_id) ON DELETE CASCADE ON UPDATE CASCADE,
	[turns_until_add_ship] INTEGER NOT NULL DEFAULT 0,
	[num_ships] INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE [gardeners] 
(
	[encounter_id] INTEGER NOT NULL REFERENCES [encounters](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[system_id] INTEGER NOT NULL DEFAULT 0,
    	[fleet_id] INTEGER REFERENCES [fleets](id) ON DELETE CASCADE ON UPDATE CASCADE,
    	[gardener_fleet_id] INTEGER NOT NULL DEFAULT 0,
    	[turns_to_wait] INTEGER NOT NULL DEFAULT 0,
    	[is_gardener] BOOLEAN NOT NULL DEFAULT 'False',
    	[deep_space_system_id] INTEGER DEFAULT NULL REFERENCES [star_systems](id) ON DELETE SET NULL ON UPDATE CASCADE,
    	[deep_space_orbital_id] INTEGER DEFAULT NULL REFERENCES [orbital_objects](id) ON DELETE SET NULL ON UPDATE CASCADE
);

CREATE TABLE [swarmers] 
(
	[encounter_id] INTEGER NOT NULL REFERENCES [encounters](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[system_id] INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[orbit_id] INTEGER NOT NULL REFERENCES [orbital_objects](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[growth_stage] INTEGER,
	[hive_fleet] INTEGER REFERENCES [fleets](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[queen_fleet] INTEGER REFERENCES [fleets](id) ON DELETE SET NULL ON UPDATE CASCADE,
	[spawn_hive_delay] INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE [von_neumann]
(
	[encounter_id] INTEGER NOT NULL PRIMARY KEY REFERENCES [encounters](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[homeworld_system_id] INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[homeworld_orbit_id] INTEGER NOT NULL REFERENCES [orbital_objects](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[resources] INTEGER NOT NULL,
	[construction_progress] NUMERIC NOT NULL,

	[resources_collected_last_turn] INTEGER NOT NULL DEFAULT 0,
	[project_design_id] INTEGER REFERENCES [designs](id) ON DELETE RESTRICT ON UPDATE CASCADE,
	[last_collection_system] INTEGER NOT NULL DEFAULT 0,
	[last_target_system] INTEGER NOT NULL DEFAULT 0,
	[last_collection_turn] INTEGER NOT NULL DEFAULT 0,

	[last_target_turn] INTEGER NOT NULL DEFAULT 0,
	[fleet_id] INTEGER REFERENCES [fleets](id) ON DELETE SET NULL ON UPDATE CASCADE
);

CREATE TABLE [von_neumann_targets] 
(
	[encounter_id] INTEGER REFERENCES [encounters](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[system_id] INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[threat_level] INTEGER NOT NULL,
	CONSTRAINT "encounter_system_id" UNIQUE ([encounter_id], [system_id]) ON CONFLICT ABORT
);

CREATE TABLE [protean_ship_orbit_map]
(
	[encounter_id] INTEGER REFERENCES [encounters](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[ship_id] INTEGER REFERENCES [ships](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[orbital_object_id] INTEGER REFERENCES [orbital_objects](id) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE [waypoints] 
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[mission_id] INTEGER NOT NULL REFERENCES [missions](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[waypoint_type] INTEGER NOT NULL,
	[destination_id] INTEGER REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE [weapon_instances] 
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[section_instance_id] INTEGER NOT NULL REFERENCES [section_instances](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[weapon_id] INTEGER NOT NULL,
	[structure] NUMERIC NOT NULL,
	[max_structure] NUMERIC,
	[node_name] TEXT,
	[module_instance_id] INTEGER DEFAULT NULL REFERENCES [module_instances](id) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE [module_instances]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[section_instance_id] INTEGER NOT NULL REFERENCES [section_instances](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[module_node_id] TEXT NOT NULL,
	[structure] NUMERIC NOT NULL,
	[repair_points] INTEGER NOT NULL
);

CREATE TABLE [weapons] 
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[asset_id] INTEGER NOT NULL REFERENCES [assets](id),
	[player_id] INTEGER REFERENCES [players](id)
);

CREATE TABLE [freighters]
(
	[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[system_id] INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[design_id] INTEGER NOT NULL REFERENCES [designs](id) ON DELETE RESTRICT ON UPDATE CASCADE,
	[ship_id] INTEGER REFERENCES [ships](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[is_player_built] BOOLEAN NOT NULL DEFAULT 'False'
);

CREATE TABLE [trade_result_nodes]
(
	[system_id]				INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[produced]				INTEGER NOT NULL,
	[production_capacity]	INTEGER NOT NULL,
	[consumption]			INTEGER NOT NULL,
	[freighters]			INTEGER NOT NULL,
	[dock_capacity]			INTEGER NOT NULL,
	[export_int]			INTEGER NOT NULL,
	[export_prov]			INTEGER NOT NULL,
	[export_loc]			INTEGER NOT NULL,
	[import_int]			INTEGER NOT NULL,
	[import_prov]			INTEGER NOT NULL,
	[import_loc]			INTEGER NOT NULL,
	[range]					NUMERIC NOT NULL,
	[turn_id]				INTEGER NOT NULL
);

CREATE TABLE [plagues]
(
	[id]							INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[colony_id]						INTEGER NOT NULL REFERENCES [colonies](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[plague_type]					INTEGER NOT NULL,
	[infection_rate]				NUMERIC NOT NULL,
	[infected_population_imperial]	NUMERIC NOT NULL,
	[infected_population_civilian]	NUMERIC NOT NULL,
	[launching_player]				INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE [sdb_info]
(
	[orbital_object_id] INTEGER NOT NULL REFERENCES [orbital_objects](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[ship_id] INTEGER NOT NULL PRIMARY KEY REFERENCES [ships](id) ON DELETE CASCADE
);

CREATE TABLE [ai_old_colony_owner]
(
	[colony_id] INTEGER NOT NULL REFERENCES [colonies](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE
);

-- this stores the locking setting for sliders in game
CREATE TABLE [ui_slider_notch_settings]
(
	[id]							INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[player_id]						INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[slider_type]					INTEGER NOT NULL,
	[slider_value]					DOUBLE DEFAULT 0,
	[colony_id]						INTEGER DEFAULT NULL REFERENCES [colonies](id) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE [incoming_randoms]
(
	[id]							INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[player_id]						INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[system_id]						INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[type]							INTEGER NOT NULL,
	[turn]							INTEGER NOT NULL
);

CREATE TABLE [incoming_gm]
(
	[id]							INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[system_id]						INTEGER NOT NULL REFERENCES [star_systems](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[type]							INTEGER NOT NULL,
	[turn]							INTEGER NOT NULL
);

CREATE TABLE [piracy_fleet_detections]
(
	[fleet_id] INTEGER REFERENCES [fleets](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[player_id] INTEGER NOT NULL REFERENCES [players](id) ON DELETE CASCADE ON UPDATE CASCADE,
	[mission_id] INTEGER REFERENCES [missions](id) ON DELETE CASCADE ON UPDATE CASCADE
);



CREATE TRIGGER t_station_cleanup BEFORE DELETE ON stations
BEGIN
DELETE FROM ships WHERE id = old.ship_id;
END;

-- The following t_XXX triggers are for multiplayer compatibility
CREATE TRIGGER t_admirals AFTER INSERT ON admirals
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('admirals', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'admirals' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'admirals' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'admirals' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE admirals SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_build_orders AFTER INSERT ON build_orders
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('build_orders', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'build_orders' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'build_orders' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'build_orders' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE build_orders SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_design_modules AFTER INSERT ON design_modules
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('design_modules', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'design_modules' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'design_modules' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'design_modules' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE design_modules SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_design_section_weapon_banks AFTER INSERT ON design_section_weapon_banks
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('design_section_weapon_banks', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'design_section_weapon_banks' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'design_section_weapon_banks' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'design_section_weapon_banks' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE design_section_weapon_banks SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_design_sections AFTER INSERT ON design_sections
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('design_sections', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'design_sections' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'design_sections' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'design_sections' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE design_sections SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_designs AFTER INSERT ON designs
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('designs', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'designs' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'designs' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'designs' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE designs SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_fleets AFTER INSERT ON fleets
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('fleets', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'fleets' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'fleets' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'fleets' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE fleets SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_government_actions AFTER INSERT ON government_actions
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('government_actions', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'government_actions' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'government_actions' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'government_actions' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE government_actions SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_invoice_build_orders AFTER INSERT ON invoice_build_orders
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('invoice_build_orders', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'invoice_build_orders' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'invoice_build_orders' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'invoice_build_orders' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE invoice_build_orders SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_invoice_instances AFTER INSERT ON invoice_instances
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('invoice_instances', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'invoice_instances' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'invoice_instances' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'invoice_instances' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE invoice_instances SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_invoices AFTER INSERT ON invoices
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('invoices', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'invoices' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'invoices' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'invoices' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE invoices SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_missions AFTER INSERT ON missions
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('missions', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'missions' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'missions' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'missions' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE missions SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_modules AFTER INSERT ON modules
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('modules', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'modules' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'modules' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'modules' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE modules SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_move_orders AFTER INSERT ON move_orders
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('move_orders', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'move_orders' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'move_orders' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'move_orders' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE move_orders SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_orbital_objects AFTER INSERT ON orbital_objects
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('orbital_objects', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'orbital_objects' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'orbital_objects' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'orbital_objects' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE orbital_objects SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_provinces AFTER INSERT ON provinces
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('provinces', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'provinces' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'provinces' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'provinces' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE provinces SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_research_projects AFTER INSERT ON research_projects
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('research_projects', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'research_projects' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'research_projects' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'research_projects' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE research_projects SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_sections AFTER INSERT ON sections
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('sections', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'sections' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'sections' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'sections' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE sections SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_ships AFTER INSERT ON ships
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('ships', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'ships' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'ships' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'ships' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE ships SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_station_queued_modules AFTER INSERT ON station_queued_modules
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('station_queued_modules', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'station_queued_modules' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'station_queued_modules' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'station_queued_modules' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE station_queued_modules SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_techs AFTER INSERT ON techs
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('techs', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'techs' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'techs' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'techs' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE techs SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_travel_nodes AFTER INSERT ON travel_nodes
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('travel_nodes', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'travel_nodes' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'travel_nodes' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'travel_nodes' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE travel_nodes SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_turn_events AFTER INSERT ON turn_events
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('turn_events', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'turn_events' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'turn_events' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'turn_events' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE turn_events SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_waypoints AFTER INSERT ON waypoints
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('waypoints', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'waypoints' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'waypoints' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'waypoints' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE waypoints SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_weapons AFTER INSERT ON weapons
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('weapons', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'weapons' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'weapons' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'weapons' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE weapons SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;

CREATE TRIGGER tsting_operations AFTER INSERT ON sting_operations
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('sting_operations', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'sting_operations' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'sting_operations' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'sting_operations' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE sting_operations SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;

CREATE TRIGGER tintel_missions AFTER INSERT ON intel_missions
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('intel_missions', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'intel_missions' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'intel_missions' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'intel_missions' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE intel_missions SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;

CREATE TRIGGER tintel_responses AFTER INSERT ON intel_responses
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('intel_responses', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'intel_responses' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'intel_responses' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'intel_responses' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE intel_responses SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;

CREATE TRIGGER tincoming_randoms AFTER INSERT ON incoming_randoms
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('incoming_randoms', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'incoming_randoms' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'incoming_randoms' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'incoming_randoms' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE incoming_randoms SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;

CREATE TRIGGER tincoming_gm AFTER INSERT ON incoming_gm
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('incoming_gm', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'incoming_gm' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'incoming_gm' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'incoming_gm' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE incoming_gm SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;

CREATE TRIGGER tui_slider_notch_settings AFTER INSERT ON ui_slider_notch_settings
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('ui_slider_notch_settings', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'ui_slider_notch_settings' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'ui_slider_notch_settings' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'ui_slider_notch_settings' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE ui_slider_notch_settings SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;

CREATE TRIGGER tretrofitstation_orders AFTER INSERT ON retrofitstation_orders
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('retrofitstation_orders', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'retrofitstation_orders' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'retrofitstation_orders' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'retrofitstation_orders' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE retrofitstation_orders SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;

CREATE TRIGGER tretrofit_orders AFTER INSERT ON retrofit_orders
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('retrofit_orders', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'retrofit_orders' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'retrofit_orders' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'retrofit_orders' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE retrofit_orders SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_weapon_instances AFTER INSERT ON weapon_instances
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('weapon_instances', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'weapon_instances' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'weapon_instances' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'weapon_instances' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE weapon_instances SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_module_instances AFTER INSERT ON module_instances
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('module_instances', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'module_instances' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'module_instances' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'module_instances' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE module_instances SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_section_instances AFTER INSERT ON section_instances
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('section_instances', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'section_instances' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'section_instances' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'section_instances' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE section_instances SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_armor_instances AFTER INSERT ON armor_instances
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('armor_instances', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'armor_instances' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'armor_instances' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'armor_instances' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE armor_instances SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;
CREATE TRIGGER t_morale_event_history AFTER INSERT ON morale_event_history
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('morale_event_history', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'morale_event_history' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'morale_event_history' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'morale_event_history' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE morale_event_history SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;

CREATE TRIGGER t_loa_fleet_compositions AFTER INSERT ON loa_fleet_compositions
BEGIN
INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('loa_fleet_compositions', (SELECT id FROM clientinfo WHERE ROWID = 1));
UPDATE gen_unique SET current = current + 1 WHERE generator = 'loa_fleet_compositions' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1);
SELECT trigger_rowid((~(((SELECT current FROM gen_unique WHERE generator = 'loa_fleet_compositions' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)&(SELECT id FROM clientinfo WHERE ROWID = 1)))&(((SELECT current FROM gen_unique WHERE generator = 'loa_fleet_compositions' AND player_id = (SELECT id FROM clientinfo WHERE ROWID = 1))<< 4)|(SELECT id FROM clientinfo WHERE ROWID = 1)));
UPDATE loa_fleet_compositions SET id = trigger_rowid() WHERE ROWID = new.ROWID;
END;



-- Table indexing from lordfranko on the forums, for speeding up data retrieval. This will also increase database size and insertion times a bit
-- but as long as the size is still acceptable and the insertion times infrequent, this is OK.
--
CREATE INDEX [idx_ai_fleet_info] ON [ai_fleet_info] ([player_id] COLLATE NOCASE ASC, [system_id] COLLATE NOCASE ASC, [admiral_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_ai] ON [ai] ([stance] COLLATE NOCASE ASC);
CREATE INDEX [idx_admirals] ON [admirals] ([player_id] COLLATE NOCASE ASC, [orbital_object_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_ai_intel_battle_reports] ON [ai_intel_battle_reports] ([player_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_ai_intel_colonies] ON [ai_intel_colonies] ([player_id] COLLATE NOCASE ASC, [colony_id] COLLATE NOCASE ASC, [owning_player] COLLATE NOCASE ASC);
CREATE INDEX [idx_ai_intel_designs] ON [ai_intel_designs] ([player_id] COLLATE NOCASE ASC, [intel_on_player_id] COLLATE NOCASE ASC, [design_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_ai_intel_factions] ON [ai_intel_factions] ([player_id] COLLATE NOCASE ASC, [intel_on_player_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_ai_intel_fleets] ON [ai_intel_fleets] ([player_id] COLLATE NOCASE ASC, [intel_on_player_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_ai_intel_stations] ON [ai_intel_stations] ([player_id] COLLATE NOCASE ASC, [station_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_ai_old_colony_owner] ON [ai_old_colony_owner] ([colony_id] COLLATE NOCASE ASC, [player_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_ai_tech_styles] ON [ai_tech_styles] ([player_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_ai_tech_weights] ON [ai_tech_weights] ([ai_id] COLLATE NOCASE ASC, [tech_family] COLLATE NOCASE ASC);
CREATE INDEX [idx_armistice_treaties] ON [armistice_treaties] ([treaty_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_armor_instances] ON [armor_instances] ([section_instance_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_asteroid_monitors] ON [asteroid_monitors] ([encounter_id] COLLATE NOCASE ASC, [system_id] COLLATE NOCASE ASC, [orbital_object_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_build_orders] ON [build_orders] ([design_id] COLLATE NOCASE ASC, [star_system_id] COLLATE NOCASE ASC, [mission_id] COLLATE NOCASE ASC, [invoice_instance_id] COLLATE NOCASE ASC, [ai_fleet_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_clientinfo] ON [clientinfo] ([id] COLLATE NOCASE ASC);
CREATE INDEX [idx_colonies] ON [colonies] ([orbital_object_id] COLLATE NOCASE ASC, [player_id] COLLATE NOCASE ASC, [repair_points] COLLATE NOCASE ASC, [owning_system] COLLATE NOCASE ASC, [imp_pop] COLLATE NOCASE ASC, [colony_stage] COLLATE NOCASE ASC, [damaged_last_turn] COLLATE NOCASE ASC, [economy_rating] COLLATE NOCASE ASC, [shipcon_rate] COLLATE NOCASE ASC);
CREATE INDEX [idx_colony_morale_history] ON [colony_morale_history] ([colony_id] COLLATE NOCASE ASC, [turn_id] COLLATE NOCASE ASC, [faction_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_colony_traps] ON [colony_traps] ([system_id] COLLATE NOCASE ASC, [planet_id] COLLATE NOCASE ASC, [fleet_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_combat_data] ON [combat_data] ([conflict_id] COLLATE NOCASE ASC, [data] COLLATE NOCASE ASC);
CREATE INDEX [idx_demands] ON [demands] ([initiating_player_id] COLLATE NOCASE ASC, [receiving_player_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_design_attributes] ON [design_attributes] ([design_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_design_modules] ON [design_modules] ([design_section_id] COLLATE NOCASE ASC, [module_id] COLLATE NOCASE ASC, [weapon_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_design_section_techs] ON [design_section_techs] ([id] COLLATE NOCASE ASC, [tech_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_design_section_weapon_banks] ON [design_section_weapon_banks] ([design_section_id] COLLATE NOCASE ASC, [weapon_id] COLLATE NOCASE ASC, [design_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_design_sections] ON [design_sections] ([design_id] COLLATE NOCASE ASC, [asset_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_designs_encountered] ON [designs_encountered] ([player_id], [design_id]);
CREATE INDEX [idx_designs] ON [designs] ([player_id] COLLATE NOCASE ASC, [is_prototyped] COLLATE NOCASE ASC, [is_attributes_discovered] COLLATE NOCASE ASC, [retrofit_design_id] COLLATE NOCASE ASC, [name] COLLATE NOCASE ASC, [visible_to_player] COLLATE NOCASE ASC, [class] COLLATE NOCASE ASC);
CREATE INDEX [idx_diplomacy_reaction_history] ON [diplomacy_reaction_history] ([player_id] COLLATE NOCASE ASC, [towards_player_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_diplomacy_states] ON [diplomacy_states] ([player_id] COLLATE NOCASE ASC, [towards_player_id] COLLATE NOCASE ASC, [is_encountered] COLLATE NOCASE ASC);
CREATE INDEX [idx_diplomacy_action_history] ON [diplomacy_action_history] ([player_id] COLLATE NOCASE ASC, [towards_player_id] COLLATE NOCASE ASC, [consequence_type] COLLATE NOCASE ASC, [action_sub_type] COLLATE NOCASE ASC);
CREATE INDEX [idx_feasibility_projects] ON [feasibility_projects] ([tech_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_fleets] ON [fleets] ([player_id] COLLATE NOCASE ASC, [admiral_id] COLLATE NOCASE ASC, [system_id] COLLATE NOCASE ASC, [supporting_system_id] COLLATE NOCASE ASC, [prev_system_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_freighters] ON [freighters] ([system_id] COLLATE NOCASE ASC, [player_id] COLLATE NOCASE ASC, [design_id] COLLATE NOCASE ASC, [ship_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_gardeners] ON [gardeners] ([encounter_id] COLLATE NOCASE ASC, [system_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_governments] ON [governments] ([player_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_independent_colonies] ON [independent_colonies] ([race_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_independent_races] ON [independent_races] ([orbital_object_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_invoice_build_orders] ON [invoice_build_orders] ([design_id] COLLATE NOCASE ASC, [invoice_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_invoice_instances] ON [invoice_instances] ([player_id] COLLATE NOCASE ASC, [system_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_invoices] ON [invoices] ([player_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_large_asteroids] ON [large_asteroids] ([orbital_object_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_limitation_treaties] ON [limitation_treaties] ([treaty_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_locust_swarm_targets] ON [locust_swarm_targets] ([system_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_locust_swarms] ON [locust_swarms] ([encounter_id] COLLATE NOCASE ASC, [fleet_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_missions] ON [missions] ([fleet_id] COLLATE NOCASE ASC, [target_system_id] COLLATE NOCASE ASC, [target_orbital_object_id] COLLATE NOCASE ASC, [target_fleet_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_module_instances] ON [module_instances] ([section_instance_id] COLLATE NOCASE ASC, [module_node_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_module_psionics] ON [module_psionics] ([design_module_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_modules] ON [modules] ([player_id] COLLATE NOCASE ASC, [asset_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_morrigi_relic] ON [morrigi_relic] ([encounter_id] COLLATE NOCASE ASC, [system_id] COLLATE NOCASE ASC, [fleet_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_move_orders] ON [move_orders] ([fleet_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_name_value_pairs] ON [name_value_pairs] ([name] COLLATE NOCASE ASC, [value] COLLATE NOCASE ASC);
CREATE INDEX [idx_orbital_objects] ON [orbital_objects] ([parent_id] COLLATE NOCASE ASC, [star_system_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_plagues] ON [plagues] ([colony_id] COLLATE NOCASE ASC, [plague_type] COLLATE NOCASE ASC);
CREATE INDEX [idx_planets] ON [planets] ([ring_id] COLLATE NOCASE ASC, [resources] COLLATE NOCASE ASC, [type] COLLATE NOCASE ASC, [infrastructure] COLLATE NOCASE ASC);
CREATE INDEX [idx_player_client_info] ON [player_client_info] ([player_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_player_colony_history] ON [player_colony_history] ([colony_id] COLLATE NOCASE ASC, [turn_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_player_diplomacy_points] ON [player_diplomacy_points] ([player_id] COLLATE NOCASE ASC, [faction_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_player_empire_history] ON [player_empire_history] ([player_id] COLLATE NOCASE ASC, [turn_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_players] ON [players] ([alliance_id] COLLATE NOCASE ASC, [faction_id] COLLATE NOCASE ASC, [is_ai] COLLATE NOCASE ASC, [homeworld_id] COLLATE NOCASE ASC, [primary_color_id] COLLATE NOCASE ASC, [badge_asset_id] COLLATE NOCASE ASC, [avatar_asset_id] COLLATE NOCASE ASC, [is_ai_rebellion_player] COLLATE NOCASE ASC, [is_standard_player] COLLATE NOCASE ASC, [is_defeated] COLLATE NOCASE ASC, [secondary_color] COLLATE NOCASE ASC);
CREATE INDEX [idx_protean_ship_orbit_map] ON [protean_ship_orbit_map] ([encounter_id] COLLATE NOCASE ASC, [ship_id] COLLATE NOCASE ASC, [orbital_object_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_provinces] ON [provinces] ([player_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_requests] ON [requests] ([initiating_player_id] COLLATE NOCASE ASC, [receiving_player_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_research_projects] ON [research_projects] ([player_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_retrofitstation_orders] ON [retrofitstation_orders] ([design_id] COLLATE NOCASE ASC, [ship_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_retrofit_orders] ON [retrofit_orders] ([design_id] COLLATE NOCASE ASC, [star_system_id] COLLATE NOCASE ASC, [ship_id] COLLATE NOCASE ASC, [invoice_instance_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_satellites] ON [satellites] ([player_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_section_instances] ON [section_instances] ([section_id] COLLATE NOCASE ASC, [ship_id] COLLATE NOCASE ASC, [station_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_sections] ON [sections] ([asset_id] COLLATE NOCASE ASC, [player_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_ships] ON [ships] ([fleet_id] COLLATE NOCASE ASC, [design_id] COLLATE NOCASE ASC, [parent_id] COLLATE NOCASE ASC, [ai_fleet_id] COLLATE NOCASE ASC, [name] COLLATE NOCASE ASC, [serial_num] COLLATE NOCASE ASC, [system_position] COLLATE NOCASE ASC, [fleet_position] COLLATE NOCASE ASC, [params] COLLATE NOCASE ASC, [rider_index] COLLATE NOCASE ASC, [psionic_power] COLLATE NOCASE ASC);
CREATE INDEX [idx_special_projects] ON [special_projects] ([player_id] COLLATE NOCASE ASC, [tech_id] COLLATE NOCASE ASC, [encounter_id] COLLATE NOCASE ASC, [fleet_id] COLLATE NOCASE ASC, [target_player_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_star_systems] ON [star_systems] ([province_id] COLLATE NOCASE ASC, [terrain_id] COLLATE NOCASE ASC, [is_open] COLLATE NOCASE ASC);
CREATE INDEX [idx_station_queued_modules] ON [station_queued_modules] ([design_section_id] COLLATE NOCASE ASC, [module_id] COLLATE NOCASE ASC, [weapon_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_stations] ON [stations] ([player_id] COLLATE NOCASE ASC, [design_id] COLLATE NOCASE ASC, [ship_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_stellar_props] ON [stellar_props] ([model_asset_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_suulkas] ON [suulkas] ([player_id] COLLATE NOCASE ASC, [ship_id] COLLATE NOCASE ASC, [admiral_id] COLLATE NOCASE ASC, [station_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_swarmers] ON [swarmers] ([encounter_id] COLLATE NOCASE ASC, [system_id] COLLATE NOCASE ASC, [orbit_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_system_killers] ON [system_killers] ([encounter_id] COLLATE NOCASE ASC, [fleet_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_trade_result_nodes] ON [trade_result_nodes] ([system_id] COLLATE NOCASE ASC, [turn_id] COLLATE NOCASE ASC, [freighters] COLLATE NOCASE ASC, [range] COLLATE NOCASE ASC);
CREATE INDEX [idx_travel_nodes] ON [travel_nodes] ([system1_id] COLLATE NOCASE ASC, [system2_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_treaties] ON [treaties] ([initiating_player_id] COLLATE NOCASE ASC, [receiving_player_id] COLLATE NOCASE ASC, [active] COLLATE NOCASE ASC);
CREATE INDEX [idx_treaty_consequences] ON [treaty_consequences] ([treaty_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_treaty_incentives] ON [treaty_incentives] ([treaty_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_turn_events] ON [turn_events] ([player_id] COLLATE NOCASE ASC, [system_id] COLLATE NOCASE ASC, [orbital_id] COLLATE NOCASE ASC, [colony_id] COLLATE NOCASE ASC, [fleet_id] COLLATE NOCASE ASC, [tech_id] COLLATE NOCASE ASC, [mission_id] COLLATE NOCASE ASC, [design_id] COLLATE NOCASE ASC, [admiral_id] COLLATE NOCASE ASC, [treaty_id] COLLATE NOCASE ASC, [special_project_id] COLLATE NOCASE ASC, [system2_id] COLLATE NOCASE ASC, [combat_id] COLLATE NOCASE ASC, [target_player_id] COLLATE NOCASE ASC, [ship_id] COLLATE NOCASE ASC, [province_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_ui_slider_notch_settings] ON [ui_slider_notch_settings] ([player_id] COLLATE NOCASE ASC, [colony_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_von_neumann] ON [von_neumann] ([homeworld_system_id] COLLATE NOCASE ASC, [homeworld_orbit_id] COLLATE NOCASE ASC, [fleet_id] COLLATE NOCASE ASC, [project_design_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_waypoints] ON [waypoints] ([mission_id] COLLATE NOCASE ASC, [destination_id] COLLATE NOCASE ASC, [waypoint_type] COLLATE NOCASE ASC);
CREATE INDEX [idx_weapon_instances] ON [weapon_instances] ([section_instance_id] COLLATE NOCASE ASC, [weapon_id] COLLATE NOCASE ASC);
CREATE INDEX [idx_weapons] ON [weapons] ([player_id] COLLATE NOCASE ASC, [asset_id] COLLATE NOCASE ASC);
