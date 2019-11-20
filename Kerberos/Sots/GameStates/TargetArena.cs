// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.TargetArena
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal class TargetArena : IDisposable
	{
		private RealShipClasses _currentShipClass = RealShipClasses.NumShipClasses;
		private string _modelFile = "props\\models\\Target_Engine.scene";
		private Ship[] _targets = new Ship[6];
		private float _goalDist = 400f;
		private readonly Vector3[] TargetDirs = new Vector3[6]
		{
	  new Vector3(0.0f, 1f, 0.0f),
	  new Vector3(0.0f, -1f, 0.0f),
	  new Vector3(1f, 0.0f, 0.0f),
	  new Vector3(-1f, 0.0f, 0.0f),
	  new Vector3(0.0f, 0.0f, 1f),
	  new Vector3(0.0f, 0.0f, -1f)
		};
		private App _game;
		private GameObjectSet _objects;
		private GameObjectSet _targetObjects;
		private bool _activated;
		private bool _ready;
		private bool _targetsLoaded;
		private Player _targetPlayer;
		private ShipSectionAsset _section;
		private WeaponTestWeaponLauncher _launcher;
		private string _faction;

		private Matrix[] GetTargetTransforms(float goalDist)
		{
			return ((IEnumerable<Vector3>)this.TargetDirs).Select<Vector3, Matrix>((Func<Vector3, Matrix>)(x =>
		  {
			  Vector3 vector3 = -x;
			  float num = Vector3.Dot(vector3, Vector3.UnitY);
			  Vector3 up = (double)num > 0.990000009536743 ? -Vector3.UnitZ : ((double)num < -0.990000009536743 ? Vector3.UnitZ : Vector3.UnitY);
			  return Matrix.CreateWorld(x * goalDist, vector3, up);
		  })).ToArray<Matrix>();
		}

		private Ship AddTarget(Vector3 pos, Vector3 rot)
		{
			Ship ship = Ship.CreateShip(this._game, new CreateShipParams()
			{
				player = this._targetPlayer,
				sections = (IEnumerable<ShipSectionAsset>)new ShipSectionAsset[1]
			  {
		  this._section
			  },
				turretHousings = this._game.AssetDatabase.TurretHousings,
				weapons = this._game.AssetDatabase.Weapons,
				psionics = this._game.AssetDatabase.Psionics,
				faction = this._game.AssetDatabase.Factions.First<Faction>((Func<Faction, bool>)(x => this._faction == x.Name)),
				isKillable = false,
				enableAI = false
			});
			ship.Position = pos;
			ship.Rotation = rot;
			this._targetObjects.Add((IGameObject)ship);
			return ship;
		}

		public TargetArena(App game, string faction)
		{
			this._game = game;
			this._objects = new GameObjectSet(game);
			this._targetObjects = new GameObjectSet(game);
			this._faction = faction;
			this._targetPlayer = new Player(game, (GameSession)null, new PlayerInfo()
			{
				AvatarAssetPath = string.Empty,
				BadgeAssetPath = string.Empty,
				PrimaryColor = Vector3.One,
				SecondaryColor = Vector3.One
			}, Player.ClientTypes.AI);
			this._objects.Add((IGameObject)this._targetPlayer);
			Faction faction1 = game.AssetDatabase.Factions.FirstOrDefault<Faction>((Func<Faction, bool>)(x => x.Name == faction)) ?? game.AssetDatabase.Factions.First<Faction>();
			LogicalWeapon weapon = game.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.WeaponName == "Mis_Missile"));
			LogicalWeapon weapon1 = game.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.WeaponName == weapon.SubWeapon));
			WeaponModelPaths weaponModelPaths1 = LogicalWeapon.GetWeaponModelPaths(weapon, faction1);
			WeaponModelPaths weaponModelPaths2 = LogicalWeapon.GetWeaponModelPaths(weapon1, faction1);
			weapon.AddGameObjectReference();
			this._launcher = game.AddObject<WeaponTestWeaponLauncher>(new List<object>()
	  {
		(object) this._targetPlayer.ObjectID,
		(object) (weapon.GameObject != null ? weapon.GameObject.ObjectID : 0),
		(object) weaponModelPaths1.ModelPath,
		(object) weaponModelPaths2.ModelPath
	  }.ToArray());
			this._objects.Add((IGameObject)this._launcher);
			this.ResetTargets();
		}

		public void ResetTargets()
		{
			this._targetObjects.Clear(true);
			this._targetsLoaded = false;
			this._section = new ShipSectionAsset()
			{
				Banks = new LogicalBank[0],
				Modules = new LogicalModuleMount[0],
				ExcludeSections = new string[0],
				Class = ShipClass.BattleRider,
				Faction = this._faction,
				FileName = string.Empty,
				Structure = 100,
				Maneuvering = new ShipManeuveringInfo()
				{
					LinearAccel = 200f,
					LinearSpeed = 400f,
					RotationSpeed = 300f,
					RotAccel = new Vector3(100f, 100f, 100f)
				},
				ManeuveringType = "Fast",
				Mass = 15000f,
				ModelName = this._modelFile,
				DestroyedModelName = this._modelFile,
				DamageEffect = new LogicalEffect(),
				DeathEffect = new LogicalEffect(),
				ReactorFailureDeathEffect = new LogicalEffect(),
				ReactorCriticalDeathEffect = new LogicalEffect(),
				AbsorbedDeathEffect = new LogicalEffect(),
				Mounts = new LogicalMount[0],
				PsionicAbilities = new SectionEnumerations.PsionicAbility[0],
				Type = ShipSectionType.Mission
			};
			Matrix[] targetTransforms = this.GetTargetTransforms(this._goalDist);
			for (int index = 0; index < targetTransforms.Length; ++index)
				this._targets[index] = this.AddTarget(targetTransforms[index].Position, targetTransforms[index].EulerAngles);
		}

		public void PositionsChanged()
		{
			Matrix[] targetTransforms = this.GetTargetTransforms(this._goalDist);
			for (int index = 0; index < targetTransforms.Length; ++index)
				this._targets[index].Maneuvering.PostAddGoal(targetTransforms[index].Position, targetTransforms[index].Forward);
		}

		public int WeaponLauncherID
		{
			get
			{
				if (this._launcher == null)
					return 0;
				return this._launcher.ObjectID;
			}
		}

		public void SetShipClass(RealShipClasses value)
		{
			if (this._currentShipClass == value)
				return;
			switch (value)
			{
				case RealShipClasses.Cruiser:
				case RealShipClasses.BattleCruiser:
					this._goalDist = 750f;
					this._modelFile = "props\\models\\CR_Target.scene";
					break;
				case RealShipClasses.Dreadnought:
				case RealShipClasses.BattleShip:
					this._goalDist = 1500f;
					this._modelFile = "props\\models\\DN_Target.scene";
					break;
				case RealShipClasses.Leviathan:
					this._goalDist = 2000f;
					this._modelFile = "props\\models\\LV_Target.scene";
					break;
				case RealShipClasses.BattleRider:
				case RealShipClasses.Drone:
				case RealShipClasses.BoardingPod:
				case RealShipClasses.EscapePod:
				case RealShipClasses.AssaultShuttle:
				case RealShipClasses.Biomissile:
					this._goalDist = 300f;
					this._modelFile = "props\\models\\BR_Target.scene";
					break;
				case RealShipClasses.Station:
					this._goalDist = 2500f;
					this._modelFile = "props\\models\\LV_Target.scene";
					break;
			}
			this._currentShipClass = value;
			this.ResetTargets();
			this.PositionsChanged();
		}

		public void Activate()
		{
			this._activated = true;
		}

		public void Update()
		{
			if (this._activated && !this._ready && this._objects.IsReady())
			{
				this._ready = true;
				this._objects.Activate();
			}
			if (!this._activated || this._targetsLoaded || !this._targetObjects.IsReady())
				return;
			this.PositionsChanged();
			this._targetObjects.Activate();
			foreach (Ship target in this._targets)
				target.Maneuvering.PostSetProp("CanAvoid", false);
			this._targetsLoaded = true;
		}

		public void ResetTargetPositions()
		{
			foreach (Ship target in this._targets)
			{
				target.Maneuvering.PostSetProp("ResetPosition");
				target.PostSetProp("ClearDamageVisuals");
			}
		}

		public void LaunchWeapon(IGameObject target, int numLaunches)
		{
			if (this._launcher == null)
				return;
			int num = target != null ? target.ObjectID : 0;
			this._launcher.PostSetProp("Fire", (object)numLaunches, (object)num);
		}

		public void Dispose()
		{
			this._objects.Dispose();
			this._targetObjects.Dispose();
		}
	}
}
