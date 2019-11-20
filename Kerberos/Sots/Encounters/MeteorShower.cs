// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Encounters.MeteorShower
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Encounters
{
	internal class MeteorShower
	{
		private static Random MSRandom = new Random();
		private static string[] MeteorDesignFiles = new string[12]
		{
	  "Small_01.section",
	  "Small_02.section",
	  "Small_03.section",
	  "Small_04.section",
	  "Small_05.section",
	  "Medium_01.section",
	  "Medium_02.section",
	  "Large_01.section",
	  "Large_02.section",
	  "Large_03.section",
	  "Large_04.section",
	  "Large_05.section"
		};
		public static bool ForceEncounter = false;
		private int PlayerId = -1;
		private const string FactionName = "asteroids";
		private const string PlayerName = "Meteors";
		private const string PlayerAvatar = "\\base\\factions\\asteroids\\avatars\\Asteroids_Avatar.tga";
		private const string FleetName = "Meteors";
		private int[] _meteorDesignIds;

		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}

		public int[] MeteorDesignIds
		{
			get
			{
				return this._meteorDesignIds;
			}
		}

		public static MeteorShower InitializeEncounter(
		  GameDatabase gamedb,
		  AssetDatabase assetdb)
		{
			if (assetdb.GetFaction("asteroids") == null)
				return (MeteorShower)null;
			MeteorShower meteorShower = new MeteorShower();
			meteorShower.PlayerId = gamedb.InsertPlayer("Meteors", "asteroids", new int?(), assetdb.RandomEncounterPrimaryColor, new Vector3(0.0f), "", "\\base\\factions\\asteroids\\avatars\\Asteroids_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			int length = ((IEnumerable<string>)MeteorShower.MeteorDesignFiles).Count<string>();
			meteorShower._meteorDesignIds = new int[length];
			for (int index = 0; index < length; ++index)
			{
				DesignInfo design = new DesignInfo(meteorShower.PlayerId, "Huge Meteor " + (object)index, new string[1]
				{
		  string.Format("factions\\{0}\\sections\\{1}", (object) "asteroids", (object) MeteorShower.MeteorDesignFiles[index])
				});
				meteorShower._meteorDesignIds[index] = gamedb.InsertDesignByDesignInfo(design);
			}
			return meteorShower;
		}

		public static MeteorShower ResumeEncounter(GameDatabase gamedb)
		{
			if (gamedb.AssetDatabase.GetFaction("asteroids") == null)
				return (MeteorShower)null;
			MeteorShower meteorShower = new MeteorShower();
			PlayerInfo playerInfo = gamedb.GetPlayerInfos().ToList<PlayerInfo>().FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return x.Name.Contains("Meteors");
			   return false;
		   }));
			meteorShower.PlayerId = playerInfo.ID;
			List<DesignInfo> list = gamedb.GetDesignInfosForPlayer(meteorShower.PlayerId).ToList<DesignInfo>();
			int length = ((IEnumerable<string>)MeteorShower.MeteorDesignFiles).Count<string>();
			meteorShower._meteorDesignIds = new int[length];
			for (int i = 0; i < length; ++i)
			{
				DesignInfo designInfo = list.FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.DesignSections[0].FilePath.EndsWith(MeteorShower.MeteorDesignFiles[i])));
				if (designInfo == null)
				{
					DesignInfo design = new DesignInfo(meteorShower.PlayerId, "Huge Meteor " + (object)i, new string[1]
					{
			string.Format("factions\\{0}\\sections\\{1}", (object) "asteroids", (object) MeteorShower.MeteorDesignFiles[i])
					});
					meteorShower._meteorDesignIds[i] = gamedb.InsertDesignByDesignInfo(design);
				}
				else
					meteorShower._meteorDesignIds[i] = designInfo.ID;
			}
			return meteorShower;
		}

		public void UpdateTurn(GameSession game)
		{
			foreach (FleetInfo fleetInfo in game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>())
				game.GameDatabase.RemoveFleet(fleetInfo.ID);
		}

		public void ExecuteEncounter(
		  GameSession game,
		  int targetSystem,
		  float intensity = 1f,
		  bool multiPlanets = false)
		{
			Random safeRandom = App.GetSafeRandom();
			int minValue = (int)((double)game.AssetDatabase.GlobalMeteorShowerData.MinMeteors * (double)intensity);
			int maxValue = (int)((double)game.AssetDatabase.GlobalMeteorShowerData.MaxMeteors * (double)intensity);
			int num = safeRandom.Next(minValue, maxValue);
			int fleetID = game.GameDatabase.InsertFleet(this.PlayerId, 0, targetSystem, targetSystem, "Meteors", FleetType.FL_NORMAL);
			List<int> intList = new List<int>();
			for (int index = 0; index < num; ++index)
				intList.Add(game.GameDatabase.InsertShip(fleetID, safeRandom.Choose<int>((IList<int>)this.MeteorDesignIds), null, (ShipParams)0, new int?(), 0));
			List<ColonyInfo> list = game.GameDatabase.GetColonyInfosForSystem(targetSystem).ToList<ColonyInfo>();
			foreach (int shipID in intList)
			{
				int planetId = 0;
				if (multiPlanets && list.Count > 0)
					planetId = safeRandom.Choose<ColonyInfo>((IList<ColonyInfo>)list).OrbitalObjectID;
				game.GameDatabase.UpdateShipSystemPosition(shipID, new Matrix?(MeteorShower.GetSpawnTransform(game.App, targetSystem, planetId)));
			}
		}

		public void AddEncounter(GameSession game, PlayerInfo targetPlayer, int? targetSystem = null)
		{
			Random safeRandom = App.GetSafeRandom();
			List<int> list = game.GameDatabase.GetPlayerColonySystemIDs(targetPlayer.ID).ToList<int>();
			if (!targetSystem.HasValue && list.Count <= 0)
				return;
			if (!targetSystem.HasValue)
			{
				List<int> noAsteroidBelt = list.Where<int>((Func<int, bool>)(x => !game.GameDatabase.GetStarSystemOrbitalObjectInfos(x).Any<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(y => game.GameDatabase.GetAsteroidBeltInfo(y.ID) != null)))).ToList<int>();
				list.RemoveAll((Predicate<int>)(x => noAsteroidBelt.Contains(x)));
				targetSystem = list.Count <= 0 ? new int?(safeRandom.Choose<int>((IList<int>)noAsteroidBelt)) : new int?(safeRandom.Choose<int>((IList<int>)list));
			}
			game.GameDatabase.InsertIncomingRandom(targetPlayer.ID, targetSystem.Value, RandomEncounter.RE_ASTEROID_SHOWER, game.GameDatabase.GetTurnCount() + 1);
			if (!game.DetectedIncomingRandom(targetPlayer.ID, targetSystem.Value))
				return;
			game.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_INCOMING_ASTEROID_SHOWER,
				EventMessage = TurnEventMessage.EM_INCOMING_ASTEROID_SHOWER,
				PlayerID = targetPlayer.ID,
				SystemID = targetSystem.Value,
				TurnNumber = game.GameDatabase.GetTurnCount()
			});
		}

		public static Matrix GetBaseEnemyFleetTrans(App app, int systemId)
		{
			float num = 0.0f;
			Matrix matrix = Matrix.Identity;
			foreach (OrbitalObjectInfo orbitalObjectInfo in app.GameDatabase.GetStarSystemOrbitalObjectInfos(systemId))
			{
				if (app.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID) != null && app.GameDatabase.GetColonyInfoForPlanet(orbitalObjectInfo.ID) != null)
				{
					Matrix orbitalTransform = app.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID);
					float lengthSquared = orbitalTransform.Position.LengthSquared;
					if ((double)lengthSquared > (double)num)
					{
						num = lengthSquared;
						matrix = orbitalTransform;
					}
				}
			}
			if ((double)num <= 0.0)
				return Matrix.Identity;
			return matrix;
		}

		public static Matrix GetSpawnTransform(App app, int systemId, int planetId = 0)
		{
			float num1 = 0.0f;
			float num2 = 5000f;
			Matrix? nullable = new Matrix?();
			if (planetId != 0)
			{
				PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(planetId);
				ColonyInfo colonyInfoForPlanet = app.GameDatabase.GetColonyInfoForPlanet(planetId);
				if (planetInfo != null && colonyInfoForPlanet != null)
				{
					num2 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size) + 15000f;
					nullable = new Matrix?(app.GameDatabase.GetOrbitalTransform(planetId));
				}
			}
			if (!nullable.HasValue)
			{
				foreach (OrbitalObjectInfo orbitalObjectInfo in app.GameDatabase.GetStarSystemOrbitalObjectInfos(systemId))
				{
					PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID);
					if (planetInfo != null && app.GameDatabase.GetColonyInfoForPlanet(orbitalObjectInfo.ID) != null)
					{
						Matrix orbitalTransform = app.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID);
						float lengthSquared = orbitalTransform.Position.LengthSquared;
						if ((double)lengthSquared > (double)num1)
						{
							num1 = lengthSquared;
							nullable = new Matrix?(orbitalTransform);
							num2 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size) + 15000f;
						}
					}
				}
			}
			if (!nullable.HasValue)
				return Matrix.Identity;
			Vector3 position = nullable.Value.Position;
			double num3 = (double)position.Normalize();
			Matrix world = Matrix.CreateWorld(nullable.Value.Position, position, Vector3.UnitY);
			Vector3 vector3 = Matrix.PolarDeviation(MeteorShower.MSRandom, 80f).Forward * num2;
			vector3.Y = Math.Max(Math.Min(vector3.Y, 300f), -300f);
			float num4 = MeteorShower.MSRandom.NextInclusive(-1500f, 1500f);
			double num5 = (double)vector3.Normalize();
			Vector3 forward = -vector3;
			return Matrix.CreateWorld(vector3 * (num2 + num4), forward, Vector3.UnitY) * world;
		}
	}
}
