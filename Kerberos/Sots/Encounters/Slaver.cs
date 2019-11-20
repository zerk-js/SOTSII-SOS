// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Encounters.Slaver
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Encounters
{
	internal class Slaver
	{
		private int PlayerId = -1;
		private const string FactionName = "slavers";
		private const string PlayerName = "Slaver";
		private const string PlayerAvatar = "\\base\\factions\\slavers\\avatars\\Slavers_Avatar.tga";
		private const string FleetName = "Slaver Band";
		private const string wraithAbductorFile = "cr_wraith_abductor.section";
		private const string commandSectionFile = "cr_cmd.section";
		private const string engineSectionFile = "cr_eng_fusion.section";
		private const string scavengerSectionFile = "cr_mis_scavenger.section";
		private const string slaveDiskSectionFile = "br_slavedisk.section";
		public static bool ForceEncounter;
		private int _wraithAbductorDesignId;
		private int _scavengerDesignId;
		private int _slaveDiskDesignId;

		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}

		public int WraithAbductorDesignId
		{
			get
			{
				return this._wraithAbductorDesignId;
			}
		}

		public int ScavengerDesignId
		{
			get
			{
				return this._scavengerDesignId;
			}
		}

		public int SlaveDiskDesignId
		{
			get
			{
				return this._slaveDiskDesignId;
			}
		}

		public void InitDesigns(GameDatabase db)
		{
			List<DesignInfo> list = db.GetDesignInfosForPlayer(this.PlayerId).ToList<DesignInfo>();
			this._slaveDiskDesignId = db.AddOrGetEncounterDesignInfo((IEnumerable<DesignInfo>)list, this.PlayerId, "Slave Disk", "slavers", "br_slavedisk.section");
			this._wraithAbductorDesignId = db.AddOrGetEncounterDesignInfo((IEnumerable<DesignInfo>)list, this.PlayerId, "Slaver Wraith Abductor", "slavers", "cr_wraith_abductor.section");
			this._scavengerDesignId = db.AddOrGetEncounterDesignInfo((IEnumerable<DesignInfo>)list, this.PlayerId, "Slaver Scavenger", "slavers", "cr_mis_scavenger.section", "cr_cmd.section", "cr_eng_fusion.section");
			DesignInfo designInfo1 = db.GetDesignInfo(this._slaveDiskDesignId);
			DesignInfo designInfo2 = db.GetDesignInfo(this._scavengerDesignId);
			designInfo1.Role = ShipRole.SLAVEDISK;
			designInfo2.Role = ShipRole.SCAVENGER;
			db.UpdateDesign(designInfo1);
			db.UpdateDesign(designInfo2);
		}

		public static Slaver InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			Slaver slaver = new Slaver();
			slaver.PlayerId = gamedb.InsertPlayer(nameof(Slaver), "slavers", new int?(), assetdb.RandomEncounterPrimaryColor, new Vector3(0.0f), "", "\\base\\factions\\slavers\\avatars\\Slavers_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			slaver.InitDesigns(gamedb);
			return slaver;
		}

		public static Slaver ResumeEncounter(GameDatabase gamedb)
		{
			Slaver slaver = new Slaver();
			PlayerInfo playerInfo = gamedb.GetPlayerInfos().ToList<PlayerInfo>().FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return x.Name.Contains(nameof(Slaver));
			   return false;
		   }));
			slaver.PlayerId = playerInfo == null ? gamedb.InsertPlayer(nameof(Slaver), "slavers", new int?(), new Vector3(0.0f), new Vector3(0.0f), "", "\\base\\factions\\slavers\\avatars\\Slavers_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal) : playerInfo.ID;
			slaver.InitDesigns(gamedb);
			return slaver;
		}

		public void UpdateTurn(GameSession game)
		{
			foreach (FleetInfo fleetInfo in game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>())
				game.GameDatabase.RemoveFleet(fleetInfo.ID);
		}

		public void ExecuteEncounter(GameSession game, PlayerInfo targetPlayer, int targetSystem)
		{
			Random safeRandom = App.GetSafeRandom();
			int num1 = safeRandom.Next(game.AssetDatabase.GlobalSlaverData.MinAbductors, game.AssetDatabase.GlobalSlaverData.MaxAbductors);
			int num2 = safeRandom.Next(game.AssetDatabase.GlobalSlaverData.MinScavengers, game.AssetDatabase.GlobalSlaverData.MaxScavengers);
			int fleetID = game.GameDatabase.InsertFleet(this.PlayerId, 0, targetSystem, targetSystem, "Slaver Band", FleetType.FL_NORMAL);
			for (int index = 0; index < num1; ++index)
				game.GameDatabase.InsertShip(fleetID, this._wraithAbductorDesignId, null, (ShipParams)0, new int?(), 0);
			DesignInfo designInfo = game.GameDatabase.GetDesignInfo(this._scavengerDesignId);
			for (int index1 = 0; index1 < num2; ++index1)
			{
				int parentID = game.GameDatabase.InsertShip(fleetID, this._scavengerDesignId, null, (ShipParams)0, new int?(), 0);
				if (designInfo != null)
				{
					int index2 = 0;
					foreach (DesignSectionInfo designSection in designInfo.DesignSections)
					{
						foreach (WeaponBankInfo chooseWeapon in DesignLab.ChooseWeapons(game, (IList<LogicalWeapon>)game.AssetDatabase.Weapons.ToList<LogicalWeapon>(), ShipRole.SCAVENGER, WeaponRole.PLANET_ATTACK, designSection.ShipSectionAsset, this.PlayerId, (AITechStyles)null))
						{
							if (chooseWeapon.DesignID.HasValue && chooseWeapon.DesignID.Value != 0)
							{
								int shipID = game.GameDatabase.InsertShip(fleetID, chooseWeapon.DesignID.Value, null, (ShipParams)0, new int?(), 0);
								game.GameDatabase.SetShipParent(shipID, parentID);
								game.GameDatabase.UpdateShipRiderIndex(shipID, index2);
								++index2;
							}
						}
					}
				}
			}
		}

		public void AddEncounter(GameSession game, PlayerInfo targetPlayer)
		{
			Random safeRandom = App.GetSafeRandom();
			List<int> list = game.GameDatabase.GetPlayerColonySystemIDs(targetPlayer.ID).ToList<int>();
			if (list.Count <= 0)
				return;
			int systemid = safeRandom.Choose<int>((IList<int>)list);
			game.GameDatabase.InsertIncomingRandom(targetPlayer.ID, systemid, RandomEncounter.RE_SLAVERS, game.GameDatabase.GetTurnCount() + 1);
			if (!game.DetectedIncomingRandom(targetPlayer.ID, systemid))
				return;
			game.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_INCOMING_SLAVERS,
				EventMessage = TurnEventMessage.EM_INCOMING_SLAVERS,
				PlayerID = targetPlayer.ID,
				SystemID = systemid,
				TurnNumber = game.GameDatabase.GetTurnCount()
			});
		}

		public static Matrix GetBaseEnemyFleetTrans(App app, Kerberos.Sots.GameStates.StarSystem starSystem)
		{
			return Slaver.GetSpawnTransform(app, starSystem);
		}

		public static Matrix GetSpawnTransform(App app, Kerberos.Sots.GameStates.StarSystem starSystem)
		{
			List<CombatZonePositionInfo> list = starSystem.NeighboringSystems.Select<NeighboringSystemInfo, CombatZonePositionInfo>((Func<NeighboringSystemInfo, CombatZonePositionInfo>)(x => starSystem.GetEnteryZoneForOuterSystem(x.SystemID))).ToList<CombatZonePositionInfo>();
			CombatZonePositionInfo zonePositionInfo1 = (CombatZonePositionInfo)null;
			Vector3 forward = new Vector3();
			float num1 = float.MaxValue;
			List<StellarBody> stellarBodyList = new List<StellarBody>();
			foreach (StellarBody stellarBody in starSystem.GetPlanetsInSystem())
			{
				if (stellarBody.Population != 0.0)
				{
					foreach (CombatZonePositionInfo zonePositionInfo2 in list)
					{
						Vector3 vector3 = stellarBody.Parameters.Position - zonePositionInfo2.Center;
						float lengthSquared = vector3.LengthSquared;
						if ((double)lengthSquared < (double)num1)
						{
							num1 = lengthSquared;
							forward = vector3;
							zonePositionInfo1 = zonePositionInfo2;
						}
					}
				}
			}
			if (zonePositionInfo1 == null)
			{
				forward.X = (App.GetSafeRandom().CoinToss(0.5) ? -1f : 1f) * App.GetSafeRandom().NextInclusive(0.0001f, 1f);
				forward.Y = 0.0f;
				forward.Z = (App.GetSafeRandom().CoinToss(0.5) ? -1f : 1f) * App.GetSafeRandom().NextInclusive(0.0001f, 1f);
				double num2 = (double)forward.Normalize();
				float num3 = App.GetSafeRandom().NextInclusive(10000f, starSystem.GetSystemRadius());
				return Matrix.CreateWorld(forward * num3, -forward, Vector3.UnitY);
			}
			double num4 = (double)forward.Normalize();
			return Matrix.CreateWorld(zonePositionInfo1.Center, forward, Vector3.UnitY);
		}
	}
}
