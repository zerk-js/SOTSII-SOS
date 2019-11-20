// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.DataObjectCache
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.SQLite;
using System.Collections.Generic;

namespace Kerberos.Sots.Data
{
	internal class DataObjectCache
	{
		public DiplomacyStatesCache diplomacy_states { get; private set; }

		public PlayersCache players { get; private set; }

		public Dictionary<int, List<int>> fleetShips { get; private set; }

		public TableCache<int, FleetInfo> fleets { get; private set; }

		public DesignsCache designs { get; private set; }

		public ShipsCache ships { get; private set; }

		public ColoniesCache colonies { get; private set; }

		public StationsCache stations { get; private set; }

		public SectionInstancesCache section_instances { get; private set; }

		public ArmorInstancesCache armor_instances { get; private set; }

		public WeaponInstancesCache weapon_instances { get; private set; }

		public ModuleInstancesCache module_instances { get; private set; }

		public MissionsCache missions { get; private set; }

		public ColonyTrapsCache colony_traps { get; private set; }

		public ExploreRecordsCache explore_records { get; private set; }

		public NodeLinesCache node_lines { get; private set; }

		public MoraleEventHistoryCache morale_event_history { get; private set; }

		public TableCache<int, StarSystemInfo> star_systems { get; private set; }

		public TableCache<PlayerTechInfo.PrimaryKey, PlayerTechInfo> player_techs { get; private set; }

		public TableCache<PlayerBranchInfo.PrimaryKey, PlayerBranchInfo> player_tech_branches { get; private set; }

		public Dictionary<int, Dictionary<string, int>> CachedPlayerShipNames { get; private set; }

		public Dictionary<int, HashSet<string>> CachedPlayerDesignNames { get; private set; }

		public Dictionary<DataObjectCache.SystemPlayerID, float> CachedSystemStratSensorRanges { get; private set; }

		public Dictionary<DataObjectCache.SystemPlayerID, bool> CachedSystemHasGateFlags { get; private set; }

		public DataObjectCache(SQLiteConnection db, AssetDatabase assets)
		{
			this.diplomacy_states = new DiplomacyStatesCache(db, assets);
			this.players = new PlayersCache(db, assets, this.diplomacy_states);
			this.diplomacy_states.PostInit(this.players);
			this.fleetShips = new Dictionary<int, List<int>>();
			this.fleets = new TableCache<int, FleetInfo>();
			this.designs = new DesignsCache(db, assets);
			this.ships = new ShipsCache(db, assets, this.designs);
			this.colonies = new ColoniesCache(db, assets);
			this.stations = new StationsCache(db, assets, this.designs);
			this.section_instances = new SectionInstancesCache(db, assets, this.designs, this.ships, this.stations);
			this.armor_instances = new ArmorInstancesCache(db, assets, this.section_instances);
			this.weapon_instances = new WeaponInstancesCache(db, assets, this.section_instances);
			this.module_instances = new ModuleInstancesCache(db, assets, this.section_instances);
			this.section_instances.PostInit(this.armor_instances, this.weapon_instances, this.module_instances);
			this.missions = new MissionsCache(db, assets);
			this.morale_event_history = new MoraleEventHistoryCache(db, assets);
			this.colony_traps = new ColonyTrapsCache(db, assets);
			this.explore_records = new ExploreRecordsCache(db, assets);
			this.node_lines = new NodeLinesCache(db, assets);
			this.star_systems = new TableCache<int, StarSystemInfo>();
			this.player_techs = new TableCache<PlayerTechInfo.PrimaryKey, PlayerTechInfo>();
			this.player_tech_branches = new TableCache<PlayerBranchInfo.PrimaryKey, PlayerBranchInfo>();
			this.CachedPlayerShipNames = new Dictionary<int, Dictionary<string, int>>();
			this.CachedPlayerDesignNames = new Dictionary<int, HashSet<string>>();
			this.CachedSystemStratSensorRanges = new Dictionary<DataObjectCache.SystemPlayerID, float>();
			this.CachedSystemHasGateFlags = new Dictionary<DataObjectCache.SystemPlayerID, bool>();
		}

		public void Clear()
		{
			this.diplomacy_states.Clear();
			this.players.Clear();
			this.fleetShips.Clear();
			this.fleets.Clear();
			this.designs.Clear();
			this.ships.Clear();
			this.colonies.Clear();
			this.stations.Clear();
			this.section_instances.Clear();
			this.armor_instances.Clear();
			this.weapon_instances.Clear();
			this.module_instances.Clear();
			this.missions.Clear();
			this.morale_event_history.Clear();
			this.colony_traps.Clear();
			this.explore_records.Clear();
			this.node_lines.Clear();
			this.star_systems.Clear();
			this.player_techs.Clear();
			this.player_tech_branches.Clear();
			this.CachedPlayerShipNames.Clear();
			this.CachedPlayerDesignNames.Clear();
			this.CachedSystemStratSensorRanges.Clear();
			this.CachedSystemHasGateFlags.Clear();
		}

		public struct SystemPlayerID
		{
			public int SystemID;
			public int PlayerID;
		}
	}
}
