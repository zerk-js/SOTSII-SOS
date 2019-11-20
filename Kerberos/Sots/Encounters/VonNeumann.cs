// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Encounters.VonNeumann
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Encounters
{
	internal class VonNeumann
	{
		private static int MaxVonNeumanns = 1;
		public static Dictionary<VonNeumann.VonNeumannShipDesigns, VonNeumann.VonNeumannDesignInfo> StaticShipDesigns = new Dictionary<VonNeumann.VonNeumannShipDesigns, VonNeumann.VonNeumannDesignInfo>()
	{
	  {
		VonNeumann.VonNeumannShipDesigns.BerserkerMothership,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_berserker_mothership.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.BoardingPod,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_boarding_pod.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.CollectorMothership,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_collector_mothership.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.CollectorProbe,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_collector_probe.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.DiscAbsorber,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_disc_absorber.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.DiscCloaker,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_disc_cloaker.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.DiscDisintegrator,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_disc_disintegrator.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.DiscEmitter,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_disc_emitter.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.DiscEMPulser,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_disc_EMPulser.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.DiscImpactor,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_disc_impactor.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.DiscOpressor,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_disc_opressor.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.DiscPossessor,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_disc_possessor.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.DiscScreamer,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_disc_screamer.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.DiscShielder,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_disc_shielder.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.NeoBerserker,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_neo_berserker.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.PlanetKiller,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_planet_killer.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.Pyramid,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_pyramid.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.SeekerMothership,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_seeker_mothership.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.SeekerProbe,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_seeker_probe.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.Moon,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_moon.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.UnderConstruction1,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_underconstruction_v1.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.UnderConstruction2,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_underconstruction_v2.section"
		}
	  },
	  {
		VonNeumann.VonNeumannShipDesigns.UnderConstruction3,
		new VonNeumann.VonNeumannDesignInfo()
		{
		  AssetName = "vnm_underconstruction_v3.section"
		}
	  }
	};
		private int PlayerId = -1;
		private int HomeWorldSystemId = -1;
		private int HomeWorldPlanetId = -1;
		private const string FactionName = "vonneumann";
		private const string PlayerName = "Von Neumann";
		private const string PlayerAvatar = "\\base\\factions\\vonneumann\\avatars\\Vonneumann_Avatar.tga";
		private const string CollectorFleetName = "Von Neumann Collector";
		private const string SeekerFleetName = "Von Neumann Seeker";
		private const string BerserkerFleetName = "Von Neumann Berserker";
		private const string NeoBerserkerFleetName = "Von Neumann NeoBerserker";
		private const string SystemKillerFleetName = "Von Neumann System Killer";
		public const float VPriDiffToChangeTarget = 500f;
		public const float VPriDefencePlatformShift = -1500f;
		public const float VPriPlanetShift = 20f;
		public const float VChildIntegrateTime = 8f;
		private int NumInstances;
		private List<KeyValuePair<StarSystemInfo, Vector3>> _outlyingStars;
		public bool ForceVonNeumannAttack;
		public bool ForceVonNeumannAttackCycle;

		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}

		public int HomeWorldSystemID
		{
			get
			{
				return this.HomeWorldSystemId;
			}
		}

		public int HomeWorldPlanetID
		{
			get
			{
				return this.HomeWorldPlanetId;
			}
		}

		private VonNeumann()
		{
			this.PlayerId = -1;
			this.NumInstances = 0;
		}

		public static VonNeumann InitializeEncounter(
		  GameDatabase gamedb,
		  AssetDatabase assetdb)
		{
			VonNeumann vonNeumann = new VonNeumann();
			vonNeumann.PlayerId = gamedb.InsertPlayer("Von Neumann", "vonneumann", new int?(), assetdb.RandomEncounterPrimaryColor, new Vector3(0.0f), "", "\\base\\factions\\vonneumann\\avatars\\Vonneumann_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			foreach (KeyValuePair<VonNeumann.VonNeumannShipDesigns, VonNeumann.VonNeumannDesignInfo> keyValuePair in VonNeumann.StaticShipDesigns.ToList<KeyValuePair<VonNeumann.VonNeumannShipDesigns, VonNeumann.VonNeumannDesignInfo>>())
			{
				DesignInfo design = new DesignInfo(vonNeumann.PlayerId, keyValuePair.Key.ToString(), new string[1]
				{
		  string.Format("factions\\{0}\\sections\\{1}", (object) "vonneumann", (object) keyValuePair.Value.AssetName)
				});
				DesignLab.SummarizeDesign(assetdb, gamedb, design);
				VonNeumann.StaticShipDesigns[keyValuePair.Key] = new VonNeumann.VonNeumannDesignInfo()
				{
					DesignId = gamedb.InsertDesignByDesignInfo(design),
					AssetName = keyValuePair.Value.AssetName
				};
			}
			vonNeumann._outlyingStars = EncounterTools.GetOutlyingStars(gamedb);
			return vonNeumann;
		}

		public static VonNeumann ResumeEncounter(GameDatabase gamedb)
		{
			VonNeumann vonNeumann = new VonNeumann();
			PlayerInfo playerInfo = gamedb.GetPlayerInfos().ToList<PlayerInfo>().FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return x.Name.Contains("Von Neumann");
			   return false;
		   }));
			vonNeumann.PlayerId = playerInfo.ID;
			List<DesignInfo> list = gamedb.GetDesignInfosForPlayer(vonNeumann.PlayerId).ToList<DesignInfo>();
			foreach (KeyValuePair<VonNeumann.VonNeumannShipDesigns, VonNeumann.VonNeumannDesignInfo> keyValuePair in new Dictionary<VonNeumann.VonNeumannShipDesigns, VonNeumann.VonNeumannDesignInfo>((IDictionary<VonNeumann.VonNeumannShipDesigns, VonNeumann.VonNeumannDesignInfo>)VonNeumann.StaticShipDesigns))
			{
				KeyValuePair<VonNeumann.VonNeumannShipDesigns, VonNeumann.VonNeumannDesignInfo> kvp = keyValuePair;
				DesignInfo designInfo = list.FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.DesignSections[0].FilePath.EndsWith(kvp.Value.AssetName)));
				if (designInfo != null)
					VonNeumann.StaticShipDesigns[kvp.Key].DesignId = designInfo.ID;
			}
			FleetInfo fleetInfo = gamedb.GetFleetInfosByPlayerID(vonNeumann.PlayerID, FleetType.FL_NORMAL).ToList<FleetInfo>().FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(x => x.Name == "Von Neumann NeoBerserker"));
			if (fleetInfo != null)
			{
				vonNeumann.HomeWorldSystemId = fleetInfo.SystemID;
				PlanetInfo planetInfo = gamedb.GetPlanetInfosOrbitingStar(fleetInfo.SystemID).ToList<PlanetInfo>().FirstOrDefault<PlanetInfo>();
				if (planetInfo != null)
					vonNeumann.HomeWorldPlanetId = planetInfo.ID;
			}
			return vonNeumann;
		}

		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, NamesPool namesPool)
		{
			if (this.NumInstances >= VonNeumann.MaxVonNeumanns)
				return;
			int val1 = this._outlyingStars.Count<KeyValuePair<StarSystemInfo, Vector3>>();
			if (val1 == 0)
				return;
			Random safeRandom = App.GetSafeRandom();
			int val2 = 5;
			float num1 = 5f;
			KeyValuePair<StarSystemInfo, Vector3> outlyingStar = this._outlyingStars[safeRandom.Next(Math.Min(val1, val2))];
			this._outlyingStars.Remove(outlyingStar);
			Vector3 origin = outlyingStar.Key.Origin + Vector3.Normalize(outlyingStar.Value) * num1;
			App.Log.Trace(string.Format("Found von neumann homeworld target - Picked System = {0}   Target Coords = {1}", (object)outlyingStar.Key.Name, (object)origin), "game");
			StellarClass stellarClass = StarHelper.ChooseStellarClass(safeRandom);
			this.HomeWorldSystemId = gamedb.InsertStarSystem(new int?(), namesPool.GetSystemName(), new int?(), stellarClass.ToString(), origin, false, true, new int?());
			gamedb.GetStarSystemInfo(this.HomeWorldSystemId);
			int num2 = 5;
			float starOrbitStep = StarSystemVars.Instance.StarOrbitStep;
			float num3 = StarHelper.CalcRadius(stellarClass.Size) + (float)num2 * ((float)num2 * 0.1f * starOrbitStep);
			float x = Ellipse.CalcSemiMinorAxis(num3, 0.0f);
			OrbitalPath path = new OrbitalPath();
			path.Scale = new Vector2(x, num3);
			path.InitialAngle = 0.0f;
			this.HomeWorldPlanetId = gamedb.InsertPlanet(new int?(), this.HomeWorldSystemId, path, "VonNeumonia", "normal", new int?(), 0.0f, 0, 0, 5f);
			PlanetInfo planetInfo = gamedb.GetPlanetInfo(this.HomeWorldPlanetId);
			path = new OrbitalPath();
			path.Scale = new Vector2(15f, 15f);
			path.Rotation = new Vector3(0.0f, 0.0f, 0.0f);
			path.DeltaAngle = 10f;
			path.InitialAngle = 10f;
			VonNeumannInfo vi = new VonNeumannInfo()
			{
				SystemId = this.HomeWorldSystemId,
				OrbitalId = this.HomeWorldPlanetId,
				Resources = assetdb.GlobalVonNeumannData.StartingResources,
				ConstructionProgress = 0
			};
			float radius = StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
			vi.FleetId = new int?(gamedb.InsertFleet(this.PlayerId, 0, vi.SystemId, vi.SystemId, "Von Neumann NeoBerserker", FleetType.FL_NORMAL));
			float num4 = radius + 2000f;
			float num5 = 1000f;
			Matrix matrix1 = gamedb.GetOrbitalTransform(this.HomeWorldPlanetId);
			matrix1 = Matrix.CreateWorld(matrix1.Position, Vector3.Normalize(matrix1.Position), Vector3.UnitY);
			Matrix matrix2 = matrix1;
			matrix2.Position = matrix2.Position + matrix2.Forward * num4 - matrix2.Right * num5;
			for (int index = 0; index < 3; ++index)
			{
				int shipID = gamedb.InsertShip(vi.FleetId.Value, VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.NeoBerserker].DesignId, null, (ShipParams)0, new int?(), 0);
				gamedb.UpdateShipSystemPosition(shipID, new Matrix?(matrix2));
				matrix2.Position += matrix2.Right * num5;
			}
			Random random = new Random();
			float radians1 = (float)((random.CoinToss(0.5) ? -1.0 : 1.0) * 0.785398185253143);
			float radians2 = random.NextInclusive(0.0f, 6.283185f);
			Matrix rotationY1 = Matrix.CreateRotationY(radians1);
			Matrix rotationY2 = Matrix.CreateRotationY(radians2);
			Matrix world = Matrix.CreateWorld(rotationY1.Forward * (matrix1.Position.Length * 0.75f), rotationY2.Forward, Vector3.UnitY);
			VonNeumann.VonNeumannShipDesigns index1 = (VonNeumann.VonNeumannShipDesigns)random.NextInclusive(21, 23);
			int shipID1 = gamedb.InsertShip(vi.FleetId.Value, VonNeumann.StaticShipDesigns[index1].DesignId, null, (ShipParams)0, new int?(), 0);
			gamedb.UpdateShipSystemPosition(shipID1, new Matrix?(world));
			world.Position -= world.Right * 1000f;
			int shipID2 = gamedb.InsertShip(vi.FleetId.Value, VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.Moon].DesignId, null, (ShipParams)0, new int?(), 0);
			gamedb.UpdateShipSystemPosition(shipID2, new Matrix?(world));
			world = Matrix.CreateWorld(world.Position + world.Right * 1000f * 2f, -world.Forward, Vector3.UnitY);
			int shipID3 = gamedb.InsertShip(vi.FleetId.Value, VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.Moon].DesignId, null, (ShipParams)0, new int?(), 0);
			gamedb.UpdateShipSystemPosition(shipID3, new Matrix?(world));
			gamedb.InsertVonNeumannInfo(vi);
			++this.NumInstances;
		}

		public void UpdateTurn(GameSession game, int id)
		{
			if (game.GameDatabase.GetTurnCount() < game.AssetDatabase.RandomEncMinTurns && !this.ForceVonNeumannAttack && !this.ForceVonNeumannAttackCycle)
				return;
			VonNeumannInfo vonNeumannInfo = game.GameDatabase.GetVonNeumannInfo(id);
			bool flag = vonNeumannInfo == null || this.HomeWorldSystemId <= 0;
			List<FleetInfo> list = game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerID, FleetType.FL_NORMAL).ToList<FleetInfo>();
			if (flag || list.FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(x => x.Name == "Von Neumann NeoBerserker")) == null)
			{
				foreach (FleetInfo fleetInfo in list)
					game.GameDatabase.RemoveFleet(fleetInfo.ID);
				game.GameDatabase.RemoveEncounter(id);
			}
			else
			{
				vonNeumannInfo.Resources += vonNeumannInfo.ResourcesCollectedLastTurn;
				vonNeumannInfo.ResourcesCollectedLastTurn = 0;
				if (vonNeumannInfo.ProjectDesignId.HasValue)
					this.BuildProject(game, ref vonNeumannInfo);
				this.ProcessTargets(game, ref vonNeumannInfo);
				this.SendCollector(game, ref vonNeumannInfo, this.ForceVonNeumannAttack);
				game.GameDatabase.UpdateVonNeumannInfo(vonNeumannInfo);
			}
		}

		public static void HandleMomRetreated(GameSession game, int id, int ru)
		{
			VonNeumannInfo vonNeumannInfo = game.GameDatabase.GetVonNeumannInfo(id);
			if (vonNeumannInfo == null)
				return;
			vonNeumannInfo.Resources += ru;
			game.GameDatabase.UpdateVonNeumannInfo(vonNeumannInfo);
		}

		public bool IsHomeWorldFleet(FleetInfo fi)
		{
			return fi.Name == "Von Neumann NeoBerserker";
		}

		public bool CanSpawnFleetAtHomeWorld(FleetInfo fi)
		{
			return fi.Name == "Von Neumann NeoBerserker" || fi.Name == "Von Neumann System Killer";
		}

		public Matrix? GetVNFleetSpawnMatrixAtHomeWorld(
		  GameDatabase gamedb,
		  FleetInfo fi,
		  int fleetIndex)
		{
			if (!this.CanSpawnFleetAtHomeWorld(fi))
				return new Matrix?();
			Matrix orbitalTransform = gamedb.GetOrbitalTransform(this.HomeWorldPlanetId);
			Matrix world = Matrix.CreateWorld(orbitalTransform.Position, -Vector3.Normalize(orbitalTransform.Position), Vector3.UnitY);
			if (fi.Name == "Von Neumann System Killer")
			{
				world.Position += -world.Right * 30000f;
				return new Matrix?(world);
			}
			world.Position += -world.Forward * (float)(3000.0 + 1000.0 * (double)fleetIndex);
			return new Matrix?(world);
		}

		private static FleetInfo GetCollectorFleetInfo(List<FleetInfo> fleets)
		{
			foreach (FleetInfo fleet in fleets)
			{
				if (fleet.Name == "Von Neumann Collector")
					return fleet;
			}
			return (FleetInfo)null;
		}

		private static FleetInfo GetSeekerFleetInfo(List<FleetInfo> fleets)
		{
			foreach (FleetInfo fleet in fleets)
			{
				if (fleet.Name == "Von Neumann Seeker")
					return fleet;
			}
			return (FleetInfo)null;
		}

		private static FleetInfo GetBerserkerFleetInfo(List<FleetInfo> fleets)
		{
			foreach (FleetInfo fleet in fleets)
			{
				if (fleet.Name == "Von Neumann Berserker")
					return fleet;
			}
			return (FleetInfo)null;
		}

		private static FleetInfo GetSystemKillerFleetInfo(List<FleetInfo> fleets)
		{
			foreach (FleetInfo fleet in fleets)
			{
				if (fleet.Name == "Von Neumann System Killer")
					return fleet;
			}
			return (FleetInfo)null;
		}

		private void ProcessTargets(GameSession game, ref VonNeumannInfo vi)
		{
			List<FleetInfo> list = game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			FleetInfo seekerFleetInfo = VonNeumann.GetSeekerFleetInfo(list);
			FleetInfo berserkerFleetInfo = VonNeumann.GetBerserkerFleetInfo(list);
			FleetInfo systemKillerFleetInfo = VonNeumann.GetSystemKillerFleetInfo(list);
			int lastTargetSystem = vi.LastTargetSystem;
			VonNeumannTargetInfo neumannTargetInfo1 = vi.TargetInfos.FirstOrDefault<VonNeumannTargetInfo>((Func<VonNeumannTargetInfo, bool>)(x => x.SystemId == lastTargetSystem));
			if (neumannTargetInfo1 != null)
			{
				FleetInfo fleetInfo = (FleetInfo)null;
				switch (neumannTargetInfo1.ThreatLevel)
				{
					case 1:
						fleetInfo = seekerFleetInfo;
						break;
					case 2:
						fleetInfo = berserkerFleetInfo;
						break;
					case 3:
						fleetInfo = systemKillerFleetInfo;
						break;
				}
				if (fleetInfo == null)
				{
					if (neumannTargetInfo1.ThreatLevel < 3)
						++neumannTargetInfo1.ThreatLevel;
				}
				else
				{
					vi.TargetInfos.Remove(neumannTargetInfo1);
					game.GameDatabase.RemoveVonNeumannTargetInfo(vi.Id, neumannTargetInfo1.SystemId);
					fleetInfo.SystemID = vi.SystemId;
					game.GameDatabase.UpdateFleetLocation(fleetInfo.ID, fleetInfo.SystemID, new int?());
					vi.FleetId = new int?(fleetInfo.ID);
				}
			}
			vi.LastTargetSystem = 0;
			int turnCount = game.GameDatabase.GetTurnCount();
			if (turnCount <= vi.LastTargetTurn + game.AssetDatabase.GlobalVonNeumannData.TargetCycle && !this.ForceVonNeumannAttackCycle)
				return;
			vi.LastTargetTurn = turnCount;
			VonNeumannTargetInfo neumannTargetInfo2 = vi.TargetInfos.FirstOrDefault<VonNeumannTargetInfo>();
			if (neumannTargetInfo2 == null)
				return;
			switch (neumannTargetInfo2.ThreatLevel)
			{
				case 1:
					if (seekerFleetInfo != null)
					{
						seekerFleetInfo.SystemID = neumannTargetInfo2.SystemId;
						vi.LastTargetSystem = neumannTargetInfo2.SystemId;
						game.GameDatabase.UpdateFleetLocation(seekerFleetInfo.ID, seekerFleetInfo.SystemID, new int?());
						vi.FleetId = new int?(seekerFleetInfo.ID);
						break;
					}
					if (vi.ProjectDesignId.HasValue)
						break;
					vi.ProjectDesignId = new int?(VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.SeekerMothership].DesignId);
					break;
				case 2:
					if (berserkerFleetInfo != null)
					{
						berserkerFleetInfo.SystemID = neumannTargetInfo2.SystemId;
						vi.LastTargetSystem = neumannTargetInfo2.SystemId;
						game.GameDatabase.UpdateFleetLocation(berserkerFleetInfo.ID, berserkerFleetInfo.SystemID, new int?());
						vi.FleetId = new int?(berserkerFleetInfo.ID);
						break;
					}
					if (vi.ProjectDesignId.HasValue)
						break;
					vi.ProjectDesignId = new int?(VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.BerserkerMothership].DesignId);
					break;
				case 3:
					if (systemKillerFleetInfo != null)
					{
						systemKillerFleetInfo.SystemID = neumannTargetInfo2.SystemId;
						vi.LastTargetSystem = neumannTargetInfo2.SystemId;
						game.GameDatabase.UpdateFleetLocation(systemKillerFleetInfo.ID, systemKillerFleetInfo.SystemID, new int?());
						vi.FleetId = new int?(systemKillerFleetInfo.ID);
						break;
					}
					if (vi.ProjectDesignId.HasValue)
						break;
					vi.ProjectDesignId = new int?(VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.PlanetKiller].DesignId);
					break;
			}
		}

		private void BuildProject(GameSession game, ref VonNeumannInfo vi)
		{
			int num = Math.Min(game.AssetDatabase.GlobalVonNeumannData.BuildRate, (int)((double)vi.Resources * (double)game.AssetDatabase.DefaultStratModifiers[StratModifiers.OverharvestModifier]));
			DesignInfo designInfo = game.GameDatabase.GetDesignInfo(vi.ProjectDesignId.Value);
			if (vi.ConstructionProgress + num > designInfo.ProductionCost || this.ForceVonNeumannAttackCycle)
			{
				vi.Resources -= (int)((double)(designInfo.ProductionCost - vi.ConstructionProgress) / (double)game.AssetDatabase.DefaultStratModifiers[StratModifiers.OverharvestModifier]);
				string name = "";
				int? projectDesignId1 = vi.ProjectDesignId;
				int designId1 = VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.SeekerMothership].DesignId;
				if ((projectDesignId1.GetValueOrDefault() != designId1 ? 0 : (projectDesignId1.HasValue ? 1 : 0)) != 0)
				{
					name = "Von Neumann Seeker";
				}
				else
				{
					int? projectDesignId2 = vi.ProjectDesignId;
					int designId2 = VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.BerserkerMothership].DesignId;
					if ((projectDesignId2.GetValueOrDefault() != designId2 ? 0 : (projectDesignId2.HasValue ? 1 : 0)) != 0)
					{
						name = "Von Neumann Berserker";
					}
					else
					{
						int? projectDesignId3 = vi.ProjectDesignId;
						int designId3 = VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.PlanetKiller].DesignId;
						if ((projectDesignId3.GetValueOrDefault() != designId3 ? 0 : (projectDesignId3.HasValue ? 1 : 0)) != 0)
							name = "Von Neumann System Killer";
					}
				}
				int fleetID = game.GameDatabase.InsertFleet(this.PlayerId, 0, vi.SystemId, vi.SystemId, name, FleetType.FL_NORMAL);
				game.GameDatabase.InsertShip(fleetID, vi.ProjectDesignId.Value, null, (ShipParams)0, new int?(), 0);
				vi.ConstructionProgress = 0;
				vi.ProjectDesignId = new int?();
			}
			else
			{
				vi.Resources -= (int)((double)game.AssetDatabase.GlobalVonNeumannData.BuildRate / (double)game.AssetDatabase.DefaultStratModifiers[StratModifiers.OverharvestModifier]);
				vi.ConstructionProgress += num;
			}
		}

		public void SendCollector(GameSession game, ref VonNeumannInfo vi, bool forceHomeworldAttack = false)
		{
			List<FleetInfo> list = game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			VonNeumannGlobalData globalVonNeumannData = game.AssetDatabase.GlobalVonNeumannData;
			FleetInfo fleetInfo = VonNeumann.GetCollectorFleetInfo(list);
			if (fleetInfo == null)
			{
				DesignInfo designInfo = game.GameDatabase.GetDesignInfo(VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.CollectorMothership].DesignId);
				if ((double)vi.Resources > (double)designInfo.ProductionCost / (double)game.AssetDatabase.DefaultStratModifiers[StratModifiers.OverharvestModifier])
				{
					vi.Resources -= (int)((double)designInfo.ProductionCost / (double)game.AssetDatabase.DefaultStratModifiers[StratModifiers.OverharvestModifier]);
					vi.FleetId = new int?(game.GameDatabase.InsertFleet(this.PlayerId, 0, vi.SystemId, vi.SystemId, "Von Neumann Collector", FleetType.FL_NORMAL));
					game.GameDatabase.InsertShip(vi.FleetId.Value, VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.CollectorMothership].DesignId, null, (ShipParams)0, new int?(), 0);
					fleetInfo = game.GameDatabase.GetFleetInfo(vi.FleetId.Value);
				}
				else
				{
					vi.Resources = 0;
					vi.FleetId = new int?(game.GameDatabase.InsertFleet(this.PlayerId, 0, vi.SystemId, vi.SystemId, "Von Neumann Collector", FleetType.FL_NORMAL));
					game.GameDatabase.InsertShip(vi.FleetId.Value, VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.CollectorMothership].DesignId, null, (ShipParams)0, new int?(), 0);
					fleetInfo = game.GameDatabase.GetFleetInfo(vi.FleetId.Value);
				}
				int sysId = vi.LastCollectionSystem;
				if (!vi.TargetInfos.Any<VonNeumannTargetInfo>((Func<VonNeumannTargetInfo, bool>)(x => x.SystemId == sysId)) && sysId != 0)
					vi.TargetInfos.Add(new VonNeumannTargetInfo()
					{
						SystemId = sysId,
						ThreatLevel = 1
					});
			}
			else if (vi.LastCollectionSystem != 0)
			{
				foreach (int num1 in game.GameDatabase.GetStarSystemOrbitalObjectIDs(fleetInfo.SystemID).ToList<int>())
				{
					PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(num1);
					LargeAsteroidInfo largeAsteroidInfo = game.GameDatabase.GetLargeAsteroidInfo(num1);
					int num2 = globalVonNeumannData.SalvageCapacity - vi.ResourcesCollectedLastTurn;
					if (planetInfo != null)
					{
						if (planetInfo.Resources > num2)
						{
							planetInfo.Resources -= num2;
							vi.ResourcesCollectedLastTurn = globalVonNeumannData.SalvageCapacity;
							break;
						}
						vi.ResourcesCollectedLastTurn += planetInfo.Resources;
						planetInfo.Resources = 0;
						game.GameDatabase.UpdatePlanet(planetInfo);
					}
					else if (largeAsteroidInfo != null)
					{
						if (largeAsteroidInfo.Resources > num2)
						{
							largeAsteroidInfo.Resources -= num2;
							vi.ResourcesCollectedLastTurn = globalVonNeumannData.SalvageCapacity;
							break;
						}
						vi.ResourcesCollectedLastTurn += largeAsteroidInfo.Resources;
						largeAsteroidInfo.Resources = 0;
						game.GameDatabase.UpdateLargeAsteroidInfo(largeAsteroidInfo);
					}
				}
				vi.Resources += vi.ResourcesCollectedLastTurn;
				fleetInfo.SystemID = vi.SystemId;
				game.GameDatabase.UpdateFleetLocation(fleetInfo.ID, fleetInfo.SystemID, new int?());
			}
			vi.LastCollectionSystem = 0;
			int turnCount = game.GameDatabase.GetTurnCount();
			if (fleetInfo == null || turnCount <= vi.LastCollectionTurn + globalVonNeumannData.SalvageCycle && !forceHomeworldAttack && !this.ForceVonNeumannAttackCycle)
				return;
			vi.LastCollectionTurn = turnCount;
			List<int> intList = new List<int>();
			foreach (int num in game.GameDatabase.GetStarSystemIDs().ToList<int>())
			{
				int system = num;
				if (!vi.TargetInfos.Any<VonNeumannTargetInfo>((Func<VonNeumannTargetInfo, bool>)(x => x.SystemId == system)))
				{
					int? systemOwningPlayer = game.GameDatabase.GetSystemOwningPlayer(system);
					if (systemOwningPlayer.HasValue)
					{
						Player playerObject = game.GetPlayerObject(systemOwningPlayer.Value);
						if (playerObject == null || playerObject.IsAI())
							continue;
					}
					intList.Add(system);
				}
			}
			if (intList.Count <= 0)
				return;
			fleetInfo.SystemID = forceHomeworldAttack || this.ForceVonNeumannAttackCycle ? game.GameDatabase.GetOrbitalObjectInfo(game.GameDatabase.GetPlayerInfo(game.LocalPlayer.ID).Homeworld.Value).StarSystemID : intList[App.GetSafeRandom().Next(intList.Count)];
			vi.FleetId = new int?(fleetInfo.ID);
			vi.LastCollectionSystem = fleetInfo.SystemID;
			game.GameDatabase.UpdateFleetLocation(fleetInfo.ID, fleetInfo.SystemID, new int?());
		}

		public void HandleHomeSystemDefeated(App app, FleetInfo fi, List<int> attackingPlayers)
		{
			VonNeumannInfo vonNeumannInfo = app.Game.GameDatabase.GetVonNeumannInfos().FirstOrDefault<VonNeumannInfo>((Func<VonNeumannInfo, bool>)(x =>
		   {
			   int? fleetId = x.FleetId;
			   int id = fi.ID;
			   if (fleetId.GetValueOrDefault() == id)
				   return fleetId.HasValue;
			   return false;
		   }));
			if (vonNeumannInfo == null)
				return;
			foreach (int attackingPlayer in attackingPlayers)
			{
				float stratModifier = app.GameDatabase.GetStratModifier<float>(StratModifiers.ResearchModifier, attackingPlayer);
				app.GameDatabase.SetStratModifier(StratModifiers.ResearchModifier, attackingPlayer, (object)(float)((double)stratModifier + 0.0500000007450581));
				app.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_VN_HW_DEFEATED,
					EventMessage = TurnEventMessage.EM_VN_HW_DEFEATED,
					PlayerID = attackingPlayer,
					TurnNumber = app.GameDatabase.GetTurnCount(),
					ShowsDialog = true
				});
			}
			foreach (FleetInfo fleetInfo in app.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>())
				app.GameDatabase.RemoveFleet(fleetInfo.ID);
			app.GameDatabase.RemoveEncounter(vonNeumannInfo.Id);
		}

		public static Matrix GetBaseEnemyFleetTrans(App app, int systemID)
		{
			return VonNeumann.GetSpawnTransform(app, systemID);
		}

		public static Matrix GetSpawnTransform(App app, int systemId)
		{
			bool flag = false;
			float num1 = 0.0f;
			float num2 = 0.0f;
			OrbitalObjectInfo orbitalObjectInfo1 = (OrbitalObjectInfo)null;
			Vector3 vector3 = Vector3.Zero;
			foreach (OrbitalObjectInfo orbitalObjectInfo2 in app.GameDatabase.GetStarSystemOrbitalObjectInfos(systemId))
			{
				ColonyInfo colonyInfoForPlanet = app.GameDatabase.GetColonyInfoForPlanet(orbitalObjectInfo2.ID);
				if (!flag || colonyInfoForPlanet != null)
				{
					PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(orbitalObjectInfo2.ID);
					float num3 = 1000f;
					if (planetInfo != null)
						num3 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
					Vector3 position = app.GameDatabase.GetOrbitalTransform(orbitalObjectInfo2.ID).Position;
					float num4 = position.Length + num3;
					if ((double)num4 > (double)num1 || !flag && colonyInfoForPlanet != null)
					{
						orbitalObjectInfo1 = orbitalObjectInfo2;
						num1 = num4;
						flag = colonyInfoForPlanet != null;
						vector3 = position;
						num2 = num3 + 7000f;
					}
				}
			}
			if (orbitalObjectInfo1 == null)
				return Matrix.CreateWorld(-Vector3.UnitZ * 50000f, Vector3.UnitZ, Vector3.UnitY);
			Vector3 forward = -vector3;
			double num5 = (double)forward.Normalize();
			return Matrix.CreateWorld(vector3 - forward * num2, forward, Vector3.UnitY);
		}

		public enum VonNeumannShipDesigns
		{
			BerserkerMothership,
			BoardingPod,
			CollectorMothership,
			CollectorProbe,
			Disc,
			DiscAbsorber,
			DiscCloaker,
			DiscDisintegrator,
			DiscEmitter,
			DiscEMPulser,
			DiscImpactor,
			DiscOpressor,
			DiscPossessor,
			DiscScreamer,
			DiscShielder,
			NeoBerserker,
			PlanetKiller,
			Pyramid,
			SeekerMothership,
			SeekerProbe,
			Moon,
			UnderConstruction1,
			UnderConstruction2,
			UnderConstruction3,
		}

		public class VonNeumannDesignInfo
		{
			public int DesignId;
			public string AssetName;
		}
	}
}
