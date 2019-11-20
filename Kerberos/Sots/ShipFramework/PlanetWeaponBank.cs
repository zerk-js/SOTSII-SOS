// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.PlanetWeaponBank
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using System.Collections.Generic;

namespace Kerberos.Sots.ShipFramework
{
	[GameObjectType(InteropGameObjectType.IGOT_PLANETWEAPONBANK)]
	internal class PlanetWeaponBank : WeaponBank
	{
		public LogicalWeapon SubWeapon { get; private set; }

		public string WeaponModel { get; private set; }

		public string SubWeaponModel { get; private set; }

		public float ThinkTime { get; private set; }

		public int NumLaunchers { get; private set; }

		public PlanetWeaponBank(
		  App game,
		  IGameObject owner,
		  LogicalBank bank,
		  Module module,
		  LogicalWeapon weapon,
		  int weaponLevel,
		  LogicalWeapon subWeapon,
		  WeaponEnums.TurretClasses tClass,
		  string model,
		  string subModel,
		  float thinkTime,
		  int numLaunchers)
		  : base(game, owner, bank, module, weapon, weaponLevel, 0, 0, 0, weapon.DefaultWeaponSize, tClass)
		{
			this.SubWeapon = subWeapon;
			this.WeaponModel = model;
			this.SubWeaponModel = subModel;
			this.ThinkTime = thinkTime;
			this.NumLaunchers = numLaunchers;
		}

		public override void AddExistingObject(App game)
		{
			game.AddExistingObject((IGameObject)this, new List<object>()
	  {
		(object) (this.Weapon != null ? this.Weapon.GameObject.ObjectID : 0),
		(object) this.Owner.ObjectID,
		(object) this.WeaponLevel,
		(object) this.TargetFilter,
		(object) this.FireMode,
		(object) (int) this.WeaponSize,
		(object) (this.SubWeapon != null ? this.SubWeapon.GameObject.ObjectID : 0),
		(object) this.WeaponModel,
		(object) this.SubWeaponModel,
		(object) this.ThinkTime,
		(object) this.NumLaunchers
	  }.ToArray());
		}
	}
}
