// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.ShipBuilder
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal class ShipBuilder : IDisposable
	{
		private bool _loading = true;
		public const string CommandSectionIdentifier = "command";
		public const string MissionSectionIdentifier = "mission";
		public const string EngineSectionIdentifier = "engine";
		public const string CommandSectionNodeName = "Command";
		public const string EngineSectionNodeName = "Engine";
		private readonly App _game;
		private ShipSectionAsset[] _sections;
		private Ship[] _ship;
		private List<Ship> _attachedShips;
		private int _curShip;
		private int _loadShip;
		private bool _ready;

		public bool Loading
		{
			get
			{
				return this._loading;
			}
		}

		public IEnumerable<ShipSectionAsset> Sections
		{
			get
			{
				return (IEnumerable<ShipSectionAsset>)this._sections;
			}
		}

		public Ship Ship
		{
			get
			{
				if (!this._ready)
					return this._ship[this._loadShip];
				return this._ship[this._curShip];
			}
		}

		public ShipBuilder(App game)
		{
			this._game = game;
			this._curShip = 0;
			this._loadShip = 1;
			this._ship = new Ship[2];
			this._attachedShips = new List<Ship>();
		}

		public void Clear()
		{
			if (this._ship[this._loadShip] == null)
				return;
			foreach (Ship attachedShip in this._attachedShips)
				attachedShip.Dispose();
			this._attachedShips.Clear();
			this._ship[this._loadShip].Dispose();
			this._ship[this._loadShip] = (Ship)null;
			this._ready = false;
		}

		public void ForceSyncRiders()
		{
			foreach (IGameObject attachedShip in this._attachedShips)
				attachedShip.PostSetProp("ForceInstantSync");
		}

		public void New(
		  Player player,
		  IEnumerable<ShipSectionAsset> sections,
		  IEnumerable<LogicalTurretHousing> turretHousings,
		  IEnumerable<LogicalWeapon> weapons,
		  IEnumerable<LogicalWeapon> preferredWeapons,
		  IEnumerable<WeaponAssignment> assignedWeapons,
		  IEnumerable<LogicalModule> modules,
		  IEnumerable<LogicalModule> preferredModules,
		  IEnumerable<ModuleAssignment> assignedModules,
		  IEnumerable<LogicalPsionic> psionics,
		  DesignSectionInfo[] techs,
		  Faction faction,
		  string shipName,
		  string priorityWeapon)
		{
			this.Clear();
			this._sections = sections.ToArray<ShipSectionAsset>();
			CreateShipParams createShipParams = new CreateShipParams();
			createShipParams.player = player;
			createShipParams.sections = sections;
			createShipParams.turretHousings = turretHousings;
			createShipParams.weapons = weapons;
			createShipParams.preferredWeapons = preferredWeapons;
			createShipParams.assignedWeapons = assignedWeapons;
			createShipParams.modules = modules;
			createShipParams.preferredModules = preferredModules;
			createShipParams.addPsionics = false;
			createShipParams.defenceBoatIsActive = true;
			createShipParams.priorityWeapon = priorityWeapon;
			createShipParams.assignedModules = assignedModules;
			createShipParams.psionics = psionics;
			createShipParams.faction = faction;
			createShipParams.shipName = shipName;
			foreach (DesignSectionInfo tech1 in techs)
			{
				DesignSectionInfo dsi = tech1;
				ShipSectionAsset shipSectionAsset = dsi.ShipSectionAsset ?? this._game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == dsi.FilePath));
				foreach (int tech2 in dsi.Techs)
					createShipParams.assignedTechs[(int)shipSectionAsset.Type].Techs.Add(tech2);
			}
			createShipParams.isKillable = false;
			createShipParams.enableAI = false;
			this._ship[this._loadShip] = Ship.CreateShip(this._game, createShipParams);
			this.PostNewShip(player);
			this._ready = false;
			this._loading = true;
		}

		public void New(
		  Player player,
		  DesignInfo design,
		  string shipName,
		  int serialNumber,
		  bool autoAddDrawable = true)
		{
			this.Clear();
			this._ship[this._loadShip] = Ship.CreateShip(this._game, Matrix.Identity, design, shipName, serialNumber, 0, 0, player, 0, -1, -1, autoAddDrawable, true, false, (IEnumerable<Player>)null);
			this.PostNewShip(player);
			this._ready = false;
			this._loading = true;
		}

		private void PostNewShip(Player player)
		{
			foreach (IGameObject weaponBank in this._ship[this._loadShip].WeaponBanks)
				weaponBank.PostSetProp("OnlyFireOnClick", true);
			foreach (BattleRiderMount battleRiderMount in this._ship[this._loadShip].BattleRiderMounts)
			{
				if (battleRiderMount.DesignID != 0)
				{
					Ship ship = Ship.CreateShip(this._game.Game, Matrix.Identity, new ShipInfo()
					{
						DesignID = battleRiderMount.DesignID,
						FleetID = 0,
						ParentID = this._ship[this._loadShip].DatabaseID,
						SerialNumber = 0,
						ShipName = string.Empty,
						RiderIndex = this._attachedShips.Count
					}, this._ship[this._loadShip].ObjectID, 0, player.ObjectID, false, (IEnumerable<Player>)null);
					ship.PostSetProp("SetKillable", false);
					this._attachedShips.Add(ship);
				}
			}
		}

		public void Update()
		{
			if (this._ready || this._ship[this._loadShip] == null || (!this.AllAttachedShipsLoaded() || this._ship[this._loadShip].ObjectStatus == GameObjectStatus.Pending) || this._ship[this._loadShip].Active)
				return;
			this._ready = true;
			if (this._ship[this._curShip] != null)
				this._ship[this._curShip].Active = false;
			this._ship[this._loadShip].Active = true;
			foreach (Ship attachedShip in this._attachedShips)
				attachedShip.Active = true;
			int curShip = this._curShip;
			this._curShip = this._loadShip;
			this._loadShip = curShip;
			this._loading = false;
		}

		private bool AllAttachedShipsLoaded()
		{
			bool flag = true;
			foreach (GameObject attachedShip in this._attachedShips)
			{
				if (attachedShip.ObjectStatus != GameObjectStatus.Ready)
					flag = false;
			}
			return flag;
		}

		public void Dispose()
		{
			foreach (Ship attachedShip in this._attachedShips)
				attachedShip.Dispose();
			this._attachedShips.Clear();
			if (this._ship[this._loadShip] != null)
			{
				this._ship[this._loadShip].Dispose();
				this._ship[this._loadShip] = (Ship)null;
			}
			if (this._ship[this._curShip] == null)
				return;
			this._ship[this._curShip].Dispose();
			this._ship[this._curShip] = (Ship)null;
		}
	}
}
