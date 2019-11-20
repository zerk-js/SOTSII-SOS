// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarMapElements.StarMap
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.StarMapElements
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMAP)]
	internal class StarMap : StarMapBase
	{
		public static bool AlwaysInRange;
		private readonly GameSession _sim;
		private readonly GameDatabase _db;
		private StarMapViewFilter _viewFilter;

		public StarMap(App game, GameSession sim, Sky sky)
		  : base(game, sky)
		{
			this._sim = sim;
			this._db = sim.GameDatabase;
		}

		public StarMapViewFilter ViewFilter
		{
			get
			{
				return this._viewFilter;
			}
			set
			{
				this._viewFilter = value;
				this.PostSetProp(nameof(ViewFilter), (object)value);
			}
		}

		protected override void GetAdditionalParams(List<object> parms)
		{
		}

		protected override void UpdateProvince(
		  StarMapProvince o,
		  ProvinceInfo oi,
		  StarMapBase.SyncContext context)
		{
			ProvinceInfo provinceInfo = this._db.GetProvinceInfo(oi.ID);
			oi.Name = provinceInfo.Name;
			oi.PlayerID = provinceInfo.PlayerID;
			oi.CapitalSystemID = provinceInfo.CapitalSystemID;
			o.SetPosition(this._db.GetStarSystemOrigin(oi.CapitalSystemID));
			if (this._db.IsStarSystemVisibleToPlayer(this._sim.LocalPlayer.ID, oi.CapitalSystemID))
				o.SetCapital(this.Systems.Reverse[oi.CapitalSystemID]);
			o.SetPlayer(this._sim.GetPlayerObject(oi.PlayerID));
		}

		protected override StarMapProvince CreateProvince(
		  GameObjectSet gos,
		  ProvinceInfo oi,
		  StarMapBase.SyncContext context)
		{
			StarMapProvince starMapProvince = new StarMapProvince(this.App);
			starMapProvince.SetLabel(oi.Name);
			gos.Add((IGameObject)starMapProvince);
			return starMapProvince;
		}

		protected override StarMapFilter CreateFilter(
		  GameObjectSet gos,
		  StarMapViewFilter type,
		  StarMapBase.SyncContext context)
		{
			StarMapFilter starMapFilter = new StarMapFilter(this.App);
			starMapFilter.SetFilterType(type);
			gos.Add((IGameObject)starMapFilter);
			return starMapFilter;
		}

		protected override StarMapNodeLine CreateNodeLine(
		  GameObjectSet gos,
		  NodeLineInfo oi,
		  StarMapBase.SyncContext context)
		{
			StarMapNodeLine starMapNodeLine = new StarMapNodeLine(this.App, this._db.GetStarSystemOrigin(oi.System1ID), this._db.GetStarSystemOrigin(oi.System2ID));
			gos.Add((IGameObject)starMapNodeLine);
			return starMapNodeLine;
		}

		protected override StarMapProp CreateProp(
		  GameObjectSet gos,
		  StellarPropInfo oi,
		  StarMapBase.SyncContext context)
		{
			StarMapProp starMapProp = new StarMapProp(this.App, oi.AssetPath, oi.Transform.Position, oi.Transform.EulerAngles, 1f);
			gos.Add((IGameObject)starMapProp);
			return starMapProp;
		}

		protected override StarMapTerrain CreateTerrain(
		  GameObjectSet gos,
		  TerrainInfo oi,
		  StarMapBase.SyncContext context)
		{
			StarMapTerrain starMapTerrain = new StarMapTerrain(this.App, oi.Origin, oi.Name);
			gos.Add((IGameObject)starMapTerrain);
			return starMapTerrain;
		}

		public void UpdateSystemTrade(int systemID)
		{
			KeyValuePair<StarMapSystem, int>? nullable = new KeyValuePair<StarMapSystem, int>?(this.Systems.Forward.FirstOrDefault<KeyValuePair<StarMapSystem, int>>((Func<KeyValuePair<StarMapSystem, int>, bool>)(x => x.Value == systemID)));
			if (!nullable.HasValue)
				return;
			nullable.Value.Key.SetProductionValues(this._sim.GetExportCapacity(nullable.Value.Value), this._sim.GetMaxExportCapacity(nullable.Value.Value));
		}

		protected override void UpdateSystem(
		  StarMapSystem o,
		  StarSystemInfo systemInfo,
		  StarMapBase.SyncContext context)
		{
			int? systemProvinceId = this._db.GetStarSystemProvinceID(systemInfo.ID);
			o.SetProvince(systemProvinceId.HasValue ? this.Provinces.Reverse[systemProvinceId.Value] : (StarMapProvince)null);
			o.SetPosition(systemInfo.Origin);
			o.SetTerrain(systemInfo.TerrainID.HasValue ? this.Terrain.Reverse[systemInfo.TerrainID.Value] : (StarMapTerrain)null);
			IEnumerable<int> orbitalObjectIds = this._db.GetStarSystemOrbitalObjectIDs(systemInfo.ID);
			List<int> source = new List<int>();
			bool flag1 = false;
			List<StationInfo> list1 = this._db.GetStationForSystem(systemInfo.ID).Where<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == StationType.NAVAL)).ToList<StationInfo>();
			Dictionary<int, int> dictionary1 = new Dictionary<int, int>();
			bool flag2 = false;
			foreach (int planetID in orbitalObjectIds)
			{
				AIColonyIntel ci = this._db.GetColonyIntelForPlanet(this._sim.LocalPlayer.ID, planetID);
				if (ci != null)
				{
					if (!dictionary1.ContainsKey(ci.OwningPlayerID))
						dictionary1.Add(ci.OwningPlayerID, 0);
					Dictionary<int, int> dictionary2;
					int owningPlayerId;
					(dictionary2 = dictionary1)[owningPlayerId = ci.OwningPlayerID] = dictionary2[owningPlayerId] + 1;
					if (ci.OwningPlayerID == this._sim.LocalPlayer.ID)
						flag1 = true;
					source.Add(ci.OwningPlayerID);
					List<TreatyInfo> list2 = this._sim.GameDatabase.GetTreatyInfos().ToList<TreatyInfo>().Where<TreatyInfo>((Func<TreatyInfo, bool>)(x => x.Type == TreatyType.Trade)).ToList<TreatyInfo>();
					if (flag1 || list2.Any<TreatyInfo>((Func<TreatyInfo, bool>)(x =>
				   {
					   if (x.InitiatingPlayerId == this._sim.LocalPlayer.ID && x.ReceivingPlayerId == ci.OwningPlayerID)
						   return true;
					   if (x.ReceivingPlayerId == this._sim.LocalPlayer.ID)
						   return x.InitiatingPlayerId == ci.OwningPlayerID;
					   return false;
				   })))
						flag2 = true;
				}
			}
			if (dictionary1.Count == 0)
			{
				foreach (StationInfo stationInfo in list1)
				{
					if (stationInfo.PlayerID == this._sim.LocalPlayer.ID)
						flag1 = true;
				}
			}
			if (flag1)
			{
				float supportRange = GameSession.GetSupportRange(this._db.AssetDatabase, this._db, this._sim.LocalPlayer.ID);
				o.SetSupportRange(supportRange);
			}
			int? systemOwningPlayer = this._db.GetSystemOwningPlayer(systemInfo.ID);
			if (systemOwningPlayer.HasValue && this._sim.GameDatabase.IsStarSystemVisibleToPlayer(this._sim.LocalPlayer.ID, systemInfo.ID) && StarMap.IsInRange(this._sim.GameDatabase, this._sim.LocalPlayer.ID, systemInfo.ID))
			{
				o.SetPlayerBadge(Path.GetFileNameWithoutExtension(this._db.GetPlayerInfo(systemOwningPlayer.Value).BadgeAssetPath));
				o.SetOwningPlayer(this._sim.GetPlayerObject(systemOwningPlayer.Value));
			}
			else
			{
				o.SetPlayerBadge("");
				o.SetOwningPlayer((Player)null);
			}
			source.Sort();
			o.SetPlayers(source.Select<int, Player>((Func<int, Player>)(playerId => this._sim.GetPlayerObject(playerId))).ToArray<Player>());
			List<PlayerInfo> playerInfos = context.PlayerInfos;
			List<Player> playerList1 = new List<Player>();
			List<Player> playerList2 = new List<Player>();
			foreach (PlayerInfo playerInfo in playerInfos)
			{
				if (this._db.SystemHasGate(systemInfo.ID, playerInfo.ID) && (playerInfo.ID == this._sim.LocalPlayer.ID || this._db.IsSurveyed(this._sim.LocalPlayer.ID, systemInfo.ID) && StarMap.IsInRange(this._db, playerInfo.ID, systemInfo.ID)))
					playerList1.Add(this._sim.GetPlayerObject(playerInfo.ID));
				if (this._db.SystemHasAccelerator(systemInfo.ID, playerInfo.ID) && (playerInfo.ID == this._sim.LocalPlayer.ID || this._db.IsSurveyed(this._sim.LocalPlayer.ID, systemInfo.ID) && StarMap.IsInRange(this._db, playerInfo.ID, systemInfo.ID)))
					playerList2.Add(this._sim.GetPlayerObject(playerInfo.ID));
				IEnumerable<StationInfo> forSystemAndPlayer = this._db.GetStationForSystemAndPlayer(systemInfo.ID, playerInfo.ID);
				if (playerInfo.ID == this._sim.LocalPlayer.ID)
				{
					o.SetHasNavalStation(forSystemAndPlayer.Any<StationInfo>((Func<StationInfo, bool>)(x =>
				   {
					   if (x.PlayerID == this._sim.LocalPlayer.ID && x.DesignInfo.StationLevel > 0)
						   return x.DesignInfo.StationType == StationType.NAVAL;
					   return false;
				   })));
					o.SetHasScienceStation(forSystemAndPlayer.Any<StationInfo>((Func<StationInfo, bool>)(x =>
				   {
					   if (x.PlayerID == this._sim.LocalPlayer.ID && x.DesignInfo.StationLevel > 0)
						   return x.DesignInfo.StationType == StationType.SCIENCE;
					   return false;
				   })));
					o.SetHasTradeStation(forSystemAndPlayer.Any<StationInfo>((Func<StationInfo, bool>)(x =>
				   {
					   if (x.PlayerID == this._sim.LocalPlayer.ID && x.DesignInfo.StationLevel > 0)
						   return x.DesignInfo.StationType == StationType.CIVILIAN;
					   return false;
				   })));
					o.SetHasDiploStation(forSystemAndPlayer.Any<StationInfo>((Func<StationInfo, bool>)(x =>
				   {
					   if (x.PlayerID == this._sim.LocalPlayer.ID && x.DesignInfo.StationLevel > 0)
						   return x.DesignInfo.StationType == StationType.DIPLOMATIC;
					   return false;
				   })));
				}
				o.SetStationCapacity(this._db.GetNumberMaxStationsSupportedBySystem(this._sim, systemInfo.ID, this._sim.LocalPlayer.ID));
				if (playerInfo.ID == this._sim.LocalPlayer.ID && systemOwningPlayer.HasValue)
				{
					int cruiserEquivalent = this._db.GetSystemSupportedCruiserEquivalent(this._sim, systemInfo.ID, playerInfo.ID);
					int remainingSupportPoints = this._db.GetRemainingSupportPoints(this._sim, systemInfo.ID, playerInfo.ID);
					if (systemOwningPlayer.Value == this._sim.LocalPlayer.ID)
					{
						o.SetNavalCapacity(cruiserEquivalent);
						o.SetNavalUsage(cruiserEquivalent - remainingSupportPoints);
					}
					else
					{
						o.SetNavalCapacity(0);
						o.SetNavalUsage(0);
					}
				}
			}
			o.SetColonyTrapped(this._sim.GameDatabase.GetColonyTrapInfosAtSystem(systemInfo.ID).Where<ColonyTrapInfo>((Func<ColonyTrapInfo, bool>)(x =>
		   {
			   if (this._sim.GameDatabase.GetFleetInfo(x.FleetID) != null)
				   return this._sim.GameDatabase.GetFleetInfo(x.FleetID).PlayerID == this._sim.LocalPlayer.ID;
			   return false;
		   })).Any<ColonyTrapInfo>());
			o.SetPlayersWithGates(playerList1.ToArray());
			o.SetPlayersWithAccelerators(playerList2.ToArray());
			o.SetSensorRange(this._sim.GameDatabase.GetSystemStratSensorRange(systemInfo.ID, this._sim.LocalPlayer.ID));
			TradeResultsTable tradeResultsTable = this._sim.GameDatabase.GetTradeResultsTable();
			TradeResultsTable resultsHistoryTable = this._sim.GameDatabase.GetLastTradeResultsHistoryTable();
			if (flag2 && tradeResultsTable.TradeNodes.ContainsKey(systemInfo.ID))
			{
				if (resultsHistoryTable.TradeNodes.ContainsKey(systemInfo.ID))
					o.SetTradeValues(this._sim, tradeResultsTable.TradeNodes[systemInfo.ID], resultsHistoryTable.TradeNodes[systemInfo.ID], systemInfo.ID);
				else
					o.SetTradeValues(this._sim, tradeResultsTable.TradeNodes[systemInfo.ID], new TradeNode(), systemInfo.ID);
			}
			else
				o.SetTradeValues(this._sim, new TradeNode(), new TradeNode(), systemInfo.ID);
			int exploredByPlayer = this._db.GetLastTurnExploredByPlayer(this._sim.LocalPlayer.ID, systemInfo.ID);
			int turnCount = this._db.GetTurnCount();
			o.SetIsSurveyed(exploredByPlayer != 0);
			this.SetSystemHasBeenRecentlySurveyed(this.Systems.Reverse[systemInfo.ID], exploredByPlayer != 0 && turnCount - exploredByPlayer <= 5 && !flag1);
			this.SetSystemHasRecentCombat(this.Systems.Reverse[systemInfo.ID], this._sim.CombatData.GetFirstCombatInSystem(this._sim.GameDatabase, systemInfo.ID, this._sim.GameDatabase.GetTurnCount() - 1) != null);
			this.SetSystemIsMissionTarget(this.Systems.Reverse[systemInfo.ID], this._db.GetPlayerMissionInfosAtSystem(this._sim.LocalPlayer.ID, systemInfo.ID).Any<MissionInfo>(), this._db.GetPlayerInfo(this._sim.LocalPlayer.ID).PrimaryColor);
			bool flag3 = SuperNova.IsPlayerSystemsInSuperNovaEffectRanges(this._db, this._sim.LocalPlayer.ID, systemInfo.ID) || NeutronStar.IsPlayerSystemsInNeutronStarEffectRanges(this._sim, this._sim.LocalPlayer.ID, systemInfo.ID);
			this.SetSystemRequriesSuperNovaWarning(this.Systems.Reverse[systemInfo.ID], flag3);
			this.UpdateSystemTrade(systemInfo.ID);
			o.SetHasLoaGate(this._db.GetFleetInfoBySystemID(systemInfo.ID, FleetType.FL_ACCELERATOR).Any<FleetInfo>());
		}

		public void ClearSystemEffects()
		{
			foreach (StarSystemInfo starSystemInfo in this.App.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>())
			{
				if (this.Systems.Reverse.Keys.Contains<int>(starSystemInfo.ID))
				{
					this.SetSystemIsMissionTarget(this.Systems.Reverse[starSystemInfo.ID], false, this._db.GetPlayerInfo(this._sim.LocalPlayer.ID).PrimaryColor);
					this.SetSystemHasRecentCombat(this.Systems.Reverse[starSystemInfo.ID], false);
					this.SetSystemHasBeenRecentlySurveyed(this.Systems.Reverse[starSystemInfo.ID], false);
					this.SetSystemRequriesSuperNovaWarning(this.Systems.Reverse[starSystemInfo.ID], false);
				}
			}
		}

		public void SetMissionEffectTarget(StarSystemInfo sys, bool value)
		{
			if (!this.Systems.Reverse.Keys.Contains<int>(sys.ID))
				return;
			this.SetSystemIsMissionTarget(this.Systems.Reverse[sys.ID], value, this._db.GetPlayerInfo(this._sim.LocalPlayer.ID).PrimaryColor);
		}

		protected override StarMapSystem CreateSystem(
		  GameObjectSet gos,
		  StarSystemInfo oi,
		  StarMapBase.SyncContext context)
		{
			StellarClass stellarClass = StellarClass.Parse(oi.StellarClass);
			StarMapSystem starMapSystem = new StarMapSystem(this.App, StarHelper.GetDisplayParams(stellarClass).AssetPath, oi.Origin, StarHelper.CalcRadius(stellarClass.Size) / StarSystemVars.Instance.StarRadiusIa, oi.Name);
			starMapSystem.SetSensorRange(this._sim.GameDatabase.GetSystemStratSensorRange(oi.ID, this._sim.LocalPlayer.ID));
			gos.Add((IGameObject)starMapSystem);
			return starMapSystem;
		}

		protected override void UpdateFleet(
		  StarMapFleet o,
		  FleetInfo oi,
		  StarMapBase.SyncContext context)
		{
			FleetLocation fleetLocation = this._db.GetFleetLocation(oi.ID, true);
			o.SetSystemID(fleetLocation.SystemID);
			o.SetPosition(fleetLocation.Coords);
			o.FleetID = oi.ID;
			if (fleetLocation.Direction.HasValue)
				o.SetDirection(fleetLocation.Direction.Value);
			MoveOrderInfo orderInfoByFleetId = this.App.GameDatabase.GetMoveOrderInfoByFleetID(oi.ID);
			if (orderInfoByFleetId != null && (double)orderInfoByFleetId.Progress <= 0.0)
			{
				StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(fleetLocation.SystemID);
				if (starSystemInfo != (StarSystemInfo)null)
					o.SetPosition(starSystemInfo.Origin);
			}
			o.SetIsInTransit(orderInfoByFleetId != null && ((double)orderInfoByFleetId.Progress > 0.0 || oi.Type == FleetType.FL_ACCELERATOR && (oi.SystemID == 0 || this._sim.GameDatabase.GetStarSystemInfo(oi.SystemID).IsDeepSpace)));
			o.SetPlayer(this._sim.GetPlayerObject(oi.PlayerID));
			if (oi.PlayerID == this._sim.LocalPlayer.ID)
				o.SetSensorRange(GameSession.GetFleetSensorRange(this.App.AssetDatabase, this._db, oi.ID));
			if (o.ObjectStatus == GameObjectStatus.Ready)
				o.PostSetActive(true);
			o.SetIsLoaGate(oi.Type == FleetType.FL_ACCELERATOR);
		}

		protected override StarMapFleet CreateFleet(
		  GameObjectSet gos,
		  FleetInfo oi,
		  StarMapBase.SyncContext context)
		{
			StarMapFleet o = new StarMapFleet(this.App, this._db.AssetDatabase.GetMiniShipDirectory(this._sim.App, this._db.GetFactionName(this._db.GetPlayerFactionID(oi.PlayerID)), oi.Type, this._db.GetShipInfoByFleetID(oi.ID, false).ToList<ShipInfo>()).ID);
			if (oi.PlayerID == this._sim.LocalPlayer.ID)
				o.SetSensorRange(GameSession.GetFleetSensorRange(this.App.AssetDatabase, this._db, oi.ID));
			this.UpdateFleet(o, oi, context);
			gos.Add((IGameObject)o);
			return o;
		}

		protected override void OnInitialize(GameObjectSet gos, params object[] parms)
		{
			if (this._sim.IsMultiplayer)
				this.PostObjectAddObjects((IGameObject)new StarMapServerName(this.App, new Vector3(0.0f, 0.0f, 0.0f), this.App.Network.GameName));
			StarMapBase.SyncContext context = new StarMapBase.SyncContext(this._db);
			this.CreateFilter(gos, StarMapViewFilter.VF_SUPPORT_RANGE, context);
			this.CreateFilter(gos, StarMapViewFilter.VF_SENSOR_RANGE, context);
			this.CreateFilter(gos, StarMapViewFilter.VF_TRADE, context);
			this.Props.Sync(gos, this._db.GetStellarProps(), context, false);
			this.Sync(gos);
		}

		public void Sync(GameObjectSet gos)
		{
			StarMapBase.SyncContext context = new StarMapBase.SyncContext(this._db);
			List<StarSystemInfo> list1 = this._db.GetStarSystemInfos().ToList<StarSystemInfo>();
			foreach (StarSystemInfo starSystemInfo in list1.Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => !x.IsVisible)).ToList<StarSystemInfo>())
			{
				if (!StarMap.IsInRange(this._db, this._sim.LocalPlayer.ID, starSystemInfo.ID))
				{
					list1.Remove(starSystemInfo);
				}
				else
				{
					starSystemInfo.IsVisible = true;
					this._db.UpdateStarSystemVisible(starSystemInfo.ID, true);
				}
			}
			IEnumerable<StarMapTerrain> source1 = this.Terrain.Sync(gos, this._db.GetTerrainInfos(), context, false);
			IEnumerable<StarMapProvince> source2 = this.Provinces.Sync(gos, this._db.GetProvinceInfos(), context, false);
			IEnumerable<StarMapSystem> source3 = this.Systems.Sync(gos, (IEnumerable<StarSystemInfo>)list1, context, false);
			List<FleetInfo> list2 = this._db.GetFleetInfos(FleetType.FL_NORMAL | FleetType.FL_CARAVAN | FleetType.FL_ACCELERATOR).Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
		   {
			   if (!x.IsReserveFleet)
				   return this._sim.GetPlayerObject(x.PlayerID) != null;
			   return false;
		   })).ToList<FleetInfo>();
			int swarmerPlayer = this._sim.ScriptModules == null || this._sim.ScriptModules.Swarmers == null ? 0 : this._sim.ScriptModules.Swarmers.PlayerID;
			if (swarmerPlayer != 0)
			{
				foreach (FleetInfo fleetInfo in list2.Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
			   {
				   if (x.PlayerID == swarmerPlayer)
					   return x.Name.Contains("Swarm");
				   return false;
			   })).ToList<FleetInfo>())
					list2.Remove(fleetInfo);
			}
			foreach (FleetInfo fleetInfo in list2.Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
		   {
			   if (this._db.GetMissionByFleetID(x.ID) != null)
				   return this._db.GetMissionByFleetID(x.ID).Type == MissionType.PIRACY;
			   return false;
		   })).ToList<FleetInfo>())
			{
				if (!this._db.PirateFleetVisibleToPlayer(fleetInfo.ID, this._sim.LocalPlayer.ID))
					list2.Remove(fleetInfo);
			}
			IEnumerable<StarMapFleet> source4 = this.Fleets.Sync(gos, (IEnumerable<FleetInfo>)list2, context, true);
			this.PostObjectAddObjects((IGameObject[])source2.ToArray<StarMapProvince>());
			this.PostObjectAddObjects((IGameObject[])source3.ToArray<StarMapSystem>());
			this.PostObjectAddObjects((IGameObject[])source4.ToArray<StarMapFleet>());
			if (this._sim.LocalPlayer.Faction.Name == "human")
				this.PostObjectAddObjects((IGameObject[])this.NodeLines.Sync(gos, (IEnumerable<NodeLineInfo>)this._db.GetExploredNodeLines(this._sim.LocalPlayer.ID).Where<NodeLineInfo>((Func<NodeLineInfo, bool>)(x => x.IsPermenant)).ToList<NodeLineInfo>(), context, false).ToArray<StarMapNodeLine>());
			this.PostObjectAddObjects((IGameObject[])source1.ToArray<StarMapTerrain>());
			Dictionary<int, int> dictionary1 = new Dictionary<int, int>();
			List<StarMap.PlayerSystemPair> playerSystemPairList = new List<StarMap.PlayerSystemPair>();
			foreach (StarMapFleet key in this.Fleets.Forward.Keys)
			{
				this._sim.GameDatabase.IsStealthFleet(key.FleetID);
				if (key.InTransit)
				{
					key.SetVisible(StarMap.IsInRange(this.App.Game.GameDatabase, this._sim.LocalPlayer.ID, key.Position, 1f, (Dictionary<int, List<ShipInfo>>)null));
				}
				else
				{
					dictionary1[key.SystemID] = 0;
					bool flag = false;
					foreach (StarMap.PlayerSystemPair playerSystemPair in playerSystemPairList)
					{
						if (playerSystemPair.PlayerID == key.PlayerID && playerSystemPair.SystemID == key.SystemID)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						key.SetVisible(false);
					}
					else
					{
						key.SetVisible(StarMap.IsInRange(this.App.Game.GameDatabase, this._sim.LocalPlayer.ID, key.Position, 1f, (Dictionary<int, List<ShipInfo>>)null));
						playerSystemPairList.Add(new StarMap.PlayerSystemPair()
						{
							PlayerID = key.PlayerID,
							SystemID = key.SystemID
						});
					}
				}
			}
			foreach (StarMapFleet key in this.Fleets.Forward.Keys)
			{
				if (!key.InTransit && key.IsVisible)
				{
					key.SetSystemFleetIndex(dictionary1[key.SystemID]);
					Dictionary<int, int> dictionary2;
					int systemId;
					(dictionary2 = dictionary1)[systemId = key.SystemID] = dictionary2[systemId] + 1;
				}
			}
			foreach (StarMapFleet key in this.Fleets.Forward.Keys)
			{
				if (!key.InTransit && key.IsVisible)
					key.SetSystemFleetCount(dictionary1[key.SystemID]);
			}
			foreach (HomeworldInfo homeworld in this._db.GetHomeworlds())
			{
				HomeworldInfo hw = homeworld;
				ColonyInfo hwci = this._db.GetColonyInfo(hw.ColonyID);
				if (hwci != null && list1.Any<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
			   {
				   if (x.ID == hw.SystemID)
					   return hw.PlayerID == hwci.PlayerID;
				   return false;
			   })))
					this.PostSetProp("Homeworld", (object)this._sim.GetPlayerObject(hw.PlayerID).ObjectID, (object)this.Systems.Reverse[hw.SystemID].ObjectID);
			}
			foreach (StarSystemInfo starSystemInfo in list1)
				this.PostSetProp("ProvinceCapitalEffect", (object)false, (object)this.Systems.Reverse[starSystemInfo.ID].ObjectID);
			foreach (ProvinceInfo provinceInfo in this._db.GetProvinceInfos().ToList<ProvinceInfo>())
			{
				ProvinceInfo p = provinceInfo;
				if (list1.Any<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => x.ID == p.CapitalSystemID)))
					this.PostSetProp("ProvinceCapitalEffect", (object)true, (object)this.Systems.Reverse[p.CapitalSystemID].ObjectID);
			}
			this.PostSetProp("RegenerateTerrain");
			this.PostSetProp("RegenerateBorders");
			this.PostSetProp("RegenerateFilters");
		}

		public void Select(IGameObject o)
		{
			this.PostSetProp("Selected", o.GetObjectID());
		}

		private void SetSystemIsMissionTarget(StarMapSystem system, bool value, Vector3 Color)
		{
			this.PostSetProp("MissionTarget", (object)system.ObjectID, (object)value, (object)Color.X, (object)Color.Y, (object)Color.Z);
		}

		private void SetSystemHasBeenRecentlySurveyed(StarMapSystem system, bool value)
		{
			this.PostSetProp("RecentSurvey", (object)system.ObjectID, (object)value);
		}

		private void SetSystemHasRecentCombat(StarMapSystem system, bool value)
		{
			this.PostSetProp("RecentCombatEffect", (object)system.ObjectID, (object)value);
		}

		private void SetSystemRequriesSuperNovaWarning(StarMapSystem system, bool value)
		{
			this.PostSetProp("SuperNovaWarningEffect", (object)system.ObjectID, (object)value);
		}

		public static bool IsInRange(GameDatabase db, int playerid, int systemId)
		{
			return StarMap.IsInRange(db, playerid, db.GetStarSystemOrigin(systemId), 1f, (Dictionary<int, List<ShipInfo>>)null);
		}

		public static bool IsInRange(
		  GameDatabase db,
		  int playerid,
		  StarSystemInfo ssi,
		  Dictionary<int, List<ShipInfo>> cachedFleetShips = null)
		{
			return StarMap.IsInRange(db, playerid, ssi.Origin, 1f, cachedFleetShips);
		}

		public static bool IsInRange(
		  GameDatabase db,
		  int playerid,
		  Vector3 loc,
		  float rangeMultiplier = 1f,
		  Dictionary<int, List<ShipInfo>> cachedFleetShips = null)
		{
			if (StarMap.AlwaysInRange)
				return true;
			List<int> list = db.GetPlayerInfos().Where<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (x.ID != playerid)
				   return db.GetDiplomacyStateBetweenPlayers(x.ID, playerid) == DiplomacyState.ALLIED;
			   return true;
		   })).Select<PlayerInfo, int>((Func<PlayerInfo, int>)(x => x.ID)).ToList<int>();
			List<int> intList = new List<int>();
			foreach (int playerid1 in list)
				intList.AddRange(db.GetPlayerColonySystemIDs(playerid1));
			foreach (int num in intList)
			{
				StarSystemInfo starSystemInfo = db.GetStarSystemInfo(num);
				if (!(starSystemInfo == (StarSystemInfo)null))
				{
					float stratSensorRange = db.GetSystemStratSensorRange(num, playerid);
					if ((double)(starSystemInfo.Origin - loc).Length <= (double)stratSensorRange * (double)rangeMultiplier)
						return true;
				}
			}
			List<FleetInfo> fleetInfoList = new List<FleetInfo>();
			foreach (int playerID in list)
				fleetInfoList.AddRange(db.GetFleetInfosByPlayerID(playerID, FleetType.FL_ALL_COMBAT | FleetType.FL_NORMAL | FleetType.FL_CARAVAN));
			float stratSensorRange1 = db.AssetDatabase.DefaultStratSensorRange;
			foreach (FleetInfo fleet in fleetInfoList)
			{
				List<ShipInfo> cachedShips = (List<ShipInfo>)null;
				if (cachedFleetShips != null && !cachedFleetShips.TryGetValue(fleet.ID, out cachedShips))
					cachedShips = new List<ShipInfo>();
				float num = GameSession.GetFleetSensorRange(db.AssetDatabase, db, fleet, cachedShips);
				if ((double)num == 0.0 && db.GetShipsByFleetID(fleet.ID).Any<int>())
					num = db.AssetDatabase.DefaultStratSensorRange;
				if ((double)(db.GetFleetLocation(fleet.ID, false).Coords - loc).Length <= (double)num * (double)rangeMultiplier)
					return true;
			}
			return false;
		}

		public static bool IsInRange(
		  GameDatabase db,
		  int playerid,
		  Vector3 loc,
		  Dictionary<FleetInfo, List<ShipInfo>> fleetShips,
		  List<StarSystemInfo> colonySystems)
		{
			if (StarMap.AlwaysInRange)
				return true;
			foreach (StarSystemInfo colonySystem in colonySystems)
			{
				float stratSensorRange = db.GetSystemStratSensorRange(colonySystem.ID, playerid);
				if ((double)(colonySystem.Origin - loc).Length <= (double)stratSensorRange)
					return true;
			}
			float stratSensorRange1 = db.AssetDatabase.DefaultStratSensorRange;
			foreach (KeyValuePair<FleetInfo, List<ShipInfo>> fleetShip in fleetShips)
			{
				float num = GameSession.GetFleetSensorRange(db.AssetDatabase, db, fleetShip.Key, fleetShip.Value);
				if ((double)num == 0.0 && fleetShip.Value.Count > 0)
					num = db.AssetDatabase.DefaultStratSensorRange;
				if ((double)(db.GetFleetLocation(fleetShip.Key.ID, false).Coords - loc).Length <= (double)num)
					return true;
			}
			return false;
		}

		private class PlayerSystemPair
		{
			public int PlayerID;
			public int SystemID;
		}
	}
}
