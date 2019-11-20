// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Encounters.SuperNova
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Encounters
{
	internal class SuperNova
	{
		public static SuperNova InitializeEncounter()
		{
			return new SuperNova();
		}

		public static SuperNova ResumeEncounter()
		{
			return new SuperNova();
		}

		public void UpdateTurn(GameSession game, int id)
		{
			SuperNovaInfo superNovaInfo = game.GameDatabase.GetSuperNovaInfo(id);
			if (superNovaInfo == null)
			{
				game.GameDatabase.RemoveEncounter(id);
			}
			else
			{
				StarSystemInfo starSystemInfo1 = game.GameDatabase.GetStarSystemInfo(superNovaInfo.SystemId);
				Vector3 starSystemOrigin = game.GameDatabase.GetStarSystemOrigin(superNovaInfo.SystemId);
				List<StarSystemInfo> list1 = game.GameDatabase.GetSystemsInRange(starSystemOrigin, game.AssetDatabase.GlobalSuperNovaData.BlastRadius).ToList<StarSystemInfo>();
				list1.Insert(0, starSystemInfo1);
				float inRangeMinHazard = game.AssetDatabase.GlobalSuperNovaData.SystemInRangeMinHazard;
				float inRangeMaxHazard = game.AssetDatabase.GlobalSuperNovaData.SystemInRangeMaxHazard;
				int rangeBioReduction = game.AssetDatabase.GlobalSuperNovaData.SystemInRangeBioReduction;
				Dictionary<int, int> dictionary1 = new Dictionary<int, int>();
				foreach (StarSystemInfo starSystemInfo2 in list1)
				{
					foreach (ColonyInfo colonyInfo in game.GameDatabase.GetColonyInfosForSystem(starSystemInfo2.ID).ToList<ColonyInfo>())
					{
						if (!dictionary1.ContainsKey(colonyInfo.PlayerID))
							dictionary1.Add(colonyInfo.PlayerID, 0);
						Dictionary<int, int> dictionary2;
						int playerId;
						(dictionary2 = dictionary1)[playerId = colonyInfo.PlayerID] = dictionary2[playerId] + 1;
					}
				}
				List<int> intList1 = new List<int>();
				foreach (StarSystemInfo starSystemInfo2 in list1)
				{
					List<ColonyInfo> list2 = game.GameDatabase.GetColonyInfosForSystem(starSystemInfo2.ID).ToList<ColonyInfo>();
					if (superNovaInfo.TurnsRemaining > 0)
					{
						foreach (ColonyInfo colonyInfo in list2)
						{
							if (!intList1.Contains(colonyInfo.PlayerID))
							{
								game.GameDatabase.InsertTurnEvent(new TurnEvent()
								{
									EventType = TurnEventType.EV_SUPER_NOVA_TURN,
									EventMessage = TurnEventMessage.EM_SUPER_NOVA_TURN,
									PlayerID = colonyInfo.PlayerID,
									SystemID = superNovaInfo.SystemId,
									ArrivalTurns = superNovaInfo.TurnsRemaining,
									TurnNumber = game.GameDatabase.GetTurnCount(),
									Param1 = starSystemInfo1.Name,
									NumShips = dictionary1[colonyInfo.PlayerID],
									ShowsDialog = true
								});
								intList1.Add(colonyInfo.PlayerID);
								Player playerObject = game.GetPlayerObject(colonyInfo.PlayerID);
								if (playerObject != null && playerObject.IsStandardPlayer)
									game.InsertNewRadiationShieldResearchProject(colonyInfo.PlayerID);
							}
						}
					}
					else
					{
						foreach (ColonyInfo colonyInfo in list2)
						{
							if (!intList1.Contains(colonyInfo.PlayerID))
							{
								game.GameDatabase.InsertTurnEvent(new TurnEvent()
								{
									EventType = TurnEventType.EV_SUPER_NOVA_DESTROYED_SYSTEM,
									EventMessage = TurnEventMessage.EM_SUPER_NOVA_DESTROYED_SYSTEM,
									PlayerID = colonyInfo.PlayerID,
									SystemID = superNovaInfo.SystemId,
									TurnNumber = game.GameDatabase.GetTurnCount(),
									Param1 = starSystemInfo1.Name,
									NumShips = dictionary1[colonyInfo.PlayerID],
									ShowsDialog = true
								});
								intList1.Add(colonyInfo.PlayerID);
							}
						}
						if (starSystemInfo2.ID != superNovaInfo.SystemId)
						{
							float length = (starSystemOrigin - game.GameDatabase.GetStarSystemOrigin(starSystemInfo2.ID)).Length;
							List<int> intList2 = new List<int>();
							foreach (ColonyInfo colonyInfo in list2)
							{
								if (!intList2.Contains(colonyInfo.PlayerID))
								{
									if (game.GameDatabase.GetPlayerInfo(colonyInfo.PlayerID).isStandardPlayer)
										GameSession.ApplyMoralEvent(game.App, MoralEvent.ME_SUPER_NOVA_RADIATION, colonyInfo.PlayerID, new int?(), new int?(), new int?(starSystemInfo2.ID));
									intList2.Add(colonyInfo.PlayerID);
								}
								if (!game.GameDatabase.GetHasPlayerStudiedSpecialProject(colonyInfo.PlayerID, SpecialProjectType.RadiationShielding))
								{
									foreach (ColonyFactionInfo faction in colonyInfo.Factions)
										game.GameDatabase.RemoveCivilianPopulation(faction.OrbitalObjectID, faction.FactionID);
									game.GameDatabase.RemoveColonyOnPlanet(colonyInfo.OrbitalObjectID);
									PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(colonyInfo.OrbitalObjectID);
									planetInfo.Biosphere = rangeBioReduction;
									planetInfo.Suitability += (float)((App.GetSafeRandom().CoinToss(50) ? -1.0 : 1.0) * (((double)inRangeMaxHazard - (double)inRangeMinHazard) * (double)Math.Min(Math.Max((float)(1.0 - (double)length / (double)game.AssetDatabase.GlobalSuperNovaData.BlastRadius), 1f), 0.0f) + (double)inRangeMinHazard));
									game.GameDatabase.UpdatePlanet(planetInfo);
								}
							}
						}
					}
				}
				if (superNovaInfo.TurnsRemaining > 0)
				{
					--superNovaInfo.TurnsRemaining;
					game.GameDatabase.UpdateSuperNovaInfo(superNovaInfo);
				}
				else
				{
					game.GameDatabase.RemoveEncounter(id);
					game.GameDatabase.DestroyStarSystem(game, superNovaInfo.SystemId);
					if (!(game.App.CurrentState is StarMapState))
						return;
					((StarMapState)game.App.CurrentState).ClearSelectedObject();
					((StarMapState)game.App.CurrentState).RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_ALL);
				}
			}
		}

		public void ExecuteInstance(
		  GameSession game,
		  GameDatabase gamedb,
		  AssetDatabase assetdb,
		  int systemid)
		{
			gamedb.InsertSuperNovaInfo(new SuperNovaInfo()
			{
				SystemId = systemid,
				TurnsRemaining = App.GetSafeRandom().NextInclusive(assetdb.GlobalSuperNovaData.MinExplodeTurns, assetdb.GlobalSuperNovaData.MaxExplodeTurns)
			});
			List<int> list = game.GameDatabase.GetStandardPlayerIDs().ToList<int>();
			List<int> intList = new List<int>();
			Vector3 starSystemOrigin = game.GameDatabase.GetStarSystemOrigin(systemid);
			foreach (StarSystemInfo starSystemInfo in game.GameDatabase.GetSystemsInRange(starSystemOrigin, game.AssetDatabase.GlobalSuperNovaData.BlastRadius).ToList<StarSystemInfo>())
			{
				if (starSystemInfo.ID != systemid)
				{
					foreach (ColonyInfo colonyInfo in game.GameDatabase.GetColonyInfosForSystem(starSystemInfo.ID).ToList<ColonyInfo>())
					{
						if (!intList.Contains(colonyInfo.PlayerID) && list.Contains(colonyInfo.PlayerID))
						{
							game.InsertNewRadiationShieldResearchProject(colonyInfo.PlayerID);
							intList.Add(colonyInfo.PlayerID);
						}
					}
				}
			}
		}

		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, int targetSystem)
		{
			gamedb.InsertIncomingGM(targetSystem, EasterEgg.GM_SUPER_NOVA, gamedb.GetTurnCount() + 1);
			foreach (PlayerInfo playerInfo in gamedb.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				if (gamedb.GetStratModifier<int>(StratModifiers.GrandMenaceWarningTime, playerInfo.ID) > 0)
					gamedb.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_INCOMING_SUPER_NOVA,
						EventMessage = TurnEventMessage.EM_INCOMING_SUPER_NOVA,
						PlayerID = playerInfo.ID,
						SystemID = targetSystem,
						TurnNumber = gamedb.GetTurnCount()
					});
			}
		}

		public static void AddSuperNovas(GameSession game, GameDatabase gamedb, AssetDatabase assetdb)
		{
			if (!gamedb.HasEndOfFleshExpansion() || game.ScriptModules.SuperNova == null || gamedb.GetTurnCount() < assetdb.GlobalSuperNovaData.MinTurns)
				return;
			string nameValue1 = game.GameDatabase.GetNameValue("GMCount");
			if (string.IsNullOrEmpty(nameValue1))
			{
				game.GameDatabase.InsertNameValuePair("GMCount", "0");
				nameValue1 = game.GameDatabase.GetNameValue("GMCount");
			}
			int nameValue2 = game.GameDatabase.GetNameValue<int>("GSGrandMenaceCount");
			int num1 = int.Parse(nameValue1);
			if (num1 >= nameValue2)
				return;
			Random safeRandom = App.GetSafeRandom();
			if (!safeRandom.CoinToss(assetdb.GlobalSuperNovaData.Chance))
				return;
			List<StarSystemInfo> list1 = gamedb.GetStarSystemInfos().ToList<StarSystemInfo>();
			List<SuperNovaInfo> list2 = gamedb.GetSuperNovaInfos().ToList<SuperNovaInfo>();
			List<int> intList = new List<int>();
			foreach (StarSystemInfo starSystemInfo in list1)
			{
				StarSystemInfo ssi = starSystemInfo;
				StellarClass stellarClass = new StellarClass(ssi.StellarClass);
				if ((stellarClass.Type == StellarType.O || stellarClass.Type == StellarType.B) && !list2.Any<SuperNovaInfo>((Func<SuperNovaInfo, bool>)(x => x.SystemId == ssi.ID)))
					intList.Add(ssi.ID);
			}
			if (intList.Count <= 0)
				return;
			game.ScriptModules.SuperNova.AddInstance(gamedb, assetdb, safeRandom.Choose<int>((IList<int>)intList));
			int num2;
			game.GameDatabase.UpdateNameValuePair("GMCount", (num2 = num1 + 1).ToString());
		}

		public static bool IsPlayerSystemsInSuperNovaEffectRanges(
		  GameDatabase gamedb,
		  int playerId,
		  int systemId)
		{
			StarSystemInfo starSystemInfo = gamedb.GetStarSystemInfo(systemId);
			if (starSystemInfo == (StarSystemInfo)null)
				return false;
			List<SuperNovaInfo> list = gamedb.GetSuperNovaInfos().ToList<SuperNovaInfo>();
			if (list.Count == 0)
				return false;
			bool flag = false;
			foreach (ColonyInfo colonyInfo in gamedb.GetColonyInfosForSystem(systemId).ToList<ColonyInfo>())
			{
				if (colonyInfo.PlayerID == playerId)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
				return false;
			float num = gamedb.AssetDatabase.GlobalSuperNovaData.BlastRadius * gamedb.AssetDatabase.GlobalSuperNovaData.BlastRadius;
			foreach (SuperNovaInfo superNovaInfo in list)
			{
				if ((double)(gamedb.GetStarSystemOrigin(superNovaInfo.SystemId) - starSystemInfo.Origin).LengthSquared <= (double)num)
					return true;
			}
			return false;
		}
	}
}
